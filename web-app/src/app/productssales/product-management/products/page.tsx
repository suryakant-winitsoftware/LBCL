"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
  Plus,
  Search,
  Filter,
  Download,
  Upload,
  MoreHorizontal,
  Eye,
  Edit,
  Trash2,
  TreePine,
  Tags,
  Link2,
  BarChart3,
  Ruler,
  Camera,
  X,
  ImageOff,
  Loader2,
  UploadCloud,
  Image as ImageIcon,
  ChevronDown,
  List,
  Grid3X3
} from "lucide-react";
import {
  skuService,
  SKUListView,
  PagedResponse
} from "@/services/sku/sku.service";
import { PagingRequest } from "@/types/common.types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem
} from "@/components/ui/dropdown-menu";
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
import { PaginationControls } from "@/components/ui/pagination-controls";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import {
  skuImagesService,
  FileSys,
  ImageUploadRequest
} from "@/services/sku/sku-images.service";

export default function ManageSKUsPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [skus, setSKUs] = useState<SKUListView[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedSKU, setSelectedSKU] = useState<SKUListView | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [pageSize, setPageSize] = useState(10); // Start with 10 for faster loading
  const [useMasterData, setUseMasterData] = useState(true); // Default to using master data API
  const [statusFilter, setStatusFilter] = useState<string[]>([]); // Filter for active/inactive
  const searchInputRef = useRef<HTMLInputElement>(null); // Ref for search input
  const [viewMode, setViewMode] = useState<"list" | "card">("list"); // View mode state

  // Image management state
  const [imageDialogOpen, setImageDialogOpen] = useState(false);
  const [selectedSKUForImage, setSelectedSKUForImage] =
    useState<SKUListView | null>(null);
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadPreview, setUploadPreview] = useState<string>("");
  const [uploading, setUploading] = useState(false);
  const [skusWithImages, setSKUsWithImages] = useState<any[]>([]); // Same as working version
  const [imageUrls, setImageUrls] = useState<Record<string, string>>({});
  const [imageLoadingStates, setImageLoadingStates] = useState<
    Record<string, boolean>
  >({});
  const [imageErrorStates, setImageErrorStates] = useState<
    Record<string, boolean>
  >({});
  const [recentlyUploadedImages, setRecentlyUploadedImages] = useState<
    Set<string>
  >(new Set());
  const [exporting, setExporting] = useState(false);

  // COMMENTED OUT: Secondary API approach - keeping for reference
  const fetchSKUsOld = async () => {
    /*
    setLoading(true);
    try {
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: searchTerm
          ? [{ Name: "skucodeandname", Value: searchTerm }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
        IsCountRequired: true // This ensures we get total count for pagination
      };

      console.log(`📄 Fetching page ${currentPage} with size ${pageSize}`);

      // Use the same approach as promotions service
      const response = await skuService.getAllSKUs(request);

      if (response && response.success !== false) {
        let skuData: SKUListView[] = [];

        // Handle multiple response structures - same as promotions
        const dataSource = response.data || response;

        // Check for nested Data.PagedData structure (backend wraps response)
        if (
          dataSource.Data?.PagedData &&
          Array.isArray(dataSource.Data.PagedData)
        ) {
          skuData = dataSource.Data.PagedData;
          // Use TotalCount from server for proper pagination
          setTotalCount(dataSource.Data.TotalCount || 0);
          console.log(
            `✅ Loaded ${skuData.length} items, Total: ${dataSource.Data.TotalCount}`
          );
        } else if (
          dataSource.PagedData &&
          Array.isArray(dataSource.PagedData)
        ) {
          skuData = dataSource.PagedData;
          setTotalCount(dataSource.TotalCount || 0);
          console.log(
            `✅ Loaded ${skuData.length} items, Total: ${dataSource.TotalCount}`
          );
        } else if (dataSource.Data && Array.isArray(dataSource.Data)) {
          skuData = dataSource.Data;
          // Warning: This structure doesn't provide server-side pagination
          setTotalCount(dataSource.TotalCount || dataSource.Data.length);
          console.warn(
            "⚠️ Using fallback structure - may not have proper pagination"
          );
        } else if (dataSource.items && Array.isArray(dataSource.items)) {
          skuData = dataSource.items;
          setTotalCount(dataSource.totalCount || dataSource.items.length);
        } else if (Array.isArray(dataSource)) {
          // This shouldn't happen with proper pagination
          console.warn(
            "⚠️ Received raw array - pagination may not work correctly"
          );
          skuData = dataSource.slice(
            (currentPage - 1) * pageSize,
            currentPage * pageSize
          );
          setTotalCount(dataSource.length);
        } else {
          console.error("❌ Unexpected response structure:", dataSource);
        }

        // Log first SKU to see field names
        if (skuData.length > 0) {
        }

        // Map the actual field names to expected field names
        const mappedSKUs = skuData.map((sku: any) => ({
          SKUUID: sku.UID || sku.Id || sku.Code, // Map UID to SKUUID
          SKUCode: sku.Code || sku.UID, // Map Code to SKUCode
          SKULongName: sku.LongName || sku.Name || sku.AliasName, // Map LongName to SKULongName
          IsActive: sku.IsActive !== false, // Keep IsActive as is
          // Keep all original fields for debugging
          ...sku
        }));

        setSKUs(mappedSKUs);

        // Log pagination info
        console.log(
          `📊 Page ${currentPage}/${Math.ceil(totalCount / pageSize)} loaded`
        );
      } else {
        setSKUs([]);
        setTotalCount(0);

        toast({
          title: "No Data",
          description: response.error || "No products found or API error",
          variant: "default"
        });
      }
    } catch (error) {
      toast({
        title: "Error",
        description: `Failed to fetch SKUs: ${
          error instanceof Error ? error.message : "Unknown error"
        }`,
        variant: "destructive"
      });
      // Set empty arrays on error
      setSKUs([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
    */
  };

  // Primary method - use the optimized backend endpoint with proper pagination
  const fetchSKUs = async () => {
    await fetchSKUsWebView(currentPage, pageSize);
  };

  // Primary API endpoint - Using SelectAllSKUDetailsWebView with proper backend pagination
  const fetchSKUsWebView = useCallback(
    async (page: number = currentPage, size: number = pageSize) => {
      setLoading(true);
      try {
        // Use the same endpoint the promotion service uses for all SKU data
        // Build request body matching backend PagingRequest model
        const filterCriterias = [];

        // Add search filter if present
        if (searchTerm && searchTerm.trim()) {
          filterCriterias.push({
            Name: "skucodeandname",
            Value: searchTerm.trim()
          });
        }

        // Add status filter if selected
        if (statusFilter.length > 0) {
          // If only one status is selected, filter by IsActive
          if (statusFilter.length === 1) {
            filterCriterias.push({
              Name: "IsActive",
              Value: statusFilter[0] === "Active" ? "true" : "false"
            });
          }
          // If both are selected, no need to filter (show all)
        }

        const requestBody = {
          PageNumber: page,
          PageSize: size,
          IsCountRequired: true, // Essential for getting total count
          FilterCriterias: filterCriterias,
          SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }]
        };

        console.log(
          `📊 Fetching SKUs WebView - Page: ${page}, Size: ${size}`,
          searchTerm ? `Search: ${searchTerm}` : ""
        );

        const response = await fetch(
          `${
            process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
          }/SKU/SelectAllSKUDetailsWebView`,
          {
            method: "POST",
            headers: {
              Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
              "Content-Type": "application/json",
              Accept: "application/json"
            },
            body: JSON.stringify(requestBody)
          }
        );

        if (response.ok) {
          const result = await response.json();

          if (result.IsSuccess && result.Data) {
            const data = result.Data;
            let skuData: any[] = [];

            // Handle PagedResponse structure from backend
            if (data.PagedData && Array.isArray(data.PagedData)) {
              skuData = data.PagedData.map((sku: any) => ({
                SKUUID: sku.SKUUID || sku.UID || sku.Id,
                SKUCode: sku.SKUCode || sku.Code,
                SKULongName: sku.SKULongName || sku.LongName || sku.Name,
                IsActive: sku.IsActive !== false,
                ...sku
              }));
            }
            setSKUs(skuData);
            setTotalCount(data.TotalCount || skuData.length);

            console.log(
              `✅ Loaded page ${page}/${Math.ceil(
                (data.TotalCount || skuData.length) / size
              )} - ${skuData.length} items, Total: ${
                data.TotalCount || skuData.length
              }`
            );
          } else {
            // Handle empty response
            setSKUs([]);
            setTotalCount(0);
            toast({
              title: "No Data",
              description: "No products found matching your criteria",
              variant: "default"
            });
          }
        } else {
          // Handle non-OK response
          const errorData = await response.json().catch(() => null);
          toast({
            title: "Error",
            description: errorData?.Message || "Failed to fetch products",
            variant: "destructive"
          });
          setSKUs([]);
          setTotalCount(0);
        }
      } catch (error) {
        console.error("❌ Error fetching SKU master data:", error);
        toast({
          title: "Error",
          description: `Failed to fetch products: ${
            error instanceof Error ? error.message : "Unknown error"
          }`,
          variant: "destructive"
        });
        setSKUs([]);
        setTotalCount(0);
      } finally {
        setLoading(false);
      }
    },
    [toast]
  );

  // Debounce search to reduce API calls
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState(searchTerm);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 300); // 300ms delay

    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Check for Ctrl+F (Windows/Linux) or Cmd+F (Mac)
      if ((e.ctrlKey || e.metaKey) && e.key === "f") {
        e.preventDefault(); // Prevent browser's default find
        searchInputRef.current?.focus();
        searchInputRef.current?.select(); // Select existing text for easy replacement
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, []);

  useEffect(() => {
    // Use optimized WebView endpoint with proper pagination
    fetchSKUsWebView(currentPage, pageSize);
  }, [currentPage, debouncedSearchTerm, pageSize, statusFilter]);

  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page when searching
  };

  const handleViewDetails = (skuUID: string | undefined) => {
    if (!skuUID) return;
    router.push(
      `/productssales/product-management/products/view?uid=${skuUID}`
    );
  };

  const handleEdit = (skuUID: string | undefined) => {
    if (!skuUID) return;
    router.push(
      `/productssales/product-management/products/edit?uid=${skuUID}`
    );
  };

  const handleDeleteClick = (sku: SKUListView) => {
    setSelectedSKU(sku);
    setDeleteDialogOpen(true);
  };

  // Exact same fetch function as working SKUImageManager
  const fetchSKUsWithImages = async () => {
    setLoading(true);
    try {
      const result = await skuImagesService.getSKUsWithImages(
        1, // currentPage
        1000, // pageSize - get all
        undefined // searchTerm
      );
      setSKUsWithImages(result.skusWithImages);

      // Generate image URLs for all images - exact same logic as working version
      const urls: Record<string, string> = {};
      for (const skuWithImages of result.skusWithImages) {
        for (const image of skuWithImages.images) {
          if (!urls[image.UID]) {
            urls[image.UID] = await skuImagesService.getImageBlob(image);
          }
        }
      }
      setImageUrls(urls);

      console.log("Loaded SKUs with images:", result.skusWithImages.length);
      console.log("Generated image URLs:", Object.keys(urls).length);
    } catch (error) {
      console.error("Failed to fetch SKU images:", error);
      toast({
        title: "Error",
        description: "Failed to fetch SKU images",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const handleImageUpload = (sku: SKUListView) => {
    setSelectedSKUForImage(sku);
    setImageDialogOpen(true);
    setUploadFile(null);
    setUploadPreview("");
  };

  // Function to get existing images for a SKU
  const getSkuImages = (skuUID: string | undefined) => {
    if (!skuUID) return [];
    const skuWithImages = skusWithImages.find(
      (swi) => swi.sku?.SKUUID === skuUID || swi.sku?.UID === skuUID
    );
    return skuWithImages?.images || [];
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith("image/")) {
      toast({
        title: "Invalid File",
        description: "Please select an image file",
        variant: "destructive"
      });
      return;
    }

    // Validate file size (5MB max)
    if (file.size > 5 * 1024 * 1024) {
      toast({
        title: "File Too Large",
        description: "Please select an image smaller than 5MB",
        variant: "destructive"
      });
      return;
    }

    setUploadFile(file);

    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
      setUploadPreview(e.target?.result as string);
    };
    reader.readAsDataURL(file);
  };

  // Modified upload function to REPLACE existing image instead of adding multiple
  const handleUpload = async () => {
    if (!uploadFile || !selectedSKUForImage) return;

    setUploading(true);
    try {
      const base64Data = await skuImagesService.fileToBase64(uploadFile);
      const skuUID = selectedSKUForImage.SKUUID || selectedSKUForImage.UID;

      // Check if there are existing images for this SKU
      const existingImages = getSkuImages(skuUID);

      // If there are existing images, delete the first/default one
      if (existingImages.length > 0) {
        console.log("Found existing images, will replace the main image");
        const imageToReplace =
          existingImages.find((img) => img.IsDefault) || existingImages[0];

        if (imageToReplace && imageToReplace.UID) {
          console.log("Deleting existing image:", imageToReplace.UID);
          try {
            await skuImagesService.deleteSKUImage(imageToReplace.UID);
            console.log("Successfully deleted old image");
          } catch (deleteError) {
            console.error("Failed to delete existing image:", deleteError);
            // Continue with upload even if delete fails
          }
        }
      }

      // Now upload the new image
      const uploadRequest: ImageUploadRequest = {
        linkedItemType: "SKU",
        linkedItemUID: skuUID,
        fileSysType: "Image",
        fileData: base64Data,
        fileType: uploadFile.type,
        fileName: uploadFile.name,
        displayName: `${
          selectedSKUForImage.SKUCode || selectedSKUForImage.Code
        } - ${uploadFile.name}`,
        fileSize: uploadFile.size,
        isDefault: true // Always set as default since we're replacing
      };

      console.log("=== UPLOAD DEBUG ===");
      console.log("Replacing image for SKU:", skuUID);
      console.log("Upload request:", uploadRequest);

      const result = await skuImagesService.uploadSKUImage(uploadRequest);
      console.log("Upload result:", result);

      toast({
        title: "Image Updated Successfully",
        description:
          existingImages.length > 0
            ? "Product image has been replaced"
            : "Product image has been added"
      });

      // Mark this SKU as recently uploaded (reuse skuUID from above)
      setRecentlyUploadedImages((prev) => new Set(prev).add(skuUID));

      // Clear the recently uploaded indicator after 5 seconds
      setTimeout(() => {
        setRecentlyUploadedImages((prev) => {
          const newSet = new Set(prev);
          newSet.delete(skuUID);
          return newSet;
        });
      }, 5000);

      // Reset upload state exactly like working version
      setImageDialogOpen(false);
      setUploadFile(null);
      setUploadPreview("");
      setSelectedSKUForImage(null);

      // Refresh all images using same approach as working version
      console.log("Refreshing all images after upload...");
      await new Promise((resolve) => setTimeout(resolve, 1000)); // Small delay
      await loadImagesForProductsSKUs();
    } catch (error) {
      console.error("Upload error:", error);
      toast({
        title: "Error",
        description: "Failed to upload image",
        variant: "destructive"
      });
    } finally {
      setUploading(false);
    }
  };

  // Use exact same approach as working images page but for products table SKUs
  useEffect(() => {
    if (skus.length > 0) {
      loadImagesForProductsSKUs();
    }
  }, [skus]);

  const loadImagesForProductsSKUs = async () => {
    setLoading(true);
    try {
      console.log(
        "Loading images using working page approach for",
        skus.length,
        "products"
      );

      // Step 1: Get ALL images from database (exact same as working version)
      const fileSysRequest = {
        PageNumber: 1,
        PageSize: 1000,
        IsCountRequired: true,
        FilterCriterias: [], // Empty like working version
        SortCriterias: []
      };

      console.log("Fetching ALL SKU images from database...");
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/FileSys/SelectAllFileSysDetails`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(fileSysRequest)
        }
      );

      if (!response.ok) {
        throw new Error(`FileSys API error! status: ${response.status}`);
      }

      const fileSysResult = await response.json();
      console.log("Raw FileSys API response structure:", {
        IsSuccess: fileSysResult?.IsSuccess,
        DataExists: !!fileSysResult?.Data,
        PagedDataExists: !!fileSysResult?.Data?.PagedData,
        PagedDataLength: fileSysResult?.Data?.PagedData?.length || 0
      });

      const allFileSysRecords = fileSysResult?.Data?.PagedData || [];
      console.log("Total FileSys records from API:", allFileSysRecords.length);

      // Log first few records to see structure
      if (allFileSysRecords.length > 0) {
        console.log("First FileSys record example:", allFileSysRecords[0]);
      }

      const allSkuImages = allFileSysRecords.filter(
        (fileSys: any) =>
          fileSys.LinkedItemType === "SKU" && fileSys.FileSysType === "Image"
      );

      console.log("Filtered SKU images:", allSkuImages.length);
      if (allSkuImages.length > 0) {
        console.log("First SKU image example:", allSkuImages[0]);
      }

      // Step 2: Group images by SKU UID (same as working version)
      const imagesBySkuUID = new Map();
      allSkuImages.forEach((image: any) => {
        const skuUID = image.LinkedItemUID;
        if (!imagesBySkuUID.has(skuUID)) {
          imagesBySkuUID.set(skuUID, []);
        }
        imagesBySkuUID.get(skuUID).push(image);
      });

      // Step 3: Combine products SKUs with their images
      const productsWithImages = skus.map((sku: any) => {
        const skuUID = sku.SKUUID || sku.UID;
        const images = imagesBySkuUID.get(skuUID) || [];
        const defaultImage =
          images.find((img: any) => img.IsDefault) || images[0];

        console.log("SKU", skuUID, "has", images.length, "images");

        return {
          sku: sku,
          images,
          defaultImage
        };
      });

      setSKUsWithImages(productsWithImages);

      // Step 4: Generate image URLs (same as working version)
      const urls: Record<string, string> = {};
      const loadingStates: Record<string, boolean> = {};
      const errorStates: Record<string, boolean> = {};

      for (const skuWithImages of productsWithImages) {
        for (const image of skuWithImages.images) {
          if (!urls[image.UID]) {
            loadingStates[image.UID] = true;
            try {
              urls[image.UID] = await skuImagesService.getImageBlob(image);
              loadingStates[image.UID] = false;
            } catch (error) {
              console.error(`Failed to load image ${image.UID}:`, error);
              errorStates[image.UID] = true;
              loadingStates[image.UID] = false;
            }
          }
        }
      }
      setImageUrls(urls);
      setImageLoadingStates(loadingStates);
      setImageErrorStates(errorStates);

      console.log(
        "Generated image URLs for",
        Object.keys(urls).length,
        "images"
      );
    } catch (error) {
      console.error("Failed to load images for products:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!selectedSKU) return;

    setDeleting(true);
    try {
      const skuId = selectedSKU.SKUUID || selectedSKU.UID;
      if (!skuId) {
        throw new Error("No valid SKU ID found");
      }
      await skuService.deleteSKU(skuId);
      toast({
        title: "Success",
        description: `Product "${
          selectedSKU.SKULongName || selectedSKU.LongName || selectedSKU.Name
        }" has been deleted successfully.`
      });
      // Refresh the list
      fetchSKUs();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete product. Please try again.",
        variant: "destructive"
      });
    } finally {
      setDeleting(false);
      setDeleteDialogOpen(false);
      setSelectedSKU(null);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">Manage Products</h1>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={async () => {
              setExporting(true);
              try {
                console.log("Starting export of all products...");

                // Step 1: Get total count first
                const filterCriterias = [];

                // Add search filter if present
                if (searchTerm && searchTerm.trim()) {
                  filterCriterias.push({
                    Name: "skucodeandname",
                    Value: searchTerm.trim()
                  });
                }

                // Add status filter if selected
                if (statusFilter.length === 1) {
                  filterCriterias.push({
                    Name: "IsActive",
                    Value: statusFilter[0] === "Active" ? "true" : "false"
                  });
                }

                const countRequestBody = {
                  PageNumber: 1,
                  PageSize: 1, // Just get 1 item to get total count
                  IsCountRequired: true,
                  FilterCriterias: filterCriterias,
                  SortCriterias: [
                    { SortParameter: "SKUCode", Direction: "Asc" }
                  ]
                };

                const countResponse = await fetch(
                  `${
                    process.env.NEXT_PUBLIC_API_URL ||
                    "http://localhost:8000/api"
                  }/SKU/SelectAllSKUDetailsWebView`,
                  {
                    method: "POST",
                    headers: {
                      Authorization: `Bearer ${localStorage.getItem(
                        "auth_token"
                      )}`,
                      "Content-Type": "application/json",
                      Accept: "application/json"
                    },
                    body: JSON.stringify(countRequestBody)
                  }
                );

                if (!countResponse.ok) {
                  throw new Error(
                    `Count API error! status: ${countResponse.status}`
                  );
                }

                const countResult = await countResponse.json();
                const totalCount = countResult?.Data?.TotalCount || 0;
                console.log(`Total products to export: ${totalCount}`);

                if (totalCount === 0) {
                  toast({
                    title: "No Data",
                    description: "No products found to export.",
                    variant: "default"
                  });
                  return;
                }

                // Step 2: Fetch all products in batches
                const batchSize = 5000; // Safe batch size for most systems
                const totalPages = Math.ceil(totalCount / batchSize);
                let allSKUData: any[] = [];

                console.log(
                  `Will fetch ${totalPages} batches of ${batchSize} products each`
                );

                for (let page = 1; page <= totalPages; page++) {
                  console.log(`Fetching batch ${page}/${totalPages}...`);

                  const batchRequestBody = {
                    PageNumber: page,
                    PageSize: batchSize,
                    IsCountRequired: false, // No need for count in batch requests
                    FilterCriterias: filterCriterias, // Use same filters as count request
                    SortCriterias: [
                      { SortParameter: "SKUCode", Direction: "Asc" }
                    ]
                  };

                  const batchResponse = await fetch(
                    `${
                      process.env.NEXT_PUBLIC_API_URL ||
                      "http://localhost:8000/api"
                    }/SKU/SelectAllSKUDetailsWebView`,
                    {
                      method: "POST",
                      headers: {
                        Authorization: `Bearer ${localStorage.getItem(
                          "auth_token"
                        )}`,
                        "Content-Type": "application/json",
                        Accept: "application/json"
                      },
                      body: JSON.stringify(batchRequestBody)
                    }
                  );

                  if (!batchResponse.ok) {
                    throw new Error(
                      `Batch ${page} API error! status: ${batchResponse.status}`
                    );
                  }

                  const batchResult = await batchResponse.json();

                  if (
                    batchResult.IsSuccess &&
                    batchResult.Data &&
                    batchResult.Data.PagedData
                  ) {
                    const batchData = batchResult.Data.PagedData.map(
                      (sku: any) => ({
                        SKUUID: sku.SKUUID || sku.UID || sku.Id,
                        SKUCode: sku.SKUCode || sku.Code,
                        SKULongName:
                          sku.SKULongName || sku.LongName || sku.Name,
                        IsActive: sku.IsActive !== false,
                        ...sku
                      })
                    );

                    allSKUData.push(...batchData);
                    console.log(
                      `Batch ${page} complete. Total collected: ${allSKUData.length}/${totalCount}`
                    );
                  }

                  // Small delay between requests to avoid overwhelming the server
                  await new Promise((resolve) => setTimeout(resolve, 100));
                }

                console.log(
                  `Successfully fetched all ${allSKUData.length} products for export`
                );

                // Create custom export with only essential fields
                const headers = ["SKU Code", "Product Name", "Status"];

                // Map ALL SKUs to only include essential fields
                const exportData = allSKUData.map((sku) => [
                  sku.SKUCode || sku.Code || "",
                  sku.SKULongName || sku.LongName || sku.Name || "",
                  sku.IsActive ? "Active" : "Inactive"
                ]);

                // Create CSV content with headers
                const csvContent = [
                  headers.join(","),
                  ...exportData.map((row) =>
                    row
                      .map((field) =>
                        typeof field === "string" && field.includes(",")
                          ? `"${field}"`
                          : field
                      )
                      .join(",")
                  )
                ].join("\n");

                // Create and download the file
                const blob = new Blob([csvContent], {
                  type: "text/csv;charset=utf-8;"
                });
                const url = URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = `products-export-${
                  new Date().toISOString().split("T")[0]
                }.csv`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);

                toast({
                  title: "Export Complete",
                  description: `${exportData.length} products exported successfully to CSV file.`
                });
              } catch (error) {
                console.error("Export error:", error);
                toast({
                  title: "Export Failed",
                  description: "Failed to export products. Please try again.",
                  variant: "destructive"
                });
              } finally {
                setExporting(false);
              }
            }}
            disabled={exporting}
          >
            {exporting ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Exporting...
              </>
            ) : (
              <>
                <Download className="h-4 w-4 mr-2" />
                Export
              </>
            )}
          </Button>
          <Button
            onClick={() =>
              router.push("/productssales/product-management/products/create")
            }
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Product
          </Button>
        </div>
      </div>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3 px-4">
          <div className="flex items-center gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by product name or code... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 h-9 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* View Toggle - Commented out for now */}
            {/* <div className="flex items-center bg-gray-100 rounded-md p-0.5">
              <Button
                variant={viewMode === "list" ? "default" : "ghost"}
                size="sm"
                onClick={() => setViewMode("list")}
                className={`px-3 h-8 rounded ${viewMode === "list" ? "shadow-sm" : "hover:bg-transparent"}`}
              >
                <List className="h-4 w-4" />
              </Button>
              <Button
                variant={viewMode === "card" ? "default" : "ghost"}
                size="sm"
                onClick={() => setViewMode("card")}
                className={`px-3 h-8 rounded ${viewMode === "card" ? "shadow-sm" : "hover:bg-transparent"}`}
              >
                <Grid3X3 className="h-4 w-4" />
              </Button>
            </div> */}

            {/* Status Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" className="h-9 px-3">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {statusFilter.length > 0 && (
                    <Badge
                      variant="secondary"
                      className="ml-2 px-1.5 py-0 text-xs"
                    >
                      {statusFilter.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-1" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={statusFilter.includes("Active")}
                  onCheckedChange={(checked) => {
                    setStatusFilter((prev) =>
                      checked
                        ? [...prev, "Active"]
                        : prev.filter((s) => s !== "Active")
                    );
                    setCurrentPage(1); // Reset to first page when filter changes
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-green-500 rounded-full" />
                    Active
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={statusFilter.includes("Inactive")}
                  onCheckedChange={(checked) => {
                    setStatusFilter((prev) =>
                      checked
                        ? [...prev, "Inactive"]
                        : prev.filter((s) => s !== "Inactive")
                    );
                    setCurrentPage(1); // Reset to first page when filter changes
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-red-500 rounded-full" />
                    Inactive
                  </div>
                </DropdownMenuCheckboxItem>
                {statusFilter.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => {
                        setStatusFilter([]);
                        setCurrentPage(1);
                      }}
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

      {/* Products Management */}
      <div className="space-y-6">
        {viewMode === "list" ? (
          <Card className="shadow-sm">
            <div className="overflow-hidden rounded-lg">
              <Table>
                <TableHeader>
                  <TableRow className="border-b bg-gray-50/50">
                    <TableHead className="pl-6">SKU Code</TableHead>
                    <TableHead>Product Name</TableHead>
                    <TableHead>Image</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right pr-6">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {loading ? (
                    <>
                      {[...Array(pageSize)].map((_, index) => (
                        <TableRow key={`skeleton-${index}`}>
                          <TableCell className="pl-6 py-2">
                            <Skeleton className="h-4 w-24" />
                          </TableCell>
                          <TableCell className="py-2">
                            <Skeleton className="h-4 w-48" />
                          </TableCell>
                          <TableCell className="py-2">
                            <Skeleton className="h-10 w-10 rounded-md" />
                          </TableCell>
                          <TableCell className="py-2">
                            <Skeleton className="h-5 w-16 rounded-full" />
                          </TableCell>
                          <TableCell className="text-right pr-6 py-2">
                            <Skeleton className="h-7 w-7 rounded ml-auto" />
                          </TableCell>
                        </TableRow>
                      ))}
                    </>
                  ) : skus.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={5} className="text-center py-8">
                        No products found
                      </TableCell>
                    </TableRow>
                  ) : (
                    skus.map((sku, index) => {
                      const skuUID = sku.SKUUID || sku.UID;

                      // Find matching SKU in skusWithImages (same logic as working version)
                      const skuWithImages = skusWithImages.find(
                        (swi) =>
                          swi.sku.SKUUID === skuUID || swi.sku.UID === skuUID
                      );

                      const images = skuWithImages?.images || [];
                      const defaultImage =
                        skuWithImages?.defaultImage || images[0];
                      const isRecentlyUploaded =
                        recentlyUploadedImages.has(skuUID);

                      // Debug logging
                      if (index === 0 || images.length > 0) {
                        console.log("=== DISPLAY DEBUG Row", index, "===");
                        console.log("SKU UID used for lookup:", skuUID);
                        console.log("Found skuWithImages:", !!skuWithImages);
                        console.log("Images found:", images.length);
                        console.log("Default image:", defaultImage?.UID);
                        console.log(
                          "Image URL:",
                          defaultImage ? imageUrls[defaultImage.UID] : "none"
                        );
                      }

                      return (
                        <TableRow
                          key={
                            sku.SKUUID || sku.UID || sku.Code || `sku-${index}`
                          }
                          className="hover:bg-gray-50/50 transition-colors"
                        >
                          <TableCell className="font-medium pl-6 py-2">
                            <span className="text-sm">
                              {sku.SKUCode || sku.Code}
                            </span>
                          </TableCell>
                          <TableCell className="py-2">
                            <span className="text-sm">
                              {sku.SKULongName || sku.LongName || sku.Name}
                            </span>
                          </TableCell>
                          <TableCell className="py-2">
                            <div className="flex items-center gap-3">
                              <div className="relative group">
                                {isRecentlyUploaded && (
                                  <div className="absolute -top-2 -right-2 z-10">
                                    <Badge
                                      variant="default"
                                      className="text-[10px] px-1.5 py-0.5 animate-pulse bg-green-500"
                                    >
                                      New
                                    </Badge>
                                  </div>
                                )}
                                {defaultImage ? (
                                  <div className="relative">
                                    {imageLoadingStates[defaultImage.UID] ? (
                                      // Loading state
                                      <div
                                        className="h-10 w-10 rounded-md border border-gray-200 bg-gray-50 flex items-center justify-center"
                                        title="Loading image..."
                                      >
                                        <Loader2 className="h-4 w-4 text-gray-400 animate-spin" />
                                      </div>
                                    ) : imageErrorStates[defaultImage.UID] ||
                                      !imageUrls[defaultImage.UID] ? (
                                      // Error state or no URL
                                      <div
                                        className="h-10 w-10 rounded-md border border-red-200 bg-red-50 flex flex-col items-center justify-center cursor-pointer group"
                                        onClick={() => handleImageUpload(sku)}
                                        title="Failed to load image - Click to upload new"
                                      >
                                        <ImageOff className="h-3 w-3 text-red-400" />
                                        <span className="text-[8px] text-red-400 font-medium">
                                          Failed
                                        </span>
                                      </div>
                                    ) : (
                                      // Image loaded successfully
                                      <>
                                        <img
                                          src={imageUrls[defaultImage.UID]}
                                          alt={
                                            sku.SKULongName ||
                                            sku.LongName ||
                                            sku.Name
                                          }
                                          title={
                                            sku.SKULongName ||
                                            sku.LongName ||
                                            sku.Name
                                          }
                                          className="h-10 w-10 rounded-md object-cover border border-gray-200 transition-transform group-hover:scale-105"
                                          onError={(e) => {
                                            setImageErrorStates((prev) => ({
                                              ...prev,
                                              [defaultImage.UID]: true
                                            }));
                                          }}
                                        />
                                        {images.length > 1 && (
                                          <div className="absolute -bottom-1 -right-1 bg-primary text-primary-foreground text-xs rounded-full w-5 h-5 flex items-center justify-center font-semibold">
                                            {images.length}
                                          </div>
                                        )}
                                        <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity rounded-md flex items-center justify-center">
                                          <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={() =>
                                              handleImageUpload(sku)
                                            }
                                            className="h-6 w-6 p-0 bg-white/90 hover:bg-white"
                                            title="Replace Image"
                                          >
                                            <UploadCloud className="h-3 w-3 text-gray-700" />
                                          </Button>
                                        </div>
                                      </>
                                    )}
                                  </div>
                                ) : (
                                  <button
                                    onClick={() => handleImageUpload(sku)}
                                    className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-primary/50 transition-all flex flex-col items-center justify-center cursor-pointer group"
                                    title="Upload Product Image"
                                  >
                                    <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-primary" />
                                    <span className="text-[8px] text-gray-500 group-hover:text-primary font-medium">
                                      Upload
                                    </span>
                                  </button>
                                )}
                              </div>
                            </div>
                          </TableCell>
                          <TableCell className="py-2">
                            <Badge
                              variant={sku.IsActive ? "default" : "secondary"}
                              className={
                                sku.IsActive
                                  ? "bg-green-100 text-green-800 hover:bg-green-100 text-xs"
                                  : "bg-gray-100 text-gray-600 hover:bg-gray-100 text-xs"
                              }
                            >
                              {sku.IsActive ? "Active" : "Inactive"}
                            </Badge>
                          </TableCell>
                          <TableCell className="text-right pr-6 py-2">
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
                                  onClick={() =>
                                    handleViewDetails(sku.SKUUID || sku.UID)
                                  }
                                  className="cursor-pointer"
                                >
                                  <Eye className="mr-2 h-4 w-4" />
                                  View Details
                                </DropdownMenuItem>
                                <DropdownMenuItem
                                  onClick={() =>
                                    handleEdit(sku.SKUUID || sku.UID)
                                  }
                                  className="cursor-pointer"
                                >
                                  <Edit className="mr-2 h-4 w-4" />
                                  Edit Product
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

            {totalCount > 0 && (
              <div className="px-6 py-4 border-t bg-gray-50/30">
                <PaginationControls
                  currentPage={currentPage}
                  totalCount={totalCount}
                  pageSize={pageSize}
                  onPageChange={(page) => {
                    setCurrentPage(page);
                    window.scrollTo({ top: 0, behavior: "smooth" });
                  }}
                  onPageSizeChange={(size) => {
                    setPageSize(size);
                    setCurrentPage(1); // Reset to first page when changing page size
                  }}
                  itemName="products"
                />
              </div>
            )}
          </Card>
        ) : (
          /* Card View */
          <div>
            {loading ? (
              /* Loading skeleton for cards */
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                {[...Array(pageSize)].map((_, index) => (
                  <Card key={`card-skeleton-${index}`} className="shadow-sm">
                    <CardContent className="p-4">
                      <Skeleton className="h-32 w-full rounded-md mb-3" />
                      <Skeleton className="h-4 w-20 mb-2" />
                      <Skeleton className="h-4 w-full mb-2" />
                      <Skeleton className="h-6 w-16" />
                    </CardContent>
                  </Card>
                ))}
              </div>
            ) : skus.length === 0 ? (
              <Card className="shadow-sm">
                <CardContent className="py-16 text-center">
                  <ImageOff className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                  <p className="text-gray-500">No products found</p>
                </CardContent>
              </Card>
            ) : (
              <>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                  {skus.map((sku, index) => {
                    const skuUID = sku.SKUUID || sku.UID;
                    const skuWithImages = skusWithImages.find(
                      (swi) =>
                        swi.sku.SKUUID === skuUID || swi.sku.UID === skuUID
                    );
                    const images = skuWithImages?.images || [];
                    const defaultImage =
                      skuWithImages?.defaultImage || images[0];
                    const isRecentlyUploaded =
                      recentlyUploadedImages.has(skuUID);

                    return (
                      <Card
                        key={
                          sku.SKUUID || sku.UID || sku.Code || `card-${index}`
                        }
                        className="shadow-sm hover:shadow-md transition-shadow cursor-pointer"
                      >
                        <CardContent className="p-4">
                          {/* Image Section */}
                          <div className="relative mb-3">
                            {defaultImage ? (
                              <div className="relative group">
                                {imageLoadingStates[defaultImage.UID] ? (
                                  <div
                                    className="h-32 w-full rounded-md border border-gray-200 bg-gray-50 flex items-center justify-center"
                                    title="Loading image..."
                                  >
                                    <Loader2 className="h-6 w-6 text-gray-400 animate-spin" />
                                  </div>
                                ) : imageErrorStates[defaultImage.UID] ||
                                  !imageUrls[defaultImage.UID] ? (
                                  <div
                                    className="h-32 w-full rounded-md border border-red-200 bg-red-50 flex flex-col items-center justify-center cursor-pointer"
                                    onClick={() => handleImageUpload(sku)}
                                    title="Failed to load image - Click to upload new"
                                  >
                                    <ImageOff className="h-8 w-8 text-red-400 mb-2" />
                                    <span className="text-xs text-red-400 font-medium">
                                      Failed
                                    </span>
                                  </div>
                                ) : (
                                  <>
                                    <img
                                      src={imageUrls[defaultImage.UID]}
                                      alt={
                                        sku.SKULongName ||
                                        sku.LongName ||
                                        sku.Name
                                      }
                                      className="h-32 w-full rounded-md object-cover border border-gray-200"
                                      onError={(e) => {
                                        setImageErrorStates((prev) => ({
                                          ...prev,
                                          [defaultImage.UID]: true
                                        }));
                                      }}
                                    />
                                    <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity rounded-md flex items-center justify-center gap-2">
                                      <Button
                                        variant="secondary"
                                        size="sm"
                                        onClick={() =>
                                          handleViewDetails(skuUID)
                                        }
                                        className="h-8 px-2"
                                      >
                                        <Eye className="h-4 w-4" />
                                      </Button>
                                      <Button
                                        variant="secondary"
                                        size="sm"
                                        onClick={() => handleEdit(skuUID)}
                                        className="h-8 px-2"
                                      >
                                        <Edit className="h-4 w-4" />
                                      </Button>
                                      <Button
                                        variant="secondary"
                                        size="sm"
                                        onClick={() => handleImageUpload(sku)}
                                        className="h-8 px-2"
                                      >
                                        <Camera className="h-4 w-4" />
                                      </Button>
                                    </div>
                                  </>
                                )}
                                {isRecentlyUploaded && (
                                  <Badge
                                    variant="default"
                                    className="absolute -top-2 -right-2 text-xs bg-green-500"
                                  >
                                    New
                                  </Badge>
                                )}
                              </div>
                            ) : (
                              <button
                                onClick={() => handleImageUpload(sku)}
                                className="h-32 w-full rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-primary/50 transition-all flex flex-col items-center justify-center"
                              >
                                <UploadCloud className="h-8 w-8 text-gray-400 mb-2" />
                                <span className="text-xs text-gray-500">
                                  Upload Image
                                </span>
                              </button>
                            )}
                          </div>

                          {/* Product Details */}
                          <div className="space-y-2">
                            <div className="flex items-center justify-between">
                              <span className="text-xs text-gray-500 font-medium">
                                {sku.SKUCode || sku.Code}
                              </span>
                              <Badge
                                variant={sku.IsActive ? "default" : "secondary"}
                                className={
                                  sku.IsActive
                                    ? "bg-green-100 text-green-800 hover:bg-green-100 text-xs px-2 py-0"
                                    : "bg-gray-100 text-gray-600 hover:bg-gray-100 text-xs px-2 py-0"
                                }
                              >
                                {sku.IsActive ? "Active" : "Inactive"}
                              </Badge>
                            </div>
                            <h3 className="font-medium text-sm line-clamp-2 min-h-[2.5rem]">
                              {sku.SKULongName || sku.LongName || sku.Name}
                            </h3>
                            <div className="flex justify-end pt-2">
                              <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                  <Button
                                    variant="ghost"
                                    className="h-8 w-8 p-0"
                                  >
                                    <span className="sr-only">Open menu</span>
                                    <MoreHorizontal className="h-4 w-4" />
                                  </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end">
                                  <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                  <DropdownMenuItem
                                    onClick={() => handleViewDetails(skuUID)}
                                    className="cursor-pointer"
                                  >
                                    <Eye className="mr-2 h-4 w-4" />
                                    View Details
                                  </DropdownMenuItem>
                                  <DropdownMenuItem
                                    onClick={() => handleEdit(skuUID)}
                                    className="cursor-pointer"
                                  >
                                    <Edit className="mr-2 h-4 w-4" />
                                    Edit Product
                                  </DropdownMenuItem>
                                  <DropdownMenuItem
                                    onClick={() => handleImageUpload(sku)}
                                    className="cursor-pointer"
                                  >
                                    <Camera className="mr-2 h-4 w-4" />
                                    Update Image
                                  </DropdownMenuItem>
                                  <DropdownMenuSeparator />
                                  <DropdownMenuItem
                                    onClick={() => handleDeleteClick(sku)}
                                    className="cursor-pointer text-red-600"
                                  >
                                    <Trash2 className="mr-2 h-4 w-4" />
                                    Delete
                                  </DropdownMenuItem>
                                </DropdownMenuContent>
                              </DropdownMenu>
                            </div>
                          </div>
                        </CardContent>
                      </Card>
                    );
                  })}
                </div>

                {/* Pagination for Card View */}
                {totalCount > 0 && (
                  <Card className="shadow-sm mt-4">
                    <CardContent className="py-4">
                      <PaginationControls
                        currentPage={currentPage}
                        totalCount={totalCount}
                        pageSize={pageSize}
                        onPageChange={(page) => {
                          setCurrentPage(page);
                          window.scrollTo({ top: 0, behavior: "smooth" });
                        }}
                        onPageSizeChange={(size) => {
                          setPageSize(size);
                          setCurrentPage(1);
                        }}
                        itemName="products"
                      />
                    </CardContent>
                  </Card>
                )}
              </>
            )}
          </div>
        )}

        {/* Delete Confirmation Dialog */}
        <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Are you sure?</AlertDialogTitle>
              <AlertDialogDescription>
                This action cannot be undone. This will permanently delete the
                product
                {selectedSKU && (
                  <>
                    {" "}
                    <strong>
                      `
                      {selectedSKU.SKULongName ||
                        selectedSKU.LongName ||
                        selectedSKU.Name}
                      `
                    </strong>{" "}
                    (Code: {selectedSKU.SKUCode || selectedSKU.Code})
                  </>
                )}{" "}
                and remove all associated data.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel disabled={deleting}>Cancel</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDeleteConfirm}
                disabled={deleting}
                className="bg-red-600 hover:bg-red-700"
              >
                {deleting ? "Deleting..." : "Delete"}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

        {/* Image Upload Dialog */}
        <Dialog open={imageDialogOpen} onOpenChange={setImageDialogOpen}>
          <DialogContent className="sm:max-w-[600px]">
            <DialogHeader>
              <DialogTitle>Product Image</DialogTitle>
              <DialogDescription className="flex items-center gap-2 mt-1">
                <Badge variant="outline" className="font-mono text-xs">
                  {selectedSKUForImage?.SKUCode || selectedSKUForImage?.Code}
                </Badge>
                <span className="text-sm">
                  {selectedSKUForImage?.SKULongName ||
                    selectedSKUForImage?.LongName ||
                    selectedSKUForImage?.Name}
                </span>
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              {/* Show existing image if any */}
              {(() => {
                const existingImages = getSkuImages(
                  selectedSKUForImage?.SKUUID || selectedSKUForImage?.UID
                );
                const currentImage =
                  existingImages.find((img) => img.IsDefault) ||
                  existingImages[0];

                if (currentImage && !uploadPreview) {
                  return (
                    <div className="space-y-3">
                      <div className="flex items-center justify-between">
                        <Label className="text-sm font-medium">
                          Current Image
                        </Label>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() =>
                            document.getElementById("picture")?.click()
                          }
                          className="h-8"
                        >
                          <UploadCloud className="h-3.5 w-3.5 mr-1.5" />
                          Replace Image
                        </Button>
                      </div>
                      <div className="flex justify-center">
                        <div className="relative">
                          {imageUrls[currentImage.UID] ? (
                            <img
                              src={imageUrls[currentImage.UID]}
                              alt="Current product image"
                              className="max-w-full h-48 object-contain rounded-lg border border-gray-200"
                            />
                          ) : (
                            <div className="w-48 h-48 rounded-lg border border-gray-200 bg-gray-50 flex items-center justify-center">
                              <Loader2 className="h-8 w-8 text-gray-400 animate-spin" />
                            </div>
                          )}
                        </div>
                      </div>
                      <div className="relative pt-2 border-t">
                        <p className="text-xs text-gray-500 text-center">
                          Upload a new image to replace the current one
                        </p>
                      </div>
                    </div>
                  );
                }
                return null;
              })()}

              {/* Upload section */}
              {!uploadPreview ? (
                <div
                  className="relative"
                  onDragOver={(e) => {
                    e.preventDefault();
                    e.currentTarget.classList.add("border-primary");
                  }}
                  onDragLeave={(e) => {
                    e.preventDefault();
                    e.currentTarget.classList.remove("border-primary");
                  }}
                  onDrop={(e) => {
                    e.preventDefault();
                    e.currentTarget.classList.remove("border-primary");
                    const file = e.dataTransfer.files[0];
                    if (file && file.type.startsWith("image/")) {
                      const event = {
                        target: { files: [file] }
                      } as unknown as React.ChangeEvent<HTMLInputElement>;
                      handleFileSelect(event);
                    }
                  }}
                >
                  <Label htmlFor="picture" className="cursor-pointer block">
                    <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center hover:border-gray-400 transition-colors">
                      <UploadCloud className="mx-auto h-12 w-12 text-gray-400 mb-3" />
                      <p className="text-sm font-medium text-gray-700 mb-1">
                        Click to upload or drag and drop
                      </p>
                      <p className="text-xs text-gray-500">
                        PNG, JPG, GIF up to 5MB
                      </p>
                      <Input
                        id="picture"
                        type="file"
                        accept="image/*"
                        onChange={handleFileSelect}
                        className="hidden"
                      />
                    </div>
                  </Label>
                </div>
              ) : (
                <div className="relative">
                  <div className="relative rounded-lg border-2 border-gray-200 overflow-hidden bg-gray-50">
                    <img
                      src={uploadPreview}
                      alt="Preview"
                      className="w-full h-64 object-contain"
                    />
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        setUploadFile(null);
                        setUploadPreview("");
                      }}
                      className="absolute top-2 right-2 h-8 w-8 p-0 bg-white/90 hover:bg-white rounded-full"
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                  {uploadFile && (
                    <div className="mt-2 flex items-center justify-between text-xs text-gray-600 px-1">
                      <span className="truncate max-w-[200px]">
                        {uploadFile.name}
                      </span>
                      <span>
                        {(uploadFile.size / 1024 / 1024).toFixed(2)} MB
                      </span>
                    </div>
                  )}
                </div>
              )}
            </div>
            <DialogFooter>
              <Button
                variant="outline"
                onClick={() => {
                  setImageDialogOpen(false);
                  setUploadFile(null);
                  setUploadPreview("");
                }}
                disabled={uploading}
              >
                Cancel
              </Button>
              <Button
                onClick={handleUpload}
                disabled={!uploadFile || uploading}
              >
                {uploading ? (
                  <>
                    <span className="mr-2">Uploading...</span>
                    <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                  </>
                ) : (
                  (() => {
                    const existingImages = getSkuImages(
                      selectedSKUForImage?.SKUUID || selectedSKUForImage?.UID
                    );
                    return existingImages.length > 0
                      ? "Replace Image"
                      : "Upload Image";
                  })()
                )}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
