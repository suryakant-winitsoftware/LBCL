'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  ArrowLeft,
  Save,
  Upload,
  X,
  Image as ImageIcon,
  Trash2,
} from 'lucide-react';
import planogramService, { PlanogramSetup } from '@/services/planogramService';
import PlanogramHierarchySelector from '@/components/planogram/PlanogramHierarchySelector';

export default function CreatePlanogramPage() {
  const router = useRouter();
  const { toast } = useToast();
  
  // Form state
  const [formData, setFormData] = useState<PlanogramSetup>({
    CategoryCode: '',
    ShareOfShelfCm: 100,
    SelectionType: 'Category',
    SelectionValue: '',
  });
  
  // Product hierarchy state
  const [selectedAttributes, setSelectedAttributes] = useState<
    Array<{ type: string; code: string; value: string; uid?: string; level?: number }>
  >([]);
  
  // Image upload state
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>('');
  const [uploadedImages, setUploadedImages] = useState<any[]>([]);
  const [uploading, setUploading] = useState(false);
  const [saving, setSaving] = useState(false);
  
  // Created planogram UID (after save)
  const [planogramUID, setPlanogramUID] = useState<string>('');

  // Load images when planogram UID or SelectionValue is set
  useEffect(() => {
    const planogramId = planogramUID || formData.SelectionValue;
    if (planogramId) {
      loadUploadedImages(planogramId);
    }
  }, [planogramUID, formData.SelectionValue]);

  const handleInputChange = (field: keyof PlanogramSetup, value: any) => {
    console.log('=== HANDLE INPUT CHANGE ===');
    console.log('Field:', field);
    console.log('Value:', value);
    console.log('Current formData before update:', formData);
    
    setFormData(prev => {
      const newData = {
        ...prev,
        [field]: value,
      };
      console.log('New formData after update:', newData);
      return newData;
    });
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      toast({
        title: 'Invalid file type',
        description: 'Please select an image file',
        variant: 'destructive',
      });
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      toast({
        title: 'File too large',
        description: 'Please select an image smaller than 5MB',
        variant: 'destructive',
      });
      return;
    }

    setSelectedImage(file);
    
    // Create preview
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

  const handleSavePlanogram = async () => {
    console.log('=== SAVING PLANOGRAM ===');
    console.log('Form data:', formData);
    console.log('Selected attributes:', selectedAttributes);
    console.log('SelectionValue:', formData.SelectionValue);
    console.log('SelectionType:', formData.SelectionType);
    
    // Validate required fields
    if (!formData.SelectionValue || !formData.SelectionType) {
      console.log('VALIDATION FAILED - Missing selection');
      toast({
        title: 'Validation Error',
        description: 'Please select from the product hierarchy first. Check browser console for details.',
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
      
      // Prepare data in correct format for API
      const planogramData = {
        // Use SelectionValue as both UID and CategoryCode (like existing data)
        UID: formData.SelectionValue,  // This becomes the planogram UID
        CategoryCode: formData.SelectionValue,  // Category code is the selection value
        ShareOfShelfCm: formData.ShareOfShelfCm || 100,
        SelectionType: formData.SelectionType,  // The type selected (Category, Brand, etc.)
        SelectionValue: formData.SelectionValue,  // The actual value selected
        CreatedBy: createdBy,
        CreatedTime: new Date().toISOString(),
        ModifiedBy: createdBy,
        ModifiedTime: new Date().toISOString(),
        SS: 1  // Active status
      };
      
      console.log('Prepared planogram data:', planogramData);
      
      // Create the planogram setup
      console.log('Calling planogramService.createPlanogramSetup...');
      const response = await planogramService.createPlanogramSetup(planogramData);
      console.log('Create response:', response);
      console.log('Response type:', typeof response);
      console.log('Response keys:', response ? Object.keys(response) : 'null response');
      
      // Check different response formats
      const isSuccess = response?.IsSuccess || response?.isSuccess || response?.StatusCode === 200;
      // Backend returns { Data: { uid: "guid", message: "..." } }
      const uid = response?.Data?.uid || response?.Data?.UID || response?.uid || response?.UID || formData.SelectionValue;
      
      console.log('IsSuccess:', isSuccess);
      console.log('Extracted UID:', uid);
      console.log('Full response Data:', response?.Data);
      
      if (isSuccess && uid) {
        setPlanogramUID(uid);
        
        toast({
          title: 'Success',
          description: 'Planogram setup created successfully',
        });
        
        // Load existing images for this planogram
        await loadUploadedImages(uid);
        
        // If there's an image selected, upload it immediately
        if (selectedImage) {
          await handleUploadImage(uid);
        }
        
        return uid;
      } else {
        console.error('=== RESPONSE VALIDATION FAILED ===');
        console.error('Full response:', response);
        console.error('IsSuccess check:', isSuccess);
        console.error('UID check:', uid);
        console.error('Response Data:', response?.Data);
        console.error('Response keys:', response ? Object.keys(response) : 'null');
        
        const errorMsg = response?.ErrorMessage || response?.Message || response?.Data?.message || 'Unknown error';
        throw new Error(`Failed to create planogram setup: ${errorMsg}`);
      }
    } catch (error) {
      console.error('=== SAVE ERROR DETAILS ===');
      console.error('Error object:', error);
      console.error('Error message:', error instanceof Error ? error.message : 'Unknown error');
      console.error('Error stack:', error instanceof Error ? error.stack : 'No stack');
      
      const errorMessage = error instanceof Error ? error.message : 'Failed to save planogram setup';
      toast({
        title: 'Save Error',
        description: `${errorMessage}. Check console for details.`,
        variant: 'destructive',
      });
      return null;
    } finally {
      setSaving(false);
    }
  };

  const handleUploadImage = async (uid?: string) => {
    // Use the selection value as planogram ID if no specific UID provided
    const planogramId = uid || planogramUID || formData.SelectionValue;
    
    console.log('=== UPLOADING IMAGE ===');
    console.log('Planogram UID:', planogramId);
    console.log('From uid param:', uid);
    console.log('From planogramUID state:', planogramUID);
    console.log('From SelectionValue:', formData.SelectionValue);
    console.log('Selected image:', selectedImage);
    
    if (!planogramId) {
      toast({
        title: 'Error',
        description: 'Please select from product hierarchy first',
        variant: 'destructive',
      });
      return;
    }
    
    if (!selectedImage) {
      toast({
        title: 'Error',
        description: 'Please select an image first',
        variant: 'destructive',
      });
      return;
    }

    try {
      setUploading(true);
      
      console.log('Calling uploadPlanogramImageDirect with UID:', planogramId);
      await planogramService.uploadPlanogramImageDirect(planogramId, selectedImage);
      
      toast({
        title: 'Success',
        description: 'Image uploaded successfully',
      });
      
      // Clear selection after successful upload
      clearImageSelection();
      
      // Reload uploaded images
      await loadUploadedImages(planogramId);
      
    } catch (error) {
      console.error('=== IMAGE UPLOAD ERROR ===');
      console.error('Error:', error);
      toast({
        title: 'Upload Failed',
        description: error instanceof Error ? error.message : 'Failed to upload image',
        variant: 'destructive',
      });
    } finally {
      setUploading(false);
    }
  };

  const loadUploadedImages = async (uid?: string) => {
    const planogramId = uid || planogramUID || formData.SelectionValue;
    
    if (!planogramId) {
      console.log('No planogram ID available, clearing images');
      setUploadedImages([]);
      return;
    }
    
    try {
      console.log('Loading images for planogram ID:', planogramId);
      const result = await planogramService.getPlanogramImages(planogramId);
      
      console.log('Image load result:', result);
      
      if (result?.IsSuccess && result?.Data) {
        console.log(`Found ${result.Data.length} images for planogram ${uid}:`, result.Data);
        // Filter to ensure we only show images for this specific planogram
        const filteredImages = result.Data.filter((img: any) => 
          img.LinkedItemUID === uid && 
          (img.LinkedItemType === 'Planogram' || img.LinkedItemType === 'planogram_image')
        );
        console.log(`After filtering: ${filteredImages.length} images`);
        setUploadedImages(filteredImages);
      } else {
        console.log('No images found or request failed');
        setUploadedImages([]);
      }
    } catch (error) {
      console.error('Error loading images:', error);
      setUploadedImages([]);
    }
  };

  const handleDeleteImage = async (fileUID: string) => {
    try {
      await planogramService.deletePlanogramImage(fileUID);
      
      toast({
        title: 'Success',
        description: 'Image deleted successfully',
      });
      
      // Reload images
      if (planogramUID) {
        await loadUploadedImages(planogramUID);
      }
    } catch (error) {
      console.error('Error deleting image:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete image',
        variant: 'destructive',
      });
    }
  };

  const handleSaveAndUpload = async () => {
    // First save the planogram if not already saved
    let uid = planogramUID;
    if (!uid) {
      uid = await handleSavePlanogram();
    }
    
    // Then upload the image if we have a UID
    if (uid && selectedImage) {
      await handleUploadImage(uid);
    }
  };

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.back()}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold">Create Planogram Setup</h1>
            <p className="text-muted-foreground">
              Create a new planogram configuration with images
            </p>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto space-y-6">
        
        {/* Product Hierarchy Selection */}
        <Card>
          <CardHeader>
            <CardTitle>Product Hierarchy Selection</CardTitle>
            <CardDescription>
              Select the product category, brand, or SKU for this planogram
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <PlanogramHierarchySelector
              onSelectionChange={({ selectionType, selectionValue, fullHierarchy }) => {
                console.log('=== PARENT RECEIVED SELECTION CALLBACK ===');
                console.log('Received Selection Type:', selectionType);
                console.log('Received Selection Value:', selectionValue);
                console.log('Received Full Hierarchy:', fullHierarchy);
                
                // Update form data with the selection
                console.log('Updating form data...');
                handleInputChange('SelectionType', selectionType);
                handleInputChange('SelectionValue', selectionValue);
                handleInputChange('CategoryCode', selectionValue);
                
                // Store the full hierarchy for reference
                setSelectedAttributes(fullHierarchy);
                
                console.log('Form data after update:', {
                  SelectionType: selectionType,
                  SelectionValue: selectionValue,
                  CategoryCode: selectionValue
                });
              }}
            />
            
            
            {formData.SelectionType && formData.SelectionValue && (
              <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded-lg">
                <div className="text-sm">
                  <strong>✓ Selected:</strong> {formData.SelectionType} - {formData.SelectionValue}
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Shelf Configuration */}
        <Card>
          <CardHeader>
            <CardTitle>Shelf Configuration</CardTitle>
            <CardDescription>
              Configure the shelf space allocation
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-3">
              <div className="space-y-2">
                <Label htmlFor="shareOfShelf">
                  Share of Shelf (cm) <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="shareOfShelf"
                  type="number"
                  value={formData.ShareOfShelfCm}
                  onChange={(e) => handleInputChange('ShareOfShelfCm', parseInt(e.target.value) || 0)}
                  placeholder="100"
                />
              </div>
              
              <div className="flex items-end">
                <div className="text-center p-3 bg-muted rounded">
                  <div className="text-lg font-semibold">
                    {formData.ShareOfShelfCm || 0} cm
                  </div>
                  <div className="text-xs text-muted-foreground">Allocated</div>
                </div>
              </div>

              <div className="flex items-end">
                <Button
                  onClick={handleSavePlanogram}
                  disabled={saving || !formData.SelectionValue}
                >
                  <Save className="h-4 w-4 mr-2" />
                  {saving ? 'Saving...' : 'Save Configuration'}
                </Button>
              </div>
            </div>

            {planogramUID && (
              <div className="text-sm text-muted-foreground">
                ✓ Configuration saved successfully
              </div>
            )}
            
            {!formData.SelectionValue && (
              <div className="text-sm text-muted-foreground">
                Please complete product selection first
              </div>
            )}
          </CardContent>
        </Card>

        {/* Image Management */}
        <Card>
          <CardHeader>
            <CardTitle>Planogram Images</CardTitle>
            <CardDescription>
              Upload and manage images for this planogram
            </CardDescription>
          </CardHeader>
          <CardContent>
            {!formData.SelectionValue ? (
              <div className="text-center py-8">
                <div className="text-sm text-muted-foreground">
                  Please select from product hierarchy first to enable image uploads
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-3">
                    <h4 className="font-medium">Upload Image</h4>
                    {imagePreview ? (
                      <div className="relative">
                        <img
                          src={imagePreview}
                          alt="Preview"
                          className="w-full h-48 object-cover rounded border"
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
                        className="border-2 border-dashed rounded p-6 text-center cursor-pointer hover:bg-muted/50"
                        onClick={() => document.getElementById('image-upload')?.click()}
                      >
                        <ImageIcon className="mx-auto h-8 w-8 text-muted-foreground mb-2" />
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

                    {selectedImage && (
                      <Button
                        onClick={() => handleUploadImage()}
                        disabled={uploading}
                        className="w-full"
                      >
                        <Upload className="h-4 w-4 mr-2" />
                        {uploading ? 'Uploading...' : 'Upload Image'}
                      </Button>
                    )}
                  </div>

                  <div className="space-y-3">
                    <h4 className="font-medium">
                      Uploaded Images ({uploadedImages.length})
                    </h4>
                    {uploadedImages.length > 0 ? (
                      <div className="space-y-2 max-h-48 overflow-y-auto">
                        {uploadedImages.map((image) => (
                          <div key={image.UID} className="flex items-center gap-2 p-2 border rounded">
                            <ImageIcon className="h-4 w-4 text-muted-foreground" />
                            <div className="flex-1 min-w-0">
                              <div className="text-sm font-medium truncate">
                                {image.DisplayName || image.FileName}
                              </div>
                              <div className="text-xs text-muted-foreground">
                                {Math.round(image.FileSize / 1024)} KB
                              </div>
                            </div>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDeleteImage(image.UID)}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-6 text-sm text-muted-foreground">
                        No images uploaded yet
                      </div>
                    )}
                  </div>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

    </div>
  );
}