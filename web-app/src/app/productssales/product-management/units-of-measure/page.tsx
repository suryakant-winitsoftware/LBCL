'use client'

import React, { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { SafeSelect, SafeSelectItem } from '@/components/ui/safe-select'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'
import { Badge } from '@/components/ui/badge'
import { 
  Dialog, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogTrigger 
} from '@/components/ui/dialog'
import { useToast } from '@/components/ui/use-toast'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem
} from '@/components/ui/dropdown-menu'
import { PaginationControls } from '@/components/ui/pagination-controls'
import { Skeleton } from '@/components/ui/skeleton'
import { 
  Plus, 
  Search, 
  Filter, 
  Edit, 
  Trash2, 
  Database, 
  RefreshCw, 
  Download,
  Settings,
  Eye,
  AlertCircle,
  MoreHorizontal,
  Upload,
  ChevronDown,
  X,
  Check
} from 'lucide-react'

import DynamicFormGenerator from '@/components/dynamic/DynamicFormGenerator'
import { 
  fullyDynamicUOMService, 
  DynamicTableSchema 
} from '@/services/sku/fully-dynamic-uom.service'

interface UOMRecord {
  [key: string]: any
}

export default function UnitsOfMeasurePage() {
  const router = useRouter()
  const { toast } = useToast()

  // Dynamic state - no hardcoded fields
  const [schema, setSchema] = useState<DynamicTableSchema | null>(null)
  const [records, setRecords] = useState<UOMRecord[]>([])
  const [filteredRecords, setFilteredRecords] = useState<UOMRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedCategory, setSelectedCategory] = useState<string>('')
  const [selectedRecord, setSelectedRecord] = useState<UOMRecord | null>(null)
  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [showEditDialog, setShowEditDialog] = useState(false)
  const [showViewDialog, setShowViewDialog] = useState(false)
  const [typeFilter, setTypeFilter] = useState<string[]>([]) // Filter for UOM types
  const searchInputRef = useRef<HTMLInputElement>(null) // Ref for search input

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [totalCount, setTotalCount] = useState(0)

  // Dynamic table name - can be changed to any table
  const tableName = 'SKUUOM' // This could come from props or config
  
  // Track if component has been initialized
  const [isInitialized, setIsInitialized] = useState(false)

  // Load initial schema only
  useEffect(() => {
    loadInitialData().then(() => {
      setIsInitialized(true)
    })
  }, [])

  // Debounce search to reduce API calls (same pattern as products page)
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState(searchTerm)
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm)
    }, 300) // 300ms delay
    return () => clearTimeout(timer)
  }, [searchTerm])

  // Load data when debouncedSearchTerm or typeFilter changes
  useEffect(() => {
    if (!schema || !isInitialized) return
    
    if (currentPage === 1) {
      loadPagedData()
    } else {
      setCurrentPage(1) // This will trigger loadPagedData via the next useEffect
    }
  }, [debouncedSearchTerm, typeFilter])

  // Load data when page or pageSize changes
  useEffect(() => {
    // Only load if initialized (prevents double load on mount)
    if (schema && isInitialized) {
      loadPagedData()
    }
  }, [currentPage, pageSize])

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Check for Ctrl+F (Windows/Linux) or Cmd+F (Mac)
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault() // Prevent browser's default find
        searchInputRef.current?.focus()
        searchInputRef.current?.select() // Select existing text for easy replacement
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [])

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1) // Reset to first page when searching
  }

  const loadInitialData = async () => {
    try {
      setLoading(true)
      console.log('🚀 Starting initial data load...')
      
      // Get schema dynamically from the service
      const tableSchema = await fullyDynamicUOMService.getTableSchema(tableName)
      setSchema(tableSchema)
      
      // Load initial data after schema is set
      if (tableSchema) {
        console.log('📊 Loading initial UOM data...')
        console.log('Schema loaded:', tableSchema)
        
        // Use proper server-side pagination
        const { data, totalCount: serverTotalCount } = await fullyDynamicUOMService.getPagedUOMData(
          1, // Always start with page 1 for initial load
          pageSize,
          undefined, // No search term on initial load (undefined instead of empty string)
          undefined // No filter on initial load (undefined instead of empty string)
        )

        console.log(`✅ Data received:`, data)
        console.log(`📊 Total count:`, serverTotalCount)

        setRecords(data)
        setFilteredRecords(data)
        setTotalCount(serverTotalCount)
        console.log(`📄 Initial load - showing ${data.length} of ${serverTotalCount} total records`)
        
        if (data.length === 0) {
          console.warn('⚠️ No data returned from API, but request was successful')
        }
      } else {
        console.warn('⚠️ No schema available, cannot load data')
      }
      
      return true // Return success
    } catch (error) {
      console.error('❌ Failed to load initial data:', error)
      toast({
        title: 'Error',
        description: 'Failed to load initial data',
        variant: 'destructive'
      })
      return false // Return failure
    } finally {
      setLoading(false)
    }
  }

  const loadPagedData = async () => {
    try {
      // Preserve search input focus during loading
      const wasSearchFocused = document.activeElement === searchInputRef.current
      const cursorPosition = searchInputRef.current?.selectionStart || 0
      
      // Only show loading for initial loads, not for search updates
      if (!records.length) {
        setLoading(true)
      }

      console.log(`🔍 Loading page ${currentPage} with pageSize ${pageSize}, search: "${debouncedSearchTerm}", filter: "${typeFilter}"`)
      
      // Use proper server-side pagination
      const { data, totalCount: serverTotalCount } = await fullyDynamicUOMService.getPagedUOMData(
        currentPage,
        pageSize,
        debouncedSearchTerm,
        typeFilter.join(',')
      )

      setRecords(data)
      setFilteredRecords(data)
      setTotalCount(serverTotalCount)

      console.log(`📄 Loaded page ${currentPage} - showing ${data.length} of ${serverTotalCount} total records`)
      console.log(`📊 Data sample:`, data.slice(0, 2))
      
      // Restore search input focus and cursor position if it was focused before
      if (wasSearchFocused && searchInputRef.current) {
        setTimeout(() => {
          searchInputRef.current?.focus()
          searchInputRef.current?.setSelectionRange(cursorPosition, cursorPosition)
        }, 0)
      }
      
      // Only show toast on manual refresh, not on automatic data loading
      // Remove the automatic toast to prevent spam
    } catch (error: any) {
      console.error('Failed to load paged data:', error)
      toast({
        title: 'Error',
        description: `Failed to load data: ${error?.message || 'Unknown error'}`,
        variant: 'destructive'
      })
      
      setRecords([])
      setFilteredRecords([])
      setTotalCount(0)
    } finally {
      setLoading(false)
    }
  }

  // Note: Client-side filtering removed - now using server-side pagination

  // Refresh data function
  const refreshData = () => {
    loadPagedData()
  }

  const handleView = (record: UOMRecord) => {
    setSelectedRecord(record)
    setShowViewDialog(true)
  }

  const handleEdit = (record: UOMRecord) => {
    setSelectedRecord(record)
    setShowEditDialog(true)
  }

  const handleDelete = async (record: UOMRecord) => {
    if (!schema) return

    const confirmMessage = `Are you sure you want to delete this unit of measure?\n\n` +
      `${schema.displayField}: ${record[schema.displayField] || 'Unknown'}`

    if (!window.confirm(confirmMessage)) return

    try {
      const identifier = record[schema.primaryKey]
      const success = await fullyDynamicUOMService.performOperation(
        tableName, 'delete', undefined, identifier
      )

      if (success) {
        toast({
          title: 'Success',
          description: 'Unit of measure deleted successfully',
        })
        refreshData() // Reload data
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: `Failed to delete: ${error.message}`,
        variant: 'destructive'
      })
    }
  }

  const handleExport = async () => {
    if (!schema || filteredRecords.length === 0) return

    try {
      // Create CSV with dynamic columns
      const headers = schema.fields
        .filter(f => f.category !== 'system-metadata')
        .map(f => f.displayName)

      const rows = filteredRecords.map(record =>
        schema.fields
          .filter(f => f.category !== 'system-metadata')
          .map(field => {
            const value = record[field.name]
            return value !== null && value !== undefined ? value.toString() : ''
          })
      )

      const csvContent = [
        headers.join(','),
        ...rows.map(row => row.join(','))
      ].join('\n')

      // Download file
      const blob = new Blob([csvContent], { type: 'text/csv' })
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `units-of-measure-export-${new Date().toISOString().split('T')[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)

      toast({
        title: 'Export Complete',
        description: `Exported ${filteredRecords.length} records`,
      })
    } catch (error) {
      toast({
        title: 'Export Failed',
        description: error.message,
        variant: 'destructive'
      })
    }
  }

  // Get unique categories from schema
  const availableCategories = schema ? 
    [...new Set(schema.fields.map(f => f.category))].sort() : []

  // Get display fields for table - dynamically show important fields
  const displayFields = schema ? 
    schema.fields.filter(f => 
      // Show non-system fields
      f.category !== 'system-metadata' && 
      // Exclude Id and UID fields
      f.name !== 'Id' &&
      f.name !== 'UID' &&
      // Limit to important categories for display
      ['identifier', 'basic', 'descriptive-text', 'status', 'unit-definition', 'sku-relationship'].includes(f.category)
    ).map(field => {
      // Rename SKUUID to Product Code
      if (field.name === 'SKUUID') {
        return {
          ...field,
          displayName: 'Product Code'
        }
      }
      return field
    }).slice(0, 8) : [] // Show first 8 important fields

  // Records are already paginated by server
  const paginatedRecords = filteredRecords

  // Helper function to render cell values based on field type
  function renderCellValue(value: any, field: any) {
    if (value === null || value === undefined || value === '') {
      return <span className="text-gray-400 italic">-</span>
    }

    // Special handling for SKU-related fields
    if (field.category === 'sku-relationship') {
      const strValue = value.toString()
      if (field.name === 'SKUUID') {
        return <span className="font-mono text-xs text-blue-600">{strValue}</span>
      }
      if (field.name === 'SKUCode') {
        return <span className="font-mono font-semibold">{strValue}</span>
      }
      if (field.name === 'SKUName') {
        return (
          <div className="max-w-[200px]">
            <span className="text-sm font-medium">{strValue.length > 25 ? `${strValue.substring(0, 25)}...` : strValue}</span>
          </div>
        )
      }
    }

    switch (field.type) {
      case 'boolean':
        return (
          <Badge variant={value ? 'default' : 'secondary'}>
            {value ? 'Active' : 'Inactive'}
          </Badge>
        )
      
      case 'date':
      case 'datetime':
        return formatDateToDayMonthYear(value, value.toString())
      
      case 'decimal':
      case 'number':
        const num = parseFloat(value)
        if (isNaN(num)) return value
        
        return (
          <span className="font-mono">
            {num.toLocaleString()}
            {field.uiHints?.suffix && (
              <span className="text-muted-foreground ml-1">
                {field.uiHints.suffix}
              </span>
            )}
          </span>
        )
      
      default:
        const strValue = value.toString()
        // Special styling for UOM codes
        if (field.name.toLowerCase().includes('uomcode')) {
          return <Badge variant="outline" className="font-mono">{strValue}</Badge>
        }
        return strValue.length > 30 ? 
          `${strValue.substring(0, 30)}...` : 
          strValue
    }
  }

  if (loading) {
    return (
      <div className="space-y-6">
        {/* Header Skeleton */}
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Units of Measure</h1>
            <p className="text-muted-foreground">
              Manage measurement units for your products
            </p>
          </div>
          <div className="flex gap-2">
            <Skeleton className="h-9 w-20" />
            <Skeleton className="h-9 w-20" />
            <Skeleton className="h-9 w-20" />
            <Skeleton className="h-9 w-24" />
          </div>
        </div>
        
        {/* Filters Skeleton */}
        <Card>
          <CardHeader>
            <div className="flex items-center gap-4">
              <Skeleton className="h-9 w-64" />
              <Skeleton className="h-9 w-48" />
              <Skeleton className="h-6 w-32" />
            </div>
          </CardHeader>
        </Card>

        {/* Table Skeleton */}
        <Card>
          <CardContent className="p-6">
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead><Skeleton className="h-4 w-24" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-32" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-20" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-16" /></TableHead>
                    <TableHead><Skeleton className="h-4 w-20" /></TableHead>
                    <TableHead className="text-right"><Skeleton className="h-4 w-16 ml-auto" /></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {[...Array(10)].map((_, index) => (
                    <TableRow key={`skeleton-${index}`}>
                      <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                      <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                      <TableCell className="text-right">
                        <Skeleton className="h-8 w-8 rounded ml-auto" />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
            
            {/* Pagination Skeleton */}
            <div className="flex items-center justify-between mt-4">
              <Skeleton className="h-4 w-32" />
              <div className="flex gap-2">
                <Skeleton className="h-8 w-8" />
                <Skeleton className="h-8 w-8" />
                <Skeleton className="h-8 w-8" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  if (!schema) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Units of Measure</h1>
            <p className="text-muted-foreground">
              Manage measurement units for your products
            </p>
          </div>
        </div>
        
        <Card>
          <CardContent className="p-12">
            <div className="text-center text-red-500">
              <AlertCircle className="h-12 w-12 mx-auto mb-4" />
              <h3 className="text-lg font-semibold mb-2">Schema Discovery Failed</h3>
              <p className="text-muted-foreground mb-4">
                Unable to discover schema for table: {tableName}
              </p>
              <Button onClick={refreshData}>
                <RefreshCw className="h-4 w-4 mr-2" />
                Retry
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Units of Measure</h1>
          <p className="text-muted-foreground">
            Manage measurement units for your products
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleExport}
            disabled={filteredRecords.length === 0}
          >
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={refreshData}
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
            {/* Hide Add Unit button
            <DialogTrigger asChild>
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Add Unit
              </Button>
            </DialogTrigger>
            */}
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Create New Unit of Measure</DialogTitle>
              </DialogHeader>
              <DynamicFormGenerator
                tableName={tableName}
                onSave={() => {
                  setShowCreateDialog(false)
                  refreshData()
                }}
                onCancel={() => setShowCreateDialog(false)}
              />
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3 px-4">
          <div className="flex items-center gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by SKU ID (e.g., PCM00TIC01)... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 h-9 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* UOM Type Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" className="h-9 px-3">
                  <Filter className="h-4 w-4 mr-2" />
                  UOM Type
                  {typeFilter.length > 0 && (
                    <Badge variant="secondary" className="ml-2 px-1.5 py-0 text-xs">
                      {typeFilter.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-1" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by UOM Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={typeFilter.includes("base-uom")}
                  onCheckedChange={(checked) => {
                    setTypeFilter(prev => 
                      checked 
                        ? [...prev, "base-uom"]
                        : prev.filter(s => s !== "base-uom")
                    )
                    setCurrentPage(1) // Reset to first page when filter changes
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-blue-500 rounded-full" />
                    Base UOM
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={typeFilter.includes("outer-uom")}
                  onCheckedChange={(checked) => {
                    setTypeFilter(prev => 
                      checked 
                        ? [...prev, "outer-uom"]
                        : prev.filter(s => s !== "outer-uom")
                    )
                    setCurrentPage(1) // Reset to first page when filter changes
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-green-500 rounded-full" />
                    Outer UOM
                  </div>
                </DropdownMenuCheckboxItem>
                {typeFilter.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => {
                        setTypeFilter([])
                        setCurrentPage(1)
                      }}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          <Table>
            <TableHeader>
              <TableRow className="bg-gray-50/50">
                {displayFields.map((field, index) => (
                  <TableHead 
                    key={field.name}
                    className={`
                      ${index === 0 ? "pl-6" : ""}
                      ${(field.name === 'IsBaseUOM' || field.name === 'IsBaseUom' || 
                         field.name === 'IsOuterUOM' || field.name === 'IsOuterUom') ? "text-center" : ""}
                    `}
                  >
                    {field.displayName}
                    {field.isRequired && (
                      <span className="text-red-500 ml-1">*</span>
                    )}
                  </TableHead>
                ))}
                {/* Hide Actions column
                <TableHead className="text-right pr-6">Actions</TableHead>
                */}
              </TableRow>
            </TableHeader>
            <TableBody>
              {paginatedRecords.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={displayFields.length + 1} className="text-center py-12">
                    <div className="flex flex-col items-center gap-2">
                      <Database className="h-8 w-8 text-gray-400" />
                      <p className="text-gray-500 font-medium">
                        {records.length === 0 
                          ? 'No units of measure found' 
                          : 'No units match your current filters'
                        }
                      </p>
                      <p className="text-sm text-gray-400">
                        {records.length === 0 
                          ? 'Try adjusting your search criteria or add new units' 
                          : 'Try different filter options'
                        }
                      </p>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                paginatedRecords.map((record, index) => {
                  const uniqueKey = record.Id || record.UID || record[schema.primaryKey] || `fallback-${index}-${Math.random().toString(36).substring(2, 9)}`;
                  
                  return (
                    <TableRow 
                      key={`uom-row-${uniqueKey}-idx${index}`}
                      className="hover:bg-gray-50/50 transition-colors"
                    >
                      {displayFields.map((field, fieldIndex) => (
                        <TableCell 
                          key={`uom-cell-${index}-${fieldIndex}-${field.name}`}
                          className={`
                            ${fieldIndex === 0 ? "pl-6" : ""}
                            ${(field.name === 'IsBaseUOM' || field.name === 'IsBaseUom' || 
                               field.name === 'IsOuterUOM' || field.name === 'IsOuterUom') ? "text-center" : ""}
                          `}
                        >
                          {field.name === 'Code' || field.name === 'SKUUID' ? (
                            <span className="font-medium text-sm">
                              {renderCellValue(record[field.name], field)}
                            </span>
                          ) : field.name === 'IsBaseUOM' || field.name === 'IsBaseUom' ? (
                            <div className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${
                              record[field.name] 
                                ? 'bg-blue-100 text-blue-800' 
                                : 'bg-gray-100 text-gray-600'
                            }`}>
                              {record[field.name] ? (
                                <>
                                  <Check className="h-3 w-3" />
                                  <span>Base</span>
                                </>
                              ) : (
                                <>
                                  <X className="h-3 w-3" />
                                  <span>Not Base</span>
                                </>
                              )}
                            </div>
                          ) : field.name === 'IsOuterUOM' || field.name === 'IsOuterUom' ? (
                            <div className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${
                              record[field.name] 
                                ? 'bg-green-100 text-green-800' 
                                : 'bg-gray-100 text-gray-600'
                            }`}>
                              {record[field.name] ? (
                                <>
                                  <Check className="h-3 w-3" />
                                  <span>Outer</span>
                                </>
                              ) : (
                                <>
                                  <X className="h-3 w-3" />
                                  <span>Not Outer</span>
                                </>
                              )}
                            </div>
                          ) : field.name === 'Multiplier' ? (
                            <Badge 
                              variant="outline" 
                              className="text-xs"
                            >
                              {record[field.name] || 1}x
                            </Badge>
                          ) : (
                            <span className="text-sm">
                              {renderCellValue(record[field.name], field)}
                            </span>
                          )}
                        </TableCell>
                      ))}
                      {/* Hide Actions cell
                      <TableCell className="text-right pr-6">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" className="h-8 w-8 p-0">
                              <span className="sr-only">Open menu</span>
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuItem
                              onClick={() => handleView(record)}
                              className="cursor-pointer"
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => handleEdit(record)}
                              className="cursor-pointer"
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit Unit
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() => handleDelete(record)}
                              className="cursor-pointer text-red-600 focus:text-red-600"
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete Unit
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                      */}
                    </TableRow>
                  )
                })
              )}
            </TableBody>
          </Table>
        </div>

        {totalCount > 0 && (
          <div className="px-6 py-4 border-t bg-gray-50/30">
            <PaginationControls
              currentPage={currentPage}
              totalCount={totalCount}
              pageSize={pageSize}
              onPageChange={(page) => {
                setCurrentPage(page)
                window.scrollTo({ top: 0, behavior: "smooth" })
              }}
              onPageSizeChange={(size) => {
                setPageSize(size)
                setCurrentPage(1) // Reset to first page when changing page size
              }}
              itemName="units"
            />
          </div>
        )}
      </Card>

      {/* Edit Dialog */}
      <Dialog open={showEditDialog} onOpenChange={setShowEditDialog}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              Edit Unit: {selectedRecord?.[schema.displayField]}
            </DialogTitle>
          </DialogHeader>
          {selectedRecord && (
            <DynamicFormGenerator
              tableName={tableName}
              recordId={selectedRecord[schema.primaryKey]}
              onSave={() => {
                setShowEditDialog(false)
                setSelectedRecord(null)
                refreshData()
              }}
              onCancel={() => {
                setShowEditDialog(false)
                setSelectedRecord(null)
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* View Dialog */}
      <Dialog open={showViewDialog} onOpenChange={setShowViewDialog}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              View Unit: {selectedRecord?.[schema.displayField]}
            </DialogTitle>
          </DialogHeader>
          {selectedRecord && (
            <DynamicFormGenerator
              tableName={tableName}
              recordId={selectedRecord[schema.primaryKey]}
              readonly={true}
              onCancel={() => {
                setShowViewDialog(false)
                setSelectedRecord(null)
              }}
            />
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}