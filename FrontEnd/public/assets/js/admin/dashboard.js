function adminDashboard() {
  return {
    statCards: [],
    topStudents: [],
    lowestStudents: [],
    isLoading: {
      metrics: true,
      charts: true,
      students: true,
    },
    async initDashboard() {
      const token = localStorage.getItem("accessToken");
      const headers = {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      };

      try {
        loadComponent("sidebar-placeholder", "../components/sidebar.html");
        loadComponent("navbar-placeholder", "../components/nav.html");

        // --- Step 1: Fetch metrics and student data first for a fast initial render ---
        const metricsRes = await fetch(
          "https://localhost:7157/api/v1/dashboard/admin/metrics/overview",
          { method: "GET", headers: headers }
        );
        if (!metricsRes.ok) throw new Error("Failed to fetch dashboard metrics.");

        const metrics = await metricsRes.json();

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

        // Hide loaders for the first batch of content
        this.isLoading.metrics = false;
        this.isLoading.students = false;

        // --- Step 2: Fetch chart data in parallel ---
        const [topRes, lowRes, batchRes] = await Promise.all([
          fetch(
            "https://localhost:7157/api/v1/dashboard/admin/analytics/assessments/top-performing",
            { method: "GET", headers: headers }
          ),
          fetch(
            "https://localhost:7157/api/v1/dashboard/admin/analytics/assessments/lowest-performing",
            { method: "GET", headers: headers }
          ),
          fetch("https://localhost:7157/api/v1/dashboard/batch-distribution", {
            method: "GET", headers: headers
          }),
        ]);

        if ([topRes, lowRes, batchRes].some((res) => !res.ok)) {
          throw new Error("Failed to fetch chart data.");
        }

        const top = await topRes.json();
        const low = await lowRes.json();
        const batchData = await batchRes.json();

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
        // Hide all loaders on error
        this.isLoading = { metrics: false, charts: false, students: false };
      }
    },
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
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