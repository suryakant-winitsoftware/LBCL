import { PagingRequest } from "@/types/common.types";
import { getAuthHeaders } from "@/lib/auth-service";

// SKU model based on backend
export interface SKU {
  Id?: number;
  UID: string;
  Code: string;
  Name: string;
  ArabicName?: string;
  AliasName?: string;
  LongName?: string;
  BaseUOM?: string;
  OuterUOM?: string;
  FromDate?: string;
  ToDate?: string;
  IsStockable?: boolean;
  ParentUID?: string;
  IsActive?: boolean;
  IsThirdParty?: boolean;
  SupplierOrgUID?: string;
  SKUImage?: string;
  CatalogueURL?: string;
  IsSelected?: boolean;
  Qty?: number;
  IsFocusSKU?: boolean;
  CompanyUID?: string;
  OrgUID?: string;
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
}

class SKUService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  /**
   * Get all SKUs with paging
   */
  async getAllSKUs(pageNumber = 1, pageSize = 1000): Promise<SKU[]> {
    const request: PagingRequest = {
      PageNumber: pageNumber,
      PageSize: pageSize,
      FilterCriterias: [],
      SortCriterias: [
        {
          SortParameter: "Name",
          Direction: "Asc",
        },
      ],
      IsCountRequired: false,
    };

    console.log("🔍 [SKUService] Fetching all SKUs...");
    console.log(
      "📡 [SKUService] API URL:",
      `${this.baseURL}/SKU/SelectAllSKUDetails`
    );
    console.log(
      "📦 [SKUService] Request payload:",
      JSON.stringify(request, null, 2)
    );

    const response = await fetch(`${this.baseURL}/SKU/SelectAllSKUDetails`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(request),
    });

    console.log(
      "📨 [SKUService] Response status:",
      response.status,
      response.statusText
    );

    if (!response.ok) {
      const errorText = await response.text();
      console.error("❌ [SKUService] HTTP Error:", response.status, errorText);
      throw new Error(
        `Failed to fetch SKUs: ${response.status} ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<SKU>> = await response.json();

    console.log(
      "📋 [SKUService] Full API Response:",
      JSON.stringify(result, null, 2).substring(0, 500)
    );

    if (!result.IsSuccess) {
      console.warn(
        "⚠️ [SKUService] API returned unsuccessful:",
        result.ErrorMessage
      );
      return [];
    }

    if (!result.Data || !result.Data.PagedData) {
      console.warn("⚠️ [SKUService] No Data or PagedData in response");
      return [];
    }

    console.log("✅ [SKUService] Found SKUs:", result.Data.PagedData.length);

    // Log first few SKUs for debugging
    if (result.Data.PagedData.length > 0) {
      console.log(
        "📝 [SKUService] Sample SKUs:",
        result.Data.PagedData.slice(0, 3).map((sku) => ({
          UID: sku.UID,
          Code: sku.Code,
          Name: sku.Name,
        }))
      );
    }

    return result.Data.PagedData;
  }

  /**
   * Search SKUs with filter
   */
  async searchSKUs(searchText: string): Promise<SKU[]> {
    const request: PagingRequest = {
      PageNumber: 1,
      PageSize: 100,
      FilterCriterias: searchText
        ? [
            {
              FieldName: "Name",
              FieldValue: searchText,
              FilterType: "Contains",
            },
          ]
        : [],
      SortCriterias: [
        {
          SortParameter: "Name",
          Direction: "Asc",
        },
      ],
      IsCountRequired: false,
    };

    console.log("🔍 [SKUService] Searching SKUs with text:", searchText);

    const response = await fetch(`${this.baseURL}/SKU/SelectAllSKUDetails`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      console.error("❌ [SKUService] Search failed:", response.status);
      return [];
    }

    const result: ApiResponse<PagedResponse<SKU>> = await response.json();

    if (!result.IsSuccess || !result.Data || !result.Data.PagedData) {
      return [];
    }

    console.log(
      "✅ [SKUService] Search found:",
      result.Data.PagedData.length,
      "SKUs"
    );
    return result.Data.PagedData;
  }

  /**
   * Get SKUs by organization
   */
  async getSKUsByOrganization(orgUID: string): Promise<SKU[]> {
    const request: PagingRequest = {
      PageNumber: 1,
      PageSize: 1000,
      FilterCriterias: orgUID
        ? [
            {
              FieldName: "OrgUID",
              FieldValue: orgUID,
              FilterType: "Equals",
            },
          ]
        : [],
      SortCriterias: [
        {
          SortParameter: "Name",
          Direction: "Asc",
        },
      ],
      IsCountRequired: false,
    };

    const response = await fetch(`${this.baseURL}/SKU/SelectAllSKUDetails`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      return [];
    }

    const result: ApiResponse<PagedResponse<SKU>> = await response.json();

    if (!result.IsSuccess || !result.Data || !result.Data.PagedData) {
      return [];
    }

    return result.Data.PagedData;
  }
}

export const skuService = new SKUService();
