'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { taxService, ITax, ITaxSkuMap } from '@/services/tax/tax.service';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { useToast } from '@/components/ui/use-toast';
import { ArrowLeft, Save, Loader2, Plus, X, Search } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';

interface SKU {
  UID: string;
  Code: string;
  Name: string;
}

const CreateTaxMasterPage = () => {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [searchingSKU, setSearchingSKU] = useState(false);
  const [skuSearchTerm, setSkuSearchTerm] = useState('');
  const [availableSKUs, setAvailableSKUs] = useState<SKU[]>([]);
  const [selectedSKUs, setSelectedSKUs] = useState<string[]>([]);
  
  const [formData, setFormData] = useState<{
    tax: Partial<ITax>;
    skuMappings: string[];
  }>({
    tax: {
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
    },
    skuMappings: []
  });

  // Generate unique UID
  const generateUID = () => {
    const timestamp = Date.now().toString(36).toUpperCase();
    const random = Math.random().toString(36).substring(2, 8).toUpperCase();
    return `TAX-MASTER-${timestamp}-${random}`;
  };

  useEffect(() => {
    setFormData(prev => ({
      ...prev,
      tax: {
        ...prev.tax,
        UID: generateUID()
      }
    }));
    // Load available SKUs (in production, this would be an API call)
    loadMockSKUs();
  }, []);

  const loadMockSKUs = () => {
    // Mock SKUs - in production, load from API
    setAvailableSKUs([
      { UID: 'SKU-0045', Code: 'SKU045', Name: 'Product SKU 045' },
      { UID: 'SKU-0046', Code: 'SKU046', Name: 'Product SKU 046' },
      { UID: 'SKU-0047', Code: 'SKU047', Name: 'Product SKU 047' },
      { UID: 'SKU-0048', Code: 'SKU048', Name: 'Product SKU 048' },
      { UID: 'SKU-0049', Code: 'SKU049', Name: 'Product SKU 049' },
    ]);
  };

  const handleTaxFieldChange = (field: keyof ITax, value: any) => {
    setFormData(prev => ({
      ...prev,
      tax: {
        ...prev.tax,
        [field]: value
      }
    }));
  };

  const toggleSKUSelection = (skuUID: string) => {
    setSelectedSKUs(prev => 
      prev.includes(skuUID) 
        ? prev.filter(id => id !== skuUID)
        : [...prev, skuUID]
    );
  };

  const validateForm = (): boolean => {
    if (!formData.tax.Name?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'Tax Name is required',
        variant: 'destructive'
      });
      return false;
    }
    if (!formData.tax.Code?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'Tax Code is required',
        variant: 'destructive'
      });
      return false;
    }
    if (!formData.tax.BaseTaxRate || formData.tax.BaseTaxRate < 0 || formData.tax.BaseTaxRate > 100) {
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
      
      // Prepare tax object
      const taxData: ITax = {
        ...formData.tax as ITax,
        ValidFrom: formData.tax.ValidFrom ? `${formData.tax.ValidFrom}T00:00:00` : currentTime,
        ValidUpto: formData.tax.ValidUpto ? `${formData.tax.ValidUpto}T23:59:59` : '2099-12-31T23:59:59',
        CreatedBy: 'ADMIN',
        CreatedTime: currentTime,
        ModifiedBy: 'ADMIN',
        ModifiedTime: currentTime,
        ServerAddTime: currentTime,
        ServerModifiedTime: currentTime,
      };

      // Prepare SKU mappings
      const skuMapList = selectedSKUs.map(skuUID => ({
        UID: `TAXSKUMAP-${Date.now().toString(36).toUpperCase()}-${Math.random().toString(36).substring(2, 5).toUpperCase()}`,
        SKUUID: skuUID,
        TaxUID: taxData.UID,
        ActionType: 1, // Add action
        CreatedBy: 'ADMIN',
        CreatedTime: currentTime,
        ModifiedBy: 'ADMIN',
        ModifiedTime: currentTime,
        ServerAddTime: currentTime,
        ServerModifiedTime: currentTime,
      }));

      // Call CreateTaxMaster API
      const payload = {
        Tax: taxData,
        TaxSKUMapList: skuMapList
      };

      console.log('Creating Tax Master:', payload);
      
      // Call the createTaxMaster API
      const result = await taxService.createTaxMaster(payload);
      
      if (result) {
        toast({
          title: 'Success',
          description: `Tax Master created successfully with ${selectedSKUs.length} SKU mappings`,
        });
      }
      
      router.push('/administration/tax-configuration/taxes');
    } catch (error: any) {
      console.error('Error creating tax master:', error);
      toast({
        title: 'Error',
        description: error.message || 'Failed to create tax master',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const filteredSKUs = availableSKUs.filter(sku => 
    sku.Code.toLowerCase().includes(skuSearchTerm.toLowerCase()) ||
    sku.Name.toLowerCase().includes(skuSearchTerm.toLowerCase())
  );

  return (
    <div className="container mx-auto p-6 max-w-6xl">
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
        <h1 className="text-3xl font-bold">Create Tax Master</h1>
        <p className="text-gray-600 mt-2">
          Create a tax with SKU mappings
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Left Column - Tax Details */}
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Tax Information</CardTitle>
                <CardDescription>
                  Define the tax configuration
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="uid">Tax UID *</Label>
                    <Input
                      id="uid"
                      value={formData.tax.UID}
                      onChange={(e) => handleTaxFieldChange('UID', e.target.value)}
                      required
                    />
                  </div>
                  <div>
                    <Label htmlFor="code">Tax Code *</Label>
                    <Input
                      id="code"
                      value={formData.tax.Code}
                      onChange={(e) => handleTaxFieldChange('Code', e.target.value.toUpperCase())}
                      placeholder="e.g., GST18"
                      required
                    />
                  </div>
                </div>

                <div>
                  <Label htmlFor="name">Tax Name *</Label>
                  <Input
                    id="name"
                    value={formData.tax.Name}
                    onChange={(e) => handleTaxFieldChange('Name', e.target.value)}
                    placeholder="e.g., GST 18%"
                    required
                  />
                </div>

                <div>
                  <Label htmlFor="legalName">Legal Name</Label>
                  <Input
                    id="legalName"
                    value={formData.tax.LegalName}
                    onChange={(e) => handleTaxFieldChange('LegalName', e.target.value)}
                    placeholder="Official tax name"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="taxRate">Tax Rate (%) *</Label>
                    <Input
                      id="taxRate"
                      type="number"
                      step="0.01"
                      min="0"
                      max="100"
                      value={formData.tax.BaseTaxRate}
                      onChange={(e) => handleTaxFieldChange('BaseTaxRate', parseFloat(e.target.value) || 0)}
                      required
                    />
                  </div>
                  <div>
                    <Label htmlFor="calculationType">Type</Label>
                    <Select
                      value={formData.tax.TaxCalculationType}
                      onValueChange={(value) => handleTaxFieldChange('TaxCalculationType', value)}
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
                      value={formData.tax.ApplicableAt}
                      onValueChange={(value) => handleTaxFieldChange('ApplicableAt', value)}
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
                      value={formData.tax.Status}
                      onValueChange={(value) => handleTaxFieldChange('Status', value)}
                    >
                      <SelectTrigger id="status">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Active">Active</SelectItem>
                        <SelectItem value="Inactive">Inactive</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="validFrom">Valid From</Label>
                    <Input
                      id="validFrom"
                      type="date"
                      value={formData.tax.ValidFrom?.split('T')[0]}
                      onChange={(e) => handleTaxFieldChange('ValidFrom', e.target.value)}
                    />
                  </div>
                  <div>
                    <Label htmlFor="validUpto">Valid Until</Label>
                    <Input
                      id="validUpto"
                      type="date"
                      value={formData.tax.ValidUpto?.split('T')[0]}
                      onChange={(e) => handleTaxFieldChange('ValidUpto', e.target.value)}
                    />
                  </div>
                </div>

                <div className="flex items-center space-x-2">
                  <Switch
                    id="taxOnTax"
                    checked={formData.tax.IsTaxOnTaxApplicable}
                    onCheckedChange={(checked) => handleTaxFieldChange('IsTaxOnTaxApplicable', checked)}
                  />
                  <Label htmlFor="taxOnTax">Enable Tax on Tax</Label>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Right Column - SKU Mappings */}
          <div className="space-y-6">
            <Card className="h-full">
              <CardHeader>
                <CardTitle>SKU Mappings</CardTitle>
                <CardDescription>
                  Select SKUs to apply this tax
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                    <Input
                      type="text"
                      placeholder="Search SKUs..."
                      value={skuSearchTerm}
                      onChange={(e) => setSkuSearchTerm(e.target.value)}
                      className="pl-10"
                    />
                  </div>

                  <div className="border rounded-md">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead className="w-12"></TableHead>
                          <TableHead>SKU Code</TableHead>
                          <TableHead>Name</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {filteredSKUs.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={3} className="text-center py-4">
                              No SKUs found
                            </TableCell>
                          </TableRow>
                        ) : (
                          filteredSKUs.map(sku => (
                            <TableRow 
                              key={sku.UID}
                              className={selectedSKUs.includes(sku.UID) ? 'bg-blue-50' : ''}
                            >
                              <TableCell>
                                <input
                                  type="checkbox"
                                  checked={selectedSKUs.includes(sku.UID)}
                                  onChange={() => toggleSKUSelection(sku.UID)}
                                  className="h-4 w-4"
                                />
                              </TableCell>
                              <TableCell className="font-mono text-sm">
                                {sku.Code}
                              </TableCell>
                              <TableCell>{sku.Name}</TableCell>
                            </TableRow>
                          ))
                        )}
                      </TableBody>
                    </Table>
                  </div>

                  <div className="flex items-center justify-between">
                    <p className="text-sm text-gray-600">
                      {selectedSKUs.length} SKU(s) selected
                    </p>
                    {selectedSKUs.length > 0 && (
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => setSelectedSKUs([])}
                      >
                        Clear Selection
                      </Button>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Form Actions */}
        <div className="flex justify-end space-x-4 mt-6">
          <Button
            type="button"
            variant="outline"
            onClick={() => router.push('/administration/tax-configuration/taxes')}
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
                Create Tax Master
              </>
            )}
          </Button>
        </div>
      </form>
    </div>
  );
};

export default CreateTaxMasterPage;