"use client";

import React, { useState, useEffect, useRef } from 'react';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
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
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Plus,
  Edit,
  Trash2,
  Search,
  RefreshCw,
  List,
  Eye,
  ChevronDown,
  ChevronRight,
  MoreHorizontal,
  Package,
  Filter,
  X
} from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

import {
  listService,
  ListHeader,
  ListItem,
  CreateListItemRequest,
  UpdateListItemRequest,
  PagingRequest,
  FilterCriteria,
  SortCriteria
} from '@/services/listService';
import { ListItemModal } from './components/list-item-modal';
import { useToast } from '@/components/ui/use-toast';

export default function ListManagementPage() {
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [listHeaders, setListHeaders] = useState<ListHeader[]>([]);
  const [totalHeaderCount, setTotalHeaderCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Pagination - Headers
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Filters and Search
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedEditableStatus, setSelectedEditableStatus] = useState<string[]>([]);
  const [selectedVisibilityStatus, setSelectedVisibilityStatus] = useState<string[]>([]);

  // Expandable rows state
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [listItems, setListItems] = useState<Record<string, ListItem[]>>({});
  const [loadingItems, setLoadingItems] = useState<Record<string, boolean>>({});

  // Inline editing state
  const [editingItems, setEditingItems] = useState<Set<string>>(new Set());
  const [tempItemData, setTempItemData] = useState<Record<string, Partial<CreateListItemRequest>>>({});
  const [newRowCounter, setNewRowCounter] = useState(0);

  // Modal states
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isViewModalOpen, setIsViewModalOpen] = useState(false);
  const [selectedListItem, setSelectedListItem] = useState<ListItem | null>(null);
  
  // Delete confirmation
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [itemToDelete, setItemToDelete] = useState<ListItem | null>(null);

  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      fetchListHeaders();
    }, searchTerm ? 500 : 0);
    
    return () => clearTimeout(debounceTimer);
  }, [searchTerm, currentPage, pageSize, selectedEditableStatus, selectedVisibilityStatus]);
  
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

  const fetchListHeaders = async () => {
    try {
      setLoading(true);
      setError(null);

      const filterCriterias: FilterCriteria[] = [];
      
      if (searchTerm) {
        filterCriterias.push({
          name: 'Name',
          value: searchTerm,
          type: 'Like'
        });
      }
      
      // Add Editable status filters
      if (selectedEditableStatus.length > 0) {
        selectedEditableStatus.forEach(status => {
          filterCriterias.push({
            name: 'IsEditable',
            value: status,
            type: 'Equal'
          });
        });
      }
      
      // Add Visibility status filters
      if (selectedVisibilityStatus.length > 0) {
        selectedVisibilityStatus.forEach(status => {
          filterCriterias.push({
            name: 'IsVisibleInUI',
            value: status,
            type: 'Equal'
          });
        });
      }

      const sortCriterias: SortCriteria[] = [
        { sortParameter: 'Name', direction: 'Asc' }
      ];

      const request: PagingRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        sortCriterias,
        filterCriterias,
        isCountRequired: true
      };

      const response = await listService.getListHeaders(request);
      setListHeaders(response.pagedData);
      setTotalHeaderCount(response.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch list headers');
      toast({
        title: 'Error',
        description: 'Failed to fetch list headers',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  // Toggle row expansion and fetch items
  const toggleRowExpansion = async (headerUID: string) => {
    const newExpandedRows = new Set(expandedRows);
    
    if (newExpandedRows.has(headerUID)) {
      newExpandedRows.delete(headerUID);
    } else {
      newExpandedRows.add(headerUID);
      
      // Fetch items if not already loaded
      if (!listItems[headerUID]) {
        setLoadingItems(prev => ({ ...prev, [headerUID]: true }));
        
        try {
          const items = await listService.getListItemsByHeaderUID(headerUID);
          setListItems(prev => ({ ...prev, [headerUID]: items }));
        } catch (error) {
          console.error('Error fetching items for header:', error);
          toast({
            title: 'Error',
            description: 'Failed to fetch items for this header',
            variant: 'destructive'
          });
          setListItems(prev => ({ ...prev, [headerUID]: [] }));
        } finally {
          setLoadingItems(prev => ({ ...prev, [headerUID]: false }));
        }
      }
    }
    
    setExpandedRows(newExpandedRows);
  };

  // Inline editing functions
  const startEditing = (itemUID: string, item: ListItem) => {
    setEditingItems(prev => new Set([...prev, itemUID]));
    setTempItemData(prev => ({
      ...prev,
      [itemUID]: {
        code: item.code,
        name: item.name,
        listHeaderUID: item.listHeaderUID,
        serialNo: item.serialNo,
        isEditable: item.isEditable,
        uid: item.uid
      }
    }));
  };

  const cancelEditing = (itemUID: string) => {
    setEditingItems(prev => {
      const newSet = new Set(prev);
      newSet.delete(itemUID);
      return newSet;
    });
    setTempItemData(prev => {
      const newData = { ...prev };
      delete newData[itemUID];
      return newData;
    });
  };

  const saveEdit = async (item: ListItem, headerUID: string) => {
    if (!item.uid) return;

    // Check if it's a new item
    if (item.uid.startsWith('new-')) {
      await saveNewItem(item, headerUID);
      return;
    }

    try {
      const tempData = tempItemData[item.uid];
      if (!tempData) return;

      const updatedItem: UpdateListItemRequest = {
        id: item.id!,
        code: tempData.code || item.code,
        name: tempData.name || item.name,
        listHeaderUID: headerUID,
        serialNo: tempData.serialNo || item.serialNo,
        isEditable: tempData.isEditable ?? item.isEditable,
        uid: item.uid
      };

      await listService.updateListItem(updatedItem);
      
      toast({
        title: 'Success',
        description: 'List item updated successfully'
      });

      // Update local state
      setListItems(prev => ({
        ...prev,
        [headerUID]: prev[headerUID]?.map(i => 
          i.uid === item.uid ? { ...item, ...tempData } : i
        ) || []
      }));

      // Clear editing state
      cancelEditing(item.uid);
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to update list item',
        variant: 'destructive'
      });
    }
  };

  const updateTempData = (itemUID: string, field: string, value: any) => {
    setTempItemData(prev => ({
      ...prev,
      [itemUID]: {
        ...prev[itemUID],
        [field]: value
      }
    }));
  };

  // Add new item row
  const addNewItem = (headerUID: string) => {
    const newItemId = `new-${newRowCounter}`;
    setNewRowCounter(prev => prev + 1);
    
    const newItem: ListItem = {
      uid: newItemId,
      code: '',
      name: '',
      listHeaderUID: headerUID,
      serialNo: 1,
      isEditable: true,
      createdBy: 'ADMIN',
      modifiedBy: 'ADMIN',
      createdTime: new Date().toISOString(),
      modifiedTime: new Date().toISOString(),
      serverAddTime: new Date().toISOString(),
      serverModifiedTime: new Date().toISOString()
    };
    
    // Add to the beginning of the list
    setListItems(prev => ({
      ...prev,
      [headerUID]: [newItem, ...(prev[headerUID] || [])]
    }));
    
    // Start editing the new row
    startEditing(newItemId, newItem);
  };

  // Save new item
  const saveNewItem = async (item: ListItem, headerUID: string) => {
    const tempData = tempItemData[item.uid!] || {};
    
    // Validation
    if (!tempData.code?.trim() || !tempData.name?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'Code and Name are required',
        variant: 'destructive'
      });
      return;
    }

    try {
      const newItem: CreateListItemRequest = {
        code: tempData.code!,
        name: tempData.name!,
        listHeaderUID: headerUID,
        serialNo: tempData.serialNo || 1,
        isEditable: tempData.isEditable ?? true,
        uid: listService.generateUID(tempData.name!)
      };

      await listService.createListItem(newItem);
      
      toast({
        title: 'Success',
        description: 'List item created successfully'
      });

      // Remove new row and refresh
      setListItems(prev => ({
        ...prev,
        [headerUID]: (prev[headerUID] || []).filter(i => i.uid !== item.uid)
      }));
      
      setEditingItems(prev => {
        const newSet = new Set(prev);
        newSet.delete(item.uid!);
        return newSet;
      });
      
      // Refresh the items for this header
      try {
        setLoadingItems(prev => ({ ...prev, [headerUID]: true }));
        const items = await listService.getListItemsByHeaderUID(headerUID);
        setListItems(prev => ({ ...prev, [headerUID]: items }));
      } catch (refreshError) {
        console.error('Error refreshing items:', refreshError);
      } finally {
        setLoadingItems(prev => ({ ...prev, [headerUID]: false }));
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to create list item',
        variant: 'destructive'
      });
    }
  };

  // Delete item
  const confirmDeleteItem = (item: ListItem) => {
    setItemToDelete(item);
    setDeleteConfirmOpen(true);
  };

  const handleDeleteListItem = async () => {
    if (!itemToDelete || !itemToDelete.uid) return;

    try {
      await listService.deleteListItem(itemToDelete.uid);
      
      toast({
        title: 'Success',
        description: 'List item deleted successfully'
      });

      // Remove from local state
      const headerUID = itemToDelete.listHeaderUID;
      setListItems(prev => ({
        ...prev,
        [headerUID]: prev[headerUID]?.filter(i => i.uid !== itemToDelete.uid) || []
      }));
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete list item',
        variant: 'destructive'
      });
    } finally {
      setDeleteConfirmOpen(false);
      setItemToDelete(null);
    }
  };

  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1);
  };

  const resetFilters = () => {
    setSearchTerm('');
    setSelectedEditableStatus([]);
    setSelectedVisibilityStatus([]);
    setCurrentPage(1);
  };

  const getStatusBadgeVariant = (isActive: boolean) => {
    return isActive ? 'default' : 'secondary';
  };

  return (
    <div className="container mx-auto py-6 space-y-8">
      {/* Header Section */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-bold tracking-tight">List Management</h1>
          <p className="text-sm text-muted-foreground">
            Manage list headers and their associated items
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => fetchListHeaders()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
        </div>
      </div>

      <div className="space-y-4">
        {/* Search and Filters */}
        <Card className="shadow-sm border-gray-200">
          <div className="p-3">
            <div className="flex gap-3">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  ref={searchInputRef}
                  placeholder="Search by name or code... (Ctrl+F)"
                  value={searchTerm}
                  onChange={(e) => handleSearch(e.target.value)}
                  className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
                />
              </div>
              
              {/* Editable Status Filter */}
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline">
                    <Filter className="h-4 w-4 mr-2" />
                    Editable
                    {selectedEditableStatus.length > 0 && (
                      <Badge variant="secondary" className="ml-2">
                        {selectedEditableStatus.length}
                      </Badge>
                    )}
                    <ChevronDown className="h-4 w-4 ml-2" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-48">
                  <DropdownMenuLabel>Filter by Editable Status</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuCheckboxItem
                    checked={selectedEditableStatus.includes("true")}
                    onCheckedChange={(checked) => {
                      setSelectedEditableStatus(prev => 
                        checked 
                          ? [...prev, "true"]
                          : prev.filter(s => s !== "true")
                      )
                      setCurrentPage(1)
                    }}
                  >
                    Editable
                  </DropdownMenuCheckboxItem>
                  <DropdownMenuCheckboxItem
                    checked={selectedEditableStatus.includes("false")}
                    onCheckedChange={(checked) => {
                      setSelectedEditableStatus(prev => 
                        checked 
                          ? [...prev, "false"]
                          : prev.filter(s => s !== "false")
                      )
                      setCurrentPage(1)
                    }}
                  >
                    Read Only
                  </DropdownMenuCheckboxItem>
                  {selectedEditableStatus.length > 0 && (
                    <>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        onClick={() => {
                          setSelectedEditableStatus([])
                          setCurrentPage(1)
                        }}
                      >
                        <X className="h-4 w-4 mr-2" />
                        Clear Filter
                      </DropdownMenuItem>
                    </>
                  )}
                </DropdownMenuContent>
              </DropdownMenu>

              {/* Visibility Status Filter */}
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline">
                    <Filter className="h-4 w-4 mr-2" />
                    Visibility
                    {selectedVisibilityStatus.length > 0 && (
                      <Badge variant="secondary" className="ml-2">
                        {selectedVisibilityStatus.length}
                      </Badge>
                    )}
                    <ChevronDown className="h-4 w-4 ml-2" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-48">
                  <DropdownMenuLabel>Filter by UI Visibility</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuCheckboxItem
                    checked={selectedVisibilityStatus.includes("true")}
                    onCheckedChange={(checked) => {
                      setSelectedVisibilityStatus(prev => 
                        checked 
                          ? [...prev, "true"]
                          : prev.filter(s => s !== "true")
                      )
                      setCurrentPage(1)
                    }}
                  >
                    Visible
                  </DropdownMenuCheckboxItem>
                  <DropdownMenuCheckboxItem
                    checked={selectedVisibilityStatus.includes("false")}
                    onCheckedChange={(checked) => {
                      setSelectedVisibilityStatus(prev => 
                        checked 
                          ? [...prev, "false"]
                          : prev.filter(s => s !== "false")
                      )
                      setCurrentPage(1)
                    }}
                  >
                    Hidden
                  </DropdownMenuCheckboxItem>
                  {selectedVisibilityStatus.length > 0 && (
                    <>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        onClick={() => {
                          setSelectedVisibilityStatus([])
                          setCurrentPage(1)
                        }}
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
                onClick={() => fetchListHeaders()}
                disabled={loading}
              >
                <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
              </Button>
            </div>
          </div>
        </Card>

        {/* Error Display */}
        {error && (
          <Card className="border-red-200 bg-red-50">
            <CardContent className="pt-4">
              <p className="text-red-600 text-sm">{error}</p>
            </CardContent>
          </Card>
        )}

        {/* Main Table Card */}
        <Card className="shadow-sm border-gray-200">
          <div className="p-0">
            <div className="overflow-hidden rounded-lg">
              <Table>
                <TableHeader>
                  <TableRow className="bg-gray-50/50">
                    <TableHead className="w-[50px] pl-6"></TableHead>
                    <TableHead className="font-medium pl-2">Code</TableHead>
                  <TableHead className="font-medium">Name</TableHead>
                  <TableHead className="font-medium">Editable</TableHead>
                  <TableHead className="font-medium">UI Visible</TableHead>
                  <TableHead className="font-medium">Modified</TableHead>
                  <TableHead className="text-right font-medium pr-6">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <>
                    {[...Array(pageSize)].map((_, index) => (
                      <TableRow key={`skeleton-${index}`}>
                        <TableCell className="pl-6"><Skeleton className="h-8 w-8" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                        <TableCell className="text-right pr-6">
                          <Skeleton className="h-8 w-8 rounded ml-auto" />
                        </TableCell>
                      </TableRow>
                    ))}
                  </>
                ) : listHeaders.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-12">
                      <div className="flex flex-col items-center space-y-4">
                        <Package className="h-12 w-12 text-muted-foreground/50" />
                        <div className="space-y-2">
                          <p className="text-sm font-medium text-muted-foreground">No list headers found</p>
                          <p className="text-xs text-muted-foreground">
                            {searchTerm || selectedEditableStatus.length > 0 || selectedVisibilityStatus.length > 0 ? 'Try adjusting your filters' : 'No list headers available'}
                          </p>
                        </div>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  listHeaders.map((header) => (
                    <React.Fragment key={header.uid}>
                      <TableRow className="hover:bg-muted/30 transition-colors">
                        <TableCell className="py-3 pl-6">
                          <Button
                            variant="ghost"
                            size="sm"
                            className="h-8 w-8 p-0 hover:bg-muted"
                            onClick={() => toggleRowExpansion(header.uid)}
                          >
                            {expandedRows.has(header.uid) ? (
                              <ChevronDown className="h-4 w-4 transition-transform" />
                            ) : (
                              <ChevronRight className="h-4 w-4 transition-transform" />
                            )}
                          </Button>
                        </TableCell>
                        <TableCell className="font-medium text-sm">{header.code}</TableCell>
                        <TableCell className="text-sm">{header.name}</TableCell>
                        <TableCell>
                          <Badge variant={getStatusBadgeVariant(header.isEditable)}>
                            {header.isEditable ? 'Editable' : 'Read Only'}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <Badge variant={getStatusBadgeVariant(header.isVisibleInUI)}>
                            {header.isVisibleInUI ? 'Visible' : 'Hidden'}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-sm text-muted-foreground">
                          {header.serverModifiedTime ? new Date(header.serverModifiedTime).toLocaleDateString() : '-'}
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
                                onClick={() => {
                                  if (!expandedRows.has(header.uid)) {
                                    toggleRowExpansion(header.uid);
                                  }
                                }}
                                className="cursor-pointer"
                              >
                                <Eye className="mr-2 h-4 w-4" />
                                View Items
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                      {expandedRows.has(header.uid) && (
                        <TableRow>
                          <TableCell colSpan={7} className="p-0 border-b-2">
                            <div className="bg-gradient-to-b from-muted/20 to-muted/10 px-6 py-4">
                              {loadingItems[header.uid] ? (
                                <div className="flex items-center justify-center py-8 space-x-3">
                                  <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                                  <span className="text-sm text-muted-foreground">Loading items...</span>
                                </div>
                              ) : listItems[header.uid] && listItems[header.uid].length > 0 ? (
                                <div className="space-y-4">
                                  <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-2">
                                      <div className="p-1.5 bg-primary/10 rounded-md">
                                        <List className="h-4 w-4 text-primary" />
                                      </div>
                                      <div>
                                        <span className="text-sm font-semibold">List Items</span>
                                        <span className="text-sm text-muted-foreground ml-2">
                                          ({listItems[header.uid]?.length || 0} items)
                                        </span>
                                      </div>
                                    </div>
                                  </div>
                                  <div className="rounded-lg border bg-card overflow-hidden">
                                    <Table>
                                      <TableHeader>
                                        <TableRow className="bg-muted/30 hover:bg-muted/30">
                                          <TableHead className="text-xs font-medium">Code</TableHead>
                                          <TableHead className="text-xs font-medium">Name</TableHead>
                                          <TableHead className="text-xs font-medium">Serial No</TableHead>
                                          <TableHead className="text-xs font-medium">Editable</TableHead>
                                          <TableHead className="text-xs font-medium">Modified</TableHead>
                                          <TableHead className="text-right text-xs font-medium">Actions</TableHead>
                                        </TableRow>
                                      </TableHeader>
                                      <TableBody>
                                        {(listItems[header.uid] || []).map((item) => {
                                          const isEditing = editingItems.has(item.uid);
                                          const tempData = tempItemData[item.uid] || {};
                                          
                                          return (
                                            <TableRow 
                                              key={item.uid} 
                                              className={`hover:bg-muted/20 ${isEditing ? 'bg-blue-50' : ''}`}
                                            >
                                              {/* Code - Editable */}
                                              <TableCell className="font-medium text-xs py-2">
                                                {isEditing ? (
                                                  <Input
                                                    value={tempData.code ?? item.code}
                                                    onChange={(e) => updateTempData(item.uid, 'code', e.target.value)}
                                                    placeholder="Code"
                                                    className="w-24 h-7 text-xs"
                                                  />
                                                ) : (
                                                  item.code
                                                )}
                                              </TableCell>

                                              {/* Name - Editable */}
                                              <TableCell className="text-xs py-2">
                                                {isEditing ? (
                                                  <Input
                                                    value={tempData.name ?? item.name}
                                                    onChange={(e) => updateTempData(item.uid, 'name', e.target.value)}
                                                    placeholder="Name"
                                                    className="w-32 h-7 text-xs"
                                                  />
                                                ) : (
                                                  item.name
                                                )}
                                              </TableCell>

                                              {/* Serial No - Editable */}
                                              <TableCell className="text-xs py-2">
                                                {isEditing ? (
                                                  <Input
                                                    type="number"
                                                    value={tempData.serialNo ?? item.serialNo}
                                                    onChange={(e) => updateTempData(item.uid, 'serialNo', parseInt(e.target.value) || 1)}
                                                    className="w-20 h-7 text-xs"
                                                    min="1"
                                                  />
                                                ) : (
                                                  item.serialNo || '-'
                                                )}
                                              </TableCell>

                                              {/* Editable Status */}
                                              <TableCell className="py-2">
                                                {isEditing ? (
                                                  <Switch
                                                    checked={tempData.isEditable ?? item.isEditable}
                                                    onCheckedChange={(checked) => updateTempData(item.uid, 'isEditable', checked)}
                                                  />
                                                ) : (
                                                  <Badge 
                                                    variant={item.isEditable ? 'default' : 'secondary'} 
                                                    className="text-xs px-2 py-0"
                                                  >
                                                    {item.isEditable ? 'Editable' : 'Read Only'}
                                                  </Badge>
                                                )}
                                              </TableCell>

                                              {/* Modified */}
                                              <TableCell className="text-xs py-2 text-muted-foreground">
                                                {item.serverModifiedTime ? new Date(item.serverModifiedTime).toLocaleDateString() : '-'}
                                              </TableCell>
                                              
                                              {/* Actions */}
                                              <TableCell className="text-right py-2">
                                                {isEditing ? (
                                                  <div className="flex gap-1 justify-end">
                                                    <Button
                                                      variant="ghost"
                                                      size="sm"
                                                      className="h-6 w-6 p-0 text-green-600 hover:text-green-700"
                                                      onClick={() => saveEdit(item, header.uid)}
                                                    >
                                                      ✓
                                                    </Button>
                                                    <Button
                                                      variant="ghost"
                                                      size="sm"
                                                      className="h-6 w-6 p-0 text-red-600 hover:text-red-700"
                                                      onClick={() => cancelEditing(item.uid)}
                                                    >
                                                      ✕
                                                    </Button>
                                                  </div>
                                                ) : (
                                                  <div className="flex gap-1 justify-end">
                                                    <Button
                                                      variant="ghost"
                                                      size="sm"
                                                      className="h-6 w-6 p-0"
                                                      onClick={() => startEditing(item.uid, item)}
                                                      title="Edit item"
                                                    >
                                                      <Edit className="h-3 w-3" />
                                                    </Button>
                                                    <Button
                                                      variant="ghost"
                                                      size="sm"
                                                      className="h-6 w-6 p-0 text-red-600 hover:text-red-700"
                                                      onClick={() => confirmDeleteItem(item)}
                                                      title="Delete item"
                                                    >
                                                      <Trash2 className="h-3 w-3" />
                                                    </Button>
                                                  </div>
                                                )}
                                              </TableCell>
                                            </TableRow>
                                          )
                                        })}
                                        {/* Add Item Row */}
                                        <TableRow className="hover:bg-muted/10">
                                          <TableCell colSpan={6} className="text-center py-3">
                                            <Button
                                              variant="ghost"
                                              size="sm"
                                              onClick={() => addNewItem(header.uid)}
                                              className="text-primary hover:text-primary/80"
                                            >
                                              <Plus className="h-4 w-4 mr-2" />
                                              Add Item
                                            </Button>
                                          </TableCell>
                                        </TableRow>
                                      </TableBody>
                                    </Table>
                                  </div>
                                </div>
                              ) : (
                                <div className="flex flex-col items-center justify-center py-12 space-y-4">
                                  <div className="p-3 bg-muted/50 rounded-full">
                                    <Package className="h-6 w-6 text-muted-foreground" />
                                  </div>
                                  <p className="text-sm text-muted-foreground">No items found in this list</p>
                                  <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => addNewItem(header.uid)}
                                  >
                                    <Plus className="h-4 w-4 mr-2" />
                                    Add Item
                                  </Button>
                                </div>
                              )}
                            </div>
                          </TableCell>
                        </TableRow>
                      )}
                    </React.Fragment>
                  ))
                )}
              </TableBody>
            </Table>
            </div>
          </div>
          
          {totalHeaderCount > 0 && (
            <div className="px-6 py-4 border-t bg-gray-50/30">
              <PaginationControls
                currentPage={currentPage}
                totalCount={totalHeaderCount}
                pageSize={pageSize}
                onPageChange={setCurrentPage}
                onPageSizeChange={(size) => {
                  setPageSize(size);
                  setCurrentPage(1);
                }}
                itemName="list headers"
              />
            </div>
          )}
        </Card>
      </div>

      {/* Delete Confirmation */}
      <AlertDialog open={deleteConfirmOpen} onOpenChange={setDeleteConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete List Item</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete the list item "{itemToDelete?.name}"?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteListItem}
              className="bg-red-600 hover:bg-red-700"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}