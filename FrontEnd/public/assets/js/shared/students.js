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

    async init() {
      await this.loadSidebarAndNavbar();
      await this.loadBatches();
      await this.fetchStudents();
      this.disableForInstructor();
    },

    async loadBatches() {
      try {
        const res = await fetch("http://localhost:5162/api/v1/batches/all", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        });
        const data = await res.json();
        if (data.status) this.batches = data.data;
      } catch (err) {
        console.error("Failed to load batches:", err);
      }
    },

    async fetchStudents() {
      const params = new URLSearchParams();

      if (this.selectedBatch) params.append("batchId", this.selectedBatch);
      if (this.searchQuery) params.append("query", this.searchQuery);
      if (this.statusFilter) params.append("status", this.statusFilter);

      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.currentPage);

      const url = `http://localhost:5162/api/v1/Students/search?${params.toString()}`;
      console.log("Fetching students with URL:", url);

      try {
        const res = await fetch(url, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        });
        const data = await res.json();

        if (data.status) {
          this.students = data.data.items;
          this.pagination = data.data;
          console.log(this.pagination)
        } else {
          this.students = []; 
          this.pagination = {};
        }
      } catch (err) {
        console.error("Failed to fetch students:", err);
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
      console.log(this.newStudent);
      this.users.push(this.newStudent);
      const bulkStudent = {
        users: this.users,}
      console.log("Users to be created:", JSON.stringify(bulkStudent));
      try {
        const res = await fetch("http://localhost:5162/api/v1/Students", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
          body: JSON.stringify(bulkStudent),
        });
        const data = await res.json();
        if (data.status) {
          alert("Student created successfully!");
          this.closeCreateModal();
          this.resetAndFetch();
        } else {
          alert(data.errors.users.Email);
        }
     
      } catch (err) {
        console.error(err);
        alert("Failed to create student.");
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
  };
}

async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
