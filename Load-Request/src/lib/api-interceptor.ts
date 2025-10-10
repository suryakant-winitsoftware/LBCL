import { authService } from "./auth-service"

// Create a wrapper for fetch that handles authentication errors
export const authenticatedFetch = async (
  url: string, 
  options: RequestInit = {}
): Promise<Response> => {
  const token = authService.getToken()
  
  // Add auth header if token exists
  if (token) {
    options.headers = {
      ...options.headers,
      "Authorization": `Bearer ${token}`
    }
  }
  
  try {
    const response = await fetch(url, options)
    
    // Handle authentication errors
    if (response.status === 401 || response.status === 403) {
      // Clear auth data
      authService.clearTokens()
      authService.clearUser()
      
      // Redirect to login
      if (typeof window !== "undefined") {
        window.location.href = "/login?reason=unauthorized"
      }
    }
    
    return response
  } catch (error) {
    throw error
  }
}

// Helper to clear auth data
export const clearAuthAndRedirect = (reason: string = "expired") => {
  authService.clearTokens()
  authService.clearUser()
  
  if (typeof window !== "undefined") {
    window.location.href = `/login?reason=${reason}`
  }
}