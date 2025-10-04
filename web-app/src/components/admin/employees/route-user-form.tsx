"use client";

import React, { useState, useEffect } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import * as z from "zod";
import {
  Loader2,
  Copy,
  RefreshCw,
  Mail,
  Building,
  Shield,
  IdCard,
  MapPin,
  Globe,
  Home,
  Eye,
  EyeOff,
  X,
  Check,
  ChevronsUpDown,
} from "lucide-react";
import { format } from "date-fns";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { PhoneInput } from "@/components/ui/phone-input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
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
import { useToast } from "@/components/ui/use-toast";
import { Switch } from "@/components/ui/switch";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { authService } from "@/lib/auth-service";
import { Skeleton } from "@/components/ui/skeleton";

// Import services
import { roleService } from "@/services/admin/role.service";
import {
  organizationService,
  Organization,
  OrgType,
} from "@/services/organizationService";
import { employeeService } from "@/services/admin/employee.service";
import {
  locationService,
  Location,
  LocationType,
} from "@/services/locationService";
import { mobileAppActionService } from "@/services/mobileAppActionService";
import { listItemService, ListItem } from "@/services/listItemService";

// Import organization hierarchy utilities
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
  getOrganizationDisplayName,
} from "@/utils/organizationHierarchyUtils";

// Import location hierarchy utilities
import {
  handleLocationSelection,
  resetLocationHierarchy,
  getFinalSelectedLocation,
  initializeLocationHierarchy,
  LocationLevel,
  getLocationDisplayName,
} from "@/utils/locationHierarchy";

// Define the form schema based on web portal UserFormData structure
const routeUserFormSchema = z
  .object({
    // Basic Information (Required fields)
    code: z.string().min(1, "User code is required"),
    name: z.string().min(1, "Name is required"),
    userRoleUID: z.string().min(1, "Role is required"),
    departmentUID: z.string().min(1, "Department is required"),
    orgUID: z.string().min(1, "Sales Org is required"),

    // Fields with defaults (always required)
    isActive: z.boolean(),
    authType: z.string(),
    status: z.string(),
    canHandleStock: z.boolean(),
    hasEOT: z.boolean(),
    mobileAppAccess: z.boolean(),

    // Optional fields
    email: z
      .string()
      .email("Invalid email address")
      .optional()
      .or(z.literal("")),
    phone: z.string().optional(),
    // routeUID: z.string().optional(),
    // billToCustomer: z.string().optional(),
    reportsToUID: z.string().optional(),
    aliasName: z.string().optional(),
    loginId: z.string().optional(),
    empNo: z.string().optional(),
    branchUID: z.string().optional(),
    salesOfficeUID: z.string().optional(),
    collectionLimit: z.number().optional(),
    designation: z.string().optional(),
    applicableOrgs: z.array(z.string()).optional(),
    assignedRoutes: z.array(z.string()).optional(),
    // Location assignment fields
    locationUID: z.string().optional(),
    locationType: z.string().optional(),
    locationValue: z.string().optional(),
    password: z
      .string()
      .min(6, "Password must be at least 6 characters")
      .optional()
      .or(z.literal("")),
    confirmPassword: z.string().optional().or(z.literal("")),
    startDate: z.string().optional(),
    routeFromDate: z.string().optional(),
    routeToDate: z.string().optional(),
  })
  .refine(
    (data) => {
      // If password is provided, confirm password is required and must match
      if (data.password) {
        if (!data.confirmPassword) {
          return false; // confirmPassword required when password is provided
        }
        return data.password === data.confirmPassword;
      }
      // If no password provided, confirm password should also be empty
      return true;
    },
    {
      message: "Please confirm your password",
      path: ["confirmPassword"],
    }
  )
  .refine(
    (data) => {
      // Check if passwords match when both are provided
      if (data.password && data.confirmPassword) {
        return data.password === data.confirmPassword;
      }
      return true;
    },
    {
      message: "Passwords do not match",
      path: ["confirmPassword"],
    }
  );

type RouteUserFormData = z.infer<typeof routeUserFormSchema>;

// Interfaces matching web portal exactly
interface RouteUserFormProps {
  user?: any | null;
  employeeId?: string;
  onSuccess: () => void;
  onCancel: () => void;
  isModal?: boolean;
}

interface SelectionItem {
  uid: string;
  code: string;
  label: string;
  value: string;
}

// UserType interface removed - not needed anymore

interface Route {
  uid: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

interface LocationItem {
  uid: string;
  code: string;
  name: string;
  locationTypeUID?: string;
  locationTypeName?: string;
  parentUID?: string | null;
  itemLevel?: number;
  hasChild?: boolean;
}

// Location Assignment Component (Simplified version of RouteUserLocationAssignment)
const LocationAssignmentComponent: React.FC<{
  selectedCountries: string[];
  selectedDepots: string[];
  selectedSubAreas: string[];
  onCountryChange: (countryUID: string, checked: boolean) => void;
  onDepotChange: (depotUID: string, checked: boolean) => void;
  onSubAreaChange: (subAreaUID: string, checked: boolean) => void;
  locations: LocationItem[];
}> = ({
  selectedCountries,
  selectedDepots,
  selectedSubAreas,
  onCountryChange,
  onDepotChange,
  onSubAreaChange,
  locations,
}) => {
  // Get location hierarchy functions
  const getCountries = (): LocationItem[] => {
    return locations.filter((loc) => loc.locationTypeUID === "Country");
  };

  const getDepots = (): LocationItem[] => {
    if (selectedCountries.length === 0) return [];
    return locations.filter(
      (loc) =>
        loc.locationTypeUID === "Region" &&
        selectedCountries.includes(loc.parentUID || "")
    );
  };

  const getSubAreas = (): LocationItem[] => {
    if (selectedDepots.length === 0) return [];
    return locations.filter(
      (loc) =>
        loc.locationTypeUID === "City" &&
        selectedDepots.includes(loc.parentUID || "")
    );
  };

  const countries = getCountries();
  const depots = getDepots();
  const subAreas = getSubAreas();

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center text-blue-600">
          <Globe className="h-5 w-5 mr-2" />
          Assign Country, Region And City
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Countries Section */}
        {countries.length > 0 && (
          <div>
            <div className="bg-blue-600 text-white p-3 rounded-md flex items-center justify-between mb-4">
              <span className="font-semibold flex items-center">
                <Globe className="h-4 w-4 mr-2" />
                Select Country
              </span>
              {selectedCountries.length > 0 && (
                <Badge variant="secondary" className="bg-white text-blue-600">
                  {selectedCountries.length} selected
                </Badge>
              )}
            </div>

            <div className="space-y-2">
              {countries.map((country) => (
                <div
                  key={country.uid}
                  className={`p-3 border rounded-md flex items-center justify-between ${
                    selectedCountries.includes(country.uid)
                      ? "bg-blue-50 border-blue-200"
                      : ""
                  }`}
                >
                  <div className="flex items-center">
                    <Checkbox
                      checked={selectedCountries.includes(country.uid)}
                      onCheckedChange={(checked) =>
                        onCountryChange(country.uid, !!checked)
                      }
                    />
                    <span className="ml-2 font-medium">{country.name}</span>
                  </div>
                  <Button size="sm" variant="secondary">
                    <Eye className="h-4 w-4 mr-1" />
                    VIEW
                  </Button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Depots/Regions Section */}
        {selectedCountries.length > 0 && (
          <div>
            <div className="bg-green-600 text-white p-3 rounded-md flex items-center justify-between mb-4">
              <span className="font-semibold flex items-center">
                <Building className="h-4 w-4 mr-2" />
                Select Region
              </span>
              {selectedDepots.length > 0 && (
                <Badge variant="secondary" className="bg-white text-green-600">
                  {selectedDepots.length} selected
                </Badge>
              )}
            </div>

            <div className="space-y-2">
              {depots.length > 0 ? (
                depots.map((depot) => (
                  <div
                    key={depot.uid}
                    className={`p-3 border rounded-md flex items-center justify-between ${
                      selectedDepots.includes(depot.uid)
                        ? "bg-green-50 border-green-200"
                        : ""
                    }`}
                  >
                    <div className="flex items-center">
                      <Checkbox
                        checked={selectedDepots.includes(depot.uid)}
                        onCheckedChange={(checked) =>
                          onDepotChange(depot.uid, !!checked)
                        }
                      />
                      <span className="ml-2">
                        [{depot.code}] {depot.name}
                      </span>
                    </div>
                    <Button size="sm" variant="secondary">
                      <Eye className="h-4 w-4 mr-1" />
                      VIEW
                    </Button>
                  </div>
                ))
              ) : (
                <div className="text-center py-8 text-gray-500">
                  No regions available for selected countries
                </div>
              )}
            </div>
          </div>
        )}

        {/* Sub Areas/Cities Section */}
        {selectedDepots.length > 0 && (
          <div>
            <div className="bg-orange-600 text-white p-3 rounded-md flex items-center justify-between mb-4">
              <span className="font-semibold flex items-center">
                <Home className="h-4 w-4 mr-2" />
                Select City
              </span>
              {selectedSubAreas.length > 0 && (
                <Badge variant="secondary" className="bg-white text-orange-600">
                  {selectedSubAreas.length} selected
                </Badge>
              )}
            </div>

            <div className="space-y-2">
              {subAreas.length > 0 ? (
                subAreas.map((subArea) => (
                  <div
                    key={subArea.uid}
                    className={`p-3 border rounded-md flex items-center justify-between ${
                      selectedSubAreas.includes(subArea.uid)
                        ? "bg-orange-50 border-orange-200"
                        : ""
                    }`}
                  >
                    <div className="flex items-center">
                      <Checkbox
                        checked={selectedSubAreas.includes(subArea.uid)}
                        onCheckedChange={(checked) =>
                          onSubAreaChange(subArea.uid, !!checked)
                        }
                      />
                      <span className="ml-2">
                        [{subArea.code}] {subArea.name}
                      </span>
                    </div>
                    <Button size="sm" variant="secondary">
                      <Eye className="h-4 w-4 mr-1" />
                      VIEW
                    </Button>
                  </div>
                ))
              ) : (
                <div className="text-center py-8 text-gray-500">
                  No cities available for selected regions
                </div>
              )}
            </div>
          </div>
        )}

        {/* Summary */}
        {(selectedCountries.length > 0 ||
          selectedDepots.length > 0 ||
          selectedSubAreas.length > 0) && (
          <div className="bg-gray-50 p-4 rounded-md">
            <div className="text-sm text-gray-600">
              <strong>Selected: </strong>
              {selectedCountries.length} Countries, {selectedDepots.length}{" "}
              Regions, {selectedSubAreas.length} Cities
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export function RouteUserForm({
  user,
  employeeId,
  onSuccess,
  onCancel,
  isModal = false,
}: RouteUserFormProps) {
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [loadingMasterData, setLoadingMasterData] = useState(true);

  // Master data states
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]);
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([]);
  const [userRoles, setUserRoles] = useState<SelectionItem[]>([]);
  // userTypes state removed - not needed anymore
  const [departments, setDepartments] = useState<SelectionItem[]>([]);
  const [routes, setRoutes] = useState<Route[]>([]);
  const [reportToUsers, setReportToUsers] = useState<SelectionItem[]>([]);
  const [filteredReportToUsers, setFilteredReportToUsers] = useState<
    SelectionItem[]
  >([]);
  // UI states
  const [activeTab, setActiveTab] = useState("userDetail");
  const [selectedOrgUID, setSelectedOrgUID] = useState<string>("");
  const [copyLocationVisible, setCopyLocationVisible] = useState(false);
  const [selectedSourceEmployee, setSelectedSourceEmployee] = useState("");
  const [employeeSearchQuery, setEmployeeSearchQuery] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [authTypes, setAuthTypes] = useState<ListItem[]>([]);

  // Location assignment states
  const [employeeData, setEmployeeData] = useState<any>(user || null);
  const [loadingEmployee, setLoadingEmployee] = useState(false);
  const [existingEncryptedPassword, setExistingEncryptedPassword] =
    useState<string>("");

  // Location hierarchy states
  const [locations, setLocations] = useState<Location[]>([]);
  const [locationTypes, setLocationTypes] = useState<LocationType[]>([]);
  const [locationLevels, setLocationLevels] = useState<LocationLevel[]>([]);
  const [selectedLocations, setSelectedLocations] = useState<string[]>([]);
  const [selectedLocationUID, setSelectedLocationUID] = useState<string>("");
  const [existingJobPositionUID, setExistingJobPositionUID] =
    useState<string>("");
  const [pendingLocationSetup, setPendingLocationSetup] = useState<{
    locationType: string;
    locationValue: string;
  } | null>(null);
  const [pendingOrgSetup, setPendingOrgSetup] = useState<string>("");
  const [pendingDeptSetup, setPendingDeptSetup] = useState<string>("");

  // Searchable dropdown states
  const [openRole, setOpenRole] = useState(false);
  const [openReportTo, setOpenReportTo] = useState(false);
  const [openDepartment, setOpenDepartment] = useState(false);

  const isEdit = !!(user || employeeId);

  const form = useForm<RouteUserFormData>({
    resolver: zodResolver(routeUserFormSchema),
    mode: "onChange", // Enable validation on change to show errors immediately
    defaultValues: {
      code: "",
      name: "",
      userRoleUID: "",
      departmentUID: "",
      orgUID: "",
      isActive: true,
      email: "",
      phone: "",
      // routeUID: "",
      // billToCustomer: "",
      reportsToUID: "",
      aliasName: "",
      confirmPassword: "",
      loginId: "",
      empNo: "",
      authType: "Local",
      status: "Active",
      branchUID: "",
      salesOfficeUID: "",
      collectionLimit: 0,
      canHandleStock: false,
      designation: "Employee",
      hasEOT: true,
      applicableOrgs: [],
      assignedRoutes: [],
      password: "",
      startDate: format(new Date(), "yyyy-MM-dd"),
      routeFromDate: "",
      routeToDate: "",
      mobileAppAccess: false,
      // Location default values
      locationUID: "",
      locationType: "",
      locationValue: "",
    },
  });

  // Load master data on component mount
  useEffect(() => {
    const initializeData = async () => {
      // Load master data and employee data in parallel
      const promises = [loadMasterData()];

      // Add employee data loading to parallel promises
      if (user) {
        promises.push(
          Promise.resolve().then(() => {
            loadUserDetails(user.uid);
            setEmployeeData(user);
          })
        );
      } else if (employeeId) {
        promises.push(loadEmployeeById(employeeId));
      }

      // Wait for all data to load in parallel
      await Promise.allSettled(promises);
    };

    initializeData();
  }, [user, employeeId]);

  // Handle pending location setup when master data is loaded
  useEffect(() => {
    if (
      pendingLocationSetup &&
      locations.length > 0 &&
      locationTypes.length > 0
    ) {
      const { locationType, locationValue } = pendingLocationSetup;

      // Find the location UID by matching the location value (name)
      const matchedLocation = locations.find(
        (loc) =>
          loc.Name === locationValue && loc.LocationTypeName === locationType
      );

      if (!matchedLocation) {
        // Fallback: Try to find by just name if type doesn't match exactly
        const fallbackLocation = locations.find(
          (loc) => loc.Name === locationValue
        );
        if (fallbackLocation) {
          const locationUID = fallbackLocation.UID;
          setSelectedLocationUID(locationUID);
          form.setValue("locationUID", locationUID);
          setupLocationHierarchy(locationUID);
          setPendingLocationSetup(null); // Clear pending setup
        } else {
          setPendingLocationSetup(null); // Clear pending setup even on failure
        }
      } else {
        const locationUID = matchedLocation.UID;
        setSelectedLocationUID(locationUID);
        form.setValue("locationUID", locationUID);
        setupLocationHierarchy(locationUID);
        setPendingLocationSetup(null); // Clear pending setup
      }
    }
  }, [pendingLocationSetup, locations, locationTypes, form]);

  // Handle pending organization setup when master data is loaded
  useEffect(() => {
    if (pendingOrgSetup && organizations.length > 0 && orgTypes.length > 0) {
      const selectedOrg = organizations.find(
        (org) => org.UID === pendingOrgSetup
      );
      if (selectedOrg) {
        form.setValue("orgUID", pendingOrgSetup);
        setSelectedOrgUID(pendingOrgSetup);
        setupOrganizationHierarchy(pendingOrgSetup);
        setPendingOrgSetup(""); // Clear pending setup
      } else {
        setPendingOrgSetup(""); // Clear pending setup even on failure
      }
    }
  }, [pendingOrgSetup, organizations, orgTypes, form]);

  // Handle pending department setup when master data is loaded
  useEffect(() => {
    if (pendingDeptSetup && departments.length > 0) {
      // Find department by name/code
      const dept = departments.find(
        (d) =>
          d.code?.toLowerCase() === pendingDeptSetup.toLowerCase() ||
          d.label?.toLowerCase() === pendingDeptSetup.toLowerCase()
      );

      if (dept) {
        form.setValue("departmentUID", dept.uid);
        setPendingDeptSetup(""); // Clear pending setup
      } else {
        // Try to use as-is if it's already a UID
        if (pendingDeptSetup.startsWith("dept-")) {
          form.setValue("departmentUID", pendingDeptSetup);
        }
        setPendingDeptSetup(""); // Clear pending setup even on failure
      }
    }
  }, [pendingDeptSetup, departments, form]);

  // Load routes when organization changes
  useEffect(() => {
    if (selectedOrgUID) {
      loadRoutesForOrganization(selectedOrgUID);
    }
  }, [selectedOrgUID]);

  // Track if this is the initial load for edit mode
  const [isInitialLoad, setIsInitialLoad] = useState(true);

  // Auto-set mobile app access based on selected role's IsAppUser flag
  useEffect(() => {
    const checkRoleMobileAccess = async () => {
      const selectedRoleUID = form.watch("userRoleUID");

      if (!selectedRoleUID) {
        return;
      }

      // For edit mode, skip auto-configuration on initial load
      // but allow it when user changes the role
      if (isEdit && isInitialLoad) {
        setIsInitialLoad(false);
        return;
      }

      try {
        // Fetch full role details to check IsAppUser flag
        const roleDetails = await roleService.getRoleById(selectedRoleUID);

        if (roleDetails) {
          // Check the IsAppUser property from the role
          const shouldHaveMobileAccess = roleDetails.IsAppUser === true;

          // Auto-set mobile access based on role's IsAppUser flag
          const currentMobileAccess = form.getValues("mobileAppAccess");
          if (currentMobileAccess !== shouldHaveMobileAccess) {
            form.setValue("mobileAppAccess", shouldHaveMobileAccess);

            // Only show toast for role changes, not initial load
            if (!isInitialLoad) {
              if (shouldHaveMobileAccess) {
                toast({
                  title: "Mobile Access Updated",
                  description: `Mobile app access has been automatically enabled based on the ${
                    roleDetails.RoleNameEn || roleDetails.Code
                  } role configuration.`,
                  variant: "default",
                });
              } else {
                toast({
                  title: "Mobile Access Updated",
                  description: `Mobile app access has been automatically disabled based on the ${
                    roleDetails.RoleNameEn || roleDetails.Code
                  } role configuration.`,
                  variant: "default",
                });
              }
            }
          }
        }
      } catch (error) {
        // Don't show error to user, just log it
      }
    };

    checkRoleMobileAccess();
  }, [form.watch("userRoleUID"), isEdit, isInitialLoad]);

  // Show all employees in Reports To dropdown - no filtering needed
  useEffect(() => {
    // Always show all employees regardless of role or organization selection
    setFilteredReportToUsers(reportToUsers);
  }, [reportToUsers]);

  const loadAuthTypes = async () => {
    try {
      const authTypeList = await listItemService.getAuthTypes();
      // Filter out any invalid entries
      const validAuthTypes = authTypeList.filter(
        (item) => item && (item.code || item.uid)
      );
      setAuthTypes(validAuthTypes);
    } catch (error) {
      console.error("Failed to load auth types:", error);
      // Keep auth types empty if API fails
      setAuthTypes([]);
    }
  };

  const loadMasterData = async () => {
    try {
      // Load all APIs in parallel with proper error handling
      const results = await Promise.allSettled([
        loadOrganizations(),
        loadRoles(),
        loadDepartments(),
        loadReportToUsers(),
        loadLocations(),
        loadAuthTypes(),
      ]);

      results.forEach((result, index) => {
        const dataTypes = [
          "organizations",
          "roles",
          "departments",
          "reportToUsers",
          "locations",
          "authTypes",
        ];
        if (result.status === "rejected") {
        } else {
        }
      });

      // Re-populate form selections immediately
      if (employeeData && isEdit) {
        repopulateFormSelections(employeeData);
      }
      // Set loading to false after all data is loaded
      setLoadingMasterData(false);
    } catch (error) {
      toast({
        title: "Warning",
        description:
          "Failed to load some master data. Some features may not work properly.",
        variant: "destructive",
      });
      setLoadingMasterData(false);
    }
  };

  // Helper function to re-populate form selections after master data loads
  const repopulateFormSelections = (data: any) => {
    if (data.JobPosition) {
      // Store the existing JobPosition UID for updates
      setExistingJobPositionUID(
        data.JobPosition.UID || data.JobPosition.uid || ""
      );

      // Re-populate role selection
      const roleUID = data.JobPosition.UserRoleUID || "";
      if (roleUID) {
        let role = userRoles.find((r) => r.uid === roleUID);
        if (!role && roleUID) {
          role = userRoles.find(
            (r) =>
              r.label?.toLowerCase() === roleUID.toLowerCase() ||
              r.code?.toLowerCase() === roleUID.toLowerCase()
          );
        }

        if (role) {
          form.setValue("userRoleUID", role.uid);
        } else if (userRoles.length > 0) {
        }
      }

      // Re-populate department selection
      const deptName = data.JobPosition.Department || "";
      if (deptName) {
        if (departments.length > 0) {
          // Departments already loaded, find and set immediately

          // Try to find by code or label (not by uid since deptName is "Operations" not "dept-Operations")
          let dept = departments.find(
            (d) =>
              d.code?.toLowerCase() === deptName.toLowerCase() ||
              d.label?.toLowerCase() === deptName.toLowerCase()
          );

          if (!dept && deptName) {
            // Fallback: try to find by uid
            dept = departments.find((d) => d.uid === deptName);
          }

          if (dept) {
            form.setValue("departmentUID", dept.uid);
          } else {
            // Set pending for retry
            setPendingDeptSetup(deptName);
          }
        } else {
          // Departments not loaded yet, set pending
          setPendingDeptSetup(deptName);
        }
      }

      // Re-populate reports-to selection
      const reportsToUID = data.JobPosition.ReportsToUID || "";
      if (reportsToUID) {
        let reportsTo = reportToUsers.find((u) => u.uid === reportsToUID);
        if (!reportsTo && reportsToUID) {
          reportsTo = reportToUsers.find(
            (u) =>
              u.code === reportsToUID || u.label?.includes(`[${reportsToUID}]`)
          );
        }

        if (reportsTo) {
          form.setValue("reportsToUID", reportsTo.uid);
        } else if (reportToUsers.length > 0) {
        }
      }

      // Re-populate organization hierarchy
      const orgUID = data.JobPosition.OrgUID || "";
      if (orgUID) {
        form.setValue("orgUID", orgUID);
        setSelectedOrgUID(orgUID);

        if (organizations.length > 0 && orgTypes.length > 0) {
          // Organizations already loaded, setup immediately
          setupOrganizationHierarchy(orgUID);
        } else {
          // Set pending setup to be processed when master data loads
          setPendingOrgSetup(orgUID);
        }
      }
    }
  };

  const loadOrganizations = async () => {
    try {
      // Load organizations
      const orgsResponse = await organizationService.getOrganizations(1, 1000);
      const activeOrgs = orgsResponse.data.filter((org) => org.IsActive);
      setOrganizations(activeOrgs);

      // Load organization types
      const typesResult = await organizationService.getOrganizationTypes(
        1,
        1000
      );
      setOrgTypes(typesResult);

      // Initialize organization hierarchy
      const initialLevels = initializeOrganizationHierarchy(
        activeOrgs,
        typesResult
      );
      setOrgLevels(initialLevels);
    } catch (error) {
      setOrganizations([]);
      setOrgTypes([]);
      setOrgLevels([]);
    }
  };

  const loadRoles = async () => {
    try {
      const rolePagingRequest = roleService.buildRolePagingRequest(1, 100);
      const rolesResponse = await roleService.getRoles(rolePagingRequest);

      if (rolesResponse && rolesResponse.pagedData) {
        const processedRoles = rolesResponse.pagedData
          .filter((role: any) => role.IsActive)
          .map((role: any) => ({
            uid: role.UID,
            code: role.Code,
            label: role.RoleNameEn || role.Code,
            value: role.UID,
          }));
        setUserRoles(processedRoles);
      } else {
        setUserRoles([]);
      }
    } catch (error: any) {
      // Check if it's an authentication error
      if (
        error?.status === 401 ||
        error?.message?.includes("Authentication failed")
      ) {
      } else if (error?.status === 404) {
      } else {
      }

      setUserRoles([]);
    }
  };

  const loadDepartments = async () => {
    try {
      const departmentList = await listItemService.getDepartments();
      // Convert to the expected format
      const formattedDepartments = departmentList.map((dept) => ({
        uid: dept.code, // Use code as UID for consistency with backend
        code: dept.code,
        label: dept.name,
        value: dept.code,
      }));
      setDepartments(formattedDepartments);
    } catch (error) {
      console.error("Failed to load departments:", error);
      setDepartments([]);
    }
  };

  const loadReportToUsers = async () => {
    try {
      const pagingRequest = employeeService.buildPagingRequest(1, 100); // Reduced from 1000 to 100
      const employeesResponse = await employeeService.getEmployees(
        pagingRequest
      );

      if (employeesResponse && employeesResponse.pagedData) {
        const processedEmployees = employeesResponse.pagedData.map(
          (emp: any) => ({
            uid: emp.UID || emp.uid,
            code: emp.Code || emp.code,
            label: `[${emp.Code || emp.code}] ${emp.Name || emp.name}`,
            value: emp.UID || emp.uid,
          })
        );
        setReportToUsers(processedEmployees);
        // Initialize filtered list with all users
        setFilteredReportToUsers(processedEmployees);
      } else {
        setReportToUsers([]);
        setFilteredReportToUsers([]);
      }
    } catch (error: any) {
      // Check if it's an authentication error
      if (
        error?.status === 401 ||
        error?.message?.includes("Authentication failed")
      ) {
      } else if (error?.status === 404) {
      } else {
      }

      setReportToUsers([]);
      setFilteredReportToUsers([]);
    }
  };

  const loadLocations = async () => {
    try {
      // Load location types and locations in parallel
      const [locationTypesResult, locationsResult] = await Promise.allSettled([
        locationService.getLocationTypes(),
        locationService.getLocations(1, 500), // Reduced from 5000 to 500
      ]);

      // Handle location types
      if (locationTypesResult.status === "fulfilled") {
        const types = locationTypesResult.value;
        setLocationTypes(types);

        // Initialize location hierarchy
        const locationData =
          locationsResult.status === "fulfilled"
            ? locationsResult.value.data
            : [];
        const initialLevels = initializeLocationHierarchy(locationData, types);
        initialLevels.forEach((level, index) => {});
        setLocationLevels(initialLevels);
        setSelectedLocations([]);
      } else {
        setLocationTypes([]);
      }

      // Handle locations
      if (locationsResult.status === "fulfilled") {
        const locationData = locationsResult.value.data;
        setLocations(locationData);

        // Debug: Check location structure and hierarchy
        const locationsByType = new Map();
        locationData.forEach((loc) => {
          if (!locationsByType.has(loc.LocationTypeName)) {
            locationsByType.set(loc.LocationTypeName, []);
          }
          locationsByType.get(loc.LocationTypeName).push(loc);
        });

        // Debug: Check parent-child relationships
        const withParents = locationData.filter((loc) => loc.ParentUID);
        const withoutParents = locationData.filter(
          (loc) => !loc.ParentUID || loc.ParentUID === ""
        );
      } else {
        setLocations([]);
      }
    } catch (error) {
      setLocations([]);
      setLocationTypes([]);
      setLocationLevels([]);
      setSelectedLocations([]);
    }
  };

  const loadRoutesForOrganization = async (orgUID: string) => {
    try {
      // Use the existing dropdown API service
      const { api } = await import("@/services/api");

      // Call the dropdown API for routes using the existing service method
      const response = await api.dropdown.getRoute(orgUID);

      if ((response as any).IsSuccess && (response as any).Data) {
        const routeData = Array.isArray((response as any).Data)
          ? (response as any).Data
          : [];

        // Convert to the format expected by the form
        const formattedRoutes = routeData.map((route: any) => ({
          uid: route.UID,
          code: route.Code,
          name: route.Label,
          description: route.Label,
          isActive: true,
        }));

        setRoutes(formattedRoutes);
      } else {
        setRoutes([]);
      }
    } catch (error) {
      setRoutes([]);
    }
  };

  // Handle organization hierarchy selection
  const handleOrganizationSelect = (levelIndex: number, value: string) => {
    if (!value) return;

    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      orgLevels,
      selectedOrgs,
      organizations,
      orgTypes
    );

    setOrgLevels(updatedLevels);
    setSelectedOrgs(updatedSelectedOrgs);

    // Update form value with the final selected organization
    const finalOrgUID = getFinalSelectedOrganization(updatedSelectedOrgs);
    if (finalOrgUID) {
      form.setValue("orgUID", finalOrgUID);
      setSelectedOrgUID(finalOrgUID);
      loadRoutesForOrganization(finalOrgUID);
    }
  };

  // Reset organization selection
  const resetOrganizationSelection = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(
      organizations,
      orgTypes
    );
    setOrgLevels(resetLevels);
    setSelectedOrgs(resetSelectedOrgs);
    form.setValue("orgUID", "");
    setSelectedOrgUID("");
    setRoutes([]);
  };

  // Handle location hierarchy selection
  const handleLocationSelect = (levelIndex: number, value: string) => {
    if (!value) return;

    const selectedLocation = locations.find((loc) => loc.UID === value);

    const { updatedLevels, updatedSelectedLocations } = handleLocationSelection(
      levelIndex,
      value,
      locationLevels,
      selectedLocations,
      locations,
      locationTypes
    );

    setLocationLevels(updatedLevels);
    setSelectedLocations(updatedSelectedLocations);

    // Update form value with the final selected location
    const finalLocationUID = getFinalSelectedLocation(updatedSelectedLocations);
    if (finalLocationUID) {
      form.setValue("locationUID", finalLocationUID);
      setSelectedLocationUID(finalLocationUID);

      // Also set locationType and locationValue for JobPosition
      const selectedLocation = locations.find(
        (loc) => loc.UID === finalLocationUID
      );
      if (selectedLocation) {
        form.setValue("locationType", selectedLocation.LocationTypeName || "");
        form.setValue("locationValue", selectedLocation.Name || "");
      }
    }
  };

  // Reset location selection
  const resetLocationSelection = () => {
    const { resetLevels, resetSelectedLocations } = resetLocationHierarchy(
      locations,
      locationTypes
    );
    setLocationLevels(resetLevels);
    setSelectedLocations(resetSelectedLocations);
    form.setValue("locationUID", "");
    form.setValue("locationType", "");
    form.setValue("locationValue", "");
    setSelectedLocationUID("");
  };

  // Generate user code with automatic prefix matching database pattern
  const generateUserCode = () => {
    // Use TB prefix to match existing database pattern
    const defaultPrefix = "TB";

    // Generate code with prefix + 4-digit number
    const timestamp = Date.now().toString().slice(-4); // Last 4 digits
    const generatedCode = `${defaultPrefix}${timestamp}`;

    form.setValue("code", generatedCode);

    toast({
      title: "Success",
      description: `User code generated: ${generatedCode}`,
    });
  };

  const loadUserDetails = async (uid: string) => {
    setLoading(true);
    try {
      // Implementation for loading existing user details would go here
      // This would fetch user data and populate the form
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to load user details",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const loadEmployeeById = async (empId: string) => {
    setLoadingEmployee(true);

    try {
      // Use the correct API endpoint
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api"
        }/MaintainUser/SelectMaintainUserDetailsByUID?empUID=${empId}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            ...authService.getAuthHeaders(),
          },
        }
      );

      if (response.ok) {
        const result = await response.json();

        if (result.IsSuccess && result.Data) {
          const data = result.Data;
          setEmployeeData(data);

          // Store existing encrypted password for edit mode
          if (data.Emp && data.Emp.EncryptedPassword) {
            setExistingEncryptedPassword(data.Emp.EncryptedPassword);
            console.log("Stored existing encrypted password for preservation");
          }

          // Master data should now be loaded from the wait above

          // Populate form with employee data from Emp object
          if (data.Emp) {
            form.setValue("code", data.Emp.Code || "");
            form.setValue("name", data.Emp.Name || "");
            form.setValue("aliasName", data.Emp.AliasName || "");
            form.setValue("isActive", data.Emp.Status === "Active");

            // Login ID is not in the form schema, skip it
            // form.setValue('loginId', data.Emp.LoginId || '');
          }

          // Populate from EmpInfo object
          if (data.EmpInfo) {
            form.setValue("email", data.EmpInfo.Email || "");
            form.setValue("phone", data.EmpInfo.Phone || "");
          }

          // No need to determine user type anymore since it's not used by backend

          // Populate from JobPosition object
          if (data.JobPosition) {
            // Set Role
            const roleUID = data.JobPosition.UserRoleUID || "";
            // console.log(
            //   "Available roles:",
            //   userRoles.map((r) => ({ uid: r.uid, label: r.label }))
            // );

            // Find the role in the loaded roles (try multiple matching strategies)
            let role = userRoles.find((r) => r.uid === roleUID);
            if (!role && roleUID) {
              // Try case-insensitive search by label/name
              role = userRoles.find(
                (r) =>
                  r.label?.toLowerCase() === roleUID.toLowerCase() ||
                  r.code?.toLowerCase() === roleUID.toLowerCase()
              );
            }

            if (role) {
              form.setValue("userRoleUID", role.uid);
            } else if (roleUID) {
              // console.log("⚠️ Role not found in list, setting directly:", roleUID);
              // console.log("Available role UIDs:", userRoles.map(r => r.uid));
              form.setValue("userRoleUID", roleUID);
            }

            // Handle department - Try to match by name or use as-is
            const deptName = data.JobPosition.Department || "";
            // console.log("Department from API:", deptName);

            if (deptName) {
              if (departments.length > 0) {
                // console.log("Available departments:", departments.map((d) => ({ uid: d.uid, label: d.label })));

                let dept = departments.find((d) => d.uid === deptName);
                if (!dept && deptName) {
                  // Try case-insensitive search by label/name/code
                  dept = departments.find(
                    (d) =>
                      d.label?.toLowerCase() === deptName.toLowerCase() ||
                      d.code?.toLowerCase() === deptName.toLowerCase()
                  );
                }

                if (dept) {
                  // console.log("✅ Found matching department:", dept);
                  form.setValue("departmentUID", dept.uid);
                } else {
                  // console.log("⚠️ Department not found, setting pending:", deptName);
                  setPendingDeptSetup(deptName);
                }
              } else {
                // Departments not loaded yet, set pending
                setPendingDeptSetup(deptName);
              }
            }

            // Set Organization
            const orgUID = data.JobPosition.OrgUID || "";
            // console.log("Setting organization UID:", orgUID);
            form.setValue("orgUID", orgUID);

            if (orgUID) {
              setSelectedOrgUID(orgUID);

              // Set pending setup to be processed when master data loads
              setPendingOrgSetup(orgUID);
            }

            // Set Reports To
            const reportsToUID = data.JobPosition.ReportsToUID || "";
            // console.log(
            //   "Available report-to users (first 5):",
            //   reportToUsers
            //     .slice(0, 5)
            //     .map((u) => ({ uid: u.uid, label: u.label }))
            // );
            // console.log("Setting reports to UID:", reportsToUID);

            let reportsTo = reportToUsers.find((u) => u.uid === reportsToUID);
            if (!reportsTo && reportsToUID) {
              // Try search by code or label
              reportsTo = reportToUsers.find(
                (u) =>
                  u.code === reportsToUID ||
                  u.label?.includes(`[${reportsToUID}]`)
              );
            }

            if (reportsTo) {
              // console.log("✅ Found matching reports-to user:", reportsTo);
              form.setValue("reportsToUID", reportsTo.uid);
            } else if (reportsToUID) {
              // console.log("⚠️ Reports-to user not found, setting directly:", reportsToUID);
              // console.log("Available report-to UIDs:", reportToUsers.map(u => u.uid).slice(0, 5));
              form.setValue("reportsToUID", reportsToUID);
            }

            // Set organization hierarchy properly
            if (orgUID) {
              setSelectedOrgUID(orgUID);
              // Set up organization hierarchy immediately
              setupOrganizationHierarchy(orgUID);
            }

            // Get location type and value from JobPosition
            const locationType =
              data.JobPosition.LocationType ||
              data.JobPosition.locationType ||
              "";
            const locationValue =
              data.JobPosition.LocationValue ||
              data.JobPosition.locationValue ||
              "";

            // console.log("📍 Location data from JobPosition:", { locationType, locationValue });

            if (locationType && locationValue) {
              // Set location form fields immediately
              form.setValue("locationType", locationType);
              form.setValue("locationValue", locationValue);

              // Set pending location setup to be processed when master data loads
              setPendingLocationSetup({ locationType, locationValue });
              // console.log("📍 Location setup pending for:", { locationType, locationValue });
            }
          }

          // Set mobile app access to false by default (mobile app integration disabled for now)
          // console.log("📱 Mobile app access status: false (feature disabled)");
          form.setValue("mobileAppAccess", false);

          // console.log("📝 Final form values for user:", {
          //   code: form.getValues("code"),
          //   name: form.getValues("name"),
          //   userRoleUID: form.getValues("userRoleUID"),
          //   departmentUID: form.getValues("departmentUID"),
          //   orgUID: form.getValues("orgUID"),
          //   reportsToUID: form.getValues("reportsToUID"),
          //   mobileAppAccess: form.getValues("mobileAppAccess"),
          //   locationUID: form.getValues("locationUID"),
          //   locationType: form.getValues("locationType"),
          //   locationValue: form.getValues("locationValue"),
          // });

          return; // Success, exit early
        }
      }

      // If we couldn't load from the main endpoint, show warning
      // console.warn("⚠️ Could not load employee data from API");
    } catch (error) {
      // console.error("❌ Error loading employee:", error);
      toast({
        title: "Warning",
        description:
          "Could not load employee data. Please check if the employee exists.",
        variant: "default",
      });
    } finally {
      setLoadingEmployee(false);
    }
  };

  // Helper function to set up organization hierarchy
  const setupOrganizationHierarchy = (orgUID: string) => {
    const selectedOrganization = organizations.find((o) => o.UID === orgUID);
    if (selectedOrganization) {
      // console.log(
      //   "Setting up organization hierarchy for:",
      //   selectedOrganization
      // );

      // If it has a parent, we need to set up the hierarchy properly
      if (selectedOrganization.ParentUID) {
        const parentOrg = organizations.find(
          (o) => o.UID === selectedOrganization.ParentUID
        );
        if (parentOrg) {
          // Set parent first
          const parentResult = handleOrganizationSelection(
            0,
            parentOrg.UID,
            orgLevels,
            [],
            organizations,
            orgTypes
          );

          // Then set the child
          const childResult = handleOrganizationSelection(
            1,
            selectedOrganization.UID,
            parentResult.updatedLevels,
            parentResult.updatedSelectedOrgs,
            organizations,
            orgTypes
          );

          setOrgLevels(childResult.updatedLevels);
          setSelectedOrgs(childResult.updatedSelectedOrgs);
        }
      } else {
        // It's a root organization
        const orgHierarchyResult = handleOrganizationSelection(
          0,
          orgUID,
          orgLevels,
          selectedOrgs,
          organizations,
          orgTypes
        );
        setOrgLevels(orgHierarchyResult.updatedLevels);
        setSelectedOrgs(orgHierarchyResult.updatedSelectedOrgs);
      }
    }
  };

  // Helper function to build location ancestry path
  const buildLocationPath = (locationUID: string): Location[] => {
    const path: Location[] = [];
    let currentUID = locationUID;

    while (currentUID) {
      const location = locations.find((l) => l.UID === currentUID);
      if (!location) break;

      path.unshift(location); // Add to beginning to get root-to-leaf order
      currentUID = location.ParentUID || "";
    }

    // console.log("📍 Location path:", path.map(l => `${l.Name}(${l.LocationTypeName})`));
    return path;
  };

  // Helper function to set up location hierarchy
  const setupLocationHierarchy = (locationUID: string) => {
    // console.log("🔄 Setting up location hierarchy for:", locationUID);

    if (
      !locationUID ||
      !locations.length ||
      !locationTypes.length ||
      !locationLevels.length
    ) {
      // console.log("🚫 Missing data for location setup:", {
      //   locationUID: !!locationUID,
      //   locations: locations.length,
      //   locationTypes: locationTypes.length,
      //   locationLevels: locationLevels.length
      // });
      return;
    }

    const selectedLocation = locations.find((l) => l.UID === locationUID);
    if (!selectedLocation) {
      // console.log("❌ Location not found in locations list:", locationUID);
      return;
    }

    // console.log("📍 Selected location details:", {
    //   UID: selectedLocation.UID,
    //   Name: selectedLocation.Name,
    //   LocationTypeName: selectedLocation.LocationTypeName,
    //   ParentUID: selectedLocation.ParentUID
    // });

    // Build the complete path from root to the selected location
    const locationPath = buildLocationPath(locationUID);

    if (locationPath.length === 0) {
      // console.log("❌ Could not build location path");
      return;
    }

    // Start with fresh location levels
    let currentLevels = initializeLocationHierarchy(locations, locationTypes);
    let currentSelectedLocations: string[] = [];

    // console.log("🌱 Starting with initial levels:", currentLevels.length);

    // Walk through the path and build hierarchy step by step
    for (let i = 0; i < locationPath.length; i++) {
      const targetLocation = locationPath[i];
      // console.log(`🚶 Step ${i + 1}: Looking for ${targetLocation.Name} in level ${i}`);

      // Find which level contains this location
      let foundLevelIndex = -1;
      for (let levelIdx = 0; levelIdx < currentLevels.length; levelIdx++) {
        const level = currentLevels[levelIdx];
        const locationInLevel = level.locations.find(
          (loc) => loc.UID === targetLocation.UID
        );
        if (locationInLevel) {
          foundLevelIndex = levelIdx;
          break;
        }
      }

      if (foundLevelIndex >= 0) {
        // console.log(`✅ Found ${targetLocation.Name} in level ${foundLevelIndex}`);

        // Select this location in the hierarchy
        const result = handleLocationSelection(
          foundLevelIndex,
          targetLocation.UID,
          currentLevels,
          currentSelectedLocations,
          locations,
          locationTypes
        );

        currentLevels = result.updatedLevels;
        currentSelectedLocations = result.updatedSelectedLocations;

        // console.log(`📊 After selection - Levels: ${currentLevels.length}, Selected: ${currentSelectedLocations.length}`);
      } else {
        // console.log(`❌ Could not find ${targetLocation.Name} in any current level`);
        // If we can't find the location in current levels, we might need to rebuild
        // This could happen if the location hierarchy data is inconsistent
        break;
      }
    }

    // Update the state with the final hierarchy
    setLocationLevels(currentLevels);
    setSelectedLocations(currentSelectedLocations);
    setSelectedLocationUID(locationUID);
    // console.log("✅ Location hierarchy setup complete:", {
    //   levels: currentLevels.length,
    //   selectedLocations: currentSelectedLocations.length,
    //   finalSelection: locationUID
    // });

    // Debug: Log each level's selection
    currentLevels.forEach((level, index) => {
      // console.log(`  Level ${index}: ${level.locationTypeName} - Selected: ${level.selectedLocationUID || 'None'}`);
    });
  };

  // Helper function to get current user UID from JWT token (matching web portal)
  const getCurrentUserFromToken = () => {
    try {
      const user = authService.getCurrentUser();
      return user?.uid || "ADMIN";
    } catch (error) {
      // console.error("Error extracting user from token:", error);
      return "ADMIN";
    }
  };

  // Helper function to generate unique UID (matching database format - no hyphens or underscores)
  const generateUID = (prefix: string = "RU") => {
    const timestamp = String(Date.now()).slice(-4); // Last 4 digits
    const random = Math.floor(Math.random() * 100)
      .toString()
      .padStart(2, "0");
    return `${prefix}${timestamp}${random}`; // No hyphens or underscores
  };

  const handleCopyLocationMapping = async () => {
    if (!selectedSourceEmployee) {
      toast({
        title: "Validation Error",
        description:
          "Please select a source employee to copy location mappings from.",
        variant: "destructive",
      });
      return;
    }

    try {
      setLoading(true);

      // Get source employee's full data
      const sourceEmployee = await employeeService.getEmployeeById(
        selectedSourceEmployee
      );

      // console.log("Source employee data received:", sourceEmployee);

      if (!sourceEmployee) {
        throw new Error("Source employee not found");
      }

      // The data is already unwrapped in getEmployeeById
      const sourceData = sourceEmployee;

      // For edit mode - directly update the employee
      if (isEdit) {
        const targetUID = user?.uid || employeeData?.UID || employeeId;
        if (!targetUID) {
          throw new Error("Target employee UID not found");
        }

        // Copy organization mappings via API
        try {
          await employeeService.copyLocationMapping(
            selectedSourceEmployee,
            targetUID
          );
        } catch (err) {
          // console.log("Note: EmpOrgMapping API might not be available, copying data directly");
        }

        // Reload the page to get updated data
        toast({
          title: "Success",
          description: "Location data copied successfully! Reloading...",
        });

        setTimeout(() => {
          window.location.reload();
        }, 1000);
      } else {
        // For create mode - copy data to form fields
        // console.log("Copying data for create mode. Source data structure:", sourceData);

        if (sourceData.JobPosition) {
          const jobPos = sourceData.JobPosition;
          // console.log("JobPosition data:", jobPos);

          // Set organization
          if (jobPos.OrgUID) {
            // console.log("Setting orgUID:", jobPos.OrgUID);
            form.setValue("orgUID", jobPos.OrgUID);
            setSelectedOrgUID(jobPos.OrgUID);

            // Set pending setup to be processed when master data loads
            if (organizations.length > 0 && orgTypes.length > 0) {
              // Organizations already loaded, setup immediately
              setupOrganizationHierarchy(jobPos.OrgUID);
            } else {
              setPendingOrgSetup(jobPos.OrgUID);
              // console.log("📍 Organization setup pending for:", jobPos.OrgUID);
            }

            // Load routes for this organization
            await loadRoutesForOrganization(jobPos.OrgUID);
          }

          // Set department (field might be "Department" or "DepartmentUID")
          const deptValue = jobPos.DepartmentUID || jobPos.Department;
          if (deptValue) {
            // console.log("Setting department value:", deptValue);

            // If it's already a UID format (starts with "dept-"), use it directly
            if (deptValue.startsWith("dept-")) {
              form.setValue("departmentUID", deptValue);
            } else {
              // Otherwise, find the department by name/code and get its UID
              const dept = departments.find(
                (d) =>
                  d.code?.toLowerCase() === deptValue.toLowerCase() ||
                  d.label?.toLowerCase() === deptValue.toLowerCase()
              );

              if (dept) {
                // console.log("Found department, setting UID:", dept.uid);
                form.setValue("departmentUID", dept.uid);
              } else {
                // console.warn("Department not found:", deptValue);
                // Set the raw value as fallback
                form.setValue("departmentUID", deptValue);
              }
            }
          }

          // Set user role
          if (jobPos.UserRoleUID) {
            // console.log("Setting userRoleUID:", jobPos.UserRoleUID);
            form.setValue("userRoleUID", jobPos.UserRoleUID);
          }

          // Set reports to
          if (jobPos.ReportsToUID) {
            // console.log("Setting reportsToUID:", jobPos.ReportsToUID);
            form.setValue("reportsToUID", jobPos.ReportsToUID);
          }

          // Set designation
          if (jobPos.Designation) {
            // console.log("Setting designation:", jobPos.Designation);
            form.setValue("designation", jobPos.Designation);
          }

          // Set user type from JobPosition
          // UserType field removed - not used by backend

          // Copy other location-related fields
          if (
            sourceData.RouteUserMappings &&
            sourceData.RouteUserMappings.length > 0
          ) {
            const routes = sourceData.RouteUserMappings.map(
              (r: any) => r.RouteUID
            );
            // console.log("Setting routes:", routes);
            form.setValue("assignedRoutes", routes);
          }

          // Copy applicable organizations if available
          if (sourceData.ApplicableOrgs) {
            // console.log("Setting applicable orgs:", sourceData.ApplicableOrgs);
            form.setValue("applicableOrgs", sourceData.ApplicableOrgs);
          }

          // Copy location data from JobPosition
          if (jobPos.LocationType && jobPos.LocationValue) {
            // console.log("Setting location data:", jobPos.LocationType, jobPos.LocationValue);
            form.setValue("locationType", jobPos.LocationType);
            form.setValue("locationValue", jobPos.LocationValue);

            // Try to set up location hierarchy from the copied data
            const matchingLocation = locations.find(
              (loc) =>
                loc.LocationTypeName === jobPos.LocationType &&
                loc.Name === jobPos.LocationValue
            );

            if (matchingLocation) {
              // console.log("Setting up location hierarchy for:", matchingLocation);
              setupLocationHierarchy(matchingLocation.UID);
            }
          }

          // Copy branch and sales office assignments
          if (jobPos.BranchUID) {
            // console.log("Setting branchUID:", jobPos.BranchUID);
            form.setValue("branchUID", jobPos.BranchUID);
          }
          if (jobPos.SalesOfficeUID) {
            // console.log("Setting salesOfficeUID:", jobPos.SalesOfficeUID);
            form.setValue("salesOfficeUID", jobPos.SalesOfficeUID);
          }
        } else {
          // console.log("No JobPosition found in source data");
        }

        // Copy data from Emp object
        if (sourceData.Emp) {
          // console.log("Copying from Emp data");
          const emp = sourceData.Emp;

          // Don't copy name, code, or login ID - new employee should have their own
          // But copy auth type if it's the same
          if (emp.AuthType) {
            // console.log("Setting authType:", emp.AuthType);
            form.setValue("authType", emp.AuthType);
          }
        }

        // Copy only location-relevant data from EmpInfo
        if (sourceData.EmpInfo) {
          // console.log("Copying location-relevant data from EmpInfo");
          const empInfo = sourceData.EmpInfo;

          // Don't copy personal details like email/phone - new employee should have their own

          // Copy can handle stock permission - this is location/role relevant
          if (empInfo.CanHandleStock !== undefined) {
            // console.log("Setting canHandleStock:", empInfo.CanHandleStock);
            form.setValue("canHandleStock", empInfo.CanHandleStock);
          }
        }

        toast({
          title: "Success",
          description: "Location data copied to form! Please review and save.",
        });
      }

      setCopyLocationVisible(false);
      setSelectedSourceEmployee("");
      setEmployeeSearchQuery("");
    } catch (error: any) {
      // console.error("Copy location mapping error:", error);
      toast({
        title: "Error",
        description:
          error.message || "Failed to copy location data. Please try again.",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: RouteUserFormData) => {
    setLoading(true);

    try {
      console.log("🔄 Creating/updating route user...", values);
      console.log("📧 Email value:", values.email);
      console.log("📱 Phone value:", values.phone);

      // Validate required fields
      if (
        !values.code ||
        !values.name ||
        !values.userRoleUID ||
        !values.orgUID ||
        !values.departmentUID
      ) {
        toast({
          title: "Validation Error",
          description: "Please fill in all required fields.",
          variant: "destructive",
        });
        setLoading(false);
        return;
      }

      // Check for duplicate code (matching web portal logic)
      try {
        // This would call actual duplicate check API in production
        // console.log("Checking for duplicate code:", values.code);
      } catch (error) {
        // console.log("Duplicate check failed, proceeding:", error);
      }

      // Generate UIDs
      const currentUser = getCurrentUserFromToken();
      // Use employee code as UID to match database pattern
      const userUID = user?.uid || values.code || generateUID("TB");
      // Use existing JobPosition UID in edit mode, or create new one
      const jobPositionUID =
        isEdit && existingJobPositionUID ? existingJobPositionUID : userUID;
      const currentTime = new Date().toISOString();

      // Get company UID - use selected organization or fallback
      const companyUID =
        values.orgUID || organizations[0]?.UID || "AUTO_SELECT";

      // Log the current form values to debug
      console.log("📊 Form values before submission:", {
        orgUID: values.orgUID,
        departmentUID: values.departmentUID,
        userRoleUID: values.userRoleUID,
        companyUID: companyUID,
        email: values.email,
        phone: values.phone,
      });

      // Build the employee data structure exactly matching the backend C# model (PascalCase)
      const formData = {
        Emp: {
          // Don't send id field - let backend auto-generate it
          UID: userUID,
          CompanyUID: companyUID,
          Code: values.code,
          Name: values.name,
          AliasName: values.aliasName || values.name,
          LoginId: values.loginId || values.code,
          EmpNo: values.empNo || values.code,
          AuthType: values.authType || "Local",
          Status: values.status || "Active",
          ProfileImage: "",
          JobPositionUid: jobPositionUID,
          // In create mode: use entered password, in edit mode: preserve existing password
          EncryptedPassword: isEdit
            ? existingEncryptedPassword
            : values.password,
          ActionType: isEdit ? 1 : 0, // 0 = Add, 1 = Update (must be number)
          ApprovalStatus: "Approved",
          ActiveAuthKey: "",
          IsMandatoryChangePassword: false,
          // BaseModel fields
          CreatedBy: currentUser,
          CreatedTime: currentTime,
          ModifiedBy: currentUser,
          ModifiedTime: currentTime,
          ServerAddTime: currentTime,
          ServerModifiedTime: currentTime,
          SS: 0,
          KeyUID: userUID,
          IsSelected: false,
        },
        EmpInfo: {
          // Don't send id field - let backend auto-generate it
          UID: isEdit ? employeeData?.EmpInfo?.UID || userUID : userUID, // Use actual EmpInfo UID when editing (PascalCase)
          EmpUID: userUID, // PascalCase
          Email: values.email || "", // PascalCase
          Phone: values.phone || "", // PascalCase
          StartDate: values.startDate ? new Date(values.startDate) : new Date(),
          CanHandleStock: values.canHandleStock || false, // PascalCase
          ActionType: isEdit ? 1 : 0, // 0 = Add, 1 = Update (must be number)
          AdGroup: "",
          AdUsername: "",
          // BaseModel fields
          CreatedBy: currentUser,
          CreatedTime: currentTime,
          ModifiedBy: currentUser,
          ModifiedTime: currentTime,
          ServerAddTime: currentTime,
          ServerModifiedTime: currentTime,
          SS: 0,
          KeyUID: userUID, // Use same UID
          IsSelected: false,
        },
        JobPosition: {
          // Don't send id field - let backend auto-generate it
          UID: jobPositionUID,
          EmpUID: userUID,
          CompanyUID: companyUID,
          OrgUID: values.orgUID,
          UserRoleUID: values.userRoleUID,
          BranchUID: values.branchUID || undefined,
          SalesOfficeUID: values.salesOfficeUID || undefined,
          DepartmentUID: values.departmentUID,
          Department: values.departmentUID?.replace("dept-", "") || "",
          CollectionLimit: values.collectionLimit || 0,
          Designation: values.designation || "Employee",
          HasEOT: values.hasEOT || true,
          LocationMappingTemplateUID: undefined,
          LocationMappingTemplateName: undefined,
          SKUMappingTemplateUID: undefined,
          SKUMappingTemplateName: undefined,
          LocationType: values.locationType || undefined,
          LocationValue: values.locationValue || undefined,
          SeqCode: undefined,
          EmpCode: values.code,
          ReportsToUID: values.reportsToUID || "",
          ActionType: isEdit ? 1 : 0, // 0 = Add, 1 = Update (must be number)
          // BaseModel fields
          CreatedBy: currentUser,
          CreatedTime: currentTime,
          ModifiedBy: currentUser,
          ModifiedTime: currentTime,
          ServerAddTime: currentTime,
          ServerModifiedTime: currentTime,
          SS: 0,
          KeyUID: jobPositionUID, // Use job position UID
          IsSelected: false,
        },
        EmpOrgMapping: values.applicableOrgs?.map((orgUID: string) => ({
          EmpUID: userUID,
          OrgUID: orgUID,
          IsActive: true,
        })) || [
          {
            EmpUID: userUID,
            OrgUID: values.orgUID,
            IsActive: true,
          },
        ],
      };

      // Call the API using the same endpoint as web portal
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api"
        }/MaintainUser/CUDEmployee`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            ...authService.getAuthHeaders(),
          },
          body: JSON.stringify(formData),
        }
      );

      console.log(
        "🌐 API Response Status:",
        response.status,
        response.statusText
      );

      if (!response.ok) {
        const errorText = await response.text();
        console.error("❌ API Error Response:", errorText);
        throw new Error(
          `HTTP error! status: ${response.status}, message: ${errorText}`
        );
      }

      const result = await response.json();
      console.log("📥 API Response Body:", result);

      if (!result.IsSuccess && !result.isSuccessResponse) {
        throw new Error(
          result.Message || result.message || "Route user creation failed"
        );
      }

      console.log("✅ User operation successful, response:", result);
      console.log(
        "📧 Main API call completed - checking if email/phone were updated"
      );

      // Update location data if provided
      if (values.locationType && values.locationValue) {
        try {
          // console.log("📍 Updating job position location:", {
          //   jobPositionUID,
          //   locationType: values.locationType,
          //   locationValue: values.locationValue
          // });

          const locationUpdated =
            await employeeService.updateJobPositionLocation(
              jobPositionUID,
              values.locationType,
              values.locationValue
            );

          if (locationUpdated) {
            // console.log("✅ Job position location updated successfully");
          } else {
            // console.warn("⚠️ Job position location update failed");
          }
        } catch (locationError: unknown) {
          // console.error("❌ Failed to update job position location:", locationError);
          // Don't fail the entire operation for location update errors
          toast({
            title: "Warning",
            description:
              "Employee saved but location update failed. You can update it later.",
            variant: "default",
          });
        }
      }

      // Handle mobile app access based on form toggle
      if (values.mobileAppAccess) {
        try {
          // console.log("🔄 Setting up mobile app access for employee:", userUID);

          if (isEdit) {
            // For edit mode, update mobile app access
            await mobileAppActionService.updateMobileAppAccess(
              userUID,
              true,
              companyUID
            );
            // console.log("✅ Mobile app access updated successfully");
          } else {
            // For create mode, create new mobile app action
            const mobileAppResult =
              await mobileAppActionService.createMobileAppAction(
                userUID,
                companyUID,
                values.loginId
              );

            if (mobileAppResult.success) {
              console.log(
                "✅ Mobile app access created successfully for new employee"
              );
            } else {
              console.warn(
                "⚠️ Mobile app access creation failed:",
                mobileAppResult.message
              );
            }
          }
        } catch (mobileAppError: unknown) {
          // console.error("❌ Failed to set up mobile app access:", mobileAppError);
          const errorMessage =
            (mobileAppError as any)?.message ||
            "Mobile app access setup failed";

          // Don't fail the entire operation for mobile app access errors
          toast({
            title: "Warning",
            description: errorMessage.includes("not available")
              ? "Employee created. Mobile app feature is currently not available."
              : "Employee created but mobile app access setup failed. You can enable it later.",
            variant: "default",
          });
        }
      } else if (isEdit) {
        // For edit mode, if mobile app access is disabled, update it
        try {
          // console.log("🔄 Disabling mobile app access for employee:", userUID);
          await mobileAppActionService.updateMobileAppAccess(
            userUID,
            false,
            companyUID
          );
          // console.log("✅ Mobile app access disabled successfully");
        } catch (mobileAppError: unknown) {
          // console.error("❌ Failed to disable mobile app access:", mobileAppError);
          const errorMessage =
            (mobileAppError as any)?.message ||
            "Mobile app access update failed";

          // Don't fail the entire operation for mobile app access errors
          toast({
            title: "Warning",
            description: errorMessage.includes("not available")
              ? "Employee updated. Mobile app feature is currently not available."
              : "Employee updated but mobile app access update failed.",
            variant: "default",
          });
        }
      }

      // Handle route assignments (matching web portal logic)
      const routesToAssign = [];
      if (values.assignedRoutes && values.assignedRoutes.length > 0) {
        routesToAssign.push(...values.assignedRoutes);
      }
      // if (values.routeUID && !routesToAssign.includes(values.routeUID)) {
      //   routesToAssign.push(values.routeUID);
      // }

      if (routesToAssign.length > 0) {
        try {
          // console.log(
          //   "🔄 Creating route user assignments for routes:",
          //   routesToAssign
          // );

          const routeUsers = routesToAssign.map((routeUID: string) => {
            const routeUserUID = `RU${String(Date.now()).slice(-4)}`;

            // Format for backend API - using PascalCase to match web portal
            return {
              UID: routeUserUID,
              RouteUID: routeUID,
              JobPositionUID: jobPositionUID,
              FromDate: values.routeFromDate || "2025-01-01T00:00:00",
              ToDate: values.routeToDate || "2099-12-31T00:00:00",
              IsActive: true,
              ActionType: 0, // 0 = Add
              CreatedBy: currentUser,
              ModifiedBy: currentUser,
              CreatedTime: currentTime,
              ModifiedTime: currentTime,
              ServerAddTime: currentTime,
              ServerModifiedTime: currentTime,
              // Don't send Id field - let backend auto-generate it
              SS: 0,
            };
          });

          // console.log("🔄 Sending route user data:", routeUsers);

          const routeResponse = await fetch(
            `${
              process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api"
            }/RouteUser/CreateRouteUser`,
            {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
                ...authService.getAuthHeaders(),
              },
              body: JSON.stringify(routeUsers),
            }
          );

          if (routeResponse.ok) {
            const routeResult = await routeResponse.json();
            // console.log("✅ Route users created successfully:", routeResult);
          } else {
            // console.warn("⚠️ Route user creation failed but user was created");
            toast({
              title: "Partial Success",
              description: `Route user ${
                isEdit ? "updated" : "created"
              } successfully, but failed to assign some routes.`,
              variant: "destructive",
            });
          }
        } catch (routeError) {
          // console.error("❌ Route user creation error:", routeError);
          toast({
            title: "Partial Success",
            description: `Route user ${
              isEdit ? "updated" : "created"
            } successfully, but failed to assign routes.`,
            variant: "destructive",
          });
        }
      }

      toast({
        title: "Success",
        description: `Route user ${
          isEdit ? "updated" : "created"
        } successfully${
          routesToAssign.length > 0
            ? ` with ${routesToAssign.length} route(s) assigned`
            : ""
        }${
          !isEdit && values.mobileAppAccess ? " with mobile app access" : ""
        }!`,
      });

      // Show additional success info
      toast({
        title: "Route User Details",
        description: `User Code: ${values.code} | Name: ${values.name}`,
      });

      onSuccess();
    } catch (error) {
      console.error("❌ Failed to save route user:", error);
      console.error(
        "❌ Error stack:",
        error instanceof Error ? error.stack : "No stack trace available"
      );
      console.error("❌ Error details:", {
        message: error instanceof Error ? error.message : String(error),
        name: error instanceof Error ? error.name : "Unknown",
        cause: error instanceof Error ? error.cause : undefined,
      });

      toast({
        title: "Error",
        description: `Failed to ${
          isEdit ? "update" : "create"
        } route user. Please try again. Error: ${
          error instanceof Error ? error.message : String(error)
        }`,
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  // Show skeleton loader while loading data
  if (loadingMasterData || (loading && isEdit)) {
    return (
      <div className={`space-y-3 ${isModal ? "p-4" : "p-0"}`}>
        {/* Basic Information Skeleton */}
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-40" />
            <Skeleton className="h-4 w-60 mt-2" />
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-10 w-full" />
              </div>
              <div className="space-y-2">
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-10 w-full" />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-10 w-full" />
              </div>
              <div className="space-y-2">
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-10 w-full" />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Organization Hierarchy Skeleton */}
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-64 mt-2" />
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-10 w-full" />
              </div>
              <div className="space-y-2">
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-10 w-full" />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Location Assignment Skeleton */}
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-40" />
            <Skeleton className="h-4 w-56 mt-2" />
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="space-y-2">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-10 w-full" />
            </div>
          </CardContent>
        </Card>

        {/* Action Buttons Skeleton */}
        <div className="flex justify-end space-x-2 pt-6 border-t">
          <Skeleton className="h-9 w-20" />
          <Skeleton className="h-9 w-32" />
        </div>
      </div>
    );
  }

  return (
    <div className={`space-y-3 ${isModal ? "p-4" : "p-0"}`}>
      <Form {...form}>
        <form
          onSubmit={(e) => {
            console.log("Form onSubmit event triggered");
            console.log("Form is valid:", form.formState.isValid);
            console.log("Form errors:", form.formState.errors);
            console.log("Form values:", form.getValues());

            // Check required fields
            const values = form.getValues();
            const missingFields = [];
            if (!values.code) missingFields.push("User Code");
            if (!values.name) missingFields.push("Name");
            if (!values.userRoleUID) missingFields.push("Role");
            if (!values.departmentUID) missingFields.push("Department");
            if (!values.orgUID) missingFields.push("Sales Org");
            if (!isEdit && !values.password) missingFields.push("Password");

            if (missingFields.length > 0) {
              console.log("Missing required fields:", missingFields);
              toast({
                title: "Missing Required Fields",
                description: `Please fill in: ${missingFields.join(", ")}`,
                variant: "destructive",
              });
            }

            form.handleSubmit(handleSubmit, (errors) => {
              console.log("Form validation failed with errors:", errors);
              const errorMessages = Object.entries(errors).map(
                ([field, error]) => {
                  return `${field}: ${error?.message || "Invalid"}`;
                }
              );
              toast({
                title: "Validation Error",
                description: errorMessages.join(", "),
                variant: "destructive",
              });
            })(e);
          }}
          className="space-y-3"
        >
          {/* Basic Information */}
          <Card>
            <CardHeader className="relative">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="flex items-center text-gray-800">
                    <IdCard className="h-5 w-5 mr-2 text-gray-600" />
                    Basic Information
                  </CardTitle>
                  <CardDescription>
                    Essential user information required for account creation.
                  </CardDescription>
                </div>
                <div className="flex items-center gap-4">
                  <Button
                    variant="outline"
                    className="border-gray-300"
                    type="button"
                    onClick={() => setCopyLocationVisible(true)}
                  >
                    <Copy className="h-4 w-4 mr-2" />
                    Copy Other User Detail
                  </Button>
                  <FormField
                    control={form.control}
                    name="isActive"
                    render={({ field }) => (
                      <FormItem className="flex items-center space-x-3">
                        <FormLabel className="text-sm font-medium text-gray-700">
                          Status:
                        </FormLabel>
                        <FormControl>
                          <div className="flex items-center space-x-3">
                            <Switch
                              checked={field.value}
                              onCheckedChange={field.onChange}
                            />
                            <span
                              className={`text-sm font-medium px-2 py-1 rounded-md ${
                                field.value
                                  ? "text-green-700 bg-green-100"
                                  : "text-red-700 bg-red-100"
                              }`}
                            >
                              {field.value ? "Active" : "Inactive"}
                            </span>
                          </div>
                        </FormControl>
                      </FormItem>
                    )}
                  />
                </div>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <FormField
                  control={form.control}
                  name="authType"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">
                        * Auth Type
                      </FormLabel>
                      <Select
                        onValueChange={field.onChange}
                        value={field.value}
                        disabled={isEdit}
                      >
                        <FormControl>
                          <SelectTrigger className="border-gray-300 h-9">
                            <SelectValue placeholder="Select authentication type" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {authTypes.map((authType, index) => {
                            const key =
                              authType.uid ||
                              authType.code ||
                              `auth-type-${index}`;
                            const value =
                              authType.code || authType.uid || `auth-${index}`;
                            return (
                              <SelectItem key={key} value={value}>
                                {authType.name || "Unknown"}
                              </SelectItem>
                            );
                          })}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="code"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">
                        * User Code (Auto-generated)
                      </FormLabel>
                      <div className="flex">
                        <FormControl>
                          <Input
                            {...field}
                            placeholder="Enter user code or click generate"
                            className="border-gray-300 h-9"
                          />
                        </FormControl>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          className="ml-2 border-gray-300"
                          onClick={generateUserCode}
                          disabled={isEdit}
                        >
                          <RefreshCw className="h-4 w-4" />
                        </Button>
                      </div>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">* Name</FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          placeholder="Enter name"
                          className="border-gray-300 h-9"
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="userRoleUID"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">* Role</FormLabel>
                      <Popover open={openRole} onOpenChange={setOpenRole}>
                        <PopoverTrigger asChild>
                          <FormControl>
                            <Button
                              variant="outline"
                              role="combobox"
                              aria-expanded={openRole}
                              className="w-full h-9 justify-between border-gray-300"
                            >
                              {field.value
                                ? userRoles.find(
                                    (role) => role.uid === field.value
                                  )?.label
                                : "Select Role"}
                              <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                            </Button>
                          </FormControl>
                        </PopoverTrigger>
                        <PopoverContent className="w-full p-0" align="start">
                          <Command>
                            <CommandInput placeholder="Search roles..." />
                            <CommandEmpty>No role found.</CommandEmpty>
                            <CommandList>
                              <CommandGroup>
                                {userRoles.map((role) => (
                                  <CommandItem
                                    key={role.uid}
                                    value={role.label}
                                    onSelect={() => {
                                      field.onChange(role.uid);
                                      setOpenRole(false);
                                    }}
                                  >
                                    <Check
                                      className={`mr-2 h-4 w-4 ${
                                        field.value === role.uid
                                          ? "opacity-100"
                                          : "opacity-0"
                                      }`}
                                    />
                                    {role.label}
                                  </CommandItem>
                                ))}
                              </CommandGroup>
                            </CommandList>
                          </Command>
                        </PopoverContent>
                      </Popover>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="reportsToUID"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">Report To</FormLabel>
                      <Popover
                        open={openReportTo}
                        onOpenChange={setOpenReportTo}
                      >
                        <PopoverTrigger asChild>
                          <FormControl>
                            <Button
                              variant="outline"
                              role="combobox"
                              aria-expanded={openReportTo}
                              className="w-full h-9 justify-between border-gray-300"
                            >
                              {field.value
                                ? filteredReportToUsers.find(
                                    (user) => user.uid === field.value
                                  )?.label
                                : "Select Report To User"}
                              <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                            </Button>
                          </FormControl>
                        </PopoverTrigger>
                        <PopoverContent className="w-full p-0" align="start">
                          <Command>
                            <CommandInput placeholder="Search users..." />
                            <CommandEmpty>No user found.</CommandEmpty>
                            <CommandList>
                              <CommandGroup>
                                {filteredReportToUsers.map((user) => (
                                  <CommandItem
                                    key={user.uid}
                                    value={user.label}
                                    onSelect={() => {
                                      field.onChange(user.uid);
                                      setOpenReportTo(false);
                                    }}
                                  >
                                    <Check
                                      className={`mr-2 h-4 w-4 ${
                                        field.value === user.uid
                                          ? "opacity-100"
                                          : "opacity-0"
                                      }`}
                                    />
                                    {user.label}
                                  </CommandItem>
                                ))}
                              </CommandGroup>
                            </CommandList>
                          </Command>
                        </PopoverContent>
                      </Popover>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="departmentUID"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">
                        * Department
                      </FormLabel>
                      <Popover
                        open={openDepartment}
                        onOpenChange={setOpenDepartment}
                      >
                        <PopoverTrigger asChild>
                          <FormControl>
                            <Button
                              variant="outline"
                              role="combobox"
                              aria-expanded={openDepartment}
                              className="w-full h-9 justify-between border-gray-300"
                            >
                              {field.value
                                ? departments.find(
                                    (dept) => dept.uid === field.value
                                  )?.label
                                : "Select Department"}
                              <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                            </Button>
                          </FormControl>
                        </PopoverTrigger>
                        <PopoverContent className="w-full p-0" align="start">
                          <Command>
                            <CommandInput placeholder="Search departments..." />
                            <CommandEmpty>No department found.</CommandEmpty>
                            <CommandList>
                              <CommandGroup>
                                {departments.map((dept) => (
                                  <CommandItem
                                    key={dept.uid}
                                    value={dept.label}
                                    onSelect={() => {
                                      field.onChange(dept.uid);
                                      setOpenDepartment(false);
                                    }}
                                  >
                                    <Check
                                      className={`mr-2 h-4 w-4 ${
                                        field.value === dept.uid
                                          ? "opacity-100"
                                          : "opacity-0"
                                      }`}
                                    />
                                    {dept.label}
                                  </CommandItem>
                                ))}
                              </CommandGroup>
                            </CommandList>
                          </Command>
                        </PopoverContent>
                      </Popover>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </CardContent>
          </Card>

          {/* Contact Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center text-gray-800">
                <Mail className="h-5 w-5 mr-2 text-gray-600" />
                Contact Information
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">Email:</FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          type="email"
                          placeholder="Enter email"
                          className="border-gray-300 h-9"
                          disabled={false} // Explicitly enable the field
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="phone"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">Mobile:</FormLabel>
                      <FormControl>
                        <PhoneInput
                          value={field.value}
                          onChange={field.onChange}
                          placeholder="Enter mobile number"
                          className="border-gray-300"
                          defaultCountry="IN"
                          disabled={false} // Explicitly enable the field
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </CardContent>
          </Card>

          {/* Location & Assignment */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center text-gray-800">
                <MapPin className="h-5 w-5 mr-2 text-gray-600" />
                Location & Assignment
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {/* Organization Hierarchy - Dynamic Cascading Fields (copied exactly from route create) */}
                {orgLevels.length > 0 ? (
                  <>
                    {/* Render all levels dynamically */}
                    {orgLevels.map((level, index) => (
                      <div
                        key={`${level.orgTypeUID}_${index}`}
                        className={index === 0 ? "md:col-span-2" : ""}
                      >
                        <Label className="text-sm font-medium text-gray-700 mb-1.5">
                          {level.dynamicLabel || level.orgTypeName}
                          {index === 0 && (
                            <span className="text-red-500 ml-1">*</span>
                          )}
                        </Label>
                        <Select
                          value={level.selectedOrgUID || ""}
                          onValueChange={(value) =>
                            handleOrganizationSelect(index, value)
                          }
                        >
                          <SelectTrigger className="h-9">
                            <SelectValue
                              placeholder={`Select ${(
                                level.dynamicLabel || level.orgTypeName
                              ).toLowerCase()}`}
                            />
                          </SelectTrigger>
                          <SelectContent>
                            {level.organizations.map((org) => (
                              <SelectItem key={org.UID} value={org.UID}>
                                <div className="flex items-center justify-between w-full">
                                  <span>{org.Name}</span>
                                  {org.Code && (
                                    <span className="text-muted-foreground ml-2 text-xs">
                                      ({org.Code})
                                    </span>
                                  )}
                                </div>
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        {index === 0 && form.formState.errors.orgUID && (
                          <p className="text-xs text-red-500 mt-1">
                            {form.formState.errors.orgUID.message}
                          </p>
                        )}
                      </div>
                    ))}
                    {/* Reset button and info text */}
                    <div className="md:col-span-2 flex items-center justify-between">
                      <div className="flex-1">
                        {orgLevels.length > 1 && !selectedOrgs.length && (
                          <div className="text-xs text-muted-foreground italic">
                            Multiple organization types available. Select from
                            any to continue.
                          </div>
                        )}
                      </div>
                      {selectedOrgs.length > 0 && (
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={resetOrganizationSelection}
                          className="ml-4 text-xs"
                        >
                          <X className="h-3 w-3 mr-1" />
                          Reset Selection
                        </Button>
                      )}
                    </div>
                  </>
                ) : (
                  <div className="md:col-span-2 text-center py-8 text-muted-foreground">
                    <Building className="h-8 w-8 mx-auto mb-2 opacity-50" />
                    <p className="text-sm">Loading organization hierarchy...</p>
                  </div>
                )}

                {/* Location Hierarchy - Geographic Assignment */}
                <div className="md:col-span-2 border-t pt-4 mt-4">
                  <div className="flex items-center mb-4">
                    <Globe className="h-4 w-4 mr-2 text-blue-600" />
                    <h3 className="text-sm font-medium text-gray-700">
                      Geographic Location Assignment
                    </h3>
                  </div>
                </div>

                {locationLevels.length > 0 ? (
                  <>
                    {/* Render all location levels dynamically - exactly like organization hierarchy */}
                    {locationLevels.map((level, index) => (
                      <div
                        key={`${level.locationTypeUID}_${index}`}
                        className={index === 0 ? "md:col-span-2" : ""}
                      >
                        <Label className="text-sm font-medium text-gray-700 mb-1.5">
                          {level.dynamicLabel || level.locationTypeName}
                          {index === 0 && (
                            <span className="text-blue-500 ml-1">*</span>
                          )}
                        </Label>
                        <Select
                          value={level.selectedLocationUID || ""}
                          onValueChange={(value) =>
                            handleLocationSelect(index, value)
                          }
                        >
                          <SelectTrigger className="h-9 border-blue-200 focus:border-blue-400">
                            <SelectValue
                              placeholder={`Select ${(
                                level.dynamicLabel || level.locationTypeName
                              ).toLowerCase()}`}
                            />
                          </SelectTrigger>
                          <SelectContent>
                            {level.locations.map((location) => (
                              <SelectItem
                                key={location.UID}
                                value={location.UID}
                              >
                                <div className="flex items-center justify-between w-full">
                                  <span className="font-medium">
                                    {getLocationDisplayName(location)}
                                  </span>
                                  {location.Code && (
                                    <span className="text-xs text-blue-500 ml-2">
                                      [{location.Code}]
                                    </span>
                                  )}
                                </div>
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    ))}

                    {/* Location selection summary and reset */}
                    <div className="md:col-span-2 mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          {locationLevels.length > 1 &&
                            !selectedLocations.length && (
                              <div className="text-xs text-muted-foreground italic">
                                Multiple location types available. Select from
                                any to continue.
                              </div>
                            )}
                        </div>
                        {selectedLocations.length > 0 && (
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={resetLocationSelection}
                            className="ml-4 text-xs border-blue-300 text-blue-600 hover:bg-blue-100"
                          >
                            <X className="h-3 w-3 mr-1" />
                            Reset Location
                          </Button>
                        )}
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="md:col-span-2 text-center py-6 text-muted-foreground">
                    <Globe className="h-6 w-6 mx-auto mb-2 opacity-50" />
                    <p className="text-sm">Loading location hierarchy...</p>
                  </div>
                )}

                {/* <FormField
                  control={form.control}
                  name="routeUID"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">Route:</FormLabel>
                      <Select
                        onValueChange={(value) => {
                          field.onChange(value);
                          if (value) {
                            form.setValue("assignedRoutes", []);
                          }
                        }}
                        value={field.value}
                        disabled={!selectedOrgUID}
                      >
                        <FormControl>
                          <SelectTrigger className="border-gray-300 h-9">
                            <SelectValue
                              placeholder={
                                selectedOrgUID
                                  ? "Select Route (or use Assign Locations tab)"
                                  : "Select Sales Org first"
                              }
                            />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {routes.map((route) => (
                            <SelectItem key={route.uid} value={route.uid}>
                              [{route.code}] {route.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                /> */}

                {/* <FormField
                  control={form.control}
                  name="billToCustomer"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-gray-700">
                        Bill To Customer:
                      </FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          placeholder="Enter bill to customer"
                          className="border-gray-300 h-9"
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                /> */}
              </div>
            </CardContent>
          </Card>

          {/* Security - Only show in create mode */}
          {!isEdit && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center text-gray-800">
                  <Shield className="h-5 w-5 mr-2 text-gray-600" />
                  Security
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="password"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-gray-700">
                          {isEdit ? "Password (optional)" : "* Password"}
                        </FormLabel>
                        <FormControl>
                          <div className="relative">
                            <Input
                              {...field}
                              type={showPassword ? "text" : "password"}
                              placeholder="Enter password"
                              className="border-gray-300 h-9 pr-10"
                            />
                            <button
                              type="button"
                              onClick={() => setShowPassword(!showPassword)}
                              className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                            >
                              {showPassword ? (
                                <EyeOff className="h-4 w-4" />
                              ) : (
                                <Eye className="h-4 w-4" />
                              )}
                            </button>
                          </div>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="confirmPassword"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-gray-700">
                          {isEdit
                            ? "Confirm Password (optional)"
                            : "* Confirm Password"}
                        </FormLabel>
                        <FormControl>
                          <div className="relative">
                            <Input
                              {...field}
                              type={showConfirmPassword ? "text" : "password"}
                              placeholder="Re-enter password"
                              className="border-gray-300 h-9 pr-10"
                            />
                            <button
                              type="button"
                              onClick={() =>
                                setShowConfirmPassword(!showConfirmPassword)
                              }
                              className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                            >
                              {showConfirmPassword ? (
                                <EyeOff className="h-4 w-4" />
                              ) : (
                                <Eye className="h-4 w-4" />
                              )}
                            </button>
                          </div>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>
                <FormDescription className="mt-2 text-sm text-gray-500">
                  {isEdit
                    ? "Leave empty to keep current password. If changing, password must be at least 6 characters"
                    : "Password must be at least 6 characters"}
                </FormDescription>
              </CardContent>
            </Card>
          )}

          {/* Form Actions */}
          <div className="flex justify-end space-x-2 pt-6 border-t border-gray-200">
            <Button
              type="button"
              variant="outline"
              onClick={onCancel}
              disabled={loading}
              className="border-gray-300 h-9"
            >
              Cancel
            </Button>
            {isEdit ? (
              <Button
                type="submit"
                disabled={loading}
                className="bg-blue-600 hover:bg-blue-700"
                onClick={() => {
                  console.log("Save Changes button clicked");
                  console.log("Loading state:", loading);
                  console.log("Form state:", form.formState);
                  console.log("Form errors:", form.formState.errors);
                  console.log("Form values:", form.getValues());
                  // Don't prevent default - let form handle submission
                }}
              >
                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Save Changes
              </Button>
            ) : (
              <Button
                type="submit"
                disabled={loading}
                className="bg-green-600 hover:bg-green-700 text-white"
              >
                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Create Employee
              </Button>
            )}
          </div>
        </form>
      </Form>

      {/* Copy Location Mapping Dialog */}
      <Dialog open={copyLocationVisible} onOpenChange={setCopyLocationVisible}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Copy Location Mapping</DialogTitle>
            <DialogDescription>
              {isEdit
                ? "Copy location data from another user directly to database"
                : "Copy location data from another user to pre-fill form fields"}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-3">
            {/* Source User Selection with integrated search */}
            <div>
              <Label htmlFor="sourceUser" className="text-sm font-medium">
                Select Source User *
              </Label>
              <Select
                value={selectedSourceEmployee}
                onValueChange={setSelectedSourceEmployee}
              >
                <SelectTrigger className="mt-1 h-9">
                  <SelectValue placeholder="Click to search and select user" />
                </SelectTrigger>
                <SelectContent>
                  <div className="px-2 py-2">
                    <Input
                      type="text"
                      placeholder="Search by name..."
                      value={employeeSearchQuery}
                      onChange={(e) => setEmployeeSearchQuery(e.target.value)}
                      className="h-8"
                      onClick={(e) => e.stopPropagation()}
                      onKeyDown={(e) => e.stopPropagation()}
                    />
                  </div>
                  <div className="max-h-[200px] overflow-y-auto">
                    {reportToUsers
                      .filter((user) => {
                        if (!employeeSearchQuery) return true;
                        const query = employeeSearchQuery.toLowerCase();
                        return user.label.toLowerCase().includes(query);
                      })
                      .slice(0, 50)
                      .map((sourceUser) => (
                        <SelectItem key={sourceUser.uid} value={sourceUser.uid}>
                          {sourceUser.label}
                        </SelectItem>
                      ))}
                    {reportToUsers.filter((user) => {
                      if (!employeeSearchQuery) return true;
                      const query = employeeSearchQuery.toLowerCase();
                      return user.label.toLowerCase().includes(query);
                    }).length === 0 && (
                      <div className="px-2 py-2 text-sm text-gray-500 text-center">
                        No employees found
                      </div>
                    )}
                  </div>
                </SelectContent>
              </Select>
            </div>

            {/* Information Panel - Only show for create mode */}
            {!isEdit && (
              <div className="bg-gray-50 p-4 rounded-lg">
                <h4 className="font-medium text-blue-600 mb-2">
                  The following fields will be copied from the selected user:
                </h4>
                <ul className="text-sm text-gray-600 space-y-1 list-disc list-inside">
                  <li>Organization hierarchy and department</li>
                  <li>User role and designation</li>
                  <li>Reporting manager</li>
                  <li>Location assignments (Country, Region, City)</li>
                </ul>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setCopyLocationVisible(false);
                setSelectedSourceEmployee("");
                setEmployeeSearchQuery("");
              }}
            >
              Cancel
            </Button>
            <Button
              onClick={handleCopyLocationMapping}
              disabled={!selectedSourceEmployee}
            >
              <Copy className="h-4 w-4 mr-2" />
              Copy Location Mapping
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
