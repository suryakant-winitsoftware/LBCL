'use client';

import React, { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Plus, Search, Filter, Download, Upload, Eye, Edit, Trash2, MoreVertical, ChevronDown, X, Store, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
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
import { PaginationControls } from '@/components/ui/pagination-controls';
import { DataTable } from '@/components/ui/data-table';
import { ColumnDef } from '@tanstack/react-table';
import {
  IStore,
  StoreListRequest,
  PagedResponse,
  STORE_STATUS
} from '@/types/store.types';

export default function ManageStoresPage() {
  const router = useRouter();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [stores, setStores] = useState<IStore[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string[]>([]);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [storeToDelete, setStoreToDelete] = useState<IStore | null>(null);

  // Debounce search term to avoid too many API calls
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
  }, [debouncedSearchTerm, selectedStatus]);

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault();
        searchInputRef.current?.focus();
        searchInputRef.current?.select();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Memoize filter criteria to prevent unnecessary re-renders
  const filterCriteria = useMemo(() => {
    const filters: any[] = [];

    if (debouncedSearchTerm) {
      const looksLikeCode = /^[A-Z0-9_-]{3,}$/i.test(debouncedSearchTerm);

      if (looksLikeCode) {
        filters.push({
          Name: 'Code',
          Value: debouncedSearchTerm,
          Type: 2,
          FilterMode: 0
        });
      } else {
        filters.push({
          Name: 'Name',
          Value: debouncedSearchTerm,
          Type: 2,
          FilterMode: 0
        });
      }
    }

    // Handle multiple status selections
    if (selectedStatus.length > 0) {
      selectedStatus.forEach(status => {
        if (status === 'active') {
          filters.push({
            name: 'IsActive',
            value: true
          });
        } else if (status === 'inactive') {
          filters.push({
            name: 'IsActive',
            value: false
          });
        } else if (status === 'blocked') {
          filters.push({
            name: 'IsBlocked',
            value: true
          });
        }
      });
    }

    return filters;
  }, [debouncedSearchTerm, selectedStatus]);

  // Fetch stores with optimized caching and error handling
  const fetchStores = useCallback(async () => {
    setLoading(true);
    try {
      const request: StoreListRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: filterCriteria,
        sortCriterias: [{ sortParameter: 'Name', direction: 0 }],
        isCountRequired: currentPage === 1
      };

      const response: PagedResponse<IStore> = await storeService.getAllStores(request);
      setStores(response.pagedData || []);

      if (response.totalCount === -1 && totalCount > 0) {
        // Keep existing count if API returns -1
      } else {
        setTotalCount(response.totalCount || 0);
      }
    } catch (error) {
      console.error('Error fetching stores:', error);
      toast.error('Failed to fetch stores. Please try again.');
      setStores([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, filterCriteria]);

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

      switch (action) {
        case 'activate':
          await storeService.activateStore(uid);
          toast.success('Store activated successfully');
          break;
        case 'deactivate':
          await storeService.deactivateStore(uid);
          toast.success('Store deactivated successfully');
          break;
        case 'block':
          await storeService.blockStore(uid);
          toast.success('Store blocked successfully');
          break;
        case 'unblock':
          await storeService.unblockStore(uid);
          toast.success('Store unblocked successfully');
          break;
      }

      fetchStores();
    } catch (error) {
      console.error(`Error ${action}ing store:`, error);
      toast.error(`Failed to ${action} store: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  };

  // Handle export with current filters
  const handleExport = useCallback(async () => {
    try {
      const blob = await storeService.exportStores({
        searchText: debouncedSearchTerm,
        status: selectedStatus.length > 0 ? selectedStatus[0] : ''
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
  }, [debouncedSearchTerm, selectedStatus]);

  // Handle import
  const handleImport = useCallback(() => {
    router.push('/administration/store-management/stores/import');
  }, [router]);

  // Define columns for DataTable
  const columns: ColumnDef<IStore>[] = [
    {
      accessorKey: 'Code',
      header: () => <div className="pl-6">Code</div>,
      cell: ({ row }) => {
        const store = row.original;
        return (
          <div className="pl-6">
            <span className="font-medium text-sm">{store.Code || store.code}</span>
          </div>
        );
      },
    },
    {
      accessorKey: 'Name',
      header: 'Name',
      cell: ({ row }) => {
        const store = row.original;
        return <span className="text-sm">{store.Name || store.name}</span>;
      },
    },
    {
      accessorKey: 'Status',
      header: () => <div className="text-center">Status</div>,
      cell: ({ row }) => {
        const store = row.original;
        const isActive = store.IsActive ?? store.is_active;
        return (
          <div className="text-center">
            <Badge variant={isActive ? 'default' : 'secondary'}>
              {isActive ? 'Active' : 'Inactive'}
            </Badge>
          </div>
        );
      },
    },
    {
      id: 'actions',
      header: () => <div className="text-right pr-6">Actions</div>,
      cell: ({ row }) => {
        const store = row.original;
        const uid = store.UID || store.uid || store.Code || store.code;

        return (
          <div className="text-right pr-6">
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
                  onClick={() => router.push(`/administration/store-management/stores/view/${uid}`)}
                  className="cursor-pointer"
                >
                  <Eye className="mr-2 h-4 w-4" />
                  View Details
                </DropdownMenuItem>
                <DropdownMenuItem
                  onClick={() => router.push(`/administration/store-management/stores/edit/${uid}`)}
                  className="cursor-pointer"
                >
                  <Edit className="mr-2 h-4 w-4" />
                  Edit Store
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                {(store.IsActive ?? store.is_active) ? (
                  <DropdownMenuItem
                    onClick={() => handleStatusChange(store, 'deactivate')}
                    className="cursor-pointer"
                  >
                    Deactivate Store
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    onClick={() => handleStatusChange(store, 'activate')}
                    className="cursor-pointer"
                  >
                    Activate Store
                  </DropdownMenuItem>
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
          </div>
        );
      },
    },
  ];

  return (
    <div className="container mx-auto py-4 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Store Management</h1>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handleImport}>
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="sm" onClick={handleExport}>
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={() => router.push('/administration/store-management/stores/create')} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Store
          </Button>
        </div>
      </div>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by name or code... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Status Dropdown */}
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
                  checked={selectedStatus.includes("active")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev =>
                      checked
                        ? [...prev, "active"]
                        : prev.filter(s => s !== "active")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Active
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedStatus.includes("inactive")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev =>
                      checked
                        ? [...prev, "inactive"]
                        : prev.filter(s => s !== "inactive")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Inactive
                </DropdownMenuCheckboxItem>
                {selectedStatus.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => setSelectedStatus([])}>
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          <DataTable
            columns={columns}
            data={stores}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>

        {stores.length > 0 && (
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