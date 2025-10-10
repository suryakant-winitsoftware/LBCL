"use client"

import { useState, useEffect } from "react"
import { ArrowLeft, Eye, EyeOff, Key, Smartphone, Monitor } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible"
import { SkeletonLoader } from "@/components/ui/loader"
import { useToast } from "@/components/ui/use-toast"
import { Employee, ModuleHierarchy, Permission } from "@/types/admin.types"
import { employeeService } from "@/services/admin/employee.service"
import { permissionService } from "@/services/admin/permission.service"

interface EmployeePermissionViewerProps {
  employeeId: string
  onBack: () => void
}

interface PermissionItem {
  module: string
  subModule: string
  page: string
  path: string
  permissions: {
    view: boolean
    add: boolean
    edit: boolean
    delete: boolean
    approve: boolean
    download: boolean
    full: boolean
  }
}

export function EmployeePermissionViewer({ employeeId, onBack }: EmployeePermissionViewerProps) {
  const { toast } = useToast()
  const [employee, setEmployee] = useState<Employee | null>(null)
  const [webPermissions, setWebPermissions] = useState<PermissionItem[]>([])
  const [mobilePermissions, setMobilePermissions] = useState<PermissionItem[]>([])
  const [loading, setLoading] = useState(true)
  const [expandedModules, setExpandedModules] = useState<Set<string>>(new Set())

  useEffect(() => {
    loadEmployeePermissions()
  }, [employeeId])

  const loadEmployeePermissions = async () => {
    try {
      setLoading(true)

      // Load employee data
      const empData = await employeeService.getEmployeeById(employeeId)
      setEmployee(empData.emp)

      // Get employee's role
      if (empData.jobPosition?.userRoleUID) {
        // Load web permissions
        const webPerms = await permissionService.getPermissionsByRole(
          empData.jobPosition.userRoleUID,
          'Web',
          true // Assuming principal role for now
        )

        // Load mobile permissions
        const mobilePerms = await permissionService.getPermissionsByRole(
          empData.jobPosition.userRoleUID,
          'Mobile',
          true
        )

        // Transform permissions to display format
        setWebPermissions(transformPermissions(webPerms))
        setMobilePermissions(transformPermissions(mobilePerms))
      }
    } catch (error) {
      console.error('Failed to load employee permissions:', error)
      toast({
        title: "Error",
        description: "Failed to load employee permissions. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const transformPermissions = (permissions: Permission[]): PermissionItem[] => {
    // Transform permissions with module hierarchy data from backend
    // Note: In production, this should get module hierarchy from the permission service
    return permissions.map(perm => {
      // Extract module info from permission data or use fallback
      const moduleInfo = getModuleInfoForPermission(perm.subSubModuleUID)
      
      return {
        module: moduleInfo.module || "System Module",
        subModule: moduleInfo.subModule || "General",
        page: moduleInfo.page || `Page ${perm.subSubModuleUID}`,
        path: moduleInfo.path || "#",
        permissions: {
          view: perm.viewAccess,
          add: perm.addAccess,
          edit: perm.editAccess,
          delete: perm.deleteAccess,
          approve: perm.approvalAccess,
          download: perm.downloadAccess,
          full: perm.fullAccess
        }
      }
    })
  }

  // Helper function to get module info for a permission
  // In production, this would use the actual module hierarchy API
  const getModuleInfoForPermission = (pageUID: string) => {
    // This is a fallback - in production this would come from backend API
    const moduleMap: Record<string, { module: string; subModule: string; page: string; path: string }> = {
      // These would be loaded from the backend API in production
    }
    
    return moduleMap[pageUID] || {
      module: "System",
      subModule: "General",
      page: `Permission ${pageUID}`,
      path: "#"
    }
  }

  const toggleModule = (moduleKey: string) => {
    const newExpanded = new Set(expandedModules)
    if (newExpanded.has(moduleKey)) {
      newExpanded.delete(moduleKey)
    } else {
      newExpanded.add(moduleKey)
    }
    setExpandedModules(newExpanded)
  }

  const groupPermissionsByModule = (permissions: PermissionItem[]) => {
    const grouped = permissions.reduce((acc, perm) => {
      const key = perm.module
      if (!acc[key]) {
        acc[key] = []
      }
      acc[key].push(perm)
      return acc
    }, {} as Record<string, PermissionItem[]>)

    return grouped
  }

  const renderPermissionBadges = (permissions: PermissionItem['permissions']) => {
    const badges = []
    
    if (permissions.full) {
      badges.push(
        <Badge key="full" className="bg-green-100 text-green-800">
          Full Access
        </Badge>
      )
    } else {
      if (permissions.view) badges.push(<Badge key="view" variant="secondary">View</Badge>)
      if (permissions.add) badges.push(<Badge key="add" variant="secondary">Add</Badge>)
      if (permissions.edit) badges.push(<Badge key="edit" variant="secondary">Edit</Badge>)
      if (permissions.delete) badges.push(<Badge key="delete" variant="destructive">Delete</Badge>)
      if (permissions.approve) badges.push(<Badge key="approve" variant="default">Approve</Badge>)
      if (permissions.download) badges.push(<Badge key="download" variant="outline">Download</Badge>)
    }

    return badges.length > 0 ? badges : [
      <Badge key="none" variant="secondary" className="bg-red-100 text-red-800">
        No Access
      </Badge>
    ]
  }

  const renderPermissionList = (permissions: PermissionItem[]) => {
    if (permissions.length === 0) {
      return (
        <div className="text-center py-8 text-muted-foreground">
          <EyeOff className="mx-auto h-12 w-12 mb-4 opacity-50" />
          <p>No permissions found for this platform</p>
        </div>
      )
    }

    const groupedPermissions = groupPermissionsByModule(permissions)

    return (
      <div className="space-y-4">
        {Object.entries(groupedPermissions).map(([module, modulePermissions]) => {
          const moduleKey = `${module}`
          const isExpanded = expandedModules.has(moduleKey)

          return (
            <Card key={module}>
              <Collapsible open={isExpanded} onOpenChange={() => toggleModule(moduleKey)}>
                <CollapsibleTrigger asChild>
                  <CardHeader className="cursor-pointer hover:bg-muted/50 transition-colors">
                    <div className="flex items-center justify-between">
                      <div>
                        <CardTitle className="text-lg">{module}</CardTitle>
                        <CardDescription>
                          {modulePermissions.length} permission{modulePermissions.length !== 1 ? 's' : ''}
                        </CardDescription>
                      </div>
                      <div className="flex items-center gap-2">
                        <Badge variant="outline">
                          {modulePermissions.length}
                        </Badge>
                        <Button variant="ghost" size="sm">
                          {isExpanded ? "Collapse" : "Expand"}
                        </Button>
                      </div>
                    </div>
                  </CardHeader>
                </CollapsibleTrigger>
                <CollapsibleContent>
                  <CardContent className="pt-0">
                    <div className="space-y-3">
                      {modulePermissions.map((perm, index) => (
                        <div key={index} className="border rounded-lg p-4">
                          <div className="flex items-start justify-between">
                            <div className="flex-1">
                              <h4 className="font-medium">{perm.page}</h4>
                              <p className="text-sm text-muted-foreground">{perm.subModule}</p>
                              <p className="text-xs text-muted-foreground font-mono mt-1">
                                {perm.path}
                              </p>
                            </div>
                            <div className="flex flex-wrap gap-1 ml-4">
                              {renderPermissionBadges(perm.permissions)}
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  </CardContent>
                </CollapsibleContent>
              </Collapsible>
            </Card>
          )
        })}
      </div>
    )
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <SkeletonLoader className="h-10 w-20" />
          <div>
            <SkeletonLoader className="h-6 w-48 mb-2" />
            <SkeletonLoader className="h-4 w-32" />
          </div>
        </div>
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Card key={i}>
              <CardHeader>
                <SkeletonLoader className="h-6 w-40" />
                <SkeletonLoader className="h-4 w-24" />
              </CardHeader>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="outline" size="sm" onClick={onBack}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to List
        </Button>
        <div>
          <h2 className="text-2xl font-bold">{employee?.name}</h2>
          <p className="text-muted-foreground">
            {employee?.loginId} • {employee?.code}
          </p>
        </div>
      </div>

      {/* Employee Info Card */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Key className="h-5 w-5" />
            Permission Overview
          </CardTitle>
          <CardDescription>
            View all permissions assigned to this employee through their role
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="flex items-center gap-3">
              <Monitor className="h-5 w-5 text-blue-600" />
              <div>
                <p className="font-medium">Web Platform</p>
                <p className="text-sm text-muted-foreground">
                  {webPermissions.length} permission{webPermissions.length !== 1 ? 's' : ''}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Smartphone className="h-5 w-5 text-green-600" />
              <div>
                <p className="font-medium">Mobile Platform</p>
                <p className="text-sm text-muted-foreground">
                  {mobilePermissions.length} permission{mobilePermissions.length !== 1 ? 's' : ''}
                </p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Permissions by Platform */}
      <Tabs defaultValue="web" className="w-full">
        <TabsList>
          <TabsTrigger value="web" className="flex items-center gap-2">
            <Monitor className="h-4 w-4" />
            Web Permissions ({webPermissions.length})
          </TabsTrigger>
          <TabsTrigger value="mobile" className="flex items-center gap-2">
            <Smartphone className="h-4 w-4" />
            Mobile Permissions ({mobilePermissions.length})
          </TabsTrigger>
        </TabsList>
        
        <TabsContent value="web" className="mt-6">
          {renderPermissionList(webPermissions)}
        </TabsContent>
        
        <TabsContent value="mobile" className="mt-6">
          {renderPermissionList(mobilePermissions)}
        </TabsContent>
      </Tabs>
    </div>
  )
}