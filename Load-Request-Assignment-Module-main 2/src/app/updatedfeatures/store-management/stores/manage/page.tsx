'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Plus, Search, Filter, Download, Upload, Eye, Edit, Trash2, RefreshCw, MoreVertical } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { toast } from 'sonner';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { storeService } from '@/services/storeService';
import {
  IStore,
  StoreListRequest,
  PagedResponse,
  STORE_TYPES,
  STORE_CLASSIFICATIONS,
  STORE_STATUS
} from '@/types/store.types';

export default function ManageStoresPage() {
  const router = useRouter();
  const [stores, setStores] = useState<IStore[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState<string>('');
  const [selectedClassification, setSelectedClassification] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [storeToDelete, setStoreToDelete] = useState<IStore | null>(null);

  // Fetch stores
  const fetchStores = useCallback(async () => {
    setLoading(true);
    try {
      const request: StoreListRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: [],
        sortCriterias: [{ sortParameter: 'Name', direction: 0 }],
        isCountRequired: true
      };

      // Add filters (using camelCase for JSON serialization)
      if (searchTerm) {
        request.filterCriterias?.push({
          name: 'Name',
          value: searchTerm,
          operator: 'contains'
        });
      }

      if (selectedType) {
        request.filterCriterias?.push({
          name: 'Type',
          value: selectedType
        });
      }

      if (selectedClassification) {
        request.filterCriterias?.push({
          name: 'FranchiseeOrgUID',
          value: selectedClassification
        });
      }

      if (selectedStatus) {
        if (selectedStatus === 'active') {
          request.filterCriterias?.push({
            name: 'IsActive',
            value: true
          });
        } else if (selectedStatus === 'inactive') {
          request.filterCriterias?.push({
            name: 'IsActive',
            value: false
          });
        } else if (selectedStatus === 'blocked') {
          request.filterCriterias?.push({
            name: 'IsBlocked',
            value: true
          });
        }
      }

      const response: PagedResponse<IStore> = await storeService.getAllStores(request);
      setStores(response.pagedData || []);
      setTotalCount(response.totalCount || 0);
    } catch (error) {
      console.error('Error fetching stores:', error);
      toast.error('Failed to fetch stores');
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, searchTerm, selectedType, selectedClassification, selectedStatus]);

  useEffect(() => {
    fetchStores();
  }, [fetchStores]);

  // Handle delete
  const handleDelete = async () => {
    if (!storeToDelete) return;

    try {
      const uid = storeToDelete.UID || storeToDelete.uid || storeToDelete.Code || storeToDelete.code;
      await storeService.deleteStore(uid);
      toast.success('Store deleted successfully');
      setDeleteDialogOpen(false);
      setStoreToDelete(null);
      fetchStores();
    } catch (error) {
      console.error('Error deleting store:', error);
      toast.error('Failed to delete store');
    }
  };

  // Handle status changes
  const handleStatusChange = async (store: IStore, action: 'activate' | 'deactivate' | 'block' | 'unblock') => {
    try {
      const uid = store.UID || store.uid || store.Code || store.code;
      console.log(`🔄 Store Status Change: ${action}ing store ${uid}`, { store, action });
      
      switch (action) {
        case 'activate':
          console.log('📞 Calling storeService.activateStore...');
          await storeService.activateStore(uid);
          toast.success('Store activated successfully');
          break;
        case 'deactivate':
          console.log('📞 Calling storeService.deactivateStore...');
          await storeService.deactivateStore(uid);
          toast.success('Store deactivated successfully');
          break;
        case 'block':
          console.log('📞 Calling storeService.blockStore...');
          await storeService.blockStore(uid);
          toast.success('Store blocked successfully');
          break;
        case 'unblock':
          console.log('📞 Calling storeService.unblockStore...');
          await storeService.unblockStore(uid);
          toast.success('Store unblocked successfully');
          break;
      }
      
      console.log('✅ Status change completed, refreshing store list...');
      fetchStores();
    } catch (error) {
      console.error(`❌ Error ${action}ing store:`, error);
      toast.error(`Failed to ${action} store: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  };

  // Handle export
  const handleExport = async () => {
    try {
      const blob = await storeService.exportStores({
        searchText: searchTerm,
        type: selectedType,
        organization: selectedClassification,
        status: selectedStatus
      });
      
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `stores_${new Date().toISOString().split('T')[0]}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
      
      toast.success('Stores exported successfully');
    } catch (error) {
      console.error('Error exporting stores:', error);
      toast.error('Failed to export stores');
    }
  };

  // Handle import
  const handleImport = () => {
    router.push('/updatedfeatures/store-management/stores/import');
  };

  // Reset filters
  const resetFilters = () => {
    setSearchTerm('');
    setSelectedType('');
    setSelectedClassification('');
    setSelectedStatus('');
    setCurrentPage(1);
  };

  // Get status badge color
  const getStatusBadgeVariant = (status: string) => {
    switch (status) {
      case 'Approved':
      case 'Active':
        return 'default';
      case 'Pending':
        return 'secondary';
      case 'Blocked':
      case 'Inactive':
        return 'destructive';
      default:
        return 'outline';
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Store Management</h1>
          <p className="text-muted-foreground">Manage customer outlets and stores</p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={handleImport}
            className="flex items-center gap-2"
          >
            <Upload className="h-4 w-4" />
            Import
          </Button>
          <Button
            variant="outline"
            onClick={handleExport}
            className="flex items-center gap-2"
          >
            <Download className="h-4 w-4" />
            Export
          </Button>
          <Button
            onClick={() => router.push('/updatedfeatures/store-management/stores/create')}
            className="flex items-center gap-2"
          >
            <Plus className="h-4 w-4" />
            Create Store
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filters
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search stores..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9"
              />
            </div>
            <Select value={selectedType || "all"} onValueChange={(value) => setSelectedType(value === "all" ? "" : value)}>
              <SelectTrigger>
                <SelectValue placeholder="All Types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                {Object.entries(STORE_TYPES).map(([key, value]) => (
                  <SelectItem key={key} value={key}>
                    {value}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={selectedClassification || "all"} onValueChange={(value) => setSelectedClassification(value === "all" ? "" : value)}>
              <SelectTrigger>
                <SelectValue placeholder="All Organizations" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Organizations</SelectItem>
                <SelectItem value="Farmley">Farmley</SelectItem>
                <SelectItem value="7Eleven">7Eleven</SelectItem>
              </SelectContent>
            </Select>
            <Select value={selectedStatus || "all"} onValueChange={(value) => setSelectedStatus(value === "all" ? "" : value)}>
              <SelectTrigger>
                <SelectValue placeholder="All Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="inactive">Inactive</SelectItem>
                <SelectItem value="blocked">Blocked</SelectItem>
              </SelectContent>
            </Select>
            <Button
              variant="outline"
              onClick={resetFilters}
              className="flex items-center gap-2"
            >
              <RefreshCw className="h-4 w-4" />
              Reset
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Stores Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex justify-between items-center">
            <span>Stores ({totalCount})</span>
            <div className="flex items-center gap-2">
              <span className="text-sm font-normal">Show:</span>
              <Select value={pageSize.toString()} onValueChange={(v) => setPageSize(Number(v))}>
                <SelectTrigger className="w-20">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="10">10</SelectItem>
                  <SelectItem value="20">20</SelectItem>
                  <SelectItem value="50">50</SelectItem>
                  <SelectItem value="100">100</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Organization</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Location</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  // Loading skeletons
                  Array.from({ length: 5 }).map((_, index) => (
                    <TableRow key={index}>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-28" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    </TableRow>
                  ))
                ) : stores && stores.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-8">
                      No stores found
                    </TableCell>
                  </TableRow>
                ) : stores ? (
                  stores.map((store, index) => (
                    <TableRow key={store.UID || store.uid || store.Code || store.code || `store-${index}`}>
                      <TableCell className="font-medium">{store.Code || store.code}</TableCell>
                      <TableCell>{store.Name || store.name}</TableCell>
                      <TableCell>{STORE_TYPES[store.Type || store.type as keyof typeof STORE_TYPES] || store.Type || store.type}</TableCell>
                      <TableCell>{store.FranchiseeOrgUID || '-'}</TableCell>
                      <TableCell>
                        <Badge variant={(store.IsActive ?? store.is_active) ? 'default' : (store.IsBlocked ?? store.is_blocked) ? 'destructive' : 'secondary'}>
                          {(store.IsBlocked ?? store.is_blocked) ? 'Blocked' : (store.IsActive ?? store.is_active) ? 'Active' : 'Inactive'}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-sm">
                        {[store.CityUID || store.city_uid, store.RegionUID || store.region_uid]
                          .filter(Boolean)
                          .join(', ') || '-'}
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" className="h-8 w-8 p-0">
                              <span className="sr-only">Open menu</span>
                              <MoreVertical className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuItem
                              onClick={() => {
                                const uid = store.UID || store.uid || store.Code || store.code;
                                console.log('🔍 Navigating to view store:', uid);
                                router.push(`/updatedfeatures/store-management/stores/view/${uid}`);
                              }}
                              className="cursor-pointer"
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => {
                                const uid = store.UID || store.uid || store.Code || store.code;
                                console.log('✏️ Navigating to edit store:', uid);
                                router.push(`/updatedfeatures/store-management/stores/edit/${uid}`);
                              }}
                              className="cursor-pointer"
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit Store
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            {(store.IsBlocked ?? store.is_blocked) ? (
                              <DropdownMenuItem 
                                onClick={() => handleStatusChange(store, 'unblock')}
                                className="cursor-pointer"
                              >
                                <Badge variant="destructive" className="mr-2">Blocked</Badge>
                                Unblock Store
                              </DropdownMenuItem>
                            ) : (store.IsActive ?? store.is_active) ? (
                              <DropdownMenuItem 
                                onClick={() => handleStatusChange(store, 'deactivate')}
                                className="cursor-pointer"
                              >
                                <Badge variant="default" className="mr-2">Active</Badge>
                                Deactivate Store
                              </DropdownMenuItem>
                            ) : (
                              <>
                                <DropdownMenuItem 
                                  onClick={() => handleStatusChange(store, 'activate')}
                                  className="cursor-pointer"
                                >
                                  <Badge variant="secondary" className="mr-2">Inactive</Badge>
                                  Activate Store
                                </DropdownMenuItem>
                                <DropdownMenuItem 
                                  onClick={() => handleStatusChange(store, 'block')}
                                  className="cursor-pointer text-red-600 focus:text-red-600"
                                >
                                  <Badge variant="destructive" className="mr-2">Block</Badge>
                                  Block Store
                                </DropdownMenuItem>
                              </>
                            )}
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() => {
                                setStoreToDelete(store);
                                setDeleteDialogOpen(true);
                              }}
                              className="cursor-pointer text-red-600 focus:text-red-600"
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete Store
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))
                ) : null}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between mt-4">
              <div className="text-sm text-muted-foreground">
                Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, totalCount)} of {totalCount} stores
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                >
                  Previous
                </Button>
                <div className="flex items-center gap-1">
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    const page = i + 1;
                    return (
                      <Button
                        key={page}
                        variant={currentPage === page ? 'default' : 'outline'}
                        size="sm"
                        onClick={() => setCurrentPage(page)}
                      >
                        {page}
                      </Button>
                    );
                  })}
                  {totalPages > 5 && <span className="px-2">...</span>}
                  {totalPages > 5 && (
                    <Button
                      variant={currentPage === totalPages ? 'default' : 'outline'}
                      size="sm"
                      onClick={() => setCurrentPage(totalPages)}
                    >
                      {totalPages}
                    </Button>
                  )}
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Store</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete store "{storeToDelete?.Name || storeToDelete?.name}"? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}