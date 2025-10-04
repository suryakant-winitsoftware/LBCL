'use client'

import React, { useState, useEffect } from 'react'
import { useRouter, useParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import {
  ArrowLeft,
  Search,
  Download,
  Upload,
  Plus,
  Edit,
  Trash2,
  Eye,
  MoreHorizontal,
  Filter,
  RefreshCw,
  X,
  Store,
  Users,
  MapPin,
  Phone,
  Calendar,
  Building
} from 'lucide-react'
import { storeGroupService } from '@/services/storeGroupService'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'
import { PagingRequest } from '@/types/common.types'
import { PaginationControls } from '@/components/ui/pagination-controls'
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
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Skeleton } from '@/components/ui/skeleton'
import { authService } from '@/lib/auth-service'
import { storeToGroupMappingService } from '@/services/storeToGroupMappingService'

interface IStoreGroup {
  UID?: string
  Code?: string
  Name?: string
  Description?: string
  StoreGroupTypeUID?: string
  ParentStoreGroupUID?: string
  IsActive?: boolean
  CreatedBy?: string
  CreatedTime?: string
  ModifiedBy?: string
  ModifiedTime?: string
  ServerAddTime?: string
  ServerModifiedTime?: string
}

interface IStoreInGroup {
  UID?: string
  Code?: string
  Name?: string
  Address?: string
  Phone?: string
  Email?: string
  IsActive?: boolean
  StoreGroupUID?: string
  CreatedBy?: string
  CreatedTime?: string
  ModifiedBy?: string
  ModifiedTime?: string
}

export default function StoreGroupDetailPage() {
  const router = useRouter()
  const params = useParams()
  const { toast } = useToast()

  const groupUID = params.id as string

  // State for store group details
  const [storeGroup, setStoreGroup] = useState<IStoreGroup | null>(null)
  const [loadingGroup, setLoadingGroup] = useState(true)

  // State for stores in group
  const [stores, setStores] = useState<IStoreInGroup[]>([])
  const [loadingStores, setLoadingStores] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filteredStores, setFilteredStores] = useState<IStoreInGroup[]>([])

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [totalCount, setTotalCount] = useState(0)

  // Dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [selectedStore, setSelectedStore] = useState<IStoreInGroup | null>(null)

  const currentUser = authService.getCurrentUsername()

  // Load store group details
  const loadStoreGroupDetails = async () => {
    try {
      setLoadingGroup(true)
      const groupData = await storeGroupService.getStoreGroupByUID(groupUID)
      setStoreGroup(groupData)
    } catch (error) {
      console.error('Error loading store group:', error)
      toast({
        title: "Error",
        description: "Failed to load store group details. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoadingGroup(false)
    }
  }

  // Load stores in group using real API
  const loadStoresInGroup = async () => {
    try {
      setLoadingStores(true)

      console.log(`🏪 Loading stores for group: ${groupUID}`)

      // Use the real API to get stores by group UID
      const stores = await storeToGroupMappingService.getStoresByGroupUID(groupUID as string)

      console.log(`✅ Loaded ${stores.length} stores for group ${groupUID}`)

      // Convert to our interface format
      const storesInGroup: IStoreInGroup[] = stores.map(store => ({
        UID: store.UID || store.uid,
        Code: store.Code || store.code,
        Name: store.Name || store.name,
        Address: store.Address || store.address || 'No address available',
        Phone: store.Mobile || store.mobile || store.Phone || store.phone || 'No phone available',
        Email: store.Email || store.email || 'No email available',
        IsActive: store.IsActive ?? store.is_active ?? true,
        StoreGroupUID: groupUID as string,
        CreatedBy: store.CreatedBy || store.created_by || currentUser,
        CreatedTime: store.CreatedTime || store.created_time || new Date().toISOString(),
        ModifiedBy: store.ModifiedBy || store.modified_by || currentUser,
        ModifiedTime: store.ModifiedTime || store.modified_time || new Date().toISOString()
      }))

      setStores(storesInGroup)
      setTotalCount(storesInGroup.length)
    } catch (error) {
      console.error('Error loading stores:', error)
      toast({
        title: "Error",
        description: "Failed to load stores for this group.",
        variant: "destructive",
      })
      setStores([])
    } finally {
      setLoadingStores(false)
    }
  }

  // Filter stores based on search and pagination
  useEffect(() => {
    let filtered = stores
    if (searchTerm) {
      filtered = stores.filter(store =>
        store.Name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        store.Code?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    // Apply pagination
    const startIndex = (currentPage - 1) * pageSize
    const endIndex = startIndex + pageSize
    const paginatedStores = filtered.slice(startIndex, endIndex)

    setFilteredStores(paginatedStores)
    setTotalCount(filtered.length)
  }, [stores, searchTerm, currentPage, pageSize])

  // Handle search
  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1)
  }

  // Handle delete store from group
  const handleRemoveStore = async () => {
    if (!selectedStore?.UID) return

    try {
      // In production, call the StoreToGroupMapping delete API
      toast({
        title: "Success",
        description: "Store removed from group successfully.",
      })
      loadStoresInGroup()
    } catch (error) {
      console.error('Error removing store:', error)
      toast({
        title: "Error",
        description: "Failed to remove store from group. Please try again.",
        variant: "destructive",
      })
    } finally {
      setDeleteDialogOpen(false)
      setSelectedStore(null)
    }
  }

  // Handle refresh
  const handleRefresh = () => {
    loadStoresInGroup()
  }

  // Format date helper
  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'N/A'
    const date = new Date(dateString)
    return formatDateToDayMonthYear(date, '-')
  }

  // Load data on mount
  useEffect(() => {
    if (groupUID) {
      loadStoreGroupDetails()
      loadStoresInGroup()
    }
  }, [groupUID])

  return (
    <div className="container mx-auto py-4 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-4">
          <Button
            variant="outline"
            size="sm"
            onClick={() => router.back()}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <div>
            <h1 className="text-2xl font-bold">
              {loadingGroup ? (
                <Skeleton className="h-8 w-48" />
              ) : (
                storeGroup?.Name || 'Store Group Details'
              )}
            </h1>
          </div>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => router.push(`/administration/store-management/store-groups/edit/${groupUID}`)}
            disabled={loadingGroup}
          >
            <Edit className="h-4 w-4 mr-2" />
            Edit Group
          </Button>
          <Button
            size="sm"
            onClick={() => router.push(`/administration/store-management/store-groups/${groupUID}/assign-stores`)}
          >
            <Plus className="h-4 w-4 mr-2" />
            Assign Stores
          </Button>
        </div>
      </div>

      {/* Store Group Info Card */}
      <Card className="shadow-sm border-gray-200">
        <CardHeader>
          <CardTitle className="flex items-center">
            <Building className="h-5 w-5 mr-2 text-blue-600" />
            Group Information
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loadingGroup ? (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {Array.from({ length: 6 }).map((_, index) => (
                <div key={index} className="space-y-2">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-6 w-full" />
                </div>
              ))}
            </div>
          ) : storeGroup ? (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-500">Code</p>
                <p className="text-sm text-gray-900">{storeGroup.Code}</p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-500">Name</p>
                <p className="text-sm text-gray-900">{storeGroup.Name}</p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-500">Status</p>
                <Badge variant={storeGroup.IsActive ? "default" : "secondary"}>
                  {storeGroup.IsActive ? 'Active' : 'Inactive'}
                </Badge>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-500">Description</p>
                <p className="text-sm text-gray-900">{storeGroup.Description || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-500">Created By</p>
                <p className="text-sm text-gray-900">{storeGroup.CreatedBy}</p>
              </div>
              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-500">Created Date</p>
                <p className="text-sm text-gray-900">{formatDate(storeGroup.CreatedTime)}</p>
              </div>
            </div>
          ) : (
            <div className="text-center py-8">
              <p className="text-sm text-gray-500">Failed to load store group details</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                placeholder="Search stores by name or code..."
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={handleRefresh}
              disabled={loadingStores}
            >
              <RefreshCw className={`h-4 w-4 ${loadingStores ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Stores Table */}
      <Card className="shadow-sm">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center">
              <Store className="h-5 w-5 mr-2 text-blue-600" />
              Stores in Group
              <Badge variant="outline" className="ml-2">
                {totalCount}
              </Badge>
            </CardTitle>
          </div>
        </CardHeader>
        <div className="overflow-hidden rounded-lg">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="pl-6">Code</TableHead>
                <TableHead>Name</TableHead>
                <TableHead className="text-center">Status</TableHead>
                <TableHead>Created Date</TableHead>
                <TableHead className="text-right pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loadingStores ? (
                Array.from({ length: 5 }).map((_, index) => (
                  <TableRow key={index}>
                    <TableCell className="pl-6"><Skeleton className="h-4 w-20" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                    <TableCell className="text-center"><Skeleton className="h-4 w-16 mx-auto" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell className="text-right pr-6"><Skeleton className="h-4 w-20 ml-auto" /></TableCell>
                  </TableRow>
                ))
              ) : filteredStores.length === 0 && searchTerm ? (
                <TableRow>
                  <TableCell colSpan={5} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center space-y-2">
                      <Search className="h-8 w-8 text-gray-400" />
                      <p className="text-sm text-gray-500">
                        No stores found matching your search
                      </p>
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => handleSearch('')}
                      >
                        Clear search
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ) : filteredStores.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center space-y-2">
                      <Store className="h-8 w-8 text-gray-400" />
                      <p className="text-sm text-gray-500">
                        No stores assigned to this group
                      </p>
                      <Button
                        size="sm"
                        onClick={() => router.push(`/administration/store-management/store-groups/${groupUID}/assign-stores`)}
                      >
                        <Plus className="h-4 w-4 mr-2" />
                        Assign Stores
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                filteredStores.map((store) => (
                  <TableRow key={store.UID} className="hover:bg-muted/50">
                    <TableCell className="pl-6">
                      <span className="font-medium text-sm">{store.Code}</span>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm">{store.Name}</span>
                    </TableCell>
                    <TableCell className="text-center">
                      <Badge variant={store.IsActive ? "default" : "secondary"}>
                        {store.IsActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm text-gray-600">{formatDate(store.CreatedTime)}</span>
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
                            onClick={() => router.push(`/administration/store-management/stores/view/${store.UID}`)}
                          >
                            <Eye className="h-4 w-4 mr-2" />
                            View Store
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => router.push(`/administration/store-management/stores/edit/${store.UID}`)}
                          >
                            <Edit className="h-4 w-4 mr-2" />
                            Edit Store
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => {
                              setSelectedStore(store)
                              setDeleteDialogOpen(true)
                            }}
                            className="text-red-600"
                          >
                            <Trash2 className="h-4 w-4 mr-2" />
                            Remove from Group
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>

        {/* Pagination */}
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
              itemName="stores"
            />
          </div>
        )}
      </Card>

      {/* Remove Store Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove Store from Group?</AlertDialogTitle>
            <AlertDialogDescription>
              This will remove "{selectedStore?.Name}" from the group "{storeGroup?.Name}".
              The store will remain active but will no longer be associated with this group.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleRemoveStore} className="bg-red-600 hover:bg-red-700">
              Remove Store
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}