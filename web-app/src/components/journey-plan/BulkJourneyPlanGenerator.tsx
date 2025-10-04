"use client";

import React, { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import moment from "moment";

// UI Components
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useToast } from "@/components/ui/use-toast";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

// Icons
import {
  Calendar,
  CalendarDays,
  CheckCircle2,
  Loader2,
  AlertTriangle,
} from "lucide-react";

// Services
import { api } from "@/services/api";
import { holidayService } from "@/services/holidayService";
import { authService } from "@/lib/auth-service";

// Types
interface BulkJourneyPlanGeneratorProps {
  open: boolean;
  onClose: () => void;
  routeUID: string;
  orgUID: string;
  jobPositionUID: string;
  loginId: string;
  routeSchedule: any | null;
  onSuccess: (count: number) => void;
}

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  SeqNo?: number;
}

interface GeneratedPlan {
  date: Date;
  isHoliday: boolean;
  holidayName?: string;
  isWorkingDay: boolean;
  dayType: string;
  planExists: boolean;
  assignedCustomers: Customer[];
  startTime: string;
  endTime: string;
}

// Form Schema
const bulkGenerationSchema = z.object({
  generationType: z.enum(["daily", "weekly", "monthly"]),
  startDate: z.date(),
  endDate: z.date(),
  includeHolidays: z.boolean(),
  weeklyPattern: z.object({
    monday: z.boolean(),
    tuesday: z.boolean(),
    wednesday: z.boolean(),
    thursday: z.boolean(),
    friday: z.boolean(),
    saturday: z.boolean(),
    sunday: z.boolean(),
  }).optional(),
  monthlyDay: z.number().min(1).max(31).optional(),
  skipExisting: z.boolean(),
  // Customer assignment is now handled per-date
  visitTiming: z.object({
    startTime: z.string(),
    endTime: z.string(),
    defaultDuration: z.number().min(5).max(480),
    defaultTravelTime: z.number().min(0).max(240),
  }).optional(),
}).refine((data) => data.endDate >= data.startDate, {
  message: "End date must be after start date",
  path: ["endDate"],
}).refine((data) => {
  if (data.generationType === "weekly") {
    const pattern = data.weeklyPattern;
    if (!pattern) return false;
    return Object.values(pattern).some(Boolean);
  }
  return true;
}, {
  message: "At least one day must be selected for weekly pattern",
  path: ["weeklyPattern"],
});

type BulkGenerationFormData = z.infer<typeof bulkGenerationSchema>;

export function BulkJourneyPlanGenerator({
  open,
  onClose,
  routeUID,
  orgUID,
  jobPositionUID,
  loginId,
  routeSchedule,
  onSuccess,
}: BulkJourneyPlanGeneratorProps) {
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [generatedPlans, setGeneratedPlans] = useState<GeneratedPlan[]>([]);
  const [previewLoading, setPreviewLoading] = useState(false);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [customersLoading, setCustomersLoading] = useState(false);
  const [showCustomerAssignment, setShowCustomerAssignment] = useState(false);

  const form = useForm<BulkGenerationFormData>({
    resolver: zodResolver(bulkGenerationSchema),
    defaultValues: {
      generationType: "weekly",
      startDate: new Date(),
      endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000), // 30 days
      includeHolidays: false,
      skipExisting: true,
      weeklyPattern: {
        monday: true,
        tuesday: true,
        wednesday: true,
        thursday: true,
        friday: true,
        saturday: false,
        sunday: false,
      },
      monthlyDay: 1,
      visitTiming: {
        startTime: "00:00",
        endTime: "00:00",
        defaultDuration: 30,
        defaultTravelTime: 30,
      },
    },
  });

  const watchedValues = form.watch();

  // Load customers when component mounts
  useEffect(() => {
    if (routeUID) {
      loadRouteCustomers();
    }
  }, [routeUID]);

  // Generate preview when form values change
  useEffect(() => {
    if (watchedValues.startDate && watchedValues.endDate) {
      generatePreview();
    }
  }, [watchedValues.startDate, watchedValues.endDate, watchedValues.generationType, watchedValues.weeklyPattern, watchedValues.monthlyDay, watchedValues.includeHolidays]);

  const loadRouteCustomers = async () => {
    setCustomersLoading(true);
    try {
      // Use the correct dropdown API for customers by route
      const response = await api.dropdown.getCustomersByRoute(routeUID);
      console.log("Route customers response for bulk generator:", response);
      
      if (response?.IsSuccess && response?.Data) {
        // Customers are returned as ISelectionItem from dropdown API
        const customers = Array.isArray(response.Data) 
          ? response.Data.map((cust: any, index: number) => ({
              UID: cust.UID,
              Code: cust.Code || "",
              Name: cust.Label || cust.Code,
              Address: cust.ExtData?.Address || "",
              ContactNo: cust.ExtData?.Mobile || cust.ExtData?.Phone || "",
              SeqNo: index + 1,
            }))
          : [];
        setCustomers(customers);
        console.log("Route customers loaded for bulk generator:", customers);
      } else {
        console.warn("No customer data received:", response);
        setCustomers([]);
      }
    } catch (error) {
      console.error("Error loading customers for bulk generator:", error);
      setCustomers([]);
    } finally {
      setCustomersLoading(false);
    }
  };

  const generatePreview = async () => {
    setPreviewLoading(true);
    try {
      const formData = form.getValues();
      const dates = await generateDatesForPlan(formData);
      
      // Check for existing plans and holidays
      const plansWithDetails = await Promise.all(
        dates.map(async (date) => {
          const [holidayInfo, existingPlan] = await Promise.all([
            checkHolidayStatus(date),
            checkExistingPlan(date),
          ]);

          return {
            date,
            isHoliday: holidayInfo.isHoliday,
            holidayName: holidayInfo.name,
            isWorkingDay: !holidayInfo.isHoliday,
            dayType: holidayInfo.isHoliday ? "Holiday" : "WorkDay",
            planExists: existingPlan,
            assignedCustomers: [],
            startTime: routeSchedule?.StartTime ? formatTimeFromTimeSpan(routeSchedule.StartTime) : "00:00",
            endTime: routeSchedule?.EndTime ? formatTimeFromTimeSpan(routeSchedule.EndTime) : "00:00",
          };
        })
      );

      setGeneratedPlans(plansWithDetails);
      setShowCustomerAssignment(plansWithDetails.length > 0);
    } catch (error) {
      console.error("Error generating preview:", error);
      toast({
        title: "Error",
        description: "Failed to generate preview",
        variant: "destructive",
      });
    } finally {
      setPreviewLoading(false);
    }
  };

  const generateDatesForPlan = async (formData: BulkGenerationFormData): Promise<Date[]> => {
    const dates: Date[] = [];
    const current = new Date(formData.startDate);
    const endDate = new Date(formData.endDate);

    while (current <= endDate) {
      let shouldInclude = false;

      switch (formData.generationType) {
        case "daily":
          shouldInclude = true;
          break;

        case "weekly":
          if (formData.weeklyPattern) {
            const dayOfWeek = current.getDay();
            const dayMap = [
              formData.weeklyPattern.sunday,
              formData.weeklyPattern.monday,
              formData.weeklyPattern.tuesday,
              formData.weeklyPattern.wednesday,
              formData.weeklyPattern.thursday,
              formData.weeklyPattern.friday,
              formData.weeklyPattern.saturday,
            ];
            shouldInclude = dayMap[dayOfWeek];
          }
          break;

        case "monthly":
          if (formData.monthlyDay) {
            shouldInclude = current.getDate() === formData.monthlyDay;
          }
          break;
      }

      if (shouldInclude) {
        dates.push(new Date(current));
      }

      current.setDate(current.getDate() + 1);
    }

    return dates;
  };

  const checkHolidayStatus = async (date: Date): Promise<{ isHoliday: boolean; name?: string }> => {
    try {
      const holidays = await holidayService.getHolidaysForYear(orgUID, moment(date).year());
      const dateStr = moment(date).format('YYYY-MM-DD');
      const holiday = holidays.find((h: any) => moment(h.Date).format('YYYY-MM-DD') === dateStr);
      
      return {
        isHoliday: !!holiday,
        name: holiday?.Name,
      };
    } catch (error) {
      console.error("Error checking holiday status:", error);
      return { isHoliday: false };
    }
  };

  const checkExistingPlan = async (date: Date): Promise<boolean> => {
    try {
      const response = await api.beatHistory.selectAll({
        pageNumber: 1,
        pageSize: 1,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: [
          {
            filterBy: 'VisitDate',
            filterValue: moment(date).format('YYYY-MM-DD'),
            filterOperator: 'equals'
          },
          {
            filterBy: 'RouteUID',
            filterValue: routeUID,
            filterOperator: 'equals'
          },
          {
            filterBy: 'OrgUID',
            filterValue: orgUID,
            filterOperator: 'equals'
          }
        ]
      });

      return response?.IsSuccess && response?.Data?.PagedData?.length > 0;
    } catch (error) {
      console.error("Error checking existing plan:", error);
      return false;
    }
  };

  const handleGenerate = async (formData: BulkGenerationFormData) => {
    setLoading(true);
    try {
      const currentUser = authService.getCurrentUser();
      let createdCount = 0;

      // Filter plans to create
      const plansToCreate = generatedPlans.filter(plan => {
        // Skip existing plans if skipExisting is true
        if (formData.skipExisting && plan.planExists) return false;
        
        // Skip holidays if includeHolidays is false
        if (!formData.includeHolidays && plan.isHoliday) return false;
        
        return true;
      });

      console.log(`Creating ${plansToCreate.length} journey plans`);

      // Generate journey plans in batches
      const batchSize = 10;
      for (let i = 0; i < plansToCreate.length; i += batchSize) {
        const batch = plansToCreate.slice(i, i + batchSize);
        
        await Promise.all(
          batch.map(async (plan) => {
            try {
              const yearMonth = parseInt(moment(plan.date).format("YYMM"));
              
              // Create Beat History
              const beatHistoryData = {
                UID: crypto.randomUUID(),
                OrgUID: orgUID,
                RouteUID: routeUID,
                RouteScheduleUID: routeSchedule?.UID || null,
                LoginId: loginId,
                JobPositionUID: jobPositionUID,
                VisitDate: moment(plan.date).format('YYYY-MM-DD'),
                PlannedStartTime: routeSchedule?.StartTime ? 
                  formatTimeFromTimeSpan(routeSchedule.StartTime) : "00:00",
                PlannedEndTime: routeSchedule?.EndTime ? 
                  formatTimeFromTimeSpan(routeSchedule.EndTime) : "00:00",
                YearMonth: yearMonth,
                IsPlanned: true,
                Status: plan.isHoliday ? "Holiday" : "Planned",
                IsHoliday: plan.isHoliday,
                HolidayName: plan.holidayName || "",
                IsWorkingDay: plan.isWorkingDay,
                DayType: plan.dayType,
                ScheduleType: routeSchedule?.Type || "Manual",
                Notes: `Generated via bulk creation - ${formData.generationType}`,
                CreatedBy: currentUser?.loginId || "SYSTEM",
                ModifiedBy: currentUser?.loginId || "SYSTEM",
              };

              const response = await api.journeyPlan.createBeatHistory(beatHistoryData);
              if (response?.IsSuccess) {
                createdCount++;
                
                // Create StoreHistory records for customer visits if customers are assigned
                const customersToAssign = plan.assignedCustomers;
                if (customersToAssign.length > 0) {
                  await createStoreHistoriesForPlan(
                    beatHistoryData.UID,
                    customersToAssign,
                    plan.date,
                    formData
                  );
                }
              } else {
                console.error("Failed to create plan for", plan.date, response);
              }
            } catch (error) {
              console.error("Error creating plan for", plan.date, error);
            }
          })
        );

        // Small delay between batches to avoid overwhelming the server
        if (i + batchSize < plansToCreate.length) {
          await new Promise(resolve => setTimeout(resolve, 100));
        }
      }

      toast({
        title: "Success",
        description: `Created ${createdCount} journey plans successfully!`,
      });

      onSuccess(createdCount);
      onClose();
    } catch (error: any) {
      console.error("Error generating journey plans:", error);
      toast({
        title: "Error",
        description: error?.message || "Failed to generate journey plans",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const formatTimeFromTimeSpan = (timeSpan: string | any): string => {
    if (typeof timeSpan === 'string') {
      return timeSpan.substring(0, 5);
    }
    if (timeSpan && typeof timeSpan === 'object') {
      const hours = String(timeSpan.Hours || timeSpan.hours || 8).padStart(2, '0');
      const minutes = String(timeSpan.Minutes || timeSpan.minutes || 0).padStart(2, '0');
      return `${hours}:${minutes}`;
    }
    return "00:00";
  };

  const getDayName = (date: Date): string => {
    return moment(date).format('dddd');
  };

  const getCustomersForPlan = (plan: GeneratedPlan): Customer[] => {
    return plan.assignedCustomers;
  };

  const createStoreHistoriesForPlan = async (
    beatHistoryUID: string,
    customersToAssign: Customer[],
    visitDate: Date,
    formData: BulkGenerationFormData
  ) => {
    try {
      const currentUser = authService.getCurrentUser();
      const yearMonth = parseInt(moment(visitDate).format("YYMM"));
      const timing = formData.visitTiming || {
        startTime: "00:00",
        endTime: "00:00",
        defaultDuration: 30,
        defaultTravelTime: 30,
      };
      
      // Calculate time slots for each customer
      let currentTime = timing.startTime;
      
      const storeHistoryData = customersToAssign.map((customer, index) => {
        const startTime = currentTime;
        
        // Calculate end time
        const startMinutes = parseInt(startTime.split(":")[0]) * 60 + parseInt(startTime.split(":")[1]);
        const endMinutes = startMinutes + timing.defaultDuration;
        const endHours = Math.floor(endMinutes / 60);
        const endMins = endMinutes % 60;
        const endTime = `${endHours.toString().padStart(2, "0")}:${endMins.toString().padStart(2, "0")}`;
        
        // Calculate next start time (add travel time)
        if (index < customersToAssign.length - 1) {
          const nextStartMinutes = endMinutes + timing.defaultTravelTime;
          const nextStartHours = Math.floor(nextStartMinutes / 60);
          const nextStartMins = nextStartMinutes % 60;
          currentTime = `${nextStartHours.toString().padStart(2, "0")}:${nextStartMins.toString().padStart(2, "0")}`;
        }
        
        return {
          UID: crypto.randomUUID(),
          BeatHistoryUID: beatHistoryUID,
          OrgUID: orgUID,
          RouteUID: routeUID,
          StoreUID: customer.UID,
          YearMonth: yearMonth,
          IsPlanned: true,
          SerialNo: index + 1,
          Status: "Planned",
          PlannedLoginTime: startTime,
          PlannedLogoutTime: endTime,
          VisitDate: moment(visitDate).format('YYYY-MM-DD'),
          CreatedBy: currentUser?.loginId || "SYSTEM",
          ModifiedBy: currentUser?.loginId || "SYSTEM",
        };
      });
      
      console.log(`Creating ${storeHistoryData.length} store histories for ${moment(visitDate).format('DD/MM/YYYY')}`);
      
      // Create store histories in batch
      const storeResponse = await api.beatHistory.createBulkStoreHistories(storeHistoryData);
      if (!storeResponse?.IsSuccess) {
        console.error("Failed to create store histories for", visitDate, storeResponse);
      } else {
        console.log("Store histories created successfully for", visitDate);
      }
    } catch (error) {
      console.error("Error creating store histories for", visitDate, error);
    }
  };

  const getValidPlansCount = () => {
    return generatedPlans.filter(plan => {
      const formData = form.getValues();
      if (formData.skipExisting && plan.planExists) return false;
      if (!formData.includeHolidays && plan.isHoliday) return false;
      return true;
    }).length;
  };

  const getSelectedCustomersCount = () => {
    return generatedPlans.reduce((total, plan) => total + plan.assignedCustomers.length, 0);
  };

  const toggleCustomerForDate = (dateIndex: number, customer: Customer) => {
    const updatedPlans = [...generatedPlans];
    const plan = updatedPlans[dateIndex];
    
    const isAssigned = plan.assignedCustomers.some(c => c.UID === customer.UID);
    if (isAssigned) {
      // Remove customer
      plan.assignedCustomers = plan.assignedCustomers.filter(c => c.UID !== customer.UID);
    } else {
      // Add customer
      plan.assignedCustomers = [...plan.assignedCustomers, customer];
    }
    
    setGeneratedPlans(updatedPlans);
  };

  const assignAllCustomersToDate = (dateIndex: number) => {
    const updatedPlans = [...generatedPlans];
    updatedPlans[dateIndex].assignedCustomers = [...customers];
    setGeneratedPlans(updatedPlans);
  };

  const clearAllCustomersFromDate = (dateIndex: number) => {
    const updatedPlans = [...generatedPlans];
    updatedPlans[dateIndex].assignedCustomers = [];
    setGeneratedPlans(updatedPlans);
  };

  const assignAllCustomersToAllDates = () => {
    const updatedPlans = generatedPlans.map(plan => ({
      ...plan,
      assignedCustomers: [...customers]
    }));
    setGeneratedPlans(updatedPlans);
  };

  const clearAllCustomersFromAllDates = () => {
    const updatedPlans = generatedPlans.map(plan => ({
      ...plan,
      assignedCustomers: []
    }));
    setGeneratedPlans(updatedPlans);
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <CalendarDays className="h-5 w-5" />
            Bulk Journey Plan Generation
          </DialogTitle>
          <DialogDescription>
            Create multiple journey plans for the selected route based on your schedule pattern
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(handleGenerate)} className="space-y-6">
            {/* Generation Type */}
            <div className="space-y-4">
              <FormField
                control={form.control}
                name="generationType"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium">📋 Generation Pattern</FormLabel>
                    <FormControl>
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                        {[
                          { 
                            value: 'daily', 
                            label: 'Daily', 
                            icon: '📆', 
                            description: 'Every day in date range',
                            color: 'border-green-200 bg-green-50'
                          },
                          { 
                            value: 'weekly', 
                            label: 'Weekly', 
                            icon: '📅', 
                            description: 'Specific days of week',
                            color: 'border-blue-200 bg-blue-50'
                          },
                          { 
                            value: 'monthly', 
                            label: 'Monthly', 
                            icon: '🗓️', 
                            description: 'Same day each month',
                            color: 'border-purple-200 bg-purple-50'
                          }
                        ].map((option) => (
                          <div
                            key={option.value}
                            className={`
                              relative cursor-pointer rounded-lg border-2 p-4 text-center transition-all
                              ${field.value === option.value
                                ? `${option.color} border-opacity-100 shadow-md`
                                : 'border-gray-200 bg-white hover:border-gray-300 hover:bg-gray-50'
                              }
                            `}
                            onClick={() => field.onChange(option.value)}
                          >
                            <div className="text-2xl mb-2">{option.icon}</div>
                            <div className={`font-medium text-sm ${
                              field.value === option.value ? 'text-gray-800' : 'text-gray-600'
                            }`}>
                              {option.label}
                            </div>
                            <div className={`text-xs mt-1 ${
                              field.value === option.value ? 'text-gray-600' : 'text-gray-500'
                            }`}>
                              {option.description}
                            </div>
                            {field.value === option.value && (
                              <div className="absolute -top-1 -right-1 w-5 h-5 bg-blue-500 rounded-full flex items-center justify-center">
                                <CheckCircle2 className="w-3 h-3 text-white" />
                              </div>
                            )}
                          </div>
                        ))}
                      </div>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Date Range */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

              <FormField
                control={form.control}
                name="startDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="flex items-center gap-2">
                      🗓️ Start Date
                    </FormLabel>
                    <FormControl>
                      <Input
                        type="date"
                        value={field.value ? moment(field.value).format('YYYY-MM-DD') : ''}
                        onChange={(e) => field.onChange(new Date(e.target.value))}
                        className="text-sm"
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="endDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="flex items-center gap-2">
                      🏁 End Date
                    </FormLabel>
                    <FormControl>
                      <Input
                        type="date"
                        value={field.value ? moment(field.value).format('YYYY-MM-DD') : ''}
                        onChange={(e) => field.onChange(new Date(e.target.value))}
                        className="text-sm"
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Weekly Pattern */}
            {watchedValues.generationType === "weekly" && (
              <Card className="border-2">
                <CardHeader className="bg-gradient-to-r from-blue-50 to-indigo-50 pb-3">
                  <CardTitle className="text-base font-semibold flex items-center justify-between">
                    <span className="flex items-center gap-2">
                      <CalendarDays className="h-5 w-5 text-blue-600" />
                      Weekly Schedule Pattern
                    </span>
                    <Badge variant="secondary" className="text-xs">
                      Select active days
                    </Badge>
                  </CardTitle>
                </CardHeader>
                <CardContent className="pt-6">
                  <div className="space-y-4">
                    {/* Professional Week Table */}
                    <div className="overflow-hidden rounded-lg border">
                      <table className="w-full">
                        <thead className="bg-gray-50">
                          <tr>
                            {[
                              { key: 'sunday', label: 'Sunday', isWeekend: true },
                              { key: 'monday', label: 'Monday', isWeekend: false },
                              { key: 'tuesday', label: 'Tuesday', isWeekend: false },
                              { key: 'wednesday', label: 'Wednesday', isWeekend: false },
                              { key: 'thursday', label: 'Thursday', isWeekend: false },
                              { key: 'friday', label: 'Friday', isWeekend: false },
                              { key: 'saturday', label: 'Saturday', isWeekend: true },
                            ].map(({ label, isWeekend }) => (
                              <th 
                                key={label}
                                className={`px-2 py-3 text-xs font-medium text-center ${
                                  isWeekend ? 'text-red-600 bg-red-50' : 'text-gray-700'
                                }`}
                              >
                                {label.slice(0, 3).toUpperCase()}
                              </th>
                            ))}
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            {[
                              { key: 'sunday', label: 'Sun', isWeekend: true },
                              { key: 'monday', label: 'Mon', isWeekend: false },
                              { key: 'tuesday', label: 'Tue', isWeekend: false },
                              { key: 'wednesday', label: 'Wed', isWeekend: false },
                              { key: 'thursday', label: 'Thu', isWeekend: false },
                              { key: 'friday', label: 'Fri', isWeekend: false },
                              { key: 'saturday', label: 'Sat', isWeekend: true },
                            ].map(({ key, label, isWeekend }) => (
                              <td key={key} className="p-2 text-center border-t">
                                <FormField
                                  control={form.control}
                                  name={`weeklyPattern.${key as keyof typeof watchedValues.weeklyPattern}`}
                                  render={({ field }) => (
                                    <FormItem>
                                      <FormControl>
                                        <button
                                          type="button"
                                          onClick={() => field.onChange(!field.value)}
                                          className={`
                                            w-12 h-12 rounded-full font-semibold text-sm transition-all transform
                                            ${field.value 
                                              ? 'bg-blue-500 text-white shadow-lg scale-110 ring-2 ring-blue-200' 
                                              : isWeekend 
                                                ? 'bg-red-100 text-red-600 hover:bg-red-200' 
                                                : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                                            }
                                          `}
                                        >
                                          {field.value ? '✓' : label[0]}
                                        </button>
                                      </FormControl>
                                    </FormItem>
                                  )}
                                />
                              </td>
                            ))}
                          </tr>
                        </tbody>
                      </table>
                    </div>
                    
                    {/* Quick Actions */}
                    <div className="flex items-center justify-center gap-2 pt-2">
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      onClick={() => {
                        const currentPattern = form.getValues("weeklyPattern") || {};
                        form.setValue("weeklyPattern", {
                          ...currentPattern,
                          monday: true,
                          tuesday: true,
                          wednesday: true,
                          thursday: true,
                          friday: true,
                          saturday: false,
                          sunday: false,
                        });
                      }}
                      className="text-xs"
                    >
                      Weekdays Only
                    </Button>
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      onClick={() => {
                        const currentPattern = form.getValues("weeklyPattern") || {};
                        form.setValue("weeklyPattern", {
                          ...currentPattern,
                          monday: true,
                          tuesday: true,
                          wednesday: true,
                          thursday: true,
                          friday: true,
                          saturday: true,
                          sunday: true,
                        });
                      }}
                      className="text-xs"
                    >
                      All Days
                    </Button>
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      onClick={() => {
                        const currentPattern = form.getValues("weeklyPattern") || {};
                        form.setValue("weeklyPattern", {
                          ...currentPattern,
                          monday: false,
                          tuesday: false,
                          wednesday: false,
                          thursday: false,
                          friday: false,
                          saturday: false,
                          sunday: false,
                        });
                      }}
                      className="text-xs"
                    >
                      Clear All
                    </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Monthly Day */}
            {watchedValues.generationType === "monthly" && (
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm flex items-center gap-2">
                    📅 Monthly Pattern
                    <span className="text-xs text-muted-foreground">
                      (Select day of the month)
                    </span>
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <FormField
                    control={form.control}
                    name="monthlyDay"
                    render={({ field }) => (
                      <FormItem>
                        <div className="space-y-4">
                          {/* Number Input with Visual Enhancement */}
                          <div className="flex items-center gap-4">
                            <FormLabel className="text-sm font-medium">Day of Month:</FormLabel>
                            <div className="flex items-center gap-2">
                              <FormControl>
                                <Input
                                  type="number"
                                  min="1"
                                  max="31"
                                  value={field.value || ''}
                                  onChange={(e) => field.onChange(parseInt(e.target.value))}
                                  className="w-20 text-center font-bold text-lg"
                                  placeholder="1"
                                />
                              </FormControl>
                              <span className="text-sm text-muted-foreground">
                                (1-31)
                              </span>
                            </div>
                          </div>
                          
                          {/* Visual Calendar Grid for Quick Selection */}
                          <div className="space-y-2">
                            <Label className="text-sm text-muted-foreground">Quick Select:</Label>
                            <div className="grid grid-cols-7 gap-1">
                              {Array.from({ length: 31 }, (_, i) => i + 1).map((day) => (
                                <button
                                  key={day}
                                  type="button"
                                  onClick={() => field.onChange(day)}
                                  className={`
                                    relative w-8 h-8 rounded text-xs font-medium transition-all
                                    ${field.value === day
                                      ? 'bg-blue-500 text-white shadow-md'
                                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                                    }
                                  `}
                                >
                                  {day}
                                  {field.value === day && (
                                    <div className="absolute -top-1 -right-1 w-3 h-3 bg-green-500 rounded-full flex items-center justify-center">
                                      <CheckCircle2 className="w-2 h-2 text-white" />
                                    </div>
                                  )}
                                </button>
                              ))}
                            </div>
                          </div>

                          {/* Common Presets */}
                          <div className="flex flex-wrap gap-2">
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              onClick={() => field.onChange(1)}
                              className="text-xs"
                            >
                              1st of Month
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              onClick={() => field.onChange(15)}
                              className="text-xs"
                            >
                              Mid-Month (15th)
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              onClick={() => field.onChange(31)}
                              className="text-xs"
                            >
                              End of Month (31st)
                            </Button>
                          </div>

                          {field.value && (
                            <div className="text-sm text-blue-600 bg-blue-50 p-2 rounded">
                              📅 Journey plans will be created on the <strong>{field.value}</strong>
                              {field.value === 1 ? 'st' : field.value === 2 ? 'nd' : field.value === 3 ? 'rd' : 'th'} 
                              {' '}day of each month
                            </div>
                          )}
                        </div>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </CardContent>
              </Card>
            )}

            {/* Options */}
            <div className="flex gap-4">
              <FormField
                control={form.control}
                name="includeHolidays"
                render={({ field }) => (
                  <FormItem className="flex items-center space-x-2">
                    <FormControl>
                      <Checkbox
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                    <FormLabel>Include holidays</FormLabel>
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="skipExisting"
                render={({ field }) => (
                  <FormItem className="flex items-center space-x-2">
                    <FormControl>
                      <Checkbox
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                    <FormLabel>Skip existing plans</FormLabel>
                  </FormItem>
                )}
              />
            </div>

            {/* Customer Assignment Per Date */}
            {showCustomerAssignment && generatedPlans.length > 0 && (
              <Card>
                <CardHeader>
                  <CardTitle className="text-sm flex items-center justify-between">
                    <span className="flex items-center gap-2">
                      👥 Customer Assignment ({customers.length} available)
                      {customersLoading && <Loader2 className="h-4 w-4 animate-spin" />}
                    </span>
                    <div className="flex gap-2">
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        onClick={assignAllCustomersToAllDates}
                        className="text-xs"
                      >
                        Assign All to All Dates
                      </Button>
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        onClick={clearAllCustomersFromAllDates}
                        className="text-xs"
                      >
                        Clear All
                      </Button>
                    </div>
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Visit Timing Configuration */}
                  <div className="space-y-3 p-3 bg-muted/30 rounded-lg">
                    <Label className="text-sm font-medium">Visit Timing Settings</Label>
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <Label className="text-xs">Visit Duration (min)</Label>
                        <Input
                          type="number"
                          min="5"
                          max="480"
                          defaultValue={30}
                          onChange={(e) => {
                            const current = form.getValues("visitTiming") || {
                              startTime: "00:00",
                              endTime: "00:00",
                              defaultDuration: 30,
                              defaultTravelTime: 30,
                            };
                            form.setValue("visitTiming", {
                              ...current,
                              defaultDuration: parseInt(e.target.value),
                            });
                          }}
                        />
                      </div>
                      <div>
                        <Label className="text-xs">Travel Time (min)</Label>
                        <Input
                          type="number"
                          min="0"
                          max="240"
                          defaultValue={30}
                          onChange={(e) => {
                            const current = form.getValues("visitTiming") || {
                              startTime: "00:00",
                              endTime: "00:00",
                              defaultDuration: 30,
                              defaultTravelTime: 30,
                            };
                            form.setValue("visitTiming", {
                              ...current,
                              defaultTravelTime: parseInt(e.target.value),
                            });
                          }}
                        />
                      </div>
                    </div>
                  </div>

                  {/* Date-wise Customer Assignment */}
                  <div className="space-y-3">
                    <Label className="text-sm font-medium">
                      Assign Customers to Specific Dates ({generatedPlans.filter(p => p.assignedCustomers.length > 0).length} dates have customers)
                    </Label>
                    
                    <div className="max-h-96 overflow-y-auto space-y-3">
                      {generatedPlans.map((plan, dateIndex) => {
                        const formData = form.getValues();
                        const willSkip = (formData.skipExisting && plan.planExists) || 
                                        (!formData.includeHolidays && plan.isHoliday);
                        
                        if (willSkip) return null;

                        return (
                          <Card key={dateIndex} className="p-3">
                            <div className="space-y-3">
                              {/* Date Header */}
                              <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                  <span className="font-medium">
                                    {moment(plan.date).format('DD/MM/YYYY')} - {getDayName(plan.date)}
                                  </span>
                                  <div className="flex gap-1">
                                    {plan.isHoliday && (
                                      <Badge variant="outline" className="text-xs">Holiday</Badge>
                                    )}
                                    {plan.planExists && (
                                      <Badge variant="outline" className="text-xs">Exists</Badge>
                                    )}
                                  </div>
                                </div>
                                <div className="flex gap-1">
                                  <Button
                                    type="button"
                                    size="sm"
                                    variant="outline"
                                    onClick={() => assignAllCustomersToDate(dateIndex)}
                                    className="text-xs h-6"
                                  >
                                    All
                                  </Button>
                                  <Button
                                    type="button"
                                    size="sm"
                                    variant="outline"
                                    onClick={() => clearAllCustomersFromDate(dateIndex)}
                                    className="text-xs h-6"
                                  >
                                    Clear
                                  </Button>
                                </div>
                              </div>

                              {plan.holidayName && (
                                <div className="text-xs text-orange-600">
                                  Holiday: {plan.holidayName}
                                </div>
                              )}

                              {/* Customer Selection for this date */}
                              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-2">
                                {customers.map((customer) => {
                                  const isAssigned = plan.assignedCustomers.some(c => c.UID === customer.UID);
                                  return (
                                    <div
                                      key={customer.UID}
                                      className={`p-2 rounded border cursor-pointer transition-colors ${
                                        isAssigned 
                                          ? 'bg-blue-50 border-blue-200 text-blue-800' 
                                          : 'bg-gray-50 border-gray-200 hover:bg-gray-100'
                                      }`}
                                      onClick={() => toggleCustomerForDate(dateIndex, customer)}
                                    >
                                      <div className="flex items-center gap-2">
                                        <Checkbox
                                          checked={isAssigned}
                                          onChange={() => {}} // Controlled by parent click
                                          className="pointer-events-none"
                                        />
                                        <div className="flex-1 min-w-0">
                                          <div className="text-xs font-medium truncate">
                                            {customer.Name}
                                          </div>
                                          {customer.Code && (
                                            <div className="text-xs text-muted-foreground truncate">
                                              {customer.Code}
                                            </div>
                                          )}
                                        </div>
                                      </div>
                                    </div>
                                  );
                                })}
                              </div>

                              {plan.assignedCustomers.length > 0 && (
                                <div className="text-xs text-blue-600 mt-2">
                                  {plan.assignedCustomers.length} customer{plan.assignedCustomers.length > 1 ? 's' : ''} selected
                                </div>
                              )}

                              {customers.length === 0 && (
                                <div className="text-center py-4 text-muted-foreground text-xs">
                                  No customers available for this route
                                </div>
                              )}
                            </div>
                          </Card>
                        );
                      })}
                    </div>
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Schedule Info */}
            <Alert>
              <CalendarDays className="h-4 w-4" />
              <AlertDescription>
                {routeSchedule ? (
                  <>Using schedule: <strong>{routeSchedule.Name}</strong> ({routeSchedule.Type})</>
                ) : (
                  <>Manual journey plan generation - no automatic schedule configured for this route</>
                )}
              </AlertDescription>
            </Alert>

            {/* Preview */}
            <Card>
              <CardHeader>
                <CardTitle className="text-sm flex items-center justify-between">
                  <span className="flex items-center gap-2">
                    <Calendar className="h-4 w-4" />
                    Preview ({getValidPlansCount()} plans, {getSelectedCustomersCount()} customer assignments)
                  </span>
                  {previewLoading && <Loader2 className="h-4 w-4 animate-spin" />}
                </CardTitle>
              </CardHeader>
              <CardContent>
                {generatedPlans.length > 0 ? (
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 max-h-64 overflow-y-auto">
                    {generatedPlans.slice(0, 50).map((plan, index) => {
                      const formData = form.getValues();
                      const willSkip = (formData.skipExisting && plan.planExists) || 
                                      (!formData.includeHolidays && plan.isHoliday);
                      
                      return (
                        <div
                          key={index}
                          className={`p-2 rounded border text-xs ${
                            willSkip ? "bg-gray-50 opacity-50" : "bg-white"
                          } ${
                            plan.isHoliday ? "border-orange-200" : ""
                          } ${
                            plan.planExists ? "border-blue-200" : ""
                          }`}
                        >
                          <div className="flex items-center justify-between">
                            <span className="font-medium">
                              {moment(plan.date).format('DD/MM')} - {getDayName(plan.date)}
                            </span>
                            <div className="flex gap-1">
                              {plan.isHoliday && (
                                <Badge variant="outline" className="text-xs">Holiday</Badge>
                              )}
                              {plan.planExists && (
                                <Badge variant="outline" className="text-xs">Exists</Badge>
                              )}
                              {!willSkip && (
                                <CheckCircle2 className="h-3 w-3 text-green-500" />
                              )}
                            </div>
                          </div>
                          {plan.holidayName && (
                            <div className="text-xs text-orange-600 mt-1">
                              {plan.holidayName}
                            </div>
                          )}
                          {!willSkip && plan.assignedCustomers.length > 0 && (
                            <div className="text-xs text-blue-600 mt-1">
                              {plan.assignedCustomers.length} customer{plan.assignedCustomers.length > 1 ? 's' : ''} assigned
                            </div>
                          )}
                        </div>
                      );
                    })}
                    {generatedPlans.length > 50 && (
                      <div className="p-2 text-center text-xs text-muted-foreground">
                        ... and {generatedPlans.length - 50} more
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="text-center py-4 text-muted-foreground">
                    No plans generated. Please check your date range and pattern settings.
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Actions */}
            <div className="flex items-center justify-end gap-2">
              <Button type="button" variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button 
                type="submit" 
                disabled={loading || getValidPlansCount() === 0}
              >
                {loading ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Generating...
                  </>
                ) : (
                  <>
                    <CheckCircle2 className="mr-2 h-4 w-4" />
                    Generate {getValidPlansCount()} Plans
                  </>
                )}
              </Button>
            </div>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}