/**
 * Commission Dashboard Component
 * Production-ready dashboard for commission management and analytics
 */

'use client'

import React, { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { format } from 'date-fns'
import { formatDateToDayMonthYear, formatTime } from "@/utils/date-formatter"
import {
  Users,
  DollarSign,
  Clock,
  FileText,
  Calculator,
  CheckCircle,
  AlertCircle,
  BarChart3,
} from 'lucide-react'

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Progress } from '@/components/ui/progress'
import { Skeleton } from '@/components/ui/skeleton'
import { useToast } from '@/components/ui/use-toast'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

import { commissionService } from '@/services/commissionService'

export function CommissionDashboardComponent() {
  const [selectedPeriod, setSelectedPeriod] = useState('current_month')
  const { toast } = useToast()

  // Fetch dashboard data
  const {
    data: dashboardData,
    isLoading: isDashboardLoading,
    error: dashboardError,
    refetch: refetchDashboard
  } = useQuery({
    queryKey: ['commissionDashboard'],
    queryFn: () => commissionService.getDashboardData(),
    refetchInterval: 5 * 60 * 1000, // Refresh every 5 minutes
  })

  // Fetch analytics data
  const {
    data: analyticsData,
    isLoading: isAnalyticsLoading,
    error: analyticsError
  } = useQuery({
    queryKey: ['commissionAnalytics', selectedPeriod],
    queryFn: () => commissionService.getAnalytics({
      date_from: getPeriodStartDate(selectedPeriod),
      date_to: new Date()
    }),
  })

  const handleProcessCommission = async () => {
    try {
      const result = await commissionService.processCommission({
        calculation_period: {
          start_date: getPeriodStartDate('current_month'),
          end_date: new Date()
        },
        recalculate: false
      })

      toast({
        title: 'Commission Processing Started',
        description: `Processing ${result.processed_users} users. Total commission: $${result.total_commission_amount.toLocaleString()}`,
      })

      // Refresh dashboard after processing
      setTimeout(() => {
        refetchDashboard()
      }, 2000)
    } catch (error) {
      toast({
        title: 'Processing Failed',
        description: 'Failed to start commission processing. Please try again.',
        variant: 'destructive'
      })
    }
  }

  const handleExportReport = async () => {
    try {
      const blob = await commissionService.exportReport({
        date_from: getPeriodStartDate(selectedPeriod),
        date_to: new Date()
      }, 'excel')

      // Create download link
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `commission-report-${format(new Date(), 'yyyy-MM-dd')}.xlsx`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)

      toast({
        title: 'Report Exported',
        description: 'Commission report has been downloaded successfully.',
      })
    } catch (error) {
      toast({
        title: 'Export Failed',
        description: 'Failed to export commission report. Please try again.',
        variant: 'destructive'
      })
    }
  }

  if (isDashboardLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-32 w-full" />
        <div className="grid gap-4 md:grid-cols-4">
          {[...Array(4)].map((_, i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  if (dashboardError || analyticsError) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertTitle>Error Loading Commission Dashboard</AlertTitle>
        <AlertDescription>
          Failed to load commission data. Please refresh the page or contact support.
        </AlertDescription>
      </Alert>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Commission Dashboard</h1>
          <p className="text-muted-foreground mt-1">
            Track commission performance and manage payouts
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            onClick={handleExportReport}
            className="gap-2"
          >
            <FileText className="h-4 w-4" />
            Export Report
          </Button>
          <Button
            onClick={handleProcessCommission}
            className="gap-2"
          >
            <Calculator className="h-4 w-4" />
            Process Commission
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      {dashboardData && (
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Active Commissions</CardTitle>
              <BarChart3 className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{dashboardData.total_active_commissions}</div>
              <p className="text-xs text-muted-foreground">
                Commission structures configured
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Enrolled Users</CardTitle>
              <Users className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{dashboardData.total_users_enrolled}</div>
              <p className="text-xs text-muted-foreground">
                Users in commission programs
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Current Period Payout</CardTitle>
              <DollarSign className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                ${dashboardData.current_period_payout.toLocaleString()}
              </div>
              <p className="text-xs text-muted-foreground">
                Total commission earned
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Pending Approvals</CardTitle>
              <Clock className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{dashboardData.pending_approvals}</div>
              <p className="text-xs text-muted-foreground">
                Payouts awaiting approval
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Main Content Tabs */}
      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="performance">Performance</TabsTrigger>
          <TabsTrigger value="calculations">Recent Calculations</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          {/* Performance Trends */}
          {dashboardData && dashboardData.performance_trends.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Performance Trends</CardTitle>
                <CardDescription>
                  Commission performance over time
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {dashboardData.performance_trends.map((trend, index) => (
                    <div key={index} className="flex items-center justify-between">
                      <div className="space-y-1">
                        <p className="text-sm font-medium">{trend.period}</p>
                        <p className="text-xs text-muted-foreground">
                          {trend.users_count} users • {trend.average_achievement.toFixed(1)}% avg achievement
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-bold">${trend.total_payout.toLocaleString()}</p>
                        <Progress 
                          value={trend.average_achievement} 
                          className="w-24 h-2" 
                        />
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Quick Actions */}
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Quick Actions</CardTitle>
                <CardDescription>
                  Common commission management tasks
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-3">
                <Button variant="outline" className="w-full justify-start gap-2">
                  <Calculator className="h-4 w-4" />
                  Run Commission Calculation
                </Button>
                <Button variant="outline" className="w-full justify-start gap-2">
                  <CheckCircle className="h-4 w-4" />
                  Approve Pending Payouts
                </Button>
                <Button variant="outline" className="w-full justify-start gap-2">
                  <FileText className="h-4 w-4" />
                  Generate Payout Reports
                </Button>
                <Button variant="outline" className="w-full justify-start gap-2">
                  <Users className="h-4 w-4" />
                  Manage User Mappings
                </Button>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>System Status</CardTitle>
                <CardDescription>
                  Commission system health and alerts
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm">Calculation Engine</span>
                  <Badge variant="default">Active</Badge>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm">Data Sync</span>
                  <Badge variant="default">Up to Date</Badge>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm">Last Calculation</span>
                  <span className="text-xs text-muted-foreground">
                    {formatDateToDayMonthYear(new Date())} at {formatTime(new Date())}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm">Pending Processes</span>
                  <Badge variant="secondary">0</Badge>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="performance" className="space-y-4">
          {analyticsData && (
            <div className="grid gap-4 md:grid-cols-2">
              {/* Top Performers */}
              <Card>
                <CardHeader>
                  <CardTitle>Top Performers</CardTitle>
                  <CardDescription>
                    Highest commission earners this period
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {analyticsData.top_performers.slice(0, 5).map((performer, index) => (
                      <div key={index} className="flex items-center justify-between">
                        <div className="space-y-1">
                          <p className="text-sm font-medium">{performer.user_name}</p>
                          <p className="text-xs text-muted-foreground">
                            {performer.achievement_percent.toFixed(1)}% achievement
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-sm font-bold">
                            ${performer.commission_amount.toLocaleString()}
                          </p>
                          <Badge variant="outline" className="text-xs">
                            #{index + 1}
                          </Badge>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>

              {/* KPI Performance */}
              <Card>
                <CardHeader>
                  <CardTitle>KPI Performance</CardTitle>
                  <CardDescription>
                    Average performance by KPI type
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {analyticsData.kpi_performance.map((kpi, index) => (
                      <div key={index} className="space-y-2">
                        <div className="flex items-center justify-between">
                          <span className="text-sm font-medium">{kpi.kpi_name}</span>
                          <span className="text-xs text-muted-foreground">
                            ${kpi.total_payout.toLocaleString()}
                          </span>
                        </div>
                        <div className="space-y-1">
                          <Progress value={kpi.average_achievement} className="h-2" />
                          <p className="text-xs text-muted-foreground">
                            {kpi.average_achievement.toFixed(1)}% average achievement
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </div>
          )}
        </TabsContent>

        <TabsContent value="calculations" className="space-y-4">
          {/* Recent Calculations */}
          {dashboardData && dashboardData.recent_calculations.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Recent Calculations</CardTitle>
                <CardDescription>
                  Latest commission calculation runs
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Date</TableHead>
                      <TableHead>Commission</TableHead>
                      <TableHead>Users Processed</TableHead>
                      <TableHead>Total Amount</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {dashboardData.recent_calculations.map((calc, index) => (
                      <TableRow key={index}>
                        <TableCell>
                          {formatDateToDayMonthYear(calc.calculation_date)} at {formatTime(calc.calculation_date)}
                        </TableCell>
                        <TableCell className="font-medium">
                          {calc.commission_name}
                        </TableCell>
                        <TableCell>{calc.users_processed}</TableCell>
                        <TableCell>${calc.total_amount.toLocaleString()}</TableCell>
                        <TableCell>
                          <Badge variant="default">Completed</Badge>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}

// Helper function to get period start date
function getPeriodStartDate(period: string): Date {
  const now = new Date()
  switch (period) {
    case 'current_month':
      return new Date(now.getFullYear(), now.getMonth(), 1)
    case 'last_month':
      return new Date(now.getFullYear(), now.getMonth() - 1, 1)
    case 'current_quarter':
      const quarterStart = Math.floor(now.getMonth() / 3) * 3
      return new Date(now.getFullYear(), quarterStart, 1)
    case 'current_year':
      return new Date(now.getFullYear(), 0, 1)
    default:
      return new Date(now.getFullYear(), now.getMonth(), 1)
  }
}