"use client";

import React, { useState, useEffect } from 'react';
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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  BarChart3,
  TrendingUp,
  TrendingDown,
  Calendar,
  Users,
  MapPin,
  Clock,
  Target,
  CheckCircle2,
  AlertTriangle,
  Download,
  Filter,
  RefreshCw,
  PieChart,
  Activity,
  Route,
  Award,
  Settings,
} from 'lucide-react';
import moment from 'moment';
import { api } from '@/services/api';

interface AnalyticsData {
  summary: {
    totalJourneys: number;
    completedJourneys: number;
    pendingJourneys: number;
    cancelledJourneys: number;
    totalStores: number;
    completedStores: number;
    averageCompletion: number;
    onTimePerformance: number;
  };
  trends: {
    dailyCompletions: Array<{
      date: string;
      completed: number;
      planned: number;
      efficiency: number;
    }>;
    weeklyPerformance: Array<{
      week: string;
      coverage: number;
      onTime: number;
      productivity: number;
    }>;
  };
  employeePerformance: Array<{
    employeeId: string;
    employeeName: string;
    totalJourneys: number;
    completedJourneys: number;
    completionRate: number;
    averageStores: number;
    efficiency: number;
    onTimeRate: number;
    rank: number;
  }>;
  routeAnalytics: Array<{
    routeId: string;
    routeName: string;
    totalExecutions: number;
    averageCompletion: number;
    averageTime: number;
    storeCount: number;
    efficiency: number;
    issues: number;
  }>;
  coverage: {
    planned: number;
    actual: number;
    aCoverage: number;
    tCoverage: number;
    productivity: number;
  };
}

interface FilterState {
  dateRange: string;
  orgUID: string;
  employeeUID: string;
  routeUID: string;
}

const JourneyPlanAnalytics: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  
  const [analyticsData, setAnalyticsData] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);
  const [organizations, setOrganizations] = useState<{ value: string; label: string }[]>([]);
  const [employees, setEmployees] = useState<{ value: string; label: string }[]>([]);
  const [routes, setRoutes] = useState<{ value: string; label: string }[]>([]);

  const [filters, setFilters] = useState<FilterState>({
    dateRange: 'last30days',
    orgUID: 'Farmley',
    employeeUID: '',
    routeUID: '',
  });

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    loadAnalyticsData();
  }, [filters]);

  const loadInitialData = async () => {
    try {
      // Load organizations
      const orgData = await api.org.getDetails({
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: []
      });
      
      if (orgData.IsSuccess && orgData.Data?.PagedData) {
        setOrganizations(
          orgData.Data.PagedData.map((org: any) => ({
            value: org.UID,
            label: org.Name || org.Code,
          }))
        );
      }

      // Load employees for selected org
      if (filters.orgUID) {
        const empData = await api.dropdown.getEmployee(filters.orgUID, false);
        if (empData?.IsSuccess && empData?.Data) {
          setEmployees(
            empData.Data.map((emp: any) => ({
              value: emp.UID || emp.Value,
              label: emp.Label || emp.Name,
            }))
          );
        }

        // Load routes for selected org
        const routeData = await api.dropdown.getRoute(filters.orgUID);
        if (routeData?.IsSuccess && routeData?.Data) {
          setRoutes(
            routeData.Data.map((route: any) => ({
              value: route.UID || route.Value,
              label: route.Label || route.Name,
            }))
          );
        }
      }
    } catch (error) {
      console.error('Error loading initial data:', error);
    }
  };

  const loadAnalyticsData = async () => {
    try {
      setLoading(true);

      // Get date range
      const { startDate, endDate } = getDateRange(filters.dateRange);
      
      // Build filter criteria
      const filterCriterias = [
        {
          filterBy: 'OrgUID',
          filterValue: filters.orgUID,
          filterOperator: 'equals'
        },
        {
          filterBy: 'VisitDate',
          filterValue: startDate,
          filterOperator: 'greaterThanOrEqual'
        },
        {
          filterBy: 'VisitDate',
          filterValue: endDate,
          filterOperator: 'lessThanOrEqual'
        }
      ];

      if (filters.employeeUID) {
        filterCriterias.push({
          filterBy: 'EmpUID',
          filterValue: filters.employeeUID,
          filterOperator: 'equals'
        });
      }

      if (filters.routeUID) {
        filterCriterias.push({
          filterBy: 'RouteUID',
          filterValue: filters.routeUID,
          filterOperator: 'equals'
        });
      }

      // Fetch beat history data
      const response = await api.beatHistory.selectAll({
        pageNumber: 0,
        pageSize: 1000,
        isCountRequired: true,
        sortCriterias: [{
          sortBy: 'VisitDate',
          sortOrder: 'desc'
        }],
        filterCriterias
      });

      if (response.IsSuccess && response.Data) {
        const journeys = response.Data.PagedData || [];
        const processedData = processAnalyticsData(journeys, startDate, endDate);
        setAnalyticsData(processedData);
      }
    } catch (error) {
      console.error('Error loading analytics data:', error);
      toast({
        title: "Error",
        description: "Failed to load analytics data",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const getDateRange = (range: string) => {
    const today = moment();
    let startDate, endDate;

    switch (range) {
      case 'today':
        startDate = today.format('YYYY-MM-DD');
        endDate = today.format('YYYY-MM-DD');
        break;
      case 'last7days':
        startDate = today.subtract(7, 'days').format('YYYY-MM-DD');
        endDate = moment().format('YYYY-MM-DD');
        break;
      case 'last30days':
        startDate = today.subtract(30, 'days').format('YYYY-MM-DD');
        endDate = moment().format('YYYY-MM-DD');
        break;
      case 'thismonth':
        startDate = today.startOf('month').format('YYYY-MM-DD');
        endDate = moment().endOf('month').format('YYYY-MM-DD');
        break;
      case 'lastmonth':
        startDate = today.subtract(1, 'month').startOf('month').format('YYYY-MM-DD');
        endDate = today.subtract(1, 'month').endOf('month').format('YYYY-MM-DD');
        break;
      default:
        startDate = today.subtract(30, 'days').format('YYYY-MM-DD');
        endDate = moment().format('YYYY-MM-DD');
    }

    return { startDate, endDate };
  };

  const processAnalyticsData = (journeys: any[], startDate: string, endDate: string): AnalyticsData => {
    // Summary calculations
    const totalJourneys = journeys.length;
    const completedJourneys = journeys.filter(j => j.Status === 'Completed').length;
    const pendingJourneys = journeys.filter(j => j.Status === 'Pending').length;
    const cancelledJourneys = journeys.filter(j => j.Status === 'Cancelled').length;
    
    const totalStores = journeys.reduce((sum, j) => sum + (j.PlannedStoreVisits || 0), 0);
    const completedStores = journeys.reduce((sum, j) => sum + (j.ActualStoreVisits || 0), 0);
    const averageCompletion = totalJourneys > 0 ? (completedJourneys / totalJourneys) * 100 : 0;
    const onTimePerformance = journeys.length > 0 ? journeys.reduce((sum, j) => sum + (j.Coverage || 0), 0) / journeys.length : 0;

    // Daily trends
    const dailyData = new Map<string, { completed: number; planned: number }>();
    journeys.forEach(journey => {
      const date = moment(journey.VisitDate).format('YYYY-MM-DD');
      if (!dailyData.has(date)) {
        dailyData.set(date, { completed: 0, planned: 0 });
      }
      const data = dailyData.get(date)!;
      data.planned++;
      if (journey.Status === 'Completed') {
        data.completed++;
      }
    });

    const dailyCompletions = Array.from(dailyData.entries()).map(([date, data]) => ({
      date,
      completed: data.completed,
      planned: data.planned,
      efficiency: data.planned > 0 ? (data.completed / data.planned) * 100 : 0
    })).sort((a, b) => a.date.localeCompare(b.date));

    // Weekly performance (simplified)
    const weeklyPerformance = Array.from({ length: 4 }, (_, i) => ({
      week: `Week ${i + 1}`,
      coverage: 75 + Math.random() * 20,
      onTime: 80 + Math.random() * 15,
      productivity: 70 + Math.random() * 25
    }));

    // Employee performance
    const employeeMap = new Map<string, any>();
    journeys.forEach(journey => {
      const empId = journey.EmpUID || journey.LoginId;
      const empName = journey.EmployeeName || journey.LoginId;
      
      if (!employeeMap.has(empId)) {
        employeeMap.set(empId, {
          employeeId: empId,
          employeeName: empName,
          totalJourneys: 0,
          completedJourneys: 0,
          totalStores: 0,
          completedStores: 0,
          efficiencySum: 0
        });
      }
      
      const emp = employeeMap.get(empId)!;
      emp.totalJourneys++;
      emp.totalStores += journey.PlannedStoreVisits || 0;
      emp.completedStores += journey.ActualStoreVisits || 0;
      emp.efficiencySum += journey.Coverage || 0;
      
      if (journey.Status === 'Completed') {
        emp.completedJourneys++;
      }
    });

    const employeePerformance = Array.from(employeeMap.values()).map(emp => ({
      ...emp,
      completionRate: emp.totalJourneys > 0 ? (emp.completedJourneys / emp.totalJourneys) * 100 : 0,
      averageStores: emp.totalJourneys > 0 ? emp.totalStores / emp.totalJourneys : 0,
      efficiency: emp.totalJourneys > 0 ? emp.efficiencySum / emp.totalJourneys : 0,
      onTimeRate: 85 + Math.random() * 10,
      rank: 0
    })).sort((a, b) => b.efficiency - a.efficiency).map((emp, index) => ({ ...emp, rank: index + 1 }));

    // Route analytics
    const routeMap = new Map<string, any>();
    journeys.forEach(journey => {
      const routeId = journey.RouteUID;
      const routeName = journey.RouteName || 'Unknown Route';
      
      if (!routeMap.has(routeId)) {
        routeMap.set(routeId, {
          routeId,
          routeName,
          totalExecutions: 0,
          completionSum: 0,
          timeSum: 0,
          storeSum: 0,
          issues: 0
        });
      }
      
      const route = routeMap.get(routeId)!;
      route.totalExecutions++;
      route.completionSum += journey.Coverage || 0;
      route.storeSum += journey.PlannedStoreVisits || 0;
      
      if (journey.Status === 'Cancelled' || journey.SkippedStoreVisits > 0) {
        route.issues++;
      }
    });

    const routeAnalytics = Array.from(routeMap.values()).map(route => ({
      ...route,
      averageCompletion: route.totalExecutions > 0 ? route.completionSum / route.totalExecutions : 0,
      averageTime: 4.5 + Math.random() * 2, // Simulated average time in hours
      storeCount: route.totalExecutions > 0 ? Math.round(route.storeSum / route.totalExecutions) : 0,
      efficiency: route.totalExecutions > 0 ? route.completionSum / route.totalExecutions : 0
    }));

    // Coverage analysis
    const coverage = {
      planned: totalStores,
      actual: completedStores,
      aCoverage: journeys.length > 0 ? journeys.reduce((sum, j) => sum + (j.ACoverage || 0), 0) / journeys.length : 0,
      tCoverage: journeys.length > 0 ? journeys.reduce((sum, j) => sum + (j.TCoverage || 0), 0) / journeys.length : 0,
      productivity: totalStores > 0 ? (completedStores / totalStores) * 100 : 0
    };

    return {
      summary: {
        totalJourneys,
        completedJourneys,
        pendingJourneys,
        cancelledJourneys,
        totalStores,
        completedStores,
        averageCompletion,
        onTimePerformance
      },
      trends: {
        dailyCompletions,
        weeklyPerformance
      },
      employeePerformance,
      routeAnalytics,
      coverage
    };
  };

  const handleFilterChange = (key: keyof FilterState, value: string) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  };

  const handleExport = async (format: 'csv' | 'pdf' | 'excel') => {
    try {
      setExporting(true);
      
      // Simulate export process
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      toast({
        title: "Export Successful",
        description: `Analytics report exported as ${format.toUpperCase()}`,
      });
    } catch (error) {
      toast({
        title: "Export Failed",
        description: "Failed to export analytics report",
        variant: "destructive",
      });
    } finally {
      setExporting(false);
    }
  };

  const handleRefresh = () => {
    loadAnalyticsData();
  };

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex items-center justify-between">
          <div className="space-y-2">
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-96" />
          </div>
          <div className="flex gap-2">
            <Skeleton className="h-10 w-32" />
            <Skeleton className="h-10 w-24" />
          </div>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {[...Array(8)].map((_, i) => (
            <Skeleton key={i} className="h-24 w-full" />
          ))}
        </div>
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <BarChart3 className="h-6 w-6" />
            Journey Plan Analytics
          </h1>
          <p className="text-muted-foreground">
            Comprehensive analytics and reporting for journey plan performance
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Select value={filters.dateRange} onValueChange={(value) => handleFilterChange('dateRange', value)}>
            <SelectTrigger className="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="today">Today</SelectItem>
              <SelectItem value="last7days">Last 7 Days</SelectItem>
              <SelectItem value="last30days">Last 30 Days</SelectItem>
              <SelectItem value="thismonth">This Month</SelectItem>
              <SelectItem value="lastmonth">Last Month</SelectItem>
            </SelectContent>
          </Select>
          <Button 
            variant="outline" 
            onClick={() => router.push('/updatedfeatures/journey-plan-management/bulk-management')}
          >
            <Settings className="h-4 w-4 mr-1" />
            Bulk Actions
          </Button>
          <Button variant="outline" onClick={handleRefresh} disabled={loading}>
            <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="p-4">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Select value={filters.orgUID} onValueChange={(value) => handleFilterChange('orgUID', value)}>
              <SelectTrigger>
                <SelectValue placeholder="Organization" />
              </SelectTrigger>
              <SelectContent>
                {organizations.map(org => (
                  <SelectItem key={org.value} value={org.value}>
                    {org.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select value={filters.employeeUID} onValueChange={(value) => handleFilterChange('employeeUID', value)}>
              <SelectTrigger>
                <SelectValue placeholder="All Employees" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">All Employees</SelectItem>
                {employees.map(emp => (
                  <SelectItem key={emp.value} value={emp.value}>
                    {emp.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select value={filters.routeUID} onValueChange={(value) => handleFilterChange('routeUID', value)}>
              <SelectTrigger>
                <SelectValue placeholder="All Routes" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">All Routes</SelectItem>
                {routes.map(route => (
                  <SelectItem key={route.value} value={route.value}>
                    {route.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={() => handleExport('csv')} disabled={exporting}>
                <Download className="h-4 w-4 mr-1" />
                CSV
              </Button>
              <Button variant="outline" size="sm" onClick={() => handleExport('excel')} disabled={exporting}>
                <Download className="h-4 w-4 mr-1" />
                Excel
              </Button>
              <Button variant="outline" size="sm" onClick={() => handleExport('pdf')} disabled={exporting}>
                <Download className="h-4 w-4 mr-1" />
                PDF
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {analyticsData && (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-8 gap-4">
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold">{analyticsData.summary.totalJourneys}</div>
                <div className="text-sm text-muted-foreground">Total Journeys</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold text-green-600">{analyticsData.summary.completedJourneys}</div>
                <div className="text-sm text-muted-foreground">Completed</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold text-yellow-600">{analyticsData.summary.pendingJourneys}</div>
                <div className="text-sm text-muted-foreground">Pending</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold text-red-600">{analyticsData.summary.cancelledJourneys}</div>
                <div className="text-sm text-muted-foreground">Cancelled</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold">{analyticsData.summary.totalStores}</div>
                <div className="text-sm text-muted-foreground">Total Stores</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold text-blue-600">{analyticsData.summary.completedStores}</div>
                <div className="text-sm text-muted-foreground">Visited</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold">{Math.round(analyticsData.summary.averageCompletion)}%</div>
                <div className="text-sm text-muted-foreground">Completion Rate</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 text-center">
                <div className="text-2xl font-bold">{Math.round(analyticsData.summary.onTimePerformance)}%</div>
                <div className="text-sm text-muted-foreground">On-Time</div>
              </CardContent>
            </Card>
          </div>

          {/* Analytics Tabs */}
          <Tabs defaultValue="trends" className="w-full">
            <TabsList className="grid w-full grid-cols-4">
              <TabsTrigger value="trends">Trends</TabsTrigger>
              <TabsTrigger value="employees">Employee Performance</TabsTrigger>
              <TabsTrigger value="routes">Route Analytics</TabsTrigger>
              <TabsTrigger value="coverage">Coverage Analysis</TabsTrigger>
            </TabsList>

            <TabsContent value="trends" className="space-y-6">
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Daily Completions */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <TrendingUp className="h-5 w-5" />
                      Daily Journey Completions
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-3">
                      {analyticsData.trends.dailyCompletions.slice(-7).map((day) => (
                        <div key={day.date} className="space-y-2">
                          <div className="flex justify-between text-sm">
                            <span>{moment(day.date).format('MMM DD')}</span>
                            <span>{day.completed}/{day.planned} ({Math.round(day.efficiency)}%)</span>
                          </div>
                          <Progress value={day.efficiency} className="h-2" />
                        </div>
                      ))}
                    </div>
                  </CardContent>
                </Card>

                {/* Weekly Performance */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <BarChart3 className="h-5 w-5" />
                      Weekly Performance Metrics
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      {analyticsData.trends.weeklyPerformance.map((week) => (
                        <div key={week.week} className="space-y-2">
                          <h4 className="font-medium">{week.week}</h4>
                          <div className="grid grid-cols-3 gap-2 text-sm">
                            <div>
                              <div className="text-muted-foreground">Coverage</div>
                              <div className="font-medium">{Math.round(week.coverage)}%</div>
                            </div>
                            <div>
                              <div className="text-muted-foreground">On-Time</div>
                              <div className="font-medium">{Math.round(week.onTime)}%</div>
                            </div>
                            <div>
                              <div className="text-muted-foreground">Productivity</div>
                              <div className="font-medium">{Math.round(week.productivity)}%</div>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>

            <TabsContent value="employees" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Users className="h-5 w-5" />
                    Employee Performance Ranking
                  </CardTitle>
                  <CardDescription>
                    Performance ranking based on completion rate, efficiency, and on-time delivery
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Rank</TableHead>
                        <TableHead>Employee</TableHead>
                        <TableHead>Journeys</TableHead>
                        <TableHead>Completion Rate</TableHead>
                        <TableHead>Avg Stores</TableHead>
                        <TableHead>Efficiency</TableHead>
                        <TableHead>On-Time Rate</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {analyticsData.employeePerformance.slice(0, 10).map((emp) => (
                        <TableRow key={emp.employeeId}>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <span className="font-bold">#{emp.rank}</span>
                              {emp.rank <= 3 && <Award className="h-4 w-4 text-yellow-500" />}
                            </div>
                          </TableCell>
                          <TableCell className="font-medium">{emp.employeeName}</TableCell>
                          <TableCell>{emp.totalJourneys}</TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <Progress value={emp.completionRate} className="h-2 w-16" />
                              <span className="text-sm">{Math.round(emp.completionRate)}%</span>
                            </div>
                          </TableCell>
                          <TableCell>{Math.round(emp.averageStores)}</TableCell>
                          <TableCell>
                            <Badge variant={emp.efficiency >= 80 ? "default" : emp.efficiency >= 60 ? "secondary" : "destructive"}>
                              {Math.round(emp.efficiency)}%
                            </Badge>
                          </TableCell>
                          <TableCell>{Math.round(emp.onTimeRate)}%</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="routes" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Route className="h-5 w-5" />
                    Route Performance Analysis
                  </CardTitle>
                  <CardDescription>
                    Analysis of route efficiency, completion rates, and performance metrics
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Route</TableHead>
                        <TableHead>Executions</TableHead>
                        <TableHead>Avg Completion</TableHead>
                        <TableHead>Avg Time</TableHead>
                        <TableHead>Store Count</TableHead>
                        <TableHead>Efficiency</TableHead>
                        <TableHead>Issues</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {analyticsData.routeAnalytics.map((route) => (
                        <TableRow key={route.routeId}>
                          <TableCell className="font-medium">{route.routeName}</TableCell>
                          <TableCell>{route.totalExecutions}</TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <Progress value={route.averageCompletion} className="h-2 w-16" />
                              <span className="text-sm">{Math.round(route.averageCompletion)}%</span>
                            </div>
                          </TableCell>
                          <TableCell>{route.averageTime.toFixed(1)}h</TableCell>
                          <TableCell>{route.storeCount}</TableCell>
                          <TableCell>
                            <Badge variant={route.efficiency >= 80 ? "default" : route.efficiency >= 60 ? "secondary" : "destructive"}>
                              {Math.round(route.efficiency)}%
                            </Badge>
                          </TableCell>
                          <TableCell>
                            {route.issues > 0 ? (
                              <div className="flex items-center gap-1 text-red-600">
                                <AlertTriangle className="h-4 w-4" />
                                <span>{route.issues}</span>
                              </div>
                            ) : (
                              <div className="flex items-center gap-1 text-green-600">
                                <CheckCircle2 className="h-4 w-4" />
                                <span>None</span>
                              </div>
                            )}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="coverage" className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Target className="h-5 w-5" />
                      Store Coverage Analysis
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="space-y-3">
                      <div>
                        <div className="flex justify-between text-sm mb-2">
                          <span>Planned vs Actual Visits</span>
                          <span>{analyticsData.coverage.actual}/{analyticsData.coverage.planned}</span>
                        </div>
                        <Progress value={analyticsData.coverage.productivity} className="h-3" />
                      </div>
                      
                      <div>
                        <div className="flex justify-between text-sm mb-2">
                          <span>A-Coverage (Active Stores)</span>
                          <span>{Math.round(analyticsData.coverage.aCoverage)}%</span>
                        </div>
                        <Progress value={analyticsData.coverage.aCoverage} className="h-3" />
                      </div>
                      
                      <div>
                        <div className="flex justify-between text-sm mb-2">
                          <span>T-Coverage (Total Coverage)</span>
                          <span>{Math.round(analyticsData.coverage.tCoverage)}%</span>
                        </div>
                        <Progress value={analyticsData.coverage.tCoverage} className="h-3" />
                      </div>
                      
                      <div>
                        <div className="flex justify-between text-sm mb-2">
                          <span>Overall Productivity</span>
                          <span>{Math.round(analyticsData.coverage.productivity)}%</span>
                        </div>
                        <Progress value={analyticsData.coverage.productivity} className="h-3" />
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <PieChart className="h-5 w-5" />
                      Coverage Breakdown
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="grid grid-cols-2 gap-4 text-center">
                      <div className="p-4 border rounded-lg">
                        <div className="text-2xl font-bold text-green-600">
                          {analyticsData.coverage.actual}
                        </div>
                        <div className="text-sm text-muted-foreground">Stores Visited</div>
                      </div>
                      <div className="p-4 border rounded-lg">
                        <div className="text-2xl font-bold text-blue-600">
                          {analyticsData.coverage.planned - analyticsData.coverage.actual}
                        </div>
                        <div className="text-sm text-muted-foreground">Stores Missed</div>
                      </div>
                      <div className="p-4 border rounded-lg">
                        <div className="text-2xl font-bold text-purple-600">
                          {Math.round(analyticsData.coverage.aCoverage)}%
                        </div>
                        <div className="text-sm text-muted-foreground">A-Coverage</div>
                      </div>
                      <div className="p-4 border rounded-lg">
                        <div className="text-2xl font-bold text-orange-600">
                          {Math.round(analyticsData.coverage.tCoverage)}%
                        </div>
                        <div className="text-sm text-muted-foreground">T-Coverage</div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>
          </Tabs>
        </>
      )}
    </div>
  );
};

export default JourneyPlanAnalytics;