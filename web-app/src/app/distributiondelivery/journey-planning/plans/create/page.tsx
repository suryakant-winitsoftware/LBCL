"use client";

import React, { useState, useEffect, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { motion, AnimatePresence } from "framer-motion";
import moment from "moment";
import { cn } from "@/lib/utils";

// UI Components
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/ui/use-toast";
import { Form } from "@/components/ui/form";

// Icons
import {
  ArrowLeft,
  ArrowRight,
  CheckCircle2,
  Calendar,
  Users,
  Building2,
  Loader2,
  RotateCcw
} from "lucide-react";

// Services & Components
import { authService } from "@/lib/auth-service";
import { apiService, api } from "@/services/api";
import { holidayService } from "@/services/holidayService";
import { storeHistoryService } from "@/services/storeHistoryService";
import { BulkJourneyPlanGenerator } from "@/components/journey-plan/BulkJourneyPlanGenerator";
import {
  organizationService,
  Organization,
  OrgType
} from "@/services/organizationService";
import { journeyPlanDataLoader } from "@/services/journeyPlanDataLoader";
import { cacheService } from "@/services/cacheService";
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel
} from "@/utils/organizationHierarchyUtils";

// Stepper Components
import {
  StepBasicSetup,
  StepScheduleCustomers,
  StepReview
} from "@/components/journey-plan/create-steps";

// Types
interface DropdownOption {
  value: string;
  label: string;
}

interface Employee {
  JobPositionUID: string;
  LoginId: string;
  Name: string;
  Mobile?: string;
  Status?: string;
  RouteUID?: string;
  IsAssignedToRoute?: boolean;
}

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  Type?: string;
  Status?: string;
  SeqNo?: number;
}

interface CustomerWithTime extends Customer {
  startTime: string;
  endTime: string;
  visitDuration: number;
  visitDay?: Date;
}

// Using imported StepConfig type from components

// Form Schema
const journeyPlanFormSchema = z.object({
  // Step 1: Basic Setup
  orgUID: z.string().min(1, "Organization is required"),
  routeUID: z.string().min(1, "Route is required"),
  selectedEmployees: z
    .array(z.string())
    .min(1, "At least one employee is required"), // Multiple employees
  loginIds: z.array(z.string()).optional(), // Multiple login IDs
  selectedEmployee: z.string().optional(), // Keep for backward compatibility
  loginId: z.string().optional(), // Keep for backward compatibility
  routeScheduleUID: z.string().optional(),
  scheduleType: z.enum(["Daily", "Weekly", "Monthly"]).default("Daily"),
  approvalStatus: z.enum(["draft", "approved", "pending"]).default("draft"),

  // Step 2: Schedule & Planning
  visitDate: z.date({
    required_error: "Visit date is required"
  }),
  endDate: z.date().optional(), // For recurring schedules
  plannedStartTime: z.string().min(1, "Planned start time is required"),
  plannedEndTime: z.string().min(1, "Planned end time is required"),
  defaultDuration: z
    .number()
    .min(10, "Duration must be at least 10 minutes")
    .default(30),
  defaultTravelTime: z
    .number()
    .min(5, "Travel time must be at least 5 minutes")
    .default(15),

  // Step 3: Customer Selection
  selectedCustomersWithTimes: z
    .array(
      z.object({
        customerUID: z.string(),
        startTime: z.string(),
        endTime: z.string(),
        visitDuration: z.number(),
        visitDay: z.date().optional()
      })
    )
    .min(1, "At least one customer must be selected"),

  // Additional planning fields
  specialInstructions: z.string().optional(),
  notes: z.string().optional(),

  // Optional fields for enhanced planning - PLANNING ONLY (not execution)
  locationUID: z.string().optional(), // Journey plan location/branch
  vehicleUID: z.string().optional(), // Assigned vehicle for journey
  routeWHOrgUID: z.string().optional(), // Warehouse organization
  emergencyContact: z.string().optional() // Emergency contact for the journey
});

type JourneyPlanFormData = z.infer<typeof journeyPlanFormSchema>;

// Step Configuration
const progressSteps = [
  { num: 1, label: "Basic Setup", mobileLabel: "Setup" },
  { num: 2, label: "Schedule & Customers", mobileLabel: "Schedule" },
  { num: 3, label: "Review & Create", mobileLabel: "Review" }
];

export default function CreateJourneyPlanPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [currentStep, setCurrentStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [isPageReady, setIsPageReady] = useState(false);

  // Step 1 States
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [routes, setRoutes] = useState<any[]>([]);
  const [allRoutes, setAllRoutes] = useState<any[]>([]);
  const [routesWithExistingPlans, setRoutesWithExistingPlans] = useState<
    string[]
  >([]); // Store all routes
  const [salesmen, setSalesmen] = useState<Employee[]>([]); // Only salesmen, not all employees
  const [selectedSalesman, setSelectedSalesman] = useState<Employee | null>(
    null
  );
  const [vehicles, setVehicles] = useState<any[]>([]);
  const [locations, setLocations] = useState<any[]>([]);
  const [showAvailableRoutesOnly, setShowAvailableRoutesOnly] = useState(false); // Disabled due to backend API issue

  // Organization hierarchy state
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([]);
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]);

  // Dropdown data for organization hierarchy
  const [dropdowns, setDropdowns] = useState({
    organizations: [] as DropdownOption[]
  });

  // Step 2 States
  const [selectedSchedule, setSelectedSchedule] = useState<any>(null);
  const [showBulkGenerator, setShowBulkGenerator] = useState(false);

  // Bulk Generation States
  const [bulkGenerationMode, setBulkGenerationMode] = useState<
    "single" | "bulk"
  >("single");
  const [bulkConfig, setBulkConfig] = useState({
    type: "daily" as "daily" | "weekly" | "monthly",
    startDate: new Date(),
    endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000), // 30 days from now
    weeklyPattern: {
      monday: false,
      tuesday: false,
      wednesday: false,
      thursday: false,
      friday: false,
      saturday: false,
      sunday: false
    }
  });

  // Step 3 States
  const [routeCustomers, setRouteCustomers] = useState<Customer[]>([]);
  const [selectedCustomersWithTimes, setSelectedCustomersWithTimes] = useState<
    CustomerWithTime[]
  >([]);
  const [customerSearchTerm, setCustomerSearchTerm] = useState("");

  // Track if schedule times have been set from route
  const scheduleTimesSet = useRef(false);

  // Loading States
  const [dropdownsLoading, setDropdownsLoading] = useState({
    organizations: false,
    routes: false,
    employees: false,
    vehicles: false,
    locations: false,
    customers: false
  });

  const form = useForm<JourneyPlanFormData>({
    resolver: zodResolver(journeyPlanFormSchema),
    defaultValues: {
      orgUID: "",
      routeUID: "",
      selectedEmployees: [],
      loginIds: [],
      selectedEmployee: "",
      loginId: "",
      routeScheduleUID: "",
      scheduleType: "Daily",
      approvalStatus: "draft",
      visitDate: new Date(),
      endDate: undefined,
      plannedStartTime: "00:00", // Default to 00:00
      plannedEndTime: "00:00", // Default to 00:00
      defaultDuration: 30,
      defaultTravelTime: 15,
      selectedCustomersWithTimes: [],
      specialInstructions: "",
      notes: "",

      // Optional enhanced planning fields - PLANNING ONLY
      locationUID: "",
      vehicleUID: "",
      routeWHOrgUID: "",
      emergencyContact: ""
    }
  });

  // Track if initial load is done
  const initialLoadDone = useRef(false);

  // Load initial data with optimized loader
  useEffect(() => {
    if (initialLoadDone.current) return;
    initialLoadDone.current = true;

    // Show page immediately, load data in background
    setIsPageReady(true);
    loadInitialDataOptimized();
  }, []);

  // Sync selectedCustomersWithTimes state with form value
  useEffect(() => {
    const formCustomers = form.watch("selectedCustomersWithTimes");
    if (formCustomers && Array.isArray(formCustomers)) {
      // Convert form format to CustomerWithTime format for display
      const customersWithDetails = formCustomers
        .map((fc) => {
          const customer = routeCustomers.find((c) => c.UID === fc.customerUID);
          if (customer) {
            return {
              ...customer,
              startTime: fc.startTime,
              endTime: fc.endTime,
              visitDuration: fc.visitDuration,
              visitDay: fc.visitDay
            };
          }
          return null;
        })
        .filter(Boolean) as CustomerWithTime[];

      setSelectedCustomersWithTimes(customersWithDetails);
    }
  }, [form.watch("selectedCustomersWithTimes"), routeCustomers]);

  // Watch orgUID from form
  const watchedOrgUID = form.watch("orgUID");

  // Track loading state to prevent duplicate calls
  const loadingRoutes = useRef(false);
  const lastLoadedOrgUID = useRef<string | null>(null);

  // Optimized load dependent dropdowns - Load only routes initially
  const loadDependentDropdowns = useCallback(
    async (orgUID: string) => {
      // Prevent duplicate calls for the same orgUID
      if (loadingRoutes.current || lastLoadedOrgUID.current === orgUID) {
        console.log("[Routes] Skipping duplicate load for orgUID:", orgUID);
        return;
      }

      loadingRoutes.current = true;
      lastLoadedOrgUID.current = orgUID;

      // Only load routes initially, others on-demand
      setDropdownsLoading((prev) => ({ ...prev, routes: true }));

      try {
        // Check localStorage first
        const cacheKey = `routes_${orgUID}`;
        const cached = localStorage.getItem(cacheKey);

        if (cached) {
          const cachedData = JSON.parse(cached);
          if (Date.now() - cachedData.timestamp < 5 * 60 * 1000) {
            // 5 min cache
            console.log("[Routes] Loading from cache for orgUID:", orgUID);
            setAllRoutes(cachedData.routes);

            // Apply filtering if enabled
            if (showAvailableRoutesOnly) {
              const visitDate = form.watch("visitDate");
              if (visitDate) {
                console.log(
                  "[Routes] Applying filter to cached routes for date:",
                  visitDate
                );
                filterAvailableRoutes(cachedData.routes);
              } else {
                console.log(
                  "[Routes] No visit date set yet, showing all cached routes temporarily"
                );
                setRoutes(cachedData.routes);
              }
            } else {
              console.log(
                "[Routes] Filter disabled, showing all cached routes"
              );
              setRoutes(cachedData.routes);
            }

            setDropdownsLoading((prev) => ({ ...prev, routes: false }));
            loadingRoutes.current = false;
            return;
          }
        }

        console.log("[Routes] Loading from API for orgUID:", orgUID);
        // Load only routes
        const result = await journeyPlanDataLoader.loadRoutes(orgUID);

        setAllRoutes(result.data);

        // Apply filtering if enabled
        if (showAvailableRoutesOnly) {
          const visitDate = form.watch("visitDate");
          if (visitDate) {
            console.log(
              "[Routes] Applying filter to newly loaded routes for date:",
              visitDate
            );
            await filterAvailableRoutes(result.data);
          } else {
            console.log(
              "[Routes] No visit date yet, showing all routes until date is selected"
            );
            setRoutes(result.data);
          }
        } else {
          console.log("[Routes] Filter disabled, showing all routes");
          setRoutes(result.data);
        }

        // Cache for next time
        localStorage.setItem(
          cacheKey,
          JSON.stringify({
            routes: result.data,
            timestamp: Date.now()
          })
        );
      } catch (error) {
        console.error("Error loading routes:", error);
      } finally {
        setDropdownsLoading((prev) => ({ ...prev, routes: false }));
        loadingRoutes.current = false;
      }
    },
    [showAvailableRoutesOnly, form]
  );

  // Track last loaded employees org to prevent duplicate calls
  const lastLoadedEmployeesOrg = useRef<string | null>(null);

  // Load salesmen only (not all employees) - for admin/manager to assign
  const loadSalesmenOnDemand = async (orgUID: string) => {
    // Prevent duplicate calls for the same org
    if (lastLoadedEmployeesOrg.current === orgUID) {
      console.log("[Salesmen] Skipping duplicate load for orgUID:", orgUID);
      return;
    }

    lastLoadedEmployeesOrg.current = orgUID;
    setDropdownsLoading((prev) => ({ ...prev, employees: true }));
    try {
      console.log("[Salesmen] Loading salesmen only for orgUID:", orgUID);
      const result = await journeyPlanDataLoader.loadEmployees(orgUID);
      // Filter to only show salesmen (not managers/admins)
      const salesmenOnly = result.data.filter((emp: Employee) => {
        // Check if employee is a salesman based on role/position
        // This might need adjustment based on your actual role structure
        return (
          emp.JobPositionUID &&
          !emp.Name?.toLowerCase().includes("manager") &&
          !emp.Name?.toLowerCase().includes("admin")
        );
      });
      setSalesmen(salesmenOnly);
      console.log(
        `[Salesmen] Loaded ${salesmenOnly.length} salesmen out of ${result.data.length} employees`
      );
    } catch (error) {
      console.error("Error loading salesmen:", error);
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, employees: false }));
    }
  };

  // Track last loaded route-employee combination to prevent duplicate calls
  const lastLoadedRouteEmployees = useRef<string | null>(null);

  // Load salesmen assigned to specific route
  const loadSalesmenForRoute = async (orgUID: string, routeUID: string) => {
    const combinedKey = `${orgUID}_${routeUID}`;

    // Prevent duplicate calls for the same org-route combination
    if (lastLoadedRouteEmployees.current === combinedKey) {
      console.log("[Employees] Skipping duplicate load for route:", routeUID);
      return;
    }

    lastLoadedRouteEmployees.current = combinedKey;
    setDropdownsLoading((prev) => ({ ...prev, employees: true }));
    try {
      console.log("[Salesmen] Loading salesmen for route:", routeUID);

      // First get all salesmen for the org
      const result = await journeyPlanDataLoader.loadEmployees(orgUID);
      // Filter to salesmen only
      let salesmenList = result.data.filter((emp: Employee) => {
        return (
          emp.JobPositionUID &&
          !emp.Name?.toLowerCase().includes("manager") &&
          !emp.Name?.toLowerCase().includes("admin")
        );
      });

      console.log("All salesmen loaded:", salesmenList);

      // Filter salesmen based on route assignment
      const routeAssignedSalesmen = salesmenList.filter(
        (emp) => emp.RouteUID === routeUID
      );

      console.log("Salesmen assigned to route:", routeAssignedSalesmen);

      // Then try to get route details for RouteUserList
      try {
        const routeDetails = await api.route.getMasterByUID(routeUID);
        if (routeDetails?.IsSuccess && routeDetails?.Data) {
          const routeMaster = routeDetails.Data;
          console.log("Route master data:", routeMaster);

          // Priority 1: Check RouteUserList (multiple employees assigned to route)
          if (
            routeMaster.RouteUserList &&
            routeMaster.RouteUserList.length > 0
          ) {
            console.log("Route has RouteUserList:", routeMaster.RouteUserList);

            // Get all JobPositionUIDs from RouteUserList
            const assignedJobPositionUIDs = routeMaster.RouteUserList.map(
              (ru: any) => ru.JobPositionUID
            );

            // Filter salesmen to show ONLY those in RouteUserList
            const assignedSalesmen = salesmenList.filter((emp) =>
              assignedJobPositionUIDs.includes(emp.JobPositionUID)
            );

            console.log(
              `Found ${assignedSalesmen.length} salesmen assigned to this route`
            );

            // If we found assigned salesmen, show them
            if (assignedSalesmen.length > 0) {
              // Only show assigned salesmen for this route
              salesmenList = assignedSalesmen.map((emp) => ({
                ...emp,
                IsAssignedToRoute: true
              }));

              // Auto-select all assigned salesmen (supports multiple)
              if (assignedSalesmen.length >= 1) {
                const jobPositionUIDs = assignedSalesmen.map(
                  (s) => s.JobPositionUID
                );
                const loginIds = assignedSalesmen.map((s) => s.LoginId);
                form.setValue("selectedEmployees", jobPositionUIDs);
                form.setValue("loginIds", loginIds);

                // For backward compatibility, set single fields if only one
                if (assignedSalesmen.length === 1) {
                  const firstSalesman = assignedSalesmen[0];
                  form.setValue(
                    "selectedEmployee",
                    firstSalesman.JobPositionUID
                  );
                  form.setValue("loginId", firstSalesman.LoginId);
                  setSelectedSalesman(firstSalesman);
                }
                console.log(
                  `Auto-selected ${assignedSalesmen.length} salesmen for route`
                );
              }
            } else {
              console.warn(
                "No salesmen found matching RouteUserList JobPositionUIDs"
              );
              // Show all salesmen if none match route
              salesmenList = salesmenList.map((emp) => ({
                ...emp,
                IsAssignedToRoute: false
              }));
            }
          }
          // Priority 2: If no RouteUserList, check Route.JobPositionUID (single salesman)
          else if (routeMaster.Route?.JobPositionUID) {
            console.log(
              "Route has single JobPositionUID:",
              routeMaster.Route.JobPositionUID
            );

            // Find salesman matching the route's JobPositionUID
            const assignedSalesmen = salesmenList.filter(
              (emp) => emp.JobPositionUID === routeMaster.Route.JobPositionUID
            );

            if (assignedSalesmen.length > 0) {
              // Show only salesmen with matching JobPositionUID
              salesmenList = assignedSalesmen.map((emp) => ({
                ...emp,
                IsAssignedToRoute: true
              }));

              // Auto-select all assigned salesmen
              const jobPositionUIDs = assignedSalesmen.map(
                (s) => s.JobPositionUID
              );
              const loginIds = assignedSalesmen.map((s) => s.LoginId);
              form.setValue("selectedEmployees", jobPositionUIDs);
              form.setValue("loginIds", loginIds);

              // For backward compatibility
              if (assignedSalesmen.length === 1) {
                const firstSalesman = assignedSalesmen[0];
                form.setValue("selectedEmployee", firstSalesman.JobPositionUID);
                form.setValue("loginId", firstSalesman.LoginId);
                setSelectedSalesman(firstSalesman);
              }
              console.log(
                `Auto-selected ${assignedSalesmen.length} salesmen from Route.JobPositionUID`
              );
            } else {
              console.warn(
                "No salesman found with JobPositionUID:",
                routeMaster.Route.JobPositionUID
              );
              // Show all salesmen if none match
              salesmenList = salesmenList.map((emp) => ({
                ...emp,
                IsAssignedToRoute: false
              }));
            }
          }
          // Priority 3: Check if salesmen have RouteUID in their data
          else if (routeAssignedSalesmen.length > 0) {
            console.log("Using salesmen with RouteUID in their data");
            salesmenList = routeAssignedSalesmen.map((emp) => ({
              ...emp,
              IsAssignedToRoute: true
            }));

            // Auto-select all assigned salesmen
            const jobPositionUIDs = routeAssignedSalesmen.map(
              (s) => s.JobPositionUID
            );
            const loginIds = routeAssignedSalesmen.map((s) => s.LoginId);
            form.setValue("selectedEmployees", jobPositionUIDs);
            form.setValue("loginIds", loginIds);

            // For backward compatibility
            if (routeAssignedSalesmen.length === 1) {
              const firstSalesman = routeAssignedSalesmen[0];
              form.setValue("selectedEmployee", firstSalesman.JobPositionUID);
              form.setValue("loginId", firstSalesman.LoginId);
              setSelectedSalesman(firstSalesman);
            }
            console.log(
              `Auto-selected ${routeAssignedSalesmen.length} salesmen from RouteUID`
            );
          }
          // No specific assignment found
          else {
            console.log(
              "No specific route-salesman assignment found, showing all salesmen"
            );
            salesmenList = salesmenList.map((emp) => ({
              ...emp,
              IsAssignedToRoute: false
            }));
          }
        }
      } catch (error) {
        console.warn(
          "Could not get route details for employee filtering:",
          error
        );
      }

      setSalesmen(salesmenList);
      console.log("Salesmen loaded and filtered:", salesmenList.length);
    } catch (error) {
      console.error("Error loading employees for route:", error);
      setEmployees([]);
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, employees: false }));
    }
  };

  // Load vehicles on-demand
  const loadVehiclesOnDemand = async (orgUID: string) => {
    if (vehicles.length > 0) return; // Already loaded

    setDropdownsLoading((prev) => ({ ...prev, vehicles: true }));
    try {
      const result = await journeyPlanDataLoader.loadVehicles(orgUID);
      setVehicles(result.data);
    } catch (error) {
      console.error("Error loading vehicles:", error);
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, vehicles: false }));
    }
  };

  // Organization selection handler
  useEffect(() => {
    console.log("watchedOrgUID changed:", watchedOrgUID);
    if (watchedOrgUID && watchedOrgUID !== lastLoadedOrgUID.current) {
      console.log("Loading dependent dropdowns for:", watchedOrgUID);
      loadDependentDropdowns(watchedOrgUID);
    }
  }, [watchedOrgUID, loadDependentDropdowns]);

  // Track last loaded route to prevent duplicate calls
  const lastLoadedRouteUID = useRef<string | null>(null);
  const loadingRouteDetails = useRef(false);

  // Route selection handler
  useEffect(() => {
    const routeUID = form.watch("routeUID");
    const orgUID = form.watch("orgUID");
    console.log("Route selection changed:", routeUID);

    // Prevent duplicate calls for the same route
    if (routeUID === lastLoadedRouteUID.current && !routeUID) {
      return;
    }

    if (routeUID && orgUID && !loadingRouteDetails.current) {
      loadingRouteDetails.current = true;
      lastLoadedRouteUID.current = routeUID;

      // Load route details
      loadRouteDetails(routeUID).finally(() => {
        loadingRouteDetails.current = false;
      });
      // Load customers for this route and date
      loadRouteCustomers(routeUID);
      // Load salesmen for this org and filter by route
      loadSalesmenForRoute(orgUID, routeUID);
    } else if (orgUID && !routeUID) {
      // No route selected, load all salesmen for org
      loadSalesmenOnDemand(orgUID);
    }
  }, [form.watch("routeUID")]);

  // Track last filtered date to prevent duplicate filtering
  const lastFilteredDate = useRef<Date | null>(null);
  const filteringRoutes = useRef(false);

  // Date change handler - refresh available routes when date changes
  // Only trigger when in step 1 to avoid duplicate calls when moving to step 2
  useEffect(() => {
    const visitDate = form.watch("visitDate");
    const routeUID = form.watch("routeUID");
    console.log(
      "[DateChange] Visit date changed:",
      visitDate,
      "Current step:",
      currentStep
    );

    // Handle different behaviors based on current step
    if (currentStep === 1) {
      // Step 1: Filter available routes
      // Prevent duplicate filtering for the same date
      if (
        lastFilteredDate.current?.getTime() === visitDate?.getTime() ||
        filteringRoutes.current
      ) {
        console.log("[DateChange] Same date or already filtering, skipping");
        return;
      }

      if (showAvailableRoutesOnly && allRoutes.length > 0 && visitDate) {
        console.log("[DateChange] Refreshing available routes for new date");
        filteringRoutes.current = true;
        lastFilteredDate.current = visitDate;

        filterAvailableRoutes(allRoutes).finally(() => {
          filteringRoutes.current = false;
        });
      }
    } else if (currentStep === 2 && routeUID && visitDate) {
      // Step 2: Reload customers for the new date
      console.log("[DateChange] On step 2, reloading customers for new date");
      
      // Reset the last loaded customers to force reload
      lastLoadedCustomersRoute.current = null;
      
      // Reload customers for the selected date
      loadRouteCustomers(routeUID);
    }
  }, [form.watch("visitDate"), showAvailableRoutesOnly, currentStep]);

  // Optimized API Functions - Non-blocking
  const loadInitialDataOptimized = async () => {
    console.log("[InitialLoad] Starting initial data load");
    // Don't block UI, load in background
    setTimeout(async () => {
      setDropdownsLoading((prev) => ({ ...prev, organizations: true }));

      try {
        const startTime = Date.now();

        // Try to load from localStorage first for instant display
        const cachedOrgs = localStorage.getItem("journey_orgs");
        const cachedTypes = localStorage.getItem("journey_org_types");

        if (cachedOrgs && cachedTypes) {
          console.log("[InitialLoad] Using cached organization data");
          // Use cached data immediately
          const orgs = JSON.parse(cachedOrgs);
          const types = JSON.parse(cachedTypes);

          setOrgTypes(types);
          setOrganizations(orgs);

          const initialLevels = initializeOrganizationHierarchy(orgs, types);
          setOrgLevels(initialLevels);

          setDropdownsLoading((prev) => ({ ...prev, organizations: false }));

          // Then refresh in background
          journeyPlanDataLoader.loadInitialData().then((result) => {
            if (result.organizations.length > 0) {
              console.log("[InitialLoad] Background refresh completed");
              localStorage.setItem(
                "journey_orgs",
                JSON.stringify(result.organizations)
              );
              localStorage.setItem(
                "journey_org_types",
                JSON.stringify(result.orgTypes)
              );
            }
          });
        } else {
          console.log("[InitialLoad] Loading fresh organization data");
          // No cache, load fresh
          const result = await journeyPlanDataLoader.loadInitialData();

          setOrgTypes(result.orgTypes);
          setOrganizations(result.organizations);

          const initialLevels = initializeOrganizationHierarchy(
            result.organizations,
            result.orgTypes
          );
          setOrgLevels(initialLevels);

          // Save to localStorage for next time
          localStorage.setItem(
            "journey_orgs",
            JSON.stringify(result.organizations)
          );
          localStorage.setItem(
            "journey_org_types",
            JSON.stringify(result.orgTypes)
          );
          console.log(`[InitialLoad] Completed in ${Date.now() - startTime}ms`);
        }
      } catch (error) {
        console.error("[InitialLoad] Error loading initial data:", error);
        // Don't show error toast for background loading
      } finally {
        setDropdownsLoading((prev) => ({ ...prev, organizations: false }));
      }
    }, 100); // Small delay to let UI render first
  };

  // Organization hierarchy functions
  const handleOrganizationSelect = (levelIndex: number, value: string) => {
    if (!value) return;

    console.log(`[Organization] Selected at level ${levelIndex}:`, value);

    // Use utility function to handle organization selection
    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      orgLevels,
      selectedOrgs,
      organizations,
      orgTypes
    );

    console.log("[Organization] Updated org levels:", updatedLevels);
    console.log("[Organization] Updated selected orgs:", updatedSelectedOrgs);

    setOrgLevels(updatedLevels);
    setSelectedOrgs(updatedSelectedOrgs);

    // Get the final selected organization (leaf node)
    const finalOrgUID = getFinalSelectedOrganization(updatedSelectedOrgs);
    const finalOrg = finalOrgUID
      ? organizations.find((org) => org.UID === finalOrgUID)
      : undefined;

    console.log("[Organization] Final selected organization UID:", finalOrgUID);
    console.log("[Organization] Final selected organization object:", finalOrg);

    if (finalOrg) {
      console.log("[Organization] Setting orgUID in form:", finalOrg.UID);
      form.setValue("orgUID", finalOrg.UID);
      // Clear dependent dropdowns when org changes
      form.setValue("routeUID", "");
      form.setValue("selectedEmployees", []);
      form.setValue("loginIds", []);
      form.setValue("selectedEmployee", "");
      form.setValue("loginId", "");
      setSelectedSalesman(null);
      // Reset tracking refs when org changes
      lastLoadedOrgUID.current = null;
      lastLoadedRouteUID.current = null;
      lastLoadedCustomersRoute.current = null;
      lastLoadedEmployeesOrg.current = null;
      lastLoadedRouteEmployees.current = null;
      scheduleTimesSet.current = false;
      // Clear dropdowns
      setRoutes([]);
      setSalesmen([]);
      setSelectedSalesman(null);
    } else {
      console.log(
        "[Organization] No final organization selected yet - hierarchy incomplete"
      );
    }
  };

  const loadRoutes = async (orgUID: string) => {
    setDropdownsLoading((prev) => ({ ...prev, routes: true }));
    try {
      console.log("[LoadRoutes] Loading AVAILABLE routes for orgUID:", orgUID);

      // For Journey Plan creation, we want routes that are:
      // 1. Active
      // 2. Not already planned for the selected date (if date is selected)
      // 3. Have available employees

      const visitDate = form.watch("visitDate");
      console.log(
        "[LoadRoutes] Current visit date:",
        visitDate,
        "Step:",
        currentStep
      );

      // Use dropdown API which should give us active routes for the org
      let response;
      try {
        response = await api.dropdown.getRoute(orgUID);
        console.log("[LoadRoutes] Available routes response:", response);
      } catch (dropdownError) {
        console.warn(
          "[LoadRoutes] Dropdown route API failed, trying getUserDDL:",
          dropdownError
        );
        response = await api.route.getUserDDL(orgUID);
        console.log("[LoadRoutes] getUserDDL fallback response:", response);
      }

      if (response?.IsSuccess && response?.Data) {
        // Routes are returned as ISelectionItem from dropdown API
        // The database query already filters by org_uid, so all returned routes are valid
        let allRoutesData = response.Data.filter((route: any) => {
          // Basic validation - ensure we have required fields
          return route.UID && route.Label;
        });

        console.log("[LoadRoutes] All routes loaded:", allRoutesData);
        setAllRoutes(allRoutesData);

        // Filter routes based on availability (no existing journey plans for selected date)
        // Only filter if we're on step 1 to avoid duplicate filtering
        if (showAvailableRoutesOnly && currentStep === 1) {
          console.log("[LoadRoutes] Filtering routes for availability");
          await filterAvailableRoutes(allRoutesData);
        } else {
          console.log("[LoadRoutes] Setting routes without filtering");
          setRoutes(allRoutesData);
        }
      } else {
        console.warn("No route data received:", response);
        setRoutes([]);
      }
    } catch (error) {
      console.error("Error loading routes:", error);
      setRoutes([]);
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, routes: false }));
    }
  };

  const loadSalesmen = async (orgUID: string) => {
    setDropdownsLoading((prev) => ({ ...prev, employees: true }));
    try {
      console.log("Loading salesmen for orgUID:", orgUID);

      // Try dropdown API first for better filtering
      let response;
      try {
        response = await api.dropdown.getEmployee(orgUID);
        console.log("Employee dropdown response:", response);
      } catch (dropdownError) {
        console.warn(
          "Dropdown employee API failed, trying jobPosition.getByUID:",
          dropdownError
        );
        response = await api.jobPosition.getByUID(orgUID);
        console.log("JobPosition fallback response:", response);
      }

      if (response?.IsSuccess && response?.Data) {
        // Filter to get only salesmen (not managers/admins)
        const salesmenData = response.Data.filter((emp: any) => {
          // Basic validation and role check
          return (
            emp.UID &&
            emp.Label &&
            !emp.Label?.toLowerCase().includes("manager") &&
            !emp.Label?.toLowerCase().includes("admin")
          );
        }).map((emp: any) => ({
          JobPositionUID: emp.UID,
          LoginId: emp.Code,
          Name: emp.Label,
          Mobile: emp.ExtData?.Mobile || "",
          Status: "Active",
          RouteUID: emp.ExtData?.RouteUID
        }));

        console.log("Filtered salesmen:", salesmenData);
        setSalesmen(salesmenData);
      }
    } catch (error) {
      console.error("Error loading salesmen:", error);
      setSalesmen([]);
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, employees: false }));
    }
  };

  const loadVehicles = async (orgUID: string) => {
    setDropdownsLoading((prev) => ({ ...prev, vehicles: true }));
    try {
      // Use the correct dropdown API endpoint for vehicles
      const response = await api.dropdown.getVehicle(orgUID);
      if (response?.IsSuccess && response?.Data) {
        // Vehicles are returned as ISelectionItem from dropdown API
        const vehicleData = response.Data.filter(
          (vehicle: any) => vehicle.UID && vehicle.Label
        ).map((vehicle: any) => ({
          UID: vehicle.UID,
          Name: vehicle.Label, // Label contains formatted vehicle info
          Code: vehicle.Code,
          VehicleNo: vehicle.ExtData?.VehicleNo || "",
          RegistrationNo: vehicle.ExtData?.RegistrationNo || ""
        }));
        setVehicles(vehicleData);
      }
    } catch (error) {
      console.error("Error loading vehicles:", error);
      setVehicles([]);
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, vehicles: false }));
    }
  };

  const loadLocations = async (orgUID: string) => {
    setDropdownsLoading((prev) => ({ ...prev, locations: true }));
    try {
      console.log("Loading locations for orgUID:", orgUID);

      // Use the correct location API - it expects array of location types
      const response = await api.location.getByTypes([
        "Region",
        "Area",
        "Territory"
      ]);
      console.log("Location response:", response);

      if (response?.IsSuccess && response?.Data) {
        // Locations might be returned as ILocation objects or simple objects
        const locationData = Array.isArray(response.Data)
          ? response.Data.map((loc: any) => ({
              UID: loc.UID,
              Name: loc.Name || loc.LocationName,
              Code: loc.Code || loc.LocationCode,
              Type: loc.Type || loc.LocationType
            }))
          : [];
        setLocations(locationData);
        console.log("Locations loaded:", locationData);
      } else {
        console.warn("No location data received:", response);
        setLocations([]);
      }
    } catch (error) {
      console.error("Error loading locations:", error);
      setLocations([]); // Set empty array on error instead of leaving undefined
      // Locations are optional for journey plans, so don't show error to user
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, locations: false }));
    }
  };

  const loadRouteDetails = async (routeUID: string) => {
    try {
      console.log("[RouteDetails] Loading for routeUID:", routeUID);
      const masterData = await api.route.getMasterByUID(routeUID);
      if (masterData?.IsSuccess && masterData?.Data) {
        const routeMaster = masterData.Data;
        console.log("[RouteDetails] Route master data loaded:", routeMaster);

        // Extract RouteSchedule if available
        if (routeMaster.RouteSchedule) {
          console.log("RouteSchedule found:", routeMaster.RouteSchedule);
          setSelectedSchedule(routeMaster.RouteSchedule);
          form.setValue("routeScheduleUID", routeMaster.RouteSchedule.UID);

          // Auto-populate timing from schedule if available, otherwise use business hours
          if (routeMaster.RouteSchedule.StartTime) {
            const startTime = formatTimeFromTimeSpan(
              routeMaster.RouteSchedule.StartTime
            );
            form.setValue("plannedStartTime", startTime);
            scheduleTimesSet.current = true;
            console.log(
              "[RouteDetails] Set plannedStartTime from schedule:",
              startTime
            );
          }
          if (routeMaster.RouteSchedule.EndTime) {
            const endTime = formatTimeFromTimeSpan(
              routeMaster.RouteSchedule.EndTime
            );
            form.setValue("plannedEndTime", endTime);
            scheduleTimesSet.current = true;
            console.log(
              "[RouteDetails] Set plannedEndTime from schedule:",
              endTime
            );
          }
          // If no schedule times, keep default business hours (00:00-00:00)
          if (routeMaster.RouteSchedule.VisitDurationInMinutes) {
            form.setValue(
              "defaultDuration",
              routeMaster.RouteSchedule.VisitDurationInMinutes
            );
            console.log(
              "[RouteDetails] Set defaultDuration from schedule:",
              routeMaster.RouteSchedule.VisitDurationInMinutes
            );
          }
          if (routeMaster.RouteSchedule.TravelTimeInMinutes) {
            form.setValue(
              "defaultTravelTime",
              routeMaster.RouteSchedule.TravelTimeInMinutes
            );
            console.log(
              "[RouteDetails] Set defaultTravelTime from schedule:",
              routeMaster.RouteSchedule.TravelTimeInMinutes
            );
          }
        } else {
          console.log("[RouteDetails] No RouteSchedule found for this route");
          setSelectedSchedule(null);
          // Keep default business hours if no schedule
          if (!scheduleTimesSet.current) {
            console.log(
              "[RouteDetails] No schedule, keeping default business hours (00:00-00:00)"
            );
          }
        }

        // Auto-select salesman if route has default
        if (routeMaster.Route?.JobPositionUID) {
          const salesman = salesmen.find(
            (s) => s.JobPositionUID === routeMaster.Route.JobPositionUID
          );
          if (salesman) {
            form.setValue("selectedEmployee", salesman.JobPositionUID);
            form.setValue("loginId", salesman.LoginId);
            setSelectedSalesman(salesman);
            console.log(
              "[RouteDetails] Auto-selected salesman:",
              salesman.Name
            );
          }
        }
      }
    } catch (error) {
      console.error("Error loading route details:", error);
    }
  };

  // Helper function to format TimeSpan to HH:mm format
  const formatTimeFromTimeSpan = (timeSpan: string | any): string => {
    if (typeof timeSpan === "string") {
      // Handle format like "08:00:00" or "08:00"
      return timeSpan.substring(0, 5);
    }
    if (timeSpan && typeof timeSpan === "object") {
      // Handle TimeSpan object format - default to 0 if not found
      const hours = String(timeSpan.Hours || timeSpan.hours || 0).padStart(
        2,
        "0"
      );
      const minutes = String(
        timeSpan.Minutes || timeSpan.minutes || 0
      ).padStart(2, "0");
      return `${hours}:${minutes}`;
    }
    return "00:00"; // Default fallback to business hours start
  };

  // Debounce timer for filter operations
  const filterDebounceTimer = useRef<NodeJS.Timeout | null>(null);

  const filterAvailableRoutes = async (allRoutesData: any[]) => {
    // Clear any existing debounce timer
    if (filterDebounceTimer.current) {
      clearTimeout(filterDebounceTimer.current);
    }

    // Set up new debounce
    return new Promise<void>((resolve) => {
      filterDebounceTimer.current = setTimeout(async () => {
        console.log(
          "[Filter] Filtering available routes for date:",
          form.watch("visitDate")
        );
        const visitDate = form.watch("visitDate");
        const orgUID = form.watch("orgUID");
        
        // Validate required parameters before API call
        if (!visitDate) {
          console.error("[Filter] Visit date is required for filtering");
          setRoutes(allRoutesData);
          resolve();
          return;
        }
        
        if (!orgUID) {
          console.error("[Filter] Organization UID is required for filtering");
          setRoutes(allRoutesData);
          resolve();
          return;
        }
        
        const selectedDate = moment(visitDate).format("YYYY-MM-DD");

        try {
          console.log("[Filter] Making API call with:", { selectedDate, orgUID });
          
          // Check existing journey plans for the selected date
          let existingPlansResponse;
          try {
            existingPlansResponse = await api.beatHistory.selectAll({
              pageNumber: 1,
              pageSize: 1000, // Get all existing plans
              isCountRequired: false,
              sortCriterias: [],
              filterCriterias: [
                {
                  filterBy: "VisitDate",
                  filterValue: selectedDate,
                  filterOperator: "equals"
                },
                {
                  filterBy: "OrgUID",
                  filterValue: orgUID,
                  filterOperator: "equals"
                }
              ]
            });
          } catch (apiError: any) {
            console.error("[Filter] BeatHistory API call failed:", apiError);
            
            // If the API fails, we'll disable filtering and show all routes
            // This is better than crashing the entire component
            console.log("[Filter] API failed, falling back to showing all routes");
            setRoutes(allRoutesData);
            
            toast({
              title: "Route filtering unavailable",
              description: "Unable to check existing journey plans. Showing all routes.",
              variant: "default"
            });
            
            resolve();
            return;
          }

          console.log(
            "Existing journey plans response:",
            existingPlansResponse
          );

          let routesWithExistingPlans: string[] = [];

          // Check if the response is successful and has data
          if (existingPlansResponse && typeof existingPlansResponse === 'object') {
            // Handle different response formats
            if (existingPlansResponse.IsSuccess && existingPlansResponse.Data?.PagedData) {
              routesWithExistingPlans = existingPlansResponse.Data.PagedData
                .filter((plan: any) => plan && plan.RouteUID)
                .map((plan: any) => plan.RouteUID);
              console.log(
                "Routes with existing journey plans:",
                routesWithExistingPlans
              );
            } else if (Array.isArray(existingPlansResponse)) {
              // Handle direct array response
              routesWithExistingPlans = existingPlansResponse
                .filter((plan: any) => plan && plan.RouteUID)
                .map((plan: any) => plan.RouteUID);
            } else {
              console.log("[Filter] No existing plans found or unexpected response format");
            }
          } else {
            console.log("[Filter] Invalid response format from API");
          }

          // Store routes with existing plans for reference
          setRoutesWithExistingPlans(routesWithExistingPlans);

          // Filter out routes that already have journey plans
          const availableRoutes = allRoutesData.filter(
            (route) => !routesWithExistingPlans.includes(route.UID)
          );

          const plannedRoutes = allRoutesData.filter((route) =>
            routesWithExistingPlans.includes(route.UID)
          );

          console.log("Available routes (no existing plans):", availableRoutes);
          console.log("Routes with existing plans:", plannedRoutes);
          console.log(
            `Available: ${availableRoutes.length}, Planned: ${plannedRoutes.length}, Total: ${allRoutesData.length}`
          );

          setRoutes(availableRoutes);

          // Show info message about filtering
          if (availableRoutes.length < allRoutesData.length) {
            const filteredCount = allRoutesData.length - availableRoutes.length;
            toast({
              title: "Routes Filtered",
              description: `Showing ${
                availableRoutes.length
              } available routes (${filteredCount} routes already have journey plans for ${moment(
                visitDate
              ).format("DD/MMM/YYYY")})`,
              variant: "default"
            });
          }
        } catch (error) {
          console.error("Error filtering available routes:", error);
          // Fallback to showing all routes if filtering fails
          setRoutes(allRoutesData);
          toast({
            title: "Warning",
            description:
              "Could not check existing journey plans. Showing all routes.",
            variant: "destructive"
          });
        } finally {
          resolve();
        }
      }, 300); // 300ms debounce delay
    });
  };

  const toggleRouteFilter = async () => {
    const newValue = !showAvailableRoutesOnly;
    setShowAvailableRoutesOnly(newValue);

    console.log(
      `[Filter] Toggling route filter to: ${
        newValue ? "available only" : "all routes"
      }`
    );

    if (allRoutes.length > 0) {
      if (newValue) {
        // Show available routes only
        await filterAvailableRoutes(allRoutes);
      } else {
        // Show all routes with indicators
        const routesWithIndicators = allRoutes.map((route) => ({
          ...route,
          hasExistingPlan: routesWithExistingPlans.includes(route.UID)
        }));
        setRoutes(routesWithIndicators);

        const plannedCount = routesWithExistingPlans.length;
        const availableCount = allRoutes.length - plannedCount;

        toast({
          title: "Showing All Routes",
          description: `${allRoutes.length} total routes: ${availableCount} available, ${plannedCount} with existing plans`,
          variant: "default"
        });
      }
    }
  };

  // Track last loaded customers to prevent duplicate calls
  const lastLoadedCustomersRoute = useRef<string | null>(null);

  const loadRouteCustomers = async (routeUID: string) => {
    // Get the selected visit date
    const visitDate = form.watch("visitDate");
    
    // Create a unique key for this route+date combination
    const cacheKey = `${routeUID}_${visitDate ? moment(visitDate).format("YYYY-MM-DD") : 'all'}`;
    
    // Prevent duplicate calls for the same route+date combination
    if (lastLoadedCustomersRoute.current === cacheKey) {
      console.log(
        "[Customers] Skipping duplicate load for:",
        cacheKey
      );
      return;
    }

    lastLoadedCustomersRoute.current = cacheKey;
    setDropdownsLoading((prev) => ({ ...prev, customers: true }));
    try {
      console.log("[Customers] Loading for routeUID:", routeUID, "date:", visitDate);
      
      // Use the updated data loader that supports date-based queries
      const result = await journeyPlanDataLoader.loadRouteCustomers(
        routeUID, 
        visitDate || undefined
      );

      console.log(`[Customers] Loaded ${result.data.length} customers in ${result.loadTime}ms`);
      
      // Check if these are flexible schedule customers or standard
      const isFlexibleSchedule = result.data.some((c: any) => c.ScheduleType === 'FLEXIBLE');
      
      setRouteCustomers(result.data);

      if (result.fromCache) {
        console.log("[Customers] Data loaded from cache");
      }

      // Show appropriate feedback based on the type of data loaded
      if (visitDate && isFlexibleSchedule) {
        if (result.data.length === 0) {
          toast({
            title: "No customers scheduled",
            description: `No customers are scheduled for ${moment(visitDate).format("DD/MMM/YYYY")}`,
            variant: "default"
          });
        } else {
          toast({
            title: "Customers loaded",
            description: `${result.data.length} customers scheduled for ${moment(visitDate).format("DD/MMM/YYYY")}`,
            duration: 2000
          });
        }
      } else if (result.data.length > 0) {
        // Standard customers loaded
        if (result.loadTime > 1500) {
          toast({
            title: "Customers loaded",
            description: `${result.data.length} customers found${visitDate ? ' (showing all route customers)' : ''}`,
            duration: 2000
          });
        }
      } else {
        toast({
          title: "No customers found",
          description: "This route has no customers assigned",
          variant: "default"
        });
      }
    } catch (error) {
      console.error("Error loading customers:", error);
      setRouteCustomers([]);
      toast({
        title: "Error",
        description: "Failed to load customers",
        variant: "destructive"
      });
    } finally {
      setDropdownsLoading((prev) => ({ ...prev, customers: false }));
    }
  };

  // Step Navigation
  const handleNext = async () => {
    let fieldsToValidate: (keyof JourneyPlanFormData)[] = [];

    switch (currentStep) {
      case 1:
        fieldsToValidate = ["orgUID", "routeUID", "selectedEmployees"];
        break;
      case 2:
        fieldsToValidate = [
          "visitDate",
          "plannedStartTime",
          "plannedEndTime",
          "selectedCustomersWithTimes"
        ];
        // Validate time range
        const startTime = form.getValues("plannedStartTime");
        const endTime = form.getValues("plannedEndTime");
        if (startTime >= endTime) {
          toast({
            title: "Invalid Time Range",
            description: "Planned end time must be after start time",
            variant: "destructive"
          });
          return;
        }
        break;
    }

    const isValid = await form.trigger(fieldsToValidate);
    if (isValid) {
      console.log(
        `[Navigation] Moving from step ${currentStep} to step ${
          currentStep + 1
        }`
      );
      // Clear any pending route filtering when moving to step 2
      if (currentStep === 1) {
        if (filterDebounceTimer.current) {
          clearTimeout(filterDebounceTimer.current);
          filterDebounceTimer.current = null;
        }
      }
      setCurrentStep((prev) => Math.min(prev + 1, progressSteps.length));
    }
  };

  const handlePrevious = () => {
    console.log(
      `[Navigation] Moving from step ${currentStep} to step ${currentStep - 1}`
    );
    setCurrentStep((prev) => Math.max(prev - 1, 1));
  };

  // Customer Selection Functions
  const calculateCustomerTimes = () => {
    const plannedStartTime = form.getValues("plannedStartTime") || "00:00";
    const defaultDuration = form.getValues("defaultDuration") || 30;
    const defaultTravelTime = form.getValues("defaultTravelTime") || 15;

    console.log("[CustomerTimes] Calculating with:", {
      plannedStartTime,
      defaultDuration,
      defaultTravelTime,
      customersCount: selectedCustomersWithTimes.length
    });

    let currentTime = plannedStartTime;
    const updatedCustomers = selectedCustomersWithTimes.map(
      (customer, index) => {
        const startTime = currentTime;
        const endTimeMinutes =
          parseInt(startTime.split(":")[0]) * 60 +
          parseInt(startTime.split(":")[1]) +
          defaultDuration;
        const endHours = Math.floor(endTimeMinutes / 60);
        const endMins = endTimeMinutes % 60;
        const endTime = `${endHours.toString().padStart(2, "0")}:${endMins
          .toString()
          .padStart(2, "0")}`;

        if (index < selectedCustomersWithTimes.length - 1) {
          const nextStartMinutes = endTimeMinutes + defaultTravelTime;
          const nextStartHours = Math.floor(nextStartMinutes / 60);
          const nextStartMins = nextStartMinutes % 60;
          currentTime = `${nextStartHours
            .toString()
            .padStart(2, "0")}:${nextStartMins.toString().padStart(2, "0")}`;
        }

        return {
          ...customer,
          startTime,
          endTime,
          visitDuration: defaultDuration
        };
      }
    );

    setSelectedCustomersWithTimes(updatedCustomers);
    form.setValue(
      "selectedCustomersWithTimes",
      updatedCustomers.map((c) => ({
        customerUID: c.UID,
        startTime: c.startTime,
        endTime: c.endTime,
        visitDuration: c.visitDuration
      }))
    );
  };

  const toggleCustomerSelection = (customerUID: string) => {
    const customer = routeCustomers.find((c) => c.UID === customerUID);
    if (!customer) return;

    const isSelected = selectedCustomersWithTimes.some(
      (c) => c.UID === customerUID
    );

    if (isSelected) {
      const updatedCustomers = selectedCustomersWithTimes.filter(
        (c) => c.UID !== customerUID
      );
      setSelectedCustomersWithTimes(updatedCustomers);
      form.setValue(
        "selectedCustomersWithTimes",
        updatedCustomers.map((c) => ({
          customerUID: c.UID,
          startTime: c.startTime,
          endTime: c.endTime,
          visitDuration: c.visitDuration
        }))
      );
    } else {
      const plannedStartTime = form.getValues("plannedStartTime") || "00:00";
      const defaultDuration = form.getValues("defaultDuration") || 30;

      const newCustomer: CustomerWithTime = {
        ...customer,
        startTime: plannedStartTime,
        endTime: plannedStartTime, // Will be calculated properly
        visitDuration: defaultDuration
      };

      const updatedCustomers = [...selectedCustomersWithTimes, newCustomer];
      setSelectedCustomersWithTimes(updatedCustomers);
      form.setValue(
        "selectedCustomersWithTimes",
        updatedCustomers.map((c) => ({
          customerUID: c.UID,
          startTime: c.startTime,
          endTime: c.endTime,
          visitDuration: c.visitDuration
        }))
      );

      // Recalculate times after adding
      setTimeout(calculateCustomerTimes, 100);
    }
  };

  // Track submission state to prevent duplicate submissions
  const isSubmitting = useRef(false);

  // Form Submission
  const onSubmit = async (values: JourneyPlanFormData) => {
    console.log("[Submit] onSubmit called, currentStep:", currentStep);

    // Only allow submission from step 3 (Review step)
    if (currentStep !== 3) {
      console.log("[Submit] Not on review step, preventing submission");
      toast({
        title: "Please complete all steps",
        description:
          "Navigate to the review step before creating the journey plan.",
        variant: "destructive"
      });
      return;
    }

    // Prevent duplicate submissions
    if (isSubmitting.current) {
      console.log("[Submit] Submission already in progress, skipping");
      return;
    }

    // Validate required data
    if (
      !values.selectedCustomersWithTimes ||
      values.selectedCustomersWithTimes.length === 0
    ) {
      console.log("[Submit] No customers selected, preventing submission");
      toast({
        title: "No customers selected",
        description:
          "Please select at least one customer before creating the journey plan.",
        variant: "destructive"
      });
      return;
    }

    isSubmitting.current = true;
    setLoading(true);
    try {
      console.log("[Submit] Starting journey plan creation with values:", {
        visitDate: values.visitDate,
        plannedStartTime: values.plannedStartTime,
        plannedEndTime: values.plannedEndTime,
        selectedEmployee: values.selectedEmployee,
        customersCount: values.selectedCustomersWithTimes?.length || 0,
        scheduleType: values.scheduleType,
        approvalStatus: values.approvalStatus
      });
      const currentUser = authService.getCurrentUser();
      console.log("[Submit] Current logged-in user:", currentUser);
      const yearMonth = parseInt(moment(values.visitDate).format("YYMM"));

      // Get selected employees (supports multiple selection)
      const selectedEmployeeUIDs = values.selectedEmployees || [];
      const selectedLoginIds = values.loginIds || [];

      // Check if we have multiple employees selected
      const isMultipleEmployees = selectedEmployeeUIDs.length > 1;

      // If no employees selected through multi-select, check single fields for backward compatibility
      if (selectedEmployeeUIDs.length === 0 && values.selectedEmployee) {
        selectedEmployeeUIDs.push(values.selectedEmployee);
        selectedLoginIds.push(values.loginId);
      }

      if (selectedEmployeeUIDs.length === 0) {
        throw new Error("No employee selected for journey plan assignment");
      }

      console.log(
        `[Submit] Creating journey plans for ${selectedEmployeeUIDs.length} employees`
      );

      // If multiple employees, use bulk create API
      if (isMultipleEmployees) {
        const bulkPlans = [];

        for (let i = 0; i < selectedEmployeeUIDs.length; i++) {
          const employeeUID = selectedEmployeeUIDs[i];
          const salesman = salesmen.find(
            (s) => s.JobPositionUID === employeeUID
          );

          if (!salesman) {
            console.warn(
              `[Submit] Salesman not found for JobPositionUID: ${employeeUID}`
            );
            continue;
          }

          let actualLoginId = salesman.LoginId;
          if (actualLoginId?.startsWith("[") && actualLoginId.includes("]")) {
            actualLoginId = actualLoginId.substring(
              1,
              actualLoginId.indexOf("]")
            );
          }

          if (!actualLoginId) {
            console.warn(
              `[Submit] Salesman ${salesman.Name} has no valid LoginId`
            );
            continue;
          }

          bulkPlans.push({
            OrgUID: values.orgUID,
            RouteUID: values.routeUID,
            RouteScheduleUID: values.routeScheduleUID || values.routeUID,
            SalesmanLoginId: actualLoginId,
            SalesmanJobPositionUID: employeeUID,
            VisitDate: moment(values.visitDate).format("YYYY-MM-DD"),
            PlannedStartTime: values.plannedStartTime || "00:00",
            PlannedEndTime: values.plannedEndTime || "00:00",
            PlannedStores: values.selectedCustomersWithTimes.map(
              (customer) => ({
                StoreUID: customer.customerUID,
                PlannedStartTime: customer.startTime || "00:00",
                PlannedEndTime: customer.endTime || "00:00",
                PlannedDuration: customer.visitDuration || 30
              })
            ),
            TargetCollectionAmount: values.targetCollectionAmount || 0,
            MinimumProductiveStores: values.minimumProductiveStores || 5,
            MaxSkipAllowed: values.maxSkipAllowed || 2,
            PriorityLevel: values.priorityLevel || "medium",
            SpecialInstructions: values.specialInstructions || "",
            ScheduleType: values.scheduleType || "Daily",
            ApprovalStatus: values.approvalStatus || "approved",
            Notes: values.notes || ""
          });
        }

        if (bulkPlans.length === 0) {
          throw new Error("No valid employees found for journey plan creation");
        }

        console.log(`[Submit] Creating ${bulkPlans.length} journey plans`);

        // Create each journey plan using the regular BeatHistory API
        let successCount = 0;
        let failedEmployees = [];

        for (const plan of bulkPlans) {
          try {
            const beatHistoryData = {
              // Required identifiers - Add timestamp to prevent duplicates
              UID: `BH_${plan.RouteUID}_${moment(plan.VisitDate).format(
                "YYYYMMDD"
              )}_${plan.SalesmanJobPositionUID}_${Date.now()}`,
              OrgUID: plan.OrgUID,
              RouteUID: plan.RouteUID,
              JobPositionUID: plan.SalesmanJobPositionUID,
              DefaultJobPositionUID: plan.SalesmanJobPositionUID,
              VisitDate: plan.VisitDate,
              YearMonth: parseInt(moment(plan.VisitDate).format("YYMM")),

              // PLANNING FIELDS ONLY
              // Default to "00:00" if not set by user
              PlannedStartTime: plan.PlannedStartTime || "00:00",
              PlannedEndTime: plan.PlannedEndTime || "00:00",
              PlannedStoreVisits: plan.PlannedStores?.length || 0,

              // Optional planning fields - only if provided
              ...(plan.LocationUID && { LocationUID: plan.LocationUID }),
              ...(plan.RouteWHOrgUID && { RouteWHOrgUID: plan.RouteWHOrgUID }),
              ...(plan.VehicleUID && {
                UserJourneyVehicleUID: plan.VehicleUID
              }),

              // Store all planning data in Notes as structured JSON
              Notes: JSON.stringify({
                userNotes: plan.Notes || "",
                scheduleType: plan.ScheduleType || "Daily",
                approvalStatus: plan.ApprovalStatus || "approved",
                routeScheduleUID: plan.RouteScheduleUID || "",
                specialInstructions: plan.SpecialInstructions || "",

                // Enhanced optional planning fields - PLANNING ONLY
                emergencyContact: plan.EmergencyContact || "",

                createdBy: "ADMIN",
                creationTimestamp: new Date().toISOString(),
                planType: "JOURNEY_PLAN_BULK"
              }),

              // Metadata - required by backend
              CreatedBy: createdByCode,
              ModifiedBy: createdByCode,
              CreatedTime: new Date(),
              ModifiedTime: new Date(),
              ServerAddTime: new Date(),
              ServerModifiedTime: new Date()
            };

            const response = await api.beatHistory.create(beatHistoryData);

            if (response?.IsSuccess) {
              successCount++;

              // Create store histories
              if (plan.PlannedStores?.length > 0) {
                const storeHistories = plan.PlannedStores.map(
                  (store, index) => ({
                    // Required identifiers
                    UID: `SH_${beatHistoryData.UID}_${
                      store.StoreUID
                    }_${Date.now()}_${index}`,
                    BeatHistoryUID: beatHistoryData.UID,
                    OrgUID: plan.OrgUID,
                    RouteUID: plan.RouteUID,
                    StoreUID: store.StoreUID,
                    YearMonth: parseInt(moment(plan.VisitDate).format("YYMM")),

                    // PLANNING FIELDS ONLY
                    IsPlanned: true,
                    SerialNo: index + 1,
                    Status: "Pending",
                    PlannedLoginTime: store.PlannedStartTime || "00:00",
                    PlannedLogoutTime: store.PlannedEndTime || "00:00",
                    PlannedTimeSpendInMinutes: store.PlannedDuration || 30,

                    // Metadata - required by backend
                    CreatedBy: createdByCode,
                    ModifiedBy: createdByCode,
                    CreatedTime: new Date(),
                    ModifiedTime: new Date(),
                    ServerAddTime: new Date(),
                    ServerModifiedTime: new Date()
                  })
                );

                // Store histories should be created automatically by the backend when BeatHistory is created
                console.log(`[Submit] BeatHistory created successfully. Store histories should be handled by backend.`);
              }
            } else {
              // Check for duplicate key error in bulk creation
              if (
                response?.Message &&
                response.Message.includes("duplicate key")
              ) {
                console.warn(
                  `Journey plan already exists for ${plan.SalesmanLoginId} on ${plan.VisitDate}`
                );
                failedEmployees.push(
                  `${plan.SalesmanLoginId} (already exists)`
                );
              } else {
                failedEmployees.push(plan.SalesmanLoginId);
              }
            }
          } catch (error) {
            console.error(
              `Failed to create plan for ${plan.SalesmanLoginId}:`,
              error
            );
            failedEmployees.push(plan.SalesmanLoginId);
          }
        }

        if (successCount > 0) {
          toast({
            title: "Success",
            description: `Created ${successCount} journey plans successfully!${
              failedEmployees.length > 0
                ? ` (${failedEmployees.length} failed)`
                : ""
            }`
          });
          router.push(
            "/updatedfeatures/journey-plan-management/journey-plans/manage"
          );
        } else {
          throw new Error("Failed to create any journey plans");
        }
      } else {
        // Single employee - use existing logic
        const selectedSalesmanUID = selectedEmployeeUIDs[0];
        const selectedLoginId = selectedLoginIds[0];

        const salesman = salesmen.find(
          (s) => s.JobPositionUID === selectedSalesmanUID
        );
        if (!salesman) {
          throw new Error("Selected salesman not found");
        }

        try {
          // Extract the actual LoginId from the salesman object
          let actualLoginId = salesman.LoginId || selectedLoginId;

          // If LoginId looks like "[TB01003] Name", extract just the ID part
          if (actualLoginId.startsWith("[") && actualLoginId.includes("]")) {
            actualLoginId = actualLoginId.substring(
              1,
              actualLoginId.indexOf("]")
            );
          }

          // Validate salesman has a valid LoginId for database foreign key constraint
          if (!actualLoginId) {
            throw new Error(`Salesman ${salesman.Name} has no valid LoginId`);
          }

          // Note: Existence check is now handled by the admin API endpoint
          // The API will return a conflict if plan already exists

          console.log(
            `[Submit] Creating journey plan for salesman: ${salesman.Name} (LoginId: ${actualLoginId})`
          );

          // Use schedule configuration from form
          const scheduleType = values.scheduleType || "Daily";
          const routeScheduleUID = values.routeScheduleUID || values.routeUID;

          console.log(
            `[Submit] Using schedule: type=${scheduleType}, routeScheduleUID=${routeScheduleUID}`
          );

          // Generate unique journey UID for this plan
          const beatHistoryUID = `BH_${values.routeUID}_${moment(
            values.visitDate
          ).format("YYYYMMDD")}_${selectedSalesmanUID}`;

          // Determine the correct CreatedBy value
          // The foreign key constraint requires CreatedBy to be a valid employee code
          let createdByCode = "ADMIN"; // Default fallback

          if (currentUser) {
            // Try to use the current user's code or uid
            if (currentUser.uid && currentUser.uid !== "") {
              createdByCode = currentUser.uid;
            } else if (currentUser.loginId && currentUser.loginId !== "") {
              // If loginId is like "admin", we need to convert to "ADMIN" (the employee code)
              createdByCode = currentUser.loginId.toUpperCase();
            }
          }

          console.log(
            `[Submit] Using CreatedBy: ${createdByCode} for journey plan`
          );

          // Create journey plan using BeatHistory API
          // FIXED: Only send PLANNING data - Backend handles all defaults
          const beatHistoryData = {
            // Required identifiers for journey PLAN
            UID: `BH_${values.routeUID}_${moment(values.visitDate).format(
              "YYYYMMDD"
            )}_${selectedSalesmanUID}_${Date.now()}`,
            OrgUID: values.orgUID,
            RouteUID: values.routeUID,
            JobPositionUID: selectedSalesmanUID, // Keep JobPositionUID for who will execute
            DefaultJobPositionUID: selectedSalesmanUID,
            VisitDate: moment(values.visitDate).format("YYYY-MM-DD"),
            YearMonth: yearMonth,

            // PLANNING FIELDS ONLY - Set by Admin/Manager during creation
            // Default to "00:00" if not set by user
            PlannedStartTime: values.plannedStartTime || "00:00",
            PlannedEndTime: values.plannedEndTime || "00:00",
            PlannedStoreVisits: values.selectedCustomersWithTimes?.length || 0,

            // Optional planning fields - only if user provides them
            ...(values.locationUID && { LocationUID: values.locationUID }),
            ...(values.routeWHOrgUID && {
              RouteWHOrgUID: values.routeWHOrgUID
            }),
            ...(values.vehicleUID && {
              UserJourneyVehicleUID: values.vehicleUID
            }),

            // Additional planning data stored in Notes field as structured data
            Notes: JSON.stringify({
              // User notes
              userNotes: values.notes || "",

              // Planning configuration
              scheduleType: values.scheduleType || "Daily",
              approvalStatus: values.approvalStatus || "draft",
              routeScheduleUID: values.routeScheduleUID || "",

              // Instructions
              specialInstructions: values.specialInstructions || "",

              // Default settings used for this plan
              defaultDuration: values.defaultDuration || 30,
              defaultTravelTime: values.defaultTravelTime || 15,

              // Enhanced optional planning fields - PLANNING ONLY
              emergencyContact: values.emergencyContact || "",

              // Metadata
              createdBy: "ADMIN",
              creationTimestamp: new Date().toISOString(),
              planType: "JOURNEY_PLAN"
            }),

            // Metadata - required by backend
            CreatedBy: createdByCode,
            ModifiedBy: createdByCode,
            CreatedTime: new Date(),
            ModifiedTime: new Date(),
            ServerAddTime: new Date(),
            ServerModifiedTime: new Date()

            // DO NOT include execution fields - backend will set them to 0:
            // - StartTime, EndTime (will be NULL until execution)
            // - ActualStoreVisits, UnPlannedStoreVisits (default 0)
            // - ZeroSalesStoreVisits, MSLStoreVisits, SkippedStoreVisits (default 0)
            // - Coverage, ACoverage, TCoverage (default 0)
            // - UserJourneyUID, InvoiceStatus (NULL until execution)
          };

          // Validate date format before sending
          const formattedVisitDate = moment(values.visitDate).format(
            "YYYY-MM-DD"
          );
          console.log("[Submit] Original visit date:", values.visitDate);
          console.log("[Submit] Formatted visit date:", formattedVisitDate);
          console.log(
            "[Submit] BeatHistory data visit date:",
            beatHistoryData.VisitDate
          );

          console.log(
            "[Submit] Using regular BeatHistory API with data:",
            beatHistoryData
          );
          const response = await api.beatHistory.create(beatHistoryData);

          if (!response?.IsSuccess) {
            // Check for specific error types
            if (response?.StatusCode === 409) {
              throw new Error(
                `Journey plan already exists for ${salesman.Name} on this date`
              );
            }

            // Check for duplicate key error
            if (
              response?.Message &&
              response.Message.includes("duplicate key")
            ) {
              throw new Error(
                `Journey plan already exists for ${salesman.Name} on ${moment(
                  values.visitDate
                ).format(
                  "YYYY-MM-DD"
                )}. Please select a different date or employee.`
              );
            }

            throw new Error(
              response?.Message ||
                `Failed to create journey plan for ${salesman.Name}`
            );
          }

          console.log("[Submit] Beat history created successfully:", response);
          console.log("[Submit] Response Data:", response.Data);
          console.log("[Submit] Success status:", response.IsSuccess);
          console.log("[Submit] Response message:", response.Message);

          // Verify the created record details
          if (response.Data) {
            console.log(
              "[Submit] Created BeatHistory UID:",
              response.Data.UID || "No UID in response"
            );
            console.log(
              "[Submit] Created with VisitDate:",
              response.Data.VisitDate || beatHistoryData.VisitDate
            );
            console.log(
              "[Submit] Created with OrgUID:",
              response.Data.OrgUID || beatHistoryData.OrgUID
            );
          }

          // Now create store histories for the planned stores
          if (values.selectedCustomersWithTimes?.length > 0) {
            const storeHistories = values.selectedCustomersWithTimes.map(
              (customer, index) => ({
                // Required identifiers
                UID: `SH_${beatHistoryData.UID}_${
                  customer.customerUID
                }_${Date.now()}_${index}`,
                BeatHistoryUID: beatHistoryData.UID,
                OrgUID: values.orgUID,
                RouteUID: values.routeUID,
                StoreUID: customer.customerUID,
                YearMonth: yearMonth,

                // PLANNING FIELDS ONLY
                IsPlanned: true,
                SerialNo: index + 1,
                Status: "Pending",
                PlannedLoginTime: customer.startTime || "00:00",
                PlannedLogoutTime: customer.endTime || "00:00",
                PlannedTimeSpendInMinutes: customer.visitDuration || 30,

                // Metadata - required by backend
                CreatedBy: createdByCode,
                ModifiedBy: createdByCode,
                CreatedTime: new Date(),
                ModifiedTime: new Date(),
                ServerAddTime: new Date(),
                ServerModifiedTime: new Date()

                // DO NOT include execution fields - backend will set defaults:
                // - LoginTime, LogoutTime (NULL until execution)
                // - IsSkipped, IsProductive (false by default)
                // - NoOfVisits, VisitDuration, TravelTime (0 by default)
                // - ActualValue, ActualVolume (0 by default)
              })
            );

            console.log(
              `[Submit] Store histories for ${storeHistories.length} stores should be created automatically by backend.`
            );
          }

          // Success - Journey plan created for salesman
          toast({
            title: "Success",
            description: `Journey plan created successfully for ${salesman.Name}`,
            variant: "default"
          });

          // Wait a moment and verify the data was created before redirecting
          console.log("[Submit] Verifying journey plan was created...");
          setTimeout(async () => {
            try {
              const verifyRequest = {
                pageNumber: 1,
                pageSize: 10,
                isCountRequired: false,
                sortCriterias: [],
                filterCriterias: [
                  {
                    filterBy: "VisitDate",
                    filterOperator: "equals",
                    filterValue: moment(values.visitDate).format("YYYY-MM-DD")
                  },
                  {
                    filterBy: "OrgUID",
                    filterOperator: "equals",
                    filterValue: values.orgUID
                  },
                  {
                    filterBy: "JobPositionUID",
                    filterOperator: "equals",
                    filterValue: selectedSalesmanUID
                  }
                ]
              };
              const verifyResponse = await api.beatHistory.selectAll(
                verifyRequest
              );
              console.log(
                "[Verify] Journey plan verification response:",
                verifyResponse
              );

              if (
                verifyResponse?.IsSuccess &&
                verifyResponse?.Data?.PagedData?.length > 0
              ) {
                console.log(
                  "[Verify] SUCCESS: Found created journey plan in database"
                );
              } else {
                console.warn(
                  "[Verify] WARNING: Journey plan not found in database after creation"
                );
                toast({
                  title: "Warning",
                  description:
                    "Journey plan created but not immediately visible. Please refresh the management page.",
                  variant: "destructive"
                });
              }
            } catch (verifyError) {
              console.error(
                "[Verify] Error verifying journey plan:",
                verifyError
              );
            }
          }, 2000); // Wait 2 seconds

          // Navigate to management page
          router.push(
            "/updatedfeatures/journey-plan-management/journey-plans/manage"
          );
        } catch (error: any) {
          // Single plan creation failed
          throw error;
        }
      }
    } catch (error: any) {
      console.error("Error creating journey plan:", error);
      toast({
        title: "Error",
        description: error?.message || "Failed to create journey plan",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
      isSubmitting.current = false;
      console.log("[Submit] Journey plan creation completed");
    }
  };

  // Step Content Renderers
  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <StepBasicSetup
            form={form}
            routes={routes}
            employees={salesmen}
            vehicles={vehicles}
            locations={locations}
            selectedSalesman={selectedSalesman}
            setSelectedSalesman={setSelectedSalesman}
            dropdownsLoading={dropdownsLoading}
            orgLevels={orgLevels}
            selectedOrgs={selectedOrgs}
            handleOrganizationSelect={handleOrganizationSelect}
            showAvailableRoutesOnly={showAvailableRoutesOnly}
            onToggleRouteFilter={toggleRouteFilter}
            allRoutesCount={allRoutes.length}
            routesWithExistingPlans={routesWithExistingPlans}
          />
        );

      case 2:
        return (
          <StepScheduleCustomers
            form={form}
            routeCustomers={routeCustomers}
            dropdownsLoading={dropdownsLoading}
          />
        );

      case 3:
        return (
          <StepReview
            form={form}
            organizations={organizations}
            routes={routes}
            employees={salesmen}
            vehicles={vehicles}
            locations={locations}
            selectedCustomersWithTimes={selectedCustomersWithTimes}
            selectedSchedule={selectedSchedule}
          />
        );

      default:
        return null;
    }
  };

  const handleReset = () => {
    form.reset();
    setCurrentStep(1);
    setSelectedCustomersWithTimes([]);
    setSelectedEmployees([]);
    setFilters({
      selectedCustomerType: 'all',
      selectedArea: 'all',
      searchTerm: ''
    });
  };

  return (
    <div className="min-h-screen">
      {/* Header */}
      <div className="bg-white border-b px-6 py-5">
        <div className="max-w-6xl">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-semibold text-gray-900">
                Create Journey Plan
              </h1>
              <p className="text-sm text-gray-600 mt-1">
                Create a new journey plan for your sales routes
              </p>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={() =>
                router.push(
                  "/updatedfeatures/journey-plan-management/journey-plans/manage"
                )
              }
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Journey Plans
            </Button>
          </div>
        </div>
      </div>

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
            <Form {...form}>
              <form
          onSubmit={(e) => {
            e.preventDefault();
            console.log(
              "[Form] Form submit triggered, currentStep:",
              currentStep
            );

            // Only allow submission from step 3 and when button is clicked
            if (currentStep === 3) {
              console.log("[Form] On step 3, allowing submission");
              form.handleSubmit(onSubmit)(e);
            } else {
              console.log("[Form] Not on step 3, preventing submission");
              toast({
                title: "Cannot submit yet",
                description: "Please complete all steps before submitting.",
                variant: "destructive"
              });
            }
          }}
          onKeyDown={(e) => {
            // Prevent form submission on Enter key
            if (e.key === "Enter") {
              e.preventDefault();
              console.log("[Form] Enter key prevented");
              return false;
            }
          }}
        >
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
                      <Button
                        type="button"
                        variant="ghost"
                        onClick={handleReset}
                        className="h-10"
                      >
                        <RotateCcw className="h-4 w-4 mr-2" />
                        Reset
                      </Button>

                      {currentStep < progressSteps.length ? (
                        <Button type="button" onClick={handleNext} className="h-10">
                          Next
                          <ArrowRight className="h-4 w-4 ml-2" />
                        </Button>
                      ) : (
                        <Button
                          type="button"
                          className="h-10"
                          disabled={loading || selectedCustomersWithTimes.length === 0}
                          onClick={(e) => {
                    e.preventDefault();
                    console.log(
                      "[Submit Button] Create button clicked explicitly"
                    );

                    // Additional validation before submission
                    if (selectedCustomersWithTimes.length === 0) {
                      toast({
                        title: "Validation Error",
                        description:
                          "Please select at least one customer before creating the journey plan.",
                        variant: "destructive"
                      });
                      return;
                    }

                    // Manually trigger form submission only when button is clicked
                    console.log("[Submit Button] Triggering form submission");
                    form.handleSubmit(onSubmit)();
                  }}
                >
                          {loading ? (
                            <>
                              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                              Creating...
                            </>
                          ) : (
                            <>
                              <CheckCircle2 className="h-4 w-4 mr-2" />
                              Create Journey Plan
                            </>
                          )}
                        </Button>
                      )}
                    </div>
                  </div>
                </div>
              </form>
            </Form>
          </div>
        </div>
      </div>

      {/* Bulk Generator Dialog */}
      {showBulkGenerator && (
        <BulkJourneyPlanGenerator
          open={showBulkGenerator}
          onClose={() => setShowBulkGenerator(false)}
          routeUID={form.watch("routeUID") || ""}
          orgUID={form.watch("orgUID") || ""}
          jobPositionUID={form.watch("jobPositionUID") || ""}
          loginId={form.watch("loginId") || ""}
          routeSchedule={selectedSchedule}
          onSuccess={(count) => {
            toast({
              title: "Success",
              description: `Created ${count} journey plans successfully!`
            });
            router.push(
              "/updatedfeatures/journey-plan-management/journey-plans/manage"
            );
          }}
        />
      )}
    </div>
  );
}
