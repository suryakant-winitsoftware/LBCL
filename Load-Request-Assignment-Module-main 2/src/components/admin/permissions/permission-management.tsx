"use client"

import { useState, useCallback, useEffect } from "react"
import { useSearchParams } from "next/navigation"
import { Key, Monitor, Smartphone, Copy, Download, RefreshCw, Save, Undo } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { useToast } from "@/components/ui/use-toast"
import { PermissionMatrix } from "./permission-matrix"
import { BulkPermissionEditor } from "./bulk-permission-editor"
import { PermissionTemplates } from "./permission-templates"
import { PermissionCopyDialog } from "./permission-copy-dialog"
import { Role, PermissionMatrix as PermissionMatrixType } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"
import { permissionService } from "@/services/admin/permission.service"

export function PermissionManagement() {
  const { toast } = useToast()
  const searchParams = useSearchParams()
  const [selectedRoleId, setSelectedRoleId] = useState<string>("")
  const [selectedPlatform, setSelectedPlatform] = useState<"Web" | "Mobile">("Web")
  const [roles, setRoles] = useState<Role[]>([])
  const [loading, setLoading] = useState(true)
  const [permissionMatrix, setPermissionMatrix] = useState<PermissionMatrixType | null>(null)
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false)
  const [refreshTrigger, setRefreshTrigger] = useState(0)

  // Dialog states
  const [isCopyDialogOpen, setIsCopyDialogOpen] = useState(false)
  const [isBulkEditorOpen, setIsBulkEditorOpen] = useState(false)
  const [isTemplatesOpen, setIsTemplatesOpen] = useState(false)

  // Statistics
  const [stats, setStats] = useState({
    totalPermissions: 0,
    grantedPermissions: 0,
    webPermissions: 0,
    mobilePermissions: 0,
    fullAccessPages: 0,
    viewOnlyPages: 0
  })

  // Load roles on mount
  useEffect(() => {
    loadRoles()
    
    // Check for role parameter in URL
    const roleParam = searchParams.get('role')
    if (roleParam) {
      setSelectedRoleId(roleParam)
    }
  }, [searchParams])

  // Load permission matrix when role or platform changes
  useEffect(() => {
    if (selectedRoleId) {
      loadPermissionMatrix()
    }
  }, [selectedRoleId, selectedPlatform, refreshTrigger])

  const loadRoles = async () => {
    try {
      setLoading(true)
      const pagingRequest = roleService.buildRolePagingRequest(1, 1000)
      const response = await roleService.getRoles(pagingRequest)
      setRoles(response.pagedData.filter(role => role.IsActive))
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to load roles. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const loadPermissionMatrix = async () => {
    if (!selectedRoleId) return

    try {
      setLoading(true)
      
      const matrix = await permissionService.getPermissionMatrix(selectedRoleId, selectedPlatform)
      
      
      setPermissionMatrix(matrix)
      
      // Calculate statistics
      let totalPermissions = 0
      let grantedPermissions = 0
      let fullAccessPages = 0
      let viewOnlyPages = 0

      matrix.modules.forEach(module => {
        module.subModules.forEach(subModule => {
          subModule.pages.forEach(page => {
            totalPermissions++
            const perms = page.permissions
            
            if (perms.viewAccess || perms.addAccess || perms.editAccess || 
                perms.deleteAccess || perms.approvalAccess || perms.downloadAccess || perms.fullAccess) {
              grantedPermissions++
            }
            
            if (perms.fullAccess) {
              fullAccessPages++
            } else if (perms.viewAccess && !perms.addAccess && !perms.editAccess && !perms.deleteAccess) {
              viewOnlyPages++
            }
          })
        })
      })


      setStats(prev => ({
        ...prev,
        totalPermissions,
        grantedPermissions,
        fullAccessPages,
        viewOnlyPages,
        [selectedPlatform === 'Web' ? 'webPermissions' : 'mobilePermissions']: grantedPermissions
      }))

    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to load permissions. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const handleRoleChange = useCallback((roleId: string) => {
    if (hasUnsavedChanges) {
      if (!confirm("You have unsaved changes. Are you sure you want to change the role?")) {
        return
      }
    }
    setSelectedRoleId(roleId)
    setHasUnsavedChanges(false)
  }, [hasUnsavedChanges])

  const handlePlatformChange = useCallback((platform: "Web" | "Mobile") => {
    if (hasUnsavedChanges) {
      if (!confirm("You have unsaved changes. Are you sure you want to change the platform?")) {
        return
      }
    }
    setSelectedPlatform(platform)
    setHasUnsavedChanges(false)
  }, [hasUnsavedChanges])

  const handlePermissionChange = useCallback((updatedMatrix: PermissionMatrixType) => {
    setPermissionMatrix(updatedMatrix)
    setHasUnsavedChanges(true)
  }, [])

  const handleSavePermissions = useCallback(async () => {
    if (!permissionMatrix) return

    try {
      setLoading(true)
      await permissionService.updatePermissionMatrix(permissionMatrix)
      setHasUnsavedChanges(false)
      toast({
        title: "Success",
        description: "Permissions saved successfully.",
      })
      triggerRefresh()
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to save permissions. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }, [permissionMatrix, toast])

  const handleDiscardChanges = useCallback(() => {
    setHasUnsavedChanges(false)
    triggerRefresh()
  }, [])

  const handleCopyPermissions = useCallback(async (sourceRoleId: string, targetRoleId: string) => {
    try {
      await permissionService.copyPermissions(sourceRoleId, targetRoleId, selectedPlatform)
      toast({
        title: "Success",
        description: "Permissions copied successfully.",
      })
      if (targetRoleId === selectedRoleId) {
        triggerRefresh()
      }
      setIsCopyDialogOpen(false)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to copy permissions. Please try again.",
        variant: "destructive",
      })
    }
  }, [selectedPlatform, selectedRoleId, toast])

  const handleBulkPermissionUpdate = useCallback(async (
    pageUIDs: string[], 
    permissions: any
  ) => {
    try {
      await permissionService.bulkUpdatePermissions(
        selectedRoleId, 
        selectedPlatform, 
        pageUIDs, 
        permissions
      )
      toast({
        title: "Success",
        description: `Bulk permissions updated for ${pageUIDs.length} pages.`,
      })
      triggerRefresh()
      setIsBulkEditorOpen(false)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to update bulk permissions. Please try again.",
        variant: "destructive",
      })
    }
  }, [selectedRoleId, selectedPlatform, toast])

  const handleExportPermissions = useCallback(async () => {
    if (!selectedRoleId) return

    try {
      const blob = await permissionService.exportPermissionMatrix(
        selectedRoleId, 
        selectedPlatform, 
        'csv'
      )
      const url = URL.createObjectURL(blob)
      const a = document.createElement("a")
      a.href = url
      a.download = `permissions-${selectedRoleId}-${selectedPlatform}-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
      toast({
        title: "Success",
        description: "Permission matrix exported successfully.",
      })
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to export permissions. Please try again.",
        variant: "destructive",
      })
    }
  }, [selectedRoleId, selectedPlatform, toast])

  const triggerRefresh = useCallback(() => {
    setRefreshTrigger(prev => prev + 1)
  }, [])

  const selectedRole = roles.find(role => role.UID === selectedRoleId)

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Permission Management</h1>
          <p className="text-muted-foreground">
            Configure role-based permissions for pages and features
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button 
            variant="outline" 
            size="sm" 
            onClick={() => setIsTemplatesOpen(true)}
            disabled={!selectedRoleId}
          >
            Templates
          </Button>
          <Button 
            variant="outline" 
            size="sm" 
            onClick={() => setIsCopyDialogOpen(true)}
            disabled={!selectedRoleId}
          >
            <Copy className="mr-2 h-4 w-4" />
            Copy Permissions
          </Button>
          <Button 
            variant="outline" 
            size="sm" 
            onClick={handleExportPermissions}
            disabled={!selectedRoleId}
          >
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button 
            variant="outline" 
            size="sm" 
            onClick={triggerRefresh}
          >
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
        </div>
      </div>

      {/* Role and Platform Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Key className="h-5 w-5" />
            Permission Configuration
          </CardTitle>
          <CardDescription>
            Select a role and platform to configure permissions
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Role</label>
              <Select value={selectedRoleId} onValueChange={handleRoleChange}>
                <SelectTrigger>
                  <SelectValue placeholder="Select a role" />
                </SelectTrigger>
                <SelectContent>
                  {roles.map(role => (
                    <SelectItem key={role.UID} value={role.UID}>
                      <div className="flex items-center gap-2">
                        <span>{role.RoleNameEn}</span>
                        <div className="flex gap-1">
                          {role.IsPrincipalRole && (
                            <Badge variant="outline" className="text-xs">Principal</Badge>
                          )}
                          {role.IsAdmin && (
                            <Badge variant="destructive" className="text-xs">Admin</Badge>
                          )}
                        </div>
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {selectedRole && (
                <p className="text-xs text-muted-foreground">
                  Code: {selectedRole.Code} • 
                  {selectedRole.IsPrincipalRole ? ' Principal' : ''}
                  {selectedRole.IsDistributorRole ? ' Distributor' : ''}
                  {selectedRole.IsAdmin ? ' Admin' : ''}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Platform</label>
              <Select value={selectedPlatform} onValueChange={handlePlatformChange}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Web">
                    <div className="flex items-center gap-2">
                      <Monitor className="h-4 w-4" />
                      Web Application
                    </div>
                  </SelectItem>
                  <SelectItem value="Mobile">
                    <div className="flex items-center gap-2">
                      <Smartphone className="h-4 w-4" />
                      Mobile Application
                    </div>
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Action Buttons */}
          {selectedRoleId && (
            <div className="flex items-center justify-between mt-4 pt-4 border-t">
              <div className="flex gap-2">
                <Button 
                  variant="outline" 
                  size="sm"
                  onClick={() => setIsBulkEditorOpen(true)}
                >
                  Bulk Edit
                </Button>
              </div>
              <div className="flex gap-2">
                {hasUnsavedChanges && (
                  <>
                    <Button 
                      variant="outline" 
                      size="sm"
                      onClick={handleDiscardChanges}
                    >
                      <Undo className="mr-2 h-4 w-4" />
                      Discard Changes
                    </Button>
                    <Button 
                      size="sm"
                      onClick={handleSavePermissions}
                      disabled={loading}
                    >
                      <Save className="mr-2 h-4 w-4" />
                      Save Changes
                    </Button>
                  </>
                )}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Statistics */}
      {selectedRoleId && (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-6">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Pages</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stats.totalPermissions}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Granted</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-green-600">{stats.grantedPermissions}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Full Access</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-blue-600">{stats.fullAccessPages}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">View Only</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-yellow-600">{stats.viewOnlyPages}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Web Perms</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-indigo-600">{stats.webPermissions}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Mobile Perms</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-emerald-600">{stats.mobilePermissions}</div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Permission Matrix */}
      {selectedRoleId && permissionMatrix && (
        <Card>
          <CardHeader>
            <CardTitle>Permission Matrix</CardTitle>
            <CardDescription>
              Configure specific permissions for each page and feature
            </CardDescription>
          </CardHeader>
          <CardContent>
            <PermissionMatrix
              matrix={permissionMatrix}
              loading={loading}
              onChange={handlePermissionChange}
            />
          </CardContent>
        </Card>
      )}

      {/* No Selection State */}
      {!selectedRoleId && (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Key className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">Select a Role</h3>
            <p className="text-muted-foreground text-center max-w-sm">
              Choose a role from the dropdown above to view and configure its permissions.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Copy Permissions Dialog */}
      <Dialog open={isCopyDialogOpen} onOpenChange={setIsCopyDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Copy Permissions</DialogTitle>
          </DialogHeader>
          <PermissionCopyDialog
            roles={roles}
            sourceRoleId={selectedRoleId}
            platform={selectedPlatform}
            onCopy={handleCopyPermissions}
            onCancel={() => setIsCopyDialogOpen(false)}
          />
        </DialogContent>
      </Dialog>

      {/* Bulk Permission Editor Dialog */}
      <Dialog open={isBulkEditorOpen} onOpenChange={setIsBulkEditorOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Bulk Permission Editor</DialogTitle>
          </DialogHeader>
          <BulkPermissionEditor
            matrix={permissionMatrix}
            onUpdate={handleBulkPermissionUpdate}
            onCancel={() => setIsBulkEditorOpen(false)}
          />
        </DialogContent>
      </Dialog>

      {/* Permission Templates Dialog */}
      <Dialog open={isTemplatesOpen} onOpenChange={setIsTemplatesOpen}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Permission Templates</DialogTitle>
          </DialogHeader>
          <PermissionTemplates
            selectedRoleId={selectedRoleId}
            platform={selectedPlatform}
            onApplyTemplate={() => {
              triggerRefresh()
              setIsTemplatesOpen(false)
            }}
            onClose={() => setIsTemplatesOpen(false)}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}