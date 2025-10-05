"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { ArrowLeft, Save } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"
import { territoryService, Territory } from "@/services/territoryService"
import { organizationService } from "@/services/organizationService"
import { authService } from "@/lib/auth-service"
import { Switch } from "@/components/ui/switch"
import { Skeleton } from "@/components/ui/skeleton"

export function CreateTerritory() {
  const router = useRouter()
  const { toast } = useToast()

  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [territories, setTerritories] = useState<Territory[]>([])
  const [parentTerritories, setParentTerritories] = useState<Territory[]>([])
  const [principalOrgUID, setPrincipalOrgUID] = useState<string>("")

  const [formData, setFormData] = useState<Partial<Territory>>({
    UID: "",
    TerritoryCode: "",
    TerritoryName: "",
    ManagerEmpUID: "",
    ClusterCode: "",
    ParentUID: null,
    ItemLevel: 0,
    HasChild: false,
    IsImport: false,
    IsLocal: false,
    IsNonLicense: 0,
    IsActive: true
  })

  useEffect(() => {
    fetchInitialData()
  }, [])

  useEffect(() => {
    if (formData.ItemLevel && formData.ItemLevel > 1 && territories.length > 0) {
      const parentLevel = (formData.ItemLevel || 1) - 1
      const parents = territories.filter(ter => ter.ItemLevel === parentLevel)
      setParentTerritories(parents)
    } else {
      setParentTerritories([])
    }
  }, [formData.ItemLevel, territories])

  const fetchInitialData = async () => {
    setLoading(true)
    try {
      // Fetch territories and organizations in parallel
      const [territoriesResult, orgsResult] = await Promise.all([
        territoryService.getTerritories(1, 1000),
        organizationService.getOrganizations(1, 1000)
      ])

      setTerritories(territoriesResult.data)

      // Find and auto-select the principal organization (first one with ShowInTemplate = true)
      const principalOrg = orgsResult.data.find(org => org.ShowInTemplate === true)
      if (principalOrg) {
        setPrincipalOrgUID(principalOrg.UID)
        console.log("Auto-selected principal organization:", principalOrg.UID, principalOrg.Name)
      } else if (orgsResult.data.length > 0) {
        // Fallback to first org if no ShowInTemplate found
        setPrincipalOrgUID(orgsResult.data[0].UID)
        console.log("Auto-selected first organization:", orgsResult.data[0].UID, orgsResult.data[0].Name)
      }
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

  const validateForm = (): boolean => {
    if (!formData.TerritoryCode?.trim()) {
      toast({
        title: "Validation Error",
        description: "Territory code is required",
        variant: "destructive"
      })
      return false
    }

    if (!formData.TerritoryName?.trim()) {
      toast({
        title: "Validation Error",
        description: "Territory name is required",
        variant: "destructive"
      })
      return false
    }

    if (formData.ItemLevel && formData.ItemLevel > 1 && !formData.ParentUID) {
      toast({
        title: "Validation Error",
        description: "Parent territory is required for levels greater than 1",
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
      const currentUser = authService.getCurrentUser()
      const currentUserUID = currentUser?.uid || currentUser?.id || "SYSTEM"
      const currentTime = new Date().toISOString()

      // Use the principal organization UID that was auto-selected
      if (!principalOrgUID) {
        toast({
          title: "Error",
          description: "Organization not loaded. Please refresh the page.",
          variant: "destructive"
        })
        return
      }

      const submitData = {
        ...formData,
        UID: formData.TerritoryCode,
        OrgUID: principalOrgUID,
        CreatedBy: currentUserUID,
        CreatedTime: currentTime,
        ModifiedBy: currentUserUID,
        ModifiedTime: currentTime
      }

      await territoryService.createTerritory(submitData)
      toast({
        title: "Success",
        description: "Territory created successfully"
      })

      router.push("/administration/territory-management/territories")
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to create territory",
        variant: "destructive"
      })
    } finally {
      setSaving(false)
    }
  }

  const handleInputChange = (field: keyof Territory, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }))
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10 rounded" />
          <div>
            <Skeleton className="h-8 w-48 mb-2" />
            <Skeleton className="h-4 w-64" />
          </div>
        </div>

        <div className="max-w-2xl">
          <div className="bg-white rounded-lg border p-6 space-y-6">
            <div>
              <Skeleton className="h-6 w-32 mb-2" />
              <Skeleton className="h-4 w-48" />
            </div>

            <div className="grid gap-6 md:grid-cols-2">
              {[...Array(8)].map((_, i) => (
                <div key={i} className="space-y-2">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-10 w-full rounded" />
                  <Skeleton className="h-3 w-32" />
                </div>
              ))}
            </div>

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
          onClick={() => router.push("/administration/territory-management/territories")}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Territory</h1>
          <p className="text-muted-foreground">
            Add a new territory to the system
          </p>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Territory Details</CardTitle>
            <CardDescription>
              Enter the territory information below
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-6 md:grid-cols-2">
              {/* Territory Code */}
              <div className="space-y-2">
                <Label htmlFor="code" className="text-sm font-medium">
                  Territory Code <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="code"
                  value={formData.TerritoryCode || ""}
                  onChange={(e) => {
                    const upperCaseCode = e.target.value.toUpperCase()
                    handleInputChange("TerritoryCode", upperCaseCode)
                    handleInputChange("UID", upperCaseCode)
                  }}
                  placeholder="e.g., NORTH, SOUTH, EAST"
                  className="font-mono"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Unique identifier for this territory (will be converted to uppercase)
                </p>
              </div>

              {/* Territory Name */}
              <div className="space-y-2">
                <Label htmlFor="name" className="text-sm font-medium">
                  Territory Name <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="name"
                  value={formData.TerritoryName || ""}
                  onChange={(e) => handleInputChange("TerritoryName", e.target.value)}
                  placeholder="e.g., North Region, South Zone"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Display name for this territory
                </p>
              </div>

              {/* Manager Employee UID - Hidden for now */}
              {/* <div className="space-y-2">
                <Label htmlFor="manager" className="text-sm font-medium">
                  Manager Employee UID
                </Label>
                <Input
                  id="manager"
                  value={formData.ManagerEmpUID || ""}
                  onChange={(e) => handleInputChange("ManagerEmpUID", e.target.value)}
                  placeholder="e.g., EMP001"
                />
                <p className="text-xs text-muted-foreground">
                  Territory manager employee identifier
                </p>
              </div> */}

              {/* Cluster Code - Hidden for now */}
              {/* <div className="space-y-2">
                <Label htmlFor="cluster" className="text-sm font-medium">
                  Cluster Code
                </Label>
                <Input
                  id="cluster"
                  value={formData.ClusterCode || ""}
                  onChange={(e) => handleInputChange("ClusterCode", e.target.value.toUpperCase())}
                  placeholder="e.g., CLR1, CLR2"
                />
                <p className="text-xs text-muted-foreground">
                  Cluster grouping for this territory
                </p>
              </div> */}

              {/* Item Level - Hidden for now */}
              {/* <div className="space-y-2">
                <Label htmlFor="level" className="text-sm font-medium">
                  Hierarchy Level <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.ItemLevel?.toString() || "0"}
                  onValueChange={(value) => handleInputChange("ItemLevel", parseInt(value))}
                >
                  <SelectTrigger id="level">
                    <SelectValue placeholder="Select level" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">Level 0 (Root)</SelectItem>
                    <SelectItem value="1">Level 1</SelectItem>
                    <SelectItem value="2">Level 2</SelectItem>
                    <SelectItem value="3">Level 3</SelectItem>
                    <SelectItem value="4">Level 4</SelectItem>
                    <SelectItem value="5">Level 5</SelectItem>
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Position in the territory hierarchy
                </p>
              </div> */}

              {/* Parent Territory */}
              <div className="space-y-2">
                <Label htmlFor="parent">
                  Parent Territory (Optional)
                </Label>
                <Select
                  value={formData.ParentUID || "none"}
                  onValueChange={(value) => handleInputChange("ParentUID", value === "none" ? null : value)}
                  disabled={parentTerritories.length === 0}
                >
                  <SelectTrigger id="parent">
                    <SelectValue placeholder={
                      parentTerritories.length === 0
                        ? "No parent territories available"
                        : "Select parent territory"
                    } />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    {parentTerritories.map((territory) => (
                      <SelectItem key={territory.UID} value={territory.UID}>
                        {territory.TerritoryName} ({territory.TerritoryCode})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground">
                  Select a parent territory if this is a sub-territory
                </p>
              </div>

              {/* Territory Properties */}
              <div className="space-y-2 md:col-span-2">
                <Label className="text-sm font-medium">Territory Properties</Label>
                <div className="grid gap-4 md:grid-cols-2">
                  {/* <div className="flex items-center space-x-3 p-4 border rounded-lg bg-gray-50">
                    <Switch
                      id="hasChild"
                      checked={formData.HasChild || false}
                      onCheckedChange={(checked) => handleInputChange("HasChild", checked)}
                    />
                    <div className="flex-1">
                      <Label htmlFor="hasChild" className="font-medium text-sm cursor-pointer">
                        Has Child Territories
                      </Label>
                      <p className="text-xs text-muted-foreground mt-1">
                        Allow sub-territories
                      </p>
                    </div>
                  </div> */}

                  {/* <div className="flex items-center space-x-3 p-4 border rounded-lg bg-gray-50">
                    <Switch
                      id="isImport"
                      checked={formData.IsImport || false}
                      onCheckedChange={(checked) => handleInputChange("IsImport", checked)}
                    />
                    <div className="flex-1">
                      <Label htmlFor="isImport" className="font-medium text-sm cursor-pointer">
                        Import Territory
                      </Label>
                      <p className="text-xs text-muted-foreground mt-1">
                        Import business unit
                      </p>
                    </div>
                  </div> */}

                  {/* <div className="flex items-center space-x-3 p-4 border rounded-lg bg-gray-50">
                    <Switch
                      id="isLocal"
                      checked={formData.IsLocal || false}
                      onCheckedChange={(checked) => handleInputChange("IsLocal", checked)}
                    />
                    <div className="flex-1">
                      <Label htmlFor="isLocal" className="font-medium text-sm cursor-pointer">
                        Local Territory
                      </Label>
                      <p className="text-xs text-muted-foreground mt-1">
                        Local business unit
                      </p>
                    </div>
                  </div> */}

                  <div className="flex items-center space-x-3 p-4 border rounded-lg bg-gray-50">
                    <Switch
                      id="isActive"
                      checked={formData.IsActive !== false}
                      onCheckedChange={(checked) => handleInputChange("IsActive", checked)}
                    />
                    <div className="flex-1">
                      <Label htmlFor="isActive" className="font-medium text-sm cursor-pointer">
                        Active
                      </Label>
                      <p className="text-xs text-muted-foreground mt-1">
                        Territory is active
                      </p>
                    </div>
                  </div>
                </div>
              </div>

              {/* Non-License - Hidden for now */}
              {/* <div className="space-y-2 md:col-span-2">
                <Label htmlFor="nonLicense" className="text-sm font-medium">
                  Non-License Type
                </Label>
                <Select
                  value={formData.IsNonLicense?.toString() || "0"}
                  onValueChange={(value) => handleInputChange("IsNonLicense", parseInt(value))}
                >
                  <SelectTrigger id="nonLicense">
                    <SelectValue placeholder="Select non-license type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">Licensed</SelectItem>
                    <SelectItem value="1">Non-Licensed</SelectItem>
                  </SelectContent>
                </Select>
              </div> */}
            </div>

            {/* Action Buttons */}
            <div className="flex justify-end gap-4 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push("/administration/territory-management/territories")}
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
