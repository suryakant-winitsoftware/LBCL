"use client"

import { useEffect } from "react"
import { useRouter, usePathname } from "next/navigation"
import { useAuth } from "@/providers/auth-provider"
import { usePermissions } from "@/providers/permission-provider"
import { AlertCircle, Lock } from "lucide-react"
import { SkeletonLoader } from "@/components/ui/loader"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

interface ProtectedRouteProps {
  children: React.ReactNode
  requireAuth?: boolean
  requiredPermission?: {
    subSubModuleUid?: string
    action?: 'view' | 'add' | 'edit' | 'delete' | 'download' | 'approval'
  }
  fallback?: React.ReactNode
  redirectTo?: string
}

export function ProtectedRoute({
  children,
  requireAuth = true,
  requiredPermission,
  fallback,
  redirectTo = "/login"
}: ProtectedRouteProps) {
  const router = useRouter()
  const pathname = usePathname()
  const { isAuthenticated, isLoading: authLoading } = useAuth()
  const { hasPermission, isLoading: permissionLoading } = usePermissions()
  
  useEffect(() => {
    // Only redirect if we're sure the user is not authenticated
    if (requireAuth && !authLoading && !isAuthenticated) {
      router.push(`${redirectTo}?from=${encodeURIComponent(pathname)}`)
    }
  }, [requireAuth, authLoading, isAuthenticated, router, redirectTo, pathname])
  
  // Show loading only during initial auth check
  if (authLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center space-y-4">
          <SkeletonLoader className="h-8 w-48 rounded mx-auto" />
          <SkeletonLoader className="h-4 w-32 rounded mx-auto" />
        </div>
      </div>
    )
  }
  
  // If auth is required but user is not authenticated, show loading (redirect will happen)
  if (requireAuth && !isAuthenticated) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center space-y-4">
          <SkeletonLoader className="h-8 w-40 rounded mx-auto" />
          <SkeletonLoader className="h-4 w-28 rounded mx-auto" />
        </div>
      </div>
    )
  }
  
  // If no permission is required, show content
  if (!requiredPermission) {
    return <>{children}</>
  }
  
  // Check permissions
  const hasRequiredPermission = () => {
    if (permissionLoading) return true // Allow while loading
    
    if (requiredPermission.subSubModuleUid) {
      return hasPermission(
        requiredPermission.subSubModuleUid,
        requiredPermission.action || 'view'
      )
    }
    
    // For now, if no specific permission UID is provided, allow access
    return true
  }
  
  // Check permission
  if (!hasRequiredPermission()) {
    if (fallback) {
      return <>{fallback}</>
    }
    
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <Card className="w-full max-w-md shadow-lg">
          <CardHeader className="text-center">
            <div className="mx-auto h-16 w-16 bg-red-100 rounded-full flex items-center justify-center mb-4">
              <Lock className="h-8 w-8 text-red-600" />
            </div>
            <CardTitle className="text-2xl font-bold text-gray-900">
              Access Denied
            </CardTitle>
            <CardDescription className="text-gray-600 mt-2">
              You don&apos;t have permission to access this page
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
              <div className="flex">
                <AlertCircle className="h-5 w-5 text-amber-600 mt-0.5 mr-2 flex-shrink-0" />
                <div className="text-sm text-amber-800">
                  <p className="font-medium mb-1">Permission Required</p>
                  <p>
                    {requiredPermission?.action 
                      ? `You need "${requiredPermission.action}" permission for this resource.`
                      : "You need additional permissions to access this resource."}
                  </p>
                </div>
              </div>
            </div>
            <div className="flex gap-3">
              <Button
                variant="outline"
                className="flex-1"
                onClick={() => router.back()}
              >
                Go Back
              </Button>
              <Button
                className="flex-1"
                onClick={() => router.push("/dashboard")}
              >
                Go to Dashboard
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }
  
  // User has access
  return <>{children}</>
}

// Higher-order component for pages
export function withProtectedRoute<P extends object>(
  Component: React.ComponentType<P>,
  options?: Omit<ProtectedRouteProps, 'children'>
) {
  return function ProtectedComponent(props: P) {
    return (
      <ProtectedRoute {...options}>
        <Component {...props} />
      </ProtectedRoute>
    )
  }
}