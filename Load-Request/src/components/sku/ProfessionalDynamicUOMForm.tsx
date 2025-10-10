/**
 * Professional Dynamic UOM Form
 * Clean, modern UI with better user experience
 */

import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { 
  Loader2, 
  RefreshCw, 
  Package, 
  Ruler, 
  Weight, 
  Droplet,
  Settings,
  Info,
  Check,
  X,
  AlertCircle
} from 'lucide-react';
import { trulyDynamicFieldsService, type DynamicField } from '@/services/sku/truly-dynamic-fields.service';
import { cn } from '@/lib/utils';

interface ProfessionalDynamicUOMFormProps {
  skuUid?: string;
  onSave?: (formData: any) => void;
  onCancel?: () => void;
}

export const ProfessionalDynamicUOMForm: React.FC<ProfessionalDynamicUOMFormProps> = ({
  skuUid,
  onSave,
  onCancel
}) => {
  const [fields, setFields] = useState<DynamicField[]>([]);
  const [formData, setFormData] = useState<{ [key: string]: any }>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState('basic');
  const [hasChanges, setHasChanges] = useState(false);

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
        // If no fields from API, use a comprehensive default set
        setFields(getDefaultUOMFields());
      } else {
        setFields(discoveredFields);
      }
      
      // Initialize form data
      const initialData: { [key: string]: any } = {};
      (discoveredFields.length > 0 ? discoveredFields : getDefaultUOMFields()).forEach(field => {
        initialData[field.name] = field.value || getDefaultValue(field);
      });
      setFormData(initialData);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to discover fields');
      // Use default fields as fallback
      setFields(getDefaultUOMFields());
    } finally {
      setLoading(false);
    }
  };

  const getDefaultValue = (field: DynamicField) => {
    if (field.type === 'boolean') return false;
    if (field.type === 'number') return 0;
    return '';
  };

  const getDefaultUOMFields = (): DynamicField[] => {
    return [
      // Basic Information
      { name: 'Code', label: 'UOM Code', type: 'text', value: '', required: true },
      { name: 'Name', label: 'UOM Name', type: 'text', value: '', required: true },
      { name: 'Label', label: 'Display Label', type: 'text', value: '', required: true },
      { name: 'Barcodes', label: 'Barcodes', type: 'text', value: '', required: false },
      { name: 'Multiplier', label: 'Multiplier', type: 'number', value: 1, required: true },
      { name: 'IsBaseUOM', label: 'Base UOM', type: 'boolean', value: false, required: false },
      { name: 'IsOuterUOM', label: 'Outer UOM', type: 'boolean', value: false, required: false },
      
      // Dimensions
      { name: 'Length', label: 'Length', type: 'number', value: 0, required: false },
      { name: 'Width', label: 'Width', type: 'number', value: 0, required: false },
      { name: 'Height', label: 'Height', type: 'number', value: 0, required: false },
      { name: 'Depth', label: 'Depth', type: 'number', value: 0, required: false },
      { name: 'DimensionUnit', label: 'Dimension Unit', type: 'select', value: 'CM', required: false, options: ['CM', 'M', 'MM', 'IN', 'FT'] },
      
      // Weight
      { name: 'Weight', label: 'Net Weight', type: 'number', value: 0, required: false },
      { name: 'GrossWeight', label: 'Gross Weight', type: 'number', value: 0, required: false },
      { name: 'WeightUnit', label: 'Weight Unit', type: 'select', value: 'KG', required: false, options: ['KG', 'G', 'LB', 'OZ'] },
      { name: 'GrossWeightUnit', label: 'Gross Weight Unit', type: 'select', value: 'KG', required: false, options: ['KG', 'G', 'LB', 'OZ'] },
      
      // Volume
      { name: 'Volume', label: 'Volume', type: 'number', value: 0, required: false },
      { name: 'VolumeUnit', label: 'Volume Unit', type: 'select', value: 'L', required: false, options: ['L', 'ML', 'GAL', 'CMQ', 'MTQ'] },
      { name: 'Liter', label: 'Liters', type: 'number', value: 0, required: false },
      { name: 'KGM', label: 'KGM', type: 'number', value: 0, required: false }
    ];
  };

  const handleFieldChange = (fieldName: string, value: any) => {
    setFormData(prev => ({
      ...prev,
      [fieldName]: value
    }));
    setHasChanges(true);
  };

  const categorizeFields = () => {
    const categories = {
      basic: [] as DynamicField[],
      dimensions: [] as DynamicField[],
      weight: [] as DynamicField[],
      volume: [] as DynamicField[],
      other: [] as DynamicField[]
    };

    fields.forEach(field => {
      const name = field.name.toLowerCase();
      if (name.includes('code') || name.includes('name') || name.includes('label') || 
          name.includes('barcode') || name.includes('multiplier') || name.includes('base') || 
          name.includes('outer')) {
        categories.basic.push(field);
      } else if (name.includes('length') || name.includes('width') || name.includes('height') || 
                 name.includes('depth') || name.includes('dimension')) {
        categories.dimensions.push(field);
      } else if (name.includes('weight') || name.includes('kgm')) {
        categories.weight.push(field);
      } else if (name.includes('volume') || name.includes('liter')) {
        categories.volume.push(field);
      } else {
        categories.other.push(field);
      }
    });

    return categories;
  };

  const renderField = (field: DynamicField) => {
    const value = formData[field.name];

    switch (field.type) {
      case 'boolean':
        return (
          <div className="flex items-center justify-between p-3 rounded-lg bg-gray-50 hover:bg-gray-100 transition-colors">
            <Label htmlFor={field.name} className="text-sm font-medium cursor-pointer">
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Switch
              id={field.name}
              checked={!!value}
              onCheckedChange={(checked) => handleFieldChange(field.name, checked)}
              className="data-[state=checked]:bg-blue-600"
            />
          </div>
        );

      case 'select':
        return (
          <div className="space-y-2">
            <Label htmlFor={field.name} className="text-sm font-medium">
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Select
              value={value?.toString() || ''}
              onValueChange={(newValue) => handleFieldChange(field.name, newValue)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={`Select ${field.label}`} />
              </SelectTrigger>
              <SelectContent>
                {(field.options || ['Option 1', 'Option 2']).map((option) => (
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
            <Label htmlFor={field.name} className="text-sm font-medium">
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <div className="relative">
              <Input
                id={field.name}
                type="number"
                value={value || 0}
                onChange={(e) => handleFieldChange(field.name, parseFloat(e.target.value) || 0)}
                placeholder={`0`}
                className="pr-12"
                step="any"
              />
              {field.name.toLowerCase().includes('weight') && (
                <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm">
                  {formData['WeightUnit'] || 'KG'}
                </span>
              )}
              {field.name.toLowerCase().includes('dimension') && !field.name.toLowerCase().includes('unit') && (
                <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm">
                  {formData['DimensionUnit'] || 'CM'}
                </span>
              )}
            </div>
          </div>
        );

      case 'text':
      default:
        return (
          <div className="space-y-2">
            <Label htmlFor={field.name} className="text-sm font-medium">
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Input
              id={field.name}
              type="text"
              value={value || ''}
              onChange={(e) => handleFieldChange(field.name, e.target.value)}
              placeholder={`Enter ${field.label.toLowerCase()}`}
              className="w-full"
            />
          </div>
        );
    }
  };

  const handleSave = () => {
    // Validate required fields
    const missingRequired = fields
      .filter(f => f.required && !formData[f.name])
      .map(f => f.label);

    if (missingRequired.length > 0) {
      setError(`Please fill required fields: ${missingRequired.join(', ')}`);
      return;
    }

    onSave?.(formData);
    setHasChanges(false);
  };

  if (loading) {
    return (
      <Card className="w-full border-0 shadow-lg">
        <CardContent className="p-8">
          <div className="flex flex-col items-center justify-center space-y-4">
            <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
            <p className="text-gray-600">Discovering UOM configuration fields...</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  const categorizedFields = categorizeFields();
  const hasMultipleCategories = Object.values(categorizedFields).filter(arr => arr.length > 0).length > 1;

  return (
    <Card className="w-full border-0 shadow-lg">
      <CardHeader className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-t-lg">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-white rounded-lg shadow-sm">
              <Package className="h-5 w-5 text-blue-600" />
            </div>
            <div>
              <CardTitle className="text-xl">UOM Configuration</CardTitle>
              <CardDescription className="mt-1">
                Configure units of measurement with {fields.length} dynamic fields
              </CardDescription>
            </div>
          </div>
          <div className="flex items-center gap-2">
            {hasChanges && (
              <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">
                Unsaved Changes
              </Badge>
            )}
            <Button
              variant="outline"
              size="sm"
              onClick={discoverFields}
              className="bg-white"
            >
              <RefreshCw className="h-4 w-4 mr-2" />
              Refresh
            </Button>
          </div>
        </div>
      </CardHeader>
      
      <CardContent className="p-6">
        {error && (
          <Alert variant="destructive" className="mb-6">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>Error</AlertTitle>
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {hasMultipleCategories ? (
          <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
            <TabsList className="grid w-full grid-cols-4 mb-6">
              {categorizedFields.basic.length > 0 && (
                <TabsTrigger value="basic" className="flex items-center gap-2">
                  <Settings className="h-4 w-4" />
                  Basic
                </TabsTrigger>
              )}
              {categorizedFields.dimensions.length > 0 && (
                <TabsTrigger value="dimensions" className="flex items-center gap-2">
                  <Ruler className="h-4 w-4" />
                  Dimensions
                </TabsTrigger>
              )}
              {categorizedFields.weight.length > 0 && (
                <TabsTrigger value="weight" className="flex items-center gap-2">
                  <Weight className="h-4 w-4" />
                  Weight
                </TabsTrigger>
              )}
              {categorizedFields.volume.length > 0 && (
                <TabsTrigger value="volume" className="flex items-center gap-2">
                  <Droplet className="h-4 w-4" />
                  Volume
                </TabsTrigger>
              )}
            </TabsList>
            
            <TabsContent value="basic" className="space-y-4 mt-0">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {categorizedFields.basic.map(field => (
                  <div key={field.name}>
                    {renderField(field)}
                  </div>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="dimensions" className="space-y-4 mt-0">
              <div className="p-4 bg-blue-50 rounded-lg mb-4">
                <div className="flex items-start gap-2">
                  <Info className="h-4 w-4 text-blue-600 mt-0.5" />
                  <p className="text-sm text-blue-900">
                    Enter product dimensions for accurate shipping calculations
                  </p>
                </div>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {categorizedFields.dimensions.map(field => (
                  <div key={field.name}>
                    {renderField(field)}
                  </div>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="weight" className="space-y-4 mt-0">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {categorizedFields.weight.map(field => (
                  <div key={field.name}>
                    {renderField(field)}
                  </div>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="volume" className="space-y-4 mt-0">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {categorizedFields.volume.map(field => (
                  <div key={field.name}>
                    {renderField(field)}
                  </div>
                ))}
              </div>
            </TabsContent>
          </Tabs>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {fields.map(field => (
              <div key={field.name}>
                {renderField(field)}
              </div>
            ))}
          </div>
        )}

        <Separator className="my-6" />

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <Info className="h-4 w-4" />
            <span>Fields marked with <span className="text-red-500">*</span> are required</span>
          </div>
          
          <div className="flex items-center gap-3">
            <Button 
              variant="outline" 
              onClick={onCancel}
              className="min-w-[100px]"
            >
              <X className="h-4 w-4 mr-2" />
              Cancel
            </Button>
            <Button 
              onClick={handleSave}
              className="min-w-[100px] bg-blue-600 hover:bg-blue-700"
              disabled={!hasChanges}
            >
              <Check className="h-4 w-4 mr-2" />
              Save UOM
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};