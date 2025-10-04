"use client"

import { useState, useEffect, useCallback, useMemo, useRef } from "react"
import { Edit, Trash2, UserCheck, Download, FileText, Lock, Shield } from "lucide-react"
import { roleService } from "@/services/admin/role.service"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { useToast } from "@/components/ui/use-toast"
import { DataGrid, DataGridColumn, DataGridAction, DataGridBulkAction } from "@/components/ui/data-grid"
import { Employee, PagingRequest, PagedResponse } from "@/types/admin.types"
import { employeeService } from "@/services/admin/employee.service"
import { cn } from "@/lib/utils"
import { formatDateToDayMonthYear } from "@/utils/date-formatter"
import { OTPGeneratorDialog } from "./otp-generator-dialog"

interface EmployeeGridProps {
  searchQuery: string
  filters: {
    status: string[]
    roles: string[]
    organizations: string[]
  }
  refreshTrigger: number
  onEdit: (employeeId: string) => void
  onDelete?: (employeeId: string) => void
  onDeactivate?: (employeeId: string) => void
  onActivate?: (employeeId: string) => void
  onViewPermissions?: (employeeId: string) => void
  onViewDetails?: (employeeId: string) => void
  onChangePassword?: (employeeId: string, employeeName: string) => void
  onBulkAction: (action: string, selectedIds: string[]) => void
  onStatsUpdate: (stats: { total: number; active: number; inactive: number; pending: number }) => void
  onRolesUpdate?: (roles: Array<{uid: string, name: string}>) => void
}

export function EmployeeGrid({
  searchQuery,
  filters,
  refreshTrigger,
  onEdit,
  onDelete,
  onDeactivate,
  onActivate,
  onViewPermissions,
  onViewDetails,
  onChangePassword,
  onBulkAction,
  onStatsUpdate,
  onRolesUpdate
}: EmployeeGridProps) {
  const { toast } = useToast()
  const [allData, setAllData] = useState<Employee[]>([]) // Store all employees
  const [data, setData] = useState<Employee[]>([]) // Store filtered/paginated data
  const [loading, setLoading] = useState(false)
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([])
  const isLoadingRef = useRef(false) // Use ref to track loading state
  const hasLoadedRef = useRef(false) // Use ref to track if data has been loaded

  // OTP Generator Dialog State
  const [otpDialogOpen, setOtpDialogOpen] = useState(false)
  const [selectedEmployeeForOTP, setSelectedEmployeeForOTP] = useState<{
    name: string
    encryptedPassword: string
  } | null>(null)

  // Pagination state
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 10,
    total: 0
  })

  // Sorting state
  const [sorting, setSorting] = useState<{
    field?: string
    direction?: "asc" | "desc"
  }>({})

  // Load all data from backend (once)
  const loadAllData = async () => {
    // Prevent multiple simultaneous calls using ref
    if (isLoadingRef.current || hasLoadedRef.current) {
      console.log('Skipping load - already loading or loaded')
      return;
    }
    
    console.log('Starting to load employee data...')
    isLoadingRef.current = true;
    setLoading(true)
    
    try {
      // Fetch a reasonable amount of employees
      const pagingRequest = employeeService.buildPagingRequest(
        1,
        100, // Start with just 100 employees
        undefined, // No search
        undefined, // No filters
        undefined,
        undefined
      )

      const response = await employeeService.getEmployees(pagingRequest)
      
      let allEmployees = response.pagedData || []
      
      // Enhance employee data with role information
      const enhancedEmployees = await Promise.all(
        allEmployees.map(async (emp: any) => {
          try {
            // Get detailed employee data that includes JobPosition
            const detailedEmp = await employeeService.getEmployeeById(emp.UID)
            let roleName = null
            
            // If employee has a role, fetch role details
            if (detailedEmp?.JobPosition?.UserRoleUID) {
              try {
                const roleInfo = await roleService.getRoleById(detailedEmp.JobPosition.UserRoleUID)
                roleName = roleInfo?.Name || roleInfo?.RoleNameEn || roleInfo?.Code
              } catch (roleError) {
                console.warn(`Failed to load role for employee ${emp.UID}:`, roleError)
              }
            }
            
            return {
              ...emp,
              RoleName: roleName,
              JobPosition: detailedEmp?.JobPosition
            }
          } catch (error) {
            console.warn(`Failed to enhance employee ${emp.UID}:`, error)
            return emp // Return original employee data if enhancement fails
          }
        })
      )
      
      setAllData(enhancedEmployees)
      hasLoadedRef.current = true
      
      // Calculate stats from all data
      const stats = {
        total: response.totalRecords || enhancedEmployees.length,
        active: enhancedEmployees.filter((e: any) => e.Status === 'Active').length,
        inactive: enhancedEmployees.filter((e: any) => e.Status === 'Inactive').length,
        pending: enhancedEmployees.filter((e: any) => e.Status === 'Pending').length
      }
      onStatsUpdate(stats)
      
      // Extract unique roles for filtering
      if (onRolesUpdate) {
        const uniqueRoles = new Map<string, string>()
        enhancedEmployees.forEach((emp: any) => {
          if (emp.RoleName && emp.JobPosition?.UserRoleUID) {
            uniqueRoles.set(emp.JobPosition.UserRoleUID, emp.RoleName)
          }
        })
        
        const roleOptions = Array.from(uniqueRoles.entries()).map(([uid, name]) => ({
          uid,
          name
        }))
        
        onRolesUpdate(roleOptions)
      }
      
      console.log('Successfully loaded', enhancedEmployees.length, 'employees with role data')

    } catch (error) {
      console.error('Failed to load employees:', error)
      toast({
        title: "Error",
        description: "Failed to load employees. Please try again.",
        variant: "destructive",
      })
      // Set empty data on error to prevent infinite loading
      setAllData([])
      setData([])
      hasLoadedRef.current = true // Mark as loaded even on error to prevent retries
    } finally {
      setLoading(false)
      isLoadingRef.current = false
    }
  }

  // Filter and search data locally
  const filterData = useCallback(() => {
    let filteredData = [...allData]
    
    // Apply search filter
    if (searchQuery) {
      const query = searchQuery.toLowerCase()
      filteredData = filteredData.filter((employee: any) => {
        return (
          employee.Name?.toLowerCase().includes(query) ||
          employee.Code?.toLowerCase().includes(query) ||
          employee.LoginId?.toLowerCase().includes(query) ||
          employee.Email?.toLowerCase().includes(query)
        )
      })
    }
    
    // Apply status filter
    if (filters.status.length > 0) {
      filteredData = filteredData.filter((employee: any) => 
        filters.status.includes(employee.Status)
      )
    }
    
    // Apply role filter
    if (filters.roles.length > 0) {
      filteredData = filteredData.filter((employee: any) => {
        const roleName = employee.RoleName
        return roleName && filters.roles.includes(roleName)
      })
    }
    
    // Apply sorting
    if (sorting.field) {
      filteredData.sort((a: any, b: any) => {
        const aValue = a[sorting.field!] || ''
        const bValue = b[sorting.field!] || ''
        
        if (sorting.direction === 'desc') {
          return bValue > aValue ? 1 : -1
        } else {
          return aValue > bValue ? 1 : -1
        }
      })
    }
    
    // Update pagination total
    setPagination(prev => ({
      ...prev,
      total: filteredData.length
    }))
    
    // Apply pagination
    const start = (pagination.current - 1) * pagination.pageSize
    const end = start + pagination.pageSize
    const paginatedData = filteredData.slice(start, end)
    
    setData(paginatedData)
  }, [allData, searchQuery, filters, sorting.field, sorting.direction, pagination.current, pagination.pageSize])

  // Load all data on mount only
  useEffect(() => {
    loadAllData()
  }, []) // Only run once on mount
  
  // Handle refresh trigger
  useEffect(() => {
    if (refreshTrigger > 0) {
      hasLoadedRef.current = false
      loadAllData()
    }
  }, [refreshTrigger])
  
  // Filter data when search, filters, sorting, or pagination changes
  useEffect(() => {
    if (allData.length > 0 || hasLoadedRef.current) {
      filterData()
    }
  }, [filterData, allData])
  
  // Reset to page 1 when search or filters change
  useEffect(() => {
    setPagination(prev => ({
      ...prev,
      current: 1
    }))
  }, [searchQuery, filters])

  // Handle pagination change
  const handlePaginationChange = useCallback((page: number, pageSize: number) => {
    setPagination(prev => ({
      ...prev,
      current: page,
      pageSize
    }))
  }, [])

  // Handle sorting change
  const handleSortingChange = useCallback((field: string, direction: "asc" | "desc") => {
    console.log('Sorting changed:', field, direction)
    setSorting({ field, direction })
    setPagination(prev => ({ ...prev, current: 1 })) // Reset to first page
  }, [])

  // Handle selection change
  const handleSelectionChange = useCallback((keys: string[]) => {
    setSelectedRowKeys(keys)
  }, [])

  // Handle bulk actions
  const handleBulkActionClick = useCallback((actionKey: string) => {
    onBulkAction(actionKey, selectedRowKeys)
    setSelectedRowKeys([]) // Clear selection after action
  }, [onBulkAction, selectedRowKeys])

  // Define columns - using PascalCase keys to match backend data format
  const columns: DataGridColumn<any>[] = useMemo(() => [
    {
      key: 'Code',
      title: 'Employee Code',
      width: '120px',
      sortable: true,
      className: 'pl-6',
      render: (value: string, record: any) => (
        <span className="font-mono text-sm">{value}</span>
      )
    },
    {
      key: 'Name',
      title: 'Name',
      width: '200px',
      sortable: true,
      render: (value: string, record: any) => (
        <div>
          <div className="font-medium">{value}</div>
        </div>
      )
    },
    {
      key: 'LoginId',
      title: 'Login ID',
      width: '180px',
      sortable: true,
      render: (value: string) => (
        <span className="text-sm font-mono text-blue-600">{value || 'N/A'}</span>
      )
    },
    {
      key: 'RoleName',
      title: 'Role',
      width: '150px',
      sortable: true,
      render: (value: string, record: any) => {
        const roleName = value || record.RoleName
        return (
          <span className="text-sm">{roleName || 'No Role'}</span>
        )
      }
    },
    {
      key: 'Status',
      title: 'Status',
      width: '100px',
      sortable: true,
      render: (value: string) => (
        <Badge 
          variant={value === 'Active' ? 'default' : value === 'Inactive' ? 'secondary' : 'destructive'}
          className={cn(
            value === 'Active' && 'bg-green-100 text-green-800 hover:bg-green-100',
            value === 'Inactive' && 'bg-red-100 text-red-800 hover:bg-red-100',
            value === 'Pending' && 'bg-yellow-100 text-yellow-800 hover:bg-yellow-100'
          )}
        >
          {value}
        </Badge>
      )
    },
    {
      key: 'ModifiedTime',
      title: 'Modified',
      width: '110px',
      sortable: true,
      render: (value: string) => {
        if (!value) return <span className="text-sm text-muted-foreground">N/A</span>;
        return <span className="text-sm text-muted-foreground">{formatDateToDayMonthYear(value)}</span>;
      }
    }
  ], [])

  // Handler for OTP generation
  const handleGenerateOTP = useCallback(async (record: any) => {
    try {
      // Fetch employee details to get ENCRIPTED_PASSWORD from emp table
      const employeeDetails = await employeeService.getEmployeeById(record.UID)

      console.log("Full employee details response:", employeeDetails)
      console.log("Checking for ENCRIPTED_PASSWORD in:", {
        "employeeDetails.Emp": employeeDetails?.Emp,
        "employeeDetails.emp": employeeDetails?.emp,
        "employeeDetails": employeeDetails
      })

      // Try multiple possible field locations and spellings
      const encryptedPassword =
        // Try direct object fields
        employeeDetails?.ENCRIPTED_PASSWORD ||
        employeeDetails?.EncriptedPassword ||
        employeeDetails?.encriptedPassword ||
        employeeDetails?.ENCRYPTED_PASSWORD ||
        employeeDetails?.EncryptedPassword ||
        employeeDetails?.encryptedPassword ||
        // Try nested Emp object (PascalCase)
        employeeDetails?.Emp?.ENCRIPTED_PASSWORD ||
        employeeDetails?.Emp?.EncriptedPassword ||
        employeeDetails?.Emp?.ENCRYPTED_PASSWORD ||
        employeeDetails?.Emp?.EncryptedPassword ||
        employeeDetails?.Emp?.Password ||
        // Try nested emp object (camelCase)
        employeeDetails?.emp?.ENCRIPTED_PASSWORD ||
        employeeDetails?.emp?.encriptedPassword ||
        employeeDetails?.emp?.ENCRYPTED_PASSWORD ||
        employeeDetails?.emp?.encryptedPassword ||
        employeeDetails?.emp?.password ||
        null

      if (!encryptedPassword) {
        toast({
          title: "Error",
          description: "Employee encrypted password not found. Please check the database field mapping.",
          variant: "destructive",
        })
        console.error("ENCRIPTED_PASSWORD field not found. Available fields:", Object.keys(employeeDetails || {}))
        if (employeeDetails?.Emp) {
          console.error("Available Emp fields:", Object.keys(employeeDetails.Emp))
        }
        if (employeeDetails?.emp) {
          console.error("Available emp fields:", Object.keys(employeeDetails.emp))
        }
        return
      }

      console.log("Found encrypted password, length:", encryptedPassword?.length)

      setSelectedEmployeeForOTP({
        name: record.Name || record.name || 'Unknown',
        encryptedPassword: encryptedPassword
      })
      setOtpDialogOpen(true)
    } catch (error) {
      console.error("Failed to fetch employee details for OTP:", error)
      toast({
        title: "Error",
        description: "Failed to load employee details for OTP generation.",
        variant: "destructive",
      })
    }
  }, [toast])

  // Define actions - using PascalCase properties to match backend data
  const actions: DataGridAction<any>[] = useMemo(() => {
    const baseActions = [
      {
        key: 'view-details',
        label: 'View Details',
        icon: FileText,
        onClick: (record: any) => onViewDetails?.(record.UID)
      },
      {
        key: 'edit',
        label: 'Edit Employee',
        icon: Edit,
        onClick: (record: any) => onEdit(record.UID)
      },
      {
        key: 'generate-otp',
        label: 'Generate OTP',
        icon: Shield,
        onClick: (record: any) => handleGenerateOTP(record)
      },
      {
        key: 'change-password',
        label: 'Change Password',
        icon: Lock,
        onClick: (record: any) => onChangePassword?.(record.UID, record.Name)
      }
    ]

    // Add activate/deactivate based on status
    if (onActivate) {
      baseActions.push({
        key: 'activate',
        label: 'Activate',
        icon: UserCheck,
        onClick: (record) => onActivate(record.UID),
        disabled: (record) => record.Status === 'Active',
        variant: 'default'
      })
    }

    if (onDeactivate) {
      baseActions.push({
        key: 'deactivate',
        label: 'Deactivate',
        icon: Trash2,
        onClick: (record) => onDeactivate(record.UID),
        disabled: (record) => record.Status === 'Inactive',
        variant: 'destructive'
      })
    }

    return baseActions
  }, [onEdit, onDeactivate, onActivate, onViewDetails, onChangePassword, handleGenerateOTP])

  // Define bulk actions
  const bulkActions: DataGridBulkAction[] = useMemo(() => [
    {
      key: 'activate',
      label: 'Activate',
      icon: UserCheck
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      icon: Trash2,
      variant: 'destructive'
    },
    {
      key: 'export',
      label: 'Export Selected',
      icon: Download
    }
  ], [])

  return (
    <>
      <DataGrid
        data={data}
        columns={columns}
        loading={loading}
        pagination={{
          current: pagination.current,
          pageSize: pagination.pageSize,
          total: pagination.total,
          showSizeChanger: true,
          pageSizeOptions: [10, 20, 50, 100],
          onChange: handlePaginationChange
        }}
        sorting={{
          field: sorting.field,
          direction: sorting.direction,
          onChange: handleSortingChange
        }}
        // selection={{
        //   selectedRowKeys,
        //   onChange: handleSelectionChange,
        //   getRowKey: (record) => record.UID
        // }}
        actions={actions}
        bulkActions={bulkActions}
        onBulkAction={handleBulkActionClick}
        emptyText="No employees found. Try adjusting your search or filters."
        className="min-h-[400px]"
      />

      {/* OTP Generator Dialog */}
      {selectedEmployeeForOTP && (
        <OTPGeneratorDialog
          open={otpDialogOpen}
          onOpenChange={setOtpDialogOpen}
          employeeName={selectedEmployeeForOTP.name}
          encryptedPassword={selectedEmployeeForOTP.encryptedPassword}
        />
      )}
    </>
  )
}