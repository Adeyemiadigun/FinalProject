import { api, loadComponent, logOut } from "../shared/utils.js";

window.assessmentDetailsPage = function () {
  return {
    assessment: {},
    metrics: {},
    students: [],
    page: 1,
    perPage: 5,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
    assessmentId: new URLSearchParams(window.location.search).get(
      "assessmentId"
    ),
    charts: {},

    async init() {
      // Load layout components first
      await Promise.all([
        loadComponent("sidebar-placeholder", "../components/sidebar.html"),
        loadComponent("navbar-placeholder", "../components/nav.html"),
      ]);

      // Load all data
      this.loadAssessment();
    },

    async loadAssessment() {
      await this.fetchAssessment();
      await this.fetchMetrics();
      await this.fetchScoreDistribution();
      await this.fetchBatchPerformance();
      await this.fetchStudentPerformance();
    },

    async fetchAssessment() {
      const res = await api.get(`/Assessments/${this.assessmentId}`);
      const data = await res.json();
      this.assessment = data.data;
    },

    async fetchMetrics() {
      const res = await api.get(`/Assessments/${this.assessmentId}/metrics`);
      const data = await res.json();
      this.metrics = data.data;
    },

    async fetchScoreDistribution() {
      const res = await api.get(
        `/Assessments/${this.assessmentId}/score-distribution`
      );
      const data = await res.json();
      const dist = data.data || [];

      this.drawChart("scoreDistChart", {
        type: "bar",
        labels: dist.map((x) => x.cap),
        datasets: [
          {
            label: "Number of Students",
            data: dist.map((x) => x.count),
            backgroundColor: "#3b82f6",
          },
        ],
      });
    },

    async fetchBatchPerformance() {
      const res = await api.get(
        `/Assessments/${this.assessmentId}/batch-performance`
      );
      const data = await res.json();
      const perf = data.data || [];

      this.drawChart("batchPerfChart", {
        type: "doughnut",
        labels: perf.map((x) => x.batchName),
        datasets: [
          {
            data: perf.map((x) => x.averageScore),
            backgroundColor: [
              "#3b82f6",
              "#f59e0b",
              "#10b981",
              "#ef4444",
              "#6366f1",
            ],
          },
        ],
      });
    },

    async fetchStudentPerformance() {
      const res = await api.get(
        `/Assessments/${this.assessmentId}/students?CurrentPage=${this.page}&PageSize=${this.perPage}`
      );
      const data = await res.json();
      const paginated = data.data;

      this.students = paginated.items || [];
      this.totalPages = paginated.totalPages;
      this.hasNextPage = paginated.hasNextPage;
      this.hasPreviousPage = paginated.hasPreviousPage;
    },

    prevPage() {
      if (this.page > 1) {
        this.page--;
        this.fetchStudentPerformance();
      }
    },

    nextPage() {
      if (this.page < this.totalPages) {
        this.page++;
        this.fetchStudentPerformance();
      }
    },

    drawChart(id, { type, labels, datasets }) {
      const ctx = document.getElementById(id)?.getContext("2d");
      if (!ctx) return;

      // Destroy old chart if exists
      if (this.charts[id]) {
        this.charts[id].destroy();
      }

      this.charts[id] = new Chart(ctx, {
        type,
        data: { labels, datasets },
        options: {
          responsive: true,
          scales: type === "bar" ? { y: { beginAtZero: true } } : {},
        },
      });
    },

    logOut,
  };
};
