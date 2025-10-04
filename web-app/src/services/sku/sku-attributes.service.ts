import { getAuthHeaders } from "@/lib/auth-service";

// SKU Attributes Types based on actual database structure
export interface SKUAttribute {
  Id?: number;
  UID: string;
  SS?: number;
  SKUUID: string; // Reference to SKU
  Type: string; // Hierarchy type like "Brand", "Category", etc.
  Code: string; // Hierarchy code like "Farmley", "Seeds", etc.
  Value: string; // Hierarchy display value
  ParentType?: string;
  CreatedBy: string;
  CreatedTime?: string;
  ModifiedBy: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

export interface CreateSKUAttributeRequest {
  UID: string;
  SKUUID: string;
  Type: string;
  Code: string;
  Value: string;
  ParentType?: string;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface SKUAttributeDropdownModel {
  UID: string;
  DropDownTitle: string;
  ParentUID: string;
}

export interface SKUAttributeResponse {
  PagedData: SKUAttribute[];
  TotalCount: number;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

export interface PagingRequest {
  PageNumber: number;
  PageSize: number;
  IsCountRequired: boolean;
  FilterCriterias: any[];
  SortCriterias: any[];
}

class SKUAttributesService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  /**
   * Get all SKU attributes with pagination, sorting, and filtering
   */
  async getAllSKUAttributes(request: PagingRequest): Promise<ApiResponse<SKUAttributeResponse>> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/SelectAllSKUAttributesDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(request),
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU attributes: ${response.statusText}`);
    }

    const result: ApiResponse<SKUAttributeResponse> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU attributes");
    }

    return result;
  }

  /**
   * Get SKU attribute by UID
   */
  async getSKUAttributeByUID(uid: string): Promise<SKUAttribute> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/SelectSKUAttributesByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU attribute: ${response.statusText}`);
    }

    const result: ApiResponse<SKUAttribute> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU attribute");
    }

    return result.Data;
  }

  /**
   * Get SKU group types for attribute dropdown
   */
  async getSKUGroupTypeForAttribute(): Promise<SKUAttributeDropdownModel[]> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/GetSKUGroupTypeForSKuAttribute`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU group types: ${response.statusText}`);
    }

    const result: ApiResponse<SKUAttributeDropdownModel[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU group types");
    }

    return result.Data;
  }

  /**
   * Create a single SKU attribute
   */
  async createSKUAttribute(
    attribute: CreateSKUAttributeRequest
  ): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/CreateSKUAttributes`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(attribute),
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to create SKU attribute: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to create SKU attribute");
    }

    return result.Data;
  }

  /**
   * Update an existing SKU attribute
   */
  async updateSKUAttribute(attribute: SKUAttribute): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/UpdateSKUAttributes`,
      {
        method: "PUT",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(attribute),
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to update SKU attribute: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to update SKU attribute");
    }

    return result.Data;
  }

  /**
   * Delete SKU attribute by UID
   */
  async deleteSKUAttribute(uid: string): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/DeleteSKUAttributesByUID?UID=${uid}`,
      {
        method: "DELETE",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to delete SKU attribute: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to delete SKU attribute");
    }

    return result.Data;
  }

  /**
   * Create multiple SKU attributes in bulk
   */
  async createBulkSKUAttributes(
    attributes: CreateSKUAttributeRequest[]
  ): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/CreateBulkSKUAttributes`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(attributes),
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to create bulk SKU attributes: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to create bulk SKU attributes"
      );
    }

    return result.Data;
  }

  /**
   * Create/Update/Delete SKU attributes in bulk
   */
  async cudBulkSKUAttributes(
    attributes: CreateSKUAttributeRequest[]
  ): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/CUDBulkSKUAttributes`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(attributes),
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to CUD bulk SKU attributes: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to CUD bulk SKU attributes"
      );
    }

    return result.Data;
  }

  /**
   * Get SKU attributes by SKU UID
   */
  async getSKUAttributesBySKUUID(skuUID: string): Promise<SKUAttribute[]> {
    const response = await fetch(
      `${this.baseURL}/SKUAttributes/SelectSKUAttributesByUID?UID=${skuUID}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU attributes: ${response.statusText}`);
    }

    const result: ApiResponse<SKUAttribute[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU attributes");
    }

    return result.Data;
  }

  /**
   * Helper to create SKU with attributes in the correct sequence
   */
  async createSKUWithAttributes(
    skuData: any,
    attributes: Array<{ type: string; code: string; value: string }>
  ): Promise<{
    skuResult: number;
    attributesResult: number;
    uomResult?: number;
    configResult?: number;
    groupMappingResults?: number[];
    hierarchyResults?: number[];
  }> {
    // Get the current timestamp for proper date handling
    const currentTime = new Date().toISOString();
    const finalCascadeSelection =
      attributes.length > 0 ? attributes[attributes.length - 1] : null;

    // Ensure all required fields are present
    const skuPayload = {
      ...skuData,
      // Fix timestamp issues
      CreatedTime: currentTime,
      ModifiedTime: currentTime,
      ServerAddTime: currentTime,
      ServerModifiedTime: currentTime,

      CompanyUID: skuData.CompanyUID || "",
      ParentUID: finalCascadeSelection?.code || "", // Use final cascade selection as parent
      SKUImage: skuData.SKUImage || "",
      CatalogueURL: skuData.CatalogueURL || "",
      IsFocusSKU: skuData.IsFocusSKU || false,

      // Additional ISKUV1 fields - populate from hierarchy selection
      L1: attributes.length > 0 ? attributes[0]?.code || "" : "",
      L2: attributes.length > 1 ? attributes[1]?.code || "" : "",
      L3: attributes.length > 2 ? attributes[2]?.code || "" : "",
      L4: attributes.length > 3 ? attributes[3]?.code || "" : "",
      L5: attributes.length > 4 ? attributes[4]?.code || "" : "",
      L6: attributes.length > 5 ? attributes[5]?.code || "" : "",
      ModelCode: skuData.ModelCode || "",
      Year: skuData.Year || new Date().getFullYear(),
      Type: skuData.Type || "",
      ProductType: skuData.ProductType || "",
      Category:
        attributes.find((attr) => attr.type === "Category")?.value || "",
      Tonnage: "",
      Capacity: "",
      StarRating: "",
      ProductCategoryId: skuData.ProductCategoryId || 0,
      ProductCategoryName: "",
      ItemSeries: "",
      HSNCode: skuData.HSNCode || "",
      IsAvailableInApMaster: false,
      FilterKeys: [],
    };

    console.log("Creating SKU with payload:", skuPayload);

    // First create the SKU
    const skuResult = await fetch(`${this.baseURL}/SKU/CreateSKU`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(skuPayload),
    });

    const responseText = await skuResult.text();
    console.log("SKU creation response:", responseText);

    if (!skuResult.ok) {
      throw new Error(
        `Failed to create SKU: ${skuResult.statusText} - ${responseText}`
      );
    }

    let skuResponse: ApiResponse<number>;
    try {
      skuResponse = JSON.parse(responseText);
    } catch (e) {
      throw new Error(`Failed to parse SKU response: ${responseText}`);
    }

    if (!skuResponse.IsSuccess) {
      throw new Error(skuResponse.ErrorMessage || "Failed to create SKU");
    }

    // Then create the SKU attributes with proper parent linking using UIDs
    const attributeRequests: CreateSKUAttributeRequest[] = attributes.map(
      (attr, index) => ({
        UID: `${skuData.UID}_${attr.type}_${attr.code}`,
        SKUUID: skuData.UID,
        Type: attr.type,
        Code: attr.code,
        Value: attr.value,
        // ParentType should reference the UID of the parent attribute, not the type name
        ParentType:
          index > 0
            ? `${skuData.UID}_${attributes[index - 1].type}_${
                attributes[index - 1].code
              }`
            : undefined,
        CreatedBy: skuData.CreatedBy || "ADMIN",
        ModifiedBy: skuData.ModifiedBy || "ADMIN",
      })
    );

    const attributesResult = await this.createBulkSKUAttributes(
      attributeRequests
    );

    // Create SKU UOM configurations if BaseUOM or OuterUOM is provided
    let uomResult: number | undefined;
    if (skuData.BaseUOM || skuData.OuterUOM) {
      const uomConfigs = [];

      if (skuData.BaseUOM) {
        uomConfigs.push({
          UID: `${skuData.UID}_BASE_UOM`,
          SKUUID: skuData.UID,
          Code: skuData.BaseUOM,
          Name: skuData.BaseUOM,
          Label: skuData.BaseUOM,
          Barcodes: "",
          IsBaseUOM: true, // Changed from IsBaseUom to IsBaseUOM
          IsOuterUOM: false, // Changed from IsOuterUom to IsOuterUOM
          Multiplier: 1,
          Length: 0,
          Depth: 0,
          Width: 0,
          Height: 0,
          Volume: 0,
          Weight: 0,
          GrossWeight: 0,
          DimensionUnit: "",
          VolumeUnit: "",
          WeightUnit: "",
          GrossWeightUnit: "",
          Liter: 0,
          KGM: 0,
          CreatedBy: skuData.CreatedBy || "ADMIN",
          ModifiedBy: skuData.ModifiedBy || "ADMIN",
          CreatedTime: currentTime,
          ModifiedTime: currentTime,
          ServerAddTime: currentTime,
          ServerModifiedTime: currentTime,
        });
      }

      if (skuData.OuterUOM) {
        uomConfigs.push({
          UID: `${skuData.UID}_OUTER_UOM`,
          SKUUID: skuData.UID,
          Code: skuData.OuterUOM,
          Name: skuData.OuterUOM,
          Label: skuData.OuterUOM,
          Barcodes: "",
          IsBaseUOM: false, // Changed from IsBaseUom to IsBaseUOM
          IsOuterUOM: true, // Changed from IsOuterUom to IsOuterUOM
          Multiplier: 12, // Default multiplier, should be configurable
          Length: 0,
          Depth: 0,
          Width: 0,
          Height: 0,
          Volume: 0,
          Weight: 0,
          GrossWeight: 0,
          DimensionUnit: "",
          VolumeUnit: "",
          WeightUnit: "",
          GrossWeightUnit: "",
          Liter: 0,
          KGM: 0,
          CreatedBy: skuData.CreatedBy || "ADMIN",
          ModifiedBy: skuData.ModifiedBy || "ADMIN",
          CreatedTime: currentTime,
          ModifiedTime: currentTime,
          ServerAddTime: currentTime,
          ServerModifiedTime: currentTime,
        });
      }

      // Create UOM configurations
      for (const uomConfig of uomConfigs) {
        try {
          console.log("Creating SKU UOM with config:", uomConfig);
          const uomResponse = await fetch(
            `${this.baseURL}/SKUUOM/CreateSKUUOM`,
            {
              method: "POST",
              headers: {
                ...getAuthHeaders(),
                "Content-Type": "application/json",
                Accept: "application/json",
              },
              body: JSON.stringify(uomConfig),
            }
          );

          const uomResponseText = await uomResponse.text();
          console.log("UOM creation response:", uomResponseText);

          if (uomResponse.ok) {
            const result: ApiResponse<number> = JSON.parse(uomResponseText);
            if (result.IsSuccess) {
              uomResult = result.Data;
              console.log("UOM created successfully:", uomResult);
            } else {
              console.error("UOM creation failed:", result.ErrorMessage);
            }
          } else {
            console.error(
              "UOM creation HTTP error:",
              uomResponse.status,
              uomResponseText
            );
          }
        } catch (error) {
          console.error("Failed to create SKU UOM configuration:", error);
        }
      }
    }

    // Create SKU Config for organization-specific settings
    let configResult: number | undefined;
    if (
      skuData.OrgUID &&
      (skuData.CanBuy !== undefined || skuData.CanSell !== undefined)
    ) {
      const skuConfig = {
        UID: skuData.UID,
        SKUUID: skuData.UID,
        OrgUID: skuData.OrgUID,
        DistributionChannelOrgUID: skuData.OrgUID,
        CanBuy: skuData.CanBuy ?? true,
        CanSell: skuData.CanSell ?? true,
        BuyingUOM: skuData.BuyingUOM || skuData.BaseUOM,
        SellingUOM: skuData.SellingUOM || skuData.OuterUOM,
        IsActive: skuData.IsActive ?? true,
        CreatedBy: skuData.CreatedBy || "ADMIN",
        ModifiedBy: skuData.ModifiedBy || "ADMIN",
      };

      try {
        console.log("Creating SKU Config:", skuConfig);
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
        console.log("Config creation response:", configResponseText);

        if (configResponse.ok) {
          const result: ApiResponse<number> = JSON.parse(configResponseText);
          if (result.IsSuccess) {
            configResult = result.Data;
            console.log("Config created successfully:", configResult);
          } else {
            console.error("Config creation failed:", result.ErrorMessage);
          }
        } else {
          console.error(
            "Config creation HTTP error:",
            configResponse.status,
            configResponseText
          );
        }
      } catch (error) {
        console.error("Failed to create SKU Config:", error);
      }
    }

    // Create SKU to Group mappings and hierarchy data
    let groupMappingResults: number[] = [];
    let hierarchyResults: number[] = [];

    if (attributes && attributes.length > 0) {
      console.log("Creating SKU hierarchy mappings...");

      for (const attribute of attributes) {
        try {
          // Step 1: Create SKU to Group Mapping
          const mappingPayload = {
            UID: `${skuData.UID}_${attribute.type}_${attribute.code}`,
            SKUUID: skuData.UID,
            SKUGroupUID: attribute.code, // The group UID is typically the code from hierarchy selection
            CreatedBy: skuData.CreatedBy || "ADMIN",
            ModifiedBy: skuData.ModifiedBy || "ADMIN",
            CreatedTime: currentTime,
            ModifiedTime: currentTime,
            ServerAddTime: currentTime,
            ServerModifiedTime: currentTime,
          };

          console.log("Creating SKU to Group mapping:", mappingPayload);
          const mappingResponse = await fetch(
            `${this.baseURL}/SKUToGroupMapping/CreateSKUToGroupMapping`,
            {
              method: "POST",
              headers: {
                ...getAuthHeaders(),
                "Content-Type": "application/json",
                Accept: "application/json",
              },
              body: JSON.stringify(mappingPayload),
            }
          );

          const mappingResponseText = await mappingResponse.text();
          console.log("SKU to Group mapping response:", mappingResponseText);

          if (mappingResponse.ok) {
            const mappingResult: ApiResponse<number> =
              JSON.parse(mappingResponseText);
            if (mappingResult.IsSuccess) {
              groupMappingResults.push(mappingResult.Data);
              console.log(
                "SKU to Group mapping created successfully:",
                mappingResult.Data
              );

              // Step 2: Create SKU Group Hierarchy Data
              try {
                console.log(
                  "Creating hierarchy data for type:",
                  attribute.type,
                  "uid:",
                  attribute.code
                );
                const hierarchyResponse = await fetch(
                  `${
                    this.baseURL
                  }/SKUGroup/InsertSKUGroupHierarchy?type=${encodeURIComponent(
                    attribute.type
                  )}&uid=${encodeURIComponent(attribute.code)}`,
                  {
                    method: "POST",
                    headers: {
                      ...getAuthHeaders(),
                      Accept: "application/json",
                    },
                  }
                );

                const hierarchyResponseText = await hierarchyResponse.text();
                console.log(
                  "Hierarchy creation response:",
                  hierarchyResponseText
                );

                if (hierarchyResponse.ok) {
                  const hierarchyResult: ApiResponse<number> = JSON.parse(
                    hierarchyResponseText
                  );
                  if (hierarchyResult.IsSuccess) {
                    hierarchyResults.push(hierarchyResult.Data);
                    console.log(
                      "Hierarchy data created successfully:",
                      hierarchyResult.Data
                    );
                  } else {
                    console.error(
                      "Hierarchy creation failed:",
                      hierarchyResult.ErrorMessage
                    );
                  }
                } else {
                  console.error(
                    "Hierarchy creation HTTP error:",
                    hierarchyResponse.status,
                    hierarchyResponseText
                  );
                }
              } catch (error) {
                console.error("Failed to create hierarchy data:", error);
              }
            } else {
              console.error(
                "SKU to Group mapping failed:",
                mappingResult.ErrorMessage
              );
            }
          } else {
            console.error(
              "SKU to Group mapping HTTP error:",
              mappingResponse.status,
              mappingResponseText
            );
          }
        } catch (error) {
          console.error("Failed to create SKU to Group mapping:", error);
        }
      }
    }

    return {
      skuResult: skuResponse.Data,
      attributesResult,
      uomResult,
      configResult,
      groupMappingResults,
      hierarchyResults,
    };
  }
}

export const skuAttributesService = new SKUAttributesService();
