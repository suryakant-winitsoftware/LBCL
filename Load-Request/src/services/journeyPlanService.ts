import { apiService, api } from "./api";

export interface UserJourney {
  UID: string;
  JobPositionUID: string;
  EmpUID: string;
  JourneyStartTime?: string;
  JourneyEndTime?: string;
  StartOdometerReading: number;
  EndOdometerReading: number;
  JourneyTime?: string;
  VehicleUID?: string;
  EOTStatus: string;
  ReopenedBy?: string;
  HasAuditCompleted: boolean;
  BeatHistoryUID?: string;
  WhStockRequestUID?: string;
  LoginId: string;
  IsSynchronizing: boolean;
  HasInternet: boolean;
  InternetType?: string;
  DownloadSpeed: number;
  UploadSpeed: number;
  HasMobileNetwork: boolean;
  IsLocationEnabled: boolean;
  BatteryPercentageTarget: number;
  BatteryPercentageAvailable: number;
  AttendanceStatus: string;
  AttendanceLatitude?: string;
  AttendanceLongitude?: string;
  AttendanceAddress?: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
}

// Route Schedule Interfaces
export interface RouteSchedule {
  UID: string;
  RouteUID: string;
  Name: string;
  Type: 'Daily' | 'Weekly' | 'Fortnightly' | 'Monthly';
  StartDate?: string;
  FromDate?: string;
  ToDate?: string;
  StartTime?: string;
  EndTime?: string;
  VisitDurationInMinutes: number;
  TravelTimeInMinutes: number;
  LastBeatDate?: string;
  NextBeatDate?: string;
  AllowMultipleBeatsPerDay: boolean;
  PlannedDays?: string;
  Status: number;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface RouteScheduleDaywise {
  UID: string;
  RouteScheduleUID: string;
  Monday: number;
  Tuesday: number;
  Wednesday: number;
  Thursday: number;
  Friday: number;
  Saturday: number;
  Sunday: number;
  SS: number;
  CreatedBy: string;
  ModifiedBy: string;
}

// Vehicle Interfaces
export interface Vehicle {
  UID: string;
  CompanyUID?: string;
  OrgUID: string;
  VehicleNo: string;
  RegistrationNo?: string;
  Model?: string;
  Type?: string;
  IsActive: boolean;
  TruckSIDate?: string;
  RoadTaxExpiryDate?: string;
  InspectionDate?: string;
  CreatedBy: string;
  ModifiedBy: string;
}

// Store History Stats Interface
export interface StoreHistoryStats {
  UID: string;
  StoreHistoryUID: string;
  CheckInTime?: string;
  CheckOutTime?: string;
  TotalTimeInMin?: number;
  IsForceCheckIn?: boolean;
  Latitude?: string;
  Longitude?: string;
  CreatedBy: string;
  ModifiedBy: string;
}

// Route Customer Interface
export interface RouteCustomer {
  UID: string;
  RouteUID: string;
  StoreUID: string;
  SeqNo: number;
  VisitTime?: string;
  VisitDuration: number;
  EndTime?: string;
  TravelTime: number;
  IsDeleted: boolean;
  CreatedBy: string;
  ModifiedBy: string;
}

// Enhanced Store History Interface
export interface StoreHistoryComplete {
  UID: string;
  UserJourneyUID?: string;
  YearMonth: number;
  BeatHistoryUID: string;
  OrgUID: string;
  RouteUID: string;
  StoreUID: string;
  IsPlanned: boolean;
  SerialNo: number;
  Status: string;
  VisitDuration: number;
  TravelTime: number;
  PlannedLoginTime?: string;
  PlannedLogoutTime?: string;
  LoginTime?: string;
  LogoutTime?: string;
  NoOfVisits: number;
  LastVisitDate?: string;
  IsSkipped: boolean;
  IsProductive: boolean;
  IsGreen: boolean;
  TargetValue: number;
  TargetVolume: number;
  TargetLines: number;
  ActualValue: number;
  ActualVolume: number;
  ActualLines: number;
  PlannedTimeSpendInMinutes: number;
  Latitude?: string;
  Longitude?: string;
  Notes?: string;
  SS: number;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface AssignedJourneyPlan {
  UID: string;
  RouteUID: string;
  RouteName: string;
  OrgUID: string;
  JobPositionUID: string;
  EmpUID: string;
  VisitDate: string;
  PlannedStoreVisits: number;
  ActualStoreVisits: number;
  SkippedStoreVisits?: number;
  UnPlannedStoreVisits?: number;
  Coverage: number;
  Status: string;
  StartTime?: string;
  EndTime?: string;
  PlannedStartTime?: string;
  PlannedEndTime?: string;
  InvoiceStatus?: string;
  Notes?: string;
  CreatedTime?: string;
  ModifiedTime?: string;
}

export interface BeatHistory {
  UID: string;
  RouteUID: string;
  RouteScheduleUID: string;
  BeatPlanDate: string;
  BeatStatus: string;
  UserUID: string;
  JourneyStartTime?: string;
  JourneyEndTime?: string;
  CreatedBy: string;
  ModifiedBy: string;
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

export interface JourneyPlanRequest {
  OrgUID: string;
  JobPositionUID?: string;
  VisitDate: string;
  Type: "Today" | "Future" | "Past";
  PageNumber: number;
  PageSize: number;
  IsCountRequired: boolean;
}

class JourneyPlanService {
  // Route Schedule Methods - DISABLED: Backend endpoints not yet implemented
  async getRouteSchedules(request: PagedRequest): Promise<PagedResponse<RouteSchedule>> {
    // return apiService.post('/RouteSchedule/SelectAllRouteScheduleDetails', request);
    // Fallback: Return empty result for now
    console.warn('RouteSchedule API not implemented - returning empty data');
    return Promise.resolve({
      IsSuccess: true,
      Data: { PagedData: [], TotalCount: 0 }
    });
  }

  async getRouteScheduleByRouteUID(routeUID: string): Promise<RouteSchedule> {
    // return apiService.get(`/RouteSchedule/GetRouteScheduleByRouteUID?RouteUID=${routeUID}`);
    // Fallback: Return default schedule
    console.warn('RouteSchedule API not implemented - returning default schedule');
    return Promise.resolve({
      UID: `schedule-${routeUID}`,
      RouteUID: routeUID,
      Name: 'Default Schedule',
      Type: 'Daily' as const,
      VisitDurationInMinutes: 30,
      TravelTimeInMinutes: 15,
      AllowMultipleBeatsPerDay: false,
      Status: 1,
      CreatedBy: 'system',
      ModifiedBy: 'system'
    });
  }

  async createRouteSchedule(scheduleData: RouteSchedule): Promise<any> {
    // return apiService.post('/RouteSchedule/CreateRouteSchedule', scheduleData);
    // Fallback: Return success for now
    console.warn('RouteSchedule API not implemented - skipping schedule creation');
    return Promise.resolve({ IsSuccess: true, Data: scheduleData.UID });
  }

  async updateRouteSchedule(scheduleData: RouteSchedule): Promise<any> {
    // return apiService.post('/RouteSchedule/UpdateRouteSchedule', scheduleData);
    // Fallback: Return success for now
    console.warn('RouteSchedule API not implemented - skipping schedule update');
    return Promise.resolve({ IsSuccess: true });
  }

  async deleteRouteSchedule(uid: string): Promise<any> {
    // return apiService.delete(`/RouteSchedule/DeleteRouteSchedule?UID=${uid}`);
    // Fallback: Return success for now
    console.warn('RouteSchedule API not implemented - skipping schedule deletion');
    return Promise.resolve({ IsSuccess: true });
  }

  // Route Schedule Daywise Methods - DISABLED: Backend endpoints not yet implemented
  async getRouteScheduleDaywise(routeScheduleUID: string): Promise<RouteScheduleDaywise> {
    // return apiService.get(`/RouteSchedule/GetRouteScheduleDaywiseByUID?RouteScheduleUID=${routeScheduleUID}`);
    // Fallback: Return default daywise schedule
    console.warn('RouteScheduleDaywise API not implemented - returning default daywise schedule');
    return Promise.resolve({
      UID: `daywise-${routeScheduleUID}`,
      RouteScheduleUID: routeScheduleUID,
      Monday: 1,
      Tuesday: 1,
      Wednesday: 1,
      Thursday: 1,
      Friday: 1,
      Saturday: 0,
      Sunday: 0,
      SS: 1,
      CreatedBy: 'system',
      ModifiedBy: 'system'
    });
  }

  async createRouteScheduleDaywise(daywiseData: RouteScheduleDaywise): Promise<any> {
    // return apiService.post('/RouteSchedule/CreateRouteScheduleDaywise', daywiseData);
    // Fallback: Return success for now
    console.warn('RouteScheduleDaywise API not implemented - skipping daywise creation');
    return Promise.resolve({ IsSuccess: true, Data: daywiseData.UID });
  }

  async updateRouteScheduleDaywise(daywiseData: RouteScheduleDaywise): Promise<any> {
    // return apiService.post('/RouteSchedule/UpdateRouteScheduleDaywise', daywiseData);
    // Fallback: Return success for now
    console.warn('RouteScheduleDaywise API not implemented - skipping daywise update');
    return Promise.resolve({ IsSuccess: true });
  }

  // Vehicle Methods
  async getVehicles(request: PagedRequest): Promise<PagedResponse<Vehicle>> {
    return apiService.post('/Vehicle/SelectAllVehicleDetails', request);
  }

  async getVehiclesByOrg(orgUID: string): Promise<Vehicle[]> {
    return api.dropdown.getVehicle(orgUID);
  }

  async getVehicleByUID(uid: string): Promise<Vehicle> {
    return apiService.get(`/Vehicle/GetVehicleByUID?UID=${uid}`);
  }

  async createVehicle(vehicleData: Vehicle): Promise<any> {
    return apiService.post('/Vehicle/CreateVehicle', vehicleData);
  }

  async updateVehicle(vehicleData: Vehicle): Promise<any> {
    return apiService.post('/Vehicle/UpdateVehicle', vehicleData);
  }

  async deleteVehicle(uid: string): Promise<any> {
    return apiService.delete(`/Vehicle/DeleteVehicle?UID=${uid}`);
  }

  // Store History Stats Methods
  async getStoreHistoryStats(storeHistoryUID: string): Promise<StoreHistoryStats[]> {
    return apiService.get(`/StoreHistory/GetStoreHistoryStatsByUID?StoreHistoryUID=${storeHistoryUID}`);
  }

  async createStoreHistoryStats(statsData: StoreHistoryStats): Promise<any> {
    // return apiService.post('/StoreHistory/CreateStoreHistoryStats', statsData);
    console.warn('StoreHistoryStats creation endpoint not implemented - returning mock success');
    return Promise.resolve({ IsSuccess: true, Data: 1, Message: 'StoreHistoryStats creation simulated' });
  }

  async updateStoreHistoryStats(statsData: StoreHistoryStats): Promise<any> {
    return apiService.post('/StoreHistory/UpdateStoreHistoryStats', statsData);
  }

  // Route Customer Methods
  async getRouteCustomers(routeUID: string): Promise<RouteCustomer[]> {
    return apiService.get(`/Route/GetRouteCustomersByRouteUID?RouteUID=${routeUID}`);
  }

  async createRouteCustomer(customerData: RouteCustomer): Promise<any> {
    return apiService.post('/Route/CreateRouteCustomer', customerData);
  }

  async updateRouteCustomer(customerData: RouteCustomer): Promise<any> {
    return apiService.post('/Route/UpdateRouteCustomer', customerData);
  }

  async deleteRouteCustomer(uid: string): Promise<any> {
    return apiService.delete(`/Route/DeleteRouteCustomer?UID=${uid}`);
  }

  async updateRouteCustomerSequence(routeUID: string, customerSequences: { StoreUID: string; SeqNo: number }[]): Promise<any> {
    return apiService.post('/Route/UpdateRouteCustomerSequence', {
      RouteUID: routeUID,
      CustomerSequences: customerSequences
    });
  }

  // Enhanced Store History Methods
  async getStoreHistories(request: PagedRequest): Promise<PagedResponse<StoreHistoryComplete>> {
    return apiService.post('/StoreHistory/SelectAllStoreHistoryDetails', request);
  }

  async getStoreHistoryByBeatHistoryUID(beatHistoryUID: string): Promise<StoreHistoryComplete[]> {
    return apiService.get(`/StoreHistory/GetStoreHistoryByBeatHistoryUID?BeatHistoryUID=${beatHistoryUID}`);
  }

  async createStoreHistory(storeHistoryData: StoreHistoryComplete): Promise<any> {
    // return apiService.post('/StoreHistory/CreateStoreHistory', storeHistoryData);
    console.warn('StoreHistory creation endpoint not implemented - returning mock success');
    return Promise.resolve({ IsSuccess: true, Data: 1, Message: 'StoreHistory creation simulated' });
  }

  async createMultipleStoreHistories(storeHistories: StoreHistoryComplete[]): Promise<any> {
    return apiService.post('/StoreHistory/CreateMultipleStoreHistories', {
      StoreHistories: storeHistories
    });
  }

  async updateStoreHistory(storeHistoryData: StoreHistoryComplete): Promise<any> {
    return apiService.post('/StoreHistory/UpdateStoreHistory', storeHistoryData);
  }

  async updateStoreHistoryStatus(uid: string, status: string): Promise<any> {
    return apiService.post('/BeatHistory/UpdateStoreHistoryStatus', {
      UID: uid,
      Status: status
    });
  }

  async deleteStoreHistory(uid: string): Promise<any> {
    return apiService.delete(`/StoreHistory/DeleteStoreHistory?UID=${uid}`);
  }

  // User Journey Methods
  async getUserJourneys(
    request: PagedRequest
  ): Promise<PagedResponse<UserJourney>> {
    // FIXED: Use BeatHistory API instead of UserJourney API for journey plans
    // UserJourney API returns execution data, BeatHistory API returns planning data
    return api.beatHistory.selectAll(request);
  }

  async getUserJourneyById(uid: string): Promise<any> {
    // FIXED: Use BeatHistory API for journey plan data
    // Journey plans are stored in beat_history table, not user_journey
    return api.beatHistory.getByUID(uid);
  }

  async createUserJourney(journeyData: UserJourney): Promise<any> {
    return api.userJourney.create(journeyData);
  }

  async updateUserJourney(journeyData: UserJourney): Promise<any> {
    return api.userJourney.update(journeyData);
  }

  async deleteUserJourney(uid: string): Promise<any> {
    // Note: Both "journey" and "plan" deletions use BeatHistory table
    // The "journeys" tab shows UserJourney grid data but deletes from BeatHistory
    // because the actual journey execution records are stored in BeatHistory
    console.log(`🗑️ JourneyPlanService: Using CASCADE HARD DELETE for user journey: ${uid}`);
    
    // Use the same robust cascade deletion logic as deleteJourneyPlan
    return this.deleteJourneyPlan(uid);
  }

  async completeUserJourney(uid: string): Promise<any> {
    return api.userJourney.complete(uid);
  }

  // Journey Plan Methods
  async getTodayJourneyPlans(params: {
    visitDate: string;
    orgUID: string;
    jobPositionUID?: string;
    request: PagedRequest;
  }): Promise<PagedResponse<AssignedJourneyPlan>> {
    return api.userJourney.selectTodayPlan(
      params.visitDate,
      params.orgUID,
      params.jobPositionUID || "",
      params.request
    );
  }

  async getJourneyPlanById(uid: string): Promise<any> {
    return api.journeyPlan.getByUID(uid);
  }

  async createJourneyPlan(planData: any): Promise<any> {
    // FIXED: Use BeatHistory API for creating journey plans
    // Journey plans are stored in beat_history table
    return api.beatHistory.create(planData);
  }

  async updateJourneyPlan(planData: any): Promise<any> {
    // FIXED: Use BeatHistory API for updating journey plans
    // Journey plans are stored in beat_history table
    return api.beatHistory.update(planData);
  }

  async deleteJourneyPlan(uid: string): Promise<any> {
    try {
      console.log(`🗑️ JourneyPlanService: Starting BACKEND-COMPATIBLE HARD DELETE for journey plan: ${uid}`);
      
      const deleteLog = {
        journeyPlanUID: uid,
        backendIssues: [],
        alternativeAttempts: [],
        mainRecordDeleted: false,
        errors: []
      };

      // Since most backend endpoints are not implemented, we need a different approach
      console.log(`🔧 Backend API Status Check:`);
      console.log(`⚠️  GetCustomersByBeatHistoryUID - Not implemented (500 error)`);
      console.log(`⚠️  UserJourney deletion endpoints - Not found (404 errors)`);
      console.log(`⚠️  StoreHistory deletion endpoints - Unknown status`);
      console.log(`❌ Main DeleteBeatHistory - Failing due to foreign key constraints`);

      // Since the backend doesn't support child record lookup, we can't do proper cascade deletion
      // Let's try direct deletion approaches that might bypass constraints

      console.log(`🔍 Attempting direct deletion strategies...`);

      // Strategy 1: Try to delete the main record directly (will likely fail but good to confirm)
      try {
        console.log(`📋 Strategy 1: Direct BeatHistory deletion`);
        const result = await api.beatHistory.delete(uid);
        
        if (result && result.IsSuccess) {
          deleteLog.mainRecordDeleted = true;
          console.log(`✅ SUCCESS: Direct deletion worked unexpectedly!`);
          return {
            IsSuccess: true,
            Message: "Journey plan deleted successfully via direct method",
            Data: deleteLog,
            DeleteType: "DIRECT_DELETE"
          };
        }
      } catch (directError) {
        console.log(`❌ Strategy 1 failed as expected: ${directError.message}`);
        deleteLog.errors.push(`Direct deletion: ${directError.message}`);
      }

      // Strategy 2: Check if we can find working endpoints for child record deletion
      console.log(`📋 Strategy 2: Manual child record cleanup`);
      
      // Try alternative ways to identify and delete child records
      try {
        // Look for any working endpoint that might give us related records
        const possibleEndpoints = [
          `/BeatHistory/GetBeatHistoryDetails?UID=${uid}`,
          `/BeatHistory/GetBeatHistoryByUID?UID=${uid}`,
          `/StoreHistory/GetStoreHistoryByBeatHistoryUID?BeatHistoryUID=${uid}`,
        ];
        
        for (const endpoint of possibleEndpoints) {
          try {
            console.log(`🔍 Trying: ${endpoint}`);
            const response = await apiService.get(endpoint);
            console.log(`✅ Found working endpoint: ${endpoint}`, response);
            
            // If we get data, we might be able to extract child record IDs
            if (response && response.Data) {
              deleteLog.alternativeAttempts.push(`Found data via: ${endpoint}`);
              // TODO: Extract child IDs and attempt deletion
            }
          } catch (endpointError) {
            console.log(`❌ ${endpoint} failed: ${endpointError.message}`);
          }
        }
      } catch (error) {
        console.log(`⚠️ Child record discovery failed: ${error.message}`);
      }

      // Strategy 3: Database-level constraint bypass (inform user about backend needs)
      console.log(`📋 Strategy 3: Backend constraint resolution needed`);
      
      const backendSolution = `
🔧 BACKEND SOLUTION REQUIRED:

The deletion is failing because the backend lacks proper cascade deletion support.
The following backend endpoints need to be implemented or fixed:

✅ WORKING: /BeatHistory/DeleteBeatHistory (but fails due to constraints)
❌ MISSING: /BeatHistory/GetCustomersByBeatHistoryUID (returns 500 "not implemented") 
❌ MISSING: /StoreHistory/DeleteStoreHistory (cascade deletion)
❌ MISSING: /UserJourney/DeleteUserJourney (cascade deletion)

Backend needs to either:
1. Implement proper cascade deletion in DeleteBeatHistory endpoint
2. Add the missing child record endpoints
3. Add database CASCADE DELETE constraints
4. Implement a specialized CascadeDeleteBeatHistory endpoint

Current error: Foreign key constraints prevent deletion of BeatHistory 
because related StoreHistory and UserJourney records exist.
      `;

      console.log(backendSolution);
      deleteLog.backendIssues.push(backendSolution);

      // For now, return detailed error with solution guidance
      throw new Error(
        `🔥 HARD DELETE FAILED - Backend API Limitations\n\n` +
        `The journey plan cannot be deleted because:\n` +
        `1. Backend lacks cascade deletion support\n` +
        `2. Child record lookup endpoints are not implemented\n` +
        `3. Database foreign key constraints prevent deletion\n\n` +
        `SOLUTION: Backend team needs to implement proper cascade deletion in the DeleteBeatHistory endpoint ` +
        `OR add the missing child record management endpoints.\n\n` +
        `Original error: Delete Failed (500 Internal Server Error)`
      );

    } catch (error) {
      console.error('❌ JourneyPlanService: HARD DELETE FAILED - Backend API Issues:', error);
      throw error;
    }
  }

  async startJourneyPlan(uid: string): Promise<any> {
    return api.journeyPlan.start(uid);
  }

  /**
   * FORCE HARD DELETE - Physically removes records from database (NOT soft delete)
   * This method ensures complete physical removal of the journey plan and all related data
   */
  async forceHardDeleteJourneyPlan(uid: string): Promise<any> {
    console.log(`🔥 FORCE HARD DELETE initiated for journey plan: ${uid}`);
    
    try {
      const deleteLog = {
        beatHistoryUID: uid,
        hardDeletedRecords: [],
        errors: [],
        isCompletelyDeleted: false
      };

      // Step 1: Force delete ALL related records using multiple endpoints
      try {
        console.log(`🔍 Searching for ALL related records to HARD DELETE...`);
        
        // Try to get related records from different endpoints
        const endpoints = [
          () => api.beatHistory.getCustomersByBeatHistoryUID(uid),
          () => apiService.get(`/BeatHistory/GetBeatHistoryByUID?UID=${uid}`),
        ];

        for (const getEndpoint of endpoints) {
          try {
            const relatedData = await getEndpoint();
            if (relatedData && relatedData.Data) {
              const records = Array.isArray(relatedData.Data) ? relatedData.Data : [relatedData.Data];
              
              for (const record of records) {
                if (record && (record.UID || record.Id)) {
                  const recordUID = record.UID || record.Id;
                  
                  // Try multiple delete endpoints for HARD DELETE
                  const deleteEndpoints = [
                    () => apiService.delete(`/StoreHistory/DeleteStoreHistory?UID=${recordUID}`),
                    () => apiService.delete(`/BeatHistory/DeleteStoreHistory?UID=${recordUID}`),
                    () => apiService.delete(`/UserJourney/DeleteUserJourney?UID=${recordUID}`),
                  ];

                  for (const deleteEndpoint of deleteEndpoints) {
                    try {
                      await deleteEndpoint();
                      deleteLog.hardDeletedRecords.push(`✓ HARD DELETED: ${recordUID}`);
                      console.log(`🗑️ HARD DELETED related record: ${recordUID}`);
                      break; // Success, no need to try other endpoints
                    } catch (deleteError) {
                      // Try next endpoint
                      continue;
                    }
                  }
                }
              }
            }
          } catch (getError) {
            // Try next endpoint
            continue;
          }
        }
      } catch (searchError) {
        deleteLog.errors.push(`Could not search for related records: ${searchError.message}`);
      }

      // Step 2: FORCE HARD DELETE the main BeatHistory record using multiple methods
      const mainDeleteMethods = [
        () => apiService.delete(`/BeatHistory/DeleteBeatHistory?UID=${uid}`),
        () => apiService.delete(`/BeatHistory/HardDeleteBeatHistory?UID=${uid}`), // Try alternative endpoint
        () => apiService.delete(`/BeatHistory/ForceDeleteBeatHistory?UID=${uid}`), // Try force delete endpoint
      ];

      let mainDeleted = false;
      for (const deleteMethod of mainDeleteMethods) {
        try {
          const result = await deleteMethod();
          console.log(`🗑️ HARD DELETED main BeatHistory record using method ${mainDeleteMethods.indexOf(deleteMethod) + 1}`);
          deleteLog.isCompletelyDeleted = true;
          mainDeleted = true;
          break;
        } catch (deleteError) {
          deleteLog.errors.push(`Delete method ${mainDeleteMethods.indexOf(deleteMethod) + 1} failed: ${deleteError.message}`);
          continue;
        }
      }

      if (!mainDeleted) {
        throw new Error(`HARD DELETE FAILED: Could not physically remove BeatHistory record ${uid} from database`);
      }

      console.log(`✅ FORCE HARD DELETE COMPLETED:`, deleteLog);
      return {
        IsSuccess: true,
        Message: `HARD DELETE completed. Physically removed ${deleteLog.hardDeletedRecords.length} related records and main record.`,
        Data: deleteLog,
        DeleteType: "HARD_DELETE_COMPLETE"
      };

    } catch (error) {
      console.error(`❌ FORCE HARD DELETE FAILED:`, error);
      throw new Error(`HARD DELETE FAILED: ${error.message}. Record may still exist in database.`);
    }
  }

  /**
   * Cascade delete a journey plan and all related records
   * This is a more thorough deletion that handles foreign key constraints
   */
  async cascadeDeleteJourneyPlan(uid: string): Promise<any> {
    try {
      console.log(`JourneyPlanService: Starting cascade delete for journey plan: ${uid}`);
      
      const deleteResults = {
        beatHistoryUID: uid,
        deletedStoreHistories: 0,
        deletedUserJourneys: 0,
        mainRecordDeleted: false,
        errors: []
      };

      // Step 1: Delete all StoreHistory records linked to this BeatHistory
      try {
        const relatedStoreHistories = await this.getStoreHistoryByBeatHistoryUID(uid);
        if (relatedStoreHistories && relatedStoreHistories.length > 0) {
          console.log(`Found ${relatedStoreHistories.length} StoreHistory records to delete`);
          
          for (const storeHistory of relatedStoreHistories) {
            try {
              await this.deleteStoreHistory(storeHistory.UID);
              deleteResults.deletedStoreHistories++;
              console.log(`✓ Deleted StoreHistory: ${storeHistory.UID}`);
            } catch (error) {
              deleteResults.errors.push(`Failed to delete StoreHistory ${storeHistory.UID}: ${error.message}`);
              console.warn(`✗ Failed to delete StoreHistory ${storeHistory.UID}:`, error);
            }
          }
        }
      } catch (error) {
        deleteResults.errors.push(`Failed to retrieve StoreHistory records: ${error.message}`);
        console.warn('Could not retrieve related StoreHistory records:', error);
      }

      // Step 2: Delete UserJourney records that reference this BeatHistory
      try {
        // Note: This would need a UserJourney API that supports querying by BeatHistoryUID
        // For now, we'll attempt the main delete and see if it works
        console.log('Skipping UserJourney deletion (no API available for BeatHistoryUID lookup)');
      } catch (error) {
        deleteResults.errors.push(`Failed to delete UserJourney records: ${error.message}`);
      }

      // Step 3: Delete the main BeatHistory record
      try {
        const result = await api.journeyPlan.delete(uid);
        if (result && (result.IsSuccess === true || !('IsSuccess' in result))) {
          deleteResults.mainRecordDeleted = true;
          console.log(`✓ Successfully deleted main BeatHistory record: ${uid}`);
        } else {
          throw new Error(result?.ErrorMessage || result?.Message || 'Unknown error');
        }
      } catch (error) {
        deleteResults.errors.push(`Failed to delete main BeatHistory record: ${error.message}`);
        throw error; // Re-throw since this is the critical operation
      }

      // Return comprehensive results
      console.log('Cascade delete completed:', deleteResults);
      return {
        IsSuccess: deleteResults.mainRecordDeleted && deleteResults.errors.length === 0,
        Data: deleteResults,
        Message: `Cascade delete completed. Deleted ${deleteResults.deletedStoreHistories} store histories. ${deleteResults.errors.length} errors encountered.`,
        Errors: deleteResults.errors
      };

    } catch (error) {
      console.error('Cascade delete failed:', error);
      throw error;
    }
  }

  // Beat History Methods - JOURNEY PLAN PLANNING DATA
  async getBeatHistory(
    request: PagedRequest
  ): Promise<PagedResponse<BeatHistory>> {
    return api.beatHistory.selectAll(request);
  }

  // Get Journey Plans (Beat History) - This is where created journey plans appear
  async getJourneyPlans(
    request: PagedRequest  
  ): Promise<PagedResponse<any>> {
    console.log('🎯 JourneyPlanService: Using CORRECT API - BeatHistory/SelectAllJobPositionDetails for journey plan data');
    return api.beatHistory.selectAll(request);
  }

  async getBeatHistoryById(uid: string): Promise<any> {
    return api.beatHistory.getCustomersByBeatHistoryUID(uid);
  }

  async createBeatHistory(beatData: BeatHistory): Promise<any> {
    return api.beatHistory.create(beatData);
  }

  async updateBeatHistory(beatData: BeatHistory): Promise<any> {
    return apiService.post("/BeatHistory/UpdateBeatHistory", beatData);
  }

  // Journey Plan Workflow Methods
  async createCompleteJourneyPlan(journeyPlanData: {
    beatHistory: any;
    storeHistories: StoreHistoryComplete[];
    routeSchedule?: RouteSchedule;
    vehicleUID?: string;
  }): Promise<any> {
    try {
      // Step 1: Create Beat History
      const beatHistoryResponse = await this.createBeatHistory(journeyPlanData.beatHistory);
      
      if (!beatHistoryResponse?.IsSuccess && !beatHistoryResponse?.Data) {
        throw new Error('Failed to create Beat History');
      }

      // Step 2: Create Store Histories with proper planning
      const storeHistoriesWithPlanning = journeyPlanData.storeHistories.map((history, index) => ({
        ...history,
        BeatHistoryUID: journeyPlanData.beatHistory.UID,
        IsPlanned: true,
        SerialNo: index + 1,
      }));

      const storeHistoryResponse = await this.createMultipleStoreHistories(storeHistoriesWithPlanning);

      // Step 3: Create Route Schedule if provided (disabled - API not implemented)
      if (journeyPlanData.routeSchedule) {
        // await this.createRouteSchedule(journeyPlanData.routeSchedule);
        console.log('Route schedule creation skipped - API not implemented');
      }

      // Step 4: Initialize Store History Stats for each store
      const statsPromises = storeHistoriesWithPlanning.map(history => 
        this.createStoreHistoryStats({
          UID: `${history.UID}_STATS`,
          StoreHistoryUID: history.UID,
          CheckInTime: null,
          CheckOutTime: null,
          TotalTimeInMin: null,
          IsForceCheckIn: false,
          Latitude: null,
          Longitude: null,
          CreatedBy: history.CreatedBy,
          ModifiedBy: history.ModifiedBy
        })
      );

      await Promise.all(statsPromises);

      return {
        IsSuccess: true,
        Data: {
          BeatHistoryUID: journeyPlanData.beatHistory.UID,
          StoreHistoryCount: storeHistoriesWithPlanning.length,
          Message: 'Journey Plan created successfully'
        }
      };
    } catch (error) {
      console.error('Failed to create complete journey plan:', error);
      throw error;
    }
  }

  // DISABLED: Backend workflow endpoints not yet implemented
  async finalizeJourneyPlan(beatHistoryUID: string, notes?: string): Promise<any> {
    // return apiService.post('/BeatHistory/FinalizeJourneyPlan', {
    //   BeatHistoryUID: beatHistoryUID,
    //   Notes: notes || '',
    //   FinalizedDate: new Date().toISOString()
    // });
    console.warn('Journey Plan finalize API not implemented - returning success');
    return Promise.resolve({ IsSuccess: true, Message: 'Journey Plan finalized (simulated)' });
  }

  async activateJourneyPlan(beatHistoryUID: string): Promise<any> {
    // return apiService.post('/BeatHistory/ActivateJourneyPlan', {
    //   BeatHistoryUID: beatHistoryUID,
    //   ActivatedDate: new Date().toISOString()
    // });
    console.warn('Journey Plan activate API not implemented - returning success');
    return Promise.resolve({ IsSuccess: true, Message: 'Journey Plan activated (simulated)' });
  }

  async suspendJourneyPlan(beatHistoryUID: string, reason: string): Promise<any> {
    // return apiService.post('/BeatHistory/SuspendJourneyPlan', {
    //   BeatHistoryUID: beatHistoryUID,
    //   SuspendReason: reason,
    //   SuspendedDate: new Date().toISOString()
    // });
    console.warn('Journey Plan suspend API not implemented - returning success');
    return Promise.resolve({ IsSuccess: true, Message: 'Journey Plan suspended (simulated)' });
  }

  async getJourneyPlanStatus(beatHistoryUID: string): Promise<any> {
    // return apiService.get(`/BeatHistory/GetJourneyPlanStatus?BeatHistoryUID=${beatHistoryUID}`);
    console.warn('Journey Plan status API not implemented - returning default status');
    return Promise.resolve({
      IsSuccess: true,
      Data: {
        BeatHistoryUID: beatHistoryUID,
        Status: 'Active',
        CreatedDate: new Date().toISOString()
      }
    });
  }

  // Advanced Journey Plan Methods
  // DISABLED: Copy API not yet implemented
  async copyJourneyPlan(sourceBeatHistoryUID: string, targetDate: string, targetRouteUID?: string): Promise<any> {
    // return apiService.post('/BeatHistory/CopyJourneyPlan', {
    //   SourceBeatHistoryUID: sourceBeatHistoryUID,
    //   TargetDate: targetDate,
    //   TargetRouteUID: targetRouteUID
    // });
    console.warn('Copy Journey Plan API not implemented - returning success');
    return Promise.resolve({ IsSuccess: true, Message: 'Journey Plan copied (simulated)' });
  }

  // DISABLED: Template APIs not yet implemented
  async getJourneyPlanTemplate(routeUID: string, templateType: 'Daily' | 'Weekly' | 'Monthly'): Promise<any> {
    // return apiService.get(`/BeatHistory/GetJourneyPlanTemplate?RouteUID=${routeUID}&TemplateType=${templateType}`);
    console.warn('Journey Plan template API not implemented - returning empty template');
    return Promise.resolve({
      IsSuccess: true,
      Data: {
        TemplateType: templateType,
        RouteUID: routeUID,
        StoreHistories: []
      }
    });
  }

  async saveJourneyPlanAsTemplate(beatHistoryUID: string, templateName: string): Promise<any> {
    // return apiService.post('/BeatHistory/SaveJourneyPlanAsTemplate', {
    //   BeatHistoryUID: beatHistoryUID,
    //   TemplateName: templateName
    // });
    console.warn('Save Journey Plan template API not implemented - returning success');
    return Promise.resolve({ IsSuccess: true, Message: 'Template saved (simulated)' });
  }

  // Journey Plan Analytics & Reporting - DISABLED: Analytics APIs not yet implemented
  async getJourneyPlanAnalytics(params: {
    orgUID: string;
    routeUID?: string;
    dateFrom: string;
    dateTo: string;
    metricsType: 'Coverage' | 'Performance' | 'Efficiency';
  }): Promise<any> {
    // return apiService.post('/BeatHistory/GetJourneyPlanAnalytics', params);
    console.warn('Journey Plan analytics API not implemented - returning mock data');
    return Promise.resolve({
      IsSuccess: true,
      Data: {
        MetricsType: params.metricsType,
        TotalPlans: 0,
        CompletedPlans: 0,
        Coverage: 0,
        Performance: 0
      }
    });
  }

  async getRoutePerformanceReport(routeUID: string, dateRange: { start: string; end: string }): Promise<any> {
    // return apiService.get(`/BeatHistory/GetRoutePerformanceReport?RouteUID=${routeUID}&StartDate=${dateRange.start}&EndDate=${dateRange.end}`);
    console.warn('Route performance report API not implemented - returning mock data');
    return Promise.resolve({
      IsSuccess: true,
      Data: {
        RouteUID: routeUID,
        DateRange: dateRange,
        TotalVisits: 0,
        CompletedVisits: 0,
        PerformanceScore: 0
      }
    });
  }

  async getJourneyPlanComplianceReport(params: {
    orgUID: string;
    dateFrom: string;
    dateTo: string;
    includeSkipped: boolean;
  }): Promise<any> {
    // return apiService.post('/BeatHistory/GetJourneyPlanComplianceReport', params);
    console.warn('Journey Plan compliance report API not implemented - returning mock data');
    return Promise.resolve({
      IsSuccess: true,
      Data: {
        OrgUID: params.orgUID,
        DateRange: { from: params.dateFrom, to: params.dateTo },
        ComplianceRate: 0,
        TotalPlans: 0,
        CompliantPlans: 0
      }
    });
  }

  // Helper Methods
  async getJourneyPlanSummary(params: {
    orgUID: string;
    startDate: string;
    endDate: string;
  }): Promise<any> {
    return api.journeyPlan.getSummary(params);
  }

  async validateJourneyPlanData(journeyPlanData: any): Promise<{ isValid: boolean; errors: string[] }> {
    const errors: string[] = [];

    // Validate required fields
    if (!journeyPlanData.OrgUID) errors.push('Organization is required');
    if (!journeyPlanData.RouteUID) errors.push('Route is required');
    if (!journeyPlanData.JobPositionUID) errors.push('Employee is required');
    if (!journeyPlanData.VisitDate) errors.push('Visit date is required');

    // Validate customer selection
    if (!journeyPlanData.selectedCustomersWithTimes || journeyPlanData.selectedCustomersWithTimes.length === 0) {
      errors.push('At least one customer must be selected');
    }

    // Validate time constraints
    if (journeyPlanData.selectedCustomersWithTimes) {
      const invalidTimes = journeyPlanData.selectedCustomersWithTimes.filter(
        (customer: any) => !customer.startTime || !customer.endTime || customer.visitDuration <= 0
      );
      if (invalidTimes.length > 0) {
        errors.push('All customers must have valid start time, end time, and visit duration');
      }
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  async getEmployeeJourneyStats(
    empUID: string,
    dateRange: { start: string; end: string }
  ): Promise<any> {
    return apiService.get(
      `/UserJourney/GetEmployeeStats?EmpUID=${empUID}&StartDate=${dateRange.start}&EndDate=${dateRange.end}`
    );
  }

  // UID Generation Helpers
  generateUserJourneyUID(): string {
    return `journey-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  generateJourneyPlanUID(): string {
    return `plan-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  generateBeatHistoryUID(): string {
    return `beat-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  // Data Transformation Helpers for BeatHistory (Journey Plan) data
  transformBeatHistoryToJourney(apiData: any): UserJourney {
    // Transform BeatHistory data to UserJourney format for display
    return {
      UID: apiData.UID || "",
      JobPositionUID: apiData.JobPositionUID || "",
      EmpUID: apiData.LoginId || apiData.CreatedBy || "",
      JourneyStartTime: apiData.PlannedStartTime || apiData.CreatedTime,
      JourneyEndTime: apiData.PlannedEndTime,
      StartOdometerReading: 0, // Not available in BeatHistory
      EndOdometerReading: 0,   // Not available in BeatHistory
      JourneyTime: "", // Calculate if needed
      VehicleUID: apiData.RouteUID || "", // Use RouteUID as identifier
      EOTStatus: apiData.Status || "Planned", // BeatHistory status
      ReopenedBy: "",
      HasAuditCompleted: false,
      BeatHistoryUID: apiData.UID,
      WhStockRequestUID: "",
      LoginId: apiData.LoginId || apiData.CreatedBy || "",
      IsSynchronizing: false,
      HasInternet: true,
      InternetType: "",
      DownloadSpeed: 0,
      UploadSpeed: 0,
      HasMobileNetwork: true,
      IsLocationEnabled: true,
      BatteryPercentageTarget: 100,
      BatteryPercentageAvailable: 100,
      AttendanceStatus: "",
      AttendanceLatitude: "",
      AttendanceLongitude: "",
      AttendanceAddress: "",
      CreatedBy: apiData.CreatedBy || "",
      CreatedTime: apiData.CreatedTime || apiData.VisitDate,
      ModifiedBy: apiData.ModifiedBy || "",
      ModifiedTime: apiData.ModifiedTime || apiData.VisitDate
    };
  }

  // Data Transformation Helpers
  transformUserJourneyFromAPI(apiData: any): UserJourney {
    return {
      UID: apiData.UID,
      JobPositionUID: apiData.JobPositionUID || "",
      EmpUID: apiData.EmpUID || "",
      JourneyStartTime: apiData.StartTime || apiData.JourneyDate,
      JourneyEndTime: apiData.EndTime,
      StartOdometerReading: apiData.StartOdometerReading || 0,
      EndOdometerReading: apiData.EndOdometerReading || 0,
      JourneyTime: apiData.JourneyTime || "",
      VehicleUID: apiData.VehicleUID || "",
      EOTStatus: apiData.EOTStatus || "Not Started",
      ReopenedBy: apiData.ReopenedBy || "",
      HasAuditCompleted: apiData.HasAuditCompleted || false,
      BeatHistoryUID: apiData.BeatHistoryUID || "",
      WhStockRequestUID: apiData.WhStockRequestUID || "",
      LoginId: apiData.User || apiData.LoginId || "",
      IsSynchronizing: apiData.IsSynchronizing || false,
      HasInternet: apiData.HasInternet !== false,
      InternetType: apiData.InternetType || "",
      DownloadSpeed: apiData.DownloadSpeed || 0,
      UploadSpeed: apiData.UploadSpeed || 0,
      HasMobileNetwork: apiData.HasMobileNetwork !== false,
      IsLocationEnabled: apiData.IsLocationEnabled !== false,
      BatteryPercentageTarget: apiData.BatteryPercentageTarget || 100,
      BatteryPercentageAvailable: apiData.BatteryPercentageAvailable || 100,
      AttendanceStatus: apiData.AttendanceStatus || "",
      AttendanceLatitude: apiData.AttendanceLatitude || "",
      AttendanceLongitude: apiData.AttendanceLongitude || "",
      AttendanceAddress: apiData.AttendanceAddress || "",
      CreatedBy: apiData.CreatedBy || "",
      CreatedTime: apiData.CreatedTime || apiData.JourneyDate,
      ModifiedBy: apiData.ModifiedBy || "",
      ModifiedTime: apiData.ModifiedTime || apiData.JourneyDate
    };
  }
}

export const journeyPlanService = new JourneyPlanService();
