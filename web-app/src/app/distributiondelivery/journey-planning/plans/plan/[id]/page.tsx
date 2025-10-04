"use client";

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  ArrowLeft,
  Calendar,
  Clock,
  MapPin,
  Users,
  Route,
  CheckCircle,
  AlertCircle,
  Loader,
  Edit,
  Play,
  Pause,
  Square,
  Navigation,
  Phone,
  Mail,
  Activity,
  TrendingUp,
  Package,
  Store,
  Timer,
  Target,
  XCircle,
  CheckCircle2,
  AlertTriangle,
  Info,
  RefreshCw,
  Download,
  Printer,
} from 'lucide-react';
import moment from 'moment';
import { formatDateToDayMonthYear, formatTime, formatDayOfWeek, formatDateTime } from '@/utils/date-formatter';
import { api } from '@/services/api';

interface BeatHistoryDetail {
  Id: number;
  UID: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
  UserJourneyUID: string;
  RouteUID: string;
  StartTime?: string;
  EndTime?: string;
  JobPositionUID: string;
  LoginId: string;
  VisitDate: string;
  LocationUID: string;
  PlannedStartTime?: string;
  PlannedEndTime?: string;
  PlannedStoreVisits: number;
  UnplannedStoreVisits: number;
  ZeroSalesStoreVisits: number;
  MSLStoreVisits: number;
  SkippedStoreVisits: number;
  ActualStoreVisits: number;
  Coverage: number;
  ACoverage: number;
  TCoverage: number;
  InvoiceStatus: string;
  Notes?: string;
  InvoiceFinalizationDate?: string;
  RouteWHOrgUID: string;
  CFDTime?: string;
  HasAuditCompleted: boolean;
  WHStockAuditUID?: string;
  SS?: number;
  DefaultJobPositionUID: string;
  UserJourneyVehicleUID?: string;
  YearMonth: number;
}

interface StoreVisitDetail {
  UID: string;
  BeatHistoryUID: string;
  StoreUID: string;
  StoreName: string;
  StoreCode: string;
  StoreType?: string;
  CustomerName?: string;
  CustomerCode?: string;
  CustomerType?: string;
  Address?: string;
  City?: string;
  State?: string;
  PinCode?: string;
  ContactPerson?: string;
  ContactNumber?: string;
  EmailId?: string;
  SerialNo: number;
  Status: string;
  IsPlanned: boolean;
  PlannedLoginTime?: string;
  PlannedLogoutTime?: string;
  LoginTime?: string;
  LogoutTime?: string;
  LoginLatitude?: number;
  LoginLongitude?: number;
  LogoutLatitude?: number;
  LogoutLongitude?: number;
  VisitDuration: number;
  TravelTime: number;
  TravelDistance?: number;
  TargetValue: number;
  ActualValue: number;
  OrderValue?: number;
  OrderCount?: number;
  ProductsSold?: number;
  IsProductive: boolean;
  IsSkipped: boolean;
  SkipReason?: string;
  InvoiceCount?: number;
  InvoiceValue?: number;
  CollectionAmount?: number;
  OutstandingAmount?: number;
  Notes?: string;
  ImageCount?: number;
  ActivityCount?: number;
  ModifiedTime?: string;
}

interface RouteDetail {
  UID: string;
  Code: string;
  Name: string;
  Description?: string;
  OrgUID: string;
  OrgName?: string;
  JobPositionUID?: string;
  JobPositionName?: string;
  VehicleUID?: string;
  VehicleName?: string;
  VehicleNumber?: string;
  TotalCustomers: number;
  ActiveCustomers?: number;
  IsActive: boolean;
}

interface EmployeeDetail {
  UID: string;
  LoginId: string;
  Name: string;
  EmailId?: string;
  MobileNumber?: string;
  JobPositionUID: string;
  JobPositionName?: string;
  OrgUID: string;
  OrgName?: string;
  ManagerUID?: string;
  ManagerName?: string;
  IsActive: boolean;
}

const JourneyPlanDetailView: React.FC = () => {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const journeyPlanId = params.id as string;

  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [beatHistory, setBeatHistory] = useState<BeatHistoryDetail | null>(null);
  const [storeVisits, setStoreVisits] = useState<StoreVisitDetail[]>([]);
  const [routeDetails, setRouteDetails] = useState<RouteDetail | null>(null);
  const [employeeDetails, setEmployeeDetails] = useState<EmployeeDetail | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState("overview");

  useEffect(() => {
    if (journeyPlanId) {
      loadCompleteJourneyPlanDetails();
    }
  }, [journeyPlanId]);

  const loadCompleteJourneyPlanDetails = async () => {
    try {
      setLoading(true);
      
      // Load journey plan (beat history) details
      console.log('Loading journey plan details for:', journeyPlanId);
      
      // Try to get beat history by UID
      const beatHistoryResponse = await api.beatHistory.getByUID(journeyPlanId);
      
      if (beatHistoryResponse?.IsSuccess && beatHistoryResponse?.Data) {
        const beatData = beatHistoryResponse.Data;
        setBeatHistory(beatData);
        
        // Load store visits for this journey plan
        if (beatData.UID) {
          await loadStoreVisits(beatData.UID);
        }
        
        // Load route details
        if (beatData.RouteUID) {
          await loadRouteDetails(beatData.RouteUID);
        }
        
        // Load employee details
        if (beatData.LoginId) {
          await loadEmployeeDetails(beatData.LoginId);
        }
      } else {
        // If direct UID fetch fails, try with SelectAll using filter
        const selectAllResponse = await api.beatHistory.selectAll({
          pageNumber: 1,
          pageSize: 1,
          isCountRequired: false,
          sortCriterias: [],
          filterCriterias: [{
            fieldName: 'UID',
            operator: 'EQ',
            value: journeyPlanId
          }]
        });
        
        if (selectAllResponse?.IsSuccess && selectAllResponse?.Data?.PagedData?.length > 0) {
          const beatData = selectAllResponse.Data.PagedData[0];
          setBeatHistory(beatData);
          
          // Load related data
          if (beatData.UID) {
            await loadStoreVisits(beatData.UID);
          }
          if (beatData.RouteUID) {
            await loadRouteDetails(beatData.RouteUID);
          }
          if (beatData.LoginId) {
            await loadEmployeeDetails(beatData.LoginId);
          }
        } else {
          toast({
            title: "Error",
            description: "Journey plan not found",
            variant: "destructive",
          });
        }
      }
    } catch (error) {
      console.error('Error loading journey plan details:', error);
      toast({
        title: "Error",
        description: "Failed to load journey plan details",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const loadStoreVisits = async (beatHistoryUID: string) => {
    try {
      console.log('Loading store visits for beat history:', beatHistoryUID);
      const response = await api.beatHistory.getCustomersByBeatHistoryUID(beatHistoryUID);
      
      if (response?.IsSuccess && response?.Data) {
        // Handle different response formats
        let visits = [];
        if (Array.isArray(response.Data)) {
          visits = response.Data;
        } else if (response.Data.StoreHistories) {
          visits = response.Data.StoreHistories;
        } else if (response.Data.PagedData) {
          visits = response.Data.PagedData;
        }
        
        // Transform and enrich store visit data
        const transformedVisits = visits.map((visit: any, index: number) => ({
          UID: visit.UID || `visit_${index}`,
          BeatHistoryUID: beatHistoryUID,
          StoreUID: visit.StoreUID || visit.CustomerUID,
          StoreName: visit.StoreName || visit.CustomerName || 'Unknown Store',
          StoreCode: visit.StoreCode || visit.CustomerCode || '',
          StoreType: visit.StoreType || visit.CustomerType || 'Regular',
          CustomerName: visit.CustomerName || visit.StoreName,
          CustomerCode: visit.CustomerCode || visit.StoreCode,
          CustomerType: visit.CustomerType || 'Retail',
          Address: visit.Address || visit.CustomerAddress || '',
          City: visit.City,
          State: visit.State,
          PinCode: visit.PinCode,
          ContactPerson: visit.ContactPerson,
          ContactNumber: visit.ContactNumber || visit.MobileNumber,
          EmailId: visit.EmailId,
          SerialNo: visit.SerialNo || index + 1,
          Status: visit.Status || 'Pending',
          IsPlanned: visit.IsPlanned !== false,
          PlannedLoginTime: visit.PlannedLoginTime,
          PlannedLogoutTime: visit.PlannedLogoutTime,
          LoginTime: visit.LoginTime,
          LogoutTime: visit.LogoutTime,
          LoginLatitude: visit.LoginLatitude,
          LoginLongitude: visit.LoginLongitude,
          LogoutLatitude: visit.LogoutLatitude,
          LogoutLongitude: visit.LogoutLongitude,
          VisitDuration: visit.VisitDuration || 0,
          TravelTime: visit.TravelTime || 0,
          TravelDistance: visit.TravelDistance,
          TargetValue: visit.TargetValue || 0,
          ActualValue: visit.ActualValue || 0,
          OrderValue: visit.OrderValue || visit.InvoiceValue || 0,
          OrderCount: visit.OrderCount || visit.InvoiceCount || 0,
          ProductsSold: visit.ProductsSold || 0,
          IsProductive: visit.IsProductive || false,
          IsSkipped: visit.IsSkipped || false,
          SkipReason: visit.SkipReason,
          InvoiceCount: visit.InvoiceCount || 0,
          InvoiceValue: visit.InvoiceValue || 0,
          CollectionAmount: visit.CollectionAmount || 0,
          OutstandingAmount: visit.OutstandingAmount || 0,
          Notes: visit.Notes,
          ImageCount: visit.ImageCount || 0,
          ActivityCount: visit.ActivityCount || 0,
          ModifiedTime: visit.ModifiedTime,
        }));
        
        setStoreVisits(transformedVisits.sort((a, b) => a.SerialNo - b.SerialNo));
      }
    } catch (error) {
      console.error('Error loading store visits:', error);
    }
  };

  const loadRouteDetails = async (routeUID: string) => {
    try {
      console.log('Loading route details for:', routeUID);
      const response = await api.route.getByUID(routeUID);
      
      if (response?.IsSuccess && response?.Data) {
        setRouteDetails(response.Data);
      }
    } catch (error) {
      console.error('Error loading route details:', error);
    }
  };

  const loadEmployeeDetails = async (loginId: string) => {
    try {
      console.log('Loading employee details for:', loginId);
      // Try to load employee details - this might need a different API endpoint
      // For now, we'll use what we have from beat history
      setEmployeeDetails({
        UID: loginId,
        LoginId: loginId,
        Name: loginId, // Will be replaced with actual name from API
        JobPositionUID: beatHistory?.JobPositionUID || '',
        OrgUID: beatHistory?.RouteWHOrgUID || '',
        IsActive: true,
      });
    } catch (error) {
      console.error('Error loading employee details:', error);
    }
  };

  const handleRefresh = async () => {
    setRefreshing(true);
    await loadCompleteJourneyPlanDetails();
    setRefreshing(false);
    toast({
      title: "Refreshed",
      description: "Journey plan data has been updated",
    });
  };

  const handleBack = () => {
    router.push('/updatedfeatures/journey-plan-management/journey-plans/manage');
  };

  const handleEdit = () => {
    router.push(`/updatedfeatures/journey-plan-management/journey-plans/edit/${journeyPlanId}`);
  };

  const handlePrint = () => {
    window.print();
  };

  const handleExport = () => {
    // Export functionality - could generate PDF or Excel
    toast({
      title: "Export",
      description: "Export functionality coming soon",
    });
  };

  const handleStatusAction = async (action: string) => {
    try {
      setActionLoading(action);
      
      // Implement actual API calls here
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      toast({
        title: "Success",
        description: `Journey plan ${action} successfully`,
      });
      
      await handleRefresh();
    } catch (error) {
      toast({
        title: "Error",
        description: `Failed to ${action} journey plan`,
        variant: "destructive",
      });
    } finally {
      setActionLoading(null);
    }
  };

  const getStatusVariant = (status: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (status?.toLowerCase()) {
      case 'completed': return 'default';
      case 'in progress': case 'in_progress': case 'started': return 'secondary';
      case 'pending': case 'not started': return 'outline';
      case 'cancelled': case 'skipped': return 'destructive';
      default: return 'outline';
    }
  };

  const getJourneyStatus = (): string => {
    if (!beatHistory) return 'Unknown';
    
    if (beatHistory.EndTime && beatHistory.HasAuditCompleted) {
      return 'Completed';
    } else if (beatHistory.StartTime && !beatHistory.EndTime) {
      return 'In Progress';
    } else if (beatHistory.StartTime && beatHistory.EndTime && !beatHistory.HasAuditCompleted) {
      return 'Pending Audit';
    } else {
      return 'Not Started';
    }
  };

  const calculateStatistics = () => {
    const plannedVisits = beatHistory?.PlannedStoreVisits || 0;
    const actualVisits = beatHistory?.ActualStoreVisits || 0;
    const skippedVisits = beatHistory?.SkippedStoreVisits || 0;
    const pendingVisits = plannedVisits - actualVisits - skippedVisits;
    const productiveVisits = storeVisits.filter(v => v.IsProductive).length;
    const totalOrderValue = storeVisits.reduce((sum, v) => sum + (v.OrderValue || 0), 0);
    const totalTargetValue = storeVisits.reduce((sum, v) => sum + v.TargetValue, 0);
    const totalActualValue = storeVisits.reduce((sum, v) => sum + v.ActualValue, 0);
    const avgVisitDuration = storeVisits.length > 0 
      ? storeVisits.reduce((sum, v) => sum + v.VisitDuration, 0) / storeVisits.length 
      : 0;

    return {
      plannedVisits,
      actualVisits,
      skippedVisits,
      pendingVisits,
      productiveVisits,
      totalOrderValue,
      totalTargetValue,
      totalActualValue,
      avgVisitDuration,
      coverage: beatHistory?.Coverage || 0,
      productivity: plannedVisits > 0 ? Math.round((productiveVisits / plannedVisits) * 100) : 0,
      achievement: totalTargetValue > 0 ? Math.round((totalActualValue / totalTargetValue) * 100) : 0,
    };
  };

  const stats = calculateStatistics();
  const journeyStatus = getJourneyStatus();

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex items-center justify-between">
          <div className="space-y-2">
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-96" />
          </div>
          <div className="flex gap-2">
            <Skeleton className="h-10 w-24" />
            <Skeleton className="h-10 w-24" />
          </div>
        </div>
        
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {[1, 2, 3, 4].map(i => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
        
        <Skeleton className="h-96" />
      </div>
    );
  }

  if (!beatHistory) {
    return (
      <div className="container mx-auto py-6">
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <AlertCircle className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">Journey Plan Not Found</h3>
            <p className="text-muted-foreground mb-4">
              The requested journey plan could not be found.
            </p>
            <Button onClick={handleBack}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to Journey Plans
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-bold">Journey Plan Details</h1>
            <Badge variant={getStatusVariant(journeyStatus)}>
              {journeyStatus}
            </Badge>
          </div>
          <p className="text-muted-foreground">
            {formatDayOfWeek(beatHistory.VisitDate)}, {formatDateToDayMonthYear(beatHistory.VisitDate)} • {beatHistory.UID}
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="icon" onClick={handleRefresh} disabled={refreshing}>
            <RefreshCw className={`h-4 w-4 ${refreshing ? 'animate-spin' : ''}`} />
          </Button>
          <Button variant="outline" size="icon" onClick={handlePrint}>
            <Printer className="h-4 w-4" />
          </Button>
          <Button variant="outline" size="icon" onClick={handleExport}>
            <Download className="h-4 w-4" />
          </Button>
          <Button variant="outline" onClick={handleBack}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back
          </Button>
          <Button onClick={handleEdit}>
            <Edit className="mr-2 h-4 w-4" />
            Edit
          </Button>
        </div>
      </div>

      {/* Key Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Visit Progress
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.actualVisits}/{stats.plannedVisits}</div>
            <Progress value={stats.coverage} className="mt-2 h-2" />
            <p className="text-xs text-muted-foreground mt-1">{stats.coverage}% completed</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Productivity
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.productivity}%</div>
            <p className="text-xs text-muted-foreground mt-1">
              {stats.productiveVisits} productive visits
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Achievement
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">₹{stats.totalActualValue.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground mt-1">
              Target: ₹{stats.totalTargetValue.toLocaleString()}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Avg Visit Time
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{Math.round(stats.avgVisitDuration)} mins</div>
            <p className="text-xs text-muted-foreground mt-1">
              Total stores: {storeVisits.length}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Main Content Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="stores">Store Visits ({storeVisits.length})</TabsTrigger>
          <TabsTrigger value="timeline">Timeline</TabsTrigger>
          <TabsTrigger value="analytics">Analytics</TabsTrigger>
        </TabsList>

        {/* Overview Tab */}
        <TabsContent value="overview" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Journey Information */}
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle>Journey Information</CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Route</label>
                      <div className="flex items-center gap-2 mt-1">
                        <Route className="h-4 w-4" />
                        <span className="font-medium">
                          {routeDetails?.Name || 'Route ' + beatHistory.RouteUID}
                        </span>
                        {routeDetails?.Code && (
                          <span className="text-sm text-muted-foreground">({routeDetails.Code})</span>
                        )}
                      </div>
                    </div>
                    
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Employee</label>
                      <div className="flex items-center gap-2 mt-1">
                        <Users className="h-4 w-4" />
                        <span className="font-medium">{beatHistory.LoginId}</span>
                      </div>
                    </div>

                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Visit Date</label>
                      <div className="flex items-center gap-2 mt-1">
                        <Calendar className="h-4 w-4" />
                        <span className="font-medium">
                          {formatDateToDayMonthYear(beatHistory.VisitDate)}
                        </span>
                      </div>
                    </div>

                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Invoice Status</label>
                      <div className="mt-1">
                        <Badge variant={getStatusVariant(beatHistory.InvoiceStatus)}>
                          {beatHistory.InvoiceStatus}
                        </Badge>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-4">
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Planned Time</label>
                      <div className="flex items-center gap-2 mt-1">
                        <Clock className="h-4 w-4" />
                        <span>
                          {beatHistory.PlannedStartTime ? formatTime(beatHistory.PlannedStartTime) : '09:00'} - 
                          {beatHistory.PlannedEndTime ? formatTime(beatHistory.PlannedEndTime) : '18:00'}
                        </span>
                      </div>
                    </div>

                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Actual Time</label>
                      <div className="flex items-center gap-2 mt-1">
                        <Clock className="h-4 w-4" />
                        <span>
                          {beatHistory.StartTime ? formatTime(beatHistory.StartTime) : '--:--'} - 
                          {beatHistory.EndTime ? formatTime(beatHistory.EndTime) : '--:--'}
                        </span>
                      </div>
                    </div>

                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Audit Status</label>
                      <div className="mt-1">
                        {beatHistory.HasAuditCompleted ? (
                          <Badge variant="default">
                            <CheckCircle className="mr-1 h-3 w-3" />
                            Audit Completed
                          </Badge>
                        ) : (
                          <Badge variant="outline">
                            <AlertCircle className="mr-1 h-3 w-3" />
                            Pending Audit
                          </Badge>
                        )}
                      </div>
                    </div>

                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Organization</label>
                      <div className="text-sm">
                        {routeDetails?.OrgName || beatHistory.RouteWHOrgUID || 'N/A'}
                      </div>
                    </div>
                  </div>
                </div>

                {beatHistory.Notes && (
                  <>
                    <Separator />
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Notes</label>
                      <p className="mt-1 text-sm">{beatHistory.Notes}</p>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>

            {/* Visit Summary */}
            <Card>
              <CardHeader>
                <CardTitle>Visit Summary</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <CheckCircle2 className="h-4 w-4 text-green-600" />
                      <span className="text-sm">Completed</span>
                    </div>
                    <span className="font-bold text-green-600">{stats.actualVisits}</span>
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <AlertTriangle className="h-4 w-4 text-yellow-600" />
                      <span className="text-sm">Pending</span>
                    </div>
                    <span className="font-bold text-yellow-600">{stats.pendingVisits}</span>
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <XCircle className="h-4 w-4 text-red-600" />
                      <span className="text-sm">Skipped</span>
                    </div>
                    <span className="font-bold text-red-600">{stats.skippedVisits}</span>
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Target className="h-4 w-4 text-blue-600" />
                      <span className="text-sm">Unplanned</span>
                    </div>
                    <span className="font-bold text-blue-600">{beatHistory.UnplannedStoreVisits}</span>
                  </div>
                </div>

                <Separator />

                <div className="space-y-3">
                  <div className="text-sm text-muted-foreground">Coverage Metrics</div>
                  
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span>Overall Coverage</span>
                      <span>{beatHistory.Coverage}%</span>
                    </div>
                    <Progress value={beatHistory.Coverage} className="h-2" />
                  </div>
                  
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span>A Coverage</span>
                      <span>{beatHistory.ACoverage}%</span>
                    </div>
                    <Progress value={beatHistory.ACoverage} className="h-2" />
                  </div>
                  
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span>T Coverage</span>
                      <span>{beatHistory.TCoverage}%</span>
                    </div>
                    <Progress value={beatHistory.TCoverage} className="h-2" />
                  </div>
                </div>

                <Separator />

                {/* Journey Actions */}
                <div className="space-y-2">
                  {journeyStatus === 'Not Started' && (
                    <Button
                      className="w-full"
                      onClick={() => handleStatusAction('start')}
                      disabled={actionLoading === 'start'}
                    >
                      {actionLoading === 'start' ? (
                        <Loader className="mr-2 h-4 w-4 animate-spin" />
                      ) : (
                        <Play className="mr-2 h-4 w-4" />
                      )}
                      Start Journey
                    </Button>
                  )}

                  {journeyStatus === 'In Progress' && (
                    <>
                      <Button
                        variant="outline"
                        className="w-full"
                        onClick={() => handleStatusAction('pause')}
                        disabled={actionLoading === 'pause'}
                      >
                        {actionLoading === 'pause' ? (
                          <Loader className="mr-2 h-4 w-4 animate-spin" />
                        ) : (
                          <Pause className="mr-2 h-4 w-4" />
                        )}
                        Pause Journey
                      </Button>
                      <Button
                        className="w-full"
                        onClick={() => handleStatusAction('complete')}
                        disabled={actionLoading === 'complete'}
                      >
                        {actionLoading === 'complete' ? (
                          <Loader className="mr-2 h-4 w-4 animate-spin" />
                        ) : (
                          <CheckCircle className="mr-2 h-4 w-4" />
                        )}
                        Complete Journey
                      </Button>
                    </>
                  )}

                  <Button
                    variant="outline"
                    className="w-full"
                    onClick={() => router.push(`/updatedfeatures/journey-plan-management/live-dashboard?journey=${journeyPlanId}`)}
                  >
                    <Navigation className="mr-2 h-4 w-4" />
                    Track Live
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Audit Trail */}
          <Card>
            <CardHeader>
              <CardTitle>Audit Trail</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Created By</span>
                  <span>{beatHistory.CreatedBy}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Created Time</span>
                  <span>{moment(beatHistory.CreatedTime).format('DD/MM/YYYY HH:mm:ss')}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Last Modified By</span>
                  <span>{beatHistory.ModifiedBy}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Last Modified Time</span>
                  <span>{moment(beatHistory.ModifiedTime).format('DD/MM/YYYY HH:mm:ss')}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Server Add Time</span>
                  <span>{moment(beatHistory.ServerAddTime).format('DD/MM/YYYY HH:mm:ss')}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Server Modified Time</span>
                  <span>{moment(beatHistory.ServerModifiedTime).format('DD/MM/YYYY HH:mm:ss')}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Store Visits Tab */}
        <TabsContent value="stores" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Store Visit Details</CardTitle>
              <CardDescription>
                Complete list of all store visits for this journey plan
              </CardDescription>
            </CardHeader>
            <CardContent>
              {storeVisits.length === 0 ? (
                <div className="text-center py-12 text-muted-foreground">
                  <Store className="h-12 w-12 mx-auto mb-4 opacity-50" />
                  <p>No store visits found for this journey plan</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {storeVisits.map((visit) => (
                    <Card key={visit.UID} className="border">
                      <CardContent className="p-4">
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            {/* Store Header */}
                            <div className="flex items-center gap-3 mb-3">
                              <span className="flex items-center justify-center h-8 w-8 rounded-full bg-primary/10 text-primary font-semibold text-sm">
                                {visit.SerialNo}
                              </span>
                              <div className="flex-1">
                                <div className="flex items-center gap-2">
                                  <h4 className="font-semibold">{visit.StoreName}</h4>
                                  <Badge variant={getStatusVariant(visit.Status)}>
                                    {visit.Status}
                                  </Badge>
                                  {visit.IsProductive && (
                                    <Badge variant="default" className="text-xs">
                                      Productive
                                    </Badge>
                                  )}
                                  {visit.IsSkipped && (
                                    <Badge variant="destructive" className="text-xs">
                                      Skipped
                                    </Badge>
                                  )}
                                </div>
                                <div className="text-sm text-muted-foreground mt-1">
                                  {visit.StoreCode && <span className="mr-3">Code: {visit.StoreCode}</span>}
                                  {visit.StoreType && <span className="mr-3">Type: {visit.StoreType}</span>}
                                  {visit.CustomerType && <span>Customer: {visit.CustomerType}</span>}
                                </div>
                              </div>
                            </div>

                            {/* Store Details Grid */}
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-3">
                              {/* Location Info */}
                              <div className="space-y-2">
                                <div className="text-sm">
                                  <MapPin className="h-3 w-3 inline mr-1" />
                                  <span className="text-muted-foreground">Address:</span>
                                  <div className="text-xs mt-1">
                                    {visit.Address || 'N/A'}
                                    {visit.City && `, ${visit.City}`}
                                    {visit.State && `, ${visit.State}`}
                                    {visit.PinCode && ` - ${visit.PinCode}`}
                                  </div>
                                </div>
                                {visit.ContactPerson && (
                                  <div className="text-sm">
                                    <Users className="h-3 w-3 inline mr-1" />
                                    <span className="text-muted-foreground">Contact:</span>
                                    <div className="text-xs mt-1">{visit.ContactPerson}</div>
                                  </div>
                                )}
                              </div>

                              {/* Visit Timing */}
                              <div className="space-y-2">
                                <div className="text-sm">
                                  <Clock className="h-3 w-3 inline mr-1" />
                                  <span className="text-muted-foreground">Planned:</span>
                                  <div className="text-xs mt-1">
                                    {visit.PlannedLoginTime || '--:--'} - {visit.PlannedLogoutTime || '--:--'}
                                  </div>
                                </div>
                                <div className="text-sm">
                                  <Timer className="h-3 w-3 inline mr-1" />
                                  <span className="text-muted-foreground">Actual:</span>
                                  <div className="text-xs mt-1">
                                    {visit.LoginTime ? moment(visit.LoginTime).format('HH:mm') : '--:--'} - 
                                    {visit.LogoutTime ? moment(visit.LogoutTime).format('HH:mm') : '--:--'}
                                  </div>
                                </div>
                                <div className="text-sm">
                                  <Activity className="h-3 w-3 inline mr-1" />
                                  <span className="text-muted-foreground">Duration:</span>
                                  <span className="text-xs ml-1">{visit.VisitDuration} mins</span>
                                </div>
                              </div>

                              {/* Performance Metrics */}
                              <div className="space-y-2">
                                <div className="text-sm">
                                  <Target className="h-3 w-3 inline mr-1" />
                                  <span className="text-muted-foreground">Target:</span>
                                  <span className="text-xs ml-1">₹{visit.TargetValue.toLocaleString()}</span>
                                </div>
                                <div className="text-sm">
                                  <TrendingUp className="h-3 w-3 inline mr-1" />
                                  <span className="text-muted-foreground">Actual:</span>
                                  <span className="text-xs ml-1">₹{visit.ActualValue.toLocaleString()}</span>
                                </div>
                                {visit.OrderCount > 0 && (
                                  <div className="text-sm">
                                    <Package className="h-3 w-3 inline mr-1" />
                                    <span className="text-muted-foreground">Orders:</span>
                                    <span className="text-xs ml-1">{visit.OrderCount} (₹{visit.OrderValue?.toLocaleString() || 0})</span>
                                  </div>
                                )}
                              </div>
                            </div>

                            {/* Additional Info */}
                            <div className="flex items-center gap-4 text-xs text-muted-foreground">
                              {visit.ContactNumber && (
                                <span>
                                  <Phone className="h-3 w-3 inline mr-1" />
                                  {visit.ContactNumber}
                                </span>
                              )}
                              {visit.EmailId && (
                                <span>
                                  <Mail className="h-3 w-3 inline mr-1" />
                                  {visit.EmailId}
                                </span>
                              )}
                              {visit.TravelDistance && (
                                <span>Distance: {visit.TravelDistance} km</span>
                              )}
                              {visit.ImageCount > 0 && (
                                <span>Images: {visit.ImageCount}</span>
                              )}
                              {visit.ActivityCount > 0 && (
                                <span>Activities: {visit.ActivityCount}</span>
                              )}
                            </div>

                            {/* Skip Reason or Notes */}
                            {(visit.SkipReason || visit.Notes) && (
                              <div className="mt-3 p-2 bg-muted rounded text-sm">
                                {visit.SkipReason && (
                                  <div>
                                    <span className="font-medium">Skip Reason:</span> {visit.SkipReason}
                                  </div>
                                )}
                                {visit.Notes && (
                                  <div>
                                    <span className="font-medium">Notes:</span> {visit.Notes}
                                  </div>
                                )}
                              </div>
                            )}
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Timeline Tab */}
        <TabsContent value="timeline" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Journey Timeline</CardTitle>
              <CardDescription>
                Chronological view of all activities during the journey
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {/* Journey Start */}
                {beatHistory.StartTime && (
                  <div className="flex gap-4">
                    <div className="flex flex-col items-center">
                      <div className="flex items-center justify-center h-8 w-8 rounded-full bg-green-100 text-green-600">
                        <Play className="h-4 w-4" />
                      </div>
                      <div className="w-px h-full bg-border" />
                    </div>
                    <div className="flex-1 pb-4">
                      <div className="font-medium">Journey Started</div>
                      <div className="text-sm text-muted-foreground">
                        {moment(beatHistory.StartTime).format('HH:mm:ss')}
                      </div>
                    </div>
                  </div>
                )}

                {/* Store Visits */}
                {storeVisits.map((visit, index) => (
                  <div key={visit.UID} className="flex gap-4">
                    <div className="flex flex-col items-center">
                      <div className="flex items-center justify-center h-8 w-8 rounded-full bg-primary/10 text-primary">
                        {visit.SerialNo}
                      </div>
                      {index < storeVisits.length - 1 && <div className="w-px h-full bg-border" />}
                    </div>
                    <div className="flex-1 pb-4">
                      <div className="font-medium">{visit.StoreName}</div>
                      <div className="text-sm text-muted-foreground">
                        {visit.LoginTime && moment(visit.LoginTime).format('HH:mm')} - 
                        {visit.LogoutTime && moment(visit.LogoutTime).format('HH:mm')}
                      </div>
                      <div className="text-xs text-muted-foreground mt-1">
                        {visit.IsProductive ? 'Productive' : 'Non-productive'} • 
                        Duration: {visit.VisitDuration} mins
                      </div>
                    </div>
                  </div>
                ))}

                {/* Journey End */}
                {beatHistory.EndTime && (
                  <div className="flex gap-4">
                    <div className="flex flex-col items-center">
                      <div className="flex items-center justify-center h-8 w-8 rounded-full bg-blue-100 text-blue-600">
                        <CheckCircle className="h-4 w-4" />
                      </div>
                    </div>
                    <div className="flex-1">
                      <div className="font-medium">Journey Completed</div>
                      <div className="text-sm text-muted-foreground">
                        {moment(beatHistory.EndTime).format('HH:mm:ss')}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Analytics Tab */}
        <TabsContent value="analytics" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Performance Metrics</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div>
                    <div className="flex justify-between mb-2">
                      <span className="text-sm">Visit Completion</span>
                      <span className="text-sm font-medium">{stats.coverage}%</span>
                    </div>
                    <Progress value={stats.coverage} />
                  </div>
                  <div>
                    <div className="flex justify-between mb-2">
                      <span className="text-sm">Productivity Rate</span>
                      <span className="text-sm font-medium">{stats.productivity}%</span>
                    </div>
                    <Progress value={stats.productivity} />
                  </div>
                  <div>
                    <div className="flex justify-between mb-2">
                      <span className="text-sm">Target Achievement</span>
                      <span className="text-sm font-medium">{stats.achievement}%</span>
                    </div>
                    <Progress value={stats.achievement} />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Visit Statistics</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableBody>
                    <TableRow>
                      <TableCell>Total Planned</TableCell>
                      <TableCell className="text-right font-medium">{stats.plannedVisits}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Completed</TableCell>
                      <TableCell className="text-right font-medium text-green-600">{stats.actualVisits}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Skipped</TableCell>
                      <TableCell className="text-right font-medium text-red-600">{stats.skippedVisits}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Unplanned</TableCell>
                      <TableCell className="text-right font-medium text-blue-600">{beatHistory.UnplannedStoreVisits}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Zero Sales</TableCell>
                      <TableCell className="text-right font-medium">{beatHistory.ZeroSalesStoreVisits}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>MSL Visits</TableCell>
                      <TableCell className="text-right font-medium">{beatHistory.MSLStoreVisits}</TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Financial Summary</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableBody>
                    <TableRow>
                      <TableCell>Target Value</TableCell>
                      <TableCell className="text-right font-medium">₹{stats.totalTargetValue.toLocaleString()}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Actual Value</TableCell>
                      <TableCell className="text-right font-medium">₹{stats.totalActualValue.toLocaleString()}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Order Value</TableCell>
                      <TableCell className="text-right font-medium">₹{stats.totalOrderValue.toLocaleString()}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Achievement</TableCell>
                      <TableCell className="text-right font-medium">{stats.achievement}%</TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Time Analysis</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableBody>
                    <TableRow>
                      <TableCell>Journey Start</TableCell>
                      <TableCell className="text-right">{beatHistory.StartTime ? moment(beatHistory.StartTime).format('HH:mm:ss') : '--:--:--'}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Journey End</TableCell>
                      <TableCell className="text-right">{beatHistory.EndTime ? moment(beatHistory.EndTime).format('HH:mm:ss') : '--:--:--'}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Total Duration</TableCell>
                      <TableCell className="text-right">
                        {beatHistory.StartTime && beatHistory.EndTime 
                          ? moment.duration(moment(beatHistory.EndTime).diff(moment(beatHistory.StartTime))).humanize()
                          : 'N/A'}
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Avg Visit Time</TableCell>
                      <TableCell className="text-right">{Math.round(stats.avgVisitDuration)} mins</TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default JourneyPlanDetailView;