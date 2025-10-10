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
import { Progress } from "@/components/ui/progress"
import { useRouter } from "next/navigation"
import { ArrowLeft, CheckCircle, Download, Eye, Truck, MapPin, User, CalendarCheck, Package, Weight, Users } from "lucide-react"

export default function ApprovedRequestsPage() {
  const router = useRouter()

  const handleBack = () => {
    router.push("/load-management/logistics-approval")
  }

  // Mock data for approved requests
  const approvedRequests = [
    {
      id: "LSR-2024-001",
      route: "Route A - Downtown",
      requestDate: "2024-01-14",
      approvalDate: "2024-01-14",
      requestedBy: "Alice Johnson",
      depot: "Depot North",
      approvedLoad: "800 kg",
      assignedTruck: "TRK-001",
      assignedDriver: "John Smith",
      assignedHelper: "Mike Davis",
      status: "In Transit",
      loadSheetId: "LS-2024-001",
      estimatedDelivery: "2024-01-14 16:00",
      completionPercentage: 75
    },
    {
      id: "LSR-2024-002", 
      route: "Route B - Suburbs",
      requestDate: "2024-01-13",
      approvalDate: "2024-01-13",
      requestedBy: "Bob Smith",
      depot: "Depot South",
      approvedLoad: "650 kg",
      assignedTruck: "TRK-003",
      assignedDriver: "Sarah Wilson",
      assignedHelper: "Tom Brown",
      status: "Delivered",
      loadSheetId: "LS-2024-002",
      actualDelivery: "2024-01-13 15:30",
      completionPercentage: 100
    },
    {
      id: "LSR-2024-003",
      route: "Route C - Industrial",
      requestDate: "2024-01-12",
      approvalDate: "2024-01-12",
      requestedBy: "Carol Wilson",
      depot: "Depot East",
      approvedLoad: "920 kg",
      assignedTruck: "TRK-007",
      assignedDriver: "David Lee",
      assignedHelper: "James Clark",
      status: "Delivered",
      loadSheetId: "LS-2024-003",
      actualDelivery: "2024-01-12 17:45",
      completionPercentage: 100
    }
  ]

  const getStatusColor = (status: string) => {
    switch (status) {
      case "In Transit": return "secondary"
      case "Delivered": return "default"
      case "Load Sheet Generated": return "outline"
      default: return "secondary"
    }
  }

  const getDepotColor = (depot: string) => {
    const variants: ("outline" | "secondary" | "default")[] = ["outline", "secondary", "default"]
    return variants[depot.length % variants.length]
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <div className="flex items-center gap-4 mb-4">
          <Button
            variant="outline"
            size="sm"
            onClick={handleBack}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Logistics Approval
          </Button>
        </div>
        <div className="flex items-center gap-3 mb-2">
          <div className="p-3 rounded-lg bg-muted">
            <CheckCircle className="h-6 w-6 text-muted-foreground" />
          </div>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Approved Load Service Requests</h1>
            <p className="text-muted-foreground mt-1">Track completed approvals and their execution status</p>
          </div>
        </div>
      </div>

      <div className="max-w-full">
        <Card className="border shadow-sm">
          <CardContent className="p-0">
            {approvedRequests.length > 0 ? (
              <div className="w-full">
                <Table>
                  <TableHeader>
                    <TableRow className="bg-muted/50">
                      <TableHead className="font-semibold min-w-[140px]">
                        <div className="flex items-center gap-2">
                          <Package className="h-4 w-4" />
                          Request ID
                        </div>
                      </TableHead>
                      <TableHead className="font-semibold hidden sm:table-cell">Route</TableHead>
                      <TableHead className="font-semibold hidden md:table-cell">
                        <div className="flex items-center gap-2">
                          <User className="h-4 w-4" />
                          Requested By
                        </div>
                      </TableHead>
                      <TableHead className="font-semibold hidden lg:table-cell">Depot</TableHead>
                      <TableHead className="font-semibold hidden lg:table-cell">
                        <div className="flex items-center gap-2">
                          <CalendarCheck className="h-4 w-4" />
                          Approval Date
                        </div>
                      </TableHead>
                      <TableHead className="font-semibold w-20 hidden md:table-cell">
                        <div className="flex items-center gap-2">
                          <Weight className="h-4 w-4" />
                          Load
                        </div>
                      </TableHead>
                      <TableHead className="font-semibold hidden xl:table-cell">
                        <div className="flex items-center gap-2">
                          <Truck className="h-4 w-4" />
                          Resources
                        </div>
                      </TableHead>
                      <TableHead className="font-semibold w-20">Status</TableHead>
                      <TableHead className="font-semibold w-24 hidden sm:table-cell">Progress</TableHead>
                      <TableHead className="text-right font-semibold w-24">Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {approvedRequests.map((request, index) => (
                      <TableRow 
                        key={request.id}
                        className={`hover:bg-muted/30 transition-colors ${
                          index % 2 === 0 ? 'bg-background' : 'bg-muted/10'
                        }`}
                      >
                        <TableCell className="font-medium">
                          <div className="flex items-center gap-2">
                            <div className="p-1.5 rounded bg-green-100 dark:bg-green-900/30">
                              <CheckCircle className="h-3 w-3 text-green-600 dark:text-green-400" />
                            </div>
                            <div>
                              <span className="font-semibold text-foreground">{request.id}</span>
                              <div className="text-xs text-muted-foreground">{request.loadSheetId}</div>
                              <div className="text-xs text-muted-foreground sm:hidden">{request.route}</div>
                              <div className="text-xs text-muted-foreground md:hidden">{request.requestedBy}</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell className="text-muted-foreground hidden sm:table-cell">
                          {request.route}
                        </TableCell>
                        <TableCell className="text-muted-foreground hidden md:table-cell">
                          {request.requestedBy}
                        </TableCell>
                        <TableCell className="hidden lg:table-cell">
                          <Badge variant={getDepotColor(request.depot)} className="text-xs">
                            {request.depot}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-muted-foreground text-sm hidden lg:table-cell">
                          {request.approvalDate}
                        </TableCell>
                        <TableCell className="font-medium text-sm hidden md:table-cell">
                          {request.approvedLoad}
                        </TableCell>
                        <TableCell className="hidden xl:table-cell">
                          <div className="flex flex-col gap-1">
                            <div className="flex items-center gap-1">
                              <Truck className="h-3 w-3 text-muted-foreground" />
                              <span className="text-xs font-medium">{request.assignedTruck}</span>
                            </div>
                            <div className="flex items-center gap-1">
                              <User className="h-3 w-3 text-muted-foreground" />
                              <span className="text-xs text-muted-foreground">{request.assignedDriver}</span>
                            </div>
                            <div className="flex items-center gap-1">
                              <Users className="h-3 w-3 text-muted-foreground" />
                              <span className="text-xs text-muted-foreground">{request.assignedHelper}</span>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge variant={getStatusColor(request.status)} className="text-xs">
                            {request.status}
                          </Badge>
                          <div className="text-xs text-muted-foreground md:hidden mt-1">
                            {request.approvedLoad}
                          </div>
                        </TableCell>
                        <TableCell className="hidden sm:table-cell">
                          <div className="flex items-center gap-2 min-w-[80px]">
                            <Progress value={request.completionPercentage} className="h-2 flex-1" />
                            <span className="text-xs font-medium text-muted-foreground">
                              {request.completionPercentage}%
                            </span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="flex justify-end gap-1">
                            <Button 
                              variant="outline" 
                              size="sm" 
                              className="h-7 w-7 p-0"
                              onClick={() => router.push(`/load-management/logistics-approval/view/${request.id}`)}
                            >
                              <Eye className="h-3 w-3" />
                            </Button>
                            <Button 
                              variant="outline" 
                              size="sm" 
                              className="h-7 w-7 p-0 hidden sm:inline-flex"
                              onClick={() => {
                                // Mock download functionality
                                const link = document.createElement('a');
                                link.href = `data:text/plain;charset=utf-8,Load Sheet for ${request.id}\nTruck: ${request.assignedTruck}\nDriver: ${request.assignedDriver}\nHelper: ${request.assignedHelper}\nLoad: ${request.approvedLoad}\nRoute: ${request.route}`;
                                link.download = `LoadSheet_${request.id}.txt`;
                                link.click();
                              }}
                            >
                              <Download className="h-3 w-3" />
                            </Button>
                            {request.status === "In Transit" && (
                              <Button 
                                variant="outline" 
                                size="sm" 
                                className="h-7 w-7 p-0 hidden md:inline-flex"
                                onClick={() => router.push(`/load-management/logistics-approval/track/${request.id}`)}
                              >
                                <MapPin className="h-3 w-3" />
                              </Button>
                            )}
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            ) : (
              <div className="text-center py-12">
                <CheckCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-foreground mb-2">No Approved Requests</h3>
                <p className="text-muted-foreground">There are no approved load service requests to display.</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}