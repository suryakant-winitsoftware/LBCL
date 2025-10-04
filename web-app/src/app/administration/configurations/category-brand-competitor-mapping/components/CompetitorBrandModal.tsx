'use client';

import React, { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import competitorBrandService, {
  CompetitorBrandMapping,
  CompetitorBrandMappingDto,
  CompetitorBrandUpdateDto,
  DropdownOption,
} from '@/services/competitorBrandService';

interface CompetitorBrandModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
  mapping?: CompetitorBrandMapping | null;
}

export function CompetitorBrandModal({
  open,
  onClose,
  onSuccess,
  mapping,
}: CompetitorBrandModalProps) {
  const { toast } = useToast();
  const isEdit = Boolean(mapping);

  // Form state
  const [formData, setFormData] = useState({
    categoryCode: '',
    brandCode: '',
    competitorCode: '',
  });
  const [loading, setLoading] = useState(false);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [isNewCompetitor, setIsNewCompetitor] = useState(false);
  const [newCompetitorName, setNewCompetitorName] = useState('');

  // Dropdown data
  const [categories, setCategories] = useState<DropdownOption[]>([]);
  const [brands, setBrands] = useState<DropdownOption[]>([]);
  const [competitors, setCompetitors] = useState<DropdownOption[]>([]);

  useEffect(() => {
    if (open) {
      loadInitialData();
      if (mapping) {
        setFormData({
          categoryCode: mapping.categoryCode || '',
          brandCode: mapping.brandCode || '',
          competitorCode: mapping.competitorCompany || '',
        });
        setIsNewCompetitor(false);
        setNewCompetitorName('');
      } else {
        setFormData({
          categoryCode: '',
          brandCode: '',
          competitorCode: '',
        });
        setIsNewCompetitor(false);
        setNewCompetitorName('');
      }
      setFormErrors({});
    }
  }, [open, mapping]);

  useEffect(() => {
    if (formData.categoryCode) {
      loadBrands(formData.categoryCode);
    } else {
      setBrands([]);
      setFormData(prev => ({ ...prev, brandCode: '' }));
    }
  }, [formData.categoryCode]);

  const loadInitialData = async () => {
    try {
      const [categoriesData, competitorsData] = await Promise.all([
        competitorBrandService.getCategories(),
        competitorBrandService.getCompetitors(),
      ]);
      setCategories(categoriesData);
      setCompetitors(competitorsData);
    } catch (error) {
      console.error('Error loading initial data:', error);
      toast({
        title: 'Error',
        description: 'Failed to load form data',
        variant: 'destructive',
      });
    }
  };

  const loadBrands = async (categoryCode: string) => {
    try {
      const brandsData = await competitorBrandService.getBrandsByCategory(categoryCode);
      setBrands(brandsData);
    } catch (error) {
      console.error('Error loading brands:', error);
      toast({
        title: 'Error',
        description: 'Failed to load brands',
        variant: 'destructive',
      });
    }
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.categoryCode) {
      errors.categoryCode = 'Category is required';
    }
    if (!formData.brandCode) {
      errors.brandCode = 'Brand is required';
    }
    if (isNewCompetitor) {
      if (!newCompetitorName.trim()) {
        errors.competitorCode = 'Competitor name is required';
      }
    } else {
      if (!formData.competitorCode) {
        errors.competitorCode = 'Competitor is required';
      }
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) {
      return;
    }

    try {
      setLoading(true);

      const competitorValue = isNewCompetitor ? newCompetitorName.trim() : formData.competitorCode;

      if (isEdit) {
        const updateData: CompetitorBrandUpdateDto = {
          uid: mapping!.uid!,
          competitorCode: competitorValue,
        };
        await competitorBrandService.updateMapping(updateData);
      } else {
        const createData: CompetitorBrandMappingDto = {
          categoryCode: formData.categoryCode,
          brandCode: formData.brandCode,
          competitorCode: competitorValue,
        };
        await competitorBrandService.createMapping(createData);
      }

      onSuccess();
    } catch (error: any) {
      console.error('Error saving mapping:', error);
      
      let errorMessage = 'Failed to save mapping';
      if (error.response?.data?.Error) {
        errorMessage = error.response.data.Error;
      } else if (error.response?.data?.error) {
        errorMessage = error.response.data.error;
      } else if (error.message) {
        errorMessage = error.message;
      }

      toast({
        title: 'Error',
        description: errorMessage,
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleFieldChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // Clear field error when user starts typing
    if (formErrors[field]) {
      setFormErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const selectedCategory = categories.find(c => c.value === formData.categoryCode);
  const selectedBrand = brands.find(b => b.value === formData.brandCode);

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? 'Edit Competitor Mapping' : 'Add Competitor Category'}
          </DialogTitle>
          <DialogDescription>
            {isEdit
              ? 'Update the competitor mapping details below.'
              : 'Create a new competitor category mapping by filling out the form below.'}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-6 py-4">
          {/* Category Selection */}
          <div className="grid gap-2">
            <Label htmlFor="category" className="text-sm font-medium">
              Our Category <span className="text-red-500">*</span>
            </Label>
            <Select
              value={formData.categoryCode}
              onValueChange={(value) => handleFieldChange('categoryCode', value)}
              disabled={isEdit} // Don't allow changing category in edit mode
            >
              <SelectTrigger className={formErrors.categoryCode ? 'border-red-500' : ''}>
                <SelectValue placeholder="Select category" />
              </SelectTrigger>
              <SelectContent>
                {categories.map((category) => (
                  <SelectItem key={category.value} value={category.value}>
                    {category.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {formErrors.categoryCode && (
              <p className="text-sm text-red-500">{formErrors.categoryCode}</p>
            )}
          </div>

          {/* Brand Selection */}
          <div className="grid gap-2">
            <Label htmlFor="brand" className="text-sm font-medium">
              Our Brand <span className="text-red-500">*</span>
            </Label>
            <Select
              value={formData.brandCode}
              onValueChange={(value) => handleFieldChange('brandCode', value)}
              disabled={isEdit || !formData.categoryCode} // Don't allow changing brand in edit mode
            >
              <SelectTrigger className={formErrors.brandCode ? 'border-red-500' : ''}>
                <SelectValue placeholder="Select brand" />
              </SelectTrigger>
              <SelectContent>
                {brands.map((brand) => (
                  <SelectItem key={brand.value} value={brand.value}>
                    {brand.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {formErrors.brandCode && (
              <p className="text-sm text-red-500">{formErrors.brandCode}</p>
            )}
          </div>

          {/* Competitor Selection */}
          <div className="grid gap-2">
            <div className="flex items-center justify-between mb-1">
              <Label htmlFor="competitor" className="text-sm font-medium">
                Competitor Company <span className="text-red-500">*</span>
              </Label>
              <div className="inline-flex items-center rounded-lg bg-muted p-1">
                <button
                  type="button"
                  onClick={() => {
                    setIsNewCompetitor(false);
                    setFormErrors(prev => ({ ...prev, competitorCode: '' }));
                  }}
                  className={`px-3 py-1.5 text-xs font-medium rounded-md transition-all ${
                    !isNewCompetitor 
                      ? 'bg-background text-foreground shadow-sm' 
                      : 'text-muted-foreground hover:text-foreground'
                  }`}
                >
                  Select Existing
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setIsNewCompetitor(true);
                    setFormErrors(prev => ({ ...prev, competitorCode: '' }));
                  }}
                  className={`px-3 py-1.5 text-xs font-medium rounded-md transition-all ${
                    isNewCompetitor 
                      ? 'bg-background text-foreground shadow-sm' 
                      : 'text-muted-foreground hover:text-foreground'
                  }`}
                >
                  Add New
                </button>
              </div>
            </div>
            
            {!isNewCompetitor ? (
              <>
                <Select
                  value={formData.competitorCode}
                  onValueChange={(value) => handleFieldChange('competitorCode', value)}
                >
                  <SelectTrigger className={formErrors.competitorCode && !isNewCompetitor ? 'border-red-500' : ''}>
                    <SelectValue placeholder="Select existing competitor" />
                  </SelectTrigger>
                  <SelectContent>
                    {competitors.length > 0 ? (
                      competitors.map((competitor) => (
                        <SelectItem key={competitor.value} value={competitor.value}>
                          {competitor.label}
                        </SelectItem>
                      ))
                    ) : (
                      <div className="py-2 px-3 text-sm text-muted-foreground">
                        No existing competitors found. Click "Add New" to create one.
                      </div>
                    )}
                  </SelectContent>
                </Select>
              </>
            ) : (
              <>
                <Input
                  placeholder="Enter new competitor name (e.g., Sony, Samsung, LG)"
                  value={newCompetitorName}
                  onChange={(e) => {
                    setNewCompetitorName(e.target.value);
                    if (formErrors.competitorCode) {
                      setFormErrors(prev => ({ ...prev, competitorCode: '' }));
                    }
                  }}
                  className={formErrors.competitorCode && isNewCompetitor ? 'border-red-500' : ''}
                />
                <p className="text-xs text-muted-foreground">
                  This will create a new competitor company in the system.
                </p>
              </>
            )}
            
            {formErrors.competitorCode && (
              <p className="text-sm text-red-500">{formErrors.competitorCode}</p>
            )}
          </div>

          {/* Summary */}
          {(selectedCategory || selectedBrand || formData.competitorCode || newCompetitorName) && (
            <div className="rounded-lg border p-4 bg-muted/50">
              <h4 className="text-sm font-medium mb-2">Summary</h4>
              <div className="space-y-1 text-sm">
                {selectedCategory && (
                  <p>
                    <span className="font-medium">Category:</span> {selectedCategory.label}
                  </p>
                )}
                {selectedBrand && (
                  <p>
                    <span className="font-medium">Brand:</span> {selectedBrand.label}
                  </p>
                )}
                {(isNewCompetitor ? newCompetitorName : formData.competitorCode) && (
                  <p>
                    <span className="font-medium">Competitor:</span> {isNewCompetitor ? newCompetitorName : formData.competitorCode}
                    {isNewCompetitor && <span className="text-xs text-muted-foreground ml-2">(new)</span>}
                  </p>
                )}
              </div>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={loading}>
            Back
          </Button>
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? 'Saving...' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}