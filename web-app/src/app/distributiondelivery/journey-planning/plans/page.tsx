"use client";

import React, { useState, useCallback } from 'react';
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
  Navigation,
  Activity,
  Settings,
  Calendar,
  ArrowRight,
} from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { JourneyPlanGrid } from '@/components/admin/journey-plan/journey-plan-grid';
import { journeyPlanService } from '@/services/journeyPlanService';

interface FilterState {
  search: string;
  jobPositionUID: string;
  empUID: string;
  eotStatus: string;
  attendanceStatus: string;
  visitDate: Date | null;
  orgUID: string;
}

const JourneyPlanManagement: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();

  // State management
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteRouteId, setDeleteRouteId] = useState<string | null>(null);
  const [completeJourneyDialogOpen, setCompleteJourneyDialogOpen] = useState(false);
  const [completeJourneyId, setCompleteJourneyId] = useState<string | null>(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const [filters, setFilters] = useState<FilterState>({
    search: '',
    jobPositionUID: '',
    empUID: '',
    eotStatus: '',
    attendanceStatus: '',
    visitDate: null,
    orgUID: 'Farmley'
  });

  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, search: value }));
  };

  const handleFilterChange = (key: keyof FilterState, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleCreateJourneyPlan = () => {
    router.push('/distributiondelivery/journey-planning/plans/create');
  };

  const handleViewJourney = (uid: string) => {
    router.push(`/distributiondelivery/journey-planning/plans/view/${uid}`);
  };

  const handleEditJourney = (uid: string) => {
    router.push(`/distributiondelivery/journey-planning/plans/edit/${uid}`);
  };

  const handleDeleteClick = (uid: string) => {
    setDeleteRouteId(uid);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteRouteId) return;

    try {
      await journeyPlanService.deleteUserJourney(deleteRouteId);
      toast({
        title: 'Success',
        description: 'Journey deleted successfully'
      });
      triggerRefresh();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete journey',
        variant: 'destructive'
      });
    } finally {
      setDeleteDialogOpen(false);
      setDeleteRouteId(null);
    }
  };

  const handleCompleteJourneyClick = (uid: string) => {
    setCompleteJourneyId(uid);
    setCompleteJourneyDialogOpen(true);
  };

  const handleCompleteJourneyConfirm = async () => {
    if (!completeJourneyId) return;

    try {
      await journeyPlanService.completeUserJourney(completeJourneyId);
      toast({
        title: 'Success',
        description: 'Journey completed successfully'
      });
      triggerRefresh();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to complete journey',
        variant: 'destructive'
      });
    } finally {
      setCompleteJourneyDialogOpen(false);
      setCompleteJourneyId(null);
    }
  };

  const triggerRefresh = useCallback(() => {
    setRefreshTrigger((prev) => prev + 1);
  }, []);

  const handleRefresh = () => {
    triggerRefresh();
  };

  return (
    <div className="min-h-screen">
      {/* Modern Header with Gradient */}
      <div className="border-b sticky top-0 z-10 bg-white">
        <div className="container mx-auto px-4 py-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-100 rounded-lg">
                <Navigation className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <h1 className="text-2xl font-semibold text-gray-900">
                  Journey Plan Management
                </h1>
                <p className="text-sm text-muted-foreground">
                  Monitor and manage user journey plans across your organization
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() =>
                  router.push('/distributiondelivery/journey-planning/live-dashboard')
                }
                className=""
              >
                <Activity className="mr-2 h-4 w-4" />
                Live Dashboard
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() =>
                  router.push('/distributiondelivery/journey-planning/bulk-management')
                }
                className=""
              >
                <Settings className="mr-2 h-4 w-4" />
                Bulk Management
              </Button>
              <Button
                onClick={handleCreateJourneyPlan}
                size="sm"
                className="bg-blue-600 hover:bg-blue-700 text-white"
              >
                <Plus className="mr-2 h-4 w-4" />
                Create Journey Plan
              </Button>
            </div>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 py-6 space-y-6">
        {/* Quick Access Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => router.push('/distributiondelivery/journey-planning/today')}>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <Calendar className="h-8 w-8 text-blue-600" />
                <div>
                  <h3 className="font-semibold">Today's Plans</h3>
                  <p className="text-sm text-gray-600">View today's scheduled plans</p>
                </div>
                <ArrowRight className="h-4 w-4 text-gray-400 ml-auto" />
              </div>
            </CardContent>
          </Card>
          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => router.push('/distributiondelivery/journey-planning/bulk-management')}>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <Settings className="h-8 w-8 text-green-600" />
                <div>
                  <h3 className="font-semibold">Bulk Management</h3>
                  <p className="text-sm text-gray-600">Manage multiple plans</p>
                </div>
                <ArrowRight className="h-4 w-4 text-gray-400 ml-auto" />
              </div>
            </CardContent>
          </Card>
          <Card className="cursor-pointer hover:shadow-lg transition-shadow" onClick={() => router.push('/distributiondelivery/journey-planning/analytics')}>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <Activity className="h-8 w-8 text-purple-600" />
                <div>
                  <h3 className="font-semibold">Analytics</h3>
                  <p className="text-sm text-gray-600">View journey analytics</p>
                </div>
                <ArrowRight className="h-4 w-4 text-gray-400 ml-auto" />
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Main Content */}
        <Card className="border">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Navigation className="h-5 w-5" />
              User Journeys
            </CardTitle>
            <CardDescription>
              Manage and monitor user journey plans and their progress
            </CardDescription>
          </CardHeader>
          <CardContent className="p-0">
            {/* Filters */}
            <div className="p-6 border-b">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search by employee..."
                    value={filters.search}
                    onChange={(e) => handleSearch(e.target.value)}
                    className="pl-10"
                  />
                </div>

                <Select
                  value={filters.eotStatus || 'all'}
                  onValueChange={(value) =>
                    handleFilterChange(
                      'eotStatus',
                      value === 'all' ? '' : value
                    )
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="EOT Status" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Status</SelectItem>
                    <SelectItem value="Not Started">Not Started</SelectItem>
                    <SelectItem value="In Progress">In Progress</SelectItem>
                    <SelectItem value="Completed">Completed</SelectItem>
                    <SelectItem value="Paused">Paused</SelectItem>
                  </SelectContent>
                </Select>

                <Select
                  value={filters.attendanceStatus || 'all'}
                  onValueChange={(value) =>
                    handleFilterChange(
                      'attendanceStatus',
                      value === 'all' ? '' : value
                    )
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Attendance Status" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Status</SelectItem>
                    <SelectItem value="Present">Present</SelectItem>
                    <SelectItem value="Absent">Absent</SelectItem>
                    <SelectItem value="Late">Late</SelectItem>
                  </SelectContent>
                </Select>

                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={async () => {
                      try {
                        const blob = await journeyPlanService.exportUserJourneys("csv", {
                          search: filters.search,
                          eotStatus: filters.eotStatus,
                          attendanceStatus: filters.attendanceStatus
                        });
                        const url = URL.createObjectURL(blob);
                        const a = document.createElement("a");
                        a.href = url;
                        a.download = `user-journeys-export-${new Date().toISOString().split("T")[0]}.csv`;
                        document.body.appendChild(a);
                        a.click();
                        document.body.removeChild(a);
                        URL.revokeObjectURL(url);
                        
                        toast({
                          title: "Success",
                          description: "User journeys exported successfully.",
                        });
                      } catch (error) {
                        toast({
                          title: "Error",
                          description: "Failed to export user journeys. Please try again.",
                          variant: "destructive",
                        });
                      }
                    }}
                  >
                    <Download className="h-4 w-4 mr-2" />
                    Export
                  </Button>

                  <Button
                    variant="outline"
                    size="icon"
                    onClick={handleRefresh}
                    className="hover:bg-gray-50"
                  >
                    <RefreshCw className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </div>

            {/* Journey Plan Grid */}
            <JourneyPlanGrid
              searchQuery={filters.search}
              filters={{
                ...filters,
                visitDate: filters.visitDate || new Date() // Provide a default date for type compatibility
              }}
              refreshTrigger={refreshTrigger}
              onView={handleViewJourney}
              onEdit={handleEditJourney}
              onComplete={handleCompleteJourneyClick}
              onDelete={handleDeleteClick}
            />
          </CardContent>
        </Card>

        {/* Delete Confirmation Dialog */}
        <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Are you sure?</AlertDialogTitle>
              <AlertDialogDescription>
                This action cannot be undone. This will permanently delete the journey
                and all associated data.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDeleteConfirm}
                className="bg-destructive text-destructive-foreground"
              >
                Delete
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

        {/* Complete Journey Dialog */}
        <AlertDialog
          open={completeJourneyDialogOpen}
          onOpenChange={setCompleteJourneyDialogOpen}
        >
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Complete Journey</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to mark this journey as completed? This
                action will finalize the journey status.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction onClick={handleCompleteJourneyConfirm}>
                Complete Journey
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  );
};

export default JourneyPlanManagement;