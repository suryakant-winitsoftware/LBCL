"use client"

import React, { useState, useCallback, useMemo } from "react"
import { 
  ChevronDown, 
  ChevronRight, 
  Eye, 
  Plus, 
  Edit, 
  Trash2, 
  Check, 
  Download,
  Shield,
  Monitor,
  Smartphone,
  Search,
  Filter
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Checkbox } from "@/components/ui/checkbox"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { 
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select"
import { 
  PermissionMatrix as PermissionMatrixType, 
  PagePermission,
  Module,
  SubModule,
  Page
} from "@/types/admin.types"

interface PermissionMatrixProps {
  matrix: PermissionMatrixType
  loading?: boolean
  onChange: (updatedMatrix: PermissionMatrixType) => void
}

type PermissionType = 'viewAccess' | 'addAccess' | 'editAccess' | 'deleteAccess' | 'approvalAccess' | 'downloadAccess' | 'fullAccess'

interface PermissionColumn {
  key: PermissionType
  label: string
  icon: React.ComponentType<{ className?: string }>
  description: string
  color: string
}

const permissionColumns: PermissionColumn[] = [
  {
    key: 'viewAccess',
    label: 'View',
    icon: Eye,
    description: 'Can view and read data',
    color: 'text-gray-700'
  },
  {
    key: 'addAccess',
    label: 'Add',
    icon: Plus,
    description: 'Can create new records',
    color: 'text-gray-700'
  },
  {
    key: 'editAccess',
    label: 'Edit',
    icon: Edit,
    description: 'Can modify existing records',
    color: 'text-gray-700'
  },
  {
    key: 'deleteAccess',
    label: 'Delete',
    icon: Trash2,
    description: 'Can remove records',
    color: 'text-gray-700'
  },
  {
    key: 'approvalAccess',
    label: 'Approve',
    icon: Check,
    description: 'Can approve/reject requests',
    color: 'text-gray-700'
  },
  {
    key: 'downloadAccess',
    label: 'Download',
    icon: Download,
    description: 'Can export/download data',
    color: 'text-gray-700'
  },
  {
    key: 'fullAccess',
    label: 'Full',
    icon: Shield,
    description: 'Complete access to all features',
    color: 'text-gray-700'
  }
]

export function PermissionMatrix({ matrix, loading = false, onChange }: PermissionMatrixProps) {

  const [expandedModules, setExpandedModules] = useState<Set<string>>(new Set())
  const [expandedSubModules, setExpandedSubModules] = useState<Set<string>>(new Set())
  const [searchQuery, setSearchQuery] = useState("")
  const [permissionFilter, setPermissionFilter] = useState<'all' | 'granted' | 'denied'>('all')
  const [selectedPages, setSelectedPages] = useState<Set<string>>(new Set())

  // Filter and search logic
  const filteredMatrix = useMemo(() => {
    if (!matrix) {
      return null
    }


    const filtered = {
      ...matrix,
      modules: matrix.modules.map(module => ({
        ...module,
        subModules: module.subModules.map(subModule => ({
          ...subModule,
          pages: subModule.pages.filter(page => {
            // Search filter - using correct property names
            const matchesSearch = searchQuery === "" || 
              page.page.SubSubModuleNameEn.toLowerCase().includes(searchQuery.toLowerCase()) ||
              subModule.subModule.SubModuleNameEn.toLowerCase().includes(searchQuery.toLowerCase()) ||
              module.module.ModuleNameEn.toLowerCase().includes(searchQuery.toLowerCase())

            // Permission filter
            const hasAnyPermission = Object.values(page.permissions).some(Boolean)
            const matchesPermissionFilter = 
              permissionFilter === 'all' ||
              (permissionFilter === 'granted' && hasAnyPermission) ||
              (permissionFilter === 'denied' && !hasAnyPermission)

            const matches = matchesSearch && matchesPermissionFilter
            if (!matches && searchQuery !== "") {
            }

            return matches
          })
        })).filter(subModule => subModule.pages.length > 0)
      })).filter(module => module.subModules.length > 0)
    }


    return filtered
  }, [matrix, searchQuery, permissionFilter])

  const toggleModuleExpansion = useCallback((moduleUid: string) => {
    setExpandedModules(prev => {
      const newSet = new Set(prev)
      if (newSet.has(moduleUid)) {
        newSet.delete(moduleUid)
      } else {
        newSet.add(moduleUid)
      }
      return newSet
    })
  }, [])

  const toggleSubModuleExpansion = useCallback((subModuleUid: string) => {
    setExpandedSubModules(prev => {
      const newSet = new Set(prev)
      if (newSet.has(subModuleUid)) {
        newSet.delete(subModuleUid)
      } else {
        newSet.add(subModuleUid)
      }
      return newSet
    })
  }, [])

  const handlePermissionChange = useCallback((
    moduleUid: string,
    subModuleUid: string,
    pageUid: string,
    permissionType: PermissionType,
    value: boolean
  ) => {
    if (!matrix) return

    const updatedMatrix = {
      ...matrix,
      modules: matrix.modules.map(module => {
        if (module.module.UID !== moduleUid) return module

        return {
          ...module,
          subModules: module.subModules.map(subModule => {
            if (subModule.subModule.UID !== subModuleUid) return subModule

            return {
              ...subModule,
              pages: subModule.pages.map(page => {
                if (page.page.UID !== pageUid) return page

                const newPermissions = { ...page.permissions }

                if (permissionType === 'fullAccess' && value) {
                  // Full access grants all permissions
                  Object.keys(newPermissions).forEach(key => {
                    if (key !== 'uid' && key !== 'id' && key !== 'createdBy' && key !== 'createdTime' && key !== 'modifiedBy' && key !== 'modifiedTime') {
                      newPermissions[key as PermissionType] = true
                    }
                  })
                } else if (permissionType === 'fullAccess' && !value) {
                  // Removing full access removes all permissions
                  Object.keys(newPermissions).forEach(key => {
                    if (key !== 'uid' && key !== 'id' && key !== 'createdBy' && key !== 'createdTime' && key !== 'modifiedBy' && key !== 'modifiedTime') {
                      newPermissions[key as PermissionType] = false
                    }
                  })
                } else {
                  newPermissions[permissionType] = value
                  
                  // If granting any permission, ensure view access is also granted
                  if (value && permissionType !== 'viewAccess') {
                    newPermissions.viewAccess = true
                  }
                  
                  // If removing view access, remove all other permissions
                  if (!value && permissionType === 'viewAccess') {
                    Object.keys(newPermissions).forEach(key => {
                      if (key !== 'uid' && key !== 'id' && key !== 'createdBy' && key !== 'createdTime' && key !== 'modifiedBy' && key !== 'modifiedTime') {
                        newPermissions[key as PermissionType] = false
                      }
                    })
                  }
                  
                  // Update full access based on other permissions
                  const hasAllPermissions = permissionColumns
                    .filter(col => col.key !== 'fullAccess')
                    .every(col => newPermissions[col.key])
                  newPermissions.fullAccess = hasAllPermissions
                }

                // Mark as modified and update timestamp
                newPermissions.isModified = true
                newPermissions.modifiedBy = 'ADMIN'
                newPermissions.modifiedTime = new Date().toISOString()
                
                // IMPORTANT: Preserve all backend fields from original permissions
                if (page.permissions.roleUID) {
                  newPermissions.roleUID = page.permissions.roleUID
                }
                if (page.permissions.subSubModuleUID) {
                  newPermissions.subSubModuleUID = page.permissions.subSubModuleUID
                }
                if (page.permissions.platform) {
                  newPermissions.platform = page.permissions.platform
                }
                if (page.permissions.uid) {
                  newPermissions.uid = page.permissions.uid
                }
                if (page.permissions.id) {
                  newPermissions.id = page.permissions.id
                }
                if (page.permissions.createdBy) {
                  newPermissions.createdBy = page.permissions.createdBy
                }
                if (page.permissions.createdTime) {
                  newPermissions.createdTime = page.permissions.createdTime
                }

                return {
                  ...page,
                  permissions: newPermissions
                }
              })
            }
          })
        }
      })
    }

    onChange(updatedMatrix)
  }, [matrix, onChange])

  const handleBulkPermissionChange = useCallback((
    permissionType: PermissionType,
    value: boolean,
    scope: 'selected' | 'visible' | 'all'
  ) => {
    if (!matrix) return

    let pagesToUpdate: string[] = []

    if (scope === 'selected') {
      pagesToUpdate = Array.from(selectedPages)
    } else if (scope === 'visible' && filteredMatrix) {
      pagesToUpdate = filteredMatrix.modules.flatMap(module =>
        module.subModules.flatMap(subModule =>
          subModule.pages.map(page => page.page.UID)
        )
      )
    } else {
      pagesToUpdate = matrix.modules.flatMap(module =>
        module.subModules.flatMap(subModule =>
          subModule.pages.map(page => page.page.UID)
        )
      )
    }

    const updatedMatrix = {
      ...matrix,
      modules: matrix.modules.map(module => ({
        ...module,
        subModules: module.subModules.map(subModule => ({
          ...subModule,
          pages: subModule.pages.map(page => {
            if (!pagesToUpdate.includes(page.page.UID)) return page

            const newPermissions = { ...page.permissions }

            if (permissionType === 'fullAccess' && value) {
              Object.keys(newPermissions).forEach(key => {
                newPermissions[key as PermissionType] = true
              })
            } else if (permissionType === 'fullAccess' && !value) {
              Object.keys(newPermissions).forEach(key => {
                newPermissions[key as PermissionType] = false
              })
            } else {
              newPermissions[permissionType] = value
              
              if (value && permissionType !== 'viewAccess') {
                newPermissions.viewAccess = true
              }
              
              if (!value && permissionType === 'viewAccess') {
                Object.keys(newPermissions).forEach(key => {
                  newPermissions[key as PermissionType] = false
                })
              }
              
              const hasAllPermissions = permissionColumns
                .filter(col => col.key !== 'fullAccess')
                .every(col => newPermissions[col.key])
              newPermissions.fullAccess = hasAllPermissions
            }
            
            // IMPORTANT: Preserve all backend fields from original permissions
            if (page.permissions.roleUID) {
              newPermissions.roleUID = page.permissions.roleUID
            }
            if (page.permissions.subSubModuleUID) {
              newPermissions.subSubModuleUID = page.permissions.subSubModuleUID
            }
            if (page.permissions.platform) {
              newPermissions.platform = page.permissions.platform
            }
            if (page.permissions.uid) {
              newPermissions.uid = page.permissions.uid
            }
            if (page.permissions.id) {
              newPermissions.id = page.permissions.id
            }
            if (page.permissions.createdBy) {
              newPermissions.createdBy = page.permissions.createdBy
            }
            if (page.permissions.createdTime) {
              newPermissions.createdTime = page.permissions.createdTime
            }
            
            // Mark as modified and update timestamp
            newPermissions.isModified = true
            newPermissions.modifiedBy = 'ADMIN'
            newPermissions.modifiedTime = new Date().toISOString()

            return {
              ...page,
              permissions: newPermissions
            }
          })
        }))
      }))
    }

    onChange(updatedMatrix)
  }, [matrix, filteredMatrix, selectedPages, onChange])

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
    if (!filteredMatrix) return

    if (selected) {
      const allPageUids = filteredMatrix.modules.flatMap(module =>
        module.subModules.flatMap(subModule =>
          subModule.pages.map(page => page.page.UID)
        )
      )
      setSelectedPages(new Set(allPageUids))
    } else {
      setSelectedPages(new Set())
    }
  }, [filteredMatrix])

  const getPermissionStats = useCallback(() => {
    if (!matrix) return { total: 0, granted: 0, percentage: 0 }

    let total = 0
    let granted = 0

    matrix.modules.forEach(module => {
      module.subModules.forEach(subModule => {
        subModule.pages.forEach(page => {
          total++
          if (Object.values(page.permissions).some(Boolean)) {
            granted++
          }
        })
      })
    })

    return {
      total,
      granted,
      percentage: total > 0 ? Math.round((granted / total) * 100) : 0
    }
  }, [matrix])

  if (loading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-8 w-32" />
        </div>
        {[...Array(5)].map((_, i) => (
          <Card key={i}>
            <CardContent className="p-4">
              <Skeleton className="h-12 w-full" />
            </CardContent>
          </Card>
        ))}
      </div>
    )
  }

  if (!filteredMatrix) {
    return (
      <div className="text-center py-8">
        <p className="text-muted-foreground">No permission data available.</p>
      </div>
    )
  }

  const stats = getPermissionStats()
  const visiblePages = filteredMatrix.modules.flatMap(module =>
    module.subModules.flatMap(subModule => subModule.pages)
  )
  const allVisibleSelected = visiblePages.length > 0 && visiblePages.every(page => selectedPages.has(page.page.UID))

  return (
    <div className="space-y-6">
      {/* Professional Header Controls */}
      <div className="bg-white border border-gray-200 rounded-lg p-4">
        <div className="flex flex-col lg:flex-row gap-4 items-start lg:items-center justify-between">
          <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3 flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search modules and pages..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9 w-64"
              />
            </div>
            <Select value={permissionFilter} onValueChange={(value: any) => setPermissionFilter(value)}>
              <SelectTrigger className="w-48">
                <Filter className="mr-2 h-4 w-4" />
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Pages</SelectItem>
                <SelectItem value="granted">With Permissions</SelectItem>
                <SelectItem value="denied">No Permissions</SelectItem>
              </SelectContent>
            </Select>
          </div>
          
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2 text-sm text-gray-600 bg-gray-50 px-3 py-2 rounded">
              {matrix.platform === 'Web' ? <Monitor className="h-4 w-4" /> : <Smartphone className="h-4 w-4" />}
              <span>{matrix.platform}</span>
            </div>
            <Badge variant="outline" className="px-3 py-1">
              {stats.granted}/{stats.total} ({stats.percentage}%)
            </Badge>
          </div>
        </div>
      </div>

      {/* Professional Bulk Actions */}
      {selectedPages.size > 0 && (
        <Card className="border-gray-200">
          <CardContent className="p-4">
            <div className="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-4">
              <div className="flex items-center gap-3">
                <Badge variant="secondary" className="bg-gray-100 text-gray-700">
                  {selectedPages.size} pages selected
                </Badge>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setSelectedPages(new Set())}
                  className="text-gray-500 hover:text-gray-700"
                >
                  Clear
                </Button>
              </div>
              <div className="flex flex-wrap gap-2">
                <span className="text-sm text-gray-600 mr-2">Grant:</span>
                {permissionColumns.map(column => {
                  const Icon = column.icon
                  return (
                    <TooltipProvider key={column.key}>
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleBulkPermissionChange(column.key, true, 'selected')}
                            className="hover:bg-gray-50"
                          >
                            <Icon className="h-4 w-4 mr-1" />
                            {column.label}
                          </Button>
                        </TooltipTrigger>
                        <TooltipContent>
                          <p>{column.description}</p>
                        </TooltipContent>
                      </Tooltip>
                    </TooltipProvider>
                  )
                })}
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    permissionColumns.forEach(col => {
                      handleBulkPermissionChange(col.key, false, 'selected')
                    })
                  }}
                  className="text-red-600 hover:bg-red-50"
                >
                  Clear All
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Clean Permission Table */}
      <Card className="border-gray-200">
        <CardHeader className="bg-gray-50 border-b">
          <CardTitle className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <span className="text-lg font-semibold text-gray-900">Permission Matrix</span>
              <Badge variant="outline" className="text-xs">
                {filteredMatrix?.modules.length || 0} modules
              </Badge>
            </div>
            <div className="flex items-center gap-2">
              <Checkbox
                checked={allVisibleSelected}
                onCheckedChange={handleSelectAll}
              />
              <span className="text-sm text-gray-600">Select All</span>
            </div>
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-100 border-b">
                <tr>
                  <th className="text-left p-3 font-medium text-gray-700 min-w-96">
                    Page / Feature
                  </th>
                  {permissionColumns.map(column => {
                    const Icon = column.icon
                    return (
                      <th key={column.key} className="text-center p-3 font-medium text-gray-700 min-w-20">
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <div className="flex flex-col items-center gap-1 cursor-help">
                                <Icon className="h-4 w-4" />
                                <span className="text-xs">{column.label}</span>
                              </div>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p className="font-medium">{column.label}</p>
                              <p className="text-xs text-gray-500">{column.description}</p>
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      </th>
                    )
                  })}
                </tr>
              </thead>
              <tbody>
                {filteredMatrix.modules.map(module => (
                  <React.Fragment key={module.module.UID}>
                    {/* Clean Module Header */}
                    <tr className="bg-gray-50 border-b hover:bg-gray-100 transition-colors">
                      <td colSpan={permissionColumns.length + 1} className="p-3">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => toggleModuleExpansion(module.module.UID)}
                          className="flex items-center gap-2 font-medium text-gray-800 hover:text-gray-900 w-full justify-start"
                        >
                          {expandedModules.has(module.module.UID) ? 
                            <ChevronDown className="h-4 w-4" /> : 
                            <ChevronRight className="h-4 w-4" />
                          }
                          <span>{module.module.ModuleNameEn}</span>
                          <Badge variant="outline" className="ml-2 text-xs">
                            {module.subModules.reduce((count, sub) => count + sub.pages.length, 0)} pages
                          </Badge>
                        </Button>
                      </td>
                    </tr>

                    {/* Module Content */}
                    {expandedModules.has(module.module.UID) && module.subModules.map(subModule => (
                      <React.Fragment key={subModule.subModule.UID}>
                        {/* SubModule Header */}
                        <tr className="bg-gray-25 border-b">
                          <td colSpan={permissionColumns.length + 1} className="p-3 pl-8">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => toggleSubModuleExpansion(subModule.subModule.UID)}
                              className="flex items-center gap-2 text-sm text-gray-700 hover:text-gray-900 hover:bg-gray-100"
                            >
                              {expandedSubModules.has(subModule.subModule.UID) ? 
                                <ChevronDown className="h-3 w-3" /> : 
                                <ChevronRight className="h-3 w-3" />
                              }
                              {subModule.subModule.SubModuleNameEn}
                              <Badge variant="outline" className="ml-2 text-xs bg-gray-100 text-gray-600 border-gray-300">
                                {subModule.pages.length} pages
                              </Badge>
                            </Button>
                          </td>
                        </tr>

                        {/* Pages */}
                        {expandedSubModules.has(subModule.subModule.UID) && subModule.pages.map(page => (
                          <tr key={page.page.UID} className="border-b hover:bg-gray-50 transition-colors">
                            <td className="p-3 pl-12">
                              <div className="flex items-center gap-3">
                                <Checkbox
                                  checked={selectedPages.has(page.page.UID)}
                                  onCheckedChange={(checked) => handlePageSelection(page.page.UID, checked as boolean)}
                                  className="border-gray-300"
                                />
                                <div>
                                  <div className="font-medium text-sm text-gray-900">{page.page.SubSubModuleNameEn}</div>
                                  <div className="text-xs text-gray-500">{page.page.RelativePath}</div>
                                </div>
                              </div>
                            </td>
                            {permissionColumns.map(column => (
                              <td key={column.key} className="text-center p-3">
                                <Checkbox
                                  checked={page.permissions[column.key] || false}
                                  onCheckedChange={(checked) => 
                                    handlePermissionChange(
                                      module.module.UID,
                                      subModule.subModule.UID,
                                      page.page.UID,
                                      column.key,
                                      checked as boolean
                                    )
                                  }
                                  className="border-gray-300"
                                />
                              </td>
                            ))}
                          </tr>
                        ))}
                      </React.Fragment>
                    ))}
                  </React.Fragment>
                ))}
              </tbody>
            </table>
          </div>

          {visiblePages.length === 0 && (
            <div className="text-center py-8 text-muted-foreground">
              <p>No pages match the current filters.</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}