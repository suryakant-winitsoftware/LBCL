/**
 * Optimized Data Loader for Journey Plan Creation
 * Handles parallel loading, caching, and progressive data fetching
 */

import { api } from '@/services/api';
import { cacheService, CACHE_KEYS } from './cacheService';
import { organizationService } from './organizationService';

interface LoadingState {
  organizations: boolean;
  routes: boolean;
  employees: boolean;
  vehicles: boolean;
  locations: boolean;
  customers: boolean;
}

interface DataLoadResult<T> {
  data: T;
  fromCache: boolean;
  loadTime: number;
}

class JourneyPlanDataLoader {
  private loadingPromises: Map<string, Promise<any>> = new Map();

  /**
   * Load all initial data in parallel
   */
  async loadInitialData(): Promise<{
    organizations: any[];
    orgTypes: any[];
    loadTime: number;
  }> {
    const startTime = Date.now();

    // Load organizations and types in parallel
    const [orgsResult, typesResult] = await Promise.all([
      this.loadOrganizations(),
      this.loadOrganizationTypes(),
    ]);

    return {
      organizations: orgsResult.data,
      orgTypes: typesResult.data,
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Load organization-specific data in parallel
   */
  async loadOrgData(orgUID: string): Promise<{
    routes: any[];
    employees: any[];
    vehicles: any[];
    locations: any[];
    loadTime: number;
  }> {
    const startTime = Date.now();

    // Check if we're already loading this data
    const loadingKey = `org:${orgUID}`;
    if (this.loadingPromises.has(loadingKey)) {
      console.log('[DataLoader] Waiting for existing org data load...');
      return this.loadingPromises.get(loadingKey);
    }

    // Create loading promise
    const loadPromise = this.performOrgDataLoad(orgUID);
    this.loadingPromises.set(loadingKey, loadPromise);

    try {
      const result = await loadPromise;
      return {
        ...result,
        loadTime: Date.now() - startTime,
      };
    } finally {
      this.loadingPromises.delete(loadingKey);
    }
  }

  private async performOrgDataLoad(orgUID: string) {
    // Load all org-specific data in parallel
    const [routes, employees, vehicles, locations] = await Promise.all([
      this.loadRoutes(orgUID),
      this.loadEmployees(orgUID),
      this.loadVehicles(orgUID),
      this.loadLocations(orgUID),
    ]);

    return {
      routes: routes.data,
      employees: employees.data,
      vehicles: vehicles.data,
      locations: locations.data,
    };
  }

  /**
   * Load routes with caching
   */
  async loadRoutes(orgUID: string): Promise<DataLoadResult<any[]>> {
    const cacheKey = CACHE_KEYS.routes(orgUID);
    const startTime = Date.now();

    const data = await cacheService.getOrFetch(
      cacheKey,
      async () => {
        try {
          // Try primary API first
          const response = await api.dropdown.getRoute(orgUID);
          if (response?.IsSuccess && response?.Data) {
            return response.Data.filter((route: any) => route.UID && route.Label);
          }
        } catch (error) {
          console.warn('[DataLoader] Primary route API failed, trying fallback');
        }

        // Fallback API
        const response = await api.route.getUserDDL(orgUID);
        if (response?.IsSuccess && response?.Data) {
          return response.Data.filter((route: any) => route.UID && route.Label);
        }
        return [];
      },
      10 * 60 * 1000 // Cache for 10 minutes
    );

    return {
      data: data || [],
      fromCache: Date.now() - startTime < 100, // If loaded in < 100ms, likely from cache
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Load employees with caching
   */
  async loadEmployees(orgUID: string): Promise<DataLoadResult<any[]>> {
    const cacheKey = CACHE_KEYS.employees(orgUID);
    const startTime = Date.now();

    const data = await cacheService.getOrFetch(
      cacheKey,
      async () => {
        try {
          const response = await api.dropdown.getEmployee(orgUID);
          if (response?.IsSuccess && response?.Data) {
            return response.Data.map((emp: any) => ({
              JobPositionUID: emp.UID,
              LoginId: emp.Code,
              Name: emp.Label,
              Mobile: emp.ExtData?.Mobile || "",
              Status: "Active",
              RouteUID: emp.ExtData?.RouteUID,
            }));
          }
        } catch (error) {
          console.warn('[DataLoader] Employee API failed');
        }
        return [];
      },
      10 * 60 * 1000 // Cache for 10 minutes
    );

    return {
      data: data || [],
      fromCache: Date.now() - startTime < 100,
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Load vehicles with caching
   */
  async loadVehicles(orgUID: string): Promise<DataLoadResult<any[]>> {
    const cacheKey = CACHE_KEYS.vehicles(orgUID);
    const startTime = Date.now();

    const data = await cacheService.getOrFetch(
      cacheKey,
      async () => {
        try {
          const response = await api.dropdown.getVehicle(orgUID);
          if (response?.IsSuccess && response?.Data) {
            return response.Data.map((vehicle: any) => ({
              UID: vehicle.UID,
              Name: vehicle.Label,
              Code: vehicle.Code,
              VehicleNo: vehicle.ExtData?.VehicleNo || "",
              RegistrationNo: vehicle.ExtData?.RegistrationNo || "",
            }));
          }
        } catch (error) {
          console.warn('[DataLoader] Vehicle API failed');
        }
        return [];
      },
      15 * 60 * 1000 // Cache for 15 minutes
    );

    return {
      data: data || [],
      fromCache: Date.now() - startTime < 100,
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Load locations with caching
   */
  async loadLocations(orgUID: string): Promise<DataLoadResult<any[]>> {
    const cacheKey = CACHE_KEYS.locations(orgUID);
    const startTime = Date.now();

    const data = await cacheService.getOrFetch(
      cacheKey,
      async () => {
        try {
          const response = await api.location.getByTypes(["Region", "Area", "Territory"]);
          if (response?.IsSuccess && response?.Data) {
            return Array.isArray(response.Data) 
              ? response.Data.map((loc: any) => ({
                  UID: loc.UID,
                  Name: loc.Name || loc.LocationName,
                  Code: loc.Code || loc.LocationCode,
                  Type: loc.Type || loc.LocationType,
                }))
              : [];
          }
        } catch (error) {
          console.warn('[DataLoader] Location API failed');
        }
        return [];
      },
      15 * 60 * 1000 // Cache for 15 minutes
    );

    return {
      data: data || [],
      fromCache: Date.now() - startTime < 100,
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Load route customers with caching - supports date-based flexible scheduling
   */
  async loadRouteCustomers(routeUID: string, visitDate?: Date | string): Promise<DataLoadResult<any[]>> {
    const dateKey = visitDate ? `_${typeof visitDate === 'string' ? visitDate : visitDate.toISOString().split('T')[0]}` : '';
    const cacheKey = CACHE_KEYS.customers(routeUID) + dateKey;
    const startTime = Date.now();

    const data = await cacheService.getOrFetch(
      cacheKey,
      async () => {
        try {
          // If visitDate is provided, use the new flexible scheduling API
          if (visitDate) {
            const formattedDate = typeof visitDate === 'string' 
              ? visitDate 
              : visitDate.toISOString().split('T')[0];
            
            console.log('[DataLoader] Loading customers for route and date:', routeUID, formattedDate);
            
            try {
              // Try the new flexible scheduling API first
              const response = await api.routeCustomerSchedule.getForDate(routeUID, formattedDate);
              console.log('[DataLoader] Flexible schedule API response:', response);
              
              if (response?.IsSuccess && response?.Data) {
                const customers = Array.isArray(response.Data) 
                  ? response.Data.map((item: any, index: number) => ({
                      UID: item.store_uid || item.StoreUID || '',
                      Code: item.store_code || `STORE_${index + 1}`,
                      Name: item.store_name || `Store ${item.store_uid || item.StoreUID}`,
                      Address: item.address || '',
                      ContactNo: item.contact || '',
                      Type: 'Store',
                      Status: 'Active',
                      SeqNo: item.seq_no || item.SeqNo || index + 1,
                      VisitTime: item.visit_time || item.VisitTime,
                      VisitDuration: item.visit_duration || item.VisitDuration || 30,
                      EndTime: item.end_time || item.EndTime,
                      TravelTime: item.travel_time || item.TravelTime || 15,
                      ScheduleType: 'FLEXIBLE' // Indicate this came from flexible scheduling
                    }))
                  : [];
                
                console.log('[DataLoader] Processed flexible schedule customers:', customers);
                return customers;
              }
            } catch (flexError) {
              console.warn('[DataLoader] Flexible scheduling API not available, falling back to standard API:', flexError);
            }
          }
          
          // Fallback to standard route customers API
          console.log('[DataLoader] Loading all customers for route:', routeUID);
          const response = await api.dropdown.getCustomersByRoute(routeUID);
          console.log('[DataLoader] Standard customer API response:', response);
          
          if (response?.IsSuccess && response?.Data) {
            const customers = Array.isArray(response.Data) 
              ? response.Data.map((cust: any, index: number) => {
                  console.log('[DataLoader] Processing customer:', cust);
                  return {
                    UID: cust.UID || '',
                    Code: cust.Code || '',
                    Name: cust.Label || cust.Code || 'Unknown Store',
                    Address: '', // Backend doesn't provide this, set to empty
                    ContactNo: '', // Backend doesn't provide this, set to empty
                    Type: 'Store', // Default type since backend doesn't provide
                    Status: 'Active', // Default status since backend doesn't provide
                    SeqNo: index + 1, // Generate sequence number
                    ScheduleType: 'STANDARD' // Indicate this came from standard API
                  };
                })
              : [];
            
            console.log('[DataLoader] Processed standard customers:', customers);
            return customers;
          } else {
            console.log('[DataLoader] No customer data received or API failed');
            return [];
          }
        } catch (error) {
          console.error('[DataLoader] Customer API failed:', error);
          return [];
        }
      },
      10 * 60 * 1000 // Cache for 10 minutes
    );

    return {
      data: data || [],
      fromCache: Date.now() - startTime < 100,
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Load organizations with caching
   */
  private async loadOrganizations(): Promise<DataLoadResult<any[]>> {
    const cacheKey = CACHE_KEYS.organizations();
    const startTime = Date.now();

    const data = await cacheService.getOrFetch(
      cacheKey,
      async () => {
        const result = await organizationService.getOrganizations(1, 1000);
        return result.data;
      },
      30 * 60 * 1000 // Cache for 30 minutes
    );

    return {
      data: data || [],
      fromCache: Date.now() - startTime < 100,
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Load organization types with caching
   */
  private async loadOrganizationTypes(): Promise<DataLoadResult<any[]>> {
    const cacheKey = CACHE_KEYS.orgTypes();
    const startTime = Date.now();

    const data = await cacheService.getOrFetch(
      cacheKey,
      async () => {
        return await organizationService.getOrganizationTypes();
      },
      60 * 60 * 1000 // Cache for 1 hour
    );

    return {
      data: data || [],
      fromCache: Date.now() - startTime < 100,
      loadTime: Date.now() - startTime,
    };
  }

  /**
   * Prefetch data for next likely actions
   */
  prefetchRouteData(routeUIDs: string[]): void {
    // Prefetch customer data for routes in background
    routeUIDs.forEach(routeUID => {
      const cacheKey = CACHE_KEYS.customers(routeUID);
      if (!cacheService.has(cacheKey)) {
        this.loadRouteCustomers(routeUID).catch(error => 
          console.error('[DataLoader] Prefetch failed:', error)
        );
      }
    });
  }

  /**
   * Clear all caches
   */
  clearCache(): void {
    cacheService.clear();
    console.log('[DataLoader] All caches cleared');
  }

  /**
   * Clear specific org cache
   */
  clearOrgCache(orgUID: string): void {
    cacheService.clear(CACHE_KEYS.routes(orgUID));
    cacheService.clear(CACHE_KEYS.employees(orgUID));
    cacheService.clear(CACHE_KEYS.vehicles(orgUID));
    cacheService.clear(CACHE_KEYS.locations(orgUID));
    console.log(`[DataLoader] Cleared cache for org: ${orgUID}`);
  }
}

// Singleton instance
export const journeyPlanDataLoader = new JourneyPlanDataLoader();