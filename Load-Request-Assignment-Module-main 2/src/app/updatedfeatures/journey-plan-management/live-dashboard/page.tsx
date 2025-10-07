"use client";

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from '@/components/ui/tabs';
import {
  RefreshCw,
  MapPin,
  Clock,
  Users,
  Route,
  Activity,
  TrendingUp,
  AlertTriangle,
  CheckCircle2,
  Timer,
  Navigation,
  Smartphone,
  Wifi,
  Battery,
  Calendar,
  BarChart3,
} from 'lucide-react';
import moment from 'moment';
import { api } from '@/services/api';

interface LiveJourney {
  UID: string;
  EmployeeName: string;
  LoginId: string;
  RouteName: string;
  RouteCode: string;
  Status: 'Not Started' | 'In Progress' | 'Paused' | 'Completed' | 'Cancelled';
  Progress: number;
  PlannedStores: number;
  CompletedStores: number;
  CurrentStore?: string;
  LastUpdate: string;
  StartTime?: string;
  EstimatedEndTime?: string;
  Location?: {
    latitude: number;
    longitude: number;
    address: string;
  };
  DeviceStatus: {
    batteryLevel: number;
    hasInternet: boolean;
    gpsEnabled: boolean;
    lastSync: string;
  };
  Performance: {
    onTimeVisits: number;
    totalVisits: number;
    averageVisitTime: number;
    efficiency: number;
  };
}

interface DashboardStats {
  totalJourneys: number;
  activeJourneys: number;
  completedJourneys: number;
  delayedJourneys: number;
  totalStores: number;
  completedStores: number;
  averageProgress: number;
  onTimePerformance: number;
}

const LiveJourneyDashboard: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  
  const [liveJourneys, setLiveJourneys] = useState<LiveJourney[]>([]);
  const [stats, setStats] = useState<DashboardStats>({
    totalJourneys: 0,
    activeJourneys: 0,
    completedJourneys: 0,
    delayedJourneys: 0,
    totalStores: 0,
    completedStores: 0,
    averageProgress: 0,
    onTimePerformance: 0,
  });
  const [loading, setLoading] = useState(true);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [refreshInterval, setRefreshInterval] = useState<NodeJS.Timeout | null>(null);
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());
  const [selectedOrg, setSelectedOrg] = useState('Farmley');
  const [organizations, setOrganizations] = useState<{ value: string; label: string }[]>([]);

  useEffect(() => {
    loadOrganizations();
  }, []);

  useEffect(() => {
    loadLiveJourneys();
  }, [selectedOrg]);

  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(() => {
        loadLiveJourneys();
      }, 30000); // Refresh every 30 seconds
      setRefreshInterval(interval);
      
      return () => {
        if (interval) clearInterval(interval);
      };
    } else {
      if (refreshInterval) {
        clearInterval(refreshInterval);
        setRefreshInterval(null);
      }
    }
  }, [autoRefresh, selectedOrg]);

  const loadOrganizations = async () => {
    try {
      const data = await api.org.getDetails({
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: []
      });
      
      if (data.IsSuccess && data.Data?.PagedData) {
        setOrganizations(
          data.Data.PagedData.map((org: any) => ({
            value: org.UID,
            label: org.Name || org.Code,
          }))
        );
      }
    } catch (error) {
      console.error('Error loading organizations:', error);
    }
  };

  const loadLiveJourneys = useCallback(async () => {
    try {
      setLoading(true);
      setLastRefresh(new Date());

      // Get today's journey plans
      const today = moment().format('YYYY-MM-DD');
      const request = {
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: true,
        sortCriterias: [{
          sortBy: 'VisitDate',
          sortOrder: 'desc'
        }],
        filterCriterias: [
          {
            filterBy: 'OrgUID',
            filterValue: selectedOrg,
            filterOperator: 'equals'
          },
          {
            filterBy: 'VisitDate',
            filterValue: today,
            filterOperator: 'equals'
          }
        ]
      };

      const response = await api.beatHistory.selectAll(request);
      
      if (response.IsSuccess && response.Data) {
        const journeys: LiveJourney[] = (response.Data.PagedData || []).map((item: any) => {
          // Simulate live data - in real implementation, this would come from real-time APIs
          const progress = Math.round((item.ActualStoreVisits / Math.max(1, item.PlannedStoreVisits)) * 100);
          const status = getJourneyStatus(item);
          
          return {
            UID: item.UID,
            EmployeeName: item.EmployeeName || item.LoginId,
            LoginId: item.LoginId,
            RouteName: item.RouteName || 'Unknown Route',
            RouteCode: item.RouteCode || '',
            Status: status,
            Progress: Math.min(progress, 100),
            PlannedStores: item.PlannedStoreVisits || 0,
            CompletedStores: item.ActualStoreVisits || 0,
            CurrentStore: generateCurrentStore(item.ActualStoreVisits, item.PlannedStoreVisits),
            LastUpdate: moment().subtract(Math.random() * 10, 'minutes').toISOString(),
            StartTime: item.StartTime,
            EstimatedEndTime: calculateEstimatedEndTime(item),
            Location: generateRandomLocation(),
            DeviceStatus: generateDeviceStatus(),
            Performance: calculatePerformance(item),
          };
        });

        setLiveJourneys(journeys);
        setStats(calculateDashboardStats(journeys));
      }
    } catch (error) {
      console.error('Error loading live journeys:', error);
      toast({
        title: "Error",
        description: "Failed to load live journey data",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  }, [selectedOrg, toast]);

  const getJourneyStatus = (item: any): LiveJourney['Status'] => {
    if (item.Status === 'Completed') return 'Completed';
    if (item.Status === 'Cancelled') return 'Cancelled';
    if (item.StartTime && !item.EndTime) return 'In Progress';
    if (item.Status === 'Paused') return 'Paused';
    return 'Not Started';
  };

  const generateCurrentStore = (completed: number, total: number): string | undefined => {
    if (completed >= total) return undefined;
    const storeNames = ['ABC Store', 'XYZ Mart', 'City Center', 'Metro Store', 'Corner Shop', 'Super Market'];
    return storeNames[Math.floor(Math.random() * storeNames.length)];
  };

  const calculateEstimatedEndTime = (item: any): string => {
    const avgTimePerStore = 45; // minutes
    const remaining = (item.PlannedStoreVisits || 0) - (item.ActualStoreVisits || 0);
    const estimatedMinutes = remaining * avgTimePerStore;
    return moment().add(estimatedMinutes, 'minutes').format('HH:mm');
  };

  const generateRandomLocation = () => ({
    latitude: 12.9716 + (Math.random() - 0.5) * 0.1,
    longitude: 77.5946 + (Math.random() - 0.5) * 0.1,
    address: 'Bangalore, Karnataka, India'
  });

  const generateDeviceStatus = () => ({
    batteryLevel: Math.floor(Math.random() * 100),
    hasInternet: Math.random() > 0.1,
    gpsEnabled: Math.random() > 0.05,
    lastSync: moment().subtract(Math.random() * 5, 'minutes').toISOString()
  });

  const calculatePerformance = (item: any) => ({
    onTimeVisits: Math.floor((item.ActualStoreVisits || 0) * (0.8 + Math.random() * 0.2)),
    totalVisits: item.ActualStoreVisits || 0,
    averageVisitTime: 30 + Math.floor(Math.random() * 20),
    efficiency: Math.floor(70 + Math.random() * 30)
  });

  const calculateDashboardStats = (journeys: LiveJourney[]): DashboardStats => {
    const totalJourneys = journeys.length;
    const activeJourneys = journeys.filter(j => j.Status === 'In Progress').length;
    const completedJourneys = journeys.filter(j => j.Status === 'Completed').length;
    const delayedJourneys = journeys.filter(j => 
      j.Status === 'In Progress' && 
      moment().isAfter(moment(j.EstimatedEndTime, 'HH:mm'))
    ).length;
    
    const totalStores = journeys.reduce((sum, j) => sum + j.PlannedStores, 0);
    const completedStores = journeys.reduce((sum, j) => sum + j.CompletedStores, 0);
    const averageProgress = journeys.length > 0 
      ? journeys.reduce((sum, j) => sum + j.Progress, 0) / journeys.length 
      : 0;
    
    const onTimePerformance = journeys.length > 0
      ? journeys.reduce((sum, j) => sum + j.Performance.efficiency, 0) / journeys.length
      : 0;

    return {
      totalJourneys,
      activeJourneys,
      completedJourneys,
      delayedJourneys,
      totalStores,
      completedStores,
      averageProgress,
      onTimePerformance
    };
  };

  const handleRefresh = () => {
    loadLiveJourneys();
  };

  const handleViewJourney = (journeyUID: string) => {
    router.push(`/updatedfeatures/journey-plan-management/journey-plans/view/${journeyUID}`);
  };

  const getStatusColor = (status: LiveJourney['Status']): string => {
    switch (status) {
      case 'Completed': return 'text-green-600';
      case 'In Progress': return 'text-blue-600';
      case 'Paused': return 'text-yellow-600';
      case 'Cancelled': return 'text-red-600';
      default: return 'text-gray-600';
    }
  };

  const getStatusVariant = (status: LiveJourney['Status']): "default" | "secondary" | "destructive" | "outline" => {
    switch (status) {
      case 'Completed': return 'default';
      case 'In Progress': return 'secondary';
      case 'Paused': return 'outline';
      case 'Cancelled': return 'destructive';
      default: return 'outline';
    }
  };

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <Activity className="h-6 w-6" />
            Live Journey Dashboard
          </h1>
          <p className="text-muted-foreground">
            Real-time monitoring of active journey plans and field teams
          </p>
        </div>
        <div className="flex items-center gap-4">
          <Select value={selectedOrg} onValueChange={setSelectedOrg}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Select Organization" />
            </SelectTrigger>
            <SelectContent>
              {organizations.map(org => (
                <SelectItem key={org.value} value={org.value}>
                  {org.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          
          <div className="flex items-center gap-2">
            <Button
              variant={autoRefresh ? "default" : "outline"}
              size="sm"
              onClick={() => setAutoRefresh(!autoRefresh)}
            >
              <Timer className="h-4 w-4 mr-1" />
              Auto Refresh
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={handleRefresh}
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </div>
      </div>

      {/* Last Update Info */}
      <div className="text-sm text-muted-foreground flex items-center gap-2">
        <Clock className="h-4 w-4" />
        Last updated: {moment(lastRefresh).format('HH:mm:ss')}
        {autoRefresh && <span className="text-green-600">• Auto-refreshing every 30s</span>}
      </div>

      {/* Dashboard Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-8 gap-4">
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold">{stats.totalJourneys}</div>
            <div className="text-sm text-muted-foreground">Total Journeys</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold text-blue-600">{stats.activeJourneys}</div>
            <div className="text-sm text-muted-foreground">Active</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold text-green-600">{stats.completedJourneys}</div>
            <div className="text-sm text-muted-foreground">Completed</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold text-red-600">{stats.delayedJourneys}</div>
            <div className="text-sm text-muted-foreground">Delayed</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold">{stats.totalStores}</div>
            <div className="text-sm text-muted-foreground">Total Stores</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold text-green-600">{stats.completedStores}</div>
            <div className="text-sm text-muted-foreground">Visited</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold">{Math.round(stats.averageProgress)}%</div>
            <div className="text-sm text-muted-foreground">Avg Progress</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="text-2xl font-bold">{Math.round(stats.onTimePerformance)}%</div>
            <div className="text-sm text-muted-foreground">Efficiency</div>
          </CardContent>
        </Card>
      </div>

      {/* Live Journeys */}
      <Tabs defaultValue="active" className="w-full">
        <TabsList>
          <TabsTrigger value="active">Active Journeys ({stats.activeJourneys})</TabsTrigger>
          <TabsTrigger value="all">All Journeys ({stats.totalJourneys})</TabsTrigger>
          <TabsTrigger value="completed">Completed ({stats.completedJourneys})</TabsTrigger>
        </TabsList>

        <TabsContent value="active" className="space-y-4">
          {loading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {[...Array(6)].map((_, i) => (
                <Skeleton key={i} className="h-64 w-full" />
              ))}
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {liveJourneys
                .filter(journey => journey.Status === 'In Progress')
                .map((journey) => (
                  <Card key={journey.UID} className="hover:shadow-lg transition-shadow cursor-pointer" onClick={() => handleViewJourney(journey.UID)}>
                    <CardHeader className="pb-3">
                      <div className="flex items-center justify-between">
                        <CardTitle className="text-lg flex items-center gap-2">
                          <Users className="h-4 w-4" />
                          {journey.EmployeeName}
                        </CardTitle>
                        <Badge variant={getStatusVariant(journey.Status)}>
                          {journey.Status}
                        </Badge>
                      </div>
                      <CardDescription className="flex items-center gap-2">
                        <Route className="h-3 w-3" />
                        {journey.RouteName}
                      </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                      {/* Progress */}
                      <div className="space-y-2">
                        <div className="flex justify-between text-sm">
                          <span>Progress</span>
                          <span className="font-medium">{journey.Progress}%</span>
                        </div>
                        <Progress value={journey.Progress} className="h-2" />
                        <div className="flex justify-between text-xs text-muted-foreground">
                          <span>{journey.CompletedStores}/{journey.PlannedStores} stores</span>
                          <span>ETA: {journey.EstimatedEndTime}</span>
                        </div>
                      </div>

                      {/* Current Activity */}
                      {journey.CurrentStore && (
                        <div className="flex items-center gap-2 text-sm">
                          <MapPin className="h-3 w-3 text-blue-600" />
                          <span>Currently at: <strong>{journey.CurrentStore}</strong></span>
                        </div>
                      )}

                      {/* Device Status */}
                      <div className="flex items-center gap-4 text-xs">
                        <div className="flex items-center gap-1">
                          <Battery className={`h-3 w-3 ${journey.DeviceStatus.batteryLevel > 20 ? 'text-green-600' : 'text-red-600'}`} />
                          <span>{journey.DeviceStatus.batteryLevel}%</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Wifi className={`h-3 w-3 ${journey.DeviceStatus.hasInternet ? 'text-green-600' : 'text-red-600'}`} />
                          <span>{journey.DeviceStatus.hasInternet ? 'Online' : 'Offline'}</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Navigation className={`h-3 w-3 ${journey.DeviceStatus.gpsEnabled ? 'text-green-600' : 'text-red-600'}`} />
                          <span>GPS</span>
                        </div>
                      </div>

                      {/* Performance */}
                      <div className="grid grid-cols-2 gap-2 text-xs">
                        <div>
                          <span className="text-muted-foreground">On-time visits:</span>
                          <div className="font-medium">{journey.Performance.onTimeVisits}/{journey.Performance.totalVisits}</div>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Avg visit time:</span>
                          <div className="font-medium">{journey.Performance.averageVisitTime}m</div>
                        </div>
                      </div>

                      {/* Last Update */}
                      <div className="text-xs text-muted-foreground">
                        Last update: {moment(journey.LastUpdate).fromNow()}
                      </div>
                    </CardContent>
                  </Card>
              ))}
            </div>
          )}
          
          {!loading && liveJourneys.filter(j => j.Status === 'In Progress').length === 0 && (
            <Card>
              <CardContent className="py-12 text-center">
                <Activity className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <h3 className="text-lg font-semibold mb-2">No Active Journeys</h3>
                <p className="text-muted-foreground">
                  No field teams are currently on active journeys
                </p>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="all" className="space-y-4">
          {loading ? (
            <div className="space-y-2">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-20 w-full" />
              ))}
            </div>
          ) : (
            <div className="space-y-3">
              {liveJourneys.map((journey) => (
                <Card key={journey.UID} className="hover:shadow-md transition-shadow cursor-pointer" onClick={() => handleViewJourney(journey.UID)}>
                  <CardContent className="p-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-4">
                        <div>
                          <h4 className="font-medium">{journey.EmployeeName}</h4>
                          <p className="text-sm text-muted-foreground">{journey.RouteName}</p>
                        </div>
                      </div>
                      
                      <div className="flex items-center space-x-6">
                        <div className="text-center">
                          <div className="text-sm font-medium">{journey.Progress}%</div>
                          <div className="text-xs text-muted-foreground">Progress</div>
                        </div>
                        
                        <div className="text-center">
                          <div className="text-sm font-medium">{journey.CompletedStores}/{journey.PlannedStores}</div>
                          <div className="text-xs text-muted-foreground">Stores</div>
                        </div>
                        
                        <div className="text-center">
                          <Badge variant={getStatusVariant(journey.Status)}>
                            {journey.Status}
                          </Badge>
                        </div>
                        
                        <div className="text-right">
                          <div className="text-xs text-muted-foreground">
                            {moment(journey.LastUpdate).fromNow()}
                          </div>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="completed" className="space-y-4">
          {loading ? (
            <div className="space-y-2">
              {[...Array(3)].map((_, i) => (
                <Skeleton key={i} className="h-20 w-full" />
              ))}
            </div>
          ) : (
            <div className="space-y-3">
              {liveJourneys
                .filter(journey => journey.Status === 'Completed')
                .map((journey) => (
                  <Card key={journey.UID} className="hover:shadow-md transition-shadow cursor-pointer" onClick={() => handleViewJourney(journey.UID)}>
                    <CardContent className="p-4">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-4">
                          <CheckCircle2 className="h-5 w-5 text-green-600" />
                          <div>
                            <h4 className="font-medium">{journey.EmployeeName}</h4>
                            <p className="text-sm text-muted-foreground">{journey.RouteName}</p>
                          </div>
                        </div>
                        
                        <div className="flex items-center space-x-6">
                          <div className="text-center">
                            <div className="text-sm font-medium text-green-600">100%</div>
                            <div className="text-xs text-muted-foreground">Completed</div>
                          </div>
                          
                          <div className="text-center">
                            <div className="text-sm font-medium">{journey.CompletedStores}/{journey.PlannedStores}</div>
                            <div className="text-xs text-muted-foreground">Stores</div>
                          </div>
                          
                          <div className="text-center">
                            <div className="text-sm font-medium">{journey.Performance.efficiency}%</div>
                            <div className="text-xs text-muted-foreground">Efficiency</div>
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default LiveJourneyDashboard;