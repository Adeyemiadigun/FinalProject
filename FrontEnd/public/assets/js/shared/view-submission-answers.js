async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
function viewSubmissionPage() {
  return {
    submissionId: new URLSearchParams(window.location.search).get("id"),
    submission: {
      submittedAnswers: [],
    },
    sidebarOpen: true,
    token: localStorage.getItem("accessToken"),

    async init() {
      await this.loadLayout();
      await this.loadSubmission();
    },

    async loadLayout() {
      const role = localStorage.getItem("userRole");
      const sidebar =
        role == "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role == "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await loadComponent("sidebar-placeholder", sidebar);
      await loadComponent("navbar-placeholder", navbar);
    },

    async loadSubmission() {
      try {
        const res = await fetch(
          `https://localhost:7157/api/v1/students/${this.submissionId}/submission`,
          {
            headers: {
              Authorization: `Bearer ${this.token}`,
            },
          }
        );

        const data = await res.json();
        if (data.status) {
          this.submission = data.data;
        } else {
          alert("Failed to load submission");
        }
      } catch (err) {
        console.error("Error fetching submission:", err);
        alert("Failed to load submission");
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
  };
}
