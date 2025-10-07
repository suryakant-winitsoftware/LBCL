'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Search, Filter, Plus, FolderTree, ChevronRight } from 'lucide-react'
import { skuService, PagedResponse } from '@/services/sku/sku.service'
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

interface Category {
  CategoryUID: string
  CategoryCode: string
  CategoryName: string
  ParentCategoryName?: string
  ProductCount: number
  IsActive: boolean
}

export default function ProductCategoriesPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const pageSize = 10

  const fetchCategories = async () => {
    setLoading(true)
    try {
      const mockCategories: Category[] = [
        {
          CategoryUID: '1',
          CategoryCode: 'BEV001',
          CategoryName: 'Beverages',
          ProductCount: 45,
          IsActive: true
        },
        {
          CategoryUID: '2',
          CategoryCode: 'SNACK001',
          CategoryName: 'Snacks',
          ProductCount: 32,
          IsActive: true
        },
        {
          CategoryUID: '3',
          CategoryCode: 'DAIRY001',
          CategoryName: 'Dairy Products',
          ProductCount: 28,
          IsActive: true
        },
        {
          CategoryUID: '4',
          CategoryCode: 'MEAT001',
          CategoryName: 'Meat & Poultry',
          ProductCount: 15,
          IsActive: true
        },
        {
          CategoryUID: '5',
          CategoryCode: 'BAKERY001',
          CategoryName: 'Bakery',
          ProductCount: 22,
          IsActive: true
        }
      ]
      
      const filteredCategories = mockCategories.filter(cat => 
        searchTerm === '' || 
        cat.CategoryName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cat.CategoryCode.toLowerCase().includes(searchTerm.toLowerCase())
      )
      
      setCategories(filteredCategories)
      setTotalCount(filteredCategories.length)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch categories',
        variant: 'destructive'
      })
      console.error('Error fetching categories:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchCategories()
  }, [currentPage, searchTerm])

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1)
  }

  const handleViewProducts = (categoryId: string) => {
    router.push(`/updatedfeatures/sku-management/products/manage?category=${categoryId}`)
  }

  const handleEditCategory = (categoryId: string) => {
    router.push(`/updatedfeatures/sku-management/groups/manage?uid=${categoryId}`)
  }

  const totalPages = Math.ceil(totalCount / pageSize)

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Categories</h1>
          <p className="text-muted-foreground">
            Organize products by categories and manage category hierarchy
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => router.push('/updatedfeatures/sku-management/groups/hierarchy')}
          >
            <FolderTree className="h-4 w-4 mr-2" />
            View Hierarchy
          </Button>
          <Button
            onClick={() => router.push('/updatedfeatures/sku-management/groups/manage')}
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Category
          </Button>
        </div>
      </div>

      <Card className="p-6">
        <div className="flex gap-4 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by category name or code..."
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
                <TableHead>Category Code</TableHead>
                <TableHead>Category Name</TableHead>
                <TableHead>Parent Category</TableHead>
                <TableHead>Product Count</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8">
                    Loading...
                  </TableCell>
                </TableRow>
              ) : categories.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8">
                    No categories found
                  </TableCell>
                </TableRow>
              ) : (
                categories.map((category) => (
                  <TableRow key={category.CategoryUID}>
                    <TableCell className="font-medium">{category.CategoryCode}</TableCell>
                    <TableCell>{category.CategoryName}</TableCell>
                    <TableCell>{category.ParentCategoryName || '-'}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        {category.ProductCount}
                        <Button
                          variant="ghost"
                          size="sm"
                          className="h-6 w-6 p-0"
                          onClick={() => handleViewProducts(category.CategoryUID)}
                        >
                          <ChevronRight className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={category.IsActive ? 'default' : 'secondary'}>
                        {category.IsActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleViewProducts(category.CategoryUID)}
                        >
                          View Products
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditCategory(category.CategoryUID)}
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
              {Math.min(currentPage * pageSize, totalCount)} of {totalCount} categories
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