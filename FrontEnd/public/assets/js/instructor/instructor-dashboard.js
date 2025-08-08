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

    async init() {
      await this.loadLayout();
      await this.loadData();
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

      await Promise.all([
        loadComponent("sidebar-placeholder", sidebar),
        loadComponent("navbar-placeholder", navbar),
      ]);
    },

    async loadData() {
      try {
        const [metrics, batchData, recent, scoreTrend] = await Promise.all([
          api
            .get("/Dashboard/instructor/metrics/overview")
            .then((r) => r.json()),
          api.get("/Dashboard/batch-distribution").then((r) => r.json()),
          api.get("/Instructors/assessment/recents").then((r) => r.json()),
          api.get("/Assessments/assessment-scores").then((r) => r.json()),
        ]);

        if (metrics.status) this.summary = metrics.data;
        if (batchData.status) this.batches = batchData.data;
        if (recent.status) this.recentAssessments = recent.data;
        if (scoreTrend.status) this.assessmentScoreTrends = scoreTrend.data;

        this.drawCharts();
      } catch (error) {
        console.error("Error loading dashboard data:", error);
        Swal.fire(
          "Error",
          "Failed to load data. Please try again later.",
          "error"
        );
      }
    },

    drawCharts() {
      // Assessment Scores Chart
      new Chart(document.getElementById("assessmentScoreChart"), {
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

      // Student Batch Distribution Chart
      new Chart(document.getElementById("studentBatchChart"), {
        type: "pie",
        data: {
          labels: this.batches.map((b) => b.name),
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
