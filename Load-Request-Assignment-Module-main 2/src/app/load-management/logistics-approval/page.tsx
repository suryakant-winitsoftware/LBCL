"use client"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useRouter } from "next/navigation"
import { Inbox, Clock, CheckCircle, ArrowLeft, History } from "lucide-react"

export default function LogisticsApprovalPage() {
  const router = useRouter()

  const handleIncomingRequests = () => {
    router.push("/load-management/logistics-approval/incoming")
  }

  const handlePendingRequests = () => {
    router.push("/load-management/logistics-approval/pending")
  }

  const handleApprovedRequests = () => {
    router.push("/load-management/logistics-approval/approved")
  }

  const handleApprovalHistory = () => {
    router.push("/load-management/logistics-approval/history")
  }

  const handleBack = () => {
    router.push("/load-management")
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
            Back to Load Management
          </Button>
        </div>
        <h1 className="text-2xl font-semibold mb-2">Logistics Approval Agent</h1>
        <p className="text-muted-foreground">Review and approve load service requests from various departments</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-muted">
                <Inbox className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <CardTitle className="text-lg">Incoming Requests</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              Review new load service requests coming from different departments and teams
            </CardDescription>
            <Button 
              onClick={handleIncomingRequests}
              variant="outline"
              className="w-full"
            >
              Review Incoming
            </Button>
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-muted">
                <Clock className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <CardTitle className="text-lg">Pending Requests</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              Manage requests that are currently under review and require further action
            </CardDescription>
            <Button 
              onClick={handlePendingRequests}
              variant="outline"
              className="w-full"
            >
              View Pending
            </Button>
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-muted">
                <CheckCircle className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <CardTitle className="text-lg">Approved Requests</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              View history of approved requests and track their implementation status
            </CardDescription>
            <Button 
              onClick={handleApprovedRequests}
              variant="outline"
              className="w-full"
            >
              View Approved
            </Button>
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-muted">
                <History className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <CardTitle className="text-lg">Approval History</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              View complete history of all processed approvals with detailed records
            </CardDescription>
            <Button 
              onClick={handleApprovalHistory}
              variant="outline"
              className="w-full"
            >
              View History
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}