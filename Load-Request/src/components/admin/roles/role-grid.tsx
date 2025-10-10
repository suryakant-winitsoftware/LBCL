"use client"

import { useState, useEffect, useCallback, useMemo } from "react"
import { Edit, Trash2, Settings, Shield, Monitor, Smartphone, Crown, Eye, Building, Users, CalendarDays, UserCheck, RefreshCw } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { useToast } from "@/components/ui/use-toast"
import { DataGrid, DataGridColumn, DataGridAction, DataGridBulkAction } from "@/components/ui/data-grid"
import { Role, PagingRequest } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"
import { cn } from "@/lib/utils"

interface RoleGridProps {
  searchQuery: string
  filters: {
    isPrincipalRole: boolean[]
    isDistributorRole: boolean[]
    isAdmin: boolean[]
    isWebUser: boolean[]
    isAppUser: boolean[]
    isActive: boolean[]
  }
  refreshTrigger: number
  onView: (roleId: string) => void
  onEdit: (roleId: string) => void
  onDelete: (roleId: string) => void
  onManagePermissions: (roleId: string) => void
  onBulkAction: (action: string, selectedIds: string[]) => void
  onStatsUpdate: (stats: { total: number; active: number; principal: number; distributor: number; admin: number; web: number; mobile: number }) => void
  onRefresh?: () => void
}

export function RoleGrid({
  searchQuery,
  filters,
  refreshTrigger,
  onView,
  onEdit,
  onDelete,
  onManagePermissions,
  onBulkAction,
  onStatsUpdate,
  onRefresh
}: RoleGridProps) {
  const { toast } = useToast()
  const [data, setData] = useState<Role[]>([])
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
      
      const pagingRequest = roleService.buildRolePagingRequest(
        pagination.current,
        pagination.pageSize,
        searchQuery,
        {
          isPrincipalRole: filters.isPrincipalRole.length > 0 ? filters.isPrincipalRole[0] : undefined,
          isDistributorRole: filters.isDistributorRole.length > 0 ? filters.isDistributorRole[0] : undefined,
          isAdmin: filters.isAdmin.length > 0 ? filters.isAdmin[0] : undefined,
          isWebUser: filters.isWebUser.length > 0 ? filters.isWebUser[0] : undefined,
          isAppUser: filters.isAppUser.length > 0 ? filters.isAppUser[0] : undefined,
          isActive: filters.isActive.length > 0 ? filters.isActive[0] : undefined,
        },
        sorting.field,
        sorting.direction?.toUpperCase() as "ASC" | "DESC"
      )

      const response = await roleService.getRoles(pagingRequest)
      
      setData(response.pagedData)
      setPagination(prev => ({
        ...prev,
        total: response.totalRecords
      }))

      // Update stats with null check - backend uses PascalCase properties
      const pagedData = response.pagedData || []
      const stats = {
        total: response.totalRecords || 0,
        active: pagedData.filter(role => role.IsActive).length,
        principal: pagedData.filter(role => role.IsPrincipalRole).length,
        distributor: pagedData.filter(role => role.IsDistributorRole).length,
        admin: pagedData.filter(role => role.IsAdmin).length,
        web: pagedData.filter(role => role.IsWebUser).length,
        mobile: pagedData.filter(role => role.IsAppUser).length
      }
      onStatsUpdate(stats)

    } catch (error) {
      console.error('Failed to load roles:', error)
      toast({
        title: "Error",
        description: "Failed to load roles. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }, [pagination.current, pagination.pageSize, searchQuery, filters, sorting])

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
      key: 'RoleNameEn',
      title: 'Role Information',
      sortable: true,
      render: (value: string, record: any) => (
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <div className="font-semibold text-gray-900">{value}</div>
            {record.IsAdmin && (
              <Badge variant="destructive" className="text-xs">
                <Shield className="w-3 h-3 mr-1" />
                Admin
              </Badge>
            )}
          </div>
          <div className="flex items-center gap-4 text-xs text-gray-600">
            <span className="font-mono bg-gray-100 px-2 py-0.5 rounded">{record.Code}</span>
            {record.OrgUid && (
              <span className="flex items-center gap-1">
                <Building className="w-3 h-3" />
                {record.OrgUid}
              </span>
            )}
          </div>
        </div>
      )
    },
    {
      key: 'roleType',
      title: 'Role Type',
      width: '150px',
      render: (value: any, record: any) => (
        <div className="flex flex-wrap gap-1">
          {record.IsPrincipalRole && (
            <Badge variant="default" className="text-xs bg-purple-100 text-purple-800">
              <Crown className="w-3 h-3 mr-1" />
              Principal
            </Badge>
          )}
          {record.IsDistributorRole && (
            <Badge variant="default" className="text-xs bg-orange-100 text-orange-800">
              <Building className="w-3 h-3 mr-1" />
              Distributor
            </Badge>
          )}
          {!record.IsPrincipalRole && !record.IsDistributorRole && !record.IsAdmin && (
            <Badge variant="outline" className="text-xs">
              Standard
            </Badge>
          )}
        </div>
      )
    },
    {
      key: 'platforms',
      title: 'Platform Access',
      width: '160px',
      render: (value: any, record: any) => (
        <div className="space-y-1">
          <div className="flex gap-1">
            {record.IsWebUser ? (
              <Badge variant="secondary" className="text-xs bg-blue-100 text-blue-800">
                <Monitor className="w-3 h-3 mr-1" />
                Web
              </Badge>
            ) : (
              <Badge variant="outline" className="text-xs text-gray-400">
                <Monitor className="w-3 h-3 mr-1" />
                No Web
              </Badge>
            )}
            {record.IsAppUser ? (
              <Badge variant="secondary" className="text-xs bg-green-100 text-green-800">
                <Smartphone className="w-3 h-3 mr-1" />
                Mobile
              </Badge>
            ) : (
              <Badge variant="outline" className="text-xs text-gray-400">
                <Smartphone className="w-3 h-3 mr-1" />
                No Mobile
              </Badge>
            )}
          </div>
        </div>
      )
    },
    {
      key: 'ParentRoleName',
      title: 'Parent Role',
      width: '130px',
      sortable: true,
      render: (value: string, record: any) => (
        <div className="flex items-center gap-1">
          {value ? (
            <>
              <Users className="w-3 h-3 text-gray-500" />
              <span className="text-sm">{value}</span>
            </>
          ) : (
            <span className="text-sm text-gray-400">—</span>
          )}
        </div>
      )
    },
    {
      key: 'IsActive',
      title: 'Status',
      width: '100px',
      sortable: true,
      render: (value: boolean) => (
        <Badge 
          variant={value ? 'default' : 'secondary'}
          className={cn(
            'font-medium',
            value ? 'bg-green-100 text-green-800 hover:bg-green-100' : 'bg-red-100 text-red-800 hover:bg-red-100'
          )}
        >
          {value ? 'Active' : 'Inactive'}
        </Badge>
      )
    },
    {
      key: 'userInfo',
      title: 'Created/Modified',
      width: '180px',
      sortable: true,
      render: (value: any, record: any) => (
        <div className="space-y-1 text-xs">
          <div className="flex items-center gap-1 text-gray-600">
            <UserCheck className="w-3 h-3" />
            <span>{record.CreatedBy || 'System'}</span>
          </div>
          <div className="flex items-center gap-1 text-gray-500">
            <CalendarDays className="w-3 h-3" />
            <span>
              {record.ModifiedTime 
                ? new Date(record.ModifiedTime).toLocaleDateString('en-US', { 
                    month: 'short', 
                    day: 'numeric', 
                    year: 'numeric' 
                  })
                : record.CreatedTime 
                  ? new Date(record.CreatedTime).toLocaleDateString('en-US', { 
                      month: 'short', 
                      day: 'numeric', 
                      year: 'numeric' 
                    })
                  : 'N/A'}
            </span>
          </div>
        </div>
      )
    }
  ], [])

  // Define actions
  const actions: DataGridAction<Role>[] = useMemo(() => [
    {
      key: 'view',
      label: 'View Details',
      icon: Eye,
      onClick: (record) => onView(record.UID)
    },
    {
      key: 'edit',
      label: 'Edit Role',
      icon: Edit,
      onClick: (record) => onEdit(record.UID)
    },
    {
      key: 'manage-permissions',
      label: 'Manage Permissions',
      icon: Settings,
      onClick: (record) => onManagePermissions(record.UID)
    },
    {
      key: 'delete',
      label: 'Deactivate',
      icon: Trash2,
      onClick: (record) => onDelete(record.UID),
      disabled: (record) => !record.IsActive,
      variant: 'destructive'
    },
    {
      key: 'reactivate',
      label: 'Reactivate',
      icon: RefreshCw,
      onClick: async (record) => {
        try {
          // Reactivate the role by updating IsActive to true
          const reactivatedRole = {
            ...record,
            IsActive: true,
            ModifiedBy: "ADMIN",
            ModifiedTime: new Date().toISOString()
          }
          await roleService.updateRole(reactivatedRole)
          toast({
            title: "Success",
            description: "Role has been reactivated successfully.",
          })
          // Trigger refresh to update the grid
          if (onRefresh) {
            onRefresh()
          } else {
            // If no refresh callback provided, reload data
            loadData()
          }
        } catch (error) {
          toast({
            title: "Error",
            description: "Failed to reactivate role. Please try again.",
            variant: "destructive",
          })
        }
      },
      disabled: (record) => record.IsActive, // Only show for inactive roles
      variant: 'default'
    }
  ], [onView, onEdit, onDelete, onManagePermissions, toast, loadData, onRefresh])

  // Define bulk actions
  const bulkActions: DataGridBulkAction[] = useMemo(() => [
    {
      key: 'activate',
      label: 'Activate',
      icon: Shield
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      icon: Trash2,
      variant: 'destructive'
    },
    {
      key: 'enable-web',
      label: 'Enable Web Access',
      icon: Monitor
    },
    {
      key: 'enable-mobile',
      label: 'Enable Mobile Access',
      icon: Smartphone
    },
    {
      key: 'export',
      label: 'Export Selected'
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
      emptyText="No roles found. Try adjusting your search or filters."
      className="min-h-[400px]"
    />
  )
}