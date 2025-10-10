"use client";

import React, { useState, useEffect } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { 
  Building2, 
  Info,
  Check,
  X,
  RefreshCw,
  RotateCcw
} from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { 
  organizationService, 
  Organization, 
  OrgType 
} from "@/services/organizationService";
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
  getOrganizationDisplayName
} from "@/utils/organizationHierarchyUtils";
import { useToast } from "@/components/ui/use-toast";
import { cn } from "@/lib/utils";

interface CascadingOrganizationSelectorProps {
  selectedOrgs?: string[];
  onSelectionChange: (selectedOrgs: string[], finalOrgUID?: string, hierarchyData?: any) => void;
  multiSelect?: boolean;
  placeholder?: string;
  showReset?: boolean;
  compact?: boolean;
}

export default function CascadingOrganizationSelector({
  selectedOrgs = [],
  onSelectionChange,
  multiSelect = false,
  placeholder = "Select organizations",
  showReset = true,
  compact = false
}: CascadingOrganizationSelectorProps) {
  const { toast } = useToast();
  
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]);
  const [selectedOrgsState, setSelectedOrgsState] = useState<string[]>(selectedOrgs);
  const [loading, setLoading] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    setSelectedOrgsState(selectedOrgs);
  }, [selectedOrgs]);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [orgsResult, typesResult] = await Promise.all([
        organizationService.getOrganizations(1, 1000), // Get all organizations for cascading
        organizationService.getOrganizationTypes()
      ]);
      
      setOrganizations(orgsResult.data);
      setOrgTypes(typesResult);
      
      // Initialize with root level organizations using utility
      const initialLevels = initializeOrganizationHierarchy(
        orgsResult.data,
        typesResult
      );
      setOrgLevels(initialLevels);
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch organization data",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const handleOrganizationSelect = (levelIndex: number, value: string) => {
    if (!value) return;

    // Use utility function to handle organization selection
    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      orgLevels,
      selectedOrgsState,
      organizations,
      orgTypes
    );

    setOrgLevels(updatedLevels);
    setSelectedOrgsState(updatedSelectedOrgs);

    // Get the final selected organization
    const finalSelectedOrg = getFinalSelectedOrganization(updatedSelectedOrgs);
    
    // Build hierarchy data for review section
    const hierarchyData: any = {};
    updatedLevels.forEach((level, idx) => {
      if (level.selectedOrgUID) {
        const selectedOrg = organizations.find(org => org.UID === level.selectedOrgUID);
        if (selectedOrg) {
          const levelName = level.dynamicLabel || level.orgTypeName || `level${idx + 1}`;
          hierarchyData[levelName] = [{
            uid: selectedOrg.UID,
            name: selectedOrg.Name,
            code: selectedOrg.Code,
            type: selectedOrg.OrgTypeName
          }];
        }
      }
    });
    
    // For multi-select, pass all selected orgs; for single-select, pass the final one
    if (multiSelect) {
      onSelectionChange(updatedSelectedOrgs, finalSelectedOrg, hierarchyData);
    } else {
      onSelectionChange(finalSelectedOrg ? [finalSelectedOrg] : [], finalSelectedOrg, hierarchyData);
    }
  };

  const handleReset = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(
      organizations,
      orgTypes
    );
    
    setOrgLevels(resetLevels);
    setSelectedOrgsState(resetSelectedOrgs);
    onSelectionChange(resetSelectedOrgs);
  };

  // Get hierarchy stats
  const getHierarchyStats = () => {
    return {
      totalLevels: orgLevels.length,
      selectedLevels: selectedOrgsState.length,
      totalOrgs: organizations.length
    };
  };

  const stats = getHierarchyStats();
  const finalSelectedOrg = organizations.find(org => 
    org.UID === getFinalSelectedOrganization(selectedOrgsState)
  );

  if (compact) {
    return (
      <div className="space-y-3">
        {/* Compact Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Building2 className="h-4 w-4 text-gray-600" />
            <span className="text-sm font-medium">Organization</span>
            {stats.selectedLevels > 0 && (
              <Badge variant="secondary" className="text-xs">
                {stats.selectedLevels} level{stats.selectedLevels !== 1 ? 's' : ''}
              </Badge>
            )}
          </div>
          {showReset && selectedOrgsState.length > 0 && (
            <Button variant="ghost" size="sm" onClick={handleReset}>
              <RotateCcw className="h-3 w-3" />
            </Button>
          )}
        </div>

        {/* Compact Selection Display */}
        {finalSelectedOrg && (
          <div className="p-2 bg-blue-50 border border-blue-200 rounded-lg">
            <div className="flex items-center gap-2">
              <Check className="h-4 w-4 text-blue-600" />
              <span className="text-sm font-medium text-blue-900">
                {getOrganizationDisplayName(finalSelectedOrg)}
              </span>
              <Badge variant="outline" className="text-xs">
                {finalSelectedOrg.OrgTypeName}
              </Badge>
            </div>
          </div>
        )}

        {/* Compact Cascading Selectors */}
        <Button
          variant="outline"
          onClick={() => setIsExpanded(!isExpanded)}
          className="w-full justify-between"
        >
          {finalSelectedOrg ? "Change Selection" : "Select Organization"}
          {isExpanded ? <X className="h-4 w-4" /> : <Building2 className="h-4 w-4" />}
        </Button>

        {isExpanded && (
          <Card>
            <CardContent className="p-4 space-y-3">
              {loading ? (
                <div className="space-y-2">
                  {[...Array(2)].map((_, i) => (
                    <div key={i}>
                      <Skeleton className="h-4 w-20 mb-1" />
                      <Skeleton className="h-10 w-full" />
                    </div>
                  ))}
                </div>
              ) : (
                <>
                  {orgLevels.map((level, index) => (
                    <div key={`${level.orgTypeUID}_${index}`}>
                      <Label className="text-sm font-medium text-gray-700 mb-1">
                        {level.dynamicLabel || level.orgTypeName}
                        {index === 0 && <span className="text-red-500 ml-1">*</span>}
                      </Label>
                      <Select
                        value={level.selectedOrgUID || ""}
                        onValueChange={(value) => handleOrganizationSelect(index, value)}
                      >
                        <SelectTrigger className="h-10">
                          <SelectValue
                            placeholder={`Select ${(level.dynamicLabel || level.orgTypeName).toLowerCase()}`}
                          />
                        </SelectTrigger>
                        <SelectContent>
                          {level.organizations.map((org) => (
                            <SelectItem key={org.UID} value={org.UID}>
                              <div className="flex items-center justify-between w-full">
                                <span>{org.Name}</span>
                                {org.Code && (
                                  <span className="text-muted-foreground ml-2 text-xs">
                                    ({org.Code})
                                  </span>
                                )}
                              </div>
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  ))}
                </>
              )}
            </CardContent>
          </Card>
        )}
      </div>
    );
  }

  return (
    <Card className="w-full">
      <CardContent className="p-4">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div>
            <h4 className="font-semibold text-gray-900">Organization Hierarchy</h4>
            <p className="text-sm text-gray-600">
              {stats.totalOrgs} organizations with cascading selection
            </p>
          </div>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={fetchData}
              disabled={loading}
            >
              <RefreshCw className={cn("h-4 w-4", loading && "animate-spin")} />
            </Button>
            {showReset && selectedOrgsState.length > 0 && (
              <Button
                variant="outline"
                size="sm"
                onClick={handleReset}
              >
                <RotateCcw className="h-4 w-4 mr-1" />
                Reset
              </Button>
            )}
          </div>
        </div>

        {/* Selection Summary */}
        {selectedOrgsState.length > 0 && (
          <div className="mb-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
            <div className="flex items-center gap-2 mb-2">
              <Check className="h-4 w-4 text-blue-600" />
              <span className="text-sm font-medium text-blue-900">
                Selection: {selectedOrgsState.length} level{selectedOrgsState.length !== 1 ? 's' : ''}
              </span>
            </div>
            
            {finalSelectedOrg && (
              <div className="flex items-center gap-2">
                <span className="text-sm text-blue-800">
                  Final: <strong>{getOrganizationDisplayName(finalSelectedOrg)}</strong>
                </span>
                <Badge variant="outline" className="text-xs">
                  {finalSelectedOrg.OrgTypeName}
                </Badge>
              </div>
            )}
          </div>
        )}

        {/* Cascading Organization Selectors */}
        <div className="space-y-4">
          {loading ? (
            <div className="space-y-3">
              {[...Array(3)].map((_, i) => (
                <div key={i}>
                  <Skeleton className="h-4 w-32 mb-2" />
                  <Skeleton className="h-10 w-full" />
                </div>
              ))}
            </div>
          ) : orgLevels.length === 0 ? (
            <div className="p-8 text-center">
              <Building2 className="h-12 w-12 text-gray-300 mx-auto mb-4" />
              <p className="text-gray-500 font-medium">No organizations found</p>
              <p className="text-sm text-gray-400 mt-1">
                No organization structure available
              </p>
            </div>
          ) : (
            <>
              {/* Render all levels dynamically */}
              {orgLevels.map((level, index) => (
                <div key={`${level.orgTypeUID}_${index}`}>
                  <Label className="text-sm font-medium text-gray-700 mb-2 flex items-center gap-2">
                    <Building2 className="h-4 w-4" />
                    {level.dynamicLabel || level.orgTypeName}
                    {index === 0 && <span className="text-red-500">*</span>}
                    <Badge variant="secondary" className="text-xs">
                      Level {index + 1}
                    </Badge>
                  </Label>
                  
                  <Select
                    value={level.selectedOrgUID || ""}
                    onValueChange={(value) => handleOrganizationSelect(index, value)}
                  >
                    <SelectTrigger className="h-10">
                      <SelectValue
                        placeholder={`Select ${(level.dynamicLabel || level.orgTypeName).toLowerCase()}`}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {level.organizations.map((org) => (
                        <SelectItem key={org.UID} value={org.UID}>
                          <div className="flex items-center justify-between w-full">
                            <span>{org.Name}</span>
                            {org.Code && (
                              <span className="text-muted-foreground ml-2 text-xs">
                                ({org.Code})
                              </span>
                            )}
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  
                  {level.organizations.length > 0 && (
                    <p className="text-xs text-gray-500 mt-1">
                      {level.organizations.length} option{level.organizations.length !== 1 ? 's' : ''} available
                    </p>
                  )}
                </div>
              ))}

              {/* Status Info */}
              {orgLevels.length > 1 && selectedOrgsState.length === 0 && (
                <Alert className="bg-yellow-50 border-yellow-200">
                  <Info className="h-4 w-4 text-yellow-600" />
                  <AlertDescription className="text-yellow-800">
                    Multiple organization levels available. Select from the first level to continue cascading.
                  </AlertDescription>
                </Alert>
              )}

              {selectedOrgsState.length > 0 && (
                <Alert className="bg-green-50 border-green-200">
                  <Check className="h-4 w-4 text-green-600" />
                  <AlertDescription className="text-green-800">
                    <strong>Selection Complete:</strong> {selectedOrgsState.length} level{selectedOrgsState.length !== 1 ? 's' : ''} selected.
                    {finalSelectedOrg && (
                      <> Final organization: <strong>{getOrganizationDisplayName(finalSelectedOrg)}</strong></>
                    )}
                  </AlertDescription>
                </Alert>
              )}
            </>
          )}
        </div>

        {/* Info */}
        <Alert className="mt-4 bg-blue-50 border-blue-200">
          <Info className="h-4 w-4 text-blue-600" />
          <AlertDescription className="text-blue-800">
            <strong>Cascading Selection:</strong> Each selection reveals the next level in your organization hierarchy. 
            This ensures proper parent-child relationships are maintained.
          </AlertDescription>
        </Alert>
      </CardContent>
    </Card>
  );
}