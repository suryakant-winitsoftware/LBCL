'use client';

import React, { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Plus, Search, Filter, Download, Eye, Edit, Trash2, MoreVertical, ChevronDown, X, CreditCard, RefreshCw } from 'lucide-react';
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
import { storeCreditService } from '@/services/storeCreditService';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { DataTable } from '@/components/ui/data-table';
import { ColumnDef } from '@tanstack/react-table';
import {
  IStoreCredit,
  StoreCreditListRequest,
  PagedResponse,
  CreditType
} from '@/types/storeCredit.types';

export default function ManageStoreCreditPage() {
  const router = useRouter();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [storeCredits, setStoreCredits] = useState<IStoreCredit[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [selectedCreditType, setSelectedCreditType] = useState<string[]>([]);
  const [selectedStatus, setSelectedStatus] = useState<string[]>([]);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [storeCreditToDelete, setStoreCreditToDelete] = useState<IStoreCredit | null>(null);

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
  }, [debouncedSearchTerm, selectedCreditType, selectedStatus]);

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
      // Detect if search term looks like a Store Code (alphanumeric with possible hyphens/underscores)
      // or a Price List (usually alphanumeric, could have hyphens)
      // Store Code pattern: typically numeric or short alphanumeric (e.g., "1000", "ST001")
      // Price List pattern: usually contains letters and may have hyphens (e.g., "GLMBEAUTPL", "1001-PL")

      const looksLikeStoreCode = /^\d+$/.test(debouncedSearchTerm); // Pure numbers = Store Code

      if (looksLikeStoreCode) {
        // Search by Store Code (StoreUID field - maps to store_uid column)
        filters.push({
          Name: 'store_uid',
          Value: debouncedSearchTerm,
          Type: 2,
          FilterMode: 0
        });
      } else {
        // Search by Price List (maps to price_list column)
        filters.push({
          Name: 'price_list',
          Value: debouncedSearchTerm,
          Type: 2,
          FilterMode: 0
        });
      }
    }

    // Handle credit type selections
    if (selectedCreditType.length > 0) {
      selectedCreditType.forEach(type => {
        filters.push({
          Name: 'credit_type',
          Value: type.toUpperCase(),
          Type: 2,
          FilterMode: 1
        });
      });
    }

    // Handle status selections
    if (selectedStatus.length > 0) {
      selectedStatus.forEach(status => {
        if (status === 'active') {
          filters.push({
            Name: 'is_active',
            Value: true,
            Type: 0,
            FilterMode: 0
          });
        } else if (status === 'inactive') {
          filters.push({
            Name: 'is_active',
            Value: false,
            Type: 0,
            FilterMode: 0
          });
        } else if (status === 'blocked') {
          filters.push({
            Name: 'is_blocked',
            Value: true,
            Type: 0,
            FilterMode: 0
          });
        }
      });
    }

    return filters;
  }, [debouncedSearchTerm, selectedCreditType, selectedStatus]);

  // Fetch store credits with optimized caching and error handling
  const fetchStoreCredits = useCallback(async () => {
    setLoading(true);
    try {
      const request: StoreCreditListRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: filterCriteria,
        isCountRequired: true
      };

      console.log('Store Credit Request:', JSON.stringify(request, null, 2));

      const response: PagedResponse<IStoreCredit> = await storeCreditService.getAllStoreCredits(request);

      console.log('Store Credit Response:', response);
      console.log('Response keys:', Object.keys(response));
      console.log('pagedData:', response.pagedData);
      console.log('PagedData:', response.PagedData);
      console.log('totalCount:', response.totalCount);
      console.log('TotalCount:', response.TotalCount);

      const credits = response.pagedData || response.PagedData || [];
      console.log('Setting credits:', credits.length, credits);
      setStoreCredits(credits);

      if ((response.totalCount === -1 || response.TotalCount === -1) && totalCount > 0) {
        // Keep existing count if API returns -1
      } else {
        const count = response.totalCount || response.TotalCount || 0;
        console.log('Setting total count:', count);
        setTotalCount(count);
      }
    } catch (error) {
      console.error('Error fetching store credits:', error);
      toast.error('Failed to fetch store credits. Please try again.');
      setStoreCredits([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, filterCriteria]);

  useEffect(() => {
    fetchStoreCredits();
  }, [fetchStoreCredits]);

  // Handle delete
  const handleDelete = async () => {
    if (!storeCreditToDelete) return;

    try {
      const uid = storeCreditToDelete.UID || storeCreditToDelete.uid;
      if (!uid) {
        toast.error('Invalid store credit UID');
        return;
      }
      await storeCreditService.deleteStoreCredit(uid);
      toast.success('Store credit deleted successfully');
      setDeleteDialogOpen(false);
      setStoreCreditToDelete(null);
      fetchStoreCredits();
    } catch (error) {
      console.error('Error deleting store credit:', error);
      toast.error('Failed to delete store credit');
    }
  };

  // Handle export with current filters
  const handleExport = useCallback(async () => {
    try {
      const blob = await storeCreditService.exportStoreCredits({
        searchText: debouncedSearchTerm,
        creditType: selectedCreditType.length > 0 ? selectedCreditType[0] : ''
      });

      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `store_credits_${new Date().toISOString().split('T')[0]}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);

      toast.success('Store credits exported successfully');
    } catch (error) {
      console.error('Error exporting store credits:', error);
      toast.error('Export feature not yet implemented');
    }
  }, [debouncedSearchTerm, selectedCreditType]);

  // Format currency
  const formatCurrency = (amount?: number) => {
    if (amount === undefined || amount === null) return '0.00';
    return amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  };

  // Define columns for DataTable
  const columns: ColumnDef<IStoreCredit>[] = [
    {
      accessorKey: 'StoreUID',
      header: () => <div className="pl-6">Store Code</div>,
      cell: ({ row }) => {
        const storeCredit = row.original;
        return (
          <div className="pl-6">
            <span className="font-medium text-sm">{storeCredit.StoreUID || storeCredit.store_uid}</span>
          </div>
        );
      },
    },
    {
      accessorKey: 'CreditType',
      header: 'Credit Type',
      cell: ({ row }) => {
        const storeCredit = row.original;
        const creditType = storeCredit.CreditType || storeCredit.credit_type;
        return (
          <Badge variant={creditType === 'CREDIT' ? 'default' : creditType === 'CASH' ? 'secondary' : 'outline'}>
            {creditType}
          </Badge>
        );
      },
    },
    {
      accessorKey: 'CreditLimit',
      header: 'Credit Limit',
      cell: ({ row }) => {
        const storeCredit = row.original;
        const creditLimit = storeCredit.CreditLimit ?? storeCredit.credit_limit;
        return <span className="text-sm">{formatCurrency(creditLimit)}</span>;
      },
    },
    {
      accessorKey: 'PriceList',
      header: 'Price List',
      cell: ({ row }) => {
        const storeCredit = row.original;
        const priceList = storeCredit.PriceList || storeCredit.price_list;
        return <span className="text-sm">{priceList || 'N/A'}</span>;
      },
    },
    {
      accessorKey: 'PaymentTermLabel',
      header: () => <div className="text-center">Payment Term</div>,
      cell: ({ row }) => {
        const storeCredit = row.original;
        const paymentTerm = storeCredit.PaymentTermLabel || storeCredit.payment_term_label || storeCredit.PaymentTermUID || storeCredit.payment_term_uid;
        return <div className="text-center text-sm">{paymentTerm || 'N/A'}</div>;
      },
    },
    {
      accessorKey: 'Status',
      header: 'Status',
      cell: ({ row }) => {
        const storeCredit = row.original;
        const isActive = storeCredit.IsActive ?? storeCredit.is_active;
        const isBlocked = storeCredit.IsBlocked ?? storeCredit.is_blocked;

        if (isBlocked) {
          return <Badge variant="destructive">Blocked</Badge>;
        }

        return (
          <Badge variant={isActive ? 'default' : 'secondary'}>
            {isActive ? 'Active' : 'Inactive'}
          </Badge>
        );
      },
    },
    {
      id: 'actions',
      header: () => <div className="text-right pr-6">Actions</div>,
      cell: ({ row }) => {
        const storeCredit = row.original;
        const uid = storeCredit.UID || storeCredit.uid;

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
                  onClick={() => router.push(`/administration/store-management/store-credit/view/${uid}`)}
                  className="cursor-pointer"
                >
                  <Eye className="mr-2 h-4 w-4" />
                  View Details
                </DropdownMenuItem>
                <DropdownMenuItem
                  onClick={() => router.push(`/administration/store-management/store-credit/edit/${uid}`)}
                  className="cursor-pointer"
                >
                  <Edit className="mr-2 h-4 w-4" />
                  Edit Credit
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onClick={() => {
                    setStoreCreditToDelete(storeCredit);
                    setDeleteDialogOpen(true);
                  }}
                  className="cursor-pointer text-red-600 focus:text-red-600"
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete Credit
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
        <div className="flex items-center gap-2">
          <CreditCard className="h-6 w-6" />
          <h1 className="text-2xl font-bold">Store Credit Management</h1>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handleExport}>
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button variant="outline" size="sm" onClick={fetchStoreCredits}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button onClick={() => router.push('/administration/store-management/store-credit/create')} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Store Credit
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
                placeholder="Search by Store Code or Price List... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Credit Type Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Credit Type
                  {selectedCreditType.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedCreditType.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Credit Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={selectedCreditType.includes("CASH")}
                  onCheckedChange={(checked) => {
                    setSelectedCreditType(prev =>
                      checked
                        ? [...prev, "CASH"]
                        : prev.filter(s => s !== "CASH")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Cash
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedCreditType.includes("CREDIT")}
                  onCheckedChange={(checked) => {
                    setSelectedCreditType(prev =>
                      checked
                        ? [...prev, "CREDIT"]
                        : prev.filter(s => s !== "CREDIT")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Credit
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedCreditType.includes("MIXED")}
                  onCheckedChange={(checked) => {
                    setSelectedCreditType(prev =>
                      checked
                        ? [...prev, "MIXED"]
                        : prev.filter(s => s !== "MIXED")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Mixed
                </DropdownMenuCheckboxItem>
                {selectedCreditType.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => setSelectedCreditType([])}>
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>

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
                <DropdownMenuCheckboxItem
                  checked={selectedStatus.includes("blocked")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev =>
                      checked
                        ? [...prev, "blocked"]
                        : prev.filter(s => s !== "blocked")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Blocked
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
            data={storeCredits}
            loading={loading}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />
        </div>

        {storeCredits.length > 0 && (
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
              itemName="store credits"
            />
          </div>
        )}
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Store Credit</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete store credit for store "{storeCreditToDelete?.StoreUID || storeCreditToDelete?.store_uid}"? This action cannot be undone.
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