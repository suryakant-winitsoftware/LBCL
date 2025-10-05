import { getAuthHeaders } from "@/lib/auth-service";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

export interface SKUClassGroupItem {
  Id?: number;
  UID: string;
  SKUClassGroupUID: string;
  SKUCode: string;
  SKUUID?: string;
  SerialNumber: number;
  ModelQty: number;
  ModelUoM: string;
  SupplierOrgUID?: string;
  LeadTimeInDays: number;
  DailyCutOffTime?: string;
  IsExclusive: boolean;
  MaxQTY: number;
  MinQTY: number;
  IsExcluded?: boolean;
  ActionType?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

export interface SKUClassGroupItemView {
  Id: number;
  UID: string;
  SKUClassGroupUID: string;
  SKUClassGroupName?: string;
  SKUCode: string;
  SKUUID: string;
  SKUName?: string;
  SerialNumber: number;
  ModelQty: number;
  ModelUoM: string;
  SupplierOrgUID: string;
  SupplierOrgName?: string;
  LeadTimeInDays: number;
  DailyCutOffTime: string;
  IsExclusive: boolean;
  MaxQTY: number;
  MinQTY: number;
  IsExcluded: boolean;
  ActionType: number;
}

export interface SKUClassGroupItemCreateData {
  UID: string;
  SKUClassGroupUID: string;
  SKUCode: string;
  SKUUID?: string;
  SerialNumber: number;
  ModelQty: number;
  ModelUoM: string;
  SupplierOrgUID?: string;
  LeadTimeInDays: number;
  DailyCutOffTime?: string;
  IsExclusive: boolean;
  MaxQTY: number;
  MinQTY: number;
  IsExcluded?: boolean;
  ActionType?: number;
  CreatedBy?: string;
  CreatedTime?: Date;
  ModifiedBy?: string;
  ModifiedTime?: Date;
  ServerAddTime?: Date;
  ServerModifiedTime?: Date;
}

export interface SKUClassGroupItemUpdateData
  extends SKUClassGroupItemCreateData {
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

class SKUClassGroupItemsService {
  /**
   * Get all SKU Class Group Items with pagination
   */
  async getAllSKUClassGroupItems(
    pageNumber: number = 1,
    pageSize: number = 50,
    searchTerm: string = "",
    skuClassGroupUID?: string
  ): Promise<{ data: SKUClassGroupItem[]; totalCount: number }> {
    try {
      const FilterCriterias = [];

      if (searchTerm) {
        FilterCriterias.push({ Name: "sku_code", Value: searchTerm, Type: 1 });
      }

      if (skuClassGroupUID) {
        FilterCriterias.push({
          Name: "sku_class_group_uid",
          Value: skuClassGroupUID,
          Type: 0
        });
      }

      const response = await fetch(
        `${BASE_URL}/SKUClassGroupItems/SelectAllSKUClassGroupItemsDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: pageNumber - 1, // Convert to 0-based for backend
            PageSize: pageSize,
            IsCountRequired: true,
            FilterCriterias: FilterCriterias,
            SortCriterias: [{ SortParameter: "SerialNumber", Direction: "Asc" }]
          })
        }
      );

      const result: ApiResponse<PaginatedResponse<SKUClassGroupItem>> =
        await response.json();

      if (result.IsSuccess && result.Data?.PagedData) {
        return {
          data: result.Data.PagedData,
          totalCount: result.Data.TotalCount || 0
        };
      }

      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU Class Group Items"
      );
    } catch (error) {
      console.error("Error fetching SKU Class Group Items:", error);
      throw error;
    }
  }

  /**
   * Get all SKU Class Group Items View with pagination
   */
  async getAllSKUClassGroupItemsView(
    pageNumber: number = 1,
    pageSize: number = 50,
    searchTerm: string = "",
    skuClassGroupUID?: string
  ): Promise<{ data: SKUClassGroupItemView[]; totalCount: number }> {
    try {
      const filterCriterias = [];

      if (searchTerm) {
        filterCriterias.push({ Name: "sku_code", Value: searchTerm, Type: 1 });
      }

      if (skuClassGroupUID) {
        filterCriterias.push({
          Name: "sku_class_group_uid",
          Value: skuClassGroupUID,
          Type: 0
        });
      }

      const response = await fetch(
        `${BASE_URL}/SKUClassGroupItems/SelectAllSKUClassGroupItemView`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: pageNumber - 1, // Convert to 0-based for backend
            PageSize: pageSize,
            IsCountRequired: true,
            FilterCriterias: filterCriterias,
            SortCriterias: [{ SortParameter: "SerialNumber", Direction: "Asc" }]
          })
        }
      );

      const result: ApiResponse<PaginatedResponse<SKUClassGroupItemView>> =
        await response.json();

      if (result.IsSuccess && result.Data?.PagedData) {
        return {
          data: result.Data.PagedData,
          totalCount: result.Data.TotalCount || 0
        };
      }

      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU Class Group Items View"
      );
    } catch (error) {
      console.error("Error fetching SKU Class Group Items View:", error);
      throw error;
    }
  }

  /**
   * Get single SKU Class Group Item by UID
   */
  async getSKUClassGroupItemByUID(uid: string): Promise<SKUClassGroupItem> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroupItems/GetSKUClassGroupItemsByUID?UID=${uid}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      const result: ApiResponse<SKUClassGroupItem> = await response.json();

      if (result.IsSuccess && result.Data) {
        return result.Data;
      }

      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU Class Group Item"
      );
    } catch (error) {
      console.error("Error fetching SKU Class Group Item:", error);
      throw error;
    }
  }

  /**
   * Create new SKU Class Group Item
   */
  async createSKUClassGroupItem(
    data: SKUClassGroupItemCreateData
  ): Promise<number> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroupItems/CreateSKUClassGroupItems`,
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
        result.ErrorMessage || "Failed to create SKU Class Group Item"
      );
    } catch (error) {
      console.error("Error creating SKU Class Group Item:", error);
      throw error;
    }
  }

  /**
   * Update existing SKU Class Group Item
   */
  async updateSKUClassGroupItem(
    data: SKUClassGroupItemUpdateData
  ): Promise<number> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroupItems/UpdateSKUClassGroupItems`,
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
        result.ErrorMessage || "Failed to update SKU Class Group Item"
      );
    } catch (error) {
      console.error("Error updating SKU Class Group Item:", error);
      throw error;
    }
  }

  /**
   * Delete SKU Class Group Item
   */
  async deleteSKUClassGroupItem(uid: string): Promise<number> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUClassGroupItems/DeleteSKUClassGroupItems?UID=${uid}`,
        {
          method: "DELETE",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      const result: ApiResponse<number> = await response.json();

      if (result.IsSuccess && result.Data) {
        return result.Data;
      }

      throw new Error(
        result.ErrorMessage || "Failed to delete SKU Class Group Item"
      );
    } catch (error) {
      console.error("Error deleting SKU Class Group Item:", error);
      throw error;
    }
  }

  /**
   * Get available SKUs for dropdown
   */
  async getAvailableSKUs(): Promise<
    { uid: string; code: string; name: string }[]
  > {
    try {
      // This would need to be implemented based on your SKU API
      // For now, returning mock data
      return [
        { uid: "sku1", code: "SKU001", name: "Product 1" },
        { uid: "sku2", code: "SKU002", name: "Product 2" },
        { uid: "sku3", code: "SKU003", name: "Product 3" }
      ];
    } catch (error) {
      console.error("Error fetching available SKUs:", error);
      throw error;
    }
  }

  /**
   * Get available supplier organizations for dropdown
   */
  async getAvailableSupplierOrgs(): Promise<{ uid: string; name: string }[]> {
    try {
      // This would need to be implemented based on your organization API
      // For now, returning mock data
      return [
        { uid: "org1", name: "Supplier Org 1" },
        { uid: "org2", name: "Supplier Org 2" },
        { uid: "org3", name: "Supplier Org 3" }
      ];
    } catch (error) {
      console.error("Error fetching supplier organizations:", error);
      throw error;
    }
  }

  /**
   * Get available UoMs for dropdown
   */
  async getAvailableUoMs(): Promise<string[]> {
    try {
      // This would need to be implemented based on your UoM API
      // For now, returning mock data
      return ["PCS", "KG", "LTR", "BOX", "CASE"];
    } catch (error) {
      console.error("Error fetching UoMs:", error);
      throw error;
    }
  }

  /**
   * Check if an item with the given SKU and Group already exists
   */
  async checkIfItemExists(
    skuClassGroupUID: string,
    skuCode: string
  ): Promise<boolean> {
    try {
      const filterCriterias = [
        { Name: "sku_class_group_uid", Value: skuClassGroupUID, Type: 0 },
        { Name: "sku_code", Value: skuCode, Type: 0 }
      ];

      const response = await fetch(
        `${BASE_URL}/SKUClassGroupItems/SelectAllSKUClassGroupItemsDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 1,
            IsCountRequired: true,
            FilterCriterias: filterCriterias,
            SortCriterias: []
          })
        }
      );

      const result: ApiResponse<PaginatedResponse<SKUClassGroupItem>> =
        await response.json();

      if (result.IsSuccess && result.Data) {
        return result.Data.TotalCount > 0;
      }

      return false;
    } catch (error) {
      console.error("Error checking if item exists:", error);
      return false;
    }
  }
}

export const skuClassGroupItemsService = new SKUClassGroupItemsService();
