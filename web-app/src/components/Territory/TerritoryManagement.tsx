"use client"

import { useState, useEffect, useCallback, useRef } from "react"
import { useRouter } from "next/navigation"
import { Plus, Search, Edit, Trash2, RefreshCw, Filter, Building2, Globe, Building, Download, Eye, ChevronDown, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"
import { formatDateToDayMonthYear } from "@/utils/date-formatter"
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
import { territoryService, Territory } from "@/services/territoryService"
import { DataGrid, DataGridColumn, DataGridAction } from "@/components/ui/data-grid"

export function TerritoryManagement() {
  const router = useRouter()
  const { toast } = useToast()

  const [territories, setTerritories] = useState<Territory[]>([])
  const [filteredTerritories, setFilteredTerritories] = useState<Territory[]>([])
  const [loading, setLoading] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [territoryToDelete, setTerritoryToDelete] = useState<Territory | null>(null)
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
    import: 0,
    local: 0
  })

  // Fetch territories when refresh is triggered
  useEffect(() => {
    fetchTerritories()
  }, [refreshTrigger])

  // Filter territories when filters change
  useEffect(() => {
    filterTerritories()
    setPagination(prev => ({ ...prev, current: 1 }))
  }, [territories, searchQuery])

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault()
        searchInputRef.current?.focus()
        searchInputRef.current?.select()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])

  const fetchTerritories = async () => {
    setLoading(true)
    try {
      const result = await territoryService.getTerritories(1, 5000)

      setTerritories(result.data)
      setFilteredTerritories(result.data)

      setPagination(prev => ({ ...prev, total: result.data.length }))

      // Update statistics
      setStats({
        total: result.data.length,
        withChildren: result.data.filter(ter => ter.HasChild).length,
        import: result.data.filter(ter => ter.IsImport).length,
        local: result.data.filter(ter => ter.IsLocal).length
      })
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch territories",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const filterTerritories = () => {
    let filtered = [...territories]

    // Filter by search query
    if (searchQuery) {
      filtered = filtered.filter(territory =>
        territory.TerritoryName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        territory.TerritoryCode.toLowerCase().includes(searchQuery.toLowerCase()) ||
        (territory.ClusterCode && territory.ClusterCode.toLowerCase().includes(searchQuery.toLowerCase()))
      )
    }

    setFilteredTerritories(filtered)

    // Update statistics based on filtered data
    setStats({
      total: filtered.length,
      withChildren: filtered.filter(ter => ter.HasChild).length,
      import: filtered.filter(ter => ter.IsImport).length,
      local: filtered.filter(ter => ter.IsLocal).length
    })

    // Update pagination total
    setPagination(prev => ({ ...prev, total: filtered.length }))
  }

  const clearFilters = () => {
    setSearchQuery("")
    setPagination(prev => ({ ...prev, current: 1 }))
  }

  const handleCreate = () => {
    router.push("/administration/territory-management/territories/create")
  }

  const handleEdit = (territory: Territory) => {
    router.push(`/administration/territory-management/territories/${territory.UID}`)
  }

  const handleView = (territory: Territory) => {
    router.push(`/administration/territory-management/territories/${territory.UID}`)
  }

  const handleDeleteClick = (territory: Territory) => {
    setTerritoryToDelete(territory)
    setDeleteDialogOpen(true)
  }

  const handleDelete = async () => {
    if (!territoryToDelete) return

    try {
      await territoryService.deleteTerritory(territoryToDelete.UID)
      toast({
        title: "Success",
        description: "Territory deleted successfully"
      })
      setDeleteDialogOpen(false)
      setTerritoryToDelete(null)
      setRefreshTrigger(prev => prev + 1)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete territory",
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

  const columns: DataGridColumn<Territory>[] = [
    {
      key: "TerritoryCode",
      title: "Code",
      width: "120px",
      sortable: true,
      render: (value: any, record: Territory) => (
        <code className="text-sm bg-gray-100 px-2 py-1 rounded font-mono">{record.TerritoryCode}</code>
      )
    },
    {
      key: "TerritoryName",
      title: "Name",
      sortable: true,
      render: (value: any, record: Territory) => (
        <div className="flex items-center gap-2">
          {record.HasChild && <Building2 className="h-4 w-4 text-blue-600" />}
          <span className="font-medium">{record.TerritoryName}</span>
        </div>
      )
    },
    {
      key: "ClusterCode",
      title: "Cluster",
      render: (value: any, record: Territory) => (
        record.ClusterCode ? (
          <Badge variant="secondary">{record.ClusterCode}</Badge>
        ) : (
          <span className="text-gray-400">-</span>
        )
      )
    },
    {
      key: "ItemLevel",
      title: "Level",
      width: "80px",
      sortable: true,
      render: (value: any, record: Territory) => (
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
      render: (value: any, record: Territory) => {
        if (!record.ParentUID) return <span className="text-gray-400">-</span>
        const parent = territories.find(ter => ter.UID === record.ParentUID)
        return parent ? (
          <span className="text-sm">
            {parent.TerritoryName} ({parent.TerritoryCode})
          </span>
        ) : (
          <span className="text-sm text-gray-400">{record.ParentUID}</span>
        )
      }
    },
    {
      key: "Type",
      title: "Type",
      render: (value: any, record: Territory) => (
        <div className="flex gap-1">
          {record.IsImport && <Badge variant="default" className="bg-blue-100 text-blue-800">Import</Badge>}
          {record.IsLocal && <Badge variant="default" className="bg-green-100 text-green-800">Local</Badge>}
          {record.IsNonLicense === 1 && <Badge variant="outline">Non-License</Badge>}
        </div>
      )
    },
    {
      key: "Status",
      title: "Status",
      render: (value: any, record: Territory) => (
        record.IsActive ? (
          <Badge variant="default" className="bg-green-100 text-green-800">Active</Badge>
        ) : (
          <Badge variant="secondary">Inactive</Badge>
        )
      )
    },
    {
      key: "CreatedTime",
      title: "Created",
      width: "150px",
      render: (value: any, record: Territory) => (
        <span className="text-sm text-gray-600">
          {formatDateToDayMonthYear(record.CreatedTime)}
        </span>
      )
    },
  ]

  const actions: DataGridAction<Territory>[] = [
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
          <h1 className="text-3xl font-bold tracking-tight">Territory Management</h1>
          <p className="text-muted-foreground">
            Manage sales territories and their hierarchy
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            onClick={async () => {
              try {
                if (filteredTerritories.length === 0) {
                  toast({
                    title: "No Data",
                    description: "No territories found to export.",
                    variant: "default",
                  })
                  return
                }

                const headers = ["Code", "Name", "Cluster", "Level", "Parent", "Import", "Local", "Non-License", "Status"]

                const exportData = filteredTerritories.map(territory => {
                  let parentDisplay = "Root"
                  if (territory.ParentUID) {
                    const parentTerritory = territories.find(ter => ter.UID === territory.ParentUID)
                    parentDisplay = parentTerritory ? `${parentTerritory.TerritoryName} (${parentTerritory.TerritoryCode})` : territory.ParentUID
                  }

                  return [
                    territory.TerritoryCode || "",
                    territory.TerritoryName || "",
                    territory.ClusterCode || "",
                    territory.ItemLevel?.toString() || "",
                    parentDisplay,
                    territory.IsImport ? "Yes" : "No",
                    territory.IsLocal ? "Yes" : "No",
                    territory.IsNonLicense === 1 ? "Yes" : "No",
                    territory.IsActive ? "Active" : "Inactive"
                  ]
                })

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

                const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" })
                const url = URL.createObjectURL(blob)
                const a = document.createElement("a")
                a.href = url
                a.download = `territories-export-${new Date().toISOString().split("T")[0]}.csv`
                document.body.appendChild(a)
                a.click()
                document.body.removeChild(a)
                URL.revokeObjectURL(url)

                toast({
                  title: "Export Complete",
                  description: `${exportData.length} territories exported successfully to CSV file.`,
                });
              } catch (error) {
                toast({
                  title: "Error",
                  description: "Failed to export territories. Please try again.",
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
            Add Territory
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Territories</CardTitle>
            <Globe className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              All sales territories
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
              Parent territories with sub-regions
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Import Territories</CardTitle>
            <Download className="h-4 w-4 text-purple-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-purple-600">{stats.import.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Import business territories
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Local Territories</CardTitle>
            <Building2 className="h-4 w-4 text-orange-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-orange-600">{stats.local.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Local business territories
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
                placeholder="Search by name, code, or cluster... (Ctrl+F)"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {searchQuery && (
              <Button
                variant="outline"
                onClick={clearFilters}
              >
                <X className="h-4 w-4 mr-2" />
                Clear
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Territories Table */}
      <Card>
        <CardHeader>
          <CardTitle>Territories</CardTitle>
          <CardDescription>
            {pagination.total > 0 ? `${pagination.total} territories found` : 'No territories to display'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <DataGrid
            data={filteredTerritories.slice((pagination.current - 1) * pagination.pageSize, pagination.current * pagination.pageSize)}
            columns={columns}
            loading={loading}
            actions={actions}
            emptyText="No territories found. Click 'Add Territory' to create a new territory."
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
            <DialogTitle>Delete Territory</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete "{territoryToDelete?.TerritoryName}"?
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
