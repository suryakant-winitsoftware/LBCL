"use client"

import { useState, useEffect, useCallback, useMemo, useRef } from "react"
import { Eye, Edit, Trash2, Calendar, Users, MapPin } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import { DataGrid, DataGridColumn, DataGridAction } from "@/components/ui/data-grid"
import { Route, PagedRequest, routeService } from "@/services/routeService"
import salesTeamActivityService from '@/services/sales-team-activity.service'
import { cn } from "@/lib/utils"
import moment from "moment"
import { formatDateToDayMonthYear, formatTime } from '@/utils/date-formatter'
import { authService } from "@/lib/auth-service"

interface RouteGridProps {
  searchQuery: string
  filters: {
    search: string
    validFromFilter: string
    validToFilter: string
    status: string[]
    selectedRole: string
    selectedEmployee: string
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
      search: filters.search,
      validFromFilter: filters.validFromFilter,
      validToFilter: filters.validToFilter,
      status: filters.status,
      selectedRole: filters.selectedRole,
      selectedEmployee: filters.selectedEmployee,
      refreshTrigger
    })
    
    // Skip if params haven't changed and we've already loaded
    if (loadKey === lastLoadParams.current && initialLoadDone.current) {
      return
    }
    
    lastLoadParams.current = loadKey

    const loadRoutes = async () => {
      try {
        setLoading(true)
        
        // Use the same API as field-activities page with server-side filtering
        const routesData = await salesTeamActivityService.getRoutes({
          pageNumber: 1,
          pageSize: 1000,
          search: filters.search,
          status: filters.status,
          selectedRole: filters.selectedRole,
          selectedEmployee: filters.selectedEmployee,
          validFromFilter: filters.validFromFilter,
          validToFilter: filters.validToFilter
        });

        const routesList = routesData?.Data?.PagedData || [];
        const uniqueRoutes = deduplicateRoutes(routesList)
        
        setRoutes(uniqueRoutes)
        setPagination(prev => ({
          ...prev,
          total: uniqueRoutes.length,
          current: 1
        }))
        
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
        setPagination(prev => ({
          ...prev,
          total: 0,
          current: 1
        }))
      } finally {
        setLoading(false)
      }
    }

    loadRoutes()
    
    // Cleanup function
    return () => {
      // Clean cleanup - no more aggressive aborting
    }
  }, [filters.search, filters.selectedRole, filters.selectedEmployee, filters.status, refreshTrigger, toast])

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

  // Process data for pagination (server-side filtering already applied)
  const processedData = useMemo(() => {
    let filteredData = [...routes]
    
    // All filtering is now done server-side, so we just need pagination and sorting

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
  }, [routes, sorting, pagination.current, pagination.pageSize])

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


  // Define columns
  const columns: DataGridColumn<Route>[] = useMemo(() => [
    {
      key: 'Code',
      title: 'Route',
      width: '250px',
      sortable: true,
      className: 'pl-6',
      render: (value: string, record: Route) => (
        <div className="space-y-2 pl-6">
          <div>
            <div className="font-semibold text-sm">{value}</div>
            <div className="text-sm text-muted-foreground">{record.Name}</div>
          </div>
          <div className="flex items-center gap-2">
            <Badge variant="outline" className="text-xs">
              <Users className="h-3 w-3 mr-1" />
              {record.TotalCustomers || 0}
            </Badge>
          </div>
        </div>
      )
    },
    {
      key: 'IsActive',
      title: 'Status',
      width: '100px',
      sortable: true,
      className: 'text-center',
      render: (value: boolean, record: Route) => (
        <div className="flex justify-center">
          <Badge 
            variant={record.IsActive ? 'default' : 'secondary'}
            className={cn(
              "text-xs",
              record.IsActive ? 'bg-green-100 text-green-800 hover:bg-green-100' : 'bg-gray-100 text-gray-600 hover:bg-gray-100'
            )}
          >
            {record.IsActive ? 'Active' : 'Inactive'}
          </Badge>
        </div>
      )
    },
    {
      key: 'RoleUID',
      title: 'Role',
      width: '150px',
      sortable: true,
      render: (value: string, record: Route) => (
        <span className="text-sm text-gray-600 bg-gray-50 px-2 py-1 rounded-md">
          {record.RoleUID || '-'}
        </span>
      )
    },
    {
      key: 'EmployeeID',
      title: 'Employee ID',
      width: '150px',
      sortable: true,
      render: (value: string, record: Route) => (
        <span className="text-sm text-gray-600 bg-gray-50 px-2 py-1 rounded-md">
          {record.JobPositionUID || '-'}
        </span>
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
      title: 'Valid From',
      width: '120px',
      sortable: true,
      render: (value: string) => (
        <div className="flex items-center gap-1 text-sm">
          <Calendar className="h-3 w-3 text-muted-foreground" />
          <span>{value ? formatDateToDayMonthYear(value) : '-'}</span>
        </div>
      )
    },
    {
      key: 'ValidUpto',
      title: 'Valid Upto',
      width: '120px',
      sortable: true,
      render: (value: string) => (
        <div className="flex items-center gap-1 text-sm">
          <Calendar className="h-3 w-3 text-muted-foreground" />
          <span>{value ? formatDateToDayMonthYear(value) : '-'}</span>
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
          actions={actions}
          emptyText="No routes found. Create your first route to get started."
          className="min-h-[400px] min-w-[1200px]"
        />
    </div>
  )
}