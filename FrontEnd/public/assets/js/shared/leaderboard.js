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
      if (role != "admin") {
        
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");
      }
      if (role === "instructor") {
        await loadComponent("sidebar-placeholder", "../components/instructor-sidebar.html");
        await loadComponent("navbar-placeholder", "../components/instructor-nav.html");
      }
      await this.loadBatches();
      await this.loadLeaderboard();
    },

    async loadBatches() {
      const token = localStorage.getItem("token");
      const res = await fetch("/api/v1/batches", {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      this.batches = data;
    },

    async loadLeaderboard() {
      const token = localStorage.getItem("token");
      const query = new URLSearchParams({
        batchId: this.selectedBatch || "",
        pageNumber: this.pagination.pageNumber,
        pageSize: this.pagination.pageSize,
      });

      const res = await fetch(`/api/v1/admin/leaderboard?${query}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      const response = await res.json();
      if (response.status && response.data) {
        this.leaderboard = response.data.items;
        this.pagination.totalPages = response.data.totalPages;
        this.pagination.totalItems = response.data.totalItems;
      }
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
