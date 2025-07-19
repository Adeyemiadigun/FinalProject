function assessmentSubmissionsPage() {
  return {
    submissions: [],
    page: 1,
    totalPages: 1,
    perPage: 10,
    assessmentId: null,

    init() {
      const params = new URLSearchParams(window.location.search);
      this.assessmentId = params.get("id");
      if (!this.assessmentId) {
        Swal.fire("Error", "Assessment ID not provided", "error");
        return;
      }
      this.fetchSubmissions();
    },

    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
    async fetchSubmissions() {
      const token = localStorage.getItem("accessToken");
      const url = `https://localhost:7157/api/v1/assessments/${this.assessmentId}/submissions?pageSize=${this.perPage}&currentPage=${this.page}`;

      try {
        const res = await fetch(url, {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });

        const result = await res.json();

        if (result.status) {
          this.submissions = result.data.items;
          this.totalPages = result.data.totalPages;
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
  };
}
