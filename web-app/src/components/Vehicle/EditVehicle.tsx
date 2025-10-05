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
import { vehicleService, Vehicle } from "@/services/vehicleService"
import { authService } from "@/lib/auth-service"
import { Switch } from "@/components/ui/switch"
import { Skeleton } from "@/components/ui/skeleton"

interface EditVehicleProps {
  uid: string
}

export function EditVehicle({ uid }: EditVehicleProps) {
  const router = useRouter()
  const { toast } = useToast()

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [formData, setFormData] = useState<Partial<Vehicle>>({})

  useEffect(() => {
    fetchVehicle()
  }, [uid])

  const fetchVehicle = async () => {
    setLoading(true)
    try {
      const vehicle = await vehicleService.getVehicleByUID(uid)
      setFormData(vehicle)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch vehicle details",
        variant: "destructive"
      })
      router.push("/administration/vehicle-management/vehicles")
    } finally {
      setLoading(false)
    }
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

    setSaving(true)
    try {
      const currentUser = authService.getCurrentUser()
      const currentUserUID = currentUser?.uid || currentUser?.id || "SYSTEM"
      const currentTime = new Date().toISOString()

      const submitData = {
        ...formData,
        ModifiedBy: currentUserUID,
        ModifiedTime: currentTime,
        TruckSIDate: formData.TruckSIDate ? new Date(formData.TruckSIDate).toISOString() : new Date().toISOString(),
        RoadTaxExpiryDate: formData.RoadTaxExpiryDate ? new Date(formData.RoadTaxExpiryDate).toISOString() : new Date().toISOString(),
        InspectionDate: formData.InspectionDate ? new Date(formData.InspectionDate).toISOString() : new Date().toISOString()
      } as Vehicle

      await vehicleService.updateVehicle(submitData)
      toast({
        title: "Success",
        description: "Vehicle updated successfully"
      })

      router.push("/administration/vehicle-management/vehicles")
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to update vehicle",
        variant: "destructive"
      })
    } finally {
      setSaving(false)
    }
  }

  const handleInputChange = (field: keyof Vehicle, value: any) => {
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
          onClick={() => router.push("/administration/vehicle-management/vehicles")}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Edit Vehicle</h1>
          <p className="text-muted-foreground">
            Update vehicle information
          </p>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Vehicle Details</CardTitle>
            <CardDescription>
              Update the vehicle information below
            </CardDescription>
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
                  disabled
                />
                <p className="text-xs text-muted-foreground">
                  Vehicle number cannot be changed
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
                <Input
                  id="model"
                  value={formData.Model || ""}
                  onChange={(e) => handleInputChange("Model", e.target.value)}
                  placeholder="e.g., Toyota Hiace"
                  required
                />
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

              {/* Warehouse Code */}
              <div className="space-y-2">
                <Label htmlFor="warehouseCode" className="text-sm font-medium">
                  Warehouse Code
                </Label>
                <Input
                  id="warehouseCode"
                  value={formData.WarehouseCode || ""}
                  onChange={(e) => handleInputChange("WarehouseCode", e.target.value.toUpperCase())}
                  placeholder="e.g., WH001"
                />
                <p className="text-xs text-muted-foreground">
                  Associated warehouse code
                </p>
              </div>

              {/* Location Code */}
              <div className="space-y-2">
                <Label htmlFor="locationCode" className="text-sm font-medium">
                  Location Code
                </Label>
                <Input
                  id="locationCode"
                  value={formData.LocationCode || ""}
                  onChange={(e) => handleInputChange("LocationCode", e.target.value.toUpperCase())}
                  placeholder="e.g., LOC001"
                />
                <p className="text-xs text-muted-foreground">
                  Location or depot code
                </p>
              </div>

              {/* Territory */}
              <div className="space-y-2">
                <Label htmlFor="territoryUID" className="text-sm font-medium">
                  Territory
                </Label>
                <Input
                  id="territoryUID"
                  value={formData.TerritoryUID || ""}
                  onChange={(e) => handleInputChange("TerritoryUID", e.target.value.toUpperCase())}
                  placeholder="e.g., NORTH, SOUTH"
                />
                <p className="text-xs text-muted-foreground">
                  Territory code or UID
                </p>
              </div>

              {/* Active Status */}
              <div className="space-y-2">
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
                      Vehicle is active and available
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex justify-end gap-4 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push("/administration/vehicle-management/vehicles")}
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
                    Update
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
