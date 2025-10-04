import { apiRequest, buildPagingRequest } from '../../lib/api-config';

class OrganizationService {
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
   * Get available organizations from product data
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

      console.log('🏢 Fetching organizations from SKU data...');

      // Get a sample of SKU data to extract unique organizations
      const response = await apiRequest(
        '/SKU/SelectAllSKUDetailsWebView',
        {
          method: 'POST',
          body: JSON.stringify({
            pageNumber: 1,
            pageSize: 100, // Get more to find unique orgs
            filterCriterias: [],
            isCountRequired: false
          })
        },
        token
      );

      if (response.success) {
        const products = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];

        // Extract unique organizations
        const orgSet = new Set();
        const organizations = [];

        products.forEach(product => {
          const orgUID = product.OrgUID || product.orgUID;
          const divisionUID = product.DivisionUID || product.divisionUID;

          if (orgUID && !orgSet.has(orgUID)) {
            orgSet.add(orgUID);
            organizations.push({
              uid: orgUID,
              name: orgUID.replace(/_/g, ' ').toUpperCase(),
              type: 'Organization'
            });
          }

          if (divisionUID && divisionUID !== orgUID && !orgSet.has(divisionUID)) {
            orgSet.add(divisionUID);
            organizations.push({
              uid: divisionUID,
              name: divisionUID.replace(/_/g, ' ').toUpperCase(),
              type: 'Division'
            });
          }
        });

        // Add hardcoded organizations if none found
        if (organizations.length === 0) {
          organizations.push(
            { uid: 'EPIC01', name: 'EPIC 01', type: 'Organization' },
            { uid: 'Farmley', name: 'Farmley', type: 'Organization' },
            { uid: 'Supplier', name: 'Supplier', type: 'Division' }
          );
        }

        console.log('🏢 Found organizations:', organizations);

        return {
          success: true,
          organizations: organizations.sort((a, b) => a.name.localeCompare(b.name))
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