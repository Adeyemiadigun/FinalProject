function batchPage() {
  return {
    showCreateModal: false,
    selectedBatchId: "",
    batchOptions: [],
    batchList: [],
    newBatch: { name: "", number: "" },
    metrics: null,
    trendChart: null,
    difficultyChart: null,

    async init() {
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");
      await this.fetchBatchOptions();
      await this.fetchBatchList();
      await this.fetchMetrics();
      this.drawCharts(); // Dummy for now or add real endpoint if exists
    },

    async fetchBatchOptions() {
      const token = localStorage.getItem("token");
      const res = await fetch("/api/v1/admin/batches/all", {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      this.batchOptions = data.data;
    },

    async fetchBatchList() {
      const token = localStorage.getItem("token");
      const res = await fetch(
        `/api/v1/admin/batches/analytics?pageNumber=1&pageSize=10`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
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
      const token = localStorage.getItem("token");
      const batchIdParam = this.selectedBatchId
        ? `?batchId=${this.selectedBatchId}`
        : "";
      const res = await fetch(`/api/v1/admin/batch/analytics${batchIdParam}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      this.metrics = data.data;
    },

    async createBatch() {
      const token = localStorage.getItem("token");
      const payload = {
        name: this.newBatch.name,
        batchNumber: parseInt(this.newBatch.number),
        startDate: new Date().toISOString(),
        endDate: null,
      };

      const res = await fetch("/api/v1/admin/batches", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });

      if (res.ok) {
        this.showCreateModal = false;
        this.newBatch = { name: "", number: "" };
        await this.fetchBatchOptions();
        await this.fetchBatchList();
      } else {
        alert("Failed to create batch.");
      }
    },

    drawCharts() {
      this.trendChart = new Chart(document.getElementById("trendChart"), {
        type: "line",
        data: {
          labels: ["Week 1", "Week 2", "Week 3", "Week 4"],
          datasets: [
            {
              label: "Avg Score",
              data: [70, 75, 74, 78],
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
                data: [35, 45, 20],
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

async function loadComponent(id, path) {
  const res = await fetch(path);
  document.getElementById(id).innerHTML = await res.text();
}
