'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { taxService, ITax } from '@/services/tax/tax.service';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { useToast } from '@/components/ui/use-toast';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

const EditTaxPage = () => {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [formData, setFormData] = useState<Partial<ITax>>({
    UID: '',
    Name: '',
    LegalName: '',
    Code: '',
    ApplicableAt: 'Item',
    TaxCalculationType: 'Percentage',
    BaseTaxRate: 0,
    Status: 'Active',
    ValidFrom: '',
    ValidUpto: '',
    IsTaxOnTaxApplicable: false,
  });

  const taxId = params.id as string;

  // Load existing tax data
  useEffect(() => {
    const loadTaxData = async () => {
      if (!taxId) return;
      
      setLoadingData(true);
      try {
        const tax = await taxService.getTaxByUID(taxId);
        if (tax) {
          setFormData({
            ...tax,
            ValidFrom: tax.ValidFrom ? tax.ValidFrom.split('T')[0] : '',
            ValidUpto: tax.ValidUpto ? tax.ValidUpto.split('T')[0] : '',
          });
        } else {
          toast({
            title: 'Error',
            description: 'Tax not found',
            variant: 'destructive',
          });
          router.push('/administration/tax-configuration/taxes');
        }
      } catch (error) {
        console.error('Error loading tax:', error);
        toast({
          title: 'Error',
          description: 'Failed to load tax data',
          variant: 'destructive',
        });
        router.push('/administration/tax-configuration/taxes');
      } finally {
        setLoadingData(false);
      }
    };

    loadTaxData();
  }, [taxId]);

  const handleInputChange = (field: keyof ITax, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const validateForm = () => {
    if (!formData.Name?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'Tax Name is required',
        variant: 'destructive'
      });
      return false;
    }
    if (!formData.Code?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'Tax Code is required',
        variant: 'destructive'
      });
      return false;
    }
    if (formData.BaseTaxRate === undefined || formData.BaseTaxRate < 0 || formData.BaseTaxRate > 100) {
      toast({
        title: 'Validation Error',
        description: 'Tax Rate must be between 0 and 100',
        variant: 'destructive'
      });
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      const currentTime = new Date().toISOString();
      const payload: ITax = {
        ...formData as ITax,
        ValidFrom: formData.ValidFrom ? `${formData.ValidFrom}T00:00:00` : undefined,
        ValidUpto: formData.ValidUpto ? `${formData.ValidUpto}T23:59:59` : undefined,
        ModifiedBy: 'ADMIN',
        ModifiedTime: currentTime,
      };

      const result = await taxService.updateTax(payload);
      
      if (result) {
        toast({
          title: 'Success',
          description: 'Tax updated successfully',
        });
        router.push('/administration/tax-configuration/taxes');
      }
    } catch (error: any) {
      console.error('Error updating tax:', error);
      toast({
        title: 'Error',
        description: error.message || 'Failed to update tax',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    router.push('/administration/tax-configuration/taxes');
  };

  if (loadingData) {
    return (
      <div className="container mx-auto p-6 max-w-4xl">
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin mr-2" />
          <span>Loading tax data...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 max-w-4xl">
      <div className="mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="mb-4"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <h1 className="text-3xl font-bold">Edit Tax</h1>
        <p className="text-gray-600 mt-2">
          Modify the tax configuration: {formData.Name}
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="space-y-6">
          {/* Basic Information */}
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>
                Modify the fundamental details of the tax
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="uid">Tax UID *</Label>
                  <Input
                    id="uid"
                    value={formData.UID}
                    disabled
                    className="bg-gray-50"
                  />
                  <p className="text-xs text-gray-500 mt-1">Unique identifier (cannot be changed)</p>
                </div>
                <div>
                  <Label htmlFor="code">Tax Code *</Label>
                  <Input
                    id="code"
                    value={formData.Code}
                    onChange={(e) => handleInputChange('Code', e.target.value)}
                    placeholder="Enter tax code"
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">Short code for easy reference</p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="name">Tax Name *</Label>
                  <Input
                    id="name"
                    value={formData.Name}
                    onChange={(e) => handleInputChange('Name', e.target.value)}
                    placeholder="Enter tax name"
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">Display name for this tax</p>
                </div>
                <div>
                  <Label htmlFor="legalName">Legal Name</Label>
                  <Input
                    id="legalName"
                    value={formData.LegalName || ''}
                    onChange={(e) => handleInputChange('LegalName', e.target.value)}
                    placeholder="Legal name (optional)"
                  />
                  <p className="text-xs text-gray-500 mt-1">Official legal name if different</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Tax Configuration */}
          <Card>
            <CardHeader>
              <CardTitle>Tax Configuration</CardTitle>
              <CardDescription>
                Set how this tax is calculated and applied
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <Label htmlFor="applicableAt">Applicable At *</Label>
                  <Select
                    value={formData.ApplicableAt}
                    onValueChange={(value) => handleInputChange('ApplicableAt', value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select level" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Item">Item Level</SelectItem>
                      <SelectItem value="Order">Order Level</SelectItem>
                      <SelectItem value="Invoice">Invoice Level</SelectItem>
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-gray-500 mt-1">Where this tax applies</p>
                </div>
                <div>
                  <Label htmlFor="calculationType">Calculation Type *</Label>
                  <Select
                    value={formData.TaxCalculationType}
                    onValueChange={(value) => handleInputChange('TaxCalculationType', value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Percentage">Percentage</SelectItem>
                      <SelectItem value="Fixed">Fixed Amount</SelectItem>
                      <SelectItem value="Slab">Slab Based</SelectItem>
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-gray-500 mt-1">How tax is calculated</p>
                </div>
                <div>
                  <Label htmlFor="baseTaxRate">
                    Base Tax Rate * {formData.TaxCalculationType === 'Percentage' ? '(%)' : '($)'}
                  </Label>
                  <Input
                    id="baseTaxRate"
                    type="number"
                    step="0.01"
                    min="0"
                    max={formData.TaxCalculationType === 'Percentage' ? '100' : undefined}
                    value={formData.BaseTaxRate}
                    onChange={(e) => handleInputChange('BaseTaxRate', parseFloat(e.target.value) || 0)}
                    placeholder="0.00"
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    {formData.TaxCalculationType === 'Percentage' ? 'Percentage rate (0-100%)' : 'Fixed amount in dollars'}
                  </p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="status">Status *</Label>
                  <Select
                    value={formData.Status}
                    onValueChange={(value) => handleInputChange('Status', value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select status" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Active">Active</SelectItem>
                      <SelectItem value="Inactive">Inactive</SelectItem>
                      <SelectItem value="Pending">Pending</SelectItem>
                      <SelectItem value="Expired">Expired</SelectItem>
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-gray-500 mt-1">Current status of this tax</p>
                </div>
                <div className="flex items-center space-x-2 pt-6">
                  <Switch
                    id="isTaxOnTaxApplicable"
                    checked={formData.IsTaxOnTaxApplicable || false}
                    onCheckedChange={(checked) => handleInputChange('IsTaxOnTaxApplicable', checked)}
                  />
                  <Label htmlFor="isTaxOnTaxApplicable" className="text-sm">
                    Enable Tax on Tax (Compound Tax)
                  </Label>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Validity Period */}
          <Card>
            <CardHeader>
              <CardTitle>Validity Period</CardTitle>
              <CardDescription>
                Set the date range when this tax is applicable
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="validFrom">Valid From</Label>
                  <Input
                    id="validFrom"
                    type="date"
                    value={formData.ValidFrom}
                    onChange={(e) => handleInputChange('ValidFrom', e.target.value)}
                  />
                  <p className="text-xs text-gray-500 mt-1">Start date for tax applicability</p>
                </div>
                <div>
                  <Label htmlFor="validUpto">Valid Until</Label>
                  <Input
                    id="validUpto"
                    type="date"
                    value={formData.ValidUpto}
                    onChange={(e) => handleInputChange('ValidUpto', e.target.value)}
                  />
                  <p className="text-xs text-gray-500 mt-1">End date for tax applicability</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Form Actions */}
          <div className="flex justify-end gap-4 pt-6">
            <Button 
              type="button" 
              variant="outline" 
              onClick={handleCancel}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Updating Tax...
                </>
              ) : (
                <>
                  <Save className="h-4 w-4 mr-2" />
                  Update Tax
                </>
              )}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default EditTaxPage;