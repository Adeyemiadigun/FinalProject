function submissionPage() {
  return {
    submissions: [],
    pagination: {
      currentPage: 1,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
      pageSize: 5,
    },

    loading: false, // ⬅️ Added

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
      const token = localStorage.getItem("accessToken");
      const params = new URLSearchParams({
        pageSize: this.pagination.pageSize,
        currentPage: this.pagination.currentPage,
      });

      const res = await fetch(
        `https://localhost:7157/api/v1/Students/submissions?${params}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      const data = await res.json();

      if (data?.status && data.data) {
        this.submissions = data.data.items;
        this.pagination.currentPage = data.data.currentPage;
        this.pagination.totalPages = data.data.totalPages;
        this.pagination.hasNextPage = data.data.hasNextPage;
        this.pagination.hasPreviousPage = data.data.hasPreviousPage;
      } else {
        this.submissions = [];
      }
      this.loading = false; // ⬅️ Hide loader after fetching
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
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },

    formatDate(dateStr) {
      const date = new Date(dateStr);
      return date.toLocaleString();
    },
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
