"use client"

import { useState } from "react"
import { useAuth } from "@/providers/auth-provider"
import { usePermissions } from "@/providers/permission-provider"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { permissionService } from "@/lib/permission-service"
import { RefreshCw, Database, Eye, Settings } from "lucide-react"
import { SkeletonLoader } from "@/components/ui/loader"

export default function DebugPage() {
  const { user } = useAuth()
  const { modules, permissions, menuHierarchy, isLoading, refreshPermissions } = usePermissions()
  const [testResults, setTestResults] = useState<{
    modules: unknown
    permissions: unknown
    errors: string[]
  } | null>(null)
  const [testing, setTesting] = useState(false)
  
  const testBackendAPI = async () => {
    if (!user?.roles?.[0]) return
    
    setTesting(true)
    const results = { modules: null as unknown, permissions: null as unknown, errors: [] as string[] }
    
    try {
      // Test modules API
      const modulesMaster = await permissionService.getModulesMaster('Web')
      results.modules = modulesMaster
      
      // Test permissions API
      const userPermissions = await permissionService.getPermissionsByRole(
        user.roles[0].uid || user.roles[0].code,
        'Web',
        user.roles[0].isPrincipalRole || false
      )
      results.permissions = userPermissions
      
    } catch (error) {
      results.errors.push(error instanceof Error ? error.message : String(error))
    }
    
    setTestResults(results)
    setTesting(false)
  }
  
  return (
    <div className="p-8">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">System Debug Console</h1>
            <p className="text-gray-600 mt-1">Debug backend integration and permission system</p>
          </div>
          <div className="flex gap-2">
            <Button onClick={refreshPermissions} disabled={isLoading}>
              {isLoading ? (
                <>
                  <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                  Refreshing...
                </>
              ) : (
                <>
                  <RefreshCw className="mr-2 h-4 w-4" />
                  Refresh
                </>
              )}
            </Button>
            <Button onClick={testBackendAPI} disabled={testing}>
              <Database className="mr-2 h-4 w-4" />
              Test Backend
            </Button>
          </div>
        </div>
        
        {/* Current User Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Eye className="h-5 w-5" />
              Current User Context
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="font-medium">User Details:</p>
                <pre className="text-xs bg-gray-100 p-2 rounded mt-1 overflow-auto">
{JSON.stringify({
  id: user?.id,
  uid: user?.uid,
  loginId: user?.loginId,
  name: user?.name,
  status: user?.status
}, null, 2)}
                </pre>
              </div>
              <div>
                <p className="font-medium">Role Details:</p>
                <pre className="text-xs bg-gray-100 p-2 rounded mt-1 overflow-auto">
{JSON.stringify(user?.roles || [], null, 2)}
                </pre>
              </div>
            </div>
          </CardContent>
        </Card>
        
        {/* Modules Data */}
        <Card>
          <CardHeader>
            <CardTitle>Backend Modules Data</CardTitle>
            <CardDescription>
              Raw module hierarchy from WINITAPI
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div>
                <p className="font-medium">Modules ({modules.modules?.length || 0}):</p>
                <div className="max-h-64 overflow-auto">
                  <pre className="text-xs bg-gray-100 p-2 rounded mt-1">
{JSON.stringify(modules.modules || [], null, 2)}
                  </pre>
                </div>
              </div>
              
              <div>
                <p className="font-medium">Sub-Modules ({modules.subModules?.length || 0}):</p>
                <div className="max-h-64 overflow-auto">
                  <pre className="text-xs bg-gray-100 p-2 rounded mt-1">
{JSON.stringify(modules.subModules || [], null, 2)}
                  </pre>
                </div>
              </div>
              
              <div>
                <p className="font-medium">Sub-Sub-Modules/Pages ({modules.subSubModules?.length || 0}):</p>
                <div className="max-h-64 overflow-auto">
                  <pre className="text-xs bg-gray-100 p-2 rounded mt-1">
{JSON.stringify(modules.subSubModules || [], null, 2)}
                  </pre>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
        
        {/* Permissions Data */}
        <Card>
          <CardHeader>
            <CardTitle>User Permissions</CardTitle>
            <CardDescription>
              Permissions for current user role
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex items-center gap-4">
                <Badge>Total Permissions: {permissions.length}</Badge>
                <Badge variant="outline">Loading: {isLoading ? 'Yes' : 'No'}</Badge>
              </div>
              
              <div className="max-h-64 overflow-auto">
                <pre className="text-xs bg-gray-100 p-2 rounded">
{JSON.stringify(permissions, null, 2)}
                </pre>
              </div>
            </div>
          </CardContent>
        </Card>
        
        {/* Menu Hierarchy */}
        <Card>
          <CardHeader>
            <CardTitle>Processed Menu Hierarchy</CardTitle>
            <CardDescription>
              Menu structure after permission filtering
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="max-h-64 overflow-auto">
              <pre className="text-xs bg-gray-100 p-2 rounded">
{JSON.stringify(menuHierarchy, null, 2)}
              </pre>
            </div>
          </CardContent>
        </Card>
        
        {/* API Test Results */}
        {testResults && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Backend API Test Results
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {testResults.errors.length > 0 && (
                  <div>
                    <p className="font-medium text-red-600">Errors:</p>
                    <ul className="list-disc list-inside text-sm text-red-600 space-y-1">
                      {testResults.errors.map((error: string, index: number) => (
                        <li key={index}>{error}</li>
                      ))}
                    </ul>
                  </div>
                )}
                
                <div>
                  <p className="font-medium">Raw Modules Response:</p>
                  <div className="max-h-64 overflow-auto">
                    <pre className="text-xs bg-gray-100 p-2 rounded mt-1">
{JSON.stringify(testResults.modules, null, 2)}
                    </pre>
                  </div>
                </div>
                
                <div>
                  <p className="font-medium">Raw Permissions Response:</p>
                  <div className="max-h-64 overflow-auto">
                    <pre className="text-xs bg-gray-100 p-2 rounded mt-1">
{JSON.stringify(testResults.permissions, null, 2)}
                    </pre>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  )
}