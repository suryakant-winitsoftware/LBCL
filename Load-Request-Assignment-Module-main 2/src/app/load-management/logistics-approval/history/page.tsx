'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { ArrowLeft, Eye, Calendar, Package, User, Search, Filter, CheckCircle, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

interface ApprovalRequest {
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
  totalItems: number;
  totalQuantity: number;
  approvedQuantity: number;
  action: 'Approved' | 'Rejected';
  notes?: string;
}

export default function LogisticsApprovalHistoryPage() {
  const router = useRouter();
  const [requests, setRequests] = useState<ApprovalRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [loadTypeFilter, setLoadTypeFilter] = useState<string>('all');

  useEffect(() => {
    loadApprovalHistory();
  }, []);

  const loadApprovalHistory = () => {
    // Mock data - in real app this would come from API
    const mockRequests: ApprovalRequest[] = [
      {
        id: '1',
        movementCode: 'SKTT01E0001',
        routeCode: '[SKTT01]SKTT01',
        salesmanId: '[LSR001]',
        salesmanName: 'John Kumar',
        loadType: 'Normal',
        submittedDate: '2024-01-15',
        requiredDate: '2024-01-16',
        processedDate: '2024-01-15',
        status: 'Approved',
        totalItems: 5,
        totalQuantity: 150,
        approvedQuantity: 145,
        action: 'Approved',
        notes: 'Approved with minor quantity adjustments for stock availability'
      },
      {
        id: '2',
        movementCode: 'SKTT01E0002',
        routeCode: '[SKTT01]SKTT01',
        salesmanId: '[LSR001]',
        salesmanName: 'John Kumar',
        loadType: 'Emergency',
        submittedDate: '2024-01-14',
        requiredDate: '2024-01-14',
        processedDate: '2024-01-14',
        status: 'Approved',
        totalItems: 3,
        totalQuantity: 85,
        approvedQuantity: 85,
        action: 'Approved',
        notes: 'Emergency request approved as per priority'
      },
      {
        id: '3',
        movementCode: 'SKTT01E0003',
        routeCode: '[SKTT01]SKTT01',
        salesmanId: '[LSR001]',
        salesmanName: 'John Kumar',
        loadType: 'Normal',
        submittedDate: '2024-01-13',
        requiredDate: '2024-01-14',
        processedDate: '2024-01-13',
        status: 'Rejected',
        totalItems: 4,
        totalQuantity: 120,
        approvedQuantity: 0,
        action: 'Rejected',
        notes: 'Insufficient stock availability. Request exceeds current inventory levels'
      },
      {
        id: '4',
        movementCode: 'SKTT02E0001',
        routeCode: '[SKTT02]SKTT02',
        salesmanId: '[LSR002]',
        salesmanName: 'Sarah Patel',
        loadType: 'Normal',
        submittedDate: '2024-01-12',
        requiredDate: '2024-01-13',
        processedDate: '2024-01-12',
        status: 'Approved',
        totalItems: 6,
        totalQuantity: 200,
        approvedQuantity: 180,
        action: 'Approved',
        notes: 'Approved with quantity adjustment for route optimization'
      },
      {
        id: '5',
        movementCode: 'SKTT03E0001',
        routeCode: '[SKTT03]SKTT03',
        salesmanId: '[LSR003]',
        salesmanName: 'Mike Johnson',
        loadType: 'Emergency',
        submittedDate: '2024-01-11',
        requiredDate: '2024-01-11',
        processedDate: '2024-01-11',
        status: 'Approved',
        totalItems: 2,
        totalQuantity: 50,
        approvedQuantity: 50,
        action: 'Approved'
      },
      {
        id: '6',
        movementCode: 'SKTT04E0001',
        routeCode: '[SKTT04]SKTT04',
        salesmanId: '[LSR004]',
        salesmanName: 'Lisa Wong',
        loadType: 'Normal',
        submittedDate: '2024-01-10',
        requiredDate: '2024-01-11',
        processedDate: '2024-01-10',
        status: 'Rejected',
        totalItems: 7,
        totalQuantity: 280,
        approvedQuantity: 0,
        action: 'Rejected',
        notes: 'Route capacity exceeded. Truck availability constraints'
      }
    ];

    setRequests(mockRequests);
    setLoading(false);
  };

  const filteredRequests = requests.filter(request => {
    const matchesSearch = request.movementCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         request.salesmanName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         request.salesmanId.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || request.status.toLowerCase() === statusFilter.toLowerCase();
    const matchesLoadType = loadTypeFilter === 'all' || request.loadType.toLowerCase() === loadTypeFilter.toLowerCase();
    
    return matchesSearch && matchesStatus && matchesLoadType;
  });

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

  const getLoadTypeBadge = (type: string) => {
    return type === 'Emergency' 
      ? <Badge variant="secondary">Emergency</Badge>
      : <Badge variant="outline">Normal</Badge>;
  };

  const getActionIcon = (action: string) => {
    return action === 'Approved' 
      ? <CheckCircle className="w-4 h-4 text-muted-foreground" />
      : <XCircle className="w-4 h-4 text-muted-foreground" />;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Loading approval history...</p>
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
                Back
              </Button>
              <div>
                <h1 className="text-2xl font-bold">Logistics Approval History</h1>
                <p className="text-sm text-muted-foreground mt-1">
                  Load Management › Logistics Approval › History
                </p>
              </div>
            </div>
          </div>
        </div>


        {/* Approval History Table */}
        <div className="bg-card rounded-lg border overflow-hidden">
          <div className="px-6 py-4 border-b">
            <h3 className="text-lg font-semibold">Approval History</h3>
            <p className="text-sm text-muted-foreground">Complete history of processed load requests</p>
          </div>
          <div className="w-full">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="px-4 py-3 min-w-[140px]">Movement Code</TableHead>
                  <TableHead className="px-4 py-3 min-w-[120px]">Salesman</TableHead>
                  <TableHead className="px-4 py-3 w-[80px] hidden sm:table-cell">Type</TableHead>
                  <TableHead className="px-4 py-3 w-[100px] hidden md:table-cell">Submitted</TableHead>
                  <TableHead className="px-4 py-3 w-[100px] hidden lg:table-cell">Processed</TableHead>
                  <TableHead className="px-4 py-3 w-[60px] text-center hidden lg:table-cell">Items</TableHead>
                  <TableHead className="px-4 py-3 w-[80px] text-center hidden md:table-cell">Requested</TableHead>
                  <TableHead className="px-4 py-3 w-[80px] text-center">Approved</TableHead>
                  <TableHead className="px-4 py-3 w-[100px]">Status</TableHead>
                  <TableHead className="px-4 py-3 w-[80px]">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredRequests.map((request) => (
                  <TableRow key={request.id}>
                    <TableCell className="px-4 py-3">
                      <div>
                        <div className="font-medium text-sm">{request.movementCode}</div>
                        <div className="text-xs text-muted-foreground">{request.routeCode}</div>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div>
                        <div className="font-medium text-sm">{request.salesmanName}</div>
                        <div className="text-xs text-muted-foreground">{request.salesmanId}</div>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3 hidden sm:table-cell">
                      {getLoadTypeBadge(request.loadType)}
                    </TableCell>
                    <TableCell className="px-4 py-3 hidden md:table-cell">
                      <div className="flex items-center space-x-1">
                        <Calendar className="w-3 h-3 text-muted-foreground" />
                        <span className="text-xs">{new Date(request.submittedDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</span>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3 hidden lg:table-cell">
                      <div className="flex items-center space-x-1">
                        <Calendar className="w-3 h-3 text-muted-foreground" />
                        <span className="text-xs">{new Date(request.processedDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</span>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3 text-center hidden lg:table-cell">
                      <span className="font-medium text-sm">{request.totalItems}</span>
                    </TableCell>
                    <TableCell className="px-4 py-3 text-center hidden md:table-cell">
                      <span className="font-medium text-sm">{request.totalQuantity}</span>
                    </TableCell>
                    <TableCell className="px-4 py-3 text-center">
                      <div className="flex items-center justify-center space-x-1">
                        {getActionIcon(request.action)}
                        <span className="font-medium text-sm">
                          {request.approvedQuantity}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div className="flex flex-col space-y-1">
                        {getStatusBadge(request.status)}
                        {request.notes && (
                          <div className="text-xs text-muted-foreground max-w-[120px] truncate" title={request.notes}>
                            {request.notes}
                          </div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <Button 
                        variant="outline" 
                        size="sm"
                        onClick={() => router.push(`/load-management/logistics-approval/view/${request.id}`)}
                        className="flex items-center gap-1 text-xs h-7"
                      >
                        <Eye className="w-3 h-3" />
                        <span className="hidden sm:inline">View</span>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </div>

        {filteredRequests.length === 0 && (
          <div className="text-center py-12">
            <Package className="w-16 h-16 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-medium mb-2">No approvals found</h3>
            <p className="text-muted-foreground">
              {searchTerm || statusFilter !== 'all' || loadTypeFilter !== 'all'
                ? 'No approvals match your current filters.'
                : 'You haven\'t processed any load requests yet.'}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}