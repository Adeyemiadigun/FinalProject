function studentDetailsPage() {
  return {
    studentId: new URLSearchParams(window.location.search).get("id"),
    student: {},
    analytics: {},
    submissions: [],
    searchText: "",
    token: "",
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
      this.token = localStorage.getItem("accessToken");
      const role = localStorage.getItem("userRole");
      console.log(role)
      const sidebar =
        role == "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role == "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await loadComponent("sidebar-placeholder", sidebar);
      await loadComponent("navbar-placeholder", navbar);
      await Promise.all([
        this.loadStudent(),
        this.loadAnalytics(),
        this.loadSubmissions(),
      ]);
      this.drawCharts();
    },

    async loadStudent() {
      const res = await fetch(
        `https://localhost:7157/api/v1/Students/${this.studentId}/details`,
        {
          headers: {
            Authorization: `Bearer ${this.token}`,
          },
        }
      );
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
    },

    async loadAnalytics() {
      const res = await fetch(
        `https://localhost:7157/api/v1/students/${this.studentId}/analytics`,
        {
          headers: {
            Authorization: `Bearer ${this.token}`,
          },
        }
      );
      const data = await res.json();
      this.analytics = data.data;
    },

    async loadSubmissions() {
      const res = await fetch(
        `https://localhost:7157/api/v1/Students/${this.studentId}/submissions`,
        {
          headers: {
            Authorization: `Bearer ${this.token}`,
          },
        }
      );
      const data = await res.json();
      this.submissions = data.data.sort(
        (a, b) => new Date(b.submittedAt) - new Date(a.submittedAt)
      );
    },

    async updateStatus(newStatus) {
      await fetch(
        `https://localhost:7157/api/students/${this.studentId}/status`,
        {
          method: "PATCH",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${this.token}`,
          },
          body: JSON.stringify({ status: newStatus }),
        }
      );
      this.student.status = newStatus;
      alert("Student status updated.");
    },

    async reassignBatch() {
      // const batchId = prompt("Enter new Batch ID:");
      // if (!batchId) return;
      const res = await fetch(
        `https://localhost:7157/api/students/${this.studentId}/reassign-batch?batchId=${batchId}`,
        {
          method: "PATCH",
          
          headers: {
            Authorization: `Bearer ${this.token}`,
          },
        }
        
      );
      if (res.ok) {
        alert("Batch reassigned successfully.");
        this.loadStudent();
      }
    },

    resendAssessment() {
      alert("Triggering resend logic (not implemented)");
      // Optional implementation hook
    },

    drawCharts() {
      const ctx1 = document.getElementById("scoreTrendChart");
      new Chart(ctx1, {
        type: "line",
        data: {
          labels: this.submissions.map((s) => s.title),
          datasets: [
            {
              label: "Score (%)",
              data: this.submissions.map((s) => s.totalScore),
              borderColor: "#3b82f6",
              fill: false,
              tension: 0.3,
            },
          ],
        },
      });

      const typeCounts = this.submissions.reduce((acc, s) => {
        const type = s.title.includes("Coding") ? "Coding" : "MCQ";
        acc[type] = (acc[type] || 0) + 1;
        return acc;
      }, {});

      const ctx2 = document.getElementById("typeDistChart");
      new Chart(ctx2, {
        type: "doughnut",
        data: {
          labels: Object.keys(typeCounts),
          datasets: [
            {
              data: Object.values(typeCounts),
              backgroundColor: ["#3b82f6", "#f59e0b"],
            },
          ],
        },
      });
    },
    navToStudentSubmissions()
    {
      window.location.href= `/public/shared/student-submission.html?id=${this.studentId}`
    }
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
