import { apiService } from './api';

export interface RouteSchedule {
  UID: string;
  CompanyUID?: string;
  RouteUID: string;
  Name: string;
  Type: 'Daily' | 'Weekly' | 'Fortnightly' | 'Monthly' | 'WeeklyCycle';
  StartDate?: string;
  Status: number;
  FromDate?: string;
  ToDate?: string;
  StartTime?: string;
  EndTime?: string;
  VisitDurationInMinutes: number;
  TravelTimeInMinutes: number;
  NextBeatDate?: string;
  LastBeatDate?: string;
  AllowMultipleBeatsPerDay: boolean;
  PlannedDays?: string;
  CycleWeeks?: number;
  PatternType?: string;
  CreatedBy?: string;
  ModifiedBy?: string;
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
  SS?: number;
  CreatedBy?: string;
  ModifiedBy?: string;
}

export interface RouteScheduleFortnight {
  UID: string;
  CompanyUID?: string;
  RouteScheduleUID: string;
  Monday: number;
  Tuesday: number;
  Wednesday: number;
  Thursday: number;
  Friday: number;
  Saturday: number;
  Sunday: number;
  MondayFN: number;
  TuesdayFN: number;
  WednesdayFN: number;
  ThursdayFN: number;
  FridayFN: number;
  SaturdayFN: number;
  SundayFN: number;
}

export interface RouteScheduleWeeklyCycle {
  UID: string;
  CompanyUID?: string;
  RouteScheduleUID: string;
  WeekNumber: number;
  Monday: number;
  Tuesday: number;
  Wednesday: number;
  Thursday: number;
  Friday: number;
  Saturday: number;
  Sunday: number;
  CycleLength: number;
  IsActive: number;
  CreatedBy?: string;
  ModifiedBy?: string;
}

export interface GenerateJourneyPlanRequest {
  RouteScheduleUID: string;
  OrgUID: string;
  JobPositionUID: string;
  LoginId: string;
  StartDate?: string;
  EndDate?: string;
  IncludeHolidays: boolean;
  CustomerUIDs?: string[];
  CreatedBy: string;
}

class RouteScheduleService {
  // Create a new route schedule
  async createRouteSchedule(schedule: RouteSchedule): Promise<any> {
    return apiService.post('/RouteSchedule/CreateRouteSchedule', schedule);
  }

  // Get route schedules by route UID
  async getRouteSchedulesByRouteUID(routeUID: string): Promise<RouteSchedule[]> {
    try {
      const response = await apiService.get(`/RouteSchedule/GetRouteScheduleByRouteUID?routeUID=${routeUID}`);
      return response?.Data || [];
    } catch (error) {
      console.error('Error fetching route schedules:', error);
      return [];
    }
  }

  // Get route schedule by UID
  async getRouteScheduleByUID(uid: string): Promise<RouteSchedule | null> {
    try {
      const response = await apiService.get(`/RouteSchedule/GetRouteScheduleByUID?UID=${uid}`);
      return response?.Data || null;
    } catch (error) {
      console.error('Error fetching route schedule:', error);
      return null;
    }
  }

  // Update route schedule
  async updateRouteSchedule(schedule: RouteSchedule): Promise<any> {
    return apiService.post('/RouteSchedule/UpdateRouteSchedule', schedule);
  }

  // Delete route schedule
  async deleteRouteSchedule(uid: string): Promise<any> {
    return apiService.delete(`/RouteSchedule/DeleteRouteSchedule?UID=${uid}`);
  }

  // Create route schedule daywise configuration
  async createRouteScheduleDaywise(daywise: RouteScheduleDaywise): Promise<any> {
    return apiService.post('/RouteSchedule/CreateRouteScheduleDaywise', daywise);
  }

  // Get route schedule daywise by schedule UID
  async getRouteScheduleDaywise(scheduleUID: string): Promise<RouteScheduleDaywise | null> {
    try {
      const response = await apiService.get(`/RouteSchedule/GetRouteScheduleDaywise?RouteScheduleUID=${scheduleUID}`);
      return response?.Data || null;
    } catch (error) {
      console.error('Error fetching daywise schedule:', error);
      return null;
    }
  }

  // Update route schedule daywise
  async updateRouteScheduleDaywise(daywise: RouteScheduleDaywise): Promise<any> {
    return apiService.post('/RouteSchedule/UpdateRouteScheduleDaywise', daywise);
  }

  // Get route schedule weekly cycle by schedule UID
  async getRouteScheduleWeeklyCycle(scheduleUID: string): Promise<RouteScheduleWeeklyCycle[]> {
    try {
      const response = await apiService.get(`/RouteSchedule/GetRouteScheduleWeeklyCycle?RouteScheduleUID=${scheduleUID}`);
      return response?.Data || [];
    } catch (error) {
      console.error('Error fetching weekly cycle schedule:', error);
      return [];
    }
  }

  // Create route schedule weekly cycle configuration
  async createRouteScheduleWeeklyCycle(weeklyCycle: RouteScheduleWeeklyCycle): Promise<any> {
    return apiService.post('/RouteSchedule/CreateRouteScheduleWeeklyCycle', weeklyCycle);
  }

  // Update route schedule weekly cycle
  async updateRouteScheduleWeeklyCycle(weeklyCycle: RouteScheduleWeeklyCycle): Promise<any> {
    return apiService.post('/RouteSchedule/UpdateRouteScheduleWeeklyCycle', weeklyCycle);
  }

  // Generate journey plans from schedule
  async generateJourneyPlansFromSchedule(request: GenerateJourneyPlanRequest): Promise<any> {
    return apiService.post('/RouteSchedule/GenerateJourneyPlansFromSchedule', request);
  }

  // Calculate next beat date based on schedule type
  calculateNextBeatDate(schedule: RouteSchedule, lastBeatDate?: Date): Date {
    const baseDate = lastBeatDate || new Date();
    
    switch (schedule.Type) {
      case 'Daily':
        return new Date(baseDate.getTime() + 24 * 60 * 60 * 1000);
      case 'Weekly':
        return new Date(baseDate.getTime() + 7 * 24 * 60 * 60 * 1000);
      case 'Fortnightly':
        return new Date(baseDate.getTime() + 14 * 24 * 60 * 60 * 1000);
      case 'Monthly':
        const nextMonth = new Date(baseDate);
        nextMonth.setMonth(nextMonth.getMonth() + 1);
        return nextMonth;
      default:
        return new Date(baseDate.getTime() + 24 * 60 * 60 * 1000);
    }
  }

  // Check if a date matches the schedule pattern
  isDateInSchedule(date: Date, schedule: RouteSchedule, daywise?: RouteScheduleDaywise): boolean {
    // Check if date is within schedule range
    if (schedule.FromDate && new Date(schedule.FromDate) > date) return false;
    if (schedule.ToDate && new Date(schedule.ToDate) < date) return false;

    // Check daywise configuration
    if (daywise) {
      const dayOfWeek = date.getDay();
      const dayMap = [
        daywise.Sunday,
        daywise.Monday,
        daywise.Tuesday,
        daywise.Wednesday,
        daywise.Thursday,
        daywise.Friday,
        daywise.Saturday
      ];
      
      if (dayMap[dayOfWeek] !== 1) return false;
    }

    // Check schedule type patterns
    if (schedule.LastBeatDate) {
      const lastBeat = new Date(schedule.LastBeatDate);
      const daysSince = Math.floor((date.getTime() - lastBeat.getTime()) / (24 * 60 * 60 * 1000));
      
      switch (schedule.Type) {
        case 'Daily':
          return daysSince >= 1;
        case 'Weekly':
          return daysSince >= 7;
        case 'Fortnightly':
          return daysSince >= 14;
        case 'Monthly':
          const monthsDiff = (date.getFullYear() - lastBeat.getFullYear()) * 12 + 
                          (date.getMonth() - lastBeat.getMonth());
          return monthsDiff >= 1;
      }
    }

    return true;
  }

  // Generate dates for a schedule within a range
  async generateScheduleDates(
    schedule: RouteSchedule,
    startDate: Date,
    endDate: Date,
    options?: {
      excludeHolidays?: boolean;
      orgUID?: string;
      daywise?: RouteScheduleDaywise;
    }
  ): Promise<Date[]> {
    const dates: Date[] = [];
    const current = new Date(startDate);

    while (current <= endDate) {
      if (this.isDateInSchedule(current, schedule, options?.daywise)) {
        // Check for holidays if needed
        if (options?.excludeHolidays && options?.orgUID) {
          // Would need to integrate with holiday service
          dates.push(new Date(current));
        } else {
          dates.push(new Date(current));
        }
      }
      
      // Move to next potential date based on schedule type
      switch (schedule.Type) {
        case 'Daily':
          current.setDate(current.getDate() + 1);
          break;
        case 'Weekly':
          current.setDate(current.getDate() + 7);
          break;
        case 'Fortnightly':
          current.setDate(current.getDate() + 14);
          break;
        case 'Monthly':
          current.setMonth(current.getMonth() + 1);
          break;
        default:
          current.setDate(current.getDate() + 1);
      }
    }

    return dates;
  }

  // Get schedule type display text
  getScheduleTypeDisplay(type: string): string {
    const typeMap: Record<string, string> = {
      'Daily': 'Every Day',
      'Weekly': 'Every Week',
      'Fortnightly': 'Every 2 Weeks',
      'Monthly': 'Every Month'
    };
    return typeMap[type] || type;
  }

  // Get day names from daywise configuration
  getActiveDays(daywise: RouteScheduleDaywise): string[] {
    const days: string[] = [];
    if (daywise.Monday === 1) days.push('Mon');
    if (daywise.Tuesday === 1) days.push('Tue');
    if (daywise.Wednesday === 1) days.push('Wed');
    if (daywise.Thursday === 1) days.push('Thu');
    if (daywise.Friday === 1) days.push('Fri');
    if (daywise.Saturday === 1) days.push('Sat');
    if (daywise.Sunday === 1) days.push('Sun');
    return days;
  }
}

export const routeScheduleService = new RouteScheduleService();