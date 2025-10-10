"use client"

import { createContext, useContext, useEffect, useState, ReactNode, useCallback } from "react"
import { useAuth } from "./auth-provider"
import { productionPermissionService } from "@/lib/production-permission-service"
import { clearAllCaches } from "@/lib/clear-cache"
import { 
  Module,
  SubModule,
  SubSubModule,
  ModulePermission 
} from "@/types/permission.types"

interface ModulesMasterView {
  modules: Module[]
  subModules: SubModule[]
  subSubModules: SubSubModule[]
}

interface PermissionContextType {
  permissions: ModulePermission[]
  modules: ModulesMasterView
  menuHierarchy: Module[]
  isLoading: boolean
  error: string | null
  
  // Permission checking methods
  hasPermission: (subSubModuleUid: string, action: 'view' | 'add' | 'edit' | 'delete' | 'download' | 'approval') => boolean
  hasAnyPermission: (subSubModuleUid: string) => boolean
  canShowInMenu: (subSubModuleUid: string) => boolean
  
  // Data fetching methods
  refreshPermissions: () => Promise<void>
  refreshMenu: () => Promise<void>
  getPermissionForPage: (relativePath: string) => Promise<ModulePermission | null>
}

const PermissionContext = createContext<PermissionContextType | undefined>(undefined)

interface PermissionProviderProps {
  children: ReactNode
}

export function PermissionProvider({ children }: PermissionProviderProps) {
  const { user, isAuthenticated, isLoading: authLoading } = useAuth()
  const [permissions, setPermissions] = useState<ModulePermission[]>([])
  const [modules, setModules] = useState<ModulesMasterView>({
    modules: [],
    subModules: [],
    subSubModules: []
  })
  const [menuHierarchy, setMenuHierarchy] = useState<Module[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  
  // Load permissions and modules when user logs in
  const loadPermissions = useCallback(async () => {
    // Don't try to load if auth is still loading
    if (authLoading) {
      return
    }
    
    if (!isAuthenticated || !user?.roles?.[0]) {
      setPermissions([])
      setModules({ modules: [], subModules: [], subSubModules: [] })
      setMenuHierarchy([])
      setIsLoading(false)
      return
    }
    
    setIsLoading(true)
    setError(null)
    
    try {
      const currentRole = user.roles[0]
      const platform = 'Web' // Default to Web, can be made dynamic
      
      // Use the role code for API calls (backend expects 'Admin' not 'admin-role')
      const roleIdentifier = currentRole.code || currentRole.uid
      
      // Fetch modules and permissions in parallel
      const [modulesMaster, userPermissions] = await Promise.all([
        productionPermissionService.getModulesMaster(platform),
        productionPermissionService.getPermissionsByRole(
          roleIdentifier,
          platform,
          currentRole.isPrincipalRole || false
        )
      ])
      
      // Permission data loaded successfully
      console.log('✅ Permission data loaded:', {
        modules: modulesMaster.modules?.length || 0,
        subModules: modulesMaster.subModules?.length || 0,
        subSubModules: modulesMaster.subSubModules?.length || 0,
        permissions: userPermissions?.length || 0
      })
      
      setModules(modulesMaster)
      setPermissions(userPermissions)
      
      // Build menu hierarchy
      const menu = productionPermissionService.buildMenuHierarchy(modulesMaster, userPermissions)
      
      // Menu hierarchy built successfully
      console.log('📊 Menu hierarchy built:', {
        totalModules: menu.length,
        modules: menu.map(m => ({
          uid: m.uid,
          name: m.moduleNameEn,
          childrenCount: m.children?.length || 0
        }))
      })
      
      setMenuHierarchy(menu)
      
    } catch (err) {
      // Error loading permissions
      setError(err instanceof Error ? err.message : "Failed to load permissions")
      
      // Clear data on error
      setPermissions([])
      setModules({ modules: [], subModules: [], subSubModules: [] })
      setMenuHierarchy([])
    } finally {
      setIsLoading(false)
    }
  }, [isAuthenticated, user, authLoading])
  
  useEffect(() => {
    loadPermissions()
  }, [loadPermissions])
  
  // Permission checking methods
  const hasPermission = useCallback((
    subSubModuleUid: string, 
    action: 'view' | 'add' | 'edit' | 'delete' | 'download' | 'approval'
  ): boolean => {
    return productionPermissionService.hasPermission(permissions, subSubModuleUid, action)
  }, [permissions])
  
  const hasAnyPermission = useCallback((subSubModuleUid: string): boolean => {
    const permission = permissions.find(p => p.subSubModuleUid === subSubModuleUid)
    if (!permission) return false
    
    return permission.fullAccess || 
           permission.viewAccess || 
           permission.addAccess || 
           permission.editAccess || 
           permission.deleteAccess || 
           permission.downloadAccess || 
           permission.approvalAccess
  }, [permissions])
  
  const canShowInMenu = useCallback((subSubModuleUid: string): boolean => {
    const permission = permissions.find(p => p.subSubModuleUid === subSubModuleUid)
    return permission ? permission.showInMenu && (permission.fullAccess || permission.viewAccess) : false
  }, [permissions])
  
  const getPermissionForPage = useCallback(async (relativePath: string): Promise<ModulePermission | null> => {
    if (!user?.roles?.[0]) return null
    
    // For production, we'll use the cached permissions instead of individual API calls
    const pagePermission = permissions.find(p => {
      // Find permission by matching relative path in subSubModules
      return modules.subSubModules?.some(ssm => 
        ssm.uid === p.subSubModuleUid && ssm.relativePath === relativePath
      )
    })
    return Promise.resolve(pagePermission || null)
  }, [user, permissions, modules])
  
  const refreshMenu = useCallback(async () => {
    if (!isAuthenticated || !user?.roles?.[0]) return
    
    setIsLoading(true)
    try {
      // First refresh backend menu cache
      await productionPermissionService.refreshMenuCache('Web')
      
      // Then reload permissions
      await loadPermissions()
    } catch (error) {
      console.error('Failed to refresh menu:', error)
    } finally {
      setIsLoading(false)
    }
  }, [isAuthenticated, user, loadPermissions])
  
  const contextValue: PermissionContextType = {
    permissions,
    modules,
    menuHierarchy,
    isLoading: isLoading || authLoading,
    error,
    hasPermission,
    hasAnyPermission,
    canShowInMenu,
    refreshPermissions: loadPermissions,
    refreshMenu,
    getPermissionForPage
  }
  
  return (
    <PermissionContext.Provider value={contextValue}>
      {children}
    </PermissionContext.Provider>
  )
}

export function usePermissions() {
  const context = useContext(PermissionContext)
  if (context === undefined) {
    throw new Error("usePermissions must be used within a PermissionProvider")
  }
  return context
}

// Permission guard hook for components
export function usePermissionGuard(
  subSubModuleUid: string,
  requiredAction: 'view' | 'add' | 'edit' | 'delete' | 'download' | 'approval' = 'view'
) {
  const { hasPermission, isLoading } = usePermissions()
  
  return {
    hasAccess: hasPermission(subSubModuleUid, requiredAction),
    isLoading
  }
}