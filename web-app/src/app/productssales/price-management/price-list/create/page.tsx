"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
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
import { ArrowLeft, Save, Building } from "lucide-react";
import { useToast } from "@/components/ui/use-toast";
import {
  skuPriceService,
  ISKUPriceList,
} from "@/services/sku/sku-price.service";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { useOrganizationHierarchy } from "@/hooks/useOrganizationHierarchy";
import { organizationService } from "@/services/organizationService";

export default function CreatePriceListPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);

  // Organization hierarchy hook - same as Create Product page
  const {
    orgLevels,
    selectedOrgs,
    initializeHierarchy,
    selectOrganization,
    resetHierarchy,
    finalSelectedOrganization,
    hasSelection,
  } = useOrganizationHierarchy();

  // State for stores (distribution channels)
  const [stores, setStores] = useState<any[]>([]);
  const [loadingStores, setLoadingStores] = useState(false);

  const [formData, setFormData] = useState<ISKUPriceList>({
    UID: `PL_${Date.now()}`,
    Code: "",
    Name: "",
    Type: "STANDARD",
    OrgUID: "",
    DistributionChannelUID: "",
    Priority: 1,
    SelectionGroup: "All",
    SelectionType: "All",
    SelectionUID: "",
    IsActive: true,
    Status: "Draft",
    ValidFrom: new Date().toISOString(),
    ValidUpto: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString(),
    CreatedBy: "ADMIN",
    ModifiedBy: "ADMIN",
  });

  // Load organization hierarchy on mount
  useEffect(() => {
    const loadOrganizationHierarchy = async () => {
      try {
        const [orgTypesResult, orgsResult] = await Promise.all([
          organizationService.getOrganizationTypes(),
          organizationService.getOrganizations(1, 1000),
        ]);

        if (orgsResult.data.length > 0 && orgTypesResult.length > 0) {
          initializeHierarchy(orgsResult.data, orgTypesResult);
        }
      } catch (error) {
        console.error("Failed to load organization hierarchy:", error);
        toast({
          title: "Warning",
          description: "Failed to load organization data",
          variant: "default",
        });
      }
    };

    loadOrganizationHierarchy();
  }, []);

  // Update OrgUID when organization is selected
  useEffect(() => {
    if (finalSelectedOrganization) {
      setFormData((prev) => ({
        ...prev,
        OrgUID: finalSelectedOrganization,
      }));
    }
  }, [finalSelectedOrganization]);

  // Load stores for distribution channel dropdown based on selected organization
  useEffect(() => {
    const loadStores = async () => {
      // Only load stores if an organization is selected
      if (!finalSelectedOrganization) {
        setStores([]);
        return;
      }

      try {
        setLoadingStores(true);

        // Clear previous distribution channel selection when org changes
        setFormData((prev) => ({
          ...prev,
          DistributionChannelUID: "",
        }));

        const response = await fetch(
          `${
            process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
          }/Store/SelectAllStore`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            },
            body: JSON.stringify({
              PageNumber: 1,
              PageSize: 500,
              FilterCriterias: [
                {
                  Name: "FranchiseeOrgUID",
                  Value: finalSelectedOrganization,
                },
              ],
              SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }],
              IsCountRequired: true,
            }),
          }
        );

        if (response.ok) {
          const result = await response.json();
          console.log("Store API Response:", result);

          // Handle the response structure
          let storeData = [];
          if (result.Data?.PagedData && Array.isArray(result.Data.PagedData)) {
            storeData = result.Data.PagedData;
          } else if (result.PagedData && Array.isArray(result.PagedData)) {
            storeData = result.PagedData;
          } else if (result.Data && Array.isArray(result.Data)) {
            storeData = result.Data;
          }

          // The API should already filter by FranchiseeOrgUID, so we don't need additional filtering
          setStores(storeData);

          console.log(
            `✅ Loaded ${storeData.length} stores for organization: ${finalSelectedOrganization}`
          );

          // If no stores found, try loading without filter as fallback
          if (storeData.length === 0) {
            console.log(
              "No stores found with FranchiseeOrgUID filter. Trying without filter..."
            );

            const fallbackResponse = await fetch(
              `${
                process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
              }/Store/SelectAllStore`,
              {
                method: "POST",
                headers: {
                  "Content-Type": "application/json",
                  Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
                },
                body: JSON.stringify({
                  PageNumber: 1,
                  PageSize: 100,
                  FilterCriterias: [],
                  SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }],
                  IsCountRequired: false,
                }),
              }
            );

            if (fallbackResponse.ok) {
              const fallbackResult = await fallbackResponse.json();
              let fallbackStoreData = [];
              if (
                fallbackResult.Data?.PagedData &&
                Array.isArray(fallbackResult.Data.PagedData)
              ) {
                fallbackStoreData = fallbackResult.Data.PagedData;
              } else if (
                fallbackResult.PagedData &&
                Array.isArray(fallbackResult.PagedData)
              ) {
                fallbackStoreData = fallbackResult.PagedData;
              }

              if (fallbackStoreData.length > 0) {
                console.log(
                  `✅ Loaded ${fallbackStoreData.length} stores without filter (fallback)`
                );
                setStores(fallbackStoreData);
              }
            }
          }
        } else {
          console.error(
            "Failed to fetch stores:",
            response.status,
            response.statusText
          );
          setStores([]);
        }
      } catch (error) {
        console.error("Failed to load stores:", error);
        setStores([]);
      } finally {
        setLoadingStores(false);
      }
    };

    loadStores();
  }, [finalSelectedOrganization]); // Reload when organization selection changes

  const handleOrganizationSelection = (levelIndex: number, value: string) => {
    selectOrganization(levelIndex, value);

    if (value) {
      setFormData((prev) => ({
        ...prev,
        OrgUID: finalSelectedOrganization || value,
      }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.Code || !formData.Name) {
      toast({
        title: "Error",
        description: "Code and Name are required",
        variant: "destructive",
      });
      return;
    }

    if (!hasSelection) {
      toast({
        title: "Validation Error",
        description: "Please select an organization",
        variant: "destructive",
      });
      return;
    }

    setLoading(true);
    try {
      await skuPriceService.createPriceList({
        ...formData,
        OrgUID: finalSelectedOrganization || formData.OrgUID,
        CreatedTime: new Date().toISOString(),
        ModifiedTime: new Date().toISOString(),
        ServerAddTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
      });

      toast({
        title: "Success",
        description: "Price list created successfully",
      });

      router.push("/productssales/price-management/lists");
    } catch (error: any) {
      toast({
        title: "Error",
        description:
          error.response?.data?.ErrorMessage || "Failed to create price list",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              Create Price List
            </h1>
            <p className="text-muted-foreground">
              Create a new price list for managing product prices
            </p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6">
          {/* Organization Hierarchy - Dynamic selection like Create Product */}
          <Card className="border-blue-200">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Building className="h-5 w-5 text-blue-600" />
                Organization Hierarchy
              </CardTitle>
              <CardDescription>
                Select the organization hierarchy for this price list
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {orgLevels.map((level, index) => (
                <div
                  key={`${level.orgTypeUID}-${level.level}`}
                  className="space-y-2"
                >
                  <Label htmlFor={`org-level-${index}`} className="font-medium">
                    {level.dynamicLabel || level.orgTypeName}
                  </Label>
                  <Select
                    value={level.selectedOrgUID || ""}
                    onValueChange={(value) =>
                      handleOrganizationSelection(index, value)
                    }
                  >
                    <SelectTrigger>
                      <SelectValue
                        placeholder={`Select ${level.orgTypeName.toLowerCase()}`}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {level.organizations.map((org) => (
                        <SelectItem key={org.UID} value={org.UID}>
                          {org.Code ? `${org.Name} (${org.Code})` : org.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              ))}

              {hasSelection && (
                <div className="p-3 bg-green-50 rounded-md">
                  <p className="text-sm text-green-800">
                    ✓ Selected Organization: {finalSelectedOrganization}
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Basic Information</h3>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="code">Code *</Label>
                <Input
                  id="code"
                  value={formData.Code}
                  onChange={(e) =>
                    setFormData({ ...formData, Code: e.target.value })
                  }
                  placeholder="e.g., STD001"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Unique identifier for this price list
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="name">Name *</Label>
                <Input
                  id="name"
                  value={formData.Name}
                  onChange={(e) =>
                    setFormData({ ...formData, Name: e.target.value })
                  }
                  placeholder="e.g., Standard Price List"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Display name for this price list
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="type">Type</Label>
                <Select
                  value={formData.Type}
                  onValueChange={(value) =>
                    setFormData({ ...formData, Type: value })
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="STANDARD">Standard</SelectItem>
                    <SelectItem value="PROMOTIONAL">Promotional</SelectItem>
                    <SelectItem value="SEASONAL">Seasonal</SelectItem>
                    <SelectItem value="DISCOUNT">Discount</SelectItem>
                    <SelectItem value="WHOLESALE">Wholesale</SelectItem>
                    <SelectItem value="RETAIL">Retail</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="priority">Priority</Label>
                <Input
                  id="priority"
                  type="number"
                  min="1"
                  value={formData.Priority}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      Priority: parseInt(e.target.value) || 1,
                    })
                  }
                  placeholder="1"
                />
                <p className="text-xs text-muted-foreground">
                  Lower number = higher priority
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Distribution Channel</h3>
            <div className="space-y-2">
              <Label htmlFor="distChannel">
                Select Store/Distribution Channel
              </Label>
              {!hasSelection ? (
                <div className="p-3 bg-yellow-50 border border-yellow-200 rounded-md">
                  <p className="text-sm text-yellow-800">
                    ⚠️ Please select an organization first to view available
                    stores
                  </p>
                </div>
              ) : (
                <>
                  <Select
                    value={formData.DistributionChannelUID}
                    onValueChange={(value) =>
                      setFormData({
                        ...formData,
                        DistributionChannelUID: value,
                      })
                    }
                    disabled={loadingStores || !hasSelection}
                  >
                    <SelectTrigger>
                      <SelectValue
                        placeholder={
                          !hasSelection
                            ? "Select organization first"
                            : loadingStores
                            ? "Loading stores..."
                            : stores.length === 0
                            ? "No stores for this organization"
                            : "Select store/channel"
                        }
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingStores ? (
                        <SelectItem value="loading" disabled>
                          Loading stores for selected organization...
                        </SelectItem>
                      ) : stores.length > 0 ? (
                        stores.map((store) => (
                          <SelectItem key={store.UID} value={store.UID}>
                            {store.Name} {store.Code ? `(${store.Code})` : ""}
                            {store.Type ? ` - ${store.Type}` : ""}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>
                          No stores available for the selected organization
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    {stores.length > 0
                      ? `${stores.length} store(s) available for the selected organization`
                      : "No stores found for the selected organization"}
                  </p>
                </>
              )}
            </div>
          </Card>

          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Selection Criteria</h3>
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="selectionGroup">Selection Group</Label>
                <Select
                  value={formData.SelectionGroup}
                  onValueChange={(value) => {
                    setFormData({
                      ...formData,
                      SelectionGroup: value,
                      SelectionType:
                        value === "All" ? "All" : formData.SelectionType,
                      SelectionUID:
                        value === "All" ? "" : formData.SelectionUID,
                    });
                  }}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select group" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="All">
                      All (Applies to everyone)
                    </SelectItem>
                    <SelectItem value="Organization">Organization</SelectItem>
                    <SelectItem value="Store">Store</SelectItem>
                    <SelectItem value="Customer">Customer</SelectItem>
                    <SelectItem value="Region">Region</SelectItem>
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Defines who this price list applies to
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="selectionType">Selection Type</Label>
                <Select
                  value={formData.SelectionType}
                  onValueChange={(value) =>
                    setFormData({ ...formData, SelectionType: value })
                  }
                  disabled={formData.SelectionGroup === "All"}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="All">All</SelectItem>
                    <SelectItem value="Specific">Specific</SelectItem>
                    <SelectItem value="Group">Group</SelectItem>
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  {formData.SelectionGroup === "All"
                    ? 'Not applicable when group is "All"'
                    : "How to apply the selection"}
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="selectionUID">
                  {formData.SelectionGroup === "Organization"
                    ? "Organization UID"
                    : formData.SelectionGroup === "Store"
                    ? "Store UID"
                    : formData.SelectionGroup === "Customer"
                    ? "Customer UID"
                    : formData.SelectionGroup === "Region"
                    ? "Region UID"
                    : "Selection UID"}
                </Label>
                {formData.SelectionGroup === "Store" &&
                stores.length > 0 &&
                formData.SelectionType !== "All" ? (
                  <Select
                    value={formData.SelectionUID}
                    onValueChange={(value) =>
                      setFormData({ ...formData, SelectionUID: value })
                    }
                    disabled={
                      formData.SelectionGroup === "All" ||
                      formData.SelectionType === "All"
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select store" />
                    </SelectTrigger>
                    <SelectContent>
                      {stores.map((store) => (
                        <SelectItem key={store.UID} value={store.UID}>
                          {store.Name} {store.Code ? `(${store.Code})` : ""}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                ) : formData.SelectionGroup === "Organization" &&
                  hasSelection ? (
                  <Input
                    id="selectionUID"
                    value={finalSelectedOrganization || ""}
                    disabled
                    placeholder="Organization UID"
                  />
                ) : (
                  <Input
                    id="selectionUID"
                    value={formData.SelectionUID}
                    onChange={(e) =>
                      setFormData({ ...formData, SelectionUID: e.target.value })
                    }
                    placeholder={
                      formData.SelectionGroup === "All"
                        ? "Not applicable"
                        : `Enter ${formData.SelectionGroup} UID`
                    }
                    disabled={
                      formData.SelectionGroup === "All" ||
                      formData.SelectionType === "All"
                    }
                  />
                )}
                <p className="text-xs text-muted-foreground">
                  {formData.SelectionGroup === "All"
                    ? 'Not applicable when group is "All"'
                    : `Specific ${(
                        formData.SelectionGroup || ""
                      ).toLowerCase()} this applies to`}
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Validity Period</h3>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="validFrom">Valid From *</Label>
                <Input
                  id="validFrom"
                  type="datetime-local"
                  value={formData.ValidFrom?.toString().slice(0, 16)}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      ValidFrom: new Date(e.target.value).toISOString(),
                    })
                  }
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="validUpto">Valid To *</Label>
                <Input
                  id="validUpto"
                  type="datetime-local"
                  value={formData.ValidUpto?.toString().slice(0, 16)}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      ValidUpto: new Date(e.target.value).toISOString(),
                    })
                  }
                  required
                />
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Status & Settings</h3>
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="status">Status</Label>
                  <Select
                    value={formData.Status}
                    onValueChange={(value) =>
                      setFormData({ ...formData, Status: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Draft">Draft</SelectItem>
                      <SelectItem value="Published">Published</SelectItem>
                      <SelectItem value="Archived">Archived</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <Label>Active</Label>
                  <p className="text-sm text-muted-foreground">
                    Enable this price list for use
                  </p>
                </div>
                <Switch
                  checked={formData.IsActive}
                  onCheckedChange={(checked) =>
                    setFormData({ ...formData, IsActive: checked })
                  }
                />
              </div>
            </div>
          </Card>

          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => router.back()}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? (
                <>
                  <span className="animate-spin mr-2">⏳</span>
                  Creating...
                </>
              ) : (
                <>
                  <Save className="h-4 w-4 mr-2" />
                  Create Price List
                </>
              )}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
}
