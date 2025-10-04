'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, Save, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';
import { storeService } from '@/services/storeService';
import { IStoreMaster, STORE_TYPES } from '@/types/store.types';

export default function EditStorePage() {
  const router = useRouter();
  const params = useParams();
  const storeId = params.id as string;

  const [store, setStore] = useState<IStoreMaster | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    code: '',
    type: '',
    legalName: '',
    franchiseeOrgUID: '',
    isActive: true,
    isBlocked: false,
    email: '',
    mobile: ''
  });

  useEffect(() => {
    if (storeId) {
      fetchStoreDetails();
    }
  }, [storeId]);

  const fetchStoreDetails = async () => {
    setLoading(true);
    try {
      console.log('✏️ Fetching store details for edit, ID:', storeId);
      
      // Use working fallback method directly (master API has SQL bug)
      console.log('🔄 Fetching store data for editing using working API...');
      let storeDetails;
      
      try {
        // Get basic store data using our working workaround
        const basicStore = await storeService.getStoreByUID(storeId);
        console.log('✅ Successfully retrieved store data for editing');
        
        // Create a mock master structure
        storeDetails = {
          store: basicStore,
          contacts: [],
          addresses: [],
          storeCredits: [],
          storeAdditionalInfo: null
        };
      } catch (error) {
        console.error('❌ Failed to fetch store for editing:', error);
        throw new Error(`Failed to fetch store ${storeId}: ${error.message}`);
      }
      
      setStore(storeDetails);
      
      // Populate form data
      const storeInfo = storeDetails.store;
      setFormData({
        name: storeInfo.Name || storeInfo.name || '',
        code: storeInfo.Code || storeInfo.code || '',
        type: storeInfo.Type || storeInfo.type || '',
        legalName: storeInfo.LegalName || storeInfo.legal_name || '',
        franchiseeOrgUID: storeInfo.FranchiseeOrgUID || storeInfo.franchisee_org_uid || '',
        isActive: storeInfo.IsActive ?? storeInfo.is_active ?? true,
        isBlocked: storeInfo.IsBlocked ?? storeInfo.is_blocked ?? false,
        email: storeInfo.Email || storeInfo.email || '',
        mobile: storeInfo.Mobile || storeInfo.mobile || ''
      });
    } catch (error) {
      console.error('❌ Error fetching store details:', error);
      toast.error('Failed to fetch store details');
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field: string, value: string | boolean) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (!store) return;

      const updatedStore = {
        ...store.store,
        Name: formData.name,
        Code: formData.code,
        Type: formData.type,
        LegalName: formData.legalName,
        FranchiseeOrgUID: formData.franchiseeOrgUID,
        IsActive: formData.isActive,
        IsBlocked: formData.isBlocked,
        Email: formData.email,
        Mobile: formData.mobile
      };

      await storeService.updateStore(updatedStore);
      toast.success('Store updated successfully');
      router.push(`/administration/store-management/stores/view/${storeId}`);
    } catch (error) {
      console.error('Error updating store:', error);
      toast.error('Failed to update store');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="container mx-auto p-6 space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <Skeleton className="h-8 w-48" />
        </div>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-32" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!store) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center py-8">
          <h2 className="text-2xl font-bold mb-2">Store Not Found</h2>
          <p className="text-muted-foreground mb-4">The requested store could not be found.</p>
          <Button onClick={() => router.push('/administration/store-management/stores')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Store Management
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.push(`/administration/store-management/stores/view/${storeId}`)}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Edit Store</h1>
            <p className="text-muted-foreground">Update store information</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => router.push(`/administration/store-management/stores/view/${storeId}`)}
          >
            <X className="h-4 w-4 mr-2" />
            Cancel
          </Button>
          <Button onClick={handleSave} disabled={saving}>
            <Save className="h-4 w-4 mr-2" />
            {saving ? 'Saving...' : 'Save Changes'}
          </Button>
        </div>
      </div>

      {/* Form */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle>Basic Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="code">Store Code *</Label>
              <Input
                id="code"
                value={formData.code}
                onChange={(e) => handleInputChange('code', e.target.value)}
                placeholder="Enter store code"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="name">Store Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => handleInputChange('name', e.target.value)}
                placeholder="Enter store name"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="legalName">Legal Name</Label>
              <Input
                id="legalName"
                value={formData.legalName}
                onChange={(e) => handleInputChange('legalName', e.target.value)}
                placeholder="Enter legal name"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="type">Store Type</Label>
              <Select value={formData.type} onValueChange={(value) => handleInputChange('type', value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select store type" />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(STORE_TYPES).map(([key, value]) => (
                    <SelectItem key={key} value={key}>
                      {value}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="franchiseeOrgUID">Organization</Label>
              <Select 
                value={formData.franchiseeOrgUID} 
                onValueChange={(value) => handleInputChange('franchiseeOrgUID', value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select organization" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Farmley">Farmley</SelectItem>
                  <SelectItem value="7Eleven">7Eleven</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardContent>
        </Card>

        {/* Contact & Status */}
        <Card>
          <CardHeader>
            <CardTitle>Contact & Status</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={formData.email}
                onChange={(e) => handleInputChange('email', e.target.value)}
                placeholder="Enter email address"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="mobile">Mobile</Label>
              <Input
                id="mobile"
                value={formData.mobile}
                onChange={(e) => handleInputChange('mobile', e.target.value)}
                placeholder="Enter mobile number"
              />
            </div>

            <div className="space-y-4 pt-4">
              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <Label htmlFor="isActive">Active Status</Label>
                  <p className="text-sm text-muted-foreground">
                    Enable or disable this store
                  </p>
                </div>
                <Switch
                  id="isActive"
                  checked={formData.isActive}
                  onCheckedChange={(checked) => handleInputChange('isActive', checked)}
                />
              </div>

              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <Label htmlFor="isBlocked">Blocked Status</Label>
                  <p className="text-sm text-muted-foreground">
                    Block or unblock this store
                  </p>
                </div>
                <Switch
                  id="isBlocked"
                  checked={formData.isBlocked}
                  onCheckedChange={(checked) => handleInputChange('isBlocked', checked)}
                />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Save Actions */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex justify-end gap-2">
            <Button
              variant="outline"
              onClick={() => router.push(`/administration/store-management/stores/view/${storeId}`)}
            >
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={saving}>
              <Save className="h-4 w-4 mr-2" />
              {saving ? 'Saving...' : 'Save Changes'}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}