import { getAuthHeaders } from "@/lib/auth-service";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

export interface SKUClassGroup {
  Id: number;
  UID: string;
  Name: string;
  Description: string;
  OrgUID: string;
  DistributionChannelUID: string;
  SKUClassUID?: string;
  IsActive: boolean;
  FromDate: string;
  ToDate: string;
  Priority: number;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
}

export interface SKUClassGroupCreateData {
  UID: string;
  Name: string;
  Description: string;
  OrgUID: string;
  DistributionChannelUID: string;
  SKUClassUID?: string;
  IsActive: boolean;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface SKUClassGroupUpdateData extends SKUClassGroupCreateData {
  // Same as create for now
}

export interface PaginatedResponse<T> {
  PagedData: T[];
  TotalCount: number;
  PageNumber: number;
  PageSize: number;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

class SKUClassGroupsService {
  /**
   * Get all SKU Class Groups with pagination
   */
  async getAllSKUClassGroups(
    pageNumber: number = 1,
    pageSize: number = 50,
    searchTerm: string = ""
  ): Promise<{ data: SKUClassGroup[]; totalCount: number }> {
    try {
      const filterCriterias = searchTerm
        ? [{ Name: "Name", Value: searchTerm, Type: 1 }]
        : [];

      const response = await fetch(
        `${BASE_URL}/SKUClassGroup/SelectAllSKUClassGroupDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: pageNumber,
            PageSize: pageSize,
            IsCountRequired: true,
            FilterCriterias: filterCriterias,
            SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }]
          })
        }
      );

      // Check if response is OK and is JSON
      if (!response.ok) {
        console.error(
          `SKU Class Groups API returned status: ${response.status}`
        );
        const text = await response.text();
        console.error("Response body:", text);
        throw new Error(`API returned status ${response.status}`);
      }

      const contentType = response.headers.get("content-type");
      if (!contentType || !contentType.includes("application/json")) {
        const text = await response.text();
        console.error("Non-JSON response received:", text);
        throw new Error("API returned non-JSON response");
      }

      const result: ApiResponse<PaginatedResponse<SKUClassGroup>> =
        await response.json();

      if (result.IsSuccess && result.Data?.PagedData) {
        return {
          data: result.Data.PagedData,
          totalCount: result.Data.TotalCount || 0
        };
      }

      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU Class Groups"
      );
    } catch (error) {
      console.error("Error fetching SKU Class Groups:", error);
      throw error;
    }
  }

  /**
   * Get single SKU Class Group by UID
   */
  async getSKUClassGroupByUID(uid: string): Promise<SKUClassGroup | null> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroup/SelectSKUClassGroupByUID?UID=${uid}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      if (!response.ok) {
        console.error(
          `Failed to fetch SKU Class Group: ${response.status} ${response.statusText}`
        );
        return null;
      }

      const text = await response.text();
      if (!text || text.trim() === "") {
        console.error("Empty response from API");
        return null;
      }

      const result: ApiResponse<SKUClassGroup> = JSON.parse(text);

      if (result.IsSuccess && result.Data) {
        return result.Data;
      }

      console.error("API returned unsuccessful response:", result.ErrorMessage);
      return null;
    } catch (error) {
      console.error("Error fetching SKU Class Group:", error);
      return null;
    }
  }

  /**
   * Create new SKU Class Group
   */
  async createSKUClassGroup(data: SKUClassGroupCreateData): Promise<number> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroup/CreateSKUClassGroup`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify(data)
        }
      );

      const result: ApiResponse<number> = await response.json();

      if (result.IsSuccess && result.Data) {
        return result.Data;
      }

      throw new Error(
        result.ErrorMessage || "Failed to create SKU Class Group"
      );
    } catch (error) {
      console.error("Error creating SKU Class Group:", error);
      throw error;
    }
  }

  /**
   * Update existing SKU Class Group
   */
  async updateSKUClassGroup(data: SKUClassGroupUpdateData): Promise<number> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroup/UpdateSKUClassGroup`,
        {
          method: "PUT",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify(data)
        }
      );

      const result: ApiResponse<number> = await response.json();

      if (result.IsSuccess && result.Data) {
        return result.Data;
      }

      throw new Error(
        result.ErrorMessage || "Failed to update SKU Class Group"
      );
    } catch (error) {
      console.error("Error updating SKU Class Group:", error);
      throw error;
    }
  }

  /**
   * Delete SKU Class Group
   */
  async deleteSKUClassGroup(uid: string): Promise<number> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroup/DeleteSKUClassGroup?UID=${uid}`,
        {
          method: "DELETE",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      const result: ApiResponse<number> = await response.json();

      if (result.IsSuccess) {
        return result.Data || 1;
      }

      throw new Error(
        result.ErrorMessage || "Failed to delete SKU Class Group"
      );
    } catch (error) {
      console.error("Error deleting SKU Class Group:", error);
      throw error;
    }
  }

  /**
   * Get available organizations for dropdown
   */
  async getAvailableOrganizations(): Promise<{ uid: string; name: string }[]> {
    // For now, return commonly used organizations
    // In future, this could be fetched from an organizations API
    return [
      { uid: "Farmley", name: "Farmley" },
      { uid: "EPIC01", name: "EPIC01" },
      { uid: "Supplier", name: "Supplier" }
    ];
  }

  /**
   * Get available distribution channels for dropdown
   */
  async getAvailableDistributionChannels(): Promise<
    { uid: string; name: string }[]
  > {
    // For now, return commonly used channels
    // In future, this could be fetched from a distribution channels API
    return [
      { uid: "RO", name: "RO" },
      { uid: "DISTRIBUTOR_AAAAAAA", name: "Distributor" },
      { uid: "FRANCHISEE", name: "Franchisee" }
    ];
  }
}

export const skuClassGroupsService = new SKUClassGroupsService();
