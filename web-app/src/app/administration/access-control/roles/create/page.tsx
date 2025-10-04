"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { motion, AnimatePresence } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { useToast } from "@/components/ui/use-toast";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command"
import {
  ArrowLeft,
  ArrowRight,
  CheckCircle2,
  Crown,
  Shield,
  Building,
  Monitor,
  Smartphone,
  RotateCcw,
  Search,
  ChevronDown,
  Check
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Module, ModuleHierarchy } from "@/types/admin.types";
import { roleService } from "@/services/admin/role.service";
import { permissionService } from "@/services/admin/permission.service";
import { rolePermissionService } from "@/services/admin/role-permission.service";
import { authService } from "@/lib/auth-service";

const roleFormSchema = z.object({
  code: z
    .string()
    .min(1, "Role code is required")
    .max(20, "Role code must be 20 characters or less"),
  roleNameEn: z
    .string()
    .min(1, "Role name is required")
    .max(100, "Role name must be 100 characters or less"),
  roleNameOther: z.string().optional(),
  parentRoleUid: z.string().optional(),
  orgUid: z.string().optional(),
  isPrincipalRole: z.boolean().default(false),
  isDistributorRole: z.boolean().default(false),
  isAdmin: z.boolean().default(false),
  isWebUser: z.boolean().default(true),
  isAppUser: z.boolean().default(false),
  isActive: z.boolean().default(true),
  haveWarehouse: z.boolean().default(false),
  haveVehicle: z.boolean().default(false),
  isForReportsTo: z.boolean().default(false)
});

type RoleFormData = z.infer<typeof roleFormSchema>;

interface StepConfig {
  num: number;
  label: string;
  mobileLabel: string;
}

const progressSteps: StepConfig[] = [
  { num: 1, label: "Basic Information", mobileLabel: "Basic" },
  {
    num: 2,
    label: "Permissions & Access Configuration",
    mobileLabel: "Configuration"
  }
];

export default function CreateRolePage() {
  const router = useRouter();
  const { toast } = useToast();
  const [currentStep, setCurrentStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [webModules, setWebModules] = useState<ModuleHierarchy[]>([]);
  const [mobileModules, setMobileModules] = useState<ModuleHierarchy[]>([]);
  const [selectedWebModules, setSelectedWebModules] = useState<string[]>([]);
  const [selectedMobileModules, setSelectedMobileModules] = useState<string[]>(
    []
  );
  const [webModulePages, setWebModulePages] = useState<
    { uid: string; name: string }[]
  >([]);
  const [mobileModulePages, setMobileModulePages] = useState<
    { uid: string; name: string }[]
  >([]);
  const [availableRoles, setAvailableRoles] = useState<any[]>([]);
  const [loadingRoles, setLoadingRoles] = useState(false);
  const [parentRolePopoverOpen, setParentRolePopoverOpen] = useState(false);

  // Get current user's organization
  const getCurrentUserOrg = () => {
    const currentUser = authService.getCurrentUser();
    return currentUser?.currentOrganization?.uid || currentUser?.currentOrganization?.code || "WINIT";
  };

  const form = useForm<RoleFormData>({
    resolver: zodResolver(roleFormSchema),
    defaultValues: {
      code: "",
      roleNameEn: "",
      roleNameOther: "",
      parentRoleUid: "",
      orgUid: getCurrentUserOrg(),
      isPrincipalRole: false,
      isDistributorRole: false,
      isAdmin: false,
      isWebUser: true,
      isAppUser: false,
      isActive: true,
      haveWarehouse: false,
      haveVehicle: false,
      isForReportsTo: false
    }
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
    trigger,
    watch,
    setValue,
    reset
  } = form;
  const formData = watch();

  // Load modules and roles - only on mount
  useEffect(() => {
    const loadModules = async () => {
      try {
        // Use the same method as permission management to get module hierarchy
        const [webHierarchy, mobileHierarchy] = await Promise.all([
          permissionService["getModuleHierarchy"]("Web"),
          permissionService["getModuleHierarchy"]("Mobile")
        ]);

        setWebModules(Array.isArray(webHierarchy) ? webHierarchy : []);
        setMobileModules(Array.isArray(mobileHierarchy) ? mobileHierarchy : []);

        // Extract all pages from hierarchy for module selection
        const webPages = extractPagesFromHierarchy(webHierarchy);
        const mobilePages = extractPagesFromHierarchy(mobileHierarchy);

        setWebModulePages(webPages);
        setMobileModulePages(mobilePages);
      } catch (error) {
        console.error("Failed to load modules:", error);
        setWebModules([]);
        setMobileModules([]);
        setWebModulePages([]);
        setMobileModulePages([]);
      }
    };

    const loadAvailableRoles = async () => {
      try {
        setLoadingRoles(true);
        const pagingRequest = roleService.buildRolePagingRequest(1, 100);
        const response = await roleService.getRoles(pagingRequest);
        // Filter to only show active roles that can be parent roles
        const activeRoles = response.pagedData.filter((role) => role.IsActive);
        setAvailableRoles(activeRoles);
      } catch (error) {
        console.error("Failed to load available roles:", error);
        setAvailableRoles([]);
      } finally {
        setLoadingRoles(false);
      }
    };

    loadModules();
    loadAvailableRoles();
  }, []); // Empty dependency array - only run once on mount

  // Helper function to extract all pages from module hierarchy
  const extractPagesFromHierarchy = (hierarchy: ModuleHierarchy[]) => {
    const pages: { uid: string; name: string }[] = [];

    hierarchy.forEach((module) => {
      module.children?.forEach((subModule) => {
        subModule.children?.forEach((page) => {
          pages.push({
            uid: page.UID,
            name: `${module.ModuleNameEn} > ${subModule.SubModuleNameEn} > ${page.SubSubModuleNameEn}`
          });
        });
      });
    });

    return pages;
  };

  const validateStep = async (step: number): Promise<boolean> => {
    switch (step) {
      case 1:
        return await trigger(["code", "roleNameEn"]);
      case 2:
        if (!(formData.isPrincipalRole || formData.isDistributorRole))
          return false;
        if (!(formData.isWebUser || formData.isAppUser)) return false;
        if (formData.isWebUser && selectedWebModules.length === 0) return false;
        if (formData.isAppUser && selectedMobileModules.length === 0)
          return false;
        return true;
      default:
        return true;
    }
  };

  const handleNext = async () => {
    const isValid = await validateStep(currentStep);

    if (currentStep === 2 && !isValid) {
      if (!(formData.isPrincipalRole || formData.isDistributorRole)) {
        toast({
          title: "Validation Error",
          description:
            "Please select at least one role type (Principal or Distributor).",
          variant: "destructive"
        });
        return;
      }
      if (!(formData.isWebUser || formData.isAppUser)) {
        toast({
          title: "Validation Error",
          description: "Please select at least one platform (Web or Mobile).",
          variant: "destructive"
        });
        return;
      }
      if (formData.isWebUser && selectedWebModules.length === 0) {
        toast({
          title: "Validation Error",
          description: "Please select at least one web module.",
          variant: "destructive"
        });
        return;
      }
      if (formData.isAppUser && selectedMobileModules.length === 0) {
        toast({
          title: "Validation Error",
          description: "Please select at least one mobile module.",
          variant: "destructive"
        });
        return;
      }
    }

    if (isValid && currentStep < progressSteps.length) {
      setCurrentStep(currentStep + 1);
    }
  };

  const handlePrevious = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleReset = () => {
    reset({
      code: "",
      roleNameEn: "",
      roleNameOther: "",
      parentRoleUid: "",
      orgUid: getCurrentUserOrg(),
      isPrincipalRole: false,
      isDistributorRole: false,
      isAdmin: false,
      isWebUser: true,
      isAppUser: false,
      isActive: true,
      haveWarehouse: false,
      haveVehicle: false,
      isForReportsTo: false
    });
    setCurrentStep(1);
    setSelectedWebModules([]);
    setSelectedMobileModules([]);
  };

  const handleWebModuleToggle = (moduleUID: string) => {
    setSelectedWebModules((prev) =>
      prev.includes(moduleUID)
        ? prev.filter((uid) => uid !== moduleUID)
        : [...prev, moduleUID]
    );
  };

  const handleMobileModuleToggle = (moduleUID: string) => {
    setSelectedMobileModules((prev) =>
      prev.includes(moduleUID)
        ? prev.filter((uid) => uid !== moduleUID)
        : [...prev, moduleUID]
    );
  };

  const onSubmit = async (data: RoleFormData) => {
    try {
      setLoading(true);

      const roleData = {
        code: data.code,
        roleNameEn: data.roleNameEn,
        roleNameOther: data.roleNameOther,
        parentRoleUid: data.parentRoleUid,
        orgUid: data.orgUid,
        isPrincipalRole: data.isPrincipalRole,
        isDistributorRole: data.isDistributorRole,
        isAdmin: data.isAdmin,
        isWebUser: data.isWebUser,
        isAppUser: data.isAppUser,
        isActive: data.isActive,
        haveWarehouse: data.haveWarehouse,
        haveVehicle: data.haveVehicle,
        isForReportsTo: data.isForReportsTo,
        uid: data.code,
        createdDate: new Date().toISOString()
      };

      // Use the new service to create role with permissions
      await rolePermissionService.createRoleWithPermissions(
        roleData,
        selectedWebModules,
        selectedMobileModules,
        webModules,
        mobileModules
      );

      toast({
        title: "Success",
        description: "Role created successfully with permissions!"
      });

      router.push("/administration/access-control/roles");
    } catch (error) {
      console.error("Failed to create role:", error);
      toast({
        title: "Error",
        description: "Failed to create role. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="space-y-6"
          >
            <div className="flex items-start justify-between">
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-1">
                  Basic Information
                </h2>
                <p className="text-gray-600">
                  Provide essential role information and identification details.
                </p>
              </div>
              <div className="flex items-center space-x-3">
                <Label className="text-sm font-medium text-gray-700">
                  Status:
                </Label>
                <div className="flex items-center space-x-3">
                  <Switch
                    checked={formData.isActive}
                    onCheckedChange={(checked) =>
                      setValue("isActive", !!checked)
                    }
                  />
                  <span
                    className={`text-sm font-medium px-2 py-1 rounded-md ${
                      formData.isActive
                        ? "text-green-700 bg-green-100"
                        : "text-red-700 bg-red-100"
                    }`}
                  >
                    {formData.isActive ? "Active" : "Inactive"}
                  </span>
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <Label htmlFor="code">Role Code *</Label>
                <Input
                  id="code"
                  placeholder="ROLE_MANAGER"
                  {...register("code")}
                  className={errors.code ? "border-red-300" : ""}
                  onInput={(e) => {
                    // Block spacing and convert to uppercase
                    const target = e.target as HTMLInputElement;
                    const value = target.value.replace(/\s/g, "").toUpperCase();
                    target.value = value;
                    // Trigger form update
                    form.setValue("code", value);
                  }}
                />
                {errors.code && (
                  <p className="text-sm text-red-600">{errors.code.message}</p>
                )}
                <p className="text-xs text-gray-500">
                  Unique identifier for the role (max 20 characters, no spaces)
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="roleNameEn">Role Name *</Label>
                <Input
                  id="roleNameEn"
                  placeholder="Manager"
                  {...register("roleNameEn")}
                  className={errors.roleNameEn ? "border-red-300" : ""}
                />
                {errors.roleNameEn && (
                  <p className="text-sm text-red-600">
                    {errors.roleNameEn.message}
                  </p>
                )}
                <p className="text-xs text-gray-500">
                  Display name for the role (max 100 characters)
                </p>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="roleNameOther">Alias Name</Label>
              <Input
                id="roleNameOther"
                placeholder="Alternative name for the role..."
                {...register("roleNameOther")}
              />
              <p className="text-xs text-gray-500">
                Optional alternative name for the role (max 100 characters)
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="parentRoleUid">Parent Role</Label>
              <Popover open={parentRolePopoverOpen} onOpenChange={setParentRolePopoverOpen}>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={parentRolePopoverOpen}
                    className="w-full justify-between"
                    disabled={loadingRoles}
                  >
                    {loadingRoles
                      ? "Loading roles..."
                      : formData.parentRoleUid && formData.parentRoleUid !== ""
                      ? availableRoles.find((role) => role.UID === formData.parentRoleUid)?.RoleNameEn || "Select parent role"
                      : "Select parent role (optional)"}
                    <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
                  <Command>
                    <div className="flex items-center border-b px-4 py-2">
                      <Search className="mr-3 h-4 w-4 shrink-0 opacity-50" />
                      <CommandInput 
                        placeholder="Search roles..." 
                        className="flex h-9 w-full rounded-md bg-transparent py-2 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50 border-0 focus:ring-0"
                      />
                    </div>
                    <CommandEmpty className="py-6 text-center text-sm">
                      <div className="flex flex-col items-center gap-2">
                        <Search className="h-8 w-8 text-muted-foreground/50" />
                        <p>No roles found</p>
                        <p className="text-xs text-muted-foreground">Try a different search term</p>
                      </div>
                    </CommandEmpty>
                    <CommandGroup>
                      <CommandList className="max-h-[280px] overflow-y-auto">
                        <CommandItem
                          value="No Parent Role"
                          onSelect={() => {
                            setValue("parentRoleUid", "")
                            setParentRolePopoverOpen(false)
                          }}
                          className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-accent rounded-none"
                        >
                          <div className="flex items-center gap-4 flex-1">
                            <Check
                              className={`h-4 w-4 shrink-0 ${
                                !formData.parentRoleUid || formData.parentRoleUid === "" ? "opacity-100 text-primary" : "opacity-0"
                              }`}
                            />
                            <div className="flex flex-col gap-1.5 flex-1 min-w-0">
                              <div className="font-medium text-sm text-gray-600">No Parent Role</div>
                              <div className="text-xs text-muted-foreground">
                                This role will not have a parent role
                              </div>
                            </div>
                          </div>
                        </CommandItem>
                        {availableRoles.map((role) => (
                          <CommandItem
                            key={role.UID}
                            value={`${role.RoleNameEn} ${role.Code}`}
                            onSelect={() => {
                              setValue("parentRoleUid", role.UID)
                              setParentRolePopoverOpen(false)
                            }}
                            className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-accent rounded-none"
                          >
                            <div className="flex items-center gap-4 flex-1">
                              <Check
                                className={`h-4 w-4 shrink-0 ${
                                  formData.parentRoleUid === role.UID ? "opacity-100 text-primary" : "opacity-0"
                                }`}
                              />
                              <div className="flex flex-col gap-1.5 flex-1 min-w-0">
                                <div className="font-medium text-sm truncate">{role.RoleNameEn}</div>
                                <div className="text-xs text-muted-foreground">
                                  Code: {role.Code}
                                </div>
                              </div>
                            </div>
                            <div className="flex gap-2 ml-4 shrink-0">
                              {role.IsPrincipalRole && (
                                <Badge variant="outline" className="text-[10px] px-2 py-1 h-auto">
                                  Principal
                                </Badge>
                              )}
                              {role.IsDistributorRole && (
                                <Badge variant="secondary" className="text-[10px] px-2 py-1 h-auto">
                                  Distributor
                                </Badge>
                              )}
                              {role.IsAdmin && (
                                <Badge variant="destructive" className="text-[10px] px-2 py-1 h-auto">
                                  Admin
                                </Badge>
                              )}
                            </div>
                          </CommandItem>
                        ))}
                      </CommandList>
                    </CommandGroup>
                  </Command>
                </PopoverContent>
              </Popover>
              <p className="text-xs text-gray-500">
                Select a parent role to establish hierarchy (optional).
                {loadingRoles && " Loading..."}
                {!loadingRoles &&
                  availableRoles.length === 0 &&
                  " No roles available."}
                {!loadingRoles &&
                  availableRoles.length > 0 &&
                  ` ${availableRoles.length} roles available.`}
              </p>
            </div>

          </motion.div>
        );

      case 2:
        return (
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="space-y-8"
          >
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-1">
                Permissions & Access Configuration
              </h2>
              <p className="text-gray-600">
                Configure role type, administrative privileges, platform access,
                and module permissions.
              </p>
            </div>

            {/* Role Type Section */}
            <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">
                Role Type
              </Label>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isPrincipalRole
                      ? "border-blue-500 bg-blue-50"
                      : "border-gray-200"
                  }`}
                >
                  <Checkbox
                    checked={formData.isPrincipalRole}
                    onCheckedChange={(checked) =>
                      setValue("isPrincipalRole", !!checked)
                    }
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Crown className="h-4 w-4 text-blue-600" />
                      Principal Role
                    </Label>
                    <p className="text-sm text-gray-600">
                      High-level organizational role with broad permissions
                    </p>
                  </div>
                </div>

                <div
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isDistributorRole
                      ? "border-green-500 bg-green-50"
                      : "border-gray-200"
                  }`}
                >
                  <Checkbox
                    checked={formData.isDistributorRole}
                    onCheckedChange={(checked) =>
                      setValue("isDistributorRole", !!checked)
                    }
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Building className="h-4 w-4 text-green-600" />
                      Distributor Role
                    </Label>
                    <p className="text-sm text-gray-600">
                      Role for distributors with specific business permissions
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Administrative Privileges Section */}
            <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">
                Administrative Privileges
              </Label>
              <div
                className={`flex items-center justify-between rounded-lg border p-4 transition-colors ${
                  formData.isAdmin
                    ? "border-red-500 bg-red-50"
                    : "border-gray-200"
                }`}
              >
                <div className="space-y-0.5">
                  <Label className="flex items-center gap-2 text-base">
                    <Shield className="h-4 w-4 text-red-600" />
                    Administrator Privileges
                  </Label>
                  <p className="text-sm text-gray-600">
                    Grants full administrative access to the system. Use with
                    caution.
                  </p>
                </div>
                <Switch
                  checked={formData.isAdmin}
                  onCheckedChange={(checked) => setValue("isAdmin", checked)}
                />
              </div>

              {formData.isAdmin && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                  <div className="flex items-center gap-2 text-red-800">
                    <Shield className="h-4 w-4" />
                    <span className="font-medium">
                      Warning: Administrator Role
                    </span>
                  </div>
                  <p className="text-red-700 text-sm mt-1">
                    This role will have complete administrative access to the
                    system. Only assign to trusted personnel.
                  </p>
                </div>
              )}
            </div>

            {/* Additional Configuration Section */}
            {/* <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">
                Additional Configuration
              </Label>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <Label className="text-sm">Warehouse Access</Label>
                    <p className="text-xs text-gray-600">
                      Can manage warehouses
                    </p>
                  </div>
                  <Switch
                    checked={formData.haveWarehouse}
                    onCheckedChange={(checked) =>
                      setValue("haveWarehouse", checked)
                    }
                  />
                </div>

                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <Label className="text-sm">Vehicle Access</Label>
                    <p className="text-xs text-gray-600">Can manage vehicles</p>
                  </div>
                  <Switch
                    checked={formData.haveVehicle}
                    onCheckedChange={(checked) =>
                      setValue("haveVehicle", checked)
                    }
                  />
                </div>

                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <Label className="text-sm">Reports To Access</Label>
                    <p className="text-xs text-gray-600">
                      Can configure reporting structure
                    </p>
                  </div>
                  <Switch
                    checked={formData.isForReportsTo}
                    onCheckedChange={(checked) =>
                      setValue("isForReportsTo", checked)
                    }
                  />
                </div>
              </div>
            </div> */}

            {/* Platform Access Section */}
            <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">
                Platform Access
              </Label>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isWebUser
                      ? "border-blue-500 bg-blue-50"
                      : "border-gray-200"
                  }`}
                >
                  <Checkbox
                    checked={formData.isWebUser}
                    onCheckedChange={(checked) =>
                      setValue("isWebUser", !!checked)
                    }
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Monitor className="h-4 w-4 text-blue-600" />
                      Web Application
                    </Label>
                    <p className="text-sm text-gray-600">
                      Access to the web-based administrative interface
                    </p>
                  </div>
                </div>

                <div
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isAppUser
                      ? "border-green-500 bg-green-50"
                      : "border-gray-200"
                  }`}
                >
                  <Checkbox
                    checked={formData.isAppUser}
                    onCheckedChange={(checked) =>
                      setValue("isAppUser", !!checked)
                    }
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Smartphone className="h-4 w-4 text-green-600" />
                      Mobile Application
                    </Label>
                    <p className="text-sm text-gray-600">
                      Access to the mobile application interface
                    </p>
                  </div>
                </div>
              </div>

              {(formData.isWebUser || formData.isAppUser) && (
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <div className="flex items-center gap-2 text-blue-800">
                    <Monitor className="h-4 w-4" />
                    <span className="font-medium">
                      Platform Access Configured
                    </span>
                  </div>
                  <p className="text-blue-700 text-sm mt-1">
                    {formData.isWebUser && formData.isAppUser
                      ? "This role will have access to both web and mobile platforms."
                      : formData.isWebUser
                      ? "This role will have access to the web platform only."
                      : "This role will have access to the mobile platform only."}
                  </p>
                </div>
              )}
            </div>

            {/* Module Access Section */}
            {/* <div className="space-y-6">
              <Label className="text-lg font-medium text-gray-900">
                Module Access
              </Label>

              {formData.isWebUser && (
                <div className="space-y-4">
                  <div className="flex items-center gap-2">
                    <Monitor className="h-5 w-5 text-blue-600" />
                    <Label className="text-base font-medium">
                      Web Platform Modules
                    </Label>
                    <Badge variant="secondary">
                      {selectedWebModules.length} selected
                    </Badge>
                  </div>
                  <div className="space-y-4 max-h-96 overflow-y-auto border rounded-lg p-4">
                    {(webModules || []).map((module) => (
                      <div key={module.UID} className="space-y-2">
                        <div className="font-medium text-sm text-gray-900 border-b pb-1">
                          {module.ModuleNameEn}
                        </div>
                        {module.children?.map((subModule) => (
                          <div key={subModule.UID} className="ml-4 space-y-1">
                            <div className="font-medium text-xs text-gray-700">
                              {subModule.SubModuleNameEn}
                            </div>
                            {subModule.children?.map((page) => (
                              <div
                                key={page.UID}
                                className={`ml-4 flex items-center space-x-2 p-2 rounded transition-colors ${
                                  selectedWebModules.includes(page.UID)
                                    ? "border border-blue-500 bg-blue-50"
                                    : "hover:bg-gray-50"
                                }`}
                              >
                                <Checkbox
                                  checked={selectedWebModules.includes(
                                    page.UID
                                  )}
                                  onCheckedChange={() =>
                                    handleWebModuleToggle(page.UID)
                                  }
                                />
                                <div className="flex-1">
                                  <Label className="text-xs cursor-pointer">
                                    {page.SubSubModuleNameEn}
                                  </Label>
                                </div>
                              </div>
                            ))}
                          </div>
                        ))}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {formData.isAppUser && (
                <div className="space-y-4">
                  <div className="flex items-center gap-2">
                    <Smartphone className="h-5 w-5 text-green-600" />
                    <Label className="text-base font-medium">
                      Mobile Platform Modules
                    </Label>
                    <Badge variant="secondary">
                      {selectedMobileModules.length} selected
                    </Badge>
                  </div>
                  <div className="space-y-4 max-h-96 overflow-y-auto border rounded-lg p-4">
                    {(mobileModules || []).map((module) => (
                      <div key={module.UID} className="space-y-2">
                        <div className="font-medium text-sm text-gray-900 border-b pb-1">
                          {module.ModuleNameEn}
                        </div>
                        {module.children?.map((subModule) => (
                          <div key={subModule.UID} className="ml-4 space-y-1">
                            <div className="font-medium text-xs text-gray-700">
                              {subModule.SubModuleNameEn}
                            </div>
                            {subModule.children?.map((page) => (
                              <div
                                key={page.UID}
                                className={`ml-4 flex items-center space-x-2 p-2 rounded transition-colors ${
                                  selectedMobileModules.includes(page.UID)
                                    ? "border border-green-500 bg-green-50"
                                    : "hover:bg-gray-50"
                                }`}
                              >
                                <Checkbox
                                  checked={selectedMobileModules.includes(
                                    page.UID
                                  )}
                                  onCheckedChange={() =>
                                    handleMobileModuleToggle(page.UID)
                                  }
                                />
                                <div className="flex-1">
                                  <Label className="text-xs cursor-pointer">
                                    {page.SubSubModuleNameEn}
                                  </Label>
                                </div>
                              </div>
                            ))}
                          </div>
                        ))}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {!formData.isWebUser && !formData.isAppUser && (
                <div className="text-center py-8 text-gray-500 border rounded-lg">
                  <p>
                    Please select at least one platform above to configure
                    module access.
                  </p>
                </div>
              )}
            </div> */}
          </motion.div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen">
      {/* Header */}
      <div className="px-6 py-5 border-b">
        <div className="max-w-6xl">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-semibold text-gray-900">
                Create Role
              </h1>
              <p className="text-sm text-gray-600 mt-1">
                Define a new role with permissions and access configuration
              </p>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={() =>
                router.push("/administration/access-control/roles")
              }
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Roles
            </Button>
          </div>
        </div>
      </div>

      {/* Progress Section */}
      <div className="px-6 py-4 border-b">
        <div className="flex items-center justify-between max-w-6xl">
          <div className="flex items-center flex-1">
            {progressSteps.map((step, index) => (
              <React.Fragment key={step.num}>
                <div className="flex items-center">
                  <div
                    className={cn(
                      "w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-all",
                      currentStep > step.num
                        ? "bg-blue-600 text-white"
                        : currentStep === step.num
                        ? "bg-blue-600 text-white ring-2 ring-blue-300 ring-offset-2"
                        : "bg-gray-200 text-gray-500"
                    )}
                  >
                    {currentStep > step.num ? (
                      <CheckCircle2 className="h-4 w-4" />
                    ) : (
                      <span>{step.num}</span>
                    )}
                  </div>
                  <span
                    className={cn(
                      "ml-2 text-sm hidden md:inline",
                      currentStep >= step.num
                        ? "text-gray-900 font-medium"
                        : "text-gray-500"
                    )}
                  >
                    {step.label}
                  </span>
                </div>
                {index < progressSteps.length - 1 && (
                  <div className="flex-1 mx-4">
                    <div className="h-0.5 bg-gray-200">
                      <div
                        className="h-full bg-blue-600 transition-all duration-300"
                        style={{
                          width: currentStep > step.num ? "100%" : "0%"
                        }}
                      />
                    </div>
                  </div>
                )}
              </React.Fragment>
            ))}
          </div>
        </div>
      </div>

      {/* Form Content */}
      <div className="px-6 pb-8">
        <div className="max-w-6xl">
          <div className="p-6">
            <form>
              <AnimatePresence mode="wait">
                {renderStepContent()}
              </AnimatePresence>

              {/* Navigation */}
              <div className="border-t pt-6 mt-8">
                <div className="flex items-center justify-between">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={handlePrevious}
                    disabled={currentStep === 1}
                    className={cn("h-10", currentStep === 1 && "invisible")}
                  >
                    <ArrowLeft className="h-4 w-4 mr-2" />
                    Previous
                  </Button>

                  <div className="flex items-center gap-3">
                    <Button
                      type="button"
                      variant="ghost"
                      onClick={handleReset}
                      disabled={loading}
                      className="h-10"
                    >
                      <RotateCcw className="h-4 w-4 mr-2" />
                      Reset
                    </Button>

                    {currentStep < progressSteps.length ? (
                      <Button
                        type="button"
                        onClick={handleNext}
                        disabled={loading}
                        className="h-10"
                      >
                        Next
                        <ArrowRight className="h-4 w-4 ml-2" />
                      </Button>
                    ) : (
                      <Button
                        type="button"
                        onClick={handleSubmit(onSubmit)}
                        disabled={loading}
                        className="h-10"
                      >
                        {loading ? (
                          <>
                            <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                            Creating...
                          </>
                        ) : (
                          <>Create Role</>
                        )}
                      </Button>
                    )}
                  </div>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
