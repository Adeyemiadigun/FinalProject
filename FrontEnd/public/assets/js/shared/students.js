import { api, loadComponent, logOut } from "../shared/utils.js";

window.studentsPage = function () {
  return {
    students: [],
    batches: [],
    selectedBatch: "",
    statusFilter: "",
    searchQuery: "",
    pagination: {},
    currentPage: 1,
    pageSize: 10,
    showCreateModal: false,
    newStudent: {
      fullName: "",
      email: "",
      password: "",
      confirmPassword: "",
      batchId: "",
    },
    isLoading: true,
    isSubmitting: false,
    sidebarOpen: true,
    uploadFile: null,
    uploadBatchId: "",
    isUploading: false,
    showUploadModal: false,

    handleFileChange(event) {
      this.uploadFile = event.target.files[0];
    },

    async submitStudentCSV() {
      if (!this.uploadFile || !this.uploadBatchId) {
        Swal.fire("Error", "Please select a file and a batch.", "error");
        return;
      }

      this.isUploading = true;

      const formData = new FormData();
      formData.append("StudentFile", this.uploadFile);

      try {
        const res = await api.postFormData(
          `/Students/upload?BatchId=${this.uploadBatchId}`,
          formData,
        );

        const data = await res.json();
        if (data.status) {
          Swal.fire(
            "Success",
            data.message || "Students uploaded successfully!",
            "success"
          );
          this.closeUploadModal();
          this.resetAndFetch();
        } else {
          Swal.fire(
            "Error",
            data.message || "Failed to upload students.",
            "error"
          );
        }
      } catch (err) {
        console.error("Upload failed:", err);
      } finally {
        this.isUploading = false;
      }
    },
    openUploadModal() {
      this.showUploadModal = true;
    },
    closeUploadModal() {
      this.showUploadModal = false;
      this.uploadFile = null;
      this.uploadBatchId = "";
    },

    async init() {
      await this.loadSidebarAndNavbar();
      await this.loadBatches();
      await this.fetchStudents();
      this.disableForInstructor();
    },
    resetForm() {
      this.newStudent = {
        fullName: "",
        email: "",
        password: "",
        confirmPassword: "",
        batchId: "",
      };
      this.uploadFile = null;
      this.uploadBatchId = "";
    },

    async loadBatches() {
      try {
        const res = await api.get("/batches/all");
        const data = await res.json();
        if (data.status) this.batches = data.data;
        console.log("Batches loaded:", this.batches);
      } catch (err) {
        console.error("Failed to load batches:", err);
        Swal.fire("Error", "Could not load batch options.", "error");
      }
    },

    async fetchStudents() {
      this.isLoading = true;

      const params = new URLSearchParams();
      if (this.selectedBatch) params.append("batchId", this.selectedBatch);
      if (this.searchQuery) params.append("query", this.searchQuery);
      if (this.statusFilter) params.append("status", this.statusFilter);

      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.currentPage);

      try {
        const res = await api.get(`/Students/search?${params.toString()}`);
        const data = await res.json();

        if (data.status) {
          this.students = data.data.items || [];
          this.pagination = data.data;
        } else {
          this.students = [];
          this.pagination = {};
          Swal.fire(
            "Error",
            data.message || "Failed to fetch students.",
            "error"
          );
        }
      } catch (err) {
        this.students = [];
        console.error("Failed to fetch students:", err);
        Swal.fire(
          "Error",
          "An error occurred while fetching students.",
          "error"
        );
      } finally {
        this.isLoading = false;
      }
    },

    resetAndFetch() {
      this.currentPage = 1;
      this.fetchStudents();
    },

    prevPage() {
      if (this.pagination.hasPreviousPage) {
        this.currentPage--;
        this.fetchStudents();
      }
    },

    nextPage() {
      if (this.pagination.hasNextPage) {
        this.currentPage++;
        this.fetchStudents();
      }
    },

    goToDetail(id) {
      window.location.href = `/public/shared/student-details.html?id=${id}`;
    },

    openCreateModal() {
      this.showCreateModal = true;
    },

    closeCreateModal() {
      this.showCreateModal = false;
      this.resetForm();
    },

    resetForm() {
      this.newStudent = {
        fullName: "",
        email: "",
        password: "",
        confirmPassword: "",
        batchId: "",
      };
    },

    async submitStudent() {
      if (this.newStudent.password !== this.newStudent.confirmPassword) {
        Swal.fire("Error", "Passwords do not match.", "error");
        return;
      }

      this.isSubmitting = true;
      const bulkStudent = { users: [this.newStudent] };

      try {
        const res = await api.post("/Students", bulkStudent);
        const data = await res.json();

        if (data.status) {
          Swal.fire("Success!", "Student created successfully!", "success");
          this.closeCreateModal();
          this.resetAndFetch();
        } else {
          const errorMessage =
            data.errors?.users?.Email ||
            data.message ||
            "An unknown error occurred.";
          Swal.fire("Error", errorMessage, "error");
        }
      } catch (err) {
        console.error(err);
        Swal.fire("Error", "Failed to create student.", "error");
      } finally {
        this.isSubmitting = false;
      }
    },

    disableForInstructor() {
      try {
        const role = localStorage.getItem("userRole");
        if (role?.toLowerCase() === "instructor") {
          document.getElementById("addStudentBtn").disabled = true;
          document.getElementById("uploadStudentBtn").disabled = true;
        }
      } catch (err) {
        console.warn("Could not disable for instructor:", err);
      }
    },

    async loadSidebarAndNavbar() {
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
    },

    logOut,
  };
};
