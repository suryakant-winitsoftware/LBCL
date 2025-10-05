'use client';

import React, { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Plus, Search, Filter, Download, Eye, Edit, MoreVertical, Building2, RefreshCw } from 'lucide-react';
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
import { Skeleton } from '@/components/ui/skeleton';
import { distributorService, IDistributor } from '@/services/distributor.service';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { PagingRequest } from '@/types/common.types';

export default function ManageDistributorsPage() {
  const router = useRouter();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [distributors, setDistributors] = useState<IDistributor[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string[]>([]);

  // Debounce search term
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

  // Keyboard shortcut for search
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

  // Memoize filter criteria
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

    if (selectedStatus.length > 0) {
      selectedStatus.forEach(status => {
        filters.push({
          Name: 'Status',
          Value: status,
          Type: 1,
          FilterMode: 0
        });
      });
    }

    return filters;
  }, [debouncedSearchTerm, selectedStatus]);

  // Fetch distributors
  const fetchDistributors = useCallback(async () => {
    setLoading(true);
    try {
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: filterCriteria,
        SortCriterias: [{ SortParameter: 'Name', Direction: 0 }],
        IsCountRequired: true
      };

      const response = await distributorService.getAllDistributors(request);
      setDistributors(response.PagedData || []);
      setTotalCount(response.TotalCount || 0);
    } catch (error) {
      console.error('Error fetching distributors:', error);
      toast.error('Failed to load distributors');
      setDistributors([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, filterCriteria]);

  useEffect(() => {
    fetchDistributors();
  }, [fetchDistributors]);

  const handleExport = async (format: 'csv' | 'excel') => {
    try {
      toast.info(`Exporting to ${format.toUpperCase()}...`);
      const blob = format === 'csv'
        ? await distributorService.exportToCSV(searchTerm)
        : await distributorService.exportToExcel(searchTerm);

      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `distributors-${new Date().toISOString().split('T')[0]}.${format === 'csv' ? 'csv' : 'xlsx'}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success(`Exported successfully`);
    } catch (error) {
      console.error('Export failed:', error);
      toast.error('Failed to export distributors');
    }
  };

  const getStatusBadgeVariant = (status?: string) => {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'default';
      case 'inactive':
        return 'secondary';
      case 'blocked':
        return 'destructive';
      default:
        return 'outline';
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Distributors</h1>
          <p className="text-muted-foreground">
            Manage your distributor network
          </p>
        </div>
        <Button onClick={() => router.push('/administration/distributor-management/distributors/create')}>
          <Plus className="mr-2 h-4 w-4" />
          Add Distributor
        </Button>
      </div>

      {/* Filters and Search */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex flex-1 items-center gap-2">
              <div className="relative flex-1 max-w-sm">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  ref={searchInputRef}
                  placeholder="Search by name or code..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-9"
                />
              </div>

              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" size="icon">
                    <Filter className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-48">
                  <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  {['Active', 'Inactive', 'Blocked'].map((status) => (
                    <DropdownMenuCheckboxItem
                      key={status}
                      checked={selectedStatus.includes(status)}
                      onCheckedChange={(checked) => {
                        setSelectedStatus(prev =>
                          checked
                            ? [...prev, status]
                            : prev.filter(s => s !== status)
                        );
                      }}
                    >
                      {status}
                    </DropdownMenuCheckboxItem>
                  ))}
                </DropdownMenuContent>
              </DropdownMenu>

              <Button
                variant="outline"
                size="icon"
                onClick={fetchDistributors}
                title="Refresh"
              >
                <RefreshCw className="h-4 w-4" />
              </Button>
            </div>

            <div className="flex items-center gap-2">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline">
                    <Download className="mr-2 h-4 w-4" />
                    Export
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => handleExport('csv')}>
                    Export as CSV
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => handleExport('excel')}>
                    Export as Excel
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </div>

          {(selectedStatus.length > 0 || debouncedSearchTerm) && (
            <div className="mt-4 flex flex-wrap items-center gap-2">
              <span className="text-sm text-muted-foreground">Active filters:</span>
              {debouncedSearchTerm && (
                <Badge variant="secondary">
                  Search: {debouncedSearchTerm}
                </Badge>
              )}
              {selectedStatus.map(status => (
                <Badge key={status} variant="secondary">
                  Status: {status}
                </Badge>
              ))}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setSearchTerm('');
                  setSelectedStatus([]);
                }}
              >
                Clear all
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Table */}
      <Card>
        <CardContent className="p-0">
          <div className="relative">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Contact Person</TableHead>
                  <TableHead>Contact Number</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Open Account Date</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  Array.from({ length: pageSize }).map((_, index) => (
                    <TableRow key={index}>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-28" /></TableCell>
                      <TableCell><Skeleton className="h-6 w-16" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-8 w-8 ml-auto" /></TableCell>
                    </TableRow>
                  ))
                ) : distributors.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="h-24 text-center">
                      <div className="flex flex-col items-center justify-center text-muted-foreground">
                        <Building2 className="h-8 w-8 mb-2" />
                        <p>No distributors found</p>
                        {(debouncedSearchTerm || selectedStatus.length > 0) && (
                          <p className="text-sm mt-1">Try adjusting your filters</p>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  distributors.map((distributor) => (
                    <TableRow key={distributor.UID}>
                      <TableCell className="font-medium">{distributor.Code}</TableCell>
                      <TableCell>{distributor.Name}</TableCell>
                      <TableCell>{distributor.ContactPerson || '-'}</TableCell>
                      <TableCell>{distributor.ContactNumber || '-'}</TableCell>
                      <TableCell>
                        <Badge variant={getStatusBadgeVariant(distributor.Status)}>
                          {distributor.Status || 'Unknown'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {distributor.OpenAccountDate
                          ? new Date(distributor.OpenAccountDate).toLocaleDateString()
                          : '-'}
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreVertical className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => router.push(`/administration/distributor-management/distributors/view/${distributor.UID}`)}
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              View
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => router.push(`/administration/distributor-management/distributors/edit/${distributor.UID}`)}
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit
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
          {!loading && totalCount > 0 && (
            <div className="border-t p-4">
              <PaginationControls
                currentPage={currentPage}
                totalPages={totalPages}
                pageSize={pageSize}
                totalCount={totalCount}
                onPageChange={setCurrentPage}
                onPageSizeChange={(newSize) => {
                  setPageSize(newSize);
                  setCurrentPage(1);
                }}
              />
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
