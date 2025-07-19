async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}

function assessmentDashboard() {
  return {
    selectedBatch: "",
    selectedInstructor: "",
    batches: [],
    instructors: [],
    sidebarOpen: true,
    async init() {
      const token = localStorage.getItem("accessToken");
      this.headers = { Authorization: `Bearer ${token}` };

      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");

      await this.loadDropdowns();
      await this.reloadData();
    },

    async loadDropdowns() {
      try {
        const batchRes = await fetch(
          "https://localhost:7157/api/v1/Batches/all",
          {
            headers: this.headers,
          }
        );
        let batch = await batchRes.json();
        this.batches = batch.data;

        const instructorRes = await fetch(
          "https://localhost:7157/api/v1/Instructors",
          {
            headers: this.headers,
          }
        );
        let data = await instructorRes.json();
        this.instructors = data.data;
      } catch (err) {
        console.error("Failed to load dropdown data:", err);
      }
    },

    async reloadData() {
      const query = new URLSearchParams();
      if (this.selectedInstructor)
        query.append("instructorId", this.selectedInstructor);
      if (this.selectedBatch) query.append("batchId", this.selectedBatch);

      try {
        const metricsRes = await fetch(
          `https://localhost:7157/api/v1/dashboard/admin/assessments/metrics?${query}`,
          {
            headers: this.headers,
          }
        );
        const metrics = await metricsRes.json();
        console.log(metrics);
        this.renderMetrics(metrics.data);
      } catch (err) {
        console.error("Failed to fetch metrics:", err);
      }

      try {
        const scoreRes = await fetch(
          `https://localhost:7157/api/v1/Dashboard/admin/analytics/assessments/score-trends?${query}`,
          {
            headers: this.headers,
          }
        );
        const scoreTrend = await scoreRes.json();
        this.renderScoreChart(scoreTrend.data);
      } catch (err) {
        console.error("Failed to fetch score trends:", err);
      }

      try {
        const createdRes = await fetch(
          `https://localhost:7157/api/v1/Dashboard/admin/analytics/assessments/created-trend?${query}`,
          {
            headers: this.headers,
          }
        );
        const createdTrend = await createdRes.json();
        this.renderCreatedChart(createdTrend.data);
      } catch (err) {
        console.error("Failed to fetch created trend:", err);
      }

      try {
        const recentsRes = await fetch(
          `https://localhost:7157/api/v1/Assessments/recents?${query}`,
          {
            headers: this.headers,
          }
        );
        const recents = await recentsRes.json();
        console.log(recents);
        this.renderAssessments(recents.data);
      } catch (err) {
        console.error("Failed to fetch recent assessments:", err);
      }
    },

    renderMetrics(metrics) {
      document.getElementById("metrics-cards").innerHTML = `
        <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
          <div class="bg-white p-4 rounded shadow">Total: ${metrics.totalAssessments}</div>
          <div class="bg-white p-4 rounded shadow">Active: ${metrics.activeAssessments}</div>
          <div class="bg-white p-4 rounded shadow">Avg Score: ${metrics.averageScore}%</div>
          <div class="bg-white p-4 rounded shadow">Pass Rate: ${metrics.passRate}%</div>
          <div class="bg-white p-4 rounded shadow">Completion: ${metrics.completionRate}%</div>
        </div>`;
    },
    renderScoreChart(scoreTrend) {
      new Chart(document.getElementById("scoreTrendsChart"), {
        type: "line",
        data: {
          labels: scoreTrend.map((d) => d.label),
          datasets: [
            {
              label: "Avg. Score",
              data: scoreTrend.map((d) => d.averageScore),
              borderColor: "#4f46e5",
              tension: 0.4,
            },
          ],
        },
        options: { responsive: true, scales: { y: { beginAtZero: true } } },
      });
    },

    renderCreatedChart(createdTrend) {
      new Chart(document.getElementById("createdOverTimeChart"), {
        type: "line",
        data: {
          labels: createdTrend.map((d) => d.label),
          datasets: [
            {
              label: "Assessments Created",
              data: createdTrend.map((d) => d.count),
              borderColor: "#10b981",
              tension: 0.4,
            },
          ],
        },
        options: { responsive: true, scales: { y: { beginAtZero: true } } },
      });
    },

    renderAssessments(data) {
      const tbody = document.getElementById("assessment-table-body");
      tbody.innerHTML = data
        .map(
          (x) => `
          <tr class="border-t">
            <td class="p-2">${x.title}</td>
            <td class="p-2">${x.type ?? "N/A"}</td>
            <td class="p-2">${x.instructorName ?? "N/A"}</td>
            <td class="p-2 text-green-600 font-medium">${x.status ?? "N/A"}</td>
            <td class="p-2 space-x-2">
              <a href="/public/assessments/assessment-details.html?assessmentId=${
                x.id
              }" class="text-blue-600 hover:underline">
                View
              </a>
            </td>
          </tr>`
        )
        .join("");
    },

    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
  };
}
