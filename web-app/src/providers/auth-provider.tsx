"use client"

import { createContext, useContext, useEffect, ReactNode } from "react"
import { useAuthStore } from "@/stores/auth-store"
import { User } from "@/types/auth.types"

interface AuthContextType {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
  login: (credentials: { loginId: string; password: string; rememberMe: boolean }) => Promise<{ success: boolean; user?: User }>
  logout: () => Promise<void>
  refreshToken: () => Promise<boolean>
  validateSession: () => Promise<boolean>
  updateUser: (user: User) => void
  clearError: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const {
    user,
    isAuthenticated,
    isLoading,
    error,
    login,
    logout,
    refreshToken,
    validateSession,
    updateUser,
    clearError,
    initializeAuth
  } = useAuthStore()

  useEffect(() => {
    // Initialize authentication on app start
    initializeAuth()
  }, [initializeAuth])

  const contextValue: AuthContextType = {
    user,
    isAuthenticated,
    isLoading,
    error,
    login,
    logout,
    refreshToken,
    validateSession,
    updateUser,
    clearError
  }

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider")
  }
  return context
}

// Custom hook for protected routes
export function useAuthGuard() {
  const { isAuthenticated, isLoading, user } = useAuth()
  
  return {
    isAuthenticated,
    isLoading,
    user,
    canAccess: isAuthenticated && !isLoading,
    needsAuth: !isAuthenticated && !isLoading
  }
}