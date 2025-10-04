/**
 * Store to Group Mapping Service
 * Handles API calls for store-to-group relationships
 */

import { apiService } from "./api";
import { storeService } from "./storeService";
import { StoreListRequest, IStore } from "@/types/store.types";

interface IStoreToGroupMapping {
  UID?: string;
  StoreUID?: string;
  StoreGroupUID?: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

interface MappingListRequest {
  PageNumber: number;
  PageSize: number;
  FilterCriterias?: Array<{
    PropertyName: string;
    Name?: string;
    Value: any;
    Operator?: string;
  }>;
  SortCriterias?: Array<{
    SortParameter: string;
    Direction: number;
  }>;
  IsCountRequired?: boolean;
}

interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
}

class StoreToGroupMappingService {
  private getBaseUrl(): string {
    return process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";
  }

  /**
   * Get all store-to-group mappings with filtering
   * Backend requires GET with JSON body (non-standard but that's how it works)
   */
  async getAllMappings(
    request: MappingListRequest
  ): Promise<PagedResponse<IStoreToGroupMapping>> {
    try {
      // Build request body with EXACT format that works (camelCase)
      const requestBody = {
        pageNumber: request.PageNumber,
        pageSize: request.PageSize,
        filterCriterias:
          request.FilterCriterias?.map((filter) => ({
            PropertyName: filter.PropertyName, // Keep PropertyName as is
            Name: filter.Name || filter.PropertyName,
            Value: filter.Value,
            Operator: filter.Operator || "equals",
          })) || [],
        isCountRequired: request.IsCountRequired || false,
      };

      // Use POST method as the backend now properly accepts it
      const response = await apiService.post(
        "/StoreToGroupMapping/SelectAllStoreToGroupMapping",
        requestBody
      );

      console.log("📊 Raw mapping API response:", response);

      // Handle the response
      const data = response;

      // Handle different response structures from .NET API
      if (data && typeof data === "object") {
        // Check for wrapped response with Data property
        if (data.Data && data.Data.PagedData !== undefined) {
          return {
            PagedData: data.Data.PagedData,
            TotalCount: data.Data.TotalCount || 0,
          };
        }
        // Check for direct PagedData
        if (data.PagedData !== undefined) {
          return {
            PagedData: data.PagedData,
            TotalCount: data.TotalCount || 0,
          };
        }
      }

      return {
        PagedData: [],
        TotalCount: 0,
      };
    } catch (error) {
      console.error("Error fetching store-to-group mappings:", error);
      throw error;
    }
  }

  /**
   * Get stores by store group UID
   * This combines the mapping API with the store API to get full store details
   */
  async getStoresByGroupUID(storeGroupUID: string): Promise<IStore[]> {
    try {
      console.log(`🏪 Getting stores for group UID: ${storeGroupUID}`);

      // Step 1: Get mappings for this specific store group with EXACT format
      const mappingRequest: MappingListRequest = {
        PageNumber: 1,
        PageSize: 1000,
        FilterCriterias: [
          {
            PropertyName: "store_group_uid", // snake_case for PostgreSQL column
            Name: "store_group_uid",
            Value: storeGroupUID,
            Operator: "equals",
          },
        ],
        SortCriterias: [],
        IsCountRequired: true,
      };

      console.log("📋 Fetching mappings for store group...");
      console.log("🔍 Mapping request being sent:", mappingRequest);
      const mappingResponse = await this.getAllMappings(mappingRequest);

      console.log("📊 Mapping response structure:", {
        hasPagedData: !!mappingResponse.PagedData,
        pagedDataLength: mappingResponse.PagedData?.length,
        totalCount: mappingResponse.TotalCount,
        firstItem: mappingResponse.PagedData?.[0],
      });

      if (
        !mappingResponse.PagedData ||
        mappingResponse.PagedData.length === 0
      ) {
        console.log("❌ No store mappings found for this group");
        return [];
      }

      console.log(
        `✅ Found ${mappingResponse.PagedData.length} store mappings for group ${storeGroupUID}`
      );

      // Step 2: Extract store UIDs from mappings
      // Log the actual property names to debug
      if (mappingResponse.PagedData.length > 0) {
        console.log(
          "🔍 Sample mapping object keys:",
          Object.keys(mappingResponse.PagedData[0])
        );
        console.log("🔍 Sample mapping object:", mappingResponse.PagedData[0]);
      }

      const storeUIDs = mappingResponse.PagedData.map((mapping) => {
        // Try different possible property names
        const uid =
          mapping.store_uid ||
          mapping.StoreUID ||
          mapping.storeUID ||
          mapping.store_UID;
        if (!uid) {
          console.log("⚠️ No store UID found in mapping:", mapping);
        }
        return uid;
      }).filter(Boolean) as string[];

      if (storeUIDs.length === 0) {
        console.log("❌ No valid store UIDs in mappings");
        return [];
      }

      console.log("🏪 Store UIDs to fetch:", storeUIDs);

      // Step 3: Get store details for each UID
      const storeRequest: StoreListRequest = {
        pageNumber: 1,
        pageSize: 2000, // Get a large set to ensure we have all stores
        filterCriterias: [],
        sortCriterias: [{ sortParameter: "Name", direction: 0 }],
        isCountRequired: false,
      };

      console.log("🔍 Fetching store details...");
      const storeResponse = await storeService.getAllStores(storeRequest);

      if (!storeResponse.pagedData || storeResponse.pagedData.length === 0) {
        console.log("❌ No stores found in system");
        return [];
      }

      console.log(
        `📊 Got ${storeResponse.pagedData.length} total stores from system`
      );

      // Log sample store to see property names
      if (storeResponse.pagedData.length > 0) {
        console.log(
          "🔍 Sample store object keys:",
          Object.keys(storeResponse.pagedData[0])
        );
        console.log(
          "🔍 First few stores UIDs:",
          storeResponse.pagedData.slice(0, 5).map((s) => ({
            UID: s.UID,
            uid: s.uid,
            Code: s.Code || s.code,
          }))
        );
      }

      // Step 4: Filter stores to only those in our group
      const groupStores = storeResponse.pagedData.filter((store) => {
        const storeUID = store.UID || store.uid;
        const match = storeUIDs.includes(storeUID);
        if (match) {
          console.log(`✅ Match found: Store ${storeUID} is in group`);
        }
        return match;
      });

      console.log(
        `✅ Found ${groupStores.length} stores in group out of ${storeResponse.pagedData.length} total stores`
      );
      console.log("🏪 Store UIDs we were looking for:", storeUIDs);

      return groupStores;
    } catch (error) {
      console.error("❌ Error fetching stores by group UID:", error);
      throw error;
    }
  }

  /**
   * Get mapping by UID
   */
  async getMappingByUID(uid: string): Promise<IStoreToGroupMapping | null> {
    try {
      const response = await apiService.get(
        `/StoreToGroupMapping/SelectStoreToGroupMappingByUID?UID=${uid}`
      );

      if (response && response.Data) {
        return response.Data;
      } else if (response) {
        return response;
      }

      return null;
    } catch (error) {
      console.error("Error fetching mapping by UID:", error);
      throw error;
    }
  }

  /**
   * Create new store-to-group mapping
   */
  async createMapping(
    mapping: Omit<
      IStoreToGroupMapping,
      "UID" | "CreatedTime" | "ServerAddTime" | "ServerModifiedTime"
    >
  ): Promise<number> {
    try {
      const now = new Date().toISOString();
      const mappingData = {
        ...mapping,
        CreatedTime: now,
        ModifiedTime: now,
        ServerAddTime: now,
        ServerModifiedTime: now,
      };

      const response = await apiService.post(
        "/StoreToGroupMapping/CreateStoreToGroupMapping",
        mappingData
      );
      return response.data || response;
    } catch (error) {
      console.error("Error creating store-to-group mapping:", error);
      throw error;
    }
  }

  /**
   * Update existing store-to-group mapping
   */
  async updateMapping(mapping: IStoreToGroupMapping): Promise<number> {
    try {
      const now = new Date().toISOString();
      const mappingData = {
        ...mapping,
        ModifiedTime: now,
        ServerModifiedTime: now,
      };

      const response = await apiService.put(
        "/StoreToGroupMapping/UpdateStoreToGroupMapping",
        mappingData
      );
      return response.data || response;
    } catch (error) {
      console.error("Error updating store-to-group mapping:", error);
      throw error;
    }
  }

  /**
   * Delete store-to-group mapping
   */
  async deleteMapping(uid: string): Promise<number> {
    try {
      const response = await apiService.delete(
        `/StoreToGroupMapping/DeleteStoreToGroupMapping?UID=${uid}`
      );
      return response.data || response;
    } catch (error) {
      console.error("Error deleting store-to-group mapping:", error);
      throw error;
    }
  }
}

// Export singleton instance
export const storeToGroupMappingService = new StoreToGroupMappingService();

// Export class for testing
export default StoreToGroupMappingService;
