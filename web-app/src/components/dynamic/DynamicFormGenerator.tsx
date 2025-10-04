'use client'

// 100% Dynamic Form Generator - Adapts to ANY schema
import React, { useState, useEffect, useMemo } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { SafeSelect, SafeSelectItem, cleanSelectOptions } from '@/components/ui/safe-select'
import { Textarea } from '@/components/ui/textarea'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible'
import { useToast } from '@/components/ui/use-toast'
import { ChevronDown, ChevronRight, Save, RefreshCw, Database, Info, AlertCircle } from 'lucide-react'
import { 
  fullyDynamicUOMService, 
  DynamicTableSchema, 
  DynamicField 
} from '@/services/sku/fully-dynamic-uom.service'

interface DynamicFormGeneratorProps {
  tableName: string
  recordId?: string // For editing existing records
  onSave?: (data: any) => void
  onCancel?: () => void
  readonly?: boolean
}

interface FormState {
  [key: string]: any
}

interface ValidationState {
  [key: string]: string[]
}

export function DynamicFormGenerator({
  tableName,
  recordId,
  onSave,
  onCancel,
  readonly = false
}: DynamicFormGeneratorProps) {
  const { toast } = useToast()
  
  // Dynamic state - no hardcoded fields
  const [schema, setSchema] = useState<DynamicTableSchema | null>(null)
  const [formData, setFormData] = useState<FormState>({})
  const [validationErrors, setValidationErrors] = useState<ValidationState>({})
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set(['identifier']))

  // Dynamically group fields by category
  const fieldsByCategory = useMemo(() => {
    if (!schema) return {}
    
    const categories: Record<string, DynamicField[]> = {}
    
    schema.fields.forEach(field => {
      if (!categories[field.category]) {
        categories[field.category] = []
      }
      categories[field.category].push(field)
    })
    
    return categories
  }, [schema])

  // Category display information - generated dynamically
  const categoryInfo = useMemo(() => {
    if (!schema) return {}
    
    const info: Record<string, { title: string; description: string; icon: any; priority: number }> = {}
    
    Object.keys(fieldsByCategory).forEach((category, index) => {
      const fields = fieldsByCategory[category]
      const fieldCount = fields.length
      const requiredCount = fields.filter(f => f.isRequired).length
      
      info[category] = {
        title: category.split('-').map(word => 
          word.charAt(0).toUpperCase() + word.slice(1)
        ).join(' '),
        description: `${fieldCount} fields (${requiredCount} required)`,
        icon: index % 3 === 0 ? Database : index % 3 === 1 ? Info : AlertCircle,
        priority: fields.some(f => f.isRequired) ? 1 : 2
      }
    })
    
    return info
  }, [fieldsByCategory])

  // Load schema and data
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true)
        
        // Load schema dynamically
        const tableSchema = await fullyDynamicUOMService.getTableSchema(tableName)
        setSchema(tableSchema)
        
        // Initialize form data with defaults
        const initialData: FormState = {}
        tableSchema.fields.forEach(field => {
          if (field.metadata.defaultValue !== undefined) {
            initialData[field.name] = field.metadata.defaultValue
          } else {
            // Set type-appropriate defaults
            switch (field.type) {
              case 'boolean':
                initialData[field.name] = false
                break
              case 'number':
              case 'decimal':
                initialData[field.name] = 0
                break
              case 'datetime':
              case 'date':
                if (field.name.toLowerCase().includes('created') || 
                    field.name.toLowerCase().includes('modified')) {
                  initialData[field.name] = new Date().toISOString()
                } else {
                  initialData[field.name] = ''
                }
                break
              default:
                initialData[field.name] = ''
            }
          }
        })
        
        // Load existing record if editing
        if (recordId) {
          try {
            const existingData = await fullyDynamicUOMService.performOperation(
              tableName, 'read', undefined, recordId
            )
            if (existingData) {
              // Merge existing data with defaults
              Object.keys(existingData).forEach(key => {
                initialData[key] = existingData[key]
              })
            }
          } catch (error) {
            toast({
              title: 'Warning',
              description: 'Could not load existing record data',
              variant: 'default'
            })
          }
        }
        
        setFormData(initialData)
        
        // Auto-expand required sections
        const requiredCategories = Object.keys(fieldsByCategory).filter(category => 
          fieldsByCategory[category].some(field => field.isRequired)
        )
        setExpandedSections(new Set(requiredCategories))
        
      } catch (error) {
        console.error('Failed to load schema:', error)
        toast({
          title: 'Error',
          description: `Failed to load form schema: ${error.message}`,
          variant: 'destructive'
        })
      } finally {
        setLoading(false)
      }
    }
    
    loadData()
  }, [tableName, recordId])

  // Handle form field changes dynamically
  const handleFieldChange = (fieldName: string, value: any, field: DynamicField) => {
    setFormData(prev => ({
      ...prev,
      [fieldName]: value
    }))
    
    // Clear validation errors for this field
    setValidationErrors(prev => {
      const updated = { ...prev }
      delete updated[fieldName]
      return updated
    })

    // Handle dynamic field relationships
    handleFieldRelationships(fieldName, value, field)
  }

  // Handle dynamic field relationships and auto-fill logic
  const handleFieldRelationships = (fieldName: string, value: any, field: DynamicField) => {
    if (!schema) return

    // Auto-generate related fields based on patterns
    schema.fields.forEach(relatedField => {
      const relatedName = relatedField.name.toLowerCase()
      const currentName = fieldName.toLowerCase()

      // Auto-fill UID from Code
      if (currentName === 'code' && relatedName === 'uid' && value) {
        setFormData(prev => ({ ...prev, [relatedField.name]: value }))
      }

      // Auto-fill display names from primary name
      if (currentName.includes('name') && !currentName.includes('arabic') && 
          (relatedName.includes('longname') || relatedName.includes('aliasname')) && value) {
        setFormData(prev => ({ ...prev, [relatedField.name]: value }))
      }

      // Auto-calculate multiplier relationships
      if (currentName.includes('multiplier') && relatedName.includes('quantity') && value) {
        const multiplier = parseFloat(value) || 1
        setFormData(prev => ({ 
          ...prev, 
          [relatedField.name]: (parseFloat(prev[relatedField.name]) || 0) * multiplier 
        }))
      }
    })
  }

  // Validate form data dynamically
  const validateForm = async (): Promise<boolean> => {
    if (!schema) return false

    try {
      const validation = await fullyDynamicUOMService.validateUOMData(formData)
      
      if (!validation.isValid) {
        // Convert validation errors to field-specific errors
        const fieldErrors: ValidationState = {}
        validation.errors.forEach(error => {
          // Try to extract field name from error message
          const field = schema.fields.find(f => error.includes(f.displayName))
          const fieldName = field ? field.name : 'general'
          
          if (!fieldErrors[fieldName]) {
            fieldErrors[fieldName] = []
          }
          fieldErrors[fieldName].push(error)
        })
        
        setValidationErrors(fieldErrors)
        return false
      }

      setValidationErrors({})
      return true
    } catch (error) {
      toast({
        title: 'Validation Error',
        description: 'Failed to validate form data',
        variant: 'destructive'
      })
      return false
    }
  }

  // Save form data
  const handleSave = async () => {
    if (!schema) return

    const isValid = await validateForm()
    if (!isValid) {
      toast({
        title: 'Validation Failed',
        description: 'Please fix the errors before saving',
        variant: 'destructive'
      })
      return
    }

    setSaving(true)
    try {
      const operation = recordId ? 'update' : 'create'
      const result = await fullyDynamicUOMService.performOperation(
        tableName, operation, formData, recordId
      )

      if (result) {
        toast({
          title: 'Success',
          description: `Record ${operation}d successfully`,
        })
        
        if (onSave) {
          onSave(formData)
        }
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: `Failed to save: ${error.message}`,
        variant: 'destructive'
      })
    } finally {
      setSaving(false)
    }
  }

  // Render field dynamically based on its properties
  const renderField = (field: DynamicField) => {
    const value = formData[field.name]
    const errors = validationErrors[field.name]
    const hasError = errors && errors.length > 0
    const isDisabled = readonly || field.isReadonly || saving

    // Get UI component type from hints or field characteristics
    const componentType = field.uiHints?.component || getDefaultComponentType(field)

    return (
      <div key={field.name} className="space-y-2">
        <Label htmlFor={field.name} className="flex items-center gap-2">
          {field.displayName}
          {field.isRequired && <span className="text-red-500">*</span>}
          {field.metadata?.sampleValues && (
            <Badge variant="outline" className="text-xs">
              e.g., {field.metadata.sampleValues[0]}
            </Badge>
          )}
        </Label>

        {renderFieldInput(field, value, componentType, isDisabled)}

        {/* Field metadata */}
        {field.metadata?.patterns && field.metadata.patterns.length > 0 && (
          <div className="text-xs text-muted-foreground">
            Patterns: {field.metadata.patterns.join(', ')}
          </div>
        )}

        {/* Validation errors */}
        {hasError && (
          <div className="text-sm text-red-500">
            {errors.map((error, index) => (
              <div key={index}>{error}</div>
            ))}
          </div>
        )}
      </div>
    )
  }

  // Render appropriate input component
  const renderFieldInput = (field: DynamicField, value: any, componentType: string, isDisabled: boolean) => {
    const commonProps = {
      id: field.name,
      value: value || '',
      disabled: isDisabled,
    }

    switch (componentType) {
      case 'switch':
        return (
          <Switch
            checked={!!value}
            onCheckedChange={(checked) => handleFieldChange(field.name, checked, field)}
            disabled={isDisabled}
          />
        )

      case 'select':
        const validOptions = cleanSelectOptions(field.validationRules?.options || []);

        if (validOptions.length === 0) {
          // If no valid options, render as text input instead
          return (
            <Input
              {...commonProps}
              type="text"
              onChange={(e) => handleFieldChange(field.name, e.target.value, field)}
              placeholder={`Enter ${field.displayName.toLowerCase()}`}
            />
          );
        }

        // Debug logging for select components
        if (process.env.NODE_ENV === 'development' && validOptions.length > 0) {
          console.log('✅ Safe Select Field:', {
            fieldName: field.name,
            optionCount: validOptions.length,
            options: validOptions.slice(0, 5) // First 5 options for debugging
          });
        }

        return (
          <SafeSelect
            value={value && validOptions.includes(String(value)) ? String(value) : undefined}
            onValueChange={(newValue) => handleFieldChange(field.name, newValue, field)}
            disabled={isDisabled}
            placeholder={`Select ${field.displayName.toLowerCase()}`}
          >
            {validOptions.map((option: string, index: number) => (
              <SafeSelectItem key={`${field.name}-opt-${index}-${option.substring(0, 10)}`} value={option}>
                {option}
              </SafeSelectItem>
            ))}
          </SafeSelect>
        )

      case 'textarea':
        return (
          <Textarea
            {...commonProps}
            rows={3}
            onChange={(e) => handleFieldChange(field.name, e.target.value, field)}
            placeholder={`Enter ${field.displayName.toLowerCase()}`}
          />
        )

      case 'number':
        return (
          <div className="relative">
            <Input
              {...commonProps}
              type="number"
              step={field.type === 'decimal' ? '0.01' : '1'}
              min={field.validationRules?.min}
              max={field.validationRules?.max}
              onChange={(e) => handleFieldChange(field.name, parseFloat(e.target.value) || 0, field)}
              placeholder={`0${field.uiHints?.suffix ? ` ${field.uiHints.suffix}` : ''}`}
            />
            {field.uiHints?.suffix && (
              <div className="absolute right-3 top-1/2 transform -translate-y-1/2 text-sm text-muted-foreground">
                {field.uiHints.suffix}
              </div>
            )}
          </div>
        )

      case 'date':
        return (
          <Input
            {...commonProps}
            type="date"
            onChange={(e) => handleFieldChange(field.name, e.target.value, field)}
          />
        )

      case 'datetime':
        return (
          <Input
            {...commonProps}
            type="datetime-local"
            onChange={(e) => handleFieldChange(field.name, e.target.value, field)}
          />
        )

      default:
        return (
          <Input
            {...commonProps}
            type="text"
            onChange={(e) => handleFieldChange(field.name, e.target.value, field)}
            placeholder={`Enter ${field.displayName.toLowerCase()}`}
            pattern={field.validationRules?.pattern}
          />
        )
    }
  }

  // Get default component type based on field characteristics
  const getDefaultComponentType = (field: DynamicField): string => {
    // Check for valid select options (non-empty, reasonable count)
    const hasValidOptions = field.validationRules?.options && 
      Array.isArray(field.validationRules.options) &&
      field.validationRules.options.length > 0 && 
      field.validationRules.options.length <= 20 &&
      field.validationRules.options.some((opt: any) => opt !== null && opt !== undefined && String(opt).trim() !== '')
    
    if (hasValidOptions) return 'select'
    if (field.type === 'boolean') return 'switch'
    if (field.type === 'number' || field.type === 'decimal') return 'number'
    if (field.type === 'date') return 'date'
    if (field.type === 'datetime') return 'datetime'
    if (field.metadata?.maxLength > 255) return 'textarea'
    return 'text'
  }

  // Toggle section expansion
  const toggleSection = (category: string) => {
    setExpandedSections(prev => {
      const updated = new Set(prev)
      if (updated.has(category)) {
        updated.delete(category)
      } else {
        updated.add(category)
      }
      return updated
    })
  }

  if (loading) {
    return (
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-center space-x-2">
            <RefreshCw className="h-4 w-4 animate-spin" />
            <span>Loading dynamic form schema...</span>
          </div>
        </CardContent>
      </Card>
    )
  }

  if (!schema) {
    return (
      <Card>
        <CardContent className="p-6">
          <div className="text-center text-red-500">
            <AlertCircle className="h-8 w-8 mx-auto mb-2" />
            <h3 className="font-semibold">Schema Load Failed</h3>
            <p>Unable to load dynamic form schema for table: {tableName}</p>
          </div>
        </CardContent>
      </Card>
    )
  }

  // Sort categories by priority
  const sortedCategories = Object.keys(fieldsByCategory).sort((a, b) => {
    const priorityA = categoryInfo[a]?.priority || 3
    const priorityB = categoryInfo[b]?.priority || 3
    return priorityA - priorityB
  })

  return (
    <div className="space-y-6">
      {/* Form Header */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Database className="h-5 w-5" />
            {recordId ? 'Edit' : 'Create'} {schema.tableName}
          </CardTitle>
          <div className="text-sm text-muted-foreground space-y-1">
            <div>Table: {schema.tableName}</div>
            <div>Fields: {schema.fields.length} total</div>
            <div>Schema Confidence: {Math.round((schema.metadata.confidence || 0) * 100)}%</div>
          </div>
        </CardHeader>
      </Card>

      {/* Dynamic Form Sections */}
      <div className="space-y-4">
        {sortedCategories.map(category => {
          const fields = fieldsByCategory[category]
          const info = categoryInfo[category]
          const isExpanded = expandedSections.has(category)
          const IconComponent = info.icon

          return (
            <Card key={category}>
              <Collapsible open={isExpanded} onOpenChange={() => toggleSection(category)}>
                <CollapsibleTrigger asChild>
                  <CardHeader className="cursor-pointer hover:bg-gray-50 transition-colors">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <IconComponent className="h-5 w-5 text-blue-600" />
                        <div>
                          <CardTitle className="text-lg">{info.title}</CardTitle>
                          <div className="text-sm text-muted-foreground">{info.description}</div>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        {fields.some(f => validationErrors[f.name]) && (
                          <Badge variant="destructive" className="text-xs">
                            Errors
                          </Badge>
                        )}
                        {isExpanded ? (
                          <ChevronDown className="h-4 w-4" />
                        ) : (
                          <ChevronRight className="h-4 w-4" />
                        )}
                      </div>
                    </div>
                  </CardHeader>
                </CollapsibleTrigger>
                
                <CollapsibleContent>
                  <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                      {fields.map(renderField)}
                    </div>
                  </CardContent>
                </CollapsibleContent>
              </Collapsible>
            </Card>
          )
        })}
      </div>

      {/* Form Actions */}
      {!readonly && (
        <Card>
          <CardContent className="p-6">
            <div className="flex justify-end gap-4">
              <Button
                type="button"
                variant="outline"
                onClick={onCancel}
                disabled={saving}
              >
                Cancel
              </Button>
              <Button 
                onClick={handleSave}
                disabled={saving}
                className="bg-blue-600 hover:bg-blue-700"
              >
                <Save className="h-4 w-4 mr-2" />
                {saving ? 'Saving...' : recordId ? 'Update' : 'Create'}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

export default DynamicFormGenerator