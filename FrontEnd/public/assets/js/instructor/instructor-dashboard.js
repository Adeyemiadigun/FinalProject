import { api, loadComponent, logOut } from "../shared/utils.js";

window.instructorDashboard = function () {
  return {
    summary: {
      totalAssessments: 0,
      totalStudents: 0,
      avgScore: 0,
      passRate: 0,
    },
    batches: [],
    recentAssessments: [],
    assessmentScoreTrends: [],
    scoreByTypes: [],
    filters: {
      fromDate: "",
      toDate: "",
    },
    loading: true,
    scoreChart: null,
    scoreByTypesChart: null,
    showCreateModal: false,
    newAssessment: {
      title: "",
      description: "",
      technologyStack: null,
      durationInMinutes: null,
      startDate: "",
      endDate: "",
      passingScore: null,
      batchIds: [],
    },
    resetNewAssessment() {
      this.newAssessment = {
        title: "",
        description: "",
        technologyStack: null,
        durationInMinutes: null,
        startDate: "",
        endDate: "",
        passingScore: null,
        batchIds: [],
      };
    },

    async init() {
      await this.loadLayout();
      await this.loadData();
      await this.loadScores();
      await this.loadScoreByTypes();
      this.loading = false;
    },

    async loadLayout() {
      const role = localStorage.getItem("userRole");
      const sidebar =
        role === "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role === "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await loadComponent("sidebar-placeholder", sidebar);
      await loadComponent("navbar-placeholder", navbar);
    },

    async loadData() {
      try {
        const metricsRes = await api.get(
          "/Dashboard/instructor/metrics/overview"
        );
        const metrics = await metricsRes.json();
        if (metrics.status) this.summary = metrics.data;
      } catch (error) {
        console.error("Failed to load metrics:", error);
      }

      try {
        const batchRes = await api.get("/Dashboard/batch-distribution");
        const batchData = await batchRes.json();
        if (batchData.status) this.batches = batchData.data;
        console.log(
          "Batches reactive in Alpine?",
          this.batches.map((b) => b.batchName)
        );
      } catch (error) {
        console.error("Failed to load batch distribution:", error);
      }

      try {
        const recentRes = await api.get("/Instructors/assessment/recents");
        const recent = await recentRes.json();
        if (recent.status) this.recentAssessments = recent.data;
      } catch (error) {
        console.error("Failed to load recent assessments:", error);
      }
      console.log(this.batches);
      this.drawBatchChart();
    },
    async submitAssessment() {
  try {
    const res = await api.post("/assessments", this.newAssessment);
    const json = await res.json();

    if (json.status) {
      Swal.fire("Success", "Assessment created successfully", "success");
      const assessmentId = json.data.id;

      this.resetNewAssessment();
      this.showCreateModal = false;
      await this.fetchPage?.(); // Optional: refresh page data

      // Redirect to question setup page
      window.location.href = `/public/instructor/assessment-questions.html?assessmentId=${assessmentId}`;
    } else {
      Swal.fire("Error", json.message || "Failed to create assessment", "error");
    }
  } catch (error) {
    console.error("Submission failed", error);
    Swal.fire("Error", "Something went wrong while creating assessment", "error");
  }
},
    async loadScores() {
      try {
        let query = [];
        if (this.filters.fromDate)
          query.push(`fromDate=${this.filters.fromDate}`);
        if (this.filters.toDate) query.push(`toDate=${this.filters.toDate}`);
        const queryString = query.length ? `?${query.join("&")}` : "";

        const res = await api.get(
          `/Assessments/assessment-scores${queryString}`
        );
        const json = await res.json();
        if (json.status) {
          this.assessmentScoreTrends = json.data;
          this.drawScoreChart(); // only redraw score chart
        }
      } catch (err) {
        console.error("Failed to load scores:", err);
      }
    },

    drawCharts() {
      this.drawScoreChart();
      this.drawBatchChart();
    },
    async loadScoreByTypes() {
      try {
        let query = [];
        if (this.filters.fromDate)
          query.push(`startDate=${this.filters.fromDate}`);
        if (this.filters.toDate) query.push(`endDate=${this.filters.toDate}`);
        if (this.filters.batchId) query.push(`batchId=${this.filters.batchId}`);
        const queryString = query.length ? `?${query.join("&")}` : "";

        const res = await api.get(
          `/Instructors/assessments/score-by-types${queryString}`
        );
        const json = await res.json();

        if (json.status) {
          this.scoreByTypes = json.data;
          console.log("Score by types data:", this.scoreByTypes);
          this.drawScoreByTypesChart();
        }
      } catch (err) {
        console.error("Failed to load score by question type", err);
      }
    },
    drawScoreByTypesChart() {
      const ctx = document.getElementById("scoreByTypesChart");

      if (this.scoreByTypesChart) this.scoreByTypesChart.destroy();

      if (!this.scoreByTypes.length) {
        ctx.parentElement.innerHTML += `<p class="text-gray-500 text-sm mt-2">No data available</p>`;
        return;
      }

      this.scoreByTypesChart = new Chart(ctx, {
        type: "bar",
        data: {
          labels: this.scoreByTypes.map((x) => x.type),
          datasets: [
            {
              label: "Average Score (%)",
              data: this.scoreByTypes.map((x) => x.averageScore),
              backgroundColor: "#3b82f6",
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
              max: 100,
            },
          },
          plugins: {
            tooltip: {
              callbacks: {
                label: (context) => {
                  const score = context.raw;
                  const index = context.dataIndex;
                  const attempts = this.scoreByTypes[index]?.attemptCount || 0;
                  return [
                    `Avg Score: ${score.toFixed(1)}%`,
                    `Attempts: ${attempts}`,
                  ];
                },
              },
            },
          },
        },
      });
    },

    drawScoreChart() {
      const ctx = document.getElementById("assessmentScoreChart");
      if (this.scoreChart) this.scoreChart.destroy();

      this.scoreChart = new Chart(ctx, {
        type: "bar",
        data: {
          labels: this.assessmentScoreTrends.map((x) => x.assessmentTitle),
          datasets: [
            {
              label: "Average Score (%)",
              data: this.assessmentScoreTrends.map((x) => x.averageScore),
              backgroundColor: "#3b82f6",
            },
          ],
        },
      });
    },

    drawBatchChart() {
      new Chart(document.getElementById("studentBatchChart"), {
        type: "pie",
        data: {
          labels: this.batches.map((b) => b.batchName),
          datasets: [
            {
              data: this.batches.map((b) => b.studentCount),
              backgroundColor: ["#3b82f6", "#f59e0b", "#10b981"],
            },
          ],
        },
      });
    },

    logOut,
  };
};
