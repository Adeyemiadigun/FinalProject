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
    page: 1,
    perPage: 10,
    showCreateModal: false,
    newInstructor: {
      fullName: "",
      email: "",
      password: "",
      confirmPassword: "",
    },

    init() {
      this.fetchInstructors();
    },

    fetchInstructors() {
      fetch("/api/admin/instructors", {
        headers: { Authorization: "Bearer " + localStorage.getItem("token") },
      })
        .then((res) => res.json())
        .then((data) => (this.instructors = data));
    },

    async registerInstructor() {
      const payload = { ...this.newInstructor };

      const response = await fetch("/api/admin/register-instructor", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: "Bearer " + localStorage.getItem("token"),
        },
        body: JSON.stringify(payload),
      });

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

    get filteredInstructors() {
      return this.instructors.filter(
        (i) =>
          (!this.search ||
            i.name.toLowerCase().includes(this.search.toLowerCase()) ||
            i.email.toLowerCase().includes(this.search.toLowerCase())) &&
          (!this.statusFilter || i.status === this.statusFilter)
      );
    },

    get paginatedInstructors() {
      const start = (this.page - 1) * this.perPage;
      return this.filteredInstructors.slice(start, start + this.perPage);
    },

    get totalPages() {
      return Math.ceil(this.filteredInstructors.length / this.perPage);
    },

    nextPage() {
      if (this.page < this.totalPages) this.page++;
    },
    prevPage() {
      if (this.page > 1) this.page--;
    },

    viewInstructor(instructor) {
      window.location.href = `/admin/instructor-details.html?id=${instructor.id}`;
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
