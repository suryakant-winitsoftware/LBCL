"use client";

import React, { useState, useEffect, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/use-toast";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { ScrollArea } from "@/components/ui/scroll-area";
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
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import { ChevronDown } from "lucide-react";
import {
  ArrowLeft,
  Save,
  Route,
  Store,
  Search,
  MapPin,
  Check,
  X,
  Loader2,
  RefreshCw,
  Filter,
  Info,
} from "lucide-react";
import { authService } from "@/lib/auth-service";
import { routeService } from "@/services/routeService";
import { storeService } from "@/services/storeService";

// Types
interface RouteDetails {
  uid: string;
  code: string;
  name: string;
  orgUID: string;
  isActive: boolean;
  status: string;
  totalCustomers?: number;
  roleUID?: string;
}

interface StoreDetails {
  uid: string;
  code: string;
  name: string;
  area?: string;
  address?: string;
  contactNo?: string;
  isActive: boolean;
  type?: string;
}

interface RouteCustomerMapping {
  RouteUID: string;
  StoreUID: string;
  SeqNo: number;
  VisitTime?: string;
  VisitDuration?: number;
  EndTime?: string;
  TravelTime?: number;
  IsDeleted: boolean;
  UID: string;
  ActionType?: number; // 0 = Add, 1 = Update, 2 = Delete
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
}

interface RouteMasterData {
  Route: any;
  RouteSchedule?: any;
  RouteScheduleDaywise?: any;
  RouteScheduleFortnight?: any;
  RouteCustomersList: RouteCustomerMapping[];
  RouteUserList?: any[];
}

export default function CustomerMappingPage() {
  const router = useRouter();
  const { toast } = useToast();

  // State management
  const [saving, setSaving] = useState(false);
  const [loadingRoutes, setLoadingRoutes] = useState(true);
  const [loadingStores, setLoadingStores] = useState(false);

  const [routes, setRoutes] = useState<RouteDetails[]>([]);
  const [selectedRoute, setSelectedRoute] = useState<string>("");
  const [currentRouteData, setCurrentRouteData] =
    useState<RouteMasterData | null>(null);

  const [allStores, setAllStores] = useState<StoreDetails[]>([]);
  const [assignedStores, setAssignedStores] = useState<Set<string>>(new Set());
  const [selectedStores, setSelectedStores] = useState<Set<string>>(new Set());

  const [searchTerm, setSearchTerm] = useState("");
  const [filterType, setFilterType] = useState<
    "all" | "assigned" | "unassigned"
  >("all");

  // Lazy loading state
  const [displayedStores, setDisplayedStores] = useState<StoreDetails[]>([]);
  const [storesPagination, setStoresPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: false,
  });
  const [loadingMoreStores, setLoadingMoreStores] = useState(false);
  const scrollAreaRef = useRef<HTMLDivElement>(null);

  const currentUser = authService.getCurrentUser();
  // Use same organization logic as manage page
  let organizationUID = "";
  if (currentUser?.currentOrganization?.uid) {
    organizationUID = currentUser.currentOrganization.uid;
  } else if (
    currentUser?.availableOrganizations?.length &&
    currentUser.availableOrganizations.length > 0
  ) {
    organizationUID = currentUser.availableOrganizations[0].uid;
  }

  // Fetch routes for the organization
  const fetchRoutes = async () => {
    setLoadingRoutes(true);
    try {
      // Use routeService like in the manage page
      // First try with a reasonable page size, then fetch all if needed
      let allRoutes: any[] = [];
      let currentPage = 1;
      const pageSize = 100; // Reasonable batch size
      let hasMorePages = true;

      while (hasMorePages) {
        const request = {
          pageNumber: currentPage,
          pageSize: pageSize,
          isCountRequired: currentPage === 1, // Only get count on first request
          sortCriterias: [],
          filterCriterias: [],
        };

        const response = await routeService.getRoutes(request, organizationUID);

        if (response.IsSuccess && response.Data?.PagedData) {
          allRoutes = [...allRoutes, ...response.Data.PagedData];

          // Check if we have more pages
          if (
            response.Data.PagedData.length < pageSize ||
            (response.Data.TotalCount &&
              allRoutes.length >= response.Data.TotalCount)
          ) {
            hasMorePages = false;
          } else {
            currentPage++;
          }
        } else {
          hasMorePages = false;
        }
      }

      // Process all routes
      if (allRoutes.length > 0) {
        setRoutes(
          allRoutes.map((r: any) => ({
            uid: r.UID,
            code: r.Code,
            name: r.Name,
            orgUID: r.OrgUID,
            isActive: r.IsActive,
            status: r.Status,
            totalCustomers: r.TotalCustomers || 0,
            roleUID: r.RoleUID,
          }))
        );
      } else {
        setRoutes([]);
      }
    } catch (error) {
      console.error("Error fetching routes:", error);
      toast({
        title: "Error",
        description: "Failed to load routes. Please try again.",
        variant: "destructive",
      });
      setRoutes([]);
    } finally {
      setLoadingRoutes(false);
    }
  };

  // Fetch route details with assigned stores
  const fetchRouteDetails = async (routeUID: string) => {
    if (!routeUID) return;

    setLoadingStores(true);
    try {
      const token = authService.getToken();
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api"
        }/Route/SelectRouteMasterViewByUID?UID=${routeUID}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (!response.ok)
        throw new Error(`HTTP error! status: ${response.status}`);

      const result = await response.json();
      if (result.IsSuccess && result.Data) {
        console.log("Route details fetched:", result.Data);
        console.log("RouteCustomersList:", result.Data.RouteCustomersList);

        setCurrentRouteData(result.Data);

        // Set assigned stores
        const assigned = new Set<string>();
        const selected = new Set<string>();

        if (
          result.Data.RouteCustomersList &&
          Array.isArray(result.Data.RouteCustomersList)
        ) {
          console.log("=== Analyzing RouteCustomersList from backend ===");
          result.Data.RouteCustomersList.forEach((rc: RouteCustomerMapping) => {
            console.log(
              `Store ${rc.StoreUID}: IsDeleted=${rc.IsDeleted}, ActionType=${rc.ActionType}, UID=${rc.UID}`
            );
          });

          // Filter out deleted stores first
          const activeStores = result.Data.RouteCustomersList.filter(
            (rc: RouteCustomerMapping) => rc.IsDeleted === false
          );
          const deletedStores = result.Data.RouteCustomersList.filter(
            (rc: RouteCustomerMapping) => rc.IsDeleted === true
          );

          console.log(
            `Active stores (IsDeleted=false): ${activeStores.length}`,
            activeStores.map((rc: RouteCustomerMapping) => rc.StoreUID)
          );
          console.log(
            `Deleted stores (IsDeleted=true): ${deletedStores.length}`,
            deletedStores.map((rc: RouteCustomerMapping) => rc.StoreUID)
          );

          activeStores.forEach((rc: RouteCustomerMapping) => {
            assigned.add(rc.StoreUID);
            selected.add(rc.StoreUID);
          });
        }

        console.log(
          `Total assigned stores: ${assigned.size}`,
          Array.from(assigned)
        );

        setAssignedStores(assigned);
        setSelectedStores(selected);

        // Update the route in the routes list with actual store count
        setRoutes((prevRoutes) =>
          prevRoutes.map((route) =>
            route.uid === routeUID
              ? { ...route, totalCustomers: assigned.size }
              : route
          )
        );

        // Load initial stores with lazy loading
        await fetchStoresPage(1, searchTerm);
      }
    } catch (error) {
      console.error("Error fetching route details:", error);
      toast({
        title: "Error",
        description: "Failed to load route details. Please try again.",
        variant: "destructive",
      });
    } finally {
      setLoadingStores(false);
    }
  };

  // Lazy load stores - fetch initial batch and load more as needed
  const fetchStoresPage = useCallback(
    async (page: number = 1, search: string = "", append: boolean = false) => {
      if (append) {
        setLoadingMoreStores(true);
      } else {
        setLoadingStores(true);
      }

      try {
        const filterCriterias = [];

        // Add search filter if provided
        if (search.trim()) {
          console.log("Adding Code search filter:", search.trim());
          // Only search by Code for now - this was working before
          filterCriterias.push({
            name: "Code",
            value: search.trim(),
            operator: "contains",
          });
        }

        const request = {
          pageNumber: page,
          pageSize: storesPagination.pageSize,
          filterCriterias,
          sortCriterias: [{ sortParameter: "Code", direction: 0 as 0 }], // Changed to Code since Name might not exist
          isCountRequired: page === 1, // Only get count on first request
        };

        console.log("Store search request:", JSON.stringify(request, null, 2));

        const response = await storeService.getAllStores(request);

        if (response.pagedData) {
          // Map the stores data to match our interface
          const fetchedStores = response.pagedData.map((s: any) => ({
            uid: s.UID || s.uid,
            code: s.Code || s.code || s.UID || s.uid,
            name: s.Name || s.name || `Store ${s.UID || s.uid}`,
            area: s.Area || s.area || "",
            address: s.Address || s.address || "",
            contactNo: s.ContactNo || s.contactNo || "",
            isActive: s.IsActive !== false,
            type: s.Type || s.type || "Store",
          }));

          if (append) {
            // Append to existing stores, but filter out duplicates
            setDisplayedStores((prev) => {
              const existingIds = new Set(prev.map((s) => s.uid));
              const newStores = fetchedStores.filter(
                (s: StoreDetails) => !existingIds.has(s.uid)
              );
              return [...prev, ...newStores];
            });
            setAllStores((prev) => {
              const existingIds = new Set(prev.map((s) => s.uid));
              const newStores = fetchedStores.filter(
                (s: StoreDetails) => !existingIds.has(s.uid)
              );
              return [...prev, ...newStores];
            });
          } else {
            // Replace stores (new search or initial load)
            setDisplayedStores(fetchedStores);
            setAllStores(fetchedStores);
          }

          // Update pagination state
          const newTotalCount =
            page === 1 ? response.totalCount || 0 : storesPagination.totalCount;
          // For append mode, calculate the actual new count after filtering duplicates
          let actualNewCount = fetchedStores.length;
          if (append) {
            const existingIds = new Set(displayedStores.map((s) => s.uid));
            actualNewCount = fetchedStores.filter(
              (s: StoreDetails) => !existingIds.has(s.uid)
            ).length;
          }
          const currentDisplayedCount = append
            ? displayedStores.length + actualNewCount
            : fetchedStores.length;

          setStoresPagination((prev) => ({
            currentPage: page,
            pageSize: prev.pageSize,
            totalCount: newTotalCount,
            hasMore:
              fetchedStores.length === prev.pageSize &&
              (newTotalCount === 0 || currentDisplayedCount < newTotalCount),
          }));

          console.log(
            `Pagination update: Page ${page}, fetched ${
              fetchedStores.length
            }, displayed ${currentDisplayedCount}, total ${newTotalCount}, hasMore: ${
              fetchedStores.length === storesPagination.pageSize &&
              (newTotalCount === 0 || currentDisplayedCount < newTotalCount)
            }`
          );

          console.log(`Loaded ${fetchedStores.length} stores (page ${page})`);
        }
      } catch (error) {
        console.error("Error fetching stores:", error);
        toast({
          title: "Error",
          description: "Failed to load stores. Please try again.",
          variant: "destructive",
        });
      } finally {
        setLoadingStores(false);
        setLoadingMoreStores(false);
      }
    },
    [storesPagination.pageSize, displayedStores.length, toast]
  );

  // Load more stores when user scrolls or clicks "Load More"
  const loadMoreStores = useCallback(async () => {
    if (!storesPagination.hasMore || loadingMoreStores || loadingStores) {
      console.log(
        `Skipping loadMoreStores: hasMore=${storesPagination.hasMore}, loadingMore=${loadingMoreStores}, loading=${loadingStores}`
      );
      return;
    }

    console.log(
      `Loading page ${
        storesPagination.currentPage + 1
      } with search: "${searchTerm}"`
    );
    await fetchStoresPage(storesPagination.currentPage + 1, searchTerm, true);
  }, [
    storesPagination.hasMore,
    storesPagination.currentPage,
    loadingMoreStores,
    loadingStores,
    searchTerm,
    fetchStoresPage,
  ]);

  // Handle route selection
  const handleRouteSelection = (routeUID: string) => {
    setSelectedRoute(routeUID);
    if (routeUID) {
      fetchRouteDetails(routeUID);
    } else {
      setCurrentRouteData(null);
      setAssignedStores(new Set());
      setSelectedStores(new Set());
      setAllStores([]);
      setDisplayedStores([]);
    }
  };

  // Debounced search effect
  useEffect(() => {
    if (!selectedRoute) return;

    const timeoutId = setTimeout(() => {
      fetchStoresPage(1, searchTerm, false);
    }, 300); // 300ms debounce

    return () => clearTimeout(timeoutId);
  }, [searchTerm, selectedRoute]);

  // Auto-load more stores when scrolling near bottom
  const handleScroll = useCallback(
    (event: React.UIEvent<HTMLDivElement>) => {
      const { scrollTop, scrollHeight, clientHeight } = event.currentTarget;
      const scrollPercentage = (scrollTop + clientHeight) / scrollHeight;

      // Load more when user scrolls to 80% of the content
      if (
        scrollPercentage > 0.8 &&
        storesPagination.hasMore &&
        !loadingMoreStores &&
        !loadingStores
      ) {
        loadMoreStores();
      }
    },
    [storesPagination.hasMore, loadingMoreStores, loadingStores, loadMoreStores]
  );

  // Reset scroll position when search changes
  useEffect(() => {
    if (scrollAreaRef.current) {
      const scrollElement = scrollAreaRef.current.querySelector(
        "[data-radix-scroll-area-viewport]"
      );
      if (scrollElement) {
        scrollElement.scrollTop = 0;
      }
    }
  }, [searchTerm, filterType]);

  // Handle store checkbox changes
  const handleStoreToggle = (storeUID: string) => {
    const newSelected = new Set(selectedStores);
    if (newSelected.has(storeUID)) {
      newSelected.delete(storeUID);
    } else {
      newSelected.add(storeUID);
    }
    setSelectedStores(newSelected);
  };

  // Select all visible stores
  const selectAllVisible = () => {
    const filtered = getFilteredStores();
    const newSelected = new Set(selectedStores);
    filtered.forEach((store) => newSelected.add(store.uid));
    setSelectedStores(newSelected);
  };

  // Deselect all stores
  const deselectAll = () => {
    setSelectedStores(new Set());
  };

  // Reset to original assignments
  const resetToOriginal = () => {
    setSelectedStores(new Set(assignedStores));
  };

  // Filter stores based on assignment filter type (search is handled server-side now)
  const getFilteredStores = useCallback(() => {
    let filtered = displayedStores;

    // Apply assignment filter (search is handled in fetchStoresPage)
    switch (filterType) {
      case "assigned":
        filtered = filtered.filter((store) => assignedStores.has(store.uid));
        break;
      case "unassigned":
        filtered = filtered.filter((store) => !assignedStores.has(store.uid));
        break;
    }

    return filtered;
  }, [displayedStores, filterType, assignedStores]);

  // Check if there are unsaved changes
  const hasUnsavedChanges = () => {
    if (assignedStores.size !== selectedStores.size) return true;
    for (const uid of assignedStores) {
      if (!selectedStores.has(uid)) return true;
    }
    return false;
  };

  // Save changes - only update store mappings without disturbing route details
  const saveChanges = async () => {
    if (!selectedRoute || !currentRouteData) {
      toast({
        title: "Error",
        description: "Please select a route first.",
        variant: "destructive",
      });
      return;
    }

    setSaving(true);
    try {
      const token = authService.getToken();

      // Prepare ONLY the route customer list changes with proper ActionType
      const routeCustomersList: RouteCustomerMapping[] = [];
      let seqNo = 1;

      // First, check which stores are existing vs new
      const existingStoreUIDs = new Set(
        currentRouteData.RouteCustomersList?.filter((rc) => !rc.IsDeleted)?.map(
          (rc) => rc.StoreUID
        ) || []
      );

      // BACKEND-COMPATIBLE APPROACH: Send Add, Update, and Delete actions
      // Step 1: Handle stores to be deleted (were assigned but now unselected)
      const storesToDelete = Array.from(assignedStores).filter(
        (storeUID) => !selectedStores.has(storeUID)
      );

      storesToDelete.forEach((storeUID) => {
        const existing = currentRouteData.RouteCustomersList?.find(
          (rc) => rc.StoreUID === storeUID && !rc.IsDeleted
        );

        if (existing) {
          routeCustomersList.push({
            RouteUID: selectedRoute,
            StoreUID: storeUID,
            SeqNo: existing.SeqNo || 0,
            VisitTime: existing.VisitTime || "00:00:00",
            VisitDuration: existing.VisitDuration || 0,
            EndTime: existing.EndTime || "00:00:00",
            TravelTime: existing.TravelTime || 0,
            IsDeleted: true, // Mark for deletion
            UID: existing.UID || `${selectedRoute}_${storeUID}`,
            ActionType: 2, // 2 = Delete
            CreatedBy: existing.CreatedBy || "ADMIN",
            CreatedTime: existing.CreatedTime || new Date().toISOString(),
            ModifiedBy: currentUser?.name || "ADMIN",
            ModifiedTime: new Date().toISOString(),
          } as any);
        }
      });

      // Step 2: Handle selected stores (Add new or Update existing)
      allStores.forEach((store) => {
        if (selectedStores.has(store.uid)) {
          const existing = currentRouteData.RouteCustomersList?.find(
            (rc) => rc.StoreUID === store.uid && !rc.IsDeleted
          );

          const isExisting = existingStoreUIDs.has(store.uid);

          routeCustomersList.push({
            RouteUID: selectedRoute,
            StoreUID: store.uid,
            SeqNo: existing?.SeqNo || seqNo++,
            VisitTime: existing?.VisitTime || "00:00:00",
            VisitDuration: existing?.VisitDuration || 0,
            EndTime: existing?.EndTime || "00:00:00",
            TravelTime: existing?.TravelTime || 0,
            IsDeleted: false, // Active store
            UID: existing?.UID || `${selectedRoute}_${store.uid}`,
            ActionType: isExisting ? 1 : 0, // 0 = Add (new), 1 = Update (existing)
            CreatedBy: existing?.CreatedBy || currentUser?.name || "ADMIN",
            CreatedTime: existing?.CreatedTime || new Date().toISOString(),
            ModifiedBy: currentUser?.name || "ADMIN",
            ModifiedTime: new Date().toISOString(),
          } as any);
        }
      });

      const addCount = routeCustomersList.filter(
        (rc) => rc.ActionType === 0
      ).length;
      const updateCount = routeCustomersList.filter(
        (rc) => rc.ActionType === 1
      ).length;
      const deleteCount = routeCustomersList.filter(
        (rc) => rc.ActionType === 2
      ).length;

      console.log(
        `Sending ${routeCustomersList.length} route customer actions: ${addCount} adds, ${updateCount} updates, ${deleteCount} deletes`
      );

      // Keep all existing route data EXACTLY as is, only update the customer list and total count
      const activeCustomersCount = routeCustomersList.filter(
        (rc) => rc.ActionType !== 2
      ).length; // Count non-deleted stores

      const updatedRouteMaster = {
        Route: {
          ...currentRouteData.Route,
          TotalCustomers: activeCustomersCount, // Update total customer count
          ModifiedBy: currentUser?.name || "ADMIN",
          ModifiedTime: new Date().toISOString(),
        },
        RouteSchedule: currentRouteData.RouteSchedule, // Keep schedule unchanged (can be null to avoid backend updates)
        RouteScheduleDaywise: currentRouteData.RouteScheduleDaywise, // Keep daywise unchanged (can be null to avoid backend updates)
        RouteScheduleFortnight: currentRouteData.RouteScheduleFortnight, // Keep fortnight unchanged (can be null)
        RouteCustomersList: routeCustomersList, // ONLY update this
        RouteUserList: currentRouteData.RouteUserList || [], // Keep users unchanged
      };

      console.log("=== SENDING UPDATE TO BACKEND ===");
      console.log(
        `Updating RouteCustomersList and TotalCustomers (${activeCustomersCount}), keeping other details intact`
      );
      console.log("RouteCustomersList being sent:", routeCustomersList);
      console.log("Breakdown of stores being sent:");
      routeCustomersList.forEach((rc) => {
        console.log(
          `  - ${rc.StoreUID}: IsDeleted=${rc.IsDeleted}, ActionType=${rc.ActionType} (0=Add, 1=Update, 2=Delete)`
        );
      });
      console.log("Full payload:", JSON.stringify(updatedRouteMaster, null, 2));

      // Update route
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api"
        }/Route/UpdateRouteMaster`,
        {
          method: "PUT",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify(updatedRouteMaster),
        }
      );

      console.log("Update response status:", response.status, response.ok);

      if (!response.ok) {
        const errorText = await response.text();
        console.error("Update failed with error:", errorText);

        // Parse error to check for common issues
        if (
          errorText.includes("NpgsqlTransaction") ||
          errorText.includes("transaction")
        ) {
          toast({
            title: "Transaction Error",
            description:
              "Database transaction failed. This may be due to concurrent updates or constraint violations. Please try again.",
            variant: "destructive",
          });
        } else if (
          errorText.includes("DELETE") ||
          errorText.includes("route_customer")
        ) {
          toast({
            title: "Store Removal Error",
            description:
              "Failed to remove stores from route. Please check if stores are used in active journey plans.",
            variant: "destructive",
          });
        } else {
          toast({
            title: "Update Failed",
            description: `Failed to update route: ${errorText}`,
            variant: "destructive",
          });
        }

        // Still try to refresh to show current state
        setTimeout(() => {
          fetchRouteDetails(selectedRoute);
        }, 2000);
        return;
      }

      let result;
      try {
        result = await response.json();
      } catch (e) {
        console.error("Failed to parse response:", e);
        toast({
          title: "Warning",
          description:
            "Server response was not in expected format. Please check if the update succeeded.",
          variant: "default",
        });
        // Still try to refresh to see current state
        setTimeout(() => {
          fetchRouteDetails(selectedRoute);
        }, 1000);
        return;
      }

      console.log("Update response:", result);

      if (result.IsSuccess) {
        const activeStoresCount = routeCustomersList.filter(
          (rc) => !rc.IsDeleted
        ).length;

        toast({
          title: "Success",
          description: `Successfully updated store assignments. ${activeStoresCount} stores assigned to route.`,
        });

        // Update assigned stores to match selected
        setAssignedStores(new Set(selectedStores));

        // Update the route in the routes list with new store count
        setRoutes((prevRoutes) =>
          prevRoutes.map((route) =>
            route.uid === selectedRoute
              ? { ...route, totalCustomers: activeStoresCount }
              : route
          )
        );

        // Refresh route details to confirm changes
        setTimeout(async () => {
          console.log("Refreshing route details to confirm changes...");
          await fetchRouteDetails(selectedRoute);

          // Also refresh the routes list to get updated counts from server
          await fetchRoutes();
        }, 500);
      } else {
        console.error("Update failed with result:", result);
        throw new Error(result.ErrorMessage || "Failed to update route");
      }
    } catch (error: any) {
      console.error("Error saving changes:", error);
      toast({
        title: "Error",
        description:
          error.message || "Failed to save changes. Please try again.",
        variant: "destructive",
      });
    } finally {
      setSaving(false);
    }
  };

  // Initial load
  useEffect(() => {
    fetchRoutes();
  }, []);

  // State for route search
  const [routeSearchOpen, setRouteSearchOpen] = useState(false);
  const [routeSearchValue, setRouteSearchValue] = useState("");

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto p-6 max-w-7xl">
        {/* Header Section */}
        <div className="mb-8 bg-white rounded-lg shadow-sm p-6">
          <div className="flex items-center justify-between mb-4">
            <Button
              variant="outline"
              size="sm"
              onClick={() =>
                router.push("/distributiondelivery/route-management/routes")
              }
              className="hover:bg-gray-50"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Routes
            </Button>

            {hasUnsavedChanges() && (
              <Badge
                variant="outline"
                className="py-2 px-3 bg-yellow-50 border-yellow-300 text-yellow-800"
              >
                <Info className="h-4 w-4 mr-2" />
                You have unsaved changes
              </Badge>
            )}
          </div>

          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Customer Mapping
            </h1>
            <p className="text-gray-600 text-lg">
              Manage store assignments and configure route-customer
              relationships
            </p>
          </div>
        </div>

        {/* Route Selection with Search */}
        <Card className="mb-8 shadow-sm">
          <CardHeader className="pb-4">
            <CardTitle className="flex items-center gap-3 text-xl">
              <div className="p-2 bg-blue-50 rounded-lg">
                <Route className="h-5 w-5 text-blue-600" />
              </div>
              Select Route
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div className="space-y-2">
                <Label
                  htmlFor="route-select"
                  className="text-sm font-medium text-gray-700"
                >
                  Choose a route to manage store assignments
                </Label>
                <Popover
                  open={routeSearchOpen}
                  onOpenChange={setRouteSearchOpen}
                >
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      role="combobox"
                      aria-expanded={routeSearchOpen}
                      className="w-full justify-between h-10"
                      disabled={loadingRoutes}
                    >
                      {selectedRoute ? (
                        <span className="truncate">
                          {routes.find((r) => r.uid === selectedRoute)?.code} -{" "}
                          {routes.find((r) => r.uid === selectedRoute)?.name}
                        </span>
                      ) : (
                        <span className="text-gray-500">Select a route...</span>
                      )}
                      <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-[400px] p-0">
                    <Command>
                      <CommandInput
                        placeholder="Search routes by code or name..."
                        value={routeSearchValue}
                        onValueChange={setRouteSearchValue}
                        className="h-9"
                      />
                      <CommandEmpty>No route found.</CommandEmpty>
                      <CommandGroup>
                        <ScrollArea className="h-[200px]">
                          {routes
                            .filter((route) => {
                              const search = routeSearchValue.toLowerCase();
                              return (
                                route.code.toLowerCase().includes(search) ||
                                route.name.toLowerCase().includes(search)
                              );
                            })
                            .map((route) => (
                              <CommandItem
                                key={route.uid}
                                value={route.uid}
                                onSelect={() => {
                                  handleRouteSelection(route.uid);
                                  setRouteSearchOpen(false);
                                  setRouteSearchValue("");
                                }}
                                className="flex items-center justify-between py-2 px-2 cursor-pointer hover:bg-gray-50"
                              >
                                <div className="flex flex-col">
                                  <span className="font-medium">
                                    {route.code}
                                  </span>
                                  <span className="text-sm text-gray-600">
                                    {route.name}
                                  </span>
                                </div>
                              </CommandItem>
                            ))}
                        </ScrollArea>
                      </CommandGroup>
                    </Command>
                  </PopoverContent>
                </Popover>
              </div>

              {selectedRoute && currentRouteData && (
                <div className="bg-gradient-to-br from-blue-50 to-indigo-50 p-5 rounded-lg border border-blue-200">
                  <h3 className="text-sm font-semibold text-gray-700 mb-3">
                    Route Information
                  </h3>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="space-y-1">
                      <span className="text-xs text-gray-500 uppercase tracking-wider">
                        Route Code
                      </span>
                      <p className="font-semibold text-gray-900">
                        {currentRouteData.Route?.Code}
                      </p>
                    </div>
                    <div className="space-y-1">
                      <span className="text-xs text-gray-500 uppercase tracking-wider">
                        Status
                      </span>
                      <Badge
                        variant={
                          currentRouteData.Route?.IsActive
                            ? "default"
                            : "secondary"
                        }
                        className="w-fit"
                      >
                        {currentRouteData.Route?.Status || "Active"}
                      </Badge>
                    </div>
                    <div className="space-y-1">
                      <span className="text-xs text-gray-500 uppercase tracking-wider">
                        Assigned Stores
                      </span>
                      <p className="font-semibold text-gray-900">
                        {assignedStores.size}
                      </p>
                    </div>
                    <div className="space-y-1">
                      <span className="text-xs text-gray-500 uppercase tracking-wider">
                        Role
                      </span>
                      <p className="font-semibold text-gray-900">
                        {currentRouteData.Route?.RoleUID || "N/A"}
                      </p>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Store Assignment Section */}
        {selectedRoute && (
          <Card className="shadow-sm">
            <CardHeader className="border-b bg-gray-50">
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-3 text-xl">
                  <div className="p-2 bg-green-50 rounded-lg">
                    <Store className="h-5 w-5 text-green-600" />
                  </div>
                  Store Assignment
                </CardTitle>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={resetToOriginal}
                    disabled={!hasUnsavedChanges()}
                    className="hover:bg-white"
                  >
                    <RefreshCw className="h-4 w-4 mr-2" />
                    Reset
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={selectAllVisible}
                    className="hover:bg-white"
                  >
                    <Check className="h-4 w-4 mr-2" />
                    Select All
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={deselectAll}
                    className="hover:bg-white"
                  >
                    <X className="h-4 w-4 mr-2" />
                    Clear All
                  </Button>
                  <Button
                    onClick={saveChanges}
                    disabled={saving || !hasUnsavedChanges()}
                    className="bg-blue-600 hover:bg-blue-700 text-white"
                  >
                    {saving ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Saving...
                      </>
                    ) : (
                      <>
                        <Save className="h-4 w-4 mr-2" />
                        Save Changes
                      </>
                    )}
                  </Button>
                </div>
              </div>
            </CardHeader>
            <CardContent className="p-6">
              {/* Filters and Search */}
              <div className="space-y-4 mb-6">
                <div className="flex gap-3">
                  <div className="flex-1">
                    <div className="relative">
                      <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-5 w-5" />
                      <Input
                        placeholder="Search stores by code, name, or area..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="pl-10 h-11 text-base"
                      />
                    </div>
                  </div>
                  <Select
                    value={filterType}
                    onValueChange={(value: any) => setFilterType(value)}
                  >
                    <SelectTrigger className="w-[200px] h-11">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">
                        <div className="flex items-center gap-2">
                          <Filter className="h-4 w-4" />
                          All Stores
                        </div>
                      </SelectItem>
                      <SelectItem value="assigned">
                        <div className="flex items-center gap-2">
                          <Check className="h-4 w-4 text-green-600" />
                          Assigned Only
                        </div>
                      </SelectItem>
                      <SelectItem value="unassigned">
                        <div className="flex items-center gap-2">
                          <X className="h-4 w-4 text-gray-600" />
                          Unassigned Only
                        </div>
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {/* Statistics Cards */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                  <div className="bg-gradient-to-br from-blue-50 to-blue-100 p-4 rounded-lg border border-blue-200">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-xs font-medium text-blue-600 uppercase tracking-wider">
                          Loaded Stores
                        </p>
                        <p className="text-2xl font-bold text-blue-900 mt-1">
                          {displayedStores.length}
                          {storesPagination.totalCount > 0 && (
                            <span className="text-sm text-blue-600 ml-1">
                              / {storesPagination.totalCount}
                            </span>
                          )}
                        </p>
                      </div>
                      <div className="p-2 bg-blue-200 rounded-lg">
                        <Store className="h-4 w-4 text-blue-700" />
                      </div>
                    </div>
                  </div>

                  <div className="bg-gradient-to-br from-green-50 to-green-100 p-4 rounded-lg border border-green-200">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-xs font-medium text-green-600 uppercase tracking-wider">
                          Originally Assigned
                        </p>
                        <p className="text-2xl font-bold text-green-900 mt-1">
                          {assignedStores.size}
                        </p>
                      </div>
                      <div className="p-2 bg-green-200 rounded-lg">
                        <Check className="h-4 w-4 text-green-700" />
                      </div>
                    </div>
                  </div>

                  <div className="bg-gradient-to-br from-yellow-50 to-yellow-100 p-4 rounded-lg border border-yellow-200">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-xs font-medium text-yellow-600 uppercase tracking-wider">
                          Selected
                        </p>
                        <p className="text-2xl font-bold text-yellow-900 mt-1">
                          {selectedStores.size}
                        </p>
                      </div>
                      <div className="p-2 bg-yellow-200 rounded-lg">
                        <MapPin className="h-4 w-4 text-yellow-700" />
                      </div>
                    </div>
                  </div>

                  <div className="bg-gradient-to-br from-gray-50 to-gray-100 p-4 rounded-lg border border-gray-200">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-xs font-medium text-gray-600 uppercase tracking-wider">
                          Changes
                        </p>
                        <div className="flex items-baseline gap-2 mt-1">
                          {(() => {
                            let added = 0;
                            let removed = 0;

                            // Count additions (in selected but not in assigned)
                            selectedStores.forEach((uid) => {
                              if (!assignedStores.has(uid)) added++;
                            });

                            // Count removals (in assigned but not in selected)
                            assignedStores.forEach((uid) => {
                              if (!selectedStores.has(uid)) removed++;
                            });

                            if (added === 0 && removed === 0) {
                              return (
                                <p className="text-2xl font-bold text-gray-900">
                                  0
                                </p>
                              );
                            }

                            return (
                              <div className="flex gap-2">
                                {added > 0 && (
                                  <span className="text-lg font-bold text-green-700">
                                    +{added}
                                  </span>
                                )}
                                {removed > 0 && (
                                  <span className="text-lg font-bold text-red-700">
                                    -{removed}
                                  </span>
                                )}
                              </div>
                            );
                          })()}
                        </div>
                      </div>
                      <div className="p-2 bg-gray-200 rounded-lg">
                        <RefreshCw className="h-4 w-4 text-gray-700" />
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Store List */}
              {loadingStores && displayedStores.length === 0 ? (
                <div className="space-y-3">
                  {[...Array(5)].map((_, i) => (
                    <Skeleton key={i} className="h-16" />
                  ))}
                </div>
              ) : (
                <div className="border border-gray-200 rounded-lg bg-white">
                  <ScrollArea
                    className="h-[500px] px-4 py-2"
                    ref={scrollAreaRef}
                    onScrollCapture={handleScroll}
                  >
                    <div className="space-y-2 pr-2">
                      {getFilteredStores().map((store, index) => {
                        const isAssigned = assignedStores.has(store.uid);
                        const isSelected = selectedStores.has(store.uid);
                        const hasChanged = isAssigned !== isSelected;

                        return (
                          <div
                            key={`${store.uid}-${index}`}
                            className={cn(
                              "flex items-center justify-between p-4 rounded-lg border-2 transition-all duration-200",
                              isSelected &&
                                hasChanged &&
                                "bg-yellow-50 border-yellow-300 shadow-sm",
                              isSelected &&
                                !hasChanged &&
                                "bg-green-50 border-green-300",
                              !isSelected &&
                                isAssigned &&
                                "bg-red-50 border-red-200",
                              !isSelected &&
                                !isAssigned &&
                                "bg-white border-gray-200 hover:border-gray-300 hover:shadow-sm"
                            )}
                          >
                            <div className="flex items-center gap-4">
                              <Checkbox
                                checked={isSelected}
                                onCheckedChange={() =>
                                  handleStoreToggle(store.uid)
                                }
                                className="h-5 w-5"
                              />
                              <div className="flex-1">
                                <div className="flex items-center gap-3 mb-1">
                                  <span className="font-semibold text-gray-900">
                                    {store.code}
                                  </span>
                                  <span className="text-gray-600">
                                    {store.name}
                                  </span>
                                  {isAssigned && !hasChanged && (
                                    <Badge className="bg-green-100 text-green-700 border-green-300">
                                      Currently Assigned
                                    </Badge>
                                  )}
                                  {hasChanged && (
                                    <Badge
                                      variant="outline"
                                      className={cn(
                                        "font-medium",
                                        isSelected
                                          ? "border-yellow-400 bg-yellow-100 text-yellow-700"
                                          : "border-red-400 bg-red-100 text-red-700"
                                      )}
                                    >
                                      {isSelected
                                        ? "+ To be Added"
                                        : "- To be Removed"}
                                    </Badge>
                                  )}
                                </div>
                                {store.area && (
                                  <p className="text-sm text-gray-500 flex items-center gap-1">
                                    <MapPin className="h-3 w-3" />
                                    {store.area}
                                  </p>
                                )}
                              </div>
                            </div>
                            <div className="flex items-center gap-2">
                              <Badge
                                variant={
                                  store.isActive ? "default" : "secondary"
                                }
                              >
                                {store.isActive ? "Active" : "Inactive"}
                              </Badge>
                            </div>
                          </div>
                        );
                      })}

                      {getFilteredStores().length === 0 && !loadingStores && (
                        <div className="text-center py-12">
                          <div className="inline-flex items-center justify-center w-16 h-16 bg-gray-100 rounded-full mb-4">
                            <Store className="h-8 w-8 text-gray-400" />
                          </div>
                          <h3 className="text-lg font-medium text-gray-900 mb-1">
                            No stores found
                          </h3>
                          <p className="text-gray-500">
                            Try adjusting your search or filter criteria
                          </p>
                        </div>
                      )}

                      {/* Auto-loading indicator */}
                      {loadingMoreStores && (
                        <div className="text-center py-4 border-t border-gray-200">
                          <div className="flex items-center justify-center text-sm text-gray-600">
                            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                            Loading more stores...
                          </div>
                        </div>
                      )}

                      {/* End of list indicator */}
                      {!storesPagination.hasMore &&
                        displayedStores.length > 0 &&
                        !loadingMoreStores && (
                          <div className="text-center py-4 border-t border-gray-200">
                            <p className="text-sm text-gray-500">
                              All stores loaded ({displayedStores.length} total)
                            </p>
                          </div>
                        )}
                    </div>
                  </ScrollArea>
                </div>
              )}
            </CardContent>
          </Card>
        )}

        {/* Empty State */}
        {!selectedRoute && !loadingRoutes && (
          <Card className="shadow-sm">
            <CardContent className="py-16">
              <div className="text-center max-w-md mx-auto">
                <div className="inline-flex items-center justify-center w-20 h-20 bg-gray-100 rounded-full mb-6">
                  <Route className="h-10 w-10 text-gray-400" />
                </div>
                <h3 className="text-2xl font-semibold mb-3 text-gray-900">
                  No Route Selected
                </h3>
                <p className="text-gray-600 text-lg">
                  Select a route from the dropdown above to view and manage
                  store assignments.
                </p>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
