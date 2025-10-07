"use client"

import { useState, useEffect } from "react"
import { ChevronRight, ChevronDown, MapPin, Search, Globe, Building, MapPinned, Edit, Trash2, Eye, MoreHorizontal, Plus, RefreshCw, Download } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"
import { locationService, Location, LocationHierarchyNode, LocationType } from "@/services/locationService"
import { cn } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { useRouter } from "next/navigation"

interface TreeNodeProps {
  node: LocationHierarchyNode
  level: number
  onToggle: (uid: string) => void
  expandedNodes: Set<string>
  locations: Location[]
  onRefresh: () => void
}

function TreeNode({ node, level, onToggle, expandedNodes, locations, onRefresh }: TreeNodeProps) {
  const router = useRouter()
  const { toast } = useToast()
  const isExpanded = expandedNodes.has(node.uid)
  const hasChildren = node.children && node.children.length > 0
  
  // Find the full location object
  const location = locations.find(l => l.UID === node.uid)
  
  const handleEdit = () => {
    router.push(`/updatedfeatures/location-management/locations/create?id=${node.uid}`)
  }
  
  const handleDelete = async () => {
    if (!confirm(`Are you sure you want to delete ${node.name}?`)) return
    
    try {
      await locationService.deleteLocation(node.uid)
      toast({
        title: "Success",
        description: "Location deleted successfully"
      })
      onRefresh()
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete location",
        variant: "destructive"
      })
    }
  }
  
  // Get location type color
  const getLocationTypeColor = (typeName: string) => {
    switch (typeName?.toLowerCase()) {
      case 'country': return 'blue'
      case 'state': case 'region': return 'green'
      case 'city': case 'district': return 'purple'
      case 'area': case 'zone': return 'orange'
      default: return 'gray'
    }
  }
  
  const color = getLocationTypeColor(node.locationTypeName)
  
  // Get icon based on location type
  const getLocationIcon = () => {
    switch (node.locationTypeName?.toLowerCase()) {
      case 'country': return <Globe className={`h-4 w-4 text-${color}-600`} />
      case 'state': case 'region': return <Building className={`h-4 w-4 text-${color}-600`} />
      case 'city': case 'district': return <MapPinned className={`h-4 w-4 text-${color}-600`} />
      default: return <MapPin className={`h-4 w-4 text-${color}-600`} />
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
        
        {/* Location Icon */}
        <div className={cn(
          "p-2 rounded-lg",
          `bg-${color}-100 text-${color}-700`
        )}>
          {getLocationIcon()}
        </div>
        
        {/* Location Details */}
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
              {node.locationTypeName}
            </Badge>
            {level === 0 && (
              <Badge variant="default" className="text-xs">
                Root
              </Badge>
            )}
          </div>
          {hasChildren && (
            <p className="text-sm text-gray-500 mt-1">
              {node.children.length} child location{node.children.length !== 1 ? 's' : ''}
            </p>
          )}
        </div>
        
        {/* Action Menu */}
        {location && (
          <div className="opacity-0 group-hover:opacity-100 transition-opacity duration-200">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-8 w-8 p-0"
                  onClick={(e) => e.stopPropagation()}
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
                <DropdownMenuItem onClick={() => router.push(`/updatedfeatures/location-management/locations/${location.UID}`)}>
                  <Eye className="mr-2 h-4 w-4" />
                  View Details
                </DropdownMenuItem>
                <DropdownMenuItem 
                  onClick={(e) => {
                    e.stopPropagation()
                    router.push(`/updatedfeatures/location-management/locations/create?parentUID=${location.UID}`)
                  }}
                >
                  <Plus className="mr-2 h-4 w-4" />
                  Add Child Location
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
              locations={locations}
              onRefresh={onRefresh}
            />
          ))}
        </div>
      )}
    </div>
  )
}

export function LocationHierarchy() {
  const { toast } = useToast()
  const router = useRouter()
  
  const [locations, setLocations] = useState<Location[]>([])
  const [hierarchyData, setHierarchyData] = useState<LocationHierarchyNode[]>([])
  const [locationTypes, setLocationTypes] = useState<LocationType[]>([])
  const [loading, setLoading] = useState(false)
  const [selectedType, setSelectedType] = useState<string>("all")
  const [searchQuery, setSearchQuery] = useState("")
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set())
  const [filteredData, setFilteredData] = useState<LocationHierarchyNode[]>([])

  useEffect(() => {
    fetchLocationTypes()
    fetchLocations()
  }, [])

  useEffect(() => {
    if (locations.length > 0) {
      buildHierarchy()
    }
  }, [locations, selectedType])

  useEffect(() => {
    filterHierarchy()
  }, [hierarchyData, searchQuery])

  const fetchLocationTypes = async () => {
    try {
      const types = await locationService.getLocationTypes()
      setLocationTypes(types.sort((a, b) => a.LevelNo - b.LevelNo))
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch location types",
        variant: "destructive"
      })
    }
  }

  const fetchLocations = async () => {
    setLoading(true)
    try {
      const data = await locationService.getLocationHierarchy()
      setLocations(data)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch locations",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const buildHierarchy = () => {
    const tree = locationService.buildLocationTree(
      locations, 
      selectedType === "all" ? undefined : selectedType
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
    
    const filterNodes = (nodes: LocationHierarchyNode[]): LocationHierarchyNode[] => {
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
      }, [] as LocationHierarchyNode[])
    }

    const filtered = filterNodes(hierarchyData)
    setFilteredData(filtered)
    
    // Expand all nodes when searching
    if (searchQuery) {
      const allNodeIds = new Set<string>()
      const collectIds = (nodes: LocationHierarchyNode[]) => {
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
    const collectIds = (nodes: LocationHierarchyNode[]) => {
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

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Location Hierarchy</h1>
          <p className="text-muted-foreground">
            Visualize and navigate through your geographical location structure
          </p>
        </div>
        <div className="flex gap-2">
          <Button 
            variant="outline" 
            onClick={() => fetchLocations()}
            disabled={loading}
          >
            <RefreshCw className={cn("mr-2 h-4 w-4", loading && "animate-spin")} />
            Refresh
          </Button>
          <Button 
            variant="outline" 
            onClick={() => {
              const data = JSON.stringify(hierarchyData, null, 2)
              const blob = new Blob([data], { type: 'application/json' })
              const url = URL.createObjectURL(blob)
              const a = document.createElement('a')
              a.href = url
              a.download = 'location-hierarchy.json'
              document.body.appendChild(a)
              a.click()
              document.body.removeChild(a)
              URL.revokeObjectURL(url)
            }}
          >
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button onClick={() => router.push('/updatedfeatures/location-management/locations/create')}>
            <Plus className="mr-2 h-4 w-4" />
            Add Location
          </Button>
        </div>
      </div>

      {/* Controls */}
      <Card className="shadow-lg">
        <CardHeader className="bg-gradient-to-r from-blue-50 to-blue-100 border-b">
          <CardTitle className="text-xl">Location Hierarchy</CardTitle>
          <CardDescription className="text-gray-600">
            Expand and collapse to navigate through geographical location levels
          </CardDescription>
          {/* Statistics Cards */}
          <div className="grid grid-cols-3 gap-4 mt-4">
            {loading ? (
              [...Array(3)].map((_, i) => (
                <div key={i} className="bg-white p-3 rounded-lg shadow-sm">
                  <Skeleton className="h-4 w-24 mb-2" />
                  <Skeleton className="h-8 w-12" />
                </div>
              ))
            ) : (
              <>
                <div className="bg-white p-3 rounded-lg shadow-sm">
                  <p className="text-sm font-medium text-gray-600">Total Locations</p>
                  <p className="text-2xl font-bold text-blue-600">{locations.length}</p>
                </div>
                <div className="bg-white p-3 rounded-lg shadow-sm">
                  <p className="text-sm font-medium text-gray-600">Root Locations</p>
                  <p className="text-2xl font-bold text-green-600">{hierarchyData.length}</p>
                </div>
                <div className="bg-white p-3 rounded-lg shadow-sm">
                  <p className="text-sm font-medium text-gray-600">Location Types</p>
                  <p className="text-2xl font-bold text-purple-600">{locationTypes.length}</p>
                </div>
              </>
            )}
          </div>
        </CardHeader>
        <CardContent className="p-6 space-y-4">
          {/* Filters */}
          <div className="flex flex-col gap-4 md:flex-row md:items-center">
            <div className="flex-1 flex gap-2">
              <Input
                placeholder="Search locations..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="max-w-sm"
              />
              <Search className="h-5 w-5 text-gray-400 mt-2" />
            </div>
            
            <Select value={selectedType} onValueChange={setSelectedType}>
              <SelectTrigger className="w-[200px]">
                <SelectValue placeholder="All Types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                {locationTypes.map((type) => (
                  <SelectItem key={type.UID} value={type.UID}>
                    {type.Name} (Level {type.LevelNo})
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
                {/* Skeleton for location hierarchy */}
                {[...Array(4)].map((_, i) => (
                  <div key={i} className="space-y-2">
                    <div className="flex items-center gap-3 py-2 px-3 rounded-md bg-white border">
                      <Skeleton className="h-4 w-4" />
                      <Skeleton className="h-4 w-4" />
                      <div className="flex-1 flex items-center gap-4">
                        <Skeleton className="h-4 w-32" />
                        <Skeleton className="h-4 w-16 rounded" />
                        <Skeleton className="h-4 w-20 rounded" />
                      </div>
                    </div>
                    {/* Child locations */}
                    {i < 2 && (
                      <div className="ml-6 space-y-1">
                        {[...Array(3)].map((_, j) => (
                          <div key={j} className="flex items-center gap-3 py-2 px-3 rounded-md bg-white border opacity-75">
                            <Skeleton className="h-3 w-3" />
                            <Skeleton className="h-3 w-3" />
                            <div className="flex-1 flex items-center gap-3">
                              <Skeleton className="h-3 w-24" />
                              <Skeleton className="h-3 w-12 rounded" />
                              <Skeleton className="h-3 w-16 rounded" />
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : filteredData.length === 0 ? (
              <div className="text-center py-12">
                <MapPin className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 font-medium">No locations found</p>
                <p className="text-sm text-gray-400 mt-2">
                  {searchQuery ? "Try adjusting your search query" : "No locations have been created yet"}
                </p>
              </div>
            ) : (
              <div className="space-y-3">
                {filteredData.map((node) => (
                  <TreeNode
                    key={node.uid}
                    node={node}
                    level={0}
                    onToggle={handleToggle}
                    expandedNodes={expandedNodes}
                    locations={locations}
                    onRefresh={fetchLocations}
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