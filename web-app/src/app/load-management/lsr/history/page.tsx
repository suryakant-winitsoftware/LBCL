'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { ArrowLeft, Eye, Calendar, Package, User, Search, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
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

interface LoadRequest {
  id: string;
  movementCode: string;
  routeCode: string;
  salesmanName: string;
  loadType: 'Normal' | 'Emergency';
  submittedDate: string;
  requiredDate: string;
  status: 'Approved' | 'Rejected' | 'Pending';
  totalItems: number;
  totalQuantity: number;
  approvedBy?: string;
  approvedDate?: string;
  rejectionReason?: string;
}

export default function LSRHistoryPage() {
  const router = useRouter();
  const [requests, setRequests] = useState<LoadRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [loadTypeFilter, setLoadTypeFilter] = useState<string>('all');

  useEffect(() => {
    loadRequestHistory();
  }, []);

  const loadRequestHistory = () => {
    // Mock data - in real app this would come from API
    const mockRequests: LoadRequest[] = [
      {
        id: '1',
        movementCode: 'SKTT01E0001',
        routeCode: '[SKTT01]SKTT01',
        salesmanName: 'John Kumar',
        loadType: 'Normal',
        submittedDate: '2024-01-15',
        requiredDate: '2024-01-16',
        status: 'Approved',
        totalItems: 5,
        totalQuantity: 150,
        approvedBy: 'Agent Smith',
        approvedDate: '2024-01-15'
      },
      {
        id: '2',
        movementCode: 'SKTT01E0002',
        routeCode: '[SKTT01]SKTT01',
        salesmanName: 'John Kumar',
        loadType: 'Emergency',
        submittedDate: '2024-01-14',
        requiredDate: '2024-01-14',
        status: 'Approved',
        totalItems: 3,
        totalQuantity: 85,
        approvedBy: 'Agent Johnson',
        approvedDate: '2024-01-14'
      },
      {
        id: '3',
        movementCode: 'SKTT01E0003',
        routeCode: '[SKTT01]SKTT01',
        salesmanName: 'John Kumar',
        loadType: 'Normal',
        submittedDate: '2024-01-13',
        requiredDate: '2024-01-14',
        status: 'Rejected',
        totalItems: 4,
        totalQuantity: 120,
        rejectionReason: 'Insufficient stock availability'
      },
      {
        id: '4',
        movementCode: 'SKTT01E0004',
        routeCode: '[SKTT01]SKTT01',
        salesmanName: 'John Kumar',
        loadType: 'Normal',
        submittedDate: '2024-01-12',
        requiredDate: '2024-01-13',
        status: 'Approved',
        totalItems: 6,
        totalQuantity: 200,
        approvedBy: 'Agent Williams',
        approvedDate: '2024-01-12'
      },
      {
        id: '5',
        movementCode: 'SKTT01E0005',
        routeCode: '[SKTT01]SKTT01',
        salesmanName: 'John Kumar',
        loadType: 'Emergency',
        submittedDate: '2024-01-11',
        requiredDate: '2024-01-11',
        status: 'Approved',
        totalItems: 2,
        totalQuantity: 50,
        approvedBy: 'Agent Brown',
        approvedDate: '2024-01-11'
      }
    ];

    setRequests(mockRequests);
    setLoading(false);
  };

  const filteredRequests = requests.filter(request => {
    const matchesSearch = request.movementCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         request.salesmanName.toLowerCase().includes(searchTerm.toLowerCase());
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
      case 'pending':
        return <Badge variant="secondary">Pending</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const getLoadTypeBadge = (type: string) => {
    return type === 'Emergency' 
      ? <Badge variant="secondary">Emergency</Badge>
      : <Badge variant="outline">Normal</Badge>;
  };

  if (loading) {
    return (
      <div className="p-8">
        <div className="max-w-7xl mx-auto">
          <div className="mb-8">
            <Skeleton className="h-10 w-48 mb-4" />
            <Skeleton className="h-5 w-96" />
          </div>
          <div className="bg-card rounded-lg border overflow-hidden">
            <div className="px-6 py-4 border-b">
              <Skeleton className="h-6 w-40 mb-2" />
              <Skeleton className="h-4 w-64" />
            </div>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    {[1, 2, 3, 4, 5, 6, 7, 8, 9].map((i) => (
                      <TableHead key={i} className="px-4 py-3">
                        <Skeleton className="h-4 w-20" />
                      </TableHead>
                    ))}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {[1, 2, 3, 4, 5].map((i) => (
                    <TableRow key={i}>
                      <TableCell className="px-4 py-3"><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell className="px-4 py-3"><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell className="px-4 py-3"><Skeleton className="h-6 w-20 rounded-full" /></TableCell>
                      <TableCell className="px-4 py-3"><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell className="px-4 py-3"><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell className="px-4 py-3 text-center"><Skeleton className="h-4 w-8 mx-auto" /></TableCell>
                      <TableCell className="px-4 py-3 text-center"><Skeleton className="h-4 w-12 mx-auto" /></TableCell>
                      <TableCell className="px-4 py-3"><Skeleton className="h-6 w-20 rounded-full" /></TableCell>
                      <TableCell className="px-4 py-3"><Skeleton className="h-8 w-20" /></TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </div>
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
                Back
              </Button>
              <div>
                <h1 className="text-2xl font-bold">LSR Request History</h1>
                <p className="text-sm text-muted-foreground mt-1">
                  Load Management › LSR › Request History
                </p>
              </div>
            </div>
          </div>
        </div>


        {/* Requests Table */}
        <div className="bg-card rounded-lg border overflow-hidden">
          <div className="px-6 py-4 border-b">
            <h3 className="text-lg font-semibold">Request History</h3>
            <p className="text-sm text-muted-foreground">Complete history of your load requests</p>
          </div>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="px-4 py-3">Movement Code</TableHead>
                  <TableHead className="px-4 py-3">Route</TableHead>
                  <TableHead className="px-4 py-3">Load Type</TableHead>
                  <TableHead className="px-4 py-3">Submitted</TableHead>
                  <TableHead className="px-4 py-3">Required</TableHead>
                  <TableHead className="px-4 py-3 text-center">Items</TableHead>
                  <TableHead className="px-4 py-3 text-center">Quantity</TableHead>
                  <TableHead className="px-4 py-3">Status</TableHead>
                  <TableHead className="px-4 py-3">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredRequests.map((request) => (
                  <TableRow key={request.id}>
                    <TableCell className="px-4 py-3">
                      <div>
                        <div className="font-medium">{request.movementCode}</div>
                        <div className="text-sm text-muted-foreground">{request.salesmanName}</div>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <span className="font-medium">{request.routeCode}</span>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      {getLoadTypeBadge(request.loadType)}
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div className="flex items-center space-x-1">
                        <Calendar className="w-4 h-4 text-muted-foreground" />
                        <span>{new Date(request.submittedDate).toLocaleDateString()}</span>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div className="flex items-center space-x-1">
                        <Calendar className="w-4 h-4 text-muted-foreground" />
                        <span>{new Date(request.requiredDate).toLocaleDateString()}</span>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3 text-center">
                      <span className="font-medium">{request.totalItems}</span>
                    </TableCell>
                    <TableCell className="px-4 py-3 text-center">
                      <span className="font-medium">{request.totalQuantity}</span>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div className="space-y-1">
                        {getStatusBadge(request.status)}
                        {request.status === 'Approved' && request.approvedBy && (
                          <div className="text-xs text-muted-foreground">
                            By: {request.approvedBy}
                          </div>
                        )}
                        {request.status === 'Rejected' && request.rejectionReason && (
                          <div className="text-xs text-muted-foreground">
                            {request.rejectionReason}
                          </div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <Button 
                        variant="outline" 
                        size="sm"
                        onClick={() => router.push(`/load-management/lsr/view/${request.id}`)}
                        className="flex items-center gap-2"
                      >
                        <Eye className="w-4 h-4" />
                        View
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
            <h3 className="text-lg font-medium mb-2">No requests found</h3>
            <p className="text-muted-foreground">
              {searchTerm || statusFilter !== 'all' || loadTypeFilter !== 'all'
                ? 'No requests match your current filters.'
                : 'You haven\'t submitted any load requests yet.'}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}