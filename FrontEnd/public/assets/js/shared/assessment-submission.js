import { api, loadComponent, logOut } from "../shared/utils.js";

window.assessmentSubmissionsPage = function () {
  return {
    submissions: [],
    page: 1,
    totalPages: 1,
    perPage: 10,
    assessmentId: null,

    async init() {
      // Load sidebar & navbar dynamically based on user role
      await this.loadLayout();

      // Parse URL params
      const params = new URLSearchParams(window.location.search);
      this.assessmentId = params.get("id");
      if (!this.assessmentId) {
        Swal.fire("Error", "Assessment ID not provided", "error");
        return;
      }

      await this.fetchSubmissions();
    },

    async loadLayout() {
      const role = localStorage.getItem("userRole");
      let sidebarPath = "";
      let navbarPath = "";

      if (role === "Admin") {
        sidebarPath = "/public/components/sidebar.html";
        navbarPath = "/public/components/nav.html";
      } else if (role === "Instructor") {
        sidebarPath = "/public/components/instructor-sidebar.html";
        navbarPath = "/public/components/instructor-nav.html";
      } else {
        return; // Exit if role is unknown
      }

      // Use shared utility
      await Promise.all([
        loadComponent("sidebar-placeholder", sidebarPath),
        loadComponent("navbar-placeholder", navbarPath),
      ]);
    },

    async fetchSubmissions() {
      try {
        const res = await api.get(
          `/assessments/${this.assessmentId}/submissions?pageSize=${this.perPage}&currentPage=${this.page}`
        );

        const result = await res.json();
        if (result.status) {
          this.submissions = result.data.items || [];
          this.totalPages = result.data.totalPages || 1;
        } else {
          Swal.fire(
            "Error",
            result.message || "Failed to load submissions",
            "error"
          );
        }
      } catch (error) {
        console.error(error);
        Swal.fire(
          "Error",
          "An error occurred while loading submissions.",
          "error"
        );
      }
    },

    changePage(newPage) {
      if (newPage >= 1 && newPage <= this.totalPages) {
        this.page = newPage;
        this.fetchSubmissions();
      }
    },

    formatDate(dateStr) {
      const date = new Date(dateStr);
      return date.toLocaleString();
    },

    logOut,
  };
};
