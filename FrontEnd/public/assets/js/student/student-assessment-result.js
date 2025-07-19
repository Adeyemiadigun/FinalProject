function loadLayout() {
  fetch("/public/components/sidebar-student.html")
    .then((res) => res.text())
    .then(
      (html) =>
        (document.getElementById("sidebar-placeholder").innerHTML = html)
    );

  fetch("/public/components/navbar-student.html")
    .then((res) => res.text())
    .then(
      (html) => (document.getElementById("navbar-placeholder").innerHTML = html)
    );
}

function viewResult() {
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

    async init() {
      const urlParams = new URLSearchParams(window.location.search);
      const assessmentId = urlParams.get("id");
      const token = localStorage.getItem("accessToken");

      if (!assessmentId) return;

      const res = await fetch(
        `https://localhost:7157/api/v1/Assessments/${assessmentId}/submission`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      const json = await res.json();
      if (!json.status) return;

      const submission = json.data;
      const submittedAnswers = submission.submittedAnswers || [];

      const correctCount = submittedAnswers.filter((q) => q.isCorrect).length;
      const wrongCount = submittedAnswers.length - correctCount;

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
          questionType: ans.questionType,
          submittedAnswer: ans.submittedAnswer,
          isCorrect: ans.isCorrect,
          correctAnswerText: this.extractCorrectAnswer(ans),
          testCases: ans.testCases,
        })),
      };
    },
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },

    extractCorrectAnswer(ans) {
      if (ans.questionType === "MCQ" || ans.questionType === "Objective") {
        const correctOptions = ans.options.filter((o) => o.isCorrect);
        return correctOptions.map((o) => o.optionText).join(", ");
      }
      return "";
    },
  };
}
