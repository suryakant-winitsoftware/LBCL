"use client";

import React, { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Calendar } from "@/components/ui/calendar";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useToast } from "@/components/ui/use-toast";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import { 
  Calendar as CalendarIcon,
  Users,
  MapPin,
  Clock,
  CheckCircle,
  AlertCircle,
  RefreshCw,
  UserCheck,
  Info,
  Car,
  Route,
  TrendingUp
} from "lucide-react";
import { journeyPlanService } from "@/services/journeyPlanService";
import { authService } from "@/lib/auth-service";
import moment from "moment";
import { cn } from "@/lib/utils";
import { useRouter } from "next/navigation";

interface JourneyPlan {
  beatHistoryUID: string;
  visitDate: string;
  salesmanName: string;
  salesmanLoginId: string;
  routeUID: string;
  routeName: string;
  vehicleName?: string;
  empUID: string;
  scheduleCall: number;
  actualStoreVisits: number;
  skippedStore: number;
  pendingVisits: number;
  status?: string;
}

export default function ViewJourneyPlansPage() {
  const { toast } = useToast();
  const router = useRouter();
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [assignedPlans, setAssignedPlans] = useState<JourneyPlan[]>([]);
  const [unassignedPlans, setUnassignedPlans] = useState<JourneyPlan[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState<JourneyPlan | null>(null);
  const [showReassignDialog, setShowReassignDialog] = useState(false);
  const currentUser = authService.getCurrentUser();

  useEffect(() => {
    loadJourneyPlans();
  }, [selectedDate]);

  const loadJourneyPlans = async () => {
    setLoading(true);
    try {
      // Load both assigned and unassigned plans for the selected date
      const [assignedResponse, unassignedResponse] = await Promise.all([
        journeyPlanService.getTodayJourneyPlanDetails({
          visitDate: selectedDate,
          type: "Assigned",
          orgUID: currentUser?.orgUID || "",
          pageNumber: 1,
          pageSize: 100,
        }),
        journeyPlanService.getTodayJourneyPlanDetails({
          visitDate: selectedDate,
          type: "UnAssigned",
          orgUID: currentUser?.orgUID || "",
          pageNumber: 1,
          pageSize: 100,
        })
      ]);

      // Handle different response formats
      const assignedData = assignedResponse?.pagedData || assignedResponse?.data?.pagedData || [];
      const unassignedData = unassignedResponse?.pagedData || unassignedResponse?.data?.pagedData || [];
      
      setAssignedPlans(Array.isArray(assignedData) ? assignedData : []);
      setUnassignedPlans(Array.isArray(unassignedData) ? unassignedData : []);
    } catch (error) {
      console.error("Error loading journey plans:", error);
      toast({
        title: "Error",
        description: "Failed to load journey plans. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const handleReassign = async (beatHistoryUID: string, newEmpUID: string, newJobPositionUID: string) => {
    try {
      // Import the fixed service if not already imported
      const { journeyPlanServiceFixed } = await import('@/services/journeyPlanService.fixed');
      
      await journeyPlanServiceFixed.reassignJourneyPlan({
        beatHistoryUID,
        newEmpUID,
        newJobPositionUID,
        reassignedBy: currentUser?.uid || currentUser?.email || 'ADMIN',
        reason: 'Manual reassignment from web interface'
      });
      
      toast({
        title: "Success",
        description: "Journey plan reassigned successfully",
      });
      
      setShowReassignDialog(false);
      loadJourneyPlans();
    } catch (error: any) {
      console.error('Error reassigning journey plan:', error);
      toast({
        title: "Error",
        description: error?.message || "Failed to reassign journey plan",
        variant: "destructive"
      });
    }
  };

  const handleCancel = async (beatHistoryUID: string) => {
    if (!confirm("Are you sure you want to cancel this journey plan?")) return;
    
    try {
      // Import the fixed service if not already imported
      const { journeyPlanServiceFixed } = await import('@/services/journeyPlanService.fixed');
      
      await journeyPlanServiceFixed.cancelJourneyPlan({
        beatHistoryUID,
        cancelledBy: currentUser?.uid || currentUser?.email || 'ADMIN',
        reason: 'Cancelled from web interface'
      });
      
      toast({
        title: "Success",
        description: "Journey plan cancelled successfully",
      });
      
      loadJourneyPlans();
    } catch (error: any) {
      console.error('Error cancelling journey plan:', error);
      toast({
        title: "Error",
        description: error?.message || "Failed to cancel journey plan",
        variant: "destructive"
      });
    }
  };

  const JourneyPlanCard = ({ plan }: { plan: JourneyPlan }) => {
    const progress = plan.scheduleCall > 0 
      ? Math.round((plan.actualStoreVisits / plan.scheduleCall) * 100) 
      : 0;

    return (
      <Card className="hover:shadow-lg transition-shadow">
        <CardContent className="p-4">
          <div className="flex justify-between items-start mb-3">
            <div className="flex-1">
              <h4 className="font-semibold text-lg flex items-center gap-2">
                <Route className="h-4 w-4" />
                {plan.routeName || "Route Name"}
              </h4>
              <p className="text-sm text-gray-600 mt-1">
                Beat ID: {plan.beatHistoryUID?.split('_').slice(0, -3).join('_') || "N/A"}
              </p>
            </div>
            <Badge variant={progress === 100 ? "success" : progress > 0 ? "warning" : "default"}>
              {progress}% Complete
            </Badge>
          </div>

          <div className="space-y-2">
            <div className="flex items-center gap-2 text-sm">
              <Users className="h-4 w-4 text-gray-500" />
              <span className="font-medium">{plan.salesmanName || "Unassigned"}</span>
              {plan.salesmanLoginId && (
                <span className="text-gray-500">({plan.salesmanLoginId})</span>
              )}
            </div>

            <div className="flex items-center gap-2 text-sm">
              <MapPin className="h-4 w-4 text-gray-500" />
              <span>{plan.scheduleCall || 0} stores scheduled</span>
            </div>

            <div className="flex items-center gap-4 text-sm">
              <span className="flex items-center gap-1">
                <CheckCircle className="h-4 w-4 text-green-500" />
                {plan.actualStoreVisits || 0} visited
              </span>
              <span className="flex items-center gap-1 text-orange-500">
                <Clock className="h-4 w-4" />
                {plan.pendingVisits || plan.scheduleCall} pending
              </span>
              {plan.skippedStore > 0 && (
                <span className="flex items-center gap-1 text-red-500">
                  <AlertCircle className="h-4 w-4" />
                  {plan.skippedStore} skipped
                </span>
              )}
            </div>

            {plan.vehicleName && (
              <div className="flex items-center gap-2 text-sm">
                <Car className="h-4 w-4 text-gray-500" />
                <span>{plan.vehicleName}</span>
              </div>
            )}

            {/* Progress Bar */}
            <div className="w-full bg-gray-200 rounded-full h-2 mt-2">
              <div 
                className={cn(
                  "h-2 rounded-full transition-all",
                  progress === 100 ? "bg-green-500" : 
                  progress > 0 ? "bg-blue-500" : "bg-gray-300"
                )}
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>

          <div className="flex gap-2 mt-4">
            <Button
              variant="outline"
              size="sm"
              className="flex-1"
              onClick={() => router.push(`/updatedfeatures/journey-plan-management/journey-plans/view/${plan.beatHistoryUID}`)}
            >
              View Details
            </Button>
            {plan.actualStoreVisits === 0 && (
              <>
                <Button
                  variant="outline"
                  size="sm"
                  className="flex-1"
                  onClick={() => {
                    setSelectedPlan(plan);
                    setShowReassignDialog(true);
                  }}
                >
                  <UserCheck className="h-4 w-4 mr-1" />
                  Reassign
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleCancel(plan.beatHistoryUID)}
                >
                  <AlertCircle className="h-4 w-4" />
                </Button>
              </>
            )}
          </div>
        </CardContent>
      </Card>
    );
  };

  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">Journey Plans</h1>
          <p className="text-gray-600 mt-1">
            View and manage auto-generated journey plans
          </p>
        </div>
        <Button onClick={loadJourneyPlans} disabled={loading}>
          <RefreshCw className={cn("h-4 w-4 mr-2", loading && "animate-spin")} />
          Refresh
        </Button>
      </div>

      {/* Info Alert */}
      <Card className="mb-6 bg-blue-50 border-blue-200">
        <CardContent className="p-4">
          <div className="flex items-start gap-3">
            <Info className="h-5 w-5 text-blue-600 mt-0.5" />
            <div>
              <h4 className="font-semibold text-blue-900 mb-1">How Journey Plans Work</h4>
              <p className="text-sm text-blue-800">
                Journey plans are automatically generated every night at 11 PM based on active routes and their schedules. 
                The system creates plans for the next 15 days, excluding holidays and weekends. 
                You can view, reassign, or cancel plans, but cannot create them manually.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Calendar Sidebar */}
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <CalendarIcon className="h-5 w-5" />
              Select Date
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Calendar
              mode="single"
              selected={selectedDate}
              onSelect={(date) => date && setSelectedDate(date)}
              className="rounded-md"
            />
            <div className="mt-4 space-y-2">
              <Button 
                variant="outline" 
                className="w-full"
                onClick={() => setSelectedDate(new Date())}
              >
                Today
              </Button>
              <Button 
                variant="outline" 
                className="w-full"
                onClick={() => setSelectedDate(moment().add(1, 'day').toDate())}
              >
                Tomorrow
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Journey Plans List */}
        <div className="lg:col-span-3">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                <span>Plans for {moment(selectedDate).format("MMMM DD, YYYY")}</span>
                <Badge variant="outline">
                  {moment(selectedDate).format("dddd")}
                </Badge>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <Tabs defaultValue="assigned">
                <TabsList className="grid w-full grid-cols-2">
                  <TabsTrigger value="assigned">
                    Assigned ({assignedPlans.length})
                  </TabsTrigger>
                  <TabsTrigger value="unassigned">
                    Unassigned ({unassignedPlans.length})
                  </TabsTrigger>
                </TabsList>

                <TabsContent value="assigned" className="mt-4">
                  <ScrollArea className="h-[600px]">
                    {loading ? (
                      <div className="space-y-4">
                        {[1, 2, 3].map((i) => (
                          <Skeleton key={i} className="h-40" />
                        ))}
                      </div>
                    ) : assignedPlans.length > 0 ? (
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {assignedPlans.map((plan) => (
                          <JourneyPlanCard key={plan.beatHistoryUID} plan={plan} />
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-12">
                        <CalendarIcon className="h-12 w-12 mx-auto mb-3 text-gray-300" />
                        <p className="text-gray-500">No assigned journey plans for this date.</p>
                        <p className="text-sm text-gray-400 mt-2">
                          Plans are auto-generated based on route schedules.
                        </p>
                      </div>
                    )}
                  </ScrollArea>
                </TabsContent>

                <TabsContent value="unassigned" className="mt-4">
                  <ScrollArea className="h-[600px]">
                    {unassignedPlans.length > 0 ? (
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {unassignedPlans.map((plan) => (
                          <JourneyPlanCard key={plan.beatHistoryUID} plan={plan} />
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-12">
                        <Users className="h-12 w-12 mx-auto mb-3 text-gray-300" />
                        <p className="text-gray-500">No unassigned journey plans.</p>
                        <p className="text-sm text-gray-400 mt-2">
                          All routes have been assigned to salespeople.
                        </p>
                      </div>
                    )}
                  </ScrollArea>
                </TabsContent>
              </Tabs>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Plans</p>
                <p className="text-2xl font-bold">{assignedPlans.length + unassignedPlans.length}</p>
              </div>
              <TrendingUp className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Stores</p>
                <p className="text-2xl font-bold">
                  {assignedPlans.reduce((sum, p) => sum + p.scheduleCall, 0)}
                </p>
              </div>
              <MapPin className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Visited</p>
                <p className="text-2xl font-bold">
                  {assignedPlans.reduce((sum, p) => sum + p.actualStoreVisits, 0)}
                </p>
              </div>
              <CheckCircle className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Completion</p>
                <p className="text-2xl font-bold">
                  {assignedPlans.length > 0 
                    ? Math.round(
                        (assignedPlans.reduce((sum, p) => sum + p.actualStoreVisits, 0) / 
                         assignedPlans.reduce((sum, p) => sum + p.scheduleCall, 0)) * 100
                      ) 
                    : 0}%
                </p>
              </div>
              <TrendingUp className="h-8 w-8 text-orange-500" />
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}