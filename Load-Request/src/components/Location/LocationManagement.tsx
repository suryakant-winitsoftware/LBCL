"use client"

import { useState, useEffect, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Plus, Search, Edit, Trash2, MapPin, RefreshCw, Filter, Building2, Globe, Building, MapPinned } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { useToast } from "@/components/ui/use-toast"
import { locationService, Location, LocationType } from "@/services/locationService"
import { DataGrid, DataGridColumn, DataGridAction } from "@/components/ui/data-grid"

export function LocationManagement() {
  const router = useRouter()
  const { toast } = useToast()
  
  const [locations, setLocations] = useState<Location[]>([])
  const [locationTypes, setLocationTypes] = useState<LocationType[]>([])
  const [loading, setLoading] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedType, setSelectedType] = useState<string>("all")
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [locationToDelete, setLocationToDelete] = useState<Location | null>(null)
  const [refreshTrigger, setRefreshTrigger] = useState(0)
  
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 20,
    total: 0
  })
  
  // Statistics state
  const [stats, setStats] = useState({
    total: 0,
    withChildren: 0,
    types: 0
  })

  // Fetch location types on mount
  useEffect(() => {
    fetchLocationTypes()
  }, [])

  // Fetch locations when filters change
  useEffect(() => {
    fetchLocations()
  }, [pagination.current, pagination.pageSize, selectedType, refreshTrigger])

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
      const filters = []
      if (selectedType && selectedType !== "all") {
        filters.push({
          Name: "location_type_uid",
          Value: selectedType,
          Type: 0, // 0 = Equal
          FilterType: 0
        })
      }
      
      if (searchQuery) {
        filters.push({
          Name: "name",
          Value: searchQuery,
          Type: 1, // 1 = LIKE
          FilterType: 1
        })
      }

      const result = await locationService.getLocations(
        pagination.current,
        pagination.pageSize,
        filters
      )

      setLocations(result.data)
      setPagination(prev => ({ ...prev, total: result.total }))
      
      // Update statistics
      setStats({
        total: result.total,
        withChildren: result.data.filter(loc => loc.HasChild).length,
        types: locationTypes.length
      })
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

  const handleSearch = useCallback(() => {
    setPagination(prev => ({ ...prev, current: 1 }))
    setRefreshTrigger(prev => prev + 1)
  }, [])

  const handleCreate = () => {
    router.push("/updatedfeatures/location-management/locations/create")
  }

  const handleEdit = (location: Location) => {
    router.push(`/updatedfeatures/location-management/locations/create?id=${location.UID}`)
  }

  const handleDeleteClick = (location: Location) => {
    setLocationToDelete(location)
    setDeleteDialogOpen(true)
  }

  const handleDelete = async () => {
    if (!locationToDelete) return

    try {
      await locationService.deleteLocation(locationToDelete.UID)
      toast({
        title: "Success",
        description: "Location deleted successfully"
      })
      setDeleteDialogOpen(false)
      setLocationToDelete(null)
      setRefreshTrigger(prev => prev + 1)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete location",
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

  const handleBuildHierarchy = async (location: Location) => {
    try {
      setLoading(true)
      const response = await locationService.insertLocationHierarchy(
        location.LocationTypeName || '',
        location.UID
      )
      if (response.IsSuccess) {
        toast({
          title: "Success",
          description: "Location hierarchy built successfully"
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
        description: "Error building location hierarchy",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const getTypeColor = (level: number): "default" | "secondary" | "outline" => {
    // Professional color scheme based on hierarchy level
    switch(level) {
      case 1: return "default"    // Country - primary color
      case 2: return "secondary"  // State/Province - secondary color
      case 3: return "outline"    // Region/District - outline style
      case 4: return "secondary"  // City - secondary color
      case 5: return "outline"    // Area/Zone - outline style
      default: return "outline"   // Others - outline style
    }
  }

  const columns: DataGridColumn<Location>[] = [
    {
      key: "Code",
      title: "Code",
      width: "120px",
      sortable: true,
      render: (value: any, record: Location) => (
        <code className="text-sm bg-gray-100 px-2 py-1 rounded font-mono">{record.Code}</code>
      )
    },
    {
      key: "Name",
      title: "Name",
      sortable: true,
      render: (value: any, record: Location) => (
        <div className="flex items-center gap-2">
          {record.HasChild && <Building2 className="h-4 w-4 text-blue-600" />}
          <span className="font-medium">{record.Name}</span>
        </div>
      )
    },
    {
      key: "LocationTypeName",
      title: "Type",
      render: (value: any, record: Location) => {
        const variant = getTypeColor(record.ItemLevel || 0)
        return (
          <Badge 
            variant={variant}
            className={cn(
              variant === "default" && "bg-blue-100 text-blue-800 hover:bg-blue-100",
              variant === "secondary" && "bg-gray-100 text-gray-800 hover:bg-gray-100",
              variant === "outline" && "border-gray-300"
            )}
          >
            {record.LocationTypeName || "N/A"}
          </Badge>
        )
      }
    },
    {
      key: "ItemLevel",
      title: "Level",
      width: "80px",
      sortable: true,
      render: (value: any, record: Location) => (
        <div className="text-center">
          <span className="inline-flex items-center justify-center w-8 h-8 text-sm font-medium bg-gray-100 rounded-full">
            {record.ItemLevel || 0}
          </span>
        </div>
      )
    },
    {
      key: "ParentUID",
      title: "Parent",
      render: (value: any, record: Location) => {
        if (!record.ParentUID) return <span className="text-gray-400">-</span>
        const parent = locations.find(loc => loc.UID === record.ParentUID)
        return parent ? (
          <span className="text-sm">
            {parent.Name} ({parent.Code})
          </span>
        ) : (
          <span className="text-sm text-gray-400">{record.ParentUID}</span>
        )
      }
    },
    {
      key: "CreatedTime",
      title: "Created",
      width: "150px",
      render: (value: any, record: Location) => (
        <span className="text-sm text-gray-600">
          {record.CreatedTime ? new Date(record.CreatedTime).toLocaleDateString() : '-'}
        </span>
      )
    },
  ]

  const actions: DataGridAction<Location>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: Edit,
      onClick: (record) => handleEdit(record)
    },
    {
      key: 'build-hierarchy',
      label: 'Build Hierarchy',
      icon: Building2,
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
          <h1 className="text-3xl font-bold tracking-tight">Location Management</h1>
          <p className="text-muted-foreground">
            Manage geographical locations and their hierarchy
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
            Add Location
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Locations</CardTitle>
            <Globe className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              All geographical locations
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Hierarchical Nodes</CardTitle>
            <Building className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{stats.withChildren.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Parent locations with sub-regions
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Location Levels</CardTitle>
            <MapPinned className="h-4 w-4 text-purple-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-purple-600">{stats.types.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Hierarchy levels configured
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Search & Filter</CardTitle>
          <CardDescription>Find locations quickly using search and filters</CardDescription>
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
            
            <Select value={selectedType} onValueChange={setSelectedType}>
              <SelectTrigger>
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

            <Button
              variant="outline"
              onClick={() => {
                setSearchQuery("")
                setSelectedType("all")
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

      {/* Locations Table */}
      <Card>
        <CardHeader>
          <CardTitle>Locations</CardTitle>
          <CardDescription>
            {pagination.total > 0 ? `${pagination.total} locations found` : 'No locations to display'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <DataGrid
            data={locations}
            columns={columns}
            loading={loading}
            actions={actions}
            emptyText="No locations found. Click 'Add Location' to create a new location."
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
            <DialogTitle>Delete Location</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete "{locationToDelete?.Name}"? 
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