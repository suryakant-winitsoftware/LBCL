"use client"
import { createContext, useContext, useState, useEffect } from 'react'
import AuthService from '../services/auth'

const AuthContext = createContext()

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    // Check for existing session on mount
    const savedUser = localStorage.getItem('currentUser')
    const savedToken = localStorage.getItem('authToken')

    if (savedUser) {
      try {
        const parsedUser = JSON.parse(savedUser)
        setUser(parsedUser)

        // Console log restored session
        console.log('🔄 Auth Session Restored:', parsedUser);
        console.log('🔄 Restored Token:', savedToken);
      } catch (error) {
        console.error('Error parsing saved user:', error)
        localStorage.removeItem('currentUser')
        localStorage.removeItem('authToken')
      }
    }
    setLoading(false)
  }, [])

  const login = async (username, password, system) => {
    try {
      // For admin login, still use API authentication
      const apiResult = await AuthService.login(username, password)
      
      if (apiResult.success) {
        const generatedToken = apiResult.data.token || apiResult.data.Token || 
                              apiResult.data.access_token || apiResult.data.AccessToken || 
                              apiResult.data.authToken || apiResult.data.AuthToken ||
                              apiResult.data.jwt || apiResult.data.JWT;
        
        // Also check if token was already stored during the auth.js call
        const storedToken = localStorage.getItem('authToken');
        
        const finalToken = generatedToken || storedToken;

        // Console log the auth token
        console.log('🔐 Auth Token Generated:', finalToken);
        console.log('🔐 Generated Token:', generatedToken);
        console.log('🔐 Stored Token:', storedToken);

        if (!finalToken) {
          throw new Error('Authentication succeeded but no token was generated');
        }
        
        const userSession = {
          id: Math.random().toString(36).substring(2, 11),
          username: username,
          name: apiResult.data.user?.name || apiResult.data.User?.name || username,
          role: system === 'admin' ? 'super_admin' : (apiResult.data.user?.role || 'user'),
          system: system,
          loginTime: new Date().toISOString(),
          token: finalToken
        }

        setUser(userSession)
        localStorage.setItem('currentUser', JSON.stringify(userSession))
        localStorage.setItem('authToken', finalToken)

        // Console log when token is saved
        console.log('✅ Auth Token Saved to localStorage:', finalToken);
        console.log('👤 User Session Created:', userSession);
        
        return { success: true, user: userSession }
      } else {
        // API authentication failed
        throw new Error(apiResult.error || 'Authentication failed')
      }
    } catch (error) {
      return { success: false, error: error.message || 'Invalid credentials' }
    }
  }

  const logout = () => {
    const currentToken = localStorage.getItem('authToken');

    // Console log logout
    console.log('🚪 User Logout - Clearing token:', currentToken);

    setUser(null)
    localStorage.removeItem('currentUser')
    localStorage.removeItem('authToken')
    AuthService.logout()

    console.log('✅ Logout complete - All auth data cleared');
  }

  const value = {
    user,
    login,
    logout,
    loading,
    isAuthenticated: !!user,
    isStockUser: user?.system === 'stock',
    isDeliveryUser: user?.system === 'delivery',
    isAdminUser: user?.system === 'admin',
    isItineraryUser: user?.system === 'itinerary'
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}