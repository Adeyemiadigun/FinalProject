import { api, loadComponent, logOut } from "../shared/utils.js";

window.questionsPage = function () {
  return {
    assessmentId: null,
    assessmentTitle: "",
    passingScore: "",
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

      await this.loadAssessment();
      await this.loadQuestions();
    },

    async loadAssessment() {
      try {
        const res = await api.get(`/Assessments/${this.assessmentId}`);
        const data = await res.json();

        this.assessmentTitle = data.data.title;
        this.passingScore = data.data.passingScore;
        this.aiForm.technologyStack = data.data.technologyStack;
      } catch (err) {
        console.error("Failed to load assessment", err);
        // handleResponse in utils already triggers Swal + redirect for 401/403
      }
    },

    async loadQuestions() {
      try {
        const res = await api.get(
          `/Assessments/${this.assessmentId}/questions/answers`
        );
        const data = await res.json();
        this.questions = data?.data || [];
      } catch (err) {
        console.error("Failed to load questions", err);
        this.questions = [];
      }
    },

    getQuestionTypeLabel(type) {
      return { 1: "MCQ", 2: "Objective", 3: "Coding" }[type] || "Unknown";
    },

    async generateAIQuestion() {
      try {
        const res = await api.post("/Ai/generate-text", this.aiForm);
        const result = await res.json();

        if (!result.status) {
          Swal.fire("Error", "Failed to generate question.", "error");
          return;
        }

        this.aiPreviewData = {
          ...result.data,
          questionType: this.mapQuestionType(result.data.questionType),
        };
        this.showAIModal = false;
        this.showAIPreviewModal = true;
      } catch (error) {
        console.error("AI Generation Error", error);
        Swal.fire("Error", "Error generating AI question.", "error");
      }
    },

    mapQuestionType(type) {
      const map = { MCQ: 1, Objective: 2, Coding: 3 };
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
        answer: this.aiPreviewData.answer || {
          answerText: this.aiPreviewData.answerText || "",
        },
      };

      this.showCreateModal = true;
      this.showAIPreviewModal = false;
    },
    GotoAiGenModal()
    {
      this.showAIModal = true;
      this.showCreateModal = false;
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
      console.log("Submitting question:", payload);
      const endpoint = this.showEditModal
        ? `/Questions/${this.questions[this.editIndex].id}`
        : `/Assessments/${this.assessmentId}/questions`;

      const body = this.showEditModal ? payload : [payload];
  


      try {
           let res;
           if (this.showEditModal) {
             res = await api.put(endpoint, body);
           } else {
             res = await api.post(endpoint, body);
           }
        if (res.ok) {
          await this.loadQuestions();
          if (this.showEditModal) this.closeModals();
          if (this.showCreateModal) this.resetForm();
        }
      } catch(error) {
        console.error("Failed to submit question", error);
        Swal.fire("Error", "Failed to submit question", "error");
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

      try {
        await api.delete(`/Questions/${questionId}`);
        this.questions = this.questions.filter((q) => q.id !== questionId);
      } catch {
        Swal.fire("Error", "Failed to delete question", "error");
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

    logOut,
  };
};
