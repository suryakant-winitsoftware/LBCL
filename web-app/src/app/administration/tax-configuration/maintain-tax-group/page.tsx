'use client';

import React, { useState, useEffect } from 'react';
import { taxService, ITaxSelectionItem, ITaxGroup } from '@/services/tax/tax.service';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/components/ui/use-toast';
import { RefreshCw, Save, Settings } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';

interface TaxChange {
  taxUID: string;
  action: 'Add' | 'Remove';
  isSelected: boolean;
}

const MaintainTaxGroupPage = () => {
  const { toast } = useToast();
  const [taxSelectionItems, setTaxSelectionItems] = useState<ITaxSelectionItem[]>([]);
  const [taxGroups, setTaxGroups] = useState<ITaxGroup[]>([]);
  const [selectedTaxGroupUID, setSelectedTaxGroupUID] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [changes, setChanges] = useState<Map<string, TaxChange>>(new Map());

  useEffect(() => {
    loadTaxGroups();
  }, []);

  useEffect(() => {
    if (selectedTaxGroupUID) {
      loadTaxSelectionItems(selectedTaxGroupUID);
    } else {
      setTaxSelectionItems([]);
      setChanges(new Map());
    }
  }, [selectedTaxGroupUID]);

  const loadTaxGroups = async () => {
    try {
      const response = await taxService.getTaxGroupDetails({
        pageNumber: 1,
        pageSize: 100,
        isCountRequired: false,
        filterCriterias: [],
        sortCriterias: [
          {
            sortParameter: 'Name',
            direction: 'Asc',
          },
        ],
      });
      
      if (response && response.PagedData) {
        setTaxGroups(response.PagedData);
        if (response.PagedData.length > 0) {
          setSelectedTaxGroupUID(response.PagedData[0].UID);
        }
      }
    } catch (error) {
      console.error('Error loading tax groups:', error);
      toast({
        title: 'Error',
        description: 'Failed to load tax groups',
        variant: 'destructive',
      });
    }
  };

  const loadTaxSelectionItems = async (taxGroupUID: string) => {
    setLoading(true);
    try {
      const items = await taxService.getTaxSelectionItems(taxGroupUID);
      setTaxSelectionItems(items);
      setChanges(new Map());
      
      toast({
        title: 'Loaded',
        description: `Loaded ${items.length} tax items for selection`,
      });
    } catch (error) {
      console.error('Error loading tax selection items:', error);
      toast({
        title: 'Error',
        description: 'Failed to load tax selection items',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSelectionChange = (taxUID: string, isSelected: boolean) => {
    // Update the UI immediately
    setTaxSelectionItems((prev) =>
      prev.map((item) =>
        item.TaxUID === taxUID ? { ...item, IsSelected: isSelected } : item
      )
    );

    // Track the change
    const newChanges = new Map(changes);
    const existingItem = taxSelectionItems.find(item => item.TaxUID === taxUID);
    const wasOriginallySelected = existingItem ? existingItem.IsSelected : false;

    if (isSelected === wasOriginallySelected) {
      // Reverted to original state, remove from changes
      newChanges.delete(taxUID);
    } else {
      // Add or update change
      newChanges.set(taxUID, {
        taxUID,
        action: isSelected ? 'Add' : 'Remove',
        isSelected,
      });
    }
    
    setChanges(newChanges);
  };

  const handleSave = async () => {
    if (changes.size === 0) {
      toast({
        title: 'No Changes',
        description: 'No modifications have been made',
      });
      return;
    }

    if (!selectedTaxGroupUID) {
      toast({
        title: 'Error',
        description: 'Please select a tax group',
        variant: 'destructive',
      });
      return;
    }

    setSaving(true);
    try {
      // Get the selected tax group details
      const selectedGroup = taxGroups.find(g => g.UID === selectedTaxGroupUID);
      if (!selectedGroup) {
        throw new Error('Selected tax group not found');
      }

      // Prepare the tax group taxes list with actions
      const taxGroupTaxes = Array.from(changes.values()).map((change, index) => ({
        UID: `TGT-${Date.now()}-${index}`, // Generate temp UID
        TaxGroupUID: selectedTaxGroupUID,
        TaxUID: change.taxUID,
        ActionType: change.action === 'Add' ? 0 : 2, // 0 = Add, 2 = Delete
        CreatedBy: 'ADMIN',
        CreatedTime: new Date().toISOString(),
        ModifiedBy: 'ADMIN',
        ModifiedTime: new Date().toISOString(),
        ServerAddTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
      }));

      // Prepare the update payload
      const updatePayload = {
        TaxGroup: {
          ...selectedGroup,
          ModifiedBy: 'ADMIN',
          ModifiedTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
        },
        TaxGroupTaxes: taxGroupTaxes,
      };

      // Call the update API
      await taxService.updateTaxGroupMaster(updatePayload);
      
      toast({
        title: 'Success',
        description: `Successfully updated ${changes.size} tax assignments`,
      });

      // Reload the data to reflect changes
      await loadTaxSelectionItems(selectedTaxGroupUID);
      
    } catch (error) {
      console.error('Error saving tax selections:', error);
      toast({
        title: 'Error',
        description: 'Failed to save tax selections. Please try again.',
        variant: 'destructive',
      });
    } finally {
      setSaving(false);
    }
  };

  const handleRefresh = () => {
    if (selectedTaxGroupUID) {
      loadTaxSelectionItems(selectedTaxGroupUID);
    }
  };

  const getSelectedCount = () => taxSelectionItems.filter(item => item.IsSelected).length;
  const getUnselectedCount = () => taxSelectionItems.filter(item => !item.IsSelected).length;

  return (
    <div className="container mx-auto p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold flex items-center gap-2">
          <Settings className="h-8 w-8" />
          Tax Group Management
        </h1>
        <p className="text-gray-600 mt-2">
          Select which taxes belong to each tax group
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Tax Group Management
              </CardTitle>
              <p className="text-sm text-gray-600 mt-1">
                Select which taxes belong to each tax group
              </p>
            </div>
            <div className="flex gap-2">
              <Button
                onClick={handleRefresh}
                variant="outline"
                size="icon"
                disabled={loading || !selectedTaxGroupUID}
                title="Refresh"
              >
                <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
              </Button>
              <Button
                onClick={handleSave}
                disabled={changes.size === 0 || saving}
                className="flex items-center gap-2"
              >
                <Save className="h-4 w-4" />
                {saving ? 'Saving...' : `Save Changes${changes.size > 0 ? ` (${changes.size})` : ''}`}
              </Button>
            </div>
          </div>
        </CardHeader>

        <CardContent>
          {/* Tax Group Selector */}
          <div className="mb-6">
            <Label htmlFor="taxGroup" className="text-base font-medium">
              Select Tax Group to Manage
            </Label>
            <Select value={selectedTaxGroupUID} onValueChange={setSelectedTaxGroupUID}>
              <SelectTrigger id="taxGroup" className="w-full mt-2">
                <SelectValue placeholder="Choose a tax group to configure" />
              </SelectTrigger>
              <SelectContent>
                {taxGroups.map((group) => (
                  <SelectItem key={group.UID} value={group.UID}>
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{group.Name}</span>
                      <span className="text-sm text-gray-500">({group.Code})</span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {taxGroups.length === 0 && (
              <p className="text-sm text-orange-600 mt-1">
                No tax groups found. Please create a tax group first.
              </p>
            )}
          </div>

          {/* Tax Selection Items */}
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <RefreshCw className="h-6 w-6 animate-spin mr-2" />
              <span>Loading tax items...</span>
            </div>
          ) : !selectedTaxGroupUID ? (
            <Card>
              <CardContent className="text-center py-12">
                <Settings className="h-12 w-12 mx-auto mb-4 text-gray-400" />
                <p className="text-gray-600 mb-2">Select a tax group to manage its taxes</p>
                <p className="text-sm text-gray-500">
                  You can add or remove taxes from the selected group
                </p>
              </CardContent>
            </Card>
          ) : taxSelectionItems.length === 0 ? (
            <Card>
              <CardContent className="text-center py-12">
                <p className="text-gray-600 mb-2">No taxes available</p>
                <p className="text-sm text-gray-500">
                  Please create some taxes first before configuring tax groups
                </p>
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-medium">Available Taxes</h3>
                <div className="flex gap-2">
                  <Badge variant="outline">
                    Total: {taxSelectionItems.length}
                  </Badge>
                  <Badge variant="default">
                    Selected: {getSelectedCount()}
                  </Badge>
                </div>
              </div>

              {taxSelectionItems.map((item) => {
                const hasChanges = changes.has(item.TaxUID);
                const change = changes.get(item.TaxUID);
                
                return (
                  <Card
                    key={item.TaxUID}
                    className={`transition-all ${
                      hasChanges
                        ? 'border-blue-500 bg-blue-50/50 shadow-md'
                        : 'hover:shadow-sm'
                    }`}
                  >
                    <CardContent className="flex items-center justify-between py-4">
                      <div className="flex items-center gap-3">
                        <Checkbox
                          id={item.TaxUID}
                          checked={item.IsSelected}
                          onCheckedChange={(checked) =>
                            handleSelectionChange(item.TaxUID, checked as boolean)
                          }
                        />
                        <Label
                          htmlFor={item.TaxUID}
                          className="cursor-pointer flex-1"
                        >
                          <div>
                            <p className="font-medium text-gray-900">
                              {item.TaxName || 'Unnamed Tax'}
                            </p>
                            <p className="text-sm text-gray-500">
                              UID: {item.TaxUID}
                            </p>
                          </div>
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        {hasChanges && (
                          <Badge 
                            variant={change?.action === 'Add' ? 'default' : 'destructive'}
                            className="text-xs"
                          >
                            {change?.action === 'Add' ? '+ Adding' : '- Removing'}
                          </Badge>
                        )}
                        {item.IsSelected && !hasChanges && (
                          <Badge variant="outline" className="text-xs">
                            Assigned
                          </Badge>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}

          {/* Summary Statistics */}
          {taxSelectionItems.length > 0 && (
            <Card className="mt-6">
              <CardContent className="py-6">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div className="text-center">
                    <p className="text-2xl font-bold text-gray-900">{taxSelectionItems.length}</p>
                    <p className="text-sm text-gray-600">Total Taxes</p>
                  </div>
                  <div className="text-center">
                    <p className="text-2xl font-bold text-green-600">{getSelectedCount()}</p>
                    <p className="text-sm text-gray-600">Assigned</p>
                  </div>
                  <div className="text-center">
                    <p className="text-2xl font-bold text-gray-500">{getUnselectedCount()}</p>
                    <p className="text-sm text-gray-600">Available</p>
                  </div>
                  <div className="text-center">
                    <p className="text-2xl font-bold text-blue-600">{changes.size}</p>
                    <p className="text-sm text-gray-600">Pending Changes</p>
                  </div>
                </div>

                {changes.size > 0 && (
                  <div className="mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
                    <p className="text-sm font-medium text-blue-900 mb-2">Pending Changes:</p>
                    <div className="space-y-1">
                      {Array.from(changes.values()).map((change) => {
                        const taxName = taxSelectionItems.find(item => item.TaxUID === change.taxUID)?.TaxName || 'Unknown Tax';
                        return (
                          <div key={change.taxUID} className="text-sm text-blue-800 flex items-center gap-2">
                            <Badge 
                              variant={change.action === 'Add' ? 'default' : 'destructive'} 
                              className="text-xs"
                            >
                              {change.action}
                            </Badge>
                            <span>{taxName}</span>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default MaintainTaxGroupPage;