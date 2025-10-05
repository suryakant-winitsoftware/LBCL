"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { ArrowLeft, Save, Globe, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"
import { vehicleService, Vehicle } from "@/services/vehicleService"
import { territoryService, Territory } from "@/services/territoryService"
import { locationService, Location, LocationType } from "@/services/locationService"
import { organizationService } from "@/services/organizationService"
import { authService } from "@/lib/auth-service"
import { Switch } from "@/components/ui/switch"

// Import location hierarchy utilities
import {
  handleLocationSelection,
  resetLocationHierarchy,
  getFinalSelectedLocation,
  initializeLocationHierarchy,
  LocationLevel,
  getLocationDisplayName
} from "@/utils/locationHierarchy"

// Import organization hierarchy utilities
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
} from "@/utils/organizationHierarchyUtils"

export function CreateVehicle() {
  const router = useRouter()
  const { toast } = useToast()

  const [saving, setSaving] = useState(false)
  const [territories, setTerritories] = useState<Territory[]>([])
  const [locations, setLocations] = useState<Location[]>([])
  const [locationTypes, setLocationTypes] = useState<LocationType[]>([])
  const [locationLevels, setLocationLevels] = useState<LocationLevel[]>([])
  const [selectedLocations, setSelectedLocations] = useState<string[]>([])

  // Organization hierarchy state
  const [organizations, setOrganizations] = useState<any[]>([])
  const [orgTypes, setOrgTypes] = useState<any[]>([])
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([])
  const [selectedOrgs, setSelectedOrgs] = useState<{ [key: number]: string }>({})

  const [formData, setFormData] = useState<Partial<Vehicle>>({
    VehicleNo: "",
    RegistrationNo: "",
    Model: "",
    Type: "",
    IsActive: true,
    TruckSIDate: new Date().toISOString().split('T')[0],
    RoadTaxExpiryDate: new Date().toISOString().split('T')[0],
    InspectionDate: new Date().toISOString().split('T')[0],
    WeightLimit: null,
    Capacity: null,
    LoadingCapacity: null,
    WarehouseCode: "",
    LocationCode: "",
    TerritoryUID: "",
    OrgUID: ""
  })

  // Load territories, locations, and organizations on mount
  useEffect(() => {
    let isMounted = true
    let hasLoaded = false

    const loadData = async () => {
      // Prevent duplicate calls
      if (hasLoaded) {
        console.log("CreateVehicle: Skipping duplicate loadData call")
        return
      }

      hasLoaded = true
      console.log("CreateVehicle: Starting data load...")

      try {
        // Load territories
        console.log("CreateVehicle: Loading territories...")
        const territoriesResult = await territoryService.getTerritories(1, 1000)
        if (isMounted) {
          if (territoriesResult.data && territoriesResult.data.length > 0) {
            console.log(`CreateVehicle: Loaded ${territoriesResult.data.length} territories`)
            setTerritories(territoriesResult.data)
          } else {
            console.log("CreateVehicle: No territories found")
            setTerritories([])
          }
        }

        // Load organization types and organizations
        console.log("CreateVehicle: Loading organizations...")
        const [typesResult, orgsResult] = await Promise.all([
          organizationService.getOrganizationTypes(),
          organizationService.getOrganizations(1, 1000),
        ])

        if (isMounted) {
          // Filter to only show active organizations with ShowInTemplate
          const activeOrgs = orgsResult.data.filter(
            (org: any) => org.ShowInTemplate === true
          )

          console.log(`CreateVehicle: Loaded ${activeOrgs.length} organizations`)
          setOrganizations(activeOrgs)
          setOrgTypes(typesResult)

          // Initialize organization hierarchy
          const initialOrgLevels = initializeOrganizationHierarchy(
            activeOrgs,
            typesResult
          )
          setOrgLevels(initialOrgLevels)
        }

        // Load location types
        console.log("CreateVehicle: Loading location types...")
        const locationTypesResult = await locationService.getLocationTypes()
        if (isMounted) {
          if (locationTypesResult.success && locationTypesResult.data) {
            console.log(`CreateVehicle: Loaded ${locationTypesResult.data.length} location types`)
            setLocationTypes(locationTypesResult.data)
          } else {
            console.log("CreateVehicle: No location types found")
            setLocationTypes([])
          }
        }

        // Load locations (warehouses)
        console.log("CreateVehicle: Loading locations...")
        const locationsResult = await locationService.getLocations(1, 1000)
        if (isMounted) {
          if (locationsResult.data && locationsResult.data.length > 0) {
            console.log(`CreateVehicle: Loaded ${locationsResult.data.length} locations`)
            setLocations(locationsResult.data)

            // Initialize location hierarchy
            if (locationTypesResult.success && locationTypesResult.data && locationTypesResult.data.length > 0) {
              const initialLevels = initializeLocationHierarchy(
                locationsResult.data,
                locationTypesResult.data
              )
              setLocationLevels(initialLevels)
              setSelectedLocations([])
            } else {
              setLocationLevels([])
              setSelectedLocations([])
            }
          } else {
            // No locations available - set empty array to stop loading
            console.log("CreateVehicle: No locations found")
            setLocations([])
            setLocationLevels([])
            setSelectedLocations([])
          }
        }

        console.log("CreateVehicle: Data load completed successfully")
      } catch (error) {
        console.error("CreateVehicle: Error loading data:", error)
        // Set empty arrays to stop loading state
        if (isMounted) {
          setTerritories([])
          setOrganizations([])
          setOrgLevels([])
          setLocations([])
          setLocationLevels([])
          setSelectedLocations([])
          toast({
            title: "Warning",
            description: "Failed to load data",
            variant: "destructive"
          })
        }
      }
    }

    loadData()

    return () => {
      console.log("CreateVehicle: Component unmounting, cleaning up...")
      isMounted = false
    }
  }, [])

  // Handle location selection
  const handleLocationSelect = (levelIndex: number, locationUID: string) => {
    const result = handleLocationSelection(
      levelIndex,
      locationUID,
      locationLevels,
      selectedLocations,
      locations,
      locationTypes
    )

    setLocationLevels(result.updatedLevels)
    setSelectedLocations(result.updatedSelectedLocations)

    // Update form data with selected location
    const finalLocation = getFinalSelectedLocation(result.updatedSelectedLocations, locations)
    if (finalLocation) {
      handleInputChange("WarehouseCode", finalLocation.Code)
      handleInputChange("LocationCode", finalLocation.Code)
    }
  }

  // Reset location selection
  const resetLocationSelection = () => {
    const result = resetLocationHierarchy(locations, locationTypes)
    setLocationLevels(result.resetLevels)
    setSelectedLocations(result.resetSelectedLocations)
    handleInputChange("WarehouseCode", "")
    handleInputChange("LocationCode", "")
  }

  // Handle organization hierarchy selection
  const handleOrganizationSelect = (levelIndex: number, value: string) => {
    if (!value) return

    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      orgLevels,
      selectedOrgs,
      organizations,
      orgTypes
    )

    setOrgLevels(updatedLevels)
    setSelectedOrgs(updatedSelectedOrgs)

    // Get the final selected organization UID
    const finalOrgUID = getFinalSelectedOrganization(updatedSelectedOrgs)
    if (finalOrgUID) {
      handleInputChange("OrgUID", finalOrgUID)
      console.log("CreateVehicle: Selected org UID:", finalOrgUID)
    }
  }

  // Reset organization selection
  const resetOrganizationSelection = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(
      organizations,
      orgTypes
    )
    setOrgLevels(resetLevels)
    setSelectedOrgs(resetSelectedOrgs)
    handleInputChange("OrgUID", "")
  }

  const validateForm = (): boolean => {
    if (!formData.VehicleNo?.trim()) {
      toast({
        title: "Validation Error",
        description: "Vehicle number is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.RegistrationNo?.trim()) {
      toast({
        title: "Validation Error",
        description: "Registration number is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.Model?.trim()) {
      toast({
        title: "Validation Error",
        description: "Model is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.Type?.trim()) {
      toast({
        title: "Validation Error",
        description: "Type is required",
        variant: "destructive"
      })
      return false
    }

    return true
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!validateForm()) return

    // Validate organization selection
    if (!formData.OrgUID) {
      toast({
        title: "Validation Error",
        description: "Please select an organization",
        variant: "destructive"
      })
      return
    }

    setSaving(true)
    try {
      const currentTime = new Date().toISOString()

      const submitData = {
        ...formData,
        UID: formData.VehicleNo,
        CreatedBy: "ADMIN",
        CreatedTime: currentTime,
        ModifiedBy: "ADMIN",
        ModifiedTime: currentTime,
        TruckSIDate: formData.TruckSIDate ? new Date(formData.TruckSIDate).toISOString() : new Date().toISOString(),
        RoadTaxExpiryDate: formData.RoadTaxExpiryDate ? new Date(formData.RoadTaxExpiryDate).toISOString() : new Date().toISOString(),
        InspectionDate: formData.InspectionDate ? new Date(formData.InspectionDate).toISOString() : new Date().toISOString()
      }

      console.log("CreateVehicle: Submitting data:", submitData)

      await vehicleService.createVehicle(submitData)
      toast({
        title: "Success",
        description: "Vehicle created successfully"
      })

      router.push("/administration/distributor-management/MaintainVan")
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to create vehicle",
        variant: "destructive"
      })
    } finally {
      setSaving(false)
    }
  }

  const handleInputChange = (field: keyof Vehicle, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }))
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => router.push("/administration/distributor-management/MaintainVan")}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Vehicle</h1>
          <p className="text-muted-foreground">
            Add a new vehicle to the fleet
          </p>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle>Vehicle Details</CardTitle>
                <CardDescription>
                  Enter the vehicle information below
                </CardDescription>
              </div>
              <div className="flex items-center space-x-3">
                <Label className="text-sm font-medium text-gray-700">
                  Status:
                </Label>
                <div className="flex items-center space-x-3">
                  <Switch
                    checked={formData.IsActive !== false}
                    onCheckedChange={(checked) => handleInputChange("IsActive", checked)}
                  />
                  <span
                    className={`text-sm font-medium px-2 py-1 rounded-md ${
                      formData.IsActive !== false
                        ? "text-green-700 bg-green-100"
                        : "text-red-700 bg-red-100"
                    }`}
                  >
                    {formData.IsActive !== false ? "Active" : "Inactive"}
                  </span>
                </div>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-6 md:grid-cols-2">
              {/* Vehicle Number */}
              <div className="space-y-2">
                <Label htmlFor="vehicleNo" className="text-sm font-medium">
                  Vehicle Number <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="vehicleNo"
                  value={formData.VehicleNo || ""}
                  onChange={(e) => handleInputChange("VehicleNo", e.target.value.toUpperCase())}
                  placeholder="e.g., VEH001"
                  className="font-mono"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Unique vehicle identifier
                </p>
              </div>

              {/* Registration Number */}
              <div className="space-y-2">
                <Label htmlFor="registrationNo" className="text-sm font-medium">
                  Registration Number <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="registrationNo"
                  value={formData.RegistrationNo || ""}
                  onChange={(e) => handleInputChange("RegistrationNo", e.target.value.toUpperCase())}
                  placeholder="e.g., ABC-1234"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Vehicle registration plate number
                </p>
              </div>

              {/* Model */}
              <div className="space-y-2">
                <Label htmlFor="model" className="text-sm font-medium">
                  Model <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.Model || ""}
                  onValueChange={(value) => handleInputChange("Model", value)}
                >
                  <SelectTrigger id="model">
                    <SelectValue placeholder="Select vehicle model" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Toyota Hiace">Toyota Hiace</SelectItem>
                    <SelectItem value="Isuzu NQR">Isuzu NQR</SelectItem>
                    <SelectItem value="Mitsubishi Canter">Mitsubishi Canter</SelectItem>
                    <SelectItem value="Nissan Urvan">Nissan Urvan</SelectItem>
                    <SelectItem value="Ford Transit">Ford Transit</SelectItem>
                    <SelectItem value="Mercedes-Benz Sprinter">Mercedes-Benz Sprinter</SelectItem>
                    <SelectItem value="Hyundai Porter">Hyundai Porter</SelectItem>
                    <SelectItem value="Tata 407">Tata 407</SelectItem>
                    <SelectItem value="Mahindra Bolero">Mahindra Bolero</SelectItem>
                    <SelectItem value="Other">Other</SelectItem>
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Vehicle model name
                </p>
              </div>

              {/* Type */}
              <div className="space-y-2">
                <Label htmlFor="type" className="text-sm font-medium">
                  Type <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.Type || ""}
                  onValueChange={(value) => handleInputChange("Type", value)}
                >
                  <SelectTrigger id="type">
                    <SelectValue placeholder="Select vehicle type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Truck">Truck</SelectItem>
                    <SelectItem value="Van">Van</SelectItem>
                    <SelectItem value="Lorry">Lorry</SelectItem>
                    <SelectItem value="Pickup">Pickup</SelectItem>
                    <SelectItem value="Car">Car</SelectItem>
                    <SelectItem value="Prime Mover">Prime Mover</SelectItem>
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Type of vehicle
                </p>
              </div>

              {/* Truck SI Date */}
              <div className="space-y-2">
                <Label htmlFor="truckSIDate" className="text-sm font-medium">
                  Truck SI Date
                </Label>
                <Input
                  id="truckSIDate"
                  type="date"
                  value={formData.TruckSIDate?.split('T')[0] || ""}
                  onChange={(e) => handleInputChange("TruckSIDate", e.target.value)}
                />
                <p className="text-xs text-muted-foreground">
                  Truck special inspection date
                </p>
              </div>

              {/* Road Tax Expiry Date */}
              <div className="space-y-2">
                <Label htmlFor="roadTaxExpiryDate" className="text-sm font-medium">
                  Road Tax Expiry Date
                </Label>
                <Input
                  id="roadTaxExpiryDate"
                  type="date"
                  value={formData.RoadTaxExpiryDate?.split('T')[0] || ""}
                  onChange={(e) => handleInputChange("RoadTaxExpiryDate", e.target.value)}
                />
                <p className="text-xs text-muted-foreground">
                  Road tax expiration date
                </p>
              </div>

              {/* Inspection Date */}
              <div className="space-y-2">
                <Label htmlFor="inspectionDate" className="text-sm font-medium">
                  Inspection Date
                </Label>
                <Input
                  id="inspectionDate"
                  type="date"
                  value={formData.InspectionDate?.split('T')[0] || ""}
                  onChange={(e) => handleInputChange("InspectionDate", e.target.value)}
                />
                <p className="text-xs text-muted-foreground">
                  Last inspection date
                </p>
              </div>

              {/* Weight Limit */}
              <div className="space-y-2">
                <Label htmlFor="weightLimit" className="text-sm font-medium">
                  Weight Limit (kg)
                </Label>
                <Input
                  id="weightLimit"
                  type="number"
                  step="0.01"
                  value={formData.WeightLimit || ""}
                  onChange={(e) => handleInputChange("WeightLimit", e.target.value ? parseFloat(e.target.value) : null)}
                  placeholder="e.g., 5000.00"
                />
                <p className="text-xs text-muted-foreground">
                  Maximum weight capacity in kilograms
                </p>
              </div>

              {/* Capacity */}
              <div className="space-y-2">
                <Label htmlFor="capacity" className="text-sm font-medium">
                  Capacity (m³)
                </Label>
                <Input
                  id="capacity"
                  type="number"
                  step="0.01"
                  value={formData.Capacity || ""}
                  onChange={(e) => handleInputChange("Capacity", e.target.value ? parseFloat(e.target.value) : null)}
                  placeholder="e.g., 25.50"
                />
                <p className="text-xs text-muted-foreground">
                  Volume capacity in cubic meters
                </p>
              </div>

              {/* Loading Capacity */}
              <div className="space-y-2">
                <Label htmlFor="loadingCapacity" className="text-sm font-medium">
                  Loading Capacity (kg)
                </Label>
                <Input
                  id="loadingCapacity"
                  type="number"
                  step="0.01"
                  value={formData.LoadingCapacity || ""}
                  onChange={(e) => handleInputChange("LoadingCapacity", e.target.value ? parseFloat(e.target.value) : null)}
                  placeholder="e.g., 3000.00"
                />
                <p className="text-xs text-muted-foreground">
                  Loading capacity in kilograms
                </p>
              </div>

              {/* Organization Hierarchy - Dynamic Cascading Dropdowns */}
              {orgLevels.length > 0 ? (
                <>
                  {orgLevels.map((level, index) => (
                    <div key={`${level.orgTypeUID}_${index}`} className="space-y-2">
                      <Label className="text-sm font-medium">
                        {level.orgTypeName || `Level ${index + 1}`}
                        {index === 0 && <span className="text-red-500"> *</span>}
                      </Label>
                      <Select
                        value={level.selectedOrgUID || ""}
                        onValueChange={(value) => handleOrganizationSelect(index, value)}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder={`Select ${level.orgTypeName || 'organization'}`} />
                        </SelectTrigger>
                        <SelectContent>
                          {level.organizations.map((org) => (
                            <SelectItem key={org.UID} value={org.UID}>
                              {org.Name} {org.Code && `(${org.Code})`}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  ))}
                  {Object.keys(selectedOrgs).length > 0 && (
                    <div className="col-span-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={resetOrganizationSelection}
                      >
                        <X className="h-3 w-3 mr-1" />
                        Reset Organization
                      </Button>
                    </div>
                  )}
                </>
              ) : (
                <div className="col-span-2 text-center py-4 text-muted-foreground">
                  <p className="text-sm">Loading organization hierarchy...</p>
                </div>
              )}

              {/* Location Hierarchy - Dynamic Cascading Dropdowns */}
              {locations.length === 0 && locationLevels.length === 0 ? (
                <div className="col-span-2 text-center py-6 border rounded-lg bg-gray-50">
                  <Globe className="h-6 w-6 mx-auto mb-2 text-gray-400" />
                  <p className="text-sm text-gray-600 font-medium">No locations available</p>
                  <p className="text-xs text-gray-500 mt-1">Please create locations in the system first</p>
                </div>
              ) : locationLevels.length > 0 ? (
                <>
                  {locationLevels.map((level, index) => (
                    <div
                      key={`${level.locationTypeUID}_${index}`}
                      className={index === 0 ? "col-span-2" : ""}
                    >
                      <Label className="text-sm font-medium text-gray-700 mb-1.5">
                        {level.dynamicLabel || level.locationTypeName}
                        {index === 0 && (
                          <span className="text-blue-500 ml-1">*</span>
                        )}
                      </Label>
                      <Select
                        value={level.selectedLocationUID || ""}
                        onValueChange={(value) => handleLocationSelect(index, value)}
                      >
                        <SelectTrigger className="h-9 border-blue-200 focus:border-blue-400">
                          <SelectValue
                            placeholder={`Select ${(level.dynamicLabel || level.locationTypeName).toLowerCase()}`}
                          />
                        </SelectTrigger>
                        <SelectContent>
                          {level.locations.map((location) => (
                            <SelectItem
                              key={location.UID}
                              value={location.UID}
                            >
                              <div className="flex items-center justify-between w-full">
                                <span className="font-medium">
                                  {getLocationDisplayName(location)}
                                </span>
                                {location.Code && (
                                  <span className="text-xs text-blue-500 ml-2">
                                    [{location.Code}]
                                  </span>
                                )}
                              </div>
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  ))}

                  {/* Location selection summary and reset */}
                  <div className="col-span-2 mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
                    <div className="flex items-center justify-between">
                      <div className="flex-1">
                        {locationLevels.length > 1 && !selectedLocations.length && (
                          <div className="text-xs text-muted-foreground italic">
                            Multiple location types available. Select from any to continue.
                          </div>
                        )}
                      </div>
                      {selectedLocations.length > 0 && (
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={resetLocationSelection}
                          className="ml-4 text-xs border-blue-300 text-blue-600 hover:bg-blue-100"
                        >
                          <X className="h-3 w-3 mr-1" />
                          Reset Location
                        </Button>
                      )}
                    </div>
                  </div>
                </>
              ) : null}

              {/* Territory */}
              <div className="space-y-2">
                <Label htmlFor="territoryUID" className="text-sm font-medium">
                  Territory
                </Label>
                <Select
                  value={formData.TerritoryUID || ""}
                  onValueChange={(value) => handleInputChange("TerritoryUID", value)}
                  disabled={territories.length === 0}
                >
                  <SelectTrigger id="territoryUID">
                    <SelectValue placeholder={territories.length === 0 ? "No territories available" : "Select territory"} />
                  </SelectTrigger>
                  <SelectContent>
                    {territories.length > 0 ? (
                      territories.map((territory) => (
                        <SelectItem key={territory.UID} value={territory.UID}>
                          {territory.TerritoryName} ({territory.TerritoryCode})
                        </SelectItem>
                      ))
                    ) : (
                      <SelectItem value="none" disabled>
                        No territories available
                      </SelectItem>
                    )}
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  {territories.length === 0 ? "Please create territories in the system first" : "Territory assignment"}
                </p>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex justify-end gap-4 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push("/administration/distributor-management/MaintainVan")}
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
                    Create
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
