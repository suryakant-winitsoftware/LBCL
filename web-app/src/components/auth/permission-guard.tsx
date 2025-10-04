"use client"

import { usePermissions } from "@/providers/permission-provider"
import { ReactNode } from "react"

interface PermissionGuardProps {
  children: ReactNode
  subSubModuleUid: string
  action?: 'view' | 'add' | 'edit' | 'delete' | 'download' | 'approval'
  fallback?: ReactNode
  showError?: boolean
}

export function PermissionGuard({
  children,
  subSubModuleUid,
  action = 'view',
  fallback = null,
  showError = false
}: PermissionGuardProps) {
  const { hasPermission, isLoading } = usePermissions()
  
  if (isLoading) {
    return null // or a loading spinner
  }
  
  const hasAccess = hasPermission(subSubModuleUid, action)
  
  if (!hasAccess) {
    if (showError && fallback) {
      return <>{fallback}</>
    }
    return null
  }
  
  return <>{children}</>
}

// Utility components for common permission checks
export function CanView({ 
  children, 
  moduleUid,
  fallback 
}: { 
  children: ReactNode
  moduleUid: string
  fallback?: ReactNode 
}) {
  return (
    <PermissionGuard 
      subSubModuleUid={moduleUid} 
      action="view"
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  )
}

export function CanEdit({ 
  children, 
  moduleUid,
  fallback 
}: { 
  children: ReactNode
  moduleUid: string
  fallback?: ReactNode 
}) {
  return (
    <PermissionGuard 
      subSubModuleUid={moduleUid} 
      action="edit"
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  )
}

export function CanDelete({ 
  children, 
  moduleUid,
  fallback 
}: { 
  children: ReactNode
  moduleUid: string
  fallback?: ReactNode 
}) {
  return (
    <PermissionGuard 
      subSubModuleUid={moduleUid} 
      action="delete"
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  )
}

export function CanAdd({ 
  children, 
  moduleUid,
  fallback 
}: { 
  children: ReactNode
  moduleUid: string
  fallback?: ReactNode 
}) {
  return (
    <PermissionGuard 
      subSubModuleUid={moduleUid} 
      action="add"
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  )
}

export function CanDownload({ 
  children, 
  moduleUid,
  fallback 
}: { 
  children: ReactNode
  moduleUid: string
  fallback?: ReactNode 
}) {
  return (
    <PermissionGuard 
      subSubModuleUid={moduleUid} 
      action="download"
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  )
}

export function CanApprove({ 
  children, 
  moduleUid,
  fallback 
}: { 
  children: ReactNode
  moduleUid: string
  fallback?: ReactNode 
}) {
  return (
    <PermissionGuard 
      subSubModuleUid={moduleUid} 
      action="approval"
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  )
}