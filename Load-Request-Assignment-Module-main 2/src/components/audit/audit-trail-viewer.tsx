/**
 * Audit Trail Viewer Component
 * Production-ready component for viewing and filtering audit logs
 */

import React, { useState, useMemo } from 'react'
import { format } from 'date-fns'
import { 
  Calendar,
  Clock,
  FileText,
  Filter,
  Download,
  User,
  Activity,
  ChevronDown,
  ChevronRight,
  Eye,
  Edit,
  Trash,
  Plus,
  LogOut,
  LogIn,
  FileDown
} from 'lucide-react'

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Loader } from '@/components/ui/loader'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible'
import { useAuditTrailPaged } from '@/hooks/use-audit'
import type { AuditTrailEntry, AuditTrailPagingRequest } from '@/types/audit.types'

interface AuditTrailViewerProps {
  entityType?: string
  entityUID?: string
  showFilters?: boolean
  pageSize?: number
}

export function AuditTrailViewer({
  entityType,
  entityUID,
  showFilters = true,
  pageSize = 20
}: AuditTrailViewerProps) {
  const [pageNumber, setPageNumber] = useState(1)
  const [searchText, setSearchText] = useState('')
  const [commandTypeFilter, setCommandTypeFilter] = useState<string>('all')
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set())

  // Build request object
  const request: AuditTrailPagingRequest = useMemo(() => {
    const filterCriterias = []

    if (entityType) {
      filterCriterias.push({
        field: 'linkedItemType',
        operator: 'eq' as const,
        value: entityType
      })
    }

    if (entityUID) {
      filterCriterias.push({
        field: 'linkedItemUID',
        operator: 'eq' as const,
        value: entityUID
      })
    }

    if (searchText) {
      filterCriterias.push({
        field: 'empName',
        operator: 'contains' as const,
        value: searchText
      })
    }

    if (commandTypeFilter && commandTypeFilter !== 'all') {
      filterCriterias.push({
        field: 'commandType',
        operator: 'eq' as const,
        value: commandTypeFilter
      })
    }

    return {
      pageNumber,
      pageSize,
      sortCriterias: [
        {
          field: 'commandDate',
          direction: 'desc'
        }
      ],
      filterCriterias,
      isCountRequired: true
    }
  }, [entityType, entityUID, pageNumber, pageSize, searchText, commandTypeFilter])

  // Fetch data
  const { data, isLoading, error } = useAuditTrailPaged(request)

  // Toggle row expansion
  const toggleRowExpansion = (id: string) => {
    const newExpanded = new Set(expandedRows)
    if (newExpanded.has(id)) {
      newExpanded.delete(id)
    } else {
      newExpanded.add(id)
    }
    setExpandedRows(newExpanded)
  }

  // Get icon for command type
  const getCommandIcon = (commandType: string) => {
    switch (commandType) {
      case 'Insert':
        return <Plus className="h-4 w-4" />
      case 'Update':
        return <Edit className="h-4 w-4" />
      case 'Delete':
        return <Trash className="h-4 w-4" />
      case 'View':
        return <Eye className="h-4 w-4" />
      case 'Export':
        return <FileDown className="h-4 w-4" />
      case 'Login':
        return <LogIn className="h-4 w-4" />
      case 'Logout':
        return <LogOut className="h-4 w-4" />
      default:
        return <Activity className="h-4 w-4" />
    }
  }

  // Get variant for command type badge
  const getCommandVariant = (commandType: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (commandType) {
      case 'Delete':
        return 'destructive'
      case 'Insert':
        return 'default'
      case 'Update':
        return 'secondary'
      default:
        return 'outline'
    }
  }

  // Export audit trail
  const handleExport = async () => {
    if (!data?.data) return

    const csv = [
      ['Date/Time', 'User', 'Action', 'Entity Type', 'Entity ID', 'Changes'].join(','),
      ...data.data.map(entry => [
        format(new Date(entry.commandDate), 'yyyy-MM-dd HH:mm:ss'),
        entry.empName,
        entry.commandType,
        entry.linkedItemType,
        entry.linkedItemUID,
        entry.hasChanges ? `${entry.changeData?.length || 0} changes` : 'No changes'
      ].join(','))
    ].join('\n')

    const blob = new Blob([csv], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `audit-trail-${format(new Date(), 'yyyy-MM-dd-HHmmss')}.csv`
    a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Audit Trail
          </CardTitle>
          {data?.data && data.data.length > 0 && (
            <Button
              variant="outline"
              size="sm"
              onClick={handleExport}
              className="gap-2"
            >
              <Download className="h-4 w-4" />
              Export
            </Button>
          )}
        </div>
      </CardHeader>
      <CardContent>
        {/* Filters */}
        {showFilters && (
          <div className="mb-6 space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <Label htmlFor="search">Search by User</Label>
                <Input
                  id="search"
                  placeholder="Enter user name..."
                  value={searchText}
                  onChange={(e) => setSearchText(e.target.value)}
                  className="mt-1"
                />
              </div>
              <div>
                <Label htmlFor="commandType">Action Type</Label>
                <Select
                  value={commandTypeFilter}
                  onValueChange={setCommandTypeFilter}
                >
                  <SelectTrigger id="commandType" className="mt-1">
                    <SelectValue placeholder="All Actions" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Actions</SelectItem>
                    <SelectItem value="Insert">Insert</SelectItem>
                    <SelectItem value="Update">Update</SelectItem>
                    <SelectItem value="Delete">Delete</SelectItem>
                    <SelectItem value="View">View</SelectItem>
                    <SelectItem value="Export">Export</SelectItem>
                    <SelectItem value="Login">Login</SelectItem>
                    <SelectItem value="Logout">Logout</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="flex items-end">
                <Button
                  variant="outline"
                  onClick={() => {
                    setSearchText('')
                    setCommandTypeFilter('all')
                    setPageNumber(1)
                  }}
                  className="w-full"
                >
                  <Filter className="h-4 w-4 mr-2" />
                  Clear Filters
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* Loading State */}
        {isLoading && (
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-16 w-full" />
            ))}
          </div>
        )}

        {/* Error State */}
        {error && (
          <Alert variant="destructive">
            <AlertDescription>
              Failed to load audit trail. Please try again later.
            </AlertDescription>
          </Alert>
        )}

        {/* Data Table */}
        {!isLoading && !error && data?.data && (
          <>
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-[50px]"></TableHead>
                    <TableHead>Date/Time</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Action</TableHead>
                    <TableHead>Entity</TableHead>
                    <TableHead>Changes</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.data.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                        No audit trail entries found
                      </TableCell>
                    </TableRow>
                  ) : (
                    data.data.map((entry) => (
                      <React.Fragment key={entry.id || entry.uid}>
                        <TableRow 
                          className="cursor-pointer hover:bg-muted/50"
                          onClick={() => toggleRowExpansion(entry.id?.toString() || entry.uid || '')}
                        >
                          <TableCell>
                            {entry.hasChanges && entry.changeData && entry.changeData.length > 0 && (
                              expandedRows.has(entry.id?.toString() || entry.uid || '') ? (
                                <ChevronDown className="h-4 w-4" />
                              ) : (
                                <ChevronRight className="h-4 w-4" />
                              )
                            )}
                          </TableCell>
                          <TableCell className="font-mono text-sm">
                            <div className="flex items-center gap-2">
                              <Clock className="h-4 w-4 text-muted-foreground" />
                              {format(new Date(entry.commandDate), 'MMM dd, yyyy HH:mm')}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <User className="h-4 w-4 text-muted-foreground" />
                              {entry.empName}
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge variant={getCommandVariant(entry.commandType)} className="gap-1">
                              {getCommandIcon(entry.commandType)}
                              {entry.commandType}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm">
                              <div className="font-medium">{entry.linkedItemType}</div>
                              <div className="text-muted-foreground text-xs">{entry.linkedItemUID}</div>
                            </div>
                          </TableCell>
                          <TableCell>
                            {entry.hasChanges && entry.changeData ? (
                              <Badge variant="outline">
                                {entry.changeData.length} changes
                              </Badge>
                            ) : (
                              <span className="text-muted-foreground text-sm">No changes</span>
                            )}
                          </TableCell>
                        </TableRow>
                        {/* Expandable change details */}
                        {entry.hasChanges && entry.changeData && entry.changeData.length > 0 && (
                          <TableRow>
                            <TableCell colSpan={6} className="p-0">
                              <Collapsible
                                open={expandedRows.has(entry.id?.toString() || entry.uid || '')}
                              >
                                <CollapsibleContent>
                                  <div className="p-4 bg-muted/30">
                                    <h4 className="text-sm font-medium mb-2">Change Details:</h4>
                                    <div className="space-y-2">
                                      {entry.changeData.map((change, idx) => (
                                        <div key={idx} className="text-sm grid grid-cols-3 gap-2">
                                          <div className="font-medium">{change.field}:</div>
                                          <div className="text-muted-foreground">
                                            {JSON.stringify(change.oldValue)}
                                          </div>
                                          <div className="text-foreground">
                                            → {JSON.stringify(change.newValue)}
                                          </div>
                                        </div>
                                      ))}
                                    </div>
                                    {entry.docNo && (
                                      <div className="mt-2 text-sm">
                                        <span className="font-medium">Document:</span> {entry.docNo}
                                      </div>
                                    )}
                                  </div>
                                </CollapsibleContent>
                              </Collapsible>
                            </TableCell>
                          </TableRow>
                        )}
                      </React.Fragment>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>

            {/* Pagination */}
            {data.totalPages && data.totalPages > 1 && (
              <div className="flex items-center justify-between mt-4">
                <div className="text-sm text-muted-foreground">
                  Showing {((pageNumber - 1) * pageSize) + 1} to{' '}
                  {Math.min(pageNumber * pageSize, data.totalCount || 0)} of{' '}
                  {data.totalCount || 0} entries
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPageNumber(p => Math.max(1, p - 1))}
                    disabled={pageNumber === 1}
                  >
                    Previous
                  </Button>
                  <div className="flex items-center gap-1">
                    {[...Array(Math.min(5, data.totalPages))].map((_, i) => {
                      const page = i + 1
                      return (
                        <Button
                          key={page}
                          variant={page === pageNumber ? 'default' : 'outline'}
                          size="sm"
                          onClick={() => setPageNumber(page)}
                        >
                          {page}
                        </Button>
                      )
                    })}
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPageNumber(p => Math.min(data.totalPages || 1, p + 1))}
                    disabled={pageNumber === data.totalPages}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </>
        )}
      </CardContent>
    </Card>
  )
}

// Export mini viewer for inline use
export function AuditTrailMiniViewer({ 
  entityType, 
  entityUID 
}: { 
  entityType: string
  entityUID: string 
}) {
  return (
    <AuditTrailViewer
      entityType={entityType}
      entityUID={entityUID}
      showFilters={false}
      pageSize={5}
    />
  )
}