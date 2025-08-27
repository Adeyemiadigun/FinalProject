import { api, loadComponent, logOut } from "../shared/utils.js";

function instructorProfile() {
  return {
    loading: true,
    instructor: {},
    summary: {
      totalAssessments: 0,
      totalStudents: 0,
      avgScore: 0,
      passRate: 0,
    },
    pagination: {
      currentPage: 1,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
      pageSize: 5,
    },

    assessments: [],
    showEditModal: false,
    editForm: { name: "", email: "", currentPassword: "", newPassword: "" },
    errors: {},

    async init() {
      
      await this.loadLayout();
      await Promise.all([
        this.fetchInstructor(),
        this.fetchMetrics(),
        this.fetchAssessments(),
      ]);

    },
    async loadLayout() {
      const role = localStorage.getItem("userRole");
      const sidebar =
        role === "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role === "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await loadComponent("sidebar-placeholder", sidebar);
      await loadComponent("navbar-placeholder", navbar);
    },

    async fetchInstructor() {
      try {
        const res = await api.get("/instructors/me");
        const data = await res.json();
        if (!res.ok) throw data;

        this.instructor = {
          fullName: data.data.fullName,
          email: data.data.email,
          dateCreated: data.data.dateCreated,
        };
        this.assessments = data.assessments || [];
        this.editForm.name = data.fullName;
        this.editForm.email = data.email;
      } catch (err) {
        Swal.fire("Error", err.message || "Failed to load instructor", "error");
      } finally {
        this.loading = false;
      }
    },

    async fetchMetrics() {
      try {
        const metricsRes = await api.get(
          "/Dashboard/instructor/metrics/overview"
        );
        const metrics = await metricsRes.json();
        if (metrics.status) this.summary = metrics.data;
      } catch (error) {
        console.error("Failed to load metrics:", error);
      }
    },
    async fetchAssessments() {
      try {
        const params = new URLSearchParams({
          pageSize: this.pagination.pageSize,
          currentPage: this.pagination.currentPage,
        });
        const res = await api.get(`/instructors/assessments/summaries?${params}`);
        const data = await res.json();
        if (res.status) {
          this.assessments = data.data.items;
          // keep currentPage controlled by frontend
          this.pagination.totalPages = data.data.totalPages;
          this.pagination.hasNextPage = data.data.hasNextPage;
          this.pagination.hasPreviousPage = data.data.hasPreviousPage;
        } else {
          this.assessments = [];
        }
          
      } catch (error) {
        console.error("Failed to load metrics:", error);
      }
    },
    nextPage() {
      if (this.pagination.hasNextPage) {
        this.pagination.currentPage++;
        this.fetchSubmissions();
      }
    },

    prevPage() {
      if (this.pagination.hasPreviousPage) {
        this.pagination.currentPage--;
        this.fetchSubmissions();
      }
    },

    async submitUpdate() {
      this.errors = {};
      if (!this.editForm.name) this.errors.name = "Name is required";
      if (!this.editForm.email) this.errors.email = "Email is required";
      if (Object.keys(this.errors).length) return;

      try {
        const res = await api.put("/Users/update", {
          fullName: this.editForm.name,
          email: this.editForm.email,
          currentPassword: this.editForm.currentPassword || null,
          newPassword: this.editForm.newPassword || null,
        });
        const json = await res.json();
        if (!res.ok) throw json;
        Swal.fire("Success", "Profile updated successfully", "success");
        this.instructor.fullName = this.editForm.name;
        this.instructor.email = this.editForm.email;
        this.closeEditModal();
      } catch (err) {
        Swal.fire("Error", err.message || "Failed to update profile", "error");
      }
    },

    openEditModal() {
      this.editForm.name = this.instructor.fullName;
      this.editForm.email = this.instructor.email;
      this.editForm.currentPassword = "";
      this.editForm.newPassword = "";
      this.showEditModal = true;
    },
    closeEditModal() {
      this.showEditModal = false;
    },
    formatDate(d) {
      return new Date(d).toLocaleDateString();
    },
  };
}

window.instructorProfile = instructorProfile;
