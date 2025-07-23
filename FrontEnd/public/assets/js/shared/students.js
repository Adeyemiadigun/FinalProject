function studentsPage() {
  return {
    students: [],
    batches: [],
    selectedBatch: "",
    statusFilter: "",
    searchQuery: "",
    pagination: {},
    currentPage: 1,
    pageSize: 10,
    users: [],
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

    async init() {
      await this.loadSidebarAndNavbar();
      await this.loadBatches();
      await this.fetchStudents();
      this.disableForInstructor();
    },

    async loadBatches() {
      try {
        const res = await fetch("https://localhost:7157/api/v1/batches/all", {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
          },
        });
        const data = await res.json();
        if (data.status) this.batches = data.data;
      } catch (err) {
        console.error("Failed to load batches:", err);
        Swal.fire('Error', 'Could not load batch options.', 'error');
      }
    },

    async fetchStudents() {
      const params = new URLSearchParams();
      this.isLoading = true;

      if (this.selectedBatch) params.append("batchId", this.selectedBatch);
      if (this.searchQuery) params.append("query", this.searchQuery);
      if (this.statusFilter) params.append("status", this.statusFilter);

      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.currentPage);

      const url = `https://localhost:7157/api/v1/Students/search?${params.toString()}`;
      console.log("Fetching students with URL:", url);

      try {
        const res = await fetch(url, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
          },
        });
        const data = await res.json();

        if (data.status) {
          this.students = data.data.items || [];
          this.pagination = data.data;
          console.log(this.pagination);
        } else {
          this.students = [];
          this.pagination = {};
          Swal.fire('Error', data.message || 'Failed to fetch students.', 'error');
        }
      } catch (err) {
        this.students = [];
        console.error("Failed to fetch students:", err);
        Swal.fire('Error', 'An error occurred while fetching students.', 'error');
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
        Swal.fire('Error', 'Passwords do not match.', 'error');
        return;
      }

      this.isSubmitting = true;
      console.log(this.newStudent);
      // Create a fresh user array for the request
      const usersToCreate = [this.newStudent];
      const bulkStudent = {
        users: usersToCreate,
      };

      try {
        const res = await fetch("https://localhost:7157/api/v1/Students", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
          },
          body: JSON.stringify(bulkStudent),
        });
        const data = await res.json();
        if (data.status) {
          Swal.fire('Success!', 'Student created successfully!', 'success');
          this.closeCreateModal();
          this.resetAndFetch();
        } else {
          const errorMessage = data.errors?.users?.Email || data.message || 'An unknown error occurred.';
          Swal.fire('Error', errorMessage, 'error');
        }
      } catch (err) {
        console.error(err);
        Swal.fire('Error', 'Failed to create student.', 'error');
      } finally {
        this.isSubmitting = false;
      }
    },

    disableForInstructor() {
      try {
        const role = localStorage.getItem("userRole");
        if (role?.toLowerCase() === "instructor") {
          document.getElementById("addStudentBtn").disabled = true;
        }
      } catch (err) {
        console.warn("Could not decode token:", err);
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

      await loadComponent("sidebar-placeholder", sidebar);
      await loadComponent("navbar-placeholder", navbar);
    },

    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
