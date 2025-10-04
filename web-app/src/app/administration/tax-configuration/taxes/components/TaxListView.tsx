'use client';

import React, { useState, useEffect } from 'react';
import { taxService, ITax, PagingRequest } from '@/services/tax/tax.service';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Search, RefreshCw, Eye, ChevronLeft, ChevronRight, Plus, Trash2, Edit, MoreHorizontal } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useToast } from '@/components/ui/use-toast';

interface TaxListViewProps {
  // Props for potential future use
}

const TaxListView: React.FC<TaxListViewProps> = () => {
  const router = useRouter();
  const { toast } = useToast();
  const [taxes, setTaxes] = useState<ITax[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(10);

  // Debounce search term to avoid too many API calls
  useEffect(() => {
    console.log('=== SEARCH TERM CHANGE ===');
    console.log('New search term:', searchTerm);
    
    const timer = setTimeout(() => {
      console.log('=== DEBOUNCED SEARCH ===');
      console.log('Setting debounced term to:', searchTerm);
      setDebouncedSearchTerm(searchTerm);
      setCurrentPage(1); // Reset to first page when search changes
    }, 300);
    
    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    loadTaxes();
  }, [currentPage, debouncedSearchTerm]);

  const loadTaxes = async () => {
    setLoading(true);
    try {
      const request: PagingRequest = {
        pageNumber: currentPage - 1, // Convert to 0-based indexing
        pageSize: pageSize,
        isCountRequired: true,
        filterCriterias: debouncedSearchTerm
          ? [
              {
                name: 'Name',
                value: debouncedSearchTerm,
                operator: 'Contains',
              }
            ]
          : [],
        sortCriterias: [
          {
            sortParameter: 'Name',
            direction: 'Asc',
          },
        ],
      };

      console.log('=== TAX SEARCH DEBUG ===');
      console.log('Search term:', debouncedSearchTerm);
      console.log('Request payload:', JSON.stringify(request, null, 2));

      const response = await taxService.getTaxDetails(request);
      
      console.log('=== TAX SEARCH RESPONSE ===');
      console.log('Response:', {
        PagedData: response?.PagedData?.length,
        TotalCount: response?.TotalCount,
        HasData: !!response?.PagedData
      });
      setTaxes(response.PagedData);
      setTotalCount(response.TotalCount);
    } catch (error) {
      console.error('=== TAX SEARCH ERROR ===');
      console.error('Error loading taxes:', error);
      console.error('Request that failed:', request);
      
      toast({
        title: 'Error',
        description: 'Failed to load taxes. Please try again.',
        variant: 'destructive',
      });
      
      // Set empty data on error
      setTaxes([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  };

  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    // Debounced search and page reset will happen automatically via useEffect
  };

  const handleDelete = async (uid: string, name: string) => {
    if (confirm(`Are you sure you want to delete tax "${name}"?\n\nNote: If this tax is assigned to tax groups or mapped to SKUs, you'll need to remove those associations first.`)) {
      try {
        await taxService.deleteTax(uid);
        toast({
          title: 'Success',
          description: 'Tax deleted successfully',
        });
        loadTaxes(); // Reload the list
      } catch (error: any) {
        console.error('Error deleting tax:', error);
        toast({
          title: 'Cannot Delete Tax',
          description: error.message || 'Failed to delete tax',
          variant: 'destructive',
        });
      }
    }
  };

  const formatTaxRate = (rate: number) => {
    return `${rate}%`;
  };

  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString();
  };

  const getStatusBadge = (status: string | undefined) => {
    const variant = status === 'Active' ? 'default' : 'secondary';
    return <Badge variant={variant}>{status || 'Unknown'}</Badge>;
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="space-y-4">
      <CardHeader className="px-0 pt-0">
        <div className="flex items-center justify-between">
          <CardTitle>Tax List</CardTitle>
          <div className="flex gap-2">
            <Button
              onClick={() => router.push('/administration/tax-configuration/taxes/create')}
              disabled={loading}
            >
              <Plus className="h-4 w-4 mr-2" />
              Create Tax
            </Button>
            <Button
              onClick={() => router.push('/administration/tax-configuration/taxes/create-master')}
              disabled={loading}
              variant="outline"
            >
              <Plus className="h-4 w-4 mr-2" />
              Create Tax Master
            </Button>
            <Button
              onClick={loadTaxes}
              variant="outline"
              size="icon"
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </div>
      </CardHeader>

      <CardContent className="px-0">
        {/* Search Bar */}
        <div className="flex gap-2 mb-4">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              type="text"
              placeholder="Search by tax name or code... (auto-search enabled)"
              value={searchTerm}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="pl-10"
            />
          </div>
          {debouncedSearchTerm && (
            <Button 
              variant="outline" 
              onClick={() => handleSearchChange('')}
              disabled={loading}
            >
              Clear
            </Button>
          )}
        </div>

        {/* Tax Table */}
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Code</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Legal Name</TableHead>
                <TableHead>Applicable At</TableHead>
                <TableHead>Type</TableHead>
                <TableHead className="text-right">Tax Rate</TableHead>
                <TableHead>Tax on Tax</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Valid From</TableHead>
                <TableHead>Valid To</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={11} className="text-center py-8">
                    <div className="flex items-center justify-center">
                      <RefreshCw className="h-6 w-6 animate-spin mr-2" />
                      Loading taxes...
                    </div>
                  </TableCell>
                </TableRow>
              ) : taxes.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={11} className="text-center py-8">
                    {debouncedSearchTerm ? (
                      <div>
                        <p>No taxes found for "{debouncedSearchTerm}"</p>
                        <Button 
                          variant="link" 
                          onClick={() => handleSearchChange('')}
                          className="text-sm mt-2"
                        >
                          Clear search to view all taxes
                        </Button>
                      </div>
                    ) : (
                      'No taxes found'
                    )}
                  </TableCell>
                </TableRow>
              ) : (
                taxes.map((tax) => (
                  <TableRow key={tax.UID}>
                    <TableCell className="font-mono text-sm">
                      {tax.Code}
                    </TableCell>
                    <TableCell className="font-medium">{tax.Name}</TableCell>
                    <TableCell>{tax.LegalName || '-'}</TableCell>
                    <TableCell>
                      <Badge variant="secondary">
                        {tax.ApplicableAt || 'Not Set'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline">
                        {tax.TaxCalculationType || 'Percentage'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right font-medium">
                      {formatTaxRate(tax.BaseTaxRate)}
                    </TableCell>
                    <TableCell>
                      <Badge variant={tax.IsTaxOnTaxApplicable ? "default" : "secondary"}>
                        {tax.IsTaxOnTaxApplicable ? 'Yes' : 'No'}
                      </Badge>
                    </TableCell>
                    <TableCell>{getStatusBadge(tax.Status)}</TableCell>
                    <TableCell>{formatDate(tax.ValidFrom)}</TableCell>
                    <TableCell>{formatDate(tax.ValidUpto)}</TableCell>
                    <TableCell className="text-right">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem
                            onClick={() => router.push(`/administration/tax-configuration/taxes/details/${tax.UID}`)}
                          >
                            <Eye className="mr-2 h-4 w-4" />
                            View Details
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => router.push(`/administration/tax-configuration/taxes/edit/${tax.UID}`)}
                          >
                            <Edit className="mr-2 h-4 w-4" />
                            Edit Tax
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => handleDelete(tax.UID, tax.Name)}
                            className="text-red-600 focus:text-red-600"
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete Tax
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
        {totalPages > 1 && (
          <div className="flex items-center justify-between mt-4">
            <div className="text-sm text-gray-600">
              Showing {(currentPage - 1) * pageSize + 1} to{' '}
              {Math.min(currentPage * pageSize, totalCount)} of {totalCount} taxes
              {debouncedSearchTerm && (
                <span className="text-blue-600 ml-1">
                  (filtered by "{debouncedSearchTerm}")
                </span>
              )}
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage((prev) => Math.max(1, prev - 1))}
                disabled={currentPage === 1 || loading}
              >
                <ChevronLeft className="h-4 w-4" />
                Previous
              </Button>
              <div className="text-sm">
                Page {currentPage} of {totalPages}
              </div>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage((prev) => Math.min(totalPages, prev + 1))}
                disabled={currentPage === totalPages || loading}
              >
                Next
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </div>
  );
};

export default TaxListView;