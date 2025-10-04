"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
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
import { DataTable } from "@/components/ui/data-table";
import { useToast } from "@/components/ui/use-toast";
import {
  Plus,
  Search,
  RefreshCw,
  Package,
  Building2,
  ExternalLink,
  Trash2
} from "lucide-react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger
} from "@/components/ui/tooltip";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from "@/components/ui/alert-dialog";
import {
  skuClassGroupsService,
  SKUClassGroup
} from "@/services/sku-class-groups.service";
import {
  skuClassGroupItemsService,
  SKUClassGroupItem
} from "@/services/sku-class-group-items.service";

export default function GroupItemsPage() {
  const router = useRouter();
  const { toast } = useToast();

  // Data states for Items
  const [classGroupItems, setClassGroupItems] = useState<
    (SKUClassGroupItem & {
      SKUClassGroupName?: string;
      SKUName?: string;
      SupplierOrgName?: string;
    })[]
  >([]);
  const [filteredItems, setFilteredItems] = useState<
    (SKUClassGroupItem & {
      SKUClassGroupName?: string;
      SKUName?: string;
      SupplierOrgName?: string;
    })[]
  >([]);
  const [loadingItems, setLoadingItems] = useState(false);
  const [totalItemsCount, setTotalItemsCount] = useState(0);
  const [currentItemsPage, setCurrentItemsPage] = useState(1);
  const [selectedGroupForItems, setSelectedGroupForItems] = useState<string>("");
  const [pageSize] = useState(20);

  // Data states for Groups (for filter dropdown)
  const [classGroups, setClassGroups] = useState<SKUClassGroup[]>([]);

  // Form states for Items
  const [deleteItemDialogOpen, setDeleteItemDialogOpen] = useState(false);
  const [deletingItem, setDeletingItem] = useState<SKUClassGroupItem | null>(null);

  // Filter states
  const [searchItemTerm, setSearchItemTerm] = useState("");

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    loadClassGroupItems();
  }, [currentItemsPage, searchItemTerm, selectedGroupForItems]);

  useEffect(() => {
    applyItemFilters();
  }, [classGroupItems, searchItemTerm]);

  const loadInitialData = async () => {
    try {
      // Load class groups for the filter dropdown
      const groupsResult = await skuClassGroupsService.getAllSKUClassGroups(1, 1000, "");
      setClassGroups(groupsResult.data);
    } catch (error) {
      console.error("Failed to load dropdown data:", error);
    }
  };

  const loadClassGroupItems = async () => {
    if (loadingItems) return; // Prevent duplicate calls
    try {
      setLoadingItems(true);

      // Use the View endpoint to get items with joined data
      const result = await skuClassGroupItemsService.getAllSKUClassGroupItemsView(
        currentItemsPage,
        pageSize,
        searchItemTerm,
        selectedGroupForItems || undefined
      );

      // The View endpoint already provides SKUName and PlantName
      const itemsWithView = result.data.map((item) => ({
        ...item,
        SKUClassGroupName:
          classGroups.find((g) => g.UID === item.SKUClassGroupUID)?.Name ||
          item.SKUClassGroupUID,
        SupplierOrgName:
          item.PlantName || (item.SupplierOrgUID ? item.SupplierOrgUID : "N/A"),
        ModelUoM: item.ModelUoM || "N/A",
        DailyCutOffTime: item.DailyCutOffTime || "N/A"
      }));

      setClassGroupItems(itemsWithView as any);
      setTotalItemsCount(result.totalCount);
    } catch (error) {
      console.error("Error loading items:", error);
      toast({
        title: "Error",
        description: "Failed to load SKU Class Group Items",
        variant: "destructive"
      });
    } finally {
      setLoadingItems(false);
    }
  };

  const applyItemFilters = () => {
    let filtered = [...classGroupItems];

    if (searchItemTerm) {
      filtered = filtered.filter(
        (item) =>
          item.SKUCode.toLowerCase().includes(searchItemTerm.toLowerCase()) ||
          item.SKUName?.toLowerCase().includes(searchItemTerm.toLowerCase())
      );
    }

    setFilteredItems(filtered);
  };

  const handleCreateItem = () => {
    const params = new URLSearchParams();
    if (selectedGroupForItems) {
      params.set("groupUID", selectedGroupForItems);
    }
    router.push(
      `/productssales/product-management/group-items/add?${params.toString()}`
    );
  };

  const handleEditItem = (item: SKUClassGroupItem) => {
    router.push(
      `/productssales/product-management/group-items/add?uid=${item.UID}`
    );
  };

  const handleDeleteItem = (item: SKUClassGroupItem) => {
    setDeletingItem(item);
    setDeleteItemDialogOpen(true);
  };

  const confirmDeleteItem = async () => {
    if (!deletingItem) return;

    try {
      await skuClassGroupItemsService.deleteSKUClassGroupItem(deletingItem.UID);
      toast({
        title: "Success",
        description: "SKU Class Group Item deleted successfully"
      });

      setDeleteItemDialogOpen(false);
      setDeletingItem(null);
      loadClassGroupItems();
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "Failed to delete SKU Class Group Item",
        variant: "destructive"
      });
    }
  };

  // Table columns for Items
  const itemColumns = [
    {
      accessorKey: "SKUCode",
      header: "SKU Code",
      cell: ({ row }: any) => (
        <div className="flex items-center gap-2">
          <Package className="h-4 w-4 text-primary" />
          <div>
            <div className="font-medium">{row.original.SKUCode}</div>
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  <div className="text-sm text-muted-foreground truncate max-w-[200px] cursor-help">
                    {row.original.SKUName || "N/A"}
                  </div>
                </TooltipTrigger>
                <TooltipContent>
                  <p className="max-w-xs">{row.original.SKUName || "N/A"}</p>
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>
          </div>
        </div>
      )
    },
    {
      accessorKey: "SKUClassGroupName",
      header: "Class Group",
      cell: ({ row }: any) => (
        <Badge variant="outline">
          {row.original.SKUClassGroupName || row.original.SKUClassGroupUID}
        </Badge>
      )
    },
    {
      accessorKey: "SerialNumber",
      header: "Serial #",
      cell: ({ row }: any) => (
        <div className="text-center">{row.original.SerialNumber}</div>
      )
    },
    {
      accessorKey: "ModelQty",
      header: "Model Qty",
      cell: ({ row }: any) => (
        <div className="flex items-center gap-1">
          <span className="font-medium">{row.original.ModelQty || 0}</span>
          <Badge variant="secondary" className="text-xs">
            {row.original.ModelUoM}
          </Badge>
        </div>
      )
    },
    {
      accessorKey: "SupplierOrgName",
      header: "Supplier",
      cell: ({ row }: any) => (
        <div className="flex items-center gap-2">
          <Building2 className="h-4 w-4 text-muted-foreground" />
          <span>{row.original.SupplierOrgName}</span>
        </div>
      )
    },
    {
      accessorKey: "MinMaxQty",
      header: "Min/Max Qty",
      cell: ({ row }: any) => (
        <div className="text-sm">
          <span className="text-muted-foreground">Min:</span>{" "}
          {row.original.MinQTY || 0} |
          <span className="text-muted-foreground"> Max:</span>{" "}
          {row.original.MaxQTY || 0}
        </div>
      )
    },
    {
      accessorKey: "IsExclusive",
      header: "Exclusive",
      cell: ({ row }: any) => (
        <Badge variant={row.original.IsExclusive ? "default" : "secondary"}>
          {row.original.IsExclusive ? "Yes" : "No"}
        </Badge>
      )
    },
    {
      id: "actions",
      header: "Actions",
      cell: ({ row }: any) => (
        <div className="flex gap-2">
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleEditItem(row.original)}
                >
                  <ExternalLink className="h-4 w-4" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>
                <p>Edit item in new page</p>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleDeleteItem(row.original)}
            className="text-destructive hover:text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      )
    }
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            SKU Group Items
          </h1>
          <p className="text-muted-foreground">
            Manage individual SKU items within class groups
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={loadClassGroupItems}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button onClick={handleCreateItem}>
            <Plus className="h-4 w-4 mr-2" />
            Add SKU Item
          </Button>
        </div>
      </div>

      {/* Filters for Items */}
      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label>Search SKU</Label>
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by SKU code or name..."
                  value={searchItemTerm}
                  onChange={(e) => setSearchItemTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <div>
              <Label>Class Group</Label>
              <Select
                value={selectedGroupForItems || "all"}
                onValueChange={(value) =>
                  setSelectedGroupForItems(value === "all" ? "" : value)
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="All Groups" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Groups</SelectItem>
                  {classGroups.map((group) => (
                    <SelectItem key={group.UID} value={group.UID}>
                      {group.Name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Results for Items */}
      <Card>
        <CardHeader>
          <CardTitle>
            Group Items ({filteredItems.length} of {totalItemsCount})
          </CardTitle>
          <CardDescription>
            Showing {filteredItems.length} SKU class group items
          </CardDescription>
        </CardHeader>
        <CardContent>
          {loadingItems ? (
            <div className="flex items-center justify-center h-32">
              <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full"></div>
            </div>
          ) : filteredItems.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-32 text-center">
              <Package className="h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-lg font-medium">No Items Found</p>
              <p className="text-sm text-muted-foreground mt-2">
                {searchItemTerm || selectedGroupForItems
                  ? "Try adjusting your filters or search criteria"
                  : 'Click "Add SKU Item" to create your first SKU Class Group Item'}
              </p>
            </div>
          ) : (
            <DataTable columns={itemColumns} data={filteredItems} />
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog for Items */}
      <AlertDialog
        open={deleteItemDialogOpen}
        onOpenChange={setDeleteItemDialogOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Item</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete `{deletingItem?.SKUCode}`? This
              action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeletingItem(null)}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmDeleteItem}
              className="bg-destructive text-destructive-foreground"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}