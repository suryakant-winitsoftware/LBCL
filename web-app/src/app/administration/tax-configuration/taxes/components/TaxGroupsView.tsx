'use client';

import React, { useState, useEffect } from 'react';
import { taxService, ITaxGroup, PagingRequest } from '@/services/tax/tax.service';
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
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Search, RefreshCw, Eye, Edit, ChevronLeft, ChevronRight, Plus } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useToast } from '@/components/ui/use-toast';

const TaxGroupsView = () => {
  const router = useRouter();
  const { toast } = useToast();
  const [taxGroups, setTaxGroups] = useState<ITaxGroup[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(10);
  const [selectedGroup, setSelectedGroup] = useState<ITaxGroup | null>(null);

  useEffect(() => {
    loadTaxGroups();
  }, [currentPage]);

  const loadTaxGroups = async () => {
    setLoading(true);
    try {
      const request: PagingRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        isCountRequired: true,
        filterCriterias: searchTerm
          ? [
              {
                name: 'Name',
                value: searchTerm,
                operator: 'Contains',
              },
            ]
          : [],
        sortCriterias: [
          {
            sortParameter: 'Name',
            direction: 'Asc',
          },
        ],
      };

      const response = await taxService.getTaxGroupDetails(request);
      setTaxGroups(response.PagedData);
      setTotalCount(response.TotalCount);
    } catch (error) {
      console.error('Error loading tax groups:', error);
      toast({
        title: 'Error',
        description: 'Failed to load tax groups. Please try again.',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    setCurrentPage(1);
    loadTaxGroups();
  };

  const viewGroupDetails = async (uid: string) => {
    try {
      const group = await taxService.getTaxGroupByUID(uid);
      if (group) {
        setSelectedGroup(group);
        toast({
          title: 'Tax Group Details',
          description: `Loaded details for ${group.Name}`,
        });
      } else {
        toast({
          title: 'Not Found',
          description: 'Tax group details not found',
          variant: 'destructive',
        });
      }
    } catch (error) {
      console.error('Error loading tax group details:', error);
      toast({
        title: 'Error',
        description: 'Failed to load tax group details',
        variant: 'destructive',
      });
    }
  };

  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString();
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="space-y-4">
      <CardHeader className="px-0 pt-0">
        <div className="flex items-center justify-between">
          <CardTitle>Tax Groups</CardTitle>
          <div className="flex gap-2">
            <Button
              onClick={() => router.push('/administration/tax-configuration/taxes/create-group')}
              disabled={loading}
            >
              <Plus className="h-4 w-4 mr-2" />
              Create Tax Group
            </Button>
            <Button
              onClick={loadTaxGroups}
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
              placeholder="Search tax groups..."
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

        {/* Tax Groups Table */}
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Group Code</TableHead>
                <TableHead>Group Name</TableHead>
                <TableHead>Created By</TableHead>
                <TableHead>Created Date</TableHead>
                <TableHead>Modified By</TableHead>
                <TableHead>Modified Date</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-8">
                    <div className="flex items-center justify-center">
                      <RefreshCw className="h-6 w-6 animate-spin mr-2" />
                      Loading tax groups...
                    </div>
                  </TableCell>
                </TableRow>
              ) : taxGroups.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-8">
                    No tax groups found
                  </TableCell>
                </TableRow>
              ) : (
                taxGroups.map((group) => (
                  <TableRow key={group.UID}>
                    <TableCell className="font-mono text-sm">
                      {group.Code}
                    </TableCell>
                    <TableCell className="font-medium">{group.Name}</TableCell>
                    <TableCell>{group.CreatedBy || '-'}</TableCell>
                    <TableCell>{formatDate(group.CreatedTime)}</TableCell>
                    <TableCell>{group.ModifiedBy || '-'}</TableCell>
                    <TableCell>{formatDate(group.ModifiedTime)}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => viewGroupDetails(group.UID)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          disabled
                          title="Edit functionality coming soon"
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                      </div>
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
              {Math.min(currentPage * pageSize, totalCount)} of {totalCount} tax groups
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

        {/* Selected Group Details */}
        {selectedGroup && (
          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Selected Tax Group Details</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-gray-600">Group Name</p>
                  <p className="font-medium">{selectedGroup.Name}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-600">Group Code</p>
                  <p className="font-mono">{selectedGroup.Code}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-600">UID</p>
                  <p className="font-mono text-sm">{selectedGroup.UID}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-600">Action Type</p>
                  <Badge variant="outline">{selectedGroup.ActionType || 0}</Badge>
                </div>
              </div>
            </CardContent>
          </Card>
        )}
      </CardContent>
    </div>
  );
};

export default TaxGroupsView;