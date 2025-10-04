"use client"

import { useState, useEffect } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { ArrowLeft, Save, Warehouse, Globe, Building, Home, Eye } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Switch } from "@/components/ui/switch"
import { Badge } from "@/components/ui/badge"
import { Checkbox } from "@/components/ui/checkbox"
import { useToast } from "@/components/ui/use-toast"
import { apiService } from "@/services/api"
import { organizationService, Organization, OrgType, TaxGroup } from "@/services/organizationService"
import { locationService, Location } from "@/services/locationService"
import { Skeleton } from "@/components/ui/skeleton"

interface CreateWarehouseForm {
  uid: string
  warehouseCode: string
  warehouseName: string
  orgTypeUID: string
  parentUID: string
  countryUID: string
  regionUID: string
  cityUID: string
  taxGroupUID: string
  addressUID: string
  addressName: string
  addressLine1: string
  addressLine2: string
  addressLine3: string
  landmark: string
  area: string
  zipCode: string
  city: string
  regionCode: string
  linkedItemUID: string
  status: string
  isActive: boolean
  showInUI: boolean
  showInTemplate: boolean
}

export function CreateWarehouse() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { toast } = useToast()

  const warehouseId = searchParams.get("id")
  const isEditMode = !!warehouseId

  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [warehouseTypes, setWarehouseTypes] = useState<OrgType[]>([])
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [taxGroups, setTaxGroups] = useState<TaxGroup[]>([])
  const [allLocations, setAllLocations] = useState<Location[]>([])
  const [selectedCountries, setSelectedCountries] = useState<string[]>([])
  const [selectedRegions, setSelectedRegions] = useState<string[]>([])
  const [selectedCities, setSelectedCities] = useState<string[]>([])

  const [formData, setFormData] = useState<CreateWarehouseForm>({
    uid: "",
    warehouseCode: "",
    warehouseName: "",
    orgTypeUID: "",
    parentUID: "",
    countryUID: "",
    regionUID: "",
    cityUID: "",
    taxGroupUID: "",
    addressUID: "",
    addressName: "",
    addressLine1: "",
    addressLine2: "",
    addressLine3: "",
    landmark: "",
    area: "",
    zipCode: "",
    city: "",
    regionCode: "",
    linkedItemUID: "",
    status: "Active",
    isActive: true,
    showInUI: true,
    showInTemplate: true
  })

  useEffect(() => {
    fetchInitialData()
    if (isEditMode && warehouseId) {
      fetchWarehouseDetails(warehouseId)
    }
  }, [isEditMode, warehouseId])

  useEffect(() => {
    // Auto-set UID to be the same as Code (uppercase)
    if (formData.warehouseCode && !isEditMode) {
      handleInputChange("uid", formData.warehouseCode.toUpperCase())
    }
  }, [formData.warehouseCode, isEditMode])

  const fetchInitialData = async () => {
    setLoading(true)
    try {
      const [typesResult, orgsResult, taxResult, locationsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 1000),
        organizationService.getTaxGroups(),
        locationService.getLocations(1, 1000)
      ])

      // Filter for warehouse types - only show FRWH and WOWH
      const whTypes = typesResult.filter(type =>
        type.UID === 'FRWH' ||
        type.UID === 'WOWH'
      )

      setWarehouseTypes(whTypes)
      setOrganizations(orgsResult.data)
      setTaxGroups(taxResult)
      setAllLocations(locationsResult.data)
    } catch (error) {
      console.error('Failed to fetch initial data:', error)
      toast({
        title: "Error",
        description: "Failed to fetch initial data",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const fetchWarehouseDetails = async (id: string) => {
    setLoading(true)
    try {
      const response: any = await apiService.get(`/Org/ViewFranchiseeWarehouseByUID?UID=${id}`)
      const warehouse = response.Data

      setFormData({
        uid: warehouse.UID || "",
        warehouseCode: warehouse.WarehouseCode || "",
        warehouseName: warehouse.WarehouseName || "",
        orgTypeUID: warehouse.OrgTypeUID || "",
        parentUID: warehouse.ParentUID || "",
        countryUID: warehouse.CountryUID || "",
        regionUID: warehouse.RegionUID || "",
        cityUID: warehouse.CityUID || "",
        taxGroupUID: warehouse.TaxGroupUID || "",
        addressUID: warehouse.AddressUID || "",
        addressName: warehouse.AddressName || "",
        addressLine1: warehouse.AddressLine1 || "",
        addressLine2: warehouse.AddressLine2 || "",
        addressLine3: warehouse.AddressLine3 || "",
        landmark: warehouse.Landmark || "",
        area: warehouse.Area || "",
        zipCode: warehouse.ZipCode || "",
        city: warehouse.City || "",
        regionCode: warehouse.RegionCode || "",
        linkedItemUID: warehouse.LinkedItemUID || "",
        status: warehouse.Status || "Active",
        isActive: warehouse.IsActive !== undefined ? warehouse.IsActive : true
      })

    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch warehouse details",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  // Location hierarchy functions
  const getCountries = (): Location[] => {
    return allLocations.filter((loc) =>
      loc.LocationTypeName?.toLowerCase().includes('country') || loc.ItemLevel === 1
    )
  }

  const getRegions = (): Location[] => {
    if (selectedCountries.length === 0) return []
    return allLocations.filter(
      (loc) =>
        (loc.LocationTypeName?.toLowerCase().includes('region') ||
         loc.LocationTypeName?.toLowerCase().includes('state') ||
         loc.ItemLevel === 2) &&
        selectedCountries.includes(loc.ParentUID || "")
    )
  }

  const getCities = (): Location[] => {
    if (selectedRegions.length === 0) return []
    return allLocations.filter(
      (loc) =>
        (loc.LocationTypeName?.toLowerCase().includes('city') || loc.ItemLevel === 3) &&
        selectedRegions.includes(loc.ParentUID || "")
    )
  }

  // Location selection handlers
  const handleCountryChange = (countryUID: string, checked: boolean) => {
    setSelectedCountries(prev =>
      checked ? [...prev, countryUID] : prev.filter(c => c !== countryUID)
    )
    if (!checked) {
      // Remove related regions and cities when country is deselected
      const relatedRegions = allLocations
        .filter(loc => loc.ParentUID === countryUID)
        .map(loc => loc.UID)
      setSelectedRegions(prev => prev.filter(r => !relatedRegions.includes(r)))
      setSelectedCities(prev => {
        const citiesToRemove = allLocations
          .filter(loc => relatedRegions.includes(loc.ParentUID || ""))
          .map(loc => loc.UID)
        return prev.filter(c => !citiesToRemove.includes(c))
      })
    }
  }

  const handleRegionChange = (regionUID: string, checked: boolean) => {
    setSelectedRegions(prev =>
      checked ? [...prev, regionUID] : prev.filter(r => r !== regionUID)
    )
    if (!checked) {
      // Remove related cities when region is deselected
      const relatedCities = allLocations
        .filter(loc => loc.ParentUID === regionUID)
        .map(loc => loc.UID)
      setSelectedCities(prev => prev.filter(c => !relatedCities.includes(c)))
    }
  }

  const handleCityChange = (cityUID: string, checked: boolean) => {
    setSelectedCities(prev =>
      checked ? [...prev, cityUID] : prev.filter(c => c !== cityUID)
    )
  }

  const validateForm = (): boolean => {
    if (!formData.uid?.trim()) {
      toast({
        title: "Validation Error",
        description: "Warehouse UID is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.warehouseCode?.trim()) {
      toast({
        title: "Validation Error",
        description: "Warehouse code is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.warehouseName?.trim()) {
      toast({
        title: "Validation Error",
        description: "Warehouse name is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.orgTypeUID) {
      toast({
        title: "Validation Error",
        description: "Warehouse type is required",
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
      // Get current user from localStorage
      const currentUser = localStorage.getItem('username') || 'ADMIN'

      // Prepare payload matching backend EditWareHouseItemView
      const payload = {
        UID: formData.uid,
        WarehouseCode: formData.warehouseCode,
        WarehouseName: formData.warehouseName,
        OrgTypeUID: formData.orgTypeUID,
        ParentUID: formData.parentUID || "",
        CountryUID: formData.countryUID || "",
        RegionUID: formData.regionUID || "",
        CityUID: formData.cityUID || "",
        TaxGroupUID: formData.taxGroupUID || "",
        AddressUID: formData.addressUID || crypto.randomUUID(),
        AddressName: formData.addressName || "",
        AddressLine1: formData.addressLine1 || "",
        AddressLine2: formData.addressLine2 || "",
        AddressLine3: formData.addressLine3 || "",
        Landmark: formData.landmark || "",
        Area: formData.area || "",
        ZipCode: formData.zipCode || "",
        City: formData.city || "",
        RegionCode: formData.regionCode || "",
        LinkedItemUID: formData.linkedItemUID || formData.uid,
        Status: formData.status,
        IsActive: formData.isActive,
        ShowInUI: formData.showInUI,
        ShowInTemplate: formData.showInTemplate,
        CreatedBy: currentUser,
        ModifiedBy: currentUser
      }

      if (isEditMode && warehouseId) {
        await apiService.put('/Org/UpdateViewFranchiseeWarehouse', payload)
        toast({
          title: "Success",
          description: "Warehouse updated successfully"
        })
      } else {
        await apiService.post('/Org/CreateViewFranchiseeWarehouse', payload)
        toast({
          title: "Success",
          description: "Warehouse created successfully"
        })
      }

      router.push("/administration/warehouse-management/warehouses")
    } catch (error) {
      toast({
        title: "Error",
        description: isEditMode ? "Failed to update warehouse" : "Failed to create warehouse",
        variant: "destructive"
      })
    } finally {
      setSaving(false)
    }
  }

  const handleInputChange = (field: keyof CreateWarehouseForm, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }))
  }

  if (loading) {
    return (
      <div className="space-y-6">
        {/* Page Header Skeleton */}
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10 rounded" />
          <div>
            <Skeleton className="h-8 w-56 mb-2" />
            <Skeleton className="h-4 w-80" />
          </div>
        </div>

        {/* Form Skeleton */}
        <div className="max-w-4xl">
          <div className="bg-white rounded-lg border shadow-sm">
            <div className="p-6 border-b">
              <Skeleton className="h-6 w-48 mb-2" />
              <Skeleton className="h-4 w-64" />
            </div>

            <div className="p-6 space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                {/* Form fields skeleton */}
                {[...Array(12)].map((_, i) => (
                  <div key={i} className="space-y-2">
                    <Skeleton className="h-4 w-28" />
                    <Skeleton className="h-10 w-full rounded" />
                    {i % 3 === 0 && (
                      <div className="flex items-center space-x-2 mt-2">
                        <Skeleton className="h-4 w-4 rounded" />
                        <Skeleton className="h-3 w-32" />
                      </div>
                    )}
                    <Skeleton className="h-3 w-40" />
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
          onClick={() => router.push("/administration/warehouse-management/warehouses")}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {isEditMode ? "Edit Warehouse" : "Create Warehouse"}
          </h1>
          <p className="text-muted-foreground">
            {isEditMode ? "Update warehouse details" : "Add a new warehouse to the system"}
          </p>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Warehouse Details</CardTitle>
            <CardDescription>
              Enter the warehouse information below
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Basic Information Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold border-b pb-2">Basic Information</h3>
              <div className="grid gap-6 md:grid-cols-2">
                {/* Warehouse Code */}
                <div className="space-y-2">
                  <Label htmlFor="code" className="text-sm font-medium">
                    Warehouse Code <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="code"
                    value={formData.warehouseCode || ""}
                    onChange={(e) => handleInputChange("warehouseCode", e.target.value.toUpperCase())}
                    placeholder="e.g., WH001, FWH001"
                    className="font-mono"
                    required
                  />
                  <p className="text-xs text-muted-foreground">
                    Short code for this warehouse
                  </p>
                </div>

                {/* Warehouse Name */}
                <div className="space-y-2">
                  <Label htmlFor="name" className="text-sm font-medium">
                    Warehouse Name <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="name"
                    value={formData.warehouseName || ""}
                    onChange={(e) => handleInputChange("warehouseName", e.target.value)}
                    placeholder="e.g., Main Distribution Center"
                    required
                  />
                  <p className="text-xs text-muted-foreground">
                    Display name for this warehouse
                  </p>
                </div>

                {/* Warehouse Type */}
                <div className="space-y-2">
                  <Label htmlFor="orgType" className="text-sm font-medium">
                    Warehouse Type <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={formData.orgTypeUID || ""}
                    onValueChange={(value) => handleInputChange("orgTypeUID", value)}
                  >
                    <SelectTrigger id="orgType">
                      <SelectValue placeholder="Select warehouse type" />
                    </SelectTrigger>
                    <SelectContent>
                      {warehouseTypes.map((type) => (
                        <SelectItem key={type.UID} value={type.UID}>
                          <div className="flex items-center justify-between w-full">
                            <span>{type.Name}</span>
                            {type.IsWh && (
                              <Badge variant="outline" className="ml-2 text-xs">Warehouse</Badge>
                            )}
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Select the type/category of this warehouse
                  </p>
                </div>

                {/* Parent Organization */}
                <div className="space-y-2">
                  <Label htmlFor="parentOrg">
                    Parent Organization
                  </Label>
                  <Select
                    value={formData.parentUID || "none"}
                    onValueChange={(value) => handleInputChange("parentUID", value === "none" ? "" : value)}
                  >
                    <SelectTrigger id="parentOrg">
                      <SelectValue placeholder="Select parent organization" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">None (Top Level)</SelectItem>
                      {organizations.filter(org => org.IsActive).map((org) => (
                        <SelectItem key={org.UID} value={org.UID}>
                          {org.Name} ({org.Code})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Select the parent organization in the hierarchy
                  </p>
                </div>

                {/* Tax Group */}
                <div className="space-y-2">
                  <Label htmlFor="taxGroup">Tax Group</Label>
                  <Select
                    value={formData.taxGroupUID || "none"}
                    onValueChange={(value) => handleInputChange("taxGroupUID", value === "none" ? "" : value)}
                  >
                    <SelectTrigger id="taxGroup">
                      <SelectValue placeholder="Select tax group" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">None</SelectItem>
                      {taxGroups.map((group) => (
                        <SelectItem key={group.UID} value={group.UID}>
                          <div className="flex items-center justify-between w-full">
                            <span>{group.Name}</span>
                            {group.TaxPercentage && (
                              <Badge variant="outline" className="ml-2 text-xs">
                                {group.TaxPercentage}%
                              </Badge>
                            )}
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Status Dropdown */}
                <div className="space-y-2">
                  <Label htmlFor="status" className="text-sm font-medium">
                    Status
                  </Label>
                  <Select
                    value={formData.status || "Active"}
                    onValueChange={(value) => handleInputChange("status", value)}
                  >
                    <SelectTrigger id="status">
                      <SelectValue placeholder="Select status" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Active">Active</SelectItem>
                      <SelectItem value="Inactive">Inactive</SelectItem>
                      <SelectItem value="Pending">Pending</SelectItem>
                      <SelectItem value="Suspended">Suspended</SelectItem>
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Current operational status of the warehouse
                  </p>
                </div>
              </div>
            </div>

            {/* Address Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold border-b pb-2">Address Details</h3>
              <div className="grid gap-6 md:grid-cols-2">
                {/* Address Name */}
                <div className="space-y-2">
                  <Label htmlFor="addressName">Address Name</Label>
                  <Input
                    id="addressName"
                    value={formData.addressName || ""}
                    onChange={(e) => handleInputChange("addressName", e.target.value)}
                    placeholder="e.g., Main Address"
                  />
                </div>

                {/* Landmark */}
                <div className="space-y-2">
                  <Label htmlFor="landmark">Landmark</Label>
                  <Input
                    id="landmark"
                    value={formData.landmark || ""}
                    onChange={(e) => handleInputChange("landmark", e.target.value)}
                    placeholder="Nearby landmark"
                  />
                </div>

                {/* Address Line 1 */}
                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="addressLine1">Address Line 1</Label>
                  <Input
                    id="addressLine1"
                    value={formData.addressLine1 || ""}
                    onChange={(e) => handleInputChange("addressLine1", e.target.value)}
                    placeholder="Street address"
                  />
                </div>
              </div>
            </div>

            {/* Geographic Location Assignment */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center text-blue-600">
                  <Globe className="h-5 w-5 mr-2" />
                  Geographic Location Assignment
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Countries Section */}
                {getCountries().length > 0 && (
                  <div>
                    <div className="bg-blue-600 text-white p-3 rounded-md flex items-center justify-between mb-4">
                      <span className="font-semibold flex items-center">
                        <Globe className="h-4 w-4 mr-2" />
                        Select Country
                      </span>
                      {selectedCountries.length > 0 && (
                        <Badge variant="secondary" className="bg-white text-blue-600">
                          {selectedCountries.length} selected
                        </Badge>
                      )}
                    </div>

                    <div className="space-y-2">
                      {getCountries().map((country) => (
                        <div
                          key={country.UID}
                          className={`p-3 border rounded-md flex items-center justify-between ${
                            selectedCountries.includes(country.UID)
                              ? "bg-blue-50 border-blue-200"
                              : ""
                          }`}
                        >
                          <div className="flex items-center">
                            <Checkbox
                              checked={selectedCountries.includes(country.UID)}
                              onCheckedChange={(checked) =>
                                handleCountryChange(country.UID, !!checked)
                              }
                            />
                            <span className="ml-2 font-medium">{country.Name}</span>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* Regions Section */}
                {selectedCountries.length > 0 && (
                  <div>
                    <div className="bg-green-600 text-white p-3 rounded-md flex items-center justify-between mb-4">
                      <span className="font-semibold flex items-center">
                        <Building className="h-4 w-4 mr-2" />
                        Select Region
                      </span>
                      {selectedRegions.length > 0 && (
                        <Badge variant="secondary" className="bg-white text-green-600">
                          {selectedRegions.length} selected
                        </Badge>
                      )}
                    </div>

                    <div className="space-y-2">
                      {getRegions().length > 0 ? (
                        getRegions().map((region) => (
                          <div
                            key={region.UID}
                            className={`p-3 border rounded-md flex items-center justify-between ${
                              selectedRegions.includes(region.UID)
                                ? "bg-green-50 border-green-200"
                                : ""
                            }`}
                          >
                            <div className="flex items-center">
                              <Checkbox
                                checked={selectedRegions.includes(region.UID)}
                                onCheckedChange={(checked) =>
                                  handleRegionChange(region.UID, !!checked)
                                }
                              />
                              <span className="ml-2">
                                [{region.Code}] {region.Name}
                              </span>
                            </div>
                          </div>
                        ))
                      ) : (
                        <div className="text-center py-8 text-gray-500">
                          No regions available for selected countries
                        </div>
                      )}
                    </div>
                  </div>
                )}

                {/* Cities Section */}
                {selectedRegions.length > 0 && (
                  <div>
                    <div className="bg-orange-600 text-white p-3 rounded-md flex items-center justify-between mb-4">
                      <span className="font-semibold flex items-center">
                        <Home className="h-4 w-4 mr-2" />
                        Select City
                      </span>
                      {selectedCities.length > 0 && (
                        <Badge variant="secondary" className="bg-white text-orange-600">
                          {selectedCities.length} selected
                        </Badge>
                      )}
                    </div>

                    <div className="space-y-2">
                      {getCities().length > 0 ? (
                        getCities().map((city) => (
                          <div
                            key={city.UID}
                            className={`p-3 border rounded-md flex items-center justify-between ${
                              selectedCities.includes(city.UID)
                                ? "bg-orange-50 border-orange-200"
                                : ""
                            }`}
                          >
                            <div className="flex items-center">
                              <Checkbox
                                checked={selectedCities.includes(city.UID)}
                                onCheckedChange={(checked) =>
                                  handleCityChange(city.UID, !!checked)
                                }
                              />
                              <span className="ml-2">
                                [{city.Code}] {city.Name}
                              </span>
                            </div>
                          </div>
                        ))
                      ) : (
                        <div className="text-center py-8 text-gray-500">
                          No cities available for selected regions
                        </div>
                      )}
                    </div>
                  </div>
                )}

                {/* Summary */}
                {(selectedCountries.length > 0 ||
                  selectedRegions.length > 0 ||
                  selectedCities.length > 0) && (
                  <div className="bg-gray-50 p-4 rounded-md">
                    <div className="text-sm text-gray-600">
                      <strong>Selected: </strong>
                      {selectedCountries.length} Countries, {selectedRegions.length}{" "}
                      Regions, {selectedCities.length} Cities
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Settings Section */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold border-b pb-2">Settings</h3>
              <div className="grid gap-6 md:grid-cols-2">
                {/* Active Toggle */}
                <div className="space-y-2">
                  <Label className="text-sm font-medium">Active Status</Label>
                  <div className="flex items-center space-x-3 p-4 border rounded-lg bg-gray-50">
                    <Switch
                      id="isActive"
                      checked={formData.isActive}
                      onCheckedChange={(checked) => handleInputChange("isActive", checked)}
                    />
                    <div className="flex-1">
                      <Label htmlFor="isActive" className="font-medium text-sm cursor-pointer">
                        Is Active
                      </Label>
                      <p className="text-xs text-muted-foreground mt-1">
                        Enable or disable this warehouse
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Selected Type Info */}
            {formData.orgTypeUID && (() => {
              const selectedType = warehouseTypes.find(t => t.UID === formData.orgTypeUID)
              return selectedType && (
                <div className="col-span-2 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <h4 className="text-sm font-medium text-blue-900 mb-2">Warehouse Type Information</h4>
                  <div className="space-y-2">
                    <p className="text-sm text-blue-800">
                      <span className="font-medium">Selected Type:</span> {selectedType.Name}
                    </p>
                    <div className="flex gap-2">
                      {selectedType.IsWh && (
                        <Badge variant="outline" className="text-xs border-blue-300">
                          Warehouse
                        </Badge>
                      )}
                      {selectedType.IsCompanyOrg && (
                        <Badge variant="outline" className="text-xs border-blue-300">
                          Company Organization
                        </Badge>
                      )}
                      {selectedType.IsFranchiseeOrg && (
                        <Badge variant="outline" className="text-xs border-blue-300">
                          Franchisee Organization
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
              )
            })()}

            {/* Action Buttons */}
            <div className="flex justify-end gap-4 pt-4 col-span-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push("/administration/warehouse-management/warehouses")}
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
