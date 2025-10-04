"use client";

import React, { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/components/ui/use-toast";
import {
  ArrowLeft,
  MapPin,
  Users,
  Clock,
  Calendar,
  Building2,
  User,
  Car,
  Edit,
  Trash2,
  Phone,
  Mail,
  AlertCircle,
  CheckCircle2,
  TrendingUp,
  Route as RouteIcon,
  Navigation,
  Timer,
  UserCheck,
} from "lucide-react";
import { api } from "@/services/api";
import { format, parseISO } from "date-fns";
import { cn } from "@/lib/utils";

interface RouteScheduleInfo {
  CompanyUID: string;
  RouteUID: string;
  Name: string;
  Type: string;
  StartDate: string;
  Status: number;
  FromDate: string;
  ToDate: string;
  StartTime: string;
  EndTime: string;
  VisitDurationInMinutes: number;
  TravelTimeInMinutes: number;
  NextBeatDate: string;
  LastBeatDate: string;
  AllowMultipleBeatsPerDay: boolean;
  PlannedDays: string;
  Id: number;
  UID: string;
  CreatedBy: string;
  ModifiedBy: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
  IsSelected: boolean;
}

interface RouteUser {
  RouteUID: string;
  JobPositionUID: string;
  FromDate: string;
  ToDate: string;
  IsActive: boolean;
  ActionType: number;
  Id: number;
  UID: string;
  CreatedBy: string;
  ModifiedBy: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
  IsSelected: boolean;
}

interface RouteDetail {
  // Basic route information
  UID: string;
  Code: string;
  Name: string;
  Description?: string;
  CompanyUID?: string;
  OrgUID: string;
  OrganizationName?: string;
  RoleUID: string;
  RoleName?: string;
  JobPositionUID?: string;
  JobPositionName?: string;
  VehicleUID?: string;
  VehicleName?: string;
  LocationUID?: string;
  LocationName?: string;
  IsActive: boolean;
  Status: string;
  ValidFrom: string;
  ValidUpto: string;
  VisitTime?: string;
  VisitDuration?: number;
  TravelTime?: number;
  TotalCustomers?: number;
  CreatedDate?: string;
  ModifiedDate?: string;
  CreatedBy?: string;
  ModifiedBy?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  
  // Additional fields from API response
  serialNumber?: number;
  PrintStanding?: boolean;
  PrintForward?: boolean;
  PrintTopup?: boolean;
  PrintOrderSummary?: boolean;
  AutoFreezeJP?: boolean;
  AddToRun?: boolean;
  IsChangeApplied?: boolean;
  IsCustomerWithTime?: boolean;
  Id?: number;
  IsSelected?: boolean;
  
  // Nested objects from API
  RouteSchedule?: RouteScheduleInfo;
  RouteUserList?: RouteUser[];
}

interface RouteCustomer {
  UID: string;
  RouteUID: string;
  CustomerUID: string;
  CustomerCode?: string;
  CustomerName: string;
  Address?: string;
  ContactNo?: string;
  Sequence: number;
  VisitTime?: string;
  VisitDuration?: number;
  EndTime?: string;
  TravelTime?: number;
  IsActive: boolean;
  Status: string;
  WeekOff?: {
    [key: string]: boolean;
  };
  SpecialDays?: string[];
}

interface RouteSchedule {
  UID: string;
  RouteUID: string;
  CustomerUID: string;
  CustomerName: string;
  VisitDate: string;
  VisitTime: string;
  VisitDuration: number;
  EndTime: string;
  TravelTime: number;
  Sequence: number;
  Status: string;
  IsCompleted: boolean;
}

const RouteDetailPage = () => {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const routeUID = params.uid as string;

  // State
  const [routeDetail, setRouteDetail] = useState<RouteDetail | null>(null);
  const [routeCustomers, setRouteCustomers] = useState<RouteCustomer[]>([]);
  const [routeSchedules, setRouteSchedules] = useState<RouteSchedule[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'overview' | 'customers' | 'schedule'>('overview');

  // Fetch route data
  useEffect(() => {
    if (routeUID) {
      fetchRouteData();
    }
  }, [routeUID]);

  const fetchRouteData = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      // Try multiple APIs to fetch route details
      let routeData = null;
      
      // First try: Get route master details (this fails with backend service issue)
      // Skip this for now and go directly to the working endpoint
      
      // Second try: Get basic route details (THIS WORKS!)
      try {
        const routeResponse = await api.route.getByUID(routeUID);
        if (routeResponse.IsSuccess && routeResponse.Data) {
          // This API returns the route data directly
          const apiData = routeResponse.Data;
          
          // Map the working API response to our interface
          routeData = {
            ...apiData,
            // Map additional fields for display
            OrganizationName: apiData.CompanyUID || apiData.OrgUID,
            RoleName: apiData.RoleUID,
            JobPositionName: apiData.JobPositionUID,
            // Use the dates as they come
            ValidFrom: apiData.ValidFrom,
            ValidUpto: apiData.ValidUpto,
            CreatedDate: apiData.ServerAddTime || apiData.CreatedTime,
            ModifiedDate: apiData.ServerModifiedTime,
            // No nested objects from this API
            RouteSchedule: null,
            RouteUserList: [],
          };
          console.log("Route detail data loaded successfully:", routeData);
        }
      } catch (detailError) {
        console.warn("Failed to fetch route details:", detailError);
      }
      
      // Third try: Search in routes list if basic route fetch failed
      if (!routeData) {
        console.warn("Route detail API failed, trying search fallback");
        try {
          const pagedRequest = {
            PageIndex: 1,
            PageSize: 1000,
            SearchTerm: routeUID,
            SortField: "Code",
            SortDirection: 0
          };
          const routesResponse = await api.route.selectAll(pagedRequest);
          if (routesResponse.IsSuccess && routesResponse.Data?.PagedData) {
            const foundRoute = routesResponse.Data.PagedData.find((r: any) => 
              r.UID === routeUID || r.Code === routeUID
            );
            if (foundRoute) {
              routeData = foundRoute;
            }
          }
        } catch (searchError) {
          console.warn("Failed to search for route:", searchError);
        }
      }
      
      if (!routeData) {
        throw new Error(`Route with ID "${routeUID}" not found. The route may have been deleted or you may not have permission to view it.`);
      }
      
      setRouteDetail(routeData);

      // Fetch route customers (optional - don't fail if this doesn't work)
      try {
        const customersResponse = await api.routeCustomer.getByRouteUID(routeUID);
        if (customersResponse.IsSuccess && customersResponse.Data) {
          const customers = Array.isArray(customersResponse.Data) ? customersResponse.Data : [];
          setRouteCustomers(customers);
        }
      } catch (customerError) {
        console.warn("Route customers data not available:", customerError);
        setRouteCustomers([]);
      }

      // Fetch route schedules (optional - don't fail if this doesn't work)
      try {
        const schedulesResponse = await api.routeCustomerSchedule.getByRoute(routeUID);
        if (schedulesResponse.IsSuccess && schedulesResponse.Data) {
          const schedules = Array.isArray(schedulesResponse.Data) ? schedulesResponse.Data : [];
          setRouteSchedules(schedules);
        }
      } catch (scheduleError) {
        console.warn("Route schedules data not available:", scheduleError);
        setRouteSchedules([]);
      }

    } catch (error: any) {
      console.error("Error fetching route data:", error);
      let errorMessage = "Failed to fetch route data";
      
      if (error.message) {
        errorMessage = error.message;
      } else if (error.status === 404) {
        errorMessage = `Route with ID "${routeUID}" was not found`;
      } else if (error.status === 403) {
        errorMessage = "You don't have permission to view this route";
      } else if (error.status === 500) {
        errorMessage = "Server error occurred while fetching route data";
      }
      
      setError(errorMessage);
      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleEdit = () => {
    router.push(`/distributiondelivery/route-management/routes/edit/${routeUID}`);
  };

  const handleDelete = async () => {
    const confirmMessage = `Are you sure you want to delete the route "${routeDetail?.Name || routeUID}"?\n\nThis will permanently delete:\n• The route configuration\n• All customer assignments\n• All visit schedules\n\nThis action cannot be undone.`;
    
    if (window.confirm(confirmMessage)) {
      try {
        // Try multiple delete endpoints if available
        let deleteResponse = null;
        
        try {
          // Try the standard delete endpoint
          deleteResponse = await api.route.delete?.(routeUID);
        } catch (deleteError) {
          console.warn("Standard delete failed, trying alternative:", deleteError);
          
          // If standard delete fails, you could try alternative approaches here
          // For now, we'll just re-throw the error
          throw deleteError;
        }
        
        if (deleteResponse && deleteResponse.IsSuccess) {
          toast({
            title: "Success",
            description: `Route "${routeDetail?.Name || routeUID}" has been deleted successfully.`,
          });
          router.push("/distributiondelivery/route-management/routes");
        } else {
          throw new Error(deleteResponse?.Message || deleteResponse?.ErrorMessage || "Failed to delete route");
        }
      } catch (error: any) {
        console.error("Delete route error:", error);
        
        let errorMessage = "Failed to delete route";
        if (error.message) {
          errorMessage = error.message;
        } else if (error.status === 404) {
          errorMessage = "Route not found - it may have already been deleted";
        } else if (error.status === 403) {
          errorMessage = "You don't have permission to delete this route";
        } else if (error.status === 500) {
          errorMessage = "Server error occurred while deleting the route";
        }
        
        toast({
          title: "Delete Failed",
          description: errorMessage,
          variant: "destructive",
        });
      }
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="container mx-auto p-6 space-y-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Skeleton className="h-10 w-10" />
            <div>
              <Skeleton className="h-8 w-64 mb-2" />
              <Skeleton className="h-4 w-32" />
            </div>
          </div>
          <div className="flex gap-2">
            <Skeleton className="h-10 w-20" />
            <Skeleton className="h-10 w-20" />
          </div>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="bg-white p-6 rounded-lg border">
              <Skeleton className="h-6 w-6 mb-4" />
              <Skeleton className="h-8 w-16 mb-2" />
              <Skeleton className="h-4 w-24" />
            </div>
          ))}
        </div>
        
        <div className="bg-white p-6 rounded-lg border space-y-4">
          <Skeleton className="h-6 w-48" />
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-16 w-full" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (error || !routeDetail) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center gap-4 mb-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.back()}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back
          </Button>
        </div>
        
        <div className="bg-white rounded-lg border p-12 text-center">
          <AlertCircle className="h-16 w-16 text-red-500 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 mb-2">
            Route Not Found
          </h3>
          <div className="text-gray-600 mb-6 space-y-2">
            <p className="font-medium">Route ID: <code className="bg-gray-100 px-2 py-1 rounded text-sm">{routeUID}</code></p>
            <p>{error || "The route you're looking for doesn't exist or couldn't be loaded."}</p>
            <div className="text-sm text-gray-500 mt-4">
              <p><strong>Possible reasons:</strong></p>
              <ul className="text-left inline-block mt-2 space-y-1">
                <li>• The route may have been deleted</li>
                <li>• You may not have permission to view this route</li>
                <li>• The route ID might be incorrect</li>
                <li>• There may be a temporary server issue</li>
              </ul>
            </div>
          </div>
          <div className="flex justify-center gap-3">
            <Button
              onClick={() => router.push("/distributiondelivery/route-management/routes")}
              variant="outline"
            >
              View All Routes
            </Button>
            <Button
              onClick={() => router.back()}
              variant="outline"
            >
              Go Back
            </Button>
            <Button
              onClick={fetchRouteData}
              className="bg-blue-600 hover:bg-blue-700"
            >
              Try Again
            </Button>
          </div>
        </div>
      </div>
    );
  }

  // Stats calculations
  const totalCustomers = routeCustomers.length;
  const activeCustomers = routeCustomers.filter(c => c.IsActive).length;
  const totalSchedules = routeSchedules.length;
  const completedSchedules = routeSchedules.filter(s => s.IsCompleted).length;

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.back()}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-blue-600 rounded-lg flex items-center justify-center">
                <RouteIcon className="h-5 w-5 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">
                  {routeDetail.Name}
                </h1>
                <div className="flex items-center gap-2 text-sm text-gray-500">
                  <span>Code: {routeDetail.Code}</span>
                  <Separator orientation="vertical" className="h-4" />
                  <Badge
                    variant={routeDetail.IsActive ? "default" : "secondary"}
                    className={cn(
                      routeDetail.IsActive
                        ? "bg-green-100 text-green-800"
                        : "bg-gray-100 text-gray-800"
                    )}
                  >
                    {routeDetail.Status}
                  </Badge>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div className="flex gap-2">
          <Button
            onClick={handleEdit}
            className="bg-blue-600 hover:bg-blue-700"
          >
            <Edit className="h-4 w-4 mr-2" />
            Edit
          </Button>
          <Button
            onClick={handleDelete}
            variant="outline"
            className="text-red-600 hover:text-red-700 hover:bg-red-50"
          >
            <Trash2 className="h-4 w-4 mr-2" />
            Delete
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
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
              <p className="text-sm text-gray-600">Active Customers</p>
              <p className="text-2xl font-bold text-green-600">{activeCustomers}</p>
            </div>
            <UserCheck className="h-8 w-8 text-green-500" />
          </div>
        </div>
        
        <div className="bg-white p-6 rounded-lg border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Visits</p>
              <p className="text-2xl font-bold text-gray-900">{totalSchedules}</p>
            </div>
            <Calendar className="h-8 w-8 text-purple-500" />
          </div>
        </div>
        
        <div className="bg-white p-6 rounded-lg border">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Completed Visits</p>
              <p className="text-2xl font-bold text-blue-600">{completedSchedules}</p>
            </div>
            <CheckCircle2 className="h-8 w-8 text-blue-500" />
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="bg-white rounded-lg border">
        <div className="border-b border-gray-200">
          <nav className="flex space-x-8 px-6">
            {[
              { id: 'overview', label: 'Overview', icon: RouteIcon },
              { id: 'customers', label: 'Customers', icon: Users },
              { id: 'schedule', label: 'Schedule', icon: Calendar },
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
              {/* Route Information */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Route Information</h3>
                  
                  <div className="space-y-3">
                    <div className="flex items-center gap-3">
                      <RouteIcon className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Route Code</p>
                        <p className="font-medium">{routeDetail.Code}</p>
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-3">
                      <Building2 className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Organization</p>
                        <p className="font-medium">{routeDetail.OrganizationName || routeDetail.OrgUID}</p>
                      </div>
                    </div>
                    
                    {routeDetail.Description && (
                      <div className="flex items-start gap-3">
                        <AlertCircle className="h-5 w-5 text-gray-400 mt-0.5" />
                        <div>
                          <p className="text-sm text-gray-500">Description</p>
                          <p className="font-medium">{routeDetail.Description}</p>
                        </div>
                      </div>
                    )}
                    
                    <div className="flex items-center gap-3">
                      <Calendar className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm text-gray-500">Valid Period</p>
                        <p className="font-medium">
                          {format(parseISO(routeDetail.ValidFrom), 'MMM d, yyyy')} - {format(parseISO(routeDetail.ValidUpto), 'MMM d, yyyy')}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Assignment Details</h3>
                  
                  <div className="space-y-3">
                    {routeDetail.RoleName && (
                      <div className="flex items-center gap-3">
                        <User className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Role</p>
                          <p className="font-medium">{routeDetail.RoleName}</p>
                        </div>
                      </div>
                    )}
                    
                    {routeDetail.JobPositionName && (
                      <div className="flex items-center gap-3">
                        <UserCheck className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Job Position</p>
                          <p className="font-medium">{routeDetail.JobPositionName}</p>
                        </div>
                      </div>
                    )}
                    
                    {routeDetail.VehicleName && (
                      <div className="flex items-center gap-3">
                        <Car className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Vehicle</p>
                          <p className="font-medium">{routeDetail.VehicleName}</p>
                        </div>
                      </div>
                    )}
                    
                    {routeDetail.LocationName && (
                      <div className="flex items-center gap-3">
                        <MapPin className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Location</p>
                          <p className="font-medium">{routeDetail.LocationName}</p>
                        </div>
                      </div>
                    )}
                    
                    {routeDetail.VisitTime && (
                      <div className="flex items-center gap-3">
                        <Clock className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Visit Time</p>
                          <p className="font-medium">{routeDetail.VisitTime}</p>
                        </div>
                      </div>
                    )}
                    
                    {routeDetail.VisitDuration && (
                      <div className="flex items-center gap-3">
                        <Timer className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Visit Duration</p>
                          <p className="font-medium">{routeDetail.VisitDuration} minutes</p>
                        </div>
                      </div>
                    )}
                    
                    {routeDetail.TravelTime && (
                      <div className="flex items-center gap-3">
                        <Navigation className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Travel Time</p>
                          <p className="font-medium">{routeDetail.TravelTime} minutes</p>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
              
              {/* Route Schedule Information */}
              {routeDetail.RouteSchedule && (
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Schedule Configuration</h3>
                  
                  <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="flex items-center gap-3">
                        <Calendar className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Schedule Type</p>
                          <p className="font-medium">{routeDetail.RouteSchedule.Type}</p>
                        </div>
                      </div>
                      
                      <div className="flex items-center gap-3">
                        <Clock className="h-5 w-5 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-500">Schedule Period</p>
                          <p className="font-medium">
                            {format(parseISO(routeDetail.RouteSchedule.FromDate), 'MMM d, yyyy')} - {format(parseISO(routeDetail.RouteSchedule.ToDate), 'MMM d, yyyy')}
                          </p>
                        </div>
                      </div>
                      
                      {routeDetail.RouteSchedule.StartTime !== "00:00:00" && (
                        <div className="flex items-center gap-3">
                          <Timer className="h-5 w-5 text-gray-400" />
                          <div>
                            <p className="text-sm text-gray-500">Time Window</p>
                            <p className="font-medium">
                              {routeDetail.RouteSchedule.StartTime} - {routeDetail.RouteSchedule.EndTime}
                            </p>
                          </div>
                        </div>
                      )}
                      
                      <div className="flex items-center gap-3">
                        <Badge
                          variant={routeDetail.RouteSchedule.Status === 1 ? "default" : "secondary"}
                          className={
                            routeDetail.RouteSchedule.Status === 1
                              ? "bg-green-100 text-green-800"
                              : "bg-gray-100 text-gray-800"
                          }
                        >
                          {routeDetail.RouteSchedule.Status === 1 ? "Active" : "Inactive"}
                        </Badge>
                      </div>
                    </div>
                  </div>
                </div>
              )}
              
              {/* Route Users */}
              {routeDetail.RouteUserList && routeDetail.RouteUserList.length > 0 && (
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Assigned Users</h3>
                  
                  <div className="space-y-3">
                    {routeDetail.RouteUserList.map((user: any) => (
                      <div key={user.UID} className="bg-gray-50 rounded-lg p-4">
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-3">
                            <User className="h-5 w-5 text-gray-400" />
                            <div>
                              <p className="font-medium">{user.JobPositionUID}</p>
                              <p className="text-sm text-gray-500">
                                {format(parseISO(user.FromDate), 'MMM d, yyyy')} - {format(parseISO(user.ToDate), 'MMM d, yyyy')}
                              </p>
                            </div>
                          </div>
                          <Badge
                            variant={user.IsActive ? "default" : "secondary"}
                            className={
                              user.IsActive
                                ? "bg-green-100 text-green-800"
                                : "bg-gray-100 text-gray-800"
                            }
                          >
                            {user.IsActive ? "Active" : "Inactive"}
                          </Badge>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
          
          {activeTab === 'customers' && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-gray-900">Route Customers</h3>
                <Badge variant="outline">
                  {activeCustomers} of {totalCustomers} active
                </Badge>
              </div>
              
              {routeCustomers.length === 0 ? (
                <div className="text-center py-12">
                  <Users className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-gray-600 mb-2">
                    No Customers Assigned
                  </h4>
                  <p className="text-gray-500">
                    No customers have been assigned to this route yet.
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {routeCustomers
                    .sort((a, b) => a.Sequence - b.Sequence)
                    .map((customer) => (
                      <div
                        key={customer.UID}
                        className="bg-gray-50 rounded-lg p-4 border border-gray-200"
                      >
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-4">
                            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                              <span className="text-blue-600 font-bold text-sm">
                                {customer.Sequence}
                              </span>
                            </div>
                            <div>
                              <div className="flex items-center gap-2">
                                <h4 className="font-semibold text-gray-900">
                                  {customer.CustomerName}
                                </h4>
                                <Badge
                                  variant={customer.IsActive ? "default" : "secondary"}
                                  className={cn(
                                    "text-xs",
                                    customer.IsActive
                                      ? "bg-green-100 text-green-800"
                                      : "bg-gray-100 text-gray-800"
                                  )}
                                >
                                  {customer.Status}
                                </Badge>
                              </div>
                              {customer.CustomerCode && (
                                <p className="text-sm text-gray-500">
                                  Code: {customer.CustomerCode}
                                </p>
                              )}
                              {customer.Address && (
                                <p className="text-sm text-gray-600 flex items-center gap-1 mt-1">
                                  <MapPin className="h-3 w-3" />
                                  {customer.Address}
                                </p>
                              )}
                              {customer.ContactNo && (
                                <p className="text-sm text-gray-600 flex items-center gap-1">
                                  <Phone className="h-3 w-3" />
                                  {customer.ContactNo}
                                </p>
                              )}
                            </div>
                          </div>
                          
                          <div className="text-right text-sm text-gray-500">
                            {customer.VisitTime && (
                              <div className="flex items-center gap-1">
                                <Clock className="h-4 w-4" />
                                {customer.VisitTime}
                              </div>
                            )}
                            {customer.VisitDuration && (
                              <div className="flex items-center gap-1">
                                <Timer className="h-4 w-4" />
                                {customer.VisitDuration}min
                              </div>
                            )}
                          </div>
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
                    Total: {totalSchedules}
                  </Badge>
                  <Badge variant="outline" className="bg-green-50 text-green-700">
                    Completed: {completedSchedules}
                  </Badge>
                </div>
              </div>
              
              {routeSchedules.length === 0 ? (
                <div className="text-center py-12">
                  <Calendar className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-gray-600 mb-2">
                    No Schedules Found
                  </h4>
                  <p className="text-gray-500">
                    No visit schedules have been created for this route yet.
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {routeSchedules
                    .sort((a, b) => new Date(a.VisitDate).getTime() - new Date(b.VisitDate).getTime())
                    .map((schedule) => (
                      <div
                        key={schedule.UID}
                        className="bg-gray-50 rounded-lg p-4 border border-gray-200"
                      >
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-4">
                            <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                              <span className="text-purple-600 font-bold text-sm">
                                {schedule.Sequence}
                              </span>
                            </div>
                            <div>
                              <div className="flex items-center gap-2">
                                <h4 className="font-semibold text-gray-900">
                                  {schedule.CustomerName}
                                </h4>
                                <Badge
                                  variant={schedule.IsCompleted ? "default" : "outline"}
                                  className={cn(
                                    "text-xs",
                                    schedule.IsCompleted
                                      ? "bg-green-100 text-green-800"
                                      : "bg-yellow-100 text-yellow-800"
                                  )}
                                >
                                  {schedule.IsCompleted ? "Completed" : schedule.Status}
                                </Badge>
                              </div>
                              <div className="flex items-center gap-4 mt-1 text-sm text-gray-600">
                                <div className="flex items-center gap-1">
                                  <Calendar className="h-3 w-3" />
                                  {format(parseISO(schedule.VisitDate), 'MMM d, yyyy')}
                                </div>
                                <div className="flex items-center gap-1">
                                  <Clock className="h-3 w-3" />
                                  {schedule.VisitTime} - {schedule.EndTime}
                                </div>
                                <div className="flex items-center gap-1">
                                  <Timer className="h-3 w-3" />
                                  {schedule.VisitDuration}min
                                </div>
                              </div>
                            </div>
                          </div>
                          
                          <div className="text-right">
                            {schedule.IsCompleted ? (
                              <CheckCircle2 className="h-6 w-6 text-green-500" />
                            ) : (
                              <Clock className="h-6 w-6 text-yellow-500" />
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default RouteDetailPage;