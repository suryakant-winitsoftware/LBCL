'use client'

import React, { useState, useEffect, useRef } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { 
  ArrowLeft,
  Search, 
  Download,
  Package,
  Edit,
  Plus,
  Trash2,
  MoreHorizontal,
  RefreshCw,
  Filter,
  ChevronDown,
  X
} from 'lucide-react'
import { skuPriceService, ISKUPrice, ISKUPriceList, SKUPriceViewDTO } from '@/services/sku/sku-price.service'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'
import { PagingRequest } from '@/types/common.types'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Badge } from '@/components/ui/badge'
import { useToast } from '@/components/ui/use-toast'
import { PaginationControls } from '@/components/ui/pagination-controls'
import { Skeleton } from '@/components/ui/skeleton'
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

export default function ManagePriceListPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { toast } = useToast()
  const searchInputRef = useRef<HTMLInputElement>(null)
  const priceListUID = searchParams.get('uid')
  
  // State
  const [priceList, setPriceList] = useState<ISKUPriceList | null>(null)
  const [products, setProducts] = useState<ISKUPrice[]>([])
  const [loading, setLoading] = useState(false)
  const [loadingPriceList, setLoadingPriceList] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [pageSize, setPageSize] = useState(20)
  
  // Filter states
  const [selectedStatus, setSelectedStatus] = useState<string[]>([])
  const [validFromFilter, setValidFromFilter] = useState('')
  const [validToFilter, setValidToFilter] = useState('')
  
  // Delete dialog state
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [productToDelete, setProductToDelete] = useState<string | null>(null)
  
  // Inline editing state
  const [editingProducts, setEditingProducts] = useState<Set<string>>(new Set())
  const [tempProductData, setTempProductData] = useState<Record<string, Partial<ISKUPrice>>>({})
  const [newRowCounter, setNewRowCounter] = useState(0)
  
  // Generate UID function (same as TPPPriceTab)
  const generateUID = () => {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
      /[xy]/g,
      function (c) {
        const r = (Math.random() * 16) | 0;
        const v = c === "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      }
    );
  };
  
  // Fetch price list details
  const fetchPriceListDetails = async () => {
    if (!priceListUID) return
    
    setLoadingPriceList(true)
    try {
      const response = await skuPriceService.getPriceListByUID(priceListUID)
      setPriceList(response)
    } catch (error) {
      console.error('Error fetching price list details:', error)
      toast({
        title: 'Error',
        description: 'Failed to fetch price list details',
        variant: 'destructive'
      })
    } finally {
      setLoadingPriceList(false)
    }
  }
  
  // Fetch products
  const fetchProducts = async () => {
    if (!priceListUID) return
    
    setLoading(true)
    try {
      // Build filter criteria array
      const filterCriterias: any[] = []
      
      // Add search term filter - search by SKU code OR name
      // Use a concatenated field to search both at once
      if (searchTerm) {
        // This will search in a concatenated string of SKU code and name
        // Using CONCAT with ILIKE for case-insensitive search
        filterCriterias.push({
          Name: "CONCAT(LOWER(sp.sku_code), ' ', LOWER(s.name))",
          Value: searchTerm.toLowerCase(),
          Type: 2, // Like (partial match)
          FilterMode: 0 // And
        })
      }
      
      // Add status filters
      if (selectedStatus.length > 0) {
        selectedStatus.forEach(status => {
          filterCriterias.push({ Name: 'Status', Value: status, Type: 1 })
        })
      }
      
      // Add Valid From date filter
      if (validFromFilter) {
        filterCriterias.push({ Name: 'ValidFrom', Value: validFromFilter, Type: 3 })
      }
      
      // Add Valid To date filter  
      if (validToFilter) {
        filterCriterias.push({ Name: 'ValidUpto', Value: validToFilter, Type: 3 })
      }
      
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: filterCriterias,
        SortCriterias: [
          {
            SortParameter: 'SKUCode',
            Direction: 'Asc' as const
          }
        ],
        IsCountRequired: true
      }
      
      // Use getSKUPriceView to get products for this specific price list
      const response = await skuPriceService.getSKUPriceView(request, priceListUID)
      
      if (response.PagedData && response.PagedData.length > 0) {
        // The response contains SKUPriceViewDTO with SKUPriceList property
        const products = response.PagedData[0].SKUPriceList || []
        setProducts(products)
        // Use the TotalCount from the response for proper pagination
        setTotalCount(response.TotalCount || products.length)
      } else {
        setProducts([])
        setTotalCount(0)
      }
    } catch (error) {
      console.error('Error fetching products:', error)
      toast({
        title: 'Error',
        description: 'Failed to fetch products',
        variant: 'destructive'
      })
    } finally {
      setLoading(false)
    }
  }
  
  useEffect(() => {
    if (priceListUID) {
      fetchPriceListDetails()
    }
  }, [priceListUID])

  useEffect(() => {
    if (priceListUID) {
      fetchProducts()
    }
  }, [priceListUID, currentPage, pageSize, searchTerm, selectedStatus, validFromFilter, validToFilter])
  
  // Add keyboard shortcut for Ctrl+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault()
        searchInputRef.current?.focus()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])
  
  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1)
  }
  
  const formatDate = (date: Date | string | undefined) => {
    return formatDateToDayMonthYear(date, '-')
  }
  
  // Inline editing functions
  const startEditing = (productUID: string, product: ISKUPrice) => {
    setEditingProducts(prev => new Set([...prev, productUID]))
    setTempProductData(prev => ({
      ...prev,
      [productUID]: {
        SKUCode: product.SKUCode,
        SKUName: product.SKUName,
        UOM: product.UOM,
        Price: product.Price,
        MRP: product.MRP,
        ValidFrom: product.ValidFrom?.toString().slice(0, 10),
        ValidUpto: product.ValidUpto?.toString().slice(0, 10),
        Status: product.Status
      }
    }))
  }

  const cancelEditing = (productUID: string) => {
    setEditingProducts(prev => {
      const newSet = new Set(prev)
      newSet.delete(productUID)
      return newSet
    })
    
    // If it's a new product, remove it from the list
    if (productUID.startsWith('new-')) {
      setProducts(prev => prev.filter(p => p.UID !== productUID))
    }
    
    // Clear temp data
    setTempProductData(prev => {
      const newData = { ...prev }
      delete newData[productUID]
      return newData
    })
  }

  const saveEdit = async (product: ISKUPrice) => {
    if (!product.UID) return

    const tempData = tempProductData[product.UID] || {}
    
    // Validation
    if (!tempData.SKUCode?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'SKU Code is required',
        variant: 'destructive'
      })
      return
    }

    try {
      if (product.UID.startsWith('new-')) {
        // Create new product
        // Validation like TPPPriceTab
        if (!tempData.SKUCode?.trim() || !tempData.Price || !tempData.ValidFrom || !tempData.ValidUpto) {
          toast({
            title: 'Validation Error',
            description: 'Please fill in all required fields (SKU Code, Price, Valid From, Valid To)',
            variant: 'destructive'
          })
          return
        }
        
        const newProduct = {
          UID: generateUID(), // Generate UID like TPPPriceTab
          SKUCode: tempData.SKUCode!,
          SKUName: tempData.SKUName || "",
          SKUUID: product.SKUUID || "",
          SKUPriceListUID: priceListUID,
          UOM: tempData.UOM || product.UOM || "PCS",
          Price: tempData.Price!,
          DefaultWSPrice: tempData.Price! || 0,
          DefaultRetPrice: tempData.Price! || 0,
          DummyPrice: tempData.Price!,
          MRP: tempData.MRP || 0,
          PriceUpperLimit: 0,
          PriceLowerLimit: 0,
          ValidFrom: new Date(tempData.ValidFrom!),
          ValidUpto: new Date(tempData.ValidUpto!),
          Status: tempData.Status || "Active",
          IsActive: true,
          IsTaxIncluded: false,
          CreatedBy: "ADMIN",
          CreatedTime: new Date().toISOString(),
          ModifiedBy: "ADMIN",
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          IsLatest: 1,
          VersionNo: "1.0",
          LadderingAmount: 0,
          LadderingPercentage: 0,
        } as ISKUPrice
        
        await skuPriceService.createSKUPrice(newProduct)
        toast({
          title: 'Success',
          description: 'Product price created successfully'
        })
        
        // Remove new row and refresh like TPPPriceTab
        setProducts(prev => prev.filter(p => p.UID !== product.UID))
        setEditingProducts(prev => {
          const newSet = new Set(prev)
          newSet.delete(product.UID!)
          return newSet
        })
        
        // Refresh products like TPPPriceTab
        fetchProducts()
      } else {
        // Use TPPPriceTab approach - spread all original fields and override specific ones
        const updatedProduct: ISKUPrice = {
          ...product,
          Price: tempData.Price ?? product.Price,
          MRP: tempData.MRP ?? product.MRP,
          ValidFrom: tempData.ValidFrom ? `${tempData.ValidFrom}T00:00:00` : product.ValidFrom,
          ValidUpto: tempData.ValidUpto ? `${tempData.ValidUpto}T00:00:00` : product.ValidUpto,
          Status: tempData.Status ?? product.Status,
        }

        console.log('Manage page - Updated product being sent:', updatedProduct)
        
        await skuPriceService.updateSKUPrice(updatedProduct)
        toast({
          title: 'Success',
          description: 'Product price updated successfully'
        })
      }
      
      // Refresh products and clear editing state
      await fetchProducts()
      cancelEditing(product.UID)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save product price',
        variant: 'destructive'
      })
    }
  }

  const updateTempData = (productUID: string, field: string, value: any) => {
    setTempProductData(prev => ({
      ...prev,
      [productUID]: {
        ...prev[productUID],
        [field]: value
      }
    }))
  }

  const addNewProduct = () => {
    const newProductId = `new-${newRowCounter}`
    setNewRowCounter(prev => prev + 1)
    
    const newProduct: ISKUPrice = {
      UID: newProductId,
      SKUCode: '',
      SKUName: '',
      UOM: 'PCS',
      Price: 0,
      MRP: 0,
      Status: 'Active',
      ValidFrom: new Date().toISOString(),
      ValidUpto: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString(),
      IsActive: true,
      SKUUID: '',
      SKUPriceListUID: priceListUID || ''
    }
    
    // Add to the beginning of the list
    setProducts(prev => [newProduct, ...prev])
    
    // Start editing the new row
    startEditing(newProductId, newProduct)
  }
  
  const handleDeleteProduct = (productUID: string) => {
    setProductToDelete(productUID)
    setDeleteDialogOpen(true)
  }
  
  const confirmDeleteProduct = async () => {
    if (!productToDelete) return
    
    try {
      await skuPriceService.deleteSKUPrice(productToDelete)
      toast({
        title: 'Success',
        description: 'Product price deleted successfully'
      })
      
      // Refresh the products list
      fetchProducts()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete product price',
        variant: 'destructive'
      })
    } finally {
      setDeleteDialogOpen(false)
      setProductToDelete(null)
    }
  }
  
  
  const exportProducts = async () => {
    try {
      // For now, we'll export the current products
      const csvContent = [
        ['SKU Code', 'Product Name', 'UOM', 'Price', 'MRP', 'Valid From', 'Valid To', 'Status'].join(','),
        ...products.map(p => [
          p.SKUCode,
          p.SKUName || '',
          p.UOM,
          p.Price,
          p.MRP,
          formatDate(p.ValidFrom),
          formatDate(p.ValidUpto),
          p.Status
        ].join(','))
      ].join('\n')
      
      const blob = new Blob([csvContent], { type: 'text/csv' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `price-list-${priceListUID}-products.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
      
      toast({
        title: 'Success',
        description: 'Products exported successfully'
      })
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to export products',
        variant: 'destructive'
      })
    }
  }
  
  if (!priceListUID) {
    return (
      <div className="container mx-auto py-6">
        <Card className="p-8 text-center">
          <p className="text-muted-foreground">No price list selected</p>
          <Button
            variant="outline"
            className="mt-4"
            onClick={() => router.push('/productssales/price-management/lists')}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Price Lists
          </Button>
        </Card>
      </div>
    )
  }
  
  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Back Navigation */}
      <div className="mb-6">
        <Button
          variant="outline"
          size="default"
          onClick={() => router.push('/productssales/price-management/lists')}
          className="group hover:bg-gray-50 transition-colors"
        >
          <ArrowLeft className="h-4 w-4 mr-2 transition-transform group-hover:-translate-x-1" />
          Back to Price Lists
        </Button>
      </div>

      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Manage Price List Products
          </h1>
          {loadingPriceList ? (
            <Skeleton className="h-4 w-48 mt-1" />
          ) : priceList ? (
            <p className="text-sm text-muted-foreground mt-1">
              {priceList.Name} ({priceList.Code})
            </p>
          ) : null}
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={exportProducts}>
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={addNewProduct}>
            <Plus className="h-4 w-4 mr-2" />
            Add Product
          </Button>
        </div>
      </div>
      
      {/* Price List Info Card */}
      {priceList && (
        <Card className="p-4 bg-muted/30 border-0">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <p className="text-xs text-muted-foreground">Type</p>
              <p className="text-sm font-medium">{priceList.Type || 'Standard'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Priority</p>
              <p className="text-sm font-medium">{priceList.Priority}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Valid From</p>
              <p className="text-sm font-medium">{formatDate(priceList.ValidFrom)}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Status</p>
              <Badge variant={priceList.Status === 'Published' ? 'default' : 'secondary'}>
                {priceList.Status}
              </Badge>
            </div>
          </div>
        </Card>
      )}
      
      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <div className="p-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by SKU code or name... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Valid From Date Filter */}
            <div className="flex items-center gap-2">
              <Input
                type="date"
                value={validFromFilter}
                onChange={(e) => {
                  setValidFromFilter(e.target.value)
                  setCurrentPage(1)
                }}
                className="w-40"
                placeholder="Valid From"
              />
              {validFromFilter && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setValidFromFilter('')
                    setCurrentPage(1)
                  }}
                  className="h-8 w-8 p-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </div>

            {/* Valid To Date Filter */}
            <div className="flex items-center gap-2">
              <Input
                type="date"
                value={validToFilter}
                onChange={(e) => {
                  setValidToFilter(e.target.value)
                  setCurrentPage(1)
                }}
                className="w-40"
                placeholder="Valid To"
              />
              {validToFilter && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setValidToFilter('')
                    setCurrentPage(1)
                  }}
                  className="h-8 w-8 p-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </div>

            {/* Status Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {selectedStatus.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedStatus.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={selectedStatus.includes("Active")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev => 
                      checked 
                        ? [...prev, "Active"]
                        : prev.filter(s => s !== "Active")
                    )
                    setCurrentPage(1)
                  }}
                >
                  Active
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedStatus.includes("Inactive")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev => 
                      checked 
                        ? [...prev, "Inactive"]
                        : prev.filter(s => s !== "Inactive")
                    )
                    setCurrentPage(1)
                  }}
                >
                  Inactive
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedStatus.includes("Draft")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev => 
                      checked 
                        ? [...prev, "Draft"]
                        : prev.filter(s => s !== "Draft")
                    )
                    setCurrentPage(1)
                  }}
                >
                  Draft
                </DropdownMenuCheckboxItem>
                {selectedStatus.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => {
                        setSelectedStatus([])
                        setCurrentPage(1)
                      }}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
            
            <Button
              variant="outline"
              size="default"
              onClick={() => fetchProducts()}
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </div>
      </Card>
      
      {/* Products Table */}
      <Card className="shadow-sm border-gray-200">
        <div className="p-0">
          <div className="overflow-hidden rounded-lg">
            <Table>
              <TableHeader>
                <TableRow className="bg-gray-50/50">
                  <TableHead className="font-medium pl-6">SKU Code</TableHead>
                <TableHead className="font-medium">Product Name</TableHead>
                <TableHead className="font-medium">UOM</TableHead>
                <TableHead className="font-medium">Price List</TableHead>
                <TableHead className="text-right font-medium">Price</TableHead>
                <TableHead className="text-right font-medium">MRP</TableHead>
                <TableHead className="font-medium">Valid From</TableHead>
                <TableHead className="font-medium">Valid To</TableHead>
                <TableHead className="font-medium">Status</TableHead>
                <TableHead className="text-right font-medium pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <>
                  {[...Array(10)].map((_, index) => (
                    <TableRow key={`skeleton-${index}`}>
                      <TableCell className="pl-6"><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-28" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-6 w-16 rounded-full" /></TableCell>
                      <TableCell className="pr-6"><Skeleton className="h-8 w-8 rounded ml-auto" /></TableCell>
                    </TableRow>
                  ))}
                </>
              ) : products.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={10} className="text-center py-12">
                    <div className="flex flex-col items-center space-y-4">
                      <Package className="h-12 w-12 text-muted-foreground/50" />
                      <div className="space-y-2">
                        <p className="text-sm font-medium text-muted-foreground">No products found</p>
                        <p className="text-xs text-muted-foreground">
                          {searchTerm || selectedStatus.length > 0 || validFromFilter || validToFilter ? 'Try adjusting your filters' : 'Add your first product'}
                        </p>
                      </div>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={addNewProduct}
                      >
                        <Plus className="h-4 w-4 mr-2" />
                        Add Product
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                products.map((product) => {
                  const isEditing = editingProducts.has(product.UID!)
                  const tempData = tempProductData[product.UID!] || {}
                  const isNew = product.UID!.startsWith('new-')
                  
                  return (
                    <TableRow 
                      key={product.UID} 
                      className={`hover:bg-muted/30 ${isEditing ? (isNew ? 'bg-green-50' : 'bg-blue-50') : ''}`}
                    >
                      {/* SKU Code - Editable for new products */}
                      <TableCell className="font-medium pl-6">
                        {isEditing && isNew ? (
                          <Input
                            value={tempData.SKUCode ?? product.SKUCode}
                            onChange={(e) => updateTempData(product.UID!, 'SKUCode', e.target.value)}
                            placeholder="SKU Code"
                            className="w-32 h-8"
                          />
                        ) : (
                          product.SKUCode
                        )}
                      </TableCell>
                      
                      {/* SKU Name - Editable for new products */}
                      <TableCell>
                        {isEditing && isNew ? (
                          <Input
                            value={tempData.SKUName ?? product.SKUName}
                            onChange={(e) => updateTempData(product.UID!, 'SKUName', e.target.value)}
                            placeholder="Product Name"
                            className="w-40 h-8"
                          />
                        ) : (
                          product.SKUName || '-'
                        )}
                      </TableCell>
                      
                      {/* UOM - Editable for new products */}
                      <TableCell>
                        {isEditing && isNew ? (
                          <Select
                            value={tempData.UOM ?? product.UOM}
                            onValueChange={(value) => updateTempData(product.UID!, 'UOM', value)}
                          >
                            <SelectTrigger className="w-20 h-8">
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="PCS">PCS</SelectItem>
                              <SelectItem value="KG">KG</SelectItem>
                              <SelectItem value="LTR">LTR</SelectItem>
                              <SelectItem value="MTR">MTR</SelectItem>
                              <SelectItem value="BOX">BOX</SelectItem>
                              <SelectItem value="CTN">CTN</SelectItem>
                            </SelectContent>
                          </Select>
                        ) : (
                          <span className="px-2 py-0.5 bg-muted rounded text-xs">
                            {product.UOM}
                          </span>
                        )}
                      </TableCell>
                      
                      {/* Price List - Show as badge */}
                      <TableCell>
                        <Badge variant="outline" className="text-xs">
                          {priceList?.Name || priceList?.Code || 'Default'}
                        </Badge>
                      </TableCell>
                      
                      {/* Price - Always editable */}
                      <TableCell className="text-right font-medium">
                        {isEditing ? (
                          <Input
                            type="number"
                            step="0.01"
                            value={tempData.Price ?? product.Price}
                            onChange={(e) => updateTempData(product.UID!, 'Price', parseFloat(e.target.value) || 0)}
                            className="w-24 h-8 text-right"
                          />
                        ) : (
                          product.Price?.toFixed(2)
                        )}
                      </TableCell>
                      
                      {/* MRP - Always editable */}
                      <TableCell className="text-right text-muted-foreground">
                        {isEditing ? (
                          <Input
                            type="number"
                            step="0.01"
                            value={tempData.MRP ?? product.MRP}
                            onChange={(e) => updateTempData(product.UID!, 'MRP', parseFloat(e.target.value) || 0)}
                            className="w-24 h-8 text-right"
                          />
                        ) : (
                          product.MRP?.toFixed(2)
                        )}
                      </TableCell>
                      
                      {/* Valid From - Always editable */}
                      <TableCell className="text-sm text-muted-foreground">
                        {isEditing ? (
                          <Input
                            type="date"
                            value={tempData.ValidFrom ?? product.ValidFrom?.toString().slice(0, 10)}
                            onChange={(e) => updateTempData(product.UID!, 'ValidFrom', e.target.value)}
                            className="w-32 h-8"
                          />
                        ) : (
                          formatDate(product.ValidFrom)
                        )}
                      </TableCell>
                      
                      {/* Valid To - Always editable */}
                      <TableCell className="text-sm text-muted-foreground">
                        {isEditing ? (
                          <Input
                            type="date"
                            value={tempData.ValidUpto ?? product.ValidUpto?.toString().slice(0, 10)}
                            onChange={(e) => updateTempData(product.UID!, 'ValidUpto', e.target.value)}
                            className="w-32 h-8"
                          />
                        ) : (
                          formatDate(product.ValidUpto)
                        )}
                      </TableCell>
                      
                      {/* Status - Always editable */}
                      <TableCell>
                        {isEditing ? (
                          <Select
                            value={tempData.Status ?? product.Status}
                            onValueChange={(value) => updateTempData(product.UID!, 'Status', value)}
                          >
                            <SelectTrigger className="w-24 h-8">
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="Active">Active</SelectItem>
                              <SelectItem value="Inactive">Inactive</SelectItem>
                              <SelectItem value="Draft">Draft</SelectItem>
                            </SelectContent>
                          </Select>
                        ) : (
                          <Badge 
                            variant={product.Status === 'Active' ? 'default' : 'secondary'}
                            className="text-xs"
                          >
                            {product.Status}
                          </Badge>
                        )}
                      </TableCell>
                      
                      {/* Actions */}
                      <TableCell className="text-right pr-6">
                        {isEditing ? (
                          <div className="flex gap-1 justify-end">
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0 text-green-600 hover:text-green-700"
                              onClick={() => saveEdit(product)}
                            >
                              ✓
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0 text-red-600 hover:text-red-700"
                              onClick={() => cancelEditing(product.UID!)}
                            >
                              ✕
                            </Button>
                          </div>
                        ) : (
                          <div className="flex gap-1 justify-end">
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0"
                              onClick={() => startEditing(product.UID!, product)}
                              title="Edit product"
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0"
                              onClick={() => {
                                // Smart add - copy this product's data with new dates
                                const newProductId = `new-${newRowCounter}`
                                setNewRowCounter(prev => prev + 1)
                                
                                // Calculate the next day after current product's ValidUpto (Valid To)
                                let currentEndDate: Date
                                
                                console.log('Manage page - Current ValidUpto raw:', product.ValidUpto, typeof product.ValidUpto)
                                
                                if (product.ValidUpto) {
                                  // Handle different date formats
                                  if (typeof product.ValidUpto === 'string') {
                                    // Handle ISO format with time
                                    if (product.ValidUpto.includes('T')) {
                                      // Extract just the date part to avoid timezone issues
                                      const dateOnly = product.ValidUpto.split('T')[0]
                                      currentEndDate = new Date(dateOnly + 'T12:00:00') // Use noon to avoid timezone issues
                                    }
                                    // If it's already in YYYY-MM-DD format
                                    else if (product.ValidUpto.match(/^\d{4}-\d{2}-\d{2}$/)) {
                                      currentEndDate = new Date(product.ValidUpto + 'T12:00:00')
                                    } else {
                                      currentEndDate = new Date(product.ValidUpto)
                                    }
                                  } else if (product.ValidUpto instanceof Date) {
                                    currentEndDate = new Date(product.ValidUpto)
                                  } else {
                                    // Fallback to string conversion
                                    currentEndDate = new Date(String(product.ValidUpto))
                                  }
                                  
                                  // Check if date is valid
                                  if (isNaN(currentEndDate.getTime())) {
                                    console.error('Manage page - Invalid date from ValidUpto:', product.ValidUpto)
                                    currentEndDate = new Date() // Use current date as fallback
                                  }
                                } else {
                                  currentEndDate = new Date()
                                }
                                
                                // Add 1 day to get the next start date
                                const nextDay = new Date(currentEndDate)
                                nextDay.setDate(nextDay.getDate() + 1)
                                
                                // Debug logging
                                console.log('Manage page - Parsed end date:', currentEndDate.toISOString().slice(0, 10))
                                console.log('Manage page - Next start day:', nextDay.toISOString().slice(0, 10))
                                
                                // Set end date to Dec 31 of next year from the new start date
                                const nextYearEnd = new Date(nextDay.getFullYear() + 1, 11, 31)
                                
                                const newProduct: ISKUPrice = {
                                  UID: newProductId,
                                  SKUCode: product.SKUCode,
                                  SKUName: product.SKUName,
                                  UOM: product.UOM,
                                  Price: product.Price,
                                  MRP: product.MRP,
                                  Status: 'Active',
                                  ValidFrom: nextDay.toISOString().slice(0, 10), // Format as YYYY-MM-DD
                                  ValidUpto: nextYearEnd.toISOString().slice(0, 10), // Format as YYYY-MM-DD
                                  IsActive: true,
                                  SKUUID: product.SKUUID,
                                  SKUPriceListUID: priceListUID || ''
                                }
                                
                                // Add to the list right below the source product
                                setProducts(prev => {
                                  const sourceIndex = prev.findIndex(p => p.UID === product.UID)
                                  
                                  if (sourceIndex !== -1) {
                                    // Insert new product right after the source product
                                    const updatedProducts = [
                                      ...prev.slice(0, sourceIndex + 1),
                                      newProduct,
                                      ...prev.slice(sourceIndex + 1)
                                    ]
                                    return updatedProducts
                                  } else {
                                    // Fallback: add to the beginning if source not found
                                    return [newProduct, ...prev]
                                  }
                                })
                                
                                // Start editing the new row with proper temp data
                                setEditingProducts(prev => new Set([...prev, newProductId]))
                                setTempProductData(prev => ({
                                  ...prev,
                                  [newProductId]: {
                                    SKUCode: newProduct.SKUCode,
                                    SKUName: newProduct.SKUName,
                                    UOM: newProduct.UOM,
                                    Price: newProduct.Price,
                                    MRP: newProduct.MRP,
                                    ValidFrom: newProduct.ValidFrom,
                                    ValidUpto: newProduct.ValidUpto,
                                    Status: newProduct.Status
                                  }
                                }))
                              }}
                              title="Add new price period for this SKU"
                            >
                              <Plus className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0 text-red-600 hover:text-red-700"
                              onClick={() => handleDeleteProduct(product.UID!)}
                              title="Delete product"
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        )}
                      </TableCell>
                    </TableRow>
                  )
                })
              )}
              {/* Add Product Row */}
              {!loading && products.length > 0 && (
                <TableRow className="hover:bg-muted/10">
                  <TableCell colSpan={10} className="text-center py-3">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={addNewProduct}
                      className="text-primary hover:text-primary/80"
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Add Product
                    </Button>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
          </div>
        </div>
        
        {totalCount > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={totalCount}
              pageSize={pageSize}
              onPageChange={setCurrentPage}
              onPageSizeChange={setPageSize}
              itemName="products"
            />
          </div>
        )}
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the product price.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction 
              onClick={confirmDeleteProduct} 
              className="bg-red-600 hover:bg-red-700"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>


    </div>
  )
}