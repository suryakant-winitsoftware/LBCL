'use client'

import { useEffect, useState } from 'react'
import { usePathname } from 'next/navigation'
import { usePermissions } from '@/providers/permission-provider'

interface PagePermissions {
  canView: boolean
  canAdd: boolean
  canEdit: boolean
  canDelete: boolean
  canDownload: boolean
  canApprove: boolean
  moduleUid?: string
  isLoading: boolean
}

/**
 * Hook to get permissions for the current page based on the URL path
 * This dynamically resolves the module UID from the navigation path
 */
export function usePagePermissions(): PagePermissions {
  const pathname = usePathname()
  const { permissions, modules, hasPermission, isLoading: permissionsLoading } = usePermissions()
  const [pagePermissions, setPagePermissions] = useState<PagePermissions>({
    canView: false,
    canAdd: false,
    canEdit: false,
    canDelete: false,
    canDownload: false,
    canApprove: false,
    isLoading: true
  })

  useEffect(() => {
    console.log('🔍 usePagePermissions Debug - Start:', {
      pathname,
      permissionsLoading,
      hasModules: !!modules?.subSubModules,
      moduleCount: modules?.subSubModules?.length || 0
    })

    if (permissionsLoading || !modules?.subSubModules) {
      console.log('⏸️ usePagePermissions - Waiting for data...')
      return
    }

    // Extract the relative path from the full pathname
    // Remove leading slashes and convert to relative path format
    let relativePath = pathname.replace(/^\//, '')
    
    // Handle special cases for administration paths
    if (relativePath.startsWith('administration/')) {
      relativePath = relativePath.replace('administration/', '')
    }
    
    console.log('🛤️ Path transformation:', {
      originalPath: pathname,
      relativePath,
      availablePaths: modules.subSubModules.map(ssm => ssm.relativePath).slice(0, 10)
    })
    
    // Extract the last meaningful segment from the path
    const lastSegment = pathname.split('/').pop()
    
    console.log('🔍 Path analysis:', {
      pathname,
      relativePath, 
      lastSegment,
      availableRelativePaths: modules.subSubModules.map(ssm => ssm.relativePath)
    })
    
    // Find the matching module using multiple strategies
    let matchingModule = null
    
    // Strategy 1: Direct exact match with relative path
    matchingModule = modules.subSubModules.find(ssm => ssm.relativePath === relativePath)
    if (matchingModule) {
      console.log('✅ Strategy 1 - Direct match found:', matchingModule)
    } else {
      console.log('❌ Strategy 1 - No direct match for:', relativePath)
      
      // Strategy 2: Match by last segment
      matchingModule = modules.subSubModules.find(ssm => ssm.relativePath === lastSegment)
      if (matchingModule) {
        console.log('✅ Strategy 2 - Last segment match found:', matchingModule)
      } else {
        console.log('❌ Strategy 2 - No last segment match for:', lastSegment)
        
        // Strategy 3: Find modules that contain the last segment
        const containingModules = modules.subSubModules.filter(ssm => 
          ssm.relativePath?.includes(lastSegment || '') ||
          lastSegment?.includes(ssm.relativePath || '')
        )
        
        console.log('🎯 Strategy 3 - Modules containing segment:', containingModules.map(m => ({
          uid: m.uid,
          name: m.subSubModuleNameEn,
          relativePath: m.relativePath
        })))
        
        // Prefer exact matches over partial matches
        matchingModule = containingModules.find(ssm => ssm.relativePath === lastSegment) ||
                        containingModules.find(ssm => !ssm.relativePath?.includes('/')) ||
                        containingModules[0]
        
        if (matchingModule) {
          console.log('✅ Strategy 3 - Best containing match:', matchingModule)
        } else {
          console.log('❌ Strategy 3 - No containing match found')
        }
      }
    }

    if (matchingModule) {
      const moduleUid = matchingModule.uid
      
      console.log('🔑 Found module - checking permissions:', {
        moduleUid,
        moduleName: matchingModule.subSubModuleNameEn,
        relativePath: matchingModule.relativePath,
        totalPermissions: permissions.length,
        userPermissions: permissions.filter(p => p.subSubModuleUid === moduleUid)
      })
      
      const permissionChecks = {
        canView: hasPermission(moduleUid, 'view'),
        canAdd: hasPermission(moduleUid, 'add'),
        canEdit: hasPermission(moduleUid, 'edit'),
        canDelete: hasPermission(moduleUid, 'delete'),
        canDownload: hasPermission(moduleUid, 'download'),
        canApprove: hasPermission(moduleUid, 'approval'),
      }
      
      console.log('🎯 Permission results:', {
        moduleUid,
        permissions: permissionChecks,
        hasAnyPermission: Object.values(permissionChecks).some(Boolean)
      })
      
      setPagePermissions({
        canView: permissionChecks.canView,
        canAdd: permissionChecks.canAdd,
        canEdit: permissionChecks.canEdit,
        canDelete: permissionChecks.canDelete,
        canDownload: permissionChecks.canDownload,
        canApprove: permissionChecks.canApprove,
        moduleUid,
        isLoading: false
      })
    } else {
      console.log('❌ No direct match found - trying fallback...')
      
      // Try to find by matching the last segment of the path
      const lastSegment = pathname.split('/').pop()
      console.log('🔄 Fallback search:', { 
        lastSegment,
        availableLastSegments: modules.subSubModules.map(ssm => ({
          uid: ssm.uid,
          lastSegment: ssm.relativePath?.split('/').pop()
        })).slice(0, 10)
      })
      
      const fallbackModule = modules.subSubModules.find(ssm => {
        const ssmLastSegment = ssm.relativePath?.split('/').pop()
        return ssmLastSegment === lastSegment
      })
      
      if (fallbackModule) {
        const moduleUid = fallbackModule.uid
        console.log('✅ Fallback match found:', {
          moduleUid,
          moduleName: fallbackModule.subSubModuleNameEn,
          relativePath: fallbackModule.relativePath
        })
        
        const fallbackPermissionChecks = {
          canView: hasPermission(moduleUid, 'view'),
          canAdd: hasPermission(moduleUid, 'add'),
          canEdit: hasPermission(moduleUid, 'edit'),
          canDelete: hasPermission(moduleUid, 'delete'),
          canDownload: hasPermission(moduleUid, 'download'),
          canApprove: hasPermission(moduleUid, 'approval'),
        }
        
        console.log('🎯 Fallback permission results:', {
          moduleUid,
          permissions: fallbackPermissionChecks
        })
        
        setPagePermissions({
          canView: fallbackPermissionChecks.canView,
          canAdd: fallbackPermissionChecks.canAdd,
          canEdit: fallbackPermissionChecks.canEdit,
          canDelete: fallbackPermissionChecks.canDelete,
          canDownload: fallbackPermissionChecks.canDownload,
          canApprove: fallbackPermissionChecks.canApprove,
          moduleUid,
          isLoading: false
        })
      } else {
        console.log('🚫 No module found for path - defaulting to no access:', {
          pathname,
          relativePath,
          lastSegment
        })
        
        // No permissions found - default to no access
        setPagePermissions({
          canView: false,
          canAdd: false,
          canEdit: false,
          canDelete: false,
          canDownload: false,
          canApprove: false,
          isLoading: false
        })
      }
    }
  }, [pathname, permissions, modules, hasPermission, permissionsLoading])

  return pagePermissions
}