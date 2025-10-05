'use client'

import { useState, useEffect } from 'react'
import { useRouter, useParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { useToast } from '@/components/ui/use-toast'
import { ArrowLeft, Loader2, Warehouse } from 'lucide-react'
import { apiService } from '@/services/api'

export default function WarehouseViewPage() {
  const router = useRouter()
  const params = useParams()
  const uid = params.uid as string
  const { toast } = useToast()

  const [warehouse, setWarehouse] = useState<any>(null)
  const [parentOrg, setParentOrg] = useState<any>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (uid) {
      fetchWarehouseDetails()
    }
  }, [uid])

  const fetchWarehouseDetails = async () => {
    try {
      setLoading(true)

      // Fetch warehouse details
      const response: any = await apiService.get(`/Org/GetOrgByUID?UID=${uid}`)

      if (response.IsSuccess && response.Data) {
        setWarehouse(response.Data)

        // Fetch parent organization details if ParentUID exists
        if (response.Data.ParentUID) {
          try {
            const parentResponse: any = await apiService.get(`/Org/GetOrgByUID?UID=${response.Data.ParentUID}`)
            if (parentResponse.IsSuccess && parentResponse.Data) {
              setParentOrg(parentResponse.Data)
            }
          } catch (error) {
            console.warn('Failed to fetch parent org:', error)
          }
        }
      } else {
        toast({
          title: 'Error',
          description: 'Warehouse not found',
          variant: 'destructive'
        })
        router.push('/administration/warehouse-management/warehouses')
      }
    } catch (error) {
      console.error('Error fetching warehouse:', error)
      toast({
        title: 'Error',
        description: 'Failed to load warehouse details',
        variant: 'destructive'
      })
      router.push('/administration/warehouse-management/warehouses')
    } finally {
      setLoading(false)
    }
  }

  const handleEdit = () => {
    router.push(`/administration/warehouse-management/warehouses/create?id=${uid}`)
  }

  const handleDelete = async () => {
    if (!confirm(`Are you sure you want to delete warehouse "${warehouse?.Name}"?`)) {
      return
    }

    try {
      await apiService.delete(`/Org/DeleteViewFranchiseeWarehouse?UID=${uid}`)
      toast({
        title: 'Success',
        description: 'Warehouse deleted successfully',
      })
      router.push('/administration/warehouse-management/warehouses')
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete warehouse',
        variant: 'destructive'
      })
    }
  }

  if (loading) {
    return (
      <div className="container mx-auto py-8 flex items-center justify-center min-h-[400px]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    )
  }

  if (!warehouse) {
    return (
      <div className="container mx-auto py-8">
        <p>Warehouse not found</p>
      </div>
    )
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="outline"
          size="sm"
          onClick={() => router.push('/administration/warehouse-management/warehouses')}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Warehouses
        </Button>
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <Warehouse className="h-6 w-6" />
            {warehouse.Name}
          </h1>
          <p className="text-sm text-muted-foreground">Warehouse Details</p>
        </div>
      </div>

      {/* Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle>Basic Information</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Warehouse Code</label>
              <p className="text-base font-semibold mt-1">{warehouse.Code || '-'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Warehouse Name</label>
              <p className="text-base font-semibold mt-1">{warehouse.Name || '-'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Type</label>
              <p className="text-base mt-1">
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                  {warehouse.OrgTypeUID || '-'}
                </span>
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Status</label>
              <p className="text-base mt-1">
                {warehouse.IsActive ? (
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    Active
                  </span>
                ) : (
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                    Inactive
                  </span>
                )}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Parent Organization */}
      {parentOrg && (
        <Card>
          <CardHeader>
            <CardTitle>Parent Organization</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              <div>
                <label className="text-sm font-medium text-muted-foreground">Parent Name</label>
                <p className="text-base font-semibold mt-1">{parentOrg.Name || '-'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground">Parent Code</label>
                <p className="text-base mt-1">{parentOrg.Code || '-'}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Audit Information */}
      <Card>
        <CardHeader>
          <CardTitle>Audit Information</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Created By</label>
              <p className="text-base mt-1">{warehouse.CreatedBy || '-'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Created Time</label>
              <p className="text-base mt-1">
                {warehouse.CreatedTime ? new Date(warehouse.CreatedTime).toLocaleString() : '-'}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Modified By</label>
              <p className="text-base mt-1">{warehouse.ModifiedBy || '-'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Modified Time</label>
              <p className="text-base mt-1">
                {warehouse.ModifiedTime ? new Date(warehouse.ModifiedTime).toLocaleString() : '-'}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Server Add Time</label>
              <p className="text-base mt-1">
                {warehouse.ServerAddTime ? new Date(warehouse.ServerAddTime).toLocaleString() : '-'}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Server Modified Time</label>
              <p className="text-base mt-1">
                {warehouse.ServerModifiedTime ? new Date(warehouse.ServerModifiedTime).toLocaleString() : '-'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
