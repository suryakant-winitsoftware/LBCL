"use client";

import { useState, useEffect, useCallback, useMemo } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import {
  hierarchyService,
  SKUGroupType,
  SKUGroup,
  HierarchyOption
} from "@/services/hierarchy.service";
import { skuService, SKUToGroupMapping } from "@/services/sku/sku.service";
import { useToast } from "@/components/ui/use-toast";
import { apiService } from "@/services/api";
import { 
  Search, 
  ChevronDown, 
  X, 
  Package, 
  Filter,
  RefreshCw,
  CheckSquare,
  Square,
  ChevronRight
} from "lucide-react";

interface SKUProduct {
  SKUUID: string;
  SKUCode: string;
  SKULongName: string;
  IsActive: boolean;
  ParentUID?: string;
  ProductCategoryId?: string;
  L1?: string;
  L2?: string;
  L3?: string;
  selected?: boolean;
}

interface SelectedHierarchy {
  level: number;
  levelName: string;
  uid: string;
  code: string;
  name: string;
}

interface ProductAttributesWithHierarchyFilterProps {
  /**
   * Callback when product selections change
   */
  onProductsChange: (products: SKUProduct[]) => void;

  /**
   * Enable multi-select for products
   */
  multiSelectProducts?: boolean;

  /**
   * Show selected count badge
   */
  showSelectedCount?: boolean;

  /**
   * Initial selected products (UIDs)
   */
  initialSelectedProducts?: string[];

  /**
   * Enable search in products
   */
  enableProductSearch?: boolean;

  /**
   * Page size for product loading
   */
  productPageSize?: number;

  /**
   * Custom title for the component
   */
  title?: string;

  /**
   * Custom description for the component
   */
  description?: string;
}

export default function ProductAttributesWithHierarchyFilter({
  onProductsChange,
  multiSelectProducts = true,
  showSelectedCount = true,
  initialSelectedProducts = [],
  enableProductSearch = true,
  productPageSize = 50,
  title = "Product Selection with Hierarchy",
  description = "Select products by filtering through the SKU group hierarchy"
}: ProductAttributesWithHierarchyFilterProps) {
  const { toast } = useToast();

  // Hierarchy state
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([]);
  const [hierarchyOptions, setHierarchyOptions] = useState<Record<string, HierarchyOption[]>>({});
  const [selectedHierarchy, setSelectedHierarchy] = useState<Map<number, SelectedHierarchy>>(new Map());
  const [loadingHierarchy, setLoadingHierarchy] = useState<Record<number, boolean>>({});

  // Products state
  const [products, setProducts] = useState<SKUProduct[]>([]);
  const [selectedProducts, setSelectedProducts] = useState<Set<string>>(new Set(initialSelectedProducts));
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [productSearchTerm, setProductSearchTerm] = useState("");
  const [totalProductCount, setTotalProductCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);

  // Initialize hierarchy types on mount
  useEffect(() => {
    const initializeHierarchy = async () => {
      try {
        const types = await hierarchyService.getHierarchyTypes();
        console.log("Loaded hierarchy types:", types);
        setHierarchyTypes(types.sort((a, b) => a.ItemLevel - b.ItemLevel));
        
        // Load root level options
        if (types.length > 0) {
          const rootType = types[0];
          setLoadingHierarchy(prev => ({ ...prev, 1: true }));
          const options = await hierarchyService.getHierarchyOptionsForType(rootType.UID);
          setHierarchyOptions({ [rootType.UID]: options });
          setLoadingHierarchy(prev => ({ ...prev, 1: false }));
        }
      } catch (error) {
        console.error("Failed to initialize hierarchy:", error);
        toast({
          title: "Error",
          description: "Failed to load hierarchy structure",
          variant: "destructive"
        });
      }
    };

    initializeHierarchy();
  }, []);

  // Handle hierarchy selection
  const handleHierarchySelect = async (level: number, value: string) => {
    if (!value) return;

    const type = hierarchyTypes[level - 1];
    const options = hierarchyOptions[type.UID] || [];
    const selected = options.find(opt => (opt.uid || opt.code) === value);

    if (!selected) return;

    // Update selected hierarchy
    const newSelection = new Map(selectedHierarchy);
    
    // Clear selections below this level
    for (let i = level + 1; i <= hierarchyTypes.length; i++) {
      newSelection.delete(i);
    }

    // Set current selection
    newSelection.set(level, {
      level,
      levelName: type.Name,
      uid: selected.uid || selected.code,
      code: selected.code,
      name: selected.value
    });

    setSelectedHierarchy(newSelection);

    // Load next level options if not the last level
    if (level < hierarchyTypes.length) {
      const nextType = hierarchyTypes[level];
      setLoadingHierarchy(prev => ({ ...prev, [level + 1]: true }));
      
      try {
        // Get child SKU groups based on parent UID
        const childGroups = await hierarchyService.getChildSKUGroups(selected.uid || selected.code);
        
        // Convert SKU groups to hierarchy options format
        const nextOptions: HierarchyOption[] = childGroups
          .filter(group => group.SKUGroupTypeUID === nextType.UID)
          .map(group => ({
            code: group.Code,
            value: group.Name,
            type: nextType.Name,
            uid: group.UID
          }));
        
        setHierarchyOptions(prev => ({
          ...prev,
          [nextType.UID]: nextOptions
        }));
      } catch (error) {
        console.error("Failed to load next level options:", error);
        toast({
          title: "Error",
          description: "Failed to load options",
          variant: "destructive"
        });
      } finally {
        setLoadingHierarchy(prev => ({ ...prev, [level + 1]: false }));
      }
    }

    // Load products based on selection
    await loadProducts(newSelection);
  };

  // Load products based on hierarchy selection
  const loadProducts = async (hierarchy: Map<number, SelectedHierarchy>, page: number = 1, search?: string) => {
    setLoadingProducts(true);
    
    try {
      // Build filter criteria
      const filterCriterias: any[] = [];
      
      // Add search filter if provided
      if (search || productSearchTerm) {
        filterCriterias.push({
          Name: "skucodeandname",
          Value: search || productSearchTerm
        });
      }

      // Add active filter
      filterCriterias.push({
        Name: "IsActive",
        Value: true
      });

      // If hierarchy is selected, add server-side filters
      if (hierarchy.size > 0) {
        // Get the deepest selected hierarchy level
        const maxLevel = Math.max(...Array.from(hierarchy.keys()));
        const selectedItem = hierarchy.get(maxLevel);
        
        if (selectedItem) {
          console.log("Selected hierarchy item:", {
            level: maxLevel,
            code: selectedItem.code,
            uid: selectedItem.uid,
            name: selectedItem.name,
            levelName: selectedItem.levelName
          });
          
          // Check what type of group is selected
          const selectedType = hierarchyTypes[maxLevel - 1];
          console.log("Selected type:", selectedType?.Name);
          
          // If a Brand is selected, filter by ParentUID directly
          if (selectedType?.Name === "Brand" || selectedType?.Code === "Brand") {
            console.log("Brand selected, adding ParentUID filter:", selectedItem.uid);
            filterCriterias.push({
              Name: "ParentUID",
              Value: selectedItem.uid
            });
          } else {
            // For non-brand selections, get all child brands and filter by multiple ParentUIDs
            console.log("Non-brand selected, getting all child brands...");
            
            const allBrandUIDs = new Set<string>();
            
            // Recursively get all descendant brands
            const getDescendantBrands = async (parentUID: string) => {
              const childGroups = await hierarchyService.getChildSKUGroups(parentUID);
              console.log(`Found ${childGroups.length} children for ${parentUID}`);
              
              for (const child of childGroups) {
                // Check if this is a brand
                const childType = hierarchyTypes.find(t => t.UID === child.SKUGroupTypeUID);
                if (childType?.Name === "Brand" || childType?.Code === "Brand") {
                  allBrandUIDs.add(child.UID);
                  console.log("Found brand:", child.Code, child.Name, "UID:", child.UID);
                } else {
                  // Recursively get children if not a brand
                  await getDescendantBrands(child.UID);
                }
              }
            };
            
            await getDescendantBrands(selectedItem.uid);
            console.log("All brand UIDs found:", Array.from(allBrandUIDs));
            
            // Add filter for multiple ParentUIDs
            if (allBrandUIDs.size > 0) {
              const brandUIDsArray = Array.from(allBrandUIDs);
              console.log("Adding ParentUIDs filter with brands:", brandUIDsArray);
              filterCriterias.push({
                Name: "ParentUIDs",
                Value: JSON.stringify(brandUIDsArray)
              });
            } else {
              console.log("WARNING: No brands found for the selected hierarchy!");
            }
          }
        }
      }

      const request = {
        PageNumber: page,
        PageSize: productPageSize,
        IsCountRequired: page === 1,
        FilterCriterias: filterCriterias,
        SortCriterias: []
      };

      console.log("Loading products with server-side filtering:");
      console.log("Request details:", JSON.stringify(request, null, 2));
      console.log("Filter criterias:", filterCriterias);

      const response = await skuService.getAllSKUs(request);
      console.log("Raw API Response:", response);
      
      // The SKU service returns a wrapped response with success and data properties
      // The actual API response is in response.data if success is true
      let responseData;
      let isSuccess = false;
      
      if (response?.success) {
        // Service call was successful
        isSuccess = true;
        // The actual API response is in response.data
        responseData = response.data;
      } else if (response?.IsSuccess) {
        // Direct API response format
        isSuccess = true;
        responseData = response.Data;
      }
      
      console.log("API Response parsed:", {
        success: isSuccess,
        hasData: !!responseData,
        responseDataKeys: responseData ? Object.keys(responseData) : [],
        hasPagedData: responseData?.PagedData !== undefined,
        count: responseData?.PagedData?.length || 0,
        totalCount: responseData?.TotalCount
      });

      if (isSuccess && responseData && responseData.PagedData) {
        const productData = responseData.PagedData;
        console.log("Products loaded:", productData.length);
        if (productData.length > 0) {
          console.log("Sample product:", productData[0]);
        }
        
        // Map products to our format
        const mappedProducts: SKUProduct[] = productData.map((p: any) => ({
          SKUUID: p.SKUUID || p.UID,
          SKUCode: p.SKUCode || p.Code,
          SKULongName: p.SKULongName || p.LongName || p.Name,
          IsActive: p.IsActive !== false,
          ParentUID: p.ParentUID,
          ProductCategoryId: p.ProductCategoryId,
          L1: p.L1,
          L2: p.L2,
          L3: p.L3,
          selected: selectedProducts.has(p.SKUUID || p.UID)
        }));

        if (page === 1) {
          console.log("Setting products - Before:", products.length);
          console.log("Setting products - New mapped products:", mappedProducts);
          setProducts(mappedProducts);
          setTotalProductCount(responseData.TotalCount || mappedProducts.length);
          console.log(`Setting ${mappedProducts.length} products in state, TotalCount: ${responseData.TotalCount}`);
          // Add a timeout to check if state was actually updated
          setTimeout(() => {
            console.log("After state update check - products length should be:", mappedProducts.length);
          }, 100);
        } else {
          setProducts(prev => {
            const combined = [...prev, ...mappedProducts];
            console.log(`Appending ${mappedProducts.length} products to existing ${prev.length}, total will be ${combined.length}`);
            return combined;
          });
        }

        setCurrentPage(page);
        console.log(`Final: Set ${mappedProducts.length} products to display`);
      } else {
        console.log("No products in API response or request failed");
        if (page === 1) {
          setProducts([]);
          setTotalProductCount(0);
        }
      }
    } catch (error) {
      console.error("Error loading products:", error);
      toast({
        title: "Error",
        description: "Failed to load products",
        variant: "destructive"
      });
    } finally {
      setLoadingProducts(false);
    }
  };

  // Handle product selection
  const handleProductSelect = (productUID: string, checked: boolean) => {
    const newSelection = new Set(selectedProducts);
    
    if (checked) {
      newSelection.add(productUID);
    } else {
      newSelection.delete(productUID);
    }

    setSelectedProducts(newSelection);

    // Update products with selection state
    const updatedProducts = products.map(p => ({
      ...p,
      selected: newSelection.has(p.SKUUID)
    }));

    setProducts(updatedProducts);

    // Notify parent
    const selectedProductList = updatedProducts.filter(p => p.selected);
    onProductsChange(selectedProductList);
  };

  // Handle select all/none
  const handleSelectAll = (selectAll: boolean) => {
    const newSelection = new Set(selectedProducts);
    
    products.forEach(product => {
      if (selectAll) {
        newSelection.add(product.SKUUID);
      } else {
        newSelection.delete(product.SKUUID);
      }
    });

    setSelectedProducts(newSelection);

    // Update products with selection state
    const updatedProducts = products.map(p => ({
      ...p,
      selected: newSelection.has(p.SKUUID)
    }));

    setProducts(updatedProducts);

    // Notify parent
    const selectedProductList = updatedProducts.filter(p => p.selected);
    onProductsChange(selectedProductList);
  };

  // Handle search
  const handleSearch = useCallback((search: string) => {
    setProductSearchTerm(search);
    setCurrentPage(1);
    loadProducts(selectedHierarchy, 1, search);
  }, [selectedHierarchy]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      if (productSearchTerm !== "") {
        handleSearch(productSearchTerm);
      }
    }, 500);

    return () => clearTimeout(timer);
  }, [productSearchTerm]);

  // Clear all selections
  const clearAll = () => {
    setSelectedHierarchy(new Map());
    setSelectedProducts(new Set());
    setProducts([]);
    setProductSearchTerm("");
    setCurrentPage(1);
    setTotalProductCount(0);
    
    // Clear hierarchy options except root
    if (hierarchyTypes.length > 0) {
      const rootType = hierarchyTypes[0];
      const rootOptions = hierarchyOptions[rootType.UID] || [];
      setHierarchyOptions({ [rootType.UID]: rootOptions });
    }

    onProductsChange([]);
  };

  // Filtered products based on search
  const filteredProducts = useMemo(() => {
    if (!enableProductSearch || !productSearchTerm) return products;
    
    const searchLower = productSearchTerm.toLowerCase();
    return products.filter(p => 
      p.SKUCode.toLowerCase().includes(searchLower) ||
      p.SKULongName.toLowerCase().includes(searchLower)
    );
  }, [products, productSearchTerm, enableProductSearch]);

  // Debug: Monitor products state changes
  useEffect(() => {
    console.log("🔄 Products state updated:", {
      length: products.length,
      firstProduct: products[0],
      filteredLength: filteredProducts.length
    });
  }, [products, filteredProducts]);

  const selectedCount = selectedProducts.size;

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>{title}</CardTitle>
          <CardDescription>{description}</CardDescription>
          {showSelectedCount && selectedCount > 0 && (
            <Badge variant="secondary" className="w-fit">
              {selectedCount} product{selectedCount !== 1 ? "s" : ""} selected
            </Badge>
          )}
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Hierarchy Selection */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label className="text-base font-medium">SKU Group Hierarchy</Label>
              {selectedHierarchy.size > 0 && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={clearAll}
                  className="text-xs"
                >
                  <X className="h-3 w-3 mr-1" />
                  Clear All
                </Button>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {hierarchyTypes.map((type, index) => {
                const level = index + 1;
                const isDisabled = level > 1 && !selectedHierarchy.has(level - 1);
                const options = hierarchyOptions[type.UID] || [];
                const selected = selectedHierarchy.get(level);

                return (
                  <div key={type.UID} className="space-y-2">
                    <Label className="text-sm font-medium">
                      {type.Name}
                      {selected && (
                        <Badge variant="outline" className="ml-2 text-xs">
                          {selected.name}
                        </Badge>
                      )}
                    </Label>
                    <Select
                      disabled={isDisabled || loadingHierarchy[level]}
                      value={selected?.uid || ""}
                      onValueChange={(value) => handleHierarchySelect(level, value)}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={`Select ${type.Name}`} />
                      </SelectTrigger>
                      <SelectContent>
                        {loadingHierarchy[level] ? (
                          <div className="p-2">
                            <Skeleton className="h-4 w-20" />
                          </div>
                        ) : options.length > 0 ? (
                          options.map(option => (
                            <SelectItem key={option.uid || option.code} value={option.uid || option.code}>
                              {option.code} - {option.value}
                            </SelectItem>
                          ))
                        ) : (
                          <div className="p-2 text-sm text-muted-foreground">
                            No options available
                          </div>
                        )}
                      </SelectContent>
                    </Select>
                  </div>
                );
              })}
            </div>
            
            {/* Show info about what will be loaded */}
            {selectedHierarchy.size > 0 && (
              <div className="text-sm text-muted-foreground">
                Products will be filtered based on the selected hierarchy attributes.
              </div>
            )}
          </div>

          {/* Products Section */}
          {(() => {
            console.log("Rendering products section - hierarchy size:", selectedHierarchy.size);
            return selectedHierarchy.size > 0;
          })() && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <Label className="text-base font-medium">Products</Label>
                <div className="flex items-center gap-2">
                  {multiSelectProducts && products.length > 0 && (
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleSelectAll(true)}
                        disabled={selectedCount === products.length}
                      >
                        <CheckSquare className="h-3 w-3 mr-1" />
                        Select All
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleSelectAll(false)}
                        disabled={selectedCount === 0}
                      >
                        <Square className="h-3 w-3 mr-1" />
                        Clear
                      </Button>
                    </div>
                  )}
                  {loadingProducts && (
                    <RefreshCw className="h-4 w-4 animate-spin text-muted-foreground" />
                  )}
                </div>
              </div>

              {/* Product Search */}
              {enableProductSearch && (
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search products by code or name..."
                    value={productSearchTerm}
                    onChange={(e) => setProductSearchTerm(e.target.value)}
                    className="pl-10"
                  />
                </div>
              )}

              {/* Products List */}
              <div className="border rounded-lg">
                <ScrollArea className="h-[400px]">
                  {loadingProducts && products.length === 0 ? (
                    <div className="p-4 space-y-2">
                      {Array.from({ length: 5 }).map((_, i) => (
                        <div key={i} className="flex items-center gap-3">
                          <Skeleton className="h-4 w-4" />
                          <Skeleton className="h-4 w-24" />
                          <Skeleton className="h-4 w-48" />
                        </div>
                      ))}
                    </div>
                  ) : (() => {
                    console.log("Checking filteredProducts for display:", {
                      filteredLength: filteredProducts.length,
                      firstFiltered: filteredProducts[0],
                      productsLength: products.length,
                      loadingProducts
                    });
                    return filteredProducts.length > 0;
                  })() ? (
                    <div className="p-2">
                      {filteredProducts.map(product => (
                        <div
                          key={product.SKUUID}
                          className="flex items-center gap-3 p-2 hover:bg-muted/50 rounded-md"
                        >
                          {multiSelectProducts && (
                            <Checkbox
                              checked={product.selected || false}
                              onCheckedChange={(checked) => 
                                handleProductSelect(product.SKUUID, checked as boolean)
                              }
                            />
                          )}
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2">
                              <span className="font-mono text-sm">{product.SKUCode}</span>
                              <span className="text-sm text-muted-foreground truncate">
                                {product.SKULongName}
                              </span>
                            </div>
                            {(product.L1 || product.L2 || product.L3) && (
                              <div className="flex items-center gap-1 mt-1">
                                {product.L1 && (
                                  <Badge variant="outline" className="text-xs">
                                    {product.L1}
                                  </Badge>
                                )}
                                {product.L2 && (
                                  <>
                                    <ChevronRight className="h-3 w-3 text-muted-foreground" />
                                    <Badge variant="outline" className="text-xs">
                                      {product.L2}
                                    </Badge>
                                  </>
                                )}
                                {product.L3 && (
                                  <>
                                    <ChevronRight className="h-3 w-3 text-muted-foreground" />
                                    <Badge variant="outline" className="text-xs">
                                      {product.L3}
                                    </Badge>
                                  </>
                                )}
                              </div>
                            )}
                          </div>
                          {product.IsActive ? (
                            <Badge variant="default" className="text-xs">Active</Badge>
                          ) : (
                            <Badge variant="secondary" className="text-xs">Inactive</Badge>
                          )}
                        </div>
                      ))}

                      {/* Load More */}
                      {products.length < totalProductCount && (
                        <div className="p-4 text-center">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => loadProducts(selectedHierarchy, currentPage + 1)}
                            disabled={loadingProducts}
                          >
                            {loadingProducts ? (
                              <>
                                <RefreshCw className="h-3 w-3 mr-2 animate-spin" />
                                Loading...
                              </>
                            ) : (
                              <>
                                Load More ({products.length} of {totalProductCount})
                              </>
                            )}
                          </Button>
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="p-8 text-center">
                      <Package className="h-12 w-12 mx-auto text-muted-foreground mb-3" />
                      <p className="text-sm font-medium">No products found</p>
                      <p className="text-xs text-muted-foreground mt-1">
                        {productSearchTerm 
                          ? "Try adjusting your search term"
                          : "Select a different hierarchy level"}
                      </p>
                    </div>
                  )}
                </ScrollArea>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}