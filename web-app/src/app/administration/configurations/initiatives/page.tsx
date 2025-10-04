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
import { Plus, Edit, Trash2, Eye, Filter, Download, Upload, MoreHorizontal, Loader2, Search, ChevronDown, X, Building2, Users, Package } from 'lucide-react'
import { cn } from '@/lib/utils'
import { organizationService } from '@/services/organizationService'
import { initiativeService, type Initiative as InitiativeDTO } from '@/services/initiativeService'
import { format } from 'date-fns'

// Types
interface Initiative {
  id: number
  initiativeName: string
  initiativeDescription: string
  allocationNo: string
  brand: string
  contractAmount: number
  availableAmount?: number
  customerType: string
  activityType: string
  startDate: string
  endDate: string
  status: 'ACTIVE' | 'DRAFT' | 'SUBMITTED' | 'CANCELLED'
  createdDate?: string
  createdBy?: string
  selectedCustomersCount: number
  selectedProductsCount: number
  daysLeft: number
  contractCode?: string
}

export default function InitiativesPage() {
  const router = useRouter()
  const { toast } = useToast()
  const searchInputRef = useRef<HTMLInputElement>(null)
  
  const [initiatives, setInitiatives] = useState<Initiative[]>([])
  const [filteredInitiatives, setFilteredInitiatives] = useState<Initiative[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterStatus, setFilterStatus] = useState<string[]>([])
  const [filterBrand, setFilterBrand] = useState<string[]>([])
  const [filterActivityType, setFilterActivityType] = useState<string[]>([])
  const [exporting, setExporting] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  
  // Dropdown data
  const [salesOrgs, setSalesOrgs] = useState<any[]>([])
  const [brands, setBrands] = useState<string[]>([])
  const [activityTypes, setActivityTypes] = useState<string[]>([])

  // Table columns configuration
  const columns = [
    {
      accessorKey: 'initiativeName',
      header: () => <div className="pl-6">Initiative Name</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.getValue('initiativeName')}</span>
          <div className="text-xs text-gray-500 max-w-xs truncate">
            {row.original.initiativeDescription}
          </div>
          <div className="text-xs text-gray-400 mt-1">
            By {row.original.createdBy} on {format(new Date(row.original.createdDate), 'MMM dd, yyyy')}
          </div>
        </div>
      )
    },
    {
      accessorKey: 'allocationNo',
      header: 'Allocation Info',
      cell: ({ row }: any) => (
        <div>
          <span className="font-medium text-sm">{row.getValue('allocationNo')}</span>
          <div className="text-xs text-gray-500">{row.original.brand}</div>
          <div className="text-xs text-gray-400">{row.original.activityType}</div>
        </div>
      )
    },
    {
      accessorKey: 'contractAmount',
      header: 'Budget',
      cell: ({ row }: any) => (
        <div className="font-medium text-sm">
          ${row.getValue('contractAmount').toLocaleString()}
        </div>
      )
    },
    {
      accessorKey: 'startDate',
      header: 'Start Date',
      cell: ({ row }: any) => {
        const startDate = row.getValue('startDate')
        return startDate ? format(new Date(startDate), 'MMM dd, yyyy') : '-'
      }
    },
    {
      accessorKey: 'endDate',
      header: 'End Date',
      cell: ({ row }: any) => {
        const endDate = row.original.endDate
        return endDate ? format(new Date(endDate), 'MMM dd, yyyy') : '-'
      }
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
                onClick={() => handleView(row.original.id)}
                className="cursor-pointer"
              >
                <Eye className="mr-2 h-4 w-4" />
                View Details
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => handleEdit(row.original.id)}
                className="cursor-pointer"
              >
                <Edit className="mr-2 h-4 w-4" />
                Edit Initiative
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => handleDelete(row.original.id)}
                className="cursor-pointer text-red-600"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ]

  useEffect(() => {
    loadData()
  }, [])

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Check for Ctrl+F (Windows/Linux) or Cmd+F (Mac)
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault() // Prevent browser's default find
        searchInputRef.current?.focus()
        searchInputRef.current?.select() // Select existing text for easy replacement
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])

  useEffect(() => {
    filterData()
    setCurrentPage(1) // Reset to first page when filters change
  }, [initiatives, searchTerm, filterStatus, filterBrand, filterActivityType])

  const loadData = async () => {
    try {
      setLoading(true)
      
      // Load sales organizations
      const orgsResponse = await organizationService.getOrganizations()
      let orgs = []
      if (Array.isArray(orgsResponse)) {
        orgs = orgsResponse
      } else if (orgsResponse?.data && Array.isArray(orgsResponse.data)) {
        orgs = orgsResponse.data
      }
      
      // Normalize organization field names
      orgs = orgs.map((org: any) => ({
        id: org.id || org.Id || org.ID || 0,
        uid: org.uid || org.UID || org.Uid || '',
        code: org.code || org.Code || '',
        name: org.name || org.Name || '',
        description: org.description || org.Description || ''
      }))
      
      setSalesOrgs(orgs)
      
      // Fetch initiatives from API
      const searchRequest = {
        pageNumber: 1,
        pageSize: 100,
        sortBy: 'createdDate',
        sortDirection: 'DESC' as const
      }
      
      const response = await initiativeService.searchInitiatives(searchRequest)

      console.log('Initiative search response:', response)
      console.log('Response type:', typeof response)
      console.log('Response keys:', Object.keys(response || {}))
      
      // Handle different response formats
      let initiativesData: any[] = []
      
      if (response?.pagedData) {
        // Expected format with pagedData
        initiativesData = response.pagedData
      } else if (Array.isArray(response)) {
        // Direct array response
        initiativesData = response
      } else if (response?.data?.PagedData) {
        // API format with Data.PagedData
        initiativesData = response.data.PagedData
      } else if (response?.Data?.PagedData) {
        // API format with Data.PagedData (PascalCase)
        initiativesData = response.Data.PagedData
      } else {
        console.warn('Unexpected response format:', response)
        initiativesData = []
      }
      
      console.log('Initiatives data before transformation:', initiativesData)

      // Transform API data to match component interface
      const transformedInitiatives: Initiative[] = initiativesData.map((init: any) => ({
        id: init.initiativeId || init.InitiativeId || init.id,
        initiativeName: init.name || init.Name || init.initiativeName,
        initiativeDescription: init.description || init.Description || init.initiativeDescription || '',
        allocationNo: init.allocationNo || init.AllocationNo || '',
        brand: init.brand || init.Brand || '',
        contractAmount: init.contractAmount || init.ContractAmount || 0,
        availableAmount: 0, // Will be fetched separately if needed
        customerType: init.customerType || init.CustomerType || '',
        activityType: init.activityType || init.ActivityType || '',
        startDate: init.startDate || init.StartDate || '',
        endDate: init.endDate || init.EndDate || '',
        status: (init.status || init.Status || 'DRAFT').toUpperCase() as any,
        createdDate: init.createdOn || init.CreatedOn || init.startDate || init.StartDate || '', // Using created date if available
        createdBy: init.createdBy || init.CreatedBy || 'System',
        selectedCustomersCount: init.customers?.length || init.Customers?.length || 0,
        selectedProductsCount: init.products?.length || init.Products?.length || 0,
        daysLeft: initiativeService.calculateDaysLeft(init.endDate || init.EndDate),
        contractCode: init.contractCode || init.ContractCode || ''
      }))
      
      console.log('Transformed initiatives:', transformedInitiatives)
      console.log('Transformed initiatives count:', transformedInitiatives.length)

      setInitiatives(transformedInitiatives)

      // Extract unique values for filter dropdowns
      const uniqueBrands = [...new Set(transformedInitiatives.map(i => i.brand))]
      const uniqueActivityTypes = [...new Set(transformedInitiatives.map(i => i.activityType))]
      
      setBrands(uniqueBrands)
      setActivityTypes(uniqueActivityTypes)
      
    } catch (error) {
      console.error('Error loading initiatives:', error)
      toast({
        title: 'Error',
        description: 'Failed to load initiatives. Please refresh the page.',
        variant: 'destructive',
      })
    } finally {
      setLoading(false)
    }
  }

  const filterData = () => {
    let filtered = [...initiatives]

    if (searchTerm) {
      filtered = filtered.filter(item => 
        item.initiativeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.allocationNo.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.brand.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.createdBy.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    // Filter by selected statuses
    if (filterStatus.length > 0) {
      filtered = filtered.filter(item => 
        filterStatus.includes(item.status)
      )
    }

    // Filter by selected brands
    if (filterBrand.length > 0) {
      filtered = filtered.filter(item => 
        filterBrand.includes(item.brand)
      )
    }

    // Filter by selected activity types
    if (filterActivityType.length > 0) {
      filtered = filtered.filter(item => 
        filterActivityType.includes(item.activityType)
      )
    }

    setFilteredInitiatives(filtered)
  }

  const handleView = (id: number) => {
    router.push(`/administration/configurations/initiatives/view/${id}`)
  }

  const handleEdit = (id: number) => {
    router.push(`/administration/configurations/initiatives/create?mode=Edit&id=${id}`)
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this initiative?')) return

    try {
      const success = await initiativeService.deleteInitiative(id)
      if (success) {
        setInitiatives(prev => prev.filter(i => i.id !== id))
        toast({
          title: 'Success',
          description: 'Initiative deleted successfully',
        })
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete initiative',
        variant: 'destructive'
      })
    }
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      console.log("Starting export of initiatives...")
      console.log(`Current filtered initiatives: ${filteredInitiatives.length}`)
      
      if (filteredInitiatives.length === 0) {
        toast({
          title: "No Data",
          description: "No initiatives found to export.",
          variant: "default",
        })
        return
      }

      // Create custom export with only essential fields
      const headers = ["Initiative Name", "Allocation No", "Brand", "Contract Amount", "Activity Type", "Start Date", "End Date", "Days Left", "Created By"]

      // Map current filtered initiatives to only include essential fields
      const exportData = filteredInitiatives.map(initiative => [
        initiative.initiativeName || "",
        initiative.allocationNo || "",
        initiative.brand || "",
        initiative.contractAmount?.toString() || "",
        initiative.activityType || "",
        initiative.startDate || "",
        initiative.endDate || "",
        initiative.daysLeft?.toString() || "0",
        initiative.createdBy || ""
      ])

      console.log(`Exporting ${exportData.length} initiatives`)

      // Create CSV content with headers
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

      // Create and download the file
      const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" })
      const url = URL.createObjectURL(blob)
      const a = document.createElement("a")
      a.href = url
      a.download = `initiatives-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      toast({
        title: "Export Complete",
        description: `${exportData.length} initiatives exported successfully to CSV file.`,
      })
    } catch (error) {
      console.error("Export error:", error)
      toast({
        title: "Export Failed",
        description: "Failed to export initiatives. Please try again.",
        variant: "destructive",
      })
    } finally {
      setExporting(false)
    }
  }

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Manage Initiatives</h1>
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
          <Button onClick={() => router.push('/administration/configurations/initiatives/create')} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Initiative
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
                placeholder="Search by name, allocation, brand... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Status Filter Dropdown */}
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
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {['ACTIVE', 'DRAFT', 'SUBMITTED', 'CANCELLED'].map((status) => (
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
                    <div className="flex items-center gap-2">
                      <Badge 
                        variant="secondary" 
                        className={cn(
                          "text-xs",
                          status === 'ACTIVE' && "bg-green-100 text-green-800",
                          status === 'DRAFT' && "bg-yellow-100 text-yellow-800",
                          status === 'SUBMITTED' && "bg-blue-100 text-blue-800",
                          status === 'CANCELLED' && "bg-red-100 text-red-800"
                        )}
                      >
                        {status}
                      </Badge>
                    </div>
                  </DropdownMenuCheckboxItem>
                ))}
                {filterStatus.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterStatus([])}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Brand Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Brand
                  {filterBrand.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterBrand.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Brand</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {brands.map((brand) => (
                  <DropdownMenuCheckboxItem
                    key={brand}
                    checked={filterBrand.includes(brand)}
                    onCheckedChange={(checked) => {
                      setFilterBrand(prev => 
                        checked 
                          ? [...prev, brand]
                          : prev.filter(b => b !== brand)
                      )
                    }}
                  >
                    <div className="flex items-center gap-2">
                      <Building2 className="h-3 w-3" />
                      {brand}
                    </div>
                  </DropdownMenuCheckboxItem>
                ))}
                {filterBrand.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterBrand([])}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Activity Type Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Activity
                  {filterActivityType.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterActivityType.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Activity Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {activityTypes.map((type) => (
                  <DropdownMenuCheckboxItem
                    key={type}
                    checked={filterActivityType.includes(type)}
                    onCheckedChange={(checked) => {
                      setFilterActivityType(prev => 
                        checked 
                          ? [...prev, type]
                          : prev.filter(t => t !== type)
                      )
                    }}
                  >
                    {type}
                  </DropdownMenuCheckboxItem>
                ))}
                {filterActivityType.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterActivityType([])}
                    >
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
            data={filteredInitiatives.slice((currentPage - 1) * pageSize, currentPage * pageSize)}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>
        
        {filteredInitiatives.length > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={filteredInitiatives.length}
              pageSize={pageSize}
              onPageChange={(page) => {
                setCurrentPage(page);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }}
              onPageSizeChange={(size) => {
                setPageSize(size);
                setCurrentPage(1); // Reset to first page when changing page size
              }}
              itemName="initiatives"
            />
          </div>
        )}
      </Card>
    </div>
  )
}