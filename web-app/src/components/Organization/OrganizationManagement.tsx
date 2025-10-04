"use client"

import { useState, useEffect, useCallback, useRef } from "react"
import { useRouter } from "next/navigation"
import { Plus, Search, Edit, Trash2, Building2, Filter, Globe, Building, MapPinned, Network, Download, Upload, Eye, MoreHorizontal, ChevronDown, X, Package, RefreshCw } from "lucide-react"
import { usePagePermissions } from "@/hooks/use-page-permissions"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { useToast } from "@/components/ui/use-toast"
import { organizationService, Organization, OrgType } from "@/services/organizationService"
import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"
import { formatDateToDayMonthYear } from "@/utils/date-formatter"
import { PaginationControls } from "@/components/ui/pagination-controls"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Skeleton } from "@/components/ui/skeleton"

export function OrganizationManagement() {
  const router = useRouter()
  const { toast } = useToast()
  const searchInputRef = useRef<HTMLInputElement>(null)
  // Get permissions dynamically based on the current page path
  const permissions = usePagePermissions()
  
  
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([])
  const [loading, setLoading] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedOrgTypes, setSelectedOrgTypes] = useState<string[]>([])
  const [selectedStatus, setSelectedStatus] = useState<string[]>([])
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [organizationToDelete, setOrganizationToDelete] = useState<Organization | null>(null)
  const [refreshTrigger, setRefreshTrigger] = useState(0)
  
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 20,
    total: 0
  })
  
  // Statistics state
  const [stats, setStats] = useState({
    total: 0,
    active: 0,
    types: 0
  })

  // Fetch organization types on mount
  useEffect(() => {
    fetchOrgTypes()
  }, [])

  // Fetch organizations when filters change (including search)
  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      fetchOrganizations()
    }, searchQuery ? 500 : 0) // 500ms debounce for search, immediate for other changes
    
    return () => clearTimeout(debounceTimer)
  }, [pagination.current, pagination.pageSize, selectedOrgTypes, selectedStatus, searchQuery, refreshTrigger])
  
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

  const fetchOrgTypes = async () => {
    try {
      const types = await organizationService.getOrganizationTypes()
      // Filter to show only organization types with ShowInUI = true
      const visibleTypes = types.filter(type => type.ShowInUI !== false)
      setOrgTypes(visibleTypes)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch organization types",
        variant: "destructive"
      })
    }
  }

  const fetchOrganizations = async () => {
    setLoading(true)
    try {
      const filters = []
      
      // Add organization type filters
      if (selectedOrgTypes.length > 0) {
        filters.push({
          Name: "OrgTypeUID",
          Value: selectedOrgTypes, // Pass array of selected org type UIDs
          Type: 4, // IN operation - checks if OrgTypeUID is in the array
          FilterType: 4
        })
      }
      
      if (searchQuery) {
        filters.push({
          Name: "Name",
          Value: searchQuery,
          Type: 10, // Contains (more reliable than LIKE)
          FilterType: 10
        })
      }
      
      // Add status filters (applied client-side after fetching)
      // Status filtering will be done after fetching all visible organizations


      // Fetch more items to account for ShowInUI filtering
      // We'll fetch a larger page size and then filter
      const fetchPageSize = pagination.pageSize * 3 // Fetch 3x to ensure we have enough after filtering
      
      const result = await organizationService.getOrganizations(
        1, // Always start from page 1 for now
        1000, // Fetch up to 1000 to get all for proper filtering
        filters
      )

      // Filter organizations to show only those with ShowInUI = true
      let allVisibleOrgs = result.data.filter(org => org.ShowInUI !== false)
      
      // Apply status filtering client-side
      if (selectedStatus.length > 0) {
        allVisibleOrgs = allVisibleOrgs.filter(org => {
          if (selectedStatus.includes("active") && org.IsActive) return true
          if (selectedStatus.includes("inactive") && !org.IsActive) return true
          return false
        })
      }
      
      // Now apply client-side pagination to the filtered results
      const startIndex = (pagination.current - 1) * pagination.pageSize
      const endIndex = startIndex + pagination.pageSize
      const paginatedOrgs = allVisibleOrgs.slice(startIndex, endIndex)
      
      setOrganizations(paginatedOrgs)
      
      // Update pagination with the actual visible count
      setPagination(prev => ({ ...prev, total: allVisibleOrgs.length }))
      
      // Update statistics with all visible organizations
      setStats({
        total: allVisibleOrgs.length,
        active: allVisibleOrgs.filter(org => org.IsActive).length,
        types: orgTypes.length
      })
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch organizations",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }


  const handleCreate = () => {
    router.push("/administration/organization-management/organizations/create")
  }

  const handleEdit = (organization: Organization) => {
    router.push(`/administration/organization-management/organizations/create?id=${organization.UID}`)
  }
  
  const handleView = (organization: Organization) => {
    router.push(`/administration/organization-management/organizations/${organization.UID}`)
  }

  const handleDeleteClick = (organization: Organization) => {
    setOrganizationToDelete(organization)
    setDeleteDialogOpen(true)
  }

  const handleDelete = async () => {
    if (!organizationToDelete) return

    try {
      await organizationService.deleteOrganization(organizationToDelete.UID)
      toast({
        title: "Success",
        description: "Organization deleted successfully"
      })
      setDeleteDialogOpen(false)
      setOrganizationToDelete(null)
      setRefreshTrigger(prev => prev + 1)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete organization",
        variant: "destructive"
      })
    }
  }


  const handleBuildHierarchy = async (organization: Organization) => {
    try {
      setLoading(true)
      const response = await organizationService.insertOrganizationHierarchy(organization.UID)
      if (response.IsSuccess) {
        toast({
          title: "Success",
          description: "Organization hierarchy built successfully"
        })
      } else {
        toast({
          title: "Error",
          description: response.Error || "Failed to build hierarchy",
          variant: "destructive"
        })
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Error building organization hierarchy",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const handleExport = async () => {
    try {
      const blob = await organizationService.exportOrganizations("csv")
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `organizations-${new Date().toISOString().split("T")[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
      
      toast({
        title: "Success",
        description: "Organizations exported successfully"
      })
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to export organizations",
        variant: "destructive"
      })
    }
  }

  const handleImport = () => {
    router.push("/administration/organization-management/import")
  }

  // Get org type color based on level
  const getOrgTypeColor = (orgTypeUID: string): string => {
    const typeHierarchy = organizationService.buildOrgTypeHierarchy(orgTypes)
    const typeInfo = typeHierarchy.get(orgTypeUID)
    if (!typeInfo) return "default"
    
    const colorsByLevel = [
      "default",    // Level 0 - Blue
      "secondary",  // Level 1 - Gray
      "outline",    // Level 2+ - Outline
    ]
    
    return colorsByLevel[Math.min(typeInfo.level, colorsByLevel.length - 1)] as any
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Organization Management</h1>
          <p className="text-muted-foreground">
            Manage organizational structure and hierarchy
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          {permissions.canAdd && (
            <Button
              variant="outline"
              onClick={handleImport}
            >
              <Upload className="mr-2 h-4 w-4" />
              Import
            </Button>
          )}
          {permissions.canDownload && (
            <Button
              variant="outline"
              onClick={handleExport}
            >
              <Download className="mr-2 h-4 w-4" />
              Export
            </Button>
          )}
          <Button
            variant="outline"
            onClick={() => setRefreshTrigger(prev => prev + 1)}
          >
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          {permissions.canAdd && (
            <Button onClick={handleCreate}>
              <Plus className="mr-2 h-4 w-4" />
              Add Organization
            </Button>
          )}
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Organizations</CardTitle>
            <Building2 className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              All registered organizations
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Organizations</CardTitle>
            <Building className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{stats.active.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Currently operational units
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Organization Types</CardTitle>
            <Filter className="h-4 w-4 text-purple-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-purple-600">{stats.types.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Different organization categories
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Search Bar - Exactly like product groups */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3">
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
            
            {/* Organization Type Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Organization Type
                  {selectedOrgTypes.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedOrgTypes.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
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
                      setPagination(prev => ({ ...prev, current: 1 }))
                    }}
                  >
                    {type.Name}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>

            {/* Status Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {selectedStatus.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedStatus.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={selectedStatus.includes("active")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev => 
                      checked 
                        ? [...prev, "active"]
                        : prev.filter(s => s !== "active")
                    )
                    setPagination(prev => ({ ...prev, current: 1 }))
                  }}
                >
                  Active
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedStatus.includes("inactive")}
                  onCheckedChange={(checked) => {
                    setSelectedStatus(prev => 
                      checked 
                        ? [...prev, "inactive"]
                        : prev.filter(s => s !== "inactive")
                    )
                    setPagination(prev => ({ ...prev, current: 1 }))
                  }}
                >
                  Inactive
                </DropdownMenuCheckboxItem>
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Clear All Button */}
            {(searchQuery || selectedOrgTypes.length > 0 || selectedStatus.length > 0) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchQuery("")
                  setSelectedOrgTypes([])
                  setSelectedStatus([])
                  setPagination(prev => ({ ...prev, current: 1 }))
                }}
              >
                <X className="h-4 w-4 mr-2" />
                Clear All
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Organizations Table */}
      <Card className="shadow-sm border-gray-200">
        <div className="p-0">
          <div className="overflow-hidden rounded-lg">
            <Table>
              <TableHeader>
                <TableRow className="bg-gray-50/50">
                  <TableHead className="pl-6">Code</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Parent</TableHead>
                  <TableHead>Location</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Created</TableHead>
                  {(permissions.canEdit || permissions.canDelete) && (
                    <TableHead className="text-right pr-6">Actions</TableHead>
                  )}
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  // Loading skeletons
                  Array.from({ length: 5 }).map((_, index) => (
                    <TableRow key={index}>
                      <TableCell className="pl-6"><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-28" /></TableCell>
                      <TableCell className="pr-6"><Skeleton className="h-4 w-24" /></TableCell>
                    </TableRow>
                  ))
                ) : organizations && organizations.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} className="text-center py-12">
                      <div className="flex flex-col items-center space-y-4">
                        <Package className="h-12 w-12 text-muted-foreground/50" />
                        <div className="space-y-2">
                          <p className="text-sm font-medium text-muted-foreground">No organizations found</p>
                          <p className="text-xs text-muted-foreground">
                            {searchQuery || selectedOrgTypes.length > 0 || selectedStatus.length > 0 ? 'Try adjusting your filters' : 'Click Add Organization to create a new organization'}
                          </p>
                        </div>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : organizations ? (
                  organizations.map((org, index) => {
                    const variant = getOrgTypeColor(org.OrgTypeUID)
                    return (
                      <TableRow key={org.UID || `org-${index}`}>
                        <TableCell className="pl-6">
                          <code className="text-sm bg-gray-100 px-2 py-1 rounded font-mono">{org.Code}</code>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Building2 className="h-4 w-4 text-gray-600" />
                            <span className="font-medium">{org.Name}</span>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge 
                            variant={variant}
                            className={cn(
                              variant === "default" && "bg-blue-100 text-blue-800 hover:bg-blue-100",
                              variant === "secondary" && "bg-gray-100 text-gray-800 hover:bg-gray-100"
                            )}
                          >
                            {org.OrgTypeName || org.OrgTypeUID}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          {org.ParentUID ? (
                            <span className="text-sm">{org.ParentName || org.ParentUID}</span>
                          ) : (
                            <span className="text-gray-400">-</span>
                          )}
                        </TableCell>
                        <TableCell>
                          {org.CountryName || org.RegionName || org.CityName || org.CountryUID ? (
                            <div className="flex items-center gap-1 text-sm text-gray-600">
                              <MapPinned className="h-3 w-3" />
                              <span>{[org.CountryName, org.RegionName, org.CityName].filter(Boolean).join(', ') || org.CountryUID}</span>
                            </div>
                          ) : (
                            <span className="text-gray-400">-</span>
                          )}
                        </TableCell>
                        <TableCell>
                          <Badge variant={org.IsActive ? "default" : "secondary"}>
                            {org.IsActive ? "Active" : "Inactive"}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <span className="text-sm text-gray-600">
                            {formatDateToDayMonthYear(org.CreatedTime)}
                          </span>
                        </TableCell>
                        {(permissions.canEdit || permissions.canDelete) && (
                          <TableCell className="text-right pr-6">
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" className="h-8 w-8 p-0">
                                  <span className="sr-only">Open menu</span>
                                  <MoreHorizontal className="h-4 w-4" />
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end">
                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                {permissions.canView && (
                                  <DropdownMenuItem
                                    onClick={() => handleView(org)}
                                    className="cursor-pointer"
                                  >
                                    <Eye className="mr-2 h-4 w-4" />
                                    View Details
                                  </DropdownMenuItem>
                                )}
                                {permissions.canEdit && (
                                  <DropdownMenuItem
                                    onClick={() => handleEdit(org)}
                                    className="cursor-pointer"
                                  >
                                    <Edit className="mr-2 h-4 w-4" />
                                    Edit
                                  </DropdownMenuItem>
                                )}
                                {(permissions.canEdit || permissions.canDelete) && <DropdownMenuSeparator />}
                                {permissions.canDelete && (
                                  <DropdownMenuItem
                                    onClick={() => handleDeleteClick(org)}
                                    className="cursor-pointer text-red-600 focus:text-red-600"
                                  >
                                    <Trash2 className="mr-2 h-4 w-4" />
                                    Delete
                                  </DropdownMenuItem>
                                )}
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </TableCell>
                        )}
                      </TableRow>
                    )
                  })
                ) : null}
              </TableBody>
            </Table>
          </div>
          
          {/* Pagination Controls */}
          {pagination.total > 0 && (
            <div className="px-6 py-4 border-t bg-gray-50/30">
              <PaginationControls
                currentPage={pagination.current}
                totalCount={pagination.total}
                pageSize={pagination.pageSize}
                onPageChange={(page) => setPagination(prev => ({ ...prev, current: page }))}
                onPageSizeChange={(size) => setPagination(prev => ({ ...prev, pageSize: size, current: 1 }))}
                itemName="organizations"
              />
            </div>
          )}
        </div>
      </Card>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Organization</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete "{organizationToDelete?.Name}"? 
              This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteDialogOpen(false)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
            >
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}