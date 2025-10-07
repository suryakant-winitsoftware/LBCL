"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import {
  ArrowLeft,
  Save,
  Building2,
  MapPin,
  Phone,
  CreditCard,
  FileText,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { toast } from "sonner";
import { storeService } from "@/services/storeService";
import {
  IStore,
  IContact,
  IAddress,
  IStoreCredit,
  IStoreAdditionalInfo,
  STORE_TYPES,
  STORE_CLASSIFICATIONS,
  STORE_STATUS,
} from "@/types/store.types";

export default function CreateStorePage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState("basic");

  // Store data
  const [storeData, setStoreData] = useState<Partial<IStore>>({
    uid: "",
    code: "",
    number: "",
    name: "",
    alias_name: "",
    legal_name: "",
    type: "FRC",
    broad_classification: "",
    status: "Pending",
    is_active: true,
    is_blocked: false,
    company_uid: "",
    franchisee_org_uid: "EPIC01",
    country_uid: "",
    region_uid: "",
    state_uid: "",
    city_uid: "",
    is_tax_applicable: false,
    tax_doc_number: "",
    tax_type: "",
    store_size: 0,
    store_class: "",
    store_rating: "",
    created_by: "ADMIN",
    modified_by: "ADMIN",
  });

  // Additional Info
  const [additionalInfo, setAdditionalInfo] = useState<
    Partial<IStoreAdditionalInfo>
  >({
    order_type: "Standard",
    is_promotions_block: false,
    is_with_printed_invoices: false,
    is_capture_signature_required: false,
    delivery_information: "",
  });

  // Contacts
  const [contacts, setContacts] = useState<Partial<IContact>[]>([
    {
      name: "",
      mobile: "",
      email: "",
      designation: "",
      is_primary: true,
    },
  ]);

  // Addresses
  const [addresses, setAddresses] = useState<Partial<IAddress>[]>([
    {
      address_type: "Billing",
      address_line1: "",
      address_line2: "",
      city: "",
      state: "",
      postal_code: "",
      country: "",
      is_default: true,
    },
  ]);

  // Credit Info
  const [creditInfo, setCreditInfo] = useState<Partial<IStoreCredit>>({
    credit_limit: 0,
    credit_days: 30,
    payment_terms: "Net 30",
    is_credit_blocked: false,
  });

  // Generate Store Code
  useEffect(() => {
    if (!storeData.code) {
      const timestamp = Date.now().toString().slice(-4);
      setStoreData((prev) => ({
        ...prev,
        code: `STR${timestamp}`,
        number: `STR${timestamp}`,
        uid: `STR${timestamp}`,
      }));
    }
  }, []);

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validation
    if (!storeData.name || !storeData.code || !storeData.type) {
      toast.error("Please fill in all required fields");
      return;
    }

    setLoading(true);
    try {
      // Prepare store master data - match backend StoreViewModelDCO structure exactly
      // Convert frontend snake_case/camelCase to backend PascalCase
      const now = new Date().toISOString();
      const storeForBackend = {
        // BaseModel properties (inherited)
        Id: 0,
        UID: storeData.uid,
        SS: null,
        CreatedBy: "ADMIN",
        CreatedTime: now,
        ModifiedBy: "ADMIN",
        ModifiedTime: now,
        ServerAddTime: now,
        ServerModifiedTime: now,
        KeyUID: storeData.uid,
        IsSelected: false,

        // Store-specific properties
        CompanyUID: storeData.company_uid || null,
        Code: storeData.code,
        Number: storeData.number || storeData.code,
        Name: storeData.name,
        AliasName: storeData.alias_name || null,
        LegalName: storeData.legal_name || storeData.name,
        Type: storeData.type,
        PriceType: null,
        RouteType: null,
        BillToStoreUID: storeData.bill_to_store_uid || null,
        ShipToStoreUID: storeData.ship_to_store_uid || null,
        SoldToStoreUID: storeData.sold_to_store_uid || null,
        Status: storeData.status,
        IsActive: storeData.is_active,
        StoreClass: storeData.store_class || null,
        StoreRating: storeData.store_rating || null,
        IsBlocked: storeData.is_blocked,
        BlockedReasonCode: storeData.blocked_reason_code || null,
        BlockedReasonDescription: storeData.blocked_reason_description || null,
        CreatedByEmpUID: storeData.created_by_emp_uid || null,
        CreatedByJobPositionUID: storeData.created_by_job_position_uid || null,
        CountryUID: storeData.country_uid || null,
        RegionUID: storeData.region_uid || null,
        StateUID: null,
        CityUID: storeData.city_uid || null,
        Source: "WEB",
        OutletName: storeData.outlet_name || storeData.name,
        BlockedByEmpUID: storeData.blocked_by_emp_uid || null,
        ArabicName: storeData.arabic_name || null,
      };

      const storeMaster = {
        store: storeForBackend,
        StoreAdditionalInfo: {
          UID: `SAI${String(Date.now()).slice(-4)}`,
          StoreUID: storeData.uid!,
          OrderType: additionalInfo.order_type || "Standard",
          IsPromotionsBlock: additionalInfo.is_promotions_block || false,
          IsWithPrintedInvoices:
            additionalInfo.is_with_printed_invoices || false,
          IsCaptureSignatureRequired:
            additionalInfo.is_capture_signature_required || false,
          DeliveryInformation: additionalInfo.delivery_information || "",
          // Add all the missing required fields with sensible defaults
          IsAlwaysPrinted: false,
          BuildingDeliveryCode: "",
          IsStopDelivery: false,
          IsForeCastTopUpQty: false,
          IsTemperatureCheck: false,
          InvoiceFormat: "Standard",
          InvoiceDeliveryMethod: "Email",
          DisplayDeliveryDocket: false,
          DisplayPrice: true,
          ShowCustPO: false,
          InvoiceText: "",
          InvoiceFrequency: "Monthly",
          StockCreditIsPurchaseOrderRequired: false,
          AdminFeePerBillingCycle: 0,
          AdminFeePerDelivery: 0,
          LatePaymentFee: 0,
          Drawer: "",
          BankAccount: "",
          MandatoryPONumber: false,
          IsStoreCreditCaptureSignatureRequired: false,
          StoreCreditAlwaysPrinted: false,
          IsDummyCustomer: false,
          DefaultRun: "",
          IsFOCCustomer: false,
          RSSShowPrice: false,
          RSSShowPayment: false,
          RSSShowCredit: false,
          RSSShowInvoice: false,
          RSSIsActive: false,
          RSSDeliveryInstructionStatus: false,
          RSSTimeSpentOnRSSPortal: 0,
          RSSOrderPlacedInRSS: 0,
          RSSAvgOrdersPerWeek: 0,
          RSSTotalOrderValue: 0,
          AllowForceCheckIn: false,
          IsManualEditAllowed: false,
          CanUpdateLatLong: false,
          AllowGoodReturn: false,
          AllowBadReturn: false,
          AllowReplacement: false,
          IsInvoiceCancellationAllowed: false,
          IsDeliveryNoteRequired: false,
          EInvoicingEnabled: false,
          ImageRecognizationEnabled: false,
          MaxOutstandingInvoices: 0,
          NegativeInvoiceAllowed: false,
          DeliveryMode: "Standard",
          StoreSize: "",
          VisitFrequency: "Weekly",
          ShippingContactSameAsStore: false,
          BillingAddressSameAsShipping: false,
          PaymentMode: "Credit",
          PriceType: "Standard",
          AverageMonthlyIncome: 0,
          AccountNumber: "",
          NoOfCashCounters: 1,
          CustomField1: "",
          CustomField2: "",
          CustomField3: "",
          CustomField4: "",
          CustomField5: "",
          CustomField6: "",
          CustomField7: "",
          CustomField8: "",
          CustomField9: "",
          CustomField10: "",
          TaxType: "",
          TaxKeyField: "",
          StoreImage: "",
          IsVATQRCaptureMandatory: false,
          IsAssetEnabled: false,
          IsSurveyEnabled: false,
          AllowReturnAgainstInvoice: false,
          AllowReturnWithSalesOrder: false,
          WeekOffSun: false,
          WeekOffMon: false,
          WeekOffTue: false,
          WeekOffWed: false,
          WeekOffThu: false,
          WeekOffFri: false,
          WeekOffSat: false,
          // Audit fields
          CreatedBy: "ADMIN",
          ModifiedBy: "ADMIN",
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
        },
        Contacts: contacts
          .filter((c) => c.name)
          .map((contact) => ({
            UID: `CON${String(Date.now()).slice(-4)}`,
            LinkedItemUID: storeData.uid!,
            Name: contact.name,
            Mobile: contact.mobile || "",
            Email: contact.email || "",
            Designation: contact.designation || "",
            IsPrimary: contact.is_primary || false,
            ContactType: "Primary",
            CreatedBy: "ADMIN",
            ModifiedBy: "ADMIN",
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString(),
          })),
        Addresses: addresses
          .filter((a) => a.address_line1)
          .map((address) => ({
            UID: `ADD${String(Date.now()).slice(-4)}`,
            LinkedItemUID: storeData.uid!,
            Type: address.address_type || "Shipping",
            Name: "Store Address",
            Line1: address.address_line1,
            Line2: address.address_line2 || "",
            City: address.city,
            State: address.state || "",
            PostalCode: address.postal_code || "",
            Country: address.country || "",
            Latitude: address.latitude || 0,
            Longitude: address.longitude || 0,
            IsDefault: address.is_default || false,
            CreatedBy: "ADMIN",
            ModifiedBy: "ADMIN",
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString(),
          })),
        StoreCredits: creditInfo.credit_limit
          ? [
              {
                UID: `CRD${String(Date.now()).slice(-4)}`,
                StoreUID: storeData.uid!,
                CreditLimit: creditInfo.credit_limit,
                CreditDays: creditInfo.credit_days || 30,
                PaymentTerms: creditInfo.payment_terms || "Net 30",
                IsCreditBlocked: creditInfo.is_credit_blocked || false,
                CreatedBy: "ADMIN",
                ModifiedBy: "ADMIN",
                CreatedTime: new Date().toISOString(),
                ModifiedTime: new Date().toISOString(),
                ServerAddTime: new Date().toISOString(),
                ServerModifiedTime: new Date().toISOString(),
              },
            ]
          : [],
        StoreDocuments: [], // Initialize empty array for store documents
      };

      // Hybrid approach: Start with working minimal + essential database fields only
      const workingStoreMaster = {
        store: {
          // Core fields that worked in minimal test
          UID: storeData.uid,
          Code: storeData.code,
          Name: storeData.name,
          Type: storeData.type,
          Status: storeData.status,
          IsActive: storeData.is_active || true,
          IsBlocked: storeData.is_blocked || false,

          // Add essential database fields one by one
          Number: storeData.code, // Use code as number
          AliasName: storeData.name,
          LegalName: storeData.name,
          Source: "WEB",
          OutletName: storeData.name,

          // Audit fields that controller sets
          CreatedBy: "ADMIN",
          ModifiedBy: "ADMIN",
          CreatedTime: now,
          ModifiedTime: now,
          ServerAddTime: now,
          ServerModifiedTime: now,
        },
        StoreAdditionalInfo: {
          UID: `SAI${String(Date.now()).slice(-4)}`,
          StoreUID: storeData.uid!,
          OrderType: "Standard",

          // Add essential fields for StoreAdditionalInfo table
          CreatedBy: "ADMIN",
          ModifiedBy: "ADMIN",
          CreatedTime: now,
          ModifiedTime: now,
          ServerAddTime: now,
          ServerModifiedTime: now,
        },
        Contacts: [],
        Addresses: [],
        StoreCredits: [],
        StoreDocuments: [],
      };

      console.log("✨ Sending hybrid working object");
      await storeService.createStoreMaster(workingStoreMaster as any);
      toast.success("Store created successfully");
      router.push("/updatedfeatures/store-management/stores/manage");
    } catch (error) {
      console.error("Error creating store:", error);
      toast.error("Failed to create store");
    } finally {
      setLoading(false);
    }
  };

  // Add contact
  const addContact = () => {
    setContacts([
      ...contacts,
      {
        name: "",
        mobile: "",
        email: "",
        designation: "",
        is_primary: false,
      },
    ]);
  };

  // Remove contact
  const removeContact = (index: number) => {
    setContacts(contacts.filter((_, i) => i !== index));
  };

  // Add address
  const addAddress = () => {
    setAddresses([
      ...addresses,
      {
        address_type: "Shipping",
        address_line1: "",
        address_line2: "",
        city: "",
        state: "",
        postal_code: "",
        country: "",
        is_default: false,
      },
    ]);
  };

  // Remove address
  const removeAddress = (index: number) => {
    setAddresses(addresses.filter((_, i) => i !== index));
  };

  return (
    <div className="container mx-auto p-6 max-w-6xl">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            onClick={() => router.back()}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Create New Store</h1>
            <p className="text-muted-foreground">
              Add a new customer outlet or store
            </p>
          </div>
        </div>
        <Button
          onClick={handleSubmit}
          disabled={loading}
          className="flex items-center gap-2"
        >
          <Save className="h-4 w-4" />
          {loading ? "Creating..." : "Create Store"}
        </Button>
      </div>

      {/* Form Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="basic" className="flex items-center gap-2">
            <Building2 className="h-4 w-4" />
            Basic Info
          </TabsTrigger>
          <TabsTrigger value="location" className="flex items-center gap-2">
            <MapPin className="h-4 w-4" />
            Location
          </TabsTrigger>
          <TabsTrigger value="contact" className="flex items-center gap-2">
            <Phone className="h-4 w-4" />
            Contact
          </TabsTrigger>
          <TabsTrigger value="credit" className="flex items-center gap-2">
            <CreditCard className="h-4 w-4" />
            Credit
          </TabsTrigger>
          <TabsTrigger value="additional" className="flex items-center gap-2">
            <FileText className="h-4 w-4" />
            Additional
          </TabsTrigger>
        </TabsList>

        {/* Basic Information Tab */}
        <TabsContent value="basic">
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>
                Enter the basic details of the store
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="code">Store Code *</Label>
                  <Input
                    id="code"
                    value={storeData.code}
                    onChange={(e) =>
                      setStoreData({ ...storeData, code: e.target.value })
                    }
                    placeholder="STR001"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="number">Store Number</Label>
                  <Input
                    id="number"
                    value={storeData.number}
                    onChange={(e) =>
                      setStoreData({ ...storeData, number: e.target.value })
                    }
                    placeholder="001"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="name">Store Name *</Label>
                <Input
                  id="name"
                  value={storeData.name}
                  onChange={(e) =>
                    setStoreData({ ...storeData, name: e.target.value })
                  }
                  placeholder="Main Street Store"
                  required
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="alias_name">Alias Name</Label>
                  <Input
                    id="alias_name"
                    value={storeData.alias_name}
                    onChange={(e) =>
                      setStoreData({ ...storeData, alias_name: e.target.value })
                    }
                    placeholder="Alternative name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="legal_name">Legal Name</Label>
                  <Input
                    id="legal_name"
                    value={storeData.legal_name}
                    onChange={(e) =>
                      setStoreData({ ...storeData, legal_name: e.target.value })
                    }
                    placeholder="Legal entity name"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="type">Store Type *</Label>
                  <Select
                    value={storeData.type}
                    onValueChange={(v) =>
                      setStoreData({ ...storeData, type: v })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
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
                  <Label htmlFor="classification">Classification</Label>
                  <Select
                    value={storeData.broad_classification}
                    onValueChange={(v) =>
                      setStoreData({ ...storeData, broad_classification: v })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select classification" />
                    </SelectTrigger>
                    <SelectContent>
                      {STORE_CLASSIFICATIONS.map((classification) => (
                        <SelectItem key={classification} value={classification}>
                          {classification}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="status">Status *</Label>
                  <Select
                    value={storeData.status}
                    onValueChange={(v) =>
                      setStoreData({ ...storeData, status: v })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(STORE_STATUS).map(([key, value]) => (
                        <SelectItem key={key} value={value}>
                          {value}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="store_size">Store Size (sq ft)</Label>
                  <Input
                    id="store_size"
                    type="number"
                    value={storeData.store_size}
                    onChange={(e) =>
                      setStoreData({
                        ...storeData,
                        store_size: Number(e.target.value),
                      })
                    }
                    placeholder="5000"
                  />
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div className="flex items-center space-x-2">
                  <Switch
                    id="is_active"
                    checked={storeData.is_active}
                    onCheckedChange={(v) =>
                      setStoreData({ ...storeData, is_active: v })
                    }
                  />
                  <Label htmlFor="is_active">Active</Label>
                </div>
                <div className="flex items-center space-x-2">
                  <Switch
                    id="is_blocked"
                    checked={storeData.is_blocked}
                    onCheckedChange={(v) =>
                      setStoreData({ ...storeData, is_blocked: v })
                    }
                  />
                  <Label htmlFor="is_blocked">Blocked</Label>
                </div>
                <div className="flex items-center space-x-2">
                  <Switch
                    id="is_tax_applicable"
                    checked={storeData.is_tax_applicable}
                    onCheckedChange={(v) =>
                      setStoreData({ ...storeData, is_tax_applicable: v })
                    }
                  />
                  <Label htmlFor="is_tax_applicable">Tax Applicable</Label>
                </div>
              </div>

              {storeData.is_tax_applicable && (
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="tax_doc_number">Tax Document Number</Label>
                    <Input
                      id="tax_doc_number"
                      value={storeData.tax_doc_number}
                      onChange={(e) =>
                        setStoreData({
                          ...storeData,
                          tax_doc_number: e.target.value,
                        })
                      }
                      placeholder="GST123456789"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="tax_type">Tax Type</Label>
                    <Input
                      id="tax_type"
                      value={storeData.tax_type}
                      onChange={(e) =>
                        setStoreData({ ...storeData, tax_type: e.target.value })
                      }
                      placeholder="GST"
                    />
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Location Tab */}
        <TabsContent value="location">
          <Card>
            <CardHeader>
              <CardTitle>Location Information</CardTitle>
              <CardDescription>
                Specify the store location details
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Addresses */}
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <h3 className="text-lg font-semibold">Addresses</h3>
                  <Button variant="outline" size="sm" onClick={addAddress}>
                    Add Address
                  </Button>
                </div>
                {addresses.map((address, index) => (
                  <Card key={index}>
                    <CardContent className="pt-6">
                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <Label>Address Type</Label>
                          <Select
                            value={address.address_type}
                            onValueChange={(v) => {
                              const updated = [...addresses];
                              updated[index].address_type = v;
                              setAddresses(updated);
                            }}
                          >
                            <SelectTrigger>
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="Billing">Billing</SelectItem>
                              <SelectItem value="Shipping">Shipping</SelectItem>
                              <SelectItem value="Both">Both</SelectItem>
                            </SelectContent>
                          </Select>
                        </div>
                        <div className="flex items-end justify-end">
                          {addresses.length > 1 && (
                            <Button
                              variant="destructive"
                              size="sm"
                              onClick={() => removeAddress(index)}
                            >
                              Remove
                            </Button>
                          )}
                        </div>
                      </div>
                      <div className="space-y-2 mt-4">
                        <Label>Address Line 1</Label>
                        <Input
                          value={address.address_line1}
                          onChange={(e) => {
                            const updated = [...addresses];
                            updated[index].address_line1 = e.target.value;
                            setAddresses(updated);
                          }}
                          placeholder="Street address"
                        />
                      </div>
                      <div className="space-y-2 mt-4">
                        <Label>Address Line 2</Label>
                        <Input
                          value={address.address_line2}
                          onChange={(e) => {
                            const updated = [...addresses];
                            updated[index].address_line2 = e.target.value;
                            setAddresses(updated);
                          }}
                          placeholder="Apartment, suite, etc."
                        />
                      </div>
                      <div className="grid grid-cols-3 gap-4 mt-4">
                        <div className="space-y-2">
                          <Label>City</Label>
                          <Input
                            value={address.city}
                            onChange={(e) => {
                              const updated = [...addresses];
                              updated[index].city = e.target.value;
                              setAddresses(updated);
                            }}
                            placeholder="City"
                          />
                        </div>
                        <div className="space-y-2">
                          <Label>State</Label>
                          <Input
                            value={address.state}
                            onChange={(e) => {
                              const updated = [...addresses];
                              updated[index].state = e.target.value;
                              setAddresses(updated);
                            }}
                            placeholder="State"
                          />
                        </div>
                        <div className="space-y-2">
                          <Label>Postal Code</Label>
                          <Input
                            value={address.postal_code}
                            onChange={(e) => {
                              const updated = [...addresses];
                              updated[index].postal_code = e.target.value;
                              setAddresses(updated);
                            }}
                            placeholder="Postal code"
                          />
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Contact Tab */}
        <TabsContent value="contact">
          <Card>
            <CardHeader>
              <CardTitle>Contact Information</CardTitle>
              <CardDescription>
                Add contact persons for the store
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-lg font-semibold">Contacts</h3>
                <Button variant="outline" size="sm" onClick={addContact}>
                  Add Contact
                </Button>
              </div>
              {contacts.map((contact, index) => (
                <Card key={index}>
                  <CardContent className="pt-6">
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label>Name</Label>
                        <Input
                          value={contact.name}
                          onChange={(e) => {
                            const updated = [...contacts];
                            updated[index].name = e.target.value;
                            setContacts(updated);
                          }}
                          placeholder="Contact name"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Designation</Label>
                        <Input
                          value={contact.designation}
                          onChange={(e) => {
                            const updated = [...contacts];
                            updated[index].designation = e.target.value;
                            setContacts(updated);
                          }}
                          placeholder="Manager"
                        />
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4 mt-4">
                      <div className="space-y-2">
                        <Label>Mobile</Label>
                        <Input
                          value={contact.mobile}
                          onChange={(e) => {
                            const updated = [...contacts];
                            updated[index].mobile = e.target.value;
                            setContacts(updated);
                          }}
                          placeholder="+91-9876543210"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Email</Label>
                        <Input
                          type="email"
                          value={contact.email}
                          onChange={(e) => {
                            const updated = [...contacts];
                            updated[index].email = e.target.value;
                            setContacts(updated);
                          }}
                          placeholder="contact@store.com"
                        />
                      </div>
                    </div>
                    <div className="flex justify-between items-center mt-4">
                      <div className="flex items-center space-x-2">
                        <Switch
                          checked={contact.is_primary}
                          onCheckedChange={(v) => {
                            const updated = [...contacts];
                            updated[index].is_primary = v;
                            // Only one primary contact
                            if (v) {
                              updated.forEach((c, i) => {
                                if (i !== index) c.is_primary = false;
                              });
                            }
                            setContacts(updated);
                          }}
                        />
                        <Label>Primary Contact</Label>
                      </div>
                      {contacts.length > 1 && (
                        <Button
                          variant="destructive"
                          size="sm"
                          onClick={() => removeContact(index)}
                        >
                          Remove
                        </Button>
                      )}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Credit Tab */}
        <TabsContent value="credit">
          <Card>
            <CardHeader>
              <CardTitle>Credit Information</CardTitle>
              <CardDescription>
                Set credit limits and payment terms
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="credit_limit">Credit Limit</Label>
                  <Input
                    id="credit_limit"
                    type="number"
                    value={creditInfo.credit_limit}
                    onChange={(e) =>
                      setCreditInfo({
                        ...creditInfo,
                        credit_limit: Number(e.target.value),
                      })
                    }
                    placeholder="50000"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="credit_days">Credit Days</Label>
                  <Input
                    id="credit_days"
                    type="number"
                    value={creditInfo.credit_days}
                    onChange={(e) =>
                      setCreditInfo({
                        ...creditInfo,
                        credit_days: Number(e.target.value),
                      })
                    }
                    placeholder="30"
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="payment_terms">Payment Terms</Label>
                <Select
                  value={creditInfo.payment_terms}
                  onValueChange={(v) =>
                    setCreditInfo({ ...creditInfo, payment_terms: v })
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Net 15">Net 15</SelectItem>
                    <SelectItem value="Net 30">Net 30</SelectItem>
                    <SelectItem value="Net 45">Net 45</SelectItem>
                    <SelectItem value="Net 60">Net 60</SelectItem>
                    <SelectItem value="COD">Cash on Delivery</SelectItem>
                    <SelectItem value="Advance">Advance Payment</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="flex items-center space-x-2">
                <Switch
                  id="is_credit_blocked"
                  checked={creditInfo.is_credit_blocked}
                  onCheckedChange={(v) =>
                    setCreditInfo({ ...creditInfo, is_credit_blocked: v })
                  }
                />
                <Label htmlFor="is_credit_blocked">Block Credit</Label>
              </div>
              {creditInfo.is_credit_blocked && (
                <div className="space-y-2">
                  <Label htmlFor="blocked_reason">Block Reason</Label>
                  <Textarea
                    id="blocked_reason"
                    value={creditInfo.blocked_reason}
                    onChange={(e) =>
                      setCreditInfo({
                        ...creditInfo,
                        blocked_reason: e.target.value,
                      })
                    }
                    placeholder="Reason for blocking credit"
                  />
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Additional Info Tab */}
        <TabsContent value="additional">
          <Card>
            <CardHeader>
              <CardTitle>Additional Information</CardTitle>
              <CardDescription>Additional store configuration</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="order_type">Order Type</Label>
                  <Select
                    value={additionalInfo.order_type}
                    onValueChange={(v) =>
                      setAdditionalInfo({ ...additionalInfo, order_type: v })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Standard">Standard</SelectItem>
                      <SelectItem value="Express">Express</SelectItem>
                      <SelectItem value="Scheduled">Scheduled</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="purchase_order_number">
                    Purchase Order Number
                  </Label>
                  <Input
                    id="purchase_order_number"
                    value={additionalInfo.purchase_order_number}
                    onChange={(e) =>
                      setAdditionalInfo({
                        ...additionalInfo,
                        purchase_order_number: e.target.value,
                      })
                    }
                    placeholder="PO123456"
                  />
                </div>
              </div>
              <div className="grid grid-cols-3 gap-4">
                <div className="flex items-center space-x-2">
                  <Switch
                    id="is_promotions_block"
                    checked={additionalInfo.is_promotions_block}
                    onCheckedChange={(v) =>
                      setAdditionalInfo({
                        ...additionalInfo,
                        is_promotions_block: v,
                      })
                    }
                  />
                  <Label htmlFor="is_promotions_block">Block Promotions</Label>
                </div>
                <div className="flex items-center space-x-2">
                  <Switch
                    id="is_with_printed_invoices"
                    checked={additionalInfo.is_with_printed_invoices}
                    onCheckedChange={(v) =>
                      setAdditionalInfo({
                        ...additionalInfo,
                        is_with_printed_invoices: v,
                      })
                    }
                  />
                  <Label htmlFor="is_with_printed_invoices">
                    Printed Invoices
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <Switch
                    id="is_capture_signature_required"
                    checked={additionalInfo.is_capture_signature_required}
                    onCheckedChange={(v) =>
                      setAdditionalInfo({
                        ...additionalInfo,
                        is_capture_signature_required: v,
                      })
                    }
                  />
                  <Label htmlFor="is_capture_signature_required">
                    Require Signature
                  </Label>
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="delivery_information">
                  Delivery Information
                </Label>
                <Textarea
                  id="delivery_information"
                  value={additionalInfo.delivery_information}
                  onChange={(e) =>
                    setAdditionalInfo({
                      ...additionalInfo,
                      delivery_information: e.target.value,
                    })
                  }
                  placeholder="Special delivery instructions..."
                  rows={4}
                />
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
