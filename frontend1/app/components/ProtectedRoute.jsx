"use client"
import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '../contexts/AuthContext'

const ProtectedRoute = ({ children, requiredSystem }) => {
  const { user, loading, isAuthenticated } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!loading) {
      if (!isAuthenticated) {
        // Redirect to unified login page
        router.push('/user/unified-login')
        return
      }

      if (requiredSystem && user?.system !== requiredSystem) {
        // Allow admin users to access any portal without additional login
        if (user?.system === 'admin') {
          return
        }
        
        // User is authenticated but not for the right system - redirect to unified login
        router.push('/user/unified-login')
        return
      }
    }
  }, [user, loading, isAuthenticated, requiredSystem, router])

  if (loading) {
    return (
      <div style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: '#ffffff'
      }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{
            animation: 'spin 1s linear infinite',
            borderRadius: '50%',
            height: '32px',
            width: '32px',
            border: '2px solid #375AE6',
            borderTop: '2px solid transparent',
            margin: '0 auto 16px auto'
          }}></div>
          <p style={{ color: '#000000', fontSize: '16px' }}>Loading...</p>
        </div>
      </div>
    )
  }

  if (!isAuthenticated || (requiredSystem && user?.system !== requiredSystem && user?.system !== 'admin')) {
    return null // Will redirect via useEffect
  }

  return children
}

export default ProtectedRoute