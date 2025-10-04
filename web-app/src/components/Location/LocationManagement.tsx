"use client"

import { useState, useEffect, useCallback, useRef } from "react"
import { useRouter } from "next/navigation"
import { Plus, Search, Edit, Trash2, MapPin, RefreshCw, Filter, Building2, Globe, Building, MapPinned, Download, Eye, ChevronDown, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"
import { formatDateToDayMonthYear } from "@/utils/date-formatter"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuLabel,
  DropdownMenuCheckboxItem,
} from '@/components/ui/dropdown-menu'
import { useToast } from "@/components/ui/use-toast"
import { locationService, Location, LocationType } from "@/services/locationService"
import { DataGrid, DataGridColumn, DataGridAction } from "@/components/ui/data-grid"

export function LocationManagement() {
  const router = useRouter()
  const { toast } = useToast()
  
  const [locations, setLocations] = useState<Location[]>([])
  const [filteredLocations, setFilteredLocations] = useState<Location[]>([])
  const [locationTypes, setLocationTypes] = useState<LocationType[]>([])
  const [loading, setLoading] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [filterLocationTypes, setFilterLocationTypes] = useState<string[]>([])
  const [filterLevels, setFilterLevels] = useState<string[]>([])
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [locationToDelete, setLocationToDelete] = useState<Location | null>(null)
  const [refreshTrigger, setRefreshTrigger] = useState(0)
  const searchInputRef = useRef<HTMLInputElement>(null)
  
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

  // Fetch locations when refresh is triggered
  useEffect(() => {
    fetchLocations()
  }, [refreshTrigger])

  // Filter locations when filters change
  useEffect(() => {
    filterLocations()
    setPagination(prev => ({ ...prev, current: 1 })) // Reset to first page when filters change
  }, [locations, searchQuery, filterLocationTypes, filterLevels])

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Check for Ctrl+F (Windows/Linux) or Cmd+F (Mac)
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault() // Prevent browser's default find
        searchInputRef.current?.focus()
        searchInputRef.current?.select() // Select existing text for easy replacement
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])

  const fetchLocationTypes = async () => {
    try {
      const types = await locationService.getLocationTypes()
      // Filter to show only location types where ShowInUI is true (same as location types page)
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
      // Get all locations without filters
      const result = await locationService.getLocations(1, 5000)

      setLocations(result.data)
      // Initialize filtered locations with all data initially
      setFilteredLocations(result.data)
      
      // Update pagination total with all data initially
      setPagination(prev => ({ ...prev, total: result.data.length }))
      
      // Update statistics
      setStats({
        total: result.data.length,
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

  const filterLocations = () => {
    let filtered = [...locations]

    // Filter by search query
    if (searchQuery) {
      filtered = filtered.filter(location => 
        location.Name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        location.Code.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    // Filter by selected location types
    if (filterLocationTypes.length > 0) {
      filtered = filtered.filter(location => 
        filterLocationTypes.includes(location.LocationTypeUID || '')
      )
    }

    // Filter by selected levels
    // if (filterLevels.length > 0) {
    //   filtered = filtered.filter(location => 
    //     filterLevels.includes(location.ItemLevel?.toString() || '')
    //   )
    // }

    setFilteredLocations(filtered)
    
    // Update statistics based on filtered data
    setStats({
      total: filtered.length,
      withChildren: filtered.filter(loc => loc.HasChild).length,
      types: locationTypes.length
    })

    // Update pagination total
    setPagination(prev => ({ ...prev, total: filtered.length }))
  }

  const handleSearch = useCallback(() => {
    // Filtering happens automatically via useEffect
    setPagination(prev => ({ ...prev, current: 1 }))
  }, [])

  const clearFilters = () => {
    setSearchQuery("")
    setFilterLocationTypes([])
    setFilterLevels([])
    setPagination(prev => ({ ...prev, current: 1 }))
  }

  const handleCreate = () => {
    router.push("/administration/location-management/locations/create")
  }

  const handleEdit = (location: Location) => {
    router.push(`/administration/location-management/locations/create?id=${location.UID}`)
  }

  const handleView = (location: Location) => {
    router.push(`/administration/location-management/locations/${location.UID}`)
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
          {formatDateToDayMonthYear(record.CreatedTime)}
        </span>
      )
    },
  ]

  const actions: DataGridAction<Location>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: Eye,
      onClick: (record) => handleView(record)
    },
    {
      key: 'edit',
      label: 'Edit',
      icon: Edit,
      onClick: (record) => handleEdit(record)
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
            onClick={async () => {
              try {
                if (filteredLocations.length === 0) {
                  toast({
                    title: "No Data",
                    description: "No locations found to export.",
                    variant: "default",
                  })
                  return
                }

                // Create custom export with filtered data
                const headers = ["Code", "Name", "Location Type", "Level", "Parent", "Has Child"]
                
                // Map current filtered locations to export format
                const exportData = filteredLocations.map(location => {
                  // Find parent location
                  let parentDisplay = "Root"
                  if (location.ParentUID) {
                    const parentLocation = locations.find(loc => loc.UID === location.ParentUID)
                    parentDisplay = parentLocation ? `${parentLocation.Name} (${parentLocation.Code})` : location.ParentUID
                  }
                  
                  return [
                    location.Code || "",
                    location.Name || "",
                    location.LocationTypeName || "",
                    location.ItemLevel?.toString() || "",
                    parentDisplay,
                    location.HasChild ? "Yes" : "No"
                  ]
                })

                // Create CSV content with headers
                const csvContent = [
                  headers.join(","),
                  ...exportData.map(row => 
                    row.map(field => 
                      typeof field === 'string' && field.includes(',') 
                        ? `"${field}"` 
                        : field
                    ).join(",")
                  )
                ].join("\n")

                // Create and download the file
                const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" })
                const url = URL.createObjectURL(blob)
                const a = document.createElement("a")
                a.href = url
                a.download = `locations-export-${new Date().toISOString().split("T")[0]}.csv`
                document.body.appendChild(a)
                a.click()
                document.body.removeChild(a)
                URL.revokeObjectURL(url)
                
                toast({
                  title: "Export Complete",
                  description: `${exportData.length} locations exported successfully to CSV file.`,
                });
              } catch (error) {
                toast({
                  title: "Error", 
                  description: "Failed to export locations. Please try again.",
                  variant: "destructive",
                });
              }
            }}
          >
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
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

      {/* Search and Filters */}
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
            
            {/* Location Type Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Location Type
                  {filterLocationTypes.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterLocationTypes.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>Filter by Location Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {locationTypes.map((type) => (
                  <DropdownMenuCheckboxItem
                    key={type.UID}
                    checked={filterLocationTypes.includes(type.UID)}
                    onCheckedChange={(checked) => {
                      setFilterLocationTypes(prev => 
                        checked 
                          ? [...prev, type.UID]
                          : prev.filter(id => id !== type.UID)
                      )
                    }}
                  >
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-xs">
                        {type.Name} (Level {type.LevelNo})
                      </Badge>
                    </div>
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Level Filter Dropdown - Commented Out */}
            {/* <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Level
                  {filterLevels.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {filterLevels.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Level</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {Array.from(new Set(locationTypes.map(type => type.LevelNo)))
                  .sort((a, b) => a - b)
                  .map((level) => (
                  <DropdownMenuCheckboxItem
                    key={level}
                    checked={filterLevels.includes(level.toString())}
                    onCheckedChange={(checked) => {
                      setFilterLevels(prev => 
                        checked 
                          ? [...prev, level.toString()]
                          : prev.filter(l => l !== level.toString())
                      )
                    }}
                  >
                    <div className="flex items-center gap-2">
                      <div className={`w-2 h-2 rounded-full ${
                        level === 1 ? 'bg-blue-500' :
                        level === 2 ? 'bg-green-500' :
                        level === 3 ? 'bg-yellow-500' :
                        level === 4 ? 'bg-purple-500' :
                        level === 5 ? 'bg-red-500' :
                        'bg-gray-500'
                      }`} />
                      Level {level}
                    </div>
                  </DropdownMenuCheckboxItem>
                ))}
                {filterLevels.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setFilterLevels([])}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu> */}

            {/* Clear All Filters Button */}
            {(searchQuery || filterLocationTypes.length > 0) && (
              <Button
                variant="outline"
                onClick={clearFilters}
              >
                <X className="h-4 w-4 mr-2" />
                Clear All
              </Button>
            )}
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
            data={filteredLocations.slice((pagination.current - 1) * pagination.pageSize, pagination.current * pagination.pageSize)}
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