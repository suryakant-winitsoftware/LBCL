"use client"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"
import { useRouter } from "next/navigation"
import { Plus, Clock, CheckCircle, ArrowLeft, History } from "lucide-react"
import { useEffect, useState } from "react"
import { lsrService, LSRStats } from "./services/lsr.service"

export default function LSRPage() {
  const router = useRouter()
  const [stats, setStats] = useState<LSRStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchStats()
  }, [])

  const fetchStats = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await lsrService.getStats()
      setStats(data)
    } catch (err) {
      console.error('Error fetching LSR stats:', err)
      setError('Failed to load statistics. Using default values.')
      // Set default stats on error
      setStats({
        totalRequests: 0,
        pendingRequests: 0,
        approvedRequests: 0,
        rejectedRequests: 0,
      })
    } finally {
      setLoading(false)
    }
  }

  const handleCreateRequest = () => {
    router.push("/load-management/lsr/create")
  }

  const handlePendingRequests = () => {
    router.push("/load-management/lsr/pending")
  }

  const handleApprovedRequests = () => {
    router.push("/load-management/lsr/approved")
  }

  const handleRequestHistory = () => {
    router.push("/load-management/lsr/history")
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
        <h1 className="text-2xl font-semibold mb-2">LSR - Van Sales Rep Portal</h1>
        <p className="text-muted-foreground">Initiate daily load requests, review system recommendations, and adjust buffer</p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-destructive/10 border border-destructive/20 rounded-lg text-destructive text-sm">
          {error}
        </div>
      )}

      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[1, 2, 3, 4].map((i) => (
            <Card key={i} className="hover:shadow-md transition-shadow">
              <CardHeader className="pb-4">
                <div className="flex items-center gap-3">
                  <Skeleton className="h-9 w-9 rounded-lg" />
                  <Skeleton className="h-6 w-32" />
                </div>
              </CardHeader>
              <CardContent>
                <Skeleton className="h-4 w-full mb-2" />
                <Skeleton className="h-4 w-3/4 mb-4" />
                <Skeleton className="h-10 w-full rounded-md" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-muted">
                <Plus className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <CardTitle className="text-lg">Create New Request</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              Submit a new load service request for vehicle scheduling and route assignment
            </CardDescription>
            <Button 
              onClick={handleCreateRequest}
              variant="outline"
              className="w-full"
            >
              Create Request
            </Button>
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="pb-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-muted">
                  <Clock className="h-5 w-5 text-muted-foreground" />
                </div>
                <div>
                  <CardTitle className="text-lg">Pending Requests</CardTitle>
                </div>
              </div>
              {stats && stats.pendingRequests > 0 && (
                <Badge variant="secondary" className="ml-2">
                  {stats.pendingRequests}
                </Badge>
              )}
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              View and track load service requests that are pending approval or processing
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
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-muted">
                  <CheckCircle className="h-5 w-5 text-muted-foreground" />
                </div>
                <div>
                  <CardTitle className="text-lg">Approved Requests</CardTitle>
                </div>
              </div>
              {stats && stats.approvedRequests > 0 && (
                <Badge variant="default" className="ml-2">
                  {stats.approvedRequests}
                </Badge>
              )}
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              Browse approved load service requests and their execution details
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
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-muted">
                  <History className="h-5 w-5 text-muted-foreground" />
                </div>
                <div>
                  <CardTitle className="text-lg">Request History</CardTitle>
                </div>
              </div>
              {stats && stats.totalRequests > 0 && (
                <Badge variant="outline" className="ml-2">
                  {stats.totalRequests}
                </Badge>
              )}
            </div>
          </CardHeader>
          <CardContent>
            <CardDescription className="text-sm text-muted-foreground mb-4">
              View complete history of all your load service requests with detailed status
            </CardDescription>
            <Button
              onClick={handleRequestHistory}
              variant="outline"
              className="w-full"
            >
              View History
            </Button>
          </CardContent>
        </Card>
        </div>
      )}
    </div>
  )
}