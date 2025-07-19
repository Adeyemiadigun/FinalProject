document.addEventListener("DOMContentLoaded", function () {
  const resetPasswordForm = document.getElementById("resetPasswordForm");

  if (resetPasswordForm) {
    resetPasswordForm.addEventListener("submit", async function (e) {
      e.preventDefault();

      const newPassword = document.getElementById("newPassword").value;
      const confirmPassword = document.getElementById("confirmPassword").value;
      const button = e.submitter;
      const originalButtonText = button.innerHTML;

      const params = new URLSearchParams(window.location.search);
      const email = params.get('email');
      const token = params.get('token');

      if (!email || !token) {
        return Swal.fire("Error", "Invalid reset link. Please request a new one.", "error");
      }

      if (newPassword !== confirmPassword) {
        return Swal.fire({
          icon: "error",
          title: "Password Mismatch",
          text: "Both passwords must match.",
          confirmButtonColor: "#ef4444"
        });
      }

      button.disabled = true;
      button.innerHTML = 'Resetting...';

      try {
        const response = await fetch('https://localhost:7157/api/v1/Auth/reset-password', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ email, token, newPassword })
        });

        const result = await response.json();

        if (!response.ok || !result.status) {
          throw new Error(result.message || 'Failed to reset password. The link may be invalid or expired.');
        }

        Swal.fire({
          icon: "success",
          title: "Password Reset",
          text: "Your password has been successfully updated!",
          confirmButtonColor: "#4f46e5"
        }).then(() => {
          window.location.href = "/public/auth/login.html";
        });
      } catch (error) {
        Swal.fire("Error", error.message, "error");
      } finally {
        button.disabled = false;
        button.innerHTML = originalButtonText;
      }
    });
  }
});