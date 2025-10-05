'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import {
  ArrowLeft,
  Edit,
  Loader2,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { toast } from 'sonner';
import { distributorService, IDistributorMasterView } from '@/services/distributor.service';

export default function ViewDistributorPage() {
  const router = useRouter();
  const params = useParams();
  const uid = params.uid as string;

  const [loading, setLoading] = useState(true);
  const [distributorData, setDistributorData] = useState<IDistributorMasterView | null>(null);

  useEffect(() => {
    const fetchDistributor = async () => {
      try {
        setLoading(true);
        const data = await distributorService.getDistributorByUID(uid);
        setDistributorData(data);
      } catch (error) {
        console.error('Error fetching distributor:', error);
        toast.error('Failed to load distributor details');
        router.push('/administration/distributor-management/distributors');
      } finally {
        setLoading(false);
      }
    };

    if (uid) {
      fetchDistributor();
    }
  }, [uid, router]);

  const getStatusBadgeVariant = (status?: string) => {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'default';
      case 'inactive':
        return 'secondary';
      case 'blocked':
        return 'destructive';
      default:
        return 'outline';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!distributorData) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-muted-foreground">Distributor not found</p>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-3xl font-bold tracking-tight">
                {distributorData.Org.Name}
              </h1>
              <Badge variant={getStatusBadgeVariant(distributorData.Org.Status)}>
                {distributorData.Org.Status}
              </Badge>
            </div>
            <p className="text-muted-foreground">
              Distributor Details - {distributorData.Org.Code}
            </p>
          </div>
        </div>
        <Button
          onClick={() => router.push(`/administration/distributor-management/distributors/edit/${uid}`)}
        >
          <Edit className="mr-2 h-4 w-4" />
          Edit Distributor
        </Button>
      </div>

      {/* Content - No Tabs */}
      <div className="space-y-6">
        {/* Basic Information */}
        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Organization Details</CardTitle>
              <CardDescription>Basic information about the distributor</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Code</p>
                  <p className="text-lg">{distributorData.Org.Code}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Name</p>
                  <p className="text-lg">{distributorData.Org.Name}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Status</p>
                  <Badge variant={getStatusBadgeVariant(distributorData.Org.Status)}>
                    {distributorData.Org.Status}
                  </Badge>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Legal Name</p>
                  <p className="text-lg">{distributorData.Store.LegalName || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Alpha Search Code</p>
                  <p className="text-lg">{distributorData.Store.Number || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">VAT Registration Number</p>
                  <p className="text-lg">{distributorData.Store.TaxDocNumber || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Territory</p>
                  <p className="text-lg">{distributorData.Org.TerritoryUid || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Created Date</p>
                  <p className="text-lg">
                    {distributorData.Org.CreatedTime
                      ? new Date(distributorData.Org.CreatedTime).toLocaleDateString()
                      : '-'}
                  </p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Created By</p>
                  <p className="text-lg">{distributorData.Org.CreatedBy || '-'}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Store Information - Hidden for now */}
          {/* <Card>
            <CardHeader>
              <CardTitle>Store Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Store Type</p>
                  <p className="text-lg">{distributorData.Store.Type || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Store Class</p>
                  <p className="text-lg">{distributorData.Store.StoreClass || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Active Status</p>
                  <Badge variant={distributorData.Store.IsActive ? 'default' : 'secondary'}>
                    {distributorData.Store.IsActive ? 'Active' : 'Inactive'}
                  </Badge>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Blocked</p>
                  <Badge variant={distributorData.Store.IsBlocked ? 'destructive' : 'default'}>
                    {distributorData.Store.IsBlocked ? 'Yes' : 'No'}
                  </Badge>
                </div>
              </div>
            </CardContent>
          </Card> */}
        </div>

        {/* Contacts */}
        <div className="space-y-4">
          <h2 className="text-xl font-semibold">Contact Information</h2>
          {!distributorData.Contacts || distributorData.Contacts.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center text-muted-foreground">
                No contacts available
              </CardContent>
            </Card>
          ) : (
            distributorData.Contacts.map((contact, index) => (
              <Card key={index}>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    {contact.Name}
                    {contact.IsDefault && (
                      <Badge variant="outline" className="ml-2">
                        Primary
                      </Badge>
                    )}
                  </CardTitle>
                  {contact.Designation && (
                    <CardDescription>{contact.Designation}</CardDescription>
                  )}
                </CardHeader>
                <CardContent className="space-y-2">
                  <div className="grid grid-cols-2 gap-4">
                    {contact.Mobile && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Mobile</p>
                        <p>{contact.Mobile}</p>
                      </div>
                    )}
                    {contact.Phone && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Phone</p>
                        <p>{contact.Phone}</p>
                      </div>
                    )}
                    {contact.Email && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Email</p>
                        <p>{contact.Email}</p>
                      </div>
                    )}
                    {contact.Fax && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Fax</p>
                        <p>{contact.Fax}</p>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div>

        {/* Address */}
        <div className="space-y-4">
          <h2 className="text-xl font-semibold">Address Information</h2>
          <Card>
            <CardHeader>
              <CardTitle>Address Information</CardTitle>
              <CardDescription>Distributor location details</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {distributorData.Address ? (
                <div className="space-y-2">
                  {distributorData.Address.Line1 && <p>{distributorData.Address.Line1}</p>}
                  {distributorData.Address.Line2 && <p>{distributorData.Address.Line2}</p>}
                  {distributorData.Address.Line3 && <p>{distributorData.Address.Line3}</p>}
                  <p>
                    {[
                      distributorData.Address.City,
                      distributorData.Address.StateCode,
                      distributorData.Address.ZipCode,
                    ]
                      .filter(Boolean)
                      .join(', ')}
                  </p>
                  {distributorData.Address.CountryCode && <p>{distributorData.Address.CountryCode}</p>}

                  <div className="grid grid-cols-2 gap-4 mt-4 pt-4 border-t">
                    {distributorData.Address.Depot && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Warehouse/Depot Code</p>
                        <p>{distributorData.Address.Depot}</p>
                      </div>
                    )}
                    {distributorData.Address.LocationUID && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Destination Location</p>
                        <p>{distributorData.Address.LocationUID}</p>
                      </div>
                    )}
                    {distributorData.Address.Phone && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Phone</p>
                        <p>{distributorData.Address.Phone}</p>
                      </div>
                    )}
                    {distributorData.Address.Email && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Email</p>
                        <p>{distributorData.Address.Email}</p>
                      </div>
                    )}
                  </div>

                  {/* Secondary Location Section */}
                  {(distributorData.Address.CustomField1 ||
                    distributorData.Address.CustomField2 ||
                    distributorData.Address.CustomField3 ||
                    distributorData.Address.CustomField4 ||
                    distributorData.Address.CustomField5 ||
                    distributorData.Address.CustomField6) && (
                    <div className="mt-6 pt-6 border-t">
                      <h4 className="text-sm font-semibold mb-4">Secondary Location</h4>
                      <div className="grid grid-cols-2 gap-4">
                        {distributorData.Address.CustomField1 && (
                          <div>
                            <p className="text-sm font-medium text-muted-foreground">Location Code</p>
                            <p>{distributorData.Address.CustomField1}</p>
                          </div>
                        )}
                        {distributorData.Address.CustomField2 && (
                          <div>
                            <p className="text-sm font-medium text-muted-foreground">Description</p>
                            <p>{distributorData.Address.CustomField2}</p>
                          </div>
                        )}
                        {distributorData.Address.CustomField3 && (
                          <div className="col-span-2">
                            <p className="text-sm font-medium text-muted-foreground">Address Line 1</p>
                            <p>{distributorData.Address.CustomField3}</p>
                          </div>
                        )}
                        {distributorData.Address.CustomField4 && (
                          <div className="col-span-2">
                            <p className="text-sm font-medium text-muted-foreground">Address Line 2</p>
                            <p>{distributorData.Address.CustomField4}</p>
                          </div>
                        )}
                        {distributorData.Address.CustomField5 && (
                          <div className="col-span-2">
                            <p className="text-sm font-medium text-muted-foreground">Address Line 3</p>
                            <p>{distributorData.Address.CustomField5}</p>
                          </div>
                        )}
                        {distributorData.Address.CustomField6 && (
                          <div className="col-span-2">
                            <p className="text-sm font-medium text-muted-foreground">Address Line 4</p>
                            <p>{distributorData.Address.CustomField6}</p>
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <p className="text-center text-muted-foreground">No address available</p>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Credit Information - Hidden for now */}
        {/* <div className="space-y-4">
          <h2 className="text-xl font-semibold">Credit Information</h2>
          <Card>
            <CardHeader>
              <CardTitle>Credit Details</CardTitle>
              <CardDescription>Credit limit and payment information</CardDescription>
            </CardHeader>
            <CardContent>
              {distributorData.StoreCredit ? (
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Credit Limit</p>
                    <p className="text-lg font-semibold">
                      ${distributorData.StoreCredit.CreditLimit?.toFixed(2) || '0.00'}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Temporary Credit</p>
                    <p className="text-lg font-semibold">
                      ${distributorData.StoreCredit.TemporaryCredit?.toFixed(2) || '0.00'}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Outstanding Invoices</p>
                    <p className="text-lg">{distributorData.StoreCredit.OutstandingInvoices || 0}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Payment Mode</p>
                    <p className="text-lg">{distributorData.StoreCredit.PreferredPaymentMode || '-'}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Credit Status</p>
                    <Badge variant={distributorData.StoreCredit.IsBlocked ? 'destructive' : 'default'}>
                      {distributorData.StoreCredit.IsBlocked ? 'Blocked' : 'Active'}
                    </Badge>
                  </div>
                </div>
              ) : (
                <p className="text-center text-muted-foreground">No credit information available</p>
              )}
            </CardContent>
          </Card>
        </div> */}

        {/* Documents - Hidden for now */}
        {/* <div className="space-y-4">
          <h2 className="text-xl font-semibold">Documents</h2>
          {!distributorData.Documents || distributorData.Documents.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center text-muted-foreground">
                No documents available
              </CardContent>
            </Card>
          ) : (
            distributorData.Documents.map((doc, index) => (
              <Card key={index}>
                <CardHeader>
                  <CardTitle>{doc.DocumentType || 'Document'}</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <p className="text-sm font-medium text-muted-foreground">Document Number</p>
                      <p>{doc.DocumentNo || '-'}</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium text-muted-foreground">Valid From</p>
                      <p>
                        {doc.ValidFrom
                          ? new Date(doc.ValidFrom).toLocaleDateString()
                          : '-'}
                      </p>
                    </div>
                    <div>
                      <p className="text-sm font-medium text-muted-foreground">Valid Until</p>
                      <p>
                        {doc.ValidUpTo
                          ? new Date(doc.ValidUpTo).toLocaleDateString()
                          : '-'}
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div> */}
      </div>
    </div>
  );
}
