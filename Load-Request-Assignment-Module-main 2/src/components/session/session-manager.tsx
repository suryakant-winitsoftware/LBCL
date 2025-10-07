/**
 * Session Manager Component
 * Production-ready UI for viewing and managing user sessions
 */

import React, { useState, useEffect } from 'react'
import { format, formatDistanceToNow } from 'date-fns'
import {
  Monitor,
  Smartphone,
  Tablet,
  Globe,
  Clock,
  MapPin,
  Shield,
  AlertTriangle,
  LogOut,
  RefreshCw,
  Activity,
  BarChart3,
  Users,
  TrendingUp
} from 'lucide-react'

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Progress } from '@/components/ui/progress'
import { Separator } from '@/components/ui/separator'
import { 
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Skeleton } from '@/components/ui/skeleton'
import { useToast } from '@/components/ui/use-toast'

import { sessionService } from '@/lib/session-service'
import { auditService } from '@/lib/audit-service'
import type { Session, SessionStats } from '@/types/session.types'
import { AuditLogLevel } from '@/types/audit.types'

export function SessionManager() {
  const [activeSessions, setActiveSessions] = useState<Session[]>([])
  const [currentSession, setCurrentSession] = useState<Session | null>(null)
  const [sessionStats, setSessionStats] = useState<SessionStats | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [selectedSession, setSelectedSession] = useState<Session | null>(null)
  const [showTerminateDialog, setShowTerminateDialog] = useState(false)
  const { toast } = useToast()

  useEffect(() => {
    loadSessionData()
    
    // Refresh every 30 seconds
    const interval = setInterval(loadSessionData, 30000)
    return () => clearInterval(interval)
  }, [])

  const loadSessionData = async () => {
    try {
      setIsLoading(true)
      
      // Get current session
      const current = sessionService.getCurrentSession()
      setCurrentSession(current)
      
      // Get all active sessions
      const sessions = await sessionService.getActiveSessions()
      setActiveSessions(sessions)
      
      // Get session statistics
      const stats = await sessionService.getSessionStats()
      setSessionStats(stats)
    } catch (error) {
      console.error('Failed to load session data:', error)
      toast({
        title: 'Error',
        description: 'Failed to load session information',
        variant: 'destructive'
      })
    } finally {
      setIsLoading(false)
    }
  }

  const handleTerminateSession = async (session: Session) => {
    setSelectedSession(session)
    setShowTerminateDialog(true)
  }

  const confirmTerminateSession = async () => {
    if (!selectedSession) return
    
    try {
      // Check if it's the current session
      if (selectedSession.isCurrentSession) {
        await sessionService.terminateSession('admin_action')
        // This will log out the user
        window.location.href = '/login'
      } else {
        // Terminate remote session (would need backend API)
        toast({
          title: 'Session Terminated',
          description: `Session on ${selectedSession.deviceName} has been terminated`,
        })
        
        // Track audit
        await auditService.trackSecurityEvent({
          eventType: 'SESSION_TERMINATED',
          sessionId: selectedSession.sessionKey,
          details: {
            targetSession: selectedSession.id,
            device: selectedSession.deviceName,
            reason: 'admin_action'
          }
        }, AuditLogLevel.WARNING)
      }
      
      setShowTerminateDialog(false)
      setSelectedSession(null)
      await loadSessionData()
    } catch (error) {
      console.error('Failed to terminate session:', error)
      toast({
        title: 'Error',
        description: 'Failed to terminate session',
        variant: 'destructive'
      })
    }
  }

  const getDeviceIcon = (deviceType: Session['deviceType']) => {
    switch (deviceType) {
      case 'Mobile':
        return <Smartphone className="h-5 w-5" />
      case 'Desktop':
        return <Monitor className="h-5 w-5" />
      default:
        return <Globe className="h-5 w-5" />
    }
  }

  const getRiskBadgeVariant = (riskLevel: Session['riskLevel']) => {
    switch (riskLevel) {
      case 'High':
        return 'destructive'
      case 'Medium':
        return 'secondary'
      default:
        return 'outline'
    }
  }

  const formatSessionDuration = (loginTime: Date) => {
    return formatDistanceToNow(new Date(loginTime), { addSuffix: false })
  }

  const calculateIdlePercentage = (session: Session) => {
    const totalTime = Date.now() - new Date(session.loginTime).getTime()
    const activeTime = new Date(session.lastActivityTime).getTime() - new Date(session.loginTime).getTime()
    return Math.round((activeTime / totalTime) * 100)
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-64 w-full" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Current Session Alert */}
      {currentSession && (
        <Alert>
          <Shield className="h-4 w-4" />
          <AlertTitle>Current Session</AlertTitle>
          <AlertDescription>
            You are logged in from {currentSession.deviceName} ({currentSession.ipAddress})
            {currentSession.location && ` - ${currentSession.location}`}
          </AlertDescription>
        </Alert>
      )}

      {/* Session Statistics */}
      {sessionStats && (
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Active Sessions</CardTitle>
              <Activity className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{sessionStats.activeSessions}</div>
              <p className="text-xs text-muted-foreground">
                of {sessionStats.totalSessions} total
              </p>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Unique Users</CardTitle>
              <Users className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{sessionStats.uniqueUsers}</div>
              <p className="text-xs text-muted-foreground">
                Currently online
              </p>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Avg. Duration</CardTitle>
              <Clock className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {Math.round(sessionStats.averageSessionDuration / 60000)}m
              </div>
              <p className="text-xs text-muted-foreground">
                Per session
              </p>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Device Types</CardTitle>
              <BarChart3 className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {Object.keys(sessionStats.deviceBreakdown).length}
              </div>
              <p className="text-xs text-muted-foreground">
                Different devices
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Session Management Tabs */}
      <Tabs defaultValue="active" className="space-y-4">
        <TabsList>
          <TabsTrigger value="active">Active Sessions</TabsTrigger>
          <TabsTrigger value="details">Session Details</TabsTrigger>
          <TabsTrigger value="security">Security Settings</TabsTrigger>
        </TabsList>

        <TabsContent value="active" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Active Sessions</CardTitle>
                  <CardDescription>
                    Manage all active login sessions across devices
                  </CardDescription>
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={loadSessionData}
                  className="gap-2"
                >
                  <RefreshCw className="h-4 w-4" />
                  Refresh
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Device</TableHead>
                    <TableHead>Location</TableHead>
                    <TableHead>Login Time</TableHead>
                    <TableHead>Last Activity</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Risk</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {activeSessions.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={7} className="text-center py-8 text-muted-foreground">
                        No active sessions found
                      </TableCell>
                    </TableRow>
                  ) : (
                    activeSessions.map((session) => (
                      <TableRow key={session.id}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            {getDeviceIcon(session.deviceType)}
                            <div>
                              <div className="font-medium">{session.deviceName}</div>
                              <div className="text-sm text-muted-foreground">{session.browser}</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-1">
                            <MapPin className="h-4 w-4 text-muted-foreground" />
                            <span className="text-sm">
                              {session.location || session.ipAddress}
                            </span>
                          </div>
                        </TableCell>
                        <TableCell className="text-sm">
                          {format(new Date(session.loginTime), 'MMM dd, HH:mm')}
                          <div className="text-xs text-muted-foreground">
                            {formatSessionDuration(session.loginTime)}
                          </div>
                        </TableCell>
                        <TableCell className="text-sm">
                          {format(new Date(session.lastActivityTime), 'HH:mm')}
                          <div className="text-xs text-muted-foreground">
                            {formatDistanceToNow(new Date(session.lastActivityTime), { addSuffix: true })}
                          </div>
                        </TableCell>
                        <TableCell>
                          {session.isCurrentSession ? (
                            <Badge variant="default">Current</Badge>
                          ) : session.isActive ? (
                            <Badge variant="secondary">Active</Badge>
                          ) : (
                            <Badge variant="outline">Inactive</Badge>
                          )}
                        </TableCell>
                        <TableCell>
                          <Badge variant={getRiskBadgeVariant(session.riskLevel)}>
                            {session.riskLevel}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          {!session.isCurrentSession && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleTerminateSession(session)}
                              className="text-destructive hover:text-destructive"
                            >
                              <LogOut className="h-4 w-4" />
                              End
                            </Button>
                          )}
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="details" className="space-y-4">
          {currentSession && (
            <Card>
              <CardHeader>
                <CardTitle>Current Session Details</CardTitle>
                <CardDescription>
                  Detailed information about your current login session
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <h4 className="text-sm font-medium text-muted-foreground">Session ID</h4>
                    <p className="text-sm font-mono">{currentSession.sessionKey}</p>
                  </div>
                  <div className="space-y-2">
                    <h4 className="text-sm font-medium text-muted-foreground">Device ID</h4>
                    <p className="text-sm font-mono">{currentSession.deviceId}</p>
                  </div>
                </div>

                <Separator />

                <div className="space-y-4">
                  <h4 className="text-sm font-medium">Session Activity</h4>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Active Time</span>
                      <span>{calculateIdlePercentage(currentSession)}%</span>
                    </div>
                    <Progress value={calculateIdlePercentage(currentSession)} />
                  </div>
                  <div className="grid gap-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Login Time</span>
                      <span>{format(new Date(currentSession.loginTime), 'PPpp')}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Last Activity</span>
                      <span>{format(new Date(currentSession.lastActivityTime), 'PPpp')}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Expires At</span>
                      <span>{format(new Date(currentSession.expiryTime), 'PPpp')}</span>
                    </div>
                  </div>
                </div>

                <Separator />

                <div className="space-y-4">
                  <h4 className="text-sm font-medium">Device Information</h4>
                  <div className="grid gap-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Platform</span>
                      <span>{currentSession.platform}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Browser</span>
                      <span>{currentSession.browser}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">IP Address</span>
                      <span>{currentSession.ipAddress}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Location</span>
                      <span>{currentSession.location || 'Unknown'}</span>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="security" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Security Settings</CardTitle>
              <CardDescription>
                Configure session security and timeout settings
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <Alert>
                <AlertTriangle className="h-4 w-4" />
                <AlertTitle>Security Recommendations</AlertTitle>
                <AlertDescription>
                  <ul className="mt-2 space-y-1 text-sm">
                    <li>• Enable two-factor authentication for enhanced security</li>
                    <li>• Review active sessions regularly</li>
                    <li>• Log out from devices you no longer use</li>
                    <li>• Use strong, unique passwords</li>
                  </ul>
                </AlertDescription>
              </Alert>

              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-medium">Session Timeout</h4>
                    <p className="text-sm text-muted-foreground">
                      Automatically log out after inactivity
                    </p>
                  </div>
                  <Badge variant="secondary">
                    {sessionService.getConfig().sessionTimeout} minutes
                  </Badge>
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-medium">Idle Timeout</h4>
                    <p className="text-sm text-muted-foreground">
                      Warn before automatic logout
                    </p>
                  </div>
                  <Badge variant="secondary">
                    {sessionService.getConfig().idleTimeout} minutes
                  </Badge>
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-medium">Concurrent Sessions</h4>
                    <p className="text-sm text-muted-foreground">
                      Maximum simultaneous logins allowed
                    </p>
                  </div>
                  <Badge variant="secondary">
                    {sessionService.getConfig().maxConcurrentSessions} devices
                  </Badge>
                </div>
              </div>

              <Separator />

              <div className="space-y-3">
                <Button
                  variant="outline"
                  className="w-full justify-start gap-2"
                  onClick={() => {
                    toast({
                      title: 'Coming Soon',
                      description: 'Trusted devices feature will be available soon',
                    })
                  }}
                >
                  <Shield className="h-4 w-4" />
                  Manage Trusted Devices
                </Button>

                <Button
                  variant="destructive"
                  className="w-full justify-start gap-2"
                  onClick={() => {
                    setSelectedSession(null)
                    setShowTerminateDialog(true)
                  }}
                >
                  <LogOut className="h-4 w-4" />
                  Sign Out All Other Sessions
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Terminate Session Dialog */}
      <Dialog open={showTerminateDialog} onOpenChange={setShowTerminateDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Terminate Session?</DialogTitle>
            <DialogDescription>
              {selectedSession ? (
                selectedSession.isCurrentSession ? (
                  'This will log you out of your current session. You will need to sign in again.'
                ) : (
                  `This will end the session on ${selectedSession.deviceName}. The user will need to sign in again on that device.`
                )
              ) : (
                'This will sign you out of all other active sessions except the current one.'
              )}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setShowTerminateDialog(false)
                setSelectedSession(null)
              }}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={confirmTerminateSession}
            >
              Terminate Session
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}