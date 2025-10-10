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

interface CascadingLevel {
  type: SKUGroupType;
  groups: SKUGroup[];
  selectedGroups: string[];
  isLoading: boolean;
  isExpanded: boolean;
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
  const [cascadingLevels, setCascadingLevels] = useState<CascadingLevel[]>([]);
  const [groupTypes, setGroupTypes] = useState<SKUGroupType[]>([]);
  const [isLoadingTypes, setIsLoadingTypes] = useState(false);
  const [specificSearchTerm, setSpecificSearchTerm] = useState("");
  const [searchResults, setSearchResults] = useState<FinalProduct[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [selectedProducts, setSelectedProducts] = useState<string[]>([]);
  const [showFinalPreview, setShowFinalPreview] = useState(false);
  const [finalProducts, setFinalProducts] = useState<FinalProduct[]>([]);
  const productCache = useRef<{ [groupUID: string]: FinalProduct[] }>({});

  // Use the hook for loading hierarchy
  const { data: hierarchyData, isLoading: isLoadingHierarchy } = useProductHierarchy(orgUid, showEmptyTypes);

  // Initialize cascading levels when hierarchy data loads
  useEffect(() => {
    console.log("🔄 Hierarchy data effect triggered:", hierarchyData?.length || 0);
    console.log("📝 Current hierarchyData:", hierarchyData);
    
    if (hierarchyData && hierarchyData.length > 0) {
      console.log("✅ Setting up cascading levels with", hierarchyData.length, "types");
      setGroupTypes(hierarchyData);
      const initialLevels = hierarchyData.map((type: SKUGroupType) => ({
        type,
        groups: [],
        selectedGroups: value.hierarchySelections?.[type.UID] || [],
        isLoading: false,
        isExpanded: false
      }));
      console.log("🎯 Initial cascading levels:", initialLevels);
      setCascadingLevels(initialLevels);
    } else {
      console.log("❌ No hierarchy data to process");
    }
  }, [hierarchyData, value.hierarchySelections]);

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
      
      // Handle API response structure - the API returns PagedData directly
      const mappings = mappingsResult?.PagedData || mappingsResult?.Data?.PagedData || [];
      
      if (mappings.length === 0) {
        return [];
      }
      
      // Get SKU UIDs from mappings
      const skuUIDs = mappings.map((m: any) => m.SKUUID);
      
      // Fetch all SKUs and filter by UIDs
      const skusResponse = await fetch(
        `${API_CONFIG.baseURL}/SKU/SelectAllSKUDetailsWebView`,
        {
          method: "POST",
          headers: getCommonHeaders(),
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 1000,
            FilterCriterias: [],
            SortCriterias: [],
            IsCountRequired: false
          })
        }
      );

      if (!skusResponse.ok) {
        console.error("Failed to load SKUs");
        return [];
      }

      const skusResult = await skusResponse.json();
      console.log("SKUs API Response:", skusResult);
      
      // Handle API response structure - the API returns PagedData directly
      const allSkus = skusResult?.PagedData || skusResult?.Data?.PagedData || [];
      
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

  const loadGroupsForType = async (typeUID: string, parentUID?: string) => {
    try {
      const FilterCriterias = [
        {
          Field: "SKUGroupTypeUID",
          Value: typeUID,
          Operator: "Equal"
        }
      ];

      if (parentUID) {
        FilterCriterias.push({
          Field: "ParentUID",
          Value: parentUID,
          Operator: "Equal"
        });
      }

      const response = await fetch(
        `${API_CONFIG.baseURL}${API_CONFIG.endpoints.skuGroup.selectAll}`,
        {
          method: "POST",
          headers: getCommonHeaders(),
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 1000,
            SortCriterias: [],
            FilterCriterias,
            IsCountRequired: true
          })
        }
      );

      if (response.ok) {
        const result = await response.json();
        console.log("LoadGroupsForType API Response:", result);
        
        // Handle API response structure - the API returns PagedData directly
        return result?.PagedData || result?.Data?.PagedData || [];
      }
    } catch (error) {
      console.error("Error loading groups:", error);
    }
    return [];
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
            PageSize: 100,
            FilterCriterias: [
              {
                Field: "SKULongName",
                Value: searchTerm,
                Operator: "Contains"
              }
            ],
            SortCriterias: [],
            IsCountRequired: false
          })
        }
      );

      if (response.ok) {
        const result = await response.json();
        console.log("Search Products API Response:", result);
        
        // Handle API response structure - the API returns PagedData directly
        const items = result?.PagedData || result?.Data?.PagedData || [];
        
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

  const handleHierarchySelection = (typeUID: string, groupUID: string, selected: boolean) => {
    const newSelections = { ...value.hierarchySelections };
    
    if (!newSelections[typeUID]) {
      newSelections[typeUID] = [];
    }

    if (selected) {
      newSelections[typeUID] = [...newSelections[typeUID], groupUID];
    } else {
      newSelections[typeUID] = newSelections[typeUID].filter(id => id !== groupUID);
    }

    onChange({
      ...value,
      hierarchySelections: newSelections
    });

    // Update cascading levels
    setCascadingLevels(prev => 
      prev.map(level => 
        level.type.UID === typeUID 
          ? { ...level, selectedGroups: newSelections[typeUID] || [] }
          : level
      )
    );
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
              ) : hierarchyData.length === 0 ? (
                <div className="flex items-center justify-center py-8 text-gray-500">
                  <Package className="w-8 h-8 mr-3" />
                  <span>No product hierarchy available</span>
                </div>
              ) : (
                <div className="space-y-3">
                  {cascadingLevels.map((level, index) => (
                    <div key={level.type.UID} className="border rounded-lg p-3">
                      <button
                        type="button"
                        onClick={() => {
                          // Toggle expansion and load groups if needed
                          const newLevels = [...cascadingLevels];
                          newLevels[index].isExpanded = !newLevels[index].isExpanded;
                          
                          if (newLevels[index].isExpanded && newLevels[index].groups.length === 0) {
                            newLevels[index].isLoading = true;
                            setCascadingLevels(newLevels);
                            
                            // Load groups for this type
                            loadGroupsForType(level.type.UID).then(groups => {
                              setCascadingLevels(prev => 
                                prev.map((l, i) => 
                                  i === index 
                                    ? { ...l, groups: groups || [], isLoading: false }
                                    : l
                                )
                              );
                            });
                          } else {
                            setCascadingLevels(newLevels);
                          }
                        }}
                        className="w-full flex items-center justify-between p-2 hover:bg-gray-50 rounded"
                      >
                        <div className="flex items-center">
                          <span className="font-medium">{level.type.Name}</span>
                          {level.selectedGroups.length > 0 && (
                            <Badge variant="secondary" className="ml-2">
                              {level.selectedGroups.length} selected
                            </Badge>
                          )}
                        </div>
                        {level.isExpanded ? <ChevronUp /> : <ChevronDown />}
                      </button>

                      {level.isExpanded && (
                        <div className="mt-2 pl-4">
                          {level.isLoading ? (
                            <div className="flex items-center py-4">
                              <Loader2 className="w-4 h-4 animate-spin mr-2" />
                              <span className="text-sm">Loading...</span>
                            </div>
                          ) : level.groups.length === 0 ? (
                            <div className="text-sm text-gray-500 py-2">
                              No groups available
                            </div>
                          ) : (
                            <ScrollArea className="h-48">
                              <div className="space-y-2">
                                {level.groups.map(group => (
                                  <label
                                    key={group.UID}
                                    className="flex items-center space-x-2 p-2 hover:bg-gray-50 rounded cursor-pointer"
                                  >
                                    <Checkbox
                                      checked={level.selectedGroups.includes(group.UID)}
                                      onCheckedChange={(checked) => 
                                        handleHierarchySelection(level.type.UID, group.UID, !!checked)
                                      }
                                      disabled={disabled}
                                    />
                                    <span className="text-sm">
                                      {group.Name} ({group.Code})
                                    </span>
                                  </label>
                                ))}
                              </div>
                            </ScrollArea>
                          )}
                        </div>
                      )}
                    </div>
                  ))}
                  
                  {/* Final Products Preview */}
                  {Object.keys(value.hierarchySelections || {}).length > 0 && (
                    <div className="mt-4 border-t pt-4">
                      <button
                        type="button"
                        onClick={() => setShowFinalPreview(!showFinalPreview)}
                        className="w-full flex items-center justify-between p-2 hover:bg-gray-50 rounded"
                      >
                        <div className="flex items-center">
                          <Package className="w-4 h-4 mr-2" />
                          <span className="font-medium">Products Preview</span>
                          {finalProducts.length > 0 && (
                            <Badge variant="secondary" className="ml-2">
                              {finalProducts.length} products
                            </Badge>
                          )}
                        </div>
                        {showFinalPreview ? <ChevronUp /> : <ChevronDown />}
                      </button>
                      
                      {showFinalPreview && (
                        <div className="mt-2 p-3 bg-gray-50 rounded-lg">
                          {isLoadingTypes ? (
                            <div className="flex items-center justify-center py-4">
                              <Loader2 className="w-4 h-4 animate-spin mr-2" />
                              <span className="text-sm">Loading products...</span>
                            </div>
                          ) : finalProducts.length === 0 ? (
                            <div className="text-center py-4 text-gray-500">
                              <Package className="w-8 h-8 mx-auto mb-2" />
                              <p className="text-sm">No products found for selected groups</p>
                            </div>
                          ) : (
                            <div>
                              <div className="mb-2 flex items-center justify-between">
                                <span className="text-sm font-medium">Selected Products</span>
                                <span className="text-xs text-gray-500">
                                  {value.selectedFinalProducts?.length || 0} selected
                                </span>
                              </div>
                              <ScrollArea className="h-64">
                                <div className="space-y-2">
                                  {finalProducts.map(product => {
                                    const isSelected = value.selectedFinalProducts?.some(
                                      p => (typeof p === 'string' ? p : p.UID) === product.UID
                                    );
                                    return (
                                      <div
                                        key={product.UID}
                                        className={`flex items-center justify-between p-2 border rounded hover:bg-white ${
                                          isSelected ? 'bg-blue-50 border-blue-300' : 'bg-white'
                                        }`}
                                      >
                                        <div className="flex-1">
                                          <div className="font-medium text-sm">{product.Name}</div>
                                          <div className="text-xs text-gray-500">
                                            Code: {product.Code} | MRP: ₹{product.MRP}
                                          </div>
                                          {product.GroupName && (
                                            <div className="text-xs text-gray-400">
                                              Group: {product.GroupName}
                                            </div>
                                          )}
                                        </div>
                                        <Checkbox
                                          checked={isSelected}
                                          onCheckedChange={(checked) => {
                                            const productInfo = {
                                              UID: product.UID,
                                              Code: product.Code,
                                              Name: product.Name
                                            };
                                            
                                            if (checked) {
                                              onChange({
                                                ...value,
                                                selectedFinalProducts: [
                                                  ...(value.selectedFinalProducts || []),
                                                  productInfo
                                                ]
                                              });
                                            } else {
                                              onChange({
                                                ...value,
                                                selectedFinalProducts: (value.selectedFinalProducts || []).filter(
                                                  p => (typeof p === 'string' ? p : p.UID) !== product.UID
                                                )
                                              });
                                            }
                                          }}
                                          disabled={disabled}
                                        />
                                      </div>
                                    );
                                  })}
                                </div>
                              </ScrollArea>
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  )}
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

                {/* No Results */}
                {specificSearchTerm && !isSearching && searchResults.length === 0 && (
                  <div className="text-center py-8 text-gray-500">
                    <Package className="w-12 h-12 mx-auto mb-2" />
                    <p>No products found</p>
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
            {Object.values(value.hierarchySelections || {}).flat().length} groups selected
          </>
        )}
        {selectionType === "specific" && (
          <>{value.specificProducts.length} products selected</>
        )}
      </div>
    </div>
  );
}