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
          Accept: "application/json",
        },
        body: JSON.stringify(skuGroup),
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
          Direction: "Asc",
        },
        {
          SortParameter: "Name",
          Direction: "Asc",
        },
      ],
      IsCountRequired: false,
    };

    console.log("🔍 [SKUGroupService] Fetching all SKU Groups...");

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
   * Get a single SKU Group by UID
   */
  async getSKUGroupByUID(uid: string): Promise<SKUGroup | null> {
    try {
      console.log(`🔍 [SKUGroupService] Fetching SKU Group with UID: ${uid}`);

      const url = `${this.baseURL}/SKUGroup/GetSKUGroupByUID?uid=${encodeURIComponent(uid)}`;

      const response = await fetch(url, {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      });

      if (!response.ok) {
        console.error(`❌ [SKUGroupService] Failed to fetch SKU Group with UID ${uid}`);
        return null;
      }

      const result: ApiResponse<SKUGroup> = await response.json();

      if (!result.IsSuccess || !result.Data) {
        console.warn(`⚠️ [SKUGroupService] No SKU Group found for UID ${uid}`);
        return null;
      }

      console.log(`✅ [SKUGroupService] Found SKU Group: ${result.Data.Name}`);
      return result.Data;
    } catch (error) {
      console.error(`❌ [SKUGroupService] Error fetching SKU Group with UID ${uid}:`, error);
      return null;
    }
  }

  /**
   * Get SKU Groups by Type UID
   */
  async getSKUGroupsByTypeUID(typeUID: string): Promise<SKUGroup[]> {
    try {
      console.log(
        `🔍 [SKUGroupService] Fetching SKU Groups for type: ${typeUID}`
      );

      const url = `${
        this.baseURL
      }/SKUGroup/GetAllSKUGroupBySKUGroupTypeUID?skuGroupTypeUID=${encodeURIComponent(
        typeUID
      )}`;

      console.log(`🌐 [SKUGroupService] Request URL: ${url}`);

      const response = await fetch(url, {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      });

      console.log(`📡 [SKUGroupService] Response status: ${response.status}`);

      if (!response.ok) {
        console.error(
          `❌ [SKUGroupService] Failed to fetch SKU Groups for type ${typeUID}`
        );
        const errorText = await response.text();
        console.error(`❌ [SKUGroupService] Error response body:`, errorText);
        return [];
      }

      const result: ApiResponse<SKUGroup[]> = await response.json();
      console.log(
        `📋 [SKUGroupService] Raw API Response:`,
        JSON.stringify(result, null, 2)
      );

      if (!result.IsSuccess || !result.Data) {
        console.warn(
          `⚠️ [SKUGroupService] API returned unsuccessful or no data:`,
          {
            IsSuccess: result.IsSuccess,
            HasData: !!result.Data,
            ErrorMessage: result.ErrorMessage,
          }
        );
        return [];
      }

      console.log(
        `✅ [SKUGroupService] Found ${result.Data.length} SKU Groups for type ${typeUID}`
      );

      // Debug: Specifically check for C001 in the raw response
      const c001Group = result.Data.find((g) => g.Code === "C001");
      if (c001Group) {
        console.log(
          "🎯 [SKUGroupService] C001 FOUND in API response:",
          c001Group
        );
      } else {
        console.log("❌ [SKUGroupService] C001 NOT FOUND in API response");
        console.log(
          "Available codes (first 20):",
          result.Data.slice(0, 20).map((g) => g.Code)
        );
        console.log(
          'Codes containing "001":',
          result.Data.filter((g) => g.Code?.includes("001")).map((g) => g.Code)
        );
        console.log(
          'Codes starting with "C":',
          result.Data.filter((g) => g.Code?.startsWith("C"))
            .map((g) => g.Code)
            .slice(0, 10)
        );
      }

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
