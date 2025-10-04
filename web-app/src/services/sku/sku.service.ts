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
    Barcodes?: string;
    Length?: number;
    Width?: number;
    Height?: number;
    Weight?: number;
    Volume?: number;
  }>;
  FileSysList?: Array<{
    LinkedItemUID: string;
    FileSysType: string;
    FileType: string;
    FileName: string;
    RelativePath: string;
    IsDefault: boolean;
    FileSize: number;
    UID: string;
  }>;
  CustomSKUFields?: Array<any>;
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
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  // SKU CRUD Operations
  async getSKUByUID(uid: string): Promise<SKU> {
    const response = await fetch(
      `${this.baseURL}/SKU/SelectSKUByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
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
    // FIXED: Use the WebView endpoint that has the LEFT JOIN fix for sku_ext_data
    const workingEndpoint = `${this.baseURL}/SKU/SelectAllSKUDetailsWebView`;

    // Convert PascalCase to camelCase for backend compatibility
    const camelCaseRequest = {
      pageNumber: request.PageNumber,
      pageSize: request.PageSize,
      isCountRequired: request.IsCountRequired,
      filterCriterias:
        request.FilterCriterias?.map((f) => ({
          name: f.Name,
          value: f.Value,
        })) || [],
      sortCriterias:
        request.SortCriterias?.map((s) => ({
          sortParameter: s.SortParameter,
          direction: s.Direction,
        })) || [],
    };

    // API call to WebView endpoint with LEFT JOIN fix

    try {
      const response = await fetch(workingEndpoint, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(camelCaseRequest),
      });

      // Response received

      let responseData;
      try {
        responseData = await response.json();
      } catch (parseError) {
        console.error("❌ JSON parse error:", parseError);
        return {
          success: false,
          error: `Failed to parse response: ${parseError}`,
          status: response.status,
        };
      }

      // Process API response

      if (!response.ok) {
        console.error("❌ HTTP error:", response.status, response.statusText);
        return {
          success: false,
          error: `HTTP ${response.status}: ${response.statusText}`,
          status: response.status,
          data: responseData,
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

          // Data extracted from response

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
              status: response.status,
            };
          } else {
            // Return the extracted data as is
            result = {
              success:
                responseData.IsSuccess !== false &&
                responseData.isSuccess !== false,
              data: extractedData,
              message: responseData.Message || responseData.message,
              status: response.status,
            };
          }
        } else if ("Data" in responseData || "data" in responseData) {
          // Legacy format without IsSuccess
          result = {
            success: true,
            data: responseData.Data || responseData.data,
            message: responseData.Message || responseData.message,
            status: response.status,
          };
        } else {
          // If response is direct data
          result = {
            success: true,
            data: responseData,
            status: response.status,
          };
        }
      } else {
        result = {
          success: true,
          data: responseData,
          status: response.status,
        };
      }

      // Return processed result

      return result;
    } catch (error) {
      console.error("💥 getAllSKUs error:", error);
      return {
        success: false,
        error: error instanceof Error ? error.message : "Network error",
        status: 0,
        details: error,
      };
    }
  }

  async createSKU(sku: CreateSKURequest): Promise<number> {
    const response = await fetch(`${this.baseURL}/SKU/CreateSKU`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(sku),
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
        Accept: "application/json",
      },
      body: JSON.stringify(sku),
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
        Accept: "application/json",
      },
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
        AttributeTypes: [],
      };

      const response = await fetch(
        `${this.baseURL}/DataPreparation/PrepareSKUMaster`,
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

      if (response.ok) {
        const result = await response.json();
        return result;
      }
    } catch (error) {
      // Silent fail
    }
  }

  /**
   * Prepare SKU Master Data in chunks to avoid timeout issues
   * @param chunkSize Number of SKUs to process per chunk (default: 50)
   * @param orgUIDs Optional organization UIDs to filter
   * @param progressCallback Optional callback to track progress
   */
  async prepareSKUMasterDataInChunks(
    chunkSize: number = 50,
    orgUIDs?: string[],
    progressCallback?: (
      processed: number,
      total: number,
      currentChunk: number
    ) => void
  ): Promise<{ success: boolean; totalProcessed: number; errors: any[] }> {
    try {
      // First, get all SKU UIDs to process
      console.log("📊 Getting total SKU count for chunked processing...");

      const allSKUsRequest: PagingRequest = {
        PageNumber: 1,
        PageSize: 10000, // Get a large batch to get all UIDs
        IsCountRequired: true,
        FilterCriterias: orgUIDs?.length
          ? [{ Name: "OrgUIDs", Value: JSON.stringify(orgUIDs) }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
      };

      const allSKUsResponse = await this.getAllSKUs(allSKUsRequest);
      let allSKUUIDs: string[] = [];

      // Extract SKU UIDs from response
      if (allSKUsResponse?.success && allSKUsResponse?.data) {
        const dataSource = allSKUsResponse.data;
        let skuData: any[] = [];

        if (dataSource.Data?.PagedData) {
          skuData = dataSource.Data.PagedData;
        } else if (dataSource.PagedData) {
          skuData = dataSource.PagedData;
        } else if (Array.isArray(dataSource.Data)) {
          skuData = dataSource.Data;
        } else if (Array.isArray(dataSource)) {
          skuData = dataSource;
        }

        allSKUUIDs = skuData
          .map((sku) => sku.UID || sku.SKUUID || sku.Id)
          .filter(Boolean);
      }

      if (allSKUUIDs.length === 0) {
        return {
          success: false,
          totalProcessed: 0,
          errors: ["No SKUs found to process"],
        };
      }

      console.log(
        `🔄 Starting chunked processing: ${allSKUUIDs.length} SKUs in chunks of ${chunkSize}`
      );

      // Split into chunks
      const chunks: string[][] = [];
      for (let i = 0; i < allSKUUIDs.length; i += chunkSize) {
        chunks.push(allSKUUIDs.slice(i, i + chunkSize));
      }

      const results = {
        success: true,
        totalProcessed: 0,
        errors: [] as any[],
      };

      // Process each chunk
      for (let i = 0; i < chunks.length; i++) {
        const chunk = chunks[i];
        const chunkNumber = i + 1;

        try {
          console.log(
            `📦 Processing chunk ${chunkNumber}/${chunks.length} (${chunk.length} SKUs)...`
          );

          const requestBody = {
            SKUUIDs: chunk,
            OrgUIDs: orgUIDs || [],
            DistributionChannelUIDs: [],
            AttributeTypes: [],
          };

          const response = await fetch(
            `${this.baseURL}/DataPreparation/PrepareSKUMaster`,
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

          if (response.ok) {
            const result = await response.json();
            if (result.IsSuccess !== false) {
              results.totalProcessed += chunk.length;
              console.log(`✅ Chunk ${chunkNumber} completed successfully`);
            } else {
              results.errors.push(
                `Chunk ${chunkNumber}: ${
                  result.ErrorMessage || "Unknown error"
                }`
              );
            }
          } else {
            results.errors.push(
              `Chunk ${chunkNumber}: HTTP ${response.status} - ${response.statusText}`
            );
          }

          // Call progress callback
          if (progressCallback) {
            progressCallback(
              results.totalProcessed,
              allSKUUIDs.length,
              chunkNumber
            );
          }

          // Small delay between chunks to avoid overwhelming the server
          if (i < chunks.length - 1) {
            await new Promise((resolve) => setTimeout(resolve, 1000));
          }
        } catch (error) {
          const errorMsg =
            error instanceof Error ? error.message : "Unknown error";
          results.errors.push(`Chunk ${chunkNumber}: ${errorMsg}`);
          console.error(`❌ Chunk ${chunkNumber} failed:`, error);
        }
      }

      results.success = results.errors.length < chunks.length; // Success if at least some chunks worked

      console.log(
        `🏁 Chunked processing complete: ${results.totalProcessed}/${allSKUUIDs.length} SKUs processed`
      );
      if (results.errors.length > 0) {
        console.warn(
          `⚠️ ${results.errors.length} chunks had errors:`,
          results.errors
        );
      }

      return results;
    } catch (error) {
      console.error("💥 Chunked processing failed:", error);
      return {
        success: false,
        totalProcessed: 0,
        errors: [error instanceof Error ? error.message : "Unknown error"],
      };
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
            Value: skuUID,
          },
        ],
        SortCriterias: [],
      };

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
            Value: skuUID,
          },
        ],
        SortCriterias: [],
      };

      const response = await fetch(
        `${this.baseURL}/SKUUOM/SelectAllSKUUOMDetails`,
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
            Value: skuUID,
          },
        ],
        SortCriterias: [],
      };

      const response = await fetch(
        `${this.baseURL}/SKUConfig/SelectAllSKUConfigDetails`,
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
        Accept: "application/json",
      },
      body: JSON.stringify({ SKUUIDs: skuUIDs }),
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

  async getSKUMasterByUID(uid: string): Promise<SKUMasterData> {
    const response = await fetch(
      `${this.baseURL}/SKU/SelectSKUMasterByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU master data: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUMasterData> = await response.json();
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
          Accept: "application/json",
        },
        body: JSON.stringify(request),
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
          Accept: "application/json",
        },
        body: JSON.stringify(request),
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
          Accept: "application/json",
        },
        body: JSON.stringify(request),
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
          Accept: "application/json",
        },
        body: JSON.stringify(mapping),
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
          Accept: "application/json",
        },
        body: JSON.stringify(request),
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
          Value: Array.isArray(value)
            ? JSON.stringify(value)
            : value.toString(),
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
        Direction: sortDirection === "asc" ? "Asc" : "Desc",
      },
    ];
  }

  // Export functionality
  async exportSKUs(
    format: "csv" | "excel",
    searchTerm?: string
  ): Promise<Blob> {
    try {
      // Build request to get all SKUs for export
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 10000, // Get up to 10,000 SKUs
        FilterCriterias: searchTerm
          ? [{ Name: "skucodeandname", Value: searchTerm }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
        IsCountRequired: true,
      };

      const response = await this.getAllSKUs(request);

      let skuData: SKUListView[] = [];
      if (response && response.success !== false) {
        // Handle multiple response structures - same logic as in the page
        const dataSource = response.data || response;

        if (
          dataSource.Data?.PagedData &&
          Array.isArray(dataSource.Data.PagedData)
        ) {
          skuData = dataSource.Data.PagedData;
        } else if (
          dataSource.PagedData &&
          Array.isArray(dataSource.PagedData)
        ) {
          skuData = dataSource.PagedData;
        } else if (dataSource.Data && Array.isArray(dataSource.Data)) {
          skuData = dataSource.Data;
        } else if (dataSource.items && Array.isArray(dataSource.items)) {
          skuData = dataSource.items;
        } else if (Array.isArray(dataSource)) {
          skuData = dataSource;
        }

        // Map the actual field names to expected field names
        skuData = skuData.map((sku: any) => ({
          SKUUID: sku.UID || sku.Id || sku.Code,
          SKUCode: sku.Code || sku.UID,
          SKULongName: sku.LongName || sku.Name || sku.AliasName,
          IsActive: sku.IsActive !== false,
          ...sku,
        }));
      }

      if (format === "csv") {
        return this.exportToCSV(skuData);
      } else {
        return this.exportToExcel(skuData);
      }
    } catch (error) {
      console.error("Failed to export SKUs:", error);
      throw new Error("Failed to export SKUs");
    }
  }

  private exportToCSV(skus: any[]): Blob {
    const headers = [
      "SKU Code",
      "Product Name",
      "Alias Name",
      "Organization UID",
      "Supplier Org UID",
      "Base UOM",
      "Outer UOM",
      "HSN Code",
      "L1 Category",
      "L2 Category",
      "L3 Category",
      "Is Active",
      "Is Stockable",
      "Is Third Party",
      "Is Focus SKU",
      "From Date",
      "To Date",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date",
    ];

    const csvContent = [
      headers.join(","),
      ...skus.map((sku) =>
        [
          `"${sku.SKUCode || sku.Code || ""}"`,
          `"${sku.SKULongName || sku.LongName || sku.Name || ""}"`,
          `"${sku.AliasName || ""}"`,
          `"${sku.OrgUID || ""}"`,
          `"${sku.SupplierOrgUID || ""}"`,
          `"${sku.BaseUOM || ""}"`,
          `"${sku.OuterUOM || ""}"`,
          `"${sku.HSNCode || ""}"`,
          `"${sku.L1 || ""}"`,
          `"${sku.L2 || ""}"`,
          `"${sku.L3 || ""}"`,
          `"${sku.IsActive ? "Active" : "Inactive"}"`,
          `"${sku.IsStockable ? "Yes" : "No"}"`,
          `"${sku.IsThirdParty ? "Yes" : "No"}"`,
          `"${sku.IsFocusSKU ? "Yes" : "No"}"`,
          `"${
            sku.FromDate ? new Date(sku.FromDate).toLocaleDateString() : ""
          }"`,
          `"${sku.ToDate ? new Date(sku.ToDate).toLocaleDateString() : ""}"`,
          `"${sku.CreatedBy || ""}"`,
          `"${
            sku.CreatedTime
              ? new Date(sku.CreatedTime).toLocaleDateString()
              : ""
          }"`,
          `"${sku.ModifiedBy || ""}"`,
          `"${
            sku.ModifiedTime
              ? new Date(sku.ModifiedTime).toLocaleDateString()
              : ""
          }"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportToExcel(skus: any[]): Blob {
    // For now, return CSV format with Excel MIME type
    // In the future, could use libraries like xlsx for proper Excel export
    const csvContent = this.exportToCSV(skus);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }
}

export const skuService = new SKUService();
