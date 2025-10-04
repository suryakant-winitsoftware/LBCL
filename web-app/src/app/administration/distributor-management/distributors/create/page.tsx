'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  ArrowLeft,
  Save,
  Building2,
  MapPin,
  Phone,
  CreditCard,
  FileText,
  User,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { toast } from 'sonner';
import {
  distributorService,
  IDistributorMasterView,
  IOrg,
  IStore,
  IStoreAdditionalInfo,
  IStoreCredit,
  IContact,
  IAddress,
  IStoreDocument,
} from '@/services/distributor.service';
import { territoryService, Territory } from '@/services/territoryService';

export default function CreateDistributorPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('basic');
  const [territories, setTerritories] = useState<Territory[]>([]);

  // Generate UID
  const generateUID = () => `DIST${Date.now()}`;

  // Organization data
  const [orgData, setOrgData] = useState<Partial<IOrg>>({
    UID: generateUID(),
    Code: '',
    Name: '',
    OrgTypeUid: 'FR', // Franchisee/Distributor
    Status: 'Active',
    IsActive: true,
    CreatedBy: 'ADMIN',
    ModifiedBy: 'ADMIN',
  });

  // Store data
  const [storeData, setStoreData] = useState<Partial<IStore>>({
    UID: orgData.UID,
    Code: '',
    Name: '',
    Type: 'Distributor',
    Status: 'Active',
    IsActive: true,
    IsBlocked: false,
    FranchiseeOrgUid: orgData.UID,
    CreatedBy: 'ADMIN',
    ModifiedBy: 'ADMIN',
  });

  // Store Additional Info
  const [additionalInfo, setAdditionalInfo] = useState<Partial<IStoreAdditionalInfo>>({
    UID: orgData.UID,
    OrderType: 'Standard',
    IsPromotionsBlock: false,
    PaymentMode: 'Credit',
    PriceType: 'Standard',
  });

  // Store Credit
  const [creditInfo, setCreditInfo] = useState<Partial<IStoreCredit>>({
    UID: generateUID(),
    StoreUid: orgData.UID,
    CreditLimit: 0,
    TemporaryCredit: 0,
    IsActive: true,
    IsBlocked: false,
    OutstandingInvoices: 0,
    CreatedBy: 'ADMIN',
    ModifiedBy: 'ADMIN',
  });

  // Contacts
  const [contacts, setContacts] = useState<Partial<IContact>[]>([
    {
      UID: generateUID(),
      Name: '',
      Phone: '',
      Mobile: '',
      Email: '',
      Designation: '',
      LinkedItemUID: orgData.UID,
      LinkedItemType: 'Store',
      IsDefault: true,
      IsEditable: true,
      CreatedBy: 'ADMIN',
      ModifiedBy: 'ADMIN',
    },
  ]);

  // Address
  const [address, setAddress] = useState<Partial<IAddress>>({
    UID: generateUID(),
    Type: 'Billing',
    Name: '',
    Line1: '',
    Line2: '',
    City: '',
    StateCode: '',
    ZipCode: '',
    CountryCode: '',
    LinkedItemUID: orgData.UID,
    LinkedItemType: 'Store',
    IsDefault: true,
    IsEditable: true,
    Status: 'Active',
    CreatedBy: 'ADMIN',
    ModifiedBy: 'ADMIN',
  });

  // Documents
  const [documents, setDocuments] = useState<Partial<IStoreDocument>[]>([]);

  // Sync Code and Name
  useEffect(() => {
    if (orgData.Code) {
      setStoreData(prev => ({ ...prev, Code: orgData.Code }));
    }
  }, [orgData.Code]);

  useEffect(() => {
    if (orgData.Name) {
      setStoreData(prev => ({ ...prev, Name: orgData.Name }));
      setAddress(prev => ({ ...prev, Name: orgData.Name }));
    }
  }, [orgData.Name]);

  // Load territories on mount
  useEffect(() => {
    const loadTerritories = async () => {
      try {
        const result = await territoryService.getTerritories(1, 1000);
        if (result.data) {
          setTerritories(result.data);
        }
      } catch (error) {
        console.error('Error loading territories:', error);
      }
    };
    loadTerritories();
  }, []);

  // Add contact
  const addContact = () => {
    setContacts([
      ...contacts,
      {
        UID: generateUID(),
        Name: '',
        Phone: '',
        Mobile: '',
        Email: '',
        Designation: '',
        LinkedItemUID: orgData.UID,
        LinkedItemType: 'Store',
        IsDefault: false,
        IsEditable: true,
        CreatedBy: 'ADMIN',
        ModifiedBy: 'ADMIN',
      },
    ]);
  };

  // Remove contact
  const removeContact = (index: number) => {
    if (contacts.length > 1) {
      setContacts(contacts.filter((_, i) => i !== index));
    }
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validation
    if (!orgData.Name || !orgData.Code) {
      toast.error('Please fill in Distributor Name and Code');
      return;
    }

    setLoading(true);
    try {
      const now = new Date().toISOString();

      const distributorMasterView: IDistributorMasterView = {
        Org: {
          ...orgData as IOrg,
          CreatedTime: now,
          ModifiedTime: now,
        },
        Store: {
          Id: 0,
          ...storeData as IStore,
          CreatedTime: now,
          ModifiedTime: now,
        },
        StoreAdditionalInfo: {
          ...additionalInfo as IStoreAdditionalInfo,
          CreatedBy: 'ADMIN',
          CreatedTime: now,
          ModifiedBy: 'ADMIN',
          ModifiedTime: now,
        },
        StoreCredit: {
          ...creditInfo as IStoreCredit,
          CreatedTime: now,
          ModifiedTime: now,
        },
        Address: {
          ...address as IAddress,
          CreatedBy: 'ADMIN',
          CreatedTime: now,
          ModifiedBy: 'ADMIN',
          ModifiedTime: now,
        },
        Contacts: contacts.map(contact => ({
          ...contact as IContact,
          CreatedTime: now,
          ModifiedTime: now,
        })),
        Documents: documents.map(doc => ({
          ...doc as IStoreDocument,
          CreatedTime: now,
          ModifiedTime: now,
        })),
      };

      await distributorService.createDistributor(distributorMasterView);
      toast.success('Distributor created successfully');
      router.push('/administration/distributor-management/distributors');
    } catch (error: any) {
      console.error('Error creating distributor:', error);
      toast.error(error?.message || 'Failed to create distributor');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.back()}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Create Distributor</h1>
            <p className="text-muted-foreground">Add a new distributor to your network</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => router.back()}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={loading}>
            <Save className="mr-2 h-4 w-4" />
            {loading ? 'Saving...' : 'Save Distributor'}
          </Button>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Tabs value={activeTab} onValueChange={setActiveTab}>
          <TabsList className="grid w-full grid-cols-5">
            <TabsTrigger value="basic">
              <Building2 className="mr-2 h-4 w-4" />
              Basic Info
            </TabsTrigger>
            <TabsTrigger value="contacts">
              <Phone className="mr-2 h-4 w-4" />
              Contacts
            </TabsTrigger>
            <TabsTrigger value="address">
              <MapPin className="mr-2 h-4 w-4" />
              Address
            </TabsTrigger>
            <TabsTrigger value="credit">
              <CreditCard className="mr-2 h-4 w-4" />
              Credit Info
            </TabsTrigger>
            <TabsTrigger value="additional">
              <FileText className="mr-2 h-4 w-4" />
              Additional
            </TabsTrigger>
          </TabsList>

          {/* Basic Information */}
          <TabsContent value="basic" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Organization Details</CardTitle>
                <CardDescription>Basic information about the distributor</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="code">
                      Distributor Code <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id="code"
                      value={orgData.Code}
                      onChange={(e) => setOrgData({ ...orgData, Code: e.target.value })}
                      placeholder="e.g., DIST001"
                      required
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="name">
                      Distributor Name <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id="name"
                      value={orgData.Name}
                      onChange={(e) => setOrgData({ ...orgData, Name: e.target.value })}
                      placeholder="e.g., ABC Distributors"
                      required
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="status">Status</Label>
                    <Select
                      value={orgData.Status}
                      onValueChange={(value) => setOrgData({ ...orgData, Status: value })}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Active">Active</SelectItem>
                        <SelectItem value="Inactive">Inactive</SelectItem>
                        <SelectItem value="Pending">Pending</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="legalName">Legal Name</Label>
                    <Input
                      id="legalName"
                      value={storeData.LegalName}
                      onChange={(e) => setStoreData({ ...storeData, LegalName: e.target.value })}
                      placeholder="Legal business name"
                    />
                  </div>
                </div>

                <div className="flex items-center space-x-2">
                  <Switch
                    id="isActive"
                    checked={orgData.IsActive}
                    onCheckedChange={(checked) => setOrgData({ ...orgData, IsActive: checked })}
                  />
                  <Label htmlFor="isActive">Active</Label>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Contacts */}
          <TabsContent value="contacts" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Contact Information</CardTitle>
                <CardDescription>Manage distributor contacts</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {contacts.map((contact, index) => (
                  <Card key={index} className="p-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label>Contact Name {index === 0 && <span className="text-red-500">*</span>}</Label>
                        <Input
                          value={contact.Name}
                          onChange={(e) => {
                            const newContacts = [...contacts];
                            newContacts[index].Name = e.target.value;
                            setContacts(newContacts);
                          }}
                          placeholder="Full name"
                          required={index === 0}
                        />
                      </div>

                      <div className="space-y-2">
                        <Label>Mobile</Label>
                        <Input
                          value={contact.Mobile}
                          onChange={(e) => {
                            const newContacts = [...contacts];
                            newContacts[index].Mobile = e.target.value;
                            setContacts(newContacts);
                          }}
                          placeholder="+1 234 567 8900"
                        />
                      </div>

                      <div className="space-y-2">
                        <Label>Email</Label>
                        <Input
                          type="email"
                          value={contact.Email}
                          onChange={(e) => {
                            const newContacts = [...contacts];
                            newContacts[index].Email = e.target.value;
                            setContacts(newContacts);
                          }}
                          placeholder="email@example.com"
                        />
                      </div>

                      <div className="space-y-2">
                        <Label>Designation</Label>
                        <Input
                          value={contact.Designation}
                          onChange={(e) => {
                            const newContacts = [...contacts];
                            newContacts[index].Designation = e.target.value;
                            setContacts(newContacts);
                          }}
                          placeholder="e.g., Manager"
                        />
                      </div>
                    </div>

                    {index > 0 && (
                      <Button
                        type="button"
                        variant="destructive"
                        size="sm"
                        className="mt-4"
                        onClick={() => removeContact(index)}
                      >
                        Remove Contact
                      </Button>
                    )}
                  </Card>
                ))}

                <Button type="button" variant="outline" onClick={addContact}>
                  <User className="mr-2 h-4 w-4" />
                  Add Another Contact
                </Button>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Address */}
          <TabsContent value="address" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Address Information</CardTitle>
                <CardDescription>Distributor location details</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 gap-4">
                  <div className="space-y-2">
                    <Label>Address Line 1</Label>
                    <Input
                      value={address.Line1}
                      onChange={(e) => setAddress({ ...address, Line1: e.target.value })}
                      placeholder="Street address"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label>Address Line 2</Label>
                    <Input
                      value={address.Line2}
                      onChange={(e) => setAddress({ ...address, Line2: e.target.value })}
                      placeholder="Apartment, suite, etc."
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label>City</Label>
                      <Input
                        value={address.City}
                        onChange={(e) => setAddress({ ...address, City: e.target.value })}
                        placeholder="City"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>State</Label>
                      <Input
                        value={address.StateCode}
                        onChange={(e) => setAddress({ ...address, StateCode: e.target.value })}
                        placeholder="State"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>ZIP Code</Label>
                      <Input
                        value={address.ZipCode}
                        onChange={(e) => setAddress({ ...address, ZipCode: e.target.value })}
                        placeholder="ZIP"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>Country</Label>
                      <Input
                        value={address.CountryCode}
                        onChange={(e) => setAddress({ ...address, CountryCode: e.target.value })}
                        placeholder="Country"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>Territory</Label>
                      <Select
                        value={address.TerritoryCode}
                        onValueChange={(value) => setAddress({ ...address, TerritoryCode: value })}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Select Territory" />
                        </SelectTrigger>
                        <SelectContent>
                          {territories.map((territory) => (
                            <SelectItem key={territory.UID} value={territory.TerritoryCode}>
                              {territory.TerritoryName} ({territory.TerritoryCode})
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Credit Information */}
          <TabsContent value="credit" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Credit Information</CardTitle>
                <CardDescription>Credit limit and payment terms</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Credit Limit</Label>
                    <Input
                      type="number"
                      value={creditInfo.CreditLimit}
                      onChange={(e) => setCreditInfo({ ...creditInfo, CreditLimit: parseFloat(e.target.value) })}
                      placeholder="0.00"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label>Temporary Credit</Label>
                    <Input
                      type="number"
                      value={creditInfo.TemporaryCredit}
                      onChange={(e) => setCreditInfo({ ...creditInfo, TemporaryCredit: parseFloat(e.target.value) })}
                      placeholder="0.00"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label>Payment Term</Label>
                    <Input
                      value={creditInfo.PaymentTermUid}
                      onChange={(e) => setCreditInfo({ ...creditInfo, PaymentTermUid: e.target.value })}
                      placeholder="e.g., Net 30"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label>Preferred Payment Mode</Label>
                    <Select
                      value={creditInfo.PreferredPaymentMode}
                      onValueChange={(value) => setCreditInfo({ ...creditInfo, PreferredPaymentMode: value })}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select mode" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Cash">Cash</SelectItem>
                        <SelectItem value="Credit">Credit</SelectItem>
                        <SelectItem value="Cheque">Cheque</SelectItem>
                        <SelectItem value="Bank Transfer">Bank Transfer</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="flex items-center space-x-2">
                  <Switch
                    id="isBlocked"
                    checked={creditInfo.IsBlocked}
                    onCheckedChange={(checked) => setCreditInfo({ ...creditInfo, IsBlocked: checked })}
                  />
                  <Label htmlFor="isBlocked">Credit Blocked</Label>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Additional Information */}
          <TabsContent value="additional" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Additional Information</CardTitle>
                <CardDescription>Extra configuration and settings</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Order Type</Label>
                    <Select
                      value={additionalInfo.OrderType}
                      onValueChange={(value) => setAdditionalInfo({ ...additionalInfo, OrderType: value })}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Standard">Standard</SelectItem>
                        <SelectItem value="Express">Express</SelectItem>
                        <SelectItem value="Bulk">Bulk</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label>Payment Mode</Label>
                    <Select
                      value={additionalInfo.PaymentMode}
                      onValueChange={(value) => setAdditionalInfo({ ...additionalInfo, PaymentMode: value })}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Cash">Cash</SelectItem>
                        <SelectItem value="Credit">Credit</SelectItem>
                        <SelectItem value="Mixed">Mixed</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label>Price Type</Label>
                    <Select
                      value={additionalInfo.PriceType}
                      onValueChange={(value) => setAdditionalInfo({ ...additionalInfo, PriceType: value })}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Standard">Standard</SelectItem>
                        <SelectItem value="Special">Special</SelectItem>
                        <SelectItem value="Promotional">Promotional</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label>Visit Frequency (days)</Label>
                    <Input
                      type="number"
                      value={additionalInfo.VisitFrequency}
                      onChange={(e) => setAdditionalInfo({ ...additionalInfo, VisitFrequency: parseInt(e.target.value) })}
                      placeholder="7"
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <div className="flex items-center space-x-2">
                    <Switch
                      id="promotionsBlock"
                      checked={additionalInfo.IsPromotionsBlock}
                      onCheckedChange={(checked) => setAdditionalInfo({ ...additionalInfo, IsPromotionsBlock: checked })}
                    />
                    <Label htmlFor="promotionsBlock">Block Promotions</Label>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </form>
    </div>
  );
}
