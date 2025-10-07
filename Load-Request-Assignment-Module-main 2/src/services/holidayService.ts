import { apiService } from './api';

export interface Holiday {
  UID: string;
  Name: string;
  Date: string;
  Type: 'National' | 'Regional' | 'Company' | 'Optional';
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
  sortCriterias?: Array<{ 
    columnName: string; 
    sortDirection: 'ASC' | 'DESC' 
  }>;
  filterCriterias?: Array<{ 
    columnName: string; 
    filterValue: string; 
    filterType: string;
  }>;
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
  private baseUrl = '';

  /**
   * Get holidays with pagination
   */
  async getHolidayDetails(pagingRequest: PagingRequest): Promise<PagedResponse<Holiday>> {
    try {
      const response = await apiService.post('/api/Holiday/GetHolidayDetails', pagingRequest);
      return response?.data || { pagedData: [], totalCount: 0 };
    } catch (error) {
      console.error('Error fetching holidays:', error);
      return { pagedData: [], totalCount: 0 };
    }
  }

  /**
   * Get holiday by organization UID
   */
  async getHolidayByOrgUID(orgUID: string): Promise<Holiday[]> {
    try {
      const response = await apiService.get('/api/Holiday/GetHolidayByOrgUID', {
        params: { UID: orgUID }
      });
      
      if (response?.data && !Array.isArray(response.data)) {
        return [response.data];
      }
      
      return response?.data || [];
    } catch (error) {
      console.error('Error fetching holidays by org:', error);
      return [];
    }
  }

  /**
   * Create holiday
   */
  async createHoliday(holiday: Partial<Holiday>): Promise<any> {
    try {
      const response = await apiService.post('/api/Holiday/CreateHoliday', holiday);
      return response?.data;
    } catch (error) {
      console.error('Error creating holiday:', error);
      throw error;
    }
  }

  /**
   * Update holiday
   */
  async updateHoliday(holiday: Holiday): Promise<any> {
    try {
      const response = await apiService.put('/api/Holiday/UpdateHoliday', holiday);
      return response?.data;
    } catch (error) {
      console.error('Error updating holiday:', error);
      throw error;
    }
  }

  /**
   * Delete holiday
   */
  async deleteHoliday(uid: string): Promise<any> {
    try {
      const response = await apiService.delete('/api/Holiday/DeleteHoliday', {
        params: { UID: uid }
      });
      return response?.data;
    } catch (error) {
      console.error('Error deleting holiday:', error);
      throw error;
    }
  }

  /**
   * Get holidays for a specific year
   */
  async getHolidaysByYear(year: number, orgUID?: string): Promise<Holiday[]> {
    try {
      const startDate = `${year}-01-01`;
      const endDate = `${year}-12-31`;
      
      const params: PagingRequest = {
        pageNumber: 1,
        pageSize: 100,
        isCountRequired: false,
        filterCriterias: [
          {
            columnName: 'Date',
            filterValue: startDate,
            filterType: 'GreaterThanOrEqual'
          },
          {
            columnName: 'Date',
            filterValue: endDate,
            filterType: 'LessThanOrEqual'
          }
        ],
        sortCriterias: [
          {
            columnName: 'Date',
            sortDirection: 'ASC'
          }
        ]
      };
      
      if (orgUID) {
        params.filterCriterias?.push({
          columnName: 'OrgUID',
          filterValue: orgUID,
          filterType: 'Equals'
        });
      }
      
      const response = await this.getHolidayDetails(params);
      return response.pagedData || [];
    } catch (error) {
      console.error('Error fetching holidays by year:', error);
      return [];
    }
  }

  /**
   * Get upcoming holidays
   */
  async getUpcomingHolidays(days: number, orgUID?: string): Promise<Holiday[]> {
    try {
      const today = new Date();
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + days);
      
      const params: PagingRequest = {
        pageNumber: 1,
        pageSize: 50,
        isCountRequired: false,
        filterCriterias: [
          {
            columnName: 'Date',
            filterValue: today.toISOString().split('T')[0],
            filterType: 'GreaterThanOrEqual'
          },
          {
            columnName: 'Date',
            filterValue: futureDate.toISOString().split('T')[0],
            filterType: 'LessThanOrEqual'
          },
          {
            columnName: 'IsActive',
            filterValue: 'true',
            filterType: 'Equals'
          }
        ],
        sortCriterias: [
          {
            columnName: 'Date',
            sortDirection: 'ASC'
          }
        ]
      };
      
      if (orgUID) {
        params.filterCriterias?.push({
          columnName: 'OrgUID',
          filterValue: orgUID,
          filterType: 'Equals'
        });
      }
      
      const response = await this.getHolidayDetails(params);
      return response.pagedData || [];
    } catch (error) {
      console.error('Error fetching upcoming holidays:', error);
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
      
      return holidays.some(h => 
        h.IsActive && 
        new Date(h.Date).toDateString() === new Date(date).toDateString()
      );
    } catch (error) {
      console.error('Error checking holiday:', error);
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
      const pagingRequest: PagingRequest = {
        PageNumber: 0,
        PageSize: 1,
        FilterCriterias: [
          { PropertyName: 'StoreUID', Value: storeUID, Operator: '=' }
        ],
        IsCountRequired: false
      };

      const response = await apiService.post(`/StoreWeekOff/SelectAllStoreWeekOff`, pagingRequest);

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
      console.error('Error fetching store week off:', error);
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
    if (options.customWeekendDays && options.customWeekendDays.includes(dayOfWeek)) {
      const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
      return { exclude: true, reason: `Custom weekend day: ${dayNames[dayOfWeek]}` };
    }
    
    // Check standard weekend exclusions
    if (options.excludeWeekends && (dayOfWeek === 0 || dayOfWeek === 6)) {
      return { exclude: true, reason: dayOfWeek === 0 ? 'Sunday' : 'Saturday' };
    }

    // Check store-specific weekend configuration
    if (options.storeUID) {
      const storeWeekOff = await this.getStoreWeekOff(options.storeUID);
      if (storeWeekOff) {
        const dayKeys = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'] as const;
        const dayKey = dayKeys[dayOfWeek];
        if (storeWeekOff[dayKey]) {
          const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
          return { exclude: true, reason: `Store week off: ${dayNames[dayOfWeek]}` };
        }
      }
    }
    
    // Check holiday exclusions
    if (options.excludeHolidays) {
      const isHoliday = await this.isDateHoliday(options.orgUID, date, options.holidayListUID);
      if (isHoliday) {
        return { exclude: true, reason: 'Holiday' };
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