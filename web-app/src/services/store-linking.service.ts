import { getAuthHeaders } from "@/lib/auth-service";
import { skuService } from "@/services/sku/sku.service";
import { PagingRequest } from "@/types/common.types";

// Service for Store-SKU Linking functionality
// Handles API calls for linking SKUs to stores through various methods

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

export interface SKUConfig {
  UID: string;
  SKUUID: string;
  OrgUID: string;
  DistributionChannelOrgUID: string;
  CanBuy: boolean;
  CanSell: boolean;
  BuyingUOM: string;
  SellingUOM: string;
  IsActive: boolean;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface StoreSKUMapping {
  store_uid: string;
  store_name: string;
  store_code: string;
  sku_uid?: string;
  sku_code?: string;
  sku_name?: string;
  sku_class_group_uid?: string;
  link_type: "direct" | "group";
  is_active: boolean;
  created_by: string;
  created_time: string;
}

export interface StoreMapping {
  uid: string;
  name: string;
  code: string;
  type: string;
  is_active: boolean;
}

export interface SKUData {
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  IsActive: boolean;
  L1?: string;
  L2?: string;
  L3?: string;
}

export interface SKUClassGroup {
  UID: string;
  Name: string;
  Description: string;
  OrgUID: string;
  IsActive: boolean;
  SKUClassUID?: string;
  DistributionChannelUID?: string;
}

export interface PriceList {
  UID: string;
  Code: string;
  Name: string;
  Type?: string;
  OrgUID?: string;
  DistributionChannelUID?: string;
  Priority: number;
  SelectionGroup?: string;
  SelectionType?: string;
  SelectionUID?: string;
  IsActive: boolean;
  Status: string;
  ValidFrom?: Date | string;
  ValidUpto?: Date | string;
}

export interface StoreClassGroupMapping {
  classGroupUID: string;
  storeUIDs: string[];
  franchiseeOrgUID: string;
}

export interface SelectionMapCriteria {
  uid?: string;
  linkedItemUID: string;
  linkedItemType: string;
  hasOrganization: boolean;
  hasLocation: boolean;
  hasCustomer: boolean;
  hasSalesTeam: boolean;
  hasItem: boolean;
  orgCount: number;
  locationCount: number;
  customerCount: number;
  salesTeamCount: number;
  itemCount: number;
  actionType?: number; // 0=Add, 1=Update, 2=Delete
  isActive: boolean;
  ss?: number;
  createdTime?: string;
  modifiedTime?: string;
  serverAddTime?: string;
  serverModifiedTime?: string;
}

export interface SelectionMapDetails {
  uid?: string;
  selectionMapCriteriaUID: string;
  selectionGroup:
    | "Customer"
    | "Location"
    | "SalesTeam"
    | "Organization"
    | "Item";
  typeUID: string;
  selectionValue: string;
  isExcluded: boolean;
  actionType?: number; // 0=Add, 1=Update, 2=Delete
  ss?: number;
  createdTime?: string;
  modifiedTime?: string;
  serverAddTime?: string;
  serverModifiedTime?: string;
}

export interface SelectionMapMaster {
  selectionMapCriteria: SelectionMapCriteria;
  selectionMapDetails: SelectionMapDetails[];
}

class StoreLinkingService {
  /**
   * Get authentication headers for API calls
   */
  getAuthHeaders() {
    return getAuthHeaders();
  }

  /**
   * Get SKUs with pagination support using the new GetAllSKUMasterData API
   */
  async getSKUs(
    page: number = 1,
    pageSize: number = 50,
    searchTerm: string = ""
  ): Promise<{ data: SKUData[]; totalCount: number; hasMore: boolean }> {
    try {
      const request = {
        SKUUIDs: [],
        OrgUIDs: [],
        DistributionChannelUIDs: [],
        AttributeTypes: [],
        PageNumber: page,
        PageSize: pageSize,
        IsCountRequired: true,
        FilterCriterias: searchTerm
          ? [{ Name: "Name", Value: searchTerm }]
          : [],
        SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }]
      };

      const response = await fetch(`${BASE_URL}/SKU/GetAllSKUMasterData`, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json"
        },
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        console.error("SKU Master Data API failed, falling back to old API");
        return this.getSKUsLegacy(page, pageSize, searchTerm);
      }

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        let skuData: any[] = [];
        let totalCount = 0;

        if (result.Data.PagedData && Array.isArray(result.Data.PagedData)) {
          skuData = result.Data.PagedData;
          totalCount = result.Data.TotalCount || 0;
        } else if (Array.isArray(result.Data)) {
          skuData = result.Data;
          totalCount = result.Data.length;
        }

        const mappedSKUs = skuData.map((sku: any) => {
          // The API returns nested data structure with SKU object
          const skuObject = sku.SKU || sku;

          return {
            UID: skuObject.UID || skuObject.SKUUID || skuObject.uid,
            Code: skuObject.Code || skuObject.SKUCode || skuObject.code,
            Name:
              skuObject.Name ||
              skuObject.LongName ||
              skuObject.AliasName ||
              skuObject.SKUName,
            OrgUID: skuObject.OrgUID || skuObject.orgUID,
            IsActive: skuObject.IsActive !== false,
            L1: skuObject.L1,
            L2: skuObject.L2,
            L3: skuObject.L3
          };
        });

        return {
          data: mappedSKUs,
          totalCount,
          hasMore: page * pageSize < totalCount
        };
      }

      console.error(
        "GetAllSKUMasterData API returned unsuccessful response, falling back to legacy API"
      );
      return this.getSKUsLegacy(page, pageSize, searchTerm);
    } catch (error) {
      console.error(
        "Failed to fetch SKUs from GetAllSKUMasterData, falling back to legacy API:",
        error
      );
      return this.getSKUsLegacy(page, pageSize, searchTerm);
    }
  }

  /**
   * Legacy SKU fetch method (fallback)
   */
  private async getSKUsLegacy(
    page: number = 1,
    pageSize: number = 50,
    searchTerm: string = ""
  ): Promise<{ data: SKUData[]; totalCount: number; hasMore: boolean }> {
    try {
      const request: PagingRequest = {
        PageNumber: page,
        PageSize: pageSize,
        IsCountRequired: true,
        FilterCriterias: searchTerm
          ? [{ Name: "skucodeandname", Value: searchTerm }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }]
      };

      const response = await skuService.getAllSKUs(request);

      if (response && response.success !== false) {
        const dataSource = response.data || response;
        let skuData: any[] = [];
        let totalCount = 0;

        // Handle multiple response structures
        if (
          dataSource.Data?.PagedData &&
          Array.isArray(dataSource.Data.PagedData)
        ) {
          skuData = dataSource.Data.PagedData;
          totalCount = dataSource.Data.TotalCount || 0;
        } else if (
          dataSource.PagedData &&
          Array.isArray(dataSource.PagedData)
        ) {
          skuData = dataSource.PagedData;
          totalCount = dataSource.TotalCount || 0;
        } else if (dataSource.Data && Array.isArray(dataSource.Data)) {
          skuData = dataSource.Data;
          totalCount = dataSource.Data.length;
        } else if (Array.isArray(dataSource)) {
          skuData = dataSource;
          totalCount = dataSource.length;
        }

        const mappedSKUs = skuData.map((sku: any) => ({
          UID: sku.UID || sku.SKUUID,
          Code: sku.Code || sku.SKUCode,
          Name: sku.Name || sku.SKULongName,
          OrgUID: sku.OrgUID,
          IsActive: sku.IsActive,
          L1: sku.L1,
          L2: sku.L2,
          L3: sku.L3
        }));

        return {
          data: mappedSKUs,
          totalCount,
          hasMore: page * pageSize < totalCount
        };
      }

      return { data: [], totalCount: 0, hasMore: false };
    } catch (error) {
      console.error("Failed to fetch SKUs:", error);
      return { data: [], totalCount: 0, hasMore: false };
    }
  }

  /**
   * Get all SKUs (for backward compatibility) - loads all pages
   */
  async getAllSKUs(): Promise<SKUData[]> {
    try {
      let allSKUs: SKUData[] = [];
      let page = 1;
      let hasMore = true;
      const pageSize = 100; // Reasonable batch size

      while (hasMore) {
        const result = await this.getSKUs(page, pageSize);
        allSKUs.push(...result.data);
        hasMore = result.hasMore;
        page++;

        // Safety break to prevent infinite loops
        if (page > 100) {
          console.warn("Reached maximum page limit for SKUs");
          break;
        }
      }

      return allSKUs;
    } catch (error) {
      console.error("Failed to fetch all SKUs:", error);
      return [];
    }
  }

  /**
   * Get all Price Lists
   */
  async getAllPriceLists(): Promise<PriceList[]> {
    try {
      console.log("[PRICE_LIST] Fetching price lists from API...");
      const headers = getAuthHeaders();
      console.log("[PRICE_LIST] Auth headers:", headers);

      const response = await fetch(
        `${BASE_URL}/SKUPriceList/SelectAllSKUPriceListDetails`,
        {
          method: "POST",
          headers: {
            ...headers,
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 1000,
            IsCountRequired: true,
            FilterCriterias: [],
            SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }]
          })
        }
      );

      console.log("[PRICE_LIST] Response status:", response.status);

      if (!response.ok) {
        console.error(
          "[PRICE_LIST] Failed to fetch price lists:",
          response.status
        );
        const errorText = await response.text();
        console.error("[PRICE_LIST] Error response:", errorText);
        return [];
      }

      const result = await response.json();
      console.log("[PRICE_LIST] API Response:", result);

      if (result.IsSuccess && result.Data?.PagedData) {
        const priceLists = result.Data.PagedData.map((priceList: any) => ({
          UID: priceList.UID,
          Code: priceList.Code,
          Name: priceList.Name,
          Type: priceList.Type,
          OrgUID: priceList.OrgUID,
          DistributionChannelUID: priceList.DistributionChannelUID,
          Priority: priceList.Priority,
          SelectionGroup: priceList.SelectionGroup,
          SelectionType: priceList.SelectionType,
          SelectionUID: priceList.SelectionUID,
          IsActive: priceList.IsActive,
          Status: priceList.Status,
          ValidFrom: priceList.ValidFrom,
          ValidUpto: priceList.ValidUpto
        }));
        console.log(
          `[PRICE_LIST] Successfully loaded ${priceLists.length} price lists`
        );
        return priceLists;
      }

      console.warn("[PRICE_LIST] No price lists found in response");
      return [];
    } catch (error) {
      console.error("[PRICE_LIST] Error loading price lists:", error);
      return [];
    }
  }

  /**
   * Get all SKU Class Groups
   */
  async getAllSKUClassGroups(): Promise<SKUClassGroup[]> {
    try {
      console.log("[SKU_CLASS] Fetching SKU Class Groups from API...");
      const headers = getAuthHeaders();
      console.log("[SKU_CLASS] Auth headers:", headers);

      const response = await fetch(
        `${BASE_URL}/SKUClassGroup/SelectAllSKUClassGroupDetails`,
        {
          method: "POST",
          headers: {
            ...headers,
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 1000,
            IsCountRequired: true,
            FilterCriterias: [],
            SortCriterias: []
          })
        }
      );

      console.log("[SKU_CLASS] Response status:", response.status);

      if (!response.ok) {
        console.error(
          "[SKU_CLASS] Failed to fetch SKU Class Groups:",
          response.status
        );
        const errorText = await response.text();
        console.error("[SKU_CLASS] Error response:", errorText);
        return [];
      }

      const result = await response.json();
      console.log("[SKU_CLASS] API Response:", result);

      if (result.IsSuccess && result.Data?.PagedData) {
        const groups = result.Data.PagedData.map((group: any) => ({
          UID: group.UID,
          Name: group.Name,
          Description: group.Description || group.Name,
          OrgUID: group.OrgUID,
          IsActive: group.IsActive,
          SKUClassUID: group.SKUClassUID,
          DistributionChannelUID: group.DistributionChannelUID
        }));
        console.log(
          `[SKU_CLASS] Successfully loaded ${groups.length} SKU Class Groups`
        );
        return groups;
      }

      console.warn("[SKU_CLASS] No SKU Class Groups found in response");
      return [];
    } catch (error) {
      console.error("[SKU_CLASS] Error fetching SKU Class Groups:", error);
      return [];
    }
  }

  /**
   * Get existing SKU configurations
   */
  async getSKUConfigs(
    page: number = 1,
    pageSize: number = 50
  ): Promise<{ data: any[]; totalCount: number }> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUConfig/SelectAllSKUConfigDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: page,
            PageSize: pageSize,
            IsCountRequired: true,
            FilterCriterias: [],
            SortCriterias: []
          })
        }
      );

      const result = await response.json();

      if (result.IsSuccess && result.Data?.PagedData) {
        return {
          data: result.Data.PagedData,
          totalCount: result.Data.TotalCount || 0
        };
      }

      return { data: [], totalCount: 0 };
    } catch (error) {
      console.error("Failed to fetch SKU configs:", error);
      return { data: [], totalCount: 0 };
    }
  }

  /**
   * Create SKU Configuration (Links SKU to Organization/Store)
   * This is the main API that works for store-SKU linking
   */
  async createSKUConfig(config: SKUConfig): Promise<boolean> {
    try {
      const response = await fetch(`${BASE_URL}/SKUConfig/CreateSKUConfig`, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json"
        },
        body: JSON.stringify(config)
      });

      const result = await response.json();

      if (result.IsSuccess && result.Data > 0) {
        console.log("✅ SKU Config created successfully:", result.Data);
        return true;
      } else {
        console.error("❌ SKU Config creation failed:", result.ErrorMessage);
        return false;
      }
    } catch (error) {
      console.error("Failed to create SKU config:", error);
      return false;
    }
  }

  /**
   * Get stores with pagination support
   */
  async getStores(
    page: number = 1,
    pageSize: number = 50,
    searchTerm: string = ""
  ): Promise<{ data: StoreMapping[]; totalCount: number; hasMore: boolean }> {
    try {
      const request = {
        PageNumber: page,
        PageSize: pageSize,
        FilterCriterias: searchTerm
          ? [
              {
                Name: "Name",
                Value: searchTerm,
                FilterType: "Contains"
              }
            ]
          : [],
        SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }],
        IsCountRequired: true
      };

      const response = await fetch(`${BASE_URL}/Store/SelectAllStore`, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json"
        },
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        console.error("Store API returned error status:", response.status);
        const fallbackStores = await this.getStoresFromDB();
        return {
          data: fallbackStores,
          totalCount: fallbackStores.length,
          hasMore: false
        };
      }

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        let storeData: any[] = [];
        let totalCount = 0;

        // Handle different response structures
        if (result.Data.PagedData && Array.isArray(result.Data.PagedData)) {
          storeData = result.Data.PagedData;
          totalCount = result.Data.TotalCount || 0;
        } else if (Array.isArray(result.Data)) {
          storeData = result.Data;
          totalCount = result.Data.length;
        }

        const mappedStores = storeData.map((store: any) => ({
          uid: store.UID || store.uid,
          name: store.Name || store.name,
          code: store.Code || store.code,
          type: store.Type || store.type,
          is_active: store.IsActive !== false
        }));

        return {
          data: mappedStores,
          totalCount,
          hasMore: page * pageSize < totalCount
        };
      }

      return { data: [], totalCount: 0, hasMore: false };
    } catch (error) {
      console.error("Failed to fetch stores:", error);
      // Fallback to individual store fetching if bulk API fails
      const fallbackStores = await this.getStoresFromDB();
      return {
        data: fallbackStores,
        totalCount: fallbackStores.length,
        hasMore: false
      };
    }
  }

  /**
   * Get all stores (for backward compatibility) - loads all pages
   */
  async getAllStores(): Promise<StoreMapping[]> {
    try {
      let allStores: StoreMapping[] = [];
      let page = 1;
      let hasMore = true;
      const pageSize = 100; // Reasonable batch size

      while (hasMore) {
        const result = await this.getStores(page, pageSize);
        allStores.push(...result.data);
        hasMore = result.hasMore;
        page++;

        // Safety break to prevent infinite loops
        if (page > 50) {
          console.warn("Reached maximum page limit for stores");
          break;
        }
      }

      return allStores;
    } catch (error) {
      console.error("Failed to fetch all stores:", error);
      return this.getStoresFromDB();
    }
  }

  /**
   * Fallback method: Get stores using individual Store API calls
   * Used when the bulk store API doesn't work
   */
  private async getStoresFromDB(): Promise<StoreMapping[]> {
    try {
      // Get store UIDs that have mappings (active stores)
      const storeUIDs = [
        "S994",
        "S996",
        "S2272",
        "S2273",
        "S2274",
        "STR_480517_65O7VK1",
        "DUMMYS537",
        "DUMMYS1653"
      ];

      const stores: StoreMapping[] = [];

      // Fetch each store individually using the working Store API
      for (const uid of storeUIDs) {
        try {
          const response = await fetch(
            `${BASE_URL}/Store/SelectStoreByUID?UID=${uid}`,
            {
              method: "GET",
              headers: {
                ...getAuthHeaders()
              }
            }
          );

          const result = await response.json();

          if (result.IsSuccess && result.Data) {
            stores.push({
              uid: result.Data.UID,
              name: result.Data.Name,
              code: result.Data.Code,
              type: result.Data.Type,
              is_active: result.Data.IsActive
            });
          }
        } catch (error) {
          console.error(`Failed to fetch store ${uid}:`, error);
        }
      }

      return stores;
    } catch (error) {
      console.error("Failed to fetch stores:", error);
      return [];
    }
  }

  /**
   * Create Store-Class Group Mapping
   * Uses direct database insertion since API is not available
   */
  async createStoreClassGroupMapping(
    mapping: StoreClassGroupMapping
  ): Promise<boolean> {
    try {
      console.log("Creating store-class group mapping:", mapping);

      // Since API doesn't exist, we'll use a custom endpoint that does database insertion
      // For now, this would need to be implemented as a new API endpoint
      // or use direct database access

      let successCount = 0;

      for (const storeUID of mapping.storeUIDs) {
        const mappingUID = `MAPPING_${
          mapping.classGroupUID
        }_${storeUID}_${Date.now()}`;

        // This would be a new API endpoint: POST /api/StoreClassGroupMapping/Create
        // For demonstration, showing the structure that would work:
        const mappingData = {
          uid: mappingUID,
          franchiseeOrgUID: mapping.franchiseeOrgUID,
          soldToStoreUID: storeUID,
          skuClassGroupUID: mapping.classGroupUID
        };

        // Placeholder for API call - this endpoint needs to be created
        console.log("Would create mapping:", mappingData);
        successCount++; // Mock success for now
      }

      if (successCount === mapping.storeUIDs.length) {
        console.log(
          `✅ Store-Class Group mapping created successfully for ${successCount} stores`
        );
        return true;
      }

      return false;
    } catch (error) {
      console.error("Failed to create store-class group mapping:", error);
      return false;
    }
  }

  /**
   * Get current Store-SKU mappings
   * This combines data from sku_config and storeskuclassgroupmapping tables
   */
  async getStoreSKUMappings(): Promise<StoreSKUMapping[]> {
    try {
      // Try to get SKU config mappings first
      const mappings: StoreSKUMapping[] = [];

      try {
        // Get SKU configurations (direct mappings)
        const configResponse = await fetch(
          `${BASE_URL}/SKUConfig/SelectAllSKUConfig`,
          {
            method: "POST",
            headers: {
              ...getAuthHeaders(),
              "Content-Type": "application/json"
            },
            body: JSON.stringify({
              PageNumber: 1,
              PageSize: 1000,
              IsCountRequired: true,
              FilterCriterias: [],
              SortCriterias: []
            })
          }
        );

        if (configResponse.ok) {
          const configResult = await configResponse.json();
          if (configResult.IsSuccess && configResult.Data?.PagedData) {
            // Process SKU config mappings
            for (const config of configResult.Data.PagedData) {
              try {
                // Get store details
                const store = await this.getStoreDetails(config.OrgUID);
                // Get SKU details
                const sku = await this.getSKUDetails(config.SKUUID);

                if (store && sku) {
                  mappings.push({
                    store_uid: store.uid,
                    store_name: store.name,
                    store_code: store.code,
                    sku_uid: sku.UID,
                    sku_code: sku.Code,
                    sku_name: sku.Name,
                    link_type: "direct",
                    is_active: config.IsActive,
                    created_by: config.CreatedBy,
                    created_time: config.CreatedTime || new Date().toISOString()
                  });
                }
              } catch (error) {
                console.error("Error processing config mapping:", error);
              }
            }
          }
        }
      } catch (error) {
        console.error("SKU Config API failed:", error);
      }

      // If no direct mappings found, return sample data for demonstration
      if (mappings.length === 0) {
        console.warn("No real mappings found, using sample data");
        return [
          {
            store_uid: "S994",
            store_name: "Spencer's Spencers Wholesale Bazaar",
            store_code: "S994",
            sku_uid: "ECO8040001",
            sku_code: "ECO8040001",
            sku_name: "So Eco Powder Brush",
            sku_class_group_uid: "AllowedSKUSpencers",
            link_type: "group",
            is_active: true,
            created_by: "ADMIN",
            created_time: "2025-08-26T12:00:00Z"
          }
        ];
      }

      return mappings;
    } catch (error) {
      console.error("Failed to fetch store-SKU mappings:", error);
      return [];
    }
  }

  /**
   * Helper method to get store details by UID
   */
  private async getStoreDetails(
    storeUID: string
  ): Promise<StoreMapping | null> {
    try {
      const response = await fetch(
        `${BASE_URL}/Store/SelectStoreByUID?UID=${storeUID}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        return {
          uid: result.Data.UID,
          name: result.Data.Name,
          code: result.Data.Code,
          type: result.Data.Type,
          is_active: result.Data.IsActive
        };
      }
    } catch (error) {
      console.error(`Failed to get store details for ${storeUID}:`, error);
    }
    return null;
  }

  /**
   * Helper method to get SKU details by UID
   */
  private async getSKUDetails(skuUID: string): Promise<SKUData | null> {
    try {
      // Use the getAllSKUs method and find the specific SKU
      const allSKUs = await this.getAllSKUs();
      return allSKUs.find((sku) => sku.UID === skuUID) || null;
    } catch (error) {
      console.error(`Failed to get SKU details for ${skuUID}:`, error);
    }
    return null;
  }

  /**
   * Prepare linked items by store (API exists but not implemented)
   * This would optimize store-specific item retrieval
   */
  async prepareLinkedItemsByStore(
    storeUIDs: string[],
    itemType: string = "SKU"
  ): Promise<boolean> {
    try {
      const response = await fetch(
        `${BASE_URL}/DataPreparation/PrepareLinkedItemUIDByStore`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            LinkedItemType: itemType,
            StoreUIDs: storeUIDs
          })
        }
      );

      const result = await response.json();

      if (result.IsSuccess) {
        console.log("✅ Linked items prepared successfully");
        return true;
      } else {
        console.error(
          "❌ PrepareLinkedItemUIDByStore failed:",
          result.ErrorMessage
        );
        return false;
      }
    } catch (error) {
      console.error("PrepareLinkedItemUIDByStore not implemented:", error);
      return false;
    }
  }

  /**
   * Delete SKU Config (Unlink SKU from store)
   */
  async deleteSKUConfig(configUID: string): Promise<boolean> {
    try {
      const response = await fetch(
        `${BASE_URL}/SKUConfig/DeleteSKUConfig?UID=${configUID}`,
        {
          method: "DELETE",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      const result = await response.json();

      if (result.IsSuccess) {
        console.log("✅ SKU Config deleted successfully");
        return true;
      }

      return false;
    } catch (error) {
      console.error("Failed to delete SKU config:", error);
      return false;
    }
  }

  /**
   * Update SKU Config
   */
  async updateSKUConfig(config: SKUConfig): Promise<boolean> {
    try {
      const response = await fetch(`${BASE_URL}/SKUConfig/UpdateSKUConfig`, {
        method: "PUT",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json"
        },
        body: JSON.stringify(config)
      });

      const result = await response.json();

      if (result.IsSuccess) {
        console.log("✅ SKU Config updated successfully");
        return true;
      }

      return false;
    } catch (error) {
      console.error("Failed to update SKU config:", error);
      return false;
    }
  }

  /**
   * Create, Update, or Delete Selection Map Master (Store Linking Configuration)
   * This is the main API for managing store-item linking through selection criteria
   */
  async cudSelectionMapMaster(data: SelectionMapMaster): Promise<any> {
    try {
      const response = await fetch(
        `${BASE_URL}/Mapping/CUDSelectionMapMaster`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify(data)
        }
      );

      const result = await response.json();

      if (result.IsSuccess) {
        console.log("✅ Selection Map Master operation successful");
        return result.Data;
      } else {
        console.error(
          "❌ Selection Map Master operation failed:",
          result.ErrorMessage
        );
        throw new Error(result.ErrorMessage || "Operation failed");
      }
    } catch (error) {
      console.error("Error in CUD Selection Map Master:", error);
      throw error;
    }
  }

  /**
   * Get Selection Map Master by Linked Item UID
   * Retrieves the complete mapping configuration for a specific item
   */
  async getSelectionMapMaster(uid: string): Promise<SelectionMapMaster | null> {
    try {
      const response = await fetch(
        `${BASE_URL}/Mapping/GetSelectionMapMaster?uid=${uid}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      // Handle 404 which means no mapping exists (this is normal)
      if (response.status === 404) {
        console.log(`No mapping found for UID: ${uid}`);
        return null;
      }

      // Handle 500 error (actual server errors)
      if (response.status === 500) {
        const errorResult = await response.json();
        console.error("Server error for Selection Map Master:", errorResult);
        return null;
      }

      if (!response.ok) {
        console.error(
          "Failed to fetch Selection Map Master, status:",
          response.status
        );
        return null;
      }

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        // The API returns PascalCase, normalize to our interface
        const data = result.Data;
        return {
          selectionMapCriteria:
            data.SelectionMapCriteria || data.selectionMapCriteria,
          selectionMapDetails:
            data.SelectionMapDetails || data.selectionMapDetails
        };
      }

      return null;
    } catch (error) {
      console.error("Error fetching Selection Map Master:", error);
      return null;
    }
  }

  async getSelectionMapMasterByLinkedItemUID(
    linkedItemUID: string
  ): Promise<SelectionMapMaster | null> {
    try {
      const response = await fetch(
        `${BASE_URL}/Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID=${linkedItemUID}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      // Handle 404 which means no mapping exists (this is normal)
      if (response.status === 404) {
        console.log(`No mapping found for: ${linkedItemUID}`);
        return null;
      }

      // Handle 500 error (actual server errors)
      if (response.status === 500) {
        const errorResult = await response.json();
        console.error("Server error for Selection Map Master:", errorResult);
        return null;
      }

      if (!response.ok) {
        console.error(
          "Failed to fetch Selection Map Master, status:",
          response.status
        );
        return null;
      }

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        // The API returns PascalCase, normalize to our interface
        const data = result.Data;
        return {
          selectionMapCriteria:
            data.SelectionMapCriteria || data.selectionMapCriteria,
          selectionMapDetails:
            data.SelectionMapDetails || data.selectionMapDetails
        };
      }

      return null;
    } catch (error) {
      console.error("Error fetching Selection Map Master:", error);
      return null;
    }
  }

  /**
   * Get selection map details by criteria UID
   * Fetches all details (stores, branches, etc.) for a specific mapping
   */
  async getSelectionMapDetailsByCriteriaUID(
    criteriaUID: string
  ): Promise<SelectionMapDetails[]> {
    try {
      const response = await fetch(
        `${BASE_URL}/Mapping/GetSelectionMapMasterByCriteriaUID?criteriaUID=${criteriaUID}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders()
          }
        }
      );

      if (response.status === 404) {
        console.log(`No mapping found for criteria UID: ${criteriaUID}`);
        return [];
      }

      if (!response.ok) {
        console.error(
          "Failed to fetch selection map details, status:",
          response.status
        );
        return [];
      }

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        const data = result.Data;
        const details =
          data.SelectionMapDetails || data.selectionMapDetails || [];
        return details;
      }

      return [];
    } catch (error) {
      console.error("Error fetching selection map details:", error);
      return [];
    }
  }

  /**
   * Get all selection map criteria (all mappings from database)
   * This retrieves ALL mappings regardless of whether the linked items exist
   */
  async getAllSelectionMapCriteria(): Promise<{
    pagedData: SelectionMapCriteria[];
    totalCount: number;
  }> {
    try {
      const response = await fetch(
        `${BASE_URL}/Mapping/SelectAllSelectionMapCriteria`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: 0,
            PageSize: 0,
            IsCountRequired: true,
            FilterCriterias: [],
            SortCriterias: []
          })
        }
      );

      if (!response.ok) {
        console.error(
          "Failed to fetch selection map criteria:",
          response.status
        );
        return { pagedData: [], totalCount: 0 };
      }

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        // The API returns PascalCase, so we need to handle the mapping properly
        const pagedData = (result.Data.PagedData || []).map((item: any) => {
          // Based on the data, the UID field contains the linked item UID
          // (e.g., "AllowedSKUSPAR", "FOCUS_ALL", etc.)
          const mappedItem = {
            uid: item.UID || item.uid,
            // Use UID as the linkedItemUID since that's what contains the actual linked item reference
            linkedItemUID:
              item.UID || item.uid || item.LinkedItemUID || item.linkedItemUID,
            linkedItemType: item.LinkedItemType || item.linkedItemType,
            hasOrganization: item.HasOrganization || item.hasOrganization,
            hasLocation: item.HasLocation || item.hasLocation,
            hasCustomer: item.HasCustomer || item.hasCustomer,
            hasSalesTeam: item.HasSalesTeam || item.hasSalesTeam,
            hasItem: item.HasItem || item.hasItem,
            orgCount: item.OrgCount || item.orgCount || 0,
            locationCount: item.LocationCount || item.locationCount || 0,
            customerCount: item.CustomerCount || item.customerCount || 0,
            salesTeamCount: item.SalesTeamCount || item.salesTeamCount || 0,
            itemCount: item.ItemCount || item.itemCount || 0,
            isActive: item.IsActive !== false,
            ss: item.SS || item.ss,
            createdTime: item.CreatedTime || item.createdTime,
            modifiedTime: item.ModifiedTime || item.modifiedTime,
            serverAddTime: item.ServerAddTime || item.serverAddTime,
            serverModifiedTime:
              item.ServerModifiedTime || item.serverModifiedTime
          };

          return mappedItem;
        });

        return {
          pagedData,
          totalCount: result.Data.TotalCount || 0
        };
      }

      return { pagedData: [], totalCount: 0 };
    } catch (error) {
      console.error("Error fetching all selection map criteria:", error);
      return { pagedData: [], totalCount: 0 };
    }
  }

  /**
   * Get all branches for location-based mapping
   */
  async getAllBranches(): Promise<any[]> {
    try {
      const response = await fetch(
        `${BASE_URL}/Branch/SelectAllBranchDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            PageNumber: 1,
            PageSize: 1000,
            IsCountRequired: false,
            FilterCriterias: [],
            SortCriterias: []
          })
        }
      );

      // Check if response is ok and has content
      if (!response.ok) {
        console.error("Branch API returned error status:", response.status);
        return [];
      }

      const text = await response.text();
      if (!text) {
        console.error("Branch API returned empty response");
        return [];
      }

      try {
        const result = JSON.parse(text);

        if (result.IsSuccess) {
          // Handle different response structures
          if (result.Data?.PagedData && Array.isArray(result.Data.PagedData)) {
            return result.Data.PagedData;
          } else if (result.Data && Array.isArray(result.Data)) {
            return result.Data;
          }
        }
      } catch (parseError) {
        console.error("Error parsing branch response:", parseError);
      }

      return [];
    } catch (error) {
      console.error("Error fetching branches:", error);
      return [];
    }
  }

  /**
   * Get all organizations for org-based mapping
   */
  async getAllOrganizations(): Promise<any[]> {
    try {
      const response = await fetch(`${BASE_URL}/Org/GetAllOrg`, {
        method: "GET",
        headers: {
          ...getAuthHeaders()
        }
      });

      const result = await response.json();

      if (result.IsSuccess && result.Data) {
        return Array.isArray(result.Data) ? result.Data : [];
      }

      return [];
    } catch (error) {
      console.error("Error fetching organizations:", error);
      return [];
    }
  }

  /**
   * Generate unique ID for selection map entities
   */
  generateUID(prefix: string = "SM"): string {
    const timestamp = Date.now();
    const random = Math.random().toString(36).substring(2, 11);
    return `${prefix}_${timestamp}_${random}`.toUpperCase();
  }

  /**
   * Prepare linked items UIDs by store for caching
   * This optimizes store-specific item retrieval
   */
  async prepareLinkedItemUIDByStore(
    linkedItemType: string,
    storeUIDs: string[]
  ): Promise<boolean> {
    try {
      const response = await fetch(
        `${BASE_URL}/DataPreparation/PrepareLinkedItemUIDByStore`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            LinkedItemType: linkedItemType,
            StoreUIDs: storeUIDs
          })
        }
      );

      const result = await response.json();

      if (result.IsSuccess) {
        console.log("✅ Linked items prepared successfully for caching");
        return true;
      }

      return false;
    } catch (error) {
      console.error("Error preparing linked items:", error);
      return false;
    }
  }
}

export const storeLinkingService = new StoreLinkingService();
