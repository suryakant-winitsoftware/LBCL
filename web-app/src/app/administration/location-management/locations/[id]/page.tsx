'use client'

import { useState, useEffect } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { 
  ArrowLeft, Edit, MapPin, Building2, Users, Calendar, 
  Hash, Target, Globe, Shield, Activity, Info, 
  Clock, UserCheck, MapPinned, Network, Layers,
  ChevronRight, CheckCircle2, XCircle, AlertCircle 
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { useToast } from '@/components/ui/use-toast'
import { Separator } from '@/components/ui/separator'
import { locationService, Location, LocationType } from '@/services/locationService'
import { usePagePermissions } from '@/hooks/use-page-permissions'

export default function LocationDetailPage() {
  const params = useParams()
  const router = useRouter()
  const { toast } = useToast()
  const permissions = usePagePermissions()
  
  const locationId = params.id as string
  
  const [location, setLocation] = useState<Location | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [breadcrumb, setBreadcrumb] = useState<Location[]>([])

  useEffect(() => {
    if (locationId) {
      fetchLocationDetails()
    }
  }, [locationId])

  const fetchLocationDetails = async () => {
    setLoading(true)
    setError(null)
    
    try {
      console.log('Fetching location with ID:', locationId)
      
      // Validate locationId before making the call
      if (!locationId || locationId === 'undefined' || locationId === 'null') {
        throw new Error('Invalid location ID provided')
      }
      
      const locationData = await locationService.getLocationById(locationId)
      setLocation(locationData)

      // Get breadcrumb for hierarchy
      if (locationData) {
        try {
          const breadcrumbData = await locationService.getLocationBreadcrumb(locationId)
          setBreadcrumb(breadcrumbData)
        } catch (breadcrumbError) {
          console.warn('Failed to load breadcrumb:', breadcrumbError)
        }
      }
    } catch (err) {
      console.error('Error fetching location details:', {
        locationId,
        error: err instanceof Error ? err.message : String(err)
      })
      setError('Failed to load location details')
      toast({
        title: "Error",
        description: `Failed to load location details: ${err instanceof Error ? err.message : 'Unknown error'}`,
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const handleEdit = () => {
    router.push(`/administration/location-management/locations/create?id=${locationId}`)
  }

  const handleBack = () => {
    router.push('/administration/location-management/locations')
  }


  // Get status badge variant
  const getStatusVariant = (hasChild: boolean) => {
    return hasChild ? 'default' : 'secondary'
  }

  // Format date with time
  const formatDateTime = (date: string) => {
    if (!date) return 'N/A'
    const d = new Date(date)
    return d.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  if (loading) {
    return (
      <div className="animate-in fade-in-0 duration-500 space-y-6">
        {/* Header Skeleton */}
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10 rounded-lg" />
          <div className="space-y-2">
            <Skeleton className="h-8 w-56" />
            <Skeleton className="h-4 w-80" />
          </div>
        </div>

        {/* Stats Skeleton */}
        <div className="grid gap-4 grid-cols-2 lg:grid-cols-4">
          {[...Array(4)].map((_, i) => (
            <Card key={i} className="border-muted">
              <CardContent className="p-6">
                <Skeleton className="h-4 w-20 mb-2" />
                <Skeleton className="h-8 w-32" />
              </CardContent>
            </Card>
          ))}
        </div>

        {/* Content Skeleton */}
        <div className="grid gap-6 lg:grid-cols-3">
          {[...Array(3)].map((_, i) => (
            <Card key={i} className="border-muted">
              <CardHeader>
                <Skeleton className="h-6 w-40" />
                <Skeleton className="h-4 w-64" />
              </CardHeader>
              <CardContent className="space-y-4">
                {[...Array(4)].map((_, j) => (
                  <div key={j} className="flex justify-between">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-4 w-32" />
                  </div>
                ))}
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  if (error || !location) {
    return (
      <div className="animate-in fade-in-0 duration-500 space-y-6">
        <div className="flex items-center gap-4">
          <Button 
            variant="ghost" 
            size="icon" 
            onClick={handleBack}
            className="hover:bg-destructive/10 transition-colors"
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Location Not Found</h1>
          </div>
        </div>
        
        <Card className="border-destructive/20 bg-destructive/5">
          <CardContent className="p-6">
            <div className="flex items-start gap-3">
              <AlertCircle className="h-5 w-5 text-destructive mt-0.5" />
              <div className="space-y-2">
                <p className="font-medium">Unable to load location</p>
                <p className="text-sm text-muted-foreground">
                  {error || "The requested location could not be found."}
                </p>
                <Button onClick={handleBack} className="mt-4">
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Back to Locations
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={handleBack}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Locations
        </Button>
        {permissions.canEdit && (
          <Button onClick={handleEdit}>
            <Edit className="mr-2 h-4 w-4" />
            Edit Location
          </Button>
        )}
      </div>

      {/* Location Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl flex items-center gap-3">
            {location.Name}
            {location.Name === 'Country_UAE' && (
              <Badge variant="outline" className="text-xs">
                <Globe className="mr-1 h-3 w-3" />
                UAE Region
              </Badge>
            )}
          </CardTitle>
          <div className="flex items-center gap-2 text-muted-foreground">
            <Hash className="h-3 w-3" />
            <code className="text-sm">{location.Code}</code>
            <Separator orientation="vertical" className="h-4" />
            <Badge variant="outline" className="text-xs">
              {location.LocationTypeName || location.LocationTypeUID || "Unknown Type"}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            {location.UID && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Location UID</p>
                <p className="font-semibold">{location.UID}</p>
              </div>
            )}
            {location.Code && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Location Code</p>
                <p className="font-semibold">{location.Code}</p>
              </div>
            )}
            {location.LocationTypeName && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Type</p>
                <p className="font-semibold">{location.LocationTypeName}</p>
              </div>
            )}
            {location.ItemLevel !== undefined && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Hierarchy Level</p>
                <p className="font-semibold">Level {location.ItemLevel}</p>
              </div>
            )}
            {location.CreatedBy && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Created By</p>
                <p className="font-semibold">{location.CreatedBy}</p>
              </div>
            )}
            {location.CreatedTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Created Time</p>
                <p className="font-semibold">{formatDateTime(location.CreatedTime)}</p>
              </div>
            )}
            {location.ModifiedBy && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Modified By</p>
                <p className="font-semibold">{location.ModifiedBy}</p>
              </div>
            )}
            {location.ModifiedTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Modified Time</p>
                <p className="font-semibold">{formatDateTime(location.ModifiedTime)}</p>
              </div>
            )}
            {location.ServerAddTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Server Add Time</p>
                <p className="font-semibold">{formatDateTime(location.ServerAddTime)}</p>
              </div>
            )}
            {location.ServerModifiedTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Server Modified Time</p>
                <p className="font-semibold">{formatDateTime(location.ServerModifiedTime)}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Hierarchy Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Network className="h-5 w-5" />
            Location Hierarchy
          </CardTitle>
        </CardHeader>
        <CardContent>
          {/* Breadcrumb */}
          {breadcrumb.length > 1 && (
            <div className="mb-6 p-4 bg-muted/30 rounded-lg">
              <div className="flex items-center gap-2 text-sm">
                <Layers className="h-4 w-4 text-muted-foreground" />
                <span className="text-muted-foreground font-medium">Path:</span>
                {breadcrumb.map((item, index) => (
                  <div key={item.UID} className="flex items-center">
                    {index > 0 && <ChevronRight className="h-3 w-3 mx-1 text-muted-foreground" />}
                    <Badge 
                      variant={index === breadcrumb.length - 1 ? "default" : "outline"}
                      className="text-xs"
                    >
                      {item.Name}
                    </Badge>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h4 className="font-semibold mb-3 flex items-center gap-2">
                <Users className="h-4 w-4" />
                Parent Location
              </h4>
              <div className="border rounded-lg p-4 space-y-3">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-muted-foreground">Status</span>
                  <Badge variant={location.ParentUID ? "default" : "secondary"}>
                    {location.ParentUID ? (
                      breadcrumb.length > 1 ? breadcrumb[breadcrumb.length - 2]?.Name || "Has Parent" : "Has Parent"
                    ) : (
                      "None (Top Level)"
                    )}
                  </Badge>
                </div>
                {location.ParentUID && (
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-muted-foreground">Parent UID</span>
                    <code className="text-sm font-mono bg-muted px-2 py-1 rounded">
                      {location.ParentUID}
                    </code>
                  </div>
                )}
              </div>
            </div>

            <div>
              <h4 className="font-semibold mb-3 flex items-center gap-2">
                <Layers className="h-4 w-4" />
                Child Locations
              </h4>
              <div className="border rounded-lg p-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-muted-foreground">Has Children</span>
                  <Badge variant={location.HasChild ? "default" : "secondary"}>
                    {location.HasChild ? "Yes" : "No"}
                  </Badge>
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Company Association */}
      {location.CompanyUID && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Company Association
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="border rounded-lg p-4">
              <div className="flex justify-between items-center">
                <span className="text-sm text-muted-foreground">Company UID</span>
                <code className="text-sm font-mono bg-muted px-2 py-1 rounded">
                  {location.CompanyUID}
                </code>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Additional Details for UAE */}
      {location.Name === 'Country_UAE' && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Info className="h-5 w-5" />
              Regional Information
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="p-4 bg-gradient-to-r from-primary/10 to-primary/5 rounded-lg border border-primary/20">
              <div className="flex items-start gap-3">
                <Globe className="h-5 w-5 text-primary mt-0.5" />
                <div className="space-y-2">
                  <p className="font-medium">United Arab Emirates Region</p>
                  <p className="text-sm text-muted-foreground">
                    This location represents the country-level entity for UAE operations, 
                    serving as the top-level geographical division for all business activities in the region.
                  </p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}