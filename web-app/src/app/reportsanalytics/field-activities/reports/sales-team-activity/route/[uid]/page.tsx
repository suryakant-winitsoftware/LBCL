'use client';

import React, { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
// import {
//   Select,
//   SelectContent,
//   SelectItem,
//   SelectTrigger,
//   SelectValue,
// } from '@/components/ui/select';
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from '@/components/ui/table';
import { 
  ArrowLeft, 
  Search, 
  Calendar, 
  User, 
  MapPin, 
  Clock, 
  ChevronDown, 
  ChevronRight, 
  Activity, 
  Target, 
  TrendingUp, 
  BarChart3, 
  Users, 
  Store, 
  Timer, 
  DollarSign, 
  Filter, 
  X, 
  RefreshCw,
  Building2,
  Route,
  FileText,
  Navigation,
  Phone,
  Battery,
  Smartphone,
  CheckCircle,
  XCircle,
  AlertCircle,
  Info,
  Wifi,
  Signal,
  Globe,
  Zap,
  Shield,
  Gauge,
  PlayCircle,
  StopCircle,
  Pause,
  Radio,
  Satellite,
  Network,
  Eye,
  UserCheck,
  MapPinned,
  Compass,
  Layers,
  MoreHorizontal,
  TrendingDown,
  Download,
  Upload,
  Monitor,
  Tablet,
  WifiOff,
  SignalHigh,
  SignalLow,
  SignalMedium,
  PowerOff,
  Power,
  LocateOff,
  Crosshair
} from 'lucide-react';
import salesTeamActivityService from '@/services/sales-team-activity.service';
import fileSysService from '@/services/file-sys.service';
import { format } from 'date-fns';
import { Skeleton } from '@/components/ui/skeleton';

interface BeatHistory {
  UID: string;
  RouteUID: string;
  JobPositionUID: string;
  LoginId: string;
  VisitDate: string;
  StartTime?: string;
  EndTime?: string;
  PlannedStartTime?: string;
  PlannedEndTime?: string;
  PlannedStoreVisits: number;
  ActualStoreVisits: number;
  UnplannedStoreVisits: number;
  SkippedStoreVisits: number;
  Coverage: number;
  ACoverage: number;
  Notes?: string;
  Status?: string;
  HasAuditCompleted?: boolean;
}

export default function RouteBeatHistoryPage() {
  const params = useParams();
  const router = useRouter();
  const routeUID = params.uid as string;

  const [beatHistories, setBeatHistories] = useState<BeatHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20); // Remove setPageSize since it's not needed
  const [totalCount, setTotalCount] = useState(0);
  const [routeInfo, setRouteInfo] = useState<any>(null);
  const [dateFilter, setDateFilter] = useState<'before' | 'today' | 'all' | 'custom'>('today');
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [dateRange, setDateRange] = useState<{from: Date | null; to: Date | null}>({
    from: null,
    to: null
  });
  
  // Inline expansion states
  const [expandedBeatHistory, setExpandedBeatHistory] = useState<string | null>(null);
  const [expandedUserJourney, setExpandedUserJourney] = useState<string | null>(null);
  const [beatHistoryJourneys, setBeatHistoryJourneys] = useState<Record<string, any[]>>({});
  const [userJourneyStores, setUserJourneyStores] = useState<Record<string, any[]>>({});
  const [loadingJourneys, setLoadingJourneys] = useState<Record<string, boolean>>({});
  const [loadingStores, setLoadingStores] = useState<Record<string, boolean>>({});
  
  // File system state
  const [journeyFiles, setJourneyFiles] = useState<Record<string, any[]>>({});
  const [loadingFiles, setLoadingFiles] = useState<Record<string, boolean>>({});
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  // Load route information
  const loadRouteInfo = async () => {
    try {
      const routesData = await salesTeamActivityService.getRoutes({
        pageNumber: 1,
        pageSize: 100
      });
      
      if (routesData?.Data?.PagedData?.length > 0) {
        const matchingRoute = routesData.Data.PagedData.find(
          (route: any) => route.UID === routeUID || route.uid === routeUID
        );
        if (matchingRoute) {
          setRouteInfo(matchingRoute);
        }
      }
    } catch (error) {
      console.error('Error loading route info:', error);
    }
  };

  // Load beat histories
  const loadBeatHistories = async () => {
    setLoading(true);
    try {
      const response = await salesTeamActivityService.getBeatHistoriesByRoute(
        routeUID, 
        currentPage, 
        pageSize
      );
      
      let histories = [];
      let total = 0;
      
      if (response?.Data?.PagedData) {
        histories = response.Data.PagedData;
        total = response.Data.TotalCount || histories.length;
      } else if (Array.isArray(response?.Data)) {
        histories = response.Data;
        total = histories.length;
      }
      
      setBeatHistories(histories);
      setTotalCount(total);
      
      console.log(`Loaded ${histories.length} beat histories for route ${routeUID}`);
      if (histories.length > 0) {
        console.log('Sample beat history data:', histories[0]);
        console.log('Available fields:', Object.keys(histories[0]));
      }
    } catch (error) {
      console.error('Error loading beat histories:', error);
      setBeatHistories([]);
    } finally {
      setLoading(false);
    }
  };

  // Toggle beat history expansion and load user journeys
  const toggleBeatHistory = async (beatHistoryUID: string) => {
    if (expandedBeatHistory === beatHistoryUID) {
      setExpandedBeatHistory(null);
      setExpandedUserJourney(null);
    } else {
      setExpandedBeatHistory(beatHistoryUID);
      setExpandedUserJourney(null);
      
      if (!beatHistoryJourneys[beatHistoryUID]) {
        setLoadingJourneys(prev => ({ ...prev, [beatHistoryUID]: true }));
        try {
          console.log('Loading user journeys for beat history:', beatHistoryUID);
          
          const response = await salesTeamActivityService.getUserJourneys({
            filterCriterias: [{
              fieldName: 'BeatHistoryUID',
              operator: 'Equals',
              value: beatHistoryUID
            }],
            pageNumber: 1,
            pageSize: 100
          });
          
          let journeys = [];
          if (response?.Data?.PagedData) {
            journeys = response.Data.PagedData;
          } else if (Array.isArray(response?.Data)) {
            journeys = response.Data;
          }
          
          setBeatHistoryJourneys(prev => ({ ...prev, [beatHistoryUID]: journeys }));
          console.log(`Loaded ${journeys.length} user journeys for beat history ${beatHistoryUID}`);
          
          if (journeys.length > 0) {
            console.log('Sample journey location data:', {
              address: journeys[0].AttendanceAddress || journeys[0].attendance_address,
              lat: journeys[0].AttendanceLatitude || journeys[0].attendance_latitude,
              lng: journeys[0].AttendanceLongitude || journeys[0].attendance_longitude,
              allFields: Object.keys(journeys[0])
            });
            
            // Load attendance photos for the first journey (for table display)
            if (journeys[0]?.UID) {
              await loadJourneyFiles(journeys[0].UID);
            }
          }
        } catch (error) {
          console.error('Error loading user journeys:', error);
          setBeatHistoryJourneys(prev => ({ ...prev, [beatHistoryUID]: [] }));
        } finally {
          setLoadingJourneys(prev => ({ ...prev, [beatHistoryUID]: false }));
        }
      }
    }
  };

  // Load files for a user journey
  const loadJourneyFiles = async (userJourneyUID: string) => {
    if (journeyFiles[userJourneyUID]) {
      console.log('🔍 Route Report: Files already loaded for journey:', userJourneyUID);
      return; // Already loaded
    }
    
    console.log('🔍 Route Report: Starting to load attendance photos for user journey:', userJourneyUID);
    setLoadingFiles(prev => ({ ...prev, [userJourneyUID]: true }));
    
    try {
      console.log('🔍 Route Report: Calling fileSysService.getAttendancePhotosByJourneyUID...');
      
      const files = await fileSysService.getAttendancePhotosByJourneyUID(userJourneyUID);
      
      console.log('🔍 Route Report: Files loaded successfully:', {
        userJourneyUID,
        fileCount: files.length,
        files: files.map(file => ({
          UID: file.UID,
          FileName: file.FileName,
          RelativePath: file.RelativePath,
          LinkedItemType: file.LinkedItemType,
          FileType: file.FileType,
          FileSize: file.FileSize,
          GeneratedURL: fileSysService.getFileUrl(file)
        }))
      });
      
      setJourneyFiles(prev => ({ ...prev, [userJourneyUID]: files }));
      console.log(`✅ Route Report: Successfully loaded ${files.length} attendance photos for user journey ${userJourneyUID}`);
      
      // Test file URLs for images
      files.forEach(file => {
        if (fileSysService.isImage(file.FileType)) {
          const url = fileSysService.getFileUrl(file);
          console.log(`🖼️ Route Report: Image file detected - ${file.FileName}:`, {
            fileType: file.FileType,
            relativePath: file.RelativePath,
            generatedURL: url,
            isImage: fileSysService.isImage(file.FileType)
          });
        }
      });
      
    } catch (error) {
      console.error('❌ Route Report: Error loading journey files:', error);
      setJourneyFiles(prev => ({ ...prev, [userJourneyUID]: [] }));
    } finally {
      setLoadingFiles(prev => ({ ...prev, [userJourneyUID]: false }));
    }
  };

  // Toggle user journey expansion and load store histories + files
  const toggleUserJourney = async (userJourneyUID: string) => {
    if (expandedUserJourney === userJourneyUID) {
      setExpandedUserJourney(null);
    } else {
      setExpandedUserJourney(userJourneyUID);
      
      // Load store histories
      if (!userJourneyStores[userJourneyUID]) {
        setLoadingStores(prev => ({ ...prev, [userJourneyUID]: true }));
        try {
          console.log('Loading store histories for user journey:', userJourneyUID);
          
          const histories = await salesTeamActivityService.getStoreHistoriesByUserJourneyUID(userJourneyUID);
          
          setUserJourneyStores(prev => ({ ...prev, [userJourneyUID]: histories || [] }));
          console.log(`Loaded ${histories?.length || 0} store histories for user journey ${userJourneyUID}`);
        } catch (error) {
          console.error('Error loading store histories:', error);
          setUserJourneyStores(prev => ({ ...prev, [userJourneyUID]: [] }));
        } finally {
          setLoadingStores(prev => ({ ...prev, [userJourneyUID]: false }));
        }
      }
      
      // Load files for this journey
      await loadJourneyFiles(userJourneyUID);
    }
  };

  useEffect(() => {
    if (routeUID) {
      loadRouteInfo();
      loadBeatHistories();
    }
  }, [routeUID, currentPage, pageSize]);

  // Filter beat histories based on search and date
  const filteredHistories = beatHistories.filter(history => {
    // Apply date filter
    if (dateFilter === 'before') {
      const today = new Date();
      const historyDate = history.VisitDate ? new Date(history.VisitDate) : null;
      
      if (historyDate) {
        // Reset time to start of day for comparison
        const historyDateOnly = new Date(historyDate.getFullYear(), historyDate.getMonth(), historyDate.getDate());
        const todayOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        
        if (historyDateOnly >= todayOnly) return false; // Only show dates before today
      } else {
        return false;
      }
    } else if (dateFilter === 'today') {
      const today = new Date();
      const historyDate = history.VisitDate ? new Date(history.VisitDate) : null;
      
      if (historyDate) {
        const isToday = 
          historyDate.getDate() === today.getDate() &&
          historyDate.getMonth() === today.getMonth() &&
          historyDate.getFullYear() === today.getFullYear();
        
        if (!isToday) return false;
      } else {
        return false;
      }
    } else if (dateFilter === 'custom' && (dateRange.from || dateRange.to)) {
      const historyDate = history.VisitDate ? new Date(history.VisitDate) : null;
      
      if (historyDate) {
        // Reset time to start of day for comparison
        const historyDateOnly = new Date(historyDate.getFullYear(), historyDate.getMonth(), historyDate.getDate());
        
        // Check if date is within the range
        if (dateRange.from) {
          const fromDate = new Date(dateRange.from.getFullYear(), dateRange.from.getMonth(), dateRange.from.getDate());
          if (historyDateOnly < fromDate) return false;
        }
        
        if (dateRange.to) {
          const toDate = new Date(dateRange.to.getFullYear(), dateRange.to.getMonth(), dateRange.to.getDate());
          if (historyDateOnly > toDate) return false;
        }
      } else {
        return false;
      }
    }
    
    // Apply search filter
    if (!searchTerm) return true;
    const searchLower = searchTerm.toLowerCase();
    return (
      history.UID?.toLowerCase().includes(searchLower) ||
      history.LoginId?.toLowerCase().includes(searchLower) ||
      history.Notes?.toLowerCase().includes(searchLower) ||
      (history.VisitDate && format(new Date(history.VisitDate), 'dd/MM/yyyy').includes(searchLower))
    );
  });

  const totalPages = Math.ceil(totalCount / pageSize);

  const getStatusIcon = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'completed':
        return <CheckCircle className="h-4 w-4 text-green-600" />;
      case 'skipped':
        return <XCircle className="h-4 w-4 text-red-600" />;
      case 'in progress':
        return <AlertCircle className="h-4 w-4 text-orange-600" />;
      default:
        return <Info className="h-4 w-4 text-gray-600" />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'completed':
        return 'bg-green-50 text-green-700 border-green-200';
      case 'skipped':
        return 'bg-red-50 text-red-700 border-red-200';
      case 'in progress':
        return 'bg-orange-50 text-orange-700 border-orange-200';
      default:
        return 'bg-gray-50 text-gray-700 border-gray-200';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto p-6 max-w-7xl">
        {/* Header Section */}
        <div className="mb-8">
          {/* Navigation */}
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-3">
              <Button
                variant="outline"
                size="sm"
                onClick={() => router.back()}
                className="flex items-center gap-2"
              >
                <ArrowLeft className="h-4 w-4" />
                Back to Routes
              </Button>
            </div>
            
            <div className="flex items-center gap-3">
              <Button
                variant="outline"
                size="sm"
                onClick={() => window.location.reload()}
                disabled={loading}
                className="flex items-center gap-2"
              >
                <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
                Refresh
              </Button>
            </div>
          </div>
          
          {/* Route Information Card */}
          <Card className="border-0 shadow-sm">
            <CardContent className="p-8">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-4 mb-6">
                    <div className="p-3 bg-blue-600 rounded-lg">
                      <Route className="h-6 w-6 text-white" />
                    </div>
                    <div>
                      <h1 className="text-2xl font-semibold text-gray-900">
                        Sales Route Activity Report
                      </h1>
                      <p className="text-gray-600 font-medium mt-1">
                        {routeInfo?.Code} - {routeInfo?.Name}
                      </p>
                    </div>
                  </div>
                  
                  {/* Stats Grid */}
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                    <div className="bg-white border border-gray-200 rounded-lg p-4">
                      <div className="flex items-center gap-3">
                        <div className="p-2 bg-blue-100 rounded-lg">
                          <BarChart3 className="h-5 w-5 text-blue-600" />
                        </div>
                        <div>
                          <p className="text-sm font-medium text-gray-600">Total Records</p>
                          <p className="text-2xl font-semibold text-gray-900">{beatHistories.length}</p>
                        </div>
                      </div>
                    </div>
                    
                    <div className="bg-white border border-gray-200 rounded-lg p-4">
                      <div className="flex items-center gap-3">
                        <div className="p-2 bg-green-100 rounded-lg">
                          <Activity className="h-5 w-5 text-green-600" />
                        </div>
                        <div>
                          <p className="text-sm font-medium text-gray-600">Filtered Results</p>
                          <p className="text-2xl font-semibold text-gray-900">{filteredHistories.length}</p>
                        </div>
                      </div>
                    </div>
                    
                    <div className="bg-white border border-gray-200 rounded-lg p-4">
                      <div className="flex items-center gap-3">
                        <div className="p-2 bg-purple-100 rounded-lg">
                          <Building2 className="h-5 w-5 text-purple-600" />
                        </div>
                        <div>
                          <p className="text-sm font-medium text-gray-600">Route Status</p>
                          <p className="text-lg font-semibold text-gray-900">
                            {routeInfo?.IsActive ? 'Active' : 'Inactive'}
                          </p>
                        </div>
                      </div>
                    </div>
                    
                    <div className="bg-white border border-gray-200 rounded-lg p-4">
                      <div className="flex items-center gap-3">
                        <div className="p-2 bg-orange-100 rounded-lg">
                          <Users className="h-5 w-5 text-orange-600" />
                        </div>
                        <div>
                          <p className="text-sm font-medium text-gray-600">Total Customers</p>
                          <p className="text-2xl font-semibold text-gray-900">{routeInfo?.TotalCustomers || 0}</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Filters Section */}
        <Card className="mb-6">
          <CardContent className="p-6">
            <div className="flex items-center gap-3 mb-6">
              <Filter className="h-5 w-5 text-gray-600" />
              <h2 className="text-lg font-semibold text-gray-900">Filters</h2>
              <span className="text-sm text-gray-500">({filteredHistories.length} results)</span>
            </div>
            
            {/* Search Bar and Date Filters in Same Row */}
            <div className="flex items-center flex-wrap gap-4 mb-6">
              {/* Search Bar */}
              <div className="relative w-80">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  placeholder="Search by User ID, Date, or Notes..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>

              {/* Separator */}
              <div className="w-6"></div>

              {/* Date Filters */}
              <Button
                variant={dateFilter === 'before' ? 'default' : 'outline'}
                size="sm"
                onClick={() => setDateFilter('before')}
                className="flex items-center gap-2"
              >
                <Calendar className="h-4 w-4" />
                Before Today
                {dateFilter === 'before' && filteredHistories.length > 0 && (
                  <span className="ml-1 px-2 py-0.5 bg-white/20 rounded-full text-xs">
                    {filteredHistories.length}
                  </span>
                )}
              </Button>

              <Button
                variant={dateFilter === 'today' ? 'default' : 'outline'}
                size="sm"
                onClick={() => setDateFilter('today')}
                className="flex items-center gap-2"
              >
                <Calendar className="h-4 w-4" />
                Today
                {dateFilter === 'today' && filteredHistories.length > 0 && (
                  <span className="ml-1 px-2 py-0.5 bg-white/20 rounded-full text-xs">
                    {filteredHistories.length}
                  </span>
                )}
              </Button>
              
              <Button
                variant={dateFilter === 'all' ? 'default' : 'outline'}
                size="sm"
                onClick={() => setDateFilter('all')}
                className="flex items-center gap-2"
              >
                <Clock className="h-4 w-4" />
                All Days
                {dateFilter === 'all' && beatHistories.length > 0 && (
                  <span className="ml-1 px-2 py-0.5 bg-white/20 rounded-full text-xs">
                    {beatHistories.length}
                  </span>
                )}
              </Button>
              
              <Button
                variant={dateFilter === 'custom' ? 'default' : 'outline'}
                size="sm"
                onClick={() => setDateFilter('custom')}
                className="flex items-center gap-2"
              >
                <Calendar className="h-4 w-4" />
                Date Range
              </Button>
            </div>
            
            {/* Date Range Picker */}
            {dateFilter === 'custom' && (
              <div className="mb-6 p-4 bg-gray-50 border border-gray-200 rounded-lg">
                <div className="flex items-center gap-4">
                  <div className="flex items-center gap-2">
                    <label className="text-sm font-medium text-gray-700">From:</label>
                    <Input
                      type="date"
                      value={dateRange.from ? format(dateRange.from, 'yyyy-MM-dd') : ''}
                      onChange={(e) => {
                        const newDate = e.target.value ? new Date(e.target.value) : null;
                        setDateRange(prev => ({ ...prev, from: newDate }));
                      }}
                      className="w-40"
                    />
                  </div>
                  
                  <div className="flex items-center gap-2">
                    <label className="text-sm font-medium text-gray-700">To:</label>
                    <Input
                      type="date"
                      value={dateRange.to ? format(dateRange.to, 'yyyy-MM-dd') : ''}
                      onChange={(e) => {
                        const newDate = e.target.value ? new Date(e.target.value) : null;
                        setDateRange(prev => ({ ...prev, to: newDate }));
                      }}
                      className="w-40"
                    />
                  </div>
                  
                  {(dateRange.from || dateRange.to) && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setDateRange({ from: null, to: null })}
                      className="flex items-center gap-1"
                    >
                      <X className="h-3 w-3" />
                      Clear
                    </Button>
                  )}
                </div>
                
                {dateRange.from && dateRange.to && (
                  <div className="mt-3 text-sm text-gray-600">
                    <span className="font-medium">Selected range:</span>{' '}
                    {format(dateRange.from, 'MMM dd, yyyy')} - {format(dateRange.to, 'MMM dd, yyyy')}
                    {' '}({Math.ceil((dateRange.to.getTime() - dateRange.from.getTime()) / (1000 * 60 * 60 * 24)) + 1} days)
                  </div>
                )}
              </div>
            )}
            
            {/* Status */}
            {dateFilter === 'today' && filteredHistories.length === 0 && beatHistories.length > 0 && (
              <div className="mt-4 p-3 bg-orange-50 border border-orange-200 rounded-lg flex items-center gap-2">
                <AlertCircle className="h-4 w-4 text-orange-600" />
                <span className="text-sm text-orange-700">
                  No records for today. Try "All Days" to view historical data.
                </span>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Activity Data Table */}
        <Card className="border-0 shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <FileText className="h-5 w-5" />
              Beat History Overview
              <span className="text-sm font-normal text-gray-500">
                ({filteredHistories.length} {dateFilter === 'today' ? 'today' : 'total'})
              </span>
              {dateFilter === 'today' && (
                <span className="ml-auto text-sm font-medium px-3 py-1 bg-blue-50 text-blue-700 rounded-lg border border-blue-200">
                  Today's Records
                </span>
              )}
            </CardTitle>
            <p className="text-sm text-gray-600">
              Expand any row to view detailed user journeys and store visit histories
            </p>
          </CardHeader>
          <CardContent className="p-0">
            {loading ? (
              <div className="p-6">
                {/* Skeleton Table Header */}
                <div className="bg-gray-50 p-4 border-b">
                  <div className="grid grid-cols-4 gap-4">
                    <Skeleton className="h-4 w-16" />
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-4 w-14" />
                    <Skeleton className="h-4 w-12" />
                  </div>
                </div>
                {/* Skeleton Table Rows */}
                {[...Array(5)].map((_, i) => (
                  <div key={i} className="p-4 border-b hover:bg-gray-50">
                    <div className="grid grid-cols-4 gap-4 items-center">
                      <div className="flex items-center gap-2">
                        <Skeleton className="h-4 w-4 rounded" />
                        <Skeleton className="h-4 w-24" />
                      </div>
                      <div className="flex items-center gap-2">
                        <Skeleton className="h-4 w-4 rounded" />
                        <Skeleton className="h-4 w-20" />
                      </div>
                      <div className="flex items-center gap-2">
                        <Skeleton className="h-4 w-4 rounded" />
                        <Skeleton className="h-6 w-16 rounded-full" />
                      </div>
                      <Skeleton className="h-8 w-8 rounded" />
                    </div>
                  </div>
                ))}
              </div>
            ) : filteredHistories.length === 0 ? (
              <div className="p-12 text-center text-gray-500">
                <FileText className="h-12 w-12 mx-auto mb-4 text-gray-300" />
                <p className="text-lg font-medium mb-2">No activity records found</p>
                <p className="text-sm">Try adjusting your search criteria or date filter</p>
              </div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow className="bg-gradient-to-r from-gray-50 to-gray-100 border-b border-gray-200">
                    <TableHead className="w-12"></TableHead>
                    <TableHead className="font-semibold text-gray-700 w-20">
                      <div className="flex items-center gap-2">
                        <User className="h-4 w-4 text-blue-600" />
                        Photo
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold text-gray-700">
                      <div className="flex items-center gap-2">
                        <Users className="h-4 w-4 text-blue-600" />
                        Employee
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold text-gray-700">
                      <div className="flex items-center gap-2">
                        <Calendar className="h-4 w-4 text-blue-600" />
                        Visit Date
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold text-gray-700">
                      <div className="flex items-center gap-2">
                        <Clock className="h-4 w-4 text-green-600" />
                        Start Time
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold text-gray-700">
                      <div className="flex items-center gap-2">
                        <Clock className="h-4 w-4 text-red-600" />
                        End Time
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold text-gray-700">
                      <div className="flex items-center gap-2">
                        <Store className="h-4 w-4 text-blue-600" />
                        Store Visits
                      </div>
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredHistories.map((history, index) => {
                    const isExpanded = expandedBeatHistory === history.UID;
                    const journeys = beatHistoryJourneys[history.UID] || [];
                    const isLoadingJourneys = loadingJourneys[history.UID];
                    
                    return (
                      <React.Fragment key={history.UID || `history-${index}`}>
                        <TableRow 
                          className="hover:bg-blue-50/30 transition-all duration-200 cursor-pointer border-b border-gray-100"
                          onClick={() => toggleBeatHistory(history.UID)}
                        >
                          {/* Expand Button */}
                          <TableCell className="py-3">
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0"
                              onClick={(e) => {
                                e.stopPropagation();
                                toggleBeatHistory(history.UID);
                              }}
                            >
                              {isExpanded ? (
                                <ChevronDown className="h-4 w-4" />
                              ) : (
                                <ChevronRight className="h-4 w-4" />
                              )}
                            </Button>
                          </TableCell>
                          
                          {/* Attendance Photo */}
                          <TableCell className="py-3">
                            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center overflow-hidden border-2 border-white shadow-sm">
                              {journeys.length > 0 && journeyFiles[journeys[0]?.UID]?.length > 0 ? (
                                <img 
                                  src={fileSysService.getFileUrl(journeyFiles[journeys[0].UID][0])}
                                  alt="Attendance"
                                  className="w-full h-full object-cover"
                                  onError={(e) => {
                                    const target = e.target as HTMLImageElement;
                                    target.style.display = 'none';
                                  }}
                                />
                              ) : (
                                <User className="h-5 w-5 text-gray-400" />
                              )}
                            </div>
                          </TableCell>
                          
                          {/* Employee Name */}
                          <TableCell className="py-3">
                            <div className="flex flex-col">
                              <span className="font-medium text-gray-900">{history.LoginId || 'Unknown'}</span>
                              <span className="text-xs text-gray-500">{history.JobPositionUID || 'Employee'}</span>
                            </div>
                          </TableCell>
                          
                          
                          {/* Visit Date */}
                          <TableCell className="py-3">
                            <div className="flex items-center gap-2">
                              <Calendar className="h-4 w-4 text-gray-400" />
                              <span className="font-medium">
                                {history.VisitDate ? 
                                  format(new Date(history.VisitDate), 'dd/MM/yyyy') : 
                                  'Not specified'
                                }
                              </span>
                            </div>
                          </TableCell>
                          
                          {/* Start Time */}
                          <TableCell className="py-3">
                            <div className="flex items-center gap-1">
                              <Clock className="h-3 w-3 text-green-500" />
                              <span className="text-sm font-medium">{journeys.length > 0 && journeys[0].ActualStartTime ? format(new Date(journeys[0].ActualStartTime), 'HH:mm') : (history.PlannedStartTime || '--:--')}</span>
                            </div>
                          </TableCell>
                          
                          {/* End Time */}
                          <TableCell className="py-3">
                            <div className="flex items-center gap-1">
                              <Clock className="h-3 w-3 text-red-500" />
                              <span className="text-sm font-medium">{journeys.length > 0 && journeys[0].ActualEndTime ? format(new Date(journeys[0].ActualEndTime), 'HH:mm') : (history.PlannedEndTime || '--:--')}</span>
                            </div>
                          </TableCell>

                          {/* Store Visits Count */}
                          <TableCell className="py-3">
                            <div className="flex flex-col">
                              <span className="text-lg font-bold text-gray-900">
                                {(history.PlannedStoreVisits || 0) + (history.UnPlannedStoreVisits || 0)}
                              </span>
                              <div className="flex gap-1 text-xs">
                                <span className="text-green-600 font-medium">P:{history.PlannedStoreVisits || 0}</span>
                                <span className="text-blue-600 font-medium">U:{history.UnPlannedStoreVisits || 0}</span>
                              </div>
                            </div>
                          </TableCell>
                        </TableRow>
                        
                        {/* Expanded User Journeys Section */}
                        {isExpanded && (
                          <TableRow>
                            <TableCell colSpan={7} className="p-0">
                              <div className="bg-gray-50 p-6">
                                {isLoadingJourneys ? (
                                  <div className="space-y-4">
                                    <div className="flex items-center gap-3 mb-4">
                                      <Skeleton className="h-5 w-5 rounded" />
                                      <Skeleton className="h-6 w-48" />
                                      <Skeleton className="h-4 w-20" />
                                    </div>
                                    {/* Journey Cards Skeletons */}
                                    {[1, 2].map((i) => (
                                      <div key={i} className="border border-gray-200 rounded-lg p-4 bg-white">
                                        <div className="flex items-center gap-4 mb-4">
                                          <Skeleton className="h-6 w-6 rounded" />
                                          <Skeleton className="h-4 w-4 rounded" />
                                          <Skeleton className="h-4 w-32" />
                                          <Skeleton className="h-6 w-20 rounded-full" />
                                        </div>
                                        <div className="grid grid-cols-2 gap-6">
                                          <div className="space-y-3">
                                            <div className="flex items-center gap-2">
                                              <Skeleton className="h-4 w-4 rounded" />
                                              <Skeleton className="h-4 w-24" />
                                            </div>
                                            <div className="bg-white rounded-lg p-3 border space-y-2">
                                              <div className="flex justify-between">
                                                <Skeleton className="h-3 w-16" />
                                                <Skeleton className="h-3 w-20" />
                                              </div>
                                              <div className="flex justify-between">
                                                <Skeleton className="h-3 w-14" />
                                                <Skeleton className="h-3 w-18" />
                                              </div>
                                            </div>
                                          </div>
                                          <div className="space-y-3">
                                            <div className="flex items-center gap-2">
                                              <Skeleton className="h-4 w-4 rounded" />
                                              <Skeleton className="h-4 w-28" />
                                            </div>
                                            <div className="bg-white rounded-lg p-3 border">
                                              <Skeleton className="h-12 w-full" />
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    ))}
                                  </div>
                                ) : journeys.length === 0 ? (
                                  <div className="text-center py-8 text-gray-500">
                                    <Navigation className="h-8 w-8 mx-auto mb-2 text-gray-300" />
                                    <p className="font-medium">No user journeys found</p>
                                    <p className="text-sm">No journey data available for this beat history</p>
                                  </div>
                                ) : (
                                  <div className="space-y-4">
                                    <div className="flex items-center gap-3 mb-4">
                                      <Navigation className="h-5 w-5 text-blue-600" />
                                      <h4 className="text-lg font-semibold text-gray-900">
                                        Sales Route Activity Report
                                      </h4>
                                      <span className="text-sm text-gray-500">({journeys.length} journeys)</span>
                                    </div>
                                    
                                    {/* Journey Cards */}
                                    <div className="space-y-4">
                                      {journeys.map((journey, jIndex) => {
                                        const journeyUID = journey.UID || journey.uid;
                                        const isJourneyExpanded = expandedUserJourney === journeyUID;
                                        const stores = userJourneyStores[journeyUID] || [];
                                        const isLoadingStores = loadingStores[journeyUID];
                                        
                                        return (
                                          <Card 
                                            key={journeyUID || `journey-${jIndex}`}
                                            className="border border-gray-200 hover:shadow-sm transition-shadow"
                                          >
                                            {/* Journey Header */}
                                            <div 
                                              className="p-4 cursor-pointer hover:bg-gray-50/50 transition-colors"
                                              onClick={() => toggleUserJourney(journeyUID)}
                                            >
                                              <div className="flex items-start justify-between">
                                                <div className="flex-1">
                                                  {/* User Info and Status */}
                                                  <div className="flex items-center gap-4 mb-4">
                                                    <Button
                                                      variant="ghost"
                                                      size="sm"
                                                      className="h-6 w-6 p-0"
                                                      onClick={(e) => {
                                                        e.stopPropagation();
                                                        toggleUserJourney(journeyUID);
                                                      }}
                                                    >
                                                      {isJourneyExpanded ? (
                                                        <ChevronDown className="h-4 w-4" />
                                                      ) : (
                                                        <ChevronRight className="h-4 w-4" />
                                                      )}
                                                    </Button>
                                                    
                                                    <div className="flex items-center gap-2">
                                                      <User className="h-4 w-4 text-gray-500" />
                                                      <span className="font-semibold">
                                                        {journey.LoginId || journey.loginId || 'Unknown User'}
                                                      </span>
                                                    </div>
                                                    
                                                    <span className={`px-3 py-1 rounded-md text-sm font-medium border ${
                                                      (journey.EOTStatus || journey.eot_status) === 'Completed' ? 
                                                      'bg-green-50 text-green-700 border-green-200' : 
                                                      'bg-blue-50 text-blue-700 border-blue-200'
                                                    }`}>
                                                      {journey.EOTStatus || journey.eot_status || 'Active'}
                                                    </span>
                                                    
                                                    {journey.Notes && (
                                                      <div className="ml-auto">
                                                        <span className="text-sm text-gray-500 flex items-center gap-1">
                                                          <FileText className="h-4 w-4" />
                                                          Notes Available
                                                        </span>
                                                      </div>
                                                    )}
                                                  </div>
                                                  
                                                  {/* Dashboard-Style Journey Activity Report */}
                                                  <div className="bg-white rounded-lg border border-gray-200 p-6">
                                                    {/* Main Dashboard Grid */}
                                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
                                                      
                                                      {/* Journey Status Card */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                                                        <div className="flex items-center gap-3 mb-3">
                                                          <div className="p-2 bg-blue-50 rounded-lg">
                                                            <Activity className="h-5 w-5 text-blue-600" />
                                                          </div>
                                                          <h3 className="font-medium text-gray-900">Status</h3>
                                                        </div>
                                                        <div className="text-center">
                                                          {journey.EOTStatus === 'Completed' ? (
                                                            <CheckCircle className="h-8 w-8 text-green-500 mx-auto mb-2" />
                                                          ) : (
                                                            <Clock className="h-8 w-8 text-blue-500 mx-auto mb-2" />
                                                          )}
                                                          <p className="text-lg font-semibold text-gray-900">
                                                            {journey.EOTStatus || 'Active'}
                                                          </p>
                                                        </div>
                                                      </div>

                                                      {/* Start Time Card */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                                                        <div className="flex items-center gap-3 mb-3">
                                                          <div className="p-2 bg-green-50 rounded-lg">
                                                            <PlayCircle className="h-5 w-5 text-green-600" />
                                                          </div>
                                                          <h3 className="font-medium text-gray-900">Start Time</h3>
                                                        </div>
                                                        <p className="text-xl font-bold text-gray-900">
                                                          {journey.JourneyStartTime ? 
                                                            new Date(journey.JourneyStartTime).toLocaleTimeString('en-US', { 
                                                              hour: '2-digit', 
                                                              minute: '2-digit' 
                                                            }) : '--:--'}
                                                        </p>
                                                        <p className="text-sm text-gray-500 mt-1">Journey begins</p>
                                                      </div>

                                                      {/* End Time Card */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                                                        <div className="flex items-center gap-3 mb-3">
                                                          <div className="p-2 bg-red-50 rounded-lg">
                                                            {journey.JourneyEndTime ? (
                                                              <StopCircle className="h-5 w-5 text-red-600" />
                                                            ) : (
                                                              <Timer className="h-5 w-5 text-orange-500" />
                                                            )}
                                                          </div>
                                                          <h3 className="font-medium text-gray-900">End Time</h3>
                                                        </div>
                                                        <p className="text-xl font-bold text-gray-900">
                                                          {journey.JourneyEndTime ? 
                                                            new Date(journey.JourneyEndTime).toLocaleTimeString('en-US', { 
                                                              hour: '2-digit', 
                                                              minute: '2-digit' 
                                                            }) : 'In Progress'}
                                                        </p>
                                                        <p className="text-sm text-gray-500 mt-1">
                                                          {journey.JourneyEndTime ? 'Journey completed' : 'Still ongoing'}
                                                        </p>
                                                      </div>

                                                      {/* Battery Card */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                                                        <div className="flex items-center gap-3 mb-3">
                                                          <div className="p-2 bg-yellow-50 rounded-lg">
                                                            <Battery className="h-5 w-5 text-yellow-600" />
                                                          </div>
                                                          <h3 className="font-medium text-gray-900">Battery</h3>
                                                        </div>
                                                        <p className="text-xl font-bold text-gray-900 mb-2">
                                                          {journey.BatteryPercentageAvailable || 0}%
                                                        </p>
                                                        <div className="w-full bg-gray-200 rounded-full h-2">
                                                          <div 
                                                            className={`h-2 rounded-full ${
                                                              (journey.BatteryPercentageAvailable || 0) >= (journey.BatteryPercentageTarget || 50) 
                                                                ? 'bg-green-500' : 'bg-orange-500'
                                                            }`}
                                                            style={{ width: `${journey.BatteryPercentageAvailable || 0}%` }}
                                                          ></div>
                                                        </div>
                                                        <p className="text-sm text-gray-500 mt-2">
                                                          Target: {journey.BatteryPercentageTarget || 50}%
                                                        </p>
                                                      </div>
                                                    </div>

                                                    {/* Status Grid */}
                                                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                                                      
                                                      {/* Internet Status */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 text-center shadow-sm">
                                                        <div className="flex items-center justify-center mb-2">
                                                          {journey.HasInternet ? (
                                                            <Wifi className="h-6 w-6 text-green-500" />
                                                          ) : (
                                                            <WifiOff className="h-6 w-6 text-red-500" />
                                                          )}
                                                        </div>
                                                        <p className="text-sm font-medium text-gray-900">Internet</p>
                                                        <p className={`text-xs mt-1 ${
                                                          journey.HasInternet ? 'text-green-600' : 'text-red-600'
                                                        }`}>
                                                          {journey.HasInternet ? 'Connected' : 'Disconnected'}
                                                        </p>
                                                      </div>

                                                      {/* GPS Status */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 text-center shadow-sm">
                                                        <div className="flex items-center justify-center mb-2">
                                                          {journey.IsLocationEnabled ? (
                                                            <MapPin className="h-6 w-6 text-green-500" />
                                                          ) : (
                                                            <LocateOff className="h-6 w-6 text-red-500" />
                                                          )}
                                                        </div>
                                                        <p className="text-sm font-medium text-gray-900">GPS</p>
                                                        <p className={`text-xs mt-1 ${
                                                          journey.IsLocationEnabled ? 'text-green-600' : 'text-red-600'
                                                        }`}>
                                                          {journey.IsLocationEnabled ? 'Enabled' : 'Disabled'}
                                                        </p>
                                                      </div>

                                                      {/* Mobile Network */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 text-center shadow-sm">
                                                        <div className="flex items-center justify-center mb-2">
                                                          {journey.HasMobileNetwork ? (
                                                            <Smartphone className="h-6 w-6 text-green-500" />
                                                          ) : (
                                                            <Smartphone className="h-6 w-6 text-gray-400" />
                                                          )}
                                                        </div>
                                                        <p className="text-sm font-medium text-gray-900">Mobile</p>
                                                        <p className={`text-xs mt-1 ${
                                                          journey.HasMobileNetwork ? 'text-green-600' : 'text-gray-500'
                                                        }`}>
                                                          {journey.HasMobileNetwork ? 'Available' : 'No Signal'}
                                                        </p>
                                                      </div>

                                                      {/* Sync Status */}
                                                      <div className="bg-white border border-gray-200 rounded-lg p-4 text-center shadow-sm">
                                                        <div className="flex items-center justify-center mb-2">
                                                          {journey.IsSynchronizing ? (
                                                            <RefreshCw className="h-6 w-6 text-blue-500 animate-spin" />
                                                          ) : (
                                                            <CheckCircle className="h-6 w-6 text-green-500" />
                                                          )}
                                                        </div>
                                                        <p className="text-sm font-medium text-gray-900">Sync</p>
                                                        <p className={`text-xs mt-1 ${
                                                          journey.IsSynchronizing ? 'text-blue-600' : 'text-green-600'
                                                        }`}>
                                                          {journey.IsSynchronizing ? 'Syncing' : 'Complete'}
                                                        </p>
                                                      </div>
                                                    </div>

                                                    {/* Location & Network Details */}
                                                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                                                      
                                                      {/* Network Information */}
                                                      {(journey.InternetType || journey.DownloadSpeed > 0 || journey.UploadSpeed > 0) && (
                                                        <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                                                          <div className="flex items-center gap-3 mb-4">
                                                            <div className="p-2 bg-purple-50 rounded-lg">
                                                              <Network className="h-5 w-5 text-purple-600" />
                                                            </div>
                                                            <h3 className="font-medium text-gray-900">Network Details</h3>
                                                          </div>
                                                          
                                                          {journey.InternetType && (
                                                            <div className="mb-4">
                                                              <p className="text-sm text-gray-600 mb-1">Connection Type</p>
                                                              <p className="font-semibold text-gray-900">{journey.InternetType}</p>
                                                            </div>
                                                          )}
                                                          
                                                          {(journey.DownloadSpeed > 0 || journey.UploadSpeed > 0) && (
                                                            <div className="grid grid-cols-2 gap-4">
                                                              <div className="text-center">
                                                                <Download className="h-5 w-5 text-green-600 mx-auto mb-1" />
                                                                <p className="text-lg font-bold text-gray-900">
                                                                  {journey.DownloadSpeed || 0}
                                                                </p>
                                                                <p className="text-xs text-gray-500">Mbps Down</p>
                                                              </div>
                                                              <div className="text-center">
                                                                <Upload className="h-5 w-5 text-blue-600 mx-auto mb-1" />
                                                                <p className="text-lg font-bold text-gray-900">
                                                                  {journey.UploadSpeed || 0}
                                                                </p>
                                                                <p className="text-xs text-gray-500">Mbps Up</p>
                                                              </div>
                                                            </div>
                                                          )}
                                                        </div>
                                                      )}

                                                      {/* Location Information */}
                                                      {(journey.AttendanceStatus || journey.AttendanceAddress || journey.AttendanceLatitude) && (
                                                        <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                                                          <div className="flex items-center gap-3 mb-4">
                                                            <div className="p-2 bg-red-50 rounded-lg">
                                                              <MapPin className="h-5 w-5 text-red-600" />
                                                            </div>
                                                            <h3 className="font-medium text-gray-900">Location & Attendance</h3>
                                                          </div>
                                                          
                                                          {journey.AttendanceStatus && (
                                                            <div className="mb-4">
                                                              <p className="text-sm text-gray-600 mb-2">Status</p>
                                                              <div className="flex items-center gap-2">
                                                                {journey.AttendanceStatus === 'Present' ? (
                                                                  <UserCheck className="h-4 w-4 text-green-600" />
                                                                ) : (
                                                                  <AlertCircle className="h-4 w-4 text-orange-500" />
                                                                )}
                                                                <span className={`px-2 py-1 rounded-full text-sm font-medium ${
                                                                  journey.AttendanceStatus === 'Present' 
                                                                    ? 'bg-green-100 text-green-800' 
                                                                    : 'bg-orange-100 text-orange-800'
                                                                }`}>
                                                                  {journey.AttendanceStatus}
                                                                </span>
                                                              </div>
                                                            </div>
                                                          )}
                                                          
                                                          {journey.AttendanceAddress && (
                                                            <div className="mb-4">
                                                              <p className="text-sm text-gray-600 mb-2">Address</p>
                                                              <p className="text-sm text-gray-900 leading-relaxed">
                                                                {journey.AttendanceAddress}
                                                              </p>
                                                            </div>
                                                          )}
                                                          
                                                          {journey.AttendanceLatitude && (
                                                            <div>
                                                              <p className="text-sm text-gray-600 mb-2">GPS Coordinates</p>
                                                              <div className="flex items-center gap-2 text-sm font-mono text-gray-900">
                                                                <Crosshair className="h-4 w-4 text-green-600" />
                                                                {parseFloat(journey.AttendanceLatitude).toFixed(6)}, {parseFloat(journey.AttendanceLongitude).toFixed(6)}
                                                              </div>
                                                            </div>
                                                          )}
                                                        </div>
                                                      )}
                                                    </div>

                                                    {/* Files & Photos Section - HIDDEN FOR NOW */}
                                                    {false && (
                                                      <div className="mt-6">
                                                        <div className="flex items-center gap-3 mb-4">
                                                          <div className="p-2 bg-pink-50 rounded-lg">
                                                            <FileText className="h-5 w-5 text-pink-600" />
                                                          </div>
                                                          <h3 className="font-medium text-gray-900">Attendance Photos</h3>
                                                          {loadingFiles[journeyUID] ? (
                                                            <div className="animate-spin">
                                                              <RefreshCw className="h-4 w-4 text-gray-400" />
                                                            </div>
                                                          ) : (
                                                            <span className="text-sm text-gray-500">
                                                              ({(journeyFiles[journeyUID] || []).length} photos)
                                                            </span>
                                                          )}
                                                        </div>
                                                        
                                                        {loadingFiles[journeyUID] ? (
                                                          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                                            {[1, 2, 3, 4].map((i) => (
                                                              <div key={i} className="bg-white border border-gray-200 rounded-lg p-4">
                                                                <Skeleton className="h-20 w-full mb-2" />
                                                                <Skeleton className="h-4 w-24" />
                                                              </div>
                                                            ))}
                                                          </div>
                                                        ) : (journeyFiles[journeyUID] || []).length === 0 ? (
                                                          <div className="bg-white border border-gray-200 rounded-lg p-6 text-center">
                                                            <FileText className="h-8 w-8 text-gray-300 mx-auto mb-2" />
                                                            <p className="text-sm text-gray-500">No attendance photos found for this journey</p>
                                                          </div>
                                                        ) : (
                                                          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                                                            {(journeyFiles[journeyUID] || []).map((file, fIndex) => (
                                                              <div 
                                                                key={file.UID || `file-${fIndex}`}
                                                                className="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-md transition-shadow cursor-pointer"
                                                                onClick={() => fileSysService.isImage(file.FileType) && setSelectedImage(fileSysService.getFileUrl(file))}
                                                              >
                                                                {/* File Preview */}
                                                                {fileSysService.isImage(file.FileType) ? (
                                                                  <div className="aspect-square bg-gray-100 flex items-center justify-center">
                                                                    <img 
                                                                      src={fileSysService.getFileUrl(file)}
                                                                      alt={file.DisplayName}
                                                                      className="max-w-full max-h-full object-cover"
                                                                      onError={(e) => {
                                                                        const target = e.target as HTMLImageElement;
                                                                        target.style.display = 'none';
                                                                        target.parentElement!.innerHTML = `<div class="flex items-center justify-center h-full text-gray-400"><FileText class="h-8 w-8" /></div>`;
                                                                      }}
                                                                    />
                                                                  </div>
                                                                ) : (
                                                                  <div className="aspect-square bg-gray-100 flex items-center justify-center">
                                                                    <div className="text-center">
                                                                      <span className="text-2xl">{fileSysService.getFileIcon(file.FileType)}</span>
                                                                      <p className="text-xs text-gray-500 mt-1">{file.FileType}</p>
                                                                    </div>
                                                                  </div>
                                                                )}
                                                                
                                                                {/* File Info */}
                                                                <div className="p-3">
                                                                  <div className="flex items-start justify-between mb-2">
                                                                    <h4 className="text-sm font-medium text-gray-900 truncate">
                                                                      {file.DisplayName || file.FileName}
                                                                    </h4>
                                                                    {file.LinkedItemType === 'attendance_selfie' && (
                                                                      <span className="px-1.5 py-0.5 bg-green-100 text-green-700 text-xs rounded-full flex-shrink-0 ml-2">
                                                                        Attendance
                                                                      </span>
                                                                    )}
                                                                  </div>
                                                                  
                                                                  <div className="text-xs text-gray-500 space-y-1">
                                                                    <div className="flex items-center gap-1">
                                                                      <span>Size:</span>
                                                                      <span>{fileSysService.formatFileSize(file.FileSize)}</span>
                                                                    </div>
                                                                    <div className="flex items-center gap-1">
                                                                      <span>Type:</span>
                                                                      <span className="uppercase">{file.FileSysType}</span>
                                                                    </div>
                                                                    {file.CreatedTime && (
                                                                      <div className="flex items-center gap-1">
                                                                        <span>Uploaded:</span>
                                                                        <span>{format(new Date(file.CreatedTime), 'MMM dd, HH:mm')}</span>
                                                                      </div>
                                                                    )}
                                                                  </div>
                                                                  
                                                                  {/* GPS Info for location-tagged files */}
                                                                  {file.Latitude && file.Longitude && (
                                                                    <div className="mt-2 pt-2 border-t border-gray-200">
                                                                      <div className="flex items-center gap-1 text-xs text-green-600">
                                                                        <MapPin className="h-3 w-3" />
                                                                        <span className="font-mono">
                                                                          {parseFloat(file.Latitude).toFixed(4)}, {parseFloat(file.Longitude).toFixed(4)}
                                                                        </span>
                                                                      </div>
                                                                    </div>
                                                                  )}
                                                                </div>
                                                              </div>
                                                            ))}
                                                          </div>
                                                        )}
                                                      </div>
                                                    )}
                                                  </div>
                                                  
                                                  {/* Expand Indicator */}
                                                  {!isJourneyExpanded && (
                                                    <div className="mt-4 pt-4 border-t flex items-center justify-between">
                                                      <span className="text-sm text-gray-600">
                                                        Click to view store visit details
                                                      </span>
                                                      <span className="text-sm font-medium text-blue-600 flex items-center gap-1">
                                                        View Store Visits
                                                        <ChevronRight className="h-4 w-4" />
                                                      </span>
                                                    </div>
                                                  )}
                                                </div>
                                              </div>
                                            </div>
                                            
                                            {/* Expanded Store Histories */}
                                            {isJourneyExpanded && (
                                              <div className="border-t bg-gray-50">
                                                <div className="p-4">
                                                  {isLoadingStores ? (
                                                    <div className="space-y-3">
                                                      <div className="flex items-center gap-3 mb-4">
                                                        <Skeleton className="h-5 w-5 rounded" />
                                                        <Skeleton className="h-6 w-40" />
                                                        <Skeleton className="h-4 w-16" />
                                                      </div>
                                                      {/* Store Visit Skeleton Cards */}
                                                      {[1, 2, 3].map((i) => (
                                                        <div key={i} className="p-4 bg-white border border-gray-200 rounded-lg">
                                                          <div className="flex justify-between items-start mb-3">
                                                            <div className="flex items-center gap-3">
                                                              <Skeleton className="h-4 w-20" />
                                                              <Skeleton className="h-6 w-16 rounded-full" />
                                                              <Skeleton className="h-6 w-20 rounded-full" />
                                                            </div>
                                                          </div>
                                                          <div className="grid grid-cols-4 gap-4">
                                                            <div>
                                                              <Skeleton className="h-3 w-20 mb-1" />
                                                              <Skeleton className="h-4 w-16" />
                                                            </div>
                                                            <div>
                                                              <Skeleton className="h-3 w-20 mb-1" />
                                                              <Skeleton className="h-4 w-16" />
                                                            </div>
                                                            <div>
                                                              <Skeleton className="h-3 w-16 mb-1" />
                                                              <Skeleton className="h-4 w-12" />
                                                            </div>
                                                            <div>
                                                              <Skeleton className="h-3 w-20 mb-1" />
                                                              <Skeleton className="h-4 w-16" />
                                                            </div>
                                                          </div>
                                                        </div>
                                                      ))}
                                                    </div>
                                                  ) : stores.length === 0 ? (
                                                    <div className="text-center py-8">
                                                      <Store className="h-8 w-8 mx-auto mb-3 text-gray-300" />
                                                      <p className="font-medium text-gray-900 mb-1">No Store Visits</p>
                                                      <p className="text-sm text-gray-500">No store visit records found for this journey</p>
                                                    </div>
                                                  ) : (
                                                    <div className="space-y-3">
                                                      <div className="flex items-center gap-3 mb-4">
                                                        <Store className="h-5 w-5 text-green-600" />
                                                        <h5 className="text-lg font-semibold text-gray-900">
                                                          Store Visit Details
                                                        </h5>
                                                        <span className="text-sm text-gray-500">({stores.length} visits)</span>
                                                      </div>
                                                      
                                                      {/* Debug: Show available store fields */}
                                                      {stores.length > 0 && console.log('Store data fields:', Object.keys(stores[0]))}
                                                      
                                                      {/* Store Visit Cards */}
                                                      <div className="grid gap-3">
                                                        {stores.map((store, sIndex) => (
                                                          <div 
                                                            key={store.UID || `store-${sIndex}`}
                                                            className="p-4 bg-white border border-gray-200 rounded-lg hover:shadow-sm transition-shadow"
                                                          >
                                                            <div className="flex justify-between items-start mb-3">
                                                              <div className="flex-1">
                                                                {/* Store Information Header */}
                                                                <div className="flex items-center gap-3 mb-2">
                                                                  <div className="flex items-center gap-2">
                                                                    <Store className="h-4 w-4 text-blue-600" />
                                                                    <span className="font-semibold text-gray-900">
                                                                      {store.StoreName || store.Name || store.CustomerName || `Store Visit #${sIndex + 1}`}
                                                                    </span>
                                                                  </div>
                                                                  <span className={`px-2 py-1 rounded-md text-xs font-medium border ${getStatusColor(store.Status || 'unknown')}`}>
                                                                    {store.Status || 'Unknown'}
                                                                  </span>
                                                                  {(store.IsProductive || store.isProductive) && (
                                                                    <span className="px-2 py-1 rounded-md text-xs font-medium bg-green-50 text-green-700 border border-green-200">
                                                                      Productive
                                                                    </span>
                                                                  )}
                                                                </div>
                                                                
                                                                {/* Store Details */}
                                                                <div className="flex flex-wrap items-center gap-3 text-sm text-gray-600">
                                                                  {store.StoreCode && (
                                                                    <div className="flex items-center gap-1">
                                                                      <span className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">
                                                                        Code: {store.StoreCode}
                                                                      </span>
                                                                    </div>
                                                                  )}
                                                                  {store.StoreUID && (
                                                                    <div className="flex items-center gap-1">
                                                                      <span className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded font-mono">
                                                                        Store ID: {store.StoreUID}
                                                                      </span>
                                                                    </div>
                                                                  )}
                                                                  {store.StoreAddress && (
                                                                    <div className="flex items-center gap-1">
                                                                      <MapPin className="h-3 w-3 text-gray-400" />
                                                                      <span className="text-xs truncate max-w-48">
                                                                        {store.StoreAddress}
                                                                      </span>
                                                                    </div>
                                                                  )}
                                                                  {store.City && (
                                                                    <div className="flex items-center gap-1">
                                                                      <span className="text-xs bg-green-100 text-green-800 px-2 py-1 rounded">
                                                                        📍 {store.City}
                                                                      </span>
                                                                    </div>
                                                                  )}
                                                                </div>
                                                              </div>
                                                            </div>
                                                            
                                                            {/* Store Visit Details */}
                                                            <div className="grid grid-cols-2 gap-4">
                                                              <div>
                                                                <span className="text-xs text-gray-500 block mb-1">Check-in Time</span>
                                                                <span className="text-sm font-medium">
                                                                  {(() => {
                                                                    const loginTime = store.LoginTime || store.loginTime;
                                                                    if (!loginTime || loginTime === 'Not recorded') return 'Not recorded';
                                                                    try {
                                                                      const date = new Date(loginTime);
                                                                      if (isNaN(date.getTime())) return 'Not recorded';
                                                                      return format(date, 'dd MMM yyyy, HH:mm');
                                                                    } catch {
                                                                      return 'Not recorded';
                                                                    }
                                                                  })()}
                                                                </span>
                                                              </div>
                                                              <div>
                                                                <span className="text-xs text-gray-500 block mb-1">Check-out Time</span>
                                                                <span className="text-sm font-medium">
                                                                  {(() => {
                                                                    const logoutTime = store.LogoutTime || store.logoutTime;
                                                                    if (!logoutTime || logoutTime === 'Not recorded') return 'Not recorded';
                                                                    try {
                                                                      const date = new Date(logoutTime);
                                                                      if (isNaN(date.getTime())) return 'Not recorded';
                                                                      return format(date, 'dd MMM yyyy, HH:mm');
                                                                    } catch {
                                                                      return 'Not recorded';
                                                                    }
                                                                  })()}
                                                                </span>
                                                              </div>
                                                              {/* <div>
                                                                <span className="text-xs text-gray-500 block mb-1">Visit Duration</span>
                                                                <span className="text-sm font-medium">
                                                                  {store.VisitDuration || store.visitDuration || '0'} minutes
                                                                </span>
                                                              </div> */}
                                                              {/* <div>
                                                                <span className="text-xs text-gray-500 block mb-1">Transaction Value</span>
                                                                <span className="text-sm font-semibold text-green-600">
                                                                  {store.ActualValue || store.actualValue || '0.00'}
                                                                </span>
                                                              </div> */}
                                                            </div>
                                                            
                                                            {/* Additional Store Details */}
                                                            {(store.Notes || store.VisitType || store.OrderCount || store.PaymentReceived || store.StoreContact || store.ContactNumber) && (
                                                              <div className="mt-4 pt-4 border-t grid grid-cols-2 lg:grid-cols-4 gap-3">
                                                                {store.VisitType && (
                                                                  <div>
                                                                    <span className="text-xs text-gray-500 block mb-1">Visit Type</span>
                                                                    <span className="text-sm">{store.VisitType}</span>
                                                                  </div>
                                                                )}
                                                                {store.OrderCount && (
                                                                  <div>
                                                                    <span className="text-xs text-gray-500 block mb-1">Orders Placed</span>
                                                                    <span className="text-sm">{store.OrderCount}</span>
                                                                  </div>
                                                                )}
                                                                {store.PaymentReceived && (
                                                                  <div>
                                                                    <span className="text-xs text-gray-500 block mb-1">Payment Received</span>
                                                                    <span className="text-sm">{store.PaymentReceived}</span>
                                                                  </div>
                                                                )}
                                                                {(store.StoreContact || store.ContactNumber) && (
                                                                  <div>
                                                                    <span className="text-xs text-gray-500 block mb-1">Store Contact</span>
                                                                    <div className="flex items-center gap-1">
                                                                      <Phone className="h-3 w-3 text-gray-400" />
                                                                      <span className="text-sm">{store.StoreContact || store.ContactNumber}</span>
                                                                    </div>
                                                                  </div>
                                                                )}
                                                                {store.Notes && (
                                                                  <div className="lg:col-span-4">
                                                                    <span className="text-xs text-gray-500 block mb-1">Visit Notes</span>
                                                                    <span className="text-sm">{store.Notes}</span>
                                                                  </div>
                                                                )}
                                                              </div>
                                                            )}
                                                            
                                                            {/* Store Location Details */}
                                                            {(store.Latitude || store.Longitude || store.CheckInLatitude || store.CheckInLongitude) && (
                                                              <div className="mt-4 pt-4 border-t">
                                                                <div className="flex items-center gap-2 mb-2">
                                                                  <MapPin className="h-4 w-4 text-red-600" />
                                                                  <span className="text-sm font-medium text-gray-900">Visit Location</span>
                                                                </div>
                                                                <div className="text-sm font-mono text-gray-600">
                                                                  {store.CheckInLatitude && store.CheckInLongitude ? (
                                                                    <span>Check-in: {parseFloat(store.CheckInLatitude).toFixed(6)}, {parseFloat(store.CheckInLongitude).toFixed(6)}</span>
                                                                  ) : store.Latitude && store.Longitude ? (
                                                                    <span>Location: {parseFloat(store.Latitude).toFixed(6)}, {parseFloat(store.Longitude).toFixed(6)}</span>
                                                                  ) : null}
                                                                </div>
                                                              </div>
                                                            )}
                                                          </div>
                                                        ))}
                                                      </div>
                                                    </div>
                                                  )}
                                                </div>
                                              </div>
                                            )}
                                          </Card>
                                        );
                                      })}
                                    </div>
                                  </div>
                                )}
                              </div>
                            </TableCell>
                          </TableRow>
                        )}
                      </React.Fragment>
                    );
                  })}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>

        {/* Pagination - Only show if more than 10 records */}
        {filteredHistories.length > 10 && totalPages > 1 && (
          <div className="mt-6 flex justify-between items-center">
            <div className="text-sm text-gray-600">
              Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, filteredHistories.length)} of {filteredHistories.length} entries
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                disabled={currentPage === 1}
              >
                Previous
              </Button>
              <div className="flex items-center gap-2">
                {[...Array(Math.min(5, totalPages))].map((_, i) => {
                  const pageNum = i + 1;
                  return (
                    <Button
                      key={pageNum}
                      variant={currentPage === pageNum ? "default" : "outline"}
                      onClick={() => setCurrentPage(pageNum)}
                      className="w-10"
                    >
                      {pageNum}
                    </Button>
                  );
                })}
                {totalPages > 5 && <span className="text-gray-500">...</span>}
              </div>
              <Button
                variant="outline"
                onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
                disabled={currentPage === totalPages}
              >
                Next
              </Button>
            </div>
          </div>
        )}

        {/* Image Modal for full-size viewing */}
        {selectedImage && (
          <div 
            className="fixed inset-0 bg-black bg-opacity-75 z-50 flex items-center justify-center p-4"
            onClick={() => setSelectedImage(null)}
          >
            <div className="max-w-4xl max-h-full">
              <div className="relative">
                <img 
                  src={selectedImage}
                  alt="Full size view"
                  className="max-w-full max-h-[90vh] object-contain"
                />
                <button
                  onClick={() => setSelectedImage(null)}
                  className="absolute top-4 right-4 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-75 transition-opacity"
                >
                  <X className="h-6 w-6" />
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}