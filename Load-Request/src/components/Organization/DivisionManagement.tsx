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
  Building,
  Users,
  Plus,
  Search,
  Edit,
  Trash2,
  Eye,
  GitBranch,
  Network,
  Truck,
  Store,
  RefreshCw,
  ChevronRight
} from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { apiService as api } from '@/services/api';

interface Division {
  uid: string;
  code: string;
  name: string;
  parentUID: string;
  parentName?: string;
  orgTypeUID: string;
  orgTypeName?: string;
  isActive: boolean;
  companyUID: string;
  companyName?: string;
  storeCount?: number;
  distributorCount?: number;
  hierarchyLevel?: number;
}

interface Store {
  uid: string;
  code: string;
  name: string;
  divisionUID: string;
  divisionName?: string;
  isActive: boolean;
  address?: string;
  contactPerson?: string;
  phone?: string;
}

interface Distributor {
  uid: string;
  code: string;
  name: string;
  orgUID: string;
  orgName?: string;
  storeUID: string;
  storeName?: string;
  deliveryType: string;
  isActive: boolean;
  routeCount?: number;
}

export default function DivisionManagement() {
  const [divisions, setDivisions] = useState<Division[]>([]);
  const [stores, setStores] = useState<Store[]>([]);
  const [distributors, setDistributors] = useState<Distributor[]>([]);
  const [selectedDivision, setSelectedDivision] = useState<Division | null>(null);
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('divisions');
  const [searchTerm, setSearchTerm] = useState('');
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showDetailsDialog, setShowDetailsDialog] = useState(false);
  const [dialogType, setDialogType] = useState<'division' | 'store' | 'distributor'>('division');
  const { toast } = useToast();

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  // Form state
  const [formData, setFormData] = useState<any>({
    code: '',
    name: '',
    parentUID: '',
    orgTypeUID: '',
    isActive: true,
    companyUID: ''
  });

  // Available parent organizations and types
  const [parentOrgs, setParentOrgs] = useState<any[]>([]);
  const [orgTypes, setOrgTypes] = useState<any[]>([]);

  useEffect(() => {
    fetchDivisions();
    fetchParentOrganizations();
    fetchOrgTypes();
  }, [currentPage, searchTerm]);

  const fetchDivisions = async () => {
    setLoading(true);
    try {
      // Get division-type organizations
      const response = await api.post('/api/Org/GetOrgDetails', {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: searchTerm ? [
          {
            field: 'name',
            operator: 'contains',
            value: searchTerm
          }
        ] : [],
        sortCriterias: [{ field: 'name', direction: 'asc' }],
        isCountRequired: true
      });

      if (response.data?.pagedData) {
        // Filter for division-type organizations
        const divisionOrgs = response.data.pagedData.filter((org: any) => {
          // Check if it's a division based on orgType or name pattern
          return org.orgTypeUID?.includes('division') || 
                 org.name?.toLowerCase().includes('division') ||
                 org.orgTypeUID?.includes('supplier');
        });
        
        setDivisions(divisionOrgs);
        setTotalCount(divisionOrgs.length);
      }
    } catch (error) {
      console.error('Error fetching divisions:', error);
      toast({
        title: 'Error',
        description: 'Failed to fetch divisions',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const fetchStoresForDivision = async (divisionUID: string) => {
    setLoading(true);
    try {
      const response = await api.post(`/api/Org/GetDivisions`, divisionUID);
      
      if (response.data) {
        setStores(response.data);
      }
    } catch (error) {
      console.error('Error fetching stores:', error);
      // Fallback: get all stores and filter
      try {
        const allStoresResponse = await api.post('/api/Org/GetOrgDetails', {
          pageNumber: 1,
          pageSize: 1000,
          filterCriterias: [{
            field: 'parentUID',
            operator: 'equals',
            value: divisionUID
          }],
          sortCriterias: [{ field: 'name', direction: 'asc' }],
          isCountRequired: false
        });
        
        if (allStoresResponse.data?.pagedData) {
          setStores(allStoresResponse.data.pagedData);
        }
      } catch (fallbackError) {
        console.error('Fallback error:', fallbackError);
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchDistributorsForOrg = async (orgUID: string, storeUID?: string) => {
    setLoading(true);
    try {
      const response = await api.get(
        `/api/Org/GetDeliveryDistributorsByOrgUID?orgUID=${orgUID}&storeUID=${storeUID || ''}`
      );
      
      if (response.data) {
        setDistributors(response.data);
      }
    } catch (error) {
      console.error('Error fetching distributors:', error);
      toast({
        title: 'Error',
        description: 'Failed to fetch distributors',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const fetchParentOrganizations = async () => {
    try {
      const response = await api.post('/api/Org/GetOrgDetails', {
        pageNumber: 1,
        pageSize: 1000,
        filterCriterias: [],
        sortCriterias: [{ field: 'name', direction: 'asc' }],
        isCountRequired: false
      });

      if (response.data?.pagedData) {
        setParentOrgs(response.data.pagedData);
      }
    } catch (error) {
      console.error('Error fetching parent organizations:', error);
    }
  };

  const fetchOrgTypes = async () => {
    try {
      const response = await api.post('/api/Org/GetOrgTypeDetails', {
        pageNumber: 1,
        pageSize: 100,
        filterCriterias: [],
        sortCriterias: [{ field: 'name', direction: 'asc' }],
        isCountRequired: false
      });

      if (response.data?.pagedData) {
        setOrgTypes(response.data.pagedData);
      }
    } catch (error) {
      console.error('Error fetching organization types:', error);
    }
  };

  const handleCreateDivision = async () => {
    try {
      const response = await api.post('/api/Org/CreateOrg', {
        ...formData,
        uid: generateUID(formData.name),
        createdTime: new Date().toISOString(),
        serverAddTime: new Date().toISOString(),
        serverModifiedTime: new Date().toISOString()
      });

      if (response.data) {
        toast({
          title: 'Success',
          description: 'Division created successfully'
        });
        setShowCreateDialog(false);
        fetchDivisions();
        resetForm();
      }
    } catch (error) {
      console.error('Error creating division:', error);
      toast({
        title: 'Error',
        description: 'Failed to create division',
        variant: 'destructive'
      });
    }
  };

  const handleUpdateDivision = async (division: Division) => {
    try {
      const response = await api.put('/api/Org/UpdateOrg', {
        ...division,
        modifiedTime: new Date().toISOString(),
        serverModifiedTime: new Date().toISOString()
      });

      if (response.data) {
        toast({
          title: 'Success',
          description: 'Division updated successfully'
        });
        fetchDivisions();
      }
    } catch (error) {
      console.error('Error updating division:', error);
      toast({
        title: 'Error',
        description: 'Failed to update division',
        variant: 'destructive'
      });
    }
  };

  const handleDeleteDivision = async (uid: string) => {
    if (!confirm('Are you sure you want to delete this division?')) return;

    try {
      const response = await api.delete(`/api/Org/DeleteOrg?UID=${uid}`);
      
      if (response.data) {
        toast({
          title: 'Success',
          description: 'Division deleted successfully'
        });
        fetchDivisions();
      }
    } catch (error) {
      console.error('Error deleting division:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete division. It may have dependent records.',
        variant: 'destructive'
      });
    }
  };

  const handleViewDetails = (division: Division) => {
    setSelectedDivision(division);
    fetchStoresForDivision(division.uid);
    fetchDistributorsForOrg(division.uid);
    setShowDetailsDialog(true);
  };

  const generateUID = (name: string) => {
    return name.toLowerCase().replace(/\s+/g, '-') + '-' + Date.now();
  };

  const generateCode = (name: string) => {
    return name.toUpperCase().substring(0, 3) + Date.now().toString().substring(-4);
  };

  const resetForm = () => {
    setFormData({
      code: '',
      name: '',
      parentUID: '',
      orgTypeUID: '',
      isActive: true,
      companyUID: ''
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Division Management</h1>
          <p className="text-gray-500 mt-1">Manage organizational divisions and distribution network</p>
        </div>
        <div className="space-x-2">
          <Button variant="outline" onClick={fetchDivisions}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          <Button onClick={() => {
            setDialogType('division');
            setShowCreateDialog(true);
          }}>
            <Plus className="mr-2 h-4 w-4" />
            Create Division
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Divisions</CardTitle>
            <Building className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{divisions.length}</div>
            <p className="text-xs text-muted-foreground">Active divisions</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Stores</CardTitle>
            <Store className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {divisions.reduce((sum, d) => sum + (d.storeCount || 0), 0)}
            </div>
            <p className="text-xs text-muted-foreground">Across all divisions</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Distributors</CardTitle>
            <Truck className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {divisions.reduce((sum, d) => sum + (d.distributorCount || 0), 0)}
            </div>
            <p className="text-xs text-muted-foreground">Delivery partners</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Network Depth</CardTitle>
            <Network className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {Math.max(...divisions.map(d => d.hierarchyLevel || 0), 0)}
            </div>
            <p className="text-xs text-muted-foreground">Hierarchy levels</p>
          </CardContent>
        </Card>
      </div>

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="divisions">Divisions</TabsTrigger>
          <TabsTrigger value="hierarchy">Hierarchy View</TabsTrigger>
          <TabsTrigger value="distributors">Distribution Network</TabsTrigger>
        </TabsList>

        <TabsContent value="divisions" className="space-y-4">
          {/* Search */}
          <Card>
            <CardHeader>
              <CardTitle>Search & Filter</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search divisions..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </CardContent>
          </Card>

          {/* Divisions Table */}
          <Card>
            <CardHeader>
              <CardTitle>Divisions</CardTitle>
              <CardDescription>Manage organizational divisions and suppliers</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Division Name</TableHead>
                    <TableHead>Code</TableHead>
                    <TableHead>Parent Organization</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Stores</TableHead>
                    <TableHead>Distributors</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {loading ? (
                    <TableRow>
                      <TableCell colSpan={8} className="text-center">
                        Loading divisions...
                      </TableCell>
                    </TableRow>
                  ) : divisions.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={8} className="text-center">
                        No divisions found
                      </TableCell>
                    </TableRow>
                  ) : (
                    divisions.map((division) => (
                      <TableRow key={division.uid}>
                        <TableCell className="font-medium">
                          <div className="flex items-center">
                            <GitBranch className="mr-2 h-4 w-4 text-muted-foreground" />
                            {division.name}
                          </div>
                        </TableCell>
                        <TableCell>{division.code}</TableCell>
                        <TableCell>{division.parentName || '-'}</TableCell>
                        <TableCell>
                          <Badge variant="outline">
                            {division.orgTypeName || 'Division'}
                          </Badge>
                        </TableCell>
                        <TableCell>{division.storeCount || 0}</TableCell>
                        <TableCell>{division.distributorCount || 0}</TableCell>
                        <TableCell>
                          <Badge variant={division.isActive ? 'success' : 'destructive'}>
                            {division.isActive ? 'Active' : 'Inactive'}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div className="flex space-x-2">
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleViewDetails(division)}
                            >
                              <Eye className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => {
                                setFormData(division);
                                setDialogType('division');
                                setShowCreateDialog(true);
                              }}
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleDeleteDivision(division.uid)}
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
        </TabsContent>

        <TabsContent value="hierarchy" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Division Hierarchy</CardTitle>
              <CardDescription>Visual representation of division structure</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                {divisions.filter(d => !d.parentUID).map((rootDivision) => (
                  <div key={rootDivision.uid} className="border rounded-lg p-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center">
                        <Building className="mr-2 h-5 w-5 text-blue-500" />
                        <div>
                          <p className="font-semibold">{rootDivision.name}</p>
                          <p className="text-sm text-muted-foreground">{rootDivision.code}</p>
                        </div>
                      </div>
                      <Badge>{rootDivision.storeCount || 0} stores</Badge>
                    </div>
                    
                    {/* Child divisions */}
                    <div className="ml-8 mt-2 space-y-2">
                      {divisions
                        .filter(d => d.parentUID === rootDivision.uid)
                        .map((childDivision) => (
                          <div key={childDivision.uid} className="flex items-center border-l-2 pl-4 py-2">
                            <ChevronRight className="mr-2 h-4 w-4 text-muted-foreground" />
                            <div className="flex-1">
                              <p className="font-medium">{childDivision.name}</p>
                              <p className="text-sm text-muted-foreground">
                                {childDivision.code} • {childDivision.storeCount || 0} stores
                              </p>
                            </div>
                          </div>
                        ))}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="distributors" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Distribution Network</CardTitle>
              <CardDescription>Delivery distributors and their assignments</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="text-center text-muted-foreground py-8">
                Select a division to view its distribution network
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Create/Edit Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent className="sm:max-w-[600px]">
          <DialogHeader>
            <DialogTitle>
              {formData.uid ? 'Edit' : 'Create'} Division
            </DialogTitle>
            <DialogDescription>
              {dialogType === 'division' ? 'Create a new organizational division' : 
               dialogType === 'store' ? 'Add a store to the division' :
               'Configure distribution partner'}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="name" className="text-right">
                Name
              </Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => {
                  setFormData({
                    ...formData,
                    name: e.target.value,
                    code: generateCode(e.target.value)
                  });
                }}
                className="col-span-3"
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="code" className="text-right">
                Code
              </Label>
              <Input
                id="code"
                value={formData.code}
                onChange={(e) => setFormData({...formData, code: e.target.value})}
                className="col-span-3"
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="parent" className="text-right">
                Parent Org
              </Label>
              <Select
                value={formData.parentUID}
                onValueChange={(value) => setFormData({...formData, parentUID: value})}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Select parent organization" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">None (Root)</SelectItem>
                  {parentOrgs.map((org) => (
                    <SelectItem key={org.uid} value={org.uid}>
                      {org.name} ({org.code})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="type" className="text-right">
                Type
              </Label>
              <Select
                value={formData.orgTypeUID}
                onValueChange={(value) => setFormData({...formData, orgTypeUID: value})}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Select organization type" />
                </SelectTrigger>
                <SelectContent>
                  {orgTypes.map((type) => (
                    <SelectItem key={type.uid} value={type.uid}>
                      {type.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="status" className="text-right">
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
            <Button onClick={handleCreateDivision}>
              {formData.uid ? 'Update' : 'Create'} Division
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Details Dialog */}
      <Dialog open={showDetailsDialog} onOpenChange={setShowDetailsDialog}>
        <DialogContent className="max-w-[90vw] max-h-[90vh] overflow-auto">
          <DialogHeader>
            <DialogTitle>
              Division Details - {selectedDivision?.name}
            </DialogTitle>
            <DialogDescription>
              View stores and distributors for this division
            </DialogDescription>
          </DialogHeader>
          
          {selectedDivision && (
            <Tabs defaultValue="stores">
              <TabsList>
                <TabsTrigger value="stores">Stores ({stores.length})</TabsTrigger>
                <TabsTrigger value="distributors">Distributors ({distributors.length})</TabsTrigger>
              </TabsList>
              
              <TabsContent value="stores">
                <Card>
                  <CardHeader>
                    <div className="flex justify-between">
                      <CardTitle>Stores</CardTitle>
                      <Button size="sm">
                        <Plus className="mr-2 h-4 w-4" />
                        Add Store
                      </Button>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Store Name</TableHead>
                          <TableHead>Code</TableHead>
                          <TableHead>Address</TableHead>
                          <TableHead>Contact</TableHead>
                          <TableHead>Status</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {stores.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={5} className="text-center">
                              No stores found for this division
                            </TableCell>
                          </TableRow>
                        ) : (
                          stores.map((store) => (
                            <TableRow key={store.uid}>
                              <TableCell className="font-medium">{store.name}</TableCell>
                              <TableCell>{store.code}</TableCell>
                              <TableCell>{store.address || '-'}</TableCell>
                              <TableCell>
                                <div className="text-sm">
                                  <p>{store.contactPerson || '-'}</p>
                                  <p className="text-muted-foreground">{store.phone || '-'}</p>
                                </div>
                              </TableCell>
                              <TableCell>
                                <Badge variant={store.isActive ? 'success' : 'destructive'}>
                                  {store.isActive ? 'Active' : 'Inactive'}
                                </Badge>
                              </TableCell>
                            </TableRow>
                          ))
                        )}
                      </TableBody>
                    </Table>
                  </CardContent>
                </Card>
              </TabsContent>
              
              <TabsContent value="distributors">
                <Card>
                  <CardHeader>
                    <div className="flex justify-between">
                      <CardTitle>Delivery Distributors</CardTitle>
                      <Button size="sm">
                        <Plus className="mr-2 h-4 w-4" />
                        Add Distributor
                      </Button>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Distributor Name</TableHead>
                          <TableHead>Code</TableHead>
                          <TableHead>Assigned Store</TableHead>
                          <TableHead>Delivery Type</TableHead>
                          <TableHead>Routes</TableHead>
                          <TableHead>Status</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {distributors.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={6} className="text-center">
                              No distributors found for this division
                            </TableCell>
                          </TableRow>
                        ) : (
                          distributors.map((distributor) => (
                            <TableRow key={distributor.uid}>
                              <TableCell className="font-medium">{distributor.name}</TableCell>
                              <TableCell>{distributor.code}</TableCell>
                              <TableCell>{distributor.storeName || '-'}</TableCell>
                              <TableCell>
                                <Badge variant="outline">
                                  {distributor.deliveryType}
                                </Badge>
                              </TableCell>
                              <TableCell>{distributor.routeCount || 0}</TableCell>
                              <TableCell>
                                <Badge variant={distributor.isActive ? 'success' : 'destructive'}>
                                  {distributor.isActive ? 'Active' : 'Inactive'}
                                </Badge>
                              </TableCell>
                            </TableRow>
                          ))
                        )}
                      </TableBody>
                    </Table>
                  </CardContent>
                </Card>
              </TabsContent>
            </Tabs>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}