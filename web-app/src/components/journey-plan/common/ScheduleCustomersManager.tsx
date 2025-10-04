import React, { useState, useEffect, useRef } from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
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
import { holidayService } from '@/services/holidayService';

// Common interfaces
export interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  Type?: string;
  Status?: string;
  SeqNo?: number;
}

export interface TimeSlot {
  id: string;
  dayName: string;
  startTime: string;
  endTime: string;
  customers: Customer[];
  maxCustomers: number;
  customTiming?: boolean;
  visitDuration?: number;
  scheduleIndex: number;
  scheduleType: string;
}

export interface ScheduleConfig {
  scheduleType: 'daily' | 'weekly' | 'fortnightly' | 'monthly';
  dailyDays: number;
  weeklyWeeks: number;
  fortnightlyWeeks: number;
  monthlyDays: number;
  useTimeSettings: boolean;
  dayStartTime: string;
  dayEndTime: string;
  slotDuration: number;
  includeWeekends: boolean;
  includeHolidays: boolean;
  selectedWeekdays: Record<string, boolean>;
  maxCustomersOption: 'auto' | 'custom' | 'unlimited';
  customMaxCustomers: number | null;
}

export interface ScheduleCustomersManagerProps {
  // Form integration
  form: UseFormReturn<any>;
  
  // Data
  routeCustomers: Customer[];
  dropdownsLoading?: any;
  
  // Configuration
  config?: Partial<ScheduleConfig>;
  
  // Callbacks
  onScheduleUpdate?: (timeSlots: TimeSlot[], config: ScheduleConfig) => void;
  onCustomersAssigned?: (selectedCustomers: Set<string>) => void;
  
  // UI customization
  title?: string;
  description?: string;
  showTimeTemplates?: boolean;
  showHolidayManager?: boolean;
  showWeekdaySelector?: boolean;
  showAutoDistribute?: boolean;
  
  // Feature toggles
  enableDragDrop?: boolean;
  enableCustomTiming?: boolean;
}

export const ScheduleCustomersManager: React.FC<ScheduleCustomersManagerProps> = ({
  form,
  routeCustomers,
  dropdownsLoading,
  config: initialConfig,
  onScheduleUpdate,
  onCustomersAssigned,
  title = "Schedule & Customer Assignment",
  description = "Configure your visit schedule and assign customers to optimal time slots",
  showTimeTemplates = true,
  showHolidayManager = true,
  showWeekdaySelector = true,
  showAutoDistribute = true,
  enableDragDrop = true,
  enableCustomTiming = true,
}) => {
  const { toast } = useToast();
  
  // Configuration state
  const [config, setConfig] = useState<ScheduleConfig>({
    scheduleType: 'daily',
    dailyDays: 6,
    weeklyWeeks: 2,
    fortnightlyWeeks: 4,
    monthlyDays: 30,
    useTimeSettings: false,
    dayStartTime: '00:00',
    dayEndTime: '00:00',
    slotDuration: 30,
    includeWeekends: false,
    includeHolidays: false,
    selectedWeekdays: {
      monday: true,
      tuesday: true,
      wednesday: true,
      thursday: true,
      friday: true,
      saturday: false,
      sunday: false,
    },
    maxCustomersOption: 'auto',
    customMaxCustomers: null,
    ...initialConfig,
  });

  // Core state
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
  const [selectedCustomers, setSelectedCustomers] = useState<Set<string>>(new Set());
  const [searchTerm, setSearchTerm] = useState('');
  const [draggedCustomer, setDraggedCustomer] = useState<Customer | null>(null);
  
  // Modal state
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [modalTargetSlot, setModalTargetSlot] = useState<string | null>(null);
  
  // Time slot editing
  const [editingSlot, setEditingSlot] = useState<string | null>(null);
  
  // Holiday state
  const [holidays, setHolidays] = useState<Date[]>([]);
  const [loadingHolidays, setLoadingHolidays] = useState(false);
  
  // UI toggles
  const [showWeekdaySelectorPanel, setShowWeekdaySelectorPanel] = useState(false);
  const [showHolidayManagerPanel, setShowHolidayManagerPanel] = useState(false);
  
  // Time input refs
  const startTimeRef = useRef<HTMLInputElement>(null);
  const endTimeRef = useRef<HTMLInputElement>(null);
  
  // Time templates
  const timeTemplates = [
    { name: 'Morning', startTime: '08:00', endTime: '12:00', duration: 30 },
    { name: 'Afternoon', startTime: '12:00', endTime: '17:00', duration: 30 },
    { name: 'Evening', startTime: '17:00', endTime: '20:00', duration: 20 },
    { name: 'Full Day', startTime: '00:00', endTime: '00:00', duration: 30 },
  ];

  // Initialize from form data
  useEffect(() => {
    const formStartTime = form.watch('plannedStartTime') || form.watch('dayStartsAt');
    const formEndTime = form.watch('plannedEndTime') || form.watch('dayEndsBy');
    const formDuration = form.watch('defaultDuration');
    
    if (formStartTime || formEndTime || formDuration) {
      setConfig(prev => ({
        ...prev,
        dayStartTime: formStartTime || prev.dayStartTime,
        dayEndTime: formEndTime || prev.dayEndTime,
        slotDuration: formDuration || prev.slotDuration
      }));
    }
  }, []);

  // Load holidays
  useEffect(() => {
    const orgUID = form.getValues('orgUID');
    if (orgUID && showHolidayManager) {
      loadHolidays();
    }
  }, [form.watch('orgUID'), showHolidayManager]);

  const loadHolidays = async () => {
    setLoadingHolidays(true);
    try {
      const orgUID = form.getValues('orgUID');
      if (orgUID) {
        const currentYear = new Date().getFullYear();
        const [currentYearHolidays, nextYearHolidays] = await Promise.all([
          holidayService.getHolidaysForYear(orgUID, currentYear),
          holidayService.getHolidaysForYear(orgUID, currentYear + 1)
        ]);
        const allHolidays = [...currentYearHolidays, ...nextYearHolidays];
        setHolidays(allHolidays.map(h => new Date(h.HolidayDate)));
      }
    } catch (error) {
      console.error('Error loading holidays:', error);
      // Set default holidays as fallback
      setHolidays([
        new Date('2025-01-01'),
        new Date('2025-01-26'),
        new Date('2025-03-17'),
        new Date('2025-08-15'),
        new Date('2025-10-02'),
        new Date('2025-12-25'),
      ]);
    } finally {
      setLoadingHolidays(false);
    }
  };

  // Generate time slots when config changes
  useEffect(() => {
    generateTimeSlots();
  }, [
    config.scheduleType,
    config.dailyDays,
    config.weeklyWeeks,
    config.fortnightlyWeeks,
    config.monthlyDays,
    config.dayStartTime,
    config.dayEndTime,
    config.slotDuration,
    config.includeWeekends,
    config.includeHolidays,
    config.selectedWeekdays
  ]);

  // Update parent when timeSlots or config changes
  useEffect(() => {
    if (onScheduleUpdate) {
      onScheduleUpdate(timeSlots, config);
    }
    updateFormData(timeSlots);
  }, [timeSlots, config]);

  // Update parent when customers change
  useEffect(() => {
    if (onCustomersAssigned) {
      onCustomersAssigned(selectedCustomers);
    }
  }, [selectedCustomers]);


  const calculateMaxCustomers = (startTime?: string, endTime?: string, duration?: number) => {
    if (config.maxCustomersOption === 'unlimited') return 999;
    if (config.maxCustomersOption === 'custom' && config.customMaxCustomers) {
      return config.customMaxCustomers;
    }

    const start = moment(startTime || config.dayStartTime, 'HH:mm');
    const end = moment(endTime || config.dayEndTime, 'HH:mm');
    const dur = duration || config.slotDuration;

    if (config.dayStartTime === '00:00' && config.dayEndTime === '00:00') {
      return 999;
    }

    let totalMinutes = end.diff(start, 'minutes');
    if (totalMinutes <= 0) totalMinutes += 24 * 60;

    return Math.floor(totalMinutes / dur);
  };

  const generateTimeSlots = () => {
    const slots: TimeSlot[] = [];
    const maxCustomers = calculateMaxCustomers();
    
    const getSelectedWeekdays = () => {
      return Object.entries(config.selectedWeekdays)
        .filter(([_, selected]) => selected)
        .map(([day, _]) => day.charAt(0).toUpperCase() + day.slice(1));
    };

    if (config.scheduleType === 'daily') {
      const selectedDays = getSelectedWeekdays();
      for (let i = 0; i < config.dailyDays; i++) {
        const dayIndex = i % selectedDays.length;
        slots.push({
          id: `daily-${i}`,
          dayName: selectedDays[dayIndex],
          startTime: config.dayStartTime,
          endTime: config.dayEndTime,
          customers: [],
          maxCustomers,
          visitDuration: config.slotDuration,
          scheduleIndex: i,
          scheduleType: 'daily'
        });
      }
    } else if (config.scheduleType === 'weekly') {
      const selectedDays = getSelectedWeekdays();
      for (let w = 0; w < config.weeklyWeeks; w++) {
        selectedDays.forEach((dayName, d) => {
          slots.push({
            id: `weekly-${w}-${d}`,
            dayName,
            startTime: config.dayStartTime,
            endTime: config.dayEndTime,
            customers: [],
            maxCustomers,
            visitDuration: config.slotDuration,
            scheduleIndex: w,
            scheduleType: 'weekly'
          });
        });
      }
    } else if (config.scheduleType === 'fortnightly') {
      const selectedDays = getSelectedWeekdays();
      const fortnights = Math.floor(config.fortnightlyWeeks / 2);
      for (let f = 0; f < fortnights; f++) {
        selectedDays.forEach((dayName, d) => {
          slots.push({
            id: `fortnightly-${f}-${d}`,
            dayName,
            startTime: config.dayStartTime,
            endTime: config.dayEndTime,
            customers: [],
            maxCustomers,
            visitDuration: config.slotDuration,
            scheduleIndex: f,
            scheduleType: 'fortnightly'
          });
        });
      }
    } else if (config.scheduleType === 'monthly') {
      const selectedDays = getSelectedWeekdays();
      const totalSlots = Math.min(config.monthlyDays, selectedDays.length * 4); // Max 4 weeks per month
      for (let i = 0; i < totalSlots; i++) {
        const dayIndex = i % selectedDays.length;
        const weekIndex = Math.floor(i / selectedDays.length);
        slots.push({
          id: `monthly-${i}`,
          dayName: selectedDays[dayIndex],
          startTime: config.dayStartTime,
          endTime: config.dayEndTime,
          customers: [],
          maxCustomers,
          visitDuration: config.slotDuration,
          scheduleIndex: weekIndex,
          scheduleType: 'monthly'
        });
      }
    }

    setTimeSlots(slots);
    onScheduleUpdate?.(slots, config);
  };

  const updateFormData = (slots: TimeSlot[]) => {
    if (slots.length > 0) {
      form.setValue('dayStartsAt', config.dayStartTime);
      form.setValue('dayEndsBy', config.dayEndTime);
      form.setValue('plannedStartTime', config.dayStartTime);
      form.setValue('plannedEndTime', config.dayEndTime);
      form.setValue('defaultDuration', config.slotDuration);

      // Collect customers with times
      const customersWithTimes: any[] = [];
      slots.forEach(slot => {
        slot.customers.forEach((customer, index) => {
          const startTime = moment(slot.startTime, 'HH:mm').add(index * config.slotDuration, 'minutes').format('HH:mm');
          const endTime = moment(startTime, 'HH:mm').add(config.slotDuration, 'minutes').format('HH:mm');
          
          customersWithTimes.push({
            customerUID: customer.UID,
            startTime,
            endTime,
            visitDuration: config.slotDuration,
            visitDay: slot.dayName
          });
        });
      });

      form.setValue('selectedCustomersWithTimes', customersWithTimes);
    }
  };

  const updateConfig = (updates: Partial<ScheduleConfig>) => {
    setConfig(prev => ({ ...prev, ...updates }));
  };

  const handleCustomerDrop = (customer: Customer, slotId: string) => {
    if (!enableDragDrop) return;

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
      return updated;
    });
    setSelectedCustomers(prev => {
      const newSet = new Set(prev);
      newSet.delete(customerId);
      return newSet;
    });
  };

  const updateSlotTiming = (slotId: string, startTime: string, endTime: string, duration: number) => {
    if (!enableCustomTiming) return;

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
    updateConfig({
      dayStartTime: template.startTime,
      dayEndTime: template.endTime,
      slotDuration: template.duration
    });
    
    toast({
      title: "Template Applied",
      description: `${template.name} timing applied to schedule`,
    });
  };

  const autoDistributeCustomers = () => {
    if (!showAutoDistribute) return;

    const unassignedCustomers = routeCustomers.filter(
      customer => !selectedCustomers.has(customer.UID)
    );
    
    const updatedSlots = [...timeSlots];
    const newSelectedCustomers = new Set(selectedCustomers);
    
    for (const customer of unassignedCustomers) {
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
    
    toast({
      title: "Customers Distributed",
      description: `${unassignedCustomers.length} customers assigned automatically`,
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
      className="w-full"
    >
      <Card className="w-full">
        <CardHeader className="pb-3 px-3">
          <div className="space-y-3">
            {/* Controls */}
            <div className="flex justify-end items-start">
              {/* Control buttons */}
              <div className="flex gap-3">
                  {showWeekdaySelector && (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => setShowWeekdaySelectorPanel(!showWeekdaySelectorPanel)}
                      className="gap-2"
                    >
                      <Calendar className="h-4 w-4 text-blue-500" />
                      Weekdays
                      <Badge variant="secondary">
                        {Object.values(config.selectedWeekdays).filter(v => v).length}
                      </Badge>
                    </Button>
                  )}
                  
                  {showHolidayManager && (
                    <div className="flex items-center gap-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => setShowHolidayManagerPanel(!showHolidayManagerPanel)}
                        className="gap-2"
                      >
                        <CalendarDays className="h-4 w-4 text-red-500" />
                        Holidays
                        {holidays.length > 0 && (
                          <Badge variant="secondary">{holidays.length}</Badge>
                        )}
                      </Button>
                      <Switch
                        id="holidays"
                        checked={config.includeHolidays}
                        onCheckedChange={(checked) => updateConfig({ includeHolidays: checked })}
                      />
                    </div>
                  )}
                </div>
              </div>

            <div className="flex items-center justify-between">
              <div>
                <CardTitle className="text-2xl font-bold text-gray-900 dark:text-white">
                  {title}
                </CardTitle>
                <CardDescription className="text-gray-600 dark:text-gray-400 mt-2">
                  {description}
                </CardDescription>
              </div>
              {/* Removed calendar icon from title */}
            </div>
          </div>
        </CardHeader>

        <CardContent className="space-y-4 px-3 py-3">
          {/* Schedule Configuration */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label className="text-lg font-semibold">Schedule Configuration</Label>
              <div className="flex items-center gap-2">
                <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200">
                  <Clock className="h-3 w-3 mr-1" />
                  {calculateMaxCustomers()} slots per day
                </Badge>
              </div>
            </div>
            
            <Tabs 
              value={config.scheduleType} 
              onValueChange={(v) => updateConfig({ scheduleType: v as any })}
            >
              <TabsList className="grid w-full grid-cols-4 h-12 bg-gray-100 dark:bg-gray-800">
                <TabsTrigger value="daily" className="gap-2">
                  <CalendarDays className="h-4 w-4" />
                  Daily
                </TabsTrigger>
                <TabsTrigger value="weekly" className="gap-2">
                  <CalendarRange className="h-4 w-4" />
                  Weekly
                </TabsTrigger>
                <TabsTrigger value="fortnightly" className="gap-2">
                  <CalendarRange className="h-4 w-4" />
                  Fortnightly
                </TabsTrigger>
                <TabsTrigger value="monthly" className="gap-2">
                  <CalendarClock className="h-4 w-4" />
                  Monthly
                </TabsTrigger>
              </TabsList>

              <div className="mt-4 p-5 bg-gradient-to-r from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 rounded-xl border">
                {/* Time Configuration Radio */}
                <div className="space-y-4">
                  <Label className="text-sm font-semibold">Time Configuration</Label>
                  
                  <RadioGroup 
                    value={config.useTimeSettings ? "custom" : "default"} 
                    onValueChange={(value) => updateConfig({ useTimeSettings: value === "custom" })}
                    className="grid grid-cols-2 gap-4"
                  >
                    <div className="flex items-center space-x-2 p-3 rounded-lg border bg-white dark:bg-gray-800">
                      <RadioGroupItem value="default" id="default-time" />
                      <Label htmlFor="default-time" className="cursor-pointer">
                        <div>
                          <div className="font-medium">Use Default Times</div>
                          <div className="text-xs text-muted-foreground">System will auto-assign times</div>
                        </div>
                      </Label>
                    </div>
                    <div className="flex items-center space-x-2 p-3 rounded-lg border bg-white dark:bg-gray-800">
                      <RadioGroupItem value="custom" id="custom-time" />
                      <Label htmlFor="custom-time" className="cursor-pointer">
                        <div>
                          <div className="font-medium">Set Custom Times</div>
                          <div className="text-xs text-muted-foreground">Configure specific timing</div>
                        </div>
                      </Label>
                    </div>
                  </RadioGroup>

                  {/* Show time inputs only if custom is selected */}
                  {config.useTimeSettings && (
                    <div className="space-y-3 mt-4 pt-4 border-t">
                  <div className="flex items-center justify-between">
                    <Label className="text-sm font-semibold">Default Time Settings</Label>
                    {showTimeTemplates && (
                      <div className="flex gap-1">
                        {timeTemplates.map((template) => (
                          <Button
                            key={template.name}
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={() => applyTimeTemplate(template)}
                            className="text-xs h-7"
                          >
                            {template.name}
                          </Button>
                        ))}
                      </div>
                    )}
                  </div>
                  
                  <div className="grid grid-cols-3 gap-3">
                    <div className="space-y-2">
                      <Label className="text-xs">Start Time</Label>
                      <input
                        ref={startTimeRef}
                        type="time"
                        value={config.dayStartTime}
                        onChange={(e) => updateConfig({ dayStartTime: e.target.value || '00:00' })}
                        className="h-9 text-sm flex w-full rounded-md border border-input bg-background px-3 py-2"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs">End Time</Label>
                      <input
                        ref={endTimeRef}
                        type="time"
                        value={config.dayEndTime}
                        onChange={(e) => updateConfig({ dayEndTime: e.target.value || '00:00' })}
                        className="h-9 text-sm flex w-full rounded-md border border-input bg-background px-3 py-2"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs">Duration (min)</Label>
                      <Input
                        type="number"
                        value={config.slotDuration}
                        onChange={(e) => updateConfig({ slotDuration: Number(e.target.value) })}
                        min={5}
                        max={120}
                        step={5}
                        className="h-9 text-sm"
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label className="text-xs">Max Customers per Slot</Label>
                    <div className="flex items-center gap-2">
                      <select
                        value={config.maxCustomersOption}
                        onChange={(e) => {
                          const value = e.target.value as 'auto' | 'custom' | 'unlimited';
                          updateConfig({ maxCustomersOption: value });
                        }}
                        className="h-9 px-3 text-sm border rounded-md bg-background"
                      >
                        <option value="auto">Auto Calculate</option>
                        <option value="custom">Custom</option>
                        <option value="unlimited">No Limit</option>
                      </select>
                      {config.maxCustomersOption === 'custom' && (
                        <Input
                          type="number"
                          value={config.customMaxCustomers || ''}
                          onChange={(e) => updateConfig({ 
                            customMaxCustomers: e.target.value ? Number(e.target.value) : null 
                          })}
                          min={1}
                          max={999}
                          className="h-9 text-sm w-24"
                          placeholder="Enter"
                        />
                      )}
                    </div>
                  </div>
                    </div>
                  )}
                </div>
              </div>
            </Tabs>
          </div>

          <Separator className="my-6" />
          
          {/* Main Content Area */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
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
                
                <div className="relative">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search customers..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-9"
                  />
                </div>
                
                <ScrollArea className="h-[420px] border-2 border-gray-200 dark:border-gray-700 rounded-xl bg-white dark:bg-gray-900 p-2">
                  {dropdownsLoading?.customers ? (
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
                          draggable={enableDragDrop}
                          onDragStart={() => enableDragDrop && setDraggedCustomer(customer)}
                          onDragEnd={() => enableDragDrop && setDraggedCustomer(null)}
                          className={cn(
                            "p-3 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 transition-all duration-200",
                            enableDragDrop && "cursor-move hover:bg-blue-50 dark:hover:bg-blue-950/20 hover:border-blue-300"
                          )}
                          whileHover={enableDragDrop ? { scale: 1.02 } : {}}
                          whileDrag={enableDragDrop ? { scale: 1.05, opacity: 0.8 } : {}}
                        >
                          <div className="flex items-start justify-between">
                            <div className="flex-1">
                              <div className="font-medium text-sm text-gray-900 dark:text-white">
                                {customer.Name}
                              </div>
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
                <div className="p-3">
                  {timeSlots.length === 0 ? (
                    <div className="flex flex-col items-center justify-center h-[400px] text-center">
                      <div className="h-16 w-16 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center mb-4">
                        <Calendar className="h-8 w-8 text-gray-400" />
                      </div>
                      <p className="text-gray-500 dark:text-gray-400 font-medium mb-2">
                        No Time Slots Generated
                      </p>
                      <p className="text-sm text-gray-400 dark:text-gray-500 max-w-sm">
                        Please configure the schedule settings above to generate time slots.
                      </p>
                    </div>
                  ) : (
                    <div className="space-y-2">
                      {timeSlots.map((slot) => (
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
                          onEdit={enableCustomTiming ? (slotId) => setEditingSlot(slotId) : undefined}
                          onUpdateTiming={enableCustomTiming ? updateSlotTiming : undefined}
                          isEditing={editingSlot === slot.id}
                          enableDragDrop={enableDragDrop}
                        />
                      ))}
                    </div>
                  )}
                </div>
              </ScrollArea>
            </div>
          </div>
          
          <Separator className="my-6" />
          
          {/* Summary */}
          {showAutoDistribute && (
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
                  onClick={autoDistributeCustomers}
                  disabled={unassignedCustomers.length === 0}
                >
                  Auto-Distribute Remaining
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Customer Selection Modal */}
      <Dialog open={showCustomerModal} onOpenChange={setShowCustomerModal}>
        <DialogContent className="max-w-3xl max-h-[85vh] p-0 overflow-hidden">
          <DialogHeader className="px-6 py-4 bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 border-b">
            <DialogTitle className="text-xl font-bold flex items-center gap-2">
              <Users className="h-5 w-5 text-blue-600" />
              Select Customer to Assign
            </DialogTitle>
          </DialogHeader>
          <div className="p-6 space-y-4">
            <ScrollArea className="h-[450px] border-2 border-gray-200 dark:border-gray-700 rounded-xl bg-gray-50 dark:bg-gray-900 p-3">
              {unassignedCustomers.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full text-center">
                  <div className="h-16 w-16 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center mb-4">
                    <Users className="h-8 w-8 text-gray-400" />
                  </div>
                  <p className="text-gray-500 dark:text-gray-400 font-medium">
                    No unassigned customers available
                  </p>
                </div>
              ) : (
                <div className="grid gap-2">
                  {unassignedCustomers.map((customer) => (
                    <motion.div
                      key={customer.UID}
                      className="p-4 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-lg hover:bg-blue-50 dark:hover:bg-blue-950/20 cursor-pointer transition-all group"
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
                            <div className="font-semibold text-gray-900 dark:text-white">
                              {customer.Name}
                            </div>
                            <div className="text-sm text-gray-500 dark:text-gray-400 flex items-center gap-2 mt-1">
                              <MapPin className="h-3 w-3" />
                              <span>{customer.Code}</span>
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
  isEditing?: boolean;
  onEdit?: (slotId: string) => void;
  onUpdateTiming?: (slotId: string, startTime: string, endTime: string, duration: number) => void;
  enableDragDrop?: boolean;
}> = ({ 
  slot, 
  onDrop, 
  onRemove, 
  draggedCustomer, 
  onAddCustomer, 
  isEditing, 
  onEdit, 
  onUpdateTiming,
  enableDragDrop = true
}) => {
  const [isDragOver, setIsDragOver] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);
  const [tempStartTime, setTempStartTime] = useState(slot.startTime);
  const [tempEndTime, setTempEndTime] = useState(slot.endTime);
  const [tempDuration, setTempDuration] = useState(slot.visitDuration || 30);

  const handleSaveTiming = () => {
    if (onUpdateTiming) {
      onUpdateTiming(slot.id, tempStartTime, tempEndTime, tempDuration);
    }
    setShowEditForm(false);
  };

  return (
    <motion.div
      onDragOver={enableDragDrop ? (e) => {
        e.preventDefault();
        setIsDragOver(true);
      } : undefined}
      onDragLeave={enableDragDrop ? () => setIsDragOver(false) : undefined}
      onDrop={enableDragDrop ? (e) => {
        e.preventDefault();
        setIsDragOver(false);
        if (draggedCustomer) {
          onDrop(draggedCustomer, slot.id);
        }
      } : undefined}
      className={cn(
        "border rounded-lg p-4 transition-all",
        enableDragDrop && isDragOver && "bg-accent border-accent-foreground",
        (slot.maxCustomers !== 999 && slot.customers.length === slot.maxCustomers) && "bg-muted/50",
        !isDragOver && (slot.maxCustomers === 999 || slot.customers.length < slot.maxCustomers) && "hover:shadow-sm"
      )}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
    >
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-start gap-3">
          <div className="h-10 w-10 rounded-lg bg-gradient-to-br from-blue-100 to-indigo-100 dark:from-blue-900 dark:to-indigo-900 flex items-center justify-center flex-shrink-0">
            <Calendar className="h-5 w-5 text-blue-600 dark:text-blue-400" />
          </div>
          <div className="flex-1">
            <div className="flex items-center gap-2">
              <span className="font-semibold text-gray-900 dark:text-white">
                {slot.dayName}
              </span>
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
                {onEdit && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-5 px-2 ml-2"
                    onClick={() => setShowEditForm(true)}
                  >
                    <Pencil className="h-3 w-3 mr-1" />
                    Edit Time
                  </Button>
                )}
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