import axios from 'axios';
import { useAuthStore } from '@/stores/useAuthStore';

const BASE_URL = 'http://localhost:5024';

const api = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor: attach token
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: handle 401 + refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const { accessToken, refreshToken } = useAuthStore.getState();
        const { data } = await axios.post(`${BASE_URL}/api/auth/refresh`, {
          accessToken,
          refreshToken,
        });
        console.log("token refreshed");
        useAuthStore.getState().updateTokens(data.accessToken, data.refreshToken);
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return api(originalRequest);
      } catch {
        useAuthStore.getState().logout();
        return Promise.reject(error);
      }
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  guestLogin: (guestName: string) =>
    api.post('/api/auth/guest', { guestName }),
  googleLogin: (googleIdToken: string) =>
    api.post('/api/auth/google', { idptoken:googleIdToken }),
};

export default api;
