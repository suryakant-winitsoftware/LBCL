'use client';

import { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, Package, Calendar, User, MapPin, Truck, Clock, CheckCircle, AlertCircle, FileText, Users } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

interface ApprovalRequestDetail {
  id: string;
  movementCode: string;
  routeCode: string;
  salesmanId: string;
  salesmanName: string;
  loadType: 'Normal' | 'Emergency';
  submittedDate: string;
  requiredDate: string;
  processedDate: string;
  status: 'Approved' | 'Rejected';
  priority: 'High' | 'Medium' | 'Low';
  requestedLoad: string;
  approvedLoad: string;
  assignedTruck: string;
  assignedDriver: string;
  assignedHelper: string;
  depot: string;
  notes: string;
  approvedBy: string;
  rejectionReason?: string;
  loadSheetId: string;
  estimatedDelivery?: string;
  actualDelivery?: string;
  completionPercentage: number;
  items: ApprovalItem[];
  customers: CustomerDetail[];
  timeline: TimelineEvent[];
  approvalMetrics: ApprovalMetrics;
}

interface ApprovalItem {
  itemCode: string;
  description: string;
  requestedQuantity: number;
  approvedQuantity: number;
  uom: string;
  weight: number;
  volume: number;
  stockAvailable: number;
  adjustmentReason?: string;
}

interface CustomerDetail {
  customer_name: string;
  outlet_name: string;
  contact_person: string;
  phone: string;
  email: string;
  quantity: number;
  outlet_type: string;
  order_date: string;
  delivery_date: string;
  address: string;
  priority: 'High' | 'Medium' | 'Low';
}

interface TimelineEvent {
  date: string;
  time: string;
  event: string;
  description: string;
  actor: string;
  status: 'completed' | 'current' | 'pending';
}

interface ApprovalMetrics {
  totalRequestedQuantity: number;
  totalApprovedQuantity: number;
  approvalRate: number;
  processingTime: string;
  stockUtilization: number;
}

export default function LogisticsApprovalDetailPage() {
  const router = useRouter();
  const params = useParams();
  const requestId = params?.id as string;
  
  const [request, setRequest] = useState<ApprovalRequestDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (requestId) {
      loadApprovalDetails();
    }
  }, [requestId]);

  const loadApprovalDetails = () => {
    // Mock data - in real app this would come from API
    const mockRequest: ApprovalRequestDetail = {
      id: requestId,
      movementCode: `SKTT01E000${requestId.padStart(4, '0')}`,
      routeCode: '[SKTT01]SKTT01 - North Zone Route',
      salesmanId: '[LSR001]',
      salesmanName: 'John Kumar',
      loadType: requestId === 'LSR-2024-002' ? 'Emergency' : 'Normal',
      submittedDate: '2024-01-15',
      requiredDate: '2024-01-16',
      processedDate: '2024-01-15',
      status: requestId === 'LSR-2024-003' ? 'Rejected' : 'Approved',
      priority: requestId === 'LSR-2024-001' ? 'High' : 'Medium',
      requestedLoad: '850 kg',
      approvedLoad: '800 kg',
      assignedTruck: 'TRK-001',
      assignedDriver: 'John Smith',
      assignedHelper: 'Mike Davis',
      depot: 'Depot North',
      notes: requestId === 'LSR-2024-003' 
        ? 'Request rejected due to insufficient stock availability and route capacity constraints'
        : 'Approved with minor quantity adjustments for optimal route loading and stock management',
      approvedBy: 'Agent Smith',
      rejectionReason: requestId === 'LSR-2024-003' ? 'Insufficient stock availability and route capacity exceeded' : undefined,
      loadSheetId: `LS-2024-00${requestId.slice(-1)}`,
      estimatedDelivery: requestId !== 'LSR-2024-003' ? '2024-01-16 16:00' : undefined,
      actualDelivery: requestId === 'LSR-2024-002' ? '2024-01-16 15:30' : undefined,
      completionPercentage: requestId === 'LSR-2024-002' ? 100 : requestId === 'LSR-2024-001' ? 75 : 0,
      items: [
        {
          itemCode: 'SKU001',
          description: 'Choco Bar 90ml',
          requestedQuantity: 50,
          approvedQuantity: requestId === 'LSR-2024-003' ? 0 : 45,
          uom: 'Carton',
          weight: 250,
          volume: 500,
          stockAvailable: requestId === 'LSR-2024-003' ? 20 : 100,
          adjustmentReason: requestId === 'LSR-2024-003' ? 'Insufficient stock' : 'Route optimization'
        },
        {
          itemCode: 'SKU002', 
          description: 'Vanilla Tub 500ml',
          requestedQuantity: 30,
          approvedQuantity: requestId === 'LSR-2024-003' ? 0 : 30,
          uom: 'Box',
          weight: 180,
          volume: 400,
          stockAvailable: requestId === 'LSR-2024-003' ? 15 : 80,
          adjustmentReason: requestId === 'LSR-2024-003' ? 'Insufficient stock' : undefined
        },
        {
          itemCode: 'SKU003',
          description: 'Strawberry Cup 250ml', 
          requestedQuantity: 25,
          approvedQuantity: requestId === 'LSR-2024-003' ? 0 : 20,
          uom: 'Case',
          weight: 125,
          volume: 300,
          stockAvailable: requestId === 'LSR-2024-003' ? 10 : 60,
          adjustmentReason: requestId === 'LSR-2024-003' ? 'Insufficient stock' : 'Load balancing'
        }
      ],
      customers: [
        {
          customer_name: 'ABC Retail Store',
          outlet_name: 'ABC Downtown Branch',
          contact_person: 'Rajesh Sharma',
          phone: '+91-9876543210',
          email: 'rajesh@abcretail.com',
          quantity: 35,
          outlet_type: 'Retail',
          order_date: '2024-01-14',
          delivery_date: '2024-01-16',
          address: '123 Main Street, Downtown, City - 400001',
          priority: 'High'
        },
        {
          customer_name: 'XYZ Supermarket',
          outlet_name: 'XYZ Mall Outlet',
          contact_person: 'Priya Patel',
          phone: '+91-9765432109',
          email: 'priya@xyzsupermarket.com',
          quantity: 30,
          outlet_type: 'Supermarket',
          order_date: '2024-01-14',
          delivery_date: '2024-01-16',
          address: '456 Mall Road, Central District, City - 400002',
          priority: 'Medium'
        },
        {
          customer_name: 'PQR General Store',
          outlet_name: 'PQR Corner Shop',
          contact_person: 'Amit Kumar',
          phone: '+91-9654321098',
          email: 'amit@pqrstore.com',
          quantity: 20,
          outlet_type: 'General Store',
          order_date: '2024-01-14',
          delivery_date: '2024-01-16',
          address: '789 Corner Lane, Suburb Area, City - 400003',
          priority: 'Low'
        }
      ],
      timeline: [
        {
          date: '2024-01-15',
          time: '09:00',
          event: 'Request Received',
          description: 'Load request received from LSR',
          actor: 'System',
          status: 'completed'
        },
        {
          date: '2024-01-15',
          time: '09:15',
          event: 'Assigned for Review',
          description: 'Request assigned to logistics team for review',
          actor: 'System',
          status: 'completed'
        },
        {
          date: '2024-01-15', 
          time: '10:30',
          event: 'Review Started',
          description: 'Logistics officer started reviewing request',
          actor: 'Agent Smith',
          status: 'completed'
        },
        {
          date: '2024-01-15',
          time: '11:45', 
          event: 'Stock Verification',
          description: 'Stock availability verified for all items',
          actor: 'Agent Smith',
          status: 'completed'
        },
        {
          date: '2024-01-15',
          time: '13:00',
          event: 'Route Planning',
          description: 'Optimal route and truck assignment planned',
          actor: 'Agent Smith',
          status: 'completed'
        },
        {
          date: '2024-01-15',
          time: '14:00', 
          event: requestId === 'LSR-2024-003' ? 'Request Rejected' : 'Request Approved',
          description: requestId === 'LSR-2024-003' 
            ? 'Request rejected due to insufficient stock and capacity constraints'
            : 'Request approved with truck and crew assignment',
          actor: 'Agent Smith',
          status: 'completed'
        },
        ...(requestId !== 'LSR-2024-003' ? [
          {
            date: '2024-01-15',
            time: '14:30',
            event: 'Load Sheet Generated',
            description: 'Load sheet generated and sent to depot',
            actor: 'System',
            status: 'completed' as const
          },
          {
            date: '2024-01-16',
            time: '08:00',
            event: 'Loading Started',
            description: 'Truck loading commenced at depot',
            actor: 'Depot Staff',
            status: requestId === 'LSR-2024-001' ? 'current' as const : 'completed' as const
          }
        ] : [])
      ],
      approvalMetrics: {
        totalRequestedQuantity: 105,
        totalApprovedQuantity: requestId === 'LSR-2024-003' ? 0 : 95,
        approvalRate: requestId === 'LSR-2024-003' ? 0 : 90.5,
        processingTime: '5 hours 30 minutes',
        stockUtilization: requestId === 'LSR-2024-003' ? 0 : 85
      }
    };

    setRequest(mockRequest);
    setLoading(false);
  };

  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case 'approved':
        return <Badge variant="default">Approved</Badge>;
      case 'rejected':
        return <Badge variant="destructive">Rejected</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const getPriorityBadge = (priority: string) => {
    switch (priority) {
      case 'High':
        return <Badge variant="destructive">High Priority</Badge>;
      case 'Medium':
        return <Badge variant="secondary">Medium Priority</Badge>;
      case 'Low':
        return <Badge variant="default">Low Priority</Badge>;
      default:
        return <Badge variant="secondary">{priority}</Badge>;
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Loading approval details...</p>
        </div>
      </div>
    );
  }

  if (!request) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <AlertCircle className="w-16 h-16 text-muted-foreground mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-foreground mb-2">Request Not Found</h2>
          <p className="text-muted-foreground mb-4">The requested approval record could not be found.</p>
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
                <h1 className="text-2xl font-bold">Approval Request Details</h1>
                <p className="text-sm text-muted-foreground mt-1">
                  Logistics Approval › Request Details › {request.movementCode}
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              {getStatusBadge(request.status)}
              {getPriorityBadge(request.priority)}
            </div>
          </div>
        </div>

        {/* Metrics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Approval Rate</p>
                  <p className="text-2xl font-bold text-foreground">{request.approvalMetrics.approvalRate}%</p>
                </div>
                <CheckCircle className="w-8 h-8 text-muted-foreground" />
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Processing Time</p>
                  <p className="text-2xl font-bold text-foreground">{request.approvalMetrics.processingTime}</p>
                </div>
                <Clock className="w-8 h-8 text-muted-foreground" />
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Stock Utilization</p>
                  <p className="text-2xl font-bold text-foreground">{request.approvalMetrics.stockUtilization}%</p>
                </div>
                <Package className="w-8 h-8 text-muted-foreground" />
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Completion</p>
                  <p className="text-2xl font-bold text-foreground">{request.completionPercentage}%</p>
                </div>
                <div className="w-8 h-8 bg-muted rounded-full flex items-center justify-center">
                  <div className="w-4 h-4 bg-primary rounded-full" />
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        <Tabs defaultValue="overview" className="space-y-6">
          <TabsList>
            <TabsTrigger value="overview">Overview</TabsTrigger>
            <TabsTrigger value="items">Items Details</TabsTrigger>
            <TabsTrigger value="customers">Customers</TabsTrigger>
            <TabsTrigger value="timeline">Timeline</TabsTrigger>
          </TabsList>

          <TabsContent value="overview" className="space-y-6">
            {/* Request Overview */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <FileText className="h-5 w-5" />
                  Request Overview
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Movement Code</span>
                    <p className="font-semibold">{request.movementCode}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Route</span>
                    <p className="font-semibold">{request.routeCode}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Salesman</span>
                    <p className="font-semibold">{request.salesmanId} {request.salesmanName}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Load Type</span>
                    <Badge variant={request.loadType === 'Emergency' ? 'secondary' : 'outline'}>
                      {request.loadType}
                    </Badge>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Submitted Date</span>
                    <p className="font-semibold">{new Date(request.submittedDate).toLocaleDateString()}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Required Date</span>
                    <p className="font-semibold">{new Date(request.requiredDate).toLocaleDateString()}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Processed Date</span>
                    <p className="font-semibold">{new Date(request.processedDate).toLocaleDateString()}</p>
                  </div>
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Processed By</span>
                    <p className="font-semibold">{request.approvedBy}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Assignment Details */}
            {request.status === 'Approved' && (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Truck className="h-5 w-5" />
                    Assignment & Resource Details
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Depot</span>
                      <p className="font-semibold">{request.depot}</p>
                    </div>
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Assigned Truck</span>
                      <p className="font-semibold">{request.assignedTruck}</p>
                    </div>
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Driver</span>
                      <p className="font-semibold">{request.assignedDriver}</p>
                    </div>
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Helper</span>
                      <p className="font-semibold">{request.assignedHelper}</p>
                    </div>
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Load Sheet ID</span>
                      <p className="font-semibold text-primary">{request.loadSheetId}</p>
                    </div>
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Requested Load</span>
                      <p className="font-semibold">{request.requestedLoad}</p>
                    </div>
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Approved Load</span>
                      <p className="font-semibold text-primary">{request.approvedLoad}</p>
                    </div>
                    {request.estimatedDelivery && (
                      <div>
                        <span className="text-sm font-medium text-muted-foreground">Est. Delivery</span>
                        <p className="font-semibold">{new Date(request.estimatedDelivery).toLocaleString()}</p>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Notes & Decisions */}
            <Card>
              <CardHeader>
                <CardTitle>Decision Notes</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 bg-muted rounded-lg">
                    <h4 className="font-medium mb-2">Logistics Officer Notes</h4>
                    <p className="text-muted-foreground">{request.notes}</p>
                  </div>
                  {request.rejectionReason && (
                    <div className="p-4 border border-destructive rounded-lg bg-destructive/5">
                      <h4 className="font-medium mb-2 text-destructive">Rejection Reason</h4>
                      <p className="text-muted-foreground">{request.rejectionReason}</p>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="items" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Item-wise Approval Details</CardTitle>
                <CardDescription>
                  Detailed breakdown of requested vs approved quantities with stock information
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Item Code</TableHead>
                      <TableHead>Description</TableHead>
                      <TableHead>Requested Qty</TableHead>
                      <TableHead>UOM</TableHead>
                      <TableHead>Approved Qty</TableHead>
                      <TableHead>Stock Available</TableHead>
                      <TableHead>Adjustment Reason</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {request.items.map((item, index) => (
                      <TableRow key={index}>
                        <TableCell className="font-medium">{item.itemCode}</TableCell>
                        <TableCell>{item.description}</TableCell>
                        <TableCell>{item.requestedQuantity}</TableCell>
                        <TableCell>
                          <Badge variant="outline">{item.uom}</Badge>
                        </TableCell>
                        <TableCell>
                          <span className={
                            item.approvedQuantity === 0 ? 'text-destructive font-medium' :
                            item.approvedQuantity !== item.requestedQuantity ? 'text-primary font-medium' : ''
                          }>
                            {item.approvedQuantity}
                          </span>
                        </TableCell>
                        <TableCell>
                          <span className={item.stockAvailable < item.requestedQuantity ? 'text-destructive' : 'text-foreground'}>
                            {item.stockAvailable}
                          </span>
                        </TableCell>
                        <TableCell>
                          {item.adjustmentReason && (
                            <Badge variant="secondary" className="text-xs">
                              {item.adjustmentReason}
                            </Badge>
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="customers" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Users className="h-5 w-5" />
                  Customer Details
                </CardTitle>
                <CardDescription>
                  Information about customers included in this load request
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {request.customers.map((customer, index) => (
                    <Card key={index} className="border-l-4 border-l-primary">
                      <CardContent className="p-6">
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                          <div>
                            <h4 className="font-semibold text-foreground">{customer.customer_name}</h4>
                            <p className="text-sm text-muted-foreground">{customer.outlet_name}</p>
                            <Badge variant="outline" className="mt-2">{customer.outlet_type}</Badge>
                          </div>
                          <div>
                            <h5 className="font-medium text-muted-foreground">Contact Information</h5>
                            <p className="text-sm">{customer.contact_person}</p>
                            <p className="text-sm text-muted-foreground">{customer.phone}</p>
                            <p className="text-sm text-muted-foreground">{customer.email}</p>
                          </div>
                          <div>
                            <h5 className="font-medium text-muted-foreground">Order Details</h5>
                            <p className="text-lg font-bold text-primary">{customer.quantity} units</p>
                            <p className="text-sm text-muted-foreground">Delivery: {new Date(customer.delivery_date).toLocaleDateString()}</p>
                            <Badge variant={
                              customer.priority === 'High' ? 'destructive' :
                              customer.priority === 'Medium' ? 'secondary' : 'default'
                            } className="mt-2">
                              {customer.priority} Priority
                            </Badge>
                          </div>
                        </div>
                        <div className="mt-4">
                          <h5 className="font-medium text-muted-foreground mb-2">Delivery Address</h5>
                          <p className="text-sm text-muted-foreground">{customer.address}</p>
                        </div>
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
                  Approval Timeline
                </CardTitle>
                <CardDescription>
                  Complete timeline of the approval process with detailed activity log
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {request.timeline.map((event, index) => (
                    <div key={index} className="flex items-start space-x-4">
                      <div className={`flex-shrink-0 w-4 h-4 rounded-full mt-1 ${
                        event.status === 'completed' ? 'bg-primary' :
                        event.status === 'current' ? 'bg-secondary border-2 border-primary' :
                        'bg-muted'
                      }`} />
                      <div className="flex-1">
                        <div className="flex items-center justify-between">
                          <div>
                            <h4 className="font-medium text-foreground">{event.event}</h4>
                            <p className="text-sm text-muted-foreground">{event.description}</p>
                          </div>
                          <div className="text-right text-xs text-muted-foreground">
                            <p>{new Date(event.date).toLocaleDateString()}</p>
                            <p>{event.time}</p>
                            <p className="font-medium">{event.actor}</p>
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