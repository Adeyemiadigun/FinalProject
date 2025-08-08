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
      if (batchRes.status) this.batches = batchRes.data;

      await this.fetchPage();
    },

    async fetchPage() {
      const params = new URLSearchParams();
      if (this.filters.batchId) params.append("batchId", this.filters.batchId);
      if (this.filters.status) params.append("status", this.filters.status);
      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.pagination.currentPage);

      const result = await api.get(
        `/Instructors/assessments?${params.toString()}`
      );
      if (result.status) {
        this.assessments = result.data.items;
        this.pagination = {
          currentPage: result.data.currentPage,
          totalPages: result.data.totalPages,
          hasNextPage: result.data.hasNextPage,
          hasPreviousPage: result.data.hasPreviousPage,
        };
        this.drawAllCharts();
      } else {
        Swal.fire(
          "Error",
          result.message || "Failed to load assessments",
          "error"
        );
      }
    },

    async submitAssessment() {
      const res = await api.post("/assessments", this.newAssessment);
      if (res.status) {
        Swal.fire("Success", "Assessment created successfully", "success");
        this.resetNewAssessment();
        await this.fetchPage();
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
        const ctx = document.getElementById(`chart-${a.id}`);
        if (!ctx) return;

        const labels = a.batchPerformance.map((bp) => bp.batchName);
        const data = a.batchPerformance.map((bp) => bp.averageScore);

        new Chart(ctx, {
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
      });
    },

    logOut,

    canAddQuestions(startDate) {
      return (new Date(startDate) - new Date()) / (1000 * 60) >= 0;
    },
  };
};
