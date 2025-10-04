"use client"

import { useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Plus, Download, Copy, Settings, RefreshCw, Shield, Filter } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Badge } from "@/components/ui/badge"
import { Checkbox } from "@/components/ui/checkbox"
import { Label } from "@/components/ui/label"
import { useToast } from "@/components/ui/use-toast"
import { RoleGrid } from "./role-grid"
import { RoleForm } from "./role-form"
import { RoleCloneDialog } from "./role-clone-dialog"
import { Role } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"

// Default filters
const DEFAULT_FILTERS = {
  isPrincipalRole: [] as boolean[],
  isDistributorRole: [] as boolean[],
  isAdmin: [] as boolean[],
  isWebUser: [] as boolean[],
  isAppUser: [] as boolean[],
  isActive: [] as boolean[]
}

export function RoleManagement() {
  const { toast } = useToast()
  const router = useRouter()
  const [refreshTrigger, setRefreshTrigger] = useState(0)

  // Search state
  const [searchQuery, setSearchQuery] = useState("")
  const [showFilters, setShowFilters] = useState(false)
  const [filters, setFilters] = useState(DEFAULT_FILTERS)

  // Statistics state
  const [stats, setStats] = useState({
    total: 0,
    active: 0,
    principal: 0,
    distributor: 0,
    admin: 0,
    web: 0,
    mobile: 0
  })

  const handleCreateRole = () => {
    router.push("/administration/access-control/roles/create")
  }

  const handleEditRole = useCallback((roleId: string) => {
    router.push(`/administration/access-control/roles/edit/${roleId}`)
  }, [router])

  const handleViewRole = useCallback((roleId: string) => {
    router.push(`/administration/access-control/roles/view/${roleId}`)
  }, [router])


  const handleDeleteRole = useCallback(async (roleId: string) => {
    try {
      await roleService.deleteRole(roleId)
      toast({
        title: "Success",
        description: "Role has been deactivated successfully.",
      })
      triggerRefresh()
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to deactivate role. Please try again.",
        variant: "destructive",
      })
    }
  }, [toast])

  const handleBulkAction = useCallback(async (action: string, selectedIds: string[]) => {
    try {
      switch (action) {
        case "activate":
          await roleService.bulkUpdateRoleStatus(selectedIds, true)
          toast({
            title: "Success",
            description: `${selectedIds.length} roles activated successfully.`,
          })
          break
        case "deactivate":
          await roleService.bulkUpdateRoleStatus(selectedIds, false)
          toast({
            title: "Success",
            description: `${selectedIds.length} roles deactivated successfully.`,
          })
          break
        case "enable-web":
          await roleService.bulkUpdatePlatformAccess(selectedIds, true, undefined)
          toast({
            title: "Success",
            description: `Web access enabled for ${selectedIds.length} roles.`,
          })
          break
        case "enable-mobile":
          await roleService.bulkUpdatePlatformAccess(selectedIds, undefined, true)
          toast({
            title: "Success",
            description: `Mobile access enabled for ${selectedIds.length} roles.`,
          })
          break
        case "export":
          const blob = await roleService.exportRoles("csv", {})
          const url = URL.createObjectURL(blob)
          const a = document.createElement("a")
          a.href = url
          a.download = `roles-${new Date().toISOString().split("T")[0]}.csv`
          document.body.appendChild(a)
          a.click()
          document.body.removeChild(a)
          URL.revokeObjectURL(url)
          toast({
            title: "Success",
            description: "Role data exported successfully.",
          })
          break
        default:
          throw new Error(`Unknown action: ${action}`)
      }
      triggerRefresh()
    } catch (error) {
      toast({
        title: "Error",
        description: `Failed to perform bulk action: ${error instanceof Error ? error.message : "Unknown error"}`,
        variant: "destructive",
      })
    }
  }, [toast])


  const triggerRefresh = useCallback(() => {
    setRefreshTrigger(prev => prev + 1)
  }, [])

  const handleStatsUpdate = useCallback((newStats: typeof stats) => {
    setStats(newStats)
  }, [])

  const handleExportAll = useCallback(async () => {
    try {
      const blob = await roleService.exportRoles("csv", {})
      const url = URL.createObjectURL(blob)
      const a = document.createElement("a")
      a.href = url
      a.download = `roles-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
      toast({
        title: "Success",
        description: "Role data exported successfully.",
      })
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to export role data. Please try again.",
        variant: "destructive",
      })
    }
  }, [toast])

  const handleManagePermissions = useCallback((roleId: string) => {
    // Navigate to permission management with this role selected
    window.location.href = `/administration/access-control/permission-matrix?role=${roleId}`
  }, [])

  const handleFilterChange = useCallback((filterKey: keyof typeof DEFAULT_FILTERS, value: boolean, checked: boolean) => {
    setFilters(prev => {
      const currentValues = prev[filterKey]
      if (checked) {
        return {
          ...prev,
          [filterKey]: currentValues.includes(value) ? currentValues : [...currentValues, value]
        }
      } else {
        return {
          ...prev,
          [filterKey]: currentValues.filter(v => v !== value)
        }
      }
    })
  }, [])

  const clearFilters = useCallback(() => {
    setFilters(DEFAULT_FILTERS)
  }, [])

  const hasActiveFilters = Object.values(filters).some(filterArray => filterArray.length > 0)

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Role Management</h1>
          <p className="text-muted-foreground">
            Create and manage user roles with platform-specific permissions
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="outline" size="sm" onClick={handleExportAll}>
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button variant="outline" size="sm" onClick={triggerRefresh}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          <Button onClick={handleCreateRole}>
            <Plus className="mr-2 h-4 w-4" />
            Create Role
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid gap-4 md:grid-cols-4 lg:grid-cols-7">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Roles</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
            <p className="text-xs text-muted-foreground">All roles</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{stats.active}</div>
            <p className="text-xs text-muted-foreground">Currently active</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Principal</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-600">{stats.principal}</div>
            <p className="text-xs text-muted-foreground">Principal roles</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Distributor</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-purple-600">{stats.distributor}</div>
            <p className="text-xs text-muted-foreground">Distributor roles</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Admin</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">{stats.admin}</div>
            <p className="text-xs text-muted-foreground">Admin privileges</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Web</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-indigo-600">{stats.web}</div>
            <p className="text-xs text-muted-foreground">Web access</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Mobile</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-emerald-600">{stats.mobile}</div>
            <p className="text-xs text-muted-foreground">Mobile access</p>
          </CardContent>
        </Card>
      </div>

      {/* Search Bar */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 md:flex-row md:items-center">
            <div className="flex-1">
              <Input
                placeholder="Search roles by name or code..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full"
              />
            </div>
            <Button
              variant="outline"
              size="default"
              onClick={() => setShowFilters(!showFilters)}
              className="flex items-center gap-2"
            >
              <Filter className="h-4 w-4" />
              Filter
              {hasActiveFilters && (
                <Badge variant="secondary" className="ml-1 px-1.5 py-0.5 text-xs">
                  {Object.values(filters).reduce((sum, arr) => sum + arr.length, 0)}
                </Badge>
              )}
            </Button>
          </div>
          
          {/* Filter Panel */}
          {showFilters && (
            <div className="mt-4 p-4 bg-gray-50 rounded-lg border">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-medium text-gray-900">Filter Options</h3>
                {hasActiveFilters && (
                  <Button variant="ghost" size="sm" onClick={clearFilters}>
                    Clear All
                  </Button>
                )}
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-6">
                {/* Role Type Filters */}
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-gray-700">Role Type</Label>
                  <div className="space-y-2">
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="principal-true"
                        checked={filters.isPrincipalRole.includes(true)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isPrincipalRole', true, !!checked)
                        }
                      />
                      <Label htmlFor="principal-true" className="text-sm">Principal</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="distributor-true"
                        checked={filters.isDistributorRole.includes(true)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isDistributorRole', true, !!checked)
                        }
                      />
                      <Label htmlFor="distributor-true" className="text-sm">Distributor</Label>
                    </div>
                  </div>
                </div>

                {/* Admin Status */}
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-gray-700">Admin Status</Label>
                  <div className="space-y-2">
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="admin-true"
                        checked={filters.isAdmin.includes(true)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isAdmin', true, !!checked)
                        }
                      />
                      <Label htmlFor="admin-true" className="text-sm">Admin</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="admin-false"
                        checked={filters.isAdmin.includes(false)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isAdmin', false, !!checked)
                        }
                      />
                      <Label htmlFor="admin-false" className="text-sm">Non-Admin</Label>
                    </div>
                  </div>
                </div>

                {/* Web Access */}
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-gray-700">Web Access</Label>
                  <div className="space-y-2">
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="web-true"
                        checked={filters.isWebUser.includes(true)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isWebUser', true, !!checked)
                        }
                      />
                      <Label htmlFor="web-true" className="text-sm">Enabled</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="web-false"
                        checked={filters.isWebUser.includes(false)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isWebUser', false, !!checked)
                        }
                      />
                      <Label htmlFor="web-false" className="text-sm">Disabled</Label>
                    </div>
                  </div>
                </div>

                {/* Mobile Access */}
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-gray-700">Mobile Access</Label>
                  <div className="space-y-2">
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="mobile-true"
                        checked={filters.isAppUser.includes(true)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isAppUser', true, !!checked)
                        }
                      />
                      <Label htmlFor="mobile-true" className="text-sm">Enabled</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="mobile-false"
                        checked={filters.isAppUser.includes(false)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isAppUser', false, !!checked)
                        }
                      />
                      <Label htmlFor="mobile-false" className="text-sm">Disabled</Label>
                    </div>
                  </div>
                </div>

                {/* Active Status */}
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-gray-700">Status</Label>
                  <div className="space-y-2">
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="active-true"
                        checked={filters.isActive.includes(true)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isActive', true, !!checked)
                        }
                      />
                      <Label htmlFor="active-true" className="text-sm">Active</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="active-false"
                        checked={filters.isActive.includes(false)}
                        onCheckedChange={(checked) => 
                          handleFilterChange('isActive', false, !!checked)
                        }
                      />
                      <Label htmlFor="active-false" className="text-sm">Inactive</Label>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Main Content */}
      <Card>
        <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-xl font-semibold flex items-center gap-2">
                <Shield className="h-5 w-5 text-gray-700" />
                Role Management
              </CardTitle>
              <CardDescription className="mt-1">
                Configure and manage organizational roles with platform-specific permissions
              </CardDescription>
            </div>
            <div className="text-sm text-gray-600">
              <span className="font-medium">{stats.total}</span> total roles • 
              <span className="font-medium text-green-600"> {stats.active}</span> active
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <RoleGrid
            searchQuery={searchQuery}
            filters={filters}
            refreshTrigger={refreshTrigger}
            onView={handleViewRole}
            onEdit={handleEditRole}
            onDelete={handleDeleteRole}
            onManagePermissions={handleManagePermissions}
            onBulkAction={handleBulkAction}
            onStatsUpdate={handleStatsUpdate}
            onRefresh={triggerRefresh}
          />
        </CardContent>
      </Card>
    </div>
  )
}