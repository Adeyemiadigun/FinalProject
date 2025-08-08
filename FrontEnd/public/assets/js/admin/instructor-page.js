import { api, loadComponent, logOut } from "../shared/utils.js";

window.instructorsPage = function () {
  return {
    instructors: [],
    search: "",
    statusFilter: "",
    currentPage: 1,
    pagination: {},
    pageSize: 10,
    showCreateModal: false,
    newInstructor: {
      fullName: "",
      email: "",
      password: "",
      confirmPassword: "",
    },
    sidebarOpen: true,

    async init() {
      // Load layout components
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");

      // Fetch data
      await this.fetchInstructors();
    },

    async fetchInstructors() {
      const params = new URLSearchParams();
      if (this.search) params.append("query", this.search);
      if (this.statusFilter) params.append("status", this.statusFilter);
      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.currentPage);

      try {
        const res = await api.get(`/Instructors/search?${params.toString()}`);
        const data = await res.json();

        if (res.ok && data.status) {
          this.instructors = data.data.items;
          this.pagination = data.data;
        } else {
          this.instructors = [];
          this.pagination = {};
          Swal.fire({
            icon: "warning",
            title: "No Results",
            text: data.message || "No instructors found with current filters.",
          });
        }
      } catch (err) {
        console.error("Failed to fetch instructors:", err);
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "Could not fetch instructors. Please try again later.",
        });
      }
    },

    async registerInstructor() {
      const payload = { ...this.newInstructor };

      try {
        const res = await api.post("/Instructors", payload);

        if (res.ok) {
          Swal.fire({
            icon: "success",
            title: "Instructor Registered",
            text: "The instructor has been successfully added.",
          });
          this.showCreateModal = false;
          this.resetForm();
          await this.fetchInstructors();
        } else {
          const error = await res.json();
          Swal.fire({
            icon: "error",
            title: "Registration Failed",
            text: error.message || "Could not register instructor.",
          });
        }
      } catch (err) {
        console.error(err);
        Swal.fire({
          icon: "error",
          title: "Server Error",
          text: "An unexpected error occurred while registering the instructor.",
        });
      }
    },

    resetForm() {
      this.newInstructor = {
        fullName: "",
        email: "",
        password: "",
        confirmPassword: "",
      };
    },

    resetAndFetch() {
      this.currentPage = 1;
      this.fetchInstructors();
    },

    prevPage() {
      if (this.pagination.hasPreviousPage) {
        this.currentPage--;
        this.fetchInstructors();
      }
    },

    nextPage() {
      if (this.pagination.hasNextPage) {
        this.currentPage++;
        this.fetchInstructors();
      }
    },

    viewInstructor(instructor) {
      window.location.href = `/public/admin/instructor-details.html?id=${instructor.id}`;
    },

    editInstructor(instructor) {
      Swal.fire(
        "Info",
        `Edit modal for ${instructor.fullName} not implemented yet.`,
        "info"
      );
    },

    toggleStatus(instructor) {
      Swal.fire(
        "Info",
        `Toggling status for ${instructor.fullName} not implemented yet.`,
        "info"
      );
    },

    openCreateModal() {
      this.showCreateModal = true;
      this.resetForm();
    },

    logOut, // Use shared logout
  };
}
