"use client";

import React, { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { useToast } from "@/components/ui/use-toast";
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  ChevronRight,
  ChevronDown,
  MapPin,
  RefreshCw,
  Layers,
  Building2,
  Package,
  Search,
  Plus,
  Edit,
  Trash2,
  MoreHorizontal
} from "lucide-react";
import { storeGroupService } from "@/services/storeGroupService";
import { IStoreGroup } from "@/types/storeGroup.types";
import { cn } from "@/lib/utils";

interface TreeNode extends IStoreGroup {
  children?: TreeNode[];
}

interface TreeNodeProps {
  node: TreeNode;
  level: number;
  onToggle: (uid: string) => void;
  expandedNodes: Set<string>;
  onRefresh: () => void;
  onEdit?: (storeGroup: IStoreGroup) => void;
  onDelete?: (storeGroup: IStoreGroup) => void;
}

function StoreTreeNode({ 
  node, 
  level, 
  onToggle, 
  expandedNodes, 
  onRefresh,
  onEdit,
  onDelete
}: TreeNodeProps) {
  const { toast } = useToast();
  const isExpanded = expandedNodes.has(node.UID);
  const hasChildren = node.children && node.children.length > 0;
  
  const handleDelete = async () => {
    if (!confirm(`Are you sure you want to delete ${node.Name}?`)) return;
    
    try {
      await storeGroupService.deleteStoreGroup(node.UID);
      toast({
        title: "Success",
        description: "Store group deleted successfully"
      });
      onRefresh();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete store group",
        variant: "destructive"
      });
    }
  };

  // Get type color class
  const getTypeColor = (type: string) => {
    switch (type?.toLowerCase()) {
      case 'channel': return 'blue';
      case 'region': return 'green';
      case 'area': return 'purple';
      case 'zone': return 'orange';
      default: return 'gray';
    }
  };

  const color = getTypeColor(node.StoreGroupTypeUID);

  // Get icon based on store group type
  const getStoreGroupIcon = () => {
    switch (node.StoreGroupTypeUID?.toLowerCase()) {
      case 'channel': return <Package className={`h-4 w-4 text-${color}-600`} />;
      case 'region': return <MapPin className={`h-4 w-4 text-${color}-600`} />;
      case 'area': return <Building2 className={`h-4 w-4 text-${color}-600`} />;
      default: return <Layers className={`h-4 w-4 text-${color}-600`} />;
    }
  };

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
            onClick={() => onToggle(node.UID)}
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
        
        {/* Store Group Icon */}
        <div className={cn(
          "p-2 rounded-lg",
          `bg-${color}-100 text-${color}-700`
        )}>
          {getStoreGroupIcon()}
        </div>
        
        {/* Store Group Details */}
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h4 className="font-semibold text-gray-900">{node.Name}</h4>
            <Badge 
              variant="outline" 
              className={cn('text-xs font-mono', `border-${color}-300 text-${color}-700`)}
            >
              {node.Code}
            </Badge>
            <Badge 
              className={cn(
                "text-xs",
                `bg-${color}-200 text-${color}-800 border-${color}-300`
              )}
            >
              {node.StoreGroupTypeUID}
            </Badge>
            <Badge variant="outline" className="text-xs">
              Level {node.ItemLevel}
            </Badge>
            {level === 0 && (
              <Badge variant="default" className="text-xs">
                Root
              </Badge>
            )}
          </div>
          {hasChildren && (
            <p className="text-sm text-gray-500 mt-1">
              {node.children!.length} child group{node.children!.length !== 1 ? 's' : ''}
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
                onClick={(e) => e.stopPropagation()}
              >
                <span className="sr-only">Open menu</span>
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-48">
              <DropdownMenuLabel>Actions</DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => onEdit?.(node)}>
                <Edit className="mr-2 h-4 w-4" />
                Edit
              </DropdownMenuItem>
              <DropdownMenuItem onClick={(e) => {
                e.stopPropagation();
                // TODO: Add create child functionality
              }}>
                <Plus className="mr-2 h-4 w-4" />
                Add Child Group
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
      </div>

      {/* Children */}
      {isExpanded && node.children && (
        <div className={cn(
          "mt-2 pt-2",
          `border-l-2 border-${color}-200`
        )}>
          {node.children.map((child) => (
            <StoreTreeNode
              key={child.UID}
              node={child}
              level={level + 1}
              onToggle={onToggle}
              expandedNodes={expandedNodes}
              onRefresh={onRefresh}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default function StoreHierarchy() {
  const { toast } = useToast();
  const [storeGroups, setStoreGroups] = useState<IStoreGroup[]>([]);
  const [treeData, setTreeData] = useState<TreeNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedType, setSelectedType] = useState<string>("all");
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set());
  const [filteredData, setFilteredData] = useState<TreeNode[]>([]);

  useEffect(() => {
    loadHierarchy();
  }, []);

  useEffect(() => {
    if (storeGroups.length > 0) {
      buildHierarchy();
    }
  }, [storeGroups, selectedType]);

  useEffect(() => {
    filterHierarchy();
  }, [treeData, searchQuery]);

  const loadHierarchy = async () => {
    setLoading(true);
    try {
      const response = await storeGroupService.getAllStoreGroups({
        PageNumber: 1,
        PageSize: 1000,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      });

      setStoreGroups(response.PagedData || []);
    } catch (error) {
      console.error("Error loading hierarchy:", error);
      toast({
        title: "Error",
        description: "Failed to load store hierarchy",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const buildHierarchy = () => {
    const filteredGroups = selectedType === "all" 
      ? storeGroups 
      : storeGroups.filter(group => group.StoreGroupTypeUID === selectedType);
    
    const hierarchy = buildHierarchyTree(filteredGroups);
    setTreeData(hierarchy);
    
    // Auto-expand first level
    const firstLevelIds = new Set(hierarchy.map(node => node.UID));
    setExpandedNodes(firstLevelIds);
  };

  const buildHierarchyTree = (groups: IStoreGroup[]): TreeNode[] => {
    const map = new Map<string, TreeNode>();
    const roots: TreeNode[] = [];

    // Create nodes
    groups.forEach(group => {
      map.set(group.UID, { ...group, children: [] });
    });

    // Build tree
    groups.forEach(group => {
      const node = map.get(group.UID)!;
      if (group.ParentUID && map.has(group.ParentUID)) {
        const parent = map.get(group.ParentUID)!;
        if (!parent.children) parent.children = [];
        parent.children.push(node);
      } else {
        roots.push(node);
      }
    });

    return roots;
  };

  const filterHierarchy = () => {
    if (!searchQuery) {
      setFilteredData(treeData);
      return;
    }

    const query = searchQuery.toLowerCase();
    
    const filterNodes = (nodes: TreeNode[]): TreeNode[] => {
      return nodes.reduce((filtered, node) => {
        const nodeMatches = 
          node.Name.toLowerCase().includes(query) ||
          node.Code.toLowerCase().includes(query);

        const filteredChildren = node.children ? filterNodes(node.children) : [];

        if (nodeMatches || filteredChildren.length > 0) {
          filtered.push({
            ...node,
            children: filteredChildren
          });
        }

        return filtered;
      }, [] as TreeNode[]);
    };

    const filtered = filterNodes(treeData);
    setFilteredData(filtered);
    
    // Expand all nodes when searching
    if (searchQuery) {
      const allNodeIds = new Set<string>();
      const collectIds = (nodes: TreeNode[]) => {
        nodes.forEach(node => {
          allNodeIds.add(node.UID);
          if (node.children) {
            collectIds(node.children);
          }
        });
      };
      collectIds(filtered);
      setExpandedNodes(allNodeIds);
    }
  };

  const handleToggle = (uid: string) => {
    const newExpanded = new Set(expandedNodes);
    if (newExpanded.has(uid)) {
      newExpanded.delete(uid);
    } else {
      newExpanded.add(uid);
    }
    setExpandedNodes(newExpanded);
  };

  const expandAll = () => {
    const allNodeIds = new Set<string>();
    const collectIds = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        allNodeIds.add(node.UID);
        if (node.children) {
          collectIds(node.children);
        }
      });
    };
    collectIds(filteredData);
    setExpandedNodes(allNodeIds);
  };

  const collapseAll = () => {
    setExpandedNodes(new Set());
  };

  const uniqueTypes = [...new Set(storeGroups.map(sg => sg.StoreGroupTypeUID))];

  return (
    <div className="space-y-6">
      {/* Enhanced Controls */}
      <Card className="shadow-lg">
        <CardHeader className="bg-gradient-to-r from-blue-50 to-blue-100 border-b">
          <CardTitle className="text-xl flex items-center gap-2">
            <Layers className="h-5 w-5" />
            Store Group Hierarchy
          </CardTitle>
          <CardDescription className="text-gray-600">
            Visualize and navigate through your store group structure
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
                  <p className="text-sm font-medium text-gray-600">Total Groups</p>
                  <p className="text-2xl font-bold text-blue-600">{storeGroups.length}</p>
                </div>
                <div className="bg-white p-3 rounded-lg shadow-sm">
                  <p className="text-sm font-medium text-gray-600">Root Groups</p>
                  <p className="text-2xl font-bold text-green-600">{treeData.length}</p>
                </div>
                <div className="bg-white p-3 rounded-lg shadow-sm">
                  <p className="text-sm font-medium text-gray-600">Group Types</p>
                  <p className="text-2xl font-bold text-purple-600">{uniqueTypes.length}</p>
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
                placeholder="Search store groups..."
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
                {uniqueTypes.map((type) => (
                  <SelectItem key={type} value={type}>
                    {type}
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
              <Button variant="outline" size="sm" onClick={loadHierarchy}>
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>
            </div>
          </div>

          {/* Hierarchy Tree */}
          <div className="border rounded-lg bg-gradient-to-br from-gray-50 to-gray-100 p-6 max-h-[700px] overflow-y-auto shadow-inner">
            {loading ? (
              <div className="space-y-4">
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
                  </div>
                ))}
              </div>
            ) : filteredData.length === 0 ? (
              <div className="text-center py-12">
                <Layers className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 font-medium">No store groups found</p>
                <p className="text-sm text-gray-400 mt-2">
                  {searchQuery ? "Try adjusting your search query" : "No store groups have been created yet"}
                </p>
              </div>
            ) : (
              <div className="space-y-3">
                {filteredData.map((node) => (
                  <StoreTreeNode
                    key={node.UID}
                    node={node}
                    level={0}
                    onToggle={handleToggle}
                    expandedNodes={expandedNodes}
                    onRefresh={loadHierarchy}
                  />
                ))}
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}