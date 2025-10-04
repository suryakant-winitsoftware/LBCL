'use client'

import { useState, useEffect } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { ArrowLeft, Edit, Building2, MapPin, Users, Globe, Calendar } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { useToast } from '@/components/ui/use-toast'
import { organizationService, Organization, OrgType, TaxGroup } from '@/services/organizationService'
import { usePagePermissions } from '@/hooks/use-page-permissions'

export default function OrganizationDetailPage() {
  const params = useParams()
  const router = useRouter()
  const { toast } = useToast()
  const permissions = usePagePermissions()
  
  const organizationId = params.id as string
  
  const [organization, setOrganization] = useState<Organization | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (organizationId) {
      fetchOrganizationDetails()
    }
  }, [organizationId])

  const fetchOrganizationDetails = async () => {
    setLoading(true)
    setError(null)
    
    try {
      console.log('Fetching organization with ID:', organizationId)
      
      // Validate organizationId before making the call
      if (!organizationId || 
          organizationId === 'undefined' || 
          organizationId === 'null' ||
          organizationId === 'MAINx' ||
          organizationId.includes('/')) {
        throw new Error(`Invalid organization ID: "${organizationId}"`)
      }
      
      const orgData = await organizationService.getOrganizationByUID(organizationId)
      
      // Validate response data
      if (!orgData || !orgData.UID) {
        throw new Error('Organization not found or invalid response')
      }
      
      setOrganization(orgData)
    } catch (err) {
      console.error('Error fetching organization details:', {
        organizationId,
        error: err instanceof Error ? err.message : String(err),
        stack: err instanceof Error ? err.stack : undefined
      })
      
      const errorMessage = err instanceof Error ? err.message : 'Unknown error'
      setError(errorMessage)
      
      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive"
      })
      
      // Redirect to list page after 3 seconds if the organization is not found
      setTimeout(() => {
        router.push('/administration/organization-management/organizations')
      }, 3000)
    } finally {
      setLoading(false)
    }
  }

  const handleEdit = () => {
    router.push(`/administration/organization-management/organizations/create?id=${organizationId}`)
  }

  const handleBack = () => {
    router.push('/administration/organization-management/organizations')
  }

  if (loading) {
    return (
      <div className="space-y-6">
        {/* Header Skeleton */}
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10 rounded" />
          <div>
            <Skeleton className="h-8 w-56 mb-2" />
            <Skeleton className="h-4 w-80" />
          </div>
        </div>

        {/* Content Skeleton */}
        <div className="grid gap-6 md:grid-cols-2">
          {[...Array(4)].map((_, i) => (
            <Card key={i}>
              <CardHeader>
                <Skeleton className="h-6 w-40" />
                <Skeleton className="h-4 w-64" />
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3">
                  {[...Array(3)].map((_, j) => (
                    <div key={j} className="flex justify-between">
                      <Skeleton className="h-4 w-24" />
                      <Skeleton className="h-4 w-32" />
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  if (error || !organization) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={handleBack}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Organization Not Found</h1>
          </div>
        </div>
        
        <Card>
          <CardContent className="p-6">
            <div className="space-y-4">
              <p className="text-muted-foreground">
                {error || "The requested organization could not be found."}
              </p>
              {organizationId && (
                <p className="text-sm text-muted-foreground">
                  Organization ID: <code className="px-1 py-0.5 bg-muted rounded">{organizationId}</code>
                </p>
              )}
              <div className="flex gap-2">
                <Button onClick={handleBack}>
                  Back to Organizations
                </Button>
                <Button variant="outline" onClick={() => window.location.reload()}>
                  Retry
                </Button>
              </div>
              <p className="text-sm text-muted-foreground italic">
                Redirecting to organizations list in 3 seconds...
              </p>
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
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={handleBack}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">{organization.Name}</h1>
            <p className="text-muted-foreground">
              {organization.Code} " {organization.OrgTypeName}
            </p>
          </div>
        </div>
        
        {permissions.canEdit && (
          <Button onClick={handleEdit}>
            <Edit className="mr-2 h-4 w-4" />
            Edit Organization
          </Button>
        )}
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Basic Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">UID:</span>
                <span className="text-sm font-mono">{organization.UID}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">Code:</span>
                <span className="text-sm font-mono">{organization.Code}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">Name:</span>
                <span className="text-sm">{organization.Name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">Type:</span>
                <Badge variant="outline">{organization.OrgTypeName || organization.OrgTypeUID || "Unknown"}</Badge>
              </div>
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">Status:</span>
                <Badge variant={organization.IsActive ? "default" : "secondary"}>
                  {organization.IsActive ? "Active" : "Inactive"}
                </Badge>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Hierarchy Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              Hierarchy
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">Parent:</span>
                <span className="text-sm">
                  {organization.ParentName || "None (Top Level)"}
                </span>
              </div>
              {organization.ParentUID && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Parent UID:</span>
                  <span className="text-sm font-mono">{organization.ParentUID}</span>
                </div>
              )}
              {organization.CompanyName && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Company:</span>
                  <span className="text-sm">{organization.CompanyName}</span>
                </div>
              )}
              {organization.SeqCode && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Sequence Code:</span>
                  <span className="text-sm font-mono">{organization.SeqCode}</span>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Location Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MapPin className="h-5 w-5" />
              Location
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-3">
              {organization.CountryName && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Country:</span>
                  <span className="text-sm">{organization.CountryName}</span>
                </div>
              )}
              {organization.RegionName && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Region:</span>
                  <span className="text-sm">{organization.RegionName}</span>
                </div>
              )}
              {organization.CityName && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">City:</span>
                  <span className="text-sm">{organization.CityName}</span>
                </div>
              )}
              {!organization.CountryName && !organization.RegionName && !organization.CityName && (
                <p className="text-sm text-muted-foreground">No location information</p>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Tax Information */}
        {organization.TaxGroupName && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Globe className="h-5 w-5" />
                Tax Information
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Tax Group:</span>
                  <span className="text-sm">{organization.TaxGroupName}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Tax Group UID:</span>
                  <span className="text-sm font-mono">{organization.TaxGroupUID}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {/* System Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              System Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-3">
              {organization.CreatedBy && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Created By:</span>
                  <span className="text-sm">{organization.CreatedBy}</span>
                </div>
              )}
              {organization.CreatedTime && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Created:</span>
                  <span className="text-sm">
                    {new Date(organization.CreatedTime).toLocaleDateString()}
                  </span>
                </div>
              )}
              {organization.ModifiedBy && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Modified By:</span>
                  <span className="text-sm">{organization.ModifiedBy}</span>
                </div>
              )}
              {organization.ModifiedTime && (
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Modified:</span>
                  <span className="text-sm">
                    {new Date(organization.ModifiedTime).toLocaleDateString()}
                  </span>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

      </div>
    </div>
  )
}