'use client'

import { useState, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { DataTable } from '@/components/ui/data-table'
import { useToast } from '@/components/ui/use-toast'
import { Plus, Edit, Trash2, Eye, Filter, Download, Upload, TreePine, Building } from 'lucide-react'
import { hierarchyService, SKUGroup, SKUGroupType } from '@/services/sku/hierarchy.service'
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

export default function SKUGroupsPage() {
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
  const [filterGroupType, setFilterGroupType] = useState<string>('all')
  const [filterLevel, setFilterLevel] = useState<string>('all')
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
      header: 'Code',
      cell: ({ row }: any) => <span className="font-mono">{row.getValue('Code')}</span>
    },
    {
      accessorKey: 'Name',
      header: 'Name',
    },
    {
      accessorKey: 'SkuGroupTypeUID',
      header: 'Group Type',
      cell: ({ row }: any) => {
        const skuGroupTypeUID = row.original.SKUGroupTypeUID || row.original.SkuGroupTypeUID || row.getValue('SkuGroupTypeUID')
        const groupType = groupTypes.find(gt => gt.UID === skuGroupTypeUID)
        return <Badge variant="default">{groupType?.Name || skuGroupTypeUID}</Badge>
      }
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
        if (!parent) return <span className="text-xs text-muted-foreground">Root</span>
        const parentGroup = groups.find(g => g.UID === parent)
        return <span className="text-sm text-muted-foreground">{parentGroup?.Name || parent}</span>
      }
    },
    {
      accessorKey: 'SupplierOrgUID',
      header: 'Organization',
      cell: ({ row }: any) => {
        const orgUID = row.getValue('SupplierOrgUID')
        const org = orgLevels.flatMap(level => level.organizations).find(o => o.UID === orgUID)
        return <Badge variant="secondary">{org?.Name || orgUID}</Badge>
      }
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

  useEffect(() => {
    fetchData()
    loadOrganizationHierarchy()
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
  }, [groups, searchTerm, filterGroupType, filterLevel])

  const fetchData = async () => {
    try {
      setLoading(true)
      
      // Fetch both groups and group types
      const [groupsResponse, groupTypesResponse] = await Promise.all([
        hierarchyService.getAllSKUGroups(hierarchyService.buildPagingRequest(1, 100)),
        hierarchyService.getAllSKUGroupTypes(hierarchyService.buildPagingRequest(1, 100))
      ])
      
      setGroups(groupsResponse.PagedData)
      setGroupTypes(groupTypesResponse.PagedData)
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

    if (filterGroupType && filterGroupType !== 'all') {
      filtered = filtered.filter(item => (item.SKUGroupTypeUID || item.SkuGroupTypeUID) === filterGroupType)
    }

    if (filterLevel && filterLevel !== 'all') {
      filtered = filtered.filter(item => item.ItemLevel.toString() === filterLevel)
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
    const groupType = groupTypes.find(gt => gt.UID === item.SkuGroupTypeUID)
    toast({
      title: 'SKU Group Details',
      description: `${item.Name} (${item.Code}) - ${groupType?.Name} Level ${item.ItemLevel}`,
    })
  }

  const handleDelete = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this SKU group?')) return

    try {
      await hierarchyService.deleteSKUGroup(uid)
      toast({
        title: 'Success',
        description: 'SKU group deleted successfully',
      })
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete SKU group',
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
        await hierarchyService.updateSKUGroup(updateData)
        toast({
          title: 'Success',
          description: 'SKU group updated successfully',
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
        await hierarchyService.createSKUGroup(createData)
        toast({
          title: 'Success',
          description: 'SKU group created successfully',
        })
      }
      setDialogOpen(false)
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save SKU group',
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

  // Get hierarchy statistics
  const hierarchyStats = groupTypes.map(gt => ({
    ...gt,
    count: groups.filter(g => g.SkuGroupTypeUID === gt.UID).length
  }))

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">SKU Groups</h1>
          <p className="text-muted-foreground">
            Manage actual groups like Farmley, Electronics, Smartphones
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
            Add Group
          </Button>
        </div>
      </div>

      {/* Organization Hierarchy Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <Building className="h-5 w-5" />
                Organization Hierarchy
              </CardTitle>
              <CardDescription>
                Select organization hierarchy to filter groups
              </CardDescription>
            </div>
            {hasSelection && (
              <Button 
                variant="outline" 
                size="sm"
                onClick={resetHierarchy}
              >
                Reset Selection
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent className="grid gap-4">
          {orgLevels.map((level, index) => (
            <div key={level.orgTypeUID} className="space-y-2">
              <Label className="text-sm font-medium">
                {level.orgTypeName}
              </Label>
              <div className="flex gap-2">
                <Select
                  value={selectedOrgs[index] || ''}
                  onValueChange={(value) => selectOrganization(index, value)}
                  disabled={level.organizations.length === 0}
                >
                  <SelectTrigger className="flex-1">
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
                <div className="flex-1">
                  <Input
                    placeholder={`Or type ${level.orgTypeName.toLowerCase()} manually`}
                    value={selectedOrgs[index] && !level.organizations.find(org => org.UID === selectedOrgs[index]) ? selectedOrgs[index] : ''}
                    onChange={(e) => {
                      const manualValue = e.target.value
                      if (manualValue) {
                        // Update selectedOrgs directly with manual input
                        const updatedSelectedOrgs = [...selectedOrgs]
                        updatedSelectedOrgs[index] = manualValue
                        // Clear subsequent levels when manually typing
                        for (let i = index + 1; i < updatedSelectedOrgs.length; i++) {
                          updatedSelectedOrgs[i] = ''
                        }
                        selectOrganization(index, manualValue)
                      }
                    }}
                    className="text-sm"
                  />
                </div>
              </div>
            </div>
          ))}
          
          {hasSelection && (
            <div className="p-3 bg-green-50 rounded-md">
              <p className="text-sm text-green-800">
                ✓ Selected Organization: {finalSelectedOrganization}
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Hierarchy Overview */}
      <div className="grid gap-4 md:grid-cols-3">
        {hierarchyStats.map((stat) => (
          <Card key={stat.UID}>
            <CardHeader className="pb-2">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium">
                  {stat.Name}
                </CardTitle>
                <TreePine className="h-4 w-4 text-muted-foreground" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.count}</div>
              <p className="text-xs text-muted-foreground">
                Level {stat.ItemLevel} groups
              </p>
            </CardContent>
          </Card>
        ))}
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
            <Label htmlFor="groupType">Group Type</Label>
            <Select value={filterGroupType} onValueChange={setFilterGroupType}>
              <SelectTrigger id="groupType" className="w-[150px]">
                <SelectValue placeholder="All types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All types</SelectItem>
                {groupTypes.map((gt) => (
                  <SelectItem key={gt.UID} value={gt.UID}>
                    {gt.Name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div>
            <Label htmlFor="level">Item Level</Label>
            <Select value={filterLevel} onValueChange={setFilterLevel}>
              <SelectTrigger id="level" className="w-[120px]">
                <SelectValue placeholder="All levels" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All levels</SelectItem>
                <SelectItem value="1">Level 1</SelectItem>
                <SelectItem value="2">Level 2</SelectItem>
                <SelectItem value="3">Level 3</SelectItem>
                <SelectItem value="4">Level 4</SelectItem>
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
              <CardTitle>Groups ({filteredGroups.length})</CardTitle>
              <CardDescription>
                Showing {filteredGroups.length} of {groups.length} groups
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={filteredGroups}
            loading={loading}
          />
        </CardContent>
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>
              {editingItem ? 'Edit SKU Group' : 'Create SKU Group'}
            </DialogTitle>
            <DialogDescription>
              {editingItem ? 'Update the group details' : 'Add a new SKU group'}
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