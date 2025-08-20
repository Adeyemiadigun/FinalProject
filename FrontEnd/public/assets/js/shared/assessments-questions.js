import { api, loadComponent, logOut } from "../shared/utils.js";

window.assessmentQuestionsPage = function () {
  return {
    questions: [],
    loading: true,
    assessment: {},
    isLoadingAssessment: true,

    async init() {
      await this.loadSidebarAndNavbar();

      const urlParams = new URLSearchParams(window.location.search);
      const assessmentId = urlParams.get("id");
      if (!assessmentId) {
        Swal.fire({
          icon: "error",
          title: "Missing ID",
          text: "Assessment ID not found in URL.",
        });
        window.location.href = "/public/auth/login.html";
        return;
      }
      this.fetchAssessment(assessmentId);
      this.fetchQuestions(assessmentId);
    },
    async fetchAssessment(assessmentId) {
      try {
        const res = await api.get(`/Assessments/${assessmentId}`);
        const data = await res.json();
        this.assessment = data.data;
        console.log(this.assessment)
      } catch (err) {
        console.error("Failed to fetch assessment:", err);
      } finally {
        this.isLoadingAssessment = false;
      }
    },

    async loadSidebarAndNavbar() {
      const role = localStorage.getItem("userRole");
      const sidebarPath =
        role === "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbarPath =
        role === "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await Promise.all([
        loadComponent("sidebar-placeholder", sidebarPath),
        loadComponent("navbar-placeholder", navbarPath),
      ]);
    },

    async fetchQuestions(assessmentId) {
      try {
        const response = await api.get(
          `/assessments/${assessmentId}/questions/answers`
        );
        const data = await response.json();

        if (!data.status) {
          Swal.fire({
            icon: "error",
            title: "Fetch Failed",
            text: data.message || "Unable to load assessment questions.",
          });
          return;
        }

        this.questions = data.data || [];
        console.log(this.questions);
      } catch (error) {
        console.error("Error fetching questions:", error);
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "An error occurred while fetching questions.",
        });
      } finally {
        this.loading = false;
      }
    },

    getQuestionTypeLabel(type) {
      return type === 1
        ? "MCQ"
        : type === 2
        ? "Objective"
        : type === 3
        ? "Coding"
        : "Unknown";
    },
    logOut,
  };
};
