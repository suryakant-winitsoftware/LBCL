import { create } from "zustand"
import { devtools } from "zustand/middleware"
import { authService } from "@/lib/auth-service"
import { 
  User, 
  LoginCredentials, 
  SessionInfo, 
  DeviceInfo,
  AuthState 
} from "@/types/auth.types"

interface AuthStore extends AuthState {
  // Actions
  login: (credentials: LoginCredentials) => Promise<{ success: boolean; user?: User }>
  logout: () => Promise<void>
  refreshToken: () => Promise<boolean>
  validateSession: () => Promise<boolean>
  getCurrentUser: () => User | null
  updateUser: (user: User) => void
  
  // Session Management
  sessions: SessionInfo[]
  loadSessions: () => Promise<void>
  revokeSession: () => Promise<boolean>
  
  // Device Management
  devices: DeviceInfo[]
  loadDevices: () => Promise<void>
  
  // State Management
  setLoading: (loading: boolean) => void
  setError: (error: string | null) => void
  clearError: () => void
  initializeAuth: () => Promise<void>
}

export const useAuthStore = create<AuthStore>()(
  devtools(
    (set, get) => {
      return {
        // Initial State - always start with loading true to avoid hydration mismatch
        user: null,
        isAuthenticated: false,
        isLoading: true, // Start with loading true
        error: null,
        sessionInfo: null,
        sessions: [],
        devices: [],

      // Authentication Actions
      login: async (credentials: LoginCredentials) => {
        set({ isLoading: true, error: null })

        try {
          const response = await authService.login(credentials)

          if (response.success) {
            console.log("🎉 Authentication successful in store!");
            console.log("🔐 Login completed for user:", response.user?.loginId);
            set({
              user: response.user,
              isAuthenticated: true,
              isLoading: false,
              error: null
            })
            return { success: true, user: response.user }
          } else {
            set({
              isLoading: false,
              error: response.error || "Login failed"
            })
            return { success: false }
          }
        } catch {
          set({
            isLoading: false,
            error: "Authentication service unavailable"
          })
          return { success: false }
        }
      },

      logout: async () => {
        set({ isLoading: true })
        
        try {
          await authService.logout()
          set({
            user: null,
            isAuthenticated: false,
            isLoading: false,
            error: null,
            sessionInfo: null,
            sessions: [],
            devices: []
          })
        } catch {
          set({ isLoading: false })
        }
      },

      refreshToken: async () => {
        try {
          const newToken = await authService.refreshToken()
          if (newToken) {
            return true
          } else {
            // Token refresh failed, logout user
            await get().logout()
            return false
          }
        } catch {
          await get().logout()
          return false
        }
      },

      validateSession: async () => {
        set({ isLoading: true })
        
        try {
          const isValid = await authService.validateSession()
          const user = authService.getCurrentUser()
          
          set({
            isAuthenticated: isValid && !!user,
            user: user,
            isLoading: false
          })
          
          return isValid
        } catch {
          set({
            isAuthenticated: false,
            user: null,
            isLoading: false,
            error: "Session validation failed"
          })
          return false
        }
      },

      getCurrentUser: () => {
        return authService.getCurrentUser()
      },

      updateUser: (user: User) => {
        // Update user in store and persist to localStorage
        set({ user })
        authService.setUser(user)
      },

      // Session Management
      loadSessions: async () => {
        try {
          const sessions = await authService.getActiveSessions()
          set({ sessions })
        } catch (error) {
          console.error("Failed to load sessions:", error)
        }
      },

      revokeSession: async () => {
        try {
          const success = await authService.revokeSession("current")
          if (success) {
            // Reload sessions to reflect changes
            await get().loadSessions()
          }
          return success
        } catch (error) {
          console.error("Failed to revoke session:", error)
          return false
        }
      },

      // Device Management
      loadDevices: async () => {
        try {
          const devices = await authService.getTrustedDevices()
          set({ devices })
        } catch (error) {
          console.error("Failed to load devices:", error)
        }
      },

      // State Management
      setLoading: (loading: boolean) => set({ isLoading: loading }),
      
      setError: (error: string | null) => set({ error }),
      
      clearError: () => set({ error: null }),

      initializeAuth: async () => {
        set({ isLoading: true })
        
        try {
          // First check if we have stored credentials
          const token = authService.getToken()
          const user = authService.getCurrentUser()
          
          if (token && user) {
            // We have stored credentials, validate session
            const isValid = await authService.validateSession()
            
            if (isValid) {
              set({
                isAuthenticated: true,
                user: user,
                isLoading: false
              })
            } else {
              // Session validation failed, clear auth state
              set({
                isAuthenticated: false,
                user: null,
                isLoading: false
              })
            }
          } else {
            // No stored credentials
            set({
              isAuthenticated: false,
              user: null,
              isLoading: false
            })
          }
        } catch (error) {
          set({
            isAuthenticated: false,
            user: null,
            isLoading: false,
            error: "Failed to initialize authentication"
          })
        }
      }
    }},
    {
      name: "auth-store"
    }
  )
)