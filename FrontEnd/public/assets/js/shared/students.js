function studentsPage() {
  return {
    students: [],
    batches: [],
    selectedBatch: "",
    searchQuery: "",
    statusFilter: "",
    pagination: {
      currentPage: 0,
      pageSize: 5,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    },

    async init() {
      await this.loadSidebarAndNavbar();
      await this.fetchBatches();
      await this.fetchStudents();
    },

    async fetchBatches() {
      const res = await fetch("/api/admin/batches");
      const data = await res.json();
      this.batches = data.data || [];
    },

    async fetchStudents() {
      const token = localStorage.getItem("token");
      const params = new URLSearchParams({
        page: this.pagination.currentPage,
        pageSize: this.pagination.pageSize,
      });

      if (this.selectedBatch) params.append("batchId", this.selectedBatch);
      if (this.searchQuery) params.append("query", this.searchQuery);
      if (this.statusFilter) params.append("status", this.statusFilter);

      const url = this.searchQuery
        ? `/api/admin/students/search?${params.toString()}`
        : `/api/admin/students?${params.toString()}`;

      const res = await fetch(url, {
        headers: { Authorization: `Bearer ${token}` },
      });

      const json = await res.json();
      if (json.status) {
        const data = json.data;
        this.students = data.items;
        this.pagination = {
          currentPage: data.currentPage,
          totalPages: data.totalPages,
          hasNextPage: data.hasNextPage,
          hasPreviousPage: data.hasPreviousPage,
          pageSize: data.pageSize,
        };
      }
    },

    resetAndFetch() {
      this.pagination.currentPage = 0;
      this.fetchStudents();
    },

    nextPage() {
      if (this.pagination.hasNextPage) {
        this.pagination.currentPage++;
        this.fetchStudents();
      }
    },

    prevPage() {
      if (this.pagination.hasPreviousPage) {
        this.pagination.currentPage--;
        this.fetchStudents();
      }
    },

    goToDetail(id) {
      window.location.href = `/admin/students/detail.html?studentId=${id}`;
    },

    async loadSidebarAndNavbar() {
      const role = localStorage.getItem("userRole");
      const sidebar =
        role === "admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role === "admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await loadComponent("sidebar-placeholder", sidebar);
      await loadComponent("navbar-placeholder", navbar);
    },
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
