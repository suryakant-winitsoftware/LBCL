'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { 
  ArrowLeft, Edit, Trash2, MapPin, Phone, Mail, Calendar, 
  Building2, Store, Users, CreditCard, FileText, Settings,
  CheckCircle, XCircle, AlertCircle, Info, Hash, Globe,
  Briefcase, Shield, DollarSign, Package, Clock, User
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Separator } from '@/components/ui/separator';
import { toast } from 'sonner';
import { storeService } from '@/services/storeService';
import { IStoreMaster, STORE_TYPES } from '@/types/store.types';
import { formatDateToDayMonthYear } from '@/utils/date-formatter';

export default function ViewStorePage() {
  const router = useRouter();
  const params = useParams();
  const storeId = params.id as string;

  const [store, setStore] = useState<IStoreMaster | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (storeId) {
      fetchStoreDetails();
    }
  }, [storeId]);

  const fetchStoreDetails = async () => {
    setLoading(true);
    try {
      console.log('🔍 Fetching store details for ID:', storeId);
      
      // Try the dedicated StoreMaster API first - gets everything in one call
      console.log('🏠 Trying dedicated StoreMaster API...');
      try {
        const storeDetails = await storeService.getStoreMasterByUID(storeId);
        console.log('✅ Successfully retrieved complete store master data:', storeDetails);
        
        // Validate that we have the store object
        if (!storeDetails.store) {
          console.error('❌ Store object missing from response');
          throw new Error('Invalid store data structure');
        }
        
        console.log(`📈 Store Master contains: ${storeDetails.contacts?.length || 0} contacts, ${storeDetails.addresses?.length || 0} addresses`);
        setStore(storeDetails);
        return;
      } catch (masterError) {
        console.warn('⚠️ StoreMaster API failed, using fallback approach:', masterError);
      }
      
      // Fallback: Get basic store data + individual contact/address calls
      console.log('🔄 Fallback: Fetching store data with individual API calls...');
      const basicStore = await storeService.getStoreByUID(storeId);
      console.log('✅ Successfully retrieved basic store data');
      
      // Fetch real contact and address data concurrently
      console.log('📞 Fetching real contact data...');
      console.log('📍 Fetching real address data...');
      const [contacts, addresses] = await Promise.all([
        storeService.getStoreContacts(storeId).catch(error => {
          console.warn('Contact fetch failed:', error);
          return [];
        }),
        storeService.getStoreAddresses(storeId).catch(error => {
          console.warn('Address fetch failed:', error);
          return [];
        })
      ]);
      
      console.log(`✅ Fallback: Fetched ${contacts.length} contacts and ${addresses.length} addresses`);
      
      // Add fallback contact from store basic data if no separate contact records
      let finalContacts = [...contacts];
      if (finalContacts.length === 0) {
        const storeEmail = basicStore.Email || basicStore.email;
        const storeMobile = basicStore.Mobile || basicStore.mobile;
        if (storeEmail || storeMobile) {
          finalContacts.push({
            Name: 'Store Contact',
            Type: 'Primary',
            Email: storeEmail,
            Mobile: storeMobile
          });
        }
      }
      
      const storeDetails = {
        store: basicStore,
        contacts: finalContacts,
        addresses: addresses,
        storeCredits: [],
        storeAdditionalInfo: null
      };
      
      setStore(storeDetails);
    } catch (error) {
      console.error('❌ Error fetching store details:', error);
      toast.error('Failed to fetch store details');
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = () => {
    router.push(`/administration/store-management/stores/edit/${storeId}`);
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this store? This action cannot be undone.')) {
      try {
        await storeService.deleteStore(storeId);
        toast.success('Store deleted successfully');
        router.push('/administration/store-management/stores');
      } catch (error) {
        toast.error('Failed to delete store');
      }
    }
  };

  const getStatusVariant = (isActive?: boolean, isBlocked?: boolean) => {
    if (isBlocked) return 'destructive';
    if (isActive) return 'default';
    return 'secondary';
  };

  const getStatusText = (isActive?: boolean, isBlocked?: boolean) => {
    if (isBlocked) return 'Blocked';
    if (isActive) return 'Active';
    return 'Inactive';
  };

  const getStatusBadgeVariant = (isActive?: boolean, isBlocked?: boolean) => {
    if (isBlocked) return 'destructive';
    if (isActive) return 'success';
    return 'secondary';
  };

  if (loading) {
    return (
      <div className="container mx-auto p-6 space-y-6">
        <Skeleton className="h-10 w-32" />
        <div className="space-y-4">
          <Skeleton className="h-32 w-full" />
          <div className="grid grid-cols-2 gap-4">
            <Skeleton className="h-48 w-full" />
            <Skeleton className="h-48 w-full" />
          </div>
          <Skeleton className="h-64 w-full" />
        </div>
      </div>
    );
  }

  if (!store || !store.store) {
    console.error('Store data not available:', { store, hasStore: !!store, hasStoreInfo: !!store?.store });
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

  const storeInfo = store.store;

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.push('/administration/store-management/stores')}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Stores
        </Button>
      </div>

      {/* Store Basic Information - Only Show Available Data */}
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">{storeInfo.Name || storeInfo.name}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            {storeInfo.UID && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Store Code</p>
                <p className="font-semibold">{storeInfo.UID}</p>
              </div>
            )}
            {storeInfo.CompanyUID && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Company Code</p>
                <p className="font-semibold">{storeInfo.CompanyUID}</p>
              </div>
            )}
            {/* {storeInfo.Type && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Type</p>
                <p className="font-semibold">{storeInfo.Type}</p>
              </div>
            )} */}
            {storeInfo.CreatedBy && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Created By</p>
                <p className="font-semibold">{storeInfo.CreatedBy}</p>
              </div>
            )}
            {storeInfo.CreatedTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Created Time</p>
                <p className="font-semibold">{formatDateToDayMonthYear(storeInfo.CreatedTime)}</p>
              </div>
            )}
            {storeInfo.ModifiedBy && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Modified By</p>
                <p className="font-semibold">{storeInfo.ModifiedBy}</p>
              </div>
            )}
            {storeInfo.ModifiedTime && (
              <div>
                <p className="text-sm text-muted-foreground mb-1">Modified Time</p>
                <p className="font-semibold">{formatDateToDayMonthYear(storeInfo.ModifiedTime)}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Store Credits - Show All Available Data */}
      {(store.storeCredits || store.StoreCredits) && (store.storeCredits || store.StoreCredits).length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <CreditCard className="h-5 w-5" />
              Store Credits
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {(store.storeCredits || store.StoreCredits).map((credit, index) => (
                <div key={index} className="border rounded-lg p-4 space-y-3">
                  <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                    {credit.StoreUID && (
                      <div>
                        <p className="text-sm text-muted-foreground">Store UID</p>
                        <p className="font-semibold">{credit.StoreUID}</p>
                      </div>
                    )}
                    {credit.CreditType && (
                      <div>
                        <p className="text-sm text-muted-foreground">Credit Type</p>
                        <p className="font-semibold">{credit.CreditType}</p>
                      </div>
                    )}
                    {credit.PaymentTermUID && (
                      <div>
                        <p className="text-sm text-muted-foreground">Payment Term</p>
                        <p className="font-semibold">{credit.PaymentTermUID}</p>
                      </div>
                    )}
                    {credit.PreferredPaymentMode && (
                      <div>
                        <p className="text-sm text-muted-foreground">Payment Mode</p>
                        <p className="font-semibold">{credit.PreferredPaymentMode}</p>
                      </div>
                    )}
                    {credit.OrgUID && (
                      <div>
                        <p className="text-sm text-muted-foreground">Organization</p>
                        <p className="font-semibold">{credit.OrgUID}</p>
                      </div>
                    )}
                    {credit.PriceList && (
                      <div>
                        <p className="text-sm text-muted-foreground">Price List</p>
                        <p className="font-semibold">{credit.PriceList}</p>
                      </div>
                    )}
                    {credit.CreditDays !== undefined && credit.CreditDays !== null && (
                      <div>
                        <p className="text-sm text-muted-foreground">Credit Days</p>
                        <p className="font-semibold">{credit.CreditDays} days</p>
                      </div>
                    )}
                    {credit.IsActive !== undefined && (
                      <div>
                        <p className="text-sm text-muted-foreground">Status</p>
                        <Badge variant={credit.IsActive ? "default" : "secondary"}>
                          {credit.IsActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Addresses - Only Show With Actual Data */}
      {store.addresses && store.addresses.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MapPin className="h-5 w-5" />
              Addresses
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {store.addresses.map((address, index) => {
                // Only show addresses with actual address data
                const hasAddressData = address.Name || address.Line1 || address.Line2 || address.City || address.ZipCode;
                if (!hasAddressData) return null;
                
                return (
                  <div key={index} className="border rounded-lg p-4">
                    <h4 className="font-semibold mb-2">{address.Type || 'Address'}</h4>
                    {address.Name && <p className="text-sm font-medium">{address.Name}</p>}
                    {address.Line1 && <p className="text-sm">{address.Line1}</p>}
                    {address.Line2 && <p className="text-sm">{address.Line2}</p>}
                    {address.City && <p className="text-sm">{address.City}</p>}
                    {address.ZipCode && <p className="text-sm">PIN: {address.ZipCode}</p>}
                  </div>
                );
              }).filter(Boolean)}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Contacts - Only Show With Actual Data */}
      {(() => {
        // Filter contacts with actual data (name, mobile, or email)
        const contactsWithData = store.contacts?.filter(contact => 
          contact.Name || contact.Mobile || contact.Email
        ) || [];
        
        if (contactsWithData.length === 0) return null;
        
        return (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="h-5 w-5" />
                Contacts
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {contactsWithData.map((contact, index) => (
                  <div key={index} className="border rounded-lg p-3">
                    {contact.Name && <p className="font-semibold">{contact.Name}</p>}
                    {contact.Mobile && (
                      <p className="text-sm text-muted-foreground">
                        <Phone className="inline h-3 w-3 mr-1" />
                        {contact.Mobile}
                      </p>
                    )}
                    {contact.Email && (
                      <p className="text-sm text-muted-foreground">
                        <Mail className="inline h-3 w-3 mr-1" />
                        {contact.Email}
                      </p>
                    )}
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        );
      })()}
    </div>
  );
}