"use client"

import { useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Plus, Download, Copy, Settings, RefreshCw, Filter, Shield } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import { RoleGrid } from "./role-grid"
import { RoleForm } from "./role-form"
import { RoleCloneDialog } from "./role-clone-dialog"
import { RoleFilters } from "./role-filters"
import { Role } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"

export function RoleManagement() {
  const { toast } = useToast()
  const router = useRouter()
  const [activeTab, setActiveTab] = useState("list")
  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null)
  const [isFiltersOpen, setIsFiltersOpen] = useState(false)
  const [refreshTrigger, setRefreshTrigger] = useState(0)

  // Search and filter state
  const [searchQuery, setSearchQuery] = useState("")
  const [filters, setFilters] = useState({
    isPrincipalRole: [] as boolean[],
    isDistributorRole: [] as boolean[],
    isAdmin: [] as boolean[],
    isWebUser: [] as boolean[],
    isAppUser: [] as boolean[],
    isActive: [] as boolean[]
  })

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
    router.push("/updatedfeatures/role-management/roles/create")
  }

  const handleEditRole = useCallback((roleId: string) => {
    router.push(`/updatedfeatures/role-management/roles/edit/${roleId}`)
  }, [router])

  const handleViewRole = useCallback((roleId: string) => {
    router.push(`/updatedfeatures/role-management/roles/view/${roleId}`)
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
          const blob = await roleService.exportRoles("csv", filters)
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
  }, [filters, toast])


  const triggerRefresh = useCallback(() => {
    setRefreshTrigger(prev => prev + 1)
  }, [])

  const handleStatsUpdate = useCallback((newStats: typeof stats) => {
    setStats(newStats)
  }, [])

  const handleFiltersChange = useCallback((newFilters: typeof filters) => {
    setFilters(newFilters)
    triggerRefresh()
  }, [])

  const handleExportAll = useCallback(async () => {
    try {
      const blob = await roleService.exportRoles("csv", filters)
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
  }, [filters, toast])

  const handleManagePermissions = useCallback((roleId: string) => {
    // Navigate to permission management with this role selected
    window.location.href = `/updatedfeatures/role-management/roles/permissions?role=${roleId}`
  }, [])

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
          <Button variant="outline" size="sm" onClick={() => setIsFiltersOpen(true)}>
            <Filter className="mr-2 h-4 w-4" />
            Filters
          </Button>
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
            <div className="flex gap-2">
              {filters.isPrincipalRole.length > 0 && (
                <Badge variant="secondary">Principal Filter</Badge>
              )}
              {filters.isDistributorRole.length > 0 && (
                <Badge variant="secondary">Distributor Filter</Badge>
              )}
              {filters.isAdmin.length > 0 && (
                <Badge variant="secondary">Admin Filter</Badge>
              )}
              {(filters.isWebUser.length > 0 || filters.isAppUser.length > 0) && (
                <Badge variant="secondary">Platform Filter</Badge>
              )}
            </div>
          </div>
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



      {/* Filters Dialog */}
      <Dialog open={isFiltersOpen} onOpenChange={setIsFiltersOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Filter Roles</DialogTitle>
          </DialogHeader>
          <RoleFilters
            filters={filters}
            onFiltersChange={handleFiltersChange}
            onClose={() => setIsFiltersOpen(false)}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}