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

      try {
        const scoreTrendRes = await api.get("/Assessments/assessment-scores");
        const scoreTrend = await scoreTrendRes.json();
        if (scoreTrend.status) this.assessmentScoreTrends = scoreTrend.data;
      } catch (error) {
        console.error("Failed to load assessment score trends:", error);
      }

      this.drawCharts();
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
