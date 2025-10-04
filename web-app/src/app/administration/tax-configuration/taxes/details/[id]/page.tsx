'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { taxService, ITaxMaster, ITax } from '@/services/tax/tax.service';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useToast } from '@/components/ui/use-toast';
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
  AlertCircle,
  ArrowLeft,
  Edit
} from 'lucide-react';
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

const TaxDetailsPage: React.FC = () => {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const [taxMaster, setTaxMaster] = useState<ITaxMaster | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);

  const taxUID = params.id as string;

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
        router.push('/administration/tax-configuration/taxes');
      }
    } catch (error) {
      console.error('Error loading tax details:', error);
      toast({
        title: 'Error',
        description: 'Failed to load tax details',
        variant: 'destructive',
      });
      router.push('/administration/tax-configuration/taxes');
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
    } catch (error: any) {
      console.error('Error deleting tax:', error);
      toast({
        title: 'Cannot Delete Tax',
        description: error.message || 'Failed to delete tax. This tax may be assigned to tax groups or mapped to SKUs.',
        variant: 'destructive',
      });
    } finally {
      setDeleting(false);
    }
  };

  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
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

  if (loading) {
    return (
      <div className="container mx-auto p-6 max-w-6xl">
        <div className="flex items-center justify-center py-12">
          <RefreshCw className="h-8 w-8 animate-spin mr-2" />
          <span>Loading tax details...</span>
        </div>
      </div>
    );
  }

  if (!taxMaster) {
    return (
      <div className="container mx-auto p-6 max-w-6xl">
        <div className="text-center py-12">
          <p className="text-gray-600 mb-4">Tax not found</p>
          <Button onClick={() => router.push('/administration/tax-configuration/taxes')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Tax List
          </Button>
        </div>
      </div>
    );
  }

  const tax = taxMaster.Tax;

  return (
    <div className="container mx-auto p-6 max-w-6xl">
      {/* Header with Back Button */}
      <div className="mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.push('/administration/tax-configuration/taxes')}
          className="mb-4"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Tax Management
        </Button>
        
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">{tax.Name}</h1>
            <p className="text-gray-600 mt-1">{tax.LegalName}</p>
            <div className="flex items-center gap-2 mt-2">
              <Badge variant={tax.Status === 'Active' ? 'default' : 'secondary'}>
                {tax.Status}
              </Badge>
              <span className="text-sm text-gray-500">•</span>
              <span className="text-sm text-gray-500">UID: {tax.UID}</span>
            </div>
          </div>
          
          <div className="flex items-center gap-2">
            <Button
              onClick={() => router.push(`/administration/tax-configuration/taxes/edit/${taxUID}`)}
              variant="outline"
            >
              <Edit className="h-4 w-4 mr-2" />
              Edit Tax
            </Button>
            
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button
                  variant="destructive"
                  disabled={deleting}
                >
                  <Trash2 className="h-4 w-4 mr-2" />
                  Delete Tax
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
                    <br/><br/>
                    <strong>Important:</strong> If this tax is assigned to tax groups or mapped to SKUs,
                    you must remove those associations first. This action cannot be undone and may affect existing orders.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={handleDelete}
                    className="bg-red-500 hover:bg-red-600"
                  >
                    {deleting ? 'Deleting...' : 'Delete Tax'}
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
      </div>

      {/* Main Content */}
      <Tabs defaultValue="overview" className="w-full">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="configuration">Tax Configuration</TabsTrigger>
          <TabsTrigger value="mappings">SKU Mappings</TabsTrigger>
          <TabsTrigger value="audit">Audit Trail</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Tax Rate Card */}
            <Card className="lg:col-span-1">
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Percent className="h-5 w-5 text-blue-600" />
                  Tax Rate
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-center">
                  <div className="text-4xl font-bold text-blue-600 mb-2">
                    {tax.BaseTaxRate}%
                  </div>
                  <Badge variant="outline">
                    {tax.TaxCalculationType || 'Percentage'}
                  </Badge>
                </div>
              </CardContent>
            </Card>

            {/* Basic Information */}
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <FileText className="h-5 w-5" />
                  Basic Information
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-gray-600">Tax Code</p>
                    <p className="font-mono font-medium">{tax.Code}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Applicable At</p>
                    <p className="font-medium">{tax.ApplicableAt || 'Item'}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Tax on Tax</p>
                    <Badge variant={tax.IsTaxOnTaxApplicable ? 'default' : 'secondary'}>
                      {tax.IsTaxOnTaxApplicable ? 'Enabled' : 'Disabled'}
                    </Badge>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Status</p>
                    <Badge variant={tax.Status === 'Active' ? 'default' : 'secondary'}>
                      {tax.Status}
                    </Badge>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Validity Period */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Calendar className="h-5 w-5" />
                Validity Period
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <p className="text-sm text-gray-600">Valid From</p>
                  <p className="text-lg font-medium">{formatDate(tax.ValidFrom)}</p>
                </div>
                <div className="space-y-2">
                  <p className="text-sm text-gray-600">Valid Until</p>
                  <p className="text-lg font-medium">{formatDate(tax.ValidUpto)}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="configuration" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Percent className="h-5 w-5" />
                Tax Configuration Details
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <div className="space-y-2">
                  <p className="text-sm font-medium text-gray-600">Base Tax Rate</p>
                  <div className="text-2xl font-bold text-blue-600">
                    {tax.BaseTaxRate}%
                  </div>
                </div>
                <div className="space-y-2">
                  <p className="text-sm font-medium text-gray-600">Calculation Type</p>
                  <Badge variant="outline" className="text-sm">
                    {tax.TaxCalculationType || 'Percentage'}
                  </Badge>
                </div>
                <div className="space-y-2">
                  <p className="text-sm font-medium text-gray-600">Application Level</p>
                  <p className="text-lg font-medium">{tax.ApplicableAt || 'Item'}</p>
                </div>
                <div className="space-y-2">
                  <p className="text-sm font-medium text-gray-600">Compound Tax</p>
                  <Badge variant={tax.IsTaxOnTaxApplicable ? 'default' : 'secondary'}>
                    {tax.IsTaxOnTaxApplicable ? 'Yes - Tax on Tax Enabled' : 'No - Simple Tax'}
                  </Badge>
                </div>
                <div className="space-y-2">
                  <p className="text-sm font-medium text-gray-600">Current Status</p>
                  <Badge variant={tax.Status === 'Active' ? 'default' : 'secondary'}>
                    {tax.Status}
                  </Badge>
                </div>
              </div>

              {tax.IsTaxOnTaxApplicable && (
                <div className="border-t pt-6">
                  <h3 className="font-medium text-gray-900 mb-2">Compound Tax Information</h3>
                  <p className="text-sm text-gray-600">
                    This tax is configured as a compound tax, meaning it can be calculated on top of other taxes. 
                    This is commonly used for scenarios like tax-on-tax calculations where one tax is applied 
                    to the sum of the base amount plus other applicable taxes.
                  </p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="mappings" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Building2 className="h-5 w-5" />
                SKU Mappings
              </CardTitle>
            </CardHeader>
            <CardContent>
              {taxMaster.TaxSKUMapList && taxMaster.TaxSKUMapList.length > 0 ? (
                <div>
                  <p className="text-sm text-gray-600 mb-4">
                    This tax is mapped to <span className="font-semibold">{taxMaster.TaxSKUMapList.length}</span> SKUs
                  </p>
                  <div className="space-y-2">
                    {taxMaster.TaxSKUMapList.map((mapping, index) => (
                      <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                        <div>
                          <p className="font-mono text-sm">{mapping.SKUUID}</p>
                        </div>
                        <Badge variant="outline" className="text-xs">
                          Mapped
                        </Badge>
                      </div>
                    ))}
                  </div>
                </div>
              ) : (
                <div className="text-center py-8">
                  <Building2 className="h-12 w-12 mx-auto mb-4 text-gray-400" />
                  <p className="text-gray-600">No SKU mappings found</p>
                  <p className="text-sm text-gray-500 mt-1">
                    This tax is not currently mapped to any specific SKUs
                  </p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="audit" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Audit Trail
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                {/* Creation Information */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <h3 className="font-medium text-gray-900">Creation Information</h3>
                    <div className="space-y-3">
                      <div className="flex items-center gap-3">
                        <User className="h-4 w-4 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-600">Created By</p>
                          <p className="font-medium">{tax.CreatedBy || 'System'}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <Clock className="h-4 w-4 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-600">Created On</p>
                          <p className="font-medium">{formatDateTime(tax.CreatedTime)}</p>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-4">
                    <h3 className="font-medium text-gray-900">Last Modification</h3>
                    <div className="space-y-3">
                      <div className="flex items-center gap-3">
                        <User className="h-4 w-4 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-600">Modified By</p>
                          <p className="font-medium">{tax.ModifiedBy || 'System'}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <Clock className="h-4 w-4 text-gray-400" />
                        <div>
                          <p className="text-sm text-gray-600">Modified On</p>
                          <p className="font-medium">{formatDateTime(tax.ModifiedTime)}</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Server Timestamps */}
                <div className="border-t pt-6">
                  <h3 className="font-medium text-gray-900 mb-4">Server Timestamps</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <p className="text-sm text-gray-600">Server Add Time</p>
                      <p className="font-mono text-sm">{formatDateTime(tax.ServerAddTime)}</p>
                    </div>
                    <div>
                      <p className="text-sm text-gray-600">Server Modified Time</p>
                      <p className="font-mono text-sm">{formatDateTime(tax.ServerModifiedTime)}</p>
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default TaxDetailsPage;