'use client'

import { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
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
import { skuAttributesService, SKUAttribute, SKUAttributeDropdownModel } from '@/services/sku/sku-attributes.service'

interface FormData {
  UID: string
  SKUUID: string
  Type: string
  Code: string
  Value: string
  ParentType?: string
}

export default function AttributesPage() {
  const router = useRouter()
  const [attributes, setAttributes] = useState<SKUAttribute[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [groupTypes, setGroupTypes] = useState<SKUAttributeDropdownModel[]>([])
  const [availableTypes, setAvailableTypes] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<SKUAttribute | null>(null)
  const [formData, setFormData] = useState<FormData>({
    UID: '',
    SKUUID: '',
    Type: '',
    Code: '',
    Value: '',
    ParentType: ''
  })
  const [searchTerm, setSearchTerm] = useState('')
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('')
  const [filterType, setFilterType] = useState<string[]>([])
  const [exporting, setExporting] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const searchInputRef = useRef<HTMLInputElement>(null)
  const { toast } = useToast()

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm)
    }, 500)
    return () => clearTimeout(timer)
  }, [searchTerm])

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
      accessorKey: 'Value',
      header: 'Value',
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue('Value')}</span>
      )
    },
    {
      accessorKey: 'Type',
      header: 'Type',
      cell: ({ row }: any) => (
        <Badge
          variant="secondary"
          className="bg-blue-100 text-blue-800 hover:bg-blue-100 text-xs"
        >
          {row.getValue('Type')}
        </Badge>
      )
    },
    {
      accessorKey: 'SKUUID',
      header: 'SKU Code',
      cell: ({ row }: any) => (
        <span className="text-sm font-mono text-gray-600">{row.getValue('SKUUID')}</span>
      )
    },
    {
      accessorKey: 'ParentType',
      header: 'Parent Type',
      cell: ({ row }: any) => {
        const parent = row.getValue('ParentType')
        if (!parent) return <span className="text-xs text-gray-500">None</span>
        return <span className="text-sm text-gray-600">{parent}</span>
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
                onClick={() => handleEdit(row.original)}
                className="cursor-pointer"
              >
                <Edit className="mr-2 h-4 w-4" />
                Edit Attribute
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => handleDelete(row.original.UID)}
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
    fetchGroupTypesAndAvailableTypes()
  }, [])

  useEffect(() => {
    fetchData()
  }, [currentPage, pageSize, debouncedSearchTerm, filterType])

  // Reset to page 1 when filters change
  useEffect(() => {
    setCurrentPage(1)
  }, [filterType])

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

  const fetchGroupTypesAndAvailableTypes = async () => {
    try {
      // Fetch group types for form dropdown
      const groupTypesData = await skuAttributesService.getSKUGroupTypeForAttribute()
      console.log(`Successfully loaded ${groupTypesData.length} group types`)
      setGroupTypes(groupTypesData)

      // Extract unique type names from group types
      const types = groupTypesData.map(gt => gt.DropDownTitle).filter(Boolean)
      setAvailableTypes(types)
    } catch (error) {
      console.error('Error fetching group types:', error)
    }
  }

  const fetchData = async () => {
    try {
      setLoading(true)

      // Build filter criteria with LIKE search
      const filterCriterias: any[] = []

      // Add search filter with Type=7 (Like) for pattern matching
      if (debouncedSearchTerm && debouncedSearchTerm.trim()) {
        const searchValue = debouncedSearchTerm.trim()

        // Search by Code with Type=7 (Like) for pattern matching
        filterCriterias.push({
          Name: 'Code',
          Value: searchValue,
          Type: 7  // FilterType.Like for LIKE '%value%' search
        })
      }

      // Add type filter if selected (exact match)
      if (filterType.length > 0 && filterType.length === 1) {
        filterCriterias.push({
          Name: 'Type',
          Value: filterType[0],
          Type: 1  // FilterType.Equal for exact match
        })
      }

      const requestBody = {
        PageNumber: currentPage,
        PageSize: pageSize,
        IsCountRequired: true,
        FilterCriterias: filterCriterias,
        SortCriterias: []
      }

      console.log('📊 Fetching attributes with filters:', JSON.stringify(filterCriterias, null, 2))

      const response = await skuAttributesService.getAllSKUAttributes(requestBody)

      if (response?.Data) {
        setAttributes(response.Data.PagedData || [])
        setTotalCount(response.Data.TotalCount || 0)
        console.log(`✅ Loaded ${response.Data.PagedData?.length} attributes (Total: ${response.Data.TotalCount})`)
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch data',
        variant: 'destructive'
      })
      console.error('❌ Error fetching data:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleCreate = () => {
    setEditingItem(null)
    setFormData({
      UID: '',
      SKUUID: '',
      Type: '',
      Code: '',
      Value: '',
      ParentType: ''
    })
    setDialogOpen(true)
  }

  const handleEdit = (item: SKUAttribute) => {
    setEditingItem(item)
    setFormData({
      UID: item.UID,
      SKUUID: item.SKUUID,
      Type: item.Type,
      Code: item.Code,
      Value: item.Value,
      ParentType: item.ParentType || ''
    })
    setDialogOpen(true)
  }

  const handleDelete = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this attribute?')) return

    try {
      await skuAttributesService.deleteSKUAttribute(uid)
      toast({
        title: 'Success',
        description: 'Attribute deleted successfully',
      })
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete attribute',
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
        await skuAttributesService.updateSKUAttribute(updateData)
        toast({
          title: 'Success',
          description: 'Attribute updated successfully',
        })
      } else {
        // Create new
        const createData = {
          ...formData,
          UID: `${formData.SKUUID}_${formData.Type}_${formData.Code}`,
          CreatedBy: 'ADMIN',
          ModifiedBy: 'ADMIN',
        }
        await skuAttributesService.createSKUAttribute(createData)
        toast({
          title: 'Success',
          description: 'Attribute created successfully',
        })
      }
      setDialogOpen(false)
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save attribute',
        variant: 'destructive'
      })
    }
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      // Simply export the current page data
      if (attributes.length === 0) {
        toast({
          title: "No Data",
          description: "No attributes found to export.",
          variant: "default",
        })
        setExporting(false)
        return
      }

      const headers = ["Code", "Value", "Type", "SKU Code", "Parent Type"]
      const csvRows = attributes.map(attr => [
        attr.Code || "",
        attr.Value || "",
        attr.Type || "",
        attr.SKUUID || "",
        attr.ParentType || "None"
      ])

      const csvContent = [
        headers.join(","),
        ...csvRows.map(row =>
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
      a.download = `sku-attributes-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      toast({
        title: "Export Complete",
        description: `${csvRows.length} attributes exported successfully to CSV file.`,
      })
    } catch (error) {
      console.error("Export error:", error)
      toast({
        title: "Export Failed",
        description: "Failed to export attributes. Please try again.",
        variant: "destructive",
      })
    } finally {
      setExporting(false)
    }
  }

  // Use availableTypes from group-types endpoint instead of extracting from current page

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">SKU Attributes</h1>
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
            Add Attribute
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
                placeholder="Search by code, value, type, or SKU UID... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Type Filter Dropdown */}
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
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>Filter by Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {availableTypes.map((type) => (
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
                    <Badge variant="outline" className="text-xs">
                      {type}
                    </Badge>
                  </DropdownMenuCheckboxItem>
                ))}
                {filterType.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterType([])}
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
            data={attributes}
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
              itemName="attributes"
            />
          </div>
        )}
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>
              {editingItem ? 'Edit SKU Attribute' : 'Create SKU Attribute'}
            </DialogTitle>
            <DialogDescription>
              {editingItem ? 'Update the attribute details' : 'Add a new SKU attribute'}
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="skuuid">SKU Code *</Label>
              <Input
                id="skuuid"
                value={formData.SKUUID}
                onChange={(e) => setFormData({ ...formData, SKUUID: e.target.value })}
                placeholder="Enter SKU Code"
                disabled={!!editingItem}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="type">Type *</Label>
              <Select
                value={formData.Type}
                onValueChange={(value) => setFormData({ ...formData, Type: value })}
              >
                <SelectTrigger id="type">
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {groupTypes.map((gt) => (
                    <SelectItem key={gt.UID} value={gt.DropDownTitle}>
                      {gt.DropDownTitle}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="code">Code *</Label>
              <Input
                id="code"
                value={formData.Code}
                onChange={(e) => setFormData({ ...formData, Code: e.target.value })}
                placeholder="Enter attribute code"
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="value">Value *</Label>
              <Input
                id="value"
                value={formData.Value}
                onChange={(e) => setFormData({ ...formData, Value: e.target.value })}
                placeholder="Enter attribute value"
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="parentType">Parent Type (Optional)</Label>
              <Input
                id="parentType"
                value={formData.ParentType}
                onChange={(e) => setFormData({ ...formData, ParentType: e.target.value })}
                placeholder="Enter parent type"
              />
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
