import { api, loadComponent, logOut } from "../shared/utils.js";

window.instructorAssessmentsPage = function () {
  return {
    assessments: [],
    batches: [],
    filters: { batchId: "", status: "" },
    pagination: {
      currentPage: 1,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    },
    charts: {},
    isLoading: true,
    pageSize: 2,
    showCreateModal: false,
    newAssessment: {
      title: "",
      description: "",
      technologyStack: "",
      durationInMinutes: 0,
      batchIds: [],
      startDate: "",
      endDate: "",
      passingScore: 0,
    },

    async init() {
      await loadComponent(
        "sidebar-placeholder",
        "../components/instructor-sidebar.html"
      );
      await loadComponent(
        "navbar-placeholder",
        "../components/instructor-nav.html"
      );

      const batchRes = await api.get("/Batches/all");
      if (batchRes.status) {
        var batchData = await batchRes.json();
        this.batches = batchData.data || [];
        console.log("Batches:", batchData);
      }
      await this.fetchPage();
    },

    async fetchPage() {
      this.isLoading = true;
      const params = new URLSearchParams();
      if (this.filters.batchId) params.append("batchId", this.filters.batchId);
      if (this.filters.status) params.append("status", this.filters.status);
      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.pagination.currentPage);
      console.log("Fetching assessments with params:", params.toString());

      const result = await api.get(
        `/Instructors/assessments?${params.toString()}`
      );
      console.log("Assessments result:", result);
      if (result.status) {
        var res = await result.json();
        console.log("Assessments data:", res.data);
        this.assessments = res.data.items;
        this.pagination = {
          currentPage: res.data.currentPage,
          totalPages: res.data.totalPages,
          hasNextPage: res.data.hasNextPage,
          hasPreviousPage: res.data.hasPreviousPage,
        };
        this.drawAllCharts();
      } else {
        Swal.fire(
          "Error",
          result.message || "Failed to load assessments",
          "error"
        );
      }
      this.isLoading = false;
    },

    async submitAssessment() {
      const res = await api.post("/assessments", this.newAssessment);
      if (res.status) {
        Swal.fire("Success", "Assessment created successfully", "success");
        this.resetNewAssessment();
        await this.fetchPage();
        let result = await res.json();
        const assessmentId = result.data.id;
        window.href.relocate = `/public/instructor/assessment-questions.html?assessmentId=${assessmentId}`;
      } else {
        Swal.fire(
          "Error",
          res.message || "Failed to create assessment",
          "error"
        );
      }
    },

    resetNewAssessment() {
      this.showCreateModal = false;
      this.newAssessment = {
        title: "",
        description: "",
        technologyStack: "",
        durationInMinutes: 0,
        batchIds: [],
        startDate: "",
        endDate: "",
        passingScore: 0,
      };
    },

    prevPage() {
      if (this.pagination.hasPreviousPage) {
        this.pagination.currentPage--;
        this.fetchPage();
      }
    },

    nextPage() {
      if (this.pagination.hasNextPage) {
        this.pagination.currentPage++;
        this.fetchPage();
      }
    },

    applyFilters() {
      this.pagination.currentPage = 1;
      this.fetchPage();
    },

    drawAllCharts() {
      this.assessments.forEach((a) => {
        const chartId = `chart-${a.id}`;
        const ctx = document.getElementById(chartId);
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (this.charts[chartId]) {
          this.charts[chartId].destroy();
          delete this.charts[chartId];
        }

        const labels = a.batchPerformance.map((bp) => bp.batchName);
        const data = a.batchPerformance.map((bp) => bp.averageScore);

        const newChart = new Chart(ctx, {
          type: "bar",
          data: {
            labels,
            datasets: [
              {
                label: "Avg Score (%)",
                data,
                backgroundColor: "#3b82f6",
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, max: 100 } },
          },
        });

        // Store the chart instance
        this.charts[chartId] = newChart;
      });
    },

    logOut,

    // canAddQuestions(startDate) {
    //   return (new Date(startDate) - new Date()) / (1000 * 60) >= 0;
    // },
    canAddQuestions(startDate) {
  return new Date(startDate) > new Date();
},
  };
};
