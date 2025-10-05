/**
 * Centralized API Configuration
 */

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000/api';

/**
 * Get authentication token from localStorage
 */
const getAuthToken = () => {
  if (typeof window === 'undefined') return null;

  // Try auth_token first (used by product management)
  const authToken = localStorage.getItem('auth_token');
  if (authToken && authToken !== 'null' && authToken !== 'undefined') {
    return authToken;
  }

  // Try authToken as fallback
  const authTokenAlt = localStorage.getItem('authToken');
  if (authTokenAlt && authTokenAlt !== 'null' && authTokenAlt !== 'undefined') {
    return authTokenAlt;
  }

  // Try currentUser as final fallback
  const currentUser = localStorage.getItem('currentUser');
  if (currentUser) {
    try {
      const user = JSON.parse(currentUser);
      if (user.token && user.token !== 'null' && user.token !== 'undefined') {
        return user.token;
      }
    } catch (e) {
      console.error('Error parsing currentUser:', e);
    }
  }

  return null;
};

/**
 * Get common headers for API requests
 */
export const getCommonHeaders = () => {
  const token = getAuthToken();

  return {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` })
  };
};

/**
 * Make an API request
 */
export const apiRequest = async (endpoint, options = {}) => {
  const url = `${API_BASE_URL}${endpoint}`;

  const config = {
    ...options,
    headers: {
      ...getCommonHeaders(),
      ...options.headers
    }
  };

  try {
    const response = await fetch(url, config);

    if (!response.ok) {
      const errorText = await response.text();
      console.error('API request failed:', response.status, errorText);
      return {
        success: false,
        error: `HTTP error! status: ${response.status}`,
        data: null
      };
    }

    const data = await response.json();

    // Check if the response has IsSuccess property (backend standard)
    if (data.IsSuccess !== undefined) {
      return {
        success: data.IsSuccess,
        data: data,
        error: data.IsSuccess ? null : (data.Message || 'Request failed')
      };
    }

    // Otherwise return the data wrapped in a success response
    return {
      success: true,
      data: data,
      error: null
    };
  } catch (error) {
    console.error('API request failed:', error);
    return {
      success: false,
      error: error.message || 'Request failed',
      data: null
    };
  }
};

/**
 * Build paging request parameters
 */
export const buildPagingRequest = (page = 1, pageSize = 10, filters = {}) => {
  return {
    PageNumber: page,
    PageSize: pageSize,
    ...filters
  };
};

/**
 * Extract paged data from API response
 */
export const extractPagedData = (data) => {
  if (!data) return { items: [], totalCount: 0 };

  if (data.Data) {
    return {
      items: data.Data,
      totalCount: data.TotalRecords || data.TotalCount || data.Data.length
    };
  }

  if (Array.isArray(data)) {
    return {
      items: data,
      totalCount: data.length
    };
  }

  if (data.items) {
    return {
      items: data.items,
      totalCount: data.totalCount || data.items.length
    };
  }

  return { items: [], totalCount: 0 };
};
