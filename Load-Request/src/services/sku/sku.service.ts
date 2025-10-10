import { FilterCriteria, SortCriteria } from "@/types/common.types";
import { getAuthHeaders } from "@/lib/auth-service";

// Define PagingRequest interface
interface PagingRequest {
  PageNumber: number;
  PageSize: number;
  IsCountRequired: boolean;
  FilterCriterias: FilterCriteria[];
  SortCriterias: SortCriteria[];
}

// SKU Types based on actual API responses
export interface SKU {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  ArabicName: string;
  AliasName: string;
  LongName: string;
  OrgUID: string;
  SupplierOrgUID: string;
  BaseUOM: string;
  OuterUOM: string;
  FromDate: string;
  ToDate: string;
  IsStockable: boolean;
  IsActive: boolean;
  IsThirdParty: boolean;
  IsFocusSKU: boolean;
  Year?: number;
  ProductCategoryId?: number;
  IsAvailableInApMaster?: boolean;
  FilterKeys?: string[];
  HSNCode?: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
}

export interface SKUListView {
  SKUUID: string;
  SKUCode: string;
  SKULongName: string;
  IsActive: boolean;
  // Additional fields that come from the actual API response
  UID?: string;
  Code?: string;
  Name?: string;
  LongName?: string;
  AliasName?: string;
  Id?: number;
  OrgUID?: string;
  SupplierOrgUID?: string;
  BaseUOM?: string;
  OuterUOM?: string;
  FromDate?: string;
  ToDate?: string;
  IsStockable?: boolean;
  IsThirdParty?: boolean;
  IsFocusSKU?: boolean;
  L1?: string;
  L2?: string;
  L3?: string;
  HSNCode?: string;
  ProductCategoryId?: number;
  Year?: number;
}

export interface SKUGroup {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  SKUGroupTypeUID: string;
  ParentUID?: string;
  ItemLevel: number;
  SupplierOrgUID: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
}

export interface SKUGroupType {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  ParentUID?: string;
  ItemLevel: number;
  AvailableForFilter: boolean;
}

export interface SKUToGroupMapping {
  Id: number;
  UID: string;
  SKUUID: string;
  SKUGroupUID: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
}

export interface SKUPriceList {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  DistributionChannelUID: string;
  Priority: number;
  IsActive: boolean;
  Status: string;
  ValidFrom: string;
  ValidUpto: string;
}

export interface SKUMasterData {
  SKU: SKU;
  SKUAttributes: Array<{
    SKUUID: string;
    Type: string;
    Code: string;
    Value: string;
    ParentType?: string;
  }>;
  SKUUOMs: Array<{
    SKUUID: string;
    Code: string;
    Name: string;
    Label: string;
    IsBaseUOM: boolean;
    IsOuterUOM: boolean;
    Multiplier: number;
  }>;
  SKUConfigs: Array<{
    OrgUID: string;
    DistributionChannelOrgUID: string;
    SKUUID: string;
    CanBuy: boolean;
    CanSell: boolean;
    BuyingUOM: string;
    SellingUOM: string;
    IsActive: boolean;
  }>;
}

export interface CreateSKURequest {
  UID: string;
  Code: string;
  Name: string;
  ArabicName?: string;
  AliasName?: string;
  LongName?: string;
  OrgUID: string;
  SupplierOrgUID: string;
  BaseUOM: string;
  OuterUOM: string;
  BuyingUOM?: string;
  SellingUOM?: string;
  CanBuy?: boolean;
  CanSell?: boolean;
  IsActive: boolean;
  IsStockable: boolean;
  IsThirdParty: boolean;
  FromDate: string;
  ToDate: string;
  HSNCode?: string;
  ProductCategoryId?: number;
  Year?: number;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface UpdateSKURequest extends Partial<CreateSKURequest> {
  UID: string;
  ModifiedBy: string;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
  CurrentPage: number;
  PageSize: number;
}

class SKUService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  // SKU CRUD Operations
  async getSKUByUID(uid: string): Promise<SKU> {
    const response = await fetch(
      `${this.baseURL}/SKU/SelectSKUByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU: ${response.statusText}`);
    }

    const result: ApiResponse<SKU> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU");
    }

    return result.Data;
  }

  async getAllSKUs(request: PagingRequest): Promise<any> {
    // FIXED: Use the exact same endpoint that promotions use successfully
    const workingEndpoint = `${this.baseURL}/SKU/SelectAllSKUDetails`;
    console.log("🌐 API Call: getAllSKUs (FIXED ENDPOINT)", {
      url: workingEndpoint,
      request,
      headers: getAuthHeaders(),
      note: "Using same endpoint as working promotion service"
    });

    try {
      const response = await fetch(workingEndpoint, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      });

      console.log("📡 Response status:", response.status, response.statusText);

      let responseData;
      try {
        responseData = await response.json();
      } catch (parseError) {
        console.error("❌ JSON parse error:", parseError);
        return {
          success: false,
          error: `Failed to parse response: ${parseError}`,
          status: response.status
        };
      }

      console.log("📋 Raw API Response:", responseData);

      if (!response.ok) {
        console.error("❌ HTTP error:", response.status, response.statusText);
        return {
          success: false,
          error: `HTTP ${response.status}: ${response.statusText}`,
          status: response.status,
          data: responseData
        };
      }

      // Handle API response format - same as promotions service
      let result;
      if (responseData && typeof responseData === "object") {
        // If response has standard API format with IsSuccess flag
        if (
          ("IsSuccess" in responseData || "isSuccess" in responseData) &&
          ("Data" in responseData || "data" in responseData)
        ) {
          // This is a standard WINIT API response
          const extractedData = responseData.Data || responseData.data;

          console.log("📊 Extracted data:", {
            dataExists: !!extractedData,
            dataType: typeof extractedData,
            pagedDataExists: !!(
              extractedData?.PagedData || extractedData?.pagedData
            ),
            pagedDataLength:
              (extractedData?.PagedData || extractedData?.pagedData)?.length ||
              0
          });

          // Check if the extracted data itself has PagedData
          if (
            extractedData &&
            typeof extractedData === "object" &&
            ("PagedData" in extractedData || "pagedData" in extractedData)
          ) {
            // Return the PagedData structure
            result = {
              success:
                responseData.IsSuccess !== false &&
                responseData.isSuccess !== false,
              data: extractedData, // Keep the structure with PagedData
              message: responseData.Message || responseData.message,
              status: response.status
            };
          } else {
            // Return the extracted data as is
            result = {
              success:
                responseData.IsSuccess !== false &&
                responseData.isSuccess !== false,
              data: extractedData,
              message: responseData.Message || responseData.message,
              status: response.status
            };
          }
        } else if ("Data" in responseData || "data" in responseData) {
          // Legacy format without IsSuccess
          result = {
            success: true,
            data: responseData.Data || responseData.data,
            message: responseData.Message || responseData.message,
            status: response.status
          };
        } else {
          // If response is direct data
          result = {
            success: true,
            data: responseData,
            status: response.status
          };
        }
      } else {
        result = {
          success: true,
          data: responseData,
          status: response.status
        };
      }

      console.log("✅ Final result:", {
        success: result.success,
        hasData: !!result.data,
        dataType: typeof result.data,
        sampleData: result.data
          ? Array.isArray(result.data)
            ? result.data.slice(0, 2)
            : result.data
          : null
      });

      return result;
    } catch (error) {
      console.error("💥 getAllSKUs error:", error);
      return {
        success: false,
        error: error instanceof Error ? error.message : "Network error",
        status: 0,
        details: error
      };
    }
  }

  async createSKU(sku: CreateSKURequest): Promise<number> {
    const response = await fetch(`${this.baseURL}/SKU/CreateSKU`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(sku)
    });

    if (!response.ok) {
      throw new Error(`Failed to create SKU: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to create SKU");
    }

    return result.Data;
  }

  async updateSKU(sku: UpdateSKURequest): Promise<number> {
    const response = await fetch(`${this.baseURL}/SKU/UpdateSKU`, {
      method: "PUT",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(sku)
    });

    if (!response.ok) {
      throw new Error(`Failed to update SKU: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to update SKU");
    }

    return result.Data;
  }

  async deleteSKU(uid: string): Promise<boolean> {
    const response = await fetch(`${this.baseURL}/SKU/DeleteSKU?UID=${uid}`, {
      method: "DELETE",
      headers: {
        ...getAuthHeaders(),
        Accept: "application/json"
      }
    });

    if (!response.ok) {
      const result: ApiResponse<any> = await response.json();
      throw new Error(
        result.ErrorMessage || `Failed to delete SKU: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    return result.IsSuccess;
  }

  async prepareSKUMasterData(skuUID: string): Promise<any> {
    try {
      const requestBody = {
        SKUUIDs: [skuUID],
        OrgUIDs: [],
        DistributionChannelUIDs: [],
        AttributeTypes: []
      };

      const response = await fetch(
        `${this.baseURL}/DataPreparation/PrepareSKUMaster`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(requestBody)
        }
      );

      if (response.ok) {
        const result = await response.json();
        return result;
      }
    } catch (error) {
      // Silent fail
    }
  }

  async getSKUAttributes(skuUID: string): Promise<any[]> {
    try {
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 100,
        IsCountRequired: false,
        FilterCriterias: [
          {
            Name: "SKUUID",
            Value: skuUID
          }
        ],
        SortCriterias: []
      };

      const response = await fetch(
        `${this.baseURL}/SKUAttributes/SelectAllSKUAttributesDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(request)
        }
      );

      if (!response.ok) {
        console.error("Failed to fetch SKU attributes:", response.statusText);
        return [];
      }

      const result: ApiResponse<PagedResponse<any>> = await response.json();
      if (result.IsSuccess && result.Data && result.Data.PagedData) {
        return result.Data.PagedData;
      }
      return [];
    } catch (error) {
      console.error("Error fetching SKU attributes:", error);
      return [];
    }
  }

  async getSKUUOMs(skuUID: string): Promise<any[]> {
    try {
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 100,
        IsCountRequired: false,
        FilterCriterias: [
          {
            Name: "SKUUID",
            Value: skuUID
          }
        ],
        SortCriterias: []
      };

      const response = await fetch(
        `${this.baseURL}/SKUUOM/SelectAllSKUUOMDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(request)
        }
      );

      if (!response.ok) {
        console.error("Failed to fetch SKU UOMs:", response.statusText);
        return [];
      }

      const result: ApiResponse<PagedResponse<any>> = await response.json();
      if (result.IsSuccess && result.Data && result.Data.PagedData) {
        return result.Data.PagedData;
      }
      return [];
    } catch (error) {
      console.error("Error fetching SKU UOMs:", error);
      return [];
    }
  }

  async getSKUConfigs(skuUID: string): Promise<any[]> {
    try {
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 100,
        IsCountRequired: false,
        FilterCriterias: [
          {
            Name: "SKUUID",
            Value: skuUID
          }
        ],
        SortCriterias: []
      };

      const response = await fetch(
        `${this.baseURL}/SKUConfig/SelectAllSKUConfigDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(request)
        }
      );

      if (!response.ok) {
        console.error("Failed to fetch SKU configs:", response.statusText);
        return [];
      }

      const result: ApiResponse<PagedResponse<any>> = await response.json();
      if (result.IsSuccess && result.Data && result.Data.PagedData) {
        return result.Data.PagedData;
      }
      return [];
    } catch (error) {
      console.error("Error fetching SKU configs:", error);
      return [];
    }
  }

  async getSKUMasterData(
    skuUIDs: string[]
  ): Promise<PagedResponse<SKUMasterData>> {
    const response = await fetch(`${this.baseURL}/SKU/GetAllSKUMasterData`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify({ SKUUIDs: skuUIDs })
    });

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU master data: ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<SKUMasterData>> =
      await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU master data");
    }

    return result.Data;
  }

  // SKU Group Management
  async getAllSKUGroups(
    request: PagingRequest
  ): Promise<PagedResponse<SKUGroup>> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectAllSKUGroupDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU groups: ${response.statusText}`);
    }

    const result: ApiResponse<PagedResponse<SKUGroup>> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU groups");
    }

    return result.Data;
  }

  async getAllSKUGroupTypes(
    request: PagingRequest
  ): Promise<PagedResponse<SKUGroupType>> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectAllSKUGroupTypeDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU group types: ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<SKUGroupType>> =
      await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU group types");
    }

    return result.Data;
  }

  // SKU to Group Mapping
  async getSKUToGroupMappings(
    request: PagingRequest
  ): Promise<PagedResponse<SKUToGroupMapping>> {
    const response = await fetch(
      `${this.baseURL}/SKUToGroupMapping/SelectAllSKUToGroupMappingDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU to group mappings: ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<SKUToGroupMapping>> =
      await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU to group mappings"
      );
    }

    return result.Data;
  }

  async createSKUToGroupMapping(mapping: {
    UID: string;
    SKUUID: string;
    SKUGroupUID: string;
    CreatedBy: string;
    ModifiedBy: string;
  }): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUToGroupMapping/CreateSKUToGroupMapping`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(mapping)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to create SKU to group mapping: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to create SKU to group mapping"
      );
    }

    return result.Data;
  }

  // Pricing Management
  async getAllPriceLists(
    request: PagingRequest
  ): Promise<PagedResponse<SKUPriceList>> {
    const response = await fetch(
      `${this.baseURL}/SKUPriceList/SelectAllSKUPriceListDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch price lists: ${response.statusText}`);
    }

    const result: ApiResponse<PagedResponse<SKUPriceList>> =
      await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch price lists");
    }

    return result.Data;
  }

  // Alias for backward compatibility
  getAllSKUToGroupMappings = this.getSKUToGroupMappings.bind(this);

  // Helper methods
  buildFilterCriteria(filters: Record<string, any>): FilterCriteria[] {
    const criteria: FilterCriteria[] = [];

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        criteria.push({
          Name: key,
          Value: Array.isArray(value) ? JSON.stringify(value) : value.toString()
        });
      }
    });

    return criteria;
  }

  buildSortCriteria(
    sortField: string,
    sortDirection: "asc" | "desc"
  ): SortCriteria[] {
    return [
      {
        SortParameter: sortField,
        Direction: sortDirection === "asc" ? "Asc" : "Desc"
      }
    ];
  }
}

export const skuService = new SKUService();
