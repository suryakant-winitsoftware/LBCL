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
}

export const routeService = new RouteService();

export type DeleteRouteOptions = {
  force?: boolean; // If true, attempt hard delete; if false or undefined, soft delete
};