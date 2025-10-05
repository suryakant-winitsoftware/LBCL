import { apiRequest, buildPagingRequest } from '../lib/api-config';

class OrganizationService {
  /**
   * Get authentication token dynamically
   */
  getAuthToken() {
    // First try auth_token (used by product management)
    const authToken = typeof window !== 'undefined' ? localStorage.getItem('auth_token') : null;

    if (authToken && authToken !== 'null' && authToken !== 'undefined') {
      console.log('🔑 Using auth_token from localStorage');
      return authToken;
    }

    // Try authToken as fallback
    const authTokenAlt = typeof window !== 'undefined' ? localStorage.getItem('authToken') : null;
    if (authTokenAlt && authTokenAlt !== 'null' && authTokenAlt !== 'undefined') {
      console.log('🔑 Using authToken from localStorage');
      return authTokenAlt;
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
   * Get available organizations/stores from Store API
   */
  async getOrganizations() {
    try {
      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        return {
          success: false,
          error: 'Authentication required. Please login.',
          organizations: []
        };
      }

      console.log('🏢 Fetching organizations from Store API...');

      // Use the proper Store API endpoint
      const response = await apiRequest(
        '/Store/SelectAllStore',
        {
          method: 'POST',
          body: JSON.stringify({
            pageNumber: 1,
            pageSize: 100,
            sortCriterias: [],
            filterCriterias: [],
            isCountRequired: false
          })
        }
      );

      if (response.success) {
        const stores = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];

        console.log('🏢 Raw store data:', stores);

        // Extract unique organizations from stores
        const orgSet = new Set();
        const organizations = [];

        stores.forEach(store => {
          const storeUID = store.UID || store.uid;
          const storeName = store.Name || store.name || store.StoreName || store.storeName;
          const storeCode = store.Code || store.code || store.StoreCode || store.storeCode;

          if (storeUID && !orgSet.has(storeUID)) {
            orgSet.add(storeUID);
            organizations.push({
              uid: storeUID,
              name: storeName || storeCode || storeUID,
              code: storeCode || storeUID,
              type: 'Store'
            });
          }
        });

        // Sort by name
        organizations.sort((a, b) => a.name.localeCompare(b.name));

        console.log('🏢 Found organizations/stores:', organizations);

        return {
          success: true,
          organizations: organizations
        };
      } else {
        throw new Error(response.error || 'Failed to fetch organization data');
      }
    } catch (error) {
      console.error('Error fetching organizations:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch organizations',
        organizations: [
          { uid: 'EPIC01', name: 'EPIC 01', type: 'Organization' },
          { uid: 'Farmley', name: 'Farmley', type: 'Organization' },
          { uid: 'Supplier', name: 'Supplier', type: 'Division' }
        ]
      };
    }
  }

  /**
   * Get organization dropdown options
   */
  async getOrganizationDropdownOptions() {
    const response = await this.getOrganizations();

    if (response.success) {
      const options = [
        { value: '', label: 'All Plants/Organizations', uid: '', name: 'All', type: 'All' },
        ...response.organizations.map(org => ({
          value: org.uid,
          label: `${org.name} (${org.type})`,
          uid: org.uid,
          name: org.name,
          type: org.type
        }))
      ];

      return {
        success: true,
        options: options,
        organizations: response.organizations
      };
    }

    return response;
  }
}

export default new OrganizationService();