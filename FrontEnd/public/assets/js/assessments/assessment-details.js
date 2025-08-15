import { api, loadComponent, logOut } from "../shared/utils.js";

window.assessmentDetailsPage = function () {
  return {
    assessment: {},
    metrics: {},
    students: [],
    overview: {},
    groupedStudents: [],
    showGroupedStudents: false,
    selectedGroupType: 1,
    selectedGroupLabel: "",
    page: 1,
    perPage: 5,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
    assessmentId: new URLSearchParams(window.location.search).get(
      "assessmentId"
    ),
    charts: {},
    groupedStudents: [],
    groupedPage: 1,
    groupedPerPage: 10,
    groupedTotalPages: 1,
    groupedHasNextPage: false,
    groupedHasPreviousPage: false,
    batchChart : false,
    perfChart : false,

    async init() {
      await Promise.all([
        loadComponent("sidebar-placeholder", "../components/sidebar.html"),
        loadComponent("navbar-placeholder", "../components/nav.html"),
      ]);
      this.loadAssessment();
    },

    async loadAssessment() {
      await Promise.all([
        this.fetchAssessment(),
        this.fetchMetrics(),
        this.fetchScoreDistribution(),
        this.fetchBatchPerformance(),
        this.fetchStudentPerformance(),
        this.fetchOverview(),
        this.fetchGroupedStudents(),
      ]);
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

      if (dist.length > 0) {
        this.drawScoreDistributionChart(dist);
      } else {
        // Handle empty case (optional: show "No data" message)
      }
    },

    async fetchBatchPerformance() {
      const res = await api.get(
        `/Assessments/${this.assessmentId}/batch-performance`
      );
      const data = await res.json();
      const perf = data.data || [];

      if (perf.length > 0) {
        this.drawBatchPerformanceChart(perf);
      } else {
        // Handle empty case (optional: show "No data" message)
      }
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

    async fetchOverview() {
      const res = await api.get(`/Assessments/${this.assessmentId}/overview`);
      const data = await res.json();
      this.overview = data;
      console.log(data);
    },

    async fetchGroupedStudents() {
      const res = await api.get(
        `/Assessments/${this.assessmentId}/students/grouped?type=${this.selectedGroupType}&PageSize=${this.groupedPerPage}&CurrentPage=${this.groupedPage}`
      );
      const data = await res.json();
      const paginated = data.data;

      this.groupedStudents = paginated.items || [];
      this.groupedTotalPages = paginated.totalPages;
      this.groupedHasNextPage = paginated.hasNextPage;
      this.groupedHasPreviousPage = paginated.hasPreviousPage;
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

    drawScoreDistributionChart(data) {
      if(this.perfChart)return
      this.perfChart = true
      const labels = data.map((x) => x.cap);
      const values = data.map((x) => x.count);

      this.drawChart("scoreDistChart", {
        type: "bar",
        labels,
        datasets: [
          {
            label: "Number of Students",
            data: values,
            backgroundColor: "#3b82f6",
          },
        ],
      });
    },
    drawChart(id, { type, labels, datasets }) {
      const ctx = document.getElementById(id)?.getContext("2d");
      if (!ctx) return;

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
    drawBatchPerformanceChart(data) {
       if (this.batchChart) return;
       this.batchChart = true;
      const labels = data.map((x) => x.batchName);
      const values = data.map((x) => x.averageScore);

      this.drawChart("batchPerfChart", {
        type: "doughnut",
        labels,
        datasets: [
          {
            data: values,
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

    logOut,
  };
};
