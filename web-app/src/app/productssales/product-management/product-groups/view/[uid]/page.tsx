'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { 
  ArrowLeft, Package, Hash, Building, Layers, 
  Calendar, User, Clock
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Separator } from '@/components/ui/separator';
import { toast } from 'sonner';
import { skuGroupService, SKUGroup, SKUGroupType } from '@/services/sku/sku-group.service';
import { formatDateToDayMonthYear } from '@/utils/date-formatter';

export default function ViewProductGroupPage() {
  const router = useRouter();
  const params = useParams();
  const groupUID = params.uid as string;

  const [group, setGroup] = useState<SKUGroup | null>(null);
  const [groupType, setGroupType] = useState<SKUGroupType | null>(null);
  const [parentGroup, setParentGroup] = useState<SKUGroup | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (groupUID) {
      fetchGroupDetails();
    }
  }, [groupUID]);

  const fetchGroupDetails = async () => {
    setLoading(true);
    try {
      // Fetch the group details
      const groupData = await skuGroupService.getSKUGroupByUID(groupUID);
      setGroup(groupData);

      // Fetch the group type details if available
      if (groupData.SKUGroupTypeUID) {
        try {
          const typeData = await skuGroupService.getSKUGroupTypeByUID(groupData.SKUGroupTypeUID);
          setGroupType(typeData);
        } catch (error) {
          console.warn('Could not fetch group type:', error);
        }
      }

      // Fetch parent group if available
      if (groupData.ParentUID) {
        try {
          const parentData = await skuGroupService.getSKUGroupByUID(groupData.ParentUID);
          setParentGroup(parentData);
        } catch (error) {
          console.warn('Could not fetch parent group:', error);
        }
      }
    } catch (error) {
      console.error('Error fetching group details:', error);
      toast.error('Failed to fetch product group details');
    } finally {
      setLoading(false);
    }
  };


  const getGroupTypeBadgeColor = (type: string) => {
    switch (type?.toLowerCase()) {
      case 'brand':
        return 'bg-purple-100 text-purple-800';
      case 'category':
        return 'bg-blue-100 text-blue-800';
      case 'subcategory':
        return 'bg-green-100 text-green-800';
      case 'divi':
      case 'division':
        return 'bg-orange-100 text-orange-800';
      case 'subdivi':
      case 'subdivision':
        return 'bg-yellow-100 text-yellow-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getLevelBadgeColor = (level: number) => {
    const colors = [
      'bg-red-100 text-red-800',
      'bg-orange-100 text-orange-800',
      'bg-yellow-100 text-yellow-800',
      'bg-green-100 text-green-800',
      'bg-blue-100 text-blue-800',
      'bg-purple-100 text-purple-800'
    ];
    return colors[level - 1] || 'bg-gray-100 text-gray-800';
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

  if (!group) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center py-8">
          <h2 className="text-2xl font-bold mb-2">Product Group Not Found</h2>
          <p className="text-muted-foreground mb-4">The requested product group could not be found.</p>
          <Button onClick={() => router.push('/productssales/product-management/product-groups')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Product Groups
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.push('/productssales/product-management/product-groups')}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Product Groups
        </Button>
      </div>

      {/* Main Information Card */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-2xl flex items-center gap-3">
              <Package className="h-6 w-6" />
              {group.Name}
            </CardTitle>
            <div className="flex gap-2">
              <Badge className={getGroupTypeBadgeColor(group.SKUGroupTypeUID)}>
                {group.SKUGroupTypeUID}
              </Badge>
              <Badge className={getLevelBadgeColor(group.ItemLevel)}>
                Level {group.ItemLevel}
              </Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-1">
              <p className="text-sm text-muted-foreground flex items-center gap-1">
                <Hash className="h-3 w-3" />
                Code
              </p>
              <p className="font-semibold">{group.Code}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm text-muted-foreground flex items-center gap-1">
                <Building className="h-3 w-3" />
                Supplier Organization
              </p>
              <p className="font-semibold">{group.SupplierOrgUID}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Hierarchy Information */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Group Type Details */}
        {groupType && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Layers className="h-5 w-5" />
                Group Type Details
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-sm text-muted-foreground">Type Name</p>
                <p className="font-semibold">{groupType.Name}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Type Code</p>
                <p className="font-mono text-sm">{groupType.Code}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Organization</p>
                <p className="font-semibold">{groupType.OrgUID}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Available for Filter</p>
                <Badge variant={groupType.AvailableForFilter ? 'default' : 'secondary'}>
                  {groupType.AvailableForFilter ? 'Yes' : 'No'}
                </Badge>
              </div>
              {groupType.ParentUID && (
                <div>
                  <p className="text-sm text-muted-foreground">Parent Type</p>
                  <p className="font-semibold">{groupType.ParentUID}</p>
                </div>
              )}
            </CardContent>
          </Card>
        )}

        {/* Parent Group Details */}
        {parentGroup ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Layers className="h-5 w-5" />
                Parent Group
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-sm text-muted-foreground">Parent Name</p>
                <p className="font-semibold">{parentGroup.Name}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Parent Code</p>
                <p className="font-mono text-sm">{parentGroup.Code}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Parent Type</p>
                <Badge className={getGroupTypeBadgeColor(parentGroup.SKUGroupTypeUID)}>
                  {parentGroup.SKUGroupTypeUID}
                </Badge>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Parent Level</p>
                <Badge className={getLevelBadgeColor(parentGroup.ItemLevel)}>
                  Level {parentGroup.ItemLevel}
                </Badge>
              </div>
              {parentGroup.UID && (
                <Button 
                  variant="outline" 
                  size="sm"
                  onClick={() => router.push(`/productssales/product-management/product-groups/view/${parentGroup.UID}`)}
                >
                  View Parent Group
                </Button>
              )}
            </CardContent>
          </Card>
        ) : group.ParentUID ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Layers className="h-5 w-5" />
                Parent Group
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <p className="text-sm text-muted-foreground">Parent UID</p>
                <p className="font-mono text-sm">{group.ParentUID}</p>
                <p className="text-xs text-muted-foreground">Parent group details could not be loaded</p>
              </div>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Layers className="h-5 w-5" />
                Parent Group
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-muted-foreground">This is a top-level group with no parent</p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Audit Information */}
      {(group.CreatedBy || group.ModifiedBy) && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5" />
              Audit Information
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {group.CreatedBy && (
                <div className="space-y-3">
                  <div>
                    <p className="text-sm text-muted-foreground flex items-center gap-1">
                      <User className="h-3 w-3" />
                      Created By
                    </p>
                    <p className="font-semibold">{group.CreatedBy}</p>
                  </div>
                  {group.CreatedTime && (
                    <div>
                      <p className="text-sm text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        Created On
                      </p>
                      <p className="text-sm">{formatDateToDayMonthYear(group.CreatedTime)}</p>
                    </div>
                  )}
                </div>
              )}
              
              {group.ModifiedBy && (
                <div className="space-y-3">
                  <div>
                    <p className="text-sm text-muted-foreground flex items-center gap-1">
                      <User className="h-3 w-3" />
                      Last Modified By
                    </p>
                    <p className="font-semibold">{group.ModifiedBy}</p>
                  </div>
                  {group.ModifiedTime && (
                    <div>
                      <p className="text-sm text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        Last Modified On
                      </p>
                      <p className="text-sm">{formatDateToDayMonthYear(group.ModifiedTime)}</p>
                    </div>
                  )}
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}