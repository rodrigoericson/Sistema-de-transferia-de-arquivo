import { create } from 'zustand';
import api from '../lib/api';
import type { ApiResponse, LoginResponse } from '../types';

interface AuthState {
  token: string | null;
  username: string | null;
  role: string | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
}

export const useAuth = create<AuthState>((set) => ({
  token: localStorage.getItem('sta_token'),
  username: localStorage.getItem('sta_username'),
  role: localStorage.getItem('sta_role'),
  isAuthenticated: !!localStorage.getItem('sta_token'),

  login: async (username: string, password: string) => {
    try {
      const { data } = await api.post<ApiResponse<LoginResponse>>('/auth/login', { username, password });
      if (data.success && data.data) {
        const { token, username: user, role } = data.data;
        localStorage.setItem('sta_token', token);
        localStorage.setItem('sta_username', user);
        localStorage.setItem('sta_role', role);
        set({ token, username: user, role, isAuthenticated: true });
        return true;
      }
      return false;
    } catch {
      return false;
    }
  },

  logout: () => {
    localStorage.removeItem('sta_token');
    localStorage.removeItem('sta_username');
    localStorage.removeItem('sta_role');
    set({ token: null, username: null, role: null, isAuthenticated: false });
  },
}));
