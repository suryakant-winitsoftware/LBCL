'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  ArrowLeft,
  Save,
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
import { orgService, IOrganization } from '@/services/orgService';
import { organizationService } from '@/services/organizationService';

// Import organization hierarchy utilities
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
} from '@/utils/organizationHierarchyUtils';

export default function CreateDistributorPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [territories, setTerritories] = useState<Territory[]>([]);
  const [parentOrgs, setParentOrgs] = useState<IOrganization[]>([]);

  // Organization hierarchy state
  const [organizations, setOrganizations] = useState<any[]>([]);
  const [orgTypes, setOrgTypes] = useState<any[]>([]);
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]);
  const [selectedOrgs, setSelectedOrgs] = useState<{ [key: number]: string }>({});

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
    LegalName: '',
    Number: '',
    Type: 'Distributor',
    Status: 'Active',
    IsActive: true,
    IsBlocked: false,
    FranchiseeOrgUid: orgData.UID,
    TaxDocNumber: '',
    GSTNo: '',
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
    TerritoryCode: '',
    Depot: '',
    LocationUID: '',
    CustomField1: '',
    CustomField2: '',
    CustomField3: '',
    CustomField4: '',
    CustomField5: '',
    CustomField6: '',
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

  // Load territories and organizations on mount
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        // Load territories
        const territoriesResult = await territoryService.getTerritories(1, 1000);
        if (territoriesResult.data) {
          setTerritories(territoriesResult.data);
        }

        // Load organization types and organizations
        const [typesResult, orgsResult] = await Promise.all([
          organizationService.getOrganizationTypes(),
          organizationService.getOrganizations(1, 1000),
        ]);

        // Filter to only show active organizations with ShowInTemplate
        const activeOrgs = orgsResult.data.filter(
          (org: any) => org.ShowInTemplate === true
        );

        setOrganizations(activeOrgs);
        setOrgTypes(typesResult);

        // Initialize organization hierarchy
        const initialLevels = initializeOrganizationHierarchy(
          activeOrgs,
          typesResult
        );
        setOrgLevels(initialLevels);

        console.log('Loaded organizations:', activeOrgs.length);
        console.log('Organization hierarchy levels:', initialLevels.length);
      } catch (error) {
        console.error('Error loading initial data:', error);
        toast({
          title: 'Error',
          description: 'Failed to load organization data',
          variant: 'destructive',
        });
      }
    };

    loadInitialData();
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

  // Handle organization hierarchy selection
  const handleOrganizationSelect = (levelIndex: number, value: string) => {
    if (!value) return;

    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      orgLevels,
      selectedOrgs,
      organizations,
      orgTypes
    );

    setOrgLevels(updatedLevels);
    setSelectedOrgs(updatedSelectedOrgs);

    // Get the final selected organization UID
    const finalOrgUID = getFinalSelectedOrganization(updatedSelectedOrgs);
    if (finalOrgUID) {
      // Set parent organization
      setOrgData({ ...orgData, ParentUid: finalOrgUID });
      console.log('Selected parent org:', finalOrgUID);
    }
  };

  // Reset organization selection
  const resetOrganizationSelection = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(
      organizations,
      orgTypes
    );
    setOrgLevels(resetLevels);
    setSelectedOrgs(resetSelectedOrgs);
    setOrgData({ ...orgData, ParentUid: undefined });
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
      <form onSubmit={handleSubmit} className="space-y-6">
          {/* Basic Information */}
          <div className="space-y-4">
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

                  <div className="space-y-2">
                    <Label htmlFor="alphaSearchCode">Alpha Search Code</Label>
                    <Input
                      id="alphaSearchCode"
                      value={storeData.Number}
                      onChange={(e) => setStoreData({ ...storeData, Number: e.target.value })}
                      placeholder="Search/alias code"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="vatRegNo">VAT Registration Number</Label>
                    <Input
                      id="vatRegNo"
                      value={storeData.TaxDocNumber}
                      onChange={(e) => setStoreData({ ...storeData, TaxDocNumber: e.target.value })}
                      placeholder="VAT/Tax registration number"
                    />
                  </div>

                  {/* <div className="space-y-2">
                    <Label htmlFor="gstNo">GST Number</Label>
                    <Input
                      id="gstNo"
                      value={storeData.GSTNo}
                      onChange={(e) => setStoreData({ ...storeData, GSTNo: e.target.value })}
                      placeholder="GST number"
                    />
                  </div> */}

                  {/* Organization Hierarchy - Dynamic Cascading Fields */}
                  {orgLevels.length > 0 ? (
                    <>
                      {orgLevels.map((level, index) => (
                        <div key={`${level.orgTypeUID}_${index}`} className="space-y-2">
                          <Label>
                            {level.orgTypeName || `Level ${index + 1}`}
                            {index === 0 && <span className="text-red-500"> *</span>}
                          </Label>
                          <Select
                            value={level.selectedOrgUID || ''}
                            onValueChange={(value) => handleOrganizationSelect(index, value)}
                          >
                            <SelectTrigger>
                              <SelectValue placeholder={`Select ${level.orgTypeName || 'organization'}`} />
                            </SelectTrigger>
                            <SelectContent>
                              {level.organizations.map((org) => (
                                <SelectItem key={org.UID} value={org.UID}>
                                  {org.Name}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        </div>
                      ))}
                      {Object.keys(selectedOrgs).length > 0 && (
                        <div className="col-span-2">
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={resetOrganizationSelection}
                          >
                            Reset Organization Selection
                          </Button>
                        </div>
                      )}
                    </>
                  ) : (
                    <div className="col-span-2 text-center py-4 text-muted-foreground">
                      <p className="text-sm">Loading organization hierarchy...</p>
                    </div>
                  )}

                  {/* Territory Selection */}
                  <div className="col-span-2 space-y-2">
                    <Label htmlFor="territory">Territory</Label>
                    <Select
                      value={orgData.TerritoryUid || ''}
                      onValueChange={(value) => setOrgData({ ...orgData, TerritoryUid: value })}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select territory" />
                      </SelectTrigger>
                      <SelectContent>
                        {territories.map((territory) => (
                          <SelectItem key={territory.UID} value={territory.UID}>
                            {territory.TerritoryName} ({territory.TerritoryCode})
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
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
          </div>

          {/* Contacts */}
          <div className="space-y-4">
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
          </div>

          {/* Address */}
          <div className="space-y-4">
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

                    <div className="space-y-2">
                      <Label>Destination Warehouse Code</Label>
                      <Input
                        value={address.Depot}
                        onChange={(e) => setAddress({ ...address, Depot: e.target.value })}
                        placeholder="Warehouse/Depot code"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>Destination Location Code</Label>
                      <Input
                        value={address.LocationUID}
                        onChange={(e) => setAddress({ ...address, LocationUID: e.target.value })}
                        placeholder="Location code"
                      />
                    </div>
                  </div>

                  {/* Secondary Location Section */}
                  <div className="border-t pt-4 mt-4">
                    <h4 className="text-sm font-medium mb-4">Secondary Location (Optional)</h4>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label>Secondary Location Code</Label>
                        <Input
                          value={address.CustomField1}
                          onChange={(e) => setAddress({ ...address, CustomField1: e.target.value })}
                          placeholder="Secondary location code"
                        />
                      </div>

                      <div className="space-y-2">
                        <Label>Secondary Location Description</Label>
                        <Input
                          value={address.CustomField2}
                          onChange={(e) => setAddress({ ...address, CustomField2: e.target.value })}
                          placeholder="Description"
                        />
                      </div>

                      <div className="space-y-2 col-span-2">
                        <Label>Secondary Address Line 1</Label>
                        <Input
                          value={address.CustomField3}
                          onChange={(e) => setAddress({ ...address, CustomField3: e.target.value })}
                          placeholder="Address line 1"
                        />
                      </div>

                      <div className="space-y-2 col-span-2">
                        <Label>Secondary Address Line 2</Label>
                        <Input
                          value={address.CustomField4}
                          onChange={(e) => setAddress({ ...address, CustomField4: e.target.value })}
                          placeholder="Address line 2"
                        />
                      </div>

                      <div className="space-y-2 col-span-2">
                        <Label>Secondary Address Line 3</Label>
                        <Input
                          value={address.CustomField5}
                          onChange={(e) => setAddress({ ...address, CustomField5: e.target.value })}
                          placeholder="Address line 3"
                        />
                      </div>

                      <div className="space-y-2 col-span-2">
                        <Label>Secondary Address Line 4</Label>
                        <Input
                          value={address.CustomField6}
                          onChange={(e) => setAddress({ ...address, CustomField6: e.target.value })}
                          placeholder="Address line 4"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Credit Information - Hidden for now */}
          {/* <TabsContent value="credit" className="space-y-4">
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
          </TabsContent> */}

          {/* Additional Information - Hidden for now */}
          {/* <TabsContent value="additional" className="space-y-4">
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
          </div> */}
      </form>
    </div>
  );
}
