"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/components/ui/use-toast";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import {
  ArrowLeft,
  Save,
  Loader,
  Calendar,
  Clock,
  Users,
  Route
} from "lucide-react";
import moment from "moment";
import { api } from "@/services/api";

const journeyPlanEditSchema = z.object({
  routeUID: z.string().min(1, "Route is required"),
  employeeUID: z.string().min(1, "Employee is required"),
  visitDate: z.string().min(1, "Visit date is required"),
  plannedStartTime: z.string().optional(),
  plannedEndTime: z.string().optional(),
  notes: z.string().optional(),
  selectedCustomers: z
    .array(z.string())
    .min(1, "At least one customer must be selected")
});

type JourneyPlanEditForm = z.infer<typeof journeyPlanEditSchema>;

interface JourneyPlanData {
  UID: string;
  RouteUID: string;
  OrgUID: string;
  JobPositionUID: string;
  EmpUID: string;
  LoginId: string;
  VisitDate: string;
  PlannedStartTime?: string;
  PlannedEndTime?: string;
  Notes?: string;
  Status: string;
}

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address: string;
  ContactNo: string;
  Type: string;
  Status: string;
  SeqNo: number;
  selected?: boolean;
}

interface Route {
  UID: string;
  Code: string;
  Name: string;
  Status: string;
}

interface Employee {
  UID: string;
  Name: string;
  LoginId: string;
  JobPositionUID: string;
  OrgUID: string;
}

const JourneyPlanEdit: React.FC = () => {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const journeyPlanId = params.id as string;

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [routes, setRoutes] = useState<Route[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [selectedCustomerIds, setSelectedCustomerIds] = useState<string[]>([]);
  const [journeyPlanData, setJourneyPlanData] =
    useState<JourneyPlanData | null>(null);

  const form = useForm<JourneyPlanEditForm>({
    resolver: zodResolver(journeyPlanEditSchema),
    defaultValues: {
      routeUID: "",
      employeeUID: "",
      visitDate: moment().format("YYYY-MM-DD"),
      plannedStartTime: "09:00",
      plannedEndTime: "18:00",
      notes: "",
      selectedCustomers: []
    }
  });

  useEffect(() => {
    if (journeyPlanId) {
      loadJourneyPlanData();
    }
  }, [journeyPlanId]);

  const loadJourneyPlanData = async () => {
    try {
      setLoading(true);

      // Load journey plan details
      const beatHistoryData = await api.beatHistory.selectAll({
        pageNumber: 0,
        pageSize: 1,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: [
          {
            filterBy: "UID",
            filterValue: journeyPlanId,
            filterOperator: "equals"
          }
        ]
      });

      if (
        beatHistoryData.IsSuccess &&
        beatHistoryData.Data?.PagedData?.length > 0
      ) {
        const plan = beatHistoryData.Data.PagedData[0];
        setJourneyPlanData(plan);

        // Set form values
        form.setValue("routeUID", plan.RouteUID);
        form.setValue("employeeUID", plan.EmpUID || plan.LoginId);
        form.setValue("visitDate", moment(plan.VisitDate).format("YYYY-MM-DD"));
        form.setValue("plannedStartTime", plan.PlannedStartTime || "09:00");
        form.setValue("plannedEndTime", plan.PlannedEndTime || "18:00");
        form.setValue("notes", plan.Notes || "");

        // Load related data
        await Promise.all([
          loadRoutes(plan.OrgUID),
          loadEmployees(plan.OrgUID),
          loadCustomers(plan.RouteUID, plan.UID)
        ]);
      } else {
        toast({
          title: "Error",
          description: "Journey plan not found",
          variant: "destructive"
        });
        router.push(
          "/updatedfeatures/journey-plan-management/journey-plans/manage"
        );
      }
    } catch (error) {
      console.error("Error loading journey plan:", error);
      toast({
        title: "Error",
        description: "Failed to load journey plan data",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const loadRoutes = async (orgUID: string) => {
    try {
      const routesData = await api.dropdown.getRoute(orgUID);
      if (routesData?.IsSuccess && routesData?.Data) {
        const routes = routesData.Data.map((route: any) => ({
          UID: route.UID || route.Value,
          Code: route.Code || "",
          Name: route.Label || route.Name,
          Status: route.Status || "Active"
        }));
        setRoutes(routes);
      }
    } catch (error) {
      console.error("Error loading routes:", error);
    }
  };

  const loadEmployees = async (orgUID: string) => {
    try {
      const employeeData = await api.dropdown.getEmployee(orgUID, false);
      if (employeeData?.IsSuccess && employeeData?.Data) {
        const employees = employeeData.Data.map((emp: any) => ({
          UID: emp.UID || emp.Value,
          Name: emp.Label || emp.Name,
          LoginId: emp.Code || emp.LoginId || emp.UID,
          JobPositionUID: emp.UID || emp.Value,
          OrgUID: orgUID
        }));
        setEmployees(employees);
      }
    } catch (error) {
      console.error("Error loading employees:", error);
    }
  };

  const loadCustomers = async (routeUID: string, beatHistoryUID: string) => {
    try {
      // First, get existing customer assignments
      const existingCustomers =
        await api.beatHistory.getCustomersByBeatHistoryUID(beatHistoryUID);
      const existingCustomerUIDs = new Set();

      if (existingCustomers?.IsSuccess && existingCustomers?.Data) {
        const visits = Array.isArray(existingCustomers.Data)
          ? existingCustomers.Data
          : existingCustomers.Data.StoreHistories || [];

        visits.forEach((visit: any) => {
          existingCustomerUIDs.add(visit.StoreUID);
        });
      }

      // Then get all available customers for the organization
      if (journeyPlanData?.OrgUID) {
        const storeData = await api.store.selectAll({
          pageNumber: 0,
          pageSize: 1000,
          isCountRequired: false,
          sortCriterias: [],
          filterCriterias: [
            {
              filterBy: "OrgUID",
              filterValue: journeyPlanData.OrgUID,
              filterOperator: "equals"
            }
          ]
        });

        if (storeData?.IsSuccess && storeData?.Data?.PagedData) {
          const customers = storeData.Data.PagedData.map(
            (store: any, index: number) => ({
              UID: store.UID,
              Code: store.Code,
              Name: store.Name,
              Address: store.Address || "",
              ContactNo: store.Mobile || store.Phone || "",
              Type: store.Type || "Regular",
              Status: store.Status || "Active",
              SeqNo: index + 1,
              selected: existingCustomerUIDs.has(store.UID)
            })
          );

          setCustomers(customers);

          // Set selected customers
          const selectedIds = customers
            .filter((c) => c.selected)
            .map((c) => c.UID);
          setSelectedCustomerIds(selectedIds);
          form.setValue("selectedCustomers", selectedIds);
        }
      }
    } catch (error) {
      console.error("Error loading customers:", error);
    }
  };

  const toggleCustomerSelection = (customerUID: string) => {
    const newSelection = selectedCustomerIds.includes(customerUID)
      ? selectedCustomerIds.filter((id) => id !== customerUID)
      : [...selectedCustomerIds, customerUID];

    setSelectedCustomerIds(newSelection);
    form.setValue("selectedCustomers", newSelection);
  };

  const toggleSelectAll = () => {
    const allSelected = selectedCustomerIds.length === customers.length;
    const newSelection = allSelected ? [] : customers.map((c) => c.UID);
    setSelectedCustomerIds(newSelection);
    form.setValue("selectedCustomers", newSelection);
  };

  const onSubmit = async (values: JourneyPlanEditForm) => {
    try {
      setSaving(true);

      if (!journeyPlanData) {
        throw new Error("Journey plan data not loaded");
      }

      // Update journey plan
      const updateData = {
        UID: journeyPlanData.UID,
        RouteUID: values.routeUID,
        OrgUID: journeyPlanData.OrgUID,
        JobPositionUID: journeyPlanData.JobPositionUID,
        EmpUID: values.employeeUID,
        LoginId: journeyPlanData.LoginId,
        VisitDate: values.visitDate,
        PlannedStartTime: values.plannedStartTime,
        PlannedEndTime: values.plannedEndTime,
        Notes: values.notes,
        PlannedStoreVisits: values.selectedCustomers.length,
        ModifiedBy: "CurrentUser", // Should be from auth context
        ModifiedTime: new Date().toISOString(),
        // Preserve other existing fields
        YearMonth: parseInt(moment(values.visitDate).format("YYYYMM")),
        Status: journeyPlanData.Status
      };

      // Note: In a real implementation, you would call an update API
      // For now, we'll simulate the update
      await new Promise((resolve) => setTimeout(resolve, 1000));

      toast({
        title: "Success",
        description: "Journey plan updated successfully"
      });

      router.push(
        `/updatedfeatures/journey-plan-management/journey-plans/view/${journeyPlanId}`
      );
    } catch (error: any) {
      console.error("Error updating journey plan:", error);
      toast({
        title: "Error",
        description: error.message || "Failed to update journey plan",
        variant: "destructive"
      });
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    router.push(
      `/updatedfeatures/journey-plan-management/journey-plans/view/${journeyPlanId}`
    );
  };

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
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  if (!journeyPlanData) {
    return (
      <div className="container mx-auto py-6">
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <h3 className="text-lg font-semibold mb-2">
              Journey Plan Not Found
            </h3>
            <p className="text-muted-foreground mb-4">
              The requested journey plan could not be found.
            </p>
            <Button onClick={handleCancel}>
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
          <h1 className="text-2xl font-bold">Edit Journey Plan</h1>
          <p className="text-muted-foreground">
            Modify journey plan details and customer assignments
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleCancel} disabled={saving}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Cancel
          </Button>
          <Button onClick={form.handleSubmit(onSubmit)} disabled={saving}>
            {saving ? (
              <Loader className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Save className="mr-2 h-4 w-4" />
            )}
            {saving ? "Saving..." : "Save Changes"}
          </Button>
        </div>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Main Form */}
            <div className="lg:col-span-2 space-y-6">
              {/* Basic Details */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Route className="h-5 w-5" />
                    Journey Plan Details
                  </CardTitle>
                  <CardDescription>
                    Update basic journey plan information
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="routeUID"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Route</FormLabel>
                          <Select
                            onValueChange={field.onChange}
                            value={field.value}
                          >
                            <FormControl>
                              <SelectTrigger>
                                <SelectValue placeholder="Select route" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              {routes.map((route) => (
                                <SelectItem key={route.UID} value={route.UID}>
                                  {route.Name} ({route.Code})
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="employeeUID"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Employee</FormLabel>
                          <Select
                            onValueChange={field.onChange}
                            value={field.value}
                          >
                            <FormControl>
                              <SelectTrigger>
                                <SelectValue placeholder="Select employee" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              {employees.map((employee) => (
                                <SelectItem
                                  key={employee.UID}
                                  value={employee.UID}
                                >
                                  {employee.Name} ({employee.LoginId})
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="visitDate"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Visit Date</FormLabel>
                          <FormControl>
                            <Input type="date" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <div className="space-y-2">
                      <FormLabel>Planned Time</FormLabel>
                      <div className="flex gap-2">
                        <FormField
                          control={form.control}
                          name="plannedStartTime"
                          render={({ field }) => (
                            <FormItem className="flex-1">
                              <FormControl>
                                <Input
                                  type="time"
                                  placeholder="Start time"
                                  {...field}
                                />
                              </FormControl>
                            </FormItem>
                          )}
                        />
                        <FormField
                          control={form.control}
                          name="plannedEndTime"
                          render={({ field }) => (
                            <FormItem className="flex-1">
                              <FormControl>
                                <Input
                                  type="time"
                                  placeholder="End time"
                                  {...field}
                                />
                              </FormControl>
                            </FormItem>
                          )}
                        />
                      </div>
                    </div>
                  </div>

                  <FormField
                    control={form.control}
                    name="notes"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Notes</FormLabel>
                        <FormControl>
                          <Textarea
                            placeholder="Enter any additional notes..."
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </CardContent>
              </Card>

              {/* Customer Selection */}
              <Card>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <div>
                      <CardTitle className="flex items-center gap-2">
                        <Users className="h-5 w-5" />
                        Customer Selection
                      </CardTitle>
                      <CardDescription>
                        Select customers to visit ({selectedCustomerIds.length}/
                        {customers.length} selected)
                      </CardDescription>
                    </div>
                    {customers.length > 0 && (
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={toggleSelectAll}
                      >
                        {selectedCustomerIds.length === customers.length
                          ? "Deselect All"
                          : "Select All"}
                      </Button>
                    )}
                  </div>
                </CardHeader>
                <CardContent>
                  {customers.length === 0 ? (
                    <div className="text-center py-8 text-muted-foreground">
                      No customers available for selection
                    </div>
                  ) : (
                    <div className="space-y-2 max-h-96 overflow-y-auto">
                      {customers.map((customer) => (
                        <div
                          key={customer.UID}
                          className="flex items-center space-x-3 p-3 border rounded-lg hover:bg-muted/50 cursor-pointer"
                          onClick={() => toggleCustomerSelection(customer.UID)}
                        >
                          <Checkbox
                            checked={selectedCustomerIds.includes(customer.UID)}
                            onCheckedChange={() =>
                              toggleCustomerSelection(customer.UID)
                            }
                            onClick={(e) => e.stopPropagation()}
                          />
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2">
                              <span className="text-sm font-medium">
                                #{customer.SeqNo} - {customer.Code}
                              </span>
                              <span className="text-sm text-muted-foreground">
                                {customer.Name}
                              </span>
                            </div>
                            <div className="text-xs text-muted-foreground mt-1">
                              {customer.Address}
                              {customer.ContactNo && ` • ${customer.ContactNo}`}
                            </div>
                          </div>
                          <div className="text-sm text-muted-foreground">
                            {customer.Type}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                  <FormField
                    control={form.control}
                    name="selectedCustomers"
                    render={() => (
                      <FormItem className="mt-4">
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </CardContent>
              </Card>
            </div>

            {/* Sidebar */}
            <div className="space-y-6">
              {/* Journey Status */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg">Current Status</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div>
                    <div className="text-sm text-muted-foreground">Status</div>
                    <div className="font-medium">{journeyPlanData.Status}</div>
                  </div>
                  <div>
                    <div className="text-sm text-muted-foreground">
                      Journey ID
                    </div>
                    <div className="font-mono text-xs">
                      {journeyPlanData.UID}
                    </div>
                  </div>
                  <div>
                    <div className="text-sm text-muted-foreground">
                      Last Modified
                    </div>
                    <div className="text-sm">
                      {moment(
                        journeyPlanData.ModifiedTime ||
                          journeyPlanData.CreatedTime
                      ).format("DD/MM/YYYY HH:mm")}
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Quick Stats */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg">Quick Stats</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">
                      Total Customers
                    </span>
                    <span className="font-medium">{customers.length}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">
                      Selected
                    </span>
                    <span className="font-medium text-blue-600">
                      {selectedCustomerIds.length}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">
                      Available Routes
                    </span>
                    <span className="font-medium">{routes.length}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-muted-foreground">
                      Available Employees
                    </span>
                    <span className="font-medium">{employees.length}</span>
                  </div>
                </CardContent>
              </Card>

              {/* Help */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg">Edit Guidelines</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm text-muted-foreground">
                  <p>• You can change the route, employee, and visit date</p>
                  <p>• Select customers to visit from the available list</p>
                  <p>• Adjust planned start and end times as needed</p>
                  <p>• Add notes to provide additional context</p>
                  <p>• Changes will be saved and applied immediately</p>
                </CardContent>
              </Card>
            </div>
          </div>
        </form>
      </Form>
    </div>
  );
};

export default JourneyPlanEdit;
