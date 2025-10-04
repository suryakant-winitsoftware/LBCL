"use client";

import { useState, useEffect } from "react";
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
  Popover,
  PopoverContent,
  PopoverTrigger
} from "@/components/ui/popover";
import {
  hierarchyService,
  SKUGroupType,
  HierarchyOption
} from "@/services/hierarchy.service";
import { useToast } from "@/components/ui/use-toast";
import { Search, ChevronDown, X, CheckSquare, Square } from "lucide-react";

/**
 * Dynamic Multi-Select Product Attributes with Separate Dropdowns
 * 
 * This component creates separate dropdown for each hierarchy level:
 * - Each level has its own dropdown field with checkboxes
 * - Child dropdowns only appear after parent selection
 * - Follows the same cascading pattern as ProductAttributes.tsx
 * - Multi-select capability with Select All/Clear All per dropdown
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

interface LevelSelections {
  [level: number]: Set<string>;
}

interface ProductAttributesMultiDropdownProps {
  /**
   * Callback when selections change
   */
  onSelectionsChange: (selections: SelectedAttribute[]) => void;

  /**
   * Optional field name pattern for mapping
   * Examples: "L{n}" -> L1, L2, L3...
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
   * Initial selections for edit mode
   */
  initialSelections?: SelectedAttribute[];

  /**
   * Enable search in dropdowns
   */
  enableSearch?: boolean;

  /**
   * Responsive grid columns
   */
  gridColumns?: {
    default: number;
    md: number;
    lg: number;
  };
}

export default function ProductAttributesMultiDropdown({
  onSelectionsChange,
  fieldNamePattern = "L{n}",
  showLevelNumbers = true,
  disabled = false,
  initialSelections = [],
  enableSearch = true,
  gridColumns = { default: 1, md: 2, lg: 3 }
}: ProductAttributesMultiDropdownProps) {
  const { toast } = useToast();

  // Core state - Similar to ProductAttributes.tsx
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([]);
  const [hierarchyOptions, setHierarchyOptions] = useState<Record<string, HierarchyOption[]>>({});
  const [selectedItems, setSelectedItems] = useState<Map<string, SelectedAttribute>>(new Map());
  const [loadingState, setLoadingState] = useState<Record<number, boolean>>({});
  const [loadingInitial, setLoadingInitial] = useState(true);
  const [optionsCache, setOptionsCache] = useState<Record<string, HierarchyOption[]>>({});

  // Track selections per level
  const [levelSelections, setLevelSelections] = useState<LevelSelections>({});

  // Track which dropdowns are open
  const [openDropdowns, setOpenDropdowns] = useState<Record<number, boolean>>({});

  // Search terms per dropdown
  const [searchTerms, setSearchTerms] = useState<Record<number, string>>({});

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
        
        if (types.length === 0) {
          console.warn("No hierarchy types configured");
          return;
        }

        const sortedTypes = types.sort((a, b) => (a.ItemLevel || 0) - (b.ItemLevel || 0));
        setHierarchyTypes(sortedTypes);

        // Load first level options only
        if (sortedTypes.length > 0) {
          const firstLevelType = sortedTypes[0];
          const firstLevelOptions = await hierarchyService.getHierarchyOptionsForType(
            firstLevelType.UID
          );

          setHierarchyOptions({ [firstLevelType.Name]: firstLevelOptions });
          setOptionsCache({ [firstLevelType.Name]: firstLevelOptions });
        }

        // Load initial selections if provided
        if (initialSelections.length > 0) {
          const selectionMap = new Map<string, SelectedAttribute>();
          const levelMap: LevelSelections = {};
          
          initialSelections.forEach(item => {
            selectionMap.set(`${item.level}_${item.code}`, item);
            
            if (!levelMap[item.level]) {
              levelMap[item.level] = new Set();
            }
            levelMap[item.level].add(item.code);
          });
          
          setSelectedItems(selectionMap);
          setLevelSelections(levelMap);
        }
      } catch (error) {
        console.error("Failed to initialize hierarchy:", error);
        toast({
          title: "Initialization Error",
          description: "Failed to load hierarchy configuration",
          variant: "destructive"
        });
      } finally {
        setLoadingInitial(false);
      }
    };

    initializeHierarchy();
  }, []);

  /**
   * Load child options dynamically when parent is selected
   */
  const loadChildOptions = async (parentCodes: Set<string>, childLevel: number) => {
    if (childLevel >= hierarchyTypes.length) return;

    const childType = hierarchyTypes[childLevel];
    if (!childType) return;

    try {
      setLoadingState(prev => ({ ...prev, [childLevel]: true }));

      const allChildOptions: HierarchyOption[] = [];
      
      // Load children for each selected parent
      for (const parentCode of parentCodes) {
        const cacheKey = `${parentCode}_${childLevel}`;
        
        if (optionsCache[cacheKey]) {
          // Use cached options
          allChildOptions.push(...optionsCache[cacheKey]);
        } else {
          // Load from API
          const childGroups = await hierarchyService.getChildSKUGroups(parentCode);
          const childOptions = childGroups.map(group => ({
            code: group.Code,
            value: group.Name,
            type: childType.Name,
            parentCode: parentCode
          }));
          
          // Cache the results
          setOptionsCache(prev => ({
            ...prev,
            [cacheKey]: childOptions
          }));
          
          allChildOptions.push(...childOptions);
        }
      }

      // Remove duplicates if any
      const uniqueOptions = Array.from(
        new Map(allChildOptions.map(opt => [opt.code, opt])).values()
      );

      // Store all child options for this level
      setHierarchyOptions(prev => ({
        ...prev,
        [`${childType.Name}_combined`]: uniqueOptions
      }));

    } catch (error) {
      console.error(`Failed to load options for level ${childLevel}:`, error);
      
      const childType = hierarchyTypes[childLevel];
      if (childType) {
        setHierarchyOptions(prev => ({
          ...prev,
          [`${childType.Name}_combined`]: []
        }));
      }
    } finally {
      setLoadingState(prev => ({ ...prev, [childLevel]: false }));
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
    
    const newSelections = new Map(selectedItems);
    const newLevelSelections = { ...levelSelections };

    if (checked) {
      // Add to selections
      const selectedItem: SelectedAttribute = {
        level,
        levelName: levelType.Name,
        code: item.code,
        value: item.value,
        uid: item.code,
        parentCode: item.parentCode,
        fieldName: generateFieldName(level)
      };
      newSelections.set(itemKey, selectedItem);

      // Track selection at this level
      if (!newLevelSelections[level]) {
        newLevelSelections[level] = new Set();
      }
      newLevelSelections[level].add(item.code);

      // Load children if not last level
      if (levelIndex < hierarchyTypes.length - 1) {
        await loadChildOptions(newLevelSelections[level], levelIndex + 1);
      }
    } else {
      // Remove from selections
      newSelections.delete(itemKey);

      // Remove from level selections
      if (newLevelSelections[level]) {
        newLevelSelections[level].delete(item.code);
        if (newLevelSelections[level].size === 0) {
          delete newLevelSelections[level];
        }
      }

      // Clear all child selections recursively
      for (let childLevel = level + 1; childLevel <= hierarchyTypes.length; childLevel++) {
        const keysToRemove: string[] = [];
        newSelections.forEach((selectedItem, key) => {
          if (selectedItem.level >= childLevel) {
            // Check if this is a descendant
            keysToRemove.push(key);
          }
        });
        
        keysToRemove.forEach(key => {
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

      // Reload child options if there are still parents selected
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
      item => item.level === level
    );
    
    for (const item of itemsAtLevel) {
      await handleItemToggle(
        { code: item.code, value: item.value, type: item.levelName, parentCode: item.parentCode },
        levelIndex,
        false
      );
    }
  };

  /**
   * Get options for a specific level
   */
  const getOptionsForLevel = (levelIndex: number): (HierarchyOption & { parentCode?: string })[] => {
    const levelType = hierarchyTypes[levelIndex];
    if (!levelType) return [];

    // First level - no parent dependency
    if (levelIndex === 0) {
      return hierarchyOptions[levelType.Name] || [];
    }

    // Child levels - get combined options from all parents
    return hierarchyOptions[`${levelType.Name}_combined`] || [];
  };

  /**
   * Check if a level should be shown
   */
  const shouldShowLevel = (levelIndex: number): boolean => {
    if (levelIndex === 0) return true; // Always show first level
    
    // Show if parent level has selections
    const parentLevel = levelIndex;
    return levelSelections[parentLevel] && levelSelections[parentLevel].size > 0;
  };

  /**
   * Filter options based on search term
   */
  const filterOptions = (options: (HierarchyOption & { parentCode?: string })[], levelIndex: number) => {
    const searchTerm = searchTerms[levelIndex] || "";
    if (!searchTerm) return options;

    const term = searchTerm.toLowerCase();
    return options.filter(opt =>
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
      item => item.level === level
    ).length;

    const levelType = hierarchyTypes[levelIndex];
    
    if (selectedCount === 0) {
      return `Select ${levelType?.Name || 'items'}`;
    }
    
    if (selectedCount === 1) {
      const item = Array.from(selectedItems.values()).find(i => i.level === level);
      return item ? `${item.value} (${item.code})` : `Select ${levelType?.Name || 'items'}`;
    }
    
    return `${selectedCount} ${levelType?.Name || 'items'} selected`;
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
      item => item.level === level
    ).length;

    // Don't show if parent not selected (except first level)
    if (!shouldShowLevel(levelIndex)) {
      return null;
    }

    const isFirstLevel = levelIndex === 0;
    const isEnabled = isFirstLevel || (levelSelections[levelIndex] && levelSelections[levelIndex].size > 0);

    return (
      <div key={type.UID} className="space-y-2">
        <Label>{generateLevelLabel(level, type.Name)}</Label>
        <Popover 
          open={isOpen} 
          onOpenChange={(open) => {
            setOpenDropdowns(prev => ({ ...prev, [levelIndex]: open }));
          }}
        >
          <PopoverTrigger asChild>
            <Button
              variant="outline"
              className="w-full justify-between font-normal"
              disabled={disabled || (!isFirstLevel && !shouldShowLevel(levelIndex - 1))}
            >
              <span className="truncate">{getDropdownTriggerText(levelIndex)}</span>
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
          <PopoverContent 
            className="w-[350px] p-0" 
            align="start"
          >
            {/* Dropdown Header */}
            <div className="border-b p-3">
              <div className="flex items-center justify-between mb-2">
                <h4 className="font-medium text-sm">{type.Name}</h4>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => setOpenDropdowns(prev => ({ ...prev, [levelIndex]: false }))}
                  className="h-6 w-6 p-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>

              {/* Search Input */}
              {enableSearch && options.length > 5 && (
                <div className="relative mb-2">
                  <Search className="absolute left-2 top-1/2 transform -translate-y-1/2 h-3 w-3 text-muted-foreground" />
                  <Input
                    type="text"
                    placeholder="Search..."
                    value={searchTerms[levelIndex] || ""}
                    onChange={(e) => setSearchTerms(prev => ({
                      ...prev,
                      [levelIndex]: e.target.value
                    }))}
                    className="pl-7 h-7 text-sm"
                    onClick={(e) => e.stopPropagation()}
                  />
                </div>
              )}

              {/* Select All / Clear All */}
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

            {/* Options List */}
            <ScrollArea className="h-[250px]">
              {isLoading ? (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  Loading...
                </div>
              ) : filteredOptions.length === 0 ? (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  {searchTerms[levelIndex] ? "No matching items" : 
                   levelIndex === 0 ? "No items available" : "No items for selected parents"}
                </div>
              ) : (
                <div className="p-2">
                  {filteredOptions.map(option => {
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
                            handleItemToggle(option, levelIndex, checked as boolean)
                          }
                        />
                        <Label
                          htmlFor={itemKey}
                          className="flex-1 cursor-pointer text-sm"
                        >
                          <span className="font-medium">{option.code}</span>
                          <span className="text-muted-foreground"> - {option.value}</span>
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
          <CardTitle>Product Attributes</CardTitle>
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
          <CardTitle>Product Attributes</CardTitle>
          <CardDescription>No hierarchy configuration available</CardDescription>
        </CardHeader>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Product Attributes - Multi-Select</CardTitle>
        <CardDescription>
          Select multiple items from each hierarchy level
        </CardDescription>
      </CardHeader>
      <CardContent>
        {/* Dynamic Grid of Dropdowns */}
        <div className={`grid grid-cols-${gridColumns.default} md:grid-cols-${gridColumns.md} lg:grid-cols-${gridColumns.lg} gap-4`}>
          {hierarchyTypes.map((type, index) => 
            renderLevelDropdown(type, index)
          )}
        </div>

      </CardContent>
    </Card>
  );
}