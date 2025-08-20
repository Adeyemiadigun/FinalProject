import { api, loadComponent, logOut } from "../shared/utils.js";

window.leaderboardPage = function () {
  return {
    selectedBatch: "",
    batches: [],
    leaderboard: [],
    sidebarOpen: true,
    pagination: {
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      totalItems: 0,
    },

    isLoading: true,

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

      this.loadBatches();
    },

    async loadBatches() {
      try {
        const res = await api.get("/batches/all");
        const data = await res.json();
        this.batches = data.data || [];
        this.selectedBatch = this.batches.length > 0 ? this.batches[0].id : "";
        await this.loadLeaderboard();
      } catch (error) {
        console.error("Failed to load batches:", error);
        Swal.fire("Error", "Could not load batches.", "error");
      }
    },

    async loadLeaderboard() {    
    this.isLoading = true;
      try {
        const query = new URLSearchParams({
          batchId: this.selectedBatch,
          pageSize: this.pagination.pageSize,
          currentPage: this.pagination.pageNumber,
        });

        const res = await api.get(`/Students/leaderboard?${query.toString()}`);
        const response = await res.json();

        if (response.status && response.data) {
          this.leaderboard = response.data.items;
          this.pagination.totalPages = response.data.totalPages;
          this.pagination.totalItems = response.data.totalItems;
        } else {
          this.leaderboard = [];
          Swal.fire(
            "Info",
            response.message || "No leaderboard data found.",
            "info"
          );
        }
      } catch (error) {
        console.error("Failed to load leaderboard:", error);
        Swal.fire("Error", "Could not load leaderboard.", "error");
      } 
    this.isLoading = false;
    },

    viewStudentDetails(studentId) {
      window.location.href = `/public/shared/student-details.html?id=${studentId}`;
    },

    filterByBatch() {
      this.pagination.pageNumber = 1;
      this.loadLeaderboard();
    },

    prevPage() {
      if (this.pagination.pageNumber > 1) {
        this.pagination.pageNumber--;
        this.loadLeaderboard();
      }
    },

    nextPage() {
      if (this.pagination.pageNumber < this.pagination.totalPages) {
        this.pagination.pageNumber++;
        this.loadLeaderboard();
      }
    },

    logOut,
  };
};
