function loadLayout() {
  fetch("/public/components/sidebar.html")
    .then((res) => res.text())
    .then(
      (html) =>
        (document.getElementById("sidebar-placeholder").innerHTML = html)
    );
  fetch("/public/components/nav.html")
    .then((res) => res.text())
    .then(
      (html) => (document.getElementById("navbar-placeholder").innerHTML = html)
    );
}
function instructorsPage() {
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
    init() {
      this.fetchInstructors();
    },

    async fetchInstructors() {
      const params = new URLSearchParams();
      if (this.searchQuery) params.append("query", this.search);
      if (this.statusFilter) params.append("status", this.statusFilter);
      params.append("pageSize", this.pageSize);
      params.append("currentPage", this.currentPage);

      try {
        const res = await fetch(
          `https://localhost:7157/api/v1/Instructors/search?${params.toString()}`,
          {
            headers: {
              Authorization: "Bearer " + localStorage.getItem("accessToken"),
            },
          }
        );
        const data = await res.json();
        console.log("Response data:", data);
        if (data.status) {
          this.instructors = data.data.items;
          this.pagination = data.data;
          console.log(this.pagination);
          console.log(this.instructors);
        } else {
          this.instructors = [];
          this.pagination = {};
        }
      } catch (err) {
        console.error("Failed to fetch students:", err);
      }
    },

    async registerInstructor() {
      const payload = { ...this.newInstructor };

      const response = await fetch(
        "https://localhost:7157/api/v1/Instructors",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: "Bearer " + localStorage.getItem("token"),
          },
          body: JSON.stringify(payload),
        }
      );

      if (response.ok) {
        alert("Instructor registered successfully");
        this.showCreateModal = false;
        this.resetForm();
        this.fetchInstructors();
      } else {
        const error = await response.json();
        alert(error.message || "Failed to register instructor");
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
        this.fetchStudents();
      }
    },
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
    nextPage() {
      if (this.pagination.hasNextPage) {
        this.currentPage++;
        this.fetchStudents();
      }
    },

    viewInstructor(instructor) {
      window.location.href = `/public/admin/instructor-details.html?id=${instructor.id}`;
    },

    editInstructor(instructor) {
      alert("Edit modal not implemented yet.");
    },

    toggleStatus(instructor) {
      alert(`Toggling status for ${instructor.name}`);
    },

    openCreateModal() {
      this.showCreateModal = true;
      this.resetForm();
    },
  };
}
