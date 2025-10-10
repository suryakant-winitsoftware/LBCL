'use client'

import React, { useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { useToast } from '@/components/ui/use-toast'
import { 
  FileText, 
  Shield, 
  Activity, 
  CheckCircle2, 
  XCircle,
  Eye,
  Edit,
  Trash,
  Download,
  RefreshCw
} from 'lucide-react'

// Import our audit and session components
import { AuditTrailViewer } from '@/components/audit/audit-trail-viewer'
import { SessionManager } from '@/components/session/session-manager'
import { auditService } from '@/lib/audit-service'
import { sessionService } from '@/lib/session-service'
import { useAuditTracker } from '@/hooks/use-audit'

export default function AuditTestPage() {
  const [testEntity, setTestEntity] = useState({ id: 'TEST_001', name: 'Test Entity' })
  const [auditResults, setAuditResults] = useState<string[]>([])
  const { toast } = useToast()
  const { trackCreate, trackUpdate, trackDelete, trackView, trackExport } = useAuditTracker()

  // Test audit logging
  const testAuditCreate = async () => {
    try {
      await trackCreate('TestEntity', testEntity.id, testEntity)
      setAuditResults(prev => [...prev, '✅ CREATE audit logged'])
      toast({
        title: 'Success',
        description: 'Create operation tracked in audit trail',
      })
    } catch (error) {
      setAuditResults(prev => [...prev, '❌ CREATE audit failed'])
      toast({
        title: 'Error',
        description: 'Failed to track create operation',
        variant: 'destructive'
      })
    }
  }

  const testAuditUpdate = async () => {
    try {
      const oldData = { ...testEntity }
      const newData = { ...testEntity, name: testEntity.name + ' Updated' }
      await trackUpdate('TestEntity', testEntity.id, newData, oldData)
      setTestEntity(newData)
      setAuditResults(prev => [...prev, '✅ UPDATE audit logged'])
      toast({
        title: 'Success',
        description: 'Update operation tracked in audit trail',
      })
    } catch (error) {
      setAuditResults(prev => [...prev, '❌ UPDATE audit failed'])
      toast({
        title: 'Error',
        description: 'Failed to track update operation',
        variant: 'destructive'
      })
    }
  }

  const testAuditDelete = async () => {
    try {
      await trackDelete('TestEntity', testEntity.id, testEntity)
      setAuditResults(prev => [...prev, '✅ DELETE audit logged'])
      toast({
        title: 'Success',
        description: 'Delete operation tracked in audit trail',
      })
    } catch (error) {
      setAuditResults(prev => [...prev, '❌ DELETE audit failed'])
      toast({
        title: 'Error',
        description: 'Failed to track delete operation',
        variant: 'destructive'
      })
    }
  }

  const testAuditView = async () => {
    try {
      await trackView('AuditTestPage', { viewedEntity: testEntity.id })
      setAuditResults(prev => [...prev, '✅ VIEW audit logged'])
      toast({
        title: 'Success',
        description: 'View operation tracked in audit trail',
      })
    } catch (error) {
      setAuditResults(prev => [...prev, '❌ VIEW audit failed'])
    }
  }

  const testAuditExport = async () => {
    try {
      await trackExport('TestEntity', 'CSV', 100, { test: true })
      setAuditResults(prev => [...prev, '✅ EXPORT audit logged'])
      toast({
        title: 'Success',
        description: 'Export operation tracked in audit trail',
      })
    } catch (error) {
      setAuditResults(prev => [...prev, '❌ EXPORT audit failed'])
    }
  }

  const testSecurityEvent = async () => {
    try {
      await auditService.trackSecurityEvent({
        eventType: 'LOGIN',
        ipAddress: '127.0.0.1',
        userAgent: navigator.userAgent,
        details: { test: true, timestamp: new Date().toISOString() }
      })
      setAuditResults(prev => [...prev, '✅ SECURITY event logged'])
      toast({
        title: 'Success',
        description: 'Security event tracked in audit trail',
      })
    } catch (error) {
      setAuditResults(prev => [...prev, '❌ SECURITY event failed'])
    }
  }

  // Test session management
  const testSessionInfo = () => {
    const session = sessionService.getCurrentSession()
    if (session) {
      toast({
        title: 'Current Session',
        description: (
          <div className="mt-2 space-y-1 text-sm">
            <div>Session Key: {session.sessionKey.substring(0, 20)}...</div>
            <div>Device: {session.deviceName}</div>
            <div>IP: {session.ipAddress}</div>
            <div>Login Time: {new Date(session.loginTime).toLocaleString()}</div>
          </div>
        ),
      })
    } else {
      toast({
        title: 'No Session',
        description: 'No active session found',
        variant: 'destructive'
      })
    }
  }

  const testSessionActivity = async () => {
    try {
      await sessionService.updateActivity('action')
      toast({
        title: 'Activity Updated',
        description: 'Session activity has been updated',
      })
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to update session activity',
        variant: 'destructive'
      })
    }
  }

  const testSessionStats = async () => {
    const stats = await sessionService.getSessionStats()
    if (stats) {
      toast({
        title: 'Session Statistics',
        description: (
          <div className="mt-2 space-y-1 text-sm">
            <div>Active Sessions: {stats.activeSessions}</div>
            <div>Total Sessions: {stats.totalSessions}</div>
            <div>Unique Users: {stats.uniqueUsers}</div>
            <div>Avg Duration: {Math.round(stats.averageSessionDuration / 60000)}m</div>
          </div>
        ),
      })
    }
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Audit & Session Test Page</h1>
          <p className="text-muted-foreground mt-1">
            Test and verify audit trail and session management functionality
          </p>
        </div>
        <Badge variant="outline" className="gap-1">
          <Activity className="h-3 w-3" />
          Testing Environment
        </Badge>
      </div>

      <Alert>
        <AlertTitle className="flex items-center gap-2">
          <Shield className="h-4 w-4" />
          Testing Instructions
        </AlertTitle>
        <AlertDescription className="mt-2 space-y-2">
          <p>Use this page to test the audit trail and session management features:</p>
          <ul className="list-disc pl-5 space-y-1 text-sm">
            <li>Click the test buttons to trigger different audit events</li>
            <li>Check the Audit Viewer tab to see logged events</li>
            <li>View session information and statistics</li>
            <li>Monitor the console for any errors</li>
          </ul>
        </AlertDescription>
      </Alert>

      <Tabs defaultValue="test" className="space-y-4">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="test">Test Actions</TabsTrigger>
          <TabsTrigger value="audit">Audit Viewer</TabsTrigger>
          <TabsTrigger value="session">Session Manager</TabsTrigger>
        </TabsList>

        <TabsContent value="test" className="space-y-4">
          {/* Audit Trail Tests */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Audit Trail Tests
              </CardTitle>
              <CardDescription>
                Test different types of audit logging
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>Test Entity</Label>
                <div className="flex gap-2">
                  <Input
                    value={testEntity.id}
                    onChange={(e) => setTestEntity({ ...testEntity, id: e.target.value })}
                    placeholder="Entity ID"
                  />
                  <Input
                    value={testEntity.name}
                    onChange={(e) => setTestEntity({ ...testEntity, name: e.target.value })}
                    placeholder="Entity Name"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                <Button onClick={testAuditCreate} variant="outline" className="gap-2">
                  <CheckCircle2 className="h-4 w-4" />
                  Test Create
                </Button>
                <Button onClick={testAuditUpdate} variant="outline" className="gap-2">
                  <Edit className="h-4 w-4" />
                  Test Update
                </Button>
                <Button onClick={testAuditDelete} variant="outline" className="gap-2">
                  <Trash className="h-4 w-4" />
                  Test Delete
                </Button>
                <Button onClick={testAuditView} variant="outline" className="gap-2">
                  <Eye className="h-4 w-4" />
                  Test View
                </Button>
                <Button onClick={testAuditExport} variant="outline" className="gap-2">
                  <Download className="h-4 w-4" />
                  Test Export
                </Button>
                <Button onClick={testSecurityEvent} variant="outline" className="gap-2">
                  <Shield className="h-4 w-4" />
                  Test Security
                </Button>
              </div>

              {auditResults.length > 0 && (
                <div className="mt-4 p-4 bg-muted rounded-lg">
                  <h4 className="font-medium mb-2">Test Results:</h4>
                  <div className="space-y-1 text-sm font-mono">
                    {auditResults.map((result, idx) => (
                      <div key={idx}>{result}</div>
                    ))}
                  </div>
                  <Button
                    size="sm"
                    variant="ghost"
                    className="mt-2"
                    onClick={() => setAuditResults([])}
                  >
                    Clear Results
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Session Management Tests */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Shield className="h-5 w-5" />
                Session Management Tests
              </CardTitle>
              <CardDescription>
                Test session tracking and management features
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                <Button onClick={testSessionInfo} variant="outline" className="gap-2">
                  <Activity className="h-4 w-4" />
                  Current Session
                </Button>
                <Button onClick={testSessionActivity} variant="outline" className="gap-2">
                  <RefreshCw className="h-4 w-4" />
                  Update Activity
                </Button>
                <Button onClick={testSessionStats} variant="outline" className="gap-2">
                  <FileText className="h-4 w-4" />
                  Session Stats
                </Button>
              </div>

              <Alert className="mt-4">
                <AlertDescription>
                  Session tracking is automatically active. Move your mouse, click, or type to trigger activity updates.
                  Check the console for session heartbeat logs.
                </AlertDescription>
              </Alert>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="audit">
          <Card>
            <CardHeader>
              <CardTitle>Live Audit Trail</CardTitle>
              <CardDescription>
                View audit events as they are logged
              </CardDescription>
            </CardHeader>
            <CardContent>
              <AuditTrailViewer 
                pageSize={10}
                showFilters={true}
              />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="session">
          <SessionManager />
        </TabsContent>
      </Tabs>

      {/* Debug Info */}
      <Card>
        <CardHeader>
          <CardTitle>Debug Information</CardTitle>
          <CardDescription>
            Technical details for troubleshooting
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-2 text-sm font-mono">
            <div>Audit API URL: {process.env.NEXT_PUBLIC_AUDIT_API_URL || 'Using default'}</div>
            <div>Session Active: {sessionService.isSessionActive() ? 'Yes' : 'No'}</div>
            <div>Session Config: {JSON.stringify(sessionService.getConfig(), null, 2)}</div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}