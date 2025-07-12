function instructorAssessmentsPage() {
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
    showCreateModal: false,
    newAssessment: {
      title: "",
      description: "",
      technologyStack: "",
      durationInMinutes: 0,
      startDate: "",
      endDate: "",
      passingScore: 0,
      batchIds: [],
    },

    async init() {
      const token = localStorage.getItem("token");
      await loadComponent(
        "sidebar-placeholder",
        "../components/instructor-sidebar.html"
      );
      await loadComponent(
        "navbar-placeholder",
        "../components/instructor-nav.html"
      );

      const batchRes = await fetch("/api/v1/instructor/batches", {
        headers: { Authorization: `Bearer ${token}` },
      });
      this.batches = await batchRes.json();
      await this.fetchPage(1);
    },

    async fetchPage(page) {
      const token = localStorage.getItem("token");
      const params = new URLSearchParams({
        pageNumber: page,
        pageSize: 5,
        batchId: this.filters.batchId,
        status: this.filters.status,
      });

      const res = await fetch(`/api/v1/instructor/assessments?${params}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const result = await res.json();
      this.assessments = result.data.items;
      this.pagination = {
        currentPage: result.data.currentPage,
        totalPages: result.data.totalPages,
        hasNextPage: result.data.hasNextPage,
        hasPreviousPage: result.data.hasPreviousPage,
      };
      this.drawAllCharts();
    },

    async submitAssessment() {
      const token = localStorage.getItem("token");
      const res = await fetch("/api/v1/instructor/assessments", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(this.newAssessment),
      });
      if (res.ok) {
        this.showCreateModal = false;
        this.newAssessment = {
          title: "",
          description: "",
          technologyStack: "",
          durationInMinutes: 0,
          startDate: "",
          endDate: "",
          passingScore: 0,
          batchIds: [],
        };
        await this.fetchPage(1);
      } else {
        alert("Failed to create assessment.");
      }
    },

    applyFilters() {
      this.fetchPage(1);
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

    canAddQuestions(startDate) {
      const start = new Date(startDate);
      const now = new Date();
      const diffMinutes = (start - now) / (1000 * 60);
      return diffMinutes > 10;
    },
  };
}
