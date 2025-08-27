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
    showEditModal: false,
    editAssessment: {
      id: null,
      title: "",
      description: "",
      technologyStack: "",
      durationInMinutes: 0,
      startDate: "",
      endDate: "",
      passingScore: 0,
    },
    validationErrors: {},
    openEditModal(assessment) {
      this.editAssessment = {
        id: assessment.id,
        title: assessment.title,
        description: assessment.description,
        technologyStack: assessment.technologyStack,
        durationInMinutes: assessment.durationInMinutes,
        startDate: assessment.startDate,
        endDate: assessment.endDate,
        passingScore: assessment.passingScore,
      };
      this.showEditModal = true;
    },
    canEditAssessment(startDate) {
      const now = new Date();
      const start = new Date(startDate);
      const diffInMinutes = (start - now) / (1000 * 60);
      return diffInMinutes > 3; // Disable if assessment starts in less than or equal to 3 minutes
    },
    validateErrors()
    {
      this.validationErrors = {}; // reset

      if (!this.newAssessment.title?.trim()) {
        this.validationErrors.title = "Title is required.";
      }
      if (!this.newAssessment.description?.trim()) {
        this.validationErrors.description = "Description is required.";
      }
      if (!this.newAssessment.technologyStack) {
        this.validationErrors.technologyStack = "Technology stack is required.";
      }
      if (
        !this.newAssessment.durationInMinutes ||
        this.newAssessment.durationInMinutes <= 0
      ) {
        this.validationErrors.durationInMinutes = "Duration must be greater than 0.";
      }
      if (!this.newAssessment.startDate) {
        this.validationErrors.startDate = "Start date is required.";
      }
      if (!this.newAssessment.endDate) {
        this.validationErrors.endDate = "End date is required.";
      }
      if (!this.newAssessment.passingScore && this.newAssessment.passingScore !== 0) {
        this.validationErrors.passingScore = "Passing score is required.";
      } else if (
        this.newAssessment.passingScore < 0 ||
        this.newAssessment.passingScore > 100
      ) {
        this.validationErrors.passingScore =
          "Passing score must be between 0 and 100.";
      }
      if (!this.newAssessment.batchIds?.length) {
        this.validationErrors.batchIds = "At least one batch must be selected.";
      }

      if (
        this.newAssessment.startDate &&
        this.newAssessment.endDate &&
        new Date(this.newAssessment.endDate) <= new Date(this.newAssessment.startDate)
      ) {
        this.validationErrors.endDate = "End date must be after start date.";
      }

      return Object.keys(this.validationErrors).length === 0;
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
async submitEdit() {
  const res = await api.put(`/assessments/${this.editAssessment.id}`, this.editAssessment);
  if (res.status) {
    Swal.fire("Success", "Assessment updated", "success");
    this.showEditModal = false;
    await this.fetchPage();
  } else {
    Swal.fire("Error", res.message || "Update failed", "error");
  }
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
      this.validateErrors();
      if (Object.keys(this.validationErrors).length > 0) return; 
      const res = await api.post("/assessments", this.newAssessment);
      if (res.status) {
        Swal.fire("Success", "Assessment created successfully", "success");
        this.resetNewAssessment();
        await this.fetchPage();
        let result = await res.json();
        const assessmentId = result.data.id;
        window.location.href = `/public/instructor/assessment-questions.html?assessmentId=${assessmentId}`;
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
