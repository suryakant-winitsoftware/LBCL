"use client"

import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useRouter } from "next/navigation"
import { ArrowLeft, Clock, Eye, Package, Calendar, Weight, Loader2, RefreshCw } from "lucide-react"
import { useEffect, useState } from "react"
import { lsrService, LSRRequest } from "../services/lsr.service"

export default function PendingLSRPage() {
  const router = useRouter()
  const [pendingRequests, setPendingRequests] = useState<LSRRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchPendingRequests()
  }, [])

  const fetchPendingRequests = async () => {
    try {
      setLoading(true)
      setError(null)
      const response: any = await lsrService.getPendingRequests({})
      // Handle PagedResponse structure: Data.Items or Items
      const requests = response?.Data?.Items || response?.Items || []
      setPendingRequests(requests)
    } catch (err) {
      console.error('Error fetching pending LSR requests:', err)
      setError('Failed to load pending requests')
    } finally {
      setLoading(false)
    }
  }

  const handleBack = () => {
    router.push("/load-management/lsr")
  }

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "High": return "destructive"
      case "Medium": return "secondary"
      case "Low": return "default"
      default: return "secondary"
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Awaiting Approval": return "secondary"
      case "Under Review": return "outline"
      case "Pending Assignment": return "secondary"
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
            onClick={fetchPendingRequests}
            className="flex items-center gap-2"
            disabled={loading}
          >
            <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
        </div>
        <div className="flex items-center gap-3 mb-2">
          <div className="p-3 rounded-lg bg-muted">
            <Clock className="h-6 w-6 text-muted-foreground" />
          </div>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Pending Load Service Requests</h1>
            <p className="text-muted-foreground mt-1">Track your submitted requests awaiting approval or processing</p>
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
              <div className="flex items-center justify-center py-12">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : pendingRequests.length > 0 ? (
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
                    <TableHead className="font-semibold">Status</TableHead>
                    <TableHead className="font-semibold">Priority</TableHead>
                    <TableHead className="font-semibold">
                      <div className="flex items-center gap-2">
                        <Weight className="h-4 w-4" />
                        Est. Load
                      </div>
                    </TableHead>
                    <TableHead className="text-right font-semibold">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {pendingRequests.map((request, index) => (
                    <TableRow 
                      key={request.id}
                      className={`hover:bg-muted/30 transition-colors ${
                        index % 2 === 0 ? 'bg-white' : 'bg-muted/10'
                      }`}
                    >
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-2">
                          <div className="p-1.5 rounded bg-primary/10">
                            <Clock className="h-3 w-3 text-primary" />
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
                      <TableCell>
                        <Badge variant={getStatusColor(request.status || '')} className="text-xs">
                          {request.status || 'Pending'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant={getPriorityColor(request.priority || '')} className="text-xs">
                          {request.priority || 'Medium'} Priority
                        </Badge>
                      </TableCell>
                      <TableCell className="font-medium">
                        {request.estimatedLoad || 'N/A'}
                      </TableCell>
                      <TableCell className="text-right">
                        <Button
                          variant="outline"
                          size="sm"
                          className="flex items-center gap-2"
                          onClick={() => router.push(`/load-management/lsr/view/${request.UID || request.id}`)}
                        >
                          <Eye className="h-4 w-4" />
                          View Details
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <div className="text-center py-12">
                <Clock className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-foreground mb-2">No Pending Requests</h3>
                <p className="text-muted-foreground">You don't have any pending load service requests at the moment.</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}