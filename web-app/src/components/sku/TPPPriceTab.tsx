"use client";

import React, { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Edit2,
  Trash2,
  Search,
  Plus,
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
import { Skeleton } from "@/components/ui/skeleton";
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
  isNew?: boolean; // Flag for new rows being added
  // Temporary fields for editing
  TempPrice?: number;
  TempDefaultRetPrice?: number;
  TempValidFrom?: Date | string;
  TempValidUpto?: Date | string;
  TempSKUCode?: string;
  TempSKUName?: string;
  TempSKUPriceListUID?: string;
  TempUOM?: string;
  TempMRP?: number;
  TempStatus?: string;
}

export default function TPPPriceTab() {
  const { toast } = useToast();
  
  // Simple state - just show individual price records
  const [prices, setPrices] = useState<PriceRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [priceToDelete, setPriceToDelete] = useState<PriceRow | null>(null);
  const [editingRows, setEditingRows] = useState<Set<string>>(new Set());
  const [newRowCounter, setNewRowCounter] = useState(0); // Counter for generating unique IDs for new rows

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

  const handleSearch = (value: string) => {
    setSearchTerm(value);
  };

  // Start editing a price record
  const startEditing = (priceUID: string) => {
    setEditingRows(prev => new Set([...prev, priceUID]));
  };

  // Cancel editing
  const cancelEditing = (priceUID: string) => {
    setEditingRows(prev => {
      const newSet = new Set(prev);
      newSet.delete(priceUID);
      return newSet;
    });
    // Reset temp values
    setPrices(prev => prev.map(p => 
      p.UID === priceUID 
        ? { ...p, TempPrice: undefined, TempDefaultRetPrice: undefined, TempValidFrom: undefined, TempValidUpto: undefined }
        : p
    ));
  };

  // Save edited price
  const saveEdit = async (price: PriceRow) => {
    try {
      const updatedPrice: ISKUPrice = {
        ...price,
        Price: price.TempPrice ?? price.Price,
        DefaultRetPrice: price.TempDefaultRetPrice ?? price.DefaultRetPrice,
        ValidFrom: price.TempValidFrom ?? price.ValidFrom,
        ValidUpto: price.TempValidUpto ?? price.ValidUpto,
      };
      
      console.log('TPPPriceTab - Original price:', price);
      console.log('TPPPriceTab - Updated price being sent:', updatedPrice);
      console.log('TPPPriceTab - JSON payload:', JSON.stringify(updatedPrice, null, 2));
      
      debugger; // Add debugger to inspect the exact payload structure
      
      await skuPriceService.updateSKUPrice(updatedPrice);
      toast({
        title: "Success",
        description: "Price updated successfully",
      });
      setEditingRows(prev => {
        const newSet = new Set(prev);
        newSet.delete(price.UID!);
        return newSet;
      });
      fetchPrices(); // Refresh data
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to update price",
        variant: "destructive",
      });
    }
  };

  // Delete price
  const confirmDelete = (price: PriceRow) => {
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

      // Refresh data
      fetchPrices();
      
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete price",
        variant: "destructive",
      });
    } finally {
      setDeleteDialogOpen(false);
      setPriceToDelete(null);
    }
  };

  // Add new row inline (like editing)
  const addNewRow = (basedOnPrice?: PriceRow) => {
    const newRowId = `new-${newRowCounter}`;
    setNewRowCounter(prev => prev + 1);
    
    let newRow: PriceRow;
    
    if (basedOnPrice) {
      // Smart add - copy data and set smart dates
      const nextDay = new Date(basedOnPrice.ValidUpto);
      nextDay.setDate(nextDay.getDate() + 1);
      const nextYearEnd = new Date(nextDay.getFullYear() + 1, 11, 31);
      
      newRow = {
        UID: newRowId,
        isNew: true,
        isEditing: true,
        TempSKUCode: basedOnPrice.SKUCode,
        TempSKUName: basedOnPrice.SKUName,
        TempSKUPriceListUID: basedOnPrice.SKUPriceListUID || "Default",
        TempUOM: basedOnPrice.UOM || "PCS",
        TempPrice: basedOnPrice.Price,
        TempDefaultRetPrice: basedOnPrice.DefaultRetPrice,
        TempMRP: basedOnPrice.MRP,
        TempValidFrom: nextDay.toISOString().split('T')[0],
        TempValidUpto: nextYearEnd.toISOString().split('T')[0],
        TempStatus: "Active",
        SKUUID: basedOnPrice.SKUUID,
        // Set other required fields with defaults
        SKUCode: "",
        SKUName: "",
        UOM: basedOnPrice.UOM || "PCS",
        MRP: basedOnPrice.MRP || 0,
        Price: 0,
        ValidFrom: new Date(),
        ValidUpto: new Date(),
        IsActive: true,
        Status: "Active",
        CreatedBy: "ADMIN",
        CreatedTime: new Date(),
        ModifiedBy: "ADMIN",
        ModifiedTime: new Date(),
        ServerAddTime: new Date(),
        ServerModifiedTime: new Date()
      };
    } else {
      // General add - blank row
      newRow = {
        UID: newRowId,
        isNew: true,
        isEditing: true,
        TempSKUCode: "",
        TempSKUName: "",
        TempSKUPriceListUID: "Default",
        TempUOM: "PCS",
        TempPrice: 0,
        TempDefaultRetPrice: 0,
        TempMRP: 0,
        TempValidFrom: new Date().toISOString().split('T')[0],
        TempValidUpto: new Date(new Date().getFullYear() + 1, 11, 31).toISOString().split('T')[0],
        TempStatus: "Active",
        // Set other required fields with defaults
        SKUCode: "",
        SKUName: "",
        SKUUID: "",
        UOM: "PCS",
        MRP: 0,
        Price: 0,
        ValidFrom: new Date(),
        ValidUpto: new Date(),
        IsActive: true,
        Status: "Active",
        CreatedBy: "ADMIN",
        CreatedTime: new Date(),
        ModifiedBy: "ADMIN",
        ModifiedTime: new Date(),
        ServerAddTime: new Date(),
        ServerModifiedTime: new Date()
      };
    }
    
    // Add new row to the top of the list
    setPrices(prev => [newRow, ...prev]);
    setEditingRows(prev => new Set([...prev, newRowId]));
  };

  // Generate UID function (from backup implementation)
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

  // Save new row
  const saveNewRow = async (price: PriceRow) => {
    try {
      // Validate required fields
      if (!price.TempSKUCode || !price.TempPrice || !price.TempValidFrom || !price.TempValidUpto) {
        toast({
          title: "Validation Error",
          description: "Please fill in all required fields",
          variant: "destructive",
        });
        return;
      }

      const priceToAdd: ISKUPrice = {
        UID: generateUID(), // Generate UID as required by backend
        SKUCode: price.TempSKUCode!,
        SKUName: price.TempSKUName || "",
        SKUUID: price.SKUUID || "",
        SKUPriceListUID: price.TempSKUPriceListUID || "DefaultPriceList",
        UOM: price.TempUOM || "PCS",
        Price: price.TempPrice!,
        DefaultWSPrice: price.TempDefaultRetPrice || 0, // Map to DefaultWSPrice
        DefaultRetPrice: price.TempDefaultRetPrice || 0,
        DummyPrice: price.TempPrice!, // Use Price as DummyPrice
        MRP: price.TempMRP || 0,
        PriceUpperLimit: 0,
        PriceLowerLimit: 0,
        ValidFrom: new Date(price.TempValidFrom!),
        ValidUpto: new Date(price.TempValidUpto!),
        Status: price.TempStatus || "Active",
        IsActive: true,
        IsTaxIncluded: false,
        CreatedBy: "ADMIN",
        CreatedTime: new Date().toISOString(),
        ModifiedBy: "ADMIN", 
        ModifiedTime: new Date().toISOString(),
        ServerAddTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
        IsLatest: 1,
        VersionNo: "1.0",
        LadderingAmount: 0,
        LadderingPercentage: 0,
      } as ISKUPrice;

      await skuPriceService.createSKUPrice(priceToAdd);
      
      toast({
        title: "Success",
        description: "Price added successfully",
      });

      // Remove new row and refresh data
      setPrices(prev => prev.filter(p => p.UID !== price.UID));
      setEditingRows(prev => {
        const newSet = new Set(prev);
        newSet.delete(price.UID!);
        return newSet;
      });
      fetchPrices(); // Refresh to show the saved data
      
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to add price",
        variant: "destructive",
      });
      console.error("Add price error:", error);
    }
  };

  // Cancel new row
  const cancelNewRow = (priceUID: string) => {
    setPrices(prev => prev.filter(p => p.UID !== priceUID));
    setEditingRows(prev => {
      const newSet = new Set(prev);
      newSet.delete(priceUID);
      return newSet;
    });
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount || 0);
  };

  const formatDate = (date: Date | string | undefined) => {
    if (!date) return "";
    return new Date(date).toISOString().split('T')[0];
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-semibold">SKU Price Records</h3>
        <div className="flex gap-2">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by SKU code..."
              value={searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10 w-64"
            />
          </div>
          <Button onClick={() => addNewRow()}>
            <Plus className="h-4 w-4 mr-2" />
            Add New Price
          </Button>
        </div>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>SKU Code</TableHead>
              <TableHead>SKU Name</TableHead>
              <TableHead>UOM</TableHead>
              <TableHead>Price List</TableHead>
              <TableHead>Cost Price</TableHead>
              <TableHead>Retail Price</TableHead>
              <TableHead>Valid From</TableHead>
              <TableHead>Valid To</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <>
                {[...Array(pageSize)].map((_, index) => (
                  <TableRow key={`skeleton-${index}`}>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-36" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                    <TableCell className="text-right">
                      <Skeleton className="h-8 w-20 rounded ml-auto" />
                    </TableCell>
                  </TableRow>
                ))}
              </>
            ) : prices.length === 0 ? (
              <TableRow>
                <TableCell colSpan={9} className="text-center py-8">
                  No prices found
                </TableCell>
              </TableRow>
            ) : (
              prices.map((price) => (
                <TableRow
                  key={price.UID}
                  className={editingRows.has(price.UID!) ? (price.isNew ? "bg-green-50" : "bg-blue-50") : ""}
                >
                  {/* SKU Code */}
                  <TableCell className="font-medium">
                    {editingRows.has(price.UID!) ? (
                      <Input
                        value={price.TempSKUCode ?? price.SKUCode}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempSKUCode: e.target.value}
                              : p
                          ));
                        }}
                        placeholder="SKU Code"
                        className="w-32"
                        disabled={!price.isNew} // Only editable for new rows
                      />
                    ) : (
                      price.SKUCode
                    )}
                  </TableCell>
                  
                  {/* SKU Name */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        value={price.TempSKUName ?? price.SKUName ?? ""}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempSKUName: e.target.value}
                              : p
                          ));
                        }}
                        placeholder="SKU Name"
                        className="w-40"
                        disabled={!price.isNew} // Only editable for new rows
                      />
                    ) : (
                      price.SKUName || "-"
                    )}
                  </TableCell>
                  
                  {/* UOM */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        value={price.TempUOM ?? price.UOM ?? "PCS"}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempUOM: e.target.value}
                              : p
                          ));
                        }}
                        placeholder="UOM"
                        className="w-20"
                      />
                    ) : (
                      price.UOM || "PCS"
                    )}
                  </TableCell>
                  
                  {/* Price List */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        value={price.TempSKUPriceListUID ?? price.SKUPriceListUID ?? "Default"}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempSKUPriceListUID: e.target.value}
                              : p
                          ));
                        }}
                        placeholder="Price List"
                        className="w-32"
                      />
                    ) : (
                      <Badge variant="outline">{price.SKUPriceListUID || "Default"}</Badge>
                    )}
                  </TableCell>
                  
                  {/* Cost Price */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="number"
                        step="0.01"
                        value={price.TempPrice ?? price.Price}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempPrice: parseFloat(e.target.value) || 0}
                              : p
                          ));
                        }}
                        className="w-24"
                      />
                    ) : (
                      formatCurrency(price.Price)
                    )}
                  </TableCell>

                  {/* Retail Price */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="number"
                        step="0.01"
                        value={price.TempDefaultRetPrice ?? price.DefaultRetPrice}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempDefaultRetPrice: parseFloat(e.target.value) || 0}
                              : p
                          ));
                        }}
                        className="w-24"
                      />
                    ) : (
                      formatCurrency(price.DefaultRetPrice || 0)
                    )}
                  </TableCell>

                  {/* Valid From */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="date"
                        value={price.TempValidFrom ? formatDate(price.TempValidFrom) : formatDate(price.ValidFrom)}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempValidFrom: e.target.value}
                              : p
                          ));
                        }}
                        className="w-36"
                      />
                    ) : (
                      formatDateToDayMonthYear(price.ValidFrom, "-")
                    )}
                  </TableCell>

                  {/* Valid To */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="date"
                        value={price.TempValidUpto ? formatDate(price.TempValidUpto) : formatDate(price.ValidUpto)}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempValidUpto: e.target.value}
                              : p
                          ));
                        }}
                        className="w-36"
                      />
                    ) : (
                      formatDateToDayMonthYear(price.ValidUpto, "-")
                    )}
                  </TableCell>

                  {/* Actions */}
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      {editingRows.has(price.UID!) ? (
                        <>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => price.isNew ? saveNewRow(price) : saveEdit(price)}
                          >
                            Save
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => price.isNew ? cancelNewRow(price.UID!) : cancelEditing(price.UID!)}
                          >
                            Cancel
                          </Button>
                        </>
                      ) : (
                        <>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8"
                            onClick={() => startEditing(price.UID!)}
                          >
                            <Edit2 className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8"
                            title="Add new price period for this SKU"
                            onClick={() => addNewRow(price)}
                          >
                            <Plus className="h-4 w-4" />
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
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {totalCount > 0 && (
        <PaginationControls
          currentPage={currentPage}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
          onPageSizeChange={setPageSize}
          itemName="price records"
        />
      )}

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the price record
              {priceToDelete && (
                <span className="font-semibold">
                  {" "}for SKU {priceToDelete.SKUCode} (Valid from{" "}
                  {formatDateToDayMonthYear(priceToDelete.ValidFrom, "-")} to{" "}
                  {formatDateToDayMonthYear(priceToDelete.ValidUpto, "-")})
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