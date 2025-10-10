"use client"

import { useState, useEffect, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Plus, Search, Edit, Trash2, Building2, RefreshCw, Filter, Globe, Building, MapPinned, Network } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { useToast } from "@/components/ui/use-toast"
import { organizationService, Organization, OrgType } from "@/services/organizationService"
import { DataGrid, DataGridColumn, DataGridAction } from "@/components/ui/data-grid"
import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"

export function OrganizationManagement() {
  const router = useRouter()
  const { toast } = useToast()
  
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([])
  const [loading, setLoading] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedOrgType, setSelectedOrgType] = useState<string>("all")
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [organizationToDelete, setOrganizationToDelete] = useState<Organization | null>(null)
  const [refreshTrigger, setRefreshTrigger] = useState(0)
  const [activeTab, setActiveTab] = useState<"operational" | "by-type">("operational")
  
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

  // Fetch organizations when filters change
  useEffect(() => {
    fetchOrganizations()
  }, [pagination.current, pagination.pageSize, selectedOrgType, refreshTrigger])

  const fetchOrgTypes = async () => {
    try {
      const types = await organizationService.getOrganizationTypes()
      setOrgTypes(types)
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
      if (selectedOrgType && selectedOrgType !== "all") {
        filters.push({
          Name: "OrgTypeUID",
          Value: selectedOrgType,
          Type: 0, // Equal
          FilterType: 0
        })
      }
      
      if (searchQuery) {
        filters.push({
          Name: "Name",
          Value: searchQuery,
          Type: 1, // LIKE
          FilterType: 1
        })
      }

      const result = await organizationService.getOrganizations(
        pagination.current,
        pagination.pageSize,
        filters
      )

      setOrganizations(result.data)
      setPagination(prev => ({ ...prev, total: result.total }))
      
      // Update statistics
      setStats({
        total: result.total,
        active: result.data.filter(org => org.IsActive).length,
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

  const handleSearch = useCallback(() => {
    setPagination(prev => ({ ...prev, current: 1 }))
    setRefreshTrigger(prev => prev + 1)
  }, [])

  const handleCreate = () => {
    router.push("/updatedfeatures/organization-management/organizations/create")
  }

  const handleEdit = (organization: Organization) => {
    router.push(`/updatedfeatures/organization-management/organizations/create?id=${organization.UID}`)
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

  const handlePageChange = (page: number, pageSize: number) => {
    setPagination(prev => ({ 
      ...prev, 
      current: page,
      pageSize: pageSize
    }))
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

  const columns: DataGridColumn<Organization>[] = [
    {
      key: "Code",
      title: "Code",
      width: "120px",
      sortable: true,
      render: (value: any, record: Organization) => (
        <code className="text-sm bg-gray-100 px-2 py-1 rounded font-mono">{record.Code}</code>
      )
    },
    {
      key: "Name",
      title: "Name",
      sortable: true,
      render: (value: any, record: Organization) => (
        <div className="flex items-center gap-2">
          <Building2 className="h-4 w-4 text-gray-600" />
          <span className="font-medium">{record.Name}</span>
        </div>
      )
    },
    {
      key: "OrgTypeName",
      title: "Type",
      render: (value: any, record: Organization) => {
        const variant = getOrgTypeColor(record.OrgTypeUID)
        return (
          <Badge 
            variant={variant}
            className={cn(
              variant === "default" && "bg-blue-100 text-blue-800 hover:bg-blue-100",
              variant === "secondary" && "bg-gray-100 text-gray-800 hover:bg-gray-100"
            )}
          >
            {record.OrgTypeName || record.OrgTypeUID}
          </Badge>
        )
      }
    },
    {
      key: "ParentName",
      title: "Parent",
      render: (value: any, record: Organization) => {
        if (!record.ParentUID) return <span className="text-gray-400">-</span>
        return (
          <span className="text-sm">
            {record.ParentName || record.ParentUID}
          </span>
        )
      }
    },
    {
      key: "IsActive",
      title: "Status",
      width: "100px",
      render: (value: any, record: Organization) => (
        <Badge variant={record.IsActive ? "default" : "secondary"}>
          {record.IsActive ? "Active" : "Inactive"}
        </Badge>
      )
    },
    {
      key: "CreatedTime",
      title: "Created",
      width: "150px",
      render: (value: any, record: Organization) => (
        <span className="text-sm text-gray-600">
          {record.CreatedTime ? new Date(record.CreatedTime).toLocaleDateString() : '-'}
        </span>
      )
    }
  ]

  const actions: DataGridAction<Organization>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: Edit,
      onClick: (record) => handleEdit(record)
    },
    {
      key: 'build-hierarchy',
      label: 'Build Hierarchy',
      icon: Network,
      onClick: (record) => handleBuildHierarchy(record)
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: Trash2,
      onClick: (record) => handleDeleteClick(record),
      variant: 'destructive'
    }
  ]

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
          <Button
            variant="outline"
            onClick={() => setRefreshTrigger(prev => prev + 1)}
          >
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
          <Button onClick={handleCreate}>
            <Plus className="mr-2 h-4 w-4" />
            Add Organization
          </Button>
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

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Search & Filter</CardTitle>
          <CardDescription>Find organizations quickly using search and filters</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="flex gap-2">
              <Input
                placeholder="Search by name or code..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              />
              <Button onClick={handleSearch} size="icon">
                <Search className="h-4 w-4" />
              </Button>
            </div>
            
            <Select value={selectedOrgType} onValueChange={setSelectedOrgType}>
              <SelectTrigger>
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

            <Button
              variant="outline"
              onClick={() => {
                setSearchQuery("")
                setSelectedOrgType("all")
                setRefreshTrigger(prev => prev + 1)
              }}
              className="ml-auto"
            >
              <RefreshCw className="mr-2 h-4 w-4" />
              Clear Filters
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Organizations Table */}
      <Card>
        <CardHeader>
          <CardTitle>Organizations</CardTitle>
          <CardDescription>
            {pagination.total > 0 ? `${pagination.total} organizations found` : 'No organizations to display'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <DataGrid
            data={organizations}
            columns={columns}
            loading={loading}
            actions={actions}
            emptyText="No organizations found. Click 'Add Organization' to create a new organization."
            pagination={{
              current: pagination.current,
              pageSize: pagination.pageSize,
              total: pagination.total,
              showSizeChanger: true,
              pageSizeOptions: [10, 20, 50, 100],
              onChange: handlePageChange
            }}
          />
        </CardContent>
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