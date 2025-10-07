"use client"

import { useState, useEffect, useCallback, useMemo, useRef } from "react"
import { Eye, Edit, Trash2, Calendar, Users, MapPin } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import { DataGrid, DataGridColumn, DataGridAction, DataGridBulkAction } from "@/components/ui/data-grid"
import { Route, PagedRequest, routeService } from "@/services/routeService"
import { cn } from "@/lib/utils"
import moment from "moment"
import { authService } from "@/lib/auth-service"

interface RouteGridProps {
  searchQuery: string
  filters: {
    orgUID: string
    status: string
    isActive: boolean | null
    dateRange?: {
      startDate: string
      endDate: string
      dateType: 'created' | 'modified' | 'valid'
    }
    jobPosition?: string
    hasCustomers?: boolean | null
  }
  refreshTrigger: number
  onView: (routeId: string) => void
  onEdit: (routeId: string) => void
  onDelete: (routeId: string) => void
  onBulkAction?: (action: string, selectedIds: string[]) => void
}

// Stable empty array for initial state
const EMPTY_ROUTES: Route[] = []

export function RouteGrid({
  searchQuery,
  filters,
  refreshTrigger,
  onView,
  onEdit,
  onDelete,
  onBulkAction
}: RouteGridProps) {
  const { toast } = useToast()
  const [routes, setRoutes] = useState<Route[]>(EMPTY_ROUTES)
  const [loading, setLoading] = useState(false)
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([])
  
  // Track if initial load has been done
  const initialLoadDone = useRef(false)
  const abortControllerRef = useRef<AbortController | null>(null)
  const lastLoadParams = useRef<string>("")
  
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
  }>({
    field: 'ModifiedTime',
    direction: 'desc'
  })

  // Load routes data
  useEffect(() => {
    // Create a unique key for current load parameters
    const loadKey = JSON.stringify({
      orgUID: filters.orgUID,
      status: filters.status,
      isActive: filters.isActive,
      refreshTrigger
    })
    
    // Skip if params haven't changed and we've already loaded
    if (loadKey === lastLoadParams.current && initialLoadDone.current) {
      return
    }
    
    lastLoadParams.current = loadKey

    const loadRoutes = async () => {
      // Cancel any pending request
      if (abortControllerRef.current) {
        abortControllerRef.current.abort()
      }
      
      // Create new abort controller
      abortControllerRef.current = new AbortController()
      
      try {
        setLoading(true)
        
        // Get current user's organization
        const currentUser = authService.getCurrentUser()
        let orgUID = filters.orgUID
        
        if (!orgUID && filters.orgUID !== '') {
          if (currentUser?.currentOrganization?.uid) {
            orgUID = currentUser.currentOrganization.uid
          } else if (currentUser?.availableOrganizations?.length > 0) {
            orgUID = currentUser.availableOrganizations[0].uid
          }
        }
        
        // Build filter criteria
        const filterCriteria = []
        if (filters.status) {
          filterCriteria.push({
            fieldName: 'Status',
            operator: 'EQ',
            value: filters.status
          })
        }
        
        if (filters.isActive !== null) {
          filterCriteria.push({
            fieldName: 'IsActive',
            operator: 'EQ',
            value: filters.isActive
          })
        }
        
        const request: PagedRequest = {
          pageNumber: 1,
          pageSize: 10000,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: filterCriteria
        }

        // Check if request was aborted
        if (abortControllerRef.current.signal.aborted) {
          return
        }

        const response = await routeService.getRoutes(request, orgUID)
        
        // Check again if request was aborted
        if (abortControllerRef.current.signal.aborted) {
          return
        }
        
        if (response.IsSuccess && response.Data) {
          const uniqueRoutes = deduplicateRoutes(response.Data.PagedData || [])
          setRoutes(uniqueRoutes)
          setPagination(prev => ({
            ...prev,
            total: uniqueRoutes.length,
            current: 1
          }))
        } else {
          setRoutes(EMPTY_ROUTES)
          setPagination(prev => ({
            ...prev,
            total: 0,
            current: 1
          }))
        }
        
        initialLoadDone.current = true
      } catch (error: any) {
        // Ignore abort errors
        if (error.name === 'AbortError') {
          return
        }
        
        console.error('Failed to load routes:', error)
        toast({
          title: "Error",
          description: "Failed to load routes. Please try again.",
          variant: "destructive",
        })
        setRoutes(EMPTY_ROUTES)
      } finally {
        setLoading(false)
      }
    }

    loadRoutes()
    
    // Cleanup function
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort()
      }
    }
  }, [filters.orgUID, filters.status, filters.isActive, refreshTrigger, toast])

  // Deduplicate routes helper
  const deduplicateRoutes = (routes: Route[]): Route[] => {
    const seen = new Map<string, Route>()
    
    routes.forEach(route => {
      const existingRoute = seen.get(route.UID)
      
      if (!existingRoute) {
        seen.set(route.UID, route)
      } else {
        const existingTime = existingRoute.ModifiedTime ? new Date(existingRoute.ModifiedTime).getTime() : 0
        const currentTime = route.ModifiedTime ? new Date(route.ModifiedTime).getTime() : 0
        
        if (currentTime > existingTime) {
          seen.set(route.UID, route)
        }
      }
    })
    
    return Array.from(seen.values())
  }

  // Filter and paginate data
  const processedData = useMemo(() => {
    let filteredData = [...routes]

    // Search filter
    if (searchQuery) {
      const query = searchQuery.toLowerCase()
      filteredData = filteredData.filter(route => 
        route.Code?.toLowerCase().includes(query) ||
        route.Name?.toLowerCase().includes(query) ||
        route.UID?.toLowerCase().includes(query)
      )
    }

    // Additional client-side filters
    if (filters.dateRange?.startDate || filters.dateRange?.endDate) {
      const { startDate, endDate, dateType } = filters.dateRange
      const start = startDate ? new Date(startDate).getTime() : 0
      const end = endDate ? new Date(endDate + 'T23:59:59').getTime() : Date.now()
      
      filteredData = filteredData.filter(route => {
        let dateField: string | undefined
        switch (dateType) {
          case 'created':
            dateField = route.CreatedTime
            break
          case 'modified':
            dateField = route.ModifiedTime
            break
          case 'valid':
            dateField = route.ValidFrom
            break
        }
        
        if (!dateField) return false
        const routeDate = new Date(dateField).getTime()
        return routeDate >= start && routeDate <= end
      })
    }

    if (filters.hasCustomers !== null && filters.hasCustomers !== undefined) {
      filteredData = filteredData.filter(route => 
        filters.hasCustomers ? route.TotalCustomers > 0 : route.TotalCustomers === 0
      )
    }

    if (filters.jobPosition) {
      filteredData = filteredData.filter(route => 
        route.JobPositionUID?.toLowerCase().includes(filters.jobPosition.toLowerCase())
      )
    }

    // Apply sorting
    if (sorting.field) {
      filteredData.sort((a, b) => {
        const aValue = a[sorting.field as keyof Route]
        const bValue = b[sorting.field as keyof Route]
        
        if (aValue == null && bValue == null) return 0
        if (aValue == null) return sorting.direction === 'asc' ? 1 : -1
        if (bValue == null) return sorting.direction === 'asc' ? -1 : 1
        
        // Date fields
        const dateFields = ['ValidFrom', 'ValidUpto', 'CreatedTime', 'ModifiedTime']
        if (dateFields.includes(sorting.field!)) {
          const aDate = new Date(aValue as string).getTime()
          const bDate = new Date(bValue as string).getTime()
          return sorting.direction === 'asc' ? aDate - bDate : bDate - aDate
        }
        
        // String comparison
        const aStr = String(aValue).toLowerCase()
        const bStr = String(bValue).toLowerCase()
        
        if (aStr < bStr) return sorting.direction === 'asc' ? -1 : 1
        if (aStr > bStr) return sorting.direction === 'asc' ? 1 : -1
        return 0
      })
    }

    // Calculate pagination
    const total = filteredData.length
    const startIndex = (pagination.current - 1) * pagination.pageSize
    const endIndex = startIndex + pagination.pageSize
    const paginatedData = filteredData.slice(startIndex, endIndex)
    
    return {
      data: paginatedData,
      total
    }
  }, [routes, searchQuery, filters.dateRange, filters.hasCustomers, filters.jobPosition, sorting, pagination.current, pagination.pageSize])

  // Update pagination total when filtered data changes
  useEffect(() => {
    setPagination(prev => {
      if (prev.total !== processedData.total) {
        return { ...prev, total: processedData.total, current: 1 }
      }
      return prev
    })
  }, [processedData.total])

  // Handlers
  const handlePaginationChange = useCallback((page: number, pageSize: number) => {
    setPagination(prev => ({ ...prev, current: page, pageSize }))
  }, [])

  const handleSortingChange = useCallback((field: string, direction: "asc" | "desc") => {
    setSorting({ field, direction })
  }, [])

  const handleSelectionChange = useCallback((keys: string[]) => {
    setSelectedRowKeys(keys)
  }, [])

  const handleBulkActionClick = useCallback((actionKey: string) => {
    if (onBulkAction) {
      onBulkAction(actionKey, selectedRowKeys)
      setSelectedRowKeys([])
    }
  }, [onBulkAction, selectedRowKeys])

  // Define columns
  const columns: DataGridColumn<Route>[] = useMemo(() => [
    {
      key: 'Code',
      title: 'Route',
      width: '250px',
      sortable: true,
      fixed: 'left',
      render: (value: string, record: Route) => (
        <div className="space-y-2">
          <div>
            <div className="font-semibold text-sm">{value}</div>
            <div className="text-sm text-muted-foreground">{record.Name}</div>
          </div>
          <div className="flex items-center gap-2">
            <Badge 
              variant={record.IsActive ? 'default' : 'secondary'}
              className={cn(
                "text-xs",
                record.IsActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'
              )}
            >
              {record.IsActive ? 'Active' : 'Inactive'}
            </Badge>
            <Badge variant="outline" className="text-xs">
              <Users className="h-3 w-3 mr-1" />
              {record.TotalCustomers || 0}
            </Badge>
          </div>
        </div>
      )
    },
    {
      key: 'OrgUID',
      title: 'Organization',
      width: '150px',
      sortable: true,
      render: (value: string) => (
        <div className="text-sm font-medium">{value || '-'}</div>
      )
    },
    {
      key: 'JobPositionUID',
      title: 'Assignment',
      width: '180px',
      render: (value: string, record: Route) => (
        <div className="space-y-1">
          <div className="text-sm">
            <div className="font-medium">{value || 'Unassigned'}</div>
            <div className="text-xs text-muted-foreground">{record.RoleUID}</div>
          </div>
          {record.VehicleUID && (
            <Badge variant="outline" className="text-xs">
              <MapPin className="h-3 w-3 mr-1" />
              {record.VehicleUID}
            </Badge>
          )}
        </div>
      )
    },
    {
      key: 'ValidFrom',
      title: 'Validity Period',
      width: '180px',
      sortable: true,
      render: (value: string, record: Route) => (
        <div className="space-y-1">
          <div className="flex items-center gap-1 text-xs">
            <Calendar className="h-3 w-3 text-muted-foreground" />
            <span className="font-medium">{moment(value).format('DD MMM YYYY')}</span>
          </div>
          <div className="text-xs text-muted-foreground">
            to {moment(record.ValidUpto).format('DD MMM YYYY')}
          </div>
        </div>
      )
    },
    {
      key: 'ModifiedTime',
      title: 'Last Modified',
      width: '140px',
      sortable: true,
      render: (value: string) => (
        <div className="text-xs text-muted-foreground">
          {value ? (
            <>
              <div>{moment(value).format('DD MMM YY')}</div>
              <div>{moment(value).format('HH:mm')}</div>
            </>
          ) : '-'}
        </div>
      )
    }
  ], [])

  // Define actions
  const actions: DataGridAction<Route>[] = useMemo(() => [
    {
      key: 'view',
      label: 'View Details',
      icon: Eye,
      onClick: (record) => onView(record.UID)
    },
    {
      key: 'edit',
      label: 'Edit Route',
      icon: Edit,
      onClick: (record) => onEdit(record.UID)
    },
    {
      key: 'delete',
      label: 'Delete Route',
      icon: Trash2,
      onClick: (record) => onDelete(record.UID),
      variant: 'destructive'
    }
  ], [onView, onEdit, onDelete])

  // Define bulk actions
  const bulkActions: DataGridBulkAction[] = useMemo(() => [
    {
      key: 'activate',
      label: 'Activate'
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      variant: 'destructive'
    },
    {
      key: 'export',
      label: 'Export Selected'
    },
    {
      key: 'delete',
      label: 'Delete Selected',
      icon: Trash2,
      variant: 'destructive'
    }
  ], [])

  // Loading skeleton
  if (loading && routes.length === 0) {
    return (
      <div className="w-full space-y-4">
        <div className="space-y-2 p-4">
          {[...Array(5)].map((_, index) => (
            <div key={index} className="flex items-center gap-4 p-4 border rounded-lg">
              <div className="flex-1 space-y-2">
                <div className="h-5 w-32 bg-muted animate-pulse rounded" />
                <div className="h-4 w-48 bg-muted animate-pulse rounded" />
              </div>
              <div className="h-4 w-24 bg-muted animate-pulse rounded" />
              <div className="h-4 w-28 bg-muted animate-pulse rounded" />
              <div className="flex gap-1">
                <div className="h-8 w-8 bg-muted animate-pulse rounded" />
                <div className="h-8 w-8 bg-muted animate-pulse rounded" />
              </div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="w-full">
      <div className="overflow-x-auto rounded-lg border">
        <DataGrid
          data={processedData.data}
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
          bulkActions={onBulkAction ? bulkActions : undefined}
          onBulkAction={handleBulkActionClick}
          emptyText="No routes found. Create your first route to get started."
          className="min-h-[400px]"
          tableClassName="min-w-[1200px]"
          compact={true}
        />
      </div>
      
      {routes.length > 0 && (
        <div className="mt-4 flex items-center justify-between text-sm text-muted-foreground">
          <div className="flex items-center gap-4">
            <span>Total: <strong>{processedData.total}</strong></span>
            <span>Active: <strong>{routes.filter(r => r.IsActive).length}</strong></span>
            <span>With Customers: <strong>{routes.filter(r => r.TotalCustomers > 0).length}</strong></span>
          </div>
          <div className="text-xs">
            Last updated: {moment().format('HH:mm:ss')}
          </div>
        </div>
      )}
    </div>
  )
}