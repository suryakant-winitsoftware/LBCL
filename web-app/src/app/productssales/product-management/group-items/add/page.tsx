"use client";

import { useState, useEffect, useRef } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useToast } from "@/components/ui/use-toast";
import { Checkbox } from "@/components/ui/checkbox";
import { Separator } from "@/components/ui/separator";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  ArrowLeft,
  Save,
  Package,
  Building2,
  Clock,
  Calculator,
  Info,
  Search,
  AlertCircle,
  Loader2,
  Tags,
  Hash,
  Calendar,
  Truck,
  X,
  ChevronDown,
  RefreshCw,
  Check,
  Eye,
  EyeOff,
} from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import {
  skuClassGroupItemsService,
  SKUClassGroupItem,
  SKUClassGroupItemCreateData,
  SKUClassGroupItemUpdateData,
} from "@/services/sku-class-group-items.service";
import {
  skuClassGroupsService,
  SKUClassGroup,
} from "@/services/sku-class-groups.service";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";

interface SKUOption {
  code: string;
  name: string;
  uid?: string;
  longName?: string;
}

interface SupplierOrg {
  uid: string;
  name: string;
  code?: string;
}

export default function AddSKUClassGroupItemPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();

  const editUID = searchParams.get("uid");
  const groupUID = searchParams.get("groupUID");
  const isEditMode = !!editUID;
  const hasPreselectedGroup = !!groupUID;
  // Disable class group selection when editing or when a group is preselected
  const disableGroupSelection = isEditMode || hasPreselectedGroup;

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [loadingInitialData, setLoadingInitialData] = useState(true);

  // Form data
  const [formData, setFormData] = useState<SKUClassGroupItemCreateData>({
    UID: "",
    SKUClassGroupUID: groupUID || "",
    SKUCode: "",
    SerialNumber: 1,
    ModelQty: 1,
    ModelUoM: "PCS",
    SupplierOrgUID: "",
    LeadTimeInDays: 0,
    DailyCutOffTime: "18:00",
    IsExclusive: false,
    MaxQTY: 9999,
    MinQTY: 1,
    ActionType: 0,
  });

  // Dropdown data
  const [classGroups, setClassGroups] = useState<SKUClassGroup[]>([]);
  const [availableSKUs, setAvailableSKUs] = useState<SKUOption[]>([]);
  const [skuPage, setSkuPage] = useState(1);
  const [hasMoreSKUs, setHasMoreSKUs] = useState(true);
  const [loadingSKUs, setLoadingSKUs] = useState(false);
  const [totalSKUs, setTotalSKUs] = useState(0);
  const [supplierOrgs, setSupplierOrgs] = useState<SupplierOrg[]>([]);
  const [uoms, setUoMs] = useState<string[]>([
    "PCS",
    "KG",
    "LTR",
    "BOX",
    "CASE",
    "PACK",
    "UNIT",
  ]);

  // Search states
  const [skuSearchOpen, setSkuSearchOpen] = useState(false);
  const [skuSearchValue, setSkuSearchValue] = useState("");
  const skuSearchDebounceTimer = useRef<NodeJS.Timeout>();
  const [supplierSearchOpen, setSupplierSearchOpen] = useState(false);
  const [supplierSearchValue, setSupplierSearchValue] = useState("");

  // Selected items for display
  const [selectedSKUs, setSelectedSKUs] = useState<SKUOption[]>([]);
  const [selectedSupplier, setSelectedSupplier] = useState<SupplierOrg | null>(
    null
  );
  const [selectedGroup, setSelectedGroup] = useState<SKUClassGroup | null>(
    null
  );

  // Validation errors and duplicate checking
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [duplicateSKUs, setDuplicateSKUs] = useState<Set<string>>(new Set());
  const [checkingDuplicates, setCheckingDuplicates] = useState(false);
  const [existingGroupItems, setExistingGroupItems] = useState<
    SKUClassGroupItem[]
  >([]);
  const [loadingExistingItems, setLoadingExistingItems] = useState(false);
  const [itemsToRemove, setItemsToRemove] = useState<Set<string>>(new Set());

  // UI state for showing/hiding selected items
  const [hideSelectedItems, setHideSelectedItems] = useState(false);

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    if (isEditMode && editUID) {
      loadItemForEdit(editUID);
    }
  }, [isEditMode, editUID]);

  // Auto-load more items when hiding selected and all visible items are hidden
  useEffect(() => {
    if (hideSelectedItems && !loadingSKUs && hasMoreSKUs && skuSearchOpen) {
      // Count visible items after filtering
      const visibleItems = availableSKUs.filter((sku) => {
        const isSelected = selectedSKUs.some((s) => s.code === sku.code);
        const isDuplicate = duplicateSKUs.has(sku.code);
        const isMarkedForRemoval = itemsToRemove.has(sku.code);

        if (isMarkedForRemoval) return true;
        if (isSelected || isDuplicate) return false;
        return true;
      });

      // If very few or no items are visible and we have more to load, load them
      if (visibleItems.length < 5) {
        setTimeout(() => {
          loadMoreSKUs();
        }, 100); // Small delay to prevent rapid firing
      }
    }
  }, [
    hideSelectedItems,
    availableSKUs,
    selectedSKUs,
    duplicateSKUs,
    itemsToRemove,
    loadingSKUs,
    hasMoreSKUs,
    skuSearchOpen,
  ]);

  const loadSKUs = async (page: number = 1, append: boolean = false) => {
    if (loadingSKUs) return;

    try {
      setLoadingSKUs(true);

      const pageSize = 50; // Load 50 SKUs at a time for better performance
      const requestBody = {
        PageNumber: page,
        PageSize: pageSize,
        IsCountRequired: page === 1, // Only get count on first load
        FilterCriterias: skuSearchValue
          ? [{ Name: "skucodeandname", Value: skuSearchValue }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
      };

      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/SKU/SelectAllSKUDetailsWebView`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(requestBody),
        }
      );

      if (response.ok) {
        const result = await response.json();

        if (result.IsSuccess && result.Data) {
          const data = result.Data;
          let skuData: any[] = [];

          // Handle PagedResponse structure from backend
          if (data.PagedData && Array.isArray(data.PagedData)) {
            skuData = data.PagedData;
          } else if (Array.isArray(data)) {
            skuData = data;
          }

          // Get total count if available
          if (data.TotalCount !== undefined && page === 1) {
            setTotalSKUs(data.TotalCount);
          }

          // Transform SKU data to match expected format
          const skus: SKUOption[] = skuData
            .map((sku: any) => ({
              uid: sku.SKUUID || sku.UID || sku.Id, // ✅ Fixed: Try SKUUID first, then fallback to UID/Id
              code: sku.SKUCode || sku.Code,
              name: sku.SKULongName || sku.Name || sku.LongName,
              longName: sku.LongName || sku.Name || sku.AliasName,
            }))
            .filter((sku: SKUOption) => sku.code && sku.name);

          if (append) {
            setAvailableSKUs((prev) => [...prev, ...skus]);
          } else {
            setAvailableSKUs(skus);
          }

          // Check if there are more SKUs to load
          setHasMoreSKUs(skus.length === pageSize);
        } else {
          console.error("API returned success=false or no data");
          if (!append) setAvailableSKUs([]);
        }
      } else {
        console.error("Failed to load SKUs:", response.status);
        if (!append) {
          toast({
            title: "Warning",
            description: "Failed to load SKUs.",
            variant: "default",
          });
          setAvailableSKUs([]);
        }
      }
    } catch (error) {
      console.error("Error loading SKUs:", error);
      if (!append) {
        toast({
          title: "Warning",
          description: "Failed to load SKUs from server.",
          variant: "default",
        });
        setAvailableSKUs([]);
      }
    } finally {
      setLoadingSKUs(false);
    }
  };

  // Load more SKUs when user scrolls to bottom
  const loadMoreSKUs = () => {
    if (!loadingSKUs && hasMoreSKUs) {
      const nextPage = skuPage + 1;
      setSkuPage(nextPage);
      loadSKUs(nextPage, true);
    }
  };

  // Load existing items in a group to mark them as duplicates
  const loadExistingGroupItems = async (groupUID: string) => {
    try {
      setLoadingExistingItems(true);
      const result = await skuClassGroupItemsService.getAllSKUClassGroupItems(
        1,
        1000, // Load up to 1000 existing items
        "",
        groupUID
      );

      if (result.data && result.data.length > 0) {
        const existingSKUCodes = result.data.map((item) => item.SKUCode);
        setDuplicateSKUs(new Set(existingSKUCodes));
        setExistingGroupItems(result.data);
      } else {
        setDuplicateSKUs(new Set());
        setExistingGroupItems([]);
      }
    } catch (error) {
      console.error(`Error loading existing group items:`, error);
    } finally {
      setLoadingExistingItems(false);
    }
  };

  const loadInitialData = async () => {
    try {
      setLoadingInitialData(true);

      // Load class groups
      const groupsResult = await skuClassGroupsService.getAllSKUClassGroups(
        1,
        100
      );
      setClassGroups(groupsResult.data);

      // Set selected group if groupUID is provided
      if (groupUID) {
        const group = groupsResult.data.find((g) => g.UID === groupUID);
        if (group) {
          setSelectedGroup(group);
          // Also load existing items for this group
          if (!isEditMode) {
            await loadExistingGroupItems(groupUID);
          }
        }
      }

      // Load initial batch of SKUs
      await loadSKUs(1, false);

      // Load supplier organizations
      const suppliers: SupplierOrg[] = [
        { uid: "Supplier", name: "Supplier", code: "SUPP" },
        { uid: "EPIC01", name: "EPIC01", code: "EPIC01" },
      ];
      setSupplierOrgs(suppliers);
    } catch (error) {
      console.error("Failed to load initial data:", error);
      toast({
        title: "Error",
        description: "Failed to load required data",
        variant: "destructive",
      });
    } finally {
      setLoadingInitialData(false);
    }
  };

  const loadItemForEdit = async (uid: string) => {
    try {
      setLoading(true);
      const item = await skuClassGroupItemsService.getSKUClassGroupItemByUID(
        uid
      );

      setFormData({
        ...item,
        CreatedBy: item.CreatedBy || "ADMIN",
        ModifiedBy: "ADMIN",
        ActionType: item.ActionType || 0,
      });

      // Set selected items for display
      // Create a SKU option even if not in availableSKUs list
      const sku = availableSKUs.find((s) => s.code === item.SKUCode) || {
        code: item.SKUCode,
        name: item.SKUCode,
        uid: item.SKUUID || "",
        longName: "",
      };
      setSelectedSKUs([sku]);

      const supplier = supplierOrgs.find((s) => s.uid === item.SupplierOrgUID);
      if (supplier) setSelectedSupplier(supplier);

      const group = classGroups.find((g) => g.UID === item.SKUClassGroupUID);
      if (group) setSelectedGroup(group);
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to load item details",
        variant: "destructive",
      });
      router.push("/productssales/product-management/class-groups");
    } finally {
      setLoading(false);
    }
  };

  // Check for duplicate SKUs in real-time
  const checkForDuplicates = async (skuCodes: string[], groupUID: string) => {
    if (!groupUID || skuCodes.length === 0) {
      setDuplicateSKUs(new Set());
      return;
    }

    setCheckingDuplicates(true);
    const duplicates = new Set<string>();

    try {
      for (const skuCode of skuCodes) {
        const exists = await skuClassGroupItemsService.checkIfItemExists(
          groupUID,
          skuCode
        );
        if (exists) {
          duplicates.add(skuCode);
        }
      }
    } catch (error) {
      console.error("Error checking duplicates:", error);
    }

    setDuplicateSKUs(duplicates);
    setCheckingDuplicates(false);
  };

  // Check for duplicates when selectedSKUs or group changes
  // Commented out because we already load existing items with loadExistingGroupItems
  // This was causing the duplicates to be cleared
  /*
  useEffect(() => {
    if (!isEditMode && selectedSKUs.length > 0 && formData.SKUClassGroupUID) {
      const skuCodes = selectedSKUs.map(sku => sku.code);
      checkForDuplicates(skuCodes, formData.SKUClassGroupUID);
    }
  }, [selectedSKUs, formData.SKUClassGroupUID, isEditMode]);
  */

  // Update selected group when classGroups are loaded and groupUID is provided
  useEffect(() => {
    if (groupUID && classGroups.length > 0 && !selectedGroup) {
      const group = classGroups.find((g) => g.UID === groupUID);
      if (group) {
        setSelectedGroup(group);
        // Ensure formData is also updated
        setFormData((prev) => ({
          ...prev,
          SKUClassGroupUID: groupUID,
        }));

        // Load existing items in this group to show as duplicates
        loadExistingGroupItems(groupUID);
      }
    }
  }, [groupUID, classGroups, selectedGroup]);

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.SKUClassGroupUID) {
      newErrors.SKUClassGroupUID = "Class Group is required";
    }

    // Allow submission if there are items to remove OR items to add
    if (!isEditMode && selectedSKUs.length === 0 && itemsToRemove.size === 0) {
      newErrors.SKUCode = "Select items to add or remove";
    }

    // No longer block submission for duplicates - we'll filter them out during save

    if (isEditMode && (!formData.SKUCode || !formData.SKUCode.trim())) {
      newErrors.SKUCode = "SKU Code is required";
    }

    if (formData.SerialNumber < 0) {
      newErrors.SerialNumber = "Serial number must be positive";
    }

    if (formData.ModelQty <= 0) {
      newErrors.ModelQty = "Model quantity must be greater than 0";
    }

    if (formData.MinQTY <= 0) {
      newErrors.MinQTY = "Minimum quantity must be greater than 0";
    }

    if (formData.MaxQTY < formData.MinQTY) {
      newErrors.MaxQTY =
        "Maximum quantity must be greater than or equal to minimum";
    }

    if (formData.LeadTimeInDays < 0) {
      newErrors.LeadTimeInDays = "Lead time cannot be negative";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      toast({
        title: "Validation Error",
        description: "Please fix the errors before submitting",
        variant: "destructive",
      });
      return;
    }

    try {
      setSaving(true);

      const now = new Date();

      if (isEditMode) {
        // For edit mode, update single item
        const submitData: SKUClassGroupItemUpdateData = {
          ...formData,
          ModifiedBy: "ADMIN",
          ModifiedTime: now,
          ServerModifiedTime: now,
        };

        await skuClassGroupItemsService.updateSKUClassGroupItem(submitData);
        toast({
          title: "Success",
          description: "SKU Class Group Item updated successfully",
        });
      } else {
        // For create mode, handle both additions and removals
        const skusToCreate = selectedSKUs.filter(
          (sku) => !duplicateSKUs.has(sku.code)
        );
        const skusToRemove = Array.from(itemsToRemove);

        // Check if there's anything to do
        if (skusToCreate.length === 0 && skusToRemove.length === 0) {
          toast({
            title: "No Changes",
            description: "No items to add or remove.",
            variant: "default",
          });
          return;
        }

        // Handle removals first
        let removeSuccessCount = 0;
        let removeFailed: string[] = [];

        if (skusToRemove.length > 0) {
          for (const skuCode of skusToRemove) {
            try {
              // Find the existing item to get its UID
              const existingItem = existingGroupItems.find(
                (item) => item.SKUCode === skuCode
              );
              if (existingItem) {
                await skuClassGroupItemsService.deleteSKUClassGroupItem(
                  existingItem.UID
                );
                removeSuccessCount++;
              }
            } catch (error: any) {
              removeFailed.push(skuCode);
              console.error(`Failed to remove ${skuCode}:`, error);
            }
          }
        }

        // Handle additions
        let successCount = 0;
        let failedSKUs: string[] = [];

        if (skusToCreate.length > 0) {
          for (const sku of skusToCreate) {
            try {
              // Double-check to prevent race conditions
              const exists = await skuClassGroupItemsService.checkIfItemExists(
                formData.SKUClassGroupUID,
                sku.code
              );

              if (exists) {
                failedSKUs.push(`${sku.code} (already exists)`);
                continue;
              }

              // Generate unique UID for each SKU
              const uid = `ITEM_${formData.SKUClassGroupUID}_${sku.code}`;

              const submitData: SKUClassGroupItemCreateData = {
                ...formData,
                UID: uid,
                SKUCode: sku.code,
                SKUUID: sku.uid || "", // ✅ Add the SKU UID here (fallback to empty string if null)
                CreatedBy: "ADMIN",
                ModifiedBy: "ADMIN",
                CreatedTime: now,
                ModifiedTime: now,
                ServerAddTime: now,
                ServerModifiedTime: now,
              };

              console.log(`📤 Sending data to API:`, submitData);
              const result =
                await skuClassGroupItemsService.createSKUClassGroupItem(
                  submitData
                );
              console.log(`✅ API Response:`, result);
              successCount++;
            } catch (error: any) {
              if (error.message && error.message.includes("duplicate key")) {
                failedSKUs.push(`${sku.code} (duplicate)`);
              } else {
                failedSKUs.push(`${sku.code} (error)`);
              }
            }
          }
        }

        // Show appropriate message based on results
        const messages = [];

        if (removeSuccessCount > 0) {
          messages.push(
            `Removed ${removeSuccessCount} item${
              removeSuccessCount > 1 ? "s" : ""
            }`
          );
        }
        if (removeFailed.length > 0) {
          messages.push(`Failed to remove: ${removeFailed.join(", ")}`);
        }
        if (successCount > 0) {
          messages.push(
            `Added ${successCount} item${successCount > 1 ? "s" : ""}`
          );
        }
        if (failedSKUs.length > 0) {
          messages.push(`Failed to add: ${failedSKUs.join(", ")}`);
        }

        // Determine overall status
        const hasAnySuccess = removeSuccessCount > 0 || successCount > 0;
        const hasAnyFailure = removeFailed.length > 0 || failedSKUs.length > 0;

        if (hasAnySuccess && !hasAnyFailure) {
          toast({
            title: "Success",
            description: messages.join(". "),
          });
        } else if (hasAnySuccess && hasAnyFailure) {
          toast({
            title: "Partial Success",
            description: messages.join(". "),
            variant: "default",
          });
        } else if (!hasAnySuccess && hasAnyFailure) {
          toast({
            title: "Failed",
            description: messages.join(". "),
            variant: "destructive",
          });
        }
      }

      // Navigate to the items list for this group
      if (formData.SKUClassGroupUID) {
        const params = new URLSearchParams();
        params.set("groupUID", formData.SKUClassGroupUID);
        if (selectedGroup) {
          params.set("groupName", selectedGroup.Name);
        }
        router.push(
          `/productssales/product-management/class-groups/items?${params.toString()}`
        );
      } else {
        router.push("/productssales/product-management/class-groups");
      }
    } catch (error: any) {
      // Check if it's a duplicate key error
      if (error.message && error.message.includes("duplicate key")) {
        toast({
          title: "Duplicate Item",
          description: `This SKU (${formData.SKUCode}) already exists in the selected class group. Please choose a different SKU or edit the existing item.`,
          variant: "destructive",
        });
      } else {
        toast({
          title: "Error",
          description: error.message || "Failed to save SKU Class Group Item",
          variant: "destructive",
        });
      }
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    // Navigate to the items list for this group
    if (formData.SKUClassGroupUID) {
      const params = new URLSearchParams();
      params.set("groupUID", formData.SKUClassGroupUID);
      if (selectedGroup) {
        params.set("groupName", selectedGroup.Name);
      }
      router.push(
        `/productssales/product-management/class-groups/items?${params.toString()}`
      );
    } else {
      router.push("/productssales/product-management/class-groups");
    }
  };

  if (loadingInitialData || loading) {
    return (
      <div className="space-y-6">
        {/* Header Skeleton */}
        <div className="flex items-center justify-between">
          <div className="space-y-2">
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-96" />
          </div>
          <Skeleton className="h-10 w-32" />
        </div>

        {/* Main Form Card Skeleton */}
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-80" />
          </CardHeader>
          <CardContent>
            <div className="space-y-8">
              {/* Basic Information Section */}
              <div className="space-y-6">
                <Skeleton className="h-6 w-32" />
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                  <div className="space-y-2 md:col-span-2">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-32" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                </div>
              </div>

              <Skeleton className="h-px w-full" />

              {/* Quantity Configuration Section */}
              <div className="space-y-6">
                <Skeleton className="h-6 w-40" />
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  {Array.from({ length: 4 }).map((_, i) => (
                    <div key={i} className="space-y-2">
                      <Skeleton className="h-4 w-24" />
                      <Skeleton className="h-10 w-full" />
                    </div>
                  ))}
                </div>
              </div>

              <Skeleton className="h-px w-full" />

              {/* Additional Settings Section */}
              <div className="space-y-4">
                <Skeleton className="h-6 w-36" />
                <div className="flex items-center space-x-2">
                  <Skeleton className="h-4 w-4" />
                  <Skeleton className="h-4 w-24" />
                </div>
              </div>

              {/* Form Actions */}
              <div className="flex justify-end gap-4 pt-6">
                <Skeleton className="h-10 w-20" />
                <Skeleton className="h-10 w-28" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-2">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">
            {isEditMode
              ? "Edit SKU Class Group Item"
              : "Add SKU Class Group Item"}
          </h1>
          <p className="text-muted-foreground text-base">
            {isEditMode
              ? "Update the details of the SKU class group item"
              : "Add a new SKU to a class group with detailed configuration"}
          </p>
        </div>
        <Button
          variant="outline"
          onClick={handleCancel}
          className="flex-shrink-0"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to List
        </Button>
      </div>

      {/* Main Form Card */}
      <Card>
        <CardHeader>
          <CardTitle>Item Information</CardTitle>
          <CardDescription>
            Configure the SKU class group item details. All required fields must
            be completed before saving.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-8">
            {/* Basic Information Section */}
            <div className="space-y-6">
              <div>
                <h3 className="text-lg font-medium mb-4">Basic Information</h3>

                {/* Existing Items Notice */}
                {!isEditMode && existingGroupItems.length > 0 && (
                  <div className="bg-gray-50 rounded-lg p-3 mb-4">
                    <p className="text-sm text-gray-600">
                      <span className="font-medium">
                        {existingGroupItems.length} items
                      </span>{" "}
                      in this group
                    </p>
                  </div>
                )}

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* Class Group Selection */}
                  <div className="space-y-2">
                    <Label htmlFor="classGroup">
                      Class Group <span className="text-red-500">*</span>
                    </Label>
                    <Select
                      value={formData.SKUClassGroupUID || undefined}
                      onValueChange={(value) => {
                        // Only allow changes if group selection is not disabled
                        if (!disableGroupSelection) {
                          setFormData((prev) => ({
                            ...prev,
                            SKUClassGroupUID: value,
                          }));
                          const group = classGroups.find(
                            (g) => g.UID === value
                          );
                          setSelectedGroup(group || null);
                          setErrors((prev) => ({
                            ...prev,
                            SKUClassGroupUID: "",
                          }));
                          // Load existing items for the newly selected group
                          if (value && !isEditMode) {
                            loadExistingGroupItems(value);
                          }
                        }
                      }}
                      disabled={disableGroupSelection}
                    >
                      <SelectTrigger
                        id="classGroup"
                        className={
                          errors.SKUClassGroupUID ? "border-red-500" : ""
                        }
                        disabled={disableGroupSelection}
                      >
                        <SelectValue placeholder="Select a class group" />
                      </SelectTrigger>
                      <SelectContent>
                        {classGroups.length === 0 && (
                          <div className="p-2 text-sm text-muted-foreground">
                            Loading groups...
                          </div>
                        )}
                        {classGroups.map((group) => (
                          <SelectItem key={group.UID} value={group.UID}>
                            {group.Name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    {errors.SKUClassGroupUID && (
                      <p className="text-sm text-red-500">
                        {errors.SKUClassGroupUID}
                      </p>
                    )}
                    {disableGroupSelection && (
                      <p className="text-sm text-muted-foreground">
                        {isEditMode
                          ? "Class group cannot be changed when editing"
                          : "Class group is locked for this operation"}
                      </p>
                    )}
                  </div>

                  {/* SKU Selection */}
                  <div className="space-y-2">
                    <Label>
                      SKU Selection <span className="text-red-500">*</span>
                    </Label>
                    <Popover
                      open={skuSearchOpen}
                      onOpenChange={(open) => {
                        // Don't allow opening in edit mode
                        if (!isEditMode) {
                          setSkuSearchOpen(open);
                        }
                      }}
                    >
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          role="combobox"
                          aria-expanded={skuSearchOpen}
                          disabled={isEditMode}
                          className={cn(
                            "w-full justify-between",
                            errors.SKUCode && "border-red-500"
                          )}
                        >
                          <span
                            className={
                              selectedSKUs.length > 0
                                ? ""
                                : "text-muted-foreground"
                            }
                          >
                            {isEditMode
                              ? selectedSKUs[0]
                                ? selectedSKUs[0].code
                                : "Select SKU..."
                              : selectedSKUs.length > 0
                              ? `${selectedSKUs.length} SKU${
                                  selectedSKUs.length !== 1 ? "s" : ""
                                } selected`
                              : "Select SKUs..."}
                          </span>
                          <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-[400px] p-0" align="start">
                        <div className="p-2">
                          <Input
                            placeholder="Search SKUs..."
                            value={skuSearchValue}
                            onChange={(e) => {
                              setSkuSearchValue(e.target.value);
                              // Debounce search and reload SKUs
                              if (skuSearchDebounceTimer.current) {
                                clearTimeout(skuSearchDebounceTimer.current);
                              }
                              skuSearchDebounceTimer.current = setTimeout(
                                () => {
                                  setSkuPage(1);
                                  setHasMoreSKUs(true);
                                  loadSKUs(1, false);
                                },
                                300
                              );
                            }}
                            className="mb-2"
                          />
                          {!isEditMode && (
                            <div className="flex items-center justify-between mb-2 px-2">
                              <Label className="text-sm font-medium">
                                {selectedSKUs.length} selected |{" "}
                                {availableSKUs.length} loaded
                                {totalSKUs > 0 && ` of ${totalSKUs} total`}
                              </Label>
                              <div className="flex items-center gap-2">
                                <Button
                                  variant="outline"
                                  size="sm"
                                  onClick={() =>
                                    setHideSelectedItems(!hideSelectedItems)
                                  }
                                  className="h-7"
                                >
                                  {hideSelectedItems ? (
                                    <>
                                      <Eye className="h-3 w-3 mr-1" />
                                      Show All
                                    </>
                                  ) : (
                                    <>
                                      <EyeOff className="h-3 w-3 mr-1" />
                                      Hide Selected
                                    </>
                                  )}
                                </Button>
                                {selectedSKUs.length > 0 && (
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => {
                                      setSelectedSKUs([]);
                                      setFormData((prev) => ({
                                        ...prev,
                                        SKUCode: "",
                                      }));
                                    }}
                                  >
                                    Clear all
                                  </Button>
                                )}
                              </div>
                            </div>
                          )}

                          {/* Duplicate checking status */}
                          {!isEditMode && formData.SKUClassGroupUID && (
                            <div className="px-2 py-2">
                              {checkingDuplicates && (
                                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                                  <Loader2 className="h-3 w-3 animate-spin" />
                                  Checking for duplicates...
                                </div>
                              )}
                              {!checkingDuplicates &&
                                duplicateSKUs.size > 0 && (
                                  <div className="flex items-center gap-2 text-sm text-green-600">
                                    <Check className="h-3 w-3" />
                                    {duplicateSKUs.size} SKU
                                    {duplicateSKUs.size > 1 ? "s" : ""} already
                                    in this group
                                  </div>
                                )}
                            </div>
                          )}
                        </div>
                        <Separator />
                        <div
                          className="max-h-[300px] overflow-y-auto p-2"
                          onScroll={(e) => {
                            const target = e.target as HTMLDivElement;
                            const bottom =
                              target.scrollHeight - target.scrollTop ===
                              target.clientHeight;
                            if (bottom && !loadingSKUs && hasMoreSKUs) {
                              loadMoreSKUs();
                            }
                          }}
                        >
                          {/* Show info when hiding selected items */}
                          {hideSelectedItems &&
                            (selectedSKUs.length > 0 ||
                              duplicateSKUs.size > 0) && (
                              <div className="mb-2 p-2 bg-blue-50 border border-blue-200 rounded text-xs text-blue-700">
                                Hiding{" "}
                                {selectedSKUs.length + duplicateSKUs.size} items
                                ({selectedSKUs.length} selected,{" "}
                                {duplicateSKUs.size} existing)
                              </div>
                            )}

                          {availableSKUs
                            .filter((sku) => {
                              // First apply search filter
                              const matchesSearch =
                                !skuSearchValue ||
                                sku.code
                                  .toLowerCase()
                                  .includes(skuSearchValue.toLowerCase()) ||
                                sku.name
                                  .toLowerCase()
                                  .includes(skuSearchValue.toLowerCase()) ||
                                (sku.longName &&
                                  sku.longName
                                    .toLowerCase()
                                    .includes(skuSearchValue.toLowerCase()));

                              if (!matchesSearch) return false;

                              // Apply hide selected filter if enabled
                              if (hideSelectedItems) {
                                const isSelected = selectedSKUs.some(
                                  (s) => s.code === sku.code
                                );
                                const isDuplicate = duplicateSKUs.has(sku.code);
                                const isMarkedForRemoval = itemsToRemove.has(
                                  sku.code
                                );
                                // Hide items that are selected or already exist (but show items marked for removal)
                                if (isMarkedForRemoval) {
                                  return true; // Show items marked for removal
                                }
                                if (isSelected || isDuplicate) {
                                  return false; // Hide selected and existing items
                                }
                              }

                              return true;
                            })
                            .map((sku) => {
                              const isSelected = selectedSKUs.some(
                                (s) => s.code === sku.code
                              );
                              const isDuplicate = duplicateSKUs.has(sku.code);
                              const isMarkedForRemoval = itemsToRemove.has(
                                sku.code
                              );
                              return (
                                <div
                                  key={sku.code}
                                  className={cn(
                                    "flex items-center space-x-2 p-2 rounded cursor-pointer select-none",
                                    isEditMode &&
                                      selectedSKUs[0]?.code === sku.code &&
                                      "bg-accent",
                                    !isEditMode &&
                                      isSelected &&
                                      !isDuplicate &&
                                      "bg-accent/50 hover:bg-accent/70",
                                    !isEditMode &&
                                      isDuplicate &&
                                      !isMarkedForRemoval &&
                                      "bg-green-50 border border-green-200",
                                    !isEditMode &&
                                      isDuplicate &&
                                      isMarkedForRemoval &&
                                      "bg-red-50 border border-red-200"
                                  )}
                                  onClick={() => {
                                    if (isEditMode) {
                                      setSelectedSKUs([sku]);
                                      setFormData((prev) => ({
                                        ...prev,
                                        SKUCode: sku.code,
                                      }));
                                      setSkuSearchOpen(false);
                                      setErrors((prev) => ({
                                        ...prev,
                                        SKUCode: "",
                                      }));
                                    } else {
                                      // Handle both new items and existing items
                                      if (isDuplicate) {
                                        // For existing items: toggle removal status
                                        // If currently NOT marked for removal (checked), mark for removal (uncheck)
                                        // If currently marked for removal (unchecked), unmark for removal (check)
                                        const newItemsToRemove = new Set(
                                          itemsToRemove
                                        );
                                        if (itemsToRemove.has(sku.code)) {
                                          // Currently marked for removal, so unmark it (re-check it)
                                          newItemsToRemove.delete(sku.code);
                                        } else {
                                          // Currently not marked for removal, so mark it (uncheck it)
                                          newItemsToRemove.add(sku.code);
                                        }
                                        setItemsToRemove(newItemsToRemove);
                                      } else {
                                        // Toggle selection for new items
                                        if (isSelected) {
                                          const newSKUs = selectedSKUs.filter(
                                            (s) => s.code !== sku.code
                                          );
                                          setSelectedSKUs(newSKUs);
                                          setFormData((prev) => ({
                                            ...prev,
                                            SKUCode: newSKUs
                                              .map((s) => s.code)
                                              .join(","),
                                          }));
                                        } else {
                                          const newSelection = [
                                            ...selectedSKUs,
                                            sku,
                                          ];
                                          setSelectedSKUs(newSelection);
                                          setFormData((prev) => ({
                                            ...prev,
                                            SKUCode: newSelection
                                              .map((s) => s.code)
                                              .join(","),
                                          }));
                                          setErrors((prev) => ({
                                            ...prev,
                                            SKUCode: "",
                                          }));
                                        }
                                      }
                                    }
                                  }}
                                >
                                  {!isEditMode && (
                                    <Checkbox
                                      checked={
                                        isDuplicate
                                          ? !itemsToRemove.has(sku.code)
                                          : isSelected
                                      }
                                      onCheckedChange={() => {}}
                                      className=""
                                    />
                                  )}
                                  <div className="flex-1">
                                    <div className="flex items-center gap-2">
                                      <div className="font-medium">
                                        {sku.code}
                                      </div>
                                      {!isEditMode &&
                                        isDuplicate &&
                                        !isMarkedForRemoval && (
                                          <Badge
                                            variant="outline"
                                            className="text-xs bg-green-100 text-green-800 border-green-300"
                                          >
                                            ✓ In group
                                          </Badge>
                                        )}
                                      {!isEditMode &&
                                        isDuplicate &&
                                        isMarkedForRemoval && (
                                          <Badge
                                            variant="destructive"
                                            className="text-xs"
                                          >
                                            - Will remove
                                          </Badge>
                                        )}
                                    </div>
                                    <div className="text-xs text-muted-foreground">
                                      {sku.longName || sku.name}
                                    </div>
                                    {!isEditMode &&
                                      isDuplicate &&
                                      !isMarkedForRemoval && (
                                        <div className="text-xs text-green-700 mt-1">
                                          This SKU is already in the group
                                        </div>
                                      )}
                                    {!isEditMode &&
                                      isDuplicate &&
                                      isMarkedForRemoval && (
                                        <div className="text-xs text-red-600 mt-1">
                                          This SKU will be removed from the
                                          group
                                        </div>
                                      )}
                                  </div>
                                </div>
                              );
                            })}
                          {availableSKUs.filter((sku) => {
                            // Apply same filtering logic as above
                            const matchesSearch =
                              !skuSearchValue ||
                              sku.code
                                .toLowerCase()
                                .includes(skuSearchValue.toLowerCase()) ||
                              sku.name
                                .toLowerCase()
                                .includes(skuSearchValue.toLowerCase()) ||
                              (sku.longName &&
                                sku.longName
                                  .toLowerCase()
                                  .includes(skuSearchValue.toLowerCase()));

                            if (!matchesSearch) return false;

                            if (hideSelectedItems) {
                              const isSelected = selectedSKUs.some(
                                (s) => s.code === sku.code
                              );
                              const isDuplicate = duplicateSKUs.has(sku.code);
                              const isMarkedForRemoval = itemsToRemove.has(
                                sku.code
                              );
                              if (isMarkedForRemoval) {
                                return true;
                              }
                              if (isSelected || isDuplicate) {
                                return false;
                              }
                            }

                            return true;
                          }).length === 0 && (
                            <div className="text-center py-4">
                              {hideSelectedItems ? (
                                loadingSKUs ? (
                                  <p className="text-sm text-muted-foreground">
                                    Loading more unselected items...
                                  </p>
                                ) : hasMoreSKUs ? (
                                  <p className="text-sm text-muted-foreground">
                                    Loading more items automatically...
                                  </p>
                                ) : (
                                  <p className="text-sm text-muted-foreground">
                                    No more unselected items available
                                  </p>
                                )
                              ) : (
                                <p className="text-sm text-muted-foreground">
                                  {skuSearchValue
                                    ? "No SKUs found matching your search"
                                    : loadingSKUs
                                    ? "Loading items..."
                                    : "No SKUs available"}
                                </p>
                              )}
                            </div>
                          )}
                          {loadingSKUs && (
                            <div className="flex items-center justify-center py-2">
                              <RefreshCw className="h-4 w-4 animate-spin mr-2" />
                              <span className="text-sm text-muted-foreground">
                                Loading more SKUs...
                              </span>
                            </div>
                          )}
                          {!loadingSKUs &&
                            hasMoreSKUs &&
                            availableSKUs.length > 0 && (
                              <div className="text-center py-2">
                                <span className="text-xs text-muted-foreground">
                                  {hideSelectedItems
                                    ? "Auto-loading more..."
                                    : "Scroll for more"}
                                </span>
                              </div>
                            )}
                        </div>
                        {!isEditMode && (
                          <>
                            <Separator />
                            <div className="p-2">
                              <Input
                                placeholder="Add custom SKU code + Enter"
                                onKeyDown={(e) => {
                                  if (e.key === "Enter") {
                                    e.preventDefault();
                                    const input = e.currentTarget;
                                    const code = input.value.trim();
                                    if (code) {
                                      const newSKU: SKUOption = {
                                        code,
                                        name: code,
                                      };
                                      if (
                                        !availableSKUs.some(
                                          (s) => s.code === code
                                        )
                                      ) {
                                        setAvailableSKUs((prev) => [
                                          ...prev,
                                          newSKU,
                                        ]);
                                      }
                                      if (
                                        !selectedSKUs.some(
                                          (s) => s.code === code
                                        )
                                      ) {
                                        const newSelection = [
                                          ...selectedSKUs,
                                          newSKU,
                                        ];
                                        setSelectedSKUs(newSelection);
                                        setFormData((prev) => ({
                                          ...prev,
                                          SKUCode: newSelection
                                            .map((s) => s.code)
                                            .join(","),
                                        }));
                                      }
                                      input.value = "";
                                      setErrors((prev) => ({
                                        ...prev,
                                        SKUCode: "",
                                      }));
                                    }
                                  }
                                }}
                              />
                            </div>
                          </>
                        )}
                      </PopoverContent>
                    </Popover>

                    {selectedSKUs[0] && isEditMode && (
                      <>
                        <p className="text-sm text-muted-foreground">
                          {selectedSKUs[0].longName || selectedSKUs[0].name}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          SKU cannot be changed when editing
                        </p>
                      </>
                    )}

                    {errors.SKUCode && (
                      <p className="text-sm text-red-500">{errors.SKUCode}</p>
                    )}
                  </div>
                </div>

                {/* Items Marked for Removal */}
                {!isEditMode && itemsToRemove.size > 0 && (
                  <div className="mt-6">
                    <Alert className="border-red-200 bg-red-50">
                      <AlertCircle className="h-4 w-4 text-red-600" />
                      <AlertDescription>
                        <div className="font-medium text-red-900 mb-2">
                          {itemsToRemove.size} item
                          {itemsToRemove.size > 1 ? "s" : ""} will be removed
                          from the group:
                        </div>
                        <div className="flex flex-wrap gap-1">
                          {Array.from(itemsToRemove).map((code) => (
                            <Badge
                              key={code}
                              variant="destructive"
                              className="text-xs"
                            >
                              {code}
                            </Badge>
                          ))}
                        </div>
                      </AlertDescription>
                    </Alert>
                  </div>
                )}

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
                  {/* Serial Number */}
                  <div className="space-y-2">
                    <Label htmlFor="serialNumber">Serial Number</Label>
                    <Input
                      id="serialNumber"
                      type="number"
                      value={formData.SerialNumber}
                      onChange={(e) => {
                        setFormData((prev) => ({
                          ...prev,
                          SerialNumber: parseInt(e.target.value) || 0,
                        }));
                        setErrors((prev) => ({ ...prev, SerialNumber: "" }));
                      }}
                      className={errors.SerialNumber ? "border-red-500" : ""}
                    />
                    {errors.SerialNumber && (
                      <p className="text-sm text-red-500">
                        {errors.SerialNumber}
                      </p>
                    )}
                  </div>

                  {/* Supplier Organization */}
                  <div className="space-y-2">
                    <Label htmlFor="supplier">Supplier Organization</Label>
                    <div className="flex gap-2">
                      <Select
                        value={formData.SupplierOrgUID || "none"}
                        onValueChange={(value) => {
                          const supplierUID = value === "none" ? "" : value;
                          setFormData((prev) => ({
                            ...prev,
                            SupplierOrgUID: supplierUID,
                          }));
                          const supplier = supplierOrgs.find(
                            (s) => s.uid === supplierUID
                          );
                          setSelectedSupplier(supplier || null);
                        }}
                      >
                        <SelectTrigger id="supplier" className="flex-1">
                          <SelectValue placeholder="Select supplier (optional)" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="none">None</SelectItem>
                          {supplierOrgs.map((supplier) => (
                            <SelectItem key={supplier.uid} value={supplier.uid}>
                              {supplier.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      {formData.SupplierOrgUID && (
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            setFormData((prev) => ({
                              ...prev,
                              SupplierOrgUID: "",
                            }));
                            setSelectedSupplier(null);
                          }}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              <Separator />

              {/* Quantity Configuration Section */}
              <div>
                <h3 className="text-lg font-medium mb-4">
                  Quantity Configuration
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div className="space-y-2">
                    <Label htmlFor="modelQty">
                      Model Quantity <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id="modelQty"
                      type="number"
                      step="0.01"
                      value={formData.ModelQty}
                      onChange={(e) => {
                        setFormData((prev) => ({
                          ...prev,
                          ModelQty: parseFloat(e.target.value) || 0,
                        }));
                        setErrors((prev) => ({ ...prev, ModelQty: "" }));
                      }}
                      className={errors.ModelQty ? "border-red-500" : ""}
                    />
                    {errors.ModelQty && (
                      <p className="text-sm text-red-500">{errors.ModelQty}</p>
                    )}
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="modelUoM">Model UoM</Label>
                    <Select
                      value={formData.ModelUoM}
                      onValueChange={(value) =>
                        setFormData((prev) => ({ ...prev, ModelUoM: value }))
                      }
                    >
                      <SelectTrigger id="modelUoM">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {uoms.map((uom) => (
                          <SelectItem key={uom} value={uom}>
                            {uom}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="minQty">
                      Minimum Quantity <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id="minQty"
                      type="number"
                      step="0.01"
                      value={formData.MinQTY}
                      onChange={(e) => {
                        setFormData((prev) => ({
                          ...prev,
                          MinQTY: parseFloat(e.target.value) || 0,
                        }));
                        setErrors((prev) => ({ ...prev, MinQTY: "" }));
                      }}
                      className={errors.MinQTY ? "border-red-500" : ""}
                    />
                    {errors.MinQTY && (
                      <p className="text-sm text-red-500">{errors.MinQTY}</p>
                    )}
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="maxQty">
                      Maximum Quantity <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id="maxQty"
                      type="number"
                      step="0.01"
                      value={formData.MaxQTY}
                      onChange={(e) => {
                        setFormData((prev) => ({
                          ...prev,
                          MaxQTY: parseFloat(e.target.value) || 0,
                        }));
                        setErrors((prev) => ({ ...prev, MaxQTY: "" }));
                      }}
                      className={errors.MaxQTY ? "border-red-500" : ""}
                    />
                    {errors.MaxQTY && (
                      <p className="text-sm text-red-500">{errors.MaxQTY}</p>
                    )}
                  </div>
                </div>

                {formData.MaxQTY < formData.MinQTY && (
                  <Alert variant="destructive" className="mt-4">
                    <AlertCircle className="h-4 w-4" />
                    <AlertDescription>
                      Maximum quantity must be greater than or equal to minimum
                      quantity
                    </AlertDescription>
                  </Alert>
                )}
              </div>

              <Separator />

              {/* Additional Settings Section */}
              <div>
                <h3 className="text-lg font-medium mb-4">
                  Additional Settings
                </h3>
                <div className="space-y-4">
                  <div className="flex items-center space-x-2">
                    <Checkbox
                      id="isExclusive"
                      checked={formData.IsExclusive}
                      onCheckedChange={(checked) =>
                        setFormData((prev) => ({
                          ...prev,
                          IsExclusive: !!checked,
                        }))
                      }
                    />
                    <Label
                      htmlFor="isExclusive"
                      className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                    >
                      Exclusive Item
                    </Label>
                  </div>

                  {formData.IsExclusive && (
                    <Alert>
                      <Info className="h-4 w-4" />
                      <AlertDescription>
                        This SKU will be marked as exclusive to this class group
                        with special handling.
                      </AlertDescription>
                    </Alert>
                  )}
                </div>
              </div>
            </div>

            {/* Form Actions */}
            <div className="flex justify-end gap-4 pt-6">
              <Button
                type="button"
                variant="outline"
                onClick={handleCancel}
                disabled={saving}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={saving}>
                {saving ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 h-4 w-4" />
                    {isEditMode ? "Update Item" : "Create Item"}
                  </>
                )}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
