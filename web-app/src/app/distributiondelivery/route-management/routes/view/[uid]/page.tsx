"use client";

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { 
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle 
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import { 
  ArrowLeft, 
  Edit, 
  Calendar,
  Clock,
  Users,
  MapPin,
  Settings,
  Building,
  Truck,
  CheckCircle,
  XCircle,
  Eye,
  Timer,
  Download,
  FileSpreadsheet,
} from 'lucide-react';
import { routeService } from '@/services/routeService';
import { cn } from '@/lib/utils';
import moment from 'moment';
import * as XLSX from 'xlsx';

interface RouteDetails {
  Route: any;
  RouteSchedule: any;
  RouteScheduleDaywise: any;
  RouteScheduleFortnight: any;
  RouteScheduleConfigs?: any[];
  RouteScheduleCustomerMappings?: any[];
  RouteCustomersList: any[];
  RouteUserList: any[];
}

const RouteDetailView: React.FC = () => {
  const router = useRouter();
  const params = useParams();
  const uid = params?.uid as string;
  const { toast } = useToast();

  const [loading, setLoading] = useState(true);
  const [routeDetails, setRouteDetails] = useState<RouteDetails | null>(null);
  const [scheduleCustomerMappings, setScheduleCustomerMappings] = useState<any[]>([]);
  const [scheduleConfigs, setScheduleConfigs] = useState<any[]>([]);
  const [selectedWeekDay, setSelectedWeekDay] = useState<{ week: number; day: number } | null>(null);
  const [selectedMonthDate, setSelectedMonthDate] = useState<number | null>(null);
  const [selectedFrequencyType, setSelectedFrequencyType] = useState<string | null>(null);

  useEffect(() => {
    if (uid) {
      loadRouteDetails();
    }
  }, [uid]);

  // Excel Export Function
  const exportToExcel = () => {
    if (!routeDetails) return;

    const workbook = XLSX.utils.book_new();
    const route = routeDetails.Route;
    
    // 1. COMPLETE ROUTE INFORMATION SHEET
    const routeInfoData = [
      ['COMPLETE ROUTE REPORT - SALES TEAM'],
      ['Generated Date:', new Date().toLocaleDateString(), 'Generated Time:', new Date().toLocaleTimeString()],
      [''],
      ['=== BASIC INFORMATION ==='],
      ['Route Name:', route.Name || ''],
      ['Route Code:', route.Code || ''],
      ['Description:', route.Description || ''],
      ['Route Type:', route.RouteType || 'Standard'],
      ['Status:', route.IsActive ? 'ACTIVE' : 'INACTIVE'],
      ['Organization:', route.OrgUID || ''],
      [''],
      ['=== VALIDITY & TIMING ==='],
      ['Valid From:', route.ValidFrom ? new Date(route.ValidFrom).toLocaleDateString() : 'Not Set'],
      ['Valid To:', route.ValidTo ? new Date(route.ValidTo).toLocaleDateString() : 'Not Set'],
      ['Start Time:', route.StartTime || 'Not Set'],
      ['End Time:', route.EndTime || 'Not Set'],
      ['Visit Duration (minutes):', route.VisitDuration || 'Not Set'],
      [''],
      ['=== ASSIGNMENT & RESOURCES ==='],
      ['Assigned Employee/Role:', route.JobPositionUID || 'Not Assigned'],
      ['Vehicle:', route.VehicleUID || 'Not Assigned'],
      ['Warehouse:', route.WHOrgUID || 'Not Assigned'],
      ['Location:', route.LocationUID || 'Not Assigned'],
      [''],
      ['=== SCHEDULE SUMMARY ==='],
      ['Total Route Customers:', routeDetails.RouteCustomersList?.filter((c: any) => !c.IsDeleted).length || 0],
      ['Customers with Schedule:', routeDetails.RouteScheduleCustomerMappings?.length || 0],
      ['Customers without Schedule:', (routeDetails.RouteCustomersList?.filter((c: any) => !c.IsDeleted).length || 0) - (routeDetails.RouteScheduleCustomerMappings?.length || 0)],
      ['Total Schedule Configurations:', routeDetails.RouteScheduleConfigs?.length || 0],
      [''],
      ['=== FREQUENCY BREAKDOWN ==='],
      ['Daily Schedules:', routeDetails.RouteScheduleConfigs?.filter((c: any) => c.ScheduleType?.toLowerCase() === 'daily').length || 0],
      ['Weekly Schedules:', routeDetails.RouteScheduleConfigs?.filter((c: any) => c.ScheduleType?.toLowerCase() === 'weekly').length || 0],
      ['Monthly Schedules:', routeDetails.RouteScheduleConfigs?.filter((c: any) => c.ScheduleType?.toLowerCase() === 'monthly').length || 0],
      ['Fortnight Schedules:', routeDetails.RouteScheduleConfigs?.filter((c: any) => c.ScheduleType?.toLowerCase() === 'fortnight').length || 0],
    ];
    
    const routeInfoSheet = XLSX.utils.aoa_to_sheet(routeInfoData);
    // Style headers
    routeInfoSheet['A1'] = { ...routeInfoSheet['A1'], s: { font: { bold: true, sz: 16 } } };
    routeInfoSheet['A4'] = { ...routeInfoSheet['A4'], s: { font: { bold: true, sz: 12 } } };
    routeInfoSheet['A12'] = { ...routeInfoSheet['A12'], s: { font: { bold: true, sz: 12 } } };
    routeInfoSheet['A19'] = { ...routeInfoSheet['A19'], s: { font: { bold: true, sz: 12 } } };
    routeInfoSheet['A25'] = { ...routeInfoSheet['A25'], s: { font: { bold: true, sz: 12 } } };
    routeInfoSheet['A31'] = { ...routeInfoSheet['A31'], s: { font: { bold: true, sz: 12 } } };
    
    XLSX.utils.book_append_sheet(workbook, routeInfoSheet, 'Route Information');

    // 2. DAILY VISIT PLAN SHEET
    const dailyConfigs = routeDetails.RouteScheduleConfigs?.filter(
      (c: any) => c.ScheduleType?.toLowerCase() === 'daily'
    ) || [];
    
    const dailyData: any[] = [
      ['DAILY VISIT PLAN - EVERY DAY CUSTOMERS'],
      ['This schedule repeats every working day'],
      [''],
      ['Visit Sequence', 'Customer Name', 'Customer Code', 'Address/Location', 'City', 'Contact', 'Visit Start Time', 'Visit End Time', 'Duration (min)', 'Special Instructions']
    ];
    
    if (dailyConfigs.length > 0) {
      dailyConfigs.forEach((config: any) => {
        const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
          (m: any) => m.RouteScheduleConfigUID === config.UID
        ) || [];
        
        customers.sort((a: any, b: any) => (a.SeqNo || 0) - (b.SeqNo || 0));
        
        customers.forEach((cust: any, idx: number) => {
          dailyData.push([
            cust.SeqNo || idx + 1,
            cust.CustomerName || 'Name Not Available',
            cust.CustomerCode || 'N/A',
            cust.StoreAddress || 'Address Not Set',
            cust.StoreCity || 'City Not Set',
            cust.StoreContact || 'Contact Not Available',
            cust.StartTime || config.StartTime || '09:00',
            cust.EndTime || config.EndTime || '18:00',
            config.VisitDuration || 30,
            cust.Notes || ''
          ]);
        });
      });
      
      if (dailyData.length === 4) {
        dailyData.push(['', 'No daily customers configured', '', '', '', '', '', '', '', '']);
      }
    } else {
      dailyData.push(['', 'No daily schedule configured', '', '', '', '', '', '', '', '']);
    }
    
    const dailySheet = XLSX.utils.aoa_to_sheet(dailyData);
    dailySheet['A1'] = { ...dailySheet['A1'], s: { font: { bold: true, sz: 14 } } };
    XLSX.utils.book_append_sheet(workbook, dailySheet, 'Daily Visit Plan');

    // 3. WEEKLY VISIT PLAN SHEET
    const weeklyConfigs = routeDetails.RouteScheduleConfigs?.filter(
      (c: any) => c.ScheduleType?.toLowerCase() === 'weekly'
    ) || [];
    
    const weeklyData: any[] = [
      ['WEEKLY VISIT PLAN - CUSTOMER VISITS BY DAY OF WEEK'],
      ['Sales team should follow this weekly schedule'],
      [''],
      ['Week #', 'Day of Week', 'Visit Seq', 'Customer Name', 'Customer Code', 'Address', 'City', 'Contact', 'Visit Start', 'Visit End', 'Duration', 'Total Day Customers']
    ];
    
    const dayNames = ['', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
    
    if (weeklyConfigs.length > 0) {
      // Group by week
      const weekGroups = new Map<number, any[]>();
      weeklyConfigs.forEach((config: any) => {
        const week = config.WeekNumber || 1;
        if (!weekGroups.has(week)) {
          weekGroups.set(week, []);
        }
        weekGroups.get(week)?.push(config);
      });
      
      // Add week separator
      weekGroups.forEach((configs, week) => {
        weeklyData.push([]);
        weeklyData.push([`=== WEEK ${week} SCHEDULE ===`, '', '', '', '', '', '', '', '', '', '', '']);
        
        // Sort by day
        configs.sort((a, b) => (a.DayNumber || 0) - (b.DayNumber || 0));
        
        // Process each day
        for (let dayNum = 1; dayNum <= 7; dayNum++) {
          const dayConfig = configs.find(c => c.DayNumber === dayNum);
          
          if (dayConfig) {
            const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
              (m: any) => m.RouteScheduleConfigUID === dayConfig.UID
            ) || [];
            
            if (customers.length > 0) {
              customers.sort((a: any, b: any) => (a.SeqNo || 0) - (b.SeqNo || 0));
              
              customers.forEach((cust: any, idx: number) => {
                weeklyData.push([
                  week,
                  dayNames[dayNum],
                  cust.SeqNo || idx + 1,
                  cust.CustomerName || 'Name Not Available',
                  cust.CustomerCode || 'N/A',
                  cust.StoreAddress || 'Address Not Set',
                  cust.StoreCity || 'City Not Set',
                  cust.StoreContact || 'N/A',
                  cust.StartTime || dayConfig.StartTime || '09:00',
                  cust.EndTime || dayConfig.EndTime || '18:00',
                  dayConfig.VisitDuration || 30,
                  idx === 0 ? customers.length : ''
                ]);
              });
            } else {
              weeklyData.push([
                week,
                dayNames[dayNum],
                '',
                '-- No customers scheduled --',
                '',
                '',
                '',
                '',
                dayConfig.StartTime || '09:00',
                dayConfig.EndTime || '18:00',
                '',
                0
              ]);
            }
          } else {
            weeklyData.push([
              week,
              dayNames[dayNum],
              '',
              '-- Day Off / Not Configured --',
              '',
              '',
              '',
              '',
              '',
              '',
              '',
              0
            ]);
          }
        }
      });
    } else {
      weeklyData.push(['', 'No weekly schedule configured', '', '', '', '', '', '', '', '', '', '']);
    }
    
    const weeklySheet = XLSX.utils.aoa_to_sheet(weeklyData);
    weeklySheet['A1'] = { ...weeklySheet['A1'], s: { font: { bold: true, sz: 14 } } };
    XLSX.utils.book_append_sheet(workbook, weeklySheet, 'Weekly Visit Plan');

    // 4. MONTHLY VISIT PLAN SHEET
    const monthlyConfigs = routeDetails.RouteScheduleConfigs?.filter(
      (c: any) => c.ScheduleType?.toLowerCase() === 'monthly'
    ) || [];
    
    const monthlyData: any[] = [
      ['MONTHLY VISIT PLAN - CUSTOMER VISITS BY DATE'],
      ['Visits scheduled for specific dates each month'],
      [''],
      ['Date of Month', 'Visit Seq', 'Customer Name', 'Customer Code', 'Address', 'City', 'Contact', 'Visit Start', 'Visit End', 'Duration', 'Notes']
    ];
    
    if (monthlyConfigs.length > 0) {
      monthlyConfigs.sort((a, b) => (a.DateNumber || 0) - (b.DateNumber || 0));
      
      // Create full month calendar view
      for (let date = 1; date <= 31; date++) {
        const config = monthlyConfigs.find(c => c.DateNumber === date);
        
        if (config) {
          const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
            (m: any) => m.RouteScheduleConfigUID === config.UID
          ) || [];
          
          if (customers.length > 0) {
            customers.sort((a: any, b: any) => (a.SeqNo || 0) - (b.SeqNo || 0));
            
            customers.forEach((cust: any, idx: number) => {
              monthlyData.push([
                date,
                cust.SeqNo || idx + 1,
                cust.CustomerName || 'Name Not Available',
                cust.CustomerCode || 'N/A',
                cust.StoreAddress || 'Address Not Set',
                cust.StoreCity || 'City Not Set',
                cust.StoreContact || 'N/A',
                cust.StartTime || config.StartTime || '09:00',
                cust.EndTime || config.EndTime || '18:00',
                config.VisitDuration || 30,
                cust.Notes || ''
              ]);
            });
          } else {
            monthlyData.push([
              date,
              '',
              '-- No customers scheduled --',
              '',
              '',
              '',
              '',
              config.StartTime || '09:00',
              config.EndTime || '18:00',
              '',
              ''
            ]);
          }
        }
      }
      
      if (monthlyData.length === 4) {
        monthlyData.push(['', '', 'No monthly visits configured', '', '', '', '', '', '', '', '']);
      }
    } else {
      monthlyData.push(['', '', 'No monthly schedule configured', '', '', '', '', '', '', '', '']);
    }
    
    const monthlySheet = XLSX.utils.aoa_to_sheet(monthlyData);
    monthlySheet['A1'] = { ...monthlySheet['A1'], s: { font: { bold: true, sz: 14 } } };
    XLSX.utils.book_append_sheet(workbook, monthlySheet, 'Monthly Visit Plan');

    // 5. FORTNIGHT VISIT PLAN SHEET
    const fortnightConfigs = routeDetails.RouteScheduleConfigs?.filter(
      (c: any) => c.ScheduleType?.toLowerCase() === 'fortnight'
    ) || [];
    
    const fortnightData: any[] = [
      ['FORTNIGHT VISIT PLAN - ALTERNATE WEEK CUSTOMERS'],
      ['Visits scheduled every two weeks'],
      [''],
      ['Pattern', 'Visit Seq', 'Customer Name', 'Customer Code', 'Address', 'City', 'Contact', 'Visit Start', 'Visit End', 'Duration', 'Notes']
    ];
    
    if (fortnightConfigs.length > 0) {
      fortnightConfigs.forEach((config: any, patternIdx: number) => {
        const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
          (m: any) => m.RouteScheduleConfigUID === config.UID
        ) || [];
        
        if (customers.length > 0) {
          fortnightData.push([]);
          fortnightData.push([`=== FORTNIGHT PATTERN ${patternIdx + 1} ===`, '', '', '', '', '', '', '', '', '', '']);
          
          customers.sort((a: any, b: any) => (a.SeqNo || 0) - (b.SeqNo || 0));
          
          customers.forEach((cust: any, idx: number) => {
            fortnightData.push([
              `Pattern ${patternIdx + 1}`,
              cust.SeqNo || idx + 1,
              cust.CustomerName || 'Name Not Available',
              cust.CustomerCode || 'N/A',
              cust.StoreAddress || 'Address Not Set',
              cust.StoreCity || 'City Not Set',
              cust.StoreContact || 'N/A',
              cust.StartTime || config.StartTime || '09:00',
              cust.EndTime || config.EndTime || '18:00',
              config.VisitDuration || 30,
              cust.Notes || ''
            ]);
          });
        }
      });
      
      if (fortnightData.length === 4) {
        fortnightData.push(['', '', 'No fortnight visits configured', '', '', '', '', '', '', '', '']);
      }
    } else {
      fortnightData.push(['', '', 'No fortnight schedule configured', '', '', '', '', '', '', '', '']);
    }
    
    const fortnightSheet = XLSX.utils.aoa_to_sheet(fortnightData);
    fortnightSheet['A1'] = { ...fortnightSheet['A1'], s: { font: { bold: true, sz: 14 } } };
    XLSX.utils.book_append_sheet(workbook, fortnightSheet, 'Fortnight Visit Plan');

    // 6. ALL ASSIGNED CUSTOMERS SHEET
    const allCustomersData: any[] = [
      ['COMPLETE CUSTOMER LIST - ALL ROUTE CUSTOMERS'],
      ['This includes all customers assigned to this route'],
      [''],
      ['#', 'Customer Name', 'Customer Code', 'Address', 'City', 'Region', 'Contact', 'Email', 'Schedule Status', 'Active Status', 'Visit Frequency', 'Visit Days']
    ];
    
    const scheduledCustomerUIDs = new Set(
      routeDetails.RouteScheduleCustomerMappings?.map((m: any) => m.CustomerUID) || []
    );
    
    // Create a map of customer schedules
    const customerScheduleMap = new Map<string, string[]>();
    
    // Process daily schedules
    dailyConfigs.forEach((config: any) => {
      const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
        (m: any) => m.RouteScheduleConfigUID === config.UID
      ) || [];
      customers.forEach((cust: any) => {
        if (!customerScheduleMap.has(cust.CustomerUID)) {
          customerScheduleMap.set(cust.CustomerUID, []);
        }
        customerScheduleMap.get(cust.CustomerUID)?.push('Daily');
      });
    });
    
    // Process weekly schedules
    weeklyConfigs.forEach((config: any) => {
      const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
        (m: any) => m.RouteScheduleConfigUID === config.UID
      ) || [];
      customers.forEach((cust: any) => {
        if (!customerScheduleMap.has(cust.CustomerUID)) {
          customerScheduleMap.set(cust.CustomerUID, []);
        }
        const dayName = dayNames[config.DayNumber] || '';
        customerScheduleMap.get(cust.CustomerUID)?.push(`Weekly-${dayName}`);
      });
    });
    
    // Process monthly schedules
    monthlyConfigs.forEach((config: any) => {
      const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
        (m: any) => m.RouteScheduleConfigUID === config.UID
      ) || [];
      customers.forEach((cust: any) => {
        if (!customerScheduleMap.has(cust.CustomerUID)) {
          customerScheduleMap.set(cust.CustomerUID, []);
        }
        customerScheduleMap.get(cust.CustomerUID)?.push(`Monthly-Day${config.DateNumber}`);
      });
    });
    
    let customerIndex = 1;
    routeDetails.RouteCustomersList?.forEach((customer: any) => {
      if (!customer.IsDeleted) {
        const schedules = customerScheduleMap.get(customer.UID) || [];
        
        allCustomersData.push([
          customerIndex++,
          customer.StoreName || customer.CustomerName || 'Name Not Available',
          customer.StoreCode || customer.CustomerCode || 'N/A',
          customer.StoreAddress || 'Address Not Set',
          customer.StoreCity || 'City Not Set',
          customer.StoreRegion || 'Region Not Set',
          customer.StoreContact || 'Contact Not Available',
          customer.StoreEmail || 'Email Not Available',
          scheduledCustomerUIDs.has(customer.UID) ? 'SCHEDULED' : 'NOT SCHEDULED',
          customer.IsActive ? 'ACTIVE' : 'INACTIVE',
          schedules.length > 0 ? schedules[0] : 'None',
          schedules.join(', ') || 'No Schedule'
        ]);
      }
    });
    
    const customersSheet = XLSX.utils.aoa_to_sheet(allCustomersData);
    customersSheet['A1'] = { ...customersSheet['A1'], s: { font: { bold: true, sz: 14 } } };
    XLSX.utils.book_append_sheet(workbook, customersSheet, 'All Customers');

    // 7. UNSCHEDULED CUSTOMERS SHEET
    const unscheduledData: any[] = [
      ['UNSCHEDULED CUSTOMERS - NEED SCHEDULE ASSIGNMENT'],
      ['These customers are assigned to the route but have no visit schedule'],
      [''],
      ['#', 'Customer Name', 'Customer Code', 'Address', 'City', 'Contact', 'Status', 'Action Required']
    ];
    
    let unscheduledIndex = 1;
    routeDetails.RouteCustomersList?.forEach((customer: any) => {
      if (!customer.IsDeleted && !scheduledCustomerUIDs.has(customer.UID)) {
        unscheduledData.push([
          unscheduledIndex++,
          customer.StoreName || customer.CustomerName || 'Name Not Available',
          customer.StoreCode || customer.CustomerCode || 'N/A',
          customer.StoreAddress || 'Address Not Set',
          customer.StoreCity || 'City Not Set',
          customer.StoreContact || 'Contact Not Available',
          customer.IsActive ? 'Active' : 'Inactive',
          'Add to schedule'
        ]);
      }
    });
    
    if (unscheduledIndex === 1) {
      unscheduledData.push(['', 'All customers have been scheduled!', '', '', '', '', '', '']);
    }
    
    const unscheduledSheet = XLSX.utils.aoa_to_sheet(unscheduledData);
    unscheduledSheet['A1'] = { ...unscheduledSheet['A1'], s: { font: { bold: true, sz: 14 } } };
    XLSX.utils.book_append_sheet(workbook, unscheduledSheet, 'Unscheduled Customers');

    // 8. SALES TEAM INSTRUCTIONS SHEET
    const instructionsData = [
      ['SALES TEAM INSTRUCTIONS & GUIDELINES'],
      ['Route: ' + (route.Name || 'N/A'), 'Code: ' + (route.Code || 'N/A')],
      [''],
      ['=== IMPORTANT INFORMATION ==='],
      ['1. Route Timing:', `${route.StartTime || '09:00'} to ${route.EndTime || '18:00'}`],
      ['2. Average Visit Duration:', `${route.VisitDuration || 30} minutes per customer`],
      ['3. Total Route Customers:', routeDetails.RouteCustomersList?.filter((c: any) => !c.IsDeleted).length || 0],
      ['4. Scheduled Customers:', routeDetails.RouteScheduleCustomerMappings?.length || 0],
      [''],
      ['=== DAILY CHECKLIST ==='],
      ['□ Check daily visit plan sheet for today\'s customers'],
      ['□ Verify customer sequence and timing'],
      ['□ Prepare required documents and samples'],
      ['□ Check vehicle and equipment'],
      ['□ Follow the visit sequence as per schedule'],
      [''],
      ['=== SCHEDULE TYPES ==='],
      ['• DAILY:', 'Visit these customers every working day'],
      ['• WEEKLY:', 'Visit on specific days of the week as scheduled'],
      ['• MONTHLY:', 'Visit on specific dates each month'],
      ['• FORTNIGHT:', 'Visit every two weeks as per pattern'],
      [''],
      ['=== REPORTING REQUIREMENTS ==='],
      ['• Mark attendance for each customer visit'],
      ['• Record order details and feedback'],
      ['• Update customer contact information if changed'],
      ['• Report any issues or customer complaints immediately'],
      [''],
      ['=== CONTACT INFORMATION ==='],
      ['Supervisor:', route.JobPositionUID || 'Not Assigned'],
      ['Warehouse:', route.WHOrgUID || 'Not Assigned'],
      ['Emergency Contact:', 'Update as per company policy'],
      [''],
      ['Generated on:', new Date().toLocaleString()],
      ['This is your official route plan. Follow it carefully for optimal coverage.']
    ];
    
    const instructionsSheet = XLSX.utils.aoa_to_sheet(instructionsData);
    instructionsSheet['A1'] = { ...instructionsSheet['A1'], s: { font: { bold: true, sz: 16 } } };
    XLSX.utils.book_append_sheet(workbook, instructionsSheet, 'Instructions');

    // Generate professional filename with route name and date
    const fileName = `Route_Plan_${route.Name?.replace(/[^a-z0-9]/gi, '_')}_${route.Code?.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().split('T')[0]}.xlsx`;
    
    // Write the file
    XLSX.writeFile(workbook, fileName);
    
    toast({
      title: "Export Successful",
      description: `Complete route plan exported to ${fileName}`,
    });
  };

  // Parse schedule config from route_schedule_config_uid string
  const parseScheduleConfig = (configUID: string) => {
    const parts = configUID.split('_');
    const config: any = {
      UID: configUID,
      ScheduleType: parts[0] || 'daily'
    };
    
    if (parts[0] === 'daily') {
      // daily_NA_0 or daily
      config.ScheduleType = 'daily';
      config.Description = 'Daily visits';
    } else if (parts[0] === 'weekly') {
      // weekly_W1_Monday or weekly_W2_Tuesday
      config.ScheduleType = 'weekly';
      if (parts[1] && parts[1].startsWith('W')) {
        config.WeekNumber = parseInt(parts[1].substring(1));
      }
      if (parts[2]) {
        const days = ['', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
        config.DayNumber = days.indexOf(parts[2]);
        config.DayName = parts[2];
        config.Description = `Week ${config.WeekNumber || '?'} - ${parts[2]}`;
      }
    } else if (parts[0] === 'monthly') {
      // monthly_15 or monthly_date_15
      config.ScheduleType = 'monthly';
      if (parts[1] === 'date' && parts[2]) {
        config.DateNumber = parseInt(parts[2]) || 1;
      } else if (parts[1] && !isNaN(parseInt(parts[1]))) {
        config.DateNumber = parseInt(parts[1]) || 1;
      }
      config.Description = `Date ${config.DateNumber || '?'} of every month`;
    } else if (parts[0] === 'date') {
      // date_15 (alternative monthly format)
      config.ScheduleType = 'monthly';
      config.DateNumber = parseInt(parts[1]) || 1;
      config.Description = `Date ${config.DateNumber} of every month`;
    } else if (parts[0] === 'fortnight' || parts[0] === '1st-3rd' || parts[0] === '2nd-4th') {
      // fortnight_F1_Monday or 1st-3rd_week1_Monday
      config.ScheduleType = 'fortnight';
      if (parts[0] === 'fortnight' && parts[1]) {
        config.FortnightType = parts[1]; // F1 or F2
        if (parts[2]) {
          config.DayName = parts[2];
        }
      } else {
        config.FortnightType = parts[0];
        if (parts[1]) {
          config.WeekNumber = parseInt(parts[1].replace('week', ''));
        }
        if (parts[2]) {
          config.DayName = parts[2];
        }
      }
      config.Description = `Fortnight ${config.FortnightType} - ${config.DayName || 'All days'}`;
    }
    
    return config;
  };

  const loadRouteDetails = async () => {
    if (!uid) return;
    
    setLoading(true);
    try {
      const response = await routeService.getRouteById(uid);
      if (response.IsSuccess && response.Data) {
        console.log('Route Details:', response.Data);
        
        const routeData = response.Data;
        
        // Try to fetch schedule mappings if we have schedule UID
        if (routeData.RouteSchedule && routeData.RouteSchedule.UID) {
          try {
            // Try to get schedule configs first
            const configsResponse = await routeService.getRouteScheduleConfigs(routeData.RouteSchedule.UID);
            if (configsResponse?.IsSuccess && configsResponse.Data) {
              console.log('Schedule Configs:', configsResponse.Data);
              routeData.RouteScheduleConfigs = configsResponse.Data;
            }
            
            // Try to get schedule mappings through a direct API call
            const mappingsResponse = await routeService.getRouteScheduleMappings(routeData.RouteSchedule.UID);
            if (mappingsResponse?.IsSuccess && mappingsResponse.Data) {
              console.log('Schedule Mappings:', mappingsResponse.Data);
              routeData.RouteScheduleCustomerMappings = mappingsResponse.Data;
              setScheduleCustomerMappings(mappingsResponse.Data);
              
              // If we don't have configs, parse them from mappings
              if (!routeData.RouteScheduleConfigs || routeData.RouteScheduleConfigs.length === 0) {
                const uniqueConfigs = new Map<string, any>();
                mappingsResponse.Data.forEach((mapping: any) => {
                  if (mapping.RouteScheduleConfigUID && !uniqueConfigs.has(mapping.RouteScheduleConfigUID)) {
                    const parsedConfig = parseScheduleConfig(mapping.RouteScheduleConfigUID);
                    uniqueConfigs.set(mapping.RouteScheduleConfigUID, parsedConfig);
                  }
                });
                routeData.RouteScheduleConfigs = Array.from(uniqueConfigs.values());
              }
              setScheduleConfigs(routeData.RouteScheduleConfigs);
            }
          } catch (err) {
            console.log('Could not fetch schedule mappings, using fallback', err);
            // Fallback: Parse from existing data
            parseExistingData(routeData);
          }
        } else {
          // Fallback: Parse from existing data
          parseExistingData(routeData);
        }
        
        setRouteDetails(routeData);
      } else {
        throw new Error(response.Message || 'Failed to load route details');
      }
    } catch (error: any) {
      console.error('Error loading route details:', error);
      toast({
        title: "Error",
        description: error.message || "Failed to load route details",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const parseExistingData = (routeData: any) => {
    // Create synthetic configs from available data
    if (routeData.RouteSchedule && !routeData.RouteScheduleConfigs) {
      const configs: any[] = [];
      const schedule = routeData.RouteSchedule;
      
      // Parse schedule type
      const scheduleType = schedule.Type || schedule.ScheduleType || 'daily';
      const types = scheduleType === 'multiple' ? 
        ['daily', 'weekly', 'monthly', 'fortnight'] : 
        scheduleType.split(',').map((t: string) => t.trim());
      
      types.forEach((type: string) => {
        configs.push({
          UID: `${type}_config`,
          ScheduleType: type,
          StartTime: schedule.StartTime || '00:00:00',
          EndTime: schedule.EndTime || '00:00:00',
          VisitDuration: schedule.VisitDurationInMinutes || 0,
          TravelTime: schedule.TravelTimeInMinutes || 0
        });
      });
      
      // Set both in state and in the routeData object
      setScheduleConfigs(configs);
      routeData.RouteScheduleConfigs = configs;
    }
    
    // Parse customer assignments
    if (routeData.RouteCustomersList && !routeData.RouteScheduleCustomerMappings) {
      const mappings: any[] = [];
      
      routeData.RouteCustomersList.forEach((customer: any) => {
        if (!customer.IsDeleted) {
          const frequency = customer.Frequency || 'daily';
          mappings.push({
            UID: customer.UID,
            RouteScheduleConfigUID: `${frequency.toLowerCase()}_config`,
            CustomerUID: customer.StoreUID,
            CustomerName: customer.StoreName || customer.StoreUID,
            CustomerCode: customer.StoreCode || '',
            SeqNo: customer.SeqNo,
            VisitTime: customer.VisitTime,
            VisitDuration: customer.VisitDuration,
            TravelTime: customer.TravelTime
          });
        }
      });
      
      setScheduleCustomerMappings(mappings);
      routeData.RouteScheduleCustomerMappings = mappings;
    }
  };

  const handleEdit = () => {
    router.push(`/distributiondelivery/route-management/routes/edit/${uid}`);
  };


  const getStatusBadge = (isActive: boolean, status: string) => (
    <div className="flex items-center gap-2">
      <Badge 
        variant={isActive ? 'default' : 'secondary'}
        className={cn(
          isActive 
            ? 'bg-green-100 text-green-800 hover:bg-green-100' 
            : 'bg-red-100 text-red-800 hover:bg-red-100'
        )}
      >
        {isActive ? (
          <CheckCircle className="h-3 w-3 mr-1" />
        ) : (
          <XCircle className="h-3 w-3 mr-1" />
        )}
        {isActive ? 'Active' : 'Inactive'}
      </Badge>
      <Badge variant="outline">{status}</Badge>
    </div>
  );

  const getWeekDaysDisplay = (daywise: any) => {
    if (!daywise) return 'Not configured';
    
    const days = [
      { key: 'Sunday', value: daywise.Sunday },
      { key: 'Monday', value: daywise.Monday },
      { key: 'Tuesday', value: daywise.Tuesday },
      { key: 'Wednesday', value: daywise.Wednesday },
      { key: 'Thursday', value: daywise.Thursday },
      { key: 'Friday', value: daywise.Friday },
      { key: 'Saturday', value: daywise.Saturday },
    ];

    const activeDays = days.filter(day => day.value === 1).map(day => day.key.slice(0, 3));
    return activeDays.length > 0 ? activeDays.join(', ') : 'No days selected';
  };

  const getRouteSettings = (route: any) => {
    const settings = [];
    if (route.PrintStanding) settings.push({ label: 'Standing', icon: '📄' });
    if (route.PrintForward) settings.push({ label: 'Forward', icon: '⏩' });
    if (route.PrintTopup) settings.push({ label: 'Topup', icon: '⬆️' });
    if (route.PrintOrderSummary) settings.push({ label: 'Order Summary', icon: '📋' });
    if (route.AutoFreezeJP) settings.push({ label: 'Auto Freeze JP', icon: '❄️' });
    if (route.AddToRun) settings.push({ label: 'Add to Run', icon: '🏃' });
    if (route.IsCustomerWithTime) settings.push({ label: 'Customer with Time', icon: '⏰' });
    return settings;
  };

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex items-center gap-2">
          <Skeleton className="h-8 w-8" />
          <Skeleton className="h-8 w-48" />
        </div>
        <Card>
          <CardContent className="space-y-4 pt-6">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!routeDetails) {
    return (
      <div className="container mx-auto py-6">
        <Card>
          <CardContent className="flex items-center justify-center py-20">
            <div className="text-center">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">Route not found</h3>
              <p className="text-gray-600 mb-4">The requested route could not be found.</p>
              <Button onClick={() => router.push('/distributiondelivery/route-management/routes')}>
                Back to Routes
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  const { Route: route, RouteSchedule: schedule, RouteScheduleDaywise: daywise, RouteCustomersList: customers, RouteUserList: users } = routeDetails;

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => router.push('/distributiondelivery/route-management/routes')}
            >
              <ArrowLeft className="h-4 w-4" />
            </Button>
            <h1 className="text-2xl font-bold">Route Details</h1>
          </div>
          <p className="text-muted-foreground">
            View complete route information and configuration
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={handleEdit}>
            <Edit className="h-4 w-4 mr-2" />
            Edit Route
          </Button>
        </div>
      </div>

      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="assignment">Assignment</TabsTrigger>
          <TabsTrigger value="schedule">Schedule</TabsTrigger>
          <TabsTrigger value="customers">Customers</TabsTrigger>
          <TabsTrigger value="settings">Settings</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Basic Information */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Eye className="h-5 w-5" />
                  Basic Information
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Route Code</label>
                    <p className="text-lg font-semibold">{route.Code}</p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Route Name</label>
                    <p className="text-lg font-semibold">{route.Name}</p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Status</label>
                    <div className="mt-1">
                      {getStatusBadge(route.IsActive, route.Status)}
                    </div>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Organization</label>
                    <p className="flex items-center gap-2 mt-1">
                      <Building className="h-4 w-4" />
                      {route.OrgUID}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Validity & Timing */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Calendar className="h-5 w-5" />
                  Validity & Timing
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Valid Period</label>
                    <p className="text-sm">
                      {moment(route.ValidFrom).format('DD MMM YYYY')} to {moment(route.ValidUpto).format('DD MMM YYYY')}
                    </p>
                  </div>
                  
                  {route.VisitTime && (
                    <div>
                      <label className="text-sm font-medium text-gray-600">Visit Time</label>
                      <p className="flex items-center gap-2">
                        <Clock className="h-4 w-4" />
                        {route.VisitTime} - {route.EndTime || 'No end time'}
                      </p>
                    </div>
                  )}
                  
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-600">Visit Duration</label>
                      <p className="text-sm">{route.VisitDuration} minutes</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-600">Travel Time</label>
                      <p className="text-sm">{route.TravelTime} minutes</p>
                    </div>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Total Customers</label>
                    <p className="text-2xl font-bold text-blue-600">{route.TotalCustomers}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
            
            {/* Schedule Summary Card */}
            {routeDetails?.RouteScheduleConfigs && routeDetails.RouteScheduleConfigs.length > 0 && (
              <Card className="md:col-span-2">
                <CardHeader>
                  <CardTitle className="text-lg flex items-center gap-2">
                    <Calendar className="h-5 w-5" />
                    Schedule Summary
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                    {(() => {
                      // Group by schedule type
                      const scheduleGroups = new Map<string, any[]>();
                      routeDetails.RouteScheduleConfigs.forEach((config: any) => {
                        const type = config.ScheduleType;
                        if (!scheduleGroups.has(type)) {
                          scheduleGroups.set(type, []);
                        }
                        scheduleGroups.get(type)?.push(config);
                      });
                      
                      return Array.from(scheduleGroups.entries()).map(([type, configs]) => {
                        // Count total customers for this frequency
                        const totalCustomers = configs.reduce((sum, config) => {
                          const mappings = routeDetails.RouteScheduleCustomerMappings?.filter(
                            (m: any) => m.RouteScheduleConfigUID === config.UID
                          ) || [];
                          return sum + mappings.length;
                        }, 0);
                        
                        return (
                          <div key={type} className="border rounded-lg p-3 bg-gray-50">
                            <Badge className="capitalize mb-2" variant="default">
                              {type}
                            </Badge>
                            <p className="text-2xl font-bold">{totalCustomers}</p>
                            <p className="text-sm text-gray-600">customers</p>
                            <div className="mt-2 text-xs text-gray-500">
                              {type === 'weekly' && (
                                <span>{configs.length} day(s) configured</span>
                              )}
                              {type === 'monthly' && (
                                <span>{configs.length} date(s) configured</span>
                              )}
                              {type === 'daily' && (
                                <span>Every day</span>
                              )}
                              {type === 'fortnight' && (
                                <span>{configs.length} pattern(s)</span>
                              )}
                            </div>
                          </div>
                        );
                      });
                    })()}
                  </div>
                  
                  {/* Total customers summary */}
                  <div className="mt-4 pt-4 border-t">
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium text-gray-600">Total Scheduled Customers</span>
                      <Badge variant="secondary" className="text-lg px-3">
                        {routeDetails.RouteScheduleCustomerMappings?.length || 0}
                      </Badge>
                    </div>
                    {routeDetails.RouteCustomersList && (
                      <div className="flex items-center justify-between mt-2">
                        <span className="text-sm font-medium text-gray-600">Total Assigned Customers</span>
                        <Badge variant="outline" className="text-lg px-3">
                          {routeDetails.RouteCustomersList.filter((c: any) => !c.IsDeleted).length}
                        </Badge>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        </TabsContent>

        <TabsContent value="assignment" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Users className="h-5 w-5" />
                Assignment & Resources
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Role</label>
                    <p className="flex items-center gap-2 mt-1">
                      <Settings className="h-4 w-4" />
                      {route.RoleUID || 'Not assigned'}
                    </p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Job Position / Employee</label>
                    <p className="flex items-center gap-2 mt-1">
                      <Users className="h-4 w-4" />
                      {route.JobPositionUID || 'Not assigned'}
                    </p>
                  </div>
                </div>
                
                <div className="space-y-4">
                  <div>
                    <label className="text-sm font-medium text-gray-600">Vehicle</label>
                    <p className="flex items-center gap-2 mt-1">
                      <Truck className="h-4 w-4" />
                      {route.VehicleUID || 'Not assigned'}
                    </p>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Location</label>
                    <p className="flex items-center gap-2 mt-1">
                      <MapPin className="h-4 w-4" />
                      {route.LocationUID || 'Not assigned'}
                    </p>
                  </div>
                </div>
              </div>
              
              <Separator />
              
              <div>
                <label className="text-sm font-medium text-gray-600">Warehouse Organization</label>
                <p className="flex items-center gap-2 mt-1">
                  <Building className="h-4 w-4" />
                  {route.WHOrgUID || 'Not assigned'}
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="schedule" className="space-y-4">
          {/* Export Button */}
          <div className="flex justify-end mb-4">
            <Button 
              onClick={exportToExcel}
              disabled={!routeDetails?.RouteScheduleConfigs?.length}
              className="flex items-center gap-2"
              variant="outline"
            >
              <FileSpreadsheet className="h-4 w-4" />
              Export to Excel
            </Button>
          </div>
          
          {/* Schedule Overview Card */}
          <Card>
            <CardHeader>
              <div className="flex justify-between items-start">
                <div>
                  <CardTitle className="text-lg flex items-center gap-2">
                    <Calendar className="h-5 w-5" />
                    Complete Schedule Breakdown
                  </CardTitle>
                  <CardDescription className="mt-1">
                    Detailed view of all customer visits by frequency, day, and time
                  </CardDescription>
                </div>
                <Button 
                  onClick={exportToExcel}
                  disabled={!routeDetails?.RouteScheduleConfigs?.length}
                  size="sm"
                  className="flex items-center gap-2"
                >
                  <Download className="h-4 w-4" />
                  Download Schedule
                </Button>
              </div>
            </CardHeader>
            <CardContent className="space-y-6">

              {/* Frequency Type Status Overview */}
              {routeDetails?.RouteScheduleConfigs?.length > 0 ? (
                <div className="bg-white p-4 rounded-lg border">
                  <h4 className="font-medium text-sm text-gray-700 mb-4">Schedule Frequency Configuration Status</h4>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                  {['daily', 'weekly', 'monthly', 'fortnight'].map(type => {
                    const configs = routeDetails?.RouteScheduleConfigs?.filter(
                      (c: any) => c.ScheduleType?.toLowerCase() === type || 
                                  (type === 'monthly' && c.ScheduleType?.toLowerCase() === 'month') ||
                                  (type === 'daily' && c.ScheduleType?.toLowerCase() === 'day') ||
                                  (type === 'weekly' && c.ScheduleType?.toLowerCase() === 'week')
                    ) || [];
                    const hasConfig = configs.length > 0;
                    const totalCustomers = configs.reduce((sum: number, config: any) => {
                      const customers = routeDetails?.RouteScheduleCustomerMappings?.filter(
                        (m: any) => m.RouteScheduleConfigUID === config.UID
                      ) || [];
                      return sum + customers.length;
                    }, 0);
                    
                    const isSelected = selectedFrequencyType === type;
                    
                    return (
                      <div 
                        key={type}
                        onClick={() => {
                          if (hasConfig) {
                            setSelectedFrequencyType(isSelected ? null : type);
                          }
                        }}
                        className={`
                          p-3 rounded-lg border-2 transition-all cursor-pointer hover:shadow-md
                          ${isSelected 
                            ? 'bg-blue-100 border-blue-500 ring-2 ring-blue-300'
                            : hasConfig 
                              ? totalCustomers > 0
                                ? 'bg-green-50 border-green-400 hover:bg-green-100'
                                : 'bg-orange-50 border-orange-300 hover:bg-orange-100'
                              : 'bg-gray-50 border-gray-200 opacity-60 cursor-not-allowed'
                          }
                        `}
                      >
                        <div className="flex items-start justify-between mb-2">
                          <span className="text-sm font-semibold capitalize">{type}</span>
                          {hasConfig ? (
                            totalCustomers > 0 ? (
                              <CheckCircle className="h-5 w-5 text-green-600" />
                            ) : (
                              <Clock className="h-5 w-5 text-orange-500" />
                            )
                          ) : (
                            <XCircle className="h-5 w-5 text-gray-400" />
                          )}
                        </div>
                        
                        {hasConfig ? (
                          <>
                            <div className="space-y-1">
                              <p className="text-xl font-bold">
                                {totalCustomers}
                              </p>
                              <p className="text-xs text-gray-600">customers</p>
                            </div>
                            <div className="mt-2 pt-2 border-t border-gray-200">
                              <p className="text-xs text-gray-500">
                                {configs.length} config{configs.length > 1 ? 's' : ''}
                              </p>
                            </div>
                            {totalCustomers === 0 && (
                              <Badge variant="outline" className="mt-2 text-xs border-orange-300 text-orange-700">
                                Needs customers
                              </Badge>
                            )}
                          </>
                        ) : (
                          <div className="text-xs text-gray-500 mt-1">
                            <p className="font-medium">Not configured</p>
                            <p className="mt-1">No schedule set</p>
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
              ) : (
                <div className="bg-gray-50 p-8 rounded-lg border-2 border-dashed border-gray-300 text-center">
                  <XCircle className="h-12 w-12 text-gray-400 mx-auto mb-3" />
                  <h4 className="text-lg font-medium text-gray-700 mb-2">No Schedules Configured</h4>
                  <p className="text-sm text-gray-500 mb-4">
                    This route doesn't have any frequency schedules set up yet.
                  </p>
                  <p className="text-xs text-gray-400">
                    Configure daily, weekly, monthly, or fortnight schedules to start planning customer visits.
                  </p>
                </div>
              )}

              {/* Helper text when no frequency is selected */}
              {!selectedFrequencyType && routeDetails?.RouteScheduleConfigs?.length > 0 && (
                <div className="text-center py-6 bg-blue-50 rounded-lg border-2 border-dashed border-blue-200">
                  <Calendar className="h-8 w-8 text-blue-400 mx-auto mb-2" />
                  <p className="text-sm text-blue-700 font-medium">Click on any frequency card above to view detailed schedule</p>
                  <p className="text-xs text-blue-600 mt-1">Green cards have customers assigned, orange cards need customers</p>
                </div>
              )}

              {/* Selected Frequency Details */}
              {selectedFrequencyType && (
                <Card className="border-blue-200">
                  <CardHeader className="pb-3">
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-lg flex items-center gap-2 capitalize">
                        <Calendar className="h-5 w-5 text-blue-600" />
                        {selectedFrequencyType} Schedule Details
                      </CardTitle>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setSelectedFrequencyType(null)}
                        className="text-gray-500 hover:text-gray-700"
                      >
                        <XCircle className="h-4 w-4" />
                      </Button>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">

                  {/* Daily Schedule */}
                  {selectedFrequencyType === 'daily' && (
                    <div className="space-y-4">
                      {(() => {
                      const dailyConfigs = routeDetails.RouteScheduleConfigs?.filter(
                        (c: any) => c.ScheduleType?.toLowerCase() === 'daily'
                      ) || [];
                      
                      if (dailyConfigs.length === 0) {
                        return <p className="text-gray-500 text-center py-4">No daily schedule configured</p>;
                      }
                      
                      return dailyConfigs.map((config: any) => {
                        const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
                          (m: any) => m.RouteScheduleConfigUID === config.UID
                        ) || [];
                        
                        return (
                          <Card key={config.UID}>
                            <CardHeader className="pb-3">
                              <div className="flex justify-between items-center">
                                <h4 className="font-medium">Daily Visits</h4>
                                <Badge variant="default">{customers.length} customers</Badge>
                              </div>
                            </CardHeader>
                            <CardContent>
                              <div className="space-y-3">
                                <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
                                  <div>
                                    <span className="text-gray-600">Time Range:</span>
                                    <p className="font-medium">{config.StartTime || '00:00:00'} - {config.EndTime || '00:00:00'}</p>
                                  </div>
                                  <div>
                                    <span className="text-gray-600">Visit Duration:</span>
                                    <p className="font-medium">{config.VisitDuration || 0} min</p>
                                  </div>
                                  <div>
                                    <span className="text-gray-600">Travel Time:</span>
                                    <p className="font-medium">{config.TravelTime || 0} min</p>
                                  </div>
                                  <div>
                                    <span className="text-gray-600">Total Time/Day:</span>
                                    <p className="font-medium">
                                      {customers.length * ((config.VisitDuration || 0) + (config.TravelTime || 0))} min
                                    </p>
                                  </div>
                                </div>
                                
                                {customers.length > 0 && (
                                  <div>
                                    <p className="text-sm font-medium text-gray-600 mb-2">Customer List:</p>
                                    <div className="max-h-64 overflow-y-auto pr-2 space-y-3">
                                      {customers.map((cust: any, idx: number) => (
                                        <div key={cust.UID || idx} className="p-3 bg-gray-50 rounded-lg border">
                                          <div className="flex items-start gap-3">
                                            <Badge variant="outline" className="h-8 w-8 p-0 flex items-center justify-center font-medium">
                                              #{cust.SeqNo || idx + 1}
                                            </Badge>
                                            <div className="flex-1 min-w-0">
                                              <div className="font-medium text-gray-900 mb-1">
                                                {cust.CustomerName || cust.CustomerCode || cust.CustomerUID}
                                              </div>
                                              <div className="flex flex-wrap gap-2 text-xs text-gray-600">
                                                {cust.CustomerCode && (
                                                  <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded">
                                                    Code: {cust.CustomerCode}
                                                  </span>
                                                )}
                                                {cust.StoreCity && (
                                                  <span className="bg-green-100 text-green-800 px-2 py-1 rounded">
                                                    📍 {cust.StoreCity}
                                                  </span>
                                                )}
                                                {cust.StartTime && cust.StartTime !== '00:00:00' && (
                                                  <span className="bg-purple-100 text-purple-800 px-2 py-1 rounded">
                                                    🕐 {cust.StartTime}
                                                  </span>
                                                )}
                                              </div>
                                            </div>
                                          </div>
                                        </div>
                                      ))}
                                    </div>
                                  </div>
                                )}
                              </div>
                            </CardContent>
                          </Card>
                        );
                      });
                      })()}
                    </div>
                  )}

                  {/* Weekly Schedule */}
                  {selectedFrequencyType === 'weekly' && (
                    <div className="space-y-4">
                      {(() => {
                      const weeklyConfigs = routeDetails.RouteScheduleConfigs?.filter(
                        (c: any) => c.ScheduleType?.toLowerCase() === 'weekly'
                      ) || [];
                      
                      if (weeklyConfigs.length === 0) {
                        return <p className="text-gray-500 text-center py-4">No weekly schedule configured</p>;
                      }
                      
                      // Group by week and day
                      const weekGroups = new Map<number, any[]>();
                      weeklyConfigs.forEach((config: any) => {
                        const week = config.WeekNumber || 1;
                        if (!weekGroups.has(week)) {
                          weekGroups.set(week, []);
                        }
                        weekGroups.get(week)?.push(config);
                      });
                      
                      return Array.from(weekGroups.entries()).map(([week, configs]) => {
                        const isWeekSelected = selectedWeekDay?.week === week;
                        
                        return (
                          <Card key={week}>
                            <CardHeader className="pb-3">
                              <div className="flex justify-between items-center">
                                <h4 className="font-medium text-lg">Week {week}</h4>
                                <div className="flex items-center gap-2">
                                  <Badge variant="default" className="text-sm">
                                    {configs.reduce((sum, c) => {
                                      const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
                                        (m: any) => m.RouteScheduleConfigUID === c.UID
                                      ) || [];
                                      return sum + customers.length;
                                    }, 0)} customers total
                                  </Badge>
                                  {isWeekSelected && (
                                    <Button
                                      variant="ghost"
                                      size="sm"
                                      onClick={() => setSelectedWeekDay(null)}
                                      className="text-xs"
                                    >
                                      Clear Selection
                                    </Button>
                                  )}
                                </div>
                              </div>
                            </CardHeader>
                            <CardContent>
                              {/* Legend */}
                              <div className="mb-4 flex items-center gap-4 text-xs">
                                <div className="flex items-center gap-2">
                                  <div className="w-4 h-4 bg-green-500 rounded"></div>
                                  <span>Active (Has Customers)</span>
                                </div>
                                <div className="flex items-center gap-2">
                                  <div className="w-4 h-4 bg-orange-500 rounded"></div>
                                  <span>Configured (No Customers)</span>
                                </div>
                                <div className="flex items-center gap-2">
                                  <div className="w-4 h-4 bg-gray-300 rounded"></div>
                                  <span>Not Configured</span>
                                </div>
                              </div>
                              
                              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-7 gap-2">
                                {['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'].map((day, dayIdx) => {
                                  const dayNumber = dayIdx + 1;
                                  const dayConfig = configs.find(c => c.DayNumber === dayNumber);
                                  const customers = dayConfig ? 
                                    (routeDetails.RouteScheduleCustomerMappings?.filter(
                                      (m: any) => m.RouteScheduleConfigUID === dayConfig.UID
                                    ) || []) : [];
                                  
                                  const isSelected = selectedWeekDay?.week === week && selectedWeekDay?.day === dayNumber;
                                  const hasCustomers = customers.length > 0;
                                  const isConfigured = !!dayConfig;
                                  
                                  return (
                                    <div
                                      key={day}
                                      onClick={() => {
                                        if (dayConfig && customers.length > 0) {
                                          setSelectedWeekDay({ week, day: dayNumber });
                                        }
                                      }}
                                      className={`
                                        relative border-2 rounded-lg p-3 transition-all
                                        ${hasCustomers ? 'cursor-pointer hover:shadow-lg' : ''}
                                        ${isSelected 
                                          ? 'bg-blue-100 border-blue-500 ring-2 ring-blue-300' 
                                          : hasCustomers
                                            ? 'bg-green-50 border-green-400 hover:bg-green-100' 
                                            : isConfigured
                                              ? 'bg-orange-50 border-orange-300'
                                              : 'bg-gray-50 border-gray-200 opacity-60'
                                        }
                                      `}
                                    >
                                      {/* Status indicator dot */}
                                      <div className={`absolute top-2 right-2 w-2 h-2 rounded-full ${
                                        hasCustomers ? 'bg-green-500' : isConfigured ? 'bg-orange-500' : 'bg-gray-300'
                                      }`}></div>
                                      
                                      <p className={`font-semibold text-sm mb-1 ${
                                        hasCustomers ? 'text-gray-900' : isConfigured ? 'text-gray-700' : 'text-gray-500'
                                      }`}>{day}</p>
                                      
                                      {isConfigured ? (
                                        <>
                                          {hasCustomers ? (
                                            <>
                                              <Badge 
                                                variant={isSelected ? "default" : "success"} 
                                                className="mb-2"
                                              >
                                                <CheckCircle className="h-3 w-3 mr-1" />
                                                {customers.length} {customers.length === 1 ? 'customer' : 'customers'}
                                              </Badge>
                                              <div className="text-xs text-gray-600 space-y-1">
                                                <p className="flex items-center gap-1">
                                                  <Clock className="h-3 w-3" />
                                                  {dayConfig.StartTime?.slice(0, 5) || '00:00'}
                                                </p>
                                                {dayConfig.VisitDuration > 0 && (
                                                  <p className="flex items-center gap-1">
                                                    <Timer className="h-3 w-3" />
                                                    {dayConfig.VisitDuration}min
                                                  </p>
                                                )}
                                              </div>
                                            </>
                                          ) : (
                                            <>
                                              <Badge variant="outline" className="mb-2 border-orange-300 text-orange-700">
                                                <XCircle className="h-3 w-3 mr-1" />
                                                No customers
                                              </Badge>
                                              <div className="text-xs text-orange-600">
                                                <p>Schedule exists</p>
                                                <p>Add customers</p>
                                              </div>
                                            </>
                                          )}
                                        </>
                                      ) : (
                                        <div className="text-xs text-gray-400">
                                          <XCircle className="h-4 w-4 mb-1" />
                                          <p>Not configured</p>
                                        </div>
                                      )}
                                    </div>
                                  );
                                })}
                              </div>
                              
                              {/* Show selected day's customers */}
                              {selectedWeekDay?.week === week && (() => {
                                const selectedDayConfig = configs.find(c => c.DayNumber === selectedWeekDay.day);
                                const selectedCustomers = selectedDayConfig ? 
                                  (routeDetails.RouteScheduleCustomerMappings?.filter(
                                    (m: any) => m.RouteScheduleConfigUID === selectedDayConfig.UID
                                  ) || []) : [];
                                  
                                const dayNames = ['', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
                                
                                if (!selectedDayConfig || selectedCustomers.length === 0) return null;
                                
                                return (
                                  <div className="mt-6 border-t pt-4">
                                    <div className="flex items-center justify-between mb-4">
                                      <h5 className="text-lg font-medium flex items-center gap-2">
                                        <Calendar className="h-5 w-5 text-blue-600" />
                                        {dayNames[selectedWeekDay.day]} - Customer Schedule
                                      </h5>
                                      <div className="flex items-center gap-2">
                                        <Badge variant="outline" className="text-sm">
                                          {selectedCustomers.length} {selectedCustomers.length === 1 ? 'Customer' : 'Customers'}
                                        </Badge>
                                        <span className="text-sm text-gray-500">
                                          {selectedDayConfig.StartTime?.slice(0, 5) || '00:00'} - {selectedDayConfig.EndTime?.slice(0, 5) || '23:59'}
                                        </span>
                                      </div>
                                    </div>
                                    
                                    <div className="max-h-96 overflow-y-auto pr-2 space-y-3">
                                      {selectedCustomers.map((cust: any, idx: number) => (
                                        <div key={cust.UID || idx} className="p-4 bg-white rounded-lg border hover:shadow-md transition-shadow">
                                          <div className="flex items-start gap-3">
                                            <Badge variant="outline" className="h-8 w-8 p-0 flex items-center justify-center font-medium">
                                              #{cust.SeqNo || idx + 1}
                                            </Badge>
                                            <div className="flex-1 min-w-0">
                                              <div className="font-medium text-gray-900 text-base mb-2">
                                                {cust.CustomerName || cust.CustomerCode || cust.CustomerUID}
                                              </div>
                                              <div className="flex flex-wrap gap-2 text-xs">
                                                {cust.CustomerCode && (
                                                  <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded">
                                                    Code: {cust.CustomerCode}
                                                  </span>
                                                )}
                                                {cust.StoreCity && (
                                                  <span className="bg-green-100 text-green-800 px-2 py-1 rounded">
                                                    📍 {cust.StoreCity}
                                                  </span>
                                                )}
                                                {cust.StartTime && cust.StartTime !== '00:00:00' && (
                                                  <span className="bg-purple-100 text-purple-800 px-2 py-1 rounded">
                                                    🕐 {cust.StartTime.slice(0, 5)} - {cust.EndTime?.slice(0, 5) || '23:59'}
                                                  </span>
                                                )}
                                              </div>
                                            </div>
                                          </div>
                                        </div>
                                      ))}
                                    </div>
                                  </div>
                                );
                              })()}
                          </CardContent>
                        </Card>
                      );
                    });
                      })()}
                    </div>
                  )}

                  {/* Monthly Schedule */}
                  {selectedFrequencyType === 'monthly' && (
                    <div className="space-y-4">
                      {(() => {
                      const monthlyConfigs = routeDetails.RouteScheduleConfigs?.filter(
                        (c: any) => c.ScheduleType?.toLowerCase() === 'monthly' || c.ScheduleType?.toLowerCase() === 'month'
                      ) || [];
                      
                      // Debug logging
                      console.log('Monthly Configs:', monthlyConfigs);
                      console.log('All RouteScheduleConfigs with ScheduleTypes:', routeDetails.RouteScheduleConfigs?.map(c => ({ UID: c.UID, ScheduleType: c.ScheduleType })));
                      console.log('RouteScheduleCustomerMappings:', routeDetails.RouteScheduleCustomerMappings);
                      
                      if (monthlyConfigs.length === 0) {
                        return null;
                      }
                      
                      return (
                        <Card>
                          <CardHeader className="pb-3">
                            <div className="flex justify-between items-center">
                              <h4 className="font-medium">Monthly Visit Schedule</h4>
                              <div className="flex items-center gap-2">
                                <Badge variant="outline">
                                  {monthlyConfigs.reduce((sum, config) => {
                                    const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
                                      (m: any) => m.RouteScheduleConfigUID === config.UID
                                    ) || [];
                                    return sum + customers.length;
                                  }, 0)} customers total
                                </Badge>
                                {selectedMonthDate && (
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => setSelectedMonthDate(null)}
                                    className="text-xs"
                                  >
                                    Clear Selection
                                  </Button>
                                )}
                              </div>
                            </div>
                          </CardHeader>
                          <CardContent>
                            {/* Legend */}
                            <div className="mb-4 flex items-center gap-4 text-xs">
                              <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-green-500 rounded"></div>
                                <span>Active (Has Customers)</span>
                              </div>
                              <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-orange-500 rounded"></div>
                                <span>Configured (No Customers)</span>
                              </div>
                              <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-gray-300 rounded"></div>
                                <span>Not Configured</span>
                              </div>
                              <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-blue-500 rounded"></div>
                                <span>Selected</span>
                              </div>
                            </div>
                            
                            {/* Compact 2-row layout for monthly calendar */}
                            <div className="space-y-2">
                              
                              {/* First row: dates 1-16 */}
                              <div className="grid grid-cols-16 gap-1">
                                {[...Array(16)].map((_, dateIdx) => {
                                  const date = dateIdx + 1;
                                  const dateConfig = monthlyConfigs.find((c: any) => 
                                    c.DateNumber === date || c.DayNumber === date || c.Date === date || parseInt(c.UID?.split('_')[1]) === date
                                  );
                                  const customers = dateConfig ? 
                                    (routeDetails.RouteScheduleCustomerMappings?.filter(
                                      (m: any) => m.RouteScheduleConfigUID === dateConfig.UID
                                    ) || []) : [];
                                  
                                  const isSelected = selectedMonthDate === date;
                                  const hasCustomers = customers.length > 0;
                                  const isConfigured = !!dateConfig;
                                  
                                  return (
                                    <div
                                      key={date}
                                      onClick={() => {
                                        if (dateConfig) {
                                          console.log('Clicked date:', date, 'Config:', dateConfig, 'Customers:', customers.length);
                                          setSelectedMonthDate(date === selectedMonthDate ? null : date);
                                        }
                                      }}
                                      className={`
                                        relative border rounded p-1 h-8 transition-all cursor-pointer flex items-center justify-center
                                        ${isSelected 
                                          ? 'bg-blue-100 border-blue-500 ring-1 ring-blue-300' 
                                          : hasCustomers
                                            ? 'bg-green-50 border-green-400 hover:bg-green-100 hover:shadow-sm' 
                                            : isConfigured
                                              ? 'bg-orange-50 border-orange-300 cursor-default'
                                              : 'bg-gray-50 border-gray-200 opacity-60 cursor-default'
                                        }
                                      `}
                                    >
                                      {/* Customer count at top corner */}
                                      {isConfigured && customers.length > 0 && (
                                        <div className={`absolute -top-1 -right-1 w-4 h-4 rounded-full text-xs font-bold flex items-center justify-center ${
                                          isSelected ? 'bg-blue-600 text-white' : 'bg-green-600 text-white'
                                        }`}>
                                          {customers.length}
                                        </div>
                                      )}
                                      
                                      <span className={`text-xs font-medium ${
                                        hasCustomers ? 'text-gray-900' : isConfigured ? 'text-gray-700' : 'text-gray-500'
                                      }`}>{date}</span>
                                    </div>
                                  );
                                })}
                              </div>
                              
                              {/* Second row: dates 17-31 */}
                              <div className="grid grid-cols-15 gap-1">
                                {[...Array(15)].map((_, dateIdx) => {
                                  const date = dateIdx + 17;
                                  const dateConfig = monthlyConfigs.find((c: any) => 
                                    c.DateNumber === date || c.DayNumber === date || c.Date === date || parseInt(c.UID?.split('_')[1]) === date
                                  );
                                  const customers = dateConfig ? 
                                    (routeDetails.RouteScheduleCustomerMappings?.filter(
                                      (m: any) => m.RouteScheduleConfigUID === dateConfig.UID
                                    ) || []) : [];
                                  
                                  const isSelected = selectedMonthDate === date;
                                  const hasCustomers = customers.length > 0;
                                  const isConfigured = !!dateConfig;
                                  
                                  return (
                                    <div
                                      key={date}
                                      onClick={() => {
                                        if (dateConfig) {
                                          console.log('Clicked date:', date, 'Config:', dateConfig, 'Customers:', customers.length);
                                          setSelectedMonthDate(date === selectedMonthDate ? null : date);
                                        }
                                      }}
                                      className={`
                                        relative border rounded p-1 h-8 transition-all cursor-pointer flex items-center justify-center
                                        ${isSelected 
                                          ? 'bg-blue-100 border-blue-500 ring-1 ring-blue-300' 
                                          : hasCustomers
                                            ? 'bg-green-50 border-green-400 hover:bg-green-100 hover:shadow-sm' 
                                            : isConfigured
                                              ? 'bg-orange-50 border-orange-300 cursor-default'
                                              : 'bg-gray-50 border-gray-200 opacity-60 cursor-default'
                                        }
                                      `}
                                    >
                                      {/* Customer count at top corner */}
                                      {isConfigured && customers.length > 0 && (
                                        <div className={`absolute -top-1 -right-1 w-4 h-4 rounded-full text-xs font-bold flex items-center justify-center ${
                                          isSelected ? 'bg-blue-600 text-white' : 'bg-green-600 text-white'
                                        }`}>
                                          {customers.length}
                                        </div>
                                      )}
                                      
                                      <span className={`text-xs font-medium ${
                                        hasCustomers ? 'text-gray-900' : isConfigured ? 'text-gray-700' : 'text-gray-500'
                                      }`}>{date}</span>
                                    </div>
                                  );
                                })}
                              </div>
                            </div>
                            
                            {/* Show selected date's customers */}
                            {selectedMonthDate && (() => {
                              // Use the same logic as the date finding above
                              const selectedDateConfig = monthlyConfigs.find((c: any) => 
                                c.DateNumber === selectedMonthDate || c.DayNumber === selectedMonthDate || c.Date === selectedMonthDate || parseInt(c.UID?.split('_')[1]) === selectedMonthDate
                              );
                              const selectedCustomers = selectedDateConfig ? 
                                (routeDetails.RouteScheduleCustomerMappings?.filter(
                                  (m: any) => m.RouteScheduleConfigUID === selectedDateConfig.UID
                                ) || []) : [];
                              
                              console.log('Selected date:', selectedMonthDate, 'Config found:', selectedDateConfig, 'Customers:', selectedCustomers);
                              
                              if (!selectedDateConfig) {
                                return (
                                  <div className="mt-6 border-t pt-4">
                                    <div className="bg-red-50 border border-red-200 rounded p-4 text-red-700">
                                      <p><strong>Debug:</strong> No config found for day {selectedMonthDate}</p>
                                    </div>
                                  </div>
                                );
                              }
                              
                              if (selectedCustomers.length === 0) {
                                return (
                                  <div className="mt-6 border-t pt-4">
                                    <div className="bg-orange-50 border border-orange-200 rounded p-4 text-orange-700">
                                      <p><strong>Debug:</strong> Config found for day {selectedMonthDate} but no customers</p>
                                      <p>Config UID: {selectedDateConfig.UID}</p>
                                      <p>Available mappings: {routeDetails.RouteScheduleCustomerMappings?.length || 0}</p>
                                    </div>
                                  </div>
                                );
                              }
                              
                              return (
                                <div className="mt-6 border-t pt-4">
                                  <div className="flex items-center justify-between mb-4">
                                    <h5 className="text-lg font-medium flex items-center gap-2">
                                      <Calendar className="h-5 w-5 text-blue-600" />
                                      Day {selectedMonthDate} - Customer Schedule
                                    </h5>
                                    <div className="flex items-center gap-2">
                                      <Badge variant="outline" className="text-sm">
                                        {selectedCustomers.length} {selectedCustomers.length === 1 ? 'Customer' : 'Customers'}
                                      </Badge>
                                      {selectedDateConfig.StartTime && (
                                        <span className="text-sm text-gray-500">
                                          {selectedDateConfig.StartTime?.slice(0, 5) || '00:00'} - {selectedDateConfig.EndTime?.slice(0, 5) || '23:59'}
                                        </span>
                                      )}
                                    </div>
                                  </div>
                                  
                                  <div className="max-h-80 overflow-y-auto pr-2 space-y-3 border rounded-lg p-4 bg-gray-50">
                                    {selectedCustomers.map((cust: any, idx: number) => (
                                      <div key={cust.UID || idx} className="p-4 bg-white rounded-lg border hover:shadow-md transition-shadow">
                                        <div className="flex items-start gap-3">
                                          <Badge variant="outline" className="h-8 w-8 p-0 flex items-center justify-center font-medium">
                                            #{cust.SeqNo || idx + 1}
                                          </Badge>
                                          <div className="flex-1 min-w-0">
                                            <div className="font-medium text-gray-900 text-base mb-2">
                                              {cust.CustomerName || cust.CustomerCode || cust.CustomerUID}
                                            </div>
                                            <div className="flex flex-wrap gap-2 text-xs">
                                              {cust.CustomerCode && (
                                                <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded">
                                                  Code: {cust.CustomerCode}
                                                </span>
                                              )}
                                              {cust.StoreCity && (
                                                <span className="bg-green-100 text-green-800 px-2 py-1 rounded">
                                                  📍 {cust.StoreCity}
                                                </span>
                                              )}
                                              {cust.StartTime && cust.StartTime !== '00:00:00' && (
                                                <span className="bg-purple-100 text-purple-800 px-2 py-1 rounded">
                                                  🕐 {cust.StartTime.slice(0, 5)} - {cust.EndTime?.slice(0, 5) || '23:59'}
                                                </span>
                                              )}
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              );
                            })()}
                            
                          </CardContent>
                        </Card>
                      );
                    })()}
                    </div>
                  )}

                  {/* Fortnight Schedule */}
                  {selectedFrequencyType === 'fortnight' && (
                    <div className="space-y-4">
                      {(() => {
                      const fortnightConfigs = routeDetails.RouteScheduleConfigs?.filter(
                        (c: any) => c.ScheduleType?.toLowerCase() === 'fortnight'
                      ) || [];
                      
                      if (fortnightConfigs.length === 0) {
                        return <p className="text-gray-500 text-center py-4">No fortnight schedule configured</p>;
                      }
                      
                      // Group by fortnight type
                      const fortnightGroups = new Map<string, any[]>();
                      fortnightConfigs.forEach((config: any) => {
                        const type = config.FortnightType || '1st-3rd';
                        if (!fortnightGroups.has(type)) {
                          fortnightGroups.set(type, []);
                        }
                        fortnightGroups.get(type)?.push(config);
                      });
                      
                      return Array.from(fortnightGroups.entries()).map(([type, configs]) => (
                        <Card key={type}>
                          <CardHeader className="pb-3">
                            <div className="flex justify-between items-center">
                              <h4 className="font-medium">Fortnight Pattern: {type}</h4>
                              <Badge variant="default">
                                {configs.reduce((sum, c) => {
                                  const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
                                    (m: any) => m.RouteScheduleConfigUID === c.UID
                                  ) || [];
                                  return sum + customers.length;
                                }, 0)} customers
                              </Badge>
                            </div>
                          </CardHeader>
                          <CardContent>
                            <div className="space-y-3">
                              {configs.map((config: any) => {
                                const customers = routeDetails.RouteScheduleCustomerMappings?.filter(
                                  (m: any) => m.RouteScheduleConfigUID === config.UID
                                ) || [];
                                
                                return (
                                  <div key={config.UID} className="border rounded-lg p-3">
                                    <div className="flex justify-between items-center mb-2">
                                      <span className="text-sm">
                                        {config.DayNumber && ['', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'][config.DayNumber]}
                                      </span>
                                      <Badge variant="secondary">{customers.length} customers</Badge>
                                    </div>
                                    <div className="max-h-64 overflow-y-auto pr-2 grid grid-cols-1 md:grid-cols-2 gap-2">
                                      {customers.map((cust: any, idx: number) => (
                                        <div key={cust.UID || idx} className="p-2 bg-gray-50 rounded-lg border">
                                          <div className="flex items-start gap-2">
                                            <Badge variant="outline" className="h-5 w-5 p-0 flex items-center justify-center text-xs">
                                              #{cust.SeqNo || idx + 1}
                                            </Badge>
                                            <div className="flex-1 min-w-0">
                                              <div className="font-medium text-gray-900 text-xs mb-1">
                                                {cust.CustomerName || cust.CustomerCode || cust.CustomerUID}
                                              </div>
                                              <div className="flex flex-wrap gap-1 text-xs">
                                                {cust.CustomerCode && (
                                                  <span className="bg-blue-100 text-blue-800 px-1 py-0.5 rounded text-xs">
                                                    {cust.CustomerCode}
                                                  </span>
                                                )}
                                                {cust.StoreCity && (
                                                  <span className="bg-green-100 text-green-800 px-1 py-0.5 rounded text-xs">
                                                    📍 {cust.StoreCity}
                                                  </span>
                                                )}
                                              </div>
                                            </div>
                                          </div>
                                        </div>
                                      ))}
                                    </div>
                                  </div>
                                );
                              })}
                            </div>
                          </CardContent>
                        </Card>
                      ));
                      })()}
                    </div>
                  )}
                    </div>
                  </CardContent>
                </Card>
              )}
              
              {/* Fallback Schedule Display */}
              {!routeDetails?.RouteScheduleConfigs?.length && (schedule || daywise) ? (
                <div className="space-y-4">
                  {/* Fallback to old schedule format */}
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="text-sm font-medium text-gray-600">Visit Frequency</label>
                      <p className="text-lg font-semibold">{schedule.Type || schedule.ScheduleType}</p>
                    </div>
                    
                    <div>
                      <label className="text-sm font-medium text-gray-600">Multiple Beats Per Day</label>
                      <Badge variant={schedule.AllowMultipleBeatsPerDay ? 'default' : 'secondary'}>
                        {schedule.AllowMultipleBeatsPerDay ? 'Allowed' : 'Not Allowed'}
                      </Badge>
                    </div>
                  </div>
                  
                  {(schedule.FromDate || schedule.ToDate) && (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div>
                        <label className="text-sm font-medium text-gray-600">Schedule Period</label>
                        <p className="text-sm">
                          {moment(schedule.FromDate).format('DD MMM YYYY')} to {moment(schedule.ToDate).format('DD MMM YYYY')}
                        </p>
                      </div>
                      
                      <div>
                        <label className="text-sm font-medium text-gray-600">Time Range</label>
                        <p className="text-sm">
                          {schedule.StartTime} - {schedule.EndTime}
                        </p>
                      </div>
                    </div>
                  )}
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="text-sm font-medium text-gray-600">Visit Duration</label>
                      <p className="text-sm">{schedule.VisitDurationInMinutes || schedule.VisitDuration || 0} minutes</p>
                    </div>
                    
                    <div>
                      <label className="text-sm font-medium text-gray-600">Travel Time</label>
                      <p className="text-sm">{schedule.TravelTimeInMinutes || schedule.TravelTime || 0} minutes</p>
                    </div>
                  </div>
                  
                  <Separator />
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Active Days</label>
                    <p className="text-sm mt-1">{getWeekDaysDisplay(daywise)}</p>
                  </div>
                </div>
              ) : (
                <div className="text-center py-8">
                  <p className="text-gray-500 mb-2">Schedule information is being set up</p>
                  <p className="text-sm text-gray-400">Check the frequency tabs above for detailed schedule configurations</p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="customers" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Users className="h-5 w-5" />
                Customer Assignments
              </CardTitle>
              <CardDescription>
                Total: {customers?.filter(c => !c.IsDeleted).length || 0} active customers
              </CardDescription>
            </CardHeader>
            <CardContent>
              {/* Show all customers in a single list */}
              {customers && customers.length > 0 ? (
                <div className="space-y-3 max-h-[32rem] overflow-y-auto pr-2 border rounded-lg p-4 bg-gray-50">
                  {customers.filter(c => !c.IsDeleted).map((customer, index) => (
                    <div key={customer.UID || index} className="flex items-center justify-between p-3 border rounded-lg">
                      <div className="flex items-center gap-3">
                        <Badge variant="outline" className="font-mono">
                          #{customer.SeqNo || index + 1}
                        </Badge>
                        <div>
                          <p className="font-medium">{customer.StoreUID}</p>
                          {customer.VisitTime && (
                            <p className="text-sm text-gray-500">Visit: {customer.VisitTime}</p>
                          )}
                        </div>
                      </div>
                      
                      <div className="text-right text-sm text-gray-500">
                        <p>Duration: {customer.VisitDuration || 0}min</p>
                        <p>Travel: {customer.TravelTime || 0}min</p>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-center text-gray-500 py-8">No customers assigned to this route</p>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="settings" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Route Settings & Options
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div>
                <h4 className="text-sm font-medium text-gray-600 mb-3">Active Settings</h4>
                <div className="flex flex-wrap gap-2">
                  {getRouteSettings(route).map((setting, index) => (
                    <Badge key={index} variant="secondary" className="flex items-center gap-1">
                      <span>{setting.icon}</span>
                      {setting.label}
                    </Badge>
                  ))}
                  {getRouteSettings(route).length === 0 && (
                    <p className="text-sm text-gray-500">No special settings configured</p>
                  )}
                </div>
              </div>
              
              <Separator />
              
              <div>
                <h4 className="text-sm font-medium text-gray-600 mb-3">Additional Information</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                  <div>
                    <label className="text-gray-600">Auto Freeze Time</label>
                    <p>{route.AutoFreezeRunTime || 'Not set'}</p>
                  </div>
                  
                  <div>
                    <label className="text-gray-600">Created By</label>
                    <p>{route.CreatedBy || 'Unknown'}</p>
                  </div>
                  
                  {route.CreatedTime && (
                    <div>
                      <label className="text-gray-600">Created On</label>
                      <p>{moment(route.CreatedTime).format('DD MMM YYYY, HH:mm')}</p>
                    </div>
                  )}
                  
                  {route.ModifiedTime && (
                    <div>
                      <label className="text-gray-600">Last Modified</label>
                      <p>{moment(route.ModifiedTime).format('DD MMM YYYY, HH:mm')}</p>
                    </div>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default RouteDetailView;