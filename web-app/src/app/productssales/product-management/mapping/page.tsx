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
import { Plus, Edit, Trash2, Filter, Download, Upload, Search } from 'lucide-react'
import { hierarchyService, SKUToGroupMapping, SKUGroup, SKUGroupType } from '@/services/sku/hierarchy.service'
import { skuService, SKUListView } from '@/services/sku/sku.service'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'

interface FormData {
  UID: string
  SKUUID: string
  SKUGroupUID: string
}

interface ExtendedMapping extends SKUToGroupMapping {
  SKUCode?: string
  SKUName?: string
  GroupName?: string
  GroupTypeName?: string
}

export default function ProductMappingPage() {
  const [mappings, setMappings] = useState<ExtendedMapping[]>([])
  const [filteredMappings, setFilteredMappings] = useState<ExtendedMapping[]>([])
  const [groups, setGroups] = useState<SKUGroup[]>([])
  const [groupTypes, setGroupTypes] = useState<SKUGroupType[]>([])
  const [skus, setSKUs] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<SKUToGroupMapping | null>(null)
  const [formData, setFormData] = useState<FormData>({
    UID: '',
    SKUUID: '',
    SKUGroupUID: ''
  })
  const [searchTerm, setSearchTerm] = useState('')
  const [filterGroupType, setFilterGroupType] = useState<string>('all')
  const [skuSearchTerm, setSkuSearchTerm] = useState('')
  const [groupSearchTerm, setGroupSearchTerm] = useState('')
  const { toast } = useToast()

  // Table columns configuration
  const columns = [
    {
      accessorKey: 'SKUCode',
      header: 'SKU Code',
      cell: ({ row }: any) => (
        <div>
          <span className="font-mono text-sm">{row.original.SKUCode}</span>
          <div className="text-xs text-muted-foreground truncate max-w-[200px]">
            {row.original.SKUName}
          </div>
        </div>
      )
    },
    {
      accessorKey: 'GroupName',
      header: 'Group',
      cell: ({ row }: any) => (
        <div>
          <div className="font-medium">{row.original.GroupName}</div>
          <Badge variant="outline" className="text-xs">
            {row.original.GroupTypeName}
          </Badge>
        </div>
      )
    },
    {
      accessorKey: 'CreatedBy',
      header: 'Created By',
      cell: ({ row }: any) => (
        <div>
          <div className="text-sm">{row.getValue('CreatedBy')}</div>
          <div className="text-xs text-muted-foreground">
            {formatDateToDayMonthYear(row.original.CreatedTime)}
          </div>
        </div>
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
  }, [])

  useEffect(() => {
    filterData()
  }, [mappings, searchTerm, filterGroupType])

  const fetchData = async () => {
    try {
      setLoading(true)
      
      // Fetch all required data
      const [mappingsResponse, groupsResponse, groupTypesResponse, skusResponse] = await Promise.all([
        hierarchyService.getAllSKUToGroupMappings(hierarchyService.buildPagingRequest(1, 100)),
        hierarchyService.getAllSKUGroups(hierarchyService.buildPagingRequest(1, 100)),
        hierarchyService.getAllSKUGroupTypes(hierarchyService.buildPagingRequest(1, 100)),
        skuService.getAllSKUs(hierarchyService.buildPagingRequest(1, 100))
      ])
      
      console.log('SKU Response:', skusResponse)
      
      // The SKU service wraps the response in { success, data }
      // The actual API response is in skusResponse.data
      const actualSkuResponse = skusResponse.data
      console.log('Actual SKU API Response:', actualSkuResponse)
      console.log('SKU PagedData:', actualSkuResponse?.PagedData)
      
      setGroups(groupsResponse.PagedData)
      setGroupTypes(groupTypesResponse.PagedData)
      
      // Extract SKUs from the actual response
      const skuData = actualSkuResponse?.PagedData || []
      console.log('Extracted SKUs:', skuData)
      
      // Transform SKU data to have consistent field names
      const transformedSKUs = skuData.map((sku: any) => ({
        SKUUID: sku.UID || sku.SKUUID,
        SKUCode: sku.Code || sku.SKUCode,
        SKULongName: sku.LongName || sku.Name || sku.SKULongName,
        ...sku
      }))
      console.log('Transformed SKUs:', transformedSKUs)
      setSKUs(transformedSKUs)
      
      // Enhance mappings with related data
      const enhancedMappings = mappingsResponse.PagedData.map(mapping => {
        const sku = transformedSKUs.find((s: any) => s.SKUUID === mapping.SKUUID)
        const group = groupsResponse.PagedData.find(g => g.UID === mapping.SKUGroupUID)
        const groupType = group ? groupTypesResponse.PagedData.find(gt => gt.UID === group.SkuGroupTypeUID) : null
        
        return {
          ...mapping,
          SKUCode: sku?.SKUCode,
          SKUName: sku?.SKULongName,
          GroupName: group?.Name,
          GroupTypeName: groupType?.Name
        }
      })
      
      setMappings(enhancedMappings)
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
    let filtered = [...mappings]

    if (searchTerm) {
      filtered = filtered.filter(item => 
        item.SKUCode?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.SKUName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.GroupName?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    if (filterGroupType && filterGroupType !== 'all') {
      filtered = filtered.filter(item => {
        const group = groups.find(g => g.UID === item.SKUGroupUID)
        return group?.SkuGroupTypeUID === filterGroupType
      })
    }

    setFilteredMappings(filtered)
  }

  const handleCreate = () => {
    setEditingItem(null)
    setFormData({
      UID: '',
      SKUUID: '',
      SKUGroupUID: ''
    })
    setSkuSearchTerm('')
    setGroupSearchTerm('')
    setDialogOpen(true)
  }

  const handleEdit = (item: SKUToGroupMapping) => {
    setEditingItem(item)
    setFormData({
      UID: item.UID,
      SKUUID: item.SKUUID,
      SKUGroupUID: item.SKUGroupUID
    })
    setSkuSearchTerm('')
    setGroupSearchTerm('')
    setDialogOpen(true)
  }

  const handleDelete = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this mapping?')) return

    try {
      await hierarchyService.deleteSKUToGroupMapping(uid)
      toast({
        title: 'Success',
        description: 'Mapping deleted successfully',
      })
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete mapping',
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
        await hierarchyService.updateSKUToGroupMapping(updateData)
        toast({
          title: 'Success',
          description: 'Mapping updated successfully',
        })
      } else {
        // Create new
        const createData = {
          ...formData,
          UID: `${formData.SKUUID}_${formData.SKUGroupUID}`, // Generate UID
          CreatedBy: 'ADMIN',
          CreatedTime: new Date().toISOString(),
          ModifiedBy: 'ADMIN',
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString()
        }
        await hierarchyService.createSKUToGroupMapping(createData)
        toast({
          title: 'Success',
          description: 'Mapping created successfully',
        })
      }
      setDialogOpen(false)
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save mapping',
        variant: 'destructive'
      })
    }
  }

  const getFilteredSKUs = () => {
    if (!skus || skus.length === 0) {
      console.log('No SKUs available')
      return []
    }
    if (!skuSearchTerm) {
      console.log('Returning first 50 SKUs:', skus.slice(0, 50))
      return skus.slice(0, 50) // Show first 50 by default
    }
    const filtered = skus.filter(sku => 
      sku.SKUCode?.toLowerCase().includes(skuSearchTerm.toLowerCase()) ||
      sku.SKULongName?.toLowerCase().includes(skuSearchTerm.toLowerCase())
    ).slice(0, 20)
    console.log('Filtered SKUs:', filtered)
    return filtered
  }

  const getFilteredGroups = () => {
    if (!groups || groups.length === 0) return []
    if (!groupSearchTerm) return groups.slice(0, 50) // Show first 50 by default
    return groups.filter(group => 
      group.Name?.toLowerCase().includes(groupSearchTerm.toLowerCase()) ||
      group.Code?.toLowerCase().includes(groupSearchTerm.toLowerCase())
    ).slice(0, 20)
  }


  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Mapping</h1>
          <p className="text-muted-foreground">
            Manage relationships between products and groups for classification
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
            Add Mapping
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
              placeholder="Search by product or group name..."
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
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Mappings ({filteredMappings.length})</CardTitle>
              <CardDescription>
                Showing {filteredMappings.length} of {mappings.length} mappings
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={filteredMappings}
            loading={loading}
          />
        </CardContent>
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-[600px]">
          <DialogHeader>
            <DialogTitle>
              {editingItem ? 'Edit Product Mapping' : 'Create Product Mapping'}
            </DialogTitle>
            <DialogDescription>
              {editingItem ? 'Update the mapping details' : 'Map a product to a group for classification'}
            </DialogDescription>
          </DialogHeader>
          
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="sku-search">Product *</Label>
              <div className="space-y-2">
                <div className="flex gap-2">
                  <Input
                    id="sku-search"
                    placeholder="Search products by code or name..."
                    value={skuSearchTerm}
                    onChange={(e) => setSkuSearchTerm(e.target.value)}
                  />
                  <Button variant="outline" size="icon">
                    <Search className="h-4 w-4" />
                  </Button>
                </div>
                <Select 
                  value={formData.SKUUID} 
                  onValueChange={(value) => setFormData({ ...formData, SKUUID: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select product" />
                  </SelectTrigger>
                  <SelectContent className="max-h-[200px]">
                    {getFilteredSKUs().map((sku) => (
                      <SelectItem key={sku.SKUUID} value={sku.SKUUID}>
                        <div className="flex flex-col">
                          <span className="font-mono text-sm">{sku.SKUCode}</span>
                          <span className="text-xs text-muted-foreground truncate max-w-[300px]">
                            {sku.SKULongName}
                          </span>
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            
            <div className="grid gap-2">
              <Label htmlFor="group-search">Group *</Label>
              <div className="space-y-2">
                <div className="flex gap-2">
                  <Input
                    id="group-search"
                    placeholder="Search groups by name or code..."
                    value={groupSearchTerm}
                    onChange={(e) => setGroupSearchTerm(e.target.value)}
                  />
                  <Button variant="outline" size="icon">
                    <Search className="h-4 w-4" />
                  </Button>
                </div>
                <Select 
                  value={formData.SKUGroupUID} 
                  onValueChange={(value) => setFormData({ ...formData, SKUGroupUID: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select group" />
                  </SelectTrigger>
                  <SelectContent className="max-h-[200px]">
                    {getFilteredGroups().map((group) => {
                      const groupType = groupTypes.find(gt => gt.UID === group.SkuGroupTypeUID)
                      return (
                        <SelectItem key={group.UID} value={group.UID}>
                          <div className="flex flex-col">
                            <span className="font-medium">{group.Name}</span>
                            <span className="text-xs text-muted-foreground">
                              {groupType?.Name} - Level {group.ItemLevel}
                            </span>
                          </div>
                        </SelectItem>
                      )
                    })}
                  </SelectContent>
                </Select>
              </div>
            </div>

            {/* Selected mapping preview */}
            {formData.SKUUID && formData.SKUGroupUID && (
              <div className="border rounded-lg p-3 bg-muted/50">
                <h4 className="font-semibold mb-2">Mapping Preview</h4>
                <div className="flex items-center gap-4 text-sm">
                  <div>
                    <span className="font-medium">Product:</span>{' '}
                    {skus.find(s => s.SKUUID === formData.SKUUID)?.SKUCode}
                  </div>
                  <span>→</span>
                  <div>
                    <span className="font-medium">Group:</span>{' '}
                    {groups.find(g => g.UID === formData.SKUGroupUID)?.Name}
                  </div>
                </div>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={!formData.SKUUID || !formData.SKUGroupUID}>
              {editingItem ? 'Update' : 'Create'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}