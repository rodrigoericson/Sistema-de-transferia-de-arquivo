import axios from 'axios';

const api = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('sta_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      sessionStorage.removeItem('sta_token');
      sessionStorage.removeItem('sta_username');
      sessionStorage.removeItem('sta_role');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
