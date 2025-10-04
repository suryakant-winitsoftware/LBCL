"use client"

import { useState, useEffect, useRef } from "react"
import { ChevronRight, ChevronDown, MapPin, Search, Globe, Building, MapPinned, Edit, Trash2, Eye, MoreHorizontal, Plus, RefreshCw, Download, Filter, ChevronDown as ChevronDownIcon, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
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
  DropdownMenuCheckboxItem,
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
    router.push(`/administration/location-management/locations/create?id=${node.uid}`)
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
                <DropdownMenuItem onClick={() => router.push(`/administration/location-management/locations/${location.UID}`)}>
                  <Eye className="mr-2 h-4 w-4" />
                  View Details
                </DropdownMenuItem>
                <DropdownMenuItem 
                  onClick={(e) => {
                    e.stopPropagation()
                    router.push(`/administration/location-management/locations/create?parentUID=${location.UID}`)
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
  const [selectedLocationTypes, setSelectedLocationTypes] = useState<string[]>([])
  const [searchQuery, setSearchQuery] = useState("")
  const searchInputRef = useRef<HTMLInputElement>(null)
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
  }, [locations, selectedLocationTypes])

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

  const fetchLocationTypes = async () => {
    try {
      const types = await locationService.getLocationTypes()
      // Filter to show only location types where ShowInUI is true (same as organization hierarchy)
      const visibleTypes = types.filter(type => type.ShowInUI !== false)
      setLocationTypes(visibleTypes.sort((a, b) => a.LevelNo - b.LevelNo))
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
    let filteredLocations = locations
    
    // Filter by selected location types
    if (selectedLocationTypes.length > 0) {
      filteredLocations = locations.filter(location => 
        selectedLocationTypes.includes(location.LocationTypeUID || '')
      )
    }
    
    const tree = locationService.buildLocationTree(filteredLocations)
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
            onClick={async () => {
              try {
                const blob = await locationService.exportLocationHierarchy("csv", searchQuery);
                const url = URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = `location-hierarchy-export-${new Date().toISOString().split("T")[0]}.csv`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
                
                toast({
                  title: "Success",
                  description: "Location hierarchy exported successfully.",
                });
              } catch (error) {
                toast({
                  title: "Error",
                  description: "Failed to export location hierarchy. Please try again.",
                  variant: "destructive",
                });
              }
            }}
          >
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button onClick={() => router.push('/administration/location-management/locations/create')}>
            <Plus className="mr-2 h-4 w-4" />
            Add Location
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Hierarchy View</CardTitle>
          <CardDescription>
            Explore location structure and relationships
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
                  <p className="text-sm font-medium text-gray-600">Total Locations</p>
                  <p className="text-2xl font-bold text-blue-600">{locations.length}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Filtered Results</p>
                  <p className="text-2xl font-bold text-green-600">{filteredData.length}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Selected Types</p>
                  <p className="text-2xl font-bold text-purple-600">
                    {selectedLocationTypes.length || 'All'}
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
                
                {/* Location Type Filter */}
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="outline">
                      <Filter className="h-4 w-4 mr-2" />
                      Type
                      {selectedLocationTypes.length > 0 && (
                        <Badge variant="secondary" className="ml-2">
                          {selectedLocationTypes.length}
                        </Badge>
                      )}
                      <ChevronDownIcon className="h-4 w-4 ml-2" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-56">
                    <DropdownMenuLabel>Filter by Location Type</DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    {locationTypes.map((type) => (
                      <DropdownMenuCheckboxItem
                        key={type.UID}
                        checked={selectedLocationTypes.includes(type.UID)}
                        onCheckedChange={(checked) => {
                          setSelectedLocationTypes(prev => 
                            checked 
                              ? [...prev, type.UID]
                              : prev.filter(uid => uid !== type.UID)
                          )
                        }}
                      >
                        {type.Name}
                      </DropdownMenuCheckboxItem>
                    ))}
                    {selectedLocationTypes.length > 0 && (
                      <>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          onClick={() => setSelectedLocationTypes([])}
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
                  onClick={() => fetchLocations()}
                  disabled={loading}
                >
                  <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
                </Button>
              </div>
            </div>
          </Card>

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