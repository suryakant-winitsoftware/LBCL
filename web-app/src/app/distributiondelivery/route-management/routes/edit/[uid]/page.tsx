"use client";

import React, {
  useState,
  useEffect,
  useCallback,
  useMemo,
  useRef,
} from "react";
import { useRouter, useParams } from "next/navigation";
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
  SelectValue,
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
  Settings,
  AlertCircle,
  Download,
} from "lucide-react";
import * as XLSX from "xlsx";
import { authService } from "@/lib/auth-service";
import { api, apiService } from "@/services/api";
import { storeService } from "@/services/storeService";
import { employeeService } from "@/services/admin/employee.service";
import { routeService } from "@/services/routeService";
import { routeServiceProduction } from "@/services/routeService.production";
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
    // Schedule type selection - all backend-supported options
    scheduleType: z
      .enum(["Daily", "Weekly", "MultiplePerWeeks", "monthly", "fortnight"])
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
    selectedUsers: z.array(z.string()).default([]), // For RouteUserList

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

    // Separate day selections for each visit type
    dailyWeeklyDays: z.object({
      monday: z.boolean().default(true),
      tuesday: z.boolean().default(true),
      wednesday: z.boolean().default(true),
      thursday: z.boolean().default(true),
      friday: z.boolean().default(true),
      saturday: z.boolean().default(false),
      sunday: z.boolean().default(false),
    }),
    fortnightlyDays: z.object({
      monday: z.boolean().default(false),
      tuesday: z.boolean().default(false),
      wednesday: z.boolean().default(false),
      thursday: z.boolean().default(false),
      friday: z.boolean().default(false),
      saturday: z.boolean().default(true),
      sunday: z.boolean().default(false),
    }),
  })
  .refine(
    (data) => {
      // Validate that at least one day is selected based on schedule type
      if (
        data.scheduleType === "Daily" ||
        data.scheduleType === "Weekly" ||
        data.scheduleType === "MultiplePerWeeks"
      ) {
        const hasDaySelected = Object.values(data.dailyWeeklyDays).some(
          (day) => day === true
        );
        if (!hasDaySelected) return false;
      }

      if (data.scheduleType === "Fortnightly") {
        const hasFortnightlyDay = Object.values(data.fortnightlyDays).some(
          (day) => day === true
        );
        if (!hasFortnightlyDay) return false;
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
  { num: 2, label: "Customer Selection", mobileLabel: "Customers" },
  { num: 3, label: "Schedule Details", mobileLabel: "Schedule" },
  { num: 4, label: "Review & Submit", mobileLabel: "Review" },
];

const EditRoute: React.FC = () => {
  const router = useRouter();
  const params = useParams();
  const routeUID = params.uid as string;
  const { toast } = useToast();
  const currentUser = authService.getCurrentUser();
  const currentUserUID = currentUser?.uid || "SYSTEM";

  const [currentStep, setCurrentStep] = useState(1);
  const [showOptionalFields, setShowOptionalFields] = useState(false);
  const [showAdditionalUsers, setShowAdditionalUsers] = useState(false);
  const [isLoadingRoute, setIsLoadingRoute] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [pendingOrgUID, setPendingOrgUID] = useState<string | null>(null);
  const [originalRouteData, setOriginalRouteData] = useState<any>(null);
  const [originalCustomersList, setOriginalCustomersList] = useState<any[]>([]);
  const [originalUsersList, setOriginalUsersList] = useState<any[]>([]);
  const [originalSchedule, setOriginalSchedule] = useState<any>(null);
  const [originalScheduleDaywise, setOriginalScheduleDaywise] =
    useState<any>(null);
  const [originalScheduleFortnight, setOriginalScheduleFortnight] =
    useState<any>(null);
  const [routeScheduleConfigs, setRouteScheduleConfigs] = useState<any[]>([]);
  const [routeScheduleCustomerMappings, setRouteScheduleCustomerMappings] =
    useState<any[]>([]);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    reset,
    getValues,
    formState: { errors, isSubmitting },
    trigger,
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
      selectedUsers: [],
      storeSchedules: {},
      scheduleType: "Daily" as
        | "Daily"
        | "Weekly"
        | "MultiplePerWeeks"
        | "Fortnightly",
      dailyWeeklyDays: {
        monday: true,
        tuesday: true,
        wednesday: true,
        thursday: true,
        friday: true,
        saturday: false,
        sunday: false,
      },
      fortnightlyDays: {
        monday: false,
        tuesday: false,
        wednesday: false,
        thursday: false,
        friday: false,
        saturday: true,
        sunday: false,
      },
    },
  });

  // Watch form values
  const watchedOrgUID = watch("orgUID");
  const watchedRoleUID = watch("roleUID");
  const selectedCustomers = watch("selectedCustomers") || [];
  const selectedUsers = watch("selectedUsers") || [];
  const storeSchedules = watch("storeSchedules") || {};
  const routeName = watch("name") || "";

  // Debug selectedCustomers whenever it changes
  useEffect(() => {
    console.log(
      "🎯 selectedCustomers changed:",
      selectedCustomers.length,
      "customers"
    );
    console.log("🎯 selectedCustomers UIDs:", selectedCustomers);
  }, [selectedCustomers]);

  // Multi-schedule state for Customer Selection step
  const [activeScheduleTypes, setActiveScheduleTypes] = useState<string[]>([
    "weekly",
  ]);
  const [currentlySelectedType, setCurrentlySelectedType] = useState("weekly");

  // Track customers assigned to each schedule type
  const [scheduleCustomerAssignments, setScheduleCustomerAssignments] =
    useState<{
      [scheduleType: string]: string[]; // customerUIDs
    }>({
      daily: [],
      weekly: [],
      monthly: [],
      fortnight: [],
    });

  // Backward compatibility alias
  const selectedPeriod = currentlySelectedType;

  // Customer search state
  const [customerSearch, setCustomerSearch] = useState("");

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

  // Loading states
  const [loading, setLoading] = useState({
    organizations: false,
    warehouses: false,
    roles: false,
    employees: false,
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

  // Filter customers based on search (after dropdowns is defined)
  const filteredCustomers = useMemo(() => {
    return dropdowns.customers.filter(
      (customer) =>
        customer.label.toLowerCase().includes(customerSearch.toLowerCase()) ||
        customer.value.toLowerCase().includes(customerSearch.toLowerCase())
    );
  }, [dropdowns.customers, customerSearch]);

  // Auto-populate Daily schedule when it's empty and customers are available
  useEffect(() => {
    console.log("🔄 Auto-populate useEffect triggered");
    console.log("📊 Conditions check:");
    console.log("  - dropdowns.customers.length:", dropdowns.customers.length);
    console.log("  - daily.length:", scheduleCustomerAssignments.daily.length);
    console.log(
      "  - weekly.length:",
      scheduleCustomerAssignments.weekly.length
    );
    console.log(
      "  - monthly.length:",
      scheduleCustomerAssignments.monthly.length
    );
    console.log(
      "  - fortnight.length:",
      scheduleCustomerAssignments.fortnight.length
    );
    console.log("  - isLoadingRoute:", isLoadingRoute);
    console.log("  - loading.customers:", loading.customers);

    if (
      dropdowns.customers.length > 0 &&
      scheduleCustomerAssignments.daily.length === 0 &&
      scheduleCustomerAssignments.weekly.length === 0 &&
      scheduleCustomerAssignments.monthly.length === 0 &&
      scheduleCustomerAssignments.fortnight.length === 0 &&
      !isLoadingRoute &&
      !loading.customers
    ) {
      // Auto-assign all customers to daily schedule if ALL frequency types are empty
      console.log(
        "🚀 Auto-populating Daily schedule with all customers (no existing frequency assignments found)"
      );
      const allCustomerUIDs = dropdowns.customers.map((c) => c.value);
      console.log(
        "🏪 All customer UIDs to assign:",
        allCustomerUIDs.length,
        "customers"
      );

      setScheduleCustomerAssignments((prev) => {
        const updatedAssignments = {
          ...prev,
          daily: allCustomerUIDs,
        };

        console.log("📋 Updated schedule assignments:", updatedAssignments);

        // Also update selectedCustomers to include all customers from all frequencies
        const allAssignedCustomers = Object.values(updatedAssignments)
          .flat()
          .filter((uid, index, array) => array.indexOf(uid) === index); // Remove duplicates

        console.log(
          "👥 Setting selectedCustomers to:",
          allAssignedCustomers.length,
          "customers"
        );
        setValue("selectedCustomers", allAssignedCustomers);

        return updatedAssignments;
      });

      // Switch to daily frequency to show the auto-populated customers
      setCurrentlySelectedType("daily");

      toast({
        title: "Auto-populated Daily Schedule",
        description: `Added ${allCustomerUIDs.length} customers to Daily frequency schedule`,
      });
    } else {
      console.log("❌ Auto-populate conditions not met");
    }
  }, [
    dropdowns.customers.length,
    scheduleCustomerAssignments.daily.length,
    scheduleCustomerAssignments.weekly.length,
    scheduleCustomerAssignments.monthly.length,
    scheduleCustomerAssignments.fortnight.length,
    isLoadingRoute,
    loading.customers,
    setValue,
    toast,
  ]);

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

  // Track if route has been loaded to prevent multiple API calls
  const routeLoadedRef = useRef(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  // Add ref to track previous role value to prevent infinite loops
  const previousRoleRef = useRef<string | undefined>(undefined);

  // Reset loaded flag when component mounts or routeUID changes
  useEffect(() => {
    routeLoadedRef.current = false;
    previousRoleRef.current = undefined;
  }, [routeUID]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      // Cancel any ongoing requests
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
      // Reset refs for next mount
      routeLoadedRef.current = false;
      previousRoleRef.current = undefined;
    };
  }, []);

  // Load existing route data for editing
  useEffect(() => {
    // Skip if already loaded or no routeUID
    if (routeLoadedRef.current || !routeUID) {
      if (!routeUID) {
        setLoadError("No route ID provided");
        setIsLoadingRoute(false);
      }
      return;
    }

    const loadRouteData = async () => {
      // Cancel any previous request
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }

      // Create new abort controller
      abortControllerRef.current = new AbortController();

      try {
        setIsLoadingRoute(true);
        setLoadError(null);

        // Check if request was aborted
        if (abortControllerRef.current.signal.aborted) {
          return;
        }

        // Use the master view API to get complete route data including schedule
        const response = await routeService.getRouteById(routeUID);

        // Check again if request was aborted
        if (abortControllerRef.current.signal.aborted) {
          return;
        }

        if (response.IsSuccess && response.Data) {
          const routeData = response.Data;

          console.log("=== RAW API RESPONSE STRUCTURE ===");
          console.log("Full response:", response);
          console.log("Response.Data keys:", Object.keys(routeData));
          console.log("RouteSchedule exists:", !!routeData.RouteSchedule);
          console.log(
            "RouteScheduleDaywise exists:",
            !!routeData.RouteScheduleDaywise
          );
          console.log(
            "RouteScheduleFortnight exists:",
            !!routeData.RouteScheduleFortnight
          );
          console.log(
            "🔍 RouteScheduleCustomerMappings exists:",
            !!routeData.RouteScheduleCustomerMappings
          );
          console.log(
            "🔍 RouteScheduleCustomerMappings count:",
            routeData.RouteScheduleCustomerMappings?.length || 0
          );
          console.log(
            "🔍 RouteScheduleCustomerMappings data:",
            routeData.RouteScheduleCustomerMappings
          );
          console.log(
            "🔍 RouteScheduleConfigs exists:",
            !!routeData.RouteScheduleConfigs
          );
          console.log(
            "🔍 RouteScheduleConfigs count:",
            routeData.RouteScheduleConfigs?.length || 0
          );
          console.log(
            "🔍 RouteScheduleConfigs data:",
            routeData.RouteScheduleConfigs
          );
          console.log("==================================");

          const route = routeData.Route; // Extract the actual route object
          const routeSchedule = routeData.RouteSchedule;
          const routeScheduleDaywise = routeData.RouteScheduleDaywise;
          const routeScheduleFortnight = routeData.RouteScheduleFortnight;
          const routeCustomersList = routeData.RouteCustomersList || [];
          const routeUsersList = routeData.RouteUserList || [];

          // Determine schedule type from backend data - normalize to lowercase
          const rawScheduleType = routeSchedule?.Type || "Daily";
          const scheduleType =
            rawScheduleType.toLowerCase() === "daily"
              ? "Daily"
              : rawScheduleType.toLowerCase() === "weekly"
              ? "Weekly"
              : rawScheduleType.toLowerCase() === "monthly"
              ? "monthly"
              : rawScheduleType.toLowerCase() === "fortnight" ||
                rawScheduleType.toLowerCase() === "fortnightly"
              ? "fortnight"
              : rawScheduleType.toLowerCase() === "multipleperweeks"
              ? "MultiplePerWeeks"
              : "Daily";

          // Debug logging
          console.log("=== ROUTE LOADING DEBUG ===");
          console.log("API Response:", response);
          console.log("Route Data Structure:", routeData);
          console.log("Actual Route Object:", route);
          console.log("Route Schedule:", routeSchedule);
          console.log("Route Schedule Daywise:", routeScheduleDaywise);
          console.log("Route Schedule Fortnight:", routeScheduleFortnight);
          console.log("Schedule Configuration:", {
            scheduleType,
            originalScheduleType: routeSchedule?.Type,
            fallbackApplied: !routeSchedule?.Type,
          });
          console.log("========================");
          console.log("Route Properties:", {
            Code: route?.Code,
            Name: route?.Name,
            OrgUID: route?.OrgUID,
            IsActive: route?.IsActive,
            Status: route?.Status,
            ValidFrom: route?.ValidFrom,
            ValidUpto: route?.ValidUpto,
          });

          // Parse planned days from JSON if available
          let parsedPlannedDays: string[] = [];
          if (routeSchedule?.PlannedDays) {
            try {
              parsedPlannedDays = JSON.parse(routeSchedule.PlannedDays);
              console.log("Parsed planned days from JSON:", parsedPlannedDays);
            } catch (e) {
              // Fallback to comma-separated if not valid JSON
              if (
                typeof routeSchedule.PlannedDays === "string" &&
                routeSchedule.PlannedDays.includes(",")
              ) {
                parsedPlannedDays = routeSchedule.PlannedDays.split(",").map(
                  (d) => d.trim()
                );
                console.log(
                  "Parsed planned days from comma-separated:",
                  parsedPlannedDays
                );
              }
            }
          }

          // Extract day-wise schedule if available or use parsed planned days
          // If routeScheduleDaywise is null, provide intelligent defaults based on schedule type
          const dailyWeeklyDays = routeScheduleDaywise
            ? {
                monday: routeScheduleDaywise.Monday === 1,
                tuesday: routeScheduleDaywise.Tuesday === 1,
                wednesday: routeScheduleDaywise.Wednesday === 1,
                thursday: routeScheduleDaywise.Thursday === 1,
                friday: routeScheduleDaywise.Friday === 1,
                saturday: routeScheduleDaywise.Saturday === 1,
                sunday: routeScheduleDaywise.Sunday === 1,
              }
            : parsedPlannedDays.length > 0
            ? {
                // Use parsed planned days
                monday: parsedPlannedDays.includes("Monday"),
                tuesday: parsedPlannedDays.includes("Tuesday"),
                wednesday: parsedPlannedDays.includes("Wednesday"),
                thursday: parsedPlannedDays.includes("Thursday"),
                friday: parsedPlannedDays.includes("Friday"),
                saturday: parsedPlannedDays.includes("Saturday"),
                sunday: parsedPlannedDays.includes("Sunday"),
              }
            : scheduleType === "Daily"
            ? {
                // For Daily schedules without daywise data, default to all weekdays
                monday: true,
                tuesday: true,
                wednesday: true,
                thursday: true,
                friday: true,
                saturday: false,
                sunday: false,
              }
            : {
                // For Weekly/Multiple schedules, default to Monday only
                monday: true,
                tuesday: false,
                wednesday: false,
                thursday: false,
                friday: false,
                saturday: false,
                sunday: false,
              };

          // Extract fortnightly schedule if available
          // For fortnightly, we use the regular days (Monday-Sunday) as the primary pattern
          // The MondayFN-SundayFN fields represent the alternate week pattern
          const fortnightlyDays = routeScheduleFortnight
            ? {
                monday: routeScheduleFortnight.Monday === 1,
                tuesday: routeScheduleFortnight.Tuesday === 1,
                wednesday: routeScheduleFortnight.Wednesday === 1,
                thursday: routeScheduleFortnight.Thursday === 1,
                friday: routeScheduleFortnight.Friday === 1,
                saturday: routeScheduleFortnight.Saturday === 1,
                sunday: routeScheduleFortnight.Sunday === 1,
              }
            : scheduleType === "Fortnightly"
            ? {
                // For Fortnightly schedules without fortnight data, default to all weekdays
                monday: true,
                tuesday: true,
                wednesday: true,
                thursday: true,
                friday: true,
                saturday: false,
                sunday: false,
              }
            : {
                // For non-fortnightly schedules, use minimal defaults
                monday: false,
                tuesday: false,
                wednesday: false,
                thursday: false,
                friday: false,
                saturday: false,
                sunday: false,
              };

          // Debug extracted days configuration
          console.log("=== SCHEDULE DATA EXTRACTION ===");
          console.log("Schedule Type:", scheduleType);
          console.log("RouteScheduleDaywise is null:", !routeScheduleDaywise);
          console.log(
            "RouteScheduleFortnight is null:",
            !routeScheduleFortnight
          );
          console.log("Applied Daily/Weekly Days:", dailyWeeklyDays);
          console.log("Applied Fortnightly Days:", fortnightlyDays);
          console.log("===============================");

          // Reset form with existing route data
          const formData = {
            code: route?.Code || "",
            name: route?.Name || "",
            orgUID: route?.OrgUID || "",
            roleUID: route?.RoleUID || "",
            jobPositionUID: route?.JobPositionUID || "",
            whOrgUID: route?.WHOrgUID || "",
            vehicleUID: route?.VehicleUID || "",
            isActive: route?.IsActive ?? true,
            status: route?.Status || "Active",
            validFrom: route?.ValidFrom
              ? moment(route.ValidFrom).format("YYYY-MM-DD")
              : moment().format("YYYY-MM-DD"),
            validUpto: route?.ValidUpto
              ? moment(route.ValidUpto).format("YYYY-MM-DD")
              : moment().add(1, "year").format("YYYY-MM-DD"),
            visitDuration: route?.VisitDuration || 30,
            travelTime: route?.TravelTime || 15,
            totalCustomers: route?.TotalCustomers || 0,
            allowMultipleBeatsPerDay:
              routeSchedule?.AllowMultipleBeatsPerDay || false,
            selectedCustomers: routeCustomersList.map(
              (customer: any) => customer.StoreUID
            ),
            // Reconstruct customer scheduling from route data
            customerScheduling: (() => {
              // Check if we have RouteScheduleCustomerMappings data
              const mappings = routeData.RouteScheduleCustomerMappings || [];
              const customerSchedulingData: any[] = [];

              // Group mappings by customer
              const customerMappingsMap = new Map<string, any[]>();
              mappings.forEach((mapping: any) => {
                if (!customerMappingsMap.has(mapping.CustomerUID)) {
                  customerMappingsMap.set(mapping.CustomerUID, []);
                }
                customerMappingsMap.get(mapping.CustomerUID)!.push(mapping);
              });

              // Create customer scheduling entries
              routeCustomersList.forEach((customer: any) => {
                const customerMappings =
                  customerMappingsMap.get(customer.UID) || [];
                const frequency =
                  customer.Frequency || scheduleType.toLowerCase();

                const scheduleConfigs = customerMappings.map((mapping: any) => {
                  // Parse the config UID to extract schedule info
                  const configParts =
                    mapping.RouteScheduleConfigUID?.split("_") || [];
                  const schedType = configParts[0];
                  const weekStr = configParts[1];
                  const dayNum = parseInt(configParts[2]) || 0;

                  return {
                    scheduleType: schedType,
                    weekNumber: weekStr?.startsWith("W")
                      ? parseInt(weekStr.substring(1))
                      : undefined,
                    dayNumber: dayNum,
                  };
                });

                customerSchedulingData.push({
                  customerUID: customer.StoreUID,
                  frequency: frequency,
                  scheduleConfigs:
                    scheduleConfigs.length > 0 ? scheduleConfigs : [],
                });
              });

              console.log(
                "Reconstructed customer scheduling:",
                customerSchedulingData
              );
              return customerSchedulingData;
            })(),
            visitTime:
              route?.VisitTime !== "00:00:00"
                ? route?.VisitTime?.substring(0, 5)
                : "",
            endTime:
              route?.EndTime !== "00:00:00"
                ? route?.EndTime?.substring(0, 5)
                : "",
            // Schedule configuration
            scheduleType: scheduleType as
              | "Daily"
              | "Weekly"
              | "MultiplePerWeeks"
              | "Fortnightly",
            dailyWeeklyDays: dailyWeeklyDays,
            fortnightlyDays: fortnightlyDays,
            // Print settings
            printStanding: route?.PrintStanding || false,
            printForward: route?.PrintForward || false,
            printTopup: route?.PrintTopup || false,
            printOrderSummary: route?.PrintOrderSummary || false,
            // Auto freeze settings
            autoFreezeJP: route?.AutoFreezeJP || false,
            addToRun: route?.AddToRun || false,
            // Customer settings
            isCustomerWithTime: route?.IsCustomerWithTime || false,
          };

          console.log("Form data being set:", formData);
          console.log("=== FORM DATA SCHEDULE INFO ===");
          console.log("Schedule Type in form data:", formData.scheduleType);
          console.log(
            "Daily/Weekly Days in form data:",
            formData.dailyWeeklyDays
          );
          console.log(
            "Fortnightly Days in form data:",
            formData.fortnightlyDays
          );
          console.log("===============================");

          reset(formData);

          // Populate scheduleCustomerAssignments from loaded customer scheduling data
          const loadedCustomerScheduling = formData.customerScheduling || [];
          const newScheduleAssignments: { [scheduleType: string]: string[] } = {
            daily: [],
            weekly: [],
            monthly: [],
            fortnight: [],
          };

          console.log("=== POPULATING SCHEDULE CUSTOMER ASSIGNMENTS ===");
          console.log("🔍 loadedCustomerScheduling:", loadedCustomerScheduling);
          console.log(
            "🔍 loadedCustomerScheduling length:",
            loadedCustomerScheduling.length
          );
          console.log("🔍 routeCustomersList:", routeCustomersList);
          console.log(
            "🔍 routeCustomersList length:",
            routeCustomersList.length
          );
          console.log(
            "🔍 RouteScheduleCustomerMappings from API:",
            routeData.RouteScheduleCustomerMappings
          );
          console.log(
            "🔍 RouteScheduleCustomerMappings length:",
            routeData.RouteScheduleCustomerMappings?.length || 0
          );

          // Method 1: Use customer scheduling data if available
          if (loadedCustomerScheduling.length > 0) {
            loadedCustomerScheduling.forEach((customerSchedule: any) => {
              const frequency = customerSchedule.frequency;
              const customerUID = customerSchedule.customerUID;

              if (frequency && newScheduleAssignments[frequency]) {
                if (!newScheduleAssignments[frequency].includes(customerUID)) {
                  newScheduleAssignments[frequency].push(customerUID);
                }
              }
            });
            console.log("📋 Populated from customerScheduling data");
          }
          // Method 2: Use RouteScheduleCustomerMappings if available
          else if (
            routeData.RouteScheduleCustomerMappings &&
            routeData.RouteScheduleCustomerMappings.length > 0
          ) {
            const mappings = routeData.RouteScheduleCustomerMappings;

            // Group mappings by RouteScheduleUID to determine frequency
            const mappingsBySchedule = mappings.reduce(
              (acc: any, mapping: any) => {
                if (!acc[mapping.RouteScheduleUID]) {
                  acc[mapping.RouteScheduleUID] = [];
                }
                acc[mapping.RouteScheduleUID].push(mapping);
                return acc;
              },
              {}
            );

            // Try to determine frequency from schedule type or route schedule data
            const routeScheduleType =
              routeSchedule?.Type?.toLowerCase() || "weekly";
            console.log("📅 Route schedule type:", routeScheduleType);

            mappings.forEach((mapping: any) => {
              const customerUID = mapping.CustomerUID;
              // For now, assign to the main schedule type - this could be enhanced
              // to parse the RouteScheduleConfigUID to determine specific frequency
              const frequency = routeScheduleType;

              if (frequency && newScheduleAssignments[frequency]) {
                if (!newScheduleAssignments[frequency].includes(customerUID)) {
                  newScheduleAssignments[frequency].push(customerUID);
                }
              }
            });
            console.log("🗺️ Populated from RouteScheduleCustomerMappings");
          }
          // Method 3: Fall back to RouteCustomersList with default frequency
          else if (routeCustomersList.length > 0) {
            const defaultFrequency = scheduleType.toLowerCase() || "weekly";
            routeCustomersList.forEach((customer: any) => {
              const customerUID = customer.StoreUID || customer.UID;
              const frequency =
                customer.Frequency?.toLowerCase() || defaultFrequency;

              if (frequency && newScheduleAssignments[frequency]) {
                if (!newScheduleAssignments[frequency].includes(customerUID)) {
                  newScheduleAssignments[frequency].push(customerUID);
                }
              }
            });
            console.log(
              "📝 Populated from RouteCustomersList with default frequency:",
              defaultFrequency
            );
          }

          console.log("=== FINAL SCHEDULE CUSTOMER ASSIGNMENTS ===");
          console.log(newScheduleAssignments);
          setScheduleCustomerAssignments(newScheduleAssignments);

          // Force update selectedCustomers to match the assignments
          const allAssignedCustomers = Object.values(newScheduleAssignments)
            .flat()
            .filter((uid, index, array) => array.indexOf(uid) === index); // Remove duplicates
          console.log(
            "🔄 Force updating selectedCustomers with:",
            allAssignedCustomers.length,
            "customers"
          );
          console.log("📊 allAssignedCustomers data:", allAssignedCustomers);
          setValue("selectedCustomers", allAssignedCustomers);

          // Verify the form was actually reset
          setTimeout(() => {
            const currentFormValues = getValues();
            console.log("=== FORM VALUES AFTER RESET ===");
            console.log(
              "Current scheduleType:",
              currentFormValues.scheduleType
            );
            console.log(
              "Current dailyWeeklyDays:",
              currentFormValues.dailyWeeklyDays
            );
            console.log(
              "Current fortnightlyDays:",
              currentFormValues.fortnightlyDays
            );
            console.log("===============================");
          }, 100);

          // Store original route data for update operations
          setOriginalRouteData(route);
          setOriginalCustomersList(routeCustomersList);
          setOriginalUsersList(routeUsersList);
          setOriginalSchedule(routeSchedule);
          setOriginalScheduleDaywise(routeScheduleDaywise);
          setOriginalScheduleFortnight(routeScheduleFortnight);

          // Store schedule configs and mappings for CustomerScheduler
          setRouteScheduleConfigs(routeData.RouteScheduleConfigs || []);
          setRouteScheduleCustomerMappings(
            routeData.RouteScheduleCustomerMappings || []
          );

          // Set up organization hierarchy for the loaded route
          if (route?.OrgUID) {
            console.log("Setting up organization hierarchy for:", route.OrgUID);
            setValue("orgUID", route.OrgUID);

            // Store the orgUID to set up hierarchy once organization data is loaded
            setPendingOrgUID(route.OrgUID);
          }

          // Verify form was reset by checking current values
          setTimeout(() => {
            const currentValues = getValues();
            console.log("Form values after reset:", currentValues);
            console.log("Organization UID set:", currentValues.orgUID);
          }, 100);

          // Mark as loaded to prevent further calls
          routeLoadedRef.current = true;

          toast({
            title: "Route Loaded",
            description: `Loaded route: ${route?.Code} - ${route?.Name}`,
          });
        } else {
          throw new Error(response.Message || "Failed to load route data");
        }
      } catch (error: any) {
        // Ignore abort errors
        if (error.name === "AbortError") {
          return;
        }

        console.error("Failed to load route:", error);
        setLoadError(error.message || "Failed to load route data");
        toast({
          title: "Error",
          description: error.message || "Failed to load route data",
          variant: "destructive",
        });
      } finally {
        setIsLoadingRoute(false);
      }
    };

    loadRouteData();

    // Cleanup function
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [routeUID]); // Only depend on routeUID

  // Ensure storeSchedules is always initialized
  useEffect(() => {
    if (!storeSchedules) {
      setValue("storeSchedules", {});
    }
  }, [storeSchedules, setValue]);

  // Load organization data on component mount
  useEffect(() => {
    loadOrganizationData();
  }, []);

  // Set up organization hierarchy when both organization data and pending orgUID are available
  useEffect(() => {
    if (pendingOrgUID && organizations.length > 0 && orgTypes.length > 0) {
      console.log(
        "Organization data loaded, reconstructing hierarchy for:",
        pendingOrgUID
      );
      reconstructOrganizationHierarchy(pendingOrgUID);
      setPendingOrgUID(null); // Clear pending state
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pendingOrgUID, organizations, orgTypes]);

  // Track expanded scheduling panels for each customer
  const [expandedSchedules, setExpandedSchedules] = useState<
    Record<string, boolean>
  >({});

  // Excel processing state
  const [isProcessingExcel, setIsProcessingExcel] = useState(false);

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
        organizationService.getOrganizations(1, 1000),
      ]);

      console.log("Organization Types fetched:", typesResult);
      console.log("Organizations fetched:", orgsResult.data);

      // Filter organization types to only show those with ShowInTemplate = true
      const filteredOrgTypes = typesResult.filter(
        (type) => type.ShowInTemplate !== false
      );

      // Filter organizations to only show those with ShowInTemplate = true
      const filteredOrganizations = orgsResult.data.filter(
        (org) => org.ShowInTemplate !== false
      );

      console.log(
        "Filtered organization types (show_in_template = true):",
        filteredOrgTypes
      );
      console.log(
        "Filtered organizations (show_in_template = true):",
        filteredOrganizations
      );

      setOrgTypes(filteredOrgTypes);
      setOrganizations(filteredOrganizations);

      // Build the hierarchy map using filtered organization types (logged for debugging)
      const hierarchy =
        organizationService.buildOrgTypeHierarchy(filteredOrgTypes);
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

  // Function to reconstruct organization hierarchy for editing
  const reconstructOrganizationHierarchy = useCallback(
    (targetOrgUID: string) => {
      if (!organizations.length || !orgTypes.length) {
        console.log("Organizations or org types not loaded yet");
        return;
      }

      // Find the target organization
      const targetOrg = organizations.find((org) => org.UID === targetOrgUID);
      if (!targetOrg) {
        console.log("Target organization not found:", targetOrgUID);
        return;
      }

      console.log("Reconstructing hierarchy for organization:", targetOrg);

      // Build the path from root to target organization
      const buildPath = (
        org: Organization,
        path: Organization[] = []
      ): Organization[] => {
        const currentPath = [org, ...path];

        if (org.ParentUID) {
          const parent = organizations.find((o) => o.UID === org.ParentUID);
          if (parent) {
            return buildPath(parent, currentPath);
          }
        }

        return currentPath;
      };

      const orgPath = buildPath(targetOrg);
      console.log(
        "Organization path:",
        orgPath.map((o) => `${o.Name} (${o.OrgTypeUID})`)
      );

      // Initialize hierarchy starting from root
      const initialLevels = initializeOrganizationHierarchy(
        organizations,
        orgTypes
      );
      let currentLevels = [...initialLevels];
      let selectedOrgsList: string[] = [];

      // Simulate selections through the path
      orgPath.forEach((pathOrg, index) => {
        const levelIndex = currentLevels.findIndex((level) =>
          level.organizations.some((org) => org.UID === pathOrg.UID)
        );

        if (levelIndex !== -1) {
          console.log(`Setting level ${levelIndex} to ${pathOrg.Name}`);

          // Select this organization at this level
          const result = handleOrganizationSelection(
            levelIndex,
            pathOrg.UID,
            currentLevels,
            selectedOrgsList,
            organizations,
            orgTypes
          );

          currentLevels = result.updatedLevels;
          selectedOrgsList = result.updatedSelectedOrgs;
        }
      });

      console.log("Final organization hierarchy:", currentLevels);
      console.log("Final selected orgs:", selectedOrgsList);

      // Update the state
      setOrgLevels(currentLevels);
      setSelectedOrgs(selectedOrgsList);
    },
    [organizations, orgTypes]
  );

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
        let employees: Array<{ value: string; label: string }> = [];

        if (roleUID) {
          console.log(
            `Loading employees for org: ${orgUID} and role: ${roleUID}`
          );

          try {
            // Primary: Use org+role API
            const orgRoleEmployees =
              await employeeService.getEmployeesSelectionItemByRoleUID(
                orgUID,
                roleUID
              );
            if (orgRoleEmployees && orgRoleEmployees.length > 0) {
              employees = orgRoleEmployees.map((item: any) => ({
                value: item.UID || item.Value || item.uid,
                label: item.Label || item.Text || `[${item.Code}] ${item.Name}`,
              }));
              console.log(
                `Found ${employees.length} employees using org+role API`
              );
            } else {
              // Secondary: Role-based API with org filtering
              console.log(
                "No results from org+role API, trying role-based API with org filtering"
              );
              const roleBasedEmployees =
                await employeeService.getReportsToEmployeesByRoleUID(roleUID);

              if (roleBasedEmployees && roleBasedEmployees.length > 0) {
                // Get org employees for cross-reference
                const orgData = await api.dropdown.getEmployee(orgUID, false);
                const orgEmployeeUIDs =
                  orgData.IsSuccess && orgData.Data
                    ? orgData.Data.map((emp: any) => emp.UID)
                    : [];

                // Filter role employees by organization
                const filteredRoleEmployees = roleBasedEmployees.filter(
                  (emp: any) =>
                    orgEmployeeUIDs.includes(emp.UID || emp.Value || emp.uid)
                );

                employees = filteredRoleEmployees.map((item: any) => ({
                  value: item.UID || item.Value || item.uid,
                  label:
                    item.Label || item.Text || `[${item.Code}] ${item.Name}`,
                }));
                console.log(
                  `Found ${employees.length} employees after org filtering from role-based API`
                );
              }
            }
          } catch (roleError) {
            console.error("Error in role-based employee loading:", roleError);
          }
        }

        // Fallback to org-based loading if no role or no results
        if (employees.length === 0) {
          console.log("Using fallback org-based employee loading");
          const data = await api.dropdown.getEmployee(orgUID, false);
          if (data.IsSuccess && data.Data) {
            employees = data.Data.map((emp: any) => ({
              value: emp.UID,
              label: emp.Label,
            }));
            console.log(
              `Found ${employees.length} employees using org-based API`
            );
          }
        }

        setDropdowns((prev) => ({
          ...prev,
          employees,
        }));
      } catch (error) {
        console.error("Error loading employees:", error);
        // Set empty array on error
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
          const formattedStores = response.pagedData.map((store: any) => ({
            value: store.UID || store.uid || store.Code || store.code,
            label: `${store.Code || store.code} - ${store.Name || store.name}`,
            code: store.Code || store.code,
            name: store.Name || store.name,
            ...store,
          }));

          if (append) {
            // Append to existing customers
            setDropdowns((prev) => {
              const combined = [...prev.customers, ...formattedStores];
              // Remove duplicates by value (UID)
              const uniqueMap = new Map();
              combined.forEach((item) => uniqueMap.set(item.value, item));
              return {
                ...prev,
                customers: Array.from(uniqueMap.values()),
              };
            });
          } else {
            // Replace customers
            setDropdowns((prev) => ({
              ...prev,
              customers: formattedStores,
            }));
          }

          // Update pagination state
          const hasMorePages = response.pagedData.length === pageSize;

          setCustomersPagination((prev) => ({
            ...prev,
            currentPage: page,
            totalCount: response.totalCount || 0,
            hasMore: hasMorePages,
            isLoadingMore: false,
          }));
        } else if (page === 1) {
          // Try fallback API only on first page if no data found
          const fallbackData = await api.dropdown.getCustomer(orgUID);
          if (fallbackData.IsSuccess && fallbackData.Data) {
            setDropdowns((prev) => ({
              ...prev,
              customers: fallbackData.Data.map((customer: any) => ({
                value: customer.UID,
                label: customer.Label,
              })),
            }));
            console.log(
              `Loaded ${fallbackData.Data.length} customers from fallback API`
            );
          }
        }
      } catch (error) {
        console.error("Error loading customers:", error);
        setCustomersPagination((prev) => ({ ...prev, isLoadingMore: false }));

        // Try fallback API on error for first page
        if (page === 1) {
          try {
            const fallbackData = await api.dropdown.getCustomer(orgUID);
            if (fallbackData.IsSuccess && fallbackData.Data) {
              setDropdowns((prev) => ({
                ...prev,
                customers: fallbackData.Data.map((customer: any) => ({
                  value: customer.UID,
                  label: customer.Label,
                })),
              }));
            }
          } catch (fallbackError) {
            console.error("Fallback API also failed:", fallbackError);
          }
        }
      } finally {
        if (!append) {
          setLoading((prev) => ({ ...prev, customers: false }));
        }
      }
    },
    []
  );

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
      await Promise.all([
        loadRoles(),
        loadEmployees(orgUID),
        loadCustomers(orgUID, 1, false, customerSearch),
        loadVehicles(orgUID),
        loadWarehouses(orgUID),
        loadLocations(orgUID),
      ]);
    },
    [
      loadRoles,
      loadEmployees,
      loadCustomers,
      loadVehicles,
      loadWarehouses,
      loadLocations,
      customerSearch,
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

  // Load employees when role changes (with infinite loop prevention)
  useEffect(() => {
    if (
      watchedOrgUID &&
      watchedRoleUID &&
      previousRoleRef.current !== watchedRoleUID
    ) {
      console.log(
        `Role changed from ${previousRoleRef.current} to ${watchedRoleUID}, reloading employees`
      );
      loadEmployees(watchedOrgUID, watchedRoleUID);
      previousRoleRef.current = watchedRoleUID;
    } else if (
      watchedOrgUID &&
      !watchedRoleUID &&
      previousRoleRef.current !== undefined
    ) {
      // Role was cleared, reload org-only employees
      console.log("Role cleared, reloading org-only employees");
      loadEmployees(watchedOrgUID);
      previousRoleRef.current = undefined;
    }
  }, [watchedRoleUID, watchedOrgUID, loadEmployees]);

  const validateStep = async (step: number): Promise<boolean> => {
    switch (step) {
      case 1:
        return await trigger(["code", "name", "orgUID", "roleUID", "status"]);
      case 2:
        return true; // Customer selection is optional
      case 3:
        const baseValidation = await trigger(["validFrom", "validUpto"]);
        if (!baseValidation) return false;

        // Visit Duration and Travel Time are optional, only validate if provided
        if (
          watch("visitDuration") !== undefined &&
          watch("visitDuration") !== null
        ) {
          const durationValid = await trigger("visitDuration");
          if (!durationValid) return false;
        }

        if (watch("travelTime") !== undefined && watch("travelTime") !== null) {
          const travelValid = await trigger("travelTime");
          if (!travelValid) return false;
        }

        // Additional validation for frequency-specific fields - only if configured
        // Validate schedule type and days
        const scheduleType = watch("scheduleType");

        if (scheduleType === "Weekly") {
          const weeklyDays = Object.values(
            watch("dailyWeeklyDays") || {}
          ).filter(Boolean).length;

          if (weeklyDays !== 1) {
            toast({
              title: "Validation Error",
              description: "Please select exactly one day for weekly visits",
              variant: "destructive",
            });
            return false;
          }
        } else if (scheduleType === "MultiplePerWeeks") {
          const multiplePerWeeksDays = Object.values(
            watch("dailyWeeklyDays") || {}
          ).filter(Boolean).length;

          if (multiplePerWeeksDays === 0) {
            toast({
              title: "Validation Error",
              description:
                "Please select at least one day for multiple visits per week",
              variant: "destructive",
            });
            return false;
          }
        } else if (scheduleType === "Fortnightly") {
          const fortnightlyDays = Object.values(
            watch("fortnightlyDays") || {}
          ).filter(Boolean).length;

          if (fortnightlyDays === 0) {
            toast({
              title: "Validation Error",
              description:
                "Please select at least one day for fortnightly visits",
              variant: "destructive",
            });
            return false;
          }
        }
        return true;
      default:
        return true;
    }
  };

  const handleNext = async () => {
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

  const handlePrevious = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  // Process Excel file directly
  const processExcelFile = async (file: File) => {
    setIsProcessingExcel(true);
    try {
      const buffer = await file.arrayBuffer();
      const workbook = XLSX.read(buffer, { type: "buffer" });
      const worksheet = workbook.Sheets[workbook.SheetNames[0]];
      const jsonData = XLSX.utils.sheet_to_json(worksheet) as Array<{
        CustomerID?: string;
        CustomerName?: string;
        StoreID?: string;
        StoreName?: string;
        ID?: string;
        Name?: string;
        [key: string]: any;
      }>;

      console.log("📊 Excel data loaded:", jsonData);

      // Find customers that match the Excel data
      const matchingCustomers = jsonData
        .map((row) => {
          const customerID = row.CustomerID || row.StoreID || row.ID || "";
          const customerName =
            row.CustomerName || row.StoreName || row.Name || "";

          return dropdowns.customers.find(
            (customer) =>
              customer.value.toLowerCase() === customerID.toLowerCase() ||
              customer.label.toLowerCase().includes(customerName.toLowerCase())
          );
        })
        .filter(
          (customer): customer is DropdownOption => customer !== undefined
        );

      if (matchingCustomers.length === 0) {
        toast({
          title: "No Matching Customers",
          description:
            "No customers found matching the Excel data. Please check the customer IDs or names.",
          variant: "destructive",
        });
        return;
      }

      // Add matching customers to selected list
      const newSelectedCustomers = [
        ...new Set([
          ...selectedCustomers,
          ...matchingCustomers.map((c) => c.value),
        ]),
      ];

      setValue("selectedCustomers", newSelectedCustomers);

      toast({
        title: "Excel Import Successful",
        description: `${matchingCustomers.length} customers imported from Excel file.`,
      });
    } catch (error) {
      console.error("Excel processing error:", error);
      toast({
        title: "Import Failed",
        description:
          "Failed to process Excel file. Please check the file format.",
        variant: "destructive",
      });
    } finally {
      setIsProcessingExcel(false);
    }
  };

  // Handle template download
  const handleDownloadTemplate = () => {
    const template = [
      {
        CustomerID: "STORE001",
        CustomerName: "Sample Store Name",
      },
      {
        CustomerID: "STORE002",
        CustomerName: "Another Store",
      },
    ];

    const ws = XLSX.utils.json_to_sheet(template);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Customer Template");
    XLSX.writeFile(wb, "customer_import_template.xlsx");

    toast({
      title: "Template Downloaded",
      description: "Excel template downloaded successfully.",
    });
  };

  const onSubmit = async (
    values: RouteFormData,
    shouldRedirect: boolean = true
  ) => {
    try {
      // Get storeSchedules separately since it's not in the schema
      const currentStoreSchedules = watch("storeSchedules") || {};

      // Use existing route UID for updates (from URL params)
      const existingRouteUID = routeUID;

      // For updates, use the existing route UID as schedule UID (backend expects this)
      const scheduleUID = existingRouteUID;

      // Use existing code or generate if missing
      const routeCode = values.code || generateRouteCode(values.name);

      // Generate current timestamp in ISO format for modified fields
      const currentTimestamp = new Date().toISOString();

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

      // Build route master data
      const routeMasterData = {
        Route: {
          UID: existingRouteUID,
          CompanyUID: values.orgUID,
          Code: routeCode,
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
          IsChangeApplied: true, // Set to true for updates
          User: currentUserUID, // Backend required field
          CreatedBy: originalRouteData?.CreatedBy || currentUserUID, // Preserve original creator
          CreatedTime: originalRouteData?.CreatedTime || currentTimestamp, // Preserve original creation time
          ModifiedBy: currentUserUID,
          ModifiedTime: currentTimestamp,
          ServerAddTime: originalRouteData?.ServerAddTime, // Preserve server timestamps
          ServerModifiedTime: currentTimestamp, // Update server modified time
        },
        RouteSchedule: {
          UID: originalSchedule?.UID || existingRouteUID, // Preserve existing schedule UID
          CompanyUID: values.orgUID,
          RouteUID: existingRouteUID,
          Name: values.name + " Schedule",
          Type: values.scheduleType.toLowerCase(), // Normalize schedule type to lowercase
          StartDate: values.validFrom,
          Status: 1,
          FromDate: values.validFrom,
          ToDate: values.validUpto,
          StartTime: values.visitTime ? values.visitTime + ":00" : "09:00:00", // Ensure proper format
          EndTime: values.endTime ? values.endTime + ":00" : "18:00:00", // Ensure proper format
          VisitDurationInMinutes: values.visitDuration,
          TravelTimeInMinutes: values.travelTime,
          NextBeatDate: originalSchedule?.NextBeatDate || values.validFrom, // Preserve next beat date
          LastBeatDate: originalSchedule?.LastBeatDate || values.validFrom, // Preserve last beat date
          AllowMultipleBeatsPerDay: values.allowMultipleBeatsPerDay,
          PlannedDays: (() => {
            // Generate proper planned days as JSON based on schedule type and selected days
            const scheduleType = values.scheduleType?.toLowerCase() || "daily";
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

            if (
              scheduleType === "daily" ||
              scheduleType === "weekly" ||
              scheduleType === "multipleperweeks"
            ) {
              Object.entries(values.dailyWeeklyDays).forEach(
                ([key, selected]) => {
                  if (selected && dayMapping[key as keyof typeof dayMapping]) {
                    selectedDays.push(
                      dayMapping[key as keyof typeof dayMapping]
                    );
                  }
                }
              );
            } else if (
              scheduleType === "fortnight" ||
              scheduleType === "fortnightly"
            ) {
              Object.entries(values.fortnightlyDays || {}).forEach(
                ([key, selected]) => {
                  if (selected && dayMapping[key as keyof typeof dayMapping]) {
                    selectedDays.push(
                      dayMapping[key as keyof typeof dayMapping]
                    );
                  }
                }
              );
            }

            // Return as JSON array string
            return JSON.stringify(selectedDays);
          })(),
          SS: originalSchedule?.SS || null, // Preserve sync status
          CreatedBy:
            originalSchedule?.CreatedBy ||
            originalRouteData?.CreatedBy ||
            currentUserUID,
          CreatedTime:
            originalSchedule?.CreatedTime ||
            originalRouteData?.CreatedTime ||
            currentTimestamp,
          ModifiedBy: currentUserUID,
          ModifiedTime: currentTimestamp,
          ServerAddTime:
            originalSchedule?.ServerAddTime || originalRouteData?.ServerAddTime,
          ServerModifiedTime: currentTimestamp,
        },
        RouteScheduleDaywise:
          values.scheduleType === "Daily" ||
          values.scheduleType === "Weekly" ||
          values.scheduleType === "MultiplePerWeeks"
            ? {
                UID: originalScheduleDaywise?.UID || existingRouteUID, // Preserve existing daywise UID
                RouteScheduleUID: originalSchedule?.UID || existingRouteUID, // Use schedule UID
                Monday: values.dailyWeeklyDays.monday ? 1 : 0,
                Tuesday: values.dailyWeeklyDays.tuesday ? 1 : 0,
                Wednesday: values.dailyWeeklyDays.wednesday ? 1 : 0,
                Thursday: values.dailyWeeklyDays.thursday ? 1 : 0,
                Friday: values.dailyWeeklyDays.friday ? 1 : 0,
                Saturday: values.dailyWeeklyDays.saturday ? 1 : 0,
                Sunday: values.dailyWeeklyDays.sunday ? 1 : 0,
                SS: originalScheduleDaywise?.SS || null, // Preserve sync status
                CreatedBy:
                  originalScheduleDaywise?.CreatedBy ||
                  originalRouteData?.CreatedBy ||
                  currentUserUID,
                CreatedTime:
                  originalScheduleDaywise?.CreatedTime ||
                  originalRouteData?.CreatedTime ||
                  currentTimestamp,
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
                ServerAddTime:
                  originalScheduleDaywise?.ServerAddTime ||
                  originalRouteData?.ServerAddTime,
                ServerModifiedTime: currentTimestamp,
              }
            : null,
        RouteScheduleFortnight:
          values.scheduleType === "Fortnightly"
            ? {
                UID: originalScheduleFortnight?.UID || existingRouteUID, // Preserve existing fortnight UID
                CompanyUID: values.orgUID,
                RouteScheduleUID: originalSchedule?.UID || existingRouteUID, // Use schedule UID
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
                CreatedBy: originalRouteData?.CreatedBy || currentUserUID,
                CreatedTime: originalRouteData?.CreatedTime || currentTimestamp,
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
                ServerAddTime: originalRouteData?.ServerAddTime,
                ServerModifiedTime: currentTimestamp,
              }
            : null,
        RouteCustomersList: (() => {
          // Build a complete customer list handling additions, updates, and deletions
          const customersList: any[] = [];

          // First, handle existing customers (either kept or marked as deleted)
          originalCustomersList.forEach((originalCustomer) => {
            const isStillSelected = values.selectedCustomers.includes(
              originalCustomer.StoreUID
            );

            if (isStillSelected) {
              // Customer is still selected - update it
              const index = values.selectedCustomers.indexOf(
                originalCustomer.StoreUID
              );
              const storeSchedule: StoreSchedule = (currentStoreSchedules &&
                currentStoreSchedules[originalCustomer.StoreUID]) || {
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
                  saturday: false,
                },
                specialDays: [],
              };

              customersList.push({
                UID: originalCustomer.UID, // Keep original UID
                RouteUID: existingRouteUID,
                StoreUID: originalCustomer.StoreUID,
                SeqNo: storeSchedule.sequence || index + 1,
                VisitTime:
                  storeSchedule.visitTime ||
                  originalCustomer.VisitTime ||
                  "00:00:00",
                VisitDuration:
                  storeSchedule.visitDuration ||
                  originalCustomer.VisitDuration ||
                  30,
                EndTime:
                  storeSchedule.endTime ||
                  originalCustomer.EndTime ||
                  "00:00:00",
                IsDeleted: false,
                IsBillToCustomer: false,
                TravelTime:
                  storeSchedule.travelTime || originalCustomer.TravelTime || 15,
                Frequency: (() => {
                  // Get frequency from customer scheduling or default to schedule type
                  const customerSchedule = values.customerScheduling?.find(
                    (cs) => cs.customerUID === originalCustomer.StoreUID
                  );
                  return (
                    customerSchedule?.frequency ||
                    values.scheduleType.toLowerCase()
                  );
                })(),
                ActionType: "Update",
                CreatedBy: originalCustomer.CreatedBy || currentUserUID,
                CreatedTime: originalCustomer.CreatedTime || currentTimestamp,
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
                ServerAddTime: originalCustomer.ServerAddTime,
                ServerModifiedTime: currentTimestamp,
              });
            } else {
              // Customer was removed - mark as deleted
              customersList.push({
                ...originalCustomer,
                IsDeleted: true,
                ActionType: "Delete",
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
                ServerModifiedTime: currentTimestamp,
              });
            }
          });

          // Then, handle new customers (not in original list)
          values.selectedCustomers.forEach((customerUID, index) => {
            const isNewCustomer = !originalCustomersList.some(
              (orig) => orig.StoreUID === customerUID
            );

            if (isNewCustomer) {
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
                  saturday: false,
                },
                specialDays: [],
              };

              customersList.push({
                UID: `${existingRouteUID}_${customerUID}_NEW_${Date.now()}`, // New UID for new customers
                RouteUID: existingRouteUID,
                StoreUID: customerUID,
                SeqNo: storeSchedule.sequence || index + 1,
                VisitTime: storeSchedule.visitTime || "00:00:00",
                VisitDuration: storeSchedule.visitDuration || 30,
                EndTime: storeSchedule.endTime || "00:00:00",
                IsDeleted: false,
                IsBillToCustomer: false,
                TravelTime: storeSchedule.travelTime || 15,
                Frequency: (() => {
                  // Get frequency from customer scheduling or default to schedule type
                  const customerSchedule = values.customerScheduling?.find(
                    (cs) => cs.customerUID === customerUID
                  );
                  return (
                    customerSchedule?.frequency ||
                    values.scheduleType.toLowerCase()
                  );
                })(),
                ActionType: "Add",
                CreatedBy: currentUserUID,
                CreatedTime: currentTimestamp,
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
                ServerAddTime: currentTimestamp,
                ServerModifiedTime: currentTimestamp,
              });
            }
          });

          return customersList;
        })(),
        // Smart RouteUserList update handling - preserve existing UIDs and track changes
        RouteUserList: (() => {
          const usersList: any[] = [];

          // Handle primary user (jobPositionUID)
          const existingPrimaryUser = originalUsersList.find(
            (user) => user.JobPositionUID === originalRouteData?.JobPositionUID
          );

          if (existingPrimaryUser) {
            // Update existing primary user if job position changed
            if (values.jobPositionUID !== originalRouteData?.JobPositionUID) {
              // Mark old primary user as deleted
              usersList.push({
                ...existingPrimaryUser,
                IsActive: false,
                IsDeleted: true,
                ActionType: "Delete",
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
              });

              // Add new primary user
              usersList.push({
                UID: `${existingRouteUID}_USER_PRIMARY_${Date.now()}`,
                RouteUID: existingRouteUID,
                JobPositionUID: values.jobPositionUID,
                FromDate: values.validFrom,
                ToDate: values.validUpto,
                IsActive: true,
                ActionType: "Add",
                CreatedBy: currentUserUID,
                CreatedTime: currentTimestamp,
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
              });
            } else {
              // Keep existing primary user with update
              usersList.push({
                ...existingPrimaryUser,
                FromDate: values.validFrom,
                ToDate: values.validUpto,
                ActionType: "Update",
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
              });
            }
          } else {
            // No existing primary user, add new one
            usersList.push({
              UID: `${existingRouteUID}_USER_PRIMARY_${Date.now()}`,
              RouteUID: existingRouteUID,
              JobPositionUID: values.jobPositionUID,
              FromDate: values.validFrom,
              ToDate: values.validUpto,
              IsActive: true,
              ActionType: "Add",
              CreatedBy: currentUserUID,
              CreatedTime: currentTimestamp,
              ModifiedBy: currentUserUID,
              ModifiedTime: currentTimestamp,
            });
          }

          // Handle additional users
          // Process existing additional users
          originalUsersList
            .filter(
              (user) =>
                user.JobPositionUID !== originalRouteData?.JobPositionUID
            )
            .forEach((originalUser) => {
              const isStillSelected = values.selectedUsers.includes(
                originalUser.JobPositionUID
              );

              if (isStillSelected) {
                // Keep existing user with update
                usersList.push({
                  ...originalUser,
                  FromDate: values.validFrom,
                  ToDate: values.validUpto,
                  ActionType: "Update",
                  ModifiedBy: currentUserUID,
                  ModifiedTime: currentTimestamp,
                });
              } else {
                // Mark as deleted
                usersList.push({
                  ...originalUser,
                  IsActive: false,
                  IsDeleted: true,
                  ActionType: "Delete",
                  ModifiedBy: currentUserUID,
                  ModifiedTime: currentTimestamp,
                });
              }
            });

          // Add new additional users
          values.selectedUsers
            .filter(
              (userUID) =>
                !originalUsersList.some(
                  (orig) => orig.JobPositionUID === userUID
                )
            )
            .forEach((userUID, index) => {
              usersList.push({
                UID: `${existingRouteUID}_USER_${Date.now()}_${index}`,
                RouteUID: existingRouteUID,
                JobPositionUID: userUID,
                FromDate: values.validFrom,
                ToDate: values.validUpto,
                IsActive: true,
                ActionType: "Add",
                CreatedBy: currentUserUID,
                CreatedTime: currentTimestamp,
                ModifiedBy: currentUserUID,
                ModifiedTime: currentTimestamp,
              });
            });

          return usersList;
        })(),
        // Build RouteScheduleConfigs and RouteScheduleCustomerMappings from customerScheduling data
        RouteScheduleConfigs: [], // Empty array - using master configs
        RouteScheduleCustomerMappings: (() => {
          const mappings: any[] = [];
          const customerScheduling = values.customerScheduling || [];

          console.log("🚀 === BUILDING RouteScheduleCustomerMappings ===");
          console.log(
            "🚀 values.customerScheduling:",
            values.customerScheduling
          );
          console.log(
            "🚀 customerScheduling length:",
            customerScheduling.length
          );

          if (customerScheduling && customerScheduling.length > 0) {
            console.log(
              "📝 Building mappings from customerScheduling:",
              customerScheduling.length,
              "entries"
            );
            console.log(
              "📝 Full customerScheduling data:",
              JSON.stringify(customerScheduling, null, 2)
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

            // Build the aggregated customer scheduling
            customerScheduling.forEach((customerSched) => {
              const existing = aggregatedCustomerScheduling.get(
                customerSched.customerUID
              );

              if (existing) {
                // Customer already exists - merge schedule configs
                existing.scheduleConfigs.push(...customerSched.scheduleConfigs);
              } else {
                // New customer - add to map
                aggregatedCustomerScheduling.set(customerSched.customerUID, {
                  customerUID: customerSched.customerUID,
                  frequency: customerSched.frequency,
                  scheduleConfigs: [...customerSched.scheduleConfigs],
                });
                // Assign sequence number
                customerSequenceMap.set(
                  customerSched.customerUID,
                  sequenceCounter++
                );
              }
            });

            // Create mappings from the aggregated customer data
            aggregatedCustomerScheduling.forEach((customerData) => {
              const customerSequence =
                customerSequenceMap.get(customerData.customerUID) || 1;

              // For daily schedules, create only ONE mapping per customer
              if (customerData.frequency === "daily") {
                const configUID = getConfigUID("daily", null, 0); // daily_NA_0

                const mapping = {
                  UID: routeService.generateScheduleUID(),
                  RouteScheduleUID: scheduleUID,
                  RouteScheduleConfigUID: configUID,
                  CustomerUID: customerData.customerUID,
                  SeqNo: customerSequence,
                  StartTime: values.visitTime
                    ? values.visitTime + ":00"
                    : "09:00:00",
                  EndTime: values.endTime ? values.endTime + ":00" : "10:00:00",
                  IsDeleted: false,
                  CreatedBy: originalRouteData?.CreatedBy || currentUserUID,
                  CreatedTime: originalRouteData?.CreatedTime,
                  ModifiedBy: currentUserUID,
                  ModifiedTime: currentTimestamp,
                  ServerAddTime: originalRouteData?.ServerAddTime,
                  ServerModifiedTime: currentTimestamp,
                };

                mappings.push(mapping);
              } else {
                // For weekly/monthly/fortnight schedules, process each schedule config
                customerData.scheduleConfigs.forEach((schedConfig) => {
                  const configUID = getConfigUID(
                    schedConfig.scheduleType,
                    schedConfig.weekNumber || null,
                    schedConfig.dayNumber || 1
                  );

                  const mapping = {
                    UID: routeService.generateScheduleUID(),
                    RouteScheduleUID: scheduleUID,
                    RouteScheduleConfigUID: configUID,
                    CustomerUID: customerData.customerUID,
                    SeqNo: customerSequence,
                    StartTime: values.visitTime
                      ? values.visitTime + ":00"
                      : "09:00:00",
                    EndTime: values.endTime
                      ? values.endTime + ":00"
                      : "10:00:00",
                    IsDeleted: false,
                    CreatedBy: originalRouteData?.CreatedBy || currentUserUID,
                    CreatedTime: originalRouteData?.CreatedTime,
                    ModifiedBy: currentUserUID,
                    ModifiedTime: currentTimestamp,
                    ServerAddTime: originalRouteData?.ServerAddTime,
                    ServerModifiedTime: currentTimestamp,
                  };

                  mappings.push(mapping);
                });
              }
            });

            console.log(
              "📋 Created",
              mappings.length,
              "route schedule customer mappings"
            );
            console.log("📋 Sample mapping:", mappings[0]);
          } else {
            // Fallback to original configs if no customerScheduling data
            console.log(
              "⚠️ No customerScheduling data, using original mappings"
            );
            console.log(
              "⚠️ Original routeScheduleCustomerMappings:",
              routeScheduleCustomerMappings
            );
            console.log(
              "⚠️ Original mappings count:",
              routeScheduleCustomerMappings?.length || 0
            );
            return routeScheduleCustomerMappings || [];
          }

          console.log("🚀 === FINAL RouteScheduleCustomerMappings ===");
          console.log("🚀 Total mappings:", mappings.length);
          console.log("🚀 Mappings data:", JSON.stringify(mappings, null, 2));
          return mappings;
        })(),
      };

      // Debug logging
      console.log(
        "Route Master Data being sent:",
        JSON.stringify(routeMasterData, null, 2)
      );
      console.log("Customer Update Details:", {
        originalCustomers: originalCustomersList.map((c) => c.StoreUID),
        selectedCustomers: values.selectedCustomers,
        addedCustomers: values.selectedCustomers.filter(
          (c) => !originalCustomersList.some((orig) => orig.StoreUID === c)
        ),
        removedCustomers: originalCustomersList
          .filter((orig) => !values.selectedCustomers.includes(orig.StoreUID))
          .map((c) => c.StoreUID),
        updatedCustomers: originalCustomersList
          .filter((orig) => values.selectedCustomers.includes(orig.StoreUID))
          .map((c) => c.StoreUID),
      });
      console.log("User Update Details:", {
        originalUsers: originalUsersList.map((u) => u.JobPositionUID),
        selectedUsers: values.selectedUsers,
        primaryUser: values.jobPositionUID,
        addedUsers: values.selectedUsers.filter(
          (u) => !originalUsersList.some((orig) => orig.JobPositionUID === u)
        ),
        removedUsers: originalUsersList
          .filter(
            (orig) => orig.JobPositionUID !== originalRouteData?.JobPositionUID
          )
          .filter((orig) => !values.selectedUsers.includes(orig.JobPositionUID))
          .map((u) => u.JobPositionUID),
      });
      console.log("Data structure validation:", {
        hasRoute: !!routeMasterData.Route,
        hasRouteSchedule: !!routeMasterData.RouteSchedule,
        hasRouteCustomersList: !!routeMasterData.RouteCustomersList,
        customerCount: routeMasterData.RouteCustomersList.length,
        createdByType: typeof routeMasterData.Route.CreatedBy,
        createdByValue: routeMasterData.Route.CreatedBy,
        scheduleType: routeMasterData.RouteSchedule?.Type,
        formScheduleType: values.scheduleType,
      });

      // Debug the complete schedule-related payloads
      console.log("=== VISIT FREQUENCY DEBUG ===");
      console.log("Form values.scheduleType:", values.scheduleType);
      console.log(
        "Complete RouteSchedule payload:",
        routeMasterData.RouteSchedule
      );
      console.log(
        "RouteScheduleDaywise payload:",
        routeMasterData.RouteScheduleDaywise
      );
      console.log(
        "RouteScheduleFortnight payload:",
        routeMasterData.RouteScheduleFortnight
      );
      console.log("==========================");

      // Log the new schedule configs being sent
      console.log("=== ROUTE SCHEDULE CONFIGS ===");
      console.log(
        "RouteScheduleConfigs count:",
        routeMasterData.RouteScheduleConfigs?.length || 0
      );
      console.log(
        "RouteScheduleConfigs data:",
        routeMasterData.RouteScheduleConfigs
      );
      console.log(
        "RouteScheduleCustomerMappings count:",
        routeMasterData.RouteScheduleCustomerMappings?.length || 0
      );
      console.log(
        "RouteScheduleCustomerMappings data:",
        routeMasterData.RouteScheduleCustomerMappings
      );
      console.log("===============================");

      // For update, bypass the production service's buildRouteMaster and send directly
      // because we need to preserve existing UIDs and relationships
      console.log("🎯 === SENDING UPDATE REQUEST TO API ===");
      console.log("🎯 Endpoint: /Route/UpdateRouteMaster");
      console.log(
        "🎯 RouteScheduleCustomerMappings count:",
        routeMasterData.RouteScheduleCustomerMappings?.length || 0
      );
      console.log("🎯 Full payload:", JSON.stringify(routeMasterData, null, 2));

      const response = await apiService.put(
        "/Route/UpdateRouteMaster",
        routeMasterData
      );

      console.log("🎯 === API RESPONSE ===");
      console.log("🎯 Response:", response);

      // Old approach that rebuilds structure (causing UID issues):
      /* const response = await routeServiceProduction.updateRoute(existingRouteUID, {
        code: routeCode,
        name: values.name,
        // ... rest of params
      }); */

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
          description: `Route updated successfully with ${
            values.selectedCustomers.length
          } customers${
            storesWithSchedules > 0
              ? ` (${storesWithSchedules} with custom scheduling)`
              : ""
          }!`,
        });

        if (shouldRedirect) {
          router.push("/distributiondelivery/route-management/routes");
        } else {
          // Reset the loaded flag to trigger data reload
          routeLoadedRef.current = false;
          previousRoleRef.current = undefined;

          // Clear form state
          reset();

          // Reload route data with fresh information
          setIsLoadingRoute(true);

          // Small delay to ensure state is reset
          setTimeout(() => {
            // This will trigger the useEffect to reload route data
            window.location.reload();
          }, 500);
        }
      } else {
        throw new Error(
          response.ErrorMessage || response.Message || "Failed to update route"
        );
      }
    } catch (error: any) {
      console.error("Route update error:", error);
      console.error("Error details:", error.details);
      console.error("Error status:", error.status);

      // Extract more specific error message
      const errorMessage =
        error.details?.Message ||
        error.details?.message ||
        error.message ||
        "Failed to update route. Backend returned an error.";

      // Log full error for debugging
      if (error.status === 500) {
        console.error(
          "Backend server error (500) - The route update transaction is failing."
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
                      {/* Reset button and info text - Only show if multiple levels or hierarchy is being used */}
                      {(orgLevels.length > 1 || selectedOrgs.length > 1) && (
                        <div className="md:col-span-2 flex items-center justify-between">
                          <div className="flex-1">
                            {orgLevels.length > 1 && !selectedOrgs.length && (
                              <div className="text-xs text-muted-foreground italic">
                                Multiple organization types available. Select
                                from any to continue.
                              </div>
                            )}
                            {selectedOrgs.length > 1 && (
                              <div className="text-xs text-green-600">
                                ✓ {selectedOrgs.length} levels selected
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
                      )}
                    </>
                  ) : (
                    <div className="md:col-span-2 text-center py-8 text-muted-foreground">
                      <Building2 className="h-8 w-8 mx-auto mb-2 opacity-50" />
                      <p className="text-sm">No organizations available</p>
                    </div>
                  )}
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
                      {/* Hidden - Add More Users button
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
                      */}
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

                {/* Hidden - Additional Employees Selection
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
                */}

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
                      const isSelected = selectedPeriod === period.value;
                      const hasCustomers = assignedCustomerUIDs.length > 0;

                      return (
                        <button
                          key={period.value}
                          type="button"
                          onClick={() => setCurrentlySelectedType(period.value)}
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
                            (
                              scheduleCustomerAssignments[
                                currentlySelectedType
                              ] || []
                            ).length > 0
                              ? "bg-blue-50 text-blue-700 border-blue-200"
                              : "bg-gray-50 text-gray-600 border-gray-200"
                          )}
                        >
                          {
                            (
                              scheduleCustomerAssignments[
                                currentlySelectedType
                              ] || []
                            ).length
                          }{" "}
                          selected for {currentlySelectedType}
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
                                (
                                  scheduleCustomerAssignments[
                                    currentlySelectedType
                                  ] || []
                                ).length === dropdowns.customers.length &&
                                dropdowns.customers.length > 0
                              }
                              onCheckedChange={(checked) => {
                                if (checked) {
                                  const allCustomerUIDs =
                                    dropdowns.customers.map((c) => c.value);

                                  // Add all customers to current schedule type assignments
                                  const updatedAssignments = {
                                    ...scheduleCustomerAssignments,
                                    [currentlySelectedType]: allCustomerUIDs,
                                  };
                                  setScheduleCustomerAssignments(
                                    updatedAssignments
                                  );

                                  // Update selectedCustomers with all assigned customers from all frequencies
                                  const allAssignedCustomers =
                                    Object.values(updatedAssignments).flat();
                                  setValue(
                                    "selectedCustomers",
                                    allAssignedCustomers
                                  );
                                } else {
                                  // Remove all customers from current schedule type assignments
                                  const updatedAssignments = {
                                    ...scheduleCustomerAssignments,
                                    [currentlySelectedType]: [],
                                  };
                                  setScheduleCustomerAssignments(
                                    updatedAssignments
                                  );

                                  // Update selectedCustomers with all assigned customers from all frequencies
                                  const allAssignedCustomers =
                                    Object.values(updatedAssignments).flat();
                                  setValue(
                                    "selectedCustomers",
                                    allAssignedCustomers
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
                            {dropdowns.customers.length} available
                          </span>
                        </label>
                      </div>

                      {/* Enhanced Customer List */}
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
                            // Check if customer is assigned to the current frequency type
                            const assignedToCurrentType =
                              scheduleCustomerAssignments[
                                currentlySelectedType
                              ] || [];
                            const isSelected = assignedToCurrentType.includes(
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
                                              // Add to current schedule type assignments
                                              const currentAssignments =
                                                scheduleCustomerAssignments[
                                                  currentlySelectedType
                                                ] || [];
                                              const updatedAssignments = {
                                                ...scheduleCustomerAssignments,
                                                [currentlySelectedType]: [
                                                  ...currentAssignments,
                                                  customer.value,
                                                ],
                                              };
                                              setScheduleCustomerAssignments(
                                                updatedAssignments
                                              );

                                              // Update selectedCustomers with all assigned customers from all frequencies
                                              const allAssignedCustomers =
                                                Object.values(
                                                  updatedAssignments
                                                ).flat();
                                              setValue(
                                                "selectedCustomers",
                                                allAssignedCustomers
                                              );

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
                                                    saturday: false,
                                                  },
                                                  specialDays: [],
                                                }
                                              );
                                            } else {
                                              // Remove from current schedule type assignments
                                              const currentAssignments =
                                                scheduleCustomerAssignments[
                                                  currentlySelectedType
                                                ] || [];
                                              const updatedAssignments = {
                                                ...scheduleCustomerAssignments,
                                                [currentlySelectedType]:
                                                  currentAssignments.filter(
                                                    (c) => c !== customer.value
                                                  ),
                                              };
                                              setScheduleCustomerAssignments(
                                                updatedAssignments
                                              );

                                              // Update selectedCustomers with all assigned customers from all frequencies
                                              const allAssignedCustomers =
                                                Object.values(
                                                  updatedAssignments
                                                ).flat();
                                              setValue(
                                                "selectedCustomers",
                                                allAssignedCustomers
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
                                              [customer.value]: !showAdvanced,
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
                                        ease: "easeInOut",
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
                                              saturday: "Sat",
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
              routeScheduleConfigs={(() => {
                console.log(
                  "🔧 Passing routeScheduleConfigs to CustomerScheduler:",
                  routeScheduleConfigs
                );
                console.log(
                  "🔧 routeScheduleConfigs count:",
                  routeScheduleConfigs.length
                );
                return routeScheduleConfigs;
              })()}
              routeScheduleCustomerMappings={(() => {
                console.log(
                  "🔧 Passing routeScheduleCustomerMappings to CustomerScheduler:",
                  routeScheduleCustomerMappings
                );
                console.log(
                  "🔧 routeScheduleCustomerMappings count:",
                  routeScheduleCustomerMappings.length
                );
                return routeScheduleCustomerMappings;
              })()}
              initialScheduleCustomerAssignments={(() => {
                console.log(
                  "🔄 Transforming scheduleCustomerAssignments for CustomerScheduler:"
                );
                console.log(
                  "📋 Original scheduleCustomerAssignments:",
                  scheduleCustomerAssignments
                );
                console.log(
                  "📋 scheduleCustomerAssignments keys:",
                  Object.keys(scheduleCustomerAssignments)
                );
                console.log("📋 Each frequency detail:");
                Object.entries(scheduleCustomerAssignments).forEach(
                  ([freq, customers]) => {
                    console.log(
                      `  - ${freq}: ${
                        Array.isArray(customers)
                          ? customers.length
                          : "not array"
                      } customers`
                    );
                    if (Array.isArray(customers) && customers.length > 0) {
                      console.log(`    Customer sample:`, customers[0]);
                    }
                  }
                );

                // Transform from complex object with customer details to simple customer UID arrays
                const transformed = Object.entries(
                  scheduleCustomerAssignments
                ).reduce((acc, [frequency, customers]) => {
                  if (Array.isArray(customers) && customers.length > 0) {
                    // Extract customer UIDs from the complex customer objects
                    acc[frequency] = customers
                      .map((customer) =>
                        typeof customer === "string"
                          ? customer
                          : customer.UID || customer.CustomerUID
                      )
                      .filter(Boolean);
                    console.log(
                      `📊 ${frequency} transformed:`,
                      acc[frequency].length,
                      "customer UIDs"
                    );
                  }
                  return acc;
                }, {} as { [key: string]: string[] });

                console.log(
                  "📋 Transformed scheduleCustomerAssignments:",
                  transformed
                );
                console.log(
                  "📊 Total frequencies with customers:",
                  Object.keys(transformed).length
                );
                return transformed;
              })()}
              availableCustomers={(() => {
                const customers = (selectedCustomers || [])
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
                console.log(
                  "👥 availableCustomers passed to CustomerScheduler:",
                  customers.length
                );
                return customers;
              })()}
              onCustomersScheduled={(scheduledCustomers) => {
                // Store the scheduled customers in form state for submission
                setValue("scheduledCustomers" as any, scheduledCustomers);
                console.log(
                  "📅 Customers scheduled:",
                  scheduledCustomers.length
                );
              }}
              onCustomerSchedulingChange={(customerScheduling) => {
                console.log(
                  "🔄 === onCustomerSchedulingChange CALLBACK TRIGGERED ==="
                );
                console.log(
                  "🔄 Received customerScheduling:",
                  customerScheduling
                );
                console.log(
                  "🔄 CustomerScheduling length:",
                  customerScheduling.length
                );

                // Store the customer scheduling data for the new backend structure
                setValue("customerScheduling", customerScheduling);

                // Verify the value was set
                const currentValue = watch("customerScheduling");
                console.log(
                  "🔄 Current form value after setValue:",
                  currentValue
                );
                console.log("🔄 Form value length:", currentValue?.length || 0);

                console.log(
                  "📊 Customer scheduling updated from CustomerScheduler:",
                  customerScheduling.length,
                  "entries"
                );
                console.log(
                  "📊 Details:",
                  customerScheduling.map((cs) => ({
                    customerUID: cs.customerUID,
                    frequency: cs.frequency,
                    configCount: cs.scheduleConfigs?.length || 0,
                  }))
                );
                console.log(
                  "🔄 === onCustomerSchedulingChange CALLBACK COMPLETED ==="
                );
              }}
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
                        {watch("scheduleType") || "Daily"}
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

  // Show loading state while route data is being fetched
  if (isLoadingRoute) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-4xl mx-auto p-6 space-y-6">
          {/* Header Skeleton */}
          <div className="space-y-4">
            <Skeleton className="h-8 w-48" />
            <Skeleton className="h-4 w-96" />
          </div>

          {/* Progress Steps Skeleton */}
          <div className="flex justify-center mb-8">
            <div className="flex items-center space-x-4">
              {[1, 2, 3, 4].map((step, index) => (
                <div key={step} className="flex items-center">
                  <Skeleton className="h-10 w-10 rounded-full" />
                  {index < 3 && <Skeleton className="h-0.5 w-16 mx-4" />}
                </div>
              ))}
            </div>
          </div>

          {/* Form Content Skeleton */}
          <div className="bg-white rounded-lg shadow-sm border p-6">
            <div className="space-y-6">
              {/* Basic Info Section */}
              <div className="space-y-4">
                <Skeleton className="h-6 w-32" />
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                </div>
              </div>

              {/* Organization Section */}
              <div className="space-y-4">
                <Skeleton className="h-6 w-40" />
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-28" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-16" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                </div>
              </div>

              {/* Additional Fields */}
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  {[1, 2, 3].map((item) => (
                    <div key={item} className="space-y-2">
                      <Skeleton className="h-4 w-20" />
                      <Skeleton className="h-10 w-full" />
                    </div>
                  ))}
                </div>
              </div>

              {/* Schedule Section */}
              <div className="space-y-4">
                <Skeleton className="h-6 w-36" />
                <div className="grid grid-cols-7 gap-2">
                  {["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map(
                    (day) => (
                      <div key={day} className="space-y-2">
                        <Skeleton className="h-4 w-8" />
                        <Skeleton className="h-10 w-full" />
                      </div>
                    )
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Action Buttons Skeleton */}
          <div className="flex justify-between">
            <Skeleton className="h-10 w-24" />
            <div className="flex gap-2">
              <Skeleton className="h-10 w-20" />
              <Skeleton className="h-10 w-16" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Show error state if route failed to load
  if (loadError) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center max-w-md">
          <div className="text-red-500 mb-4">
            <AlertCircle className="h-12 w-12 mx-auto" />
          </div>
          <h2 className="text-xl font-semibold text-gray-900 mb-2">
            Failed to Load Route
          </h2>
          <p className="text-gray-600 mb-4">{loadError}</p>
          <Button onClick={() => window.location.reload()} variant="outline">
            Try Again
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <div className="bg-white shadow-sm">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-normal text-gray-900">
              Edit Route {routeName && `- ${routeName}`}
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
              Cancel Edit
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
      <div className="px-6 pb-8">
        <div className="max-w-6xl">
          <div className="bg-white p-6">
            <AnimatePresence mode="wait">{renderStepContent()}</AnimatePresence>

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
                  {currentStep < progressSteps.length ? (
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
                    <div className="flex gap-3">
                      <Button
                        type="button"
                        onClick={handleSubmit((values) =>
                          onSubmit(values as RouteFormData, false)
                        )}
                        disabled={isSubmitting}
                        variant="outline"
                        className="h-10"
                      >
                        {isSubmitting ? (
                          <>
                            <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                            Saving...
                          </>
                        ) : (
                          <>
                            <Settings className="h-4 w-4 mr-2" />
                            Save & Continue Editing
                          </>
                        )}
                      </Button>
                      <Button
                        type="button"
                        onClick={handleSubmit((values) =>
                          onSubmit(values as RouteFormData, true)
                        )}
                        disabled={isSubmitting}
                        className="h-10 bg-green-600 hover:bg-green-700"
                      >
                        {isSubmitting ? (
                          <>
                            <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                            Updating...
                          </>
                        ) : (
                          <>
                            <CheckCircle2 className="h-4 w-4 mr-2" />
                            Save & Close
                          </>
                        )}
                      </Button>
                    </div>
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

export default EditRoute;
