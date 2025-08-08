import { api, loadComponent, logOut } from "../shared/utils.js";

window.batchDetailsPage = function () {
  return {
    batchId: null,
    batch: {},
    students: {
      items: [],
      currentPage: 1,
      totalPages: 1,
      pageSize: 5,
      hasNextPage: false,
      hasPreviousPage: false,
    },
    assessments: { items: [] },

    async init() {
      this.batchId = new URLSearchParams(window.location.search).get("id");
      if (!this.batchId) {
        Swal.fire("Error", "No Batch ID specified!", "error").then(() => {
          window.location.href = "/public/admin/batches.html";
        });
        return;
      }

      // Load sidebar & navbar based on role
      const role = localStorage.getItem("userRole");
      const sidebar =
        role === "Admin"
          ? "/public/components/sidebar.html"
          : "/public/components/instructor-sidebar.html";
      const navbar =
        role === "Admin"
          ? "/public/components/nav.html"
          : "/public/components/instructor-nav.html";

      await Promise.all([
        loadComponent("sidebar-placeholder", sidebar),
        loadComponent("navbar-placeholder", navbar),
      ]);

      // Load all data
      this.loadPageData();
    },

    async loadPageData() {
      try {
        const [detailsRes, assessmentsRes, trendRes, submissionsRes] =
          await Promise.all([
            api.get(`/Batches/${this.batchId}/details`),
            api.get(`/Assessments/${this.batchId}/assessments/details`),
            api.get(`/Batches/${this.batchId}/performance-trend`),
            api.get(`/Batches/${this.batchId}/submission-stats`),
          ]);

        const details = await detailsRes.json();
        const assessmentsData = await assessmentsRes.json();
        const trend = await trendRes.json();
        const submissionStats = await submissionsRes.json();

        if (details.status) this.batch = details.data;
        if (assessmentsData.status) this.assessments = assessmentsData.data;

        // Fetch first page of students
        await this.fetchStudents(1);

        // Draw charts
        if (trend.status) this.drawPerformanceTrendChart(trend.data);
        if (submissionStats.status)
          this.drawSubmissionChart(submissionStats.data);
      } catch (error) {
        console.error("Failed to load batch details:", error);
        Swal.fire(
          "Error",
          "Failed to load batch details. Please try again.",
          "error"
        );
      }
    },

    async fetchStudents(page = 1) {
      if (page < 1) return;

      try {
        const res = await api.get(
          `/Users/${this.batchId}/students?pageSize=${this.students.pageSize}&currentPage=${page}`
        );
        const studentsData = await res.json();
        if (studentsData.status) {
          this.students = studentsData.data;
        }
      } catch (error) {
        console.error("Error fetching students:", error);
        Swal.fire("Error", "Could not fetch students for this batch.", "error");
      }
    },

    drawPerformanceTrendChart(data) {
      if (!data || !data.labels || !data.scores) return;
      const ctx = document
        .getElementById("performanceTrendChart")
        .getContext("2d");

      new Chart(ctx, {
        type: "line",
        data: {
          labels: data.labels,
          datasets: [
            {
              label: "Average Score",
              data: data.scores,
              borderColor: "#3b82f6",
              backgroundColor: "rgba(59, 130, 246, 0.1)",
              fill: true,
              tension: 0.3,
            },
          ],
        },
        options: {
          responsive: true,
          scales: { y: { beginAtZero: true, max: 100 } },
        },
      });
    },

    drawSubmissionChart(data) {
      if (!data) return;
      const ctx = document.getElementById("submissionChart").getContext("2d");

      new Chart(ctx, {
        type: "bar",
        data: {
          labels: data.map((d) => d.assessmentTitle),
          datasets: [
            {
              label: "Total Assigned",
              data: data.map((d) => d.totalAssigned),
              backgroundColor: "#a5b4fc",
            },
            {
              label: "Total Submitted",
              data: data.map((d) => d.totalSubmitted),
              backgroundColor: "#4f46e5",
            },
          ],
        },
        options: {
          responsive: true,
          scales: { y: { beginAtZero: true } },
        },
      });
    },

    logOut,
  };
};
