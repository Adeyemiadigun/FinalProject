import { api, loadComponent, logOut } from "../utils.js";

document.addEventListener("alpine:init", () => {
  Alpine.data("attemptAssessment", () => {
    // ===== Private Variables =====
    const editorContentStore = {};
    let remainingSeconds = 0;
    let resizeObserver = null;
    let editorInstance = null;
    let autoSaveInterval = null;
    let lastSavedHash = "";

    function debounce(func, wait) {
      let timeout;
      return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
      };
    }

    function computeAnswersHash(questions) {
      return JSON.stringify(
        questions.map((q) => ({
          id: q.id,
          answer:
            q.type === "Coding"
              ? editorContentStore[q.id] || ""
              : q.answerText || "",
          selected: q.selectedOptionIds || [],
        }))
      );
    }

    // ===== Alpine Component =====
    return {
      assessmentId: null,
      assessment: null,
      questions: [],
      isEditorLoading: false,
      isSaving: false,
      timerText: "",
      timerInterval: null,
      currentQuestionIndex: 0,
      questionNavigator: [],

      get currentQuestion() {
        return this.questions[this.currentQuestionIndex] || null;
      },

      // ===== Lifecycle =====
      async init() {
        // Load Navbar & Sidebar
        await Promise.all([
          loadComponent(
            "sidebar-placeholder",
            "/public/components/sidebar-student.html"
          ),
          loadComponent(
            "navbar-placeholder",
            "/public/components/navbar-student.html"
          ),
        ]);

        this.assessmentId = new URLSearchParams(window.location.search).get(
          "id"
        );
        const token = localStorage.getItem("accessToken");
        if (!this.assessmentId || !token) {
          logOut();
          return;
        }

        // Preload Monaco Editor
        this.preloadMonaco();

        await this.fetchQuestions(token);
        await this.loadProgress(token);
        this.updateNavigator();

        if (this.assessment) {
          this.startTimer(this.assessment.durationInMinutes * 60);
        }

        await this.switchTo(0);

        // Periodic autosave every 30 seconds
        autoSaveInterval = window.setInterval(() => this.saveProgress(), 30000);

        window.addEventListener("beforeunload", () => this.destroy());
        document.addEventListener("visibilitychange", () =>
          this.handleVisibilityChange()
        );
      },

      async preloadMonaco() {
        if (!window.MonacoEnvironment) {
          window.MonacoEnvironment = {
            getWorkerUrl: () =>
              `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                self.MonacoEnvironment = { baseUrl: 'https://unpkg.com/monaco-editor@0.47.0/min/' };
                importScripts('https://unpkg.com/monaco-editor@0.47.0/min/vs/base/worker/workerMain.js');
              `)}`,
          };
        }

        return new Promise((resolve) => {
          require.config({
            paths: { vs: "https://unpkg.com/monaco-editor@0.47.0/min/vs" },
          });
          require(["vs/editor/editor.main"], resolve);
        });
      },

      async fetchQuestions() {
        try {
          const res = await api.get(
            `/Assessments/${this.assessmentId}/questions`
          );
          const data = await res.json();
          this.assessment = data.data;
          this.questions = (data.data.questions || []).map((q) => ({
            id: q.id,
            title: q.title,
            type: q.type,
            techStack: q.technologyStack || "javascript",
            options: q.options || [],
            testCases: q.testCases || [],
            answerText: "",
            selectedOptionIds: [],
          }));
        } catch (err) {
          console.error("Fetch questions error:", err);
          Swal.fire("Error", "Failed to load assessment", "error");
        }
      },

      async loadProgress() {
        try {
          const res = await api.get(
            `/AssessmentProgress/load?assessmentId=${this.assessmentId}`
          );
          const progress = await res.json();
          if (!progress.data?.answers) return;

          for (const ans of progress.data.answers) {
            const q = this.questions.find((q) => q.id === ans.questionId);
            if (!q) continue;
            q.answerText = ans.answerText || "";
            q.selectedOptionIds = ans.selectedOptionIds || [];
            if (q.type === "Coding")
              editorContentStore[q.id] = ans.answerText || "";
          }

          lastSavedHash = computeAnswersHash(this.questions);
        } catch (err) {
          console.error("Load progress error:", err);
        }
      },

      // ===== UI Interaction =====
      toggleOption(question, optionId) {
        const idx = question.selectedOptionIds.indexOf(optionId);
        idx === -1
          ? question.selectedOptionIds.push(optionId)
          : question.selectedOptionIds.splice(idx, 1);
        this.updateNavigator();
      },

      updateNavigator() {
        this.questionNavigator = this.questions.map((q, i) => ({
          id: q.id,
          index: i,
          isCurrent: i === this.currentQuestionIndex,
          isAnswered: this.isAnswered(q),
        }));
      },

      isAnswered(question) {
        if (!question) return false;
        if (question.type === "Coding")
          return (editorContentStore[question.id] || "").trim().length > 0;
        if (question.type === "Objective")
          return (question.answerText || "").trim().length > 0;
        if (question.type?.toLowerCase() === "mcq")
          return question.selectedOptionIds.length > 0;
        return false;
      },

      async switchTo(index) {
        if (index < 0 || index >= this.questions.length) return;

        if (editorInstance) {
          editorInstance.dispose();
          editorInstance = null;
        }
        if (resizeObserver) {
          resizeObserver.disconnect();
          resizeObserver = null;
        }

        this.currentQuestionIndex = index;
        await this.createEditor();
      },

      async createEditor() {
        await this.$nextTick();
        const container = document.getElementById("editor-container");

        if (
          !this.currentQuestion ||
          this.currentQuestion.type !== "Coding" ||
          !container
        )
          return;

        this.isEditorLoading = true;

        editorInstance = monaco.editor.create(container, {
          value: editorContentStore[this.currentQuestion.id] || "",
          language: "csharp",
          theme: "vs-dark",
          automaticLayout: false,
          minimap: { enabled: false },
          fontSize: 14,
          scrollBeyondLastLine: false,
        });

        const debouncedUpdate = debounce(() => {
          editorContentStore[this.currentQuestion.id] =
            editorInstance.getValue();
          this.updateNavigator();
        }, 300);
        editorInstance.onDidChangeModelContent(debouncedUpdate);

        resizeObserver = new ResizeObserver(() =>
          requestAnimationFrame(() => editorInstance?.layout())
        );
        resizeObserver.observe(container);

        requestAnimationFrame(() => editorInstance?.layout());
        this.isEditorLoading = false;
      },

      // ===== Progress Saving =====
      saveProgress: debounce(async function () {
        const currentHash = computeAnswersHash(this.questions);
        if (currentHash === lastSavedHash) return; // Skip if no changes

        this.isSaving = true;
        const payload = {
          assessmentId: this.assessmentId,
          answers: this.questions.map((q) => ({
            questionId: q.id,
            answerText:
              q.type === "Coding"
                ? editorContentStore[q.id] || ""
                : q.answerText || "",
            selectedOptionIds: q.selectedOptionIds,
          })),
          currentSessionStart: new Date().toISOString(),
        };

        try {
          const res = await api.post(
            "/AssessmentProgress/students/progress/save",
            payload
          );
          if (res.ok) {
            lastSavedHash = currentHash;
            console.log("Progress autosaved:", new Date().toLocaleTimeString());
          }
        } catch (err) {
          console.error("Save progress error:", err);
        } finally {
          this.isSaving = false;
        }
      }, 1000),

      // ===== Submission =====
      async submitAssessment() {
        this.isSaving = true;

        if (editorInstance && this.currentQuestion?.type === "Coding") {
          editorContentStore[this.currentQuestion.id] =
            editorInstance.getValue();
        }

        const filtered = this.questions
          .map((q) => ({
            questionId: q.id,
            submittedAnswer:
              q.type === "Coding"
                ? editorContentStore[q.id] || ""
                : q.answerText || "",
            selectedOptionIds: q.selectedOptionIds,
          }))
          .filter(
            (ans) =>
              ans.submittedAnswer?.trim() || ans.selectedOptionIds?.length
          );

        if (!filtered.length) {
          Swal.fire(
            "Empty",
            "You must attempt at least one question",
            "warning"
          );
          this.isSaving = false;
          return;
        }

        try {
          const res = await api.post(
            `/Assessments/${this.assessmentId}/submit`,
            { answers: filtered }
          );
          if (res.ok) {
            Swal.fire(
              "Submitted",
              "Assessment submitted successfully",
              "success"
            ).then(
              () =>
                (window.location.href =
                  "/public/student/student-dashboard.html")
            );
          }
        } catch (err) {
          console.error("Submission error:", err);
          Swal.fire(
            "Error",
            "An error occurred while submitting the assessment",
            "error"
          );
        } finally {
          this.isSaving = false;
        }
      },

      // ===== Timer & Cleanup =====
      startTimer(durationInSeconds) {
        remainingSeconds = durationInSeconds;
        this.updateTimerText(remainingSeconds);

        this.timerInterval = window.setInterval(() => {
          remainingSeconds--;
          this.updateTimerText(remainingSeconds);
          if (remainingSeconds <= 0) {
            window.clearInterval(this.timerInterval);
            Swal.fire(
              "Time's Up!",
              "Your assessment will be submitted automatically.",
              "warning"
            );
            this.submitAssessment();
          }
        }, 1000);
      },

      updateTimerText(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        this.timerText = `${String(mins).padStart(2, "0")}:${String(
          secs
        ).padStart(2, "0")}`;
      },

      handleVisibilityChange() {
        if (document.hidden) {
          if (this.timerInterval) window.clearInterval(this.timerInterval);
        } else if (remainingSeconds > 0) {
          this.startTimer(remainingSeconds);
        }
      },

      destroy() {
        if (editorInstance) editorInstance.dispose();
        if (this.timerInterval) window.clearInterval(this.timerInterval);
        if (autoSaveInterval) window.clearInterval(autoSaveInterval);
        if (resizeObserver) resizeObserver.disconnect();
        document.removeEventListener(
          "visibilitychange",
          this.handleVisibilityChange
        );
      },
    };
  });
});
