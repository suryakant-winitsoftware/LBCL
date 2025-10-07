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
  Settings
} from "lucide-react";
import moment from "moment";
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
  visitDate: Date;
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
    visitDate: new Date("2025-08-21"), // Set to database's current date
    orgUID: "Farmley"
  });

  // Fetch data when filters or pagination change
  useEffect(() => {
    if (activeTab === "plans") {
      fetchTodayJourneyPlans();
    }
  }, [activeTab, pagination.current, pagination.pageSize, filters]);

  const fetchTodayJourneyPlans = async () => {
    setLoading(true);
    try {
      // Get the selected date for filtering
      const selectedDate = moment(filters.visitDate).format("YYYY-MM-DD");

      // Try different pagination parameters to work around backend issues
      const request: PagedRequest = {
        pageNumber: 0, // Try 0-based pagination
        pageSize: 0, // Try 0 to get all records (no pagination)
        isCountRequired: false, // Simplify request
        sortCriterias: [],
        filterCriterias: [] // No backend filters to avoid errors
      };

      // Use BeatHistory endpoint as primary data source
      const beatHistoryData = await api.beatHistory.selectAll(request);

      // Check different response formats
      const responseData =
        beatHistoryData?.Data?.PagedData ||
        beatHistoryData?.PagedData ||
        beatHistoryData?.data ||
        beatHistoryData?.Data ||
        [];

      if (beatHistoryData?.IsSuccess || beatHistoryData) {
        // Filter by date and other criteria on client side and transform data
        const transformedPlans = responseData
          .filter((plan: any) => {
            // Filter by selected date
            if (plan.VisitDate) {
              const planDate = moment(plan.VisitDate).format("YYYY-MM-DD");
              if (planDate !== selectedDate) {
                return false;
              }
            } else {
              return false;
            }

            // Filter by JobPositionUID if provided
            if (
              filters.jobPositionUID &&
              plan.JobPositionUID !== filters.jobPositionUID
            ) {
              return false;
            }

            return true;
          })
          .map((plan: any) => {
            // Determine status based on InvoiceStatus and other fields
            let status = "Pending";
            if (plan.InvoiceStatus === "Completed" || plan.HasAuditCompleted) {
              status = "Completed";
            } else if (
              plan.InvoiceStatus === "In Progress" ||
              (plan.ActualStoreVisits > 0 &&
                plan.ActualStoreVisits < plan.PlannedStoreVisits)
            ) {
              status = "In Progress";
            }

            return {
              UID: plan.UID,
              RouteUID: plan.RouteUID,
              RouteName: plan.RouteUID ? `Route ${plan.RouteUID}` : "No Route",
              OrgUID: plan.OrgUID || "Farmley",
              JobPositionUID: plan.JobPositionUID || "-",
              EmpUID: plan.LoginId || plan.EmpUID || "-",
              VisitDate: plan.VisitDate,
              PlannedStoreVisits: plan.PlannedStoreVisits || 0,
              ActualStoreVisits: plan.ActualStoreVisits || 0,
              SkippedStoreVisits: plan.SkippedStoreVisits || 0,
              UnPlannedStoreVisits:
                plan.UnPlannedStoreVisits || plan.UnplannedStoreVisits || 0,
              Coverage: plan.Coverage || 0,
              Status: status,
              StartTime: plan.StartTime
                ? moment(plan.StartTime).format("HH:mm")
                : plan.PlannedStartTime
                ? moment(plan.PlannedStartTime).format("HH:mm")
                : "",
              EndTime: plan.EndTime
                ? moment(plan.EndTime).format("HH:mm")
                : plan.PlannedEndTime
                ? moment(plan.PlannedEndTime).format("HH:mm")
                : "",
              InvoiceStatus: plan.InvoiceStatus || "Pending"
            };
          });

        setAssignedPlans(transformedPlans);
        setPagination((prev) => ({
          ...prev,
          total: transformedPlans.length
        }));

        if (transformedPlans.length === 0) {
          toast({
            title: "No Data",
            description: `No journey plans found for ${moment(
              filters.visitDate
            ).format("DD/MM/YYYY")}${
              filters.jobPositionUID
                ? ` and Job Position: ${filters.jobPositionUID}`
                : ""
            }`,
            variant: "default"
          });
        }
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
            <Skeleton className="h-4 w-16" />
          </TableCell>
          <TableCell className="py-2">
            <Skeleton className="h-4 w-20" />
          </TableCell>
          <TableCell className="py-2 text-center">
            <Skeleton className="h-4 w-8 mx-auto" />
          </TableCell>
          <TableCell className="py-2 text-center">
            <Skeleton className="h-4 w-8 mx-auto" />
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-gray-50">
      {/* Modern Header with Gradient */}
      <div className="border-b bg-white/80 backdrop-blur-sm sticky top-0 z-10 shadow-sm">
        <div className="container mx-auto px-4 py-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl shadow-lg">
                <Navigation className="h-6 w-6 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-600 bg-clip-text text-transparent">
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
                className="hover:bg-blue-50 hover:border-blue-300"
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
                className="hover:bg-purple-50 hover:border-purple-300"
              >
                <Settings className="mr-2 h-4 w-4" />
                Bulk Management
              </Button>
              <Button
                onClick={handleCreateJourneyPlan}
                size="sm"
                className="bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white shadow-md"
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
        <Card className="border-0 shadow-lg">
          <CardContent className="p-0">
            <Tabs value={activeTab} onValueChange={setActiveTab}>
              <div className="border-b bg-gray-50/50 px-6 pt-6">
                <TabsList className="grid w-full max-w-md grid-cols-2 bg-gray-100/50">
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
                <div className="p-6 border-b bg-gray-50/30">
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <div className="relative">
                      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        placeholder="Search by employee..."
                        value={filters.search}
                        onChange={(e) => handleSearch(e.target.value)}
                        className="pl-10 bg-white"
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
                      <SelectTrigger className="bg-white">
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
                      <SelectTrigger className="bg-white">
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
                      size="icon"
                      onClick={handleRefresh}
                      disabled={loading}
                      className="bg-white hover:bg-gray-50"
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
                  filters={filters}
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
                <div className="p-6 border-b bg-gray-50/30">
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <div className="relative">
                      <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        type="date"
                        value={moment(filters.visitDate).format("YYYY-MM-DD")}
                        onChange={(e) =>
                          handleFilterChange(
                            "visitDate",
                            new Date(e.target.value)
                          )
                        }
                        className="pl-10 bg-white"
                      />
                    </div>

                    <Input
                      placeholder="Job Position UID"
                      value={filters.jobPositionUID}
                      onChange={(e) =>
                        handleFilterChange("jobPositionUID", e.target.value)
                      }
                      className="bg-white"
                    />

                    <Button
                      variant="outline"
                      size="icon"
                      onClick={handleRefresh}
                      disabled={loading}
                      className="bg-white hover:bg-gray-50"
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
                        <TableHead className="text-left">Date</TableHead>
                        <TableHead className="text-left">Route</TableHead>
                        <TableHead className="text-left">Employee ID</TableHead>
                        <TableHead className="text-center">Planned</TableHead>
                        <TableHead className="text-center">Completed</TableHead>
                        <TableHead className="text-left">Status</TableHead>
                        <TableHead className="text-center">Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {loading ? (
                        <TableSkeleton />
                      ) : assignedPlans.length === 0 ? (
                        <TableRow>
                          <TableCell colSpan={7} className="text-center py-12">
                            <div className="flex flex-col items-center gap-3">
                              <Calendar className="h-12 w-12 text-gray-300" />
                              <div className="text-muted-foreground">
                                No journey plans found for the selected date and
                                filters.
                              </div>
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={handleCreateJourneyPlan}
                                className="mt-2"
                              >
                                <Plus className="mr-2 h-4 w-4" />
                                Create Journey Plan
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ) : (
                        assignedPlans.map((plan) => (
                          <TableRow
                            key={plan.UID}
                            className="hover:bg-gray-50/50"
                          >
                            <TableCell className="py-2 text-sm">
                              {moment(plan.VisitDate).format("DD/MM/YYYY")}
                            </TableCell>
                            <TableCell className="py-2 text-sm">
                              {plan.RouteUID || "-"}
                            </TableCell>
                            <TableCell className="py-2 text-sm">
                              {plan.EmpUID || "-"}
                            </TableCell>
                            <TableCell className="py-2 text-center text-sm">
                              {plan.PlannedStoreVisits}
                            </TableCell>
                            <TableCell className="py-2 text-center text-sm">
                              {plan.ActualStoreVisits}
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
                    {(pagination.current - 1) * pagination.pageSize + 1}
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
                  plans
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      setPagination((prev) => ({
                        ...prev,
                        current: prev.current - 1
                      }))
                    }
                    disabled={pagination.current === 1}
                    className="hover:bg-white"
                  >
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      setPagination((prev) => ({
                        ...prev,
                        current: prev.current + 1
                      }))
                    }
                    disabled={
                      pagination.current * pagination.pageSize >=
                      pagination.total
                    }
                    className="hover:bg-white"
                  >
                    Next
                  </Button>
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
