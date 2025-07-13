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
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },

    async fetchBatchOptions() {
      const token = localStorage.getItem("accessToken");
      const res = await fetch("http://localhost:5162/api/v1/Batches/all", {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      this.batchOptions = data.data;
    },

    async fetchBatchList() {
      const token = localStorage.getItem("accessToken");
      const res = await fetch(
        `http://localhost:5162/api/v1/Dashboard/admin/batches/analytics?PageSize=10&CurrentPage=1`,
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
      const token = localStorage.getItem("accessToken");
      const batchIdParam = this.selectedBatchId
        ? `?batchId=${this.selectedBatchId}`
        : "";
      const res = await fetch(
        `http://localhost:5162/api/v1/Dashboard/admin/batch/analytics${batchIdParam}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      const data = await res.json();
      this.metrics = data.data;
    },
    async fetchTrend() {
      const token = localStorage.getItem("accessToken");
      const batchIdParam = this.selectedBatchId
        ? `?batchId=${this.selectedBatchId}`
        : "";
      const res = await fetch(
        `http://localhost:5162/api/v1/Dashboard/admin/batches/performance-trend${batchIdParam}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      const data = await res.json();
      this.trendChart = data.data;
    },

    async createBatch() {
      const token = localStorage.getItem("accessToken");
      const payload = {
        name: this.newBatch.name,
        batchNumber: parseInt(this.newBatch.number),
        startDate: new Date().toISOString(),
        endDate: null,
      };

      const res = await fetch("http://localhost:5162/api/v1/Batches", {
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

    drawCharts(data) {
      this.trendChart = new Chart(document.getElementById("trendChart"), {
        type: "line",
        data: {
          labels: data.map((d) => d.label),
          datasets: [
            {
              label: "Avg Score",
              data: data.map((d) => d.scores),
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
