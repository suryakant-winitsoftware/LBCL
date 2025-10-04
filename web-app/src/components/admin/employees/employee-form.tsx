"use client";
/* eslint-disable @typescript-eslint/no-explicit-any */

import { useState, useEffect } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import * as z from "zod";
import { Loader2 } from "lucide-react";
import { format } from "date-fns";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useToast } from "@/components/ui/use-toast";
import { Role } from "@/types/admin.types";
import { roleService } from "@/services/admin/role.service";
import {
  organizationService,
  Organization,
} from "@/services/organizationService";
import { mobileAppActionService } from "@/services/mobileAppActionService";

// Define the form schema matching the web portal UserFormData structure
const employeeFormSchema = z.object({
  emp: z.object({
    uid: z.string().optional(),
    code: z.string().min(1, "Employee code is required"),
    name: z.string().min(1, "Name is required"),
    loginId: z.string().min(3, "Login ID must be at least 3 characters"),
    empNo: z.string().min(1, "Employee number is required"),
    email: z
      .string()
      .email("Invalid email address")
      .optional()
      .or(z.literal("")),
    phone: z.string().optional(),
    status: z.enum(["Active", "Inactive", "Pending"]),
    authType: z.string().default("Local"),
    aliasName: z.string().optional(),
  }),

  empInfo: z
    .object({
      email: z.string().optional(),
      phone: z.string().optional(),
      startDate: z.string().optional(),
      endDate: z.string().optional(),
      canHandleStock: z.boolean().default(false),
      adGroup: z.string().optional(),
      adUsername: z.string().optional(),
    })
    .optional(),

  jobPosition: z
    .object({
      userRoleUID: z.string().min(1, "Role selection is required"),
      orgUID: z.string().min(1, "Organization is required"),
      userType: z
        .enum(["PreSales", "VanSales", "Collector", "Other"])
        .optional(),
      branchUID: z.string().optional(),
      salesOfficeUID: z.string().optional(),
      departmentUID: z.string().optional(),
      department: z.string().optional(),
      collectionLimit: z.number().optional(),
      designation: z.string().optional(),
      hasEOT: z.boolean().default(false),
      locationMappingTemplateUID: z.string().optional(),
      locationMappingTemplateName: z.string().optional(),
      skuMappingTemplateUID: z.string().optional(),
      skuMappingTemplateName: z.string().optional(),
      locationType: z.string().optional(),
      locationValue: z.string().optional(),
      seqCode: z.string().optional(),
      empCode: z.string().optional(),
    })
    .optional(),

  empOrgMapping: z
    .array(
      z.object({
        orgUID: z.string(),
        isActive: z.boolean().default(true),
      })
    )
    .optional(),

  password: z.string().optional(),

  // Mobile App Access field
  mobileAppAccess: z.boolean().default(false),
});

type EmployeeFormData = z.infer<typeof employeeFormSchema>;

interface EmployeeFormProps {
  employeeId?: string | null;
  onSuccess: () => void;
  onCancel: () => void;
}

export function EmployeeForm({
  employeeId,
  onSuccess,
  onCancel,
}: EmployeeFormProps) {
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [initialLoading] = useState(!!employeeId);
  const [roles, setRoles] = useState<Role[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);

  const isEdit = !!employeeId;

  const form = useForm<EmployeeFormData>({
    resolver: zodResolver(employeeFormSchema) as any,
    defaultValues: {
      emp: {
        code: "",
        name: "",
        loginId: "",
        empNo: "",
        email: "",
        phone: "",
        status: "Active",
        authType: "Local",
        aliasName: "",
      },
      empInfo: {
        email: "",
        phone: "",
        startDate: format(new Date(), "yyyy-MM-dd"),
        endDate: "",
        canHandleStock: false,
        adGroup: "",
        adUsername: "",
      },
      jobPosition: {
        userRoleUID: "",
        orgUID: "",
        userType: undefined,
        branchUID: "",
        salesOfficeUID: "",
        departmentUID: "",
        department: "",
        collectionLimit: 0,
        designation: "",
        hasEOT: false,
        locationMappingTemplateUID: "",
        locationMappingTemplateName: "",
        skuMappingTemplateUID: "",
        skuMappingTemplateName: "",
        locationType: "",
        locationValue: "",
        seqCode: "",
        empCode: "",
      },
      empOrgMapping: [],
      password: "",
      mobileAppAccess: false,
    },
  });

  // Load roles and organizations
  useEffect(() => {
    const loadData = async () => {
      try {
        const rolePagingRequest = roleService.buildRolePagingRequest(1, 100);
        const rolesResponse = await roleService.getRoles(rolePagingRequest);
        setRoles(rolesResponse.pagedData.filter((role) => role.IsActive));

        const orgsResponse = await organizationService.getOrganizations(
          1,
          1000
        );
        const activeOrgs = orgsResponse.data.filter((org) => org.IsActive);
        setOrganizations(activeOrgs);
      } catch (error) {
        console.error("Failed to load data:", error);
        toast({
          title: "Warning",
          description:
            "Failed to load roles and organizations. Some features may not work properly.",
          variant: "destructive",
        });
      }
    };

    loadData();
  }, [toast]);

  const onSubmit = async (data: EmployeeFormData) => {
    try {
      setLoading(true);

      // Generate IDs matching existing database pattern (no underscores or hyphens)
      const employeeUID =
        data.emp.uid || data.emp.code || `TB${String(Date.now()).slice(-4)}`;
      const jobPositionUID = employeeUID; // Job position UID should match employee UID as per database

      // Build the employee data structure matching the web portal format
      const empDTOModel = {
        Emp: {
          // Don't send Id field - let backend auto-generate it
          UID: employeeUID,
          CompanyUID: "COMPANY_001",
          Code: data.emp.code,
          Name: data.emp.name,
          AliasName: data.emp.aliasName,
          LoginId: data.emp.loginId,
          EmpNo: data.emp.empNo,
          AuthType: data.emp.authType,
          Status: data.emp.status,
          EncryptedPassword: data.password,
          JobPositionUid: jobPositionUID,
          ApprovalStatus: "Approved",
          ActionType: isEdit ? 1 : 0, // 0 = Add, 1 = Update
          CreatedBy: "ADMIN",
          CreatedTime: isEdit ? undefined : new Date().toISOString(),
          ModifiedBy: isEdit ? "ADMIN" : undefined,
          ModifiedTime: isEdit ? new Date().toISOString() : undefined,
        },
        EmpInfo: data.empInfo
          ? {
              ...data.empInfo,
              uid: employeeUID,
              empUID: employeeUID,
              actionType: isEdit ? 1 : 0,
            }
          : null,
        JobPosition: data.jobPosition
          ? {
              ...data.jobPosition,
              uid: jobPositionUID,
              empUID: employeeUID,
              companyUID: "COMPANY_001",
              LocationMappingTemplateUID:
                data.jobPosition.locationMappingTemplateUID || null,
              SKUMappingTemplateUID:
                data.jobPosition.skuMappingTemplateUID || null,
              BranchUID: data.jobPosition.branchUID || null,
              SalesOfficeUID: data.jobPosition.salesOfficeUID || null,
              LocationType: data.jobPosition.locationType || null,
              LocationValue: data.jobPosition.locationValue || null,
            }
          : null,
        EmpOrgMapping: data.empOrgMapping?.map((mapping) => ({
          ...mapping,
          empUID: employeeUID,
        })) || [
          {
            empUID: employeeUID,
            orgUID: data.jobPosition?.orgUID || "",
            isActive: true,
          },
        ],
        FileSys: null,
      };

      // Log location data being saved
      console.log("💾 Location data being saved to JobPosition:", {
        LocationType: data.jobPosition?.locationType,
        LocationValue: data.jobPosition?.locationValue,
        BranchUID: data.jobPosition?.branchUID,
        SalesOfficeUID: data.jobPosition?.salesOfficeUID,
      });
      console.log("📝 Full JobPosition data:", empDTOModel.JobPosition);

      // Call the API using the same endpoint as web portal
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/MaintainUser/CUDEmployee`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("authToken") || ""}`,
          },
          body: JSON.stringify(empDTOModel),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();

      if (!result.IsSuccess && !result.isSuccessResponse) {
        throw new Error(
          result.Message || result.message || "Employee creation failed"
        );
      }

      // If mobile app access is enabled and this is a new employee, create mobile_app_action record
      if (!isEdit && data.mobileAppAccess) {
        try {
          console.log(
            "📱 Creating mobile app access for employee:",
            employeeUID
          );

          const mobileAccessResult =
            await mobileAppActionService.createMobileAppAction(
              employeeUID,
              data.jobPosition?.orgUID || "COMPANY_001",
              data.emp.loginId
            );

          if (!mobileAccessResult.success) {
            console.error(
              "Failed to create mobile app access:",
              mobileAccessResult.message
            );
            // Show warning but don't fail the entire operation
            toast({
              title: "Warning",
              description:
                "Employee created but mobile app access could not be enabled. Please try again from the employee details page.",
              variant: "default",
            });
          } else {
            console.log("✅ Mobile app access created successfully");
          }

          // Optionally, initiate mobile DB creation
          if (data.jobPosition?.userRoleUID) {
            try {
              await mobileAppActionService.initiateMobileDBCreation(
                employeeUID,
                jobPositionUID,
                data.jobPosition.userRoleUID,
                data.jobPosition?.orgUID || "COMPANY_001",
                "", // vehicleUID - empty for now
                data.emp.code
              );
              console.log("✅ Mobile DB creation initiated");
            } catch (dbError) {
              console.error("Failed to initiate mobile DB creation:", dbError);
              // This is optional, so we don't show an error
            }
          }
        } catch (error) {
          console.error("Error enabling mobile app access:", error);
          // Don't fail the main operation, just log the error
        }
      }

      toast({
        title: "Success",
        description: `Employee ${isEdit ? "updated" : "created"} successfully${
          !isEdit && data.mobileAppAccess ? " with mobile app access" : ""
        }!`,
      });

      onSuccess();
    } catch (error) {
      console.error("Failed to save employee:", error);
      toast({
        title: "Error",
        description: `Failed to ${
          isEdit ? "update" : "create"
        } employee. Please try again.`,
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  if (initialLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin" />
        <span className="ml-2">Loading employee data...</span>
      </div>
    );
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit as any)} className="space-y-6">
        <Tabs defaultValue="basic" className="w-full">
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="basic">Basic Info</TabsTrigger>
            <TabsTrigger value="details">Additional Details</TabsTrigger>
            <TabsTrigger value="role">Role & Access</TabsTrigger>
          </TabsList>

          {/* Basic Information Tab */}
          <TabsContent value="basic" className="space-y-4">
            <Card>
              <CardHeader className="relative">
                <div className="flex items-start justify-between">
                  <div>
                    <CardTitle>Basic Information</CardTitle>
                    <CardDescription>
                      Essential employee information required for account
                      creation.
                    </CardDescription>
                  </div>
                  <FormField
                    control={form.control as any}
                    name="emp.status"
                    render={({ field }) => (
                      <FormItem className="flex items-center space-x-2">
                        <FormLabel className="text-sm font-medium">
                          Status:
                        </FormLabel>
                        <Select
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                        >
                          <FormControl>
                            <SelectTrigger className="w-[120px]">
                              <SelectValue placeholder="Select status" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            <SelectItem value="Active">Active</SelectItem>
                            <SelectItem value="Inactive">Inactive</SelectItem>
                            <SelectItem value="Pending">Pending</SelectItem>
                          </SelectContent>
                        </Select>
                      </FormItem>
                    )}
                  />
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <FormField
                    control={form.control as any}
                    name="emp.code"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Employee Code *</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="EMP001" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="emp.name"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Full Name *</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="John Doe" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="emp.loginId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Login ID *</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="johndoe" />
                        </FormControl>
                        <FormDescription>
                          This will be used for system login
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="emp.empNo"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Employee Number *</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="E12345" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="emp.email"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Email Address</FormLabel>
                        <FormControl>
                          <Input
                            {...field}
                            type="email"
                            placeholder="john@example.com"
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="emp.phone"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Phone Number</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="+1 234 567 8900" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="emp.aliasName"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Alias Name</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="John D." />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                {/* Mobile App Access Toggle */}
                <FormField
                  control={form.control as any}
                  name="mobileAppAccess"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">
                          Mobile App Access
                        </FormLabel>
                        <FormDescription>
                          Allow this employee to access the mobile application
                        </FormDescription>
                      </div>
                      <FormControl>
                        <Switch
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                    </FormItem>
                  )}
                />

                {!isEdit && (
                  <FormField
                    control={form.control as any}
                    name="password"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Initial Password</FormLabel>
                        <FormControl>
                          <Input
                            {...field}
                            type="password"
                            placeholder="Enter initial password"
                          />
                        </FormControl>
                        <FormDescription>
                          Leave empty to generate a random password
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                )}
              </CardContent>
            </Card>
          </TabsContent>

          {/* Additional Details Tab */}
          <TabsContent value="details" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Additional Details</CardTitle>
                <CardDescription>
                  Optional information for employee profile.
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <FormField
                    control={form.control as any}
                    name="empInfo.startDate"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Start Date</FormLabel>
                        <FormControl>
                          <Input {...field} type="date" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="empInfo.endDate"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>End Date</FormLabel>
                        <FormControl>
                          <Input {...field} type="date" />
                        </FormControl>
                        <FormDescription>
                          Leave empty for permanent employment
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="empInfo.email"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Work Email</FormLabel>
                        <FormControl>
                          <Input
                            {...field}
                            type="email"
                            placeholder="john@company.com"
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="empInfo.phone"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Work Phone</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="+1 234 567 8900" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="empInfo.adGroup"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>AD Group</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="Sales-Team" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="empInfo.adUsername"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>AD Username</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="johndoe@domain.com" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control as any}
                  name="empInfo.canHandleStock"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">
                          Can Handle Stock
                        </FormLabel>
                        <FormDescription>
                          Allow this employee to manage inventory and stock
                          operations
                        </FormDescription>
                      </div>
                      <FormControl>
                        <Checkbox
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                    </FormItem>
                  )}
                />
              </CardContent>
            </Card>
          </TabsContent>

          {/* Role & Access Tab */}
          <TabsContent value="role" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Role & Access</CardTitle>
                <CardDescription>
                  Assign role and organizational access to the employee.
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <FormField
                    control={form.control as any}
                    name="jobPosition.userRoleUID"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Role *</FormLabel>
                        <Select
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select role" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {roles.map((role) => (
                              <SelectItem key={role.UID} value={role.UID}>
                                {role.RoleNameEn}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="jobPosition.userType"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>User Type</FormLabel>
                        <Select
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select user type" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            <SelectItem value="PreSales">Pre-Sales</SelectItem>
                            <SelectItem value="VanSales">Van Sales</SelectItem>
                            <SelectItem value="Collector">Collector</SelectItem>
                            <SelectItem value="Other">Other</SelectItem>
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="jobPosition.orgUID"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Organization *</FormLabel>
                        <Select
                          onValueChange={field.onChange}
                          defaultValue={field.value}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select organization" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {organizations.map((org) => (
                              <SelectItem key={org.UID} value={org.UID}>
                                {org.Name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="jobPosition.designation"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Designation</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="Sales Manager" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="jobPosition.department"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Department</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="Sales" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control as any}
                    name="jobPosition.collectionLimit"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Collection Limit</FormLabel>
                        <FormControl>
                          <Input
                            {...field}
                            type="number"
                            placeholder="10000"
                            onChange={(e) =>
                              field.onChange(parseFloat(e.target.value) || 0)
                            }
                            value={field.value || ""}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control as any}
                  name="jobPosition.hasEOT"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">Has EOT</FormLabel>
                        <FormDescription>
                          End of Tour permissions
                        </FormDescription>
                      </div>
                      <FormControl>
                        <Checkbox
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                    </FormItem>
                  )}
                />

                <div className="mt-6">
                  <h4 className="text-lg font-medium mb-4">Location Mapping</h4>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                      control={form.control as any}
                      name="jobPosition.locationMappingTemplateUID"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Location Template</FormLabel>
                          <FormControl>
                            <Input
                              {...field}
                              placeholder="Select location template"
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control as any}
                      name="jobPosition.skuMappingTemplateUID"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>SKU Template</FormLabel>
                          <FormControl>
                            <Input
                              {...field}
                              placeholder="Select SKU template"
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control as any}
                      name="jobPosition.locationType"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Location Type</FormLabel>
                          <FormControl>
                            <Input {...field} placeholder="Region/Zone/Area" />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control as any}
                      name="jobPosition.locationValue"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Location Value</FormLabel>
                          <FormControl>
                            <Input
                              {...field}
                              placeholder="Location identifier"
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        {/* Form Actions */}
        <div className="flex justify-end space-x-2 pt-6 border-t">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={loading}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={loading}>
            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEdit ? "Update Employee" : "Create Employee"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
