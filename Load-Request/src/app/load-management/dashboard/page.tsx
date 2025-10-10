'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/hooks/useAuth';
import { BarChart3, TrendingUp, ClipboardList, CheckCircle, Package, Clock, MapPin } from 'lucide-react';
import { Button } from '@/components/ui/button';

export default function LoadManagementDashboard() {
  const router = useRouter();
  const { user } = useAuth();
  
  // State for dashboard data
  const [pendingRequests, setPendingRequests] = useState<any[]>([]);
  const [approvedRequests, setApprovedRequests] = useState<any[]>([]);
  const [logisticsStats, setLogisticsStats] = useState({
    todayApproved: 0,
    todayRejected: 0,
    averageApprovalTime: 2.5,
    totalRequestsToday: 0,
    approvalRate: 85
  });

  useEffect(() => {
    // Load mock data with proper statistics
    if (user) {
      // Mock data for dashboard
      const mockRequests = [
        { id: 1, requestNumber: 'LR001', customerName: 'Metro Store Downtown', status: 'Pending', totalAmount: 15000 },
        { id: 2, requestNumber: 'LR002', customerName: 'SuperMart Mall', status: 'Pending', totalAmount: 12000 },
        { id: 3, requestNumber: 'LR003', customerName: 'City Center Outlet', status: 'LogisticsApproved', totalAmount: 18000 },
        { id: 4, requestNumber: 'LR004', customerName: 'Express Corner Shop', status: 'LogisticsApproved', totalAmount: 8000 },
      ];
      
      const pendingCount = 8;
      const logisticsApprovedCount = 8;
      const rejectedCount = 8;
      
      setPendingRequests(mockRequests.slice(0, 2));
      setApprovedRequests(mockRequests.slice(2, 4));
      
      setLogisticsStats({
        todayApproved: logisticsApprovedCount,
        todayRejected: rejectedCount,
        averageApprovalTime: 3.2,
        totalRequestsToday: pendingCount + logisticsApprovedCount,
        approvalRate: Math.round((logisticsApprovedCount / (logisticsApprovedCount + rejectedCount)) * 100)
      });
    }
  }, [user]);

  // Navigation handlers
  const handleNavigateToLSR = () => {
    router.push('/load-management/lsr/create');
  };

  const handleNavigateToLogistics = () => {
    router.push('/load-management/logistics-approval');
  };

  const handleNavigateToRequests = () => {
    router.push('/load-management/lsr');
  };

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-gray-900">Welcome to Load Management</h1>
          <p className="mt-2 text-gray-600">Please sign in to access the dashboard</p>
        </div>
      </div>
    );
  }

  // Role-based dashboard content
  const isLogisticsRole = user.role === 'Logistics Approval Agent' || user.role === 'Agent Logistics Officer' || user.role?.includes('Logistics');
  const isLSRRole = user.role === 'LSR' || user.role === 'Van Sales Rep';

  return (
    <div className="p-8">
      <div className="max-w-7xl mx-auto">
        {/* Welcome Section */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            Welcome, {user.name || 'User'}
          </h1>
          <p className="mt-2 text-gray-600">
            {isLogisticsRole && "Manage and approve load requests from LSRs"}
            {isLSRRole && "Create and track your load requests"}
            {!isLogisticsRole && !isLSRRole && "Load Request Management Dashboard"}
          </p>
        </div>

        {/* LSR Cards - Only for LSR Users */}
        {(isLSRRole || !isLogisticsRole) && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <div className="bg-white overflow-hidden shadow-xs rounded-lg border">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <Package className="h-5 w-5 text-muted-foreground" />
                  </div>
                  <div className="ml-4 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-muted-foreground truncate">
                        Create New Request
                      </dt>
                      <dd className="text-lg font-medium text-foreground">
                        Start New
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-muted px-5 py-3">
                <Button 
                  onClick={handleNavigateToLSR}
                  variant="ghost"
                  size="sm"
                  className="p-0 h-auto font-medium"
                >
                  Create Request
                </Button>
              </div>
            </div>

            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <ClipboardList className="h-6 w-6 text-muted-foreground" />
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-muted-foreground truncate">
                        My Requests
                      </dt>
                      <dd className="text-lg font-medium text-foreground">
                        7
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-muted px-5 py-3">
                <div className="text-sm">
                  <Button 
                    onClick={handleNavigateToRequests}
                    variant="ghost"
                    className="p-0 h-auto font-medium"
                  >
                    View History
                  </Button>
                </div>
              </div>
            </div>

            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <CheckCircle className="h-6 w-6 text-muted-foreground" />
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-muted-foreground truncate">
                        Recent Status
                      </dt>
                      <dd className="text-lg font-medium text-foreground">
                        4 Approved
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-muted px-5 py-3">
                <div className="text-sm">
                  <Button 
                    onClick={handleNavigateToRequests}
                    variant="ghost"
                    className="p-0 h-auto font-medium"
                  >
                    Latest updates
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Stats Cards - Only for Logistics Users */}
        {isLogisticsRole && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <ClipboardList className="h-6 w-6 text-muted-foreground" />
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-muted-foreground truncate">
                        Pending Requests
                      </dt>
                      <dd className="text-lg font-medium text-foreground">
                        {logisticsStats.totalRequestsToday - logisticsStats.todayApproved}
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-muted px-5 py-3">
                <div className="text-sm">
                  <Button 
                    onClick={handleNavigateToLogistics}
                    variant="ghost"
                    className="p-0 h-auto font-medium"
                  >
                    View all
                  </Button>
                </div>
              </div>
            </div>

            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <TrendingUp className="h-6 w-6 text-muted-foreground" />
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-muted-foreground truncate">
                        Total Items Today
                      </dt>
                      <dd className="text-lg font-medium text-foreground">
                        {logisticsStats.totalRequestsToday * 5}
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-muted px-5 py-3">
                <div className="text-sm">
                  <span className="text-muted-foreground font-medium">
                    +12% from yesterday
                  </span>
                </div>
              </div>
            </div>

            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <CheckCircle className="h-6 w-6 text-muted-foreground" />
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-muted-foreground truncate">
                        Approved Today
                      </dt>
                      <dd className="text-lg font-medium text-foreground">
                        {logisticsStats.todayApproved}
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-muted px-5 py-3">
                <div className="text-sm">
                  <span className="text-muted-foreground font-medium">
                    {logisticsStats.approvalRate}% approval rate
                  </span>
                </div>
              </div>
            </div>

            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <Clock className="h-6 w-6 text-muted-foreground" />
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-muted-foreground truncate">
                        Avg Response Time
                      </dt>
                      <dd className="text-lg font-medium text-foreground">
                        {logisticsStats.averageApprovalTime}h
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-muted px-5 py-3">
                <div className="text-sm">
                  <span className="text-muted-foreground font-medium">
                    -20 min from last week
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Quick Actions */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Recent Requests */}
          <div className="bg-white shadow rounded-lg">
            <div className="px-4 py-5 sm:p-6">
              <h3 className="text-lg font-medium text-foreground mb-4">Recent Requests</h3>
              <div className="space-y-3">
                {pendingRequests.map((request) => (
                  <div key={request.id} className="flex items-center justify-between p-3 bg-muted rounded-lg">
                    <div>
                      <p className="text-sm font-medium text-foreground">{request.requestNumber}</p>
                      <p className="text-xs text-muted-foreground">{request.customerName}</p>
                    </div>
                    <div className="flex items-center space-x-2">
                      <span className="text-sm font-medium text-foreground">₹{request.totalAmount.toLocaleString()}</span>
                      <span className="px-2 py-1 text-xs font-medium bg-secondary text-secondary-foreground rounded-full">
                        {request.status}
                      </span>
                    </div>
                  </div>
                ))}
                {approvedRequests.map((request) => (
                  <div key={request.id} className="flex items-center justify-between p-3 bg-muted rounded-lg">
                    <div>
                      <p className="text-sm font-medium text-foreground">{request.requestNumber}</p>
                      <p className="text-xs text-muted-foreground">{request.customerName}</p>
                    </div>
                    <div className="flex items-center space-x-2">
                      <span className="text-sm font-medium text-foreground">₹{request.totalAmount.toLocaleString()}</span>
                      <span className="px-2 py-1 text-xs font-medium bg-secondary text-secondary-foreground rounded-full">
                        Approved
                      </span>
                    </div>
                  </div>
                ))}
              </div>
              <div className="mt-4">
                <Button 
                  onClick={handleNavigateToRequests}
                  className="w-full"
                  variant="outline"
                >
                  View All Requests
                </Button>
              </div>
            </div>
          </div>

          {/* Quick Actions */}
          <div className="bg-white shadow rounded-lg">
            <div className="px-4 py-5 sm:p-6">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Quick Actions</h3>
              <div className="space-y-3">
                <Button
                  onClick={handleNavigateToLSR}
                  className="w-full flex items-center justify-between p-4 bg-muted border rounded-lg hover:bg-muted/80 transition-colors"
                  variant="ghost"
                >
                  <div className="flex items-center">
                    <Package className="h-5 w-5 text-muted-foreground mr-3" />
                    <span className="text-sm font-medium">Create Load Request</span>
                  </div>
                  <span className="text-muted-foreground">→</span>
                </Button>
                
                {isLogisticsRole && (
                  <Button
                    onClick={handleNavigateToLogistics}
                    className="w-full flex items-center justify-between p-4 bg-muted border rounded-lg hover:bg-muted/80 transition-colors"
                    variant="ghost"
                  >
                    <div className="flex items-center">
                      <CheckCircle className="h-5 w-5 text-muted-foreground mr-3" />
                      <span className="text-sm font-medium">Logistics Approval</span>
                    </div>
                    <span className="text-muted-foreground">→</span>
                  </Button>
                )}
                
                <Button
                  onClick={() => window.location.reload()}
                  className="w-full flex items-center justify-between p-4 bg-muted border rounded-lg hover:bg-muted/80 transition-colors"
                  variant="ghost"
                >
                  <div className="flex items-center">
                    <BarChart3 className="h-5 w-5 text-muted-foreground mr-3" />
                    <span className="text-sm font-medium">Refresh Data</span>
                  </div>
                  <span className="text-muted-foreground">↻</span>
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}