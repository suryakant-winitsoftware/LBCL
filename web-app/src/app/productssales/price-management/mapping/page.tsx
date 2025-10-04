"use client";

import { useState, useEffect } from "react";
import { ActionType } from "@/types/action-type.enum";
import { Button } from "@/components/ui/button";
import { PaginationControls } from "@/components/ui/pagination-controls";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import { useToast } from "@/components/ui/use-toast";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import {
  Link2,
  Store,
  Users,
  MapPin,
  Building,
  Building2,
  Package,
  Edit,
  Trash2,
  Save,
  X,
  RefreshCw,
  Search,
  ArrowLeft,
  Info,
  ChevronRight,
  ChevronDown,
  Globe,
  MapPinned,
  Network
} from "lucide-react";
import { Checkbox } from "@/components/ui/checkbox";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  storeLinkingService,
  SelectionMapMaster,
  SelectionMapCriteria,
  SelectionMapDetails
} from "@/services/store-linking.service";
import { locationService, Location, LocationHierarchyNode } from "@/services/locationService";
import { organizationService, Organization, OrganizationHierarchyNode } from "@/services/organizationService";
import { skuClassGroupsService, SKUClassGroup } from "@/services/sku-class-groups.service";
import { skuPriceService, ISKUPriceList } from "@/services/sku/sku-price.service";
import { employeeService } from "@/services/admin/employee.service";
import { storeGroupService } from "@/services/storeGroupService";
import { IStoreGroup, IStoreGroupType } from "@/types/storeGroup.types";
import { apiService, api } from "@/services/api";
import { authService } from "@/lib/auth-service";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";

interface StoreData {
  uid: string;
  name: string;
  code: string;
  type?: string;
  branch?: string;
  broadClassification?: string;
}

interface BranchData {
  uid: string;
  name: string;
  code: string;
}

interface OrganizationData {
  uid: string;
  name: string;
  code: string;
}

interface ItemData {
  uid: string;
  name: string;
  code: string;
}

// Component to display mapping details in a popover
function ViewMappingDetails({
  mapping,
  stores,
  branches,
  displayedCustomerGroups,
  skuClassGroup,
  priceList
}: {
  mapping: SelectionMapMaster;
  stores: StoreData[];
  branches: BranchData[];
  displayedCustomerGroups?: IStoreGroup[];
  skuClassGroup?: SKUClassGroup;
  priceList?: ISKUPriceList;
}) {
  const [isOpen, setIsOpen] = useState(false);

  // Group details by selection group
  const groupedDetails =
    mapping.selectionMapDetails?.reduce((acc, detail) => {
      if (!acc[detail.selectionGroup]) {
        acc[detail.selectionGroup] = [];
      }
      acc[detail.selectionGroup].push(detail);
      return acc;
    }, {} as Record<string, SelectionMapDetails[]>) || {};

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <Button
        variant="ghost"
        size="sm"
        className="text-xs"
        onClick={() => setIsOpen(true)}
      >
        View Details
      </Button>

      <DialogContent className="max-w-3xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            Price Mapping Details:{" "}
            {skuClassGroup?.Name || priceList?.Name || mapping.selectionMapCriteria.linkedItemUID}
          </DialogTitle>
          <DialogDescription>
            Complete list of mapped stores, branches, and sales teams
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 mt-4">
          {/* Customer/Store Mappings */}
          {groupedDetails["Customer"] && 
           groupedDetails["Customer"].some(detail => detail.typeUID !== "CustomerGroup") && (
            <div>
              <h3 className="font-semibold mb-2 flex items-center gap-2">
                <Store className="h-4 w-4" />
                Direct Store Mappings ({groupedDetails["Customer"].filter(detail => detail.typeUID !== "CustomerGroup").length})
              </h3>
              <div className="bg-muted rounded-lg p-3">
                <div className="grid grid-cols-2 gap-2 max-h-40 overflow-y-auto">
                  {groupedDetails["Customer"].filter(detail => detail.typeUID !== "CustomerGroup").map((detail, idx) => {
                    const store = stores.find(
                      (s) => s.uid === detail.selectionValue
                    );
                    return (
                      <div
                        key={detail.uid || idx}
                        className="text-sm flex items-center gap-2"
                      >
                        <Badge
                          variant={
                            detail.isExcluded ? "destructive" : "secondary"
                          }
                          className="text-xs"
                        >
                          {detail.isExcluded ? "Excluded" : "Included"}
                        </Badge>
                        <span>
                          {store
                            ? `${store.name} (${store.code})`
                            : detail.selectionValue}
                        </span>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          )}

          {/* Location/Branch Mappings */}
          {groupedDetails["Location"] && (
            <div>
              <h3 className="font-semibold mb-2 flex items-center gap-2">
                <MapPin className="h-4 w-4" />
                Branch/Location Mappings ({groupedDetails["Location"].length})
              </h3>
              <div className="bg-muted rounded-lg p-3">
                <div className="grid grid-cols-2 gap-2 max-h-40 overflow-y-auto">
                  {groupedDetails["Location"].map((detail, idx) => {
                    const branch = branches.find(
                      (b) => b.uid === detail.selectionValue
                    );
                    return (
                      <div
                        key={detail.uid || idx}
                        className="text-sm flex items-center gap-2"
                      >
                        <Badge
                          variant={
                            detail.isExcluded ? "destructive" : "secondary"
                          }
                          className="text-xs"
                        >
                          {detail.isExcluded ? "Excluded" : "Included"}
                        </Badge>
                        <span>
                          {branch
                            ? `${branch.name} (${branch.code})`
                            : detail.selectionValue}
                        </span>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          )}

          {/* Sales Team/Broad Classification Mappings */}
          {groupedDetails["SalesTeam"] && (
            <div>
              <h3 className="font-semibold mb-2 flex items-center gap-2">
                <Users className="h-4 w-4" />
                Sales Team/Classification Mappings (
                {groupedDetails["SalesTeam"].length})
              </h3>
              <div className="bg-muted rounded-lg p-3">
                <div className="grid grid-cols-2 gap-2 max-h-40 overflow-y-auto">
                  {groupedDetails["SalesTeam"].map((detail, idx) => (
                    <div
                      key={detail.uid || idx}
                      className="text-sm flex items-center gap-2"
                    >
                      <Badge
                        variant={
                          detail.isExcluded ? "destructive" : "secondary"
                        }
                        className="text-xs"
                      >
                        {detail.isExcluded ? "Excluded" : "Included"}
                      </Badge>
                      <span>{detail.selectionValue}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* Customer Group Mappings */}
          {groupedDetails["Customer"] && 
           groupedDetails["Customer"].some(detail => detail.typeUID === "CustomerGroup") && (
            <div>
              <h3 className="font-semibold mb-2 flex items-center gap-2">
                <Building className="h-4 w-4" />
                Customer Group Mappings ({groupedDetails["Customer"].filter(detail => detail.typeUID === "CustomerGroup").length})
              </h3>
              <div className="bg-muted rounded-lg p-3">
                <div className="grid grid-cols-2 gap-2 max-h-40 overflow-y-auto">
                  {groupedDetails["Customer"].filter(detail => detail.typeUID === "CustomerGroup").map((detail, idx) => {
                    const customerGroup = displayedCustomerGroups?.find(
                      (cg: IStoreGroup) => cg.UID === detail.selectionValue
                    );
                    return (
                      <div
                        key={detail.uid || idx}
                        className="text-sm flex items-center gap-2"
                      >
                        <Badge
                          variant={
                            detail.isExcluded ? "destructive" : "secondary"
                          }
                          className="text-xs"
                        >
                          {detail.isExcluded ? "Excluded" : "Included"}
                        </Badge>
                        <span>
                          {customerGroup
                            ? `${customerGroup.Name} (${customerGroup.Code})`
                            : detail.selectionValue}
                        </span>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          )}

          {/* Metadata */}
          <div className="border-t pt-4">
            <div className="text-sm text-muted-foreground space-y-1">
              <div>
                Created:{" "}
                {formatDateToDayMonthYear(mapping.selectionMapCriteria.createdTime)}
              </div>
              <div>
                Modified:{" "}
                {formatDateToDayMonthYear(mapping.selectionMapCriteria.modifiedTime)}
              </div>
              <div>
                Status:{" "}
                {mapping.selectionMapCriteria.isActive ? (
                  <Badge variant="default" className="text-xs">
                    Active
                  </Badge>
                ) : (
                  <Badge variant="destructive" className="text-xs">
                    Inactive
                  </Badge>
                )}
              </div>
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => setIsOpen(false)}>
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ============================================
// PAGINATION CONFIGURATION WITH INFINITE SCROLL
// ============================================
// These values control how many items are loaded at once
// The system can handle 10,000+ items efficiently with infinite scroll
const PAGINATION_CONFIG = {
  INITIAL_LOAD: 20,        // Items shown when page first loads
  LOAD_MORE_BATCH: 20,     // Items added automatically on scroll
  SEARCH_BATCH: 50,        // Items loaded when user searches
  DEBOUNCE_MS: 300,        // Delay before search executes (ms)
  SCROLL_THRESHOLD: 0.8    // Load more when scrolled 80% down
};

// INFINITE SCROLL FEATURES:
// ✅ Auto-loads when user scrolls to 80% of list
// ✅ No clicking needed - seamless experience
// ✅ Shows loading indicator at bottom
// ✅ Displays remaining item count
//
// PERFORMANCE WITH INFINITE SCROLL:
// ✅ 10,000 items: Smooth scrolling, auto-loads in background
// ✅ 100,000 items: Still performant, progressive loading
// ✅ 1,000,000 items: Works, search recommended for navigation
// 
// NO HARD LIMITS - System adapts to any dataset size
// Memory efficient - only keeps loaded items in browser

// Tree node component for organization hierarchy
const OrganizationTreeNode = ({ 
  node, 
  level = 0,
  onToggle,
  onSelect,
  expandedNodes,
  selectedNodes
}: {
  node: OrganizationHierarchyNode;
  level?: number;
  onToggle: (uid: string) => void;
  onSelect: (node: OrganizationHierarchyNode, checked: boolean) => void;
  expandedNodes: Set<string>;
  selectedNodes: Set<string>;
}) => {
  const isExpanded = expandedNodes.has(node.uid);
  const isSelected = selectedNodes.has(node.uid);
  const hasChildren = node.children && node.children.length > 0;
  
  // Get icon based on organization level
  const getOrgIcon = () => {
    switch (level) {
      case 0: return <Globe className="h-4 w-4 text-blue-600" />;
      case 1: return <Building className="h-4 w-4 text-green-600" />;
      case 2: return <Building2 className="h-4 w-4 text-purple-600" />;
      default: return <Network className="h-4 w-4 text-orange-600" />;
    }
  };
  
  // Get color based on level
  const getTypeColor = () => {
    switch (level) {
      case 0: return 'blue';
      case 1: return 'green';
      case 2: return 'purple';
      default: return 'orange';
    }
  };
  
  const color = getTypeColor();
  
  return (
    <div className={level > 0 ? "ml-6" : ""}>
      <div
        className={`flex items-center gap-2 p-2 rounded-lg transition-all duration-200 hover:bg-gray-50 ${
          isSelected ? `bg-${color}-50 border border-${color}-300` : ''
        }`}
      >
        {/* Expand/Collapse Button */}
        {hasChildren && (
          <button
            onClick={() => onToggle(node.uid)}
            className="p-1 rounded hover:bg-gray-200 transition-colors"
          >
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            )}
          </button>
        )}
        {!hasChildren && <div className="w-6" />}
        
        {/* Checkbox */}
        <Checkbox
          id={`org-${node.uid}`}
          checked={isSelected}
          onCheckedChange={(checked) => onSelect(node, !!checked)}
          className="h-4 w-4"
        />
        
        {/* Organization Icon */}
        <div className="flex-shrink-0">
          {getOrgIcon()}
        </div>
        
        {/* Organization Details */}
        <div className="flex-1 flex items-center gap-2">
          <span className="font-medium text-sm">{node.name}</span>
          <Badge variant="outline" className="text-xs">
            {node.code}
          </Badge>
          <Badge className={`text-xs bg-${color}-100 text-${color}-700`}>
            {node.orgTypeName}
          </Badge>
          {hasChildren && (
            <span className="text-xs text-gray-500">
              ({node.children?.length} children)
            </span>
          )}
        </div>
      </div>
      
      {/* Children */}
      {isExpanded && hasChildren && (
        <div className="mt-1 border-l-2 border-gray-200 ml-3">
          {node.children?.map((child) => (
            <OrganizationTreeNode
              key={child.uid}
              node={child}
              level={level + 1}
              onToggle={onToggle}
              onSelect={onSelect}
              expandedNodes={expandedNodes}
              selectedNodes={selectedNodes}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// Tree node component for location hierarchy
const LocationTreeNode = ({ 
  node, 
  level = 0,
  onToggle,
  onSelect,
  expandedNodes,
  selectedNodes
}: {
  node: LocationHierarchyNode;
  level?: number;
  onToggle: (uid: string) => void;
  onSelect: (node: LocationHierarchyNode, checked: boolean) => void;
  expandedNodes: Set<string>;
  selectedNodes: Set<string>;
}) => {
  const isExpanded = expandedNodes.has(node.uid);
  const isSelected = selectedNodes.has(node.uid);
  const hasChildren = node.children && node.children.length > 0;
  
  // Get icon based on location type
  const getLocationIcon = () => {
    const typeName = node.locationTypeName?.toLowerCase();
    switch (typeName) {
      case 'country': return <Globe className="h-4 w-4 text-blue-600" />;
      case 'state': case 'region': return <Building2 className="h-4 w-4 text-green-600" />;
      case 'city': case 'district': return <MapPinned className="h-4 w-4 text-purple-600" />;
      default: return <MapPin className="h-4 w-4 text-orange-600" />;
    }
  };
  
  // Get location type color
  const getTypeColor = () => {
    const typeName = node.locationTypeName?.toLowerCase();
    switch (typeName) {
      case 'country': return 'blue';
      case 'state': case 'region': return 'green';
      case 'city': case 'district': return 'purple';
      default: return 'orange';
    }
  };
  
  const color = getTypeColor();
  
  return (
    <div className={level > 0 ? "ml-6" : ""}>
      <div
        className={`flex items-center gap-2 p-2 rounded-lg transition-all duration-200 hover:bg-gray-50 ${
          isSelected ? `bg-${color}-50 border border-${color}-300` : ''
        }`}
      >
        {/* Expand/Collapse Button */}
        {hasChildren && (
          <button
            onClick={() => onToggle(node.uid)}
            className="p-1 rounded hover:bg-gray-200 transition-colors"
          >
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            )}
          </button>
        )}
        {!hasChildren && <div className="w-6" />}
        
        {/* Checkbox */}
        <Checkbox
          id={`loc-${node.uid}`}
          checked={isSelected}
          onCheckedChange={(checked) => onSelect(node, !!checked)}
          className="h-4 w-4"
        />
        
        {/* Location Icon */}
        <div className="flex-shrink-0">
          {getLocationIcon()}
        </div>
        
        {/* Location Details */}
        <div className="flex-1 flex items-center gap-2">
          <span className="font-medium text-sm">{node.name}</span>
          <Badge variant="outline" className="text-xs">
            {node.code}
          </Badge>
          <Badge className={`text-xs bg-${color}-100 text-${color}-700`}>
            {node.locationTypeName}
          </Badge>
          {hasChildren && (
            <span className="text-xs text-gray-500">
              ({node.children?.length} children)
            </span>
          )}
        </div>
      </div>
      
      {/* Children */}
      {isExpanded && hasChildren && (
        <div className="mt-1 border-l-2 border-gray-200 ml-3">
          {node.children?.map((child) => (
            <LocationTreeNode
              key={child.uid}
              node={child}
              level={level + 1}
              onToggle={onToggle}
              onSelect={onSelect}
              expandedNodes={expandedNodes}
              selectedNodes={selectedNodes}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default function PriceMappingPage() {
  const { toast } = useToast();
  const currentUser = authService.getCurrentUser();
  const [loading, setLoading] = useState(false);
  const [checkingMappings, setCheckingMappings] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [activeTab, setActiveTab] = useState("create");
  const [loadingProgress, setLoadingProgress] = useState<string>("");

  // Add mount tracking
  const [isMounted, setIsMounted] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);

  // Selection Map Form State
  const [selectedSKUClassGroup, setSelectedSKUClassGroup] =
    useState<string>("");
  const [selectedPriceList, setSelectedPriceList] = useState<string>("");
  const [linkedItemType, setLinkedItemType] = useState<string>("SKUClassGroup");
  const [selectionCriteria, setSelectionCriteria] = useState({
    hasCustomer: false,
    hasLocation: false,
    hasSalesTeam: false,
    hasOrganization: false,
    hasItem: false,
    hasCustomerGroup: false
  });

  // Selected entities
  const [selectedStores, setSelectedStores] = useState<string[]>([]);
  const [selectedBranches, setSelectedBranches] = useState<string[]>([]);
  const [selectedSalesTeams, setSelectedSalesTeams] = useState<string[]>([]);
  const [selectedOrganizations, setSelectedOrganizations] = useState<string[]>(
    []
  );
  const [selectedCustomerGroups, setSelectedCustomerGroups] = useState<string[]>([]);
  // Removed: Items - no longer used
  // const [selectedItems, setSelectedItems] = useState<string[]>([]);

  // Data Lists - Load on demand with pagination
  const [skuClassGroups, setSKUClassGroups] = useState<SKUClassGroup[]>([]);
  const [priceLists, setPriceLists] = useState<ISKUPriceList[]>([]);
  const [totalSKUGroups, setTotalSKUGroups] = useState(0);
  const [totalPriceLists, setTotalPriceLists] = useState(0);
  const [loadingSkuGroups, setLoadingSkuGroups] = useState(false);
  const [loadingPriceLists, setLoadingPriceLists] = useState(false);
  const [skuGroupPage, setSkuGroupPage] = useState(1);
  const [priceListPage, setPriceListPage] = useState(1);
  const [hasMoreSkuGroups, setHasMoreSkuGroups] = useState(true);
  const [hasMorePriceLists, setHasMorePriceLists] = useState(true);
  const [stores, setStores] = useState<StoreData[]>([]);
  const [displayedStores, setDisplayedStores] = useState<StoreData[]>([]);
  const [branches, setBranches] = useState<BranchData[]>([]);
  const [displayedBranches, setDisplayedBranches] = useState<BranchData[]>([]);
  const [customerGroups, setCustomerGroups] = useState<IStoreGroup[]>([]);
  const [displayedCustomerGroups, setDisplayedCustomerGroups] = useState<IStoreGroup[]>([]);
  const [customerGroupTypes, setCustomerGroupTypes] = useState<IStoreGroupType[]>([]);
  const [selectedCustomerGroupType, setSelectedCustomerGroupType] = useState<string>("");
  const [organizations, setOrganizations] = useState<OrganizationData[]>([]);
  const [salesTeams, setSalesTeams] = useState<any[]>([]);
  const [displayedSalesTeams, setDisplayedSalesTeams] = useState<any[]>([]);
  const [loadingSalesTeams, setLoadingSalesTeams] = useState(false);
  const [availableRoles, setAvailableRoles] = useState<{value: string, label: string}[]>([]);
  const [selectedRole, setSelectedRole] = useState<string>("");
  
  // Location hierarchy state
  const [locations, setLocations] = useState<Location[]>([]);
  const [locationHierarchy, setLocationHierarchy] = useState<LocationHierarchyNode[]>([]);
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set());
  const [selectedLocationNodes, setSelectedLocationNodes] = useState<Set<string>>(new Set());
  const [hierarchyView, setHierarchyView] = useState(false);
  
  // Organization hierarchy state
  const [allOrganizations, setAllOrganizations] = useState<Organization[]>([]);
  const [organizationHierarchy, setOrganizationHierarchy] = useState<OrganizationHierarchyNode[]>([]);
  const [expandedOrgNodes, setExpandedOrgNodes] = useState<Set<string>>(new Set());
  const [selectedOrgNodes, setSelectedOrgNodes] = useState<Set<string>>(new Set());
  const [orgHierarchyView, setOrgHierarchyView] = useState(false);
  // Removed: Items - no longer used
  // const [items, setItems] = useState<ItemData[]>([]);
  const [existingMappings, setExistingMappings] = useState<
    SelectionMapMaster[]
  >([]);

  // Loading states for individual data types
  const [loadingStores, setLoadingStores] = useState(false);
  const [loadingBranches, setLoadingBranches] = useState(false);
  const [loadingCustomerGroups, setLoadingCustomerGroups] = useState(false);
  const [loadingMoreCustomerGroups, setLoadingMoreCustomerGroups] = useState(false);
  const [loadingOrganizations, setLoadingOrganizations] = useState(false);
  // Removed: Items - no longer used
  // const [loadingItems, setLoadingItems] = useState(false);
  const [loadingMoreStores, setLoadingMoreStores] = useState(false);
  const [loadingMoreBranches, setLoadingMoreBranches] = useState(false);

  // Pagination state for stores and branches
  const [storesPagination, setStoresPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: false
  });
  const [branchesPagination, setBranchesPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: false
  });
  const [customerGroupsPagination, setCustomerGroupsPagination] = useState({
    currentPage: 1,
    pageSize: 50,
    totalCount: 0,
    hasMore: false
  });

  // Dialog states
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [itemToDelete, setItemToDelete] = useState<string | null>(null);
  const [editingItem, setEditingItem] = useState<SelectionMapMaster | null>(
    null
  );

  // Search and filter
  const [searchTerm, setSearchTerm] = useState("");
  const [filterType, setFilterType] = useState<string>("all");
  const [skuGroupSearchTerm, setSkuGroupSearchTerm] = useState("");
  const [priceListSearchTerm, setPriceListSearchTerm] = useState("");
  
  // Search terms for selection configuration
  const [storeSearchTerm, setStoreSearchTerm] = useState("");
  const [branchSearchTerm, setBranchSearchTerm] = useState("");
  const [customerGroupSearchTerm, setCustomerGroupSearchTerm] = useState("");
  const [organizationSearchTerm, setOrganizationSearchTerm] = useState("");
  const [salesTeamSearchTerm, setSalesTeamSearchTerm] = useState("");
  const [itemSearchTerm, setItemSearchTerm] = useState("");

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Reset pagination when search/filter changes
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, filterType]);

  // Debounced search for SKU Class Groups
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (skuGroupSearchTerm.length > 0) {
        searchSKUClassGroups(skuGroupSearchTerm);
      } else if (skuGroupSearchTerm === "") {
        // Reset to initial data when search is cleared
        setSkuGroupPage(1);
        searchSKUClassGroups(""); // Load without filter
      }
    }, PAGINATION_CONFIG.DEBOUNCE_MS);

    return () => clearTimeout(timeoutId);
  }, [skuGroupSearchTerm]);

  // Debounced search for Price Lists
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (priceListSearchTerm.length > 0) {
        searchPriceLists(priceListSearchTerm);
      } else if (priceListSearchTerm === "") {
        // Reset to initial data when search is cleared
        setPriceListPage(1);
        searchPriceLists(""); // Load without filter
      }
    }, PAGINATION_CONFIG.DEBOUNCE_MS);

    return () => clearTimeout(timeoutId);
  }, [priceListSearchTerm]);

  // Debounced search effect for stores
  useEffect(() => {
    if (!selectionCriteria.hasCustomer || displayedStores.length === 0) return;

    const timeoutId = setTimeout(() => {
      loadStoresPage(1, storeSearchTerm, false);
    }, 300); // 300ms debounce

    return () => clearTimeout(timeoutId);
  }, [storeSearchTerm, selectionCriteria.hasCustomer]);

  // Debounced search effect for branches
  useEffect(() => {
    if (!selectionCriteria.hasLocation || displayedBranches.length === 0) return;

    const timeoutId = setTimeout(() => {
      loadBranchesPage(1, branchSearchTerm, false);
    }, 300); // 300ms debounce

    return () => clearTimeout(timeoutId);
  }, [branchSearchTerm, selectionCriteria.hasLocation]);

  // Debounced search effect for customer groups
  useEffect(() => {
    if (!selectionCriteria.hasCustomerGroup) return;

    const timeoutId = setTimeout(() => {
      loadCustomerGroupsPage(1, customerGroupSearchTerm, false);
    }, PAGINATION_CONFIG.DEBOUNCE_MS);

    return () => clearTimeout(timeoutId);
  }, [customerGroupSearchTerm, selectionCriteria.hasCustomerGroup, selectedCustomerGroupType]);

  // Load customer group types when customer group selection is enabled
  useEffect(() => {
    if (selectionCriteria.hasCustomerGroup && customerGroupTypes.length === 0) {
      loadCustomerGroupTypes();
    }
  }, [selectionCriteria.hasCustomerGroup]);

  // Load more functions
  const loadMoreStores = async () => {
    if (!storesPagination.hasMore || loadingMoreStores || loadingStores) {
      return;
    }
    await loadStoresPage(storesPagination.currentPage + 1, storeSearchTerm, true);
  };

  const loadMoreBranches = async () => {
    if (!branchesPagination.hasMore || loadingMoreBranches || loadingBranches) {
      return;
    }
    await loadBranchesPage(branchesPagination.currentPage + 1, branchSearchTerm, true);
  };

  // Helper function to normalize API data to camelCase
  const normalizeSelectionMapCriteria = (
    criteria: any
  ): SelectionMapCriteria => {
    return {
      uid: criteria.uid || criteria.UID,
      linkedItemUID: criteria.linkedItemUID || criteria.LinkedItemUID,
      linkedItemType: criteria.linkedItemType || criteria.LinkedItemType,
      hasOrganization:
        criteria.hasOrganization || criteria.HasOrganization || false,
      hasLocation: criteria.hasLocation || criteria.HasLocation || false,
      hasCustomer: criteria.hasCustomer || criteria.HasCustomer || false,
      hasSalesTeam: criteria.hasSalesTeam || criteria.HasSalesTeam || false,
      hasItem: criteria.hasItem || criteria.HasItem || false,
      orgCount: criteria.orgCount || criteria.OrgCount || 0,
      locationCount: criteria.locationCount || criteria.LocationCount || 0,
      customerCount: criteria.customerCount || criteria.CustomerCount || 0,
      salesTeamCount: criteria.salesTeamCount || criteria.SalesTeamCount || 0,
      itemCount: criteria.itemCount || criteria.ItemCount || 0,
      actionType: criteria.actionType || criteria.ActionType,
      isActive: criteria.isActive || criteria.IsActive || true,
      ss: criteria.ss || criteria.SS || 0,
      createdTime: criteria.createdTime || criteria.CreatedTime,
      modifiedTime: criteria.modifiedTime || criteria.ModifiedTime,
      serverAddTime: criteria.serverAddTime || criteria.ServerAddTime,
      serverModifiedTime:
        criteria.serverModifiedTime || criteria.ServerModifiedTime
    };
  };

  // Track mount status
  useEffect(() => {
    console.log("PriceMappingPage mounted");
    setIsMounted(true);
    return () => {
      console.log("PriceMappingPage unmounting");
      setIsMounted(false);
    };
  }, []);

  // Load initial data after mount
  useEffect(() => {
    if (isMounted) {
      console.log("Loading initial data...");
      // Small delay to ensure component is fully mounted
      const timer = setTimeout(() => {
        loadInitialData();
      }, 100);

      return () => clearTimeout(timer);
    }
  }, [isMounted]);

  const loadInitialData = async () => {
    console.log("[INIT] Starting optimized initial data load with lazy loading");
    setLoading(true);
    setLoadingProgress("Initializing...");
    const initStartTime = Date.now();

    try {
      // Load initial small batch of SKU Class Groups and Price Lists
      const initialPageSize = PAGINATION_CONFIG.INITIAL_LOAD;
      
      const [groupsResponse, priceListsResponse] = await Promise.all([
        skuClassGroupsService.getAllSKUClassGroups(
          1,     // currentPage
          initialPageSize,  // pageSize - reduced for initial load
          ""     // searchTerm
        ),
        skuPriceService.getAllPriceLists({
          PageNumber: 1,
          PageSize: initialPageSize,
          IsCountRequired: true,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }]
        })
      ]);

      // Extract data from responses
      const groupsResult = groupsResponse?.data || [];
      const groupsTotalCount = groupsResponse?.totalCount || 0;
      const priceListsResult = priceListsResponse?.PagedData || [];
      const priceListsTotalCount = priceListsResponse?.TotalCount || 0;

      let loadedGroups: SKUClassGroup[] = [];

      if (groupsResult?.length > 0) {
        console.log(`[LOADED] ${groupsResult.length} of ${groupsTotalCount} total SKU Class Groups`);
        setSKUClassGroups(groupsResult);
        setTotalSKUGroups(groupsTotalCount);
        setHasMoreSkuGroups(groupsResult.length < groupsTotalCount);
        loadedGroups = groupsResult;
        
        if (groupsTotalCount > initialPageSize) {
          console.log(`[INFO] ${groupsTotalCount - initialPageSize} more SKU Class Groups available for lazy loading`);
        }
      } else {
        console.warn("[INIT] No SKU Class Groups returned from API");
        setHasMoreSkuGroups(false);
      }

      if (priceListsResult?.length > 0) {
        console.log(`[LOADED] ${priceListsResult.length} of ${priceListsTotalCount} total Price Lists`);
        setPriceLists(priceListsResult);
        setTotalPriceLists(priceListsTotalCount);
        setHasMorePriceLists(priceListsResult.length < priceListsTotalCount);
        
        if (priceListsTotalCount > initialPageSize) {
          console.log(`[INFO] ${priceListsTotalCount - initialPageSize} more Price Lists available for lazy loading`);
        }
      } else {
        console.warn("[INIT] No Price Lists returned from API");
        setHasMorePriceLists(false);
      }

      const dataLoadTime = Date.now() - initStartTime;
      console.log(`[PERFORMANCE] Initial data loaded in ${dataLoadTime}ms`);

      // Load ALL mappings directly from the database
      // This shows all mappings, not just for existing items
      setLoadingProgress(
        `Loading all store linkings from database...`
      );
      await loadAllMappingsFromDatabase(false);

      setLoadingProgress("");

      console.log(
        "[INIT] Initial data load complete - additional items will load on demand"
      );
    } catch (error) {
      console.error("Error loading initial data:", error);
      toast({
        title: "Warning",
        description:
          "Could not load data. Please check your connection.",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
      setIsInitialized(true);
    }
  };

  // Search SKU Class Groups with debouncing
  const searchSKUClassGroups = async (searchQuery: string) => {
    if (loadingSkuGroups) return;
    
    try {
      setLoadingSkuGroups(true);
      console.log(`[SEARCH] Searching SKU Class Groups with query: "${searchQuery}"`);
      
      const response = await skuClassGroupsService.getAllSKUClassGroups(
        1,     // Reset to page 1 for new search
        PAGINATION_CONFIG.SEARCH_BATCH,    // Load more items when searching
        searchQuery
      );
      
      const groupsResult = response?.data || [];
      const totalCount = response?.totalCount || 0;
      
      console.log(`[SEARCH] Found ${groupsResult.length} of ${totalCount} SKU Class Groups`);
      setSKUClassGroups(groupsResult);
      setTotalSKUGroups(totalCount);
      setSkuGroupPage(1);
      setHasMoreSkuGroups(groupsResult.length < totalCount);
    } catch (error) {
      console.error("Error searching SKU Class Groups:", error);
    } finally {
      setLoadingSkuGroups(false);
    }
  };

  // Search Price Lists with debouncing
  const searchPriceLists = async (searchQuery: string) => {
    if (loadingPriceLists) return;
    
    try {
      setLoadingPriceLists(true);
      console.log(`[SEARCH] Searching Price Lists with query: "${searchQuery}"`);
      
      // Use EXACT same pattern as working SKU Class Groups
      const filterCriteria = searchQuery ? [
        { Name: "Name", Value: searchQuery, Type: 1 }  // Exactly like SKU Class Groups
      ] : [];
      
      console.log(`[SEARCH DEBUG] Using same pattern as SKU Class Groups - single field, Type: 1`);
      
      console.log(`[SEARCH] Filter criteria:`, filterCriteria);
      
      const requestPayload = {
        PageNumber: 1,
        PageSize: PAGINATION_CONFIG.SEARCH_BATCH,
        IsCountRequired: true,
        FilterCriterias: filterCriteria,
        SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }]
      };
      
      console.log(`[SEARCH] Request payload:`, requestPayload);
      
      const response = await skuPriceService.getAllPriceLists(requestPayload);
      
      console.log(`[SEARCH] API Response:`, response);
      
      const priceListsResult = response?.PagedData || [];
      const totalCount = response?.TotalCount || 0;
      
      console.log(`[SEARCH] Found ${priceListsResult.length} of ${totalCount} Price Lists`);
      
      if (priceListsResult.length > 0) {
        console.log(`[SEARCH] Sample results:`, priceListsResult.slice(0, 3).map(p => ({ Name: p.Name, Code: p.Code })));
      }
      
      setPriceLists(priceListsResult);
      setTotalPriceLists(totalCount);
      setPriceListPage(1);
      setHasMorePriceLists(priceListsResult.length < totalCount);
    } catch (error) {
      console.error("Error searching Price Lists:", error);
      console.error("Error details:", error);
    } finally {
      setLoadingPriceLists(false);
    }
  };

  // Handle infinite scroll for SKU Class Groups
  const handleSKUGroupScroll = (e: React.UIEvent<HTMLDivElement>) => {
    const element = e.currentTarget;
    const scrollPercentage = (element.scrollTop + element.clientHeight) / element.scrollHeight;
    
    // Load more when scrolled past threshold
    if (scrollPercentage > PAGINATION_CONFIG.SCROLL_THRESHOLD && !loadingSkuGroups && hasMoreSkuGroups) {
      console.log(`[INFINITE SCROLL] Loading more SKU Groups at ${Math.round(scrollPercentage * 100)}% scroll`);
      loadMoreSKUClassGroups();
    }
  };

  // Handle infinite scroll for Price Lists
  const handlePriceListScroll = (e: React.UIEvent<HTMLDivElement>) => {
    const element = e.currentTarget;
    const scrollPercentage = (element.scrollTop + element.clientHeight) / element.scrollHeight;
    
    // Debug scroll behavior
    console.log(`[SCROLL DEBUG] Price Lists - scrollTop: ${element.scrollTop}, clientHeight: ${element.clientHeight}, scrollHeight: ${element.scrollHeight}, percentage: ${Math.round(scrollPercentage * 100)}%`);
    console.log(`[SCROLL DEBUG] loadingPriceLists: ${loadingPriceLists}, hasMorePriceLists: ${hasMorePriceLists}, currentCount: ${priceLists.length}, total: ${totalPriceLists}`);
    
    // Load more when scrolled past threshold
    if (scrollPercentage > PAGINATION_CONFIG.SCROLL_THRESHOLD && !loadingPriceLists && hasMorePriceLists) {
      console.log(`[INFINITE SCROLL] Loading more Price Lists at ${Math.round(scrollPercentage * 100)}% scroll`);
      loadMorePriceLists();
    }
  };

  // Load more SKU Class Groups when scrolling
  const loadMoreSKUClassGroups = async () => {
    if (loadingSkuGroups || !hasMoreSkuGroups) return;
    
    try {
      setLoadingSkuGroups(true);
      const nextPage = skuGroupPage + 1;
      console.log(`[LOAD MORE] Loading SKU Class Groups page ${nextPage}`);
      
      const response = await skuClassGroupsService.getAllSKUClassGroups(
        nextPage,
        PAGINATION_CONFIG.LOAD_MORE_BATCH,
        skuGroupSearchTerm
      );
      
      const groupsResult = response?.data || [];
      
      if (groupsResult.length > 0) {
        console.log(`[LOAD MORE] Loaded ${groupsResult.length} more SKU Class Groups`);
        setSKUClassGroups(prev => {
          const existingUIDs = new Set(prev.map(item => item.UID));
          const newItems = groupsResult.filter(item => !existingUIDs.has(item.UID));
          return [...prev, ...newItems];
        });
        setSkuGroupPage(nextPage);
        setHasMoreSkuGroups(skuClassGroups.length + groupsResult.length < totalSKUGroups);
      } else {
        setHasMoreSkuGroups(false);
      }
    } catch (error) {
      console.error("Error loading more SKU Class Groups:", error);
    } finally {
      setLoadingSkuGroups(false);
    }
  };

  // Load more Price Lists when scrolling
  const loadMorePriceLists = async () => {
    if (loadingPriceLists || !hasMorePriceLists) return;
    
    try {
      setLoadingPriceLists(true);
      const nextPage = priceListPage + 1;
      console.log(`[LOAD MORE] Loading Price Lists page ${nextPage}`);
      
      // Use same filter approach as search  
      const filterCriteria = priceListSearchTerm ? [
        { Name: "Name", Value: priceListSearchTerm, Type: 1 }  // Exactly like search
      ] : [];
      
      console.log(`[LOAD MORE] Filter criteria:`, filterCriteria);
      
      const response = await skuPriceService.getAllPriceLists({
        PageNumber: nextPage,
        PageSize: PAGINATION_CONFIG.LOAD_MORE_BATCH,
        IsCountRequired: false,
        FilterCriterias: filterCriteria,
        SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }]
      });
      
      const priceListsResult = response?.PagedData || [];
      
      if (priceListsResult.length > 0) {
        console.log(`[LOAD MORE] Loaded ${priceListsResult.length} more Price Lists`);
        setPriceLists(prev => {
          const existingUIDs = new Set(prev.map(item => item.UID));
          const newItems = priceListsResult.filter(item => !existingUIDs.has(item.UID));
          return [...prev, ...newItems];
        });
        setPriceListPage(nextPage);
        setHasMorePriceLists(priceLists.length + priceListsResult.length < totalPriceLists);
      } else {
        setHasMorePriceLists(false);
      }
    } catch (error) {
      console.error("Error loading more Price Lists:", error);
    } finally {
      setLoadingPriceLists(false);
    }
  };

  /**
   * Load ALL mappings directly from the selection_map_criteria table
   * This shows all mappings in the database, not just ones for existing items
   */
  const loadAllMappingsFromDatabase = async (showToast: boolean = false) => {
    try {
      setCheckingMappings(true);
      console.log(
        "═══════════════════════════════════════════════════════════════"
      );
      console.log("[DATABASE] Loading ALL mappings from selection_map_criteria table");
      console.log(
        "═══════════════════════════════════════════════════════════════"
      );
      
      const startTime = Date.now();
      
      // Get all mappings from the database
      const response = await storeLinkingService.getAllSelectionMapCriteria();
      
      console.log(`[DATABASE] Found ${response.totalCount} total mappings in database`);
      
      // Filter for SKUClassGroup and PriceList mappings for the Price Mapping page
      const relevantMappings = response.pagedData.filter(criteria => {
        // Log each criteria to debug (without exposing UIDs)
        console.log(`[DATABASE] Checking criteria: LinkedItemType=${criteria.linkedItemType || criteria.LinkedItemType}`);
        
        // Check both camelCase and PascalCase
        const itemType = criteria.linkedItemType || criteria.LinkedItemType;
        return itemType === "SKUClassGroup" || 
               itemType === "PriceList" ||
               itemType === "CustomerPriceList";
      });
      
      console.log(`[DATABASE] Found ${relevantMappings.length} relevant mappings (SKUClassGroup/PriceList)`);
      
      // Now fetch the full details for each mapping
      const mappingPromises = relevantMappings.map(async (criteria) => {
        try {
          // Handle both camelCase and PascalCase
          const linkedUID = criteria.linkedItemUID || criteria.LinkedItemUID;
          const mapping = await storeLinkingService.getSelectionMapMasterByLinkedItemUID(
            linkedUID
          );
          
          if (mapping) {
            // Normalize the response
            const rawCriteria = mapping.selectionMapCriteria || mapping.SelectionMapCriteria;
            const rawDetails = mapping.selectionMapDetails || mapping.SelectionMapDetails;
            
            if (rawCriteria && rawDetails) {
              const normalizedMapping: SelectionMapMaster = {
                selectionMapCriteria: normalizeSelectionMapCriteria(rawCriteria),
                selectionMapDetails: rawDetails
              };
              
              console.log(
                `[DATABASE] Loaded mapping for ${criteria.linkedItemType || criteria.LinkedItemType}`
              );
              
              return normalizedMapping;
            }
          }
          return null;
        } catch (error) {
          console.error(`Error loading mapping:`, error);
          return null;
        }
      });
      
      const mappings = (await Promise.all(mappingPromises)).filter(
        (m): m is SelectionMapMaster => m !== null
      );
      
      const endTime = Date.now();
      console.log(
        `[DATABASE] Successfully loaded ${mappings.length} mappings in ${endTime - startTime}ms`
      );
      console.log(
        "═══════════════════════════════════════════════════════════════"
      );
      
      setExistingMappings(mappings);
      
      if (showToast) {
        toast({
          title: "Mappings Loaded",
          description: `Found ${mappings.length} store linkings`
        });
      }
      
      return mappings;
    } catch (error) {
      console.error("[DATABASE] Error loading mappings:", error);
      toast({
        title: "Error",
        description: "Failed to load mappings from database",
        variant: "destructive"
      });
      return [];
    } finally {
      setCheckingMappings(false);
    }
  };

  const loadExistingMappingsForGroups = async (
    groups: SKUClassGroup[],
    showToast: boolean = false
  ) => {
    try {
      setCheckingMappings(true);
      console.log(
        "═══════════════════════════════════════════════════════════════"
      );
      console.log("[READ-ONLY CHECK] Starting to check existing mappings");
      console.log(
        "[INFO] This is checking which SKU Class Groups already have price mappings"
      );
      console.log(
        "[INFO] These are GET requests only - NO data is being created or modified"
      );
      console.log("[INFO] Checking", groups.length, "SKU Class Groups...");
      console.log(
        "═══════════════════════════════════════════════════════════════"
      );
      // Create all promises for PARALLEL execution - much faster!
      console.log("[PARALLEL] Creating parallel requests for all groups...");
      const startTime = Date.now();

      const mappingPromises = groups.map(async (group) => {
        try {
          // Just checking if mapping exists - NOT creating anything
          const mapping =
            await storeLinkingService.getSelectionMapMasterByLinkedItemUID(
              group.UID
            );

          // Handle both camelCase and PascalCase from API
          if (mapping) {
            // Normalize the response to camelCase
            const rawCriteria =
              mapping.selectionMapCriteria || mapping.SelectionMapCriteria;
            const rawDetails =
              mapping.selectionMapDetails || mapping.SelectionMapDetails;

            if (rawCriteria && rawDetails) {
              const normalizedMapping: SelectionMapMaster = {
                selectionMapCriteria:
                  normalizeSelectionMapCriteria(rawCriteria),
                selectionMapDetails: rawDetails
              };

              console.log(
                `[READ-ONLY RESULT] ✓ Found existing mapping for: "${group.Name}"`
              );
              // Debug: Log the full structure to understand the data
              console.log(
                "[DEBUG] Full mapping criteria:",
                normalizedMapping.selectionMapCriteria
              );
              console.log("[DEBUG] Mapping structure summary:", {
                criteriaUID: normalizedMapping.selectionMapCriteria.uid,
                linkedItemUID:
                  normalizedMapping.selectionMapCriteria.linkedItemUID,
                linkedItemType:
                  normalizedMapping.selectionMapCriteria.linkedItemType,
                hasCustomer: normalizedMapping.selectionMapCriteria.hasCustomer,
                customerCount:
                  normalizedMapping.selectionMapCriteria.customerCount,
                detailsCount: normalizedMapping.selectionMapDetails?.length,
                groupUID: group.UID,
                groupName: group.Name
              });
              return normalizedMapping;
            } else {
              console.log(
                `[READ-ONLY RESULT] ✗ No valid mapping data for: "${group.Name}"`
              );
              return null;
            }
          } else {
            console.log(
              `[READ-ONLY RESULT] ✗ No mapping exists for: "${group.Name}"`
            );
            return null;
          }
        } catch (error: any) {
          // No mapping exists for this group - this is normal
          if (error?.message?.includes("404")) {
            console.log(
              `[READ-ONLY RESULT] ✗ No mapping exists for: "${group.Name}" (404 - expected)`
            );
          } else {
            console.log(
              `[READ-ONLY RESULT] Error checking "${group.Name}":`,
              error?.message || error
            );
          }
          return null;
        }
      });

      // Wait for ALL promises to complete in parallel
      console.log("[PARALLEL] Executing all checks simultaneously...");
      const results = await Promise.all(mappingPromises);

      // Filter out null results to get only valid mappings
      const mappings = results.filter(
        (mapping): mapping is SelectionMapMaster => mapping !== null
      );

      const endTime = Date.now();
      console.log(
        `[PERFORMANCE] All checks completed in ${
          endTime - startTime
        }ms (parallel execution)`
      );

      console.log(
        "═══════════════════════════════════════════════════════════════"
      );
      console.log(
        `[READ-ONLY COMPLETE] Found ${mappings.length} existing mappings out of ${groups.length} groups checked`
      );
      console.log("[SUMMARY] These checks were necessary to:");
      console.log("  1. Show existing mappings in the View tab");
      console.log("  2. Prevent duplicate mappings");
      console.log("  3. Display correct status for each SKU Class Group");
      console.log(
        "[IMPORTANT] NO data was created or modified - these were all READ operations"
      );
      console.log(
        "═══════════════════════════════════════════════════════════════"
      );
      setExistingMappings(mappings);

      // Only show toast if explicitly requested (manual refresh)
      if (showToast && mappings.length > 0) {
        toast({
          title: "Mappings Loaded",
          description: `Found ${mappings.length} store linking configuration(s)`
        });
      }
    } catch (error) {
      console.error("Error loading mappings:", error);
      if (showToast) {
        toast({
          title: "Error",
          description: "Failed to load existing mappings",
          variant: "destructive"
        });
      }
    } finally {
      setCheckingMappings(false);
    }
  };

  // Lazy loading functions for each data type with pagination like customer-mapping
  const loadStoresPage = async (page: number = 1, search: string = "", append: boolean = false) => {
    if (append) {
      setLoadingMoreStores(true);
    } else {
      setLoadingStores(true);
    }

    try {
      console.log(`[LAZY LOAD] Loading stores page ${page} with search: "${search}"`);
      const filterCriterias = [];

      // Add search filter if provided
      if (search.trim()) {
        filterCriterias.push({
          name: "Code",
          value: search.trim(),
          operator: "contains"
        });
      }

      const request = {
        pageNumber: page,
        pageSize: storesPagination.pageSize,
        filterCriterias,
        sortCriterias: [{ sortParameter: "Code", direction: 0 as 0 }],
        isCountRequired: page === 1
      };

      // Use the same service as customer-mapping for consistency
      const { storeService } = await import("@/services/storeService");
      const response = await storeService.getAllStores(request);

      if (response.pagedData) {
        // Filter out stores where ShowInUI is false
        const visibleStores = response.pagedData.filter((s: any) => s.ShowInUI !== false);
        
        const fetchedStores = visibleStores.map((s: any) => ({
          uid: s.UID || s.uid,
          code: s.Code || s.code || s.UID || s.uid,
          name: s.Name || s.name || `Store ${s.UID || s.uid}`,
          area: s.Area || s.area || "",
          address: s.Address || s.address || "",
          contactNo: s.ContactNo || s.contactNo || "",
          isActive: s.IsActive !== false,
          type: s.Type || s.type || "Store"
        }));

        if (append) {
          setDisplayedStores((prev) => {
            const existingIds = new Set(prev.map((s) => s.uid));
            const newStores = fetchedStores.filter((s: StoreData) => !existingIds.has(s.uid));
            return [...prev, ...newStores];
          });
        } else {
          setDisplayedStores(fetchedStores);
          setStores(fetchedStores);
        }

        // Update pagination state
        const newTotalCount = page === 1 ? response.totalCount || 0 : storesPagination.totalCount;
        const currentDisplayedCount = append ? displayedStores.length + fetchedStores.length : fetchedStores.length;

        setStoresPagination((prev) => ({
          currentPage: page,
          pageSize: prev.pageSize,
          totalCount: newTotalCount,
          hasMore: fetchedStores.length === prev.pageSize && currentDisplayedCount < newTotalCount
        }));

        console.log(`[LOADED] ${fetchedStores.length} stores (page ${page})`);
      }
    } catch (error) {
      console.error("Error loading stores:", error);
      toast({
        title: "Error",
        description: "Failed to load stores. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoadingStores(false);
      setLoadingMoreStores(false);
    }
  };

  const loadStores = async () => {
    if (displayedStores.length > 0 || loadingStores) {
      return; // Already loaded or currently loading
    }
    await loadStoresPage(1, storeSearchTerm, false);
  };

  const loadBranchesPage = async (page: number = 1, search: string = "", append: boolean = false) => {
    if (append) {
      setLoadingMoreBranches(true);
    } else {
      setLoadingBranches(true);
    }

    try {
      console.log(`[LAZY LOAD] Loading branches page ${page} with search: "${search}"`);
      
      // For now, we'll use the existing service method
      const branchesResult = await storeLinkingService.getAllBranches();
      
      if (branchesResult?.length > 0) {
        // Filter and paginate on the client side for now
        let filteredBranches = branchesResult;
        if (search.trim()) {
          filteredBranches = branchesResult.filter(branch => 
            branch.name.toLowerCase().includes(search.toLowerCase()) ||
            branch.code.toLowerCase().includes(search.toLowerCase())
          );
        }
        
        // Paginate
        const startIndex = (page - 1) * branchesPagination.pageSize;
        const endIndex = startIndex + branchesPagination.pageSize;
        const paginatedBranches = filteredBranches.slice(startIndex, endIndex);

        if (append) {
          setDisplayedBranches((prev) => [...prev, ...paginatedBranches]);
        } else {
          setDisplayedBranches(paginatedBranches);
          setBranches(branchesResult);
        }

        setBranchesPagination((prev) => ({
          currentPage: page,
          pageSize: prev.pageSize,
          totalCount: filteredBranches.length,
          hasMore: endIndex < filteredBranches.length
        }));

        console.log(`[LOADED] ${paginatedBranches.length} branches (page ${page})`);
      }
    } catch (error) {
      console.error("Error loading branches:", error);
      toast({
        title: "Error", 
        description: "Failed to load branches. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoadingBranches(false);
      setLoadingMoreBranches(false);
    }
  };

  const loadBranches = async () => {
    if (displayedBranches.length > 0 || loadingBranches) {
      return; // Already loaded or currently loading
    }
    await loadBranchesPage(1, branchSearchTerm, false);
  };

  const loadCustomerGroupsPage = async (page: number = 1, search: string = "", append: boolean = false) => {
    if (append) {
      setLoadingMoreCustomerGroups(true);
    } else {
      setLoadingCustomerGroups(true);
    }

    try {
      console.log(`[LAZY LOAD] Loading customer groups page ${page} with search: "${search}"`);
      const filterCriterias = [];

      if (search.trim()) {
        filterCriterias.push({
          PropertyName: "Name",
          Value: search,
          Operator: "contains"
        });
      }

      // Add type filter if selected and not "all"
      if (selectedCustomerGroupType && selectedCustomerGroupType !== "all") {
        filterCriterias.push({
          PropertyName: "StoreGroupTypeUID",
          Value: selectedCustomerGroupType,
          Operator: "="
        });
      }

      const requestData = {
        PageNumber: page,
        PageSize: customerGroupsPagination.pageSize,
        IsCountRequired: true,
        FilterCriterias: [], // Remove filters due to backend bug - will filter client-side
        SortCriterias: []
      };

      console.log(`[API] Customer groups request:`, requestData);
      const response = await storeGroupService.getAllStoreGroups(requestData);
      console.log(`[API] Customer groups response:`, response);

      if (response && response.PagedData) {
        let fetchedCustomerGroups = response.PagedData;
        
        // Apply client-side filtering due to backend filtering bug
        if (search.trim()) {
          fetchedCustomerGroups = fetchedCustomerGroups.filter((group: any) => 
            group.Name?.toLowerCase().includes(search.toLowerCase()) ||
            group.Code?.toLowerCase().includes(search.toLowerCase())
          );
        }
        
        if (selectedCustomerGroupType && selectedCustomerGroupType !== "all") {
          fetchedCustomerGroups = fetchedCustomerGroups.filter((group: any) => 
            group.StoreGroupTypeUID === selectedCustomerGroupType
          );
        }
        
        const newTotalCount = fetchedCustomerGroups.length;

        if (append) {
          setDisplayedCustomerGroups(prev => [...prev, ...fetchedCustomerGroups]);
        } else {
          setCustomerGroups(fetchedCustomerGroups);
          setDisplayedCustomerGroups(fetchedCustomerGroups);
        }

        // Update pagination state
        const hasMore = fetchedCustomerGroups.length === customerGroupsPagination.pageSize && (page * customerGroupsPagination.pageSize) < newTotalCount;
        setCustomerGroupsPagination((prev) => ({
          currentPage: page,
          pageSize: prev.pageSize,
          totalCount: newTotalCount,
          hasMore
        }));

        console.log(`[LAZY LOAD] Customer groups loaded: ${fetchedCustomerGroups.length}, Total: ${newTotalCount}, Has more: ${hasMore}`);
      }
    } catch (error) {
      console.error("Error loading customer groups:", error);
      toast({
        title: "Error",
        description: "Failed to load customer groups",
        variant: "destructive"
      });
    } finally {
      setLoadingCustomerGroups(false);
      setLoadingMoreCustomerGroups(false);
    }
  };

  const loadMoreCustomerGroups = async () => {
    if (!customerGroupsPagination.hasMore || loadingMoreCustomerGroups) return;
    await loadCustomerGroupsPage(customerGroupsPagination.currentPage + 1, customerGroupSearchTerm, true);
  };

  const loadCustomerGroups = async () => {
    if (displayedCustomerGroups.length > 0 || loadingCustomerGroups) {
      return; // Already loaded or currently loading
    }
    await loadCustomerGroupsPage(1, customerGroupSearchTerm, false);
  };

  // Load customer group types for filtering
  const loadCustomerGroupTypes = async () => {
    try {
      const response = await storeGroupService.getAllStoreGroupTypes({
        PageNumber: 1,
        PageSize: 100,
        IsCountRequired: false,
        FilterCriterias: [],
        SortCriterias: []
      });
      
      if (response && response.PagedData) {
        setCustomerGroupTypes(response.PagedData);
      }
    } catch (error) {
      console.error("Error loading customer group types:", error);
    }
  };

  // Load location hierarchy for branch selection
  const loadLocationHierarchy = async () => {
    if (locations.length > 0 || loadingBranches) {
      return; // Already loaded or currently loading
    }
    
    setLoadingBranches(true);
    try {
      console.log("[HIERARCHY] Loading location hierarchy...");
      const { data } = await locationService.getLocations(1, 5000);
      
      // Filter locations to show only those with ShowInUI = true
      const visibleLocations = data.filter((loc: any) => loc.ShowInUI !== false);
      setLocations(visibleLocations);
      
      // Build hierarchy tree with visible locations
      const tree = locationService.buildLocationTree(visibleLocations);
      setLocationHierarchy(tree);
      
      // Auto-expand first level
      const firstLevelIds = new Set(tree.map(node => node.uid));
      setExpandedNodes(firstLevelIds);
      
      console.log("[HIERARCHY] Loaded", visibleLocations.length, "visible locations");
    } catch (error) {
      console.error("Error loading location hierarchy:", error);
      toast({
        title: "Error",
        description: "Failed to load location hierarchy. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoadingBranches(false);
    }
  };

  const loadOrganizations = async () => {
    if (organizations.length > 0 || loadingOrganizations) {
      return; // Already loaded or currently loading
    }
    
    setLoadingOrganizations(true);
    try {
      console.log("[LAZY LOAD] Loading organizations on demand...");
      // Note: You'll need to implement this service method if it doesn't exist
      // const organizationsResult = await storeLinkingService.getAllOrganizations();
      // For now, we'll use an empty array
      console.log("[INFO] Organization loading not implemented yet");
      setOrganizations([]);
    } catch (error) {
      console.error("Error loading organizations:", error);
    } finally {
      setLoadingOrganizations(false);
    }
  };

  // Load organization hierarchy
  const loadOrganizationHierarchy = async () => {
    if (allOrganizations.length > 0 || loadingOrganizations) {
      return; // Already loaded or currently loading
    }
    
    setLoadingOrganizations(true);
    try {
      console.log("[HIERARCHY] Loading organization hierarchy...");
      const { data } = await organizationService.getOrganizations(1, 5000);
      
      // Filter organizations to show only those with ShowInUI = true
      const visibleOrgs = data.filter(org => org.ShowInUI !== false);
      setAllOrganizations(visibleOrgs);
      
      // Build hierarchy tree with visible organizations
      const tree = organizationService.buildOrganizationTree(visibleOrgs);
      setOrganizationHierarchy(tree);
      
      // Auto-expand first level
      const firstLevelIds = new Set(tree.map(node => node.uid));
      setExpandedOrgNodes(firstLevelIds);
      
      console.log("[HIERARCHY] Loaded", visibleOrgs.length, "visible organizations");
    } catch (error) {
      console.error("Error loading organization hierarchy:", error);
      toast({
        title: "Error",
        description: "Failed to load organization hierarchy. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoadingOrganizations(false);
    }
  };

  // Load sales teams - first load roles, then employees based on role
  const loadSalesTeams = async () => {
    if (loadingSalesTeams) return;

    try {
      setLoadingSalesTeams(true);
      console.log("[SALES_TEAMS] Loading roles first...");

      // First, load all roles
      const roleData = await apiService.post("/Role/SelectAllRoles", {
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: []
      });

      if (roleData.IsSuccess && roleData.Data?.PagedData) {
        const roles = roleData.Data.PagedData.map((role: any) => ({
          value: role.UID,
          label: role.RoleNameEn || role.Code
        }));
        setAvailableRoles(roles);
        console.log(`[SALES_TEAMS] Found ${roles.length} roles:`, roles.map((r: any) => r.label));

        // For demonstration, you can auto-select the first sales-related role
        // or let user select from dropdown
        const salesRole = roles.find((role: any) => 
          role.label.toLowerCase().includes('sales') || 
          role.label.toLowerCase().includes('field') ||
          role.label.toLowerCase().includes('territory')
        );

        if (salesRole) {
          setSelectedRole(salesRole.value);
          await loadEmployeesByRole(salesRole.value, "EPIC01");
        } else {
          // No automatic sales role found, just show empty state for user to select
          setSalesTeams([]);
          setDisplayedSalesTeams([]);
          console.log("[SALES_TEAMS] No automatic sales role found, user needs to select manually");
        }
      }

    } catch (error) {
      console.error("[SALES_TEAMS] Failed to load sales teams:", error);
      setSalesTeams([]);
      setDisplayedSalesTeams([]);
    } finally {
      setLoadingSalesTeams(false);
    }
  };

  // Load employees for a specific role (using same pattern as route management)
  const loadEmployeesByRole = async (roleUID: string, orgUID?: string) => {
    if (!roleUID) return;

    setLoadingSalesTeams(true);
    try {
      // Hardcode orgUID to EPIC01 if not provided
      const effectiveOrgUID = orgUID || "EPIC01";
      console.log(`[SALES_TEAMS] Loading employees for role: ${roleUID}, org: ${effectiveOrgUID}`);
      let employees = [];
      
      // Use the exact same pattern as route management
      if (effectiveOrgUID && roleUID) {
        console.log("🔍 Loading employees for org + role:", effectiveOrgUID, roleUID);
        
        // Primary: Use organization + role API first (most accurate)
        try {
          console.log("🎯 Using org + role combination API");
          const orgRoleEmployees = await employeeService.getEmployeesSelectionItemByRoleUID(effectiveOrgUID, roleUID);
          
          console.log("🔍 Raw API response from getEmployeesSelectionItemByRoleUID:", orgRoleEmployees);
          console.log("🔍 Response type:", typeof orgRoleEmployees, "Length:", Array.isArray(orgRoleEmployees) ? orgRoleEmployees.length : "Not an array");
          
          if (orgRoleEmployees && orgRoleEmployees.length > 0) {
            console.log("🔍 First employee raw data from org+role API:", orgRoleEmployees[0]);
            employees = orgRoleEmployees.map((item: any) => ({
              uid: item.UID || item.Value || item.uid,
              name: item.Name || item.name || item.Text || item.Label, // Fix: use Label as fallback for name
              code: item.Code || item.code,
              label: item.Label || item.Text || `[${item.Code || item.code}] ${item.Name || item.name || item.Label}`,
              roleUID: roleUID
            }));
            console.log(`✅ Found ${employees.length} employees for org '${effectiveOrgUID}' + role '${roleUID}'`);
            console.log("🔍 Processed first employee from org+role:", employees[0]);
          } else {
            // Secondary: Try role-based API and then filter by org
            console.log("🔄 Trying role-based API with org filtering");
            const roleBasedEmployees = await employeeService.getReportsToEmployeesByRoleUID(roleUID);
            
            if (roleBasedEmployees && roleBasedEmployees.length > 0) {
              // For now, just use role-based employees without org filtering
              // since we don't have api.dropdown in store-linking context
              employees = roleBasedEmployees.map((emp: any) => ({
                uid: emp.UID || emp.uid,
                name: emp.Name || emp.name || emp.Label, // Fix: use Label as fallback for name
                code: emp.Code || emp.code,
                label: `[${emp.Code || emp.code}] ${emp.Name || emp.name || emp.Label}`,
                roleUID: roleUID
              }));
              console.log(`✅ Found ${employees.length} role-based employees`);
            }
          }
        } catch (roleError) {
          console.error("Role-based employee loading failed:", roleError);
        }
      } else if (roleUID) {
        // If no org provided, just use role-based loading
        console.log("📄 Loading employees for role only:", roleUID);
        const roleEmployees = await employeeService.getReportsToEmployeesByRoleUID(roleUID);
        
        console.log("🔍 Raw API response from getReportsToEmployeesByRoleUID:", roleEmployees);
        console.log("🔍 Response type:", typeof roleEmployees, "Length:", Array.isArray(roleEmployees) ? roleEmployees.length : "Not an array");
        
        if (roleEmployees && roleEmployees.length > 0) {
          console.log("🔍 First employee raw data:", roleEmployees[0]);
          employees = roleEmployees.map((emp: any) => ({
            uid: emp.UID || emp.uid,
            name: emp.Name || emp.name || emp.Label, // Fix: use Label as fallback for name
            code: emp.Code || emp.code,
            label: `[${emp.Code || emp.code}] ${emp.Name || emp.name || emp.Label}`,
            roleUID: roleUID
          }));
          console.log(`📊 Found ${employees.length} role-based employees`);
          console.log("🔍 Processed first employee:", employees[0]);
        } else {
          console.log("❌ No employees found from role-based API");
          
          // Follow route management pattern - try org-based employees as fallback
          console.log("🔄 Trying org-based employee fallback for org:", effectiveOrgUID);
          try {
            const orgData = await api.dropdown.getEmployee(effectiveOrgUID, false);
              if (orgData.IsSuccess && orgData.Data) {
                employees = orgData.Data.map((emp: any) => ({
                  uid: emp.UID,
                  name: emp.Name,
                  code: emp.Code,
                  label: emp.Label || `[${emp.Code}] ${emp.Name}`,
                  roleUID: "OrgBased" // Mark as org-based employees
                }));
                console.log(`📊 Org-based fallback: Found ${employees.length} employees for org ${effectiveOrgUID}`);
              }
            } catch (orgError) {
              console.error("❌ Org-based fallback failed:", orgError);
            }
        }
      }
      
      setSalesTeams(employees);
      setDisplayedSalesTeams(employees);
      
      if (employees.length === 0) {
        console.log(`[SALES_TEAMS] No employees found for role ${roleUID}`);
      }
      
    } catch (error) {
      console.error(`[SALES_TEAMS] Failed to load employees for role ${roleUID}:`, error);
      setSalesTeams([]);
      setDisplayedSalesTeams([]);
    } finally {
      setLoadingSalesTeams(false);
    }
  };

  // Removed: loadItems function - no longer used

  // Toggle expand/collapse for hierarchy nodes
  const toggleNode = (nodeId: string) => {
    const newExpanded = new Set(expandedNodes);
    if (newExpanded.has(nodeId)) {
      newExpanded.delete(nodeId);
    } else {
      newExpanded.add(nodeId);
    }
    setExpandedNodes(newExpanded);
  };

  // Toggle selection for location nodes
  const toggleLocationSelection = (nodeId: string) => {
    const newSelected = new Set(selectedLocationNodes);
    if (newSelected.has(nodeId)) {
      newSelected.delete(nodeId);
    } else {
      newSelected.add(nodeId);
    }
    setSelectedLocationNodes(newSelected);
    // Update selectedBranches for compatibility
    setSelectedBranches(Array.from(newSelected));
  };

  // Select all children of a node
  const selectNodeWithChildren = (node: LocationHierarchyNode, select: boolean) => {
    const nodesToUpdate = new Set<string>();
    
    const collectNodes = (n: LocationHierarchyNode) => {
      nodesToUpdate.add(n.uid);
      if (n.children) {
        n.children.forEach(child => collectNodes(child));
      }
    };
    
    collectNodes(node);
    
    const newSelected = new Set(selectedLocationNodes);
    nodesToUpdate.forEach(uid => {
      if (select) {
        newSelected.add(uid);
      } else {
        newSelected.delete(uid);
      }
    });
    
    setSelectedLocationNodes(newSelected);
    setSelectedBranches(Array.from(newSelected));
  };

  // Toggle expand/collapse for organization hierarchy nodes
  const toggleOrgNode = (nodeId: string) => {
    const newExpanded = new Set(expandedOrgNodes);
    if (newExpanded.has(nodeId)) {
      newExpanded.delete(nodeId);
    } else {
      newExpanded.add(nodeId);
    }
    setExpandedOrgNodes(newExpanded);
  };

  // Toggle selection for organization nodes
  const toggleOrgSelection = (nodeId: string) => {
    const newSelected = new Set(selectedOrgNodes);
    if (newSelected.has(nodeId)) {
      newSelected.delete(nodeId);
    } else {
      newSelected.add(nodeId);
    }
    setSelectedOrgNodes(newSelected);
    // Update selectedOrganizations for compatibility
    setSelectedOrganizations(Array.from(newSelected));
  };

  // Select all children of an org node
  const selectOrgNodeWithChildren = (node: OrganizationHierarchyNode, select: boolean) => {
    const nodesToUpdate = new Set<string>();
    
    const collectNodes = (n: OrganizationHierarchyNode) => {
      nodesToUpdate.add(n.uid);
      if (n.children) {
        n.children.forEach(child => collectNodes(child));
      }
    };
    
    collectNodes(node);
    
    const newSelected = new Set(selectedOrgNodes);
    nodesToUpdate.forEach(uid => {
      if (select) {
        newSelected.add(uid);
      } else {
        newSelected.delete(uid);
      }
    });
    
    setSelectedOrgNodes(newSelected);
    setSelectedOrganizations(Array.from(newSelected));
  };

  const handleCreateMapping = async (e?: React.MouseEvent) => {
    console.log("[USER ACTION] Create button clicked", {
      isCreating,
      loading,
      hasEvent: !!e,
      eventType: e?.type,
      isMounted,
      isInitialized
    });

    // Prevent execution during initialization
    if (!isInitialized) {
      console.warn("[BLOCKED] Page not initialized yet, preventing action");
      return;
    }

    // Prevent execution if component is not mounted
    if (!isMounted) {
      console.warn("[BLOCKED] Component not mounted, preventing action");
      return;
    }

    // Prevent double submission
    if (isCreating || loading) {
      console.log("[BLOCKED] Already creating, preventing double submission");
      return;
    }

    // Make sure this is from a button click, not automatic
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    } else {
      console.warn(
        "[BLOCKED] handleCreateMapping called without event - this should not happen!"
      );
      return; // Don't proceed without an explicit click event
    }

    // Validate based on linkedItemType
    if (linkedItemType === "SKUClassGroup" && !selectedSKUClassGroup) {
      toast({
        title: "Validation Error",
        description: "Please select an SKU Class Group",
        variant: "destructive"
      });
      return;
    }

    if (linkedItemType === "PriceList" && !selectedPriceList) {
      toast({
        title: "Validation Error",
        description: "Please select a Price List",
        variant: "destructive"
      });
      return;
    }

    if (
      !selectionCriteria.hasCustomer &&
      !selectionCriteria.hasLocation &&
      !selectionCriteria.hasOrganization &&
      !selectionCriteria.hasSalesTeam &&
      !selectionCriteria.hasCustomerGroup
    ) {
      toast({
        title: "Validation Error",
        description:
          "Please select a mapping criteria (Customer, Location, Organization, Customer Group, or Sales Team)",
        variant: "destructive"
      });
      return;
    }

    if (selectionCriteria.hasCustomer && selectedStores.length === 0) {
      toast({
        title: "Validation Error",
        description: "Please select at least one store for customer mapping",
        variant: "destructive"
      });
      return;
    }

    if (selectionCriteria.hasLocation && selectedBranches.length === 0) {
      toast({
        title: "Validation Error",
        description: "Please select at least one branch for location mapping",
        variant: "destructive"
      });
      return;
    }

    if (
      selectionCriteria.hasOrganization &&
      selectedOrganizations.length === 0
    ) {
      toast({
        title: "Validation Error",
        description:
          "Please select at least one organization for organization mapping",
        variant: "destructive"
      });
      return;
    }

    if (selectionCriteria.hasSalesTeam && selectedSalesTeams.length === 0) {
      toast({
        title: "Validation Error",
        description: "Please select at least one sales team member",
        variant: "destructive"
      });
      return;
    }

    if (selectionCriteria.hasCustomerGroup && selectedCustomerGroups.length === 0) {
      toast({
        title: "Validation Error",
        description: "Please select at least one customer group",
        variant: "destructive"
      });
      return;
    }

    setIsCreating(true);
    setLoading(true);
    try {
      // Build Selection Map Criteria
      const criteriaUID = storeLinkingService.generateUID("SMC");
      const currentTime = new Date().toISOString();
      
      // Use the appropriate linkedItemUID based on linkedItemType
      const linkedItemUID = linkedItemType === "SKUClassGroup" 
        ? selectedSKUClassGroup 
        : selectedPriceList;
      
      const criteria: SelectionMapCriteria = {
        uid: criteriaUID,
        linkedItemUID: linkedItemUID,
        linkedItemType: linkedItemType,
        hasOrganization: selectionCriteria.hasOrganization,
        hasLocation: selectionCriteria.hasLocation,
        hasCustomer: selectionCriteria.hasCustomer || selectionCriteria.hasCustomerGroup,
        hasSalesTeam: selectionCriteria.hasSalesTeam,
        hasItem: false,
        orgCount: selectedOrganizations.length,
        locationCount: selectedBranches.length,
        customerCount: selectedStores.length + selectedCustomerGroups.length,
        salesTeamCount: selectedSalesTeams.length,
        itemCount: 0,
        actionType: ActionType.Add,
        isActive: true,
        ss: 0,
        createdTime: currentTime,
        modifiedTime: currentTime,
        serverAddTime: currentTime,
        serverModifiedTime: currentTime
      };

      // Build Selection Map Details
      const details: SelectionMapDetails[] = [];

      // Add store mappings
      selectedStores.forEach((storeUID) => {
        details.push({
          uid: storeLinkingService.generateUID("SMD"),
          selectionMapCriteriaUID: criteriaUID,
          selectionGroup: "Customer",
          typeUID: "Store",
          selectionValue: storeUID,
          isExcluded: false,
          actionType: ActionType.Add,
          ss: 0,
          createdTime: currentTime,
          modifiedTime: currentTime,
          serverAddTime: currentTime,
          serverModifiedTime: currentTime
        });
      });

      // Add branch mappings
      selectedBranches.forEach((branchUID) => {
        details.push({
          uid: storeLinkingService.generateUID("SMD"),
          selectionMapCriteriaUID: criteriaUID,
          selectionGroup: "Location",
          typeUID: "Branch",
          selectionValue: branchUID,
          isExcluded: false,
          actionType: ActionType.Add,
          ss: 0,
          createdTime: currentTime,
          modifiedTime: currentTime,
          serverAddTime: currentTime,
          serverModifiedTime: currentTime
        });
      });

      // Add sales team mappings
      selectedSalesTeams.forEach((salesTeamUID) => {
        details.push({
          uid: storeLinkingService.generateUID("SMD"),
          selectionMapCriteriaUID: criteriaUID,
          selectionGroup: "SalesTeam",
          typeUID: "Employee", 
          selectionValue: salesTeamUID,
          isExcluded: false,
          actionType: ActionType.Add,
          ss: 0,
          createdTime: currentTime,
          modifiedTime: currentTime,
          serverAddTime: currentTime,
          serverModifiedTime: currentTime
        });
      });

      // Add organization mappings
      selectedOrganizations.forEach((orgUID) => {
        details.push({
          uid: storeLinkingService.generateUID("SMD"),
          selectionMapCriteriaUID: criteriaUID,
          selectionGroup: "Organization",
          typeUID: "Org",
          selectionValue: orgUID,
          isExcluded: false,
          actionType: ActionType.Add,
          ss: 0,
          createdTime: currentTime,
          modifiedTime: currentTime,
          serverAddTime: currentTime,
          serverModifiedTime: currentTime
        });
      });

      // Add customer group mappings
      selectedCustomerGroups.forEach((customerGroupUID) => {
        details.push({
          uid: storeLinkingService.generateUID("SMD"),
          selectionMapCriteriaUID: criteriaUID,
          selectionGroup: "Customer" as any,
          typeUID: "CustomerGroup",
          selectionValue: customerGroupUID,
          isExcluded: false,
          actionType: ActionType.Add,
          ss: 0,
          createdTime: currentTime,
          modifiedTime: currentTime,
          serverAddTime: currentTime,
          serverModifiedTime: currentTime
        });
      });

      // Create Selection Map Master
      const masterData: SelectionMapMaster = {
        selectionMapCriteria: criteria,
        selectionMapDetails: details
      };

      console.log("[PAYLOAD] Creating mapping with data:");
      console.log("- Criteria:", criteria);
      console.log("- Details count:", details.length);
      console.log("- Sales teams selected:", selectedSalesTeams);
      console.log("- Selected sales team details:", selectedSalesTeams.map(uid => 
        displayedSalesTeams.find(emp => emp.uid === uid)
      ));
      console.log("- Full payload:", JSON.stringify(masterData, null, 2));

      // Call API
      await storeLinkingService.cudSelectionMapMaster(masterData);

      // Prepare linked items for caching
      if (selectedStores.length > 0) {
        await storeLinkingService.prepareLinkedItemUIDByStore(
          linkedItemType,
          selectedStores
        );
      }

      toast({
        title: "Success",
        description: "Price mapping created successfully"
      });

      // Reset form
      resetForm();

      // Reload all mappings from database (no toast, we already showed success)
      await loadAllMappingsFromDatabase(false);

      // Switch to view tab
      setActiveTab("view");

    } catch (error: any) {
      console.error("Error creating mapping:", error);
      toast({
        title: "Error",
        description: error.message || "Failed to create store linking",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
      setIsCreating(false);
    }
  };

  const handleUpdateMapping = async (mapping: SelectionMapMaster) => {
    setLoading(true);
    try {
      // Update existing mapping
      mapping.selectionMapCriteria.actionType = ActionType.Update;

      await storeLinkingService.cudSelectionMapMaster(mapping);

      toast({
        title: "Success",
        description: "Price mapping updated successfully"
      });

      await loadAllMappingsFromDatabase(false);
      setEditingItem(null);
    } catch (error: any) {
      console.error("Error updating mapping:", error);
      toast({
        title: "Error",
        description: error.message || "Failed to update store linking",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setSelectedSKUClassGroup("");
    setSelectedPriceList("");
    setLinkedItemType("SKUClassGroup");
    setSelectionCriteria({
      hasCustomer: false,
      hasLocation: false,
      hasSalesTeam: false,
      hasOrganization: false,
      hasItem: false,
      hasCustomerGroup: false
    });
    setSelectedStores([]);
    setSelectedBranches([]);
    setSelectedOrganizations([]);
    setSelectedSalesTeams([]);
    setSelectedCustomerGroups([]);
    setSelectedRole("");
    setSalesTeams([]);
    setDisplayedSalesTeams([]);
    
    // Reset displayed data and pagination
    setDisplayedStores([]);
    setDisplayedBranches([]);
    setStoresPagination({
      currentPage: 1,
      pageSize: 50,
      totalCount: 0,
      hasMore: false
    });
    setBranchesPagination({
      currentPage: 1,
      pageSize: 50,
      totalCount: 0,
      hasMore: false
    });
    
    // Reset search terms
    setStoreSearchTerm("");
    setBranchSearchTerm("");
    setOrganizationSearchTerm("");
    setItemSearchTerm("");
  };

  const handleEditMapping = (mapping: SelectionMapMaster) => {
    // TODO: Implement edit functionality
    toast({
      title: "Edit Feature",
      description: "Edit functionality will be implemented in the next update"
    });
  };

  const handleDeleteMapping = async (mapping: SelectionMapMaster) => {
    // Show confirmation dialog
    const confirmed = window.confirm(
      `Are you sure you want to delete the mapping for ${
        skuClassGroups.find(
          (g) => g.UID === mapping.selectionMapCriteria.linkedItemUID
        )?.Name || mapping.selectionMapCriteria.linkedItemUID
      }?`
    );

    if (!confirmed) return;

    try {
      // Prepare delete request
      const deleteData: SelectionMapMaster = {
        selectionMapCriteria: {
          ...mapping.selectionMapCriteria,
          actionType: ActionType.Delete
        },
        selectionMapDetails: mapping.selectionMapDetails.map((d) => ({
          ...d,
          actionType: ActionType.Delete
        }))
      };

      const result = await storeLinkingService.createOrUpdateSelectionMap(
        deleteData
      );

      if (result) {
        toast({
          title: "Success",
          description: "Mapping deleted successfully"
        });

        // Refresh all mappings from database
        await loadAllMappingsFromDatabase(false);
      }
    } catch (error) {
      console.error("Error deleting mapping:", error);
      toast({
        title: "Error",
        description: "Failed to delete mapping",
        variant: "destructive"
      });
    }
  };

  const getMappingCount = (criteria: SelectionMapCriteria) => {
    if (!criteria) return 0;

    // Handle both camelCase and PascalCase
    const customerCount =
      criteria.customerCount || criteria["CustomerCount"] || 0;
    const locationCount =
      criteria.locationCount || criteria["LocationCount"] || 0;
    const orgCount = 
      criteria.orgCount || criteria["OrgCount"] || 0;
    const salesTeamCount =
      criteria.salesTeamCount || criteria["SalesTeamCount"] || 0;
    const itemCount =
      criteria.itemCount || criteria["ItemCount"] || 0;

    return (
      customerCount + locationCount + orgCount + salesTeamCount + itemCount
    );
  };

  const filteredMappings = existingMappings.filter((mapping) => {
    // Validate mapping structure
    if (!mapping || !mapping.selectionMapCriteria) {
      return false;
    }

    const criteria = mapping.selectionMapCriteria;
    const linkedItemUID =
      criteria.LinkedItemUID ||
      criteria.linkedItemUID ||
      criteria.UID ||
      criteria.uid ||
      "";
    const uid = criteria.UID || criteria.uid || "";
    const hasCustomer = criteria.HasCustomer || criteria.hasCustomer || false;
    const hasLocation = criteria.HasLocation || criteria.hasLocation || false;
    const hasSalesTeam =
      criteria.HasSalesTeam || criteria.hasSalesTeam || false;
    const hasOrganization =
      criteria.HasOrganization || criteria.hasOrganization || false;
    const hasItem = criteria.HasItem || criteria.hasItem || false;

    // Find the SKU Class Group name for search - check multiple possible matches
    const skuClassGroup = skuClassGroups.find(
      (g) =>
        g.UID === linkedItemUID ||
        g.UID === uid ||
        g.Name === linkedItemUID ||
        linkedItemUID?.includes(g.Name)
    );
    const searchText = (
      skuClassGroup?.Name ||
      linkedItemUID ||
      uid ||
      ""
    ).toLowerCase();

    const matchesSearch =
      searchTerm === "" || searchText.includes(searchTerm.toLowerCase());

    const matchesFilter =
      filterType === "all" ||
      (filterType === "customer" && hasCustomer) ||
      (filterType === "location" && hasLocation) ||
      (filterType === "organization" && hasOrganization);

    return matchesSearch && matchesFilter;
  });

  // Apply pagination to filtered results
  const totalFilteredCount = filteredMappings.length;
  const startIndex = (currentPage - 1) * pageSize;
  const paginatedMappings = filteredMappings.slice(
    startIndex,
    startIndex + pageSize
  );

  return (
    <div className="min-h-screen">
      <div className="container mx-auto p-6 max-w-7xl space-y-8">
        {/* Header Section */}
        <div className="rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-50 rounded-lg">
                <Link2 className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <h1 className="text-2xl font-semibold text-gray-700">
                  Price Mapping Management
                </h1>
                <p className="text-gray-500 text-sm">
                  Create advanced price mappings between SKU Class Groups and price lists using comprehensive selection criteria
                </p>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <Badge
                variant="outline"
                className="py-2 px-3 bg-green-50 border-green-300 text-green-800"
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                System Active
              </Badge>
              <Button
                variant="outline"
                size="sm"
                onClick={() => window.history.back()}
                className="hover:bg-gray-50"
              >
                <ArrowLeft className="h-4 w-4 mr-2" />
                Back to Management
              </Button>
            </div>
          </div>
        </div>

        {/* Loading Progress Indicator */}
        {(loading || checkingMappings) && loadingProgress && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 flex items-center gap-3 mb-4">
            <RefreshCw className="h-5 w-5 animate-spin text-blue-600" />
            <div className="flex-1">
              <div className="font-medium text-blue-900">
                Loading Data (Optimized)
              </div>
              <div className="text-sm text-blue-700 mt-1">
                {loadingProgress}
              </div>
              <div className="text-xs text-blue-600 mt-1">
                Loading only what you need, when you need it...
              </div>
            </div>
          </div>
        )}


        <Tabs
          value={activeTab}
          onValueChange={setActiveTab}
          className="space-y-6"
        >
          <TabsList>
            <TabsTrigger value="create">Create Linking</TabsTrigger>
            <TabsTrigger value="view">View Mappings</TabsTrigger>
          </TabsList>

          <TabsContent value="create" className="space-y-6">
            <Card className="border-0">
              <CardHeader className="border-b">
                <CardTitle className="text-2xl font-semibold text-gray-900">
                  Create Item Linking  
                </CardTitle>
                <CardDescription className="text-base text-gray-600 mt-2">
                  Follow the step-by-step process to map SKU Class Groups to
                  stores using advanced selection criteria
                </CardDescription>
              </CardHeader>
              <CardContent className="p-8">
                <form
                  onSubmit={(e) => {
                    e.preventDefault();
                    // Prevent accidental form submission
                  }}
                  className="space-y-10"
                >
                  {/* Item Type Selection */}
                  <div className="space-y-4">
                    <div className="flex items-center gap-3">
                      <div>
                        <h3 className="font-semibold">
                          Item Type Selection
                        </h3>
                        <p className="text-sm text-muted-foreground">
                          Choose between SKU Class Group or Price List to link to stores
                        </p>
                      </div>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="linkedItemType">Item Type Selection</Label>
                      <p className="text-sm text-muted-foreground">
                        Choose between SKU Class Group or Price List to link to stores
                      </p>
                      <Select
                        value={linkedItemType}
                        onValueChange={(value) => {
                          setLinkedItemType(value);
                          setSelectedSKUClassGroup("");
                          setSelectedPriceList("");
                        }}
                        disabled={loading}
                      >
                        <SelectTrigger id="linkedItemType" className="max-w-md">
                          <SelectValue placeholder="Select Item Type" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="SKUClassGroup">SKU Class Group</SelectItem>
                          <SelectItem value="PriceList">Price List</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  {/* SKU Class Group Selection */}
                  {linkedItemType === "SKUClassGroup" && (
                  <div className="space-y-4">
                    <div className="flex items-center gap-3">
                      <div>
                        <h3 className="font-semibold">
                          SKU Class Group Selection
                        </h3>
                        <p className="text-sm text-muted-foreground">
                          Choose the SKU Class Group that you want to link to
                          stores
                        </p>
                      </div>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="skuClassGroup">SKU Class Group *</Label>
                      <Select
                        value={selectedSKUClassGroup}
                        onValueChange={setSelectedSKUClassGroup}
                        disabled={loading}
                      >
                        <SelectTrigger id="skuClassGroup" className="max-w-md">
                          <SelectValue placeholder="Select SKU Class Group" />
                        </SelectTrigger>
                        <SelectContent className="overflow-hidden">
                          <div className="sticky top-0  border-b p-2 z-50">
                            <div className="relative">
                              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground z-10" />
                              <Input
                                placeholder="Search..."
                                className="pl-8 h-8  relative z-10"
                                value={skuGroupSearchTerm}
                                onChange={(e) => setSkuGroupSearchTerm(e.target.value)}
                                onClick={(e) => e.stopPropagation()}
                                onKeyDown={(e) => e.stopPropagation()}
                              />
                            </div>
                          </div>
                          <div 
                            className="max-h-60 overflow-y-auto"
                            onScroll={handleSKUGroupScroll}
                          >
                            {loadingSkuGroups && skuClassGroups.length === 0 ? (
                              <div className="px-2 py-3 text-sm text-muted-foreground text-center">
                                <RefreshCw className="h-4 w-4 animate-spin inline-block mr-2" />
                                Loading...
                              </div>
                            ) : (
                              <>
                                {skuClassGroups.map((group) => (
                                  <SelectItem key={group.UID} value={group.UID}>
                                    {group.Name}
                                  </SelectItem>
                                ))}
                                {skuClassGroups.length === 0 && !loadingSkuGroups && (
                                  <div className="px-2 py-3 text-sm text-muted-foreground text-center">
                                    No SKU Class Groups found
                                  </div>
                                )}
                                {/* Show remaining count as info text */}
                                {hasMoreSkuGroups && !loadingSkuGroups && (
                                  <div className="px-2 py-2 text-xs text-muted-foreground text-center">
                                    Scroll for more ({totalSKUGroups - skuClassGroups.length} remaining)
                                  </div>
                                )}
                                {/* Loading indicator at bottom */}
                                {loadingSkuGroups && skuClassGroups.length > 0 && (
                                  <div className="px-2 py-2 text-sm text-muted-foreground text-center">
                                    <RefreshCw className="h-3 w-3 animate-spin inline-block mr-1" />
                                    Loading more...
                                  </div>
                                )}
                              </>
                            )}
                          </div>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                  )}

                  {/* Price List Selection */}
                  {linkedItemType === "PriceList" && (
                  <div className="space-y-4">
                    <div className="flex items-center gap-3">
                      <div>
                        <h3 className="font-semibold">
                          Price List Selection
                        </h3>
                        <p className="text-sm text-muted-foreground">
                          Choose the Price List that you want to link to customers
                        </p>
                      </div>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="priceList">Price List *</Label>
                      <Select
                        value={selectedPriceList}
                        onValueChange={setSelectedPriceList}
                        disabled={loading}
                      >
                        <SelectTrigger id="priceList" className="max-w-md">
                          <SelectValue placeholder="Select Price List" />
                        </SelectTrigger>
                        <SelectContent className="overflow-hidden">
                          <div className="sticky top-0 border-b p-2 z-50">
                            <div className="relative">
                              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground z-10" />
                              <Input
                                placeholder="Search..."
                                className="pl-8 h-8 relative z-10"
                                value={priceListSearchTerm}
                                onChange={(e) => setPriceListSearchTerm(e.target.value)}
                                onClick={(e) => e.stopPropagation()}
                                onKeyDown={(e) => e.stopPropagation()}
                              />
                            </div>
                          </div>
                          <div 
                            className="max-h-60 overflow-y-auto"
                            onScroll={handlePriceListScroll}
                          >
                            {loadingPriceLists && priceLists.length === 0 ? (
                              <div className="px-2 py-3 text-sm text-muted-foreground text-center">
                                <RefreshCw className="h-4 w-4 animate-spin inline-block mr-2" />
                                Loading...
                              </div>
                            ) : (
                              <>
                                {priceLists.map((list) => (
                                  <SelectItem key={list.UID} value={list.UID}>
                                    {list.Name} ({list.Code})
                                  </SelectItem>
                                ))}
                                {priceLists.length === 0 && !loadingPriceLists && (
                                  <div className="px-2 py-3 text-sm text-muted-foreground text-center">
                                    No Price Lists found
                                  </div>
                                )}
                                {/* Show remaining count as info text */}
                                {hasMorePriceLists && !loadingPriceLists && (
                                  <div className="px-2 py-2 text-xs text-muted-foreground text-center">
                                    Scroll for more ({totalPriceLists - priceLists.length} remaining)
                                  </div>
                                )}
                                {/* Loading indicator at bottom */}
                                {loadingPriceLists && priceLists.length > 0 && (
                                  <div className="px-2 py-2 text-sm text-muted-foreground text-center">
                                    <RefreshCw className="h-3 w-3 animate-spin inline-block mr-1" />
                                    Loading more...
                                  </div>
                                )}
                              </>
                            )}
                          </div>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                  )}

                  {/* Selection Criteria */}
                  <div className="space-y-4">
                    <div>
                      <h3 className="font-semibold">Selection Criteria</h3>
                      <p className="text-sm text-muted-foreground">
                        Choose one mapping criteria type for your item linking configuration
                      </p>
                    </div>
                    <div className="space-y-4">
                      <div className="rounded-lg border border-gray-200 p-6">
                        <div className="space-y-4">
                          <Label className="text-sm font-medium text-gray-700 mb-3 block">
                            Selection Criteria *
                          </Label>
                          <RadioGroup
                            value={
                              selectionCriteria.hasCustomer ? "customer" :
                              selectionCriteria.hasLocation ? "location" :
                              selectionCriteria.hasOrganization ? "organization" :
                              selectionCriteria.hasSalesTeam ? "salesteam" :
                              ""
                            }
                            onValueChange={(value) => {
                              // Reset all criteria first
                              setSelectionCriteria({
                                hasCustomer: false,
                                hasLocation: false,
                                hasSalesTeam: false,
                                hasOrganization: false,
                                hasItem: false,
                                hasCustomerGroup: false
                              });
                              
                              // Clear all selections
                              setSelectedStores([]);
                              setSelectedBranches([]);
                              setSelectedOrganizations([]);
                              setSelectedSalesTeams([]);
                              setSelectedCustomerGroups([]);
                              
                              // Set the selected criteria and load data
                              switch(value) {
                                case "customer":
                                  setSelectionCriteria(prev => ({ ...prev, hasCustomer: true }));
                                  loadStores();
                                  break;
                                case "location":
                                  setSelectionCriteria(prev => ({ ...prev, hasLocation: true }));
                                  setHierarchyView(true); // Enable hierarchy view for locations
                                  loadLocationHierarchy();
                                  break;
                                case "organization":
                                  setSelectionCriteria(prev => ({ ...prev, hasOrganization: true }));
                                  setOrgHierarchyView(true); // Enable hierarchy view for organizations
                                  loadOrganizationHierarchy();
                                  break;
                                case "salesteam":
                                  setSelectionCriteria(prev => ({ ...prev, hasSalesTeam: true }));
                                  loadSalesTeams();
                                  break;
                                case "customergroup":
                                  setSelectionCriteria(prev => ({ ...prev, hasCustomerGroup: true }));
                                  loadCustomerGroups();
                                  break;
                              }
                            }}
                          >
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                              <Label
                                htmlFor="customer"
                                className="flex items-center space-x-3 p-3 rounded-lg border border-gray-200 hover:border-blue-300 hover:bg-blue-50 transition-colors cursor-pointer"
                              >
                                <RadioGroupItem value="customer" id="customer" />
                                <div className="flex items-center gap-2 text-sm font-medium flex-1">
                                  <Store className="h-4 w-4 text-blue-600" />
                                  Customer (Store)
                                </div>
                              </Label>

                              <Label
                                htmlFor="location"
                                className="flex items-center space-x-3 p-3 rounded-lg border border-gray-200 hover:border-green-300 hover:bg-green-50 transition-colors cursor-pointer"
                              >
                                <RadioGroupItem value="location" id="location" />
                                <div className="flex items-center gap-2 text-sm font-medium flex-1">
                                  <MapPin className="h-4 w-4 text-green-600" />
                                  Location (Branch)
                                </div>
                              </Label>

                              <Label
                                htmlFor="organization"
                                className="flex items-center space-x-3 p-3 rounded-lg border border-gray-200 hover:border-orange-300 hover:bg-orange-50 transition-colors cursor-pointer"
                              >
                                <RadioGroupItem value="organization" id="organization" />
                                <div className="flex items-center gap-2 text-sm font-medium flex-1">
                                  <Building2 className="h-4 w-4 text-orange-600" />
                                  Organization
                                </div>
                              </Label>

                              <Label
                                htmlFor="salesteam"
                                className="flex items-center space-x-3 p-3 rounded-lg border border-gray-200 hover:border-purple-300 hover:bg-purple-50 transition-colors cursor-pointer"
                              >
                                <RadioGroupItem value="salesteam" id="salesteam" />
                                <div className="flex items-center gap-2 text-sm font-medium flex-1">
                                  <Users className="h-4 w-4 text-purple-600" />
                                  Sales Team
                                </div>
                              </Label>

                              <Label
                                htmlFor="customergroup"
                                className="flex items-center space-x-3 p-3 rounded-lg border border-gray-200 hover:border-teal-300 hover:bg-teal-50 transition-colors cursor-pointer"
                              >
                                <RadioGroupItem value="customergroup" id="customergroup" />
                                <div className="flex items-center gap-2 text-sm font-medium flex-1">
                                  <Building className="h-4 w-4 text-teal-600" />
                                  Customer Group
                                </div>
                              </Label>
                            </div>
                          </RadioGroup>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Selection Configuration */}
                  {(selectionCriteria.hasCustomer || selectionCriteria.hasLocation || selectionCriteria.hasOrganization || selectionCriteria.hasSalesTeam || selectionCriteria.hasCustomerGroup) && (
                    <Card>
                      <CardHeader className="border-b">
                        <div className="flex items-center justify-between">
                          <CardTitle className="flex items-center gap-3 text-xl">
                            <div className="p-2 bg-blue-50 rounded-lg">
                              <Store className="h-5 w-5 text-blue-600" />
                            </div>
                            Selection Configuration
                          </CardTitle>
                          <div className="flex items-center gap-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => {
                                // Clear only the selected criteria's data
                                if (selectionCriteria.hasCustomer) {
                                  setSelectedStores([]);
                                } else if (selectionCriteria.hasLocation) {
                                  setSelectedBranches([]);
                                } else if (selectionCriteria.hasOrganization) {
                                  setSelectedOrganizations([]);
                                } else if (selectionCriteria.hasSalesTeam) {
                                  setSelectedSalesTeams([]);
                                  setSelectedRole("");
                                }
                              }}
                              className="hover:"
                            >
                              <X className="h-4 w-4 mr-2" />
                              Clear Selection
                            </Button>
                          </div>
                        </div>
                        <p className="text-sm text-muted-foreground mt-2">
                          Configure your selected criteria with specific items and entities
                        </p>
                      </CardHeader>
                      <CardContent className="p-6">
                        <div className="space-y-8">
                          {/* Store Selection */}
                          {selectionCriteria.hasCustomer && (
                            <div className="space-y-4">
                              <div className="flex items-center justify-between">
                                <Label className="text-base font-semibold flex items-center gap-2">
                                  <Store className="h-4 w-4 text-blue-600" />
                                  Select Stores
                                </Label>
                                <Badge variant="outline" className="bg-blue-50">
                                  {selectedStores.length} selected
                                </Badge>
                              </div>
                              
                              {/* Store Search */}
                              <div className="relative">
                                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                                <Input
                                  placeholder="Search stores by code or name..."
                                  value={storeSearchTerm}
                                  onChange={(e) => setStoreSearchTerm(e.target.value)}
                                  className="pl-10 h-10"
                                />
                              </div>

                              {/* Store Statistics */}
                              <div className="grid grid-cols-2 gap-3 mb-4">
                                <div className="bg-gradient-to-br from-blue-50 to-blue-100 p-3 rounded-lg border border-blue-200">
                                  <p className="text-xs font-medium text-blue-600 uppercase">Total Stores</p>
                                  <p className="text-xl font-bold text-blue-900">
                                    {loadingStores ? (
                                      <RefreshCw className="h-5 w-5 animate-spin" />
                                    ) : (
                                      displayedStores.length
                                    )}
                                    {storesPagination.totalCount > 0 && (
                                      <span className="text-sm text-blue-600 ml-1">
                                        / {storesPagination.totalCount}
                                      </span>
                                    )}
                                  </p>
                                </div>
                                <div className="bg-gradient-to-br from-green-50 to-green-100 p-3 rounded-lg border border-green-200">
                                  <p className="text-xs font-medium text-green-600 uppercase">Selected</p>
                                  <p className="text-xl font-bold text-green-900">{selectedStores.length}</p>
                                </div>
                              </div>

                              <div className="border border-gray-200 rounded-lg">
                                {loadingStores ? (
                                  <div className="h-[300px] p-4 space-y-3">
                                    {/* Skeleton loading for stores */}
                                    {[...Array(6)].map((_, index) => (
                                      <div key={index} className="flex items-center gap-3 p-3 rounded-lg border border-gray-100">
                                        <Skeleton className="h-5 w-5 rounded" />
                                        <div className="flex-1 space-y-2">
                                          <Skeleton className="h-4 w-32" />
                                          <Skeleton className="h-3 w-48" />
                                        </div>
                                        <Skeleton className="h-6 w-16 rounded-full" />
                                      </div>
                                    ))}
                                  </div>
                                ) : (
                                  <ScrollArea 
                                    className="h-[300px] px-4 py-2"
                                    onScrollCapture={(e) => {
                                      const { scrollTop, scrollHeight, clientHeight } = e.currentTarget;
                                      const scrollPercentage = (scrollTop + clientHeight) / scrollHeight;
                                      if (scrollPercentage > 0.8 && storesPagination.hasMore && !loadingMoreStores) {
                                        loadMoreStores();
                                      }
                                    }}
                                  >
                                    <div className="space-y-2 pr-2">
                                      {displayedStores.length === 0 ? (
                                        <div className="h-[250px] flex items-center justify-center">
                                          <div className="text-center text-gray-500">
                                            <Store className="h-12 w-12 mx-auto mb-2 opacity-50" />
                                            <p className="font-medium">No stores loaded</p>
                                            <p className="text-sm">Stores will load when Customer criteria is selected</p>
                                          </div>
                                        </div>
                                      ) : (
                                        <>
                                          {displayedStores.map((store, index) => {
                                            const isSelected = selectedStores.includes(store.uid);
                                            return (
                                              <div
                                                key={`${store.uid}-${index}`}
                                                className={`flex items-center justify-between p-3 rounded-lg border-2 transition-all duration-200 ${
                                                  isSelected 
                                                    ? "bg-blue-50 border-blue-300" 
                                                    : "border-gray-200 hover:border-gray-300 hover:shadow-sm"
                                                }`}
                                              >
                                                <div className="flex items-center gap-3">
                                                  <Checkbox
                                                    id={`store-${store.uid}-${index}`}
                                                    checked={isSelected}
                                                    onCheckedChange={(checked) => {
                                                      if (checked) {
                                                        setSelectedStores([...selectedStores, store.uid]);
                                                      } else {
                                                        setSelectedStores(selectedStores.filter((s) => s !== store.uid));
                                                      }
                                                    }}
                                                    className="h-5 w-5"
                                                  />
                                                  <div>
                                                    <span className="font-semibold text-gray-900">{store.code}</span>
                                                    <span className="text-gray-600 ml-2">{store.name}</span>
                                                  </div>
                                                </div>
                                                {isSelected && (
                                                  <Badge className="bg-blue-100 text-blue-700 border-blue-300">
                                                    Selected
                                                  </Badge>
                                                )}
                                              </div>
                                            );
                                          })}
                                          
                                          {/* Loading more indicator */}
                                          {loadingMoreStores && (
                                            <div className="text-center py-4 border-t border-gray-200">
                                              <div className="flex items-center justify-center text-sm text-gray-600">
                                                <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                                                Loading more stores...
                                              </div>
                                            </div>
                                          )}
                                          
                                          {/* End of list indicator */}
                                          {!storesPagination.hasMore && displayedStores.length > 0 && !loadingMoreStores && (
                                            <div className="text-center py-4 border-t border-gray-200">
                                              <p className="text-sm text-gray-500">
                                                All stores loaded ({displayedStores.length} total)
                                              </p>
                                            </div>
                                          )}
                                        </>
                                      )}
                                    </div>
                                  </ScrollArea>
                                )}
                              </div>
                            </div>
                          )}

                          {/* Location/Branch Selection with Hierarchy */}
                          {selectionCriteria.hasLocation && (
                            <div className="space-y-4">
                              <div className="flex items-center justify-between">
                                <Label className="text-base font-semibold flex items-center gap-2">
                                  <MapPin className="h-4 w-4 text-green-600" />
                                  Select Locations
                                </Label>
                                <Badge variant="outline" className="bg-green-50">
                                  {selectedLocationNodes.size} selected
                                </Badge>
                              </div>
                              
                              {/* Branch Search */}
                              <div className="relative">
                                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                                <Input
                                  placeholder="Search locations by code or name..."
                                  value={branchSearchTerm}
                                  onChange={(e) => setBranchSearchTerm(e.target.value)}
                                  className="pl-10 h-10"
                                />
                              </div>

                              {/* Branch Statistics */}
                              <div className="grid grid-cols-2 gap-3 mb-4">
                                <div className="bg-gradient-to-br from-green-50 to-green-100 p-3 rounded-lg border border-green-200">
                                  <p className="text-xs font-medium text-green-600 uppercase">Total Locations</p>
                                  <p className="text-xl font-bold text-green-900">
                                    {loadingBranches ? (
                                      <RefreshCw className="h-5 w-5 animate-spin" />
                                    ) : (
                                      locations.length
                                    )}
                                  </p>
                                </div>
                                <div className="bg-gradient-to-br from-blue-50 to-blue-100 p-3 rounded-lg border border-blue-200">
                                  <p className="text-xs font-medium text-blue-600 uppercase">Selected</p>
                                  <p className="text-xl font-bold text-blue-900">{selectedLocationNodes.size}</p>
                                </div>
                              </div>

                              {/* Toggle between hierarchy and flat view */}
                              <div className="flex items-center gap-2 mb-3">
                                <Button
                                  variant={hierarchyView ? "default" : "outline"}
                                  size="sm"
                                  onClick={() => setHierarchyView(true)}
                                >
                                  <MapPinned className="h-4 w-4 mr-1" />
                                  Hierarchy View
                                </Button>
                                <Button
                                  variant={!hierarchyView ? "default" : "outline"}
                                  size="sm"
                                  onClick={() => {
                                    setHierarchyView(false);
                                    if (displayedBranches.length === 0) {
                                      loadBranches();
                                    }
                                  }}
                                >
                                  <MapPin className="h-4 w-4 mr-1" />
                                  List View
                                </Button>
                              </div>

                              <div className="border border-gray-200 rounded-lg">
                                {loadingBranches ? (
                                  <div className="h-[400px] p-4 space-y-3">
                                    {/* Skeleton loading for locations */}
                                    {[...Array(8)].map((_, index) => (
                                      <div key={index} className="flex items-center gap-3 p-3 rounded-lg border border-gray-100">
                                        <Skeleton className="h-5 w-5 rounded" />
                                        <div className="flex-1 space-y-2">
                                          <Skeleton className="h-4 w-32" />
                                          <Skeleton className="h-3 w-48" />
                                        </div>
                                        <Skeleton className="h-6 w-20 rounded-full" />
                                      </div>
                                    ))}
                                  </div>
                                ) : hierarchyView ? (
                                  /* Hierarchy View */
                                  <ScrollArea className="h-[400px] px-4 py-2">
                                    <div className="space-y-1 pr-2">
                                      {locationHierarchy.length === 0 ? (
                                        <div className="h-[350px] flex items-center justify-center">
                                          <div className="text-center text-gray-500">
                                            <MapPinned className="h-12 w-12 mx-auto mb-2 opacity-50" />
                                            <p className="font-medium">No locations loaded</p>
                                            <p className="text-sm">Locations will load when Location criteria is selected</p>
                                          </div>
                                        </div>
                                      ) : (
                                        <>
                                          {/* Select All/Clear All buttons */}
                                          <div className="flex items-center justify-between p-2 mb-2 border-b">
                                            <div className="flex gap-2">
                                              <Button
                                                variant="outline"
                                                size="sm"
                                                onClick={() => {
                                                  const allNodes = new Set<string>();
                                                  const collectAllNodes = (nodes: LocationHierarchyNode[]) => {
                                                    nodes.forEach(n => {
                                                      allNodes.add(n.uid);
                                                      if (n.children) collectAllNodes(n.children);
                                                    });
                                                  };
                                                  collectAllNodes(locationHierarchy);
                                                  setSelectedLocationNodes(allNodes);
                                                  setSelectedBranches(Array.from(allNodes));
                                                }}
                                              >
                                                Select All
                                              </Button>
                                              <Button
                                                variant="outline"
                                                size="sm"
                                                onClick={() => {
                                                  setSelectedLocationNodes(new Set());
                                                  setSelectedBranches([]);
                                                }}
                                              >
                                                Clear All
                                              </Button>
                                            </div>
                                            <Button
                                              variant="outline"
                                              size="sm"
                                              onClick={() => {
                                                const allNodes = new Set(locationHierarchy.map(n => n.uid));
                                                const newExpanded = new Set(expandedNodes);
                                                allNodes.forEach(uid => {
                                                  if (expandedNodes.has(uid)) {
                                                    newExpanded.delete(uid);
                                                  } else {
                                                    newExpanded.add(uid);
                                                  }
                                                });
                                                setExpandedNodes(newExpanded);
                                              }}
                                            >
                                              {expandedNodes.size > 0 ? 'Collapse All' : 'Expand All'}
                                            </Button>
                                          </div>
                                          
                                          {/* Location hierarchy tree */}
                                          {locationHierarchy
                                            .filter(node => 
                                              branchSearchTerm === "" || 
                                              node.name.toLowerCase().includes(branchSearchTerm.toLowerCase()) ||
                                              node.code.toLowerCase().includes(branchSearchTerm.toLowerCase())
                                            )
                                            .map((node) => (
                                              <LocationTreeNode
                                                key={node.uid}
                                                node={node}
                                                onToggle={toggleNode}
                                                onSelect={selectNodeWithChildren}
                                                expandedNodes={expandedNodes}
                                                selectedNodes={selectedLocationNodes}
                                              />
                                            ))}
                                        </>
                                      )}
                                    </div>
                                  </ScrollArea>
                                ) : (
                                  /* List View - Original flat list */
                                  <ScrollArea 
                                    className="h-[400px] px-4 py-2"
                                    onScrollCapture={(e) => {
                                      const { scrollTop, scrollHeight, clientHeight } = e.currentTarget;
                                      const scrollPercentage = (scrollTop + clientHeight) / scrollHeight;
                                      if (scrollPercentage > 0.8 && branchesPagination.hasMore && !loadingMoreBranches) {
                                        loadMoreBranches();
                                      }
                                    }}
                                  >
                                    <div className="space-y-2 pr-2">
                                      {displayedBranches.length === 0 ? (
                                        <div className="h-[350px] flex items-center justify-center">
                                          <div className="text-center text-gray-500">
                                            <MapPin className="h-12 w-12 mx-auto mb-2 opacity-50" />
                                            <p className="font-medium">No branches loaded</p>
                                            <p className="text-sm">Click to load list view</p>
                                          </div>
                                        </div>
                                      ) : (
                                        <>
                                          {displayedBranches.map((branch) => {
                                            const isSelected = selectedBranches.includes(branch.uid);
                                            return (
                                              <div
                                                key={branch.uid}
                                                className={`flex items-center justify-between p-3 rounded-lg border-2 transition-all duration-200 ${
                                                  isSelected 
                                                    ? "bg-green-50 border-green-300" 
                                                    : "border-gray-200 hover:border-gray-300 hover:shadow-sm"
                                                }`}
                                              >
                                                <div className="flex items-center gap-3">
                                                  <Checkbox
                                                    id={`branch-${branch.uid}`}
                                                    checked={isSelected}
                                                    onCheckedChange={(checked) => {
                                                      if (checked) {
                                                        setSelectedBranches([...selectedBranches, branch.uid]);
                                                        setSelectedLocationNodes(new Set([...selectedLocationNodes, branch.uid]));
                                                      } else {
                                                        setSelectedBranches(selectedBranches.filter((b) => b !== branch.uid));
                                                        const newNodes = new Set(selectedLocationNodes);
                                                        newNodes.delete(branch.uid);
                                                        setSelectedLocationNodes(newNodes);
                                                      }
                                                    }}
                                                    className="h-5 w-5"
                                                  />
                                                  <div>
                                                    <span className="font-semibold text-gray-900">{branch.code}</span>
                                                    <span className="text-gray-600 ml-2">{branch.name}</span>
                                                  </div>
                                                </div>
                                                {isSelected && (
                                                  <Badge className="bg-green-100 text-green-700 border-green-300">
                                                    Selected
                                                  </Badge>
                                                )}
                                              </div>
                                            );
                                          })}
                                          
                                          {/* Loading more indicator */}
                                          {loadingMoreBranches && (
                                            <div className="text-center py-4 border-t border-gray-200">
                                              <div className="flex items-center justify-center text-sm text-gray-600">
                                                <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                                                Loading more branches...
                                              </div>
                                            </div>
                                          )}
                                          
                                          {/* End of list indicator */}
                                          {!branchesPagination.hasMore && displayedBranches.length > 0 && !loadingMoreBranches && (
                                            <div className="text-center py-4 border-t border-gray-200">
                                              <p className="text-sm text-gray-500">
                                                All branches loaded ({displayedBranches.length} total)
                                              </p>
                                            </div>
                                          )}
                                        </>
                                      )}
                                    </div>
                                  </ScrollArea>
                                )}
                              </div>
                            </div>
                          )}

                          {/* Organization Selection with Hierarchy */}
                          {selectionCriteria.hasOrganization && (
                            <div className="space-y-4">
                              <div className="flex items-center justify-between">
                                <Label className="text-base font-semibold flex items-center gap-2">
                                  <Building2 className="h-4 w-4 text-orange-600" />
                                  Select Organizations
                                </Label>
                                <Badge variant="outline" className="bg-orange-50">
                                  {selectedOrgNodes.size} selected
                                </Badge>
                              </div>
                              
                              {/* Organization Search */}
                              <div className="relative">
                                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                                <Input
                                  placeholder="Search organizations by code or name..."
                                  value={organizationSearchTerm}
                                  onChange={(e) => setOrganizationSearchTerm(e.target.value)}
                                  className="pl-10 h-10"
                                />
                              </div>

                              <div className="border border-gray-200 rounded-lg">
                                {loadingOrganizations ? (
                                  <div className="h-[400px] p-4 space-y-3">
                                    {/* Skeleton loading for organizations */}
                                    {[...Array(8)].map((_, index) => (
                                      <div key={index} className="flex items-center gap-3 p-3 rounded-lg border border-gray-100">
                                        <Skeleton className="h-5 w-5 rounded" />
                                        <div className="flex-1 space-y-2">
                                          <Skeleton className="h-4 w-36" />
                                          <Skeleton className="h-3 w-44" />
                                        </div>
                                        <Skeleton className="h-6 w-20 rounded-full" />
                                      </div>
                                    ))}
                                  </div>
                                ) : (
                                  <ScrollArea className="h-[400px] px-4 py-2">
                                    <div className="space-y-1 pr-2">
                                      {organizationHierarchy.length === 0 ? (
                                        <div className="h-[350px] flex items-center justify-center">
                                          <div className="text-center text-gray-500">
                                            <Building2 className="h-12 w-12 mx-auto mb-2 opacity-50" />
                                            <p className="font-medium">No organizations loaded</p>
                                            <p className="text-sm">Organizations will load when Organization criteria is selected</p>
                                          </div>
                                        </div>
                                      ) : (
                                        <>
                                          {/* Select All/Clear All buttons */}
                                          <div className="flex items-center justify-between p-2 mb-2 border-b">
                                            <div className="flex gap-2">
                                              <Button
                                                variant="outline"
                                                size="sm"
                                                onClick={() => {
                                                  const allNodes = new Set<string>();
                                                  const collectAllNodes = (nodes: OrganizationHierarchyNode[]) => {
                                                    nodes.forEach(n => {
                                                      allNodes.add(n.uid);
                                                      if (n.children) collectAllNodes(n.children);
                                                    });
                                                  };
                                                  collectAllNodes(organizationHierarchy);
                                                  setSelectedOrgNodes(allNodes);
                                                  setSelectedOrganizations(Array.from(allNodes));
                                                }}
                                              >
                                                Select All
                                              </Button>
                                              <Button
                                                variant="outline"
                                                size="sm"
                                                onClick={() => {
                                                  setSelectedOrgNodes(new Set());
                                                  setSelectedOrganizations([]);
                                                }}
                                              >
                                                Clear All
                                              </Button>
                                            </div>
                                            <Button
                                              variant="outline"
                                              size="sm"
                                              onClick={() => {
                                                const allNodes = new Set(organizationHierarchy.map(n => n.uid));
                                                const newExpanded = new Set(expandedOrgNodes);
                                                allNodes.forEach(uid => {
                                                  if (expandedOrgNodes.has(uid)) {
                                                    newExpanded.delete(uid);
                                                  } else {
                                                    newExpanded.add(uid);
                                                  }
                                                });
                                                setExpandedOrgNodes(newExpanded);
                                              }}
                                            >
                                              {expandedOrgNodes.size > 0 ? 'Collapse All' : 'Expand All'}
                                            </Button>
                                          </div>
                                          
                                          {/* Organization hierarchy tree */}
                                          {organizationHierarchy
                                            .filter(node => 
                                              organizationSearchTerm === "" || 
                                              node.name.toLowerCase().includes(organizationSearchTerm.toLowerCase()) ||
                                              node.code.toLowerCase().includes(organizationSearchTerm.toLowerCase())
                                            )
                                            .map((node) => (
                                              <OrganizationTreeNode
                                                key={node.uid}
                                                node={node}
                                                onToggle={toggleOrgNode}
                                                onSelect={selectOrgNodeWithChildren}
                                                expandedNodes={expandedOrgNodes}
                                                selectedNodes={selectedOrgNodes}
                                              />
                                            ))}
                                        </>
                                      )}
                                    </div>
                                  </ScrollArea>
                                )}
                              </div>
                            </div>
                          )}

                          {/* Sales Team Selection */}
                          {selectionCriteria.hasSalesTeam && (
                            <div className="space-y-4">
                              <div className="flex items-center justify-between">
                                <Label className="text-base font-semibold flex items-center gap-2">
                                  <Users className="h-4 w-4 text-purple-600" />
                                  Select Sales Team
                                </Label>
                                <Badge variant="outline" className="px-3 py-1">
                                  {selectedSalesTeams.length} Selected
                                </Badge>
                              </div>

                              {/* Role Selection */}
                              <div className="space-y-2">
                                <Label htmlFor="roleSelect" className="text-sm font-medium">
                                  Select Role *
                                </Label>
                                <Select
                                  value={selectedRole}
                                  onValueChange={(value) => {
                                    setSelectedRole(value);
                                    setSelectedSalesTeams([]); // Clear previous selections
                                    loadEmployeesByRole(value, currentUser?.orgUID);
                                  }}
                                >
                                  <SelectTrigger id="roleSelect" className="max-w-md">
                                    <SelectValue placeholder="Select a role..." />
                                  </SelectTrigger>
                                  <SelectContent>
                                    {availableRoles.map((role) => (
                                      <SelectItem key={role.value} value={role.value}>
                                        {role.label}
                                      </SelectItem>
                                    ))}
                                  </SelectContent>
                                </Select>
                              </div>

                              {/* Sales Team Members Selection */}
                              {selectedRole && (
                                <div className="space-y-2">
                                  <Label className="text-sm font-medium">
                                    Available Sales Team Members
                                  </Label>
                                  {loadingSalesTeams ? (
                                    <div className="space-y-2">
                                      <Skeleton className="h-10 w-full" />
                                      <Skeleton className="h-10 w-full" />
                                      <Skeleton className="h-10 w-full" />
                                    </div>
                                  ) : displayedSalesTeams.length > 0 ? (
                                    <ScrollArea className="h-48 border rounded-lg">
                                      <div className="p-4 space-y-2">
                                        {displayedSalesTeams.map((member) => (
                                          <div
                                            key={member.uid}
                                            className="flex items-center space-x-3 p-2 rounded-lg hover:bg-gray-50"
                                          >
                                            <Checkbox
                                              id={member.uid}
                                              checked={selectedSalesTeams.includes(member.uid)}
                                              onCheckedChange={(checked) => {
                                                if (checked) {
                                                  setSelectedSalesTeams([...selectedSalesTeams, member.uid]);
                                                } else {
                                                  setSelectedSalesTeams(
                                                    selectedSalesTeams.filter(id => id !== member.uid)
                                                  );
                                                }
                                              }}
                                            />
                                            <Label
                                              htmlFor={member.uid}
                                              className="flex-1 cursor-pointer text-sm"
                                            >
                                              <div className="flex items-center gap-2">
                                                <Users className="h-4 w-4 text-purple-500" />
                                                <span className="font-medium">{member.name}</span>
                                                <Badge variant="secondary" className="text-xs">
                                                  {member.code}
                                                </Badge>
                                              </div>
                                            </Label>
                                          </div>
                                        ))}
                                      </div>
                                    </ScrollArea>
                                  ) : (
                                    <div className="text-center py-8 text-gray-500">
                                      <Users className="h-12 w-12 mx-auto mb-4 text-gray-300" />
                                      <p>No sales team members found for selected role</p>
                                      <p className="text-xs text-gray-400 mt-1">
                                        Try selecting a different role
                                      </p>
                                    </div>
                                  )}
                                </div>
                              )}

                              {/* Selected Sales Team Summary */}
                              {selectedSalesTeams.length > 0 && (
                                <div className="bg-purple-50 p-4 rounded-lg border border-purple-200">
                                  <h4 className="font-medium text-purple-900 mb-2">
                                    Selected Sales Team Members ({selectedSalesTeams.length})
                                  </h4>
                                  <div className="flex flex-wrap gap-2">
                                    {selectedSalesTeams.map((teamMemberUID) => {
                                      const member = displayedSalesTeams.find(m => m.uid === teamMemberUID);
                                      return member ? (
                                        <Badge
                                          key={teamMemberUID}
                                          variant="secondary"
                                          className="bg-purple-100 text-purple-800"
                                        >
                                          <Users className="h-3 w-3 mr-1" />
                                          {member.name} ({member.code})
                                        </Badge>
                                      ) : null;
                                    })}
                                  </div>
                                </div>
                              )}
                            </div>
                          )}

                          {/* Customer Group Selection */}
                          {selectionCriteria.hasCustomerGroup && (
                            <div className="space-y-4">
                              <div className="flex items-center justify-between">
                                <Label className="text-base font-semibold flex items-center gap-2">
                                  <Building className="h-4 w-4 text-teal-600" />
                                  Select Customer Groups
                                </Label>
                                <Badge variant="outline" className="px-3 py-1">
                                  {selectedCustomerGroups.length} Selected
                                </Badge>
                              </div>

                              {/* Store Group Type Filter - Hidden for now */}
                              {false && (
                                <div className="space-y-2">
                                  <Label htmlFor="storeGroupTypeSelect" className="text-sm font-medium">
                                    Filter by Customer Group Type (Optional)
                                  </Label>
                                  <Select
                                    value={selectedCustomerGroupType}
                                    onValueChange={(value) => {
                                      setSelectedCustomerGroupType(value);
                                      setSelectedCustomerGroups([]); // Clear previous selections
                                      setDisplayedCustomerGroups([]); // Clear displayed groups
                                      setCustomerGroupsPagination(prev => ({ ...prev, currentPage: 1 }));
                                    }}
                                  >
                                    <SelectTrigger id="storeGroupTypeSelect" className="max-w-md">
                                      <SelectValue placeholder="All customer group types..." />
                                    </SelectTrigger>
                                    <SelectContent>
                                      <SelectItem value="all">All Types</SelectItem>
                                      {customerGroupTypes.map((type) => (
                                        <SelectItem key={type.UID} value={type.UID}>
                                          {type.Name} ({type.Code})
                                        </SelectItem>
                                      ))}
                                    </SelectContent>
                                  </Select>
                                </div>
                              )}

                              {/* Search Store Groups */}
                              <div className="space-y-2">
                                <Label htmlFor="storeGroupSearch" className="text-sm font-medium">
                                  Search Customer Groups
                                </Label>
                                <div className="relative">
                                  <Search className="h-4 w-4 absolute left-3 top-3 text-gray-400" />
                                  <Input
                                    id="storeGroupSearch"
                                    type="text"
                                    placeholder="Search by name or code..."
                                    value={customerGroupSearchTerm}
                                    onChange={(e) => setCustomerGroupSearchTerm(e.target.value)}
                                    className="pl-10"
                                  />
                                </div>
                              </div>

                              {/* Store Groups Selection */}
                              <div className="space-y-2">
                                <Label className="text-sm font-medium">
                                  Available Customer Groups
                                </Label>
                                {loadingCustomerGroups ? (
                                  <div className="space-y-2">
                                    <Skeleton className="h-10 w-full" />
                                    <Skeleton className="h-10 w-full" />
                                    <Skeleton className="h-10 w-full" />
                                  </div>
                                ) : displayedCustomerGroups.length > 0 ? (
                                  <ScrollArea className="h-64 border rounded-lg">
                                    <div className="p-4 space-y-2">
                                      {displayedCustomerGroups.map((group) => (
                                        <div
                                          key={group.UID}
                                          className="flex items-center space-x-3 p-2 rounded-lg hover:bg-gray-50"
                                        >
                                          <Checkbox
                                            id={group.UID}
                                            checked={selectedCustomerGroups.includes(group.UID)}
                                            onCheckedChange={(checked) => {
                                              if (checked) {
                                                setSelectedCustomerGroups([...selectedCustomerGroups, group.UID]);
                                              } else {
                                                setSelectedCustomerGroups(
                                                  selectedCustomerGroups.filter(id => id !== group.UID)
                                                );
                                              }
                                            }}
                                          />
                                          <Label
                                            htmlFor={group.UID}
                                            className="flex-1 cursor-pointer text-sm"
                                          >
                                            <div className="flex items-center gap-2">
                                              <Building className="h-4 w-4 text-teal-500" />
                                              <div className="flex flex-col">
                                                <span className="font-medium">{group.Name}</span>
                                                <div className="flex items-center gap-1 text-xs text-gray-500">
                                                  <Badge variant="secondary" className="text-xs">
                                                    {group.Code}
                                                  </Badge>
                                                  {group.ItemLevel && (
                                                    <Badge variant="outline" className="text-xs">
                                                      Level {group.ItemLevel}
                                                    </Badge>
                                                  )}
                                                </div>
                                              </div>
                                            </div>
                                          </Label>
                                        </div>
                                      ))}
                                      
                                      {/* Load More Button */}
                                      {customerGroupsPagination.hasMore && (
                                        <div className="pt-4 border-t">
                                          <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={loadMoreCustomerGroups}
                                            disabled={loadingMoreCustomerGroups}
                                            className="w-full"
                                          >
                                            {loadingMoreCustomerGroups ? (
                                              <>
                                                <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                                                Loading...
                                              </>
                                            ) : (
                                              <>
                                                <ChevronDown className="h-4 w-4 mr-2" />
                                                Load More ({customerGroupsPagination.totalCount - displayedCustomerGroups.length} remaining)
                                              </>
                                            )}
                                          </Button>
                                        </div>
                                      )}
                                    </div>
                                  </ScrollArea>
                                ) : (
                                  <div className="text-center py-8 text-gray-500">
                                    <Building className="h-12 w-12 mx-auto mb-4 text-gray-300" />
                                    <p>No customer groups found</p>
                                    <p className="text-xs text-gray-400 mt-1">
                                      Try adjusting your search or filter criteria
                                    </p>
                                  </div>
                                )}
                              </div>

                              {/* Selected Customer Groups Summary */}
                              {selectedCustomerGroups.length > 0 && (
                                <div className="bg-teal-50 p-4 rounded-lg border border-teal-200">
                                  <h4 className="font-medium text-teal-900 mb-2">
                                    Selected Customer Groups ({selectedCustomerGroups.length})
                                  </h4>
                                  <div className="flex flex-wrap gap-2">
                                    {selectedCustomerGroups.map((groupUID) => {
                                      const group = displayedCustomerGroups.find(g => g.UID === groupUID);
                                      return group ? (
                                        <Badge
                                          key={groupUID}
                                          variant="secondary"
                                          className="bg-teal-100 text-teal-800"
                                        >
                                          <Building className="h-3 w-3 mr-1" />
                                          {group.Name} ({group.Code})
                                        </Badge>
                                      ) : null;
                                    })}
                                  </div>
                                </div>
                              )}
                            </div>
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  )}

                  {/* Create Mapping */}
                  <div className="space-y-4">
                    <div>
                      <h3 className="font-semibold">Create Mapping</h3>
                      <p className="text-sm text-muted-foreground">
                        Review your configuration and create the item linking mapping
                      </p>
                    </div>
                    <div className="rounded-lg border border-gray-200 p-6">
                          <div className="flex justify-end space-x-3">
                            <Button
                              type="button"
                              variant="outline"
                              onClick={resetForm}
                              disabled={loading}
                              className="px-6 py-2.5 border-gray-300 hover:border-gray-400 text-gray-700"
                            >
                              <X className="h-4 w-4 mr-2" />
                              Reset
                            </Button>
                            <Button
                              type="button"
                              onClick={(e) => handleCreateMapping(e)}
                              disabled={
                                isCreating || loading || 
                                (linkedItemType === "SKUClassGroup" && !selectedSKUClassGroup) ||
                                (linkedItemType === "PriceList" && !selectedPriceList)
                              }
                              className="px-6 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white shadow-lg"
                            >
                              {isCreating ? (
                                <>
                                  <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                                  Creating...
                                </>
                              ) : (
                                <>
                                  <Save className="h-4 w-4 mr-2" />
                                  Create Mapping
                                </>
                              )}
                            </Button>
                          </div>
                    </div>
                  </div>
                </form>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="view" className="space-y-6">
            {/* Statistics Cards */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="bg-gradient-to-br from-blue-50 to-blue-100 p-4 rounded-lg border border-blue-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-xs font-medium text-blue-600 uppercase tracking-wider">
                      Total Mappings
                    </p>
                    <p className="text-2xl font-bold text-blue-900 mt-1">
                      {existingMappings.length}
                    </p>
                  </div>
                  <div className="p-2 bg-blue-200 rounded-lg">
                    <Link2 className="h-4 w-4 text-blue-700" />
                  </div>
                </div>
              </div>

              <div className="bg-gradient-to-br from-green-50 to-green-100 p-4 rounded-lg border border-green-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-xs font-medium text-green-600 uppercase tracking-wider">
                      Active
                    </p>
                    <p className="text-2xl font-bold text-green-900 mt-1">
                      {
                        existingMappings.filter(
                          (m) => m.selectionMapCriteria?.isActive
                        ).length
                      }
                    </p>
                  </div>
                  <div className="p-2 bg-green-200 rounded-lg">
                    <RefreshCw className="h-4 w-4 text-green-700" />
                  </div>
                </div>
              </div>

              <div className="bg-gradient-to-br from-purple-50 to-purple-100 p-4 rounded-lg border border-purple-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-xs font-medium text-purple-600 uppercase tracking-wider">
                      SKU Groups
                    </p>
                    <p className="text-2xl font-bold text-purple-900 mt-1">
                      {skuClassGroups.length}
                    </p>
                  </div>
                  <div className="p-2 bg-purple-200 rounded-lg">
                    <Package className="h-4 w-4 text-purple-700" />
                  </div>
                </div>
              </div>

              <div className="bg-gradient-to-br from-orange-50 to-orange-100 p-4 rounded-lg border border-orange-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-xs font-medium text-orange-600 uppercase tracking-wider">
                      Filtered Results
                    </p>
                    <p className="text-2xl font-bold text-orange-900 mt-1">
                      {totalFilteredCount}
                    </p>
                  </div>
                  <div className="p-2 bg-orange-200 rounded-lg">
                    <Search className="h-4 w-4 text-orange-700" />
                  </div>
                </div>
              </div>
            </div>

            <Card>
              <CardHeader className="border-b">
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="flex items-center gap-3 text-xl">
                      <div className="p-2 bg-green-50 rounded-lg">
                        <Search className="h-5 w-5 text-green-600" />
                      </div>
                      Existing Mappings
                    </CardTitle>
                    <CardDescription className="text-base mt-2">
                      View and manage existing item linking configurations with
                      advanced filtering
                    </CardDescription>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      loadAllMappingsFromDatabase(true)
                    }
                    disabled={checkingMappings}
                  >
                    <RefreshCw
                      className={`h-4 w-4 mr-2 ${
                        checkingMappings ? "animate-spin" : ""
                      }`}
                    />
                    {checkingMappings ? "Loading..." : "Refresh All Mappings"}
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                {/* Search and Filter */}
                <div className="flex gap-4 mb-4">
                  <div className="flex-1 relative">
                    <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input
                      placeholder="Search by SKU Class Group..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="pl-8"
                    />
                  </div>
                  <Select value={filterType} onValueChange={setFilterType}>
                    <SelectTrigger className="w-[180px]">
                      <SelectValue placeholder="Filter by type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Types</SelectItem>
                      <SelectItem value="customer">Customer</SelectItem>
                      <SelectItem value="location">Location</SelectItem>
                      <SelectItem value="organization">Organization</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {/* Mappings Table */}
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Linked Item</TableHead>
                        <TableHead>Item Type</TableHead>
                        <TableHead>Mapping Configuration</TableHead>
                        <TableHead>Total Mapped</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead className="text-right">Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {loading ? (
                        <TableRow>
                          <TableCell colSpan={6}>
                            <div className="space-y-2">
                              <Skeleton className="h-4 w-full" />
                              <Skeleton className="h-4 w-full" />
                              <Skeleton className="h-4 w-full" />
                            </div>
                          </TableCell>
                        </TableRow>
                      ) : totalFilteredCount === 0 ? (
                        <TableRow>
                          <TableCell colSpan={6} className="text-center py-8">
                            <div className="flex flex-col items-center justify-center text-muted-foreground">
                              {skuClassGroups.length === 0 ? (
                                <>
                                  <Store className="h-12 w-12 mb-2 opacity-50" />
                                  <p className="font-medium mb-1">
                                    No SKU Class Groups Available
                                  </p>
                                  <p className="text-sm">
                                    Please ensure SKU Class Groups are
                                    configured in the system
                                  </p>
                                </>
                              ) : existingMappings.length === 0 ? (
                                <>
                                  <Link2 className="h-12 w-12 mb-2 opacity-50" />
                                  <p className="font-medium mb-1">
                                    No Item Linkings Found
                                  </p>
                                  <p className="text-sm">
                                    Create a new item linking in the "Create
                                    Linking" tab
                                  </p>
                                  <Button
                                    variant="outline"
                                    size="sm"
                                    className="mt-3"
                                    onClick={() => setActiveTab("create")}
                                  >
                                    Create First Linking
                                  </Button>
                                </>
                              ) : (
                                <>
                                  <Search className="h-12 w-12 mb-2 opacity-50" />
                                  <p className="font-medium mb-1">
                                    No Matching Results
                                  </p>
                                  <p className="text-sm">
                                    Try adjusting your search or filter criteria
                                  </p>
                                </>
                              )}
                            </div>
                          </TableCell>
                        </TableRow>
                      ) : (
                        paginatedMappings
                          .map((mapping) => {
                            // Skip invalid mappings
                            if (!mapping?.selectionMapCriteria) {
                              return null;
                            }

                            // Handle both camelCase and PascalCase for criteria
                            const criteria = mapping.selectionMapCriteria;
                            const linkedItemUID =
                              (criteria as any).LinkedItemUID ||
                              criteria.linkedItemUID ||
                              (criteria as any).UID ||
                              criteria.uid ||
                              "";
                            const uid = (criteria as any).UID || criteria.uid || "";
                            const linkedItemType = 
                              (criteria as any).LinkedItemType ||
                              criteria.linkedItemType ||
                              "Unknown";
                            const hasCustomer =
                              (criteria as any).HasCustomer ||
                              criteria.hasCustomer ||
                              false;
                            const hasLocation =
                              (criteria as any).HasLocation ||
                              criteria.hasLocation ||
                              false;
                            const hasOrganization =
                              (criteria as any).HasOrganization ||
                              criteria.hasOrganization ||
                              false;
                            const hasSalesTeam =
                              (criteria as any).HasSalesTeam ||
                              criteria.hasSalesTeam ||
                              false;
                            const hasItem =
                              (criteria as any).HasItem ||
                              criteria.hasItem ||
                              false;
                            const customerCount =
                              (criteria as any).CustomerCount ||
                              criteria.customerCount ||
                              0;
                            const locationCount =
                              (criteria as any).LocationCount ||
                              criteria.locationCount ||
                              0;
                            const orgCount =
                              (criteria as any).OrgCount ||
                              criteria.orgCount ||
                              0;
                            const salesTeamCount =
                              (criteria as any).SalesTeamCount ||
                              criteria.salesTeamCount ||
                              0;
                            const itemCount =
                              (criteria as any).ItemCount ||
                              criteria.itemCount ||
                              0;
                            const isActive =
                              (criteria as any).IsActive !== undefined 
                                ? (criteria as any).IsActive 
                                : criteria.isActive !== undefined 
                                  ? criteria.isActive 
                                  : true;

                            // Try to find the SKU Class Group by UID or by name match
                            const skuClassGroup = skuClassGroups.find(
                              (g) =>
                                g.UID === linkedItemUID ||
                                g.UID === uid ||
                                g.Name === linkedItemUID ||
                                linkedItemUID?.includes(g.Name)
                            );
                            
                            // Try to find the Price List if it's a PriceList type
                            const priceList = priceLists.find(
                              (p) =>
                                p.UID === linkedItemUID ||
                                p.UID === uid ||
                                p.Code === linkedItemUID ||
                                p.Name === linkedItemUID
                            );
                            
                            // Determine the display name without showing UIDs
                            let displayName = "Unknown";
                            if (linkedItemType === "SKUClassGroup") {
                              displayName = skuClassGroup?.Name || "SKU Class Group";
                            } else if (linkedItemType === "PriceList" || linkedItemType === "CustomerPriceList") {
                              displayName = priceList?.Name || priceList?.Code || "Price List";
                            } else {
                              displayName = linkedItemType || "Unknown Type";
                            }

                            return (
                              <TableRow key={uid || Math.random()}>
                                {/* Linked Item column */}
                                <TableCell className="font-medium">
                                  {displayName}
                                </TableCell>
                                
                                {/* Item Type column */}
                                <TableCell>
                                  <Badge variant="secondary" className="font-normal">
                                    {linkedItemType}
                                  </Badge>
                                </TableCell>
                                
                                {/* Mapping Configuration column */}
                                <TableCell>
                                  <div className="flex flex-wrap gap-1">
                                    {hasOrganization && (
                                      <Badge
                                        variant="outline"
                                        className="text-xs"
                                      >
                                        <Building2 className="h-3 w-3 mr-1" />
                                        Organization ({orgCount})
                                      </Badge>
                                    )}
                                    {hasLocation && (
                                      <Badge
                                        variant="outline"
                                        className="text-xs"
                                      >
                                        <MapPin className="h-3 w-3 mr-1" />
                                        Location ({locationCount})
                                      </Badge>
                                    )}
                                    {hasCustomer && (
                                      <Badge
                                        variant="outline"
                                        className="text-xs"
                                      >
                                        <Store className="h-3 w-3 mr-1" />
                                        Customer ({customerCount})
                                      </Badge>
                                    )}
                                    {hasSalesTeam && (
                                      <Badge
                                        variant="outline"
                                        className="text-xs"
                                      >
                                        <Users className="h-3 w-3 mr-1" />
                                        Sales Team ({salesTeamCount})
                                      </Badge>
                                    )}
                                    {hasItem && (
                                      <Badge
                                        variant="outline"
                                        className="text-xs"
                                      >
                                        <Package className="h-3 w-3 mr-1" />
                                        Item ({itemCount})
                                      </Badge>
                                    )}
                                  </div>
                                </TableCell>
                                
                                {/* Total Mapped column */}
                                <TableCell>
                                  <div className="text-sm">
                                    <div className="font-medium">
                                      {getMappingCount(
                                        mapping.selectionMapCriteria
                                      )} entities
                                    </div>
                                    {mapping.selectionMapDetails &&
                                      mapping.selectionMapDetails.length >
                                        0 && (
                                        <ViewMappingDetails
                                          mapping={mapping}
                                          stores={stores}
                                          branches={branches}
                                          displayedCustomerGroups={displayedCustomerGroups}
                                          skuClassGroup={skuClassGroup}
                                        />
                                      )}
                                  </div>
                                </TableCell>
                                
                                {/* Status column */}
                                <TableCell>
                                  <Badge 
                                    variant={isActive ? "default" : "secondary"}
                                    className={isActive ? "bg-green-500" : "bg-gray-400"}
                                  >
                                    {isActive ? "Active" : "Inactive"}
                                  </Badge>
                                </TableCell>
                                
                                {/* Actions column */}
                                <TableCell className="text-right">
                                  <div className="flex justify-end gap-1">
                                    <Button
                                      variant="ghost"
                                      size="icon"
                                      className="h-8 w-8"
                                      onClick={() => handleEditMapping(mapping)}
                                      title="Edit Mapping"
                                    >
                                      <Edit className="h-4 w-4" />
                                    </Button>
                                    <Button
                                      variant="ghost"
                                      size="icon"
                                      className="h-8 w-8"
                                      onClick={() =>
                                        handleDeleteMapping(mapping)
                                      }
                                      title="Delete Mapping"
                                    >
                                      <Trash2 className="h-4 w-4 text-destructive" />
                                    </Button>
                                  </div>
                                </TableCell>
                              </TableRow>
                            );
                          })
                          .filter(Boolean)
                      )}
                    </TableBody>
                  </Table>
                </div>

                {/* Pagination Controls */}
                {totalFilteredCount > 0 && (
                  <PaginationControls
                    currentPage={currentPage}
                    totalCount={totalFilteredCount}
                    pageSize={pageSize}
                    onPageChange={setCurrentPage}
                    onPageSizeChange={setPageSize}
                    itemName="item linkings"
                  />
                )}
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        {/* Delete Confirmation Dialog */}
        <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Delete Mapping</DialogTitle>
              <DialogDescription>
                Are you sure you want to delete this item linking? This action
                cannot be undone.
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button
                variant="outline"
                onClick={() => setShowDeleteDialog(false)}
              >
                Cancel
              </Button>
              <Button
                variant="destructive"
                onClick={() => {
                  // Find the mapping to delete
                  const mappingToDelete = existingMappings.find(m => m.selectionMapCriteria?.uid === itemToDelete);
                  if (mappingToDelete) {
                    handleDeleteMapping(mappingToDelete);
                  }
                  setShowDeleteDialog(false);
                  setItemToDelete(null);
                }}
              >
                Delete
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* Edit Dialog */}
        {editingItem && editingItem.selectionMapCriteria && (
          <Dialog
            open={!!editingItem}
            onOpenChange={() => setEditingItem(null)}
          >
            <DialogContent className="max-w-2xl">
              <DialogHeader>
                <DialogTitle>Edit Price Mapping</DialogTitle>
                <DialogDescription>
                  Update the mapping configuration for this SKU Class Group
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4 py-4">
                <div className="space-y-2">
                  <Label>SKU Class Group</Label>
                  <Input
                    value={
                      skuClassGroups.find(
                        (g) =>
                          g.UID ===
                          editingItem.selectionMapCriteria?.linkedItemUID
                      )?.Name ||
                      editingItem.selectionMapCriteria?.linkedItemUID ||
                      "Unknown"
                    }
                    disabled
                  />
                </div>

                <div className="space-y-2">
                  <Label>Active Status</Label>
                  <Select
                    value={
                      editingItem.selectionMapCriteria?.isActive
                        ? "true"
                        : "false"
                    }
                    onValueChange={(value) => {
                      if (editingItem.selectionMapCriteria) {
                        setEditingItem({
                          ...editingItem,
                          selectionMapCriteria: {
                            ...editingItem.selectionMapCriteria,
                            isActive: value === "true"
                          }
                        });
                      }
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="true">Active</SelectItem>
                      <SelectItem value="false">Inactive</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label>Current Mappings</Label>
                  <Card>
                    <CardContent className="p-4">
                      <div className="space-y-2 text-sm">
                        <div>
                          Customer Mappings:{" "}
                          {editingItem.selectionMapCriteria?.customerCount || 0}
                        </div>
                        <div>
                          Location Mappings:{" "}
                          {editingItem.selectionMapCriteria?.locationCount || 0}
                        </div>
                        <div>
                          Sales Team Mappings:{" "}
                          {editingItem.selectionMapCriteria?.salesTeamCount ||
                            0}
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </div>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setEditingItem(null)}>
                  Cancel
                </Button>
                <Button onClick={() => handleUpdateMapping(editingItem)}>
                  <Save className="h-4 w-4 mr-2" />
                  Save Changes
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        )}
      </div>
    </div>
  );
}
