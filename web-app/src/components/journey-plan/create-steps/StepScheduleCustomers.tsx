import React, { useState, useEffect } from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ScrollArea } from '@/components/ui/scroll-area';
import { useToast } from '@/components/ui/use-toast';
import { 
  Calendar,
  Clock,
  Users,
  MapPin,
  Plus,
  X,
  Search,
  CheckCircle
} from 'lucide-react';
import moment from 'moment';
import { Separator } from '@/components/ui/separator';
import { format, parseISO } from 'date-fns';
import { cn } from '@/lib/utils';

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


interface RouteScheduleInfo {
  UID?: string;
  CompanyUID?: string;
  RouteUID?: string;
  Name?: string;
  Type?: string;
  StartDate?: string;
  Status?: number;
  FromDate?: string;
  ToDate?: string;
  StartTime?: string;
  EndTime?: string;
  VisitDurationInMinutes?: number;
  TravelTimeInMinutes?: number;
  PlannedDays?: string;
  AllowMultipleBeatsPerDay?: boolean;
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
  
  // Route schedule from form (loaded from route details)
  const routeUID = form.watch('routeUID');
  const [routeSchedule, setRouteSchedule] = useState<RouteScheduleInfo | null>(null);
  const [selectedCustomers, setSelectedCustomers] = useState<string[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [activeTab, setActiveTab] = useState<'overview' | 'customers' | 'schedule'>('overview');
  
  // Simple schedule management
  const [selectedCustomersWithTimes, setSelectedCustomersWithTimes] = useState<Array<{
    customerUID: string;
    startTime: string;
    endTime: string;
    visitDuration: number;
    seqNo: number;
  }>>([]);
  
  // Get planned times from the parent form (set in Basic Setup step)
  const formStartTime = form.watch('plannedStartTime');
  const formEndTime = form.watch('plannedEndTime');
  const defaultDuration = form.watch('defaultDuration') || 30;

  // Load route schedule from API when route changes
  useEffect(() => {
    if (routeUID) {
      loadRouteSchedule(routeUID);
    }
  }, [routeUID]);

  // Initialize form data and sync with selectedCustomersWithTimes
  useEffect(() => {
    const formCustomers = form.watch('selectedCustomersWithTimes') || [];
    setSelectedCustomersWithTimes(formCustomers);
    setSelectedCustomers(formCustomers.map((c: any) => c.customerUID));
  }, [form.watch('selectedCustomersWithTimes')]);

  // Load route schedule information
  const loadRouteSchedule = async (routeUID: string) => {
    try {
      const { api } = await import('@/services/api');
      const response: any = await api.route.getMasterByUID(routeUID);
      
      if (response?.IsSuccess && response?.Data?.RouteSchedule) {
        const schedule = response.Data.RouteSchedule;
        setRouteSchedule(schedule);
        
        // Update form with schedule information if available
        if (schedule.StartTime && !form.watch('plannedStartTime')) {
          const startTime = formatTimeFromTimeSpan(schedule.StartTime);
          form.setValue('plannedStartTime', startTime);
        }
        if (schedule.EndTime && !form.watch('plannedEndTime')) {
          const endTime = formatTimeFromTimeSpan(schedule.EndTime);
          form.setValue('plannedEndTime', endTime);
        }
        if (schedule.VisitDurationInMinutes && !form.watch('defaultDuration')) {
          form.setValue('defaultDuration', schedule.VisitDurationInMinutes);
        }
        if (schedule.TravelTimeInMinutes && !form.watch('defaultTravelTime')) {
          form.setValue('defaultTravelTime', schedule.TravelTimeInMinutes);
        }
      }
    } catch (error) {
      console.error('Error loading route schedule:', error);
    }
  };
  
  // Helper function to format TimeSpan to HH:mm format (same as parent component)
  const formatTimeFromTimeSpan = (timeSpan: string | any): string => {
    if (typeof timeSpan === "string") {
      return timeSpan.substring(0, 5);
    }
    if (timeSpan && typeof timeSpan === "object") {
      const hours = String(timeSpan.Hours || timeSpan.hours || 0).padStart(2, "0");
      const minutes = String(timeSpan.Minutes || timeSpan.minutes || 0).padStart(2, "0");
      return `${hours}:${minutes}`;
    }
    return "00:00";
  };

  // Customer management functions
  const addCustomerToSchedule = (customer: Customer) => {
    const seqNo = selectedCustomersWithTimes.length + 1;
    const startTime = moment(formStartTime || '00:00', 'HH:mm')
      .add((seqNo - 1) * defaultDuration, 'minutes')
      .format('HH:mm');
    const endTime = moment(startTime, 'HH:mm')
      .add(defaultDuration, 'minutes')
      .format('HH:mm');
    
    const newCustomer = {
      customerUID: customer.UID,
      startTime,
      endTime,
      visitDuration: defaultDuration,
      seqNo
    };
    
    const updatedCustomers = [...selectedCustomersWithTimes, newCustomer];
    setSelectedCustomersWithTimes(updatedCustomers);
    setSelectedCustomers(prev => [...prev, customer.UID]);
    form.setValue('selectedCustomersWithTimes', updatedCustomers);
    
    toast({
      title: "Customer Added",
      description: `${customer.Name} has been added to the schedule`,
    });
  };
  
  const removeCustomerFromSchedule = (customerUID: string) => {
    const updatedCustomers = selectedCustomersWithTimes
      .filter(c => c.customerUID !== customerUID)
      .map((c, index) => ({ ...c, seqNo: index + 1 })); // Renumber sequence
    
    setSelectedCustomersWithTimes(updatedCustomers);
    setSelectedCustomers(prev => prev.filter(id => id !== customerUID));
    form.setValue('selectedCustomersWithTimes', updatedCustomers);
    
    const removedCustomer = routeCustomers.find(c => c.UID === customerUID);
    toast({
      title: "Customer Removed",
      description: `${removedCustomer?.Name || 'Customer'} has been removed from the schedule`,
    });
  };


  const recalculateAllTimes = () => {
    const updatedCustomers = selectedCustomersWithTimes.map((customer, index) => {
      const startTime = moment(formStartTime || '00:00', 'HH:mm')
        .add(index * defaultDuration, 'minutes')
        .format('HH:mm');
      const endTime = moment(startTime, 'HH:mm')
        .add(defaultDuration, 'minutes')
        .format('HH:mm');
      
      return {
        ...customer,
        startTime,
        endTime,
        visitDuration: defaultDuration,
        seqNo: index + 1
      };
    });
    
    setSelectedCustomersWithTimes(updatedCustomers);
    form.setValue('selectedCustomersWithTimes', updatedCustomers);
    
    toast({
      title: "Times Recalculated",
      description: "All visit times have been recalculated based on current settings",
    });
  };

  const filteredCustomers = routeCustomers.filter(customer =>
    customer.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    customer.Code.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const unassignedCustomers = filteredCustomers.filter(
    customer => !selectedCustomers.includes(customer.UID)
  );

  // Stats calculations (like route management)
  const totalCustomers = routeCustomers.length;
  const availableCustomers = unassignedCustomers.length;
  const scheduledCustomers = selectedCustomersWithTimes.length;

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="space-y-6"
    >
      {/* Stats Cards - exactly like route management */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white p-6 rounded-lg border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Customers</p>
              <p className="text-2xl font-bold text-gray-900">{totalCustomers}</p>
            </div>
            <Users className="h-8 w-8 text-blue-500" />
          </div>
        </div>
        
        <div className="bg-white p-6 rounded-lg border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Available Customers</p>
              <p className="text-2xl font-bold text-orange-600">{availableCustomers}</p>
            </div>
            <Users className="h-8 w-8 text-orange-500" />
          </div>
        </div>
        
        <div className="bg-white p-6 rounded-lg border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Scheduled Visits</p>
              <p className="text-2xl font-bold text-green-600">{scheduledCustomers}</p>
            </div>
            <Calendar className="h-8 w-8 text-green-500" />
          </div>
        </div>
        
        <div className="bg-white p-6 rounded-lg border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Visit Date</p>
              <p className="text-lg font-bold text-purple-600">
                {moment(form.watch('visitDate')).format('MMM DD')}
              </p>
            </div>
            <Calendar className="h-8 w-8 text-purple-500" />
          </div>
        </div>
      </div>

      {/* Main Content Card - exactly like route management */}
      <div className="bg-white rounded-lg border">
        {/* Navigation Tabs */}
        <div className="border-b border-gray-200">
          <nav className="flex space-x-8 px-6">
            {[
              { id: 'overview', label: 'Overview', icon: Calendar },
              { id: 'customers', label: 'Customers', icon: Users },
              { id: 'schedule', label: 'Schedule', icon: Clock },
            ].map((tab) => {
              const Icon = tab.icon;
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id as any)}
                  className={cn(
                    "flex items-center gap-2 py-4 px-1 border-b-2 font-medium text-sm transition-colors",
                    activeTab === tab.id
                      ? "border-blue-500 text-blue-600"
                      : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
                  )}
                >
                  <Icon className="h-4 w-4" />
                  {tab.label}
                </button>
              );
            })}
          </nav>
        </div>
        
        <div className="p-6">
          {activeTab === 'overview' && (
            <div className="space-y-6">
              {/* Journey Plan Settings */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Journey Plan Settings</h3>
                  
                  <div className="space-y-3">
                    <div className="flex items-center gap-3">
                      <Calendar className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Visit Date</p>
                        <Input
                          type="date"
                          value={moment(form.watch('visitDate')).format('YYYY-MM-DD')}
                          onChange={(e) => form.setValue('visitDate', new Date(e.target.value))}
                          className="w-40 h-8 mt-1"
                        />
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-3">
                      <Clock className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Planned Start Time</p>
                        <Input
                          type="time"
                          value={formStartTime || '00:00'}
                          onChange={(e) => {
                            form.setValue('plannedStartTime', e.target.value);
                            recalculateAllTimes();
                          }}
                          className="w-32 h-8 mt-1"
                        />
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-3">
                      <Clock className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Planned End Time</p>
                        <Input
                          type="time"
                          value={formEndTime || '00:00'}
                          onChange={(e) => form.setValue('plannedEndTime', e.target.value)}
                          className="w-32 h-8 mt-1"
                        />
                      </div>
                    </div>
                  </div>
                </div>
                
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Timing Configuration</h3>
                  
                  <div className="space-y-3">
                    <div className="flex items-center gap-3">
                      <Clock className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Default Visit Duration (minutes)</p>
                        <Input
                          type="number"
                          min="15"
                          max="120"
                          step="15"
                          value={form.watch('defaultDuration') || 30}
                          onChange={(e) => {
                            form.setValue('defaultDuration', Number(e.target.value));
                            recalculateAllTimes();
                          }}
                          className="w-24 h-8 mt-1"
                        />
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-3">
                      <Clock className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Travel Time Between Visits (minutes)</p>
                        <Input
                          type="number"
                          min="5"
                          max="60"
                          step="5"
                          value={form.watch('defaultTravelTime') || 15}
                          onChange={(e) => form.setValue('defaultTravelTime', Number(e.target.value))}
                          className="w-24 h-8 mt-1"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              
              {/* Route Schedule Information - exactly like route management */}
              {routeSchedule && (
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Route Schedule Configuration</h3>
                  
                  <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="flex items-center gap-3">
                        <Calendar className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Schedule Type</p>
                          <p className="font-medium">{routeSchedule.Type || 'Standard'}</p>
                        </div>
                      </div>
                      
                      {routeSchedule.FromDate && routeSchedule.ToDate && (
                        <div className="flex items-center gap-3">
                          <Calendar className="h-5 w-5 text-gray-400" />
                          <div>
                            <p className="text-sm text-gray-500">Schedule Period</p>
                            <p className="font-medium">
                              {format(parseISO(routeSchedule.FromDate), 'MMM d, yyyy')} - {format(parseISO(routeSchedule.ToDate), 'MMM d, yyyy')}
                            </p>
                          </div>
                        </div>
                      )}
                      
                      {routeSchedule.StartTime && routeSchedule.EndTime && (
                        <div className="flex items-center gap-3">
                          <Clock className="h-5 w-5 text-gray-400" />
                          <div>
                            <p className="text-sm text-gray-500">Time Window</p>
                            <p className="font-medium">
                              {routeSchedule.StartTime} - {routeSchedule.EndTime}
                            </p>
                          </div>
                        </div>
                      )}
                      
                      {routeSchedule.VisitDurationInMinutes && (
                        <div className="flex items-center gap-3">
                          <Clock className="h-5 w-5 text-gray-400" />
                          <div>
                            <p className="text-sm text-gray-500">Visit Duration</p>
                            <p className="font-medium">{routeSchedule.VisitDurationInMinutes} minutes</p>
                          </div>
                        </div>
                      )}
                    </div>
                    
                    <div className="flex items-center gap-2 pt-2 border-t">
                      <Badge
                        variant={routeSchedule.Status === 1 ? "default" : "secondary"}
                        className={
                          routeSchedule.Status === 1
                            ? "bg-green-100 text-green-800"
                            : "bg-gray-100 text-gray-800"
                        }
                      >
                        {routeSchedule.Status === 1 ? "Active" : "Inactive"}
                      </Badge>
                      {routeSchedule.AllowMultipleBeatsPerDay && (
                        <Badge variant="outline" className="text-xs">
                          Multiple Visits Allowed
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}
          
          {activeTab === 'customers' && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-gray-900">Available Customers</h3>
                <Badge variant="outline">
                  {availableCustomers} of {totalCustomers} available
                </Badge>
              </div>
              
              <div className="relative mb-4">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search customers..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-9"
                />
              </div>
              
              {unassignedCustomers.length === 0 ? (
                <div className="text-center py-12">
                  <Users className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-gray-600 mb-2">
                    {selectedCustomers.length === 0 ? 'No Customers Found' : 'All Customers Assigned'}
                  </h4>
                  <p className="text-gray-500">
                    {selectedCustomers.length === 0 
                      ? 'No customers match your search criteria.'
                      : 'All customers have been assigned to the journey plan.'}
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {unassignedCustomers.map((customer) => (
                    <div
                      key={customer.UID}
                      className="bg-gray-50 rounded-lg p-4 border border-gray-200"
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                            <Users className="h-5 w-5 text-blue-600" />
                          </div>
                          <div>
                            <div className="flex items-center gap-2">
                              <h4 className="font-semibold text-gray-900">
                                {customer.Name}
                              </h4>
                            </div>
                            <p className="text-sm text-gray-500">
                              Code: {customer.Code}
                            </p>
                            {customer.Address && (
                              <p className="text-sm text-gray-600 flex items-center gap-1 mt-1">
                                <MapPin className="h-3 w-3" />
                                {customer.Address}
                              </p>
                            )}
                          </div>
                        </div>
                        
                        <Button
                          onClick={() => addCustomerToSchedule(customer)}
                          className="bg-blue-600 hover:bg-blue-700"
                        >
                          <Plus className="h-4 w-4 mr-2" />
                          Add to Schedule
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
          
          {activeTab === 'schedule' && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-gray-900">Schedule Overview</h3>
                <div className="flex gap-2">
                  <Badge variant="outline">
                    Total: {scheduledCustomers}
                  </Badge>
                  <Badge variant="outline" className="bg-green-50 text-green-700">
                    Date: {moment(form.watch('visitDate')).format('MMM DD, YYYY')}
                  </Badge>
                  {scheduledCustomers > 0 && (
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={recalculateAllTimes}
                    >
                      <Clock className="h-4 w-4 mr-1" />
                      Recalculate Times
                    </Button>
                  )}
                </div>
              </div>
              
              {selectedCustomersWithTimes.length === 0 ? (
                <div className="text-center py-12">
                  <Calendar className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-gray-600 mb-2">
                    No Scheduled Visits
                  </h4>
                  <p className="text-gray-500">
                    No customers have been scheduled for this journey plan yet.
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {selectedCustomersWithTimes
                    .sort((a, b) => a.seqNo - b.seqNo)
                    .map((scheduledCustomer) => {
                      const customer = routeCustomers.find(c => c.UID === scheduledCustomer.customerUID);
                      if (!customer) return null;
                      
                      return (
                        <div
                          key={scheduledCustomer.customerUID}
                          className="bg-gray-50 rounded-lg p-4 border border-gray-200"
                        >
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-4">
                              <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                                <span className="text-purple-600 font-bold text-sm">
                                  {scheduledCustomer.seqNo}
                                </span>
                              </div>
                              <div>
                                <div className="flex items-center gap-2">
                                  <h4 className="font-semibold text-gray-900">
                                    {customer.Name}
                                  </h4>
                                  <Badge
                                    variant="outline"
                                    className="text-xs bg-blue-100 text-blue-800"
                                  >
                                    Scheduled
                                  </Badge>
                                </div>
                                <div className="flex items-center gap-4 mt-1 text-sm text-gray-600">
                                  <div className="flex items-center gap-1">
                                    <MapPin className="h-3 w-3" />
                                    {customer.Code}
                                  </div>
                                  <div className="flex items-center gap-1">
                                    <Clock className="h-3 w-3" />
                                    {scheduledCustomer.startTime} - {scheduledCustomer.endTime}
                                  </div>
                                  <div className="flex items-center gap-1">
                                    <Clock className="h-3 w-3" />
                                    {scheduledCustomer.visitDuration}min
                                  </div>
                                </div>
                              </div>
                            </div>
                            
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => removeCustomerFromSchedule(scheduledCustomer.customerUID)}
                              className="text-red-600 hover:text-red-700 hover:bg-red-50"
                            >
                              <X className="h-4 w-4 mr-1" />
                              Remove
                            </Button>
                          </div>
                        </div>
                      );
                    })}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </motion.div>
  );
};