async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}

async function initLayout() {
  await loadComponent("sidebar-placeholder", "/public/components/sidebar-student.html");
  await loadComponent("navbar-placeholder", "/public/components/navbar-student.html");
}

function dashboardApp() {
  return {
    student: { name: "", batch: "" },
    summaryCards: {},
    ongoing: [],
    upcoming: [],
    history: [],
    scoreTrend: [],

    async initDashboard() {
     await loadComponent(
        "sidebar-placeholder",
        "/public/components/sidebar-student.html"
      );
      await loadComponent(
        "navbar-placeholder",
        "/public/components/navbar-student.html"
      );
      const token = localStorage.getItem("accessToken");
      const headers = { Authorization: `Bearer ${token}` };

     
  try {
    await this.fetchStudentInfo(headers);
  } catch (e) {
    console.error("Failed to fetch student info:", e);
  }

  try {
    await this.fetchSummary(headers);
  } catch (e) {
    console.error("Failed to fetch summary:", e);
  }

  try {
    await this.fetchOngoing(headers);
  } catch (e) {
    console.error("Failed to fetch ongoing assessments:", e);
  }

  try {
    await this.fetchUpcoming(headers);
  } catch (e) {
    console.error("Failed to fetch upcoming assessments:", e);
  }

  try {
    await this.fetchHistory(headers);
  } catch (e) {
    console.error("Failed to fetch history:", e);
  }

  try {
    await this.fetchTrend(headers);
  } catch (e) {
    console.error("Failed to fetch performance trend:", e);
  }
    },
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
    async fetchStudentInfo(headers) {
      const res = await fetch(
        "https://localhost:7157/api/v1/students/details",
        { headers }
      );
      const data = await res.json();
      const d = data.data;
      this.student.name = d.fullName;
      this.student.batch = d.batchName;
    },

    async fetchSummary(headers) {
      const res = await fetch(
        "https://localhost:7157/api/v1/Students/summary",
        { headers }
      );
      const data = await res.json();
      const d = data.data;
      this.summaryCards = {
        "Total Assessments": d.totalAssessments,
        "Avg Score": `${Math.round(d.averageScore)}%`,
        "Completion Rate": `${Math.round(d.completionRate)}%`,
        Completed: d.completedAssessments,
      };
    },

    async fetchOngoing(headers) {
      const res = await fetch(
        "https://localhost:7157/api/v1/Students/ongoing",
        { headers }
      );
      const data = await res.json();
      this.ongoing = data.data || [];
    },

    async fetchUpcoming(headers) {
      const res = await fetch(
        "https://localhost:7157/api/v1/Students/upcoming",
        { headers }
      );
      const data = await res.json();
      this.upcoming = data.data || [];
    },

    async fetchHistory(headers) {
      const res = await fetch(
        "https://localhost:7157/api/v1/Students/history",
        {
          headers,
        }
      );
      const data = await res.json();
      this.history = data.data || [];
    },

    async fetchTrend(headers) {
      const res = await fetch(
        "https://localhost:7157/api/v1/Students/performance-trend",
        { headers }
      );
      const data = await res.json();
      const labels = data.data.map((x) => x.labels);
      const scores = data.data.map((x) => x.scores);
      new Chart(document.getElementById("performanceChart"), {
        type: "line",
        data: {
          labels: labels,
          datasets: [
            {
              label: "Score %",
              data: scores,
              borderColor: "#6366f1",
              tension: 0.4,
              fill: false,
            },
          ],
        },
        options: {
          responsive: true,
          scales: { y: { beginAtZero: true, max: 100 } },
        },
      });
    },

    formatDate(dateStr) {
      const d = new Date(dateStr);
      return d.toLocaleDateString("en-US", {
        year: "numeric",
        month: "short",
        day: "numeric",
      });
    },
  };
}
