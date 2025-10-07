'use client'

import * as React from 'react'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from '@/components/ui/select'
import { 
  ChevronLeft, 
  ChevronRight, 
  ChevronsLeft, 
  ChevronsRight,
  Search,
  ArrowUpDown
} from 'lucide-react'

interface Column {
  accessorKey?: string
  id?: string
  header: string | React.ReactNode | ((props: any) => React.ReactNode)
  cell?: (props: any) => React.ReactNode
  sortable?: boolean
}

interface DataTableProps {
  columns: Column[]
  data: any[]
  loading?: boolean
  searchable?: boolean
  pagination?: boolean
  pageSize?: number
  onRowClick?: (row: any) => void
}

export function DataTable({
  columns,
  data,
  loading = false,
  searchable = true,
  pagination = true,
  pageSize = 10,
  onRowClick
}: DataTableProps) {
  const [globalFilter, setGlobalFilter] = React.useState<string>('')
  const [sorting, setSorting] = React.useState<{key: string, direction: 'asc' | 'desc'} | null>(null)
  const [currentPage, setCurrentPage] = React.useState(1)

  // Filter data based on global search
  const filteredData = React.useMemo(() => {
    if (!globalFilter) return data
    
    return data.filter(row =>
      columns.some(column => {
        if (column.accessorKey) {
          const value = row[column.accessorKey]
          return value && value.toString().toLowerCase().includes(globalFilter.toLowerCase())
        }
        return false
      })
    )
  }, [data, globalFilter, columns])

  // Sort data
  const sortedData = React.useMemo(() => {
    if (!sorting) return filteredData

    return [...filteredData].sort((a, b) => {
      const aValue = a[sorting.key]
      const bValue = b[sorting.key]

      if (aValue === bValue) return 0

      if (sorting.direction === 'asc') {
        return aValue > bValue ? 1 : -1
      } else {
        return aValue < bValue ? 1 : -1
      }
    })
  }, [filteredData, sorting])

  // Paginate data
  const paginatedData = React.useMemo(() => {
    if (!pagination) return sortedData
    
    const startIndex = (currentPage - 1) * pageSize
    const endIndex = startIndex + pageSize
    return sortedData.slice(startIndex, endIndex)
  }, [sortedData, currentPage, pageSize, pagination])

  const totalPages = Math.ceil(sortedData.length / pageSize)

  const handleSort = (key: string) => {
    setSorting(prev => {
      if (prev?.key === key) {
        return prev.direction === 'asc' 
          ? { key, direction: 'desc' }
          : null
      }
      return { key, direction: 'asc' }
    })
  }

  const renderHeader = (column: Column) => {
    if (typeof column.header === 'function') {
      return column.header({ column })
    }
    
    if (column.sortable !== false && column.accessorKey) {
      return (
        <Button
          variant="ghost"
          onClick={() => handleSort(column.accessorKey!)}
          className="h-auto p-0 font-medium hover:bg-transparent"
        >
          {column.header}
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      )
    }
    
    return column.header
  }

  const renderCell = (column: Column, row: any) => {
    if (column.cell) {
      return column.cell({ row: { original: row, getValue: (key: string) => row[key] } })
    }
    
    if (column.accessorKey) {
      return row[column.accessorKey]
    }
    
    return null
  }

  if (loading) {
    return (
      <div className="space-y-4">
        {searchable && (
          <div className="flex items-center space-x-2">
            <Search className="h-4 w-4 text-muted-foreground" />
            <Input 
              placeholder="Search..."
              value=""
              disabled
              className="max-w-sm"
            />
          </div>
        )}
        
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                {columns.map((column, index) => (
                  <TableHead key={column.accessorKey || column.id || index}>
                    {typeof column.header === 'string' ? column.header : 'Loading...'}
                  </TableHead>
                ))}
              </TableRow>
            </TableHeader>
            <TableBody>
              {[...Array(3)].map((_, index) => (
                <TableRow key={index}>
                  {columns.map((_, colIndex) => (
                    <TableCell key={colIndex}>
                      <div className="h-6 bg-muted animate-pulse rounded" />
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {searchable && (
        <div className="flex items-center space-x-2">
          <Search className="h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search..."
            value={globalFilter || ''}
            onChange={(e) => setGlobalFilter(e.target.value)}
            className="max-w-sm"
          />
        </div>
      )}

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              {columns.map((column, index) => (
                <TableHead key={column.accessorKey || column.id || index}>
                  {renderHeader(column)}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {paginatedData.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center">
                  No results found.
                </TableCell>
              </TableRow>
            ) : (
              paginatedData.map((row, rowIndex) => (
                <TableRow 
                  key={rowIndex}
                  className={onRowClick ? 'cursor-pointer hover:bg-muted/50' : ''}
                  onClick={() => onRowClick?.(row)}
                >
                  {columns.map((column, colIndex) => (
                    <TableCell key={column.accessorKey || column.id || colIndex}>
                      {renderCell(column, row)}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {pagination && totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <p className="text-sm text-muted-foreground">
              Showing {((currentPage - 1) * pageSize) + 1} to{' '}
              {Math.min(currentPage * pageSize, sortedData.length)} of{' '}
              {sortedData.length} results
            </p>
          </div>

          <div className="flex items-center space-x-2">
            <div className="flex items-center space-x-2">
              <p className="text-sm font-medium">Rows per page</p>
              <Select
                value={pageSize.toString()}
                onValueChange={(value) => {
                  setCurrentPage(1)
                  // Note: pageSize is prop-controlled, parent should handle this
                }}
              >
                <SelectTrigger className="h-8 w-[70px]">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent side="top">
                  {[10, 20, 30, 40, 50].map((size) => (
                    <SelectItem key={size} value={size.toString()}>
                      {size}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex items-center space-x-2">
              <p className="text-sm font-medium">
                Page {currentPage} of {totalPages}
              </p>
              
              <div className="flex items-center space-x-1">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(1)}
                  disabled={currentPage === 1}
                >
                  <ChevronsLeft className="h-4 w-4" />
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                  disabled={currentPage === 1}
                >
                  <ChevronLeft className="h-4 w-4" />
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                  disabled={currentPage === totalPages}
                >
                  <ChevronRight className="h-4 w-4" />
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(totalPages)}
                  disabled={currentPage === totalPages}
                >
                  <ChevronsRight className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}