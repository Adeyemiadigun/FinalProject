function instructorDashboard() {
  return {
    summary: {
      totalAssessments: 0,
      totalStudents: 0,
      avgScore: 0,
      passRate: 0,
    },
    batches: [],
    recentAssessments: [],

    async loadData() {
      const token = localStorage.getItem("token");
      const headers = { Authorization: `Bearer ${token}` };

      this.summary = await fetch("/api/v1/instructor/dashboard/summary", {
        headers,
      }).then((r) => r.json());
      this.batches = await fetch(
        "/api/v1/instructor/dashboard/batch-distribution",
        { headers }
      ).then((r) => r.json());
      this.recentAssessments = await fetch(
        "/api/v1/instructor/dashboard/recent-assessments",
        { headers }
      ).then((r) => r.json());

      this.drawCharts();
    },

    drawCharts() {
      new Chart(document.getElementById("assessmentScoreChart"), {
        type: "bar",
        data: {
          labels: ["OOP Basics", "LINQ Challenge", "Entity Framework"],
          datasets: [
            {
              label: "Average Score (%)",
              data: [74, 69, 78],
              backgroundColor: "#3b82f6",
            },
          ],
        },
      });

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
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
