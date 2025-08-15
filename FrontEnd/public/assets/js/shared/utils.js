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
      console.log("API Error:", errorData);
    } catch {
      // Fallback if JSON parsing fails
    }
    Swal.fire({
      toast: true,
      position: "top-end",
      icon: "error",
      title: errorMessage,
      showConfirmButton: false,
      timer: 3000,
      timerProgressBar: true,
    });

    throw new Error(errorMessage);
  }

  return response;
}
const api = {
  async request(
    endpoint,
    method = "GET",
    body = null,
    contentType = "application/json"
  ) {
    const token = localStorage.getItem("accessToken");
    const headers = {};
    if (token) headers["Authorization"] = `Bearer ${token}`;
    if (contentType) headers["Content-Type"] = contentType;

    const config = { method, headers };

    if (body) {
      config.body = contentType ? JSON.stringify(body) : body;
    }

    const res = await fetch(`${API_BASE_URL}${endpoint}`, config);
    return await handleResponse(res);
  },

  get(endpoint) {
    return this.request(endpoint, "GET");
  },
  post(endpoint, body) {
    return this.request(endpoint, "POST", body);
  },
  postFormData(endpoint, formData) {
    return this.request(endpoint, "POST", formData,null); // contentType = null
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