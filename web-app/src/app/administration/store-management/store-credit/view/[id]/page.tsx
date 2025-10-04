'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import {
  ArrowLeft,
  Store, CreditCard,
  AlertCircle, DollarSign, Clock
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';
import { storeCreditService } from '@/services/storeCreditService';
import { IStoreCredit } from '@/types/storeCredit.types';
import { formatDateToDayMonthYear } from '@/utils/date-formatter';

export default function ViewStoreCreditPage() {
  const router = useRouter();
  const params = useParams();
  const storeCreditId = params.id as string;

  const [storeCredit, setStoreCredit] = useState<IStoreCredit | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (storeCreditId) {
      fetchStoreCreditDetails();
    }
  }, [storeCreditId]);

  const fetchStoreCreditDetails = async () => {
    setLoading(true);
    try {
      const creditDetails = await storeCreditService.getStoreCreditByUID(storeCreditId);
      setStoreCredit(creditDetails);
    } catch (error) {
      console.error('Error fetching store credit details:', error);
      toast.error('Failed to fetch store credit details');
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadgeVariant = (isActive?: boolean, isBlocked?: boolean) => {
    if (isBlocked) return 'destructive';
    if (isActive) return 'default';
    return 'secondary';
  };

  const getStatusText = (isActive?: boolean, isBlocked?: boolean) => {
    if (isBlocked) return 'Blocked';
    if (isActive) return 'Active';
    return 'Inactive';
  };

  const getCreditTypeBadgeVariant = (creditType?: string) => {
    switch (creditType?.toUpperCase()) {
      case 'CASH':
        return 'default';
      case 'CREDIT':
        return 'secondary';
      case 'MIXED':
        return 'outline';
      default:
        return 'secondary';
    }
  };

  const formatCurrency = (amount?: number) => {
    if (amount === undefined || amount === null) return '$0.00';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2,
    }).format(amount);
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

  if (!storeCredit) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center py-8">
          <h2 className="text-2xl font-bold mb-2">Store Credit Not Found</h2>
          <p className="text-muted-foreground mb-4">The requested store credit could not be found.</p>
          <Button onClick={() => router.push('/administration/store-management/store-credit')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Store Credit Management
          </Button>
        </div>
      </div>
    );
  }

  const isActive = storeCredit.IsActive ?? storeCredit.is_active;
  const isBlocked = storeCredit.IsBlocked ?? storeCredit.is_blocked;

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.push('/administration/store-management/store-credit')}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Store Credits
        </Button>
      </div>

      {/* Store Credit Basic Information */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-2xl flex items-center gap-2">
              <Store className="h-6 w-6" />
              Store ID: {storeCredit.StoreUID || storeCredit.store_uid}
            </CardTitle>
            <Badge variant={getStatusBadgeVariant(isActive, isBlocked)}>
              {getStatusText(isActive, isBlocked)}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            <div>
              <p className="text-sm text-muted-foreground mb-1">Store ID</p>
              <p className="font-semibold">{storeCredit.StoreUID || storeCredit.store_uid}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Organization</p>
              <p className="font-semibold">{storeCredit.OrgUID || storeCredit.org_uid}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Credit Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            Credit Information
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            <div>
              <p className="text-sm text-muted-foreground mb-1">Credit Type</p>
              <Badge variant={getCreditTypeBadgeVariant(storeCredit.CreditType || storeCredit.credit_type)}>
                {storeCredit.CreditType || storeCredit.credit_type}
              </Badge>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Credit Limit</p>
              <p className="font-semibold">{formatCurrency(storeCredit.CreditLimit ?? storeCredit.credit_limit)}</p>
            </div>
            {/* <div>
              <p className="text-sm text-muted-foreground mb-1">Temporary Credit</p>
              <p className="font-semibold">{formatCurrency(storeCredit.TemporaryCredit ?? storeCredit.temporary_credit)}</p>
            </div> */}
            <div>
              <p className="text-sm text-muted-foreground mb-1">Credit Days</p>
              <p className="font-semibold">{storeCredit.CreditDays ?? storeCredit.credit_days ?? 0} days</p>
            </div>
            {/* <div>
              <p className="text-sm text-muted-foreground mb-1">Temporary Credit Days</p>
              <p className="font-semibold">{storeCredit.TemporaryCreditDays ?? storeCredit.temporary_credit_days ?? 0} days</p>
            </div> */}
            {/* <div>
              <p className="text-sm text-muted-foreground mb-1">Total Balance</p>
              <p className="font-semibold">{formatCurrency(storeCredit.TotalBalance ?? storeCredit.total_balance)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Overdue Balance</p>
              <p className="font-semibold">{formatCurrency(storeCredit.OverdueBalance ?? storeCredit.overdue_balance)}</p>
            </div> */}
            <div>
              <p className="text-sm text-muted-foreground mb-1">Available Balance</p>
              <p className="font-semibold">{formatCurrency(storeCredit.AvailableBalance ?? storeCredit.available_balance)}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Payment Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <DollarSign className="h-5 w-5" />
            Payment Information
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            <div>
              <p className="text-sm text-muted-foreground mb-1">Payment Term UID</p>
              <p className="font-semibold">{storeCredit.PaymentTermUID || storeCredit.payment_term_uid}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Preferred Payment Mode</p>
              <p className="font-semibold">{storeCredit.PreferredPaymentMode || storeCredit.preferred_payment_mode}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Price List</p>
              <p className="font-semibold">{storeCredit.PriceList || storeCredit.price_list}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Invoice & Outstanding Information - Hidden */}
      {/* <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Invoice & Outstanding Information
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            <div>
              <p className="text-sm text-muted-foreground mb-1">Outstanding Invoices</p>
              <p className="font-semibold">{formatCurrency(storeCredit.OutstandingInvoices ?? storeCredit.outstanding_invoices)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Invoice Admin Fee (Per Cycle)</p>
              <p className="font-semibold">{formatCurrency(storeCredit.InvoiceAdminFeePerBillingCycle ?? storeCredit.invoice_admin_fee_per_billing_cycle)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Invoice Admin Fee (Per Delivery)</p>
              <p className="font-semibold">{formatCurrency(storeCredit.InvoiceAdminFeePerDelivery ?? storeCredit.invoice_admin_fee_per_delivery)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Invoice Late Payment Fee</p>
              <p className="font-semibold">{formatCurrency(storeCredit.InvoiceLatePaymentFee ?? storeCredit.invoice_late_payment_fee)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Allow Cash on Credit Exceed</p>
              <Badge variant={(storeCredit.IsAllowCashOnCreditExceed ?? storeCredit.is_allow_cash_on_credit_exceed) ? 'default' : 'secondary'}>
                {(storeCredit.IsAllowCashOnCreditExceed ?? storeCredit.is_allow_cash_on_credit_exceed) ? 'Yes' : 'No'}
              </Badge>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Outstanding Bill Control</p>
              <Badge variant={(storeCredit.IsOutstandingBillControl ?? storeCredit.is_outstanding_bill_control) ? 'default' : 'secondary'}>
                {(storeCredit.IsOutstandingBillControl ?? storeCredit.is_outstanding_bill_control) ? 'Enabled' : 'Disabled'}
              </Badge>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Invoice Cancellation Allowed</p>
              <Badge variant={(storeCredit.IsCancellationOfInvoiceAllowed ?? storeCredit.is_cancellation_of_invoice_allowed) ? 'default' : 'secondary'}>
                {(storeCredit.IsCancellationOfInvoiceAllowed ?? storeCredit.is_cancellation_of_invoice_allowed) ? 'Yes' : 'No'}
              </Badge>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Negative Invoice Allowed</p>
              <Badge variant={(storeCredit.IsNegativeInvoiceAllowed ?? storeCredit.is_negative_invoice_allowed) ? 'default' : 'secondary'}>
                {(storeCredit.IsNegativeInvoiceAllowed ?? storeCredit.is_negative_invoice_allowed) ? 'Yes' : 'No'}
              </Badge>
            </div>
          </div>
        </CardContent>
      </Card> */}

      {/* Status Information - Only show if blocked */}
      {isBlocked && (
        <Card className="border-destructive">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-destructive">
              <AlertCircle className="h-5 w-5" />
              Blocking Information
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <p className="text-sm text-muted-foreground mb-1">Blocking Reason Code</p>
                <p className="font-semibold">{storeCredit.BlockingReasonCode || storeCredit.blocking_reason_code}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground mb-1">Blocking Reason Description</p>
                <p className="font-semibold">{storeCredit.BlockingReasonDescription || storeCredit.blocking_reason_description}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Audit Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Clock className="h-5 w-5" />
            Audit Information
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            <div>
              <p className="text-sm text-muted-foreground mb-1">Created By</p>
              <p className="font-semibold">{storeCredit.CreatedBy || storeCredit.created_by}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Created Time</p>
              <p className="font-semibold">{formatDateToDayMonthYear(storeCredit.CreatedTime || storeCredit.created_time)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Modified By</p>
              <p className="font-semibold">{storeCredit.ModifiedBy || storeCredit.modified_by}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Modified Time</p>
              <p className="font-semibold">{formatDateToDayMonthYear(storeCredit.ModifiedTime || storeCredit.modified_time)}</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}