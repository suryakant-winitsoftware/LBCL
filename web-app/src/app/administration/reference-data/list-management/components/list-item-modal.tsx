"use client";

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
import { Switch } from '@/components/ui/switch';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, AlertCircle } from 'lucide-react';
import {
  ListItem,
  ListHeader,
  CreateListItemRequest,
  UpdateListItemRequest,
  listService
} from '@/services/listService';

interface ListItemModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit?: (data: CreateListItemRequest | UpdateListItemRequest) => Promise<void>;
  mode: 'create' | 'edit' | 'view';
  title: string;
  initialData?: ListItem | null;
  listHeaders: ListHeader[];
}

export function ListItemModal({
  isOpen,
  onClose,
  onSubmit,
  mode,
  title,
  initialData,
  listHeaders
}: ListItemModalProps) {
  const [formData, setFormData] = useState<Partial<CreateListItemRequest>>({
    code: '',
    name: '',
    listHeaderUID: '',
    serialNo: 1,
    isEditable: true,
    uid: ''
  });

  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);

  useEffect(() => {
    if (isOpen) {
      if (initialData && (mode === 'edit' || mode === 'view')) {
        setFormData({
          code: initialData.code || '',
          name: initialData.name || '',
          listHeaderUID: initialData.listHeaderUID || '',
          serialNo: initialData.serialNo || 1,
          isEditable: initialData.isEditable ?? true,
          uid: initialData.uid || ''
        });
      } else if (mode === 'create') {
        setFormData({
          code: '',
          name: '',
          listHeaderUID: '',
          serialNo: 1,
          isEditable: true,
          uid: ''
        });
      }
      setErrors([]);
    }
  }, [isOpen, initialData, mode]);

  const handleSubmit = async () => {
    if (mode === 'view') return;

    setErrors([]);

    // Validate form data
    const validationErrors = listService.validateListItem(formData);
    if (validationErrors.length > 0) {
      setErrors(validationErrors);
      return;
    }

    // Generate UID if creating new item
    if (mode === 'create' && (!formData.uid || formData.uid.trim() === '')) {
      formData.uid = listService.generateUID(formData.name || 'LISTITEM');
    }

    setLoading(true);
    try {
      if (mode === 'create') {
        await onSubmit?.(formData as CreateListItemRequest);
      } else if (mode === 'edit' && initialData) {
        await onSubmit?.({
          ...formData,
          id: initialData.id
        } as UpdateListItemRequest);
      }
      onClose();
    } catch (error) {
      setErrors([error instanceof Error ? error.message : 'An error occurred']);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field: keyof CreateListItemRequest, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
    // Clear errors when user starts typing
    if (errors.length > 0) {
      setErrors([]);
    }
  };

  const getSelectedHeader = () => {
    return listHeaders.find(h => h.uid === formData.listHeaderUID);
  };

  const isViewMode = mode === 'view';

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            {title}
            {mode === 'view' && (
              <Badge variant="outline" className="ml-2">Read Only</Badge>
            )}
          </DialogTitle>
          <DialogDescription>
            {mode === 'create' && 'Create a new list item entry.'}
            {mode === 'edit' && 'Edit the list item details.'}
            {mode === 'view' && 'View list item details and information.'}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-6 py-4">
          {/* Error Display */}
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

          <div className="grid grid-cols-2 gap-4">
            {/* Code */}
            <div className="space-y-2">
              <Label htmlFor="code">Code *</Label>
              <Input
                id="code"
                value={formData.code || ''}
                onChange={(e) => handleInputChange('code', e.target.value)}
                placeholder="Enter code"
                disabled={isViewMode}
                className={isViewMode ? 'bg-gray-50' : ''}
              />
            </div>

            {/* UID */}
            <div className="space-y-2">
              <Label htmlFor="uid">UID</Label>
              <Input
                id="uid"
                value={formData.uid || ''}
                onChange={(e) => handleInputChange('uid', e.target.value)}
                placeholder="Auto-generated if empty"
                disabled={isViewMode}
                className={isViewMode ? 'bg-gray-50' : ''}
              />
              {mode === 'create' && (
                <p className="text-xs text-gray-500">
                  Leave empty to auto-generate based on name
                </p>
              )}
            </div>
          </div>

          {/* Name */}
          <div className="space-y-2">
            <Label htmlFor="name">Name *</Label>
            <Input
              id="name"
              value={formData.name || ''}
              onChange={(e) => handleInputChange('name', e.target.value)}
              placeholder="Enter item name"
              disabled={isViewMode}
              className={isViewMode ? 'bg-gray-50' : ''}
            />
          </div>

          {/* List Header */}
          <div className="space-y-2">
            <Label htmlFor="listHeaderUID">List Header *</Label>
            {isViewMode ? (
              <div className="p-2 border rounded-md bg-gray-50">
                {getSelectedHeader()?.name || 'Unknown Header'}
              </div>
            ) : (
              <Select
                value={formData.listHeaderUID || ''}
                onValueChange={(value) => handleInputChange('listHeaderUID', value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select list header" />
                </SelectTrigger>
                <SelectContent>
                  {listHeaders.map((header) => (
                    <SelectItem key={header.uid} value={header.uid}>
                      <div className="flex flex-col">
                        <span>{header.name}</span>
                        <span className="text-xs text-gray-500">{header.code}</span>
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            {/* Serial Number */}
            <div className="space-y-2">
              <Label htmlFor="serialNo">Serial Number</Label>
              <Input
                id="serialNo"
                type="number"
                value={formData.serialNo || ''}
                onChange={(e) => handleInputChange('serialNo', parseInt(e.target.value) || 1)}
                placeholder="1"
                min="1"
                disabled={isViewMode}
                className={isViewMode ? 'bg-gray-50' : ''}
              />
            </div>

            {/* Editable Status */}
            <div className="space-y-2">
              <Label htmlFor="isEditable">Editable</Label>
              <div className="flex items-center space-x-2 pt-2">
                <Switch
                  id="isEditable"
                  checked={formData.isEditable ?? true}
                  onCheckedChange={(checked) => handleInputChange('isEditable', checked)}
                  disabled={isViewMode}
                />
                <Label htmlFor="isEditable" className="cursor-pointer">
                  {formData.isEditable ? 'Editable' : 'Read Only'}
                </Label>
              </div>
            </div>
          </div>

          {/* View Mode Additional Info */}
          {isViewMode && initialData && (
            <div className="border-t pt-4 space-y-3">
              <h4 className="font-medium text-sm text-gray-900">System Information</h4>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="font-medium text-gray-700">Created By:</span>
                  <p className="text-gray-600">{initialData.createdBy || 'Unknown'}</p>
                </div>
                <div>
                  <span className="font-medium text-gray-700">Modified By:</span>
                  <p className="text-gray-600">{initialData.modifiedBy || 'Unknown'}</p>
                </div>
                <div>
                  <span className="font-medium text-gray-700">Created:</span>
                  <p className="text-gray-600">
                    {initialData.createdTime ? new Date(initialData.createdTime).toLocaleString() : 'Unknown'}
                  </p>
                </div>
                <div>
                  <span className="font-medium text-gray-700">Last Modified:</span>
                  <p className="text-gray-600">
                    {initialData.modifiedTime ? new Date(initialData.modifiedTime).toLocaleString() : 'Unknown'}
                  </p>
                </div>
              </div>
            </div>
          )}
        </div>

        <DialogFooter className="gap-2">
          <Button variant="outline" onClick={onClose} disabled={loading}>
            {isViewMode ? 'Close' : 'Cancel'}
          </Button>
          {!isViewMode && (
            <Button onClick={handleSubmit} disabled={loading}>
              {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {mode === 'create' ? 'Create' : 'Save Changes'}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}