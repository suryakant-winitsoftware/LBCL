import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Separator } from '@/components/ui/separator';
import { AlertCircle, Save, X } from 'lucide-react';

import {
  settingsService,
  Setting,
  CreateSettingRequest,
  UpdateSettingRequest
} from '@/services/settings.service';

const SETTING_TYPES = ['Global', 'UI', 'FR', 'Test'] as const;
const DATA_TYPES = ['String', 'Int', 'Boolean'] as const;

interface SettingModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit?: (data: CreateSettingRequest | UpdateSettingRequest) => Promise<void>;
  mode: 'create' | 'edit' | 'view';
  title: string;
  initialData?: Setting | null;
}

export function SettingModal({
  isOpen,
  onClose,
  onSubmit,
  mode,
  title,
  initialData
}: SettingModalProps) {
  const [formData, setFormData] = useState<Partial<CreateSettingRequest>>({
    type: 'Global',
    name: '',
    value: '',
    dataType: 'String',
    isEditable: true,
    uid: ''
  });

  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);

  useEffect(() => {
    if (isOpen) {
      if (mode === 'create') {
        setFormData({
          type: 'Global',
          name: '',
          value: '',
          dataType: 'String',
          isEditable: true,
          uid: ''
        });
      } else if (initialData) {
        setFormData({
          type: initialData.type,
          name: initialData.name,
          value: initialData.value,
          dataType: initialData.dataType,
          isEditable: initialData.isEditable,
          uid: initialData.uid
        });
      }
      setErrors([]);
    }
  }, [isOpen, mode, initialData]);

  const generateUID = () => {
    if (formData.name) {
      const uid = settingsService.generateUID(formData.name);
      setFormData(prev => ({ ...prev, uid }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!onSubmit || mode === 'view') return;

    const validationErrors = settingsService.validateSetting(formData);
    if (validationErrors.length > 0) {
      setErrors(validationErrors);
      return;
    }

    if (!formData.uid && mode === 'create') {
      generateUID();
      if (!formData.uid) {
        setErrors(['UID is required']);
        return;
      }
    }

    try {
      setLoading(true);
      setErrors([]);

      const settingData = formData as CreateSettingRequest;
      
      if (mode === 'edit' && initialData) {
        const updateData: UpdateSettingRequest = {
          ...settingData,
          id: initialData.id
        };
        await onSubmit(updateData);
      } else {
        await onSubmit(settingData);
      }
    } catch (error) {
      setErrors([error instanceof Error ? error.message : 'An error occurred']);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field: keyof CreateSettingRequest, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // Auto-generate UID when name changes in create mode
    if (field === 'name' && mode === 'create' && value) {
      const uid = settingsService.generateUID(value);
      setFormData(prev => ({ ...prev, uid }));
    }
  };

  const formatValue = (value: string, dataType: string) => {
    if (dataType === 'Boolean') {
      return ['1', 'true'].includes(value.toLowerCase()) ? 'true' : 'false';
    }
    return value;
  };

  const getBooleanDisplayValue = (value: string) => {
    return ['1', 'true'].includes(value.toLowerCase());
  };

  const isReadOnly = mode === 'view';

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            {title}
            {mode === 'view' && initialData && (
              <Badge variant="outline">{initialData.type}</Badge>
            )}
          </DialogTitle>
          <DialogDescription>
            {mode === 'create' && 'Create a new system setting'}
            {mode === 'edit' && 'Modify the selected setting'}
            {mode === 'view' && 'View setting details and metadata'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6">
          {errors.length > 0 && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>
                <ul className="list-disc list-inside space-y-1">
                  {errors.map((error, index) => (
                    <li key={index}>{error}</li>
                  ))}
                </ul>
              </AlertDescription>
            </Alert>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Basic Information */}
            <div className="space-y-4">
              <div>
                <Label htmlFor="name">Name *</Label>
                <Input
                  id="name"
                  value={formData.name || ''}
                  onChange={(e) => handleInputChange('name', e.target.value)}
                  placeholder="Setting name"
                  disabled={isReadOnly}
                  className={isReadOnly ? 'bg-gray-50' : ''}
                />
              </div>

              <div>
                <Label htmlFor="type">Type *</Label>
                <Select
                  value={formData.type || 'Global'}
                  onValueChange={(value) => handleInputChange('type', value)}
                  disabled={isReadOnly}
                >
                  <SelectTrigger className={isReadOnly ? 'bg-gray-50' : ''}>
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    {SETTING_TYPES.map(type => (
                      <SelectItem key={type} value={type}>{type}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div>
                <Label htmlFor="dataType">Data Type *</Label>
                <Select
                  value={formData.dataType || 'String'}
                  onValueChange={(value) => handleInputChange('dataType', value)}
                  disabled={isReadOnly}
                >
                  <SelectTrigger className={isReadOnly ? 'bg-gray-50' : ''}>
                    <SelectValue placeholder="Select data type" />
                  </SelectTrigger>
                  <SelectContent>
                    {DATA_TYPES.map(dataType => (
                      <SelectItem key={dataType} value={dataType}>{dataType}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            {/* Value and Configuration */}
            <div className="space-y-4">
              <div>
                <Label htmlFor="value">Value *</Label>
                {formData.dataType === 'Boolean' ? (
                  <div className="flex items-center space-x-2 mt-2">
                    <Switch
                      checked={getBooleanDisplayValue(formData.value || '0')}
                      onCheckedChange={(checked) => 
                        handleInputChange('value', checked ? '1' : '0')
                      }
                      disabled={isReadOnly}
                    />
                    <Label htmlFor="booleanValue" className="text-sm">
                      {getBooleanDisplayValue(formData.value || '0') ? 'True' : 'False'}
                    </Label>
                  </div>
                ) : (
                  <Textarea
                    id="value"
                    value={formData.value || ''}
                    onChange={(e) => handleInputChange('value', e.target.value)}
                    placeholder="Setting value"
                    disabled={isReadOnly}
                    className={isReadOnly ? 'bg-gray-50' : ''}
                    rows={3}
                  />
                )}
              </div>

              <div className="flex items-center space-x-2">
                <Switch
                  checked={formData.isEditable || false}
                  onCheckedChange={(checked) => handleInputChange('isEditable', checked)}
                  disabled={isReadOnly}
                />
                <Label htmlFor="isEditable" className="text-sm">
                  Allow editing by users
                </Label>
              </div>

              <div>
                <Label htmlFor="uid">UID *</Label>
                <div className="flex gap-2">
                  <Input
                    id="uid"
                    value={formData.uid || ''}
                    onChange={(e) => handleInputChange('uid', e.target.value)}
                    placeholder="Unique identifier"
                    disabled={isReadOnly || mode === 'edit'}
                    className={isReadOnly || mode === 'edit' ? 'bg-gray-50' : ''}
                  />
                  {mode === 'create' && (
                    <Button
                      type="button"
                      variant="outline"
                      onClick={generateUID}
                      disabled={!formData.name}
                    >
                      Generate
                    </Button>
                  )}
                </div>
                {mode === 'edit' && (
                  <p className="text-xs text-gray-500 mt-1">
                    UID cannot be modified after creation
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Metadata for View Mode */}
          {mode === 'view' && initialData && (
            <>
              <Separator />
              <div className="space-y-4">
                <h4 className="font-medium text-sm text-gray-900">Metadata</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                  <div>
                    <Label className="text-xs text-gray-500">ID</Label>
                    <p className="font-mono">{initialData.id}</p>
                  </div>
                  <div>
                    <Label className="text-xs text-gray-500">SS</Label>
                    <p className="font-mono">{initialData.ss || 'N/A'}</p>
                  </div>
                  <div>
                    <Label className="text-xs text-gray-500">Created Time</Label>
                    <p>{new Date(initialData.createdTime).toLocaleString()}</p>
                  </div>
                  <div>
                    <Label className="text-xs text-gray-500">Modified Time</Label>
                    <p>{new Date(initialData.modifiedTime).toLocaleString()}</p>
                  </div>
                  <div>
                    <Label className="text-xs text-gray-500">Server Add Time</Label>
                    <p>{new Date(initialData.serverAddTime).toLocaleString()}</p>
                  </div>
                  <div>
                    <Label className="text-xs text-gray-500">Server Modified Time</Label>
                    <p>{new Date(initialData.serverModifiedTime).toLocaleString()}</p>
                  </div>
                </div>
              </div>
            </>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              <X className="mr-2 h-4 w-4" />
              {mode === 'view' ? 'Close' : 'Cancel'}
            </Button>
            {mode !== 'view' && (
              <Button type="submit" disabled={loading}>
                <Save className="mr-2 h-4 w-4" />
                {loading ? 'Saving...' : (mode === 'create' ? 'Create Setting' : 'Update Setting')}
              </Button>
            )}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}