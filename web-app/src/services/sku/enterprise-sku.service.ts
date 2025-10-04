import { getAuthHeaders } from "@/lib/auth-service";

// Enterprise SKU Creation Service
// Uses ISKUV1 model with all extended fields according to backend structure

/**
 * ISKUV1 Interface - Now supports dynamic hierarchy levels
 *
 * This interface maintains backward compatibility with L1-L6 fields
 * while also supporting unlimited dynamic hierarchy levels through
 * index signature [key: string]: any
 */
export interface ISKUV1 {
  // Base SKU fields (from ISKU interface)
  UID: string;
  Code: string;
  Name: string;
  ArabicName?: string;
  AliasName?: string;
  LongName?: string;
  OrgUID: string;
  SupplierOrgUID?: string;
  BaseUOM?: string;
  OuterUOM?: string;
  FromDate: string;
  ToDate: string;
  IsStockable: boolean;
  ParentUID?: string;
  IsActive: boolean;
  IsThirdParty: boolean;
  CreatedBy: string;
  ModifiedBy: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;

  // Extended ISKUV1 fields (maps to sku_ext_data table)
  // Kept for backward compatibility but now optional
  L1?: string; // Hierarchy level 1
  L2?: string; // Hierarchy level 2
  L3?: string; // Hierarchy level 3
  L4?: string; // Hierarchy level 4
  L5?: string; // Hierarchy level 5
  L6?: string; // Hierarchy level 6

  // Extended fields (optional metadata - not hierarchy related)
  ModelCode?: string; // Product model code
  Year: number; // Product year
  Type?: string; // Product type
  ProductType?: string; // Product type classification
  Tonnage?: string; // Tonnage specification
  Capacity?: string; // Capacity specification
  StarRating?: string; // Star rating
  ProductCategoryId: number; // Product category ID
  ProductCategoryName?: string; // Product category name
  ItemSeries?: string; // Item series
  HSNCode?: string; // HSN code for taxation
  IsAvailableInApMaster: boolean; // Available in AP master
  FilterKeys: string[]; // Filter keys array

  // Dynamic field support - allows unlimited hierarchy levels
  // This enables L7, L8... L999 or any custom field pattern
  [key: string]: any;
}

export interface CustomSKUField {
  UID: string;
  SKUUID: string;
  CustomField: CustomFieldData[];
}

export interface CustomFieldData {
  SNo: number;
  UID: string;
  Label: string;
  Type: string;
  Value: string;
}

export interface SKUAttributeRequest {
  UID: string;
  SKUUID: string;
  Type: string;
  Code: string;
  Value: string;
  ParentType?: string;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface UOMDetail {
  code: string;
  name?: string;
  label?: string;
  barcode?: string;
  isBaseUOM?: boolean;
  isOuterUOM?: boolean;
  multiplier?: number;
  length?: number;
  width?: number;
  height?: number;
  depth?: number;
  volume?: number;
  weight?: number;
  grossWeight?: number;
  dimensionUnit?: string;
  volumeUnit?: string;
  weightUnit?: string;
  grossWeightUnit?: string;
  liter?: number;
  kgm?: number;
}

/**
 * Enhanced Enterprise SKU Creation Request
 * Now supports dynamic hierarchy levels without limits
 */
export interface EnterpriseSkuCreationRequest {
  skuData: ISKUV1;
  attributes: Array<{
    type: string;
    code: string;
    value: string;
    level?: number; // Optional level number for dynamic mapping
    fieldName?: string; // Optional custom field name (e.g., "L7", "Category1", etc.)
  }>;
  customFields?: CustomFieldData[];
  uomDetails?: UOMDetail[];
  organizationUID: string;
  supplierOrganizationUID?: string;
  distributionChannelUID?: string;
  canBuy?: boolean;
  canSell?: boolean;

  // Dynamic configuration options
  fieldPattern?: string; // Pattern for field naming (e.g., "L{n}", "Level{n}")
  enableDynamicHierarchy?: boolean; // Enable unlimited hierarchy levels
}

export interface EnterpriseSkuCreationResult {
  skuResult: number;
  attributesResult: number;
  customFieldsResult?: number;
  dataPreparationResult?: any;
  uomResult?: number;
  configResult?: number;
  groupMappingResults?: number[];
  hierarchyResults?: number[];
}

class EnterpriseSkuService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  /**
   * Helper method to generate dynamic field names
   */
  private generateFieldName(pattern: string, level: number): string {
    return pattern.replace("{n}", level.toString());
  }

  /**
   * Create enterprise-grade SKU with all extended features
   * Now supports dynamic hierarchy levels without hardcoded limits
   * Maintains backward compatibility while enabling unlimited levels
   */
  async createEnterpriseSKU(
    request: EnterpriseSkuCreationRequest
  ): Promise<EnterpriseSkuCreationResult> {
    const currentTime = new Date().toISOString();
    const finalCascadeSelection =
      request.attributes.length > 0
        ? request.attributes[request.attributes.length - 1]
        : null;

    // Prepare ISKUV1 payload with all extended fields
    const iskuV1Payload: ISKUV1 = {
      ...request.skuData,

      // Timestamps
      CreatedTime: currentTime,
      ModifiedTime: currentTime,
      ServerAddTime: currentTime,
      ServerModifiedTime: currentTime,

      // Organization data
      OrgUID: request.organizationUID,
      SupplierOrgUID:
        request.supplierOrganizationUID || request.organizationUID,
    };

    // Dynamic hierarchy mapping - supports unlimited levels
    if (request.enableDynamicHierarchy) {
      // Use dynamic field mapping for unlimited hierarchy levels
      const fieldPattern = request.fieldPattern || "L{n}";

      request.attributes.forEach((attr, index) => {
        if (attr.code) {
          // Use custom field name if provided, otherwise generate from pattern
          const fieldName =
            attr.fieldName ||
            (attr.level
              ? this.generateFieldName(fieldPattern, attr.level)
              : this.generateFieldName(fieldPattern, index + 1));

          // Dynamically set the field value
          iskuV1Payload[fieldName] = attr.code;
        }
      });
    } else {
      // Backward compatibility: map to L1-L6 for first 6 levels
      // But also support L7+ if more than 6 attributes provided
      request.attributes.forEach((attr, index) => {
        if (attr.code) {
          const levelNum = index + 1;
          const fieldName = attr.fieldName || `L${levelNum}`;
          iskuV1Payload[fieldName] = attr.code;
        }
      });
    }

    // Add remaining fields to the payload
    iskuV1Payload.ParentUID = finalCascadeSelection?.code || "";

    // Extended data fields - these are optional and not related to hierarchy
    // The hierarchy is already mapped dynamically above (L1, L2... or custom pattern)
    // These are just additional product metadata fields
    iskuV1Payload.Year = request.skuData.Year || new Date().getFullYear();
    iskuV1Payload.ProductCategoryId = request.skuData.ProductCategoryId || 0;
    iskuV1Payload.IsAvailableInApMaster = false;
    iskuV1Payload.FilterKeys = request.skuData.FilterKeys || [];

    // DO NOT hardcode any hierarchy type names like "Category", "Brand", etc.
    // All hierarchy data is already dynamically mapped to L1, L2... or custom fields above

    // Step 1: Create SKU using ISKUV1 interface (backend handles sku_ext_data automatically)
    const skuResult = await fetch(`${this.baseURL}/SKU/CreateSKU`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(iskuV1Payload),
    });

    const skuResponseText = await skuResult.text();

    if (!skuResult.ok) {
      throw new Error(
        `Failed to create enterprise SKU: ${skuResult.statusText} - ${skuResponseText}`
      );
    }

    const skuResponse = JSON.parse(skuResponseText);
    if (!skuResponse.IsSuccess) {
      throw new Error(
        skuResponse.ErrorMessage || "Failed to create enterprise SKU"
      );
    }

    // Step 2: Create SKU Attributes (if any)
    let attributesResult = 0;
    if (request.attributes.length > 0) {
      const attributeRequests: SKUAttributeRequest[] = request.attributes.map(
        (attr, index) => ({
          UID: `${request.skuData.UID}_${attr.type}_${attr.code}`,
          SKUUID: request.skuData.UID,
          Type: attr.type,
          Code: attr.code,
          Value: attr.value,
          // Auto-calculate parent_type based on hierarchy
          ParentType:
            index === 0
              ? undefined
              : `${request.skuData.UID}_${request.attributes[index - 1].type}_${
                  request.attributes[index - 1].code
                }`,
          CreatedBy: request.skuData.CreatedBy || "ADMIN",
          ModifiedBy: request.skuData.ModifiedBy || "ADMIN",
        })
      );

      try {
        const attributesResponse = await fetch(
          `${this.baseURL}/SKUAttributes/CreateBulkSKUAttributes`,
          {
            method: "POST",
            headers: {
              ...getAuthHeaders(),
              "Content-Type": "application/json",
              Accept: "application/json",
            },
            body: JSON.stringify(attributeRequests),
          }
        );

        const attrResponseText = await attributesResponse.text();

        if (attributesResponse.ok) {
          const result = JSON.parse(attrResponseText);
          if (result.IsSuccess) {
            attributesResult = result.Data;
          }
        }
      } catch (error) {
        // Silent fail for attributes
      }
    }

    // Step 3: Create Custom SKU Fields (if any)
    let customFieldsResult: number | undefined;
    if (request.customFields && request.customFields.length > 0) {
      const customFieldPayload: CustomSKUField = {
        UID: `${request.skuData.UID}_CUSTOM`,
        SKUUID: request.skuData.UID,
        CustomField: request.customFields,
      };

      try {
        const customFieldResponse = await fetch(
          `${this.baseURL}/CustomSKUField/CreateCustomSKUField`,
          {
            method: "POST",
            headers: {
              ...getAuthHeaders(),
              "Content-Type": "application/json",
              Accept: "application/json",
            },
            body: JSON.stringify(customFieldPayload),
          }
        );

        if (customFieldResponse.ok) {
          const result = await customFieldResponse.json();
          if (result.IsSuccess) {
            customFieldsResult = result.Data;
          }
        }
      } catch (error) {
        // Silent fail for custom fields
      }
    }

    // Step 4: Create SKU UOM configurations (Base and Outer)
    let uomResult: number | undefined;
    if (
      request.skuData.BaseUOM ||
      request.skuData.OuterUOM ||
      request.uomDetails
    ) {
      const uomPayloads = [];

      // Create Base UOM
      if (request.skuData.BaseUOM) {
        const baseUomDetails = request.uomDetails?.find(
          (u) => u.code === request.skuData.BaseUOM && u.isBaseUOM
        );
        const uomPayload: any = {
          UID: `${request.skuData.UID}_UOM_BASE`,
          SKUUID: request.skuData.UID,
          Code: request.skuData.BaseUOM,
          Name: baseUomDetails?.name || request.skuData.BaseUOM,
          Label: baseUomDetails?.label || request.skuData.BaseUOM,
          IsBaseUOM: true,
          IsOuterUOM: false,
          CreatedBy: request.skuData.CreatedBy || "ADMIN",
          ModifiedBy: request.skuData.ModifiedBy || "ADMIN",
          CreatedTime: currentTime,
          ModifiedTime: currentTime,
          ServerAddTime: currentTime,
          ServerModifiedTime: currentTime,
        };

        // Only add fields if user provided them
        if (baseUomDetails) {
          if (baseUomDetails.barcode !== undefined)
            uomPayload.Barcodes = baseUomDetails.barcode;
          if (baseUomDetails.multiplier !== undefined)
            uomPayload.Multiplier = baseUomDetails.multiplier;
          if (baseUomDetails.length !== undefined)
            uomPayload.Length = baseUomDetails.length;
          if (baseUomDetails.width !== undefined)
            uomPayload.Width = baseUomDetails.width;
          if (baseUomDetails.height !== undefined)
            uomPayload.Height = baseUomDetails.height;
          if (baseUomDetails.depth !== undefined)
            uomPayload.Depth = baseUomDetails.depth;
          if (baseUomDetails.volume !== undefined)
            uomPayload.Volume = baseUomDetails.volume;
          if (baseUomDetails.weight !== undefined)
            uomPayload.Weight = baseUomDetails.weight;
          if (baseUomDetails.grossWeight !== undefined)
            uomPayload.GrossWeight = baseUomDetails.grossWeight;
          if (baseUomDetails.dimensionUnit !== undefined)
            uomPayload.DimensionUnit = baseUomDetails.dimensionUnit;
          if (baseUomDetails.volumeUnit !== undefined)
            uomPayload.VolumeUnit = baseUomDetails.volumeUnit;
          if (baseUomDetails.weightUnit !== undefined)
            uomPayload.WeightUnit = baseUomDetails.weightUnit;
          if (baseUomDetails.grossWeightUnit !== undefined)
            uomPayload.GrossWeightUnit = baseUomDetails.grossWeightUnit;
          if (baseUomDetails.liter !== undefined)
            uomPayload.Liter = baseUomDetails.liter;
          if (baseUomDetails.kgm !== undefined)
            uomPayload.KGM = baseUomDetails.kgm;
        }

        uomPayloads.push(uomPayload);
      }

      // Create Outer UOM if different from Base
      if (
        request.skuData.OuterUOM &&
        request.skuData.OuterUOM !== request.skuData.BaseUOM
      ) {
        const outerUomDetails = request.uomDetails?.find(
          (u) => u.code === request.skuData.OuterUOM && u.isOuterUOM
        );
        const uomPayload: any = {
          UID: `${request.skuData.UID}_UOM_OUTER`,
          SKUUID: request.skuData.UID,
          Code: request.skuData.OuterUOM,
          Name: outerUomDetails?.name || request.skuData.OuterUOM,
          Label: outerUomDetails?.label || request.skuData.OuterUOM,
          IsBaseUOM: false,
          IsOuterUOM: true,
          CreatedBy: request.skuData.CreatedBy || "ADMIN",
          ModifiedBy: request.skuData.ModifiedBy || "ADMIN",
          CreatedTime: currentTime,
          ModifiedTime: currentTime,
          ServerAddTime: currentTime,
          ServerModifiedTime: currentTime,
        };

        // Only add fields if user provided them
        if (outerUomDetails) {
          if (outerUomDetails.barcode !== undefined)
            uomPayload.Barcodes = outerUomDetails.barcode;
          if (outerUomDetails.multiplier !== undefined)
            uomPayload.Multiplier = outerUomDetails.multiplier;
          if (outerUomDetails.length !== undefined)
            uomPayload.Length = outerUomDetails.length;
          if (outerUomDetails.width !== undefined)
            uomPayload.Width = outerUomDetails.width;
          if (outerUomDetails.height !== undefined)
            uomPayload.Height = outerUomDetails.height;
          if (outerUomDetails.depth !== undefined)
            uomPayload.Depth = outerUomDetails.depth;
          if (outerUomDetails.volume !== undefined)
            uomPayload.Volume = outerUomDetails.volume;
          if (outerUomDetails.weight !== undefined)
            uomPayload.Weight = outerUomDetails.weight;
          if (outerUomDetails.grossWeight !== undefined)
            uomPayload.GrossWeight = outerUomDetails.grossWeight;
          if (outerUomDetails.dimensionUnit !== undefined)
            uomPayload.DimensionUnit = outerUomDetails.dimensionUnit;
          if (outerUomDetails.volumeUnit !== undefined)
            uomPayload.VolumeUnit = outerUomDetails.volumeUnit;
          if (outerUomDetails.weightUnit !== undefined)
            uomPayload.WeightUnit = outerUomDetails.weightUnit;
          if (outerUomDetails.grossWeightUnit !== undefined)
            uomPayload.GrossWeightUnit = outerUomDetails.grossWeightUnit;
          if (outerUomDetails.liter !== undefined)
            uomPayload.Liter = outerUomDetails.liter;
          if (outerUomDetails.kgm !== undefined)
            uomPayload.KGM = outerUomDetails.kgm;
        }

        uomPayloads.push(uomPayload);
      }

      // Add any additional UOM configurations from uomDetails
      if (request.uomDetails) {
        request.uomDetails.forEach((uom) => {
          // Skip if already added as base or outer
          if (
            (uom.code === request.skuData.BaseUOM && uom.isBaseUOM) ||
            (uom.code === request.skuData.OuterUOM && uom.isOuterUOM)
          ) {
            return;
          }

          const uomPayload: any = {
            UID: `${request.skuData.UID}_UOM_${uom.code}`,
            SKUUID: request.skuData.UID,
            Code: uom.code,
            Name: uom.name || uom.code,
            Label: uom.label || uom.code,
            IsBaseUOM: uom.isBaseUOM || false,
            IsOuterUOM: uom.isOuterUOM || false,
            CreatedBy: request.skuData.CreatedBy || "ADMIN",
            ModifiedBy: request.skuData.ModifiedBy || "ADMIN",
            CreatedTime: currentTime,
            ModifiedTime: currentTime,
            ServerAddTime: currentTime,
            ServerModifiedTime: currentTime,
          };

          // Only add optional fields if user provided them
          if (uom.barcode !== undefined) uomPayload.Barcodes = uom.barcode;
          if (uom.multiplier !== undefined)
            uomPayload.Multiplier = uom.multiplier;
          if (uom.length !== undefined) uomPayload.Length = uom.length;
          if (uom.width !== undefined) uomPayload.Width = uom.width;
          if (uom.height !== undefined) uomPayload.Height = uom.height;
          if (uom.depth !== undefined) uomPayload.Depth = uom.depth;
          if (uom.volume !== undefined) uomPayload.Volume = uom.volume;
          if (uom.weight !== undefined) uomPayload.Weight = uom.weight;
          if (uom.grossWeight !== undefined)
            uomPayload.GrossWeight = uom.grossWeight;
          if (uom.dimensionUnit !== undefined)
            uomPayload.DimensionUnit = uom.dimensionUnit;
          if (uom.volumeUnit !== undefined)
            uomPayload.VolumeUnit = uom.volumeUnit;
          if (uom.weightUnit !== undefined)
            uomPayload.WeightUnit = uom.weightUnit;
          if (uom.grossWeightUnit !== undefined)
            uomPayload.GrossWeightUnit = uom.grossWeightUnit;
          if (uom.liter !== undefined) uomPayload.Liter = uom.liter;
          if (uom.kgm !== undefined) uomPayload.KGM = uom.kgm;

          uomPayloads.push(uomPayload);
        });
      }

      try {
        for (const uomPayload of uomPayloads) {
          console.log("🏷️ Creating UOM with barcode:", {
            Code: uomPayload.Code,
            Barcodes: uomPayload.Barcodes,
            payload: uomPayload,
          });

          const uomResponse = await fetch(
            `${this.baseURL}/SKUUOM/CreateSKUUOM`,
            {
              method: "POST",
              headers: {
                ...getAuthHeaders(),
                "Content-Type": "application/json",
                Accept: "application/json",
              },
              body: JSON.stringify(uomPayload),
            }
          );

          const uomResponseText = await uomResponse.text();
          console.log("📦 UOM API Response:", {
            status: uomResponse.status,
            ok: uomResponse.ok,
            response: uomResponseText,
          });

          if (uomResponse.ok) {
            const result = JSON.parse(uomResponseText);
            console.log("✅ UOM Creation Result:", result);
            if (result.IsSuccess) {
              uomResult = (uomResult || 0) + 1;
            } else {
              console.error("❌ UOM Creation Failed:", result.ErrorMessage);
            }
          } else {
            console.error(
              "❌ UOM API Call Failed:",
              uomResponse.status,
              uomResponseText
            );
          }
        }
      } catch (error) {
        // Silent fail for UOM
      }
    }

    // Step 5: Create SKU Config for organization settings
    let configResult: number | undefined;
    if (request.organizationUID) {
      const skuConfig = {
        UID: request.skuData.UID,
        SKUUID: request.skuData.UID,
        OrgUID: request.organizationUID,
        DistributionChannelOrgUID:
          request.distributionChannelUID || request.organizationUID,
        CanBuy: request.canBuy !== undefined ? request.canBuy : true,
        CanSell: request.canSell !== undefined ? request.canSell : true,
        BuyingUOM: request.skuData.BaseUOM,
        SellingUOM: request.skuData.OuterUOM,
        IsActive: request.skuData.IsActive,
        CreatedBy: request.skuData.CreatedBy || "ADMIN",
        ModifiedBy: request.skuData.ModifiedBy || "ADMIN",
      };

      try {
        const configResponse = await fetch(
          `${this.baseURL}/SKUConfig/CreateSKUConfig`,
          {
            method: "POST",
            headers: {
              ...getAuthHeaders(),
              "Content-Type": "application/json",
              Accept: "application/json",
            },
            body: JSON.stringify(skuConfig),
          }
        );

        const configResponseText = await configResponse.text();

        if (configResponse.ok) {
          const result = JSON.parse(configResponseText);
          if (result.IsSuccess) {
            configResult = result.Data;
          }
        }
      } catch (error) {
        // Silent fail for config
      }
    }

    // Trigger data preparation to ensure master data is available
    try {
      const prepareResponse = await fetch(
        `${this.baseURL}/DataPreparation/PrepareSKUMaster?UID=${iskuV1Payload.UID}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders(),
            Accept: "application/json",
          },
        }
      );

      if (prepareResponse.ok) {
        await prepareResponse.json();
      }
    } catch (error) {
      // Silent fail for preparation
    }

    return {
      skuResult: skuResponse.Data,
      attributesResult,
      customFieldsResult,
      uomResult,
      configResult,
      groupMappingResults: [], // Would implement group mappings if needed
      hierarchyResults: [], // Would implement hierarchy data if needed
    };
  }

  // Organization methods removed - use organizationService instead
}

export const enterpriseSkuService = new EnterpriseSkuService();
