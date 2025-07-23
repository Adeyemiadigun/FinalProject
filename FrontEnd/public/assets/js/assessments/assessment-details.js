function assessmentDetailsPage() {
  return {
    token: localStorage.getItem("accessToken") || "",
    assessment: {},
    metrics: {},
    students: [],
    page: 1,
    perPage: 5,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
    assessmentId: new URLSearchParams(window.location.search).get(
      "assessmentId"
    ),

    init() {
      loadComponent("sidebar-placeholder", "../components/sidebar.html");
      loadComponent("navbar-placeholder", "../components/nav.html");
      this.loadAssessment();
    },

    async loadAssessment() {
      await this.fetchAssessment();
      await this.fetchMetrics();
      await this.fetchScoreDistribution();
      await this.fetchBatchPerformance();
      await this.fetchStudentPerformance();
    },

    async fetchAssessment() {
      console.log(this.assessmentId);
      const res = await fetch(
        `https://localhost:7157/api/v1/Assessments/${this.assessmentId}`,
        {
          headers: { Authorization: `Bearer ${this.token}` },
        }
      );
      const data = await res.json();
      this.assessment = data.data;
    },

    async fetchMetrics() {
      const res = await fetch(
        `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/metrics`,
        {
          headers: { Authorization: `Bearer ${this.token}` },
        }
      );
      const data = await res.json();
      this.metrics = data.data;
    },

    async fetchScoreDistribution() {
      const res = await fetch(
        `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/score-distribution`,
        {
          headers: { Authorization: `Bearer ${this.token}` },
        }
      );
      const data = await res.json();
      const dist = data.data;

      new Chart(document.getElementById("scoreDistChart"), {
        type: "bar",
        data: {
          labels: dist.map((x) => x.cap),
          datasets: [
            {
              label: "Number of Students",
              data: dist.map((x) => x.count),
              backgroundColor: "#3b82f6",
            },
          ],
        },
      });
    },

    async fetchBatchPerformance() {
      const res = await fetch(
        `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/batch-performance`,
        {
          headers: { Authorization: `Bearer ${this.token}` },
        }
      );
      const data = await res.json();
      const perf = data.data;

      new Chart(document.getElementById("batchPerfChart"), {
        type: "doughnut",
        data: {
          labels: perf.map((x) => x.batchName),
          datasets: [
            {
              data: perf.map((x) => x.averageScore),
              backgroundColor: [
                "#3b82f6",
                "#f59e0b",
                "#10b981",
                "#ef4444",
                "#6366f1",
              ],
            },
          ],
        },
      });
    },

    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
    async fetchStudentPerformance() {
      const res = await fetch(
        `https://localhost:7157/api/v1/Assessments/${this.assessmentId}/students?CurrentPage=${this.page}&PageSize=${this.perPage}`,
        {
          headers: { Authorization: `Bearer ${this.token}` },
        }
      );
      const data = await res.json();
      console.log(data);
      const paginated = data.data;
      this.students = paginated.items;
      console.log(paginated);
      console.log(paginated.items);
      console.log(this.students);
      this.totalPages = paginated.totalPages;
      this.hasNextPage = paginated.hasNextPage;
      this.hasPreviousPage = paginated.hasPreviousPage;
    },

    prevPage() {
      if (this.page > 1) {
        this.page--;
        this.fetchStudentPerformance();
      }
    },

    nextPage() {
      if (this.page < this.totalPages) {
        this.page++;
        this.fetchStudentPerformance();
      }
    },
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
