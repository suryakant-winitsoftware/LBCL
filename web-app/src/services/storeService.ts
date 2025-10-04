/**
 * Store Service
 * Handles all Store/Customer related API calls
 * Uses existing authentication from api.ts
 */

import { apiService } from "./api";
import { authService } from "@/lib/auth-service";
import { getCurrentUser } from "@/utils/auth";
import {
  IStore,
  IStoreMaster,
  IStoreAdditionalInfo,
  IStoreCredit,
  IContact,
  IAddress,
  IStoreCustomer,
  IRouteCustomer,
  StoreListRequest,
  PagedResponse,
  ApiResponse,
  StoreFormData,
  StoreSearchParams,
} from "@/types/store.types";

class StoreService {
  private getBaseUrl(): string {
    return process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";
  }

  /**
   * Get all stores with pagination and filtering
   * Uses NEW OPTIMIZED SelectAllStoreBasic API - 100x faster!
   * Uses authenticated API service with Bearer token
   */
  async getAllStores(
    request: StoreListRequest
  ): Promise<PagedResponse<IStore>> {
    try {
      // Use the new optimized API endpoint that's 100x faster
      const response = await apiService.post(
        "/Store/SelectAllStoreBasic",
        request
      );
      // Only log errors, not successful responses to reduce noise

      // Handle different response structures from .NET API
      if (response && typeof response === "object") {
        // Check for capital case PagedData (from .NET API)
        if (response.PagedData !== undefined) {
          return {
            pagedData: response.PagedData,
            totalCount: response.TotalCount || 0,
          };
        }
        // If response has Data property with PagedData (wrapped response)
        if (response.Data && response.Data.PagedData !== undefined) {
          // If we got empty data, just return empty - don't make expensive fallback calls
          if (
            response.Data.PagedData.length === 0 &&
            response.Data.TotalCount === 0
          ) {
            console.warn(
              "Store API returned empty data - check filters or data availability"
            );
            // Return empty data instead of expensive fallback that loads 200+ records
          }
          return {
            pagedData: response.Data.PagedData,
            totalCount: response.Data.TotalCount || 0,
          };
        }
        // If response has data property with PagedData
        if (response.data && response.data.PagedData !== undefined) {
          return {
            pagedData: response.data.PagedData,
            totalCount: response.data.TotalCount || 0,
          };
        }
        // If response has lowercase pagedData
        if (response.pagedData !== undefined) {
          return response;
        }
        // If response has data property with lowercase pagedData
        if (response.data && response.data.pagedData !== undefined) {
          return response.data;
        }
      }

      // Return empty response if structure is not recognized
      console.warn("Unexpected API response structure - check backend");
      return {
        pagedData: [],
        totalCount: 0,
      };
    } catch (error) {
      console.error("Error fetching stores:", error);
      throw error;
    }
  }

  /**
   * Get all stores using OLD SLOW API (for compatibility only)
   * @deprecated Use getAllStores() instead which uses the faster SelectAllStoreBasic
   */
  async getAllStoresOld(
    request: StoreListRequest
  ): Promise<PagedResponse<IStore>> {
    try {
      const response = await apiService.post("/Store/SelectAllStore", request);

      if (response && typeof response === "object") {
        if (response.PagedData !== undefined) {
          return {
            pagedData: response.PagedData,
            totalCount: response.TotalCount || 0,
          };
        }
        if (response.Data && response.Data.PagedData !== undefined) {
          return {
            pagedData: response.Data.PagedData,
            totalCount: response.Data.TotalCount || 0,
          };
        }
      }

      return {
        pagedData: [],
        totalCount: 0,
      };
    } catch (error) {
      console.error("Error fetching stores (old API):", error);
      throw error;
    }
  }

  /**
   * Get store by UID - Uses SelectAllStore without filters then finds by UID
   * Uses authenticated API service with Bearer token
   */
  async getStoreByUID(uid: string): Promise<IStore> {
    try {
      console.log(
        `🔍 StoreService.getStoreByUID: Fetching store with UID=${uid}`
      );

      // First try with a filter to get the specific store
      const filteredRequest: StoreListRequest = {
        pageNumber: 1,
        pageSize: 10,
        filterCriterias: [
          {
            name: "UID",
            value: uid,
            operator: "equals",
          },
        ],
        sortCriterias: [],
        isCountRequired: false,
      };

      console.log("📞 Trying filtered search for specific UID:", uid);
      try {
        const filteredResponse = await this.getAllStores(filteredRequest);
        if (
          filteredResponse.pagedData &&
          filteredResponse.pagedData.length > 0
        ) {
          console.log("✅ Found store using filtered search");
          return filteredResponse.pagedData[0];
        }
      } catch (filterError) {
        console.log("⚠️ Filtered search failed, trying unfiltered approach");
      }

      // Fallback: Use SelectAllStore WITHOUT filters to avoid SQL bug, then filter client-side
      const request: StoreListRequest = {
        pageNumber: 1,
        pageSize: 50, // Get more stores to increase chance of finding our target
        filterCriterias: [], // NO FILTERS to avoid SQL bug
        sortCriterias: [],
        isCountRequired: false,
      };

      console.log("📞 Getting store list and filtering for UID:", uid);
      const response = await this.getAllStores(request);
      console.log("📋 Got", response.pagedData?.length || 0, "stores from API");

      if (response.pagedData && response.pagedData.length > 0) {
        // Debug: Log first few stores to see the structure
        console.log(
          "🔍 Sample store data structure:",
          response.pagedData.slice(0, 3).map((s) => ({
            UID: s.UID,
            uid: s.uid,
            Code: s.Code,
            code: s.code,
            Name: s.Name || s.name,
          }))
        );

        // Check if store 1000 exists with different property names
        const debugSearch = response.pagedData.filter((s) => {
          const storeUID = s.UID || s.uid || s.Uid;
          const storeCode = s.Code || s.code;
          return (
            storeUID?.toString().includes("1000") ||
            storeCode?.toString().includes("1000")
          );
        });

        if (debugSearch.length > 0) {
          console.log('🔍 Found stores containing "1000":', debugSearch);
        }

        // Filter client-side to find the store with matching UID
        const store = response.pagedData.find((s) => {
          const storeUID = s.UID || s.uid || s.Uid;
          const storeCode = s.Code || s.code;
          return (
            storeUID === uid ||
            storeCode === uid ||
            storeUID === String(uid) ||
            storeCode === String(uid)
          );
        });

        if (store) {
          console.log(
            "✅ Found store:",
            store.Code || store.code,
            "-",
            store.Name || store.name
          );
          return store;
        } else {
          // If not found in first page, try getting more pages
          console.log(
            "🔄 Store not found in first page, trying larger page size..."
          );
          const largerRequest: StoreListRequest = {
            pageNumber: 1,
            pageSize: 200, // Get more stores
            filterCriterias: [],
            sortCriterias: [],
            isCountRequired: false,
          };

          const largerResponse = await this.getAllStores(largerRequest);
          console.log(
            "📋 Got",
            largerResponse.pagedData?.length || 0,
            "stores in expanded search"
          );

          // Debug: Log more sample data from larger set
          console.log(
            "🔍 Expanded search sample:",
            largerResponse.pagedData.slice(0, 5).map((s) => ({
              UID: s.UID,
              uid: s.uid,
              Code: s.Code,
              code: s.code,
            }))
          );

          // Check in expanded search for store 1000
          const debugExpandedSearch = largerResponse.pagedData.filter((s) => {
            const storeUID = s.UID || s.uid || s.Uid;
            const storeCode = s.Code || s.code;
            return (
              storeUID?.toString().includes("1000") ||
              storeCode?.toString().includes("1000")
            );
          });

          if (debugExpandedSearch.length > 0) {
            console.log(
              '🔍 Found stores containing "1000" in expanded search:',
              debugExpandedSearch
            );
          } else {
            console.log(
              '❌ No stores containing "1000" found in',
              largerResponse.pagedData.length,
              "stores"
            );
          }

          const storeFromLargerSet = largerResponse.pagedData.find((s) => {
            const storeUID = s.UID || s.uid || s.Uid;
            const storeCode = s.Code || s.code;
            return (
              storeUID === uid ||
              storeCode === uid ||
              storeUID === String(uid) ||
              storeCode === String(uid)
            );
          });

          if (storeFromLargerSet) {
            console.log(
              "✅ Found store in expanded search:",
              storeFromLargerSet.Code || storeFromLargerSet.code,
              "-",
              storeFromLargerSet.Name || storeFromLargerSet.name
            );
            return storeFromLargerSet;
          }
        }
      }

      throw new Error(`Store with UID ${uid} not found in any dataset`);
    } catch (error) {
      console.error("❌ Error fetching store:", error);
      throw error;
    }
  }

  /**
   * Create new store
   * Uses authenticated API service with Bearer token
   */
  async createStore(store: IStore): Promise<number> {
    try {
      // Generate UID if not provided
      if (!store.uid) {
        store.uid = this.generateUID();
      }

      // Set timestamps
      const now = new Date().toISOString();
      store.created_time = now;
      store.modified_time = now;
      store.server_add_time = now;
      store.server_modified_time = now;

      const response = await apiService.post("/Store/CreateStore", store);
      return response.data || response;
    } catch (error) {
      console.error("Error creating store:", error);
      throw error;
    }
  }

  /**
   * Update existing store
   * Uses authenticated API service with Bearer token
   */
  async updateStore(store: IStore): Promise<number> {
    try {
      // Update timestamps
      store.modified_time = new Date().toISOString();
      store.server_modified_time = new Date().toISOString();

      const response = await apiService.put("/Store/UpdateStore", store);
      return response.data || response;
    } catch (error) {
      console.error("Error updating store:", error);
      throw error;
    }
  }

  /**
   * Delete store by UID
   * Uses authenticated API service with Bearer token
   */
  async deleteStore(uid: string): Promise<number> {
    try {
      const response = await apiService.delete(`/Store/DeleteStore?UID=${uid}`);
      return response.data || response;
    } catch (error) {
      console.error("Error deleting store:", error);
      throw error;
    }
  }

  /**
   * Update store status (activate/deactivate/block/unblock)
   * Uses authenticated API service with Bearer token
   */
  async updateStoreStatus(
    uid: string,
    isActive?: boolean,
    isBlocked?: boolean
  ): Promise<number> {
    try {
      console.log(
        `🔄 Updating store ${uid} status: active=${isActive}, blocked=${isBlocked}`
      );

      // Get current user from token for audit fields
      const token = authService.getToken();
      const currentUser = getCurrentUser(token || "");

      // Try to get current store data, but don't fail if it doesn't work
      let currentStore: any = {};
      try {
        currentStore = await this.getStoreByUID(uid);
      } catch (error) {
        console.warn(
          "Could not fetch current store data, using minimal data:",
          error
        );
      }

      // Create minimal Store object with required fields
      const storeData = {
        UID: uid,
        IsActive:
          isActive !== undefined
            ? isActive
            : currentStore.IsActive ?? currentStore.is_active ?? true,
        IsBlocked:
          isBlocked !== undefined
            ? isBlocked
            : currentStore.IsBlocked ?? currentStore.is_blocked ?? false,
        ModifiedBy: currentUser || "ADMIN",
        ModifiedTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
        // Add other required fields from currentStore if available
        Name: currentStore.Name || currentStore.name || "",
        Type: currentStore.Type || currentStore.type || "FRC",
        Code: currentStore.Code || currentStore.code || uid,
      };

      // Create StoreApprovalDTO structure expected by backend
      const storeApprovalDTO = {
        Store: storeData,
        ApprovalRequestItem: null,
        ApprovalStatusUpdate: null,
      };

      console.log("📤 Sending UpdateStoreStatus request:", storeApprovalDTO);
      const response = await apiService.put(
        "/Store/UpdateStoreStatus",
        storeApprovalDTO
      );
      console.log("✅ Store status updated successfully:", response);
      return response.data || response;
    } catch (error) {
      console.error("❌ Error updating store status:", error);
      throw error;
    }
  }

  /**
   * Activate store
   */
  async activateStore(uid: string): Promise<number> {
    return this.updateStoreStatus(uid, true, false);
  }

  /**
   * Deactivate store
   */
  async deactivateStore(uid: string): Promise<number> {
    return this.updateStoreStatus(uid, false, false);
  }

  /**
   * Block store
   */
  async blockStore(uid: string): Promise<number> {
    return this.updateStoreStatus(uid, false, true);
  }

  /**
   * Unblock store
   */
  async unblockStore(uid: string): Promise<number> {
    return this.updateStoreStatus(uid, true, false);
  }

  /**
   * Create store with all related data (Master)
   * Uses authenticated API service with Bearer token
   */
  async createStoreMaster(storeMaster: IStoreMaster): Promise<number> {
    try {
      // Set timestamps for all entities
      const now = new Date().toISOString();

      // Store timestamps
      storeMaster.store.created_time = now;
      storeMaster.store.modified_time = now;
      storeMaster.store.server_add_time = now;
      storeMaster.store.server_modified_time = now;

      // Additional info timestamps
      if (storeMaster.storeAdditionalInfo) {
        storeMaster.storeAdditionalInfo.store_uid = storeMaster.store.uid;
      }

      // Set store_uid for all related entities
      if (storeMaster.contacts) {
        storeMaster.contacts.forEach((contact) => {
          contact.linked_item_uid = storeMaster.store.uid;
        });
      }

      if (storeMaster.addresses) {
        storeMaster.addresses.forEach((address) => {
          address.linked_item_uid = storeMaster.store.uid;
        });
      }

      if (storeMaster.storeCredits) {
        storeMaster.storeCredits.forEach((credit) => {
          credit.store_uid = storeMaster.store.uid;
        });
      }

      const response = await apiService.post(
        "/Store/CreateStoreMaster",
        storeMaster
      );
      return response.data || response;
    } catch (error) {
      console.error("Error creating store master:", error);
      throw error;
    }
  }

  /**
   * Get store master (complete data) by UID
   * Uses authenticated API service with Bearer token
   */
  async getStoreMasterByUID(uid: string): Promise<IStoreMaster> {
    try {
      console.log(`🏠 Fetching complete store master data for UID: ${uid}`);

      // Try to call the API directly first
      try {
        const directResponse = await fetch(
          `${this.getBaseUrl()}/Store/SelectStoreMasterByUID?UID=${uid}`,
          {
            method: "GET",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${authService.getToken()}`,
            },
          }
        );

        if (directResponse.ok) {
          const directData = await directResponse.json();
          console.log("✅ Direct API call succeeded:", directData);

          if (directData && directData.Data) {
            const storeMaster = directData.Data;
            if (storeMaster && storeMaster.store) {
              return {
                store: storeMaster.store || storeMaster.Store,
                contacts: storeMaster.Contacts || storeMaster.contacts || [],
                addresses: storeMaster.addresses || storeMaster.Addresses || [],
                storeCredits:
                  storeMaster.StoreCredits || storeMaster.storeCredits || [],
                storeAdditionalInfo:
                  storeMaster.StoreAdditionalInfo ||
                  storeMaster.storeAdditionalInfo,
              };
            }
          }
        }
      } catch (directError) {
        console.warn("Direct API call failed, using apiService:", directError);
      }

      // Fallback to using apiService
      const response = await apiService.get(
        `/Store/SelectStoreMasterByUID?UID=${uid}`
      );
      console.log(
        "🏠 StoreMaster API raw response:",
        JSON.stringify(response).substring(0, 500)
      );

      // Handle the response structure
      let storeMaster = null;

      // Check for error responses
      if (response && response.IsSuccess === false) {
        console.error(
          "❌ API returned error:",
          response.Message || response.ErrorMessage
        );
        throw new Error(
          response.Message || response.ErrorMessage || "API request failed"
        );
      }

      if (response && response.Data) {
        storeMaster = response.Data;
      } else if (response) {
        storeMaster = response;
      }

      if (storeMaster && storeMaster.store) {
        console.log("✅ StoreMaster retrieved successfully:", {
          storeUID: storeMaster.store?.UID,
          hasStore: !!storeMaster.store,
          contactsCount:
            storeMaster.Contacts?.length || storeMaster.contacts?.length || 0,
          addressesCount:
            storeMaster.addresses?.length || storeMaster.Addresses?.length || 0,
        });

        // Normalize the structure to match our interface - API uses mixed case
        return {
          store: storeMaster.store || storeMaster.Store,
          contacts: storeMaster.Contacts || storeMaster.contacts || [],
          addresses: storeMaster.addresses || storeMaster.Addresses || [],
          storeCredits:
            storeMaster.StoreCredits || storeMaster.storeCredits || [],
          storeAdditionalInfo:
            storeMaster.StoreAdditionalInfo || storeMaster.storeAdditionalInfo,
        };
      }

      console.error("❌ Invalid store master structure:", {
        hasStoreMaster: !!storeMaster,
        hasStore: !!storeMaster?.store,
        hasStoreCapital: !!storeMaster?.Store,
        keys: storeMaster ? Object.keys(storeMaster) : [],
      });
      throw new Error(`Invalid or missing store data for UID: ${uid}`);
    } catch (error) {
      console.error("❌ Error fetching store master:", error);

      // If the dedicated API fails, fall back to individual API calls
      console.log("🔄 Falling back to individual API calls...");
      const basicStore = await this.getStoreByUID(uid);
      const [contacts, addresses] = await Promise.all([
        this.getStoreContacts(uid).catch(() => []),
        this.getStoreAddresses(uid).catch(() => []),
      ]);

      return {
        store: basicStore,
        contacts: contacts,
        addresses: addresses,
        storeCredits: [],
        storeAdditionalInfo: null,
      };
    }
  }

  /**
   * Update store master
   * Uses authenticated API service with Bearer token
   */
  async updateStoreMaster(storeMaster: IStoreMaster): Promise<number> {
    try {
      // Update timestamps
      const now = new Date().toISOString();
      storeMaster.store.modified_time = now;
      storeMaster.store.server_modified_time = now;

      const response = await apiService.put(
        "/Store/UpdateStoreMaster",
        storeMaster
      );
      return response.data || response;
    } catch (error) {
      console.error("Error updating store master:", error);
      throw error;
    }
  }

  /**
   * Get stores by route UID
   * Uses authenticated API service with Bearer token
   */
  async getStoresByRouteUID(routeUID: string): Promise<IStoreCustomer[]> {
    try {
      const response = await apiService.get(
        `/Store/GetStoreCustomersByRouteUID?routeUID=${routeUID}`
      );
      return response.data || response;
    } catch (error) {
      console.error("Error fetching stores by route:", error);
      throw error;
    }
  }

  /**
   * Get all store items (for selection/dropdown)
   * Uses authenticated API service with Bearer token
   */
  async getAllStoreItems(
    request: StoreListRequest,
    orgUID?: string
  ): Promise<PagedResponse<IStoreCustomer>> {
    try {
      const url = orgUID
        ? `/Store/GetAllStoreItems?OrgUID=${orgUID}`
        : "/Store/GetAllStoreItems";
      const response = await apiService.post(url, request);
      return response.data || response;
    } catch (error) {
      console.error("Error fetching store items:", error);
      throw error;
    }
  }

  /**
   * Get stores as selection items
   * Uses authenticated API service with Bearer token
   */
  async getAllStoreAsSelectionItems(
    request: StoreListRequest,
    orgUID?: string
  ): Promise<any> {
    try {
      const url = orgUID
        ? `/Store/GetAllStoreAsSelectionItems?OrgUID=${orgUID}`
        : "/Store/GetAllStoreAsSelectionItems";
      const response = await apiService.post(url, request);
      return response.data || response;
    } catch (error) {
      console.error("Error fetching store selection items:", error);
      throw error;
    }
  }

  /**
   * Search stores
   */
  async searchStores(params: StoreSearchParams): Promise<IStore[]> {
    try {
      const request: StoreListRequest = {
        pageNumber: 1,
        pageSize: 100,
        filterCriterias: [],
        sortCriterias: [{ sortParameter: "Name", direction: 0 }],
      };

      // Build filter criteria (using camelCase for JSON)
      if (params.searchText) {
        request.filterCriterias?.push({
          name: "Name",
          value: params.searchText,
          operator: "contains",
        });
      }

      if (params.type) {
        request.filterCriterias?.push({
          name: "Type",
          value: params.type,
        });
      }

      if (params.classification) {
        request.filterCriterias?.push({
          name: "BroadClassification",
          value: params.classification,
        });
      }

      if (params.status) {
        request.filterCriterias?.push({
          name: "Status",
          value: params.status,
        });
      }

      if (params.isActive !== undefined) {
        request.filterCriterias?.push({
          name: "IsActive",
          value: params.isActive,
        });
      }

      const response = await this.getAllStores(request);
      return response.pagedData;
    } catch (error) {
      console.error("Error searching stores:", error);
      throw error;
    }
  }

  /**
   * Export stores to CSV
   */
  async exportStores(filters?: StoreSearchParams): Promise<Blob> {
    try {
      const stores = await this.searchStores(filters || {});

      // Convert to CSV
      const headers = [
        "Code",
        "Name",
        "Type",
        "Classification",
        "Status",
        "Active",
        "Country",
        "Region",
        "City",
      ];
      const rows = stores.map((store) => [
        store.code,
        store.name,
        store.type,
        store.broad_classification || "",
        store.status,
        store.is_active ? "Yes" : "No",
        store.country_uid || "",
        store.region_uid || "",
        store.city_uid || "",
      ]);

      const csv = [
        headers.join(","),
        ...rows.map((row) => row.map((cell) => `"${cell}"`).join(",")),
      ].join("\n");

      return new Blob([csv], { type: "text/csv" });
    } catch (error) {
      console.error("Error exporting stores:", error);
      throw error;
    }
  }

  /**
   * Import stores from CSV
   */
  async importStores(
    file: File
  ): Promise<{ success: number; failed: number; errors: string[] }> {
    try {
      const formData = new FormData();
      formData.append("file", file);

      const response = await apiService.post("/Store/ImportStores", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });

      return response.data.data || response.data;
    } catch (error) {
      console.error("Error importing stores:", error);
      throw error;
    }
  }

  /**
   * Validate store data
   */
  validateStore(store: Partial<IStore>): {
    isValid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];

    if (!store.code) errors.push("Store code is required");
    if (!store.name) errors.push("Store name is required");
    if (!store.type) errors.push("Store type is required");
    if (!store.status) errors.push("Store status is required");

    // Validate email format if provided
    if (store.email && !this.isValidEmail(store.email)) {
      errors.push("Invalid email format");
    }

    // Validate mobile format if provided
    if (store.mobile && !this.isValidMobile(store.mobile)) {
      errors.push("Invalid mobile number format");
    }

    return {
      isValid: errors.length === 0,
      errors,
    };
  }

  /**
   * Generate unique ID
   */
  private generateUID(): string {
    const timestamp = Date.now();
    const random = Math.random().toString(36).substring(2, 9);
    return `STR_${timestamp}_${random}`.toUpperCase();
  }

  /**
   * Validate email format
   */
  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  /**
   * Validate mobile format
   */
  private isValidMobile(mobile: string): boolean {
    const mobileRegex = /^[+]?[\d\s-()]+$/;
    return mobileRegex.test(mobile) && mobile.replace(/\D/g, "").length >= 10;
  }

  /**
   * Get contacts for a store by linked item UID
   */
  async getStoreContacts(storeUID: string): Promise<any[]> {
    try {
      console.log(`📞 Fetching contacts for store UID: ${storeUID}`);

      // Get all contacts first (backend filtering seems broken)
      const request = {
        pageNumber: 1,
        pageSize: 500, // Get more to ensure we have all data
        filterCriterias: [], // Empty filters - get all and filter client-side
        sortCriterias: [],
        isCountRequired: false,
      };

      console.log("🔍 Getting all contacts and filtering client-side...");
      const response = await apiService.post(
        "/Contact/SelectAllContactDetails",
        request
      );
      console.log("📞 Contact API raw response:", response);

      let allContacts = [];
      if (response && response.Data && response.Data.PagedData) {
        allContacts = response.Data.PagedData || [];
      } else if (response && response.PagedData) {
        allContacts = response.PagedData || [];
      }

      console.log(`📊 Total contacts retrieved: ${allContacts.length}`);

      // COMPREHENSIVE CLIENT-SIDE FILTERING
      console.log(`\n🔍 ANALYZING CONTACT DATA FOR STORE ${storeUID}`);
      console.log(`Total contacts in database: ${allContacts.length}`);

      // Try multiple matching strategies
      const filterStrategies = [
        // Strategy 1: Exact LinkedItemUID match
        (contact) => {
          const linkedUID = contact.LinkedItemUID || contact.linked_item_uid;
          return linkedUID === storeUID;
        },
        // Strategy 2: Try UID field (some contacts have wrong LinkedItemUID)
        (contact) => {
          return contact.UID === storeUID;
        },
        // Strategy 3: Try partial matches (in case of data inconsistency)
        (contact) => {
          const linkedUID =
            contact.LinkedItemUID || contact.linked_item_uid || "";
          return linkedUID.includes(storeUID) || storeUID.includes(linkedUID);
        },
      ];

      let storeContacts = [];
      let usedStrategy = "";

      for (let i = 0; i < filterStrategies.length; i++) {
        const strategy = filterStrategies[i];
        const matches = allContacts.filter(strategy);

        if (matches.length > 0) {
          storeContacts = matches;
          usedStrategy = `Strategy ${i + 1}`;
          console.log(
            `✅ Found ${matches.length} contacts using ${usedStrategy}`
          );
          break;
        }
      }

      // Debug: Show sample of ALL contacts to understand data structure
      console.log("\n--- CONTACT DATA ANALYSIS ---");
      const sampleContacts = allContacts.slice(0, 10);
      sampleContacts.forEach((contact, index) => {
        console.log(`Contact ${index + 1}:`, {
          UID: contact.UID,
          LinkedItemUID: contact.LinkedItemUID,
          Name: contact.Name || contact.name || "NO_NAME",
          Mobile: contact.Mobile || contact.mobile || "NO_MOBILE",
          Email: contact.Email || contact.email || "NO_EMAIL",
          HasRealData: !!(contact.Name || contact.Mobile || contact.Email),
        });
      });

      if (storeContacts.length === 0) {
        console.log(
          `❌ NO CONTACTS found for store ${storeUID} using any strategy`
        );
        console.log(
          "Available LinkedItemUIDs in first 20 contacts:",
          allContacts
            .slice(0, 20)
            .map((c) => c.LinkedItemUID || c.linked_item_uid)
            .filter(Boolean)
        );
      }

      console.log("=== END CONTACT ANALYSIS ===\n");
      return storeContacts;
    } catch (error) {
      console.error("❌ Error fetching contacts:", error);
      return [];
    }
  }

  /**
   * Get addresses for a store by linked item UID
   */
  async getStoreAddresses(storeUID: string): Promise<any[]> {
    try {
      console.log(`📍 Fetching addresses for store UID: ${storeUID}`);

      // Get all addresses first (backend filtering seems broken)
      const request = {
        pageNumber: 1,
        pageSize: 500, // Get more to ensure we have all data
        filterCriterias: [], // Empty filters - get all and filter client-side
        sortCriterias: [],
        isCountRequired: false,
      };

      console.log("🔍 Getting all addresses and filtering client-side...");
      const response = await apiService.post(
        "/Address/SelectAllAddressDetails",
        request
      );
      console.log("📍 Address API raw response:", response);

      let allAddresses = [];
      if (response && response.Data && response.Data.PagedData) {
        allAddresses = response.Data.PagedData || [];
      } else if (response && response.PagedData) {
        allAddresses = response.PagedData || [];
      }

      console.log(`📊 Total addresses retrieved: ${allAddresses.length}`);

      // COMPREHENSIVE CLIENT-SIDE FILTERING FOR ADDRESSES
      console.log(`\n🔍 ANALYZING ADDRESS DATA FOR STORE ${storeUID}`);
      console.log(`Total addresses in database: ${allAddresses.length}`);

      // Try multiple matching strategies
      const addressFilterStrategies = [
        // Strategy 1: Exact LinkedItemUID match
        (address) => {
          const linkedUID = address.LinkedItemUID || address.linked_item_uid;
          return linkedUID === storeUID;
        },
        // Strategy 2: Check if UID contains store UID (format like "Billing_S1518")
        (address) => {
          const uid = address.UID || "";
          return uid.includes(storeUID);
        },
        // Strategy 3: Try partial matches
        (address) => {
          const linkedUID =
            address.LinkedItemUID || address.linked_item_uid || "";
          return linkedUID.includes(storeUID) || storeUID.includes(linkedUID);
        },
      ];

      let storeAddresses = [];
      let usedAddressStrategy = "";

      for (let i = 0; i < addressFilterStrategies.length; i++) {
        const strategy = addressFilterStrategies[i];
        const matches = allAddresses.filter(strategy);

        if (matches.length > 0) {
          storeAddresses = matches;
          usedAddressStrategy = `Strategy ${i + 1}`;
          console.log(
            `✅ Found ${matches.length} addresses using ${usedAddressStrategy}`
          );
          break;
        }
      }

      // Debug: Show sample of ALL addresses to understand data structure
      console.log("\n--- ADDRESS DATA ANALYSIS ---");
      const sampleAddresses = allAddresses.slice(0, 10);
      sampleAddresses.forEach((address, index) => {
        console.log(`Address ${index + 1}:`, {
          UID: address.UID,
          LinkedItemUID: address.LinkedItemUID,
          Type: address.Type || "NO_TYPE",
          Name: address.Name || "NO_NAME",
          Line1: address.Line1 || "NO_ADDRESS",
          City: address.City || "NO_CITY",
          HasRealData: !!(address.Line1 || address.City || address.Name),
        });
      });

      if (storeAddresses.length === 0) {
        console.log(
          `❌ NO ADDRESSES found for store ${storeUID} using any strategy`
        );
        console.log(
          "Available LinkedItemUIDs in first 20 addresses:",
          allAddresses
            .slice(0, 20)
            .map((a) => a.LinkedItemUID || a.linked_item_uid)
            .filter(Boolean)
        );
        console.log(
          "Available UIDs in first 20 addresses:",
          allAddresses
            .slice(0, 20)
            .map((a) => a.UID)
            .filter(Boolean)
        );
      }

      console.log("=== END ADDRESS ANALYSIS ===\n");
      return storeAddresses;
    } catch (error) {
      console.error("❌ Error fetching addresses:", error);
      return [];
    }
  }

  /**
   * Get store statistics
   */
  async getStoreStatistics(): Promise<{
    totalStores: number;
    activeStores: number;
    pendingStores: number;
    blockedStores: number;
    byType: Record<string, number>;
    byClassification: Record<string, number>;
  }> {
    try {
      // This would normally be a separate API endpoint
      // For now, we'll fetch all stores and calculate
      const request: StoreListRequest = {
        pageNumber: 1,
        pageSize: 10000,
        isCountRequired: true,
      };

      const response = await this.getAllStores(request);
      const stores = response.pagedData;

      const stats = {
        totalStores: response.totalCount,
        activeStores: stores.filter((s) => s.is_active).length,
        pendingStores: stores.filter((s) => s.status === "Pending").length,
        blockedStores: stores.filter((s) => s.is_blocked).length,
        byType: {} as Record<string, number>,
        byClassification: {} as Record<string, number>,
      };

      // Count by type
      stores.forEach((store) => {
        if (store.type) {
          stats.byType[store.type] = (stats.byType[store.type] || 0) + 1;
        }
        if (store.broad_classification) {
          stats.byClassification[store.broad_classification] =
            (stats.byClassification[store.broad_classification] || 0) + 1;
        }
      });

      return stats;
    } catch (error) {
      console.error("Error fetching store statistics:", error);
      throw error;
    }
  }
}

// Export singleton instance
export const storeService = new StoreService();

// Export class for testing
export default StoreService;
