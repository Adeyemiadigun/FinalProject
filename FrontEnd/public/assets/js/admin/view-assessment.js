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
    isInitialized: false,

    // State properties
    filters: {
      instructorId: "",
      batchId: "",
      date:null, // 1-based month
    },
    isLoading: {
      initial: true,
      metrics: true,
      charts: true,
      assessments: true,
    },

    // Initialization
    async init() {
       if (this.isInitialized) {
         console.log("Component already initialized, skipping...");
         return;
       }

       this.isInitialized = true;
       console.log("Initializing assessments page...");
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

      // Destroy existing charts before loading new data
      this.destroyCharts();

      const params = new URLSearchParams(this.filters).toString();

      try {
        // METRICS
        await this.loadMetrics(params);

        // CHARTS - Load chart data
        await Promise.all([this.loadScoreTrends(), this.loadCreatedTrends()]);
        await this.loadRecentAssessments(); 
      } catch (error) {
        console.error("loadData overall error:", error);
      } finally {
        this.isLoading.metrics = false;
        this.isLoading.assessments = false;
        this.isLoading.charts = false;

        // Render charts after a delay to ensure DOM is ready
        await this.$nextTick();
        setTimeout(() => {
          this.renderCharts();
        }, 150);
      }
    },

    async loadMetrics(params) {
      try {
        const res = await api.get(
          `/dashboard/admin/assessments/metrics?${params}`
        );
        if (!res.ok) throw new Error("Failed to fetch metrics");
        const data = await res.json();
        const m = data.data;
        this.metrics = [
          { label: "Total Assessments", value: m.totalAssessments },
          { label: "Active Assessments", value: m.activeAssessments },
          { label: "Average Score", value: `${m.averageScore}%` },
          { label: "Pass Rate", value: `${m.passRate}%` },
          { label: "Completion Rate", value: `${m.completionRate}%` },
        ];
      } catch (err) {
        console.warn("Metrics error:", err);
        this.metrics = [
          { label: "Total Assessments", value: 0 },
          { label: "Active Assessments", value: 0 },
          { label: "Average Score", value: "0%" },
          { label: "Pass Rate", value: "0%" },
          { label: "Completion Rate", value: "0%" },
        ];
      }
    },
   async loadTrend()
    {
       this.destroyCharts();  // Ensure old charts are removed
      await this.loadScoreTrends();
      await this.loadCreatedTrends();
      await this.$nextTick();
      setTimeout(() => {
        this.renderCharts();
      }, 100);
    },


    async loadScoreTrends() {
      try {
        console.log("entered here")
        const monthValue = this.filters.date;
        console.log("Month value:", monthValue);
        let url =`/Dashboard/admin/analytics/assessments/score-trends?instructorId=${this.filters.instructorId}&batchId=${this.filters.batchId}`
          if (monthValue) {
            const selectedDate = new Date(monthValue + "-01");
            const isoDate = selectedDate.toISOString();
            console.log("ISO Date:", isoDate);
            url += ` &date=${isoDate}`;
          }
          console.log("URL for score trends:", url);
        const res = await api.get(url
        );
        console.log("Response:", res);
        if (!res.ok) throw new Error("Failed to fetch score trends");
        const data = await res.json();
        console.log("Score trends data:", data);
        if (data.data && data.data.length > 0) {
          this.chartConfigs.scoreTrendsChart = {
            type: "line",
            labels: data.data.map((d) => d.label || d.Label),
            datasets: [
              {
                label: "Avg. Score",
                data: data.data.map((d) => d.averageScore ?? d.AverageScore),
                borderColor: "#4f46e5",
                backgroundColor: "rgba(79, 70, 229, 0.1)",
                tension: 0.4,
                fill: false,
                pointBackgroundColor: "#4f46e5",
                pointBorderColor: "#4f46e5",
                pointRadius: 4,
              },
            ],
          };
        } else {
          this.chartConfigs.scoreTrendsChart = null;
        }
      } catch (err) {
        console.warn("Score trends error:", err);
        this.chartConfigs.scoreTrendsChart = null;
      }
    },
    async loadRecentAssessments() {
      try{
        const res = await api.get(`/Assessments/recents`);
        if (!res.ok) throw new Error("Failed to fetch recent assessments");
        const data = await res.json();
        console.log("Recent assessments data:", data);
        this.assessments = data.data || [];
      }
      catch{}
    },

    async loadCreatedTrends() {
      try {
        const monthValue = this.filters.date;
        let url = `/Dashboard/admin/analytics/assessments/created-trend?instructorId=${this.filters.instructorId}&batchId=${this.filters.batchId}`;
         if (monthValue) {
           const selectedDate = new Date(monthValue + "-01");
           const isoDate = selectedDate.toISOString();
           url += `&date=${isoDate}`;
         }
         const res = await api.get(url);
        if (!res.ok) throw new Error("Failed to fetch created trends");
        const data = await res.json();

        if (data.data && data.data.length > 0) {
          this.chartConfigs.createdOverTimeChart = {
            type: "line",
            labels: data.data.map((d) => d.label || d.Label),
            datasets: [
              {
                label: "Assessments Created",
                data: data.data.map((d) => d.count ?? d.Count),
                borderColor: "#10b981",
                backgroundColor: "rgba(16, 185, 129, 0.1)",
                tension: 0.4,
                fill: false,
                pointBackgroundColor: "#10b981",
                pointBorderColor: "#10b981",
                pointRadius: 4,
              },
            ],
          };
        } else {
          this.chartConfigs.createdOverTimeChart = null;
        }
      } catch (err) {
        console.warn("Created trends error:", err);
        this.chartConfigs.createdOverTimeChart = null;
      }
    },

    destroyCharts() {
      Object.keys(this.charts).forEach((chartId) => {
        if (this.charts[chartId]) {
          try {
            this.charts[chartId].destroy();
          } catch (error) {
            console.warn(`Error destroying chart ${chartId}:`, error);
          }
          this.charts[chartId] = null;
        }
      });
    },

    renderCharts() {
      if (this.chartConfigs.scoreTrendsChart) {
        this.drawChart("scoreTrendsChart", this.chartConfigs.scoreTrendsChart);
      }
      if (this.chartConfigs.createdOverTimeChart) {
        this.drawChart(
          "createdOverTimeChart",
          this.chartConfigs.createdOverTimeChart
        );
      }
    },

    drawChart(canvasId, chartData) {
      // Wait for element to be available and visible
      const maxAttempts = 10;
      let attempts = 0;

      const attemptDraw = () => {
        attempts++;
        const canvas = document.getElementById(canvasId);

        if (!canvas) {
          console.warn(
            `Canvas element #${canvasId} not found. Attempt ${attempts}/${maxAttempts}`
          );
          if (attempts < maxAttempts) {
            setTimeout(attemptDraw, 100);
          }
          return;
        }

        // Check if canvas is visible and has dimensions
        if (
          canvas.offsetParent === null ||
          canvas.clientWidth === 0 ||
          canvas.clientHeight === 0
        ) {
          console.warn(
            `Canvas #${canvasId} not visible or has no dimensions. Attempt ${attempts}/${maxAttempts}`
          );
          if (attempts < maxAttempts) {
            setTimeout(attemptDraw, 100);
          }
          return;
        }

        try {
          const ctx = canvas.getContext("2d");

          if (!ctx) {
            console.error(`Cannot get 2D context for canvas #${canvasId}`);
            return;
          }

          // Destroy existing chart if it exists
          if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
            this.charts[canvasId] = null;
          }

          // Create new chart
          this.charts[canvasId] = new Chart(ctx, {
            type: chartData.type,
            data: {
              labels: chartData.labels,
              datasets: chartData.datasets,
            },
            options: {
              responsive: true,
              maintainAspectRatio: false,
              interaction: {
                intersect: false,
                mode: "index",
              },
              plugins: {
                legend: {
                  display: true,
                  position: "top",
                  labels: {
                    padding: 20,
                    usePointStyle: true,
                  },
                },
                tooltip: {
                  backgroundColor: "rgba(0, 0, 0, 0.8)",
                  titleColor: "white",
                  bodyColor: "white",
                  borderColor: "rgba(255, 255, 255, 0.1)",
                  borderWidth: 1,
                },
              },
              scales: {
                y: {
                  beginAtZero: true,
                  grid: {
                    color: "rgba(0, 0, 0, 0.1)",
                    drawBorder: false,
                  },
                  ticks: {
                    color: "#6b7280",
                  },
                },
                x: {
                  grid: {
                    color: "rgba(0, 0, 0, 0.1)",
                    drawBorder: false,
                  },
                  ticks: {
                    color: "#6b7280",
                  },
                },
              },
              elements: {
                line: {
                  borderWidth: 2,
                },
                point: {
                  hoverRadius: 6,
                },
              },
            },
          });

          console.log(`Chart ${canvasId} created successfully`);
        } catch (error) {
          console.error(`Error creating chart ${canvasId}:`, error);
          if (attempts < maxAttempts) {
            setTimeout(attemptDraw, 200);
          }
        }
      };

      attemptDraw();
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

    logOut,
  };
};
