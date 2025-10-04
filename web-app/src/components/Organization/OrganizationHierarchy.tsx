"use client"

import { useState, useEffect } from "react"
import { ChevronRight, ChevronDown, Building2, Search, Globe, Building, MapPinned, Network, Edit, Trash2, Eye, RefreshCw, Download, Plus, MoreHorizontal } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
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
import { organizationService, Organization, OrganizationHierarchyNode, OrgType } from "@/services/organizationService"
import { cn } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"
import { useRouter } from "next/navigation"

interface TreeNodeProps {
  node: OrganizationHierarchyNode
  level: number
  onToggle: (uid: string) => void
  expandedNodes: Set<string>
  orgTypeColors: Map<string, string>
  organizations: Organization[]
  onRefresh: () => void
}

function TreeNode({ node, level, onToggle, expandedNodes, orgTypeColors, organizations, onRefresh }: TreeNodeProps) {
  const router = useRouter()
  const { toast } = useToast()
  const isExpanded = expandedNodes.has(node.uid)
  const hasChildren = node.children && node.children.length > 0
  const color = orgTypeColors.get(node.orgTypeUID) || "gray"
  
  // Find the full organization object
  const org = organizations.find(o => o.UID === node.uid)
  
  const handleEdit = () => {
    router.push(`/updatedfeatures/organization-management/organizations/create?id=${node.uid}`)
  }
  
  const handleDelete = async () => {
    if (!confirm(`Are you sure you want to delete ${node.name}?`)) return
    
    try {
      await organizationService.deleteOrganization(node.uid)
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
  
  const handleInsertHierarchy = async () => {
    try {
      const result = await organizationService.insertOrganizationHierarchy(node.uid)
      if (result.IsSuccess) {
        toast({
          title: "Success",
          description: "Organization hierarchy inserted successfully"
        })
      } else {
        toast({
          title: "Error",
          description: result.Error || "Failed to insert hierarchy",
          variant: "destructive"
        })
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to insert organization hierarchy",
        variant: "destructive"
      })
    }
  }

  const getIcon = () => {
    switch (level) {
      case 0: return <Globe className="h-4 w-4 text-blue-600" />
      case 1: return <Building className="h-4 w-4 text-green-600" />
      case 2: return <MapPinned className="h-4 w-4 text-purple-600" />
      default: return <Building2 className="h-4 w-4 text-gray-600" />
    }
  }

  return (
    <div className={cn(level > 0 && "border-l-2 ml-4", `border-${color}-200`)}>
      <div
        className={cn(
          "group flex items-center gap-3 py-3 px-4 rounded-lg transition-all duration-200",
          "hover:shadow-md hover:scale-[1.01] cursor-pointer",
          level === 0 ? "shadow-lg" : level === 1 ? "shadow-md" : "shadow-sm",
          `bg-${color}-50 border border-${color}-300`,
          level === 0 && "border-2"
        )}
        style={{ marginLeft: `${level * 16}px` }}
      >
        {/* Expand/Collapse Button */}
        {hasChildren && (
          <button
            onClick={() => onToggle(node.uid)}
            className={cn(
              "p-1.5 rounded-md transition-all hover:bg-white/80",
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
        
        {/* Organization Icon */}
        <div className={cn(
          "p-2 rounded-lg",
          `bg-${color}-100 text-${color}-700`
        )}>
          {getIcon()}
        </div>
        
        {/* Organization Details */}
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h4 className="font-semibold text-gray-900">{node.name}</h4>
            <Badge 
              variant="outline" 
              className={cn('text-xs font-mono', `border-${color}-300 text-${color}-700`)}
            >
              {node.code}
            </Badge>
            <Badge 
              className={cn(
                "text-xs",
                `bg-${color}-200 text-${color}-800 border-${color}-300`
              )}
            >
              {node.orgTypeName}
            </Badge>
            {level === 0 && (
              <Badge variant="default" className="text-xs">
                Root
              </Badge>
            )}
          </div>
          {hasChildren && (
            <p className="text-sm text-gray-500 mt-1">
              {node.children.length} child organization{node.children.length !== 1 ? 's' : ''}
            </p>
          )}
        </div>
        
        {/* Action Menu */}
        {org && (
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
                <DropdownMenuItem onClick={handleEdit}>
                  <Edit className="mr-2 h-4 w-4" />
                  Edit
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => router.push(`/updatedfeatures/organization-management/organizations/${org.UID}`)}>
                  <Eye className="mr-2 h-4 w-4" />
                  View Details
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleInsertHierarchy}>
                  <RefreshCw className="mr-2 h-4 w-4" />
                  Insert Hierarchy
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem 
                  onClick={handleDelete}
                  className="text-red-600 hover:text-red-700 hover:bg-red-50"
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        )}
      </div>

      {/* Children */}
      {isExpanded && node.children && (
        <div className={cn(
          "mt-2 pt-2",
          `border-l-2 border-${color}-200`
        )}>
          {node.children.map((child) => (
            <TreeNode
              key={child.uid}
              node={child}
              level={level + 1}
              onToggle={onToggle}
              expandedNodes={expandedNodes}
              orgTypeColors={orgTypeColors}
              organizations={organizations}
              onRefresh={onRefresh}
            />
          ))}
        </div>
      )}
    </div>
  )
}

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
  onRefresh
}: TypeGroupProps) {
  const router = useRouter()
  const { toast } = useToast()
  const isExpanded = expandedTypes.has(orgType.UID)
  const hasContent = organizations.length > 0 || childGroups.length > 0
  const color = orgTypeColors.get(orgType.UID) || "gray"
  
  const handleEdit = (org: Organization) => {
    router.push(`/updatedfeatures/organization-management/organizations/create?id=${org.UID}`)
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
  
  const handleInsertHierarchy = async (org: Organization) => {
    try {
      const result = await organizationService.insertOrganizationHierarchy(org.UID)
      if (result.IsSuccess) {
        toast({
          title: "Success",
          description: "Organization hierarchy inserted successfully"
        })
      } else {
        toast({
          title: "Error",
          description: result.Error || "Failed to insert hierarchy",
          variant: "destructive"
        })
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to insert organization hierarchy",
        variant: "destructive"
      })
    }
  }

  // Dynamic styling based on level
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
        {/* Expand/Collapse Button */}
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
        
        {/* Type Icon */}
        <div className={cn(
          "p-3 rounded-xl",
          `bg-${color}-200 text-${color}-700`
        )}>
          <Network className="h-6 w-6" />
        </div>
        
        {/* Type Details */}
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
        
        {/* Quick Actions */}
        <div className="opacity-0 group-hover:opacity-100 transition-opacity duration-200">
          <Button
            variant="outline"
            size="sm"
            onClick={(e) => {
              e.stopPropagation()
              router.push(`/updatedfeatures/organization-management/organizations/create?orgTypeUID=${orgType.UID}`)
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
      </div>

      {/* Expanded Content */}
      {isExpanded && (
        <div className={cn(
          "mt-4 ml-8 space-y-3 border-l-4 pl-6",
          `border-${color}-200`
        )}>
          {/* Organizations under this type */}
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
                    {/* Organization Icon */}
                    <div className={cn(
                      "p-2 rounded-lg",
                      `bg-${color}-100 text-${color}-600`
                    )}>
                      <Building2 className="h-4 w-4" />
                    </div>
                    
                    {/* Organization Details */}
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
                    
                    {/* Status */}
                    <div className="flex items-center gap-2">
                      <Badge 
                        variant={org.IsActive ? "default" : "secondary"}
                        className="text-xs"
                      >
                        {org.IsActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </div>
                    
                    {/* Action Menu */}
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
                          <DropdownMenuItem onClick={() => handleEdit(org)}>
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => router.push(`/updatedfeatures/organization-management/organizations/${org.UID}`)}>
                            <Eye className="mr-2 h-4 w-4" />
                            View Details
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleInsertHierarchy(org)}>
                            <RefreshCw className="mr-2 h-4 w-4" />
                            Insert Hierarchy
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem 
                            onClick={() => handleDelete(org)}
                            className="text-red-600 hover:text-red-700 hover:bg-red-50"
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Child type groups */}
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
                />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export function OrganizationHierarchy() {
  const { toast } = useToast()
  const router = useRouter()
  
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([])
  const [hierarchyData, setHierarchyData] = useState<OrganizationHierarchyNode[]>([])
  const [loading, setLoading] = useState(false)
  const [selectedOrgType, setSelectedOrgType] = useState<string>("all")
  const [searchQuery, setSearchQuery] = useState("")
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set())
  const [expandedTypes, setExpandedTypes] = useState<Set<string>>(new Set())
  const [filteredData, setFilteredData] = useState<OrganizationHierarchyNode[]>([])
  const [activeView, setActiveView] = useState<"operational" | "by-type">("operational")

  useEffect(() => {
    fetchData()
  }, [])

  useEffect(() => {
    if (organizations.length > 0) {
      buildHierarchy()
    }
  }, [organizations, selectedOrgType])

  useEffect(() => {
    filterHierarchy()
  }, [hierarchyData, searchQuery])

  const fetchData = async () => {
    setLoading(true)
    try {
      const [orgsResult, typesResult] = await Promise.all([
        organizationService.getOrganizationHierarchy(),
        organizationService.getOrganizationTypes()
      ])
      // Filter organizations to show only those with ShowInUI = true
      const visibleOrgs = orgsResult.filter(org => org.ShowInUI !== false)
      setOrganizations(visibleOrgs)
      
      // Filter organization types to show only those with ShowInUI = true
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

  const buildHierarchy = () => {
    const tree = organizationService.buildOrganizationTree(
      organizations, 
      selectedOrgType === "all" ? undefined : selectedOrgType
    )
    setHierarchyData(tree)
    
    // Auto-expand first level
    const firstLevelIds = new Set(tree.map(node => node.uid))
    setExpandedNodes(firstLevelIds)
  }

  const filterHierarchy = () => {
    if (!searchQuery) {
      setFilteredData(hierarchyData)
      return
    }

    const query = searchQuery.toLowerCase()
    
    const filterNodes = (nodes: OrganizationHierarchyNode[]): OrganizationHierarchyNode[] => {
      return nodes.reduce((filtered, node) => {
        const nodeMatches = 
          node.name.toLowerCase().includes(query) ||
          node.code.toLowerCase().includes(query)

        const filteredChildren = node.children ? filterNodes(node.children) : []

        if (nodeMatches || filteredChildren.length > 0) {
          filtered.push({
            ...node,
            children: filteredChildren
          })
        }

        return filtered
      }, [] as OrganizationHierarchyNode[])
    }

    const filtered = filterNodes(hierarchyData)
    setFilteredData(filtered)
    
    // Expand all nodes when searching
    if (searchQuery) {
      const allNodeIds = new Set<string>()
      const collectIds = (nodes: OrganizationHierarchyNode[]) => {
        nodes.forEach(node => {
          allNodeIds.add(node.uid)
          if (node.children) {
            collectIds(node.children)
          }
        })
      }
      collectIds(filtered)
      setExpandedNodes(allNodeIds)
    }
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

  const handleToggleType = (uid: string) => {
    const newExpanded = new Set(expandedTypes)
    if (newExpanded.has(uid)) {
      newExpanded.delete(uid)
    } else {
      newExpanded.add(uid)
    }
    setExpandedTypes(newExpanded)
  }

  const expandAll = () => {
    const allNodeIds = new Set<string>()
    const collectIds = (nodes: OrganizationHierarchyNode[]) => {
      nodes.forEach(node => {
        allNodeIds.add(node.uid)
        if (node.children) {
          collectIds(node.children)
        }
      })
    }
    collectIds(filteredData)
    setExpandedNodes(allNodeIds)
  }

  const collapseAll = () => {
    setExpandedNodes(new Set())
  }

  // Build org type hierarchy
  const orgTypeHierarchy = organizationService.buildOrgTypeHierarchy(orgTypes)
  
  // Generate colors for org types
  const getOrgTypeColors = (): Map<string, string> => {
    const colors = new Map<string, string>()
    const colorsByLevel = ["blue", "green", "purple", "orange", "gray"]
    
    orgTypeHierarchy.forEach((type, uid) => {
      colors.set(uid, colorsByLevel[Math.min(type.level, colorsByLevel.length - 1)])
    })
    
    return colors
  }

  const orgTypeColors = getOrgTypeColors()

  // Build hierarchical type view
  const buildHierarchicalTypeView = (): any[] => {
    const groupedByType = new Map<string, Organization[]>()
    
    // Group organizations by type
    organizations.forEach(org => {
      if (!groupedByType.has(org.OrgTypeUID)) {
        groupedByType.set(org.OrgTypeUID, [])
      }
      groupedByType.get(org.OrgTypeUID)!.push(org)
    })

    // Build tree starting from root types
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
      })).filter(group => group.organizations.length > 0 || group.childGroups.length > 0)
    }

    return buildTypeTree(null)
  }

  const typeViewData = buildHierarchicalTypeView()

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Organization Hierarchy</h1>
          <p className="text-muted-foreground">
            Visualize and navigate through your organizational structure
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
          <Button 
            variant="outline" 
            onClick={async () => {
              try {
                const blob = await organizationService.exportOrganizations("csv")
                const url = URL.createObjectURL(blob)
                const a = document.createElement('a')
                a.href = url
                a.download = `organizations-export-${new Date().toISOString().split("T")[0]}.csv`
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
          <Button onClick={() => router.push('/updatedfeatures/organization-management/organizations/create')}>
            <Plus className="mr-2 h-4 w-4" />
            Add Organization
          </Button>
        </div>
      </div>

      {/* Controls */}
      <Card>
        <CardHeader>
          <CardTitle>Hierarchy View</CardTitle>
          <CardDescription>
            Explore different organizational structures and relationships
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Tabs value={activeView} onValueChange={(v) => setActiveView(v as any)}>
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="operational">Operational Hierarchy</TabsTrigger>
              <TabsTrigger value="by-type">Organizations by Type</TabsTrigger>
            </TabsList>

            <TabsContent value="operational" className="space-y-4">
              {/* Statistics Cards */}
              <div className="grid grid-cols-3 gap-4">
                {loading ? (
                  [...Array(3)].map((_, i) => (
                    <div key={i} className="bg-white p-4 rounded-lg shadow-sm border">
                      <Skeleton className="h-4 w-32 mb-2" />
                      <Skeleton className="h-8 w-12" />
                    </div>
                  ))
                ) : (
                  <>
                    <div className="bg-white p-4 rounded-lg shadow-sm border">
                      <p className="text-sm font-medium text-gray-600">Total Organizations</p>
                      <p className="text-2xl font-bold text-blue-600">{organizations.length}</p>
                    </div>
                    <div className="bg-white p-4 rounded-lg shadow-sm border">
                      <p className="text-sm font-medium text-gray-600">Root Organizations</p>
                      <p className="text-2xl font-bold text-green-600">{hierarchyData.length}</p>
                    </div>
                    <div className="bg-white p-4 rounded-lg shadow-sm border">
                      <p className="text-sm font-medium text-gray-600">With Parents</p>
                      <p className="text-2xl font-bold text-purple-600">
                        {organizations.filter(org => org.ParentUID).length}
                      </p>
                    </div>
                  </>
                )}
              </div>

              {/* Filters */}
              <div className="flex flex-col gap-4 md:flex-row md:items-center">
                <div className="flex-1 flex gap-2">
                  <Input
                    placeholder="Search organizations..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="max-w-sm"
                  />
                  <Search className="h-5 w-5 text-gray-400 mt-2" />
                </div>
                
                <Select value={selectedOrgType} onValueChange={setSelectedOrgType}>
                  <SelectTrigger className="w-[200px]">
                    <SelectValue placeholder="All Types" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Types</SelectItem>
                    {orgTypes.map((type) => (
                      <SelectItem key={type.UID} value={type.UID}>
                        {type.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>

                <div className="flex gap-2">
                  <Button variant="outline" size="sm" onClick={expandAll}>
                    Expand All
                  </Button>
                  <Button variant="outline" size="sm" onClick={collapseAll}>
                    Collapse All
                  </Button>
                </div>
              </div>

              {/* Hierarchy Tree */}
              <div className="border rounded-lg bg-gradient-to-br from-gray-50 to-gray-100 p-6 max-h-[700px] overflow-y-auto shadow-inner">
                {loading ? (
                  <div className="space-y-4">
                    {/* Skeleton for hierarchy nodes */}
                    {[...Array(3)].map((_, i) => (
                      <div key={i} className="space-y-3">
                        <div className="flex items-center gap-4 p-4 rounded-lg bg-white border">
                          <Skeleton className="h-5 w-5 rounded" />
                          <Skeleton className="h-10 w-10 rounded-lg" />
                          <div className="flex-1 space-y-2">
                            <div className="flex items-center gap-2">
                              <Skeleton className="h-5 w-32" />
                              <Skeleton className="h-5 w-16" />
                              <Skeleton className="h-5 w-24" />
                            </div>
                            <Skeleton className="h-4 w-48" />
                          </div>
                          <Skeleton className="h-8 w-8 rounded" />
                        </div>
                        {/* Child nodes */}
                        {i < 2 && (
                          <div className="ml-8 space-y-2">
                            {[...Array(2)].map((_, j) => (
                              <div key={j} className="flex items-center gap-4 p-3 rounded-lg bg-white border">
                                <Skeleton className="h-4 w-4 rounded" />
                                <Skeleton className="h-8 w-8 rounded-lg" />
                                <div className="flex-1 space-y-1">
                                  <div className="flex items-center gap-2">
                                    <Skeleton className="h-4 w-24" />
                                    <Skeleton className="h-4 w-12" />
                                    <Skeleton className="h-4 w-16" />
                                  </div>
                                </div>
                                <Skeleton className="h-6 w-6 rounded" />
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                ) : filteredData.length === 0 ? (
                  <div className="text-center py-12">
                    <Building2 className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                    <p className="text-gray-500 font-medium">No organizations found</p>
                    <p className="text-sm text-gray-400 mt-2">
                      {searchQuery ? "Try adjusting your search query" : "No organizations have been created yet"}
                    </p>
                  </div>
                ) : (
                  <div className="space-y-1">
                    {filteredData.map((node) => (
                      <TreeNode
                        key={node.uid}
                        node={node}
                        level={0}
                        onToggle={handleToggle}
                        expandedNodes={expandedNodes}
                        orgTypeColors={orgTypeColors}
                        organizations={organizations}
                        onRefresh={fetchData}
                      />
                    ))}
                  </div>
                )}
              </div>
            </TabsContent>

            <TabsContent value="by-type" className="space-y-4">
              {/* Statistics Cards */}
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

              {/* Type-based Hierarchy */}
              <div className="border rounded-lg bg-gradient-to-br from-gray-50 to-gray-100 p-6 max-h-[700px] overflow-y-auto shadow-inner">
                {loading ? (
                  <div className="space-y-6">
                    {/* Skeleton for type groups */}
                    {[...Array(2)].map((_, i) => (
                      <div key={i} className="space-y-4">
                        {/* Type header skeleton */}
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
                        
                        {/* Organizations under type skeleton */}
                        <div className="ml-8 space-y-3 border-l-4 border-blue-200 pl-6">
                          <Skeleton className="h-5 w-32" />
                          <div className="grid gap-2">
                            {[...Array(3)].map((_, j) => (
                              <div key={j} className="flex items-center gap-3 p-3 rounded-lg bg-white border">
                                <Skeleton className="h-8 w-8 rounded-lg" />
                                <div className="flex-1 space-y-1">
                                  <div className="flex items-center gap-2">
                                    <Skeleton className="h-4 w-28" />
                                    <Skeleton className="h-4 w-16" />
                                    <Skeleton className="h-4 w-20" />
                                  </div>
                                  <Skeleton className="h-3 w-40" />
                                </div>
                                <Skeleton className="h-5 w-12 rounded" />
                                <Skeleton className="h-6 w-6 rounded" />
                              </div>
                            ))}
                          </div>
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
                      />
                    ))}
                  </div>
                )}
              </div>
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  )
}