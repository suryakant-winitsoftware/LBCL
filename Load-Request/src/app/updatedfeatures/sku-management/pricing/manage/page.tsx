'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Search, Filter, Plus, Upload, DollarSign, Calendar } from 'lucide-react'
import { skuService, SKUPriceList, PagedResponse } from '@/services/sku/sku.service'
import { PagingRequest } from '@/types/common.types'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useToast } from '@/components/ui/use-toast'

export default function ManagePricingPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [priceLists, setPriceLists] = useState<SKUPriceList[]>([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const pageSize = 10

  const fetchPriceLists = async () => {
    setLoading(true)
    try {
      const request: PagingRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: searchTerm ? [{ name: 'Name', value: searchTerm }] : [],
        sortCriterias: [{ sortParameter: 'Priority', direction: 'Asc' }],
        isCountRequired: true
      }
      
      const response: PagedResponse<SKUPriceList> = await skuService.getAllPriceLists(request)
      setPriceLists(response.PagedData)
      setTotalCount(response.TotalCount)
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

  useEffect(() => {
    fetchPriceLists()
  }, [currentPage, searchTerm])

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1)
  }

  const getStatusBadge = (priceList: SKUPriceList) => {
    const now = new Date()
    const validFrom = new Date(priceList.ValidFrom)
    const validUpto = new Date(priceList.ValidUpto)

    if (!priceList.IsActive) {
      return <Badge variant="secondary">Inactive</Badge>
    }
    if (now < validFrom) {
      return <Badge variant="outline">Scheduled</Badge>
    }
    if (now > validUpto) {
      return <Badge variant="destructive">Expired</Badge>
    }
    return <Badge variant="default">Active</Badge>
  }

  const totalPages = Math.ceil(totalCount / pageSize)

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Manage Pricing</h1>
          <p className="text-muted-foreground">
            Configure and manage product price lists
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => router.push('/updatedfeatures/sku-management/pricing/bulk-update')}
          >
            <Upload className="h-4 w-4 mr-2" />
            Bulk Update
          </Button>
          <Button
            onClick={() => router.push('/updatedfeatures/sku-management/pricing/create')}
          >
            <Plus className="h-4 w-4 mr-2" />
            Create Price List
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Price Lists</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {priceLists.filter(p => p.IsActive && new Date() >= new Date(p.ValidFrom) && new Date() <= new Date(p.ValidUpto)).length}
            </div>
            <p className="text-xs text-muted-foreground">Currently in effect</p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Scheduled</CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {priceLists.filter(p => p.IsActive && new Date() < new Date(p.ValidFrom)).length}
            </div>
            <p className="text-xs text-muted-foreground">Future price lists</p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Price Lists</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalCount}</div>
            <p className="text-xs text-muted-foreground">All price configurations</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Price Lists</CardTitle>
          <CardDescription>
            Manage pricing configurations for different channels and periods
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4 mb-6">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                placeholder="Search price lists..."
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
                  <TableHead>Code</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Distribution Channel</TableHead>
                  <TableHead>Priority</TableHead>
                  <TableHead>Valid From</TableHead>
                  <TableHead>Valid To</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <TableRow>
                    <TableCell colSpan={8} className="text-center py-8">
                      Loading...
                    </TableCell>
                  </TableRow>
                ) : priceLists.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} className="text-center py-8">
                      No price lists found
                    </TableCell>
                  </TableRow>
                ) : (
                  priceLists.map((priceList) => (
                    <TableRow key={priceList.UID}>
                      <TableCell className="font-medium">{priceList.Code}</TableCell>
                      <TableCell>{priceList.Name}</TableCell>
                      <TableCell>{priceList.DistributionChannelUID}</TableCell>
                      <TableCell>{priceList.Priority}</TableCell>
                      <TableCell>{new Date(priceList.ValidFrom).toLocaleDateString()}</TableCell>
                      <TableCell>{new Date(priceList.ValidUpto).toLocaleDateString()}</TableCell>
                      <TableCell>{getStatusBadge(priceList)}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                          >
                            View
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                          >
                            Edit
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-between mt-4">
              <p className="text-sm text-muted-foreground">
                Showing {(currentPage - 1) * pageSize + 1} to{' '}
                {Math.min(currentPage * pageSize, totalCount)} of {totalCount} price lists
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
        </CardContent>
      </Card>
    </div>
  )
}