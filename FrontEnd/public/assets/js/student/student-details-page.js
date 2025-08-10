import { api, loadComponent, logOut } from "../shared/utils.js";

window.studentDetailsPage = function () {
  return {
    studentId: new URLSearchParams(window.location.search).get("id"),
    student: {},
    analytics: {},
    submissions: [],
    searchText: "",
    scoreTrend: [],
    typeScoreStart: null,
    typeScoreEnd: null,
    assessmentHistory: [],
    pagination: {
      totalItems: 0,
      totalPages: 0,
      currentPage: 1,
      pageSize: 5,
    },
    _scoreTrendChart: null,
    _scoreByTypeChart: null,
    get completionRate() {
      if (!this.analytics.totalAssessments) return 0;
      return Math.round(
        (this.analytics.attempted / this.analytics.totalAssessments) * 100
      );
    },

    filteredSubmissions() {
      const term = this.searchText.toLowerCase();
      return this.submissions.filter((s) =>
        s.title.toLowerCase().includes(term)
      );
    },

    async init() {
      const role = localStorage.getItem("userRole");
      const sidebar =
        role === "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role === "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await Promise.all([
        loadComponent("sidebar-placeholder", sidebar),
        loadComponent("navbar-placeholder", navbar),
      ]);

      await Promise.all([
        this.loadStudent(),
        this.loadAnalytics(),
        // this.loadSubmissions(),

        this.fetchScoreTrend(),
        this.loadScoreByType(),

        this.loadAssessmentHistory(),
      ]);
    },
    async fetchScoreTrend(monthValue = null) {
      try {
        let url = `/Students/${this.studentId}/score-trends`;
        if (monthValue) {
          const selectedDate = new Date(monthValue + "-01");
          const isoDate = selectedDate.toISOString();
          url += `?date=${isoDate}`;
        }
        console.log(url);
        const res = await api.get(url);
        const data = await res.json();

        this.scoreTrend = data.data || [];

        this.drawScoreTrendChart(); // refresh the chart
      } catch (error) {
        console.error("Error fetching score trend:", error);
        Swal.fire("Error", "Failed to load score trend data.", "error");
      }
    },
    async loadAssessmentHistory(page = 1) {
  try {
    const params = new URLSearchParams({
      pageSize: this.pagination.pageSize,
      currentPage: page,
    });

    if (this.searchText) {
      params.append("titleSearch", this.searchText);
    }
console.log(params.toString());
    const res = await api.get(`/Students/${this.studentId}/assessment-history?${params}`);
    const data = await res.json();

    this.assessmentHistory = data.data.items || [];
    this.pagination.totalItems = data.data.totalItems;
    this.pagination.totalPages = data.data.totalPages;
    this.pagination.currentPage = data.data.currentPage;
  } catch (error) {
    console.error("Error loading assessment history:", error);
    Swal.fire("Error", "Could not load assessment history.", "error");
  }
},
    async loadStudent() {
      try {
        const res = await api.get(`/Students/${this.studentId}/details`);
        const data = await res.json();
        this.student = {
          name: data.data.fullName,
          email: data.data.email,
          status: data.data.status ? "Active" : "Inactive",
          batch: data.data.batchName || "-",
          joinDate: new Date(
            data.data.createdAt || Date.now()
          ).toLocaleDateString(),
        };
      } catch (error) {
        console.error("Error loading student details:", error);
        Swal.fire("Error", "Could not load student details.", "error");
      }
    },

    async loadAnalytics() {
      try {
        const res = await api.get(`/Students/${this.studentId}/analytics`);
        const data = await res.json();
        this.analytics = data.data || {};
      } catch (error) {
        console.error("Error loading student analytics:", error);
        Swal.fire("Error", "Could not load student analytics.", "error");
      }
    },

    async loadSubmissions() {
      try {
        const res = await api.get(`/Students/${this.studentId}/submissions`);
        const data = await res.json();
        this.submissions = (data.data || []).sort(
          (a, b) => new Date(b.submittedAt) - new Date(a.submittedAt)
        );
      } catch (error) {
        console.error("Error loading submissions:", error);
        Swal.fire("Error", "Could not load submissions.", "error");
      }
    },

    async updateStatus(newStatus) {
      try {
        await api.patch(`/students/${this.studentId}/status`, {
          status: newStatus,
        });
        this.student.status = newStatus;
        Swal.fire("Success", "Student status updated.", "success");
      } catch (error) {
        console.error("Error updating status:", error);
        Swal.fire("Error", "Failed to update student status.", "error");
      }
    },

    async reassignBatch(batchId) {
      if (!batchId) return;
      try {
        const res = await api.patch(
          `/students/${this.studentId}/reassign-batch?batchId=${batchId}`
        );
        if (res.ok) {
          Swal.fire("Success", "Batch reassigned successfully.", "success");
          this.loadStudent();
        }
      } catch (error) {
        console.error("Error reassigning batch:", error);
        Swal.fire("Error", "Failed to reassign batch.", "error");
      }
    },
    async loadScoreByType(startDate = null, endDate = null) {
      try {
        let url = `/Students/${this.studentId}/score-by-type`;
        if (startDate && endDate) {
          const params = new URLSearchParams({
            startDate: new Date(startDate).toISOString(),
            endDate: new Date(endDate).toISOString(),
          });
          url += `?${params.toString()}`;
        }

        const res = await api.get(url);
        const data = await res.json();

        this.analytics.scoreByType = data.data || [];

        // 🛠️ Delay drawing the chart until the DOM is fully rendered
        setTimeout(() => {
          this.drawScoreByTypeChart();
        }, 100); // delay by 100ms
      } catch (error) {
        console.error("Error fetching score-by-type:", error);
        Swal.fire("Error", "Failed to load score by question type.", "error");
      }
    },

    drawCharts() {
      this.drawScoreTrendChart();
      this.drawScoreByTypeChart();
    },
    drawScoreTrendChart() {
      const ctx1 = document.getElementById("scoreTrendChart");
      if (!ctx1) return;

      // Clear old canvas if any
      if (this._scoreTrendChart) {
        this._scoreTrendChart.destroy();
      }

      const labels = this.scoreTrend.map((s) => s.labels);
      const scores = this.scoreTrend.map((s) => s.scores);

      this._scoreTrendChart = new Chart(ctx1, {
        type: "line",
        data: {
          labels,
          datasets: [
            {
              label: "Average Score (%)",
              data: scores,
              borderColor: "#3b82f6",
              fill: false,
              tension: 0.3,
            },
          ],
        },
      });
    },
drawScoreByTypeChart() {
  const ctx = document.getElementById("scoreByTypeChart");
  if (!ctx || !this.analytics.scoreByType) return;

  if (this._scoreByTypeChart) {
    this._scoreByTypeChart.destroy();
  }

  console.log(this.analytics.scoreByType);

  const labels = this.analytics.scoreByType.map((t) => t.type);
  const scores = this.analytics.scoreByType.map((t) => t.averageScore);
  const attemptCounts = this.analytics.scoreByType.map((t) => t.attemptCount || 0);

  this._scoreByTypeChart = new Chart(ctx, {
    type: "pie",
    data: {
      labels,
      datasets: [
        {
          data: scores,
          backgroundColor: [
            "#3b82f6",
            "#f59e0b",
            "#10b981",
            "#ef4444",
            "#8b5cf6",
            "#14b8a6",
          ],
        },
      ],
    },
    options: {
      plugins: {
        tooltip: {
          callbacks: {
            label: function (context) {
              const index = context.dataIndex;
              const label = context.label;
              const score = context.raw.toFixed(1);
              const count = attemptCounts[index];
              return `${label}: ${score}% (${count} attempt${count === 1 ? '' : 's'})`;
            },
          },
        },
      },
    },
  });
},
    navToStudentSubmissions() {
      window.location.href = `/public/shared/student-submission.html?id=${this.studentId}`;
    },

    logOut,
  };
};
