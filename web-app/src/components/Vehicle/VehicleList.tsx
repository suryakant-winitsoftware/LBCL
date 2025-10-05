"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Plus, Search, Edit, Trash2, Truck, Eye, MoreVertical } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { useToast } from "@/components/ui/use-toast"
import { vehicleService, Vehicle } from "@/services/vehicleService"
import { organizationService } from "@/services/organizationService"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { Skeleton } from "@/components/ui/skeleton"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

// Import organization hierarchy utilities
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
} from "@/utils/organizationHierarchyUtils"

export function VehicleList() {
  const router = useRouter()
  const { toast } = useToast()

  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState("")
  const [totalCount, setTotalCount] = useState(0)
  const [currentPage, setCurrentPage] = useState(1)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [vehicleToDelete, setVehicleToDelete] = useState<Vehicle | null>(null)
  const pageSize = 10

  // Organization hierarchy state
  const [organizations, setOrganizations] = useState<any[]>([])
  const [orgTypes, setOrgTypes] = useState<any[]>([])
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([])
  const [selectedOrgs, setSelectedOrgs] = useState<{ [key: number]: string }>({})
  const [selectedOrgUID, setSelectedOrgUID] = useState<string>("")

  // Load organizations on mount
  useEffect(() => {
    const loadOrganizations = async () => {
      try {
        const [typesResult, orgsResult] = await Promise.all([
          organizationService.getOrganizationTypes(),
          organizationService.getOrganizations(1, 1000),
        ])

        const activeOrgs = orgsResult.data.filter(
          (org: any) => org.ShowInTemplate === true
        )

        setOrganizations(activeOrgs)
        setOrgTypes(typesResult)

        const initialOrgLevels = initializeOrganizationHierarchy(
          activeOrgs,
          typesResult
        )
        setOrgLevels(initialOrgLevels)

        // Auto-select the first organization (Principal) by default
        if (initialOrgLevels.length > 0 && initialOrgLevels[0].organizations.length > 0) {
          const firstOrg = initialOrgLevels[0].organizations[0]
          const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
            0,
            firstOrg.UID,
            initialOrgLevels,
            {},
            activeOrgs,
            typesResult
          )

          setOrgLevels(updatedLevels)
          setSelectedOrgs(updatedSelectedOrgs)

          const finalOrgUID = getFinalSelectedOrganization(updatedSelectedOrgs)
          if (finalOrgUID) {
            setSelectedOrgUID(finalOrgUID)
          }
        }
      } catch (error) {
        console.error("Error loading organizations:", error)
      }
    }

    loadOrganizations()
  }, [])

  useEffect(() => {
    let isMounted = true

    const fetchVehicles = async () => {
      if (!selectedOrgUID) {
        if (isMounted) {
          setLoading(false)
        }
        return
      }

      console.log("VehicleList: Fetching vehicles for org:", selectedOrgUID)
      if (isMounted) {
        setLoading(true)
      }

      try {
        const filterCriterias = searchTerm
          ? [
              {
                propertyName: "VehicleNo",
                comparisonOperator: 6, // Contains
                value: searchTerm,
                logicalOperator: 1
              }
            ]
          : []

        const result = await vehicleService.getVehicles(
          currentPage,
          pageSize,
          selectedOrgUID,
          filterCriterias
        )

        if (isMounted) {
          console.log("VehicleList: Loaded", result.data?.length || 0, "vehicles")
          setVehicles(result.data || [])
          setTotalCount(result.totalCount || 0)
        }
      } catch (error) {
        console.error("VehicleList: Error loading vehicles:", error)
        if (isMounted) {
          setVehicles([])
          setTotalCount(0)
        }
      } finally {
        if (isMounted) {
          setLoading(false)
        }
      }
    }

    fetchVehicles()

    return () => {
      console.log("VehicleList: Cleanup - unmounting")
      isMounted = false
    }
  }, [currentPage, searchTerm, selectedOrgUID])

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

    const finalOrgUID = getFinalSelectedOrganization(updatedSelectedOrgs)
    if (finalOrgUID) {
      setSelectedOrgUID(finalOrgUID)
    }
  }

  // Reset organization selection
  const handleOrganizationReset = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(
      organizations,
      orgTypes
    )
    setOrgLevels(resetLevels)
    setSelectedOrgs(resetSelectedOrgs)
    setSelectedOrgUID("")
    setVehicles([])
    setTotalCount(0)

    toast({
      title: "Organization Reset",
      description: "Organization selection has been cleared",
    })
  }

  const handleDelete = async () => {
    if (!vehicleToDelete) return

    try {
      await vehicleService.deleteVehicle(vehicleToDelete.UID)
      toast({
        title: "Success",
        description: "Vehicle deleted successfully"
      })
      // Trigger refetch by resetting page or toggling a state
      setCurrentPage(1)
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete vehicle",
        variant: "destructive"
      })
    } finally {
      setDeleteDialogOpen(false)
      setVehicleToDelete(null)
    }
  }

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A"
    return new Date(dateString).toLocaleDateString()
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <Skeleton className="h-8 w-48 mb-2" />
            <Skeleton className="h-4 w-64" />
          </div>
          <Skeleton className="h-10 w-32" />
        </div>

        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-32 mb-2" />
            <Skeleton className="h-4 w-48" />
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <Skeleton className="h-10 w-full" />
              <div className="space-y-2">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} className="h-16 w-full" />
                ))}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Vehicle Management</h1>
          <p className="text-muted-foreground">
            Manage your fleet vehicles
          </p>
        </div>
        <Button onClick={() => router.push("/administration/vehicle-management/vehicles/create")}>
          <Plus className="mr-2 h-4 w-4" />
          Add Vehicle
        </Button>
      </div>

      {/* Organization Selection */}
      <Card>
        <CardHeader>
          <CardTitle>Select Organization</CardTitle>
          <CardDescription>
            Choose an organization to view its vehicles
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
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
                  <div className="col-span-3">
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={handleOrganizationReset}
                    >
                      Reset Organization
                    </Button>
                  </div>
                )}
              </>
            ) : (
              <div className="col-span-3 text-center py-4 text-muted-foreground">
                <p className="text-sm">Loading organizations...</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Message when no organization selected */}
      {!selectedOrgUID && (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Truck className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">Select Organization First</h3>
            <p className="text-muted-foreground text-center">
              Please select an organization from the dropdown above to view vehicles
            </p>
          </CardContent>
        </Card>
      )}

      {/* Table Card */}
      {selectedOrgUID && (
        <Card>
          <CardHeader>
            <CardTitle>Vehicles</CardTitle>
            <CardDescription>
              A list of all vehicles in the selected organization
            </CardDescription>
          </CardHeader>
          <CardContent>
            {/* Search */}
            <div className="mb-4">
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by vehicle number..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>

            {/* Table */}
            {loading ? (
              <div className="space-y-2">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} className="h-16 w-full" />
                ))}
              </div>
            ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Vehicle No</TableHead>
                    <TableHead>Registration No</TableHead>
                    <TableHead>Model</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Capacity</TableHead>
                    <TableHead>Warehouse</TableHead>
                    <TableHead>Territory</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {vehicles.map((vehicle) => (
                    <TableRow key={vehicle.UID}>
                      <TableCell className="font-medium">{vehicle.VehicleNo}</TableCell>
                      <TableCell>{vehicle.RegistrationNo}</TableCell>
                      <TableCell>{vehicle.Model}</TableCell>
                      <TableCell>{vehicle.Type}</TableCell>
                      <TableCell>{vehicle.Capacity ? `${vehicle.Capacity} m³` : '-'}</TableCell>
                      <TableCell>{vehicle.WarehouseCode || '-'}</TableCell>
                      <TableCell>{vehicle.TerritoryUID || '-'}</TableCell>
                      <TableCell>
                        <Badge variant={vehicle.IsActive ? "default" : "secondary"}>
                          {vehicle.IsActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreVertical className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() =>
                                router.push(`/administration/distributor-management/MaintainVan/view/${vehicle.UID}`)
                              }
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() =>
                                router.push(`/administration/distributor-management/MaintainVan/edit/${vehicle.UID}`)
                              }
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              className="text-red-600"
                              onClick={() => {
                                setVehicleToDelete(vehicle)
                                setDeleteDialogOpen(true)
                              }}
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
            )}

            {/* Pagination */}
            {totalCount > pageSize && (
            <div className="flex items-center justify-between mt-4">
              <p className="text-sm text-muted-foreground">
                Showing {(currentPage - 1) * pageSize + 1} to{" "}
                {Math.min(currentPage * pageSize, totalCount)} of {totalCount} vehicles
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(currentPage - 1)}
                  disabled={currentPage === 1}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(currentPage + 1)}
                  disabled={currentPage * pageSize >= totalCount}
                >
                  Next
                </Button>
              </div>
            </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will permanently delete the vehicle "{vehicleToDelete?.VehicleNo}".
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete}>Delete</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
