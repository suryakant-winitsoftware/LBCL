"use client"

import { useState, useCallback, useEffect, useMemo } from "react"
import { useSearchParams } from "next/navigation"
import { Key, Monitor, Smartphone, Copy, Download, RefreshCw, Save, Undo, Search, ChevronDown, Check } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { useToast } from "@/components/ui/use-toast"
import { PermissionMatrix } from "./permission-matrix"
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
  const [rolePopoverOpen, setRolePopoverOpen] = useState(false)
  const [permissionMatrix, setPermissionMatrix] = useState<PermissionMatrixType | null>(null)
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false)
  const [refreshTrigger, setRefreshTrigger] = useState(0)

  // Dialog states
  const [isCopyDialogOpen, setIsCopyDialogOpen] = useState(false)
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
              <Popover open={rolePopoverOpen} onOpenChange={setRolePopoverOpen}>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={rolePopoverOpen}
                    className="w-full justify-between"
                  >
                    {selectedRoleId
                      ? roles.find((role) => role.UID === selectedRoleId)?.RoleNameEn || "Select a role"
                      : "Select a role"}
                    <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
                  <Command>
                    <div className="flex items-center border-b px-4 py-2">
                      <Search className="mr-3 h-4 w-4 shrink-0 opacity-50" />
                      <CommandInput 
                        placeholder="Search roles..." 
                        className="flex h-9 w-full rounded-md bg-transparent py-2 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50 border-0 focus:ring-0"
                      />
                    </div>
                    <CommandEmpty className="py-6 text-center text-sm">
                      <div className="flex flex-col items-center gap-2">
                        <Search className="h-8 w-8 text-muted-foreground/50" />
                        <p>No roles found</p>
                        <p className="text-xs text-muted-foreground">Try a different search term</p>
                      </div>
                    </CommandEmpty>
                    <CommandGroup>
                      <CommandList className="max-h-[280px] overflow-y-auto">
                        {roles.map((role) => (
                          <CommandItem
                            key={role.UID}
                            value={`${role.RoleNameEn} ${role.Code}`}
                            onSelect={() => {
                              handleRoleChange(role.UID)
                              setRolePopoverOpen(false)
                            }}
                            className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-accent rounded-none"
                          >
                            <div className="flex items-center gap-4 flex-1">
                              <Check
                                className={`h-4 w-4 shrink-0 ${
                                  selectedRoleId === role.UID ? "opacity-100 text-primary" : "opacity-0"
                                }`}
                              />
                              <div className="flex flex-col gap-1.5 flex-1 min-w-0">
                                <div className="font-medium text-sm truncate">{role.RoleNameEn}</div>
                                <div className="text-xs text-muted-foreground">
                                  Code: {role.Code}
                                </div>
                              </div>
                            </div>
                            <div className="flex gap-2 ml-4 shrink-0">
                              {role.IsPrincipalRole && (
                                <Badge variant="outline" className="text-[10px] px-2 py-1 h-auto">
                                  Principal
                                </Badge>
                              )}
                              {role.IsDistributorRole && (
                                <Badge variant="secondary" className="text-[10px] px-2 py-1 h-auto">
                                  Distributor
                                </Badge>
                              )}
                              {role.IsAdmin && (
                                <Badge variant="destructive" className="text-[10px] px-2 py-1 h-auto">
                                  Admin
                                </Badge>
                              )}
                            </div>
                          </CommandItem>
                        ))}
                      </CommandList>
                    </CommandGroup>
                  </Command>
                </PopoverContent>
              </Popover>
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
              onSave={handleSavePermissions}
              onDiscard={handleDiscardChanges}
              hasUnsavedChanges={hasUnsavedChanges}
            />
          </CardContent>
        </Card>
      )}

      {/* No Selection State */}
      {!selectedRoleId && (
        <Card className="border-2 border-dashed border-gray-300 bg-gray-50/50">
          <CardContent className="flex flex-col items-center justify-center py-16 px-8">
            <div className="relative mb-6">
              <div className="absolute -inset-2 bg-blue-100 rounded-full opacity-20"></div>
              <Key className="h-16 w-16 text-blue-600 relative z-10" />
            </div>
            
            <h3 className="text-2xl font-bold text-gray-900 mb-3">Get Started with Role Permissions</h3>
            
            <div className="text-center max-w-md mb-6">
              <p className="text-gray-600 text-lg mb-2">
                Select a role to view and manage its permissions across Web and Mobile platforms.
              </p>
              <p className="text-sm text-gray-500">
                You can configure access levels for different pages and features based on the selected role.
              </p>
            </div>

            <div className="flex flex-col items-center gap-4">
              <div className="flex items-center gap-2 text-sm text-blue-600 bg-blue-50 px-4 py-2 rounded-lg border border-blue-200">
                <span className="w-2 h-2 bg-blue-600 rounded-full animate-pulse"></span>
                <span className="font-medium">Choose a role from the dropdown above</span>
              </div>
              
              <div className="flex items-center gap-6 text-xs text-gray-500">
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center">
                    <span className="text-green-600 font-bold">1</span>
                  </div>
                  <span>Select Role</span>
                </div>
                <div className="w-8 h-px bg-gray-300"></div>
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                    <span className="text-blue-600 font-bold">2</span>
                  </div>
                  <span>Choose Platform</span>
                </div>
                <div className="w-8 h-px bg-gray-300"></div>
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 bg-purple-100 rounded-full flex items-center justify-center">
                    <span className="text-purple-600 font-bold">3</span>
                  </div>
                  <span>Configure Permissions</span>
                </div>
              </div>
            </div>
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