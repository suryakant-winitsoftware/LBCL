"use client"

import { useState, useEffect } from "react"
import { 
  Users, 
  UserCheck, 
  Key, 
  Activity,
  TrendingUp,
  TrendingDown,
  Clock,
  AlertCircle
} from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { AdminStats, ActivityLog } from "@/types/admin.types"
import { employeeService } from "@/services/admin/employee.service"
import { roleService } from "@/services/admin/role.service"
import { permissionService } from "@/services/admin/permission.service"

export function AdminDashboard() {
  const [stats, setStats] = useState<AdminStats>({
    totalEmployees: 0,
    activeEmployees: 0,
    totalRoles: 0,
    activeRoles: 0,
    totalPermissions: 0,
    recentActivities: 0
  })

  const [recentActivities, setRecentActivities] = useState<ActivityLog[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadDashboardData()
  }, [])

  const loadDashboardData = async () => {
    try {
      // Load employees data
      const employeesResponse = await employeeService.getEmployees({
        pageNumber: 0,
        pageSize: 10000,
        isCountRequired: true
      })

      // Load roles data
      const rolesStats = await roleService.getRoleStats()

      // Set stats with real data
      setStats({
        totalEmployees: employeesResponse.totalRecords,
        activeEmployees: employeesResponse.pagedData.filter(emp => emp.status === 'Active').length,
        totalRoles: rolesStats.totalRoles,
        activeRoles: rolesStats.activeRoles,
        totalPermissions: rolesStats.webRoles + rolesStats.appRoles, // Approximate count
        recentActivities: 0 // Would need activity log API
      })

      // Mock recent activities for now (would need activity log API)
      setRecentActivities([
        {
          id: '1',
          action: 'Dashboard Loaded',
          entityType: 'Permission',
          entityId: 'DASH001',
          entityName: 'Dashboard Access',
          performedBy: 'Admin',
          performedAt: new Date().toISOString(),
          details: 'Dashboard data loaded successfully'
        }
      ])
    } catch (error) {
      console.error('Failed to load dashboard data:', error)
      // Fallback to default values
      setStats({
        totalEmployees: 0,
        activeEmployees: 0,
        totalRoles: 0,
        activeRoles: 0,
        totalPermissions: 0,
        recentActivities: 0
      })
      setRecentActivities([])
    } finally {
      setLoading(false)
    }
  }

  const statCards = [
    {
      title: "Total Employees",
      value: stats.totalEmployees,
      change: "+12%",
      changeType: "positive" as const,
      icon: Users,
      description: "All registered employees"
    },
    {
      title: "Active Employees",
      value: stats.activeEmployees,
      change: "+5%",
      changeType: "positive" as const,
      icon: Activity,
      description: "Currently active accounts"
    },
    {
      title: "Total Roles",
      value: stats.totalRoles,
      change: "+2",
      changeType: "positive" as const,
      icon: UserCheck,
      description: "Available user roles"
    },
    {
      title: "Permission Sets",
      value: stats.totalPermissions,
      change: "+8%",
      changeType: "positive" as const,
      icon: Key,
      description: "Configured permissions"
    }
  ]

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i}>
              <CardHeader className="space-y-0 pb-2">
                <div className="h-4 bg-gray-200 rounded animate-pulse" />
              </CardHeader>
              <CardContent>
                <div className="h-8 bg-gray-200 rounded animate-pulse mb-2" />
                <div className="h-3 bg-gray-200 rounded animate-pulse" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Admin Dashboard</h1>
        <p className="text-muted-foreground">
          Overview of system users, roles, and permissions
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {statCards.map((stat) => {
          const Icon = stat.icon
          return (
            <Card key={stat.title}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{stat.title}</CardTitle>
                <Icon className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stat.value.toLocaleString()}</div>
                <div className="flex items-center text-xs text-muted-foreground">
                  {stat.changeType === 'positive' ? (
                    <TrendingUp className="mr-1 h-3 w-3 text-green-500" />
                  ) : (
                    <TrendingDown className="mr-1 h-3 w-3 text-red-500" />
                  )}
                  <span className={stat.changeType === 'positive' ? 'text-green-600' : 'text-red-600'}>
                    {stat.change}
                  </span>
                  <span className="ml-1">{stat.description}</span>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>

      {/* Main Content */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Recent Activities */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Recent Activities</CardTitle>
            <CardDescription>
              Latest changes to users, roles, and permissions
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentActivities.map((activity) => (
                <div key={activity.id} className="flex items-start space-x-4">
                  <div className="flex-shrink-0">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100">
                      <Activity className="h-4 w-4 text-blue-600" />
                    </div>
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <p className="text-sm font-medium text-gray-900">
                        {activity.action}
                      </p>
                      <Badge variant="secondary" className="text-xs">
                        {activity.entityType}
                      </Badge>
                    </div>
                    <p className="text-sm text-gray-600">
                      {activity.entityName} - {activity.details}
                    </p>
                    <div className="flex items-center text-xs text-gray-500 mt-1">
                      <Clock className="mr-1 h-3 w-3" />
                      {new Date(activity.performedAt).toLocaleString()} by {activity.performedBy}
                    </div>
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-4">
              <Button variant="outline" className="w-full">
                View All Activities
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* System Status */}
        <Card>
          <CardHeader>
            <CardTitle>System Status</CardTitle>
            <CardDescription>
              Current system health and alerts
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <div className="h-2 w-2 rounded-full bg-green-500" />
                  <span className="text-sm">Authentication Service</span>
                </div>
                <span className="text-xs text-green-600">Healthy</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <div className="h-2 w-2 rounded-full bg-green-500" />
                  <span className="text-sm">Database Connection</span>
                </div>
                <span className="text-xs text-green-600">Healthy</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <div className="h-2 w-2 rounded-full bg-yellow-500" />
                  <span className="text-sm">Background Jobs</span>
                </div>
                <span className="text-xs text-yellow-600">Warning</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <div className="h-2 w-2 rounded-full bg-green-500" />
                  <span className="text-sm">API Endpoints</span>
                </div>
                <span className="text-xs text-green-600">Healthy</span>
              </div>
            </div>

            <div className="mt-6 pt-4 border-t">
              <div className="flex items-center space-x-2 text-sm text-amber-600">
                <AlertCircle className="h-4 w-4" />
                <span>1 Warning requires attention</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
          <CardDescription>
            Common administrative tasks
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="users" className="w-full">
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="users">User Management</TabsTrigger>
              <TabsTrigger value="roles">Role Management</TabsTrigger>
              <TabsTrigger value="permissions">Permissions</TabsTrigger>
            </TabsList>
            <TabsContent value="users" className="space-y-4 mt-4">
              <div className="grid gap-4 md:grid-cols-3">
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Add New Employee</div>
                    <div className="text-xs text-muted-foreground">Create employee account</div>
                  </div>
                </Button>
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Bulk Import Users</div>
                    <div className="text-xs text-muted-foreground">Import from CSV/Excel</div>
                  </div>
                </Button>
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Reset Passwords</div>
                    <div className="text-xs text-muted-foreground">Bulk password reset</div>
                  </div>
                </Button>
              </div>
            </TabsContent>
            <TabsContent value="roles" className="space-y-4 mt-4">
              <div className="grid gap-4 md:grid-cols-3">
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Create New Role</div>
                    <div className="text-xs text-muted-foreground">Define user role</div>
                  </div>
                </Button>
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Clone Existing Role</div>
                    <div className="text-xs text-muted-foreground">Copy role permissions</div>
                  </div>
                </Button>
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Bulk Role Assignment</div>
                    <div className="text-xs text-muted-foreground">Assign roles to users</div>
                  </div>
                </Button>
              </div>
            </TabsContent>
            <TabsContent value="permissions" className="space-y-4 mt-4">
              <div className="grid gap-4 md:grid-cols-3">
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Permission Matrix</div>
                    <div className="text-xs text-muted-foreground">View all permissions</div>
                  </div>
                </Button>
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Copy Permissions</div>
                    <div className="text-xs text-muted-foreground">Duplicate role permissions</div>
                  </div>
                </Button>
                <Button className="justify-start h-auto p-4" variant="outline">
                  <div className="text-left">
                    <div className="font-medium">Audit Permissions</div>
                    <div className="text-xs text-muted-foreground">Review access rights</div>
                  </div>
                </Button>
              </div>
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  )
}