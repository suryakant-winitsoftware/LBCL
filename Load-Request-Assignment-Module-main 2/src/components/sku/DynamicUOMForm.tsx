/**
 * Dynamic UOM Form Component
 * Automatically discovers ALL fields from temp_sku_uom table
 * Adapts when new columns are added to database
 * Shows all available UOM fields during product creation
 */

import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, RefreshCw } from 'lucide-react';
import { dynamicTableFieldsService, type DynamicTableSchema, type TableFieldDefinition } from '@/services/sku/dynamic-table-fields.service';

interface DynamicUOMFormProps {
  skuUid: string;
  onSave?: (formData: any) => void;
  onCancel?: () => void;
}

export const DynamicUOMForm: React.FC<DynamicUOMFormProps> = ({
  skuUid,
  onSave,
  onCancel
}) => {
  const [schema, setSchema] = useState<DynamicTableSchema | null>(null);
  const [formData, setFormData] = useState<{ [key: string]: any }>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  // Load dynamic schema on component mount
  useEffect(() => {
    loadSchema();
  }, []);

  /**
   * Load table schema dynamically
   */
  const loadSchema = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const tableSchema = await dynamicTableFieldsService.getTableSchema('temp_sku_uom');
      setSchema(tableSchema);
      
      // Initialize form data with default values
      const initialData: { [key: string]: any } = { sku_uid: skuUid };
      tableSchema.fields.forEach(field => {
        initialData[field.name] = field.defaultValue;
      });
      setFormData(initialData);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load UOM fields');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Refresh schema to discover new fields
   */
  const refreshSchema = async () => {
    try {
      setRefreshing(true);
      
      // Clear cache and reload
      dynamicTableFieldsService.clearCache('temp_sku_uom');
      await loadSchema();
      
    } catch (err) {
      setError('Failed to refresh schema');
    } finally {
      setRefreshing(false);
    }
  };

  /**
   * Handle field value change
   */
  const handleFieldChange = (fieldName: string, value: any) => {
    setFormData(prev => ({
      ...prev,
      [fieldName]: value
    }));
  };

  /**
   * Handle form submission
   */
  const handleSave = () => {
    if (onSave) {
      onSave(formData);
    }
  };

  /**
   * Render field based on its type
   */
  const renderField = (field: TableFieldDefinition) => {
    const value = formData[field.name] || '';

    switch (field.type) {
      case 'boolean':
        return (
          <div className="flex items-center space-x-2">
            <Switch
              id={field.name}
              checked={!!value}
              onCheckedChange={(checked) => handleFieldChange(field.name, checked)}
            />
            <Label htmlFor={field.name}>{field.label}</Label>
            {field.required && <span className="text-red-500">*</span>}
          </div>
        );

      case 'select':
        return (
          <div className="space-y-2">
            <Label htmlFor={field.name}>
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Select
              value={value}
              onValueChange={(newValue) => handleFieldChange(field.name, newValue)}
            >
              <SelectTrigger>
                <SelectValue placeholder={`Select ${field.label}`} />
              </SelectTrigger>
              <SelectContent>
                {field.options?.map((option) => (
                  <SelectItem key={option} value={option}>
                    {option}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        );

      case 'number':
        return (
          <div className="space-y-2">
            <Label htmlFor={field.name}>
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Input
              id={field.name}
              type="number"
              value={value}
              onChange={(e) => handleFieldChange(field.name, parseFloat(e.target.value) || 0)}
              placeholder={`Enter ${field.label}`}
              step="any"
              min="0"
            />
          </div>
        );

      case 'text':
      default:
        return (
          <div className="space-y-2">
            <Label htmlFor={field.name}>
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Input
              id={field.name}
              type="text"
              value={value}
              onChange={(e) => handleFieldChange(field.name, e.target.value)}
              placeholder={`Enter ${field.label}`}
            />
          </div>
        );
    }
  };

  /**
   * Group fields by category for tabbed interface
   */
  const groupFieldsByCategory = () => {
    if (!schema) return {};

    const grouped: { [category: string]: TableFieldDefinition[] } = {};
    
    schema.fields.forEach(field => {
      if (!grouped[field.category]) {
        grouped[field.category] = [];
      }
      grouped[field.category].push(field);
    });

    return grouped;
  };

  // Loading state
  if (loading) {
    return (
      <Card className="w-full">
        <CardContent className="p-6">
          <div className="flex items-center justify-center space-x-2">
            <Loader2 className="h-4 w-4 animate-spin" />
            <span>Discovering UOM fields dynamically from database...</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Error state
  if (error) {
    return (
      <Card className="w-full">
        <CardContent className="p-6">
          <Alert variant="destructive">
            <AlertDescription>
              {error}
              <Button 
                variant="outline" 
                size="sm" 
                onClick={loadSchema}
                className="ml-2"
              >
                Retry
              </Button>
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  if (!schema) {
    return null;
  }

  // Handle empty schema (no fields discovered yet)
  if (schema.fields.length === 0) {
    return (
      <Card className="w-full">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>UOM Configuration - Dynamic Fields</CardTitle>
            <Button
              variant="outline"
              size="sm"
              onClick={refreshSchema}
              disabled={refreshing}
            >
              <RefreshCw className={`h-4 w-4 mr-2 ${refreshing ? 'animate-spin' : ''}`} />
              Discover Fields
            </Button>
          </div>
        </CardHeader>
        <CardContent className="p-6">
          <Alert>
            <AlertDescription>
              No UOM fields discovered yet. The system will automatically discover all available fields from your database.
              <br />
              Click "Discover Fields" to scan the database structure and populate fields dynamically.
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  const fieldsByCategory = groupFieldsByCategory();
  const categories = Object.keys(fieldsByCategory);

  return (
    <Card className="w-full">
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>UOM Configuration - Dynamic Fields</CardTitle>
          <Button
            variant="outline"
            size="sm"
            onClick={refreshSchema}
            disabled={refreshing}
          >
            <RefreshCw className={`h-4 w-4 mr-2 ${refreshing ? 'animate-spin' : ''}`} />
            Refresh Fields
          </Button>
        </div>
        <p className="text-sm text-gray-600">
          {schema.fields.length} fields discovered dynamically from {schema.tableName} table
        </p>
      </CardHeader>
      
      <CardContent className="p-6">
        {categories.length > 1 ? (
          <Tabs defaultValue={categories[0]} className="w-full">
            <TabsList className="grid w-full grid-cols-auto">
              {categories.map(category => (
                <TabsTrigger key={category} value={category}>
                  {schema.categories[category]}
                </TabsTrigger>
              ))}
            </TabsList>
            
            {categories.map(category => (
              <TabsContent key={category} value={category} className="mt-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {fieldsByCategory[category].map(field => (
                    <div key={field.name}>
                      {renderField(field)}
                    </div>
                  ))}
                </div>
              </TabsContent>
            ))}
          </Tabs>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {schema.fields.map(field => (
              <div key={field.name}>
                {renderField(field)}
              </div>
            ))}
          </div>
        )}

        <div className="flex justify-end space-x-2 mt-6">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button onClick={handleSave}>
            Save UOM Configuration
          </Button>
        </div>

        {/* Debug info for development */}
        {process.env.NODE_ENV === 'development' && (
          <details className="mt-6 p-4 bg-gray-50 rounded">
            <summary className="cursor-pointer text-sm font-medium">
              Debug: Dynamic Schema Info
            </summary>
            <pre className="mt-2 text-xs overflow-auto">
              {JSON.stringify({ schema, formData }, null, 2)}
            </pre>
          </details>
        )}
      </CardContent>
    </Card>
  );
};