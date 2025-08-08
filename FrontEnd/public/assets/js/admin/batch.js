import { api, loadComponent, logOut } from "../shared/utils.js";

 window.batchPage = function () {
  return {
    showCreateModal: false,
    selectedBatchId: "",
    batchOptions: [],
    batchList: [],
    newBatch: { name: "", number: "" },
    metrics: null,
    trendChart: null,
    difficultyChart: null,
    trendData: null,
    sidebarOpen: true,

    async init() {
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");

      await this.fetchBatchOptions();
      await this.fetchBatchList();
      await this.fetchMetrics();
      await this.fetchTrend();
      this.drawCharts();
    },

    logOut, // Use global logout function

    async fetchBatchOptions() {
      const res = await api.get("/Batches/all");
      const data = await res.json();
      this.batchOptions = data.data ?? [];
    },

    async fetchBatchList() {
      const res = await api.get(
        `/Dashboard/admin/batches/analytics?PageSize=10&CurrentPage=1`
      );
      const json = await res.json();
      this.batchList = json.data.items.map((batch) => ({
        id: batch.id,
        name: batch.name,
        studentCount: batch.studentCount,
        assessmentCount: batch.assessments ?? 0,
        passRate: batch.passRate ?? 0,
      }));
    },

    async fetchMetrics() {
      const batchIdParam = this.selectedBatchId
        ? `?batchId=${this.selectedBatchId}`
        : "";
      const res = await api.get(
        `/Dashboard/admin/batch/analytics${batchIdParam}`
      );
      const data = await res.json();
      this.metrics = data.data;
    },

    async fetchTrend() {
      const batchIdParam = this.selectedBatchId
        ? `?batchId=${this.selectedBatchId}`
        : "";
      const res = await api.get(
        `/Dashboard/admin/batches/performance-trend${batchIdParam}`
      );
      const data = await res.json();
      this.trendData = data.data;
      console.log(this.trendData.labels);
    },

    async createBatch() {
      const payload = {
        name: this.newBatch.name,
        batchNumber: parseInt(this.newBatch.number),
        startDate: new Date().toISOString(),
        endDate: null,
      };

      const res = await api.post("/Batches", payload);

      if (res.ok) {
        this.showCreateModal = false;
        this.newBatch = { name: "", number: "" };
        await this.fetchBatchOptions();
        await this.fetchBatchList();
      } else {
        // This is usually handled by `api` automatically
        alert("Failed to create batch.");
      }
    },

    drawCharts() {
      this.trendChart = new Chart(document.getElementById("trendChart"), {
        type: "line",
        data: {
          labels: this.trendData.labels || [],
          datasets: [
            {
              label: "Avg Score",
              data: this.trendData.scores || [],
              fill: false,
              borderColor: "#3b82f6",
              tension: 0.4,
            },
          ],
        },
        options: {
          responsive: true,
          scales: { y: { beginAtZero: true, max: 100 } },
        },
      });

      this.difficultyChart = new Chart(
        document.getElementById("difficultyChart"),
        {
          type: "pie",
          data: {
            labels: ["Easy", "Medium", "Hard"],
            datasets: [
              {
                data: [35, 45, 20], // You can replace this with backend data
                backgroundColor: ["#10b981", "#f59e0b", "#ef4444"],
              },
            ],
          },
          options: { responsive: true },
        }
      );
    },
  };
}
