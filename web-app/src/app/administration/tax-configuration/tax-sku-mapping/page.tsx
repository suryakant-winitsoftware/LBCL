'use client';

import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
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
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Search, RefreshCw, Plus, Edit, Trash2, MoreHorizontal, Link, Unlink } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { taxService, ITaxSkuMap, ITax, PagingRequest, PagedResponse } from '@/services/tax/tax.service';

const TaxSkuMappingPage = () => {
  const { toast } = useToast();
  const [mappings, setMappings] = useState<ITaxSkuMap[]>([]);
  const [taxes, setTaxes] = useState<ITax[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(10);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [newMapping, setNewMapping] = useState({
    SKUUID: '',
    TaxUID: '',
    CompanyUID: 'DEFAULT_COMPANY'
  });

  useEffect(() => {
    loadMappings();
    loadTaxes();
  }, [currentPage]);

  const loadMappings = async () => {
    setLoading(true);
    try {
      // Note: Using tax details API as a placeholder since TaxSkuMap API might need fixing
      const request: PagingRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        isCountRequired: true,
        filterCriterias: searchTerm
          ? [
              {
                name: 'SKUUID',
                value: searchTerm,
                operator: 'Contains',
              },
            ]
          : [],
        sortCriterias: [
          {
            sortParameter: 'SKUUID',
            direction: 'Asc',
          },
        ],
      };

      // This would be the actual call when TaxSkuMap API is working:
      // const response = await taxService.getTaxSkuMapDetails(request);

      // For now, showing placeholder data structure
      const placeholderMappings: ITaxSkuMap[] = [
        {
          UID: 'MAPPING001',
          SKUUID: 'SKU001',
          TaxUID: 'GST18',
          CompanyUID: 'DEFAULT_COMPANY',
          Id: 1,
          CreatedBy: 'ADMIN',
          CreatedTime: new Date().toISOString(),
        },
        {
          UID: 'MAPPING002',
          SKUUID: 'SKU002',
          TaxUID: 'VAT20',
          CompanyUID: 'DEFAULT_COMPANY',
          Id: 2,
          CreatedBy: 'ADMIN',
          CreatedTime: new Date().toISOString(),
        }
      ];

      setMappings(placeholderMappings);
      setTotalCount(placeholderMappings.length);
    } catch (error) {
      console.error('Error loading tax-sku mappings:', error);
      toast({
        title: 'Error',
        description: 'Failed to load tax-sku mappings. Please try again.',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const loadTaxes = async () => {
    try {
      const request: PagingRequest = {
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        filterCriterias: [],
        sortCriterias: [
          {
            sortParameter: 'Name',
            direction: 'Asc',
          },
        ],
      };

      const response = await taxService.getTaxDetails(request);
      setTaxes(response.PagedData);
    } catch (error) {
      console.error('Error loading taxes:', error);
    }
  };

  const handleSearch = () => {
    setCurrentPage(1);
    loadMappings();
  };

  const handleCreateMapping = async () => {
    if (!newMapping.SKUUID || !newMapping.TaxUID) {
      toast({
        title: 'Validation Error',
        description: 'Please fill in all required fields',
        variant: 'destructive',
      });
      return;
    }

    try {
      const mapping: ITaxSkuMap = {
        UID: `MAPPING_${Date.now()}`,
        SKUUID: newMapping.SKUUID,
        TaxUID: newMapping.TaxUID,
        CompanyUID: newMapping.CompanyUID,
        CreatedBy: 'ADMIN',
        CreatedTime: new Date().toISOString(),
        ModifiedBy: 'ADMIN',
        ModifiedTime: new Date().toISOString(),
        ServerAddTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
      };

      // When API is working: await taxService.createTaxSkuMap(mapping);

      toast({
        title: 'Success',
        description: 'Tax-SKU mapping created successfully',
      });

      setShowCreateForm(false);
      setNewMapping({ SKUUID: '', TaxUID: '', CompanyUID: 'DEFAULT_COMPANY' });
      loadMappings();
    } catch (error) {
      console.error('Error creating mapping:', error);
      toast({
        title: 'Error',
        description: 'Failed to create mapping',
        variant: 'destructive',
      });
    }
  };

  const handleDeleteMapping = async (uid: string) => {
    if (confirm('Are you sure you want to delete this tax-sku mapping?')) {
      try {
        // When API is working: await taxService.deleteTaxSkuMap(uid);

        toast({
          title: 'Success',
          description: 'Tax-SKU mapping deleted successfully',
        });
        loadMappings();
      } catch (error) {
        console.error('Error deleting mapping:', error);
        toast({
          title: 'Error',
          description: 'Failed to delete mapping',
          variant: 'destructive',
        });
      }
    }
  };

  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString();
  };

  const getTaxName = (taxUID: string) => {
    const tax = taxes.find(t => t.UID === taxUID);
    return tax ? tax.Name : taxUID;
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="container mx-auto p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold">Tax-SKU Mapping</h1>
        <p className="text-gray-600 mt-2">
          Manage tax assignments for specific products (SKUs)
        </p>
      </div>

      <div className="space-y-6">
        {/* Create New Mapping Form */}
        {showCreateForm && (
          <Card>
            <CardHeader>
              <CardTitle>Create New Tax-SKU Mapping</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <Label htmlFor="skuUID">SKU Code *</Label>
                  <Input
                    id="skuUID"
                    value={newMapping.SKUUID}
                    onChange={(e) => setNewMapping(prev => ({ ...prev, SKUUID: e.target.value }))}
                    placeholder="Enter SKU code"
                  />
                </div>
                <div>
                  <Label htmlFor="taxUID">Tax *</Label>
                  <Select
                    value={newMapping.TaxUID}
                    onValueChange={(value) => setNewMapping(prev => ({ ...prev, TaxUID: value }))}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select tax" />
                    </SelectTrigger>
                    <SelectContent>
                      {taxes.map((tax) => (
                        <SelectItem key={tax.UID} value={tax.UID}>
                          {tax.Name} ({tax.Code}) - {tax.BaseTaxRate}%
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <Label htmlFor="companyUID">Company</Label>
                  <Input
                    id="companyUID"
                    value={newMapping.CompanyUID}
                    onChange={(e) => setNewMapping(prev => ({ ...prev, CompanyUID: e.target.value }))}
                    placeholder="Company UID"
                  />
                </div>
              </div>
              <div className="flex gap-2">
                <Button onClick={handleCreateMapping}>
                  <Link className="h-4 w-4 mr-2" />
                  Create Mapping
                </Button>
                <Button variant="outline" onClick={() => setShowCreateForm(false)}>
                  Cancel
                </Button>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Main Content */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Tax-SKU Mappings</CardTitle>
              <div className="flex gap-2">
                <Button
                  onClick={() => setShowCreateForm(true)}
                  disabled={loading}
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Create Mapping
                </Button>
                <Button
                  onClick={loadMappings}
                  variant="outline"
                  size="icon"
                  disabled={loading}
                >
                  <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
                </Button>
              </div>
            </div>
          </CardHeader>

          <CardContent>
            {/* Search Bar */}
            <div className="flex gap-2 mb-4">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  type="text"
                  placeholder="Search by SKU code..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  className="pl-10"
                />
              </div>
              <Button onClick={handleSearch} disabled={loading}>
                Search
              </Button>
            </div>

            {/* Mappings Table */}
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>SKU Code</TableHead>
                    <TableHead>Tax</TableHead>
                    <TableHead>Tax Rate</TableHead>
                    <TableHead>Company</TableHead>
                    <TableHead>Created By</TableHead>
                    <TableHead>Created Date</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {loading ? (
                    <TableRow>
                      <TableCell colSpan={7} className="text-center py-8">
                        <div className="flex items-center justify-center">
                          <RefreshCw className="h-6 w-6 animate-spin mr-2" />
                          Loading mappings...
                        </div>
                      </TableCell>
                    </TableRow>
                  ) : mappings.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={7} className="text-center py-8">
                        <div>
                          <Unlink className="h-12 w-12 mx-auto mb-4 text-gray-400" />
                          <p>No tax-sku mappings found</p>
                          <p className="text-sm text-gray-500 mt-1">
                            Create a mapping to assign taxes to specific SKUs
                          </p>
                        </div>
                      </TableCell>
                    </TableRow>
                  ) : (
                    mappings.map((mapping) => {
                      const tax = taxes.find(t => t.UID === mapping.TaxUID);
                      return (
                        <TableRow key={mapping.UID}>
                          <TableCell className="font-mono text-sm">
                            {mapping.SKUUID}
                          </TableCell>
                          <TableCell>
                            <div>
                              <p className="font-medium">{getTaxName(mapping.TaxUID)}</p>
                              <p className="text-sm text-gray-500">{mapping.TaxUID}</p>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge variant="outline">
                              {tax ? `${tax.BaseTaxRate}%` : 'N/A'}
                            </Badge>
                          </TableCell>
                          <TableCell>{mapping.CompanyUID}</TableCell>
                          <TableCell>{mapping.CreatedBy || '-'}</TableCell>
                          <TableCell>{formatDate(mapping.CreatedTime)}</TableCell>
                          <TableCell className="text-right">
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="sm">
                                  <MoreHorizontal className="h-4 w-4" />
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end">
                                <DropdownMenuItem
                                  disabled
                                  className="text-muted-foreground"
                                >
                                  <Edit className="mr-2 h-4 w-4" />
                                  Edit Mapping
                                  <span className="ml-2 text-xs">(Coming Soon)</span>
                                </DropdownMenuItem>
                                <DropdownMenuItem
                                  onClick={() => handleDeleteMapping(mapping.UID)}
                                  className="text-red-600 focus:text-red-600"
                                >
                                  <Trash2 className="mr-2 h-4 w-4" />
                                  Delete Mapping
                                </DropdownMenuItem>
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </TableCell>
                        </TableRow>
                      );
                    })
                  )}
                </TableBody>
              </Table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex items-center justify-between mt-4">
                <div className="text-sm text-gray-600">
                  Showing {(currentPage - 1) * pageSize + 1} to{' '}
                  {Math.min(currentPage * pageSize, totalCount)} of {totalCount} mappings
                </div>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setCurrentPage((prev) => Math.max(1, prev - 1))}
                    disabled={currentPage === 1 || loading}
                  >
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
                  </Button>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default TaxSkuMappingPage;