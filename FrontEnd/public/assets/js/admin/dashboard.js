function adminDashboard() {
  return {
    statCards: [],
    topStudents: [],
    lowestStudents: [],

    async initDashboard() {
      const token = localStorage.getItem("token");
      const headers = { headers: { Authorization: `Bearer ${token}` } };

      try {
        
        loadComponent("sidebar-placeholder", "../components/sidebar.html");
        loadComponent("navbar-placeholder", "../components/nav.html");
        const [metricsRes, topRes, lowRes, batchRes] = await Promise.all([
          fetch("/api/v1/admin/metrics/overview", headers),
          fetch("/api/v1/admin/analytics/assessments/top-performing", headers),
          fetch(
            "/api/v1/admin/analytics/assessments/lowest-performing",
            headers
          ),
          fetch("/api/v1/admin/analytics/students-per-batch", headers),
        ]);

        if (
          [metricsRes, topRes, lowRes, batchRes].some(
            (res) => res.status === 401
          )
        ) {
          await window.refreshToken(); 
          return this.initDashboard();
        }

        const metrics = await metricsRes.json();
        const top = await topRes.json();
        const low = await lowRes.json();
        const batchData = await batchRes.json();

        this.statCards = [
          {
            label: "Assessments",
            value: metrics.data.totalAssessments,
            bg: "bg-blue-500",
            icon: '<path d="M5 13l4 4L19 7" />',
          },
          {
            label: "Active",
            value: metrics.data.activeAssessments,
            bg: "bg-green-500",
            icon: '<path d="M12 4v16m8-8H4" />',
          },
          {
            label: "Students",
            value: metrics.data.totalStudents,
            bg: "bg-yellow-500",
            icon: '<path d="M3 10h18M3 6h18M3 14h18M3 18h18" />',
          },
          {
            label: "Avg Score",
            value: `${metrics.data.averageScore}%`,
            bg: "bg-purple-500",
            icon: '<path d="M5 13l4 4L19 7" />',
          },
          {
            label: "Completion",
            value: `${metrics.data.completionRate}%`,
            bg: "bg-indigo-500",
            icon: '<path d="M12 4v16m8-8H4" />',
          },
          {
            label: "Batches",
            value: metrics.data.totalBatches,
            bg: "bg-pink-500",
            icon: '<path d="M3 10h18M3 6h18" />',
          },
        ];

        this.topStudents = metrics.data.topStudents;
        this.lowestStudents = metrics.data.lowestStudents;
        this.drawChart("topAssessmentChart", top.data);
        this.drawChart("lowAssessmentChart", low.data);
        this.drawChart("batchChart", batchData.data);
      } catch (err) {
        console.error(err);
        alert("Dashboard failed to load. Please try again later.");
      }
    },

    drawChart(id, data) {
      const ctx = document.getElementById(id).getContext("2d");
      new Chart(ctx, {
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
        options: { responsive: true, scales: { y: { beginAtZero: true } } },
      });
    },
  };
}
async function loadComponent(id, path) {
  const res = await fetch(path);
  document.getElementById(id).innerHTML = await res.text();
}