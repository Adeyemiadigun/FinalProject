import { api, loadComponent, logOut } from "../shared/utils.js";

window.submissionPage = function () {
  return {
    submissions: [],
    pagination: {
      currentPage: 1,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
      pageSize: 5,
    },

    loading: false,

    async init() {
      await this.loadSidebarNavbar();
      this.fetchSubmissions();
    },

    async loadSidebarNavbar() {
      await loadComponent(
        "sidebar-placeholder",
        "/public/components/sidebar-student.html"
      );
      await loadComponent(
        "navbar-placeholder",
        "/public/components/navbar-student.html"
      );
    },

    async fetchSubmissions() {
      this.loading = true;
      try {
        const params = new URLSearchParams({
          pageSize: this.pagination.pageSize,
          currentPage: this.pagination.currentPage,
        });

        const res = await api.get(`/Students/submissions?${params}`);
        const data = await res.json();

        if (data?.status && data.data) {
          this.submissions = data.data.items;

          // keep currentPage controlled by frontend
          this.pagination.totalPages = data.data.totalPages;
          this.pagination.hasNextPage = data.data.hasNextPage;
          this.pagination.hasPreviousPage = data.data.hasPreviousPage;
        } else {
          this.submissions = [];
        }
      } catch (error) {
        console.error("Failed to fetch submissions:", error);
        this.submissions = [];
      } finally {
        this.loading = false;
      }
    },

    nextPage() {
      if (this.pagination.hasNextPage) {
        this.pagination.currentPage++;
        this.fetchSubmissions();
      }
    },

    prevPage() {
      if (this.pagination.hasPreviousPage) {
        this.pagination.currentPage--;
        this.fetchSubmissions();
      }
    },

    logOut() {
      logOut();
    },

    formatDate(dateStr) {
      const date = new Date(dateStr);
      return date.toLocaleString();
    },
  };
};
