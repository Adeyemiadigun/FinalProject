import { api, loadComponent, logOut } from "../shared/utils.js";

window.dashboardApp = function () {
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

      try {
        await this.fetchStudentInfo();
      } catch (e) {
        console.error("Failed to fetch student info:", e);
      }

      try {
        await this.fetchSummary();
      } catch (e) {
        console.error("Failed to fetch summary:", e);
      }

      try {
        await this.fetchOngoing();
      } catch (e) {
        console.error("Failed to fetch ongoing assessments:", e);
      }

      try {
        await this.fetchUpcoming();
      } catch (e) {
        console.error("Failed to fetch upcoming assessments:", e);
      }

      try {
        await this.fetchHistory();
      } catch (e) {
        console.error("Failed to fetch history:", e);
      }

      try {
        await this.fetchTrend();
      } catch (e) {
        console.error("Failed to fetch performance trend:", e);
      }
    },

    async fetchStudentInfo() {
      const res = await api.get("/students/details");
      const data = await res.json();
      const d = data.data;
      this.student.name = d.fullName;
      this.student.batch = d.batchName;
    },

    async fetchSummary() {
      const res = await api.get("/Students/summary");
      const data = await res.json();
      const d = data.data;
      this.summaryCards = {
        "Total Assessments": d.totalAssessments,
        "Avg Score": `${Math.round(d.averageScore)}%`,
        "Completion Rate": `${Math.round(d.completionRate)}%`,
        Completed: d.completedAssessments,
      };
    },

    async fetchOngoing() {
      const res = await api.get("/Students/ongoing");
      const data = await res.json();
      this.ongoing = data.data || [];
    },

    async fetchUpcoming() {
      const res = await api.get("/Students/upcoming");
      const data = await res.json();
      this.upcoming = data.data || [];
    },

    async fetchHistory() {
      const res = await api.get("/Students/history");
      const data = await res.json();
      this.history = data.data || [];
    },

    async fetchTrend() {
      const res = await api.get("/Students/performance-trend");
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

    logOut,
  };
};
