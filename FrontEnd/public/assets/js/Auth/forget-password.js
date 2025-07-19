document.addEventListener("DOMContentLoaded", function () {
  const forgotPasswordForm = document.getElementById("forgotPasswordForm");

  if (forgotPasswordForm) {
    forgotPasswordForm.addEventListener("submit", async function (e) {
      e.preventDefault();
      const email = document.getElementById("email").value;
      const button = e.submitter;
      const originalButtonText = button.innerHTML;
      button.disabled = true;
      button.innerHTML = 'Sending...';

      try {
        await fetch('https://localhost:7157/api/v1/Auth/forgot-password', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          // The backend expects a raw string, not a JSON object for this endpoint.
          body: JSON.stringify(email) 
        });

        // Since the backend might return Ok() on failure, we just show a generic success message.
        // This is good practice to prevent email enumeration.
        Swal.fire({
          icon: "success",
          title: "Email Sent",
          text: `If an account with the email ${email} exists, a password reset link has been sent.`,
          confirmButtonColor: "#4f46e5"
        });

      } catch (error) {
        console.error("Forgot password error:", error);
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "An unexpected error occurred. Please try again later.",
          confirmButtonColor: "#ef4444"
        });
      } finally {
        button.disabled = false;
        button.innerHTML = originalButtonText;
      }
    });
  }
});