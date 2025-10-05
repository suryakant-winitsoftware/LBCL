"use client"

import { useState, useEffect } from "react"
import { useRouter, useParams } from "next/navigation"
import { ArrowLeft, Edit, Truck, Calendar, MapPin, Package, Weight, Gauge } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { Skeleton } from "@/components/ui/skeleton"
import { useToast } from "@/components/ui/use-toast"
import { vehicleService, Vehicle } from "@/services/vehicleService"

export function VehicleDetail() {
  const router = useRouter()
  const params = useParams()
  const { toast } = useToast()
  const uid = params?.uid as string

  const [vehicle, setVehicle] = useState<Vehicle | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchVehicleDetails = async () => {
      if (!uid) return

      try {
        const result = await vehicleService.getVehicleByUID(uid)
        setVehicle(result)
      } catch (error) {
        console.error("Error fetching vehicle details:", error)
        toast({
          title: "Error",
          description: "Failed to load vehicle details",
          variant: "destructive"
        })
      } finally {
        setLoading(false)
      }
    }

    fetchVehicleDetails()
  }, [uid])

  const formatDate = (dateString: string | null) => {
    if (!dateString) return "N/A"
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-10 w-32" />
        </div>
        <div className="grid gap-6">
          <Skeleton className="h-64 w-full" />
          <Skeleton className="h-64 w-full" />
        </div>
      </div>
    )
  }

  if (!vehicle) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Vehicle Not Found</h1>
          </div>
        </div>
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Truck className="h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-muted-foreground">The requested vehicle could not be found.</p>
            <Button className="mt-4" onClick={() => router.back()}>
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{vehicle.VehicleNo}</h1>
          <p className="text-muted-foreground">
            Vehicle Details
          </p>
        </div>
      </div>

      {/* Basic Information */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>Core vehicle details and registration</CardDescription>
            </div>
            <Badge variant={vehicle.IsActive ? "default" : "secondary"}>
              {vehicle.IsActive ? "Active" : "Inactive"}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Vehicle Number</p>
              <p className="text-lg font-semibold">{vehicle.VehicleNo || "N/A"}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Registration Number</p>
              <p className="text-lg font-semibold">{vehicle.RegistrationNo || "N/A"}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Model</p>
              <p className="text-lg font-semibold">{vehicle.Model || "N/A"}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Type</p>
              <p className="text-lg font-semibold">{vehicle.Type || "N/A"}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Capacity & Specifications */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Package className="h-5 w-5" />
            Capacity & Specifications
          </CardTitle>
          <CardDescription>Vehicle capacity and load specifications</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="space-y-1">
              <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                <Weight className="h-4 w-4" />
                Weight Limit
              </div>
              <p className="text-lg font-semibold">
                {vehicle.WeightLimit ? `${vehicle.WeightLimit} kg` : "N/A"}
              </p>
            </div>

            <div className="space-y-1">
              <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                <Gauge className="h-4 w-4" />
                Capacity
              </div>
              <p className="text-lg font-semibold">
                {vehicle.Capacity ? `${vehicle.Capacity} m³` : "N/A"}
              </p>
            </div>

            <div className="space-y-1">
              <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                <Package className="h-4 w-4" />
                Loading Capacity
              </div>
              <p className="text-lg font-semibold">
                {vehicle.LoadingCapacity ? `${vehicle.LoadingCapacity} kg` : "N/A"}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Location & Assignment */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <MapPin className="h-5 w-5" />
            Location & Assignment
          </CardTitle>
          <CardDescription>Warehouse and territory assignment</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Warehouse Code</p>
              <p className="text-lg font-semibold">{vehicle.WarehouseCode || "N/A"}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Location Code</p>
              <p className="text-lg font-semibold">{vehicle.LocationCode || "N/A"}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Territory</p>
              <p className="text-lg font-semibold">{vehicle.TerritoryUID || "N/A"}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Dates & Compliance */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Calendar className="h-5 w-5" />
            Important Dates & Compliance
          </CardTitle>
          <CardDescription>SI, tax, and inspection dates</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Truck SI Date</p>
              <p className="text-lg font-semibold">{formatDate(vehicle.TruckSIDate)}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Road Tax Expiry Date</p>
              <p className="text-lg font-semibold">{formatDate(vehicle.RoadTaxExpiryDate)}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Inspection Date</p>
              <p className="text-lg font-semibold">{formatDate(vehicle.InspectionDate)}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* System Information */}
      <Card>
        <CardHeader>
          <CardTitle>System Information</CardTitle>
          <CardDescription>Created and modified details</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Created By</p>
              <p className="text-base">{vehicle.CreatedBy || "N/A"}</p>
              <p className="text-sm text-muted-foreground">
                {formatDate(vehicle.CreatedTime)}
              </p>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">Last Modified By</p>
              <p className="text-base">{vehicle.ModifiedBy || "N/A"}</p>
              <p className="text-sm text-muted-foreground">
                {formatDate(vehicle.ModifiedTime)}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
