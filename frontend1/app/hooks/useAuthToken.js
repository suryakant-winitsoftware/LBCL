"use client"
import { useEffect, useState } from 'react'
import { useAuth } from '../contexts/AuthContext'

export const useAuthToken = () => {
  const { user } = useAuth()
  const [token, setToken] = useState(null)

  useEffect(() => {
    // Get token from multiple sources in priority order
    const getToken = () => {
      // Priority 1: Auth context user token
      if (user?.token) {
        console.log('🔐 Token from auth context user')
        return user.token
      }

      // Priority 2: Direct from localStorage authToken
      const authToken = localStorage.getItem('authToken')
      if (authToken && authToken !== 'null' && authToken !== 'undefined') {
        console.log('🔐 Token from localStorage authToken')
        return authToken
      }

      // Priority 3: From currentUser in localStorage
      const currentUser = localStorage.getItem('currentUser')
      if (currentUser) {
        try {
          const userData = JSON.parse(currentUser)
          if (userData.token && userData.token !== 'null' && userData.token !== 'undefined') {
            console.log('🔐 Token from localStorage currentUser')
            return userData.token
          }
        } catch (e) {
          console.error('Error parsing currentUser:', e)
        }
      }

      console.warn('⚠️ No token found from any source')
      return null
    }

    const currentToken = getToken()
    setToken(currentToken)
  }, [user])

  return token
}

export default useAuthToken