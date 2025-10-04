'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { taxService, ITaxGroup, ITaxSelectionItem, ITaxGroupMaster, ITaxGroupTaxes } from '@/services/tax/tax.service';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
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

const EditTaxGroupPage = () => {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [availableTaxes, setAvailableTaxes] = useState<ITaxSelectionItem[]>([]);
  const [searchTerm, setSearchTerm] = useState('');

  const [formData, setFormData] = useState<ITaxGroup>({
    UID: '',
    Name: '',
    Code: '',
    ActionType: 1, // Update
    CreatedBy: '',
    CreatedTime: '',
    ModifiedBy: 'ADMIN',
    ModifiedTime: new Date().toISOString(),
    IsSelected: false
  });

  const [selectedTaxes, setSelectedTaxes] = useState<ITaxGroupTaxes[]>([]);
  const [originalTaxes, setOriginalTaxes] = useState<ITaxGroupTaxes[]>([]);

  const groupId = params.id as string;

  // Load existing tax group data
  useEffect(() => {
    const loadTaxGroupData = async () => {
      if (!groupId) return;

      setLoadingData(true);
      try {
        // Load tax group details
        const taxGroup = await taxService.getTaxGroupByUID(groupId);
        if (taxGroup) {
          setFormData(taxGroup);

          // Load available taxes and current tax group taxes
          const [taxes] = await Promise.all([
            taxService.getTaxSelectionItems(groupId)
          ]);

          setAvailableTaxes(taxes);

          // Convert selected taxes to ITaxGroupTaxes format
          const currentTaxes: ITaxGroupTaxes[] = taxes
            .filter(tax => tax.IsSelected)
            .map(tax => ({
              UID: `${groupId}_${tax.TaxUID}`,
              TaxGroupUID: groupId,
              TaxUID: tax.TaxUID,
              ActionType: 0, // No change initially
              CreatedBy: 'ADMIN',
              CreatedTime: new Date().toISOString(),
              ModifiedBy: 'ADMIN',
              ModifiedTime: new Date().toISOString()
            }));

          setSelectedTaxes(currentTaxes);
          setOriginalTaxes([...currentTaxes]);
        } else {
          toast({
            title: 'Error',
            description: 'Tax group not found',
            variant: 'destructive',
          });
          router.push('/administration/tax-configuration/tax-groups');
        }
      } catch (error) {
        console.error('Error loading tax group:', error);
        toast({
          title: 'Error',
          description: 'Failed to load tax group data',
          variant: 'destructive',
        });
        router.push('/administration/tax-configuration/tax-groups');
      } finally {
        setLoadingData(false);
      }
    };

    loadTaxGroupData();
  }, [groupId]);

  const handleInputChange = (field: keyof ITaxGroup, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleTaxToggle = (taxUID: string, taxName: string) => {
    const existingIndex = selectedTaxes.findIndex(t => t.TaxUID === taxUID);
    const wasOriginallySelected = originalTaxes.some(t => t.TaxUID === taxUID);

    if (existingIndex >= 0) {
      // Tax is currently selected, remove it
      if (wasOriginallySelected) {
        // Mark for deletion if it was originally selected
        setSelectedTaxes(prev => prev.map(tax =>
          tax.TaxUID === taxUID
            ? { ...tax, ActionType: 2 } // Delete
            : tax
        ));
      } else {
        // Remove completely if it was just added
        setSelectedTaxes(prev => prev.filter(t => t.TaxUID !== taxUID));
      }
    } else {
      // Tax is not selected, add it
      if (wasOriginallySelected) {
        // Restore if it was originally selected but marked for deletion
        setSelectedTaxes(prev => prev.map(tax =>
          tax.TaxUID === taxUID
            ? { ...tax, ActionType: 0 } // No change
            : tax
        ));
      } else {
        // Add new tax
        const newTax: ITaxGroupTaxes = {
          UID: `${groupId}_${taxUID}_${Date.now()}`,
          TaxGroupUID: groupId,
          TaxUID: taxUID,
          ActionType: 1, // Create
          CreatedBy: 'ADMIN',
          CreatedTime: new Date().toISOString(),
          ModifiedBy: 'ADMIN',
          ModifiedTime: new Date().toISOString()
        };
        setSelectedTaxes(prev => [...prev, newTax]);
      }
    }
  };

  const isTaxSelected = (taxUID: string) => {
    const tax = selectedTaxes.find(t => t.TaxUID === taxUID);
    return tax && tax.ActionType !== 2; // Not marked for deletion
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
      const payload: ITaxGroupMaster = {
        TaxGroup: {
          ...formData,
          ActionType: 1, // Update
          ModifiedBy: 'ADMIN',
          ModifiedTime: new Date().toISOString(),
        },
        TaxGroupTaxes: selectedTaxes.filter(tax => tax.ActionType !== 0) // Only include changes
      };

      const result = await taxService.updateTaxGroupMaster(payload);

      if (result) {
        toast({
          title: 'Success',
          description: 'Tax group updated successfully',
        });
        router.push('/administration/tax-configuration/tax-groups');
      }
    } catch (error: any) {
      console.error('Error updating tax group:', error);
      toast({
        title: 'Error',
        description: error.message || 'Failed to update tax group',
        variant: 'destructive'
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    router.push('/administration/tax-configuration/tax-groups');
  };

  const filteredTaxes = availableTaxes.filter(tax =>
    tax.TaxName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    tax.TaxUID.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getActionBadge = (taxUID: string) => {
    const tax = selectedTaxes.find(t => t.TaxUID === taxUID);
    if (!tax) return null;

    switch (tax.ActionType) {
      case 1: return <Badge variant="default" className="ml-2">New</Badge>;
      case 2: return <Badge variant="destructive" className="ml-2">Remove</Badge>;
      default: return null;
    }
  };

  if (loadingData) {
    return (
      <div className="container mx-auto p-6 max-w-4xl">
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin mr-2" />
          <span>Loading tax group data...</span>
        </div>
      </div>
    );
  }

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
        <h1 className="text-3xl font-bold">Edit Tax Group</h1>
        <p className="text-gray-600 mt-2">
          Modify the tax group configuration: {formData.Name}
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="space-y-6">
          {/* Basic Information */}
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>
                Modify the fundamental details of the tax group
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="code">Tax Group Code *</Label>
                  <Input
                    id="code"
                    value={formData.Code}
                    onChange={(e) => handleInputChange('Code', e.target.value)}
                    placeholder="Enter tax group code"
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">Short code for easy reference</p>
                </div>
                <div>
                  <Label htmlFor="name">Tax Group Name *</Label>
                  <Input
                    id="name"
                    value={formData.Name}
                    onChange={(e) => handleInputChange('Name', e.target.value)}
                    placeholder="Enter tax group name"
                    required
                  />
                  <p className="text-xs text-gray-500 mt-1">Display name for this tax group</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Tax Selection */}
          <Card>
            <CardHeader>
              <CardTitle>Tax Selection</CardTitle>
              <CardDescription>
                Select which taxes should be included in this tax group
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Search Bar */}
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

              {/* Tax Selection Table */}
              <div className="rounded-md border max-h-96 overflow-y-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="w-12">Select</TableHead>
                      <TableHead>Tax Name</TableHead>
                      <TableHead>Tax UID</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {filteredTaxes.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={4} className="text-center py-8">
                          {searchTerm ? 'No taxes found matching your search' : 'No taxes available'}
                        </TableCell>
                      </TableRow>
                    ) : (
                      filteredTaxes.map((tax) => (
                        <TableRow key={tax.TaxUID}>
                          <TableCell>
                            <Checkbox
                              checked={isTaxSelected(tax.TaxUID)}
                              onCheckedChange={() => handleTaxToggle(tax.TaxUID, tax.TaxName)}
                            />
                          </TableCell>
                          <TableCell className="font-medium">
                            <div className="flex items-center">
                              {tax.TaxName}
                              {getActionBadge(tax.TaxUID)}
                            </div>
                          </TableCell>
                          <TableCell className="font-mono text-sm">
                            {tax.TaxUID}
                          </TableCell>
                          <TableCell>
                            <Badge variant={isTaxSelected(tax.TaxUID) ? "default" : "secondary"}>
                              {isTaxSelected(tax.TaxUID) ? "Selected" : "Available"}
                            </Badge>
                          </TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </div>

              <div className="text-sm text-gray-600">
                <strong>{selectedTaxes.filter(t => t.ActionType !== 2).length}</strong> taxes selected
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
                  Updating Tax Group...
                </>
              ) : (
                <>
                  <Save className="h-4 w-4 mr-2" />
                  Update Tax Group
                </>
              )}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default EditTaxGroupPage;