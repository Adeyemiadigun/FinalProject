const token = localStorage.getItem("accessToken");
const headers = {
  "Content-Type": "application/json",
  Authorization: `Bearer ${token}`,
};

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

      const batchRes = await fetch(
        "https://localhost:7157/api/v1/Batches/all",
        {
          method: "GET",
          headers: headers,
        }
      );
      const data = await batchRes.json();
      console.log(data);
      this.batches = data.data;
      await this.fetchPage();
    },

    async fetchPage() {
      
      const params = new URLSearchParams();
      if (this.filters.batchId) params.append("batchId", this.filters.batchId);
      if (this.filters.status) params.append("status", this.filters.status);
      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.pagination.currentPage);

      const res = await fetch(
        `https://localhost:7157/api/v1/Instructors/assessments?${params}`,
        {
          method: "GET",
          headers: headers,
        }
      );
      const result = await res.json();
      console.log(result);
      this.assessments = result.data.items;
      console.log(this.assessments);
      this.pagination = {
        currentPage: result.data.currentPage,
        totalPages: result.data.totalPages,
        hasNextPage: result.data.hasNextPage,
        hasPreviousPage: result.data.hasPreviousPage,
      };
      this.drawAllCharts();
    },

    async submitAssessment() {
      console.log(this.newAssessment)
      const res = await fetch("https://localhost:7157/api/v1/assessments", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(this.newAssessment),
      });
      if (res.ok) {
        alert("Assessment created successfully");
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
        console.log(this.newAssessment);
        await this.fetchPage();
      } else {
        alert("Failed to create assessment.");
      }
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
        console.log(this.pagination.currentPage)
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
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },

    canAddQuestions(startDate) {
      const start = new Date(startDate);
      const now = new Date();
      const diffMinutes = (start - now) / (1000 * 60);
      return diffMinutes > 10;
    },
  };
}
