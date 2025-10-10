"use client"

import { useState, useCallback, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Plus, Download, Upload, Filter, RefreshCw } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import { EmployeeGrid } from "./employee-grid"
import { EmployeePermissionViewer } from "./employee-permission-viewer"
import { EmployeeFilters } from "./employee-filters"
import { Employee, PagingRequest } from "@/types/admin.types"
import { employeeService } from "@/services/admin/employee.service"

export function EmployeeManagement() {
  const router = useRouter()
  const { toast } = useToast()
  const [activeTab, setActiveTab] = useState("list")
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<string | null>(null)
  const [isFiltersOpen, setIsFiltersOpen] = useState(false)
  const [refreshTrigger, setRefreshTrigger] = useState(0)
  const [loadingStats, setLoadingStats] = useState(true)

  // Search and filter state
  const [searchQuery, setSearchQuery] = useState("")
  const [filters, setFilters] = useState({
    status: [] as string[],
    roles: [] as string[],
    organizations: [] as string[]
  })

  // Statistics state
  const [stats, setStats] = useState({
    total: 0,
    active: 0,
    inactive: 0,
    pending: 0
  })

  // Load statistics separately
  useEffect(() => {
    const loadStats = async () => {
      setLoadingStats(true)
      try {
        const statsData = await employeeService.getEmployeeStats()
        setStats(statsData)
      } catch (error) {
        console.error("Failed to load stats:", error)
      } finally {
        setLoadingStats(false)
      }
    }
    loadStats()
  }, [refreshTrigger])

  const handleCreateEmployee = () => {
    router.push('/updatedfeatures/employee-management/employees/create')
  }

  const handleEditEmployee = useCallback((employeeId: string) => {
    router.push(`/updatedfeatures/employee-management/employees/${employeeId}/edit`)
  }, [router])

  const handleViewPermissions = useCallback((employeeId: string) => {
    setSelectedEmployeeId(employeeId)
    setActiveTab("permissions")
  }, [])

  const handleViewDetails = useCallback((employeeId: string) => {
    router.push(`/updatedfeatures/employee-management/employees/${employeeId}`)
  }, [router])

  const handleDeactivateEmployee = useCallback(async (employeeId: string) => {
    try {
      await employeeService.deactivateEmployee(employeeId)
      toast({
        title: "Success",
        description: "Employee has been deactivated successfully.",
      })
      triggerRefresh()
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to deactivate employee. Please try again.",
        variant: "destructive",
      })
    }
  }, [toast])

  const handleActivateEmployee = useCallback(async (employeeId: string) => {
    try {
      await employeeService.activateEmployee(employeeId)
      toast({
        title: "Success",
        description: "Employee has been activated successfully.",
      })
      triggerRefresh()
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to activate employee. Please try again.",
        variant: "destructive",
      })
    }
  }, [toast])

  const handleDeleteEmployee = useCallback(async (employeeId: string) => {
    if (window.confirm("Are you sure you want to permanently delete this employee? \n\nNote: If this employee has existing records (orders, journey plans, etc.), the employee will be marked as deleted instead of being permanently removed.")) {
      try {
        await employeeService.deleteEmployee(employeeId)
        toast({
          title: "Success",
          description: "Employee has been deleted/deactivated successfully.",
        })
        triggerRefresh()
      } catch (error: any) {
        toast({
          title: "Warning",
          description: error.message || "Employee has been deactivated instead of deleted due to existing references.",
          variant: "destructive",
        })
        triggerRefresh()
      }
    }
  }, [toast])

  const handleBulkAction = useCallback(async (action: string, selectedIds: string[]) => {
    try {
      switch (action) {
        case "activate":
          await employeeService.bulkUpdateStatus(selectedIds, "Active")
          toast({
            title: "Success",
            description: `${selectedIds.length} employees activated successfully.`,
          })
          break
        case "deactivate":
          await employeeService.bulkUpdateStatus(selectedIds, "Inactive")
          toast({
            title: "Success",
            description: `${selectedIds.length} employees deactivated successfully.`,
          })
          break
        case "export":
          const blob = await employeeService.exportEmployees("csv", filters)
          const url = URL.createObjectURL(blob)
          const a = document.createElement("a")
          a.href = url
          a.download = `employees-${new Date().toISOString().split("T")[0]}.csv`
          document.body.appendChild(a)
          a.click()
          document.body.removeChild(a)
          URL.revokeObjectURL(url)
          toast({
            title: "Success",
            description: "Employee data exported successfully.",
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

  const handleFiltersChange = useCallback((newFilters: typeof filters) => {
    setFilters(newFilters)
    triggerRefresh()
  }, [])

  const handleExportAll = useCallback(async () => {
    try {
      const blob = await employeeService.exportEmployees("csv", filters)
      const url = URL.createObjectURL(blob)
      const a = document.createElement("a")
      a.href = url
      a.download = `employees-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
      toast({
        title: "Success",
        description: "Employee data exported successfully.",
      })
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to export employee data. Please try again.",
        variant: "destructive",
      })
    }
  }, [filters, toast])

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Employee Management</h1>
          <p className="text-muted-foreground">
            Manage employees, assign roles, and control access permissions
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
          <Button variant="outline" size="sm">
            <Upload className="mr-2 h-4 w-4" />
            Import
          </Button>
          <Button variant="outline" size="sm" onClick={triggerRefresh}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          <Button onClick={handleCreateEmployee}>
            <Plus className="mr-2 h-4 w-4" />
            Add Employee
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Employees</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              All registered employees
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{stats.active.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Currently active accounts
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Inactive</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">{stats.inactive.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Deactivated accounts
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-yellow-600">{stats.pending.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Awaiting activation
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Search Bar */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 md:flex-row md:items-center">
            <div className="flex-1">
              <Input
                placeholder="Search employees by name, code, or login ID..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full"
              />
            </div>
            <div className="flex gap-2">
              {filters.status.length > 0 && (
                <Badge variant="secondary">
                  Status: {filters.status.join(", ")}
                </Badge>
              )}
              {filters.roles.length > 0 && (
                <Badge variant="secondary">
                  Roles: {filters.roles.length}
                </Badge>
              )}
              {filters.organizations.length > 0 && (
                <Badge variant="secondary">
                  Orgs: {filters.organizations.length}
                </Badge>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Main Content */}
      <Card>
        <CardHeader>
          <CardTitle>Employee List</CardTitle>
          <CardDescription>
            Manage employee accounts, roles, and permissions
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList>
              <TabsTrigger value="list">Employee List</TabsTrigger>
              <TabsTrigger value="permissions" disabled={!selectedEmployeeId}>
                View Permissions
              </TabsTrigger>
            </TabsList>
            
            <TabsContent value="list" className="mt-6">
              <EmployeeGrid
                searchQuery={searchQuery}
                filters={filters}
                refreshTrigger={refreshTrigger}
                onEdit={handleEditEmployee}
                onDelete={handleDeleteEmployee}
                onDeactivate={handleDeactivateEmployee}
                onActivate={handleActivateEmployee}
                onViewPermissions={handleViewPermissions}
                onViewDetails={handleViewDetails}
                onBulkAction={handleBulkAction}
                onStatsUpdate={() => {}}
              />
            </TabsContent>
            
            <TabsContent value="permissions" className="mt-6">
              {selectedEmployeeId && (
                <EmployeePermissionViewer 
                  employeeId={selectedEmployeeId}
                  onBack={() => setActiveTab("list")}
                />
              )}
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      {/* Filters Dialog */}
      <Dialog open={isFiltersOpen} onOpenChange={setIsFiltersOpen}>
        <DialogContent className="max-w-2xl z-[9999]" style={{ zIndex: 9999 }}>
          <DialogHeader>
            <DialogTitle>Filter Employees</DialogTitle>
          </DialogHeader>
          <EmployeeFilters
            filters={filters}
            onFiltersChange={handleFiltersChange}
            onClose={() => setIsFiltersOpen(false)}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}