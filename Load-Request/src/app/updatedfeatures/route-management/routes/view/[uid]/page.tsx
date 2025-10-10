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
  Trash2,
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
} from 'lucide-react';
import { routeService } from '@/services/routeService';
import { cn } from '@/lib/utils';
import moment from 'moment';

interface RouteDetails {
  Route: any;
  RouteSchedule: any;
  RouteScheduleDaywise: any;
  RouteScheduleFortnight: any;
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

  useEffect(() => {
    if (uid) {
      loadRouteDetails();
    }
  }, [uid]);

  const loadRouteDetails = async () => {
    if (!uid) return;
    
    setLoading(true);
    try {
      const response = await routeService.getRouteById(uid);
      if (response.IsSuccess && response.Data) {
        setRouteDetails(response.Data);
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

  const handleEdit = () => {
    router.push(`/updatedfeatures/route-management/routes/edit/${uid}`);
  };

  const handleDelete = async () => {
    if (!uid) return;
    
    const confirmed = window.confirm('Are you sure you want to delete this route? This action cannot be undone.');
    if (!confirmed) return;

    try {
      await routeService.deleteRoute(uid);
      toast({
        title: "Success",
        description: "Route deleted successfully",
      });
      router.push('/updatedfeatures/route-management/routes/manage');
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "Failed to delete route",
        variant: "destructive",
      });
    }
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
              <Button onClick={() => router.push('/updatedfeatures/route-management/routes/manage')}>
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
              onClick={() => router.push('/updatedfeatures/route-management/routes/manage')}
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
          <Button variant="destructive" onClick={handleDelete}>
            <Trash2 className="h-4 w-4 mr-2" />
            Delete Route
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

                  <div>
                    <label className="text-sm font-medium text-gray-600">Route UID</label>
                    <p className="text-sm text-gray-500 font-mono">{route.UID}</p>
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
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Calendar className="h-5 w-5" />
                Schedule Configuration
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              {schedule ? (
                <div className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="text-sm font-medium text-gray-600">Visit Frequency</label>
                      <p className="text-lg font-semibold">{schedule.Type}</p>
                    </div>
                    
                    <div>
                      <label className="text-sm font-medium text-gray-600">Multiple Beats Per Day</label>
                      <Badge variant={schedule.AllowMultipleBeatsPerDay ? 'default' : 'secondary'}>
                        {schedule.AllowMultipleBeatsPerDay ? 'Allowed' : 'Not Allowed'}
                      </Badge>
                    </div>
                  </div>
                  
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
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="text-sm font-medium text-gray-600">Visit Duration</label>
                      <p className="text-sm">{schedule.VisitDurationInMinutes} minutes</p>
                    </div>
                    
                    <div>
                      <label className="text-sm font-medium text-gray-600">Travel Time</label>
                      <p className="text-sm">{schedule.TravelTimeInMinutes} minutes</p>
                    </div>
                  </div>
                  
                  <Separator />
                  
                  <div>
                    <label className="text-sm font-medium text-gray-600">Active Days</label>
                    <p className="text-sm mt-1">{getWeekDaysDisplay(daywise)}</p>
                  </div>
                </div>
              ) : (
                <p className="text-gray-500">No schedule configuration found</p>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="customers" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Users className="h-5 w-5" />
                Customer Assignments ({customers?.length || 0})
              </CardTitle>
            </CardHeader>
            <CardContent>
              {customers && customers.length > 0 ? (
                <div className="space-y-3 max-h-96 overflow-y-auto">
                  {customers.map((customer, index) => (
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
                        <p>Duration: {customer.VisitDuration || 30}min</p>
                        <p>Travel: {customer.TravelTime || 15}min</p>
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