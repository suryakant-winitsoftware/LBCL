'use client'

import React, { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { 
  Search, 
  Download, 
  Upload, 
  Plus,
  Edit,
  Trash2,
  ChevronDown,
  ChevronRight,
  Package,
  Eye,
  MoreHorizontal,
  Filter,
  RefreshCw,
  X
} from 'lucide-react'
import { skuPriceService, ISKUPriceList, ISKUPrice } from '@/services/sku/sku-price.service'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'
import { PagingRequest } from '@/types/common.types'
import { uomTypesService, UOMType } from '@/services/sku/uom-types.service'
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { PaginationControls } from '@/components/ui/pagination-controls'
import { Skeleton } from '@/components/ui/skeleton'

export default function ProductPricesPage() {
  const router = useRouter()
  const { toast } = useToast()
  const searchInputRef = useRef<HTMLInputElement>(null)
  
  // State for Price Lists
  const [priceLists, setPriceLists] = useState<ISKUPriceList[]>([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [itemToDelete, setItemToDelete] = useState<string | null>(null)
  const [pageSize, setPageSize] = useState(10)
  
  // Filter states
  const [validFromFilter, setValidFromFilter] = useState('')
  const [validToFilter, setValidToFilter] = useState('')
  const [selectedStatus, setSelectedStatus] = useState<string[]>([])
  
  // State for expanded rows and products
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set())
  const [priceListProducts, setPriceListProducts] = useState<Record<string, ISKUPrice[]>>({})
  const [loadingProducts, setLoadingProducts] = useState<Record<string, boolean>>({})
  
  // State for inline editing
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
  const [deleteProductDialog, setDeleteProductDialog] = useState(false)
  const [productToDelete, setProductToDelete] = useState<ISKUPrice | null>(null)
  
  // UOM Types state
  const [uomTypes, setUomTypes] = useState<UOMType[]>([])
  const [loadingUOM, setLoadingUOM] = useState(false)


  // Fetch Price Lists
  const fetchPriceLists = async () => {
    setLoading(true)
    try {
      // Build filter criteria array
      const filterCriterias: any[] = []
      
      // Add search term filters - search by name or code
      // Use a smart approach: detect if it looks like a code, otherwise search by name
      if (searchTerm) {
        // Check if it looks like a price list code (usually has numbers/uppercase)
        const looksLikeCode = /^[A-Z0-9_-]{3,}$/i.test(searchTerm);

        if (looksLikeCode) {
          // Search by Code
          filterCriterias.push({ Name: 'Code', Value: searchTerm, Type: 2, FilterMode: 0 });
        } else {
          // Search by Name
          filterCriterias.push({ Name: 'Name', Value: searchTerm, Type: 2, FilterMode: 0 });
        }
      }
      
      // Add Valid From date filter
      if (validFromFilter) {
        filterCriterias.push({ Name: 'ValidFrom', Value: validFromFilter, Type: 3 }) // Type 3 for date comparison
      }
      
      // Add Valid To date filter  
      if (validToFilter) {
        filterCriterias.push({ Name: 'ValidUpto', Value: validToFilter, Type: 3 })
      }
      
      // Add Status filters
      if (selectedStatus.length > 0) {
        selectedStatus.forEach(status => {
          filterCriterias.push({ Name: 'Status', Value: status, Type: 1 })
        })
      }
      
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: filterCriterias,
        SortCriterias: [],
        IsCountRequired: true
      }
      
      const response = await skuPriceService.getAllPriceLists(request)
      const priceListsData = response.PagedData || []
      setPriceLists(priceListsData)
      setTotalCount(response.TotalCount || 0)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch price lists',
        variant: 'destructive'
      })
      console.error('Error fetching price lists:', error)
    } finally {
      setLoading(false)
    }
  }

  // Load UOM types and filter options on component mount
  useEffect(() => {
    const loadUOMTypes = async () => {
      try {
        setLoadingUOM(true)
        const types = await uomTypesService.getAllUOMTypes()
        setUomTypes(types)
        console.log(`Loaded ${types.length} UOM types for dropdowns`)
      } catch (error) {
        console.error('Failed to load UOM types:', error)
        toast({
          title: 'Error',
          description: 'Failed to load Units of Measurement',
          variant: 'destructive'
        })
      } finally {
        setLoadingUOM(false)
      }
    }
    
    loadUOMTypes()
  }, [])

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
  
  // Combined effect to handle fetching with debounced search and filters
  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      fetchPriceLists()
    }, searchTerm ? 500 : 0)
    
    return () => clearTimeout(debounceTimer)
  }, [searchTerm, currentPage, pageSize, validFromFilter, validToFilter, selectedStatus])

  const handleDelete = async () => {
    if (!itemToDelete) return

    try {
      await skuPriceService.deletePriceList(itemToDelete)
      toast({
        title: 'Success',
        description: 'Price list deleted successfully'
      })
      fetchPriceLists()
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete price list',
        variant: 'destructive'
      })
    } finally {
      setDeleteDialogOpen(false)
      setItemToDelete(null)
    }
  }

  const confirmDelete = (uid: string) => {
    setItemToDelete(uid)
    setDeleteDialogOpen(true)
  }

  const formatDate = (date: Date | string | undefined) => {
    return formatDateToDayMonthYear(date, '-')
  }
  
  // Toggle row expansion and fetch products
  const toggleRowExpansion = async (priceListUID: string) => {
    const newExpandedRows = new Set(expandedRows)
    
    if (newExpandedRows.has(priceListUID)) {
      newExpandedRows.delete(priceListUID)
    } else {
      newExpandedRows.add(priceListUID)
      
      // Fetch products if not already loaded
      if (!priceListProducts[priceListUID]) {
        setLoadingProducts(prev => ({ ...prev, [priceListUID]: true }))
        
        try {
          const request: PagingRequest = {
            PageNumber: 1,
            PageSize: 100, // Get up to 100 products
            FilterCriterias: [],
            SortCriterias: [],
            IsCountRequired: true
          }
          
          const response = await skuPriceService.getSKUPriceView(request, priceListUID)
          
          if (response.PagedData && response.PagedData.length > 0) {
            const products = response.PagedData[0].SKUPriceList || []
            setPriceListProducts(prev => ({ ...prev, [priceListUID]: products }))
          } else {
            setPriceListProducts(prev => ({ ...prev, [priceListUID]: [] }))
          }
        } catch (error) {
          console.error('Error fetching products for price list:', error)
          toast({
            title: 'Error',
            description: 'Failed to fetch products for this price list',
            variant: 'destructive'
          })
          setPriceListProducts(prev => ({ ...prev, [priceListUID]: [] }))
        } finally {
          setLoadingProducts(prev => ({ ...prev, [priceListUID]: false }))
        }
      }
    }
    
    setExpandedRows(newExpandedRows)
  }

  // Inline editing functions
  const startEditing = (productUID: string, product: ISKUPrice) => {
    setEditingProducts(prev => new Set([...prev, productUID]))
    setTempProductData(prev => ({
      ...prev,
      [productUID]: {
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
    setTempProductData(prev => {
      const newData = { ...prev }
      delete newData[productUID]
      return newData
    })
  }

  const saveEdit = async (product: ISKUPrice, priceListUID: string) => {
    if (!product.UID) return

    // Check if it's a new product
    if (product.UID.startsWith('new-')) {
      await saveNewProduct(product, priceListUID)
      return
    }

    try {
      const tempData = tempProductData[product.UID]
      if (!tempData) return

      // Create clean payload without temp fields (like TPPPriceTab approach)
      const { TempPrice, TempDefaultWSPrice, TempDefaultRetPrice, TempDummyPrice, TempMRP, TempPriceLowerLimit, TempValidFrom, TempValidUpto, ...cleanProduct } = product
      
      const updatedProduct: ISKUPrice = {
        ...cleanProduct,
        Price: tempData.Price ?? product.Price,
        MRP: tempData.MRP ?? product.MRP,
        ValidFrom: tempData.ValidFrom ? `${tempData.ValidFrom}T00:00:00` : product.ValidFrom,
        ValidUpto: tempData.ValidUpto ? `${tempData.ValidUpto}T00:00:00` : product.ValidUpto,
        Status: tempData.Status ?? product.Status
      }

      // Debug logging
      console.log('LISTS PAGE - Original product:', product)
      console.log('LISTS PAGE - Temp data:', tempData)
      console.log('LISTS PAGE - Updated product being sent:', updatedProduct)
      console.log('LISTS PAGE - JSON payload:', JSON.stringify(updatedProduct, null, 2))

      debugger; // Add debugger to compare with working TPPPriceTab

      await skuPriceService.updateSKUPrice(updatedProduct)
      
      toast({
        title: 'Success',
        description: 'Product price updated successfully'
      })

      // Update local state
      setPriceListProducts(prev => ({
        ...prev,
        [product.SKUPriceListUID!]: prev[product.SKUPriceListUID!]?.map(p => 
          p.UID === product.UID ? updatedProduct : p
        ) || []
      }))

      // Clear editing state
      cancelEditing(product.UID)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to update product price',
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

  // Add new product row
  const addNewProduct = (priceListUID: string) => {
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
      SKUPriceListUID: priceListUID
    }
    
    // Add to the beginning of the list
    setPriceListProducts(prev => ({
      ...prev,
      [priceListUID]: [newProduct, ...(prev[priceListUID] || [])]
    }))
    
    // Start editing the new row
    startEditing(newProductId, newProduct)
  }

  // Save new product
  const saveNewProduct = async (product: ISKUPrice, priceListUID: string) => {
    if (!product.SKUCode.trim()) {
      toast({
        title: 'Validation Error',
        description: 'SKU Code is required',
        variant: 'destructive'
      })
      return
    }

    try {
      const tempData = tempProductData[product.UID!] || {}
      
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
        description: 'Product price added successfully'
      })

      // Remove new row and refresh like TPPPriceTab
      setPriceListProducts(prev => ({
        ...prev,
        [priceListUID]: (prev[priceListUID] || []).filter(p => p.UID !== product.UID)
      }))
      
      setEditingProducts(prev => {
        const newSet = new Set(prev)
        newSet.delete(product.UID!)
        return newSet
      })
      
      // Refresh the products for this price list
      try {
        setLoadingProducts(prev => ({ ...prev, [priceListUID]: true }))
        
        const request: PagingRequest = {
          PageNumber: 1,
          PageSize: 100,
          FilterCriterias: [],
          SortCriterias: [],
          IsCountRequired: true
        }
        
        const response = await skuPriceService.getSKUPriceView(request, priceListUID)
        
        if (response.PagedData && response.PagedData.length > 0) {
          const products = response.PagedData[0].SKUPriceList || []
          setPriceListProducts(prev => ({ ...prev, [priceListUID]: products }))
        } else {
          setPriceListProducts(prev => ({ ...prev, [priceListUID]: [] }))
        }
      } catch (refreshError) {
        console.error('Error refreshing products:', refreshError)
      } finally {
        setLoadingProducts(prev => ({ ...prev, [priceListUID]: false }))
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to add product price',
        variant: 'destructive'
      })
    }
  }

  // Delete product
  const confirmDeleteProduct = (product: ISKUPrice) => {
    setProductToDelete(product)
    setDeleteProductDialog(true)
  }

  const handleDeleteProduct = async () => {
    if (!productToDelete || !productToDelete.UID) return

    try {
      await skuPriceService.deleteSKUPrice(productToDelete.UID)
      
      toast({
        title: 'Success',
        description: 'Product price deleted successfully'
      })

      // Remove from local state
      const priceListUID = productToDelete.SKUPriceListUID!
      setPriceListProducts(prev => ({
        ...prev,
        [priceListUID]: prev[priceListUID]?.filter(p => p.UID !== productToDelete.UID) || []
      }))
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete product price',
        variant: 'destructive'
      })
    } finally {
      setDeleteProductDialog(false)
      setProductToDelete(null)
    }
  }

  return (
    <div className="container mx-auto py-6 space-y-8">
      {/* Header Section */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-bold tracking-tight">Price Management</h1>
          <p className="text-sm text-muted-foreground">
            Manage and configure product pricing across different price lists
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="default">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button
            variant="outline"
            size="default"
            onClick={async () => {
              try {
                const blob = await skuPriceService.exportPriceLists("csv", searchTerm);
                const filename = `price-lists-export-${new Date().toISOString().split("T")[0]}.csv`;
                
                const url = URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
                
                toast({
                  title: "Success",
                  description: "Price lists exported successfully.",
                });
              } catch (error) {
                toast({
                  title: "Error",
                  description: "Failed to export price lists. Please try again.",
                  variant: "destructive",
                });
              }
            }}
          >
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button
            onClick={() => router.push('/productssales/price-management/price-list/create')}
            size="default"
          >
            <Plus className="h-4 w-4 mr-2" />
            Create Price List
          </Button>
        </div>
      </div>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <div className="p-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by name or code... (Ctrl+F)"
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
                  checked={selectedStatus.includes("Published")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev => 
                      checked 
                        ? [...prev, "Published"]
                        : prev.filter(s => s !== "Published")
                    )
                    setCurrentPage(1)
                  }}
                >
                  Published
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
              onClick={() => fetchPriceLists()}
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </div>
      </Card>

      <div className="space-y-4">

        {/* Main Table Card */}
        <Card className="shadow-sm border-gray-200">
          <div className="p-0">
            <div className="overflow-hidden rounded-lg">
            <Table>
              <TableHeader>
                <TableRow className="bg-gray-50/50">
                  <TableHead className="w-[50px] pl-6"></TableHead>
                  <TableHead className="font-medium pl-2">Code</TableHead>
                  <TableHead className="font-medium">Name</TableHead>
                  {/* <TableHead className="font-medium">Type</TableHead> */}
                  <TableHead className="font-medium">Priority</TableHead>
                  <TableHead className="font-medium">Valid From</TableHead>
                  <TableHead className="font-medium">Valid To</TableHead>
                  <TableHead className="font-medium">Status</TableHead>
                  <TableHead className="text-right font-medium pr-6">Actions</TableHead>
                </TableRow>
              </TableHeader>
            <TableBody>
              {loading ? (
                <>
                  {[...Array(pageSize)].map((_, index) => (
                    <TableRow key={`skeleton-${index}`}>
                      <TableCell className="pl-6"><Skeleton className="h-8 w-8" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      {/* <TableCell><Skeleton className="h-4 w-24" /></TableCell> */}
                      <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-6 w-16 rounded-full" /></TableCell>
                      <TableCell className="text-right pr-6">
                        <Skeleton className="h-8 w-8 rounded ml-auto" />
                      </TableCell>
                    </TableRow>
                  ))}
                </>
              ) : priceLists.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} className="text-center py-12">
                    <div className="flex flex-col items-center space-y-4">
                      <Package className="h-12 w-12 text-muted-foreground/50" />
                      <div className="space-y-2">
                        <p className="text-sm font-medium text-muted-foreground">No price lists found</p>
                        <p className="text-xs text-muted-foreground">
                          {searchTerm ? 'Try adjusting your search terms' : 'Create your first price list'}
                        </p>
                      </div>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                priceLists.map((priceList) => (
                  <React.Fragment key={priceList.UID}>
                    <TableRow className="hover:bg-muted/30 transition-colors">
                      <TableCell className="py-3 pl-6">
                        <Button
                          variant="ghost"
                          size="sm"
                          className="h-8 w-8 p-0 hover:bg-muted"
                          onClick={() => toggleRowExpansion(priceList.UID!)}
                        >
                          {expandedRows.has(priceList.UID!) ? (
                            <ChevronDown className="h-4 w-4 transition-transform" />
                          ) : (
                            <ChevronRight className="h-4 w-4 transition-transform" />
                          )}
                        </Button>
                      </TableCell>
                      <TableCell className="font-medium text-sm">{priceList.Code}</TableCell>
                      <TableCell className="text-sm">{priceList.Name}</TableCell>
                      {/* <TableCell className="text-sm text-muted-foreground">{priceList.Type || '-'}</TableCell> */}
                      <TableCell className="text-sm font-medium">{priceList.Priority}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">{formatDate(priceList.ValidFrom)}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">{formatDate(priceList.ValidUpto)}</TableCell>
                      <TableCell>
                        <Badge 
                          variant={priceList.Status === 'Published' ? 'default' : 'secondary'}
                          className="font-medium"
                        >
                          {priceList.Status}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right pr-6">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" className="h-8 w-8 p-0">
                              <span className="sr-only">Open menu</span>
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => router.push(`/productssales/price-management/price-list/manage?uid=${priceList.UID}`)}
                              className="cursor-pointer"
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => router.push(`/productssales/price-management/price-list/edit?uid=${priceList.UID}`)}
                              className="cursor-pointer"
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit Price List
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() => confirmDelete(priceList.UID!)}
                              className="cursor-pointer text-red-600 focus:text-red-600"
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete Price List
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                    {expandedRows.has(priceList.UID!) && (
                      <TableRow>
                        <TableCell colSpan={8} className="p-0 border-b-2">
                          <div className="bg-gradient-to-b from-muted/20 to-muted/10 px-6 py-4">
                            {loadingProducts[priceList.UID!] ? (
                              <div className="flex items-center justify-center py-8 space-x-3">
                                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                                <span className="text-sm text-muted-foreground">Loading products...</span>
                              </div>
                            ) : priceListProducts[priceList.UID!] && priceListProducts[priceList.UID!].length > 0 ? (
                              // Debug: Products found for {priceList.UID} - {priceListProducts[priceList.UID!].length} items
                              <div className="space-y-4">
                                <div className="flex items-center justify-between">
                                  <div className="flex items-center gap-2">
                                    <div className="p-1.5 bg-primary/10 rounded-md">
                                      <Package className="h-4 w-4 text-primary" />
                                    </div>
                                    <div>
                                      <span className="text-sm font-semibold">Product Pricing</span>
                                      <span className="text-sm text-muted-foreground ml-2">
                                        ({priceListProducts[priceList.UID!]?.length || 0} items)
                                      </span>
                                    </div>
                                  </div>
                                  <div className="flex gap-2">
                                    {(priceListProducts[priceList.UID!]?.length || 0) > 10 && (
                                      <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => router.push(`/productssales/price-management/price-list/manage?uid=${priceList.UID}`)}
                                      >
                                        View All Products
                                      </Button>
                                    )}
                                  </div>
                                </div>
                                <div className="rounded-lg border bg-card overflow-hidden">
                                  <Table>
                                    <TableHeader>
                                      <TableRow className="bg-muted/30 hover:bg-muted/30">
                                        <TableHead className="text-xs font-medium">SKU Code</TableHead>
                                        <TableHead className="text-xs font-medium">Product Name</TableHead>
                                        <TableHead className="text-xs font-medium">UOM</TableHead>
                                        <TableHead className="text-right text-xs font-medium">Price</TableHead>
                                        <TableHead className="text-right text-xs font-medium">MRP</TableHead>
                                        <TableHead className="text-xs font-medium">Valid From</TableHead>
                                        <TableHead className="text-xs font-medium">Valid To</TableHead>
                                        <TableHead className="text-xs font-medium">Status</TableHead>
                                        <TableHead className="text-right text-xs font-medium">Actions</TableHead>
                                      </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                      {(priceListProducts[priceList.UID!] || []).slice(0, 10).map((product) => {
                                        const isEditing = editingProducts.has(product.UID!)
                                        const tempData = tempProductData[product.UID!] || {}
                                        
                                        return (
                                          <TableRow 
                                            key={product.UID} 
                                            className={`hover:bg-muted/20 ${isEditing ? 'bg-blue-50' : ''}`}
                                          >
                                            {/* SKU Code - Editable for new products */}
                                            <TableCell className="font-medium text-xs py-2">
                                              {isEditing && product.UID!.startsWith('new-') ? (
                                                <Input
                                                  value={tempData.SKUCode ?? product.SKUCode}
                                                  onChange={(e) => updateTempData(product.UID!, 'SKUCode', e.target.value)}
                                                  placeholder="SKU Code"
                                                  className="w-24 h-7 text-xs"
                                                />
                                              ) : (
                                                product.SKUCode
                                              )}
                                            </TableCell>
                                            {/* SKU Name - Editable for new products */}
                                            <TableCell className="text-xs py-2">
                                              {isEditing && product.UID!.startsWith('new-') ? (
                                                <Input
                                                  value={tempData.SKUName ?? product.SKUName}
                                                  onChange={(e) => updateTempData(product.UID!, 'SKUName', e.target.value)}
                                                  placeholder="Product Name"
                                                  className="w-32 h-7 text-xs"
                                                />
                                              ) : (
                                                product.SKUName || '-'
                                              )}
                                            </TableCell>
                                            <TableCell className="text-xs py-2">
                                              {isEditing ? (
                                                <Select
                                                  value={tempData.UOM ?? product.UOM ?? 'PCS'}
                                                  onValueChange={(value) => updateTempData(product.UID!, 'UOM', value)}
                                                  disabled={loadingUOM}
                                                >
                                                  <SelectTrigger className="w-24 h-7 text-xs">
                                                    <SelectValue placeholder="Select UOM" />
                                                  </SelectTrigger>
                                                  <SelectContent>
                                                    {uomTypes.length > 0 ? (
                                                      uomTypes.map((uom) => (
                                                        <SelectItem key={uom.UID} value={uom.UID} className="text-xs">
                                                          {uom.Name || uom.UID}
                                                        </SelectItem>
                                                      ))
                                                    ) : (
                                                      // Fallback to common UOM types if API fails
                                                      ['PCS', 'BOX', 'CTN', 'KG', 'L', 'EA'].map((uom) => (
                                                        <SelectItem key={uom} value={uom} className="text-xs">
                                                          {uom}
                                                        </SelectItem>
                                                      ))
                                                    )}
                                                  </SelectContent>
                                                </Select>
                                              ) : (
                                                <span className="px-2 py-0.5 bg-muted rounded text-xs">
                                                  {product.UOM}
                                                </span>
                                              )}
                                            </TableCell>
                                            
                                            {/* Price - Editable */}
                                            <TableCell className="text-right text-xs py-2">
                                              {isEditing ? (
                                                <Input
                                                  type="number"
                                                  step="0.01"
                                                  value={tempData.Price ?? product.Price}
                                                  onChange={(e) => updateTempData(product.UID!, 'Price', parseFloat(e.target.value) || 0)}
                                                  className="w-20 h-7 text-xs text-right"
                                                />
                                              ) : (
                                                <span className="font-medium">{product.Price?.toFixed(2)}</span>
                                              )}
                                            </TableCell>
                                            
                                            {/* MRP - Editable */}
                                            <TableCell className="text-right text-xs py-2">
                                              {isEditing ? (
                                                <Input
                                                  type="number"
                                                  step="0.01"
                                                  value={tempData.MRP ?? product.MRP}
                                                  onChange={(e) => updateTempData(product.UID!, 'MRP', parseFloat(e.target.value) || 0)}
                                                  className="w-20 h-7 text-xs text-right"
                                                />
                                              ) : (
                                                <span className="text-muted-foreground">{product.MRP?.toFixed(2)}</span>
                                              )}
                                            </TableCell>
                                            
                                            {/* Valid From - Editable */}
                                            <TableCell className="text-xs py-2">
                                              {isEditing ? (
                                                <Input
                                                  type="date"
                                                  value={tempData.ValidFrom ?? (product.ValidFrom ? (typeof product.ValidFrom === 'string' ? product.ValidFrom.slice(0, 10) : new Date(product.ValidFrom).toISOString().slice(0, 10)) : '')}
                                                  onChange={(e) => updateTempData(product.UID!, 'ValidFrom', e.target.value)}
                                                  className="w-28 h-7 text-xs"
                                                />
                                              ) : (
                                                <span className="text-muted-foreground">{formatDate(product.ValidFrom)}</span>
                                              )}
                                            </TableCell>
                                            
                                            {/* Valid To - Editable */}
                                            <TableCell className="text-xs py-2">
                                              {isEditing ? (
                                                <Input
                                                  type="date"
                                                  value={tempData.ValidUpto ?? (product.ValidUpto ? (typeof product.ValidUpto === 'string' ? product.ValidUpto.slice(0, 10) : new Date(product.ValidUpto).toISOString().slice(0, 10)) : '')}
                                                  onChange={(e) => updateTempData(product.UID!, 'ValidUpto', e.target.value)}
                                                  className="w-28 h-7 text-xs"
                                                />
                                              ) : (
                                                <span className="text-muted-foreground">{formatDate(product.ValidUpto)}</span>
                                              )}
                                            </TableCell>
                                            
                                            {/* Status - Editable */}
                                            <TableCell className="py-2">
                                              {isEditing ? (
                                                <Select
                                                  value={tempData.Status ?? product.Status}
                                                  onValueChange={(value) => updateTempData(product.UID!, 'Status', value)}
                                                >
                                                  <SelectTrigger className="w-20 h-7 text-xs">
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
                                                  className="text-xs px-2 py-0"
                                                >
                                                  {product.Status}
                                                </Badge>
                                              )}
                                            </TableCell>
                                            
                                            {/* Actions */}
                                            <TableCell className="text-right py-2">
                                              {isEditing ? (
                                                <div className="flex gap-1 justify-end">
                                                  <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    className="h-6 w-6 p-0 text-green-600 hover:text-green-700"
                                                    onClick={() => saveEdit(product, priceList.UID!)}
                                                  >
                                                    ✓
                                                  </Button>
                                                  <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    className="h-6 w-6 p-0 text-red-600 hover:text-red-700"
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
                                                    className="h-6 w-6 p-0"
                                                    onClick={() => startEditing(product.UID!, product)}
                                                    title="Edit product"
                                                  >
                                                    <Edit className="h-3 w-3" />
                                                  </Button>
                                                  <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    className="h-6 w-6 p-0"
                                                    onClick={() => {
                                                      // Smart add - copy this product's data with new dates
                                                      const newProductId = `new-${priceList.UID}-${newRowCounter}`
                                                      setNewRowCounter(prev => prev + 1)
                                                      
                                                      // Calculate the next day after current product's ValidUpto (Valid To)
                                                      let currentEndDate: Date
                                                      
                                                      console.log('Current ValidUpto raw:', product.ValidUpto, typeof product.ValidUpto)
                                                      
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
                                                          console.error('Invalid date from ValidUpto:', product.ValidUpto)
                                                          currentEndDate = new Date() // Use current date as fallback
                                                        }
                                                      } else {
                                                        currentEndDate = new Date()
                                                      }
                                                      
                                                      // Add 1 day to get the next start date
                                                      const nextStartDay = new Date(currentEndDate)
                                                      nextStartDay.setDate(nextStartDay.getDate() + 1)
                                                      
                                                      // Debug logging
                                                      console.log('Parsed end date:', currentEndDate.toISOString().slice(0, 10))
                                                      console.log('Next start day:', nextStartDay.toISOString().slice(0, 10))
                                                      
                                                      // Set end date to Dec 31 of next year from the new start date
                                                      const nextYearEnd = new Date(nextStartDay.getUTCFullYear() + 1, 11, 31)
                                                      
                                                      const newProduct: ISKUPrice = {
                                                        UID: newProductId,
                                                        SKUCode: product.SKUCode,
                                                        SKUName: product.SKUName,
                                                        UOM: product.UOM,
                                                        Price: product.Price,
                                                        MRP: product.MRP,
                                                        Status: 'Active',
                                                        ValidFrom: nextStartDay.toISOString().slice(0, 10),
                                                        ValidUpto: nextYearEnd.toISOString().slice(0, 10),
                                                        IsActive: true,
                                                        SKUUID: product.SKUUID,
                                                        SKUPriceListUID: priceList.UID || ''
                                                      }
                                                      
                                                      // Add to the price list products right below the source product
                                                      setPriceListProducts(prev => {
                                                        const currentProducts = prev[priceList.UID!] || []
                                                        const sourceIndex = currentProducts.findIndex(p => p.UID === product.UID)
                                                        
                                                        if (sourceIndex !== -1) {
                                                          // Insert new product right after the source product
                                                          const updatedProducts = [
                                                            ...currentProducts.slice(0, sourceIndex + 1),
                                                            newProduct,
                                                            ...currentProducts.slice(sourceIndex + 1)
                                                          ]
                                                          return {
                                                            ...prev,
                                                            [priceList.UID!]: updatedProducts
                                                          }
                                                        } else {
                                                          // Fallback: add to the beginning if source not found
                                                          return {
                                                            ...prev,
                                                            [priceList.UID!]: [newProduct, ...currentProducts]
                                                          }
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
                                                    <Plus className="h-3 w-3" />
                                                  </Button>
                                                  <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    className="h-6 w-6 p-0 text-red-600 hover:text-red-700"
                                                    onClick={() => confirmDeleteProduct(product)}
                                                    title="Delete product"
                                                  >
                                                    <Trash2 className="h-3 w-3" />
                                                  </Button>
                                                </div>
                                              )}
                                            </TableCell>
                                          </TableRow>
                                        )
                                      })}
                                      {/* Add Product Row */}
                                      <TableRow className="hover:bg-muted/10">
                                        <TableCell colSpan={9} className="text-center py-3">
                                          <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={() => addNewProduct(priceList.UID!)}
                                            className="text-primary hover:text-primary/80"
                                          >
                                            <Plus className="h-4 w-4 mr-2" />
                                            Add Product
                                          </Button>
                                        </TableCell>
                                      </TableRow>
                                    </TableBody>
                                  </Table>
                                </div>
                              </div>
                            ) : (
                              <div className="flex flex-col items-center justify-center py-12 space-y-4">
                                <div className="p-3 bg-muted/50 rounded-full">
                                  <Package className="h-6 w-6 text-muted-foreground" />
                                </div>
                                <p className="text-sm text-muted-foreground">No products found in this price list</p>
                                <Button
                                  variant="outline"
                                  size="sm"
                                  onClick={() => addNewProduct(priceList.UID!)}
                                >
                                  <Plus className="h-4 w-4 mr-2" />
                                  Add Product
                                </Button>
                              </div>
                            )}
                          </div>
                        </TableCell>
                      </TableRow>
                    )}
                  </React.Fragment>
                ))
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
                itemName="price lists"
              />
            </div>
          )}
        </Card>
      </div>

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the price list.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-red-600 hover:bg-red-700">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Delete Product Confirmation Dialog */}
      <AlertDialog open={deleteProductDialog} onOpenChange={setDeleteProductDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Product Price</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this product price? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction 
              onClick={handleDeleteProduct} 
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