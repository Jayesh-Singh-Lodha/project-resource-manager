import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserRole } from '../types';

interface AuthState {
  token: string | null;
  role: UserRole | null;
  fullName: string | null;
  forcePasswordChange: boolean;
  isAuthenticated: boolean;

  login: (token: string, role: UserRole, fullName: string, forcePasswordChange: boolean) => void;
  logout: () => void;
  clearForcePasswordChange: () => void;
}

/**
 * Auth store persisted to localStorage.
 * Survives page refreshes — user stays logged in until token expires or they logout.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      role: null,
      fullName: null,
      forcePasswordChange: false,
      isAuthenticated: false,

      login: (token, role, fullName, forcePasswordChange) =>
        set({
          token,
          role,
          fullName,
          forcePasswordChange,
          isAuthenticated: true,
        }),

      logout: () =>
        set({
          token: null,
          role: null,
          fullName: null,
          forcePasswordChange: false,
          isAuthenticated: false,
        }),

      clearForcePasswordChange: () =>
        set({ forcePasswordChange: false }),
    }),
    {
      name: 'prm-auth',
    }
  )
);
