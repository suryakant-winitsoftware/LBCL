"use client";

import React, { useState, useEffect, useRef, useCallback } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from "@/components/ui/dropdown-menu";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import { Skeleton } from "@/components/ui/skeleton";
import { PaginationControls } from "@/components/ui/pagination-controls";
import {
  Plus,
  Search,
  Edit,
  Trash2,
  FileDown,
  Layers,
  Filter,
  ChevronDown,
  ChevronRight,
  X,
  Store,
  MoreVertical,
  Upload,
  Users,
  MapPin,
  RefreshCw,
  Eye
} from "lucide-react";
import { useRouter } from "next/navigation";
import { storeGroupService } from "@/services/storeGroupService";
import { storeService } from "@/services/storeService";
import { storeToGroupMappingService } from "@/services/storeToGroupMappingService";
import { IStoreGroup, StoreGroupListRequest } from "@/types/storeGroup.types";
import StoreGroupDialog from "./StoreGroupDialog";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";

interface IStoreInGroup {
  UID?: string;
  Code?: string;
  Name?: string;
  IsActive?: boolean;
  StoreGroupUID?: string;
}

export default function StoreGroupList() {
  const { toast } = useToast();
  const router = useRouter();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [storeGroups, setStoreGroups] = useState<IStoreGroup[]>([]);
  const [allStoreGroups, setAllStoreGroups] = useState<IStoreGroup[]>([]); // Store all data for client-side filtering
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingStoreGroup, setEditingStoreGroup] = useState<IStoreGroup | null>(null);
  const [useClientSideFiltering, setUseClientSideFiltering] = useState(false);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [selectedTypes, setSelectedTypes] = useState<string[]>([]);

  // State for expanded rows and stores
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [groupStores, setGroupStores] = useState<Record<string, IStoreInGroup[]>>({});
  const [loadingStores, setLoadingStores] = useState<Record<string, boolean>>({});

  // Refs to track previous filter values
  const prevFiltersRef = useRef({ searchTerm: '', selectedTypes: '' });

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 500);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Reset to page 1 when filters change
  useEffect(() => {
    const currentFilters = {
      searchTerm: debouncedSearchTerm,
      selectedTypes: selectedTypes.join(',')
    };

    // Check if filters actually changed
    const filtersChanged =
      prevFiltersRef.current.searchTerm !== currentFilters.searchTerm ||
      prevFiltersRef.current.selectedTypes !== currentFilters.selectedTypes;

    if (filtersChanged) {
      console.log('🔄 Filters changed, resetting to page 1');
      prevFiltersRef.current = currentFilters;
      setCurrentPage(1);
    }
  }, [debouncedSearchTerm, selectedTypes]);

  // Initial data load - fetch all data once for client-side filtering
  useEffect(() => {
    const loadAllData = async () => {
      setLoading(true);
      try {
        const request: StoreGroupListRequest = {
          PageNumber: 1,
          PageSize: 1000, // Get all data
          FilterCriterias: [],
          SortCriterias: [],
          IsCountRequired: true
        };

        console.log('📥 Loading all store groups for client-side filtering...');
        console.log('📤 Request:', request);
        const response = await storeGroupService.getAllStoreGroups(request);

        console.log('📦 Raw API Response:', response);
        console.log('✅ Loaded all store groups:', response.PagedData?.length, 'items');

        if (response.PagedData && response.PagedData.length > 0) {
          console.log('📋 Sample data:', response.PagedData.slice(0, 3));
        }

        setAllStoreGroups(response.PagedData || []);
        setUseClientSideFiltering(true);
      } catch (error) {
        console.error("❌ Error loading all store groups:", error);
        toast({
          title: "Error",
          description: "Failed to load store groups",
          variant: "destructive"
        });
      } finally {
        setLoading(false);
      }
    };

    loadAllData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Only run once on mount

  // Client-side filtering and pagination
  useEffect(() => {
    if (allStoreGroups.length === 0) {
      // No data loaded yet
      setStoreGroups([]);
      setTotalCount(0);
      return;
    }

    console.log('🔍 Applying client-side filters:', {
      searchTerm: debouncedSearchTerm,
      selectedTypes: selectedTypes.join(','),
      totalData: allStoreGroups.length
    });

    let filtered = [...allStoreGroups];

    // Apply search filter
    if (debouncedSearchTerm) {
      const searchLower = debouncedSearchTerm.toLowerCase();
      filtered = filtered.filter(group => {
        const code = (group.Code || '').toLowerCase();
        const name = (group.Name || '').toLowerCase();
        const type = (group.StoreGroupTypeUID || '').toLowerCase();
        return code.includes(searchLower) ||
               name.includes(searchLower) ||
               type.includes(searchLower);
      });
    }

    // Apply type filter
    if (selectedTypes.length > 0) {
      filtered = filtered.filter(group =>
        selectedTypes.includes(group.StoreGroupTypeUID)
      );
    }

    // Calculate pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filtered.slice(startIndex, endIndex);

    console.log('✅ Client-side filtering complete:', {
      filteredCount: filtered.length,
      displayingCount: paginatedData.length,
      page: currentPage
    });

    setStoreGroups(paginatedData);
    setTotalCount(filtered.length);
  }, [allStoreGroups, debouncedSearchTerm, selectedTypes, currentPage, pageSize]);

  // Refresh function for manual reload
  const refreshData = useCallback(() => {
    setCurrentPage(1);
    // Reload all data
    const loadAllData = async () => {
      setLoading(true);
      try {
        const request: StoreGroupListRequest = {
          PageNumber: 1,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [],
          IsCountRequired: true
        };

        const response = await storeGroupService.getAllStoreGroups(request);
        setAllStoreGroups(response.PagedData || []);
      } catch (error) {
        console.error("Error reloading data:", error);
      } finally {
        setLoading(false);
      }
    };
    loadAllData();
  }, []);

  // Load stores for a specific group using real API
  const loadGroupStores = async (groupUID: string) => {
    try {
      setLoadingStores(prev => ({ ...prev, [groupUID]: true }));

      console.log(`🏪 Loading stores for group: ${groupUID}`);

      // Use the real API to get stores by group UID
      const stores = await storeToGroupMappingService.getStoresByGroupUID(groupUID);

      console.log(`✅ Loaded ${stores.length} stores for group ${groupUID}`);

      // Convert to our interface format
      const storesInGroup: IStoreInGroup[] = stores.map(store => ({
        UID: store.UID || store.uid,
        Code: store.Code || store.code,
        Name: store.Name || store.name,
        IsActive: store.IsActive ?? store.is_active ?? true,
        StoreGroupUID: groupUID
      }));

      setGroupStores(prev => ({ ...prev, [groupUID]: storesInGroup }));
    } catch (error) {
      console.error('❌ Error loading group stores:', error);
      toast({
        title: "Error",
        description: "Failed to load stores for this group.",
        variant: "destructive",
      });
      setGroupStores(prev => ({ ...prev, [groupUID]: [] }));
    } finally {
      setLoadingStores(prev => ({ ...prev, [groupUID]: false }));
    }
  };

  // Toggle row expansion and fetch stores
  const toggleRowExpansion = async (groupUID: string) => {
    const newExpandedRows = new Set(expandedRows);

    if (newExpandedRows.has(groupUID)) {
      newExpandedRows.delete(groupUID);
    } else {
      newExpandedRows.add(groupUID);

      // Load stores if not already loaded
      if (!groupStores[groupUID]) {
        await loadGroupStores(groupUID);
      }
    }

    setExpandedRows(newExpandedRows);
  };

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

  const handleAdd = () => {
    setEditingStoreGroup(null);
    setShowDialog(true);
  };

  const handleEdit = (storeGroup: IStoreGroup) => {
    setEditingStoreGroup(storeGroup);
    setShowDialog(true);
  };

  const handleDelete = async (storeGroup: IStoreGroup) => {
    if (!confirm(`Are you sure you want to delete "${storeGroup.Name}"?`)) return;

    try {
      await storeGroupService.deleteStoreGroup(storeGroup.UID);
      toast({
        title: "Success",
        description: `Store group "${storeGroup.Name}" deleted successfully`
      });
      refreshData();
    } catch (error) {
      console.error("Error deleting store group:", error);
      toast({
        title: "Error",
        description: "Failed to delete store group",
        variant: "destructive"
      });
    }
  };

  const handleDialogClose = (saved: boolean) => {
    setShowDialog(false);
    setEditingStoreGroup(null);
    if (saved) {
      refreshData();
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
    // Export functionality
    const csvContent = [
      ["Code", "Name", "Type", "Level", "Parent UID", "Has Child"],
      ...storeGroups.map(sg => [
        sg.Code,
        sg.Name,
        sg.StoreGroupTypeUID,
        sg.ItemLevel.toString(),
        sg.ParentUID || "",
        sg.HasChild ? "Yes" : "No"
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `store_groups_${new Date().toISOString()}.csv`;
    a.click();
  };

  const getTypeColor = (type: string) => {
    switch (type.toLowerCase()) {
      case 'channel':
        return 'bg-blue-100 text-blue-800';
      case 'region':
        return 'bg-green-100 text-green-800';
      case 'area':
        return 'bg-purple-100 text-purple-800';
      case 'zone':
        return 'bg-orange-100 text-orange-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <>
      {/* Header with actions */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Store Groups</h2>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="sm" onClick={handleExport}>
            <FileDown className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={handleAdd} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Group
          </Button>
        </div>
      </div>

      {/* Search Bar - Exactly like product groups */}
      <Card className="shadow-sm border-gray-200 mb-4">
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
            
            {/* Type Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Type
                  {selectedTypes.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedTypes.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={selectedTypes.includes("Channel")}
                  onCheckedChange={(checked) => {
                    setSelectedTypes(prev => 
                      checked 
                        ? [...prev, "Channel"]
                        : prev.filter(t => t !== "Channel")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Channel
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedTypes.includes("Region")}
                  onCheckedChange={(checked) => {
                    setSelectedTypes(prev => 
                      checked 
                        ? [...prev, "Region"]
                        : prev.filter(t => t !== "Region")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Region
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedTypes.includes("Area")}
                  onCheckedChange={(checked) => {
                    setSelectedTypes(prev => 
                      checked 
                        ? [...prev, "Area"]
                        : prev.filter(t => t !== "Area")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Area
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedTypes.includes("Zone")}
                  onCheckedChange={(checked) => {
                    setSelectedTypes(prev => 
                      checked 
                        ? [...prev, "Zone"]
                        : prev.filter(t => t !== "Zone")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Zone
                </DropdownMenuCheckboxItem>
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Clear All Button */}
            {(searchTerm || selectedTypes.length > 0) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm("");
                  setSelectedTypes([]);
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
                <TableHead className="w-10 pl-6"></TableHead>
                <TableHead>Code</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead className="text-center">Level</TableHead>
                <TableHead className="text-center">Has Child</TableHead>
                <TableHead>Created By</TableHead>
                <TableHead>Created Date</TableHead>
                <TableHead className="text-right pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 10 }).map((_, i) => (
                  <TableRow key={i}>
                    <TableCell className="pl-6">
                      <Skeleton className="h-6 w-6" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-36" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-6 w-20 rounded-full" />
                    </TableCell>
                    <TableCell className="text-center">
                      <Skeleton className="h-5 w-12 mx-auto" />
                    </TableCell>
                    <TableCell className="text-center">
                      <Skeleton className="h-5 w-16 mx-auto" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-28" />
                    </TableCell>
                    <TableCell className="text-right pr-6">
                      <div className="flex justify-end">
                        <Skeleton className="h-8 w-8 rounded" />
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : storeGroups.length > 0 ? (
                storeGroups.map((storeGroup) => (
                  <React.Fragment key={storeGroup.UID}>
                    <TableRow className="hover:bg-muted/50">
                      <TableCell className="pl-6">
                        <Button
                          variant="ghost"
                          size="sm"
                          className="h-8 w-8 p-0 hover:bg-muted"
                          onClick={() => toggleRowExpansion(storeGroup.UID!)}
                        >
                          {expandedRows.has(storeGroup.UID!) ? (
                            <ChevronDown className="h-4 w-4 transition-transform" />
                          ) : (
                            <ChevronRight className="h-4 w-4 transition-transform" />
                          )}
                        </Button>
                      </TableCell>
                      <TableCell className="font-medium">{storeGroup.Code}</TableCell>
                    <TableCell>{storeGroup.Name}</TableCell>
                    <TableCell>
                      <Badge className={getTypeColor(storeGroup.StoreGroupTypeUID)}>
                        {storeGroup.StoreGroupTypeUID}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="flex items-center justify-center gap-1">
                        <Layers className="h-3 w-3" />
                        {storeGroup.ItemLevel}
                      </div>
                    </TableCell>
                    <TableCell className="text-center">
                      {storeGroup.HasChild ? (
                        <Badge variant="outline" className="bg-green-50">Yes</Badge>
                      ) : (
                        <Badge variant="outline" className="bg-gray-50">No</Badge>
                      )}
                    </TableCell>
                    <TableCell>{storeGroup.CreatedBy || 'N/A'}</TableCell>
                    <TableCell>
                      {formatDateToDayMonthYear(storeGroup.CreatedTime)}
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
                          <DropdownMenuItem onClick={() => router.push(`/administration/store-management/store-groups/view/${storeGroup.UID}`)}>
                            <Eye className="mr-2 h-4 w-4" />
                            View Details
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleEdit(storeGroup)}>
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => handleDelete(storeGroup)}
                            className="text-red-600 focus:text-red-600"
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                  {expandedRows.has(storeGroup.UID!) && (
                    <TableRow>
                      <TableCell colSpan={9} className="p-0 border-b-2">
                        <div className="bg-gradient-to-b from-muted/20 to-muted/10 px-6 py-4">
                          {loadingStores[storeGroup.UID!] ? (
                            <div className="flex items-center justify-center py-8 space-x-3">
                              <RefreshCw className="h-5 w-5 animate-spin text-blue-600" />
                              <span className="text-sm text-gray-600">Loading stores...</span>
                            </div>
                          ) : (
                            <div className="space-y-4">
                              <div className="flex items-center justify-between">
                                <h4 className="text-sm font-semibold flex items-center">
                                  <Users className="h-4 w-4 mr-2 text-blue-600" />
                                  Stores in {storeGroup.Name}
                                  <Badge variant="outline" className="ml-2">
                                    {groupStores[storeGroup.UID!]?.length || 0} stores
                                  </Badge>
                                </h4>
                                <Button
                                  size="sm"
                                  variant="outline"
                                  onClick={() => {
                                    // Navigate to assign stores page
                                    console.log('Assign stores to group:', storeGroup.UID);
                                  }}
                                >
                                  <Plus className="h-4 w-4 mr-2" />
                                  Assign Stores
                                </Button>
                              </div>

                              {groupStores[storeGroup.UID!]?.length === 0 ? (
                                <div className="text-center py-8">
                                  <MapPin className="h-8 w-8 mx-auto text-gray-400 mb-2" />
                                  <p className="text-sm text-gray-500">No stores assigned to this group</p>
                                </div>
                              ) : (
                                <Table>
                                  <TableHeader>
                                    <TableRow>
                                      <TableHead>Code</TableHead>
                                      <TableHead>Name</TableHead>
                                      <TableHead className="text-center">Status</TableHead>
                                      <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                  </TableHeader>
                                  <TableBody>
                                    {groupStores[storeGroup.UID!]?.map((store) => (
                                      <TableRow key={store.UID} className="hover:bg-muted/50">
                                        <TableCell className="font-medium">{store.Code}</TableCell>
                                        <TableCell>{store.Name}</TableCell>
                                        <TableCell className="text-center">
                                          <Badge variant={store.IsActive ? "default" : "secondary"} className="text-xs">
                                            {store.IsActive ? 'Active' : 'Inactive'}
                                          </Badge>
                                        </TableCell>
                                        <TableCell className="text-right">
                                          <Button
                                            size="sm"
                                            variant="ghost"
                                            onClick={() => {
                                              // Navigate to store details
                                              console.log('View store:', store.UID);
                                            }}
                                          >
                                            <Eye className="h-4 w-4" />
                                          </Button>
                                        </TableCell>
                                      </TableRow>
                                    ))}
                                  </TableBody>
                                </Table>
                              )}
                            </div>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  )}
                </React.Fragment>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={9} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center py-8">
                      <Store className="h-12 w-12 text-gray-400 mb-3" />
                      <p className="text-sm font-medium text-gray-900">No store groups found</p>
                      <p className="text-sm text-gray-500 mt-1">
                        {searchTerm || selectedTypes.length > 0 
                          ? "Try adjusting your search or filters" 
                          : "Click 'Add Group' to create your first store group"}
                      </p>
                    </div>
                  </TableCell>
                </TableRow>
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
                itemName="store groups"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Dialog */}
      {showDialog && (
        <StoreGroupDialog
          open={showDialog}
          onClose={handleDialogClose}
          storeGroup={editingStoreGroup}
        />
      )}
    </>
  );
}