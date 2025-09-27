// Note: utils.js should be included via <script> tag in HTML
// Access utilities from global scope
const api = window.utils?.api;
const loadComponent = window.utils?.loadComponent;
const logOut = window.utils?.logOut;

window.viewResultPage = function () {
  return {
    result: {
      title: "",
      date: "",
      score: 0,
      feedback: "",
      correctCount: 0,
      wrongCount: 0,
      questions: [],
    },
    resultLoaded: false,

    async init() {
      if (!api || !loadComponent || !logOut) {
        console.error('Required utilities not available');
        Swal.fire("Error", "System initialization failed", "error");
        return;
      }

      try {
        await loadComponent(
          "sidebar-placeholder",
          "/public/components/sidebar-student.html"
        );
        await loadComponent(
          "navbar-placeholder",
          "/public/components/navbar-student.html"
        );
      } catch (error) {
        console.error("Failed to load components:", error);
      }

      const urlParams = new URLSearchParams(window.location.search);
      const assessmentId = urlParams.get("id");

      if (!assessmentId) {
        Swal.fire("Error", "No assessment ID provided.", "error");
        return;
      }

      try {
        const json = await api.get(`/Assessments/${assessmentId}/student-answers`);

        if (!json || !json.status) {
          Swal.fire("Error", json?.message || "Unable to load result.", "error");
          return;
        }

        const submission = json.data;
        if (!submission) {
          Swal.fire("Error", "No submission data available.", "error");
          return;
        }

        // Validate submission data
        if (!submission.submittedAt || !submission.assessmentEndDate) {
          Swal.fire("Info", "This assessment is still ongoing.", "info").then(() => {
            window.location.href = "/public/student/dashboard.html";
          });
          return;
        }

        if (new Date(submission.assessmentEndDate) > new Date()) {
          Swal.fire("Info", "This assessment is still ongoing.", "info").then(() => {
            window.location.href = "/public/student/dashboard.html";
          });
          return;
        }

        const submittedAnswers = submission.submittedAnswers || [];
        const correctCount = submittedAnswers.filter((q) => q.isCorrect).length;
        const wrongCount = Math.max(0, submittedAnswers.length - correctCount);

        // Handle date safely
        let dateString = "Not Available";
        if (submission.submittedAt) {
          try {
            dateString = new Date(submission.submittedAt).toLocaleDateString();
          } catch (dateError) {
            console.warn("Invalid date format:", submission.submittedAt);
          }
        }

        this.result = {
          title: submission.title || "Untitled Assessment",
          date: dateString,
          score: submission.totalScore || 0,
          feedback: submission.feedback || submission.feedBack || "No feedback provided",
          passingPercentage: submission.passingPercentage || 0,
          totalMarks: submission.totalMarks || 0,
          correctCount,
          wrongCount,
          questions: submittedAnswers.map((ans) => ({
            questionId: ans.questionId,
            questionText: ans.questionText || "",
            questionType: this.mapQuestionType(ans.questionType),
            submittedAnswer: ans.submittedAnswer || "",
            isCorrect: ans.isCorrect || false,
            score: ans.score || 0,
            options: ans.options || [],
            selectedOptions: ans.selectedOptions || [],
            correctAnswerText: this.extractCorrectAnswer(ans),
            testCases: ans.testCases || [],
          })),
        };
      } catch (error) {
        console.error("Error fetching result:", error);
        Swal.fire("Error", "Failed to load assessment result. " + (error.message || ""), "error");
      } finally {
        this.resultLoaded = true;
      }
    },

    logOut() {
      if (logOut) {
        logOut();
      } else {
        // Fallback logout
        localStorage.clear();
        window.location.href = "/public/auth/login.html";
      }
    },

    mapQuestionType(type) {
      if (type === null || type === undefined) return "Unknown";
      
      switch (parseInt(type)) {
        case 1:
          return "MCQ";
        case 2:
          return "Objective";
        case 3:
          return "Coding";
        default:
          return "Unknown";
      }
    },

    extractCorrectAnswer(ans) {
      if (!ans || ans.questionType !== 1) return "";
      
      const options = ans.options || [];
      const correctOptions = options.filter((o) => o.isCorrect);
      
      if (correctOptions.length === 0) return "";
      
      return correctOptions
        .map((o) => o.optionText || "")
        .join(", ");
    },
  };
};
