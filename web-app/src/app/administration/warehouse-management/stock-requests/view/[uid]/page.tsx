'use client'

import { useState, useEffect } from 'react'
import { useRouter, useParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { DataTable } from '@/components/ui/data-table'
import { useToast } from '@/components/ui/use-toast'
import { ArrowLeft, Loader2, Edit, Package, Calendar, User, MapPin } from 'lucide-react'
import { apiService } from '@/services/api'

interface WHStockRequestDetail {
  UID: string
  RequestCode: string
  RequestType: string
  RouteCode?: string
  RouteName?: string
  SourceCode?: string
  SourceName?: string
  TargetCode?: string
  TargetName?: string
  SourceOrgUID?: string
  SourceWHUID?: string
  TargetOrgUID?: string
  TargetWHUID?: string
  SourceOrgCode?: string
  SourceOrgName?: string
  SourceWHCode?: string
  SourceWHName?: string
  TargetOrgCode?: string
  TargetOrgName?: string
  TargetWHCode?: string
  TargetWHName?: string
  Status: string
  StockType?: string
  RequiredByDate: string
  RequestedTime: string
  ModifiedTime?: string
  Remarks?: string
}

interface WHStockRequestLine {
  UID: string
  SKUCode?: string
  SKUName?: string
  UOM?: string
  UOM1?: string
  UOM2?: string
  RequestedQty?: number
  RequestedQty1?: number
  RequestedQty2?: number
  ApprovedQty?: number
  ApprovedQty1?: number
  ApprovedQty2?: number
  CollectedQty?: number
  CollectedQty1?: number
  CollectedQty2?: number
  LineNumber?: number
}

export default function StockRequestViewPage() {
  const router = useRouter()
  const params = useParams()
  const uid = params?.uid as string
  const [loading, setLoading] = useState(true)
  const [requestData, setRequestData] = useState<WHStockRequestDetail | null>(null)
  const [requestLines, setRequestLines] = useState<WHStockRequestLine[]>([])
  const { toast } = useToast()

  // Table columns for request lines
  const columns = [
    {
      accessorKey: 'LineNumber',
      header: () => <div className="pl-6">#</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.getValue('LineNumber') || '-'}</span>
        </div>
      )
    },
    {
      accessorKey: 'SKUCode',
      header: 'SKU Code',
      cell: ({ row }: any) => (
        <span className="font-medium text-sm">{row.getValue('SKUCode') || '-'}</span>
      )
    },
    {
      accessorKey: 'SKUName',
      header: 'SKU Name',
      cell: ({ row }: any) => (
        <span className="text-sm">{row.getValue('SKUName') || '-'}</span>
      )
    },
    {
      accessorKey: 'UOM',
      header: 'UOM',
      cell: ({ row }: any) => (
        <Badge variant="outline" className="text-xs">
          {row.getValue('UOM') || 'N/A'}
        </Badge>
      )
    },
    {
      accessorKey: 'RequestedQty',
      header: 'Requested',
      cell: ({ row }: any) => (
        <span className="text-sm font-medium text-blue-600">
          {row.getValue('RequestedQty') || row.original.RequestedQty1 || 0}
        </span>
      )
    },
    {
      accessorKey: 'ApprovedQty',
      header: 'Approved',
      cell: ({ row }: any) => (
        <span className="text-sm font-medium text-green-600">
          {row.getValue('ApprovedQty') || row.original.ApprovedQty1 || 0}
        </span>
      )
    },
    {
      accessorKey: 'CollectedQty',
      header: () => <div className="text-right pr-6">Collected</div>,
      cell: ({ row }: any) => (
        <div className="text-right pr-6">
          <span className="text-sm font-medium text-purple-600">
            {row.getValue('CollectedQty') || row.original.CollectedQty1 || 0}
          </span>
        </div>
      )
    },
  ]

  useEffect(() => {
    if (uid) {
      fetchRequestDetails()
    }
  }, [uid])

  const fetchRequestDetails = async () => {
    try {
      setLoading(true)

      const response: any = await apiService.get(`/WHStock/SelectLoadRequestDataByUID?UID=${uid}`)

      const data = response.Data

      // Backend returns: { WHStockRequest: {...}, WHStockRequestLines: [...] }
      if (data && data.WHStockRequest) {
        setRequestData(data.WHStockRequest)
        setRequestLines(data.WHStockRequestLines || [])
      } else if (data && Array.isArray(data) && data.length >= 2) {
        // Fallback: Old array format
        const requestDetails = data[0]
        const lines = data[1]

        if (requestDetails && requestDetails.length > 0) {
          setRequestData(requestDetails[0])
        }
        if (lines && Array.isArray(lines)) {
          setRequestLines(lines)
        }
      } else if (data) {
        // Last resort
        setRequestData(data)
        setRequestLines([])
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch request details',
        variant: 'destructive'
      })
      console.error('Error fetching request details:', error)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="container mx-auto py-8 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <Loader2 className="h-8 w-8 animate-spin mx-auto text-primary" />
          <p className="mt-2 text-sm text-gray-600">Loading request details...</p>
        </div>
      </div>
    )
  }

  if (!requestData) {
    return (
      <div className="container mx-auto py-8">
        <Button variant="outline" onClick={() => router.back()} className="mb-4">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-gray-600">Request not found</p>
          </CardContent>
        </Card>
      </div>
    )
  }

  const statusColors: Record<string, string> = {
    'Pending': 'bg-yellow-100 text-yellow-800',
    'Approved': 'bg-green-100 text-green-800',
    'Rejected': 'bg-red-100 text-red-800',
    'Completed': 'bg-blue-100 text-blue-800',
  }

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between">
        <Button variant="outline" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
      </div>

      {/* Request Header */}
      <Card>
        <CardHeader>
          <div className="flex items-start justify-between">
            <div>
              <CardTitle className="text-2xl">{requestData.RequestCode}</CardTitle>
              <CardDescription className="mt-2">
                Request Type: <span className="font-medium">{requestData.RequestType}</span>
              </CardDescription>
            </div>
            <Badge className={`${statusColors[requestData.Status]} text-sm px-3 py-1`}>
              {requestData.Status}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="flex items-start gap-3">
              <MapPin className="h-5 w-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-600">Source Organization</p>
                <p className="text-sm font-semibold">{requestData.SourceOrgName || '-'}</p>
                <p className="text-xs text-gray-500">{requestData.SourceOrgCode || '-'}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <MapPin className="h-5 w-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-600">Source Warehouse</p>
                <p className="text-sm font-semibold">{requestData.SourceWHName || '-'}</p>
                <p className="text-xs text-gray-500">{requestData.SourceWHCode || '-'}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <MapPin className="h-5 w-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-600">Target Organization</p>
                <p className="text-sm font-semibold">{requestData.TargetOrgName || '-'}</p>
                <p className="text-xs text-gray-500">{requestData.TargetOrgCode || '-'}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <MapPin className="h-5 w-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-600">Target Warehouse</p>
                <p className="text-sm font-semibold">{requestData.TargetWHName || '-'}</p>
                <p className="text-xs text-gray-500">{requestData.TargetWHCode || '-'}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <Package className="h-5 w-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-600">Route</p>
                <p className="text-sm font-semibold">{requestData.RouteName || '-'}</p>
                <p className="text-xs text-gray-500">{requestData.RouteCode || '-'}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <Calendar className="h-5 w-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-600">Required By</p>
                <p className="text-sm font-semibold">
                  {requestData.RequiredByDate ? new Date(requestData.RequiredByDate).toLocaleDateString() : '-'}
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <Calendar className="h-5 w-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-gray-600">Requested On</p>
                <p className="text-sm font-semibold">
                  {requestData.RequestedTime ? new Date(requestData.RequestedTime).toLocaleString() : '-'}
                </p>
              </div>
            </div>

            {requestData.StockType && (
              <div className="flex items-start gap-3">
                <Package className="h-5 w-5 text-gray-400 mt-0.5" />
                <div>
                  <p className="text-sm font-medium text-gray-600">Stock Type</p>
                  <p className="text-sm font-semibold">{requestData.StockType}</p>
                </div>
              </div>
            )}
          </div>

          {requestData.Remarks && (
            <div className="mt-6 p-4 bg-gray-50 rounded-lg">
              <p className="text-sm font-medium text-gray-600 mb-2">Remarks</p>
              <p className="text-sm text-gray-700">{requestData.Remarks}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Request Lines */}
      <Card>
        <CardHeader>
          <CardTitle>Request Line Items</CardTitle>
          <CardDescription>
            {requestLines.length} item{requestLines.length !== 1 ? 's' : ''} in this request
          </CardDescription>
        </CardHeader>
        <CardContent>
          {requestLines.length > 0 ? (
            <DataTable
              columns={columns}
              data={requestLines}
              loading={false}
              searchable={false}
              pagination={false}
              noWrapper={true}
            />
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Package className="h-12 w-12 mx-auto mb-3 text-gray-300" />
              <p>No line items found for this request</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Summary Statistics */}
      {requestLines.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card>
            <CardContent className="pt-6">
              <div className="text-center">
                <p className="text-sm font-medium text-gray-600">Total Requested</p>
                <h3 className="text-3xl font-bold text-blue-600 mt-2">
                  {requestLines.reduce((sum, line) => sum + (line.RequestedQty || line.RequestedQty1 || 0), 0)}
                </h3>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="text-center">
                <p className="text-sm font-medium text-gray-600">Total Approved</p>
                <h3 className="text-3xl font-bold text-green-600 mt-2">
                  {requestLines.reduce((sum, line) => sum + (line.ApprovedQty || line.ApprovedQty1 || 0), 0)}
                </h3>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="text-center">
                <p className="text-sm font-medium text-gray-600">Total Collected</p>
                <h3 className="text-3xl font-bold text-purple-600 mt-2">
                  {requestLines.reduce((sum, line) => sum + (line.CollectedQty || line.CollectedQty1 || 0), 0)}
                </h3>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
