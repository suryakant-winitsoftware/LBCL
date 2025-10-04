import { apiService } from "./api";

// Route interfaces
export interface RouteCustomer {
  storeUID: string;
  seqNo: number;
  visitTime: string;
  endTime: string;
  visitDuration: number;
  travelTime: number;
  isActive: boolean;
}

export interface RouteScheduleDaywise {
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
  saturday: boolean;
  sunday: boolean;
}

export interface RouteSchedule {
  type: "Daily" | "Weekly" | "Monthly" | "Fortnightly";
  plannedDays?: string; // For monthly schedules
  startDate?: string;
  endDate?: string;
}

export interface RouteMasterData {
  route: {
    code: string;
    name: string;
    description?: string;
    companyUID: string;
    orgUID: string;
    whOrgUID?: string;
    roleUID: string;
    jobPositionUID?: string;
    vehicleUID?: string;
    locationUID?: string;
    isActive: boolean;
    status: string;
    validFrom: string;
    validUpto: string;
    visitTime: string;
    endTime: string;
    visitDuration: number;
    travelTime: number;
    totalCustomers: number;
    printStanding?: boolean;
    printForward?: boolean;
    printTopup?: boolean;
    printOrderSummary?: boolean;
    autoFreezeJP?: boolean;
    addToRun?: boolean;
    autoFreezeRunTime?: string;
    isCustomerWithTime?: boolean;
  };
  routeSchedule: RouteSchedule;
  routeScheduleDaywise?: RouteScheduleDaywise;
  routeCustomers: RouteCustomer[];
  routeUsers?: Array<{
    empUID: string;
    isPrimary: boolean;
  }>;
}

export const routeServiceFixed = {
  /**
   * Create complete route with schedule, customers, and users
   * This is the main API to create a route with all components
   */
  async createRouteMaster(data: RouteMasterData) {
    try {
      const response = await apiService.post("/Route/CreateRouteMaster", data);
      return response;
    } catch (error) {
      console.error("Error creating route master:", error);
      throw error;
    }
  },

  /**
   * Update existing route details
   */
  async updateRoute(routeData: any) {
    try {
      const response = await apiService.put(
        "/Route/UpdateRouteDetails",
        routeData
      );
      return response;
    } catch (error) {
      console.error("Error updating route:", error);
      throw error;
    }
  },

  /**
   * Get all routes with pagination and filters
   */
  async getRoutes(params: {
    orgUID: string;
    pageNumber?: number;
    pageSize?: number;
    isCountRequired?: boolean;
    sortCriterias?: Array<{
      columnName: string;
      sortDirection: "ASC" | "DESC";
    }>;
    filterCriterias?: Array<{
      columnName: string;
      filterValue: string;
      filterType: string;
    }>;
  }) {
    try {
      const response = await apiService.post("/Route/SelectAllRouteDetails", {
        orgUID: params.orgUID,
        pageNumber: params.pageNumber || 1,
        pageSize: params.pageSize || 50,
        isCountRequired: params.isCountRequired ?? true,
        sortCriterias: params.sortCriterias || [],
        filterCriterias: params.filterCriterias || []
      });
      return response;
    } catch (error) {
      console.error("Error fetching routes:", error);
      throw error;
    }
  },

  /**
   * Get single route details by UID
   */
  async getRouteByUID(uid: string) {
    try {
      const response = await apiService.get(
        `/Route/SelectRouteDetailByUID?UID=${uid}`
      );
      return response;
    } catch (error) {
      console.error("Error fetching route details:", error);
      throw error;
    }
  },

  /**
   * Delete route
   */
  async deleteRoute(uid: string) {
    try {
      const response = await apiService.delete(
        `/Route/DeleteRouteDetail?UID=${uid}`
      );
      return response;
    } catch (error) {
      console.error("Error deleting route:", error);
      throw error;
    }
  },

  /**
   * Get route master view (includes all related data)
   */
  async getRouteMasterView(uid: string) {
    try {
      const response = await apiService.get(
        `/Route/SelectRouteMasterViewByUID?UID=${uid}`
      );
      return response;
    } catch (error) {
      console.error("Error fetching route master view:", error);
      throw error;
    }
  },

  /**
   * Add customers to route
   */
  async addCustomersToRoute(routeUID: string, customers: RouteCustomer[]) {
    try {
      // Try multiple endpoints for adding customers
      try {
        // First try batch add
        const response = await apiService.post(
          "/RouteCustomer/AddMultipleCustomers",
          {
            routeUID,
            customers
          }
        );
        return response;
      } catch (batchError) {
        // If batch fails, try adding individually
        const results = [];
        for (const customer of customers) {
          try {
            const response = await apiService.post(
              "/RouteCustomer/CreateRouteCustomerDetails",
              {
                routeUID,
                storeUID: customer.storeUID,
                seqNo: customer.seqNo,
                visitTime: customer.visitTime,
                endTime: customer.endTime,
                visitDuration: customer.visitDuration,
                travelTime: customer.travelTime,
                isActive: customer.isActive
              }
            );
            results.push(response);
          } catch (individualError) {
            console.error("Error adding individual customer:", individualError);
          }
        }

        // Update route total customers count
        const routeData = await this.getRouteByUID(routeUID);
        await this.updateRoute({
          ...routeData,
          totalCustomers: customers.length
        });

        return { success: true, data: results };
      }
    } catch (error) {
      console.error("Error adding customers to route:", error);
      throw error;
    }
  },

  /**
   * Get route customers
   */
  async getRouteCustomers(routeUID: string) {
    try {
      // Try primary endpoint
      const response = await apiService.get(
        `/RouteCustomer/GetRouteCustomers?routeUID=${routeUID}`
      );
      return response?.data || response || [];
    } catch (error) {
      // Try alternative endpoint
      try {
        const response = await apiService.post(
          "/RouteCustomer/SelectAllRouteCustomerDetails",
          {
            pageNumber: 1,
            pageSize: 100,
            isCountRequired: false,
            filterCriterias: [
              {
                columnName: "RouteUID",
                filterValue: routeUID,
                filterType: "Equals"
              }
            ]
          }
        );
        return response?.data?.pagedData || [];
      } catch (alternativeError) {
        console.error("Error fetching route customers:", error);
        return [];
      }
    }
  },

  /**
   * Update route customers
   */
  async updateRouteCustomers(routeUID: string, customers: RouteCustomer[]) {
    try {
      // Try to use specific endpoint if it exists
      const response = await apiService.put(
        "/RouteCustomer/UpdateRouteCustomers",
        {
          routeUID,
          customers
        }
      );
      return response;
    } catch (error) {
      console.error("Error updating route customers:", error);
      // Fallback: use add customers approach
      throw error;
    }
  },

  /**
   * Get vehicles for dropdown
   */
  async getVehicles(orgUID: string) {
    try {
      const response = await apiService.get(
        `/Vehicle/GetVehiclesByOrg?orgUID=${orgUID}`
      );
      return response;
    } catch (error) {
      console.error("Error fetching vehicles:", error);
      return [];
    }
  },

  /**
   * Get employees/salesmen for dropdown
   */
  async getEmployees(orgUID: string) {
    try {
      const response = await apiService.get(
        `/Employee/GetEmployeesByOrg?orgUID=${orgUID}`
      );
      return response;
    } catch (error) {
      console.error("Error fetching employees:", error);
      return [];
    }
  },

  /**
   * Get roles for dropdown
   */
  async getRoles(orgUID: string) {
    try {
      const response = await apiService.get(
        `/Role/GetRolesByOrg?orgUID=${orgUID}`
      );
      return response;
    } catch (error) {
      console.error("Error fetching roles:", error);
      return [];
    }
  },

  /**
   * Get route change log
   */
  async getRouteChangeLog(params: {
    pageNumber?: number;
    pageSize?: number;
    isCountRequired?: boolean;
    filterCriterias?: any[];
    sortCriterias?: any[];
  }) {
    try {
      const response = await apiService.post(
        "/Route/SelectRouteChangeLogAllDetails",
        {
          pageNumber: params.pageNumber || 1,
          pageSize: params.pageSize || 50,
          isCountRequired: params.isCountRequired ?? true,
          filterCriterias: params.filterCriterias || [],
          sortCriterias: params.sortCriterias || []
        }
      );
      return response;
    } catch (error) {
      console.error("Error fetching route change log:", error);
      throw error;
    }
  }
};
