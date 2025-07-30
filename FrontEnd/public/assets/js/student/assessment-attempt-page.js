document.addEventListener("alpine:init", () => {
  Alpine.data("attemptAssessment", () => {
    // These are private variables for the component instance, not reactive.
    const editorContentStore = {};
    let remainingSeconds = 0;
    let resizeObserver = null;
    let editorInstance = null; // Non-reactive editor instance

    function debounce(func, wait) {
      let timeout;
      return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
      };
    }

    // This is the public, reactive object for Alpine.
    return {
      assessmentId: null,
      assessment: null,
      questions: [],
      isEditorLoading: false,
      isSaving: false,
      timerText: "",
      timerInterval: null,
      currentQuestionIndex: 0,
      questionNavigator: [], // Memoized navigator

      get currentQuestion() {
        return this.questions[this.currentQuestionIndex] || null;
      },

      async init() {
        // Preload Monaco Editor in parallel with other setup
        this.preloadMonaco();

        // Load components
        await Promise.all([
          this.loadComponent(
            "sidebar-placeholder",
            "/public/components/sidebar-student.html"
          ),
          this.loadComponent(
            "navbar-placeholder",
            "/public/components/navbar-student.html"
          ),
        ]);

        this.assessmentId = new URLSearchParams(window.location.search).get(
          "id"
        );
        const token = localStorage.getItem("accessToken");
        if (!this.assessmentId || !token) {
          localStorage.removeItem("accessToken");
          window.location.href = "/public/auth/login.html";
          return;
        }

        await this.fetchQuestions(token);
        await this.loadProgress(token);
        this.updateNavigator(); // Initial navigator state

        if (this.assessment) {
          this.startTimer(this.assessment.durationInMinutes * 60);
        }

        await this.switchTo(0);

        window.addEventListener("beforeunload", () => this.destroy());
        document.addEventListener("visibilitychange", () =>
          this.handleVisibilityChange()
        );
      },

      async preloadMonaco() {
        if (!window.MonacoEnvironment) {
          window.MonacoEnvironment = {
            getWorkerUrl: function () {
              return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                  self.MonacoEnvironment = { baseUrl: 'https://unpkg.com/monaco-editor@0.47.0/min/' };
                  importScripts('https://unpkg.com/monaco-editor@0.47.0/min/vs/base/worker/workerMain.js');
                `)}`;
            },
          };
        }

        return new Promise((resolve) => {
          require.config({
            paths: { vs: "https://unpkg.com/monaco-editor@0.47.0/min/vs" },
          });
          require(["vs/editor/editor.main"], resolve);
        });
      },

      async fetchQuestions(token) {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 10000);

        try {
          const res = await fetch(
            `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/questions`,
            {
              headers: { Authorization: `Bearer ${token}` },
              signal: controller.signal,
            }
          );
          if (!res.ok) throw new Error("Failed to fetch questions");
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
        } finally {
          clearTimeout(timeoutId);
        }
      },

      async loadProgress(token) {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 10000);
        try {
          const res = await fetch(
            `https://localhost:7157/api/v1/AssessmentProgress/load?assessmentId=${this.assessmentId}`,
            {
              headers: { Authorization: `Bearer ${token}` },
              signal: controller.signal,
            }
          );
          if (!res.ok) return;
          const progress = await res.json();
          if (!progress.data || !progress.data.answers) return;
          console.log(progress.data)
          for (const ans of progress.data.answers) {
            const q = this.questions.find((q) => q.id === ans.questionId);
            if (!q) continue;
            q.answerText = ans.answerText || q.answerText;
            q.selectedOptionIds = ans.selectedOptionIds || [];
            if (q.type === "Coding") {
              editorContentStore[q.id] = ans.answerText || "";
            }
          }
        } catch (err) {
          console.error("Load progress error:", err);
        } finally {
          clearTimeout(timeoutId);
        }
      },

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
        if (question.type === "MCQ" || question.type === "Mcq")
          return question.selectedOptionIds.length > 0;
        return false;
      },

      async switchTo(index) {
        if (index < 0 || index >= this.questions.length) return;

        // Destroy the old editor instance before switching
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
        ) {
          return;
        }

        this.isEditorLoading = true;
        console.log(this.currentQuestion)
        editorInstance = monaco.editor.create(container, {
          value: editorContentStore[this.currentQuestion.id] || "",
          language: "csharp",
          theme: "vs-dark",
          automaticLayout: false, // We manage layout with ResizeObserver
          minimap: { enabled: false },
          fontSize: 14,
          scrollBeyondLastLine: false,
        });

        const debouncedUpdate = debounce(() => {
          if (this.currentQuestion) {
            editorContentStore[this.currentQuestion.id] =
              editorInstance.getValue();
            this.updateNavigator();
          }
        }, 300);
        editorInstance.onDidChangeModelContent(debouncedUpdate);

        resizeObserver = new ResizeObserver(() => {
          requestAnimationFrame(() => editorInstance?.layout());
        });
        resizeObserver.observe(container);

        requestAnimationFrame(() => editorInstance?.layout());
        this.isEditorLoading = false;
      },

      saveProgress: debounce(async function () {
        this.isSaving = true;
        const token = localStorage.getItem("accessToken");

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
          const res = await fetch(
            "https://localhost:7157/api/v1/AssessmentProgress/students/progress/save",
            {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
              },
              body: JSON.stringify(payload),
            }
          );
          if (res.ok) {
            Swal.fire("Saved", "Progress saved successfully", "success");
          } else {
            Swal.fire("Error", "Failed to save progress", "error");
          }
        } catch (err) {
          console.error("Save progress error:", err);
          Swal.fire(
            "Error",
            "An error occurred while saving progress",
            "error"
          );
        } finally {
          this.isSaving = false;
        }
      }, 1000),

      async submitAssessment() {
        this.isSaving = true;
        if (editorInstance && this.currentQuestion?.type === "Coding") {
          editorContentStore[this.currentQuestion.id] = editorInstance.getValue();
        }

        const token = localStorage.getItem("accessToken");
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
              (ans.submittedAnswer && ans.submittedAnswer.trim() !== "") ||
              (ans.selectedOptionIds && ans.selectedOptionIds.length > 0)
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
          const res = await fetch(
            `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/submit`,
            {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
              },
              body: JSON.stringify({ answers: filtered }),
            }
          );

          if (res.ok) {
            Swal.fire(
              "Submitted",
              "Assessment submitted successfully",
              "success"
            ).then(() => {
              window.location.href = "/public/student/student-dashboard.html";
            });
          } else {
            Swal.fire("Error", "Failed to submit assessment", "error");
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

      async loadComponent(id, path) {
        try {
          const res = await fetch(path);
          if (!res.ok) throw new Error(`Component load failed: ${path}`);
          document.getElementById(id).innerHTML = await res.text();
        } catch (err) {
          console.error("Load component error:", err);
        }
      },

      startTimer(durationInSeconds) {
        remainingSeconds = durationInSeconds;
        this.updateTimerText(remainingSeconds);

        this.timerInterval = setInterval(() => {
          remainingSeconds--;
          this.updateTimerText(remainingSeconds);
          if (remainingSeconds <= 0) {
            clearInterval(this.timerInterval);
            Swal.fire(
              "Time's Up!",
              "Your assessment will be submitted automatically.",
              "warning"
            );
            this.submitAssessment();
          }
        }, 1000); // Corrected to 1 second
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
          if (this.timerInterval) clearInterval(this.timerInterval);
        } else {
          if (remainingSeconds > 0) {
            this.startTimer(remainingSeconds);
          }
        }
      },

      destroy() {
        if (editorInstance) {
          editorInstance.dispose();
          editorInstance = null;
        }
        if (this.timerInterval) {
          clearInterval(this.timerInterval);
        }
        if (resizeObserver) {
          resizeObserver.disconnect();
        }
        document.removeEventListener(
          "visibilitychange",
          this.handleVisibilityChange
        );
      },
    };
  });
});
