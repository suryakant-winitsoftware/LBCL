"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";
import { DataTable } from "@/components/ui/data-table";
import { PaginationControls } from "@/components/ui/pagination-controls";
import { useToast } from "@/components/ui/use-toast";
import { Switch } from "@/components/ui/switch";
import { Checkbox } from "@/components/ui/checkbox";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import { Separator } from "@/components/ui/separator";
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
import { Skeleton } from "@/components/ui/skeleton";
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel
} from "@/utils/organizationHierarchyUtils";
import {
  ArrowLeft,
  ArrowRight,
  Save,
  CalendarIcon,
  CheckCircle2,
  Building2,
  Filter,
  Search,
  Upload,
  Users,
  Package,
  Package2,
  Eye,
  EyeOff,
  Edit2,
  MoreHorizontal,
  ChevronDown,
  ChevronRight,
  ChevronUp,
  X,
  Check,
  FileText,
  Target,
  Clock,
  Info,
  AlertCircle,
  Sparkles,
  BarChart3,
  ShoppingCart,
  Zap,
  Award,
  TrendingUp,
  Calendar,
  MapPin,
  Plus,
  Trash2
} from "lucide-react";
import {
  organizationService,
  Organization,
  OrgType
} from "@/services/organizationService";
import { storeService } from "@/services/storeService";
import { skuService } from "@/services/sku/sku.service";
import { hierarchyService } from "@/services/hierarchy.service";
import { uomService } from "@/services/uom.service";
import {
  initiativeService,
  type AllocationMaster,
  type CreateInitiativeRequest
} from "@/services/initiativeService";
import { initiativeFileService } from "@/services/initiative-file.service";
import { format } from "date-fns";
import { cn } from "@/lib/utils";

// Types
interface AllocationData {
  allocationNo: string;
  activityNo: string;
  brand: string;
  availableAmount: number;
  allocationName: string;
  allocationDescription: string;
  startDate: string;
  endDate: string;
  daysLeft: number;
}

interface InitiativeFormData {
  selectedAllocation: AllocationData | null;
  salesOrgId: number;
  contractAmount: number;
  initiativeName: string;
  initiativeDescription: string;
  isActive: boolean;
  customerType: "HOCustomer" | "Customer" | "";
  activityType: string;
  displayType: string;
  displayLocation: string;
  startDate: string;
  endDate: string;
  posmImage: File | null;
  defaultImage: File | null;
  emailAttachment: File | null;
  posmImagePreview: string | null;
  defaultImagePreview: string | null;
  emailAttachmentPreview: string | null;
  selectedProducts: any[];
  selectedCustomers: any[];
}

const CUSTOMER_TYPES = [
  { value: "HOCustomer", label: "HO Customer" },
  { value: "Customer", label: "Customer" }
];

export default function CreateInitiativePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();
  const mode = searchParams.get("mode") || "Add"; // Add or Edit

  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [currentStep, setCurrentStep] = useState(1);
  const [showSelectedAllocation, setShowSelectedAllocation] = useState(true);

  // Selected Allocation Component
  const SelectedAllocationDisplay = () => {
    if (!formData.selectedAllocation) return null;

    return (
      <div className="mb-6 border rounded-lg bg-blue-50/50">
        <button
          onClick={() => setShowSelectedAllocation(!showSelectedAllocation)}
          className="w-full px-4 py-3 flex items-center justify-between hover:bg-blue-50 transition-colors"
          type="button"
        >
          <div className="flex items-center gap-2">
            <Package className="h-4 w-4 text-blue-600" />
            <span className="font-medium text-sm">Selected Allocation</span>
            <Badge variant="secondary" className="text-xs">
              {formData.selectedAllocation.allocationNo}
            </Badge>
          </div>
          {showSelectedAllocation ? (
            <ChevronUp className="h-4 w-4 text-gray-500" />
          ) : (
            <ChevronDown className="h-4 w-4 text-gray-500" />
          )}
        </button>

        {showSelectedAllocation && (
          <div className="px-4 pb-3 border-t">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mt-3">
              <div>
                <p className="text-xs text-gray-500">Allocation Name</p>
                <p className="text-sm font-medium">
                  {formData.selectedAllocation.allocationName}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Description</p>
                <p className="text-sm font-medium">
                  {formData.selectedAllocation.allocationDescription}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Available Amount</p>
                <p className="text-sm font-medium">
                  {formData.selectedAllocation.availableAmount.toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Contract Amount</p>
                <p className="text-sm font-medium">
                  {formData.contractAmount.toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Brand</p>
                <Badge variant="outline" className="text-xs mt-1">
                  {formData.selectedAllocation.brand}
                </Badge>
              </div>
              <div>
                <p className="text-xs text-gray-500">Period</p>
                <p className="text-sm">
                  {formData.selectedAllocation.startDate} -{" "}
                  {formData.selectedAllocation.endDate}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Days Left</p>
                <p
                  className={cn(
                    "text-sm font-medium",
                    formData.selectedAllocation.daysLeft === 0 &&
                      "text-red-600",
                    formData.selectedAllocation.daysLeft > 0 &&
                      formData.selectedAllocation.daysLeft <= 7 &&
                      "text-red-600",
                    formData.selectedAllocation.daysLeft > 7 &&
                      formData.selectedAllocation.daysLeft <= 30 &&
                      "text-orange-600",
                    formData.selectedAllocation.daysLeft > 30 &&
                      "text-green-600"
                  )}
                >
                  {formData.selectedAllocation.daysLeft === 0
                    ? "Expired"
                    : `${formData.selectedAllocation.daysLeft} days`}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Activity No</p>
                <p className="text-sm font-medium">
                  {formData.selectedAllocation.activityNo}
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    );
  };

  // Form data
  const [formData, setFormData] = useState<InitiativeFormData>({
    selectedAllocation: null,
    salesOrgId: 0,
    contractAmount: 0,
    initiativeName: "",
    initiativeDescription: "",
    isActive: true,
    customerType: "",
    activityType: "",
    displayType: "",
    displayLocation: "",
    startDate: format(new Date(), "yyyy-MM-dd"),
    endDate: format(
      new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),
      "yyyy-MM-dd"
    ),
    posmImage: null,
    defaultImage: null,
    emailAttachment: null,
    posmImagePreview: null,
    defaultImagePreview: null,
    emailAttachmentPreview: null,
    selectedProducts: [],
    selectedCustomers: []
  });

  // Dropdown data
  const [salesOrgs, setSalesOrgs] = useState<any[]>([]);
  const [allocations, setAllocations] = useState<AllocationData[]>([]);
  const [filteredAllocations, setFilteredAllocations] = useState<
    AllocationData[]
  >([]);

  // Organization hierarchy state
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]);
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([]); // Array of selected org UIDs from top to bottom
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [selectedOrgCode, setSelectedOrgCode] = useState<string>(""); // Store the selected org code for API calls
  const [activityTypes, setActivityTypes] = useState<any[]>([]);
  const [displayTypes, setDisplayTypes] = useState<any[]>([]);
  const [displayLocations, setDisplayLocations] = useState<any[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [customerSearch, setCustomerSearch] = useState("");
  const [loadingCustomers, setLoadingCustomers] = useState(false);
  const [customersPagination, setCustomersPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: true,
    isLoadingMore: false
  });

  // Filter state for allocations
  const [searchTerm, setSearchTerm] = useState("");
  const [filterBrand, setFilterBrand] = useState<string[]>([]);

  // Popups
  const [showCustomerPopup, setShowCustomerPopup] = useState(false);
  const [customerPopoverOpen, setCustomerPopoverOpen] = useState(false);
  const [productPopoverOpen, setProductPopoverOpen] = useState(false);
  const [showSelectedCustomers, setShowSelectedCustomers] = useState(false);
  const [customerDisplaySettings, setCustomerDisplaySettings] = useState<Record<string, { displayType: string; displayLocation: string }>>({});
  const [isAutoLoading, setIsAutoLoading] = useState(false);
  const [productPttPrices, setProductPttPrices] = useState<Record<string, number>>({});

  // Product state
  const [products, setProducts] = useState<any[]>([]);
  const [productSearch, setProductSearch] = useState("");
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [productsPagination, setProductsPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: true
  });

  // Current page for allocations table
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Helper function to calculate days left
  const calculateDaysLeft = (endDate: string): number => {
    if (!endDate) return 0;
    const end = new Date(endDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0); // Reset time to start of day for accurate comparison
    end.setHours(0, 0, 0, 0);
    const diffTime = end.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 ? diffDays : 0;
  };

  // Allocation columns configuration
  const allocationColumns = [
    {
      accessorKey: "select",
      header: () => <div className="pl-6">Select</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <Checkbox
            checked={
              formData.selectedAllocation?.allocationNo ===
              row.original.allocationNo
            }
            onCheckedChange={() => selectAllocation(row.original)}
            className="data-[state=checked]:bg-blue-600 data-[state=checked]:border-blue-600 [&[data-state=checked]>span]:!text-white [&[data-state=checked]_svg]:!text-white"
          />
        </div>
      )
    },
    {
      accessorKey: "allocationNo",
      header: "Allocation No",
      cell: ({ row }: any) => (
        <span className="font-medium text-sm">
          {row.getValue("allocationNo")}
        </span>
      )
    },
    {
      accessorKey: "activityNo",
      header: "Activity No",
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue("activityNo")}</span>
      )
    },
    {
      accessorKey: "brand",
      header: "Brand",
      cell: ({ row }: any) => (
        <Badge
          variant="secondary"
          className="bg-blue-100 text-blue-800 hover:bg-blue-100 text-xs"
        >
          {row.getValue("brand")}
        </Badge>
      )
    },
    {
      accessorKey: "availableAmount",
      header: "Available Amount",
      cell: ({ row }: any) => (
        <span className="text-sm">
          {row.getValue("availableAmount").toLocaleString()}
        </span>
      )
    },
    {
      accessorKey: "allocationName",
      header: "Allocation Name",
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue("allocationName")}</span>
      )
    },
    {
      accessorKey: "allocationDescription",
      header: "Description",
      cell: ({ row }: any) => (
        <span className="text-sm text-gray-600">
          {row.original.allocationDescription || "-"}
        </span>
      )
    },
    {
      accessorKey: "startDate",
      header: "Start Date",
      cell: ({ row }: any) => (
        <span className="text-sm">
          {format(new Date(row.getValue("startDate")), "MMM dd, yyyy")}
        </span>
      )
    },
    {
      accessorKey: "endDate",
      header: "End Date",
      cell: ({ row }: any) => (
        <span className="text-sm">
          {format(new Date(row.getValue("endDate")), "MMM dd, yyyy")}
        </span>
      )
    },
    {
      accessorKey: "daysLeft",
      header: "Days Left",
      cell: ({ row }: any) => {
        const daysLeft = row.getValue("daysLeft");
        const isExpired = daysLeft === 0;
        const isCritical = daysLeft > 0 && daysLeft <= 7;
        const isWarning = daysLeft > 7 && daysLeft <= 30;
        const isGood = daysLeft > 30;

        return (
          <div className="flex items-center gap-2">
            {isExpired && <AlertCircle className="h-4 w-4 text-red-600" />}
            {isCritical && <Clock className="h-4 w-4 text-red-600" />}
            {isWarning && <Clock className="h-4 w-4 text-orange-500" />}
            {isGood && <Clock className="h-4 w-4 text-green-600" />}
            <span
              className={cn(
                "text-sm font-medium",
                isExpired && "text-red-600",
                isCritical && "text-red-600",
                isWarning && "text-orange-600",
                isGood && "text-green-600"
              )}
            >
              {isExpired ? "Expired" : `${daysLeft} days`}
            </span>
          </div>
        );
      }
    }
  ];

  // Load customers with pagination
  const loadCustomers = useCallback(
    async (page: number = 1, append: boolean = false, search: string = "") => {
      if (!append) {
        setLoadingCustomers(true);
      } else {
        setCustomersPagination((prev) => ({ ...prev, isLoadingMore: true }));
      }

      try {
        const filterCriterias = [];

        // Add search filter if provided
        if (search && search.trim()) {
          filterCriterias.push({
            name: "Code",
            value: search.trim(),
            operator: "contains"
          });
        }

        const response = await storeService.getAllStores({
          pageNumber: page,
          pageSize: customersPagination.pageSize,
          isCountRequired: page === 1,
          sortCriterias: [{ sortParameter: "Code", direction: 0 }],
          filterCriterias
        });

        if (response.pagedData && response.pagedData.length > 0) {
          const newCustomers = response.pagedData.map((store: any) => ({
            uid: store.UID || store.uid || store.Code || store.code,
            name: store.Name || store.name || store.Code || store.code,
            code:
              store.Code ||
              store.code ||
              store.Customer_Code ||
              store.customerCode ||
              "",
            type: store.StoreType === "HO" ? "HOCustomer" : "Customer"
          }));

          if (append) {
            setCustomers((prev) => [...prev, ...newCustomers]);
          } else {
            setCustomers(newCustomers);
          }

          setCustomersPagination((prev) => ({
            ...prev,
            currentPage: page,
            totalCount: response.totalRecords || 0,
            hasMore: response.pagedData.length === customersPagination.pageSize
          }));
        } else {
          if (!append) {
            setCustomers([]);
          }
          setCustomersPagination((prev) => ({ ...prev, hasMore: false }));
        }
      } catch (error) {
        console.error("Error loading customers:", error);
        if (!append) {
          setCustomers([]);
        }
      } finally {
        setLoadingCustomers(false);
        setCustomersPagination((prev) => ({ ...prev, isLoadingMore: false }));
      }
    },
    [customersPagination.pageSize]
  );

  // Load more customers for infinite scroll
  const loadMoreCustomers = useCallback(async () => {
    if (!customersPagination.hasMore || customersPagination.isLoadingMore)
      return;

    const nextPage = customersPagination.currentPage + 1;
    await loadCustomers(nextPage, true, customerSearch);
  }, [customersPagination, loadCustomers, customerSearch]);

  // Handle customer scroll for infinite loading
  const handleCustomerScroll = useCallback(
    (e: React.UIEvent<HTMLDivElement>) => {
      const element = e.currentTarget;
      const threshold = 0.8;

      const scrollPosition = element.scrollTop + element.clientHeight;
      const scrollHeight = element.scrollHeight;
      const scrollPercentage = scrollPosition / scrollHeight;

      if (
        scrollPercentage > threshold &&
        customersPagination.hasMore &&
        !customersPagination.isLoadingMore
      ) {
        loadMoreCustomers();
      }
    },
    [
      customersPagination.hasMore,
      customersPagination.isLoadingMore,
      loadMoreCustomers
    ]
  );

  // Load products with search and brand filtering
  // Helper function to find brand UID by brand name/code
  const findBrandUID = async (brandName: string): Promise<string | null> => {
    try {
      console.log("Finding brand UID for brand:", brandName);

      // Get hierarchy types to find the Brand type
      const hierarchyTypes = await hierarchyService.getHierarchyTypes();
      const brandType = hierarchyTypes.find(
        (type) => type.Name === "Brand" || type.Code === "Brand"
      );

      if (!brandType) {
        console.log("Brand type not found in hierarchy");
        return null;
      }

      console.log("Found brand type:", brandType);

      // Get all brands from the hierarchy
      const brandOptions = await hierarchyService.getHierarchyOptionsForType(
        brandType.UID
      );
      console.log("All brand options:", brandOptions);

      // Find the brand by name or code
      const matchingBrand = brandOptions.find(
        (option) =>
          option.value === brandName ||
          option.code === brandName ||
          option.value.toLowerCase() === brandName.toLowerCase() ||
          option.code?.toLowerCase() === brandName.toLowerCase()
      );

      if (matchingBrand) {
        console.log("Found matching brand:", matchingBrand);
        return matchingBrand.uid || matchingBrand.code;
      }

      console.log("No matching brand found for:", brandName);
      return null;
    } catch (error) {
      console.error("Error finding brand UID:", error);
      return null;
    }
  };

  const loadProducts = useCallback(
    async (page: number = 1, search: string = "") => {
      setLoadingProducts(true);
      try {
        const filterCriterias = [];

        // Add search filter
        if (search && search.trim()) {
          filterCriterias.push({
            Name: "skucodeandname",
            Value: search.trim()
          });
        }

        // Filter by brand from selected allocation using proper hierarchy approach
        if (formData.selectedAllocation?.brand) {
          console.log(
            "Filtering products by brand:",
            formData.selectedAllocation.brand
          );

          // Find the brand UID using the same approach as ProductAttributesWithHierarchyFilter
          const brandUID = await findBrandUID(
            formData.selectedAllocation.brand
          );

          if (brandUID) {
            console.log(
              "Found brand UID:",
              brandUID,
              "for brand:",
              formData.selectedAllocation.brand
            );
            // Filter by ParentUID as shown in the ProductAttributesWithHierarchyFilter
            filterCriterias.push({
              Name: "ParentUID",
              Value: brandUID
            });
          } else {
            console.log(
              "Could not find brand UID for:",
              formData.selectedAllocation.brand
            );
            console.log(
              "Proceeding without brand filter - will show all products"
            );
          }
        }

        // Add active filter
        filterCriterias.push({
          Name: "IsActive",
          Value: true
        });

        const request = {
          PageNumber: page,
          PageSize: productsPagination.pageSize,
          FilterCriterias: filterCriterias,
          SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
          IsCountRequired: page === 1
        };

        console.log(
          "🔍 Product API Request:",
          JSON.stringify(request, null, 2)
        );
        console.log("🔍 Filter Criterias:", filterCriterias);

        const response = await skuService.getAllSKUs(request);
        console.log("🔍 Raw API Response:", response);
        console.log("🔍 Response type:", typeof response);
        console.log(
          "🔍 Response keys:",
          response ? Object.keys(response) : "No response"
        );

        // Handle the response structure from SKU service
        let pagedData = null;
        let totalCount = 0;

        if (response && response.success && response.data) {
          // Response format: { success: true, data: { PagedData: [...], TotalCount: ... } }
          pagedData = response.data.PagedData || response.data.pagedData;
          totalCount =
            response.data.TotalCount || response.data.totalCount || 0;

          console.log("🔍 PagedData extracted:", pagedData);
          console.log("🔍 TotalCount:", totalCount);
        } else if (response && response.pagedData) {
          // Fallback for direct format
          pagedData = response.pagedData;
          totalCount = response.totalRecords || 0;
        }

        if (pagedData && pagedData.length > 0) {
          console.log("🔍 Processing products:", pagedData.length);

          const newProducts = pagedData.map((sku: any) => ({
            id: sku.SKUUID || sku.UID,
            code: sku.SKUCode || sku.Code,
            name: sku.SKULongName || sku.LongName || sku.Name,
            price: sku.Price || 0,
            uom: sku.UOM || "",
            category: sku.Category || "",
            isActive: sku.IsActive
          }));

          if (page === 1) {
            setProducts(newProducts);
            console.log("🔍 Set products (page 1):", newProducts.length);
          } else {
            setProducts((prev) => [...prev, ...newProducts]);
            console.log("🔍 Added products to existing:", newProducts.length);
          }

          setProductsPagination((prev) => ({
            ...prev,
            currentPage: page,
            totalCount: totalCount,
            hasMore: pagedData.length === productsPagination.pageSize
          }));
        } else {
          console.log("🔍 No products found in response");
          if (page === 1) {
            setProducts([]);
          }
          setProductsPagination((prev) => ({ ...prev, hasMore: false }));
        }
      } catch (error) {
        console.error("Error loading products:", error);
        if (page === 1) {
          setProducts([]);
        }
      } finally {
        setLoadingProducts(false);
      }
    },
    [productsPagination.pageSize, formData.selectedAllocation?.brand]
  );

  // Debounce product search
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      loadProducts(1, productSearch);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [productSearch, loadProducts]);

  // Reload products when allocation (brand) changes
  useEffect(() => {
    console.log("Allocation changed:", formData.selectedAllocation?.brand);
    // Reset products and reload with new brand filter (or no filter if no allocation)
    setProducts([]);
    setProductsPagination((prev) => ({
      ...prev,
      currentPage: 1,
      totalCount: 0,
      hasMore: true
    }));
    loadProducts(1, productSearch);
  }, [formData.selectedAllocation?.brand, loadProducts]);

  // Debounce customer search
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      loadCustomers(1, false, customerSearch);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [customerSearch, loadCustomers]);

  // Load initial data
  useEffect(() => {
    loadInitialData();
  }, []);

  // Filter allocations when search term or filters change
  useEffect(() => {
    filterAllocations();
    setCurrentPage(1);
  }, [allocations, searchTerm, filterBrand]);

  // Auto-show selected customers panel when customers are selected
  useEffect(() => {
    if (formData.selectedCustomers.length > 0 && !showSelectedCustomers) {
      setShowSelectedCustomers(true);
    }
  }, [formData.selectedCustomers.length]);

  const loadInitialData = async () => {
    setLoadingData(true);
    try {
      // Load organization hierarchy data - use the same methods as route page
      const [orgTypesResult, orgsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 1000) // Get all orgs
      ]);

      console.log("Organization Types fetched:", orgTypesResult);
      console.log("Organizations fetched:", orgsResult);

      // Process the data
      let orgsData: Organization[] = orgsResult.data || [];
      let orgTypesData: OrgType[] = orgTypesResult || [];

      // Filter organizations to only show those with ShowInTemplate = true
      // This hides internal/system organizations from user selection
      const filteredOrganizations = orgsData.filter(
        (org) => org.ShowInTemplate === true && org.IsActive !== false
      );

      // Filter organization types to only show those with ShowInTemplate = true
      const filteredOrgTypes = orgTypesData.filter(
        (type) => type.ShowInTemplate !== false
      );

      console.log("Organizations before filter:", orgsData.length);
      console.log(
        "Organizations after ShowInTemplate filter:",
        filteredOrganizations.length
      );
      console.log("Filtered organizations:", filteredOrganizations);

      setOrganizations(filteredOrganizations);
      setOrgTypes(filteredOrgTypes);

      // Initialize organization hierarchy
      if (filteredOrganizations.length > 0 && filteredOrgTypes.length > 0) {
        const initialLevels = initializeOrganizationHierarchy(
          filteredOrganizations,
          filteredOrgTypes
        );
        setOrgLevels(initialLevels);

        // Don't auto-select on initial load - let user select manually
        // This avoids the slice error on uninitialized state
      }

      // Fetch all allocations initially (will be filtered when org is selected)
      try {
        // Load allocations for all organizations initially
        const allAllocationsResponse =
          await initiativeService.getAvailableAllocations("");

        // Transform API data to match component interface
        const transformedAllocations: AllocationData[] =
          allAllocationsResponse.map((allocation: AllocationMaster) => ({
            allocationNo: allocation.allocationNo,
            activityNo: allocation.activityNo,
            brand: allocation.brand,
            availableAmount: allocation.availableAllocationAmount,
            allocationName: allocation.allocationName,
            allocationDescription: allocation.allocationDescription,
            startDate: allocation.startDate,
            endDate: allocation.endDate,
            daysLeft: calculateDaysLeft(allocation.endDate) // Auto-calculate days left
          }));

        setAllocations(transformedAllocations);
      } catch (error) {
        console.error("Error loading initial allocations:", error);
        setAllocations([]);
      }

      setActivityTypes([
        { id: 1, name: "Display" },
        { id: 2, name: "Sampling" },
        { id: 3, name: "Demo" }
      ]);

      setDisplayTypes([
        { id: 1, name: "Floor Display" },
        { id: 2, name: "Window Display" },
        { id: 3, name: "Counter Display" }
      ]);

      setDisplayLocations([
        { id: 1, name: "Entrance" },
        { id: 2, name: "Aisle End" },
        { id: 3, name: "Check Out" }
      ]);

      // Load initial customers and products
      loadCustomers(1, false, "");

      // Load initial products (no brand filter initially)
      loadProducts(1, "");
    } catch (error) {
      console.error("Error loading initial data:", error);
      toast({
        title: "Error",
        description: "Failed to load data. Please refresh the page.",
        variant: "destructive"
      });
    } finally {
      setLoadingData(false);
    }
  };

  // Organization selection handlers
  const handleOrgSelection = (orgUID: string, levelIndex: number) => {
    // Ensure selectedOrgs is always an array
    const currentSelectedOrgs = Array.isArray(selectedOrgs) ? selectedOrgs : [];

    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      orgUID,
      orgLevels,
      currentSelectedOrgs,
      organizations,
      orgTypes
    );

    setOrgLevels(updatedLevels);
    setSelectedOrgs(updatedSelectedOrgs);

    // Get the final selected organization
    const finalSelectedOrg = getFinalSelectedOrganization(updatedSelectedOrgs);
    if (finalSelectedOrg) {
      const selectedOrg = organizations.find(
        (org) => org.UID === finalSelectedOrg
      );
      if (selectedOrg) {
        setSelectedOrgCode(selectedOrg.Code || "");
        // Load allocations for the selected organization
        loadAllocationsForOrganization(selectedOrg.Code || "");
      }
    }
  };

  const handleOrganizationReset = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(
      organizations,
      orgTypes
    );

    setOrgLevels(resetLevels);
    setSelectedOrgs(resetSelectedOrgs);
    setSelectedOrgCode("");
    setAllocations([]);
  };

  const loadAllocationsForOrganization = async (orgCode: string) => {
    if (!orgCode) return;

    try {
      const allocationsResponse =
        await initiativeService.getAvailableAllocations(orgCode);

      // Transform API data to match component interface
      const transformedAllocations: AllocationData[] = allocationsResponse.map(
        (allocation: AllocationMaster) => ({
          allocationNo: allocation.allocationNo,
          activityNo: allocation.activityNo,
          brand: allocation.brand,
          availableAmount: allocation.availableAllocationAmount,
          allocationName: allocation.allocationName,
          allocationDescription: allocation.allocationDescription,
          startDate: allocation.startDate,
          endDate: allocation.endDate,
          daysLeft: calculateDaysLeft(allocation.endDate) // Auto-calculate days left
        })
      );

      setAllocations(transformedAllocations);
    } catch (error) {
      console.error("Error loading allocations:", error);
      setAllocations([]);
      toast({
        title: "Error",
        description: "Failed to load allocations for the selected organization",
        variant: "destructive"
      });
    }
  };

  const filterAllocations = () => {
    let filtered = [...allocations];

    if (searchTerm) {
      filtered = filtered.filter(
        (a) =>
          a.allocationName.toLowerCase().includes(searchTerm.toLowerCase()) ||
          a.allocationNo.toLowerCase().includes(searchTerm.toLowerCase()) ||
          a.brand.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    if (filterBrand.length > 0) {
      filtered = filtered.filter((a) => filterBrand.includes(a.brand));
    }

    setFilteredAllocations(filtered);
  };

  const selectAllocation = (allocation: AllocationData) => {
    setFormData((prev) => ({
      ...prev,
      selectedAllocation: allocation,
      contractAmount: allocation.availableAmount
    }));
  };

  const handleInputChange = (field: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleFileUpload = (
    field: "posmImage" | "defaultImage" | "emailAttachment",
    file: File
  ) => {
    // Validate file based on type
    const fileTypeMap = {
      posmImage: 'POSM' as const,
      defaultImage: 'DefaultImage' as const,
      emailAttachment: 'EmailAttachment' as const
    };

    const validation = initiativeFileService.validateFile(file, fileTypeMap[field]);

    if (!validation.valid) {
      toast({
        title: "Invalid File",
        description: validation.error,
        variant: "destructive"
      });
      return;
    }

    // Generate preview for images
    const previewField = `${field}Preview` as keyof InitiativeFormData;

    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = (e) => {
        setFormData((prev) => ({
          ...prev,
          [field]: file,
          [previewField]: e.target?.result as string
        }));
      };
      reader.readAsDataURL(file);
    } else {
      // For non-image files, just store the file info
      setFormData((prev) => ({
        ...prev,
        [field]: file,
        [previewField]: null
      }));
    }

    toast({
      title: "File Selected",
      description: `${file.name} (${initiativeFileService.formatFileSize(file.size)}) ready for upload`
    });
  };

  const handleRemoveFile = (field: "posmImage" | "defaultImage" | "emailAttachment") => {
    const previewField = `${field}Preview` as keyof InitiativeFormData;
    setFormData((prev) => ({
      ...prev,
      [field]: null,
      [previewField]: null
    }));

    toast({
      title: "File Removed",
      description: "The file has been removed"
    });
  };

  const handleCustomerUpload = async (file: File) => {
    try {
      toast({
        title: "Processing File",
        description: "Reading and validating customer data..."
      });

      // Read file based on type
      const fileExtension = file.name.split(".").pop()?.toLowerCase();

      if (fileExtension === "csv") {
        const text = await file.text();
        const lines = text.split("\n").filter((line) => line.trim());

        // Assuming CSV format: Code,Name
        const uploadedCustomers: string[] = [];
        for (let i = 1; i < lines.length; i++) {
          // Skip header
          const [code, name] = lines[i].split(",").map((s) => s.trim());
          if (code) {
            // Find customer by code
            const customer = customers.find((c) => c.code === code);
            if (
              customer &&
              !formData.selectedCustomers.includes(customer.uid)
            ) {
              uploadedCustomers.push(customer.uid);
            }
          }
        }

        if (uploadedCustomers.length > 0) {
          handleInputChange("selectedCustomers", [
            ...formData.selectedCustomers,
            ...uploadedCustomers
          ]);
          toast({
            title: "Success",
            description: `Added ${uploadedCustomers.length} customers from file`
          });
        } else {
          toast({
            title: "No Customers Added",
            description: "No matching customers found in the file",
            variant: "destructive"
          });
        }
      } else {
        // For Excel files, you would need a library like xlsx
        toast({
          title: "Excel Support",
          description: "Excel file processing would require additional library",
          variant: "destructive"
        });
      }
    } catch (error) {
      console.error("Error uploading customers:", error);
      toast({
        title: "Upload Failed",
        description: "Failed to process the uploaded file",
        variant: "destructive"
      });
    }
  };

  const updateCustomerDisplaySettings = (
    customerId: string,
    field: "displayType" | "displayLocation",
    value: string
  ) => {
    setCustomerDisplaySettings(prev => ({
      ...prev,
      [customerId]: {
        ...prev[customerId],
        [field]: value
      }
    }));
  };

  const proceedToNext = () => {
    if (currentStep < 4) {
      setCurrentStep(currentStep + 1);
    }
  };

  const goToPrevious = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const validateCurrentStep = (): boolean => {
    switch (currentStep) {
      case 1:
        return (
          formData.selectedAllocation !== null && formData.contractAmount > 0
        );
      case 2:
        return formData.initiativeName.trim() !== "";
      case 3:
        return (
          formData.customerType !== "" &&
          formData.selectedCustomers.length > 0 &&
          formData.activityType !== ""
        );
      case 4:
        return formData.selectedProducts.length > 0;
      default:
        return true;
    }
  };

  const handleSave = async () => {
    if (!validateCurrentStep()) {
      toast({
        title: "Validation Error",
        description: "Please fill all required fields",
        variant: "destructive"
      });
      return;
    }

    setLoading(true);
    try {
      // Use the selected organization code
      const salesOrgCode = selectedOrgCode || "";

      if (!salesOrgCode) {
        toast({
          title: "Error",
          description: "Please select an organization first",
          variant: "destructive"
        });
        setLoading(false);
        return;
      }

      // Prepare request data
      const request: CreateInitiativeRequest = {
        allocationNo: formData.selectedAllocation?.allocationNo || "",
        name: formData.initiativeName,
        description: formData.initiativeDescription,
        salesOrgCode: salesOrgCode,
        brand: formData.selectedAllocation?.brand || "",
        contractAmount: formData.contractAmount,
        activityType: formData.activityType,
        displayType: formData.displayType,
        displayLocation: formData.displayLocation,
        customerType: formData.customerType,
        startDate: formData.startDate,
        endDate: formData.endDate,
        // Backend expects CustomerCodes (array of strings)
        // selectedCustomers already contains UIDs/codes as strings
        customerCodes: formData.selectedCustomers.map((customerId) => {
          // Find the actual customer object to get the code
          const customer = customers.find(c => c.uid === customerId);
          const code = customer?.code || customer?.uid || customerId;
          console.log('Customer mapping:', { customerId, customer, extractedCode: code });
          return code;
        }),
        products: formData.selectedProducts.map((p) => ({
          itemCode: p.ItemCode || p.code,
          itemName: p.ItemName || p.name
        }))
      };

      // Debug the full request being sent
      console.log('Full Initiative Request:', JSON.stringify(request, null, 2));
      console.log('CustomerCodes being sent:', request.customerCodes);

      // Create initiative via API
      const response = await initiativeService.createInitiative(request);

      // Debug the actual response structure
      console.log('Create Initiative Response:', response);

      // Handle different property name formats from API
      const initiativeId = (
        response.initiativeId ||
        response.InitiativeId ||
        response.initiative_id ||
        response.id
      )?.toString();

      const initiativeUID = (
        response.uid ||
        response.UID ||
        response.Uid ||
        initiativeId // fallback to ID if UID not available
      )?.toString();

      if (!initiativeId) {
        throw new Error(`Initiative ID not found in response. Available properties: ${Object.keys(response).join(', ')}`);
      }

      // Upload files if any were selected
      const uploadPromises = [];

      if (formData.posmImage) {
        uploadPromises.push(
          initiativeFileService.uploadInitiativeFile(
            initiativeUID || initiativeId,
            formData.posmImage,
            'POSM'
          ).catch(err => {
            console.error('Failed to upload POSM image:', err);
            return { error: 'POSM upload failed' };
          })
        );
      }

      if (formData.defaultImage) {
        uploadPromises.push(
          initiativeFileService.uploadInitiativeFile(
            initiativeUID || initiativeId,
            formData.defaultImage,
            'DefaultImage'
          ).catch(err => {
            console.error('Failed to upload default image:', err);
            return { error: 'Default image upload failed' };
          })
        );
      }

      if (formData.emailAttachment) {
        uploadPromises.push(
          initiativeFileService.uploadInitiativeFile(
            initiativeUID || initiativeId,
            formData.emailAttachment,
            'EmailAttachment'
          ).then(result => {
            console.log('Email attachment upload completed:', result);
            return result;
          }).catch(err => {
            console.error('Failed to upload email attachment:', err);
            return { error: `Email attachment upload failed: ${err.message}` };
          })
        );
      }

      // Wait for all uploads to complete
      if (uploadPromises.length > 0) {
        console.log('Starting file uploads...');
        const uploadResults = await Promise.all(uploadPromises);
        const failedUploads = uploadResults.filter(r => r.error);

        if (failedUploads.length > 0) {
          console.error('Some file uploads failed:', failedUploads);
          toast({
            title: "Warning",
            description: `Initiative created but some files failed to upload: ${failedUploads.map(f => f.error).join(', ')}`,
            variant: "default"
          });
        } else {
          console.log('All files uploaded successfully:', uploadResults);

          // Log success for debugging
          uploadResults.forEach((result, index) => {
            if (result.success) {
              console.log(`File ${index + 1} uploaded:`, {
                fileUID: result.fileUID,
                path: result.relativePath,
                message: result.message
              });
            }
          });
        }
      }

      // Show success toast
      toast({
        title: "Success",
        description: `Initiative ${response.contractCode || response.ContractCode || response.name || response.Name || ''} created successfully`
      });

      // Redirect to initiatives list after a short delay
      setTimeout(() => {
        router.push('/administration/configurations/initiatives');
      }, 1000);
    } catch (error: any) {
      console.error("Error saving initiative:", error);
      toast({
        title: "Error",
        description:
          error.response?.data?.message ||
          error.message ||
          "Failed to create initiative",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const handleAddNew = () => {
    // Reset form
    setFormData({
      selectedAllocation: null,
      salesOrgId: 0,
      contractAmount: 0,
      initiativeName: "",
      initiativeDescription: "",
      isActive: true,
      customerType: "",
      activityType: "",
      displayType: "",
      displayLocation: "",
      startDate: format(new Date(), "yyyy-MM-dd"),
      endDate: format(
        new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),
        "yyyy-MM-dd"
      ),
      posmImage: null,
      defaultImage: null,
      emailAttachment: null,
      posmImagePreview: null,
      defaultImagePreview: null,
      emailAttachmentPreview: null,
      selectedProducts: [],
      selectedCustomers: []
    });
    setCurrentStep(1);
  };


  const renderStepIndicator = () => {
    const steps = [
      {
        num: 1,
        label: "Allocation",
        mobileLabel: "Allocation"
      },
      {
        num: 2,
        label: "Details",
        mobileLabel: "Details"
      },
      {
        num: 3,
        label: "Target Configuration",
        mobileLabel: "Target"
      },
      {
        num: 4,
        label: "Products",
        mobileLabel: "Products"
      },
      {
        num: 5,
        label: "Review",
        mobileLabel: "Review"
      }
    ];

    return (
      <div className="px-6 py-4 border-b">
        <div className="flex items-center justify-between max-w-6xl">
          <div className="flex items-center flex-1">
            {steps.map((step, index) => (
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
                {index < steps.length - 1 && (
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
    );
  };

  // Show loading state
  if (loadingData) {
    return (
      <div className="container mx-auto py-4 space-y-4">
        <Card>
          <CardContent className="py-8">
            <div className="space-y-4">
              <Skeleton className="h-12 w-64" />
              <Skeleton className="h-4 w-48" />
              <div className="grid grid-cols-4 gap-4 mt-6">
                <Skeleton className="h-10 w-full rounded-full" />
                <Skeleton className="h-10 w-full rounded-full" />
                <Skeleton className="h-10 w-full rounded-full" />
                <Skeleton className="h-10 w-full rounded-full" />
              </div>
              <Skeleton className="h-64 w-full mt-6" />
              <div className="flex justify-end gap-2 mt-6">
                <Skeleton className="h-10 w-24" />
                <Skeleton className="h-10 w-32" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }


  return (
    <div className="min-h-screen bg-gray-50">
      <div className="w-full">
        {/* Header */}
        <div className="bg-white border-b">
          <div className="px-6 py-4">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-semibold text-gray-900">
                  {mode} Initiative
                </h1>
                <p className="text-sm text-gray-500 mt-1">
                  Create and configure a new marketing initiative
                </p>
              </div>
              <Button
                variant="outline"
                onClick={() =>
                  router.push("/administration/configurations/initiatives")
                }
                className="h-10"
              >
                <ArrowLeft className="mr-2 h-4 w-4" />
                Back
              </Button>
            </div>
          </div>
        </div>

        {/* Main Content - No Card wrapper */}
        <div className="bg-white">
          {/* Step Indicator */}
          {renderStepIndicator()}

          <div className="px-2 pb-8">
            <div className="w-full">
              <div className="bg-white p-2">
                {/* Step Content */}
                <div className="min-h-[400px]">
                  {currentStep === 1 && (
                    <div className="space-y-6 animate-fadeIn">
                      {/* Allocation Selection Header */}
                      <div className="flex items-center justify-between">
                        <div>
                          <h3 className="text-xl font-bold text-gray-900">
                            Select Allocation
                          </h3>
                          <p className="text-sm text-gray-500 mt-1">
                            {selectedOrgCode
                              ? `Showing allocations for ${selectedOrgCode}`
                              : "Showing all available allocations"}
                          </p>
                        </div>
                        <Badge className="bg-blue-100 text-blue-700 border-0">
                          <Info className="mr-1 h-3 w-3" />
                          {filteredAllocations.length} Available
                        </Badge>
                      </div>

                      {/* Enhanced Search and Filters */}
                      <Card className="border border-gray-100 shadow-sm hover:shadow-md transition-shadow">
                        <CardContent className="py-4">
                          <div className="flex gap-3">
                            <div className="relative flex-1">
                              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-5 w-5" />
                              <Input
                                placeholder="Search allocations..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="pl-10 h-11 border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 rounded-xl"
                              />
                            </div>
                            <Button variant="outline" className="rounded-xl">
                              <Filter className="mr-2 h-4 w-4" />
                              Filters
                            </Button>
                          </div>
                        </CardContent>
                      </Card>

                      {/* Organization Selection - Compact */}
                      <div className="border rounded-lg p-4 bg-gray-50/30">
                        <div className="flex items-center justify-between mb-3">
                          <div className="flex items-center gap-2">
                            <Building2 className="h-4 w-4 text-gray-600" />
                            <span className="text-sm font-medium text-gray-700">
                              Organization Selection
                              <span className="text-xs text-gray-500 ml-2">
                                (Optional - filter allocations)
                              </span>
                            </span>
                          </div>
                          {selectedOrgs.length > 0 && (
                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              onClick={handleOrganizationReset}
                              className="text-xs h-7 px-2"
                            >
                              <X className="h-3 w-3 mr-1" />
                              Reset
                            </Button>
                          )}
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                          {orgLevels.length > 0 ? (
                            <>
                              {orgLevels.map((level, index) => (
                                <div key={level.orgTypeUID}>
                                  <Label className="text-xs font-medium text-gray-600 mb-1">
                                    {level.dynamicLabel || level.orgTypeName}
                                    {index === 0 && (
                                      <span className="text-red-500 ml-1">
                                        *
                                      </span>
                                    )}
                                  </Label>
                                  <Select
                                    value={level.selectedOrgUID || ""}
                                    onValueChange={(value) =>
                                      handleOrgSelection(value, index)
                                    }
                                    disabled={
                                      index > 0 && !selectedOrgs[index - 1]
                                    }
                                  >
                                    <SelectTrigger className="h-9 text-sm">
                                      <SelectValue
                                        placeholder={`Select ${level.orgTypeName}`}
                                      />
                                    </SelectTrigger>
                                    <SelectContent>
                                      {level.organizations.map((org) => (
                                        <SelectItem
                                          key={org.UID}
                                          value={org.UID}
                                        >
                                          <div className="flex items-center justify-between w-full">
                                            <span className="text-sm">
                                              {org.Name}
                                            </span>
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
                                </div>
                              ))}
                              {selectedOrgCode && (
                                <div className="md:col-span-2 mt-2">
                                  <div className="text-xs text-green-600 flex items-center gap-1">
                                    <Check className="h-3 w-3" />
                                    Selected: {selectedOrgCode}
                                  </div>
                                </div>
                              )}
                            </>
                          ) : (
                            <div className="md:col-span-2 py-4 text-center text-muted-foreground">
                              <Skeleton className="h-9 w-full" />
                            </div>
                          )}
                        </div>
                      </div>

                      {/* Enhanced Allocations Table */}
                      {filteredAllocations.length === 0 ? (
                        <Card className="border-gray-200">
                          <CardContent className="py-8">
                            <div className="flex flex-col items-center text-center">
                              <Package className="h-10 w-10 text-gray-400 mb-3" />
                              <p className="font-medium text-gray-900 text-lg">
                                No Allocations Available
                              </p>
                              <p className="text-sm text-gray-600 mt-2 max-w-md">
                                No allocations found for the selected
                                organization. Please try a different
                                organization or contact your administrator.
                              </p>
                            </div>
                          </CardContent>
                        </Card>
                      ) : (
                        <Card className="border border-gray-100 shadow-sm overflow-hidden">
                          <div className="bg-gradient-to-r from-gray-50 to-gray-100/50 px-6 py-4 border-b">
                            <div className="flex items-center justify-between">
                              <h4 className="font-semibold text-gray-700">
                                Available Allocations
                              </h4>
                              <div className="flex gap-2">
                                <Badge
                                  variant="secondary"
                                  className="bg-green-100 text-green-700 border-0"
                                >
                                  <TrendingUp className="mr-1 h-3 w-3" />
                                  Active
                                </Badge>
                              </div>
                            </div>
                          </div>
                          <div className="overflow-hidden">
                            <DataTable
                              columns={allocationColumns}
                              data={filteredAllocations.slice(
                                (currentPage - 1) * pageSize,
                                currentPage * pageSize
                              )}
                              loading={false}
                              searchable={false}
                              pagination={false}
                              noWrapper={true}
                            />
                          </div>

                          {filteredAllocations.length > 0 && (
                            <div className="px-6 py-4 border-t bg-gray-50/30">
                              <PaginationControls
                                currentPage={currentPage}
                                totalCount={filteredAllocations.length}
                                pageSize={pageSize}
                                onPageChange={setCurrentPage}
                                onPageSizeChange={setPageSize}
                                itemName="allocations"
                              />
                            </div>
                          )}
                        </Card>
                      )}

                      {/* Selected Allocation Details - Compact */}
                      {formData.selectedAllocation && (
                        <div className="border border-blue-200 bg-blue-50/30 rounded-lg p-4">
                          <div className="flex items-center justify-between mb-3">
                            <h4 className="text-sm font-semibold text-gray-900">
                              Selected Allocation
                            </h4>
                            <Badge variant="secondary" className="text-xs">
                              <CheckCircle2 className="mr-1 h-3 w-3" />
                              Active
                            </Badge>
                          </div>

                          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-3">
                            <div>
                              <p className="text-xs text-gray-500">
                                Allocation No
                              </p>
                              <p className="text-sm font-medium">
                                {formData.selectedAllocation.allocationNo}
                              </p>
                            </div>
                            <div>
                              <p className="text-xs text-gray-500">
                                Activity No
                              </p>
                              <p className="text-sm font-medium">
                                {formData.selectedAllocation.activityNo}
                              </p>
                            </div>
                            <div>
                              <p className="text-xs text-gray-500">Brand</p>
                              <Badge variant="outline" className="text-xs mt-1">
                                {formData.selectedAllocation.brand}
                              </Badge>
                            </div>
                            <div>
                              <p className="text-xs text-gray-500">Available</p>
                              <p className="text-sm font-semibold text-green-600">
                                {formData.selectedAllocation.availableAmount.toLocaleString()}
                              </p>
                            </div>
                          </div>

                          {formData.selectedAllocation
                            .allocationDescription && (
                            <div className="mb-3 p-2 bg-gray-50 rounded">
                              <p className="text-xs text-gray-500 mb-1">
                                Description
                              </p>
                              <p className="text-xs text-gray-700">
                                {
                                  formData.selectedAllocation
                                    .allocationDescription
                                }
                              </p>
                            </div>
                          )}

                          <Separator className="my-3" />

                          <div className="space-y-2">
                            <div className="flex items-center justify-between">
                              <Label className="text-sm">
                                Contract Amount{" "}
                                <span className="text-red-500">*</span>
                              </Label>
                              <span className="text-xs text-gray-500">
                                Max:{" "}
                                {formData.selectedAllocation.availableAmount.toLocaleString()}
                              </span>
                            </div>
                            <div className="flex gap-2">
                              <div className="relative flex-1">
                                <Input
                                  type="number"
                                  placeholder="Enter amount"
                                  value={formData.contractAmount || ""}
                                  onChange={(e) =>
                                    handleInputChange(
                                      "contractAmount",
                                      parseFloat(e.target.value) || 0
                                    )
                                  }
                                  max={
                                    formData.selectedAllocation.availableAmount
                                  }
                                  className="h-9"
                                />
                              </div>
                            </div>
                            {formData.contractAmount > 0 && (
                              <div className="flex items-center justify-between text-xs">
                                <span className="text-gray-500">
                                  Using:{" "}
                                  {(
                                    (formData.contractAmount /
                                      formData.selectedAllocation
                                        .availableAmount) *
                                    100
                                  ).toFixed(0)}
                                  %
                                </span>
                                <span className="text-gray-600">
                                  Remaining:{" "}
                                  <span className="font-medium">
                                    {(
                                      formData.selectedAllocation
                                        .availableAmount -
                                      formData.contractAmount
                                    ).toLocaleString()}
                                  </span>
                                </span>
                              </div>
                            )}
                            <Progress
                              value={
                                (formData.contractAmount /
                                  formData.selectedAllocation.availableAmount) *
                                100
                              }
                              className="h-1.5"
                            />
                          </div>
                        </div>
                      )}
                    </div>
                  )}

                  {currentStep === 2 && (
                    <div className="space-y-6 animate-fadeIn">
                      {/* Selected Allocation Display */}
                      <SelectedAllocationDisplay />
                      <div className="flex items-center justify-between mb-6">
                        <div>
                          <h3 className="text-xl font-bold text-gray-900">
                            Initiative Configuration
                          </h3>
                          <p className="text-sm text-gray-500 mt-1">
                            Set up the details and parameters for your
                            initiative
                          </p>
                        </div>
                      </div>

                      {/* Basic Details Card */}
                      <Card className="border border-gray-100 shadow-sm hover:shadow-md transition-shadow">
                        <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100/50 border-b">
                          <div className="flex items-center gap-2">
                            <FileText className="h-5 w-5 text-blue-500" />
                            <CardTitle className="text-lg">
                              Basic Information
                            </CardTitle>
                          </div>
                        </CardHeader>
                        <CardContent className="space-y-5 pt-6">
                          <div className="space-y-2">
                            <Label className="text-sm font-semibold flex items-center gap-1">
                              Initiative Name{" "}
                              <span className="text-red-500">*</span>
                            </Label>
                            <Input
                              value={formData.initiativeName}
                              onChange={(e) =>
                                handleInputChange(
                                  "initiativeName",
                                  e.target.value
                                )
                              }
                              placeholder="e.g., Summer Sales Campaign 2024"
                              className="h-11 rounded-xl border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                            />
                          </div>

                          <div className="space-y-2">
                            <Label className="text-sm font-semibold">
                              Description
                            </Label>
                            <Textarea
                              value={formData.initiativeDescription}
                              onChange={(e) =>
                                handleInputChange(
                                  "initiativeDescription",
                                  e.target.value
                                )
                              }
                              placeholder="Describe the initiative objectives and goals..."
                              rows={4}
                              className="rounded-xl border-gray-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 resize-none"
                            />
                            <div className="text-xs text-gray-500">
                              {formData.initiativeDescription.length}/500
                              characters
                            </div>
                          </div>

                          <div className="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                            <div className="flex items-center space-x-3">
                              <div
                                className={`p-2 rounded-lg ${
                                  formData.isActive
                                    ? "bg-green-100"
                                    : "bg-gray-200"
                                }`}
                              >
                                <Zap
                                  className={`h-4 w-4 ${
                                    formData.isActive
                                      ? "text-green-600"
                                      : "text-gray-400"
                                  }`}
                                />
                              </div>
                              <div>
                                <Label className="text-sm font-semibold">
                                  Status
                                </Label>
                                <p className="text-xs text-gray-500">
                                  Initiative is{" "}
                                  {formData.isActive ? "active" : "inactive"}
                                </p>
                              </div>
                            </div>
                            <Switch
                              checked={formData.isActive}
                              onCheckedChange={(checked) =>
                                handleInputChange("isActive", checked)
                              }
                              className="data-[state=checked]:bg-green-500"
                            />
                          </div>

                          {/* File Upload Section with Previews */}
                          <div className="space-y-3 pt-2">
                            <Separator />
                            <div>
                              <h4 className="text-sm font-semibold text-gray-900 mb-3">
                                Attachments
                              </h4>
                              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                {/* POSM Upload */}
                                <div className="space-y-2">
                                  <Label className="text-xs text-gray-600">
                                    POSM Image
                                  </Label>
                                  {formData.posmImage ? (
                                    <div className="relative group">
                                      {formData.posmImagePreview ? (
                                        <div className="w-full h-40 bg-gray-50 rounded-lg border border-gray-200 p-2">
                                          <img
                                            src={formData.posmImagePreview}
                                            alt="POSM preview"
                                            className="w-full h-full object-contain rounded"
                                          />
                                        </div>
                                      ) : (
                                        <div className="w-full h-40 bg-gray-100 rounded-lg border border-gray-200 flex items-center justify-center">
                                          <div className="text-center p-2">
                                            <FileText className="h-8 w-8 text-gray-400 mx-auto mb-1" />
                                            <p className="text-xs text-gray-600 truncate max-w-[120px]">
                                              {formData.posmImage.name}
                                            </p>
                                            <p className="text-xs text-gray-500">
                                              {initiativeFileService.formatFileSize(formData.posmImage.size)}
                                            </p>
                                          </div>
                                        </div>
                                      )}
                                      <button
                                        type="button"
                                        onClick={() => handleRemoveFile("posmImage")}
                                        className="absolute top-2 right-2 p-1.5 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity shadow-lg hover:bg-red-600"
                                      >
                                        <X className="h-3 w-3" />
                                      </button>
                                    </div>
                                  ) : (
                                    <>
                                      <Input
                                        type="file"
                                        id="posm-upload"
                                        accept="image/*,.pdf"
                                        onChange={(e) => {
                                          const file = e.target.files?.[0];
                                          if (file) handleFileUpload("posmImage", file);
                                        }}
                                        className="hidden"
                                      />
                                      <Label
                                        htmlFor="posm-upload"
                                        className="w-full h-40 border-2 border-dashed border-gray-300 rounded-lg flex flex-col items-center justify-center cursor-pointer hover:border-blue-400 hover:bg-blue-50/50 transition-colors"
                                      >
                                        <Upload className="h-6 w-6 text-gray-400 mb-2" />
                                        <span className="text-xs text-gray-600">Upload POSM</span>
                                        <span className="text-xs text-gray-400 mt-1">Image or PDF</span>
                                      </Label>
                                    </>
                                  )}
                                </div>

                                {/* Default Image Upload */}
                                <div className="space-y-2">
                                  <Label className="text-xs text-gray-600">
                                    Default Image
                                  </Label>
                                  {formData.defaultImage ? (
                                    <div className="relative group">
                                      {formData.defaultImagePreview ? (
                                        <div className="w-full h-40 bg-gray-50 rounded-lg border border-gray-200 p-2">
                                          <img
                                            src={formData.defaultImagePreview}
                                            alt="Default preview"
                                            className="w-full h-full object-contain rounded"
                                          />
                                        </div>
                                      ) : (
                                        <div className="w-full h-40 bg-gray-100 rounded-lg border border-gray-200 flex items-center justify-center">
                                          <div className="text-center p-2">
                                            <FileText className="h-8 w-8 text-gray-400 mx-auto mb-1" />
                                            <p className="text-xs text-gray-600 truncate max-w-[120px]">
                                              {formData.defaultImage.name}
                                            </p>
                                            <p className="text-xs text-gray-500">
                                              {initiativeFileService.formatFileSize(formData.defaultImage.size)}
                                            </p>
                                          </div>
                                        </div>
                                      )}
                                      <button
                                        type="button"
                                        onClick={() => handleRemoveFile("defaultImage")}
                                        className="absolute top-2 right-2 p-1.5 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity shadow-lg hover:bg-red-600"
                                      >
                                        <X className="h-3 w-3" />
                                      </button>
                                    </div>
                                  ) : (
                                    <>
                                      <Input
                                        type="file"
                                        id="default-upload"
                                        accept="image/*"
                                        onChange={(e) => {
                                          const file = e.target.files?.[0];
                                          if (file) handleFileUpload("defaultImage", file);
                                        }}
                                        className="hidden"
                                      />
                                      <Label
                                        htmlFor="default-upload"
                                        className="w-full h-40 border-2 border-dashed border-gray-300 rounded-lg flex flex-col items-center justify-center cursor-pointer hover:border-blue-400 hover:bg-blue-50/50 transition-colors"
                                      >
                                        <Upload className="h-6 w-6 text-gray-400 mb-2" />
                                        <span className="text-xs text-gray-600">Upload Image</span>
                                        <span className="text-xs text-gray-400 mt-1">JPG, PNG, GIF</span>
                                      </Label>
                                    </>
                                  )}
                                </div>

                                {/* Email Agreement Upload */}
                                <div className="space-y-2">
                                  <Label className="text-xs text-gray-600">
                                    Email Agreement
                                  </Label>
                                  {formData.emailAttachment ? (
                                    <div className="relative group">
                                      <div className="w-full h-40 bg-gray-100 rounded-lg border border-gray-200 flex items-center justify-center">
                                        <div className="text-center p-2">
                                          <FileText className="h-10 w-10 text-blue-400 mx-auto mb-2" />
                                          <p className="text-xs text-gray-600 truncate max-w-[140px] font-medium">
                                            {formData.emailAttachment.name}
                                          </p>
                                          <p className="text-xs text-gray-500 mt-1">
                                            {initiativeFileService.formatFileSize(formData.emailAttachment.size)}
                                          </p>
                                        </div>
                                      </div>
                                      <button
                                        type="button"
                                        onClick={() => handleRemoveFile("emailAttachment")}
                                        className="absolute top-2 right-2 p-1.5 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity shadow-lg hover:bg-red-600"
                                      >
                                        <X className="h-3 w-3" />
                                      </button>
                                    </div>
                                  ) : (
                                    <>
                                      <Input
                                        type="file"
                                        id="email-upload"
                                        accept=".pdf,.doc,.docx,.zip"
                                        onChange={(e) => {
                                          const file = e.target.files?.[0];
                                          if (file) handleFileUpload("emailAttachment", file);
                                        }}
                                        className="hidden"
                                      />
                                      <Label
                                        htmlFor="email-upload"
                                        className="w-full h-40 border-2 border-dashed border-gray-300 rounded-lg flex flex-col items-center justify-center cursor-pointer hover:border-blue-400 hover:bg-blue-50/50 transition-colors"
                                      >
                                        <Upload className="h-6 w-6 text-gray-400 mb-2" />
                                        <span className="text-xs text-gray-600">Upload Agreement</span>
                                        <span className="text-xs text-gray-400 mt-1">PDF, DOC, ZIP</span>
                                      </Label>
                                    </>
                                  )}
                                </div>
                              </div>
                              <p className="text-xs text-gray-500 mt-3">
                                Max file size: 50MB per file
                              </p>
                            </div>
                          </div>
                        </CardContent>
                      </Card>
                    </div>
                  )}

                  {currentStep === 3 && (
                    <div className="space-y-6 animate-fadeIn">
                      {/* Selected Allocation Display */}
                      <SelectedAllocationDisplay />

                      {/* Target Configuration Header */}
                      <div className="flex items-center justify-between">
                        <div>
                          <h3 className="text-xl font-bold text-gray-900">
                            Target Configuration
                          </h3>
                          <p className="text-sm text-gray-500 mt-1">
                            Configure customer targeting and display settings
                          </p>
                        </div>
                      </div>

                      {/* Customer Selection and Selected Customers - Same Row */}
                      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                        <Card className="border border-gray-100 shadow-sm hover:shadow-md transition-shadow lg:col-span-2">
                          <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100/50 border-b">
                            <div className="flex items-center gap-2">
                              <Target className="h-5 w-5 text-purple-500" />
                              <CardTitle className="text-lg">
                                Customer & Display Settings
                              </CardTitle>
                            </div>
                          </CardHeader>
                          <CardContent className="space-y-5 pt-6">
                            <div className="space-y-3">
                              <Label className="text-sm font-semibold flex items-center gap-1">
                                Customer Type{" "}
                                <span className="text-red-500">*</span>
                              </Label>

                              {/* Customer Type Selection - Horizontal */}
                              <div className="grid grid-cols-2 gap-3">
                                {CUSTOMER_TYPES.map((type) => (
                                  <div
                                    key={type.value}
                                    className={cn(
                                      "flex items-center space-x-3 p-3 border rounded-lg cursor-pointer transition-all",
                                      formData.customerType === type.value
                                        ? "border-blue-500 bg-blue-50"
                                        : "border-gray-200 hover:bg-gray-50"
                                    )}
                                    onClick={() =>
                                      handleInputChange(
                                        "customerType",
                                        type.value
                                      )
                                    }
                                  >
                                    <div
                                      className={cn(
                                        "w-4 h-4 rounded-full border-2 flex items-center justify-center",
                                        formData.customerType === type.value
                                          ? "border-blue-500"
                                          : "border-gray-300"
                                      )}
                                    >
                                      {formData.customerType === type.value && (
                                        <div className="w-2 h-2 rounded-full bg-blue-500" />
                                      )}
                                    </div>
                                    <Label className="flex items-center gap-2 cursor-pointer flex-1">
                                      <Users className="h-4 w-4 text-gray-400" />
                                      {type.label}
                                    </Label>
                                  </div>
                                ))}
                              </div>

                              {/* Customer Selection Dropdown - Shows when type is selected */}
                              {formData.customerType && (
                                <div className="space-y-2">
                                  <div className="flex items-center justify-between">
                                    <Label className="text-sm">
                                      Select Customer
                                    </Label>
                                    <Button
                                      type="button"
                                      variant="outline"
                                      size="sm"
                                      className="h-7 text-xs"
                                      onClick={() =>
                                        document
                                          .getElementById("customer-upload")
                                          ?.click()
                                      }
                                    >
                                      <Upload className="h-3 w-3 mr-1" />
                                      Upload CSV
                                    </Button>
                                    <input
                                      type="file"
                                      id="customer-upload"
                                      accept=".csv,.xlsx"
                                      className="hidden"
                                      onChange={(e) => {
                                        const file = e.target.files?.[0];
                                        if (file) {
                                          // Handle CSV upload
                                          handleCustomerUpload(file);
                                        }
                                      }}
                                    />
                                  </div>
                                  <Popover
                                    open={customerPopoverOpen}
                                    onOpenChange={setCustomerPopoverOpen}
                                  >
                                    <PopoverTrigger asChild>
                                      <Button
                                        variant="outline"
                                        role="combobox"
                                        aria-expanded={customerPopoverOpen}
                                        className="w-full justify-between h-10 font-normal"
                                      >
                                        <span className="truncate">
                                          {formData.selectedCustomers.length > 0
                                            ? `${
                                                formData.selectedCustomers
                                                  .length
                                              } customer${
                                                formData.selectedCustomers
                                                  .length > 1
                                                  ? "s"
                                                  : ""
                                              } selected`
                                            : "Search and select customers"}
                                        </span>
                                        <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                                      </Button>
                                    </PopoverTrigger>
                                    <PopoverContent
                                      className="w-[var(--radix-popover-trigger-width)] p-0"
                                      align="start"
                                    >
                                      <Command>
                                        <CommandInput
                                          placeholder="Search customers..."
                                          value={customerSearch}
                                          onValueChange={setCustomerSearch}
                                        />
                                        {loadingCustomers &&
                                        !customersPagination.isLoadingMore ? (
                                          <div className="py-2 space-y-2">
                                            <Skeleton className="h-8 w-full" />
                                            <Skeleton className="h-8 w-full" />
                                            <Skeleton className="h-8 w-full" />
                                          </div>
                                        ) : (
                                          <>
                                            <CommandEmpty>
                                              No customer found.
                                            </CommandEmpty>
                                            <CommandList
                                              onScroll={handleCustomerScroll}
                                              className="max-h-[300px] overflow-y-auto"
                                            >
                                              <CommandGroup>
                                                {customers
                                                  .filter(
                                                    (c) =>
                                                      c.type ===
                                                      formData.customerType
                                                  )
                                                  .map((customer) => (
                                                    <CommandItem
                                                      key={customer.uid}
                                                      value={customer.uid}
                                                      onSelect={(value) => {
                                                        const isSelected =
                                                          formData.selectedCustomers.includes(
                                                            value
                                                          );
                                                        if (isSelected) {
                                                          handleInputChange(
                                                            "selectedCustomers",
                                                            formData.selectedCustomers.filter(
                                                              (id) =>
                                                                id !== value
                                                            )
                                                          );
                                                        } else {
                                                          handleInputChange(
                                                            "selectedCustomers",
                                                            [
                                                              ...formData.selectedCustomers,
                                                              value
                                                            ]
                                                          );
                                                        }
                                                      }}
                                                    >
                                                      <Checkbox
                                                        checked={formData.selectedCustomers.includes(
                                                          customer.uid
                                                        )}
                                                        className="mr-2 data-[state=checked]:bg-blue-600 data-[state=checked]:border-blue-600 [&[data-state=checked]>span]:!text-white [&[data-state=checked]_svg]:!text-white"
                                                      />
                                                      <div className="flex flex-col">
                                                        <span className="font-medium">
                                                          {customer.name}
                                                        </span>
                                                        <span className="text-xs text-gray-500">
                                                          Code: {customer.code}
                                                        </span>
                                                      </div>
                                                    </CommandItem>
                                                  ))}
                                                {customersPagination.isLoadingMore && (
                                                  <div className="py-2 text-center text-xs text-gray-500">
                                                    Loading more...
                                                  </div>
                                                )}
                                              </CommandGroup>
                                            </CommandList>
                                          </>
                                        )}
                                      </Command>
                                    </PopoverContent>
                                  </Popover>
                                </div>
                              )}
                            </div>

                            {/* Activity Type and Display Type in same row */}
                            <div className="grid grid-cols-2 gap-3">
                              <div className="space-y-2">
                                <Label className="text-sm font-semibold flex items-center gap-1">
                                  Activity Type{" "}
                                  <span className="text-red-500">*</span>
                                </Label>
                                <Select
                                  value={formData.activityType}
                                  onValueChange={(value) =>
                                    handleInputChange("activityType", value)
                                  }
                                >
                                  <SelectTrigger className="h-10 rounded-lg">
                                    <SelectValue placeholder="Choose activity" />
                                  </SelectTrigger>
                                  <SelectContent>
                                    {activityTypes.map((type) => (
                                      <SelectItem
                                        key={type.id}
                                        value={type.name}
                                      >
                                        <div className="flex items-center gap-2">
                                          <BarChart3 className="h-4 w-4 text-gray-400" />
                                          {type.name}
                                        </div>
                                      </SelectItem>
                                    ))}
                                  </SelectContent>
                                </Select>
                              </div>

                              <div className="space-y-2">
                                <Label className="text-sm font-semibold">
                                  Display Type
                                </Label>
                                <Select
                                  value={formData.displayType}
                                  onValueChange={(value) =>
                                    handleInputChange("displayType", value)
                                  }
                                >
                                  <SelectTrigger className="h-10 rounded-lg">
                                    <SelectValue placeholder="Select display" />
                                  </SelectTrigger>
                                  <SelectContent>
                                    {displayTypes.map((type) => (
                                      <SelectItem
                                        key={type.id}
                                        value={type.name}
                                      >
                                        <div className="flex items-center gap-2">
                                          <Eye className="h-4 w-4 text-gray-400" />
                                          {type.name}
                                        </div>
                                      </SelectItem>
                                    ))}
                                  </SelectContent>
                                </Select>
                              </div>
                            </div>

                            {/* Display Location Dropdown */}
                            <div className="space-y-2">
                              <Label className="text-sm font-semibold flex items-center gap-1">
                                <MapPin className="h-4 w-4 text-gray-400" />
                                Display Location
                              </Label>
                              <Select
                                value={formData.displayLocation}
                                onValueChange={(value) =>
                                  handleInputChange("displayLocation", value)
                                }
                              >
                                <SelectTrigger className="h-10 rounded-lg">
                                  <SelectValue placeholder="Select location" />
                                </SelectTrigger>
                                <SelectContent>
                                  {displayLocations.map((location) => (
                                    <SelectItem
                                      key={location.id}
                                      value={location.name}
                                    >
                                      <div className="flex items-center gap-2">
                                        <MapPin className="h-4 w-4 text-gray-400" />
                                        {location.name}
                                      </div>
                                    </SelectItem>
                                  ))}
                                </SelectContent>
                              </Select>
                            </div>

                            <div className="space-y-3">
                              <Label className="text-sm font-semibold flex items-center gap-1">
                                <Calendar className="h-4 w-4 text-gray-400" />
                                Duration
                              </Label>
                              <div className="grid grid-cols-2 gap-3">
                                <div className="space-y-2">
                                  <Label className="text-xs text-gray-500">
                                    Start Date
                                  </Label>
                                  <div className="relative">
                                    <CalendarIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                                    <Input
                                      type="date"
                                      value={formData.startDate}
                                      onChange={(e) =>
                                        handleInputChange(
                                          "startDate",
                                          e.target.value
                                        )
                                      }
                                      className="pl-10 h-10 rounded-xl"
                                    />
                                  </div>
                                </div>

                                <div className="space-y-2">
                                  <Label className="text-xs text-gray-500">
                                    End Date
                                  </Label>
                                  <div className="relative">
                                    <CalendarIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                                    <Input
                                      type="date"
                                      value={formData.endDate}
                                      onChange={(e) =>
                                        handleInputChange(
                                          "endDate",
                                          e.target.value
                                        )
                                      }
                                      className="pl-10 h-10 rounded-xl"
                                    />
                                  </div>
                                </div>
                              </div>
                              {formData.startDate && formData.endDate && (
                                <div className="flex items-center gap-2 p-3 bg-blue-50 rounded-xl">
                                  <Clock className="h-4 w-4 text-blue-500" />
                                  <span className="text-sm text-blue-700">
                                    Duration:{" "}
                                    {Math.ceil(
                                      (new Date(formData.endDate).getTime() -
                                        new Date(
                                          formData.startDate
                                        ).getTime()) /
                                        (1000 * 60 * 60 * 24)
                                    )}{" "}
                                    days
                                  </span>
                                </div>
                              )}
                            </div>
                          </CardContent>
                        </Card>

                        {/* Selected Customers Section - Right Side */}
                        <Card className="border border-gray-100 shadow-sm lg:col-span-1 min-h-96">
                          <CardHeader className="bg-gradient-to-r from-green-50 to-green-100/50 border-b">
                            <div className="flex items-center justify-between">
                              <CardTitle className="text-lg flex items-center gap-2">
                                <Users className="h-5 w-5 text-green-500" />
                                Selected Customers
                              </CardTitle>
                              {formData.selectedCustomers.length > 0 && (
                                <Badge className="bg-green-100 text-green-700 border-0">
                                  {formData.selectedCustomers.length} selected
                                </Badge>
                              )}
                            </div>
                          </CardHeader>
                          <CardContent className="pt-4">
                            {formData.selectedCustomers.length === 0 ? (
                              <p className="text-sm text-gray-500">
                                Select customers from the left panel to configure display settings.
                              </p>
                            ) : (
                              <div className="space-y-3 max-h-96 min-h-96 overflow-y-auto">
                                {formData.selectedCustomers.map((customerId, index) => {
                                  const customer = customers.find(c => c.uid === customerId)
                                  return (
                                    <div key={index} className="p-3 border rounded-lg bg-white hover:shadow-sm transition-shadow">
                                      <div className="space-y-3">
                                        {/* Customer Info */}
                                        <div className="flex items-center gap-3">
                                          <div className="p-2 bg-green-50 rounded-lg">
                                            <Building2 className="h-4 w-4 text-green-600" />
                                          </div>
                                          <div className="flex-1">
                                            <div className="font-medium text-sm">{customer?.name || `Customer ${index + 1}`}</div>
                                            <div className="text-xs text-gray-500">{customer?.code || customerId}</div>
                                          </div>
                                          <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={() => {
                                              const updatedCustomers = formData.selectedCustomers.filter(id => id !== customerId)
                                              handleInputChange('selectedCustomers', updatedCustomers)
                                            }}
                                            className="h-6 w-6 p-0 text-red-500 hover:text-red-700 hover:bg-red-50"
                                          >
                                            <X className="h-3 w-3" />
                                          </Button>
                                        </div>

                                        {/* Display Settings for this customer */}
                                        <div className="grid grid-cols-2 gap-3">
                                          <div className="space-y-1">
                                            <Label className="text-xs text-gray-600">Display Type</Label>
                                            <Select
                                              value={customerDisplaySettings[customerId]?.displayType || ''}
                                              onValueChange={(value) => updateCustomerDisplaySettings(customerId, 'displayType', value)}
                                            >
                                              <SelectTrigger className="h-8 text-xs">
                                                <SelectValue placeholder="Select type" />
                                              </SelectTrigger>
                                              <SelectContent>
                                                {displayTypes.map((type) => (
                                                  <SelectItem key={type.id} value={type.name}>
                                                    <div className="flex items-center gap-2">
                                                      <Eye className="h-3 w-3 text-gray-400" />
                                                      <span className="text-xs">{type.name}</span>
                                                    </div>
                                                  </SelectItem>
                                                ))}
                                              </SelectContent>
                                            </Select>
                                          </div>
                                          
                                          <div className="space-y-1">
                                            <Label className="text-xs text-gray-600">Display Location</Label>
                                            <Select
                                              value={customerDisplaySettings[customerId]?.displayLocation || ''}
                                              onValueChange={(value) => updateCustomerDisplaySettings(customerId, 'displayLocation', value)}
                                            >
                                              <SelectTrigger className="h-8 text-xs">
                                                <SelectValue placeholder="Select location" />
                                              </SelectTrigger>
                                              <SelectContent>
                                                {displayLocations.map((location) => (
                                                  <SelectItem key={location.id} value={location.name}>
                                                    <div className="flex items-center gap-2">
                                                      <MapPin className="h-3 w-3 text-gray-400" />
                                                      <span className="text-xs">{location.name}</span>
                                                    </div>
                                                  </SelectItem>
                                                ))}
                                              </SelectContent>
                                            </Select>
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                  )
                                })}
                              </div>
                            )}
                          </CardContent>
                        </Card>
                      </div>
                    </div>
                  )}

                  {currentStep === 4 && (
                    <div className="space-y-6 animate-fadeIn">
                      {/* Selected Allocation Display */}
                      <SelectedAllocationDisplay />
                      {/* Product Configuration Header with Dropdown */}
                      <div className="flex items-center justify-between gap-4">
                        <div>
                          <h3 className="text-xl font-bold text-gray-900">
                            Product Configuration
                          </h3>
                          <p className="text-sm text-gray-500 mt-1">
                            {formData.selectedAllocation?.brand
                              ? `Choose products from ${formData.selectedAllocation.brand} brand`
                              : "Choose products to include in this initiative"}
                          </p>
                        </div>

                        {/* Product Search Dropdown and Actions */}
                        <div className="flex gap-2">
                          <Popover
                            open={productPopoverOpen}
                            onOpenChange={setProductPopoverOpen}
                          >
                            <PopoverTrigger asChild>
                              <Button
                                variant="outline"
                                role="combobox"
                                aria-expanded={productPopoverOpen}
                                className="w-[300px] justify-between h-10 font-normal"
                              >
                                <span className="truncate">
                                  {formData.selectedProducts.length > 0
                                    ? `${
                                        formData.selectedProducts.length
                                      } product${
                                        formData.selectedProducts.length > 1
                                          ? "s"
                                          : ""
                                      } selected`
                                    : "Search and select products"}
                                </span>
                                <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                              </Button>
                            </PopoverTrigger>
                            <PopoverContent
                              className="w-[300px] p-0"
                              align="end"
                            >
                              <Command>
                                <CommandInput
                                  placeholder="Search products..."
                                  value={productSearch}
                                  onValueChange={setProductSearch}
                                />
                                {loadingProducts && products.length === 0 ? (
                                  <div className="py-2 space-y-2 px-2">
                                    <Skeleton className="h-8 w-full" />
                                    <Skeleton className="h-8 w-full" />
                                    <Skeleton className="h-8 w-full" />
                                  </div>
                                ) : (
                                  <>
                                    <CommandEmpty>
                                      No products found.
                                    </CommandEmpty>
                                    <CommandGroup>
                                      <div 
                                        className="max-h-64 overflow-y-auto"
                                        onScroll={(e) => {
                                          const { scrollTop, scrollHeight, clientHeight } = e.currentTarget
                                          const scrollPercentage = (scrollTop + clientHeight) / scrollHeight
                                          
                                          // Trigger loading when user scrolls to 80%
                                          if (
                                            scrollPercentage >= 0.8 && 
                                            productsPagination.hasMore && 
                                            !loadingProducts && 
                                            !isAutoLoading
                                          ) {
                                            setIsAutoLoading(true)
                                            loadProducts(productsPagination.currentPage + 1, productSearch)
                                              .finally(() => setIsAutoLoading(false))
                                          }
                                        }}
                                      >
                                        {products.map((product) => {
                                          const isSelected =
                                            formData.selectedProducts.some(
                                              (p) => p.id === product.id
                                            );

                                          return (
                                            <CommandItem
                                              key={product.id}
                                              onSelect={() => {
                                                if (isSelected) {
                                                  handleInputChange(
                                                    "selectedProducts",
                                                    formData.selectedProducts.filter(
                                                      (p) => p.id !== product.id
                                                    )
                                                  );
                                                } else {
                                                  handleInputChange(
                                                    "selectedProducts",
                                                    [
                                                      ...formData.selectedProducts,
                                                      product
                                                    ]
                                                  );
                                                }
                                              }}
                                              className="flex items-center gap-2"
                                            >
                                              <Checkbox checked={isSelected} />
                                              <div className="flex-1">
                                                <div className="font-medium">
                                                  {product.code}
                                                </div>
                                                <div className="text-xs text-gray-500 truncate">
                                                  {product.name}
                                                </div>
                                              </div>
                                            </CommandItem>
                                          );
                                        })}
                                        
                                        {/* Auto Loading Indicator */}
                                        {(loadingProducts || isAutoLoading) && productsPagination.hasMore && (
                                          <div className="flex justify-center py-3 border-t">
                                            <div className="flex items-center gap-2 text-xs text-gray-500">
                                              <div className="animate-spin rounded-full h-3 w-3 border-2 border-gray-300 border-t-blue-600" />
                                              Loading more products...
                                            </div>
                                          </div>
                                        )}
                                        
                                        {/* End of list indicator */}
                                        {!productsPagination.hasMore && products.length > 0 && (
                                          <div className="text-center py-2 border-t">
                                            <p className="text-xs text-gray-400">All products loaded</p>
                                          </div>
                                        )}
                                      </div>
                                    </CommandGroup>
                                  </>
                                )}
                              </Command>
                            </PopoverContent>
                          </Popover>

                          <Button
                            variant="outline"
                            onClick={() =>
                              document.getElementById("product-upload")?.click()
                            }
                          >
                            <Upload className="mr-2 h-4 w-4" />
                            Import CSV
                          </Button>
                          <input
                            type="file"
                            id="product-upload"
                            accept=".csv,.xlsx"
                            className="hidden"
                            onChange={(e) => {
                              const file = e.target.files?.[0];
                              if (file) {
                                // TODO: Handle product CSV upload
                                toast({
                                  title: "Feature Coming Soon",
                                  description:
                                    "Product CSV import will be available soon"
                                });
                              }
                            }}
                          />
                        </div>
                      </div>

                      {/* Selected Products List */}
                      <Card className="border border-gray-100 shadow-sm">
                        <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100/50 border-b">
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2">
                              <Package className="h-5 w-5 text-blue-500" />
                              <CardTitle className="text-lg">
                                Product Selection
                              </CardTitle>
                            </div>
                            {formData.selectedProducts.length > 0 && (
                              <Badge className="bg-blue-100 text-blue-700 border-0">
                                {formData.selectedProducts.length} items
                              </Badge>
                            )}
                          </div>
                        </CardHeader>
                        <CardContent className="pt-6">
                          {formData.selectedProducts.length === 0 ? (
                            <div className="flex flex-col items-center justify-center py-12 text-center">
                              <div className="p-4 bg-gray-100 rounded-full mb-4">
                                <ShoppingCart className="h-8 w-8 text-gray-400" />
                              </div>
                              <p className="text-gray-500 font-medium">
                                No products selected
                              </p>
                              <p className="text-sm text-gray-400 mt-1">
                                Use the dropdown to select products
                              </p>
                            </div>
                          ) : (
                            <div className="space-y-2 max-h-96 overflow-y-auto">
                              {formData.selectedProducts.map(
                                (product, index) => (
                                  <div
                                    key={index}
                                    className="group flex items-center justify-between p-3 bg-white border rounded-lg hover:border-blue-200 hover:shadow-sm transition-all"
                                  >
                                    <div className="flex items-center gap-3 flex-1">
                                      <div className="p-2 bg-blue-50 rounded-lg">
                                        <Package className="h-4 w-4 text-blue-600" />
                                      </div>
                                      <div className="flex-1 grid grid-cols-2 gap-4">
                                        <div>
                                          <div className="font-medium text-sm text-gray-900">
                                            {product.code}
                                          </div>
                                          <div className="text-xs text-gray-500">
                                            {product.name}
                                          </div>
                                        </div>
                                        <div>
                                          <div className="text-xs text-gray-500">Barcode</div>
                                          <div className="font-mono text-xs">
                                            {product.barcode || product.ean || product.EAN || 'N/A'}
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                    <div className="flex items-center gap-3">
                                      <div className="flex flex-col gap-1">
                                        <div className="text-xs text-gray-500">PTT Price</div>
                                        <input
                                          type="number"
                                          placeholder="0.00"
                                          value={productPttPrices[product.id] || ''}
                                          onChange={(e) => {
                                            const value = parseFloat(e.target.value) || 0;
                                            setProductPttPrices(prev => ({
                                              ...prev,
                                              [product.id]: value
                                            }));
                                          }}
                                          className="w-24 px-2 py-1 text-sm font-semibold text-green-600 border border-gray-200 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                          step="0.01"
                                          min="0"
                                        />
                                      </div>
                                      {product.price > 0 && (
                                        <div className="text-right">
                                          <div className="text-xs text-gray-500">List Price</div>
                                          <div className="text-sm font-semibold text-gray-700">
                                            {product.price}
                                          </div>
                                        </div>
                                      )}
                                      <Button
                                        variant="ghost"
                                        size="sm"
                                        onClick={() =>
                                          handleInputChange(
                                            "selectedProducts",
                                            formData.selectedProducts.filter(
                                              (p) => p.id !== product.id
                                            )
                                          )
                                        }
                                        className="opacity-0 group-hover:opacity-100 transition-opacity h-8 w-8 p-0"
                                      >
                                        <X className="h-4 w-4 text-red-500" />
                                      </Button>
                                    </div>
                                  </div>
                                )
                              )}
                            </div>
                          )}
                        </CardContent>
                      </Card>
                    </div>
                  )}

                  {currentStep === 5 && (
                    <div className="space-y-6 animate-fadeIn">
                      {/* Selected Allocation Display */}
                      <SelectedAllocationDisplay />
                      <div className="flex items-center justify-between">
                        <div>
                          <h3 className="text-xl font-bold text-gray-900">
                            Review & Confirm
                          </h3>
                          <p className="text-sm text-gray-500 mt-1">
                            Please review all details before creating the
                            initiative
                          </p>
                        </div>
                        <Badge className="bg-green-100 text-green-700 border-0 px-3 py-1">
                          <CheckCircle2 className="mr-1 h-4 w-4" />
                          Ready to Submit
                        </Badge>
                      </div>

                      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        <Card className="border border-gray-100 shadow-sm hover:shadow-md transition-shadow">
                          <CardHeader className="bg-gradient-to-r from-blue-50 to-purple-50 border-b">
                            <div className="flex items-center gap-2">
                              <FileText className="h-5 w-5 text-blue-500" />
                              <CardTitle className="text-lg">
                                Initiative Summary
                              </CardTitle>
                            </div>
                          </CardHeader>
                          <CardContent className="space-y-4 pt-6">
                            <div className="space-y-3">
                              <div className="flex justify-between py-2 border-b">
                                <span className="text-sm text-gray-500">
                                  Name
                                </span>
                                <span className="font-semibold text-gray-900">
                                  {formData.initiativeName || "Not set"}
                                </span>
                              </div>
                              <div className="flex justify-between py-2 border-b">
                                <span className="text-sm text-gray-500">
                                  Customer Type
                                </span>
                                <Badge variant="secondary">
                                  {formData.customerType || "Not selected"}
                                </Badge>
                              </div>
                              <div className="flex justify-between py-2 border-b">
                                <span className="text-sm text-gray-500">
                                  Activity
                                </span>
                                <span className="font-medium">
                                  {formData.activityType || "Not selected"}
                                </span>
                              </div>
                              <div className="flex justify-between py-2 border-b">
                                <span className="text-sm text-gray-500">
                                  Duration
                                </span>
                                <div className="text-right">
                                  <div className="font-medium">
                                    {format(
                                      new Date(formData.startDate),
                                      "MMM dd"
                                    )}{" "}
                                    -{" "}
                                    {format(
                                      new Date(formData.endDate),
                                      "MMM dd, yyyy"
                                    )}
                                  </div>
                                  <div className="text-xs text-gray-500">
                                    {Math.ceil(
                                      (new Date(formData.endDate).getTime() -
                                        new Date(
                                          formData.startDate
                                        ).getTime()) /
                                        (1000 * 60 * 60 * 24)
                                    )}{" "}
                                    days
                                  </div>
                                </div>
                              </div>
                              <div className="flex justify-between py-2">
                                <span className="text-sm text-gray-500">
                                  Status
                                </span>
                                <Badge
                                  className={
                                    formData.isActive
                                      ? "bg-green-100 text-green-700 border-0"
                                      : "bg-gray-100 text-gray-700 border-0"
                                  }
                                >
                                  {formData.isActive ? "Active" : "Inactive"}
                                </Badge>
                              </div>
                            </div>
                          </CardContent>
                        </Card>

                        <Card className="border border-gray-100 shadow-sm hover:shadow-md transition-shadow">
                          <CardHeader className="bg-gradient-to-r from-green-50 to-emerald-50 border-b">
                            <CardTitle className="text-lg">
                              Budget Allocation
                            </CardTitle>
                          </CardHeader>
                          <CardContent className="space-y-4 pt-6">
                            {formData.selectedAllocation ? (
                              <>
                                <div className="space-y-3">
                                  <div className="flex justify-between py-2 border-b">
                                    <span className="text-sm text-gray-500">
                                      Allocation No
                                    </span>
                                    <span className="font-mono font-semibold">
                                      {formData.selectedAllocation.allocationNo}
                                    </span>
                                  </div>
                                  <div className="flex justify-between py-2 border-b">
                                    <span className="text-sm text-gray-500">
                                      Brand
                                    </span>
                                    <Badge className="bg-purple-100 text-purple-700 border-0">
                                      {formData.selectedAllocation.brand}
                                    </Badge>
                                  </div>
                                  <div className="flex justify-between py-2 border-b">
                                    <span className="text-sm text-gray-500">
                                      Available Budget
                                    </span>
                                    <span className="font-medium">
                                      {formData.selectedAllocation.availableAmount.toLocaleString()}
                                    </span>
                                  </div>
                                  <div className="flex justify-between py-2">
                                    <span className="text-sm text-gray-500">
                                      Contract Amount
                                    </span>
                                    <span className="font-bold text-xl text-green-600">
                                      {formData.contractAmount.toLocaleString()}
                                    </span>
                                  </div>
                                </div>

                                <div className="p-4 bg-blue-50 rounded-xl">
                                  <div className="flex justify-between items-center mb-2">
                                    <span className="text-sm text-blue-700 font-medium">
                                      Budget Utilization
                                    </span>
                                    <span className="text-sm text-blue-700 font-bold">
                                      {(
                                        (formData.contractAmount /
                                          formData.selectedAllocation
                                            .availableAmount) *
                                        100
                                      ).toFixed(0)}
                                      %
                                    </span>
                                  </div>
                                  <Progress
                                    value={
                                      (formData.contractAmount /
                                        formData.selectedAllocation
                                          .availableAmount) *
                                      100
                                    }
                                    className="h-2"
                                  />
                                </div>
                              </>
                            ) : (
                              <div className="text-center py-8 text-gray-500">
                                No allocation selected
                              </div>
                            )}
                          </CardContent>
                        </Card>
                      </div>

                      <Card className="border border-gray-100 shadow-sm hover:shadow-md transition-shadow col-span-full">
                        <CardHeader className="bg-gradient-to-r from-purple-50 to-pink-50 border-b">
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2">
                              <Package2 className="h-5 w-5 text-purple-500" />
                              <CardTitle className="text-lg">
                                Product Selection
                              </CardTitle>
                            </div>
                            <Badge className="bg-purple-100 text-purple-700 border-0">
                              {formData.selectedProducts.length} products
                            </Badge>
                          </div>
                        </CardHeader>
                        <CardContent className="pt-6">
                          {formData.selectedProducts.length === 0 ? (
                            <div className="text-center py-8 text-gray-500">
                              <Package className="h-12 w-12 text-gray-300 mx-auto mb-3" />
                              <p>No products selected</p>
                            </div>
                          ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                              {formData.selectedProducts
                                .slice(0, 6)
                                .map((product, index) => (
                                  <div
                                    key={index}
                                    className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg"
                                  >
                                    <div className="p-2 bg-white rounded">
                                      <Package className="h-4 w-4 text-gray-500" />
                                    </div>
                                    <div className="flex-1">
                                      <div className="font-medium text-sm">
                                        {product.name}
                                      </div>
                                      <div className="text-xs text-gray-500">
                                        {product.price}
                                      </div>
                                    </div>
                                  </div>
                                ))}
                              {formData.selectedProducts.length > 6 && (
                                <div className="col-span-full text-center py-2">
                                  <Badge
                                    variant="secondary"
                                    className="text-xs"
                                  >
                                    +{formData.selectedProducts.length - 6} more
                                    products
                                  </Badge>
                                </div>
                              )}
                            </div>
                          )}
                        </CardContent>
                      </Card>
                    </div>
                  )}
                </div>

                {/* Enhanced Action Buttons */}
                <div className="flex justify-between items-center pt-8 border-t border-gray-100">
                  <div>
                    {currentStep > 1 && (
                      <Button
                        variant="outline"
                        onClick={goToPrevious}
                        className="rounded-xl hover:shadow-md transition-all"
                      >
                        <ArrowLeft className="mr-2 h-4 w-4" />
                        Previous
                      </Button>
                    )}
                  </div>

                  <div className="flex items-center gap-3">
                    {currentStep < 4 ? (
                      <Button
                        onClick={proceedToNext}
                        disabled={!validateCurrentStep()}
                        className="rounded-xl bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600 shadow-lg hover:shadow-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        Continue
                        <ArrowRight className="ml-2 h-4 w-4" />
                      </Button>
                    ) : (
                      <Button
                        onClick={handleSave}
                        disabled={loading}
                        className="rounded-xl bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600 shadow-lg hover:shadow-xl transition-all disabled:opacity-50"
                      >
                        {loading ? (
                          <>
                            <div className="inline-block mr-2">
                              <Skeleton className="h-4 w-4 rounded-full" />
                            </div>
                            Creating Initiative...
                          </>
                        ) : (
                          <>
                            <CheckCircle2 className="mr-2 h-4 w-4" />
                            Create Initiative
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

      {/* Customer Selection Dialog */}
      <Dialog open={showCustomerPopup} onOpenChange={setShowCustomerPopup}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Select Parent Customers</DialogTitle>
            <DialogDescription>
              Choose customers for this initiative
            </DialogDescription>
          </DialogHeader>
          <div className="py-4">
            <Input
              placeholder="Search by Customer Code / Name"
              className="mb-4"
            />
            <div className="max-h-96 overflow-auto">
              <div className="text-center text-gray-500 py-8">
                No customers available
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setShowCustomerPopup(false)}
            >
              Cancel
            </Button>
            <Button>Submit</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
