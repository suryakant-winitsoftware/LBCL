import { apiService } from './api';

export interface Route {
  UID: string;
  Code: string;
  Name: string;
  CompanyUID?: string; // Added based on RouteChangeLog model
  OrgUID: string;
  WHOrgUID?: string;
  RoleUID: string;
  JobPositionUID?: string;
  VehicleUID?: string;
  LocationUID?: string;
  IsActive: boolean;
  Status: string;
  ValidFrom: string;
  ValidUpto: string;
  VisitTime?: string;
  EndTime?: string;
  VisitDuration: number;
  TravelTime: number;
  TotalCustomers: number;
  IsCustomerWithTime: boolean;
  PrintStanding: boolean;
  PrintForward: boolean;
  PrintTopup: boolean;
  PrintOrderSummary: boolean;
  AutoFreezeJP: boolean;
  AddToRun: boolean;
  AutoFreezeRunTime?: string;
  User?: string; // Backend required field
  IsChangeApplied?: boolean; // From RouteChangeLog
  CreatedBy: string;
  ModifiedBy: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  SS?: number; // Sync status
}

export interface RouteSchedule {
  UID: string;
  CompanyUID: string;
  RouteUID: string;
  Name: string;
  Type: string;
  StartDate: string;
  Status: number;
  FromDate: string;
  ToDate: string;
  StartTime: string;
  EndTime: string;
  VisitDurationInMinutes: number;
  TravelTimeInMinutes: number;
  NextBeatDate: string;
  LastBeatDate: string;
  AllowMultipleBeatsPerDay: boolean;
  PlannedDays: string;
  SS?: number | null; // Backend sync status field
  CreatedBy: string;
  ModifiedBy: string;
}

export interface RouteCustomer {
  UID: string;
  RouteUID: string;
  StoreUID: string; // Database uses store_uid
  SeqNo: number; // Database uses seq_no
  VisitTime: null | string;
  VisitDuration: number;
  EndTime: null | string;
  IsDeleted: boolean;
  IsBillToCustomer?: boolean; // Indicates if this is the billing customer
  TravelTime: number;
  ActionType: string;
  CreatedBy: string;
  CreatedTime: undefined | string;
  ModifiedBy: string;
  ModifiedTime: undefined | string;
  ServerAddTime: undefined | string;
  ServerModifiedTime: undefined | string;
}

export interface RouteUser {
  UID?: string;
  RouteUID: string;
  JobPositionUID: string;
  FromDate?: Date | string;
  ToDate?: Date | string;
  IsActive?: boolean;
  ActionType?: string;
}

export interface RouteMaster {
  Route: Route; // Should be RouteChangeLog according to backend
  RouteSchedule: RouteSchedule;
  RouteScheduleDaywise?: any;
  RouteScheduleFortnight?: any;
  RouteCustomersList: RouteCustomer[];
  RouteUserList?: RouteUser[]; // Multiple employees assigned to route
}

export interface PagedRequest {
  pageNumber: number;
  pageSize: number;
  isCountRequired: boolean;
  sortCriterias: any[];
  filterCriterias: any[];
}

export interface PagedResponse<T> {
  IsSuccess: boolean;
  Data: {
    PagedData: T[];
    TotalCount: number;
  };
  Message?: string;
}

class RouteService {
  async getRoutes(request: PagedRequest, orgUID?: string): Promise<PagedResponse<Route>> {
    // Use SelectRouteChangeLogAllDetails API for getting routes (as specified in requirements)
    // This endpoint returns the edited data for current day and night from RouteChangeLog table
    const response = await apiService.post<PagedResponse<Route>>('/Route/SelectRouteChangeLogAllDetails', request);
    
    // Deduplicate routes based on UID (keep the most recently modified one)
    if (response.IsSuccess && response.Data?.PagedData) {
      const uniqueRoutes = this.deduplicateRoutes(response.Data.PagedData);
      response.Data.PagedData = uniqueRoutes;
      response.Data.TotalCount = uniqueRoutes.length;
    }
    
    return response;
  }
  
  private deduplicateRoutes(routes: Route[]): Route[] {
    const routeMap = new Map<string, Route>();
    
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
  
  async getRouteDetails(request: PagedRequest, orgUID?: string): Promise<PagedResponse<Route>> {
    // Alternative endpoint for regular route details if needed
    const url = `/Route/SelectAllRouteDetails?OrgUID=${encodeURIComponent(orgUID || '')}`;
    return apiService.post(url, request);
  }

  async getRouteById(uid: string): Promise<any> {
    return apiService.get(`/Route/SelectRouteMasterViewByUID?UID=${uid}`);
  }

  async getRouteDetailsById(uid: string): Promise<any> {
    return apiService.get(`/Route/SelectRouteDetailByUID?UID=${uid}`);
  }

  async getRouteScheduleMappings(routeScheduleUID: string): Promise<any> {
    // Fetch route schedule customer mappings
    return apiService.get(`/Route/GetRouteScheduleCustomerMappings?RouteScheduleUID=${routeScheduleUID}`);
  }

  async getRouteScheduleConfigs(routeScheduleUID: string): Promise<any> {
    // Fetch route schedule configs
    return apiService.get(`/Route/GetRouteScheduleConfigs?RouteScheduleUID=${routeScheduleUID}`);
  }

  async createRoute(routeData: RouteMaster): Promise<any> {
    return apiService.post('/Route/CreateRouteMaster', routeData);
  }

  async updateRoute(routeData: RouteMaster): Promise<any> {
    return apiService.put('/Route/UpdateRouteMaster', routeData);
  }

  // Remove soft delete - we only do cascade delete now
  
  async deleteRoute(uid: string): Promise<any> {
    try {
      // This now does CASCADE DELETE - removes everything
      const response = await apiService.delete<any>(`/Route/DeleteRouteDetail?UID=${uid}`);
      
      if (!response.IsSuccess) {
        const errorMessage = response.Message || response.ErrorMessage || 'Failed to delete route';
        throw new Error(errorMessage);
      }
      
      return response;
    } catch (error: any) {
      // Re-throw with enhanced error message
      if (error.message) {
        throw error;
      }
      throw new Error('Failed to delete route. Please try again later.');
    }
  }
  
  async cascadeDeleteRoute(uid: string): Promise<any> {
    // Same as deleteRoute since backend now does cascade delete
    return this.deleteRoute(uid);
  }

  async getRouteDropdown(orgUID: string): Promise<any> {
    return apiService.post(`/Dropdown/GetRouteDropDown?orgUID=${orgUID}`);
  }

  // Route creation helpers
  generateRouteUID(): string {
    // More unique UID generation with better randomness
    const timestamp = Date.now();
    const random1 = Math.random().toString(36).substring(2, 7);
    const random2 = Math.random().toString(36).substring(2, 7);
    return `RT${timestamp}${random1}${random2}`.toUpperCase();
  }

  generateScheduleUID(): string {
    // More unique UID generation with better randomness
    const timestamp = Date.now();
    const random1 = Math.random().toString(36).substring(2, 7);
    const random2 = Math.random().toString(36).substring(2, 7);
    return `SCH${timestamp}${random1}${random2}`.toUpperCase();
  }

  generateDaywiseUID(): string {
    return `daywise-${Date.now()}-${Math.random().toString(36).substring(2, 11)}`;
  }

  generateFortnightUID(): string {
    return `fortnight-${Date.now()}-${Math.random().toString(36).substring(2, 11)}`;
  }

  async exportRoutes(
    format: "csv" | "excel",
    filters?: { search?: string }
  ): Promise<Blob> {
    // Build paging request to get all routes (up to 10,000)
    const pagingRequest: PagedRequest = {
      pageNumber: 1,
      pageSize: 10000,
      isCountRequired: false,
      sortCriterias: [],
      filterCriterias: []
    };

    // Add search filter if provided
    if (filters?.search && filters.search.trim() !== '') {
      pagingRequest.filterCriterias.push({
        Name: "Name", // Search by route name
        Value: filters.search,
        Type: 6, // Contains
        DataType: "System.String"
      });
    }

    const response = await this.getRoutes(pagingRequest);
    
    if (!response.IsSuccess || !response.Data?.PagedData) {
      throw new Error('Failed to fetch routes for export');
    }

    if (format === "csv") {
      return this.exportToCSV(response.Data.PagedData);
    } else {
      return this.exportToExcel(response.Data.PagedData);
    }
  }

  private exportToCSV(routes: Route[]): Blob {
    const headers = [
      "Code",
      "Name", 
      "Organization UID",
      "Role UID",
      "Status",
      "Valid From",
      "Valid To",
      "Visit Duration (min)",
      "Travel Time (min)",
      "Total Customers",
      "Is Active",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date"
    ];

    const csvContent = [
      headers.join(","),
      ...routes.map(route => [
        `"${route.Code || ''}"`,
        `"${route.Name || ''}"`,
        `"${route.OrgUID || ''}"`,
        `"${route.RoleUID || ''}"`,
        `"${route.Status || ''}"`,
        `"${route.ValidFrom ? new Date(route.ValidFrom).toLocaleDateString() : ''}"`,
        `"${route.ValidUpto ? new Date(route.ValidUpto).toLocaleDateString() : ''}"`,
        `"${route.VisitDuration || 0}"`,
        `"${route.TravelTime || 0}"`,
        `"${route.TotalCustomers || 0}"`,
        `"${route.IsActive ? 'Active' : 'Inactive'}"`,
        `"${route.CreatedBy || ''}"`,
        `"${route.CreatedTime ? new Date(route.CreatedTime).toLocaleDateString() : ''}"`,
        `"${route.ModifiedBy || ''}"`,
        `"${route.ModifiedTime ? new Date(route.ModifiedTime).toLocaleDateString() : ''}"`
      ].join(","))
    ].join("\n");

    return new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
  }

  private exportToExcel(routes: Route[]): Blob {
    // For now, return CSV format with Excel MIME type
    // In the future, could use libraries like xlsx for proper Excel export
    const csvContent = this.exportToCSV(routes);
    return new Blob([csvContent], { 
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;' 
    });
  }
}

export const routeService = new RouteService();

export type DeleteRouteOptions = {
  force?: boolean; // If true, attempt hard delete; if false or undefined, soft delete
};