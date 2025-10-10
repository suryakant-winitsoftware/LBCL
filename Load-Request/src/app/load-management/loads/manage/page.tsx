"use client";

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { useToast } from '@/components/ui/use-toast';
import {
  Plus,
  Search,
  RefreshCw,
  Download,
  Filter,
  Truck,
  Package,
  Calendar,
  MapPin,
  Clock,
  CheckCircle,
  XCircle,
  AlertCircle,
  Edit,
  Trash2,
  Eye,
} from 'lucide-react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { apiService } from '@/services/api';

interface Load {
  id: string;
  loadNumber: string;
  vehicleNumber: string;
  driverName: string;
  driverContact: string;
  routeId: string;
  routeName: string;
  loadDate: string;
  startTime: string;
  endTime: string;
  totalItems: number;
  totalWeight: number;
  totalVolume: number;
  deliveryCount: number;
  completedDeliveries: number;
  pendingDeliveries: number;
  failedDeliveries: number;
  status: 'scheduled' | 'loading' | 'in-transit' | 'completed' | 'cancelled';
  capacity: number;
  utilizationPercent: number;
  estimatedDistance: number;
  actualDistance: number;
  fuelConsumption: number;
  createdAt: string;
  updatedAt: string;
}

interface FilterState {
  search: string;
  vehicleNumber: string;
  driverName: string;
  routeId: string;
  status: string;
  dateRange: {
    startDate: string;
    endDate: string;
  };
  utilizationMin: number;
  utilizationMax: number;
}

const LoadManagement: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();

  const [loads, setLoads] = useState<Load[]>([]);
  const [loading, setLoading] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteLoadId, setDeleteLoadId] = useState<string | null>(null);
  const [routes, setRoutes] = useState<{ value: string; label: string }[]>([]);
  const [vehicles, setVehicles] = useState<{ value: string; label: string }[]>([]);

  const [filters, setFilters] = useState<FilterState>({
    search: '',
    vehicleNumber: '',
    driverName: '',
    routeId: '',
    status: '',
    dateRange: {
      startDate: '',
      endDate: '',
    },
    utilizationMin: 0,
    utilizationMax: 100,
  });

  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 10,
    total: 0,
  });

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    loadLoads();
  }, [filters, pagination.current, pagination.pageSize]);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      
      // Mock data for demonstration
      setRoutes([
        { value: 'RT001', label: 'North Zone Route' },
        { value: 'RT002', label: 'South Zone Route' },
        { value: 'RT003', label: 'East Zone Route' },
        { value: 'RT004', label: 'West Zone Route' },
      ]);

      setVehicles([
        { value: 'VH001', label: 'MH-12-AB-1234' },
        { value: 'VH002', label: 'MH-12-CD-5678' },
        { value: 'VH003', label: 'MH-12-EF-9012' },
        { value: 'VH004', label: 'MH-12-GH-3456' },
      ]);

    } catch (error) {
      console.error('Error loading initial data:', error);
      toast({
        title: 'Error',
        description: 'Failed to load initial data',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const loadLoads = async () => {
    try {
      setLoading(true);
      
      // Mock data for demonstration
      const mockLoads: Load[] = [
        {
          id: 'LD001',
          loadNumber: 'LOAD-2024-001',
          vehicleNumber: 'MH-12-AB-1234',
          driverName: 'John Doe',
          driverContact: '+91 9876543210',
          routeId: 'RT001',
          routeName: 'North Zone Route',
          loadDate: '2024-01-15',
          startTime: '08:00',
          endTime: '18:00',
          totalItems: 250,
          totalWeight: 1500,
          totalVolume: 2000,
          deliveryCount: 25,
          completedDeliveries: 20,
          pendingDeliveries: 3,
          failedDeliveries: 2,
          status: 'in-transit',
          capacity: 2000,
          utilizationPercent: 75,
          estimatedDistance: 150,
          actualDistance: 0,
          fuelConsumption: 0,
          createdAt: '2024-01-14T10:00:00Z',
          updatedAt: '2024-01-15T08:00:00Z',
        },
        {
          id: 'LD002',
          loadNumber: 'LOAD-2024-002',
          vehicleNumber: 'MH-12-CD-5678',
          driverName: 'Jane Smith',
          driverContact: '+91 9876543211',
          routeId: 'RT002',
          routeName: 'South Zone Route',
          loadDate: '2024-01-15',
          startTime: '09:00',
          endTime: '19:00',
          totalItems: 300,
          totalWeight: 1800,
          totalVolume: 2500,
          deliveryCount: 30,
          completedDeliveries: 30,
          pendingDeliveries: 0,
          failedDeliveries: 0,
          status: 'completed',
          capacity: 2500,
          utilizationPercent: 72,
          estimatedDistance: 180,
          actualDistance: 185,
          fuelConsumption: 25,
          createdAt: '2024-01-14T11:00:00Z',
          updatedAt: '2024-01-15T19:00:00Z',
        },
        {
          id: 'LD003',
          loadNumber: 'LOAD-2024-003',
          vehicleNumber: 'MH-12-EF-9012',
          driverName: 'Mike Johnson',
          driverContact: '+91 9876543212',
          routeId: 'RT003',
          routeName: 'East Zone Route',
          loadDate: '2024-01-16',
          startTime: '08:30',
          endTime: '18:30',
          totalItems: 200,
          totalWeight: 1200,
          totalVolume: 1800,
          deliveryCount: 20,
          completedDeliveries: 0,
          pendingDeliveries: 20,
          failedDeliveries: 0,
          status: 'scheduled',
          capacity: 2000,
          utilizationPercent: 60,
          estimatedDistance: 120,
          actualDistance: 0,
          fuelConsumption: 0,
          createdAt: '2024-01-15T14:00:00Z',
          updatedAt: '2024-01-15T14:00:00Z',
        },
      ];

      setLoads(mockLoads);
      setPagination(prev => ({ ...prev, total: mockLoads.length }));

    } catch (error) {
      console.error('Error loading loads:', error);
      toast({
        title: 'Error',
        description: 'Failed to load loads',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteLoadId) return;

    try {
      setLoading(true);
      
      // API call would go here
      toast({
        title: 'Success',
        description: 'Load deleted successfully',
      });

      setDeleteDialogOpen(false);
      setDeleteLoadId(null);
      loadLoads();
    } catch (error) {
      console.error('Error deleting load:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete load',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: Load['status']) => {
    const statusConfig = {
      scheduled: { label: 'Scheduled', variant: 'secondary' as const, icon: Calendar },
      loading: { label: 'Loading', variant: 'outline' as const, icon: Package },
      'in-transit': { label: 'In Transit', variant: 'default' as const, icon: Truck },
      completed: { label: 'Completed', variant: 'success' as const, icon: CheckCircle },
      cancelled: { label: 'Cancelled', variant: 'destructive' as const, icon: XCircle },
    };

    const config = statusConfig[status];
    const Icon = config.icon;

    return (
      <Badge variant={config.variant} className="gap-1">
        <Icon className="h-3 w-3" />
        {config.label}
      </Badge>
    );
  };

  const getUtilizationColor = (percent: number) => {
    if (percent >= 90) return 'text-foreground';
    if (percent >= 70) return 'text-foreground';
    if (percent >= 50) return 'text-muted-foreground';
    return 'text-destructive';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Load Management</h1>
          <p className="text-muted-foreground">
            Manage vehicle loads, track deliveries, and monitor capacity utilization
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => loadLoads()}
            disabled={loading}
          >
            <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Button
            variant="outline"
            onClick={() => toast({ title: 'Export feature coming soon' })}
          >
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button
            onClick={() => router.push('/updatedfeatures/load-management/loads/create')}
          >
            <Plus className="h-4 w-4 mr-2" />
            Create Load
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Loads
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{loads.length}</div>
            <p className="text-xs text-muted-foreground">Active today</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              In Transit
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loads.filter(l => l.status === 'in-transit').length}
            </div>
            <p className="text-xs text-muted-foreground">Currently moving</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Avg. Utilization
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loads.length > 0
                ? Math.round(loads.reduce((acc, l) => acc + l.utilizationPercent, 0) / loads.length)
                : 0}%
            </div>
            <p className="text-xs text-muted-foreground">Capacity usage</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Deliveries
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loads.reduce((acc, l) => acc + l.completedDeliveries, 0)}
            </div>
            <p className="text-xs text-muted-foreground">Completed today</p>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg flex items-center gap-2">
            <Filter className="h-4 w-4" />
            Filters
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <div>
              <label className="text-sm font-medium">Search</label>
              <Input
                placeholder="Load number, driver..."
                value={filters.search}
                onChange={(e) => setFilters({ ...filters, search: e.target.value })}
                className="mt-1"
              />
            </div>
            <div>
              <label className="text-sm font-medium">Vehicle</label>
              <Select
                value={filters.vehicleNumber}
                onValueChange={(value) => setFilters({ ...filters, vehicleNumber: value })}
              >
                <SelectTrigger className="mt-1">
                  <SelectValue placeholder="All vehicles" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">All vehicles</SelectItem>
                  {vehicles.map((vehicle) => (
                    <SelectItem key={vehicle.value} value={vehicle.value}>
                      {vehicle.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div>
              <label className="text-sm font-medium">Route</label>
              <Select
                value={filters.routeId}
                onValueChange={(value) => setFilters({ ...filters, routeId: value })}
              >
                <SelectTrigger className="mt-1">
                  <SelectValue placeholder="All routes" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">All routes</SelectItem>
                  {routes.map((route) => (
                    <SelectItem key={route.value} value={route.value}>
                      {route.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div>
              <label className="text-sm font-medium">Status</label>
              <Select
                value={filters.status}
                onValueChange={(value) => setFilters({ ...filters, status: value })}
              >
                <SelectTrigger className="mt-1">
                  <SelectValue placeholder="All status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">All status</SelectItem>
                  <SelectItem value="scheduled">Scheduled</SelectItem>
                  <SelectItem value="loading">Loading</SelectItem>
                  <SelectItem value="in-transit">In Transit</SelectItem>
                  <SelectItem value="completed">Completed</SelectItem>
                  <SelectItem value="cancelled">Cancelled</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Loads Table */}
      <Card>
        <CardHeader>
          <CardTitle>Loads</CardTitle>
          <CardDescription>
            View and manage all vehicle loads
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Load Number</TableHead>
                <TableHead>Vehicle</TableHead>
                <TableHead>Driver</TableHead>
                <TableHead>Route</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Deliveries</TableHead>
                <TableHead>Utilization</TableHead>
                <TableHead>Schedule</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={9} className="text-center py-8">
                    <div className="flex items-center justify-center gap-2">
                      <RefreshCw className="h-4 w-4 animate-spin" />
                      Loading loads...
                    </div>
                  </TableCell>
                </TableRow>
              ) : loads.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} className="text-center py-8 text-muted-foreground">
                    No loads found
                  </TableCell>
                </TableRow>
              ) : (
                loads.map((load) => (
                  <TableRow key={load.id}>
                    <TableCell className="font-medium">
                      {load.loadNumber}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Truck className="h-4 w-4 text-muted-foreground" />
                        {load.vehicleNumber}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{load.driverName}</p>
                        <p className="text-xs text-muted-foreground">{load.driverContact}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <MapPin className="h-4 w-4 text-muted-foreground" />
                        {load.routeName}
                      </div>
                    </TableCell>
                    <TableCell>
                      {getStatusBadge(load.status)}
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="flex items-center gap-1 text-sm">
                          <CheckCircle className="h-3 w-3 text-muted-foreground" />
                          <span>{load.completedDeliveries}</span>
                        </div>
                        <div className="flex items-center gap-1 text-sm">
                          <Clock className="h-3 w-3 text-muted-foreground" />
                          <span>{load.pendingDeliveries}</span>
                        </div>
                        {load.failedDeliveries > 0 && (
                          <div className="flex items-center gap-1 text-sm">
                            <XCircle className="h-3 w-3 text-destructive" />
                            <span>{load.failedDeliveries}</span>
                          </div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <div className={`font-medium ${getUtilizationColor(load.utilizationPercent)}`}>
                          {load.utilizationPercent}%
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {load.totalWeight}kg / {load.capacity}kg
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="text-sm">
                        <p>{load.loadDate}</p>
                        <p className="text-xs text-muted-foreground">
                          {load.startTime} - {load.endTime}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => router.push(`/updatedfeatures/load-management/loads/view/${load.id}`)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => router.push(`/updatedfeatures/load-management/loads/edit/${load.id}`)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            setDeleteLoadId(load.id);
                            setDeleteDialogOpen(true);
                          }}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Load</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this load? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete}>Delete</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default LoadManagement;