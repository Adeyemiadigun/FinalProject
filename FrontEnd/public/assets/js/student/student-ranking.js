import { api, loadComponent, logOut } from "../shared/utils.js";

window.studentRanking = function () {
  return {
    viewMode: "my-batch", // "my-batch" or "all-batches"
    rankings: [],
    currentPage: 1,
    pageSize: 10,
    totalItems: 0,
    loading: false,

    get filteredRankings() {
      return this.rankings;
    },

    get paginatedRankings() {
      return this.filteredRankings;
    },

    nextPage() {
      if (this.currentPage * this.pageSize < this.totalItems) {
        this.currentPage++;
        this.fetchRankings();
      }
    },

    prevPage() {
      if (this.currentPage > 1) {
        this.currentPage--;
        this.fetchRankings();
      }
    },

    async init() {
      // Load shared layout components
      await Promise.all([
        loadComponent(
          "sidebar-placeholder",
          "/public/components/sidebar-student.html"
        ),
        loadComponent(
          "navbar-placeholder",
          "/public/components/navbar-student.html"
        ),
      ]);

      this.fetchRankings();
    },

    logOut() {
      logOut(); // Uses central logout
    },

    async fetchRankings() {
      this.loading = true;
      const params = new URLSearchParams({
        pageSize: this.pageSize,
        currentPage: this.currentPage,
        viewMode: this.viewMode,
      });

      try {
        // Endpoint depends on view mode
        const endpoint =
          this.viewMode === "my-batch"
            ? `/Students/batch/leaderboard?${params.toString()}`
            : `/Students/all/leaderboard?${params.toString()}`;

        const res = await api.get(endpoint);
        const json = await res.json();
        

        if (json.status) {
          this.rankings = json.data.items.map((s) => ({
            id: s.id,
            name: s.name,
            score: s.avgScore,
            completed: s.completedAssessments,
            batch: s.batchName || "N/A",
            isCurrentUser: s.id === this.getCurrentUserId(),
          }));
          this.totalItems = json.data.totalItems;
        } else {
          Swal.fire(
            "Error",
            json.message || "Failed to load leaderboard.",
            "error"
          );
        }
      } catch (err) {
        console.error("Error loading leaderboard:", err);
        Swal.fire("Error", "Unable to fetch leaderboard.", "error");
      } finally {
        this.loading = false;
      }
    },

    getCurrentUserId() {
      try {
        const token = localStorage.getItem("accessToken");
        if (!token) return null;
        const payload = JSON.parse(atob(token.split(".")[1]));
        return payload.sub || payload.id || null;
      } catch {
        return null;
      }
    },
  };
};
