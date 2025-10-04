'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { taxService, ITax, ITaxGroup, ITaxGroupTaxes, PagingRequest } from '@/services/tax/tax.service';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { useToast } from '@/components/ui/use-toast';
import { ArrowLeft, Save, Loader2, Search, Plus, X } from 'lucide-react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';

const CreateTaxGroupPage = () => {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [loadingTaxes, setLoadingTaxes] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [availableTaxes, setAvailableTaxes] = useState<ITax[]>([]);
  const [selectedTaxes, setSelectedTaxes] = useState<string[]>([]);
  
  const [formData, setFormData] = useState<Partial<ITaxGroup>>({
    UID: '',
    Name: '',
    Code: '',
    ActionType: 0,
  });

  // Auto-set UID to be the same as Code
  useEffect(() => {
    if (formData.Code) {
      setFormData(prev => ({
        ...prev,
        UID: formData.Code.toUpperCase()
      }));
    }
  }, [formData.Code]);

  useEffect(() => {
    loadAvailableTaxes();
  }, []);

  const loadAvailableTaxes = async () => {
    setLoadingTaxes(true);
    try {
      const request: PagingRequest = {
        pageNumber: 1,
        pageSize: 100, // Load up to 100 taxes
        isCountRequired: false,
        filterCriterias: [],
        sortCriterias: [
          {
            sortParameter: 'Name',
            direction: 'Asc',
          },
        ],
      };

      const response = await taxService.getTaxDetails(request);
      setAvailableTaxes(response.PagedData);
    } catch (error) {
      console.error('Error loading taxes:', error);
      toast({
        title: 'Error',
        description: 'Failed to load available taxes',
        variant: 'destructive',
      });
    } finally {
      setLoadingTaxes(false);
    }
  };

  const handleInputChange = (field: keyof ITaxGroup, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const toggleTaxSelection = (taxUID: string) => {
    setSelectedTaxes(prev => 
      prev.includes(taxUID) 
        ? prev.filter(id => id !== taxUID)
        : [...prev, taxUID]
    );
  };

  const selectAllTaxes = () => {
    if (selectedTaxes.length === filteredTaxes.length) {
      setSelectedTaxes([]);
    } else {
      setSelectedTaxes(filteredTaxes.map(tax => tax.UID));
    }
  };

  const validateForm = (): boolean => {
    if (!formData.Name?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'Tax Group Name is required',
        variant: 'destructive'
      });
      return false;
    }
    if (!formData.Code?.trim()) {
      toast({
        title: 'Validation Error',
        description: 'Tax Group Code is required',
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
      
      // Prepare tax group object
      const taxGroupData: ITaxGroup = {
        ...formData as ITaxGroup,
        CreatedBy: 'ADMIN',
        CreatedTime: currentTime,
        ModifiedBy: 'ADMIN',
        ModifiedTime: currentTime,
        ServerAddTime: currentTime,
        ServerModifiedTime: currentTime,
      };

      // Prepare tax group taxes mappings
      const taxGroupTaxes: ITaxGroupTaxes[] = selectedTaxes.map(taxUID => ({
        UID: `TGTAX-${Date.now().toString(36).toUpperCase()}-${Math.random().toString(36).substring(2, 5).toUpperCase()}`,
        TaxGroupUID: taxGroupData.UID,
        TaxUID: taxUID,
        ActionType: 1, // Add action
        CreatedBy: 'ADMIN',
        CreatedTime: currentTime,
        ModifiedBy: 'ADMIN',
        ModifiedTime: currentTime,
        ServerAddTime: currentTime,
        ServerModifiedTime: currentTime,
      }));

      // Call CreateTaxGroupMaster API
      const payload = {
        TaxGroup: taxGroupData,
        TaxGroupTaxes: taxGroupTaxes
      };

      console.log('Creating Tax Group Master:', payload);
      
      const result = await taxService.createTaxGroupMaster(payload);
      
      if (result) {
        toast({
          title: 'Success',
          description: `Tax Group created successfully with ${selectedTaxes.length} tax mappings`,
        });
        router.push('/administration/tax-configuration/taxes');
      }
    } catch (error: any) {
      console.error('Error creating tax group:', error);
      toast({
        title: 'Error',
        description: error.message || 'Failed to create tax group',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const filteredTaxes = availableTaxes.filter(tax => 
    tax.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    tax.Code.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const formatTaxRate = (rate: number) => {
    return `${rate}%`;
  };

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
        <h1 className="text-3xl font-bold">Create Tax Group</h1>
        <p className="text-gray-600 mt-2">
          Group multiple taxes together for easier management
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Left Column - Tax Group Details */}
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Tax Group Information</CardTitle>
                <CardDescription>
                  Define the tax group configuration
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <Label htmlFor="code">Tax Group Code *</Label>
                  <Input
                    id="code"
                    value={formData.Code}
                    onChange={(e) => handleInputChange('Code', e.target.value.toUpperCase())}
                    placeholder="e.g., GSTGROUP, VATGROUP"
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">Short code for quick reference</p>
                </div>

                <div>
                  <Label htmlFor="name">Tax Group Name *</Label>
                  <Input
                    id="name"
                    value={formData.Name}
                    onChange={(e) => handleInputChange('Name', e.target.value)}
                    placeholder="e.g., GST Tax Group, VAT Tax Group"
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">Descriptive name for the tax group</p>
                </div>

                <div className="pt-4">
                  <h3 className="font-medium mb-2">Group Summary</h3>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Selected Taxes:</span>
                      <span className="font-medium">{selectedTaxes.length}</span>
                    </div>
                    {selectedTaxes.length > 0 && (
                      <div className="mt-3 p-3 bg-blue-50 rounded-md">
                        <p className="text-blue-800 font-medium mb-2">Combined Tax Rates:</p>
                        <div className="space-y-1">
                          {selectedTaxes.map(taxUID => {
                            const tax = availableTaxes.find(t => t.UID === taxUID);
                            if (!tax) return null;
                            return (
                              <div key={taxUID} className="flex justify-between text-xs">
                                <span>{tax.Name}:</span>
                                <span className="font-mono">{formatTaxRate(tax.BaseTaxRate)}</span>
                              </div>
                            );
                          })}
                          <div className="pt-2 border-t border-blue-200 flex justify-between font-medium">
                            <span>Total:</span>
                            <span className="font-mono">
                              {formatTaxRate(
                                selectedTaxes.reduce((sum, uid) => {
                                  const tax = availableTaxes.find(t => t.UID === uid);
                                  return sum + (tax?.BaseTaxRate || 0);
                                }, 0)
                              )}
                            </span>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Right Column - Tax Selection */}
          <div className="space-y-6">
            <Card className="h-full">
              <CardHeader>
                <CardTitle>Select Taxes</CardTitle>
                <CardDescription>
                  Choose taxes to include in this group
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                    <Input
                      type="text"
                      placeholder="Search taxes..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="pl-10"
                    />
                  </div>

                  {loadingTaxes ? (
                    <div className="text-center py-8">
                      <Loader2 className="h-6 w-6 animate-spin mx-auto mb-2" />
                      <p className="text-sm text-gray-600">Loading available taxes...</p>
                    </div>
                  ) : (
                    <>
                      <div className="flex justify-between items-center">
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={selectAllTaxes}
                        >
                          {selectedTaxes.length === filteredTaxes.length ? 'Deselect All' : 'Select All'}
                        </Button>
                        <p className="text-sm text-gray-600">
                          {selectedTaxes.length} of {filteredTaxes.length} selected
                        </p>
                      </div>

                      <div className="border rounded-md max-h-[400px] overflow-y-auto">
                        <Table>
                          <TableHeader>
                            <TableRow>
                              <TableHead className="w-12 sticky top-0 bg-white"></TableHead>
                              <TableHead className="sticky top-0 bg-white">Tax Name</TableHead>
                              <TableHead className="sticky top-0 bg-white">Code</TableHead>
                              <TableHead className="text-right sticky top-0 bg-white">Rate</TableHead>
                            </TableRow>
                          </TableHeader>
                          <TableBody>
                            {filteredTaxes.length === 0 ? (
                              <TableRow>
                                <TableCell colSpan={4} className="text-center py-4">
                                  No taxes found
                                </TableCell>
                              </TableRow>
                            ) : (
                              filteredTaxes.map(tax => (
                                <TableRow 
                                  key={tax.UID}
                                  className={selectedTaxes.includes(tax.UID) ? 'bg-blue-50' : ''}
                                >
                                  <TableCell>
                                    <Checkbox
                                      checked={selectedTaxes.includes(tax.UID)}
                                      onCheckedChange={() => toggleTaxSelection(tax.UID)}
                                    />
                                  </TableCell>
                                  <TableCell>
                                    <div>
                                      <p className="font-medium">{tax.Name}</p>
                                      {tax.LegalName && (
                                        <p className="text-xs text-gray-500">{tax.LegalName}</p>
                                      )}
                                    </div>
                                  </TableCell>
                                  <TableCell>
                                    <span className="font-mono text-sm">{tax.Code}</span>
                                  </TableCell>
                                  <TableCell className="text-right">
                                    <Badge variant="outline">
                                      {formatTaxRate(tax.BaseTaxRate)}
                                    </Badge>
                                  </TableCell>
                                </TableRow>
                              ))
                            )}
                          </TableBody>
                        </Table>
                      </div>

                      {selectedTaxes.length > 0 && (
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => setSelectedTaxes([])}
                          className="w-full"
                        >
                          Clear Selection
                        </Button>
                      )}
                    </>
                  )}
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
          <Button type="submit" disabled={loading || loadingTaxes}>
            {loading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Creating...
              </>
            ) : (
              <>
                <Save className="mr-2 h-4 w-4" />
                Create Tax Group
              </>
            )}
          </Button>
        </div>
      </form>
    </div>
  );
};

export default CreateTaxGroupPage;