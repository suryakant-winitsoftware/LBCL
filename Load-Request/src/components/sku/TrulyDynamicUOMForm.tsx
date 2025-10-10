/**
 * Truly Dynamic UOM Form - NO HARDCODING
 * Discovers ALL fields from API response
 */

import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, RefreshCw } from 'lucide-react';
import { trulyDynamicFieldsService, type DynamicField } from '@/services/sku/truly-dynamic-fields.service';

interface TrulyDynamicUOMFormProps {
  skuUid?: string;
  onSave?: (formData: any) => void;
  onCancel?: () => void;
}

export const TrulyDynamicUOMForm: React.FC<TrulyDynamicUOMFormProps> = ({
  skuUid,
  onSave,
  onCancel
}) => {
  const [fields, setFields] = useState<DynamicField[]>([]);
  const [formData, setFormData] = useState<{ [key: string]: any }>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    discoverFields();
  }, []);

  const discoverFields = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Discover fields from SKUUOM API
      const discoveredFields = await trulyDynamicFieldsService.discoverFieldsFromAPI('SKUUOM/SelectAllSKUUOMDetails');
      
      if (discoveredFields.length === 0) {
        setError('No fields discovered. The API might not be returning data.');
      } else {
        setFields(discoveredFields);
        
        // Initialize form data
        const initialData: { [key: string]: any } = {};
        discoveredFields.forEach(field => {
          initialData[field.name] = field.value || '';
        });
        setFormData(initialData);
      }
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to discover fields');
    } finally {
      setLoading(false);
    }
  };

  const handleFieldChange = (fieldName: string, value: any) => {
    setFormData(prev => ({
      ...prev,
      [fieldName]: value
    }));
  };

  const renderField = (field: DynamicField) => {
    const value = formData[field.name];

    switch (field.type) {
      case 'boolean':
        return (
          <div className="flex items-center space-x-2">
            <Switch
              id={field.name}
              checked={!!value}
              onCheckedChange={(checked) => handleFieldChange(field.name, checked)}
            />
            <Label htmlFor={field.name}>
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
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
            />
          </div>
        );

      case 'date':
        return (
          <div className="space-y-2">
            <Label htmlFor={field.name}>
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Input
              id={field.name}
              type="date"
              value={value}
              onChange={(e) => handleFieldChange(field.name, e.target.value)}
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

  if (loading) {
    return (
      <Card className="w-full">
        <CardContent className="p-6">
          <div className="flex items-center justify-center space-x-2">
            <Loader2 className="h-4 w-4 animate-spin" />
            <span>Discovering fields from API...</span>
          </div>
        </CardContent>
      </Card>
    );
  }

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
                onClick={discoverFields}
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

  return (
    <Card className="w-full">
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Dynamic UOM Configuration</CardTitle>
            <p className="text-sm text-gray-600 mt-1">
              {fields.length} fields discovered dynamically from API
            </p>
          </div>
          <Button
            variant="outline"
            size="sm"
            onClick={discoverFields}
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
        </div>
      </CardHeader>
      
      <CardContent className="p-6">
        {fields.length === 0 ? (
          <Alert>
            <AlertDescription>
              No fields discovered. Click Refresh to try again.
            </AlertDescription>
          </Alert>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {fields.map(field => (
                <div key={field.name}>
                  {renderField(field)}
                </div>
              ))}
            </div>

            <div className="flex justify-end space-x-2 mt-6">
              <Button variant="outline" onClick={onCancel}>
                Cancel
              </Button>
              <Button onClick={() => onSave?.(formData)}>
                Save Configuration
              </Button>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
};