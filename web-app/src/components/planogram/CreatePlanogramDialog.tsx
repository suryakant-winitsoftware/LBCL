'use client';

import React, { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import { Plus, Save, Upload, X } from 'lucide-react';
import PlanogramHierarchySelector from './PlanogramHierarchySelector';
import planogramService, { PlanogramSetup } from '@/services/planogramService';

interface CreatePlanogramDialogProps {
  onSuccess?: () => void;
}

export default function CreatePlanogramDialog({ onSuccess }: CreatePlanogramDialogProps) {
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  
  // Form state
  const [formData, setFormData] = useState<PlanogramSetup>({
    CategoryCode: '',
    ShareOfShelfCm: 100,
    SelectionType: '',
    SelectionValue: '',
  });
  
  // Image upload state
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>('');
  const [uploading, setUploading] = useState(false);

  const handleInputChange = (field: keyof PlanogramSetup, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      toast({
        title: 'Invalid file type',
        description: 'Please select an image file',
        variant: 'destructive',
      });
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      toast({
        title: 'File too large',
        description: 'Please select an image smaller than 5MB',
        variant: 'destructive',
      });
      return;
    }

    setSelectedImage(file);
    
    const reader = new FileReader();
    reader.onload = (e) => {
      setImagePreview(e.target?.result as string);
    };
    reader.readAsDataURL(file);
  };

  const clearImageSelection = () => {
    setSelectedImage(null);
    setImagePreview('');
  };

  const handleSaveWithImage = async () => {
    if (!formData.SelectionValue || !formData.SelectionType) {
      toast({
        title: 'Validation Error',
        description: 'Please select from the product hierarchy',
        variant: 'destructive',
      });
      return;
    }

    try {
      setSaving(true);

      // Get user info for created_by
      let createdBy = 'ADMIN';
      try {
        const userInfoStr = localStorage.getItem("user_info");
        if (userInfoStr) {
          const userInfo = JSON.parse(userInfoStr);
          createdBy = userInfo.name || userInfo.username || userInfo.UID || 'ADMIN';
        }
      } catch (e) {
        console.error("Failed to parse user_info:", e);
      }

      // Prepare planogram data
      const planogramData = {
        UID: formData.SelectionValue,
        CategoryCode: formData.SelectionValue,
        ShareOfShelfCm: formData.ShareOfShelfCm || 100,
        SelectionType: formData.SelectionType,
        SelectionValue: formData.SelectionValue,
        CreatedBy: createdBy,
        CreatedTime: new Date().toISOString(),
        ModifiedBy: createdBy,
        ModifiedTime: new Date().toISOString(),
        SS: 1
      };

      // Create planogram
      const response = await planogramService.createPlanogramSetup(planogramData);
      const isSuccess = response?.IsSuccess || response?.isSuccess || response?.StatusCode === 200;
      
      if (isSuccess) {
        // If there's an image, upload it
        if (selectedImage) {
          setUploading(true);
          try {
            await planogramService.uploadPlanogramImageDirect(formData.SelectionValue, selectedImage);
            toast({
              title: 'Success',
              description: 'Planogram created and image uploaded successfully',
            });
          } catch (imageError) {
            toast({
              title: 'Partial Success',
              description: 'Planogram created but image upload failed',
              variant: 'destructive',
            });
          }
          setUploading(false);
        } else {
          toast({
            title: 'Success',
            description: 'Planogram created successfully',
          });
        }

        // Reset form and close dialog
        setFormData({
          CategoryCode: '',
          ShareOfShelfCm: 100,
          SelectionType: '',
          SelectionValue: '',
        });
        clearImageSelection();
        setOpen(false);
        
        if (onSuccess) {
          onSuccess();
        }
      } else {
        throw new Error('Failed to create planogram');
      }
    } catch (error) {
      console.error('Error creating planogram:', error);
      toast({
        title: 'Error',
        description: 'Failed to create planogram setup',
        variant: 'destructive',
      });
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="h-4 w-4 mr-2" />
          Create Planogram
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Create New Planogram</DialogTitle>
          <DialogDescription>
            Configure a new planogram setup with optional image
          </DialogDescription>
        </DialogHeader>
        
        <div className="space-y-6">
          {/* Product Hierarchy Selection */}
          <div className="space-y-3">
            <Label className="text-sm font-medium">Product Selection</Label>
            <PlanogramHierarchySelector
              onSelectionChange={({ selectionType, selectionValue, fullHierarchy }) => {
                handleInputChange('SelectionType', selectionType);
                handleInputChange('SelectionValue', selectionValue);
                handleInputChange('CategoryCode', selectionValue);
              }}
            />
            
            {formData.SelectionType && formData.SelectionValue && (
              <div className="text-sm text-green-600 bg-green-50 p-2 rounded">
                ✓ Selected: {formData.SelectionType} - {formData.SelectionValue}
              </div>
            )}
          </div>

          {/* Share of Shelf */}
          <div className="space-y-2">
            <Label htmlFor="shareOfShelf">Share of Shelf (cm)</Label>
            <Input
              id="shareOfShelf"
              type="number"
              value={formData.ShareOfShelfCm}
              onChange={(e) => handleInputChange('ShareOfShelfCm', parseInt(e.target.value) || 0)}
              placeholder="100"
              className="w-32"
            />
          </div>

          {/* Image Upload */}
          <div className="space-y-3">
            <Label className="text-sm font-medium">Planogram Image (Optional)</Label>
            
            {imagePreview ? (
              <div className="relative">
                <img
                  src={imagePreview}
                  alt="Preview"
                  className="w-full h-32 object-cover rounded border"
                />
                <Button
                  variant="outline"
                  size="sm"
                  className="absolute top-2 right-2"
                  onClick={clearImageSelection}
                >
                  <X className="h-4 w-4" />
                </Button>
                <div className="mt-2 text-xs text-muted-foreground">
                  {selectedImage?.name} ({Math.round((selectedImage?.size || 0) / 1024)} KB)
                </div>
              </div>
            ) : (
              <div
                className="border-2 border-dashed rounded p-4 text-center cursor-pointer hover:bg-muted/50"
                onClick={() => document.getElementById('image-upload')?.click()}
              >
                <Upload className="mx-auto h-6 w-6 text-muted-foreground mb-2" />
                <div className="text-sm">Click to upload image</div>
                <div className="text-xs text-muted-foreground">PNG, JPG up to 5MB</div>
                <input
                  id="image-upload"
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={handleFileSelect}
                />
              </div>
            )}
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-4">
            <Button variant="outline" onClick={() => setOpen(false)}>
              Cancel
            </Button>
            <Button 
              onClick={handleSaveWithImage}
              disabled={saving || uploading || !formData.SelectionValue}
            >
              <Save className="h-4 w-4 mr-2" />
              {saving ? 'Creating...' : uploading ? 'Uploading...' : 'Create Planogram'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}