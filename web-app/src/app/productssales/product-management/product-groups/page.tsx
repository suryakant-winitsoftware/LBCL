'use client'

import { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
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
import { skuGroupService, SKUGroup, SKUGroupType } from '@/services/sku/sku-group.service'
import { useOrganizationHierarchy } from '@/hooks/useOrganizationHierarchy'
import { organizationService } from '@/services/organizationService'

interface FormData {
  UID: string
  Code: string
  Name: string
  SkuGroupTypeUID: string
  ParentUID?: string
  ItemLevel: number
  SupplierOrgUID: string
}

export default function ProductGroupsPage() {
  const router = useRouter()
  const [groups, setGroups] = useState<SKUGroup[]>([])
  const [groupTypes, setGroupTypes] = useState<SKUGroupType[]>([])
  const [filteredGroups, setFilteredGroups] = useState<SKUGroup[]>([])
  const [loading, setLoading] = useState(true)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<SKUGroup | null>(null)
  const [formData, setFormData] = useState<FormData>({
    UID: '',
    Code: '',
    Name: '',
    SkuGroupTypeUID: '',
    ItemLevel: 1,
    SupplierOrgUID: 'Supplier'
  })
  const [searchTerm, setSearchTerm] = useState('')
  const [filterGroupType, setFilterGroupType] = useState<string[]>([]) // Changed to array for multiple selection
  const [filterLevel, setFilterLevel] = useState<string[]>([]) // Changed to array for multiple selection
  const [exporting, setExporting] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
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
      accessorKey: 'Code',
      header: () => <div className="pl-6">Code</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.getValue('Code')}</span>
        </div>
      )
    },
    {
      accessorKey: 'Name',
      header: 'Name',
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue('Name')}</span>
      )
    },
    {
      accessorKey: 'SkuGroupTypeUID',
      header: 'Group Type',
      cell: ({ row }: any) => {
        const skuGroupTypeUID = row.original.SKUGroupTypeUID || row.original.SkuGroupTypeUID || row.getValue('SkuGroupTypeUID')
        const groupType = groupTypes.find(gt => gt.UID === skuGroupTypeUID)
        return (
          <Badge 
            variant="secondary" 
            className="bg-blue-100 text-blue-800 hover:bg-blue-100 text-xs"
          >
            {groupType?.Name || skuGroupTypeUID}
          </Badge>
        )
      }
    },
    {
      accessorKey: 'ItemLevel',
      header: 'Level',
      cell: ({ row }: any) => (
        <Badge 
          variant="outline" 
          className="text-xs"
        >
          Level {row.getValue('ItemLevel')}
        </Badge>
      )
    },
    {
      accessorKey: 'ParentUID',
      header: 'Parent',
      cell: ({ row }: any) => {
        const parent = row.getValue('ParentUID')
        if (!parent) return <span className="text-xs text-gray-500">Root</span>
        const parentGroup = groups.find(g => g.UID === parent)
        return <span className="text-sm text-gray-600">{parentGroup?.Name || parent}</span>
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
                Edit Group
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
    setCurrentPage(1) // Reset to first page when filters change
  }, [groups, searchTerm, filterGroupType, filterLevel])

  const fetchData = async () => {
    try {
      setLoading(true)
      
      // First get the total count to determine how many batches we need
      const countResponse = await skuGroupService.getAllSKUGroups({
        PageNumber: 1,
        PageSize: 1,
        IsCountRequired: true,
        FilterCriterias: [],
        SortCriterias: []
      })
      const totalCount = countResponse?.Data?.TotalCount || 0
      console.log(`Total SKU Groups in database: ${totalCount}`)
      
      if (totalCount === 0) {
        setGroups([])
        setGroupTypes([])
        return
      }
      
      // Fetch all groups in batches
      const batchSize = 5000
      const totalPages = Math.ceil(totalCount / batchSize)
      let allGroups: SKUGroup[] = []
      
      for (let page = 1; page <= totalPages; page++) {
        console.log(`Fetching groups batch ${page}/${totalPages}...`)
        const batchResponse = await skuGroupService.getAllSKUGroups({
          PageNumber: page,
          PageSize: batchSize,
          IsCountRequired: false,
          FilterCriterias: [],
          SortCriterias: []
        })
        if (batchResponse?.Data?.PagedData) {
          allGroups = [...allGroups, ...batchResponse.Data.PagedData]
        }
      }
      
      console.log(`Successfully loaded ${allGroups.length} groups`)
      
      // Fetch all group types (usually fewer, but let's handle large numbers too)
      const groupTypesCountResponse = await skuGroupService.getAllSKUGroupTypes({
        PageNumber: 1,
        PageSize: 1,
        IsCountRequired: true,
        FilterCriterias: [],
        SortCriterias: []
      })
      const groupTypesCount = groupTypesCountResponse?.Data?.TotalCount || 0
      
      let allGroupTypes: any[] = []
      if (groupTypesCount > 0) {
        const groupTypePages = Math.ceil(groupTypesCount / batchSize)
        for (let page = 1; page <= groupTypePages; page++) {
          const batchResponse = await skuGroupService.getAllSKUGroupTypes({
            PageNumber: page,
            PageSize: batchSize,
            IsCountRequired: false,
            FilterCriterias: [],
            SortCriterias: []
          })
          if (batchResponse?.Data?.PagedData) {
            allGroupTypes = [...allGroupTypes, ...batchResponse.Data.PagedData]
          }
        }
      }
      
      console.log(`Successfully loaded ${allGroupTypes.length} group types`)
      
      setGroups(allGroups)
      setGroupTypes(allGroupTypes)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch data',
        variant: 'destructive'
      })
      console.error('Error fetching data:', error)
    } finally {
      setLoading(false)
    }
  }

  const filterData = () => {
    let filtered = [...groups]

    if (searchTerm) {
      filtered = filtered.filter(item => 
        item.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.Code.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    // Filter by selected group types
    if (filterGroupType.length > 0) {
      filtered = filtered.filter(item => 
        filterGroupType.includes(item.SKUGroupTypeUID || item.SkuGroupTypeUID || '')
      )
    }

    // Filter by selected levels
    if (filterLevel.length > 0) {
      filtered = filtered.filter(item => 
        filterLevel.includes(item.ItemLevel.toString())
      )
    }

    setFilteredGroups(filtered)
  }

  const handleCreate = () => {
    setEditingItem(null)
    setFormData({
      UID: '',
      Code: '',
      Name: '',
      SkuGroupTypeUID: '',
      ItemLevel: 1,
      SupplierOrgUID: finalSelectedOrganization || 'Supplier'
    })
    setDialogOpen(true)
  }

  const handleEdit = (item: SKUGroup) => {
    setEditingItem(item)
    setFormData({
      UID: item.UID,
      Code: item.Code,
      Name: item.Name,
      SkuGroupTypeUID: item.SkuGroupTypeUID,
      ParentUID: item.ParentUID,
      ItemLevel: item.ItemLevel,
      SupplierOrgUID: item.SupplierOrgUID
    })
    setDialogOpen(true)
  }

  const handleView = (item: SKUGroup) => {
    router.push(`/productssales/product-management/product-groups/view/${item.UID}`)
  }

  const handleDelete = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this product group?')) return

    try {
      await skuGroupService.deleteSKUGroup(uid)
      toast({
        title: 'Success',
        description: 'Product group deleted successfully',
      })
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete product group',
        variant: 'destructive'
      })
    }
  }

  const handleSave = async () => {
    try {
      if (editingItem) {
        // Update existing
        const updateData = {
          ...editingItem,
          ...formData,
          ModifiedBy: 'ADMIN',
          ModifiedTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString()
        }
        await skuGroupService.updateSKUGroup(updateData)
        toast({
          title: 'Success',
          description: 'Product group updated successfully',
        })
      } else {
        // Create new
        const createData = {
          ...formData,
          UID: formData.Code, // Use code as UID for simplicity
          CreatedBy: 'ADMIN',
          CreatedTime: new Date().toISOString(),
          ModifiedBy: 'ADMIN',
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString()
        }
        await skuGroupService.createSKUGroup(createData)
        toast({
          title: 'Success',
          description: 'Product group created successfully',
        })
      }
      setDialogOpen(false)
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save product group',
        variant: 'destructive'
      })
    }
  }

  const getAvailableParents = () => {
    console.log('🔍 getAvailableParents called')
    console.log('📋 Current formData:', formData)
    console.log('📋 All groups:', groups)
    console.log('📋 All groupTypes:', groupTypes)
    
    if (!formData.SkuGroupTypeUID) {
      console.log('❌ No SkuGroupTypeUID selected')
      return []
    }
    
    const currentGroupType = groupTypes.find(gt => gt.UID === formData.SkuGroupTypeUID)
    console.log('🎯 Current group type:', currentGroupType)
    
    if (!currentGroupType) {
      console.log('❌ Current group type not found')
      return []
    }
    
    // For Level 1, can have other Level 1 groups as parents
    // For higher levels, can have groups from lower levels as parents
    if (currentGroupType.ItemLevel === 1) {
      console.log('📊 Level 1 selected - looking for other Level 1 groups')
      const availableParents = groups.filter(g => {
        console.log(`🔎 Checking group: ${g.Name} (${g.UID}) - Type: ${g.SKUGroupTypeUID || g.SkuGroupTypeUID}, Level: ${g.ItemLevel}`)
        const groupTypeUID = g.SKUGroupTypeUID || g.SkuGroupTypeUID
        const isValidParent = groupTypeUID === formData.SkuGroupTypeUID && g.UID !== formData.UID
        console.log(`✅ Group type match: ${groupTypeUID} === ${formData.SkuGroupTypeUID} = ${groupTypeUID === formData.SkuGroupTypeUID}`)
        console.log(`✅ Is valid parent: ${isValidParent}`)
        return isValidParent
      })
      console.log('📝 Available Level 1 parents:', availableParents)
      return availableParents
    } else {
      console.log(`📊 Level ${currentGroupType.ItemLevel} selected - looking for lower/equal level groups`)
      const parentGroupTypes = groupTypes.filter(gt => gt.ItemLevel <= currentGroupType.ItemLevel)
      console.log('🎯 Parent group types:', parentGroupTypes)
      
      const availableParents = groups.filter(g => {
        console.log(`🔎 Checking group: ${g.Name} (${g.UID}) - Type: ${g.SKUGroupTypeUID || g.SkuGroupTypeUID}, Level: ${g.ItemLevel}`)
        const groupTypeUID = g.SKUGroupTypeUID || g.SkuGroupTypeUID
        const matchesParentType = parentGroupTypes.some(pgt => pgt.UID === groupTypeUID)
        const notSelf = g.UID !== formData.UID
        const isValidParent = matchesParentType && notSelf
        console.log(`✅ Group type: ${groupTypeUID}, Parent types: ${parentGroupTypes.map(p => p.UID).join(', ')}`)
        console.log(`✅ Matches parent type: ${matchesParentType}, Not self: ${notSelf}, Is valid: ${isValidParent}`)
        return isValidParent
      })
      console.log('📝 Available higher level parents:', availableParents)
      return availableParents
    }
  }

  const handleGroupTypeChange = (groupTypeUID: string) => {
    const groupType = groupTypes.find(gt => gt.UID === groupTypeUID)
    if (groupType) {
      setFormData({
        ...formData,
        SkuGroupTypeUID: groupTypeUID,
        ItemLevel: groupType.ItemLevel,
        ParentUID: undefined
      })
    }
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      console.log("Starting export of product groups...")
      console.log(`Current filtered groups: ${filteredGroups.length}`)
      
      if (filteredGroups.length === 0) {
        toast({
          title: "No Data",
          description: "No product groups found to export.",
          variant: "default",
        })
        return
      }

      // Create custom export with only essential fields
      const headers = ["Code", "Name", "Group Type", "Level", "Parent"]
      
      // Map current filtered groups to only include essential fields
      const exportData = filteredGroups.map(group => {
        const groupType = groupTypes.find(gt => gt.UID === (group.SKUGroupTypeUID || group.SkuGroupTypeUID))
        
        // Use same logic as table display for parent
        let parentDisplay = "Root"
        if (group.ParentUID) {
          const parentGroup = groups.find(g => g.UID === group.ParentUID)
          parentDisplay = parentGroup?.Name || group.ParentUID
        }
        
        return [
          group.Code || "",
          group.Name || "",
          groupType?.Name || "",
          group.ItemLevel?.toString() || "",
          parentDisplay
        ]
      })

      console.log(`Exporting ${exportData.length} product groups`)

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
      a.download = `product-groups-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      toast({
        title: "Export Complete",
        description: `${exportData.length} product groups exported successfully to CSV file.`,
      })
    } catch (error) {
      console.error("Export error:", error)
      toast({
        title: "Export Failed",
        description: "Failed to export product groups. Please try again.",
        variant: "destructive",
      })
    } finally {
      setExporting(false)
    }
  }


  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Product Groups</h1>
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
            Add Group
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
                placeholder="Search by name or code... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Group Type Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Group Type
                  {filterGroupType.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterGroupType.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>Filter by Group Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {groupTypes.map((gt) => (
                  <DropdownMenuCheckboxItem
                    key={gt.UID}
                    checked={filterGroupType.includes(gt.UID)}
                    onCheckedChange={(checked) => {
                      setFilterGroupType(prev => 
                        checked 
                          ? [...prev, gt.UID]
                          : prev.filter(id => id !== gt.UID)
                      )
                    }}
                  >
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-xs">
                        {gt.Name}
                      </Badge>
                    </div>
                  </DropdownMenuCheckboxItem>
                ))}
                {filterGroupType.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterGroupType([])}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Level Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Level
                  {filterLevel.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterLevel.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Level</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={filterLevel.includes("1")}
                  onCheckedChange={(checked) => {
                    setFilterLevel(prev => 
                      checked 
                        ? [...prev, "1"]
                        : prev.filter(l => l !== "1")
                    )
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-blue-500 rounded-full" />
                    Level 1
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={filterLevel.includes("2")}
                  onCheckedChange={(checked) => {
                    setFilterLevel(prev => 
                      checked 
                        ? [...prev, "2"]
                        : prev.filter(l => l !== "2")
                    )
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-green-500 rounded-full" />
                    Level 2
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={filterLevel.includes("3")}
                  onCheckedChange={(checked) => {
                    setFilterLevel(prev => 
                      checked 
                        ? [...prev, "3"]
                        : prev.filter(l => l !== "3")
                    )
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-yellow-500 rounded-full" />
                    Level 3
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={filterLevel.includes("4")}
                  onCheckedChange={(checked) => {
                    setFilterLevel(prev => 
                      checked 
                        ? [...prev, "4"]
                        : prev.filter(l => l !== "4")
                    )
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-purple-500 rounded-full" />
                    Level 4
                  </div>
                </DropdownMenuCheckboxItem>
                {filterLevel.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterLevel([])}
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
            data={filteredGroups.slice((currentPage - 1) * pageSize, currentPage * pageSize)}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>
        
        {filteredGroups.length > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={filteredGroups.length}
              pageSize={pageSize}
              onPageChange={(page) => {
                setCurrentPage(page);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }}
              onPageSizeChange={(size) => {
                setPageSize(size);
                setCurrentPage(1); // Reset to first page when changing page size
              }}
              itemName="groups"
            />
          </div>
        )}
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>
              {editingItem ? 'Edit Product Group' : 'Create Product Group'}
            </DialogTitle>
            <DialogDescription>
              {editingItem ? 'Update the group details' : 'Add a new product group'}
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="code">Code *</Label>
              <Input
                id="code"
                value={formData.Code}
                onChange={(e) => setFormData({ ...formData, Code: e.target.value })}
                placeholder="Enter group code"
              />
            </div>
            
            <div className="grid gap-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.Name}
                onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                placeholder="Enter group name"
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="groupType">Group Type *</Label>
              <Select 
                value={formData.SkuGroupTypeUID} 
                onValueChange={handleGroupTypeChange}
              >
                <SelectTrigger id="groupType">
                  <SelectValue placeholder="Select group type" />
                </SelectTrigger>
                <SelectContent>
                  {groupTypes.map((gt) => (
                    <SelectItem key={gt.UID} value={gt.UID}>
                      {gt.Name} (Level {gt.ItemLevel})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="level">Item Level</Label>
              <Input
                id="level"
                value={formData.ItemLevel.toString()}
                disabled
                className="bg-muted"
              />
              <p className="text-xs text-muted-foreground">
                Automatically set based on group type
              </p>
            </div>

            {formData.SkuGroupTypeUID && (
              <div className="grid gap-2">
                <Label htmlFor="parent">Parent Group</Label>
                <Select 
                  value={formData.ParentUID || 'no-parent'} 
                  onValueChange={(value) => setFormData({ ...formData, ParentUID: value === 'no-parent' ? undefined : value })}
                >
                  <SelectTrigger id="parent">
                    <SelectValue placeholder="Select parent group" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="no-parent">No Parent (Root Level)</SelectItem>
                    {getAvailableParents().map((parent) => {
                      const parentGroupType = groupTypes.find(gt => gt.UID === parent.SkuGroupTypeUID)
                      return (
                        <SelectItem key={parent.UID} value={parent.UID}>
                          {parent.Name} ({parent.Code}) - {parentGroupType?.Name} Level {parent.ItemLevel}
                        </SelectItem>
                      )
                    })}
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  {getAvailableParents().length === 0 
                    ? `No parent groups available for Level ${formData.ItemLevel}. ${groups.length === 0 ? 'No groups exist yet.' : `Total groups: ${groups.length}`}` 
                    : `${getAvailableParents().length} parent group(s) available from ${groups.length} total groups`}
                </p>
                <div className="text-xs text-gray-400 mt-1">
                  Debug: Selected Type: {formData.SkuGroupTypeUID}, Level: {formData.ItemLevel}
                </div>
              </div>
            )}

            <div className="grid gap-2">
              <Label htmlFor="supplier">Organization</Label>
              <Select 
                value={formData.SupplierOrgUID} 
                onValueChange={(value) => setFormData({ ...formData, SupplierOrgUID: value })}
              >
                <SelectTrigger id="supplier">
                  <SelectValue placeholder="Select organization" />
                </SelectTrigger>
                <SelectContent>
                  {/* Show the final selected organization from hierarchy */}
                  {finalSelectedOrganization ? (
                    <SelectItem value={finalSelectedOrganization}>
                      {orgLevels.flatMap(level => level.organizations)
                        .find(org => org.UID === finalSelectedOrganization)?.Name || finalSelectedOrganization}
                    </SelectItem>
                  ) : (
                    <>
                      <SelectItem value="Supplier">Default Supplier</SelectItem>
                      {/* Show all organizations from hierarchy */}
                      {orgLevels.flatMap(level => level.organizations).map(org => (
                        <SelectItem key={org.UID} value={org.UID}>
                          {org.Code ? `${org.Name} (${org.Code})` : org.Name}
                        </SelectItem>
                      ))}
                    </>
                  )}
                </SelectContent>
              </Select>
              {finalSelectedOrganization && (
                <p className="text-xs text-muted-foreground">
                  Using selected organization from hierarchy
                </p>
              )}
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSave}>
              {editingItem ? 'Update' : 'Create'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}