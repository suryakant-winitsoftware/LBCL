'use client'

import { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Checkbox } from '@/components/ui/checkbox'
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
import { Plus, Edit, Trash2, Eye, Filter, Download, Upload, Building, MoreHorizontal, Search, ChevronDown, X } from 'lucide-react'
import { skuGroupService, SKUGroupType } from '@/services/sku/sku-group.service'
import { useOrganizationHierarchy } from '@/hooks/useOrganizationHierarchy'
import { organizationService } from '@/services/organizationService'

interface FormData {
  UID: string
  Code: string
  Name: string
  OrgUID: string
  ParentUID?: string
  ItemLevel: number
  AvailableForFilter: boolean
}

export default function SKUGroupTypesPage() {
  const router = useRouter()
  const [groupTypes, setGroupTypes] = useState<SKUGroupType[]>([])
  const [filteredGroupTypes, setFilteredGroupTypes] = useState<SKUGroupType[]>([])
  const [loading, setLoading] = useState(true)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<SKUGroupType | null>(null)
  const [formData, setFormData] = useState<FormData>({
    UID: '',
    Code: '',
    Name: '',
    OrgUID: 'Farmley',
    ItemLevel: 1,
    AvailableForFilter: false
  })
  const [searchTerm, setSearchTerm] = useState('')
  const [filterLevel, setFilterLevel] = useState<string[]>([]) // Changed to array for multi-select
  const [maxItemLevel, setMaxItemLevel] = useState(4)
  const [isAddingNewLevel, setIsAddingNewLevel] = useState(false)
  const [newLevelName, setNewLevelName] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const searchInputRef = useRef<HTMLInputElement>(null)
  const { toast } = useToast()

  // Organization hierarchy integration
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
      accessorKey: 'ItemLevel',
      header: 'Level',
      cell: ({ row }: any) => (
        <Badge variant="outline" className="text-xs">
          Level {row.getValue('ItemLevel')}
        </Badge>
      )
    },
    {
      accessorKey: 'ParentUID',
      header: 'Parent',
      cell: ({ row }: any) => {
        const parent = row.getValue('ParentUID')
        return parent ? (
          <span className="text-sm text-gray-600">{parent}</span>
        ) : (
          <span className="text-xs text-gray-500">Root</span>
        )
      }
    },
    {
      accessorKey: 'AvailableForFilter',
      header: 'Filterable',
      cell: ({ row }: any) => (
        <Badge 
          variant={row.getValue('AvailableForFilter') ? 'default' : 'secondary'}
          className={row.getValue('AvailableForFilter') 
            ? 'bg-green-100 text-green-800 hover:bg-green-100 text-xs' 
            : 'bg-gray-100 text-gray-600 hover:bg-gray-100 text-xs'}
        >
          {row.getValue('AvailableForFilter') ? 'Yes' : 'No'}
        </Badge>
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
                Edit Type
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ]

  // Load organization hierarchy
  useEffect(() => {
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

    loadOrganizationHierarchy()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    fetchGroupTypes()
  }, [])

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

  useEffect(() => {
    filterData()
    setCurrentPage(1) // Reset to first page when filters change
  }, [groupTypes, searchTerm, filterLevel])

  const fetchGroupTypes = async () => {
    try {
      setLoading(true)
      const response = await skuGroupService.getAllSKUGroupTypes({
        PageNumber: 1,
        PageSize: 100,
        IsCountRequired: true,
        FilterCriterias: [],
        SortCriterias: []
      })
      
      let fetchedGroupTypes: SKUGroupType[] = []
      if (response.IsSuccess && response.Data && response.Data.PagedData) {
        fetchedGroupTypes = response.Data.PagedData
        setGroupTypes(fetchedGroupTypes)
      }
      
      // Calculate maximum item level from existing data
      if (fetchedGroupTypes.length > 0) {
        const currentMaxLevel = Math.max(...fetchedGroupTypes.map(item => item.ItemLevel), 0)
        setMaxItemLevel(Math.max(currentMaxLevel, 4)) // Minimum 4 levels
      } else {
        setMaxItemLevel(4) // Default to 4 levels if no data
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch SKU group types',
        variant: 'destructive'
      })
      console.error('Error fetching group types:', error)
    } finally {
      setLoading(false)
    }
  }

  const filterData = () => {
    let filtered = [...groupTypes]

    if (searchTerm) {
      filtered = filtered.filter(item => 
        item.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.Code.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    // Filter by selected levels
    if (filterLevel.length > 0) {
      filtered = filtered.filter(item => 
        filterLevel.includes(item.ItemLevel.toString())
      )
    }

    setFilteredGroupTypes(filtered)
  }

  const handleOrganizationSelection = (levelIndex: number, value: string) => {
    selectOrganization(levelIndex, value)
  }

  const handleCreate = () => {
    setEditingItem(null)
    setFormData({
      UID: '',
      Code: '',
      Name: '',
      OrgUID: finalSelectedOrganization || 'Farmley',
      ItemLevel: 1,
      AvailableForFilter: false
    })
    setDialogOpen(true)
  }

  const handleEdit = (item: SKUGroupType) => {
    setEditingItem(item)
    setFormData({
      UID: item.UID,
      Code: item.Code,
      Name: item.Name,
      OrgUID: item.OrgUID,
      ParentUID: item.ParentUID,
      ItemLevel: item.ItemLevel,
      AvailableForFilter: item.AvailableForFilter
    })
    setDialogOpen(true)
  }

  const handleView = (item: SKUGroupType) => {
    router.push(`/productssales/product-management/group-types/view/${item.UID}`)
  }

  const handleDelete = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this SKU group type?')) return

    try {
      await skuGroupService.deleteSKUGroupType(uid)
      toast({
        title: 'Success',
        description: 'SKU group type deleted successfully',
      })
      fetchGroupTypes()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete SKU group type',
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
        await skuGroupService.updateSKUGroupType(updateData)
        toast({
          title: 'Success',
          description: 'SKU group type updated successfully',
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
        await skuGroupService.createSKUGroupType(createData)
        toast({
          title: 'Success',
          description: 'SKU group type created successfully',
        })
      }
      setDialogOpen(false)
      fetchGroupTypes()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save SKU group type',
        variant: 'destructive'
      })
    }
  }

  const getAvailableParents = () => {
    return groupTypes.filter(gt => gt.ItemLevel < formData.ItemLevel)
  }

  // Get dynamic level names from existing data only
  const getLevelNames = (): Record<number, string[]> => {
    const levelNames: Record<number, string[]> = {}
    
    groupTypes.forEach(gt => {
      if (!levelNames[gt.ItemLevel]) {
        levelNames[gt.ItemLevel] = []
      }
      if (!levelNames[gt.ItemLevel].includes(gt.Name)) {
        levelNames[gt.ItemLevel].push(gt.Name)
      }
    })

    return levelNames
  }

  // Get all available levels from existing data
  const getAvailableLevels = () => {
    const existingLevels = [...new Set(groupTypes.map(gt => gt.ItemLevel))].sort((a, b) => a - b)
    const levels = []
    
    // Add existing levels
    existingLevels.forEach(level => {
      const levelNames = getLevelNames()[level] || []
      const nameDisplay = levelNames.length > 0 ? ` (${levelNames.join(', ')})` : ''
      levels.push({
        value: level,
        label: `Level ${level}${nameDisplay}`,
        isExisting: true
      })
    })
    
    // Add option for next new level
    const nextLevel = existingLevels.length > 0 ? Math.max(...existingLevels) + 1 : 1
    levels.push({
      value: nextLevel,
      label: `Level ${nextLevel} (New)`,
      isExisting: false
    })
    
    return levels
  }

  // Update max level when new level is created
  const updateMaxLevel = (newLevel: number) => {
    if (newLevel > maxItemLevel) {
      setMaxItemLevel(newLevel)
    }
  }

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Group Types</h1>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="sm">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={handleCreate} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Group Type
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
                {getAvailableLevels()
                  .filter(level => level.isExisting)
                  .map((level) => (
                    <DropdownMenuCheckboxItem
                      key={level.value}
                      checked={filterLevel.includes(level.value.toString())}
                      onCheckedChange={(checked) => {
                        setFilterLevel(prev => 
                          checked 
                            ? [...prev, level.value.toString()]
                            : prev.filter(l => l !== level.value.toString())
                        )
                      }}
                    >
                      <div className="flex items-center gap-2">
                        <Badge variant="outline" className="text-xs">
                          {level.label}
                        </Badge>
                      </div>
                    </DropdownMenuCheckboxItem>
                  ))}
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
            data={filteredGroupTypes.slice((currentPage - 1) * pageSize, currentPage * pageSize)}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>

        {filteredGroupTypes.length > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={filteredGroupTypes.length}
              pageSize={pageSize}
              onPageChange={(page) => {
                setCurrentPage(page);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }}
              onPageSizeChange={(size) => {
                setPageSize(size);
                setCurrentPage(1);
              }}
              itemName="group types"
            />
          </div>
        )}
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>
              {editingItem ? 'Edit Group Type' : 'Create Group Type'}
            </DialogTitle>
            <DialogDescription>
              {editingItem ? 'Update the group type details' : 'Add a new group type to the hierarchy'}
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="code">Code *</Label>
              <Input
                id="code"
                value={formData.Code}
                onChange={(e) => setFormData({ ...formData, Code: e.target.value })}
                placeholder="Enter group type code"
              />
            </div>
            
            <div className="grid gap-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.Name}
                onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                placeholder="Enter group type name"
              />
            </div>

            {/* Organization Hierarchy - Same as Create Product */}
            <div className="grid gap-4">
              <div className="flex items-center gap-2">
                <Building className="h-4 w-4 text-blue-600" />
                <Label className="font-medium">Organization Hierarchy</Label>
              </div>
              {orgLevels.map((level, index) => (
                <div key={`${level.orgTypeUID}-${level.level}`} className="grid gap-2">
                  <Label htmlFor={`org-level-${index}`} className="text-sm">
                    {level.dynamicLabel || level.orgTypeName}
                  </Label>
                  <Select
                    value={level.selectedOrgUID || ''}
                    onValueChange={(value) => {
                      handleOrganizationSelection(index, value)
                      setFormData({ ...formData, OrgUID: finalSelectedOrganization || value })
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={`Select ${level.orgTypeName.toLowerCase()}`} />
                    </SelectTrigger>
                    <SelectContent>
                      {level.organizations.map((org) => (
                        <SelectItem key={org.UID} value={org.UID}>
                          {org.Code ? `${org.Name} (${org.Code})` : org.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              ))}
              
              {hasSelection && (
                <div className="p-2 bg-green-50 rounded-md border">
                  <p className="text-xs text-green-800">
                    ✓ Selected: {finalSelectedOrganization}
                  </p>
                </div>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="level">Item Level *</Label>
              <Select 
                value={formData.ItemLevel.toString()} 
                onValueChange={(value) => {
                  const levelValue = parseInt(value)
                  setFormData({ ...formData, ItemLevel: levelValue })
                  updateMaxLevel(levelValue)
                }}
              >
                <SelectTrigger id="level">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {getAvailableLevels().map((level) => (
                    <SelectItem key={level.value} value={level.value.toString()}>
                      {level.label}
                      {level.value === 1 && groupTypes.length === 0 && ' (Root)'}
                      {level.value === 1 && groupTypes.length > 0 && groupTypes.filter(gt => gt.ItemLevel === 1).length === 0 && ' (Root)'}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              
              {/* Show info about selected level */}
              {formData.ItemLevel > 0 && (
                <div className="text-xs text-muted-foreground mt-1">
                  {(() => {
                    const existingAtLevel = groupTypes.filter(gt => gt.ItemLevel === formData.ItemLevel)
                    if (existingAtLevel.length > 0) {
                      const names = [...new Set(existingAtLevel.map(gt => gt.Name))]
                      return `Existing at this level: ${names.join(', ')}`
                    }
                    return formData.ItemLevel === 1 ? 'This will be a root level item' : 'This will be a new level'
                  })()}
                </div>
              )}
            </div>

            {formData.ItemLevel > 1 && (
              <div className="grid gap-2">
                <Label htmlFor="parent">Parent Group Type</Label>
                <Select 
                  value={formData.ParentUID || 'no-parent'} 
                  onValueChange={(value) => setFormData({ ...formData, ParentUID: value === 'no-parent' ? undefined : value })}
                >
                  <SelectTrigger id="parent">
                    <SelectValue placeholder="Select parent" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="no-parent">No Parent (Optional)</SelectItem>
                    {getAvailableParents()
                      .sort((a, b) => a.ItemLevel - b.ItemLevel || a.Name.localeCompare(b.Name))
                      .map((parent) => {
                        const levelNames = getLevelNames()[parent.ItemLevel] || []
                        const levelContext = levelNames.length > 1 ? ` (${levelNames.join(', ')})` : ''
                        return (
                          <SelectItem key={parent.UID} value={parent.UID}>
                            {parent.Name} - Level {parent.ItemLevel}{levelContext}
                          </SelectItem>
                        )
                      })}
                  </SelectContent>
                </Select>
                
                {/* Show info about parent selection */}
                <div className="text-xs text-muted-foreground">
                  {getAvailableParents().length === 0 
                    ? `No parent options available. Level ${formData.ItemLevel} items can be root items.`
                    : `Select a parent from levels 1-${formData.ItemLevel - 1} or leave as root item.`
                  }
                </div>
              </div>
            )}

            <div className="flex items-center space-x-2">
              <Checkbox
                id="filterable"
                checked={formData.AvailableForFilter}
                onCheckedChange={(checked) => 
                  setFormData({ ...formData, AvailableForFilter: checked as boolean })
                }
              />
              <Label htmlFor="filterable">Available for filtering</Label>
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