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
import {
  ArrowLeft,
  Calendar,
  Clock,
  MapPin,
  Users,
  Route,
  AlertCircle,
  Edit,
} from 'lucide-react';
import moment from 'moment';
import { api } from '@/services/api';
import { authService } from '@/lib/auth-service';

interface JourneyPlanDetail {
  UID: string;
  RouteUID: string;
  RouteName: string;
  RouteCode: string;
  OrgUID: string;
  OrgName: string;
  JobPositionUID: string;
  JobPositionName: string;
  EmpUID: string;
  EmployeeName: string;
  LoginId: string;
  VisitDate: string;
  YearMonth: number;
  PlannedStoreVisits: number;
  ActualStoreVisits: number;
  SkippedStoreVisits: number;
  Coverage: number;
  ACoverage: number;
  TCoverage: number;
  Status: string;
  InvoiceStatus: string;
  StartTime?: string;
  EndTime?: string;
  PlannedStartTime?: string;
  PlannedEndTime?: string;
  Notes?: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
}

interface StoreVisit {
  UID: string;
  StoreUID: string;
  StoreName: string;
  StoreCode: string;
  Address: string;
  SerialNo: number;
  Status: string;
  IsPlanned: boolean;
  PlannedLoginTime?: string;
  PlannedLogoutTime?: string;
  LoginTime?: string;
  LogoutTime?: string;
  VisitDuration: number;
  TravelTime: number;
  TargetValue: number;
  ActualValue: number;
  IsProductive: boolean;
  IsSkipped: boolean;
}

const JourneyPlanDetailView: React.FC = () => {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const journeyPlanId = params.id as string;

  const [loading, setLoading] = useState(true);
  const [journeyPlan, setJourneyPlan] = useState<JourneyPlanDetail | null>(null);
  const [storeVisits, setStoreVisits] = useState<StoreVisit[]>([]);

  useEffect(() => {
    if (journeyPlanId) {
      loadJourneyPlanDetails();
    }
  }, [journeyPlanId]);

  const loadJourneyPlanDetails = async () => {
    try {
      setLoading(true);
      
      // Load journey plan details
      const beatHistoryData = await api.beatHistory.selectAll({
        pageNumber: 0,
        pageSize: 1,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: [{
          filterBy: 'UID',
          filterValue: journeyPlanId,
          filterOperator: 'equals'
        }]
      });

      if (beatHistoryData.IsSuccess && beatHistoryData.Data?.PagedData?.length > 0) {
        const plan = beatHistoryData.Data.PagedData[0];
        setJourneyPlan({
          UID: plan.UID,
          RouteUID: plan.RouteUID,
          RouteName: plan.RouteName || 'Unknown Route',
          RouteCode: plan.RouteCode || '',
          OrgUID: plan.OrgUID,
          OrgName: plan.OrgName || 'Unknown Organization',
          JobPositionUID: plan.JobPositionUID,
          JobPositionName: plan.JobPositionName || 'Unknown Position',
          EmpUID: plan.EmpUID || plan.LoginId,
          EmployeeName: plan.EmployeeName || plan.LoginId,
          LoginId: plan.LoginId,
          VisitDate: plan.VisitDate,
          YearMonth: plan.YearMonth,
          PlannedStoreVisits: plan.PlannedStoreVisits || 0,
          ActualStoreVisits: plan.ActualStoreVisits || 0,
          SkippedStoreVisits: plan.SkippedStoreVisits || 0,
          Coverage: plan.Coverage || 0,
          ACoverage: plan.ACoverage || 0,
          TCoverage: plan.TCoverage || 0,
          Status: plan.Status || 'Pending',
          InvoiceStatus: plan.InvoiceStatus || 'Pending',
          StartTime: plan.StartTime,
          EndTime: plan.EndTime,
          PlannedStartTime: plan.PlannedStartTime,
          PlannedEndTime: plan.PlannedEndTime,
          Notes: plan.Notes,
          CreatedBy: plan.CreatedBy,
          CreatedTime: plan.CreatedTime,
          ModifiedBy: plan.ModifiedBy,
          ModifiedTime: plan.ModifiedTime,
        });

        // Load store visits for this journey plan
        await loadStoreVisits(journeyPlanId, plan);
      } else {
        toast({
          title: "Error",
          description: "Journey plan not found",
          variant: "destructive",
        });
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

  const loadStoreVisits = async (beatHistoryUID: string, planData?: any) => {
    try {
      console.log('Loading real store visits for:', beatHistoryUID);
      
      // Get planned store count and start time from plan data
      const plannedStoreCount = planData?.PlannedStoreVisits || journeyPlan?.PlannedStoreVisits || 0;
      const plannedStartTime = planData?.PlannedStartTime || journeyPlan?.PlannedStartTime || '09:00';
      
      // Primary approach: Load real customers from the route
      const routeUID = planData?.RouteUID || journeyPlan?.RouteUID;
      
      if (!routeUID) {
        console.log('No route UID available to load customers');
        setStoreVisits([]);
        return;
      }

      try {
        console.log('Loading real customers for route:', routeUID);
        const response = await api.dropdown.getCustomersByRoute(routeUID);
        
        // Handle response format
        let customers = [];
        if (response?.IsSuccess && response?.Data) {
          customers = response.Data;
        } else if (Array.isArray(response)) {
          customers = response;
        }
        
        if (customers.length === 0) {
          console.log('No customers found for this route');
          setStoreVisits([]);
          return;
        }
        
        console.log(`Found ${customers.length} real customers for route ${routeUID}`);
        
        // Use actual customers - limit to planned count or first 4
        const storesForVisit = plannedStoreCount > 0 
          ? customers.slice(0, plannedStoreCount)
          : customers.slice(0, Math.min(4, customers.length));
        
        const transformedVisits = storesForVisit.map((customer: any, index: number) => {
          // Extract real store information
          const storeUID = customer.UID || customer.Value || '';
          const storeLabel = customer.Label || customer.Text || customer.Name || '';
          const storeCode = customer.Code || storeUID;
          
          return {
            UID: `SH_${beatHistoryUID}_${storeUID}_${index}`,
            StoreUID: storeUID,
            StoreName: storeLabel,
            StoreCode: storeCode,
            Address: customer.Address || '',
            SerialNo: index + 1,
            Status: 'Planned',
            IsPlanned: true,
            PlannedLoginTime: moment(plannedStartTime, 'HH:mm')
              .add(index * 45, 'minutes')
              .format('HH:mm'),
            PlannedLogoutTime: moment(plannedStartTime, 'HH:mm')
              .add(index * 45 + 30, 'minutes')
              .format('HH:mm'),
            LoginTime: null,
            LogoutTime: null,
            VisitDuration: 30,
            TravelTime: 15,
            TargetValue: 0,
            ActualValue: 0,
            IsProductive: false,
            IsSkipped: false,
          };
        });
        
        setStoreVisits(transformedVisits);
        console.log(`Successfully loaded ${transformedVisits.length} real store visits`);
        
      } catch (error) {
        console.error('Error loading customers:', error);
        setStoreVisits([]);
      }
      
      
    } catch (error) {
      console.error('Error loading store visits:', error);
      // Set empty array to prevent UI errors
      setStoreVisits([]);
    }
  };

  const handleBack = () => {
    router.push('/updatedfeatures/journey-plan-management/journey-plans/manage');
  };

  const handleEdit = () => {
    router.push(`/updatedfeatures/journey-plan-management/journey-plans/edit/${journeyPlanId}`);
  };


  const getStatusVariant = (status: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (status?.toLowerCase()) {
      case 'completed': return 'default';
      case 'in_progress': case 'started': return 'secondary';
      case 'pending': return 'outline';
      case 'cancelled': case 'skipped': return 'destructive';
      default: return 'outline';
    }
  };

  const getStatusColor = (status: string): string => {
    switch (status?.toLowerCase()) {
      case 'completed': return 'text-green-600';
      case 'in_progress': case 'started': return 'text-blue-600';
      case 'pending': return 'text-yellow-600';
      case 'cancelled': case 'skipped': return 'text-red-600';
      default: return 'text-gray-600';
    }
  };

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex items-center justify-between">
          <div className="space-y-2">
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-96" />
          </div>
          <Skeleton className="h-10 w-32" />
        </div>
        
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 space-y-6">
            <Skeleton className="h-96 w-full" />
            <Skeleton className="h-64 w-full" />
          </div>
          <div className="space-y-6">
            <Skeleton className="h-48 w-full" />
            <Skeleton className="h-32 w-full" />
          </div>
        </div>
      </div>
    );
  }

  if (!journeyPlan) {
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
          <h1 className="text-2xl font-bold">Journey Plan Assignment</h1>
          <p className="text-muted-foreground">
            View and manage journey plan assignment for {journeyPlan.EmployeeName} on {moment(journeyPlan.VisitDate).format('DD MMMM YYYY')}
          </p>
        </div>
        <div className="flex gap-2">
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

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Journey Plan Overview */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-2">
                  <Route className="h-5 w-5" />
                  Assignment Details
                </CardTitle>
                <Badge variant={journeyPlan.Status === 'Pending' ? 'secondary' : 'default'}>
                  {journeyPlan.Status === 'Pending' ? 'Assigned' : journeyPlan.Status}
                </Badge>
              </div>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Route</label>
                    <div className="flex items-center gap-2 mt-1">
                      <Route className="h-4 w-4" />
                      <span className="font-medium">{journeyPlan.RouteName}</span>
                      <span className="text-sm text-muted-foreground">({journeyPlan.RouteCode})</span>
                    </div>
                  </div>
                  
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Employee</label>
                    <div className="flex items-center gap-2 mt-1">
                      <Users className="h-4 w-4" />
                      <span className="font-medium">{journeyPlan.EmployeeName}</span>
                      <span className="text-sm text-muted-foreground">({journeyPlan.LoginId})</span>
                    </div>
                  </div>

                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Visit Date</label>
                    <div className="flex items-center gap-2 mt-1">
                      <Calendar className="h-4 w-4" />
                      <span className="font-medium">
                        {moment(journeyPlan.VisitDate).format('DD MMMM YYYY')}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="space-y-4">
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Planned Time</label>
                    <div className="flex items-center gap-2 mt-1">
                      <Clock className="h-4 w-4" />
                      <span>
                        {journeyPlan.PlannedStartTime || '09:00'} - {journeyPlan.PlannedEndTime || '18:00'}
                      </span>
                    </div>
                  </div>

                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Stores to Visit</label>
                    <div className="flex items-center gap-2 mt-1">
                      <MapPin className="h-4 w-4" />
                      <span className="font-medium">
                        {journeyPlan.PlannedStoreVisits} stores
                      </span>
                    </div>
                  </div>

                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Assignment Status</label>
                    <div className="mt-1">
                      <Badge variant={journeyPlan.Status === 'Pending' ? 'secondary' : 'default'}>
                        {journeyPlan.Status === 'Pending' ? 'Assigned' : journeyPlan.Status}
                      </Badge>
                    </div>
                  </div>
                </div>
              </div>

              {/* Additional Details */}
              <Separator className="my-4" />
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Organization</label>
                  <div className="mt-1">
                    <span className="font-medium">{journeyPlan.OrgName || journeyPlan.OrgUID}</span>
                  </div>
                </div>
                
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Job Position</label>
                  <div className="mt-1">
                    <span className="font-medium">{journeyPlan.JobPositionName || journeyPlan.JobPositionUID}</span>
                  </div>
                </div>
                
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Year/Month</label>
                  <div className="mt-1">
                    <span className="font-medium">{journeyPlan.YearMonth}</span>
                  </div>
                </div>
                
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Coverage Metrics</label>
                  <div className="mt-1 text-sm space-y-1">
                    <div>Total: {journeyPlan.Coverage}%</div>
                    <div>Active: {journeyPlan.ACoverage}%</div>
                    <div>Target: {journeyPlan.TCoverage}%</div>
                  </div>
                </div>
              </div>

              {journeyPlan.Notes && (
                <>
                  <Separator className="my-4" />
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Additional Information</label>
                    <div className="mt-2 p-3 bg-muted/50 rounded-lg">
                      {(() => {
                        try {
                          const notesData = JSON.parse(journeyPlan.Notes);
                          return (
                            <div className="space-y-2 text-sm">
                              {notesData.userNotes && (
                                <div>
                                  <span className="font-medium">User Notes:</span> {notesData.userNotes}
                                </div>
                              )}
                              {notesData.scheduleType && (
                                <div>
                                  <span className="font-medium">Schedule Type:</span> {notesData.scheduleType}
                                </div>
                              )}
                              {notesData.approvalStatus && (
                                <div>
                                  <span className="font-medium">Approval Status:</span> {notesData.approvalStatus}
                                </div>
                              )}
                              {notesData.specialInstructions && (
                                <div>
                                  <span className="font-medium">Special Instructions:</span> {notesData.specialInstructions}
                                </div>
                              )}
                              {notesData.emergencyContact && (
                                <div>
                                  <span className="font-medium">Emergency Contact:</span> {notesData.emergencyContact}
                                </div>
                              )}
                              {notesData.createdBy && (
                                <div>
                                  <span className="font-medium">Created By:</span> {notesData.createdBy} at {notesData.creationTimestamp ? moment(notesData.creationTimestamp).format('DD/MM/YYYY HH:mm') : ''}
                                </div>
                              )}
                            </div>
                          );
                        } catch (e) {
                          // If Notes is not JSON, display as plain text
                          return <p className="text-sm">{journeyPlan.Notes}</p>;
                        }
                      })()}
                    </div>
                  </div>
                </>
              )}
            </CardContent>
          </Card>

          {/* Planned Store Visits */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                Planned Store Visits ({storeVisits.length})
              </CardTitle>
              <CardDescription>
                Stores assigned from route {journeyPlan.RouteName} for {moment(journeyPlan.VisitDate).format('DD MMM YYYY')}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {storeVisits.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  No customer visits found for this journey plan
                </div>
              ) : (
                <div className="space-y-4">
                  {storeVisits.map((visit) => (
                    <div
                      key={visit.UID}
                      className="border rounded-lg p-4 hover:bg-muted/50 transition-colors"
                    >
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-2">
                            <span className="font-medium text-sm bg-primary/10 text-primary px-2 py-1 rounded">
                              #{visit.SerialNo}
                            </span>
                            <h4 className="font-semibold">{visit.StoreName}</h4>
                            <Badge variant={getStatusVariant(visit.Status)}>
                              {visit.Status}
                            </Badge>
                          </div>
                          
                          <div className="text-sm text-muted-foreground mb-2">
                            {visit.StoreCode && <span className="mr-4">Code: {visit.StoreCode}</span>}
                            {visit.Address && <span>{visit.Address}</span>}
                          </div>

                          <div className="grid grid-cols-2 gap-4 text-sm">
                            <div>
                              <span className="text-muted-foreground">Planned Time:</span>
                              <div className="font-medium">
                                {visit.PlannedLoginTime} - {visit.PlannedLogoutTime}
                              </div>
                            </div>
                            <div>
                              <span className="text-muted-foreground">Visit Duration:</span>
                              <div className="font-medium">{visit.VisitDuration} mins</div>
                            </div>
                            <div>
                              <span className="text-muted-foreground">Travel Time:</span>
                              <div className="font-medium">{visit.TravelTime} mins</div>
                            </div>
                            <div>
                              <span className="text-muted-foreground">Visit Type:</span>
                              <div className="font-medium">Regular Visit</div>
                            </div>
                          </div>
                        </div>

                        <div className="flex flex-col items-end gap-2">
                          <Badge variant="secondary" className="text-xs">
                            Planned
                          </Badge>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Planning Summary */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Planning Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Total Stores on Route</span>
                  <span className="font-semibold">{storeVisits.length || 0}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Planned to Visit</span>
                  <span className="font-semibold text-blue-600">{journeyPlan.PlannedStoreVisits}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Route Coverage</span>
                  <span className="font-semibold">{Math.round((journeyPlan.PlannedStoreVisits / (storeVisits.length || 1)) * 100)}%</span>
                </div>
                <Separator />
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Planned Duration</span>
                  <span className="font-semibold">
                    {journeyPlan.PlannedStartTime || '09:00'} - {journeyPlan.PlannedEndTime || '18:00'}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Assignment Status</span>
                  <Badge variant={journeyPlan.Status === 'Pending' ? 'secondary' : 'default'}>
                    {journeyPlan.Status === 'Pending' ? 'Assigned' : journeyPlan.Status}
                  </Badge>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Admin Actions */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Admin Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <Button
                className="w-full"
                onClick={handleEdit}
              >
                <Edit className="mr-2 h-4 w-4" />
                Edit Plan
              </Button>
              
              <Button
                variant="outline"
                className="w-full"
                onClick={() => {
                  // Copy journey plan for another date
                  router.push(`/updatedfeatures/journey-plan-management/journey-plans/create?copyFrom=${journeyPlanId}`);
                }}
              >
                <Calendar className="mr-2 h-4 w-4" />
                Copy for Another Date
              </Button>
              
              <Button
                variant="destructive"
                className="w-full"
                onClick={async () => {
                  if (confirm('Are you sure you want to delete this journey plan?')) {
                    try {
                      const response = await api.beatHistory.delete(journeyPlanId);
                      if (response.IsSuccess) {
                        toast({
                          title: "Success",
                          description: "Journey plan deleted successfully",
                        });
                        router.push('/updatedfeatures/journey-plan-management/journey-plans/manage');
                      }
                    } catch (error) {
                      toast({
                        title: "Error",
                        description: "Failed to delete journey plan",
                        variant: "destructive",
                      });
                    }
                  }
                }}
              >
                Delete Plan
              </Button>
            </CardContent>
          </Card>

          {/* Plan Info */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Plan Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div>
                <div className="text-muted-foreground">Created by</div>
                <div>{journeyPlan.CreatedBy}</div>
              </div>
              <div>
                <div className="text-muted-foreground">Created on</div>
                <div>{moment(journeyPlan.CreatedTime).format('DD/MM/YYYY HH:mm')}</div>
              </div>
              <div>
                <div className="text-muted-foreground">Last modified</div>
                <div>{moment(journeyPlan.ModifiedTime).format('DD/MM/YYYY HH:mm')}</div>
              </div>
              <div>
                <div className="text-muted-foreground">Journey ID</div>
                <div className="font-mono text-xs">{journeyPlan.UID}</div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};

export default JourneyPlanDetailView;