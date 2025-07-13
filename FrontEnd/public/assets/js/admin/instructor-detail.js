function instructorDetailsPage() {
  return {
    instructor: {},
    assessments: [],

    async init() {
      const id = new URLSearchParams(window.location.search).get("id");
      const token = localStorage.getItem("accessToken");

      const [profileRes, assessmentRes] = await Promise.all([
        fetch(`http://localhost:5162/api/v1/Instructors/${id}/details`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch(
          `http://localhost:5162/api/v1/Instructors/${id}/assessment/details?pageSize=10&currentPage=1`,
          {
            headers: { Authorization: `Bearer ${token}` },
          }
        ),
      ]);

      const profileData = await profileRes.json();
      const assessmentsData = await assessmentRes.json();
      console.log(profileData);
      console.log(assessmentsData);
      this.instructor = profileData.data;
      console.log(this.instructor);
      this.assessments =
        assessmentsData.items || assessmentsData.data?.items || [];
    },
    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
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
