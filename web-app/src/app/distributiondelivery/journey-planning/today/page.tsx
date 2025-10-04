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
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/components/ui/use-toast";
import {
  Search,
  RefreshCw,
  MoreHorizontal,
  Eye,
  PlayCircle,
  Clock,
  Calendar,
  Users,
  MapPin,
  Download,
  ArrowLeft
} from "lucide-react";
import moment from "moment";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import {
  journeyPlanService,
  AssignedJourneyPlan,
  PagedRequest
} from "@/services/journeyPlanService";
import { api } from "@/services/api";

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

const TodaysJourneyPlans: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();

  // State management
  const [assignedPlans, setAssignedPlans] = useState<AssignedJourneyPlan[]>([]);
  const [loading, setLoading] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteItemId, setDeleteItemId] = useState<string | null>(null);
  const [startJourneyDialogOpen, setStartJourneyDialogOpen] = useState(false);
  const [startJourneyId, setStartJourneyId] = useState<string | null>(null);

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
    visitDate: new Date(), // Default to today
    orgUID: "Farmley"
  });

  // Fetch data when filters or pagination change (debounced)
  useEffect(() => {
    const timer = setTimeout(() => {
      fetchTodayJourneyPlans();
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
      
      // Check different response formats
      const responseData = (beatHistoryData as any)?.Data?.PagedData ||
        (beatHistoryData as any)?.PagedData ||
        (beatHistoryData as any)?.data ||
        (beatHistoryData as any)?.Data ||
        [];

      if ((beatHistoryData as any)?.IsSuccess || beatHistoryData) {
        // Always filter by the selected date (default to today)
        const selectedDate = filters.visitDate ? moment(filters.visitDate).format("YYYY-MM-DD") : moment().format("YYYY-MM-DD");
        
        const transformedPlans = responseData
          .filter((plan: any) => {
            // Check for visit_date (lowercase from database) or VisitDate (PascalCase from some APIs)
            const visitDate = plan.visit_date || plan.VisitDate || plan.visitDate;
            
            // Always filter by date for today's plans
            if (visitDate) {
              const planDate = moment(visitDate).format("YYYY-MM-DD");
              if (planDate !== selectedDate) {
                return false;
              }
            } else {
              return false; // Skip records without dates
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
        
        setAssignedPlans(transformedPlans);
        setPagination((prev) => ({
          ...prev,
          total: transformedPlans.length,
          current: 1 // Reset to first page when data changes
        }));

        console.log(`Found ${transformedPlans.length} journey plans for today`);
      } else {
        setAssignedPlans([]);
        setPagination((prev) => ({ ...prev, total: 0 }));
        toast({
          title: "No Data",
          description:
            "No journey plans found for today. Please ensure test data is inserted in the database.",
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
          "Failed to fetch today's journey plans. Check console for details.",
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
  const handleFilterChange = (key: keyof FilterState, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPagination((prev) => ({ ...prev, current: 1 }));
  };

  const handleViewPlan = (uid: string) => {
    router.push(
      `/distributiondelivery/journey-planning/plans/plan/${uid}`
    );
  };

  const handleDeleteClick = (uid: string) => {
    setDeleteItemId(uid);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteItemId) return;

    try {
      await journeyPlanService.deleteJourneyPlan(deleteItemId);
      toast({
        title: "Success",
        description: "Journey plan deleted successfully"
      });
      fetchTodayJourneyPlans();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete journey plan",
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

  const handleRefresh = () => {
    fetchTodayJourneyPlans();
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
      {/* Modern Header */}
      <div className="border-b sticky top-0 z-10 bg-white">
        <div className="container mx-auto px-4 py-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => router.push('/distributiondelivery/journey-planning/plans')}
                className="p-2"
              >
                <ArrowLeft className="h-4 w-4" />
              </Button>
              <div className="p-2 bg-blue-100 rounded-lg">
                <Calendar className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <h1 className="text-2xl font-semibold text-gray-900">
                  Today's Journey Plans
                </h1>
                <p className="text-sm text-muted-foreground">
                  Monitor and manage today's scheduled journey plans
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Button
                onClick={() =>
                  router.push("/distributiondelivery/journey-planning/plans/create")
                }
                size="sm"
                className="bg-blue-600 hover:bg-blue-700 text-white"
              >
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
                        visitDate: new Date(),
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
                        e.target.value ? new Date(e.target.value) : new Date()
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
                      a.download = `todays-journey-plans-export-${new Date().toISOString().split("T")[0]}.csv`;
                      document.body.appendChild(a);
                      a.click();
                      document.body.removeChild(a);
                      URL.revokeObjectURL(url);
                      
                      toast({
                        title: "Success",
                        description: "Today's journey plans exported successfully.",
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
                              setFilters(prev => ({ ...prev, visitDate: new Date() }));
                            }}
                            className="mt-2"
                          >
                            <RefreshCw className="mr-2 h-4 w-4" />
                            Reset to Today
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
                                return "No Date";
                              }
                              const momentDate = moment(date);
                              if (!momentDate.isValid()) {
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
                                  handleDeleteClick(plan.UID)
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

            {/* Pagination */}
            {assignedPlans.length > pagination.pageSize && (
              <div className="flex items-center justify-center space-x-2 py-4">
                <div className="flex items-center space-x-6 lg:space-x-8">
                  <div className="flex items-center space-x-2">
                    <p className="text-sm font-medium">Rows per page</p>
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
                      <SelectTrigger className="h-8 w-[70px]">
                        <SelectValue placeholder={pagination.pageSize} />
                      </SelectTrigger>
                      <SelectContent side="top">
                        {[10, 20, 30, 40, 50].map((pageSize) => (
                          <SelectItem key={pageSize} value={pageSize.toString()}>
                            {pageSize}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="flex w-[100px] items-center justify-center text-sm font-medium">
                    Page {pagination.current} of{" "}
                    {Math.ceil(pagination.total / pagination.pageSize)}
                  </div>
                  <div className="flex items-center space-x-2">
                    <Button
                      variant="outline"
                      className="hidden h-8 w-8 p-0 lg:flex"
                      onClick={() =>
                        setPagination((prev) => ({
                          ...prev,
                          current: 1
                        }))
                      }
                      disabled={pagination.current === 1}
                    >
                      <span className="sr-only">Go to first page</span>
                      <span className="h-4 w-4">««</span>
                    </Button>
                    <Button
                      variant="outline"
                      className="h-8 w-8 p-0"
                      onClick={() =>
                        setPagination((prev) => ({
                          ...prev,
                          current: Math.max(1, prev.current - 1)
                        }))
                      }
                      disabled={pagination.current === 1}
                    >
                      <span className="sr-only">Go to previous page</span>
                      <span className="h-4 w-4">‹</span>
                    </Button>
                    <Button
                      variant="outline"
                      className="h-8 w-8 p-0"
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
                      <span className="sr-only">Go to next page</span>
                      <span className="h-4 w-4">›</span>
                    </Button>
                    <Button
                      variant="outline"
                      className="hidden h-8 w-8 p-0 lg:flex"
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
                      <span className="sr-only">Go to last page</span>
                      <span className="h-4 w-4">»»</span>
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
                This action cannot be undone. This will permanently delete the journey plan
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
      </div>
    </div>
  );
};

export default TodaysJourneyPlans;