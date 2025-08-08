const API_BASE_URL = "https://localhost:7157/api/v1";

async function handleResponse(response) {
  if (response.status === 401 || response.status === 403) {
    // Unauthorized or Forbidden → log out user
    Swal.fire({
      icon: "warning",
      title: "Session Expired",
      text: "Your session has expired. Please log in again.",
    }).then(() => {
      logOut();
    });
    throw new Error("Unauthorized");
  }

  if (!response.ok) {
    // Try to parse ProblemDetails JSON
    let errorMessage = "Something went wrong.";
    try {
      const errorData = await response.json();
      errorMessage = errorData.detail || errorData.title || errorMessage;
    } catch {
      // Fallback if JSON parsing fails
    }
    Swal.fire({
      icon: "error",
      title: "Request Failed",
      text: errorMessage,
    });
    throw new Error(errorMessage);
  }

  return response;
}

 const api = {
  async request(endpoint, method = "GET", body = null) {
    const token = localStorage.getItem("accessToken");
    const headers = {
      "Content-Type": "application/json",
    };
    if (token) headers["Authorization"] = `Bearer ${token}`;

    const config = { method, headers };
    if (body) config.body = JSON.stringify(body);

    try {
      const res = await fetch(`${API_BASE_URL}${endpoint}`, config);
      return await handleResponse(res); // central error handler
    } catch (error) {
      console.error(`API request failed: ${method} ${endpoint}`, error);
      throw error;
    }
  },

  get(endpoint) {
    return this.request(endpoint, "GET");
  },
  post(endpoint, body) {
    return this.request(endpoint, "POST", body);
  },
  put(endpoint, body) {
    return this.request(endpoint, "PUT", body);
  },
  delete(endpoint) {
    return this.request(endpoint, "DELETE");
  },
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
export { api, loadComponent, logOut };