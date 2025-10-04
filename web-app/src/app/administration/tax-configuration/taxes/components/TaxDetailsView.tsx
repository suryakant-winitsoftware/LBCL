'use client';

import React, { useState, useEffect } from 'react';
import { taxService, ITaxMaster, ITax } from '@/services/tax/tax.service';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useToast } from '@/components/ui/use-toast';
import { formatDateToDayMonthYear } from '@/utils/date-formatter';
import { 
  RefreshCw, 
  Calendar, 
  Percent, 
  FileText, 
  Building2,
  Hash,
  User,
  Clock,
  Trash2,
  AlertCircle
} from 'lucide-react';
import { useRouter } from 'next/navigation';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog';

interface TaxDetailsViewProps {
  taxUID: string | null;
}

const TaxDetailsView: React.FC<TaxDetailsViewProps> = ({ taxUID }) => {
  const router = useRouter();
  const { toast } = useToast();
  const [taxMaster, setTaxMaster] = useState<ITaxMaster | null>(null);
  const [loading, setLoading] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (taxUID) {
      loadTaxDetails(taxUID);
    }
  }, [taxUID]);

  const loadTaxDetails = async (uid: string) => {
    setLoading(true);
    try {
      const details = await taxService.selectTaxMasterViewByUID(uid);
      if (details) {
        setTaxMaster(details);
      } else {
        toast({
          title: 'Not Found',
          description: 'Tax details not found',
          variant: 'destructive',
        });
      }
    } catch (error) {
      console.error('Error loading tax details:', error);
      toast({
        title: 'Error',
        description: 'Failed to load tax details',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!taxUID) return;
    
    setDeleting(true);
    try {
      await taxService.deleteTax(taxUID);
      toast({
        title: 'Success',
        description: 'Tax deleted successfully',
      });
      // Navigate back to tax list
      router.push('/administration/tax-configuration/taxes');
    } catch (error) {
      console.error('Error deleting tax:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete tax',
        variant: 'destructive',
      });
    } finally {
      setDeleting(false);
    }
  };

  const formatDate = (dateString: string | undefined) => {
    return formatDateToDayMonthYear(dateString);
  };

  const formatDateTime = (dateString: string | undefined) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (!taxUID) {
    return (
      <Card>
        <CardContent className="text-center py-12">
          <FileText className="h-12 w-12 mx-auto mb-4 text-gray-400" />
          <p className="text-gray-600">
            Select a tax from the Tax List to view its details
          </p>
        </CardContent>
      </Card>
    );
  }

  if (loading) {
    return (
      <Card>
        <CardContent className="text-center py-12">
          <RefreshCw className="h-8 w-8 animate-spin mx-auto mb-4" />
          <p className="text-gray-600">Loading tax details...</p>
        </CardContent>
      </Card>
    );
  }

  if (!taxMaster) {
    return (
      <Card>
        <CardContent className="text-center py-12">
          <p className="text-gray-600">No tax details available</p>
        </CardContent>
      </Card>
    );
  }

  const tax = taxMaster.Tax;

  return (
    <div className="space-y-6">
      <CardHeader className="px-0 pt-0">
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="text-2xl">{tax.Name}</CardTitle>
            <p className="text-gray-600 mt-1">{tax.LegalName}</p>
          </div>
          <div className="flex items-center gap-2">
            <Badge variant={tax.Status === 'Active' ? 'default' : 'secondary'}>
              {tax.Status}
            </Badge>
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button
                  variant="destructive"
                  size="icon"
                  disabled={deleting}
                  title="Delete Tax"
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle className="flex items-center gap-2">
                    <AlertCircle className="h-5 w-5 text-red-500" />
                    Confirm Delete
                  </AlertDialogTitle>
                  <AlertDialogDescription>
                    Are you sure you want to delete the tax "{tax.Name}"?
                    This action cannot be undone.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={handleDelete}
                    className="bg-red-500 hover:bg-red-600"
                  >
                    Delete Tax
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
            <Button
              onClick={() => loadTaxDetails(taxUID)}
              variant="outline"
              size="icon"
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </div>
      </CardHeader>

      <Tabs defaultValue="general" className="w-full">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="general">General Information</TabsTrigger>
          <TabsTrigger value="tax-details">Tax Details</TabsTrigger>
          <TabsTrigger value="audit">Audit Information</TabsTrigger>
        </TabsList>

        <TabsContent value="general" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Basic Information
              </CardTitle>
            </CardHeader>
            <CardContent className="grid grid-cols-2 gap-6">
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Tax Code</p>
                <p className="font-mono font-medium">{tax.Code}</p>
              </div>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">UID</p>
                <p className="font-mono text-sm">{tax.UID}</p>
              </div>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Name</p>
                <p className="font-medium">{tax.Name}</p>
              </div>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Legal Name</p>
                <p className="font-medium">{tax.LegalName || '-'}</p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="tax-details" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Percent className="h-5 w-5" />
                Tax Configuration
              </CardTitle>
            </CardHeader>
            <CardContent className="grid grid-cols-2 gap-6">
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Tax Rate</p>
                <p className="text-2xl font-bold text-blue-600">
                  {tax.BaseTaxRate}%
                </p>
              </div>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Calculation Type</p>
                <Badge variant="outline" className="mt-1">
                  {tax.TaxCalculationType || 'Percentage'}
                </Badge>
              </div>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Applicable At</p>
                <p className="font-medium">{tax.ApplicableAt || 'Item'}</p>
              </div>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Tax on Tax</p>
                <Badge variant={tax.IsTaxOnTaxApplicable ? 'default' : 'secondary'}>
                  {tax.IsTaxOnTaxApplicable ? 'Enabled' : 'Disabled'}
                </Badge>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Calendar className="h-5 w-5" />
                Validity Period
              </CardTitle>
            </CardHeader>
            <CardContent className="grid grid-cols-2 gap-6">
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Valid From</p>
                <p className="font-medium">{formatDate(tax.ValidFrom)}</p>
              </div>
              <div className="space-y-1">
                <p className="text-sm text-gray-600">Valid Until</p>
                <p className="font-medium">{formatDate(tax.ValidUpto)}</p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="audit" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Audit Trail
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-1">
                  <p className="text-sm text-gray-600">Created By</p>
                  <div className="flex items-center gap-2">
                    <User className="h-4 w-4 text-gray-400" />
                    <p className="font-medium">{tax.CreatedBy || 'System'}</p>
                  </div>
                </div>
                <div className="space-y-1">
                  <p className="text-sm text-gray-600">Created On</p>
                  <p className="font-medium">{formatDateTime(tax.CreatedTime)}</p>
                </div>
                <div className="space-y-1">
                  <p className="text-sm text-gray-600">Modified By</p>
                  <div className="flex items-center gap-2">
                    <User className="h-4 w-4 text-gray-400" />
                    <p className="font-medium">{tax.ModifiedBy || 'System'}</p>
                  </div>
                </div>
                <div className="space-y-1">
                  <p className="text-sm text-gray-600">Modified On</p>
                  <p className="font-medium">{formatDateTime(tax.ModifiedTime)}</p>
                </div>
              </div>
              <div className="border-t pt-4 grid grid-cols-2 gap-6">
                <div className="space-y-1">
                  <p className="text-sm text-gray-600">Server Add Time</p>
                  <p className="font-mono text-sm">
                    {formatDateTime(tax.ServerAddTime)}
                  </p>
                </div>
                <div className="space-y-1">
                  <p className="text-sm text-gray-600">Server Modified Time</p>
                  <p className="font-mono text-sm">
                    {formatDateTime(tax.ServerModifiedTime)}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* SKU Mappings if available */}
      {taxMaster.TaxSKUMapList && taxMaster.TaxSKUMapList.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              SKU Mappings
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">
              This tax is mapped to {taxMaster.TaxSKUMapList.length} SKUs
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default TaxDetailsView;