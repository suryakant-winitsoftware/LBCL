"use client";

import React, { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Plus,
  Minus,
  Save,
  X,
  Edit2,
  Check,
  ChevronDown,
  ChevronUp,
  Search,
  Trash2,
} from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import { skuPriceService, ISKUPrice } from "@/services/sku/sku-price.service";
import { PagingRequest } from "@/types/common.types";
import { PaginationControls } from "@/components/ui/pagination-controls";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface PriceRow extends ISKUPrice {
  isEditing?: boolean;
  // Temporary fields for editing
  TempPrice?: number;
  TempDefaultWSPrice?: number;
  TempDefaultRetPrice?: number;
  TempValidFrom?: Date | string;
  TempValidUpto?: Date | string;
}

export default function TPPPriceTab() {
  const { toast } = useToast();
  const [prices, setPrices] = useState<PriceRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [editingRows, setEditingRows] = useState<Set<string>>(new Set());
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [priceToDelete, setPriceToDelete] = useState<PriceRow | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [newPriceData, setNewPriceData] = useState<Partial<PriceRow>>({});

  // Simple fetch - get individual price records with server-side pagination
  const fetchPrices = async () => {
    setLoading(true);
    try {
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: searchTerm
          ? [{ Name: "SKUCode", Value: searchTerm }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
        IsCountRequired: true,
      };
      
      const response = await skuPriceService.getAllSKUPrices(request);
      setPrices(response.PagedData || []);
      setTotalCount(response.TotalCount || 0);
      
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch prices",
        variant: "destructive",
      });
      console.error("Error fetching prices:", error);
    } finally {
      setLoading(false);
    }
  };
  
  // Helper function to group prices by SKU
  const groupPricesBySKU = (prices: ISKUPrice[]): TPPPriceRow[] => {
    const grouped: { [key: string]: TPPPriceRow[] } = {};
    
    prices.forEach((price) => {
      const key = price.SKUCode || "";
      if (!grouped[key]) {
        grouped[key] = [];
      }
      // Check for duplicates
      const exists = grouped[key].some(p => p.UID === price.UID);
      if (!exists) {
        grouped[key].push(price as TPPPriceRow);
      }
    });
    
    // Create hierarchical structure
    const hierarchical: TPPPriceRow[] = [];
    
    Object.entries(grouped).forEach(([, skuPrices]) => {
      if (skuPrices.length > 0) {
        // Sort by ValidFrom date
        const sorted = [...skuPrices].sort((a, b) => {
          const dateA = a.ValidFrom ? new Date(a.ValidFrom).getTime() : 0;
          const dateB = b.ValidFrom ? new Date(b.ValidFrom).getTime() : 0;
          return dateA - dateB;
        });
        
        // First is parent, rest are children
        const parent = { ...sorted[0] };
        parent.childRows = sorted.slice(1);
        hierarchical.push(parent);
      }
    });
    
    // Sort by SKU Code
    return hierarchical.sort((a, b) => 
      (a.SKUCode || "").localeCompare(b.SKUCode || "")
    );
  };

  // Fetch prices when page, search, or page size changes
  useEffect(() => {
    fetchPrices();
  }, [currentPage, pageSize]);

  // Handle search with debounce
  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      setCurrentPage(1); // Reset to first page on search
      fetchPrices();
    }, searchTerm ? 500 : 0);
    
    return () => clearTimeout(debounceTimer);
  }, [searchTerm]);

  // Toggle row expansion
  const toggleRowExpansion = (uid: string) => {
    const newExpanded = new Set(expandedRows);
    if (newExpanded.has(uid)) {
      newExpanded.delete(uid);
      // Clear new row data when collapsing
      const newData = { ...newRowData };
      delete newData[uid];
      setNewRowData(newData);
    } else {
      newExpanded.add(uid);
      // Initialize new row data with defaults
      const price = tppPrices.find((p) => p.UID === uid);
      if (price) {
        // Find the latest Valid To date among this SKU's prices (parent and all children)
        let latestValidUpto: Date | null = null;

        // Check parent price ValidUpto
        if (price.ValidUpto) {
          latestValidUpto = new Date(price.ValidUpto);
        }

        // Check all child rows for the latest ValidUpto
        if (price.childRows && price.childRows.length > 0) {
          price.childRows.forEach((child) => {
            if (child.ValidUpto) {
              const childValidUpto = new Date(child.ValidUpto);
              if (!latestValidUpto || childValidUpto > latestValidUpto) {
                latestValidUpto = childValidUpto;
              }
            }
          });
        }

        // Calculate the new Valid From date - day after the latest Valid To
        let newValidFrom: Date;
        if (latestValidUpto) {
          // Create a proper date for the day after latest Valid To
          const latestDate = new Date(latestValidUpto);
          newValidFrom = new Date(
            latestDate.getFullYear(),
            latestDate.getMonth(),
            latestDate.getDate() + 1,
            12,
            0,
            0 // Set to noon to avoid timezone issues
          );
        } else {
          // If no Valid To date exists anywhere, use today
          const today = new Date();
          newValidFrom = new Date(
            today.getFullYear(),
            today.getMonth(),
            today.getDate(),
            12,
            0,
            0 // Set to noon
          );
        }

        // Set Valid To to be 30 days after the new Valid From
        const newValidUpto = new Date(
          newValidFrom.getFullYear(),
          newValidFrom.getMonth(),
          newValidFrom.getDate() + 29, // 30 days total including start date
          12,
          0,
          0 // Set to noon
        );

        setNewRowData({
          ...newRowData,
          [uid]: {
            SKUCode: price.SKUCode,
            SKUName: price.SKUName,
            UOM: price.UOM || "EA",
            ValidFrom: newValidFrom,
            ValidUpto: newValidUpto,
            Price: price.Price || 0, // Copy current price as default
            DefaultWSPrice: price.DefaultWSPrice || 0, // Copy current wholesale price
            DefaultRetPrice: price.DefaultRetPrice || 0, // Copy current retail price
            IsActive: true,
            Status: "Active",
          },
        });
      }
    }
    setExpandedRows(newExpanded);
  };

  // Toggle row editing
  const toggleRowEditing = (uid: string) => {
    const newEditing = new Set(editingRows);
    if (newEditing.has(uid)) {
      newEditing.delete(uid);
    } else {
      newEditing.add(uid);
      // Copy current values to temp values for editing
      // First check in main prices
      let price = tppPrices.find((p) => p.UID === uid);

      // If not found in main prices, check in child rows
      if (!price) {
        for (const parentPrice of tppPrices) {
          if (parentPrice.childRows) {
            price = parentPrice.childRows.find((child) => child.UID === uid);
            if (price) break;
          }
        }
      }

      if (price) {
        price.TempPrice = price.Price;
        price.TempDefaultWSPrice = price.DefaultWSPrice || 0;
        price.TempDefaultRetPrice = price.DefaultRetPrice || 0;
        price.TempValidFrom = price.ValidFrom;
        price.TempValidUpto = price.ValidUpto;
      }
    }
    setEditingRows(newEditing);
  };

  // Save edited price
  const savePrice = async (price: TPPPriceRow) => {
    try {
      // Validate dates
      if (
        price.TempValidFrom &&
        price.TempValidUpto &&
        new Date(price.TempValidFrom) >= new Date(price.TempValidUpto)
      ) {
        toast({
          title: "Validation Error",
          description: "Valid From date must be before Valid To date",
          variant: "destructive",
        });
        return;
      }

      // Update the price object
      const updatedPrice: TPPPriceRow = {
        ...price,
        Price: price.TempPrice || 0,
        DefaultWSPrice: price.TempDefaultWSPrice || 0,
        DefaultRetPrice: price.TempDefaultRetPrice || 0,
        ValidFrom: price.TempValidFrom || price.ValidFrom,
        ValidUpto: price.TempValidUpto || price.ValidUpto,
        ModifiedTime: new Date(),
        ServerModifiedTime: new Date(),
      };

      await skuPriceService.updateSKUPrice(updatedPrice as ISKUPrice);

      toast({
        title: "Success",
        description: "Price updated successfully",
      });

      // Update local state - check if it's a parent or child row
      setTppPrices((prev) =>
        prev.map((p) => {
          if (p.UID === price.UID) {
            // It's a parent row
            return updatedPrice;
          } else if (p.childRows) {
            // Check if it's a child row
            const updatedChildRows = p.childRows.map((child) =>
              child.UID === price.UID ? updatedPrice : child
            );
            return { ...p, childRows: updatedChildRows };
          }
          return p;
        })
      );

      // Exit edit mode
      toggleRowEditing(price.UID!);
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to update price",
        variant: "destructive",
      });
    }
  };

  // Cancel editing
  const cancelEdit = (uid: string) => {
    toggleRowEditing(uid);
  };

  // Save new price entry
  const saveNewPriceEntry = async (parentPrice: TPPPriceRow) => {
    const newData = newRowData[parentPrice.UID!];
    if (!newData) return;

    try {
      // Check if parent price has required fields
      if (!parentPrice.CreatedBy) {
        console.warn(
          "Parent price missing CreatedBy, using first available price data"
        );
        // Try to get CreatedBy from any existing price
        if (tppPrices.length > 0 && tppPrices[0].CreatedBy) {
          parentPrice.CreatedBy = tppPrices[0].CreatedBy;
        }
      }
      // Validate required fields
      if (!newData.Price || newData.Price <= 0) {
        toast({
          title: "Validation Error",
          description: "Please enter a valid cost price",
          variant: "destructive",
        });
        return;
      }

      // Validate dates
      if (
        newData.ValidFrom &&
        newData.ValidUpto &&
        new Date(newData.ValidFrom) >= new Date(newData.ValidUpto)
      ) {
        toast({
          title: "Validation Error",
          description: "Valid From date must be before Valid To date",
          variant: "destructive",
        });
        return;
      }

      // Find the latest Valid To date among existing prices for this SKU
      let latestValidUpto: Date | null = null;
      if (parentPrice.ValidUpto) {
        latestValidUpto = new Date(parentPrice.ValidUpto);
      }
      if (parentPrice.childRows && parentPrice.childRows.length > 0) {
        parentPrice.childRows.forEach((child) => {
          if (child.ValidUpto) {
            const childValidUpto = new Date(child.ValidUpto);
            if (!latestValidUpto || childValidUpto > latestValidUpto) {
              latestValidUpto = childValidUpto;
            }
          }
        });
      }

      // Ensure new price starts after the latest existing price
      if (latestValidUpto && newData.ValidFrom) {
        const newValidFrom = new Date(newData.ValidFrom);
        newValidFrom.setHours(0, 0, 0, 0); // Reset time to start of day

        const minAllowedDate = new Date(latestValidUpto);
        minAllowedDate.setDate(minAllowedDate.getDate() + 1);
        minAllowedDate.setHours(0, 0, 0, 0); // Reset time to start of day

        if (newValidFrom < minAllowedDate) {
          toast({
            title: "Validation Error",
            description: `New price must start on or after ${formatDateToDayMonthYear(minAllowedDate)}. Prices cannot overlap.`,
            variant: "destructive",
          });
          return;
        }
      }

      // Generate a proper GUID format for UID
      const generateUID = () => {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
          /[xy]/g,
          function (c) {
            const r = (Math.random() * 16) | 0;
            const v = c === "x" ? r : (r & 0x3) | 0x8;
            return v.toString(16);
          }
        );
      };

      // Create a new price entry based on parent price data
      const newPrice: ISKUPrice = {
        UID: generateUID(),
        SKUCode: parentPrice.SKUCode,
        SKUName: parentPrice.SKUName,
        SKUPriceListUID: parentPrice.SKUPriceListUID || "DefaultPriceList",
        SKUUID: parentPrice.SKUUID, // This should be the actual SKU UID from parent
        UOM: newData.UOM || parentPrice.UOM || "EA",
        Price: newData.Price || 0,
        DefaultWSPrice: newData.DefaultWSPrice || 0,
        DefaultRetPrice: newData.DefaultRetPrice || 0,
        DummyPrice: newData.Price || 0,
        MRP: newData.MRP || newData.Price || 0, // Use MRP if provided, else use Price
        PriceUpperLimit: parentPrice.PriceUpperLimit || 0,
        PriceLowerLimit: parentPrice.PriceLowerLimit || 0,
        ValidFrom: newData.ValidFrom || new Date(),
        ValidUpto:
          newData.ValidUpto ||
          new Date(new Date().setMonth(new Date().getMonth() + 1)),
        Status: "Active",
        IsActive: true,
        IsTaxIncluded: parentPrice.IsTaxIncluded || false,
        CreatedBy: parentPrice.CreatedBy || undefined, // Use existing CreatedBy from parent price
        CreatedTime: new Date().toISOString(),
        ModifiedBy: parentPrice.CreatedBy || undefined, // Use same as CreatedBy
        ModifiedTime: new Date().toISOString(),
        IsLatest: 1,
        VersionNo: parentPrice.VersionNo || "1.0",
        LadderingAmount: 0,
        LadderingPercentage: 0,
      };

      console.log("Creating new price with data:", newPrice); // Debug log

      await skuPriceService.createSKUPrice(newPrice);

      toast({
        title: "Success",
        description: "New price entry created successfully",
      });

      // Refresh data
      await fetchPrices(true);

      // Collapse the row
      toggleRowExpansion(parentPrice.UID!);
    } catch (error: any) {
      console.error("Error creating price:", error);
      toast({
        title: "Error",
        description: error?.message || "Failed to create new price entry",
        variant: "destructive",
      });
    }
  };

  // Update new row data
  const updateNewRowData = (parentUid: string, field: string, value: any) => {
    setNewRowData((prev) => ({
      ...prev,
      [parentUid]: {
        ...prev[parentUid],
        [field]: value,
      },
    }));
  };

  // Update temp values for editing
  const updateTempValue = (uid: string, field: string, value: any) => {
    setTppPrices((prev) =>
      prev.map((price) => {
        if (price.UID === uid) {
          return {
            ...price,
            [field]: value,
          };
        }
        return price;
      })
    );
  };

  // Delete price functions
  const confirmDelete = (price: TPPPriceRow) => {
    setPriceToDelete(price);
    setDeleteDialogOpen(true);
  };

  const handleDelete = async () => {
    if (!priceToDelete) return;

    try {
      await skuPriceService.deleteSKUPrice(priceToDelete.UID!);

      toast({
        title: "Success",
        description: "Price deleted successfully",
      });

      // Update local state - remove from parent or child rows
      setTppPrices((prev) => {
        // First check if it's a parent row
        const filteredPrices = prev.filter((p) => p.UID !== priceToDelete.UID);

        // If same length, it wasn't a parent, so check child rows
        if (filteredPrices.length === prev.length) {
          return prev.map((p) => {
            if (p.childRows) {
              const filteredChildren = p.childRows.filter(
                (child) => child.UID !== priceToDelete.UID
              );
              return { ...p, childRows: filteredChildren };
            }
            return p;
          });
        }

        return filteredPrices;
      });

      // Remove from local state after successful delete
      setAllGroupedPrices((prev) => {
        // Filter out the deleted price from parent or child rows
        return prev.map((p) => {
          if (p.UID === priceToDelete.UID) {
            // Remove parent row entirely
            return null;
          } else if (p.childRows) {
            // Remove from child rows
            const filteredChildren = p.childRows.filter(
              (child) => child.UID !== priceToDelete.UID
            );
            return { ...p, childRows: filteredChildren };
          }
          return p;
        }).filter(Boolean) as TPPPriceRow[];
      });
      
      // Refresh data from server to be sure
      await fetchPrices(true);

      setDeleteDialogOpen(false);
      setPriceToDelete(null);
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete price",
        variant: "destructive",
      });
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount || 0);
  };

  const formatDate = (date: Date | string | undefined) => {
    return formatDateToDayMonthYear(date, "-");
  };

  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page on search
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-semibold">Price Management</h3>
        <div className="flex gap-2">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by SKU code or name..."
              value={searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10 w-64"
            />
          </div>
          <Button
            onClick={() => {
              // Add logic to create completely new SKU price
              toast({
                title: "Info",
                description:
                  "Use the + button on existing rows to add date-based pricing",
              });
            }}
          >
            <Plus className="h-4 w-4 mr-2" />
            Add New SKU Price
          </Button>
        </div>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-12"></TableHead>
              <TableHead className="w-28">SKU Code</TableHead>
              <TableHead className="w-48 max-w-xs">SKU Name</TableHead>
              <TableHead className="w-32">Valid From</TableHead>
              <TableHead className="w-32">Valid To</TableHead>
              <TableHead className="w-20">UOM</TableHead>
              <TableHead className="w-28">Cost Price</TableHead>
              <TableHead className="w-28">A.Retail Price</TableHead>
              <TableHead className="w-24 text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={9} className="text-center py-8">
                  <div className="flex justify-center">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                  </div>
                </TableCell>
              </TableRow>
            ) : tppPrices.length === 0 ? (
              <TableRow>
                <TableCell colSpan={9} className="text-center py-8">
                  No prices found
                </TableCell>
              </TableRow>
            ) : (
              <>
                {tppPrices.map((price) => (
                  <React.Fragment key={price.UID}>
                    {/* Main Row */}
                    <TableRow
                      className={
                        editingRows.has(price.UID!) ? "bg-blue-50" : ""
                      }
                    >
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-6 w-6"
                          onClick={() => toggleRowExpansion(price.UID!)}
                        >
                          {expandedRows.has(price.UID!) ? (
                            <Minus className="h-4 w-4" />
                          ) : (
                            <Plus className="h-4 w-4" />
                          )}
                        </Button>
                      </TableCell>
                      <TableCell className="font-medium">
                        {price.SKUCode}
                      </TableCell>
                      <TableCell
                        className="max-w-xs truncate"
                        title={price.SKUName || "-"}
                      >
                        {price.SKUName || "-"}
                      </TableCell>
                      <TableCell>
                        {editingRows.has(price.UID!) ? (
                          <Input
                            type="date"
                            value={
                              price.TempValidFrom
                                ? new Date(price.TempValidFrom)
                                    .toISOString()
                                    .split("T")[0]
                                : ""
                            }
                            onChange={(e) => {
                              const newValidFrom = new Date(e.target.value);

                              // Check against temp Valid To if exists, otherwise use original
                              const validUpto =
                                price.TempValidUpto || price.ValidUpto;
                              if (
                                validUpto &&
                                newValidFrom >= new Date(validUpto)
                              ) {
                                toast({
                                  title: "Validation Error",
                                  description:
                                    "Valid From date must be before Valid To date",
                                  variant: "destructive",
                                });
                                return;
                              }

                              updateTempValue(
                                price.UID!,
                                "TempValidFrom",
                                e.target.value
                              );
                            }}
                            className="w-32"
                            max={
                              price.TempValidUpto
                                ? new Date(
                                    new Date(price.TempValidUpto).getTime() -
                                      86400000
                                  )
                                    .toISOString()
                                    .split("T")[0]
                                : ""
                            }
                          />
                        ) : (
                          formatDate(price.ValidFrom)
                        )}
                      </TableCell>
                      <TableCell>
                        {editingRows.has(price.UID!) ? (
                          <Input
                            type="date"
                            value={
                              price.TempValidUpto
                                ? new Date(price.TempValidUpto)
                                    .toISOString()
                                    .split("T")[0]
                                : ""
                            }
                            onChange={(e) => {
                              const newValidUpto = new Date(e.target.value);

                              // Check against temp Valid From if exists, otherwise use original
                              const validFrom =
                                price.TempValidFrom || price.ValidFrom;
                              if (
                                validFrom &&
                                newValidUpto <= new Date(validFrom)
                              ) {
                                toast({
                                  title: "Validation Error",
                                  description:
                                    "Valid To date must be after Valid From date",
                                  variant: "destructive",
                                });
                                return;
                              }

                              updateTempValue(
                                price.UID!,
                                "TempValidUpto",
                                e.target.value
                              );
                            }}
                            className="w-32"
                            min={
                              price.TempValidFrom
                                ? new Date(
                                    new Date(price.TempValidFrom).getTime() +
                                      86400000
                                  )
                                    .toISOString()
                                    .split("T")[0]
                                : ""
                            }
                          />
                        ) : (
                          formatDate(price.ValidUpto)
                        )}
                      </TableCell>
                      <TableCell>{price.UOM || "EA"}</TableCell>
                      <TableCell>
                        {editingRows.has(price.UID!) ? (
                          <Input
                            type="number"
                            value={price.TempPrice || 0}
                            onChange={(e) =>
                              updateTempValue(
                                price.UID!,
                                "TempPrice",
                                parseFloat(e.target.value)
                              )
                            }
                            className="w-24"
                            step="0.01"
                          />
                        ) : (
                          formatCurrency(price.Price)
                        )}
                      </TableCell>
                      <TableCell>
                        {editingRows.has(price.UID!) ? (
                          <Input
                            type="number"
                            value={price.TempDefaultRetPrice || 0}
                            onChange={(e) =>
                              updateTempValue(
                                price.UID!,
                                "TempDefaultRetPrice",
                                parseFloat(e.target.value)
                              )
                            }
                            className="w-24"
                            step="0.01"
                          />
                        ) : (
                          formatCurrency(price.DefaultRetPrice || 0)
                        )}
                      </TableCell>
                      <TableCell>
                        <div className="flex justify-end gap-2">
                          {editingRows.has(price.UID!) ? (
                            <>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-green-600"
                                onClick={() => savePrice(price)}
                              >
                                <Check className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-red-600"
                                onClick={() => cancelEdit(price.UID!)}
                              >
                                <X className="h-4 w-4" />
                              </Button>
                            </>
                          ) : (
                            <>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8"
                                onClick={() => toggleRowEditing(price.UID!)}
                              >
                                <Edit2 className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-red-600"
                                onClick={() => confirmDelete(price)}
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            </>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>

                    {/* Expanded Row for New Entry */}
                    {expandedRows.has(price.UID!) && (
                      <TableRow
                        key={`${price.UID}_expanded`}
                        className="bg-green-50"
                      >
                        <TableCell></TableCell>
                        <TableCell className="font-medium">
                          {price.SKUCode}
                        </TableCell>
                        <TableCell
                          className="max-w-xs truncate"
                          title={price.SKUName || "-"}
                        >
                          {price.SKUName || "-"}
                        </TableCell>
                        <TableCell>
                          <Input
                            type="date"
                            value={(() => {
                              if (!newRowData[price.UID!]?.ValidFrom) return "";
                              const date = new Date(
                                newRowData[price.UID!].ValidFrom!
                              );
                              const year = date.getFullYear();
                              const month = String(
                                date.getMonth() + 1
                              ).padStart(2, "0");
                              const day = String(date.getDate()).padStart(
                                2,
                                "0"
                              );
                              return `${year}-${month}-${day}`;
                            })()}
                            onChange={(e) => {
                              // Parse the date string properly to avoid timezone issues
                              const [year, month, day] = e.target.value
                                .split("-")
                                .map(Number);
                              const newValidFrom = new Date(
                                year,
                                month - 1,
                                day,
                                12,
                                0,
                                0
                              ); // Set to noon to avoid timezone issues
                              const validUpto =
                                newRowData[price.UID!]?.ValidUpto;

                              // Find the latest Valid To date among existing prices
                              let latestValidUpto: Date | null = null;
                              if (price.ValidUpto) {
                                latestValidUpto = new Date(price.ValidUpto);
                              }
                              if (
                                price.childRows &&
                                price.childRows.length > 0
                              ) {
                                price.childRows.forEach((child) => {
                                  if (child.ValidUpto) {
                                    const childValidUpto = new Date(
                                      child.ValidUpto
                                    );
                                    if (
                                      !latestValidUpto ||
                                      childValidUpto > latestValidUpto
                                    ) {
                                      latestValidUpto = childValidUpto;
                                    }
                                  }
                                });
                              }

                              // Validate that new Valid From is not overlapping with existing dates
                              if (latestValidUpto) {
                                // Calculate the minimum allowed date (day after latest Valid To)
                                const minAllowedDate = new Date(
                                  latestValidUpto
                                );
                                minAllowedDate.setDate(
                                  minAllowedDate.getDate() + 1
                                );
                                minAllowedDate.setHours(0, 0, 0, 0); // Reset time to start of day

                                const newValidFromDate = new Date(newValidFrom);
                                newValidFromDate.setHours(0, 0, 0, 0); // Reset time to start of day

                                if (newValidFromDate < minAllowedDate) {
                                  toast({
                                    title: "Validation Error",
                                    description: `Valid From date must be on or after ${formatDateToDayMonthYear(minAllowedDate)}`,
                                    variant: "destructive",
                                  });
                                  return;
                                }
                              }

                              // Validate that Valid From is before Valid To
                              if (
                                validUpto &&
                                newValidFrom >= new Date(validUpto)
                              ) {
                                toast({
                                  title: "Validation Error",
                                  description:
                                    "Valid From date must be before Valid To date",
                                  variant: "destructive",
                                });
                                return;
                              }

                              updateNewRowData(
                                price.UID!,
                                "ValidFrom",
                                newValidFrom
                              );
                            }}
                            className="w-32"
                            min={(() => {
                              // Find the latest Valid To date to set as minimum
                              let latestValidUpto: Date | null = null;
                              if (price.ValidUpto) {
                                latestValidUpto = new Date(price.ValidUpto);
                              }
                              if (
                                price.childRows &&
                                price.childRows.length > 0
                              ) {
                                price.childRows.forEach((child) => {
                                  if (child.ValidUpto) {
                                    const childValidUpto = new Date(
                                      child.ValidUpto
                                    );
                                    if (
                                      !latestValidUpto ||
                                      childValidUpto > latestValidUpto
                                    ) {
                                      latestValidUpto = childValidUpto;
                                    }
                                  }
                                });
                              }
                              return latestValidUpto
                                ? new Date(latestValidUpto.getTime() + 86400000)
                                    .toISOString()
                                    .split("T")[0]
                                : "";
                            })()}
                            max={
                              newRowData[price.UID!]?.ValidUpto
                                ? new Date(
                                    new Date(
                                      newRowData[price.UID!].ValidUpto!
                                    ).getTime() - 86400000
                                  )
                                    .toISOString()
                                    .split("T")[0]
                                : ""
                            }
                          />
                        </TableCell>
                        <TableCell>
                          <Input
                            type="date"
                            value={(() => {
                              if (!newRowData[price.UID!]?.ValidUpto) return "";
                              const date = new Date(
                                newRowData[price.UID!].ValidUpto!
                              );
                              const year = date.getFullYear();
                              const month = String(
                                date.getMonth() + 1
                              ).padStart(2, "0");
                              const day = String(date.getDate()).padStart(
                                2,
                                "0"
                              );
                              return `${year}-${month}-${day}`;
                            })()}
                            onChange={(e) => {
                              // Parse the date string properly to avoid timezone issues
                              const [year, month, day] = e.target.value
                                .split("-")
                                .map(Number);
                              const newValidUpto = new Date(
                                year,
                                month - 1,
                                day,
                                12,
                                0,
                                0
                              ); // Set to noon to avoid timezone issues
                              const validFrom =
                                newRowData[price.UID!]?.ValidFrom;

                              // Validate that Valid To is after Valid From
                              if (
                                validFrom &&
                                newValidUpto <= new Date(validFrom)
                              ) {
                                toast({
                                  title: "Validation Error",
                                  description:
                                    "Valid To date must be after Valid From date",
                                  variant: "destructive",
                                });
                                return;
                              }

                              updateNewRowData(
                                price.UID!,
                                "ValidUpto",
                                newValidUpto
                              );
                            }}
                            className="w-32"
                            min={
                              newRowData[price.UID!]?.ValidFrom
                                ? new Date(
                                    new Date(
                                      newRowData[price.UID!].ValidFrom!
                                    ).getTime() + 86400000
                                  )
                                    .toISOString()
                                    .split("T")[0]
                                : ""
                            }
                          />
                        </TableCell>
                        <TableCell>{price.UOM || "EA"}</TableCell>
                        <TableCell>
                          <Input
                            type="number"
                            value={newRowData[price.UID!]?.Price || ""}
                            onChange={(e) =>
                              updateNewRowData(
                                price.UID!,
                                "Price",
                                parseFloat(e.target.value)
                              )
                            }
                            className="w-24"
                            step="0.01"
                            placeholder="0.00"
                          />
                        </TableCell>
                        <TableCell>
                          <Input
                            type="number"
                            value={
                              newRowData[price.UID!]?.DefaultRetPrice || ""
                            }
                            onChange={(e) =>
                              updateNewRowData(
                                price.UID!,
                                "DefaultRetPrice",
                                parseFloat(e.target.value)
                              )
                            }
                            className="w-24"
                            step="0.01"
                            placeholder="0.00"
                          />
                        </TableCell>
                        <TableCell>
                          <div className="flex justify-end gap-2">
                            <Button
                              variant="ghost"
                              size="icon"
                              className="h-8 w-8 text-green-600"
                              onClick={() => saveNewPriceEntry(price)}
                            >
                              <Save className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              className="h-8 w-8 text-red-600"
                              onClick={() => toggleRowExpansion(price.UID!)}
                            >
                              <X className="h-4 w-4" />
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    )}

                    {/* Child Rows (Additional price entries for same SKU) */}
                    {price.childRows?.map((childPrice) =>
                      editingRows.has(childPrice.UID!) ? (
                        // Editing mode for child row
                        <TableRow key={childPrice.UID} className="bg-blue-50">
                          <TableCell></TableCell>
                          <TableCell className="font-medium">
                            {childPrice.SKUCode}
                          </TableCell>
                          <TableCell
                            className="max-w-xs truncate"
                            title={childPrice.SKUName || "-"}
                          >
                            {childPrice.SKUName || "-"}
                          </TableCell>
                          <TableCell>
                            <Input
                              type="date"
                              value={
                                childPrice.TempValidFrom
                                  ? new Date(childPrice.TempValidFrom)
                                      .toISOString()
                                      .split("T")[0]
                                  : new Date(childPrice.ValidFrom || "")
                                      .toISOString()
                                      .split("T")[0]
                              }
                              onChange={(e) => {
                                childPrice.TempValidFrom = e.target.value;
                                setTppPrices([...tppPrices]);
                              }}
                              className="w-32"
                            />
                          </TableCell>
                          <TableCell>
                            <Input
                              type="date"
                              value={
                                childPrice.TempValidUpto
                                  ? new Date(childPrice.TempValidUpto)
                                      .toISOString()
                                      .split("T")[0]
                                  : new Date(childPrice.ValidUpto || "")
                                      .toISOString()
                                      .split("T")[0]
                              }
                              onChange={(e) => {
                                childPrice.TempValidUpto = e.target.value;
                                setTppPrices([...tppPrices]);
                              }}
                              className="w-32"
                              min={
                                childPrice.TempValidFrom
                                  ? new Date(
                                      new Date(
                                        childPrice.TempValidFrom
                                      ).getTime() + 86400000
                                    )
                                      .toISOString()
                                      .split("T")[0]
                                  : ""
                              }
                            />
                          </TableCell>
                          <TableCell>{childPrice.UOM || "EA"}</TableCell>
                          <TableCell>
                            <Input
                              type="number"
                              value={childPrice.TempPrice || 0}
                              onChange={(e) => {
                                childPrice.TempPrice = parseFloat(
                                  e.target.value
                                );
                                setTppPrices([...tppPrices]);
                              }}
                              className="w-24"
                              step="0.01"
                            />
                          </TableCell>
                          <TableCell>
                            <Input
                              type="number"
                              value={childPrice.TempDefaultRetPrice || 0}
                              onChange={(e) => {
                                childPrice.TempDefaultRetPrice = parseFloat(
                                  e.target.value
                                );
                                setTppPrices([...tppPrices]);
                              }}
                              className="w-24"
                              step="0.01"
                            />
                          </TableCell>
                          <TableCell>
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-green-600"
                                onClick={() => savePrice(childPrice)}
                              >
                                <Check className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-red-600"
                                onClick={() => cancelEdit(childPrice.UID!)}
                              >
                                <X className="h-4 w-4" />
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ) : (
                        // Display mode for child row
                        <TableRow key={childPrice.UID}>
                          <TableCell></TableCell>
                          <TableCell className="font-medium">
                            {childPrice.SKUCode}
                          </TableCell>
                          <TableCell
                            className="max-w-xs truncate"
                            title={childPrice.SKUName || "-"}
                          >
                            {childPrice.SKUName || "-"}
                          </TableCell>
                          <TableCell>
                            {formatDate(childPrice.ValidFrom)}
                          </TableCell>
                          <TableCell>
                            {formatDate(childPrice.ValidUpto)}
                          </TableCell>
                          <TableCell>{childPrice.UOM || "EA"}</TableCell>
                          <TableCell>
                            {formatCurrency(childPrice.Price)}
                          </TableCell>
                          <TableCell>
                            {formatCurrency(childPrice.DefaultRetPrice || 0)}
                          </TableCell>
                          <TableCell>
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8"
                                onClick={() =>
                                  toggleRowEditing(childPrice.UID!)
                                }
                              >
                                <Edit2 className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-red-600"
                                onClick={() => confirmDelete(childPrice)}
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      )
                    )}
                  </React.Fragment>
                ))}
              </>
            )}
          </TableBody>
        </Table>
      </div>

      {totalCount > 0 && (
        <div className="space-y-2">
          <PaginationControls
            currentPage={currentPage}
            totalCount={totalCount}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
            onPageSizeChange={setPageSize}
            itemName="unique SKUs"
          />
          <p className="text-sm text-muted-foreground text-center">
            Showing unique SKUs. Each SKU may have multiple price entries with different validity periods.
          </p>
        </div>
      )}

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the
              price entry
              {priceToDelete && (
                <span className="font-semibold">
                  {" "}
                  for SKU {priceToDelete.SKUCode} (Valid from{" "}
                  {formatDate(priceToDelete.ValidFrom)} to{" "}
                  {formatDate(priceToDelete.ValidUpto)})
                </span>
              )}
              .
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setPriceToDelete(null)}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-red-600 hover:bg-red-700"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
