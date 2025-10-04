import { apiService, api } from './api';
import { authService } from '@/lib/auth-service';
import { employeeService } from '@/services/admin/employee.service';

// Types
export interface Route {
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  JobPositionUID?: string;
  VehicleUID?: string;
  IsActive: boolean;
  Status: string;
  ValidFrom: string;
  ValidUpto: string;
  TotalCustomers: number;
  CreatedBy: string;
  ModifiedBy: string;
  ModifiedTime?: string;
}

export interface UserJourney {
  id: number;
  uid: string;
  jobPositionUID: string;
  empUID: string;
  loginId: string;
  journeyStartTime: string;
  journeyEndTime?: string;
  startOdometerReading: number;
  endOdometerReading: number;
  vehicleUID?: string;
  eotStatus: string;
  hasAuditCompleted: boolean;
  beatHistoryUID: string;
  attendanceStatus?: string;
  attendanceLatitude?: string;
  attendanceLongitude?: string;
  attendanceAddress?: string;
}

export interface BeatHistory {
  uid: string;
  routeUID: string;
  jobPositionUID: string;
  loginId: string;
  visitDate: string;
  plannedStartTime: string;
  plannedEndTime: string;
  plannedStoreVisits: number;
  actualStoreVisits: number;
  skippedStoreVisits: number;
  coverage: number;
  yearMonth: number;
}

export interface StoreHistory {
  UID: string;
  UserJourneyUID: string;
  BeatHistoryUID: string;
  RouteUID: string;
  StoreUID: string;
  Status: string;
  LoginTime?: string;
  LogoutTime?: string;
  IsPlanned: boolean;
  IsProductive: boolean;
  IsSkipped: boolean;
  ActualValue: number;
  ActualVolume: number;
  ActualLines: number;
  Latitude?: string;
  Longitude?: string;
  VisitDuration?: number;
  TravelTime?: number;
  PlannedLoginTime?: string;
  PlannedLogoutTime?: string;
  SerialNo?: number;
  TargetValue?: number;
  TargetVolume?: number;
  TargetLines?: number;
  IsGreen?: boolean;
  PlannedTimeSpendInMinutes?: number;
  Notes?: string;
  NoOfVisits?: number;
  LastVisitDate?: string;
}

export interface SalesTeamActivityFilters {
  startDate: string;
  endDate: string;
  routeUID?: string;
  jobPositionUID?: string;
  empUID?: string;
  status?: string;
}

export interface DashboardMetrics {
  totalJourneys: number;
  activeRoutes: number;
  totalStoresVisited: number;
  productiveStores: number;
  averageCoverage: number;
  totalSalesValue: number;
  averageVisitDuration: number;
  skippedStores: number;
}

export interface RoutePerformance {
  routeUID: string;
  routeName: string;
  totalVisits: number;
  plannedVisits: number;
  actualVisits: number;
  coverage: number;
  salesValue: number;
  productivityRate: number;
}

class SalesTeamActivityService {
  // Clean Routes API with working server-side filtering - CONSERVATIVE APPROACH
  async getRoutes(options: any = {}) {
    try {
      console.log('🔍 SERVICE: getRoutes called with options:', options);

      // Start with empty filters and add them one by one to isolate the issue
      let filterCriterias: any[] = [];

      // Only add filters that are known to work - avoid complex combinations

      // Simple role filtering (working)
      if (options.selectedRole && options.selectedRole !== 'all' && options.selectedRole !== '') {
        filterCriterias.push({
          Name: 'RoleUID',
          Value: options.selectedRole,
          Type: 1, // Equal
          DataType: 'System.String',
          FilterGroup: 0,
          FilterMode: 0
        });
      }

      // Simple employee filtering (working)
      if (options.selectedEmployee && options.selectedEmployee !== 'all' && options.selectedEmployee !== '') {
        filterCriterias.push({
          Name: 'JobPositionUID',
          Value: options.selectedEmployee,
          Type: 1, // Equal
          DataType: 'System.String',
          FilterGroup: 0,
          FilterMode: 0
        });
      }

      // DISABLE STATUS FILTERING TEMPORARILY - it may be causing the OR issue
      // if (options.status && options.status.length > 0) {
      //   // Status filtering logic here
      // }

      // SEARCH BY BOTH CODE AND NAME - using OR logic
      if (options.search && options.search.trim()) {
        const searchTerm = options.search.trim();
        console.log('🔍 SERVICE: Adding search filters for term:', searchTerm);

        // Search by code (first filter)
        filterCriterias.push({
          Name: 'code',
          Value: searchTerm,
          Type: 2, // Like/Contains for string
          DataType: 'System.String',
          FilterGroup: 0,
          FilterMode: 0 // AND for first filter
        });

        // Search by name (second filter with OR)
        filterCriterias.push({
          Name: 'name',
          Value: searchTerm,
          Type: 2, // Like/Contains for string
          DataType: 'System.String',
          FilterGroup: 0,
          FilterMode: 1 // OR - this creates (code LIKE x OR name LIKE x)
        });
      }

      console.log('🔍 SERVICE: Final filterCriterias:', JSON.stringify(filterCriterias, null, 2));

      // Make the API call
      const requestPayload = {
        PageNumber: options.pageNumber || 1,
        PageSize: options.pageSize || 50,
        IsCountRequired: true,
        FilterCriterias: filterCriterias,
        SortCriterias: []
      };

      console.log('🔍 SERVICE: Making API call with payload:', JSON.stringify(requestPayload, null, 2));

      const result = await apiService.post('/Route/SelectAllRouteDetails?OrgUID=EPIC01', requestPayload);

      console.log('🔍 SERVICE: API call successful, result:', {
        isSuccess: result?.IsSuccess,
        dataCount: result?.Data?.PagedData?.length,
        totalCount: result?.Data?.TotalCount
      });

      return result;

    } catch (error: any) {
      console.error('❌ SERVICE: Failed to load routes:', error);
      console.error('❌ SERVICE: Error details:', {
        message: error.message,
        status: error.status,
        endpoint: error.endpoint
      });

      // If there's a SQL syntax error, try with NO filters as a fallback
      if (error.message && error.message.includes('syntax error')) {
        console.log('🔄 SERVICE: SQL syntax error detected, trying fallback with no filters...');
        try {
          const fallbackPayload = {
            PageNumber: options.pageNumber || 1,
            PageSize: options.pageSize || 50,
            IsCountRequired: true,
            FilterCriterias: [], // NO FILTERS
            SortCriterias: []
          };

          console.log('🔄 SERVICE: Fallback API call with no filters');
          const fallbackResult = await apiService.post('/Route/SelectAllRouteDetails?OrgUID=EPIC01', fallbackPayload);

          console.log('✅ SERVICE: Fallback successful, returning all routes');
          return fallbackResult;
        } catch (fallbackError) {
          console.error('❌ SERVICE: Fallback also failed:', fallbackError);
        }
      }

      return {
        Data: { PagedData: [], TotalCount: 0 },
        IsSuccess: false,
        StatusCode: 500,
        Message: error.message || 'Failed to load routes'
      };
    }
  }

  // Simple test method - get routes with no filters at all
  async getRoutesSimple() {
    try {
      console.log('🧪 SERVICE: Testing basic API call with no filters...');

      const requestPayload = {
        PageNumber: 1,
        PageSize: 10,
        IsCountRequired: true,
        FilterCriterias: [],
        SortCriterias: []
      };

      const result = await apiService.post('/Route/SelectAllRouteDetails?OrgUID=EPIC01', requestPayload);

      console.log('🧪 SERVICE: Basic API test result:', {
        isSuccess: result?.IsSuccess,
        dataCount: result?.Data?.PagedData?.length,
        totalCount: result?.Data?.TotalCount
      });

      return result;
    } catch (error) {
      console.error('🧪 SERVICE: Basic API test failed:', error);
      throw error;
    }
  }

  // BACKUP: Keep original method for reference
  async getRoutesWithFilters(filters: any = {}) {
    try {
      console.log('🔍 SERVICE: Testing route API with filters:', filters);
      
      // Build filter criteria using correct PascalCase field names (matching model properties)
      console.log('🔍 SERVICE: Building filter criteria with PascalCase field names');
      let filterCriterias: any[] = [];
      
      // Status filter (IsActive)
      if (filters.status && filters.status.length > 0) {
        if (filters.status.includes('active')) {
          filterCriterias.push({
            Name: 'IsActive', // Use PascalCase - backend will map to is_active
            Value: true,
            Type: 1, // FilterType.Equal
            DataType: typeof(true),
            FilterGroup: 0, // Field
            FilterMode: 0 // And
          });
        } else if (filters.status.includes('inactive')) {
          filterCriterias.push({
            Name: 'IsActive', // Use PascalCase - backend will map to is_active
            Value: false,
            Type: 1, // FilterType.Equal
            DataType: typeof(false),
            FilterGroup: 0, // Field
            FilterMode: 0 // And
          });
        }
      }
      
      // Role filter (RoleUID)
      if (filters.selectedRole && filters.selectedRole !== 'all') {
        filterCriterias.push({
          Name: 'RoleUID', // PascalCase to match model property
          Value: filters.selectedRole,
          Type: 1, // Equal
          DataType: typeof(''),
          FilterGroup: 0, // Field
          FilterMode: 0 // And
        });
      }
      
      // Employee filter (JobPositionUID)
      if (filters.selectedEmployee && filters.selectedEmployee !== 'all') {
        filterCriterias.push({
          Name: 'JobPositionUID', // PascalCase to match model property
          Value: filters.selectedEmployee,
          Type: 1, // Equal
          DataType: typeof(''),
          FilterGroup: 0, // Field
          FilterMode: 0 // And
        });
      }
      
      // Search filter (Code or Name)
      if (filters.search && filters.search.trim()) {
        // Add OR condition for search across Code and Name
        filterCriterias.push({
          Name: 'Code',
          Value: filters.search.trim(),
          Type: 15, // FilterType.Contains
          DataType: typeof(''),
          FilterGroup: 0, // Field
          FilterMode: 1 // Or
        });
        filterCriterias.push({
          Name: 'Name',
          Value: filters.search.trim(), 
          Type: 15, // FilterType.Contains
          DataType: typeof(''),
          FilterGroup: 0, // Field
          FilterMode: 1 // Or
        });
      }
      
      // TODO: Add filters back one by one after confirming basic API works
      // if (filters.search && filters.search.trim()) {
      //   filterCriterias.push({
      //     Name: 'code',
      //     Value: filters.search.trim(),
      //     Type: 10, // FilterType.Contains
      //     FilterMode: 1 // FilterMode.Or
      //   });
      // }

      console.log('🔍 SERVICE: Using filterCriterias:', filterCriterias);

      const requestBody = {
        PageNumber: filters.pageNumber || 1,
        PageSize: filters.pageSize || 50,
        IsCountRequired: true,
        FilterCriterias: filterCriterias,
        SortCriterias: []
      };

      console.log('🔍 SERVICE: Request body:', JSON.stringify(requestBody, null, 2));
      console.log('🔍 SERVICE: About to send request to:', '/Route/SelectAllRouteDetails?OrgUID=EPIC01');

      const response = await apiService.post('/Route/SelectAllRouteDetails?OrgUID=EPIC01', requestBody);

      console.log('🔍 SERVICE: Server response:', {
        success: response?.IsSuccess,
        dataCount: response?.Data?.PagedData?.length,
        totalCount: response?.Data?.TotalCount
      });

      return response;
    } catch (error) {
      console.error('Error loading routes - testing basic API call:', error);
      return { Data: { PagedData: [], TotalCount: 0 }, IsSuccess: false };
    }
  }

  // Get roles for filtering - using exact same implementation as route creation/edit pages
  async getRoles() {
    try {
      
      // Use the exact same API call as route creation/edit pages
      const data = await apiService.post("/Role/SelectAllRoles", {
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: [],
      });
      
      console.log('🔍 SERVICE: SelectAllRoles API response:', {
        data,
        isSuccess: data?.IsSuccess,
        hasData: !!data?.Data,
        hasPagedData: !!data?.Data?.PagedData,
        pagedDataLength: data?.Data?.PagedData?.length
      });
      
      // Use the exact same logic as route pages
      if (data.IsSuccess && data.Data?.PagedData) {
        const roles = data.Data.PagedData.map((role: any) => ({
          UID: role.UID,
          Name: role.RoleNameEn || role.Code,
          Code: role.Code
        }));
        
        console.log(`✅ SERVICE: Found ${roles.length} roles:`, roles.slice(0, 3));
        return roles;
      } else {
        console.warn('⚠️ SERVICE: Invalid response structure or no roles found');
        console.log('Full response:', data);
        return [];
      }
    } catch (error) {
      console.error('❌ SERVICE: Error fetching roles:', error);
      return [];
    }
  }

  // Get employees for filtering - using exact same implementation as route creation page
  async getEmployees(orgUID: string = 'EPIC01', roleUID?: string) {
    try {
      console.log(`🔍 SERVICE: Loading employees using exact same logic as route pages - org: ${orgUID}${roleUID ? `, role: ${roleUID}` : ''}`);
      
      let employees = [];
      
      if (roleUID) {
        console.log("🔍 Loading employees for org + role:", orgUID, roleUID);
        // Always use organization + role API first (most accurate) - same as route creation
        try {
          console.log("🎯 Using org + role combination API");
          const orgRoleEmployees = await employeeService.getEmployeesSelectionItemByRoleUID(
            orgUID,
            roleUID
          );
          
          if (orgRoleEmployees && orgRoleEmployees.length > 0) {
            employees = orgRoleEmployees.map((item: any) => ({
              UID: item.UID || item.Value || item.uid,
              Name: item.Name || item.name,
              Code: item.Code || item.code,
              Label: item.Label || item.Text || `[${item.Code || item.code}] ${item.Name || item.name}`,
            }));
            console.log(`✅ Found ${employees.length} employees for org '${orgUID}' + role '${roleUID}'`);
          } else {
            // Secondary: Try role-based API and then filter by org - same as route creation
            console.log("🔄 Trying role-based API with org filtering");
            const roleBasedEmployees = await employeeService.getReportsToEmployeesByRoleUID(roleUID);
            
            if (roleBasedEmployees && roleBasedEmployees.length > 0) {
              // Load all org employees to cross-reference - same as route creation
              const orgData = await api.dropdown.getEmployee(orgUID, false);
              const orgEmployeeUIDs = new Set();
              
              if (orgData.IsSuccess && orgData.Data) {
                orgData.Data.forEach((emp: any) => {
                  orgEmployeeUIDs.add(emp.UID);
                });
              }
              
              // Filter role employees to only include those in the selected org
              const filteredRoleEmployees = roleBasedEmployees.filter(
                (emp: any) => orgEmployeeUIDs.has(emp.UID || emp.uid)
              );
              
              employees = filteredRoleEmployees.map((emp: any) => ({
                UID: emp.UID || emp.uid,
                Name: emp.Name || emp.name,
                Code: emp.Code || emp.code,
                Label: `[${emp.Code || emp.code}] ${emp.Name || emp.name}`,
              }));
              console.log(`✅ Found ${employees.length} role-based employees filtered for org '${orgUID}'`);
            }
          }
        } catch (roleError) {
          console.error("Role-based employee loading failed:", roleError);
        }
      }
      
      // If no role-based employees found or no role provided, fall back to org-based loading - same as route creation
      if (employees.length === 0) {
        console.log("📄 Loading all employees for organization:", orgUID);
        const data = await api.dropdown.getEmployee(orgUID, false);
        
        if (data.IsSuccess && data.Data) {
          employees = data.Data.map((emp: any) => ({
            UID: emp.UID,
            Name: emp.Name,
            Code: emp.Code,
            Label: emp.Label,
          }));
          console.log(`📊 Loaded ${employees.length} org-based employees`);
        }
      }
      
      console.log(`📊 SERVICE: Final employees result: ${employees.length} employees`);
      return employees;
    } catch (error) {
      console.error('❌ SERVICE: Error fetching employees:', error);
      return [];
    }
  }

  private deduplicateRoutes(routes: any[]): any[] {
    const routeMap = new Map<string, any>();
    
    routes.forEach(route => {
      const existingRoute = routeMap.get(route.UID);
      
      if (!existingRoute) {
        // First occurrence of this UID
        routeMap.set(route.UID, route);
      } else {
        // Compare modification times and keep the most recent
        const existingTime = existingRoute.ModifiedTime ? new Date(existingRoute.ModifiedTime).getTime() : 0;
        const currentTime = route.ModifiedTime ? new Date(route.ModifiedTime).getTime() : 0;
        
        if (currentTime > existingTime) {
          routeMap.set(route.UID, route);
        }
      }
    });
    
    return Array.from(routeMap.values());
  }

  async getRouteChangeLog(filters: any = {}) {
    try {
      const response = await apiService.post('/Route/SelectRouteChangeLogAllDetails', {
        pageNumber: filters.pageNumber || 1,
        pageSize: filters.pageSize || 50,
        isCountRequired: true,
        filterCriterias: filters.filterCriterias || [],
        sortCriterias: []
      });
      return response.data || { Data: { PagedData: [], TotalCount: 0 } };
    } catch (error) {
      console.error('Error fetching route change log:', error);
      return { Data: { PagedData: [], TotalCount: 0 } };
    }
  }

  async getRouteByUID(uid: string) {
    try {
      const response = await apiService.get(`/Route/SelectRouteDetailByUID?UID=${uid}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching route by UID:', error);
      throw error;
    }
  }

  // Get users/employees assigned to routes
  async getRouteUsers(routeUID?: string) {
    try {
      // Convert to backend FilterCriteria format
      const filterCriterias = routeUID ? [{
        name: 'RouteUID',
        value: routeUID,
        type: 1 // Equal
      }] : [];
      
      // This would typically come from a RouteUser or Employee API
      // For now, let's get unique users from BeatHistory
      const response = await apiService.post('/BeatHistory/SelectAllJobPositionDetails', {
        pageNumber: 1,
        pageSize: 1000,
        isCountRequired: true,
        filterCriterias
      });

      if (response?.Data?.PagedData) {
        // Extract unique users from beat history
        const users = new Map();
        response.Data.PagedData.forEach((beat: any) => {
          if (beat.JobPositionUID && beat.LoginId) {
            users.set(beat.JobPositionUID, {
              jobPositionUID: beat.JobPositionUID,
              loginId: beat.LoginId,
              empUID: beat.DefaultJobPositionUID || beat.JobPositionUID
            });
          }
        });
        return { Data: { PagedData: Array.from(users.values()), TotalCount: users.size }, IsSuccess: true };
      }
      
      return { Data: { PagedData: [], TotalCount: 0 }, IsSuccess: false };
    } catch (error) {
      console.error('Error fetching route users:', error);
      return { Data: { PagedData: [], TotalCount: 0 }, IsSuccess: false };
    }
  }

  // User Journey APIs
  async getUserJourneys(filters: any = {}) {
    try {
      // Convert frontend filter format to backend FilterCriteria format
      let convertedFilters: any[] = [];
      
      if (Array.isArray(filters.filterCriterias) && filters.filterCriterias.length > 0) {
        convertedFilters = filters.filterCriterias.map((filter: any) => {
          // Map operator to FilterType enum
          let filterType = 1; // Default to Equal
          switch (filter.operator?.toLowerCase()) {
            case 'equals':
            case 'equal':
              filterType = 1; // Equal
              break;
            case 'contains':
              filterType = 10; // Contains
              break;
            default:
              filterType = 1; // Default to Equal
          }

          // Map field names to actual database column names
          let fieldName = filter.fieldName || filter.name;
          
          // Direct mapping for UserJourney table columns
          const fieldNameMap: Record<string, string> = {
            'BeatHistoryUID': 'beat_history_uid',
            'beatHistoryUID': 'beat_history_uid',
            'JobPositionUID': 'job_position_uid',
            'jobPositionUID': 'job_position_uid',
            'EmpUID': 'emp_uid',
            'empUID': 'emp_uid',
            'RouteUID': 'route_uid',
            'routeUID': 'route_uid',
            'VehicleUID': 'vehicle_uid',
            'vehicleUID': 'vehicle_uid',
            'UserJourneyUID': 'user_journey_uid',
            'userJourneyUID': 'user_journey_uid',
            'LoginId': 'login_id',
            'loginId': 'login_id',
            'VisitDate': 'visit_date',
            'visitDate': 'visit_date'
          };
          
          // Use mapped name if available, otherwise use original
          const mappedFieldName = fieldNameMap[fieldName] || fieldName;
          
          return {
            name: mappedFieldName,
            value: filter.value,
            type: filterType
          };
        });
      }
      
      const response = await apiService.post('/UserJourney/SelectAlUserJourneyDetails', {
        pageNumber: filters.pageNumber || 1,
        pageSize: filters.pageSize || 100,
        isCountRequired: true,
        filterCriterias: convertedFilters
      });
      return response;
    } catch (error) {
      console.error('Error fetching user journeys:', error);
      return { Data: { PagedData: [], TotalCount: 0 }, IsSuccess: false };
    }
  }

  async getUserJourneyGrid(filters: any = {}) {
    try {
      const response = await apiService.post('/UserJourney/GetUserJourneyGridDetails', {
        pageNumber: filters.pageNumber || 1,
        pageSize: filters.pageSize || 50,
        isCountRequired: true,
        filterCriterias: filters.filterCriterias || []
      });
      return response.data;
    } catch (error) {
      console.error('Error fetching user journey grid:', error);
      throw error;
    }
  }

  async getTodayJourneyPlan(type: string, visitDate: string, filters: any = {}) {
    try {
      const response = await apiService.post(
        `/UserJourney/SelectTodayJourneyPlanDetails?Type=${type}&VisitDate=${visitDate}&JobPositionUID=${filters.jobPositionUID || ''}&OrgUID=${filters.orgUID || ''}`,
        {
          pageNumber: filters.pageNumber || 1,
          pageSize: filters.pageSize || 50,
          isCountRequired: true,
          filterCriterias: filters.filterCriterias || []
        }
      );
      return response.data;
    } catch (error) {
      console.error('Error fetching today journey plan:', error);
      throw error;
    }
  }

  // Get beat history count for a specific route (lightweight call)
  async getBeatHistoryCountByRoute(routeUID: string) {
    try {
      console.log('🔢 SERVICE: getBeatHistoryCountByRoute called with:', { routeUID });
      
      // Call the API with minimal page size to get just the count
      const response = await apiService.get(`/BeatHistory/GetAllBeatHistoriesByRouteUID?RouteUID=${encodeURIComponent(routeUID)}&pageNumber=1&pageSize=1`);
      
      console.log('🔢 SERVICE: Beat history count response:', response);
      
      // Return just the count
      if (response?.Data?.TotalCount !== undefined) {
        return response.Data.TotalCount;
      } else if (response?.Data?.PagedData) {
        return response.Data.PagedData.length;
      } else if (Array.isArray(response?.Data)) {
        return response.Data.length;
      }
      
      return 0;
    } catch (error: any) {
      console.error('🔢 SERVICE: Error fetching beat history count:', error);
      return 0;
    }
  }

  // Get all beat histories for a specific route
  async getBeatHistoriesByRoute(routeUID: string, pageNumber?: number, pageSize?: number) {
    try {
      console.log('🎯 SERVICE: getBeatHistoriesByRoute called with:', { routeUID, pageNumber, pageSize });
      
      // Build query parameters
      let queryParams = `RouteUID=${encodeURIComponent(routeUID)}`;
      if (pageNumber && pageSize) {
        queryParams += `&pageNumber=${pageNumber}&pageSize=${pageSize}`;
      }
      
      // Use GET endpoint with query parameters
      const response = await apiService.get(`/BeatHistory/GetAllBeatHistoriesByRouteUID?${queryParams}`);
      
      console.log('🎯 SERVICE: Beat histories response:', response);
      
      // If response is already in the correct format, return it
      if (response?.Data?.PagedData || Array.isArray(response?.Data)) {
        return response;
      }
      
      // If response is a direct array, wrap it
      if (Array.isArray(response)) {
        return {
          Data: {
            PagedData: response,
            TotalCount: response.length
          },
          IsSuccess: true,
          StatusCode: 200
        };
      }
      
      return response;
    } catch (error) {
      console.error('❌ SERVICE: Error loading beat histories by route:', error);
      throw error;
    }
  }

  // Beat History APIs
  async getBeatHistories(filters: any = {}) {
    debugger; // SERVICE DEBUGGER 1: Start of getBeatHistories
    
    try {
      console.log('🎯 SERVICE: getBeatHistories called with filters:', filters);
      
      // Convert frontend filter format to backend FilterCriteria format
      let convertedFilters: any[] = [];
      
      if (Array.isArray(filters.filterCriterias) && filters.filterCriterias.length > 0) {
        console.log('🎯 SERVICE: Converting filters...');
        
        convertedFilters = filters.filterCriterias.map((filter: any) => {
          console.log('🎯 SERVICE: Processing filter:', filter);
          
          // Map operator to FilterType enum
          let filterType = 1; // Default to Equal
          switch (filter.operator?.toLowerCase()) {
            case 'equals':
            case 'equal':
              filterType = 1; // Equal
              break;
            case 'notequal':
            case 'notequals':
              filterType = 0; // NotEqual
              break;
            case 'like':
              filterType = 2; // Like
              break;
            case 'contains':
              filterType = 10; // Contains
              break;
            case 'in':
              filterType = 4; // In
              break;
            case 'between':
              filterType = 5; // Between
              break;
            case 'lessthan':
              filterType = 8; // LessThan
              break;
            case 'greaterthan':
              filterType = 9; // GreaterThan
              break;
            case 'lessthanorequal':
              filterType = 6; // LessThanOrEqual
              break;
            case 'greaterthanorequal':
              filterType = 7; // GreaterThanOrEqual
              break;
            default:
              filterType = 1; // Default to Equal
          }

          const converted = {
            name: filter.fieldName || filter.name,
            value: filter.value,
            type: filterType
          };
          
          console.log('🎯 SERVICE: Converted filter:', converted);
          return converted;
        });
      }

      // Ensure proper request structure to avoid backend null references
      const requestData = {
        pageNumber: Math.max(1, filters.pageNumber || 1), // Ensure pageNumber >= 1
        pageSize: Math.max(1, filters.pageSize || 50), // Ensure pageSize >= 1
        isCountRequired: true,
        filterCriterias: convertedFilters
      };
      
      console.log('🎯 SERVICE: Final request data:', JSON.stringify(requestData, null, 2));
      
      debugger; // SERVICE DEBUGGER 2: Before API call
      
      // Add a small delay to avoid overwhelming the backend
      await new Promise(resolve => setTimeout(resolve, 100));
      
      const response = await apiService.post('/BeatHistory/SelectAllJobPositionDetails', requestData);
      
      debugger; // SERVICE DEBUGGER 3: After API call
      
      console.log('🎯 SERVICE: API Response received:', response);
      console.log('🎯 SERVICE: Response details:', {
        status: response?.status,
        isSuccess: response?.IsSuccess,
        dataCount: response?.Data?.TotalCount,
        hasData: !!response?.Data,
        hasPagedData: !!response?.Data?.PagedData,
        pagedDataLength: response?.Data?.PagedData?.length
      });
      
      // Validate response structure
      if (response && response.IsSuccess && response.Data) {
        console.log('✅ SERVICE: Returning successful response');
        return response;
      } else {
        console.warn('⚠️ SERVICE: Invalid BeatHistories response structure');
        return { Data: { PagedData: [], TotalCount: 0 }, IsSuccess: false };
      }
      
    } catch (error: any) {
      console.error('❌ SERVICE: BeatHistories Error:', error);
      console.error('❌ SERVICE: Error details:', {
        message: error.message,
        status: error.status,
        details: error.details,
        endpoint: error.endpoint,
        stack: error.stack
      });
      
      debugger; // SERVICE DEBUGGER 4: Error occurred
      
      // If it's a backend null reference error, try without filters
      if (error.message?.includes('Object reference not set')) {
        console.log('🔄 SERVICE: Retrying BeatHistories without filters due to null reference...');
        try {
          const fallbackData = {
            pageNumber: 1,
            pageSize: 20,
            isCountRequired: true,
            filterCriterias: []
          };
          
          const fallbackResponse = await apiService.post('/BeatHistory/SelectAllJobPositionDetails', fallbackData);
          console.log('✅ SERVICE: Fallback request succeeded');
          return fallbackResponse;
        } catch (fallbackError: any) {
          console.error('❌ SERVICE: Fallback request also failed:', fallbackError.message);
        }
      }
      
      return { Data: { PagedData: [], TotalCount: 0 }, IsSuccess: false };
    }
  }

  async getBeatHistoryByUID(uid: string) {
    try {
      const response = await apiService.get(`/BeatHistory/GetBeatHistoryByUID?UID=${uid}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching beat history by UID:', error);
      throw error;
    }
  }

  // Store History APIs
  async getStoreHistory(routeUID: string, visitDate: string, storeUID: string) {
    try {
      const response = await apiService.get(
        `/StoreHistory/GetStoreHistoryByRouteUIDVisitDateAndStoreUID?routeUID=${routeUID}&visitDate=${visitDate}&storeUID=${storeUID}`
      );
      return response.data;
    } catch (error) {
      console.error('Error fetching store history:', error);
      throw error;
    }
  }

  // Get store histories by user journey UID - using the new endpoint
  async getStoreHistoriesByUserJourneyUID(userJourneyUID: string) {
    try {
      console.log(`🏪 SERVICE: Loading store histories for user journey ${userJourneyUID}...`);
      
      // Use the new custom endpoint we added to the backend
      const response = await apiService.get(`/BeatHistory/GetStoreHistoriesByUserJourneyUID?userJourneyUID=${userJourneyUID}`);
      
      console.log(`🏪 SERVICE: Store history response:`, response);
      
      let storeHistories = [];
      if (response?.Data) {
        storeHistories = Array.isArray(response.Data) ? response.Data : [response.Data];
      } else if (Array.isArray(response)) {
        storeHistories = response;
      }
      
      console.log(`✅ Loaded ${storeHistories.length} store histories for user journey ${userJourneyUID}`);
      return storeHistories;
    } catch (error) {
      console.error('❌ Error loading store histories by user journey:', error);
      return [];
    }
  }

  // Get store histories by beat history UID (alternative method)
  async getStoreHistoriesByBeatHistoryUID(beatHistoryUID: string) {
    try {
      console.log(`🏪 SERVICE: Loading store histories for beat history ${beatHistoryUID}...`);
      
      // This endpoint also doesn't exist in the current backend implementation
      console.warn('🏪 SERVICE: StoreHistory endpoints not yet implemented in backend');
      console.log('🏪 SERVICE: Data exists in store_history table but no API endpoint available');
      
      // Return empty array - no mock data
      return [];
    } catch (error) {
      console.error('❌ Error loading store histories by beat history:', error);
      return [];
    }
  }

  // Combined Dashboard Data
  async getDashboardData(filters: SalesTeamActivityFilters) {
    try {
      // Fetch all data in parallel - Routes first, then related data
      const [routes, journeys, beatHistories] = await Promise.all([
        this.getRoutes({
          filterCriterias: filters.routeUID ? [{ fieldName: 'UID', operator: 'Equals', value: filters.routeUID }] : [],
          pageNumber: 1,
          pageSize: 100
        }),
        this.getUserJourneys({
          filterCriterias: this.buildFilterCriteria(filters),
          pageNumber: 1,
          pageSize: 100
        }),
        this.getBeatHistories({
          filterCriterias: this.buildFilterCriteria(filters),
          pageNumber: 1,
          pageSize: 100
        })
      ]);

      // Safely access Data property from API responses
      const routeData = routes?.Data || { PagedData: [], TotalCount: 0 };
      const journeyData = journeys?.Data || { PagedData: [], TotalCount: 0 };
      const beatData = beatHistories?.Data || { PagedData: [], TotalCount: 0 };

      // Calculate metrics
      const metrics = this.calculateDashboardMetrics(journeyData, beatData);
      const routePerformance = this.calculateRoutePerformanceWithDetails(beatData, routeData);
      
      return {
        metrics,
        routePerformance,
        routes: routeData,
        journeys: journeyData,
        beatHistories: beatData
      };
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
      // Return default structure on error
      return {
        metrics: {
          totalJourneys: 0,
          activeRoutes: 0,
          totalStoresVisited: 0,
          productiveStores: 0,
          averageCoverage: 0,
          totalSalesValue: 0,
          averageVisitDuration: 0,
          skippedStores: 0
        },
        routePerformance: [],
        routes: { PagedData: [], TotalCount: 0 },
        journeys: { PagedData: [], TotalCount: 0 },
        beatHistories: { PagedData: [], TotalCount: 0 }
      };
    }
  }

  // Helper methods
  private buildFilterCriteria(filters: SalesTeamActivityFilters) {
    const criteria = [];
    
    if (filters.startDate && filters.endDate) {
      criteria.push({
        fieldName: 'VisitDate',
        operator: 'Between',
        value: filters.startDate,
        value2: filters.endDate
      });
    }
    
    if (filters.routeUID) {
      criteria.push({
        fieldName: 'RouteUID',
        operator: 'Equals',
        value: filters.routeUID
      });
    }
    
    if (filters.jobPositionUID) {
      criteria.push({
        fieldName: 'JobPositionUID',
        operator: 'Equals',
        value: filters.jobPositionUID
      });
    }
    
    return criteria;
  }

  private calculateDashboardMetrics(journeys: any, beatHistories: any): DashboardMetrics {
    const journeyData = journeys?.PagedData || [];
    const beatData = beatHistories?.PagedData || [];

    return {
      totalJourneys: journeys?.TotalCount || 0,
      activeRoutes: new Set(beatData.map((b: any) => b.RouteUID)).size,
      totalStoresVisited: beatData.reduce((sum: number, b: any) => sum + (b.ActualStoreVisits || 0), 0),
      productiveStores: beatData.reduce((sum: number, b: any) => sum + (b.ZeroSalesStoreVisits || 0), 0),
      averageCoverage: beatData.length > 0 
        ? beatData.reduce((sum: number, b: any) => sum + (b.Coverage || 0), 0) / beatData.length 
        : 0,
      totalSalesValue: 0, // This would come from store history data
      averageVisitDuration: 0, // Calculate from store history
      skippedStores: beatData.reduce((sum: number, b: any) => sum + (b.SkippedStoreVisits || 0), 0)
    };
  }

  private calculateRoutePerformance(beatHistories: any): RoutePerformance[] {
    const beatData = beatHistories?.PagedData || [];
    const routeMap = new Map<string, RoutePerformance>();

    beatData.forEach((beat: any) => {
      const routeUID = beat.RouteUID;
      if (!routeMap.has(routeUID)) {
        routeMap.set(routeUID, {
          routeUID,
          routeName: routeUID, // Would need to fetch route name from route API
          totalVisits: 0,
          plannedVisits: 0,
          actualVisits: 0,
          coverage: 0,
          salesValue: 0,
          productivityRate: 0
        });
      }

      const route = routeMap.get(routeUID)!;
      route.totalVisits += 1;
      route.plannedVisits += beat.PlannedStoreVisits || 0;
      route.actualVisits += beat.ActualStoreVisits || 0;
      route.coverage = route.plannedVisits > 0 
        ? (route.actualVisits / route.plannedVisits) * 100 
        : 0;
    });

    return Array.from(routeMap.values());
  }

  private calculateRoutePerformanceWithDetails(beatHistories: any, routes: any): RoutePerformance[] {
    const beatData = beatHistories?.PagedData || [];
    const routeData = routes?.PagedData || [];
    const routeMap = new Map<string, RoutePerformance>();

    // Create a map for quick route lookups
    const routeLookup = new Map<string, any>();
    routeData.forEach((route: any) => {
      routeLookup.set(route.UID, route);
    });

    beatData.forEach((beat: any) => {
      const routeUID = beat.RouteUID;
      if (!routeMap.has(routeUID)) {
        const routeDetails = routeLookup.get(routeUID);
        routeMap.set(routeUID, {
          routeUID,
          routeName: routeDetails ? `${routeDetails.Code} - ${routeDetails.Name}` : routeUID,
          totalVisits: 0,
          plannedVisits: 0,
          actualVisits: 0,
          coverage: 0,
          salesValue: 0,
          productivityRate: 0
        });
      }

      const route = routeMap.get(routeUID)!;
      route.totalVisits += 1;
      route.plannedVisits += beat.PlannedStoreVisits || 0;
      route.actualVisits += beat.ActualStoreVisits || 0;
      route.coverage = route.plannedVisits > 0 
        ? (route.actualVisits / route.plannedVisits) * 100 
        : 0;
      route.productivityRate = route.plannedVisits > 0
        ? ((route.actualVisits - (beat.ZeroSalesStoreVisits || 0)) / route.plannedVisits) * 100
        : 0;
    });

    return Array.from(routeMap.values());
  }

  // Export functionality
  async exportToExcel(data: any, filename: string = 'sales-team-activity-report') {
    // Implementation for Excel export
    const worksheet = this.convertToExcelFormat(data);
    // Use a library like xlsx to generate the file
    console.log('Exporting to Excel:', filename);
  }

  private convertToExcelFormat(data: any) {
    // Convert data to Excel-friendly format
    return data;
  }
}

export default new SalesTeamActivityService();