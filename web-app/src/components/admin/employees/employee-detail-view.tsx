"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { 
  ArrowLeft, 
  Edit, 
  Trash2,
  RefreshCw, 
  User, 
  Mail, 
  Building, 
  Shield,
  AlertTriangle,
  Phone,
  Calendar,
  Hash,
  UserCheck,
  MapPin,
  Briefcase,
  Clock
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import { Separator } from "@/components/ui/separator";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import { Skeleton } from "@/components/ui/skeleton";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { employeeService } from "@/services/admin/employee.service";
import { roleService } from "@/services/admin/role.service";
import { organizationService } from "@/services/organizationService";

interface EmployeeDetailViewProps {
  employeeId: string;
}

export function EmployeeDetailView({ employeeId }: EmployeeDetailViewProps) {
  const router = useRouter();
  const { toast } = useToast();
  const [employee, setEmployee] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [roleInfo, setRoleInfo] = useState<any>(null);
  const [reportsToUser, setReportsToUser] = useState<any>(null);
  const [organizationName, setOrganizationName] = useState<string>('');
  const [departmentName, setDepartmentName] = useState<string>('');

  useEffect(() => {
    loadEmployeeDetails();
  }, [employeeId]);

  const loadEmployeeDetails = async () => {
    try {
      setLoading(true);
      const data = await employeeService.getEmployeeById(employeeId);
      
      setEmployee(data);
      
      // Load role information if UserRoleUID exists
      if (data?.JobPosition?.UserRoleUID) {
        try {
          const role = await roleService.getRoleById(data.JobPosition.UserRoleUID);
          setRoleInfo(role);
        } catch (roleError) {
          console.error("Failed to load role details:", roleError);
        }
      }
      
      // Load reports-to user information if ReportsToUID exists and is not empty
      if (data?.JobPosition?.ReportsToUID && data.JobPosition.ReportsToUID.trim() !== "") {
        try {
          const reportsToEmployee = await employeeService.getEmployeeById(data.JobPosition.ReportsToUID);
          setReportsToUser(reportsToEmployee);
        } catch (reportsToError) {
          console.error("Failed to load reports-to employee:", reportsToError);
        }
      }
      
      // Set department name - the data already comes as readable text (e.g., "Operations")
      if (data?.JobPosition?.Department) {
        setDepartmentName(data.JobPosition.Department);
      }
      
      // Load organization name if OrgUID exists
      if (data?.JobPosition?.OrgUID) {
        try {
          const organizations = await organizationService.getOrganizations(1, 500);
          const org = organizations.data?.find((o: any) => o.UID === data.JobPosition.OrgUID);
          if (org) {
            setOrganizationName(org.Name || data.JobPosition.OrgUID);
          }
        } catch (orgError) {
          console.error("Failed to load organization:", orgError);
          setOrganizationName(data.JobPosition.OrgUID); // Fallback to UID
        }
      }
    } catch (error) {
      console.error("Failed to load employee:", error);
      toast({
        title: "Error",
        description: "Failed to load employee details.",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = () => {
    router.push(`/administration/team-management/employees/${employeeId}/edit`);
  };

  const handleDelete = () => {
    setIsDeleteDialogOpen(true);
  };

  const confirmDelete = async () => {
    try {
      await employeeService.deleteEmployee(employeeId);
      toast({
        title: "Success",
        description: "Employee deleted successfully.",
      });
      router.push("/administration/team-management/employees");
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete employee.",
        variant: "destructive",
      });
    }
    setIsDeleteDialogOpen(false);
  };

  const handleBack = () => {
    router.push("/administration/team-management/employees");
  };

  if (loading) {
    return (
      <div className="container mx-auto p-6 space-y-6">
        <Skeleton className="h-10 w-32" />
        <div className="space-y-4">
          <Skeleton className="h-32 w-full" />
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Skeleton className="h-48 w-full" />
            <Skeleton className="h-48 w-full" />
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Skeleton className="h-48 w-full" />
            <Skeleton className="h-48 w-full" />
          </div>
        </div>
      </div>
    );
  }

  if (!employee || !employee.Emp) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center py-8">
          <h2 className="text-2xl font-bold mb-2">Employee Not Found</h2>
          <p className="text-muted-foreground mb-4">The requested employee could not be found.</p>
          <Button onClick={handleBack}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Employees
          </Button>
        </div>
      </div>
    );
  }

  const emp = employee.Emp || {};
  const empInfo = employee.EmpInfo || {};
  const jobPosition = employee.JobPosition || {};

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={handleBack}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Employees
        </Button>
        <Button variant="outline" size="sm" onClick={() => loadEmployeeDetails()}>
          <RefreshCw className="h-4 w-4" />
        </Button>
      </div>

      {/* Employee Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl flex items-center gap-3">
            {emp.Name || 'Unknown'}
            <Badge variant={emp.Status === "Active" ? "default" : "secondary"}>
              {emp.Status || 'Unknown'}
            </Badge>
          </CardTitle>
          <div className="flex items-center gap-2 text-muted-foreground">
            <Hash className="h-3 w-3" />
            <code className="text-sm">{emp.Code}</code>
            <Separator orientation="vertical" className="h-4" />
            <User className="h-3 w-3" />
            <span className="text-sm">{emp.LoginId}</span>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            {emp.Name && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Full Name</p>
                <p className="font-semibold">{emp.Name}</p>
              </div>
            )}
            {emp.Code && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Employee Code</p>
                <p className="font-semibold">{emp.Code}</p>
              </div>
            )}
            {emp.LoginId && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Login ID</p>
                <p className="font-semibold">{emp.LoginId}</p>
              </div>
            )}
            {emp.AliasName && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Alias Name</p>
                <p className="font-semibold">{emp.AliasName}</p>
              </div>
            )}
            {emp.CreatedTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Created Time</p>
                <p className="font-semibold">{formatDateToDayMonthYear(emp.CreatedTime)}</p>
              </div>
            )}
            {emp.ModifiedTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Modified Time</p>
                <p className="font-semibold">{formatDateToDayMonthYear(emp.ModifiedTime)}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Contact Information */}
      {(empInfo.Email || empInfo.Phone || empInfo.StartDate || empInfo.EndDate) && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Mail className="h-5 w-5" />
              Contact & Employment Details
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
              {empInfo.Email && (
                <div>
                  <p className="text-sm text-muted-foreground mb-1">Email Address</p>
                  <p className="font-semibold">{empInfo.Email}</p>
                </div>
              )}
              {empInfo.Phone && (
                <div>
                  <p className="text-sm text-muted-foreground mb-1">Phone Number</p>
                  <p className="font-semibold">{empInfo.Phone}</p>
                </div>
              )}
              {empInfo.StartDate && (
                <div>
                  <p className="text-sm text-muted-foreground mb-1">Start Date</p>
                  <p className="font-semibold">{formatDateToDayMonthYear(empInfo.StartDate)}</p>
                </div>
              )}
              {empInfo.EndDate && (
                <div>
                  <p className="text-sm text-muted-foreground mb-1">End Date</p>
                  <p className="font-semibold">{formatDateToDayMonthYear(empInfo.EndDate)}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Role & Position Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            Role & Position
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            <div>
              <p className="text-sm text-muted-foreground mb-1">Role</p>
              <p className="font-semibold">
                {roleInfo?.Name || roleInfo?.RoleNameEn || roleInfo?.Code || jobPosition.RoleName || jobPosition.UserRoleName || 'Not Assigned'}
              </p>
            </div>
            {jobPosition.Designation && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Designation</p>
                <p className="font-semibold">{jobPosition.Designation}</p>
              </div>
            )}
            {(departmentName || jobPosition.Department) && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Department</p>
                <p className="font-semibold">{departmentName || jobPosition.Department}</p>
              </div>
            )}
            <div>
              <p className="text-sm text-muted-foreground mb-1">Reports To</p>
              <p className="font-semibold">
                {reportsToUser?.Emp?.Name || jobPosition.ReportsToName || 'Not Assigned'}
              </p>
            </div>
            {jobPosition.ReportsToUID && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Reports To UID</p>
                <p className="font-semibold">{jobPosition.ReportsToUID}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Organization & Location */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Building className="h-5 w-5" />
            Organization & Location
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            {(organizationName || jobPosition.OrgUID) && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Organization</p>
                <p className="font-semibold">{organizationName || jobPosition.OrgUID}</p>
              </div>
            )}
            {jobPosition.LocationType && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Location Type</p>
                <p className="font-semibold">{jobPosition.LocationType}</p>
              </div>
            )}
            {jobPosition.LocationValue && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Location Value</p>
                <p className="font-semibold">{jobPosition.LocationValue}</p>
              </div>
            )}
            {jobPosition.BranchUID && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Branch UID</p>
                <p className="font-semibold">{jobPosition.BranchUID}</p>
              </div>
            )}
            {jobPosition.SalesOfficeUID && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Sales Office UID</p>
                <p className="font-semibold">{jobPosition.SalesOfficeUID}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Employee</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete "{emp.Name}"? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDeleteDialogOpen(false)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={confirmDelete}>
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}