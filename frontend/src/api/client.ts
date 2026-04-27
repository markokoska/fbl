import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
});

api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('fbl_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      sessionStorage.removeItem('fbl_token');
      sessionStorage.removeItem('fbl_user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
