import { apiService } from "./api";

export interface Holiday {
  UID: string;
  Name: string;
  Date: string;
  Type: "National" | "Regional" | "Company" | "Optional";
  IsRecurring: boolean;
  Description?: string;
  ApplicableOrgUIDs?: string[];
  ApplicableRegions?: string[];
  IsActive: boolean;
  OrgUID?: string;
  CompanyUID?: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  HolidayListUID?: string;
  HolidayDate?: string;
  IsOptional?: boolean;
  Year?: number;
}

export interface HolidayList {
  UID: string;
  CompanyUID: string;
  OrgUID: string;
  Name: string;
  Description: string;
  LocationUID: string;
  IsActive: boolean;
  Year: number;
}

export interface HolidayListDetails {
  UID: string;
  CompanyUID: string;
  OrgUID: string;
  Name: string;
  Description: string;
  LocationUID: string;
  IsActive: boolean;
  Year: number;
  Holidays: Holiday[];
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  filterCriterias?: any[];  // Keep it flexible, but we'll always send empty array
  isCountRequired: boolean;
}

export interface PagedResponse<T> {
  pagedData: T[];
  totalCount: number;
}

export interface ApiResponse<T = unknown> {
  data?: T;
  status?: string;
  message?: string;
  error?: string;
}

class HolidayService {

  /**
   * Get holidays with pagination
   */
  async getHolidayDetails(
    pagingRequest: PagingRequest
  ): Promise<PagedResponse<Holiday>> {
    try {
      // Ensure we always send clean request without null filters
      const cleanRequest = {
        pageNumber: pagingRequest.pageNumber || 1,
        pageSize: pagingRequest.pageSize || 10,
        filterCriterias: [],  // Always empty array, no filters
        isCountRequired: pagingRequest.isCountRequired || true
      };
      
      const response = await apiService.post(
        "/Holiday/GetHolidayDetails",
        cleanRequest
      );
      
      // Backend returns: {Data: {PagedData: [...], TotalCount: n}, StatusCode: 200, IsSuccess: true}
      if (response?.IsSuccess && response?.Data) {
        return {
          pagedData: response.Data.PagedData || [],
          totalCount: response.Data.TotalCount || 0
        };
      }
      
      return { pagedData: [], totalCount: 0 };
    } catch (error: any) {
      // Error is already logged in api.ts, just return empty data
      return { pagedData: [], totalCount: 0 };
    }
  }

  /**
   * Get holiday by organization UID
   */
  async getHolidayByOrgUID(orgUID: string): Promise<Holiday[]> {
    try {
      const response: any = await apiService.get(`/Holiday/GetHolidayByOrgUID?UID=${orgUID}`);

      console.log("Get Holiday By OrgUID Response:", response);

      if (response?.Data && !Array.isArray(response.Data)) {
        return [response.Data];
      } else if (response?.Data && Array.isArray(response.Data)) {
        return response.Data;
      }

      return [];
    } catch (error) {
      console.error("Error fetching holidays by org:", error);
      return [];
    }
  }

  /**
   * Create holiday
   */
  async createHoliday(holiday: Partial<Holiday>): Promise<any> {
    try {
      const response = await apiService.post("/Holiday/CreateHoliday", holiday);
      console.log("Create Holiday Response:", response);
      return response;
    } catch (error) {
      console.error("Error creating holiday:", error);
      throw error;
    }
  }

  /**
   * Update holiday
   */
  async updateHoliday(holiday: Partial<Holiday>): Promise<any> {
    try {
      const response = await apiService.put("/Holiday/UpdateHoliday", holiday);
      console.log("Update Holiday Response:", response);
      return response;
    } catch (error) {
      console.error("Error updating holiday:", error);
      throw error;
    }
  }

  /**
   * Delete holiday
   */
  async deleteHoliday(uid: string): Promise<any> {
    try {
      const response = await apiService.delete(`/Holiday/DeleteHoliday?UID=${uid}`);
      console.log("Delete Holiday Response:", response);
      return response;
    } catch (error) {
      console.error("Error deleting holiday:", error);
      throw error;
    }
  }

  /**
   * Get holidays for a specific year
   */
  async getHolidaysByYear(year: number, _orgUID?: string): Promise<Holiday[]> {
    try {
      const params: PagingRequest = {
        pageNumber: 1,  // Use 1-based pagination as per your request
        pageSize: 100,
        isCountRequired: true
      };

      const response = await this.getHolidayDetails(params);
      
      // Filter by year on frontend since backend has issues with date filters
      const holidays = response.pagedData || [];
      return holidays.filter(holiday => {
        const holidayDate = new Date(holiday.HolidayDate || holiday.Date || '');
        return holidayDate.getFullYear() === year;
      });
    } catch (error) {
      console.error("Error fetching holidays by year:", error);
      return [];
    }
  }

  /**
   * Get upcoming holidays
   */
  async getUpcomingHolidays(days: number, _orgUID?: string): Promise<Holiday[]> {
    try {
      const today = new Date();
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + days);

      const params: PagingRequest = {
        pageNumber: 1,
        pageSize: 200,  // Get more records since we'll filter on frontend
        isCountRequired: false
      };

      const response = await this.getHolidayDetails(params);
      
      // Filter on frontend
      const holidays = response.pagedData || [];
      const todayTime = today.getTime();
      const futureTime = futureDate.getTime();
      
      return holidays.filter(holiday => {
        const holidayDate = new Date(holiday.HolidayDate || holiday.Date || '');
        const holidayTime = holidayDate.getTime();
        return holidayTime >= todayTime && holidayTime <= futureTime && holiday.IsActive !== false;
      });
    } catch (error) {
      console.error("Error fetching upcoming holidays:", error);
      return [];
    }
  }

  /**
   * Check if date is holiday
   */
  async isHoliday(date: string, orgUID: string): Promise<boolean> {
    try {
      const year = new Date(date).getFullYear();
      const holidays = await this.getHolidaysByYear(year, orgUID);

      return holidays.some(
        (h) =>
          h.IsActive &&
          new Date(h.Date).toDateString() === new Date(date).toDateString()
      );
    } catch (error) {
      console.error("Error checking holiday:", error);
      return false;
    }
  }

  /**
   * Get weekend configuration for stores (from existing StoreWeekOff API)
   */
  async getStoreWeekOff(storeUID: string): Promise<{
    Sun: boolean;
    Mon: boolean;
    Tue: boolean;
    Wed: boolean;
    Thu: boolean;
    Fri: boolean;
    Sat: boolean;
  } | null> {
    try {
      const pagingRequest: any = {
        PageNumber: 0,
        PageSize: 1,
        FilterCriterias: [
          { PropertyName: "StoreUID", Value: storeUID, Operator: "=" }
        ],
        IsCountRequired: false
      };

      const response: any = await apiService.post(
        `/StoreWeekOff/SelectAllStoreWeekOff`,
        pagingRequest
      );

      if (response?.IsSuccess && response.Data?.PagedData?.length > 0) {
        const weekOff = response.Data.PagedData[0];
        return {
          Sun: weekOff.Sun,
          Mon: weekOff.Mon,
          Tue: weekOff.Tue,
          Wed: weekOff.Wed,
          Thu: weekOff.Thu,
          Fri: weekOff.Fri,
          Sat: weekOff.Sat
        };
      }

      return null;
    } catch (error) {
      console.error("Error fetching store week off:", error);
      return null;
    }
  }

  /**
   * Check if a date should be excluded based on weekend and holiday settings
   */
  async shouldExcludeDate(
    date: Date,
    options: {
      orgUID: string;
      excludeWeekends?: boolean;
      excludeHolidays?: boolean;
      holidayListUID?: string;
      customWeekendDays?: number[]; // 0=Sunday, 1=Monday, etc.
      storeUID?: string; // For store-specific weekend configuration
    }
  ): Promise<{ exclude: boolean; reason?: string }> {
    const dayOfWeek = date.getDay();

    // Check custom weekend exclusions
    if (
      options.customWeekendDays &&
      options.customWeekendDays.includes(dayOfWeek)
    ) {
      const dayNames = [
        "Sunday",
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday"
      ];
      return {
        exclude: true,
        reason: `Custom weekend day: ${dayNames[dayOfWeek]}`
      };
    }

    // Check standard weekend exclusions
    if (options.excludeWeekends && (dayOfWeek === 0 || dayOfWeek === 6)) {
      return { exclude: true, reason: dayOfWeek === 0 ? "Sunday" : "Saturday" };
    }

    // Check store-specific weekend configuration
    if (options.storeUID) {
      const storeWeekOff = await this.getStoreWeekOff(options.storeUID);
      if (storeWeekOff) {
        const dayKeys = [
          "Sun",
          "Mon",
          "Tue",
          "Wed",
          "Thu",
          "Fri",
          "Sat"
        ] as const;
        const dayKey = dayKeys[dayOfWeek];
        if (storeWeekOff[dayKey]) {
          const dayNames = [
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
          ];
          return {
            exclude: true,
            reason: `Store week off: ${dayNames[dayOfWeek]}`
          };
        }
      }
    }

    // Check holiday exclusions
    if (options.excludeHolidays) {
      const isHoliday = await this.isHoliday(
        date.toISOString().split('T')[0],
        options.orgUID
      );
      if (isHoliday) {
        return { exclude: true, reason: "Holiday" };
      }
    }

    return { exclude: false };
  }

  /**
   * Generate list of valid dates within a range, excluding weekends/holidays as specified
   */
  async generateValidDates(
    startDate: Date,
    endDate: Date,
    options: {
      orgUID: string;
      excludeWeekends?: boolean;
      excludeHolidays?: boolean;
      holidayListUID?: string;
      customWeekendDays?: number[];
      storeUID?: string;
    }
  ): Promise<Date[]> {
    const validDates: Date[] = [];
    const currentDate = new Date(startDate);

    while (currentDate <= endDate) {
      const shouldExclude = await this.shouldExcludeDate(currentDate, options);

      if (!shouldExclude.exclude) {
        validDates.push(new Date(currentDate));
      }

      currentDate.setDate(currentDate.getDate() + 1);
    }

    return validDates;
  }
}

export const holidayService = new HolidayService();
