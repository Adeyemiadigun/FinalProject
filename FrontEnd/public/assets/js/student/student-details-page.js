import { api, loadComponent, logOut } from "../shared/utils.js";

window.studentDetailsPage = function () {
  return {
    studentId: new URLSearchParams(window.location.search).get("id"),
    student: {},
    analytics: {},
    submissions: [],
    searchText: "",

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
      ]);

      this.drawCharts();
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

    resendAssessment() {
      Swal.fire("Info", "Resend assessment logic not implemented.", "info");
    },

    drawCharts() {
      // --- Score Trend Chart
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

      // --- Type Distribution Chart
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

    navToStudentSubmissions() {
      window.location.href = `/public/shared/student-submission.html?id=${this.studentId}`;
    },

    logOut,
  };
};
