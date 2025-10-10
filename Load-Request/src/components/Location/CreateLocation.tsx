"use client"

import { useState, useEffect } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { ArrowLeft, Save } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"
import { locationService, Location, LocationType } from "@/services/locationService"
import { Switch } from "@/components/ui/switch"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"

export function CreateLocation() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { toast } = useToast()
  
  const locationId = searchParams.get("id")
  const isEditMode = !!locationId
  
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [locationTypes, setLocationTypes] = useState<LocationType[]>([])
  const [locations, setLocations] = useState<Location[]>([])
  const [parentLocations, setParentLocations] = useState<Location[]>([])
  
  const [formData, setFormData] = useState<Partial<Location>>({
    Code: "",
    Name: "",
    LocationTypeUID: "",
    ParentUID: null,
    HasChild: false
  })

  useEffect(() => {
    fetchInitialData()
    if (isEditMode && locationId) {
      fetchLocationDetails(locationId)
    }
  }, [isEditMode, locationId])

  useEffect(() => {
    if (formData.LocationTypeUID && locations.length > 0) {
      fetchParentLocations(formData.LocationTypeUID)
    }
  }, [formData.LocationTypeUID, locations])

  const fetchInitialData = async () => {
    setLoading(true)
    try {
      // Fetch location types
      const types = await locationService.getLocationTypes()
      setLocationTypes(types.sort((a, b) => a.LevelNo - b.LevelNo))
      
      // Fetch all locations for parent selection
      const result = await locationService.getLocations(1, 1000)
      setLocations(result.data)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch initial data",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const fetchLocationDetails = async (id: string) => {
    setLoading(true)
    try {
      const location = await locationService.getLocationById(id)
      setFormData(location)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch location details",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const fetchParentLocations = (locationTypeUID: string) => {
    const selectedType = locationTypes.find(t => t.UID === locationTypeUID)
    if (!selectedType || selectedType.LevelNo === 1) {
      setParentLocations([])
      return
    }

    // Get parent type using ParentUID property of the location type
    const parentType = locationTypes.find(t => t.UID === selectedType.ParentUID)
    if (!parentType) {
      setParentLocations([])
      return
    }

    // Filter locations by parent type in JavaScript (like React web-portal)
    const parentLocations = locations.filter(loc => loc.LocationTypeUID === parentType.UID)
    
    setParentLocations(parentLocations)
  }

  const validateForm = (): boolean => {
    if (!formData.Code?.trim()) {
      toast({
        title: "Validation Error",
        description: "Location code is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.Name?.trim()) {
      toast({
        title: "Validation Error",
        description: "Location name is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.LocationTypeUID) {
      toast({
        title: "Validation Error",
        description: "Location type is required",
        variant: "destructive"
      })
      return false
    }

    // Check if parent location is required
    const selectedType = locationTypes.find(t => t.UID === formData.LocationTypeUID)
    if (selectedType && selectedType.LevelNo > 1 && (!formData.ParentUID || formData.ParentUID === "none")) {
      toast({
        title: "Validation Error",
        description: "Parent location is required for this location type",
        variant: "destructive"
      })
      return false
    }

    return true
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) return

    setSaving(true)
    try {
      // Clean up the form data before submission
      const submitData = {
        ...formData,
        ParentUID: formData.ParentUID === "none" ? null : formData.ParentUID
      }
      
      if (isEditMode && locationId) {
        await locationService.updateLocation(locationId, submitData)
        toast({
          title: "Success",
          description: "Location updated successfully"
        })
      } else {
        await locationService.createLocation(submitData)
        toast({
          title: "Success",
          description: "Location created successfully"
        })
      }
      
      router.push("/updatedfeatures/location-management/locations/manage")
    } catch (error) {
      toast({
        title: "Error",
        description: isEditMode ? "Failed to update location" : "Failed to create location",
        variant: "destructive"
      })
    } finally {
      setSaving(false)
    }
  }

  const handleInputChange = (field: keyof Location, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }))
  }

  const getLocationTypeName = (uid: string) => {
    return locationTypes.find(t => t.UID === uid)?.Name || ""
  }

  if (loading) {
    return (
      <div className="space-y-6">
        {/* Page Header Skeleton */}
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10 rounded" />
          <div>
            <Skeleton className="h-8 w-48 mb-2" />
            <Skeleton className="h-4 w-64" />
          </div>
        </div>

        {/* Form Skeleton */}
        <div className="max-w-2xl">
          <div className="bg-white rounded-lg border p-6 space-y-6">
            <div>
              <Skeleton className="h-6 w-32 mb-2" />
              <Skeleton className="h-4 w-48" />
            </div>
            
            <div className="grid gap-6 md:grid-cols-2">
              {/* Form fields skeleton */}
              {[...Array(6)].map((_, i) => (
                <div key={i} className="space-y-2">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-10 w-full rounded" />
                  <Skeleton className="h-3 w-32" />
                </div>
              ))}
            </div>
            
            {/* Action buttons skeleton */}
            <div className="flex justify-end gap-4 pt-4">
              <Skeleton className="h-10 w-20 rounded" />
              <Skeleton className="h-10 w-24 rounded" />
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => router.push("/updatedfeatures/location-management/locations/manage")}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {isEditMode ? "Edit Location" : "Create Location"}
          </h1>
          <p className="text-muted-foreground">
            {isEditMode ? "Update location details" : "Add a new location to the system"}
          </p>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Location Details</CardTitle>
            <CardDescription>
              Enter the location information below
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-6 md:grid-cols-2">
              {/* Location Code */}
              <div className="space-y-2">
                <Label htmlFor="code" className="text-sm font-medium">
                  Location Code <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="code"
                  value={formData.Code || ""}
                  onChange={(e) => handleInputChange("Code", e.target.value.toUpperCase())}
                  placeholder="e.g., NYC, LON, TKY"
                  className="font-mono"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Unique identifier for this location (will be converted to uppercase)
                </p>
              </div>

              {/* Location Name */}
              <div className="space-y-2">
                <Label htmlFor="name" className="text-sm font-medium">
                  Location Name <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="name"
                  value={formData.Name || ""}
                  onChange={(e) => handleInputChange("Name", e.target.value)}
                  placeholder="e.g., New York City, London, Tokyo"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Display name for this location
                </p>
              </div>

              {/* Location Type */}
              <div className="space-y-2">
                <Label htmlFor="locationType" className="text-sm font-medium">
                  Location Type <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.LocationTypeUID || ""}
                  onValueChange={(value) => handleInputChange("LocationTypeUID", value)}
                >
                  <SelectTrigger id="locationType">
                    <SelectValue placeholder="Select location type" />
                  </SelectTrigger>
                  <SelectContent>
                    {locationTypes.map((type) => (
                      <SelectItem key={type.UID} value={type.UID}>
                        <div className="flex items-center justify-between w-full">
                          <span>{type.Name}</span>
                          <Badge variant="outline" className="ml-2 text-xs">
                            Level {type.LevelNo}
                          </Badge>
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Select the hierarchical level for this location
                </p>
              </div>

              {/* Parent Location */}
              <div className="space-y-2">
                <Label htmlFor="parentLocation">
                  Parent Location
                  {(() => {
                    const selectedType = locationTypes.find(t => t.UID === formData.LocationTypeUID)
                    const parentType = selectedType ? locationTypes.find(t => t.UID === selectedType.ParentUID) : null
                    return selectedType && selectedType.LevelNo > 1 && parentType && (
                      <span className="text-sm text-muted-foreground ml-2">
                        ({parentType.Name})
                      </span>
                    )
                  })()}
                </Label>
                <Select
                  value={formData.ParentUID || "none"}
                  onValueChange={(value) => handleInputChange("ParentUID", value === "none" ? null : value)}
                  disabled={!formData.LocationTypeUID || parentLocations.length === 0}
                >
                  <SelectTrigger id="parentLocation">
                    <SelectValue placeholder={
                      !formData.LocationTypeUID 
                        ? "Select location type first"
                        : parentLocations.length === 0 
                        ? "No parent locations available" 
                        : "Select parent location"
                    } />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    {parentLocations.map((location) => (
                      <SelectItem key={location.UID} value={location.UID}>
                        {location.Name} ({location.Code})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Has Children */}
              <div className="space-y-2">
                <Label className="text-sm font-medium">Location Properties</Label>
                <div className="flex items-center space-x-3 p-4 border rounded-lg bg-gray-50">
                  <Switch
                    id="hasChild"
                    checked={formData.HasChild || false}
                    onCheckedChange={(checked) => handleInputChange("HasChild", checked)}
                  />
                  <div className="flex-1">
                    <Label htmlFor="hasChild" className="font-medium text-sm cursor-pointer">
                      Enable child locations
                    </Label>
                    <p className="text-xs text-muted-foreground mt-1">
                      Allow this location to have sub-locations in the hierarchy
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Selected Type Info */}
            {formData.LocationTypeUID && (() => {
              const selectedType = locationTypes.find(t => t.UID === formData.LocationTypeUID)
              const parentType = selectedType ? locationTypes.find(t => t.UID === selectedType.ParentUID) : null
              return selectedType && (
                <div className="col-span-2 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <h4 className="text-sm font-medium text-blue-900 mb-2">Hierarchy Information</h4>
                  <div className="space-y-2">
                    <p className="text-sm text-blue-800">
                      <span className="font-medium">Current Level:</span> {selectedType.Name} 
                      <Badge variant="outline" className="ml-2 text-xs border-blue-300">
                        Level {selectedType.LevelNo}
                      </Badge>
                    </p>
                    {selectedType.LevelNo > 1 && parentType && (
                      <p className="text-sm text-blue-800">
                        <span className="font-medium">Parent Level:</span> {parentType.Name}
                        <span className="text-xs text-blue-600 ml-2">
                          (You must select a {parentType.Name} as parent)
                        </span>
                      </p>
                    )}
                  </div>
                </div>
              )
            })()}

            {/* Action Buttons */}
            <div className="flex justify-end gap-4 pt-4 col-span-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push("/updatedfeatures/location-management/locations/manage")}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={saving}>
                {saving ? (
                  <>
                    <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 h-4 w-4" />
                    {isEditMode ? "Update" : "Create"}
                  </>
                )}
              </Button>
            </div>
          </CardContent>
        </Card>
      </form>
    </div>
  )
}