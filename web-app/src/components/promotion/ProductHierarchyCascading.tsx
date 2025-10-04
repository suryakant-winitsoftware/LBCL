"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  hierarchyService,
  SKUGroupType,
  HierarchyOption,
} from "@/services/hierarchy.service";
import { useToast } from "@/components/ui/use-toast";
import {
  Search,
  ChevronDown,
  X,
  CheckSquare,
  Square,
  Package,
} from "lucide-react";
import { getAuthHeaders } from "@/lib/auth-service";
import { cn } from "@/lib/utils";

/**
 * Product Attributes with SKU Loading Component
 *
 * This component extends ProductAttributesMultiDropdown to also load actual SKU products:
 * - Shows hierarchy levels with multi-select dropdowns
 * - After selecting hierarchy, loads actual SKU products
 * - Products are filtered based on parent_uid relationships
 * - Products are loaded from the sku table based on selected sku_group
 * - Fully dynamic with no hardcoded levels or names
 */

interface SelectedAttribute {
  level: number;
  levelName: string;
  code: string;
  value: string;
  uid: string;
  parentCode?: string;
  fieldName?: string;
}

interface SKUProduct {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  ArabicName?: string;
  LongName?: string;
  BaseUOM?: string;
  OuterUOM?: string;
  IsActive: boolean;
  ParentUID?: string;
  DivisionUID?: string;
  ProductCategoryId?: number;
  FilterKeys?: string[];
  OrgUID?: string;
  HSNCode?: string;
  L2?: string;
  L3?: string;
  IsThirdParty?: boolean;
  SupplierOrgUID?: string;
  // Dynamic hierarchy fields (L1, L2, etc.)
  [key: string]: string | number | boolean | string[] | undefined;
}

interface LevelSelections {
  [level: number]: Set<string>;
}

interface ProductHierarchyCascadingProps {
  /**
   * Callback when selections change
   */
  onSelectionsChange: (selections: SelectedAttribute[]) => void;

  /**
   * Callback when SKU products are selected
   */
  onSKUSelectionsChange: (skus: SKUProduct[]) => void;

  /**
   * Optional field name pattern for mapping
   */
  fieldNamePattern?: string;

  /**
   * Show level numbers in UI
   */
  showLevelNumbers?: boolean;

  /**
   * Whether the component is disabled
   */
  disabled?: boolean;

  /**
   * Enable search in dropdowns
   */
  enableSearch?: boolean;

  /**
   * Enable SKU product loading
   */
  enableSKULoading?: boolean;

  /**
   * Show SKU products as a separate section
   */
  showSKUSection?: boolean;

  /**
   * Responsive grid columns
   */
  gridColumns?: {
    default: number;
    md: number;
    lg: number;
  };
}

export default function ProductHierarchyCascading({
  onSelectionsChange,
  onSKUSelectionsChange,
  fieldNamePattern = "L{n}",
  showLevelNumbers = true,
  disabled = false,
  enableSearch = true,
  enableSKULoading = true,
  showSKUSection = true,
  gridColumns = { default: 1, md: 2, lg: 3 },
}: ProductHierarchyCascadingProps) {
  const { toast } = useToast();
  const baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  // Core state - Similar to ProductAttributesMultiDropdown
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([]);
  const [hierarchyOptions, setHierarchyOptions] = useState<
    Record<string, HierarchyOption[]>
  >({});
  const [selectedItems, setSelectedItems] = useState<
    Map<string, SelectedAttribute>
  >(new Map());
  const [loadingState, setLoadingState] = useState<Record<number, boolean>>({});
  const [loadingInitial, setLoadingInitial] = useState(true);
  const [optionsCache, setOptionsCache] = useState<
    Record<string, HierarchyOption[]>
  >({});

  // Track selections per level
  const [levelSelections, setLevelSelections] = useState<LevelSelections>({});

  // Track which dropdowns are open
  const [openDropdowns, setOpenDropdowns] = useState<Record<number, boolean>>(
    {}
  );

  // Search terms per dropdown
  const [searchTerms, setSearchTerms] = useState<Record<number, string>>({});

  // SKU Products state
  const [skuProducts, setSKUProducts] = useState<SKUProduct[]>([]);
  const [selectedSKUs, setSelectedSKUs] = useState<Set<string>>(new Set());
  const [loadingSKUs, setLoadingSKUs] = useState(false);
  const [skuSearchTerm, setSKUSearchTerm] = useState("");
  const [skuDropdownOpen, setSKUDropdownOpen] = useState(false);

  /**
   * Generate dynamic field name based on pattern
   */
  const generateFieldName = (level: number): string => {
    return fieldNamePattern.replace("{n}", level.toString());
  };

  /**
   * Generate level label for display
   */
  const generateLevelLabel = (level: number, typeName: string): string => {
    if (showLevelNumbers) {
      return `${typeName} (${generateFieldName(level)})`;
    }
    return typeName;
  };

  /**
   * Initialize hierarchy types on mount
   */
  useEffect(() => {
    const initializeHierarchy = async () => {
      try {
        setLoadingInitial(true);

        const types = await hierarchyService.getHierarchyTypes();
        console.log(
          "[ProductHierarchyCascading] Loaded hierarchy types:",
          types
        );

        if (types.length === 0) {
          console.warn("No hierarchy types configured");
          return;
        }

        const sortedTypes = types.sort(
          (a, b) => (a.ItemLevel || 0) - (b.ItemLevel || 0)
        );
        console.log(
          "[ProductHierarchyCascading] Sorted hierarchy types:",
          sortedTypes.map((t) => ({
            name: t.Name,
            level: t.ItemLevel,
            uid: t.UID,
          }))
        );
        setHierarchyTypes(sortedTypes);

        // Load first level options only
        if (sortedTypes.length > 0) {
          const firstLevelType = sortedTypes[0];
          const firstLevelOptions =
            await hierarchyService.getHierarchyOptionsForType(
              firstLevelType.UID
            );

          console.log(
            "[ProductHierarchyCascading] First level options loaded:",
            {
              type: firstLevelType.Name,
              count: firstLevelOptions.length,
              sample: firstLevelOptions.slice(0, 3),
            }
          );

          setHierarchyOptions({ [firstLevelType.Name]: firstLevelOptions });
          setOptionsCache({ [firstLevelType.Name]: firstLevelOptions });
        }
      } catch (error) {
        console.error("Failed to initialize hierarchy:", error);
        toast({
          title: "Initialization Error",
          description: "Failed to load hierarchy configuration",
          variant: "destructive",
        });
      } finally {
        setLoadingInitial(false);
      }
    };

    initializeHierarchy();
  }, []);

  /**
   * Load SKU products based on selected hierarchy
   */
  const loadSKUProducts = useCallback(async () => {
    if (!enableSKULoading || selectedItems.size === 0) return;

    try {
      setLoadingSKUs(true);

      // Get the deepest level selection to filter by ParentUID
      let deepestLevel = 0;
      let deepestSelections: SelectedAttribute[] = [];

      selectedItems.forEach((item) => {
        if (item.level > deepestLevel) {
          deepestLevel = item.level;
          deepestSelections = [item];
        } else if (item.level === deepestLevel) {
          deepestSelections.push(item);
        }
      });

      // Build filter keys for client-side filtering
      // Format: "CODE_NAME" like "D001_Divi", "S001_SubDivi", etc.
      const filterKeys: string[] = [];
      selectedItems.forEach((item) => {
        filterKeys.push(`${item.code}_${item.levelName}`);
      });

      // Load all SKUs - WebView endpoint expects camelCase properties
      const requestBody = {
        pageNumber: 1,
        pageSize: 1000, // Load first 1000 products to avoid timeout
        isCountRequired: true,
        filterCriterias: [], // Backend will query database directly
        sortCriterias: [
          {
            sortParameter: "Code",
            direction: "Asc",
          },
        ],
      };

      console.log("Loading SKUs, will filter for:", filterKeys);

      // Use SelectAllSKUDetailsWebView which returns all products with FilterKeys
      const response = await fetch(
        `${baseURL}/SKU/SelectAllSKUDetailsWebView`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(requestBody),
        }
      );

      if (!response.ok) {
        const errorText = await response.text();
        console.error("SKU API Error:", errorText);
        throw new Error(`Failed to fetch SKU products: ${response.status}`);
      }

      const result = await response.json();

      if (
        (result.IsSuccess || result.isSuccess) &&
        (result.Data || result.data)
      ) {
        const data = result.Data || result.data;
        let skuData: SKUProduct[] = [];

        // Handle PagedData/pagedData structure (WebView returns camelCase)
        const pagedData = data.PagedData || data.pagedData;
        if (pagedData && Array.isArray(pagedData)) {
          // Log first item to see structure (WebView API returns FilterKeys)
          if (pagedData.length > 0) {
            console.log("SKU structure:", pagedData[0]);
            console.log(
              "FilterKeys available:",
              pagedData[0].FilterKeys || pagedData[0].filterKeys
            );
          }

          // WebView returns FilterKeys array for each SKU
          // FilterKeys format: ["D001_Divi", "S001_SubDivi", "C001_Category", "S925_Sub Category", "B283_Brand Name", "EPIC01_ORG", "Supplier_Division"]
          const filteredData = pagedData.filter((sku: any) => {
            // If no filters selected, return all
            if (filterKeys.length === 0) return true;

            // Check if SKU has FilterKeys array (handle both PascalCase and camelCase)
            const skuFilterKeys = sku.FilterKeys || sku.filterKeys;
            if (!skuFilterKeys || !Array.isArray(skuFilterKeys)) {
              console.warn(`SKU ${sku.Code || sku.code} has no FilterKeys`);
              return false;
            }

            // Check if all our filter keys are present in the SKU's FilterKeys
            const matches = filterKeys.every((filterKey: string) =>
              skuFilterKeys.some(
                (skuFilterKey: string) => skuFilterKey === filterKey
              )
            );

            if (matches) {
              console.log(
                `SKU ${sku.Code || sku.code} matches with FilterKeys:`,
                skuFilterKeys
              );
            }

            return matches;
          });

          console.log(
            `Filtered ${pagedData.length} SKUs to ${filteredData.length} matching selection`
          );

          // Map WebView fields to our expected format (handle both cases)
          skuData = filteredData.map((sku: any) => ({
            Id: sku.Id || sku.id,
            UID: sku.UID || sku.uid,
            Code: sku.Code || sku.code,
            Name: sku.Name || sku.name,
            ArabicName: sku.ArabicName || sku.arabicName,
            LongName: sku.LongName || sku.longName,
            BaseUOM: sku.BaseUOM || sku.baseUOM,
            OuterUOM: sku.OuterUOM || sku.outerUOM,
            IsActive: sku.IsActive !== undefined ? sku.IsActive : sku.isActive,
            ParentUID: sku.ParentUID || sku.parentUID,
            DivisionUID: sku.DivisionUID || sku.divisionUID,
            ProductCategoryId: sku.ProductCategoryId || sku.productCategoryId,
            FilterKeys: sku.FilterKeys || sku.filterKeys,
            OrgUID: sku.OrgUID || sku.orgUID,
            HSNCode: sku.HSNCode || sku.hsnCode,
            L2: sku.L2 || sku.l2,
            L3: sku.L3 || sku.l3,
            IsThirdParty:
              sku.IsThirdParty !== undefined
                ? sku.IsThirdParty
                : sku.isThirdParty,
            SupplierOrgUID: sku.SupplierOrgUID || sku.supplierOrgUID,
          }));

          // Additional filter by ParentUID if needed
          if (deepestSelections.length > 0) {
            const parentUIDs = deepestSelections.map((s) => s.code);
            const parentFilteredData = skuData.filter((sku) =>
              parentUIDs.includes(sku.ParentUID)
            );

            if (parentFilteredData.length > 0) {
              skuData = parentFilteredData;
              console.log(
                `Further filtered by ParentUID to ${skuData.length} SKUs`
              );
            }
          }
        }

        setSKUProducts(skuData);
        console.log(`Total loaded: ${skuData.length} SKU products`);
      } else {
        setSKUProducts([]);
      }
    } catch (error) {
      console.error("Failed to load SKU products:", error);
      toast({
        title: "Error",
        description: "Failed to load products. Please check your selection.",
        variant: "destructive",
      });
      setSKUProducts([]);
    } finally {
      setLoadingSKUs(false);
    }
  }, [selectedItems, enableSKULoading, baseURL, toast]);

  /**
   * Trigger SKU loading when hierarchy selection changes
   */
  useEffect(() => {
    if (enableSKULoading && selectedItems.size > 0) {
      const timer = setTimeout(() => {
        loadSKUProducts();
      }, 500); // Debounce to avoid too many API calls

      return () => clearTimeout(timer);
    } else {
      setSKUProducts([]);
      setSelectedSKUs(new Set());
    }
  }, [selectedItems, enableSKULoading, loadSKUProducts]);

  /**
   * Load child options dynamically when parent is selected
   */
  const loadChildOptions = async (
    parentCodes: Set<string>,
    childLevel: number
  ) => {
    console.log("[ProductHierarchyCascading] loadChildOptions called:", {
      parentCodes: Array.from(parentCodes),
      childLevel,
      hierarchyTypesLength: hierarchyTypes.length,
    });

    if (childLevel >= hierarchyTypes.length) {
      console.log(
        "[ProductHierarchyCascading] childLevel >= hierarchyTypes.length, returning"
      );
      return;
    }

    const childType = hierarchyTypes[childLevel];
    if (!childType) {
      console.log(
        "[ProductHierarchyCascading] No childType found for level:",
        childLevel
      );
      return;
    }

    console.log(
      "[ProductHierarchyCascading] Loading children for type:",
      childType.Name
    );

    try {
      setLoadingState((prev) => ({ ...prev, [childLevel]: true }));

      const allChildOptions: HierarchyOption[] = [];

      for (const parentCode of parentCodes) {
        const cacheKey = `${parentCode}_${childLevel}`;
        console.log(
          "[ProductHierarchyCascading] Processing parent:",
          parentCode
        );

        if (optionsCache[cacheKey]) {
          console.log("[ProductHierarchyCascading] Using cache for:", cacheKey);
          allChildOptions.push(...optionsCache[cacheKey]);
        } else {
          console.log(
            "[ProductHierarchyCascading] Fetching from API for parent:",
            parentCode
          );
          const childGroups = await hierarchyService.getChildSKUGroups(
            parentCode
          );
          console.log(
            "[ProductHierarchyCascading] API returned",
            childGroups.length,
            "groups"
          );

          const childOptions = childGroups.map((group) => ({
            code: group.Code,
            value: group.Name,
            type: childType.Name,
            parentCode: parentCode,
          }));

          setOptionsCache((prev) => ({
            ...prev,
            [cacheKey]: childOptions,
          }));

          allChildOptions.push(...childOptions);
        }
      }

      const uniqueOptions = Array.from(
        new Map(allChildOptions.map((opt) => [opt.code, opt])).values()
      );

      console.log("[ProductHierarchyCascading] Setting options:", {
        key: `${childType.Name}_combined`,
        count: uniqueOptions.length,
        options: uniqueOptions.slice(0, 3),
      });

      setHierarchyOptions((prev) => {
        const newOptions = {
          ...prev,
          [`${childType.Name}_combined`]: uniqueOptions,
        };
        console.log(
          "[ProductHierarchyCascading] HierarchyOptions after update:",
          Object.keys(newOptions)
        );
        return newOptions;
      });
    } catch (error) {
      console.error(`Failed to load options for level ${childLevel}:`, error);

      const childType = hierarchyTypes[childLevel];
      if (childType) {
        setHierarchyOptions((prev) => ({
          ...prev,
          [`${childType.Name}_combined`]: [],
        }));
      }
    } finally {
      setLoadingState((prev) => ({ ...prev, [childLevel]: false }));
    }
  };

  /**
   * Handle item selection/deselection
   */
  const handleItemToggle = async (
    item: HierarchyOption & { parentCode?: string },
    levelIndex: number,
    checked: boolean
  ) => {
    const level = levelIndex + 1;
    const levelType = hierarchyTypes[levelIndex];
    const itemKey = `${level}_${item.code}`;

    console.log("[ProductHierarchyCascading] handleItemToggle:", {
      item: item.code,
      levelIndex,
      level,
      checked,
      levelType: levelType?.Name,
    });

    const newSelections = new Map(selectedItems);
    const newLevelSelections = { ...levelSelections };

    if (checked) {
      const selectedItem: SelectedAttribute = {
        level,
        levelName: levelType.Name,
        code: item.code,
        value: item.value,
        uid: item.code,
        parentCode: item.parentCode,
        fieldName: generateFieldName(level),
      };
      newSelections.set(itemKey, selectedItem);

      if (!newLevelSelections[level]) {
        newLevelSelections[level] = new Set();
      }
      newLevelSelections[level].add(item.code);

      if (levelIndex < hierarchyTypes.length - 1) {
        console.log(
          "[ProductHierarchyCascading] Loading child options for next level:",
          {
            currentLevel: level,
            nextLevelIndex: levelIndex + 1,
            parentCodes: Array.from(newLevelSelections[level] || []),
          }
        );
        await loadChildOptions(newLevelSelections[level], levelIndex + 1);
      }
    } else {
      newSelections.delete(itemKey);

      if (newLevelSelections[level]) {
        newLevelSelections[level].delete(item.code);
        if (newLevelSelections[level].size === 0) {
          delete newLevelSelections[level];
        }
      }

      // Clear child selections
      for (
        let childLevel = level + 1;
        childLevel <= hierarchyTypes.length;
        childLevel++
      ) {
        const keysToRemove: string[] = [];
        newSelections.forEach((selectedItem, key) => {
          if (selectedItem.level >= childLevel) {
            keysToRemove.push(key);
          }
        });

        keysToRemove.forEach((key) => {
          const item = newSelections.get(key);
          if (item && newLevelSelections[item.level]) {
            newLevelSelections[item.level].delete(item.code);
            if (newLevelSelections[item.level].size === 0) {
              delete newLevelSelections[item.level];
            }
          }
          newSelections.delete(key);
        });
      }

      if (newLevelSelections[level] && newLevelSelections[level].size > 0) {
        if (levelIndex < hierarchyTypes.length - 1) {
          await loadChildOptions(newLevelSelections[level], levelIndex + 1);
        }
      }
    }

    setSelectedItems(newSelections);
    setLevelSelections(newLevelSelections);
    onSelectionsChange(Array.from(newSelections.values()));
  };

  /**
   * Handle SKU product selection
   */
  const handleSKUToggle = (sku: SKUProduct, checked: boolean) => {
    const newSelectedSKUs = new Set(selectedSKUs);

    if (checked) {
      newSelectedSKUs.add(sku.UID);
    } else {
      newSelectedSKUs.delete(sku.UID);
    }

    setSelectedSKUs(newSelectedSKUs);

    const selectedProducts = skuProducts.filter((p) =>
      newSelectedSKUs.has(p.UID)
    );
    onSKUSelectionsChange(selectedProducts);
  };

  /**
   * Select all SKUs
   */
  const selectAllSKUs = () => {
    const allUIDs = new Set(skuProducts.map((p) => p.UID));
    setSelectedSKUs(allUIDs);
    onSKUSelectionsChange(skuProducts);
  };

  /**
   * Clear all SKU selections
   */
  const clearAllSKUs = () => {
    setSelectedSKUs(new Set());
    onSKUSelectionsChange([]);
  };

  /**
   * Filter SKU products based on search
   */
  const filterSKUProducts = () => {
    if (!skuSearchTerm) return skuProducts;

    const term = skuSearchTerm.toLowerCase();
    return skuProducts.filter(
      (sku) =>
        sku.Code.toLowerCase().includes(term) ||
        sku.Name.toLowerCase().includes(term) ||
        (sku.LongName && sku.LongName.toLowerCase().includes(term))
    );
  };

  /**
   * Select all items at a level
   */
  const selectAllAtLevel = async (levelIndex: number) => {
    const options = getOptionsForLevel(levelIndex);

    for (const option of options) {
      await handleItemToggle(option, levelIndex, true);
    }
  };

  /**
   * Clear all selections at a level
   */
  const clearAllAtLevel = async (levelIndex: number) => {
    const level = levelIndex + 1;
    const itemsAtLevel = Array.from(selectedItems.values()).filter(
      (item) => item.level === level
    );

    for (const item of itemsAtLevel) {
      await handleItemToggle(
        {
          code: item.code,
          value: item.value,
          type: item.levelName,
          parentCode: item.parentCode,
        },
        levelIndex,
        false
      );
    }
  };

  /**
   * Get options for a specific level
   */
  const getOptionsForLevel = (
    levelIndex: number
  ): (HierarchyOption & { parentCode?: string })[] => {
    const levelType = hierarchyTypes[levelIndex];
    if (!levelType) {
      console.log(
        "[ProductHierarchyCascading] getOptionsForLevel: No levelType for index:",
        levelIndex
      );
      return [];
    }

    if (levelIndex === 0) {
      const options = hierarchyOptions[levelType.Name] || [];
      console.log(
        "[ProductHierarchyCascading] getOptionsForLevel (first level):",
        {
          levelIndex,
          key: levelType.Name,
          count: options.length,
        }
      );
      return options;
    }

    const key = `${levelType.Name}_combined`;
    const options = hierarchyOptions[key] || [];
    console.log(
      "[ProductHierarchyCascading] getOptionsForLevel (child level):",
      {
        levelIndex,
        key,
        count: options.length,
        availableKeys: Object.keys(hierarchyOptions),
      }
    );
    return options;
  };

  /**
   * Check if a level should be shown
   */
  const shouldShowLevel = (levelIndex: number): boolean => {
    if (levelIndex === 0) return true;

    const parentLevel = levelIndex;
    const shouldShow =
      levelSelections[parentLevel] && levelSelections[parentLevel].size > 0;

    console.log("[ProductHierarchyCascading] shouldShowLevel:", {
      levelIndex,
      parentLevel,
      shouldShow,
      levelSelections: Object.keys(levelSelections).map((k) => ({
        level: k,
        count: levelSelections[k]?.size || 0,
      })),
    });

    return shouldShow;
  };

  /**
   * Filter options based on search term
   */
  const filterOptions = (
    options: (HierarchyOption & { parentCode?: string })[],
    levelIndex: number
  ) => {
    const searchTerm = searchTerms[levelIndex] || "";
    if (!searchTerm) return options;

    const term = searchTerm.toLowerCase();
    return options.filter(
      (opt) =>
        opt.code.toLowerCase().includes(term) ||
        opt.value.toLowerCase().includes(term)
    );
  };

  /**
   * Get display text for dropdown trigger
   */
  const getDropdownTriggerText = (levelIndex: number): string => {
    const level = levelIndex + 1;
    const selectedCount = Array.from(selectedItems.values()).filter(
      (item) => item.level === level
    ).length;

    const levelType = hierarchyTypes[levelIndex];

    if (selectedCount === 0) {
      return `Select ${levelType?.Name || "items"}`;
    }

    if (selectedCount === 1) {
      const item = Array.from(selectedItems.values()).find(
        (i) => i.level === level
      );
      return item
        ? `${item.value} (${item.code})`
        : `Select ${levelType?.Name || "items"}`;
    }

    return `${selectedCount} ${levelType?.Name || "items"} selected`;
  };

  /**
   * Render a single dropdown for a level
   */
  const renderLevelDropdown = (type: SKUGroupType, levelIndex: number) => {
    const level = levelIndex + 1;
    const isLoading = loadingState[levelIndex];
    const isOpen = openDropdowns[levelIndex] || false;
    const options = getOptionsForLevel(levelIndex);
    const filteredOptions = filterOptions(options, levelIndex);
    const selectedCount = Array.from(selectedItems.values()).filter(
      (item) => item.level === level
    ).length;

    console.log("[ProductHierarchyCascading] renderLevelDropdown:", {
      type: type.Name,
      levelIndex,
      level,
      optionsCount: options.length,
      filteredCount: filteredOptions.length,
      selectedCount,
      shouldShow: shouldShowLevel(levelIndex),
    });

    if (!shouldShowLevel(levelIndex)) {
      console.log(
        "[ProductHierarchyCascading] Not showing level - shouldShowLevel returned false"
      );
      return null;
    }

    const isFirstLevel = levelIndex === 0;

    return (
      <div key={type.UID} className="space-y-2">
        <Label>{generateLevelLabel(level, type.Name)}</Label>
        <Popover
          open={isOpen}
          onOpenChange={(open) => {
            setOpenDropdowns((prev) => ({ ...prev, [levelIndex]: open }));
          }}
        >
          <PopoverTrigger asChild>
            <Button
              variant="outline"
              className="w-full justify-between font-normal"
              disabled={(() => {
                const isDisabled =
                  disabled ||
                  (!isFirstLevel && !shouldShowLevel(levelIndex - 1));
                console.log(
                  "[ProductHierarchyCascading] Button disabled check:",
                  {
                    levelIndex,
                    isFirstLevel,
                    disabled,
                    shouldShowPrevLevel:
                      levelIndex > 0 ? shouldShowLevel(levelIndex - 1) : "N/A",
                    isDisabled,
                  }
                );
                return isDisabled;
              })()}
            >
              <span className="truncate">
                {getDropdownTriggerText(levelIndex)}
              </span>
              <div className="flex items-center gap-2">
                {selectedCount > 0 && (
                  <Badge variant="secondary" className="text-xs">
                    {selectedCount}
                  </Badge>
                )}
                <ChevronDown className="h-4 w-4 shrink-0 opacity-50" />
              </div>
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-[350px] p-0" align="start">
            <div className="border-b p-3">
              <div className="flex items-center justify-between mb-2">
                <h4 className="font-medium text-sm">{type.Name}</h4>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() =>
                    setOpenDropdowns((prev) => ({
                      ...prev,
                      [levelIndex]: false,
                    }))
                  }
                  className="h-6 w-6 p-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>

              {enableSearch && options.length > 5 && (
                <div className="relative mb-2">
                  <Search className="absolute left-2 top-1/2 transform -translate-y-1/2 h-3 w-3 text-muted-foreground" />
                  <Input
                    type="text"
                    placeholder="Search..."
                    value={searchTerms[levelIndex] || ""}
                    onChange={(e) =>
                      setSearchTerms((prev) => ({
                        ...prev,
                        [levelIndex]: e.target.value,
                      }))
                    }
                    className="pl-7 h-7 text-sm"
                    onClick={(e) => e.stopPropagation()}
                  />
                </div>
              )}

              {options.length > 0 && (
                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => selectAllAtLevel(levelIndex)}
                    className="flex-1 h-7 text-xs"
                    disabled={isLoading}
                  >
                    <CheckSquare className="h-3 w-3 mr-1" />
                    Select All ({options.length})
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => clearAllAtLevel(levelIndex)}
                    className="flex-1 h-7 text-xs"
                    disabled={isLoading || selectedCount === 0}
                  >
                    <Square className="h-3 w-3 mr-1" />
                    Clear All
                  </Button>
                </div>
              )}
            </div>

            <ScrollArea className="h-[250px]">
              {isLoading ? (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  Loading...
                </div>
              ) : filteredOptions.length === 0 ? (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  {searchTerms[levelIndex]
                    ? "No matching items"
                    : levelIndex === 0
                    ? "No items available"
                    : "No items for selected parents"}
                </div>
              ) : (
                <div className="p-2">
                  {filteredOptions.map((option) => {
                    const itemKey = `${level}_${option.code}`;
                    const isSelected = selectedItems.has(itemKey);

                    return (
                      <div
                        key={option.code}
                        className="flex items-center gap-2 p-2 hover:bg-gray-50 rounded"
                      >
                        <Checkbox
                          id={itemKey}
                          checked={isSelected}
                          onCheckedChange={(checked) =>
                            handleItemToggle(
                              option,
                              levelIndex,
                              checked as boolean
                            )
                          }
                        />
                        <Label
                          htmlFor={itemKey}
                          className="flex-1 cursor-pointer text-sm"
                        >
                          <span className="font-medium">{option.code}</span>
                          <span className="text-muted-foreground">
                            {" "}
                            - {option.value}
                          </span>
                        </Label>
                      </div>
                    );
                  })}
                </div>
              )}
            </ScrollArea>
          </PopoverContent>
        </Popover>
      </div>
    );
  };

  /**
   * Render SKU products dropdown
   */
  const renderSKUDropdown = () => {
    if (!showSKUSection || !enableSKULoading) return null;
    if (selectedItems.size === 0) return null;

    const filteredSKUs = filterSKUProducts();

    return (
      <div className="space-y-2">
        <Label>
          <Package className="h-4 w-4 inline mr-1" />
          Products (SKU)
        </Label>
        <Popover open={skuDropdownOpen} onOpenChange={setSKUDropdownOpen}>
          <PopoverTrigger asChild>
            <Button
              variant="outline"
              className="w-full justify-between font-normal"
              disabled={loadingSKUs || skuProducts.length === 0}
            >
              <span className="truncate">
                {loadingSKUs
                  ? "Loading products..."
                  : skuProducts.length === 0
                  ? "No products available"
                  : selectedSKUs.size === 0
                  ? `Select products (${skuProducts.length} available)`
                  : selectedSKUs.size === 1
                  ? "1 product selected"
                  : `${selectedSKUs.size} products selected`}
              </span>
              <div className="flex items-center gap-2">
                {selectedSKUs.size > 0 && (
                  <Badge variant="secondary" className="text-xs">
                    {selectedSKUs.size}
                  </Badge>
                )}
                <ChevronDown className="h-4 w-4 shrink-0 opacity-50" />
              </div>
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-[400px] p-0" align="start">
            <div className="border-b p-3">
              <div className="flex items-center justify-between mb-2">
                <h4 className="font-medium text-sm">Products</h4>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => setSKUDropdownOpen(false)}
                  className="h-6 w-6 p-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>

              {skuProducts.length > 5 && (
                <div className="relative mb-2">
                  <Search className="absolute left-2 top-1/2 transform -translate-y-1/2 h-3 w-3 text-muted-foreground" />
                  <Input
                    type="text"
                    placeholder="Search products..."
                    value={skuSearchTerm}
                    onChange={(e) => setSKUSearchTerm(e.target.value)}
                    className="pl-7 h-7 text-sm"
                  />
                </div>
              )}

              {skuProducts.length > 0 && (
                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={selectAllSKUs}
                    className="flex-1 h-7 text-xs"
                  >
                    <CheckSquare className="h-3 w-3 mr-1" />
                    Select All ({filteredSKUs.length})
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={clearAllSKUs}
                    className="flex-1 h-7 text-xs"
                    disabled={selectedSKUs.size === 0}
                  >
                    <Square className="h-3 w-3 mr-1" />
                    Clear All
                  </Button>
                </div>
              )}
            </div>

            <ScrollArea className="h-[300px]">
              {filteredSKUs.length === 0 ? (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  {skuSearchTerm
                    ? "No matching products"
                    : "No products found for selected hierarchy"}
                </div>
              ) : (
                <div className="p-2">
                  {filteredSKUs.map((sku) => {
                    const isSelected = selectedSKUs.has(sku.UID);

                    return (
                      <div
                        key={sku.UID}
                        className="flex items-start gap-2 p-2 hover:bg-gray-50 rounded"
                      >
                        <Checkbox
                          id={`sku_${sku.UID}`}
                          checked={isSelected}
                          onCheckedChange={(checked) =>
                            handleSKUToggle(sku, checked as boolean)
                          }
                          className="mt-1"
                        />
                        <Label
                          htmlFor={`sku_${sku.UID}`}
                          className="flex-1 cursor-pointer"
                        >
                          <div className="text-sm">
                            <span className="font-medium">{sku.Code}</span>
                            <span className="text-muted-foreground">
                              {" "}
                              - {sku.Name}
                            </span>
                          </div>
                          {sku.LongName && (
                            <div className="text-xs text-muted-foreground mt-1">
                              {sku.LongName}
                            </div>
                          )}
                          <div className="flex gap-2 mt-1">
                            {sku.BaseUOM && (
                              <Badge variant="outline" className="text-xs">
                                {sku.BaseUOM}
                              </Badge>
                            )}
                            {sku.IsActive ? (
                              <Badge variant="default" className="text-xs">
                                Active
                              </Badge>
                            ) : (
                              <Badge variant="secondary" className="text-xs">
                                Inactive
                              </Badge>
                            )}
                          </div>
                        </Label>
                      </div>
                    );
                  })}
                </div>
              )}
            </ScrollArea>
          </PopoverContent>
        </Popover>
      </div>
    );
  };

  if (loadingInitial) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Product Hierarchy (Cascading)</CardTitle>
          <CardDescription>Loading hierarchy configuration...</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="animate-pulse space-y-4">
            <div className="h-10 bg-gray-200 rounded"></div>
            <div className="h-10 bg-gray-200 rounded"></div>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (hierarchyTypes.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Product Hierarchy (Cascading)</CardTitle>
          <CardDescription>
            No hierarchy configuration available
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Product Attributes & SKU Selection</CardTitle>
        <CardDescription>
          Select product hierarchy with cascading dropdowns
        </CardDescription>
      </CardHeader>
      <CardContent>
        {/* Hierarchy Dropdowns */}
        <div
          className={`grid grid-cols-${gridColumns.default} md:grid-cols-${gridColumns.md} lg:grid-cols-${gridColumns.lg} gap-4 mb-4`}
        >
          {hierarchyTypes.map((type, index) =>
            renderLevelDropdown(type, index)
          )}
        </div>

        {/* SKU Products Dropdown */}
        {renderSKUDropdown()}

        {/* Products in Selected Hierarchy */}
        {skuProducts.length > 0 && (
          <div className="mt-6 space-y-4">
            <div className="border-t pt-4">
              <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                <Package className="h-5 w-5" />
                Products in Selected Hierarchy
                <Badge variant="secondary">
                  {skuProducts.length} available
                </Badge>
              </h3>

              {/* Product Selection Summary */}
              {selectedSKUs.size > 0 && (
                <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <div className="flex items-center justify-between mb-3">
                    <span className="text-sm font-medium text-blue-900">
                      Selected Products ({selectedSKUs.size}/
                      {skuProducts.length})
                    </span>
                    <div className="flex gap-2">
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        onClick={selectAllSKUs}
                        className="text-xs"
                      >
                        Select All
                      </Button>
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        onClick={clearAllSKUs}
                        className="text-xs"
                      >
                        Clear All
                      </Button>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="text-xs text-blue-700 font-medium">
                      Selected Items:
                    </div>
                    <div className="flex flex-wrap gap-2">
                      {Array.from(selectedSKUs)
                        .slice(0, 10)
                        .map((uid) => {
                          const sku = skuProducts.find((p) => p.UID === uid);
                          if (!sku) return null;

                          return (
                            <Badge
                              key={uid}
                              variant="secondary"
                              className="text-xs py-1 px-2"
                            >
                              <span className="font-medium">{sku.Code}</span>
                              <span className="ml-1 text-gray-600">
                                - {sku.Name}
                              </span>
                              <button
                                type="button"
                                onClick={() => handleSKUToggle(sku, false)}
                                className="ml-2 hover:text-destructive"
                              >
                                <X className="h-3 w-3" />
                              </button>
                            </Badge>
                          );
                        })}
                      {selectedSKUs.size > 10 && (
                        <Badge variant="outline" className="text-xs">
                          +{selectedSKUs.size - 10} more products
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {/* Products Grid/List */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3 max-h-[400px] overflow-y-auto p-1">
                {skuProducts.slice(0, 50).map((sku) => {
                  const isSelected = selectedSKUs.has(sku.UID);
                  return (
                    <div
                      key={sku.UID}
                      className={cn(
                        "border rounded-lg p-3 cursor-pointer transition-all",
                        isSelected
                          ? "border-blue-500 bg-blue-50"
                          : "border-gray-200 hover:border-gray-300 hover:bg-gray-50"
                      )}
                      onClick={() => handleSKUToggle(sku, !isSelected)}
                    >
                      <div className="flex items-start gap-3">
                        <Checkbox
                          checked={isSelected}
                          onCheckedChange={(checked) =>
                            handleSKUToggle(sku, checked as boolean)
                          }
                          className="mt-1"
                        />
                        <div className="flex-1 min-w-0">
                          <div className="font-medium text-sm truncate">
                            {sku.Code}
                          </div>
                          <div className="text-xs text-gray-600 truncate">
                            {sku.Name}
                          </div>
                          {sku.LongName && (
                            <div className="text-xs text-gray-500 truncate mt-1">
                              {sku.LongName}
                            </div>
                          )}
                          <div className="flex gap-2 mt-2">
                            {sku.BaseUOM && (
                              <Badge variant="outline" className="text-xs">
                                {sku.BaseUOM}
                              </Badge>
                            )}
                            <Badge
                              variant={sku.IsActive ? "default" : "secondary"}
                              className="text-xs"
                            >
                              {sku.IsActive ? "Active" : "Inactive"}
                            </Badge>
                          </div>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>

              {skuProducts.length > 50 && (
                <div className="text-center text-sm text-gray-500 mt-3">
                  Showing first 50 products. Use search to find specific items.
                </div>
              )}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
