import { api, loadComponent, logOut } from "../shared/utils.js";

window.adminDashboard = function () {
  return {
    statCards: [],
    topStudents: [],
    lowestStudents: [],
    isLoading: { metrics: true, charts: true, students: true },
    filter: {
      date: null,
    },
    chartInstances: {},

    async initDashboard() {
      try {
        await loadComponent(
          "sidebar-placeholder",
          "../components/sidebar.html"
        );
        await loadComponent("navbar-placeholder", "../components/nav.html");

        // --- Step 1: Fetch metrics and students
        const metricsRes = await api.get("/dashboard/admin/metrics/overview");
        if (!metricsRes.ok)
          throw new Error("Failed to fetch dashboard metrics.");
        const metrics = await metricsRes.json();

        this.statCards = [
          {
            label: "Assessments",
            value: metrics.data.totalAssessments,
            bg: "bg-blue-500",
            icon: '<i class="fas fa-file-alt w-6 h-6"></i>', // file-like icon
          },
          {
            label: "Active Assessments",
            value: metrics.data.activeAssessments,
            bg: "bg-green-500",
            icon: '<i class="fas fa-bolt w-6 h-6"></i>', // active/power
          },
          {
            label: "Students",
            value: metrics.data.totalStudents,
            bg: "bg-yellow-500",
            icon: '<i class="fas fa-user-graduate w-6 h-6"></i>', // student icon
          },
          {
            label: "Avg Score",
            value: `${metrics.data.averageScore}%`,
            bg: "bg-purple-500",
            icon: '<i class="fas fa-chart-line w-6 h-6"></i>', // performance trend
          },
          {
            label: "Completion",
            value: `${metrics.data.completionRate}%`,
            bg: "bg-indigo-500",
            icon: '<i class="fas fa-check-circle w-6 h-6"></i>', // completion
          },
          {
            label: "Batches",
            value: metrics.data.totalBatches,
            bg: "bg-pink-500",
            icon: '<i class="fas fa-layer-group w-6 h-6"></i>', // grouped/layered
          },
        ];

        this.topStudents = metrics.data.topStudents;
        this.lowestStudents = metrics.data.lowestStudents;

        this.isLoading.metrics = false;
        this.isLoading.students = false;

        // --- Step 2: Fetch chart data in parallel
        const [topRes, lowRes, batchRes] = await Promise.all([
          api.get("/dashboard/admin/analytics/assessments/top-performing"),
          api.get("/dashboard/admin/analytics/assessments/lowest-performing"),
          api.get("/dashboard/batch-distribution"),
        ]);

        if ([topRes, lowRes, batchRes].some((res) => !res.ok)) {
          throw new Error("Failed to fetch chart data.");
        }

        const top = await topRes.json();
        const low = await lowRes.json();
        const batchData = await batchRes.json();
        console.log("Top assessments:", top);
        console.log("Lowest assessments:", low);
        this.drawChart("topAssessmentChart", top.data);
        this.drawChart("lowAssessmentChart", low.data);
        this.drawChart("batchChart", batchData.data);

        this.isLoading.charts = false;
      } catch (err) {
        console.error(err);
        Swal.fire({
          icon: "error",
          title: "Dashboard Error",
          text: "Could not load dashboard data. Please check your connection and try again.",
        });
        this.isLoading = { metrics: false, charts: false, students: false };
      }
    },
    async loadTrend() {
      console.log("Loading trend data with filter:", this.filter.date);
      if (!this.filter.date) return;

      this.isLoading.charts = true;
      await this.$nextTick(); // Ensure skeletons are shown before fetch starts

      const [year, month] = this.filter.date.split("-");
      const parsedYear = parseInt(year);
      const parsedMonth = parseInt(month);

      try {
        const [topRes, lowRes] = await Promise.all([
          api.get(
            `/dashboard/admin/analytics/assessments/top-performing?month=${parsedMonth}&year=${parsedYear}`
          ),
          api.get(
            `/dashboard/admin/analytics/assessments/lowest-performing?month=${parsedMonth}&year=${parsedYear}`
          ),
        ]);

        const top = await topRes.json();
        const low = await lowRes.json();

        console.log("Top assessments trend:", top);
        console.log("Lowest assessments trend:", low);

        // Destroy existing charts first (safe while hidden)
        if (this.chartInstances["topAssessmentChart"]) {
          this.chartInstances["topAssessmentChart"].destroy();
          this.chartInstances["topAssessmentChart"] = null;
        }
        if (this.chartInstances["lowAssessmentChart"]) {
          this.chartInstances["lowAssessmentChart"].destroy();
          this.chartInstances["lowAssessmentChart"] = null;
        }

        // Show canvases
        this.isLoading.charts = false;
        await this.$nextTick(); // Wait for DOM update so canvases are visible

        // Now draw on visible canvases
        this.drawChart("topAssessmentChart", top.data);
        this.drawChart("lowAssessmentChart", low.data);
      } catch (error) {
        console.error("Trend loading error:", error);
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "Could not load assessment trend data.",
        });
        this.isLoading.charts = false; // Reset loading on error
      }
    },
    logOut,

    drawChart(id, data) {
      const ctx = document.getElementById(id).getContext("2d");

      // ✅ Destroy existing chart instance if exists
      if (this.chartInstances[id]) {
        this.chartInstances[id].destroy();
      }

      // ✅ Create and store new chart instance
      this.chartInstances[id] = new Chart(ctx, {
        type: "bar",
        data: {
          labels: data.map((d) => d.assessmentTitle || d.batchName),
          datasets: [
            {
              label: "Average Score",
              data: data.map((d) => d.averageScore ?? d.studentCount),
              backgroundColor: "#3b82f6",
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            y: { beginAtZero: true },
          },
        },
      });
    },
  };
};
