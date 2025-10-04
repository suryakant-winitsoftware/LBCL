"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useRouter, useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import { useToast } from "@/components/ui/use-toast";
import {
  ArrowLeft,
  Save,
  Building2,
  Search,
  Check,
  ChevronDown,
  Users,
  MapPin,
  Target,
  Calendar,
  Package
} from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Popover,
  PopoverContent,
  PopoverTrigger
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList
} from "@/components/ui/command";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Alert, AlertDescription } from "@/components/ui/alert";
import targetService from "@/services/targetService";
import type { Target } from "@/services/targetService";
import { api } from "@/services/api";
import {
  organizationService,
  Organization,
  OrgType
} from "@/services/organizationService";
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  OrganizationLevel
} from "@/utils/organizationHierarchyUtils";
import ProductAttributes from "@/components/sku/ProductAttributes";

interface DropdownOption {
  value: string;
  label: string;
  code?: string;
}

export default function EditTargetPage() {
  const router = useRouter();
  const { toast } = useToast();
  const params = useParams();
  const targetId = parseInt(params.id as string);
  const [loading, setLoading] = useState(false);
  const [loadingTarget, setLoadingTarget] = useState(true);

  // Loading states
  const [loadingStates, setLoadingStates] = useState({
    organizations: false,
    employees: false,
    routes: false,
    customers: false
  });

  // Organization hierarchy state
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([]);
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]);
  const [selectedOrgUID, setSelectedOrgUID] = useState("");

  // Dropdown data with pagination support
  const [dropdowns, setDropdowns] = useState({
    employees: [] as DropdownOption[],
    routes: [] as DropdownOption[],
    customers: [] as DropdownOption[]
  });

  // Pagination states
  const [employeesPagination, setEmployeesPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: true,
    isLoadingMore: false
  });

  const [customersPagination, setCustomersPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: true,
    isLoadingMore: false
  });

  // Search states
  const [searchTerms, setSearchTerms] = useState({
    employees: "",
    routes: "",
    customers: ""
  });

  // Popover states
  const [popoverStates, setPopoverStates] = useState({
    userType: false,
    userSelection: false,
    customerSelection: false
  });

  // Product hierarchy state
  const [selectedProductAttributes, setSelectedProductAttributes] = useState<
    Array<{
      type: string;
      code: string;
      value: string;
      uid?: string;
      level: number;
      fieldName?: string;
    }>
  >([]);

  const [target, setTarget] = useState<Target>({
    UserLinkedType: "Route",
    UserLinkedUid: "",
    CustomerLinkedType: "Customer",
    CustomerLinkedUid: "",
    ItemLinkedItemType: "",
    ItemLinkedItemUid: "",
    TargetMonth: new Date().getMonth() + 1,
    TargetYear: new Date().getFullYear(),
    TargetAmount: 0,
    Status: "Not Started",
    Notes: ""
  });

  // Load organization data
  const loadOrganizationData = async (skipDefault = false) => {
    setLoadingStates((prev) => ({ ...prev, organizations: true }));
    try {
      const [typesResult, orgsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 1000)
      ]);

      const filteredOrganizations = orgsResult.data.filter(
        (org) => org.ShowInTemplate === true
      );

      const filteredOrgTypes = typesResult.filter(
        (type) => type.ShowInTemplate !== false
      );

      setOrgTypes(filteredOrgTypes);
      setOrganizations(filteredOrganizations);

      const initialLevels = initializeOrganizationHierarchy(
        filteredOrganizations,
        filteredOrgTypes
      );
      setOrgLevels(initialLevels);
      
      // Set EPIC01 as default organization if not loading saved data
      if (!skipDefault) {
        const epic01Org = filteredOrganizations.find(
          org => org.Code === 'EPIC01' || org.UID === 'EPIC01' || org.Name === 'EPIC01'
        );
        
        if (epic01Org && initialLevels.length > 0) {
          // Find which level EPIC01 belongs to
          const levelIndex = initialLevels.findIndex(level => 
            level.organizations.some(org => org.UID === epic01Org.UID)
          );
          
          if (levelIndex !== -1) {
            // Set the selected organization
            const newSelectedOrgs = [];
            newSelectedOrgs[levelIndex] = epic01Org.UID;
            setSelectedOrgs(newSelectedOrgs);
            setSelectedOrgUID(epic01Org.UID);
            
            // Load dependent dropdowns for EPIC01
            await loadDependentDropdowns(epic01Org.UID);
          }
        }
      }
    } catch (error) {
      console.error("Error fetching organization data:", error);
      toast({
        title: "Warning",
        description: "Could not load organization hierarchy",
        variant: "default"
      });
    } finally {
      setLoadingStates((prev) => ({ ...prev, organizations: false }));
    }
  };

  // Handle organization selection
  const handleOrganizationChange = (levelIndex: number, value: string) => {
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

    const finalSelectedOrg = getFinalSelectedOrganization(updatedSelectedOrgs);
    if (finalSelectedOrg) {
      setSelectedOrgUID(finalSelectedOrg);
      loadDependentDropdowns(finalSelectedOrg);
    }
  };

  // Load employees with pagination and search
  const loadEmployees = useCallback(
    async (
      orgUID: string,
      page: number = 1,
      append: boolean = false,
      searchTerm: string = ""
    ) => {
      if (page === 1) {
        setLoadingStates((prev) => ({ ...prev, employees: true }));
      } else {
        setEmployeesPagination((prev) => ({ ...prev, isLoadingMore: true }));
      }

      try {
        const data = await api.dropdown.getEmployee(orgUID, false);
        if (data.IsSuccess && data.Data) {
          let allEmployees = data.Data.map((emp: any) => ({
            value: emp.UID || emp.uid,
            label:
              emp.Name ||
              emp.name ||
              `${emp.FirstName || ""} ${emp.LastName || ""}`.trim(),
            code: emp.Code || emp.code
          }));

          // Apply search filter if provided
          if (searchTerm) {
            allEmployees = allEmployees.filter(
              (emp: DropdownOption) =>
                emp.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
                (emp.code &&
                  emp.code.toLowerCase().includes(searchTerm.toLowerCase()))
            );
          }

          // Implement client-side pagination
          const startIndex = (page - 1) * employeesPagination.pageSize;
          const endIndex = startIndex + employeesPagination.pageSize;
          const paginatedEmployees = allEmployees.slice(startIndex, endIndex);

          setDropdowns((prev) => ({
            ...prev,
            employees: append
              ? [...prev.employees, ...paginatedEmployees]
              : paginatedEmployees
          }));

          setEmployeesPagination((prev) => ({
            ...prev,
            currentPage: page,
            totalCount: allEmployees.length,
            hasMore: endIndex < allEmployees.length,
            isLoadingMore: false
          }));
        }
      } catch (error) {
        console.error("Error loading employees:", error);
        setEmployeesPagination((prev) => ({ ...prev, isLoadingMore: false }));
      } finally {
        setLoadingStates((prev) => ({ ...prev, employees: false }));
      }
    },
    [employeesPagination.pageSize]
  );

  // Load routes with search
  const loadRoutes = useCallback(
    async (orgUID: string, searchTerm: string = "") => {
      setLoadingStates((prev) => ({ ...prev, routes: true }));
      try {
        const data = await api.dropdown.getRoute(orgUID);
        if (data.IsSuccess && data.Data) {
          let routes = data.Data.map((route: any) => ({
            value: route.UID || route.uid,
            label: route.Name || route.name || route.Code || route.code,
            code: route.Code || route.code
          }));

          // Apply search filter if provided
          if (searchTerm) {
            routes = routes.filter(
              (route: DropdownOption) =>
                route.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
                (route.code &&
                  route.code.toLowerCase().includes(searchTerm.toLowerCase()))
            );
          }

          setDropdowns((prev) => ({ ...prev, routes }));
        }
      } catch (error) {
        console.error("Error loading routes:", error);
      } finally {
        setLoadingStates((prev) => ({ ...prev, routes: false }));
      }
    },
    []
  );

  // Load customers with pagination and search
  const loadCustomers = useCallback(
    async (
      orgUID: string,
      page: number = 1,
      append: boolean = false,
      searchTerm: string = ""
    ) => {
      if (page === 1) {
        setLoadingStates((prev) => ({ ...prev, customers: true }));
      } else {
        setCustomersPagination((prev) => ({ ...prev, isLoadingMore: true }));
      }

      try {
        const data = await api.dropdown.getCustomer(orgUID);
        if (data.IsSuccess && data.Data) {
          let allCustomers = data.Data.map((customer: any) => ({
            value:
              customer.UID || customer.uid || customer.Code || customer.code,
            label:
              customer.Name || customer.name || customer.Code || customer.code,
            code: customer.Code || customer.code
          }));

          // Apply search filter if provided
          if (searchTerm) {
            allCustomers = allCustomers.filter(
              (customer: DropdownOption) =>
                customer.label
                  .toLowerCase()
                  .includes(searchTerm.toLowerCase()) ||
                (customer.code &&
                  customer.code
                    .toLowerCase()
                    .includes(searchTerm.toLowerCase()))
            );
          }

          // Implement client-side pagination
          const startIndex = (page - 1) * customersPagination.pageSize;
          const endIndex = startIndex + customersPagination.pageSize;
          const paginatedCustomers = allCustomers.slice(startIndex, endIndex);

          setDropdowns((prev) => ({
            ...prev,
            customers: append
              ? [...prev.customers, ...paginatedCustomers]
              : paginatedCustomers
          }));

          setCustomersPagination((prev) => ({
            ...prev,
            currentPage: page,
            totalCount: allCustomers.length,
            hasMore: endIndex < allCustomers.length,
            isLoadingMore: false
          }));
        }
      } catch (error) {
        console.error("Error loading customers:", error);
        setCustomersPagination((prev) => ({ ...prev, isLoadingMore: false }));
      } finally {
        setLoadingStates((prev) => ({ ...prev, customers: false }));
      }
    },
    [customersPagination.pageSize]
  );

  // Load dependent dropdowns
  const loadDependentDropdowns = useCallback(
    async (orgUID: string) => {
      // Reset pagination when organization changes
      setEmployeesPagination((prev) => ({
        ...prev,
        currentPage: 1,
        hasMore: true
      }));
      setCustomersPagination((prev) => ({
        ...prev,
        currentPage: 1,
        hasMore: true
      }));

      await Promise.all([
        loadEmployees(orgUID, 1, false, searchTerms.employees),
        loadRoutes(orgUID, searchTerms.routes),
        loadCustomers(orgUID, 1, false, searchTerms.customers)
      ]);
    },
    [loadEmployees, loadRoutes, loadCustomers, searchTerms]
  );

  // Load more functions for infinite scroll
  const loadMoreEmployees = useCallback(async () => {
    if (
      !employeesPagination.hasMore ||
      employeesPagination.isLoadingMore ||
      !selectedOrgUID
    ) {
      return;
    }
    const nextPage = employeesPagination.currentPage + 1;
    await loadEmployees(selectedOrgUID, nextPage, true, searchTerms.employees);
  }, [
    employeesPagination,
    selectedOrgUID,
    loadEmployees,
    searchTerms.employees
  ]);

  const loadMoreCustomers = useCallback(async () => {
    if (
      !customersPagination.hasMore ||
      customersPagination.isLoadingMore ||
      !selectedOrgUID
    ) {
      return;
    }
    const nextPage = customersPagination.currentPage + 1;
    await loadCustomers(selectedOrgUID, nextPage, true, searchTerms.customers);
  }, [
    customersPagination,
    selectedOrgUID,
    loadCustomers,
    searchTerms.customers
  ]);

  // Load target data and pre-populate form
  const loadTarget = async () => {
    try {
      setLoadingTarget(true);
      
      // First load organizations
      await loadOrganizationData(true); // Skip auto-setting EPIC01
      
      // Then load the target
      const response = await targetService.getTargetById(targetId);
      const targetData = response.data;
      setTarget(targetData);
      
      // Pre-populate product attributes if they exist
      if (targetData.ItemLinkedItemType && targetData.ItemLinkedItemUid) {
        setSelectedProductAttributes([{
          type: targetData.ItemLinkedItemType,
          code: targetData.ItemLinkedItemUid,
          value: targetData.ItemLinkedItemType,
          uid: targetData.ItemLinkedItemUid,
          level: 1,
          fieldName: "Level1"
        }]);
      }
      
      // Set organization (default to EPIC01 for now)
      // In production, you'd determine the actual org from the saved data
      const orgToUse = 'EPIC01';
      
      // Find EPIC01 in loaded organizations
      const [typesResult, orgsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 1000)
      ]);
      
      const filteredOrganizations = orgsResult.data.filter(
        (org) => org.ShowInTemplate === true
      );
      
      const epic01Org = filteredOrganizations.find(
        org => org.Code === orgToUse || org.UID === orgToUse || org.Name === orgToUse
      );
      
      if (epic01Org) {
        // Set selected organization
        setSelectedOrgUID(epic01Org.UID);
        
        // Find and set the org in hierarchy
        const orgLevelsCopy = [...orgLevels];
        const levelIndex = orgLevelsCopy.findIndex(level => 
          level.organizations.some(org => org.UID === epic01Org.UID)
        );
        
        if (levelIndex !== -1) {
          const newSelectedOrgs = [];
          newSelectedOrgs[levelIndex] = epic01Org.UID;
          setSelectedOrgs(newSelectedOrgs);
        }
        
        // Load dropdowns for the organization
        await Promise.all([
          loadEmployees(epic01Org.UID, 1, false, ""),
          loadRoutes(epic01Org.UID, ""),
          loadCustomers(epic01Org.UID, 1, false, "")
        ]);
      }
      
    } catch (error) {
      console.error('Error loading target:', error);
      toast({
        title: 'Error',
        description: 'Failed to load target',
        variant: 'destructive',
      });
      router.push('/distributiondelivery/route-management/target');
    } finally {
      setLoadingTarget(false);
    }
  };

  // Load target data on mount
  useEffect(() => {
    loadTarget();
  }, [targetId]);

  // Debounced search effects
  useEffect(() => {
    if (!selectedOrgUID) return;

    const timeoutId = setTimeout(() => {
      loadEmployees(selectedOrgUID, 1, false, searchTerms.employees);
    }, 300);
    return () => clearTimeout(timeoutId);
  }, [searchTerms.employees, selectedOrgUID, loadEmployees]);

  useEffect(() => {
    if (!selectedOrgUID) return;

    const timeoutId = setTimeout(() => {
      loadRoutes(selectedOrgUID, searchTerms.routes);
    }, 300);
    return () => clearTimeout(timeoutId);
  }, [searchTerms.routes, selectedOrgUID, loadRoutes]);

  useEffect(() => {
    if (!selectedOrgUID) return;

    const timeoutId = setTimeout(() => {
      loadCustomers(selectedOrgUID, 1, false, searchTerms.customers);
    }, 300);
    return () => clearTimeout(timeoutId);
  }, [searchTerms.customers, selectedOrgUID, loadCustomers]);

  // Scroll handlers for lazy loading
  const handleEmployeeScroll = useCallback(
    (e: React.UIEvent<HTMLDivElement>) => {
      const element = e.currentTarget;
      const threshold = 0.8; // Load more when 80% scrolled
      const scrolled =
        (element.scrollTop + element.clientHeight) / element.scrollHeight;

      if (scrolled >= threshold) {
        loadMoreEmployees();
      }
    },
    [loadMoreEmployees]
  );

  const handleCustomerScroll = useCallback(
    (e: React.UIEvent<HTMLDivElement>) => {
      const element = e.currentTarget;
      const threshold = 0.8;
      const scrolled =
        (element.scrollTop + element.clientHeight) / element.scrollHeight;

      if (scrolled >= threshold) {
        loadMoreCustomers();
      }
    },
    [loadMoreCustomers]
  );

  // Handle product attributes change
  const handleProductAttributesChange = useCallback((attributes: any[]) => {
    setSelectedProductAttributes(attributes);

    // Update target with the first selected product attribute (most specific)
    const validAttributes = attributes.filter(
      (attr) => attr.code && attr.value
    );
    if (validAttributes.length > 0) {
      // Use the most specific (last) level for the target
      const mostSpecific = validAttributes[validAttributes.length - 1];
      setTarget((prev) => ({
        ...prev,
        ItemLinkedItemType: mostSpecific.type,
        ItemLinkedItemUid: mostSpecific.uid || mostSpecific.code
      }));
    } else {
      // Clear if no valid selection
      setTarget((prev) => ({
        ...prev,
        ItemLinkedItemType: "",
        ItemLinkedItemUid: ""
      }));
    }
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validate organization selection
    if (!selectedOrgUID) {
      toast({
        title: "Validation Error",
        description: "Please select an organization",
        variant: "destructive"
      });
      return;
    }

    // Validate user/route selection based on type
    if (!target.UserLinkedUid) {
      const userTypeLabel =
        target.UserLinkedType === "Route"
          ? "route"
          : target.UserLinkedType === "Employee"
          ? "employee"
          : "user";
      toast({
        title: "Validation Error",
        description: `Please select a ${userTypeLabel}`,
        variant: "destructive"
      });
      return;
    }

    // Verify the selected user/route exists in the loaded dropdown
    let isValidSelection = false;
    if (target.UserLinkedType === "Route") {
      isValidSelection = dropdowns.routes.some(
        (route) => route.value === target.UserLinkedUid
      );
    } else if (target.UserLinkedType === "Employee") {
      isValidSelection = dropdowns.employees.some(
        (emp) => emp.value === target.UserLinkedUid
      );
    }

    if (!isValidSelection) {
      toast({
        title: "Validation Error",
        description: `Selected ${target.UserLinkedType.toLowerCase()} is not valid for the chosen organization`,
        variant: "destructive"
      });
      return;
    }

    // Verify customer if selected
    if (target.CustomerLinkedUid && target.CustomerLinkedUid.trim() !== "") {
      const isValidCustomer = dropdowns.customers.some(
        (customer) => customer.value === target.CustomerLinkedUid
      );
      if (!isValidCustomer) {
        toast({
          title: "Validation Error",
          description:
            "Selected customer is not valid for the chosen organization",
          variant: "destructive"
        });
        return;
      }
    }

    if (target.TargetAmount <= 0) {
      toast({
        title: "Validation Error",
        description: "Target amount must be greater than 0",
        variant: "destructive"
      });
      return;
    }

    try {
      setLoading(true);
      await targetService.updateTarget(targetId, target);
      toast({
        title: "Success",
        description: "Target updated successfully"
      });
      router.push("/distributiondelivery/route-management/target");
    } catch (error) {
      console.error("Error updating target:", error);
      toast({
        title: "Error",
        description: "Failed to update target",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field: keyof Target, value: string | number) => {
    setTarget((prev) => ({
      ...prev,
      [field]: value
    }));

    // Reset UserLinkedUid when UserLinkedType changes
    if (field === "UserLinkedType") {
      setTarget((prev) => ({
        ...prev,
        UserLinkedUid: ""
      }));
    }
  };

  const currentYear = new Date().getFullYear();

  if (loadingTarget) {
    return (
      <div className="min-h-screen">
        <div className="bg-white shadow-sm">
          <div className="px-6 py-4">
            <div className="flex items-center justify-between">
              <h1 className="text-2xl font-normal text-gray-900">
                Edit Target
              </h1>
              <Button
                variant="ghost"
                size="sm"
                onClick={() =>
                  router.push("/distributiondelivery/route-management/target")
                }
                className="text-gray-600 hover:text-gray-900"
              >
                <ArrowLeft className="h-4 w-4 mr-2" />
                Back to Targets
              </Button>
            </div>
          </div>
        </div>
        <div className="px-6 py-8 max-w-6xl mx-auto">
          <div className="space-y-8">
            {/* Organization Skeleton */}
            <div className="bg-white border border-gray-200 rounded-lg p-6">
              <Skeleton className="h-6 w-48 mb-4" />
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="space-y-2">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-10 w-full" />
                  </div>
                ))}
              </div>
            </div>
            
            {/* Target Assignment Skeleton */}
            <div className="bg-white border border-gray-200 rounded-lg p-6">
              <Skeleton className="h-6 w-40 mb-4" />
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {[1, 2].map((i) => (
                  <div key={i} className="space-y-2">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-11 w-full" />
                  </div>
                ))}
              </div>
            </div>
            
            {/* Target Details Skeleton */}
            <div className="bg-white border border-gray-200 rounded-lg p-6">
              <Skeleton className="h-6 w-32 mb-4" />
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                {[1, 2, 3, 4].map((i) => (
                  <div key={i} className="space-y-2">
                    <Skeleton className="h-4 w-28" />
                    <Skeleton className="h-11 w-full" />
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen">
      {/* Header */}
      <div className="bg-white shadow-sm">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-normal text-gray-900">
              Edit Target
            </h1>
            <Button
              variant="ghost"
              size="sm"
              onClick={() =>
                router.push("/distributiondelivery/route-management/target")
              }
              className="text-gray-600 hover:text-gray-900"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Targets
            </Button>
          </div>
        </div>
      </div>

      <div className="px-6 py-8 max-w-6xl mx-auto">
        <form onSubmit={handleSubmit} className="space-y-8">
          {/* Organization Selection */}
          <div className="space-y-6">
            <div className="mb-6">
              <h2 className="text-xl font-medium text-gray-900">
                Organization Hierarchy
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                Select the organization structure for this target
              </p>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg p-6 space-y-6">
              {loadingStates.organizations ? (
                <div className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {[1, 2, 3].map((i) => (
                      <div key={i} className="space-y-2">
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-10 w-full" />
                      </div>
                    ))}
                  </div>
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-48" />
                    <div className="flex gap-2">
                      <Skeleton className="h-3 w-24" />
                      <Skeleton className="h-3 w-32" />
                    </div>
                  </div>
                </div>
              ) : orgLevels.length > 0 ? (
                <div className="space-y-6">
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {orgLevels.map((level, index) => (
                      <div key={level.orgTypeUID} className="space-y-3">
                        <Label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                          <div className="h-2 w-2 rounded-full bg-blue-500" />
                          {level.orgTypeName}
                          {index === 0 && (
                            <span className="text-red-500">*</span>
                          )}
                        </Label>
                        <Select
                          value={selectedOrgs[index] || ""}
                          onValueChange={(value) =>
                            handleOrganizationChange(index, value)
                          }
                        >
                          <SelectTrigger className="h-11 border-slate-200 focus:border-blue-500 focus:ring-blue-500/20">
                            <SelectValue
                              placeholder={`Choose ${level.orgTypeName.toLowerCase()}`}
                              className="text-sm"
                            />
                          </SelectTrigger>
                          <SelectContent>
                            {level.organizations.map((org) => (
                              <SelectItem
                                key={org.UID}
                                value={org.UID}
                                className="cursor-pointer"
                              >
                                <div className="flex items-center justify-between w-full">
                                  <span className="font-medium">
                                    {org.Name}
                                  </span>
                                  <Badge
                                    variant="outline"
                                    className="ml-2 text-xs"
                                  >
                                    {org.Code}
                                  </Badge>
                                </div>
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    ))}
                  </div>
                  {selectedOrgUID && (
                    <Alert className="border-green-200 bg-green-50">
                      <Check className="h-4 w-4 text-green-600" />
                      <AlertDescription className="text-green-800">
                        Organization hierarchy configured successfully. You can
                        now set up your target details.
                      </AlertDescription>
                    </Alert>
                  )}
                </div>
              ) : (
                <div className="text-center py-12">
                  <Building2 className="h-12 w-12 mx-auto mb-4 text-muted-foreground/50" />
                  <h3 className="text-lg font-medium mb-2">
                    No Organizations Available
                  </h3>
                  <p className="text-muted-foreground">
                    Please contact your administrator to set up organizations.
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* Target Assignment */}
          <div className="space-y-6">
            <div className="mb-6">
              <h2 className="text-xl font-medium text-gray-900">
                Target Assignment
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                Define who this target applies to and select customers
              </p>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg p-6 space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                    <Target className="h-4 w-4" />
                    Target Type
                    <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={target.UserLinkedType}
                    onValueChange={(value) =>
                      handleChange("UserLinkedType", value)
                    }
                  >
                    <SelectTrigger className="h-11 border-slate-200 focus:border-green-500 focus:ring-green-500/20">
                      <SelectValue placeholder="Select target type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Route" className="cursor-pointer">
                        <div className="flex items-center gap-2">
                          <MapPin className="h-4 w-4" />
                          Route
                        </div>
                      </SelectItem>
                      <SelectItem value="Employee" className="cursor-pointer">
                        <div className="flex items-center gap-2">
                          <Users className="h-4 w-4" />
                          Employee
                        </div>
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-3">
                  <Label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                    {target.UserLinkedType === "Route" ? (
                      <MapPin className="h-4 w-4" />
                    ) : (
                      <Users className="h-4 w-4" />
                    )}
                    {target.UserLinkedType === "Route" ? "Route" : "Employee"}
                    <span className="text-red-500">*</span>
                  </Label>
                  {!selectedOrgUID ? (
                    <div className="flex items-center justify-center h-11 border-2 border-dashed border-slate-300 rounded-lg bg-slate-50">
                      <p className="text-sm text-slate-500">
                        Select organization first
                      </p>
                    </div>
                  ) : loadingStates.employees || loadingStates.routes ? (
                    <div className="space-y-2">
                      <Skeleton className="h-11 w-full" />
                      <Skeleton className="h-3 w-32" />
                    </div>
                  ) : (
                    <Popover
                      open={popoverStates.userSelection}
                      onOpenChange={(open) =>
                        setPopoverStates((prev) => ({
                          ...prev,
                          userSelection: open
                        }))
                      }
                    >
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          role="combobox"
                          aria-expanded={popoverStates.userSelection}
                          className="w-full h-11 justify-between text-left font-normal border-slate-200 focus:border-green-500 focus:ring-green-500/20"
                        >
                          <span className="truncate">
                            {target.UserLinkedUid
                              ? (() => {
                                  const selectedItem =
                                    target.UserLinkedType === "Route"
                                      ? dropdowns.routes.find(
                                          (r) =>
                                            r.value === target.UserLinkedUid
                                        )
                                      : dropdowns.employees.find(
                                          (e) =>
                                            e.value === target.UserLinkedUid
                                        );
                                  return selectedItem?.label || "Select...";
                                })()
                              : `Choose ${target.UserLinkedType.toLowerCase()}`}
                          </span>
                          <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-full p-0" align="start">
                        <Command>
                          <div className="flex items-center border-b px-4 py-3 bg-slate-50">
                            <Search className="mr-3 h-4 w-4 shrink-0 text-slate-500" />
                            <CommandInput
                              placeholder={`Search ${target.UserLinkedType.toLowerCase()}s...`}
                              value={
                                target.UserLinkedType === "Route"
                                  ? searchTerms.routes
                                  : searchTerms.employees
                              }
                              onValueChange={(value) => {
                                setSearchTerms((prev) => ({
                                  ...prev,
                                  [target.UserLinkedType === "Route"
                                    ? "routes"
                                    : "employees"]: value
                                }));
                              }}
                              className="h-9 border-0 bg-transparent focus:ring-0"
                            />
                          </div>
                          <CommandEmpty>
                            <div className="flex flex-col items-center gap-3 py-8">
                              <Search className="h-8 w-8 text-slate-400" />
                              <div className="text-center">
                                <p className="font-medium">
                                  No {target.UserLinkedType.toLowerCase()}s
                                  found
                                </p>
                                <p className="text-sm text-muted-foreground">
                                  Try adjusting your search terms
                                </p>
                              </div>
                            </div>
                          </CommandEmpty>
                          <CommandGroup>
                            <CommandList
                              className="max-h-[280px] overflow-y-auto"
                              onScroll={
                                target.UserLinkedType === "Employee"
                                  ? handleEmployeeScroll
                                  : undefined
                              }
                            >
                              {(target.UserLinkedType === "Route"
                                ? dropdowns.routes
                                : dropdowns.employees
                              ).map((item) => (
                                <CommandItem
                                  key={item.value}
                                  value={item.label}
                                  onSelect={() => {
                                    handleChange("UserLinkedUid", item.value);
                                    setPopoverStates((prev) => ({
                                      ...prev,
                                      userSelection: false
                                    }));
                                  }}
                                  className="flex items-center gap-3 cursor-pointer py-3"
                                >
                                  <Check
                                    className={`h-4 w-4 shrink-0 ${
                                      target.UserLinkedUid === item.value
                                        ? "opacity-100 text-green-600"
                                        : "opacity-0"
                                    }`}
                                  />
                                  <div className="flex flex-col gap-1 flex-1 min-w-0">
                                    <div className="font-medium text-sm truncate">
                                      {item.label}
                                    </div>
                                    {item.code && (
                                      <Badge
                                        variant="outline"
                                        className="text-xs w-fit"
                                      >
                                        {item.code}
                                      </Badge>
                                    )}
                                  </div>
                                </CommandItem>
                              ))}
                              {target.UserLinkedType === "Employee" &&
                                employeesPagination.isLoadingMore && (
                                  <div className="flex flex-col items-center py-4 bg-slate-50 border-t space-y-2">
                                    <Skeleton className="h-4 w-24" />
                                    <div className="flex gap-2">
                                      <Skeleton className="h-3 w-16" />
                                      <Skeleton className="h-3 w-16" />
                                    </div>
                                  </div>
                                )}
                            </CommandList>
                          </CommandGroup>
                        </Command>
                      </PopoverContent>
                    </Popover>
                  )}
                </div>
              </div>

              <Separator />

              {/* Customer Selection */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-slate-700">
                    Customer Type
                  </Label>
                  <Select
                    value={target.CustomerLinkedType || "Customer"}
                    onValueChange={(value) =>
                      handleChange("CustomerLinkedType", value)
                    }
                  >
                    <SelectTrigger className="h-11 border-slate-200 focus:border-green-500 focus:ring-green-500/20">
                      <SelectValue placeholder="Select customer type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Customer">Customer</SelectItem>
                      <SelectItem value="Store">Store</SelectItem>
                      <SelectItem value="Outlet">Outlet</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-3">
                  <Label className="text-sm font-medium text-slate-700">
                    Customer (Optional)
                  </Label>
                  {!selectedOrgUID ? (
                    <div className="flex items-center justify-center h-11 border-2 border-dashed border-slate-300 rounded-lg bg-slate-50">
                      <p className="text-sm text-slate-500">
                        Select organization first
                      </p>
                    </div>
                  ) : loadingStates.customers ? (
                    <div className="space-y-2">
                      <Skeleton className="h-11 w-full" />
                      <Skeleton className="h-3 w-28" />
                    </div>
                  ) : (
                    <Popover
                      open={popoverStates.customerSelection}
                      onOpenChange={(open) =>
                        setPopoverStates((prev) => ({
                          ...prev,
                          customerSelection: open
                        }))
                      }
                    >
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          role="combobox"
                          aria-expanded={popoverStates.customerSelection}
                          className="w-full h-11 justify-between text-left font-normal border-slate-200 focus:border-green-500 focus:ring-green-500/20"
                        >
                          <span className="truncate">
                            {target.CustomerLinkedUid
                              ? dropdowns.customers.find(
                                  (c) => c.value === target.CustomerLinkedUid
                                )?.label || "Select customer..."
                              : "Choose customer (optional)"}
                          </span>
                          <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-full p-0" align="start">
                        <Command>
                          <div className="flex items-center border-b px-4 py-3 bg-slate-50">
                            <Search className="mr-3 h-4 w-4 shrink-0 text-slate-500" />
                            <CommandInput
                              placeholder="Search customers..."
                              value={searchTerms.customers}
                              onValueChange={(value) => {
                                setSearchTerms((prev) => ({
                                  ...prev,
                                  customers: value
                                }));
                              }}
                              className="h-9 border-0 bg-transparent focus:ring-0"
                            />
                          </div>
                          <CommandEmpty>
                            <div className="flex flex-col items-center gap-3 py-8">
                              <Search className="h-8 w-8 text-slate-400" />
                              <div className="text-center">
                                <p className="font-medium">
                                  No customers found
                                </p>
                                <p className="text-sm text-muted-foreground">
                                  Try adjusting your search terms
                                </p>
                              </div>
                            </div>
                          </CommandEmpty>
                          <CommandGroup>
                            <CommandList
                              className="max-h-[280px] overflow-y-auto"
                              onScroll={handleCustomerScroll}
                            >
                              {/* Clear selection option */}
                              <CommandItem
                                value="clear-selection"
                                onSelect={() => {
                                  handleChange("CustomerLinkedUid", "");
                                  setPopoverStates((prev) => ({
                                    ...prev,
                                    customerSelection: false
                                  }));
                                }}
                                className="flex items-center gap-3 cursor-pointer py-3 text-muted-foreground"
                              >
                                <Check
                                  className={`h-4 w-4 shrink-0 ${
                                    !target.CustomerLinkedUid
                                      ? "opacity-100 text-green-600"
                                      : "opacity-0"
                                  }`}
                                />
                                <div className="flex-1 min-w-0">
                                  <div className="text-sm italic">
                                    No specific customer
                                  </div>
                                </div>
                              </CommandItem>

                              {dropdowns.customers.map((customer) => (
                                <CommandItem
                                  key={customer.value}
                                  value={customer.label}
                                  onSelect={() => {
                                    handleChange(
                                      "CustomerLinkedUid",
                                      customer.value
                                    );
                                    setPopoverStates((prev) => ({
                                      ...prev,
                                      customerSelection: false
                                    }));
                                  }}
                                  className="flex items-center gap-3 cursor-pointer py-3"
                                >
                                  <Check
                                    className={`h-4 w-4 shrink-0 ${
                                      target.CustomerLinkedUid ===
                                      customer.value
                                        ? "opacity-100 text-green-600"
                                        : "opacity-0"
                                    }`}
                                  />
                                  <div className="flex flex-col gap-1 flex-1 min-w-0">
                                    <div className="font-medium text-sm truncate">
                                      {customer.label}
                                    </div>
                                    {customer.code && (
                                      <Badge
                                        variant="outline"
                                        className="text-xs w-fit"
                                      >
                                        {customer.code}
                                      </Badge>
                                    )}
                                  </div>
                                </CommandItem>
                              ))}
                              {customersPagination.isLoadingMore && (
                                <div className="flex flex-col items-center py-4 bg-slate-50 border-t space-y-2">
                                  <Skeleton className="h-4 w-24" />
                                  <div className="flex gap-2">
                                    <Skeleton className="h-3 w-16" />
                                    <Skeleton className="h-3 w-16" />
                                  </div>
                                </div>
                              )}
                            </CommandList>
                          </CommandGroup>
                        </Command>
                      </PopoverContent>
                    </Popover>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Product Selection */}
          <div className="space-y-6">
            <div className="mb-6">
              <h2 className="text-xl font-medium text-gray-900">
                Product Hierarchy
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                Select specific products or categories for this target (optional)
              </p>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg p-6">
              <ProductAttributes
                onAttributesChange={handleProductAttributesChange}
                fieldNamePattern="Level{n}"
                enableMultiSelect={false}
                initialMaxLevels={6}
                showLevelNumbers={true}
                levelLabelGenerator={(level, typeName) =>
                  `${typeName} (Level ${level})`
                }
                initialValues={{
                  itemType: target.ItemLinkedItemType,
                  itemUid: target.ItemLinkedItemUid
                }}
              />

              {/* Product Selection Summary */}
              {selectedProductAttributes.length > 0 && (
                <div className="mt-6 space-y-4">
                  <div className="flex items-center gap-2">
                    <h4 className="font-medium text-slate-700">
                      Selected Product Path
                    </h4>
                    <Badge variant="outline" className="text-xs">
                      {
                        selectedProductAttributes.filter(
                          (attr) => attr.code && attr.value
                        ).length
                      }{" "}
                      levels
                    </Badge>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {selectedProductAttributes
                      .filter((attr) => attr.code && attr.value)
                      .map((attr) => (
                        <Badge
                          key={`${attr.level}-${attr.code}`}
                          variant="secondary"
                          className="px-3 py-1 bg-purple-100 text-purple-800 border-purple-200"
                        >
                          <span className="font-medium">{attr.type}:</span>
                          <span className="ml-1">{attr.value}</span>
                          <span className="ml-1 text-purple-600">
                            ({attr.code})
                          </span>
                        </Badge>
                      ))}
                  </div>
                  {target.ItemLinkedItemType && (
                    <Alert className="border-purple-200 bg-purple-50">
                      <Package className="h-4 w-4 text-purple-600" />
                      <AlertDescription className="text-purple-800">
                        Target will be applied to:{" "}
                        <strong>{target.ItemLinkedItemType}</strong>
                        {target.ItemLinkedItemUid && (
                          <span> (ID: {target.ItemLinkedItemUid})</span>
                        )}
                      </AlertDescription>
                    </Alert>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Target Details */}
          <div className="space-y-6">
            <div className="mb-6">
              <h2 className="text-xl font-medium text-gray-900">
                Target Details
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                Set the target period, amounts, and status
              </p>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg p-6 space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <div className="space-y-3">
                  <Label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                    <Calendar className="h-4 w-4" />
                    Target Month
                    <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={target.TargetMonth.toString()}
                    onValueChange={(value) =>
                      handleChange("TargetMonth", parseInt(value))
                    }
                  >
                    <SelectTrigger className="h-11 border-slate-200 focus:border-orange-500 focus:ring-orange-500/20">
                      <SelectValue placeholder="Select month" />
                    </SelectTrigger>
                    <SelectContent>
                      {Array.from({ length: 12 }, (_, i) => i + 1).map(
                        (month) => (
                          <SelectItem key={month} value={month.toString()}>
                            {new Date(2000, month - 1).toLocaleString(
                              "default",
                              {
                                month: "long"
                              }
                            )}
                          </SelectItem>
                        )
                      )}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-3">
                  <Label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                    <Calendar className="h-4 w-4" />
                    Target Year
                    <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={target.TargetYear.toString()}
                    onValueChange={(value) =>
                      handleChange("TargetYear", parseInt(value))
                    }
                  >
                    <SelectTrigger className="h-11 border-slate-200 focus:border-orange-500 focus:ring-orange-500/20">
                      <SelectValue placeholder="Select year" />
                    </SelectTrigger>
                    <SelectContent>
                      {[currentYear - 1, currentYear, currentYear + 1].map(
                        (year) => (
                          <SelectItem key={year} value={year.toString()}>
                            {year}
                          </SelectItem>
                        )
                      )}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-3">
                  <Label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                    Target Amount
                    <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    value={target.TargetAmount}
                    onChange={(e) =>
                      handleChange("TargetAmount", parseFloat(e.target.value))
                    }
                    placeholder="0.00"
                    className="h-11 border-slate-200 focus:border-orange-500 focus:ring-orange-500/20"
                    required
                  />
                </div>

              </div>

              <div className="space-y-3">
                <Label className="text-sm font-medium text-slate-700">
                  Status
                </Label>
                <Select
                  value={target.Status}
                  onValueChange={(value) => handleChange("Status", value)}
                >
                  <SelectTrigger className="h-11 border-slate-200 focus:border-orange-500 focus:ring-orange-500/20">
                    <SelectValue placeholder="Select status" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Not Started">
                      <div className="flex items-center gap-2">
                        <div className="h-2 w-2 rounded-full bg-gray-400" />
                        Not Started
                      </div>
                    </SelectItem>
                    <SelectItem value="In Progress">
                      <div className="flex items-center gap-2">
                        <div className="h-2 w-2 rounded-full bg-blue-500" />
                        In Progress
                      </div>
                    </SelectItem>
                    <SelectItem value="Achieved">
                      <div className="flex items-center gap-2">
                        <div className="h-2 w-2 rounded-full bg-green-500" />
                        Achieved
                      </div>
                    </SelectItem>
                    <SelectItem value="Failed">
                      <div className="flex items-center gap-2">
                        <div className="h-2 w-2 rounded-full bg-red-500" />
                        Failed
                      </div>
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>

          {/* Notes */}
          <div className="space-y-6">
            <div className="mb-6">
              <h2 className="text-xl font-medium text-gray-900">
                Additional Notes
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                Add any relevant information or comments (optional)
              </p>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg p-6">
              <Textarea
                value={target.Notes}
                onChange={(e) => handleChange("Notes", e.target.value)}
                placeholder="Enter any additional notes, instructions, or comments about this target..."
                rows={4}
                className="border-slate-200 focus:border-indigo-500 focus:ring-indigo-500/20 resize-none"
              />
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end gap-4 pt-4">
            <Button
              type="button"
              variant="outline"
              size="lg"
              onClick={() =>
                router.push("/distributiondelivery/route-management/target")
              }
              className="px-8"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={loading}
              size="lg"
              className="px-8 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800"
            >
              {loading ? (
                <div className="flex items-center gap-2">
                  <div className="h-4 w-4">
                    <Skeleton className="h-4 w-4 rounded-full" />
                  </div>
                  <span>Updating Target...</span>
                </div>
              ) : (
                <>
                  <Save className="mr-2 h-4 w-4" />
                  Update Target
                </>
              )}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}