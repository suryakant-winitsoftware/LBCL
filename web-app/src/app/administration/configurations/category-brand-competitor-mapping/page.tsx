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
import { Checkbox } from '@/components/ui/checkbox';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import { PaginationControls } from '@/components/ui/pagination-controls';
import {
  Plus,
  Edit,
  Trash2,
  Search,
  Filter,
  FileDown,
  Upload,
  X,
  ChevronDown,
  MoreVertical,
  Building2,
} from 'lucide-react';
import competitorBrandService, {
  CompetitorBrandMapping,
  DropdownOption,
} from '@/services/competitorBrandService';
import { CompetitorBrandModal } from './components/CompetitorBrandModal';

export default function CategoryBrandCompetitorMappingPage() {
  const router = useRouter();
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);
  
  // State management
  const [mappings, setMappings] = useState<CompetitorBrandMapping[]>([]);
  const [filteredMappings, setFilteredMappings] = useState<CompetitorBrandMapping[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedMappings, setSelectedMappings] = useState<string[]>([]);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingMapping, setEditingMapping] = useState<CompetitorBrandMapping | null>(null);
  
  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  
  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  
  // Dropdown data
  const [categories, setCategories] = useState<DropdownOption[]>([]);

  useEffect(() => {
    loadInitialData();
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
    let filtered = [...mappings];

    // Apply search filter
    if (searchTerm) {
      const searchLower = searchTerm.toLowerCase();
      filtered = filtered.filter(mapping => 
        mapping.categoryName?.toLowerCase().includes(searchLower) ||
        mapping.brandCode?.toLowerCase().includes(searchLower) ||
        mapping.competitorCompany?.toLowerCase().includes(searchLower)
      );
    }

    // Apply category filter
    if (selectedCategories.length > 0) {
      filtered = filtered.filter(mapping => 
        selectedCategories.some(cat => 
          mapping.categoryName?.toLowerCase().includes(cat.toLowerCase())
        )
      );
    }

    // Apply pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filtered.slice(startIndex, endIndex);
    
    setFilteredMappings(paginatedData);
    setTotalCount(filtered.length);
  }, [mappings, searchTerm, selectedCategories, currentPage, pageSize]);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      const [categoriesData, mappingsResponse] = await Promise.all([
        competitorBrandService.getCategories(),
        competitorBrandService.getMappings(1, 1000) // Get all for client-side filtering
      ]);
      
      setCategories(categoriesData);
      setMappings(mappingsResponse.data);
    } catch (error) {
      console.error('Error loading data:', error);
      toast({
        title: 'Error',
        description: 'Failed to load data',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSelectAll = () => {
    if (selectedMappings.length === filteredMappings.length && filteredMappings.length > 0) {
      setSelectedMappings([]);
    } else {
      setSelectedMappings(filteredMappings.map(m => m.uid!).filter(uid => uid !== undefined));
    }
  };

  const handleSelectMapping = (uid: string) => {
    if (selectedMappings.includes(uid)) {
      setSelectedMappings(prev => prev.filter(id => id !== uid));
    } else {
      setSelectedMappings(prev => [...prev, uid]);
    }
  };

  const handleEdit = (mapping: CompetitorBrandMapping) => {
    setEditingMapping(mapping);
    setModalOpen(true);
  };

  const handleAdd = () => {
    setEditingMapping(null);
    setModalOpen(true);
  };

  const handleDelete = async () => {
    try {
      if (selectedMappings.length === 0) return;
      
      await competitorBrandService.bulkDelete(selectedMappings);
      
      toast({
        title: 'Success',
        description: `Successfully deleted ${selectedMappings.length} mapping(s)`,
      });
      
      setSelectedMappings([]);
      setDeleteDialogOpen(false);
      loadInitialData();
    } catch (error) {
      console.error('Error deleting mappings:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete mappings',
        variant: 'destructive',
      });
    }
  };

  const handleModalSuccess = () => {
    setModalOpen(false);
    setEditingMapping(null);
    loadInitialData();
    toast({
      title: 'Success',
      description: editingMapping ? 'Mapping updated successfully' : 'Mapping created successfully',
    });
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
      ["Category Name", "Our Brand", "Competitor Company"],
      ...filteredMappings.map(mapping => [
        mapping.categoryName,
        mapping.brandCode,
        mapping.competitorCompany
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `competitor_brand_mappings_${new Date().toISOString()}.csv`;
    a.click();
  };

  return (
    <div className="container mx-auto p-6">
      {/* Header with actions */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Competitor Brand Mapping</h2>
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
            Add Mapping
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
                placeholder="Search by category, brand or competitor... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setCurrentPage(1);
                }}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Category Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Category
                  {selectedCategories.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedCategories.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48 max-h-96 overflow-y-auto">
                <DropdownMenuLabel>Filter by Category</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {categories.map((category) => (
                  <DropdownMenuCheckboxItem
                    key={category.value}
                    checked={selectedCategories.includes(category.label)}
                    onCheckedChange={(checked) => {
                      setSelectedCategories(prev => 
                        checked 
                          ? [...prev, category.label]
                          : prev.filter(c => c !== category.label)
                      );
                      setCurrentPage(1);
                    }}
                  >
                    {category.label}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Clear All Button */}
            {(searchTerm || selectedCategories.length > 0) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm("");
                  setSelectedCategories([]);
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

      {/* Selected Actions */}
      {selectedMappings.length > 0 && (
        <Card className="shadow-sm border-orange-200 bg-orange-50 mb-4">
          <CardContent className="py-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="bg-orange-100 text-orange-800">
                  {selectedMappings.length} selected
                </Badge>
                <span className="text-sm text-orange-700">
                  Select actions to apply to selected mappings
                </span>
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setSelectedMappings([])}
                >
                  Clear Selection
                </Button>
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => setDeleteDialogOpen(true)}
                >
                  <Trash2 className="h-4 w-4 mr-2" />
                  Delete Selected
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Table */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-12 pl-6">
                  <Checkbox
                    checked={selectedMappings.length === filteredMappings.length && filteredMappings.length > 0}
                    onCheckedChange={handleSelectAll}
                  />
                </TableHead>
                <TableHead>Category Name</TableHead>
                <TableHead>Our Brand</TableHead>
                <TableHead>Competitor Company</TableHead>
                <TableHead className="text-right pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 10 }).map((_, index) => (
                  <TableRow key={index}>
                    <TableCell className="pl-6">
                      <Skeleton className="h-4 w-4" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-32" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-6 w-24 rounded-full" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-36" />
                    </TableCell>
                    <TableCell className="text-right pr-6">
                      <div className="flex justify-end">
                        <Skeleton className="h-8 w-8 rounded" />
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : filteredMappings.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center py-8">
                      <Building2 className="h-12 w-12 text-gray-400 mb-3" />
                      <p className="text-sm font-medium text-gray-900">No competitor brand mappings found</p>
                      <p className="text-sm text-gray-500 mt-1">
                        {searchTerm || selectedCategories.length > 0
                          ? "Try adjusting your search or filters" 
                          : "Click 'Add Mapping' to create your first competitor brand mapping"}
                      </p>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                filteredMappings.map((mapping) => (
                  <TableRow key={mapping.uid}>
                    <TableCell className="pl-6">
                      <Checkbox
                        checked={selectedMappings.includes(mapping.uid!)}
                        onCheckedChange={() => handleSelectMapping(mapping.uid!)}
                      />
                    </TableCell>
                    <TableCell className="font-medium">
                      {mapping.categoryName}
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{mapping.brandCode}</Badge>
                    </TableCell>
                    <TableCell>{mapping.competitorCompany}</TableCell>
                    <TableCell className="text-right pr-6">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" className="h-8 w-8 p-0">
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuLabel>Actions</DropdownMenuLabel>
                          <DropdownMenuItem onClick={() => handleEdit(mapping)}>
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => {
                              setSelectedMappings([mapping.uid!]);
                              setDeleteDialogOpen(true);
                            }}
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
                itemName="mappings"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Add/Edit Modal */}
      <CompetitorBrandModal
        open={modalOpen}
        onClose={() => {
          setModalOpen(false);
          setEditingMapping(null);
        }}
        onSuccess={handleModalSuccess}
        mapping={editingMapping}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirm Deletion</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete {selectedMappings.length} mapping(s)?
              This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteDialogOpen(false)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}