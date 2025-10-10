import React, { useEffect, useState } from 'react';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Calendar } from '@/components/ui/calendar';
import { Holiday, holidayService } from '@/services/holidayService';
import moment from 'moment';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";

interface HolidayCalendarProps {
  orgUID: string;
  selectedDate?: Date;
  onDateSelect?: (date: Date | undefined) => void;
  startDate?: Date;
  endDate?: Date;
  showHolidayList?: boolean;
}

interface WeekInfo {
  weekNumber: number;
  startDate: Date;
  endDate: Date;
  label: string;
  dates: Date[];
  hasWeekend: boolean;
  hasHoliday: boolean;
  holidayCount: number;
  workingDays: number;
}

export const HolidayCalendar: React.FC<HolidayCalendarProps> = ({
  orgUID,
  selectedDate,
  onDateSelect,
  startDate,
  endDate,
  showHolidayList = true,
}) => {
  const [holidays, setHolidays] = useState<Holiday[]>([]);
  const [loading, setLoading] = useState(false);
  const [holidayMap, setHolidayMap] = useState<Map<string, Holiday>>(new Map());
  
  // Selection state
  const [selectionMode, setSelectionMode] = useState<'week' | 'day'>('week');
  const [selectedWeeks, setSelectedWeeks] = useState<number[]>([]);
  const [selectedDates, setSelectedDates] = useState<Date[]>([]);
  const [activeTab, setActiveTab] = useState<'dates' | 'weekends' | 'holidays'>('dates');

  useEffect(() => {
    if (orgUID) {
      loadHolidays();
    }
  }, [orgUID]);

  const loadHolidays = async () => {
    setLoading(true);
    try {
      const start = startDate || new Date();
      const end = endDate || moment().add(3, 'months').toDate();
      
      const holidayData = await holidayService.getHolidaysInDateRange(
        orgUID,
        start,
        end
      );
      
      setHolidays(holidayData);
      
      const map = new Map<string, Holiday>();
      holidayData.forEach(h => {
        map.set(moment(h.HolidayDate).format('YYYY-MM-DD'), h);
      });
      setHolidayMap(map);
    } catch (error) {
      console.error('Error loading holidays:', error);
    } finally {
      setLoading(false);
    }
  };

  // Generate 5 weeks starting from current week
  const generateWeeks = (): WeekInfo[] => {
    const weeks: WeekInfo[] = [];
    const currentWeekStart = moment().startOf('isoWeek');
    
    for (let i = 0; i < 5; i++) {
      const weekStart = moment(currentWeekStart).add(i, 'weeks');
      const weekEnd = moment(weekStart).endOf('isoWeek');
      const dates: Date[] = [];
      let holidayCount = 0;
      let workingDays = 0;
      
      // Generate all dates for this week
      for (let d = 0; d < 7; d++) {
        const date = moment(weekStart).add(d, 'days');
        dates.push(date.toDate());
        
        const dateStr = date.format('YYYY-MM-DD');
        if (holidayMap.has(dateStr)) {
          holidayCount++;
        }
        
        // Count working days (Mon-Fri, not holiday, not past)
        if (date.day() !== 0 && date.day() !== 6 && 
            !holidayMap.has(dateStr) && 
            !date.isBefore(moment(), 'day')) {
          workingDays++;
        }
      }
      
      weeks.push({
        weekNumber: i + 1,
        startDate: weekStart.toDate(),
        endDate: weekEnd.toDate(),
        label: `Week ${i + 1}`,
        dates: dates,
        hasWeekend: true,
        hasHoliday: holidayCount > 0,
        holidayCount: holidayCount,
        workingDays: workingDays
      });
    }
    
    return weeks;
  };

  const weeks = generateWeeks();

  // Get weekends for next 2 months
  const getUpcomingWeekends = () => {
    const weekends: { date: Date; label: string }[] = [];
    const endDate = moment().add(2, 'months');
    let current = moment();
    
    while (current.isBefore(endDate)) {
      if (current.day() === 6) { // Saturday
        weekends.push({
          date: current.toDate(),
          label: current.format('dddd, MMMM DD, YYYY')
        });
      }
      if (current.day() === 0) { // Sunday
        weekends.push({
          date: current.toDate(),
          label: current.format('dddd, MMMM DD, YYYY')
        });
      }
      current.add(1, 'day');
    }
    
    return weekends;
  };

  const handleWeekToggle = (weekNumber: number) => {
    if (selectedWeeks.includes(weekNumber)) {
      setSelectedWeeks(selectedWeeks.filter(w => w !== weekNumber));
    } else {
      setSelectedWeeks([...selectedWeeks, weekNumber]);
    }
  };

  const handleDateSelection = (date: Date | undefined) => {
    if (date) {
      onDateSelect?.(date);
    }
  };

  const isDateDisabled = (date: Date) => {
    // Disable past dates
    if (moment(date).isBefore(moment().startOf('day'))) {
      return true;
    }
    
    // Disable based on active tab
    if (activeTab === 'dates') {
      // In dates tab, disable weekends and holidays
      const day = date.getDay();
      const dateStr = moment(date).format('YYYY-MM-DD');
      
      if (day === 0 || day === 6) return true; // Weekend
      if (holidayMap.has(dateStr)) return true; // Holiday
    }
    
    return false;
  };

  const modifiers = {
    holiday: (date: Date) => {
      const dateStr = moment(date).format('YYYY-MM-DD');
      return holidayMap.has(dateStr);
    },
    weekend: (date: Date) => {
      const day = date.getDay();
      return day === 0 || day === 6;
    },
  };

  const modifiersStyles = {
    holiday: {
      backgroundColor: 'rgb(254 226 226)',
      color: 'rgb(185 28 28)',
      fontWeight: '600',
    },
    weekend: {
      backgroundColor: 'rgb(241 245 249)',
      color: 'rgb(100 116 139)',
    },
  };

  if (loading) {
    return <Skeleton className="h-[500px] w-full" />;
  }

  return (
    <div className="w-full">
      <Card className="overflow-hidden">
        {/* Header with Tabs */}
        <div className="bg-gray-50 dark:bg-gray-900 p-4 border-b">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold">Schedule & Date</h3>
            <div className="text-sm text-muted-foreground">
              Select visit date and configure schedule settings
            </div>
          </div>

          {/* Tab Navigation */}
          <div className="flex items-center gap-6">
            <div className="flex items-center gap-2">
              <span className="text-sm font-medium text-red-500">*</span>
              <span className="text-sm font-medium">Visit Date:</span>
            </div>
            
            <div className="flex gap-1 bg-white dark:bg-gray-800 rounded-lg p-1 shadow-sm">
              <Button
                variant={activeTab === 'dates' ? 'default' : 'ghost'}
                size="sm"
                onClick={() => setActiveTab('dates')}
                className="px-4"
              >
                Working Days
              </Button>
              <Button
                variant={activeTab === 'weekends' ? 'default' : 'ghost'}
                size="sm"
                onClick={() => setActiveTab('weekends')}
                className="px-4"
              >
                Weekends
              </Button>
              <Button
                variant={activeTab === 'holidays' ? 'default' : 'ghost'}
                size="sm"
                onClick={() => setActiveTab('holidays')}
                className="px-4"
              >
                Holidays List
              </Button>
            </div>
          </div>
        </div>

        {/* Content Area */}
        <div className="p-6">
          {/* Working Days Tab */}
          {activeTab === 'dates' && (
            <div className="space-y-6">
              {/* Selection Mode */}
              <div className="flex items-center gap-6 pb-4 border-b">
                <span className="text-sm font-medium">Select Journey Plan for:</span>
                <div className="flex gap-4">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="mode"
                      value="week"
                      checked={selectionMode === 'week'}
                      onChange={() => setSelectionMode('week')}
                      className="w-4 h-4"
                    />
                    <span className="font-medium">Week Wise</span>
                  </label>
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="mode"
                      value="day"
                      checked={selectionMode === 'day'}
                      onChange={() => setSelectionMode('day')}
                      className="w-4 h-4"
                    />
                    <span className="font-medium">Day Wise</span>
                  </label>
                </div>
              </div>

              {/* Week Selection */}
              {selectionMode === 'week' && (
                <div>
                  <div className="flex items-center gap-2 mb-4">
                    <span className="text-sm font-medium text-red-500">*</span>
                    <span className="text-sm font-medium">Select weeks:</span>
                  </div>
                  <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
                    {weeks.map((week) => (
                      <div key={week.weekNumber} className="flex items-start gap-2">
                        <Checkbox
                          id={`week-${week.weekNumber}`}
                          checked={selectedWeeks.includes(week.weekNumber)}
                          onCheckedChange={() => handleWeekToggle(week.weekNumber)}
                        />
                        <label
                          htmlFor={`week-${week.weekNumber}`}
                          className="cursor-pointer flex-1"
                        >
                          <div className="font-medium text-sm">Week {week.weekNumber}</div>
                          <div className="text-xs text-muted-foreground">
                            {moment(week.startDate).format('MMM DD')} - {moment(week.endDate).format('MMM DD')}
                          </div>
                          <div className="text-xs text-green-600 mt-1">
                            {week.workingDays} working days
                          </div>
                          {week.hasHoliday && (
                            <div className="text-xs text-red-600">
                              {week.holidayCount} holiday{week.holidayCount > 1 ? 's' : ''}
                            </div>
                          )}
                        </label>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Day Selection - Calendar */}
              {selectionMode === 'day' && (
                <div>
                  <div className="flex items-center gap-2 mb-4">
                    <span className="text-sm font-medium text-red-500">*</span>
                    <span className="text-sm font-medium">Select date from calendar:</span>
                  </div>
                  <div className="flex justify-center">
                    <Calendar
                      mode="single"
                      selected={selectedDate}
                      onSelect={handleDateSelection}
                      disabled={isDateDisabled}
                      modifiers={modifiers}
                      modifiersStyles={modifiersStyles}
                      className="rounded-md border"
                    />
                  </div>
                  <div className="mt-4 flex items-center justify-center gap-6 text-xs">
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 bg-gray-200 rounded" />
                      <span>Weekend (Disabled)</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 bg-red-100 border border-red-300 rounded" />
                      <span>Holiday (Disabled)</span>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Weekends Tab */}
          {activeTab === 'weekends' && (
            <div className="space-y-4">
              <div className="text-sm text-muted-foreground mb-4">
                Select weekend dates for journey planning
              </div>
              <div className="grid grid-cols-2 gap-3 max-h-[400px] overflow-y-auto">
                {getUpcomingWeekends().slice(0, 20).map((weekend, index) => (
                  <label
                    key={index}
                    className="flex items-center gap-3 p-3 rounded-lg border hover:bg-gray-50 cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      value={weekend.date.toISOString()}
                      onChange={(e) => {
                        if (e.target.checked) {
                          handleDateSelection(weekend.date);
                        }
                      }}
                      className="w-4 h-4"
                    />
                    <div className="flex-1">
                      <div className="font-medium text-sm">{weekend.label}</div>
                      <div className="text-xs text-muted-foreground">
                        {moment(weekend.date).day() === 6 ? 'Saturday' : 'Sunday'}
                      </div>
                    </div>
                  </label>
                ))}
              </div>
            </div>
          )}

          {/* Holidays Tab */}
          {activeTab === 'holidays' && (
            <div className="space-y-4">
              <div className="text-sm text-muted-foreground mb-4">
                Upcoming holidays in your organization (Next 3 months)
              </div>
              <div className="space-y-2 max-h-[400px] overflow-y-auto">
                {holidays
                  .filter(h => moment(h.HolidayDate).isAfter(moment()))
                  .sort((a, b) => moment(a.HolidayDate).diff(moment(b.HolidayDate)))
                  .map((holiday) => (
                    <div
                      key={holiday.UID}
                      className="flex items-center justify-between p-3 rounded-lg bg-red-50 dark:bg-red-950/20 border border-red-100"
                    >
                      <div className="flex items-center gap-4">
                        <div className="text-center">
                          <div className="text-lg font-bold text-red-600">
                            {moment(holiday.HolidayDate).format('DD')}
                          </div>
                          <div className="text-xs text-muted-foreground">
                            {moment(holiday.HolidayDate).format('MMM')}
                          </div>
                        </div>
                        <div>
                          <div className="font-medium">{holiday.Name}</div>
                          <div className="text-sm text-muted-foreground">
                            {moment(holiday.HolidayDate).format('dddd, YYYY')}
                          </div>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <Badge variant={holiday.IsOptional ? "secondary" : "destructive"}>
                          {holiday.Type}
                        </Badge>
                        <label className="flex items-center gap-2">
                          <input
                            type="checkbox"
                            value={holiday.HolidayDate}
                            onChange={(e) => {
                              if (e.target.checked) {
                                handleDateSelection(new Date(holiday.HolidayDate));
                              }
                            }}
                            className="w-4 h-4"
                          />
                          <span className="text-sm">Select</span>
                        </label>
                      </div>
                    </div>
                  ))}
              </div>
            </div>
          )}

          {/* Selected Date Display */}
          {selectedDate && (
            <div className="mt-6 p-4 bg-green-50 dark:bg-green-950/20 rounded-lg border border-green-200">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm font-medium text-green-800">Selected Date:</div>
                  <div className="text-lg font-semibold">
                    {moment(selectedDate).format('dddd, MMMM DD, YYYY')}
                  </div>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onDateSelect?.(undefined)}
                >
                  Clear
                </Button>
              </div>
            </div>
          )}

          {/* Selected Weeks Display */}
          {selectionMode === 'week' && selectedWeeks.length > 0 && (
            <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-950/20 rounded-lg border border-blue-200">
              <div className="text-sm font-medium text-blue-800 mb-2">
                Selected Weeks: {selectedWeeks.length}
              </div>
              <div className="flex flex-wrap gap-2">
                {selectedWeeks.map(weekNum => {
                  const week = weeks.find(w => w.weekNumber === weekNum);
                  return week ? (
                    <Badge key={weekNum} variant="secondary">
                      Week {weekNum}: {moment(week.startDate).format('MMM DD')} - {moment(week.endDate).format('MMM DD')}
                    </Badge>
                  ) : null;
                })}
              </div>
            </div>
          )}
        </div>
      </Card>
    </div>
  );
};