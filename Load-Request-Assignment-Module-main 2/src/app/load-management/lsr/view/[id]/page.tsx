'use client';

import { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, Package, Calendar, User, MapPin, Truck, Clock, CheckCircle, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';

interface LoadRequestDetail {
  id: string;
  movementCode: string;
  routeCode: string;
  salesmanId: string;
  salesmanName: string;
  loadType: 'Normal' | 'Emergency';
  submittedDate: string;
  requiredDate: string;
  status: 'Approved' | 'Rejected' | 'Pending' | 'In Transit' | 'Delivered';
  priority: 'High' | 'Medium' | 'Low';
  estimatedLoad: string;
  approvedLoad?: string;
  assignedTruck?: string;
  assignedDriver?: string;
  assignedHelper?: string;
  approvalDate?: string;
  approvedBy?: string;
  rejectionReason?: string;
  notes?: string;
  items: LoadItem[];
  timeline: TimelineEvent[];
}

interface LoadItem {
  itemCode: string;
  description: string;
  requestedQuantity: number;
  approvedQuantity?: number;
  warehouseStock: number;
  uom: string;
  weight: number;
  volume: number;
}

interface TimelineEvent {
  date: string;
  time: string;
  event: string;
  description: string;
  status: 'completed' | 'current' | 'pending';
}

export default function LSRDetailPage() {
  const router = useRouter();
  const params = useParams();
  const requestId = params?.id as string;
  
  const [request, setRequest] = useState<LoadRequestDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (requestId) {
      loadRequestDetails();
    }
  }, [requestId]);

  const loadRequestDetails = () => {
    // Mock data - in real app this would come from API
    const mockRequest: LoadRequestDetail = {
      id: requestId,
      movementCode: `SKTT01E000${requestId.padStart(4, '0')}`,
      routeCode: '[SKTT01]SKTT01 - North Zone Route',
      salesmanId: '[LSR001]',
      salesmanName: 'John Kumar',
      loadType: requestId === 'LSR-2024-002' ? 'Emergency' : 'Normal',
      submittedDate: '2024-01-15',
      requiredDate: '2024-01-16',
      status: requestId === 'LSR-2024-001' ? 'Approved' : requestId === 'LSR-2024-002' ? 'In Transit' : requestId === 'LSR-2024-003' ? 'Delivered' : 'Pending',
      priority: requestId === 'LSR-2024-001' ? 'High' : 'Medium',
      estimatedLoad: '850 kg',
      approvedLoad: requestId !== 'LSR-2024-003' ? '800 kg' : undefined,
      assignedTruck: requestId !== 'LSR-2024-003' ? 'TRK-001' : undefined,
      assignedDriver: requestId !== 'LSR-2024-003' ? 'John Smith' : undefined,
      assignedHelper: requestId !== 'LSR-2024-003' ? 'Mike Davis' : undefined,
      approvalDate: requestId !== 'LSR-2024-003' ? '2024-01-15' : undefined,
      approvedBy: requestId !== 'LSR-2024-003' ? 'Agent Smith' : undefined,
      rejectionReason: requestId === 'LSR-2024-003' ? 'Insufficient stock availability' : undefined,
      notes: requestId === 'LSR-2024-001' ? 'Approved with minor quantity adjustments for optimal route loading' : undefined,
      items: [
        {
          itemCode: 'SKU001',
          description: 'Choco Bar 90ml',
          requestedQuantity: 50,
          approvedQuantity: requestId !== 'LSR-2024-003' ? 45 : undefined,
          warehouseStock: 120,
          uom: 'Carton',
          weight: 250,
          volume: 500
        },
        {
          itemCode: 'SKU002', 
          description: 'Vanilla Tub 500ml',
          requestedQuantity: 30,
          approvedQuantity: requestId !== 'LSR-2024-003' ? 30 : undefined,
          warehouseStock: 85,
          uom: 'Box',
          weight: 180,
          volume: 400
        },
        {
          itemCode: 'SKU003',
          description: 'Strawberry Cup 250ml', 
          requestedQuantity: 25,
          approvedQuantity: requestId !== 'LSR-2024-003' ? 20 : undefined,
          warehouseStock: 45,
          uom: 'Case',
          weight: 125,
          volume: 300
        }
      ],
      timeline: [
        {
          date: '2024-01-15',
          time: '09:00',
          event: 'Request Submitted',
          description: 'Load request submitted by LSR',
          status: 'completed'
        },
        {
          date: '2024-01-15', 
          time: '10:30',
          event: 'Under Review',
          description: 'Request being reviewed by logistics team',
          status: requestId === 'LSR-2024-003' ? 'completed' : 'completed'
        },
        {
          date: '2024-01-15',
          time: '14:00', 
          event: requestId === 'LSR-2024-003' ? 'Request Rejected' : 'Request Approved',
          description: requestId === 'LSR-2024-003' ? 'Request rejected due to insufficient stock' : 'Request approved and truck assigned',
          status: 'completed'
        },
        ...(requestId !== 'LSR-2024-003' ? [
          {
            date: '2024-01-16',
            time: '08:00',
            event: 'Loading Started',
            description: 'Truck loading commenced at depot',
            status: requestId === 'LSR-2024-001' ? 'current' as const : 'completed' as const
          },
          {
            date: '2024-01-16',
            time: '10:00',
            event: 'In Transit',
            description: 'Truck departed for delivery route',
            status: requestId === 'LSR-2024-002' ? 'current' as const : requestId === 'LSR-2024-001' ? 'pending' as const : 'completed' as const
          }
        ] : [])
      ]
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
      case 'pending':
        return <Badge variant="secondary">Pending</Badge>;
      case 'in transit':
        return <Badge variant="outline">In Transit</Badge>;
      case 'delivered':
        return <Badge variant="default">Delivered</Badge>;
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
          <p className="mt-4 text-muted-foreground">Loading request details...</p>
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
          <p className="text-muted-foreground mb-4">The requested load request could not be found.</p>
          <Button onClick={() => router.push('/load-management/lsr')}>
            Back to LSR
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
                onClick={() => router.push('/load-management/lsr')}
                className="flex items-center gap-2"
              >
                <ArrowLeft className="w-4 h-4" />
                Back to LSR
              </Button>
              <div>
                <h1 className="text-2xl font-bold">Load Request Details</h1>
                <p className="text-sm text-muted-foreground mt-1">
                  LSR › Request Details › {request.movementCode}
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              {getStatusBadge(request.status)}
              {getPriorityBadge(request.priority)}
            </div>
          </div>
        </div>

        <div className="space-y-6">
          {/* Request Overview */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Package className="h-5 w-5" />
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
                  <span className="text-sm font-medium text-muted-foreground">Estimated Load</span>
                  <p className="font-semibold">{request.estimatedLoad}</p>
                </div>
                {request.approvedLoad && (
                  <div>
                    <span className="text-sm font-medium text-muted-foreground">Approved Load</span>
                    <p className="font-semibold text-primary">{request.approvedLoad}</p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Assignment Details */}
          {request.status === 'Approved' || request.status === 'In Transit' || request.status === 'Delivered' ? (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Truck className="h-5 w-5" />
                  Assignment Details
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
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
                    <span className="text-sm font-medium text-muted-foreground">Approval Date</span>
                    <p className="font-semibold">{request.approvalDate ? new Date(request.approvalDate).toLocaleDateString() : 'N/A'}</p>
                  </div>
                  {request.approvedBy && (
                    <div>
                      <span className="text-sm font-medium text-muted-foreground">Approved By</span>
                      <p className="font-semibold">{request.approvedBy}</p>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          ) : null}

          {/* Rejection Details */}
          {request.status === 'Rejected' && request.rejectionReason && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-destructive">
                  <AlertCircle className="h-5 w-5" />
                  Rejection Details
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="p-4 bg-muted rounded-lg">
                  <p className="text-muted-foreground">{request.rejectionReason}</p>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Load Items */}
          <Card>
            <CardHeader>
              <CardTitle>Load Items</CardTitle>
              <CardDescription>
                Items requested for this load service request
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Item Code</TableHead>
                    <TableHead>Description</TableHead>
                    <TableHead>Warehouse Stock</TableHead>
                    <TableHead>Requested Qty</TableHead>
                    <TableHead>UOM</TableHead>
                    {request.status !== 'Rejected' && <TableHead>Approved Qty</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {request.items.map((item, index) => (
                    <TableRow key={index}>
                      <TableCell className="font-medium">{item.itemCode}</TableCell>
                      <TableCell>{item.description}</TableCell>
                      <TableCell>
                        <span className={item.warehouseStock < item.requestedQuantity ? 'text-destructive font-medium' : 'text-foreground'}>
                          {item.warehouseStock}
                        </span>
                      </TableCell>
                      <TableCell>{item.requestedQuantity}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{item.uom}</Badge>
                      </TableCell>
                      {request.status !== 'Rejected' && (
                        <TableCell>
                          <span className={item.approvedQuantity !== item.requestedQuantity ? 'text-primary font-medium' : ''}>
                            {item.approvedQuantity || 'Pending'}
                          </span>
                        </TableCell>
                      )}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>

          {/* Timeline */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Request Timeline
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {request.timeline.map((event, index) => (
                  <div key={index} className="flex items-start space-x-4">
                    <div className={`flex-shrink-0 w-4 h-4 rounded-full mt-1 ${
                      event.status === 'completed' ? 'bg-primary' :
                      event.status === 'current' ? 'bg-secondary border-2 border-primary' :
                      'bg-muted'
                    }`} />
                    <div className="flex-1">
                      <div className="flex items-center space-x-2">
                        <h4 className="font-medium text-foreground">{event.event}</h4>
                        <span className="text-sm text-muted-foreground">
                          {new Date(event.date).toLocaleDateString()} at {event.time}
                        </span>
                      </div>
                      <p className="text-sm text-muted-foreground">{event.description}</p>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Notes */}
          {request.notes && (
            <Card>
              <CardHeader>
                <CardTitle>Notes</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="p-4 bg-muted rounded-lg">
                  <p className="text-muted-foreground">{request.notes}</p>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}