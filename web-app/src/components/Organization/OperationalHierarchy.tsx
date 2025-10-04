"use client"

import { useState, useEffect, useRef } from "react"
import { ChevronRight, ChevronDown, Building2, Search, Globe, Building, MapPinned, Network, Edit, Trash2, Eye, RefreshCw, Download, Plus, MoreHorizontal, Filter, ChevronDown as ChevronDownIcon, X } from "lucide-react"
import { usePagePermissions } from "@/hooks/use-page-permissions"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Skeleton } from "@/components/ui/skeleton"
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
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
  permissions: ReturnType<typeof usePagePermissions>
}

function TreeNode({ node, level, onToggle, expandedNodes, orgTypeColors, organizations, onRefresh, permissions }: TreeNodeProps) {
  const router = useRouter()
  const { toast } = useToast()
  const isExpanded = expandedNodes.has(node.uid)
  const hasChildren = node.children && node.children.length > 0
  const color = orgTypeColors.get(node.orgTypeUID) || "gray"
  
  const org = organizations.find(o => o.UID === node.uid)
  
  // Use permissions passed from parent
  const canEdit = permissions.canEdit
  const canDelete = permissions.canDelete
  
  const handleEdit = () => {
    router.push(`/administration/organization-management/organizations/create?id=${node.uid}`)
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
        
        <div className={cn(
          "p-2 rounded-lg",
          `bg-${color}-100 text-${color}-700`
        )}>
          {getIcon()}
        </div>
        
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
        
        {org && (canEdit || canDelete) && (
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
                    <DropdownMenuItem onClick={handleEdit}>
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
                    onClick={handleDelete}
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
              permissions={permissions}
            />
          ))}
        </div>
      )}
    </div>
  )
}

export function OperationalHierarchy() {
  const { toast } = useToast()
  const router = useRouter()
  
  // Get permissions dynamically based on the current page path
  const permissions = usePagePermissions()
  
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([])
  const [hierarchyData, setHierarchyData] = useState<OrganizationHierarchyNode[]>([])
  const [loading, setLoading] = useState(false)
  const [selectedOrgTypes, setSelectedOrgTypes] = useState<string[]>([])
  const [searchQuery, setSearchQuery] = useState("")
  const searchInputRef = useRef<HTMLInputElement>(null)
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set())
  const [filteredData, setFilteredData] = useState<OrganizationHierarchyNode[]>([])

  useEffect(() => {
    fetchData()
  }, [])

  useEffect(() => {
    if (organizations.length > 0) {
      buildHierarchy()
    }
  }, [organizations, selectedOrgTypes])

  useEffect(() => {
    filterHierarchy()
  }, [hierarchyData, searchQuery])

  // Add keyboard shortcut for Ctrl+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault()
        searchInputRef.current?.focus()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
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

  const buildHierarchy = () => {
    let filteredOrgs = organizations
    
    // Filter by selected organization types
    if (selectedOrgTypes.length > 0) {
      filteredOrgs = organizations.filter(org => selectedOrgTypes.includes(org.OrgTypeUID))
    }
    
    const tree = organizationService.buildOrganizationTree(filteredOrgs)
    setHierarchyData(tree)
    
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

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Operational Hierarchy</h1>
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
          {permissions.canDownload && (
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
          <CardTitle>Hierarchy View</CardTitle>
          <CardDescription>
            Explore organizational structure and relationships
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
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
                  <p className="text-sm font-medium text-gray-600">Filtered Results</p>
                  <p className="text-2xl font-bold text-green-600">{filteredData.length}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Selected Types</p>
                  <p className="text-2xl font-bold text-purple-600">
                    {selectedOrgTypes.length || 'All'}
                  </p>
                </div>
              </>
            )}
          </div>

          {/* Search and Filters */}
          <Card className="shadow-sm border-gray-200">
            <div className="p-3">
              <div className="flex gap-3">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                  <Input
                    ref={searchInputRef}
                    placeholder="Search by name or code... (Ctrl+F)"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
                  />
                </div>
                
                {/* Organization Type Filter */}
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="outline">
                      <Filter className="h-4 w-4 mr-2" />
                      Type
                      {selectedOrgTypes.length > 0 && (
                        <Badge variant="secondary" className="ml-2">
                          {selectedOrgTypes.length}
                        </Badge>
                      )}
                      <ChevronDownIcon className="h-4 w-4 ml-2" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-56">
                    <DropdownMenuLabel>Filter by Organization Type</DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    {orgTypes.map((type) => (
                      <DropdownMenuCheckboxItem
                        key={type.UID}
                        checked={selectedOrgTypes.includes(type.UID)}
                        onCheckedChange={(checked) => {
                          setSelectedOrgTypes(prev => 
                            checked 
                              ? [...prev, type.UID]
                              : prev.filter(uid => uid !== type.UID)
                          )
                        }}
                      >
                        {type.Name}
                      </DropdownMenuCheckboxItem>
                    ))}
                    {selectedOrgTypes.length > 0 && (
                      <>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          onClick={() => setSelectedOrgTypes([])}
                        >
                          <X className="h-4 w-4 mr-2" />
                          Clear Filter
                        </DropdownMenuItem>
                      </>
                    )}
                  </DropdownMenuContent>
                </DropdownMenu>

                <div className="flex gap-2">
                  <Button variant="outline" size="sm" onClick={expandAll}>
                    Expand All
                  </Button>
                  <Button variant="outline" size="sm" onClick={collapseAll}>
                    Collapse All
                  </Button>
                </div>
                
                <Button
                  variant="outline"
                  size="default"
                  onClick={() => fetchData()}
                  disabled={loading}
                >
                  <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
                </Button>
              </div>
            </div>
          </Card>

          <div className="border rounded-lg bg-gradient-to-br from-gray-50 to-gray-100 p-6 max-h-[700px] overflow-y-auto shadow-inner">
            {loading ? (
              <div className="space-y-4">
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