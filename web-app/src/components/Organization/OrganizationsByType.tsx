"use client"

import { useState, useEffect } from "react"
import { ChevronRight, ChevronDown, Building2, Network, Edit, Trash2, Eye, RefreshCw, Download, Plus, MoreHorizontal, MapPinned } from "lucide-react"
import { usePagePermissions } from "@/hooks/use-page-permissions"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { useToast } from "@/components/ui/use-toast"
import { organizationService, Organization, OrgType } from "@/services/organizationService"
import { cn } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"
import { useRouter } from "next/navigation"

interface TypeGroupProps {
  orgType: OrgType & { level: number }
  organizations: Organization[]
  childGroups: any[]
  level: number
  expandedTypes: Set<string>
  expandedOrgs: Set<string>
  onToggleType: (uid: string) => void
  onToggleOrg: (uid: string) => void
  orgTypeColors: Map<string, string>
  allOrganizations: Organization[]
  onRefresh: () => void
  permissions: ReturnType<typeof usePagePermissions>
}

function TypeGroup({ 
  orgType, 
  organizations, 
  childGroups, 
  level, 
  expandedTypes, 
  expandedOrgs,
  onToggleType,
  onToggleOrg,
  orgTypeColors,
  allOrganizations,
  onRefresh,
  permissions
}: TypeGroupProps) {
  const router = useRouter()
  const { toast } = useToast()
  const isExpanded = expandedTypes.has(orgType.UID)
  const hasContent = organizations.length > 0 || childGroups.length > 0
  const color = orgTypeColors.get(orgType.UID) || "gray"
  
  // Use permissions passed from parent
  const canAdd = permissions.canAdd
  const canEdit = permissions.canEdit
  const canDelete = permissions.canDelete
  
  const handleEdit = (org: Organization) => {
    router.push(`/administration/organization-management/organizations/create?id=${org.UID}`)
  }
  
  const handleDelete = async (org: Organization) => {
    if (!confirm(`Are you sure you want to delete ${org.Name}?`)) return
    
    try {
      await organizationService.deleteOrganization(org.UID)
      toast({
        title: "Success",
        description: "Organization deleted successfully"
      })
      onRefresh()
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete organization",
        variant: "destructive"
      })
    }
  }

  const getShadowClass = () => {
    return level === 0 ? 'shadow-lg' : 
           level === 1 ? 'shadow-md' : 'shadow-sm'
  }

  return (
    <div className={cn(level > 0 && "ml-6")}>
      <div
        className={cn(
          "group flex items-center gap-4 py-4 px-6 rounded-lg transition-all duration-200",
          "hover:shadow-lg hover:scale-[1.01] cursor-pointer border-2",
          getShadowClass(),
          `bg-gradient-to-r from-${color}-50 to-${color}-100 border-${color}-300`,
          level === 0 && "mb-4"
        )}
        onClick={() => hasContent && onToggleType(orgType.UID)}
      >
        {hasContent && (
          <button
            className={cn(
              "p-2 rounded-full transition-all hover:bg-white/80",
              `text-${color}-700`
            )}
          >
            {isExpanded ? (
              <ChevronDown className="h-5 w-5" />
            ) : (
              <ChevronRight className="h-5 w-5" />
            )}
          </button>
        )}
        
        <div className={cn(
          "p-3 rounded-xl",
          `bg-${color}-200 text-${color}-700`
        )}>
          <Network className="h-6 w-6" />
        </div>
        
        <div className="flex-1">
          <div className="flex items-center gap-3 mb-1">
            <h3 className={cn(
              "font-bold text-gray-900",
              level === 0 ? "text-xl" : "text-lg"
            )}>
              {orgType.Name}
            </h3>
            <Badge 
              className={cn(
                "text-xs font-medium",
                `bg-${color}-300 text-${color}-900 border-${color}-400`
              )}
            >
              Type Level {orgType.level}
            </Badge>
            {orgType.IsCompanyOrg && (
              <Badge variant="outline" className="text-xs border-blue-300 text-blue-700">
                Company
              </Badge>
            )}
            {orgType.IsFranchiseeOrg && (
              <Badge variant="outline" className="text-xs border-green-300 text-green-700">
                Franchisee
              </Badge>
            )}
            {orgType.IsWh && (
              <Badge variant="outline" className="text-xs border-orange-300 text-orange-700">
                Warehouse
              </Badge>
            )}
          </div>
          <div className="flex items-center gap-4 text-sm text-gray-600">
            <span className="flex items-center gap-1">
              <Building2 className="h-4 w-4" />
              {organizations.length} organization{organizations.length !== 1 ? 's' : ''}
            </span>
            {childGroups.length > 0 && (
              <span className="flex items-center gap-1">
                <Network className="h-4 w-4" />
                {childGroups.length} child type{childGroups.length !== 1 ? 's' : ''}
              </span>
            )}
          </div>
        </div>
        
        {canAdd && (
          <div className="opacity-0 group-hover:opacity-100 transition-opacity duration-200">
            <Button
              variant="outline"
              size="sm"
              onClick={(e) => {
                e.stopPropagation()
                router.push(`/administration/organization-management/organizations/create?orgTypeUID=${orgType.UID}`)
              }}
              className={cn(
                "border-2 hover:bg-white",
                `border-${color}-300 text-${color}-700 hover:border-${color}-400`
              )}
            >
              <Plus className="mr-1 h-4 w-4" />
              Add {orgType.Name}
            </Button>
          </div>
        )}
      </div>

      {isExpanded && (
        <div className={cn(
          "mt-4 ml-8 space-y-3 border-l-4 pl-6",
          `border-${color}-200`
        )}>
          {organizations.length > 0 && (
            <div className="space-y-2">
              <h4 className="font-medium text-gray-700 text-sm uppercase tracking-wide">
                Organizations ({organizations.length})
              </h4>
              <div className="grid gap-2">
                {organizations.map((org) => (
                  <div
                    key={org.UID}
                    className={cn(
                      "group flex items-center gap-3 py-3 px-4 rounded-lg transition-all duration-200",
                      "hover:shadow-md hover:scale-[1.01] cursor-pointer",
                      `bg-white border border-${color}-200 hover:border-${color}-300`
                    )}
                  >
                    <div className={cn(
                      "p-2 rounded-lg",
                      `bg-${color}-100 text-${color}-600`
                    )}>
                      <Building2 className="h-4 w-4" />
                    </div>
                    
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-gray-900">{org.Name}</span>
                        <Badge 
                          variant="outline" 
                          className={cn('text-xs font-mono', `border-${color}-300 text-${color}-700`)}
                        >
                          {org.Code}
                        </Badge>
                        {org.ParentName && (
                          <span className="text-xs text-gray-500">
                            under {org.ParentName}
                          </span>
                        )}
                      </div>
                      {(org.CountryName || org.RegionName || org.CityName) && (
                        <div className="flex items-center gap-1 mt-1 text-xs text-gray-500">
                          <MapPinned className="h-3 w-3" />
                          {[org.CountryName, org.RegionName, org.CityName].filter(Boolean).join(', ')}
                        </div>
                      )}
                    </div>
                    
                    <div className="flex items-center gap-2">
                      <Badge 
                        variant={org.IsActive ? "default" : "secondary"}
                        className="text-xs"
                      >
                        {org.IsActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </div>
                    
                    {(canEdit || canDelete) && (
                      <div className="opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0"
                            >
                              <span className="sr-only">Open menu</span>
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end" className="w-48">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            {canEdit && (
                              <>
                                <DropdownMenuItem onClick={() => handleEdit(org)}>
                                  <Edit className="mr-2 h-4 w-4" />
                                  Edit
                                </DropdownMenuItem>
                                <DropdownMenuItem onClick={() => router.push(`/administration/organization-management/organizations/${org.UID}`)}>
                                  <Eye className="mr-2 h-4 w-4" />
                                  View Details
                                </DropdownMenuItem>
                              </>
                            )}
                            {canEdit && canDelete && <DropdownMenuSeparator />}
                            {canDelete && (
                              <DropdownMenuItem 
                                onClick={() => handleDelete(org)}
                                className="text-red-600 hover:text-red-700 hover:bg-red-50"
                              >
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            )}
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}

          {childGroups.length > 0 && (
            <div className="space-y-3">
              {organizations.length > 0 && (
                <div className="border-t border-gray-200 pt-3" />
              )}
              {childGroups.map((childGroup) => (
                <TypeGroup
                  key={childGroup.orgType.UID}
                  {...childGroup}
                  level={level + 1}
                  expandedTypes={expandedTypes}
                  expandedOrgs={expandedOrgs}
                  onToggleType={onToggleType}
                  onToggleOrg={onToggleOrg}
                  orgTypeColors={orgTypeColors}
                  allOrganizations={allOrganizations}
                  onRefresh={onRefresh}
                  permissions={permissions}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export function OrganizationsByType() {
  const { toast } = useToast()
  const router = useRouter()
  
  // Get permissions dynamically based on the current page path
  const permissions = usePagePermissions()
  
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([])
  const [loading, setLoading] = useState(false)
  const [expandedTypes, setExpandedTypes] = useState<Set<string>>(new Set())
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set())

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    setLoading(true)
    try {
      const [orgsResult, typesResult] = await Promise.all([
        organizationService.getOrganizationHierarchy(),
        organizationService.getOrganizationTypes()
      ])
      const visibleOrgs = orgsResult.filter(org => org.ShowInUI !== false)
      setOrganizations(visibleOrgs)
      
      const visibleOrgTypes = typesResult.filter(type => type.ShowInUI !== false)
      setOrgTypes(visibleOrgTypes)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch data",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const handleToggleType = (uid: string) => {
    const newExpanded = new Set(expandedTypes)
    if (newExpanded.has(uid)) {
      newExpanded.delete(uid)
    } else {
      newExpanded.add(uid)
    }
    setExpandedTypes(newExpanded)
  }

  const handleToggle = (uid: string) => {
    const newExpanded = new Set(expandedNodes)
    if (newExpanded.has(uid)) {
      newExpanded.delete(uid)
    } else {
      newExpanded.add(uid)
    }
    setExpandedNodes(newExpanded)
  }

  const orgTypeHierarchy = organizationService.buildOrgTypeHierarchy(orgTypes)
  
  const getOrgTypeColors = (): Map<string, string> => {
    const colors = new Map<string, string>()
    const colorsByLevel = ["blue", "green", "purple", "orange", "gray"]
    
    orgTypeHierarchy.forEach((type, uid) => {
      colors.set(uid, colorsByLevel[Math.min(type.level, colorsByLevel.length - 1)])
    })
    
    return colors
  }

  const orgTypeColors = getOrgTypeColors()

  const buildHierarchicalTypeView = (): any[] => {
    const groupedByType = new Map<string, Organization[]>()
    
    organizations.forEach(org => {
      if (!groupedByType.has(org.OrgTypeUID)) {
        groupedByType.set(org.OrgTypeUID, [])
      }
      groupedByType.get(org.OrgTypeUID)!.push(org)
    })

    const buildTypeTree = (parentUID: string | null | undefined): any[] => {
      const currentLevelTypes = Array.from(orgTypeHierarchy.values())
        .filter(type => {
          if (!parentUID) {
            return !type.ParentUID || type.ParentUID === ''
          }
          return type.ParentUID === parentUID
        })
        .sort((a, b) => a.Name.localeCompare(b.Name))

      return currentLevelTypes.map(typeInfo => ({
        orgType: typeInfo,
        organizations: groupedByType.get(typeInfo.UID) || [],
        childGroups: buildTypeTree(typeInfo.UID)
      }))
    }

    return buildTypeTree(null)
  }

  const typeViewData = buildHierarchicalTypeView()

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Organizations by Type</h1>
          <p className="text-muted-foreground">
            View organizations grouped by their types and hierarchy
          </p>
        </div>
        <div className="flex gap-2">
          <Button 
            variant="outline" 
            onClick={() => fetchData()}
            disabled={loading}
          >
            <RefreshCw className={cn("mr-2 h-4 w-4", loading && "animate-spin")} />
            Refresh
          </Button>
          {permissions.canDownload && (
            <Button 
              variant="outline" 
              onClick={async () => {
                try {
                  const blob = await organizationService.exportOrganizations("csv")
                  const url = URL.createObjectURL(blob)
                  const a = document.createElement('a')
                  a.href = url
                  a.download = `organizations-by-type-${new Date().toISOString().split("T")[0]}.csv`
                  document.body.appendChild(a)
                  a.click()
                  document.body.removeChild(a)
                  URL.revokeObjectURL(url)
                  
                  toast({
                    title: "Success",
                    description: "Organizations exported successfully.",
                  })
                } catch (error) {
                  toast({
                    title: "Error",
                    description: "Failed to export organizations. Please try again.",
                    variant: "destructive",
                  })
                }
              }}
            >
              <Download className="mr-2 h-4 w-4" />
              Export
            </Button>
          )}
          {permissions.canAdd && (
            <Button onClick={() => router.push('/administration/organization-management/organizations/create')}>
              <Plus className="mr-2 h-4 w-4" />
              Add Organization
            </Button>
          )}
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Type Hierarchy</CardTitle>
          <CardDescription>
            Organizations grouped by their types, following the type hierarchy structure
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-4 gap-4">
            {loading ? (
              [...Array(4)].map((_, i) => (
                <div key={i} className="bg-white p-4 rounded-lg shadow-sm border">
                  <Skeleton className="h-4 w-28 mb-2" />
                  <Skeleton className="h-8 w-8" />
                </div>
              ))
            ) : (
              <>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Organization Types</p>
                  <p className="text-2xl font-bold text-blue-600">{orgTypes.length}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Company Types</p>
                  <p className="text-2xl font-bold text-green-600">
                    {orgTypes.filter(t => t.IsCompanyOrg).length}
                  </p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Franchisee Types</p>
                  <p className="text-2xl font-bold text-purple-600">
                    {orgTypes.filter(t => t.IsFranchiseeOrg).length}
                  </p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Warehouse Types</p>
                  <p className="text-2xl font-bold text-orange-600">
                    {orgTypes.filter(t => t.IsWh).length}
                  </p>
                </div>
              </>
            )}
          </div>

          <div className="flex justify-between items-center">
            <div className="text-sm text-gray-600">
              Showing organizations grouped by their types, following the type hierarchy structure
            </div>
            <div className="flex gap-2">
              <Button 
                variant="outline" 
                size="sm" 
                onClick={() => setExpandedTypes(new Set(orgTypes.map(t => t.UID)))}
              >
                Expand All
              </Button>
              <Button 
                variant="outline" 
                size="sm" 
                onClick={() => setExpandedTypes(new Set())}
              >
                Collapse All
              </Button>
            </div>
          </div>

          <div className="border rounded-lg bg-gradient-to-br from-gray-50 to-gray-100 p-6 max-h-[700px] overflow-y-auto shadow-inner">
            {loading ? (
              <div className="space-y-6">
                {[...Array(2)].map((_, i) => (
                  <div key={i} className="space-y-4">
                    <div className="flex items-center gap-4 p-6 rounded-lg bg-gradient-to-r from-blue-50 to-blue-100 border-2 border-blue-300">
                      <Skeleton className="h-6 w-6 rounded-full" />
                      <Skeleton className="h-12 w-12 rounded-xl" />
                      <div className="flex-1 space-y-2">
                        <div className="flex items-center gap-3">
                          <Skeleton className="h-6 w-40" />
                          <Skeleton className="h-5 w-16" />
                          <Skeleton className="h-5 w-20" />
                        </div>
                        <div className="flex items-center gap-4">
                          <Skeleton className="h-4 w-32" />
                          <Skeleton className="h-4 w-24" />
                        </div>
                      </div>
                      <Skeleton className="h-8 w-24 rounded" />
                    </div>
                  </div>
                ))}
              </div>
            ) : typeViewData.length === 0 ? (
              <div className="text-center py-12">
                <Network className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 font-medium">No organization types found</p>
                <p className="text-sm text-gray-400 mt-2">
                  Create organization types to see the hierarchy
                </p>
              </div>
            ) : (
              <div className="space-y-1">
                {typeViewData.map((group) => (
                  <TypeGroup
                    key={group.orgType.UID}
                    {...group}
                    level={0}
                    expandedTypes={expandedTypes}
                    expandedOrgs={expandedNodes}
                    onToggleType={handleToggleType}
                    onToggleOrg={handleToggle}
                    orgTypeColors={orgTypeColors}
                    allOrganizations={organizations}
                    onRefresh={fetchData}
                    permissions={permissions}
                  />
                ))}
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}