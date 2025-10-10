"use client"

import { useState, useCallback, useMemo } from "react"
import { Search, CheckCircle2, XCircle, Eye, Plus, Edit, Trash2, Check, Download, Shield } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Checkbox } from "@/components/ui/checkbox"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select"
import { Separator } from "@/components/ui/separator"
import { Label } from "@/components/ui/label"
import { 
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import { 
  PermissionMatrix as PermissionMatrixType, 
  PagePermission
} from "@/types/admin.types"

interface BulkPermissionEditorProps {
  matrix: PermissionMatrixType | null
  onUpdate: (pageUIDs: string[], permissions: Partial<PagePermission>) => void
  onCancel: () => void
}

type PermissionType = 'viewAccess' | 'addAccess' | 'editAccess' | 'deleteAccess' | 'approvalAccess' | 'downloadAccess' | 'fullAccess'

interface PermissionOption {
  key: PermissionType
  label: string
  icon: React.ComponentType<{ className?: string }>
  description: string
  color: string
}

const permissionOptions: PermissionOption[] = [
  {
    key: 'viewAccess',
    label: 'View Access',
    icon: Eye,
    description: 'Allows viewing and reading data',
    color: 'text-blue-600'
  },
  {
    key: 'addAccess',
    label: 'Add Access',
    icon: Plus,
    description: 'Allows creating new records',
    color: 'text-green-600'
  },
  {
    key: 'editAccess',
    label: 'Edit Access',
    icon: Edit,
    description: 'Allows modifying existing records',
    color: 'text-yellow-600'
  },
  {
    key: 'deleteAccess',
    label: 'Delete Access',
    icon: Trash2,
    description: 'Allows removing records',
    color: 'text-red-600'
  },
  {
    key: 'approvalAccess',
    label: 'Approval Access',
    icon: Check,
    description: 'Allows approving/rejecting requests',
    color: 'text-purple-600'
  },
  {
    key: 'downloadAccess',
    label: 'Download Access',
    icon: Download,
    description: 'Allows exporting/downloading data',
    color: 'text-indigo-600'
  },
  {
    key: 'fullAccess',
    label: 'Full Access',
    icon: Shield,
    description: 'Grants complete access to all features',
    color: 'text-orange-600'
  }
]

type BulkAction = 'grant' | 'revoke' | 'replace'

export function BulkPermissionEditor({ matrix, onUpdate, onCancel }: BulkPermissionEditorProps) {
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedPages, setSelectedPages] = useState<Set<string>>(new Set())
  const [bulkAction, setBulkAction] = useState<BulkAction>('grant')
  const [selectedPermissions, setSelectedPermissions] = useState<Set<PermissionType>>(new Set())
  const [moduleFilter, setModuleFilter] = useState<string>('all')
  const [permissionFilter, setPermissionFilter] = useState<'all' | 'with-permissions' | 'no-permissions'>('all')

  // Flatten all pages from the matrix
  const allPages = useMemo(() => {
    if (!matrix) return []
    
    return matrix.modules.flatMap(module =>
      module.subModules.flatMap(subModule =>
        subModule.pages.map(page => ({
          ...page,
          moduleName: module.moduleNameEn,
          moduleUid: module.uid,
          subModuleName: subModule.subModuleNameEn,
          subModuleUid: subModule.uid
        }))
      )
    )
  }, [matrix])

  // Get unique modules for filtering
  const modules = useMemo(() => {
    if (!matrix) return []
    return matrix.modules.map(module => ({
      uid: module.uid,
      name: module.moduleNameEn
    }))
  }, [matrix])

  // Filter pages based on search and filters
  const filteredPages = useMemo(() => {
    return allPages.filter(page => {
      // Search filter
      const matchesSearch = searchQuery === "" || 
        page.pageNameEn.toLowerCase().includes(searchQuery.toLowerCase()) ||
        page.subModuleName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        page.moduleName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        page.pageUrl.toLowerCase().includes(searchQuery.toLowerCase())

      // Module filter
      const matchesModule = moduleFilter === 'all' || page.moduleUid === moduleFilter

      // Permission filter
      const hasAnyPermission = Object.values(page.permissions).some(Boolean)
      const matchesPermissionFilter = 
        permissionFilter === 'all' ||
        (permissionFilter === 'with-permissions' && hasAnyPermission) ||
        (permissionFilter === 'no-permissions' && !hasAnyPermission)

      return matchesSearch && matchesModule && matchesPermissionFilter
    })
  }, [allPages, searchQuery, moduleFilter, permissionFilter])

  const handlePageSelection = useCallback((pageUid: string, selected: boolean) => {
    setSelectedPages(prev => {
      const newSet = new Set(prev)
      if (selected) {
        newSet.add(pageUid)
      } else {
        newSet.delete(pageUid)
      }
      return newSet
    })
  }, [])

  const handleSelectAll = useCallback((selected: boolean) => {
    if (selected) {
      setSelectedPages(new Set(filteredPages.map(page => page.uid)))
    } else {
      setSelectedPages(new Set())
    }
  }, [filteredPages])

  const handlePermissionToggle = useCallback((permission: PermissionType) => {
    setSelectedPermissions(prev => {
      const newSet = new Set(prev)
      if (newSet.has(permission)) {
        newSet.delete(permission)
      } else {
        newSet.add(permission)
      }
      return newSet
    })
  }, [])

  const handleApplyBulkAction = useCallback(() => {
    if (selectedPages.size === 0 || selectedPermissions.size === 0) return

    const pageUIDs = Array.from(selectedPages)
    let permissions: Partial<PagePermission> = {}

    Array.from(selectedPermissions).forEach(permission => {
      if (bulkAction === 'grant') {
        permissions[permission] = true
        // Auto-grant view access if granting other permissions
        if (permission !== 'viewAccess') {
          permissions.viewAccess = true
        }
      } else if (bulkAction === 'revoke') {
        permissions[permission] = false
        // If revoking view access, revoke all permissions
        if (permission === 'viewAccess') {
          permissionOptions.forEach(opt => {
            permissions[opt.key] = false
          })
        }
      }
      // For 'replace', we set exactly what's selected
    })

    // Handle full access special case
    if (selectedPermissions.has('fullAccess')) {
      if (bulkAction === 'grant') {
        permissionOptions.forEach(opt => {
          permissions[opt.key] = true
        })
      } else if (bulkAction === 'revoke') {
        permissionOptions.forEach(opt => {
          permissions[opt.key] = false
        })
      }
    }

    onUpdate(pageUIDs, permissions)
  }, [selectedPages, selectedPermissions, bulkAction, onUpdate])

  const getSelectionStats = useCallback(() => {
    const selected = filteredPages.filter(page => selectedPages.has(page.uid))
    const withPermissions = selected.filter(page => Object.values(page.permissions).some(Boolean))
    
    return {
      total: selected.length,
      withPermissions: withPermissions.length,
      withoutPermissions: selected.length - withPermissions.length
    }
  }, [filteredPages, selectedPages])

  const allFilteredSelected = filteredPages.length > 0 && filteredPages.every(page => selectedPages.has(page.uid))
  const stats = getSelectionStats()

  if (!matrix) {
    return (
      <div className="text-center py-8">
        <p className="text-muted-foreground">No permission data available.</p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h3 className="text-lg font-semibold">Bulk Permission Editor</h3>
        <p className="text-sm text-muted-foreground mt-1">
          Select pages and permissions to apply bulk changes across multiple pages at once.
        </p>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Filters & Search</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label>Search Pages</Label>
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by name, module, URL..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-9"
                />
              </div>
            </div>
            
            <div className="space-y-2">
              <Label>Module</Label>
              <Select value={moduleFilter} onValueChange={setModuleFilter}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Modules</SelectItem>
                  {modules.map(module => (
                    <SelectItem key={module.uid} value={module.uid}>
                      {module.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            
            <div className="space-y-2">
              <Label>Permission Status</Label>
              <Select value={permissionFilter} onValueChange={(value: any) => setPermissionFilter(value)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Pages</SelectItem>
                  <SelectItem value="with-permissions">With Permissions</SelectItem>
                  <SelectItem value="no-permissions">No Permissions</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Page Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between text-base">
            <span>Select Pages ({filteredPages.length} available)</span>
            <div className="flex items-center gap-2">
              <Checkbox
                checked={allFilteredSelected}
                onCheckedChange={handleSelectAll}
              />
              <span className="text-sm text-muted-foreground">Select All</span>
            </div>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="max-h-64 overflow-y-auto space-y-2">
            {filteredPages.map(page => {
              const hasPermissions = Object.values(page.permissions).some(Boolean)
              
              return (
                <div
                  key={page.uid}
                  className="flex items-center space-x-3 p-2 rounded border hover:bg-gray-50"
                >
                  <Checkbox
                    checked={selectedPages.has(page.uid)}
                    onCheckedChange={(checked) => handlePageSelection(page.uid, checked as boolean)}
                  />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="font-medium text-sm">{page.pageNameEn}</span>
                      {hasPermissions ? (
                        <CheckCircle2 className="h-4 w-4 text-green-600" />
                      ) : (
                        <XCircle className="h-4 w-4 text-gray-400" />
                      )}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {page.moduleName} → {page.subModuleName}
                    </div>
                    <div className="text-xs text-muted-foreground">{page.pageUrl}</div>
                  </div>
                </div>
              )
            })}
            
            {filteredPages.length === 0 && (
              <div className="text-center py-8 text-muted-foreground">
                <p>No pages match the current filters.</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Selection Summary */}
      {selectedPages.size > 0 && (
        <Card className="bg-blue-50 border-blue-200">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <Badge variant="secondary">{stats.total} pages selected</Badge>
                <div className="text-sm text-muted-foreground">
                  {stats.withPermissions} with permissions • {stats.withoutPermissions} without permissions
                </div>
              </div>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setSelectedPages(new Set())}
              >
                Clear Selection
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Bulk Action Configuration */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Bulk Action Configuration</CardTitle>
          <CardDescription>
            Choose the action to perform and select the permissions to apply.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Action Type */}
          <div className="space-y-3">
            <Label>Action Type</Label>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <div
                className={`border rounded-lg p-3 cursor-pointer transition-colors ${
                  bulkAction === 'grant' ? 'border-green-500 bg-green-50' : 'border-gray-200 hover:border-gray-300'
                }`}
                onClick={() => setBulkAction('grant')}
              >
                <div className="flex items-center space-x-2">
                  <input
                    type="radio"
                    checked={bulkAction === 'grant'}
                    onChange={() => setBulkAction('grant')}
                    className="h-4 w-4"
                  />
                  <div>
                    <div className="font-medium text-sm">Grant Permissions</div>
                    <div className="text-xs text-muted-foreground">Add selected permissions to pages</div>
                  </div>
                </div>
              </div>
              
              <div
                className={`border rounded-lg p-3 cursor-pointer transition-colors ${
                  bulkAction === 'revoke' ? 'border-red-500 bg-red-50' : 'border-gray-200 hover:border-gray-300'
                }`}
                onClick={() => setBulkAction('revoke')}
              >
                <div className="flex items-center space-x-2">
                  <input
                    type="radio"
                    checked={bulkAction === 'revoke'}
                    onChange={() => setBulkAction('revoke')}
                    className="h-4 w-4"
                  />
                  <div>
                    <div className="font-medium text-sm">Revoke Permissions</div>
                    <div className="text-xs text-muted-foreground">Remove selected permissions from pages</div>
                  </div>
                </div>
              </div>
              
              <div
                className={`border rounded-lg p-3 cursor-pointer transition-colors ${
                  bulkAction === 'replace' ? 'border-blue-500 bg-blue-50' : 'border-gray-200 hover:border-gray-300'
                }`}
                onClick={() => setBulkAction('replace')}
              >
                <div className="flex items-center space-x-2">
                  <input
                    type="radio"
                    checked={bulkAction === 'replace'}
                    onChange={() => setBulkAction('replace')}
                    className="h-4 w-4"
                  />
                  <div>
                    <div className="font-medium text-sm">Replace Permissions</div>
                    <div className="text-xs text-muted-foreground">Set only selected permissions</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <Separator />

          {/* Permission Selection */}
          <div className="space-y-3">
            <Label>Select Permissions</Label>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              {permissionOptions.map(option => {
                const Icon = option.icon
                const isSelected = selectedPermissions.has(option.key)
                
                return (
                  <TooltipProvider key={option.key}>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <div
                          className={`border rounded-lg p-3 cursor-pointer transition-colors ${
                            isSelected ? 'border-blue-500 bg-blue-50' : 'border-gray-200 hover:border-gray-300'
                          }`}
                          onClick={() => handlePermissionToggle(option.key)}
                        >
                          <div className="flex items-center space-x-2">
                            <Checkbox
                              checked={isSelected}
                              onCheckedChange={() => handlePermissionToggle(option.key)}
                            />
                            <Icon className={`h-4 w-4 ${option.color}`} />
                            <span className="text-sm font-medium">{option.label}</span>
                          </div>
                        </div>
                      </TooltipTrigger>
                      <TooltipContent>
                        {option.description}
                      </TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                )
              })}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-between pt-4 border-t">
        <Button variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button 
          onClick={handleApplyBulkAction}
          disabled={selectedPages.size === 0 || selectedPermissions.size === 0}
        >
          Apply to {selectedPages.size} Pages
        </Button>
      </div>
    </div>
  )
}