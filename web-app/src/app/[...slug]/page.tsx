"use client"

import { useParams, useRouter } from "next/navigation"
import { notFound } from "next/navigation"
import { usePermissions } from "@/providers/permission-provider"
import { useAuth } from "@/providers/auth-provider"
import { ProtectedRoute } from "@/components/auth/protected-route"
import { SkeletonLoader } from "@/components/ui/loader"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { CanAdd, CanEdit, CanDelete, CanView } from "@/components/auth/permission-guard"
import {
  Plus,
  Edit,
  Trash2,
  Download,
  CheckCircle,
  AlertCircle,
  Search,
  Filter
} from "lucide-react"
import { getModuleIcon } from "@/lib/navigation-icons"
import { useEffect, useState } from "react"

export default function DynamicModulePage() {
  const params = useParams()
  const router = useRouter()
  const [shouldCheckNotFound, setShouldCheckNotFound] = useState(false)

  // Get the slug array and construct the path
  const slugArray = Array.isArray(params.slug) ? params.slug : [params.slug || '']
  const fullPath = slugArray.join('/')

  // Exclude LBCL delivery routes - they have their own separate app
  if (fullPath.startsWith('lbcl')) {
    return null
  }

  const { isAuthenticated, isLoading: authLoading } = useAuth()
  const { modules, permissions } = usePermissions()
  
  // Handle authentication
  if (authLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-8">
        <div className="max-w-7xl mx-auto space-y-6">
          {/* Header skeleton */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <SkeletonLoader className="h-8 w-8 rounded" />
              <div className="space-y-2">
                <SkeletonLoader className="h-8 w-48 rounded" />
                <SkeletonLoader className="h-4 w-32 rounded" />
              </div>
            </div>
            <div className="flex gap-2">
              <SkeletonLoader className="h-9 w-20 rounded" />
              <SkeletonLoader className="h-9 w-20 rounded" />
              <SkeletonLoader className="h-9 w-24 rounded" />
            </div>
          </div>
          
          {/* Content cards skeleton */}
          <div className="space-y-6">
            <div className="bg-white dark:bg-gray-950 rounded-lg border p-6 space-y-4">
              <SkeletonLoader className="h-6 w-40 rounded" />
              <SkeletonLoader className="h-4 w-64 rounded" />
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {Array.from({ length: 4 }).map((_, i) => (
                  <div key={i} className="space-y-2">
                    <SkeletonLoader className="h-4 w-20 rounded" />
                    <SkeletonLoader className="h-6 w-full rounded" />
                  </div>
                ))}
              </div>
            </div>
            
            <div className="bg-white dark:bg-gray-950 rounded-lg border p-6 space-y-4">
              <SkeletonLoader className="h-6 w-32 rounded" />
              <SkeletonLoader className="h-4 w-56 rounded" />
              <div className="space-y-4">
                <SkeletonLoader className="h-32 w-full rounded" />
                <SkeletonLoader className="h-24 w-full rounded" />
              </div>
            </div>
          </div>
        </div>
      </div>
    )
  }
  
  if (!isAuthenticated) {
    if (typeof window !== 'undefined') {
      router.push('/login')
    }
    return null
  }
  
  // Check for invalid paths and redirect
  if (fullPath.includes('loadrequestemplate') || fullPath.includes('LoadTemplate')) {
    if (typeof window !== 'undefined') {
      router.push('/dashboard')
    }
    return null
  }
  
  // Find the matching module/page from the backend data
  const findPageInHierarchy = () => {
    // Check sub-sub-modules (pages) first as they are most specific
    for (const page of modules.subSubModules || []) {
      // Check both original relativePath and hierarchical path structure
      const parentSubModule = modules.subModules?.find(sm => sm.uid === page.subModuleUID)
      const parentModule = parentSubModule ? modules.modules?.find(m => m.uid === parentSubModule.moduleUID) : null
      
      // Build expected hierarchical path using same logic as sidebar
      if (parentModule && parentSubModule) {
        const modulePart = parentModule.relativePath || parentModule.moduleNameEn.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')
        const subModulePart = parentSubModule.relativePath || parentSubModule.submoduleNameEn.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')
        const pagePart = page.relativePath || page.subSubModuleNameEn.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')
        const hierarchicalPath = `${modulePart}/${subModulePart}/${pagePart}`
        
        // Check both original path and hierarchical path
        if (page.relativePath === fullPath || hierarchicalPath === fullPath) {
          return { 
            type: 'page' as const, 
            item: page, 
            parentModule, 
            parentSubModule 
          }
        }
      }
      
      // Fallback to original path matching
      if (page.relativePath === fullPath) {
        return { 
          type: 'page' as const, 
          item: page, 
          parentModule, 
          parentSubModule 
        }
      }
    }
    
    // Check sub-modules
    for (const subModule of modules.subModules || []) {
      if (subModule.relativePath === fullPath) {
        const parentModule = modules.modules?.find(m => m.uid === subModule.moduleUID)
        return { 
          type: 'subModule' as const, 
          item: subModule, 
          parentModule, 
          parentSubModule: null 
        }
      }
    }
    
    // Check modules
    for (const moduleItem of modules.modules || []) {
      if (moduleItem.relativePath === fullPath) {
        return { 
          type: 'module' as const, 
          item: moduleItem, 
          parentModule: null, 
          parentSubModule: null 
        }
      }
    }
    
    return null
  }
  
  const pageInfo = findPageInHierarchy()

  // Trigger not-found page if no matching page is found
  useEffect(() => {
    if (!authLoading && isAuthenticated && !pageInfo) {
      setShouldCheckNotFound(true)
    }
  }, [authLoading, isAuthenticated, pageInfo])

  if (shouldCheckNotFound && !pageInfo) {
    notFound()
  }

  if (!pageInfo) {
    return null
  }
  
  const { type, item, parentModule, parentSubModule } = pageInfo
  
  // Get the appropriate permissions for this page
  const pagePermission = permissions.find(p => p.subSubModuleUid === item.uid)
  
  // Build breadcrumb
  const breadcrumb = []
  if (parentModule) breadcrumb.push(parentModule.moduleNameEn)
  if (parentSubModule) breadcrumb.push(parentSubModule.submoduleNameEn)
  
  const pageName = type === 'page' 
    ? item.subSubModuleNameEn 
    : type === 'subModule' 
    ? item.submoduleNameEn 
    : item.moduleNameEn
  
  const Icon = getModuleIcon(pageName, item.uid)
  
  return (
    <ProtectedRoute
      requiredPermission={{
        subSubModuleUid: item.uid,
        action: 'view'
      }}
    >
      <div className="p-8">
        <div className="max-w-7xl mx-auto">
          {/* Header */}
          <div className="mb-8">
            {/* Breadcrumb */}
            {breadcrumb.length > 0 && (
              <nav className="flex space-x-2 text-sm text-gray-500 mb-4">
                {breadcrumb.map((crumb, index) => (
                  <span key={index}>
                    {crumb}
                    {index < breadcrumb.length - 1 && " / "}
                  </span>
                ))}
              </nav>
            )}
            
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Icon className="h-8 w-8 text-blue-600" />
                <div>
                  <h1 className="text-3xl font-bold text-gray-900">{pageName}</h1>
                  <p className="text-gray-600 mt-1">Path: /{fullPath}</p>
                </div>
              </div>
              
              {/* Action Buttons */}
              <div className="flex gap-2">
                <CanView moduleUid={item.uid}>
                  <Button variant="outline" size="sm">
                    <Search className="mr-2 h-4 w-4" />
                    Search
                  </Button>
                </CanView>
                
                <CanView moduleUid={item.uid}>
                  <Button variant="outline" size="sm">
                    <Filter className="mr-2 h-4 w-4" />
                    Filter
                  </Button>
                </CanView>
                
                <CanAdd moduleUid={item.uid}>
                  <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    Add New
                  </Button>
                </CanAdd>
              </div>
            </div>
          </div>
          
          {/* Permission Info Card */}
          <Card className="mb-6">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-green-500" />
                Access Granted
              </CardTitle>
              <CardDescription>
                You have access to this module with the following permissions:
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="space-y-2">
                  <p className="text-sm font-medium">Available Actions:</p>
                  <div className="flex flex-wrap gap-1">
                    {pagePermission?.fullAccess && (
                      <Badge className="bg-green-100 text-green-800">Full Access</Badge>
                    )}
                    {!pagePermission?.fullAccess && (
                      <>
                        {pagePermission?.viewAccess && <Badge variant="outline">View</Badge>}
                        {pagePermission?.addAccess && <Badge variant="outline">Add</Badge>}
                        {pagePermission?.editAccess && <Badge variant="outline">Edit</Badge>}
                        {pagePermission?.deleteAccess && <Badge variant="outline">Delete</Badge>}
                        {pagePermission?.downloadAccess && <Badge variant="outline">Download</Badge>}
                        {pagePermission?.approvalAccess && <Badge variant="outline">Approve</Badge>}
                      </>
                    )}
                  </div>
                </div>
                
                <div className="space-y-2">
                  <p className="text-sm font-medium">Module Info:</p>
                  <p className="text-sm text-gray-600">UID: {item.uid}</p>
                  <p className="text-sm text-gray-600">Type: {type}</p>
                </div>
                
                <div className="space-y-2">
                  <p className="text-sm font-medium">Platform:</p>
                  <Badge>{'platform' in item ? item.platform : 'Web'}</Badge>
                </div>
                
                <div className="space-y-2">
                  <p className="text-sm font-medium">Actions:</p>
                  <div className="flex gap-1">
                    <CanEdit moduleUid={item.uid}>
                      <Button variant="outline" size="sm">
                        <Edit className="h-4 w-4" />
                      </Button>
                    </CanEdit>
                    
                    <CanDelete moduleUid={item.uid}>
                      <Button variant="outline" size="sm" className="text-red-600">
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </CanDelete>
                    
                    {pagePermission?.downloadAccess && (
                      <Button variant="outline" size="sm">
                        <Download className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
          
          {/* Dynamic Content Area */}
          <Card>
            <CardHeader>
              <CardTitle>Dynamic Content</CardTitle>
              <CardDescription>
                This page is dynamically generated based on your permissions and backend module configuration.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="p-4 bg-blue-50 rounded-lg">
                  <h3 className="font-medium text-blue-900 mb-2">Production Ready Features:</h3>
                  <ul className="text-sm text-blue-800 space-y-1">
                    <li>✅ Dynamic routing based on backend module structure</li>
                    <li>✅ Permission-based UI elements</li>
                    <li>✅ Real-time permission validation</li>
                    <li>✅ Hierarchical breadcrumb navigation</li>
                    <li>✅ Automatic access control</li>
                  </ul>
                </div>
                
                <div className="p-4 bg-gray-50 rounded-lg">
                  <h3 className="font-medium text-gray-900 mb-2">Module Details:</h3>
                  <pre className="text-xs text-gray-600 bg-white p-2 rounded border overflow-auto">
{JSON.stringify({
  path: fullPath,
  moduleType: type,
  permissions: pagePermission,
  breadcrumb: breadcrumb
}, null, 2)}
                  </pre>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </ProtectedRoute>
  )
}