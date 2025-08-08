import { api, loadComponent, logOut } from "../shared/utils.js";

window.instructorDetailsPage = function () {
  return {
    instructor: {},
    assessments: [],
    sidebarOpen: true,

    async init() {
      // Load layout components
      await loadComponent("sidebar-placeholder", "../components/sidebar.html");
      await loadComponent("navbar-placeholder", "../components/nav.html");

      const id = new URLSearchParams(window.location.search).get("id");
      if (!id) {
        Swal.fire({
          icon: "error",
          title: "Invalid Access",
          text: "Instructor ID is missing in the URL.",
        });
        return;
      }

      try {
        // Fetch profile and assessments in parallel
        const [profileRes, assessmentRes] = await Promise.all([
          api.get(`/Instructors/${id}/details`),
          api.get(
            `/Instructors/${id}/assessment/details?pageSize=10&currentPage=1`
          ),
        ]);

        // Handle unauthorized globally in api wrapper
        if (!profileRes.ok || !assessmentRes.ok) {
          const profileErr = await profileRes.json();
          const assessmentErr = await assessmentRes.json();
          throw new Error(
            profileErr?.detail ||
              assessmentErr?.detail ||
              "Failed to load instructor details."
          );
        }

        const profileData = await profileRes.json();
        const assessmentsData = await assessmentRes.json();

        this.instructor = profileData.data;
        this.assessments =
          assessmentsData.data?.items || assessmentsData.items || [];
      } catch (err) {
        console.error(err);
        Swal.fire({
          icon: "error",
          title: "Loading Error",
          text:
            err.message ||
            "Unable to load instructor details. Please try again later.",
        });
      }
    },

    formatDate(date) {
      if (!date) return "N/A";
      const d = new Date(date);
      return d.toLocaleDateString();
    },

    logOut, // Use shared logOut
  };
}
