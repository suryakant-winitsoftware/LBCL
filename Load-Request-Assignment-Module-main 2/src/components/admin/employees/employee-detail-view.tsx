"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import {
  ArrowLeft,
  Edit,
  Trash2,
  Download,
  RefreshCw,
  User,
  Mail,
  Phone,
  Calendar,
  Building,
  Shield,
  MapPin,
  Key,
  Activity,
  Clock,
  Users,
  Settings,
  FileText,
  AlertTriangle,
  CheckCircle,
  XCircle
} from "lucide-react";
import { format } from "date-fns";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useToast } from "@/components/ui/use-toast";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";
import { RouteUserForm } from "./route-user-form";
import { authService } from "@/lib/auth-service";

// Types
interface EmployeeDetail {
  uid: string;
  code: string;
  name: string;
  loginId: string;
  empNo: string;
  email?: string;
  phone?: string;
  status: "Active" | "Inactive" | "Pending";
  authType: string;
  aliasName?: string;
  createdTime?: string;
  modifiedTime?: string;
  lastLoginTime?: string;

  // Employee Info
  empInfo?: {
    startDate?: string;
    endDate?: string;
    canHandleStock: boolean;
    adGroup?: string;
    adUsername?: string;
  };

  // Job Position
  jobPosition?: {
    userRoleUID: string;
    userRoleName?: string;
    orgUID: string;
    orgName?: string;
    userType?: string;
    designation?: string;
    department?: string;
    collectionLimit?: number;
    hasEOT: boolean;
    locationType?: string;
    locationValue?: string;
    branchUID?: string;
    salesOfficeUID?: string;
  };

  // Organization Mappings
  empOrgMappings?: Array<{
    orgUID: string;
    orgName: string;
    orgCode?: string;
    isActive: boolean;
  }>;

  // Session Info
  sessionInfo?: {
    lastLoginIP?: string;
    lastLoginDevice?: string;
    totalSessions?: number;
    activeSessions?: number;
  };

  // Route Assignments
  routeAssignments?: Array<{
    routeUID: string;
    routeName: string;
    routeCode: string;
    isActive: boolean;
  }>;
}

interface EmployeeDetailViewProps {
  employeeId: string;
}

export function EmployeeDetailView({ employeeId }: EmployeeDetailViewProps) {
  const router = useRouter();
  const { toast } = useToast();
  const [employee, setEmployee] = useState<EmployeeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [activeTab, setActiveTab] = useState("overview");

  useEffect(() => {
    loadEmployeeDetails();
  }, [employeeId]);

  const loadEmployeeDetails = async () => {
    console.log("🔄 Loading employee details for ID:", employeeId);
    try {
      setLoading(true);
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/MaintainUser/SelectMaintainUserDetailsByUID?empUID=${employeeId}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            ...authService.getAuthHeaders()
          }
        }
      );

      console.log("API Response Status:", response.status);

      if (response.ok) {
        const result = await response.json();
        console.log("API Response Data:", result);

        if (result.IsSuccess && result.Data) {
          // Transform API data to EmployeeDetail format
          const apiData = result.Data;
          console.log("Processing employee data:", apiData);
          console.log("JobPosition data:", apiData.JobPosition);
          console.log("UserRoleUID:", apiData.JobPosition?.UserRoleUID);
          console.log("OrgName:", apiData.JobPosition?.OrgName);
          console.log("Department:", apiData.JobPosition?.Department);

          // Fetch role name if UserRoleUID exists
          let userRoleName = "";
          if (apiData.JobPosition?.UserRoleUID) {
            try {
              const roleResponse = await fetch(
                `${
                  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
                }/Role/SelectRolesByUID?uid=${apiData.JobPosition.UserRoleUID}`,
                {
                  method: "GET",
                  headers: {
                    "Content-Type": "application/json",
                    ...authService.getAuthHeaders()
                  }
                }
              );

              if (roleResponse.ok) {
                const roleResult = await roleResponse.json();
                console.log("Role data response:", roleResult);
                if (roleResult.IsSuccess && roleResult.Data) {
                  userRoleName =
                    roleResult.Data.RoleNameEn ||
                    roleResult.Data.Name ||
                    roleResult.Data.RoleName ||
                    "";
                }
              }
            } catch (roleError) {
              console.error("Failed to fetch role data:", roleError);
            }
          }

          // Fetch route assignments separately
          let routeAssignments: any[] = [];
          try {
            const routeResponse = await fetch(
              `${
                process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
              }/RouteUser/SelectRouteUserByUID`,
              {
                method: "POST",
                headers: {
                  "Content-Type": "application/json",
                  ...authService.getAuthHeaders()
                },
                body: JSON.stringify([employeeId])
              }
            );

            if (routeResponse.ok) {
              const routeResult = await routeResponse.json();
              console.log("Route assignments response:", routeResult);
              if (routeResult.IsSuccess && routeResult.Data) {
                routeAssignments = routeResult.Data.map((route: any) => ({
                  routeUID: route.RouteUID || route.UID,
                  routeName: route.RouteName || route.Name || route.Label,
                  routeCode: route.RouteCode || route.Code,
                  isActive: route.IsActive !== false
                }));
              }
            }
          } catch (routeError) {
            console.error("Failed to fetch route assignments:", routeError);
          }

          const employee: EmployeeDetail = {
            uid: apiData.Emp?.UID || employeeId,
            code: apiData.Emp?.Code || "",
            name: apiData.Emp?.Name || "",
            loginId: apiData.Emp?.LoginId || "",
            empNo: apiData.Emp?.EmpNo || "",
            email: apiData.EmpInfo?.Email || "",
            phone: apiData.EmpInfo?.Phone || "",
            status: apiData.Emp?.Status === "Active" ? "Active" : "Inactive",
            authType: apiData.Emp?.AuthType || "",
            aliasName: apiData.Emp?.AliasName || "",
            createdTime: apiData.Emp?.CreatedTime,
            modifiedTime: apiData.Emp?.ModifiedTime,
            lastLoginTime: apiData.Emp?.LastLoginTime,
            empInfo: apiData.EmpInfo
              ? {
                  startDate: apiData.EmpInfo.StartDate,
                  endDate: apiData.EmpInfo.EndDate,
                  canHandleStock: apiData.EmpInfo.CanHandleStock || false,
                  adGroup: apiData.EmpInfo.ADGroup,
                  adUsername: apiData.EmpInfo.ADUsername
                }
              : undefined,
            jobPosition: apiData.JobPosition
              ? {
                  userRoleUID: apiData.JobPosition.UserRoleUID || "",
                  userRoleName: userRoleName || "N/A", // Use fetched role name
                  orgUID: apiData.JobPosition.OrgUID || "",
                  orgName: apiData.JobPosition.OrgUID, // Backend doesn't return OrgName, only OrgUID
                  userType: "N/A", // UserType not available from backend
                  designation: apiData.JobPosition.Designation || "N/A", // May not be available in backend
                  department: apiData.JobPosition.Department,
                  collectionLimit: apiData.JobPosition.CollectionLimit,
                  hasEOT: apiData.JobPosition.HasEOT || false,
                  locationType: apiData.JobPosition.LocationType,
                  locationValue: apiData.JobPosition.LocationValue || "N/A", // May not be available
                  branchUID:
                    apiData.JobPosition.BranchUID ||
                    apiData.JobPosition.branchuid, // Check both casing
                  salesOfficeUID:
                    apiData.JobPosition.SalesOfficeUID ||
                    apiData.JobPosition.salesofficeuid // Check both casing
                }
              : undefined,
            empOrgMappings:
              apiData.EmpOrgMapping?.map((mapping: any) => ({
                orgUID: mapping.OrgUID,
                orgName: mapping.OrgName || mapping.OrgUID,
                orgCode: mapping.OrgCode,
                isActive: mapping.IsActive
              })) || [],
            routeAssignments: routeAssignments,
            // Remove sessionInfo since no API is available
            sessionInfo: undefined
          };

          console.log("✅ Successfully processed employee data:", employee);
          setEmployee(employee);
          setLoading(false);
          return;
        } else {
          console.error("API returned unsuccessful result:", result);
        }
      } else {
        const errorText = await response.text();
        console.error(
          "API request failed with status:",
          response.status,
          "Response:",
          errorText
        );
        toast({
          title: "Error",
          description: `Failed to load employee data (Status: ${response.status}). Please check if the employee exists.`,
          variant: "destructive"
        });
      }
    } catch (error) {
      console.error("Failed to fetch employee data:", error);
      toast({
        title: "Error",
        description:
          "Failed to load employee details. Please check if the employee ID is correct.",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = () => {
    router.push(
      `/updatedfeatures/employee-management/employees/${employeeId}/edit`
    );
  };

  const handleDelete = () => {
    setIsDeleteDialogOpen(true);
  };

  const confirmDelete = async () => {
    try {
      // In production, call the delete API
      toast({
        title: "Success",
        description: "Employee deleted successfully."
      });
      router.push("/updatedfeatures/employee-management/employees/manage");
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete employee. Please try again.",
        variant: "destructive"
      });
    } finally {
      setIsDeleteDialogOpen(false);
    }
  };

  const handleBack = () => {
    router.push("/updatedfeatures/employee-management/employees/manage");
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Active":
        return "bg-green-500";
      case "Inactive":
        return "bg-red-500";
      case "Pending":
        return "bg-yellow-500";
      default:
        return "bg-gray-500";
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case "Active":
        return <CheckCircle className="h-4 w-4" />;
      case "Inactive":
        return <XCircle className="h-4 w-4" />;
      case "Pending":
        return <AlertTriangle className="h-4 w-4" />;
      default:
        return <Activity className="h-4 w-4" />;
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <RefreshCw className="h-8 w-8 animate-spin" />
        <span className="ml-2">Loading employee details...</span>
      </div>
    );
  }

  if (!employee) {
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="text-center py-8">
            <AlertTriangle className="h-12 w-12 text-yellow-500 mx-auto mb-4" />
            <h3 className="text-lg font-medium">Employee not found</h3>
            <p className="text-muted-foreground mb-4">
              The employee with ID "{employeeId}" could not be found.
            </p>
            <Button onClick={handleBack}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to Employee List
            </Button>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={handleBack}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              {employee.name}
            </h1>
            <p className="text-muted-foreground">
              Employee Details • {employee.code}
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => loadEmployeeDetails()}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          <Button variant="outline" onClick={handleEdit}>
            <Edit className="mr-2 h-4 w-4" />
            Edit
          </Button>
          <Button variant="outline" onClick={handleDelete}>
            <Trash2 className="mr-2 h-4 w-4" />
            Delete
          </Button>
        </div>
      </div>

      {/* Overview Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Status</CardTitle>
            {getStatusIcon(employee.status)}
          </CardHeader>
          <CardContent>
            <Badge className={`${getStatusColor(employee.status)} text-white`}>
              {employee.status}
            </Badge>
            <p className="text-xs text-muted-foreground mt-1">
              Current account status
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Role</CardTitle>
            <Shield className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-xl font-bold">
              {employee.jobPosition?.userRoleName || "N/A"}
            </div>
            <p className="text-xs text-muted-foreground">
              {employee.jobPosition?.designation || "No designation"}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Organization</CardTitle>
            <Building className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-xl font-bold">
              {employee.jobPosition?.orgName || "N/A"}
            </div>
            <p className="text-xs text-muted-foreground">
              {employee.jobPosition?.department || "No department"}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Location</CardTitle>
            <MapPin className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-xl font-bold">
              {employee.jobPosition?.locationValue || "Not Set"}
            </div>
            <p className="text-xs text-muted-foreground">
              {employee.jobPosition?.locationType || "Assigned location"}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Main Content Tabs */}
      <Card>
        <CardContent className="pt-6">
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList className="grid w-full grid-cols-5">
              <TabsTrigger value="overview">Overview</TabsTrigger>
              <TabsTrigger value="contact">Contact & Personal</TabsTrigger>
              <TabsTrigger value="role">Role & Permissions</TabsTrigger>
              <TabsTrigger value="routes">Route Assignments</TabsTrigger>
              <TabsTrigger value="activity">Activity Log</TabsTrigger>
            </TabsList>

            {/* Overview Tab */}
            <TabsContent value="overview" className="space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                {/* Basic Information */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <User className="h-5 w-5" />
                      Basic Information
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="flex items-center gap-4">
                      <Avatar className="h-16 w-16">
                        <AvatarImage
                          src={`https://api.dicebear.com/7.x/initials/svg?seed=${employee.name}`}
                        />
                        <AvatarFallback>
                          {employee.name
                            .split(" ")
                            .map((n) => n[0])
                            .join("")}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <h3 className="text-lg font-semibold">
                          {employee.name}
                        </h3>
                        <p className="text-muted-foreground">
                          {employee.aliasName}
                        </p>
                      </div>
                    </div>

                    <Separator />

                    <div className="grid gap-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Employee Code:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.code}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Employee No:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.empNo}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Login ID:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.loginId}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Auth Type:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.authType}
                        </span>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                {/* Employment Details */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Calendar className="h-5 w-5" />
                      Employment Details
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="grid gap-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Start Date:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.empInfo?.startDate
                            ? format(
                                new Date(employee.empInfo.startDate),
                                "MMM dd, yyyy"
                              )
                            : "N/A"}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          End Date:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.empInfo?.endDate
                            ? format(
                                new Date(employee.empInfo.endDate),
                                "MMM dd, yyyy"
                              )
                            : "Ongoing"}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Can Handle Stock:
                        </span>
                        <Badge
                          variant={
                            employee.empInfo?.canHandleStock
                              ? "default"
                              : "secondary"
                          }
                        >
                          {employee.empInfo?.canHandleStock ? "Yes" : "No"}
                        </Badge>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Collection Limit:
                        </span>
                        <span className="text-sm font-medium">
                          ₹
                          {(
                            employee.jobPosition?.collectionLimit || 0
                          ).toLocaleString()}
                        </span>
                      </div>
                    </div>

                    <Separator />

                    <div className="grid gap-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Created:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.createdTime
                            ? format(
                                new Date(employee.createdTime),
                                "MMM dd, yyyy HH:mm"
                              )
                            : "N/A"}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Last Modified:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.modifiedTime
                            ? format(
                                new Date(employee.modifiedTime),
                                "MMM dd, yyyy HH:mm"
                              )
                            : "N/A"}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Last Login:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.lastLoginTime
                            ? format(
                                new Date(employee.lastLoginTime),
                                "MMM dd, yyyy HH:mm"
                              )
                            : "Never"}
                        </span>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>

            {/* Contact & Personal Tab */}
            <TabsContent value="contact" className="space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Mail className="h-5 w-5" />
                      Contact Information
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="grid gap-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Email:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.email || "N/A"}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Phone:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.phone || "N/A"}
                        </span>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Settings className="h-5 w-5" />
                      AD Information
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="grid gap-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          AD Group:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.empInfo?.adGroup || "N/A"}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          AD Username:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.empInfo?.adUsername || "N/A"}
                        </span>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>

            {/* Role & Permissions Tab */}
            <TabsContent value="role" className="space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Shield className="h-5 w-5" />
                      Role Assignment
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="grid gap-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Role:
                        </span>
                        <Badge variant="default">
                          {employee.jobPosition?.userRoleName || "N/A"}
                        </Badge>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          User Type:
                        </span>
                        <Badge variant="secondary">
                          {employee.jobPosition?.userType || "N/A"}
                        </Badge>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          Designation:
                        </span>
                        <span className="text-sm font-medium">
                          {employee.jobPosition?.designation || "N/A"}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-muted-foreground">
                          EOT Access:
                        </span>
                        <Badge
                          variant={
                            employee.jobPosition?.hasEOT
                              ? "default"
                              : "secondary"
                          }
                        >
                          {employee.jobPosition?.hasEOT ? "Yes" : "No"}
                        </Badge>
                      </div>

                      {/* Location Information */}
                      {(employee.jobPosition?.locationType ||
                        employee.jobPosition?.locationValue) && (
                        <div className="flex justify-between">
                          <span className="text-sm text-muted-foreground">
                            Location:
                          </span>
                          <span className="text-sm font-medium">
                            {employee.jobPosition?.locationValue
                              ? `${employee.jobPosition.locationValue} (${employee.jobPosition.locationType})`
                              : employee.jobPosition?.locationType || "N/A"}
                          </span>
                        </div>
                      )}

                      {employee.jobPosition?.branchUID && (
                        <div className="flex justify-between">
                          <span className="text-sm text-muted-foreground">
                            Branch:
                          </span>
                          <span className="text-sm font-medium">
                            {employee.jobPosition.branchUID}
                          </span>
                        </div>
                      )}

                      {employee.jobPosition?.salesOfficeUID && (
                        <div className="flex justify-between">
                          <span className="text-sm text-muted-foreground">
                            Sales Office:
                          </span>
                          <span className="text-sm font-medium">
                            {employee.jobPosition.salesOfficeUID}
                          </span>
                        </div>
                      )}
                    </div>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Building className="h-5 w-5" />
                      Organization Mappings
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-2">
                      {employee.empOrgMappings?.map((org, index) => (
                        <div
                          key={index}
                          className="flex items-center justify-between p-2 border rounded"
                        >
                          <div>
                            <span className="font-medium">{org.orgName}</span>
                            {org.orgCode && (
                              <span className="text-sm text-muted-foreground ml-2">
                                ({org.orgCode})
                              </span>
                            )}
                          </div>
                          <Badge
                            variant={org.isActive ? "default" : "secondary"}
                          >
                            {org.isActive ? "Active" : "Inactive"}
                          </Badge>
                        </div>
                      )) || (
                        <p className="text-sm text-muted-foreground">
                          No organization mappings found.
                        </p>
                      )}
                    </div>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>

            {/* Route Assignments Tab */}
            <TabsContent value="routes" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <MapPin className="h-5 w-5" />
                    Route Assignments
                  </CardTitle>
                  <CardDescription>
                    Routes assigned to this employee
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    {employee.routeAssignments?.map((route, index) => (
                      <div
                        key={index}
                        className="flex items-center justify-between p-3 border rounded"
                      >
                        <div>
                          <span className="font-medium">{route.routeName}</span>
                          <span className="text-sm text-muted-foreground ml-2">
                            ({route.routeCode})
                          </span>
                        </div>
                        <Badge
                          variant={route.isActive ? "default" : "secondary"}
                        >
                          {route.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </div>
                    )) || (
                      <p className="text-sm text-muted-foreground">
                        No route assignments found.
                      </p>
                    )}
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            {/* Activity Log Tab */}
            <TabsContent value="activity" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Clock className="h-5 w-5" />
                    Account Activity
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid gap-2">
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">
                        Last Login:
                      </span>
                      <span className="text-sm font-medium">
                        {employee.lastLoginTime
                          ? format(
                              new Date(employee.lastLoginTime),
                              "MMM dd, yyyy 'at' HH:mm"
                            )
                          : "Never logged in"}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">
                        Account Created:
                      </span>
                      <span className="text-sm font-medium">
                        {employee.createdTime
                          ? format(
                              new Date(employee.createdTime),
                              "MMM dd, yyyy"
                            )
                          : "N/A"}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">
                        Last Modified:
                      </span>
                      <span className="text-sm font-medium">
                        {employee.modifiedTime
                          ? format(
                              new Date(employee.modifiedTime),
                              "MMM dd, yyyy"
                            )
                          : "N/A"}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">
                        Account Status:
                      </span>
                      <Badge
                        variant={
                          employee.status === "Active" ? "default" : "secondary"
                        }
                      >
                        {employee.status}
                      </Badge>
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <FileText className="h-5 w-5" />
                    Recent Activity
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">
                    Activity logging feature coming soon...
                  </p>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Employee</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete "{employee.name}"? This action
              cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setIsDeleteDialogOpen(false)}
            >
              Cancel
            </Button>
            <Button variant="destructive" onClick={confirmDelete}>
              Delete Employee
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
