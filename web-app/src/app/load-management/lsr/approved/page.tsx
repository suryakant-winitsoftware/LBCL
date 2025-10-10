"use client"

import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useRouter } from "next/navigation"
import { ArrowLeft, CheckCircle, Download, Eye, Package, Calendar, Weight, Truck, User, CalendarCheck, RefreshCw } from "lucide-react"
import { useEffect, useState } from "react"
import { lsrService, LSRRequest } from "../services/lsr.service"

export default function ApprovedLSRPage() {
  const router = useRouter()
  const [approvedRequests, setApprovedRequests] = useState<LSRRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchApprovedRequests()
  }, [])

  const fetchApprovedRequests = async () => {
    try {
      setLoading(true)
      setError(null)
      const response: any = await lsrService.getApprovedRequests({})
      // Handle PagedResponse structure: Data.Items or Items
      const requests = response?.Data?.Items || response?.Items || []
      setApprovedRequests(requests)
    } catch (err) {
      console.error('Error fetching approved LSR requests:', err)
      setError('Failed to load approved requests')
    } finally {
      setLoading(false)
    }
  }

  const handleBack = () => {
    router.push("/load-management/lsr")
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Load Sheet Generated": return "outline"
      case "In Transit": return "secondary"
      case "Delivered": return "default"
      default: return "secondary"
    }
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <div className="flex items-center justify-between gap-4 mb-4">
          <Button
            variant="outline"
            size="sm"
            onClick={handleBack}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to LSR
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={fetchApprovedRequests}
            className="flex items-center gap-2"
            disabled={loading}
          >
            <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
        </div>
        <div className="flex items-center gap-3 mb-2">
          <div className="p-3 rounded-lg bg-muted">
            <CheckCircle className="h-6 w-6 text-muted-foreground" />
          </div>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Approved Load Service Requests</h1>
            <p className="text-muted-foreground mt-1">View approved requests and their execution details</p>
          </div>
        </div>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-destructive/10 border border-destructive/20 rounded-lg text-destructive text-sm">
          {error}
        </div>
      )}

      <div className="max-w-7xl mx-auto">
        <Card className="border shadow-sm">
          <CardContent className="p-0">
            {loading ? (
              <Table>
                <TableHeader>
                  <TableRow className="bg-muted/50">
                    <TableHead><Skeleton className="h-4 w-24" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-16" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-24" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-28" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-28" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-20" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-16" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-16" /></TableHead>
                    <TableHead className="text-right"><Skeleton className="h-4 w-20 ml-auto" /></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {[1, 2, 3, 4, 5].map((i) => (
                    <TableRow key={i}>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-6 w-20 rounded-full" /></TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Skeleton className="h-8 w-16" />
                          <Skeleton className="h-8 w-20" />
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : approvedRequests.length > 0 ? (
              <Table>
                <TableHeader>
                  <TableRow className="bg-muted/50">
                    <TableHead className="font-semibold">
                      <div className="flex items-center gap-2">
                        <Package className="h-4 w-4" />
                        Request ID
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold">Route</TableHead>
                    <TableHead className="font-semibold">
                      <div className="flex items-center gap-2">
                        <Calendar className="h-4 w-4" />
                        Request Date
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold">
                      <div className="flex items-center gap-2">
                        <CalendarCheck className="h-4 w-4" />
                        Approval Date
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold">
                      <div className="flex items-center gap-2">
                        <Weight className="h-4 w-4" />
                        Approved Load
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold">
                      <div className="flex items-center gap-2">
                        <Truck className="h-4 w-4" />
                        Truck
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold">
                      <div className="flex items-center gap-2">
                        <User className="h-4 w-4" />
                        Driver
                      </div>
                    </TableHead>
                    <TableHead className="font-semibold">Status</TableHead>
                    <TableHead className="text-right font-semibold">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {approvedRequests.map((request, index) => (
                    <TableRow 
                      key={request.id}
                      className={`hover:bg-muted/30 transition-colors ${
                        index % 2 === 0 ? 'bg-white' : 'bg-muted/10'
                      }`}
                    >
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-2">
                          <div className="p-1.5 rounded bg-green-100">
                            <CheckCircle className="h-3 w-3 text-green-600" />
                          </div>
                          <span className="font-semibold">{request.id || request.UID}</span>
                        </div>
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {request.route || 'N/A'}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {request.requestDate || request.createdOn ? new Date(request.requestDate || request.createdOn || '').toLocaleDateString() : 'N/A'}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {request.approvalDate || request.modifiedOn ? new Date(request.approvalDate || request.modifiedOn || '').toLocaleDateString() : 'N/A'}
                      </TableCell>
                      <TableCell className="font-medium">
                        {request.approvedLoad || 'N/A'}
                      </TableCell>
                      <TableCell className="font-medium">
                        <Badge variant="outline" className="text-xs">
                          {request.assignedTruck || 'N/A'}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {request.assignedDriver || 'N/A'}
                      </TableCell>
                      <TableCell>
                        <Badge variant={getStatusColor(request.status || '')} className="text-xs">
                          {request.status || 'Approved'}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            className="flex items-center gap-1"
                            onClick={() => router.push(`/load-management/lsr/view/${request.UID || request.id}`)}
                          >
                            <Eye className="h-3 w-3" />
                            View
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            className="flex items-center gap-1"
                            onClick={() => {
                              // Mock download functionality
                              const requestId = request.id || request.UID
                              const link = document.createElement('a');
                              link.href = `data:text/plain;charset=utf-8,Load Sheet for ${requestId}\nTruck: ${request.assignedTruck || 'N/A'}\nDriver: ${request.assignedDriver || 'N/A'}\nLoad: ${request.approvedLoad || 'N/A'}`;
                              link.download = `LoadSheet_${requestId}.txt`;
                              link.click();
                            }}
                          >
                            <Download className="h-3 w-3" />
                            Download
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <div className="text-center py-12">
                <CheckCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-foreground mb-2">No Approved Requests</h3>
                <p className="text-muted-foreground">You don't have any approved load service requests yet.</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}