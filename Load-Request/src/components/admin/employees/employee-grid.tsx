"use client"

import { useState, useEffect, useCallback, useMemo } from "react"
import { Edit, Trash2, Eye, Key, UserCheck, Download, FileText, Smartphone } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { useToast } from "@/components/ui/use-toast"
import { DataGrid, DataGridColumn, DataGridAction, DataGridBulkAction } from "@/components/ui/data-grid"
import { Employee, PagingRequest, PagedResponse } from "@/types/admin.types"
import { employeeService } from "@/services/admin/employee.service"
import { cn } from "@/lib/utils"

interface EmployeeGridProps {
  searchQuery: string
  filters: {
    status: string[]
    roles: string[]
    organizations: string[]
  }
  refreshTrigger: number
  onEdit: (employeeId: string) => void
  onDelete: (employeeId: string) => void
  onDeactivate?: (employeeId: string) => void
  onActivate?: (employeeId: string) => void
  onViewPermissions: (employeeId: string) => void
  onViewDetails?: (employeeId: string) => void
  onBulkAction: (action: string, selectedIds: string[]) => void
  onStatsUpdate: (stats: { total: number; active: number; inactive: number; pending: number }) => void
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
  onBulkAction,
  onStatsUpdate
}: EmployeeGridProps) {
  const { toast } = useToast()
  const [data, setData] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([])
  
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

  // Load data
  const loadData = useCallback(async () => {
    try {
      setLoading(true)
      
      const pagingRequest = employeeService.buildPagingRequest(
        pagination.current,
        pagination.pageSize,
        searchQuery,
        {
          status: filters.status.length > 0 ? filters.status : undefined,
          // Add more filters as needed
        },
        sorting.field,
        sorting.direction?.toUpperCase() as "ASC" | "DESC"
      )

      const response = await employeeService.getEmployees(pagingRequest)
      
      const pagedData = response.pagedData || []
      
      setData(pagedData)
      setPagination(prev => ({
        ...prev,
        total: response.totalRecords || 0
      }))

      // Get stats from backend response or calculate from total data
      // Note: These counts should ideally come from backend aggregation
      const stats = {
        total: response.totalRecords || 0,
        active: response.activeCount || 0,
        inactive: response.inactiveCount || 0,
        pending: response.pendingCount || 0
      }
      onStatsUpdate(stats)

    } catch (error) {
      console.error('Failed to load employees:', error)
      toast({
        title: "Error",
        description: "Failed to load employees. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }, [pagination.current, pagination.pageSize, searchQuery, filters, sorting.field, sorting.direction])

  // Load data on mount and when dependencies change
  useEffect(() => {
    loadData()
  }, [loadData, refreshTrigger])

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
      render: (value: string, record: any) => (
        <span className="font-mono text-sm">{value}</span>
      )
    },
    {
      key: 'Name',
      title: 'Name',
      sortable: true,
      render: (value: string, record: any) => (
        <div>
          <div className="font-medium">{value}</div>
          <div className="text-sm text-muted-foreground">{record.LoginId}</div>
        </div>
      )
    },
    {
      key: 'Email',
      title: 'Email',
      width: '200px',
      render: (value: string) => (
        <span className="text-sm">{value || 'N/A'}</span>
      )
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
      key: 'MobileAccess',
      title: 'Mobile Access',
      width: '120px',
      sortable: false,
      render: (value: any, record: any) => {
        // This will be populated if the backend includes mobile access info
        // For now, we'll check if the record has a MobileAppAccess field
        const hasMobileAccess = record.MobileAppAccess || record.HasMobileAccess;
        
        if (hasMobileAccess) {
          return (
            <div className="flex items-center gap-2">
              <Smartphone className="h-4 w-4 text-green-600" />
              <span className="text-sm text-green-600">Enabled</span>
            </div>
          );
        }
        return (
          <div className="flex items-center gap-2">
            <Smartphone className="h-4 w-4 text-gray-400" />
            <span className="text-sm text-gray-400">Disabled</span>
          </div>
        );
      }
    },
    {
      key: 'ModifiedTime',
      title: 'Modified',
      width: '140px',
      sortable: true,
      render: (value: string) => {
        if (!value) return <span className="text-sm">N/A</span>;
        const date = new Date(value);
        const day = String(date.getDate()).padStart(2, '0');
        const month = date.toLocaleDateString('en-US', { month: 'short' });
        const year = date.getFullYear();
        return <span className="text-sm">{`${day}/${month}/${year}`}</span>;
      }
    }
  ], [])

  // Define actions - using PascalCase properties to match backend data
  const actions: DataGridAction<any>[] = useMemo(() => {
    const baseActions = [
      {
        key: 'view-details',
        label: 'View Details',
        icon: FileText,
        onClick: (record) => onViewDetails?.(record.UID)
      },
      {
        key: 'view-permissions',
        label: 'View Permissions',
        icon: Eye,
        onClick: (record) => onViewPermissions(record.UID)
      },
      {
        key: 'edit',
        label: 'Edit Employee',
        icon: Edit,
        onClick: (record) => onEdit(record.UID)
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

    // Add permanent delete
    baseActions.push({
      key: 'delete',
      label: 'Delete Permanently',
      icon: Trash2,
      onClick: (record) => onDelete(record.UID),
      variant: 'destructive'
    })

    return baseActions
  }, [onEdit, onDelete, onDeactivate, onActivate, onViewPermissions])

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
      selection={{
        selectedRowKeys,
        onChange: handleSelectionChange,
        getRowKey: (record) => record.UID
      }}
      actions={actions}
      bulkActions={bulkActions}
      onBulkAction={handleBulkActionClick}
      emptyText="No employees found. Try adjusting your search or filters."
      className="min-h-[400px]"
    />
  )
}