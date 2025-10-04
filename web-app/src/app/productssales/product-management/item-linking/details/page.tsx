"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  ArrowLeft,
  Package,
  Search
} from "lucide-react";
import {
  storeLinkingService,
  SelectionMapDetails
} from "@/services/store-linking.service";
import {
  skuClassGroupsService,
  SKUClassGroup,
} from "@/services/sku-class-groups.service";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { PaginationControls } from "@/components/ui/pagination-controls";
import { Input } from "@/components/ui/input";

export default function ItemLinkingDetailsPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const uid = searchParams.get("uid");

  const [loading, setLoading] = useState(true);
  const [mappingDetails, setMappingDetails] = useState<SelectionMapDetails[]>([]);
  const [mappingMaster, setMappingMaster] = useState<any>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");
  const [linkedItemName, setLinkedItemName] = useState("");

  useEffect(() => {
    if (uid) {
      fetchMappingDetails();
    }
  }, [uid]);

  const fetchMappingDetails = async () => {
    try {
      setLoading(true);
      console.log(`[DETAILS PAGE] Fetching details for criteria UID: ${uid}`);
      const response = await storeLinkingService.getSelectionMapDetailsByCriteriaUID(uid!);
      console.log(`[DETAILS PAGE] Response:`, response);

      // Handle both array response and object with details
      if (Array.isArray(response)) {
        setMappingDetails(response);
      } else if (response?.selectionMapDetails || response?.SelectionMapDetails) {
        const details = response.selectionMapDetails || response.SelectionMapDetails;
        setMappingDetails(details);
        const master = response.selectionMapCriteria || response.SelectionMapCriteria;
        setMappingMaster(master);

        // Fetch the linked item name
        if (master) {
          const itemUID = master.linkedItemUID || master.LinkedItemUID;
          const itemType = master.linkedItemType || master.LinkedItemType;

          // Fetch the actual name from the API based on item type
          if (itemType === 'SKUClassGroup') {
            const skuGroup = await skuClassGroupsService.getSKUClassGroupByUID(itemUID);
            if (skuGroup && skuGroup.Name) {
              setLinkedItemName(skuGroup.Name);
            } else {
              setLinkedItemName(itemUID);
            }
          } else if (itemType === 'PriceList') {
            // You can add a service call here to get the Price List name
            setLinkedItemName(itemUID); // For now, just use UID
          }
        }
      } else {
        setMappingDetails([]);
      }
    } catch (error) {
      console.error(`[DETAILS PAGE] Error fetching details:`, error);
    } finally {
      setLoading(false);
    }
  };

  // Filter details based on search term
  const filteredDetails = mappingDetails.filter((detail) => {
    const selGroup = (detail.selectionGroup || detail.SelectionGroup || '').toLowerCase();
    const selValue = (detail.selectionValue || detail.SelectionValue || '').toLowerCase();
    const search = searchTerm.toLowerCase();
    return selGroup.includes(search) || selValue.includes(search);
  });

  if (!uid) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Card className="w-full max-w-md">
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">Invalid mapping UID</p>
            <Button
              onClick={() => router.back()}
              className="w-full mt-4"
              variant="outline"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Back Navigation */}
      <div className="mb-6">
        <Button
          variant="outline"
          size="default"
          onClick={() => router.push('/productssales/product-management/item-linking')}
          className="group hover:bg-gray-50 transition-colors"
        >
          <ArrowLeft className="h-4 w-4 mr-2 transition-transform group-hover:-translate-x-1" />
          Back to Product Group Linking
        </Button>
      </div>

      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Mapping Details
          </h1>
          {loading ? (
            <Skeleton className="h-4 w-64 mt-1" />
          ) : (
            <p className="text-sm text-muted-foreground mt-1">
              {linkedItemName || 'View all mapped entities for this product group linking'}
            </p>
          )}
        </div>
      </div>

      {/* Info Card */}
      {mappingMaster && !loading && (
        <Card className="p-4 bg-muted/30 border-0">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <p className="text-xs text-muted-foreground">Linked Item</p>
              <p className="text-sm font-medium">{linkedItemName || (mappingMaster.linkedItemUID || mappingMaster.LinkedItemUID)}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Item Type</p>
              <p className="text-sm font-medium">{mappingMaster.linkedItemType || mappingMaster.LinkedItemType}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Total Mapped</p>
              <p className="text-sm font-medium">{mappingDetails.length}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Status</p>
              <Badge variant={(mappingMaster.isActive || mappingMaster.IsActive) ? 'default' : 'secondary'}>
                {(mappingMaster.isActive || mappingMaster.IsActive) ? 'Active' : 'Inactive'}
              </Badge>
            </div>
          </div>
        </Card>
      )}

      {/* Search Bar */}
      <Card className="shadow-sm border-gray-200">
        <div className="p-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by selection group or value..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
            />
          </div>
        </div>
      </Card>

      {/* Data Table */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          {loading ? (
            <div className="p-8 text-center">
              <div className="space-y-2">
                {[1, 2, 3, 4, 5].map((i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            </div>
          ) : filteredDetails.length === 0 ? (
            <div className="p-12 text-center">
              <Package className="h-12 w-12 mx-auto text-gray-300" />
              <h3 className="mt-4 font-semibold text-gray-700">
                No Mapped Entities Found
              </h3>
              <p className="mt-2 text-sm text-gray-500">
                No entities are mapped to this item linking
              </p>
            </div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="pl-6">Selection Group</TableHead>
                    <TableHead>Selection Value</TableHead>
                    <TableHead className="w-[140px] text-center">Is Excluded</TableHead>
                    <TableHead className="w-[140px] text-center pr-6">Is Active</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                    {filteredDetails
                      .slice((currentPage - 1) * pageSize, currentPage * pageSize)
                      .map((detail, idx) => {
                      const selGroup = detail.selectionGroup || detail.SelectionGroup;
                      const selValue = detail.selectionValue || detail.SelectionValue;
                      const isExcluded = detail.isExcluded || detail.IsExcluded;
                      const isActive = detail.isActive !== undefined ? detail.isActive : (detail.IsActive !== undefined ? detail.IsActive : true);

                      return (
                        <TableRow key={idx} className="hover:bg-muted/30">
                          <TableCell className="pl-6">
                            <Badge variant="outline" className="font-medium">
                              {selGroup || '-'}
                            </Badge>
                          </TableCell>
                          <TableCell className="font-mono text-sm">
                            {selValue || '-'}
                          </TableCell>
                          <TableCell className="text-center">
                            <div className="flex justify-center">
                              <Badge
                                variant={isExcluded ? "destructive" : "default"}
                                className="min-w-[70px] justify-center"
                              >
                                {isExcluded ? "Excluded" : "Included"}
                              </Badge>
                            </div>
                          </TableCell>
                          <TableCell className="text-center pr-6">
                            <div className="flex justify-center">
                              <Badge
                                variant={isActive ? "default" : "secondary"}
                                className="min-w-[70px] justify-center"
                              >
                                {isActive ? "Active" : "Inactive"}
                              </Badge>
                            </div>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                </TableBody>
              </Table>

              {/* Pagination */}
              {filteredDetails.length > 0 && (
                <div className="px-6 py-4 border-t bg-gray-50/30">
                  <PaginationControls
                    currentPage={currentPage}
                    totalCount={filteredDetails.length}
                    pageSize={pageSize}
                    onPageChange={setCurrentPage}
                    onPageSizeChange={setPageSize}
                    itemName="entities"
                  />
                </div>
              )}
            </>
          )}
        </div>
      </Card>
    </div>
  );
}
