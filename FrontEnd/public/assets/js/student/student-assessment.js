import { api, loadComponent, logOut } from "../shared/utils.js";

window.studentAssessments = function () {
  return {
    assessments: [],
    status: "",
    page: 1,
    perPage: 5,
    totalPages: 1,
    interval: null,

    async init() {
      await loadComponent(
        "sidebar-placeholder",
        "/public/components/sidebar-student.html"
      );
      await loadComponent(
        "navbar-placeholder",
        "/public/components/navbar-student.html"
      );

      await this.fetchAssessments();
      this.startCountdown();
    },

    async fetchAssessments() {
      try {
        const params = new URLSearchParams({
          pageSize: this.perPage,
          currentPage: this.page,
          status: this.status || "",
        });

        const res = await api.get(`/Students/assessments?${params.toString()}`);
        const result = await res.json();

        if (result.status) {
          this.assessments = result.data.items.map((a) => {
            const status = this.determineStatus(a.startDate, a.endDate);
            return {
              ...a,
              status,
              submitted: a.submitted,
              countdown:
                status !== "Completed"
                  ? this.calculateCountdown(a, status)
                  : "—",
            };
          });
          this.totalPages = result.data.totalPages;
        } else {
          Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Failed to load assessments.",
          });
        }
      } catch (error) {
        console.error("Error fetching assessments:", error);
        Swal.fire({
          icon: "error",
          title: "An error occurred",
          text: "Something went wrong while fetching assessments.",
        });
      }
    },

    calculateCountdown(a, status) {
      const now = new Date();
      const target =
        status === "Upcoming" ? new Date(a.startDate) : new Date(a.endDate);
      const diff = target - now;
      if (diff <= 0) return "Now";
      const h = Math.floor(diff / (1000 * 60 * 60));
      const m = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const s = Math.floor((diff % (1000 * 60)) / 1000);
      return `${h}h ${m}m ${s}s`;
    },

    determineStatus(startDate, endDate) {
      const now = new Date();
      const start = new Date(startDate);
      const end = new Date(endDate);
      if (now < start) return "Upcoming";
      if (now >= start && now <= end) return "InProgress";
      return "Completed";
    },

    startCountdown() {
      this.interval = setInterval(() => {
        this.assessments.forEach((a) => {
          if (a.status === "Upcoming") {
            const diff = new Date(a.startDate) - new Date();
            if (diff <= 0) {
              a.status = "InProgress";
              a.countdown = "Now";
            } else {
              const h = Math.floor(diff / (1000 * 60 * 60));
              const m = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
              const s = Math.floor((diff % (1000 * 60)) / 1000);
              a.countdown = `${h}h ${m}m ${s}s`;
            }
          }
        });
      }, 1000);
    },

    formatDate(dateStr) {
      return new Date(dateStr).toLocaleString();
    },

    nextPage() {
      if (this.page < this.totalPages) {
        this.page++;
        this.fetchAssessments();
      }
    },

    prevPage() {
      if (this.page > 1) {
        this.page--;
        this.fetchAssessments();
      }
    },

    startAssessment(id) {
      window.location.href = `/public/student/assessment-attempt-page.html?id=${id}`;
    },

    resumeAssessment(id) {
      window.location.href = `/public/student/assessment-attempt-page.html?id=${id}`;
    },

    viewResult(id) {
      window.location.href = `/public/student/view-assessment-result.html?id=${id}`;
    },

    logOut,
  };
};
