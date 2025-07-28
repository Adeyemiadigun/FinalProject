function assessmentsPage() {
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
      // Assuming loadComponent is a global helper
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");
    },

    async loadFilters() {
      try {
        const token = localStorage.getItem("accessToken");
        const headers = { Authorization: `Bearer ${token}` };

        const [batchesRes, instructorsRes] = await Promise.all([
          fetch("https://localhost:7157/api/v1/Batches/all", { headers }),
          fetch("https://localhost:7157/api/v1/Instructors", { headers }),
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
        const token = localStorage.getItem("accessToken");
        const headers = { Authorization: `Bearer ${token}` };
        const params = new URLSearchParams(this.filters).toString();

        // Fetch all data in parallel
        const [metricsRes, assessmentsRes, scoreTrendRes, createdTrendRes] =
          await Promise.all([
            fetch(
              `https://localhost:7157/api/v1/dashboard/admin/assessments/metrics?${params}`,
              { headers }
            ),
            fetch(
              `https://localhost:7157/api/v1/Assessments/recents?${params}`,
              { headers }
            ),
            fetch(
              `https://localhost:7157/api/v1/Dashboard/admin/analytics/assessments/score-trends?instructorId=${this.filters.instructorId}&batchId=${this.filters.batchId}&month=${this.filters.month}`,
              { headers }
            ),
            fetch(
              `https://localhost:7157/api/v1/Dashboard/admin/analytics/assessments/created-trend?instructorId=${this.filters.instructorId}&batchId=${this.filters.batchId}&month=${this.filters.month}`,
              { headers }
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

        // Process and set data
        const metricsSource = metricsData.data;
        this.metrics = [
            { label: 'Total Assessments', value: metricsSource.totalAssessments },
            { label: 'Active Assessments', value: metricsSource.activeAssessments },
            { label: 'Average Score', value: `${metricsSource.averageScore}%` },
            { label: 'Pass Rate', value: `${metricsSource.passRate}%` },
            { label: 'Completion Rate', value: `${metricsSource.completionRate}%` }
        ];
        this.assessments = assessmentsData.data.map((a) => ({
          ...a,
          statusClass: this.getStatusClass(a.status),
        }));
        console.log(this.assessments)

        // Store chart configurations to be drawn after the canvas is visible
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
        // Ensure all loaders are turned off
        this.isLoading.metrics = false;
        this.isLoading.charts = false;
        this.isLoading.assessments = false;

        // Wait for Alpine to update the DOM and make the canvas visible
        await this.$nextTick();

        // Now, draw the charts on the visible canvases
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
      // Use optional chaining (?.) to prevent error if status is null or undefined
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
  };
}

// This global helper function is assumed to exist from other files like dashboard.js
async function loadComponent(id, path) {
  try {
    const res = await fetch(path);
    if (!res.ok) throw new Error(`Failed to load component: ${path}`);
    const html = await res.text();
    const element = document.getElementById(id);
    if (element) {
      element.innerHTML = html;
    }
  } catch (error) {
    console.error(error);
  }
}
