function attemptAssessment() {
  return {
    sidebarOpen: false,
    assessment: { title: "", questions: [] },
    answers: {},
    assessmentId: "",
    editor: null,
    current: 0,
    timer: 1200,
    intervalId: null,
    autoSaveId: null,
    loaded: false,
    submitted: false,
    sessionStart: new Date().toISOString(),
    isSaving: false,
    isMonacoInitializing: false, // The "lock" to prevent re-initialization

    get currentQuestion() {
      return this.assessment.questions[this.current];
    },

    get timerDisplay() {
      const m = Math.floor(this.timer / 60);
      const s = this.timer % 60;
      return `${m}m ${s < 10 ? "0" + s : s}s`;
    },

    async init() {
      // First, ensure the entire page layout is loaded and stable.
      await this.loadLayout();
      this.assessmentId = new URLSearchParams(window.location.search).get("id");
      if (!this.assessmentId) return alert("Invalid access");

      const res = await api.get(`/Assessments/${this.assessmentId}/questions`);
      const data = await res.json();

      if (!res.ok) {
        const code = data?.errorCode;
        let title = "Assessment Unavailable";

        if (code === "ASSESMENT_NOT_STARTED")
          title = "Assessment Has Not Started Yet";
        else if (code === "ASSESMENT_ENDED") title = "Assessment Has Ended";

        Swal.fire({
          icon: "error",
          title: title,
          text: data?.message || "Please check with your instructor.",
          confirmButtonText: "Back to Dashboard",
        }).then(() => {
          window.location.href = "/public/student/student-assessment.html";
        });
        return;
      }

      this.assessment = data.data;
      for (const q of this.assessment.questions) {
        this.answers[q.id] = q.type === "MCQ" ? [] : "";
      }

      const progressRes = await api.get(`/AssessmentProgress/load?assessmentId=${this.assessmentId}`);

      if (progressRes.ok) {
        const progressData = await progressRes.json();
        if (progressData?.data?.answers) {
          for (const ans of progressData.data.answers) {
            if (ans.selectedOptionIds?.length) {
              this.answers[ans.questionId] = ans.selectedOptionIds;
            } else {
              this.answers[ans.questionId] = ans.answerText || "";
            }
          }
        }

        if (progressData.data.elapsedTime) {
          const secondsUsed = Math.floor(
            progressData.data.elapsedTime.totalSeconds || 0
          );
          const totalSeconds = this.assessment.durationInMinutes * 60;
          this.timer = Math.max(0, totalSeconds - secondsUsed);
        }
      }

      this.loaded = true;
      this.initTimer();

      this.autoSaveId = setInterval(() => this.saveProgress(), 300000);

      document.addEventListener("visibilitychange", () => {
        if (document.hidden) this.saveProgress();
      });
    },

    async loadLayout() {
      try {
        await loadComponent("sidebar-placeholder", "/public/components/sidebar-student.html");
        await loadComponent("navbar-placeholder", "/public/components/navbar-student.html");
      } catch (error) {
        console.error("Failed to load layout components:", error);
      }
    },

    async saveProgress() {
      if (this.submitted || this.isSaving) return;
      this.isSaving = true;

      const formatted = Object.entries(this.answers).map(([qid, val]) => ({
        questionId: qid,
        answerText: typeof val === "string" ? val : null,
        selectedOptionIds: Array.isArray(val) ? val : [],
      }));

      try {
        await api.post("/AssessmentProgress/students/progress/save", {
          assessmentId: this.assessmentId,
          answers: formatted,
          currentSessionStart: this.sessionStart,
        });
      } catch (e) {
        console.error("Autosave failed:", e);
      } finally {
        this.isSaving = false;
      }
    },

    async submit() {
      if (this.submitted) return;

      const confirm = await Swal.fire({
        title: "Are you sure?",
        text: "Once submitted, you cannot make changes.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, submit it!",
        cancelButtonText: "Cancel",
      });

      if (!confirm.isConfirmed) return;
      this.submitted = true;
      clearInterval(this.intervalId);
      clearInterval(this.autoSaveId);
      await this.saveProgress();

      const formatted = Object.entries(this.answers)
        .map(([qid, val]) => ({
          questionId: qid,
          submittedAnswer: typeof val === "string" ? val.trim() : "",
          selectedOptionIds: Array.isArray(val) ? val : [],
        }))
        .filter(
          (ans) =>
            ans.selectedOptionIds.length > 0 ||
            (typeof ans.submittedAnswer === "string" &&
              ans.submittedAnswer.trim() !== "")
        );

      try {
        const res = await api.post(`/Assessments/${this.assessmentId}/submit`, { answers: formatted });

        if (res.ok) {
          await Swal.fire({
            icon: "success",
            title: "Submitted!",
            text: "Your assessment has been successfully submitted.",
          });
          window.location.href = "/public/student/student-assessment.html";
        } else throw new Error("Submission failed.");
      } catch (error) {
        console.error(error);
        Swal.fire({
          icon: "error",
          title: "Oops...",
          text: "Something went wrong during submission.",
        });
      }
    },

    initTimer() {
      this.intervalId = setInterval(() => {
        if (this.timer > 0) {
          this.timer--;
        } else {
          clearInterval(this.intervalId);
          this.submit();
        }
      }, 1000);
    },

    initMonaco() {
      const self = this;
      // If the editor exists or we are already in the process of creating it, do nothing.
      if (this.editor || this.isMonacoInitializing) {
        return;
      }
      this.isMonacoInitializing = true; // Set the lock

      if (typeof require === "undefined") {
        console.error("Monaco loader is not ready yet.");
        this.isMonacoInitializing = false; // Release the lock on error
        return;
      }
      require(["vs/editor/editor.main"], function () {
        self.editor = monaco.editor.create(document.getElementById("editor"), {
          value: "",
          language: self.getMonacoLanguage(
            self.currentQuestion?.technologyStack
          ),
          theme: "vs-dark",
          automaticLayout: true,
        });

        self.editor.onDidChangeModelContent(() => {
          const q = self.currentQuestion;
          if (q?.type === "Coding") {
            self.answers[q.id] = self.editor.getValue();
          }
        });

        self.updateEditorContent();
        self.isMonacoInitializing = false; // Release the lock on success
      });
    },

    updateEditorContent() {
      const q = this.currentQuestion;
      if (q?.type === "Coding" && this.editor) {
        const value = this.answers[q.id] || "";
        const lang = this.getMonacoLanguage(q.technologyStack);
        if (this.editor.getValue() !== value) {
          this.editor.setValue(value);
        }
        monaco.editor.setModelLanguage(this.editor.getModel(), lang);
      }
    },

    getMonacoLanguage(stack) {
      if (!stack) return "plaintext";
      switch (stack.toLowerCase()) {
        case "c#":
        case "csharp":
          return "csharp";
        case "javascript":
        case "js":
          return "javascript";
        case "typescript":
        case "ts":
          return "typescript";
        case "python":
          return "python";
        case "java":
          return "java";
        case "cpp":
        case "c++":
          return "cpp";
        case "html":
          return "html";
        case "json":
          return "json";
        default:
          return "plaintext";
      }
    },

    switchTo(i) {
      this.current = i;
      // The x-effect directive on the editor div will handle initialization.
      // We just need to ensure content is updated for subsequent views.
      this.$nextTick(() => this.updateEditorContent());
    },
    next() {
      if (this.current < this.assessment.questions.length - 1)
        this.switchTo(this.current + 1);
    },
    prev() {
      if (this.current > 0) this.switchTo(this.current - 1);
    },

    toggleMCQAnswer(qid, optId) {
      const selected = this.answers[qid] || [];
      this.answers[qid] = selected.includes(optId)
        ? selected.filter((id) => id !== optId)
        : [...selected, optId];
    },
  };
}
