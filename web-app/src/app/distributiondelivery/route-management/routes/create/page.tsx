"use client";

import React, { useState, useEffect, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { Form } from "@/components/ui/form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { motion, AnimatePresence } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { Switch } from "@/components/ui/switch";
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/components/ui/use-toast";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Progress } from "@/components/ui/progress";
import { Checkbox } from "@/components/ui/checkbox";
import {
  ArrowLeft,
  ArrowRight,
  RotateCcw,
  ChevronDown,
  CheckCircle2,
  RefreshCw,
  FileText,
  FileSpreadsheet,
  Users,
  Clock,
  Calendar,
  Building2,
  Plus,
  X,
  Settings,
  Search,
  Check,
  Download,
} from "lucide-react";
import * as XLSX from "xlsx";
import { authService } from "@/lib/auth-service";
import { api, apiService } from "@/services/api";
import { routeService } from "@/services/routeService";
import { routeServiceProduction } from "@/services/routeService.production";
import { storeService } from "@/services/storeService";
import { employeeService } from "@/services/admin/employee.service";
import moment from "moment";
import { cn } from "@/lib/utils";
import {
  organizationService,
  Organization,
  OrgType,
} from "@/services/organizationService";
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
} from "@/utils/organizationHierarchyUtils";

// Common Components
import CustomerScheduler from "@/components/route/route_shedule";
import RouteExcelImport from "@/components/route/RouteExcelImport";

// Form validation schema
const routeFormSchema = z
  .object({
    code: z
      .string()
      .min(1, "Route code is required")
      .max(20, "Code must be less than 20 characters"),
    name: z
      .string()
      .min(1, "Route name is required")
      .max(100, "Name must be less than 100 characters"),
    description: z.string().optional(),
    orgUID: z.string().min(1, "Organization is required"),
    whOrgUID: z.string().optional(),
    roleUID: z.string().min(1, "Role is required"),
    jobPositionUID: z.string().optional(),
    vehicleUID: z.string().optional(),
    locationUID: z.string().optional(),
    isActive: z.boolean().default(true),
    status: z.string().default("Active"),
    validFrom: z.string(),
    validUpto: z.string(),
    visitTime: z.string().optional(),
    endTime: z.string().optional(),
    visitDuration: z.number().min(0).max(480),
    travelTime: z.number().min(0).max(480),
    totalCustomers: z.number().default(0),
    // Schedule type selection - only backend-supported options
    scheduleType: z
      .enum(["Daily", "Weekly", "monthly", "fortnight", "multiple"])
      .default("Daily"),
    allowMultipleBeatsPerDay: z.boolean().default(false),
    isCustomerWithTime: z.boolean().default(false),
    printStanding: z.boolean().default(false),
    printForward: z.boolean().default(false),
    printTopup: z.boolean().default(false),
    printOrderSummary: z.boolean().default(false),
    autoFreezeJP: z.boolean().default(false),
    addToRun: z.boolean().default(false),
    autoFreezeRunTime: z.string().optional(),
    selectedCustomers: z.array(z.string()).default([]),
    // Customer-specific scheduling
    customerScheduling: z
      .array(
        z.object({
          customerUID: z.string(),
          frequency: z.enum(["daily", "weekly", "monthly", "fortnight"]),
          scheduleConfigs: z
            .array(
              z.object({
                scheduleType: z.string(),
                weekNumber: z.number().optional(),
                dayNumber: z.number().optional(),
              })
            )
            .default([]),
        })
      )
      .default([]),
    selectedUsers: z.array(z.string()).default([]), // For RouteUserList

    // Day selections for daily/weekly scheduling
    dailyWeeklyDays: z.object({
      monday: z.boolean().default(true),
      tuesday: z.boolean().default(true),
      wednesday: z.boolean().default(true),
      thursday: z.boolean().default(true),
      friday: z.boolean().default(true),
      saturday: z.boolean().default(false),
      sunday: z.boolean().default(false),
    }),
    // Weekly cycle configuration
    weeklyCycle: z
      .object({
        cycleLength: z.number().min(2).max(5).default(2),
        weeks: z
          .record(
            z.string(), // Allow string keys like "1", "2", etc.
            z.object({
              monday: z.boolean().default(false),
              tuesday: z.boolean().default(false),
              wednesday: z.boolean().default(false),
              thursday: z.boolean().default(false),
              friday: z.boolean().default(false),
              saturday: z.boolean().default(false),
              sunday: z.boolean().default(false),
              // Day-wise customer assignments
              dayCustomers: z
                .object({
                  monday: z.array(z.string()).default([]),
                  tuesday: z.array(z.string()).default([]),
                  wednesday: z.array(z.string()).default([]),
                  thursday: z.array(z.string()).default([]),
                  friday: z.array(z.string()).default([]),
                  saturday: z.array(z.string()).default([]),
                  sunday: z.array(z.string()).default([]),
                })
                .optional()
                .default({
                  monday: [],
                  tuesday: [],
                  wednesday: [],
                  thursday: [],
                  friday: [],
                  saturday: [],
                  sunday: [],
                }),
            })
          )
          .optional()
          .default({}),
      })
      .optional(),
  })
  .refine(
    (data) => {
      // Validate that at least one day is selected based on schedule type
      if (
        data.scheduleType === "Daily" ||
        data.scheduleType === "Weekly" ||
        data.scheduleType === "multiple"
      ) {
        const hasDaySelected = Object.values(data.dailyWeeklyDays).some(
          (day) => day === true
        );
        if (!hasDaySelected) return false;
      }

      return true;
    },
    {
      message: "At least one day must be selected for this schedule type",
      path: ["scheduleType"],
    }
  );

type RouteFormData = z.infer<typeof routeFormSchema>;

interface StoreSchedule {
  visitTime?: string;
  visitDuration?: number;
  endTime?: string;
  travelTime?: number;
  sequence?: number;
  weekOff?: {
    sunday?: boolean;
    monday?: boolean;
    tuesday?: boolean;
    wednesday?: boolean;
    thursday?: boolean;
    friday?: boolean;
    saturday?: boolean;
  };
  specialDays?: string[];
}

interface DropdownOption {
  value: string;
  label: string;
  orgTypeUID?: string;
  parentUID?: string;
}

interface StepConfig {
  num: number;
  label: string;
  mobileLabel: string;
}

const progressSteps: StepConfig[] = [
  { num: 1, label: "Basic Information", mobileLabel: "Basic" },
  { num: 2, label: "Customers", mobileLabel: "Customers" },
  { num: 3, label: "Schedule", mobileLabel: "Schedule" },
  { num: 4, label: "Review", mobileLabel: "Review" },
];

const CreateRoute: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  const currentUser = authService.getCurrentUser();
  const currentUserUID = currentUser?.uid || "SYSTEM";

  const [currentStep, setCurrentStep] = useState(1);
  const [showOptionalFields, setShowOptionalFields] = useState(false);
  const [showAdditionalUsers, setShowAdditionalUsers] = useState(false);
  const [rolePopoverOpen, setRolePopoverOpen] = useState(false);
  const [employeePopoverOpen, setEmployeePopoverOpen] = useState(false);
  const [showExcelImport, setShowExcelImport] = useState(false);

  // Clear temp customer scheduling data when starting a new route
  useEffect(() => {
    localStorage.removeItem("tempCustomerScheduling");
    localStorage.removeItem("tempDayWiseCustomers");
    localStorage.removeItem("tempScheduleCustomerAssignments");
    console.log("🧹 Cleared temp customer scheduling data for new route");
  }, []); // Only run once on mount

  const form = useForm<RouteFormData>({
    resolver: zodResolver(routeFormSchema),
    defaultValues: {
      code: "",
      name: "",
      orgUID: "",
      roleUID: "",
      jobPositionUID: "",
      whOrgUID: "",
      vehicleUID: "",
      isActive: true,
      status: "Active",
      validFrom: moment().format("YYYY-MM-DD"),
      validUpto: moment().add(1, "year").format("YYYY-MM-DD"),
      visitDuration: 0,
      travelTime: 0,
      totalCustomers: 0,
      allowMultipleBeatsPerDay: false,
      selectedCustomers: [],
      selectedUsers: [],
      description: "",
      scheduleType: "Daily" as const,
      dailyWeeklyDays: {
        monday: true,
        tuesday: true,
        wednesday: true,
        thursday: true,
        friday: true,
        saturday: false,
        sunday: false,
      },
      weeklyCycle: {
        cycleLength: 2,
        weeks: {
          "1": {
            monday: false,
            tuesday: false,
            wednesday: false,
            thursday: false,
            friday: false,
            saturday: false,
            sunday: false,
          },
          "2": {
            monday: false,
            tuesday: false,
            wednesday: false,
            thursday: false,
            friday: false,
            saturday: false,
            sunday: false,
          },
        },
      },
    },
  });

  // Destructure form methods for backward compatibility
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    reset,
    formState: { errors, isSubmitting },
    trigger,
  } = form;

  // Watch form values
  const watchedOrgUID = watch("orgUID");
  const watchedRoleUID = watch("roleUID");
  const selectedCustomers = watch("selectedCustomers");
  const selectedUsers = watch("selectedUsers");
  const storeSchedules = watch("storeSchedules" as any);
  const routeName = watch("name");

  // Loading states
  const [loading, setLoading] = useState({
    organizations: false,
    warehouses: false,
    roles: false,
    employees: false,
    reportToUsers: false,
    vehicles: false,
    locations: false,
    customers: false,
    orgTypes: false,
    nextStep: false,
  });

  // Dropdown data
  const [dropdowns, setDropdowns] = useState({
    organizations: [] as DropdownOption[],
    warehouses: [] as DropdownOption[],
    roles: [] as DropdownOption[],
    employees: [] as DropdownOption[],
    vehicles: [] as DropdownOption[],
    locations: [] as DropdownOption[],
    customers: [] as DropdownOption[],
    orgTypes: [] as any[],
  });

  // Lazy loading state for customers
  const [customersPagination, setCustomersPagination] = useState({
    currentPage: 1,
    pageSize: 100,
    totalCount: 0,
    hasMore: false,
    isLoadingMore: false,
  });

  // Scroll detection for lazy loading
  const customersScrollRef = useRef<HTMLDivElement>(null);
  const loadMoreCustomersTimeoutRef = useRef<NodeJS.Timeout>();

  // Organization hierarchy state
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);

  // Ref to track previous role to prevent infinite loops
  const previousRoleRef = useRef<string>("");
  // Removed unused orgHierarchy state - now using orgLevels for organization hierarchy
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([]); // Array of selected org UIDs from top to bottom
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]); // Dynamic levels based on selection

  // Update total customers when selection changes
  useEffect(() => {
    setValue("totalCustomers", selectedCustomers.length);
  }, [selectedCustomers, setValue]);

  // Ensure storeSchedules is always initialized
  useEffect(() => {
    if (!storeSchedules) {
      setValue("storeSchedules" as any, {});
    }
  }, [storeSchedules, setValue]);

  // Track expanded scheduling panels for each customer
  const [expandedSchedules, setExpandedSchedules] = useState<
    Record<string, boolean>
  >({});

  // Customer search/filter state
  const [customerSearch, setCustomerSearch] = useState("");

  // Multi-schedule state for Customer Selection step
  const [activeScheduleTypes, setActiveScheduleTypes] = useState<string[]>([
    "weekly",
  ]);
  const [currentlySelectedType, setCurrentlySelectedType] = useState("weekly");

  // Track customers assigned to each schedule type
  const [scheduleCustomerAssignments, setScheduleCustomerAssignments] =
    useState<{
      [scheduleType: string]: string[]; // customerUIDs
    }>({});

  // Track which customers are assigned to which schedule types
  const [assignedCustomers, setAssignedCustomers] = useState<Set<string>>(
    new Set()
  );

  // Backward compatibility alias
  const selectedPeriod = currentlySelectedType;

  // Excel import state
  const [isProcessingExcel, setIsProcessingExcel] = useState(false);

  // Update scheduleType form field when multiple schedule types are selected
  useEffect(() => {
    const scheduleTypeMap: Record<string, string> = {
      daily: "Daily",
      weekly: "Weekly",
      monthly: "monthly",
      fortnight: "fortnight",
    };

    // For multiple schedule types, we'll need to send an array or combined value
    // For now, let's use the first active schedule type for backward compatibility
    const primaryScheduleType = activeScheduleTypes[0];
    if (primaryScheduleType && scheduleTypeMap[primaryScheduleType]) {
      setValue("scheduleType", scheduleTypeMap[primaryScheduleType] as any);
    }
  }, [activeScheduleTypes, setValue]);

  // Filter customers based on current schedule type selection
  const filteredCustomers = dropdowns.customers.filter((customer) => {
    // Check if customer is assigned to any other schedule types (not current)
    const assignedToOtherSchedule = Object.entries(
      scheduleCustomerAssignments
    ).some(
      ([scheduleType, customerUIDs]) =>
        scheduleType !== currentlySelectedType &&
        customerUIDs.includes(customer.value)
    );

    // Show all customers that are not assigned to other schedule types
    // This allows showing both unassigned customers and customers assigned to current schedule
    return !assignedToOtherSchedule;
  });

  // Generate route code from name
  const generateRouteCode = useCallback((name: string) => {
    if (!name) return "";

    // Convert to uppercase and remove spaces/special characters
    const baseCode = name
      .toUpperCase()
      .replace(/[^A-Z0-9]/g, "") // Remove all special characters and spaces
      .trim()
      .substring(0, 8); // Limit to 8 characters

    // Add a short random suffix for uniqueness (3 digits)
    const randomSuffix = Math.floor(Math.random() * 900 + 100).toString();
    return `RT${baseCode}${randomSuffix}`;
  }, []);

  // Auto-generate code when name changes
  useEffect(() => {
    if (routeName && !watch("code")) {
      const generatedCode = generateRouteCode(routeName);
      setValue("code", generatedCode);
    }
  }, [routeName, watch, setValue, generateRouteCode]);

  const handleGenerateCode = () => {
    const newCode = generateRouteCode(routeName);
    setValue("code", newCode);
  };

  const loadOrganizationData = async () => {
    setLoading((prev) => ({ ...prev, organizations: true }));
    try {
      // Fetch organization types and organizations using the organizationService
      const [typesResult, orgsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 1000), // Get all orgs, then filter client-side
      ]);

      console.log("Organization Types fetched:", typesResult);
      console.log("Organizations fetched (before filter):", orgsResult.data);

      // Filter organizations to only show those with ShowInTemplate = true on client side
      const filteredOrganizations = orgsResult.data.filter(
        (org) => org.ShowInTemplate === true
      );
      console.log(
        "Filtered organizations (show_in_template = true):",
        filteredOrganizations
      );

      // Filter organization types to only show those with ShowInTemplate = true
      const filteredOrgTypes = typesResult.filter(
        (type) => type.ShowInTemplate !== false
      );

      setOrgTypes(filteredOrgTypes);
      setOrganizations(filteredOrganizations);

      // Build the hierarchy map (logged for debugging)
      const hierarchy =
        organizationService.buildOrgTypeHierarchy(filteredOrgTypes);
      console.log(
        "Organization Type Hierarchy:",
        Array.from(hierarchy.entries())
      );

      // Initialize with root level organizations using utility
      const initialLevels = initializeOrganizationHierarchy(
        filteredOrganizations,
        filteredOrgTypes
      );
      setOrgLevels(initialLevels);
    } catch (error) {
      console.error("Error fetching organization data:", error);
      toast({
        title: "Warning",
        description: "Could not load organization hierarchy",
        variant: "default",
      });
    } finally {
      setLoading((prev) => ({ ...prev, organizations: false }));
    }
  };

  // Organization hierarchy functions moved to utility file

  const handleOrganizationSelect = (levelIndex: number, value: string) => {
    if (!value) return;

    // Use utility function to handle organization selection
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

    // Set form value to the final selected organization
    const finalSelectedOrg = getFinalSelectedOrganization(updatedSelectedOrgs);
    if (finalSelectedOrg) {
      setValue("orgUID", finalSelectedOrg);
    }
  };

  const handleOrganizationReset = () => {
    // Use utility function to reset organization hierarchy
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(
      organizations,
      orgTypes
    );

    setOrgLevels(resetLevels);
    setSelectedOrgs(resetSelectedOrgs);

    // Clear form value
    setValue("orgUID", "");

    toast({
      title: "Organization Reset",
      description: "Organization selection has been cleared",
      variant: "default",
    });
  };

  const loadRoles = useCallback(async () => {
    setLoading((prev) => ({ ...prev, roles: true }));
    try {
      const data = await apiService.post("/Role/SelectAllRoles", {
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: [],
      });

      if (data.IsSuccess && data.Data?.PagedData) {
        setDropdowns((prev) => ({
          ...prev,
          roles: data.Data.PagedData.map((role: any) => ({
            value: role.UID,
            label: role.RoleNameEn || role.Code,
          })),
        }));
      }
    } catch (error) {
      console.error("Error loading roles:", error);
    } finally {
      setLoading((prev) => ({ ...prev, roles: false }));
    }
  }, []);

  const loadEmployees = useCallback(
    async (orgUID: string, roleUID?: string) => {
      setLoading((prev) => ({ ...prev, employees: true }));
      try {
        let employees = [];

        if (roleUID) {
          console.log("🔍 Loading employees for org + role:", orgUID, roleUID);

          // Always use organization + role API first (most accurate)
          try {
            console.log("🎯 Using org + role combination API");
            const orgRoleEmployees =
              await employeeService.getEmployeesSelectionItemByRoleUID(
                orgUID,
                roleUID
              );

            if (orgRoleEmployees && orgRoleEmployees.length > 0) {
              employees = orgRoleEmployees.map((item: any) => ({
                value: item.UID || item.Value || item.uid,
                label:
                  item.Label ||
                  item.Text ||
                  `[${item.Code || item.code}] ${item.Name || item.name}`,
              }));
              console.log(
                `✅ Found ${employees.length} employees for org '${orgUID}' + role '${roleUID}'`
              );
            } else {
              // Secondary: Try role-based API and then filter by org
              console.log("🔄 Trying role-based API with org filtering");
              const roleBasedEmployees =
                await employeeService.getReportsToEmployeesByRoleUID(roleUID);

              if (roleBasedEmployees && roleBasedEmployees.length > 0) {
                // Load all org employees to cross-reference
                const orgData = await api.dropdown.getEmployee(orgUID, false);
                const orgEmployeeUIDs = new Set();

                if (orgData.IsSuccess && orgData.Data) {
                  orgData.Data.forEach((emp: any) => {
                    orgEmployeeUIDs.add(emp.UID);
                  });
                }

                // Filter role employees to only include those in the selected org
                const filteredRoleEmployees = roleBasedEmployees.filter(
                  (emp: any) => orgEmployeeUIDs.has(emp.UID || emp.uid)
                );

                employees = filteredRoleEmployees.map((emp: any) => ({
                  value: emp.UID || emp.uid,
                  label: `[${emp.Code || emp.code}] ${emp.Name || emp.name}`,
                }));
                console.log(
                  `✅ Found ${employees.length} role-based employees filtered for org '${orgUID}'`
                );
              }
            }
          } catch (roleError) {
            console.error("Role-based employee loading failed:", roleError);
          }
        }

        // If no role-based employees found or no role provided, fall back to org-based loading
        if (employees.length === 0) {
          console.log("📄 Loading all employees for organization:", orgUID);
          const data = await api.dropdown.getEmployee(orgUID, false);
          if (data.IsSuccess && data.Data) {
            employees = data.Data.map((emp: any) => ({
              value: emp.UID,
              label: emp.Label,
            }));
            console.log(`📊 Loaded ${employees.length} org-based employees`);
          }
        }

        setDropdowns((prev) => ({
          ...prev,
          employees: employees,
        }));
      } catch (error) {
        console.error("Error loading employees:", error);
        // Ultimate fallback to empty array
        setDropdowns((prev) => ({
          ...prev,
          employees: [],
        }));
      } finally {
        setLoading((prev) => ({ ...prev, employees: false }));
      }
    },
    []
  );

  const loadCustomers = useCallback(
    async (
      orgUID: string,
      page: number = 1,
      append: boolean = false,
      search: string = ""
    ) => {
      if (!append) {
        setLoading((prev) => ({ ...prev, customers: true }));
      } else {
        setCustomersPagination((prev) => ({ ...prev, isLoadingMore: true }));
      }

      try {
        const pageSize = 100;

        console.log(`📦 Loading stores page ${page}, search: "${search}"...`);

        const filterCriterias = [];

        // Add org filter if provided
        if (orgUID) {
          filterCriterias.push({
            name: "FranchiseeOrgUID",
            value: orgUID,
            operator: "equals",
          });
        }

        // Add search filter if provided
        if (search && search.trim()) {
          console.log("Adding Code search filter:", search.trim());
          filterCriterias.push({
            name: "Code",
            value: search.trim(),
            operator: "contains",
          });
        }

        const response = await storeService.getAllStores({
          pageNumber: page,
          pageSize: pageSize,
          isCountRequired: page === 1,
          sortCriterias: [{ sortParameter: "Code", direction: 0 }],
          filterCriterias,
        });

        if (response.pagedData && response.pagedData.length > 0) {
          const newCustomers = response.pagedData.map((store: any) => ({
            value: store.UID || store.uid || store.Code || store.code,
            label: store.Name || store.name || store.Code || store.code,
            code:
              store.Code ||
              store.code ||
              store.Customer_Code ||
              store.customerCode ||
              "",
          }));

          if (append) {
            // Append to existing customers, filtering out duplicates
            setDropdowns((prev) => {
              const existingIds = new Set(prev.customers.map((c) => c.value));
              const uniqueNewCustomers = newCustomers.filter(
                (c: DropdownOption) => !existingIds.has(c.value)
              );
              return {
                ...prev,
                customers: [...prev.customers, ...uniqueNewCustomers],
              };
            });
          } else {
            // Replace customers
            setDropdowns((prev) => ({
              ...prev,
              customers: newCustomers,
            }));
          }

          // Update pagination state
          const totalCount =
            page === 1
              ? response.totalCount || 0
              : customersPagination.totalCount;
          setCustomersPagination({
            currentPage: page,
            pageSize: pageSize,
            totalCount: totalCount,
            hasMore:
              response.pagedData.length === pageSize &&
              (!totalCount || page * pageSize < totalCount),
            isLoadingMore: false,
          });

          console.log(
            `📊 Loaded page ${page}: ${
              response.pagedData.length
            } stores (Total in memory: ${
              append
                ? dropdowns.customers.length + newCustomers.length
                : newCustomers.length
            }/${totalCount})`
          );
        } else {
          // Try fallback API if no data
          const fallbackData = await api.dropdown.getCustomer(orgUID);
          if (fallbackData.IsSuccess && fallbackData.Data) {
            setDropdowns((prev) => ({
              ...prev,
              customers: fallbackData.Data.map((customer: any) => ({
                value: customer.UID,
                label: customer.Label,
                code: customer.Code || customer.code || "",
              })),
            }));
          }
        }
      } catch (error) {
        console.error("Error loading customers:", error);
        toast({
          title: "Error loading customers",
          description: "Failed to load customer list. Please try again.",
          variant: "destructive",
        });
        // Set empty customers list on error
        setDropdowns((prev) => ({
          ...prev,
          customers: [],
        }));
      } finally {
        setLoading((prev) => ({ ...prev, customers: false }));
      }
    },
    []
  );

  const loadVehicles = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, vehicles: true }));
    try {
      const data = await api.dropdown.getVehicle(orgUID);
      if (data.IsSuccess && data.Data && data.Data.length > 0) {
        setDropdowns((prev) => ({
          ...prev,
          vehicles: data.Data.map((vehicle: any) => ({
            value: vehicle.UID,
            label: vehicle.Label,
          })),
        }));
      }
    } catch (error) {
      console.error("Error loading vehicles:", error);
    } finally {
      setLoading((prev) => ({ ...prev, vehicles: false }));
    }
  }, []);

  const loadWarehouses = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, warehouses: true }));
    try {
      const data = await apiService.post(
        `/Dropdown/GetWareHouseTypeDropDown?parentUID=${orgUID}`
      );
      if (data.IsSuccess && data.Data) {
        setDropdowns((prev) => ({
          ...prev,
          warehouses: data.Data.map((wh: any) => ({
            value: wh.Value || wh.UID,
            label: wh.Label || wh.Name || wh.Code,
          })),
        }));
      }
    } catch (error) {
      console.error("Error loading warehouses:", error);
    } finally {
      setLoading((prev) => ({ ...prev, warehouses: false }));
    }
  }, []);

  const loadLocations = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, locations: true }));
    try {
      // Try different location endpoints with proper parameters
      const endpoints = [
        {
          method: "getByTypes",
          params: ["Country", "Region", "State", "City", "Area"], // Common location types
        },
        {
          method: "selectAll",
          params: {
            pageNumber: 0,
            pageSize: 100,
            isCountRequired: false,
            sortCriterias: [],
            filterCriterias: [
              {
                filterBy: "orgUID",
                filterValue: orgUID,
                filterOperator: "equals",
              },
            ],
          },
        },
      ];

      let locationData = [];
      for (const endpoint of endpoints) {
        try {
          const data =
            endpoint.method === "getByTypes"
              ? await apiService.post(
                  "/Location/GetLocationByTypes",
                  endpoint.params
                )
              : await apiService.post(
                  "/Location/SelectAllLocationDetails",
                  endpoint.params
                );

          if (data.IsSuccess && data.Data?.PagedData) {
            locationData = data.Data.PagedData;
            break;
          } else if (data.IsSuccess && Array.isArray(data.Data)) {
            locationData = data.Data;
            break;
          }
        } catch (err) {
          console.warn(
            `Location endpoint ${endpoint.method} failed, trying next...`,
            err
          );
        }
      }

      if (locationData.length > 0) {
        setDropdowns((prev) => ({
          ...prev,
          locations: locationData.map((loc: any) => ({
            value: loc.UID,
            label:
              loc.Name || loc.Code || `${loc.Type || "Location"} - ${loc.UID}`,
          })),
        }));
      } else {
        console.warn("No location data found - setting empty array");
        setDropdowns((prev) => ({ ...prev, locations: [] }));
      }
    } catch (error) {
      console.error("Error loading locations:", error);
      // Set empty array to avoid undefined issues
      setDropdowns((prev) => ({ ...prev, locations: [] }));
    } finally {
      setLoading((prev) => ({ ...prev, locations: false }));
    }
  }, []);

  const loadDependentDropdowns = useCallback(
    async (orgUID: string) => {
      // Load all dependent dropdowns in parallel
      // For employees, load org-based first (role filtering will happen in useEffect)
      await Promise.all([
        loadRoles(),
        loadEmployees(orgUID), // Load org-based employees first
        loadCustomers(orgUID, 1, false),
        loadVehicles(orgUID),
        loadWarehouses(orgUID),
        loadLocations(orgUID),
      ]);

      // Reset previous role ref when org changes
      previousRoleRef.current = "";
    },
    [
      loadRoles,
      loadEmployees,
      loadCustomers,
      loadVehicles,
      loadWarehouses,
      loadLocations,
    ]
  );

  // Load organizations and org types on mount
  useEffect(() => {
    loadOrganizationData();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Debounced search effect for customers
  useEffect(() => {
    const orgUID = watch("orgUID");
    if (!orgUID) return;

    const timeoutId = setTimeout(() => {
      loadCustomers(orgUID, 1, false, customerSearch);
    }, 300); // 300ms debounce

    return () => clearTimeout(timeoutId);
  }, [customerSearch, watch, loadCustomers]);

  // Load dependent dropdowns when org changes
  useEffect(() => {
    if (watchedOrgUID) {
      loadDependentDropdowns(watchedOrgUID);
    }
  }, [watchedOrgUID, loadDependentDropdowns]);

  // Reload employees when role changes (for role-based filtering)
  useEffect(() => {
    if (watchedOrgUID && previousRoleRef.current !== watchedRoleUID) {
      console.log(
        "🔄 Role changed, reloading employees:",
        previousRoleRef.current,
        "->",
        watchedRoleUID
      );
      previousRoleRef.current = watchedRoleUID;

      if (watchedRoleUID) {
        // Role selected - load role-based employees
        console.log("🎯 Loading role-based employees for:", watchedRoleUID);
        loadEmployees(watchedOrgUID, watchedRoleUID);
      } else {
        // Role cleared - load all org employees
        console.log("📄 Loading all org employees (no role filter)");
        loadEmployees(watchedOrgUID);
      }

      // Clear current employee selection since role changed
      const currentJobPositionUID = watch("jobPositionUID");
      if (currentJobPositionUID) {
        console.log("🔄 Clearing employee selection due to role change");
        setValue("jobPositionUID", "");
      }
    }
  }, [watchedRoleUID, watchedOrgUID, loadEmployees, setValue, watch]);

  const validateStep = async (step: number): Promise<boolean> => {
    console.log("Validating step:", step);
    switch (step) {
      case 1:
        return await trigger(["code", "name", "orgUID", "roleUID", "status"]);
      case 2:
        // Check if at least one customer is selected for schedule configuration
        const selectedForSchedule = watch("selectedCustomers") || [];
        if (selectedForSchedule.length === 0 && currentStep === 2) {
          // Check if there are any customers loaded
          if (dropdowns.customers.length === 0) {
            toast({
              title: "No Customers Available",
              description: "No customers found for this organization. Please check your organization selection or contact support.",
              variant: "destructive",
            });
            return false;
          }
          // Warn but allow proceeding if customers are available but none selected
          toast({
            title: "No Customers Selected",
            description: "You haven't selected any customers. You can still proceed, but you'll need to select customers to create schedules.",
            variant: "default",
          });
        }
        return true; // Allow proceeding even without selection
      case 3:
        // For multi-schedule mode with CustomerScheduler, we don't need the old validation
        // The CustomerScheduler component handles its own validation internally

        // First check if there are any customers available to schedule
        const selectedCustomersList = watch("selectedCustomers") || [];
        const allAssignedCustomers = Object.values(scheduleCustomerAssignments).flat();
        const availableCustomerUIDs = new Set([
          ...selectedCustomersList,
          ...allAssignedCustomers,
        ]);

        if (availableCustomerUIDs.size === 0) {
          toast({
            title: "No Customers Available",
            description:
              "Please select customers in Step 2 before configuring schedules",
            variant: "destructive",
          });
          return false;
        }

        // Check if we have any customers assigned to schedules
        const hasAssignedCustomers = Object.keys(
          scheduleCustomerAssignments
        ).some((type) => (scheduleCustomerAssignments[type] || []).length > 0);

        if (!hasAssignedCustomers) {
          toast({
            title: "Schedule Assignment Required",
            description:
              "Please assign the selected customers to at least one schedule type (Daily, Weekly, etc.)",
            variant: "destructive",
          });
          return false;
        }

        // Check if we have customerScheduling data (schedule configurations)
        const customerSchedulingData = watch("customerScheduling") || [];
        if (customerSchedulingData.length === 0) {
          toast({
            title: "Schedule Configuration Required",
            description:
              "Please complete the schedule configuration for your assigned customers",
            variant: "destructive",
          });
          return false;
        }

        return true;
      default:
        return true;
    }
  };

  const handleNext = async (e?: React.MouseEvent) => {
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    setLoading((prev) => ({ ...prev, nextStep: true }));
    try {
      const isValid = await validateStep(currentStep);
      if (isValid && currentStep < progressSteps.length) {
        setCurrentStep(currentStep + 1);
      }
    } finally {
      setLoading((prev) => ({ ...prev, nextStep: false }));
    }
  };

  const handlePrevious = (e?: React.MouseEvent) => {
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  // Load more customers for infinite scroll
  const loadMoreCustomers = useCallback(async () => {
    if (!customersPagination.hasMore || customersPagination.isLoadingMore) {
      return;
    }

    const orgUID = watch("orgUID");
    if (!orgUID) return;

    const nextPage = customersPagination.currentPage + 1;
    await loadCustomers(orgUID, nextPage, true, customerSearch);
  }, [
    customersPagination.hasMore,
    customersPagination.isLoadingMore,
    customersPagination.currentPage,
    watch,
    loadCustomers,
    customerSearch,
  ]);

  // Handle customer scroll for infinite loading
  const handleCustomerScroll = useCallback(
    (e: React.UIEvent<HTMLDivElement>) => {
      const element = e.currentTarget;
      const threshold = 0.8;

      // Calculate scroll position
      const scrollTop = element.scrollTop;
      const scrollHeight = element.scrollHeight;
      const clientHeight = element.clientHeight;
      const scrollPercentage = (scrollTop + clientHeight) / scrollHeight;

      // Load more if scrolled past threshold
      if (scrollPercentage >= threshold) {
        // Clear any existing timeout
        if (loadMoreCustomersTimeoutRef.current) {
          clearTimeout(loadMoreCustomersTimeoutRef.current);
        }

        // Debounce the load more call
        loadMoreCustomersTimeoutRef.current = setTimeout(() => {
          loadMoreCustomers();
        }, 200);
      }
    },
    [loadMoreCustomers]
  );

  // Auto-trigger lazy loading when filtered customers are low or empty
  useEffect(() => {
    // Check if we need to load more customers
    const shouldLoadMore =
      filteredCustomers.length < 10 && // Less than 10 customers visible
      customersPagination.hasMore && // More customers available
      !customersPagination.isLoadingMore && // Not currently loading
      watch("orgUID"); // Organization is selected

    if (shouldLoadMore) {
      // Trigger load more with a slight delay to avoid too frequent calls
      const timer = setTimeout(() => {
        loadMoreCustomers();
      }, 300);

      return () => clearTimeout(timer);
    }
  }, [
    filteredCustomers.length,
    customersPagination.hasMore,
    customersPagination.isLoadingMore,
    loadMoreCustomers,
    watch,
  ]);

  // Save scheduleCustomerAssignments to localStorage when it changes
  useEffect(() => {
    localStorage.setItem(
      "tempScheduleCustomerAssignments",
      JSON.stringify(scheduleCustomerAssignments)
    );
    console.log("💾 Saved schedule customer assignments:", scheduleCustomerAssignments);
  }, [scheduleCustomerAssignments]);

  // Clear localStorage on unmount
  useEffect(() => {
    return () => {
      // Only clear if we're leaving the route creation page
      if (!window.location.pathname.includes('route-management/routes/create')) {
        localStorage.removeItem("tempScheduleCustomerAssignments");
      }
    };
  }, []);

  // Remove auto-populate effect - daily customers should be explicitly selected
  // This was causing weekly customers to appear in daily schedule

  // Update activeScheduleTypes based on which types have customers assigned
  useEffect(() => {
    const typesWithCustomers = Object.keys(scheduleCustomerAssignments).filter(
      (type) => (scheduleCustomerAssignments[type] || []).length > 0
    );

    // Only update if there are types with customers and it's different from current
    if (typesWithCustomers.length > 0) {
      // Sort to ensure consistent order: daily, weekly, monthly, fortnight
      const orderedTypes = ["daily", "weekly", "monthly", "fortnight"];
      const sortedTypes = typesWithCustomers.sort(
        (a, b) => orderedTypes.indexOf(a) - orderedTypes.indexOf(b)
      );

      // Only update if different to avoid infinite loops
      if (JSON.stringify(sortedTypes) !== JSON.stringify(activeScheduleTypes)) {
        setActiveScheduleTypes(sortedTypes);
      }
    }
  }, [scheduleCustomerAssignments, activeScheduleTypes]);

  // Helper function to convert day name to number
  const getDayNumber = (dayName: string): number => {
    const days: { [key: string]: number } = {
      monday: 1,
      tuesday: 2,
      wednesday: 3,
      thursday: 4,
      friday: 5,
      saturday: 6,
      sunday: 7,
    };
    return days[dayName.toLowerCase()] || 1;
  };

  const onSubmit = async (values: RouteFormData) => {
    // Safety check: Only allow submission on the Review step (step 4)
    if (currentStep !== 4) {
      console.error(
        "❌ Attempted to submit form outside of Review step. Current step:",
        currentStep
      );
      toast({
        title: "Cannot Submit Yet",
        description: "Please complete all steps and review before submitting",
        variant: "destructive",
      });
      return;
    }

    console.log("Form submission started with values:", values);
    try {
      // Get storeSchedules separately since it's not in the schema
      const currentStoreSchedules = watch("storeSchedules" as any) || {};
      const routeUID = routeService.generateRouteUID();
      const scheduleUID = routeService.generateScheduleUID();

      // Generate a fresh code to ensure uniqueness
      const freshCode = generateRouteCode(values.name);

      // Helper function to generate config UID that matches existing master data
      const getConfigUID = (
        scheduleType: string,
        weekNumber: number | null,
        dayNumber: number
      ) => {
        let weekStr = "NA";
        let finalDayNumber = dayNumber;
        let finalScheduleType = scheduleType;

        if (scheduleType === "daily") {
          // For daily, use day 0 as per master data: daily_NA_0
          finalDayNumber = 0;
        } else if (scheduleType === "weekly") {
          // For weekly, use the actual week number (W1, W2, W3, W4, W5)
          weekStr = weekNumber ? `W${weekNumber}` : "W1";
        } else if (
          scheduleType === "fortnight" ||
          scheduleType === "fortnightly"
        ) {
          // Use the correct spelling from master data: "fortnigtly"
          finalScheduleType = "fortnigtly";
          // For fortnight, use W13 or W24 based on the pattern
          weekStr =
            weekNumber === 13 ? "W13" : weekNumber === 24 ? "W24" : "W13";
        } else if (scheduleType === "monthly") {
          // Monthly uses specific dates (1-31)
          finalDayNumber = dayNumber; // Keep the day number as-is for monthly
        }

        // Match the existing DB format: schedule_type_weekStr_dayNumber
        const configUID = `${finalScheduleType}_${weekStr}_${finalDayNumber}`;
        console.log(
          `🔍 Generated configUID: ${configUID} for ${scheduleType}, week: ${weekNumber}, day: ${dayNumber}`
        );
        return configUID;
      };

      // Helper function to get selected days for scheduling
      const getSelectedDaysForScheduling = (dailyWeeklyDays: any) => {
        const selectedDays: { day: string; dayNumber: number }[] = [];
        const dayNumberMapping = {
          monday: 1,
          tuesday: 2,
          wednesday: 3,
          thursday: 4,
          friday: 5,
          saturday: 6,
          sunday: 7,
        };

        Object.entries(dailyWeeklyDays).forEach(([key, selected]) => {
          if (
            selected &&
            dayNumberMapping[key as keyof typeof dayNumberMapping]
          ) {
            selectedDays.push({
              day: key,
              dayNumber: dayNumberMapping[key as keyof typeof dayNumberMapping],
            });
          }
        });

        return selectedDays;
      };

      // Helper function to create schedule mapping object
      const createScheduleMapping = (
        uid: string,
        scheduleUID: string,
        configUID: string,
        customerUID: string,
        seqNo: number,
        values: any,
        userUID: string
      ) => ({
        UID: uid,
        RouteScheduleUID: scheduleUID,
        RouteScheduleConfigUID: configUID,
        CustomerUID: customerUID,
        SeqNo: seqNo,
        StartTime: values.visitTime ? values.visitTime + ":00" : "00:00:00",
        EndTime: values.endTime ? values.endTime + ":00" : "00:00:00",
        IsDeleted: false,
        CreatedBy: userUID,
        CreatedTime: undefined,
        ModifiedBy: userUID,
        ModifiedTime: undefined,
        ServerAddTime: undefined,
        ServerModifiedTime: undefined,
      });

      // Don't create new configs - use existing master configs
      // RouteScheduleConfigs should be empty array for routes
      const routeScheduleConfigs: any[] = [];
      console.log(
        "📋 RouteScheduleConfigs:",
        routeScheduleConfigs,
        "(empty - using master configs)"
      );

      // Create a shared customer sequence map for consistent numbering
      const customerSequenceMap = new Map<string, number>();
      let sequenceCounter = 1;

      // Aggregate all schedule configs by customer UID
      const aggregatedCustomerScheduling = new Map<
        string,
        {
          customerUID: string;
          frequency: "daily" | "weekly" | "monthly" | "fortnight";
          scheduleConfigs: any[];
        }
      >();

      // First, add ALL customers from scheduleCustomerAssignments (all selected customers by frequency)
      console.log(
        "📝 Processing ALL selected customers from scheduleCustomerAssignments:",
        scheduleCustomerAssignments
      );

      Object.entries(scheduleCustomerAssignments).forEach(
        ([frequency, customerUIDs]) => {
          if (customerUIDs && customerUIDs.length > 0) {
            console.log(
              `📝 Adding ${customerUIDs.length} customers for ${frequency} frequency`
            );

            customerUIDs.forEach((customerUID: string) => {
              if (!aggregatedCustomerScheduling.has(customerUID)) {
                // Add customer with default config for their frequency
                aggregatedCustomerScheduling.set(customerUID, {
                  customerUID: customerUID,
                  frequency: frequency as
                    | "daily"
                    | "weekly"
                    | "monthly"
                    | "fortnight",
                  scheduleConfigs: [], // Will be populated below if specific scheduling exists
                });
                // Assign sequence number
                customerSequenceMap.set(customerUID, sequenceCounter++);
                console.log(
                  `📝 Added customer ${customerUID} with ${frequency} frequency, SeqNo ${
                    sequenceCounter - 1
                  }`
                );
              }
            });
          }
        }
      );

      // Then, process specific customer scheduling if it exists (for customers with day/date assignments)
      if (values.customerScheduling && values.customerScheduling.length > 0) {
        console.log(
          "📝 Processing specific customer scheduling entries:",
          values.customerScheduling.length
        );
        console.log(
          "📝 Raw customerScheduling data:",
          JSON.stringify(values.customerScheduling, null, 2)
        );

        values.customerScheduling.forEach((customerSched, index) => {
          console.log(
            `📝 Processing entry ${index + 1}: Customer ${
              customerSched.customerUID
            }, Frequency: ${customerSched.frequency}, Configs: ${
              customerSched.scheduleConfigs.length
            }`
          );

          const existing = aggregatedCustomerScheduling.get(
            customerSched.customerUID
          );

          if (existing) {
            // Customer already exists - update schedule configs
            existing.scheduleConfigs = [...customerSched.scheduleConfigs];
            console.log(
              `📝 Updated configs for customer ${customerSched.customerUID}, total configs: ${existing.scheduleConfigs.length}`
            );
          } else {
            // This shouldn't happen if scheduleCustomerAssignments is properly populated
            // But add as fallback
            aggregatedCustomerScheduling.set(customerSched.customerUID, {
              customerUID: customerSched.customerUID,
              frequency: customerSched.frequency,
              scheduleConfigs: [...customerSched.scheduleConfigs],
            });
            // Assign sequence number if not already assigned
            if (!customerSequenceMap.has(customerSched.customerUID)) {
              customerSequenceMap.set(
                customerSched.customerUID,
                sequenceCounter++
              );
            }
            console.log(
              `📝 Added new customer ${
                customerSched.customerUID
              } from scheduling with SeqNo ${customerSequenceMap.get(
                customerSched.customerUID
              )}`
            );
          }
        });
      }

      console.log(
        "📝 Total aggregated customers:",
        aggregatedCustomerScheduling.size
      );
      console.log(
        "📝 Final aggregated data:",
        Array.from(aggregatedCustomerScheduling.entries()).map(
          ([uid, data]) => ({
            uid,
            frequency: data.frequency,
            configCount: data.scheduleConfigs.length,
            configs: data.scheduleConfigs,
          })
        )
      );
      console.log("📝 Customer sequence map created:", customerSequenceMap);

      // Debug logging before building data
      console.log("📝 DEBUG: Building routeMasterData with:");
      console.log("  - customerScheduling:", values.customerScheduling);
      console.log(
        "  - scheduledCustomers:",
        watch("scheduledCustomers" as any)
      );
      console.log("  - currentStoreSchedules:", currentStoreSchedules);

      // Build route master data
      const routeMasterData = {
        Route: {
          UID: routeUID,
          CompanyUID: values.orgUID,
          Code: freshCode,
          Name: values.name,
          OrgUID: values.orgUID,
          WHOrgUID: values.whOrgUID || undefined, // Send undefined instead of null
          RoleUID: values.roleUID,
          JobPositionUID: values.jobPositionUID || undefined, // Send undefined instead of null
          VehicleUID: values.vehicleUID || undefined, // Send undefined instead of null
          LocationUID: values.locationUID || undefined, // Ensure undefined is sent
          IsActive: values.isActive,
          Status: values.status,
          ValidFrom: values.validFrom,
          ValidUpto: values.validUpto,
          VisitTime: values.visitTime || undefined, // Send undefined instead of null
          EndTime: values.endTime || undefined, // Send undefined instead of null
          VisitDuration: values.visitDuration,
          TravelTime: values.travelTime,
          TotalCustomers: aggregatedCustomerScheduling.size, // Use total aggregated customers
          IsCustomerWithTime: values.isCustomerWithTime,
          PrintStanding: values.printStanding,
          PrintForward: values.printForward,
          PrintTopup: values.printTopup,
          PrintOrderSummary: values.printOrderSummary,
          AutoFreezeJP: values.autoFreezeJP,
          AddToRun: values.addToRun,
          AutoFreezeRunTime: values.autoFreezeRunTime || undefined, // Send undefined instead of empty string
          IsChangeApplied: false, // Backend expects this for new routes
          User: currentUserUID, // Backend required field
          CreatedBy: currentUserUID,
          CreatedTime: undefined,
          ModifiedBy: currentUserUID,
          ModifiedTime: undefined,
          ServerAddTime: undefined,
          ServerModifiedTime: undefined,
        },
        RouteSchedule: {
          UID: scheduleUID, // Use proper schedule UID
          CompanyUID: values.orgUID,
          RouteUID: routeUID,
          Name: values.name + " Schedule",
          Type: (() => {
            // Determine the schedule type based on customer scheduling data OR form selection
            if (aggregatedCustomerScheduling.size > 0) {
              // Get all unique frequencies from customer scheduling
              const frequencyCount = new Map<string, number>();
              aggregatedCustomerScheduling.forEach((customerData) => {
                const count = frequencyCount.get(customerData.frequency) || 0;
                frequencyCount.set(customerData.frequency, count + 1);
              });

              const uniqueFrequencies = Array.from(frequencyCount.keys());
              console.log(
                `🔍 Found ${uniqueFrequencies.length} unique frequencies:`,
                uniqueFrequencies
              );

              if (uniqueFrequencies.length === 1) {
                // Single frequency type - use it directly
                const singleFrequency = uniqueFrequencies[0];
                console.log(`🔍 Single frequency type: ${singleFrequency}`);
                return singleFrequency;
              } else if (uniqueFrequencies.length > 1) {
                // Multiple frequency types - use "multiple"
                console.log(
                  `🔍 Multiple frequencies detected:`,
                  Array.from(frequencyCount.entries())
                );
                console.log(`🔍 Using multiple for mixed schedule route`);
                return "multiple";
              } else {
                // No frequencies found - fallback to daily
                console.log(`🔍 No frequencies found, defaulting to daily`);
                return "daily";
              }
            } else {
              // Use the form-selected schedule type or handle multiple active schedule types
              const formScheduleType =
                values.scheduleType?.toLowerCase() || "daily";

              // Check if user selected multiple schedule types
              if (activeScheduleTypes.length > 1) {
                console.log(
                  `🔍 Multiple schedule types selected: ${activeScheduleTypes.join(
                    ", "
                  )}`
                );
                // When multiple types are selected, use "multiple" as the schedule type
                return "multiple";
              } else if (activeScheduleTypes.length === 1) {
                console.log(
                  `🔍 Single schedule type from activeScheduleTypes: ${activeScheduleTypes[0]}`
                );
                return activeScheduleTypes[0];
              } else {
                console.log(`🔍 Using form schedule type: ${formScheduleType}`);
                return formScheduleType;
              }
            }
          })(),
          StartDate: values.validFrom,
          Status: 1,
          FromDate: values.validFrom,
          ToDate: values.validUpto,
          StartTime: values.visitTime ? values.visitTime + ":00" : "00:00:00", // Default to 00:00
          EndTime: values.endTime ? values.endTime + ":00" : "00:00:00", // Default to 00:00
          VisitDurationInMinutes: values.visitDuration,
          TravelTimeInMinutes: values.travelTime,
          NextBeatDate: values.validFrom,
          LastBeatDate: values.validFrom,
          AllowMultipleBeatsPerDay: values.allowMultipleBeatsPerDay,
          PlannedDays: (() => {
            // Get the actual schedule type we determined above
            let actualScheduleType = "";

            if (aggregatedCustomerScheduling.size > 0) {
              const frequencyCount = new Map<string, number>();
              aggregatedCustomerScheduling.forEach((customerData) => {
                const count = frequencyCount.get(customerData.frequency) || 0;
                frequencyCount.set(customerData.frequency, count + 1);
              });

              const uniqueFrequencies = Array.from(frequencyCount.keys());

              if (uniqueFrequencies.length === 1) {
                actualScheduleType = uniqueFrequencies[0];
              } else if (uniqueFrequencies.length > 1) {
                actualScheduleType = "multiple";
              } else {
                actualScheduleType = "daily";
              }
            } else if (activeScheduleTypes.length > 1) {
              actualScheduleType = "multiple";
            } else if (activeScheduleTypes.length === 1) {
              actualScheduleType = activeScheduleTypes[0];
            } else {
              actualScheduleType =
                values.scheduleType?.toLowerCase() || "daily";
            }

            console.log(
              `🔍 Determining planned days for schedule type: ${actualScheduleType}`
            );

            if (
              actualScheduleType === "daily" ||
              actualScheduleType === "weekly" ||
              actualScheduleType === "multiple"
            ) {
              // For daily/weekly schedules, use the selected days
              const selectedDays: string[] = [];
              const dayMapping = {
                monday: "Monday",
                tuesday: "Tuesday",
                wednesday: "Wednesday",
                thursday: "Thursday",
                friday: "Friday",
                saturday: "Saturday",
                sunday: "Sunday",
              };

              Object.entries(values.dailyWeeklyDays).forEach(
                ([key, selected]) => {
                  if (selected && dayMapping[key as keyof typeof dayMapping]) {
                    selectedDays.push(
                      dayMapping[key as keyof typeof dayMapping]
                    );
                  }
                }
              );

              // Return as JSON array instead of comma-separated string
              const plannedDaysJson = JSON.stringify(selectedDays);
              console.log(
                `🔍 Generated planned days JSON for ${actualScheduleType}:`,
                plannedDaysJson
              );
              return plannedDaysJson;
            } else if (actualScheduleType === "monthly") {
              // For monthly, we could save selected dates
              return JSON.stringify([]);
            } else if (actualScheduleType === "fortnight") {
              // For fortnight, save the selected days
              const selectedDays: string[] = [];
              const dayMapping = {
                monday: "Monday",
                tuesday: "Tuesday",
                wednesday: "Wednesday",
                thursday: "Thursday",
                friday: "Friday",
                saturday: "Saturday",
                sunday: "Sunday",
              };

              Object.entries(values.dailyWeeklyDays).forEach(
                ([key, selected]) => {
                  if (selected && dayMapping[key as keyof typeof dayMapping]) {
                    selectedDays.push(
                      dayMapping[key as keyof typeof dayMapping]
                    );
                  }
                }
              );

              return JSON.stringify(selectedDays);
            }

            return JSON.stringify([]);
          })(),
          SS: null, // Backend sync status field
          CreatedBy: currentUserUID,
          CreatedTime: undefined,
          ModifiedBy: currentUserUID,
          ModifiedTime: undefined,
          ServerAddTime: undefined,
          ServerModifiedTime: undefined,
        },
        RouteScheduleConfigs: routeScheduleConfigs,
        RouteScheduleCustomerMappings: (() => {
          const mappings: any[] = [];

          if (aggregatedCustomerScheduling.size > 0) {
            console.log(
              "🔍 DEBUG: Creating mappings from aggregated customers:",
              aggregatedCustomerScheduling.size
            );

            // Create mappings from the aggregated customer data
            aggregatedCustomerScheduling.forEach((customerData) => {
              console.log(
                `🔍 DEBUG: Processing customer ${customerData.customerUID}:`,
                customerData
              );

              // Get the customer's sequence number from the shared map
              const customerSequence =
                customerSequenceMap.get(customerData.customerUID) || 1;

              // For daily schedules, create only ONE mapping per customer (not 7 duplicates)
              if (customerData.frequency === "daily") {
                console.log(
                  `  🔍 DEBUG: Daily schedule - creating single mapping for customer ${customerData.customerUID}`
                );

                const configUID = getConfigUID("daily", null, 0); // daily_NA_0
                console.log(
                  `  🔍 DEBUG: Generated daily configUID: ${configUID}`
                );

                const mapping = {
                  UID: routeService.generateScheduleUID(),
                  RouteScheduleUID: scheduleUID,
                  RouteScheduleConfigUID: configUID, // Use the daily master config UID
                  CustomerUID: customerData.customerUID,
                  SeqNo: customerSequence, // Use consistent sequence number per customer
                  StartTime: values.visitTime
                    ? values.visitTime + ":00"
                    : "00:00:00",
                  EndTime: values.endTime ? values.endTime + ":00" : "00:00:00",
                  IsDeleted: false,
                  CreatedBy: currentUserUID,
                  CreatedTime: undefined,
                  ModifiedBy: currentUserUID,
                  ModifiedTime: undefined,
                  ServerAddTime: undefined,
                  ServerModifiedTime: undefined,
                };

                console.log(
                  `  🔍 DEBUG: Created SINGLE daily mapping with SeqNo ${customerSequence} for ${customerData.customerUID} -> ${configUID}:`,
                  mapping
                );
                mappings.push(mapping);
              } else {
                // For weekly/monthly/fortnight schedules
                if (customerData.scheduleConfigs.length > 0) {
                  // Process each specific schedule config if they exist
                  customerData.scheduleConfigs.forEach((schedConfig) => {
                    console.log(
                      `  🔍 DEBUG: Processing schedConfig:`,
                      schedConfig
                    );

                    // Generate the config UID that matches existing master data
                    const configUID = getConfigUID(
                      schedConfig.scheduleType,
                      schedConfig.weekNumber || null,
                      schedConfig.dayNumber || 1
                    );

                    console.log(
                      `  🔍 DEBUG: Generated configUID: ${configUID}`
                    );

                    const mapping = {
                      UID: routeService.generateScheduleUID(),
                      RouteScheduleUID: scheduleUID,
                      RouteScheduleConfigUID: configUID, // Use the master config UID
                      CustomerUID: customerData.customerUID,
                      SeqNo: customerSequence, // Use consistent sequence number per customer
                      StartTime: values.visitTime
                        ? values.visitTime + ":00"
                        : "00:00:00",
                      EndTime: values.endTime
                        ? values.endTime + ":00"
                        : "00:00:00",
                      IsDeleted: false,
                      CreatedBy: currentUserUID,
                      CreatedTime: undefined,
                      ModifiedBy: currentUserUID,
                      ModifiedTime: undefined,
                      ServerAddTime: undefined,
                      ServerModifiedTime: undefined,
                    };

                    console.log(
                      `  🔍 DEBUG: Created mapping with SeqNo ${customerSequence} for ${customerData.customerUID} -> ${configUID}:`,
                      mapping
                    );
                    mappings.push(mapping);
                  });
                } else {
                  // No specific schedule configs - DO NOT create any mapping
                  // These customers are saved in route_customer but not scheduled yet
                  console.log(
                    `  🔍 DEBUG: No specific configs for ${customerData.frequency} customer ${customerData.customerUID}, skipping schedule mapping`
                  );
                  // Customer will be in route_customer but not in route_schedule_customer_mapping
                  // This is intentional - they're assigned to the route but not scheduled to specific days/weeks
                }
              }
            });
          } else {
            // No custom customer scheduling - handle multiple schedule types or form selection
            console.log(
              `🔧 No customer scheduling data - handling ${activeScheduleTypes.length} schedule types`
            );

            let mappingSeqNo = 1;

            if (activeScheduleTypes.length > 1) {
              // Multiple schedule types selected - create mappings for each type
              console.log(
                `📊 Multiple schedule types: ${activeScheduleTypes.join(", ")}`
              );

              activeScheduleTypes.forEach((scheduleType) => {
                const customerUIDs =
                  scheduleCustomerAssignments[scheduleType] || [];
                console.log(
                  `🔄 Processing ${scheduleType} with ${customerUIDs.length} customers:`,
                  customerUIDs
                );

                customerUIDs.forEach((customerUID) => {
                  generateMappingsForCustomerAndScheduleType(
                    customerUID,
                    scheduleType,
                    mappingSeqNo++
                  );
                });
              });
            } else {
              // Single schedule type - use form selection or first active type
              const formScheduleType =
                values.scheduleType?.toLowerCase() ||
                activeScheduleTypes[0] ||
                "daily";
              console.log(
                `📊 Single schedule type: ${formScheduleType} for all selected customers`
              );

              values.selectedCustomers.forEach((customerUID) => {
                generateMappingsForCustomerAndScheduleType(
                  customerUID,
                  formScheduleType,
                  mappingSeqNo++
                );
              });
            }

            // Helper function to generate mappings for a specific customer and schedule type
            function generateMappingsForCustomerAndScheduleType(
              customerUID: string,
              scheduleType: string,
              baseSeqNo: number
            ) {
              if (scheduleType === "daily") {
                // For daily schedules, create single mapping with daily config
                const configUID = getConfigUID("daily", null, 0); // daily_NA_0

                const mapping = createScheduleMapping(
                  routeService.generateScheduleUID(),
                  scheduleUID,
                  configUID,
                  customerUID,
                  baseSeqNo,
                  values,
                  currentUserUID
                );

                mappings.push(mapping);
                console.log(
                  `📋 Created daily mapping for ${customerUID} -> ${configUID}`
                );
              } else if (scheduleType === "weekly") {
                // For weekly schedules, create mappings based on selected days
                const selectedDays = getSelectedDaysForScheduling(
                  values.dailyWeeklyDays
                );

                selectedDays.forEach(({ day, dayNumber }) => {
                  const configUID = getConfigUID("weekly", 1, dayNumber); // weekly_W1_dayNumber

                  const mapping = createScheduleMapping(
                    routeService.generateScheduleUID(),
                    scheduleUID,
                    configUID,
                    customerUID,
                    baseSeqNo,
                    values,
                    currentUserUID
                  );

                  mappings.push(mapping);
                  console.log(
                    `📋 Created weekly mapping for ${customerUID} on ${day} -> ${configUID}`
                  );
                });
              } else if (scheduleType === "monthly") {
                // For monthly schedules, create mappings for each day of month (1-31)
                const selectedDays = getSelectedDaysForScheduling(
                  values.dailyWeeklyDays
                );

                if (selectedDays.length > 0) {
                  selectedDays.forEach(({ dayNumber }) => {
                    const configUID = getConfigUID("monthly", null, dayNumber); // monthly_NA_dayNumber

                    const mapping = createScheduleMapping(
                      routeService.generateScheduleUID(),
                      scheduleUID,
                      configUID,
                      customerUID,
                      baseSeqNo,
                      values,
                      currentUserUID
                    );

                    mappings.push(mapping);
                    console.log(
                      `📋 Created monthly mapping for ${customerUID} on day ${dayNumber} -> ${configUID}`
                    );
                  });
                } else {
                  // Default monthly mapping for day 1
                  const configUID = getConfigUID("monthly", null, 1); // monthly_NA_1

                  const mapping = createScheduleMapping(
                    routeService.generateScheduleUID(),
                    scheduleUID,
                    configUID,
                    customerUID,
                    baseSeqNo,
                    values,
                    currentUserUID
                  );

                  mappings.push(mapping);
                  console.log(
                    `📋 Created default monthly mapping for ${customerUID} -> ${configUID}`
                  );
                }
              } else if (
                scheduleType === "fortnight" ||
                scheduleType === "fortnightly"
              ) {
                // For fortnightly schedules, create mappings for selected days in both weeks
                const selectedDays = getSelectedDaysForScheduling(
                  values.dailyWeeklyDays
                );

                // Create mappings for both W13 and W24 (fortnightly pattern)
                [13, 24].forEach((weekNumber) => {
                  selectedDays.forEach(({ day, dayNumber }) => {
                    const configUID = getConfigUID(
                      "fortnigtly",
                      weekNumber,
                      dayNumber
                    ); // fortnigtly_W13_dayNumber or fortnigtly_W24_dayNumber

                    const mapping = createScheduleMapping(
                      routeService.generateScheduleUID(),
                      scheduleUID,
                      configUID,
                      customerUID,
                      baseSeqNo,
                      values,
                      currentUserUID
                    );

                    mappings.push(mapping);
                    console.log(
                      `📋 Created fortnightly mapping for ${customerUID} on ${day} week ${weekNumber} -> ${configUID}`
                    );
                  });
                });
              } else if (scheduleType === "multiple") {
                // For multiple frequency types, create default mappings
                const selectedDays = getSelectedDaysForScheduling(
                  values.dailyWeeklyDays
                );
                const cycleLength = values.weeklyCycle?.cycleLength || 2;

                // Create mappings for each week in the cycle
                for (let week = 1; week <= cycleLength; week++) {
                  selectedDays.forEach(({ day, dayNumber }) => {
                    const configUID = getConfigUID("weekly", week, dayNumber); // weekly_W1_dayNumber, weekly_W2_dayNumber, etc.

                    const mapping = createScheduleMapping(
                      routeService.generateScheduleUID(),
                      scheduleUID,
                      configUID,
                      customerUID,
                      baseSeqNo,
                      values,
                      currentUserUID
                    );

                    mappings.push(mapping);
                    console.log(
                      `📋 Created multiple-per-week mapping for ${customerUID} on ${day} week ${week} -> ${configUID}`
                    );
                  });
                }
              }
            }
          }

          console.log(
            `🔗 Generated ${mappings.length} RouteScheduleCustomerMappings:`,
            mappings
          );
          return mappings;
        })(),
        RouteScheduleWeeklyCycleList: [],
        RouteCustomersList: (() => {
          const routeCustomers: any[] = [];

          // Use the aggregated customer scheduling to create unique customer records
          if (aggregatedCustomerScheduling.size > 0) {
            console.log(
              "📊 Creating RouteCustomersList from aggregated customers:",
              aggregatedCustomerScheduling.size
            );

            aggregatedCustomerScheduling.forEach((customerData) => {
              // Get the sequence number from the shared map
              const customerSeqNo =
                customerSequenceMap.get(customerData.customerUID) || 1;

              const storeSchedule: StoreSchedule = (currentStoreSchedules &&
                currentStoreSchedules[customerData.customerUID]) || {
                visitTime: "",
                visitDuration: 0,
                endTime: "",
                travelTime: 0,
                sequence: customerSeqNo,
                weekOff: {
                  sunday: false,
                  monday: false,
                  tuesday: false,
                  wednesday: false,
                  thursday: false,
                  friday: false,
                  saturday: false,
                },
                specialDays: [],
              };

              routeCustomers.push({
                UID: `${routeUID}_${customerData.customerUID}`,
                RouteUID: routeUID,
                StoreUID: customerData.customerUID,
                SeqNo: customerSeqNo, // Use the shared sequence number
                VisitTime: values.visitTime
                  ? values.visitTime + ":00"
                  : "00:00:00",
                VisitDuration: storeSchedule.visitDuration || 0,
                EndTime: values.endTime ? values.endTime + ":00" : "00:00:00",
                IsDeleted: false,
                TravelTime: storeSchedule.travelTime || 0,
                ActionType: "Add",
                Frequency: customerData.frequency, // Use the scheduled frequency
                CreatedBy: currentUserUID,
                CreatedTime: undefined,
                ModifiedBy: currentUserUID,
                ModifiedTime: undefined,
                ServerAddTime: undefined,
                ServerModifiedTime: undefined,
              });

              console.log(
                `📋 Added customer ${customerData.customerUID} with frequency: ${customerData.frequency}, configs: ${customerData.scheduleConfigs.length}`
              );
            });
          } else {
            // No customers scheduled - use form-selected schedule type with all selected customers
            const formScheduleType =
              values.scheduleType?.toLowerCase() || "daily";
            console.log(
              `📊 ${formScheduleType.toUpperCase()} schedule detected - saving ALL selected customers:`,
              values.selectedCustomers
            );
            let seqNo = 1;
            values.selectedCustomers.forEach((customerUID) => {
              const storeSchedule: StoreSchedule = (currentStoreSchedules &&
                currentStoreSchedules[customerUID]) || {
                visitTime: "",
                visitDuration: 0,
                endTime: "",
                travelTime: 0,
                sequence: seqNo,
                weekOff: {
                  sunday: false,
                  monday: false,
                  tuesday: false,
                  wednesday: false,
                  thursday: false,
                  friday: false,
                  saturday: false,
                },
                specialDays: [],
              };

              routeCustomers.push({
                UID: `${routeUID}_${customerUID}`,
                RouteUID: routeUID,
                StoreUID: customerUID,
                SeqNo: seqNo++,
                VisitTime: values.visitTime
                  ? values.visitTime + ":00"
                  : "00:00:00",
                VisitDuration: storeSchedule.visitDuration || 0,
                EndTime: values.endTime ? values.endTime + ":00" : "00:00:00",
                IsDeleted: false,
                TravelTime: storeSchedule.travelTime || 0,
                ActionType: "Add",
                Frequency: values.scheduleType?.toLowerCase() || "daily", // Use form schedule type
                CreatedBy: currentUserUID,
                CreatedTime: undefined,
                ModifiedBy: currentUserUID,
                ModifiedTime: undefined,
                ServerAddTime: undefined,
                ServerModifiedTime: undefined,
              });

              console.log(
                `📋 Added ${formScheduleType.toUpperCase()} customer ${customerUID}`
              );
            });
          }

          console.log("📦 Final RouteCustomersList:", routeCustomers);
          return routeCustomers;
        })(),
        // Add RouteUserList with PRIMARY user + selected additional users
        RouteUserList: [
          // ALWAYS include the primary employee as the main route user
          {
            UID: `${routeUID}_USER_PRIMARY_${values.jobPositionUID}`,
            RouteUID: routeUID,
            JobPositionUID: values.jobPositionUID,
            FromDate: values.validFrom,
            ToDate: values.validUpto,
            IsActive: true,
            CreatedBy: currentUserUID,
            CreatedTime: undefined,
            ModifiedBy: currentUserUID,
            ModifiedTime: undefined,
          },
          // Add any selected additional users
          ...values.selectedUsers.map((userUID) => ({
            UID: `${routeUID}_USER_ADDITIONAL_${userUID}`,
            RouteUID: routeUID,
            JobPositionUID: userUID,
            FromDate: values.validFrom,
            ToDate: values.validUpto,
            IsActive: true,
            CreatedBy: currentUserUID,
            CreatedTime: undefined,
            ModifiedBy: currentUserUID,
            ModifiedTime: undefined,
          })),
        ],
      };

      // Debug logging
      console.log(
        "Route Master Data being sent:",
        JSON.stringify(routeMasterData, null, 2)
      );
      console.log("Data structure validation:", {
        hasRoute: !!routeMasterData.Route,
        hasRouteSchedule: !!routeMasterData.RouteSchedule,
        hasRouteCustomersList: !!routeMasterData.RouteCustomersList,
        customerCount: routeMasterData.RouteCustomersList.length,
        createdByType: typeof routeMasterData.Route.CreatedBy,
        createdByValue: routeMasterData.Route.CreatedBy,
      });

      // Always use CreateRouteMaster API to ensure all data is sent correctly
      // This includes RouteScheduleCustomerMappings which is critical for the new scheduling system
      console.log(
        "Sending RouteMaster with full scheduling data:",
        routeMasterData
      );

      // Log critical data for debugging
      console.log("🔍 CRITICAL DEBUG - Data being sent to API:");
      console.log(
        "  - RouteScheduleCustomerMappings count:",
        routeMasterData.RouteScheduleCustomerMappings?.length || 0
      );
      console.log(
        "  - RouteCustomersList count:",
        routeMasterData.RouteCustomersList?.length || 0
      );

      if (routeMasterData.RouteScheduleCustomerMappings?.length > 0) {
        console.log(
          "  - Sample mapping:",
          routeMasterData.RouteScheduleCustomerMappings[0]
        );
      }
      if (routeMasterData.RouteCustomersList?.length > 0) {
        console.log(
          "  - Sample customer:",
          routeMasterData.RouteCustomersList[0]
        );
      }

      const response = await apiService.post(
        "/Route/CreateRouteMaster",
        routeMasterData
      );

      if (response.IsSuccess || response.Data > 0) {
        // Count stores with custom schedules
        const storesWithSchedules = values.selectedCustomers.filter(
          (customerUID) => {
            const schedule: StoreSchedule | undefined =
              currentStoreSchedules && currentStoreSchedules[customerUID];
            return (
              schedule &&
              (schedule.visitTime ||
                schedule.visitDuration !== 0 ||
                schedule.travelTime !== 0 ||
                (schedule.weekOff &&
                  Object.values(schedule.weekOff).some((day) => day)))
            );
          }
        ).length;

        // Clear temp customer scheduling data after successful creation
        localStorage.removeItem("tempCustomerScheduling");
        localStorage.removeItem("tempDayWiseCustomers");
        console.log(
          "🧹 Cleared temp customer scheduling data after successful route creation"
        );

        // Get the actual customer count and frequency details
        const totalCustomers =
          aggregatedCustomerScheduling.size || values.selectedCustomers.length;
        const scheduledCustomers =
          routeMasterData.RouteScheduleCustomerMappings?.length || 0;

        // Get frequency breakdown if multiple types
        let frequencyDetails = "";
        if (activeScheduleTypes.length > 1) {
          const breakdown = activeScheduleTypes
            .map((type) => {
              const count = scheduleCustomerAssignments[type]?.length || 0;
              return `${type}: ${count}`;
            })
            .join(", ");
          frequencyDetails = ` (${breakdown})`;
        }

        toast({
          title: "✅ Route Created Successfully",
          description: `Route "${
            values.name
          }" has been created with ${totalCustomers} customer${
            totalCustomers !== 1 ? "s" : ""
          }${frequencyDetails}. ${
            scheduledCustomers > 0
              ? `${scheduledCustomers} customer${
                  scheduledCustomers !== 1 ? "s" : ""
                } scheduled to specific days.`
              : "Customers assigned but not yet scheduled to specific days."
          }`,
        });
        router.push("/distributiondelivery/route-management/routes");
      } else {
        throw new Error(
          response.ErrorMessage || response.Message || "Failed to create route"
        );
      }
    } catch (error: any) {
      console.error("Route creation error:", error);
      console.error("Error details:", error.details);
      console.error("Error status:", error.status);

      // Extract more specific error message
      const errorMessage =
        error.details?.Message ||
        error.details?.message ||
        error.message ||
        "Failed to create route. Backend returned an error.";

      // Log full error for debugging
      if (error.status === 500) {
        console.error(
          "Backend server error (500) - The route creation transaction is failing."
        );
        console.error(
          "This is likely due to a database constraint or backend bug."
        );
      }

      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive",
      });
    }
  };

  // Process Excel file directly
  const processExcelFile = async (file: File) => {
    setIsProcessingExcel(true);

    try {
      const data = await file.arrayBuffer();
      const workbook = XLSX.read(data);
      const sheetName = workbook.SheetNames[0];
      const worksheet = workbook.Sheets[sheetName];

      // Convert to JSON
      const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

      if (jsonData.length < 2) {
        throw new Error(
          "Excel file must contain at least a header row and one data row"
        );
      }

      // Get headers (first row)
      const headers = jsonData[0] as string[];

      // Find column indices
      const nameIndex = headers.findIndex(
        (h) =>
          h?.toLowerCase().includes("name") ||
          h?.toLowerCase().includes("customer")
      );
      const codeIndex = headers.findIndex(
        (h) =>
          h?.toLowerCase().includes("code") || h?.toLowerCase().includes("id")
      );

      if (codeIndex === -1) {
        throw new Error(
          'Could not find a "Customer Code" column. Please ensure your Excel file has a column with "code" or "id" in the header.'
        );
      }

      // Process data rows and match with available customers
      const matchedCustomers: string[] = [];
      let notFoundCount = 0;

      for (let i = 1; i < jsonData.length; i++) {
        const row = jsonData[i] as any[];

        // Skip empty rows
        if (
          !row ||
          row.every((cell) => !cell || cell.toString().trim() === "")
        ) {
          continue;
        }

        const name =
          nameIndex !== -1 ? row[nameIndex]?.toString().trim() : undefined;
        const code = row[codeIndex]?.toString().trim();

        if (!code) {
          continue; // Skip rows without codes
        }

        // Match customers using CODE and NAME (not UID)
        const matchedCustomer = dropdowns.customers.find((c) => {
          const customerCode = (c as any).code?.toLowerCase().trim() || "";
          const customerName = c.label.toLowerCase().trim();
          const searchCode = code.toLowerCase().trim();
          const searchName = name?.toLowerCase().trim();

          // Exact match by customer code
          if (customerCode && customerCode === searchCode) {
            return true;
          }

          // Exact match by customer name
          if (searchName && customerName === searchName) {
            return true;
          }

          // Partial match by customer code
          if (customerCode && customerCode.includes(searchCode)) {
            return true;
          }

          // Partial match by customer name
          if (searchName && customerName.includes(searchName)) {
            return true;
          }

          return false;
        });

        if (matchedCustomer) {
          matchedCustomers.push(matchedCustomer.value);
        } else {
          notFoundCount++;
        }
      }

      if (matchedCustomers.length === 0) {
        throw new Error("No matching customers found in the Excel file");
      }

      // Add imported customers to selected customers
      const currentSelected = form.getValues("selectedCustomers") || [];
      const updatedSelected = [
        ...new Set([...currentSelected, ...matchedCustomers]),
      ];

      setValue("selectedCustomers", updatedSelected);

      // Show success message
      let message = `Successfully imported ${matchedCustomers.length} customer${
        matchedCustomers.length !== 1 ? "s" : ""
      }.`;
      if (notFoundCount > 0) {
        message += ` ${notFoundCount} customer${
          notFoundCount !== 1 ? "s" : ""
        } could not be found.`;
      }

      toast({
        title: "Import Successful",
        description: message,
      });
    } catch (error) {
      console.error("Error processing Excel file:", error);
      toast({
        title: "Import Failed",
        description:
          error instanceof Error
            ? error.message
            : "Failed to process Excel file",
        variant: "destructive",
      });
    } finally {
      setIsProcessingExcel(false);
      // Reset the file input
      const fileInput = document.getElementById(
        "excel-import-input"
      ) as HTMLInputElement;
      if (fileInput) {
        fileInput.value = "";
      }
    }
  };

  // Handle template download
  const handleDownloadTemplate = () => {
    const template = [
      {
        "Customer Code": "ABC001",
        "Customer Name (Optional)": "ABC Store",
      },
      {
        "Customer Code": "XYZ002",
        "Customer Name (Optional)": "XYZ Mart",
      },
    ];

    const ws = XLSX.utils.json_to_sheet(template);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Customer Template");

    ws["!cols"] = [
      { width: 15 }, // Customer Code
      { width: 25 }, // Customer Name (Optional)
    ];

    XLSX.writeFile(wb, "customer_import_template.xlsx");
  };

  const handleReset = () => {
    reset();
    setCurrentStep(1);
    setShowOptionalFields(false);
    setShowAdditionalUsers(false);
    setDropdowns((prev) => ({
      ...prev,
      warehouses: [],
      roles: [],
      employees: [],
      vehicles: [],
      locations: [],
      customers: [],
    }));
  };

  // Callbacks for CustomerScheduler - defined outside renderStepContent to avoid hooks order issues
  const handleCustomersScheduled = useCallback(
    (scheduledCustomers: any) => {
      // Store the scheduled customers in form state for submission
      setValue("scheduledCustomers" as any, scheduledCustomers);
      console.log("📅 Customers scheduled:", scheduledCustomers.length);
    },
    [setValue]
  );

  const handleCustomerSchedulingChange = useCallback(
    (customerScheduling: any) => {
      // Store the customer scheduling data for the new backend structure
      setValue("customerScheduling", customerScheduling);
      console.log(
        "📊 Customer scheduling updated from CustomerScheduler:",
        customerScheduling.length,
        "entries"
      );
      console.log(
        "📊 Details:",
        customerScheduling.map((cs: any) => ({
          customerUID: cs.customerUID,
          frequency: cs.frequency,
          configCount: cs.scheduleConfigs.length,
        }))
      );
    },
    [setValue]
  );

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.2 }}
            className="space-y-4"
          >
            <div>
              <div className="mb-6">
                <h2 className="text-xl font-medium text-gray-900">
                  Route Information
                </h2>
              </div>

              {/* Basic Route Information */}
              <div className="bg-white border border-gray-200 rounded-lg p-5">
                {/* Status Badge in top right */}
                <div className="flex justify-between items-start mb-4">
                  <h3 className="text-sm font-semibold text-gray-900">
                    Basic Information
                  </h3>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-gray-500">Status:</span>
                    <Badge
                      variant={watch("isActive") ? "default" : "secondary"}
                      className={cn(
                        "text-xs",
                        watch("isActive")
                          ? "bg-green-100 text-green-700"
                          : "bg-gray-100 text-gray-600"
                      )}
                    >
                      {watch("isActive") ? "Active" : "Inactive"}
                    </Badge>
                    <Switch
                      checked={watch("isActive")}
                      onCheckedChange={(checked) =>
                        setValue("isActive", checked)
                      }
                      className="h-4 w-8"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Route Name */}
                  <div className="md:col-span-2">
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Route Name <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      {...register("name")}
                      placeholder="e.g., Downtown Morning Route"
                      className="h-10"
                    />
                    {errors.name && (
                      <p className="text-xs text-red-500 mt-1">
                        {errors.name.message}
                      </p>
                    )}
                  </div>

                  {/* Route Code */}
                  <div>
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Route Code <span className="text-red-500">*</span>
                    </Label>
                    <div className="flex gap-2">
                      <Input
                        {...register("code")}
                        placeholder="Auto-generated"
                        className="h-10 font-mono text-sm"
                        readOnly
                      />
                      <Button
                        type="button"
                        variant="outline"
                        size="icon"
                        onClick={handleGenerateCode}
                        disabled={!routeName?.trim()}
                        className="h-10 w-10"
                      >
                        <RefreshCw className="h-4 w-4" />
                      </Button>
                    </div>
                    {errors.code && (
                      <p className="text-xs text-red-500 mt-1">
                        {errors.code.message}
                      </p>
                    )}
                  </div>

                  {/* Valid From Date */}
                  <div>
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Valid From <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      type="date"
                      {...register("validFrom")}
                      className="h-10"
                      min={moment().format("YYYY-MM-DD")}
                    />
                    {errors.validFrom && (
                      <p className="text-xs text-red-500 mt-1">
                        {errors.validFrom.message}
                      </p>
                    )}
                  </div>

                  {/* Valid To Date */}
                  <div>
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Valid To <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      type="date"
                      {...register("validUpto")}
                      className="h-10"
                      min={watch("validFrom") || moment().format("YYYY-MM-DD")}
                    />
                    {errors.validUpto && (
                      <p className="text-xs text-red-500 mt-1">
                        {errors.validUpto.message}
                      </p>
                    )}
                  </div>

                  {/* Organization Hierarchy - Dynamic Cascading Fields */}
                  {loading.organizations ? (
                    <div className="md:col-span-2">
                      <Label className="text-sm font-medium text-gray-700 mb-1.5">
                        Organization <span className="text-red-500">*</span>
                      </Label>
                      <Skeleton className="h-10 w-full" />
                    </div>
                  ) : orgLevels.length > 0 ? (
                    <>
                      {/* Render all levels dynamically */}
                      {orgLevels.map((level, index) => (
                        <div key={`${level.orgTypeUID}_${index}`}>
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
                            <SelectTrigger className="h-10">
                              <SelectValue placeholder="Select Organization" />
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
                          {index === 0 && errors.orgUID && (
                            <p className="text-xs text-red-500 mt-1">
                              {errors.orgUID.message}
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
                            onClick={handleOrganizationReset}
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
                      <Building2 className="h-8 w-8 mx-auto mb-2 opacity-50" />
                      <p className="text-sm">No organizations available</p>
                    </div>
                  )}

                  {/* Bill To Customer and Description Fields - Same Row */}
                  {/* <div className="md:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <Label className="text-sm font-medium text-gray-700 mb-1.5">
                        Bill To Customer
                      </Label>
                      <Input
                        {...register("billToCustomer")}
                        placeholder="Customer code/ID for billing (optional)"
                        className="h-10"
                      />
                      <p className="text-xs text-gray-500 mt-1">
                        Customer who receives bills
                      </p>
                    </div>
                    
                    <div>
                      <Label className="text-sm font-medium text-gray-700 mb-1.5">
                        Description
                      </Label>
                      <textarea
                        {...register("description")}
                        placeholder="Brief route description (optional)"
                        rows={2}
                        className="w-full h-10 px-3 py-2 text-sm border border-gray-300 rounded-md focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none resize-none"
                      />
                    </div>
                  </div> */}
                </div>
              </div>

              {/* Assignment Section */}
              <div className="bg-white border border-gray-200 rounded-lg p-5 mt-4">
                <h3 className="text-sm font-semibold text-gray-900 mb-4">
                  Assignment
                </h3>

                {/* Assignment Fields - Role and Primary Employee in same row */}
                <div className="flex flex-col lg:flex-row gap-4">
                  {/* Role Field */}
                  <div className="flex-1 max-w-md">
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Role <span className="text-red-500">*</span>
                    </Label>
                    {loading.roles ? (
                      <Skeleton className="h-10 w-full" />
                    ) : (
                      <Popover
                        open={rolePopoverOpen}
                        onOpenChange={setRolePopoverOpen}
                      >
                        <PopoverTrigger asChild>
                          <Button
                            variant="outline"
                            role="combobox"
                            aria-expanded={rolePopoverOpen}
                            className="w-full h-10 justify-between text-left font-normal"
                          >
                            <span className="truncate">
                              {watch("roleUID")
                                ? dropdowns.roles.find(
                                    (role) => role.value === watch("roleUID")
                                  )?.label || "Select role"
                                : "Select role"}
                            </span>
                            <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                          </Button>
                        </PopoverTrigger>
                        <PopoverContent
                          className="w-[var(--radix-popover-trigger-width)] p-0"
                          align="start"
                        >
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
                                <p className="text-xs text-muted-foreground">
                                  Try a different search term
                                </p>
                              </div>
                            </CommandEmpty>
                            <CommandGroup>
                              <CommandList className="max-h-[280px] overflow-y-auto">
                                {dropdowns.roles.map((role) => (
                                  <CommandItem
                                    key={role.value}
                                    value={role.label}
                                    onSelect={() => {
                                      setValue("roleUID", role.value);
                                      setRolePopoverOpen(false);
                                    }}
                                    className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-accent rounded-none"
                                  >
                                    <div className="flex items-center gap-4 flex-1">
                                      <Check
                                        className={`h-4 w-4 shrink-0 ${
                                          watch("roleUID") === role.value
                                            ? "opacity-100 text-primary"
                                            : "opacity-0"
                                        }`}
                                      />
                                      <div className="flex flex-col gap-1.5 flex-1 min-w-0">
                                        <div className="font-medium text-sm truncate">
                                          {role.label}
                                        </div>
                                      </div>
                                    </div>
                                  </CommandItem>
                                ))}
                              </CommandList>
                            </CommandGroup>
                          </Command>
                        </PopoverContent>
                      </Popover>
                    )}
                    {errors.roleUID && (
                      <p className="text-xs text-red-500 mt-1">
                        {errors.roleUID.message}
                      </p>
                    )}
                  </div>

                  {/* Primary Employee Field with Add More Users Button */}
                  <div className="flex-1">
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Primary Employee{" "}
                      <span className="text-xs text-gray-500">
                        (automatically assigned to route)
                      </span>
                    </Label>
                    <div className="flex gap-2">
                      <div className="flex-1 max-w-md">
                        {loading.employees ? (
                          <Skeleton className="h-10 w-full" />
                        ) : (
                          <Popover
                            open={employeePopoverOpen}
                            onOpenChange={setEmployeePopoverOpen}
                          >
                            <PopoverTrigger asChild>
                              <Button
                                variant="outline"
                                role="combobox"
                                aria-expanded={employeePopoverOpen}
                                className="w-full h-10 justify-between text-left font-normal"
                              >
                                <span className="truncate">
                                  {watch("jobPositionUID")
                                    ? dropdowns.employees.find(
                                        (emp) =>
                                          emp.value === watch("jobPositionUID")
                                      )?.label || "Select primary employee"
                                    : "Select primary employee"}
                                </span>
                                <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                              </Button>
                            </PopoverTrigger>
                            <PopoverContent
                              className="w-[var(--radix-popover-trigger-width)] p-0"
                              align="start"
                            >
                              <Command>
                                <div className="flex items-center border-b px-4 py-2">
                                  <Search className="mr-3 h-4 w-4 shrink-0 opacity-50" />
                                  <CommandInput
                                    placeholder="Search employees..."
                                    className="flex h-9 w-full rounded-md bg-transparent py-2 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50 border-0 focus:ring-0"
                                  />
                                </div>
                                <CommandEmpty className="py-6 text-center text-sm">
                                  <div className="flex flex-col items-center gap-2">
                                    <Users className="h-8 w-8 text-muted-foreground/50" />
                                    <p>No employees found</p>
                                    <p className="text-xs text-muted-foreground">
                                      Try a different search term
                                    </p>
                                  </div>
                                </CommandEmpty>
                                <CommandGroup>
                                  <CommandList className="max-h-[280px] overflow-y-auto">
                                    {dropdowns.employees.map((emp) => (
                                      <CommandItem
                                        key={emp.value}
                                        value={emp.label}
                                        onSelect={() => {
                                          setValue("jobPositionUID", emp.value);
                                          setEmployeePopoverOpen(false);
                                        }}
                                        className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-accent rounded-none"
                                      >
                                        <div className="flex items-center gap-4 flex-1">
                                          <Check
                                            className={`h-4 w-4 shrink-0 ${
                                              watch("jobPositionUID") ===
                                              emp.value
                                                ? "opacity-100 text-primary"
                                                : "opacity-0"
                                            }`}
                                          />
                                          <div className="flex flex-col gap-1.5 flex-1 min-w-0">
                                            <div className="font-medium text-sm truncate">
                                              {emp.label}
                                            </div>
                                          </div>
                                        </div>
                                      </CommandItem>
                                    ))}
                                  </CommandList>
                                </CommandGroup>
                              </Command>
                            </PopoverContent>
                          </Popover>
                        )}
                      </div>
                      {/* Hidden: Add More Users button */}
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          setShowAdditionalUsers(!showAdditionalUsers)
                        }
                        className="h-10 px-3 bg-blue-50 hover:bg-blue-100 border-blue-200 hidden"
                        title="Add backup/additional employees to this route"
                      >
                        <Plus className="h-4 w-4 mr-2" />
                        <span className="text-sm">Add More Users</span>
                      </Button>
                    </div>
                  </div>
                </div>

                {/* Show selected additional employees as tags */}
                {selectedUsers.length > 0 && (
                  <div className="mt-3">
                    <p className="text-xs text-gray-600 mb-2">
                      Additional Employees:
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {selectedUsers.map((uid) => {
                        const user = dropdowns.employees.find(
                          (e) => e.value === uid
                        );
                        return (
                          <Badge
                            key={uid}
                            variant="secondary"
                            className="bg-blue-50 text-blue-700 border-blue-200 pl-2 pr-1 py-1"
                          >
                            <span className="text-xs">{user?.label}</span>
                            <button
                              type="button"
                              onClick={() => {
                                setValue(
                                  "selectedUsers",
                                  selectedUsers.filter((u) => u !== uid)
                                );
                              }}
                              className="ml-2 hover:bg-blue-200 rounded-full p-0.5 transition-colors"
                            >
                              <X className="h-3 w-3" />
                            </button>
                          </Badge>
                        );
                      })}
                    </div>
                  </div>
                )}

                {/* Additional Employees Selection - Show when + is clicked */}
                {showAdditionalUsers && (
                  <div className="mt-4 p-3 bg-gray-50 rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <p className="text-sm font-medium text-gray-700">
                        Select Additional Employees
                      </p>
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => setShowAdditionalUsers(false)}
                        className="h-8 px-2"
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    </div>
                    <div className="space-y-2 max-h-48 overflow-y-auto">
                      {dropdowns.employees
                        .filter((emp) => emp.value !== watch("jobPositionUID"))
                        .filter((emp) => !selectedUsers.includes(emp.value))
                        .map((emp) => (
                          <button
                            key={emp.value}
                            type="button"
                            onClick={() => {
                              setValue("selectedUsers", [
                                ...selectedUsers,
                                emp.value,
                              ]);
                            }}
                            className="w-full text-left px-3 py-2 text-sm hover:bg-white rounded border border-transparent hover:border-gray-200 transition-all"
                          >
                            {emp.label}
                          </button>
                        ))}
                      {dropdowns.employees.filter(
                        (emp) =>
                          emp.value !== watch("jobPositionUID") &&
                          !selectedUsers.includes(emp.value)
                      ).length === 0 && (
                        <p className="text-xs text-gray-500 text-center py-2">
                          All available employees have been added
                        </p>
                      )}
                    </div>
                  </div>
                )}

                {/* Optional Details Dropdown */}
                <div className="mt-4">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setShowOptionalFields(!showOptionalFields)}
                    className="w-full justify-between h-10"
                  >
                    <span className="flex items-center gap-2">
                      <Settings className="h-4 w-4" />
                      Optional Details
                    </span>
                    <ChevronDown
                      className={cn(
                        "h-4 w-4 transition-transform",
                        showOptionalFields && "rotate-180"
                      )}
                    />
                  </Button>

                  {showOptionalFields && (
                    <div className="mt-3 p-4 border border-gray-200 rounded-lg bg-gray-50">
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {/* Warehouse */}
                        <div>
                          <Label className="text-sm font-medium text-gray-700 mb-1.5">
                            Warehouse
                          </Label>
                          {loading.warehouses ? (
                            <Skeleton className="h-10 w-full" />
                          ) : (
                            <Select
                              value={watch("whOrgUID")}
                              onValueChange={(value) =>
                                setValue("whOrgUID", value)
                              }
                            >
                              <SelectTrigger className="h-10">
                                <SelectValue placeholder="Select warehouse" />
                              </SelectTrigger>
                              <SelectContent>
                                {dropdowns.warehouses.map((wh) => (
                                  <SelectItem key={wh.value} value={wh.value}>
                                    {wh.label}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          )}
                        </div>

                        {/* Vehicle */}
                        <div>
                          <Label className="text-sm font-medium text-gray-700 mb-1.5">
                            Vehicle
                          </Label>
                          {loading.vehicles ? (
                            <Skeleton className="h-10 w-full" />
                          ) : (
                            <Select
                              value={watch("vehicleUID")}
                              onValueChange={(value) =>
                                setValue("vehicleUID", value)
                              }
                            >
                              <SelectTrigger className="h-10">
                                <SelectValue placeholder="Select vehicle" />
                              </SelectTrigger>
                              <SelectContent>
                                {dropdowns.vehicles.map((vehicle) => (
                                  <SelectItem
                                    key={vehicle.value}
                                    value={vehicle.value}
                                  >
                                    {vehicle.label}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          )}
                        </div>

                        {/* Location */}
                        {/* <div className="md:col-span-2">
                          <Label className="text-sm font-medium text-gray-700 mb-1.5">
                            Location
                          </Label>
                          {loading.locations ? (
                            <Skeleton className="h-10 w-full" />
                          ) : (
                            <Select
                              value={watch("locationUID")}
                              onValueChange={(value) =>
                                setValue("locationUID", value)
                              }
                            >
                              <SelectTrigger className="h-10">
                                <SelectValue placeholder="Select location" />
                              </SelectTrigger>
                              <SelectContent>
                                {dropdowns.locations.map((location) => (
                                  <SelectItem
                                    key={location.value}
                                    value={location.value}
                                  >
                                    {location.label}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          )}
                        </div> */}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
            {/* </div> */}
          </motion.div>
        );

      case 2:
        return (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.2 }}
            className="space-y-6"
          >
            <div>
              <div className="mb-8">
                <h2 className="text-2xl font-semibold text-gray-900 mb-2">
                  Customer Selection
                </h2>
                <p className="text-sm text-gray-600">
                  Choose customers to be included in this route and select the
                  visit frequency
                </p>
              </div>

              {/* Period Selection */}
              <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-xl p-6 mb-6">
                <div className="flex items-center gap-3 mb-4">
                  <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
                    <Calendar className="h-4 w-4 text-white" />
                  </div>
                  <h3 className="text-lg font-semibold text-gray-900">
                    Visit Frequency
                  </h3>
                </div>
                <p className="text-sm text-gray-600 mb-4">
                  Select how often customers will be visited on this route
                </p>
                {/* Visit Frequency Selection - Row Format */}
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <h3 className="text-sm font-medium text-gray-900">
                      Visit Frequency
                    </h3>
                    <span className="text-xs text-gray-500">
                      {
                        Object.keys(scheduleCustomerAssignments).filter(
                          (type) =>
                            (scheduleCustomerAssignments[type] || []).length > 0
                        ).length
                      }{" "}
                      frequency types selected
                    </span>
                  </div>

                  <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
                    {[
                      { value: "daily", label: "Daily", desc: "", icon: "☀️" },
                      {
                        value: "weekly",
                        label: "Weekly",
                        desc: "",
                        icon: "📅",
                      },
                      {
                        value: "monthly",
                        label: "Monthly",
                        desc: "",
                        icon: "📆",
                      },
                      {
                        value: "fortnight",
                        label: "Fortnightly",
                        desc: "",
                        icon: "🗓️",
                      },
                    ].map((period) => {
                      const assignedCustomerUIDs =
                        scheduleCustomerAssignments[period.value] || [];
                      const isSelected = currentlySelectedType === period.value;
                      const hasCustomers = assignedCustomerUIDs.length > 0;

                      return (
                        <button
                          key={period.value}
                          type="button"
                          onClick={() => {
                            setCurrentlySelectedType(period.value);
                            // Also add to activeScheduleTypes if not already there
                            if (!activeScheduleTypes.includes(period.value)) {
                              setActiveScheduleTypes([...activeScheduleTypes, period.value]);
                            }
                            // Initialize empty array for this schedule type if not exists
                            if (!scheduleCustomerAssignments[period.value]) {
                              setScheduleCustomerAssignments(prev => ({
                                ...prev,
                                [period.value]: []
                              }));
                            }
                          }}
                          className={cn(
                            "px-4 py-3 rounded-lg border text-left transition-all duration-200 hover:shadow-md relative",
                            isSelected
                              ? "bg-blue-600 text-white border-blue-600 shadow-lg"
                              : hasCustomers
                              ? "bg-blue-50 text-blue-700 border-blue-200 hover:bg-blue-100"
                              : "bg-white text-gray-700 border-gray-200 hover:border-blue-200 hover:bg-blue-50"
                          )}
                        >
                          <div className="flex items-center gap-2 mb-1">
                            <span className="text-base">{period.icon}</span>
                            <div className="font-semibold text-sm">
                              {period.label}
                            </div>
                          </div>
                          <div
                            className={cn(
                              "text-xs mb-2",
                              isSelected ? "text-blue-100" : "text-gray-500"
                            )}
                          >
                            {period.desc}
                          </div>

                          {/* Customer count badge */}
                          {hasCustomers && (
                            <div
                              className={cn(
                                "absolute top-2 right-2 px-2 py-1 rounded-full text-xs font-medium",
                                isSelected
                                  ? "bg-white text-blue-600"
                                  : "bg-blue-500 text-white"
                              )}
                            >
                              {assignedCustomerUIDs.length}
                            </div>
                          )}

                          {/* Active indicator */}
                          {isSelected && (
                            <div className="absolute bottom-2 right-2">
                              <div className="w-2 h-2 bg-white rounded-full"></div>
                            </div>
                          )}
                        </button>
                      );
                    })}
                  </div>
                </div>
              </div>

              {/* Customer Selection Card */}
              <div className="bg-white border border-gray-200 rounded-xl shadow-sm">
                <div className="p-6">
                  <div className="flex items-center justify-between mb-6">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center shadow-lg">
                        <Building2 className="h-6 w-6 text-white" />
                      </div>
                      <div>
                        <h3 className="text-xl font-bold text-gray-900">
                          Select Customers
                        </h3>
                        <p className="text-sm text-gray-500 mt-0.5">
                          Choose stores and configure individual visit schedules
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center space-x-4">
                      <div className="flex items-center gap-3">
                        <Badge
                          variant="outline"
                          className={cn(
                            "px-4 py-2 font-semibold text-sm",
                            selectedCustomers.length > 0
                              ? "bg-blue-50 text-blue-700 border-blue-200"
                              : "bg-gray-50 text-gray-600 border-gray-200"
                          )}
                        >
                          {selectedCustomers.length} selected
                        </Badge>
                        <div className="flex items-center gap-2">
                          <div className="relative">
                            <input
                              type="file"
                              accept=".xlsx,.xls"
                              onChange={(e) => {
                                const file = e.target.files?.[0];
                                if (file) {
                                  processExcelFile(file);
                                }
                              }}
                              disabled={isProcessingExcel}
                              className="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
                              id="excel-import-input"
                            />
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              disabled={isProcessingExcel}
                              className="flex items-center gap-2 hover:bg-green-50 hover:border-green-200 hover:text-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              {isProcessingExcel ? (
                                <>
                                  <RefreshCw className="w-4 h-4 animate-spin" />
                                  Processing...
                                </>
                              ) : (
                                <>
                                  <FileText className="w-4 h-4" />
                                  Import Excel
                                </>
                              )}
                            </Button>
                          </div>
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={handleDownloadTemplate}
                            className="flex items-center gap-2 hover:bg-blue-50 hover:border-blue-200 hover:text-blue-700"
                          >
                            <Download className="w-4 h-4" />
                            Download Template
                          </Button>
                        </div>
                        <div className="text-sm text-gray-600">
                          <span className="font-medium">Loaded: </span>
                          <span className="font-semibold text-gray-900">
                            {dropdowns.customers.length}
                          </span>
                          {customersPagination.totalCount > 0 && (
                            <span className="text-gray-600">
                              {" / "}
                              {customersPagination.totalCount}
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Search and Filter Bar */}
                  <div className="mb-6 space-y-4">
                    <div className="relative">
                      <Input
                        placeholder="Search customers by name or ID..."
                        value={customerSearch}
                        onChange={(e) => setCustomerSearch(e.target.value)}
                        className="pl-10 h-11 bg-gray-50 border-gray-200 focus:bg-white"
                      />
                      <div className="absolute left-3 top-1/2 transform -translate-y-1/2">
                        <Building2 className="h-4 w-4 text-gray-400" />
                      </div>
                      {customerSearch && (
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => setCustomerSearch("")}
                          className="absolute right-2 top-1/2 transform -translate-y-1/2 h-7 w-7 p-0 text-gray-400 hover:text-gray-600"
                        >
                          <X className="h-3 w-3" />
                        </Button>
                      )}
                    </div>
                  </div>

                  {loading.customers ? (
                    <div className="space-y-3">
                      {[...Array(5)].map((_, i) => (
                        <Skeleton key={i} className="h-12 w-full rounded-lg" />
                      ))}
                    </div>
                  ) : dropdowns.customers.length > 0 ? (
                    <div className="border border-gray-200 rounded-lg overflow-hidden">
                      {/* Select All Header */}
                      <div className="p-4 bg-gradient-to-r from-gray-50 to-white border-b border-gray-200">
                        <label className="flex items-center justify-between cursor-pointer group">
                          <div className="flex items-center space-x-3">
                            <Checkbox
                              checked={
                                filteredCustomers.length > 0 &&
                                filteredCustomers.every((customer) =>
                                  (
                                    scheduleCustomerAssignments[
                                      currentlySelectedType
                                    ] || []
                                  ).includes(customer.value)
                                )
                              }
                              onCheckedChange={(checked) => {
                                if (checked) {
                                  const allCustomerUIDs = filteredCustomers.map(
                                    (c) => c.value
                                  );

                                  // For daily type, directly set all customers
                                  // For other types, add to existing assignments
                                  let updatedScheduleAssignments;
                                  if (currentlySelectedType === "daily") {
                                    updatedScheduleAssignments = {
                                      ...scheduleCustomerAssignments,
                                      [currentlySelectedType]: allCustomerUIDs,
                                    };
                                  } else {
                                    const currentAssignments =
                                      scheduleCustomerAssignments[
                                        currentlySelectedType
                                      ] || [];
                                    updatedScheduleAssignments = {
                                      ...scheduleCustomerAssignments,
                                      [currentlySelectedType]: [
                                        ...new Set([
                                          ...currentAssignments,
                                          ...allCustomerUIDs
                                        ])
                                      ],
                                    };
                                  }
                                  setScheduleCustomerAssignments(updatedScheduleAssignments);

                                  // Update global selected customers to include ALL customers from ALL schedule types
                                  const allScheduledCustomers = new Set(
                                    Object.values(updatedScheduleAssignments).flat()
                                  );
                                  setValue(
                                    "selectedCustomers",
                                    Array.from(allScheduledCustomers)
                                  );
                                } else {
                                  // Clear schedule customer assignments for current frequency type
                                  const updatedScheduleAssignments = {
                                    ...scheduleCustomerAssignments,
                                    [currentlySelectedType]: [],
                                  };
                                  setScheduleCustomerAssignments(updatedScheduleAssignments);

                                  // Update global selectedCustomers to include ALL remaining customers from ALL schedule types
                                  const allScheduledCustomers = new Set(
                                    Object.values(updatedScheduleAssignments).flat()
                                  );
                                  setValue(
                                    "selectedCustomers",
                                    Array.from(allScheduledCustomers)
                                  );
                                }
                              }}
                              className="data-[state=checked]:bg-blue-600 data-[state=checked]:border-blue-600"
                            />
                            <span className="text-sm font-semibold text-gray-700 group-hover:text-gray-900">
                              Select All Customers
                            </span>
                          </div>
                          <span className="text-xs text-gray-500">
                            {filteredCustomers.length} available
                          </span>
                        </label>
                      </div>

                      {/* Enhanced Customer List with Infinite Scroll */}
                      <div
                        ref={customersScrollRef}
                        className="space-y-3 max-h-[700px] overflow-y-auto pr-2"
                        onScroll={handleCustomerScroll}
                      >
                        {filteredCustomers.length === 0 && customerSearch ? (
                          <div className="text-center py-12 bg-gray-50 rounded-xl">
                            <Building2 className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                            <h3 className="text-lg font-semibold text-gray-600 mb-2">
                              No customers found
                            </h3>
                            <p className="text-sm text-gray-500 mb-4">
                              No customers match your search for "
                              {customerSearch}"
                            </p>
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              onClick={() => setCustomerSearch("")}
                              className="mx-auto"
                            >
                              Clear search
                            </Button>
                          </div>
                        ) : (
                          filteredCustomers.map((customer) => {
                            // Check if customer is selected for the current schedule type
                            const isSelected = (
                              scheduleCustomerAssignments[
                                currentlySelectedType
                              ] || []
                            ).includes(customer.value);
                            const currentSchedule: StoreSchedule =
                              (storeSchedules &&
                                storeSchedules[customer.value]) || {
                                visitTime: "",
                                visitDuration: 0,
                                endTime: "",
                                travelTime: 0,
                                sequence: 1,
                                weekOff: {
                                  sunday: false,
                                  monday: false,
                                  tuesday: false,
                                  wednesday: false,
                                  thursday: false,
                                  friday: false,
                                  saturday: false,
                                },
                                specialDays: [],
                              };

                            const showAdvanced =
                              expandedSchedules[customer.value] || false;

                            return (
                              <motion.div
                                key={customer.value}
                                layout
                                className={cn(
                                  "group relative rounded-xl border-2 transition-all duration-200 overflow-hidden",
                                  isSelected
                                    ? "border-blue-200 bg-gradient-to-r from-blue-50 to-indigo-50 shadow-md"
                                    : "border-gray-200 bg-white hover:border-gray-300 hover:shadow-sm"
                                )}
                              >
                                {/* Store Header */}
                                <div className="p-4">
                                  <div className="flex items-center justify-between">
                                    <label className="flex items-center space-x-4 cursor-pointer flex-1">
                                      <div className="relative">
                                        <Checkbox
                                          checked={isSelected}
                                          onCheckedChange={(checked) => {
                                            if (checked) {
                                              console.log(`✅ Adding customer ${customer.label} to ${currentlySelectedType}`);
                                              // Add to current schedule type assignments FIRST
                                              const currentAssignments =
                                                scheduleCustomerAssignments[
                                                  currentlySelectedType
                                                ] || [];
                                              const updatedScheduleAssignments =
                                                {
                                                  ...scheduleCustomerAssignments,
                                                  [currentlySelectedType]: [
                                                    ...new Set([...currentAssignments, customer.value])
                                                  ],
                                                };
                                              setScheduleCustomerAssignments(
                                                updatedScheduleAssignments
                                              );

                                              // Update global selectedCustomers to include ALL customers from ALL schedule types
                                              const allScheduledCustomers =
                                                new Set(
                                                  Object.values(
                                                    updatedScheduleAssignments
                                                  ).flat()
                                                );
                                              setValue(
                                                "selectedCustomers",
                                                Array.from(
                                                  allScheduledCustomers
                                                )
                                              );

                                              // Add to assigned customers set
                                              setAssignedCustomers(
                                                new Set([
                                                  ...assignedCustomers,
                                                  customer.value,
                                                ])
                                              );

                                              setValue(
                                                `storeSchedules.${customer.value}`,
                                                {
                                                  visitTime: "",
                                                  visitDuration: 0,
                                                  endTime: "",
                                                  travelTime: 0,
                                                  sequence:
                                                    selectedCustomers.length +
                                                    1,
                                                  weekOff: {
                                                    sunday: false,
                                                    monday: false,
                                                    tuesday: false,
                                                    wednesday: false,
                                                    thursday: false,
                                                    friday: false,
                                                    saturday: false,
                                                  },
                                                  specialDays: [],
                                                }
                                              );
                                            } else {
                                              console.log(`❌ Removing customer ${customer.label} from ${currentlySelectedType}`);
                                              // Remove from current schedule type assignments FIRST
                                              const currentAssignments =
                                                scheduleCustomerAssignments[
                                                  currentlySelectedType
                                                ] || [];
                                              const updatedScheduleAssignments =
                                                {
                                                  ...scheduleCustomerAssignments,
                                                  [currentlySelectedType]:
                                                    currentAssignments.filter(
                                                      (c) =>
                                                        c !== customer.value
                                                    ),
                                                };
                                              setScheduleCustomerAssignments(
                                                updatedScheduleAssignments
                                              );

                                              // Update global selectedCustomers to include ALL customers from ALL schedule types
                                              const allScheduledCustomers =
                                                new Set(
                                                  Object.values(
                                                    updatedScheduleAssignments
                                                  ).flat()
                                                );
                                              setValue(
                                                "selectedCustomers",
                                                Array.from(
                                                  allScheduledCustomers
                                                )
                                              );

                                              // Remove from assigned customers set
                                              const newAssignedCustomers =
                                                new Set(assignedCustomers);
                                              newAssignedCustomers.delete(
                                                customer.value
                                              );
                                              setAssignedCustomers(
                                                newAssignedCustomers
                                              );

                                              const newSchedules = {
                                                ...(storeSchedules || {}),
                                              };
                                              delete newSchedules[
                                                customer.value
                                              ];
                                              setValue(
                                                "storeSchedules",
                                                newSchedules
                                              );
                                            }
                                          }}
                                          className="h-5 w-5 data-[state=checked]:bg-blue-600 data-[state=checked]:border-blue-600"
                                        />
                                        {isSelected && (
                                          <div className="absolute -top-1 -right-1 h-3 w-3 bg-blue-600 rounded-full flex items-center justify-center">
                                            <CheckCircle2 className="h-2 w-2 text-white" />
                                          </div>
                                        )}
                                      </div>

                                      <div className="flex-1 min-w-0">
                                        <div className="flex items-center space-x-3">
                                          <div
                                            className={cn(
                                              "w-10 h-10 rounded-lg flex items-center justify-center text-sm font-bold",
                                              isSelected
                                                ? "bg-blue-600 text-white"
                                                : "bg-gray-100 text-gray-600 group-hover:bg-gray-200"
                                            )}
                                          >
                                            {customer.label
                                              .charAt(0)
                                              .toUpperCase()}
                                          </div>
                                          <div className="flex-1 min-w-0">
                                            <h3
                                              className={cn(
                                                "font-semibold text-sm truncate",
                                                isSelected
                                                  ? "text-blue-900"
                                                  : "text-gray-900"
                                              )}
                                            >
                                              {customer.label}
                                            </h3>
                                            <p className="text-xs text-gray-500 mt-0.5">
                                              Code:{" "}
                                              {(customer as any).code ||
                                                customer.value}
                                            </p>
                                          </div>
                                        </div>
                                      </div>
                                    </label>

                                    {isSelected && (
                                      <div className="flex items-center space-x-2 ml-4">
                                        <Badge
                                          variant="secondary"
                                          className="bg-blue-100 text-blue-800 border-blue-200"
                                        >
                                          #
                                          {currentSchedule.sequence ||
                                            selectedCustomers.indexOf(
                                              customer.value
                                            ) + 1}
                                        </Badge>
                                        {/* <Button
                                          type="button"
                                          variant="ghost"
                                          size="sm"
                                          onClick={() =>
                                            setExpandedSchedules((prev) => ({
                                              ...prev,
                                              [customer.value]: !showAdvanced
                                            }))
                                          }
                                          className="h-8 w-8 p-0 text-blue-600 hover:bg-blue-100"
                                        >
                                          <Settings
                                            className={cn(
                                              "h-4 w-4 transition-transform",
                                              showAdvanced && "rotate-45"
                                            )}
                                          />
                                        </Button> */}
                                      </div>
                                    )}
                                  </div>

                                  {/* Quick Schedule Summary */}
                                  {isSelected &&
                                    currentSchedule.visitDuration !== 0 && (
                                      <div className="mt-3 flex items-center space-x-4 text-xs">
                                        {/* {currentSchedule.visitTime && (
                                          <div className="flex items-center space-x-1 text-blue-700">
                                            <Clock className="h-3 w-3" />
                                            <span>
                                              {currentSchedule.visitTime}
                                            </span>
                                          </div>
                                        )} */}
                                        {currentSchedule.visitDuration !==
                                          30 && (
                                          <div className="flex items-center space-x-1 text-blue-700">
                                            <span className="w-3 h-3 rounded-full bg-blue-600"></span>
                                            <span>
                                              {currentSchedule.visitDuration}min
                                            </span>
                                          </div>
                                        )}
                                      </div>
                                    )}
                                </div>

                                {/* Advanced Scheduling Options */}
                                <AnimatePresence>
                                  {isSelected && showAdvanced && (
                                    <motion.div
                                      initial={{ opacity: 0, height: 0 }}
                                      animate={{ opacity: 1, height: "auto" }}
                                      exit={{ opacity: 0, height: 0 }}
                                      transition={{
                                        duration: 0.3,
                                        ease: "easeInOut",
                                      }}
                                      className="border-t border-blue-200 bg-gradient-to-r from-blue-25 to-indigo-25"
                                    >
                                      <div className="p-4 space-y-6">
                                        {/* Timing Configuration */}
                                        {/* Visit Timing - Now Global */}
                                        {/* <div>
                                          <h4 className="text-sm font-semibold text-blue-900 mb-3 flex items-center">
                                            <Clock className="h-4 w-4 mr-2" />
                                            Visit Timing
                                          </h4>
                                          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                            <div className="space-y-2">
                                              <Label className="text-xs font-medium text-gray-700">
                                                Visit Time
                                              </Label>
                                              <Input
                                                type="time"
                                                value={
                                                  currentSchedule.visitTime ||
                                                  ""
                                                }
                                                onChange={(e) => {
                                                  setValue(
                                                    `storeSchedules.${customer.value}.visitTime` as any,
                                                    e.target.value
                                                  );
                                                  if (
                                                    currentSchedule.visitDuration &&
                                                    e.target.value
                                                  ) {
                                                    const startTime = new Date(
                                                      `2000-01-01T${e.target.value}:00`
                                                    );
                                                    const endTime = new Date(
                                                      startTime.getTime() +
                                                        currentSchedule.visitDuration *
                                                          60000
                                                    );
                                                    setValue(
                                                      `storeSchedules.${customer.value}.endTime` as any,
                                                      endTime
                                                        .toTimeString()
                                                        .slice(0, 5)
                                                    );
                                                  }
                                                }}
                                                className="h-9 text-sm border-blue-200 focus:border-blue-400 focus:ring-blue-400"
                                              />
                                            </div>
                                            <div className="space-y-2">
                                              <Label className="text-xs font-medium text-gray-700">
                                                Duration (minutes)
                                              </Label>
                                              <Input
                                                type="number"
                                                min="5"
                                                max="480"
                                                value={
                                                  currentSchedule.visitDuration ||
                                                  30
                                                }
                                                onChange={(e) => {
                                                  const duration =
                                                    parseInt(e.target.value) ||
                                                    30;
                                                  setValue(
                                                    `storeSchedules.${customer.value}.visitDuration` as any,
                                                    duration
                                                  );
                                                  if (
                                                    currentSchedule.visitTime
                                                  ) {
                                                    const startTime = new Date(
                                                      `2000-01-01T${currentSchedule.visitTime}:00`
                                                    );
                                                    const endTime = new Date(
                                                      startTime.getTime() +
                                                        duration * 60000
                                                    );
                                                    setValue(
                                                      `storeSchedules.${customer.value}.endTime` as any,
                                                      endTime
                                                        .toTimeString()
                                                        .slice(0, 5)
                                                    );
                                                  }
                                                }}
                                                className="h-9 text-sm border-blue-200 focus:border-blue-400 focus:ring-blue-400"
                                              />
                                            </div>
                                            <div className="space-y-2">
                                              <Label className="text-xs font-medium text-gray-700">
                                                Travel Time (minutes)
                                              </Label>
                                              <Input
                                                type="number"
                                                min="0"
                                                max="480"
                                                value={
                                                  currentSchedule.travelTime ||
                                                  15
                                                }
                                                onChange={(e) =>
                                                  setValue(
                                                    `storeSchedules.${customer.value}.travelTime` as any,
                                                    parseInt(e.target.value) ||
                                                      15
                                                  )
                                                }
                                                className="h-9 text-sm border-blue-200 focus:border-blue-400 focus:ring-blue-400"
                                              />
                                            </div>
                                            <div className="space-y-2">
                                              <Label className="text-xs font-medium text-gray-700">
                                                Visit Order
                                              </Label>
                                              <Input
                                                type="number"
                                                min="1"
                                                value={
                                                  currentSchedule.sequence ||
                                                  selectedCustomers.indexOf(
                                                    customer.value
                                                  ) + 1
                                                }
                                                onChange={(e) =>
                                                  setValue(
                                                    `storeSchedules.${customer.value}.sequence` as any,
                                                    parseInt(e.target.value) ||
                                                      1
                                                  )
                                                }
                                                className="h-9 text-sm border-blue-200 focus:border-blue-400 focus:ring-blue-400"
                                              />
                                            </div>
                                          </div>
                                        </div> */}

                                        {/* Store Closed Days */}
                                        {/* <div>
                                          <h4 className="text-sm font-semibold text-blue-900 mb-3 flex items-center">
                                            <Calendar className="h-4 w-4 mr-2" />
                                            Store Closed Days
                                            <span className="ml-2 text-xs text-gray-500 font-normal">
                                              (Override route schedule)
                                            </span>
                                          </h4>
                                          <div className="grid grid-cols-7 gap-2">
                                            {Object.entries({
                                              sunday: "Sun",
                                              monday: "Mon",
                                              tuesday: "Tue",
                                              wednesday: "Wed",
                                              thursday: "Thu",
                                              friday: "Fri",
                                              saturday: "Sat"
                                            }).map(([day, label]) => (
                                              <label
                                                key={day}
                                                className={cn(
                                                  "relative flex items-center justify-center h-12 rounded-lg border-2 cursor-pointer transition-all duration-200",
                                                  "hover:scale-105 active:scale-95",
                                                  currentSchedule.weekOff?.[
                                                    day as keyof typeof currentSchedule.weekOff
                                                  ]
                                                    ? "border-red-400 bg-red-100 text-red-700 shadow-sm"
                                                    : "border-gray-200 bg-white text-gray-600 hover:border-gray-300"
                                                )}
                                              >
                                                <input
                                                  type="checkbox"
                                                  checked={
                                                    currentSchedule.weekOff?.[
                                                      day as keyof typeof currentSchedule.weekOff
                                                    ] || false
                                                  }
                                                  onChange={(e) =>
                                                    setValue(
                                                      `storeSchedules.${customer.value}.weekOff.${day}` as any,
                                                      e.target.checked
                                                    )
                                                  }
                                                  className="sr-only"
                                                />
                                                <span className="text-xs font-semibold">
                                                  {label}
                                                </span>
                                                {currentSchedule.weekOff?.[
                                                  day as keyof typeof currentSchedule.weekOff
                                                ] && (
                                                  <X className="absolute -top-1 -right-1 h-3 w-3 text-red-600 bg-red-200 rounded-full p-0.5" />
                                                )}
                                              </label>
                                            ))}
                                          </div>
                                          <p className="text-xs text-gray-500 mt-2">
                                            Select days when this store is
                                            closed (route will skip on these
                                            days)
                                          </p>
                                        </div> */}
                                      </div>
                                    </motion.div>
                                  )}
                                </AnimatePresence>
                              </motion.div>
                            );
                          })
                        )}

                        {/* Loading indicator for infinite scroll */}
                        {customersPagination.isLoadingMore && (
                          <div className="flex justify-center items-center py-4 bg-gray-50 rounded-lg mt-3">
                            <RefreshCw className="h-4 w-4 text-blue-600 animate-spin mr-2" />
                            <span className="text-sm text-gray-600">
                              Loading more customers...
                            </span>
                          </div>
                        )}
                      </div>

                      {/* Summary Footer */}
                      {selectedCustomers.length > 0 && (
                        <div className="p-4 bg-gradient-to-r from-blue-50 to-indigo-50 border-t border-blue-200">
                          <div className="flex items-center justify-between">
                            <p className="text-sm font-medium text-blue-900">
                              {selectedCustomers.length} customer
                              {selectedCustomers.length !== 1 ? "s" : ""} will
                              be added to this route
                            </p>
                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              onClick={() => setValue("selectedCustomers", [])}
                              className="text-blue-600 hover:text-blue-700 hover:bg-blue-100"
                            >
                              Clear Selection
                            </Button>
                          </div>
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="text-center py-12 bg-gray-50 rounded-lg">
                      <Building2 className="h-12 w-12 text-gray-400 mx-auto mb-3" />
                      <p className="text-sm text-gray-600 font-medium">
                        No customers available
                      </p>
                      <p className="text-xs text-gray-500 mt-1">
                        Please select an organization first to load customers
                      </p>
                    </div>
                  )}
                </div>
              </div>

              {/* Quick Stats */}
              {selectedCustomers.length > 0 && (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6">
                  <div className="bg-gradient-to-r from-blue-50 to-white border border-blue-200 rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-xs text-blue-600 font-medium">
                          Total Selected
                        </p>
                        <p className="text-2xl font-bold text-blue-900">
                          {selectedCustomers.length}
                        </p>
                      </div>
                      <Building2 className="h-8 w-8 text-blue-300" />
                    </div>
                  </div>
                  <div className="bg-gradient-to-r from-green-50 to-white border border-green-200 rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-xs text-green-600 font-medium">
                          Coverage
                        </p>
                        <p className="text-2xl font-bold text-green-900">
                          {Math.round(
                            (selectedCustomers.length /
                              dropdowns.customers.length) *
                              100
                          )}
                          %
                        </p>
                      </div>
                      <CheckCircle2 className="h-8 w-8 text-green-300" />
                    </div>
                  </div>
                  <div className="bg-gradient-to-r from-purple-50 to-white border border-purple-200 rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-xs text-purple-600 font-medium">
                          Remaining
                        </p>
                        <p className="text-2xl font-bold text-purple-900">
                          {dropdowns.customers.length -
                            selectedCustomers.length}
                        </p>
                      </div>
                      <Users className="h-8 w-8 text-purple-300" />
                    </div>
                  </div>
                </div>
              )}
            </div>
          </motion.div>
        );

      case 3:
        return (
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.3 }}
          >
            <CustomerScheduler
              initialActiveScheduleTypes={activeScheduleTypes}
              initialScheduleCustomerAssignments={scheduleCustomerAssignments}
              availableCustomers={(() => {
                // Get all unique customer UIDs from both selectedCustomers and scheduleCustomerAssignments
                const allCustomerUIDs = new Set([
                  ...(selectedCustomers || []),
                  ...Object.values(scheduleCustomerAssignments).flat(),
                ]);

                // Map all unique customers to the required format
                return Array.from(allCustomerUIDs)
                  .map((customerUID) => {
                    const customer = (dropdowns.customers || []).find(
                      (c) => c.value === customerUID
                    );
                    return customer
                      ? {
                          UID: customer.value,
                          Code: (customer as any).code || customer.value,
                          Name: customer.label,
                          Address: (customer as any).address || "",
                          ContactNo: (customer as any).contactNo || "",
                          Type: (customer as any).type || "",
                          Status: (customer as any).status || "Active",
                        }
                      : null;
                  })
                  .filter((customer) => customer !== null);
              })()}
              onCustomersScheduled={handleCustomersScheduled}
              onCustomerSchedulingChange={handleCustomerSchedulingChange}
            />
          </motion.div>
        );

      case 4:
        return (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.2 }}
            className="space-y-6"
          >
            <div>
              <div className="mb-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-2">
                  Review & Create
                </h2>
                <p className="text-sm text-gray-500">
                  Verify all route details before creation
                </p>
              </div>

              <div className="space-y-6">
                {/* Basic Information */}
                <div className="bg-gray-50 rounded-lg p-6">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-medium text-gray-900 flex items-center gap-2">
                      <FileText className="h-5 w-5 text-gray-600" />
                      Basic Information
                    </h3>
                    <Badge
                      variant={watch("isActive") ? "default" : "secondary"}
                      className={cn(
                        "text-xs",
                        watch("isActive")
                          ? "bg-green-100 text-green-700"
                          : "bg-gray-100 text-gray-600"
                      )}
                    >
                      {watch("isActive") ? "Active" : "Inactive"}
                    </Badge>
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
                    <div>
                      <dt className="text-sm font-medium text-gray-500">
                        Route Name
                      </dt>
                      <dd className="mt-1 text-base text-gray-900">
                        {watch("name")}
                      </dd>
                    </div>
                    <div>
                      <dt className="text-sm font-medium text-gray-500">
                        Route Code
                      </dt>
                      <dd className="mt-1 text-base text-gray-900 font-mono">
                        {watch("code")}
                      </dd>
                    </div>
                    <div>
                      <dt className="text-sm font-medium text-gray-500">
                        Organization
                      </dt>
                      <dd className="mt-1 text-base text-gray-900">
                        {(() => {
                          const selectedOrgUID = watch("orgUID");
                          const selectedOrg = organizations.find(
                            (o) => o.UID === selectedOrgUID
                          );
                          return (
                            selectedOrg?.Name ||
                            dropdowns.organizations.find(
                              (o) => o.value === selectedOrgUID
                            )?.label ||
                            "Loading..."
                          );
                        })()}
                      </dd>
                    </div>
                  </div>
                </div>

                {/* Assignment */}
                <div className="bg-gray-50 rounded-lg p-6">
                  <h3 className="text-lg font-medium text-gray-900 mb-4">
                    Team Assignment
                  </h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
                    <div>
                      <dt className="text-sm font-medium text-gray-500">
                        Assigned Role
                      </dt>
                      <dd className="mt-1 text-base text-gray-900">
                        {dropdowns.roles.find(
                          (r) => r.value === watch("roleUID")
                        )?.label || "Loading..."}
                      </dd>
                    </div>
                    {watch("jobPositionUID") && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">
                          Employee
                        </dt>
                        <dd className="mt-1 text-base text-gray-900">
                          {dropdowns.employees.find(
                            (e) => e.value === watch("jobPositionUID")
                          )?.label || "Loading..."}
                        </dd>
                      </div>
                    )}
                    {watch("whOrgUID") && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">
                          Warehouse
                        </dt>
                        <dd className="mt-1 text-base text-gray-900">
                          {
                            dropdowns.warehouses.find(
                              (w) => w.value === watch("whOrgUID")
                            )?.label
                          }
                        </dd>
                      </div>
                    )}
                    {watch("vehicleUID") && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">
                          Vehicle
                        </dt>
                        <dd className="mt-1 text-base text-gray-900">
                          {
                            dropdowns.vehicles.find(
                              (v) => v.value === watch("vehicleUID")
                            )?.label
                          }
                        </dd>
                      </div>
                    )}
                    {/* {watch("locationUID") && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">
                          Location
                        </dt>
                        <dd className="mt-1 text-base text-gray-900">
                          {
                            dropdowns.locations.find(
                              (l) => l.value === watch("locationUID")
                            )?.label
                          }
                        </dd>
                      </div>
                    )} */}
                    {selectedUsers.length > 0 && (
                      <div>
                        <dt className="text-sm font-medium text-gray-500">
                          Additional Employees
                        </dt>
                        <dd className="mt-1">
                          <div className="flex flex-wrap gap-1 mt-1">
                            {selectedUsers.map((uid) => {
                              const emp = dropdowns.employees.find(
                                (e) => e.value === uid
                              );
                              return (
                                <Badge
                                  key={uid}
                                  variant="secondary"
                                  className="text-xs bg-blue-50 text-blue-700"
                                >
                                  {emp?.label}
                                </Badge>
                              );
                            })}
                          </div>
                        </dd>
                      </div>
                    )}
                  </div>
                  {/* {watch("description") && (
                    <div className="mt-4 pt-4 border-t">
                      <dt className="text-sm font-medium text-gray-500">
                        Description
                      </dt>
                      <dd className="mt-1 text-base text-gray-900">
                        {watch("description")}
                      </dd>
                    </div>
                  )} */}
                </div>

                {/* Schedule */}
                <div className="p-4 bg-muted/30 rounded-lg space-y-3">
                  <h3 className="text-sm font-medium">Schedule</h3>
                  <div className="grid grid-cols-2 gap-y-2 text-sm">
                    <div>
                      <span className="text-muted-foreground">
                        Validity Period:
                      </span>
                      <p className="font-medium">
                        {moment(watch("validFrom")).format("DD/MM/YYYY")} -{" "}
                        {moment(watch("validUpto")).format("DD/MM/YYYY")}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Frequency:</span>
                      <p className="font-medium">
                        {activeScheduleTypes.length > 1
                          ? `Multiple (${activeScheduleTypes
                              .map(
                                (type) =>
                                  type.charAt(0).toUpperCase() + type.slice(1)
                              )
                              .join(", ")})`
                          : activeScheduleTypes.length === 1
                          ? activeScheduleTypes[0].charAt(0).toUpperCase() +
                            activeScheduleTypes[0].slice(1)
                          : watch("scheduleType") === "multiple"
                          ? "Multiple"
                          : watch("scheduleType")}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">
                        Visit Duration:
                      </span>
                      <p className="font-medium">
                        {watch("visitDuration") === 0
                          ? "N/A"
                          : `${watch("visitDuration")} minutes`}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">
                        Travel Time:
                      </span>
                      <p className="font-medium">
                        {watch("travelTime") === 0
                          ? "N/A"
                          : `${watch("travelTime")} minutes`}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Visit Time:</span>
                      <p className="font-medium">
                        {watch("visitTime") || "NA"}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">End Time:</span>
                      <p className="font-medium">{watch("endTime") || "NA"}</p>
                    </div>
                  </div>
                </div>

                {/* Scheduled Customers */}
                <div className="p-4 bg-muted/30 rounded-lg space-y-3">
                  <h3 className="text-sm font-medium flex items-center gap-2">
                    <Users className="h-4 w-4" />
                    Scheduled Customers
                  </h3>
                  <div className="space-y-4">
                    {(() => {
                      const customerScheduling =
                        watch("customerScheduling") || [];
                      const scheduleType = watch("scheduleType");

                      // Group customers by day for better display
                      const customersByDay: { [key: string]: any[] } = {};

                      customerScheduling.forEach((customer: any) => {
                        const day = customer.day || "All Days";
                        if (!customersByDay[day]) {
                          customersByDay[day] = [];
                        }
                        customersByDay[day].push(customer);
                      });

                      const totalScheduled = customerScheduling.length;
                      const daysWithCustomers =
                        Object.keys(customersByDay).length;

                      if (totalScheduled === 0) {
                        return (
                          <div className="text-sm text-muted-foreground">
                            No customers scheduled yet. Please complete the
                            schedule step.
                          </div>
                        );
                      }

                      return (
                        <>
                          <div className="flex gap-2 flex-wrap">
                            <Badge
                              variant="outline"
                              className="bg-blue-50 text-blue-700 border-blue-200"
                            >
                              {totalScheduled} total scheduled
                            </Badge>
                            <Badge
                              variant="outline"
                              className="bg-green-50 text-green-700 border-green-200"
                            >
                              {daysWithCustomers} day
                              {daysWithCustomers !== 1 ? "s" : ""} configured
                            </Badge>
                            <Badge
                              variant="outline"
                              className="bg-purple-50 text-purple-700 border-purple-200"
                            >
                              {activeScheduleTypes.length > 1
                                ? `Multiple (${activeScheduleTypes.join(", ")})`
                                : activeScheduleTypes.length === 1
                                ? activeScheduleTypes[0]
                                : scheduleType === "multiple"
                                ? "Multiple"
                                : scheduleType}
                            </Badge>
                          </div>

                          {/* Frequency breakdown for multiple types */}
                          {activeScheduleTypes.length > 1 && (
                            <div className="mt-2 p-2 bg-blue-50 rounded-lg">
                              <p className="text-xs font-medium text-blue-900 mb-1">
                                Frequency Breakdown:
                              </p>
                              <div className="flex gap-2 flex-wrap">
                                {activeScheduleTypes.map((type) => {
                                  const count =
                                    scheduleCustomerAssignments[type]?.length ||
                                    0;
                                  return (
                                    <span
                                      key={type}
                                      className="text-xs bg-white px-2 py-1 rounded border border-blue-200"
                                    >
                                      {type.charAt(0).toUpperCase() +
                                        type.slice(1)}
                                      : {count} customers
                                    </span>
                                  );
                                })}
                              </div>
                            </div>
                          )}

                          {/* Day-wise customer breakdown */}
                          <div className="space-y-2 max-h-64 overflow-y-auto">
                            {Object.entries(customersByDay).map(
                              ([day, customers]) => (
                                <div
                                  key={day}
                                  className="border rounded-lg p-3 bg-white"
                                >
                                  <div className="flex items-center justify-between mb-2">
                                    <h4 className="text-sm font-medium text-gray-700">
                                      {day}
                                    </h4>
                                    <span className="text-xs text-gray-500">
                                      {customers.length} customer
                                      {customers.length !== 1 ? "s" : ""}
                                    </span>
                                  </div>
                                  <div className="space-y-1">
                                    {customers
                                      .slice(0, 3)
                                      .map((customer: any, idx: number) => (
                                        <div
                                          key={idx}
                                          className="flex items-center justify-between text-xs"
                                        >
                                          <span className="text-gray-600 truncate max-w-[200px]">
                                            {customer.name}
                                          </span>
                                          <span className="text-gray-400">
                                            {customer.startTime === "NA"
                                              ? "Time not set"
                                              : customer.startTime}
                                          </span>
                                        </div>
                                      ))}
                                    {customers.length > 3 && (
                                      <div className="text-xs text-gray-400 italic">
                                        +{customers.length - 3} more...
                                      </div>
                                    )}
                                  </div>
                                </div>
                              )
                            )}
                          </div>
                        </>
                      );
                    })()}
                  </div>
                </div>
              </div>
            </div>
          </motion.div>
        );

      default:
        return null;
    }
  };

  return (
    <Form {...form}>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          // Only submit when explicitly triggered by Create button
          return false;
        }}
      >
        <div className="min-h-screen">
          {/* Header */}
          <div className="bg-white shadow-sm">
            <div className="px-6 py-4">
              <div className="flex items-center justify-between">
                <h1 className="text-2xl font-normal text-gray-900">
                  Create a New Route
                </h1>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() =>
                    router.push("/distributiondelivery/route-management/routes")
                  }
                  className="text-gray-600 hover:text-gray-900"
                >
                  <ArrowLeft className="h-4 w-4 mr-2" />
                  Cancel Creation
                </Button>
              </div>
            </div>
          </div>

          {/* Excel Import Section */}
          {!showExcelImport && (
            <div className="px-6 py-4 bg-gray-50 border-b">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">
                    Have routes in Excel? Import them quickly
                  </p>
                </div>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setShowExcelImport(true)}
                  className="flex items-center gap-2"
                >
                  <FileSpreadsheet className="h-4 w-4" />
                  Import from Excel
                </Button>
              </div>
            </div>
          )}

          {/* Excel Import Component */}
          {showExcelImport && (
            <div className="px-6 py-4 bg-gray-50 border-b">
              <RouteExcelImport
                selectedFrequency={currentlySelectedType} // Pass current frequency selection
                onImportSuccess={(routes) => {
                  console.log("Imported routes:", routes);
                  
                  if (routes.length > 0) {
                    const firstRoute = routes[0];
                    
                    // Filter customers based on the selected frequency
                    const filteredCustomers = firstRoute.customers.filter(customer => {
                      const frequency = customer.frequency?.toLowerCase();
                      return frequency === currentlySelectedType;
                    });
                    
                    // Only import customers that match the selected frequency
                    if (filteredCustomers.length > 0) {
                      // Update form with route details
                      form.setValue("code", firstRoute.routeCode);
                      form.setValue("name", firstRoute.routeName);
                      form.setValue("orgUID", firstRoute.orgUID);
                      form.setValue("roleUID", firstRoute.roleUID);
                      
                      // Update selected customers based on frequency
                      const customerUIDs = filteredCustomers
                        .map(c => c.customerUID)
                        .filter(uid => uid);
                      
                      // Add customers to the selected frequency
                      setScheduleCustomerAssignments(prev => ({
                        ...prev,
                        [currentlySelectedType]: customerUIDs
                      }));
                      
                      // Update assigned customers
                      setAssignedCustomers(prev => {
                        const newSet = new Set(prev);
                        customerUIDs.forEach(uid => newSet.add(uid));
                        return newSet;
                      });
                      
                      toast({
                        title: "Import Successful",
                        description: `Imported ${filteredCustomers.length} ${currentlySelectedType} customers from Excel`,
                      });
                    } else {
                      toast({
                        title: "No Matching Customers",
                        description: `No ${currentlySelectedType} frequency customers found in the Excel file`,
                        variant: "destructive",
                      });
                    }
                  }
                  setShowExcelImport(false);
                }}
                onClose={() => setShowExcelImport(false)}
              />
            </div>
          )}

          {/* Progress Section - Simplified */}
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
                              width: currentStep > step.num ? "100%" : "0%",
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
          <div className="px-2 pb-8">
            <div className="w-full">
              <div className="bg-white p-2">
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
                      {/* <Button
                    type="button"
                    variant="ghost"
                    onClick={handleReset}
                    disabled={isSubmitting}
                    className="h-10"
                  >
                    <RotateCcw className="h-4 w-4 mr-2" />
                    Reset
                  </Button> */}

                      {(() => {
                        console.log(
                          "Button decision - currentStep:",
                          currentStep,
                          "progressSteps.length:",
                          progressSteps.length,
                          "Show Continue?",
                          currentStep < progressSteps.length
                        );
                        return currentStep < progressSteps.length;
                      })() ? (
                        <Button
                          type="button"
                          onClick={handleNext}
                          disabled={isSubmitting || loading.nextStep}
                          className="h-10 bg-blue-600 hover:bg-blue-700"
                        >
                          {loading.nextStep ? (
                            <>
                              <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                              Processing...
                            </>
                          ) : (
                            <>
                              Continue
                              <ArrowRight className="h-4 w-4 ml-2" />
                            </>
                          )}
                        </Button>
                      ) : (
                        <Button
                          type="button"
                          onClick={(e) => {
                            e.preventDefault();
                            e.stopPropagation();
                            console.log("Create button clicked");
                            console.log("Current form values:", watch());
                            console.log("Form errors:", errors);
                            form.handleSubmit(onSubmit)();
                          }}
                          disabled={isSubmitting}
                          className="h-10 bg-green-600 hover:bg-green-700"
                        >
                          {isSubmitting ? (
                            <>
                              <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                              Creating...
                            </>
                          ) : (
                            <>
                              <CheckCircle2 className="h-4 w-4 mr-2" />
                              Create Route
                            </>
                          )}
                        </Button>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </form>
    </Form>
  );
};

export default CreateRoute;
