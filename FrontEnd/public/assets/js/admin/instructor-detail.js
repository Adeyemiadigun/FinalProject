function instructorDetailsPage() {
  return {
    instructor: {},
    assessments: [],

    async init() {
      const id = new URLSearchParams(window.location.search).get("id");
      const token = localStorage.getItem("token");

      const [profileRes, assessmentRes] = await Promise.all([
        fetch(`/api/admin/instructors/${id}/details`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch(
          `/api/admin/instructors/${id}/assessment/details?pageSize=20&currentPage=1`,
          {
            headers: { Authorization: `Bearer ${token}` },
          }
        ),
      ]);

      const profileData = await profileRes.json();
      const assessmentsData = await assessmentRes.json();

      this.instructor = profileData;
      this.assessments =
        assessmentsData.items || assessmentsData.data?.items || [];
    },

    formatDate(date) {
      const d = new Date(date);
      return d.toLocaleDateString();
    },
  };
}

async function loadLayout() {
  const sidebar = await fetch("/public/components/sidebar.html").then((res) =>
    res.text()
  );
  document.getElementById("sidebar-placeholder").innerHTML = sidebar;

  const navbar = await fetch("/public/components/nav.html").then((res) =>
    res.text()
  );
  document.getElementById("navbar-placeholder").innerHTML = navbar;
}

document.addEventListener("DOMContentLoaded", loadLayout);
