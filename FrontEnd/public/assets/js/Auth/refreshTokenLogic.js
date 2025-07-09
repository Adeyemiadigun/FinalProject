export let refreshToken = async ()=>{
  let refreshToken = localStorage.getItem('refreshToken');
  if (!refreshToken) {
    return null;
  }
  await fetch('/api/auth/refresh-token',
  {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'  
  },
    body: JSON.stringify({ refreshToken })
  })
  .then(response => {
    if (!response.ok) {
      throw new Error('Failed to refresh token');
    }
    return response.json();
  })
  .then(data => {
    if (data.accessToken) {
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      return data.accessToken;
    } else {
      throw new Error('No access token returned');
    }
  })
  .catch(error => {
    console.error('Error refreshing token:', error);
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    window.location.href = '/auth/login.html'; // Redirect to login
  });
  return localStorage.getItem('accessToken');
}
