document.addEventListener("DOMContentLoaded", function () {
  document
    .getElementById("loginForm")
    .addEventListener("submit", async function (e) {
      e.preventDefault();

      const email = document.getElementById("email").value.trim();
      const password = document.getElementById("password").value;

      const loginError = document.getElementById("loginError");
      loginError.classList.add("hidden");

      try {
        const response = await fetch(
          "http://localhost:5162/api/v1/auth/login",
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({ email, password }),
          }
        );

        const data = await response.json();

        if (!response.ok || !data.status) {
          throw new Error("Invalid credentials");
        }

        localStorage.setItem("accessToken", data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);

        const decoded = jwt_decode(data.accessToken);
        const role =
          decoded.role ||
          decoded[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ];
        localStorage.setItem("userRole", role);

        if (role === "Admin") {
          window.location.href = "/public/admin/dashboard.html";
        } else if (role === "Instructor") {
          window.location.href = "/public/instructor/instructor-dashboard.html";
        } else {
          window.location.href = "/public/student/student-dashboard.html";
        }
      } catch (error) {
        console.error("Login failed:", error);
        loginError.textContent = "Invalid credentials. Please try again.";
        loginError.classList.remove("hidden");
      }
    });
});
