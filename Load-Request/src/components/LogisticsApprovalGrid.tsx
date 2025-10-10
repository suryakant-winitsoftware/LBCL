'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Filter, RefreshCw, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import CreateLoadRequestModal from '@/components/CreateLoadRequestModal';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { 
  logisticsApprovalService, 
  LoadRequest, 
  LoadRequestFilter 
} from '@/services/logistics-approval.service';
import { PagingRequest } from '@/types/common.types';
import { format } from 'date-fns';
import { toast } from 'sonner';

interface LogisticsApprovalGridProps {
  onApproveOrder?: () => void;
}

export default function LogisticsApprovalGrid({ onApproveOrder }: LogisticsApprovalGridProps) {
  const router = useRouter();
  const [loadRequests, setLoadRequests] = useState<LoadRequest[]>([]);
  const [activeTab, setActiveTab] = useState<'pending' | 'logisticsApproved' | 'ForkliftApproved' | 'rejected' | 'shipped'>('pending');
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [tabCounts, setTabCounts] = useState({
    pending: 0,
    logisticsApproved: 0,
    ForkliftApproved: 0,
    rejected: 0,
    shipped: 0
  });
  const [pagingRequest, setPagingRequest] = useState<PagingRequest>({
    pageNumber: 1,
    pageSize: 50,
    sortColumn: 'submittedDate',
    sortDirection: 'DESC'
  });

  // Auto-refresh every 30 seconds
  useEffect(() => {
    const interval = setInterval(() => {
      fetchLoadRequests(true);
    }, 30000);

    return () => clearInterval(interval);
  }, [activeTab, searchTerm]);

  useEffect(() => {
    fetchLoadRequests();
  }, [activeTab, searchTerm, pagingRequest]);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    try {
      const stats = await logisticsApprovalService.getLoadRequestStats();
      setTabCounts({
        pending: stats.pending || 0,
        logisticsApproved: stats.logisticsApproved || 0,
        ForkliftApproved: stats.forkliftApproved || 0,
        rejected: stats.rejected || 0,
        shipped: stats.shipped || 0
      });
    } catch (error) {
      console.error('Error fetching stats:', error);
    }
  };

  const fetchLoadRequests = useCallback(async (isRefresh = false) => {
    if (isRefresh) {
      setRefreshing(true);
    } else {
      setLoading(true);
    }

    try {
      // Build filter based on active tab and search
      const filter: LoadRequestFilter = {
        searchTerm: searchTerm || undefined
      };

      // Map tab to status filter
      if (activeTab === 'pending') {
        filter.status = ['Pending'];
      } else if (activeTab === 'logisticsApproved') {
        filter.status = ['LogisticsApproved'];
      } else if (activeTab === 'ForkliftApproved') {
        filter.status = ['ForkliftApproved'];
      } else if (activeTab === 'rejected') {
        filter.status = ['Rejected'];
      } else if (activeTab === 'shipped') {
        filter.status = ['Shipped'];
      }

      const response = await logisticsApprovalService.getLoadRequests(pagingRequest, filter);
      
      if (response.isSuccess) {
        setLoadRequests(response.data);
        
        // Update stats if refresh
        if (isRefresh) {
          await fetchStats();
          toast.success('Data refreshed successfully');
        }
      } else {
        toast.error(response.message || 'Failed to fetch load requests');
      }
    } catch (error) {
      console.error('Error fetching load requests:', error);
      toast.error('Error loading data. Using cached data.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [activeTab, searchTerm, pagingRequest]);

  const handleViewDetails = (request: LoadRequest) => {
    router.push(`/load-management/logistics-approval/${request.uid || request.id}`);
  };

  const handleManualRefresh = () => {
    fetchLoadRequests(true);
  };

  const handleCreateSuccess = () => {
    setShowCreateModal(false);
    fetchLoadRequests(true);
  };

  const formatDate = (dateString: string) => {
    try {
      if (!dateString) return '-';
      const date = new Date(dateString);
      return format(date, 'dd/MM/yyyy');
    } catch {
      return dateString;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg border shadow-xs p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">MANUAL LOAD REQUESTS</h1>
            <div className="flex items-center space-x-2 mt-1">
              <span className="text-sm text-muted-foreground">Dashboard</span>
              <span className="text-muted-foreground">›</span>
              <span className="text-sm font-medium text-gray-700">Manual Load Requests</span>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <Button 
              onClick={() => setShowCreateModal(true)}
              className="flex items-center space-x-2"
            >
              <Plus className="w-4 h-4" />
              <span>New Request</span>
            </Button>
            <Button 
              variant="outline" 
              onClick={handleManualRefresh}
              disabled={refreshing}
              className="flex items-center space-x-2"
            >
              <RefreshCw className={`w-4 h-4 ${refreshing ? 'animate-spin' : ''}`} />
              <span>Refresh</span>
            </Button>
            <Button variant="outline" className="flex items-center space-x-2">
              <Filter className="w-4 h-4" />
              <span>Advanced Filter</span>
            </Button>
          </div>
        </div>


        {/* Tabs */}
        <div className="flex space-x-2 p-1 bg-gray-100 rounded-lg">
          <button
            onClick={() => setActiveTab('pending')}
            className={`px-4 py-2 font-medium rounded-md transition-all flex items-center space-x-2 ${
              activeTab === 'pending' 
                ? 'bg-white text-gray-900 shadow-xs' 
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            <span>Pending</span>
            <Badge variant="secondary" className="text-xs">
              {tabCounts.pending}
            </Badge>
          </button>
          <button
            onClick={() => setActiveTab('logisticsApproved')}
            className={`px-4 py-2 font-medium rounded-md transition-all flex items-center space-x-2 ${
              activeTab === 'logisticsApproved' 
                ? 'bg-white text-gray-900 shadow-xs' 
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            <span>Logistics</span>
            <Badge variant="secondary" className="text-xs">
              {tabCounts.logisticsApproved}
            </Badge>
          </button>
          <button
            onClick={() => setActiveTab('ForkliftApproved')}
            className={`px-4 py-2 font-medium rounded-md transition-all flex items-center space-x-2 ${
              activeTab === 'ForkliftApproved' 
                ? 'bg-white text-gray-900 shadow-xs' 
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            <span>Forklift</span>
            <Badge variant="secondary" className="text-xs">
              {tabCounts.ForkliftApproved}
            </Badge>
          </button>
          <button
            onClick={() => setActiveTab('rejected')}
            className={`px-4 py-2 font-medium rounded-md transition-all flex items-center space-x-2 ${
              activeTab === 'rejected' 
                ? 'bg-white text-gray-900 shadow-xs' 
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            <span>Rejected</span>
            <Badge variant="secondary" className="text-xs">
              {tabCounts.rejected}
            </Badge>
          </button>
          <button
            onClick={() => setActiveTab('shipped')}
            className={`px-4 py-2 font-medium rounded-md transition-all flex items-center space-x-2 ${
              activeTab === 'shipped' 
                ? 'bg-white text-gray-900 shadow-xs' 
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            <span>Shipped</span>
            <Badge variant="secondary" className="text-xs">
              {tabCounts.shipped}
            </Badge>
          </button>
        </div>
      </div>

      {/* Main Table */}
      <div className="bg-white rounded-lg border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className={`h-2 w-2 rounded-full ${loading ? 'bg-yellow-500 animate-pulse' : 'bg-green-500'}`}></div>
              <p className="text-sm font-medium text-muted-foreground">
                {loading ? 'Loading...' : (
                  <>Showing <span className="font-semibold text-foreground">{loadRequests.length}</span> results</>
                )}
              </p>
            </div>
            <div className="flex items-center space-x-3">
              <div className="relative">
                <input
                  type="text"
                  placeholder="Search records..."
                  className="pl-10 pr-4 py-2 w-64 border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-ring focus:border-transparent bg-background"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
                <svg className="absolute left-3 top-2.5 w-4 h-4 text-muted-foreground" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        <div className="w-full">
          <Table>
            <TableHeader>
              <TableRow className="bg-muted/30">
                <TableHead className="w-12 px-4 py-3">
                  <input type="checkbox" className="rounded border-input text-primary focus:ring-primary w-4 h-4" />
                </TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider min-w-[120px]">Movement</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider hidden sm:table-cell">Route</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider min-w-[100px]">Salesman</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider w-16 hidden md:table-cell">Type</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider w-20">Status</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider w-24 hidden lg:table-cell">Submitted</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider w-24 hidden lg:table-cell">Needs By</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider text-center w-20">Action</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={9} className="text-center py-8">
                    <div className="flex items-center justify-center space-x-2">
                      <RefreshCw className="w-5 h-5 animate-spin text-muted-foreground" />
                      <span className="text-muted-foreground">Loading requests...</span>
                    </div>
                  </TableCell>
                </TableRow>
              ) : loadRequests.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} className="text-center py-8 text-muted-foreground">
                    No load requests found
                  </TableCell>
                </TableRow>
              ) : (
                loadRequests.map((request, index) => (
                  <TableRow key={request.uid || request.id} className={`hover:bg-muted/20 transition-colors ${index % 2 === 0 ? 'bg-background' : 'bg-muted/10'}`}>
                    <TableCell className="px-4 py-3">
                      <input type="checkbox" className="rounded border-input text-primary focus:ring-primary w-4 h-4" />
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div className="font-medium text-sm text-foreground">
                        {request.movementCode}
                      </div>
                      <div className="text-xs text-muted-foreground sm:hidden">
                        {request.routeCode}
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3 hidden sm:table-cell">
                      <div className="text-sm text-muted-foreground">{request.routeCode}</div>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div className="space-y-1">
                        <div className="font-medium text-sm text-foreground">{request.salesmanId}</div>
                        <div className="text-xs text-muted-foreground">{request.salesmanName}</div>
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3 hidden md:table-cell">
                      <Badge 
                        variant={request.loadType === 'Emergency' ? 'destructive' : 'secondary'}
                        className="text-xs"
                      >
                        {request.loadType}
                      </Badge>
                    </TableCell>
                    <TableCell className="px-4 py-3">
                      <div className="flex items-center space-x-2">
                        <div className={
                          request.status === 'Pending' ? 'h-2 w-2 bg-yellow-400 rounded-full flex-shrink-0' :
                          request.status === 'LogisticsApproved' ? 'h-2 w-2 bg-blue-500 rounded-full flex-shrink-0' :
                          request.status === 'ForkliftApproved' ? 'h-2 w-2 bg-green-500 rounded-full flex-shrink-0' :
                          request.status === 'Rejected' ? 'h-2 w-2 bg-red-500 rounded-full flex-shrink-0' :
                          'h-2 w-2 bg-gray-400 rounded-full flex-shrink-0'
                        }></div>
                        <Badge variant="outline" className="text-xs">
                          {request.status === 'LogisticsApproved' ? 'Logistics ✓' :
                           request.status === 'ForkliftApproved' ? 'Forklift ✓' :
                           request.status}
                        </Badge>
                      </div>
                      <div className="text-xs text-muted-foreground md:hidden mt-1">
                        {request.loadType}
                      </div>
                    </TableCell>
                    <TableCell className="px-4 py-3 hidden lg:table-cell">
                      <div className="text-sm text-muted-foreground">{formatDate(request.submittedDate)}</div>
                    </TableCell>
                    <TableCell className="px-4 py-3 hidden lg:table-cell">
                      <div className="text-sm text-muted-foreground">{formatDate(request.requiredDate)}</div>
                    </TableCell>
                    <TableCell className="text-center px-4 py-3">
                      <Button
                        size="sm"
                        onClick={() => handleViewDetails(request)}
                        variant="outline"
                        className="text-xs px-3 py-1"
                      >
                        <span className="hidden sm:inline">View</span>
                        <span className="sm:hidden">👁</span>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      </div>

      {/* Create Load Request Modal */}
      <CreateLoadRequestModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        onSuccess={handleCreateSuccess}
      />
    </div>
  );
}