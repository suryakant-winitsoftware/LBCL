"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";
import { DataTable } from "@/components/ui/data-table";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useToast } from "@/components/ui/use-toast";
import { PaginationControls } from "@/components/ui/pagination-controls";
import {
  Plus,
  Search,
  Edit,
  Trash2,
  RefreshCw,
  Tags,
  ChevronDown,
  ChevronRight,
  Package,
  ShoppingCart,
  PlusCircle,
  Eye,
  Filter,
  X,
  MoreHorizontal
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
import { Checkbox } from "@/components/ui/checkbox";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
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
  skuClassGroupsService,
  SKUClassGroup,
  SKUClassGroupCreateData,
  SKUClassGroupUpdateData
} from "@/services/sku-class-groups.service";
import {
  skuClassGroupItemsService,
  SKUClassGroupItem
} from "@/services/sku-class-group-items.service";

interface FormData {
  UID: string;
  Name: string;
  Description: string;
  OrgUID: string;
  DistributionChannelUID: string;
  SKUClassUID?: string;
  IsActive: boolean;
}

interface SKUClass {
  Id?: number;
  UID: string;
  CompanyUID: string;
  ClassName: string;
  Description: string;
  ClassLabel: string;
  CreatedBy?: string;
  ModifiedBy?: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

export default function SKUClassGroupsPage() {
  const router = useRouter();
  const { toast } = useToast();
  const searchInputRef = React.useRef<HTMLInputElement>(null);

  // Data states for Groups
  const [classGroups, setClassGroups] = useState<SKUClassGroup[]>([]);
  const [filteredGroups, setFilteredGroups] = useState<SKUClassGroup[]>([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);

  // Data states for SKU Classes (only for dropdown in form)
  const [skuClasses, setSKUClasses] = useState<SKUClass[]>([]);

  // Form states for Groups
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingGroup, setEditingGroup] = useState<SKUClassGroup | null>(null);
  const [deletingGroup, setDeletingGroup] = useState<SKUClassGroup | null>(null);
  const [formData, setFormData] = useState<FormData>({
    UID: "",
    Name: "",
    Description: "",
    OrgUID: "Supplier",
    DistributionChannelUID: "Supplier",
    SKUClassUID: "",
    IsActive: true
  });

  // Filter states - using arrays for multi-select
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState<string[]>([]);

  // Dropdown options for Groups
  const [organizations, setOrganizations] = useState<
    { uid: string; name: string }[]
  >([]);
  const [distributionChannels, setDistributionChannels] = useState<
    { uid: string; name: string }[]
  >([]);

  // State for expanded rows and group items
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [groupItems, setGroupItems] = useState<Record<string, (SKUClassGroupItem & {
    SKUName?: string;
    SupplierOrgName?: string;
  })[]>>({});
  const [loadingItems, setLoadingItems] = useState<Record<string, boolean>>({});
  
  // Pagination state for each group
  const [groupItemsPagination, setGroupItemsPagination] = useState<Record<string, {
    currentPage: number;
    totalCount: number;
    pageSize: number;
  }>>({});

  // Item counts for each group (for action buttons)
  const [groupItemCounts, setGroupItemCounts] = useState<Record<string, number>>({});
  const [loadingItemCounts, setLoadingItemCounts] = useState(false);

  // Initial load
  useEffect(() => {
    loadInitialData();
  }, []);

  // Load groups data when component mounts
  useEffect(() => {
    loadClassGroups();
  }, []);

  // Reload groups data when pagination or search changes
  useEffect(() => {
    loadClassGroups();
  }, [currentPage, searchTerm]);

  // Apply filters when data or filter values change
  useEffect(() => {
    applyFilters();
  }, [classGroups, filterStatus]);
  
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

  const loadInitialData = async () => {
    try {
      const [orgsResult, channelsResult] = await Promise.all([
        skuClassGroupsService.getAvailableOrganizations(),
        skuClassGroupsService.getAvailableDistributionChannels()
      ]);

      setOrganizations(orgsResult);
      setDistributionChannels(channelsResult);
    } catch (error) {
      console.error("Failed to load dropdown data:", error);
    }
  };

  const loadClassGroups = async () => {
    if (loading) return; // Prevent duplicate calls
    try {
      setLoading(true);
      const result = await skuClassGroupsService.getAllSKUClassGroups(
        currentPage,
        pageSize,
        searchTerm
      );

      console.log(`📊 Loaded ${result.data.length} groups, Group UIDs:`, result.data.map(g => g.UID));
      setClassGroups(result.data);
      setTotalCount(result.totalCount);

      // Load item counts for all groups
      await loadGroupItemCounts(result.data);
    } catch (error) {
      console.error("Failed to load class groups:", error);
      toast({
        title: "Error",
        description: "Failed to load SKU Class Groups",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  // Load item counts for groups to determine action buttons
  const loadGroupItemCounts = async (groups: SKUClassGroup[]) => {
    try {
      setLoadingItemCounts(true);
      console.log(`🔢 Loading item counts for ${groups.length} groups...`);
      const counts: Record<string, number> = {};

      // Load counts for each group in parallel
      await Promise.all(
        groups.map(async (group) => {
          try {
            const result = await skuClassGroupItemsService.getAllSKUClassGroupItems(
              1,
              1, // Only need count, not actual data
              "",
              group.UID
            );
            counts[group.UID] = result.totalCount;
            console.log(`📊 Group ${group.Name}: ${result.totalCount} items`);
          } catch (error) {
            console.error(`Failed to load count for group ${group.UID}:`, error);
            counts[group.UID] = 0;
          }
        })
      );

      setGroupItemCounts(counts);
      console.log(`✅ Loaded item counts:`, counts);
    } catch (error) {
      console.error("Failed to load group item counts:", error);
    } finally {
      setLoadingItemCounts(false);
    }
  };

  const applyFilters = () => {
    let filtered = [...classGroups];

    // Filter by status (multi-select)
    if (filterStatus.length > 0) {
      filtered = filtered.filter((group) => {
        const isActive = group.IsActive;
        return (filterStatus.includes("active") && isActive) || 
               (filterStatus.includes("inactive") && !isActive);
      });
    }

    setFilteredGroups(filtered);
  };

  const handleCreate = () => {
    setEditingGroup(null);
    setFormData({
      UID: "",
      Name: "",
      Description: "",
      OrgUID: "Supplier",
      DistributionChannelUID: "Supplier",
      SKUClassUID: "",
      IsActive: true
    });
    setDialogOpen(true);
  };

  const handleEdit = (group: SKUClassGroup) => {
    setEditingGroup(group);
    setFormData({
      UID: group.UID,
      Name: group.Name,
      Description: group.Description,
      OrgUID: group.OrgUID,
      DistributionChannelUID: group.DistributionChannelUID,
      SKUClassUID: group.SKUClassUID || "",
      IsActive: group.IsActive
    });
    setDialogOpen(true);
  };

  const handleDelete = (group: SKUClassGroup) => {
    setDeletingGroup(group);
    setDeleteDialogOpen(true);
  };

  // Handle adding items to a group
  const handleAddItems = (group: SKUClassGroup) => {
    const params = new URLSearchParams();
    params.set("groupUID", group.UID);
    router.push(`/productssales/product-management/group-items/add?${params.toString()}`);
  };

  // Handle viewing/managing existing items in a group
  const handleManageItems = (group: SKUClassGroup) => {
    const params = new URLSearchParams();
    params.set("groupUID", group.UID);
    params.set("groupName", group.Name);
    router.push(`/productssales/product-management/class-groups/items?${params.toString()}`);
  };

  const handleSubmit = async () => {
    try {
      if (!formData.Name.trim()) {
        toast({
          title: "Validation Error",
          description: "Name is required",
          variant: "destructive"
        });
        return;
      }

      const submitData: SKUClassGroupCreateData = {
        UID: formData.UID || `CLASS_GROUP_${Date.now()}`,
        Name: formData.Name.trim(),
        Description: formData.Description.trim() || formData.Name.trim(),
        OrgUID: formData.OrgUID,
        DistributionChannelUID: formData.DistributionChannelUID,
        SKUClassUID: formData.SKUClassUID || undefined,
        IsActive: formData.IsActive,
        CreatedBy: "ADMIN",
        ModifiedBy: "ADMIN"
      };

      if (editingGroup) {
        await skuClassGroupsService.updateSKUClassGroup(
          submitData as SKUClassGroupUpdateData
        );
        toast({
          title: "Success",
          description: "SKU Class Group updated successfully"
        });
      } else {
        await skuClassGroupsService.createSKUClassGroup(submitData);
        toast({
          title: "Success",
          description: "SKU Class Group created successfully"
        });
      }

      setDialogOpen(false);
      loadClassGroups();
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "Failed to save SKU Class Group",
        variant: "destructive"
      });
    }
  };

  const confirmDelete = async () => {
    if (!deletingGroup) return;

    try {
      await skuClassGroupsService.deleteSKUClassGroup(deletingGroup.UID);
      toast({
        title: "Success",
        description: "SKU Class Group deleted successfully"
      });
      setDeleteDialogOpen(false);
      setDeletingGroup(null);
      loadClassGroups();
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "Failed to delete SKU Class Group",
        variant: "destructive"
      });
    }
  };

  // Load group items (first 10 only for preview)
  const loadGroupItems = async (groupUID: string) => {
    setLoadingItems(prev => ({ ...prev, [groupUID]: true }));
    
    try {
      console.log(`🔍 Fetching first 10 items for group: ${groupUID}`);
      
      const result = await skuClassGroupItemsService.getAllSKUClassGroupItems(
        1,
        10, // Load only first 10 items for preview
        "",
        groupUID
      );
      
      console.log(`📦 API Response for group ${groupUID}:`, {
        totalCount: result.totalCount,
        dataLength: result.data.length,
        showing: `First ${result.data.length} of ${result.totalCount}`
      });
      
      // Map items with additional properties
      const itemsWithNames = result.data.map((item) => ({
        ...item,
        SKUName: item.SKUCode,
        SupplierOrgName: item.SupplierOrgUID || "N/A",
      }));
      
      // Store items and total count for "View All" button logic
      setGroupItems(prev => ({ ...prev, [groupUID]: itemsWithNames as any }));
      setGroupItemsPagination(prev => ({
        ...prev,
        [groupUID]: {
          currentPage: 1,
          totalCount: result.totalCount,
          pageSize: 10
        }
      }));
      
      console.log(`💾 Stored ${itemsWithNames.length} of ${result.totalCount} total items for group ${groupUID}`);
      
    } catch (error) {
      console.error('❌ Error fetching items for group:', error);
      toast({
        title: 'Error',
        description: 'Failed to load group items',
        variant: 'destructive'
      });
      setGroupItems(prev => ({ ...prev, [groupUID]: [] }));
    } finally {
      setLoadingItems(prev => ({ ...prev, [groupUID]: false }));
    }
  };

  // Toggle row expansion
  const toggleRowExpansion = async (groupUID: string) => {
    const newExpandedRows = new Set(expandedRows);
    
    if (newExpandedRows.has(groupUID)) {
      newExpandedRows.delete(groupUID);
    } else {
      newExpandedRows.add(groupUID);
      
      // Load first 50 items if not already loaded
      if (!groupItems[groupUID]) {
        await loadGroupItems(groupUID);
      }
    }
    
    setExpandedRows(newExpandedRows);
  };

  // Table columns for Groups
  const groupColumns = [
    {
      accessorKey: "Name",
      header: "Name",
      cell: ({ row }: any) => (
        <div className="flex items-center gap-2">
          <Tags className="h-4 w-4 text-primary" />
          <div className="font-medium">{row.original.Name}</div>
        </div>
      )
    },
    {
      accessorKey: "Description",
      header: "Description",
      cell: ({ row }: any) => (
        <div className="max-w-xs truncate">{row.original.Description}</div>
      )
    },
    {
      accessorKey: "OrgUID",
      header: "Organization",
      cell: ({ row }: any) => (
        <Badge variant="outline">{row.original.OrgUID}</Badge>
      )
    },
    {
      accessorKey: "DistributionChannelUID",
      header: "Distribution Channel",
      cell: ({ row }: any) => (
        <Badge variant="secondary">{row.original.DistributionChannelUID}</Badge>
      )
    },
    {
      accessorKey: "SKUClassUID",
      header: "SKU Class",
      cell: ({ row }: any) => {
        const skuClass = skuClasses.find(
          (c) => c.UID === row.original.SKUClassUID
        );
        return row.original.SKUClassUID ? (
          <div className="flex items-center gap-1">
            <Tags className="h-3 w-3 text-primary" />
            <span className="font-medium text-sm">
              {skuClass ? skuClass.ClassName : row.original.SKUClassUID}
            </span>
            {skuClass && (
              <span className="text-xs text-muted-foreground">
                ({skuClass.ClassLabel})
              </span>
            )}
          </div>
        ) : (
          <span className="text-muted-foreground text-sm">None</span>
        );
      }
    },
    {
      accessorKey: "IsActive",
      header: "Status",
      cell: ({ row }: any) => (
        <Badge variant={row.original.IsActive ? "default" : "secondary"}>
          {row.original.IsActive ? "Active" : "Inactive"}
        </Badge>
      )
    },
    {
      accessorKey: "CreatedTime",
      header: "Created",
      cell: ({ row }: any) => (
        <div className="text-sm">
          {formatDateToDayMonthYear(row.original.CreatedTime)}
        </div>
      )
    },
    {
      id: "actions",
      header: "Actions",
      cell: ({ row }: any) => (
        <div className="flex gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleEdit(row.original)}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleDelete(row.original)}
            className="text-destructive hover:text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      )
    }
  ];

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">SKU Class Groups</h1>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={loadClassGroups}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button onClick={handleCreate} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Group
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
                placeholder="Search by name... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Status Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {filterStatus.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterStatus.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={filterStatus.includes("active")}
                  onCheckedChange={(checked) => {
                    setFilterStatus(prev => 
                      checked 
                        ? [...prev, "active"]
                        : prev.filter(s => s !== "active")
                    )
                  }}
                >
                  Active
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={filterStatus.includes("inactive")}
                  onCheckedChange={(checked) => {
                    setFilterStatus(prev => 
                      checked 
                        ? [...prev, "inactive"]
                        : prev.filter(s => s !== "inactive")
                    )
                  }}
                >
                  Inactive
                </DropdownMenuCheckboxItem>
                {filterStatus.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterStatus([])}
                    >
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

      {/* Results for Groups */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/50 hover:bg-muted/50">
                  <TableHead className="w-[50px]"></TableHead>
                  <TableHead className="font-medium">Name</TableHead>
                  <TableHead className="font-medium">Description</TableHead>
                  <TableHead className="font-medium">SKU Class</TableHead>
                  <TableHead className="font-medium text-center">Total Items</TableHead>
                  <TableHead className="font-medium">Status</TableHead>
                  <TableHead className="text-right font-medium">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredGroups.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-8">
                      No class groups found
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredGroups.map((group) => (
                    <React.Fragment key={group.UID}>
                      <TableRow className="hover:bg-muted/30 transition-colors">
                        <TableCell className="py-3">
                          <Button
                            variant="ghost"
                            size="sm"
                            className="h-8 w-8 p-0 hover:bg-muted"
                            onClick={() => toggleRowExpansion(group.UID)}
                          >
                            {expandedRows.has(group.UID) ? (
                              <ChevronDown className="h-4 w-4 transition-transform" />
                            ) : (
                              <ChevronRight className="h-4 w-4 transition-transform" />
                            )}
                          </Button>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Tags className="h-4 w-4 text-primary" />
                            <div className="font-medium">{group.Name}</div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="max-w-xs truncate">{group.Description}</div>
                        </TableCell>
                        <TableCell>
                          {group.SKUClassUID ? (
                            <div className="flex items-center gap-1">
                              <Tags className="h-3 w-3 text-primary" />
                              <span className="font-medium text-sm">{group.SKUClassUID}</span>
                            </div>
                          ) : (
                            <span className="text-muted-foreground text-sm">None</span>
                          )}
                        </TableCell>
                        <TableCell className="text-center">
                          {loadingItemCounts ? (
                            <div className="flex justify-center">
                              <RefreshCw className="h-3 w-3 animate-spin text-muted-foreground" />
                            </div>
                          ) : (
                            <Badge variant="outline" className="font-medium">
                              {groupItemCounts[group.UID] || 0}
                            </Badge>
                          )}
                        </TableCell>
                        <TableCell>
                          <Badge variant={group.IsActive ? "default" : "secondary"}>
                            {group.IsActive ? "Active" : "Inactive"}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" className="h-8 w-8 p-0">
                                <span className="sr-only">Open menu</span>
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuLabel>Actions</DropdownMenuLabel>
                              
                              {/* Conditional Items Actions */}
                              {!loadingItemCounts && (
                                <>
                                  {groupItemCounts[group.UID] === 0 ? (
                                    <DropdownMenuItem
                                      onClick={() => handleAddItems(group)}
                                      className="cursor-pointer"
                                    >
                                      <PlusCircle className="mr-2 h-4 w-4" />
                                      Add Items
                                    </DropdownMenuItem>
                                  ) : (
                                    <>
                                      <DropdownMenuItem
                                        onClick={() => handleManageItems(group)}
                                        className="cursor-pointer"
                                      >
                                        <Eye className="mr-2 h-4 w-4" />
                                        View Items ({groupItemCounts[group.UID]})
                                      </DropdownMenuItem>
                                      <DropdownMenuItem
                                        onClick={() => handleAddItems(group)}
                                        className="cursor-pointer"
                                      >
                                        <Plus className="mr-2 h-4 w-4" />
                                        Add More Items
                                      </DropdownMenuItem>
                                    </>
                                  )}
                                  <DropdownMenuSeparator />
                                </>
                              )}
                              
                              <DropdownMenuItem
                                onClick={() => handleEdit(group)}
                                className="cursor-pointer"
                              >
                                <Edit className="mr-2 h-4 w-4" />
                                Edit
                              </DropdownMenuItem>
                              <DropdownMenuSeparator />
                              <DropdownMenuItem
                                onClick={() => handleDelete(group)}
                                className="cursor-pointer text-red-600"
                              >
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                      {expandedRows.has(group.UID) && (
                        <TableRow>
                          <TableCell colSpan={7} className="p-0 border-b-2">
                            <div className="bg-gradient-to-b from-muted/20 to-muted/10 px-6 py-4">
                              {loadingItems[group.UID] ? (
                                <div className="flex items-center justify-center py-8 space-x-3">
                                  <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                                  <span className="text-sm text-muted-foreground">Loading items...</span>
                                </div>
                              ) : groupItems[group.UID] && groupItems[group.UID].length > 0 ? (
                                <div className="space-y-4">
                                  <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-2">
                                      <div className="p-1.5 bg-primary/10 rounded-md">
                                        <Package className="h-4 w-4 text-primary" />
                                      </div>
                                      <div>
                                        <span className="text-sm font-semibold">Group Items</span>
                                        <span className="text-sm text-muted-foreground ml-2">
                                          (Showing {groupItems[group.UID]?.length || 0} of {groupItemsPagination[group.UID]?.totalCount || 0} items)
                                        </span>
                                      </div>
                                    </div>
                                    {groupItemsPagination[group.UID] && groupItemsPagination[group.UID].totalCount > 10 && (
                                      <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => router.push(`/productssales/product-management/class-groups/items?groupUID=${group.UID}&groupName=${encodeURIComponent(group.Name)}`)}
                                      >
                                        View All ({groupItemsPagination[group.UID].totalCount} items)
                                      </Button>
                                    )}
                                  </div>
                                  <div className="rounded-lg border bg-card overflow-hidden">
                                    <Table>
                                      <TableHeader>
                                        <TableRow className="bg-muted/30 hover:bg-muted/30">
                                          <TableHead className="text-xs font-medium w-[30%]">SKU Code</TableHead>
                                          <TableHead className="text-xs font-medium text-center w-[15%]">UoM</TableHead>
                                          <TableHead className="text-xs font-medium text-center w-[15%]">Min QTY</TableHead>
                                          <TableHead className="text-xs font-medium text-center w-[15%]">Max QTY</TableHead>
                                          <TableHead className="text-xs font-medium text-center w-[25%]">Exclusive</TableHead>
                                        </TableRow>
                                      </TableHeader>
                                      <TableBody>
                                        {(groupItems[group.UID] || []).map((item) => (
                                          <TableRow key={item.UID} className="hover:bg-muted/20">
                                            <TableCell className="font-medium text-xs py-2 w-[30%]">
                                              {item.SKUCode}
                                            </TableCell>
                                            <TableCell className="text-xs py-2 text-center w-[15%]">
                                              <Badge variant="secondary" className="text-xs">
                                                {item.ModelUoM || 'N/A'}
                                              </Badge>
                                            </TableCell>
                                            <TableCell className="text-xs py-2 text-center w-[15%]">
                                              <span className="font-medium">{item.MinQTY || 0}</span>
                                            </TableCell>
                                            <TableCell className="text-xs py-2 text-center w-[15%]">
                                              <span className="font-medium">{item.MaxQTY || 0}</span>
                                            </TableCell>
                                            <TableCell className="text-xs py-2 text-center w-[25%]">
                                              <Badge variant={item.IsExclusive ? "default" : "secondary"}>
                                                {item.IsExclusive ? "Yes" : "No"}
                                              </Badge>
                                            </TableCell>
                                          </TableRow>
                                        ))}
                                      </TableBody>
                                    </Table>
                                  </div>
                                </div>
                              ) : (
                                <div className="flex flex-col items-center justify-center py-12 space-y-4">
                                  <div className="p-3 bg-muted/50 rounded-full">
                                    <Package className="h-6 w-6 text-muted-foreground" />
                                  </div>
                                  <p className="text-sm text-muted-foreground">No items found in this group</p>
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
        </CardContent>
      </Card>

      {/* Create/Edit Dialog for Groups */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {editingGroup ? "Edit SKU Class Group" : "Create SKU Class Group"}
            </DialogTitle>
            <DialogDescription>
              {editingGroup
                ? "Update the class group details below."
                : "Create a new SKU classification group."}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label>Name *</Label>
              <Input
                value={formData.Name}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, Name: e.target.value }))
                }
                placeholder="e.g., AllowedSKUSpencers"
              />
            </div>

            <div>
              <Label>Description</Label>
              <Input
                value={formData.Description}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    Description: e.target.value
                  }))
                }
                placeholder="Brief description"
              />
            </div>

            <div>
              <Label>Organization</Label>
              <Select
                value={formData.OrgUID}
                onValueChange={(value) =>
                  setFormData((prev) => ({ ...prev, OrgUID: value }))
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {organizations.map((org) => (
                    <SelectItem key={org.uid} value={org.uid}>
                      {org.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Distribution Channel</Label>
              <Select
                value={formData.DistributionChannelUID}
                onValueChange={(value) =>
                  setFormData((prev) => ({
                    ...prev,
                    DistributionChannelUID: value
                  }))
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {distributionChannels.map((channel) => (
                    <SelectItem key={channel.uid} value={channel.uid}>
                      {channel.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>SKU Class (Optional)</Label>
              <Select
                value={formData.SKUClassUID || "none"}
                onValueChange={(value) =>
                  setFormData((prev) => ({
                    ...prev,
                    SKUClassUID: value === "none" ? "" : value
                  }))
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select a SKU class..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">None</SelectItem>
                  {skuClasses.map((skuClass) => (
                    <SelectItem key={skuClass.UID} value={skuClass.UID}>
                      <div className="flex items-center gap-2">
                        <Tags className="h-3 w-3" />
                        <span>{skuClass.ClassName}</span>
                        <span className="text-sm text-muted-foreground">
                          ({skuClass.ClassLabel})
                        </span>
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <div className="text-sm text-muted-foreground mt-1">
                Optional: Associate this group with a specific SKU
                classification
              </div>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                checked={formData.IsActive}
                onCheckedChange={(checked) =>
                  setFormData((prev) => ({ ...prev, IsActive: !!checked }))
                }
              />
              <Label>Active</Label>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit}>
              {editingGroup ? "Update" : "Create"} Class Group
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog for Groups */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Class Group</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete "{deletingGroup?.Name}"? This
              action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeletingGroup(null)}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmDelete}
              className="bg-destructive text-destructive-foreground"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}