'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Card,
  CardContent,
} from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from '@/components/ui/dropdown-menu';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import { PaginationControls } from '@/components/ui/pagination-controls';
import {
  Plus,
  Edit,
  Trash2,
  Upload,
  Search,
  FileDown,
  X,
  ChevronDown,
  Filter,
  MoreVertical,
  Target as TargetIcon,
  Calendar,
} from 'lucide-react';
import targetService, { Target, TargetFilter } from '@/services/targetService';

export default function TargetManagementPage() {
  const router = useRouter();
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [targets, setTargets] = useState<Target[]>([]);
  const [filteredTargets, setFilteredTargets] = useState<Target[]>([]);
  const [loading, setLoading] = useState(true);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [targetToDelete, setTargetToDelete] = useState<number | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedYears, setSelectedYears] = useState<string[]>([]);
  const [selectedMonths, setSelectedMonths] = useState<string[]>([]);
  const [selectedUserTypes, setSelectedUserTypes] = useState<string[]>([]);

  const currentYear = new Date().getFullYear();
  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  useEffect(() => {
    loadTargets();
  }, []);

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

  // Filter data based on search and filters
  useEffect(() => {
    let filtered = [...targets];

    // Apply search filter
    if (searchTerm) {
      filtered = filtered.filter(target => 
        target.UserLinkedUid?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        target.CustomerLinkedUid?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        target.ItemLinkedItemType?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply year filter
    if (selectedYears.length > 0) {
      filtered = filtered.filter(target => 
        selectedYears.includes(target.TargetYear?.toString() || '')
      );
    }

    // Apply month filter  
    if (selectedMonths.length > 0) {
      filtered = filtered.filter(target => 
        selectedMonths.includes(target.TargetMonth?.toString() || '')
      );
    }

    // Apply user type filter
    if (selectedUserTypes.length > 0) {
      filtered = filtered.filter(target => 
        selectedUserTypes.includes(target.UserLinkedType || '')
      );
    }

    // Apply pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filtered.slice(startIndex, endIndex);
    
    setFilteredTargets(paginatedData);
    setTotalCount(filtered.length);
  }, [targets, searchTerm, selectedYears, selectedMonths, selectedUserTypes, currentPage, pageSize]);

  const loadTargets = async () => {
    try {
      setLoading(true);
      const paginatedFilter = {
        PageNumber: 1,
        PageSize: 1000, // Get all for client-side filtering
      };
      const response = await targetService.getPagedTargets(paginatedFilter);
      console.log('🔍 Target API Response:', response);
      
      // Handle different response structures
      let targetsData = [];
      
      if (response?.data?.Data && Array.isArray(response.data.Data)) {
        targetsData = response.data.Data;
        console.log('✅ Using response.data.Data');
      } else if (response?.data && Array.isArray(response.data)) {
        targetsData = response.data;
        console.log('✅ Using response.data');
      } else if (Array.isArray(response)) {
        targetsData = response;
        console.log('✅ Using response directly');
      } else {
        console.log('❌ Could not extract array data from response');
      }
      
      console.log('🎯 Processed targets data:', targetsData);
      setTargets(targetsData);
    } catch (error) {
      console.error('Error loading targets:', error);
      setTargets([]);
      toast({
        title: 'Error',
        description: 'Failed to load targets',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!targetToDelete) return;

    try {
      await targetService.deleteTarget(targetToDelete);
      toast({
        title: 'Success',
        description: 'Target deleted successfully',
      });
      loadTargets();
      setDeleteDialogOpen(false);
      setTargetToDelete(null);
    } catch (error) {
      console.error('Error deleting target:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete target',
        variant: 'destructive',
      });
    }
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

  const handleExport = () => {
    const csvContent = [
      ["User Type", "User Code", "Customer Type", "Customer Code", "Category", "Month", "Year", "Target Amount", "Status"],
      ...filteredTargets.map(target => [
        target.UserLinkedType || '',
        target.UserLinkedUid || '',
        target.CustomerLinkedType || '',
        target.CustomerLinkedUid || '',
        target.ItemLinkedItemType || '',
        target.TargetMonth || '',
        target.TargetYear || '',
        target.TargetAmount || '0',
        target.Status || 'Not Set'
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `targets_${new Date().toISOString()}.csv`;
    a.click();
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(amount);
  };

  const getStatusBadgeVariant = (status?: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (status) {
      case 'Achieved':
        return 'default';
      case 'In Progress':
        return 'secondary';
      case 'Failed':
        return 'destructive';
      default:
        return 'outline';
    }
  };

  return (
    <div className="container mx-auto p-6">
      {/* Header with actions */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Target Management</h2>
        <div className="flex gap-2">
          <Button
            onClick={() => router.push('/distributiondelivery/route-management/target/upload')}
            variant="outline"
            size="sm"
          >
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="sm" onClick={handleExport}>
            <FileDown className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button
            onClick={() => router.push('/distributiondelivery/route-management/target/create')}
            size="sm"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Target
          </Button>
        </div>
      </div>

      {/* Search Bar */}
      <Card className="shadow-sm border-gray-200 mb-4">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by user code, customer code or category... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setCurrentPage(1);
                }}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Year Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Calendar className="h-4 w-4 mr-2" />
                  Year
                  {selectedYears.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedYears.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Year</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {[currentYear - 1, currentYear, currentYear + 1].map((year) => (
                  <DropdownMenuCheckboxItem
                    key={year}
                    checked={selectedYears.includes(year.toString())}
                    onCheckedChange={(checked) => {
                      setSelectedYears(prev => 
                        checked 
                          ? [...prev, year.toString()]
                          : prev.filter(y => y !== year.toString())
                      );
                      setCurrentPage(1);
                    }}
                  >
                    {year}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Month Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Calendar className="h-4 w-4 mr-2" />
                  Month
                  {selectedMonths.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedMonths.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48 max-h-96 overflow-y-auto">
                <DropdownMenuLabel>Filter by Month</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {monthNames.map((month, index) => (
                  <DropdownMenuCheckboxItem
                    key={index + 1}
                    checked={selectedMonths.includes((index + 1).toString())}
                    onCheckedChange={(checked) => {
                      setSelectedMonths(prev => 
                        checked 
                          ? [...prev, (index + 1).toString()]
                          : prev.filter(m => m !== (index + 1).toString())
                      );
                      setCurrentPage(1);
                    }}
                  >
                    {month}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* User Type Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  User Type
                  {selectedUserTypes.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedUserTypes.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by User Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={selectedUserTypes.includes("Route")}
                  onCheckedChange={(checked) => {
                    setSelectedUserTypes(prev => 
                      checked 
                        ? [...prev, "Route"]
                        : prev.filter(t => t !== "Route")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Route
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedUserTypes.includes("Employee")}
                  onCheckedChange={(checked) => {
                    setSelectedUserTypes(prev => 
                      checked 
                        ? [...prev, "Employee"]
                        : prev.filter(t => t !== "Employee")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Employee
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedUserTypes.includes("Customer")}
                  onCheckedChange={(checked) => {
                    setSelectedUserTypes(prev => 
                      checked 
                        ? [...prev, "Customer"]
                        : prev.filter(t => t !== "Customer")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Customer
                </DropdownMenuCheckboxItem>
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Clear All Button */}
            {(searchTerm || selectedYears.length > 0 || selectedMonths.length > 0 || selectedUserTypes.length > 0) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm("");
                  setSelectedYears([]);
                  setSelectedMonths([]);
                  setSelectedUserTypes([]);
                  setCurrentPage(1);
                }}
              >
                <X className="h-4 w-4 mr-2" />
                Clear All
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="pl-6">User Type</TableHead>
                <TableHead>User Code</TableHead>
                <TableHead>Customer Type</TableHead>
                <TableHead>Customer Code</TableHead>
                <TableHead>Category</TableHead>
                <TableHead>Period</TableHead>
                <TableHead className="text-right">Target Amount</TableHead>
                <TableHead className="text-center">Status</TableHead>
                <TableHead className="text-right pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 10 }).map((_, index) => (
                  <TableRow key={index}>
                    <TableCell className="pl-6">
                      <Skeleton className="h-5 w-20" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-20" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-20" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-20" />
                    </TableCell>
                    <TableCell className="text-right">
                      <Skeleton className="h-5 w-20 ml-auto" />
                    </TableCell>
                    <TableCell className="text-center">
                      <Skeleton className="h-6 w-16 mx-auto rounded-full" />
                    </TableCell>
                    <TableCell className="text-right pr-6">
                      <div className="flex justify-end">
                        <Skeleton className="h-8 w-8 rounded" />
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : filteredTargets.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center py-8">
                      <TargetIcon className="h-12 w-12 text-gray-400 mb-3" />
                      <p className="text-sm font-medium text-gray-900">No targets found</p>
                      <p className="text-sm text-gray-500 mt-1">
                        {searchTerm || selectedYears.length > 0 || selectedMonths.length > 0 || selectedUserTypes.length > 0
                          ? "Try adjusting your search or filters" 
                          : "Click 'Add Target' to create your first target"}
                      </p>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                filteredTargets.map((target, index) => (
                  <TableRow key={target.Id || `target-${index}`}>
                    <TableCell className="font-medium pl-6">
                      {target?.UserLinkedType || '-'}
                    </TableCell>
                    <TableCell>{target?.UserLinkedUid || '-'}</TableCell>
                    <TableCell>{target?.CustomerLinkedType || '-'}</TableCell>
                    <TableCell>{target?.CustomerLinkedUid || '-'}</TableCell>
                    <TableCell>{target?.ItemLinkedItemType || '-'}</TableCell>
                    <TableCell>
                      {target?.TargetMonth && target?.TargetYear 
                        ? `${monthNames[target.TargetMonth - 1]} ${target.TargetYear}`
                        : '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      {formatAmount(target?.TargetAmount || 0)}
                    </TableCell>
                    <TableCell className="text-center">
                      <Badge variant={getStatusBadgeVariant(target.Status)}>
                        {target.Status || 'Not Set'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right pr-6">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" className="h-8 w-8 p-0">
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuLabel>Actions</DropdownMenuLabel>
                          <DropdownMenuItem 
                            onClick={() =>
                              target?.Id &&
                              router.push(
                                `/distributiondelivery/route-management/target/edit/${target.Id}`
                              )
                            }
                            disabled={!target?.Id}
                          >
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => {
                              if (target?.Id) {
                                setTargetToDelete(target.Id);
                                setDeleteDialogOpen(true);
                              }
                            }}
                            className="text-red-600 focus:text-red-600"
                            disabled={!target?.Id}
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>

          {/* Pagination */}
          {totalCount > 0 && (
            <div className="px-6 py-4 border-t bg-gray-50/30">
              <PaginationControls
                currentPage={currentPage}
                totalCount={totalCount}
                pageSize={pageSize}
                onPageChange={handlePageChange}
                onPageSizeChange={handlePageSizeChange}
                itemName="targets"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirm Delete</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete this target? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteDialogOpen(false)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
            >
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}