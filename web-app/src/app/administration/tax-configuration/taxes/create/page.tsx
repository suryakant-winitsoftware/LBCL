'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
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

const CreateTaxPage = () => {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<Partial<ITax>>({
    UID: '',
    Name: '',
    LegalName: '',
    Code: '',
    ApplicableAt: 'Item',
    TaxCalculationType: 'Percentage',
    BaseTaxRate: 0,
    Status: 'Active',
    ValidFrom: new Date().toISOString().split('T')[0],
    ValidUpto: '2099-12-31',
    IsTaxOnTaxApplicable: false,
  });

  // Auto-set UID to be the same as Code
  React.useEffect(() => {
    if (formData.Code) {
      setFormData(prev => ({
        ...prev,
        UID: formData.Code.toUpperCase()
      }));
    }
  }, [formData.Code]);

  const handleInputChange = (field: keyof ITax, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const validateForm = (): boolean => {
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
        ValidFrom: formData.ValidFrom ? `${formData.ValidFrom}T00:00:00` : currentTime,
        ValidUpto: formData.ValidUpto ? `${formData.ValidUpto}T23:59:59` : '2099-12-31T23:59:59',
        CreatedBy: 'ADMIN', // Would come from auth context in production
        CreatedTime: currentTime,
        ModifiedBy: 'ADMIN',
        ModifiedTime: currentTime,
        ServerAddTime: currentTime,
        ServerModifiedTime: currentTime,
      };

      const result = await taxService.createTax(payload);
      
      if (result) {
        toast({
          title: 'Success',
          description: 'Tax created successfully',
        });
        router.push('/administration/tax-configuration/taxes');
      }
    } catch (error: any) {
      console.error('Error creating tax:', error);
      toast({
        title: 'Error',
        description: error.message || 'Failed to create tax',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    router.push('/administration/tax-configuration/taxes');
  };

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
        <h1 className="text-3xl font-bold">Create New Tax</h1>
        <p className="text-gray-600 mt-2">
          Define a new tax configuration for your products
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="space-y-6">
          {/* Basic Information */}
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>
                Enter the fundamental details of the tax
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <Label htmlFor="code">Tax Code *</Label>
                <Input
                  id="code"
                  value={formData.Code}
                  onChange={(e) => handleInputChange('Code', e.target.value.toUpperCase())}
                  placeholder="e.g., GST18, VAT20"
                  required
                />
                <p className="text-xs text-gray-500 mt-1">Short code for quick reference</p>
              </div>

              <div>
                <Label htmlFor="name">Tax Name *</Label>
                <Input
                  id="name"
                  value={formData.Name}
                  onChange={(e) => handleInputChange('Name', e.target.value)}
                  placeholder="e.g., Goods and Services Tax 18%"
                  required
                />
              </div>

              <div>
                <Label htmlFor="legalName">Legal Name</Label>
                <Input
                  id="legalName"
                  value={formData.LegalName}
                  onChange={(e) => handleInputChange('LegalName', e.target.value)}
                  placeholder="Official legal name of the tax"
                />
              </div>
            </CardContent>
          </Card>

          {/* Tax Configuration */}
          <Card>
            <CardHeader>
              <CardTitle>Tax Configuration</CardTitle>
              <CardDescription>
                Configure how the tax should be calculated and applied
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="taxRate">Base Tax Rate (%) *</Label>
                  <Input
                    id="taxRate"
                    type="number"
                    step="0.01"
                    min="0"
                    max="100"
                    value={formData.BaseTaxRate}
                    onChange={(e) => handleInputChange('BaseTaxRate', parseFloat(e.target.value) || 0)}
                    placeholder="e.g., 18.00"
                    required
                  />
                </div>
                <div>
                  <Label htmlFor="calculationType">Calculation Type</Label>
                  <Select
                    value={formData.TaxCalculationType}
                    onValueChange={(value) => handleInputChange('TaxCalculationType', value)}
                  >
                    <SelectTrigger id="calculationType">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Percentage">Percentage</SelectItem>
                      <SelectItem value="FixedAmount">Fixed Amount</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="applicableAt">Applicable At</Label>
                  <Select
                    value={formData.ApplicableAt}
                    onValueChange={(value) => handleInputChange('ApplicableAt', value)}
                  >
                    <SelectTrigger id="applicableAt">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Item">Item</SelectItem>
                      <SelectItem value="Order">Order</SelectItem>
                      <SelectItem value="Invoice">Invoice</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <Label htmlFor="status">Status</Label>
                  <Select
                    value={formData.Status}
                    onValueChange={(value) => handleInputChange('Status', value)}
                  >
                    <SelectTrigger id="status">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Active">Active</SelectItem>
                      <SelectItem value="Inactive">Inactive</SelectItem>
                      <SelectItem value="Draft">Draft</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="flex items-center space-x-2">
                <Switch
                  id="taxOnTax"
                  checked={formData.IsTaxOnTaxApplicable}
                  onCheckedChange={(checked) => handleInputChange('IsTaxOnTaxApplicable', checked)}
                />
                <Label htmlFor="taxOnTax">
                  Enable Tax on Tax
                  <span className="text-xs text-gray-500 ml-2">
                    (Calculate this tax on top of other taxes)
                  </span>
                </Label>
              </div>
            </CardContent>
          </Card>

          {/* Validity Period */}
          <Card>
            <CardHeader>
              <CardTitle>Validity Period</CardTitle>
              <CardDescription>
                Specify when this tax configuration is valid
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="validFrom">Valid From</Label>
                  <Input
                    id="validFrom"
                    type="date"
                    value={formData.ValidFrom?.split('T')[0]}
                    onChange={(e) => handleInputChange('ValidFrom', e.target.value)}
                  />
                </div>
                <div>
                  <Label htmlFor="validUpto">Valid Until</Label>
                  <Input
                    id="validUpto"
                    type="date"
                    value={formData.ValidUpto?.split('T')[0]}
                    onChange={(e) => handleInputChange('ValidUpto', e.target.value)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Form Actions */}
          <div className="flex justify-end space-x-4">
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
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Creating...
                </>
              ) : (
                <>
                  <Save className="mr-2 h-4 w-4" />
                  Create Tax
                </>
              )}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default CreateTaxPage;