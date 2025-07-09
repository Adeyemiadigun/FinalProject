
  document.getElementById('loginForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    const loginError = document.getElementById('loginError');
    loginError.classList.add('hidden');

    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
      });

      const data = await response.json();

      if (!response.ok || !data.status) {
        throw new Error('Invalid credentials');
      }

      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);

      // Decode JWT to get role
      const decoded = jwt_decode(data.accessToken);
      const role = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      localStorage.setItem('userRole', role);

      // Redirect based on role
      if (role === 'Admin') {
        window.location.href = '/admin/dashboard.html';
      } else if (role === 'Instructor') {
        window.location.href = '/instructor/dashboard.html';
      } else {
        window.location.href = '/student/dashboard.html';
      }

    } catch (error) {
      loginError.textContent = "Invalid credentials. Please try again.";
      loginError.classList.remove('hidden');
    }
  });
