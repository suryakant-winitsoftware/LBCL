'use client'

import { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { DataTable } from '@/components/ui/data-table'
import { PaginationControls } from '@/components/ui/pagination-controls'
import { useToast } from '@/components/ui/use-toast'
import { Plus, Edit, Trash2, Eye, Download, MoreHorizontal, Loader2, Search, Warehouse } from 'lucide-react'
import { apiService } from '@/services/api'
import { authService } from '@/lib/auth-service'

interface WarehouseItemView {
  Id: number
  WarehouseCode: string
  WarehouseUID: string
  WarehouseName: string
  ParentName: string
  ModifiedTime: string
  OrgTypeUID: string
}

interface EditWareHouseItemView {
  UID: string
  WarehouseCode: string
  ParentUID: string
  WarehouseName: string
  WarehouseType: string
  FranchiseCode: string
  FranchiseName: string
  OrgTypeUID: string
  AddressUID: string
  AddressName: string
  AddressLine1: string
  AddressLine2: string
  AddressLine3: string
  Landmark: string
  Area: string
  ZipCode: string
  City: string
  RegionCode: string
  LinkedItemUID: string
}

export default function WarehousesPage() {
  const router = useRouter()
  const [warehouses, setWarehouses] = useState<WarehouseItemView[]>([])
  const [filteredWarehouses, setFilteredWarehouses] = useState<WarehouseItemView[]>([])
  const [loading, setLoading] = useState(true)
  const [totalCount, setTotalCount] = useState(0)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<WarehouseItemView | null>(null)
  const [formData, setFormData] = useState<EditWareHouseItemView>({
    UID: '',
    WarehouseCode: '',
    ParentUID: '',
    WarehouseName: '',
    WarehouseType: '',
    FranchiseCode: '',
    FranchiseName: '',
    OrgTypeUID: '',
    AddressUID: '',
    AddressName: '',
    AddressLine1: '',
    AddressLine2: '',
    AddressLine3: '',
    Landmark: '',
    Area: '',
    ZipCode: '',
    City: '',
    RegionCode: '',
    LinkedItemUID: '',
  })
  const [searchTerm, setSearchTerm] = useState('')
  const [exporting, setExporting] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [franchiseeOrgUID, setFranchiseeOrgUID] = useState('')
  const [warehouseTypes, setWarehouseTypes] = useState<Array<{UID: string, WarehouseType: string}>>([])
  const [selectedWarehouseType, setSelectedWarehouseType] = useState<string>('FRWH') // Hardcoded to FRWH
  const searchInputRef = useRef<HTMLInputElement>(null)
  const { toast } = useToast()

  // Table columns configuration
  const columns = [
    {
      accessorKey: 'WarehouseCode',
      header: () => <div className="pl-6">Code</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.getValue('WarehouseCode') || '-'}</span>
        </div>
      )
    },
    {
      accessorKey: 'WarehouseName',
      header: 'Warehouse Name',
      cell: ({ row }: any) => (
        <div className="flex items-center gap-2">
          <Warehouse className="h-4 w-4 text-gray-400" />
          <span className="font-medium text-sm">{row.getValue('WarehouseName')}</span>
        </div>
      )
    },
    {
      accessorKey: 'ParentName',
      header: 'Parent',
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue('ParentName') || '-'}</span>
      )
    },
    {
      accessorKey: 'ModifiedTime',
      header: 'Last Modified',
      cell: ({ row }: any) => {
        const date = row.getValue('ModifiedTime')
        return (
          <span className="text-sm">
            {date ? new Date(date as string).toLocaleDateString() : '-'}
          </span>
        )
      }
    },
    {
      id: 'actions',
      header: () => <div className="text-right pr-6">Actions</div>,
      cell: ({ row }: any) => {
        const warehouse = row.original
        return (
          <div className="text-right pr-6">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="sm">
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => handleView(warehouse)}>
                  <Eye className="h-4 w-4 mr-2" />
                  View
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => router.push(`/administration/warehouse-management/warehouses/create?id=${warehouse.WarehouseUID}`)}>
                  <Edit className="h-4 w-4 mr-2" />
                  Edit
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onClick={() => handleDelete(warehouse)}
                  className="text-red-600"
                >
                  <Trash2 className="h-4 w-4 mr-2" />
                  Delete
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        )
      }
    },
  ]

  useEffect(() => {
    // Get franchisee UID from current user's organization
    const currentUser = authService.getCurrentUser()
    const orgUID = currentUser?.currentOrganization?.uid || currentUser?.uid || ''

    console.log('Current User:', currentUser)
    console.log('Organization UID:', orgUID)

    if (orgUID) {
      setFranchiseeOrgUID(orgUID)
    } else {
      toast({
        title: 'Warning',
        description: 'No organization found. Showing all warehouses without parent filter.',
        variant: 'default'
      })
      // Set empty to show all warehouses
      setFranchiseeOrgUID('')
    }
  }, [])

  useEffect(() => {
    // Always fetch data, even if franchiseeOrgUID is empty (will show all warehouses)
    fetchWarehouseTypes()
    fetchData()
  }, [franchiseeOrgUID, currentPage, pageSize, selectedWarehouseType])

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
  }, [warehouses, searchTerm])

  const fetchWarehouseTypes = async () => {
    try {
      const response: any = await apiService.post('/Org/GetOrgTypeDetails', {
        PageNumber: 1,
        PageSize: 999,
        IsCountRequired: true,
        FilterCriterias: [],
        SortCriterias: []
      })

      setWarehouseTypes(response.Data?.PagedData || [])
    } catch (error) {
      console.error('Error fetching warehouse types:', error)
    }
  }

  const fetchData = async () => {
    try {
      setLoading(true)

      console.log('Fetching warehouses with:', {
        franchiseeOrgUID,
        selectedWarehouseType,
        currentPage,
        pageSize
      })

      let warehouseData: any[] = []

      // Hardcoded to always fetch FRWH warehouses
      const url = `/Org/GetOrgByOrgTypeUID?OrgTypeUID=FRWH`
      console.log('🔵 API Call:', url)

      const response: any = await apiService.get(url)
      console.log('🔵 Full API Response:', response)
      console.log('🔵 Response.Data:', response.Data)
      console.log('🔵 Response.IsSuccess:', response.IsSuccess)

      // The backend returns { Data: [...], IsSuccess: true, StatusCode: 200 }
      if (response.IsSuccess && response.Data) {
        warehouseData = response.Data
        console.log('🔵 Raw warehouse data from API:', warehouseData)

        // Fetch parent organization names for all warehouses
        const parentUIDs = [...new Set(warehouseData.map((item: any) => item.ParentUID).filter(Boolean))]
        console.log('🔵 Unique Parent UIDs:', parentUIDs)

        const parentOrgMap: Record<string, string> = {}

        // Fetch parent organization details
        for (const parentUID of parentUIDs) {
          try {
            const parentResponse: any = await apiService.get(`/Org/GetOrgByUID?UID=${parentUID}`)
            if (parentResponse.IsSuccess && parentResponse.Data) {
              parentOrgMap[parentUID] = parentResponse.Data.Name || parentResponse.Data.Code || parentUID
              console.log(`🔵 Parent ${parentUID}: ${parentOrgMap[parentUID]}`)
            }
          } catch (error) {
            console.warn(`Failed to fetch parent org ${parentUID}:`, error)
            parentOrgMap[parentUID] = parentUID // Fallback to UID if fetch fails
          }
        }

        // Transform the data to match WarehouseItemView interface
        warehouseData = warehouseData.map((item: any, index: number) => {
          console.log(`🔵 Transforming item ${index}:`, item)
          return {
            Id: item.Id || 0,
            WarehouseCode: item.Code || item.OrganizationCode,
            WarehouseUID: item.UID,
            WarehouseName: item.Name || item.OrganizationNameEn,
            ParentName: parentOrgMap[item.ParentUID] || item.ParentUID || '-',
            ModifiedTime: item.ModifiedTime || item.ServerModifiedTime,
            OrgTypeUID: item.OrgTypeUID || item.org_type_uid
          }
        })
        console.log('🔵 Transformed warehouse data:', warehouseData)
      } else {
        console.log('❌ API response not successful or no data')
        warehouseData = []
      }

      console.log('Final warehouse data:', warehouseData)
      setWarehouses(warehouseData)
      setTotalCount(warehouseData.length)
    } catch (error) {
      console.error('Error fetching data:', error)
      toast({
        title: 'Error',
        description: 'Failed to fetch warehouses. Check console for details.',
        variant: 'destructive'
      })
    } finally {
      setLoading(false)
    }
  }

  const filterData = () => {
    let filtered = [...warehouses]

    if (searchTerm) {
      filtered = filtered.filter(item =>
        item.WarehouseCode?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.WarehouseName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.ParentName?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    setFilteredWarehouses(filtered)
  }

  const handleCreate = () => {
    router.push('/administration/warehouse-management/warehouses/create')
  }

  const handleEdit = async (item: WarehouseItemView) => {
    try {
      const response: any = await apiService.get(`/Org/ViewFranchiseeWarehouseByUID?UID=${item.WarehouseUID}`)
      const data = response.Data

      setEditingItem(item)
      setFormData({
        UID: data.UID || item.WarehouseUID,
        WarehouseCode: data.WarehouseCode || '',
        ParentUID: data.ParentUID || '',
        WarehouseName: data.WarehouseName || '',
        WarehouseType: data.WarehouseType || '',
        FranchiseCode: data.FranchiseCode || '',
        FranchiseName: data.FranchiseName || '',
        OrgTypeUID: data.OrgTypeUID || '',
        AddressUID: data.AddressUID || '',
        AddressName: data.AddressName || '',
        AddressLine1: data.AddressLine1 || '',
        AddressLine2: data.AddressLine2 || '',
        AddressLine3: data.AddressLine3 || '',
        Landmark: data.Landmark || '',
        Area: data.Area || '',
        ZipCode: data.ZipCode || '',
        City: data.City || '',
        RegionCode: data.RegionCode || '',
        LinkedItemUID: data.LinkedItemUID || '',
      })
      setDialogOpen(true)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to load warehouse details',
        variant: 'destructive'
      })
    }
  }

  const handleView = (item: WarehouseItemView) => {
    router.push(`/administration/warehouse-management/warehouses/view/${item.WarehouseUID}`)
  }

  const handleDelete = async (item: WarehouseItemView) => {
    if (!confirm(`Are you sure you want to delete warehouse "${item.WarehouseName}"?`)) {
      return
    }

    try {
      await apiService.delete(`/Org/DeleteViewFranchiseeWarehouse?UID=${item.WarehouseUID}`)
      toast({
        title: 'Success',
        description: 'Warehouse deleted successfully',
      })
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete warehouse',
        variant: 'destructive'
      })
    }
  }

  const handleSave = async () => {
    // Validation
    if (!formData.WarehouseCode || !formData.WarehouseName || !formData.OrgTypeUID || !formData.AddressLine1) {
      toast({
        title: 'Validation Error',
        description: 'Please fill in all required fields (Code, Name, Type, and Address Line 1)',
        variant: 'destructive'
      })
      return
    }

    try {
      if (editingItem) {
        await apiService.put('/Org/UpdateViewFranchiseeWarehouse', formData)
        toast({
          title: 'Success',
          description: 'Warehouse updated successfully',
        })
      } else {
        // Generate UID and LinkedItemUID as per backend logic
        const warehouseUID = `${franchiseeOrgUID}_${formData.WarehouseCode}`
        const addressUID = crypto.randomUUID()

        await apiService.post('/Org/CreateViewFranchiseeWarehouse', {
          ...formData,
          UID: warehouseUID,
          ParentUID: franchiseeOrgUID,
          AddressUID: addressUID,
          LinkedItemUID: warehouseUID,
        })
        toast({
          title: 'Success',
          description: 'Warehouse created successfully',
        })
      }
      setDialogOpen(false)
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save warehouse',
        variant: 'destructive'
      })
    }
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      if (filteredWarehouses.length === 0) {
        toast({
          title: "No Data",
          description: "No warehouses found to export.",
          variant: "default",
        })
        return
      }

      const headers = ["Code", "Name", "Parent", "Modified Time"]

      const exportData = filteredWarehouses.map(wh => [
        wh.WarehouseCode || "",
        wh.WarehouseName || "",
        wh.ParentName || "",
        wh.ModifiedTime ? new Date(wh.ModifiedTime).toLocaleDateString() : ""
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
      a.download = `warehouses-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      toast({
        title: "Export Complete",
        description: `${exportData.length} warehouses exported successfully.`,
      })
    } catch (error) {
      console.error("Export error:", error)
      toast({
        title: "Export Failed",
        description: "Failed to export warehouses.",
        variant: "destructive",
      })
    } finally {
      setExporting(false)
    }
  }

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Warehouses</h1>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handleExport}
            disabled={exporting || filteredWarehouses.length === 0}
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
            Add Warehouse
          </Button>
        </div>
      </div>

      {/* Search and Filter */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by code, name, or parent... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            <div>
              <Select
                value={selectedWarehouseType}
                onValueChange={(value) => {
                  setSelectedWarehouseType(value)
                  setCurrentPage(1)
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Filter by warehouse type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Warehouses</SelectItem>
                  <SelectItem value="FRWH">Franchise Warehouses (FRWH)</SelectItem>
                  <SelectItem value="VWH">Van Warehouses (VWH)</SelectItem>
                  {warehouseTypes.map((type) => (
                    <SelectItem key={type.UID} value={type.UID}>
                      {type.WarehouseType}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          <DataTable
            columns={columns}
            data={filteredWarehouses.slice((currentPage - 1) * pageSize, currentPage * pageSize)}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>

        {filteredWarehouses.length > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={filteredWarehouses.length}
              pageSize={pageSize}
              onPageChange={(page) => {
                setCurrentPage(page);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }}
              onPageSizeChange={(size) => {
                setPageSize(size);
                setCurrentPage(1);
              }}
              itemName="warehouses"
            />
          </div>
        )}
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{editingItem ? 'Edit Warehouse' : 'Add New Warehouse'}</DialogTitle>
            <DialogDescription>
              {editingItem ? 'Update warehouse details' : 'Create a new warehouse entry'}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-6 py-4">
            {/* Warehouse Details Section */}
            <div>
              <h3 className="text-lg font-semibold mb-4">Warehouse Details</h3>
              <div className="grid grid-cols-3 gap-4">
                <div className="grid gap-2">
                  <Label htmlFor="type">
                    Warehouse Type <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={formData.OrgTypeUID}
                    onValueChange={(value) => {
                      const selectedType = warehouseTypes.find(t => t.UID === value)
                      setFormData({
                        ...formData,
                        OrgTypeUID: value,
                        WarehouseType: selectedType?.WarehouseType || ''
                      })
                    }}
                  >
                    <SelectTrigger id="type">
                      <SelectValue placeholder="Select warehouse type" />
                    </SelectTrigger>
                    <SelectContent>
                      {warehouseTypes.map((type) => (
                        <SelectItem key={type.UID} value={type.UID}>
                          {type.WarehouseType}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="code">
                    Warehouse Code <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="code"
                    value={formData.WarehouseCode}
                    onChange={(e) => setFormData({ ...formData, WarehouseCode: e.target.value })}
                    placeholder="WH001"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="name">
                    Warehouse Name <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="name"
                    value={formData.WarehouseName}
                    onChange={(e) => setFormData({ ...formData, WarehouseName: e.target.value })}
                    placeholder="Main Warehouse"
                  />
                </div>
              </div>
            </div>

            {/* Address Details Section */}
            <div>
              <h3 className="text-lg font-semibold mb-4">Address Details</h3>
              <div className="grid grid-cols-3 gap-4">
                <div className="grid gap-2">
                  <Label htmlFor="addressLine1">
                    Address Line 1 <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="addressLine1"
                    value={formData.AddressLine1}
                    onChange={(e) => setFormData({ ...formData, AddressLine1: e.target.value })}
                    placeholder="Street address"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="addressLine2">Address Line 2</Label>
                  <Input
                    id="addressLine2"
                    value={formData.AddressLine2}
                    onChange={(e) => setFormData({ ...formData, AddressLine2: e.target.value })}
                    placeholder="Suite, unit, building, floor, etc."
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="addressLine3">Address Line 3</Label>
                  <Input
                    id="addressLine3"
                    value={formData.AddressLine3}
                    onChange={(e) => setFormData({ ...formData, AddressLine3: e.target.value })}
                    placeholder="Additional address info"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="city">City</Label>
                  <Input
                    id="city"
                    value={formData.City}
                    onChange={(e) => setFormData({ ...formData, City: e.target.value })}
                    placeholder="City"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="landmark">Landmark</Label>
                  <Input
                    id="landmark"
                    value={formData.Landmark}
                    onChange={(e) => setFormData({ ...formData, Landmark: e.target.value })}
                    placeholder="Nearby landmark"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="area">Area</Label>
                  <Input
                    id="area"
                    value={formData.Area}
                    onChange={(e) => setFormData({ ...formData, Area: e.target.value })}
                    placeholder="Area or locality"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="regionCode">Region</Label>
                  <Input
                    id="regionCode"
                    value={formData.RegionCode}
                    onChange={(e) => setFormData({ ...formData, RegionCode: e.target.value })}
                    placeholder="Region code"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="zipCode">Zip Code</Label>
                  <Input
                    id="zipCode"
                    value={formData.ZipCode}
                    onChange={(e) => setFormData({ ...formData, ZipCode: e.target.value })}
                    placeholder="Postal code"
                  />
                </div>
              </div>
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
