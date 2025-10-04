/**
 * Centralized API Configuration
 * All API endpoints and configuration are managed here
 */

// Base API Configuration
export const API_CONFIG = {
  baseUrl: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:8000/api',
  timeout: Number(process.env.NEXT_PUBLIC_REQUEST_TIMEOUT) || 60000, // Increased to 60 seconds for very slow APIs
  defaultPageSize: Number(process.env.NEXT_PUBLIC_DEFAULT_PAGE_SIZE) || 10,
  maxPageSize: Number(process.env.NEXT_PUBLIC_MAX_PAGE_SIZE) || 10000,
  enableLogging: true, // Always enable for debugging
  devMode: process.env.NEXT_PUBLIC_DEV_MODE === 'true'
};

// Auth API Endpoints
export const AUTH_ENDPOINTS = {
  getToken: process.env.NEXT_PUBLIC_AUTH_GET_TOKEN || '/Auth/GetToken',
  updateNewPassword: process.env.NEXT_PUBLIC_AUTH_UPDATE_NEW_PASSWORD || '/Auth/UpdateNewPassword',
  updateExistingPassword: process.env.NEXT_PUBLIC_AUTH_UPDATE_EXISTING_PASSWORD || '/Auth/UpdateExistingPasswordWithNewPassword'
};

// User Management API Endpoints
export const USER_ENDPOINTS = {
  selectAll: process.env.NEXT_PUBLIC_USER_SELECT_ALL || '/MaintainUser/SelectAllMaintainUserDetails',
  selectByUID: process.env.NEXT_PUBLIC_USER_SELECT_BY_UID || '/MaintainUser/SelectMaintainUserDetailsByUID',
  cudEmployee: process.env.NEXT_PUBLIC_USER_CUD_EMPLOYEE || '/MaintainUser/CUDEmployee',
  getMasterData: process.env.NEXT_PUBLIC_USER_GET_MASTER_DATA || '/MaintainUser/GetAllUserMasterDataByLoginID'
};

// Employee API Endpoints
export const EMPLOYEE_ENDPOINTS = {
  getReportsTo: process.env.NEXT_PUBLIC_EMP_GET_REPORTS_TO || '/Emp/GetReportsToEmployeesByRoleUID',
  getSelectionItems: process.env.NEXT_PUBLIC_EMP_GET_SELECTION_ITEMS || '/Emp/GetEmployeesSelectionItemByRoleUID'
};

// Job Position API Endpoints
export const JOB_POSITION_ENDPOINTS = {
  selectByEmp: process.env.NEXT_PUBLIC_JOB_POSITION_SELECT_BY_EMP || '/JobPosition/SelectJobPositionByEmpUID',
  create: process.env.NEXT_PUBLIC_JOB_POSITION_CREATE || '/JobPosition/CreateJobPosition',
  update: process.env.NEXT_PUBLIC_JOB_POSITION_UPDATE || '/JobPosition/UpdateJobPosition',
  updateLocation: process.env.NEXT_PUBLIC_JOB_POSITION_UPDATE_LOCATION || '/JobPosition/UpdateJobPositionLocationTypeAndValue'
};

// Employee Organization Mapping API Endpoints
export const EMP_ORG_MAPPING_ENDPOINTS = {
  getByEmp: process.env.NEXT_PUBLIC_EMP_ORG_MAPPING_GET_BY_EMP || '/EmpOrgMapping/GetEmpOrgMappingDetailsByEmpUID',
  create: process.env.NEXT_PUBLIC_EMP_ORG_MAPPING_CREATE || '/EmpOrgMapping/CreateEmpOrgMapping',
  delete: process.env.NEXT_PUBLIC_EMP_ORG_MAPPING_DELETE || '/EmpOrgMapping/DeleteEmpOrgMapping'
};

// Purchase Order API Endpoints
export const PURCHASE_ORDER_ENDPOINTS = {
  getHeaders: process.env.NEXT_PUBLIC_PO_GET_HEADERS || '/PurchaseOrder/GetPurchaseOrderHeaders',
  getMasterByUID: process.env.NEXT_PUBLIC_PO_GET_MASTER_BY_UID || '/PurchaseOrder/GetPurchaseOrderMasterByUID',
  cudPurchaseOrder: process.env.NEXT_PUBLIC_PO_CUD || '/PurchaseOrder/CUD_PurchaseOrder'
};

// Helper function to build full URL
export const buildApiUrl = (endpoint: string): string => {
  const baseUrl = API_CONFIG.baseUrl.endsWith('/') ? API_CONFIG.baseUrl.slice(0, -1) : API_CONFIG.baseUrl;
  const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
  return `${baseUrl}${cleanEndpoint}`;
};

// Helper function to get common headers
export const getCommonHeaders = (token?: string): Record<string, string> => {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };
  
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  
  return headers;
};

// Helper function for API requests with consistent error handling and retry logic
export const apiRequest = async <T = any>(
  endpoint: string,
  options: RequestInit = {},
  token?: string,
  retryCount: number = 0
): Promise<{
  success: boolean;
  data?: T;
  error?: string;
  status?: number;
}> => {
  const maxRetries = 2; // Will try up to 3 times total

  try {
    const url = buildApiUrl(endpoint);
    const headers = getCommonHeaders(token);

    if (API_CONFIG.enableLogging) {
      console.log(`🌐 API Request: ${options.method || 'GET'} ${url}`);
      if (retryCount > 0) {
        console.log(`🔄 Retry attempt ${retryCount} of ${maxRetries}`);
      }
      if (options.body) {
        console.log('📤 Request Body:', options.body);
      }
    }

    // Always log auth header for debugging 401 issues
    if (headers.Authorization) {
      console.log('🔐 Auth header included:', headers.Authorization.substring(0, 30) + '...');
    } else {
      console.warn('⚠️ No auth header in request - may get 401 error');
    }

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), API_CONFIG.timeout);

    const response = await fetch(url, {
      ...options,
      headers: {
        ...headers,
        ...options.headers,
      },
      signal: controller.signal,
    });

    clearTimeout(timeoutId);

    if (API_CONFIG.enableLogging) {
      console.log(`📥 API Response: ${response.status} ${response.statusText}`);
    }

    if (!response.ok) {
      let errorMessage = `HTTP error! status: ${response.status}`;
      try {
        const errorData = await response.text();
        errorMessage = errorData || errorMessage;
      } catch (e) {
        // Use default error message if can't parse response
      }

      // Don't retry on 4xx errors (client errors)
      if (response.status >= 400 && response.status < 500) {
        return {
          success: false,
          error: errorMessage,
          status: response.status,
        };
      }

      // Retry on 5xx errors (server errors)
      if (response.status >= 500 && retryCount < maxRetries) {
        console.warn(`⚠️ Server error (${response.status}), retrying in 2 seconds...`);
        await new Promise(resolve => setTimeout(resolve, 2000));
        return apiRequest(endpoint, options, token, retryCount + 1);
      }

      return {
        success: false,
        error: errorMessage,
        status: response.status,
      };
    }

    const data = await response.json();

    if (API_CONFIG.enableLogging) {
      console.log('📊 Response Data:', data);
    }

    return {
      success: true,
      data,
      status: response.status,
    };
  } catch (error: any) {
    if (error.name === 'AbortError') {
      // Retry on timeout if we haven't exceeded retry count
      if (retryCount < maxRetries) {
        console.warn(`⏱️ Request timed out, retrying (attempt ${retryCount + 1} of ${maxRetries})...`);
        await new Promise(resolve => setTimeout(resolve, 2000));
        return apiRequest(endpoint, options, token, retryCount + 1);
      }

      return {
        success: false,
        error: 'Request timed out after multiple attempts. The server may be experiencing high load. Please try again later.',
      };
    }

    return {
      success: false,
      error: error.message || 'An unknown error occurred',
    };
  }
};

// Paging request builder
export const buildPagingRequest = (
  page: number = 1,
  pageSize: number = API_CONFIG.defaultPageSize,
  searchQuery?: string,
  filters?: Record<string, any>,
  sortField?: string,
  sortDirection?: 'ASC' | 'DESC'
) => {
  const pagingRequest: any = {
    pageNumber: page,
    pageSize: pageSize,
    filterCriterias: [] as any[],
    isCountRequired: true,
  };

  // Only add sortCriterias if there's actual sorting needed
  if (sortField && sortDirection) {
    pagingRequest.sortCriterias = [{
      fieldName: sortField,
      sortDirection: sortDirection,
    }];
  }

  if (searchQuery) {
    pagingRequest.filterCriterias.push({
      fieldName: 'Name',
      operator: 'LIKE',
      value: `%${searchQuery}%`,
    });
  }

  if (filters) {
    Object.entries(filters).forEach(([key, value]) => {
      if (
        value !== undefined &&
        value !== null &&
        value !== '' &&
        (Array.isArray(value) ? value.length > 0 : true)
      ) {
        pagingRequest.filterCriterias.push({
          Name: key, // Changed from 'name' to 'Name' to match backend FilterCriteria model
          Value: value, // Changed from 'value' to 'Value'
          Type: Array.isArray(value) ? 4 : 1, // Changed from 'type' to 'Type'. FilterType enum: 0=NotEqual, 1=Equal, 4=In
          FilterGroup: 0, // Changed from 'filterGroup' to 'FilterGroup'
          FilterMode: 0 // Changed from 'filterMode' to 'FilterMode'
        });
      }
    });
  }

  return pagingRequest;
};

// Extract paged data helper
export const extractPagedData = (response: any): { items: any[]; totalCount: number } => {
  if (!response) {
    return { items: [], totalCount: 0 };
  }

  // Handle the exact API format from responses
  if (response.Data?.PagedData && Array.isArray(response.Data.PagedData)) {
    return {
      items: response.Data.PagedData,
      totalCount: response.Data.TotalCount || response.Data.PagedData.length,
    };
  }

  // Fallback formats
  if (response.PagedData && Array.isArray(response.PagedData)) {
    return {
      items: response.PagedData,
      totalCount: response.TotalCount || response.PagedData.length,
    };
  }

  if (response.Data && Array.isArray(response.Data)) {
    return {
      items: response.Data,
      totalCount: response.TotalCount || response.Data.length,
    };
  }

  // Default: empty array
  return { items: [], totalCount: 0 };
};