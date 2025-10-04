'use client'

import { useState, useEffect, useRef } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
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
import { Filter, Download, Loader2, Search, ChevronDown, X, RefreshCw, Package } from 'lucide-react'
import { useOrganizationHierarchy } from '@/hooks/useOrganizationHierarchy'
import { organizationService } from '@/services/organizationService'
import { apiService } from '@/services/api'

interface WHStockSummary {
  OrgUID: string
  WareHouseUID: string
  SKUUID: string
  SKUCode?: string
  SKUName?: string
  UOM?: string
  AvailableQty?: number
  ReservedQty?: number
  InTransitQty?: number
  TotalQty?: number
}

export default function StockSummaryPage() {
  const [stockData, setStockData] = useState<WHStockSummary[]>([])
  const [filteredData, setFilteredData] = useState<WHStockSummary[]>([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedOrgUID, setSelectedOrgUID] = useState('')
  const [selectedWarehouseUID, setSelectedWarehouseUID] = useState('')
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
      accessorKey: 'SKUCode',
      header: () => <div className="pl-6">SKU Code</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.getValue('SKUCode') || '-'}</span>
        </div>
      )
    },
    {
      accessorKey: 'SKUName',
      header: 'SKU Name',
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue('SKUName') || '-'}</span>
      )
    },
    {
      accessorKey: 'UOM',
      header: 'UOM',
      cell: ({ row }: any) => (
        <Badge variant="outline" className="text-xs">
          {row.getValue('UOM') || 'N/A'}
        </Badge>
      )
    },
    {
      accessorKey: 'AvailableQty',
      header: 'Available',
      cell: ({ row }: any) => (
        <span className="text-sm font-medium text-green-600">
          {row.getValue('AvailableQty') || 0}
        </span>
      )
    },
    {
      accessorKey: 'ReservedQty',
      header: 'Reserved',
      cell: ({ row }: any) => (
        <span className="text-sm font-medium text-yellow-600">
          {row.getValue('ReservedQty') || 0}
        </span>
      )
    },
    {
      accessorKey: 'InTransitQty',
      header: 'In Transit',
      cell: ({ row }: any) => (
        <span className="text-sm font-medium text-blue-600">
          {row.getValue('InTransitQty') || 0}
        </span>
      )
    },
    {
      accessorKey: 'TotalQty',
      header: () => <div className="text-right pr-6">Total</div>,
      cell: ({ row }: any) => (
        <div className="text-right pr-6">
          <span className="text-sm font-bold">
            {row.getValue('TotalQty') || 0}
          </span>
        </div>
      )
    },
  ]

  useEffect(() => {
    loadOrganizationHierarchy()
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
    setCurrentPage(1)
  }, [stockData, searchTerm])

  const fetchStockSummary = async () => {
    if (!selectedOrgUID || !selectedWarehouseUID) {
      toast({
        title: 'Selection Required',
        description: 'Please select both organization and warehouse',
        variant: 'default'
      })
      return
    }

    try {
      setLoading(true)

      const response: any = await apiService.get(`/StockUpdater/GetWHStockSummary?orgUID=${selectedOrgUID}&wareHouseUID=${selectedWarehouseUID}`)

      const data = response.Data || []
      setStockData(data)

      if (data.length === 0) {
        toast({
          title: 'No Data',
          description: 'No stock data found for selected warehouse',
          variant: 'default'
        })
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch stock summary',
        variant: 'destructive'
      })
      console.error('Error fetching data:', error)
    } finally {
      setLoading(false)
    }
  }

  const filterData = () => {
    let filtered = [...stockData]

    if (searchTerm) {
      filtered = filtered.filter(item =>
        item.SKUCode?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.SKUName?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    setFilteredData(filtered)
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      if (filteredData.length === 0) {
        toast({
          title: "No Data",
          description: "No stock data found to export.",
          variant: "default",
        })
        return
      }

      const headers = ["SKU Code", "SKU Name", "UOM", "Available", "Reserved", "In Transit", "Total"]

      const exportData = filteredData.map(stock => [
        stock.SKUCode || "",
        stock.SKUName || "",
        stock.UOM || "",
        stock.AvailableQty?.toString() || "0",
        stock.ReservedQty?.toString() || "0",
        stock.InTransitQty?.toString() || "0",
        stock.TotalQty?.toString() || "0"
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
      a.download = `stock-summary-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      toast({
        title: "Export Complete",
        description: `${exportData.length} stock records exported successfully.`,
      })
    } catch (error) {
      console.error("Export error:", error)
      toast({
        title: "Export Failed",
        description: "Failed to export stock data.",
        variant: "destructive",
      })
    } finally {
      setExporting(false)
    }
  }

  // Calculate summary statistics
  const totalAvailable = filteredData.reduce((sum, item) => sum + (item.AvailableQty || 0), 0)
  const totalReserved = filteredData.reduce((sum, item) => sum + (item.ReservedQty || 0), 0)
  const totalInTransit = filteredData.reduce((sum, item) => sum + (item.InTransitQty || 0), 0)
  const totalQty = filteredData.reduce((sum, item) => sum + (item.TotalQty || 0), 0)

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Warehouse Stock Summary</h1>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handleExport}
            disabled={exporting || filteredData.length === 0}
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
        </div>
      </div>

      {/* Filters and Selection */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <div className="grid gap-2">
              <Label htmlFor="org">Organization *</Label>
              <Select
                value={selectedOrgUID}
                onValueChange={setSelectedOrgUID}
              >
                <SelectTrigger id="org">
                  <SelectValue placeholder="Select organization" />
                </SelectTrigger>
                <SelectContent>
                  {orgLevels.flatMap(level => level.organizations).map(org => (
                    <SelectItem key={org.UID} value={org.UID}>
                      {org.Code ? `${org.Name} (${org.Code})` : org.Name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="warehouse">Warehouse *</Label>
              <Select
                value={selectedWarehouseUID}
                onValueChange={setSelectedWarehouseUID}
              >
                <SelectTrigger id="warehouse">
                  <SelectValue placeholder="Select warehouse" />
                </SelectTrigger>
                <SelectContent>
                  {orgLevels.flatMap(level => level.organizations)
                    .filter(org => org.OrgTypeUID?.includes('WAREHOUSE') || org.Code?.includes('WH'))
                    .map(warehouse => (
                      <SelectItem key={warehouse.UID} value={warehouse.UID}>
                        {warehouse.Code ? `${warehouse.Name} (${warehouse.Code})` : warehouse.Name}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex items-end">
              <Button
                onClick={fetchStockSummary}
                disabled={loading || !selectedOrgUID || !selectedWarehouseUID}
                className="w-full"
              >
                {loading ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Loading...
                  </>
                ) : (
                  <>
                    <RefreshCw className="h-4 w-4 mr-2" />
                    Load Stock Data
                  </>
                )}
              </Button>
            </div>
          </div>

          {stockData.length > 0 && (
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by SKU code or name... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Summary Cards */}
      {filteredData.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">Available Stock</p>
                  <h3 className="text-2xl font-bold text-green-600">{totalAvailable}</h3>
                </div>
                <Package className="h-8 w-8 text-green-600 opacity-50" />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">Reserved Stock</p>
                  <h3 className="text-2xl font-bold text-yellow-600">{totalReserved}</h3>
                </div>
                <Package className="h-8 w-8 text-yellow-600 opacity-50" />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">In Transit</p>
                  <h3 className="text-2xl font-bold text-blue-600">{totalInTransit}</h3>
                </div>
                <Package className="h-8 w-8 text-blue-600 opacity-50" />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">Total Stock</p>
                  <h3 className="text-2xl font-bold text-gray-900">{totalQty}</h3>
                </div>
                <Package className="h-8 w-8 text-gray-900 opacity-50" />
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Data Table */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          <DataTable
            columns={columns}
            data={filteredData.slice((currentPage - 1) * pageSize, currentPage * pageSize)}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>

        {filteredData.length > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={filteredData.length}
              pageSize={pageSize}
              onPageChange={(page) => {
                setCurrentPage(page);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }}
              onPageSizeChange={(size) => {
                setPageSize(size);
                setCurrentPage(1);
              }}
              itemName="items"
            />
          </div>
        )}
      </Card>
    </div>
  )
}
