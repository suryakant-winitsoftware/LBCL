"use client";
import React, { useState, useEffect, useRef } from "react";
import {
  Calendar,
  Clock,
  Plus,
  Settings,
  Download,
  Upload,
  Copy,
  Trash2,
  Save,
  X,
  Search,
  Filter,
  Bell,
  Users,
  TrendingUp,
  AlertTriangle,
  CheckCircle2,
  GripVertical,
  RotateCcw,
  Star,
  MapPin,
  Phone,
  Check,
  ChevronDown,
} from "lucide-react";

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  Type?: string;
  Status?: string;
}

interface ScheduledCustomer {
  id: number;
  sequence: number;
  name: string;
  startTime: string;
  endTime: string;
  duration: number;
  period: string;
  day: string | null;
  week: number | null;
  date: number | null;
  fortnightPart: string | null;
  phone: string;
  location: string;
  priority: string;
  notes: string;
  createdAt: string;
  customerUID?: string;
  customerCode?: string;
}

interface CustomerScheduling {
  customerUID: string;
  frequency: "daily" | "weekly" | "monthly" | "fortnight";
  scheduleConfigs: {
    scheduleType: string;
    weekNumber?: number;
    dayNumber?: number;
  }[];
}

interface CustomerSchedulerProps {
  selectedPeriod?: string;
  initialActiveScheduleTypes?: string[];
  initialScheduleCustomerAssignments?: {
    [scheduleType: string]: string[];
  };
  routeScheduleConfigs?: any[];
  routeScheduleCustomerMappings?: any[];
  availableCustomers?: Customer[];
  onCustomersScheduled?: (customers: ScheduledCustomer[]) => void;
  onCustomerSchedulingChange?: (
    customerScheduling: CustomerScheduling[]
  ) => void;
}

const CustomerScheduler = ({
  selectedPeriod: propSelectedPeriod,
  initialActiveScheduleTypes,
  initialScheduleCustomerAssignments,
  routeScheduleConfigs = [],
  routeScheduleCustomerMappings = [],
  availableCustomers = [],
  onCustomersScheduled,
  onCustomerSchedulingChange,
}: CustomerSchedulerProps) => {
  // Multi-schedule support - allow multiple active schedule types
  const [activeScheduleTypes, setActiveScheduleTypes] = useState<string[]>(
    initialActiveScheduleTypes && initialActiveScheduleTypes.length > 0
      ? initialActiveScheduleTypes
      : ["weekly"]
  );
  const [currentlySelectedType, setCurrentlySelectedType] = useState(
    initialActiveScheduleTypes && initialActiveScheduleTypes.length > 0
      ? initialActiveScheduleTypes.includes("daily")
        ? "daily"
        : initialActiveScheduleTypes[0]
      : "weekly"
  );
  const selectedPeriod = propSelectedPeriod || currentlySelectedType;

  // Backward compatibility: If propSelectedPeriod is provided, use single-schedule mode
  const isMultiScheduleMode = !propSelectedPeriod;

  // Track customers assigned to each schedule type
  const [scheduleCustomerAssignments, setScheduleCustomerAssignments] =
    useState<{
      [scheduleType: string]: string[]; // customerUIDs
    }>(() => {
      // Initialize from props or localStorage
      if (
        initialScheduleCustomerAssignments &&
        Object.keys(initialScheduleCustomerAssignments).length > 0
      ) {
        console.log(
          "📦 Using initial schedule customer assignments from props:",
          initialScheduleCustomerAssignments
        );
        return initialScheduleCustomerAssignments;
      }

      // Don't restore from localStorage to prevent persistence after hard reload
      return {};
    });

  // Track which customers are assigned to which schedule types
  const [assignedCustomers, setAssignedCustomers] = useState<Set<string>>(
    () => {
      if (initialScheduleCustomerAssignments) {
        const allAssigned = Object.values(
          initialScheduleCustomerAssignments
        ).flat();
        return new Set(allAssigned);
      }
      return new Set();
    }
  );

  // State for copy functionality
  const [showCopyModal, setShowCopyModal] = useState(false);
  const [selectedTargetDays, setSelectedTargetDays] = useState<string[]>([]);
  const [selectedTargetDates, setSelectedTargetDates] = useState<number[]>([]);
  const [selectedTargetWeeks, setSelectedTargetWeeks] = useState<number[]>([]);

  // Toast notification state
  const [toast, setToast] = useState<{
    show: boolean;
    message: string;
    type: "success" | "error" | "info";
  }>({ show: false, message: "", type: "success" });

  // Initialize customerScheduling from localStorage to persist across component re-renders
  const [customerScheduling, setCustomerScheduling] = useState<
    CustomerScheduling[]
  >([]);

  // Removed auto-saving to localStorage to prevent persistence after hard reload
  // Only save when explicitly needed during route creation

  // Sync activeScheduleTypes with frequencies that have assigned customers and auto-select first one
  useEffect(() => {
    const frequenciesWithCustomers = Object.keys(
      scheduleCustomerAssignments
    ).filter((type) => (scheduleCustomerAssignments[type] || []).length > 0);

    // Update activeScheduleTypes to include all frequencies with customers
    if (frequenciesWithCustomers.length > 0) {
      setActiveScheduleTypes(frequenciesWithCustomers);

      // Auto-select the first frequency if current selection doesn't have customers
      if (!frequenciesWithCustomers.includes(currentlySelectedType)) {
        setCurrentlySelectedType(frequenciesWithCustomers[0]);
      }
    }
  }, [scheduleCustomerAssignments, currentlySelectedType]);

  // Save customerScheduling to localStorage whenever it changes
  const prevCustomerSchedulingRef = useRef<string>("");
  useEffect(() => {
    const currentSchedulingStr = JSON.stringify(customerScheduling);

    // Only update if the data actually changed
    if (currentSchedulingStr !== prevCustomerSchedulingRef.current) {
      prevCustomerSchedulingRef.current = currentSchedulingStr;

      if (customerScheduling.length > 0) {
        console.log(
          "💾 Saving customerScheduling to localStorage:",
          customerScheduling.length,
          "entries"
        );
        // Removed localStorage saving to prevent persistence after hard reload

        // Notify parent immediately
        if (onCustomerSchedulingChange) {
          onCustomerSchedulingChange(customerScheduling);
        }
      }
    }
  }, [customerScheduling]);

  // State to track if daily customers have been populated
  const [dailyCustomersPopulated, setDailyCustomersPopulated] = useState(false);
  const [lastDailyCustomerCount, setLastDailyCustomerCount] = useState(0);

  // Reset daily population flag when switching to daily or when daily customers change
  useEffect(() => {
    if (currentlySelectedType === "daily") {
      setDailyCustomersPopulated(false);
      setLastDailyCustomerCount(0);
    }
  }, [currentlySelectedType, scheduleCustomerAssignments.daily?.length]);

  // Initialize from saved data on mount
  useEffect(() => {
    try {
      const savedDayWise = localStorage.getItem("tempDayWiseCustomers");
      if (savedDayWise) {
        const parsedDayWise = JSON.parse(savedDayWise);

        // Find contexts with customers
        const contextsWithCustomers = Object.entries(parsedDayWise)
          .filter(([key, customers]) => (customers as any[]).length > 0)
          .map(([key]) => key);

        if (contextsWithCustomers.length > 0) {
          console.log(
            "🔄 Found saved schedule data for contexts:",
            contextsWithCustomers
          );

          // Determine the appropriate frequency and selection based on saved data
          const firstContext = contextsWithCustomers[0];

          if (firstContext.startsWith("daily")) {
            setCurrentlySelectedType("daily");
          } else if (firstContext.startsWith("weekly")) {
            setCurrentlySelectedType("weekly");
            // Extract week and day from context key (e.g., "weekly_W1_Monday")
            const parts = firstContext.split("_");
            if (parts.length >= 3) {
              const weekNum = parseInt(parts[1].substring(1)); // Remove 'W' prefix
              setSelectedWeek(weekNum);
              setSelectedDay(parts[2]);
            }
          } else if (firstContext.startsWith("date")) {
            setCurrentlySelectedType("monthly");
            // Extract date from context key (e.g., "date_15")
            const dateNum = parseInt(firstContext.split("_")[1]);
            setSelectedDate(dateNum);
          } else if (firstContext.includes("week")) {
            setCurrentlySelectedType("fortnight");
            // Extract fortnight details
            const parts = firstContext.split("_");
            if (parts.length >= 3) {
              setSelectedFortnightPart(parts[0]);
              setSelectedDay(parts[2]);
            }
          }
        }
      }
    } catch (e) {
      console.error("Failed to initialize from saved data:", e);
    }
  }, []); // Run only on mount

  // Auto-populate daily customers from availableCustomers
  useEffect(() => {
    const currentScheduleType = isMultiScheduleMode
      ? currentlySelectedType
      : selectedPeriod;

    // For daily schedules, automatically populate ONLY customers assigned to daily
    if (
      currentScheduleType === "daily" &&
      availableCustomers.length > 0
    ) {
      let customersToPopulate = [];

      // Always use customers specifically assigned to daily schedule type
      // Never use all available customers
      const assignedToDailyUIDs = scheduleCustomerAssignments.daily || [];
      customersToPopulate = availableCustomers.filter((customer) =>
        assignedToDailyUIDs.includes(customer.UID)
      );

      // Check if we need to update (customer count changed or not yet populated)
      const needsUpdate = !dailyCustomersPopulated ||
        customersToPopulate.length !== lastDailyCustomerCount ||
        (scheduleCustomerAssignments.daily &&
         scheduleCustomerAssignments.daily.length !== lastDailyCustomerCount);

      // Convert to ScheduledCustomer format and populate dayWiseCustomers
      if (needsUpdate && customersToPopulate.length > 0) {
        const dailyCustomers = customersToPopulate.map((customer, index) => ({
          id: Date.now() + Math.random() + index,
          sequence: index + 1,
          name: customer.Name,
          customerCode: customer.Code,
          startTime: "NA",
          endTime: "NA",
          duration: 0,
          period: "daily",
          day: null,
          week: null,
          date: null,
          fortnightPart: null,
          phone: customer.ContactNo || "",
          location: customer.Address || "",
          priority: "normal",
          notes: "",
          createdAt: new Date().toISOString(),
          customerUID: customer.UID,
        }));

        setDayWiseCustomers((prev) => ({
          ...prev,
          daily_NA_0: dailyCustomers,
        }));

        setCustomers(dailyCustomers);
        setDailyCustomersPopulated(true);
        setLastDailyCustomerCount(customersToPopulate.length);

        // Also update customerScheduling for the parent component
        const newCustomerScheduling = dailyCustomers.map(customer => ({
          customerUID: customer.customerUID || "",
          frequency: "daily" as const,
          scheduleConfigs: [{
            scheduleType: "daily",
            dayNumber: 0
          }]
        }));

        if (onCustomerSchedulingChange) {
          onCustomerSchedulingChange(newCustomerScheduling);
        }
      }
    }
  }, [
    availableCustomers,
    scheduleCustomerAssignments,
    currentlySelectedType,
    selectedPeriod,
    isMultiScheduleMode,
    dailyCustomersPopulated,
    lastDailyCustomerCount,
    onCustomerSchedulingChange,
  ]);

  // Helper function to get day name from day number
  const getDayNameFromNumber = (dayNumber: number): string => {
    const days = [
      "Sunday",
      "Monday",
      "Tuesday",
      "Wednesday",
      "Thursday",
      "Friday",
      "Saturday",
    ];
    return days[dayNumber] || "Monday";
  };

  // Helper function to get fortnight part from week number
  const getFortnightPartFromWeek = (weekNumber: number | string): string => {
    // Handle string format like "W13" or just "13"
    const numericWeek =
      typeof weekNumber === "string"
        ? parseInt(weekNumber.replace("W", "").replace("w", ""))
        : weekNumber;

    if (numericWeek === 13) return "1st-3rd";
    if (numericWeek === 24) return "2nd-4th";
    return "1st-3rd"; // default
  };

  // Track if we have manual assignments to prevent data reset
  const [hasManualAssignments, setHasManualAssignments] = useState(false);

  // Populate dayWiseCustomers from scheduleCustomerAssignments with dynamic mapping
  useEffect(() => {
    const hasAnyAssignments = Object.values(scheduleCustomerAssignments).some(
      (customers) => customers.length > 0
    );

    // Don't run if we have manual assignments (prevents overriding user's manual day assignments)
    if (hasManualAssignments) {
      console.log(
        "🚫 Skipping dayWiseCustomers reset - manual assignments detected"
      );
      return;
    }

    // Run if we have assignments and available customers
    if (hasAnyAssignments && availableCustomers.length > 0) {
      if (routeScheduleConfigs.length === 0) {
      }
      if (routeScheduleCustomerMappings.length === 0) {
      }

      const newDayWiseCustomers: { [key: string]: ScheduledCustomer[] } = {};

      // Create a map of customer UID to their schedule configs
      const customerConfigMap = new Map();

      // Process route schedule customer mappings to get config relationships
      routeScheduleCustomerMappings.forEach((mapping: any) => {
        // Handle customer UID format - might have route prefix
        let customerUID = mapping.CustomerUID;
        // Remove route prefix if present (e.g., "RT17578801369880VPMNS48R3_1008" -> "1008")
        if (customerUID && customerUID.includes("_")) {
          customerUID = customerUID.split("_").pop();
        }
        const configUID = mapping.RouteScheduleConfigUID;

        // Find the matching config
        const config = routeScheduleConfigs.find(
          (c: any) => c.UID === configUID
        );
        if (config) {
          if (!customerConfigMap.has(customerUID)) {
            customerConfigMap.set(customerUID, []);
          }
          customerConfigMap.get(customerUID).push(config);
        }
      });

      // Process each frequency type

      Object.entries(scheduleCustomerAssignments).forEach(
        ([frequency, customerUIDs]) => {
          if (customerUIDs.length > 0) {
            // Convert customer UIDs to full customer objects
            const customers: ScheduledCustomer[] = customerUIDs
              .map((uid, index) => {
                const customer = availableCustomers.find((c) => c.UID === uid);

                // Get schedule-type-specific settings
                const typeSettings =
                  scheduleTypeSettings[frequency] || scheduleTypeSettings.daily;

                // Calculate end time based on start time and duration
                const startTimeMinutes = timeToMinutes(typeSettings.dayStart);
                const durationMinutes = typeSettings.defaultDuration;
                const endTimeMinutes = startTimeMinutes + durationMinutes;
                const endTime = minutesToTime(endTimeMinutes);

                return customer
                  ? {
                      id: `${customer.UID}_${frequency}`,
                      customerUID: customer.UID,
                      customerCode: customer.Code,
                      name: customer.Name,
                      address: customer.Address || "",
                      contactNo: customer.ContactNo || "",
                      type: customer.Type || "",
                      status: customer.Status || "Active",
                      sequence: index + 1,
                      startTime: typeSettings.dayStart,
                      endTime: endTime,
                      visitDuration: typeSettings.defaultDuration,
                      travelTime: typeSettings.travelTime,
                      frequency: frequency,
                      conflict: false,
                      conflictType: "",
                      isEditing: false,
                      priority: "normal",
                    }
                  : null;
              })
              .filter(Boolean) as ScheduledCustomer[];

            // Map to appropriate dayWise keys based on frequency and actual config data
            // ONLY auto-populate daily customers, others should be manually assigned via dropdown
            if (frequency === "daily") {
              newDayWiseCustomers.daily_NA_0 = customers;
            }
            // For weekly/monthly/fortnight: DON'T auto-populate dayWiseCustomers
            // Customers will be available in dropdown and manually assigned to specific days
            // This prevents auto-assignment to Week 1 Monday, etc.
          }
        }
      );

      // Only update if there are actual changes and content
      const hasChanges = Object.keys(newDayWiseCustomers).length > 0;
      if (hasChanges) {
        setDayWiseCustomers((prevDayWise) => {
          // Compare if the new data is actually different from previous
          const prevKeys = Object.keys(prevDayWise);
          const newKeys = Object.keys(newDayWiseCustomers);

          // Quick check: if key lengths are different, definitely update
          if (prevKeys.length !== newKeys.length) {
            return newDayWiseCustomers;
          }

          // Check if any content has changed
          const hasContentChanges = newKeys.some((key) => {
            const prevCustomers = prevDayWise[key] || [];
            const newCustomers = newDayWiseCustomers[key] || [];
            return (
              prevCustomers.length !== newCustomers.length ||
              JSON.stringify(prevCustomers.map((c) => c.customerUID)) !==
                JSON.stringify(newCustomers.map((c) => c.customerUID))
            );
          });

          return hasContentChanges ? newDayWiseCustomers : prevDayWise;
        });
      }
    }
  }, [
    scheduleCustomerAssignments,
    availableCustomers,
    routeScheduleConfigs,
    routeScheduleCustomerMappings,
  ]);

  // Function to rebuild customerScheduling from dayWiseCustomers (backup recovery)
  const rebuildCustomerSchedulingFromDayWise = () => {
    const allCustomers = Object.values(dayWiseCustomers).flat();
    if (allCustomers.length === 0) return [];

    const schedulingMap = new Map<string, CustomerScheduling>();

    allCustomers.forEach((customer) => {
      if (!customer.name || !customer.period) return;

      // Find the customer UID from availableCustomers based on name
      const matchingCustomer = availableCustomers.find(
        (ac) => ac.Name === customer.name
      );
      if (!matchingCustomer) return;

      const customerUID = matchingCustomer.UID;

      // Generate schedule config based on customer data
      const scheduleConfig = {
        scheduleType: customer.period,
        weekNumber:
          customer.week ||
          (customer.fortnightPart === "1st-3rd"
            ? 13
            : customer.fortnightPart === "2nd-4th"
            ? 24
            : 1),
        dayNumber: customer.day
          ? getDayNumber(customer.day)
          : customer.date || 1,
      };

      if (schedulingMap.has(customerUID)) {
        // Customer exists, add to their configs
        schedulingMap.get(customerUID)!.scheduleConfigs.push(scheduleConfig);
      } else {
        // New customer
        schedulingMap.set(customerUID, {
          customerUID,
          frequency: customer.period as
            | "daily"
            | "weekly"
            | "monthly"
            | "fortnight",
          scheduleConfigs: [scheduleConfig],
        });
      }
    });

    const rebuilt = Array.from(schedulingMap.values());

    return rebuilt;
  };

  // Removed duplicate localStorage save - already handled in the useEffect above
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(
    null
  );
  const [selectedWeek, setSelectedWeek] = useState(1);
  const [selectedDay, setSelectedDay] = useState("Monday");
  const [selectedDate, setSelectedDate] = useState(1);
  const [monthlyDaysCount, setMonthlyDaysCount] = useState(30);
  const [selectedFortnightPart, setSelectedFortnightPart] = useState("1st-3rd");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCustomerUIDs, setSelectedCustomerUIDs] = useState<string[]>(
    []
  );
  const [showCustomerDropdown, setShowCustomerDropdown] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const [showSettings, setShowSettings] = useState(false);
  const [showTimeView, setShowTimeView] = useState(false);
  const [lastAction, setLastAction] = useState(null);
  // Time settings per schedule type - each frequency has its own settings
  const [scheduleTypeSettings, setScheduleTypeSettings] = useState({
    daily: {
      dayStart: "NA",
      dayEnd: "NA",
      defaultDuration: 0,
      travelTime: 0,
    },
    weekly: {
      dayStart: "NA",
      dayEnd: "NA",
      defaultDuration: 0,
      travelTime: 0,
    },
    fortnight: {
      dayStart: "NA",
      dayEnd: "NA",
      defaultDuration: 0,
      travelTime: 0,
    },
    monthly: {
      dayStart: "NA",
      dayEnd: "NA",
      defaultDuration: 0,
      travelTime: 0,
    },
  });

  // Legacy global settings for backward compatibility
  const [settings, setSettings] = useState({
    dayStart: "NA",
    dayEnd: "NA",
    defaultDuration: 0,
    travelTime: 0,
    includeTime: true,
    autoSave: false,
    notifications: true,
  });
  const [customers, setCustomersInternal] = useState<ScheduledCustomer[]>([]);
  const [draggedCustomer, setDraggedCustomer] =
    useState<ScheduledCustomer | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

  // Helper function to get current schedule type settings
  const getCurrentScheduleTypeSettings = () => {
    // If in multi-schedule mode, use the currently selected type's settings
    if (isMultiScheduleMode && currentlySelectedType) {
      return (
        scheduleTypeSettings[currentlySelectedType] ||
        scheduleTypeSettings.daily
      );
    }
    // If in single schedule mode, use the selected period's settings
    return scheduleTypeSettings[selectedPeriod] || scheduleTypeSettings.daily;
  };

  // Helper function to convert day name to number
  const getDayNumber = (dayName: string): number => {
    const days = {
      monday: 1,
      tuesday: 2,
      wednesday: 3,
      thursday: 4,
      friday: 5,
      saturday: 6,
      sunday: 7,
    };
    return days[dayName.toLowerCase() as keyof typeof days] || 1;
  };

  // Helper function to generate schedule configs based on frequency and selections
  const generateScheduleConfigs = (frequency: string, selections: any) => {
    const configs = [];

    switch (frequency) {
      case "daily":
        // For daily, use dayNumber: 0 to match master data: daily_NA_0
        configs.push({ scheduleType: "daily", dayNumber: 0 });
        break;
      case "weekly":
        // Weekly with specific day and week
        if (selections.day) {
          configs.push({
            scheduleType: "weekly",
            weekNumber: selections.week || 1, // Include the week number (W1 for weekly)
            dayNumber: getDayNumber(selections.day),
          });
        }
        break;
      case "monthly":
        // Monthly with specific date
        if (selections.date) {
          configs.push({ scheduleType: "monthly", dayNumber: selections.date });
        }
        break;
      case "fortnight":
        // Fortnight with week pattern based on part selection (W13 or W24) AND specific day
        if (selections.fortnightPart === "1st-3rd" && selections.day) {
          configs.push({
            scheduleType: "fortnight",
            weekNumber: 13, // Fixed week number for 1st-3rd part
            dayNumber: getDayNumber(selections.day),
          });
        } else if (selections.fortnightPart === "2nd-4th" && selections.day) {
          configs.push({
            scheduleType: "fortnight",
            weekNumber: 24, // Fixed week number for 2nd-4th part
            dayNumber: getDayNumber(selections.day),
          });
        }
        break;
    }

    return configs;
  };

  // Handle adding customer with specific frequency
  const addCustomerWithFrequency = (customer: Customer, frequency: string) => {
    const scheduleConfigs = generateScheduleConfigs(frequency, {
      day: selectedDay,
      week: selectedWeek, // Pass the selected week
      date: selectedDate,
      fortnightPart: selectedFortnightPart,
    });

    const newCustomerScheduling: CustomerScheduling = {
      customerUID: customer.UID,
      frequency: frequency as "daily" | "weekly" | "monthly" | "fortnight",
      scheduleConfigs,
    };

    // Check if this customer already exists in the current scheduling
    const existingIndex = customerScheduling.findIndex(
      (cs) => cs.customerUID === customer.UID
    );
    let updatedScheduling: CustomerScheduling[];

    if (existingIndex !== -1) {
      // Customer exists - merge schedule configs
      updatedScheduling = [...customerScheduling];
      updatedScheduling[existingIndex].scheduleConfigs.push(...scheduleConfigs);
    } else {
      // New customer - add to array
      updatedScheduling = [...customerScheduling, newCustomerScheduling];
    }

    setCustomerScheduling(updatedScheduling);

    // Also add to the visual customers list
    const newScheduledCustomer: ScheduledCustomer = {
      id: Date.now() + Math.random(),
      sequence: customers.length + 1,
      name: customer.Name,
      customerCode: customer.Code,
      startTime: "NA",
      endTime: "NA",
      duration: 0,
      period: frequency,
      day:
        frequency === "weekly" || frequency === "fortnight"
          ? selectedDay
          : null,
      week: frequency === "weekly" ? selectedWeek : null, // Week only for weekly schedules
      date: frequency === "monthly" ? selectedDate : null,
      fortnightPart: frequency === "fortnight" ? selectedFortnightPart : null,
      phone: customer.ContactNo || "",
      location: customer.Address || "",
      priority: "normal",
      notes: "",
      createdAt: new Date().toISOString(),
    };

    // Update both customers list and dayWiseCustomers
    setCustomers([...customers, newScheduledCustomer]);

    // Update dayWiseCustomers based on context
    const contextKey = getCurrentContextKey();
    setDayWiseCustomers((prev) => ({
      ...prev,
      [contextKey]: [...(prev[contextKey] || []), newScheduledCustomer],
    }));

    // Notify parent component
    if (onCustomerSchedulingChange) {
      onCustomerSchedulingChange(updatedScheduling);
    }
    if (onCustomersScheduled) {
      onCustomersScheduled([...customers, newScheduledCustomer]);
    }
  };

  // Handle adding selected customers with current frequency (FIXED for batch operations)
  const addSelectedCustomers = () => {
    if (selectedCustomerUIDs.length === 0) return;

    console.log("🚀 ADD CUSTOMERS CALLED - Starting assignment");
    console.log("🔍 Selected Customer UIDs:", selectedCustomerUIDs);
    console.log("🔍 Available Customers:", availableCustomers.length);

    // Get the current schedule type - important for multi-schedule mode
    const currentScheduleType = isMultiScheduleMode
      ? currentlySelectedType
      : selectedPeriod;

    // Update multi-schedule tracking only if in multi-schedule mode
    if (isMultiScheduleMode) {
      // Update assigned customers tracking
      const newAssignedCustomers = new Set(assignedCustomers);
      selectedCustomerUIDs.forEach((uid) => newAssignedCustomers.add(uid));
      setAssignedCustomers(newAssignedCustomers);

      // Update schedule-specific assignments using the correct schedule type
      const currentAssignments =
        scheduleCustomerAssignments[currentScheduleType] || [];
      // Create a Set to ensure unique customer UIDs (no duplicates)
      const uniqueCustomerUIDs = new Set([
        ...currentAssignments,
        ...selectedCustomerUIDs,
      ]);
      setScheduleCustomerAssignments({
        ...scheduleCustomerAssignments,
        [currentScheduleType]: Array.from(uniqueCustomerUIDs),
      });
    }

    // BATCH OPERATION: Process all customers at once to avoid race conditions
    const newCustomerSchedulingEntries: CustomerScheduling[] = [];
    const newScheduledCustomers: ScheduledCustomer[] = [];

    selectedCustomerUIDs.forEach((uid, index) => {
      const customer = availableCustomers.find((c) => c.UID === uid);
      if (customer) {
        // Generate schedule configs for this customer using current schedule type
        const scheduleConfigs = generateScheduleConfigs(currentScheduleType, {
          day: selectedDay,
          week: currentScheduleType === "weekly" ? selectedWeek : 1, // Only use selectedWeek for weekly
          date: selectedDate,
          fortnightPart: selectedFortnightPart,
        });

        // Create customer scheduling entry
        const newCustomerScheduling: CustomerScheduling = {
          customerUID: customer.UID,
          frequency: currentScheduleType as
            | "daily"
            | "weekly"
            | "monthly"
            | "fortnight",
          scheduleConfigs,
        };

        newCustomerSchedulingEntries.push(newCustomerScheduling);

        // Get schedule-type-specific settings for proper time defaults
        const typeSettings =
          scheduleTypeSettings[currentScheduleType] ||
          scheduleTypeSettings.daily;
        const defaultStartTime = typeSettings.dayStart;
        const defaultDuration = typeSettings.defaultDuration;
        const calculatedEndTime = calculateEndTime(
          defaultStartTime,
          defaultDuration
        );

        // Create visual customer entry
        const newScheduledCustomer: ScheduledCustomer = {
          id: Date.now() + Math.random() + index, // Ensure unique IDs
          sequence: customers.length + index + 1,
          name: customer.Name,
          customerCode: customer.Code,
          startTime: defaultStartTime,
          endTime: calculatedEndTime,
          duration: defaultDuration,
          period: currentScheduleType,
          day:
            currentScheduleType === "weekly" ||
            currentScheduleType === "fortnight"
              ? selectedDay
              : null,
          week: currentScheduleType === "weekly" ? selectedWeek : null, // Week only for weekly schedules
          date: currentScheduleType === "monthly" ? selectedDate : null,
          fortnightPart:
            currentScheduleType === "fortnight" ? selectedFortnightPart : null,
          phone: customer.ContactNo || "",
          location: customer.Address || "",
          priority: "normal",
          notes: "",
          createdAt: new Date().toISOString(),
        };

        newScheduledCustomers.push(newScheduledCustomer);
      }
    });

    // BATCH UPDATE: Update all states at once

    // Check for duplicates and merge with existing data
    const updatedScheduling = [...customerScheduling];

    newCustomerSchedulingEntries.forEach((newEntry) => {
      const existingIndex = updatedScheduling.findIndex(
        (cs) => cs.customerUID === newEntry.customerUID
      );
      if (existingIndex !== -1) {
        // Customer exists - merge schedule configs
        updatedScheduling[existingIndex].scheduleConfigs.push(
          ...newEntry.scheduleConfigs
        );
      } else {
        // New customer - add to array
        updatedScheduling.push(newEntry);
      }
    });

    // Single state update for customerScheduling
    setCustomerScheduling(updatedScheduling);

    // Update visual customers
    setCustomers([...customers, ...newScheduledCustomers]);

    // Update dayWiseCustomers
    const contextKey = getCurrentContextKey();
    console.log("🔧 Assignment Debug:", {
      currentScheduleType,
      selectedWeek,
      selectedDay,
      selectedDate,
      selectedFortnightPart,
      contextKey,
      customersToAdd: newScheduledCustomers.length,
    });

    setDayWiseCustomers((prev) => {
      const updated = {
        ...prev,
        [contextKey]: [...(prev[contextKey] || []), ...newScheduledCustomers],
      };
      console.log("🔧 Updated dayWiseCustomers:", updated[contextKey]);
      return updated;
    });

    // Mark that we have manual assignments to prevent data reset
    setHasManualAssignments(true);
    console.log("🔒 Manual assignments flag set to true");

    // Notify parent component of the batch update
    if (onCustomerSchedulingChange) {
      onCustomerSchedulingChange(updatedScheduling);
    }

    // Notify parent about visual customers
    if (onCustomersScheduled) {
      onCustomersScheduled([...customers, ...newScheduledCustomers]);
    }

    // Clear selection
    setSelectedCustomerUIDs([]);
    setShowCustomerDropdown(false);
  };

  // Initialize dayWiseCustomers with all possible keys
  const initializeDayWiseCustomers = () => {
    const initial: { [key: string]: ScheduledCustomer[] } = {
      // Daily: Store all daily customers (always use daily_NA_0)
      daily_NA_0: [],
    };

    const days = [
      "Monday",
      "Tuesday",
      "Wednesday",
      "Thursday",
      "Friday",
      "Saturday",
      "Sunday",
    ];

    // Weekly: Store by week-day combinations (W1-W5, Monday-Sunday)
    for (let week = 1; week <= 5; week++) {
      for (const day of days) {
        initial[`weekly_W${week}_${day}`] = [];
      }
    }

    // Monthly: Store by date (1-31)
    for (let i = 1; i <= 31; i++) {
      initial[`date_${i}`] = [];
    }

    // Fortnightly: Store by week-day-part combinations
    const fortnightParts = ["1st-3rd", "2nd-4th"];
    for (let week = 1; week <= 5; week++) {
      for (const part of fortnightParts) {
        for (const day of days) {
          initial[`${part}_week${week}_${day}`] = [];
        }
      }
    }

    return initial;
  };

  // Store customers by day/week/date depending on period type (with localStorage persistence)
  const [dayWiseCustomers, setDayWiseCustomers] = useState<{
    [key: string]: ScheduledCustomer[];
  }>(() => {
    // Don't restore from localStorage to prevent persistence after hard reload
    return initializeDayWiseCustomers();
  });

  // Removed auto-saving to localStorage to prevent persistence after hard reload
  // Only save when explicitly needed during route creation

  // Update customers when context changes (day/week/month selection) or dayWiseCustomers is loaded
  useEffect(() => {
    const contextKey = getCurrentContextKey();
    const contextCustomers = dayWiseCustomers[contextKey] || [];

    console.log(
      "📍 Context changed to:",
      contextKey,
      "- Loading",
      contextCustomers.length,
      "customers"
    );
    console.log(
      "📂 Available contexts:",
      Object.keys(dayWiseCustomers).filter(
        (k) => dayWiseCustomers[k].length > 0
      )
    );
    console.log(
      "👥 Total scheduled customers:",
      Object.values(dayWiseCustomers).flat().length
    );

    setCustomers(contextCustomers);
  }, [
    selectedPeriod,
    selectedDay,
    selectedWeek,
    selectedDate,
    selectedFortnightPart,
    dayWiseCustomers,
  ]);

  // Wrapper to track all setCustomers calls
  const setCustomers = (newCustomers: any) => {
    setCustomersInternal(newCustomers);

    // Notify parent component about scheduled customers
    if (onCustomersScheduled) {
      onCustomersScheduled(newCustomers);
    }
  };

  // Handle click outside to close dropdown
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        const button = document.querySelector("[data-dropdown-trigger]");
        if (button && !button.contains(event.target as Node)) {
          setShowCustomerDropdown(false);
        }
      }
    };

    if (showCustomerDropdown) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [showCustomerDropdown]);

  // Initialize component - only run once on mount to avoid infinite loops
  const hasInitializedRef = useRef(false);
  useEffect(() => {
    // Only run once on mount
    if (hasInitializedRef.current) return;
    hasInitializedRef.current = true;

    // Clear any stale localStorage data on mount to prevent auto-population (but keep tempCustomerScheduling)
    localStorage.removeItem("customerSchedule");

    // Don't trigger parent updates on mount - let the data flow naturally
    // The parent will get updates when user actually makes changes

    // Cleanup on unmount
    return () => {
      hasInitializedRef.current = false;
    };
  }, []); // Empty dependency array - only run on mount

  useEffect(() => {}, [customers]);
  const [isAddingCustomer, setIsAddingCustomer] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState(null);
  const [selectedCustomers, setSelectedCustomers] = useState([]);
  const [expandedCustomerId, setExpandedCustomerId] = useState(null); // Track which customer row is expanded
  const [newCustomer, setNewCustomer] = useState({
    name: "",
    startTime: "",
    duration: 0, // Default to NA (0)
    phone: "",
    location: "",
    priority: "normal",
    notes: "",
  });
  const [conflicts, setConflicts] = useState([]);

  // Get all customers already assigned to any day/week/month (to prevent duplicates)
  const getAllAssignedCustomerUIDs = () => {
    const assignedUIDs = new Set<string>();

    // Check all dayWiseCustomers contexts to find already assigned customers
    Object.values(dayWiseCustomers).forEach((contextCustomers) => {
      contextCustomers.forEach((customer) => {
        if (customer.customerUID) {
          assignedUIDs.add(customer.customerUID);
        }
      });
    });

    return assignedUIDs;
  };

  // Available customers for selection - Show all customers from customer step for non-daily, prevent duplicates
  const getAvailableCustomersForSelection = () => {
    const currentScheduleType = isMultiScheduleMode
      ? currentlySelectedType
      : selectedPeriod;
    const contextKey = getCurrentContextKey();
    const currentContextCustomers = dayWiseCustomers[contextKey] || [];

    // For daily schedules, don't show dropdown - customers are managed directly in scheduled section
    if (currentScheduleType === "daily") {
      return [];
    }

    // For non-daily schedules (weekly/monthly/fortnight)
    if (isMultiScheduleMode) {
      // Multi-schedule mode: Show ALL customers assigned to current schedule type
      // Allow manual assignment to specific days via dropdown
      const assignedToCurrentType =
        scheduleCustomerAssignments[currentScheduleType] || [];
      const allAssignedUIDs = getAllAssignedCustomerUIDs();

      return availableCustomers.filter((customer) => {
        const isAssignedToCurrentType = assignedToCurrentType.includes(
          customer.UID
        );
        const isAlreadyAssignedAnywhere = allAssignedUIDs.has(customer.UID);
        const isInCurrentContext = currentContextCustomers.some(
          (c) => c.customerUID === customer.UID
        );

        // Show if: assigned to current type AND (not assigned to any specific day OR already in current context)
        return (
          isAssignedToCurrentType &&
          (!isAlreadyAssignedAnywhere || isInCurrentContext)
        );
      });
    } else {
      // Single-schedule mode: Show ALL available customers, exclude those assigned to specific days
      const allAssignedUIDs = getAllAssignedCustomerUIDs();

      return availableCustomers.filter((customer) => {
        const isAlreadyAssignedAnywhere = allAssignedUIDs.has(customer.UID);
        const isInCurrentContext = currentContextCustomers.some(
          (c) => c.customerUID === customer.UID
        );

        // Show if: not assigned to any specific day OR already in current context (allows moving between days)
        return !isAlreadyAssignedAnywhere || isInCurrentContext;
      });
    }
  };

  // Filter customers based on search term
  const getFilteredAvailableCustomers = () => {
    const availableCustomers = getAvailableCustomersForSelection();
    if (!searchTerm) return availableCustomers;

    return availableCustomers.filter(
      (customer) =>
        customer.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        customer.Code.toLowerCase().includes(searchTerm.toLowerCase())
    );
  };

  // Toggle customer selection
  const toggleCustomerSelection = (customerUID: string) => {
    setSelectedCustomerUIDs((prev) =>
      prev.includes(customerUID)
        ? prev.filter((uid) => uid !== customerUID)
        : [...prev, customerUID]
    );
  };

  // Get the current context key based on period and selection
  const getCurrentContextKey = () => {
    const currentScheduleType = isMultiScheduleMode
      ? currentlySelectedType
      : selectedPeriod;

    switch (currentScheduleType) {
      case "daily":
        return "daily_NA_0"; // Always use daily_NA_0 for daily schedules
      case "weekly":
        return `weekly_W${selectedWeek}_${selectedDay}`;
      case "monthly":
        return `date_${selectedDate}`;
      case "fortnight":
        return `${selectedFortnightPart}_week1_${selectedDay}`; // Use week1 as default for fortnight
      default:
        return selectedDay;
    }
  };

  // Add multiple selected customers

  const periods = [
    { value: "daily", label: "Daily", icon: "☀️" },
    { value: "weekly", label: "Weekly", icon: "📅" },
    { value: "monthly", label: "Monthly", icon: "📆" },
    { value: "fortnight", label: "Fortnight", icon: "🗓️" },
  ];

  const weeks = [
    {
      id: 1,
      label: "Week 1",
      dates: "Jan 1-7",
      color: "bg-gradient-to-r from-blue-500 to-blue-600",
    },
    {
      id: 2,
      label: "Week 2",
      dates: "Jan 8-14",
      color: "bg-gradient-to-r from-purple-500 to-purple-600",
    },
    {
      id: 3,
      label: "Week 3",
      dates: "Jan 15-21",
      color: "bg-gradient-to-r from-green-500 to-green-600",
    },
    {
      id: 4,
      label: "Week 4",
      dates: "Jan 22-28",
      color: "bg-gradient-to-r from-orange-500 to-orange-600",
    },
    {
      id: 5,
      label: "Week 5",
      dates: "Jan 29-31",
      color: "bg-gradient-to-r from-pink-500 to-pink-600",
    },
  ];

  const days = [
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday",
    "Sunday",
  ];

  const monthlyDates = Array.from(
    { length: monthlyDaysCount },
    (_, i) => i + 1
  );

  const monthlyDaysOptions = Array.from({ length: 30 }, (_, i) => ({
    value: i + 1,
    label: `${i + 1} Day${i + 1 > 1 ? "s" : ""}`,
  }));

  const fortnightParts = [
    { value: "1st-3rd", label: "1st and 3rd Week", weeks: [1, 3] },
    { value: "2nd-4th", label: "2nd and 4th Week", weeks: [2, 4] },
  ];

  const priorityColors = {
    high: "bg-red-50 text-red-700 border-red-200",
    normal: "bg-blue-50 text-blue-700 border-blue-200",
    low: "bg-gray-50 text-gray-600 border-gray-200",
  };

  // Auto-save functionality
  useEffect(() => {
    if (settings.autoSave && customers.length > 0) {
      localStorage.setItem("customerSchedule", JSON.stringify(customers));
    }
  }, [customers, settings.autoSave]);

  // Commented out auto-load to prevent automatic customer creation on page load
  // useEffect(() => {
  //   const saved = localStorage.getItem("customerSchedule");
  //   if (saved) {
  //     setCustomers(JSON.parse(saved));
  //   }
  // }, []);

  // Check for time conflicts
  useEffect(() => {
    // Get customers for current period selection (ignore NA times)
    const currentCustomers = customers.filter((c) => {
      // Skip customers with NA times
      if (c.startTime === "NA" || c.endTime === "NA") return false;
      switch (selectedPeriod) {
        case "daily":
          // Show all customers across all periods for conflict checking when Daily is selected
          return true;
        case "weekly":
          return (
            c.day === selectedDay &&
            c.week === selectedWeek &&
            c.period === "weekly"
          );
        case "monthly":
          return c.date === selectedDate && c.period === "monthly";
        case "fortnight":
          return (
            c.day === selectedDay &&
            c.period === "fortnight" &&
            c.fortnightPart === selectedFortnightPart
          );
        default:
          return false;
      }
    });

    const newConflicts = [];

    currentCustomers.forEach((customer, index) => {
      const startTime = timeToMinutes(customer.startTime);
      const endTime = startTime + customer.duration;

      currentCustomers.slice(index + 1).forEach((otherCustomer) => {
        const otherStartTime = timeToMinutes(otherCustomer.startTime);
        const otherEndTime = otherStartTime + otherCustomer.duration;

        if (startTime < otherEndTime && endTime > otherStartTime) {
          newConflicts.push({
            customer1: customer.id,
            customer2: otherCustomer.id,
          });
        }
      });
    });

    setConflicts(newConflicts);
  }, [
    customers,
    selectedPeriod,
    selectedDay,
    selectedWeek,
    selectedDate,
    selectedFortnightPart,
  ]);

  const timeToMinutes = (timeStr) => {
    const [hours, minutes] = timeStr.split(":").map(Number);
    return hours * 60 + minutes;
  };

  const minutesToTime = (totalMinutes) => {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours.toString().padStart(2, "0")}:${minutes
      .toString()
      .padStart(2, "0")}`;
  };

  const getAvailableWeeks = () => {
    switch (selectedPeriod) {
      case "weekly":
        return weeks;
      case "fortnight":
        // No week selection needed for fortnightly - handled by part selection only
        return [];
      case "monthly":
      case "daily":
      default:
        return [];
    }
  };

  const getCurrentPeriodLabel = () => {
    switch (selectedPeriod) {
      case "daily":
        return "All Days";
      case "weekly":
        return `Week ${selectedWeek}`;
      case "monthly":
        return `Day ${selectedDate}`;
      case "fortnight":
        return `Week ${selectedWeek}`;
      default:
        return "";
    }
  };

  const getCurrentDaysLabel = () => {
    if (selectedPeriod === "weekly") {
      return selectedDay;
    }
    if (selectedPeriod === "fortnight") {
      return `${selectedDay} (${selectedFortnightPart})`;
    }
    return "";
  };

  const addCustomer = () => {
    if (!newCustomer.name || !newCustomer.startTime) return;

    const selectedCustomer = availableCustomers.find(
      (c) => c.Name === newCustomer.name
    );

    const endTime = calculateEndTime(
      newCustomer.startTime,
      newCustomer.duration
    );
    const customer = {
      id: Date.now(),
      sequence: customers.length + 1,
      name: newCustomer.name,
      customerCode: selectedCustomer?.Code || "",
      startTime: newCustomer.startTime,
      endTime: endTime,
      duration: newCustomer.duration,
      period: selectedPeriod,
      day:
        selectedPeriod === "weekly" || selectedPeriod === "fortnight"
          ? selectedDay
          : null,
      week:
        selectedPeriod === "weekly" || selectedPeriod === "fortnight"
          ? selectedWeek
          : null,
      date: selectedPeriod === "monthly" ? selectedDate : null,
      fortnightPart:
        selectedPeriod === "fortnight" ? selectedFortnightPart : null,
      phone: newCustomer.phone,
      location: newCustomer.location,
      priority: newCustomer.priority,
      notes: newCustomer.notes,
      createdAt: new Date().toISOString(),
    };

    setCustomers([...customers, customer]);
    setLastAction({ type: "add", customer });
    setNewCustomer({
      name: "",
      startTime: "",
      duration: getCurrentScheduleTypeSettings().defaultDuration,
      phone: "",
      location: "",
      priority: "normal",
      notes: "",
    });
    setIsAddingCustomer(false);
  };

  const calculateEndTime = (startTime, duration) => {
    const [hours, minutes] = startTime.split(":").map(Number);
    const totalMinutes = hours * 60 + minutes + duration;
    const endHours = Math.floor(totalMinutes / 60);
    const endMins = totalMinutes % 60;
    return `${endHours.toString().padStart(2, "0")}:${endMins
      .toString()
      .padStart(2, "0")}`;
  };

  const deleteCustomer = (id) => {
    console.log("🗑️ DELETE CUSTOMER - Starting deletion for ID:", id);
    const contextKey = getCurrentContextKey();
    const customerToDelete = getCustomersForCurrentContext().find(
      (c) => c.id === id
    );

    console.log("🗑️ Customer to delete:", customerToDelete);
    console.log("🗑️ Current context key:", contextKey);

    if (customerToDelete) {
      // Update day-wise customers
      const updatedDayWiseCustomers = {
        ...dayWiseCustomers,
        [contextKey]: (dayWiseCustomers[contextKey] || []).filter(
          (c) => c.id !== id
        ),
      };
      setDayWiseCustomers(updatedDayWiseCustomers);
      console.log("🗑️ Updated dayWiseCustomers:", updatedDayWiseCustomers);

      // Update flat list
      const allCustomers = Object.values(updatedDayWiseCustomers).flat();
      setCustomers(allCustomers);
      console.log("🗑️ Updated all customers count:", allCustomers.length);

      // Update customer scheduling by removing the deleted customer
      console.log(
        "🗑️ Current customerScheduling before deletion:",
        customerScheduling
      );
      const updatedScheduling = customerScheduling.filter(
        (cs) => cs.customerUID !== customerToDelete.customerUID
      );
      setCustomerScheduling(updatedScheduling);
      console.log(
        "🗑️ Updated customerScheduling after deletion:",
        updatedScheduling
      );
      console.log(
        "🗑️ CustomerScheduling count: before =",
        customerScheduling.length,
        "after =",
        updatedScheduling.length
      );

      // Update schedule customer assignments
      const updatedAssignments = { ...scheduleCustomerAssignments };
      Object.keys(updatedAssignments).forEach((scheduleType) => {
        const beforeCount = updatedAssignments[scheduleType].length;
        updatedAssignments[scheduleType] = updatedAssignments[
          scheduleType
        ].filter((uid) => uid !== customerToDelete.customerUID);
        const afterCount = updatedAssignments[scheduleType].length;
        console.log(
          `🗑️ Schedule ${scheduleType}: removed ${
            beforeCount - afterCount
          } customers`
        );
      });
      setScheduleCustomerAssignments(updatedAssignments);
      console.log(
        "🗑️ Updated scheduleCustomerAssignments:",
        updatedAssignments
      );

      // Update assigned customers set
      const newAssignedCustomers = new Set(assignedCustomers);
      newAssignedCustomers.delete(customerToDelete.customerUID);
      setAssignedCustomers(newAssignedCustomers);
      console.log(
        "🗑️ Updated assigned customers count:",
        newAssignedCustomers.size
      );

      // Notify parent component of the change
      if (onCustomerSchedulingChange) {
        console.log(
          "🗑️ CALLING onCustomerSchedulingChange with",
          updatedScheduling.length,
          "entries"
        );
        onCustomerSchedulingChange(updatedScheduling);
      } else {
        console.log("⚠️ onCustomerSchedulingChange callback is not defined!");
      }

      // Notify parent about the updated scheduled customers
      if (onCustomersScheduled) {
        console.log(
          "🗑️ CALLING onCustomersScheduled with",
          allCustomers.length,
          "customers"
        );
        onCustomersScheduled(allCustomers);
      } else {
        console.log("⚠️ onCustomersScheduled callback is not defined!");
      }

      setLastAction({
        type: "delete",
        customer: customerToDelete,
        context: contextKey,
      });
      console.log("🗑️ DELETE CUSTOMER - Completed successfully");
    } else {
      console.log("⚠️ DELETE CUSTOMER - Customer not found with ID:", id);
    }
  };

  const updateCustomer = (id, updatedData) => {
    const oldCustomer = customers.find((c) => c.id === id);

    // Update customers list
    const updatedCustomers = customers.map((c) =>
      c.id === id
        ? {
            ...c,
            ...updatedData,
            endTime: calculateEndTime(
              updatedData.startTime,
              updatedData.duration
            ),
          }
        : c
    );
    setCustomers(updatedCustomers);

    // Update customer scheduling if needed
    const updatedCustomer = updatedCustomers.find((c) => c.id === id);
    if (updatedCustomer) {
      const updatedScheduling = customerScheduling.map((cs) => {
        if (cs.customerUID === updatedCustomer.customerUID) {
          // Update the scheduling entry with new data if needed
          return {
            ...cs,
            // Add any relevant fields that need updating
          };
        }
        return cs;
      });
      setCustomerScheduling(updatedScheduling);

      // Notify parent component of the change
      if (onCustomerSchedulingChange) {
        onCustomerSchedulingChange(updatedScheduling);
      }

      // Notify parent about the updated scheduled customers
      if (onCustomersScheduled) {
        onCustomersScheduled(updatedCustomers);
      }
    }

    setLastAction({ type: "update", oldCustomer, newCustomer: updatedData });
    setEditingCustomer(null);
  };

  const undoLastAction = () => {
    if (!lastAction) return;

    switch (lastAction.type) {
      case "add":
        setCustomers(customers.filter((c) => c.id !== lastAction.customer.id));
        break;
      case "delete":
        setCustomers([...customers, lastAction.customer]);
        break;
      case "update":
        setCustomers(
          customers.map((c) =>
            c.id === lastAction.oldCustomer.id ? lastAction.oldCustomer : c
          )
        );
        break;
    }
    setLastAction(null);
  };

  // Get customers for current context
  const getCustomersForCurrentContext = () => {
    const contextKey = getCurrentContextKey();
    return dayWiseCustomers[contextKey] || [];
  };

  const filteredCustomers = getCustomersForCurrentContext()
    .filter((c) => {
      // Filter by search term
      return c.name.toLowerCase().includes(searchTerm.toLowerCase());
    })
    .sort((a, b) => timeToMinutes(a.startTime) - timeToMinutes(b.startTime));

  const isConflicted = (customerId) => {
    return conflicts.some(
      (conflict) =>
        conflict.customer1 === customerId || conflict.customer2 === customerId
    );
  };

  const QuickAddForm = () => (
    <div className={`bg-white border-gray-200 border rounded-lg p-6 mb-6`}>
      <div className="flex items-center gap-3 mb-6">
        <div className="p-2 bg-gradient-to-r from-blue-500 to-purple-600 rounded-lg">
          <Plus className="w-5 h-5 text-white" />
        </div>
        <h3 className={`text-lg font-semibold ${"text-gray-900"}`}>
          Add New Customer
        </h3>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
        <div className="space-y-2">
          <label className={`text-sm font-medium ${"text-gray-700"}`}>
            Select Customer *
          </label>
          <select
            value={newCustomer.name}
            onChange={(e) => {
              const selectedCustomer = availableCustomers.find(
                (c) => c.Name === e.target.value
              );
              setNewCustomer({
                ...newCustomer,
                name: e.target.value,
                phone: selectedCustomer?.ContactNo || "",
                location: selectedCustomer?.Address || "",
                notes: "",
              });
            }}
            className="w-full px-4 py-3 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
          >
            <option value="">Select a customer...</option>
            {getAvailableCustomersForSelection().map((customer) => (
              <option key={customer.UID} value={customer.Name}>
                {customer.Name} ({customer.Code})
              </option>
            ))}
          </select>
          {getAvailableCustomersForSelection().length === 0 && (
            <p className="text-sm text-gray-500 italic">
              All selected customers have been added to the schedule
            </p>
          )}
        </div>

        <div className="space-y-2">
          <label className={`text-sm font-medium ${"text-gray-700"}`}>
            Start Time *
            <span className="text-xs text-gray-500 ml-2">
              (Default for{" "}
              {isMultiScheduleMode ? currentlySelectedType : selectedPeriod}:{" "}
              {getCurrentScheduleTypeSettings().dayStart})
            </span>
          </label>
          <input
            type="time"
            value={newCustomer.startTime}
            onChange={(e) =>
              setNewCustomer({ ...newCustomer, startTime: e.target.value })
            }
            placeholder={getCurrentScheduleTypeSettings().dayStart}
            className="w-full px-4 py-3 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
          />
        </div>

        <div className="space-y-2">
          <label className={`text-sm font-medium ${"text-gray-700"}`}>
            Duration
            <span className="text-xs text-gray-500 ml-2">
              (Default: {getCurrentScheduleTypeSettings().defaultDuration}{" "}
              minutes)
            </span>
          </label>
          <select
            value={newCustomer.duration}
            onChange={(e) =>
              setNewCustomer({
                ...newCustomer,
                duration: parseInt(e.target.value),
              })
            }
            className="w-full px-4 py-3 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
          >
            <option value={15}>15 minutes</option>
            <option value={30}>30 minutes</option>
            <option value={45}>45 minutes</option>
            <option value={60}>1 hour</option>
            <option value={90}>1.5 hours</option>
            <option value={120}>2 hours</option>
          </select>
        </div>

        <div className="space-y-2">
          <label className={`text-sm font-medium ${"text-gray-700"}`}>
            Phone Number
          </label>
          <input
            type="tel"
            placeholder="(555) 123-4567"
            value={newCustomer.phone}
            onChange={(e) =>
              setNewCustomer({ ...newCustomer, phone: e.target.value })
            }
            className="w-full px-4 py-3 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
          />
        </div>

        <div className="space-y-2">
          <label className={`text-sm font-medium ${"text-gray-700"}`}>
            Priority
          </label>
          <select
            value={newCustomer.priority}
            onChange={(e) =>
              setNewCustomer({ ...newCustomer, priority: e.target.value })
            }
            className="w-full px-4 py-3 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
          >
            <option value="low">Low Priority</option>
            <option value="normal">Normal Priority</option>
            <option value="high">High Priority</option>
          </select>
        </div>
      </div>

      <div className="space-y-2 mb-6">
        <label className={`text-sm font-medium ${"text-gray-700"}`}>
          Notes
        </label>
        <textarea
          placeholder="Additional notes or requirements..."
          value={newCustomer.notes}
          onChange={(e) =>
            setNewCustomer({ ...newCustomer, notes: e.target.value })
          }
          rows={3}
          className="w-full px-4 py-3 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none bg-white border-gray-300 text-gray-900"
        />
      </div>

      <div className="flex gap-3">
        <button
          type="button"
          onClick={addCustomer}
          disabled={!newCustomer.name || !newCustomer.startTime}
          className="flex-1 bg-gradient-to-r from-blue-500 to-purple-600 text-white px-6 py-3 rounded-lg hover:from-blue-600 hover:to-purple-700 disabled:from-gray-400 disabled:to-gray-500 disabled:cursor-not-allowed transition-all duration-200 transform hover:scale-105 disabled:hover:scale-100 font-medium"
        >
          Add Customer
        </button>
        <button
          type="button"
          onClick={() => setIsAddingCustomer(false)}
          className="px-6 py-3 rounded-lg border transition-all duration-200 font-medium border-gray-300 text-gray-700 hover:bg-gray-50"
        >
          Cancel
        </button>
      </div>
    </div>
  );

  // Save time settings for a customer
  // Customer Row Component
  // Handle drag and drop for reordering
  const handleDragStart = (
    e: React.DragEvent,
    customer: ScheduledCustomer,
    index: number
  ) => {
    setDraggedCustomer(customer);
    e.dataTransfer.effectAllowed = "move";
    // Add a drag image for better visual feedback
    const dragImage = e.currentTarget.cloneNode(true) as HTMLElement;
    dragImage.style.opacity = "0.5";
    document.body.appendChild(dragImage);
    e.dataTransfer.setDragImage(
      dragImage,
      e.clientX - e.currentTarget.getBoundingClientRect().left,
      20
    );
    setTimeout(() => document.body.removeChild(dragImage), 0);
  };

  const handleDragOver = (e: React.DragEvent, index: number) => {
    e.preventDefault();
    e.stopPropagation();
    e.dataTransfer.dropEffect = "move";

    // Only update if actually over a different item
    if (draggedCustomer && customers[index]?.id !== draggedCustomer.id) {
      setDragOverIndex(index);
    }
  };

  const handleDragEnter = (e: React.DragEvent, index: number) => {
    e.preventDefault();
    if (draggedCustomer && customers[index]?.id !== draggedCustomer.id) {
      setDragOverIndex(index);
    }
  };

  const handleDragLeave = (e: React.DragEvent) => {
    // Only clear if we're actually leaving the element
    const relatedTarget = e.relatedTarget as HTMLElement;
    if (!e.currentTarget.contains(relatedTarget)) {
      setDragOverIndex(null);
    }
  };

  const handleDrop = (e: React.DragEvent, dropIndex: number) => {
    e.preventDefault();
    e.stopPropagation();
    setDragOverIndex(null);

    if (draggedCustomer && customers[dropIndex]) {
      const contextKey = getCurrentContextKey();
      const currentCustomers = [...customers];

      // Find the original index
      const draggedIndex = currentCustomers.findIndex(
        (c) => c.id === draggedCustomer.id
      );

      if (draggedIndex !== -1 && draggedIndex !== dropIndex) {
        // Remove dragged item
        const [removed] = currentCustomers.splice(draggedIndex, 1);

        // Insert at new position
        currentCustomers.splice(dropIndex, 0, removed);

        // Update sequence numbers
        const updatedCustomers = currentCustomers.map((c, idx) => ({
          ...c,
          sequence: idx + 1,
        }));

        // Update both local state and dayWiseCustomers
        setCustomers(updatedCustomers);
        setDayWiseCustomers((prev) => ({
          ...prev,
          [contextKey]: updatedCustomers,
        }));

        // Show success feedback
        console.log(
          `✅ Moved customer from position ${draggedIndex + 1} to ${
            dropIndex + 1
          }`
        );
      }
    }
    setDraggedCustomer(null);
  };

  const handleDragEnd = () => {
    // Clean up any drag state
    setDraggedCustomer(null);
    setDragOverIndex(null);
  };

  const CustomerRow = ({
    customer,
    index,
    isConflicted,
    expandedCustomerId,
    setExpandedCustomerId,
    saveTimeSettings,
    deleteCustomer,
  }) => {
    const isExpanded = expandedCustomerId === customer.id;
    const [tempStartTime, setTempStartTime] = useState(
      customer.startTime || ""
    );
    const [tempDuration, setTempDuration] = useState(customer.duration || 0);
    const [tempTravelTime, setTempTravelTime] = useState(
      customer.travelTime || 0
    );

    return (
      <div
        draggable="true"
        onDragStart={(e) => handleDragStart(e, customer, index)}
        onDragEnter={(e) => handleDragEnter(e, index)}
        onDragOver={(e) => handleDragOver(e, index)}
        onDragLeave={handleDragLeave}
        onDrop={(e) => handleDrop(e, index)}
        onDragEnd={handleDragEnd}
        className={`relative transition-all duration-200 ${
          draggedCustomer?.id === customer.id ? "opacity-40" : ""
        }`}
      >
        {/* Drop indicator line */}
        {dragOverIndex === index && (
          <div className="absolute top-0 left-0 right-0 h-1 bg-blue-500 animate-pulse" />
        )}

        <div
          className={`px-4 py-3 transition-all cursor-move ${
            isConflicted(customer.id)
              ? "bg-red-50 border-l-4 border-red-400"
              : draggedCustomer
              ? "hover:bg-blue-50"
              : "hover:bg-gray-50"
          }`}
        >
          <div className="grid grid-cols-11 gap-2 items-center">
            <div className="col-span-1 flex items-center gap-2">
              <div
                className="cursor-move hover:bg-gray-200 rounded p-1 transition-colors"
                title="Drag to reorder"
              >
                <GripVertical className="w-4 h-4 text-gray-400" />
              </div>
              <span className="text-sm text-gray-500 font-medium">
                {index + 1}
              </span>
            </div>
            <div className="col-span-3">
              <div className="font-medium text-gray-900">{customer.name}</div>
              {customer.customerCode && (
                <div className="text-sm text-gray-500">
                  Code: {customer.customerCode}
                </div>
              )}
              {customer.notes && (
                <div className="text-xs text-gray-400 mt-1">
                  {customer.notes.substring(0, 50)}
                  {customer.notes.length > 50 ? "..." : ""}
                </div>
              )}
            </div>

            <div className="text-sm text-gray-600 col-span-2">
              <div className="flex items-center gap-1">
                <Clock className="w-3 h-3" />
                {customer.startTime === "NA" ? (
                  <span className="text-gray-400">NA</span>
                ) : (
                  `${customer.startTime} - ${customer.endTime}`
                )}
              </div>
              {customer.travelTime > 0 && (
                <div className="text-xs text-gray-500 mt-1">
                  +{customer.travelTime} min travel
                </div>
              )}
            </div>

            <div className="text-sm text-gray-600 col-span-2">
              {customer.duration === 0 ? "NA" : `${customer.duration} min`}
            </div>

            <div className="col-span-1">
              <span
                className={`px-2 py-0.5 text-xs rounded border ${
                  priorityColors[customer.priority]
                }`}
              >
                {customer.priority}
              </span>
            </div>

            <div className="flex gap-1 col-span-2">
              <button
                type="button"
                onClick={() =>
                  setExpandedCustomerId(
                    expandedCustomerId === customer.id ? null : customer.id
                  )
                }
                className="p-1.5 rounded text-gray-600 hover:bg-gray-100"
                title="Set Time"
              >
                <Clock className="w-4 h-4" />
              </button>

              <button
                type="button"
                onClick={() => deleteCustomer(customer.id)}
                className="p-1.5 rounded text-red-600 hover:bg-red-50"
                title="Delete"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        {/* Expandable Time Settings */}
        {isExpanded && (
          <div className="px-4 py-3 bg-gray-50 border-t border-gray-200">
            <div className="flex items-center gap-4">
              <div className="flex-1">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Start Time
                </label>
                <input
                  type="time"
                  value={tempStartTime}
                  onChange={(e) => setTempStartTime(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              <div className="flex-1">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Duration (minutes)
                </label>
                <input
                  type="number"
                  min="5"
                  max="240"
                  step="5"
                  value={tempDuration}
                  onChange={(e) =>
                    setTempDuration(parseInt(e.target.value) || 30)
                  }
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              <div className="flex-1">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  End Time
                </label>
                <div className="px-3 py-2 bg-white border border-gray-300 rounded-lg text-gray-700">
                  {calculateEndTime(tempStartTime, tempDuration)}
                </div>
              </div>

              <div className="flex-1">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Travel Time (minutes)
                </label>
                <input
                  type="number"
                  min="0"
                  max="120"
                  step="5"
                  value={tempTravelTime}
                  onChange={(e) =>
                    setTempTravelTime(parseInt(e.target.value) || 0)
                  }
                  placeholder="N/A"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
                <div className="text-xs text-gray-500 mt-1">
                  Time from previous customer
                </div>
              </div>

              <div className="flex gap-2 pt-6">
                <button
                  type="button"
                  onClick={() =>
                    saveTimeSettings(
                      customer.id,
                      tempStartTime,
                      tempDuration,
                      tempTravelTime
                    )
                  }
                  className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
                >
                  Save
                </button>
                <button
                  type="button"
                  onClick={() => setExpandedCustomerId(null)}
                  className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    );
  };

  const saveTimeSettings = (
    customerId,
    startTime,
    duration,
    travelTime = 0
  ) => {
    const contextKey = getCurrentContextKey();
    const updatedCustomers = (dayWiseCustomers[contextKey] || []).map((c) => {
      if (c.id === customerId) {
        return {
          ...c,
          startTime: startTime,
          endTime: calculateEndTime(startTime, duration),
          duration: duration,
          travelTime: travelTime,
        };
      }
      return c;
    });

    const updatedDayWiseCustomers = {
      ...dayWiseCustomers,
      [contextKey]: updatedCustomers,
    };
    setDayWiseCustomers(updatedDayWiseCustomers);

    const allCustomers = Object.values(updatedDayWiseCustomers).flat();
    setCustomers(allCustomers);

    setExpandedCustomerId(null); // Close the expanded row
  };

  // Show toast notification
  const showToast = (
    message: string,
    type: "success" | "error" | "info" = "success"
  ) => {
    setToast({ show: true, message, type });
    setTimeout(() => {
      setToast({ show: false, message: "", type: "success" });
    }, 3000);
  };

  // Close modal when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (showCopyModal) {
        const modalElement = document.getElementById("copy-modal");
        if (modalElement && !modalElement.contains(event.target as Node)) {
          setShowCopyModal(false);
          setSelectedTargetDays([]);
          setSelectedTargetDates([]);
        }
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [showCopyModal]);

  // Copy customers from current day to other days and/or weeks (Weekly)
  const copyToOtherDaysAndWeeks = (targetDays: string[], targetWeeks: number[]) => {
    const contextKey = getCurrentContextKey();
    const sourceCustomers = dayWiseCustomers[contextKey] || [];

    if (sourceCustomers.length === 0) {
      showToast("No customers to copy!", "error");
      return;
    }

    const updatedDayWiseCustomers = { ...dayWiseCustomers };
    const updatedCustomerScheduling = [...customerScheduling];

    // Copy to other weeks (same day)
    targetWeeks.forEach((targetWeek) => {
      const targetContextKey = `weekly_W${targetWeek}_${selectedDay}`;

      // Copy customers to target week
      const copiedCustomers = sourceCustomers.map((customer) => ({
        ...customer,
        id: Date.now() + Math.random(),
        week: targetWeek,
        createdAt: new Date().toISOString(),
      }));

      updatedDayWiseCustomers[targetContextKey] = copiedCustomers;

      // Update customer scheduling for each week
      copiedCustomers.forEach((customer) => {
        if (customer.customerUID) {
          const existingIndex = updatedCustomerScheduling.findIndex(
            (cs) => cs.customerUID === customer.customerUID
          );

          const newScheduleConfig = {
            scheduleType: "weekly",
            weekNumber: targetWeek,
            dayNumber: getDayNumber(selectedDay),
          };

          if (existingIndex !== -1) {
            if (!updatedCustomerScheduling[existingIndex].scheduleConfigs.some(
              config => config.scheduleType === "weekly" &&
              config.weekNumber === targetWeek &&
              config.dayNumber === getDayNumber(selectedDay)
            )) {
              updatedCustomerScheduling[existingIndex].scheduleConfigs.push(newScheduleConfig);
            }
          } else {
            updatedCustomerScheduling.push({
              customerUID: customer.customerUID,
              frequency: "weekly",
              scheduleConfigs: [newScheduleConfig],
            });
          }
        }
      });
    });

    // Copy to other days (current week)
    targetDays.forEach((targetDay) => {
      const targetContextKey = `weekly_W${selectedWeek}_${targetDay}`;

      // Copy customers to target day
      const copiedCustomers = sourceCustomers.map((customer) => ({
        ...customer,
        id: Date.now() + Math.random(),
        day: targetDay,
        createdAt: new Date().toISOString(),
      }));

      updatedDayWiseCustomers[targetContextKey] = copiedCustomers;

      // Update customer scheduling
      copiedCustomers.forEach((customer) => {
        if (customer.customerUID) {
          const existingIndex = updatedCustomerScheduling.findIndex(
            (cs) => cs.customerUID === customer.customerUID
          );

          const newScheduleConfig = {
            scheduleType: "weekly",
            weekNumber: selectedWeek,
            dayNumber: days.indexOf(targetDay) + 1,
          };

          if (existingIndex >= 0) {
            // Add to existing schedule configs
            updatedCustomerScheduling[existingIndex].scheduleConfigs.push(
              newScheduleConfig
            );
          } else {
            // Create new scheduling entry
            updatedCustomerScheduling.push({
              customerUID: customer.customerUID,
              frequency: "weekly",
              scheduleConfigs: [newScheduleConfig],
            });
          }
        }
      });
    });

    setDayWiseCustomers(updatedDayWiseCustomers);
    setCustomerScheduling(updatedCustomerScheduling);
    showToast(
      `Successfully copied ${sourceCustomers.length} customers to ${targetDays.length} days`,
      "success"
    );
  };

  // Copy customers from current date to other dates (Monthly)
  const copyToOtherDates = (targetDates: number[]) => {
    const contextKey = getCurrentContextKey();
    const sourceCustomers = dayWiseCustomers[contextKey] || [];

    if (sourceCustomers.length === 0) {
      showToast("No customers to copy!", "error");
      return;
    }

    const updatedDayWiseCustomers = { ...dayWiseCustomers };
    const updatedCustomerScheduling = [...customerScheduling];

    targetDates.forEach((targetDate) => {
      const targetContextKey = `date_${targetDate}`;

      // Copy customers to target date
      const copiedCustomers = sourceCustomers.map((customer) => ({
        ...customer,
        id: Date.now() + Math.random(),
        date: targetDate,
        createdAt: new Date().toISOString(),
      }));

      updatedDayWiseCustomers[targetContextKey] = copiedCustomers;

      // Update customer scheduling
      copiedCustomers.forEach((customer) => {
        if (customer.customerUID) {
          const existingIndex = updatedCustomerScheduling.findIndex(
            (cs) => cs.customerUID === customer.customerUID
          );

          const newScheduleConfig = {
            scheduleType: "monthly",
            dayNumber: targetDate,
          };

          if (existingIndex >= 0) {
            // Add to existing schedule configs
            updatedCustomerScheduling[existingIndex].scheduleConfigs.push(
              newScheduleConfig
            );
          } else {
            // Create new scheduling entry
            updatedCustomerScheduling.push({
              customerUID: customer.customerUID,
              frequency: "monthly",
              scheduleConfigs: [newScheduleConfig],
            });
          }
        }
      });
    });

    setDayWiseCustomers(updatedDayWiseCustomers);
    setCustomerScheduling(updatedCustomerScheduling);
    showToast(
      `Successfully copied ${sourceCustomers.length} customers to ${targetDates.length} dates`,
      "success"
    );
  };

  // Copy customers for Fortnight schedules
  const copyToOtherDaysAndFortnightParts = (targetParts: string[], targetDays: string[]) => {
    const contextKey = getCurrentContextKey();
    const sourceCustomers = dayWiseCustomers[contextKey] || [];

    if (sourceCustomers.length === 0) {
      showToast("No customers to copy!", "error");
      return;
    }

    const updatedDayWiseCustomers = { ...dayWiseCustomers };
    const updatedCustomerScheduling = [...customerScheduling];

    // Copy to other fortnight parts (different weeks pattern)
    targetParts.forEach((targetPart) => {
      // For the same day in different fortnight part - use the same format as getCurrentContextKey
      const targetContextKey = `${targetPart}_week1_${selectedDay}`;

      // Copy customers to target fortnight part
      const copiedCustomers = sourceCustomers.map((customer) => ({
        ...customer,
        id: Date.now() + Math.random(),
        fortnightPart: targetPart,
        createdAt: new Date().toISOString(),
      }));

      updatedDayWiseCustomers[targetContextKey] = copiedCustomers;

      // Update customer scheduling for fortnight
      copiedCustomers.forEach((customer) => {
        if (customer.customerUID) {
          const existingIndex = updatedCustomerScheduling.findIndex(
            (cs) => cs.customerUID === customer.customerUID
          );

          // Determine week number based on fortnight part
          const weekNumber = targetPart === "1st-3rd" ? 13 : 24;

          const newScheduleConfig = {
            scheduleType: "fortnight",
            weekNumber: weekNumber,
            dayNumber: getDayNumber(selectedDay),
          };

          if (existingIndex >= 0) {
            // Add to existing schedule configs
            updatedCustomerScheduling[existingIndex].scheduleConfigs.push(
              newScheduleConfig
            );
          } else {
            // Create new scheduling entry
            updatedCustomerScheduling.push({
              customerUID: customer.customerUID,
              frequency: "fortnight",
              scheduleConfigs: [newScheduleConfig],
            });
          }
        }
      });
    });

    // Copy to other days within the same fortnight part
    targetDays.forEach((targetDay) => {
      const targetContextKey = `${selectedFortnightPart}_week1_${targetDay}`;

      // Skip if it's the same day we're copying from
      if (targetDay === selectedDay) return;

      // Copy customers to target day
      const copiedCustomers = sourceCustomers.map((customer) => ({
        ...customer,
        id: Date.now() + Math.random(),
        day: targetDay,
        createdAt: new Date().toISOString(),
      }));

      updatedDayWiseCustomers[targetContextKey] = copiedCustomers;

      // Update customer scheduling
      copiedCustomers.forEach((customer) => {
        if (customer.customerUID) {
          const existingIndex = updatedCustomerScheduling.findIndex(
            (cs) => cs.customerUID === customer.customerUID
          );

          // Determine week number based on current fortnight part
          const weekNumber = selectedFortnightPart === "1st-3rd" ? 13 : 24;

          const newScheduleConfig = {
            scheduleType: "fortnight",
            weekNumber: weekNumber,
            dayNumber: getDayNumber(targetDay),
          };

          if (existingIndex >= 0) {
            // Add to existing schedule configs
            updatedCustomerScheduling[existingIndex].scheduleConfigs.push(
              newScheduleConfig
            );
          } else {
            // Create new scheduling entry
            updatedCustomerScheduling.push({
              customerUID: customer.customerUID,
              frequency: "fortnight",
              scheduleConfigs: [newScheduleConfig],
            });
          }
        }
      });
    });

    setDayWiseCustomers(updatedDayWiseCustomers);
    setCustomerScheduling(updatedCustomerScheduling);

    const totalCopied = targetParts.length + targetDays.filter(d => d !== selectedDay).length;
    showToast(
      `Successfully copied ${sourceCustomers.length} customers to ${totalCopied} schedule(s)`,
      "success"
    );
  };

  // Auto assign times to all customers in current schedule with incremental time slots
  const autoAssignTimes = (
    startTime: string,
    duration: number,
    gap: number
  ) => {
    const contextKey = getCurrentContextKey();
    const currentCustomers = dayWiseCustomers[contextKey] || [];

    if (currentCustomers.length === 0) {
      showToast("No customers assigned to current schedule", "error");
      return;
    }

    console.log(
      `🕐 Auto assigning times starting from ${startTime}, duration: ${duration}min, gap: ${gap}min`
    );

    // Calculate time for each customer
    let currentTimeMinutes = timeToMinutes(startTime);

    const updatedCustomers = currentCustomers.map((customer, index) => {
      const customerStartTime = minutesToTime(currentTimeMinutes);
      const customerEndTime = calculateEndTime(customerStartTime, duration);

      console.log(
        `🕐 Customer ${index + 1} (${
          customer.name
        }): ${customerStartTime} - ${customerEndTime}`
      );

      // Move time forward for next customer (duration + gap)
      currentTimeMinutes += duration + gap;

      return {
        ...customer,
        startTime: customerStartTime,
        endTime: customerEndTime,
        duration: duration,
      };
    });

    // Update the day wise customers
    const updatedDayWiseCustomers = {
      ...dayWiseCustomers,
      [contextKey]: updatedCustomers,
    };

    setDayWiseCustomers(updatedDayWiseCustomers);

    // Update all customers
    const allCustomers = Object.values(updatedDayWiseCustomers).flat();
    setCustomers(allCustomers);

    console.log(
      `✅ Auto assigned times to ${updatedCustomers.length} customers`
    );
    showToast(
      `Successfully assigned times to ${updatedCustomers.length} customers`,
      "success"
    );
  };

  return (
    <>
      <div className={`min-h-screen bg-gray-50 ${showCopyModal ? "blur-sm" : ""}`}>
        <div className="w-full p-4 space-y-3">

        {/* Show warning if no customers are available */}
        {availableCustomers.length === 0 && (
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
            <div className="flex items-start gap-3">
              <AlertTriangle className="w-5 h-5 text-yellow-600 mt-0.5" />
              <div>
                <h4 className="text-sm font-medium text-yellow-900">
                  No Customers Available for Scheduling
                </h4>
                <p className="text-sm text-yellow-700 mt-1">
                  Please go back to Step 2 and select customers before configuring schedules.
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Scheduling Controls */}
        <div className="bg-white rounded-lg border border-gray-200">
          {/* Row 1: Visit Frequency and Time Settings Button */}
          <div className="p-4 border-b border-gray-200">
            <div className="flex justify-between items-center gap-4">
              {/* Multi-Schedule Selection (only in multi-schedule mode) OR Single Schedule Display */}
              {isMultiScheduleMode ? (
                <div className="flex-1">
                  {/* Schedule Types - Row Format */}
                  <div className="mb-4">
                    <div className="flex items-center justify-between mb-3">
                      <h3 className="text-sm font-medium text-gray-900">
                        Schedule Types for this Route
                      </h3>
                      <span className="text-xs text-gray-500">
                        {
                          Object.keys(scheduleCustomerAssignments).filter(
                            (type) =>
                              (scheduleCustomerAssignments[type] || []).length >
                              0
                          ).length
                        }{" "}
                        frequency types selected
                      </span>
                    </div>

                    <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
                      {Object.keys(scheduleCustomerAssignments)
                        .filter((scheduleType) => {
                          const assignedCustomerUIDs =
                            scheduleCustomerAssignments[scheduleType] || [];
                          return assignedCustomerUIDs.length > 0; // Only show frequencies with assigned customers
                        })
                        .map((scheduleType) => {
                          const period = periods.find(
                            (p) => p.value === scheduleType
                          );
                          const assignedCustomerUIDs =
                            scheduleCustomerAssignments[scheduleType] || [];
                          const isSelected =
                            currentlySelectedType === scheduleType;
                          const hasCustomers = assignedCustomerUIDs.length > 0;

                          return (
                            <button
                              key={scheduleType}
                              type="button"
                              onClick={() =>
                                setCurrentlySelectedType(scheduleType)
                              }
                              className={`px-3 py-2 rounded-lg border text-left transition-all duration-200 hover:shadow-md relative ${
                                isSelected
                                  ? "bg-blue-600 text-white border-blue-600 shadow-lg"
                                  : hasCustomers
                                  ? "bg-blue-50 text-blue-700 border-blue-200 hover:bg-blue-100"
                                  : "bg-white text-gray-700 border-gray-200 hover:border-blue-200 hover:bg-blue-50"
                              }`}
                            >
                              <div className="flex items-center gap-2 mb-1">
                                <span className="text-sm">{period?.icon}</span>
                                <div className="font-semibold text-xs">
                                  {period?.label}
                                </div>
                              </div>
                              <div
                                className={`text-xs ${
                                  isSelected ? "text-blue-100" : "text-gray-500"
                                }`}
                              >
                                {period?.value === "daily"
                                  ? "Every day"
                                  : period?.value === "weekly"
                                  ? "week"
                                  : period?.value === "monthly"
                                  ? "month"
                                  : period?.value === "fortnight"
                                  ? "Bi-weekly"
                                  : ""}
                              </div>

                              {/* Customer count badge - Show unique customers only */}
                              {hasCustomers && (
                                <div
                                  className={`absolute top-1 right-1 px-1.5 py-0.5 rounded-full text-xs font-medium ${
                                    isSelected
                                      ? "bg-white text-blue-600"
                                      : "bg-blue-500 text-white"
                                  }`}
                                >
                                  {new Set(assignedCustomerUIDs).size}
                                </div>
                              )}

                              {/* Active indicator */}
                              {isSelected && (
                                <div className="absolute bottom-1 right-1">
                                  <div className="w-1.5 h-1.5 bg-white rounded-full"></div>
                                </div>
                              )}
                            </button>
                          );
                        })}
                    </div>
                  </div>

                  {/* Currently Configuring */}
                  <div className="text-sm text-gray-600 mb-2">
                    Currently configuring:{" "}
                    <span className="font-medium text-blue-600">
                      {
                        periods.find((p) => p.value === currentlySelectedType)
                          ?.label
                      }
                    </span>
                  </div>
                </div>
              ) : (
                /* Single Schedule Display - when propSelectedPeriod is provided */
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Visit Frequency
                  </label>
                  <div className="flex items-center">
                    <span className="px-4 py-2 bg-blue-500 text-white rounded-lg text-sm font-medium">
                      {periods.find((p) => p.value === selectedPeriod)?.label ||
                        selectedPeriod}
                    </span>
                  </div>
                </div>
              )}

              {/* Show selected period from Customer Selection step */}
              {propSelectedPeriod && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Visit Frequency
                  </label>
                  <div className="flex items-center">
                    <span className="px-4 py-2 bg-blue-500 text-white rounded-lg text-sm font-medium">
                      {periods.find((p) => p.value === selectedPeriod)?.label ||
                        selectedPeriod}
                    </span>
                  </div>
                </div>
              )}

              {/* Time Settings Toggle Button */}
              <div className="flex-shrink-0">
                <button
                  type="button"
                  onClick={() => setShowSettings(!showSettings)}
                  className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                    showSettings
                      ? "bg-blue-600 text-white shadow-md"
                      : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                  }`}
                >
                  <Clock className="w-4 h-4" />
                  Time Settings
                </button>
              </div>
            </div>
          </div>

          {/* Row 2: Week and Day Selection (Period-specific controls) */}
          <div className="p-4">
            {/* Weekly: Week and Day Selection */}
            {selectedPeriod === "weekly" && (
              <div className="space-y-4">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <label className="block text-sm font-medium text-gray-700">
                      Select Week
                    </label>
                    <div className="flex gap-2">
                      {getAvailableWeeks().map((week) => (
                        <button
                          type="button"
                          key={week.id}
                          onClick={() => setSelectedWeek(week.id)}
                          className={`px-3 py-2 rounded-lg text-sm font-medium transition-all border ${
                            selectedWeek === week.id
                              ? "bg-blue-500 text-white border-blue-500"
                              : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50"
                          }`}
                        >
                          Week {week.id}
                        </button>
                      ))}
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <label className="block text-sm font-medium text-gray-700">
                        Select Day
                      </label>
                      {(
                        dayWiseCustomers[
                          `weekly_W${selectedWeek}_${selectedDay}`
                        ] || []
                      ).length > 0 && (
                        <button
                          type="button"
                          onClick={() => setShowCopyModal(true)}
                          className="flex items-center gap-1 px-2 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                        >
                          <Copy className="w-3 h-3" />
                          Copy to other days
                        </button>
                      )}
                    </div>
                    <div className="grid grid-cols-7 gap-1">
                      {days.map((day) => {
                        const contextKey = `weekly_W${selectedWeek}_${day}`;
                        const customerCount = (
                          dayWiseCustomers[contextKey] || []
                        ).length;

                        // Debug logging for day counts
                        if (day === "Monday" && selectedWeek === 1) {
                          console.log("🔍 Monday W1 Debug:", {
                            contextKey,
                            customerCount,
                            customersInContext: dayWiseCustomers[contextKey],
                            allDayWiseKeys: Object.keys(dayWiseCustomers),
                            selectedWeek,
                            selectedDay,
                          });
                        }

                        return (
                          <button
                            key={day}
                            type="button"
                            onClick={() => setSelectedDay(day)}
                            className={`px-2 py-2 rounded-lg text-xs font-medium transition-all border relative ${
                              selectedDay === day
                                ? "bg-blue-500 text-white border-blue-500"
                                : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50"
                            }`}
                          >
                            {day.substring(0, 3)}
                            {customerCount > 0 && (
                              <span
                                className={`absolute -top-1 -right-1 text-[10px] px-1.5 py-0.5 rounded-full font-medium ${
                                  selectedDay === day
                                    ? "bg-white text-blue-600"
                                    : "bg-blue-500 text-white"
                                }`}
                              >
                                {customerCount}
                              </span>
                            )}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Fortnightly: Part and Day Selection */}
            {selectedPeriod === "fortnight" && (
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label className="block text-sm font-medium text-gray-700">
                    Select Fortnight Part
                  </label>
                  <select
                    value={selectedFortnightPart}
                    onChange={(e) => setSelectedFortnightPart(e.target.value)}
                    className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm bg-white text-gray-900"
                  >
                    {fortnightParts.map((part) => (
                      <option key={part.value} value={part.value}>
                        {part.label}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <label className="block text-sm font-medium text-gray-700">
                      Select Day
                    </label>
                    {(dayWiseCustomers[`${selectedFortnightPart}_week1_${selectedDay}`] || []).length > 0 && (
                      <button
                        type="button"
                        onClick={() => setShowCopyModal(true)}
                        className="flex items-center gap-1 px-2 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                      >
                        <Copy className="w-3 h-3" />
                        Copy to other days
                      </button>
                    )}
                  </div>
                  <div className="grid grid-cols-7 gap-1">
                    {days.map((day) => {
                      const contextKey = `${selectedFortnightPart}_week1_${day}`;
                      const customerCount = (dayWiseCustomers[contextKey] || [])
                        .length;

                      return (
                        <button
                          key={day}
                          type="button"
                          onClick={() => setSelectedDay(day)}
                          className={`px-2 py-2 rounded-lg text-xs font-medium transition-all border relative ${
                            selectedDay === day
                              ? "bg-blue-500 text-white border-blue-500"
                              : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50"
                          }`}
                        >
                          {day.substring(0, 3)}
                          {customerCount > 0 && (
                            <span
                              className={`absolute -top-1 -right-1 text-[10px] px-1.5 py-0.5 rounded-full font-medium ${
                                selectedDay === day
                                  ? "bg-white text-blue-600"
                                  : "bg-blue-500 text-white"
                              }`}
                            >
                              {customerCount}
                            </span>
                          )}
                        </button>
                      );
                    })}
                  </div>
                </div>
              </div>
            )}

            {/* Monthly: Schedule Configuration */}
            {selectedPeriod === "monthly" && (
              <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                {/* Compact Header */}
                <div className="flex items-center justify-between mb-3">
                  <label className="text-sm font-medium text-gray-700">
                    Monthly Schedule
                  </label>
                  {(dayWiseCustomers[`date_${selectedDate}`] || []).length >
                    0 && (
                    <button
                      type="button"
                      onClick={() => setShowCopyModal(true)}
                      className="flex items-center gap-1 px-2 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                    >
                      <Copy className="w-3 h-3" />
                      Copy to other dates
                    </button>
                  )}
                  <div className="flex items-center gap-2 text-xs">
                    <span className="text-gray-600">Days per month:</span>
                    <div className="flex items-center border border-gray-300 rounded bg-white">
                      <button
                        type="button"
                        onClick={() => {
                          const newValue = Math.max(1, monthlyDaysCount - 1);
                          setMonthlyDaysCount(newValue);
                          if (selectedDate > newValue) {
                            setSelectedDate(1);
                          }
                        }}
                        className="px-2 py-1 text-gray-600 hover:bg-gray-100 hover:text-gray-800 transition-colors border-r border-gray-300"
                        disabled={monthlyDaysCount <= 1}
                      >
                        −
                      </button>
                      <input
                        type="number"
                        min="1"
                        max="31"
                        value={monthlyDaysCount}
                        onChange={(e) => {
                          const numValue = parseInt(e.target.value) || 1;
                          const clampedValue = Math.max(
                            1,
                            Math.min(31, numValue)
                          );
                          setMonthlyDaysCount(clampedValue);
                          if (selectedDate > clampedValue) {
                            setSelectedDate(1);
                          }
                        }}
                        className="w-12 px-2 py-1 text-xs text-center border-0 focus:ring-0 focus:outline-none bg-transparent"
                      />
                      <button
                        type="button"
                        onClick={() => {
                          const newValue = Math.min(31, monthlyDaysCount + 1);
                          setMonthlyDaysCount(newValue);
                        }}
                        className="px-2 py-1 text-gray-600 hover:bg-gray-100 hover:text-gray-800 transition-colors border-l border-gray-300"
                        disabled={monthlyDaysCount >= 31}
                      >
                        +
                      </button>
                    </div>
                  </div>
                </div>

                {/* Compact Day Selection - 2 Rows */}
                <div className="space-y-1">
                  <label className="text-xs font-medium text-gray-600">
                    Select Day
                  </label>
                  <div className="bg-white border border-gray-300 rounded overflow-hidden">
                    {/* First Row - Days 1-15 */}
                    <div
                      className="grid grid-cols-15 gap-0 border-b border-gray-200"
                      style={{ gridTemplateColumns: "repeat(15, 1fr)" }}
                    >
                      {Array.from(
                        { length: Math.min(15, monthlyDaysCount) },
                        (_, i) => i + 1
                      ).map((date) => {
                        const contextKey = `date_${date}`;
                        const customerCount = (
                          dayWiseCustomers[contextKey] || []
                        ).length;

                        return (
                          <button
                            key={date}
                            type="button"
                            onClick={() => setSelectedDate(date)}
                            className={`h-7 text-xs font-medium transition-colors border-r border-gray-200 relative flex items-center justify-center ${
                              selectedDate === date
                                ? "bg-blue-500 text-white"
                                : "bg-white text-gray-700 hover:bg-blue-50"
                            }`}
                          >
                            <span className="text-xs">{date}</span>
                            {customerCount > 0 && (
                              <span
                                className={`absolute top-0.5 right-0.5 min-w-[12px] h-3 px-1 rounded-full text-[8px] font-semibold flex items-center justify-center leading-none ${
                                  selectedDate === date
                                    ? "bg-white text-blue-600 shadow-sm"
                                    : "bg-blue-500 text-white shadow-sm"
                                }`}
                                title={`${customerCount} customers assigned`}
                              >
                                {customerCount > 99 ? "99+" : customerCount}
                              </span>
                            )}
                          </button>
                        );
                      })}
                      {/* Fill empty cells in first row if less than 15 days */}
                      {Array.from(
                        { length: 15 - Math.min(15, monthlyDaysCount) },
                        (_, i) => (
                          <div
                            key={`empty-row1-${i}`}
                            className="h-7 border-r border-gray-200 bg-gray-100"
                          />
                        )
                      )}
                    </div>

                    {/* Second Row - Days 16-30 */}
                    {monthlyDaysCount > 15 && (
                      <div
                        className="grid grid-cols-15 gap-0"
                        style={{ gridTemplateColumns: "repeat(15, 1fr)" }}
                      >
                        {Array.from(
                          { length: Math.min(15, monthlyDaysCount - 15) },
                          (_, i) => i + 16
                        ).map((date) => {
                          const contextKey = `date_${date}`;
                          const customerCount = (
                            dayWiseCustomers[contextKey] || []
                          ).length;

                          return (
                            <button
                              key={date}
                              type="button"
                              onClick={() => setSelectedDate(date)}
                              className={`h-7 text-xs font-medium transition-colors border-r border-gray-200 last:border-r-0 relative flex items-center justify-center ${
                                selectedDate === date
                                  ? "bg-blue-500 text-white"
                                  : "bg-white text-gray-700 hover:bg-blue-50"
                              }`}
                            >
                              <span className="text-xs">{date}</span>
                              {customerCount > 0 && (
                                <span
                                  className={`absolute top-0.5 right-0.5 min-w-[12px] h-3 px-1 rounded-full text-[8px] font-semibold flex items-center justify-center leading-none ${
                                    selectedDate === date
                                      ? "bg-white text-blue-600 shadow-sm"
                                      : "bg-blue-500 text-white shadow-sm"
                                  }`}
                                  title={`${customerCount} customers assigned`}
                                >
                                  {customerCount > 99 ? "99+" : customerCount}
                                </span>
                              )}
                            </button>
                          );
                        })}
                        {/* Fill empty cells if less than 15 days in second row */}
                        {Array.from(
                          { length: 15 - Math.min(15, monthlyDaysCount - 15) },
                          (_, i) => (
                            <div
                              key={`empty-row2-${i}`}
                              className="h-7 border-r border-gray-200 bg-gray-100"
                            />
                          )
                        )}
                      </div>
                    )}
                  </div>
                </div>

                {/* Current Selection Info */}
                <div className="mt-3 p-2 bg-blue-50 rounded text-xs">
                  <span className="font-medium text-blue-800">
                    Selected: Day {selectedDate}
                  </span>
                  <span className="text-blue-600 ml-2">
                    ({(dayWiseCustomers[`date_${selectedDate}`] || []).length}{" "}
                    customers assigned)
                  </span>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Main Content - merged into single container */}
      {/* Customer Selection - Only show for non-daily schedules */}
      {(() => {
        const currentScheduleType = isMultiScheduleMode
          ? currentlySelectedType
          : selectedPeriod;

        if (currentScheduleType === "daily") {
          return null; // Don't show dropdown for daily schedules
        }

        return (
          <div className="bg-white rounded-lg p-4 border border-gray-200">
            <div className="mb-3">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-base font-semibold text-gray-900">
                    Customer Selection
                  </h3>
                  <p className="text-sm text-gray-500">
                    {isMultiScheduleMode
                      ? `Add customers for ${
                          periods.find((p) => p.value === currentScheduleType)
                            ?.label
                        } schedule`
                      : "Add customers to schedule for this route"}
                  </p>
                </div>
                <div className="text-sm text-gray-500">
                  <span className="font-medium">
                    {getAvailableCustomersForSelection().length}
                  </span>{" "}
                  available customers
                  {isMultiScheduleMode && assignedCustomers.size > 0 && (
                    <span className="ml-2 text-blue-600">
                      ({assignedCustomers.size} assigned to other schedules)
                    </span>
                  )}
                </div>
              </div>
            </div>
            <div className="flex items-center justify-between gap-4">
              {/* Left side - Customer Dropdown */}
              <div className="flex-1 max-w-md">
                <div className="relative">
                  <button
                    type="button"
                    data-dropdown-trigger
                    onClick={() =>
                      setShowCustomerDropdown(!showCustomerDropdown)
                    }
                    className="w-full h-10 px-3 flex items-center justify-between rounded-md border border-gray-300 bg-white text-sm font-normal hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <span className={selectedCustomerUIDs.length === 0 ? "text-gray-500" : "text-gray-900"}>
                      {selectedCustomerUIDs.length === 0
                        ? "Select customers to add..."
                        : `${selectedCustomerUIDs.length} customer${selectedCustomerUIDs.length !== 1 ? 's' : ''} selected`}
                    </span>
                    <ChevronDown className={`ml-2 h-4 w-4 shrink-0 opacity-50 transition-transform ${
                      showCustomerDropdown ? "rotate-180" : ""
                    }`} />
                  </button>

                  {/* Multi-select dropdown */}
                  {showCustomerDropdown && (
                    <div
                      ref={dropdownRef}
                      className="absolute z-50 top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-md shadow-md overflow-hidden"
                    >
                      {/* Search within dropdown */}
                      <div className="flex items-center border-b px-3 py-2">
                        <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
                        <input
                          type="text"
                          placeholder="Search customers..."
                          value={searchTerm}
                          onChange={(e) => setSearchTerm(e.target.value)}
                          onKeyDown={(e) => {
                            // Ctrl+A or Cmd+A to select all
                            if ((e.ctrlKey || e.metaKey) && e.key === 'a') {
                              e.preventDefault();
                              const filteredCustomers = getFilteredAvailableCustomers();
                              const allUIDs = filteredCustomers.map(c => c.UID);
                              setSelectedCustomerUIDs(allUIDs);
                            }
                            // Escape to close dropdown
                            if (e.key === 'Escape') {
                              e.preventDefault();
                              setShowCustomerDropdown(false);
                            }
                          }}
                          className="flex h-9 w-full bg-transparent py-2 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50 border-0 focus:ring-0"
                          autoFocus
                        />
                      </div>

                      {/* Customer list */}
                      <div
                        className="max-h-[280px] overflow-y-auto"
                      >
                        {availableCustomers.length === 0 ? (
                          <div className="py-6 text-center text-sm">
                            <div className="flex flex-col items-center gap-2">
                              <Users className="h-8 w-8 text-muted-foreground/50" />
                              <p>No customers available</p>
                              <p className="text-xs text-muted-foreground">
                                Please select customers in Step 2 first
                              </p>
                            </div>
                          </div>
                        ) : getFilteredAvailableCustomers().length === 0 ? (
                          <div className="py-6 text-center text-sm">
                            <div className="flex flex-col items-center gap-2">
                              <Search className="h-8 w-8 text-muted-foreground/50" />
                              <p>{getAvailableCustomersForSelection().length === 0
                                ? "All customers have been added"
                                : "No customers found"}</p>
                              <p className="text-xs text-muted-foreground">
                                {getAvailableCustomersForSelection().length === 0
                                  ? "All available customers are already scheduled"
                                  : "Try a different search term"}
                              </p>
                            </div>
                          </div>
                        ) : (
                          <div>
                            {/* Select All option */}
                            <div
                              onClick={() => {
                                const filteredCustomers = getFilteredAvailableCustomers();
                                const allUIDs = filteredCustomers.map(c => c.UID);
                                const allSelected = allUIDs.every(uid =>
                                  selectedCustomerUIDs.includes(uid)
                                );

                                if (allSelected) {
                                  // Deselect all
                                  setSelectedCustomerUIDs([]);
                                } else {
                                  // Select all
                                  setSelectedCustomerUIDs(allUIDs);
                                }
                              }}
                              className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-gray-100 cursor-pointer border-b font-medium"
                            >
                              <div className={`w-4 h-4 rounded border flex items-center justify-center ${
                                getFilteredAvailableCustomers().every(c =>
                                  selectedCustomerUIDs.includes(c.UID)
                                )
                                  ? "bg-blue-600 border-blue-600 text-white"
                                  : "border-gray-300 bg-white"
                              }`}>
                                {getFilteredAvailableCustomers().every(c =>
                                  selectedCustomerUIDs.includes(c.UID)
                                ) && <Check className="w-3 h-3" />}
                              </div>
                              <span>Select All ({getFilteredAvailableCustomers().length})</span>
                            </div>
                            {/* Individual customer items */}
                            {getFilteredAvailableCustomers().map((customer) => (
                              <div
                                key={customer.UID}
                                onClick={() =>
                                  toggleCustomerSelection(customer.UID)
                                }
                                className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-gray-100 cursor-pointer"
                              >
                                <div className={`w-4 h-4 rounded border flex items-center justify-center ${
                                  selectedCustomerUIDs.includes(customer.UID)
                                    ? "bg-blue-600 border-blue-600 text-white"
                                    : "border-gray-300 bg-white"
                                }`}>
                                  {selectedCustomerUIDs.includes(customer.UID) &&
                                    <Check className="w-3 h-3" />}
                                </div>
                                <div className="flex-1 min-w-0">
                                  <div className="flex items-center justify-between">
                                    <span className="text-gray-900 truncate">{customer.Name}</span>
                                    <span className="text-xs text-gray-500 ml-2">({customer.Code})</span>
                                  </div>
                                </div>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>

                      {/* Action buttons */}
                      <div className="p-3 border-t border-gray-200 bg-gray-50">
                        <div className="flex gap-2">
                          <button
                            type="button"
                            onClick={addSelectedCustomers}
                            disabled={selectedCustomerUIDs.length === 0}
                            className={`flex-1 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                              selectedCustomerUIDs.length > 0
                                ? "bg-blue-500 text-white hover:bg-blue-600"
                                : "bg-gray-200 text-gray-400 cursor-not-allowed"
                            }`}
                          >
                            {selectedCustomerUIDs.length > 0
                              ? `Add ${selectedCustomerUIDs.length} Customer${
                                  selectedCustomerUIDs.length !== 1 ? "s" : ""
                                }`
                              : "Add Customers"}
                          </button>
                          <button
                            type="button"
                            onClick={() => {
                              setSelectedCustomerUIDs([]);
                              setShowCustomerDropdown(false);
                              setSearchTerm("");
                            }}
                            className="px-4 py-2 border border-gray-300 bg-white text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-50 transition-colors"
                          >
                            Cancel
                          </button>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Right side - Summary */}
              <div className="flex-shrink-0">
                <div className="flex items-center gap-4">
                  <div className="text-right">
                    <h3 className="text-lg font-semibold text-gray-900">
                      {getCurrentPeriodLabel()}
                    </h3>
                    {getCurrentDaysLabel() && (
                      <p className="text-sm text-gray-600">
                        {getCurrentDaysLabel()}
                      </p>
                    )}
                  </div>
                  <div className="px-4 py-2 bg-blue-50 rounded-lg border border-blue-200">
                    <div className="flex items-center gap-2">
                      <Users className="w-4 h-4 text-blue-600" />
                      <span className="text-sm font-medium text-blue-900">
                        {filteredCustomers.length} scheduled
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        );
      })()}

      {conflicts.length > 0 && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
          <div className="flex items-center gap-2 text-red-800">
            <AlertTriangle className="w-4 h-4" />
            <span className="font-medium">Time Conflicts Detected</span>
          </div>
          <p className="text-sm text-red-600 mt-1">
            {conflicts.length} appointment
            {conflicts.length > 1 ? "s have" : " has"} overlapping times. Please
            review and adjust.
          </p>
        </div>
      )}

      {/* Current Schedule Type Time Settings */}
      {showSettings && (
        <div className="bg-white rounded-lg p-4 border border-gray-200">
          {(() => {
            // Get the current schedule type based on mode
            const currentScheduleType = isMultiScheduleMode
              ? currentlySelectedType
              : selectedPeriod;
            const period = periods.find((p) => p.value === currentScheduleType);
            const currentSettings =
              scheduleTypeSettings[currentScheduleType] ||
              scheduleTypeSettings.daily;

            // Get unique customer count from scheduleCustomerAssignments (source of truth)
            // This represents the actual number of unique customers assigned to this schedule type
            const uniqueCustomerUIDs =
              scheduleCustomerAssignments[currentScheduleType] || [];
            const customerCount = new Set(uniqueCustomerUIDs).size;

            return (
              <>
                <div className="mb-4">
                  <h4 className="text-base font-medium text-gray-900 mb-1 flex items-center gap-2">
                    <Clock className="h-5 w-5 text-blue-600" />
                    Time Settings for {period?.label || currentScheduleType}
                  </h4>
                  <p className="text-sm text-gray-600 flex items-center gap-2">
                    <span
                      className={`px-2 py-1 rounded text-xs font-medium ${
                        currentScheduleType === "daily"
                          ? "bg-green-100 text-green-700"
                          : currentScheduleType === "weekly"
                          ? "bg-blue-100 text-blue-700"
                          : currentScheduleType === "fortnight"
                          ? "bg-purple-100 text-purple-700"
                          : "bg-orange-100 text-orange-700"
                      }`}
                    >
                      {period?.label || currentScheduleType}
                    </span>
                    <span className="text-gray-500">
                      ({customerCount} customers assigned)
                    </span>
                  </p>
                </div>

                {/* Time Settings Grid */}
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                  <div className="space-y-2">
                    <label className="text-sm font-medium text-gray-700">
                      Day Start
                    </label>
                    <input
                      type="time"
                      value={currentSettings.dayStart}
                      onChange={(e) => {
                        setScheduleTypeSettings({
                          ...scheduleTypeSettings,
                          [currentScheduleType]: {
                            ...currentSettings,
                            dayStart: e.target.value,
                          },
                        });
                      }}
                      className="w-full px-3 py-2 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
                    />
                  </div>

                  <div className="space-y-2">
                    <label className="text-sm font-medium text-gray-700">
                      Day End
                    </label>
                    <input
                      type="time"
                      value={currentSettings.dayEnd}
                      onChange={(e) => {
                        setScheduleTypeSettings({
                          ...scheduleTypeSettings,
                          [currentScheduleType]: {
                            ...currentSettings,
                            dayEnd: e.target.value,
                          },
                        });
                      }}
                      className="w-full px-3 py-2 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
                    />
                  </div>

                  <div className="space-y-2">
                    <label className="text-sm font-medium text-gray-700">
                      Visit Duration
                    </label>
                    <select
                      value={currentSettings.defaultDuration}
                      onChange={(e) => {
                        setScheduleTypeSettings({
                          ...scheduleTypeSettings,
                          [currentScheduleType]: {
                            ...currentSettings,
                            defaultDuration: parseInt(e.target.value),
                          },
                        });
                      }}
                      className="w-full px-3 py-2 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
                    >
                      <option value={0}>N/A</option>
                      <option value={15}>15 minutes</option>
                      <option value={30}>30 minutes</option>
                      <option value={45}>45 minutes</option>
                      <option value={60}>1 hour</option>
                      <option value={90}>1.5 hours</option>
                      <option value={120}>2 hours</option>
                    </select>
                  </div>

                  <div className="space-y-2">
                    <label className="text-sm font-medium text-gray-700">
                      Travel Time
                    </label>
                    <select
                      value={currentSettings.travelTime}
                      onChange={(e) => {
                        setScheduleTypeSettings({
                          ...scheduleTypeSettings,
                          [currentScheduleType]: {
                            ...currentSettings,
                            travelTime: parseInt(e.target.value),
                          },
                        });
                      }}
                      className="w-full px-3 py-2 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
                    >
                      <option value={0}>N/A</option>
                      <option value={5}>5 minutes</option>
                      <option value={10}>10 minutes</option>
                      <option value={15}>15 minutes</option>
                      <option value={20}>20 minutes</option>
                      <option value={25}>25 minutes</option>
                      <option value={30}>30 minutes</option>
                      <option value={45}>45 minutes</option>
                      <option value={60}>1 hour</option>
                    </select>
                  </div>
                </div>

                {/* Auto Time Assignment */}
                <div className="mt-6 pt-4 border-t border-gray-200">
                  <h5 className="text-sm font-medium text-gray-900 mb-3 flex items-center gap-2">
                    <Clock className="h-4 w-4 text-blue-600" />
                    Auto Time Assignment
                  </h5>
                  <div className="bg-blue-50 p-4 rounded-lg">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                      <div className="space-y-2">
                        <label className="text-sm font-medium text-gray-700">
                          Start Time
                        </label>
                        <input
                          type="time"
                          id="autoStartTime"
                          className="w-full px-3 py-2 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
                          placeholder="09:00"
                        />
                      </div>
                      <div className="space-y-2">
                        <label className="text-sm font-medium text-gray-700">
                          Duration per Customer
                        </label>
                        <select
                          id="autoDuration"
                          defaultValue={30}
                          className="w-full px-3 py-2 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
                        >
                          <option value={15}>15 minutes</option>
                          <option value={30}>30 minutes</option>
                          <option value={45}>45 minutes</option>
                          <option value={60}>1 hour</option>
                          <option value={90}>1.5 hours</option>
                          <option value={120}>2 hours</option>
                        </select>
                      </div>
                      <div className="space-y-2">
                        <label className="text-sm font-medium text-gray-700">
                          Gap Between Customers
                        </label>
                        <select
                          id="autoGap"
                          defaultValue={0}
                          className="w-full px-3 py-2 rounded-lg border transition-all focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white border-gray-300 text-gray-900"
                        >
                          <option value={0}>No gap</option>
                          <option value={5}>5 minutes</option>
                          <option value={10}>10 minutes</option>
                          <option value={15}>15 minutes</option>
                          <option value={30}>30 minutes</option>
                        </select>
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => {
                        const startTimeInput = document.getElementById(
                          "autoStartTime"
                        ) as HTMLInputElement;
                        const durationSelect = document.getElementById(
                          "autoDuration"
                        ) as HTMLSelectElement;
                        const gapSelect = document.getElementById(
                          "autoGap"
                        ) as HTMLSelectElement;

                        const startTime = startTimeInput.value;
                        const duration = parseInt(durationSelect.value);
                        const gap = parseInt(gapSelect.value);

                        if (!startTime) {
                          showToast("Please select a start time", "error");
                          return;
                        }

                        autoAssignTimes(startTime, duration, gap);
                      }}
                      className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-lg transition-colors flex items-center gap-2 text-sm"
                    >
                      <Clock className="h-4 w-4" />
                      Auto Assign Times to All Customers
                    </button>
                    <p className="text-xs text-blue-700 mt-2">
                      This will automatically assign times to all customers in
                      the current schedule with incremental time slots.
                    </p>
                  </div>
                </div>

                {/* Global Settings */}
                <div className="mt-6 pt-4 border-t border-gray-200">
                  <h5 className="text-sm font-medium text-gray-900 mb-3">
                    Global Settings
                  </h5>
                  <div className="flex items-center gap-6">
                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        checked={settings.autoSave}
                        onChange={(e) =>
                          setSettings({
                            ...settings,
                            autoSave: e.target.checked,
                          })
                        }
                        className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                      <span className="text-sm text-gray-700">
                        Auto-save changes
                      </span>
                    </label>

                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        checked={settings.notifications}
                        onChange={(e) =>
                          setSettings({
                            ...settings,
                            notifications: e.target.checked,
                          })
                        }
                        className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                      <span className="text-sm text-gray-700">
                        Enable notifications
                      </span>
                    </label>
                  </div>
                </div>
              </>
            );
          })()}
        </div>
      )}

      {/* Add Customer Form */}
      {isAddingCustomer && <QuickAddForm />}

      {/* Removed duplicate customer display section - using unified grid below */}

      {/* Customer List - Show detailed scheduling grid for all schedule types */}
      {(() => {
        const currentScheduleType = isMultiScheduleMode
          ? currentlySelectedType
          : selectedPeriod;
        const contextKey = getCurrentContextKey();
        const contextCustomers = dayWiseCustomers[contextKey] || [];

        // Always show the scheduling grid (with empty state if no customers)
        return (
          <div className="bg-white rounded-lg border border-gray-200 overflow-hidden mt-3">
            <div className="px-4 py-3 border-b border-gray-200 bg-gray-50">
              <div className="flex items-center justify-between mb-2">
                <h3 className="text-base font-semibold text-gray-900">
                  {currentScheduleType === "daily"
                    ? "Customers Scheduled"
                    : currentScheduleType === "weekly"
                    ? `Customers Scheduled - Week ${selectedWeek}`
                    : currentScheduleType === "monthly"
                    ? `Customers Scheduled - Day ${selectedDate}`
                    : `Customers Scheduled - ${selectedFortnightPart}`}
                </h3>
                <div className="flex items-center gap-2">
                  <span
                    className={`px-2 py-1 rounded text-xs font-medium ${
                      currentScheduleType === "daily"
                        ? "bg-green-100 text-green-700"
                        : currentScheduleType === "weekly"
                        ? "bg-blue-100 text-blue-700"
                        : currentScheduleType === "monthly"
                        ? "bg-orange-100 text-orange-700"
                        : "bg-purple-100 text-purple-700"
                    }`}
                  >
                    {currentScheduleType === "daily"
                      ? "Daily Schedule"
                      : currentScheduleType === "weekly"
                      ? `Week ${selectedWeek} - ${selectedDay}`
                      : currentScheduleType === "monthly"
                      ? `Day ${selectedDate}`
                      : `${selectedFortnightPart} - ${selectedDay}`}
                  </span>
                  {contextCustomers.length > 1 && (
                    <span className="text-xs text-gray-500 italic">
                      ℹ️ Drag rows to reorder
                    </span>
                  )}
                </div>
              </div>
              <div className="grid grid-cols-11 gap-2 text-sm font-medium">
                <div className="text-gray-600 col-span-1"># / Order</div>
                <div className="text-gray-600 col-span-3">Customer</div>
                <div className="text-gray-600 col-span-2">Time</div>
                <div className="text-gray-600 col-span-2">Duration</div>
                <div className="text-gray-600 col-span-1">Priority</div>
                <div className="text-gray-600 col-span-2">Actions</div>
              </div>
            </div>

            <div className="max-h-[500px] overflow-y-auto">
              <div className="divide-y divide-gray-200">
                {contextCustomers.length === 0 ? (
                  <div className="px-4 py-8 text-center">
                    <Calendar className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                    <p className="text-base mb-1 text-gray-500">
                      No customers scheduled for this period
                    </p>
                    <p className="text-sm text-gray-400">
                      {currentScheduleType === "daily"
                        ? "Add your first daily customer to get started"
                        : "Select customers from the dropdown above to schedule them"}
                    </p>
                  </div>
                ) : (
                  contextCustomers.map((customer, index) => (
                    <CustomerRow
                      key={`${customer.id}_${index}`}
                      customer={customer}
                      index={index}
                      isConflicted={isConflicted}
                      expandedCustomerId={expandedCustomerId}
                      setExpandedCustomerId={setExpandedCustomerId}
                      saveTimeSettings={saveTimeSettings}
                      deleteCustomer={deleteCustomer}
                    />
                  ))
                )}
              </div>
            </div>
          </div>
        );
      })()}
      </div>

      {/* Copy Modal - Centered */}
      {showCopyModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            id="copy-modal"
            className="bg-white rounded-xl shadow-2xl border border-gray-200 p-6 max-w-md w-full mx-auto transform transition-all duration-300 scale-100 opacity-100"
          >
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <Copy className="w-5 h-5 text-blue-500" />
                <h3 className="text-lg font-semibold text-gray-800">
                  {selectedPeriod === "weekly"
                    ? "Copy to Other Days"
                    : selectedPeriod === "fortnight"
                    ? "Copy to Other Days/Parts"
                    : "Copy to Other Dates"}
                </h3>
              </div>
              <button
                type="button"
                onClick={() => {
                  setShowCopyModal(false);
                  setSelectedTargetDays([]);
                  setSelectedTargetDates([]);
                }}
                className="text-gray-400 hover:text-gray-600 hover:bg-gray-100 p-1 rounded-lg transition-colors"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {selectedPeriod === "weekly" && (
              <div>
                <p className="text-sm text-gray-600 mb-4">
                  Copying from{" "}
                  <span className="font-semibold text-gray-800">
                    {selectedDay} (Week {selectedWeek})
                  </span>
                </p>

                {/* Week Selection */}
                <div className="mb-4">
                  <p className="text-sm font-medium text-gray-700 mb-2">Copy to other weeks:</p>
                  <div className="grid grid-cols-3 gap-2">
                    {[1, 2, 3, 4, 5].filter(week => week !== selectedWeek).map((week) => (
                      <label
                        key={`week-${week}`}
                        className={`flex items-center p-2 rounded-lg border cursor-pointer transition-all hover:bg-blue-50 ${
                          selectedTargetWeeks.includes(week)
                            ? "border-blue-500 bg-blue-50"
                            : "border-gray-200 bg-white"
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={selectedTargetWeeks.includes(week)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setSelectedTargetWeeks([...selectedTargetWeeks, week]);
                            } else {
                              setSelectedTargetWeeks(selectedTargetWeeks.filter(w => w !== week));
                            }
                          }}
                          className="mr-2 text-blue-500 focus:ring-blue-500"
                        />
                        <span className="text-sm font-medium">Week {week}</span>
                      </label>
                    ))}
                  </div>
                </div>

                {/* Day Selection */}
                <div className="mb-4">
                  <p className="text-sm font-medium text-gray-700 mb-2">Copy to other days (in current week):</p>
                  <div className="grid grid-cols-2 gap-2">
                    {days
                      .filter((day) => day !== selectedDay)
                      .map((day) => (
                        <label
                          key={day}
                          className={`flex items-center p-2 rounded-lg border cursor-pointer transition-all hover:bg-blue-50 ${
                            selectedTargetDays.includes(day)
                              ? "border-blue-500 bg-blue-50"
                              : "border-gray-200 bg-white"
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={selectedTargetDays.includes(day)}
                            onChange={(e) => {
                              if (e.target.checked) {
                                setSelectedTargetDays([...selectedTargetDays, day]);
                              } else {
                                setSelectedTargetDays(selectedTargetDays.filter(d => d !== day));
                              }
                            }}
                            className="mr-2 text-blue-500 focus:ring-blue-500"
                          />
                          <span className="text-sm font-medium">{day}</span>
                        </label>
                      ))}
                  </div>
                </div>

                {/* Info text */}
                <p className="text-xs text-gray-500 italic">
                  Tip: Select weeks to copy to the same day in other weeks, and/or select days to copy within the current week.
                </p>
              </div>
            )}

            {selectedPeriod === "fortnight" && (
              <div>
                <p className="text-sm text-gray-600 mb-4">
                  Copying from{" "}
                  <span className="font-semibold text-gray-800">
                    {selectedDay} ({selectedFortnightPart})
                  </span>
                </p>

                {/* Fortnight Part Selection */}
                <div className="mb-4">
                  <p className="text-sm font-medium text-gray-700 mb-2">Copy to other fortnight part:</p>
                  <div className="grid grid-cols-1 gap-2">
                    {fortnightParts
                      .filter(part => part.value !== selectedFortnightPart)
                      .map((part) => (
                        <label
                          key={part.value}
                          className={`flex items-center p-2 rounded-lg border cursor-pointer transition-all hover:bg-blue-50 ${
                            selectedTargetDays.includes(part.value)
                              ? "border-blue-500 bg-blue-50"
                              : "border-gray-200 bg-white"
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={selectedTargetDays.includes(part.value)}
                            onChange={(e) => {
                              if (e.target.checked) {
                                setSelectedTargetDays([...selectedTargetDays, part.value]);
                              } else {
                                setSelectedTargetDays(selectedTargetDays.filter(d => d !== part.value));
                              }
                            }}
                            className="mr-2 text-blue-500 focus:ring-blue-500"
                          />
                          <span className="text-sm font-medium">{part.label}</span>
                        </label>
                      ))}
                  </div>
                </div>

                {/* Day Selection */}
                <div className="mb-4">
                  <p className="text-sm font-medium text-gray-700 mb-2">Copy to other days (in current fortnight part):</p>
                  <div className="grid grid-cols-2 gap-2">
                    {days
                      .filter((day) => day !== selectedDay)
                      .map((day) => (
                        <label
                          key={day}
                          className={`flex items-center p-2 rounded-lg border cursor-pointer transition-all hover:bg-blue-50 ${
                            selectedTargetDays.includes(day)
                              ? "border-blue-500 bg-blue-50"
                              : "border-gray-200 bg-white"
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={selectedTargetDays.includes(day)}
                            onChange={(e) => {
                              if (e.target.checked) {
                                setSelectedTargetDays([...selectedTargetDays, day]);
                              } else {
                                setSelectedTargetDays(selectedTargetDays.filter(d => d !== day));
                              }
                            }}
                            className="mr-2 text-blue-500 focus:ring-blue-500"
                          />
                          <span className="text-sm font-medium">{day}</span>
                        </label>
                      ))}
                  </div>
                </div>

                {/* Info text */}
                <p className="text-xs text-gray-500 italic">
                  Tip: Select the other fortnight part to copy to the same day, and/or select days to copy within the current part.
                </p>
              </div>
            )}

            {selectedPeriod === "monthly" && (
              <div>
                <p className="text-sm text-gray-600 mb-4">
                  Copying from{" "}
                  <span className="font-semibold text-gray-800">
                    Day {selectedDate}
                  </span>
                  . Select target dates:
                </p>
                <div className="grid grid-cols-5 gap-2 mb-4 max-h-60 overflow-y-auto p-2 border border-gray-100 rounded-lg">
                  {Array.from({ length: monthlyDaysCount }, (_, i) => i + 1)
                    .filter((date) => date !== selectedDate)
                    .map((date) => (
                      <label
                        key={date}
                        className={`flex items-center justify-center p-2 rounded-lg border cursor-pointer transition-all hover:bg-blue-50 ${
                          selectedTargetDates.includes(date)
                            ? "border-blue-500 bg-blue-50"
                            : "border-gray-200 bg-white"
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={selectedTargetDates.includes(date)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setSelectedTargetDates([
                                ...selectedTargetDates,
                                date,
                              ]);
                            } else {
                              setSelectedTargetDates(
                                selectedTargetDates.filter((d) => d !== date)
                              );
                            }
                          }}
                          className="mr-1 text-blue-500 focus:ring-blue-500"
                        />
                        <span className="text-sm font-medium">{date}</span>
                      </label>
                    ))}
                </div>
              </div>
            )}

            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => {
                  setShowCopyModal(false);
                  setSelectedTargetDays([]);
                  setSelectedTargetDates([]);
                }}
                className="px-4 py-2 text-sm text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={() => {
                  if (selectedPeriod === "weekly") {
                    copyToOtherDaysAndWeeks(selectedTargetDays, selectedTargetWeeks);
                  } else if (selectedPeriod === "monthly") {
                    copyToOtherDates(selectedTargetDates);
                  } else if (selectedPeriod === "fortnight") {
                    // For fortnight, we need to separate parts from days
                    const parts = selectedTargetDays.filter(d => d === "1st-3rd" || d === "2nd-4th");
                    const days = selectedTargetDays.filter(d => d !== "1st-3rd" && d !== "2nd-4th");
                    copyToOtherDaysAndFortnightParts(parts, days);
                  }
                  setShowCopyModal(false);
                  setSelectedTargetDays([]);
                  setSelectedTargetDates([]);
                  setSelectedTargetWeeks([]);
                }}
                disabled={
                  (selectedPeriod === "weekly" &&
                    selectedTargetDays.length === 0 &&
                    selectedTargetWeeks.length === 0) ||
                  (selectedPeriod === "monthly" &&
                    selectedTargetDates.length === 0) ||
                  (selectedPeriod === "fortnight" &&
                    selectedTargetDays.length === 0)
                }
                className={`px-4 py-2 text-sm text-white rounded-lg ${
                  (selectedPeriod === "weekly" &&
                    selectedTargetDays.length === 0 &&
                    selectedTargetWeeks.length === 0) ||
                  (selectedPeriod === "monthly" &&
                    selectedTargetDates.length === 0) ||
                  (selectedPeriod === "fortnight" &&
                    selectedTargetDays.length === 0)
                    ? "bg-gray-400 cursor-not-allowed"
                    : "bg-blue-500 hover:bg-blue-600"
                }`}
              >
                Copy Customers
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Toast Notification - Top Right Corner */}
      {toast.show && (
        <div
          className={`fixed top-4 right-4 z-50 transform transition-all duration-300 ${
            toast.show
              ? "translate-x-0 opacity-100"
              : "translate-x-full opacity-0"
          }`}
        >
          <div
            className={`flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg border ${
              toast.type === "success"
                ? "bg-green-50 border-green-200 text-green-800"
                : toast.type === "error"
                ? "bg-red-50 border-red-200 text-red-800"
                : "bg-blue-50 border-blue-200 text-blue-800"
            }`}
          >
            {toast.type === "success" && (
              <CheckCircle2 className="w-5 h-5 text-green-600" />
            )}
            {toast.type === "error" && (
              <AlertTriangle className="w-5 h-5 text-red-600" />
            )}
            {toast.type === "info" && (
              <Bell className="w-5 h-5 text-blue-600" />
            )}
            <span className="text-sm font-medium">{toast.message}</span>
          </div>
        </div>
      )}
    </>
  );
};

export default CustomerScheduler;
