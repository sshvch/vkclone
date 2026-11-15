import axios from 'axios';

export const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5143/api';
export const USERS_API_URL = import.meta.env.VITE_USERS_API_URL || 'http://localhost:5260/api';
export const CHAT_API_URL = import.meta.env.VITE_CHAT_API_URL || 'http://localhost:5281/api';
export const CHAT_HUB_URL = import.meta.env.VITE_CHAT_HUB_URL || 'http://localhost:5281/hubs/chat';
export const CONTENT_API_URL = import.meta.env.VITE_CONTENT_API_URL || 'http://localhost:5119/api';

export const USERS_ORIGIN = USERS_API_URL.replace(/\/api\/?$/, '');

export function resolveAvatarUrl(url) {
  if (!url) return '';
  return url.startsWith('/') ? `${USERS_ORIGIN}${url}` : url;
}

export const tokenStorage = {
  get: () => localStorage.getItem('token'),
  save: (token) => localStorage.setItem('token', token),
  clear: () => localStorage.removeItem('token'),
};

function addAuthHeader(config) {
  const token = tokenStorage.get();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}

export const api = axios.create({ baseURL: API_URL });
api.interceptors.request.use(addAuthHeader);

export const usersApi = axios.create({ baseURL: USERS_API_URL });
usersApi.interceptors.request.use(addAuthHeader);

export const chatApi = axios.create({ baseURL: CHAT_API_URL });
chatApi.interceptors.request.use(addAuthHeader);

export const contentApi = axios.create({ baseURL: CONTENT_API_URL });
contentApi.interceptors.request.use(addAuthHeader);

export function getErrorMessage(error, fallback = 'Что-то пошло не так') {
  return error?.response?.data?.message || error?.message || fallback;
}
