'use client'

import { useState, useEffect } from 'react'
import { ChevronRight, ChevronDown, Package, Search, Filter, RefreshCw, Download, Plus, MoreHorizontal, Layers, FolderTree, BarChart3, Eye, Edit, Trash2, TreePine, Tags, Link2, Database, Settings } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Skeleton } from '@/components/ui/skeleton'
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useToast } from '@/components/ui/use-toast'
import { Badge } from '@/components/ui/badge'
import { useRouter } from 'next/navigation'
import { skuService, SKU, SKUGroup, SKUGroupType, SKUToGroupMapping, PagedResponse } from '@/services/sku/sku.service'
import { PagingRequest } from '@/types/common.types'
import { cn } from '@/lib/utils'

interface ProductHierarchyNode {
  uid: string
  code: string
  name: string
  type: 'group' | 'product'
  level: number
  parentUID?: string
  groupTypeUID?: string
  groupTypeName?: string
  children: ProductHierarchyNode[]
  productCount?: number
  isActive?: boolean
}

interface TreeNodeProps {
  node: ProductHierarchyNode
  level: number
  onToggle: (uid: string) => void
  expandedNodes: Set<string>
  onRefresh: () => void
  colorMap: Map<string, string>
  onViewProducts?: (groupUID: string, groupName: string) => void
}

function TreeNode({ node, level, onToggle, expandedNodes, onRefresh, colorMap, onViewProducts }: TreeNodeProps) {
  const router = useRouter()
  const { toast } = useToast()
  const isExpanded = expandedNodes.has(node.uid)
  const hasChildren = node.children && node.children.length > 0
  const color = colorMap.get(node.groupTypeUID || 'default') || 'blue'
  
  const handleEdit = () => {
    if (node.type === 'product') {
      router.push(`/updatedfeatures/sku-management/products/edit?uid=${node.uid}`)
    } else {
      router.push(`/updatedfeatures/sku-management/groups/manage?uid=${node.uid}`)
    }
  }
  
  const handleView = () => {
    if (node.type === 'product') {
      router.push(`/updatedfeatures/sku-management/products/view?uid=${node.uid}`)
    }
  }

  const getIcon = () => {
    if (node.type === 'product') {
      return <Package className="h-4 w-4 text-purple-600" />
    }
    switch (level) {
      case 0: return <Layers className="h-4 w-4 text-blue-600" />
      case 1: return <FolderTree className="h-4 w-4 text-green-600" />
      default: return <BarChart3 className="h-4 w-4 text-orange-600" />
    }
  }

  return (
    <div className={cn(level > 0 && "border-l-2 ml-4", `border-${color}-200`)}>
      <div
        className={cn(
          "group flex items-center gap-3 py-3 px-4 rounded-lg transition-all duration-200",
          "hover:shadow-md hover:scale-[1.01] cursor-pointer",
          node.type === 'product' ? "shadow-sm" : level === 0 ? "shadow-lg" : level === 1 ? "shadow-md" : "shadow-sm",
          node.type === 'product' ? `bg-purple-50 border border-purple-300` : `bg-${color}-50 border border-${color}-300`,
          level === 0 && node.type !== 'product' && "border-2"
        )}
        style={{ marginLeft: `${level * 16}px` }}
      >
        {/* Expand/Collapse Button */}
        {hasChildren && (
          <button
            onClick={() => onToggle(node.uid)}
            className={cn(
              "p-1.5 rounded-md transition-all hover:bg-white/80",
              node.type === 'product' ? 'text-purple-700' : `text-${color}-700`
            )}
          >
            {isExpanded ? (
              <ChevronDown className="h-5 w-5" />
            ) : (
              <ChevronRight className="h-5 w-5" />
            )}
          </button>
        )}
        {!hasChildren && <div className="w-8" />}
        
        {/* Icon */}
        <div className={cn(
          "p-2 rounded-lg",
          node.type === 'product' ? 'bg-purple-100 text-purple-700' : `bg-${color}-100 text-${color}-700`
        )}>
          {getIcon()}
        </div>
        
        {/* Details */}
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h4 className="font-semibold text-gray-900">{node.name}</h4>
            <Badge 
              variant="outline" 
              className={cn('text-xs font-mono', 
                node.type === 'product' ? 'border-purple-300 text-purple-700' : `border-${color}-300 text-${color}-700`
              )}
            >
              {node.code}
            </Badge>
            {node.type === 'group' && (
              <Badge 
                className={cn(
                  "text-xs",
                  `bg-${color}-200 text-${color}-800 border-${color}-300`
                )}
              >
                {node.groupTypeName || `Level ${node.level}`}
              </Badge>
            )}
            {node.type === 'product' && (
              <Badge className="text-xs bg-purple-200 text-purple-800">
                Product
              </Badge>
            )}
          </div>
          {hasChildren && (
            <p className="text-sm text-gray-500 mt-1">
              {node.productCount ? `${node.productCount} products, ` : ''}
              {node.children.filter(c => c.type === 'group').length} subcategories
            </p>
          )}
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
              {node.type === 'product' && (
                <>
                  <DropdownMenuItem onClick={handleView}>
                    <Eye className="mr-2 h-4 w-4" />
                    View Details
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={handleEdit}>
                    <Edit className="mr-2 h-4 w-4" />
                    Edit Product
                  </DropdownMenuItem>
                </>
              )}
              {node.type === 'group' && (
                <>
                  <DropdownMenuItem 
                    onClick={() => onViewProducts && onViewProducts(node.uid, node.name)}
                  >
                    <Package className="mr-2 h-4 w-4" />
                    View Products ({node.productCount || 0})
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleEdit}>
                    <Edit className="mr-2 h-4 w-4" />
                    Edit Group
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => router.push(`/updatedfeatures/sku-management/products/create?groupId=${node.uid}`)}>
                    <Plus className="mr-2 h-4 w-4" />
                    Add Product
                  </DropdownMenuItem>
                </>
              )}
              <DropdownMenuSeparator />
              <DropdownMenuItem className="text-red-600 hover:text-red-700 hover:bg-red-50">
                <Trash2 className="mr-2 h-4 w-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
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
              onRefresh={onRefresh}
              colorMap={colorMap}
              onViewProducts={onViewProducts}
            />
          ))}
        </div>
      )}
    </div>
  )
}

export function ProductHierarchy() {
  const { toast } = useToast()
  const router = useRouter()
  
  const [skus, setSKUs] = useState<SKU[]>([])
  const [groups, setGroups] = useState<SKUGroup[]>([])
  const [groupTypes, setGroupTypes] = useState<SKUGroupType[]>([])
  const [mappings, setMappings] = useState<SKUToGroupMapping[]>([])
  const [hierarchyData, setHierarchyData] = useState<ProductHierarchyNode[]>([])
  const [loading, setLoading] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedGroupType, setSelectedGroupType] = useState<string>("all")
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set())
  const [filteredData, setFilteredData] = useState<ProductHierarchyNode[]>([])
  const [activeView, setActiveView] = useState<"hierarchy" | "by-type">("hierarchy")
  const [showProducts, setShowProducts] = useState(false)
  const [selectedGroupForProducts, setSelectedGroupForProducts] = useState<string | null>(null)
  const [selectedGroupProducts, setSelectedGroupProducts] = useState<any[]>([])
  const [loadingGroupProducts, setLoadingGroupProducts] = useState(false)

  useEffect(() => {
    fetchData()
  }, [])

  useEffect(() => {
    if (groups.length > 0 || skus.length > 0) {
      buildHierarchy()
    }
  }, [groups, skus, mappings, selectedGroupType, showProducts])

  useEffect(() => {
    filterHierarchy()
  }, [hierarchyData, searchQuery])

  const fetchData = async () => {
    setLoading(true)
    try {
      // Fetch all data in parallel
      const [groupsResponse, skusResponse, groupTypesResponse, mappingsResponse] = await Promise.all([
        skuService.getAllSKUGroups({
          PageNumber: 1,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
          IsCountRequired: false
        }),
        showProducts ? skuService.getAllSKUs({
          PageNumber: 1,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
          IsCountRequired: false
        }) : Promise.resolve({ PagedData: [] }),
        skuService.getAllSKUGroupTypes({
          PageNumber: 1,
          PageSize: 100,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
          IsCountRequired: false
        }),
        skuService.getSKUToGroupMappings({
          PageNumber: 1,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [],
          IsCountRequired: false
        })
      ])

      console.log('Groups Response:', groupsResponse)
      console.log('SKUs Response:', skusResponse)
      console.log('Group Types Response:', groupTypesResponse)
      console.log('Mappings Response:', mappingsResponse)
      
      setGroups(groupsResponse.PagedData || [])
      setSKUs(skusResponse.PagedData?.map(sku => ({
        ...sku,
        UID: sku.SKUUID,
        Name: sku.SKULongName,
        Code: sku.SKUCode
      } as any)) || [])
      setGroupTypes(groupTypesResponse.PagedData || [])
      setMappings(mappingsResponse.PagedData || [])
    } catch (error) {
      console.error('Error fetching hierarchy data:', error)
      toast({
        title: "Error",
        description: "Failed to fetch product hierarchy data",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const buildHierarchy = () => {
    const nodeMap = new Map<string, ProductHierarchyNode>()
    const rootNodes: ProductHierarchyNode[] = []

    console.log('Building hierarchy with groups:', groups)
    console.log('Show products:', showProducts)
    console.log('SKUs available:', skus.length)

    // Create nodes for all groups
    groups.forEach(group => {
      const groupType = groupTypes.find(gt => gt.UID === group.SKUGroupTypeUID)
      nodeMap.set(group.UID, {
        uid: group.UID,
        code: group.Code,
        name: group.Name,
        type: 'group',
        level: group.ItemLevel,
        parentUID: group.ParentUID,
        groupTypeUID: group.SKUGroupTypeUID,
        groupTypeName: groupType?.Name,
        children: [],
        productCount: 0
      })
    })

    // Create nodes for products if enabled
    if (showProducts) {
      skus.forEach(sku => {
        const mapping = mappings.find(m => m.SKUUID === sku.UID)
        if (mapping) {
          nodeMap.set(sku.UID, {
            uid: sku.UID,
            code: sku.Code,
            name: sku.Name,
            type: 'product',
            level: 999, // Products are always at the deepest level
            parentUID: mapping.SKUGroupUID,
            children: [],
            isActive: sku.IsActive
          })
        }
      })
    }

    // Build tree structure
    nodeMap.forEach(node => {
      if (node.parentUID && nodeMap.has(node.parentUID)) {
        const parent = nodeMap.get(node.parentUID)!
        parent.children.push(node)
        if (node.type === 'product') {
          // Count products up the tree
          let currentParent = parent
          while (currentParent) {
            currentParent.productCount = (currentParent.productCount || 0) + 1
            currentParent = currentParent.parentUID ? nodeMap.get(currentParent.parentUID) : undefined
          }
        }
      } else if (!node.parentUID && node.type === 'group') {
        rootNodes.push(node)
      }
    })

    // Sort children
    const sortChildren = (nodes: ProductHierarchyNode[]) => {
      nodes.forEach(node => {
        node.children.sort((a, b) => {
          // Groups before products
          if (a.type !== b.type) {
            return a.type === 'group' ? -1 : 1
          }
          return a.name.localeCompare(b.name)
        })
        if (node.children.length > 0) {
          sortChildren(node.children)
        }
      })
    }
    sortChildren(rootNodes)

    setHierarchyData(rootNodes)
    
    console.log('Root nodes found:', rootNodes.length)
    console.log('Total nodes in map:', nodeMap.size)
    
    // Auto-expand first level
    const firstLevelIds = new Set(rootNodes.map(node => node.uid))
    setExpandedNodes(firstLevelIds)
  }

  const filterHierarchy = () => {
    if (!searchQuery) {
      setFilteredData(hierarchyData)
      return
    }

    const query = searchQuery.toLowerCase()
    
    const filterNodes = (nodes: ProductHierarchyNode[]): ProductHierarchyNode[] => {
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
      }, [] as ProductHierarchyNode[])
    }

    const filtered = filterNodes(hierarchyData)
    setFilteredData(filtered)
    
    // Expand all nodes when searching
    if (searchQuery) {
      const allNodeIds = new Set<string>()
      const collectIds = (nodes: ProductHierarchyNode[]) => {
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

  const fetchProductsForGroup = async (groupUID: string, groupName: string) => {
    console.log('Fetching products for group:', groupUID, groupName)
    setLoadingGroupProducts(true)
    setSelectedGroupForProducts(groupUID)
    
    try {
      // Get all mappings and filter for this group
      const groupMappings = mappings.filter(mapping => mapping.SKUGroupUID === groupUID)
      console.log('Found mappings for', groupName, ':', groupMappings.length)
      
      if (groupMappings.length > 0) {
        // Get SKU UIDs
        const skuUIDs = groupMappings.map(mapping => mapping.SKUUID)
        console.log('SKU UIDs to fetch:', skuUIDs)
        
        // Fetch all SKUs and filter by UIDs
        const skusResponse = await skuService.getAllSKUs({
          PageNumber: 1,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
          IsCountRequired: false
        })
        
        // Filter SKUs that match our UIDs
        const filteredSKUs = skusResponse.PagedData.filter(sku => 
          skuUIDs.includes(sku.SKUUID)
        )
        
        console.log('Found products for', groupName, ':', filteredSKUs.length)
        setSelectedGroupProducts(filteredSKUs)
      } else {
        console.log('No products found for', groupName)
        setSelectedGroupProducts([])
      }
    } catch (error) {
      console.error('Error fetching products for group:', error)
      toast({
        title: 'Error',
        description: `Failed to fetch products for ${groupName}`,
        variant: 'destructive'
      })
      setSelectedGroupProducts([])
    } finally {
      setLoadingGroupProducts(false)
    }
  }

  const expandAll = () => {
    const allNodeIds = new Set<string>()
    const collectIds = (nodes: ProductHierarchyNode[]) => {
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

  const exportHierarchy = () => {
    const data = JSON.stringify(hierarchyData, null, 2)
    const blob = new Blob([data], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'product-hierarchy.json'
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  }

  // Generate colors for group types
  const getColorMap = (): Map<string, string> => {
    const colors = new Map<string, string>()
    const colorsByLevel = ["blue", "green", "orange", "pink", "indigo", "teal"]
    
    groupTypes.forEach((type, index) => {
      colors.set(type.UID, colorsByLevel[index % colorsByLevel.length])
    })
    colors.set('default', 'gray')
    
    return colors
  }

  const colorMap = getColorMap()

  // Calculate statistics
  const totalProducts = skus.length
  const totalGroups = groups.length
  const rootGroups = hierarchyData.length
  const activeProducts = skus.filter(s => s.IsActive).length

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Hierarchy</h1>
          <p className="text-muted-foreground">
            Visualize and navigate through your product catalog structure
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
            onClick={exportHierarchy}
          >
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button onClick={() => router.push('/updatedfeatures/sku-management/products/create')}>
            <Plus className="mr-2 h-4 w-4" />
            Add Product
          </Button>
        </div>
      </div>

      {/* Quick Navigation to New Management Pages */}
      <Card className="p-6 bg-gradient-to-r from-green-50 to-emerald-50 border-green-200">
        <div className="mb-4">
          <h2 className="text-lg font-semibold mb-2 flex items-center gap-2">
            <Settings className="h-5 w-5 text-green-600" />
            Advanced SKU Management Tools
          </h2>
          <p className="text-sm text-muted-foreground">
            Professional-grade management suite with comprehensive CRUD operations and live data
          </p>
        </div>
        
        <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
          <Button 
            variant="outline" 
            className="flex items-center gap-2 h-auto p-3 flex-col justify-start"
            onClick={() => router.push('/updatedfeatures/sku-management/group-types')}
          >
            <TreePine className="h-5 w-5 text-green-600" />
            <div className="text-center">
              <div className="font-medium">Group Types</div>
              <div className="text-xs text-muted-foreground">Define hierarchy types</div>
            </div>
          </Button>
          
          <Button 
            variant="outline" 
            className="flex items-center gap-2 h-auto p-3 flex-col justify-start"
            onClick={() => router.push('/updatedfeatures/sku-management/groups')}
          >
            <Tags className="h-5 w-5 text-purple-600" />
            <div className="text-center">
              <div className="font-medium">Groups</div>
              <div className="text-xs text-muted-foreground">Manage actual groups</div>
            </div>
          </Button>
          
          <Button 
            variant="outline" 
            className="flex items-center gap-2 h-auto p-3 flex-col justify-start"
            onClick={() => router.push('/updatedfeatures/sku-management/mappings')}
          >
            <Link2 className="h-5 w-5 text-orange-600" />
            <div className="text-center">
              <div className="font-medium">Mappings</div>
              <div className="text-xs text-muted-foreground">SKU-Group links</div>
            </div>
          </Button>
          
          <Button 
            variant="outline" 
            className="flex items-center gap-2 h-auto p-3 flex-col justify-start"
            onClick={() => router.push('/debug/api-investigation')}
          >
            <Database className="h-5 w-5 text-red-600" />
            <div className="text-center">
              <div className="font-medium">API Tools</div>
              <div className="text-xs text-muted-foreground">Debug & test APIs</div>
            </div>
          </Button>
        </div>

        <div className="mt-4 p-3 bg-white rounded-lg border border-green-200">
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center gap-4">
              <span className="text-green-600 font-medium">🚀 Complete Suite Ready</span>
              <span className="text-blue-600 font-medium">📊 Real-time Data</span>
              <span className="text-purple-600 font-medium">🔧 Professional Tools</span>
              <span className="text-orange-600 font-medium">⚡ Live Integration</span>
            </div>
            <Button 
              variant="ghost" 
              size="sm"
              onClick={() => router.push('/updatedfeatures/sku-management')}
              className="text-green-600 hover:text-green-800"
            >
              Main Dashboard →
            </Button>
          </div>
        </div>
      </Card>

      {/* Controls */}
      <Card>
        <CardHeader>
          <CardTitle>Hierarchy View</CardTitle>
          <CardDescription>
            Explore your product catalog structure and relationships
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Statistics Cards */}
          <div className="grid grid-cols-4 gap-4">
            {loading ? (
              [...Array(4)].map((_, i) => (
                <div key={i} className="bg-white p-4 rounded-lg shadow-sm border">
                  <Skeleton className="h-4 w-32 mb-2" />
                  <Skeleton className="h-8 w-12" />
                </div>
              ))
            ) : (
              <>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Total Products</p>
                  <p className="text-2xl font-bold text-blue-600">{totalProducts}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Product Groups</p>
                  <p className="text-2xl font-bold text-green-600">{totalGroups}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Root Categories</p>
                  <p className="text-2xl font-bold text-purple-600">{rootGroups}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border">
                  <p className="text-sm font-medium text-gray-600">Active Products</p>
                  <p className="text-2xl font-bold text-orange-600">{activeProducts}</p>
                </div>
              </>
            )}
          </div>

          {/* Filters */}
          <div className="flex flex-col gap-4 md:flex-row md:items-center">
            <div className="flex-1 flex gap-2">
              <Input
                placeholder="Search products or groups..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="max-w-sm"
              />
              <Search className="h-5 w-5 text-gray-400 mt-2" />
            </div>
            
            <Select value={selectedGroupType} onValueChange={setSelectedGroupType}>
              <SelectTrigger className="w-[200px]">
                <SelectValue placeholder="All Types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                {groupTypes.map((type) => (
                  <SelectItem key={type.UID} value={type.UID}>
                    {type.Name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <div className="flex gap-2">
              <Button
                variant={showProducts ? "default" : "outline"}
                size="sm"
                onClick={() => {
                  console.log('Show Products clicked, current state:', showProducts)
                  setShowProducts(!showProducts)
                }}
              >
                {showProducts ? "Hide Products" : "Show Products"}
              </Button>
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
                <Package className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 font-medium">No products or groups found</p>
                <p className="text-sm text-gray-400 mt-2">
                  {searchQuery ? "Try adjusting your search query" : "No product hierarchy has been created yet"}
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
                    onRefresh={fetchData}
                    colorMap={colorMap}
                    onViewProducts={fetchProductsForGroup}
                  />
                ))}
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Selected Group Products */}
      {selectedGroupForProducts && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Package className="h-5 w-5" />
              Products in Selected Group
            </CardTitle>
            <CardDescription>
              {selectedGroupProducts.length} product{selectedGroupProducts.length !== 1 ? 's' : ''} found
            </CardDescription>
          </CardHeader>
          <CardContent>
            {loadingGroupProducts ? (
              <div className="text-center py-8">
                <p className="text-muted-foreground">Loading products...</p>
              </div>
            ) : selectedGroupProducts.length === 0 ? (
              <div className="text-center py-8">
                <Package className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                <p className="text-muted-foreground">No products found in this group</p>
              </div>
            ) : (
              <div className="grid gap-3">
                {selectedGroupProducts.map((sku) => (
                  <div
                    key={sku.SKUUID}
                    className="flex items-center gap-3 p-3 border rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    <div className="p-2 bg-purple-100 rounded-lg">
                      <Package className="h-4 w-4 text-purple-600" />
                    </div>
                    
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <h4 className="font-medium">{sku.SKULongName}</h4>
                        <Badge variant="outline" className="text-xs font-mono">
                          {sku.SKUCode}
                        </Badge>
                      </div>
                      {sku.AliasName && (
                        <p className="text-sm text-muted-foreground">{sku.AliasName}</p>
                      )}
                    </div>
                    
                    <Badge variant={sku.IsActive ? "default" : "secondary"}>
                      {sku.IsActive ? 'Active' : 'Inactive'}
                    </Badge>
                    
                    <div className="flex gap-1">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => router.push(`/updatedfeatures/sku-management/products/view?uid=${sku.SKUUID}`)}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => router.push(`/updatedfeatures/sku-management/products/edit?uid=${sku.SKUUID}`)}
                      >
                        <Edit className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            )}
            
            <div className="mt-4 pt-4 border-t">
              <Button
                variant="outline"
                onClick={() => {
                  setSelectedGroupForProducts(null)
                  setSelectedGroupProducts([])
                }}
              >
                Close Product List
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

export default ProductHierarchy