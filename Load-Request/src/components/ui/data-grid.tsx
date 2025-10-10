"use client"

import { useState, useCallback, useMemo } from "react"
import { ChevronUp, ChevronDown, MoreHorizontal, Check } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { 
  DropdownMenu, 
  DropdownMenuContent, 
  DropdownMenuItem, 
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger 
} from "@/components/ui/dropdown-menu"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { SkeletonLoader } from "@/components/ui/loader"
import { cn } from "@/lib/utils"

export interface DataGridColumn<T = any> {
  key: string
  title: string
  width?: string
  sortable?: boolean
  filterable?: boolean
  render?: (value: any, record: T, index: number) => React.ReactNode
  className?: string
}

export interface DataGridAction<T = any> {
  key: string
  label: string
  icon?: React.ComponentType<{ className?: string }>
  onClick: (record: T) => void
  disabled?: (record: T) => boolean
  variant?: "default" | "destructive"
}

export interface DataGridBulkAction {
  key: string
  label: string
  icon?: React.ComponentType<{ className?: string }>
  variant?: "default" | "destructive"
}

export interface DataGridProps<T = any> {
  data: T[]
  columns: DataGridColumn<T>[]
  loading?: boolean
  pagination?: {
    current: number
    pageSize: number
    total: number
    showSizeChanger?: boolean
    pageSizeOptions?: number[]
    onChange: (page: number, pageSize: number) => void
  }
  sorting?: {
    field?: string
    direction?: "asc" | "desc"
    onChange: (field: string, direction: "asc" | "desc") => void
  }
  selection?: {
    selectedRowKeys: string[]
    onChange: (selectedRowKeys: string[]) => void
    getRowKey: (record: T) => string
  }
  actions?: DataGridAction<T>[]
  bulkActions?: DataGridBulkAction[]
  onBulkAction?: (actionKey: string, selectedKeys: string[]) => void
  emptyText?: string
  className?: string
}

export function DataGrid<T = any>({
  data,
  columns,
  loading = false,
  pagination,
  sorting,
  selection,
  actions,
  bulkActions,
  onBulkAction,
  emptyText = "No data available",
  className
}: DataGridProps<T>) {
  const [hoveredRow, setHoveredRow] = useState<string | null>(null)

  const handleSort = useCallback((field: string) => {
    if (!sorting?.onChange) return
    
    const newDirection = 
      sorting.field === field && sorting.direction === "asc" ? "desc" : "asc"
    sorting.onChange(field, newDirection)
  }, [sorting])

  const handleSelectAll = useCallback((checked: boolean) => {
    if (!selection || !data) return
    
    if (checked) {
      const allKeys = data.map(selection.getRowKey)
      selection.onChange(allKeys)
    } else {
      selection.onChange([])
    }
  }, [data, selection])

  const handleSelectRow = useCallback((rowKey: string, checked: boolean) => {
    if (!selection) return
    
    const newSelectedKeys = checked
      ? [...selection.selectedRowKeys, rowKey]
      : selection.selectedRowKeys.filter(key => key !== rowKey)
    
    selection.onChange(newSelectedKeys)
  }, [selection])

  const handleBulkAction = useCallback((actionKey: string) => {
    if (onBulkAction && selection) {
      onBulkAction(actionKey, selection.selectedRowKeys)
    }
  }, [onBulkAction, selection])

  const isAllSelected = useMemo(() => {
    if (!selection || !data || data.length === 0) return false
    return data.every(record => 
      selection.selectedRowKeys.includes(selection.getRowKey(record))
    )
  }, [data, selection])

  const isIndeterminate = useMemo(() => {
    if (!selection || !data || data.length === 0) return false
    const selectedCount = data.filter(record => 
      selection.selectedRowKeys.includes(selection.getRowKey(record))
    ).length
    return selectedCount > 0 && selectedCount < data.length
  }, [data, selection])

  const renderSortIcon = (field: string) => {
    if (!sorting || sorting.field !== field) return null
    
    return sorting.direction === "asc" ? (
      <ChevronUp className="ml-1 h-3 w-3" />
    ) : (
      <ChevronDown className="ml-1 h-3 w-3" />
    )
  }

  const renderPagination = () => {
    if (!pagination) return null

    const { current, pageSize, total, showSizeChanger, pageSizeOptions, onChange } = pagination
    const totalPages = Math.ceil(total / pageSize)
    const startItem = (current - 1) * pageSize + 1
    const endItem = Math.min(current * pageSize, total)

    return (
      <div className="flex items-center justify-between px-2 py-4">
        <div className="text-sm text-muted-foreground">
          Showing {startItem} to {endItem} of {total} entries
        </div>
        <div className="flex items-center space-x-2">
          {showSizeChanger && pageSizeOptions && (
            <div className="flex items-center space-x-2">
              <span className="text-sm">Show</span>
              <select
                value={pageSize}
                onChange={(e) => onChange(1, Number(e.target.value))}
                className="border rounded px-2 py-1 text-sm"
              >
                {pageSizeOptions.map(size => (
                  <option key={size} value={size}>{size}</option>
                ))}
              </select>
              <span className="text-sm">entries</span>
            </div>
          )}
          <div className="flex items-center space-x-1">
            <Button
              variant="outline"
              size="sm"
              onClick={() => onChange(current - 1, pageSize)}
              disabled={current <= 1}
            >
              Previous
            </Button>
            <span className="text-sm px-3 py-1 border rounded">
              {current} of {totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => onChange(current + 1, pageSize)}
              disabled={current >= totalPages}
            >
              Next
            </Button>
          </div>
        </div>
      </div>
    )
  }

  const renderBulkActions = () => {
    if (!bulkActions || !selection || selection.selectedRowKeys.length === 0) {
      return null
    }

    return (
      <div className="flex items-center gap-2 mb-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
        <span className="text-sm text-blue-800">
          {selection.selectedRowKeys.length} item{selection.selectedRowKeys.length !== 1 ? 's' : ''} selected
        </span>
        <div className="flex gap-2">
          {bulkActions.map(action => {
            const Icon = action.icon
            return (
              <Button
                key={action.key}
                size="sm"
                variant={action.variant === "destructive" ? "destructive" : "outline"}
                onClick={() => handleBulkAction(action.key)}
                className="h-8"
              >
                {Icon && <Icon className="mr-1 h-3 w-3" />}
                {action.label}
              </Button>
            )
          })}
        </div>
      </div>
    )
  }

  if (loading) {
    return (
      <div className={cn("space-y-4", className)}>
        {renderBulkActions()}
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                {selection && <TableHead className="w-12" />}
                {columns.map(column => (
                  <TableHead key={column.key} style={{ width: column.width }}>
                    <SkeletonLoader className="h-4 w-20" />
                  </TableHead>
                ))}
                {actions && <TableHead className="w-12" />}
              </TableRow>
            </TableHeader>
            <TableBody>
              {Array.from({ length: 5 }).map((_, index) => (
                <TableRow key={index}>
                  {selection && (
                    <TableCell>
                      <SkeletonLoader className="h-4 w-4" />
                    </TableCell>
                  )}
                  {columns.map(column => (
                    <TableCell key={column.key}>
                      <SkeletonLoader className="h-4 w-full" />
                    </TableCell>
                  ))}
                  {actions && (
                    <TableCell>
                      <SkeletonLoader className="h-4 w-4" />
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
        {/* Debug info for loading state - remove in production */}
        {process.env.NODE_ENV === 'development' && (
          <div className="text-xs text-muted-foreground p-2 bg-yellow-50 border border-yellow-200 rounded">
            Debug: DataGrid is in loading state. If this persists, check dependency arrays in parent component.
          </div>
        )}
      </div>
    )
  }

  return (
    <div className={cn("space-y-4", className)}>
      {renderBulkActions()}
      
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              {selection && (
                <TableHead className="w-12">
                  <Checkbox
                    checked={isAllSelected}
                    indeterminate={isIndeterminate}
                    onCheckedChange={handleSelectAll}
                  />
                </TableHead>
              )}
              {columns.map(column => (
                <TableHead 
                  key={column.key} 
                  style={{ width: column.width }}
                  className={cn(column.className)}
                >
                  {column.sortable ? (
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-auto p-0 font-medium hover:bg-transparent"
                      onClick={() => handleSort(column.key)}
                    >
                      {column.title}
                      {renderSortIcon(column.key)}
                    </Button>
                  ) : (
                    column.title
                  )}
                </TableHead>
              ))}
              {actions && <TableHead className="w-12">Actions</TableHead>}
            </TableRow>
          </TableHeader>
          <TableBody>
            {!data || data.length === 0 ? (
              <TableRow>
                <TableCell 
                  colSpan={columns.length + (selection ? 1 : 0) + (actions ? 1 : 0)}
                  className="h-24 text-center text-muted-foreground"
                >
                  {emptyText}
                </TableCell>
              </TableRow>
            ) : (
              data.map((record, index) => {
                const rowKey = selection?.getRowKey(record) || String(index)
                const isSelected = selection?.selectedRowKeys.includes(rowKey) || false
                const isHovered = hoveredRow === rowKey

                return (
                  <TableRow
                    key={rowKey}
                    className={cn(
                      "hover:bg-muted/50 transition-colors",
                      isSelected && "bg-blue-50 hover:bg-blue-100"
                    )}
                    onMouseEnter={() => setHoveredRow(rowKey)}
                    onMouseLeave={() => setHoveredRow(null)}
                  >
                    {selection && (
                      <TableCell>
                        <Checkbox
                          checked={isSelected}
                          onCheckedChange={(checked) => 
                            handleSelectRow(rowKey, checked as boolean)
                          }
                        />
                      </TableCell>
                    )}
                    {columns.map(column => (
                      <TableCell 
                        key={column.key}
                        className={cn(column.className)}
                      >
                        {column.render 
                          ? column.render(record[column.key as keyof T], record, index)
                          : String(record[column.key as keyof T] || '')
                        }
                      </TableCell>
                    ))}
                    {actions && (
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            {actions.map(action => {
                              const Icon = action.icon
                              const disabled = action.disabled?.(record) || false
                              
                              return (
                                <DropdownMenuItem
                                  key={action.key}
                                  onClick={() => !disabled && action.onClick(record)}
                                  disabled={disabled}
                                  className={cn(
                                    action.variant === "destructive" && 
                                    "text-red-600 focus:text-red-600"
                                  )}
                                >
                                  {Icon && <Icon className="mr-2 h-4 w-4" />}
                                  {action.label}
                                </DropdownMenuItem>
                              )
                            })}
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    )}
                  </TableRow>
                )
              })
            )}
          </TableBody>
        </Table>
      </div>

      {renderPagination()}
    </div>
  )
}