'use client'

import { useState, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Checkbox } from '@/components/ui/checkbox'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { DataTable } from '@/components/ui/data-table'
import { useToast } from '@/components/ui/use-toast'
import { Plus, Edit, Trash2, Eye, Filter, Download, Upload, Building } from 'lucide-react'
import { hierarchyService, SKUGroupType } from '@/services/sku/hierarchy.service'
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
  const [filterLevel, setFilterLevel] = useState<string>('all')
  const [maxItemLevel, setMaxItemLevel] = useState(4)
  const [isAddingNewLevel, setIsAddingNewLevel] = useState(false)
  const [newLevelName, setNewLevelName] = useState('')
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
      header: 'Code',
      cell: ({ row }: any) => <span className="font-mono">{row.getValue('Code')}</span>
    },
    {
      accessorKey: 'Name',
      header: 'Name',
    },
    {
      accessorKey: 'ItemLevel',
      header: 'Level',
      cell: ({ row }: any) => <Badge variant="outline">{row.getValue('ItemLevel')}</Badge>
    },
    {
      accessorKey: 'ParentUID',
      header: 'Parent',
      cell: ({ row }: any) => {
        const parent = row.getValue('ParentUID')
        return parent ? <span className="text-sm text-muted-foreground">{parent}</span> : <span className="text-xs text-muted-foreground">Root</span>
      }
    },
    {
      accessorKey: 'OrgUID',
      header: 'Organization',
      cell: ({ row }: any) => <Badge variant="secondary">{row.getValue('OrgUID')}</Badge>
    },
    {
      accessorKey: 'AvailableForFilter',
      header: 'Filterable',
      cell: ({ row }: any) => (
        <Badge variant={row.getValue('AvailableForFilter') ? 'default' : 'outline'}>
          {row.getValue('AvailableForFilter') ? 'Yes' : 'No'}
        </Badge>
      )
    },
    {
      id: 'actions',
      header: 'Actions',
      cell: ({ row }: any) => (
        <div className="flex gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleView(row.original)}
          >
            <Eye className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleEdit(row.original)}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleDelete(row.original.UID)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
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

  useEffect(() => {
    filterData()
  }, [groupTypes, searchTerm, filterLevel])

  const fetchGroupTypes = async () => {
    try {
      setLoading(true)
      const request = hierarchyService.buildPagingRequest(1, 100)
      const response = await hierarchyService.getAllSKUGroupTypes(request)
      setGroupTypes(response.PagedData)
      
      // Calculate maximum item level from existing data
      const currentMaxLevel = Math.max(...response.PagedData.map(item => item.ItemLevel), 0)
      setMaxItemLevel(Math.max(currentMaxLevel, 4)) // Minimum 4 levels
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

    if (filterLevel && filterLevel !== 'all') {
      filtered = filtered.filter(item => item.ItemLevel.toString() === filterLevel)
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
    // Could navigate to detailed view or show details modal
    toast({
      title: 'SKU Group Type Details',
      description: `${item.Name} (${item.Code}) - Level ${item.ItemLevel}`,
    })
  }

  const handleDelete = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this SKU group type?')) return

    try {
      await hierarchyService.deleteSKUGroupType(uid)
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
        await hierarchyService.updateSKUGroupType(updateData)
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
        await hierarchyService.createSKUGroupType(createData)
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
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">SKU Group Types</h1>
          <p className="text-muted-foreground">
            Manage hierarchy types like Category, Brand, Sub-Category
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={handleCreate}>
            <Plus className="h-4 w-4 mr-2" />
            Add Group Type
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filters
          </CardTitle>
        </CardHeader>
        <CardContent className="flex gap-4">
          <div className="flex-1">
            <Label htmlFor="search">Search</Label>
            <Input
              id="search"
              placeholder="Search by name or code..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div>
            <Label htmlFor="level">Item Level</Label>
            <Select value={filterLevel} onValueChange={setFilterLevel}>
              <SelectTrigger id="level" className="w-[200px]">
                <SelectValue placeholder="All levels" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All levels</SelectItem>
                {getAvailableLevels()
                  .filter(level => level.isExisting)
                  .map((level) => (
                    <SelectItem key={level.value} value={level.value.toString()}>
                      {level.label}
                    </SelectItem>
                  ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Group Types ({filteredGroupTypes.length})</CardTitle>
              <CardDescription>
                Showing {filteredGroupTypes.length} of {groupTypes.length} group types
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={filteredGroupTypes}
            loading={loading}
          />
        </CardContent>
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>
              {editingItem ? 'Edit SKU Group Type' : 'Create SKU Group Type'}
            </DialogTitle>
            <DialogDescription>
              {editingItem ? 'Update the group type details' : 'Add a new SKU group type to the hierarchy'}
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