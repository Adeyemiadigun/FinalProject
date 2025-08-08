import { api, loadComponent, logOut } from "../shared/utils.js";

window.viewSubmissionPage = function () {
  return {
    submissionId: new URLSearchParams(window.location.search).get("id"),
    submission: { submittedAnswers: [] },
    sidebarOpen: true,

    async init() {
      // ✅ Redirect if no submission ID
      if (!this.submissionId) {
        Swal.fire("Error", "No submission ID provided!", "error").then(() => {
          window.location.href = "/public/shared/student-submission.html";
        });
        return;
      }

      await this.loadLayout();
      await this.loadSubmission();
    },

    async loadLayout() {
      const role = localStorage.getItem("userRole");
      const sidebar =
        role === "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role === "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await Promise.all([
        loadComponent("sidebar-placeholder", sidebar),
        loadComponent("navbar-placeholder", navbar),
      ]);
    },

    async loadSubmission() {
      try {
        const res = await api.get(`/students/${this.submissionId}/submission`);
        const data = await res.json();

        if (data.status) {
          this.submission = data.data;
        } else {
          Swal.fire("Error", "Failed to load submission", "error");
        }
      } catch (err) {
        console.error("Error fetching submission:", err);
        Swal.fire("Error", "Failed to load submission", "error");
      }
    },

    getTypeLabel(type) {
      return (
        {
          1: "MCQ",
          2: "Objective",
          3: "Coding",
        }[type] || "Unknown"
      );
    },

    formatDate(dateStr) {
      return new Date(dateStr).toLocaleString();
    },

    logOut,
  };
};
