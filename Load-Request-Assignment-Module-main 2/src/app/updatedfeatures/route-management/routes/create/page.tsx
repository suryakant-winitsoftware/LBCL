"use client";

import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
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
  SelectValue
} from "@/components/ui/select";
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
  Users,
  Clock,
  Calendar,
  Building2,
  Plus,
  X,
  Settings
} from "lucide-react";
import { authService } from "@/lib/auth-service";
import { api, apiService } from "@/services/api";
import { routeService } from "@/services/routeService";
import { routeServiceProduction } from "@/services/routeService.production";
import { storeService } from "@/services/storeService";
import moment from "moment";
import { cn } from "@/lib/utils";
import {
  organizationService,
  Organization,
  OrgType
} from "@/services/organizationService";
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel
} from "@/utils/organizationHierarchyUtils";

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
    visitDuration: z.number().min(1).max(480),
    travelTime: z.number().min(1).max(480),
    totalCustomers: z.number().default(0),
    // Separate toggle approach
    enableDailyWeekly: z.boolean().default(false),
    dailyWeeklyType: z.enum(["Daily", "Weekly"]).default("Daily"),
    enableFortnightly: z.boolean().default(false),
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
    selectedUsers: z.array(z.string()).default([]), // For RouteUserList

    // Separate day selections for each visit type
    dailyWeeklyDays: z.object({
      monday: z.boolean().default(true),
      tuesday: z.boolean().default(true),
      wednesday: z.boolean().default(true),
      thursday: z.boolean().default(true),
      friday: z.boolean().default(true),
      saturday: z.boolean().default(false),
      sunday: z.boolean().default(false)
    }),
    fortnightlyDays: z.object({
      monday: z.boolean().default(false),
      tuesday: z.boolean().default(false),
      wednesday: z.boolean().default(false),
      thursday: z.boolean().default(false),
      friday: z.boolean().default(false),
      saturday: z.boolean().default(true),
      sunday: z.boolean().default(false)
    })
  })
  .refine(
    (data) => {
      // Validate that at least one frequency type is enabled
      if (!data.enableDailyWeekly && !data.enableFortnightly) {
        return false;
      }

      // Validate that if any frequency type is enabled, at least one day is selected for that type
      if (data.enableDailyWeekly) {
        const hasDailyWeeklyDay = Object.values(data.dailyWeeklyDays).some(
          (day) => day === true
        );
        if (!hasDailyWeeklyDay) return false;
      }

      if (data.enableFortnightly) {
        const hasFortnightlyDay = Object.values(data.fortnightlyDays).some(
          (day) => day === true
        );
        if (!hasFortnightlyDay) return false;
      }
      return true;
    },
    {
      message: "At least one weekday must be selected for this visit frequency",
      path: ["weekDays"]
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
  { num: 2, label: "Schedule", mobileLabel: "Schedule" },
  { num: 3, label: "Customers", mobileLabel: "Customers" },
  { num: 4, label: "Review", mobileLabel: "Review" }
];

const CreateRoute: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  const currentUser = authService.getCurrentUser();
  const currentUserUID = currentUser?.uid || "SYSTEM";

  const [currentStep, setCurrentStep] = useState(1);
  const [showOptionalFields, setShowOptionalFields] = useState(false);
  const [showAdditionalUsers, setShowAdditionalUsers] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    reset,
    formState: { errors, isSubmitting },
    trigger
  } = useForm<RouteFormData>({
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
      visitDuration: 30,
      travelTime: 15,
      totalCustomers: 0,
      allowMultipleBeatsPerDay: false,
      selectedCustomers: [],
      billToCustomer: "", // No bill-to customer selected by default
      selectedUsers: [],
      storeSchedules: {},
      description: "",
      enableDailyWeekly: false,
      dailyWeeklyType: "Daily" as const,
      enableFortnightly: false,
      dailyWeeklyDays: {
        monday: true,
        tuesday: true,
        wednesday: true,
        thursday: true,
        friday: true,
        saturday: false,
        sunday: false
      },
      fortnightlyDays: {
        monday: false,
        tuesday: false,
        wednesday: false,
        thursday: false,
        friday: false,
        saturday: true,
        sunday: false
      }
    }
  });

  // Watch form values
  const watchedOrgUID = watch("orgUID");
  const selectedCustomers = watch("selectedCustomers");
  const selectedUsers = watch("selectedUsers");
  const storeSchedules = watch("storeSchedules");
  const routeName = watch("name");

  // Loading states
  const [loading, setLoading] = useState({
    organizations: false,
    warehouses: false,
    roles: false,
    employees: false,
    vehicles: false,
    locations: false,
    customers: false,
    orgTypes: false
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
    orgTypes: [] as any[]
  });

  // Organization hierarchy state
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);
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
      setValue("storeSchedules", {});
    }
  }, [storeSchedules, setValue]);

  // Track expanded scheduling panels for each customer
  const [expandedSchedules, setExpandedSchedules] = useState<
    Record<string, boolean>
  >({});

  // Customer search/filter state
  const [customerSearch, setCustomerSearch] = useState("");

  // Filter customers based on search
  const filteredCustomers = useMemo(() => {
    return dropdowns.customers.filter(
      (customer) =>
        customer.label.toLowerCase().includes(customerSearch.toLowerCase()) ||
        customer.value.toLowerCase().includes(customerSearch.toLowerCase())
    );
  }, [dropdowns.customers, customerSearch]);

  // Generate route code from name
  const generateRouteCode = useCallback((name: string) => {
    if (!name) return "";

    // Convert to uppercase and replace spaces with underscores
    const baseCode = name
      .toUpperCase()
      .replace(/[^A-Z0-9\s]/g, "") // Remove special characters
      .replace(/\s+/g, "_") // Replace spaces with underscores
      .trim() // Remove leading/trailing spaces
      .substring(0, 10); // Limit to 10 characters to leave room for suffix

    // Add a timestamp suffix to ensure uniqueness
    const timestamp = Date.now().toString().slice(-6);
    return `RT_${baseCode}_${timestamp}`;
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
        organizationService.getOrganizations(1, 1000)
      ]);

      console.log("Organization Types fetched:", typesResult);
      console.log("Organizations fetched:", orgsResult.data);

      setOrgTypes(typesResult);
      setOrganizations(orgsResult.data);

      // Build the hierarchy map (logged for debugging)
      const hierarchy = organizationService.buildOrgTypeHierarchy(typesResult);
      console.log(
        "Organization Type Hierarchy:",
        Array.from(hierarchy.entries())
      );

      // Initialize with root level organizations using utility
      const initialLevels = initializeOrganizationHierarchy(
        orgsResult.data,
        typesResult
      );
      setOrgLevels(initialLevels);
    } catch (error) {
      console.error("Error fetching organization data:", error);
      toast({
        title: "Warning",
        description: "Could not load organization hierarchy",
        variant: "default"
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
      variant: "default"
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
        filterCriterias: []
      });

      if (data.IsSuccess && data.Data?.PagedData) {
        setDropdowns((prev) => ({
          ...prev,
          roles: data.Data.PagedData.map((role: any) => ({
            value: role.UID,
            label: role.RoleNameEn || role.Code
          }))
        }));
      }
    } catch (error) {
      console.error("Error loading roles:", error);
    } finally {
      setLoading((prev) => ({ ...prev, roles: false }));
    }
  }, []);

  const loadEmployees = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, employees: true }));
    try {
      const data = await api.dropdown.getEmployee(orgUID, false);
      if (data.IsSuccess && data.Data) {
        setDropdowns((prev) => ({
          ...prev,
          employees: data.Data.map((emp: any) => ({
            value: emp.UID,
            label: emp.Label
          }))
        }));
      }
    } catch (error) {
      console.error("Error loading employees:", error);
    } finally {
      setLoading((prev) => ({ ...prev, employees: false }));
    }
  }, []);

  const loadCustomers = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, customers: true }));
    try {
      // Use storeService like in store management page
      const response = await storeService.getAllStores({
        pageNumber: 1,
        pageSize: 500,
        isCountRequired: false,
        sortCriterias: [{ sortParameter: 'Name', direction: 0 }],
        filterCriterias: orgUID ? [
          {
            name: 'FranchiseeOrgUID',
            value: orgUID,
            operator: 'equals'
          }
        ] : []
      });

      if (response.pagedData && response.pagedData.length > 0) {
        setDropdowns((prev) => ({
          ...prev,
          customers: response.pagedData.map((store: any) => ({
            value: store.UID || store.uid || store.Code || store.code,
            label: store.Name || store.name || store.Code || store.code
          }))
        }));
      } else {
        // Try fallback API if no data
        const fallbackData = await api.dropdown.getCustomer(orgUID);
        if (fallbackData.IsSuccess && fallbackData.Data) {
          setDropdowns((prev) => ({
            ...prev,
            customers: fallbackData.Data.map((customer: any) => ({
              value: customer.UID,
              label: customer.Label
            }))
          }));
        }
      }
    } catch (error) {
      console.error("Error loading customers:", error);
      toast({
        title: "Error loading customers",
        description: "Failed to load customer list. Please try again.",
        variant: "destructive"
      });
      // Set empty customers list on error
      setDropdowns((prev) => ({
        ...prev,
        customers: []
      }));
    } finally {
      setLoading((prev) => ({ ...prev, customers: false }));
    }
  }, []);

  const loadVehicles = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, vehicles: true }));
    try {
      const data = await api.dropdown.getVehicle(orgUID);
      if (data.IsSuccess && data.Data && data.Data.length > 0) {
        setDropdowns((prev) => ({
          ...prev,
          vehicles: data.Data.map((vehicle: any) => ({
            value: vehicle.UID,
            label: vehicle.Label
          }))
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
            label: wh.Label || wh.Name || wh.Code
          }))
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
          params: ["Country", "Region", "State", "City", "Area"] // Common location types
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
                filterOperator: "equals"
              }
            ]
          }
        }
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
              loc.Name || loc.Code || `${loc.Type || "Location"} - ${loc.UID}`
          }))
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
      await Promise.all([
        loadRoles(),
        loadEmployees(orgUID),
        loadCustomers(orgUID),
        loadVehicles(orgUID),
        loadWarehouses(orgUID),
        loadLocations(orgUID)
      ]);
    },
    [
      loadRoles,
      loadEmployees,
      loadCustomers,
      loadVehicles,
      loadWarehouses,
      loadLocations
    ]
  );

  // Load organizations and org types on mount
  useEffect(() => {
    loadOrganizationData();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Load dependent dropdowns when org changes
  useEffect(() => {
    if (watchedOrgUID) {
      loadDependentDropdowns(watchedOrgUID);
    }
  }, [watchedOrgUID, loadDependentDropdowns]);

  const validateStep = async (step: number): Promise<boolean> => {
    switch (step) {
      case 1:
        return await trigger(["code", "name", "orgUID", "roleUID", "status"]);
      case 2:
        const baseValidation = await trigger([
          "validFrom",
          "validUpto"
        ]);
        if (!baseValidation) return false;

        // Visit Duration and Travel Time are optional, only validate if provided
        if (watch("visitDuration") !== undefined && watch("visitDuration") !== null) {
          const durationValid = await trigger("visitDuration");
          if (!durationValid) return false;
        }
        
        if (watch("travelTime") !== undefined && watch("travelTime") !== null) {
          const travelValid = await trigger("travelTime");
          if (!travelValid) return false;
        }

        // Additional validation for frequency-specific fields - only if configured
        // Validate separate frequency types
        if (watch("enableDailyWeekly")) {
          const dailyWeeklyDays = Object.values(
            watch("dailyWeeklyDays") || {}
          ).filter(Boolean).length;

          if (watch("dailyWeeklyType") === "Weekly") {
            if (dailyWeeklyDays !== 1) {
              toast({
                title: "Validation Error",
                description: "Please select exactly one day for weekly visits",
                variant: "destructive"
              });
              return false;
            }
          } else if (watch("dailyWeeklyType") === "Daily") {
            if (dailyWeeklyDays === 0) {
              toast({
                title: "Validation Error",
                description: "Please select at least one day for daily visits",
                variant: "destructive"
              });
              return false;
            }
          }
        }

        if (watch("enableFortnightly")) {
          const fortnightlyDays = Object.values(
            watch("fortnightlyDays") || {}
          ).filter(Boolean).length;
          if (fortnightlyDays === 0) {
            toast({
              title: "Validation Error",
              description:
                "Please select at least one day for fortnightly visits",
              variant: "destructive"
            });
            return false;
          }
        }
        return true;
      case 3:
        return true; // Customer selection is optional
      default:
        return true;
    }
  };

  const handleNext = async () => {
    const isValid = await validateStep(currentStep);
    if (isValid && currentStep < progressSteps.length) {
      setCurrentStep(currentStep + 1);
    }
  };

  const handlePrevious = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const onSubmit = async (values: RouteFormData) => {
    try {
      // Get storeSchedules separately since it's not in the schema
      const currentStoreSchedules = watch("storeSchedules") || {};
      const routeUID = routeService.generateRouteUID();
      const scheduleUID = routeService.generateScheduleUID();

      // Generate a fresh code to ensure uniqueness
      const freshCode = generateRouteCode(values.name);

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
          TotalCustomers: values.selectedCustomers.length,
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
          ServerModifiedTime: undefined
        },
        RouteSchedule: {
          UID: routeUID, // Backend bug workaround - expects Route UID here
          CompanyUID: values.orgUID,
          RouteUID: routeUID,
          Name: values.name + " Schedule",
          Type: values.enableDailyWeekly
            ? values.dailyWeeklyType
            : values.enableFortnightly
            ? "Fortnightly"
            : "Daily",
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
          PlannedDays: "",
          SS: null, // Backend sync status field
          CreatedBy: currentUserUID,
          CreatedTime: undefined,
          ModifiedBy: currentUserUID,
          ModifiedTime: undefined,
          ServerAddTime: undefined,
          ServerModifiedTime: undefined
        },
        RouteScheduleDaywise: values.enableDailyWeekly
          ? {
              UID: routeService.generateDaywiseUID(),
              RouteScheduleUID: scheduleUID, // Correct reference to schedule UID
              Monday: values.dailyWeeklyDays.monday ? 1 : 0,
              Tuesday: values.dailyWeeklyDays.tuesday ? 1 : 0,
              Wednesday: values.dailyWeeklyDays.wednesday ? 1 : 0,
              Thursday: values.dailyWeeklyDays.thursday ? 1 : 0,
              Friday: values.dailyWeeklyDays.friday ? 1 : 0,
              Saturday: values.dailyWeeklyDays.saturday ? 1 : 0,
              Sunday: values.dailyWeeklyDays.sunday ? 1 : 0,
              SS: null, // Backend sync status field
              CreatedBy: currentUserUID,
              CreatedTime: undefined,
              ModifiedBy: currentUserUID,
              ModifiedTime: undefined,
              ServerAddTime: undefined,
              ServerModifiedTime: undefined
            }
          : null,
        RouteScheduleFortnight: values.enableFortnightly
          ? {
              UID: routeService.generateFortnightUID(),
              CompanyUID: values.orgUID,
              RouteScheduleUID: scheduleUID,
              // Week 1 (current week)
              Monday: values.fortnightlyDays.monday ? 1 : 0,
              Tuesday: values.fortnightlyDays.tuesday ? 1 : 0,
              Wednesday: values.fortnightlyDays.wednesday ? 1 : 0,
              Thursday: values.fortnightlyDays.thursday ? 1 : 0,
              Friday: values.fortnightlyDays.friday ? 1 : 0,
              Saturday: values.fortnightlyDays.saturday ? 1 : 0,
              Sunday: values.fortnightlyDays.sunday ? 1 : 0,
              // Week 2 (fortnightly pattern - typically same as week 1)
              MondayFN: values.fortnightlyDays.monday ? 1 : 0,
              TuesdayFN: values.fortnightlyDays.tuesday ? 1 : 0,
              WednesdayFN: values.fortnightlyDays.wednesday ? 1 : 0,
              ThursdayFN: values.fortnightlyDays.thursday ? 1 : 0,
              FridayFN: values.fortnightlyDays.friday ? 1 : 0,
              SaturdayFN: values.fortnightlyDays.saturday ? 1 : 0,
              SundayFN: values.fortnightlyDays.sunday ? 1 : 0,
              SS: null,
              CreatedBy: currentUserUID,
              CreatedTime: undefined,
              ModifiedBy: currentUserUID,
              ModifiedTime: undefined,
              ServerAddTime: undefined,
              ServerModifiedTime: undefined
            }
          : null,
        RouteCustomersList: values.selectedCustomers.map(
          (customerUID, index) => {
            const storeSchedule: StoreSchedule = (currentStoreSchedules &&
              currentStoreSchedules[customerUID]) || {
              visitTime: "",
              visitDuration: 30,
              endTime: "",
              travelTime: 15,
              sequence: index + 1,
              weekOff: {
                sunday: false,
                monday: false,
                tuesday: false,
                wednesday: false,
                thursday: false,
                friday: false,
                saturday: false
              },
              specialDays: []
            };
            return {
              UID: `${routeUID}_${customerUID}`,
              RouteUID: routeUID,
              StoreUID: customerUID, // Database uses store_uid (confirmed from logs)
              SeqNo: storeSchedule.sequence || index + 1, // Use configured sequence or fallback to index
              VisitTime: storeSchedule.visitTime || "00:00:00",
              VisitDuration: storeSchedule.visitDuration || 30,
              EndTime: storeSchedule.endTime || "00:00:00",
              IsDeleted: false,
              IsBillToCustomer: values.billToCustomer === customerUID, // Mark if this is the bill-to customer
              TravelTime: storeSchedule.travelTime || 15,
              ActionType: "Add",
              CreatedBy: currentUserUID,
              CreatedTime: undefined,
              ModifiedBy: currentUserUID,
              ModifiedTime: undefined,
              ServerAddTime: undefined,
              ServerModifiedTime: undefined
            };
          }
        ),
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
            ModifiedTime: undefined
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
            ModifiedTime: undefined
          }))
        ]
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
        createdByValue: routeMasterData.Route.CreatedBy
      });

      // Use production service which properly builds the RouteMaster structure
      const response = await routeServiceProduction.createRoute({
        code: freshCode,
        name: values.name,
        description: values.description || '',
        orgUID: values.orgUID,
        companyUID: values.orgUID, // Add missing CompanyUID field
        whOrgUID: values.whOrgUID,
        roleUID: values.roleUID,
        jobPositionUID: values.jobPositionUID || freshCode,
        vehicleUID: values.vehicleUID,
        locationUID: values.locationUID,
        user: currentUserUID,
        isActive: values.isActive,
        status: values.status,
        validFrom: values.validFrom,
        validUpto: values.validUpto,
        scheduleType: values.enableDailyWeekly 
          ? values.dailyWeeklyType 
          : values.enableFortnightly 
          ? "Fortnightly" 
          : "Daily",
        visitTime: values.visitTime,
        endTime: values.endTime,
        visitDuration: values.visitDuration,
        travelTime: values.travelTime,
        // If frequency not configured, default to weekdays (Mon-Fri)
        monday: values.enableDailyWeekly ? values.dailyWeeklyDays?.monday : true,
        tuesday: values.enableDailyWeekly ? values.dailyWeeklyDays?.tuesday : true,
        wednesday: values.enableDailyWeekly ? values.dailyWeeklyDays?.wednesday : true,
        thursday: values.enableDailyWeekly ? values.dailyWeeklyDays?.thursday : true,
        friday: values.enableDailyWeekly ? values.dailyWeeklyDays?.friday : true,
        saturday: values.enableDailyWeekly ? values.dailyWeeklyDays?.saturday : false,
        sunday: values.enableDailyWeekly ? values.dailyWeeklyDays?.sunday : false,
        week1: values.enableFortnightly,
        week2: false,
        week3: false,
        week4: false,
        allowMultipleBeatsPerDay: values.allowMultipleBeatsPerDay,
        isCustomerWithTime: values.isCustomerWithTime,
        printStanding: values.printStanding,
        printForward: values.printForward,
        printTopup: values.printTopup,
        printOrderSummary: values.printOrderSummary,
        autoFreezeJP: values.autoFreezeJP,
        addToRun: values.addToRun,
        autoFreezeRunTime: values.autoFreezeRunTime,
        selectedCustomers: values.selectedCustomers,
        billToCustomer: values.billToCustomer, // Include bill-to customer
        customerSchedules: currentStoreSchedules,
        assignedUsers: values.selectedUsers.map(uid => ({
          jobPositionUID: uid,
          fromDate: values.validFrom,
          toDate: values.validUpto
        }))
      });

      if (response.IsSuccess || response.Data > 0) {
        // Count stores with custom schedules
        const storesWithSchedules = values.selectedCustomers.filter(
          (customerUID) => {
            const schedule: StoreSchedule | undefined =
              currentStoreSchedules && currentStoreSchedules[customerUID];
            return (
              schedule &&
              (schedule.visitTime ||
                schedule.visitDuration !== 30 ||
                schedule.travelTime !== 15 ||
                (schedule.weekOff &&
                  Object.values(schedule.weekOff).some((day) => day)))
            );
          }
        ).length;

        toast({
          title: "Success",
          description: `Route created successfully with ${
            values.selectedCustomers.length
          } customers${
            storesWithSchedules > 0
              ? ` (${storesWithSchedules} with custom scheduling)`
              : ""
          }!`
        });
        router.push("/updatedfeatures/route-management/routes/manage");
      } else {
        throw new Error(response.ErrorMessage || response.Message || "Failed to create route");
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
        variant: "destructive"
      });
    }
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
      customers: []
    }));
  };

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
                          {selectedOrgs.length > 0 && (
                            <div className="text-xs text-green-600">
                              ✓ {selectedOrgs.length} level
                              {selectedOrgs.length > 1 ? "s" : ""} selected
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
                  <div className="md:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Bill To Customer Field */}
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
                    
                    {/* Description Field */}
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
                  </div>
                </div>
              </div>

              {/* Assignment Section */}
              <div className="bg-white border border-gray-200 rounded-lg p-5 mt-4">
                <h3 className="text-sm font-semibold text-gray-900 mb-4">
                  Assignment
                </h3>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Role */}
                  <div>
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Role <span className="text-red-500">*</span>
                    </Label>
                    {loading.roles ? (
                      <Skeleton className="h-10 w-full" />
                    ) : (
                      <Select
                        value={watch("roleUID")}
                        onValueChange={(value) => setValue("roleUID", value)}
                      >
                        <SelectTrigger className="h-10">
                          <SelectValue placeholder="Select role" />
                        </SelectTrigger>
                        <SelectContent>
                          {dropdowns.roles.map((role) => (
                            <SelectItem key={role.value} value={role.value}>
                              {role.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                    {errors.roleUID && (
                      <p className="text-xs text-red-500 mt-1">
                        {errors.roleUID.message}
                      </p>
                    )}
                  </div>

                  {/* Employee with + button for multiple users */}
                  <div>
                    <Label className="text-sm font-medium text-gray-700 mb-1.5">
                      Primary Employee{" "}
                      <span className="text-xs text-gray-500">
                        (automatically assigned to route)
                      </span>
                    </Label>
                    <div className="flex gap-2">
                      {loading.employees ? (
                        <Skeleton className="h-10 w-full" />
                      ) : (
                        <Select
                          value={watch("jobPositionUID")}
                          onValueChange={(value) =>
                            setValue("jobPositionUID", value)
                          }
                        >
                          <SelectTrigger className="h-10">
                            <SelectValue placeholder="Select primary employee" />
                          </SelectTrigger>
                          <SelectContent>
                            {dropdowns.employees.map((emp) => (
                              <SelectItem key={emp.value} value={emp.value}>
                                {emp.label}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      )}
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          setShowAdditionalUsers(!showAdditionalUsers)
                        }
                        className="h-10 px-3 bg-blue-50 hover:bg-blue-100 border-blue-200"
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
                                emp.value
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
                        <div className="md:col-span-2">
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
                        </div>
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
                  Schedule & Timing
                </h2>
                <p className="text-sm text-gray-600">
                  Set up route timing and frequency
                </p>
              </div>

              <div className="space-y-6">
                {/* Validity Period */}
                <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
                  <div className="flex items-center gap-2 mb-4">
                    <div className="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center">
                      <Calendar className="h-4 w-4 text-green-600" />
                    </div>
                    <h3 className="text-lg font-semibold text-gray-900">
                      Validity Period
                    </h3>
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <Label className="text-sm font-medium text-gray-700 mb-2 block">
                        Valid From <span className="text-red-500">*</span>
                      </Label>
                      <Input
                        type="date"
                        {...register("validFrom")}
                        className="h-11 border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                      />
                      {errors.validFrom && (
                        <p className="text-sm text-red-500 mt-1.5">
                          {errors.validFrom.message}
                        </p>
                      )}
                    </div>

                    <div>
                      <Label className="text-sm font-medium text-gray-700 mb-2 block">
                        Valid Until <span className="text-red-500">*</span>
                      </Label>
                      <Input
                        type="date"
                        {...register("validUpto")}
                        className="h-11 border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                      />
                      {errors.validUpto && (
                        <p className="text-sm text-red-500 mt-1.5">
                          {errors.validUpto.message}
                        </p>
                      )}
                    </div>
                  </div>
                </div>

                <Separator className="my-6" />


                {/* Visit Frequency - Optional */}
                <div className="space-y-4">
                  <div>
                    <h3 className="text-lg font-medium text-gray-900">
                      Visit Frequency
                      <span className="text-sm font-normal text-gray-500 ml-2">(Optional)</span>
                    </h3>
                    <p className="text-sm text-gray-600 mt-1">
                      Configure visit frequency. If not set, defaults to Daily visits on all weekdays.
                    </p>
                  </div>

                  {/* Separate Toggles Approach */}
                  <div className="space-y-6">
                    {/* Daily/Weekly Visits Toggle */}
                    <div className="border border-gray-200 rounded-lg p-4">
                      <div className="flex items-center justify-between mb-4">
                        <div>
                          <h4 className="text-sm font-semibold text-gray-900">
                            Daily/Weekly Visits
                          </h4>
                          <p className="text-xs text-gray-500 mt-1">
                            Enable regular daily or weekly visits
                          </p>
                        </div>
                        <label className="relative inline-flex items-center cursor-pointer">
                          <input
                            type="checkbox"
                            checked={watch("enableDailyWeekly") || false}
                            onChange={(e) =>
                              setValue("enableDailyWeekly", e.target.checked)
                            }
                            className="sr-only peer"
                          />
                          <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                        </label>
                      </div>

                      {/* Daily/Weekly Type Selection */}
                      {watch("enableDailyWeekly") && (
                        <div className="grid grid-cols-2 gap-3 mt-4">
                          <label
                            className={cn(
                              "flex items-center justify-center p-3 rounded-lg border-2 cursor-pointer transition-all",
                              watch("dailyWeeklyType") === "Daily"
                                ? "border-blue-600 bg-blue-50 text-blue-900"
                                : "border-gray-200 hover:border-gray-300 text-gray-700"
                            )}
                          >
                            <input
                              type="radio"
                              value="Daily"
                              checked={watch("dailyWeeklyType") === "Daily"}
                              onChange={(e) =>
                                setValue(
                                  "dailyWeeklyType",
                                  e.target.value as "Daily" | "Weekly"
                                )
                              }
                              className="sr-only"
                            />
                            <span className="text-sm font-medium">Daily</span>
                          </label>

                          <label
                            className={cn(
                              "flex items-center justify-center p-3 rounded-lg border-2 cursor-pointer transition-all",
                              watch("dailyWeeklyType") === "Weekly"
                                ? "border-blue-600 bg-blue-50 text-blue-900"
                                : "border-gray-200 hover:border-gray-300 text-gray-700"
                            )}
                          >
                            <input
                              type="radio"
                              value="Weekly"
                              checked={watch("dailyWeeklyType") === "Weekly"}
                              onChange={(e) =>
                                setValue(
                                  "dailyWeeklyType",
                                  e.target.value as "Daily" | "Weekly"
                                )
                              }
                              className="sr-only"
                            />
                            <span className="text-sm font-medium">Weekly</span>
                          </label>
                        </div>
                      )}
                    </div>

                    {/* Fortnightly Visits Toggle */}
                    <div className="border border-gray-200 rounded-lg p-4">
                      <div className="flex items-center justify-between">
                        <div>
                          <h4 className="text-sm font-semibold text-gray-900">
                            Fortnightly Visits
                          </h4>
                          <p className="text-xs text-gray-500 mt-1">
                            Enable visits every two weeks on selected days
                          </p>
                        </div>
                        <label className="relative inline-flex items-center cursor-pointer">
                          <input
                            type="checkbox"
                            checked={watch("enableFortnightly") || false}
                            onChange={(e) =>
                              setValue("enableFortnightly", e.target.checked)
                            }
                            className="sr-only peer"
                          />
                          <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                        </label>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Separate Day Selections for Each Visit Type */}
                <div className="space-y-6">
                  {/* Daily/Weekly Day Selection */}
                  {watch("enableDailyWeekly") && (
                    <motion.div
                      initial={{ opacity: 0, y: -10 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ duration: 0.2 }}
                      className="border border-gray-200 rounded-lg p-5"
                    >
                      <h4 className="text-sm font-semibold text-gray-900 mb-4">
                        {watch("dailyWeeklyType") === "Daily"
                          ? "Daily Visit Days (Multiple days allowed)"
                          : "Weekly Visit Day (Single day only)"}
                      </h4>
                      <div className="grid grid-cols-7 gap-2">
                        {Object.entries({
                          monday: "Mon",
                          tuesday: "Tue",
                          wednesday: "Wed",
                          thursday: "Thu",
                          friday: "Fri",
                          saturday: "Sat",
                          sunday: "Sun"
                        }).map(([day, label]) => (
                          <label
                            key={day}
                            className={cn(
                              "flex flex-col items-center justify-center p-3 rounded-lg border-2 cursor-pointer transition-all text-center",
                              !!(watch("dailyWeeklyDays") as any)?.[day]
                                ? "border-blue-600 bg-blue-50"
                                : "border-gray-200 hover:border-gray-300 bg-white"
                            )}
                          >
                            <input
                              type="checkbox"
                              checked={
                                !!(watch("dailyWeeklyDays") as any)?.[day]
                              }
                              onChange={(e) => {
                                if (watch("dailyWeeklyType") === "Weekly") {
                                  // For weekly, only allow one selection
                                  const newDays = {
                                    monday: false,
                                    tuesday: false,
                                    wednesday: false,
                                    thursday: false,
                                    friday: false,
                                    saturday: false,
                                    sunday: false,
                                    [day]: e.target.checked
                                  };
                                  setValue("dailyWeeklyDays", newDays);
                                } else {
                                  // For daily, allow multiple selections
                                  const currentDays = watch("dailyWeeklyDays");
                                  setValue("dailyWeeklyDays", {
                                    ...currentDays,
                                    [day]: e.target.checked
                                  });
                                }
                              }}
                              className="sr-only"
                            />
                            <span
                              className={cn(
                                "text-xs font-semibold",
                                !!(watch("dailyWeeklyDays") as any)?.[day]
                                  ? "text-blue-900"
                                  : "text-gray-600"
                              )}
                            >
                              {label}
                            </span>
                            <div
                              className={cn(
                                "w-8 h-8 rounded-full border-2 flex items-center justify-center mt-1",
                                !!(watch("dailyWeeklyDays") as any)?.[day]
                                  ? "border-blue-600 bg-blue-600"
                                  : "border-gray-300"
                              )}
                            >
                              {!!(watch("dailyWeeklyDays") as any)?.[day] && (
                                <CheckCircle2 className="h-4 w-4 text-white" />
                              )}
                            </div>
                          </label>
                        ))}
                      </div>

                      {watch("dailyWeeklyType") === "Weekly" && (
                        <div className="mt-4 p-3 bg-amber-50 rounded-lg">
                          <p className="text-xs text-amber-700">
                            <strong>Weekly visits:</strong> Select exactly one
                            day for weekly visits
                          </p>
                        </div>
                      )}
                      {watch("dailyWeeklyType") === "Daily" && (
                        <div className="mt-4 p-3 bg-green-50 rounded-lg">
                          <p className="text-xs text-green-700">
                            <strong>Daily visits:</strong> Select which days
                            should be active
                          </p>
                        </div>
                      )}
                    </motion.div>
                  )}

                  {/* Fortnightly Day Selection */}
                  {watch("enableFortnightly") && (
                    <motion.div
                      initial={{ opacity: 0, y: -10 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ duration: 0.2, delay: 0.1 }}
                      className="border border-gray-200 rounded-lg p-5"
                    >
                      <h4 className="text-sm font-semibold text-gray-900 mb-4">
                        Fortnightly Visit Days (Multiple days allowed)
                      </h4>
                      <div className="grid grid-cols-7 gap-2">
                        {Object.entries({
                          monday: "Mon",
                          tuesday: "Tue",
                          wednesday: "Wed",
                          thursday: "Thu",
                          friday: "Fri",
                          saturday: "Sat",
                          sunday: "Sun"
                        }).map(([day, label]) => (
                          <label
                            key={day}
                            className={cn(
                              "flex flex-col items-center justify-center p-3 rounded-lg border-2 cursor-pointer transition-all text-center",
                              !!(watch("fortnightlyDays") as any)?.[day]
                                ? "border-purple-600 bg-purple-50"
                                : "border-gray-200 hover:border-gray-300 bg-white"
                            )}
                          >
                            <input
                              type="checkbox"
                              checked={
                                !!(watch("fortnightlyDays") as any)?.[day]
                              }
                              onChange={(e) => {
                                const currentDays = watch("fortnightlyDays");
                                setValue("fortnightlyDays", {
                                  ...currentDays,
                                  [day]: e.target.checked
                                });
                              }}
                              className="sr-only"
                            />
                            <span
                              className={cn(
                                "text-xs font-semibold",
                                !!(watch("fortnightlyDays") as any)?.[day]
                                  ? "text-purple-900"
                                  : "text-gray-600"
                              )}
                            >
                              {label}
                            </span>
                            <div
                              className={cn(
                                "w-8 h-8 rounded-full border-2 flex items-center justify-center mt-1",
                                !!(watch("fortnightlyDays") as any)?.[day]
                                  ? "border-purple-600 bg-purple-600"
                                  : "border-gray-300"
                              )}
                            >
                              {!!(watch("fortnightlyDays") as any)?.[day] && (
                                <CheckCircle2 className="h-4 w-4 text-white" />
                              )}
                            </div>
                          </label>
                        ))}
                      </div>

                      <div className="mt-4 p-3 bg-purple-50 rounded-lg">
                        <p className="text-xs text-purple-700">
                          <strong>Fortnightly visits:</strong> Select days for
                          visits every two weeks
                        </p>
                      </div>
                    </motion.div>
                  )}
                </div>

                {/* Additional Settings */}
                <div className="space-y-4 mt-6">
                  <h3 className="text-lg font-medium text-gray-900">
                    Additional Settings
                  </h3>

                  <div className="flex items-center justify-between py-4 px-4 bg-white rounded-lg border">
                    <div>
                      <p className="text-sm font-medium text-gray-900">
                        Multiple Visits Per Day
                      </p>
                      <p className="text-sm text-gray-500 mt-0.5">
                        Allow the same route to be executed multiple times in a
                        day
                      </p>
                    </div>
                    <Switch
                      checked={watch("allowMultipleBeatsPerDay")}
                      onCheckedChange={(checked) =>
                        setValue("allowMultipleBeatsPerDay", checked)
                      }
                      className="data-[state=checked]:bg-blue-600"
                    />
                  </div>
                </div>
              </div>
            </div>
          </motion.div>
        );

      case 3:
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
                  Choose customers to be included in this route
                </p>
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
                      <Badge
                        variant="outline"
                        className={cn(
                          "px-4 py-2 font-semibold text-sm",
                          selectedCustomers.length > 0
                            ? "bg-blue-50 text-blue-700 border-blue-200"
                            : "bg-gray-50 text-gray-600 border-gray-200"
                        )}
                      >
                        {selectedCustomers.length} of{" "}
                        {dropdowns.customers.length} selected
                      </Badge>
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
                                selectedCustomers.length ===
                                  dropdowns.customers.length &&
                                dropdowns.customers.length > 0
                              }
                              onCheckedChange={(checked) => {
                                if (checked) {
                                  setValue(
                                    "selectedCustomers",
                                    dropdowns.customers.map((c) => c.value)
                                  );
                                } else {
                                  setValue("selectedCustomers", []);
                                }
                              }}
                              className="data-[state=checked]:bg-blue-600 data-[state=checked]:border-blue-600"
                            />
                            <span className="text-sm font-semibold text-gray-700 group-hover:text-gray-900">
                              Select All Customers
                            </span>
                          </div>
                          <span className="text-xs text-gray-500">
                            {dropdowns.customers.length} available
                          </span>
                        </label>
                      </div>

                      {/* Enhanced Customer List */}
                      <div className="space-y-3 max-h-[700px] overflow-y-auto pr-2">
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
                            const isSelected = selectedCustomers.includes(
                              customer.value
                            );
                            const currentSchedule: StoreSchedule =
                              (storeSchedules &&
                                storeSchedules[customer.value]) || {
                                visitTime: "",
                                visitDuration: 30,
                                endTime: "",
                                travelTime: 15,
                                sequence: 1,
                                weekOff: {
                                  sunday: false,
                                  monday: false,
                                  tuesday: false,
                                  wednesday: false,
                                  thursday: false,
                                  friday: false,
                                  saturday: false
                                },
                                specialDays: []
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
                                              setValue("selectedCustomers", [
                                                ...selectedCustomers,
                                                customer.value
                                              ]);
                                              setValue(
                                                `storeSchedules.${customer.value}`,
                                                {
                                                  visitTime: "",
                                                  visitDuration: 30,
                                                  endTime: "",
                                                  travelTime: 15,
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
                                                    saturday: false
                                                  },
                                                  specialDays: []
                                                }
                                              );
                                            } else {
                                              setValue(
                                                "selectedCustomers",
                                                selectedCustomers.filter(
                                                  (c) => c !== customer.value
                                                )
                                              );
                                              const newSchedules = {
                                                ...(storeSchedules || {})
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
                                              Store ID: {customer.value}
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
                                        <Button
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
                                        </Button>
                                      </div>
                                    )}
                                  </div>

                                  {/* Quick Schedule Summary */}
                                  {isSelected &&
                                    (currentSchedule.visitTime ||
                                      currentSchedule.visitDuration !== 30) && (
                                      <div className="mt-3 flex items-center space-x-4 text-xs">
                                        {currentSchedule.visitTime && (
                                          <div className="flex items-center space-x-1 text-blue-700">
                                            <Clock className="h-3 w-3" />
                                            <span>
                                              {currentSchedule.visitTime}
                                            </span>
                                          </div>
                                        )}
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
                                        ease: "easeInOut"
                                      }}
                                      className="border-t border-blue-200 bg-gradient-to-r from-blue-25 to-indigo-25"
                                    >
                                      <div className="p-4 space-y-6">
                                        {/* Timing Configuration */}
                                        <div>
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
                                        </div>

                                        {/* Store Closed Days */}
                                        <div>
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
                                        </div>
                                      </div>
                                    </motion.div>
                                  )}
                                </AnimatePresence>
                              </motion.div>
                            );
                          })
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
                    {watch("locationUID") && (
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
                    )}
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
                  {watch("description") && (
                    <div className="mt-4 pt-4 border-t">
                      <dt className="text-sm font-medium text-gray-500">
                        Description
                      </dt>
                      <dd className="mt-1 text-base text-gray-900">
                        {watch("description")}
                      </dd>
                    </div>
                  )}
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
                        {watch("enableDailyWeekly") &&
                        watch("enableFortnightly")
                          ? `${watch("dailyWeeklyType")} + Fortnightly`
                          : watch("enableDailyWeekly")
                          ? watch("dailyWeeklyType")
                          : watch("enableFortnightly")
                          ? "Fortnightly"
                          : "None"}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">
                        Visit Duration:
                      </span>
                      <p className="font-medium">
                        {watch("visitDuration")} minutes
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">
                        Travel Time:
                      </span>
                      <p className="font-medium">
                        {watch("travelTime")} minutes
                      </p>
                    </div>
                  </div>
                </div>

                {/* Customers */}
                <div className="p-4 bg-muted/30 rounded-lg space-y-3">
                  <h3 className="text-sm font-medium">Customers</h3>
                  <div className="text-sm">
                    <Badge variant="outline">
                      {selectedCustomers.length} customers selected
                    </Badge>
                    {selectedCustomers.length > 0 && (
                      <p className="text-xs text-muted-foreground mt-2">
                        {selectedCustomers
                          .slice(0, 3)
                          .map(
                            (id) =>
                              dropdowns.customers.find((c) => c.value === id)
                                ?.label
                          )
                          .join(", ")}
                        {selectedCustomers.length > 3 &&
                          `, and ${selectedCustomers.length - 3} more...`}
                      </p>
                    )}
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
    <div className="min-h-screen bg-white">
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
                router.push("/updatedfeatures/route-management/routes/manage")
              }
              className="text-gray-600 hover:text-gray-900"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Cancel Creation
            </Button>
          </div>
        </div>
      </div>

      {/* Progress Section - Simplified */}
      <div className="px-6 py-4 border-b bg-gray-50">
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
          <div className="bg-white p-6">
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
                    disabled={isSubmitting}
                    className="h-10"
                  >
                    <RotateCcw className="h-4 w-4 mr-2" />
                    Reset
                  </Button>

                  {currentStep < progressSteps.length ? (
                    <Button
                      type="button"
                      onClick={handleNext}
                      disabled={isSubmitting}
                      className="h-10 bg-blue-600 hover:bg-blue-700"
                    >
                      Continue
                      <ArrowRight className="h-4 w-4 ml-2" />
                    </Button>
                  ) : (
                    <Button
                      type="button"
                      onClick={handleSubmit(onSubmit as any)}
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
  );
};

export default CreateRoute;
