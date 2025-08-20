import { api, loadComponent, logOut } from "../shared/utils.js";

window.dashboardApp = function () {
  return {
    student: { name: "", batch: "" },
    summaryCards: {},
    ongoing: [],
    upcoming: [],
    history: [],
    scoreTrend: [],
    questionTypeScores: [],
    filterStartDate: null,
    filterEndDate: null,
    trendStartDate: null,
    trendEndDate: null,
    _trendChartInstance: null,
    _typeChartInstance: null,
    ongoingLoading: false,
    ongoingError: false,
    upcomingLoading: false,
    upcomingError: false,
    leaderboardTop: [],
    studentRank: null,
    studentLeaderboard: null,
    
    loading: true,

    async initDashboard() {
      
      this.loading = true; 
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
      await this.fetchScoreByType();
      await this.fetchLeaderboardSummary();
      this.loading = false;
    },
    async fetchScoreByType() {
      try {
        const query = new URLSearchParams();
        if (this.filterStartDate)
          query.append("startDate", this.filterStartDate);
        if (this.filterEndDate) query.append("endDate", this.filterEndDate);

        const res = await api.get(
          `/Students/score-by-type?${query.toString()}`
        );
        const data = await res.json();
        this.questionTypeScores = data.data || [];

        const labels = this.questionTypeScores.map((x) => x.type);
        const scores = this.questionTypeScores.map((x) => x.averageScore);

        const ctx = document.getElementById("questionTypeChart");
        if (this._typeChartInstance) this._typeChartInstance.destroy(); // destroy previous instance
        this._typeChartInstance = new Chart(ctx, {
          type: "pie",
          data: {
            labels: labels,
            datasets: [
              {
                label: "Average Score (%)",
                data: scores,
                backgroundColor: [
                  "#3b82f6",
                  "#10b981",
                  "#f59e0b",
                  "#ef4444",
                  "#8b5cf6",
                ],
              },
            ],
          },
          options: {
            responsive: true,
          },
        });
      } catch (e) {
        console.error("Failed to fetch score by type:", e);
      }
    },

    async fetchStudentInfo() {
      const res = await api.get("/students/details");
      const data = await res.json();
      const d = data.data;
      console.log(d)
      this.student.name = d.fullName;
      this.student.batch = d.batchName;
    },
    async fetchLeaderboardSummary() {
  try {
    const res = await api.get("/Students/leaderboard-summary");
    const data = await res.json();
    const d = data.data;

    this.leaderboardTop = d.topThree || [];
    this.studentRank = d.studentRank;
    this.studentLeaderboard = d.currentStudent;
  } catch (e) {
    console.error("Failed to fetch leaderboard summary:", e);
  }
},

    async fetchSummary() {
      const res = await api.get("/Students/summary");
      const data = await res.json();
      const d = data.data;
      console.log(d);
      this.summaryCards = {
        "Total Assessments": d.totalAssessments ||0,
        "Avg Score": `${Math.round(d.averageScore)||0}%`,
        "Completion Rate": `${Math.round(d.completionRate)||0}%`,
        "Highest Score": d.highestScore || 0,
        "Completed": d.completed ||0,
      };
    },

    async fetchOngoing() {
      this.ongoingLoading = true;
      this.ongoingError = false;
      try {
        const res = await api.get("/Students/ongoing-assessments");
        const data = await res.json();
        this.ongoing = data.data || [];
      } catch (e) {
        console.error("Failed to fetch ongoing assessments:", e);
        this.ongoingError = true;
      } finally {
        this.ongoingLoading = false;
      }
    },

    async fetchUpcoming() {
      this.upcomingLoading = true;
      this.upcomingError = false;
      try {
        const res = await api.get("/Students/upcoming-assessments");
        const data = await res.json();
        this.upcoming = data.data || [];
      } catch (e) {
        console.error("Failed to fetch upcoming assessments:", e);
        this.upcomingError = true;
      } finally {
        this.upcomingLoading = false;
      }
    },

    async fetchHistory() {
      const res = await api.get("/Students/history");
      const data = await res.json();
      this.history = data.data || [];
    },

    async fetchTrend() {
      try {
        const query = new URLSearchParams();
        if (this.trendStartDate) query.append("startDate", this.trendStartDate);
        if (this.trendEndDate) query.append("endDate", this.trendEndDate);

        const res = await api.get(
          `/Students/performance-trend?${query.toString()}`
        );
        const data = await res.json();

        const labels = data.data.map((x) => x.labels);
        const scores = data.data.map((x) => x.scores);

        const ctx = document.getElementById("performanceChart");

        if (this._trendChartInstance) this._trendChartInstance.destroy(); // destroy old chart
        this._trendChartInstance = new Chart(ctx, {
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
      } catch (e) {
        console.error("Failed to fetch performance trend:", e);
      }
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
