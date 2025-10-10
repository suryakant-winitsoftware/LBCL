"use client"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useRouter } from "next/navigation"
import { UserCheck, Truck } from "lucide-react"

export default function LoadManagementPage() {
  const router = useRouter()

  const handleLSRClick = () => {
    router.push("/load-management/lsr")
  }

  const handleLogisticsApprovalClick = () => {
    router.push("/load-management/logistics-approval")
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <h1 className="text-2xl font-semibold mb-2">Load Management</h1>
        <p className="text-muted-foreground">Manage load requests and logistics approvals</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-4xl">
        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-muted">
                <Truck className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <CardTitle className="text-lg">LSR (Van Sales Rep)</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              Van Sales Rep initiates daily load requests, reviews system recommendations, adjusts buffer, and submits
            </CardDescription>
            <Button 
              onClick={handleLSRClick}
              variant="outline"
              className="w-full"
            >
              Access LSR Module
            </Button>
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-muted">
                <UserCheck className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <CardTitle className="text-lg">Agent Logistics Officer</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              Reviews requests depot-wise, adjusts & approves load, assigns trucks, drivers & helpers, and finalizes load sheets
            </CardDescription>
            <Button 
              onClick={handleLogisticsApprovalClick}
              variant="outline"
              className="w-full"
            >
              Access Approval Agent
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}