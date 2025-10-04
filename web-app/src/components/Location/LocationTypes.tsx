"use client"

import { useState, useEffect } from "react"
import { Plus, Edit, Trash2, ArrowUp, Save, X, MapPin, MoreHorizontal, RefreshCw, Download, Eye, Network, Globe, Building, MapPinned } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import { Skeleton } from "@/components/ui/skeleton"
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { locationService, LocationType } from "@/services/locationService"

export function LocationTypes() {
  const { toast } = useToast()
  
  const [locationTypes, setLocationTypes] = useState<LocationType[]>([])
  const [loading, setLoading] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [typeToDelete, setTypeToDelete] = useState<LocationType | null>(null)
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  
  const [formData, setFormData] = useState<Partial<LocationType>>({
    Code: "",
    Name: "",
    LevelNo: 1,
    ShowInUI: true,
    ShowInTemplate: true
  })

  useEffect(() => {
    fetchLocationTypes()
  }, [])

  const fetchLocationTypes = async () => {
    setLoading(true)
    try {
      const types = await locationService.getLocationTypes()
      // Filter to show only location types where ShowInUI is true
      const visibleTypes = types.filter(type => type.ShowInUI !== false)
      setLocationTypes(visibleTypes.sort((a, b) => a.LevelNo - b.LevelNo))
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch location types",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const handleCreate = () => {
    setFormData({
      Code: "",
      Name: "",
      LevelNo: locationTypes.length + 1,
      ShowInUI: true,
      ShowInTemplate: true
    })
    setCreateDialogOpen(true)
  }

  const handleEdit = (type: LocationType) => {
    setEditingId(type.UID)
    setFormData(type)
  }

  const handleSave = async () => {
    if (!formData.Code?.trim() || !formData.Name?.trim()) {
      toast({
        title: "Validation Error",
        description: "Code and name are required",
        variant: "destructive"
      })
      return
    }

    try {
      if (editingId) {
        await locationService.updateLocationType(editingId, formData)
        toast({
          title: "Success",
          description: "Location type updated successfully"
        })
        setEditingId(null)
      } else {
        await locationService.createLocationType(formData)
        toast({
          title: "Success",
          description: "Location type created successfully"
        })
        setCreateDialogOpen(false)
      }
      
      fetchLocationTypes()
      setFormData({
        Code: "",
        Name: "",
        LevelNo: 1,
        ShowInUI: true,
        ShowInTemplate: true
      })
    } catch (error) {
      toast({
        title: "Error",
        description: editingId 
          ? "Failed to update location type" 
          : "Failed to create location type",
        variant: "destructive"
      })
    }
  }

  const handleCancel = () => {
    setEditingId(null)
    setCreateDialogOpen(false)
    setFormData({
      Code: "",
      Name: "",
      LevelNo: 1,
      ShowInUI: true,
      ShowInTemplate: true
    })
  }

  const handleDeleteClick = (type: LocationType) => {
    setTypeToDelete(type)
    setDeleteDialogOpen(true)
  }

  const handleDelete = async () => {
    if (!typeToDelete) return

    try {
      await locationService.deleteLocationType(typeToDelete.UID)
      toast({
        title: "Success",
        description: "Location type deleted successfully"
      })
      setDeleteDialogOpen(false)
      setTypeToDelete(null)
      fetchLocationTypes()
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete location type. It may be in use.",
        variant: "destructive"
      })
    }
  }


  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Location Types</h1>
          <p className="text-muted-foreground">
            Configure hierarchical levels for your geographical locations
          </p>
        </div>
        <div className="flex gap-2">
          <Button 
            variant="outline" 
            onClick={() => fetchLocationTypes()}
            disabled={loading}
          >
            <RefreshCw className={loading ? "mr-2 h-4 w-4 animate-spin" : "mr-2 h-4 w-4"} />
            Refresh
          </Button>
          <Button 
            variant="outline" 
            onClick={async () => {
              try {
                const blob = await locationService.exportLocationTypes("csv");
                const url = URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = `location-types-export-${new Date().toISOString().split("T")[0]}.csv`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
                
                toast({
                  title: "Success",
                  description: "Location types exported successfully.",
                });
              } catch (error) {
                toast({
                  title: "Error",
                  description: "Failed to export location types. Please try again.",
                  variant: "destructive",
                });
              }
            }}
          >
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button onClick={handleCreate}>
            <Plus className="mr-2 h-4 w-4" />
            Add Location Type
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-4 gap-4">
        {loading ? (
          [...Array(4)].map((_, i) => (
            <Card key={i} className="p-4">
              <Skeleton className="h-4 w-24 mb-2" />
              <Skeleton className="h-8 w-12" />
            </Card>
          ))
        ) : (
          <>
            <Card className="p-4">
              <div className="flex items-center space-x-2">
                <Network className="h-5 w-5 text-blue-600" />
                <div>
                  <p className="text-sm font-medium text-gray-600">Total Types</p>
                  <p className="text-2xl font-bold text-blue-600">{locationTypes.length}</p>
                </div>
              </div>
            </Card>
            <Card className="p-4">
              <div className="flex items-center space-x-2">
                <Eye className="h-5 w-5 text-green-600" />
                <div>
                  <p className="text-sm font-medium text-gray-600">UI Visible</p>
                  <p className="text-2xl font-bold text-green-600">
                    {locationTypes.filter(t => t.ShowInUI).length}
                  </p>
                </div>
              </div>
            </Card>
            <Card className="p-4">
              <div className="flex items-center space-x-2">
                <MapPin className="h-5 w-5 text-purple-600" />
                <div>
                  <p className="text-sm font-medium text-gray-600">Template Types</p>
                  <p className="text-2xl font-bold text-purple-600">
                    {locationTypes.filter(t => t.ShowInTemplate).length}
                  </p>
                </div>
              </div>
            </Card>
            <Card className="p-4">
              <div className="flex items-center space-x-2">
                <ArrowUp className="h-5 w-5 text-orange-600" />
                <div>
                  <p className="text-sm font-medium text-gray-600">Max Level</p>
                  <p className="text-2xl font-bold text-orange-600">
                    {locationTypes.length > 0 ? Math.max(...locationTypes.map(t => t.LevelNo)) : 0}
                  </p>
                </div>
              </div>
            </Card>
          </>
        )}
      </div>

      {/* Location Types Grid */}
      <Card className="shadow-lg">
        <CardHeader className="bg-gradient-to-r from-blue-50 to-blue-100 border-b">
          <CardTitle className="text-xl">Hierarchy Configuration</CardTitle>
          <CardDescription className="text-gray-600">
            Define and organize your location structure levels
          </CardDescription>
        </CardHeader>
        <CardContent className="p-6">
          {loading ? (
            <div className="grid gap-4">
              {[...Array(3)].map((_, i) => (
                <div key={i} className="p-6 border rounded-lg">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                      <Skeleton className="w-12 h-12 rounded-full" />
                      <div className="space-y-2">
                        <Skeleton className="h-5 w-32" />
                        <Skeleton className="h-4 w-24" />
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Skeleton className="h-5 w-20 rounded-full" />
                      <Skeleton className="h-5 w-16 rounded-full" />
                    </div>
                    <Skeleton className="h-8 w-8 rounded" />
                  </div>
                </div>
              ))}
            </div>
          ) : locationTypes.length === 0 ? (
            <div className="text-center py-12">
              <Network className="h-16 w-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">No location types configured</h3>
              <p className="text-gray-500 mb-4">
                Define your geographical hierarchy by creating location types
              </p>
              <Button onClick={handleCreate} className="mx-auto">
                <Plus className="mr-2 h-4 w-4" />
                Create Your First Location Type
              </Button>
            </div>
          ) : (
            <div className="grid gap-4">
              {locationTypes.map((type, index) => {
                // Get icon based on typical location type patterns
                const getTypeIcon = (name: string, level: number) => {
                  const lowerName = name.toLowerCase()
                  if (lowerName.includes('country') || level === 1) {
                    return <Globe className="h-5 w-5 text-blue-600" />
                  } else if (lowerName.includes('state') || lowerName.includes('region') || level === 2) {
                    return <Building className="h-5 w-5 text-green-600" />
                  } else if (lowerName.includes('city') || lowerName.includes('district') || level === 3) {
                    return <MapPinned className="h-5 w-5 text-purple-600" />
                  } else {
                    return <MapPin className="h-5 w-5 text-orange-600" />
                  }
                }

                const getTypeColor = (level: number) => {
                  const colors = ['blue', 'green', 'purple', 'orange', 'pink', 'yellow']
                  return colors[(level - 1) % colors.length]
                }

                const color = getTypeColor(type.LevelNo)

                return (
                  <div
                    key={type.UID}
                    className={`group relative p-6 rounded-lg border-2 transition-all duration-200 hover:shadow-lg hover:scale-[1.01] bg-gradient-to-r from-${color}-50 to-${color}-100 border-${color}-200`}
                  >
                    {editingId === type.UID ? (
                      // Edit Mode
                      <div className="space-y-4">
                        <div className="flex items-center gap-4">
                          <div className={`p-3 rounded-full bg-${color}-200`}>
                            {getTypeIcon(formData.Name || '', formData.LevelNo || 1)}
                          </div>
                          <div className="flex-1 grid grid-cols-3 gap-4">
                            <div>
                              <Label className="text-sm font-medium">Level</Label>
                              <div className={`w-12 h-12 rounded-full bg-${color}-200 text-${color}-800 flex items-center justify-center font-bold text-lg`}>
                                {formData.LevelNo}
                              </div>
                            </div>
                            <div>
                              <Label className="text-sm font-medium">Code</Label>
                              <Input
                                value={formData.Code || ""}
                                onChange={(e) => setFormData({ ...formData, Code: e.target.value })}
                                className="font-mono"
                              />
                            </div>
                            <div>
                              <Label className="text-sm font-medium">Name</Label>
                              <Input
                                value={formData.Name || ""}
                                onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                              />
                            </div>
                          </div>
                        </div>
                        
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-6">
                            <label className="flex items-center gap-2">
                              <Switch
                                checked={formData.ShowInUI}
                                onCheckedChange={(checked) => 
                                  setFormData({ ...formData, ShowInUI: checked })
                                }
                              />
                              <span className="text-sm font-medium">Show in UI</span>
                            </label>
                            <label className="flex items-center gap-2">
                              <Switch
                                checked={formData.ShowInTemplate}
                                onCheckedChange={(checked) => 
                                  setFormData({ ...formData, ShowInTemplate: checked })
                                }
                              />
                              <span className="text-sm font-medium">Show in Template</span>
                            </label>
                          </div>
                          
                          <div className="flex gap-2">
                            <Button variant="outline" size="sm" onClick={handleSave}>
                              <Save className="h-4 w-4 mr-1" />
                              Save
                            </Button>
                            <Button variant="ghost" size="sm" onClick={handleCancel}>
                              <X className="h-4 w-4 mr-1" />
                              Cancel
                            </Button>
                          </div>
                        </div>
                      </div>
                    ) : (
                      // View Mode
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className={`p-3 rounded-full bg-${color}-200`}>
                            {getTypeIcon(type.Name, type.LevelNo)}
                          </div>
                          <div>
                            <div className="flex items-center gap-3 mb-2">
                              <h3 className="text-lg font-semibold text-gray-900">{type.Name}</h3>
                              <Badge variant="outline" className={`font-mono border-${color}-300 text-${color}-700`}>
                                {type.Code}
                              </Badge>
                              <Badge className={`bg-${color}-200 text-${color}-800 border-${color}-300`}>
                                Level {type.LevelNo}
                              </Badge>
                            </div>
                            <div className="flex gap-2">
                              {type.ShowInUI && (
                                <Badge variant="outline" className="text-xs border-green-300 text-green-700">
                                  <Eye className="w-3 h-3 mr-1" />
                                  UI Visible
                                </Badge>
                              )}
                              {type.ShowInTemplate && (
                                <Badge variant="outline" className="text-xs border-blue-300 text-blue-700">
                                  <MapPin className="w-3 h-3 mr-1" />
                                  Template
                                </Badge>
                              )}
                            </div>
                          </div>
                        </div>
                        
                        <div className="flex items-center gap-2">
                          {/* Action Menu */}
                          <div className="opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                                  <MoreHorizontal className="h-4 w-4" />
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end" className="w-48">
                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem onClick={() => handleEdit(type)}>
                                  <Edit className="mr-2 h-4 w-4" />
                                  Edit Type
                                </DropdownMenuItem>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem 
                                  onClick={() => handleDeleteClick(type)}
                                  className="text-red-600 hover:text-red-700 hover:bg-red-50"
                                >
                                  <Trash2 className="mr-2 h-4 w-4" />
                                  Delete Type
                                </DropdownMenuItem>
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                )
              })}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Create Dialog */}
      <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Network className="h-5 w-5 text-blue-600" />
              Create Location Type
            </DialogTitle>
            <DialogDescription>
              Add a new level to your geographical location hierarchy
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="code">Code</Label>
              <Input
                id="code"
                value={formData.Code || ""}
                onChange={(e) => setFormData({ ...formData, Code: e.target.value })}
                placeholder="e.g., COUNTRY, STATE"
                className="font-mono uppercase"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                value={formData.Name || ""}
                onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                placeholder="e.g., Country, State"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="level">Level</Label>
              <Input
                id="level"
                type="number"
                min="1"
                value={formData.LevelNo || 1}
                onChange={(e) => setFormData({ ...formData, LevelNo: parseInt(e.target.value) })}
              />
            </div>
            <div className="space-y-2">
              <div className="flex items-center space-x-2">
                <Switch
                  id="showInUI"
                  checked={formData.ShowInUI}
                  onCheckedChange={(checked) => setFormData({ ...formData, ShowInUI: checked })}
                />
                <Label htmlFor="showInUI">Show in UI</Label>
              </div>
            </div>
            <div className="space-y-2">
              <div className="flex items-center space-x-2">
                <Switch
                  id="showInTemplate"
                  checked={formData.ShowInTemplate}
                  onCheckedChange={(checked) => setFormData({ ...formData, ShowInTemplate: checked })}
                />
                <Label htmlFor="showInTemplate">Show in Template</Label>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={handleCancel}>
              Cancel
            </Button>
            <Button onClick={handleSave}>
              Create
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-red-600">
              <Trash2 className="h-5 w-5" />
              Delete Location Type
            </DialogTitle>
            <DialogDescription>
              Are you sure you want to delete &quot;<span className="font-semibold">{typeToDelete?.Name}</span>&quot;? 
              This action cannot be undone and may affect existing locations using this type.
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