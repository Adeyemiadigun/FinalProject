import { api, loadComponent, logOut } from "../shared/utils.js";

window.instructorDetailsPage = function () {
  return {
    instructor: {},
    assessments: [],
    currentPage: 1,
    pageSize: 10,
    totalPages: 1,
    sidebarOpen: true,

    async init() {
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");

      const id = new URLSearchParams(window.location.search).get("id");
      this.instructorId = id;

      if (!id) {
        Swal.fire({
          icon: "error",
          title: "Invalid Access",
          text: "Instructor ID is missing in the URL.",
        });
        return;
      }

      try {
        const [profileRes] = await Promise.all([
          api.get(`/Instructors/${id}/details`),
        ]);

        if (!profileRes.ok) {
          const err = await profileRes.json();
          throw new Error(err?.detail || "Failed to load instructor details.");
        }

        const profileData = await profileRes.json();
        this.instructor = profileData.data;

        await this.fetchAssessments(); // fetch page 1
      } catch (err) {
        console.error(err);
        Swal.fire({
          icon: "error",
          title: "Loading Error",
          text: err.message || "Unable to load instructor details.",
        });
      }
    },

    async fetchAssessments() {
      try {
        const res = await api.get(
          `/Instructors/${this.instructorId}/assessment/details?pageSize=${this.pageSize}&currentPage=${this.currentPage}`
        );

        if (!res.ok) {
          const err = await res.json();
          throw new Error(err?.detail || "Failed to load assessments.");
        }

        const data = await res.json();
        this.assessments = data.data?.items || [];
        this.totalPages = data.data?.totalPages || 1;
      } catch (err) {
        console.error(err);
        Swal.fire({
          icon: "error",
          title: "Loading Error",
          text: err.message || "Unable to load assessments.",
        });
      }
    },

    nextPage() {
      if (this.currentPage < this.totalPages) {
        this.currentPage++;
        this.fetchAssessments();
      }
    },

    prevPage() {
      if (this.currentPage > 1) {
        this.currentPage--;
        this.fetchAssessments();
      }
    },

    formatDate(date) {
      if (!date) return "N/A";
      return new Date(date).toLocaleDateString();
    },

    logOut,
  };
};
