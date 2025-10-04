"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useRouter } from "next/navigation";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from "@/components/ui/dropdown-menu";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from "@/components/ui/alert-dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/components/ui/use-toast";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Plus,
  Search,
  RefreshCw,
  MoreHorizontal,
  Eye,
  Edit,
  PlayCircle,
  PauseCircle,
  CheckCircle,
  Clock,
  Calendar,
  Users,
  MapPin,
  Battery,
  Wifi,
  Navigation,
  Smartphone,
  Activity,
  Settings,
  Download
} from "lucide-react";
import moment from "moment";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import {
  journeyPlanService,
  UserJourney,
  AssignedJourneyPlan,
  PagedRequest
} from "@/services/journeyPlanService";
import { api } from "@/services/api";
import { JourneyPlanGrid } from "@/components/admin/journey-plan/journey-plan-grid";

interface FilterState {
  search: string;
  jobPositionUID: string;
  empUID: string;
  eotStatus: string;
  attendanceStatus: string;
  visitDate: Date | null;
  orgUID: string;
}

interface PaginationState {
  current: number;
  pageSize: number;
  total: number;
}

const JourneyPlanManagement: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  const [activeTab, setActiveTab] = useState("journeys");

  // State management
  const [assignedPlans, setAssignedPlans] = useState<AssignedJourneyPlan[]>([]);
  const [loading, setLoading] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteItemId, setDeleteItemId] = useState<string | null>(null);
  const [deleteItemType, setDeleteItemType] = useState<"journey" | "plan">(
    "journey"
  );
  const [startJourneyDialogOpen, setStartJourneyDialogOpen] = useState(false);
  const [startJourneyId, setStartJourneyId] = useState<string | null>(null);
  const [completeJourneyDialogOpen, setCompleteJourneyDialogOpen] =
    useState(false);
  const [completeJourneyId, setCompleteJourneyId] = useState<string | null>(
    null
  );
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const [pagination, setPagination] = useState<PaginationState>({
    current: 1,
    pageSize: 10,
    total: 0
  });

  const [filters, setFilters] = useState<FilterState>({
    search: "",
    jobPositionUID: "",
    empUID: "",
    eotStatus: "",
    attendanceStatus: "",
    visitDate: null, // Don't filter by date initially
    orgUID: "Farmley"
  });

  // Fetch data when tab changes
  useEffect(() => {
    if (activeTab === "plans") {
      // Set filter to today's date when switching to Today's Plans tab
      const today = new Date();
      setFilters(prev => ({ ...prev, visitDate: today }));
      fetchTodayJourneyPlans();
    }
  }, [activeTab]);
  
  // Fetch data when filters or pagination change (debounced)
  useEffect(() => {
    const timer = setTimeout(() => {
      if (activeTab === "plans") {
        fetchTodayJourneyPlans();
      }
    }, 300);
    
    return () => clearTimeout(timer);
  }, [filters, pagination.current, pagination.pageSize]);

  const fetchTodayJourneyPlans = async () => {
    setLoading(true);
    try {
      // Try different pagination parameters to work around backend issues
      const request: PagedRequest = {
        pageNumber: 0, // Try 0-based pagination
        pageSize: 1000, // Get more records
        isCountRequired: true, // Get count
        sortCriterias: [], // Remove sorting to avoid SQL syntax errors
        filterCriterias: [] // No backend filters to avoid errors
      };

      // Use BeatHistory endpoint as primary data source
      const beatHistoryData = await api.beatHistory.selectAll(request);
      
      console.log('================== API RESPONSE DATA ==================');
      console.log('1. Full Response:', beatHistoryData);
      console.log('2. Response Status:', {
        IsSuccess: (beatHistoryData as any)?.IsSuccess,
        StatusCode: (beatHistoryData as any)?.StatusCode,
        TotalCount: (beatHistoryData as any)?.Data?.TotalCount
      });
      console.log('======================================================');

      // Check different response formats
      const responseData = (beatHistoryData as any)?.Data?.PagedData ||
        (beatHistoryData as any)?.PagedData ||
        (beatHistoryData as any)?.data ||
        (beatHistoryData as any)?.Data ||
        [];
      
      console.log('================== EXTRACTED DATA ==================');
      console.log('3. Total Records:', responseData?.length || 0);
      
      if (responseData?.length > 0) {
        console.log('4. First Record Sample:', responseData[0]);
        console.log('5. Sample Record Fields:', {
          RouteUID: responseData[0].RouteUID,
          JobPositionUID: responseData[0].JobPositionUID,
          LoginId: responseData[0].LoginId,
          VisitDate: responseData[0].VisitDate,
          PlannedStoreVisits: responseData[0].PlannedStoreVisits,
          ActualStoreVisits: responseData[0].ActualStoreVisits,
          UID: responseData[0].UID
        });
        
        // Show date range
        const dates = responseData.map((r: any) => r.VisitDate).filter(Boolean);
        const uniqueDates = [...new Set(dates)].sort();
        console.log('6. Date Range:', {
          totalDates: uniqueDates.length,
          firstDate: uniqueDates[0],
          lastDate: uniqueDates[uniqueDates.length - 1],
          sampleDates: uniqueDates.slice(0, 5)
        });
        
        // Show routes/positions
        const routes = responseData.map((r: any) => r.RouteUID).filter(Boolean);
        const uniqueRoutes = [...new Set(routes)];
        console.log('7. Routes/Positions:', {
          totalUniqueRoutes: uniqueRoutes.length,
          routes: uniqueRoutes
        });
        
        // Show employees
        const employees = responseData.map((r: any) => r.LoginId).filter(Boolean);
        const uniqueEmployees = [...new Set(employees)];
        console.log('8. Employees (LoginIds):', {
          totalUniqueEmployees: uniqueEmployees.length,
          employees: uniqueEmployees
        });
      }
      console.log('===================================================');

      if ((beatHistoryData as any)?.IsSuccess || beatHistoryData) {
        console.log('Processing response data...');
        // For Today's Plans tab, always filter by the selected date (default to today)
        const shouldFilterByDate = activeTab === "plans";
        const selectedDate = filters.visitDate ? moment(filters.visitDate).format("YYYY-MM-DD") : moment().format("YYYY-MM-DD");
        
        console.log('================== FILTER SETTINGS ==================');
        console.log('9. Filter Configuration:', { 
          shouldFilterByDate, 
          selectedDate,
          totalRecords: responseData.length,
          activeTab,
          visitDateFilter: filters.visitDate,
          jobPositionFilter: filters.jobPositionUID,
          todayDate: moment().format("YYYY-MM-DD")
        });
        console.log('====================================================');
        
        const transformedPlans = responseData
          .filter((plan: any) => {
            // Check for visit_date (lowercase from database) or VisitDate (PascalCase from some APIs)
            const visitDate = plan.visit_date || plan.VisitDate || plan.visitDate;
            
            // For Today's Plans tab, always filter by date
            if (shouldFilterByDate) {
              if (visitDate) {
                const planDate = moment(visitDate).format("YYYY-MM-DD");
                if (planDate !== selectedDate) {
                  return false;
                }
              } else {
                return false; // Skip records without dates
              }
            }

            // Filter by JobPositionUID if provided - check both formats
            const jobPositionUID = plan.job_position_uid || plan.JobPositionUID;
            if (
              filters.jobPositionUID &&
              jobPositionUID !== filters.jobPositionUID
            ) {
              return false;
            }

            return true;
          })
          .map((plan: any) => {
            // Map fields from both lowercase (database) and PascalCase (API) formats
            const uid = plan.uid || plan.UID;
            const routeUID = plan.route_uid || plan.RouteUID;
            const orgUID = plan.org_uid || plan.OrgUID || "Farmley";
            const jobPositionUID = plan.job_position_uid || plan.JobPositionUID || "-";
            const loginId = plan.login_id || plan.LoginId || plan.LoginID || plan.loginId;
            const empUID = loginId || plan.emp_uid || plan.EmpUID || plan.empUID || "-";
            const visitDate = plan.visit_date || plan.VisitDate || plan.visitDate;
            const plannedStoreVisits = plan.planned_store_visits || plan.PlannedStoreVisits || 0;
            const actualStoreVisits = plan.actual_store_visits || plan.ActualStoreVisits || 0;
            const skippedStoreVisits = plan.skipped_store_visits || plan.SkippedStoreVisits || 0;
            const unplannedStoreVisits = plan.unplanned_store_visits || plan.UnPlannedStoreVisits || plan.UnplannedStoreVisits || 0;
            const coverage = plan.coverage || plan.Coverage || 0;
            const invoiceStatus = plan.invoice_status || plan.InvoiceStatus || "Pending";
            const hasAuditCompleted = plan.has_audit_completed || plan.HasAuditCompleted;
            
            // Determine status based on InvoiceStatus and other fields
            let status = "Pending";
            if (invoiceStatus === "Completed" || hasAuditCompleted) {
              status = "Completed";
            } else if (
              invoiceStatus === "In Progress" ||
              (actualStoreVisits > 0 && actualStoreVisits < plannedStoreVisits)
            ) {
              status = "In Progress";
            }

            // Clean up route name - remove underscores and format properly
            let routeName = "No Route";
            if (routeUID) {
              // If route UID has underscores, replace them with spaces
              routeName = routeUID.replace(/_/g, ' ');
              // If it doesn't start with "Route", add it
              if (!routeName.toLowerCase().startsWith('route')) {
                routeName = `Route: ${routeName}`;
              }
            }

            // Log to debug
            if (!visitDate) {
              console.error('❌ MISSING DATE DETECTED:', {
                uid: uid || 'no-uid',
                routeUID: routeUID || 'no-route',
                checkedFields: {
                  'visit_date': plan.visit_date,
                  'VisitDate': plan.VisitDate,
                  'visitDate': plan.visitDate
                },
                allFieldsInPlan: Object.keys(plan),
                rawPlan: plan
              });
            }
            
            return {
              UID: uid,
              RouteUID: routeUID,
              RouteName: routeName,
              OrgUID: orgUID,
              JobPositionUID: jobPositionUID,
              EmpUID: empUID,
              VisitDate: visitDate,
              PlannedStoreVisits: plannedStoreVisits,
              ActualStoreVisits: actualStoreVisits,
              SkippedStoreVisits: skippedStoreVisits,
              UnPlannedStoreVisits: unplannedStoreVisits,
              Coverage: coverage,
              Status: status,
              InvoiceStatus: invoiceStatus
            };
          });

        console.log('================== TRANSFORMED DATA ==================');
        console.log('10. Total Transformed Records:', transformedPlans.length);
        
        if (transformedPlans.length > 0) {
          console.log('11. First Transformed Record:', transformedPlans[0]);
          
          // Check for missing dates
          const missingDates = transformedPlans.filter((p: any) => !p.VisitDate);
          if (missingDates.length > 0) {
            console.error('12. ❌ Records with Missing Dates:', missingDates.length);
            console.error('Sample missing date record:', missingDates[0]);
          } else {
            console.log('12. ✅ All records have valid dates');
          }
          
          // Summary stats
          const stats = {
            totalPlans: transformedPlans.length,
            withDates: transformedPlans.filter((p: any) => p.VisitDate).length,
            withEmployees: transformedPlans.filter((p: any) => p.EmpUID !== '-').length,
            pendingStatus: transformedPlans.filter((p: any) => p.Status === 'Pending').length,
            completedStatus: transformedPlans.filter((p: any) => p.Status === 'Completed').length,
            withPlannedVisits: transformedPlans.filter((p: any) => p.PlannedStoreVisits > 0).length,
            withActualVisits: transformedPlans.filter((p: any) => p.ActualStoreVisits > 0).length
          };
          console.log('13. Transformation Statistics:', stats);
          
          // Show sample of first 3 transformed records
          console.log('14. First 3 Transformed Records:');
          transformedPlans.slice(0, 3).forEach((plan: any, index: number) => {
            console.log(`   Record ${index + 1}:`, {
              UID: plan.UID,
              VisitDate: plan.VisitDate,
              RouteUID: plan.RouteUID,
              EmpUID: plan.EmpUID,
              Status: plan.Status
            });
          });
        }
        console.log('======================================================');
        
        setAssignedPlans(transformedPlans);
        setPagination((prev) => ({
          ...prev,
          total: transformedPlans.length,
          current: 1 // Reset to first page when data changes
        }));

        // Don't show toast for empty results - it's normal
        console.log(`Found ${transformedPlans.length} journey plans`);
      } else {
        setAssignedPlans([]);
        setPagination((prev) => ({ ...prev, total: 0 }));
        toast({
          title: "No Data",
          description:
            "No journey plans found. Please ensure test data is inserted in the database.",
          variant: "default"
        });
      }
    } catch (error) {
      // Silent error - handled by toast
      setAssignedPlans([]);
      setPagination((prev) => ({ ...prev, total: 0 }));
      toast({
        title: "Error",
        description:
          "Failed to fetch journey plans. Check console for details.",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  // Helper functions for status colors

  const getPlanStatusVariant = (
    status: string
  ): "default" | "secondary" | "destructive" | "outline" => {
    switch (status?.toLowerCase()) {
      case "completed":
        return "default";
      case "in_progress":
        return "secondary";
      case "pending":
        return "outline";
      case "cancelled":
        return "destructive";
      default:
        return "outline";
    }
  };

  // Event handlers
  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, search: value }));
    setPagination((prev) => ({ ...prev, current: 1 }));
  };

  const handleFilterChange = (key: keyof FilterState, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPagination((prev) => ({ ...prev, current: 1 }));
  };

  const handleCreateJourneyPlan = () => {
    router.push(
      "/updatedfeatures/journey-plan-management/journey-plans/create"
    );
  };

  const handleViewJourney = (uid: string) => {
    router.push(
      `/updatedfeatures/journey-plan-management/journey-plans/view/${uid}`
    );
  };

  const handleEditJourney = (uid: string) => {
    router.push(
      `/updatedfeatures/journey-plan-management/journey-plans/edit/${uid}`
    );
  };

  const handleViewPlan = (uid: string) => {
    router.push(
      `/updatedfeatures/journey-plan-management/journey-plans/plan/${uid}`
    );
  };

  const handleDeleteClick = (uid: string, type: "journey" | "plan") => {
    setDeleteItemId(uid);
    setDeleteItemType(type);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteItemId) return;

    try {
      if (deleteItemType === "journey") {
        await journeyPlanService.deleteUserJourney(deleteItemId);
        toast({
          title: "Success",
          description: "Journey deleted successfully"
        });
        triggerRefresh();
      } else {
        await journeyPlanService.deleteJourneyPlan(deleteItemId);
        toast({
          title: "Success",
          description: "Journey plan deleted successfully"
        });
        fetchTodayJourneyPlans();
      }
    } catch (error) {
      toast({
        title: "Error",
        description: `Failed to delete ${deleteItemType}`,
        variant: "destructive"
      });
    } finally {
      setDeleteDialogOpen(false);
      setDeleteItemId(null);
    }
  };

  const handleStartJourneyClick = (uid: string) => {
    setStartJourneyId(uid);
    setStartJourneyDialogOpen(true);
  };

  const handleStartJourneyConfirm = async () => {
    if (!startJourneyId) return;

    try {
      await journeyPlanService.startJourneyPlan(startJourneyId);
      toast({
        title: "Success",
        description: "Journey plan started successfully"
      });
      fetchTodayJourneyPlans();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to start journey plan",
        variant: "destructive"
      });
    } finally {
      setStartJourneyDialogOpen(false);
      setStartJourneyId(null);
    }
  };

  const handleCompleteJourneyClick = (uid: string) => {
    setCompleteJourneyId(uid);
    setCompleteJourneyDialogOpen(true);
  };

  const handleCompleteJourneyConfirm = async () => {
    if (!completeJourneyId) return;

    try {
      await journeyPlanService.completeUserJourney(completeJourneyId);
      toast({
        title: "Success",
        description: "Journey completed successfully"
      });
      triggerRefresh();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to complete journey",
        variant: "destructive"
      });
    } finally {
      setCompleteJourneyDialogOpen(false);
      setCompleteJourneyId(null);
    }
  };

  const triggerRefresh = useCallback(() => {
    setRefreshTrigger((prev) => prev + 1);
  }, []);

  const handleRefresh = () => {
    if (activeTab === "journeys") {
      triggerRefresh();
    } else {
      fetchTodayJourneyPlans();
    }
  };

  // Skeleton loader for table rows
  const TableSkeleton = () => (
    <>
      {[...Array(5)].map((_, index) => (
        <TableRow key={index} className="hover:bg-transparent">
          <TableCell className="py-2">
            <Skeleton className="h-4 w-20" />
          </TableCell>
          <TableCell className="py-2">
            <Skeleton className="h-4 w-24" />
          </TableCell>
          <TableCell className="py-2">
            <Skeleton className="h-4 w-20" />
          </TableCell>
          <TableCell className="py-2">
            <Skeleton className="h-5 w-16 rounded-full" />
          </TableCell>
          <TableCell className="py-2 text-center">
            <Skeleton className="h-6 w-6 rounded mx-auto" />
          </TableCell>
        </TableRow>
      ))}
    </>
  );

  return (
    <div className="min-h-screen">
      {/* Modern Header with Gradient */}
      <div className="border-b sticky top-0 z-10 bg-white">
        <div className="container mx-auto px-4 py-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-100 rounded-lg">
                <Navigation className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <h1 className="text-2xl font-semibold text-gray-900">
                  Journey Plan Management
                </h1>
                <p className="text-sm text-muted-foreground">
                  Monitor and manage daily journey plans and user journeys
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() =>
                  router.push(
                    "/updatedfeatures/journey-plan-management/live-dashboard"
                  )
                }
                className=""
              >
                <Activity className="mr-2 h-4 w-4" />
                Live Dashboard
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() =>
                  router.push(
                    "/updatedfeatures/journey-plan-management/bulk-management"
                  )
                }
                className=""
              >
                <Settings className="mr-2 h-4 w-4" />
                Bulk Management
              </Button>
              <Button
                onClick={handleCreateJourneyPlan}
                size="sm"
                className="bg-blue-600 hover:bg-blue-700 text-white"
              >
                <Plus className="mr-2 h-4 w-4" />
                Create Journey Plan
              </Button>
            </div>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 py-6 space-y-6">
        {/* Main Content */}
        <Card className="border">
          <CardContent className="p-0">
            <Tabs value={activeTab} onValueChange={setActiveTab}>
              <div className="border-b px-6 pt-6">
                <TabsList className="grid w-full max-w-md grid-cols-2">
                  <TabsTrigger value="journeys" className="gap-2">
                    <Navigation className="h-4 w-4" />
                    User Journeys
                  </TabsTrigger>
                  <TabsTrigger value="plans" className="gap-2">
                    <Calendar className="h-4 w-4" />
                    Today&apos;s Plans
                  </TabsTrigger>
                </TabsList>
              </div>

              {/* User Journeys Tab */}
              <TabsContent value="journeys" className="mt-0">
                {/* Filters */}
                <div className="p-6 border-b">
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <div className="relative">
                      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        placeholder="Search by employee..."
                        value={filters.search}
                        onChange={(e) => handleSearch(e.target.value)}
                        className="pl-10"
                      />
                    </div>

                    <Select
                      value={filters.eotStatus || "all"}
                      onValueChange={(value) =>
                        handleFilterChange(
                          "eotStatus",
                          value === "all" ? "" : value
                        )
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="EOT Status" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="all">All Status</SelectItem>
                        <SelectItem value="Not Started">Not Started</SelectItem>
                        <SelectItem value="In Progress">In Progress</SelectItem>
                        <SelectItem value="Completed">Completed</SelectItem>
                        <SelectItem value="Paused">Paused</SelectItem>
                      </SelectContent>
                    </Select>

                    <Select
                      value={filters.attendanceStatus || "all"}
                      onValueChange={(value) =>
                        handleFilterChange(
                          "attendanceStatus",
                          value === "all" ? "" : value
                        )
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Attendance Status" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="all">All Status</SelectItem>
                        <SelectItem value="Present">Present</SelectItem>
                        <SelectItem value="Absent">Absent</SelectItem>
                        <SelectItem value="Late">Late</SelectItem>
                      </SelectContent>
                    </Select>

                    <Button
                      variant="outline"
                      size="sm"
                      onClick={async () => {
                        try {
                          const blob = await journeyPlanService.exportUserJourneys("csv", {
                            search: filters.search,
                            eotStatus: filters.eotStatus,
                            attendanceStatus: filters.attendanceStatus
                          });
                          const url = URL.createObjectURL(blob);
                          const a = document.createElement("a");
                          a.href = url;
                          a.download = `user-journeys-export-${new Date().toISOString().split("T")[0]}.csv`;
                          document.body.appendChild(a);
                          a.click();
                          document.body.removeChild(a);
                          URL.revokeObjectURL(url);
                          
                          toast({
                            title: "Success",
                            description: "User journeys exported successfully.",
                          });
                        } catch (error) {
                          toast({
                            title: "Error",
                            description: "Failed to export user journeys. Please try again.",
                            variant: "destructive",
                          });
                        }
                      }}
                      disabled={loading}
                    >
                      <Download className="h-4 w-4 mr-2" />
                      Export
                    </Button>

                    <Button
                      variant="outline"
                      size="icon"
                      onClick={handleRefresh}
                      disabled={loading}
                      className="hover:bg-gray-50"
                    >
                      <RefreshCw
                        className={`h-4 w-4 ${loading ? "animate-spin" : ""}`}
                      />
                    </Button>
                  </div>
                </div>

                {/* Journey Plan Grid */}
                <JourneyPlanGrid
                  searchQuery={filters.search}
                  filters={{
                    ...filters,
                    visitDate: filters.visitDate || new Date() // Provide a default date for type compatibility
                  }}
                  refreshTrigger={refreshTrigger}
                  onView={handleViewJourney}
                  onEdit={handleEditJourney}
                  onComplete={handleCompleteJourneyClick}
                  onDelete={(uid) => handleDeleteClick(uid, "journey")}
                />
              </TabsContent>

              {/* Today's Journey Plans Tab */}
              <TabsContent value="plans" className="mt-0">
                {/* Filters */}
                <div className="p-6 border-b">
                    <div className="flex items-center justify-between mb-4">
                    <h3 className="text-sm font-medium text-gray-700">
                      {filters.visitDate 
                        ? moment(filters.visitDate).format("YYYY-MM-DD") === moment().format("YYYY-MM-DD")
                          ? `Today's Plans (${assignedPlans.length} total)`
                          : `Showing plans for ${formatDateToDayMonthYear(filters.visitDate)} (${assignedPlans.length} total)`
                        : `Showing all journey plans (${assignedPlans.length} total)`}
                    </h3>
                    {(filters.visitDate || filters.jobPositionUID) && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          setFilters({
                            search: "",
                            jobPositionUID: "",
                            empUID: "",
                            eotStatus: "",
                            attendanceStatus: "",
                            visitDate: null,
                            orgUID: "Farmley"
                          });
                        }}
                      >
                        Clear Filters
                      </Button>
                    )}
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <div className="relative">
                      <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        type="date"
                        value={filters.visitDate ? moment(filters.visitDate).format("YYYY-MM-DD") : ""}
                        onChange={(e) =>
                          handleFilterChange(
                            "visitDate",
                            e.target.value ? new Date(e.target.value) : null
                          )
                        }
                        className="pl-10"
                        placeholder="Filter by date"
                      />
                    </div>

                    <Select
                      value={filters.jobPositionUID || "all"}
                      onValueChange={(value) =>
                        handleFilterChange("jobPositionUID", value === "all" ? "" : value)
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="All Positions" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="all">All Positions</SelectItem>
                        <SelectItem value="TB1348">TB1348</SelectItem>
                        <SelectItem value="TB1062">TB1062</SelectItem>
                      </SelectContent>
                    </Select>

                    <Button
                      variant="outline"
                      size="sm"
                      onClick={async () => {
                        try {
                          const blob = await journeyPlanService.exportJourneyPlans("csv", {
                            visitDate: filters.visitDate,
                            jobPositionUID: filters.jobPositionUID,
                            orgUID: filters.orgUID
                          });
                          const url = URL.createObjectURL(blob);
                          const a = document.createElement("a");
                          a.href = url;
                          a.download = `journey-plans-export-${new Date().toISOString().split("T")[0]}.csv`;
                          document.body.appendChild(a);
                          a.click();
                          document.body.removeChild(a);
                          URL.revokeObjectURL(url);
                          
                          toast({
                            title: "Success",
                            description: "Journey plans exported successfully.",
                          });
                        } catch (error) {
                          toast({
                            title: "Error",
                            description: "Failed to export journey plans. Please try again.",
                            variant: "destructive",
                          });
                        }
                      }}
                      disabled={loading}
                    >
                      <Download className="h-4 w-4 mr-2" />
                      Export
                    </Button>

                    <Button
                      variant="outline"
                      size="icon"
                      onClick={handleRefresh}
                      disabled={loading}
                      className="hover:bg-gray-50"
                    >
                      <RefreshCw
                        className={`h-4 w-4 ${loading ? "animate-spin" : ""}`}
                      />
                    </Button>
                  </div>
                </div>

                {/* Table */}
                <div className="w-full overflow-x-auto">
                  <Table className="w-full">
                    <TableHeader>
                      <TableRow className="hover:bg-transparent border-b">
                        <TableHead className="text-left">Visit Date</TableHead>
                        <TableHead className="text-left">Route</TableHead>
                        <TableHead className="text-left">Job Position ID</TableHead>
                        <TableHead className="text-left">Status</TableHead>
                        <TableHead className="text-center">Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {loading ? (
                        <TableSkeleton />
                      ) : assignedPlans.length === 0 ? (
                        <TableRow>
                          <TableCell colSpan={5} className="text-center py-12">
                            <div className="flex flex-col items-center gap-3">
                              <Calendar className="h-12 w-12 text-gray-300" />
                              <div className="text-muted-foreground">
                                {filters.visitDate 
                                  ? moment(filters.visitDate).format("YYYY-MM-DD") === moment().format("YYYY-MM-DD")
                                    ? "No journey plans scheduled for today"
                                    : `No journey plans found for ${formatDateToDayMonthYear(filters.visitDate)}`
                                  : "No journey plans found. Try adjusting filters or check database."}
                              </div>
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => {
                                  setFilters(prev => ({ ...prev, visitDate: null }));
                                }}
                                className="mt-2"
                              >
                                <RefreshCw className="mr-2 h-4 w-4" />
                                Show All Dates
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ) : (
                        assignedPlans
                          .slice((pagination.current - 1) * pagination.pageSize, pagination.current * pagination.pageSize)
                          .map((plan) => (
                          <TableRow
                            key={plan.UID}
                            className="hover:bg-gray-50"
                          >
                            <TableCell className="py-2 text-sm">
                              <div className="flex items-center gap-2">
                                <Calendar className="h-3 w-3 text-gray-400" />
                                {(() => {
                                  const date = plan.VisitDate;
                                  if (!date) {
                                    console.log('Missing date for plan:', plan.UID, plan);
                                    return "No Date";
                                  }
                                  const momentDate = moment(date);
                                  if (!momentDate.isValid()) {
                                    console.log('Invalid date format:', date);
                                    return "Invalid Date";
                                  }
                                  return formatDateToDayMonthYear(date);
                                })()}
                              </div>
                            </TableCell>
                            <TableCell className="py-2 text-sm">
                              {plan.RouteName || "-"}
                            </TableCell>
                            <TableCell className="py-2 text-sm">
                              {plan.JobPositionUID || "-"}
                            </TableCell>
                            <TableCell className="py-2">
                              <Badge
                                variant={getPlanStatusVariant(plan.Status)}
                                className="text-xs"
                              >
                                {plan.Status}
                              </Badge>
                            </TableCell>
                            <TableCell className="py-2 text-center">
                              <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-8 w-8 p-0"
                                  >
                                    <MoreHorizontal className="h-4 w-4" />
                                  </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end">
                                  <DropdownMenuItem
                                    onClick={() => handleViewPlan(plan.UID)}
                                  >
                                    <Eye className="mr-2 h-4 w-4" />
                                    View Plan Details
                                  </DropdownMenuItem>
                                  {plan.Status !== "Completed" && (
                                    <DropdownMenuItem
                                      onClick={() =>
                                        handleStartJourneyClick(plan.UID)
                                      }
                                    >
                                      <PlayCircle className="mr-2 h-4 w-4" />
                                      Start Journey
                                    </DropdownMenuItem>
                                  )}
                                  <DropdownMenuSeparator />
                                  <DropdownMenuItem
                                    onClick={() =>
                                      handleDeleteClick(plan.UID, "plan")
                                    }
                                    className="text-destructive"
                                  >
                                    <MoreHorizontal className="mr-2 h-4 w-4" />
                                    Delete Plan
                                  </DropdownMenuItem>
                                </DropdownMenuContent>
                              </DropdownMenu>
                            </TableCell>
                          </TableRow>
                        ))
                      )}
                    </TableBody>
                  </Table>
                </div>
              </TabsContent>
            </Tabs>

            {/* Pagination for Plans tab */}
            {activeTab === "plans" && assignedPlans.length > 0 && (
              <div className="flex items-center justify-between px-6 py-4 border-t bg-gray-50/30">
                <div className="text-sm text-muted-foreground">
                  Showing{" "}
                  <span className="font-medium text-gray-900">
                    {Math.min((pagination.current - 1) * pagination.pageSize + 1, pagination.total)}
                  </span>{" "}
                  to{" "}
                  <span className="font-medium text-gray-900">
                    {Math.min(
                      pagination.current * pagination.pageSize,
                      pagination.total
                    )}
                  </span>{" "}
                  of{" "}
                  <span className="font-medium text-gray-900">
                    {pagination.total}
                  </span>{" "}
                  entries
                </div>
                <div className="flex items-center gap-2">
                  <Select
                    value={pagination.pageSize.toString()}
                    onValueChange={(value) =>
                      setPagination((prev) => ({
                        ...prev,
                        pageSize: parseInt(value),
                        current: 1
                      }))
                    }
                  >
                    <SelectTrigger className="w-20">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="10">10</SelectItem>
                      <SelectItem value="25">25</SelectItem>
                      <SelectItem value="50">50</SelectItem>
                      <SelectItem value="100">100</SelectItem>
                    </SelectContent>
                  </Select>
                  <div className="flex gap-1">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        setPagination((prev) => ({
                          ...prev,
                          current: 1
                        }))
                      }
                      disabled={pagination.current === 1}
                    >
                      First
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        setPagination((prev) => ({
                          ...prev,
                          current: Math.max(1, prev.current - 1)
                        }))
                      }
                      disabled={pagination.current === 1}
                    >
                      Previous
                    </Button>
                    <span className="px-3 py-1 text-sm">
                      {pagination.current} of {Math.ceil(pagination.total / pagination.pageSize)}
                    </span>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        setPagination((prev) => ({
                          ...prev,
                          current: Math.min(Math.ceil(pagination.total / pagination.pageSize), prev.current + 1)
                        }))
                      }
                      disabled={
                        pagination.current >= Math.ceil(pagination.total / pagination.pageSize)
                      }
                    >
                      Next
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        setPagination((prev) => ({
                          ...prev,
                          current: Math.ceil(pagination.total / pagination.pageSize)
                        }))
                      }
                      disabled={
                        pagination.current >= Math.ceil(pagination.total / pagination.pageSize)
                      }
                    >
                      Last
                    </Button>
                  </div>
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Delete Confirmation Dialog */}
        <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Are you sure?</AlertDialogTitle>
              <AlertDialogDescription>
                This action cannot be undone. This will permanently delete the{" "}
                {deleteItemType}
                and all associated data.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDeleteConfirm}
                className="bg-destructive text-destructive-foreground"
              >
                Delete
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

        {/* Start Journey Dialog */}
        <AlertDialog
          open={startJourneyDialogOpen}
          onOpenChange={setStartJourneyDialogOpen}
        >
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Start Journey Plan</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to start this journey plan? This will
                begin tracking the journey progress.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction onClick={handleStartJourneyConfirm}>
                Start Journey
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

        {/* Complete Journey Dialog */}
        <AlertDialog
          open={completeJourneyDialogOpen}
          onOpenChange={setCompleteJourneyDialogOpen}
        >
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Complete Journey</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to mark this journey as completed? This
                action will finalize the journey status.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction onClick={handleCompleteJourneyConfirm}>
                Complete Journey
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  );
};

export default JourneyPlanManagement;
