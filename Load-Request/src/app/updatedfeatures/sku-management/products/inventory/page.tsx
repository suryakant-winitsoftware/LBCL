'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Search, Filter, Download, Upload, Package } from 'lucide-react'
import { skuService, SKUListView, PagedResponse } from '@/services/sku/sku.service'
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

export default function ProductInventoryPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [skus, setSKUs] = useState<SKUListView[]>([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const pageSize = 10

  const fetchSKUs = async () => {
    setLoading(true)
    try {
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: searchTerm ? [{ Name: 'Name', Value: searchTerm }] : [],
        SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
        IsCountRequired: true
      }
      
      const response: PagedResponse<SKUListView> = await skuService.getAllSKUs(request)
      setSKUs(response.PagedData)
      setTotalCount(response.TotalCount)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch inventory data',
        variant: 'destructive'
      })
      console.error('Error fetching SKUs:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchSKUs()
  }, [currentPage, searchTerm])

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1)
  }

  const handleUpdateInventory = (skuUID: string) => {
    router.push(`/updatedfeatures/sku-management/products/edit?uid=${skuUID}&tab=inventory`)
  }

  const getStockStatus = (stock: number) => {
    if (stock === 0) return { label: 'Out of Stock', variant: 'destructive' as const }
    if (stock < 10) return { label: 'Low Stock', variant: 'warning' as const }
    return { label: 'In Stock', variant: 'default' as const }
  }

  const totalPages = Math.ceil(totalCount / pageSize)

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Inventory</h1>
          <p className="text-muted-foreground">
            Track and manage inventory levels for your products
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import Stock
          </Button>
          <Button variant="outline" size="sm">
            <Download className="h-4 w-4 mr-2" />
            Export Inventory
          </Button>
          <Button>
            <Package className="h-4 w-4 mr-2" />
            Stock Adjustment
          </Button>
        </div>
      </div>

      <Card className="p-6">
        <div className="flex gap-4 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by product name or code..."
              value={searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10"
            />
          </div>
          <Button variant="outline" size="icon">
            <Filter className="h-4 w-4" />
          </Button>
        </div>

        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>SKU Code</TableHead>
                <TableHead>Product Name</TableHead>
                <TableHead>Available Stock</TableHead>
                <TableHead>Reserved</TableHead>
                <TableHead>On Hand</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-8">
                    Loading...
                  </TableCell>
                </TableRow>
              ) : !skus || skus.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-8">
                    No products found
                  </TableCell>
                </TableRow>
              ) : (
                (skus || []).map((sku) => {
                  const stock = Math.floor(Math.random() * 100)
                  const stockStatus = getStockStatus(stock)
                  return (
                    <TableRow key={sku.SKUUID}>
                      <TableCell className="font-medium">{sku.SKUCode}</TableCell>
                      <TableCell>{sku.SKULongName}</TableCell>
                      <TableCell>{stock}</TableCell>
                      <TableCell>0</TableCell>
                      <TableCell>{stock}</TableCell>
                      <TableCell>
                        <Badge variant={stockStatus.variant}>
                          {stockStatus.label}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleUpdateInventory(sku.SKUUID)}
                        >
                          Update Stock
                        </Button>
                      </TableCell>
                    </TableRow>
                  )
                })
              )}
            </TableBody>
          </Table>
        </div>

        {totalPages > 1 && (
          <div className="flex items-center justify-between mt-4">
            <p className="text-sm text-muted-foreground">
              Showing {(currentPage - 1) * pageSize + 1} to{' '}
              {Math.min(currentPage * pageSize, totalCount)} of {totalCount} products
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(currentPage - 1)}
                disabled={currentPage === 1}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(currentPage + 1)}
                disabled={currentPage === totalPages}
              >
                Next
              </Button>
            </div>
          </div>
        )}
      </Card>
    </div>
  )
}