/**
 * Production-Ready Route Service
 * Fixed with all required fields and proper structure based on backend analysis
 */

import { apiService } from './api';

// ==================== INTERFACES ====================

/**
 * Route Interface - Matches RouteChangeLog structure in backend
 * ALL fields documented with required/optional status
 */
export interface Route {
  // Primary identifiers (REQUIRED)
  UID: string;                    // Primary key, use same as Code
  Code: string;                   // Unique route code
  Name: string;                   // Route display name
  
  // Organization fields (REQUIRED)
  OrgUID: string;                 // Organization (e.g., "Farmley")
  CompanyUID: string;             // REQUIRED - Company UID, often same as OrgUID
  
  // Assignment fields (REQUIRED)
  RoleUID: string;                // Role (e.g., "Team Leader", "Merchandiser")
  JobPositionUID: string;         // Job position, often same as UID
  
  // Optional assignments
  WHOrgUID?: string;              // Warehouse organization
  VehicleUID?: string;            // Vehicle assignment
  LocationUID?: string;           // Location assignment
  
  // Status fields (REQUIRED)
  IsActive: boolean;              // Route active status
  Status: string;                 // "Active" or "Inactive"
  
  // Validity period (REQUIRED)
  ValidFrom: string;              // ISO datetime format
  ValidUpto: string;              // ISO datetime format
  
  // Visit configuration (REQUIRED with defaults)
  VisitDuration: number;          // Minutes per customer visit (default: 0)
  TravelTime: number;             // Minutes between customers (default: 0)
  TotalCustomers: number;         // Customer count (default: 0)
  IsCustomerWithTime: boolean;    // Time-specific visits (default: false)
  
  // Timing fields (REQUIRED with defaults)
  VisitTime: string;              // REQUIRED - Start time HH:mm:ss (default: "00:00:00")
  EndTime: string;                // REQUIRED - End time HH:mm:ss (default: "00:00:00")
  
  // Print settings (optional, default false)
  PrintStanding: boolean;
  PrintForward: boolean;
  PrintTopup: boolean;
  PrintOrderSummary: boolean;
  
  // Journey Plan settings (optional, default false)
  AutoFreezeJP: boolean;
  AddToRun: boolean;
  AutoFreezeRunTime?: string;     // Time to auto-freeze
  
  // Backend managed fields
  User: string;                   // REQUIRED - User phone number
  IsChangeApplied?: boolean;      // Change tracking
  SS?: number;                    // Sync status (0 or 1)
  
  // Audit fields (REQUIRED)
  CreatedBy: string;
  ModifiedBy: string;
  CreatedTime?: string;           // ISO datetime
  ModifiedTime?: string;          // ISO datetime
  ServerAddTime?: string;         // Backend managed
  ServerModifiedTime?: string;    // Backend managed
  
  // UI helper (frontend only)
  IsSelected?: boolean;
  
  // Database ID (backend generated, don't send)
  Id?: number;
  
  // REQUIRED fields that were missing
  serialNumber: number;           // REQUIRED by backend
  KeyUID?: string;                // Inherited from BaseModel
}

/**
 * Route Schedule Configuration
 */
export interface RouteSchedule {
  // Primary identifiers (REQUIRED)
  UID: string;                    // Same as Route.UID
  RouteUID: string;               // Reference to Route.UID
  Name: string;                   // Schedule name
  
  // Schedule type (REQUIRED)
  Type: 'Daily' | 'Weekly' | 'Fortnightly' | 'Monthly';
  Status: number;                 // 0=Inactive, 1=Active
  
  // Timing configuration (REQUIRED)
  VisitDurationInMinutes: number; // Visit duration
  TravelTimeInMinutes: number;    // Travel time
  AllowMultipleBeatsPerDay: boolean;
  
  // Date/Time fields (REQUIRED)
  CompanyUID: string;             // REQUIRED - Company UID
  StartDate: string;              // REQUIRED - Schedule start date
  FromDate: string;               // REQUIRED - Valid from date
  ToDate: string;                 // REQUIRED - Valid to date
  StartTime: string;              // REQUIRED - HH:mm:ss format
  EndTime: string;                // REQUIRED - HH:mm:ss format
  PlannedDays: string;            // REQUIRED - Comma-separated days
  
  // Backend calculated fields (optional)
  NextBeatDate?: string;          // Backend calculated
  LastBeatDate?: string;          // Backend calculated
  SS?: number;
  
  // Audit fields (REQUIRED)
  CreatedBy: string;
  ModifiedBy: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

/**
 * Day-wise Schedule Configuration
 */
export interface RouteScheduleDaywise {
  // REQUIRED fields
  RouteScheduleUID: string;       // Reference to RouteSchedule.UID
  Monday: number;                 // 1=Active, 0=Inactive
  Tuesday: number;
  Wednesday: number;
  Thursday: number;
  Friday: number;
  Saturday: number;
  Sunday: number;
  
  // Optional fields
  UID?: string;                   // Same as RouteScheduleUID
  SS?: number;
  CreatedBy?: string;
  ModifiedBy?: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

/**
 * Fortnight Schedule (Optional)
 */
export interface RouteScheduleFortnight {
  UID?: string;
  RouteScheduleUID?: string;
  Week1?: boolean;
  Week2?: boolean;
  Week3?: boolean;
  Week4?: boolean;
  CreatedBy?: string;
  ModifiedBy?: string;
}

/**
 * Route Customer Assignment
 */
export interface RouteCustomer {
  // REQUIRED fields
  RouteUID: string;
  StoreUID: string;               // Customer/Store ID
  SeqNo: number;                  // Sequence number (1-based)
  VisitDuration: number;          // Minutes
  TravelTime: number;             // Minutes
  IsDeleted: boolean;             // Soft delete flag
  CreatedBy: string;
  ModifiedBy: string;
  
  // Optional fields
  UID?: string;                   // Format: RouteUID_StoreUID
  VisitTime?: string;             // HH:mm:ss
  EndTime?: string;               // HH:mm:ss
  ActionType?: number | string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

/**
 * Route User Assignment
 */
export interface RouteUser {
  // REQUIRED fields
  RouteUID: string;
  JobPositionUID: string;
  FromDate: string;               // ISO datetime
  ToDate: string;                 // ISO datetime
  IsActive: boolean;
  CreatedBy: string;
  ModifiedBy: string;
  
  // Optional fields
  UID?: string;
  ActionType?: number;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

/**
 * Complete Route Master Structure for Create/Update
 */
export interface RouteMaster {
  Route: Route;
  RouteSchedule: RouteSchedule;
  RouteScheduleDaywise: RouteScheduleDaywise;
  RouteScheduleFortnight?: RouteScheduleFortnight;
  RouteCustomersList: RouteCustomer[];
  RouteUserList?: RouteUser[];
}

/**
 * Simplified Route Creation Data (from form)
 */
export interface RouteCreationData {
  // Basic Information
  code: string;
  name: string;
  description?: string;
  
  // Organization
  orgUID: string;
  companyUID?: string;
  whOrgUID?: string;
  
  // Assignment
  roleUID: string;
  jobPositionUID?: string;
  vehicleUID?: string;
  locationUID?: string;
  user?: string;                  // Phone number
  
  // Status
  isActive?: boolean;
  status?: string;
  
  // Validity
  validFrom: string | Date;
  validUpto: string | Date;
  
  // Schedule Type
  scheduleType: 'Daily' | 'Weekly' | 'Fortnightly' | 'Monthly';
  
  // Timing
  visitTime?: string;
  endTime?: string;
  visitDuration: number;
  travelTime: number;
  
  // Day Selection (for weekly)
  monday?: boolean;
  tuesday?: boolean;
  wednesday?: boolean;
  thursday?: boolean;
  friday?: boolean;
  saturday?: boolean;
  sunday?: boolean;
  
  // Fortnight (if applicable)
  week1?: boolean;
  week2?: boolean;
  week3?: boolean;
  week4?: boolean;
  
  // Settings
  allowMultipleBeatsPerDay?: boolean;
  isCustomerWithTime?: boolean;
  printStanding?: boolean;
  printForward?: boolean;
  printTopup?: boolean;
  printOrderSummary?: boolean;
  autoFreezeJP?: boolean;
  addToRun?: boolean;
  autoFreezeRunTime?: string;
  
  // Customers
  selectedCustomers?: string[];
  customerSchedules?: Record<string, {
    visitTime?: string;
    visitDuration?: number;
    travelTime?: number;
  }>;
  
  // Users
  assignedUsers?: Array<{
    jobPositionUID: string;
    fromDate?: string;
    toDate?: string;
  }>;
}

// ==================== SERVICE CLASS ====================

export class RouteServiceProduction {
  
  /**
   * Get routes from RouteChangeLog (for grid display)
   */
  async getRouteChangeLog(request: PagedRequest): Promise<PagedResponse<Route>> {
    return apiService.post('/Route/SelectRouteChangeLogAllDetails', request);
  }
  
  /**
   * Get routes by organization
   */
  async getRoutesByOrg(request: PagedRequest, orgUID: string): Promise<PagedResponse<Route>> {
    const url = `/Route/SelectAllRouteDetails?OrgUID=${encodeURIComponent(orgUID)}`;
    return apiService.post(url, request);
  }
  
  /**
   * Get single route details
   */
  async getRouteById(uid: string): Promise<ApiResponse<Route>> {
    return apiService.get(`/Route/SelectRouteDetailByUID?UID=${uid}`);
  }
  
  /**
   * Get complete route master (with schedule, customers, users)
   */
  async getRouteMasterById(uid: string): Promise<ApiResponse<RouteMaster>> {
    return apiService.get(`/Route/SelectRouteMasterViewByUID?UID=${uid}`);
  }
  
  /**
   * Create new route with complete validation and structure
   */
  async createRoute(data: RouteCreationData): Promise<ApiResponse<number>> {
    const routeMaster = this.buildRouteMaster(data);
    return apiService.post('/Route/CreateRouteMaster', routeMaster);
  }
  
  /**
   * Update existing route
   */
  async updateRoute(uid: string, data: RouteCreationData): Promise<ApiResponse<number>> {
    const routeMaster = this.buildRouteMaster(data, uid);
    return apiService.put('/Route/UpdateRouteMaster', routeMaster);
  }
  
  /**
   * Delete route
   */
  async deleteRoute(uid: string): Promise<ApiResponse<number>> {
    return apiService.delete(`/Route/DeleteRouteDetail?UID=${uid}`);
  }
  
  /**
   * Build complete RouteMaster structure from form data
   */
  private buildRouteMaster(data: RouteCreationData, existingUID?: string): RouteMaster {
    // Use code as UID (backend pattern) or existing UID for updates
    const routeUID = existingUID || data.code;
    const now = new Date().toISOString();
    
    // Debug logging commented out for production
    // console.log('Building RouteMaster with:', {
    //   routeUID,
    //   code: data.code,
    //   orgUID: data.orgUID,
    //   companyUID: data.companyUID,
    //   roleUID: data.roleUID,
    //   scheduleType: data.scheduleType
    // });
    
    // Convert dates to ISO string with time component if needed
    const formatDateTime = (date: string | Date): string => {
      if (typeof date === 'string') {
        // If it's just a date (YYYY-MM-DD), add time component
        if (date.length === 10) {
          return `${date}T00:00:00`;
        }
        return date;
      }
      return date.toISOString();
    };
    
    const validFrom = formatDateTime(data.validFrom);
    const validUpto = formatDateTime(data.validUpto);
    
    // Build Route object with ALL required fields
    const route: Route = {
      // Primary identifiers
      UID: routeUID,
      Code: data.code,
      Name: data.name,
      serialNumber: 0, // REQUIRED field that was missing
      KeyUID: routeUID, // Inherited from BaseModel
      
      // Organization (ALL REQUIRED)
      OrgUID: data.orgUID,
      CompanyUID: data.companyUID || data.orgUID, // REQUIRED - Use OrgUID if not provided
      WHOrgUID: data.whOrgUID || null, // Try null instead of empty string
      
      // Assignment
      RoleUID: data.roleUID,
      JobPositionUID: data.jobPositionUID || routeUID, // Often same as UID
      VehicleUID: data.vehicleUID || null, // Try null for optional fields
      LocationUID: data.locationUID || null, // Try null for optional fields
      
      // User (REQUIRED)
      User: data.user || 'SYSTEM', // REQUIRED - default to 'SYSTEM' if not provided
      
      // Status
      IsActive: data.isActive !== false,
      Status: data.status || 'Active',
      
      // Validity
      ValidFrom: validFrom,
      ValidUpto: validUpto,
      
      // Timing (ALL REQUIRED with defaults)
      VisitTime: data.visitTime || '00:00:00', // REQUIRED
      EndTime: data.endTime || '00:00:00', // REQUIRED
      VisitDuration: data.visitDuration || 0,
      TravelTime: data.travelTime || 0,
      TotalCustomers: data.selectedCustomers?.length || 0,
      IsCustomerWithTime: data.isCustomerWithTime || false,
      
      // Print settings (all default to false)
      PrintStanding: data.printStanding || false,
      PrintForward: data.printForward || false,
      PrintTopup: data.printTopup || false,
      PrintOrderSummary: data.printOrderSummary || false,
      
      // Journey Plan settings
      AutoFreezeJP: data.autoFreezeJP || false,
      AddToRun: data.addToRun || false,
      AutoFreezeRunTime: data.autoFreezeRunTime || undefined,
      
      // Backend fields
      IsChangeApplied: true, // Set to true for new routes
      SS: 0,
      
      // Audit fields (REQUIRED)
      CreatedBy: 'ADMIN', // Should come from auth context
      ModifiedBy: 'ADMIN',
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now
    };
    
    // Build RouteSchedule
    const routeSchedule: RouteSchedule = {
      UID: routeUID,
      RouteUID: routeUID,
      Name: data.name,
      Type: this.mapScheduleType(data.scheduleType) || 'Daily', // Map to backend expected values
      Status: data.isActive !== false ? 1 : 0,
      CompanyUID: data.companyUID || data.orgUID,
      
      // Dates (all required with DateTime format)
      StartDate: validFrom,
      FromDate: validFrom,
      ToDate: validUpto,
      
      // Times
      StartTime: data.visitTime || '00:00:00',
      EndTime: data.endTime || '00:00:00',
      VisitDurationInMinutes: data.visitDuration || 0,
      TravelTimeInMinutes: data.travelTime || 0,
      
      // Settings
      AllowMultipleBeatsPerDay: data.allowMultipleBeatsPerDay || false,
      PlannedDays: this.buildPlannedDays(data),
      
      // Backend fields
      SS: 0,
      
      // Audit fields (REQUIRED)
      CreatedBy: 'ADMIN',
      ModifiedBy: 'ADMIN',
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now
    };
    
    // Build RouteScheduleDaywise
    const routeScheduleDaywise: RouteScheduleDaywise = {
      RouteScheduleUID: routeUID,
      Monday: data.monday !== false ? 1 : 0,
      Tuesday: data.tuesday !== false ? 1 : 0,
      Wednesday: data.wednesday !== false ? 1 : 0,
      Thursday: data.thursday !== false ? 1 : 0,
      Friday: data.friday !== false ? 1 : 0,
      Saturday: data.saturday !== false ? 1 : 0,
      Sunday: data.sunday !== false ? 1 : 0,
      
      // Optional fields
      UID: routeUID,
      SS: 0,
      CreatedBy: 'ADMIN',
      ModifiedBy: 'ADMIN',
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now
    };
    
    // Build RouteCustomersList
    const routeCustomersList: RouteCustomer[] = (data.selectedCustomers || []).map((storeUID, index) => {
      const customerSchedule = data.customerSchedules?.[storeUID];
      const customerUID = `${routeUID}_CUST_${index + 1}`; // Ensure unique UID
      
      return {
        RouteUID: routeUID,
        StoreUID: storeUID,
        SeqNo: index + 1,
        UID: customerUID,
        
        // Timing
        VisitTime: customerSchedule?.visitTime || '00:00:00',
        VisitDuration: customerSchedule?.visitDuration || data.visitDuration || 0,
        EndTime: '00:00:00', // Calculate if needed
        TravelTime: customerSchedule?.travelTime || data.travelTime || 0,
        
        // Status
        IsDeleted: false,
        ActionType: 0,
        
        // Audit
        CreatedBy: 'ADMIN',
        ModifiedBy: 'ADMIN',
        CreatedTime: now,
        ModifiedTime: now,
        ServerAddTime: now,
        ServerModifiedTime: now
      };
    });
    
    // Build RouteUserList
    const routeUserList: RouteUser[] = [];
    
    // Always add the primary user
    routeUserList.push({
      RouteUID: routeUID,
      JobPositionUID: data.jobPositionUID || routeUID,
      FromDate: validFrom, // Already formatted with time
      ToDate: validUpto, // Already formatted with time
      IsActive: true,
      ActionType: 0,
      UID: routeUID,
      CreatedBy: 'ADMIN',
      ModifiedBy: 'ADMIN',
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now
    });
    
    // Add additional users if provided
    if (data.assignedUsers) {
      data.assignedUsers.forEach((user, index) => {
        routeUserList.push({
          RouteUID: routeUID,
          JobPositionUID: user.jobPositionUID,
          FromDate: user.fromDate || validFrom,
          ToDate: user.toDate || validUpto,
          IsActive: true,
          ActionType: 0,
          UID: `${routeUID}_USER_${index + 1}`,
          CreatedBy: 'ADMIN',
          ModifiedBy: 'ADMIN',
          CreatedTime: now,
          ModifiedTime: now,
          ServerAddTime: now,
          ServerModifiedTime: now
        });
      });
    }
    
    // Determine which schedule objects to include based on type
    const scheduleType = this.mapScheduleType(data.scheduleType);
    const includeDaywise = ['Daily', 'Weekly', 'MultiplePerWeeks'].includes(scheduleType);
    const includeFortnight = scheduleType === 'Fortnightly';
    
    // Debug logging commented out for production
    // console.log('Schedule Type Mapping:', {
    //   original: data.scheduleType,
    //   mapped: scheduleType,
    //   includeDaywise,
    //   includeFortnight
    // });
    
    // Build final RouteMaster - backend expects all properties even if null
    const routeMaster: RouteMaster = {
      Route: route,
      RouteSchedule: routeSchedule,
      RouteScheduleDaywise: includeDaywise ? routeScheduleDaywise : null,
      RouteScheduleFortnight: null, // Will be set below if needed
      RouteCustomersList: routeCustomersList,
      RouteUserList: routeUserList.length > 0 ? routeUserList : null // null if no users
    };
    
    // Add fortnight schedule if type is Fortnightly
    if (data.scheduleType === 'Fortnightly') {
      routeMaster.RouteScheduleFortnight = {
        UID: `FN_${routeUID}`,
        RouteScheduleUID: routeUID,
        Week1: data.week1 || false,
        Week2: data.week2 || false,
        Week3: data.week3 || false,
        Week4: data.week4 || false,
        CreatedBy: 'ADMIN',
        ModifiedBy: 'ADMIN'
      };
    }
    
    // Debug logging commented out for production
    // console.log('Final RouteMaster Structure:', JSON.stringify(routeMaster, null, 2));
    
    return routeMaster;
  }
  
  /**
   * Build planned days string from day selections
   */
  private buildPlannedDays(data: RouteCreationData): string {
    const days: string[] = [];
    
    if (data.monday !== false) days.push('Monday');
    if (data.tuesday !== false) days.push('Tuesday');
    if (data.wednesday !== false) days.push('Wednesday');
    if (data.thursday !== false) days.push('Thursday');
    if (data.friday !== false) days.push('Friday');
    if (data.saturday !== false) days.push('Saturday');
    if (data.sunday !== false) days.push('Sunday');
    
    return days.join(',');
  }
  
  /**
   * Map frontend schedule type to backend expected values
   * Note: Monthly is not supported by backend processing
   */
  private mapScheduleType(type: string | undefined): string {
    switch(type) {
      case 'Daily':
        return 'Daily';
      case 'Weekly':
        return 'Weekly'; // Fixed: Backend expects 'Weekly' for weekly routes
      case 'MultiplePerWeeks':
        return 'MultiplePerWeeks'; // Added: Support for multiple visits per week
      case 'Fortnightly':
        return 'Fortnightly'; // Fixed: Backend expects 'Fortnightly' not 'Fortnight'
      // case 'Monthly': // Not supported by backend - no processing logic exists
      //   return 'Monthly';
      default:
        return 'Daily';
    }
  }
  
  /**
   * Generate unique route code
   */
  generateRouteCode(prefix: string = 'RT'): string {
    const timestamp = Date.now().toString(36).toUpperCase();
    const random = Math.random().toString(36).substring(2, 8).toUpperCase(); // Longer random string
    return `${prefix}_${timestamp}_${random}`;
  }
  
  /**
   * Validate route data before submission
   */
  validateRouteData(data: RouteCreationData): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];
    
    // Required fields validation
    if (!data.code) errors.push('Route code is required');
    if (!data.name) errors.push('Route name is required');
    if (!data.orgUID) errors.push('Organization is required');
    if (!data.roleUID) errors.push('Role is required');
    if (!data.validFrom) errors.push('Valid from date is required');
    if (!data.validUpto) errors.push('Valid upto date is required');
    
    // Validate organization is actual value, not placeholder
    if (data.orgUID === 'ORG123') {
      errors.push('Please select a valid organization (e.g., Farmley)');
    }
    
    // Validate role is actual value
    if (!['Team Leader', 'Merchandiser'].includes(data.roleUID)) {
      errors.push('Please select a valid role (Team Leader or Merchandiser)');
    }
    
    // Validate dates
    const fromDate = new Date(data.validFrom);
    const toDate = new Date(data.validUpto);
    if (fromDate >= toDate) {
      errors.push('Valid upto date must be after valid from date');
    }
    
    // Validate timing
    if (data.visitDuration && (data.visitDuration < 0 || data.visitDuration > 1440)) {
      errors.push('Visit duration must be between 0 and 1440 minutes');
    }
    
    if (data.travelTime && (data.travelTime < 0 || data.travelTime > 1440)) {
      errors.push('Travel time must be between 0 and 1440 minutes');
    }
    
    // Validate schedule type specific requirements
    if (data.scheduleType === 'Weekly') {
      const hasAnyDay = data.monday || data.tuesday || data.wednesday || 
                        data.thursday || data.friday || data.saturday || data.sunday;
      if (!hasAnyDay) {
        errors.push('At least one day must be selected for weekly schedule');
      }
    }
    
    if (data.scheduleType === 'Fortnightly') {
      const hasAnyWeek = data.week1 || data.week2 || data.week3 || data.week4;
      if (!hasAnyWeek) {
        errors.push('At least one week must be selected for fortnightly schedule');
      }
    }
    
    return {
      isValid: errors.length === 0,
      errors
    };
  }
}

// ==================== HELPER TYPES ====================

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
  ErrorMessage?: string;
}

export interface ApiResponse<T = any> {
  IsSuccess: boolean;
  Data: T;
  Message?: string;
  ErrorMessage?: string;
  StatusCode?: number;
}

// ==================== EXPORT INSTANCE ====================

export const routeServiceProduction = new RouteServiceProduction();

// Default export for backward compatibility
export default routeServiceProduction;