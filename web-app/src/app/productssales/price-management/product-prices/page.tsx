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
} from '@/components/ui/dropdown-menu'
import { DataTable } from '@/components/ui/data-table'
import { PaginationControls } from '@/components/ui/pagination-controls'
import { useToast } from '@/components/ui/use-toast'
import { Plus, Edit, Trash2, Eye, Filter, Download, Upload, MoreHorizontal, Loader2, Search, ChevronDown, X } from 'lucide-react'
import { skuPriceService, ISKUPrice } from '@/services/sku/sku-price.service'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'

export default function ProductPricesPage() {
  const router = useRouter()
  const [prices, setPrices] = useState<ISKUPrice[]>([])
  const [loading, setLoading] = useState(true)
  const [exporting, setExporting] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [totalCount, setTotalCount] = useState(0)
  const [searchTerm, setSearchTerm] = useState('')
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('')
  const [filterPriceList, setFilterPriceList] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const searchInputRef = useRef<HTMLInputElement>(null)
  const { toast } = useToast()

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 500);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Reset to page 1 when filters change
  useEffect(() => {
    if (currentPage !== 1) {
      setCurrentPage(1);
    }
  }, [debouncedSearchTerm, filterPriceList, dateFrom, dateTo]);

  // Table columns configuration
  const columns = [
    {
      accessorKey: 'SKUCode',
      header: () => <div className="pl-6">SKU Code</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.getValue('SKUCode')}</span>
        </div>
      )
    },
    {
      accessorKey: 'SKUPriceListUID',
      header: 'Price List',
      cell: ({ row }: any) => (
        <Badge variant="secondary" className="text-xs">
          {row.getValue('SKUPriceListUID') || '-'}
        </Badge>
      )
    },
    {
      accessorKey: 'Price',
      header: () => <div className="text-right pr-4">Price</div>,
      cell: ({ row }: any) => {
        const price = row.getValue('Price') || 0
        return (
          <div className="text-right font-medium pr-4">
            {typeof price === 'number' ? price.toFixed(2) : price}
          </div>
        )
      }
    },
    {
      accessorKey: 'ValidFrom',
      header: () => <div className="pl-4">Valid From</div>,
      cell: ({ row }: any) => {
        const date = row.getValue('ValidFrom')
        return <div className="pl-4">{date ? formatDateToDayMonthYear(date) : '-'}</div>
      }
    },
    {
      accessorKey: 'ValidUpto',
      header: 'Valid To',
      cell: ({ row }: any) => {
        const date = row.getValue('ValidUpto')
        return date ? formatDateToDayMonthYear(date) : '-'
      }
    },
    {
      accessorKey: 'Status',
      header: () => <div className="text-center">Status</div>,
      cell: ({ row }: any) => {
        const status = row.getValue('Status') || (row.original.IsActive ? 'Active' : 'Inactive')
        return (
          <div className="text-center">
            <Badge variant={status === 'Active' ? 'default' : 'secondary'}>
              {status}
            </Badge>
          </div>
        )
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
                Edit Price
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => handleDelete(row.original.UID!)}
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
    fetchData()
  }, [currentPage, pageSize, debouncedSearchTerm, filterPriceList])

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

  const fetchData = async () => {
    try {
      setLoading(true)

      console.log('📊 Filter State:', {
        searchTerm,
        debouncedSearchTerm,
        filterPriceList,
        dateFrom,
        dateTo
      })

      // Build filter criteria
      const filterCriterias: any[] = []

      // SKU Code filter - use snake_case database column name
      if (debouncedSearchTerm) {
        console.log('✅ Adding SKU Code filter:', debouncedSearchTerm)
        filterCriterias.push({
          name: 'sku_code',
          value: debouncedSearchTerm,
          type: 2,
          filterMode: 0
        })
      }

      // Price List filter - use database alias (all lowercase, no underscores)
      if (filterPriceList) {
        console.log('✅ Adding Price List filter:', filterPriceList)
        filterCriterias.push({
          name: 'skupricelistuid',
          value: filterPriceList,
          type: 2,
          filterMode: 0
        })
      }

      console.log('🔍 Filter Criteria:', JSON.stringify(filterCriterias, null, 2))

      const response = await skuPriceService.getAllSKUPrices({
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: filterCriterias,
        SortCriterias: [],
        IsCountRequired: currentPage === 1
      })

      // Handle both PascalCase and camelCase
      let pagedData = response.PagedData || response.pagedData || []
      const count = response.TotalCount ?? response.totalCount ?? 0

      // Apply client-side date filtering (backend doesn't support it)
      if (dateFrom || dateTo) {
        pagedData = pagedData.filter(price => {
          const validFrom = price.ValidFrom ? new Date(price.ValidFrom) : null
          const validTo = price.ValidUpto ? new Date(price.ValidUpto) : null

          if (dateFrom && validFrom) {
            const from = new Date(dateFrom)
            if (validFrom < from) return false
          }

          if (dateTo && validTo) {
            const to = new Date(dateTo)
            if (validTo > to) return false
          }

          return true
        })
      }

      setPrices(pagedData)

      // Update total count
      if (count === -1 && totalCount > 0) {
        // Keep existing count if API returns -1
      } else if (count > 0) {
        setTotalCount(count)
      } else if (currentPage === 1) {
        // Only reset to 0 on first page
        setTotalCount(0)
      }

    } catch (error: any) {
      console.error('Error fetching data:', error)

      // Don't show error toast for "not found" scenarios
      if (error?.response?.status !== 404) {
        toast({
          title: 'Error',
          description: 'Failed to fetch prices',
          variant: 'destructive'
        })
      }

      // Set empty data on error
      setPrices([])
      setTotalCount(0)
    } finally {
      setLoading(false)
    }
  }

  const handleView = (item: ISKUPrice) => {
    router.push(`/productssales/price-management/product-prices/view/${item.UID}`)
  }

  const handleEdit = (item: ISKUPrice) => {
    router.push(`/productssales/price-management/product-prices/edit/${item.UID}`)
  }

  const handleDelete = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this product price?')) return

    try {
      await skuPriceService.deleteSKUPrice(uid)
      toast({
        title: 'Success',
        description: 'Product price deleted successfully',
      })
      fetchData()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete product price',
        variant: 'destructive'
      })
    }
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      console.log("Starting export of product prices...")

      if (prices.length === 0) {
        toast({
          title: "No Data",
          description: "No product prices found to export.",
          variant: "default",
        })
        return
      }

      // Create CSV export
      const headers = ["SKU Code", "Product Name", "Price List", "UOM", "Price", "MRP", "Status", "Valid From", "Valid To"]

      const exportData = prices.map(price => [
        price.SKUCode || "",
        price.SKUName || "",
        price.SKUPriceListUID || "",
        price.UOM || "",
        price.Price?.toString() || "0",
        price.MRP?.toString() || "0",
        price.IsActive ? "Active" : "Inactive",
        price.ValidFrom ? new Date(price.ValidFrom).toLocaleDateString() : "",
        price.ValidUpto ? new Date(price.ValidUpto).toLocaleDateString() : ""
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
      a.download = `product-prices-export-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)

      toast({
        title: "Export Complete",
        description: `${exportData.length} product prices exported successfully.`,
      })
    } catch (error) {
      console.error("Export error:", error)
      toast({
        title: "Export Failed",
        description: "Failed to export product prices. Please try again.",
        variant: "destructive",
      })
    } finally {
      setExporting(false)
    }
  }

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Product Prices</h1>
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
          <Button onClick={() => router.push('/productssales/price-management/product-prices/create')} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Price
          </Button>
        </div>
      </div>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3">
          <div className="flex gap-3 flex-wrap">
            <div className="relative flex-1 min-w-[200px]">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by SKU code... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Price List Search */}
            <div className="relative">
              <Input
                placeholder="Search by Price List..."
                value={filterPriceList}
                onChange={(e) => setFilterPriceList(e.target.value)}
                className="w-56 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Date From */}
            <div className="flex items-center gap-2">
              <Input
                type="date"
                placeholder="Valid From"
                value={dateFrom}
                onChange={(e) => setDateFrom(e.target.value)}
                className="w-40"
              />
            </div>

            {/* Date To */}
            <div className="flex items-center gap-2">
              <Input
                type="date"
                placeholder="Valid To"
                value={dateTo}
                onChange={(e) => setDateTo(e.target.value)}
                className="w-40"
              />
            </div>

            {/* Clear Filters */}
            {(searchTerm || filterPriceList || dateFrom || dateTo) && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setSearchTerm('')
                  setFilterPriceList('')
                  setDateFrom('')
                  setDateTo('')
                }}
              >
                <X className="h-4 w-4 mr-2" />
                Clear All
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          <DataTable
            columns={columns}
            data={prices}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>

        {prices.length > 0 && (
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
              itemName="prices"
            />
          </div>
        )}
      </Card>
    </div>
  )
}