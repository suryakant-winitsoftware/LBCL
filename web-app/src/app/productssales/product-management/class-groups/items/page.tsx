"use client";

import React, { useState, useEffect, useRef } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useToast } from "@/components/ui/use-toast";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from "@/components/ui/alert-dialog";
import {
  ArrowLeft,
  Search,
  RefreshCw,
  Package,
  Edit,
  Trash2,
  Plus,
  Download,
  Upload,
  Filter,
  ChevronDown,
  MoreHorizontal,
  X
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuItem
} from "@/components/ui/dropdown-menu";
import { PaginationControls } from "@/components/ui/pagination-controls";
import { Skeleton } from "@/components/ui/skeleton";
import {
  skuClassGroupItemsService,
  SKUClassGroupItem,
  SKUClassGroupItemView
} from "@/services/sku-class-group-items.service";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";

export default function ClassGroupItemsPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);
  
  // Get parameters from URL
  const groupUID = searchParams.get('groupUID') || '';
  const groupName = searchParams.get('groupName') || 'Class Group';
  
  // State management
  const [items, setItems] = useState<SKUClassGroupItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  
  // Filter states
  const [filterExclusive, setFilterExclusive] = useState<string[]>([]);
  
  // Delete dialog state
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingItem, setDeletingItem] = useState<SKUClassGroupItem | null>(null);

  // Load items
  const loadItems = async () => {
    if (!groupUID) {
      toast({
        title: "Error",
        description: "Group UID is required",
        variant: "destructive"
      });
      return;
    }

    setLoading(true);
    try {
      const result = await skuClassGroupItemsService.getAllSKUClassGroupItems(
        currentPage,
        pageSize,
        searchTerm,
        groupUID
      );
      
      setItems(result.data);
      setTotalCount(result.totalCount);
      
      console.log(`📦 Loaded ${result.data.length} items for group ${groupUID}`, {
        totalCount: result.totalCount,
        currentPage,
        pageSize
      });
      
    } catch (error) {
      console.error('Error loading items:', error);
      toast({
        title: "Error",
        description: "Failed to load group items",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  // Effects
  useEffect(() => {
    loadItems();
  }, [currentPage, pageSize, groupUID]);

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (searchTerm !== '') {
        setCurrentPage(1);
        loadItems();
      } else if (searchTerm === '') {
        setCurrentPage(1);
        loadItems();
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchTerm]);
  
  // Add keyboard shortcut for Ctrl+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault();
        searchInputRef.current?.focus();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Event handlers
  const handleSearch = (term: string) => {
    setSearchTerm(term);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

  const handleRefresh = () => {
    loadItems();
  };

  const handleBack = () => {
    router.push('/productssales/product-management/class-groups');
  };

  // CRUD handlers
  const handleCreateItem = () => {
    const params = new URLSearchParams();
    if (groupUID) {
      params.set("groupUID", groupUID);
    }
    router.push(
      `/productssales/product-management/group-items/add?${params.toString()}`
    );
  };

  const handleEditItem = (item: SKUClassGroupItem) => {
    router.push(
      `/productssales/product-management/group-items/add?uid=${item.UID}`
    );
  };

  const handleDeleteItem = (item: SKUClassGroupItem) => {
    setDeletingItem(item);
    setDeleteDialogOpen(true);
  };

  const confirmDeleteItem = async () => {
    if (!deletingItem) return;

    try {
      await skuClassGroupItemsService.deleteSKUClassGroupItem(deletingItem.UID);
      toast({
        title: "Success",
        description: "Group item deleted successfully"
      });

      setDeleteDialogOpen(false);
      setDeletingItem(null);
      loadItems(); // Reload the current page
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "Failed to delete group item",
        variant: "destructive"
      });
    }
  };

  const formatDate = (date: string | undefined) => {
    if (!date) return '-';
    return formatDateToDayMonthYear(new Date(date));
  };

  // Filter items based on exclusive filter
  const filteredItems = items.filter(item => {
    if (filterExclusive.length === 0) return true;
    
    const isExclusive = item.IsExclusive;
    if (filterExclusive.includes("yes") && isExclusive) return true;
    if (filterExclusive.includes("no") && !isExclusive) return true;
    
    return false;
  });

  const renderTableContent = () => {
    if (loading) {
      return Array.from({ length: pageSize }).map((_, index) => (
        <TableRow key={index}>
          <TableCell className="pl-6">
            <Skeleton className="h-5 w-32" />
          </TableCell>
          <TableCell className="text-center">
            <Skeleton className="h-6 w-12 mx-auto rounded-full" />
          </TableCell>
          <TableCell className="text-center">
            <Skeleton className="h-5 w-8 mx-auto" />
          </TableCell>
          <TableCell className="text-center">
            <Skeleton className="h-5 w-12 mx-auto" />
          </TableCell>
          <TableCell className="text-center">
            <Skeleton className="h-5 w-12 mx-auto" />
          </TableCell>
          <TableCell className="text-center">
            <Skeleton className="h-5 w-12 mx-auto" />
          </TableCell>
          <TableCell className="text-center">
            <Skeleton className="h-6 w-10 mx-auto rounded-full" />
          </TableCell>
          <TableCell className="text-right pr-6">
            <div className="flex items-center justify-end">
              <Skeleton className="h-8 w-8 rounded" />
            </div>
          </TableCell>
        </TableRow>
      ));
    }

    if (filteredItems.length === 0) {
      return (
        <TableRow>
          <TableCell colSpan={8} className="text-center py-12">
            <div className="flex flex-col items-center space-y-4">
              <Package className="h-12 w-12 text-muted-foreground/50" />
              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">No items found</p>
                <p className="text-xs text-muted-foreground">
                  {searchTerm ? 'Try adjusting your search terms' : 'This group has no items yet'}
                </p>
              </div>
            </div>
          </TableCell>
        </TableRow>
      );
    }

    return filteredItems.map((item) => (
      <TableRow key={item.UID} className="hover:bg-muted/50">
        <TableCell className="pl-6 font-medium text-sm">{item.SKUCode}</TableCell>
        <TableCell className="text-center">
          <Badge variant="outline" className="text-xs">
            {item.ModelUoM || 'N/A'}
          </Badge>
        </TableCell>
        <TableCell className="text-center text-sm">{item.SerialNumber}</TableCell>
        <TableCell className="text-center text-sm">{item.ModelQty}</TableCell>
        <TableCell className="text-center text-sm">{item.MinQTY}</TableCell>
        <TableCell className="text-center text-sm">{item.MaxQTY}</TableCell>
        <TableCell className="text-center">
          <Badge variant={item.IsExclusive ? "default" : "secondary"} className="text-xs">
            {item.IsExclusive ? "Yes" : "No"}
          </Badge>
        </TableCell>
        <TableCell className="text-center">
          <div className="flex items-center justify-center space-x-2">
            <Button 
              variant="ghost" 
              size="sm" 
              className="h-8 w-8 p-0"
              onClick={() => handleEditItem(item)}
            >
              <Edit className="h-3 w-3" />
            </Button>
            <Button 
              variant="ghost" 
              size="sm" 
              className="h-8 w-8 p-0 text-destructive hover:text-destructive"
              onClick={() => handleDeleteItem(item)}
            >
              <Trash2 className="h-3 w-3" />
            </Button>
          </div>
        </TableCell>
      </TableRow>
    ));
  };

  return (
    <div className="container mx-auto py-4 space-y-4">
      {/* Back Navigation */}
      <div className="mb-6">
        <Button
          variant="outline"
          size="default"
          onClick={handleBack}
          className="group hover:bg-gray-50 transition-colors"
        >
          <ArrowLeft className="h-4 w-4 mr-2 transition-transform group-hover:-translate-x-1" />
          Back to Class Groups
        </Button>
      </div>

      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Group Items</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Managing items for <span className="font-semibold text-foreground">{decodeURIComponent(groupName)}</span>
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="default">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="default">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button size="default" onClick={handleCreateItem}>
            <Plus className="h-4 w-4 mr-2" />
            Add Item
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
                placeholder="Search by SKU code... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Exclusive Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Exclusive
                  {filterExclusive.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterExclusive.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Exclusive</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={filterExclusive.includes("yes")}
                  onCheckedChange={(checked) => {
                    setFilterExclusive(prev => 
                      checked 
                        ? [...prev, "yes"]
                        : prev.filter(s => s !== "yes")
                    )
                  }}
                >
                  Exclusive
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={filterExclusive.includes("no")}
                  onCheckedChange={(checked) => {
                    setFilterExclusive(prev => 
                      checked 
                        ? [...prev, "no"]
                        : prev.filter(s => s !== "no")
                    )
                  }}
                >
                  Non-Exclusive
                </DropdownMenuCheckboxItem>
                {filterExclusive.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterExclusive([])}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
            
            <Button
              variant="outline"
              size="default"
              onClick={handleRefresh}
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </CardContent>
      </Card>
      
      {/* Data Table */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <div className="overflow-hidden rounded-lg">
            <Table>
              <TableHeader>
                <TableRow className="bg-gray-50/50">
                  <TableHead className="pl-6">SKU Code</TableHead>
                  <TableHead className="text-center">UoM</TableHead>
                  <TableHead className="text-center">Serial #</TableHead>
                  <TableHead className="text-center">Model Qty</TableHead>
                  <TableHead className="text-center">Min QTY</TableHead>
                  <TableHead className="text-center">Max QTY</TableHead>
                  <TableHead className="text-center">Exclusive</TableHead>
                  <TableHead className="text-right pr-6">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {renderTableContent()}
              </TableBody>
            </Table>
          </div>

          {totalCount > 0 && (
            <div className="px-6 py-4 border-t bg-gray-50/30">
              <PaginationControls
                currentPage={currentPage}
                totalCount={totalCount}
                pageSize={pageSize}
                onPageChange={handlePageChange}
                onPageSizeChange={handlePageSizeChange}
                pageSizeOptions={[10, 20, 50, 100]}
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Item</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete item `{deletingItem?.SKUCode}`? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeletingItem(null)}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmDeleteItem}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}