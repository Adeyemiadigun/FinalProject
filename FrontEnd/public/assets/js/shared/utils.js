const API_BASE_URL = "https://localhost:7157/api/v1";

/**
 * A simple API wrapper for making authenticated requests.
 */
const api = {
  async request(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem("accessToken");
    const headers = {
      'Content-Type': 'application/json',
    };
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const config = { method, headers };

    if (body) {
      config.body = JSON.stringify(body);
    }

    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
      // This could be enhanced to handle token refreshing on 401 errors
      return response; // Return the full response for flexibility
    } catch (error) {
      console.error(`API request failed: ${method} ${endpoint}`, error);
      throw error;
    }
  },

  get: function(endpoint) { return this.request(endpoint, 'GET'); },
  post: function(endpoint, body) { return this.request(endpoint, 'POST', body); },
  put: function(endpoint, body) { return this.request(endpoint, 'PUT', body); },
  delete: function(endpoint) { return this.request(endpoint, 'DELETE'); }
};

/**
 * A reusable function to load HTML components into placeholders.
 */
async function loadComponent(id, path) {
  try {
    const res = await fetch(path);
    if (res.ok) document.getElementById(id).innerHTML = await res.text();
  } catch (error) {
    console.error(`Failed to load component from ${path}:`, error);
  }
}

function logOut() {
  localStorage.clear();
  window.location.href = "/public/auth/login.html";
}