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
        console.log('✅ Successfully retrieved complete store master data');
        console.log(`📈 Store Master contains: ${storeDetails.contacts.length} contacts, ${storeDetails.addresses.length} addresses`);
        setStore(storeDetails);
        return;
      } catch (masterError) {
        console.warn('⚠️ StoreMaster API failed, using fallback approach:', masterError.message);
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
    router.push(`/updatedfeatures/store-management/stores/edit/${storeId}`);
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this store? This action cannot be undone.')) {
      try {
        await storeService.deleteStore(storeId);
        toast.success('Store deleted successfully');
        router.push('/updatedfeatures/store-management/stores/manage');
      } catch (error) {
        console.error('Error deleting store:', error);
        toast.error('Failed to delete store');
      }
    }
  };

  const getStatusBadgeVariant = (isActive: boolean, isBlocked: boolean) => {
    if (isBlocked) return 'destructive';
    if (isActive) return 'default';
    return 'secondary';
  };

  const getStatusText = (isActive: boolean, isBlocked: boolean) => {
    if (isBlocked) return 'Blocked';
    if (isActive) return 'Active';
    return 'Inactive';
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
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
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
          <Button onClick={() => router.push('/updatedfeatures/store-management/stores/manage')}>
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
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.push('/updatedfeatures/store-management/stores/manage')}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <div>
            <h1 className="text-3xl font-bold">{storeInfo.Name || storeInfo.name}</h1>
            <div className="flex items-center gap-4 mt-1">
              <p className="text-muted-foreground flex items-center gap-1">
                <Hash className="h-4 w-4" />
                {storeInfo.Code || storeInfo.code || storeInfo.StoreNumber || storeInfo.UID}
              </p>
              <Badge 
                variant={getStatusBadgeVariant(
                  storeInfo.IsActive ?? storeInfo.is_active,
                  storeInfo.IsBlocked ?? storeInfo.is_blocked
                )}
                className="flex items-center gap-1"
              >
                {(storeInfo.IsActive ?? storeInfo.is_active) ? 
                  <CheckCircle className="h-3 w-3" /> : 
                  (storeInfo.IsBlocked ?? storeInfo.is_blocked) ? 
                  <XCircle className="h-3 w-3" /> : 
                  <AlertCircle className="h-3 w-3" />
                }
                {getStatusText(
                  storeInfo.IsActive ?? storeInfo.is_active,
                  storeInfo.IsBlocked ?? storeInfo.is_blocked
                )}
              </Badge>
            </div>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleEdit}>
            <Edit className="h-4 w-4 mr-2" />
            Edit
          </Button>
          <Button variant="destructive" onClick={handleDelete}>
            <Trash2 className="h-4 w-4 mr-2" />
            Delete
          </Button>
        </div>
      </div>

      {/* Key Metrics Overview */}
      <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-primary/10 rounded-lg">
              <Store className="h-5 w-5 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Store Type</p>
              <p className="font-semibold">
                {STORE_TYPES[storeInfo.Type || storeInfo.type as keyof typeof STORE_TYPES] || storeInfo.Type || storeInfo.type}
              </p>
            </div>
          </div>
        </Card>
        
        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-green-100 rounded-lg">
              <Globe className="h-5 w-5 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Classification</p>
              <p className="font-semibold">{storeInfo.BroadClassification || 'Not Set'}</p>
            </div>
          </div>
        </Card>
        
        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-blue-100 rounded-lg">
              <Building2 className="h-5 w-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Franchisee</p>
              <p className="font-semibold">{storeInfo.FranchiseeOrgUID || 'Not Set'}</p>
            </div>
          </div>
        </Card>
        
        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-purple-100 rounded-lg">
              <Shield className="h-5 w-5 text-purple-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Tax Status</p>
              <p className="font-semibold">
                {(storeInfo.IsTaxApplicable ?? storeInfo.is_tax_applicable) ? 'Applicable' : 'Not Applicable'}
              </p>
            </div>
          </div>
        </Card>
        
        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-orange-100 rounded-lg">
              <DollarSign className="h-5 w-5 text-orange-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Outstanding</p>
              <p className="font-semibold">₹{storeInfo.TotalOutStandings || 0}</p>
            </div>
          </div>
        </Card>
        
        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-100 rounded-lg">
              <Package className="h-5 w-5 text-indigo-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Store Size</p>
              <p className="font-semibold">{storeInfo.StoreSize || 0} sq ft</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Store Details */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Store className="h-5 w-5" />
              Basic Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium text-muted-foreground flex items-center gap-1">
                  <Hash className="h-3 w-3" /> Store Code
                </label>
                <p className="font-medium">{storeInfo.Code || storeInfo.code || storeInfo.StoreNumber || storeInfo.UID}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground flex items-center gap-1">
                  <Building2 className="h-3 w-3" /> Store Name
                </label>
                <p className="font-medium">{storeInfo.Name || storeInfo.name}</p>
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium text-muted-foreground">Type</label>
                <p className="font-medium">
                  {STORE_TYPES[storeInfo.Type || storeInfo.type as keyof typeof STORE_TYPES] || 
                   storeInfo.Type || storeInfo.type}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground">Status</label>
                <Badge 
                  variant={getStatusBadgeVariant(
                    storeInfo.IsActive ?? storeInfo.is_active,
                    storeInfo.IsBlocked ?? storeInfo.is_blocked
                  )}
                >
                  {getStatusText(
                    storeInfo.IsActive ?? storeInfo.is_active,
                    storeInfo.IsBlocked ?? storeInfo.is_blocked
                  )}
                </Badge>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium text-muted-foreground flex items-center gap-1">
                  <Briefcase className="h-3 w-3" /> Classification Type
                </label>
                <p className="font-medium">{storeInfo.ClassficationType || storeInfo.classfication_type || 'Not Set'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground flex items-center gap-1">
                  <Globe className="h-3 w-3" /> Broad Classification
                </label>
                <p className="font-medium">{storeInfo.BroadClassification || storeInfo.broad_classification || 'Not Set'}</p>
              </div>
            </div>
            
            <div>
              <label className="text-sm font-medium text-muted-foreground">Organization/Franchisee</label>
              <p className="font-medium">{storeInfo.FranchiseeOrgUID || storeInfo.franchisee_org_uid || 'Not Set'}</p>
            </div>

            {(storeInfo.LegalName || storeInfo.legal_name) && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Legal Name</label>
                <p className="font-medium">{storeInfo.LegalName || storeInfo.legal_name}</p>
              </div>
            )}
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium text-muted-foreground">Store Size</label>
                <p className="font-medium">{storeInfo.StoreSize || storeInfo.store_size || '0'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground">Store Class</label>
                <p className="font-medium">{storeInfo.StoreClass || storeInfo.store_class || 'N/A'}</p>
              </div>
            </div>
            
            {(storeInfo.OutletName || storeInfo.outlet_name) && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Outlet Name</label>
                <p className="font-medium">{storeInfo.OutletName || storeInfo.outlet_name}</p>
              </div>
            )}
            
            {(storeInfo.ArabicName || storeInfo.arabic_name) && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Arabic Name</label>
                <p className="font-medium">{storeInfo.ArabicName || storeInfo.arabic_name}</p>
              </div>
            )}
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium text-muted-foreground">Tax Applicable</label>
                <Badge variant={(storeInfo.IsTaxApplicable ?? storeInfo.is_tax_applicable) ? 'default' : 'secondary'}>
                  {(storeInfo.IsTaxApplicable ?? storeInfo.is_tax_applicable) ? 'Yes' : 'No'}
                </Badge>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground">Tax Verified</label>
                <Badge variant={(storeInfo.IsTaxDocVerified ?? storeInfo.is_tax_doc_verified) ? 'default' : 'secondary'}>
                  {(storeInfo.IsTaxDocVerified ?? storeInfo.is_tax_doc_verified) ? 'Yes' : 'No'}
                </Badge>
              </div>
            </div>
            
            {(storeInfo.TaxDocNumber || storeInfo.tax_doc_number) && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Tax Document Number</label>
                <p className="font-medium">{storeInfo.TaxDocNumber || storeInfo.tax_doc_number}</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Contact Information */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              Contact Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {(() => {
              // Check if we have any contacts with actual data
              const contactsWithData = store.contacts?.filter(contact => {
                const hasName = contact.Name || contact.name;
                const hasPhone = contact.Mobile || contact.mobile || contact.Phone || contact.phone;
                const hasEmail = contact.Email || contact.email;
                return hasName || hasPhone || hasEmail;
              }) || [];
              
              // If we have valid contacts, show them
              if (contactsWithData.length > 0) {
                return contactsWithData.map((contact, index) => (
                  <div key={index} className="border-b border-gray-100 pb-3 last:border-b-0">
                    <div className="font-semibold text-gray-900">
                      {contact.Title && `${contact.Title} `}{contact.Name || contact.name || 'Contact'}
                    </div>
                    {contact.Designation && (
                      <div className="text-sm text-gray-600 mb-2">
                        {contact.Designation} • {contact.Type || 'Primary'}
                      </div>
                    )}
                    <div className="space-y-1">
                      {(contact.Mobile || contact.Phone) && (
                        <div className="flex items-center gap-2 text-sm">
                          <Phone className="h-4 w-4 text-muted-foreground" />
                          <span>{contact.Mobile || contact.Phone}</span>
                        </div>
                      )}
                      {contact.Email && (
                        <div className="flex items-center gap-2 text-sm">
                          <Mail className="h-4 w-4 text-muted-foreground" />
                          <span>{contact.Email}</span>
                        </div>
                      )}
                    </div>
                  </div>
                ));
              }
              
              // If no valid contacts, show store's basic contact info as fallback
              const storeEmail = store.store.Email || store.store.email;
              const storeMobile = store.store.Mobile || store.store.mobile;
              
              if (storeEmail || storeMobile) {
                return (
                  <div className="border border-blue-200 bg-blue-50 p-3 rounded">
                    <div className="font-semibold text-blue-900 mb-2">
                      Store Contact Information
                    </div>
                    <div className="space-y-1">
                      {storeMobile && (
                        <div className="flex items-center gap-2 text-sm">
                          <Phone className="h-4 w-4 text-blue-600" />
                          <span>{storeMobile}</span>
                        </div>
                      )}
                      {storeEmail && (
                        <div className="flex items-center gap-2 text-sm">
                          <Mail className="h-4 w-4 text-blue-600" />
                          <span>{storeEmail}</span>
                        </div>
                      )}
                    </div>
                    <div className="text-xs text-blue-600 mt-2">
                      ℹ️ Using store's basic contact information
                    </div>
                  </div>
                );
              }
              
              // If no contact info at all
              return (
                <div className="text-center py-6">
                  <Phone className="h-8 w-8 text-gray-300 mx-auto mb-2" />
                  <p className="text-gray-500 font-medium">No Contact Information</p>
                  <p className="text-xs text-gray-400 mt-1">
                    No contact details found for this store
                  </p>
                </div>
              );
            })()}
          </CardContent>
        </Card>
      </div>

      {/* Address Information */}
      {store.addresses && store.addresses.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MapPin className="h-5 w-5" />
              Address Information
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {store.addresses.map((address, index) => {
                // Use exact field names from backend Address model
                const addressType = address.Type || address.type || 'Store';
                const addressName = address.Name || address.name;
                const line1 = address.Line1 || address.line1;
                const line2 = address.Line2 || address.line2;
                const line3 = address.Line3 || address.line3;
                const landmark = address.Landmark || address.landmark;
                const area = address.Area || address.area;
                const phone = address.Phone || address.phone;
                const mobile = address.Mobile1 || address.mobile1;
                
                // Parse city data that comes in format like "J & K_City_Jammu"
                const parseCityStateFormat = (cityStr: string) => {
                  if (!cityStr) return { city: '', state: '' };
                  if (cityStr.includes('_City_')) {
                    const parts = cityStr.split('_City_');
                    return {
                      state: parts[0]?.replace(/_/g, ' ') || '',
                      city: parts[1]?.replace(/_/g, ' ') || ''
                    };
                  }
                  return { city: cityStr, state: '' };
                };
                
                const cityData = parseCityStateFormat(address.City || address.city || '');
                const finalCity = cityData.city || address.State || address.state;
                const finalState = cityData.state;
                const zipCode = address.ZipCode || address.zip_code;
                
                // Show address only if we have basic info
                if (!line1 && !finalCity && !addressName) return null;
                
                return (
                  <div key={index} className="border border-gray-200 rounded-lg p-4">
                    <div className="flex items-center gap-2 font-semibold text-gray-900 mb-3">
                      <MapPin className="h-4 w-4" />
                      {addressType} Address
                    </div>
                    <div className="space-y-1 text-sm text-gray-700">
                      {addressName && (
                        <p className="font-medium">{addressName}</p>
                      )}
                      {line1 && <p>{line1}</p>}
                      {line2 && <p>{line2}</p>}
                      {line3 && <p>{line3}</p>}
                      {landmark && <p className="text-gray-600">Near: {landmark}</p>}
                      {area && <p>{area}</p>}
                      
                      <div className="pt-1">
                        {finalCity && finalState && (
                          <p className="font-medium">{finalCity}, {finalState}</p>
                        )}
                        {finalCity && !finalState && (
                          <p className="font-medium">{finalCity}</p>
                        )}
                        {zipCode && (
                          <p className="text-gray-600">PIN: {zipCode}</p>
                        )}
                      </div>
                      
                      {(phone || mobile) && (
                        <div className="pt-2 flex items-center gap-4 text-xs text-gray-500">
                          {phone && (
                            <span className="flex items-center gap-1">
                              <Phone className="h-3 w-3" /> {phone}
                            </span>
                          )}
                          {mobile && (
                            <span className="flex items-center gap-1">
                              <Phone className="h-3 w-3" /> {mobile}
                            </span>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                );
              }).filter(Boolean)}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Store Additional Info */}
      {store.StoreAdditionalInfo && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Settings className="h-5 w-5" />
              Business Configuration
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
              {store.StoreAdditionalInfo.OrderType && (
                <div>
                  <label className="text-muted-foreground">Order Type</label>
                  <p className="font-medium">{store.StoreAdditionalInfo.OrderType}</p>
                </div>
              )}
              {store.StoreAdditionalInfo.PaymentMode && (
                <div>
                  <label className="text-muted-foreground">Payment Mode</label>
                  <p className="font-medium">{store.StoreAdditionalInfo.PaymentMode}</p>
                </div>
              )}
              {store.StoreAdditionalInfo.DeliveryMode && (
                <div>
                  <label className="text-muted-foreground">Delivery Mode</label>
                  <p className="font-medium">{store.StoreAdditionalInfo.DeliveryMode}</p>
                </div>
              )}
              {store.StoreAdditionalInfo.InvoiceFrequency && (
                <div>
                  <label className="text-muted-foreground">Invoice Frequency</label>
                  <p className="font-medium">{store.StoreAdditionalInfo.InvoiceFrequency}</p>
                </div>
              )}
              {store.StoreAdditionalInfo.PriceType && (
                <div>
                  <label className="text-muted-foreground">Price Type</label>
                  <p className="font-medium">{store.StoreAdditionalInfo.PriceType}</p>
                </div>
              )}
              {store.StoreAdditionalInfo.VisitFrequency && (
                <div>
                  <label className="text-muted-foreground">Visit Frequency</label>
                  <p className="font-medium">{store.StoreAdditionalInfo.VisitFrequency}</p>
                </div>
              )}
            </div>
            
            <Separator />
            
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
              <div>
                <label className="text-muted-foreground">E-Invoicing</label>
                <Badge variant={store.StoreAdditionalInfo.EInvoicingEnabled ? 'default' : 'secondary'}>
                  {store.StoreAdditionalInfo.EInvoicingEnabled ? 'Enabled' : 'Disabled'}
                </Badge>
              </div>
              <div>
                <label className="text-muted-foreground">Allow Returns</label>
                <Badge variant={store.StoreAdditionalInfo.AllowGoodReturn ? 'default' : 'secondary'}>
                  {store.StoreAdditionalInfo.AllowGoodReturn ? 'Yes' : 'No'}
                </Badge>
              </div>
              <div>
                <label className="text-muted-foreground">Manual Edit</label>
                <Badge variant={store.StoreAdditionalInfo.IsManualEditAllowed ? 'default' : 'secondary'}>
                  {store.StoreAdditionalInfo.IsManualEditAllowed ? 'Allowed' : 'Not Allowed'}
                </Badge>
              </div>
              <div>
                <label className="text-muted-foreground">Asset Enabled</label>
                <Badge variant={store.StoreAdditionalInfo.IsAssetEnabled ? 'default' : 'secondary'}>
                  {store.StoreAdditionalInfo.IsAssetEnabled ? 'Yes' : 'No'}
                </Badge>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Store Documents */}
      {store.StoreDocuments && store.StoreDocuments.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5" />
              Store Documents
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {store.StoreDocuments.map((doc, index) => (
                <div key={index} className="flex items-center justify-between p-3 border rounded-lg">
                  <div className="flex items-center gap-3">
                    <FileText className="h-5 w-5 text-muted-foreground" />
                    <div>
                      <p className="font-medium">{doc.DocumentType || 'Document'}</p>
                      <p className="text-sm text-muted-foreground">
                        {doc.DocumentNumber || 'No number'}
                      </p>
                    </div>
                  </div>
                  {doc.IsVerified && (
                    <Badge variant="default" className="flex items-center gap-1">
                      <CheckCircle className="h-3 w-3" /> Verified
                    </Badge>
                  )}
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
      
      {/* Store Credits */}
      {store.StoreCredits && store.StoreCredits.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <CreditCard className="h-5 w-5" />
              Store Credits
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {store.StoreCredits.map((credit, index) => (
                <div key={index} className="flex items-center justify-between p-3 border rounded-lg">
                  <div>
                    <p className="font-medium">Credit Limit</p>
                    <p className="text-sm text-muted-foreground">
                      ₹{credit.CreditLimit || 0}
                    </p>
                  </div>
                  <div>
                    <p className="font-medium">Available</p>
                    <p className="text-sm text-muted-foreground">
                      ₹{credit.AvailableCredit || 0}
                    </p>
                  </div>
                  <div>
                    <p className="font-medium">Days</p>
                    <p className="text-sm text-muted-foreground">
                      {credit.CreditDays || 0}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Additional Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Info className="h-5 w-5" />
            System Information
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <label className="text-muted-foreground">Created</label>
              <p className="flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                {storeInfo.CreatedTime ? new Date(storeInfo.CreatedTime).toLocaleDateString() : 'N/A'}
              </p>
              <p className="text-xs text-muted-foreground">By: {storeInfo.CreatedBy || storeInfo.created_by || 'N/A'}</p>
            </div>
            <div>
              <label className="text-muted-foreground">Last Modified</label>
              <p className="flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                {storeInfo.ModifiedTime ? new Date(storeInfo.ModifiedTime).toLocaleDateString() : 'N/A'}
              </p>
              <p className="text-xs text-muted-foreground">By: {storeInfo.ModifiedBy || storeInfo.modified_by || 'N/A'}</p>
            </div>
          </div>
          
          <Separator />
          
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <label className="text-muted-foreground flex items-center gap-1">
                <User className="h-3 w-3" /> Created By
              </label>
              <p className="font-medium">{storeInfo.CreatedBy || 'System'}</p>
            </div>
            <div>
              <label className="text-muted-foreground flex items-center gap-1">
                <User className="h-3 w-3" /> Modified By
              </label>
              <p className="font-medium">{storeInfo.ModifiedBy || 'System'}</p>
            </div>
            <div>
              <label className="text-muted-foreground">Store Rating</label>
              <p className="font-medium">{storeInfo.StoreRating || storeInfo.store_rating || 'Not Rated'}</p>
            </div>
            <div>
              <label className="text-muted-foreground">Source</label>
              <p className="font-medium">{storeInfo.Source || storeInfo.source || 'Manual'}</p>
            </div>
          </div>
          
          <Separator />
          
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
            <div>
              <label className="text-muted-foreground">Internal UID</label>
              <p className="font-mono text-xs">{storeInfo.UID}</p>
            </div>
            <div>
              <label className="text-muted-foreground">Store Number</label>
              <p className="font-medium">{storeInfo.Number || storeInfo.StoreNumber || 'N/A'}</p>
            </div>
            <div>
              <label className="text-muted-foreground">Company UID</label>
              <p className="font-mono text-xs">{storeInfo.CompanyUID || 'N/A'}</p>
            </div>
          </div>

          {storeInfo.BlockedReasonDescription && (
            <div>
              <label className="text-muted-foreground">Blocked Reason</label>
              <p className="text-red-600 font-medium">{storeInfo.BlockedReasonDescription}</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}