"use client";

import React, { useState, useEffect, useRef } from "react";
import {
  ChevronDown,
  ChevronUp,
  Search,
  Package,
  Filter,
  Loader2,
  CheckCircle,
  Eye,
  EyeOff,
  Minus,
  Plus,
  ShoppingCart,
  X,
  Check,
  AlertCircle
} from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import {
  API_CONFIG,
  getCommonHeaders,
  extractPagedData,
} from "../../services/api-config";
import {
  FilterType,
  FilterGroupType,
  FilterMode,
} from "../../constants/filterConstants";
import { promotionV3Service } from "../../services/promotionV3.service";
import {
  PAGINATION_CONFIG,
  createPagingRequest,
} from "../../config/pagination.config";
import { useProductHierarchy } from "../../hooks/useProductHierarchy";
import { hierarchyService } from "@/services/hierarchy.service";

// Types matching actual backend models
export interface SKUGroupType {
  UID: string;
  OrgUID: string;
  Code: string;
  Name: string;
  ParentUID?: string;
  ItemLevel: number;
  AvailableForFilter: boolean;
}

export interface SKUGroup {
  UID: string;
  SKUGroupTypeUID: string;
  Code: string;
  Name: string;
  ParentUID?: string;
  ParentName?: string;
  ItemLevel: number;
  SupplierOrgUID?: string;
}

export interface DynamicProductSelection {
  selectionType: "all" | "hierarchy" | "specific";
  hierarchySelections: {
    [groupTypeUid: string]: string[]; // GroupType UID -> selected Group UIDs
  };
  specificProducts: Array<
    string | { UID: string; Code: string; Name: string; hierarchyPath?: string }
  >;
  excludedProducts: Array<
    string | { UID: string; Code: string; Name: string; hierarchyPath?: string }
  >;
  selectedFinalProducts?: Array<
    string | { UID: string; Code: string; Name: string; hierarchyPath?: string }
  >; // Selected products from final preview
  productQuantities?: { [productId: string]: number }; // Product quantities
}

interface DynamicProductSelectorProps {
  orgUid?: string;
  value: DynamicProductSelection;
  onChange: (selection: DynamicProductSelection) => void;
  selectionModel?: "any" | "all";
  disabled?: boolean;
  onSelectionModelChange?: (model: "any" | "all") => void;
  showEmptyTypes?: boolean; // Show group types even if they have 0 groups
}

interface HierarchyOption {
  code: string;
  value: string;
  type: string;
}

interface SelectedAttribute {
  type: string;
  code: string;
  value: string;
  uid?: string;
}

interface FinalProduct {
  UID: string;
  Code: string;
  Name: string;
  GroupName?: string;
  GroupTypeName?: string;
  MRP?: number;
  IsActive?: boolean;
}

export default function DynamicProductSelector({
  orgUid = "",
  value,
  onChange,
  selectionModel = "any",
  disabled = false,
  onSelectionModelChange,
  showEmptyTypes = false
}: DynamicProductSelectorProps) {
  const [selectionType, setSelectionType] = useState<"all" | "hierarchy" | "specific">(
    value.selectionType || "all"
  );
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([]);
  const [hierarchyOptions, setHierarchyOptions] = useState<Record<string, HierarchyOption[]>>({});
  const [selectedAttributes, setSelectedAttributes] = useState<SelectedAttribute[]>([]);
  const [loadingHierarchy, setLoadingHierarchy] = useState(true);
  const [loadingChildOptions, setLoadingChildOptions] = useState<Record<number, boolean>>({});
  const [isLoadingTypes, setIsLoadingTypes] = useState(false);
  const [specificSearchTerm, setSpecificSearchTerm] = useState("");
  const [searchResults, setSearchResults] = useState<FinalProduct[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [selectedProducts, setSelectedProducts] = useState<string[]>([]);
  const [showFinalPreview, setShowFinalPreview] = useState(false);
  const [finalProducts, setFinalProducts] = useState<FinalProduct[]>([]);
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [productPage, setProductPage] = useState(1);
  const [hasMoreProducts, setHasMoreProducts] = useState(true);
  const [totalProductCount, setTotalProductCount] = useState(0);
  const productCache = useRef<{ [groupUID: string]: FinalProduct[] }>({});
  
  // Specific products tab state
  const [allProducts, setAllProducts] = useState<FinalProduct[]>([]);
  const [specificProductsPage, setSpecificProductsPage] = useState(1);
  const [loadingSpecificProducts, setLoadingSpecificProducts] = useState(false);
  const [hasMoreSpecificProducts, setHasMoreSpecificProducts] = useState(true);

  // Use the hook for loading hierarchy
  const { data: hierarchyData, isLoading: isLoadingHierarchy } = useProductHierarchy(orgUid, showEmptyTypes);
  
  // Load initial products when Specific Products tab is selected
  useEffect(() => {
    if (value.selectionType === "specific" && allProducts.length === 0 && !loadingSpecificProducts) {
      loadSpecificProducts(1, false);
    }
  }, [value.selectionType]);

  // Initialize hierarchy data exactly like SKU management
  useEffect(() => {
    const loadHierarchyData = async () => {
      try {
        console.log("🔍 Loading hierarchy data like SKU management...");
        setLoadingHierarchy(true);
        
        if (hierarchyData && hierarchyData.length > 0) {
          const sortedTypes = hierarchyData.sort((a, b) => (a.ItemLevel || 0) - (b.ItemLevel || 0));
          setHierarchyTypes(sortedTypes);
          
          // Only load options for the first level initially (exactly like SKU management)
          const firstLevelType = sortedTypes[0];
          if (firstLevelType) {
            console.log("🔄 Loading first level options for:", firstLevelType.Name, "UID:", firstLevelType.UID);
            const firstLevelOptions = await hierarchyService.getHierarchyOptionsForType(firstLevelType.UID);
            console.log("📋 First level raw options:", firstLevelOptions.map(o => ({ code: o.code, value: o.value })));
            setHierarchyOptions({ [firstLevelType.Name]: firstLevelOptions });
            console.log("✅ Loaded", firstLevelOptions.length, "options for first level", firstLevelType.Name);
          }
          
          // Initialize attributes array with all types but empty values (exactly like SKU management)
          const initialAttributes = sortedTypes.map(type => ({ 
            type: type.Name, 
            code: '', 
            value: '', 
            uid: '' 
          }));
          setSelectedAttributes(initialAttributes);
          
          console.log("🎯 Initialized hierarchy with", sortedTypes.length, "types");
        }
      } catch (error) {
        console.error('Failed to load hierarchy data:', error);
      } finally {
        setLoadingHierarchy(false);
      }
    };
    
    if (!isLoadingHierarchy && hierarchyData) {
      loadHierarchyData();
    }
  }, [hierarchyData, isLoadingHierarchy]);

  // Auto-load child options when parent selection changes (exactly like SKU management)
  useEffect(() => {
    if (hierarchyTypes.length === 0) return; // Don't run if types not loaded yet
    
    const loadChildOptionsForLevel = async (parentCode: string, levelIndex: number) => {
      if (levelIndex >= hierarchyTypes.length) return;
      
      try {
        setLoadingChildOptions(prev => ({ ...prev, [levelIndex]: true }));
        
        const childGroups = await hierarchyService.getChildSKUGroups(parentCode);
        const childType = hierarchyTypes[levelIndex];
        
        if (childType && childGroups.length > 0) {
          const childOptions = childGroups.map(group => ({
            code: group.Code,
            value: group.Name,
            type: childType.Name
          }));
          
          setHierarchyOptions(prev => ({
            ...prev,
            [childType.Name]: childOptions
          }));
          
          console.log("🔄 Auto-loaded", childOptions.length, "child options for", childType.Name, "with parent", parentCode);
        } else if (childType) {
          // Set empty array if no children found
          setHierarchyOptions(prev => ({
            ...prev,
            [childType.Name]: []
          }));
          console.log("❌ No children found for", childType.Name, "with parent", parentCode);
        }
      } catch (error) {
        console.error('Failed to auto-load child options:', error);
        // Still set empty array on error to show proper message
        if (hierarchyTypes[levelIndex]) {
          setHierarchyOptions(prev => ({
            ...prev,
            [hierarchyTypes[levelIndex].Name]: []
          }));
        }
      } finally {
        setLoadingChildOptions(prev => ({ ...prev, [levelIndex]: false }));
      }
    };
    
    // Check each level and load children if parent is selected (exactly like SKU management)
    selectedAttributes.forEach((attr, index) => {
      if (index > 0 && selectedAttributes[index - 1]?.code) {
        const parentCode = selectedAttributes[index - 1].code;
        const currentOptions = hierarchyOptions[attr.type];
        
        // Only load if not already loaded or loading
        if (currentOptions === undefined && !loadingChildOptions[index]) {
          console.log("🔄 Auto-loading options for level", index, "type", attr.type, "with parent", parentCode);
          loadChildOptionsForLevel(parentCode, index);
        }
      }
    });
  }, [selectedAttributes.map(a => a.code).join(','), hierarchyTypes.length]); // Only re-run when selections actually change

  // Load final products when hierarchy selections change
  const loadFinalProducts = async () => {
    if (selectionType !== "hierarchy" || Object.keys(value.hierarchySelections || {}).length === 0) {
      setFinalProducts([]);
      return;
    }

    setIsLoadingTypes(true);
    try {
      const products: FinalProduct[] = [];
      const groupUIDs: string[] = [];
      
      // Collect all selected group UIDs
      const lastLevelWithSelections = cascadingLevels
        .slice()
        .reverse()
        .find((level) => level.selectedGroups.length > 0);
      
      if (lastLevelWithSelections) {
        groupUIDs.push(...lastLevelWithSelections.selectedGroups);
      } else {
        for (const groupUIDsArr of Object.values(value.hierarchySelections || {})) {
          if (Array.isArray(groupUIDsArr)) groupUIDs.push(...groupUIDsArr);
        }
      }
      
      // Remove duplicates
      const uniqueGroupUIDs = Array.from(new Set(groupUIDs));
      
      // Load products in parallel, using cache if available
      const productPromises = uniqueGroupUIDs.map(async (groupUID) => {
        if (productCache.current[groupUID]) {
          return productCache.current[groupUID];
        } else {
          const groupName = cascadingLevels
            .flatMap((level) => level.groups)
            .find((g) => g.UID === groupUID)?.Name || groupUID;
          
          const groupProducts = await loadProductsBasedOnSelection(groupUID, groupName);
          productCache.current[groupUID] = groupProducts;
          return groupProducts;
        }
      });
      
      const allProducts = (await Promise.all(productPromises)).flat();
      
      // Remove duplicates by UID
      const uniqueProducts = allProducts.filter(
        (product: FinalProduct, index: number, self: FinalProduct[]) =>
          index === self.findIndex((p: FinalProduct) => p.UID === product.UID)
      );
      
      setFinalProducts(uniqueProducts);
    } catch (error) {
      console.error("Error loading final products:", error);
    } finally {
      setIsLoadingTypes(false);
    }
  };

  // Load products based on group selection
  const loadProductsBasedOnSelection = async (
    groupUID: string,
    groupName: string
  ): Promise<FinalProduct[]> => {
    try {
      // First get the mappings for this group
      const mappingsResponse = await fetch(
        `${API_CONFIG.baseURL}${API_CONFIG.endpoints.skuToGroupMapping.selectAll}`,
        {
          method: "POST",
          headers: getCommonHeaders(),
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 1000,
            FilterCriterias: [
              {
                Field: "SKUGroupUID",
                Value: groupUID,
                Operator: "Equal"
              }
            ],
            SortCriterias: [],
            IsCountRequired: false
          })
        }
      );

      if (!mappingsResponse.ok) {
        console.error("Failed to load SKU mappings");
        return [];
      }

      const mappingsResult = await mappingsResponse.json();
      console.log("Mappings API Response:", mappingsResult);
      
      // Handle multiple possible API response structures
      let mappings = [];
      if (mappingsResult?.PagedData) {
        mappings = mappingsResult.PagedData;
      } else if (mappingsResult?.Data?.PagedData) {
        mappings = mappingsResult.Data.PagedData;
      } else if (mappingsResult?.Data && Array.isArray(mappingsResult.Data)) {
        mappings = mappingsResult.Data;
      } else if (Array.isArray(mappingsResult)) {
        mappings = mappingsResult;
      }
      
      if (mappings.length === 0) {
        return [];
      }
      
      // Get SKU UIDs from mappings
      const skuUIDs = mappings.map((m: any) => m.SKUUID);
      
      // Fetch all SKUs using pagination to handle any number of products
      let allSkus = [];
      let currentPage = 1;
      const pageSize = 1000;
      let hasMoreData = true;
      
      console.log(`Loading all SKUs for ${skuUIDs.length} mapped products...`);
      
      while (hasMoreData) {
        const skusResponse = await fetch(
          `${API_CONFIG.baseURL}/SKU/SelectAllSKUDetailsWebView`,
          {
            method: "POST",
            headers: getCommonHeaders(),
            body: JSON.stringify({
              PageNumber: currentPage,
              PageSize: pageSize,
              FilterCriterias: [],
              SortCriterias: [],
              IsCountRequired: currentPage === 1
            })
          }
        );

        if (!skusResponse.ok) {
          console.error(`Failed to load SKUs page ${currentPage}`);
          break;
        }

        const skusResult = await skusResponse.json();
        
        // Handle multiple possible API response structures
        let pageData = [];
        if (skusResult?.PagedData) {
          pageData = skusResult.PagedData;
        } else if (skusResult?.Data?.PagedData) {
          pageData = skusResult.Data.PagedData;
        } else if (skusResult?.Data && Array.isArray(skusResult.Data)) {
          pageData = skusResult.Data;
        } else if (Array.isArray(skusResult)) {
          pageData = skusResult;
        }
        
        allSkus = [...allSkus, ...pageData];
        
        // Check if we have more pages
        const totalCount = skusResult?.Data?.TotalCount || skusResult?.TotalCount || 0;
        hasMoreData = pageData.length === pageSize && allSkus.length < totalCount;
        
        if (hasMoreData) {
          currentPage++;
          console.log(`Loaded ${allSkus.length} SKUs so far, fetching page ${currentPage}...`);
        } else {
          console.log(`Finished loading ${allSkus.length} total SKUs`);
        }
      }
      
      // Filter SKUs that match our UIDs
      const filteredSkus = allSkus.filter((sku: any) => 
        skuUIDs.includes(sku.SKUUID)
      );

      // Convert to FinalProduct format
      const products = filteredSkus.map((sku: any) => {
        return {
          UID: sku.SKUUID || sku.UID,
          Code: sku.SKUCode || "N/A",
          Name: sku.SKULongName || "Unknown Product",
          GroupName: groupName,
          GroupTypeName: "Product",
          MRP: sku.MRP || 0,
          IsActive: sku.IsActive !== false
        };
      });

      return products;
    } catch (error) {
      console.error("Error loading products based on selection:", error);
      return [];
    }
  };

  // Load final products when hierarchy selections change
  useEffect(() => {
    if (selectionType === "hierarchy") {
      loadFinalProducts();
    }
  }, [value.hierarchySelections, selectionType]);


  const loadGroupsForType = async (typeUID: string, parentCode?: string) => {
    try {
      console.log("🔍 Loading groups for type:", typeUID, "with parent code:", parentCode);
      
      if (parentCode) {
        // Load child groups using the centralized service (using parent Code)
        const childGroups = await hierarchyService.getChildSKUGroups(parentCode);
        console.log("📋 Child groups found:", childGroups.length);
        console.log("📋 Child groups:", childGroups.map(g => ({ Code: g.Code, Name: g.Name, Type: g.SKUGroupTypeUID })));
        
        // Filter by type if needed (child groups should already be of the right type, but let's be safe)
        const filteredGroups = childGroups.filter(group => 
          group.SKUGroupTypeUID === typeUID
        );
        
        console.log("📋 Filtered child groups:", filteredGroups.length);
        
        return filteredGroups.map(group => ({
          UID: group.UID,
          Code: group.Code,
          Name: group.Name,
          SKUGroupTypeUID: group.SKUGroupTypeUID,
          ParentUID: group.ParentUID,
          ItemLevel: group.ItemLevel,
          SupplierOrgUID: group.SupplierOrgUID
        }));
      } else {
        // Load top-level groups for this type using the centralized service
        const hierarchyOptions = await hierarchyService.getHierarchyOptionsForType(typeUID);
        console.log("📋 Top-level options found:", hierarchyOptions.length);
        
        // Convert HierarchyOption to SKUGroup format
        return hierarchyOptions.map(option => ({
          UID: option.code, // Using code as UID for consistency
          Code: option.code,
          Name: option.value,
          SKUGroupTypeUID: typeUID,
          ParentUID: '',
          ItemLevel: 1, // Assuming top level
          SupplierOrgUID: ''
        }));
      }
    } catch (error) {
      console.error("Error loading groups:", error);
      return [];
    }
  };

  // Load products for Specific Products tab with lazy loading
  const loadSpecificProducts = async (page: number = 1, append: boolean = false) => {
    if (loadingSpecificProducts) return;
    
    try {
      setLoadingSpecificProducts(true);
      const pageSize = 10; // Load only 10 products at a time for faster response
      
      const response = await fetch(
        `${API_CONFIG.baseURL}/SKU/SelectAllSKUDetailsWebView`,
        {
          method: "POST",
          headers: getCommonHeaders(),
          body: JSON.stringify({
            PageNumber: page,
            PageSize: pageSize,
            FilterCriterias: [],
            SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
            IsCountRequired: page === 1
          })
        }
      );
      
      if (response.ok) {
        const result = await response.json();
        
        // Extract products from response
        let items = [];
        if (result?.PagedData) {
          items = result.PagedData;
        } else if (result?.Data?.PagedData) {
          items = result.Data.PagedData;
        } else if (result?.Data && Array.isArray(result.Data)) {
          items = result.Data;
        }
        
        // Get total count if available
        if (page === 1 && result?.Data?.TotalCount) {
          setTotalProductCount(result.Data.TotalCount);
          console.log(`Total products available: ${result.Data.TotalCount}`);
        }
        
        // Convert to FinalProduct format
        const products = items.map((item: any) => ({
          UID: item.SKUUID || item.UID,
          Code: item.SKUCode || item.Code,
          Name: item.SKULongName || item.Name || item.SKUName,
          hierarchyPath: item.L1 && item.L2 ? `${item.L1} > ${item.L2}` : ""
        }));
        
        if (append) {
          setAllProducts(prev => [...prev, ...products]);
        } else {
          setAllProducts(products);
        }
        
        // Check if there are more products to load
        setHasMoreSpecificProducts(items.length === pageSize);
        
        console.log(`Loaded ${products.length} products (Page ${page})`);
      }
    } catch (error) {
      console.error("Failed to load products:", error);
    } finally {
      setLoadingSpecificProducts(false);
    }
  };
  
  // Load more products when scrolling
  const loadMoreSpecificProducts = () => {
    if (!loadingSpecificProducts && hasMoreSpecificProducts) {
      const nextPage = specificProductsPage + 1;
      setSpecificProductsPage(nextPage);
      loadSpecificProducts(nextPage, true);
    }
  };

  const searchProducts = async (searchTerm: string) => {
    if (!searchTerm || searchTerm.length < 2) return;

    setIsSearching(true);
    try {
      // Use SelectAllSKUDetailsWebView API for searching products
      const response = await fetch(
        `${API_CONFIG.baseURL}/SKU/SelectAllSKUDetailsWebView`,
        {
          method: "POST",
          headers: getCommonHeaders(),
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 500, // Load first 500 search results
            FilterCriterias: searchTerm ? [
              {
                Name: "skucodeandname",
                Value: searchTerm
              }
            ] : [],
            SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
            IsCountRequired: true
          })
        }
      );

      if (response.ok) {
        const result = await response.json();
        console.log("Search Products API Response:", result);
        
        // Handle multiple possible API response structures
        let items = [];
        if (result?.PagedData) {
          items = result.PagedData;
        } else if (result?.Data?.PagedData) {
          items = result.Data.PagedData;
        } else if (result?.Data && Array.isArray(result.Data)) {
          items = result.Data;
        } else if (Array.isArray(result)) {
          items = result;
        }
        
        // Map to FinalProduct format
        const filtered = items
          .slice(0, 50) // Limit to 50 results
          .map((item: any) => {
            return {
              UID: item.SKUUID || item.UID,
              Code: item.SKUCode || item.Code,
              Name: item.SKULongName || item.Name,
              MRP: item.MRP || 0,
              IsActive: item.IsActive !== false
            };
          });

        setSearchResults(filtered);
      }
    } catch (error) {
      console.error("Error searching products:", error);
    } finally {
      setIsSearching(false);
    }
  };

  const handleSelectionTypeChange = (type: "all" | "hierarchy" | "specific") => {
    setSelectionType(type);
    onChange({
      ...value,
      selectionType: type,
      ...(type === "all" && {
        hierarchySelections: {},
        specificProducts: [],
        excludedProducts: []
      })
    });
  };

  // Handle attribute selection exactly like SKU management
  const handleAttributeChange = async (index: number, value: string) => {
    console.log("🔄 Attribute changed at index:", index, "value:", value);
    
    const updated = [...selectedAttributes];
    updated[index].code = value;
    
    // Find the selected option to get its details
    const selectedOption = hierarchyOptions[updated[index].type]?.find(opt => opt.code === value);
    if (selectedOption) {
      updated[index].value = selectedOption.value;
      updated[index].uid = value; // Using code as UID since backend uses it (exactly like SKU management)
    }
    
    // Clear all subsequent selections (exactly like SKU management)
    for (let i = index + 1; i < updated.length; i++) {
      updated[i].code = '';
      updated[i].value = '';
      updated[i].uid = '';
    }
    
    setSelectedAttributes(updated);
    
    // Clear options for all subsequent levels
    setHierarchyOptions(prev => {
      const newOptions = { ...prev };
      for (let i = index + 1; i < updated.length; i++) {
        const type = hierarchyTypes[i];
        if (type) {
          delete newOptions[type.Name];
        }
      }
      return newOptions;
    });
    
    // Update the hierarchySelections for the form
    const newSelections: { [key: string]: string[] } = {};
    updated.forEach((attr, i) => {
      const type = hierarchyTypes[i];
      if (type && attr.code) {
        newSelections[type.UID] = [attr.code];
      } else if (type) {
        newSelections[type.UID] = [];
      }
    });
    
    onChange({
      ...value,
      hierarchySelections: newSelections
    });
    
    // Load options for the next level if exists (exactly like SKU management)
    if (index < hierarchyTypes.length - 1 && value) {
      await loadChildOptions(value, index + 1);
    }
  };

  const handleSpecificProductToggle = (product: FinalProduct) => {
    const productInfo = {
      UID: product.UID,
      Code: product.Code,
      Name: product.Name
    };

    const isSelected = value.specificProducts.some(
      p => (typeof p === 'string' ? p : p.UID) === product.UID
    );

    if (isSelected) {
      onChange({
        ...value,
        specificProducts: value.specificProducts.filter(
          p => (typeof p === 'string' ? p : p.UID) !== product.UID
        )
      });
    } else {
      onChange({
        ...value,
        specificProducts: [...value.specificProducts, productInfo]
      });
    }
  };

  return (
    <div className="space-y-4">
      {/* Selection Type Tabs */}
      <Tabs value={selectionType} onValueChange={(v) => handleSelectionTypeChange(v as any)}>
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="all" disabled={disabled}>
            All Products
          </TabsTrigger>
          <TabsTrigger value="hierarchy" disabled={disabled}>
            By Hierarchy
          </TabsTrigger>
          <TabsTrigger value="specific" disabled={disabled}>
            Specific Products
          </TabsTrigger>
        </TabsList>

        {/* All Products Tab */}
        <TabsContent value="all" className="mt-4">
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center justify-center py-8 text-gray-500">
                <Package className="w-8 h-8 mr-3" />
                <span>All products will be included in this promotion</span>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Hierarchy Tab */}
        <TabsContent value="hierarchy" className="mt-4">
          <Card>
            <CardContent className="p-4">
              {(() => {
                console.log("🎨 Render state - isLoadingHierarchy:", isLoadingHierarchy);
                console.log("🎨 Render state - hierarchyData.length:", hierarchyData?.length || 0);
                console.log("🎨 Render state - cascadingLevels.length:", cascadingLevels?.length || 0);
                return null;
              })()}
              {isLoadingHierarchy ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="w-6 h-6 animate-spin mr-2" />
                  <span>Loading product hierarchy...</span>
                </div>
              ) : !hierarchyData || hierarchyData.length === 0 ? (
                <div className="flex items-center justify-center py-8 text-gray-500">
                  <Package className="w-8 h-8 mr-3" />
                  <span>No product hierarchy available. Please configure SKU Group Types first.</span>
                </div>
              ) : selectedAttributes.length === 0 ? (
                <div className="flex items-center justify-center py-8 text-gray-500">
                  <Loader2 className="w-6 h-6 animate-spin mr-2" />
                  <span>Initializing hierarchy...</span>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {selectedAttributes.map((attr, index) => {
                    const isFirstLevel = index === 0;
                    const parentSelected = index === 0 || (selectedAttributes[index - 1]?.code && selectedAttributes[index - 1]?.value);
                    const isEnabled = isFirstLevel || parentSelected;
                    const shouldShow = index === 0 || selectedAttributes[index - 1]?.code;
                    const hasOptionsLoaded = hierarchyOptions[attr.type] !== undefined;
                    
                    if (!shouldShow) return null;
                    
                    // Load child options when parent is selected (exactly like SKU management)
                    const loadChildOptions = async (parentUID: string, childIndex: number) => {
                      try {
                        setLoadingChildOptions(prev => ({ ...prev, [childIndex]: true }));
                        
                        const childGroups = await hierarchyService.getChildSKUGroups(parentUID);
                        const childType = hierarchyTypes[childIndex];
                        
                        if (childType && childGroups.length > 0) {
                          const childOptions = childGroups.map(group => ({
                            code: group.Code,
                            value: group.Name,
                            type: childType.Name
                          }));
                          
                          setHierarchyOptions(prev => ({
                            ...prev,
                            [childType.Name]: childOptions
                          }));
                          
                          console.log("✅ Manual loaded", childOptions.length, "child options for", childType.Name);
                        } else if (childType) {
                          // Clear options if no children found
                          setHierarchyOptions(prev => ({
                            ...prev,
                            [childType.Name]: []
                          }));
                        }
                      } catch (error) {
                        console.error('Failed to load child options:', error);
                      } finally {
                        setLoadingChildOptions(prev => ({ ...prev, [childIndex]: false }));
                      }
                    };
                    
                    return (
                      <div key={`${attr.type}-${index}`} className="space-y-2">
                        <Label className="font-medium text-sm">
                          {attr.type} → L{index + 1}
                        </Label>
                        <Select
                          value={attr.code}
                          disabled={!isEnabled || loadingChildOptions[index]}
                          onOpenChange={(open) => {
                            // Load options when dropdown opens if parent is selected but options not loaded
                            if (open && !isFirstLevel && parentSelected && !hasOptionsLoaded && !loadingChildOptions[index]) {
                              const parentCode = selectedAttributes[index - 1].code;
                              if (parentCode) {
                                loadChildOptions(parentCode, index);
                              }
                            }
                          }}
                          onValueChange={async (value) => {
                            await handleAttributeChange(index, value);
                          }}
                        >
                          <SelectTrigger>
                            <SelectValue placeholder={
                              loadingChildOptions[index] 
                                ? "Loading..." 
                                : `Select ${attr.type.toLowerCase()}`
                            } />
                          </SelectTrigger>
                          <SelectContent>
                            {loadingChildOptions[index] ? (
                              <SelectItem value="loading" disabled>
                                Loading options...
                              </SelectItem>
                            ) : hierarchyOptions[attr.type]?.length > 0 ? (
                              hierarchyOptions[attr.type].map((option) => (
                                <SelectItem key={option.code} value={option.code}>
                                  {option.code} - {option.value}
                                </SelectItem>
                              ))
                            ) : (
                              <SelectItem value="none" disabled>
                                {index === 0 
                                  ? "No options available" 
                                  : !parentSelected
                                    ? "Please select parent first"
                                    : "No options available for selected parent"}
                              </SelectItem>
                            )}
                          </SelectContent>
                        </Select>
                      </div>
                    );
                  })}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Specific Products Tab */}
        <TabsContent value="specific" className="mt-4">
          <Card>
            <CardContent className="p-4">
              <div className="space-y-4">
                {/* Search Bar */}
                <div className="flex gap-2">
                  <Input
                    type="text"
                    placeholder="Search products by name or code..."
                    value={specificSearchTerm}
                    onChange={(e) => setSpecificSearchTerm(e.target.value)}
                    onKeyPress={(e) => {
                      if (e.key === 'Enter') {
                        searchProducts(specificSearchTerm);
                      }
                    }}
                    disabled={disabled}
                  />
                  <Button
                    onClick={() => searchProducts(specificSearchTerm)}
                    disabled={disabled || isSearching}
                  >
                    {isSearching ? (
                      <Loader2 className="w-4 h-4 animate-spin" />
                    ) : (
                      <Search className="w-4 h-4" />
                    )}
                  </Button>
                </div>

                {/* Selected Products */}
                {value.specificProducts.length > 0 && (
                  <div>
                    <h4 className="text-sm font-medium mb-2">
                      Selected Products ({value.specificProducts.length})
                    </h4>
                    <div className="flex flex-wrap gap-2">
                      {value.specificProducts.map(product => {
                        const prod = typeof product === 'string' 
                          ? { UID: product, Name: product, Code: '' }
                          : product;
                        return (
                          <Badge key={prod.UID} variant="secondary">
                            {prod.Name}
                            <button
                              onClick={() => handleSpecificProductToggle(prod as FinalProduct)}
                              className="ml-2 text-red-500 hover:text-red-700"
                            >
                              <X className="w-3 h-3" />
                            </button>
                          </Badge>
                        );
                      })}
                    </div>
                  </div>
                )}

                {/* Search Results */}
                {searchResults.length > 0 && (
                  <div>
                    <h4 className="text-sm font-medium mb-2">
                      Search Results ({searchResults.length})
                    </h4>
                    <ScrollArea className="h-64">
                      <div className="space-y-2">
                        {searchResults.map(product => {
                          const isSelected = value.specificProducts.some(
                            p => (typeof p === 'string' ? p : p.UID) === product.UID
                          );
                          return (
                            <div
                              key={product.UID}
                              className={`flex items-center justify-between p-2 border rounded hover:bg-gray-50 ${
                                isSelected ? 'bg-blue-50 border-blue-300' : ''
                              }`}
                            >
                              <div>
                                <div className="font-medium text-sm">{product.Name}</div>
                                <div className="text-xs text-gray-500">
                                  Code: {product.Code} | MRP: ₹{product.MRP}
                                </div>
                              </div>
                              <Button
                                size="sm"
                                variant={isSelected ? "default" : "outline"}
                                onClick={() => handleSpecificProductToggle(product)}
                                disabled={disabled}
                              >
                                {isSelected ? (
                                  <>
                                    <Check className="w-4 h-4 mr-1" />
                                    Selected
                                  </>
                                ) : (
                                  'Select'
                                )}
                              </Button>
                            </div>
                          );
                        })}
                      </div>
                    </ScrollArea>
                  </div>
                )}

                {/* All Products List (when not searching) */}
                {!specificSearchTerm && allProducts.length > 0 && (
                  <div>
                    <h4 className="text-sm font-medium mb-2">
                      Available Products ({totalProductCount > 0 ? `${allProducts.length} of ${totalProductCount}` : allProducts.length})
                    </h4>
                    <ScrollArea 
                      className="h-64"
                      onScroll={(e) => {
                        const target = e.target as HTMLDivElement;
                        const scrollPercentage = (target.scrollTop / (target.scrollHeight - target.clientHeight)) * 100;
                        // Load more when user has scrolled 80% down
                        if (scrollPercentage > 80 && !loadingSpecificProducts && hasMoreSpecificProducts) {
                          loadMoreSpecificProducts();
                        }
                      }}
                    >
                      <div className="space-y-2">
                        {allProducts.map(product => {
                          const isSelected = value.specificProducts.some(
                            p => (typeof p === 'string' ? p : p.UID) === product.UID
                          );
                          return (
                            <div
                              key={product.UID}
                              className={`flex items-center justify-between p-2 border rounded hover:bg-gray-50 cursor-pointer ${
                                isSelected ? 'bg-blue-50 border-blue-300' : ''
                              }`}
                              onClick={() => handleSpecificProductToggle(product)}
                            >
                              <div>
                                <div className="font-medium text-sm">{product.Name}</div>
                                <div className="text-xs text-gray-500">
                                  Code: {product.Code}
                                  {product.hierarchyPath && ` • ${product.hierarchyPath}`}
                                </div>
                              </div>
                              <Checkbox 
                                checked={isSelected}
                                onCheckedChange={() => handleSpecificProductToggle(product)}
                                onClick={(e) => e.stopPropagation()}
                              />
                            </div>
                          );
                        })}
                        {loadingSpecificProducts && (
                          <div className="flex items-center justify-center py-2">
                            <Loader2 className="w-3 h-3 animate-spin mr-1" />
                            <span className="text-xs text-gray-500">Loading...</span>
                          </div>
                        )}
                      </div>
                    </ScrollArea>
                  </div>
                )}
                
                {/* No Results */}
                {specificSearchTerm && !isSearching && searchResults.length === 0 && (
                  <div className="text-center py-8 text-gray-500">
                    <Package className="w-12 h-12 mx-auto mb-2" />
                    <p>No products found</p>
                  </div>
                )}
                
                {/* Initial Loading - Show skeleton loader */}
                {!specificSearchTerm && allProducts.length === 0 && loadingSpecificProducts && (
                  <div className="space-y-2">
                    <h4 className="text-sm font-medium mb-2">Loading Products...</h4>
                    {[...Array(5)].map((_, i) => (
                      <div key={i} className="flex items-center justify-between p-2 border rounded animate-pulse">
                        <div className="flex-1">
                          <div className="h-4 bg-gray-200 rounded w-3/4 mb-1"></div>
                          <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                        </div>
                        <div className="h-4 w-4 bg-gray-200 rounded"></div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Summary */}
      <div className="text-sm text-gray-600">
        {selectionType === "all" && "All products will be included"}
        {selectionType === "hierarchy" && (
          <>
            {selectedAttributes.filter(attr => attr.code && attr.value).length} attributes selected
          </>
        )}
        {selectionType === "specific" && (
          <>{value.specificProducts.length} products selected</>
        )}
      </div>
    </div>
  );
}