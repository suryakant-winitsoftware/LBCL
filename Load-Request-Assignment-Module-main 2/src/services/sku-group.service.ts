import { PagingRequest } from "@/types/common.types";
import { getAuthHeaders } from "@/lib/auth-service";

// SKU Group model based on backend
export interface SKUGroup {
  Id?: number;
  UID: string;
  Code: string;
  Name: string;
  SKUGroupTypeUID: string;
  ParentUID?: string;
  ParentName?: string;
  ItemLevel: number;
  SupplierOrgUID?: string;
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

class SKUGroupService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  /**
   * Create a new SKU Group
   */
  async createSKUGroup(skuGroup: Partial<SKUGroup>): Promise<SKUGroup | null> {
    try {
      console.log("📝 [SKUGroupService] Creating SKU Group:", skuGroup);

      const response = await fetch(`${this.baseURL}/SKUGroup/CreateSKUGroup`, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(skuGroup)
      });

      if (!response.ok) {
        console.error("❌ [SKUGroupService] Failed to create SKU Group");
        return null;
      }

      const result = await response.json();
      console.log("✅ [SKUGroupService] SKU Group created successfully");
      return result.Data;
    } catch (error) {
      console.error("❌ [SKUGroupService] Error creating SKU Group:", error);
      return null;
    }
  }

  /**
   * Get all SKU Groups with paging
   */
  async getAllSKUGroups(): Promise<SKUGroup[]> {
    const request: PagingRequest = {
      PageNumber: 1,
      PageSize: 1000,
      FilterCriterias: [],
      SortCriterias: [
        {
          SortParameter: "ItemLevel",
          Direction: "Asc"
        },
        {
          SortParameter: "Name",
          Direction: "Asc"
        }
      ],
      IsCountRequired: false
    };

    console.log("🔍 [SKUGroupService] Fetching all SKU Groups...");

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

    console.log("📨 [SKUGroupService] Response status:", response.status);

    if (!response.ok) {
      console.error("❌ [SKUGroupService] Failed to fetch SKU Groups");
      return [];
    }

    const result: ApiResponse<PagedResponse<SKUGroup>> = await response.json();

    if (!result.IsSuccess || !result.Data || !result.Data.PagedData) {
      console.warn("⚠️ [SKUGroupService] No SKU Groups found");
      return [];
    }

    console.log(
      `✅ [SKUGroupService] Found ${result.Data.PagedData.length} SKU Groups`
    );
    return result.Data.PagedData;
  }

  /**
   * Get SKU Groups by Type UID
   */
  async getSKUGroupsByTypeUID(typeUID: string): Promise<SKUGroup[]> {
    try {
      console.log(
        `🔍 [SKUGroupService] Fetching SKU Groups for type: ${typeUID}`
      );

      const response = await fetch(
        `${
          this.baseURL
        }/SKUGroup/GetAllSKUGroupBySKUGroupTypeUID?skuGroupTypeUID=${encodeURIComponent(
          typeUID
        )}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders(),
            Accept: "application/json"
          }
        }
      );

      if (!response.ok) {
        console.error(
          `❌ [SKUGroupService] Failed to fetch SKU Groups for type ${typeUID}`
        );
        return [];
      }

      const result: ApiResponse<SKUGroup[]> = await response.json();

      if (!result.IsSuccess || !result.Data) {
        return [];
      }

      console.log(
        `✅ [SKUGroupService] Found ${result.Data.length} SKU Groups for type ${typeUID}`
      );
      return result.Data;
    } catch (error) {
      console.error(
        `❌ [SKUGroupService] Error fetching SKU Groups for type ${typeUID}:`,
        error
      );
      return [];
    }
  }
}

export const skuGroupService = new SKUGroupService();
