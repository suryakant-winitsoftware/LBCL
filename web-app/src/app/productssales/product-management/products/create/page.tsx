"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  ArrowLeft,
  Save,
  Building,
  Plus,
  Trash2,
  ChevronDown,
  ChevronUp
} from "lucide-react";
import { useToast } from "@/components/ui/use-toast";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";

// Import enterprise services - This is the KEY difference
import {
  enterpriseSkuService,
  ISKUV1,
  CustomFieldData,
  EnterpriseSkuCreationRequest,
  UOMDetail
} from "@/services/sku/enterprise-sku.service";
import ProductAttributes from "@/components/sku/ProductAttributes";
// import ProductAttributesWithSKU from "@/components/sku/ProductAttributesWithSKU";
import { uomTypesService, UOMType } from "@/services/sku/uom-types.service";
import { useOrganizationHierarchy } from "@/hooks/useOrganizationHierarchy";
import { organizationService } from "@/services/organizationService";

export default function CreateSKUPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [showIndustryFields, setShowIndustryFields] = useState(false);
  const [showCustomFields, setShowCustomFields] = useState(false);

  // Organization hierarchy hook - Advanced feature
  const {
    orgLevels,
    initializeHierarchy,
    selectOrganization,
    finalSelectedOrganization,
    hasSelection
  } = useOrganizationHierarchy();

  // Product hierarchy - managed by ProductAttributesConfig component
  const [selectedAttributes, setSelectedAttributes] = useState<
    Array<{ type: string; code: string; value: string; uid?: string }>
  >([]);

  // Multi-select hierarchy and SKU products
  const [multiSelectHierarchy, setMultiSelectHierarchy] = useState<any[]>([]);
  const [selectedSKUProducts, setSelectedSKUProducts] = useState<any[]>([]);

  // UOM Type options (generic UOM types from uom_type table)
  const [baseUomOptions, setBaseUomOptions] = useState<UOMType[]>([]);
  const [outerUomOptions, setOuterUomOptions] = useState<UOMType[]>([]);
  const [loadingUOM, setLoadingUOM] = useState(true);

  // Supplier organization
  const HARDCODED_SUPPLIER_UID = "Supplier";

  // Custom fields - Enterprise feature
  const [customFields, setCustomFields] = useState<CustomFieldData[]>([]);

  // Additional state for organization-specific settings
  const [canBuy, setCanBuy] = useState(true);
  const [canSell, setCanSell] = useState(true);

  // UOM Details state
  const [uomDetails, setUomDetails] = useState<UOMDetail[]>([]);
  const [showUOMConfiguration, setShowUOMConfiguration] = useState(false);

  // ISKUV1 form data - Full enterprise model with all fields
  const [formData, setFormData] = useState<ISKUV1>({
    // Base SKU fields
    UID: "",
    Code: "",
    Name: "",
    ArabicName: "",
    AliasName: "",
    LongName: "",
    OrgUID: "",
    SupplierOrgUID: "",
    BaseUOM: "",
    OuterUOM: "",
    FromDate: new Date().toISOString().split("T")[0],
    ToDate: "2099-12-31",
    IsStockable: true,
    ParentUID: "",
    IsActive: true,
    IsThirdParty: false,
    CreatedBy: "ADMIN",
    ModifiedBy: "ADMIN",

    // Extended ISKUV1 fields - Enterprise specific
    L1: "",
    L2: "",
    L3: "",
    L4: "",
    L5: "",
    L6: "",
    ModelCode: "",
    Year: new Date().getFullYear(),
    Type: "",
    ProductType: "",
    Category: "",
    Tonnage: "",
    Capacity: "",
    StarRating: "",
    ProductCategoryId: 1,
    ProductCategoryName: "",
    ItemSeries: "",
    HSNCode: "",
    IsAvailableInApMaster: false,
    FilterKeys: []
  });

  // Load organization hierarchy
  useEffect(() => {
    const loadOrganizationHierarchy = async () => {
      try {
        const [orgTypesResult, orgsResult] = await Promise.all([
          organizationService.getOrganizationTypes(),
          organizationService.getOrganizations(1, 1000)
        ]);

        if (orgsResult.data.length > 0 && orgTypesResult.length > 0) {
          initializeHierarchy(orgsResult.data, orgTypesResult);
        }
      } catch (error) {
        console.error("Failed to load organization hierarchy:", error);
        toast({
          title: "Warning",
          description: "Failed to load organization data",
          variant: "default"
        });
      }
    };

    loadOrganizationHierarchy();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Load UOM options
  useEffect(() => {
    const loadUOMData = async () => {
      try {
        setLoadingUOM(true);

        // Load ALL UOM types from database - no filtering or hardcoded data
        const allTypes = await uomTypesService.getAllUOMTypes();

        // Use all types for both base and outer UOM dropdowns
        setBaseUomOptions(allTypes);
        setOuterUomOptions(allTypes);

        console.log("✅ UOM Types Data Loaded:", {
          totalTypes: allTypes.length,
          types: allTypes.map((t) => `${t.UID} - ${t.Name}`)
        });

        // Show warning if no UOM types are loaded
        if (allTypes.length === 0) {
          console.error("⚠️ No UOM Types loaded from database!");
          toast({
            title: "No UOM Types Found",
            description: "Please configure UOM types in the system.",
            variant: "destructive"
          });
        }
      } catch (error) {
        console.error("Failed to load UOM types:", error);
        setBaseUomOptions([]);
        setOuterUomOptions([]);

        toast({
          title: "Failed to Load UOM Types",
          description: "Could not load UOM types from server.",
          variant: "destructive"
        });
      } finally {
        setLoadingUOM(false);
      }
    };

    loadUOMData();
  }, []);

  // Set supplier organization
  useEffect(() => {
    if (finalSelectedOrganization) {
      setFormData((prev) => ({
        ...prev,
        SupplierOrgUID: HARDCODED_SUPPLIER_UID
      }));
    }
  }, [finalSelectedOrganization]);

  const handleInputChange = (field: keyof ISKUV1, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));

    // Auto-generate UID from Code
    if (field === "Code" && value) {
      setFormData((prev) => ({
        ...prev,
        UID: value
      }));
    }

    // Auto-fill related fields from Name
    if (field === "Name" && value) {
      setFormData((prev) => ({
        ...prev,
        LongName: value,
        AliasName: value,
        ArabicName: value
      }));
    }
  };

  const handleOrganizationSelection = (levelIndex: number, value: string) => {
    selectOrganization(levelIndex, value);

    if (value) {
      setFormData((prev) => ({
        ...prev,
        OrgUID: finalSelectedOrganization || value
      }));
    }
  };

  const addCustomField = () => {
    const newField: CustomFieldData = {
      SNo: customFields.length + 1,
      UID: `${formData.UID}_CUSTOM_${customFields.length + 1}`,
      Label: "",
      Type: "text",
      Value: ""
    };
    setCustomFields([...customFields, newField]);
  };

  const updateCustomField = (
    index: number,
    field: keyof CustomFieldData,
    value: string
  ) => {
    const updated = [...customFields];
    updated[index] = { ...updated[index], [field]: value };
    setCustomFields(updated);
  };

  const removeCustomField = (index: number) => {
    setCustomFields(customFields.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.Code || !formData.Name) {
      toast({
        title: "Validation Error",
        description: "Product code and name are required",
        variant: "destructive"
      });
      return;
    }

    if (!hasSelection) {
      toast({
        title: "Validation Error",
        description: "Please select an organization",
        variant: "destructive"
      });
      return;
    }

    const validAttributes = selectedAttributes.filter(
      (attr) => attr.code && attr.value
    );
    if (validAttributes.length === 0) {
      toast({
        title: "Validation Error",
        description: "At least one product attribute must be selected",
        variant: "destructive"
      });
      return;
    }

    setLoading(true);
    try {
      const request: EnterpriseSkuCreationRequest = {
        skuData: {
          ...formData,
          OrgUID: finalSelectedOrganization || formData.OrgUID,
          SupplierOrgUID: HARDCODED_SUPPLIER_UID
        },
        attributes: validAttributes,
        customFields: customFields.length > 0 ? customFields : undefined,
        uomDetails: uomDetails.length > 0 ? uomDetails : undefined,
        organizationUID: finalSelectedOrganization || formData.OrgUID,
        supplierOrganizationUID: HARDCODED_SUPPLIER_UID,
        distributionChannelUID: finalSelectedOrganization || formData.OrgUID,
        canBuy,
        canSell
      } as EnterpriseSkuCreationRequest;

      const result = await enterpriseSkuService.createEnterpriseSKU(request);

      if (result && result.skuResult > 0) {
        toast({
          title: "Product Created Successfully!",
          description: "Product has been created with all extended features"
        });
      } else {
        throw new Error("Failed to create product");
      }
      router.push("/productssales/product-management/products");
    } catch (error: unknown) {
      const errorMessage =
        error instanceof Error ? error.message : "Failed to create product";
      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive"
      });
      console.error("Error creating enterprise SKU:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-semibold">Create Product</h1>
            <p className="text-sm text-muted-foreground">
              Add a new product to your inventory
            </p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6">
          {/* Organization Hierarchy - Enterprise Feature */}
          <Card className="border-blue-200">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Building className="h-5 w-5 text-blue-600" />
                Organization Hierarchy
              </CardTitle>
              <CardDescription>
                Select the organization hierarchy for this product
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

          {/* Essential Product Information */}
          <Card>
            <CardHeader>
              <CardTitle>Essential Product Information</CardTitle>
              <CardDescription>
                Core product details required for all products
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-6">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="code">
                    Product Code <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="code"
                    value={formData.Code}
                    onChange={(e) => handleInputChange("Code", e.target.value)}
                    placeholder="e.g., ALM_001"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="name">
                    Product Name <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="name"
                    value={formData.Name}
                    onChange={(e) => handleInputChange("Name", e.target.value)}
                    placeholder="e.g., Premium California Almonds"
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="longName">Long Name</Label>
                  <Input
                    id="longName"
                    value={formData.LongName}
                    onChange={(e) =>
                      handleInputChange("LongName", e.target.value)
                    }
                    placeholder="Extended product description"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="aliasName">Alias Name</Label>
                  <Input
                    id="aliasName"
                    value={formData.AliasName}
                    onChange={(e) =>
                      handleInputChange("AliasName", e.target.value)
                    }
                    placeholder="Alternative name"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="arabicName">Arabic Name</Label>
                  <Input
                    id="arabicName"
                    value={formData.ArabicName}
                    onChange={(e) =>
                      handleInputChange("ArabicName", e.target.value)
                    }
                    placeholder="اسم المنتج بالعربية"
                    dir="rtl"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hsnCode">HSN/SAC Code</Label>
                  <Input
                    id="hsnCode"
                    value={formData.HSNCode}
                    onChange={(e) =>
                      handleInputChange("HSNCode", e.target.value)
                    }
                    placeholder="e.g., 08021100"
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Product Attributes Configuration - Fully Dynamic */}
          <ProductAttributes
            onAttributesChange={(attributes) => {
              setSelectedAttributes(attributes);
              // Dynamically map selections to fields (supports unlimited levels)
              const validAttributes = attributes.filter(
                (attr) => attr.code && attr.value
              );
              validAttributes.forEach((attr, index) => {
                const fieldName = `L${index + 1}` as keyof ISKUV1;
                setFormData((prev) => ({
                  ...prev,
                  [fieldName]: attr.code
                }));
              });
            }}
            disabled={loading}
            enableMultiSelect={false} // Set to true for checkbox multi-select
            fieldNamePattern="L{n}" // Pattern for field names (L1, L2, etc.)
            showLevelNumbers={true} // Show level numbers in UI
          />

          {/* Validity Period */}
          <Card>
            <CardHeader>
              <CardTitle>Validity Period</CardTitle>
              <CardDescription>
                Set the product validity dates (required)
              </CardDescription>
            </CardHeader>
            <CardContent className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="fromDate">
                  Valid From <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="fromDate"
                  type="date"
                  value={formData.FromDate}
                  onChange={(e) =>
                    handleInputChange("FromDate", e.target.value)
                  }
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="toDate">
                  Valid To <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="toDate"
                  type="date"
                  value={formData.ToDate}
                  onChange={(e) => handleInputChange("ToDate", e.target.value)}
                  required
                />
              </div>
            </CardContent>
          </Card>

          {/* Product Hierarchy with SKU Selection - Hidden for now */}
          {/* <ProductAttributesWithSKU
            onSelectionsChange={(selections) => {
              console.log("Multi-select hierarchy:", selections);
              setMultiSelectHierarchy(selections);
            }}
            onSKUSelectionsChange={(skus) => {
              console.log("Selected SKU products:", skus);
              setSelectedSKUProducts(skus);
            }}
            showLevelNumbers={true}
            enableSearch={true}
            enableSKULoading={true}
            showSKUSection={true}
            gridColumns={{ default: 1, md: 2, lg: 3 }}
          /> */}

          {/* Organization-specific Product Settings */}
          <Card>
            <CardHeader>
              <CardTitle>Product Configuration</CardTitle>
              <CardDescription>
                Configure buying and selling settings for this product
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Can Buy</Label>
                    <p className="text-sm text-muted-foreground">
                      Allow purchasing this product
                    </p>
                  </div>
                  <Switch
                    checked={canBuy}
                    onCheckedChange={(checked) => setCanBuy(checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Can Sell</Label>
                    <p className="text-sm text-muted-foreground">
                      Allow selling this product
                    </p>
                  </div>
                  <Switch
                    checked={canSell}
                    onCheckedChange={(checked) => setCanSell(checked)}
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="buyingUOM">Buying UOM</Label>
                  <Select
                    value={formData.BaseUOM}
                    onValueChange={(value) =>
                      handleInputChange("BaseUOM", value)
                    }
                    disabled={loadingUOM || !canBuy}
                  >
                    <SelectTrigger>
                      <SelectValue
                        placeholder={
                          loadingUOM ? "Loading..." : "Select buying unit"
                        }
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>
                          Loading...
                        </SelectItem>
                      ) : baseUomOptions.length > 0 ? (
                        baseUomOptions.map((uom) => (
                          <SelectItem key={uom.UID} value={uom.UID}>
                            {uom.UID} - {uom.Name}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>
                          No UOM options available
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="sellingUOM">Selling UOM</Label>
                  <Select
                    value={formData.OuterUOM}
                    onValueChange={(value) =>
                      handleInputChange("OuterUOM", value)
                    }
                    disabled={loadingUOM || !canSell}
                  >
                    <SelectTrigger>
                      <SelectValue
                        placeholder={
                          loadingUOM ? "Loading..." : "Select selling unit"
                        }
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>
                          Loading...
                        </SelectItem>
                      ) : outerUomOptions.length > 0 ? (
                        outerUomOptions.map((uom) => (
                          <SelectItem key={uom.UID} value={uom.UID}>
                            {uom.UID} - {uom.Name}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>
                          No UOM options available
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Units of Measurement */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Units of Measurement</CardTitle>
                  <CardDescription>
                    Configure base and outer units with their properties
                  </CardDescription>
                </div>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    setShowUOMConfiguration(!showUOMConfiguration);
                    // When showing configuration, auto-add Base and Outer UOM if selected
                    if (!showUOMConfiguration) {
                      const newDetails: UOMDetail[] = [];
                      if (
                        formData.BaseUOM &&
                        !uomDetails.find(
                          (u) => u.code === formData.BaseUOM && u.isBaseUOM
                        )
                      ) {
                        newDetails.push({
                          code: formData.BaseUOM,
                          name: formData.BaseUOM,
                          label: formData.BaseUOM,
                          isBaseUOM: true,
                          isOuterUOM: false
                        });
                      }
                      if (
                        formData.OuterUOM &&
                        !uomDetails.find(
                          (u) => u.code === formData.OuterUOM && u.isOuterUOM
                        )
                      ) {
                        newDetails.push({
                          code: formData.OuterUOM,
                          name: formData.OuterUOM,
                          label: formData.OuterUOM,
                          isBaseUOM: false,
                          isOuterUOM: true
                        });
                      }
                      if (newDetails.length > 0) {
                        setUomDetails((prev) => [...prev, ...newDetails]);
                      }
                    }
                  }}
                >
                  {showUOMConfiguration ? "Hide" : "Configure"} UOM Details
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="baseUOM">
                    Base UOM <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={formData.BaseUOM}
                    onValueChange={(value) => {
                      handleInputChange("BaseUOM", value);
                      // Only add to UOM details if user configures it
                      if (
                        showUOMConfiguration &&
                        !uomDetails.find((u) => u.code === value && u.isBaseUOM)
                      ) {
                        setUomDetails((prev) => [
                          ...prev.filter((u) => !u.isBaseUOM),
                          {
                            code: value,
                            name: value,
                            label: value,
                            isBaseUOM: true,
                            isOuterUOM: false
                          }
                        ]);
                      }
                    }}
                    disabled={loadingUOM}
                  >
                    <SelectTrigger>
                      <SelectValue
                        placeholder={
                          loadingUOM ? "Loading..." : "Select base unit"
                        }
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>
                          Loading...
                        </SelectItem>
                      ) : baseUomOptions.length > 0 ? (
                        baseUomOptions.map((uom) => (
                          <SelectItem key={uom.UID} value={uom.UID}>
                            {uom.UID} - {uom.Name}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>
                          No options available
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="outerUOM">
                    Outer UOM <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={formData.OuterUOM}
                    onValueChange={(value) => {
                      handleInputChange("OuterUOM", value);
                      // Only add to UOM details if user configures it
                      if (
                        showUOMConfiguration &&
                        !uomDetails.find(
                          (u) => u.code === value && u.isOuterUOM
                        )
                      ) {
                        setUomDetails((prev) => [
                          ...prev.filter((u) => !u.isOuterUOM),
                          {
                            code: value,
                            name: value,
                            label: value,
                            isBaseUOM: false,
                            isOuterUOM: true
                          }
                        ]);
                      }
                    }}
                    disabled={loadingUOM}
                  >
                    <SelectTrigger>
                      <SelectValue
                        placeholder={
                          loadingUOM ? "Loading..." : "Select outer unit"
                        }
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>
                          Loading...
                        </SelectItem>
                      ) : outerUomOptions.length > 0 ? (
                        outerUomOptions.map((uom) => (
                          <SelectItem key={uom.UID} value={uom.UID}>
                            {uom.UID} - {uom.Name}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>
                          No options available
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {/* UOM Configuration Details */}
              {showUOMConfiguration && (
                <div className="mt-6 space-y-4 border-t pt-4">
                  <h4 className="font-medium">UOM Configuration Details</h4>
                  {uomDetails.length === 0 ? (
                    <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-md">
                      <p className="text-sm text-yellow-800">
                        Please select Base UOM and/or Outer UOM above first,
                        then click "Configure UOM Details" again to configure
                        their properties.
                      </p>
                    </div>
                  ) : (
                    <>
                      {uomDetails.map((uom, index) => (
                        <div
                          key={index}
                          className="p-4 border rounded-lg space-y-4 bg-gray-50"
                        >
                          <div className="flex items-center justify-between">
                            <h5 className="font-medium">
                              {uom.isBaseUOM
                                ? "Base UOM"
                                : uom.isOuterUOM
                                ? "Outer UOM"
                                : "Additional UOM"}
                              : {uom.code}
                            </h5>
                            {!uom.isBaseUOM && !uom.isOuterUOM && (
                              <Button
                                type="button"
                                variant="destructive"
                                size="sm"
                                onClick={() =>
                                  setUomDetails((prev) =>
                                    prev.filter((_, i) => i !== index)
                                  )
                                }
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            )}
                          </div>

                          <div className="grid grid-cols-3 gap-4">
                            <div className="space-y-2">
                              <Label>Name</Label>
                              <Input
                                value={uom.name || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  updated[index] = {
                                    ...updated[index],
                                    name: e.target.value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="UOM Name"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Label</Label>
                              <Input
                                value={uom.label || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  updated[index] = {
                                    ...updated[index],
                                    label: e.target.value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="UOM Label"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Multiplier</Label>
                              <Input
                                type="number"
                                value={uom.multiplier || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  const value = e.target.value
                                    ? parseFloat(e.target.value)
                                    : undefined;
                                  updated[index] = {
                                    ...updated[index],
                                    multiplier: value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter multiplier"
                                step="any"
                              />
                            </div>
                          </div>

                          <div className="grid grid-cols-4 gap-4">
                            <div className="space-y-2">
                              <Label>Length</Label>
                              <Input
                                type="number"
                                value={uom.length || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  const value = e.target.value
                                    ? parseFloat(e.target.value)
                                    : undefined;
                                  updated[index] = {
                                    ...updated[index],
                                    length: value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter length"
                                step="any"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Width</Label>
                              <Input
                                type="number"
                                value={uom.width || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  const value = e.target.value
                                    ? parseFloat(e.target.value)
                                    : undefined;
                                  updated[index] = {
                                    ...updated[index],
                                    width: value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter width"
                                step="any"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Height</Label>
                              <Input
                                type="number"
                                value={uom.height || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  const value = e.target.value
                                    ? parseFloat(e.target.value)
                                    : undefined;
                                  updated[index] = {
                                    ...updated[index],
                                    height: value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter height"
                                step="any"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Dimension Unit</Label>
                              <Select
                                value={uom.dimensionUnit || "none"}
                                onValueChange={(value) => {
                                  const updated = [...uomDetails];
                                  updated[index] = {
                                    ...updated[index],
                                    dimensionUnit:
                                      value === "none" ? undefined : value
                                  };
                                  setUomDetails(updated);
                                }}
                              >
                                <SelectTrigger>
                                  <SelectValue placeholder="Select unit" />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="none">None</SelectItem>
                                  <SelectItem value="CM">CM</SelectItem>
                                  <SelectItem value="M">M</SelectItem>
                                  <SelectItem value="MM">MM</SelectItem>
                                  <SelectItem value="IN">IN</SelectItem>
                                  <SelectItem value="FT">FT</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                          </div>

                          <div className="grid grid-cols-4 gap-4">
                            <div className="space-y-2">
                              <Label>Weight</Label>
                              <Input
                                type="number"
                                value={uom.weight || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  const value = e.target.value
                                    ? parseFloat(e.target.value)
                                    : undefined;
                                  updated[index] = {
                                    ...updated[index],
                                    weight: value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter weight"
                                step="any"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Weight Unit</Label>
                              <Select
                                value={uom.weightUnit || "none"}
                                onValueChange={(value) => {
                                  const updated = [...uomDetails];
                                  updated[index] = {
                                    ...updated[index],
                                    weightUnit:
                                      value === "none" ? undefined : value
                                  };
                                  setUomDetails(updated);
                                }}
                              >
                                <SelectTrigger>
                                  <SelectValue placeholder="Select unit" />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="none">None</SelectItem>
                                  <SelectItem value="KG">KG</SelectItem>
                                  <SelectItem value="G">G</SelectItem>
                                  <SelectItem value="MG">MG</SelectItem>
                                  <SelectItem value="LB">LB</SelectItem>
                                  <SelectItem value="OZ">OZ</SelectItem>
                                  <SelectItem value="TON">TON</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                            <div className="space-y-2">
                              <Label>Volume</Label>
                              <Input
                                type="number"
                                value={uom.volume || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  const value = e.target.value
                                    ? parseFloat(e.target.value)
                                    : undefined;
                                  updated[index] = {
                                    ...updated[index],
                                    volume: value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter volume"
                                step="any"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Volume Unit</Label>
                              <Select
                                value={uom.volumeUnit || "none"}
                                onValueChange={(value) => {
                                  const updated = [...uomDetails];
                                  updated[index] = {
                                    ...updated[index],
                                    volumeUnit:
                                      value === "none" ? undefined : value
                                  };
                                  setUomDetails(updated);
                                }}
                              >
                                <SelectTrigger>
                                  <SelectValue placeholder="Select unit" />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="none">None</SelectItem>
                                  <SelectItem value="L">L</SelectItem>
                                  <SelectItem value="ML">ML</SelectItem>
                                  <SelectItem value="GAL">GAL</SelectItem>
                                  <SelectItem value="QT">QT</SelectItem>
                                  <SelectItem value="PT">PT</SelectItem>
                                  <SelectItem value="M3">M³</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                          </div>

                          <div className="grid grid-cols-3 gap-4">
                            <div className="space-y-2">
                              <Label>Barcode</Label>
                              <Input
                                value={uom.barcode || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  updated[index] = {
                                    ...updated[index],
                                    barcode: e.target.value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter barcode"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Gross Weight</Label>
                              <Input
                                type="number"
                                value={uom.grossWeight || ""}
                                onChange={(e) => {
                                  const updated = [...uomDetails];
                                  const value = e.target.value
                                    ? parseFloat(e.target.value)
                                    : undefined;
                                  updated[index] = {
                                    ...updated[index],
                                    grossWeight: value
                                  };
                                  setUomDetails(updated);
                                }}
                                placeholder="Enter gross weight"
                                step="any"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label>Gross Weight Unit</Label>
                              <Select
                                value={uom.grossWeightUnit || "none"}
                                onValueChange={(value) => {
                                  const updated = [...uomDetails];
                                  updated[index] = {
                                    ...updated[index],
                                    grossWeightUnit:
                                      value === "none" ? undefined : value
                                  };
                                  setUomDetails(updated);
                                }}
                              >
                                <SelectTrigger>
                                  <SelectValue placeholder="Select unit" />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="none">None</SelectItem>
                                  <SelectItem value="KG">KG</SelectItem>
                                  <SelectItem value="G">G</SelectItem>
                                  <SelectItem value="MG">MG</SelectItem>
                                  <SelectItem value="LB">LB</SelectItem>
                                  <SelectItem value="OZ">OZ</SelectItem>
                                  <SelectItem value="TON">TON</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                          </div>
                        </div>
                      ))}

                      <Button
                        type="button"
                        variant="outline"
                        onClick={() => {
                          setUomDetails((prev) => [
                            ...prev,
                            {
                              code: "",
                              isBaseUOM: false,
                              isOuterUOM: false
                            }
                          ]);
                        }}
                      >
                        <Plus className="h-4 w-4 mr-2" />
                        Add Additional UOM
                      </Button>
                    </>
                  )}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Industry-Specific Fields (Collapsible) */}
          <Card className="border-dashed">
            <CardHeader
              className="cursor-pointer select-none"
              onClick={() => setShowIndustryFields(!showIndustryFields)}
            >
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Industry-Specific Information</CardTitle>
                  <CardDescription>
                    Optional fields for specific industries
                  </CardDescription>
                </div>
                <Button type="button" variant="ghost" size="sm">
                  {showIndustryFields ? <ChevronUp /> : <ChevronDown />}
                </Button>
              </div>
            </CardHeader>
            {showIndustryFields && (
              <CardContent className="grid gap-4">
                <div className="grid grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="modelCode">Model Code</Label>
                    <Input
                      id="modelCode"
                      value={formData.ModelCode}
                      onChange={(e) =>
                        handleInputChange("ModelCode", e.target.value)
                      }
                      placeholder="e.g., MDL-2024"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="year">Year</Label>
                    <Input
                      id="year"
                      type="number"
                      value={formData.Year}
                      onChange={(e) =>
                        handleInputChange("Year", parseInt(e.target.value))
                      }
                      placeholder="2024"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="itemSeries">Item Series</Label>
                    <Input
                      id="itemSeries"
                      value={formData.ItemSeries}
                      onChange={(e) =>
                        handleInputChange("ItemSeries", e.target.value)
                      }
                      placeholder="e.g., Premium"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="type">Type</Label>
                    <Input
                      id="type"
                      value={formData.Type}
                      onChange={(e) =>
                        handleInputChange("Type", e.target.value)
                      }
                      placeholder="e.g., Food"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="productType">Product Type</Label>
                    <Input
                      id="productType"
                      value={formData.ProductType}
                      onChange={(e) =>
                        handleInputChange("ProductType", e.target.value)
                      }
                      placeholder="e.g., Dry Fruits"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="category">Category</Label>
                    <Input
                      id="category"
                      value={formData.Category}
                      onChange={(e) =>
                        handleInputChange("Category", e.target.value)
                      }
                      placeholder="e.g., Nuts"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="capacity">Capacity</Label>
                    <Input
                      id="capacity"
                      value={formData.Capacity}
                      onChange={(e) =>
                        handleInputChange("Capacity", e.target.value)
                      }
                      placeholder="e.g., 500g"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="tonnage">Tonnage</Label>
                    <Select
                      value={formData.Tonnage || "NA"}
                      onValueChange={(value) =>
                        handleInputChange(
                          "Tonnage",
                          value === "NA" ? "" : value
                        )
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select tonnage" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="NA">Not Applicable</SelectItem>
                        <SelectItem value="1">1 Ton</SelectItem>
                        <SelectItem value="1.5">1.5 Ton</SelectItem>
                        <SelectItem value="2">2 Ton</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="starRating">Star Rating</Label>
                    <Select
                      value={formData.StarRating || "NA"}
                      onValueChange={(value) =>
                        handleInputChange(
                          "StarRating",
                          value === "NA" ? "" : value
                        )
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select rating" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="NA">Not Applicable</SelectItem>
                        {[1, 2, 3, 4, 5].map((rating) => (
                          <SelectItem key={rating} value={rating.toString()}>
                            {rating} Star{rating > 1 ? "s" : ""}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </CardContent>
            )}
          </Card>

          {/* Custom Fields (Collapsible) */}
          <Card className="border-dashed">
            <CardHeader
              className="cursor-pointer select-none"
              onClick={() => setShowCustomFields(!showCustomFields)}
            >
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Custom Fields</CardTitle>
                  <CardDescription>
                    Add custom fields specific to this product
                  </CardDescription>
                </div>
                <div className="flex items-center gap-2">
                  {showCustomFields && (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={(e) => {
                        e.stopPropagation();
                        addCustomField();
                      }}
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Add Field
                    </Button>
                  )}
                  <Button type="button" variant="ghost" size="sm">
                    {showCustomFields ? <ChevronUp /> : <ChevronDown />}
                  </Button>
                </div>
              </div>
            </CardHeader>
            {showCustomFields && (
              <CardContent className="space-y-4">
                {customFields.length === 0 ? (
                  <div className="text-center py-8 text-gray-500">
                    <p>No custom fields added yet.</p>
                    <p className="text-sm">
                      Click "Add Field" to add custom fields.
                    </p>
                  </div>
                ) : (
                  customFields.map((field, index) => (
                    <div
                      key={index}
                      className="grid grid-cols-4 gap-4 p-4 border rounded-lg"
                    >
                      <div className="space-y-2">
                        <Label>Label</Label>
                        <Input
                          value={field.Label}
                          onChange={(e) =>
                            updateCustomField(index, "Label", e.target.value)
                          }
                          placeholder="Field label"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Type</Label>
                        <Select
                          value={field.Type}
                          onValueChange={(value) =>
                            updateCustomField(index, "Type", value)
                          }
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="text">Text</SelectItem>
                            <SelectItem value="number">Number</SelectItem>
                            <SelectItem value="date">Date</SelectItem>
                            <SelectItem value="boolean">Boolean</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>Value</Label>
                        <Input
                          value={field.Value}
                          onChange={(e) =>
                            updateCustomField(index, "Value", e.target.value)
                          }
                          placeholder="Field value"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Actions</Label>
                        <Button
                          type="button"
                          variant="destructive"
                          size="sm"
                          onClick={() => removeCustomField(index)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  ))
                )}
              </CardContent>
            )}
          </Card>

          {/* Product Settings */}
          <Card>
            <CardHeader>
              <CardTitle>Product Settings</CardTitle>
              <CardDescription>Configure product behavior</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Active</Label>
                    <p className="text-sm text-muted-foreground">
                      Product is available for transactions
                    </p>
                  </div>
                  <Switch
                    checked={formData.IsActive}
                    onCheckedChange={(checked) =>
                      handleInputChange("IsActive", checked)
                    }
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Stockable</Label>
                    <p className="text-sm text-muted-foreground">
                      Track inventory for this product
                    </p>
                  </div>
                  <Switch
                    checked={formData.IsStockable}
                    onCheckedChange={(checked) =>
                      handleInputChange("IsStockable", checked)
                    }
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Third Party</Label>
                    <p className="text-sm text-muted-foreground">
                      Product is from a third-party supplier
                    </p>
                  </div>
                  <Switch
                    checked={formData.IsThirdParty}
                    onCheckedChange={(checked) =>
                      handleInputChange("IsThirdParty", checked)
                    }
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Available in AP Master</Label>
                    <p className="text-sm text-muted-foreground">
                      Available in AP master system
                    </p>
                  </div>
                  <Switch
                    checked={formData.IsAvailableInApMaster}
                    onCheckedChange={(checked) =>
                      handleInputChange("IsAvailableInApMaster", checked)
                    }
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Submit Button */}
          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => router.back()}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={loading}
              className="bg-blue-600 hover:bg-blue-700"
            >
              <Save className="h-4 w-4 mr-2" />
              {loading ? "Creating Product..." : "Create Product"}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
}
