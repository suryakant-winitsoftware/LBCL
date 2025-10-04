"use client"

import { useState, useCallback, useEffect, useMemo, useRef } from "react"
import { useRouter } from "next/navigation"
import { Plus, Download, Upload, RefreshCw, Search, Loader2, Lock, Eye, EyeOff, Filter, X, ChevronDown } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from "@/components/ui/dropdown-menu"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { EmployeeGrid } from "./employee-grid"
import { Employee, PagingRequest } from "@/types/admin.types"
import { employeeService } from "@/services/admin/employee.service"

export function EmployeeManagement() {
  const router = useRouter()
  const { toast } = useToast()
  const [refreshTrigger, setRefreshTrigger] = useState(0)
  const [loadingStats, setLoadingStats] = useState(true)

  // Change password dialog state
  const [changePasswordVisible, setChangePasswordVisible] = useState(false)
  const [selectedEmployee, setSelectedEmployee] = useState<{ id: string; name: string } | null>(null)
  const [newPassword, setNewPassword] = useState("")
  const [confirmPassword, setConfirmPassword] = useState("")
  const [showPassword, setShowPassword] = useState(false)
  const [passwordLoading, setPasswordLoading] = useState(false)

  // Search and filter state
  const [searchQuery, setSearchQuery] = useState("")
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState("")
  const [isSearching, setIsSearching] = useState(false)
  const searchInputRef = useRef<HTMLInputElement>(null)
  const [filters, setFilters] = useState({
    status: [] as string[],
    roles: [] as string[],
    organizations: [] as string[]
  })
  
  // Available roles for filtering
  const [availableRoles, setAvailableRoles] = useState<Array<{uid: string, name: string}>>([])
  const [loadingRoles, setLoadingRoles] = useState(true)
  const [isExporting, setIsExporting] = useState(false)

  // Statistics state
  const [stats, setStats] = useState({
    total: 0,
    active: 0,
    inactive: 0,
    pending: 0
  })

  // Debounce search query
  useEffect(() => {
    if (searchQuery !== debouncedSearchQuery) {
      setIsSearching(true)
    }
    
    const timer = setTimeout(() => {
      setDebouncedSearchQuery(searchQuery)
      setIsSearching(false)
    }, 300) // 300ms debounce

    return () => clearTimeout(timer)
  }, [searchQuery, debouncedSearchQuery])

  // Add keyboard shortcut for Ctrl+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault()
        searchInputRef.current?.focus()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])

  // Handle roles update from employee grid
  const handleRolesUpdate = useCallback((roles: Array<{uid: string, name: string}>) => {
    setAvailableRoles(roles)
    setLoadingRoles(false)
  }, [])

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
    router.push('/administration/team-management/employees/create')
  }

  const handleEditEmployee = useCallback((employeeId: string) => {
    router.push(`/administration/team-management/employees/${employeeId}/edit`)
  }, [router])

  const handleViewPermissions = useCallback((employeeId: string) => {
    // This would open a permissions view - placeholder for now
    router.push(`/administration/team-management/employees/${employeeId}/permissions`)
  }, [router])

  const handleViewDetails = useCallback((employeeId: string) => {
    router.push(`/administration/team-management/employees/${employeeId}`)
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

  const handleChangePassword = useCallback(async (employeeId: string, employeeName: string) => {
    setSelectedEmployee({ id: employeeId, name: employeeName })
    setChangePasswordVisible(true)
    setNewPassword("")
    setConfirmPassword("")
    setShowPassword(false)
  }, [])

  const handlePasswordSubmit = useCallback(async () => {
    if (!selectedEmployee) return

    if (!newPassword || newPassword !== confirmPassword) {
      toast({
        title: "Validation Error",
        description: "Please ensure passwords match and are at least 6 characters long.",
        variant: "destructive",
      })
      return
    }

    if (newPassword.length < 6) {
      toast({
        title: "Validation Error",
        description: "Password must be at least 6 characters long.",
        variant: "destructive",
      })
      return
    }

    setPasswordLoading(true)
    try {
      const success = await employeeService.resetPassword(selectedEmployee.id, newPassword)
      
      if (success) {
        toast({
          title: "Success",
          description: `Password changed successfully for ${selectedEmployee.name}!`,
        })
        setChangePasswordVisible(false)
        setNewPassword("")
        setConfirmPassword("")
        setSelectedEmployee(null)
      } else {
        throw new Error("Password change failed")
      }
    } catch (error) {
      console.error("Password change error:", error)
      toast({
        title: "Error",
        description: "Failed to change password. Please try again.",
        variant: "destructive",
      })
    } finally {
      setPasswordLoading(false)
    }
  }, [selectedEmployee, newPassword, confirmPassword, toast])

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
      setIsExporting(true)
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
    } finally {
      setIsExporting(false)
    }
  }, [filters, toast])

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 -mx-6 -mt-6 px-6 py-8 mb-2 border-b border-blue-100 dark:border-blue-900/30">
        <div className="flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
          <div className="space-y-2">
            <div className="flex items-center gap-3">
              <div className="h-12 w-12 rounded-xl bg-blue-500 dark:bg-blue-600 flex items-center justify-center shadow-lg">
                <svg className="h-6 w-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
              </div>
              <div>
                <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-gray-100">Employee Management</h1>
                <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                  Manage employees, assign roles, and control access permissions
                </p>
              </div>
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" onClick={handleExportAll} disabled={isExporting} className="bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800 border-gray-200 dark:border-gray-700">
              {isExporting ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Download className="mr-2 h-4 w-4" />
              )}
              {isExporting ? 'Exporting...' : 'Export'}
            </Button>
            <Button variant="outline" className="bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800 border-gray-200 dark:border-gray-700">
              <Upload className="mr-2 h-4 w-4" />
              Import
            </Button>
            <Button onClick={handleCreateEmployee} className="bg-blue-600 hover:bg-blue-700 text-white shadow-md hover:shadow-lg transition-shadow">
              <Plus className="mr-2 h-4 w-4" />
              Add Employee
            </Button>
          </div>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card className="shadow-none">
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
        <Card className="shadow-none">
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
        <Card className="shadow-none">
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
        <Card className="shadow-none">
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

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <div className="p-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search employees by name, code, or login ID... (Ctrl+F)"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
              {isSearching && (
                <Loader2 className="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400 animate-spin" />
              )}
            </div>
            
            {/* Status Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {filters.status.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filters.status.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={filters.status.includes("Active")}
                  onCheckedChange={(checked) => {
                    setFilters(prev => ({
                      ...prev,
                      status: checked 
                        ? [...prev.status, "Active"]
                        : prev.status.filter(s => s !== "Active")
                    }))
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-green-500 rounded-full" />
                    Active
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={filters.status.includes("Inactive")}
                  onCheckedChange={(checked) => {
                    setFilters(prev => ({
                      ...prev,
                      status: checked 
                        ? [...prev.status, "Inactive"]
                        : prev.status.filter(s => s !== "Inactive")
                    }))
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-red-500 rounded-full" />
                    Inactive
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={filters.status.includes("Pending")}
                  onCheckedChange={(checked) => {
                    setFilters(prev => ({
                      ...prev,
                      status: checked 
                        ? [...prev.status, "Pending"]
                        : prev.status.filter(s => s !== "Pending")
                    }))
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-yellow-500 rounded-full" />
                    Pending
                  </div>
                </DropdownMenuCheckboxItem>
                {filters.status.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => {
                        setFilters(prev => ({
                          ...prev,
                          status: []
                        }))
                      }}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>

            {/* Role Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Role
                  {filters.roles.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filters.roles.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Role</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {loadingRoles ? (
                  <DropdownMenuItem disabled>Loading roles...</DropdownMenuItem>
                ) : availableRoles.length > 0 ? (
                  availableRoles.map((role) => (
                    <DropdownMenuCheckboxItem
                      key={role.uid}
                      checked={filters.roles.includes(role.name)}
                      onCheckedChange={(checked) => {
                        setFilters(prev => ({
                          ...prev,
                          roles: checked 
                            ? [...prev.roles, role.name]
                            : prev.roles.filter(r => r !== role.name)
                        }))
                      }}
                    >
                      {role.name}
                    </DropdownMenuCheckboxItem>
                  ))
                ) : (
                  <DropdownMenuItem disabled>No roles available</DropdownMenuItem>
                )}
                {filters.roles.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => {
                        setFilters(prev => ({
                          ...prev,
                          roles: []
                        }))
                      }}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
            
            <Button
              variant="outline"
              size="default"
              onClick={triggerRefresh}
            >
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </Card>

          {/* Main Content */}
          <Card className="shadow-none">
            <CardContent className="p-0">
              <EmployeeGrid
                searchQuery={debouncedSearchQuery}
                filters={filters}
                refreshTrigger={refreshTrigger}
                onEdit={handleEditEmployee}
                onDelete={handleDeleteEmployee}
                onDeactivate={handleDeactivateEmployee}
                onActivate={handleActivateEmployee}
                onViewPermissions={handleViewPermissions}
                onViewDetails={handleViewDetails}
                onChangePassword={handleChangePassword}
                onBulkAction={handleBulkAction}
                onStatsUpdate={() => {}}
                onRolesUpdate={handleRolesUpdate}
              />
            </CardContent>
          </Card>

      {/* Change Password Dialog */}
      <Dialog open={changePasswordVisible} onOpenChange={setChangePasswordVisible}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle className="flex items-center">
              <Lock className="h-5 w-5 mr-2" />
              Change Password
            </DialogTitle>
            <DialogDescription>
              {selectedEmployee && `Changing password for: ${selectedEmployee.name}`}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="new-password">New Password</Label>
              <div className="relative">
                <Input
                  id="new-password"
                  type={showPassword ? "text" : "password"}
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  placeholder="Enter new password"
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                  onClick={() => setShowPassword(!showPassword)}
                >
                  {showPassword ? (
                    <EyeOff className="h-4 w-4" />
                  ) : (
                    <Eye className="h-4 w-4" />
                  )}
                </Button>
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="confirm-password">Confirm Password</Label>
              <Input
                id="confirm-password"
                type={showPassword ? "text" : "password"}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Confirm new password"
              />
            </div>
            {newPassword && confirmPassword && newPassword !== confirmPassword && (
              <p className="text-sm text-red-500">Passwords do not match</p>
            )}
            {newPassword && newPassword.length < 6 && (
              <p className="text-sm text-yellow-500">Password must be at least 6 characters</p>
            )}
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setChangePasswordVisible(false)
                setNewPassword("")
                setConfirmPassword("")
                setSelectedEmployee(null)
              }}
            >
              Cancel
            </Button>
            <Button
              onClick={handlePasswordSubmit}
              disabled={
                passwordLoading ||
                !newPassword ||
                newPassword !== confirmPassword ||
                newPassword.length < 6
              }
            >
              {passwordLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Change Password
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

    </div>
  )
}