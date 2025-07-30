
async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
function questionsPage() {
  return {
    assessmentId: null,
    assessmentTitle: "",
    passingScore:"",
    questions: [],
    showCreateModal: false,
    showEditModal: false,
    showAIModal: false,
    editIndex: null,
    questionForm: {
      questionText: "",
      questionType: "",
      marks: 1,
      order: 1,
      options: [],
      testCases: [],
      answer: { answerText: "" },
    },
    token: "",
    aiForm: {
      questionType: "",
      technologyStack: "",
      difficulty: "",
      topic: "",
    },
    showAIPreviewModal: false,
    aiPreviewData: {},

    async init() {
      await loadComponent(
        "sidebar-placeholder",
        "../components/instructor-sidebar.html"
      );
      await loadComponent(
        "navbar-placeholder",
        "../components/instructor-nav.html"
      );

      const params = new URLSearchParams(window.location.search);
      this.assessmentId = params.get("assessmentId");
      this.assessmentTitle = `Assessment #${this.assessmentId ?? "N/A"}`;
      this.token = localStorage.getItem("accessToken");
      await this.loadAssessment();
      await this.loadQuestions();
    },
   async loadAssessment() {
  try {
    const res = await fetch(
      `https://localhost:7157/api/v1/Assessments/${this.assessmentId}`,
      {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${this.token}`,
        },
      }
    );

    const data = await res.json();

    if (!res.ok) {
      Swal.fire({
        icon: "error",
        title: data.errorCode || "Error",
        text: data.message || "Failed to load assessment",
      }).then(() => {
        if (res.status === 401 || res.status === 403) {
          // Token issue → redirect to login
          window.location.href = "/public/auth/login.html";
        } else if (res.status === 404) {
          // Assessment not found → redirect to assessments page
          window.location.href = "/public/instructor/assessment-page.html";
        } else {
          // Other errors → fallback to dashboard
          window.location.href = "/public/instructor/dashboard.html";
        }
      });
      return;
    }

    // ✅ Success: Set values
    this.assessmentTitle = data.data.title;
    this.passingScore = data.data.passingScore;
    this.aiForm.technologyStack = data.data.technologyStack;

  } catch (err) {
    console.error("Network or parsing error:", err);
    Swal.fire({
      icon: "error",
      title: "Network Error",
      text: "Failed to load assessment. Please try again.",
    }).then(() => {
      window.location.href = "/public/instructor/dashboard.html";
    });
  }
},

    async loadQuestions() {
      try {
        const res = await fetch(
          `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/questions/answers`,
          {
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${this.token}`,
            },
          }
        );

        const data = await res.json();
        this.questions = data?.data || [];
      } catch (err) {
        console.error("Failed to load questions", err);
        this.questions = [];
      }
    },

    getQuestionTypeLabel(type) {
      return (
        {
          1: "MCQ",
          2: "Objective",
          3: "Coding",
        }[type] || "Unknown"
      );
    },

    async generateAIQuestion() {
      try {
        const res = await fetch(
          "https://localhost:7157/api/v1/Ai/generate-text",
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${this.token}`,
            },
            body: JSON.stringify(this.aiForm),
          }
        );
    
        const result = await res.json();
        if (!res.ok || !result.status) {
          alert("Failed to generate question.");
          return;
        }
    
        this.aiPreviewData = {
          ...result.data,
          questionType: this.mapQuestionType(result.data.questionType),
        };
        this.showAIModal = false;
        this.showAIPreviewModal = true;
        
      console.log(this.aiPreviewData);
      } catch (error) {
        console.error("AI Generation Error", error);
        alert("Error generating AI question.");
      }
    },
    mapQuestionType(type) {
      const map = { "MCQ": 1, "Objective": 2, "Coding": 3 };
      return typeof type === "string" ? map[type] || type : type;
    },
    useAIPreviewQuestion() {
      this.questionForm = {
        questionText: this.aiPreviewData.questionText || "",
        questionType: this.mapQuestionType(this.aiPreviewData.questionType),
        marks: 1,
        order: this.questions.length + 1,
        options: this.aiPreviewData.options || [],
        testCases: this.aiPreviewData.testCases || [],
        answer: this.aiPreviewData.answer || { answerText: this.aiPreviewData.answerText || "" }
      };
    
      this.showCreateModal = true;
      this.showAIPreviewModal = false;
    },
    
    
    async submitQuestion() {
      const payload = {
        questionText: this.questionForm.questionText,
        questionType: this.questionForm.questionType,
        marks: this.questionForm.marks,
        order: this.questionForm.order,
        options: this.questionForm.options,
        testCases: this.questionForm.testCases,
        answer: this.questionForm.answer,
      };

      const url = this.showEditModal
        ? `https://localhost:7157/api/v1/Questions/${
            this.questions[this.editIndex].id
          }`
        : `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/questions`;

      const method = this.showEditModal ? "PUT" : "POST";
      const body = this.showEditModal
        ? JSON.stringify(payload)
        : JSON.stringify([payload]);

      const res = await fetch(url, {
        method,
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${this.token}`,
        },
        body,
      });

      if (res.ok) {
        await this.loadQuestions();
        if (this.showEditModal) this.closeModals();
        if (this.showCreateModal) this.resetForm(); // don't close modal for create
      } else {
        alert("Failed to submit question");
      }
    },

    openEditModal(index) {
      this.editIndex = index;
      this.questionForm = JSON.parse(JSON.stringify(this.questions[index]));
      this.showEditModal = true;
      this.showCreateModal = false;
    },

    async deleteQuestion(questionId) {
      if (!confirm("Are you sure you want to delete this question?")) return;

      const res = await fetch(
        `https://localhost:7157/api/v1/Questions/${questionId}`,
        {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${this.token}`,
          },
        }
      );

      if (res.ok) {
        this.questions = this.questions.filter((q) => q.id !== questionId);
      } else {
        alert("Failed to delete question");
      }
    },

    resetForm() {
      this.questionForm = {
        questionText: "",
        questionType: "",
        marks: 1,
        order: this.questions.length + 1,
        options: [],
        testCases: [],
        answer: { answerText: "" },
      };
    },

    closeModals() {
      this.showCreateModal = false;
      this.showEditModal = false;
      this.resetForm();
    },

    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
  };
}
