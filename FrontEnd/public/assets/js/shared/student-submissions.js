import { api, loadComponent, logOut } from "../shared/utils.js";

window.adminStudentSubmissions = function () {
  return {
    studentId: new URLSearchParams(window.location.search).get("id"),
    submissions: [],
    studentName: "",

    // Pagination state
    currentPage: 1,
    totalPages: 0,
    pageSize: 5,
    hasNextPage: false,
    hasPreviousPage: false,
    loading: true,

    async init() {
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

      await this.fetchSubmissions(1); // Load first page
    },

    async fetchSubmissions(page) {
      this.currentPage = page;

      this.loading = true;

      try {
        const res = await api.get(
          `/Students/${this.studentId}/submissions?pageSize=${this.pageSize}&currentPage=${page}`
        );
        const json = await res.json();

        if (json.status && json.data) {
          const data = json.data;
          this.submissions = data.items;
          this.totalPages = data.totalPages;
          this.hasNextPage = data.hasNextPage;
          this.hasPreviousPage = data.hasPreviousPage;

          this.studentName =
            this.submissions.length > 0 ? this.submissions[0].studentName : "";
        } else {
          Swal.fire(
            "Error",
            json.message || "Failed to load submissions.",
            "error"
          );
        }
      } catch (error) {
        console.error("Error fetching submissions:", error);
        Swal.fire("Error", "Could not fetch submissions.", "error");
      } finally {
        this.loading = false;
      }
    },

    changePage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.fetchSubmissions(page);
      }
    },

    pages() {
      // Generate an array like [1, 2, 3, ... totalPages]
      return Array.from({ length: this.totalPages }, (_, i) => i + 1);
    },

    logOut,
  };
};
