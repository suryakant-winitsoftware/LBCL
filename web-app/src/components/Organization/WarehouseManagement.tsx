'use client';

import React, { useState, useEffect } from 'react';
import { 
  Card, 
  CardContent, 
  CardHeader, 
  CardTitle,
  CardDescription 
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { 
  Building2, 
  Package, 
  Plus, 
  Search, 
  Edit, 
  Trash2, 
  Eye,
  PackageCheck,
  TrendingUp,
  AlertCircle,
  Download,
  Upload,
  RefreshCw
} from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { apiService as api } from '@/services/api';

interface Warehouse {
  uid: string;
  franchiseeOrgUID: string;
  franchiseeOrgName: string;
  warehouseUID: string;
  warehouseName: string;
  warehouseCode: string;
  warehouseType: string;
  isActive: boolean;
  totalStock: number;
  totalValue: number;
  lastUpdated: string;
  stockTypes: {
    regular: number;
    foc: number;
    damaged: number;
  };
}

interface WarehouseStock {
  uid: string;
  skuCode: string;
  skuName: string;
  skuUID: string;
  ouQty: number;
  buQty: number;
  eaQty: number;
  totalEAQty: number;
  costPrice: number;
  totalCost: number;
  net: number;
  stockType: string;
  reservedOUQty?: number;
  reservedBUQty?: number;
  expiryDate?: string;
  batchNumber?: string;
}

export default function WarehouseManagement() {
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [selectedWarehouse, setSelectedWarehouse] = useState<Warehouse | null>(null);
  const [warehouseStock, setWarehouseStock] = useState<WarehouseStock[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterOrgType, setFilterOrgType] = useState('all');
  const [stockType, setStockType] = useState('all');
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showStockDialog, setShowStockDialog] = useState(false);
  const [organizations, setOrganizations] = useState<any[]>([]);
  const [franchiseeOrgs, setFranchiseeOrgs] = useState<any[]>([]);
  const { toast } = useToast();

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  // Form state for creating/editing warehouse
  const [formData, setFormData] = useState({
    franchiseeOrgUID: '',
    warehouseUID: '',
    warehouseType: 'Main',
    isActive: true
  });

  useEffect(() => {
    fetchWarehouses();
    fetchOrganizations();
  }, [currentPage, searchTerm, filterOrgType]);

  const fetchWarehouses = async () => {
    setLoading(true);
    try {
      const filters = [];
      if (searchTerm) {
        filters.push({
          field: 'warehouseName',
          operator: 'contains',
          value: searchTerm
        });
      }
      if (filterOrgType !== 'all') {
        filters.push({
          field: 'warehouseType',
          operator: 'equals',
          value: filterOrgType
        });
      }

      const response = await api.post('/api/Org/ViewFranchiseeWarehouse', {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: filters,
        sortCriterias: [{ field: 'warehouseName', direction: 'asc' }],
        isCountRequired: true
      });

      if (response.data?.pagedData) {
        setWarehouses(response.data.pagedData);
        setTotalCount(response.data.totalCount || 0);
      }
    } catch (error) {
      console.error('Error fetching warehouses:', error);
      toast({
        title: 'Error',
        description: 'Failed to fetch warehouses',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const fetchOrganizations = async () => {
    try {
      // Fetch all organizations
      const response = await api.post('/api/Org/GetOrgDetails', {
        pageNumber: 1,
        pageSize: 1000,
        filterCriterias: [],
        sortCriterias: [{ field: 'name', direction: 'asc' }],
        isCountRequired: false
      });

      if (response.data?.pagedData) {
        setOrganizations(response.data.pagedData);
        // Filter franchisee organizations
        const franchisees = response.data.pagedData.filter((org: any) => 
          org.orgTypeUID && org.orgTypeUID.includes('franchisee')
        );
        setFranchiseeOrgs(franchisees);
      }

      // Fetch organization types to identify warehouse organizations
      const typeResponse = await api.post('/api/Org/GetOrgTypeDetails', {
        pageNumber: 1,
        pageSize: 100,
        filterCriterias: [{ field: 'isWH', operator: 'equals', value: true }],
        sortCriterias: [],
        isCountRequired: false
      });

      if (typeResponse.data?.pagedData) {
        const warehouseTypeUIDs = typeResponse.data.pagedData.map((type: any) => type.uid);
        // Filter organizations that are warehouses
        const warehouseOrgs = organizations.filter((org: any) => 
          warehouseTypeUIDs.includes(org.orgTypeUID)
        );
        // Store for warehouse selection
      }
    } catch (error) {
      console.error('Error fetching organizations:', error);
    }
  };

  const fetchWarehouseStock = async (warehouseUID: string, franchiseeOrgUID: string) => {
    setLoading(true);
    try {
      const response = await api.post(
        `/api/Org/GetWarehouseStockDetails?FranchiseeOrgUID=${franchiseeOrgUID}&WarehouseUID=${warehouseUID}&StockType=${stockType === 'all' ? '' : stockType}`,
        {
          pageNumber: 1,
          pageSize: 1000,
          filterCriterias: [],
          sortCriterias: [{ field: 'skuName', direction: 'asc' }],
          isCountRequired: true
        }
      );

      if (response.data?.pagedData) {
        setWarehouseStock(response.data.pagedData);
      }
    } catch (error) {
      console.error('Error fetching warehouse stock:', error);
      toast({
        title: 'Error',
        description: 'Failed to fetch warehouse stock details',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCreateWarehouse = async () => {
    try {
      const response = await api.post('/api/Org/CreateViewFranchiseeWarehouse', {
        franchiseeOrgUID: formData.franchiseeOrgUID,
        warehouseUID: formData.warehouseUID,
        warehouseType: formData.warehouseType,
        isActive: formData.isActive,
        createdTime: new Date().toISOString(),
        serverAddTime: new Date().toISOString()
      });

      if (response.data) {
        toast({
          title: 'Success',
          description: 'Warehouse created successfully'
        });
        setShowCreateDialog(false);
        fetchWarehouses();
        resetForm();
      }
    } catch (error) {
      console.error('Error creating warehouse:', error);
      toast({
        title: 'Error',
        description: 'Failed to create warehouse',
        variant: 'destructive'
      });
    }
  };

  const handleUpdateWarehouse = async (warehouse: Warehouse) => {
    try {
      const response = await api.put('/api/Org/UpdateViewFranchiseeWarehouse', {
        uid: warehouse.uid,
        franchiseeOrgUID: warehouse.franchiseeOrgUID,
        warehouseUID: warehouse.warehouseUID,
        warehouseType: warehouse.warehouseType,
        isActive: warehouse.isActive,
        modifiedTime: new Date().toISOString(),
        serverModifiedTime: new Date().toISOString()
      });

      if (response.data) {
        toast({
          title: 'Success',
          description: 'Warehouse updated successfully'
        });
        fetchWarehouses();
      }
    } catch (error) {
      console.error('Error updating warehouse:', error);
      toast({
        title: 'Error',
        description: 'Failed to update warehouse',
        variant: 'destructive'
      });
    }
  };

  const handleDeleteWarehouse = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this warehouse?')) return;

    try {
      const response = await api.delete(`/api/Org/DeleteViewFranchiseeWarehouse?UID=${uid}`);
      
      if (response.data) {
        toast({
          title: 'Success',
          description: 'Warehouse deleted successfully'
        });
        fetchWarehouses();
      }
    } catch (error) {
      console.error('Error deleting warehouse:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete warehouse',
        variant: 'destructive'
      });
    }
  };

  const handleViewStock = (warehouse: Warehouse) => {
    setSelectedWarehouse(warehouse);
    fetchWarehouseStock(warehouse.warehouseUID, warehouse.franchiseeOrgUID);
    setShowStockDialog(true);
  };

  const resetForm = () => {
    setFormData({
      franchiseeOrgUID: '',
      warehouseUID: '',
      warehouseType: 'Main',
      isActive: true
    });
  };

  const calculateStockSummary = () => {
    const summary = {
      totalItems: warehouseStock.length,
      totalQuantity: warehouseStock.reduce((sum, item) => sum + item.totalEAQty, 0),
      totalValue: warehouseStock.reduce((sum, item) => sum + item.net, 0),
      regularStock: warehouseStock.filter(item => item.stockType === 'Regular').length,
      focStock: warehouseStock.filter(item => item.stockType === 'FOC').length
    };
    return summary;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Warehouse Management</h1>
          <p className="text-gray-500 mt-1">Manage warehouses and stock across organizations</p>
        </div>
        <div className="space-x-2">
          <Button variant="outline" onClick={fetchWarehouses}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          <Button onClick={() => setShowCreateDialog(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Add Warehouse
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Warehouses</CardTitle>
            <Building2 className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalCount}</div>
            <p className="text-xs text-muted-foreground">Active warehouses</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Main Warehouses</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {warehouses.filter(w => w.warehouseType === 'Main').length}
            </div>
            <p className="text-xs text-muted-foreground">Primary storage</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Van Stock</CardTitle>
            <PackageCheck className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {warehouses.filter(w => w.warehouseType === 'Van').length}
            </div>
            <p className="text-xs text-muted-foreground">Mobile inventory</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Stock Value</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${warehouses.reduce((sum, w) => sum + (w.totalValue || 0), 0).toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground">Across all warehouses</p>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4">
            <div className="flex-1">
              <Label htmlFor="search">Search</Label>
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  id="search"
                  placeholder="Search warehouses..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <div>
              <Label htmlFor="type">Warehouse Type</Label>
              <Select value={filterOrgType} onValueChange={setFilterOrgType}>
                <SelectTrigger id="type" className="w-[180px]">
                  <SelectValue placeholder="All Types" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Types</SelectItem>
                  <SelectItem value="Main">Main Warehouse</SelectItem>
                  <SelectItem value="Van">Van Stock</SelectItem>
                  <SelectItem value="Regional">Regional</SelectItem>
                  <SelectItem value="Distribution">Distribution Center</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Warehouses Table */}
      <Card>
        <CardHeader>
          <CardTitle>Warehouses</CardTitle>
          <CardDescription>Manage warehouse assignments and stock levels</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Warehouse Name</TableHead>
                <TableHead>Code</TableHead>
                <TableHead>Franchisee</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Total Stock</TableHead>
                <TableHead>Stock Value</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={8} className="text-center">
                    Loading warehouses...
                  </TableCell>
                </TableRow>
              ) : warehouses.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} className="text-center">
                    No warehouses found
                  </TableCell>
                </TableRow>
              ) : (
                warehouses.map((warehouse) => (
                  <TableRow key={warehouse.uid}>
                    <TableCell className="font-medium">
                      {warehouse.warehouseName}
                    </TableCell>
                    <TableCell>{warehouse.warehouseCode}</TableCell>
                    <TableCell>{warehouse.franchiseeOrgName}</TableCell>
                    <TableCell>
                      <Badge variant={warehouse.warehouseType === 'Main' ? 'default' : 'secondary'}>
                        {warehouse.warehouseType}
                      </Badge>
                    </TableCell>
                    <TableCell>{warehouse.totalStock || 0}</TableCell>
                    <TableCell>${(warehouse.totalValue || 0).toLocaleString()}</TableCell>
                    <TableCell>
                      <Badge variant={warehouse.isActive ? 'success' : 'destructive'}>
                        {warehouse.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex space-x-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleViewStock(warehouse)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            // Set form data for editing
                            setFormData({
                              franchiseeOrgUID: warehouse.franchiseeOrgUID,
                              warehouseUID: warehouse.warehouseUID,
                              warehouseType: warehouse.warehouseType,
                              isActive: warehouse.isActive
                            });
                            setShowCreateDialog(true);
                          }}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleDeleteWarehouse(warehouse.uid)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>

          {/* Pagination */}
          {totalCount > pageSize && (
            <div className="flex justify-center gap-2 mt-4">
              <Button
                variant="outline"
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage === 1}
              >
                Previous
              </Button>
              <span className="flex items-center px-4">
                Page {currentPage} of {Math.ceil(totalCount / pageSize)}
              </span>
              <Button
                variant="outline"
                onClick={() => setCurrentPage(p => p + 1)}
                disabled={currentPage >= Math.ceil(totalCount / pageSize)}
              >
                Next
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Create/Edit Warehouse Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent className="sm:max-w-[600px]">
          <DialogHeader>
            <DialogTitle>
              {formData.franchiseeOrgUID ? 'Edit' : 'Create'} Warehouse Assignment
            </DialogTitle>
            <DialogDescription>
              Assign a warehouse to a franchisee organization
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="franchisee" className="text-right">
                Franchisee
              </Label>
              <Select
                value={formData.franchiseeOrgUID}
                onValueChange={(value) => setFormData({...formData, franchiseeOrgUID: value})}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Select franchisee organization" />
                </SelectTrigger>
                <SelectContent>
                  {franchiseeOrgs.map((org) => (
                    <SelectItem key={org.uid} value={org.uid}>
                      {org.name} ({org.code})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="warehouse" className="text-right">
                Warehouse
              </Label>
              <Select
                value={formData.warehouseUID}
                onValueChange={(value) => setFormData({...formData, warehouseUID: value})}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Select warehouse" />
                </SelectTrigger>
                <SelectContent>
                  {organizations
                    .filter(org => org.isWH || (org.orgTypeUID && org.orgTypeUID.includes('warehouse')))
                    .map((org) => (
                      <SelectItem key={org.uid} value={org.uid}>
                        {org.name} ({org.code})
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="warehouseType" className="text-right">
                Type
              </Label>
              <Select
                value={formData.warehouseType}
                onValueChange={(value) => setFormData({...formData, warehouseType: value})}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Main">Main Warehouse</SelectItem>
                  <SelectItem value="Van">Van Stock</SelectItem>
                  <SelectItem value="Regional">Regional</SelectItem>
                  <SelectItem value="Distribution">Distribution Center</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="active" className="text-right">
                Status
              </Label>
              <Select
                value={formData.isActive ? 'active' : 'inactive'}
                onValueChange={(value) => setFormData({...formData, isActive: value === 'active'})}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="active">Active</SelectItem>
                  <SelectItem value="inactive">Inactive</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => {
              setShowCreateDialog(false);
              resetForm();
            }}>
              Cancel
            </Button>
            <Button onClick={handleCreateWarehouse}>
              {formData.franchiseeOrgUID ? 'Update' : 'Create'} Warehouse
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Stock Details Dialog */}
      <Dialog open={showStockDialog} onOpenChange={setShowStockDialog}>
        <DialogContent className="max-w-[90vw] max-h-[90vh] overflow-auto">
          <DialogHeader>
            <DialogTitle>
              Stock Details - {selectedWarehouse?.warehouseName}
            </DialogTitle>
            <DialogDescription>
              View and manage stock levels for this warehouse
            </DialogDescription>
          </DialogHeader>
          
          {selectedWarehouse && (
            <div className="space-y-4">
              {/* Stock Summary */}
              <div className="grid grid-cols-5 gap-4">
                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm">Total Items</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="text-xl font-bold">
                      {calculateStockSummary().totalItems}
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm">Total Quantity</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="text-xl font-bold">
                      {calculateStockSummary().totalQuantity}
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm">Total Value</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="text-xl font-bold">
                      ${calculateStockSummary().totalValue.toLocaleString()}
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm">Regular Stock</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="text-xl font-bold">
                      {calculateStockSummary().regularStock}
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm">FOC Stock</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="text-xl font-bold">
                      {calculateStockSummary().focStock}
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Stock Type Filter */}
              <div className="flex justify-between items-center">
                <Select value={stockType} onValueChange={setStockType}>
                  <SelectTrigger className="w-[180px]">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Stock</SelectItem>
                    <SelectItem value="Regular">Regular Stock</SelectItem>
                    <SelectItem value="FOC">FOC Stock</SelectItem>
                    <SelectItem value="Damaged">Damaged Stock</SelectItem>
                  </SelectContent>
                </Select>
                <div className="space-x-2">
                  <Button variant="outline" size="sm">
                    <Download className="mr-2 h-4 w-4" />
                    Export
                  </Button>
                  <Button variant="outline" size="sm">
                    <Upload className="mr-2 h-4 w-4" />
                    Import
                  </Button>
                </div>
              </div>

              {/* Stock Table */}
              <div className="border rounded-lg">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>SKU Code</TableHead>
                      <TableHead>SKU Name</TableHead>
                      <TableHead>OU Qty</TableHead>
                      <TableHead>BU Qty</TableHead>
                      <TableHead>EA Qty</TableHead>
                      <TableHead>Total EA</TableHead>
                      <TableHead>Cost Price</TableHead>
                      <TableHead>Total Value</TableHead>
                      <TableHead>Stock Type</TableHead>
                      <TableHead>Reserved</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {warehouseStock.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={10} className="text-center">
                          No stock data available
                        </TableCell>
                      </TableRow>
                    ) : (
                      warehouseStock.map((stock) => (
                        <TableRow key={stock.uid}>
                          <TableCell className="font-medium">{stock.skuCode}</TableCell>
                          <TableCell>{stock.skuName}</TableCell>
                          <TableCell>{stock.ouQty}</TableCell>
                          <TableCell>{stock.buQty}</TableCell>
                          <TableCell>{stock.eaQty}</TableCell>
                          <TableCell className="font-semibold">{stock.totalEAQty}</TableCell>
                          <TableCell>${stock.costPrice.toFixed(2)}</TableCell>
                          <TableCell>${stock.net.toFixed(2)}</TableCell>
                          <TableCell>
                            <Badge variant={stock.stockType === 'FOC' ? 'secondary' : 'default'}>
                              {stock.stockType}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            {stock.reservedOUQty || stock.reservedBUQty ? (
                              <Badge variant="outline">
                                {stock.reservedOUQty || 0} OU / {stock.reservedBUQty || 0} BU
                              </Badge>
                            ) : (
                              '-'
                            )}
                          </TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}