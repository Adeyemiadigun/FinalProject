
async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
function adminStudentSubmissions() {
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

    async init() {
      const token = localStorage.getItem("accessToken");
      const role = localStorage.getItem("userRole");

      await loadComponent(
        "sidebar-placeholder",
        role == "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html"
      );
      await loadComponent(
        "navbar-placeholder",
        role == "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html"
      );

      await this.fetchSubmissions(1); // Load first page
    },

    async fetchSubmissions(page) {
      this.currentPage = page;
      const token = localStorage.getItem("accessToken");

      const res = await fetch(
        `https://localhost:7157/api/v1/Students/${this.studentId}/submissions?pageNumber=${page}&pageSize=${this.pageSize}`,
        { headers: { Authorization: `Bearer ${token}` } }
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

    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
  };
}
