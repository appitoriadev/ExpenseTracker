/**
 * Base API client.
 *
 * When using the Vite dev proxy (default), set BASE_URL to '' so requests
 * go to the same origin and get forwarded to localhost:5157.
 * In production, set VITE_API_URL to your deployed backend URL.
 */
const BASE_URL = '';

const getToken = () => localStorage.getItem('et_token');

async function request(endpoint, options = {}) {
  const token = getToken();

  const response = await fetch(`${BASE_URL}${endpoint}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
    ...options,
  });

  // Token expired or invalid — clear storage and redirect to login
  if (response.status === 401) {
    localStorage.removeItem('et_token');
    sessionStorage.setItem('session_expired', '1');
    window.location.href = '/login';
    return;
  }

  // No content (e.g. DELETE 204)
  if (response.status === 204) return null;

  if (!response.ok) {
    const body = await response.json().catch(() => ({ message: response.statusText }));
    throw new Error(body.message || `Request failed with status ${response.status}`);
  }

  return response.json();
}

export const api = {
  get:    (url)         => request(url),
  post:   (url, body)   => request(url, { method: 'POST',   body: JSON.stringify(body) }),
  put:    (url, body)   => request(url, { method: 'PUT',    body: JSON.stringify(body) }),
  delete: (url)         => request(url, { method: 'DELETE' }),
};
