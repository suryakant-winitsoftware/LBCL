import React, { useState, useEffect, useRef } from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useToast } from '@/components/ui/use-toast';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { 
  Calendar,
  Clock,
  Users,
  MapPin,
  Plus,
  X,
  ChevronRight,
  CalendarDays,
  CalendarRange,
  CalendarClock,
  Search,
  Filter,
  CheckCircle,
  AlertCircle,
  Pencil,
  Check
} from 'lucide-react';
import moment from 'moment';
import { cn } from '@/lib/utils';
import { Separator } from '@/components/ui/separator';

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  Type?: string;
  Status?: string;
  SeqNo?: number;
}

interface TimeSlot {
  id: string;
  date: Date;
  dayName: string;
  startTime: string;
  endTime: string;
  customers: Customer[];
  maxCustomers: number;
  customTiming?: boolean;
  visitDuration?: number;
}

interface StepScheduleCustomersProps {
  form: UseFormReturn<any>;
  routeCustomers: Customer[];
  dropdownsLoading: any;
}

export const StepScheduleCustomers: React.FC<StepScheduleCustomersProps> = ({
  form,
  routeCustomers,
  dropdownsLoading,
}) => {
  const { toast } = useToast();
  const [scheduleType, setScheduleType] = useState<'daily' | 'weekly' | 'monthly'>('daily');
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
  const [selectedCustomers, setSelectedCustomers] = useState<Set<string>>(new Set());
  const [searchTerm, setSearchTerm] = useState('');
  const [draggedCustomer, setDraggedCustomer] = useState<Customer | null>(null);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [modalTargetSlot, setModalTargetSlot] = useState<string | null>(null);
  
  // Time slot editing
  const [editingSlot, setEditingSlot] = useState<string | null>(null);
  const [timeTemplates] = useState([
    { name: 'Morning', startTime: '08:00', endTime: '12:00', duration: 30 },
    { name: 'Afternoon', startTime: '12:00', endTime: '17:00', duration: 30 },
    { name: 'Evening', startTime: '17:00', endTime: '20:00', duration: 20 },
    { name: 'Full Day', startTime: '00:00', endTime: '00:00', duration: 30 },
  ]);
  
  // Holiday and Weekend toggles
  const [includeWeekends, setIncludeWeekends] = useState(false);
  const [includeHolidays, setIncludeHolidays] = useState(false);
  const [showWeekdaySelector, setShowWeekdaySelector] = useState(false);
  const [showHolidayManager, setShowHolidayManager] = useState(false);
  
  // Weekday selection - by default, exclude weekends (Saturday and Sunday)
  const [selectedWeekdays, setSelectedWeekdays] = useState({
    monday: true,
    tuesday: true,
    wednesday: true,
    thursday: true,
    friday: true,
    saturday: false,  // Weekend - excluded by default
    sunday: false,     // Weekend - excluded by default
  });
  
  // Holiday management
  const [holidays, setHolidays] = useState<Date[]>([]);
  const [loadingHolidays, setLoadingHolidays] = useState(false);
  
  // Schedule configuration
  const [dailyDays, setDailyDays] = useState(6); // Next 6 days
  const [weeklyWeeks, setWeeklyWeeks] = useState(2); // Next 2 weeks
  const [monthlyDays, setMonthlyDays] = useState(30); // Next 30 days
  
  // Date range configuration - use form's visitDate if available
  const formVisitDate = form.watch('visitDate');
  const initialStartDate = formVisitDate ? new Date(formVisitDate) : new Date();
  
  const [startDate, setStartDate] = useState<Date>(initialStartDate);
  const [endDate, setEndDate] = useState<Date>(moment(initialStartDate).add(7, 'days').toDate());
  const [useDateRange, setUseDateRange] = useState(false);
  
  // Default time configuration for all slots
  // Get planned times from the parent form (set in Basic Setup step)
  // These should default to 00:00
  const formStartTime = form.watch('plannedStartTime');
  const formEndTime = form.watch('plannedEndTime');
  
  // Force 00:00 if the form values are empty or not set properly
  // Ensure time is in HH:mm format for HTML time input
  const formatTimeForInput = (time: string | undefined): string => {
    if (!time || time === '') return '00:00'; // Default to 00:00
    // Ensure it's in HH:mm format
    if (time.length === 5 && time.includes(':')) return time;
    return '00:00'; // Default to 00:00
  };
  
  const [dayStartTime, setDayStartTime] = useState(formatTimeForInput(formStartTime));
  const [dayEndTime, setDayEndTime] = useState(formatTimeForInput(formEndTime));
  const [slotDuration, setSlotDuration] = useState(form.watch('defaultDuration') || 30); // 30 minutes per customer by default
  
  // Refs for the time inputs to control them directly
  const startTimeRef = useRef<HTMLInputElement>(null);
  const endTimeRef = useRef<HTMLInputElement>(null);
  const [useMaxCustomers, setUseMaxCustomers] = useState(true); // Toggle for max customers limit
  const [customMaxCustomers, setCustomMaxCustomers] = useState<number | null>(null); // Custom max customers value
  const [maxCustomersOption, setMaxCustomersOption] = useState<'auto' | 'custom' | 'unlimited'>('auto');

  // Load holidays from API on mount and when orgUID changes
  useEffect(() => {
    const orgUID = form.getValues('orgUID');
    if (orgUID) {
      loadHolidays();
    }
  }, [form.watch('orgUID')]);
  
  // Set default time values in form on mount if not already set
  useEffect(() => {
    // Get the planned times from parent form
    const currentStartTime = form.watch('plannedStartTime');
    const currentEndTime = form.watch('plannedEndTime');
    
    console.log('[StepScheduleCustomers] Initial form times:', {
      currentStartTime,
      currentEndTime,
      formStartTime,
      formEndTime
    });
    
    // Use business hours as defaults
    const startTimeToUse = currentStartTime || formStartTime || '00:00';
    const endTimeToUse = currentEndTime || formEndTime || '00:00';
    
    console.log('[StepScheduleCustomers] Setting times to:', { startTimeToUse, endTimeToUse });
    
    // Update local state with the planned times
    setDayStartTime(startTimeToUse);
    setDayEndTime(endTimeToUse);
    
    // Also ensure plannedStartTime and plannedEndTime are set if not already
    if (!currentStartTime) {
      form.setValue('plannedStartTime', startTimeToUse);
    }
    if (!currentEndTime) {
      form.setValue('plannedEndTime', endTimeToUse);
    }
    
    if (!form.watch('defaultDuration')) {
      form.setValue('defaultDuration', slotDuration);
    }
    
    // Force the input values directly using refs to avoid browser defaults
    setTimeout(() => {
      if (startTimeRef.current) {
        startTimeRef.current.value = startTimeToUse;
      }
      if (endTimeRef.current) {
        endTimeRef.current.value = endTimeToUse;
      }
      setDayStartTime(startTimeToUse);
      setDayEndTime(endTimeToUse);
    }, 100);
  }, []); // Run only on mount

  // Track if this is the initial mount
  const isInitialMount = React.useRef(true);
  
  // Update form when timeSlots change
  useEffect(() => {
    if (timeSlots.length > 0 && !isInitialMount.current) {
      updateFormData(timeSlots);
    }
  }, [timeSlots]);
  
  // Generate time slots based on schedule type and time settings
  useEffect(() => {
    // Skip generation on initial mount to prevent duplicate calls
    if (isInitialMount.current) {
      isInitialMount.current = false;
      // Still generate once after a small delay to ensure form is ready
      setTimeout(() => generateTimeSlots(), 100);
      return;
    }
    generateTimeSlots();
  }, [scheduleType, dailyDays, weeklyWeeks, monthlyDays, includeWeekends, includeHolidays, selectedWeekdays, useDateRange, startDate, endDate, dayStartTime, dayEndTime, slotDuration]);
  
  // Update form data when time slots change
  const updateFormData = (slots: TimeSlot[]) => {
    // Get the first slot date as visit date
    if (slots.length > 0) {
      // Only update visitDate if it's different from current value
      const currentVisitDate = form.watch('visitDate');
      const newVisitDate = slots[0].date;
      
      // Compare dates properly - only update if actually different
      const currentTime = currentVisitDate ? new Date(currentVisitDate).getTime() : null;
      const newTime = new Date(newVisitDate).getTime();
      
      if (currentTime !== newTime) {
        console.log('[StepScheduleCustomers] Updating visitDate from', currentVisitDate, 'to', newVisitDate);
        form.setValue('visitDate', newVisitDate);
      } else {
        console.log('[StepScheduleCustomers] visitDate unchanged, skipping update');
      }
      // Update times in form to match local state
      form.setValue('dayStartsAt', dayStartTime);
      form.setValue('dayEndsBy', dayEndTime);
      if (!form.watch('defaultDuration')) {
        form.setValue('defaultDuration', slotDuration);
      }
      
      // Collect all customers with their times
      const customersWithTimes: any[] = [];
      slots.forEach(slot => {
        slot.customers.forEach((customer, index) => {
          const startMinutes = moment(slot.startTime, 'HH:mm').minutes() + (index * slotDuration);
          const startTime = moment(slot.startTime, 'HH:mm').add(index * slotDuration, 'minutes').format('HH:mm');
          const endTime = moment(startTime, 'HH:mm').add(slotDuration, 'minutes').format('HH:mm');
          
          customersWithTimes.push({
            customerUID: customer.UID,
            startTime,
            endTime,
            visitDuration: slotDuration,
            visitDay: slot.date
          });
        });
      });
      
      form.setValue('selectedCustomersWithTimes', customersWithTimes);
    }
  };

  const loadHolidays = async () => {
    setLoadingHolidays(true);
    try {
      // Get orgUID from form
      const orgUID = form.getValues('orgUID');
      
      if (orgUID) {
        // Import holidayService from your services
        const { holidayService } = await import('@/services/holidayService');
        const currentYear = new Date().getFullYear();
        
        // Get holidays for current and next year using the correct method
        const [currentYearHolidays, nextYearHolidays] = await Promise.all([
          holidayService.getHolidaysForYear(orgUID, currentYear),
          holidayService.getHolidaysForYear(orgUID, currentYear + 1)
        ]);
        
        const allHolidays = [...currentYearHolidays, ...nextYearHolidays];
        // Map HolidayDate field correctly
        setHolidays(allHolidays.map(h => new Date(h.HolidayDate)));
      } else {
        // Set default holidays if no orgUID
        setHolidays([
          new Date('2025-01-01'), // New Year
          new Date('2025-01-26'), // Republic Day
          new Date('2025-03-17'), // Holi
          new Date('2025-08-15'), // Independence Day
          new Date('2025-10-02'), // Gandhi Jayanti
          new Date('2025-10-24'), // Diwali
          new Date('2025-12-25'), // Christmas
        ]);
      }
    } catch (error) {
      console.error('Error loading holidays:', error);
      // Set some default holidays as fallback
      setHolidays([
        new Date('2025-01-01'), // New Year
        new Date('2025-01-26'), // Republic Day
        new Date('2025-03-17'), // Holi
        new Date('2025-08-15'), // Independence Day
        new Date('2025-10-02'), // Gandhi Jayanti
        new Date('2025-10-24'), // Diwali
        new Date('2025-12-25'), // Christmas
      ]);
    } finally {
      setLoadingHolidays(false);
    }
  };

  const isHoliday = (date: moment.Moment): boolean => {
    return holidays.some(holiday => 
      moment(holiday).format('YYYY-MM-DD') === date.format('YYYY-MM-DD')
    );
  };

  const getWeekdayKey = (dayNumber: number): keyof typeof selectedWeekdays => {
    const dayMap: { [key: number]: keyof typeof selectedWeekdays } = {
      0: 'sunday',
      1: 'monday',
      2: 'tuesday',
      3: 'wednesday',
      4: 'thursday',
      5: 'friday',
      6: 'saturday',
    };
    return dayMap[dayNumber];
  };

  const shouldIncludeDate = (date: moment.Moment): boolean => {
    // Check if the weekday is selected
    const weekdayKey = getWeekdayKey(date.day());
    if (!selectedWeekdays[weekdayKey]) return false;
    
    // Check if it's a holiday
    if (isHoliday(date) && !includeHolidays) return false;
    
    return true;
  };

  const generateTimeSlots = () => {
    console.log('[StepScheduleCustomers] Generating time slots with:', {
      scheduleType,
      dayStartTime,
      dayEndTime,
      slotDuration
    });
    
    const slots: TimeSlot[] = [];
    
    // Use the values as-is, including "00:00"
    const startTime = dayStartTime;
    const endTime = dayEndTime;
    const duration = slotDuration > 0 ? slotDuration : 30; // Default 30 minutes
    
    // Don't generate if times are not set (empty strings) or both are "00:00" with no duration
    if (startTime === '' || endTime === '' || duration <= 0) {
      setTimeSlots([]);
      return;
    }
    
    // Special case: if both start and end are "00:00", treat as 24-hour period
    // Otherwise "00:00" to "00:00" would generate no slots
    if (startTime === '00:00' && endTime === '00:00') {
      // This means a full 24-hour period
      // User should set different times if they don't want 24 hours
    }
    
    const today = moment().startOf('day');
    const maxCustomers = calculateMaxCustomers(startTime, endTime, duration);
    
    // Use date range if enabled
    if (useDateRange) {
      const start = moment(startDate).startOf('day');
      const end = moment(endDate).startOf('day');
      const daysDiff = end.diff(start, 'days') + 1;
      
      for (let i = 0; i < daysDiff; i++) {
        const date = moment(start).add(i, 'days');
        if (shouldIncludeDate(date)) {
          slots.push({
            id: `range-day-${i}`,
            date: date.toDate(),
            dayName: date.format('dddd'),
            startTime: startTime,
            endTime: endTime,
            customers: [],
            maxCustomers: maxCustomers,
            visitDuration: duration
          });
        }
      }
    } else if (scheduleType === 'daily') {
      // Generate next N days
      let addedDays = 0;
      let i = 0;
      while (addedDays < dailyDays && i < dailyDays * 2) { // Safety limit
        const date = moment(today).add(i, 'days');
        if (shouldIncludeDate(date)) {
          slots.push({
            id: `day-${i}`,
            date: date.toDate(),
            dayName: date.format('dddd'),
            startTime: startTime,
            endTime: endTime,
            customers: [],
            maxCustomers: maxCustomers,
            visitDuration: duration
          });
          addedDays++;
        }
        i++;
      }
    } else if (scheduleType === 'weekly') {
      // Generate weeks (starting from Sunday)
      for (let w = 0; w < weeklyWeeks; w++) {
        const weekStart = moment(today).startOf('week').add(w, 'weeks'); // 'week' starts on Sunday
        for (let d = 0; d < 7; d++) {
          const date = moment(weekStart).add(d, 'days');
          if (shouldIncludeDate(date)) {
            slots.push({
              id: `week-${w}-day-${d}`,
              date: date.toDate(),
              dayName: date.format('dddd'),
              startTime: startTime,
              endTime: endTime,
              customers: [],
              maxCustomers: maxCustomers,
              visitDuration: duration
            });
          }
        }
      }
    } else if (scheduleType === 'monthly') {
      // Generate monthly view
      let addedDays = 0;
      let i = 0;
      while (addedDays < monthlyDays && i < monthlyDays * 2) { // Safety limit
        const date = moment(today).add(i, 'days');
        if (shouldIncludeDate(date)) {
          slots.push({
            id: `month-day-${i}`,
            date: date.toDate(),
            dayName: date.format('dddd'),
            startTime: startTime,
            endTime: endTime,
            customers: [],
            maxCustomers: maxCustomers,
            visitDuration: duration
          });
          addedDays++;
        }
        i++;
      }
    }
    
    setTimeSlots(slots);
    // Form update will happen via useEffect watching timeSlots
  };

  const calculateMaxCustomers = (startTime?: string, endTime?: string, duration?: number) => {
    // If custom max customers is set and limit is enabled, use that
    if (useMaxCustomers && customMaxCustomers !== null && startTime === undefined) {
      return customMaxCustomers;
    }
    // If no limit, return 999
    if (!useMaxCustomers && startTime === undefined) {
      return 999;
    }
    
    // Use provided times or fall back to state values
    const startTimeToUse = startTime !== undefined ? startTime : dayStartTime;
    const endTimeToUse = endTime !== undefined ? endTime : dayEndTime;
    const durationToUse = duration !== undefined ? duration : slotDuration;
    
    // Handle empty strings
    if (startTimeToUse === '' || endTimeToUse === '' || durationToUse <= 0) {
      return 0;
    }
    
    // Calculate based on time
    const start = moment(startTimeToUse, 'HH:mm');
    const end = moment(endTimeToUse, 'HH:mm');
    
    let totalMinutes = end.diff(start, 'minutes');
    
    // Special cases:
    // 1. If both are "00:00", default to 999 slots
    if (startTimeToUse === '00:00' && endTimeToUse === '00:00') {
      // Return 999 as default when times are not set
      return 999;
    }
    // 2. If end time is before start time (crosses midnight)
    else if (totalMinutes < 0) {
      totalMinutes += 24 * 60;
    }
    // 3. If totalMinutes is 0 (same time), assume 24 hours
    else if (totalMinutes === 0) {
      totalMinutes = 24 * 60;
    }
    
    return Math.floor(totalMinutes / durationToUse);
  };
  
  const updateSlotTiming = (slotId: string, startTime: string, endTime: string, duration: number) => {
    setTimeSlots(prev => prev.map(slot => {
      if (slot.id === slotId) {
        return {
          ...slot,
          startTime,
          endTime,
          visitDuration: duration,
          customTiming: true,
          maxCustomers: calculateMaxCustomers(startTime, endTime, duration)
        };
      }
      return slot;
    }));
    setEditingSlot(null);
  };
  
  const applyTimeTemplate = (template: typeof timeTemplates[0]) => {
    setTimeSlots(prev => prev.map(slot => ({
      ...slot,
      startTime: template.startTime,
      endTime: template.endTime,
      visitDuration: template.duration,
      customTiming: true,
      maxCustomers: calculateMaxCustomers(template.startTime, template.endTime, template.duration)
    })));
    toast({
      title: "Template Applied",
      description: `${template.name} timing applied to all time slots`,
    });
  };

  const handleCustomerDrop = (customer: Customer, slotId: string) => {
    setTimeSlots(prev => {
      const updated = prev.map(slot => {
        if (slot.id === slotId && slot.customers.length < slot.maxCustomers) {
          return {
            ...slot,
            customers: [...slot.customers, customer]
          };
        }
        return slot;
      });
      
      // Schedule the form update to avoid updating during render
      setTimeout(() => updateFormData(updated), 0);
      return updated;
    });
    setSelectedCustomers(prev => new Set(prev).add(customer.UID));
  };

  const removeCustomerFromSlot = (customerId: string, slotId: string) => {
    setTimeSlots(prev => {
      const updated = prev.map(slot => {
        if (slot.id === slotId) {
          return {
            ...slot,
            customers: slot.customers.filter(c => c.UID !== customerId)
          };
        }
        return slot;
      });
      
      // Schedule the form update to avoid updating during render
      setTimeout(() => updateFormData(updated), 0);
      return updated;
    });
    setSelectedCustomers(prev => {
      const newSet = new Set(prev);
      newSet.delete(customerId);
      return newSet;
    });
  };

  const filteredCustomers = routeCustomers.filter(customer =>
    customer.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    customer.Code.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const unassignedCustomers = filteredCustomers.filter(
    customer => !selectedCustomers.has(customer.UID)
  );

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="space-y-6"
    >
      <Card className="w-full">
        <CardHeader className="pb-4">
          <div className="space-y-4">
            {/* Date Range and Controls */}
            <div className="flex justify-between items-start">
              {/* Date Range Selector */}
              <div className="flex items-center gap-3">
                <div className="flex items-center gap-2 px-3 py-2 bg-white dark:bg-gray-800 rounded-lg shadow-sm border">
                  <Switch
                    id="use-date-range"
                    checked={useDateRange}
                    onCheckedChange={setUseDateRange}
                  />
                  <Label htmlFor="use-date-range" className="text-sm font-medium cursor-pointer">
                    Use Date Range
                  </Label>
                </div>
                
                {useDateRange && (
                  <motion.div
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="flex items-center gap-2"
                  >
                    <div className="flex items-center gap-2 px-3 py-1.5 bg-white dark:bg-gray-800 rounded-lg shadow-sm border">
                      <CalendarRange className="h-4 w-4 text-green-500" />
                      <Input
                        type="date"
                        value={moment(startDate).format('YYYY-MM-DD')}
                        onChange={(e) => setStartDate(new Date(e.target.value))}
                        className="h-8 w-32 border-0 p-1 text-sm"
                      />
                      <span className="text-gray-400">to</span>
                      <Input
                        type="date"
                        value={moment(endDate).format('YYYY-MM-DD')}
                        onChange={(e) => setEndDate(new Date(e.target.value))}
                        min={moment(startDate).format('YYYY-MM-DD')}
                        className="h-8 w-32 border-0 p-1 text-sm"
                      />
                    </div>
                    <div className="flex items-center gap-1">
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => {
                          setStartDate(new Date());
                          setEndDate(moment().add(7, 'days').toDate());
                        }}
                        className="text-xs"
                      >
                        1 Week
                      </Button>
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => {
                          setStartDate(new Date());
                          setEndDate(moment().add(14, 'days').toDate());
                        }}
                        className="text-xs"
                      >
                        2 Weeks
                      </Button>
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => {
                          setStartDate(new Date());
                          setEndDate(moment().add(1, 'month').toDate());
                        }}
                        className="text-xs"
                      >
                        1 Month
                      </Button>
                    </div>
                    <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                      {moment(endDate).diff(moment(startDate), 'days') + 1} days
                    </Badge>
                  </motion.div>
                )}
              </div>

              {/* Toggle Switches at Top Right */}
              <div className="flex gap-3">
                {/* Weekday Selector */}
                <div className="flex items-center gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setShowWeekdaySelector(!showWeekdaySelector)}
                    className="gap-2"
                  >
                    <Calendar className="h-4 w-4 text-blue-500" />
                    Weekdays
                    <Badge variant="secondary" className="ml-1">
                      {Object.values(selectedWeekdays).filter(v => v).length}
                    </Badge>
                  </Button>
                </div>
                
                {/* Holiday Manager */}
                <div className="flex items-center gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setShowHolidayManager(!showHolidayManager)}
                    className="gap-2"
                  >
                    <CalendarDays className="h-4 w-4 text-red-500" />
                    Holidays
                    {holidays.length > 0 && (
                      <Badge variant="secondary" className="ml-1">
                        {holidays.length}
                      </Badge>
                    )}
                  </Button>
                  <Switch
                    id="holidays"
                    checked={includeHolidays}
                    onCheckedChange={setIncludeHolidays}
                  />
                </div>
              </div>
            </div>
            
            {/* Weekday Selector Panel */}
            {showWeekdaySelector && (
              <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                className="p-4 bg-white dark:bg-gray-800 rounded-lg shadow-sm border"
              >
                <div className="flex items-center justify-between mb-3">
                  <div>
                    <Label className="font-semibold">Select Working Days</Label>
                    <p className="text-xs text-muted-foreground mt-1">Choose which days to include in the schedule</p>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setShowWeekdaySelector(false)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
                <div className="grid grid-cols-7 gap-2">
                  {Object.entries(selectedWeekdays).map(([day, selected]) => {
                    const isWeekend = day === 'saturday' || day === 'sunday';
                    return (
                      <Button
                        key={day}
                        type="button"
                        variant={selected ? "default" : "outline"}
                        size="sm"
                        onClick={() => setSelectedWeekdays(prev => ({ ...prev, [day]: !prev[day as keyof typeof prev] }))}
                        className={cn(
                          "capitalize",
                          isWeekend && !selected && "border-orange-200 text-orange-600",
                          isWeekend && selected && "bg-orange-600 hover:bg-orange-700"
                        )}
                      >
                        {day.slice(0, 3)}
                      </Button>
                    );
                  })}
                </div>
                <div className="flex gap-2 mt-3">
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    onClick={() => setSelectedWeekdays({
                      monday: true, tuesday: true, wednesday: true, 
                      thursday: true, friday: true, saturday: false, sunday: false
                    })}
                    className="text-xs"
                  >
                    Mon-Fri
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    onClick={() => setSelectedWeekdays({
                      monday: true, tuesday: true, wednesday: true, 
                      thursday: true, friday: true, saturday: true, sunday: false
                    })}
                    className="text-xs"
                  >
                    Mon-Sat
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    onClick={() => setSelectedWeekdays({
                      monday: false, tuesday: false, wednesday: false, 
                      thursday: false, friday: false, saturday: true, sunday: true
                    })}
                    className="text-xs"
                  >
                    Weekends Only
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    onClick={() => setSelectedWeekdays({
                      monday: true, tuesday: true, wednesday: true, 
                      thursday: true, friday: true, saturday: true, sunday: true
                    })}
                    className="text-xs"
                  >
                    All Days
                  </Button>
                </div>
              </motion.div>
            )}
            
            {/* Holiday Manager Panel */}
            {showHolidayManager && (
              <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                className="p-4 bg-white dark:bg-gray-800 rounded-lg shadow-sm border"
              >
                <div className="flex items-center justify-between mb-3">
                  <Label className="font-semibold">
                    Holidays 
                    {loadingHolidays && <span className="text-xs text-muted-foreground ml-2">(Loading...)</span>}
                  </Label>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setShowHolidayManager(false)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
                <ScrollArea className="h-32">
                  {holidays.length === 0 ? (
                    <div className="text-sm text-muted-foreground">No holidays configured</div>
                  ) : (
                    <div className="space-y-1">
                      {holidays.slice(0, 10).map((holiday, index) => (
                        <div key={index} className="flex items-center justify-between text-sm">
                          <span>{moment(holiday).format('MMM DD, YYYY - dddd')}</span>
                          <Badge variant="outline" className="text-xs">
                            {moment(holiday).fromNow()}
                          </Badge>
                        </div>
                      ))}
                    </div>
                  )}
                </ScrollArea>
                <div className="mt-3 flex items-center gap-2">
                  <Switch
                    id="include-holidays"
                    checked={includeHolidays}
                    onCheckedChange={setIncludeHolidays}
                  />
                  <Label htmlFor="include-holidays" className="text-sm cursor-pointer">
                    Include holidays in schedule
                  </Label>
                </div>
              </motion.div>
            )}
            
            {/* Title and Description */}
            <div className="flex items-center justify-between">
              <div>
                <CardTitle className="text-2xl font-bold text-gray-900 dark:text-white">
                  Schedule & Customer Assignment
                </CardTitle>
                <CardDescription className="text-gray-600 dark:text-gray-400 mt-2">
                  Configure your visit schedule and assign customers to optimal time slots
                </CardDescription>
              </div>
              <div className="flex items-center gap-2">
                <div className="h-12 w-12 rounded-full bg-white dark:bg-gray-800 shadow-sm flex items-center justify-center">
                  <Calendar className="h-6 w-6 text-blue-600 dark:text-blue-400" />
                </div>
              </div>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-6 p-6">
          {/* Info Banner */}
          <div className="space-y-2">
            {/* Selected Weekdays Info */}
            <div className="p-3 bg-blue-50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-800 rounded-lg">
              <div className="flex items-start gap-2">
                <Calendar className="h-4 w-4 text-blue-600 mt-0.5" />
                <div className="flex-1">
                  <div className="text-sm font-medium text-blue-800 dark:text-blue-200 mb-1">
                    Active Days
                  </div>
                  <div className="flex flex-wrap gap-1">
                    {Object.entries(selectedWeekdays).map(([day, selected]) => (
                      <Badge
                        key={day}
                        variant={selected ? "default" : "outline"}
                        className={cn(
                          "text-xs capitalize",
                          selected ? "bg-blue-600" : "bg-gray-100 text-gray-500"
                        )}
                      >
                        {day.slice(0, 3)}
                      </Badge>
                    ))}
                  </div>
                </div>
              </div>
            </div>
            
            {/* Holiday Info */}
            {!includeHolidays && holidays.length > 0 && (
              <div className="p-3 bg-amber-50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-800 rounded-lg">
                <div className="flex items-center gap-2 text-sm text-amber-800 dark:text-amber-200">
                  <AlertCircle className="h-4 w-4" />
                  <span>{holidays.length} holidays are excluded from the schedule</span>
                </div>
              </div>
            )}
          </div>
          
          {/* Schedule Type Selection */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label className="text-lg font-semibold">
                {useDateRange ? 'Date Range Schedule' : 'Schedule Type'}
              </Label>
              <div className="flex items-center gap-2">
                {useDateRange && (
                  <Badge variant="default" className="bg-green-600">
                    {moment(startDate).format('MMM DD')} - {moment(endDate).format('MMM DD')}
                  </Badge>
                )}
                <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200">
                  <Clock className="h-3 w-3 mr-1" />
                  {calculateMaxCustomers()} slots per day
                </Badge>
              </div>
            </div>
            <Tabs value={useDateRange ? 'range' : scheduleType} onValueChange={(v) => !useDateRange && setScheduleType(v as any)}>
              <TabsList className={cn(
                "grid w-full grid-cols-3 h-12 bg-gray-100 dark:bg-gray-800",
                useDateRange && "opacity-50 pointer-events-none"
              )}>
                <TabsTrigger 
                  value="daily" 
                  className="gap-2 data-[state=active]:bg-white data-[state=active]:shadow-sm data-[state=active]:text-blue-600"
                  disabled={useDateRange}
                >
                  <CalendarDays className="h-4 w-4" />
                  <span className="font-medium">Daily</span>
                </TabsTrigger>
                <TabsTrigger 
                  value="weekly" 
                  className="gap-2 data-[state=active]:bg-white data-[state=active]:shadow-sm data-[state=active]:text-blue-600"
                  disabled={useDateRange}
                >
                  <CalendarRange className="h-4 w-4" />
                  <span className="font-medium">Weekly</span>
                </TabsTrigger>
                <TabsTrigger 
                  value="monthly" 
                  className="gap-2 data-[state=active]:bg-white data-[state=active]:shadow-sm data-[state=active]:text-blue-600"
                  disabled={useDateRange}
                >
                  <CalendarClock className="h-4 w-4" />
                  <span className="font-medium">Monthly</span>
                </TabsTrigger>
              </TabsList>

              {/* Configuration for each type */}
              <motion.div 
                className="mt-4 p-5 bg-gradient-to-r from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 rounded-xl border border-gray-200 dark:border-gray-700"
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.3 }}
              >
                {useDateRange ? (
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <div className="space-y-1">
                        <Label className="text-sm font-medium">Date Range Selected</Label>
                        <div className="flex items-center gap-3 text-sm text-gray-600 dark:text-gray-400">
                          <div className="flex items-center gap-1">
                            <CalendarRange className="h-4 w-4" />
                            <span>{moment(startDate).format('MMM DD, YYYY')}</span>
                          </div>
                          <ChevronRight className="h-4 w-4" />
                          <div className="flex items-center gap-1">
                            <CalendarRange className="h-4 w-4" />
                            <span>{moment(endDate).format('MMM DD, YYYY')}</span>
                          </div>
                        </div>
                      </div>
                      <div className="text-right">
                        <div className="text-2xl font-bold text-blue-600">
                          {moment(endDate).diff(moment(startDate), 'days') + 1}
                        </div>
                        <div className="text-xs text-gray-500">Total Days</div>
                      </div>
                    </div>
                    <div className="flex items-center gap-4 pt-2 border-t">
                      <Badge variant="outline" className="bg-blue-50">
                        <CheckCircle className="h-3 w-3 mr-1" />
                        {timeSlots.length} working days
                      </Badge>
                      <Badge variant="outline" className="bg-orange-50">
                        <Calendar className="h-3 w-3 mr-1" />
                        {moment(endDate).diff(moment(startDate), 'days') + 1 - timeSlots.length} excluded
                      </Badge>
                    </div>
                  </div>
                ) : scheduleType === 'daily' && (
                  <div className="space-y-2">
                    <Label>Number of days to schedule</Label>
                    <div className="flex items-center gap-2">
                      <Input
                        type="number"
                        value={dailyDays}
                        onChange={(e) => setDailyDays(Number(e.target.value))}
                        min={1}
                        max={14}
                        className="w-20"
                      />
                      <span className="text-sm text-muted-foreground">days</span>
                    </div>
                  </div>
                )}

                {scheduleType === 'weekly' && (
                  <div className="space-y-2">
                    <Label>Number of weeks to schedule</Label>
                    <div className="flex items-center gap-2">
                      <Input
                        type="number"
                        value={weeklyWeeks}
                        onChange={(e) => setWeeklyWeeks(Number(e.target.value))}
                        min={1}
                        max={4}
                        className="w-20"
                      />
                      <span className="text-sm text-muted-foreground">weeks</span>
                    </div>
                  </div>
                )}

                {scheduleType === 'monthly' && (
                  <div className="space-y-2">
                    <Label>Days to display</Label>
                    <div className="flex items-center gap-2">
                      <Input
                        type="number"
                        value={monthlyDays}
                        onChange={(e) => setMonthlyDays(Number(e.target.value))}
                        min={7}
                        max={60}
                        className="w-20"
                      />
                      <span className="text-sm text-muted-foreground">days</span>
                    </div>
                  </div>
                )}

                <Separator className="my-4" />
                
                {/* Default Time Configuration */}
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <Label className="text-sm font-semibold">Default Time Settings</Label>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => {
                        // Apply default times to all slots
                        let maxCustomers = 999;
                        if (maxCustomersOption === 'unlimited') {
                          maxCustomers = 999;
                        } else if (maxCustomersOption === 'custom' && customMaxCustomers) {
                          maxCustomers = customMaxCustomers;
                        } else if (maxCustomersOption === 'auto' && dayStartTime !== '' && dayEndTime !== '' && slotDuration > 0) {
                          maxCustomers = calculateMaxCustomers();
                        }
                        const updatedSlots = timeSlots.map(slot => ({
                          ...slot,
                          startTime: dayStartTime,
                          endTime: dayEndTime,
                          visitDuration: slotDuration,
                          maxCustomers: maxCustomers,
                          customTiming: false
                        }));
                        setTimeSlots(updatedSlots);
                        updateFormData(updatedSlots);
                        toast({
                          title: "Default Times Applied",
                          description: "All time slots have been updated with default settings"
                        });
                      }}
                      className="text-xs"
                    >
                      Apply to All
                    </Button>
                  </div>
                  
                  <div className="space-y-3">
                    <div className="grid grid-cols-3 gap-3">
                      <div className="space-y-2">
                        <Label className="text-xs flex items-center gap-1">
                          <Clock className="h-3 w-3" />
                          Start Time
                        </Label>
                        <input
                          ref={startTimeRef}
                          type="time"
                          value={dayStartTime || '00:00'}
                          onChange={(e) => {
                            const newTime = e.target.value || '00:00';
                            console.log('[StepScheduleCustomers] Start time changed to:', newTime);
                            setDayStartTime(newTime);
                            form.setValue('dayStartsAt', newTime);
                          }}
                          className="h-9 text-sm flex w-full rounded-md border border-input bg-background px-3 py-2 ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                          placeholder="00:00"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-xs">End Time</Label>
                        <input
                          ref={endTimeRef}
                          type="time"
                          value={dayEndTime || '00:00'}
                          onChange={(e) => {
                            const newTime = e.target.value || '00:00';
                            console.log('[StepScheduleCustomers] End time changed to:', newTime);
                            setDayEndTime(newTime);
                            form.setValue('dayEndsBy', newTime);
                          }}
                          className="h-9 text-sm flex w-full rounded-md border border-input bg-background px-3 py-2 ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                          placeholder="00:00"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label className="text-xs">Visit Duration (min)</Label>
                        <Input
                          type="number"
                          value={slotDuration}
                          onChange={(e) => setSlotDuration(Number(e.target.value))}
                          min={5}
                          max={120}
                          step={5}
                          className="h-9 text-sm"
                          placeholder="30"
                        />
                      </div>
                    </div>
                    
                    <div className="space-y-2">
                      <Label className="text-xs flex items-center gap-1">
                        <Users className="h-3 w-3" />
                        Max Customers per Slot
                      </Label>
                      <div className="flex items-center gap-2">
                        <select
                          value={maxCustomersOption}
                          onChange={(e) => {
                            const value = e.target.value as 'auto' | 'custom' | 'unlimited';
                            setMaxCustomersOption(value);
                            if (value === 'unlimited') {
                              setUseMaxCustomers(false);
                            } else {
                              setUseMaxCustomers(true);
                            }
                          }}
                          className="h-9 px-3 text-sm border rounded-md bg-background"
                        >
                          <option value="auto">Auto Calculate</option>
                          <option value="custom">Custom</option>
                          <option value="unlimited">No Limit</option>
                        </select>
                        {maxCustomersOption === 'custom' && (
                          <Input
                            type="number"
                            value={customMaxCustomers || ''}
                            onChange={(e) => setCustomMaxCustomers(e.target.value ? Number(e.target.value) : null)}
                            min={1}
                            max={999}
                            className="h-9 text-sm w-24"
                            placeholder="Enter"
                          />
                        )}
                        {maxCustomersOption === 'auto' && (
                          <span className="text-xs text-muted-foreground">
                            ({dayStartTime === '00:00' && dayEndTime === '00:00' 
                              ? `999 slots`
                              : `${calculateMaxCustomers()} slots`})
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                  
                  <div className="p-2 bg-amber-50 dark:bg-amber-950/20 rounded-lg">
                    <div className="flex items-center gap-2 text-xs text-amber-700 dark:text-amber-300">
                      <AlertCircle className="h-3 w-3" />
                      <span>Individual slots can still be customized by clicking the "Edit Time" button on each slot</span>
                    </div>
                  </div>
                </div>
              </motion.div>
            </Tabs>
          </div>

          <Separator className="my-6" />
          
          {/* Main Content Area */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Customer List */}
            <div className="lg:col-span-1">
              <div className="sticky top-0 space-y-4">
                <div className="flex items-center justify-between">
                  <Label className="text-lg font-semibold flex items-center gap-2">
                    <Users className="h-4 w-4 text-gray-500" />
                    Available Customers
                  </Label>
                  <Badge variant="secondary">
                    {unassignedCustomers.length} available
                  </Badge>
                </div>
                  <div className="relative mt-2">
                    <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                    <Input
                      placeholder="Search customers..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="pl-9"
                    />
                  </div>
                </div>
                
                <ScrollArea className="h-[420px] border-2 border-gray-200 dark:border-gray-700 rounded-xl bg-white dark:bg-gray-900 p-3">
                  {dropdownsLoading.customers ? (
                    <div className="p-4 text-center text-muted-foreground">
                      Loading customers...
                    </div>
                  ) : unassignedCustomers.length === 0 ? (
                    <div className="p-4 text-center text-muted-foreground">
                      No unassigned customers
                    </div>
                  ) : (
                    <div className="space-y-2">
                      {unassignedCustomers.map((customer, index) => (
                        <motion.div
                          key={`unassigned-${customer.UID}-${index}`}
                          draggable
                          onDragStart={() => setDraggedCustomer(customer)}
                          onDragEnd={() => setDraggedCustomer(null)}
                          className="p-3 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-blue-50 dark:hover:bg-blue-950/20 hover:border-blue-300 cursor-move transition-all duration-200 bg-white dark:bg-gray-800"
                          whileHover={{ scale: 1.02 }}
                          whileDrag={{ scale: 1.05, opacity: 0.8 }}
                        >
                          <div className="flex items-start justify-between">
                            <div className="flex-1">
                              <div className="font-medium text-sm text-gray-900 dark:text-white">{customer.Name}</div>
                              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                                <span className="inline-flex items-center gap-1">
                                  <MapPin className="h-3 w-3" />
                                  {customer.Code}
                                </span>
                              </div>
                            </div>
                            <div className="ml-2">
                              <div className="h-6 w-6 rounded-full bg-gray-100 dark:bg-gray-700 flex items-center justify-center">
                                <div className="h-2 w-2 rounded-full bg-green-500"></div>
                              </div>
                            </div>
                          </div>
                        </motion.div>
                      ))}
                    </div>
                  )}
                </ScrollArea>
                
                <div className="mt-4 p-4 bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 rounded-xl border border-blue-200 dark:border-blue-800">
                  <div className="flex items-start gap-3">
                    <div className="h-8 w-8 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center flex-shrink-0">
                      <AlertCircle className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                    </div>
                    <div className="text-xs space-y-1">
                      <p className="font-semibold text-blue-900 dark:text-blue-100">Quick Tips</p>
                      <ul className="space-y-0.5 text-blue-700 dark:text-blue-300">
                        <li>• Drag customers to assign them to time slots</li>
                        <li>• Click the + button to select from a list</li>
                        <li>• Use Auto-Distribute for quick assignment</li>
                      </ul>
                    </div>
                  </div>
                </div>
            </div>

            {/* Schedule Grid */}
            <div className="lg:col-span-2">
              <div className="mb-4 flex items-center justify-between">
                <Label className="text-lg font-semibold flex items-center gap-2">
                  <CalendarDays className="h-4 w-4 text-gray-500" />
                  Schedule Calendar
                </Label>
                <div className="flex items-center gap-2">
                  <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                    <CheckCircle className="h-3 w-3 mr-1" />
                    {selectedCustomers.size} assigned
                  </Badge>
                  <Badge variant="outline" className="bg-orange-50 text-orange-700 border-orange-200">
                    <Calendar className="h-3 w-3 mr-1" />
                    {timeSlots.length} days
                  </Badge>
                </div>
              </div>
              
              <ScrollArea className="h-[500px] border-2 border-gray-200 dark:border-gray-700 rounded-xl bg-gray-50 dark:bg-gray-900">
                <div className="p-4">
                  {timeSlots.length === 0 ? (
                    <div className="flex flex-col items-center justify-center h-[400px] text-center">
                      <div className="h-16 w-16 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center mb-4">
                        <Calendar className="h-8 w-8 text-gray-400" />
                      </div>
                      <p className="text-gray-500 dark:text-gray-400 font-medium mb-2">
                        No Time Slots Generated
                      </p>
                      <p className="text-sm text-gray-400 dark:text-gray-500 max-w-sm">
                        Please set the start time, end time, and visit duration in the Default Time Settings above to generate time slots.
                      </p>
                    </div>
                  ) : scheduleType === 'weekly' ? (
                    // Weekly View - Group by weeks
                    <div className="space-y-6">
                      {Array.from({ length: weeklyWeeks }, (_, weekIndex) => {
                        const weekSlots = timeSlots.filter(slot => 
                          slot.id.startsWith(`week-${weekIndex}-`)
                        );
                        const weekStart = weekSlots[0]?.date;
                        const weekEnd = weekSlots[weekSlots.length - 1]?.date;
                        
                        return (
                          <div key={weekIndex} className="space-y-2">
                            <div className="font-semibold text-sm text-muted-foreground">
                              Week {weekIndex + 1}
                              {weekStart && weekEnd && (
                                <span className="ml-2">
                                  ({moment(weekStart).format('MMM DD')} - {moment(weekEnd).format('MMM DD')})
                                </span>
                              )}
                            </div>
                            <div className="grid gap-2">
                              {weekSlots.map((slot) => {
                                const slotDate = moment(slot.date);
                                // Check if this day is excluded based on user selection
                                const weekdayKey = getWeekdayKey(slotDate.day());
                                const isExcludedDay = !selectedWeekdays[weekdayKey];
                                const isHolidayDay = isHoliday(slotDate);
                                return (
                                  <TimeSlotCard
                                    key={slot.id}
                                    slot={slot}
                                    onDrop={handleCustomerDrop}
                                    onRemove={removeCustomerFromSlot}
                                    draggedCustomer={draggedCustomer}
                                    onAddCustomer={(slotId) => {
                                      setModalTargetSlot(slotId);
                                      setShowCustomerModal(true);
                                    }}
                                    isWeekend={isExcludedDay}
                                    isHoliday={isHolidayDay}
                                    onEdit={(slotId) => setEditingSlot(slotId)}
                                    onUpdateTiming={updateSlotTiming}
                                    isEditing={editingSlot === slot.id}
                                  />
                                );
                              })}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  ) : (
                    // Daily/Monthly View - Simple list
                    <div className="space-y-2">
                      {timeSlots.map((slot) => {
                        const slotDate = moment(slot.date);
                        // Check if this day is excluded based on user selection
                        const weekdayKey = getWeekdayKey(slotDate.day());
                        const isExcludedDay = !selectedWeekdays[weekdayKey];
                        const isHolidayDay = isHoliday(slotDate);
                        return (
                          <TimeSlotCard
                            key={slot.id}
                            slot={slot}
                            onDrop={handleCustomerDrop}
                            onRemove={removeCustomerFromSlot}
                            draggedCustomer={draggedCustomer}
                            onAddCustomer={(slotId) => {
                              setModalTargetSlot(slotId);
                              setShowCustomerModal(true);
                            }}
                            isWeekend={isExcludedDay}
                            isHoliday={isHolidayDay}
                            onEdit={(slotId) => setEditingSlot(slotId)}
                            onUpdateTiming={updateSlotTiming}
                            isEditing={editingSlot === slot.id}
                          />
                        );
                      })}
                    </div>
                  )}
                </div>
              </ScrollArea>
            </div>
          </div>
          
          <Separator className="my-6" />
          
          {/* Summary */}
          <div className="p-5 bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 rounded-xl border border-green-200 dark:border-green-800">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                  <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400" />
                </div>
                <div>
                  <p className="font-semibold text-gray-900 dark:text-white">Schedule Summary</p>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    {timeSlots.length} days scheduled • {selectedCustomers.size} of {routeCustomers.length} customers assigned
                  </p>
                </div>
              </div>
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  // Auto-distribute unassigned customers
                  const unassigned = [...unassignedCustomers];
                  const updatedSlots = [...timeSlots];
                  const newSelectedCustomers = new Set(selectedCustomers);
                  
                  for (const customer of unassigned) {
                    // Find first available slot
                    for (let i = 0; i < updatedSlots.length; i++) {
                      const slot = updatedSlots[i];
                      if (slot.customers.length < slot.maxCustomers) {
                        slot.customers.push(customer);
                        newSelectedCustomers.add(customer.UID);
                        break;
                      }
                    }
                  }
                  
                  setTimeSlots(updatedSlots);
                  setSelectedCustomers(newSelectedCustomers);
                  updateFormData(updatedSlots);
                  
                  toast({
                    title: "Customers Distributed",
                    description: `${unassigned.length} customers have been assigned to available time slots`,
                  });
                }}
                disabled={unassignedCustomers.length === 0}
              >
                Auto-Distribute Remaining
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Customer Selector Modal */}
      <Dialog open={showCustomerModal} onOpenChange={setShowCustomerModal}>
        <DialogContent className="max-w-3xl max-h-[85vh] p-0 overflow-hidden">
          <DialogHeader className="px-6 py-4 bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 border-b">
            <DialogTitle className="text-xl font-bold flex items-center gap-2">
              <Users className="h-5 w-5 text-blue-600" />
              Select Customer to Assign
            </DialogTitle>
          </DialogHeader>
          <div className="p-6 space-y-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search by name or code..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 h-11 border-2 focus:border-blue-500"
                autoFocus
              />
            </div>
            
            <div className="flex items-center justify-between text-sm">
              <span className="text-gray-600 dark:text-gray-400">
                {unassignedCustomers.length} customers available
              </span>
              <Badge variant="outline" className="bg-blue-50">
                Click to assign
              </Badge>
            </div>
            
            <ScrollArea className="h-[450px] border-2 border-gray-200 dark:border-gray-700 rounded-xl bg-gray-50 dark:bg-gray-900 p-3">
              {unassignedCustomers.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full text-center">
                  <div className="h-16 w-16 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center mb-4">
                    <Users className="h-8 w-8 text-gray-400" />
                  </div>
                  <p className="text-gray-500 dark:text-gray-400 font-medium">
                    No unassigned customers available
                  </p>
                  <p className="text-sm text-gray-400 dark:text-gray-500 mt-1">
                    All customers have been assigned to time slots
                  </p>
                </div>
              ) : (
                <div className="grid gap-2">
                  {unassignedCustomers.map((customer) => (
                    <motion.div
                      key={customer.UID}
                      className="p-4 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-lg hover:bg-blue-50 dark:hover:bg-blue-950/20 hover:border-blue-300 dark:hover:border-blue-700 cursor-pointer transition-all group"
                      onClick={() => {
                        if (modalTargetSlot) {
                          handleCustomerDrop(customer, modalTargetSlot);
                          setShowCustomerModal(false);
                          setModalTargetSlot(null);
                          toast({
                            title: "Customer Assigned",
                            description: `${customer.Name} has been assigned to the selected time slot`,
                          });
                        }
                      }}
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                          <div className="h-10 w-10 rounded-full bg-gradient-to-br from-blue-100 to-indigo-100 dark:from-blue-900 dark:to-indigo-900 flex items-center justify-center">
                            <Users className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                          </div>
                          <div>
                            <div className="font-semibold text-gray-900 dark:text-white">{customer.Name}</div>
                            <div className="text-sm text-gray-500 dark:text-gray-400 flex items-center gap-2 mt-1">
                              <MapPin className="h-3 w-3" />
                              <span>{customer.Code}</span>
                              {customer.Address && (
                                <>
                                  <span className="text-gray-400">•</span>
                                  <span className="truncate max-w-[200px]">{customer.Address}</span>
                                </>
                              )}
                            </div>
                          </div>
                        </div>
                        <ChevronRight className="h-5 w-5 text-gray-400 group-hover:text-blue-600 transition-colors" />
                      </div>
                    </motion.div>
                  ))}
                </div>
              )}
            </ScrollArea>
          </div>
        </DialogContent>
      </Dialog>
    </motion.div>
  );
};

// Time Slot Card Component
const TimeSlotCard: React.FC<{
  slot: TimeSlot;
  onDrop: (customer: Customer, slotId: string) => void;
  onRemove: (customerId: string, slotId: string) => void;
  draggedCustomer: Customer | null;
  onAddCustomer: (slotId: string) => void;
  isHoliday?: boolean;
  isWeekend?: boolean;  // Actually means "isExcludedDay" based on user selection
  isEditing?: boolean;
  onEdit?: (slotId: string) => void;
  onUpdateTiming?: (slotId: string, startTime: string, endTime: string, duration: number) => void;
}> = ({ slot, onDrop, onRemove, draggedCustomer, onAddCustomer, isHoliday, isWeekend, isEditing, onEdit, onUpdateTiming }) => {
  const [isDragOver, setIsDragOver] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);
  const [tempStartTime, setTempStartTime] = useState(slot.startTime);
  const [tempEndTime, setTempEndTime] = useState(slot.endTime);
  const [tempDuration, setTempDuration] = useState(slot.visitDuration || 30);
  
  const calculateMaxCustomers = (start: string, end: string, duration: number) => {
    const [startHour, startMin] = start.split(':').map(Number);
    const [endHour, endMin] = end.split(':').map(Number);
    const totalMinutes = (endHour * 60 + endMin) - (startHour * 60 + startMin);
    return Math.floor(totalMinutes / duration);
  };
  
  const handleSaveTiming = () => {
    if (onUpdateTiming) {
      onUpdateTiming(slot.id, tempStartTime, tempEndTime, tempDuration);
    }
    setShowEditForm(false);
  };
  
  return (
    <motion.div
      onDragOver={(e) => {
        e.preventDefault();
        setIsDragOver(true);
      }}
      onDragLeave={() => setIsDragOver(false)}
      onDrop={(e) => {
        e.preventDefault();
        setIsDragOver(false);
        if (draggedCustomer) {
          onDrop(draggedCustomer, slot.id);
        }
      }}
      className={cn(
        "border rounded-lg p-4 transition-all",
        isDragOver && "bg-accent border-accent-foreground",
        (slot.maxCustomers !== 999 && slot.customers.length === slot.maxCustomers) && "bg-muted/50",
        !isDragOver && (slot.maxCustomers === 999 || slot.customers.length < slot.maxCustomers) && "hover:shadow-sm"
      )}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
    >
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-start gap-3">
          <div className={cn(
            "h-10 w-10 rounded-lg flex items-center justify-center flex-shrink-0",
            isWeekend ? "bg-gradient-to-br from-orange-100 to-yellow-100 dark:from-orange-900 dark:to-yellow-900" :
            isHoliday ? "bg-gradient-to-br from-red-100 to-pink-100 dark:from-red-900 dark:to-pink-900" :
            "bg-gradient-to-br from-blue-100 to-indigo-100 dark:from-blue-900 dark:to-indigo-900"
          )}>
            <Calendar className={cn(
              "h-5 w-5",
              isWeekend ? "text-orange-600 dark:text-orange-400" :
              isHoliday ? "text-red-600 dark:text-red-400" :
              "text-blue-600 dark:text-blue-400"
            )} />
          </div>
          <div className="flex-1">
            <div className="flex items-center gap-2">
              <span className="font-semibold text-gray-900 dark:text-white">
                {moment(slot.date).format('dddd, MMM DD, YYYY')}
              </span>
              {isWeekend && (
                <Badge variant="outline" className="text-xs bg-orange-50 text-orange-700 border-orange-200">
                  Weekend
                </Badge>
              )}
              {isHoliday && (
                <Badge variant="outline" className="text-xs bg-red-50 text-red-700 border-red-200">
                  Holiday
                </Badge>
              )}
              {slot.customTiming && (
                <Badge variant="secondary" className="text-xs">
                  Custom Time
                </Badge>
              )}
            </div>
            {!showEditForm ? (
              <div className="flex items-center gap-2 text-xs text-gray-500 dark:text-gray-400 mt-1">
                <Clock className="h-3 w-3" />
                <span>{slot.startTime} - {slot.endTime}</span>
                <span className="text-gray-400">•</span>
                <span>{slot.visitDuration || 30} min/visit</span>
                <span className="text-gray-400">•</span>
                <span className={cn(
                  "font-medium",
                  slot.customers.length === slot.maxCustomers ? "text-red-600" : "text-green-600"
                )}>
                  {slot.maxCustomers - slot.customers.length} slots available
                </span>
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-5 px-2 ml-2"
                  onClick={() => setShowEditForm(true)}
                >
                  <Pencil className="h-3 w-3 mr-1" />
                  Edit Time
                </Button>
              </div>
            ) : (
              <div className="mt-2 p-3 bg-gray-50 dark:bg-gray-900 rounded-lg border">
                <div className="grid grid-cols-4 gap-2">
                  <div>
                    <Label className="text-xs">Start Time</Label>
                    <Input
                      type="time"
                      value={tempStartTime}
                      onChange={(e) => setTempStartTime(e.target.value)}
                      className="h-8 text-xs"
                    />
                  </div>
                  <div>
                    <Label className="text-xs">End Time</Label>
                    <Input
                      type="time"
                      value={tempEndTime}
                      onChange={(e) => setTempEndTime(e.target.value)}
                      className="h-8 text-xs"
                    />
                  </div>
                  <div>
                    <Label className="text-xs">Duration (min)</Label>
                    <Input
                      type="number"
                      value={tempDuration}
                      onChange={(e) => setTempDuration(Number(e.target.value))}
                      min={15}
                      max={120}
                      step={15}
                      className="h-8 text-xs"
                    />
                  </div>
                  <div className="flex items-end gap-1">
                    <Button
                      size="sm"
                      className="h-8 px-2 text-xs"
                      onClick={handleSaveTiming}
                    >
                      <Check className="h-3 w-3" />
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      className="h-8 px-2 text-xs"
                      onClick={() => {
                        setTempStartTime(slot.startTime);
                        setTempEndTime(slot.endTime);
                        setTempDuration(slot.visitDuration || 30);
                        setShowEditForm(false);
                      }}
                    >
                      <X className="h-3 w-3" />
                    </Button>
                  </div>
                </div>
                <div className="mt-2 text-xs text-gray-500">
                  Max customers: {slot.maxCustomers === 999 ? 'Unlimited' : calculateMaxCustomers(tempStartTime, tempEndTime, tempDuration)}
                </div>
              </div>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Badge 
            variant={slot.customers.length === 0 ? "outline" : slot.customers.length === slot.maxCustomers ? "destructive" : "default"}
            className="font-semibold"
          >
            {slot.customers.length}/{slot.maxCustomers}
          </Badge>
        </div>
      </div>
      
      {slot.customers.length > 0 && (
        <div className="space-y-2 mt-4">
          <Separator className="mb-3" />
          {slot.customers.map((customer, index) => (
            <motion.div
              key={customer.UID}
              className="flex items-center justify-between p-2.5 bg-gradient-to-r from-gray-50 to-gray-100 dark:from-gray-700 dark:to-gray-800 rounded-lg group hover:shadow-sm transition-all"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: index * 0.05 }}
            >
              <div className="flex items-center gap-3">
                <div className="h-7 w-7 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center flex-shrink-0">
                  <span className="text-xs font-bold text-blue-600 dark:text-blue-400">{index + 1}</span>
                </div>
                <div>
                  <div className="font-medium text-sm text-gray-900 dark:text-white">{customer.Name}</div>
                  <div className="text-xs text-gray-500 dark:text-gray-400">{customer.Code}</div>
                </div>
              </div>
              <Button
                variant="ghost"
                size="sm"
                className="h-7 w-7 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
                onClick={() => onRemove(customer.UID, slot.id)}
              >
                <X className="h-4 w-4 text-red-500" />
              </Button>
            </motion.div>
          ))}
        </div>
      )}
      
      {slot.customers.length < slot.maxCustomers && (
        <Button
          variant="outline"
          size="sm"
          className="w-full mt-3 h-10 text-sm font-medium border-dashed border-2 hover:border-solid hover:bg-blue-50 hover:text-blue-700 hover:border-blue-300 dark:hover:bg-blue-950/20 dark:hover:text-blue-400 dark:hover:border-blue-700 transition-all"
          onClick={() => onAddCustomer(slot.id)}
        >
          <Plus className="h-4 w-4 mr-2" />
          Add Customer
        </Button>
      )}
    </motion.div>
  );
};