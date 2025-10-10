'use client';

import { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, MapPin, Truck, Clock, CheckCircle, AlertTriangle, Phone, User, Package, Navigation, Activity } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Progress } from '@/components/ui/progress';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

interface TrackingDetail {
  id: string;
  movementCode: string;
  routeCode: string;
  loadSheetId: string;
  status: 'Loading' | 'In Transit' | 'Delivered' | 'Delayed' | 'Issue';
  currentLocation: string;
  lastUpdate: string;
  estimatedDelivery: string;
  actualDelivery?: string;
  completionPercentage: number;
  totalDistance: number;
  distanceCovered: number;
  remainingDistance: number;
  truck: TruckDetail;
  driver: DriverDetail;
  helper: HelperDetail;
  route: RouteStop[];
  deliveries: DeliveryDetail[];
  timeline: TrackingEvent[];
  alerts: TrackingAlert[];
}

interface TruckDetail {
  truckId: string;
  plateNumber: string;
  model: string;
  capacity: string;
  fuelLevel: number;
  lastMaintenance: string;
  gpsEnabled: boolean;
}

interface DriverDetail {
  name: string;
  employeeId: string;
  phone: string;
  experience: string;
  rating: number;
  licenseNumber: string;
}

interface HelperDetail {
  name: string;
  employeeId: string;
  phone: string;
  experience: string;
}

interface RouteStop {
  stopId: string;
  customerName: string;
  address: string;
  estimatedTime: string;
  actualTime?: string;
  status: 'pending' | 'current' | 'completed' | 'delayed';
  deliveryItems: number;
  contactPerson: string;
  phone: string;
  latitude: number;
  longitude: number;
}

interface DeliveryDetail {
  customerId: string;
  customerName: string;
  items: DeliveryItem[];
  totalQuantity: number;
  status: 'pending' | 'in-progress' | 'completed' | 'failed';
  deliveryTime?: string;
  notes?: string;
  signature?: string;
}

interface DeliveryItem {
  itemCode: string;
  description: string;
  quantity: number;
  delivered?: number;
  status: 'pending' | 'delivered' | 'shortage' | 'damaged';
}

interface TrackingEvent {
  timestamp: string;
  location: string;
  event: string;
  description: string;
  latitude: number;
  longitude: number;
}

interface TrackingAlert {
  id: string;
  type: 'warning' | 'error' | 'info';
  message: string;
  timestamp: string;
  resolved: boolean;
}

export default function TrackingProgressPage() {
  const router = useRouter();
  const params = useParams();
  const requestId = params?.id as string;
  
  const [tracking, setTracking] = useState<TrackingDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [autoRefresh, setAutoRefresh] = useState(true);

  useEffect(() => {
    if (requestId) {
      loadTrackingData();
    }
  }, [requestId]);

  useEffect(() => {
    if (autoRefresh && tracking) {
      const interval = setInterval(() => {
        loadTrackingData();
      }, 30000); // Refresh every 30 seconds

      return () => clearInterval(interval);
    }
  }, [autoRefresh, tracking]);

  const loadTrackingData = () => {
    // Mock data - in real app this would come from GPS/tracking API
    const mockTracking: TrackingDetail = {
      id: requestId,
      movementCode: `SKTT01E000${requestId.padStart(4, '0')}`,
      routeCode: '[SKTT01]SKTT01 - North Zone Route',
      loadSheetId: `LS-2024-00${requestId.slice(-1)}`,
      status: requestId === 'LSR-2024-001' ? 'Loading' : 'In Transit',
      currentLocation: requestId === 'LSR-2024-001' ? 'Depot North - Loading Bay 3' : 'Highway NH-48, Near Toll Plaza',
      lastUpdate: new Date().toLocaleString(),
      estimatedDelivery: '2024-01-16 16:00',
      completionPercentage: requestId === 'LSR-2024-001' ? 25 : 65,
      totalDistance: 85,
      distanceCovered: requestId === 'LSR-2024-001' ? 0 : 55,
      remainingDistance: requestId === 'LSR-2024-001' ? 85 : 30,
      truck: {
        truckId: 'TRK-001',
        plateNumber: 'MH-12-AB-1234',
        model: 'Tata LPT 1613',
        capacity: '15 Tons',
        fuelLevel: 85,
        lastMaintenance: '2024-01-10',
        gpsEnabled: true
      },
      driver: {
        name: 'John Smith',
        employeeId: 'DRV001',
        phone: '+91-9876543210',
        experience: '8 years',
        rating: 4.7,
        licenseNumber: 'MH1234567890'
      },
      helper: {
        name: 'Mike Davis',
        employeeId: 'HLP001',
        phone: '+91-9765432109',
        experience: '3 years'
      },
      route: [
        {
          stopId: 'STOP01',
          customerName: 'ABC Retail Store',
          address: '123 Main Street, Downtown, City - 400001',
          estimatedTime: '10:30',
          actualTime: requestId === 'LSR-2024-001' ? undefined : '10:45',
          status: requestId === 'LSR-2024-001' ? 'pending' : 'completed',
          deliveryItems: 35,
          contactPerson: 'Rajesh Sharma',
          phone: '+91-9876543210',
          latitude: 19.0760,
          longitude: 72.8777
        },
        {
          stopId: 'STOP02',
          customerName: 'XYZ Supermarket',
          address: '456 Mall Road, Central District, City - 400002',
          estimatedTime: '13:00',
          actualTime: undefined,
          status: requestId === 'LSR-2024-001' ? 'pending' : 'current',
          deliveryItems: 30,
          contactPerson: 'Priya Patel',
          phone: '+91-9765432109',
          latitude: 19.0896,
          longitude: 72.8656
        },
        {
          stopId: 'STOP03',
          customerName: 'PQR General Store',
          address: '789 Corner Lane, Suburb Area, City - 400003',
          estimatedTime: '15:30',
          status: 'pending',
          deliveryItems: 20,
          contactPerson: 'Amit Kumar',
          phone: '+91-9654321098',
          latitude: 19.1136,
          longitude: 72.8697
        }
      ],
      deliveries: [
        {
          customerId: 'CUST001',
          customerName: 'ABC Retail Store',
          totalQuantity: 35,
          status: requestId === 'LSR-2024-001' ? 'pending' : 'completed',
          deliveryTime: requestId === 'LSR-2024-001' ? undefined : '10:45',
          notes: requestId === 'LSR-2024-001' ? undefined : 'Delivery completed successfully. All items verified.',
          signature: requestId === 'LSR-2024-001' ? undefined : 'R. Sharma',
          items: [
            { itemCode: 'SKU001', description: 'Choco Bar 90ml', quantity: 20, delivered: requestId === 'LSR-2024-001' ? undefined : 20, status: requestId === 'LSR-2024-001' ? 'pending' : 'delivered' },
            { itemCode: 'SKU002', description: 'Vanilla Tub 500ml', quantity: 10, delivered: requestId === 'LSR-2024-001' ? undefined : 10, status: requestId === 'LSR-2024-001' ? 'pending' : 'delivered' },
            { itemCode: 'SKU003', description: 'Strawberry Cup 250ml', quantity: 5, delivered: requestId === 'LSR-2024-001' ? undefined : 5, status: requestId === 'LSR-2024-001' ? 'pending' : 'delivered' }
          ]
        },
        {
          customerId: 'CUST002',
          customerName: 'XYZ Supermarket',
          totalQuantity: 30,
          status: requestId === 'LSR-2024-001' ? 'pending' : 'in-progress',
          items: [
            { itemCode: 'SKU001', description: 'Choco Bar 90ml', quantity: 15, status: 'pending' },
            { itemCode: 'SKU002', description: 'Vanilla Tub 500ml', quantity: 10, status: 'pending' },
            { itemCode: 'SKU003', description: 'Strawberry Cup 250ml', quantity: 5, status: 'pending' }
          ]
        },
        {
          customerId: 'CUST003',
          customerName: 'PQR General Store',
          totalQuantity: 20,
          status: 'pending',
          items: [
            { itemCode: 'SKU001', description: 'Choco Bar 90ml', quantity: 10, status: 'pending' },
            { itemCode: 'SKU002', description: 'Vanilla Tub 500ml', quantity: 10, status: 'pending' }
          ]
        }
      ],
      timeline: [
        {
          timestamp: '2024-01-16 08:00',
          location: 'Depot North',
          event: 'Loading Started',
          description: 'Truck arrived at loading bay and commenced loading process',
          latitude: 19.0830,
          longitude: 72.8258
        },
        ...(requestId !== 'LSR-2024-001' ? [
          {
            timestamp: '2024-01-16 09:15',
            location: 'Depot North - Gate 2',
            event: 'Departed Depot',
            description: 'Truck departed from depot after successful loading and inspection',
            latitude: 19.0830,
            longitude: 72.8258
          },
          {
            timestamp: '2024-01-16 10:45',
            location: 'ABC Retail Store',
            event: 'First Delivery Completed',
            description: 'Successfully delivered 35 items to ABC Retail Store',
            latitude: 19.0760,
            longitude: 72.8777
          },
          {
            timestamp: '2024-01-16 12:30',
            location: 'Highway NH-48',
            event: 'En Route to Next Stop',
            description: 'Traveling towards XYZ Supermarket for next delivery',
            latitude: 19.0896,
            longitude: 72.8656
          }
        ] : [])
      ],
      alerts: requestId === 'LSR-2024-001' ? [
        {
          id: 'ALT001',
          type: 'info',
          message: 'Loading in progress - Expected completion by 09:00',
          timestamp: '2024-01-16 08:30',
          resolved: false
        }
      ] : [
        {
          id: 'ALT001',
          type: 'warning',
          message: 'Traffic congestion detected on route - 15 min delay expected',
          timestamp: '2024-01-16 12:15',
          resolved: false
        }
      ]
    };

    setTracking(mockTracking);
    setLoading(false);
  };

  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case 'loading':
        return <Badge variant="secondary">Loading</Badge>;
      case 'in transit':
        return <Badge variant="default">In Transit</Badge>;
      case 'delivered':
        return <Badge variant="outline">Delivered</Badge>;
      case 'delayed':
        return <Badge variant="destructive">Delayed</Badge>;
      case 'issue':
        return <Badge variant="destructive">Issue</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const getStopStatusIcon = (status: string) => {
    switch (status) {
      case 'completed':
        return <CheckCircle className="w-4 h-4 text-primary" />;
      case 'current':
        return <Activity className="w-4 h-4 text-secondary animate-pulse" />;
      case 'delayed':
        return <AlertTriangle className="w-4 h-4 text-destructive" />;
      default:
        return <Clock className="w-4 h-4 text-muted-foreground" />;
    }
  };

  if (loading) {
    return (
      <div className="p-8">
        <div className="max-w-7xl mx-auto">
          {/* Header Skeleton */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                <Skeleton className="h-9 w-20" />
                <div>
                  <Skeleton className="h-8 w-48 mb-2" />
                  <Skeleton className="h-4 w-96" />
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <Skeleton className="h-6 w-24" />
                <Skeleton className="h-9 w-36" />
              </div>
            </div>
          </div>

          {/* Status Overview Skeleton */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            {[1, 2, 3, 4].map((i) => (
              <Card key={i}>
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <Skeleton className="h-4 w-28 mb-2" />
                      <Skeleton className="h-8 w-20 mb-2" />
                      <Skeleton className="h-3 w-24" />
                    </div>
                    <Skeleton className="h-8 w-8 rounded-full" />
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Tabs Skeleton */}
          <div className="space-y-6">
            <Skeleton className="h-10 w-96" />

            {/* Route Progress Card Skeleton */}
            <Card>
              <CardHeader>
                <Skeleton className="h-6 w-52 mb-2" />
                <Skeleton className="h-4 w-96" />
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {[1, 2, 3].map((i) => (
                    <div key={i} className="flex items-start space-x-4">
                      <div className="flex flex-col items-center">
                        <Skeleton className="h-4 w-4 rounded-full" />
                        {i < 3 && <Skeleton className="w-px h-12 my-1" />}
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex-1">
                            <Skeleton className="h-5 w-48 mb-2" />
                            <Skeleton className="h-4 w-72" />
                          </div>
                          <div className="text-right">
                            <Skeleton className="h-5 w-20 mb-1 ml-auto" />
                            <Skeleton className="h-3 w-32 ml-auto" />
                          </div>
                        </div>
                        <div className="flex items-center space-x-4">
                          <Skeleton className="h-4 w-16" />
                          <Skeleton className="h-4 w-24" />
                          <Skeleton className="h-4 w-28" />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    );
  }

  if (!tracking) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <MapPin className="w-16 h-16 text-muted-foreground mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-foreground mb-2">Tracking Not Available</h2>
          <p className="text-muted-foreground mb-4">Unable to load tracking information for this request.</p>
          <Button onClick={() => router.push('/load-management/logistics-approval')}>
            Back to Logistics Approval
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => router.push('/load-management/logistics-approval')}
                className="flex items-center gap-2"
              >
                <ArrowLeft className="w-4 h-4" />
                Back to Logistics Approval
              </Button>
              <div>
                <h1 className="text-2xl font-bold">Load Tracking</h1>
                <p className="text-sm text-muted-foreground mt-1">
                  Live tracking for {tracking.movementCode} • Last updated: {tracking.lastUpdate}
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              {getStatusBadge(tracking.status)}
              <Button
                variant="outline"
                size="sm"
                onClick={() => setAutoRefresh(!autoRefresh)}
                className={autoRefresh ? 'bg-primary text-primary-foreground' : ''}
              >
                {autoRefresh ? 'Auto Refresh On' : 'Auto Refresh Off'}
              </Button>
            </div>
          </div>
        </div>

        {/* Status Overview */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Progress</p>
                  <p className="text-2xl font-bold text-foreground">{tracking.completionPercentage}%</p>
                  <Progress value={tracking.completionPercentage} className="w-full mt-2" />
                </div>
                <Package className="w-8 h-8 text-muted-foreground" />
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Distance Covered</p>
                  <p className="text-2xl font-bold text-foreground">{tracking.distanceCovered} km</p>
                  <p className="text-sm text-muted-foreground">of {tracking.totalDistance} km</p>
                </div>
                <Navigation className="w-8 h-8 text-muted-foreground" />
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Remaining Distance</p>
                  <p className="text-2xl font-bold text-foreground">{tracking.remainingDistance} km</p>
                  <p className="text-sm text-muted-foreground">Est. {new Date(tracking.estimatedDelivery).toLocaleTimeString()}</p>
                </div>
                <Clock className="w-8 h-8 text-muted-foreground" />
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Current Location</p>
                  <p className="text-sm font-bold text-foreground">{tracking.currentLocation}</p>
                  <p className="text-sm text-muted-foreground">{tracking.status}</p>
                </div>
                <MapPin className="w-8 h-8 text-muted-foreground" />
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Alerts */}
        {tracking.alerts.length > 0 && (
          <div className="mb-8">
            <h2 className="text-lg font-semibold mb-4">Active Alerts</h2>
            <div className="space-y-3">
              {tracking.alerts.map((alert) => (
                <Card key={alert.id} className={`border-l-4 ${
                  alert.type === 'error' ? 'border-l-destructive' :
                  alert.type === 'warning' ? 'border-l-secondary' : 'border-l-primary'
                }`}>
                  <CardContent className="p-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-2">
                        {alert.type === 'error' ? (
                          <AlertTriangle className="w-4 h-4 text-destructive" />
                        ) : alert.type === 'warning' ? (
                          <AlertTriangle className="w-4 h-4 text-secondary" />
                        ) : (
                          <Activity className="w-4 h-4 text-primary" />
                        )}
                        <p className="text-sm font-medium">{alert.message}</p>
                      </div>
                      <p className="text-xs text-muted-foreground">{alert.timestamp}</p>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>
        )}

        <Tabs defaultValue="route" className="space-y-6">
          <TabsList>
            <TabsTrigger value="route">Route Progress</TabsTrigger>
            <TabsTrigger value="vehicle">Vehicle Info</TabsTrigger>
            <TabsTrigger value="deliveries">Deliveries</TabsTrigger>
            <TabsTrigger value="timeline">Timeline</TabsTrigger>
          </TabsList>

          <TabsContent value="route" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <MapPin className="h-5 w-5" />
                  Delivery Route Progress
                </CardTitle>
                <CardDescription>
                  Current progress along the planned delivery route
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {tracking.route.map((stop, index) => (
                    <div key={stop.stopId} className="flex items-start space-x-4">
                      <div className="flex flex-col items-center">
                        {getStopStatusIcon(stop.status)}
                        {index < tracking.route.length - 1 && (
                          <div className={`w-px h-12 ${
                            stop.status === 'completed' ? 'bg-primary' : 'bg-muted'
                          }`} />
                        )}
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center justify-between">
                          <div>
                            <h4 className="font-medium text-foreground">{stop.customerName}</h4>
                            <p className="text-sm text-muted-foreground">{stop.address}</p>
                          </div>
                          <div className="text-right">
                            <Badge variant={
                              stop.status === 'completed' ? 'default' :
                              stop.status === 'current' ? 'secondary' :
                              stop.status === 'delayed' ? 'destructive' : 'outline'
                            }>
                              {stop.status}
                            </Badge>
                            <p className="text-xs text-muted-foreground mt-1">
                              Est: {stop.estimatedTime}
                              {stop.actualTime && ` | Actual: ${stop.actualTime}`}
                            </p>
                          </div>
                        </div>
                        <div className="mt-2 flex items-center space-x-4 text-sm text-muted-foreground">
                          <div className="flex items-center space-x-1">
                            <Package className="w-3 h-3" />
                            <span>{stop.deliveryItems} items</span>
                          </div>
                          <div className="flex items-center space-x-1">
                            <User className="w-3 h-3" />
                            <span>{stop.contactPerson}</span>
                          </div>
                          <div className="flex items-center space-x-1">
                            <Phone className="w-3 h-3" />
                            <span>{stop.phone}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="vehicle" className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {/* Vehicle Details */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Truck className="h-5 w-5" />
                    Vehicle Details
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Truck ID</span>
                    <p className="font-semibold">{tracking.truck.truckId}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Plate Number</span>
                    <p className="font-semibold">{tracking.truck.plateNumber}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Model</span>
                    <p className="font-semibold">{tracking.truck.model}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Capacity</span>
                    <p className="font-semibold">{tracking.truck.capacity}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Fuel Level</span>
                    <div className="flex items-center space-x-2">
                      <Progress value={tracking.truck.fuelLevel} className="flex-1" />
                      <span className="text-sm font-medium">{tracking.truck.fuelLevel}%</span>
                    </div>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">GPS Status</span>
                    <Badge variant={tracking.truck.gpsEnabled ? 'default' : 'destructive'}>
                      {tracking.truck.gpsEnabled ? 'Active' : 'Inactive'}
                    </Badge>
                  </div>
                </CardContent>
              </Card>

              {/* Driver Details */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <User className="h-5 w-5" />
                    Driver Information
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Name</span>
                    <p className="font-semibold">{tracking.driver.name}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Employee ID</span>
                    <p className="font-semibold">{tracking.driver.employeeId}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Phone</span>
                    <div className="flex items-center space-x-2">
                      <p className="font-semibold">{tracking.driver.phone}</p>
                      <Button variant="outline" size="sm">
                        <Phone className="w-3 h-3" />
                      </Button>
                    </div>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Experience</span>
                    <p className="font-semibold">{tracking.driver.experience}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Rating</span>
                    <p className="font-semibold">{tracking.driver.rating}/5.0</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">License</span>
                    <p className="font-semibold">{tracking.driver.licenseNumber}</p>
                  </div>
                </CardContent>
              </Card>

              {/* Helper Details */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <User className="h-5 w-5" />
                    Helper Information
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Name</span>
                    <p className="font-semibold">{tracking.helper.name}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Employee ID</span>
                    <p className="font-semibold">{tracking.helper.employeeId}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Phone</span>
                    <div className="flex items-center space-x-2">
                      <p className="font-semibold">{tracking.helper.phone}</p>
                      <Button variant="outline" size="sm">
                        <Phone className="w-3 h-3" />
                      </Button>
                    </div>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Experience</span>
                    <p className="font-semibold">{tracking.helper.experience}</p>
                  </div>
                </CardContent>
              </Card>
            </div>
          </TabsContent>

          <TabsContent value="deliveries" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Delivery Status</CardTitle>
                <CardDescription>
                  Detailed status of each delivery in this load
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {tracking.deliveries.map((delivery, index) => (
                    <Card key={delivery.customerId} className="border-l-4 border-l-primary">
                      <CardContent className="p-6">
                        <div className="flex items-center justify-between mb-4">
                          <div>
                            <h4 className="font-semibold text-foreground">{delivery.customerName}</h4>
                            <p className="text-sm text-muted-foreground">
                              {delivery.totalQuantity} items • {delivery.deliveryTime || 'Pending delivery'}
                            </p>
                          </div>
                          <div className="flex items-center space-x-2">
                            <Badge variant={
                              delivery.status === 'completed' ? 'default' :
                              delivery.status === 'in-progress' ? 'secondary' :
                              delivery.status === 'failed' ? 'destructive' : 'outline'
                            }>
                              {delivery.status}
                            </Badge>
                          </div>
                        </div>
                        
                        <div className="space-y-2">
                          {delivery.items.map((item, itemIndex) => (
                            <div key={itemIndex} className="flex items-center justify-between py-2 border-t">
                              <div>
                                <p className="font-medium text-sm">{item.itemCode}</p>
                                <p className="text-xs text-muted-foreground">{item.description}</p>
                              </div>
                              <div className="text-right">
                                <p className="font-medium text-sm">
                                  {item.delivered || 0} / {item.quantity}
                                </p>
                                <Badge variant={
                                  item.status === 'delivered' ? 'default' :
                                  item.status === 'shortage' ? 'secondary' :
                                  item.status === 'damaged' ? 'destructive' : 'outline'
                                } className="text-xs">
                                  {item.status}
                                </Badge>
                              </div>
                            </div>
                          ))}
                        </div>

                        {delivery.notes && (
                          <div className="mt-4 p-3 bg-muted rounded-lg">
                            <p className="text-sm text-muted-foreground">{delivery.notes}</p>
                          </div>
                        )}

                        {delivery.signature && (
                          <div className="mt-4">
                            <p className="text-sm font-medium">Signed by: {delivery.signature}</p>
                          </div>
                        )}
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="timeline" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Clock className="h-5 w-5" />
                  Tracking Timeline
                </CardTitle>
                <CardDescription>
                  Real-time location and activity updates
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {tracking.timeline.map((event, index) => (
                    <div key={index} className="flex items-start space-x-4">
                      <div className="flex-shrink-0 w-4 h-4 bg-primary rounded-full mt-1" />
                      <div className="flex-1">
                        <div className="flex items-center justify-between">
                          <div>
                            <h4 className="font-medium text-foreground">{event.event}</h4>
                            <p className="text-sm text-muted-foreground">{event.description}</p>
                          </div>
                          <div className="text-right text-xs text-muted-foreground">
                            <p>{event.timestamp}</p>
                            <p className="font-medium">{event.location}</p>
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}