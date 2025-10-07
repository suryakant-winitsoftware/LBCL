'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Label } from '@/components/ui/label'
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { ArrowLeft, ChevronRight, ChevronDown, FolderOpen, Folder, Package, Eye, ExternalLink, MoreVertical } from 'lucide-react'
import { skuService, SKUGroup, SKUToGroupMapping, PagedResponse } from '@/services/sku/sku.service'
import { PagingRequest } from '@/types/common.types'
import { useToast } from '@/components/ui/use-toast'

interface HierarchicalGroup extends SKUGroup {
  children: HierarchicalGroup[]
  skuCount?: number
  expanded?: boolean
  typeName?: string
  products?: any[]
  loadingProducts?: boolean
}

export default function GroupHierarchyPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [groups, setGroups] = useState<SKUGroup[]>([])
  const [groupTypes, setGroupTypes] = useState<any[]>([])
  const [hierarchicalGroups, setHierarchicalGroups] = useState<HierarchicalGroup[]>([])
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set())
  const [loading, setLoading] = useState(true)
  const [productCounts, setProductCounts] = useState<Map<string, number>>(new Map())
  const [expandedProducts, setExpandedProducts] = useState<Set<string>>(new Set())


  const fetchProductCounts = async () => {
    try {
      // Fetch SKU to group mappings to count products per group
      const mappingsRequest: PagingRequest = {
        PageNumber: 1,
        PageSize: 1000,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      }
      
      const mappingsResponse = await skuService.getSKUToGroupMappings(mappingsRequest)
      const counts = new Map<string, number>()
      
      mappingsResponse.PagedData.forEach(mapping => {
        const currentCount = counts.get(mapping.SKUGroupUID) || 0
        counts.set(mapping.SKUGroupUID, currentCount + 1)
      })
      
      setProductCounts(counts)
    } catch (error) {
      console.error('Error fetching product counts:', error)
    }
  }

  const fetchAllGroups = async () => {
    setLoading(true)
    try {
      const [groupsResponse, groupTypesResponse] = await Promise.all([
        skuService.getAllSKUGroups({
          PageNumber: 1,
          PageSize: 100,
          FilterCriterias: [],
          SortCriterias: [
            { SortParameter: 'ItemLevel', Direction: 'Asc' },
            { SortParameter: 'Name', Direction: 'Asc' }
          ],
          IsCountRequired: true
        }),
        skuService.getAllSKUGroupTypes({
          PageNumber: 1,
          PageSize: 100,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
          IsCountRequired: false
        })
      ])
      
      setGroups(groupsResponse.PagedData)
      setGroupTypes(groupTypesResponse.PagedData)
      
      // Fetch product counts
      await fetchProductCounts()
      
      // Build hierarchy
      const hierarchy = buildHierarchy(groupsResponse.PagedData, groupTypesResponse.PagedData)
      setHierarchicalGroups(hierarchy)
      
      // Auto-expand root nodes (those without parents)
      const rootNodes = hierarchy.map(group => group.UID)
      setExpandedNodes(new Set(rootNodes))
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch group hierarchy',
        variant: 'destructive'
      })
      console.error('Error fetching groups:', error)
    } finally {
      setLoading(false)
    }
  }

  const buildHierarchy = (flatGroups: SKUGroup[], groupTypesData: any[]): HierarchicalGroup[] => {
    const groupMap = new Map<string, HierarchicalGroup>()
    const rootGroups: HierarchicalGroup[] = []

    // Create a map of group types for easy lookup
    const typeMap = new Map<string, any>()
    groupTypesData.forEach(type => {
      typeMap.set(type.UID, type)
    })

    // First pass: create all nodes with type information
    flatGroups.forEach(group => {
      const groupType = typeMap.get(group.SKUGroupTypeUID)
      groupMap.set(group.UID, { 
        ...group, 
        children: [],
        typeName: groupType?.Name || 'Unknown'
      })
    })

    // Second pass: build tree structure
    flatGroups.forEach(group => {
      const node = groupMap.get(group.UID)!
      if (group.ParentUID && groupMap.has(group.ParentUID)) {
        const parent = groupMap.get(group.ParentUID)!
        parent.children.push(node)
      } else if (!group.ParentUID) {
        rootGroups.push(node)
      }
    })

    return rootGroups
  }


  useEffect(() => {
    fetchAllGroups()
  }, [])

  const fetchSKUsForGroup = async (groupUID: string) => {
    console.log('Fetching SKUs for group:', groupUID)
    
    // Update the group to show loading state
    setHierarchicalGroups(prevGroups => {
      const updateGroup = (groups: HierarchicalGroup[]): HierarchicalGroup[] => {
        return groups.map(group => {
          if (group.UID === groupUID) {
            return { ...group, loadingProducts: true }
          }
          if (group.children.length > 0) {
            return { ...group, children: updateGroup(group.children) }
          }
          return group
        })
      }
      return updateGroup(prevGroups)
    })

    try {
      // Get all mappings first (without filtering)
      const mappingsResponse = await skuService.getSKUToGroupMappings({
        PageNumber: 1,
        PageSize: 1000,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      })
      
      console.log('All mappings response:', mappingsResponse)
      
      // Filter mappings for this group in JavaScript
      const groupMappings = mappingsResponse.PagedData.filter(mapping => 
        mapping.SKUGroupUID === groupUID
      )
      
      console.log('Filtered mappings for', groupUID, ':', groupMappings)

      let filteredSKUs = []
      if (groupMappings.length > 0) {
        // Get SKU UIDs from mappings
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
        
        console.log('All SKUs response:', skusResponse)
        
        // Filter SKUs that match our UIDs - use UID field from SKU objects
        filteredSKUs = skusResponse.data.PagedData.filter(sku => 
          skuUIDs.includes(sku.UID)
        )
        
        console.log('Filtered SKUs:', filteredSKUs)
      }

      // Update the group with products
      setHierarchicalGroups(prevGroups => {
        const updateGroup = (groups: HierarchicalGroup[]): HierarchicalGroup[] => {
          return groups.map(group => {
            if (group.UID === groupUID) {
              return { 
                ...group, 
                products: filteredSKUs, 
                loadingProducts: false 
              }
            }
            if (group.children.length > 0) {
              return { ...group, children: updateGroup(group.children) }
            }
            return group
          })
        }
        return updateGroup(prevGroups)
      })

      // Expand the products for this group
      setExpandedProducts(prev => new Set([...prev, groupUID]))

    } catch (error) {
      console.error('Error fetching SKUs for group:', error)
      toast({
        title: 'Error',
        description: 'Failed to fetch products for this group',
        variant: 'destructive'
      })
      
      // Update the group to remove loading state
      setHierarchicalGroups(prevGroups => {
        const updateGroup = (groups: HierarchicalGroup[]): HierarchicalGroup[] => {
          return groups.map(group => {
            if (group.UID === groupUID) {
              return { ...group, loadingProducts: false, products: [] }
            }
            if (group.children.length > 0) {
              return { ...group, children: updateGroup(group.children) }
            }
            return group
          })
        }
        return updateGroup(prevGroups)
      })
    }
  }


  const handleProductsToggle = async (group: HierarchicalGroup, event: React.MouseEvent) => {
    event.stopPropagation()
    console.log('Products toggle clicked for group:', group.Name, group.UID)
    
    if (expandedProducts.has(group.UID)) {
      // Collapse products
      setExpandedProducts(prev => {
        const newSet = new Set(prev)
        newSet.delete(group.UID)
        return newSet
      })
    } else {
      // Expand products - fetch if not already loaded
      if (!group.products) {
        await fetchSKUsForGroup(group.UID)
      } else {
        setExpandedProducts(prev => new Set([...prev, group.UID]))
      }
    }
  }

  const toggleNode = (nodeId: string) => {
    const newExpanded = new Set(expandedNodes)
    if (newExpanded.has(nodeId)) {
      newExpanded.delete(nodeId)
    } else {
      newExpanded.add(nodeId)
    }
    setExpandedNodes(newExpanded)
  }

  const getIcon = (group: HierarchicalGroup, isExpanded: boolean) => {
    if (group.children.length === 0) {
      return <Package className="h-4 w-4" />
    }
    return isExpanded ? <FolderOpen className="h-4 w-4" /> : <Folder className="h-4 w-4" />
  }

  const renderGroupNode = (group: HierarchicalGroup, level: number = 0) => {
    const isExpanded = expandedNodes.has(group.UID)
    const hasChildren = group.children.length > 0
    const productCount = productCounts.get(group.UID) || 0
    const isProductsExpanded = expandedProducts.has(group.UID)

    return (
      <div key={group.UID}>
        {/* Group Node */}
        <div
          className="flex items-center gap-3 py-3 px-4 rounded-lg cursor-pointer transition-all duration-200 hover:bg-muted/50 border border-transparent hover:border-border"
          style={{ paddingLeft: `${level * 28 + 16}px` }}
          onClick={async (e) => {
            console.log('Node clicked:', group.Name, 'hasChildren:', hasChildren, 'productCount:', productCount)
            
            // Handle expand/collapse for groups with children
            if (hasChildren) {
              e.stopPropagation()
              toggleNode(group.UID)
            }
            // Handle product expansion for groups with products (but no children)
            else if (productCount > 0) {
              e.stopPropagation()
              await handleProductsToggle(group, e)
            }
          }}
        >
          {(hasChildren || productCount > 0) && (
            <div className="w-5 h-5 flex items-center justify-center">
              {hasChildren ? (
                isExpanded ? (
                  <ChevronDown className="h-4 w-4 text-muted-foreground" />
                ) : (
                  <ChevronRight className="h-4 w-4 text-muted-foreground" />
                )
              ) : (
                isProductsExpanded ? (
                  <ChevronDown className="h-4 w-4 text-primary" />
                ) : (
                  <ChevronRight className="h-4 w-4 text-primary" />
                )
              )}
            </div>
          )}
          {!hasChildren && productCount === 0 && <div className="w-5" />}
          
          <div className="h-8 w-8 rounded-md bg-background border flex items-center justify-center">
            {getIcon(group, isExpanded)}
          </div>
          
          <span className="font-semibold flex-1 text-base">{group.Name}</span>
          
          {productCount > 0 && (
            <Badge 
              variant="secondary" 
              className="gap-1.5 px-2.5 py-1"
              title={`${productCount} products - click group to expand`}
            >
              {group.loadingProducts ? (
                <>
                  <div className="w-3 h-3 border border-current border-t-transparent rounded-full animate-spin"></div>
                  Loading...
                </>
              ) : (
                <>
                  <Package className="h-3 w-3" />
                  {productCount} product{productCount !== 1 ? 's' : ''}
                </>
              )}
            </Badge>
          )}
          
          <Badge 
            variant={group.ItemLevel === 1 ? 'default' : 'outline'}
            className="px-2.5 py-1"
          >
            {group.typeName || `Level ${group.ItemLevel}`}
          </Badge>
          
          <code className="text-sm text-muted-foreground bg-muted px-2 py-1 rounded font-mono">
            {group.Code}
          </code>
        </div>
        
        {/* Products List (if expanded) */}
        {isProductsExpanded && group.products && group.products.length > 0 && (
          <div className="mt-3 space-y-2">
            {group.products.map((sku, index) => (
              <div
                key={sku.UID}
                className="flex items-center gap-4 py-3 px-4 ml-10 rounded-lg bg-primary/5 border border-primary/20 hover:bg-primary/10 transition-colors"
                style={{ paddingLeft: `${(level + 1) * 28 + 16}px` }}
              >
                <div className="w-7 h-7 bg-primary rounded-full flex items-center justify-center text-primary-foreground font-bold text-sm">
                  {index + 1}
                </div>
                <div className="h-8 w-8 rounded-md bg-background border flex items-center justify-center">
                  <Package className="h-4 w-4 text-primary" />
                </div>
                <div className="flex-1 min-w-0 space-y-1">
                  <div className="flex items-center gap-3">
                    <span className="font-semibold text-base truncate">
                      {sku.LongName || sku.Name}
                    </span>
                    <code className="text-xs font-mono bg-muted px-2 py-1 rounded">
                      {sku.Code}
                    </code>
                  </div>
                  {sku.AliasName && (
                    <p className="text-sm text-muted-foreground truncate">{sku.AliasName}</p>
                  )}
                </div>
                <Badge 
                  variant={sku.IsActive ? "default" : "secondary"} 
                  className="px-2.5 py-1"
                >
                  {sku.IsActive ? 'Active' : 'Inactive'}
                </Badge>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    className="h-9 gap-1.5"
                    onClick={() => router.push(`/updatedfeatures/sku-management/products/view?uid=${sku.UID}`)}
                  >
                    <Eye className="h-3.5 w-3.5" />
                    View
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    className="h-9 gap-1.5"
                    onClick={() => router.push(`/updatedfeatures/sku-management/products/edit?uid=${sku.UID}`)}
                  >
                    <ExternalLink className="h-3.5 w-3.5" />
                    Edit
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
        
        {/* Child Groups */}
        {isExpanded && group.children.map(child => renderGroupNode(child, level + 1))}
      </div>
    )
  }

  return (
    <div className="container mx-auto p-6 space-y-8">
      {/* Header */}
      <div className="flex items-center gap-6">
        <Button
          variant="outline"
          size="default"
          onClick={() => router.back()}
          className="gap-2"
        >
          <ArrowLeft className="h-4 w-4" />
          Back
        </Button>
        <div className="space-y-1">
          <h1 className="text-4xl font-bold tracking-tight">Group Hierarchy</h1>
          <p className="text-lg text-muted-foreground">
            Navigate through your product categorization hierarchy
          </p>
        </div>
      </div>


      {/* Main Content */}
      <Card className="shadow-sm">
        <CardHeader className="pb-4">
          <div className="flex items-center gap-3">
            <div className="h-8 w-8 rounded-lg bg-primary/10 flex items-center justify-center">
              <FolderOpen className="h-4 w-4 text-primary" />
            </div>
            <div>
              <CardTitle className="text-xl">Product Group Tree</CardTitle>
              <CardDescription className="text-base">
                Click to expand groups and view products
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent className="pt-0">
          {loading ? (
            <div className="flex flex-col items-center justify-center py-12 space-y-4">
              <div className="h-8 w-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
              <p className="text-muted-foreground font-medium">Loading hierarchy...</p>
            </div>
          ) : hierarchicalGroups.length === 0 ? (
            <div className="text-center py-12 space-y-4">
              <div className="h-12 w-12 rounded-lg bg-muted flex items-center justify-center mx-auto">
                <FolderOpen className="h-6 w-6 text-muted-foreground" />
              </div>
              <div>
                <h3 className="font-semibold text-lg">No groups found</h3>
                <p className="text-muted-foreground">Your product hierarchy will appear here</p>
              </div>
            </div>
          ) : (
            <div className="space-y-2">
              {hierarchicalGroups.map(group => renderGroupNode(group))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}