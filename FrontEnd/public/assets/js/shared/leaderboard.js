async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}

function leaderboardPage() {
  return {
    selectedBatch: "",
    batches: [],
    leaderboard: [],
    pagination: {
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      totalItems: 0,
    },

    async init() {
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
      this.loadBatches();
    },

    async loadBatches() {
      const token = localStorage.getItem("accessToken");
      const res = await fetch("http://localhost:5162/api/v1/batches/all", {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      this.batches = data.data;
      this.selectedBatch = this.batches.length > 0 ? this.batches[0].id : "";
      console.log(data)
      await this.loadLeaderboard();
    },

    async loadLeaderboard() {
      const token = localStorage.getItem("accessToken");
      const query = new URLSearchParams({
        batchId: this.selectedBatch,
        pageSize: this.pagination.pageSize,
        currentPage: this.pagination.pageNumber,
      });

      const res = await fetch(
        `http://localhost:5162/api/v1/Students/leaderboard?${query.toString()}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      const response = await res.json();
      if (response.status && response.data) {
        this.leaderboard = response.data.items;
        console.log(this.leaderboard);
        this.pagination.totalPages = response.data.totalPages;
        this.pagination.totalItems = response.data.totalItems;
      }
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
  };
}
