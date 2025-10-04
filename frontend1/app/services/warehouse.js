import { apiRequest, buildPagingRequest } from '../../lib/api-config';

class WarehouseService {
  /**
   * Get authentication token dynamically
   */
  getAuthToken() {
    // First try to get from localStorage directly
    const authToken = typeof window !== 'undefined' ? localStorage.getItem('authToken') : null;

    if (authToken && authToken !== 'null' && authToken !== 'undefined') {
      console.log('🔑 Using authToken from localStorage');
      return authToken;
    }

    // Fallback to get from currentUser session
    const currentUser = typeof window !== 'undefined' ? localStorage.getItem('currentUser') : null;

    if (currentUser) {
      try {
        const user = JSON.parse(currentUser);
        if (user.token && user.token !== 'null' && user.token !== 'undefined') {
          console.log('🔑 Using token from currentUser session');
          return user.token;
        }
      } catch (e) {
        console.error('Error parsing currentUser:', e);
      }
    }

    console.warn('⚠️ No valid auth token found');
    return null;
  }

  /**
   * Get All Stores/Warehouses
   * Endpoint: POST /api/Store/SelectAllStore
   */
  async getAllStores(filters = {}) {
    try {
      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        throw new Error('Authentication required. Please login.');
      }

      console.log('🔑 Using token for store API call:', token.substring(0, 30) + '...');

      const requestBody = buildPagingRequest(
        filters.page || 1,
        filters.pageSize || 10, // Reduced page size for better performance
        filters.searchQuery,
        {
          // Add any specific filters if needed
          orgUID: filters.orgUID
        },
        filters.sortField,
        filters.sortDirection
      );

      console.log('📤 Store Request body:', requestBody);

      const response = await apiRequest(
        '/Store/SelectAllStore',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        console.log('🏢 Full Store API Response:', response);

        // Try multiple possible response structures
        const stores = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];
        const totalCount = response.data?.Data?.TotalCount || response.data?.TotalCount || response.data?.totalCount || 0;

        console.log('🏢 Extracted store data:', stores);
        console.log('🏢 Store data type:', typeof stores);
        console.log('🏢 Is store data array:', Array.isArray(stores));

        // Log first store item structure if available
        if (Array.isArray(stores) && stores.length > 0) {
          console.log('🏢 First store item structure:', stores[0]);
          console.log('🏢 First store item keys:', Object.keys(stores[0]));
        }

        // Normalize store data and create a map for quick lookup
        const warehouseMap = {};
        let normalizedStores = [];

        if (Array.isArray(stores)) {
          normalizedStores = stores.map((store, index) => {
            if (store) {
              console.log(`🏢 Processing store ${index + 1}:`, store);

              // Try multiple possible UID field names
              const storeUID = store.uid || store.UID || store.id || store.ID || store.code || store.Code || store.number || store.Number;

              // Try multiple possible name field names
              const storeName = store.name || store.Name || store.warehouseName || store.WarehouseName ||
                               store.displayName || store.DisplayName || store.text || store.Text ||
                               store.label || store.Label || store.alias_name || store.aliasName;

              // Store type and other details
              const storeType = store.type || store.Type || store.store_class || store.storeClass || 'Store';
              const storeCode = store.code || store.Code || store.number || store.Number || storeUID;

              const normalizedStore = {
                uid: storeUID,
                code: storeCode,
                name: storeName,
                type: storeType,
                label: `${storeCode} - ${storeName}`,
                value: storeUID,

                // Additional fields that might be useful
                status: store.status || store.Status,
                isActive: store.is_active || store.isActive,
                location: store.location,
                address: store.address,

                // Keep original object for reference
                ...store
              };

              if (storeUID && storeName) {
                warehouseMap[storeUID] = storeName;
                console.log(`🏢 Mapped: ${storeUID} -> ${storeName}`);
                return normalizedStore;
              } else {
                console.warn('🔥 Missing UID or name for store:', store);
                return store; // Return original if we can't normalize
              }
            }
            return store;
          }).filter(store => store); // Remove any null/undefined stores
        }

        return {
          success: true,
          data: response.data,
          stores: normalizedStores,
          warehouses: normalizedStores, // Alias for backward compatibility
          warehouseMap: warehouseMap,
          totalCount: totalCount || 0
        };
      } else {
        throw new Error(response.error || 'Failed to fetch store data');
      }
    } catch (error) {
      console.error('Error fetching store data:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch store data',
        stores: [],
        warehouses: [],
        warehouseMap: {},
        totalCount: 0
      };
    }
  }

  /**
   * Get All Store Items (Products available at stores)
   * Endpoint: POST /api/Store/GetAllStoreItems
   */
  async getAllStoreItems(filters = {}) {
    try {
      const token = this.getAuthToken();

      if (!token) {
        throw new Error('Authentication required. Please login.');
      }

      const requestBody = buildPagingRequest(
        filters.page || 1,
        filters.pageSize || 100,
        filters.searchQuery,
        {
          orgUID: filters.orgUID
        },
        filters.sortField,
        filters.sortDirection
      );

      const response = await apiRequest(
        '/Store/GetAllStoreItems',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        const storeItems = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];
        const totalCount = response.data?.Data?.TotalCount || response.data?.TotalCount || response.data?.totalCount || 0;

        return {
          success: true,
          data: response.data,
          storeItems: Array.isArray(storeItems) ? storeItems : [],
          totalCount: totalCount || 0
        };
      } else {
        throw new Error(response.error || 'Failed to fetch store items');
      }
    } catch (error) {
      console.error('Error fetching store items:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch store items',
        storeItems: [],
        totalCount: 0
      };
    }
  }

  /**
   * Get Store by UID
   * Endpoint: GET /api/Store/SelectStoreByUID
   */
  async getStoreByUID(uid) {
    try {
      const token = this.getAuthToken();

      if (!uid) {
        throw new Error('Store UID is required');
      }

      const response = await apiRequest(
        `/Store/SelectStoreByUID?uid=${encodeURIComponent(uid)}`,
        {
          method: 'GET'
        },
        token
      );

      if (response.success) {
        const storeData = response.data?.Data || response.data;
        return {
          success: true,
          data: storeData,
          store: storeData
        };
      } else {
        throw new Error(response.error || 'Failed to fetch store details');
      }
    } catch (error) {
      console.error('Error fetching store by UID:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch store details',
        store: null
      };
    }
  }

  /**
   * Get Stores by Organization UID
   * Endpoint: POST /api/Store/SelectStoreByOrgUID
   */
  async getStoresByOrgUID(orgUID) {
    try {
      const token = this.getAuthToken();

      if (!orgUID) {
        throw new Error('Organization UID is required');
      }

      const requestBody = {
        pageNumber: 1,
        pageSize: 100,
        filterCriterias: [
          {
            fieldName: 'orgUID',
            operator: 'EQ',
            value: orgUID
          }
        ],
        isCountRequired: true
      };

      const response = await apiRequest(
        '/Store/SelectStoreByOrgUID',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        const stores = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];
        return {
          success: true,
          data: response.data,
          stores: Array.isArray(stores) ? stores : []
        };
      } else {
        throw new Error(response.error || 'Failed to fetch stores by organization');
      }
    } catch (error) {
      console.error('Error fetching stores by organization:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch stores by organization',
        stores: []
      };
    }
  }

  /**
   * Get warehouse/store dropdown options
   * This is a helper method that formats stores for dropdown use
   */
  async getWarehouseDropdownOptions(filters = {}) {
    const response = await this.getAllStores(filters);

    if (response.success) {
      const options = response.stores.map(store => ({
        value: store.uid || store.code,
        label: store.label || `${store.code} - ${store.name}`,
        uid: store.uid,
        code: store.code,
        name: store.name,
        type: store.type
      }));

      return {
        success: true,
        options: options,
        warehouses: response.stores,
        warehouseMap: response.warehouseMap
      };
    }

    return response;
  }

  /**
   * Legacy method for backward compatibility
   * @deprecated Use getAllStores() instead
   */
  async getWarehouseTypeDropdown() {
    console.warn('⚠️ getWarehouseTypeDropdown() is deprecated. Use getAllStores() instead.');
    return await this.getAllStores();
  }

  /**
   * Get warehouse name by UID
   */
  async getWarehouseNameByUID(warehouseUID) {
    try {
      if (!warehouseUID) {
        return 'N/A';
      }

      const response = await this.getAllStores();
      if (response.success && response.warehouseMap) {
        return response.warehouseMap[warehouseUID] || 'N/A';
      }

      return 'N/A';
    } catch (error) {
      console.error('Error getting warehouse name:', error);
      return 'N/A';
    }
  }

  /**
   * Cache warehouse data for better performance
   */
  warehouseCache = null;
  cacheTimestamp = null;
  cacheExpiry = 5 * 60 * 1000; // 5 minutes

  async getCachedWarehouseData() {
    const now = Date.now();

    if (this.warehouseCache && this.cacheTimestamp && (now - this.cacheTimestamp) < this.cacheExpiry) {
      console.log('📦 Using cached warehouse data');
      return this.warehouseCache;
    }

    console.log('🔄 Fetching fresh warehouse data');
    const response = await this.getAllStores();

    if (response.success) {
      this.warehouseCache = response;
      this.cacheTimestamp = now;
    }

    return response;
  }

  /**
   * Clear warehouse cache
   */
  clearCache() {
    this.warehouseCache = null;
    this.cacheTimestamp = null;
  }
}

export default new WarehouseService();