"use client"

import React, { useState, useEffect } from "react"
import { useRouter, useParams } from "next/navigation"
import { 
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle 
} from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Skeleton } from "@/components/ui/skeleton"
import { useToast } from "@/components/ui/use-toast"
import { 
  ArrowLeft, 
  Edit, 
  Trash2,
  Calendar,
  Shield,
  Users,
  Crown,
  Building,
  Monitor,
  Smartphone,
  CheckCircle,
  XCircle,
  Eye,
  Settings,
  AlertTriangle,
  Clock
} from "lucide-react"
import { cn } from "@/lib/utils"
import { Role, ModuleHierarchy } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"
import { permissionService } from "@/services/admin/permission.service"
import { rolePermissionService } from "@/services/admin/role-permission.service"
import moment from "moment"

export default function RoleDetailView() {
  const router = useRouter()
  const params = useParams()
  const uid = params?.uid as string
  const { toast } = useToast()

  const [loading, setLoading] = useState(true)
  const [role, setRole] = useState<Role | null>(null)
  const [webModules, setWebModules] = useState<ModuleHierarchy[]>([])
  const [mobileModules, setMobileModules] = useState<ModuleHierarchy[]>([])
  const [selectedWebModules, setSelectedWebModules] = useState<string[]>([])
  const [selectedMobileModules, setSelectedMobileModules] = useState<string[]>([])

  useEffect(() => {
    if (uid) {
      loadRoleDetails()
    }
  }, [uid])

  const loadRoleDetails = async () => {
    if (!uid) return
    
    setLoading(true)
    try {
      const [roleData, webHierarchy, mobileHierarchy] = await Promise.all([
        roleService.getRoleById(uid),
        permissionService['getModuleHierarchy']('Web'),
        permissionService['getModuleHierarchy']('Mobile')
      ])

      setRole(roleData)
      setWebModules(Array.isArray(webHierarchy) ? webHierarchy : [])
      setMobileModules(Array.isArray(mobileHierarchy) ? mobileHierarchy : [])

      // Load permissions using the permission service
      if (roleData.IsWebUser) {
        try {
          const webPermissions = await rolePermissionService.loadRolePermissions(
            roleData.UID,
            "Web",
            roleData.IsPrincipalRole || false
          )
          console.log('Loaded Web Permissions for view:', webPermissions)
          setSelectedWebModules(webPermissions)
        } catch (e) {
          console.warn('Failed to load web permissions:', e)
        }
      }

      if (roleData.IsAppUser) {
        try {
          const mobilePermissions = await rolePermissionService.loadRolePermissions(
            roleData.UID,
            "Mobile",
            roleData.IsPrincipalRole || false
          )
          console.log('Loaded Mobile Permissions for view:', mobilePermissions)
          setSelectedMobileModules(mobilePermissions)
        } catch (e) {
          console.warn('Failed to load mobile permissions:', e)
        }
      }
    } catch (error: any) {
      console.error('Error loading role details:', error)
      toast({
        title: "Error",
        description: error.message || "Failed to load role details",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const handleEdit = () => {
    router.push(`/updatedfeatures/role-management/roles/edit/${uid}`)
  }

  const handleDelete = async () => {
    if (!uid || !role) return
    
    const confirmed = window.confirm('Are you sure you want to deactivate this role? This action will set the role as inactive.')
    if (!confirmed) return

    try {
      const updatedRole = {
        ...role,
        isActive: false,
        modifiedDate: new Date().toISOString()
      }
      
      await roleService.updateRole(updatedRole)
      toast({
        title: "Success",
        description: "Role deactivated successfully",
      })
      router.push('/updatedfeatures/role-management/roles/manage')
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "Failed to deactivate role",
        variant: "destructive",
      })
    }
  }

  const getStatusBadge = (isActive: boolean) => (
    <Badge 
      variant={isActive ? 'default' : 'secondary'}
      className={cn(
        isActive 
          ? 'bg-green-100 text-green-800 hover:bg-green-100' 
          : 'bg-red-100 text-red-800 hover:bg-red-100'
      )}
    >
      {isActive ? (
        <CheckCircle className="h-3 w-3 mr-1" />
      ) : (
        <XCircle className="h-3 w-3 mr-1" />
      )}
      {isActive ? 'Active' : 'Inactive'}
    </Badge>
  )

  const getRoleTypeBadges = (role: Role) => {
    const badges = []
    if (role.IsPrincipalRole) {
      badges.push(
        <Badge key="principal" variant="default" className="bg-blue-100 text-blue-800">
          <Crown className="h-3 w-3 mr-1" />
          Principal
        </Badge>
      )
    }
    if (role.IsDistributorRole) {
      badges.push(
        <Badge key="distributor" variant="default" className="bg-green-100 text-green-800">
          <Building className="h-3 w-3 mr-1" />
          Distributor
        </Badge>
      )
    }
    if (role.IsAdmin) {
      badges.push(
        <Badge key="admin" variant="destructive" className="bg-red-100 text-red-800">
          <Shield className="h-3 w-3 mr-1" />
          Administrator
        </Badge>
      )
    }
    return badges
  }

  const getPlatformBadges = (role: Role) => {
    const badges = []
    if (role.IsWebUser) {
      badges.push(
        <Badge key="web" variant="default" className="bg-blue-100 text-blue-800">
          <Monitor className="h-3 w-3 mr-1" />
          Web
        </Badge>
      )
    }
    if (role.IsAppUser) {
      badges.push(
        <Badge key="mobile" variant="default" className="bg-green-100 text-green-800">
          <Smartphone className="h-3 w-3 mr-1" />
          Mobile
        </Badge>
      )
    }
    return badges
  }

  const getSelectedModuleNames = (selectedUIDs: string[], moduleHierarchy: ModuleHierarchy[]) => {
    const names: string[] = []
    
    moduleHierarchy.forEach(module => {
      module.children?.forEach(subModule => {
        subModule.children?.forEach(page => {
          if (selectedUIDs.includes(page.UID)) {
            names.push(`${module.ModuleNameEn} > ${subModule.SubModuleNameEn} > ${page.SubSubModuleNameEn}`)
          }
        })
      })
    })
    
    return names
  }

  const renderModuleAccess = (platform: 'web' | 'mobile') => {
    const isWeb = platform === 'web'
    const modules = isWeb ? webModules : mobileModules
    const selectedModules = isWeb ? selectedWebModules : selectedMobileModules
    const platformName = isWeb ? 'Web' : 'Mobile'
    const icon = isWeb ? <Monitor className="h-4 w-4" /> : <Smartphone className="h-4 w-4" />
    
    if (selectedModules.length === 0) {
      return (
        <div className="text-center py-8 text-gray-500 border rounded-lg bg-gray-50">
          <p>No {platformName.toLowerCase()} modules assigned</p>
        </div>
      )
    }

    return (
      <div className="space-y-4">
        {modules.map(module => {
          // Check if module itself is selected or any of its children
          const isModuleSelected = selectedModules.includes(module.UID)
          const selectedSubModules = module.children?.filter(sm => 
            selectedModules.includes(sm.UID)
          ) || []
          const selectedPages = module.children?.flatMap(subModule => 
            subModule.children?.filter(page => selectedModules.includes(page.UID)) || []
          ) || []
          
          // Skip if nothing is selected in this module
          if (!isModuleSelected && selectedSubModules.length === 0 && selectedPages.length === 0) {
            return null
          }

          return (
            <div key={module.UID} className="border rounded-lg p-4">
              <div className="font-medium text-sm text-gray-900 border-b pb-2 mb-3 flex items-center gap-2">
                {icon}
                {module.ModuleNameEn}
                {isModuleSelected && (
                  <Badge variant="default" className="bg-green-100 text-green-800">Full Module Access</Badge>
                )}
                <Badge variant="secondary">
                  {isModuleSelected ? 'All' : (selectedSubModules.length + selectedPages.length)}
                </Badge>
              </div>
              
              {/* Show if entire module is selected */}
              {isModuleSelected && (
                <div className="p-2 bg-green-50 rounded border mb-3">
                  <div className="flex items-center gap-2 text-sm">
                    <CheckCircle className="h-4 w-4 text-green-600" />
                    <span className="font-medium">Full module access granted</span>
                  </div>
                </div>
              )}
              
              {/* Show sub-modules and pages if not entire module */}
              {!isModuleSelected && module.children?.map(subModule => {
                const isSubModuleSelected = selectedModules.includes(subModule.UID)
                const subModulePages = subModule.children?.filter(page => 
                  selectedModules.includes(page.UID)
                ) || []
                
                if (!isSubModuleSelected && subModulePages.length === 0) return null

                return (
                  <div key={subModule.UID} className="ml-4 mb-3">
                    <div className="font-medium text-xs text-gray-700 mb-2 flex items-center gap-2">
                      {subModule.SubModuleNameEn}
                      {isSubModuleSelected && (
                        <Badge variant="outline" className="text-xs">Full Access</Badge>
                      )}
                    </div>
                    
                    {isSubModuleSelected ? (
                      <div className="ml-4 p-2 bg-blue-50 rounded border">
                        <div className="flex items-center gap-2 text-sm">
                          <CheckCircle className="h-3 w-3 text-blue-600" />
                          <span>All pages in this sub-module</span>
                        </div>
                      </div>
                    ) : (
                      <div className="ml-4 space-y-1">
                        {subModulePages.map(page => (
                          <div key={page.UID} className="flex items-center gap-2 text-sm p-2 bg-green-50 rounded border">
                            <CheckCircle className="h-3 w-3 text-green-600" />
                            <span>{page.SubSubModuleNameEn}</span>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                )
              })}
            </div>
          )
        })}
      </div>
    )
  }

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex items-center gap-2">
          <Skeleton className="h-8 w-8" />
          <Skeleton className="h-8 w-48" />
        </div>
        <Card>
          <CardContent className="space-y-4 pt-6">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
          </CardContent>
        </Card>
      </div>
    )
  }

  if (!role) {
    return (
      <div className="container mx-auto py-6">
        <Card>
          <CardContent className="flex items-center justify-center py-20">
            <div className="text-center">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">Role not found</h3>
              <p className="text-gray-600 mb-4">The requested role could not be found.</p>
              <Button onClick={() => router.push('/updatedfeatures/role-management/roles/manage')}>
                Back to Roles
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => router.push('/updatedfeatures/role-management/roles/manage')}
            >
              <ArrowLeft className="h-4 w-4" />
            </Button>
            <h1 className="text-2xl font-bold">Role Details</h1>
          </div>
          <p className="text-muted-foreground">
            View complete role information and permissions
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={handleEdit}>
            <Edit className="h-4 w-4 mr-2" />
            Edit Role
          </Button>
          <Button variant="destructive" onClick={handleDelete}>
            <Trash2 className="h-4 w-4 mr-2" />
            Deactivate Role
          </Button>
        </div>
      </div>

      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="permissions">Permissions</TabsTrigger>
          <TabsTrigger value="web-access">Web Access</TabsTrigger>
          <TabsTrigger value="mobile-access">Mobile Access</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Basic Information */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Eye className="h-5 w-5" />
                  Basic Information
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Role Code</label>
                    <p className="text-lg font-semibold">{role.Code}</p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Role Name</label>
                    <p className="text-lg font-semibold">{role.RoleNameEn}</p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Description (Alias Name)</label>
                    <p className="text-sm text-gray-700">
                      {role.RoleNameOther || 'No description provided'}
                    </p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Status</label>
                    <div className="mt-1">
                      {getStatusBadge(role.IsActive !== false)}
                    </div>
                  </div>

                  <div>
                    <label className="text-sm font-medium text-gray-600">Role UID</label>
                    <p className="text-sm text-gray-500 font-mono">{role.UID}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Role Types & Platform Access */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Settings className="h-5 w-5" />
                  Role Configuration
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div>
                  <label className="text-sm font-medium text-gray-600 mb-2 block">Role Types</label>
                  <div className="flex flex-wrap gap-2">
                    {getRoleTypeBadges(role)}
                    {!role.IsPrincipalRole && !role.IsDistributorRole && !role.IsAdmin && (
                      <Badge variant="outline">Standard Role</Badge>
                    )}
                  </div>
                </div>

                <Separator />

                <div>
                  <label className="text-sm font-medium text-gray-600 mb-2 block">Platform Access</label>
                  <div className="flex flex-wrap gap-2">
                    {getPlatformBadges(role)}
                    {!role.IsWebUser && !role.IsAppUser && (
                      <Badge variant="outline" className="text-red-600">No Platform Access</Badge>
                    )}
                  </div>
                </div>

                {role.IsAdmin && (
                  <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                    <div className="flex items-center gap-2 text-red-800">
                      <AlertTriangle className="h-4 w-4" />
                      <span className="font-medium">Administrator Role</span>
                    </div>
                    <p className="text-red-700 text-sm mt-1">
                      This role has complete administrative access to the system.
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Additional Settings */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Settings className="h-5 w-5" />
                  Additional Settings
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Parent Role</label>
                    <p className="text-sm text-gray-700">
                      {role.ParentRoleName || role.ParentRoleUid || 'No parent role'}
                    </p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Organization</label>
                    <p className="text-sm text-gray-700">
                      {role.OrgUid || 'WINIT'}
                    </p>
                  </div>
                </div>

                <Separator />
                
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div>
                      <label className="text-sm font-medium text-gray-700">Have Warehouse</label>
                      <p className="text-xs text-gray-500">Can manage warehouse operations</p>
                    </div>
                    {role.HaveWarehouse ? (
                      <CheckCircle className="h-5 w-5 text-green-600" />
                    ) : (
                      <XCircle className="h-5 w-5 text-gray-400" />
                    )}
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div>
                      <label className="text-sm font-medium text-gray-700">Have Vehicle</label>
                      <p className="text-xs text-gray-500">Can manage vehicle operations</p>
                    </div>
                    {role.HaveVehicle ? (
                      <CheckCircle className="h-5 w-5 text-green-600" />
                    ) : (
                      <XCircle className="h-5 w-5 text-gray-400" />
                    )}
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div>
                      <label className="text-sm font-medium text-gray-700">Reports To Access</label>
                      <p className="text-xs text-gray-500">Can configure reporting structure</p>
                    </div>
                    {role.IsForReportsTo ? (
                      <CheckCircle className="h-5 w-5 text-green-600" />
                    ) : (
                      <XCircle className="h-5 w-5 text-gray-400" />
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Module Access Summary */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Shield className="h-5 w-5" />
                Module Access Summary
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Monitor className="h-4 w-4 text-blue-600" />
                    <label className="text-sm font-medium text-gray-600">Web Platform</label>
                  </div>
                  <p className="text-2xl font-bold text-blue-600">{selectedWebModules.length}</p>
                  <p className="text-sm text-gray-500">modules assigned</p>
                </div>
                
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Smartphone className="h-4 w-4 text-green-600" />
                    <label className="text-sm font-medium text-gray-600">Mobile Platform</label>
                  </div>
                  <p className="text-2xl font-bold text-green-600">{selectedMobileModules.length}</p>
                  <p className="text-sm text-gray-500">modules assigned</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="permissions" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Shield className="h-5 w-5" />
                Permission Settings
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div>
                      <div className="flex items-center gap-2">
                        <Crown className="h-4 w-4 text-blue-600" />
                        <span className="font-medium">Principal Role</span>
                      </div>
                      <p className="text-sm text-gray-600">High-level organizational permissions</p>
                    </div>
                    {role.IsPrincipalRole ? (
                      <CheckCircle className="h-5 w-5 text-green-600" />
                    ) : (
                      <XCircle className="h-5 w-5 text-gray-400" />
                    )}
                  </div>

                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div>
                      <div className="flex items-center gap-2">
                        <Building className="h-4 w-4 text-green-600" />
                        <span className="font-medium">Distributor Role</span>
                      </div>
                      <p className="text-sm text-gray-600">Business distributor permissions</p>
                    </div>
                    {role.IsDistributorRole ? (
                      <CheckCircle className="h-5 w-5 text-green-600" />
                    ) : (
                      <XCircle className="h-5 w-5 text-gray-400" />
                    )}
                  </div>
                </div>

                <div className="space-y-4">
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div>
                      <div className="flex items-center gap-2">
                        <Shield className="h-4 w-4 text-red-600" />
                        <span className="font-medium">Administrator</span>
                      </div>
                      <p className="text-sm text-gray-600">Full system administrative access</p>
                    </div>
                    {role.IsAdmin ? (
                      <CheckCircle className="h-5 w-5 text-green-600" />
                    ) : (
                      <XCircle className="h-5 w-5 text-gray-400" />
                    )}
                  </div>

                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div>
                      <div className="flex items-center gap-2">
                        <Users className="h-4 w-4 text-purple-600" />
                        <span className="font-medium">Active Status</span>
                      </div>
                      <p className="text-sm text-gray-600">Role can be assigned to users</p>
                    </div>
                    {role.IsActive !== false ? (
                      <CheckCircle className="h-5 w-5 text-green-600" />
                    ) : (
                      <XCircle className="h-5 w-5 text-red-600" />
                    )}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="web-access" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Monitor className="h-5 w-5" />
                Web Platform Access ({selectedWebModules.length} modules)
              </CardTitle>
              <CardDescription>
                {role.IsWebUser 
                  ? "This role has access to the web platform with the following modules:" 
                  : "This role does not have web platform access."}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {role.IsWebUser ? (
                renderModuleAccess('web')
              ) : (
                <div className="text-center py-8 text-gray-500 border rounded-lg bg-gray-50">
                  <Monitor className="h-8 w-8 mx-auto mb-2 text-gray-400" />
                  <p>Web platform access is not enabled for this role</p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="mobile-access" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Smartphone className="h-5 w-5" />
                Mobile Platform Access ({selectedMobileModules.length} modules)
              </CardTitle>
              <CardDescription>
                {role.IsAppUser 
                  ? "This role has access to the mobile platform with the following modules:" 
                  : "This role does not have mobile platform access."}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {role.IsAppUser ? (
                renderModuleAccess('mobile')
              ) : (
                <div className="text-center py-8 text-gray-500 border rounded-lg bg-gray-50">
                  <Smartphone className="h-8 w-8 mx-auto mb-2 text-gray-400" />
                  <p>Mobile platform access is not enabled for this role</p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Metadata */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg flex items-center gap-2">
            <Clock className="h-5 w-5" />
            Role Metadata
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
            <div>
              <label className="text-gray-600">Created By</label>
              <p className="font-medium">{role.CreatedBy || 'Unknown'}</p>
            </div>
            
            {role.CreatedDate && (
              <div>
                <label className="text-gray-600">Created On</label>
                <p className="font-medium">{moment(role.CreatedDate).format('DD MMM YYYY, HH:mm')}</p>
              </div>
            )}
            
            <div>
              <label className="text-gray-600">Modified By</label>
              <p className="font-medium">{role.ModifiedBy || 'Unknown'}</p>
            </div>
            
            {role.ModifiedDate && (
              <div>
                <label className="text-gray-600">Last Modified</label>
                <p className="font-medium">{moment(role.ModifiedDate).format('DD MMM YYYY, HH:mm')}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}