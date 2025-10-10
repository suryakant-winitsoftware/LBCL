import { authService } from '@/lib/auth-service';
import { API_CONFIG } from '@/utils/config';

interface ApiRequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  body?: any;
  headers?: Record<string, string>;
  timeout?: number;
}

class ApiService {
  private baseUrl: string;

  constructor(baseUrl: string = API_CONFIG.BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private async request<T>(
    endpoint: string,
    options: ApiRequestOptions = {}
  ): Promise<T> {
    const { 
      method = 'GET', 
      body, 
      headers = {}, 
      timeout = API_CONFIG.TIMEOUT 
    } = options;

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeout);

    try {
      const token = authService.getToken();
      
      const config: RequestInit = {
        method,
        headers: {
          'Content-Type': 'application/json',
          ...headers,
          ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
        },
        signal: controller.signal,
      };

      if (body && method !== 'GET') {
        config.body = JSON.stringify(body);
      }

      const url = endpoint.startsWith('http') 
        ? endpoint 
        : `${this.baseUrl}${endpoint.startsWith('/') ? '' : '/'}${endpoint}`;

      console.log(`🔍 DEBUG API Request:`, {
        method,
        url,
        body,
        headers: config.headers,
        hasToken: !!token
      });
      
      console.log(`API Request: ${method} ${url}`);
      if (method === 'DELETE') {
        console.log('DELETE request config:', { headers: config.headers, method });
      }
      const response = await fetch(url, config);
      
      console.log(`🔍 DEBUG API Raw Response:`, {
        status: response.status,
        statusText: response.statusText,
        ok: response.ok,
        headers: Object.fromEntries(response.headers.entries())
      });
      
      if (method === 'DELETE') {
        console.log(`DELETE Response: Status ${response.status} - ${response.statusText}`);
      }

      if (!response.ok) {
        // Handle 401 Unauthorized specifically
        if (response.status === 401) {
          // Clear auth data and redirect to login
          authService.logout();
          if (typeof window !== 'undefined') {
            window.location.href = '/login';
          }
          throw new Error('Authentication failed. Please login again.');
        }
        
        // Try to get error message from response
        let errorMessage = `HTTP error! status: ${response.status}`;
        let errorDetails = null;
        
        try {
          const contentType = response.headers.get('content-type');
          if (contentType && contentType.includes('application/json')) {
            errorDetails = await response.json();
            
            // Debug: Log what we actually got
            if (method === 'POST' && endpoint.includes('Holiday')) {
              console.log('Holiday API Error Response:', errorDetails);
            }
            
            // Backend returns error in this format: {StatusCode: 500, ErrorMessage: "...", IsSuccess: false}
            if (errorDetails?.IsSuccess === false && errorDetails?.ErrorMessage) {
              errorMessage = errorDetails.ErrorMessage;
              // Don't log the full error object if we already have the message
              if (response.status !== 404) {
                console.warn(`API Error (${response.status}):`, errorMessage);
              }
            } 
            // Also handle other common error formats
            else if (errorDetails?.Message || errorDetails?.message) {
              errorMessage = errorDetails.Message || errorDetails.message;
              if (response.status !== 404) {
                console.warn(`API Error (${response.status}):`, errorMessage);
              }
            }
            // Only log full error details if we couldn't extract a message
            else if (response.status !== 404 && errorDetails && Object.keys(errorDetails).length > 0) {
              console.error('Unexpected error format:', errorDetails);
            }
          } else {
            // Try to get text response
            const textError = await response.text();
            if (textError && response.status !== 404) {
              console.error('Backend error response (text):', textError);
              errorMessage = textError.substring(0, 200);
            }
          }
        } catch (parseError) {
          console.error('Error parsing error response:', parseError);
        }
        
        console.log('🚨 DEBUG API Error Details:', {
          endpoint,
          method,
          status: response.status,
          statusText: response.statusText,
          errorMessage,
          errorDetails
        });
        
        const error: any = new Error(errorMessage);
        error.status = response.status;
        error.details = errorDetails;
        error.endpoint = endpoint;
        error.method = method;
        
        // For 404s, don't log as error and provide more context
        if (response.status === 404) {
          console.log(`📭 Resource not found: ${method} ${endpoint} - This may be normal depending on the use case`);
        } else {
          console.error(`API Error: ${method} ${endpoint}`, error);
        }
        
        throw error;
      }

      let data;
      const contentType = response.headers.get('content-type');
      
      if (method === 'DELETE') {
        // Handle DELETE responses which might be empty or have different content types
        if (contentType && contentType.includes('application/json')) {
          try {
            data = await response.json();
            console.log(`DELETE API Response: ${method} ${url} - JSON Data:`, data);
          } catch (jsonError) {
            console.log(`DELETE API Response: ${method} ${url} - JSON parse error:`, jsonError);
            // If JSON parsing fails, treat as successful delete with status
            data = { IsSuccess: true, Message: 'Delete operation completed', StatusCode: response.status };
          }
        } else {
          // Non-JSON response (might be empty or plain text)
          const textResponse = await response.text();
          console.log(`DELETE API Response: ${method} ${url} - Text Response:`, textResponse);
          
          // For successful delete operations, create a success response
          data = { 
            IsSuccess: true, 
            Message: textResponse || 'Delete operation completed successfully',
            StatusCode: response.status 
          };
        }
      } else {
        // Regular JSON handling for non-DELETE methods
        data = await response.json();
        console.log(`API Response: ${method} ${url} - Success`);
      }
      
      return data;
    } catch (error: any) {
      if (error.name === 'AbortError') {
        const timeoutError: any = new Error('Request timeout');
        timeoutError.status = 408;
        timeoutError.endpoint = endpoint;
        timeoutError.method = method;
        throw timeoutError;
      }
      
      // Add context to the error
      const apiError: any = error;
      if (!apiError.endpoint) {
        apiError.endpoint = endpoint;
        apiError.method = method;
      }
      
      // Always log the endpoint that failed for debugging
      console.error(`API Request Failed: ${method} ${endpoint}`, {
        status: apiError.status,
        message: apiError.message,
        details: apiError.details
      });
      throw apiError;
    } finally {
      clearTimeout(timeoutId);
    }
  }

  async get<T>(endpoint: string, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET', headers });
  }

  async post<T>(
    endpoint: string, 
    body?: any, 
    headers?: Record<string, string>
  ): Promise<T> {
    return this.request<T>(endpoint, { method: 'POST', body, headers });
  }

  async put<T>(
    endpoint: string, 
    body?: any, 
    headers?: Record<string, string>
  ): Promise<T> {
    return this.request<T>(endpoint, { method: 'PUT', body, headers });
  }

  async delete<T>(
    endpoint: string, 
    headers?: Record<string, string>
  ): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE', headers });
  }

  async patch<T>(
    endpoint: string, 
    body?: any, 
    headers?: Record<string, string>
  ): Promise<T> {
    return this.request<T>(endpoint, { method: 'PATCH', body, headers });
  }
}

export const apiService = new ApiService();

export const api = {
  org: {
    getDetails: (body: any) => apiService.post('/Org/GetOrgDetails', body),
  },
  route: {
    selectAll: (body: any, orgUID?: string) => apiService.post(`/Route/SelectAllRouteDetails${orgUID ? `?OrgUID=${encodeURIComponent(orgUID)}` : ''}`, body),
    selectChangeLogAll: (body: any) => apiService.post('/Route/SelectRouteChangeLogAllDetails', body),
    create: (body: any) => apiService.post('/Route/CreateRouteMaster', body), // Fixed endpoint
    update: (body: any) => apiService.put('/Route/UpdateRouteMaster', body), // Added update endpoint
    getUserDDL: (orgUID: string) => apiService.get(`/Route/GetUserDDL?OrgUID=${orgUID}`),
    getByUID: (uid: string) => apiService.get(`/Route/SelectRouteDetailByUID?UID=${uid}`),
    getMasterByUID: (uid: string) => apiService.get(`/Route/SelectRouteMasterViewByUID?UID=${uid}`),
  },
  beatHistory: {
    create: (body: any) => apiService.post('/BeatHistory/CreateBeatHistory', body),
    createBulkStoreHistories: (body: any) => apiService.post('/BeatHistory/CreateBulkStoreHistories', body),
    selectAll: (body: any) => apiService.post('/BeatHistory/SelectAllJobPositionDetails', body),
    getByRouteUID: (routeUID: string) => apiService.get(`/BeatHistory/GetSelectedBeatHistoryByRouteUID?RouteUID=${routeUID}`),
    getTodayByRouteUID: (routeUID: string) => apiService.get(`/BeatHistory/GetTodayBeatHistoryByRouteUID?RouteUID=${routeUID}`),
    getCustomersByBeatHistoryUID: (beatHistoryUID: string) => apiService.get(`/BeatHistory/GetCustomersByBeatHistoryUID?BeatHistoryUID=${beatHistoryUID}`),
    getByUID: (uid: string) => apiService.get(`/BeatHistory/GetBeatHistoryByUID?UID=${uid}`),
    update: (body: any) => apiService.put('/BeatHistory/UpdateBeatHistory', body),
    delete: (uid: string) => apiService.delete(`/BeatHistory/DeleteBeatHistory?UID=${uid}`),
    // Additional deletion endpoints for cascade operations
    deleteStoreHistory: (uid: string) => apiService.delete(`/BeatHistory/DeleteStoreHistory?UID=${uid}`),
    hardDelete: (uid: string) => apiService.delete(`/BeatHistory/HardDeleteBeatHistory?UID=${uid}`),
    cascadeDelete: (uid: string) => apiService.delete(`/BeatHistory/CascadeDeleteBeatHistory?UID=${uid}`),
  },
  dropdown: {
    getRoute: (orgUID: string) => apiService.post(`/Dropdown/GetRouteDropDown?orgUID=${orgUID}`, {}),
    getEmployee: (orgUID: string, byLoginId: boolean = false) => 
      apiService.post(`/Dropdown/GetEmpDropDown?orgUID=${orgUID}&getDataByLoginId=${byLoginId}`, {}),
    getCustomer: (orgUID: string) => apiService.post(`/Dropdown/GetCustomerDropDown?franchiseeOrgUID=${orgUID}`, {}),
    getCustomersByRoute: (routeUID: string) => apiService.post(`/Dropdown/GetCustomersByRouteUIDDropDown?routeUID=${routeUID}`, {}),
    getVehicle: (orgUID: string) => apiService.post(`/Dropdown/GetVehicleDropDown?orgUID=${orgUID}`, {}),
    getJobPosition: (orgUID: string) => apiService.post(`/Dropdown/GetJobPositionDropDown?orgUID=${orgUID}`, {}),
    getLocationsByTypes: (locationTypes: string[]) => apiService.post('/Location/GetLocationByTypes', locationTypes),
  },
  // Route Schedule APIs - DISABLED: Backend endpoints not yet implemented
  // routeSchedule: {
  //   selectAll: (body: any) => apiService.post('/RouteSchedule/SelectAllRouteScheduleDetails', body),
  //   getByRouteUID: (routeUID: string) => apiService.get(`/RouteSchedule/GetRouteScheduleByRouteUID?RouteUID=${routeUID}`),
  //   create: (body: any) => apiService.post('/RouteSchedule/CreateRouteSchedule', body),
  //   update: (body: any) => apiService.post('/RouteSchedule/UpdateRouteSchedule', body),
  //   delete: (uid: string) => apiService.delete(`/RouteSchedule/DeleteRouteSchedule?UID=${uid}`),
  //   getDaywise: (routeScheduleUID: string) => apiService.get(`/RouteSchedule/GetRouteScheduleDaywiseByUID?RouteScheduleUID=${routeScheduleUID}`),
  //   createDaywise: (body: any) => apiService.post('/RouteSchedule/CreateRouteScheduleDaywise', body),
  //   updateDaywise: (body: any) => apiService.post('/RouteSchedule/UpdateRouteScheduleDaywise', body),
  // },
  // Vehicle APIs
  vehicle: {
    selectAll: (body: any) => apiService.post('/Vehicle/SelectAllVehicleDetails', body),
    getByUID: (uid: string) => apiService.get(`/Vehicle/GetVehicleByUID?UID=${uid}`),
    getByOrg: (orgUID: string) => apiService.post(`/Dropdown/GetVehicleDropDown?orgUID=${orgUID}`, {}),
    create: (body: any) => apiService.post('/Vehicle/CreateVehicle', body),
    update: (body: any) => apiService.post('/Vehicle/UpdateVehicle', body),
    delete: (uid: string) => apiService.delete(`/Vehicle/DeleteVehicle?UID=${uid}`),
  },
  // Store History APIs - Re-enabled for deletion operations
  storeHistory: {
    // Basic CRUD operations (some may not be implemented in backend)
    selectAll: (body: any) => apiService.post('/StoreHistory/SelectAllStoreHistoryDetails', body),
    getByBeatHistoryUID: (beatHistoryUID: string) => apiService.get(`/StoreHistory/GetStoreHistoryByBeatHistoryUID?BeatHistoryUID=${beatHistoryUID}`),
    create: (body: any) => apiService.post('/StoreHistory/CreateStoreHistory', body),
    createMultiple: (body: any) => apiService.post('/StoreHistory/CreateMultipleStoreHistories', body),
    update: (body: any) => apiService.post('/StoreHistory/UpdateStoreHistory', body),
    updateStatus: (body: any) => apiService.post('/BeatHistory/UpdateStoreHistoryStatus', body), // This one exists
    
    // Deletion endpoints - Critical for cascade operations
    delete: (uid: string) => apiService.delete(`/StoreHistory/DeleteStoreHistory?UID=${uid}`),
    hardDelete: (uid: string) => apiService.delete(`/StoreHistory/HardDeleteStoreHistory?UID=${uid}`),
    deleteByBeatHistory: (beatHistoryUID: string) => apiService.delete(`/StoreHistory/DeleteByBeatHistoryUID?BeatHistoryUID=${beatHistoryUID}`),
    
    // Stats operations
    getStats: (storeHistoryUID: string) => apiService.get(`/StoreHistory/GetStoreHistoryStatsByUID?StoreHistoryUID=${storeHistoryUID}`),
    createStats: (body: any) => apiService.post('/StoreHistory/CreateStoreHistoryStats', body),
    updateStats: (body: any) => apiService.post('/StoreHistory/UpdateStoreHistoryStats', body),
    deleteStats: (storeHistoryUID: string) => apiService.delete(`/StoreHistory/DeleteStoreHistoryStats?StoreHistoryUID=${storeHistoryUID}`),
    deleteStatsByUID: (uid: string) => apiService.delete(`/StoreHistory/DeleteStoreHistoryStats?UID=${uid}`),
  },
  // Route Customer APIs
  routeCustomer: {
    getByRouteUID: (routeUID: string) => apiService.get(`/Route/GetRouteCustomersByRouteUID?RouteUID=${routeUID}`),
    create: (body: any) => apiService.post('/Route/CreateRouteCustomer', body),
    update: (body: any) => apiService.post('/Route/UpdateRouteCustomer', body),
    delete: (uid: string) => apiService.delete(`/Route/DeleteRouteCustomer?UID=${uid}`),
    updateSequence: (body: any) => apiService.post('/Route/UpdateRouteCustomerSequence', body),
  },
  
  // Flexible Route Customer Scheduling APIs
  routeCustomerSchedule: {
    create: (body: any) => apiService.post('/Route/CreateRouteCustomerSchedule', body),
    update: (body: any) => apiService.put('/Route/UpdateRouteCustomerSchedule', body),
    createBatch: (schedules: any[]) => apiService.post('/Route/CreateRouteCustomerScheduleBatch', schedules),
    getByRoute: (routeUID: string) => apiService.get(`/Route/GetRouteCustomerSchedules?routeUID=${routeUID}`),
    getForDate: (routeUID: string, date: string) => apiService.get(`/Route/GetRouteCustomersForDate?routeUID=${routeUID}&date=${date}`),
    delete: (uid: string) => apiService.delete(`/Route/DeleteRouteCustomerSchedule?uid=${uid}`),
  },
  // Journey Plan Workflow APIs - DISABLED: Most endpoints not yet implemented
  // journeyPlanWorkflow: {
  //   createComplete: (body: any) => apiService.post('/BeatHistory/CreateCompleteJourneyPlan', body),
  //   finalize: (body: any) => apiService.post('/BeatHistory/FinalizeJourneyPlan', body),
  //   activate: (body: any) => apiService.post('/BeatHistory/ActivateJourneyPlan', body),
  //   suspend: (body: any) => apiService.post('/BeatHistory/SuspendJourneyPlan', body),
  //   getStatus: (beatHistoryUID: string) => apiService.get(`/BeatHistory/GetJourneyPlanStatus?BeatHistoryUID=${beatHistoryUID}`),
  //   copy: (body: any) => apiService.post('/BeatHistory/CopyJourneyPlan', body),
  //   getTemplate: (routeUID: string, templateType: string) => apiService.get(`/BeatHistory/GetJourneyPlanTemplate?RouteUID=${routeUID}&TemplateType=${templateType}`),
  //   saveAsTemplate: (body: any) => apiService.post('/BeatHistory/SaveJourneyPlanAsTemplate', body),
  //   getAnalytics: (body: any) => apiService.post('/BeatHistory/GetJourneyPlanAnalytics', body),
  //   getPerformanceReport: (routeUID: string, startDate: string, endDate: string) => 
  //     apiService.get(`/BeatHistory/GetRoutePerformanceReport?RouteUID=${routeUID}&StartDate=${startDate}&EndDate=${endDate}`),
  //   getComplianceReport: (body: any) => apiService.post('/BeatHistory/GetJourneyPlanComplianceReport', body),
  // },
  role: {
    selectRolesByOrgUID: (orgUID: string) => apiService.get(`/Role/SelectRolesByOrgUID?OrgUID=${orgUID}`),
    getEmployeesByRoleUID: (roleUID: string) => apiService.get(`/Role/GetEmployeesSelectionItemByRoleUID?RoleUID=${roleUID}`),
  },
  jobPosition: {
    selectAll: (body: any) => apiService.post('/JobPosition/SelectAllJobPositionDetails', body),
    getByUID: (uid: string) => apiService.get(`/JobPosition/GetJobPositionByUID?UID=${uid}`),
  },
  location: {
    selectAll: (body: any) => apiService.post('/Location/SelectAllLocationDetails', body),
    getByUID: (uid: string) => apiService.get(`/Location/GetLocationByUID?UID=${encodeURIComponent(uid)}`),
    getByTypes: (locationTypes: string[]) => apiService.post('/Location/GetLocationByTypes', locationTypes),
  },
  store: {
    // Main CRUD operations
    selectAll: (body: any) => apiService.post('/Store/SelectAllStore', body),
    getByUID: (uid: string) => apiService.get(`/Store/SelectStoreByUID?UID=${encodeURIComponent(uid)}`),
    create: (body: any) => apiService.post('/Store/CreateStore', body),
    update: (body: any) => apiService.put('/Store/UpdateStore', body),
    delete: (uid: string) => apiService.delete(`/Store/DeleteStore?UID=${encodeURIComponent(uid)}`),
    
    // Store Master operations (with all related data)
    createMaster: (body: any) => apiService.post('/Store/CreateStoreMaster', body),
    getMasterByUID: (uid: string) => apiService.get(`/Store/SelectStoreByUID?UID=${encodeURIComponent(uid)}`),
    updateMaster: (body: any) => apiService.put('/Store/UpdateStoreMaster', body),
    
    // Store selection and items
    getAllStoreItems: (body: any, orgUID?: string) => 
      apiService.post(`/Store/GetAllStoreItems${orgUID ? `?OrgUID=${orgUID}` : ''}`, body),
    getAsSelectionItems: (body: any, orgUID?: string) => 
      apiService.post(`/Store/GetAllStoreAsSelectionItems${orgUID ? `?OrgUID=${orgUID}` : ''}`, body),
    
    // Store by route
    getByRouteUID: (routeUID: string) => apiService.get(`/Store/GetStoreCustomersByRouteUID?routeUID=${routeUID}`),
    getByOrgUID: (franchiseeOrgUID: string) => apiService.get(`/Store/SelectStoreByOrgUID?FranchiseeOrgUID=${franchiseeOrgUID}`),
  },
  userJourney: {
    // Use the correct working endpoints from UserJourney controller
    getGridDetails: (body: any) => apiService.post('/UserJourney/GetUserJourneyGridDetails', body),
    getByUID: (uid: string) => apiService.get(`/UserJourney/GetUserJourneyDetailsByUID?UID=${uid}`),
    create: (body: any) => apiService.post('/Route/CreateNewRoutes', body),
    update: (body: any) => apiService.post('/Route/UpdateRoute', body),
    delete: (uid: string) => apiService.delete(`/BeatHistory/DeleteBeatHistory?UID=${uid}`), // Fixed: Both journey and plan types use BeatHistory
    complete: (uid: string) => apiService.post(`/Route/CompleteRoute?UID=${uid}`, {}),
    selectTodayPlan: (visitDate: string, orgUID: string, jobPositionUID: string, body: any) => {
      // Build query params dynamically to avoid empty strings
      const params = new URLSearchParams();
      params.append('Type', 'Assigned');
      params.append('VisitDate', visitDate);
      if (jobPositionUID && jobPositionUID.trim() !== '') {
        params.append('JobPositionUID', jobPositionUID);
      }
      if (orgUID && orgUID.trim() !== '') {
        params.append('OrgUID', orgUID);
      }
      return apiService.post(`/UserJourney/SelectTodayJourneyPlanDetails?${params.toString()}`, body);
    },
  },
  journeyPlan: {
    // Salesman APIs - for execution
    getByUID: (uid: string) => apiService.get(`/BeatHistory/GetBeatHistoryByUID?UID=${uid}`),
    update: (body: any) => apiService.put('/BeatHistory/UpdateBeatHistory', body),
    updateBeatHistory: (uid: string, body: any) => apiService.put('/BeatHistory/UpdateBeatHistory', { ...body, UID: uid }),
    delete: (uid: string) => apiService.delete(`/BeatHistory/DeleteBeatHistory?UID=${uid}`),
    start: (uid: string) => apiService.post(`/BeatHistory/StartBeatHistory?UID=${uid}`, {}),
    getSummary: (body: any) => apiService.post('/BeatHistory/SelectAllJobPositionDetails', body),

    // Create journey plan using BeatHistory API
    createBeatHistory: (body: any) => apiService.post('/BeatHistory/CreateBeatHistory', body),
  },
  // Load Request APIs - Using WHStock controller endpoints
  loadRequest: {
    // Main endpoint for getting load requests with pagination and filtering
    getAll: (body: any, stockType?: string) => apiService.post(`/WHStock/SelectLoadRequestData${stockType ? `?StockType=${stockType}` : ''}`, body),
    getByUID: (uid: string) => apiService.get(`/WHStock/SelectLoadRequestDataByUID?UID=${uid}`),
    // Create/Update/Delete operations
    create: (body: any) => apiService.post('/WHStock/CUDWHStock', body),
    update: (body: any) => apiService.post('/WHStock/CUDWHStock', body),
    delete: (body: any) => apiService.post('/WHStock/CUDWHStock', body),
    // Request line operations
    updateLines: (body: any) => apiService.post('/WHStock/CUDWHStockRequestLine', body),
    // Queue-based creation
    createFromQueue: (body: any) => apiService.post('/WHStock/CreateWHStockFromQueue', body),
  },
};