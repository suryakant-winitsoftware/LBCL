import { cookies } from "next/headers"
import { redirect } from "next/navigation"
import React from "react"
import { authService } from '@/lib/auth-service'

export interface AdminUser {
  uid: string
  name: string
  loginId: string
  role: {
    uid: string
    code: string
    name: string
    isAdmin: boolean
    isWebUser: boolean
    isAppUser: boolean
  }
}

/**
 * Check if the current user has admin access
 * NOTE: This is a placeholder for server-side auth check
 * In production, you would validate the JWT token server-side
 */
export async function checkAdminAccess(): Promise<boolean> {
  // For now, we'll allow access and rely on client-side auth
  // The actual auth check happens in the client component
  return true
}

/**
 * Get current admin user from token
 */
export async function getCurrentAdminUser(): Promise<AdminUser | null> {
  try {
    const cookieStore = await cookies()
    const token = cookieStore.get('auth-token')?.value

    if (!token) {
      return null
    }

    const user = await getUserFromToken(token)
    
    if (!user || !user.role.isAdmin) {
      return null
    }

    return user
  } catch (error) {
    console.error('Error getting current admin user:', error)
    return null
  }
}

/**
 * Middleware to protect admin routes
 */
export async function requireAdminAccess() {
  const hasAccess = await checkAdminAccess()
  
  if (!hasAccess) {
    redirect("/dashboard?error=unauthorized&message=Admin access required")
  }
}

/**
 * Check specific admin permissions
 */
export async function checkAdminPermission(permission: string): Promise<boolean> {
  const user = await getCurrentAdminUser()
  
  if (!user) {
    return false
  }

  // For now, all admin users have all admin permissions
  // This can be extended later for granular admin permissions
  return user.role.isAdmin
}

/**
 * Decode JWT token and extract user information
 * In a real implementation, this would validate the token signature
 */
async function getUserFromToken(token: string): Promise<AdminUser | null> {
  try {
    // Simple JWT decode (in production, use a proper JWT library with signature verification)
    const payload = JSON.parse(atob(token.split('.')[1]))
    
    // Mock user data - in real implementation, this would come from the database
    // based on the token payload
    const mockUser: AdminUser = {
      uid: payload.sub || 'admin',
      name: payload.name || 'Administrator',
      loginId: payload.preferred_username || 'admin',
      role: {
        uid: 'ROLE_ADMIN',
        code: 'ADMIN',
        name: 'Administrator',
        isAdmin: true,
        isWebUser: true,
        isAppUser: false
      }
    }

    return mockUser
  } catch (error) {
    console.error('Error decoding token:', error)
    return null
  }
}

/**
 * Client-side hook for checking admin access
 * This should be used in client components
 */
export function useAdminAccess() {
  const checkAccess = async (): Promise<boolean> => {
    try {
      const token = authService.getToken()
      
      if (!token) {
        return false
      }

      // Make API call to verify admin access
      const response = await fetch('/api/auth/verify-admin', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })

      return response.ok
    } catch (error) {
      console.error('Error checking admin access:', error)
      return false
    }
  }

  return { checkAccess }
}

/**
 * Higher-order component for protecting admin pages
 */
export function withAdminAuth<T extends object>(
  WrappedComponent: React.ComponentType<T>
) {
  return function AdminProtectedComponent(props: T) {
    const { checkAccess } = useAdminAccess()
    
    // This would typically include loading states and redirect logic
    // For now, returning the component directly
    return <WrappedComponent {...props} />
  }
}

/**
 * Admin route configuration
 */
export const adminRoutes = {
  dashboard: '/administration',
  employees: '/administration/team-management/employees',
  roles: '/administration/access-control/roles',
  permissions: '/administration/access-control/permission-matrix',
  audit: '/administration/audit',
  settings: '/administration/settings'
} as const

/**
 * Check if a path is an admin route
 */
export function isAdminRoute(path: string): boolean {
  return path.startsWith('/administration')
}

/**
 * Get admin navigation items based on user permissions
 */
export async function getAdminNavigation(): Promise<AdminNavItem[]> {
  const user = await getCurrentAdminUser()
  
  if (!user) {
    return []
  }

  const baseNavigation: AdminNavItem[] = [
    {
      title: 'Dashboard',
      href: '/administration',
      icon: 'LayoutDashboard',
      description: 'Admin overview and statistics'
    },
    {
      title: 'Employee Management',
      href: '/administration/team-management/employees',
      icon: 'Users',
      description: 'Manage employees and user accounts'
    },
    {
      title: 'Role Management',
      href: '/administration/access-control/roles',
      icon: 'UserCheck',
      description: 'Create and manage user roles'
    },
    {
      title: 'Permission Management',
      href: '/administration/access-control/permission-matrix',
      icon: 'Key',
      description: 'Configure role-based permissions'
    },
    {
      title: 'Audit Trail',
      href: '/administration/audit',
      icon: 'FileText',
      description: 'View system activity logs'
    },
    {
      title: 'Settings',
      href: '/administration/settings',
      icon: 'Settings',
      description: 'System configuration'
    }
  ]

  // Filter navigation based on user permissions
  return baseNavigation.filter(item => {
    // For now, all admin users see all navigation items
    return true
  })
}

export interface AdminNavItem {
  title: string
  href: string
  icon: string
  description: string
  badge?: string
}