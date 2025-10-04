"use client";

import React, {
  useState,
  useEffect,
  useMemo,
  useRef,
  useTransition,
} from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { MultiSelect, MultiSelectOption } from "@/components/ui/multi-select";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  RefreshCw,
  Package,
  ChevronDown,
  ChevronUp,
  Search,
  Layers,
  CheckCircle2,
  ShoppingCart,
  Plus,
  Minus,
  AlertCircle,
  X,
  Info,
} from "lucide-react";
import {
  hierarchyService,
  SKUGroupType,
  HierarchyOption,
} from "@/services/hierarchy.service";
import { skuService } from "@/services/sku/sku.service";
import { directProductQueryService } from "../../services/direct-product-query.service";
import { Checkbox } from "@/components/ui/checkbox";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { cn } from "@/lib/utils";
import { ProductsTable } from "./ProductsTable";

export interface ProductAttribute {
  type: string;
  code: string[];
  value: string[];
  level: number;
}

export interface FinalProduct {
  UID: string;
  Code: string;
  Name: string;
  GroupName?: string;
  GroupTypeName?: string;
  MRP?: number;
  IsActive?: boolean;
  quantity?: number; // Added quantity field
}

// Product cache to prevent redundant API calls
const productCache = new Map<
  string,
  { data: FinalProduct[]; timestamp: number }
>();
const CACHE_DURATION = 30 * 60 * 1000; // 30 minutes cache
const DEFAULT_PAGE_SIZE = 50; // Configurable page size for all product queries

// Debounce hook for search
function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

export interface DynamicProductAttributesProps {
  value: ProductAttribute[];
  onChange: (attributes: ProductAttribute[]) => void;
  onFinalProductsChange?: (products: FinalProduct[]) => void;
  onSelectionModeChange?: (mode: "all" | "hierarchy" | "specific") => void;
  orgUid?: string;
  disabled?: boolean;
}

export default function DynamicProductAttributes({
  value,
  onChange,
  onFinalProductsChange,
  onSelectionModeChange,
  orgUid,
  disabled = false,
}: DynamicProductAttributesProps) {
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([]);
  const [hierarchyOptions, setHierarchyOptions] = useState<
    Record<string, HierarchyOption[]>
  >({});
  const [selectedAttributes, setSelectedAttributes] = useState<
    ProductAttribute[]
  >(value || []);
  const [finalProducts, setFinalProducts] = useState<FinalProduct[]>([]);
  const [loadingHierarchy, setLoadingHierarchy] = useState(true);
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedProducts, setSelectedProducts] = useState<Set<string>>(
    new Set()
  );
  const [productsByGroup, setProductsByGroup] = useState<
    Record<string, FinalProduct[]>
  >({});
  const [selectionMode, setSelectionMode] = useState<
    "all" | "hierarchy" | "specific"
  >("hierarchy");
  const [productQuantities, setProductQuantities] = useState<
    Record<string, number>
  >({});
  const [hierarchyTrigger, setHierarchyTrigger] = useState<number>(0);
  const [loadingChildOptions, setLoadingChildOptions] = useState<
    Record<number, boolean>
  >({});
  const productLoadTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const [hasSearched, setHasSearched] = useState(false); // Track if user has searched

  const [, startTransition] = useTransition();
  const loadingRef = useRef(false);

  // Lazy loading state for specific mode
  const [currentPage, setCurrentPage] = useState(1);
  const [totalProductCount, setTotalProductCount] = useState(0);
  const [hasMoreProducts, setHasMoreProducts] = useState(true);
  const [loadingMoreProducts, setLoadingMoreProducts] = useState(false);

  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  // Function to load more products for lazy loading
  const loadMoreProducts = async () => {
    if (loadingMoreProducts || !hasMoreProducts) return;

    setLoadingMoreProducts(true);
    try {
      const nextPage = currentPage + 1;
      const request = {
        PageNumber: nextPage,
        PageSize: DEFAULT_PAGE_SIZE,
        IsCountRequired: false,
        FilterCriterias: [], // No filters for specific mode
        SortCriterias: [], // Use empty array like working component
      };

      const response = await skuService.getAllSKUs(request);
      console.log("[Load More] SKU Service Response:", response);

      // Handle response same as working ProductAttributesWithHierarchyFilter component
      let responseData;
      let isSuccess = false;

      if (response?.success) {
        isSuccess = true;
        responseData = response.data;
      } else if (response?.IsSuccess) {
        isSuccess = true;
        responseData = response.Data;
      }

      if (!isSuccess || !responseData?.PagedData) {
        throw new Error("Failed to load more products");
      }

      // Map the API response to FinalProduct format
      const moreProducts: FinalProduct[] = responseData.PagedData.map(
        (sku: any) => ({
          UID: sku.SKUUID || sku.UID,
          Code: sku.SKUCode || sku.Code,
          Name: sku.SKULongName || sku.LongName || sku.Name,
          GroupName: sku.L2 || sku.L1 || "Uncategorized",
          GroupTypeName: sku.ParentUID,
          MRP: sku.MRP || 0,
          IsActive: sku.IsActive !== false,
        })
      );

      // Append new products
      const newProducts = [...finalProducts, ...moreProducts];

      // Update grouped products
      const grouped = newProducts.reduce((acc, product) => {
        const groupName = product.GroupName || "Uncategorized";
        if (!acc[groupName]) {
          acc[groupName] = [];
        }
        acc[groupName].push(product);
        return acc;
      }, {} as Record<string, FinalProduct[]>);

      setProductsByGroup(grouped);
      setFinalProducts(newProducts);
      setCurrentPage(nextPage);
      setHasMoreProducts(moreProducts.length === DEFAULT_PAGE_SIZE); // If we got full page, there might be more
    } catch (error) {
      console.error("Error loading more products:", error);
    } finally {
      setLoadingMoreProducts(false);
    }
  };

  // Function to fetch total product count for "All Products Mode"
  const fetchTotalProductCount = async () => {
    try {
      console.log("🔍 Fetching total product count for All Products Mode...");

      const request = {
        PageNumber: 1,
        PageSize: 1,
        FilterCriterias: [],
        IsCountRequired: true,
        SortCriterias: [],
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
          body: JSON.stringify(request),
        }
      );

      if (response.ok) {
        const result = await response.json();

        if (result.IsSuccess && result.Data && result.Data.TotalCount) {
          const totalCount = result.Data.TotalCount;
          setTotalProductCount(totalCount);
          console.log(
            `✅ Total product count fetched: ${totalCount.toLocaleString()}`
          );
        } else {
          console.warn("⚠️ No total count received from API");
          setTotalProductCount(0);
        }
      } else {
        console.error("❌ Failed to fetch total count:", response.status);
        setTotalProductCount(0);
      }
    } catch (error) {
      console.error("❌ Error fetching total product count:", error);
      setTotalProductCount(0);
    }
  };

  useEffect(() => {
    if (
      (selectionMode === "specific" || selectionMode === "hierarchy") &&
      finalProducts.length > 0
    ) {
      const selectedProductsList = finalProducts
        .filter((p) => selectedProducts.has(p.UID))
        .map((p) => ({
          ...p,
          quantity: productQuantities[p.UID] || 1, // Include the quantity for each product
        }));
      onFinalProductsChange?.(selectedProductsList);
    }
  }, [selectedProducts, selectionMode, finalProducts, productQuantities]); // Added productQuantities to dependencies

  // Fetch total product count when "All Products Mode" is selected
  useEffect(() => {
    if (selectionMode === "all") {
      fetchTotalProductCount();
    }
  }, [selectionMode]);

  // Preload data on component mount for better performance
  useEffect(() => {
    directProductQueryService.preloadAllData().catch(() => {
      // Silent fail - this is just an optimization
    });
  }, []);

  // Load product hierarchy data
  useEffect(() => {
    const loadHierarchyData = async () => {
      try {
        setLoadingHierarchy(true);
        setError(null);

        // PERFORMANCE: Preload all product data for instant switching
        directProductQueryService.preloadAllData().catch(() => {
          // Silent fail for preload
        });

        // Check cache first for hierarchy data
        const hierarchyCacheKey = "hierarchy_data";
        const cachedHierarchy = productCache.get(hierarchyCacheKey);

        if (
          cachedHierarchy &&
          Date.now() - cachedHierarchy.timestamp < CACHE_DURATION
        ) {
          const cached = cachedHierarchy.data as any;
          setHierarchyTypes(cached.types);
          setHierarchyOptions(cached.options);

          // Attributes will be initialized by the dedicated useEffect

          setLoadingHierarchy(false);
          return;
        }

        const types = await hierarchyService.getHierarchyTypes();

        // Only load first level options initially
        const firstLevelOptions: Record<string, HierarchyOption[]> = {};
        if (types.length > 0) {
          const firstType = types[0];
          const options = await hierarchyService.getHierarchyOptionsForType(
            firstType.UID
          );
          firstLevelOptions[firstType.Name] = options;
        }

        // Cache hierarchy data
        productCache.set(hierarchyCacheKey, {
          data: { types, options: firstLevelOptions } as any,
          timestamp: Date.now(),
        });

        setHierarchyTypes(types);
        setHierarchyOptions(firstLevelOptions);

        // Attributes will be initialized by the dedicated useEffect
      } catch (error) {
        setError(
          error instanceof Error
            ? error.message
            : "Failed to load product hierarchy"
        );
      } finally {
        startTransition(() => {
          setLoadingHierarchy(false);
        });
      }
    };

    loadHierarchyData();
  }, [orgUid]);

  // Initialize hierarchy structure once
  useEffect(() => {
    if (hierarchyTypes.length > 0 && selectedAttributes.length === 0) {
      // Initialize the full hierarchy structure
      const sortedTypes = hierarchyTypes.sort(
        (a, b) => (a.ItemLevel || 0) - (b.ItemLevel || 0)
      );

      // CHANGED: Show ALL hierarchy types, even if they don't have data yet
      // This allows users to see the structure and understand what needs to be configured
      if (sortedTypes.length > 0) {
        const initialAttributes = sortedTypes.map((type, index) => ({
          type: type.Name,
          code: [] as string[], // Initialize as empty array for multi-select
          value: [] as string[], // Initialize as empty array for multi-select
          level: index + 1,
        }));
        setSelectedAttributes(initialAttributes);
      }
    }
  }, [hierarchyTypes, hierarchyOptions]);

  // Handle incoming value prop separately
  useEffect(() => {
    // Only sync from parent value prop on initial mount
    if (value && value.length > 0 && selectedAttributes.length === 0) {
      setSelectedAttributes(value);
    }
  }, []); // Remove value dependency to prevent overriding local state

  // Memoized filtered products with debounced search
  const filteredProducts = useMemo(() => {
    if (!debouncedSearchQuery) return finalProducts;

    const searchLower = debouncedSearchQuery.toLowerCase();
    return finalProducts.filter(
      (product) =>
        product.Name?.toLowerCase().includes(searchLower) ||
        product.Code?.toLowerCase().includes(searchLower) ||
        product.GroupName?.toLowerCase().includes(searchLower)
    );
  }, [finalProducts, debouncedSearchQuery]);

  // Pre-fetch products ONLY for 'specific' mode
  useEffect(() => {
    // No pre-fetching needed for "all" mode since we don't load products
    // Specific mode uses lazy loading, so no pre-fetch needed either
  }, [selectionMode]);

  // Clear products when selection mode changes - but only clear, don't show no products immediately
  useEffect(() => {
    setSelectedProducts(new Set());
    setProductQuantities({});

    // Notify parent of selection mode change
    onSelectionModeChange?.(selectionMode);

    // CRITICAL: Reset loading state to allow new mode to load
    setLoadingProducts(false);
    loadingRef.current = false;

    // IMPORTANT: When switching TO specific mode, we need to clear hierarchy selections
    // so that getAllProducts() is called instead of hierarchy-filtered products
    if (selectionMode === "specific") {
      // Clear hierarchy selections to ensure we get all products, not filtered ones
      const clearedAttributes = selectedAttributes.map((attr) => ({
        ...attr,
        code: [],
        value: [],
      }));
      setSelectedAttributes(clearedAttributes);
      onChange(clearedAttributes);

      // FORCE: Clear any cached hierarchy-filtered products to ensure fresh ALL products load
      productCache.clear();

      // Force a reload trigger to ensure fresh data
      setHierarchyTrigger((prev) => prev + 1);
    }

    // Don't clear finalProducts here - let the loading effect handle it
    // Only clear the callback if we're not in hierarchy mode
    if (selectionMode !== "hierarchy") {
      onFinalProductsChange?.([]);
    }
  }, [selectionMode]);

  // Debug useEffect to monitor selectedAttributes changes
  useEffect(() => {
    console.log(
      "[DEBUG] selectedAttributes changed:",
      selectedAttributes.map((attr, idx) => ({
        index: idx,
        level: attr.level,
        type: attr.type,
        code: attr.code,
        value: attr.value,
        hasCode: Array.isArray(attr.code) ? attr.code.length : "not array",
      }))
    );
  }, [selectedAttributes]);

  // Auto-load products when hierarchy changes (same as working ProductAttributesWithHierarchyFilter)
  useEffect(() => {
    if (productLoadTimeoutRef.current) {
      clearTimeout(productLoadTimeoutRef.current);
    }
    productLoadTimeoutRef.current = setTimeout(() => {
      console.log(
        "[Auto-load] Loading products automatically after hierarchy change..."
      );
      loadFinalProducts();
    }, 1000);
    return () => {
      if (productLoadTimeoutRef.current) {
        clearTimeout(productLoadTimeoutRef.current);
      }
    };
  }, [
    selectedAttributes,
    selectionMode,
    hierarchyTrigger,
    loadingChildOptions,
  ]);

  // Separate function for loading products (auto-triggered like working component)
  const loadFinalProducts = async () => {
    if (loadingRef.current) {
      return;
    }

    // Auto-mark that products are being loaded (like working component)
    setHasSearched(true);

    // Skip hierarchy mode if attributes are empty during initialization
    if (selectionMode === "hierarchy") {
      const hasAnyValues = selectedAttributes.some((attr) => {
        if (Array.isArray(attr.value)) {
          return attr.value.length > 0;
        }
        return false;
      });
      if (selectedAttributes.length > 0 && !hasAnyValues) {
        return;
      }
    }

    if (selectionMode === "all") {
      // For "all" mode, don't load any products - just indicate all products are selected
      setFinalProducts([]);
      setProductsByGroup({});
      // Notify parent that all products are selected without loading them
      onFinalProductsChange?.("ALL_PRODUCTS" as any);
      setLoadingProducts(false);
    } else if (selectionMode === "specific") {
      // For specific mode, load all products without hierarchy filter
      setFinalProducts([]);
      setProductsByGroup({});
      setCurrentPage(1);
      setHasMoreProducts(true);
      setLoadingProducts(true);

      try {
        const request = {
          PageNumber: 1,
          PageSize: DEFAULT_PAGE_SIZE,
          IsCountRequired: true,
          FilterCriterias: [], // No filters for specific mode - get all products
          SortCriterias: [], // Use empty array like working component
        };

        const response = await skuService.getAllSKUs(request);
        console.log("[Specific Mode] SKU Service Response:", response);

        // Handle response same as working ProductAttributesWithHierarchyFilter component
        let responseData;
        let isSuccess = false;

        if (response?.success) {
          isSuccess = true;
          responseData = response.data;
        } else if (response?.IsSuccess) {
          isSuccess = true;
          responseData = response.Data;
        }

        if (!isSuccess || !responseData?.PagedData) {
          throw new Error("Failed to load products");
        }

        // Map the API response to FinalProduct format
        const products: FinalProduct[] = responseData.PagedData.map(
          (sku: any) => ({
            UID: sku.SKUUID || sku.UID,
            Code: sku.SKUCode || sku.Code,
            Name: sku.SKULongName || sku.LongName || sku.Name,
            GroupName: sku.L2 || sku.L1 || "Uncategorized",
            GroupTypeName: sku.ParentUID,
            MRP: sku.MRP || 0,
            IsActive: sku.IsActive !== false,
          })
        );

        setTotalProductCount(responseData.TotalCount || products.length);
        setHasMoreProducts(products.length < responseData.TotalCount);
        setCurrentPage(1);

        const grouped = products.reduce((acc, product) => {
          const groupName = product.GroupName || "Uncategorized";
          if (!acc[groupName]) {
            acc[groupName] = [];
          }
          acc[groupName].push(product);
          return acc;
        }, {} as Record<string, FinalProduct[]>);

        setProductsByGroup(grouped);
        setFinalProducts(products);
      } catch (error) {
        console.error("Error loading products:", error);
        setFinalProducts([]);
        setProductsByGroup({});
        onFinalProductsChange?.([]);
      } finally {
        setLoadingProducts(false);
        loadingRef.current = false;
      }
      return;
    }

    // For hierarchy mode only, need valid attributes
    if (selectionMode === "hierarchy") {
      // Debug: Show all selected attributes
      console.log(
        "[Hierarchy Mode] All selectedAttributes:",
        selectedAttributes.map((attr, idx) => ({
          index: idx,
          level: attr.level,
          type: attr.type,
          code: attr.code,
          value: attr.value,
          hasCode: Array.isArray(attr.code) ? attr.code.length : 0,
        }))
      );

      // Get all attributes that have both code and value
      const validAttributes = selectedAttributes.filter((attr) => {
        if (Array.isArray(attr.code) && Array.isArray(attr.value)) {
          return attr.code.length > 0;
        }
        return false;
      });

      // Check if we're still loading child options (cascading in progress)
      const isLoadingAnyChild = Object.values(loadingChildOptions).some(
        (loading) => loading
      );
      if (isLoadingAnyChild) {
        console.log(
          "[Hierarchy Mode] Still loading child options, skipping product load. Loading states:",
          loadingChildOptions
        );
        return;
      }

      console.log(
        "[Hierarchy Mode] Valid attributes (with selections):",
        validAttributes.map((a) => ({
          level: a.level,
          type: a.type,
          codes: a.code,
        }))
      );

      if (validAttributes.length === 0) {
        // Only clear products if we actually had some before (not during initialization)
        if (finalProducts.length > 0) {
          setFinalProducts([]);
          setProductsByGroup({});
          onFinalProductsChange?.([]);
        }
        return;
      }

      // Build cache key from attributes
      const cacheKey = `hierarchy_${validAttributes
        .map((a) => {
          if (Array.isArray(a.code) && Array.isArray(a.value)) {
            return a.code.join(",") + "_" + a.value.join(",");
          }
          return `${a.code}_${a.value}`;
        })
        .join("_")}`;
      const cached = productCache.get(cacheKey);

      if (cached && Date.now() - cached.timestamp < CACHE_DURATION) {
        startTransition(() => {
          setFinalProducts(cached.data);

          const grouped = cached.data.reduce((acc, product) => {
            const groupName = product.GroupName || "Uncategorized";
            if (!acc[groupName]) {
              acc[groupName] = [];
            }
            acc[groupName].push(product);
            return acc;
          }, {} as Record<string, FinalProduct[]>);
          setProductsByGroup(grouped);

          // Don't auto-select products in hierarchy mode - let users choose
          onFinalProductsChange?.([]);
        });
        return;
      }

      loadingRef.current = true;
      startTransition(() => {
        setLoadingProducts(true);
      });

      try {
        let allProducts: FinalProduct[] = [];

        // Build filter criteria for the API - Use proper hierarchy filtering logic
        const filterCriterias: any[] = [];

        // Add search filter if provided
        if (debouncedSearchQuery) {
          filterCriterias.push({
            Name: "skucodeandname",
            Value: debouncedSearchQuery,
          });
        }

        // Add active filter
        filterCriterias.push({
          Name: "IsActive",
          Value: true,
        });

        // Get the last (most specific) valid attribute for filtering
        const lastAttribute = validAttributes[validAttributes.length - 1];

        // Get the hierarchy type for the selected attribute (declare early for logging)
        const selectedType = lastAttribute
          ? hierarchyTypes[lastAttribute.level - 1]
          : null;

        console.log("========== HIERARCHY ATTRIBUTE SELECTION ==========");
        console.log(
          "[Hierarchy Mode] Total selectedAttributes:",
          selectedAttributes.length
        );
        console.log(
          "[Hierarchy Mode] Valid attributes (with selections):",
          validAttributes.length
        );
        console.log(
          "[Hierarchy Mode] All valid attributes:",
          validAttributes.map((a, idx) => ({
            index: idx,
            level: a.level,
            type: a.type,
            codes: a.code,
            values: a.value,
          }))
        );
        console.log(
          "[Hierarchy Mode] Using LAST attribute (index " +
            (validAttributes.length - 1) +
            ", level " +
            lastAttribute?.level +
            "):",
          {
            type: lastAttribute?.type,
            codes: lastAttribute?.code,
            values: lastAttribute?.value,
          }
        );
        console.log("===================================================");

        // Implement proper hierarchy filtering logic - same as ProductAttributesWithHierarchyFilter
        if (
          lastAttribute &&
          Array.isArray(lastAttribute.code) &&
          lastAttribute.code.length > 0
        ) {
          console.log("Selected type:", selectedType?.Name);

          // If a Brand is selected, filter by ParentUID directly
          if (
            selectedType?.Name === "Brand" ||
            selectedType?.Code === "Brand"
          ) {
            // Handle multiple brand selections
            if (lastAttribute.code.length === 1) {
              console.log(
                "Single brand selected, adding ParentUID filter:",
                lastAttribute.code[0]
              );
              filterCriterias.push({
                Name: "ParentUID",
                Value: lastAttribute.code[0],
              });
            } else {
              console.log(
                "Multiple brands selected, adding ParentUIDs filter:",
                lastAttribute.code
              );
              filterCriterias.push({
                Name: "ParentUIDs",
                Value: JSON.stringify(lastAttribute.code),
              });
            }
          } else {
            // For non-brand selections, get all child brands and filter by multiple ParentUIDs
            console.log("Non-brand selected, getting all child brands...");

            const allBrandUIDs = new Set<string>();

            // Recursively get all descendant brands
            const getDescendantBrands = async (parentUID: string) => {
              const childGroups = await hierarchyService.getChildSKUGroups(
                parentUID
              );
              console.log(
                `Found ${childGroups.length} children for ${parentUID}`
              );

              for (const child of childGroups) {
                // Check if this is a brand
                const childType = hierarchyTypes.find(
                  (t) => t.UID === child.SKUGroupTypeUID
                );
                if (
                  childType?.Name === "Brand" ||
                  childType?.Code === "Brand"
                ) {
                  allBrandUIDs.add(child.UID);
                  console.log(
                    "Found brand:",
                    child.Code,
                    child.Name,
                    "UID:",
                    child.UID
                  );
                } else {
                  // Recursively get children if not a brand
                  await getDescendantBrands(child.UID);
                }
              }
            };

            // Get all brands for all selected codes
            for (const selectedCode of lastAttribute.code) {
              await getDescendantBrands(selectedCode);
            }

            console.log("All brand UIDs found:", Array.from(allBrandUIDs));

            // Add filter for multiple ParentUIDs
            if (allBrandUIDs.size > 0) {
              const brandUIDsArray = Array.from(allBrandUIDs);
              console.log(
                "Adding ParentUIDs filter with brands:",
                brandUIDsArray
              );
              filterCriterias.push({
                Name: "ParentUIDs",
                Value: JSON.stringify(brandUIDsArray),
              });
            } else {
              console.log(
                "WARNING: No brands found for the selected hierarchy!"
              );
            }
          }
        }

        // Use SKU service (same as working ProductAttributesWithHierarchyFilter component)
        const request = {
          PageNumber: 1,
          PageSize: DEFAULT_PAGE_SIZE,
          IsCountRequired: true,
          FilterCriterias: filterCriterias,
          SortCriterias: [], // Use empty array like working component
        };

        console.log("========== HIERARCHY MODE API CALL ==========");
        console.log("[Hierarchy Mode] Selected Type:", selectedType?.Name);
        console.log(
          "[Hierarchy Mode] Selected Codes Count:",
          lastAttribute.code.length
        );
        console.log("[Hierarchy Mode] Selected Codes:", lastAttribute.code);
        console.log(
          "[Hierarchy Mode] Filter Criterias Count:",
          filterCriterias.length
        );

        // Log each filter criteria details
        filterCriterias.forEach((criteria, index) => {
          console.log(
            `[Filter ${index}] Name: "${criteria.Name}", Value:`,
            criteria.Value
          );
          if (criteria.Name === "ParentUIDs") {
            try {
              const parsed = JSON.parse(criteria.Value);
              console.log(
                `[Filter ${index}] Parsed ParentUIDs:`,
                parsed,
                "Count:",
                parsed.length
              );
            } catch (e) {
              console.log(`[Filter ${index}] Failed to parse ParentUIDs:`, e);
            }
          }
        });

        console.log(
          "[Hierarchy Mode] Request:",
          JSON.stringify(request, null, 2)
        );
        console.log("=============================================");

        const response = await skuService.getAllSKUs(request);
        console.log(
          "[Hierarchy Mode] SKU Service Response Success:",
          response?.success
        );
        console.log(
          "[Hierarchy Mode] SKU Service Response IsSuccess:",
          response?.IsSuccess
        );
        console.log(
          "[Hierarchy Mode] SKU Service Response Error:",
          response?.error
        );
        console.log("[Hierarchy Mode] Full Response:", response);

        // Handle response same as working ProductAttributesWithHierarchyFilter component
        let responseData;
        let isSuccess = false;

        if (response?.success) {
          // Service call was successful
          isSuccess = true;
          responseData = response.data;
          console.log("[Hierarchy Mode] Using response.data:", responseData);
        } else if (response?.IsSuccess) {
          // Direct API response format
          isSuccess = true;
          responseData = response.Data;
          console.log("[Hierarchy Mode] Using response.Data:", responseData);
        }

        console.log("[Hierarchy Mode] Final isSuccess:", isSuccess);
        console.log("[Hierarchy Mode] Final responseData:", responseData);
        console.log(
          "[Hierarchy Mode] PagedData exists:",
          !!responseData?.PagedData
        );
        console.log(
          "[Hierarchy Mode] PagedData length:",
          responseData?.PagedData?.length || 0
        );

        if (isSuccess && responseData && responseData.PagedData) {
          // Map the API response to FinalProduct format - server-side filtering is already applied
          allProducts = responseData.PagedData.map((sku: any) => ({
            UID: sku.SKUUID || sku.UID,
            Code: sku.SKUCode || sku.Code,
            Name: sku.SKULongName || sku.LongName || sku.Name,
            GroupName: sku.L2 || sku.L1 || "Products",
            GroupTypeName: sku.ParentUID,
            MRP: sku.MRP || 0,
            IsActive: sku.IsActive !== false,
            ParentUID: sku.ParentUID,
          }));

          console.log(
            `[Hierarchy Mode] Server-side filtering applied: received ${allProducts.length} products matching hierarchy selection`
          );
        } else {
          allProducts = [];
          console.log(
            "[Hierarchy Mode] No products in API response or request failed"
          );
          console.log(
            "[Hierarchy Mode] Response structure issue - isSuccess:",
            isSuccess,
            "responseData:",
            !!responseData,
            "PagedData:",
            !!responseData?.PagedData
          );
        }

        // Cache the results
        productCache.set(cacheKey, {
          data: allProducts,
          timestamp: Date.now(),
        });

        // Group products by their GroupName
        const grouped = allProducts.reduce((acc, product) => {
          const groupName = product.GroupName || "Products";
          if (!acc[groupName]) {
            acc[groupName] = [];
          }
          acc[groupName].push(product);
          return acc;
        }, {} as Record<string, FinalProduct[]>);

        setProductsByGroup(grouped);
        setFinalProducts(allProducts);

        // Don't auto-select products in hierarchy mode - let users choose
        onFinalProductsChange?.([]);
      } catch (error) {
        console.error("Error loading hierarchy products:", error);
        setFinalProducts([]);
        setProductsByGroup({});
        onFinalProductsChange?.([]);
      } finally {
        setLoadingProducts(false);
      }
    }
  };

  // Handle multi-select attribute change
  const handleMultiAttributeChange = (
    index: number,
    selectedCodes: string[]
  ) => {
    const updated = [...selectedAttributes];

    console.log(
      `[handleMultiAttributeChange] Called for index ${index} (${updated[index]?.type})`
    );
    console.log(`[handleMultiAttributeChange] Selected codes:`, selectedCodes);

    // Reset search state when attributes change
    setHasSearched(false);
    setFinalProducts([]);
    setProductsByGroup({});

    if (selectedCodes.length === 0) {
      // Clear this attribute and all subsequent ones
      updated[index] = { ...updated[index], code: [], value: [] };
      for (let i = index + 1; i < updated.length; i++) {
        updated[i] = { ...updated[i], code: [], value: [] }; // Use empty arrays for consistency
      }

      // Clear child options from hierarchyOptions
      for (let i = index + 1; i < hierarchyTypes.length; i++) {
        const childType = hierarchyTypes[i];
        if (childType) {
          setHierarchyOptions((prev) => ({
            ...prev,
            [childType.Name]: [],
          }));
        }
      }
    } else {
      // Get corresponding values for all selected codes
      const selectedOptions =
        hierarchyOptions[updated[index].type]?.filter((opt) =>
          selectedCodes.includes(opt.code)
        ) || [];

      const values = selectedOptions.map((opt) => opt.value);
      updated[index] = {
        ...updated[index],
        code: selectedCodes,
        value: values,
      };

      // Clear subsequent attributes when changing a parent attribute
      for (let i = index + 1; i < updated.length; i++) {
        updated[i] = { ...updated[i], code: [], value: [] }; // Use empty arrays for consistency
      }

      // Load child options asynchronously for smooth UI
      if (index < hierarchyTypes.length - 1) {
        const nextLevel = hierarchyTypes[index + 1];
        if (nextLevel) {
          // Show loading state
          setLoadingChildOptions((prev) => ({ ...prev, [index + 1]: true }));

          // Load children asynchronously
          (async () => {
            try {
              // Load children for all selected parent codes in parallel
              const childPromises = selectedCodes.map((parentCode) =>
                hierarchyService.getChildSKUGroups(parentCode)
              );

              const childResults = await Promise.all(childPromises);
              const allChildOptions: HierarchyOption[] = [];

              childResults.forEach((childGroups) => {
                const childOptions = childGroups.map((group) => ({
                  code: group.Code,
                  value: group.Name,
                  type: nextLevel.Name,
                }));
                allChildOptions.push(...childOptions);
              });

              // Remove duplicates
              const uniqueChildOptions = Array.from(
                new Map(allChildOptions.map((opt) => [opt.code, opt])).values()
              );

              // Update hierarchy options with filtered children
              setHierarchyOptions((prev) => ({
                ...prev,
                [nextLevel.Name]: uniqueChildOptions,
              }));

              // Clear subsequent levels if any
              for (let i = index + 2; i < hierarchyTypes.length; i++) {
                const childType = hierarchyTypes[i];
                if (childType) {
                  setHierarchyOptions((prev) => ({
                    ...prev,
                    [childType.Name]: [],
                  }));
                }
              }
            } catch (error) {
              console.error("Error loading child options:", error);
              // Clear child options on error
              setHierarchyOptions((prev) => ({
                ...prev,
                [nextLevel.Name]: [],
              }));
            } finally {
              // Clear loading state
              setLoadingChildOptions((prev) => ({
                ...prev,
                [index + 1]: false,
              }));
            }
          })();
        }
      }
    }

    console.log(
      `[handleMultiAttributeChange] Updated attributes:`,
      updated.map((a) => ({
        type: a.type,
        level: a.level,
        codes: a.code,
        values: a.value,
      }))
    );

    setSelectedAttributes(updated);

    // Call onChange with the updated attributes
    onChange(updated);

    // Auto-loading is handled by useEffect - same as working ProductAttributesWithHierarchyFilter
    // This prevents race conditions and ensures we use the final cascaded selection
  };

  // [REMOVED] loadProductsImmediately function - merged with main loading logic to prevent race conditions

  const refreshHierarchy = async () => {
    setLoadingHierarchy(true);
    setError(null);

    try {
      const [types, options] = await Promise.all([
        hierarchyService.getHierarchyTypes(),
        hierarchyService.getAllHierarchyOptions(),
      ]);

      setHierarchyTypes(types);
      setHierarchyOptions(options);
    } catch (error) {
      setError(
        error instanceof Error ? error.message : "Failed to refresh hierarchy"
      );
    } finally {
      setLoadingHierarchy(false);
    }
  };

  if (loadingHierarchy) {
    return (
      <Card>
        <CardContent className="p-6">
          <div className="space-y-4">
            <Skeleton className="h-4 w-48" />
            <div className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardContent className="p-6">
          <Alert variant="destructive">
            <AlertDescription>
              {error}
              <Button
                variant="outline"
                size="sm"
                onClick={refreshHierarchy}
                className="ml-2"
              >
                <RefreshCw className="h-4 w-4 mr-1" />
                Retry
              </Button>
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  if (hierarchyTypes.length === 0 && !loadingHierarchy) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Product Selection</CardTitle>
              <p className="text-sm text-gray-600 mt-1">
                Select products for your promotion
              </p>
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={refreshHierarchy}
              disabled={loadingHierarchy}
            >
              <RefreshCw className="h-4 w-4 mr-2" />
              Retry Loading
            </Button>
          </div>
        </CardHeader>
        <CardContent className="p-6">
          <Alert className="border-amber-200 bg-amber-50">
            <AlertCircle className="h-4 w-4 text-amber-600" />
            <AlertDescription className="text-amber-800">
              <strong>Product hierarchy is loading...</strong>
              <br />
              If this message persists, try refreshing or check system
              configuration.
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Product Selection</CardTitle>
            <p className="text-sm text-gray-600 mt-1">
              Select products for your promotion using different selection modes
            </p>
          </div>
          <Button
            variant="outline"
            size="sm"
            onClick={refreshHierarchy}
            disabled={loadingHierarchy}
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
        </div>
      </CardHeader>

      <CardContent className="p-6">
        {/* Selection Mode Radio Buttons */}
        <div className="mb-6 p-4 bg-gray-50 rounded-lg border">
          <Label className="text-sm font-medium mb-3 block">
            Product Selection Mode
          </Label>
          <RadioGroup
            value={selectionMode}
            onValueChange={(value) => {
              const mode = value as "all" | "hierarchy" | "specific";
              setSelectionMode(mode);
              onSelectionModeChange?.(mode);
            }}
            className="flex gap-6"
          >
            <div className="flex items-center space-x-2">
              <RadioGroupItem value="all" id="all" />
              <Label
                htmlFor="all"
                className="cursor-pointer flex items-center gap-2"
              >
                <span className="text-sm font-medium">Select All</span>
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <RadioGroupItem value="hierarchy" id="hierarchy" />
              <Label htmlFor="hierarchy" className="cursor-pointer">
                <span className="text-sm font-medium">
                  By Hierarchy (Cascading)
                </span>
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <RadioGroupItem value="specific" id="specific" />
              <Label htmlFor="specific" className="cursor-pointer">
                <span className="text-sm font-medium">Specific Products</span>
              </Label>
            </div>
          </RadioGroup>
        </div>

        {/* Mode-specific content */}
        {selectedAttributes.length === 0 && !loadingHierarchy ? (
          <Alert className="border-yellow-200 bg-yellow-50">
            <AlertCircle className="h-4 w-4 text-yellow-600" />
            <AlertDescription className="text-yellow-900">
              <strong>Product hierarchy not configured</strong>
              <br />
              No SKU Group Types found in the system. You can still select
              products manually using Select Specific Products mode above.
              <br />
              <span className="text-sm">
                To configure product hierarchy, please set up SKU Group Types in
                the admin panel.
              </span>
            </AlertDescription>
          </Alert>
        ) : selectedAttributes.length > 0 &&
          selectedAttributes.every(
            (attr) =>
              !hierarchyOptions[attr.type] ||
              hierarchyOptions[attr.type].length === 0
          ) &&
          !loadingHierarchy ? (
          <Alert className="border-amber-200 bg-amber-50">
            <Info className="h-4 w-4 text-amber-600" />
            <AlertDescription className="text-amber-700">
              <strong>Product hierarchy configuration incomplete</strong>
              <br />
              The system has hierarchy levels configured (
              {selectedAttributes.map((a) => a.type).join(", ")}) but no SKU
              Groups have been created yet.
              <br />
              <br />
              <strong>Setup Required:</strong>
              <ul className="list-disc list-inside mt-2 space-y-1">
                <li>
                  <strong>Step 1:</strong> Create SKU Groups in the admin panel
                  for each hierarchy level
                </li>
                <li>
                  <strong>Step 2:</strong> Map existing SKUs to the appropriate
                  SKU Groups
                </li>
                <li>
                  <strong>Alternative:</strong> Use{" "}
                  <strong>Specific Products</strong> mode to select products
                  directly without hierarchy
                </li>
              </ul>
              <br />
              <div className="bg-blue-50 border border-blue-200 rounded p-2 mt-2">
                <p className="text-sm text-blue-800">
                  <strong>Note:</strong> Once SKU Groups are configured, the
                  hierarchy will automatically populate and allow cascading
                  selection.
                </p>
              </div>
            </AlertDescription>
          </Alert>
        ) : loadingHierarchy ? (
          <div className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : (
          <div className="space-y-4">
            {/* Select All Mode - Just show info */}
            {selectionMode === "all" && (
              <Alert className="border-blue-200 bg-blue-50">
                <CheckCircle2 className="h-4 w-4 text-blue-600" />
                <AlertDescription className="text-blue-900">
                  <strong>All Products Mode</strong>
                  <br />
                  All products in your catalog will be included in this
                  promotion.
                  {totalProductCount > 0 && (
                    <div className="mt-2 text-sm">
                      <strong>{totalProductCount.toLocaleString()}</strong>{" "}
                      total products will be included
                    </div>
                  )}
                </AlertDescription>
              </Alert>
            )}

            {/* Hierarchy Mode - Show cascading selection */}
            {selectionMode === "hierarchy" && (
              <div
                className="p-4 border rounded-lg bg-white relative"
                style={{ isolation: "isolate" }}
              >
                <div className="mb-4">
                  <Label className="text-sm font-semibold text-gray-800 block">
                    Product Hierarchy Selection
                  </Label>
                  <p className="text-xs text-gray-500 mt-1">
                    Select one or more options at each level. Child levels
                    appear after parent selection.
                  </p>
                </div>
                <div className="flex flex-wrap gap-4">
                  {selectedAttributes
                    .filter((attr) => {
                      // Hide hierarchy levels that have no options
                      const options = hierarchyOptions[attr.type] || [];
                      return options.length > 0;
                    })
                    .map((attr, filteredIndex) => {
                      // Find the actual index in the original selectedAttributes array
                      const actualIndex = selectedAttributes.findIndex(
                        (a) => a.type === attr.type
                      );

                      // Check if parent has selection(s)
                      const parentHasSelection =
                        actualIndex === 0 ||
                        (() => {
                          const parentAttr =
                            selectedAttributes[actualIndex - 1];
                          if (!parentAttr) return false;

                          if (Array.isArray(parentAttr.code)) {
                            return parentAttr.code.length > 0;
                          }
                          return !!parentAttr.code;
                        })();

                      const isLoading =
                        loadingChildOptions[actualIndex] || false;
                      const isEnabled =
                        !disabled && parentHasSelection && !isLoading;
                      const shouldShow = parentHasSelection;

                      if (!shouldShow) return null;

                      const multiSelectOptions: MultiSelectOption[] = (
                        hierarchyOptions[attr.type] || []
                      ).map((opt) => ({
                        value: opt.code,
                        label: opt.value,
                        code: opt.code,
                      }));

                      // Ensure selectedCodes is always an array
                      let selectedCodes: string[] = [];
                      if (Array.isArray(attr.code)) {
                        selectedCodes = attr.code;
                      } else if (attr.code && typeof attr.code === "string") {
                        selectedCodes = [attr.code];
                      }

                      return (
                        <div
                          key={`${attr.type}-${actualIndex}`}
                          className="flex-1"
                          style={{ minWidth: "250px" }}
                        >
                          <div className="space-y-2">
                            <div className="flex items-center gap-2 mb-2">
                              <div className="flex items-center justify-center w-6 h-6 rounded-full bg-blue-100 text-blue-600 text-xs font-semibold">
                                {filteredIndex + 1}
                              </div>
                              <Label className="font-medium text-sm text-gray-700">
                                {attr.type}
                                {filteredIndex === 0 && (
                                  <span className="text-red-500 ml-1">*</span>
                                )}
                              </Label>
                            </div>

                            {/* Dropdown with loading state */}
                            <div className="relative">
                              {isLoading ? (
                                <Skeleton className="h-10 w-full" />
                              ) : (
                                <MultiSelect
                                  options={multiSelectOptions}
                                  selected={selectedCodes}
                                  onChange={(selected) =>
                                    handleMultiAttributeChange(
                                      actualIndex,
                                      selected
                                    )
                                  }
                                  placeholder={`Select ${attr.type.toLowerCase()}...`}
                                  disabled={!isEnabled}
                                  className="w-full"
                                  hideSelectedInButton={true}
                                />
                              )}
                            </div>
                          </div>
                        </div>
                      );
                    })}
                </div>

                {/* Show products section when hierarchy is selected */}
                {selectedAttributes.some(
                  (attr) => attr.code && attr.code.length > 0
                ) && (
                  <>
                    {/* Products List for Hierarchy Mode - Professional Card Layout */}
                    <div className="mt-6 border-t pt-4">
                      <div className="flex items-center justify-between mb-4">
                        <h4 className="text-sm font-semibold text-gray-800 flex items-center gap-2">
                          <Package className="w-4 h-4 text-blue-600" />
                          Products in Selected Hierarchy
                          {loadingProducts && (
                            <RefreshCw className="w-3 h-3 animate-spin text-blue-600 ml-1" />
                          )}
                        </h4>
                        <div className="flex items-center gap-3">
                          <button
                            type="button"
                            onClick={() => {
                              const allProductIds = new Set(
                                filteredProducts.map((p) => p.UID)
                              );
                              setSelectedProducts(allProductIds);
                              const newQuantities: Record<string, number> = {};
                              filteredProducts.forEach((p) => {
                                newQuantities[p.UID] = 1;
                              });
                              setProductQuantities(newQuantities);
                            }}
                            className="text-xs px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700"
                          >
                            Select All
                          </button>
                          <button
                            type="button"
                            onClick={() => {
                              setSelectedProducts(new Set());
                              setProductQuantities({});
                            }}
                            className="text-xs px-3 py-1 border border-gray-300 rounded hover:bg-gray-50"
                          >
                            Clear All
                          </button>
                          <span className="text-xs text-gray-500 font-medium">
                            {selectedProducts.size} of {filteredProducts.length}{" "}
                            selected
                          </span>
                        </div>
                      </div>

                      {/* Search Bar */}
                      <div className="mb-3">
                        <div className="relative">
                          <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
                          <Input
                            type="text"
                            placeholder="Search products by name or code..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            className="pl-9 pr-4 py-2 text-sm border-gray-300 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                          />
                          {searchQuery && (
                            <button
                              type="button"
                              onClick={() => setSearchQuery("")}
                              className="absolute right-2 top-2.5 text-gray-400 hover:text-gray-600"
                            >
                              <X className="h-4 w-4" />
                            </button>
                          )}
                        </div>
                        {searchQuery && (
                          <p className="text-xs text-gray-500 mt-1">
                            Showing {filteredProducts.length} of{" "}
                            {finalProducts.length} products
                          </p>
                        )}
                      </div>

                      {/* Professional Card Layout - Same as Specific Mode */}
                      <div className="border rounded-lg bg-gray-50 max-h-96 overflow-y-auto">
                        {/* Loading skeleton - shows in-place without overlay */}
                        {loadingProducts && filteredProducts.length === 0 && (
                          <div className="p-4 space-y-3">
                            {Array.from({ length: 3 }).map((_, i) => (
                              <div
                                key={i}
                                className="bg-white border border-gray-200 rounded-lg p-4"
                              >
                                <div className="flex items-start gap-3">
                                  <Skeleton className="h-4 w-4 mt-0.5" />
                                  <Package className="h-5 w-5 text-gray-300 mt-0.5 flex-shrink-0" />
                                  <div className="flex-1">
                                    <Skeleton className="h-4 w-48 mb-2" />
                                    <div className="flex items-center gap-4 mt-1">
                                      <Skeleton className="h-3 w-16" />
                                      <Skeleton className="h-3 w-24" />
                                    </div>
                                  </div>
                                </div>
                              </div>
                            ))}
                          </div>
                        )}

                        {/* No products found message */}
                        {!loadingProducts && filteredProducts.length === 0 && (
                          <div className="p-8 text-center">
                            <Package className="h-12 w-12 text-gray-300 mx-auto mb-3" />
                            <h3 className="text-sm font-medium text-gray-900 mb-1">
                              No products found
                            </h3>
                            <p className="text-xs text-gray-500 max-w-sm mx-auto">
                              {searchQuery ? (
                                <>
                                  No products match your search "
                                  <strong>{searchQuery}</strong>" in the
                                  selected hierarchy.
                                </>
                              ) : (
                                <>
                                  No products available for the selected
                                  attribute combination. Try selecting different
                                  attributes or check if products are configured
                                  for this hierarchy level.
                                </>
                              )}
                            </p>
                          </div>
                        )}

                        {/* Products list */}
                        {filteredProducts.length > 0 && (
                          <div className="p-4 space-y-3">
                            {(() => {
                              // Group filtered products
                              const filteredGrouped = filteredProducts.reduce(
                                (acc, product) => {
                                  const groupName =
                                    product.GroupName || "Uncategorized";
                                  if (!acc[groupName]) {
                                    acc[groupName] = [];
                                  }
                                  acc[groupName].push(product);
                                  return acc;
                                },
                                {} as Record<string, FinalProduct[]>
                              );

                              return Object.entries(filteredGrouped).map(
                                ([groupName, products]) => (
                                  <div key={groupName} className="space-y-2">
                                    <div className="flex items-center justify-between bg-white p-2 rounded-lg border">
                                      <div className="flex items-center gap-2">
                                        <Layers className="w-4 h-4 text-blue-600" />
                                        <span className="font-medium">
                                          {groupName}
                                        </span>
                                        <Badge
                                          variant="outline"
                                          className="text-xs"
                                        >
                                          {products.length} products
                                        </Badge>
                                      </div>
                                      <Button
                                        variant="ghost"
                                        size="sm"
                                        onClick={() => {
                                          const groupProductIds = products.map(
                                            (p) => p.UID
                                          );
                                          const newSelected = new Set(
                                            selectedProducts
                                          );
                                          const allSelected =
                                            groupProductIds.every((id) =>
                                              newSelected.has(id)
                                            );

                                          if (allSelected) {
                                            groupProductIds.forEach((id) =>
                                              newSelected.delete(id)
                                            );
                                            setProductQuantities((prev) => {
                                              const newQuantities = { ...prev };
                                              groupProductIds.forEach(
                                                (id) => delete newQuantities[id]
                                              );
                                              return newQuantities;
                                            });
                                          } else {
                                            groupProductIds.forEach((id) => {
                                              newSelected.add(id);
                                            });
                                            setProductQuantities((prev) => {
                                              const newQuantities = { ...prev };
                                              groupProductIds.forEach((id) => {
                                                if (!newQuantities[id])
                                                  newQuantities[id] = 1;
                                              });
                                              return newQuantities;
                                            });
                                          }
                                          setSelectedProducts(newSelected);
                                        }}
                                      >
                                        {products.every((p) =>
                                          selectedProducts.has(p.UID)
                                        ) ? (
                                          <CheckCircle2 className="w-4 h-4 text-green-600" />
                                        ) : (
                                          <Package className="w-4 h-4" />
                                        )}
                                      </Button>
                                    </div>

                                    <div className="ml-6 space-y-2">
                                      {products.map((product) => (
                                        <div
                                          key={product.UID}
                                          className={cn(
                                            "bg-white border border-gray-200 rounded-lg p-4 transition-all duration-200",
                                            selectedProducts.has(product.UID)
                                              ? "border-blue-300 shadow-sm bg-blue-50/50"
                                              : "hover:border-gray-300 hover:shadow-sm"
                                          )}
                                        >
                                          {/* Product Header */}
                                          <div className="flex items-start gap-3">
                                            <Checkbox
                                              id={`product-checkbox-${product.UID}`}
                                              checked={selectedProducts.has(
                                                product.UID
                                              )}
                                              onCheckedChange={(checked) => {
                                                const newSelected = new Set(
                                                  selectedProducts
                                                );
                                                if (checked) {
                                                  newSelected.add(product.UID);
                                                  setProductQuantities(
                                                    (prev) => ({
                                                      ...prev,
                                                      [product.UID]:
                                                        prev[product.UID] || 1,
                                                    })
                                                  );
                                                } else {
                                                  newSelected.delete(
                                                    product.UID
                                                  );
                                                  setProductQuantities(
                                                    (prev) => {
                                                      const newQuantities = {
                                                        ...prev,
                                                      };
                                                      delete newQuantities[
                                                        product.UID
                                                      ];
                                                      return newQuantities;
                                                    }
                                                  );
                                                }
                                                setSelectedProducts(
                                                  newSelected
                                                );
                                              }}
                                              className="mt-0.5"
                                            />
                                            <Package className="h-5 w-5 text-gray-400 mt-0.5 flex-shrink-0" />
                                            <div className="flex-1 min-w-0">
                                              <h4 className="font-medium text-gray-900 text-sm leading-5">
                                                {product.Name}
                                              </h4>
                                              <div className="flex items-center gap-4 mt-1 text-xs text-gray-500">
                                                <span className="bg-gray-100 px-2 py-0.5 rounded text-xs font-mono">
                                                  {product.Code}
                                                </span>
                                                <span>{product.GroupName}</span>
                                              </div>
                                            </div>
                                          </div>

                                          {/* Quantity Controls - Only show when selected */}
                                          {selectedProducts.has(
                                            product.UID
                                          ) && (
                                            <div className="mt-4 pt-4 border-t border-gray-200">
                                              {/* Min Purchase Qty Section */}
                                              <div className="flex items-center gap-3 mb-3">
                                                <ShoppingCart className="h-4 w-4 text-blue-600" />
                                                <span className="text-sm font-medium text-gray-700">
                                                  Min. Purchase Qty
                                                </span>
                                                <div className="flex items-center gap-1">
                                                  <button
                                                    type="button"
                                                    onClick={() => {
                                                      const currentQty =
                                                        productQuantities[
                                                          product.UID
                                                        ] || 1;
                                                      if (currentQty > 1) {
                                                        setProductQuantities(
                                                          (prev) => ({
                                                            ...prev,
                                                            [product.UID]:
                                                              currentQty - 1,
                                                          })
                                                        );
                                                      }
                                                    }}
                                                    className="h-8 w-8 flex items-center justify-center border border-gray-300 rounded-md hover:bg-gray-50 text-gray-600"
                                                  >
                                                    <Minus className="w-3 h-3" />
                                                  </button>
                                                  <input
                                                    type="number"
                                                    value={
                                                      productQuantities[
                                                        product.UID
                                                      ] || 1
                                                    }
                                                    onChange={(e) => {
                                                      const val =
                                                        parseInt(
                                                          e.target.value
                                                        ) || 1;
                                                      setProductQuantities(
                                                        (prev) => ({
                                                          ...prev,
                                                          [product.UID]:
                                                            Math.max(1, val),
                                                        })
                                                      );
                                                    }}
                                                    className="w-16 h-8 px-2 text-center text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
                                                    min="1"
                                                  />
                                                  <button
                                                    type="button"
                                                    onClick={() => {
                                                      const currentQty =
                                                        productQuantities[
                                                          product.UID
                                                        ] || 1;
                                                      setProductQuantities(
                                                        (prev) => ({
                                                          ...prev,
                                                          [product.UID]:
                                                            currentQty + 1,
                                                        })
                                                      );
                                                    }}
                                                    className="h-8 w-8 flex items-center justify-center border border-gray-300 rounded-md hover:bg-gray-50 text-gray-600"
                                                  >
                                                    <Plus className="w-3 h-3" />
                                                  </button>
                                                </div>
                                              </div>

                                              {/* Qualification Message */}
                                              <div className="flex items-center gap-2 mb-3">
                                                <CheckCircle2 className="h-4 w-4 text-blue-600" />
                                                <span className="text-sm text-blue-700">
                                                  Customer must purchase{" "}
                                                  {productQuantities[
                                                    product.UID
                                                  ] || 1}{" "}
                                                  unit to qualify for discount
                                                </span>
                                              </div>

                                              {/* Quick Set Buttons */}
                                              <div className="flex items-center gap-2">
                                                <span className="text-sm text-gray-600">
                                                  Quick set:
                                                </span>
                                                {[5, 10, 20, 50].map((qty) => (
                                                  <button
                                                    key={qty}
                                                    type="button"
                                                    onClick={() => {
                                                      setProductQuantities(
                                                        (prev) => ({
                                                          ...prev,
                                                          [product.UID]: qty,
                                                        })
                                                      );
                                                    }}
                                                    className={cn(
                                                      "px-3 py-1 text-xs rounded-md border transition-colors",
                                                      productQuantities[
                                                        product.UID
                                                      ] === qty
                                                        ? "bg-blue-600 text-white border-blue-600"
                                                        : "bg-white text-gray-600 border-gray-300 hover:bg-gray-50"
                                                    )}
                                                  >
                                                    {qty}
                                                  </button>
                                                ))}
                                              </div>
                                            </div>
                                          )}
                                        </div>
                                      ))}
                                    </div>
                                  </div>
                                )
                              );
                            })()}
                          </div>
                        )}

                        {/* Footer Summary */}
                        {selectedProducts.size > 0 && (
                          <div className="bg-blue-50 px-4 py-3 border-t border-blue-200">
                            <div className="flex items-center justify-between">
                              <div className="flex items-center gap-2">
                                <CheckCircle2 className="w-4 h-4 text-blue-600" />
                                <span className="text-sm font-medium text-blue-900">
                                  {selectedProducts.size} product
                                  {selectedProducts.size > 1 ? "s" : ""}{" "}
                                  selected
                                </span>
                              </div>
                              <div className="text-xs text-blue-700">
                                Total min. quantities:{" "}
                                {Object.values(productQuantities).reduce(
                                  (sum, qty) => sum + qty,
                                  0
                                )}{" "}
                                units
                              </div>
                            </div>
                          </div>
                        )}
                      </div>
                    </div>
                  </>
                )}
              </div>
            )}

            {/* Specific Mode - Just show info, products shown below */}
            {selectionMode === "specific" && selectedProducts.size > 0 && (
              <Alert className="border-purple-200 bg-purple-50">
                <Package className="h-4 w-4 text-purple-600" />
                <AlertDescription className="text-purple-900">
                  <div className="text-sm">
                    <strong>{selectedProducts.size}</strong> product
                    {selectedProducts.size !== 1 ? "s" : ""} selected
                  </div>
                </AlertDescription>
              </Alert>
            )}
          </div>
        )}

        {/* Specific Products Tab - Using same approach as product management */}
        {selectionMode === "specific" && (
          <div className="mt-6 border-t pt-4">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <Package className="w-4 h-4" />
                <span className="font-medium">Select Products</span>
                {selectedProducts.size > 0 && (
                  <Badge variant="default" className="ml-2">
                    {selectedProducts.size} selected
                  </Badge>
                )}
              </div>
            </div>

            {/* Use exact same approach as products management page */}
            <ProductsTable
              onProductSelectionChange={(selected, quantities) => {
                setSelectedProducts(new Set(selected));
                setProductQuantities(quantities);

                // Notify parent of changes
                const selectedProductsList = finalProducts
                  .filter((p) => selected.includes(p.UID))
                  .map((p) => ({
                    ...p,
                    quantity: quantities[p.UID] || 1,
                  }));
                onFinalProductsChange?.(selectedProductsList);
              }}
              initialSelected={Array.from(selectedProducts)}
              initialQuantities={productQuantities}
            />
          </div>
        )}
      </CardContent>
    </Card>
  );
}
