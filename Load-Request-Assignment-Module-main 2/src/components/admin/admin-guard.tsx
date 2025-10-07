"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuth } from "@/providers/auth-provider"
import { SkeletonLoader } from "@/components/ui/loader"

interface AdminGuardProps {
  children: React.ReactNode
}

export function AdminGuard({ children }: AdminGuardProps) {
  const router = useRouter()
  const { user, isLoading, isAuthenticated } = useAuth()

  useEffect(() => {
    if (!isLoading) {
      // Check if user is authenticated
      if (!isAuthenticated || !user) {
        router.push("/login?redirect=/admin")
        return
      }

      // Check if user has admin role
      const hasAdminRole = user.roles.some(role => 
        role.isAdmin || 
        role.roleNameEn?.toLowerCase().includes('admin') ||
        role.code?.toLowerCase().includes('admin')
      )

      // For development, also check if the user has any web user role
      // This allows testing the admin panel without strict admin role
      const hasWebAccess = user.roles.some(role => role.isWebUser)

      console.log('Admin access check:', {
        user: user.loginId,
        roles: user.roles.map(r => ({ 
          name: r.roleNameEn, 
          code: r.code, 
          isAdmin: r.isAdmin,
          isWebUser: r.isWebUser 
        })),
        hasAdminRole,
        hasWebAccess
      })

      if (!hasAdminRole && !hasWebAccess) {
        // User doesn't have admin or web access
        router.push("/dashboard?error=unauthorized&message=Admin or Web access required")
      }
    }
  }, [isLoading, isAuthenticated, user, router])

  // Show loading state while checking auth
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center space-y-4">
          <SkeletonLoader className="h-8 w-8 rounded-full mx-auto" />
          <p className="text-gray-600">Checking permissions...</p>
        </div>
      </div>
    )
  }

  // Don't render anything if not authenticated or doesn't have access
  const hasAccess = user && (
    user.roles.some(role => role.isAdmin) || 
    user.roles.some(role => role.isWebUser)
  )
  
  if (!isAuthenticated || !user || !hasAccess) {
    return null
  }

  return <>{children}</>
}