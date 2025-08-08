// public/assets/js/admin/assessments.js
import { api, loadComponent, logOut } from "../shared/utils.js";

window.assessmentsPage = function () {
  return {
    // Data properties
    assessments: [],
    metrics: [],
    batches: [],
    instructors: [],
    charts: {
      scoreTrendsChart: null,
      createdOverTimeChart: null,
    },
    chartConfigs: {
      scoreTrendsChart: null,
      createdOverTimeChart: null,
    },

    // State properties
    filters: {
      instructorId: "",
      batchId: "",
      month: new Date().getMonth() + 1, // 1-based month
    },
    isLoading: {
      initial: true,
      metrics: true,
      charts: true,
      assessments: true,
    },

    // Initialization
    async init() {
      await this.loadLayout();
      await this.loadFilters();
      await this.loadData();
      this.isLoading.initial = false;
    },

    async loadLayout() {
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");
    },

    async loadFilters() {
      try {
        const [batchesRes, instructorsRes] = await Promise.all([
          api.get("/Batches/all"),
          api.get("/Instructors"),
        ]);

        if (!batchesRes.ok || !instructorsRes.ok) {
          throw new Error("Failed to load filter options.");
        }

        const batchesData = await batchesRes.json();
        const instructorsData = await instructorsRes.json();

        this.batches = batchesData.data;
        this.instructors = instructorsData.data.map((i) => ({
          id: i.id,
          name: i.fullName,
        }));
      } catch (error) {
        console.error("Filter loading error:", error);
        Swal.fire({
          icon: "error",
          title: "Oops...",
          text: "Could not load filter options. The page may not function correctly.",
        });
      }
    },

    async loadData() {
      this.isLoading.metrics = true;
      this.isLoading.charts = true;
      this.isLoading.assessments = true;

      try {
        const params = new URLSearchParams(this.filters).toString();

        // Fetch all data in parallel
        const [metricsRes, assessmentsRes, scoreTrendRes, createdTrendRes] =
          await Promise.all([
            api.get(`/dashboard/admin/assessments/metrics?${params}`),
            api.get(`/Assessments/recents`),
            api.get(
              `/Dashboard/admin/analytics/assessments/score-trends?instructorId=${this.filters.instructorId}&batchId=${this.filters.batchId}&month=${this.filters.month}`
            ),
            api.get(
              `/Dashboard/admin/analytics/assessments/created-trend?instructorId=${this.filters.instructorId}&batchId=${this.filters.batchId}&month=${this.filters.month}`
            ),
          ]);

        if (
          !metricsRes.ok ||
          !assessmentsRes.ok ||
          !scoreTrendRes.ok ||
          !createdTrendRes.ok
        ) {
          throw new Error("Failed to fetch assessment data.");
        }

        const metricsData = await metricsRes.json();
        const assessmentsData = await assessmentsRes.json();
        const scoreTrendData = await scoreTrendRes.json();
        const createdTrendData = await createdTrendRes.json();

        // Set metrics
        const metricsSource = metricsData.data;
        this.metrics = [
          { label: "Total Assessments", value: metricsSource.totalAssessments },
          {
            label: "Active Assessments",
            value: metricsSource.activeAssessments,
          },
          { label: "Average Score", value: `${metricsSource.averageScore}%` },
          { label: "Pass Rate", value: `${metricsSource.passRate}%` },
          {
            label: "Completion Rate",
            value: `${metricsSource.completionRate}%`,
          },
        ];

        // Process assessments
        this.assessments = assessmentsData.data.map((a) => ({
          ...a,
          statusClass: this.getStatusClass(a.status),
        }));

        // Store chart configs
        this.chartConfigs.scoreTrendsChart = {
          type: "line",
          labels: scoreTrendData.data.map((d) => d.label || d.Label),
          datasets: [
            {
              label: "Avg. Score",
              data: scoreTrendData.data.map(
                (d) => d.averageScore ?? d.AverageScore
              ),
              borderColor: "#4f46e5",
              tension: 0.4,
            },
          ],
        };
        this.chartConfigs.createdOverTimeChart = {
          type: "line",
          labels: createdTrendData.data.map((d) => d.label || d.Label),
          datasets: [
            {
              label: "Assessments Created",
              data: createdTrendData.data.map((d) => d.count ?? d.Count),
              borderColor: "#10b981",
              tension: 0.4,
            },
          ],
        };
      } catch (error) {
        console.error("Data loading error:", error);
        Swal.fire({
          icon: "error",
          title: "Failed to Load Data",
          text: "Could not retrieve assessment data. Please try again later.",
        });
      } finally {
        // Stop all loaders
        this.isLoading.metrics = false;
        this.isLoading.charts = false;
        this.isLoading.assessments = false;

        await this.$nextTick();

        // Draw charts
        if (this.chartConfigs.scoreTrendsChart) {
          this.drawChart(
            "scoreTrendsChart",
            this.chartConfigs.scoreTrendsChart
          );
        }
        if (this.chartConfigs.createdOverTimeChart) {
          this.drawChart(
            "createdOverTimeChart",
            this.chartConfigs.createdOverTimeChart
          );
        }
      }
    },

    getStatusClass(status) {
      switch (status?.toLowerCase()) {
        case "upcoming":
          return "bg-blue-100 text-blue-800";
        case "active":
          return "bg-green-100 text-green-800";
        case "completed":
          return "bg-gray-100 text-gray-800";
        default:
          return "bg-yellow-100 text-yellow-800";
      }
    },

    drawChart(canvasId, chartData) {
      const ctx = document.getElementById(canvasId)?.getContext("2d");
      if (!ctx) return;

      // Destroy existing chart instance if it exists
      if (this.charts[canvasId]) {
        this.charts[canvasId].destroy();
      }

      this.charts[canvasId] = new Chart(ctx, {
        type: chartData.type,
        data: {
          labels: chartData.labels,
          datasets: chartData.datasets,
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { beginAtZero: true } },
        },
      });
    },

    logOut,
  };
};
