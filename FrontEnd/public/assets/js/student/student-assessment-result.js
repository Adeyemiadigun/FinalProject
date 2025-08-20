import { api, loadComponent, logOut } from "../shared/utils.js";

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
      await loadComponent(
        "sidebar-placeholder",
        "/public/components/sidebar-student.html"
      );
      await loadComponent(
        "navbar-placeholder",
        "/public/components/navbar-student.html"
      );

      const urlParams = new URLSearchParams(window.location.search);
      const assessmentId = urlParams.get("id");

      if (!assessmentId) {
        Swal.fire("Error", "No assessment ID provided.", "error");
        return;
      }

      try {
        const res = await api.get(
          `/Assessments/${assessmentId}/student-answers`
        );
        const json = await res.json();

        if (!json.status) {
          Swal.fire("Error", json.message || "Unable to load result.", "error");
          return;
        }

        const submission = json.data;

        if (
          !submission.submittedAt ||
          new Date(submission.assessmentEndDate) > new Date()
        ) {
          Swal.fire("Info", "This assessment is still ongoing.", "info").then(
            () => {
              window.location.href = "/public/student/dashboard.html";
            }
          );
          return;
        }

        const submittedAnswers = submission.submittedAnswers || [];
        const correctCount = submittedAnswers.filter((q) => q.isCorrect).length;
        const wrongCount = submittedAnswers.length - correctCount;
console.log("Submitted Answers:", submittedAnswers);
        this.result = {
          title: submission.title,
          date: new Date(submission.submittedAt).toLocaleDateString(),
          score: submission.totalScore,
          feedback: submission.feedBack,
          correctCount,
          wrongCount,
          questions: submittedAnswers.map((ans) => ({
            questionId: ans.questionId,
            questionText: ans.questionText,
            questionType: this.mapQuestionType(ans.questionType),
            submittedAnswer: ans.submittedAnswer,
            isCorrect: ans.isCorrect,
            score: ans.score,
            options: ans.options || [],
            selectedOptions: ans.selectedOptions || [],
            correctAnswerText: this.extractCorrectAnswer(ans),
            testCases: ans.testCases || [],
          })),
        };
      } catch (error) {
        console.error("Error fetching result:", error);
        Swal.fire("Error", "Failed to load assessment result.", "error");
      } finally {
        this.resultLoaded = true;
      }
    },

    logOut,

    mapQuestionType(type) {
      switch (type) {
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
      if (ans.questionType === 1) {
        // MCQ
        return (ans.options || [])
          .filter((o) => o.isCorrect)
          .map((o) => o.optionText)
          .join(", ");
      }
      return "";
    },
  };
};
