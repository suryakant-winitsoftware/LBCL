"use client"

import { useState, useEffect } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { ArrowLeft, Save, Building2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Switch } from "@/components/ui/switch"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import { organizationService, Organization, OrgType, TaxGroup, CreateOrganizationForm } from "@/services/organizationService"
import { locationService, Location } from "@/services/locationService"
import { Skeleton } from "@/components/ui/skeleton"

export function CreateOrganization() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { toast } = useToast()
  
  const organizationId = searchParams.get("id")
  const isEditMode = !!organizationId
  
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([])
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [parentOrganizations, setParentOrganizations] = useState<Organization[]>([])
  const [taxGroups, setTaxGroups] = useState<TaxGroup[]>([])
  const [countries, setCountries] = useState<Location[]>([])
  const [regions, setRegions] = useState<Location[]>([])
  const [cities, setCities] = useState<Location[]>([])
  const [companyOrgTypes, setCompanyOrgTypes] = useState<OrgType[]>([])
  
  
  const [formData, setFormData] = useState<CreateOrganizationForm>({
    uid: "",
    code: "",
    name: "",
    orgTypeUID: "",
    parentUID: "",
    countryUID: "",
    regionUID: "",
    cityUID: "",
    companyUID: null, // Set to null by default as field is commented out
    taxGroupUID: "",
    status: "Active",
    isActive: true,
    hasEarlyAccess: false,
    seqCode: ""
  })

  useEffect(() => {
    fetchInitialData()
    if (isEditMode && organizationId) {
      fetchOrganizationDetails(organizationId)
    }
  }, [isEditMode, organizationId])

  useEffect(() => {
    if (formData.orgTypeUID && organizations.length > 0) {
      fetchParentOrganizations(formData.orgTypeUID)
    }
  }, [formData.orgTypeUID, organizations])

  useEffect(() => {
    if (formData.countryUID) {
      fetchRegions(formData.countryUID)
      // Reset dependent fields
      setFormData(prev => ({ ...prev, regionUID: "", cityUID: "" }))
      setCities([])
    } else {
      setRegions([])
      setCities([])
    }
  }, [formData.countryUID])

  useEffect(() => {
    if (formData.regionUID) {
      fetchCities(formData.regionUID)
      // Reset city selection
      setFormData(prev => ({ ...prev, cityUID: "" }))
    } else {
      setCities([])
    }
  }, [formData.regionUID])

  useEffect(() => {
    // Auto-set UID to be the same as Code (uppercase)
    if (formData.code && !isEditMode) {
      handleInputChange("uid", formData.code.toUpperCase())
    }
  }, [formData.code, isEditMode])

  const fetchInitialData = async () => {
    setLoading(true)
    try {
      const [typesResult, orgsResult, taxResult, locationsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 1000),
        organizationService.getTaxGroups(),
        locationService.getLocations(1, 1000)
        // organizationService.getCompanyOrgTypes() // Commented out as field is not used
      ])
      
      setOrgTypes(typesResult)
      setOrganizations(orgsResult.data)
      setTaxGroups(taxResult)
      // setCompanyOrgTypes(companyTypesResult) // Commented out as field is not used
      
      // Filter countries from locations
      const countryLocations = locationsResult.data.filter(loc => 
        loc.LocationTypeName?.toLowerCase().includes('country') || loc.ItemLevel === 1
      )
      setCountries(countryLocations)
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

  const fetchOrganizationDetails = async (id: string) => {
    setLoading(true)
    try {
      const organization = await organizationService.getOrganizationByUID(id)
      setFormData({
        uid: organization.UID,
        code: organization.Code,
        name: organization.Name,
        orgTypeUID: organization.OrgTypeUID,
        parentUID: organization.ParentUID || "",
        countryUID: organization.CountryUID || "",
        regionUID: organization.RegionUID || "",
        cityUID: organization.CityUID || "",
        companyUID: organization.CompanyUID || "",
        taxGroupUID: organization.TaxGroupUID || "",
        status: organization.Status || "Active",
        isActive: organization.IsActive,
        hasEarlyAccess: organization.HasEarlyAccess || false,
        seqCode: organization.SeqCode || ""
      })
      
      // Load regions and cities if country is selected
      if (organization.CountryUID) {
        await fetchRegions(organization.CountryUID)
        if (organization.RegionUID) {
          await fetchCities(organization.RegionUID)
        }
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch organization details",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const fetchParentOrganizations = async (orgTypeUID: string) => {
    try {
      console.log('=== DEBUG: fetchParentOrganizations called ===')
      console.log('Selected orgTypeUID:', orgTypeUID)
      console.log('Total organizations loaded:', organizations.length)
      console.log('All organizations:', organizations.map(o => ({ UID: o.UID, Name: o.Name, Code: o.Code, OrgTypeUID: o.OrgTypeUID, IsActive: o.IsActive })))
      
      const selectedOrgType = orgTypes.find(t => t.UID === orgTypeUID)
      if (!selectedOrgType) {
        console.log('ERROR: No selected org type found')
        setParentOrganizations([])
        return
      }
      console.log('Selected org type:', selectedOrgType)

      // Check if 'epic01' exists in organizations
      const epic01 = organizations.find(org => 
        org.Code?.toLowerCase().includes('epic01') || 
        org.Name?.toLowerCase().includes('epic01') ||
        org.UID?.toLowerCase().includes('epic01')
      )
      console.log('Found epic01 organization:', epic01)

      try {
        // Build org type hierarchy map
        const typeHierarchy = organizationService.buildOrgTypeHierarchy(orgTypes)
        console.log('Built type hierarchy:', Array.from(typeHierarchy.entries()))
        
        const selectedTypeHierarchy = typeHierarchy.get(orgTypeUID)
        console.log('Selected type hierarchy:', selectedTypeHierarchy)
        
        if (!selectedTypeHierarchy) {
          console.log('FALLBACK: No hierarchy found, showing all active organizations')
          const fallbackParents = organizations.filter(org => {
            if (org.UID === formData.uid) return false
            return org.IsActive
          })
          console.log('Fallback parent organizations:', fallbackParents.length)
          setParentOrganizations(fallbackParents)
          return
        }

        // Get all valid parent types (types at level < selected type level)
        const validParentTypes = Array.from(typeHierarchy.values())
          .filter(type => type.level < selectedTypeHierarchy.level)
          .map(type => type.UID)

        console.log('Valid parent type UIDs:', validParentTypes)

        // Filter organizations that can be parents
        const validParents = organizations.filter(org => {
          // Can't be parent of itself
          if (org.UID === formData.uid) {
            console.log('Skipping self:', org.UID)
            return false
          }
          
          // Must be of a valid parent type
          const isValidType = validParentTypes.includes(org.OrgTypeUID)
          console.log(`Org ${org.Name} (${org.Code}) - OrgTypeUID: ${org.OrgTypeUID}, Valid: ${isValidType}, Active: ${org.IsActive}`)
          return isValidType && org.IsActive
        })

        console.log('Final valid parent organizations:', validParents.map(o => ({ Name: o.Name, Code: o.Code, UID: o.UID })))
        setParentOrganizations(validParents)
      } catch (hierarchyError) {
        console.error('HIERARCHY ERROR - Using fallback:', hierarchyError)
        // Fallback: show all active organizations
        const fallbackParents = organizations.filter(org => {
          if (org.UID === formData.uid) return false
          return org.IsActive
        })
        console.log('Using fallback - showing all active organizations:', fallbackParents.length)
        setParentOrganizations(fallbackParents)
      }
    } catch (error) {
      console.error('Error fetching parent organizations:', error)
      setParentOrganizations([])
    }
  }

  const fetchRegions = async (countryUID: string) => {
    try {
      const regions = await organizationService.getLocationsByParent(countryUID)
      setRegions(regions)
    } catch (error) {
      console.error("Failed to fetch regions:", error)
    }
  }

  const fetchCities = async (regionUID: string) => {
    try {
      const cities = await organizationService.getLocationsByParent(regionUID)
      setCities(cities)
    } catch (error) {
      console.error("Failed to fetch cities:", error)
    }
  }


  const validateForm = async (): Promise<boolean> => {
    if (!formData.uid?.trim()) {
      toast({
        title: "Validation Error",
        description: "Organization UID is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.code?.trim()) {
      toast({
        title: "Validation Error",
        description: "Organization code is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.name?.trim()) {
      toast({
        title: "Validation Error",
        description: "Organization name is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.orgTypeUID) {
      toast({
        title: "Validation Error",
        description: "Organization type is required",
        variant: "destructive"
      })
      return false
    }

    // Check UID uniqueness
    if (!isEditMode || formData.uid !== organizationId) {
      const isUIDUnique = await organizationService.isUIDUnique(formData.uid, organizationId || undefined)
      if (!isUIDUnique) {
        toast({
          title: "Validation Error",
          description: "This UID is already taken. Please choose a different one.",
          variant: "destructive"
        })
        return false
      }
    }

    // Check Code uniqueness
    const isCodeUnique = await organizationService.isCodeUnique(formData.code, organizationId || undefined)
    if (!isCodeUnique) {
      toast({
        title: "Validation Error",
        description: "This code is already taken. Please choose a different one.",
        variant: "destructive"
      })
      return false
    }

    return true
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!await validateForm()) return

    setSaving(true)
    try {
      if (isEditMode && organizationId) {
        await organizationService.updateOrganization(organizationId, formData as any)
        toast({
          title: "Success",
          description: "Organization updated successfully"
        })
      } else {
        await organizationService.createOrganization(formData)
        toast({
          title: "Success",
          description: "Organization created successfully"
        })
      }
      
      router.push("/administration/organization-management/organizations")
    } catch (error) {
      toast({
        title: "Error",
        description: isEditMode ? "Failed to update organization" : "Failed to create organization",
        variant: "destructive"
      })
    } finally {
      setSaving(false)
    }
  }

  const handleInputChange = (field: keyof CreateOrganizationForm, value: any) => {
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
              
              {/* Info section skeleton */}
              <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                <Skeleton className="h-5 w-48 mb-2" />
                <div className="space-y-2">
                  <Skeleton className="h-4 w-64" />
                  <div className="flex gap-2">
                    <Skeleton className="h-5 w-20 rounded-full" />
                    <Skeleton className="h-5 w-24 rounded-full" />
                    <Skeleton className="h-5 w-16 rounded-full" />
                  </div>
                </div>
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
          onClick={() => router.push("/administration/organization-management/organizations")}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {isEditMode ? "Edit Organization" : "Create Organization"}
          </h1>
          <p className="text-muted-foreground">
            {isEditMode ? "Update organization details" : "Add a new organization to the system"}
          </p>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Organization Details</CardTitle>
            <CardDescription>
              Enter the organization information below
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-6 md:grid-cols-2">
              {/* Organization Code */}
              <div className="space-y-2">
                <Label htmlFor="code" className="text-sm font-medium">
                  Organization Code <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="code"
                  value={formData.code || ""}
                  onChange={(e) => handleInputChange("code", e.target.value.toUpperCase())}
                  placeholder="e.g., HQ001, ST001"
                  className="font-mono"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Short code for this organization
                </p>
              </div>

              {/* Organization Name */}
              <div className="space-y-2">
                <Label htmlFor="name" className="text-sm font-medium">
                  Organization Name <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="name"
                  value={formData.name || ""}
                  onChange={(e) => handleInputChange("name", e.target.value)}
                  placeholder="e.g., Corporate Headquarters, Downtown Store"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Display name for this organization
                </p>
              </div>

              {/* Organization Type */}
              <div className="space-y-2">
                <Label htmlFor="orgType" className="text-sm font-medium">
                  Organization Type <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.orgTypeUID || ""}
                  onValueChange={(value) => handleInputChange("orgTypeUID", value)}
                >
                  <SelectTrigger id="orgType">
                    <SelectValue placeholder="Select organization type" />
                  </SelectTrigger>
                  <SelectContent>
                    {orgTypes.map((type) => (
                      <SelectItem key={type.UID} value={type.UID}>
                        <div className="flex items-center justify-between w-full">
                          <span>{type.Name}</span>
                          <div className="flex gap-2 ml-2">
                            {type.IsCompanyOrg && (
                              <Badge variant="outline" className="text-xs">Company</Badge>
                            )}
                            {type.IsFranchiseeOrg && (
                              <Badge variant="outline" className="text-xs">Franchisee</Badge>
                            )}
                            {type.IsWh && (
                              <Badge variant="outline" className="text-xs">Warehouse</Badge>
                            )}
                          </div>
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Select the type/category of this organization
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
                  disabled={!formData.orgTypeUID}
                >
                  <SelectTrigger id="parentOrg">
                    <SelectValue placeholder={
                      !formData.orgTypeUID 
                        ? "Select organization type first"
                        : "Select parent organization"
                    } />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None (Top Level)</SelectItem>
                    {parentOrganizations.map((org) => (
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

              {/* Country */}
              <div className="space-y-2">
                <Label htmlFor="country">Country</Label>
                <Select
                  value={formData.countryUID || "none"}
                  onValueChange={(value) => handleInputChange("countryUID", value === "none" ? "" : value)}
                >
                  <SelectTrigger id="country">
                    <SelectValue placeholder="Select country" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    {countries.map((country) => (
                      <SelectItem key={country.UID} value={country.UID}>
                        {country.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Region */}
              <div className="space-y-2">
                <Label htmlFor="region">Region/State</Label>
                <Select
                  value={formData.regionUID || "none"}
                  onValueChange={(value) => handleInputChange("regionUID", value === "none" ? "" : value)}
                  disabled={!formData.countryUID}
                >
                  <SelectTrigger id="region">
                    <SelectValue placeholder={
                      !formData.countryUID 
                        ? "Select country first"
                        : "Select region"
                    } />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    {regions.map((region) => (
                      <SelectItem key={region.UID} value={region.UID}>
                        {region.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* City */}
              <div className="space-y-2">
                <Label htmlFor="city">City</Label>
                <Select
                  value={formData.cityUID || "none"}
                  onValueChange={(value) => handleInputChange("cityUID", value === "none" ? "" : value)}
                  disabled={!formData.regionUID}
                >
                  <SelectTrigger id="city">
                    <SelectValue placeholder={
                      !formData.regionUID 
                        ? "Select region first"
                        : "Select city"
                    } />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    {cities.map((city) => (
                      <SelectItem key={city.UID} value={city.UID}>
                        {city.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Company Organization Type - Commented out, passing null by default */}
              {/* <div className="space-y-2">
                <Label htmlFor="companyUID">Company Organization Type</Label>
                <Select
                  value={formData.companyUID || "none"}
                  onValueChange={(value) => handleInputChange("companyUID", value === "none" ? "" : value)}
                >
                  <SelectTrigger id="companyUID">
                    <SelectValue placeholder="Select company organization type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    {companyOrgTypes.map((type) => (
                      <SelectItem key={type.UID} value={type.UID}>
                        {type.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Select the company organization type
                </p>
              </div> */}

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
                  Current operational status of the organization
                </p>
              </div>

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
                      Enable or disable this organization
                    </p>
                  </div>
                </div>
              </div>

              {/* Early Access */}
              <div className="space-y-2">
                <Label className="text-sm font-medium">Early Access</Label>
                <div className="flex items-center space-x-3 p-4 border rounded-lg bg-gray-50">
                  <Switch
                    id="hasEarlyAccess"
                    checked={formData.hasEarlyAccess || false}
                    onCheckedChange={(checked) => handleInputChange("hasEarlyAccess", checked)}
                  />
                  <div className="flex-1">
                    <Label htmlFor="hasEarlyAccess" className="font-medium text-sm cursor-pointer">
                      Enable Early Access
                    </Label>
                    <p className="text-xs text-muted-foreground mt-1">
                      Grant early access to new features for this organization
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Selected Type Info */}
            {formData.orgTypeUID && (() => {
              const selectedType = orgTypes.find(t => t.UID === formData.orgTypeUID)
              return selectedType && (
                <div className="col-span-2 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <h4 className="text-sm font-medium text-blue-900 mb-2">Organization Type Information</h4>
                  <div className="space-y-2">
                    <p className="text-sm text-blue-800">
                      <span className="font-medium">Selected Type:</span> {selectedType.Name}
                    </p>
                    <div className="flex gap-2">
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
                      {selectedType.IsWh && (
                        <Badge variant="outline" className="text-xs border-blue-300">
                          Warehouse
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
                onClick={() => router.push("/administration/organization-management/organizations")}
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