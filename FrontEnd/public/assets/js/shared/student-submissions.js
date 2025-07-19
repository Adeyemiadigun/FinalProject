
async function loadComponent(id, path) {
  const res = await fetch(path);
  const html = await res.text();
  document.getElementById(id).innerHTML = html;
}
function adminStudentSubmissions() {
  return {
    studentId: new URLSearchParams(window.location.search).get("id"),
    submissions: [],
    studentName: "",
    sidebarOpen: true,
    async init() {
      const token = localStorage.getItem("accessToken");
      const role = localStorage.getItem("userRole");
      const sidebar =
        role == "Admin"
          ? "../components/sidebar.html"
          : "../components/instructor-sidebar.html";
      const navbar =
        role == "Admin"
          ? "../components/nav.html"
          : "../components/instructor-nav.html";

      await loadComponent("sidebar-placeholder", sidebar);
      await loadComponent("navbar-placeholder", navbar);
      const res = await fetch(
        `https://localhost:7157/api/v1/Students/${this.studentId}/submissions`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      const json = await res.json();
      this.submissions = json.data || [];
      this.studentName = this.submissions[0].studentName;
    },

    logOut() {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      window.location.href = "/public/auth/login.html";
    },
  };
}
