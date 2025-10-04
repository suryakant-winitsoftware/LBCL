import { apiRequest, buildPagingRequest } from '../../lib/api-config';

class SKUService {
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
   * Get All SKU Details (WebView optimized)
   * Endpoint: POST /api/SKU/SelectAllSKUDetailsWebView
   */
  async getAllSKUDetails(filters = {}) {
    try {
      console.log('🚀 getAllSKUDetails called with filters:', filters);

      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        console.log('📱 localStorage content:', {
          authToken: localStorage.getItem('authToken'),
          currentUser: localStorage.getItem('currentUser')
        });
        throw new Error('Authentication required. Please login.');
      }

      console.log('🔑 Using token for SKU API call:', token.substring(0, 30) + '...');

      // Clean filters to avoid circular references
      const cleanFilters = {};
      if (filters.orgUID) {
        cleanFilters.OrgUID = filters.orgUID;
      }
      // Also handle if OrgUID is passed directly in filters
      if (filters.OrgUID) {
        cleanFilters.OrgUID = filters.OrgUID;
      }

      console.log('🧹 Clean filters:', cleanFilters);

      const requestBody = buildPagingRequest(
        filters.page || 1,
        filters.pageSize || 10, // Reduced default page size for better performance
        filters.searchQuery,
        cleanFilters,
        filters.sortField,
        filters.sortDirection
      );

      console.log('📤 SKU Request body:', JSON.stringify(requestBody, null, 2));

      const response = await apiRequest(
        '/SKU/SelectAllSKUDetailsWebView',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        let products = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];
        const totalCount = response.data?.Data?.TotalCount || response.data?.TotalCount || response.data?.totalCount || 0;

        // Debug: Log the first product to see what fields are available
        if (products && products.length > 0) {
          console.log('📦 SKU Sample:', products[0]);
          console.log('📦 SKU fields available:', Object.keys(products[0]));
        }

        // Normalize product data structure for consistent use
        if (Array.isArray(products)) {
          products = products.map(product => {
            if (product) {
              return {
                // Core identifiers
                id: product.id || product.uid || product.UID || product.SKUUID,
                uid: product.uid || product.UID || product.SKUUID || product.id,
                skuCode: product.skuCode || product.SKUCode || product.code || product.Code,
                skuName: product.skuName || product.SKUName || product.SKULongName || product.name || product.Name,

                // Organization/Plant information
                orgUID: product.OrgUID || product.orgUID || product.OrganizationUID,
                organizationUID: product.OrgUID || product.orgUID || product.OrganizationUID,
                divisionUID: product.DivisionUID || product.divisionUID,

                // Product details
                skuType: product.skuType || product.SKUType || product.type,
                uom: product.uom || product.UOM || product.BaseUOM || 'OU',
                baseUOM: product.baseUOM || product.BaseUOM || product.uom || 'OU',

                // Inventory data
                availQty: product.availQty || product.AvailQty || product.availableQty || 0,
                modelQty: product.modelQty || product.ModelQty || 0,
                inTransit: product.inTransit || product.InTransit || product.inTransitQty || 0,

                // Pricing - check all possible field names
                unitPrice: product.unitPrice || product.UnitPrice || product.price || product.Price ||
                          product.basePrice || product.BasePrice || product.sellingPrice || product.SellingPrice ||
                          product.salePrice || product.SalePrice || product.dp || product.DP || 0,
                basePrice: product.basePrice || product.BasePrice || product.unitPrice || product.UnitPrice || 0,
                mrp: product.mrp || product.MRP || 0,
                dpPrice: product.dpPrice || product.DPPrice || product.dp || product.DP || 0,
                sellingPrice: product.sellingPrice || product.SellingPrice || product.unitPrice || product.UnitPrice || 0,

                // Order quantities
                suggestedOrderQty: product.suggestedOrderQty || product.SuggestedOrderQty || 0,
                past3MonthAverage: product.past3MonthAverage || product.Past3MonthAverage || 0,
                orderQty: 0, // Initialize as 0 for order entry

                // Additional fields
                eaPerCase: product.eaPerCase || product.EaPerCase || 1,
                booked: product.booked || product.Booked || 0,

                // Keep original object for reference
                ...product
              };
            }
            return product;
          });
        }

        return {
          success: true,
          data: response.data,
          products: Array.isArray(products) ? products : [],
          totalCount: totalCount || 0
        };
      } else {
        throw new Error(response.error || 'Failed to fetch SKU details');
      }
    } catch (error) {
      console.error('Error fetching SKU details:', error);
      console.error('Error type:', typeof error);
      console.error('Error name:', error.name);
      console.error('Error stack:', error.stack);

      let errorMessage = 'Failed to fetch SKU details';
      if (error.message && error.message.includes('circular')) {
        errorMessage = 'Data formatting error. Please try again.';
      } else if (error.message) {
        errorMessage = error.message;
      }

      return {
        success: false,
        error: errorMessage,
        products: [],
        totalCount: 0
      };
    }
  }

  /**
   * Get SKU Master Data
   * Endpoint: POST /api/SKU/GetAllSKUMasterData
   */
  async getAllSKUMasterData(filters = {}) {
    try {
      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        throw new Error('Authentication required. Please login.');
      }

      const requestBody = buildPagingRequest(
        filters.page || 1,
        filters.pageSize || 100,
        filters.searchQuery,
        {},
        filters.sortField,
        filters.sortDirection
      );

      const response = await apiRequest(
        '/SKU/GetAllSKUMasterData',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        const masterData = response.data?.Data || response.data || {};
        return {
          success: true,
          data: masterData
        };
      } else {
        throw new Error(response.error || 'Failed to fetch SKU master data');
      }
    } catch (error) {
      console.error('Error fetching SKU master data:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch SKU master data'
      };
    }
  }

  /**
   * Get SKU by UID
   * Endpoint: GET /api/SKU/SelectSKUByUID
   */
  async getSKUByUID(uid) {
    try {
      const token = this.getAuthToken();

      if (!uid) {
        throw new Error('SKU UID is required');
      }

      const response = await apiRequest(
        `/SKU/SelectSKUByUID?uid=${encodeURIComponent(uid)}`,
        {
          method: 'GET'
        },
        token
      );

      if (response.success) {
        const skuData = response.data?.Data || response.data;
        return {
          success: true,
          data: skuData,
          product: skuData
        };
      } else {
        throw new Error(response.error || 'Failed to fetch SKU details');
      }
    } catch (error) {
      console.error('Error fetching SKU by UID:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch SKU details',
        product: null
      };
    }
  }

  /**
   * Get SKU Price Details
   * Endpoint: POST /api/SKUPrice/SelectAllSKUPriceDetails
   */
  async getSKUPriceDetails(filters = {}) {
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
          storeUID: filters.storeUID,
          orgUID: filters.orgUID
        },
        filters.sortField,
        filters.sortDirection
      );

      const response = await apiRequest(
        '/SKUPrice/SelectAllSKUPriceDetails',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        const prices = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];
        const totalCount = response.data?.Data?.TotalCount || response.data?.TotalCount || response.data?.totalCount || 0;

        return {
          success: true,
          data: response.data,
          prices: Array.isArray(prices) ? prices : [],
          totalCount: totalCount || 0
        };
      } else {
        throw new Error(response.error || 'Failed to fetch SKU price details');
      }
    } catch (error) {
      console.error('Error fetching SKU price details:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch SKU price details',
        prices: [],
        totalCount: 0
      };
    }
  }

  /**
   * Get Applicable Price List by Store UID
   * Endpoint: GET /api/SKUPrice/GetApplicablePriceListByStoreUID
   */
  async getApplicablePriceListByStoreUID(storeUID) {
    try {
      const token = this.getAuthToken();

      if (!storeUID) {
        throw new Error('Store UID is required');
      }

      const response = await apiRequest(
        `/SKUPrice/GetApplicablePriceListByStoreUID?storeUID=${encodeURIComponent(storeUID)}`,
        {
          method: 'GET'
        },
        token
      );

      if (response.success) {
        return {
          success: true,
          data: response.data,
          priceList: response.data
        };
      } else {
        throw new Error(response.error || 'Failed to fetch applicable price list');
      }
    } catch (error) {
      console.error('Error fetching applicable price list:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch applicable price list',
        priceList: null
      };
    }
  }

  /**
   * Search SKUs with filters
   */
  async searchSKUs(searchQuery, filters = {}) {
    const searchFilters = {
      ...filters,
      searchQuery: searchQuery
    };
    return await this.getAllSKUDetails(searchFilters);
  }

  /**
   * Get products for a specific store/warehouse
   */
  async getProductsForStore(storeUID, filters = {}) {
    // First get SKUs, then get store-specific pricing if available
    const skuResponse = await this.getAllSKUDetails(filters);

    if (skuResponse.success && storeUID) {
      // Try to get store-specific pricing
      const priceResponse = await this.getApplicablePriceListByStoreUID(storeUID);

      if (priceResponse.success && priceResponse.priceList) {
        // Merge pricing information if available
        // This would depend on the actual structure of the price list response
        console.log('📊 Store-specific pricing available:', priceResponse.priceList);
      }
    }

    return skuResponse;
  }

  /**
   * Cache SKU data for better performance
   */
  skuCache = null;
  cacheTimestamp = null;
  cacheExpiry = 5 * 60 * 1000; // 5 minutes

  async getCachedSKUData(filters = {}) {
    const now = Date.now();
    const cacheKey = JSON.stringify(filters);

    if (this.skuCache && this.skuCache[cacheKey] && this.cacheTimestamp && (now - this.cacheTimestamp) < this.cacheExpiry) {
      console.log('📦 Using cached SKU data');
      return this.skuCache[cacheKey];
    }

    console.log('🔄 Fetching fresh SKU data');
    const response = await this.getAllSKUDetails(filters);

    if (response.success) {
      if (!this.skuCache) this.skuCache = {};
      this.skuCache[cacheKey] = response;
      this.cacheTimestamp = now;
    }

    return response;
  }

  /**
   * Clear SKU cache
   */
  clearCache() {
    this.skuCache = null;
    this.cacheTimestamp = null;
  }
}

export default new SKUService();