'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Search, Filter, FolderTree, Plus } from 'lucide-react'
import { skuService, SKUGroup, SKUToGroupMapping, PagedResponse } from '@/services/sku/sku.service'
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

export default function ManageGroupsPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [groups, setGroups] = useState<SKUGroup[]>([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const pageSize = 10

  const fetchGroups = async () => {
    setLoading(true)
    try {
      const request: PagingRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: searchTerm ? [{ name: 'Name', value: searchTerm }] : [],
        sortCriterias: [{ sortParameter: 'ItemLevel', direction: 'Asc' }, { sortParameter: 'Name', direction: 'Asc' }],
        isCountRequired: true
      }
      
      const response: PagedResponse<SKUGroup> = await skuService.getAllSKUGroups(request)
      setGroups(response.PagedData)
      setTotalCount(response.TotalCount)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch groups',
        variant: 'destructive'
      })
      console.error('Error fetching groups:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchGroups()
  }, [currentPage, searchTerm])

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1)
  }

  const getLevelBadgeVariant = (level: number) => {
    switch (level) {
      case 1:
        return 'default'
      case 2:
        return 'secondary'
      default:
        return 'outline'
    }
  }

  const getLevelName = (level: number) => {
    switch (level) {
      case 1:
        return 'Brand'
      case 2:
        return 'Category'
      case 3:
        return 'Sub-Category'
      default:
        return `Level ${level}`
    }
  }

  const totalPages = Math.ceil(totalCount / pageSize)

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Manage Groups</h1>
          <p className="text-muted-foreground">
            Organize products into hierarchical groups and categories
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
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Create Group
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Product Groups</CardTitle>
          <CardDescription>
            Manage your product categorization and group hierarchy
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4 mb-6">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                placeholder="Search groups..."
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
                  <TableHead>Level</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Supplier</TableHead>
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
                ) : groups.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center py-8">
                      No groups found
                    </TableCell>
                  </TableRow>
                ) : (
                  groups.map((group) => (
                    <TableRow key={group.UID}>
                      <TableCell className="font-medium">{group.Code}</TableCell>
                      <TableCell>{group.Name}</TableCell>
                      <TableCell>
                        <Badge variant={getLevelBadgeVariant(group.ItemLevel)}>
                          {getLevelName(group.ItemLevel)}
                        </Badge>
                      </TableCell>
                      <TableCell>{group.SKUGroupTypeUID}</TableCell>
                      <TableCell>{group.SupplierOrgUID}</TableCell>
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
                {Math.min(currentPage * pageSize, totalCount)} of {totalCount} groups
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

      <Card>
        <CardHeader>
          <CardTitle>Quick Stats</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4">
            <div className="text-center p-4 border rounded-lg">
              <p className="text-2xl font-bold">1</p>
              <p className="text-sm text-muted-foreground">Brands</p>
            </div>
            <div className="text-center p-4 border rounded-lg">
              <p className="text-2xl font-bold">9</p>
              <p className="text-sm text-muted-foreground">Categories</p>
            </div>
            <div className="text-center p-4 border rounded-lg">
              <p className="text-2xl font-bold">{totalCount}</p>
              <p className="text-sm text-muted-foreground">Total Groups</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}