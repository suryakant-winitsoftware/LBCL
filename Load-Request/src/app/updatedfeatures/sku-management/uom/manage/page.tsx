'use client'

// 100% Dynamic UOM Management - Adapts to ANY database structure
import React, { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { SafeSelect, SafeSelectItem } from '@/components/ui/safe-select'
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
  AlertCircle
} from 'lucide-react'

import DynamicFormGenerator from '@/components/dynamic/DynamicFormGenerator'
import { 
  fullyDynamicUOMService, 
  DynamicTableSchema 
} from '@/services/sku/fully-dynamic-uom.service'

interface UOMRecord {
  [key: string]: any
}

export default function DynamicUOMManagePage() {
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

  // Dynamic table name - can be changed to any table
  const tableName = 'SKUUOM' // This could come from props or config

  // Load data dynamically
  useEffect(() => {
    loadDynamicData()
  }, [])

  // Filter records when search or category changes
  useEffect(() => {
    filterRecords()
  }, [searchTerm, selectedCategory, records])

  const loadDynamicData = async () => {
    try {
      setLoading(true)

      // Discover table schema dynamically
      const tableSchema = await fullyDynamicUOMService.getTableSchema(tableName)
      setSchema(tableSchema)

      // Load all data dynamically
      const allData = await fullyDynamicUOMService.getAllUOMData()
      setRecords(allData)
      setFilteredRecords(allData)

      toast({
        title: 'Data Loaded',
        description: `Loaded ${allData.length} records with ${tableSchema.fields.length} fields`,
      })
    } catch (error) {
      console.error('Failed to load data:', error)
      toast({
        title: 'Error',
        description: `Failed to load data: ${error.message}`,
        variant: 'destructive'
      })
    } finally {
      setLoading(false)
    }
  }

  const filterRecords = () => {
    if (!schema) return

    let filtered = records

    // Dynamic search across all string fields
    if (searchTerm) {
      const searchLower = searchTerm.toLowerCase()
      const searchableFields = schema.fields.filter(f => 
        f.type === 'string' && f.category !== 'system-metadata'
      )

      filtered = filtered.filter(record =>
        searchableFields.some(field => {
          const value = record[field.name]
          return value && value.toString().toLowerCase().includes(searchLower)
        })
      )
    }

    // Dynamic category filtering
    if (selectedCategory && selectedCategory !== 'all-categories') {
      const categoryFields = schema.fields.filter(f => f.category === selectedCategory)
      
      // Show records that have non-empty values in this category
      filtered = filtered.filter(record =>
        categoryFields.some(field => {
          const value = record[field.name]
          return value !== null && value !== undefined && value !== ''
        })
      )
    }

    setFilteredRecords(filtered)
  }

  const handleDelete = async (record: UOMRecord) => {
    if (!schema) return

    const confirmMessage = `Are you sure you want to delete this ${tableName}?\n\n` +
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
          description: 'Record deleted successfully',
        })
        loadDynamicData() // Reload data
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
      a.download = `${tableName}_export_${new Date().toISOString().split('T')[0]}.csv`
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

  // Get display fields for table
  const displayFields = schema ? 
    schema.fields.filter(f => 
      f.category !== 'system-metadata' && 
      !f.isReadonly &&
      f.type !== 'json'
    ).slice(0, 6) : [] // Show first 6 relevant fields

  if (loading) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="p-12">
            <div className="flex items-center justify-center space-x-2">
              <RefreshCw className="h-8 w-8 animate-spin text-blue-600" />
              <div className="text-center">
                <h3 className="text-lg font-semibold">Loading Dynamic Schema</h3>
                <p className="text-muted-foreground">
                  Discovering table structure and loading data...
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  if (!schema) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="p-12">
            <div className="text-center text-red-500">
              <AlertCircle className="h-12 w-12 mx-auto mb-4" />
              <h3 className="text-lg font-semibold mb-2">Schema Discovery Failed</h3>
              <p className="text-muted-foreground mb-4">
                Unable to discover schema for table: {tableName}
              </p>
              <Button onClick={loadDynamicData}>
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
    <div className="container mx-auto p-6 space-y-6">
      {/* Dynamic Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div>
            <h1 className="text-3xl font-bold">
              {schema.tableName} Management
            </h1>
            <p className="text-muted-foreground">
              Manage {schema.fields.length} dynamic fields across {records.length} records
            </p>
          </div>
        </div>
        
        <div className="flex items-center gap-2">
          <Badge variant="outline" className="gap-1">
            <Database className="h-3 w-3" />
            {schema.metadata.confidence ? 
              `${Math.round(schema.metadata.confidence * 100)}% confidence` : 
              'Dynamic schema'
            }
          </Badge>
        </div>
      </div>

      {/* Dynamic Controls */}
      <Card>
        <CardHeader>
          <div className="flex flex-wrap items-center justify-between gap-4">
            <div className="flex items-center gap-4">
              {/* Dynamic Search */}
              <div className="flex items-center gap-2">
                <Search className="h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search across all fields..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-64"
                />
              </div>

              {/* Dynamic Category Filter */}
              <div className="flex items-center gap-2">
                <Filter className="h-4 w-4 text-muted-foreground" />
                <SafeSelect 
                  value={selectedCategory} 
                  onValueChange={setSelectedCategory}
                  placeholder="Filter by category"
                  className="w-48"
                >
                  <SafeSelectItem value="all-categories">All Categories</SafeSelectItem>
                  {availableCategories.filter(category => category && category.trim() !== '').map((category, catIndex) => (
                    <SafeSelectItem key={`uom-category-${catIndex}-${category}`} value={category}>
                      {category.split('-').map(word => 
                        word.charAt(0).toUpperCase() + word.slice(1)
                      ).join(' ')}
                    </SafeSelectItem>
                  ))}
                </SafeSelect>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-2">
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
                onClick={loadDynamicData}
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>

              <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
                <DialogTrigger asChild>
                  <Button size="sm">
                    <Plus className="h-4 w-4 mr-2" />
                    Create New
                  </Button>
                </DialogTrigger>
                <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
                  <DialogHeader>
                    <DialogTitle>Create New {tableName}</DialogTitle>
                  </DialogHeader>
                  <DynamicFormGenerator
                    tableName={tableName}
                    onSave={() => {
                      setShowCreateDialog(false)
                      loadDynamicData()
                    }}
                    onCancel={() => setShowCreateDialog(false)}
                  />
                </DialogContent>
              </Dialog>
            </div>
          </div>
        </CardHeader>
      </Card>

      {/* Results Summary */}
      <div className="flex items-center gap-4 text-sm text-muted-foreground">
        <span>
          Showing {filteredRecords.length} of {records.length} records
        </span>
        {searchTerm && (
          <Badge variant="secondary">
            Search: "{searchTerm}"
          </Badge>
        )}
        {selectedCategory && selectedCategory !== 'all-categories' && (
          <Badge variant="secondary">
            Category: {selectedCategory.replace('-', ' ')}
          </Badge>
        )}
      </div>

      {/* Dynamic Data Grid */}
      <Card>
        <CardContent className="p-0">
          {filteredRecords.length === 0 ? (
            <div className="p-12 text-center text-muted-foreground">
              <Database className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <h3 className="text-lg font-semibold mb-2">No Records Found</h3>
              <p>
                {records.length === 0 
                  ? 'No records exist in this table yet.' 
                  : 'No records match your current filters.'
                }
              </p>
              {records.length === 0 && (
                <Button 
                  onClick={() => setShowCreateDialog(true)}
                  className="mt-4"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Create First Record
                </Button>
              )}
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="border-b bg-gray-50">
                  <tr>
                    {displayFields.map(field => (
                      <th key={field.name} className="text-left p-4 font-medium">
                        {field.displayName}
                        {field.isRequired && (
                          <span className="text-red-500 ml-1">*</span>
                        )}
                      </th>
                    ))}
                    <th className="text-left p-4 font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredRecords.map((record, index) => {
                    // Create a truly unique key by combining multiple identifiers
                    // Prioritize numeric Id, then UID, then schema primary key, then fallback
                    const uniqueKey = record.Id || record.UID || record[schema.primaryKey] || `fallback-${index}-${Math.random().toString(36).substring(2, 9)}`;
                    
                    // Keys are now properly unique using numeric Id + index
                    
                    return (
                    <tr key={`uom-row-${uniqueKey}-idx${index}`} className="border-b hover:bg-gray-50">
                      {displayFields.map((field, fieldIndex) => (
                        <td key={`uom-cell-${index}-${fieldIndex}-${field.name}`} className="p-4">
                          {renderCellValue(record[field.name], field)}
                        </td>
                      ))}
                      <td className="p-4">
                        <div className="flex items-center gap-2">
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => {
                              setSelectedRecord(record)
                              setShowViewDialog(true)
                            }}
                          >
                            <Eye className="h-3 w-3" />
                          </Button>
                          
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => {
                              setSelectedRecord(record)
                              setShowEditDialog(true)
                            }}
                          >
                            <Edit className="h-3 w-3" />
                          </Button>
                          
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => handleDelete(record)}
                            className="text-red-600 hover:text-red-700"
                          >
                            <Trash2 className="h-3 w-3" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Dialog */}
      <Dialog open={showEditDialog} onOpenChange={setShowEditDialog}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              Edit {tableName}: {selectedRecord?.[schema.displayField]}
            </DialogTitle>
          </DialogHeader>
          {selectedRecord && (
            <DynamicFormGenerator
              tableName={tableName}
              recordId={selectedRecord[schema.primaryKey]}
              onSave={() => {
                setShowEditDialog(false)
                setSelectedRecord(null)
                loadDynamicData()
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
              View {tableName}: {selectedRecord?.[schema.displayField]}
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

  // Helper function to render cell values based on field type
  function renderCellValue(value: any, field: any) {
    if (value === null || value === undefined || value === '') {
      return <span className="text-gray-400 italic">-</span>
    }

    switch (field.type) {
      case 'boolean':
        return (
          <Badge variant={value ? 'default' : 'secondary'}>
            {value ? 'Yes' : 'No'}
          </Badge>
        )
      
      case 'date':
      case 'datetime':
        try {
          const date = new Date(value)
          return date.toLocaleDateString()
        } catch {
          return value.toString()
        }
      
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
        return strValue.length > 30 ? 
          `${strValue.substring(0, 30)}...` : 
          strValue
    }
  }
}