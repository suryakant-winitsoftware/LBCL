"use client";

import React, { useState, useEffect, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/components/ui/use-toast";
import { PaginationControls } from "@/components/ui/pagination-controls";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Settings, Plus, Edit, Trash2, Layers, Search, MoreVertical, FileDown, Upload, X } from "lucide-react";
import { storeGroupService } from "@/services/storeGroupService";
import { IStoreGroupType } from "@/types/storeGroup.types";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import StoreGroupTypeDialog from "./StoreGroupTypeDialog";

export default function StoreGroupTypeList() {
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [storeGroupTypes, setStoreGroupTypes] = useState<IStoreGroupType[]>([]);
  const [filteredStoreGroupTypes, setFilteredStoreGroupTypes] = useState<IStoreGroupType[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [showDialog, setShowDialog] = useState(false);
  const [editingStoreGroupType, setEditingStoreGroupType] = useState<IStoreGroupType | null>(null);

  const loadStoreGroupTypes = async () => {
    setLoading(true);
    try {
      const response = await storeGroupService.getAllStoreGroupTypes({
        PageNumber: 1,
        PageSize: 1000, // Get all for client-side filtering
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: true
      });
      
      setStoreGroupTypes(response.PagedData || []);
      setTotalCount(response.TotalCount || 0);
    } catch (error) {
      console.error("Error loading store group types:", error);
      toast({
        title: "Error",
        description: "Failed to load store group types",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStoreGroupTypes();
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

  // Filter data based on search
  useEffect(() => {
    let filtered = [...storeGroupTypes];

    // Apply search filter
    if (searchTerm) {
      filtered = filtered.filter(type => 
        type.Name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        type.Code?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filtered.slice(startIndex, endIndex);
    
    setFilteredStoreGroupTypes(paginatedData);
    setTotalCount(filtered.length);
  }, [storeGroupTypes, searchTerm, currentPage, pageSize]);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

  const handleAdd = () => {
    setEditingStoreGroupType(null);
    setShowDialog(true);
  };

  const handleEdit = (storeGroupType: IStoreGroupType) => {
    setEditingStoreGroupType(storeGroupType);
    setShowDialog(true);
  };

  const handleDialogClose = (saved: boolean) => {
    setShowDialog(false);
    setEditingStoreGroupType(null);
    if (saved) {
      loadStoreGroupTypes();
    }
  };

  const handleDelete = async (storeGroupType: IStoreGroupType) => {
    if (!confirm(`Are you sure you want to delete "${storeGroupType.Name}"?`)) return;

    try {
      await storeGroupService.deleteStoreGroupType(storeGroupType.UID);
      toast({
        title: "Success",
        description: `Store group type "${storeGroupType.Name}" deleted successfully`
      });
      loadStoreGroupTypes();
    } catch (error) {
      console.error("Error deleting store group type:", error);
      toast({
        title: "Error",
        description: "Failed to delete store group type",
        variant: "destructive"
      });
    }
  };

  const getLevelColor = (level: number) => {
    switch (level) {
      case 1:
        return 'bg-blue-100 text-blue-800';
      case 2:
        return 'bg-green-100 text-green-800';
      case 3:
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const handleExport = () => {
    const csvContent = [
      ["Code", "Name", "Parent", "Level", "Created By", "Created Date"],
      ...filteredStoreGroupTypes.map(type => [
        type.Code,
        type.Name,
        type.ParentUID || '',
        (type.LevelNo || 1).toString(),
        type.CreatedBy || 'N/A',
        formatDateToDayMonthYear(type.CreatedTime)
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `store_group_types_${new Date().toISOString()}.csv`;
    a.click();
  };

  return (
    <>
      {/* Header with actions */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Store Group Types</h2>
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
            Add Type
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
                placeholder="Search by name or code... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setCurrentPage(1);
                }}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Clear All Button */}
            {searchTerm && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm("");
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
                <TableHead className="pl-6">Code</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Parent</TableHead>
                <TableHead className="text-center">Level</TableHead>
                <TableHead>Created By</TableHead>
                <TableHead>Created Date</TableHead>
                <TableHead className="text-center">Status</TableHead>
                <TableHead className="text-right pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 10 }).map((_, i) => (
                  <TableRow key={i}>
                    <TableCell className="pl-6">
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-36" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-28" />
                    </TableCell>
                    <TableCell className="text-center">
                      <Skeleton className="h-6 w-12 mx-auto rounded-full" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-28" />
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
              ) : filteredStoreGroupTypes.length > 0 ? (
                filteredStoreGroupTypes.map((type) => (
                  <TableRow key={type.UID}>
                    <TableCell className="font-medium pl-6">{type.Code}</TableCell>
                    <TableCell>{type.Name}</TableCell>
                    <TableCell>{type.ParentUID || '-'}</TableCell>
                    <TableCell className="text-center">
                      <div className="flex items-center justify-center gap-1">
                        <Layers className="h-3 w-3" />
                        <Badge className={getLevelColor(type.LevelNo || 1)} variant="secondary">
                          {type.LevelNo || 1}
                        </Badge>
                      </div>
                    </TableCell>
                    <TableCell>{type.CreatedBy || 'N/A'}</TableCell>
                    <TableCell>
                      {formatDateToDayMonthYear(type.CreatedTime)}
                    </TableCell>
                    <TableCell className="text-center">
                      <Badge variant="default">
                        Active
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
                          <DropdownMenuItem onClick={() => handleEdit(type)}>
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem 
                            onClick={() => handleDelete(type)}
                            className="text-red-600 focus:text-red-600"
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={8} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center py-8">
                      <Settings className="h-12 w-12 text-gray-400 mb-3" />
                      <p className="text-sm font-medium text-gray-900">No store group types found</p>
                      <p className="text-sm text-gray-500 mt-1">
                        {searchTerm
                          ? "Try adjusting your search" 
                          : "Click 'Add Type' to create your first store group type"}
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
                itemName="store group types"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Dialog */}
      {showDialog && (
        <StoreGroupTypeDialog
          open={showDialog}
          onClose={handleDialogClose}
          storeGroupType={editingStoreGroupType}
        />
      )}
    </>
  );
}