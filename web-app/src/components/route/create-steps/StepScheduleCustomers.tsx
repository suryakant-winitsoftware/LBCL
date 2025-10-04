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

interface WeeklySchedule {
  weekNumber: number;
  days: {
    [day: string]: {
      enabled: boolean;
      customers: Customer[];
      startTime: string;
      endTime: string;
      visitDuration: number;
    };
  };
}

interface StepScheduleCustomersProps {
  form: UseFormReturn<any>;
  customers: Customer[];
  dropdownsLoading: any;
  scheduleType: 'Daily' | 'Weekly' | 'MultiplePerWeeks' | 'WeeklyCycle';
}

export const StepScheduleCustomers: React.FC<StepScheduleCustomersProps> = ({
  form,
  customers,
  dropdownsLoading,
  scheduleType,
}) => {
  const { toast } = useToast();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCustomers, setSelectedCustomers] = useState<Set<string>>(new Set());
  const [draggedCustomer, setDraggedCustomer] = useState<Customer | null>(null);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [modalTargetSlot, setModalTargetSlot] = useState<{ weekNumber: number; day: string } | null>(null);
  
  // WeeklyCycle specific state
  const [cycleLength, setCycleLength] = useState(2);
  const [weeklySchedules, setWeeklySchedules] = useState<WeeklySchedule[]>([]);
  
  // Daily/Weekly schedule state
  const [selectedDays, setSelectedDays] = useState({
    monday: true,
    tuesday: true, 
    wednesday: true,
    thursday: true,
    friday: true,
    saturday: false,
    sunday: false
  });
  
  // Default settings
  const [defaultStartTime, setDefaultStartTime] = useState('09:00');
  const [defaultEndTime, setDefaultEndTime] = useState('17:00');
  const [defaultVisitDuration, setDefaultVisitDuration] = useState(30);
  
  // Initialize weekly schedules for WeeklyCycle
  useEffect(() => {
    if (scheduleType === 'WeeklyCycle') {
      const schedules = Array.from({ length: cycleLength }, (_, index) => ({
        weekNumber: index + 1,
        days: {
          monday: { enabled: false, customers: [], startTime: defaultStartTime, endTime: defaultEndTime, visitDuration: defaultVisitDuration },
          tuesday: { enabled: false, customers: [], startTime: defaultStartTime, endTime: defaultEndTime, visitDuration: defaultVisitDuration },
          wednesday: { enabled: false, customers: [], startTime: defaultStartTime, endTime: defaultEndTime, visitDuration: defaultVisitDuration },
          thursday: { enabled: false, customers: [], startTime: defaultStartTime, endTime: defaultEndTime, visitDuration: defaultVisitDuration },
          friday: { enabled: false, customers: [], startTime: defaultStartTime, endTime: defaultEndTime, visitDuration: defaultVisitDuration },
          saturday: { enabled: false, customers: [], startTime: defaultStartTime, endTime: defaultEndTime, visitDuration: defaultVisitDuration },
          sunday: { enabled: false, customers: [], startTime: defaultStartTime, endTime: defaultEndTime, visitDuration: defaultVisitDuration },
        }
      }));
      setWeeklySchedules(schedules);
    }
  }, [scheduleType, cycleLength, defaultStartTime, defaultEndTime, defaultVisitDuration]);
  
  // Update form data whenever schedules change
  useEffect(() => {
    updateFormData();
  }, [weeklySchedules, selectedDays, selectedCustomers]);
  
  const updateFormData = () => {
    if (scheduleType === 'WeeklyCycle') {
      // Update WeeklyCycle form data
      const weeklyCycleData = {
        cycleLength,
        weeks: {}
      };
      
      weeklySchedules.forEach(schedule => {
        weeklyCycleData.weeks[schedule.weekNumber.toString()] = {
          ...Object.fromEntries(
            Object.entries(schedule.days).map(([day, dayData]) => [day, dayData.enabled])
          ),
          dayCustomers: Object.fromEntries(
            Object.entries(schedule.days).map(([day, dayData]) => [day, dayData.customers.map(c => c.UID)])
          )
        };
      });
      
      form.setValue('weeklyCycle', weeklyCycleData);
    } else {
      // Update Daily/Weekly form data
      form.setValue('selectedCustomers', Array.from(selectedCustomers));
      form.setValue('dailyWeeklyDays', selectedDays);
    }
  };
  
  const handleDayToggle = (weekNumber: number, day: string, enabled: boolean) => {
    if (scheduleType === 'WeeklyCycle') {
      setWeeklySchedules(prev => prev.map(schedule => 
        schedule.weekNumber === weekNumber ? {
          ...schedule,
          days: {
            ...schedule.days,
            [day]: { ...schedule.days[day], enabled }
          }
        } : schedule
      ));
    } else {
      setSelectedDays(prev => ({ ...prev, [day]: enabled }));
    }
  };
  
  const handleCustomerDrop = (customer: Customer, weekNumber: number, day: string) => {
    if (scheduleType === 'WeeklyCycle') {
      setWeeklySchedules(prev => prev.map(schedule => 
        schedule.weekNumber === weekNumber ? {
          ...schedule,
          days: {
            ...schedule.days,
            [day]: {
              ...schedule.days[day],
              customers: [...schedule.days[day].customers, customer]
            }
          }
        } : schedule
      ));
    }
    setSelectedCustomers(prev => new Set(prev).add(customer.UID));
  };
  
  const removeCustomerFromSlot = (customerId: string, weekNumber: number, day: string) => {
    if (scheduleType === 'WeeklyCycle') {
      setWeeklySchedules(prev => prev.map(schedule => 
        schedule.weekNumber === weekNumber ? {
          ...schedule,
          days: {
            ...schedule.days,
            [day]: {
              ...schedule.days[day],
              customers: schedule.days[day].customers.filter(c => c.UID !== customerId)
            }
          }
        } : schedule
      ));
    }
    
    // Check if customer is assigned elsewhere, if not remove from selectedCustomers
    const isAssignedElsewhere = weeklySchedules.some(schedule =>
      Object.entries(schedule.days).some(([dayKey, dayData]) =>
        !(schedule.weekNumber === weekNumber && dayKey === day) &&
        dayData.customers.some(c => c.UID === customerId)
      )
    );
    
    if (!isAssignedElsewhere) {
      setSelectedCustomers(prev => {
        const newSet = new Set(prev);
        newSet.delete(customerId);
        return newSet;
      });
    }
  };
  
  const filteredCustomers = customers.filter(customer =>
    customer.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    customer.Code.toLowerCase().includes(searchTerm.toLowerCase())
  );
  
  const unassignedCustomers = filteredCustomers.filter(
    customer => !selectedCustomers.has(customer.UID)
  );
  
  const dayLabels = {
    monday: 'Monday',
    tuesday: 'Tuesday',
    wednesday: 'Wednesday', 
    thursday: 'Thursday',
    friday: 'Friday',
    saturday: 'Saturday',
    sunday: 'Sunday'
  };
  
  const dayShortLabels = {
    monday: 'Mon',
    tuesday: 'Tue',
    wednesday: 'Wed',
    thursday: 'Thu',
    friday: 'Fri',
    saturday: 'Sat',
    sunday: 'Sun'
  };

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
          {/* Default Time Configuration */}
          <div className="p-4 bg-gradient-to-r from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 rounded-xl border border-gray-200 dark:border-gray-700">
            <div className="flex items-center justify-between mb-4">
              <Label className="text-sm font-semibold">Default Time Settings</Label>
              <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200">
                <Clock className="h-3 w-3 mr-1" />
                Applied to all slots
              </Badge>
            </div>
            
            <div className="grid grid-cols-3 gap-3">
              <div className="space-y-2">
                <Label className="text-xs flex items-center gap-1">
                  <Clock className="h-3 w-3" />
                  Start Time
                </Label>
                <Input
                  type="time"
                  value={defaultStartTime}
                  onChange={(e) => setDefaultStartTime(e.target.value)}
                  className="h-9 text-sm"
                />
              </div>
              <div className="space-y-2">
                <Label className="text-xs">End Time</Label>
                <Input
                  type="time"
                  value={defaultEndTime}
                  onChange={(e) => setDefaultEndTime(e.target.value)}
                  className="h-9 text-sm"
                />
              </div>
              <div className="space-y-2">
                <Label className="text-xs">Visit Duration (min)</Label>
                <Input
                  type="number"
                  value={defaultVisitDuration}
                  onChange={(e) => setDefaultVisitDuration(Number(e.target.value))}
                  min={15}
                  max={120}
                  step={15}
                  className="h-9 text-sm"
                  placeholder="30"
                />
              </div>
            </div>
          </div>
          
          {/* Schedule Configuration Based on Type */}
          {scheduleType === 'WeeklyCycle' ? (
            <div className="space-y-4">
              {/* Cycle Length Selection */}
              <div className="space-y-3">
                <Label className="text-sm font-semibold">Cycle Configuration</Label>
                <div className="flex items-center gap-3">
                  <Label className="text-xs">Number of weeks in cycle:</Label>
                  <select
                    value={cycleLength}
                    onChange={(e) => setCycleLength(parseInt(e.target.value))}
                    className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="2">2 weeks</option>
                    <option value="3">3 weeks</option>
                    <option value="4">4 weeks</option>
                    <option value="5">5 weeks</option>
                  </select>
                </div>
              </div>
              
              {/* Week-by-week Configuration */}
              <div className="space-y-6">
                {weeklySchedules.map((schedule) => (
                  <div key={schedule.weekNumber} className="border border-gray-200 rounded-lg p-4 bg-gray-50">
                    <h4 className="text-sm font-semibold text-gray-800 mb-4">
                      Week {schedule.weekNumber}
                    </h4>
                    
                    {/* Day Selection Grid */}
                    <div className="grid grid-cols-7 gap-2 mb-4">
                      {Object.entries(dayShortLabels).map(([day, label]) => (
                        <label
                          key={day}
                          className={cn(
                            "flex flex-col items-center justify-center p-2 rounded-md border cursor-pointer transition-all text-center text-xs",
                            schedule.days[day].enabled
                              ? "border-green-500 bg-green-50 text-green-800"
                              : "border-gray-200 hover:border-gray-300 bg-white text-gray-600"
                          )}
                        >
                          <input
                            type="checkbox"
                            checked={schedule.days[day].enabled}
                            onChange={(e) => handleDayToggle(schedule.weekNumber, day, e.target.checked)}
                            className="sr-only"
                          />
                          <span className="font-medium">{label}</span>
                          <div
                            className={cn(
                              "w-6 h-6 rounded-full border flex items-center justify-center mt-1",
                              schedule.days[day].enabled
                                ? "border-green-500 bg-green-500"
                                : "border-gray-300"
                            )}
                          >
                            {schedule.days[day].enabled && (
                              <CheckCircle className="h-3 w-3 text-white" />
                            )}
                          </div>
                        </label>
                      ))}
                    </div>
                    
                    {/* Customer Assignment for Enabled Days */}
                    {Object.entries(schedule.days).some(([_, dayData]) => dayData.enabled) && (
                      <div className="mt-4 pt-4 border-t border-gray-200">
                        <h5 className="text-xs font-medium text-gray-700 mb-3">
                          Customer Assignment
                        </h5>
                        <div className="grid gap-3">
                          {Object.entries(schedule.days)
                            .filter(([_, dayData]) => dayData.enabled)
                            .map(([day, dayData]) => (
                              <DayCustomerSlot
                                key={`week-${schedule.weekNumber}-${day}`}
                                weekNumber={schedule.weekNumber}
                                day={day}
                                dayLabel={dayLabels[day]}
                                dayData={dayData}
                                unassignedCustomers={unassignedCustomers}
                                onCustomerDrop={handleCustomerDrop}
                                onCustomerRemove={removeCustomerFromSlot}
                                onAddCustomer={(weekNum, dayKey) => {
                                  setModalTargetSlot({ weekNumber: weekNum, day: dayKey });
                                  setShowCustomerModal(true);
                                }}
                                draggedCustomer={draggedCustomer}
                              />
                            ))}
                        </div>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>
          ) : (
            /* Daily/Weekly Schedule Configuration */
            <div className="space-y-4">
              <div className="space-y-3">
                <Label className="text-sm font-semibold">Working Days Selection</Label>
                <div className="grid grid-cols-7 gap-2">
                  {Object.entries(dayShortLabels).map(([day, label]) => {
                    const isWeekend = day === 'saturday' || day === 'sunday';
                    return (
                      <Button
                        key={day}
                        type="button"
                        variant={selectedDays[day] ? "default" : "outline"}
                        size="sm"
                        onClick={() => setSelectedDays(prev => ({ ...prev, [day]: !prev[day] }))}
                        className={cn(
                          "capitalize",
                          isWeekend && !selectedDays[day] && "border-orange-200 text-orange-600",
                          isWeekend && selectedDays[day] && "bg-orange-600 hover:bg-orange-700"
                        )}
                      >
                        {label}
                      </Button>
                    );
                  })}
                </div>
              </div>
              
              {/* Customer Selection for Daily/Weekly */}
              <div className="space-y-3">
                <Label className="text-sm font-semibold">Customer Selection</Label>
                <div className="p-4 border border-gray-200 rounded-lg">
                  <div className="mb-3">
                    <div className="relative">
                      <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                      <Input
                        placeholder="Search customers..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="pl-9"
                      />
                    </div>
                  </div>
                  
                  <ScrollArea className="h-48 border rounded-md p-3">
                    <div className="space-y-2">
                      {filteredCustomers.map((customer) => {
                        const isSelected = selectedCustomers.has(customer.UID);
                        return (
                          <label
                            key={customer.UID}
                            className="flex items-center space-x-2 text-sm hover:bg-gray-50 rounded p-1 cursor-pointer"
                          >
                            <input
                              type="checkbox"
                              checked={isSelected}
                              onChange={(e) => {
                                if (e.target.checked) {
                                  setSelectedCustomers(prev => new Set(prev).add(customer.UID));
                                } else {
                                  setSelectedCustomers(prev => {
                                    const newSet = new Set(prev);
                                    newSet.delete(customer.UID);
                                    return newSet;
                                  });
                                }
                              }}
                              className="rounded border-gray-300"
                            />
                            <span className="text-gray-700 truncate">{customer.Name}</span>
                            <span className="text-xs text-gray-500">({customer.Code})</span>
                          </label>
                        );
                      })}
                    </div>
                  </ScrollArea>
                </div>
              </div>
            </div>
          )}
          
          <Separator className="my-6" />
          
          {/* Main Content Area for WeeklyCycle with Customer List */}
          {scheduleType === 'WeeklyCycle' && (
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              {/* Available Customers List */}
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
                </div>
              </div>
              
              {/* Schedule Overview */}
              <div className="lg:col-span-2">
                <div className="mb-4 flex items-center justify-between">
                  <Label className="text-lg font-semibold flex items-center gap-2">
                    <CalendarDays className="h-4 w-4 text-gray-500" />
                    Weekly Cycle Overview
                  </Label>
                  <div className="flex items-center gap-2">
                    <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                      <CheckCircle className="h-3 w-3 mr-1" />
                      {selectedCustomers.size} assigned
                    </Badge>
                  </div>
                </div>
                
                <div className="p-4 bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 rounded-xl border border-green-200 dark:border-green-800">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className="h-10 w-10 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                        <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400" />
                      </div>
                      <div>
                        <p className="font-semibold text-gray-900 dark:text-white">Schedule Summary</p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                          {cycleLength} week cycle • {selectedCustomers.size} of {customers.length} customers assigned
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
          
          {/* Summary for other schedule types */}
          {scheduleType !== 'WeeklyCycle' && (
            <div className="p-5 bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 rounded-xl border border-green-200 dark:border-green-800">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="h-10 w-10 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                    <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400" />
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900 dark:text-white">Schedule Summary</p>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      {Object.values(selectedDays).filter(Boolean).length} working days • {selectedCustomers.size} of {customers.length} customers selected
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}
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
                      className="p-4 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-lg hover:bg-blue-50 dark:hover:bg-blue-950/20 hover:border-blue-300 dark:hover:border-blue-700 cursor-pointer transition-all group"
                      onClick={() => {
                        if (modalTargetSlot) {
                          handleCustomerDrop(customer, modalTargetSlot.weekNumber, modalTargetSlot.day);
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

// Day Customer Slot Component for WeeklyCycle
const DayCustomerSlot: React.FC<{
  weekNumber: number;
  day: string;
  dayLabel: string;
  dayData: any;
  unassignedCustomers: Customer[];
  onCustomerDrop: (customer: Customer, weekNumber: number, day: string) => void;
  onCustomerRemove: (customerId: string, weekNumber: number, day: string) => void;
  onAddCustomer: (weekNumber: number, day: string) => void;
  draggedCustomer: Customer | null;
}> = ({ 
  weekNumber, 
  day, 
  dayLabel, 
  dayData, 
  unassignedCustomers, 
  onCustomerDrop, 
  onCustomerRemove, 
  onAddCustomer, 
  draggedCustomer 
}) => {
  const [isDragOver, setIsDragOver] = useState(false);

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
          onCustomerDrop(draggedCustomer, weekNumber, day);
        }
      }}
      className={cn(
        "bg-white rounded-md border p-3 transition-all",
        isDragOver && "border-blue-300 bg-blue-50"
      )}
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.2 }}
    >
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs font-medium text-gray-800">{dayLabel}</span>
        <Badge variant="outline" className="text-xs">
          {dayData.customers.length} assigned
        </Badge>
      </div>
      
      {dayData.customers.length > 0 && (
        <div className="space-y-1 mb-3">
          {dayData.customers.map((customer: Customer, index: number) => (
            <div
              key={customer.UID}
              className="flex items-center justify-between p-2 bg-gray-50 rounded text-xs group"
            >
              <span className="font-medium text-gray-700">{customer.Name}</span>
              <Button
                variant="ghost"
                size="sm"
                className="h-4 w-4 p-0 opacity-0 group-hover:opacity-100"
                onClick={() => onCustomerRemove(customer.UID, weekNumber, day)}
              >
                <X className="h-3 w-3 text-red-500" />
              </Button>
            </div>
          ))}
        </div>
      )}
      
      <Button
        variant="outline"
        size="sm"
        className="w-full h-8 text-xs border-dashed hover:border-solid hover:bg-blue-50 hover:border-blue-300"
        onClick={() => onAddCustomer(weekNumber, day)}
      >
        <Plus className="h-3 w-3 mr-1" />
        Add Customer
      </Button>
    </motion.div>
  );
};