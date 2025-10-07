"use client";

import { useAuth } from "@/providers/auth-provider";
import { usePermissions } from "@/providers/permission-provider";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Shield,
  Users,
  Settings,
  Menu,
  Lock,
  Bug,
  FileCheck,
  Calculator,
  UserCog,
} from "lucide-react";
import { useRouter } from "next/navigation";
import { SkeletonLoader } from "@/components/ui/loader";
import { UpdatedFeaturesMenu } from "@/components/navigation/UpdatedFeaturesMenu";

export default function DashboardPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { menuHierarchy, permissions, isLoading } = usePermissions();

  if (!user) {
    return (
      <div className="p-8">
        <div className="max-w-7xl mx-auto space-y-8">
          {/* Header skeleton */}
          <div className="flex items-center justify-between">
            <div className="space-y-2">
              <SkeletonLoader className="h-9 w-64 rounded" />
              <SkeletonLoader className="h-5 w-80 rounded" />
            </div>
            <div className="flex items-center gap-3">
              <SkeletonLoader className="h-10 w-24 rounded" />
              <SkeletonLoader className="h-10 w-24 rounded" />
            </div>
          </div>

          {/* Stats grid skeleton */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="bg-white dark:bg-gray-950 rounded-lg border p-6 space-y-4">
                <div className="flex items-center justify-between">
                  <SkeletonLoader className="h-5 w-20 rounded" />
                  <SkeletonLoader className="h-5 w-5 rounded" />
                </div>
                <SkeletonLoader className="h-8 w-16 rounded" />
                <SkeletonLoader className="h-4 w-24 rounded" />
              </div>
            ))}
          </div>

          {/* Main content skeleton */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-white dark:bg-gray-950 rounded-lg border p-6 space-y-4">
              <SkeletonLoader className="h-6 w-32 rounded" />
              <SkeletonLoader className="h-4 w-48 rounded" />
              <div className="space-y-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <SkeletonLoader key={i} className="h-12 w-full rounded" />
                ))}
              </div>
            </div>
            <div className="bg-white dark:bg-gray-950 rounded-lg border p-6 space-y-4">
              <SkeletonLoader className="h-6 w-40 rounded" />
              <SkeletonLoader className="h-4 w-56 rounded" />
              <div className="space-y-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <SkeletonLoader key={i} className="h-12 w-full rounded" />
                ))}
              </div>
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
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">
              Welcome back, {user.name}
            </h1>
            <p className="text-gray-600 mt-1">
              WINIT Access Control System Dashboard
            </p>
          </div>
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              size="sm"
              onClick={() => router.push("/admin")}
              className="text-orange-600 border-orange-200 hover:bg-orange-50"
            >
              <UserCog className="mr-2 h-4 w-4" />
              Admin Dashboard
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => router.push("/commission")}
              className="text-green-600 border-green-200 hover:bg-green-50"
            >
              <Calculator className="mr-2 h-4 w-4" />
              Commission Dashboard
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => router.push("/audit-test")}
              className="text-purple-600 border-purple-200 hover:bg-purple-50"
            >
              <FileCheck className="mr-2 h-4 w-4" />
              Audit & Session Test
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => router.push("/debug")}
              className="text-blue-600 border-blue-200 hover:bg-blue-50"
            >
              <Bug className="mr-2 h-4 w-4" />
              Debug Console
            </Button>
            <Badge
              variant="outline"
              className="bg-green-50 text-green-700 border-green-200"
            >
              {user.status}
            </Badge>
          </div>
        </div>

        {/* User Info Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">User ID</CardTitle>
              <Shield className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{user.loginId}</div>
              <p className="text-xs text-muted-foreground">UID: {user.uid}</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                Organization
              </CardTitle>
              <Users className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {user.currentOrganization?.name}
              </div>
              <p className="text-xs text-muted-foreground">
                {user.currentOrganization?.type}
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                Active Roles
              </CardTitle>
              <Settings className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{user.roles.length}</div>
              <p className="text-xs text-muted-foreground">
                {user.roles.find((r) => r.isDefault)?.roleNameEn ||
                  "No default role"}
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Permissions</CardTitle>
              <Lock className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {isLoading ? (
                  <SkeletonLoader className="h-8 w-12 rounded inline-block" />
                ) : (
                  permissions.length
                )}
              </div>
              <p className="text-xs text-muted-foreground">
                Active permissions
              </p>
            </CardContent>
          </Card>
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Available Modules</CardTitle>
              <CardDescription>
                Modules you have access to based on your permissions
              </CardDescription>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="py-8 space-y-3">
                  {Array.from({ length: 3 }).map((_, i) => (
                    <div key={i} className="flex items-center gap-3 p-3 rounded-lg">
                      <SkeletonLoader className="h-5 w-5 rounded" />
                      <SkeletonLoader className="h-5 w-32 rounded" />
                      <SkeletonLoader className="h-4 w-16 rounded ml-auto" />
                    </div>
                  ))}
                </div>
              ) : menuHierarchy.length > 0 ? (
                <div className="space-y-2">
                  {menuHierarchy.map((module) => (
                    <div
                      key={module.uid}
                      className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                    >
                      <div className="flex items-center gap-3">
                        <Menu className="h-5 w-5 text-gray-600" />
                        <div>
                          <p className="font-medium text-sm">
                            {module.moduleNameEn}
                          </p>
                          <p className="text-xs text-gray-500">
                            {"children" in module &&
                            Array.isArray(module.children)
                              ? module.children.length
                              : 0}{" "}
                            sub-modules
                          </p>
                        </div>
                      </div>
                      <Badge variant="secondary" className="text-xs">
                        {module.platform}
                      </Badge>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-gray-500">No modules available</p>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Security Settings</CardTitle>
              <CardDescription>
                Your security preferences and settings
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">
                    Multi-Factor Authentication
                  </span>
                  <Badge
                    variant={
                      user.preferences.security.mfaEnabled
                        ? "default"
                        : "secondary"
                    }
                  >
                    {user.preferences.security.mfaEnabled
                      ? "Enabled"
                      : "Disabled"}
                  </Badge>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">
                    Trusted Devices Only
                  </span>
                  <Badge
                    variant={
                      user.preferences.security.trustedDevicesOnly
                        ? "default"
                        : "secondary"
                    }
                  >
                    {user.preferences.security.trustedDevicesOnly
                      ? "Yes"
                      : "No"}
                  </Badge>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Session Timeout</span>
                  <span className="text-sm text-gray-600">
                    {user.preferences.security.sessionTimeout} minutes
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Current Role</span>
                  <Badge>{user.roles[0]?.roleNameEn || "No Role"}</Badge>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Role Details */}
        <Card className="mt-6">
          <CardHeader>
            <CardTitle>Your Roles & Permissions</CardTitle>
            <CardDescription>
              Details about your assigned roles and their capabilities
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {user.roles.map((role) => (
                <div key={role.id} className="border rounded-lg p-4">
                  <div className="flex items-center justify-between mb-2">
                    <h4 className="font-medium">{role.roleNameEn}</h4>
                    <div className="flex gap-2">
                      {role.isAdmin && <Badge>Admin</Badge>}
                      {role.isPrincipalRole && (
                        <Badge variant="secondary">Principal</Badge>
                      )}
                      {role.isWebUser && <Badge variant="outline">Web</Badge>}
                      {role.isAppUser && (
                        <Badge variant="outline">Mobile</Badge>
                      )}
                    </div>
                  </div>
                  <p className="text-sm text-gray-600">
                    Code: {role.code} | UID: {role.uid}
                  </p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Updated Features Section */}
      <div className="mt-8">
        <Card>
          <CardHeader>
            <CardTitle className="text-2xl">Updated Features</CardTitle>
            <CardDescription>
              Access the newly updated route and journey plan management features
            </CardDescription>
          </CardHeader>
          <CardContent>
            <UpdatedFeaturesMenu />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
