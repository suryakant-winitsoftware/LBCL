"use client";

import React, { useState, useEffect, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useToast } from "@/components/ui/use-toast";
import {
  Plus,
  Search,
  Edit,
  Trash2,
  MoreHorizontal,
  Download,
  Upload,
  Filter,
  RefreshCw,
  Eye,
  Copy,
  Archive,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  ChevronDown,
  LayoutGrid,
  List,
  X,
  Image as ImageIcon,
  Save,
} from "lucide-react";
import planogramService, {
  PlanogramSetup,
  PlanogramCategory,
} from "@/services/planogramService";
import { planogramFileService, PlanogramFile } from "@/services/planogram-file.service";
import { skuGroupService, SKUGroup } from "@/services/sku-group.service";
import { hierarchyService } from "@/services/hierarchy.service";
import CreatePlanogramDialog from "@/components/planogram/CreatePlanogramDialog";
import PlanogramHierarchySelector from "@/components/planogram/PlanogramHierarchySelector";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import { PaginationControls } from "@/components/ui/pagination-controls";

export default function PlanogramSetupPage() {
  // Router hook
  const router = useRouter();

  // Toast hook
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);

  // State management
  const [setups, setSetups] = useState<PlanogramSetup[]>([]);
  const [categories, setCategories] = useState<PlanogramCategory[]>([]);
  const [categoryGroups, setCategoryGroups] = useState<SKUGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingCategories, setLoadingCategories] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedTypes, setSelectedTypes] = useState<string[]>([]);
  const [selectedSetups, setSelectedSetups] = useState<string[]>([]);

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);

  // Dialog states
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [isBulkDeleteDialogOpen, setIsBulkDeleteDialogOpen] = useState(false);
  const [isImageDialogOpen, setIsImageDialogOpen] = useState(false);
  const [selectedSetup, setSelectedSetup] = useState<PlanogramSetup | null>(
    null
  );

  // Form state
  const [formData, setFormData] = useState<PlanogramSetup>({
    categoryCode: "",
    shareOfShelfCm: 100,
    selectionType: "Category",
    selectionValue: "",
  });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  // Image gallery state (for viewing existing images only)
  const [planogramImages, setPlanogramImages] = useState<PlanogramFile[]>([]);

  // Image upload state for edit dialog
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>("");
  const [uploading, setUploading] = useState(false);

  // Load data on component mount
  useEffect(() => {
    loadData();
    loadCategories();
    loadCategoryGroups();
  }, [currentPage, pageSize, selectedTypes]);

  // Add keyboard shortcut for Ctrl+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === "f") {
        e.preventDefault();
        searchInputRef.current?.focus();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, []);

  // Load planogram setups
  const loadData = async () => {
    try {
      setLoading(true);
      const searchRequest = {
        searchText: searchTerm,
        pageNumber: currentPage,
        pageSize: pageSize,
      };

      console.log("Search request:", searchRequest);
      let response;

      // Try search first, fallback to getAll if search fails
      try {
        response = await planogramService.searchPlanogramSetups(searchRequest);
      } catch (searchError) {
        console.log(
          "Search failed, trying getAllPlanogramSetups:",
          searchError
        );
        response = await planogramService.getAllPlanogramSetups(
          currentPage,
          pageSize
        );
      }

      console.log("API response:", response);

      // Handle the actual API response structure
      // API returns: {Data: {totalCount, pageNumber, pageSize, totalPages, data: [...]}, StatusCode: 200, IsSuccess: true}
      let setupsData = [];
      let totalPages = 1;
      let totalCount = 0;

      if (response?.Data) {
        // Search API returns nested structure
        if (response.Data.data && Array.isArray(response.Data.data)) {
          setupsData = response.Data.data;
          totalPages = response.Data.totalPages || 1;
          totalCount = response.Data.totalCount || 0;
        }
        // GetAll API returns direct array
        else if (Array.isArray(response.Data)) {
          setupsData = response.Data;
          totalPages = Math.ceil(setupsData.length / pageSize);
          totalCount = setupsData.length;
        }
      }

      console.log(
        "Processed setupsData:",
        setupsData.length,
        "items, Array?",
        Array.isArray(setupsData)
      );
      console.log("First setup item:", setupsData[0]);
      setSetups(setupsData);
      setTotalPages(totalPages);
      setTotalRecords(totalCount);
    } catch (error) {
      console.error("Error loading planogram setups:", error);
      setSetups([]);
      setTotalPages(1);
      setTotalRecords(0);
      toast({
        title: "Error",
        description:
          "Failed to load planogram setups. Check console for details.",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  // Load categories
  const loadCategories = async () => {
    try {
      const response = await planogramService.getPlanogramCategories();
      console.log("Categories response:", response);

      // Handle the actual API response structure
      // API returns: {Data: [], StatusCode: 200, IsSuccess: true}
      if (response?.Data && Array.isArray(response.Data)) {
        setCategories(response.Data);
      } else {
        setCategories([]);
      }
    } catch (error) {
      console.error("Error loading categories:", error);
      setCategories([]);
      toast({
        title: "Warning",
        description: "Failed to load categories",
        variant: "destructive",
      });
    }
  };

  // Load category groups for dropdown
  const loadCategoryGroups = async () => {
    try {
      setLoadingCategories(true);
      console.log("Loading category groups for dropdown...");

      // First get hierarchy types to find Category type UID
      const hierarchyTypes = await hierarchyService.getHierarchyTypes();
      const categoryType = hierarchyTypes.find(type => type.Name === "Category");

      if (!categoryType) {
        console.warn("Category hierarchy type not found");
        setCategoryGroups([]);
        return;
      }

      console.log("Found Category type:", categoryType);

      // Get SKU groups for Category type
      const skuGroups = await skuGroupService.getSKUGroupsByTypeUID(categoryType.UID);
      console.log(`Loaded ${skuGroups.length} category groups:`, skuGroups);

      // Sort categories: top-level parents first, then children
      const topLevelCategories = skuGroups.filter(group => !group.ParentUID || group.ParentUID === "");
      const childCategories = skuGroups.filter(group => group.ParentUID && group.ParentUID !== "");

      // Combine with top-level first
      const sortedCategories = [...topLevelCategories, ...childCategories];
      console.log(`Found ${topLevelCategories.length} top-level categories, ${childCategories.length} child categories`);

      setCategoryGroups(sortedCategories);

      // Auto-select first top-level category for new records (when no existing selection)
      if (topLevelCategories.length > 0 && !formData.SelectionValue && !formData.selectionValue) {
        const firstTopCategory = topLevelCategories[0];
        console.log("Auto-selecting first top parent category:", firstTopCategory);

        setFormData((prev) => ({
          ...prev,
          SelectionType: "Category",
          selectionType: "Category",
          SelectionValue: firstTopCategory.Code,
          selectionValue: firstTopCategory.Code,
          CategoryCode: firstTopCategory.Code,
          categoryCode: firstTopCategory.Code,
        }));
      }
    } catch (error) {
      console.error("Error loading category groups:", error);
      setCategoryGroups([]);
    } finally {
      setLoadingCategories(false);
    }
  };

  // Handle search
  const handleSearch = (value: string) => {
    setSearchTerm(value);
  };

  // Debounced search effect
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (searchTerm !== "" || searchTerm === "") {
        setCurrentPage(1);
        loadData();
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchTerm]);

  // Update formData when selectedSetup changes (for edit dialog)
  useEffect(() => {
    if (selectedSetup && isEditDialogOpen) {
      console.log('🔧 Setting formData from selectedSetup:', selectedSetup);
      setFormData({
        UID: selectedSetup.UID || selectedSetup.uid,
        uid: selectedSetup.UID || selectedSetup.uid,
        CategoryCode: selectedSetup.CategoryCode || selectedSetup.categoryCode,
        categoryCode: selectedSetup.CategoryCode || selectedSetup.categoryCode,
        CategoryName: selectedSetup.CategoryName || selectedSetup.categoryName,
        categoryName: selectedSetup.CategoryName || selectedSetup.categoryName,
        ShareOfShelfCm: selectedSetup.ShareOfShelfCm || selectedSetup.shareOfShelfCm || 100,
        shareOfShelfCm: selectedSetup.ShareOfShelfCm || selectedSetup.shareOfShelfCm || 100,
        SelectionType: selectedSetup.SelectionType || selectedSetup.selectionType || "Category",
        selectionType: selectedSetup.SelectionType || selectedSetup.selectionType || "Category",
        SelectionValue: selectedSetup.SelectionValue || selectedSetup.selectionValue || "",
        selectionValue: selectedSetup.SelectionValue || selectedSetup.selectionValue || "",
        CreatedTime: selectedSetup.CreatedTime || selectedSetup.createdTime,
        createdTime: selectedSetup.CreatedTime || selectedSetup.createdTime,
        ModifiedTime: selectedSetup.ModifiedTime || selectedSetup.modifiedTime,
        modifiedTime: selectedSetup.ModifiedTime || selectedSetup.modifiedTime,
      });
    }
  }, [selectedSetup, isEditDialogOpen]);

  // Handle edit
  const handleEdit = async (setup: PlanogramSetup) => {
    try {
      setSelectedSetup(setup);
      setIsEditDialogOpen(true);

      // Clear any previous image selection
      clearImageSelection();

      // Fetch fresh data from server to ensure we have the latest values
      const setupId = setup.UID || setup.uid;
      if (setupId) {
        // Load images for this planogram
        await loadPlanogramImages(setupId);

        const response = await planogramService.getPlanogramSetupByUID(setupId);

        if (response?.Data) {
          const freshData = response.Data;

          // Map API response (PascalCase) to form data (camelCase) for consistency
          const mappedFormData: PlanogramSetup = {
            UID: freshData.UID || setupId,
            uid: freshData.UID || setupId,
            CategoryCode:
              freshData.CategoryCode || freshData.categoryCode || "",
            categoryCode:
              freshData.CategoryCode || freshData.categoryCode || "",
            ShareOfShelfCm:
              freshData.ShareOfShelfCm ?? freshData.shareOfShelfCm ?? 100,
            shareOfShelfCm:
              freshData.ShareOfShelfCm ?? freshData.shareOfShelfCm ?? 100,
            SelectionType:
              freshData.SelectionType || freshData.selectionType || "Category",
            selectionType:
              freshData.SelectionType || freshData.selectionType || "Category",
            SelectionValue:
              freshData.SelectionValue || freshData.selectionValue || "",
            selectionValue:
              freshData.SelectionValue || freshData.selectionValue || "",
          };

          setFormData(mappedFormData);
        } else {
          // Fallback to list data if server fetch fails
          const mappedFormData: PlanogramSetup = {
            UID: setup.UID || setup.uid,
            uid: setup.UID || setup.uid,
            CategoryCode: setup.CategoryCode || setup.categoryCode || "",
            categoryCode: setup.CategoryCode || setup.categoryCode || "",
            ShareOfShelfCm: setup.ShareOfShelfCm ?? setup.shareOfShelfCm ?? 100,
            shareOfShelfCm: setup.ShareOfShelfCm ?? setup.shareOfShelfCm ?? 100,
            SelectionType:
              setup.SelectionType || setup.selectionType || "Category",
            selectionType:
              setup.SelectionType || setup.selectionType || "Category",
            SelectionValue: setup.SelectionValue || setup.selectionValue || "",
            selectionValue: setup.SelectionValue || setup.selectionValue || "",
          };

          setFormData(mappedFormData);
        }
      } else {
        toast({
          title: "Error",
          description: "No valid UID found for this planogram setup",
          variant: "destructive",
        });
        setIsEditDialogOpen(false);
        return;
      }

      setFormErrors({});
    } catch (error) {
      console.error("Error loading planogram setup for edit:", error);
      toast({
        title: "Error",
        description: "Failed to load planogram setup data for editing",
        variant: "destructive",
      });
      setIsEditDialogOpen(false);
    }
  };

  // Handle delete
  const handleDelete = (setup: PlanogramSetup) => {
    setSelectedSetup(setup);
    setIsDeleteDialogOpen(true);
  };

  // Handle bulk delete
  const handleBulkDelete = () => {
    if (selectedSetups.length === 0) {
      toast({
        title: "No items selected",
        description: "Please select items to delete",
        variant: "destructive",
      });
      return;
    }
    setIsBulkDeleteDialogOpen(true);
  };

  // Validate form
  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.categoryCode) {
      errors.categoryCode = "Category code is required";
    }
    if (!formData.selectionValue) {
      errors.selectionValue = "Selection value is required";
    }
    if (
      formData.shareOfShelfCm &&
      (formData.shareOfShelfCm < 0 || formData.shareOfShelfCm > 1000)
    ) {
      errors.shareOfShelfCm = "Share of shelf must be between 0 and 1000";
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // Submit edit
  const submitEdit = async () => {
    if (!validateForm()) return;

    try {
      // Ensure we have a valid UID for the update
      const setupId =
        formData.UID ||
        formData.uid ||
        selectedSetup?.UID ||
        selectedSetup?.uid;
      if (!setupId) {
        toast({
          title: "Error",
          description: "No valid UID found for updating this planogram setup",
          variant: "destructive",
        });
        return;
      }

      // Prepare data for API (ensure PascalCase for consistency with API expectations)
      const updateData: PlanogramSetup = {
        UID: setupId,
        CategoryCode: formData.categoryCode || formData.CategoryCode,
        SelectionType: formData.selectionType || formData.SelectionType,
        SelectionValue: formData.selectionValue || formData.SelectionValue,
        ShareOfShelfCm: formData.shareOfShelfCm ?? formData.ShareOfShelfCm,
      };

      console.log("Updating planogram setup with data:", updateData);

      // Update the planogram setup
      await planogramService.updatePlanogramSetup(updateData);

      // If there's a new image selected, upload it
      if (selectedImage) {
        setUploading(true);
        try {
          await planogramService.uploadPlanogramImageDirect(
            setupId,
            selectedImage
          );
          toast({
            title: "Success",
            description: "Planogram updated and image uploaded successfully",
          });
        } catch (imageError) {
          console.error("Error uploading image:", imageError);
          toast({
            title: "Partial Success",
            description: "Planogram updated but image upload failed",
            variant: "destructive",
          });
        }
        setUploading(false);
      } else {
        toast({
          title: "Success",
          description: "Planogram setup updated successfully",
        });
      }

      // Clear form state and close dialog
      clearImageSelection();
      setIsEditDialogOpen(false);
      loadData();
    } catch (error) {
      console.error("Error updating planogram setup:", error);
      toast({
        title: "Error",
        description: "Failed to update planogram setup",
        variant: "destructive",
      });
    }
  };

  // Confirm delete
  const confirmDelete = async () => {
    const setupId = selectedSetup?.UID || selectedSetup?.uid;
    if (!setupId) return;

    try {
      await planogramService.deletePlanogramSetup(setupId);
      toast({
        title: "Success",
        description: "Planogram setup deleted successfully",
      });
      setIsDeleteDialogOpen(false);
      loadData();
    } catch (error) {
      console.error("Error deleting planogram setup:", error);
      toast({
        title: "Error",
        description: "Failed to delete planogram setup",
        variant: "destructive",
      });
    }
  };

  // Confirm bulk delete
  const confirmBulkDelete = async () => {
    try {
      await planogramService.bulkDeletePlanogramSetups(selectedSetups);
      toast({
        title: "Success",
        description: `${selectedSetups.length} planogram setups deleted successfully`,
      });
      setIsBulkDeleteDialogOpen(false);
      setSelectedSetups([]);
      loadData();
    } catch (error) {
      console.error("Error bulk deleting planogram setups:", error);
      toast({
        title: "Error",
        description: "Failed to delete planogram setups",
        variant: "destructive",
      });
    }
  };

  // Handle select all
  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedSetups(
        Array.isArray(setups)
          ? setups.map((s) => s.uid || s.UID || "").filter(Boolean)
          : []
      );
    } else {
      setSelectedSetups([]);
    }
  };

  // Handle select one
  const handleSelectOne = (uid: string, checked: boolean) => {
    if (checked) {
      setSelectedSetups([...selectedSetups, uid]);
    } else {
      setSelectedSetups(selectedSetups.filter((id) => id !== uid));
    }
  };

  // Copy to clipboard
  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    toast({
      title: "Copied",
      description: "Copied to clipboard",
    });
  };

  // Image handling functions for edit dialog
  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith("image/")) {
      toast({
        title: "Invalid file type",
        description: "Please select an image file",
        variant: "destructive",
      });
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      toast({
        title: "File too large",
        description: "Please select an image smaller than 5MB",
        variant: "destructive",
      });
      return;
    }

    setSelectedImage(file);

    const reader = new FileReader();
    reader.onload = (e) => {
      setImagePreview(e.target?.result as string);
    };
    reader.readAsDataURL(file);
  };

  const clearImageSelection = () => {
    setSelectedImage(null);
    setImagePreview("");
  };

  const loadPlanogramImages = async (planogramUid: string) => {
    try {
      const images = await planogramFileService.getPlanogramImages(planogramUid);
      setPlanogramImages(images);
    } catch (error) {
      console.error("Error loading images:", error);
      setPlanogramImages([]);
    }
  };

  const handleImageDelete = async (fileUid: string, planogramUid: string) => {
    try {
      await planogramFileService.deleteFile(fileUid);
      toast({
        title: "Success",
        description: "Image deleted successfully",
      });
      loadPlanogramImages(planogramUid);
    } catch (error) {
      console.error("Error deleting image:", error);
      toast({
        title: "Error",
        description: "Failed to delete image",
        variant: "destructive",
      });
    }
  };

  const handleViewImages = async (setup: PlanogramSetup) => {

    setSelectedSetup(setup);
    setIsImageDialogOpen(true);

    const setupId = setup.UID || setup.uid;
    if (setupId) {
      await loadPlanogramImages(setupId);
    }
  };

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Planogram Setup</h1>
          <p className="text-muted-foreground">
            Manage product planogram configurations for optimal shelf space
            allocation
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="icon" onClick={loadData}>
            <RefreshCw className="h-4 w-4" />
          </Button>
          <CreatePlanogramDialog onSuccess={() => loadData()} />
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
                placeholder="Search by category code or value... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Selection Type Filter Dropdown */}
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
                <DropdownMenuLabel>Filter by Selection Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={selectedTypes.includes("Category")}
                  onCheckedChange={(checked) => {
                    setSelectedTypes((prev) =>
                      checked
                        ? [...prev, "Category"]
                        : prev.filter((t) => t !== "Category")
                    );
                  }}
                >
                  Category
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedTypes.includes("Brand")}
                  onCheckedChange={(checked) => {
                    setSelectedTypes((prev) =>
                      checked
                        ? [...prev, "Brand"]
                        : prev.filter((t) => t !== "Brand")
                    );
                  }}
                >
                  Brand
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedTypes.includes("Product")}
                  onCheckedChange={(checked) => {
                    setSelectedTypes((prev) =>
                      checked
                        ? [...prev, "Product"]
                        : prev.filter((t) => t !== "Product")
                    );
                  }}
                >
                  Product
                </DropdownMenuCheckboxItem>
                {selectedTypes.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => setSelectedTypes([])}>
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>

            {selectedSetups.length > 0 && (
              <Button onClick={handleBulkDelete} variant="destructive">
                <Trash2 className="h-4 w-4 mr-2" />
                Delete ({selectedSetups.length})
              </Button>
            )}

            <Button
              variant="outline"
              size="default"
              onClick={loadData}
              disabled={loading}
            >
              <RefreshCw
                className={`h-4 w-4 ${loading ? "animate-spin" : ""}`}
              />
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
                  <TableHead className="w-[50px] pl-6">
                    <Checkbox
                      checked={
                        Array.isArray(setups) &&
                        selectedSetups.length === setups.length &&
                        setups.length > 0
                      }
                      onCheckedChange={handleSelectAll}
                    />
                  </TableHead>
                  <TableHead className="pl-2">Category Code</TableHead>
                  <TableHead>Selection Type</TableHead>
                  <TableHead>Selection Value</TableHead>
                  <TableHead className="text-center">
                    Shelf Space (cm)
                  </TableHead>
                  <TableHead className="text-center">Image</TableHead>
                  <TableHead>Modified Date</TableHead>
                  <TableHead className="text-right pr-6">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  Array.from({ length: pageSize }).map((_, index) => (
                    <TableRow key={index}>
                      <TableCell className="pl-6">
                        <Skeleton className="h-4 w-4" />
                      </TableCell>
                      <TableCell>
                        <Skeleton className="h-6 w-20 rounded-full" />
                      </TableCell>
                      <TableCell>
                        <Skeleton className="h-5 w-24" />
                      </TableCell>
                      <TableCell>
                        <Skeleton className="h-5 w-32" />
                      </TableCell>
                      <TableCell className="text-center">
                        <Skeleton className="h-5 w-16 mx-auto" />
                      </TableCell>
                      <TableCell className="text-center">
                        <Skeleton className="h-8 w-20 mx-auto" />
                      </TableCell>
                      <TableCell>
                        <Skeleton className="h-5 w-24" />
                      </TableCell>
                      <TableCell className="text-right pr-6">
                        <Skeleton className="h-8 w-8 ml-auto" />
                      </TableCell>
                    </TableRow>
                  ))
                ) : Array.isArray(setups) && setups.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} className="text-center py-12">
                      <div className="flex flex-col items-center space-y-4">
                        <LayoutGrid className="h-12 w-12 text-muted-foreground/50" />
                        <div className="space-y-2">
                          <p className="text-sm font-medium text-muted-foreground">
                            No planogram setups found
                          </p>
                          <p className="text-xs text-muted-foreground">
                            {searchTerm
                              ? "Try adjusting your search terms"
                              : "Create your first planogram setup"}
                          </p>
                        </div>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  Array.isArray(setups) &&
                  setups
                    .filter((setup) => {
                      // Apply selection type filter
                      if (selectedTypes.length > 0) {
                        const type = setup.SelectionType || setup.selectionType;
                        if (!selectedTypes.includes(type)) return false;
                      }
                      return true;
                    })
                    .map((setup, index) => (
                      <TableRow
                        key={setup.uid || setup.UID || `setup-${index}`}
                      >
                        <TableCell className="pl-6">
                          <Checkbox
                            checked={selectedSetups.includes(
                              setup.uid || setup.UID || ""
                            )}
                            onCheckedChange={(checked) =>
                              handleSelectOne(
                                setup.uid || setup.UID || "",
                                checked as boolean
                              )
                            }
                          />
                        </TableCell>
                        <TableCell className="font-medium">
                          <Badge variant="outline">
                            {setup.CategoryCode || setup.categoryCode}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          {setup.SelectionType || setup.selectionType}
                        </TableCell>
                        <TableCell>
                          {setup.SelectionValue || setup.selectionValue}
                        </TableCell>
                        <TableCell className="text-center">
                          {setup.ShareOfShelfCm || setup.shareOfShelfCm || 0}
                        </TableCell>
                        <TableCell className="text-center">
                          <Button
                            variant="outline"
                            size="sm"
                            className="h-8 w-20 p-0"
                            onClick={() => handleViewImages(setup)}
                          >
                            <ImageIcon className="h-4 w-4 mr-1" />
                            View
                          </Button>
                        </TableCell>
                        <TableCell>
                          {setup.ModifiedTime || setup.modifiedTime
                            ? new Date(
                                setup.ModifiedTime || setup.modifiedTime || ""
                              ).toLocaleDateString()
                            : "-"}
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
                              <DropdownMenuLabel>Actions</DropdownMenuLabel>
                              <DropdownMenuItem
                                onClick={() => handleEdit(setup)}
                              >
                                <Edit className="mr-2 h-4 w-4" />
                                Edit
                              </DropdownMenuItem>
                              <DropdownMenuSeparator />
                              <DropdownMenuItem
                                className="text-destructive"
                                onClick={() => handleDelete(setup)}
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
          </div>

          {/* Pagination */}
          {totalRecords > 0 && (
            <div className="px-6 py-4 border-t bg-gray-50/30">
              <PaginationControls
                currentPage={currentPage}
                totalCount={totalRecords}
                pageSize={pageSize}
                onPageChange={setCurrentPage}
                onPageSizeChange={(size) => {
                  setPageSize(size);
                  setCurrentPage(1);
                }}
                pageSizeOptions={[10, 25, 50, 100]}
              />
            </div>
          )}
        </CardContent>
      </Card>
      {/* Edit Dialog */}
      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit Planogram Setup</DialogTitle>
            <DialogDescription>
              Update planogram configuration details and manage images
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-6">
            {/* Current Selection Display */}
            {formData.SelectionType && formData.SelectionValue && (
              <div className="space-y-3">
                <Label className="text-sm font-medium">Current Selection</Label>
                <div className="text-sm bg-blue-50 border border-blue-200 p-3 rounded">
                  <div className="font-medium text-blue-900">
                    {formData.SelectionType}: {formData.SelectionValue}
                  </div>
                  <div className="text-blue-700 text-xs mt-1">
                    Category Code:{" "}
                    {formData.CategoryCode || formData.categoryCode}
                  </div>
                </div>
              </div>
            )}

            {/* Simple Category Selection */}
            <div className="space-y-3">
              <Label className="text-sm font-medium">
                Update Product Selection
              </Label>

              {/* Simple Category Dropdown */}
              <div className="space-y-2">
                <Label htmlFor="category-select">Category</Label>
                <Select
                  value={formData.SelectionValue || formData.selectionValue || ""}
                  onValueChange={(value) => {
                    console.log('Category selected:', value);
                    setFormData((prev) => ({
                      ...prev,
                      SelectionType: "Category",
                      selectionType: "Category",
                      SelectionValue: value,
                      selectionValue: value,
                      CategoryCode: value,
                      categoryCode: value,
                    }));
                  }}
                >
                  <SelectTrigger id="category-select">
                    <SelectValue placeholder="Select a category" />
                  </SelectTrigger>
                  <SelectContent>
                    {loadingCategories ? (
                      <div className="p-2 text-sm text-muted-foreground">
                        Loading categories...
                      </div>
                    ) : categoryGroups.length === 0 ? (
                      <div className="p-2 text-sm text-muted-foreground">
                        No categories available
                      </div>
                    ) : (
                      categoryGroups.map((category) => {
                        const isTopLevel = !category.ParentUID || category.ParentUID === "";
                        return (
                          <SelectItem key={category.UID} value={category.Code}>
                            <div className={`flex items-center ${isTopLevel ? 'font-semibold text-blue-700' : 'text-gray-600 pl-4'}`}>
                              {isTopLevel && '👑 '}
                              {category.Code} - {category.Name}
                              {isTopLevel && ' (Parent)'}
                            </div>
                          </SelectItem>
                        );
                      })
                    )}
                  </SelectContent>
                </Select>
              </div>

            </div>

            {/* Share of Shelf */}
            <div className="space-y-2">
              <Label htmlFor="edit-shareOfShelf">Share of Shelf (cm)</Label>
              <Input
                id="edit-shareOfShelf"
                type="number"
                value={
                  formData.ShareOfShelfCm ?? formData.shareOfShelfCm ?? 100
                }
                onChange={(e) => {
                  const value = parseInt(e.target.value) || 0;
                  setFormData((prev) => ({
                    ...prev,
                    ShareOfShelfCm: value,
                    shareOfShelfCm: value,
                  }));
                }}
                placeholder="100"
                className="w-32"
              />
              {formErrors.shareOfShelfCm && (
                <p className="text-sm text-destructive">
                  {formErrors.shareOfShelfCm}
                </p>
              )}
            </div>

            {/* Image Upload Section */}
            <div className="space-y-3">
              <Label className="text-sm font-medium">Planogram Images</Label>

              {/* Existing Images */}
              {planogramImages.length > 0 && (
                <div className="space-y-2">
                  <div className="text-sm text-muted-foreground">
                    Existing Images:
                  </div>
                  <div className="grid grid-cols-3 gap-3">
                    {planogramImages.map((image, index) => (
                      <div
                        key={image.UID || `image-${index}`}
                        className="relative border rounded overflow-hidden"
                      >
                        <img
                          src={image.RelativePath ? planogramFileService.getFileUrl(image.RelativePath) : "/placeholder-product.svg"}
                          alt={image.DisplayName || "Planogram image"}
                          className="w-full h-20 object-cover"
                          loading="lazy"
                          onError={(e) => {
                            const target = e.currentTarget as HTMLImageElement;
                            if (target.src !== "/placeholder-product.svg") {
                              target.src = "/placeholder-product.svg";
                            }
                          }}
                        />
                        <Button
                          variant="destructive"
                          size="icon"
                          className="absolute top-1 right-1 h-5 w-5"
                          onClick={() => {
                            const setupId =
                              selectedSetup?.UID || selectedSetup?.uid;
                            if (setupId) handleImageDelete(image.UID, setupId);
                          }}
                        >
                          <X className="h-3 w-3" />
                        </Button>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Add New Image */}
              <div className="space-y-2">
                <div className="text-sm text-muted-foreground">
                  Add New Image:
                </div>
                {imagePreview ? (
                  <div className="relative">
                    <img
                      src={imagePreview}
                      alt="Preview"
                      className="w-full h-32 object-cover rounded border"
                    />
                    <Button
                      variant="outline"
                      size="sm"
                      className="absolute top-2 right-2"
                      onClick={clearImageSelection}
                    >
                      <X className="h-4 w-4" />
                    </Button>
                    <div className="mt-2 text-xs text-muted-foreground">
                      {selectedImage?.name} (
                      {Math.round((selectedImage?.size || 0) / 1024)} KB)
                    </div>
                  </div>
                ) : (
                  <div
                    className="border-2 border-dashed rounded p-4 text-center cursor-pointer hover:bg-muted/50"
                    onClick={() =>
                      document.getElementById("edit-image-upload")?.click()
                    }
                  >
                    <Upload className="mx-auto h-6 w-6 text-muted-foreground mb-2" />
                    <div className="text-sm">Click to upload new image</div>
                    <div className="text-xs text-muted-foreground">
                      PNG, JPG up to 5MB
                    </div>
                    <input
                      id="edit-image-upload"
                      type="file"
                      accept="image/*"
                      className="hidden"
                      onChange={handleFileSelect}
                    />
                  </div>
                )}
              </div>
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-4">
              <Button
                variant="outline"
                onClick={() => setIsEditDialogOpen(false)}
              >
                Cancel
              </Button>
              <Button
                onClick={submitEdit}
                disabled={!formData.SelectionValue || uploading}
              >
                <Save className="h-4 w-4 mr-2" />
                {uploading ? "Uploading Image..." : "Update Planogram"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog
        open={isDeleteDialogOpen}
        onOpenChange={setIsDeleteDialogOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will permanently delete the planogram setup for category "
              {selectedSetup?.categoryCode}". This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmDelete}
              className="bg-destructive text-destructive-foreground"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Bulk Delete Confirmation Dialog */}
      <AlertDialog
        open={isBulkDeleteDialogOpen}
        onOpenChange={setIsBulkDeleteDialogOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will permanently delete {selectedSetups.length} selected
              planogram setup(s). This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmBulkDelete}
              className="bg-destructive text-destructive-foreground"
            >
              Delete All
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Image Management Dialog */}
      <Dialog open={isImageDialogOpen} onOpenChange={setIsImageDialogOpen}>
        <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              View Images - {selectedSetup?.categoryCode}
            </DialogTitle>
            <DialogDescription>
              View existing images for this planogram setup
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            {planogramImages.length > 0 ? (
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {planogramImages.map((image, index) => (
                  <Card
                    key={image.UID || `image-${index}`}
                    className="overflow-hidden"
                  >
                    <div className="aspect-square relative">
                      <img
                        src={image.RelativePath ? planogramFileService.getFileUrl(image.RelativePath) : "/placeholder-product.svg"}
                        alt={image.DisplayName || "Planogram image"}
                        className="w-full h-full object-cover"
                        loading="lazy"
                        onError={(e) => {
                          const target = e.currentTarget as HTMLImageElement;
                          if (target.src !== "/placeholder-product.svg") {
                            target.src = "/placeholder-product.svg";
                          }
                        }}
                      />
                      <Button
                        variant="destructive"
                        size="icon"
                        className="absolute top-2 right-2 h-6 w-6"
                        onClick={() => {
                          const setupId =
                            selectedSetup?.UID || selectedSetup?.uid;
                          if (setupId) handleImageDelete(image.UID, setupId);
                        }}
                      >
                        <Trash2 className="h-3 w-3" />
                      </Button>
                    </div>
                    <CardContent className="p-2">
                      <p className="text-xs text-muted-foreground truncate">
                        {image.DisplayName}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {planogramFileService.formatFileSize(image.FileSize)}
                      </p>
                    </CardContent>
                  </Card>
                ))}
              </div>
            ) : (
              <div className="text-center py-8">
                <ImageIcon className="mx-auto h-12 w-12 text-muted-foreground" />
                <p className="mt-2 text-muted-foreground">No images found</p>
                <p className="text-sm text-muted-foreground">
                  Images can be uploaded during planogram creation
                </p>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setIsImageDialogOpen(false)}
            >
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
