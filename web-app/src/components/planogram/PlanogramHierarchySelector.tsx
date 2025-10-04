"use client";

import React, { useState, useEffect } from "react";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card } from "@/components/ui/card";
import {
  hierarchyService,
  SKUGroupType,
  HierarchyOption,
} from "@/services/hierarchy.service";
import { skuGroupService, SKUGroup } from "@/services/sku-group.service";
import { useToast } from "@/components/ui/use-toast";
import { Loader2 } from "lucide-react";

interface HierarchyLevel {
  level: number;
  type: string;
  code: string;
  value: string;
  uid?: string;
}

interface PlanogramHierarchySelectorProps {
  onSelectionChange: (selection: {
    selectionType: string;
    selectionValue: string;
    fullHierarchy: HierarchyLevel[];
  }) => void;
  initialSelection?: {
    selectionType: string;
    selectionValue: string;
  };
}

export default function PlanogramHierarchySelector({
  onSelectionChange,
  initialSelection,
}: PlanogramHierarchySelectorProps) {
  const { toast } = useToast();

  // Hierarchy types from database
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([]);

  // Selected values for each level
  const [selectedLevels, setSelectedLevels] = useState<HierarchyLevel[]>([]);

  // Options for each level
  const [levelOptions, setLevelOptions] = useState<
    Record<number, HierarchyOption[]>
  >({});

  // Loading states
  const [loadingTypes, setLoadingTypes] = useState(true);
  const [loadingLevel, setLoadingLevel] = useState<Record<number, boolean>>({});

  // Load hierarchy types on mount
  useEffect(() => {
    loadHierarchyTypes();
  }, []);

  // Handle initial selection when provided
  useEffect(() => {
    if (initialSelection && hierarchyTypes.length > 0) {
      // Reset the component state first
      setSelectedLevels([]);
      setLevelOptions({ 1: [] });

      // Then load the initial selection
      if (initialSelection.selectionValue) {
        setTimeout(() => {
          loadInitialSelection();
        }, 100);
      }
    }
  }, [initialSelection, hierarchyTypes]);

  const loadHierarchyTypes = async () => {
    try {
      console.log("=== LOADING HIERARCHY TYPES ===");
      const types = await hierarchyService.getHierarchyTypes();
      console.log("Raw hierarchy types response:", types);
      console.log("Number of types loaded:", types?.length || 0);

      if (!types || types.length === 0) {
        console.warn("No hierarchy types found!");
        toast({
          title: "Warning",
          description:
            "No hierarchy types configured. Please configure hierarchy types in admin.",
          variant: "destructive",
        });
        return;
      }

      // Sort by level (use ItemLevel field)
      const sortedTypes = types.sort(
        (a, b) => (a.ItemLevel || 0) - (b.ItemLevel || 0)
      );
      console.log(
        "Sorted hierarchy types:",
        sortedTypes.map((t) => ({
          Name: t.Name,
          ItemLevel: t.ItemLevel,
          UID: t.UID,
        }))
      );
      setHierarchyTypes(sortedTypes);

      // Load options for the first level
      if (sortedTypes.length > 0) {
        console.log(
          `Loading first level options for type: ${sortedTypes[0].Name} (${sortedTypes[0].UID})`
        );
        await loadLevelOptions(1, sortedTypes[0].UID);
      }
    } catch (error) {
      console.error("=== HIERARCHY TYPES ERROR ===");
      console.error("Error loading hierarchy types:", error);
      toast({
        title: "Error",
        description:
          "Failed to load hierarchy types. Check console for details.",
        variant: "destructive",
      });
    } finally {
      setLoadingTypes(false);
    }
  };

  const loadLevelOptions = async (
    level: number,
    typeUID: string,
    parentUID?: string
  ) => {
    try {
      setLoadingLevel((prev) => ({ ...prev, [level]: true }));

      console.log(`=== LOADING LEVEL ${level} OPTIONS ===`);
      console.log("TypeUID:", typeUID);
      console.log("ParentUID:", parentUID);

      let skuGroups: SKUGroup[] = [];

      console.log("Calling skuGroupService.getSKUGroupsByTypeUID...");
      // Always get SKU groups by type
      skuGroups = await skuGroupService.getSKUGroupsByTypeUID(typeUID);
      console.log(`Raw SKU groups response: ${skuGroups.length} groups`);
      console.log(
        "Sample groups:",
        skuGroups
          .slice(0, 3)
          .map((g) => ({
            Code: g.Code,
            Name: g.Name,
            ParentUID: g.ParentUID,
            UID: g.UID,
          }))
      );

      // If we have a parent, filter by parent UID
      if (parentUID) {
        console.log(`Filtering by parent UID: ${parentUID}`);
        const beforeFilter = skuGroups.length;
        skuGroups = skuGroups.filter((g) => g.ParentUID === parentUID);
        console.log(
          `After parent filter: ${beforeFilter} -> ${skuGroups.length} groups`
        );
      } else {
        // For root level, get items with no parent or empty parent
        console.log("Filtering for root level items (no parent)");
        const beforeFilter = skuGroups.length;
        skuGroups = skuGroups.filter((g) => !g.ParentUID || g.ParentUID === "");
        console.log(
          `After root filter: ${beforeFilter} -> ${skuGroups.length} groups`
        );
      }

      console.log(
        `Final filtered ${skuGroups.length} SKU groups for level ${level}:`,
        skuGroups.map((g) => ({ Code: g.Code, Name: g.Name }))
      );

      // Convert SKUGroup to HierarchyOption format
      const options: HierarchyOption[] = skuGroups.map((group) => {
        console.log("Mapping group:", group);
        return {
          code: group.Code,
          value: group.Name,
          type: hierarchyTypes[level - 1]?.Name || "Unknown",
          UID: group.UID,
        };
      });

      console.log(`Converted to ${options.length} options:`, options);

      setLevelOptions((prev) => ({
        ...prev,
        [level]: options,
      }));

      return options;
    } catch (error) {
      console.error(`Error loading options for level ${level}:`, error);
      const levelType = hierarchyTypes[level - 1];
      toast({
        title: "Error",
        description: `Failed to load ${levelType?.Name || "level"} options`,
        variant: "destructive",
      });
      return [];
    } finally {
      setLoadingLevel((prev) => ({ ...prev, [level]: false }));
    }
  };

  const loadInitialSelection = async () => {
    if (!initialSelection || !initialSelection.selectionValue) return;

    console.log("=== LOADING INITIAL SELECTION ===");
    console.log("Initial selection:", initialSelection);

    try {
      // First, find which level contains our target selection value
      let targetLevel = -1;
      let targetGroup: SKUGroup | null = null;

      for (let typeIndex = 0; typeIndex < hierarchyTypes.length; typeIndex++) {
        const hierarchyType = hierarchyTypes[typeIndex];
        const level = typeIndex + 1;

        console.log(
          `Searching level ${level} (${hierarchyType.Name}) for: ${initialSelection.selectionValue}`
        );

        try {
          const skuGroups = await skuGroupService.getSKUGroupsByTypeUID(
            hierarchyType.UID
          );

          const matchingGroup = skuGroups.find(
            (group) =>
              group.Code === initialSelection.selectionValue ||
              group.Name === initialSelection.selectionValue ||
              group.UID === initialSelection.selectionValue
          );

          if (matchingGroup) {
            console.log(`Found target at level ${level}:`, matchingGroup);
            targetLevel = level;
            targetGroup = matchingGroup;
            break;
          }
        } catch (error) {
          console.error(`Error searching level ${level}:`, error);
        }
      }

      if (targetLevel === -1 || !targetGroup) {
        console.log("Target selection not found in any level");
        return;
      }

      // Now build the path from root to target
      console.log(`Building path to level ${targetLevel}`);

      // If target is at level 1, just select it directly
      if (targetLevel === 1) {
        await loadLevelOptions(1, hierarchyTypes[0].UID);
        setTimeout(() => {
          handleLevelSelection(1, targetGroup!.Code);
        }, 100);
        return;
      }

      // For deeper levels, we need to find the parent chain
      await buildParentChain(targetGroup, targetLevel);
    } catch (error) {
      console.error("Error loading initial selection:", error);
    }
  };

  const buildParentChain = async (
    targetGroup: SKUGroup,
    targetLevel: number
  ) => {
    console.log("Building parent chain for:", targetGroup);

    // Build the chain from target back to root
    const chain: { group: SKUGroup; level: number }[] = [];
    let currentGroup = targetGroup;
    let currentLevel = targetLevel;

    // Add the target to the chain
    chain.unshift({ group: currentGroup, level: currentLevel });

    // Work backwards to find parents
    while (currentLevel > 1 && currentGroup.ParentUID) {
      const parentLevel = currentLevel - 1;
      const parentTypeUID = hierarchyTypes[parentLevel - 1].UID;

      try {
        const parentGroups = await skuGroupService.getSKUGroupsByTypeUID(
          parentTypeUID
        );
        const parentGroup = parentGroups.find(
          (g) => g.UID === currentGroup.ParentUID
        );

        if (parentGroup) {
          chain.unshift({ group: parentGroup, level: parentLevel });
          currentGroup = parentGroup;
          currentLevel = parentLevel;
          console.log(`Found parent at level ${parentLevel}:`, parentGroup);
        } else {
          console.log(`Parent not found for level ${parentLevel}`);
          break;
        }
      } catch (error) {
        console.error(`Error finding parent for level ${parentLevel}:`, error);
        break;
      }
    }

    console.log("Complete chain:", chain);

    // Now select each item in the chain sequentially
    for (let i = 0; i < chain.length; i++) {
      const { group, level } = chain[i];
      const typeUID = hierarchyTypes[level - 1].UID;
      const parentUID = i > 0 ? chain[i - 1].group.UID : undefined;

      console.log(
        `Selecting level ${level}: ${group.Code} (parent: ${parentUID})`
      );

      // Load options for this level
      await loadLevelOptions(level, typeUID, parentUID);

      // Wait a bit for options to load, then select
      await new Promise((resolve) => {
        setTimeout(() => {
          handleLevelSelection(level, group.Code);
          resolve(void 0);
        }, 200);
      });
    }
  };

  const handleLevelSelection = async (level: number, value: string) => {
    console.log(`=== LEVEL ${level} SELECTION ===`);
    console.log("Selected value:", value);

    const levelType = hierarchyTypes[level - 1];
    if (!levelType) return;

    const options = levelOptions[level] || [];
    const selectedOption = options.find(
      (opt) => opt.Code === value || opt.Value === value || opt.UID === value
    );

    if (!selectedOption) {
      console.error("Selected option not found:", value);
      return;
    }

    console.log("Selected option:", selectedOption);

    // Create the hierarchy level object
    const newLevel: HierarchyLevel = {
      level,
      type: levelType.Name,
      code: selectedOption.Code || selectedOption.code,
      value:
        selectedOption.Value ||
        selectedOption.value ||
        selectedOption.Name ||
        selectedOption.Code,
      uid: selectedOption.UID || selectedOption.uid,
    };

    // Update selected levels (clear any levels after this one)
    const newSelectedLevels = [...selectedLevels.slice(0, level - 1), newLevel];
    setSelectedLevels(newSelectedLevels);

    // Clear options for levels after this one
    const newLevelOptions = { ...levelOptions };
    for (let i = level + 1; i <= hierarchyTypes.length; i++) {
      delete newLevelOptions[i];
    }
    setLevelOptions(newLevelOptions);

    // Load next level options if available
    if (level < hierarchyTypes.length) {
      const nextLevelType = hierarchyTypes[level];
      await loadLevelOptions(
        level + 1,
        nextLevelType.UID,
        selectedOption.UID || selectedOption.Code
      );
    }

    // Notify parent component
    const lastLevel = newSelectedLevels[newSelectedLevels.length - 1];
    console.log("=== NOTIFYING PARENT COMPONENT ===");
    console.log("Last level data:", lastLevel);
    console.log("Sending selectionType:", lastLevel.type);
    console.log("Sending selectionValue (code):", lastLevel.code);
    console.log("Display value:", lastLevel.value);

    onSelectionChange({
      selectionType: lastLevel.type,
      selectionValue: lastLevel.code, // Use code instead of value
      fullHierarchy: newSelectedLevels,
    });

    console.log("Parent callback called with:", {
      selectionType: lastLevel.type,
      selectionValue: lastLevel.code,
      fullHierarchy: newSelectedLevels,
    });
  };

  if (loadingTypes) {
    return (
      <Card className="p-4">
        <div className="flex items-center gap-2">
          <Loader2 className="h-4 w-4 animate-spin" />
          <span className="text-sm text-muted-foreground">
            Loading hierarchy...
          </span>
        </div>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {hierarchyTypes.map((type, index) => {
        const level = index + 1;
        const options = levelOptions[level] || [];
        const selectedValue = selectedLevels[index]?.code || "";
        const isLoading = loadingLevel[level];

        // Only show this level if:
        // 1. It's the first level, OR
        // 2. The previous level has a selection
        const showLevel = level === 1 || selectedLevels.length >= level - 1;

        if (!showLevel) return null;

        return (
          <div key={level} className="space-y-2">
            <Label htmlFor={`level-${level}`}>
              {type.Name || `Level ${level}`}
              {level === 1 && <span className="text-red-500 ml-1">*</span>}
            </Label>

            <Select
              value={selectedValue}
              onValueChange={(value) => handleLevelSelection(level, value)}
              disabled={isLoading || (level > 1 && !selectedLevels[level - 2])}
            >
              <SelectTrigger id={`level-${level}`}>
                {isLoading ? (
                  <div className="flex items-center gap-2">
                    <Loader2 className="h-4 w-4 animate-spin" />
                    <span>Loading...</span>
                  </div>
                ) : (
                  <SelectValue
                    placeholder={`Select ${type.Name || `Level ${level}`}`}
                  />
                )}
              </SelectTrigger>
              <SelectContent>
                {console.log(
                  `Rendering ${options.length} options for level ${level}:`,
                  options
                )}
                {options.length === 0 && (
                  <div className="p-2 text-sm text-muted-foreground">
                    No options available
                  </div>
                )}
                {options.map((option) => {
                  const displayText =
                    option.value ||
                    option.Value ||
                    option.Name ||
                    option.name ||
                    option.code ||
                    option.Code ||
                    "Unknown";
                  console.log(
                    "Rendering option:",
                    option,
                    "Display text:",
                    displayText
                  );
                  return (
                    <SelectItem
                      key={
                        option.UID || option.uid || option.Code || option.code
                      }
                      value={option.Code || option.code}
                    >
                      {displayText}
                      {option.Code &&
                        displayText &&
                        option.Code !== displayText && (
                          <span className="text-muted-foreground ml-2">
                            ({option.Code})
                          </span>
                        )}
                    </SelectItem>
                  );
                })}
              </SelectContent>
            </Select>
          </div>
        );
      })}

      {/* Display current selection */}
      {selectedLevels.length > 0 && (
        <Card className="p-3 bg-muted/50">
          <div className="text-sm space-y-1">
            <div>
              <span className="font-medium">Selection Type:</span>{" "}
              {selectedLevels[selectedLevels.length - 1].type}
            </div>
            <div>
              <span className="font-medium">Selection Value:</span>{" "}
              {selectedLevels[selectedLevels.length - 1].value}
            </div>
            <div>
              <span className="font-medium">Selection Code:</span>{" "}
              {selectedLevels[selectedLevels.length - 1].code}
            </div>
          </div>
        </Card>
      )}
    </div>
  );
}
