'use client'

import { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuLabel,
  DropdownMenuCheckboxItem,
} from '@/components/ui/dropdown-menu'
import { DataTable } from '@/components/ui/data-table'
import { PaginationControls } from '@/components/ui/pagination-controls'
import { useToast } from '@/components/ui/use-toast'
import { Plus, Edit, Trash2, Eye, Filter, Download, Upload, MoreHorizontal, Loader2, Search, ChevronDown, X } from 'lucide-react'
import { useOrganizationHierarchy } from '@/hooks/useOrganizationHierarchy'
import { organizationService } from '@/services/organizationService'
import { apiService } from '@/services/api'

interface WHStockRequest {
  UID: string
  RequestCode: string
  RequestType: string
  RouteCode?: string
  RouteName?: string
  SourceCode?: string
  SourceName?: string
  TargetCode?: string
  TargetName?: string
  OrgUID?: string
  Status: string
  RequiredByDate: string
  RequestedTime: string
  ModifiedTime?: string
  Remarks?: string
}

export default function StockRequestsPage() {
  const router = useRouter()
  const [requests, setRequests] = useState<WHStockRequest[]>([])
  const [filteredRequests, setFilteredRequests] = useState<WHStockRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterStatus, setFilterStatus] = useState<string[]>([])
  const [filterType, setFilterType] = useState<string[]>([])
  const [exporting, setExporting] = useState(false)Location
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [totalCount, setTotalCount] = useState(0)
  const searchInputRef = useRef<HTMLInputElement>(null)
  const { toast } = useToast()

  // Organization hierarchy hook
  const {
    orgLevels,
    selectedOrgs,
    initializeHierarchy,
    selectOrganization,
    resetHierarchy,
    finalSelectedOrganization,
    hasSelection
  } = useOrganizationHierarchy()

  // Table columns configuration
  const columns = [
    {
      accessorKey: 'RequestCode',
      header: () => <div className="pl-6">Request Code</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.getValue('RequestCode')}</span>
        </div>
      )
    },
    {
      accessorKey: 'RequestType',
      header: 'Type',
      cell: ({ row }: any) => (
        <Badge
          variant="secondary"
          className="bg-blue-100 text-blue-800 hover:bg-blue-100 text-xs"
        >
          {row.getValue('RequestType')}
        </Badge>
      )
    },
    {
      accessorKey: 'SourceName',
      header: 'Source',
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue('SourceName') || '-'}</span>
      )
    },
    {
      accessorKey: 'TargetName',
      header: 'Target',
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue('TargetName') || '-'}</span>
      )
    },
    {
      accessorKey: 'Status',
      header: 'Status',
      cell: ({ row }: any) => {
        const status = row.getValue('Status')
        const statusColors: Record<string, string> = {
          'Pending': 'bg-yellow-100 text-yellow-800',
          'Approved': 'bg-green-100 text-green-800',
          'Rejected': 'bg-red-100 text-red-800',
          'Completed': 'bg-blue-100 text-blue-800',
        }
        return (
          <Badge
            variant="outline"
            className={`text-xs ${statusColors[status as string] || ''}`}
          >
            {status}
          </Badge>
        )
      }
    },
    {
      accessorKey: 'RequiredByDate',
      header: 'Required By',
      cell: ({ row }: any) => (
        <span className="text-sm">{new Date(row.getValue('RequiredByDate')).toLocaleDateString()}</span>
      )
    },
    {
      id: 'actions',
      header: () => <div className="text-right pr-6">Actions</div>,
      cell: ({ row }: any) => (
        <div className="text-right pr-6">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <span className="sr-only">Open menu</span>
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>Actions</DropdownMenuLabel>
              <DropdownMenuItem
                onClick={() => handleView(row.original)}
                className="cursor-pointer"
              >
                <Eye className="mr-2 h-4 w-4" />
                View Details
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => handleEdit(row.original)}
                className="cursor-pointer"
              >
                <Edit className="mr-2 h-4 w-4" />
                Edit Request
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ]

  useEffect(() => {
    fetchData()
    loadOrganizationHierarchy()
  }, [currentPage, pageSize])

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault()
        searchInputRef.current?.focus()
        searchInputRef.current?.select()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])

  // Load organization hierarchy
  const loadOrganizationHierarchy = async () => {
    try {
      const [orgTypesResult, orgsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 1000)
      ])

      if (orgsResult.data.length > 0 && orgTypesResult.length > 0) {
        initializeHierarchy(orgsResult.data, orgTypesResult)
      }
    } catch (error) {
      console.error('Failed to load organization hierarchy:', error)
      toast({
        title: 'Warning',
        description: 'Failed to load organization data',
        variant: 'default'
      })
    }
  }

  useEffect(() => {
    filterData()
  }, [requests, searchTerm, filterStatus, filterType])

  const fetchData = async () => {
    try {
      setLoading(true)

      const response: any = await apiService.post('/WHStock/SelectLoadRequestData?StockType=all', {
        PageNumber: currentPage,
        PageSize: pageSize,
        IsCountRequired: true,
        FilterCriterias: [],
        SortCriterias: []
      })

      setRequests(response.Data?.PagedData || [])
      setTotalCount(response.Data?.TotalCount || 0)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch stock requests',
        variant: 'destructive'
      })
      console.error('Error fetching data:', error)
    } finally {
      setLoading(false)
    }
  }

  const filterData = () => {
    let filtered = [...requests]

    if (searchTerm) {
      filtered = filtered.filter(item =>
        item.RequestCode?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.SourceName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.TargetName?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    if (filterStatus.length > 0) {
      filtered = filtered.filter(item =>
        filterStatus.includes(item.Status)
      )
    }

    if (filterType.length > 0) {
      filtered = filtered.filter(item =>
        filterType.includes(item.RequestType)
      )
    }

    setFilteredRequests(filtered)
  }

  const handleCreate = () => {
    router.push('/administration/warehouse-management/stock-requests/create')
  }

  const handleEdit = (item: WHStockRequest) => {
    router.push(`/administration/warehouse-management/stock-requests/edit/${item.UID}`)
  }

  const handleView = (item: WHStockRequest) => {
    router.push(`/administration/warehouse-management/stock-requests/view/${item.UID}`)
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      if (filteredRequests.length === 0) {
        toast({
          title: "No Data",
          description: "No stock requests found to export.",
          variant: "default",
        })
        return
      }

      const headers = ["Request Code", "Type", "Source", "Target", "Status", "Required By", "Requested Time"]

      const exportData = filteredRequests.map(req => [
        req.RequestCode || "",
        req.RequestType || "",
        req.SourceName || "",
        req.TargetName || "",
        req.Status || "",
        new Date(req.RequiredByDate).toLocaleDateString(),
        new Date(req.RequestedTime).toLocaleString()
      ])

      const csvContent = [
        headers.join(","),
        ...exportData.map(row =>
          row.map(field =>
            typeof field === 'string' && field.includes(',')
              ? `"${field}"`
              : field
          ).join(",")
        )
      ].join("\n")

      const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" })
      const url = URL.createObjectURL(blob)
      const a = document.createElement("a")
      a.href = url
      a.download = `stock-requests-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      toast({
        title: "Export Complete",
        description: `${exportData.length} stock requests exported successfully.`,
      })
    } catch (error) {
      console.error("Export error:", error)
      toast({
        title: "Export Failed",
        description: "Failed to export stock requests.",
        variant: "destructive",
      })
    } finally {
      setExporting(false)
    }
  }

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Warehouse Stock Requests</h1>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleExport}
            disabled={exporting}
          >
            {exporting ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Exporting...
              </>
            ) : (
              <>
                <Download className="h-4 w-4 mr-2" />
                Export
              </>
            )}
          </Button>
          <Button onClick={handleCreate} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            New Request
          </Button>
        </div>
      </div>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by code, source, or target... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Status Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {filterStatus.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterStatus.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {['Pending', 'Approved', 'Rejected', 'Completed'].map((status) => (
                  <DropdownMenuCheckboxItem
                    key={status}
                    checked={filterStatus.includes(status)}
                    onCheckedChange={(checked) => {
                      setFilterStatus(prev =>
                        checked
                          ? [...prev, status]
                          : prev.filter(s => s !== status)
                      )
                    }}
                  >
                    {status}
                  </DropdownMenuCheckboxItem>
                ))}
                {filterStatus.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => setFilterStatus([])}>
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>

            {/* Type Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Type
                  {filterType.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterType.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {['LOAD', 'UNLOAD', 'TRANSFER'].map((type) => (
                  <DropdownMenuCheckboxItem
                    key={type}
                    checked={filterType.includes(type)}
                    onCheckedChange={(checked) => {
                      setFilterType(prev =>
                        checked
                          ? [...prev, type]
                          : prev.filter(t => t !== type)
                      )
                    }}
                  >
                    {type}
                  </DropdownMenuCheckboxItem>
                ))}
                {filterType.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => setFilterType([])}>
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          <DataTable
            columns={columns}
            data={filteredRequests}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>

        {totalCount > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={totalCount}
              pageSize={pageSize}
              onPageChange={(page) => {
                setCurrentPage(page);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }}
              onPageSizeChange={(size) => {
                setPageSize(size);
                setCurrentPage(1);
              }}
              itemName="requests"
            />
          </div>
        )}
      </Card>

    </div>
  )
}
