'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { 
  ArrowLeft, Layers, Hash, Building, 
  Calendar, User, Clock, Shield, Filter
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Separator } from '@/components/ui/separator';
import { toast } from 'sonner';
import { skuGroupService, SKUGroupType, SKUGroup } from '@/services/sku/sku-group.service';
import { formatDateToDayMonthYear } from '@/utils/date-formatter';

export default function ViewGroupTypePage() {
  const router = useRouter();
  const params = useParams();
  const typeUID = params.uid as string;

  const [groupType, setGroupType] = useState<SKUGroupType | null>(null);
  const [parentType, setParentType] = useState<SKUGroupType | null>(null);
  const [childTypes, setChildTypes] = useState<SKUGroupType[]>([]);
  const [relatedGroups, setRelatedGroups] = useState<SKUGroup[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (typeUID) {
      fetchGroupTypeDetails();
    }
  }, [typeUID]);

  const fetchGroupTypeDetails = async () => {
    setLoading(true);
    try {
      // Fetch the group type details
      const typeData = await skuGroupService.getSKUGroupTypeByUID(typeUID);
      setGroupType(typeData);

      // Fetch parent type if available
      if (typeData.ParentUID) {
        try {
          const parentData = await skuGroupService.getSKUGroupTypeByUID(typeData.ParentUID);
          setParentType(parentData);
        } catch (error) {
          console.warn('Could not fetch parent type:', error);
        }
      }

      // Fetch all group types to find children
      try {
        const allTypesResponse = await skuGroupService.getAllSKUGroupTypes({
          PageNumber: 1,
          PageSize: 1000,
          IsCountRequired: false,
          FilterCriterias: [],
          SortCriterias: []
        });
        
        if (allTypesResponse.Data?.PagedData) {
          // Filter for child types
          const children = allTypesResponse.Data.PagedData.filter(
            type => type.ParentUID === typeUID
          );
          setChildTypes(children);
        }
      } catch (error) {
        console.warn('Could not fetch child types:', error);
      }

      // Fetch groups that use this type
      try {
        const groupsResponse = await skuGroupService.getAllSKUGroups({
          PageNumber: 1,
          PageSize: 10,
          IsCountRequired: false,
          FilterCriterias: [
            { Name: 'SKUGroupTypeUID', Value: typeUID }
          ],
          SortCriterias: []
        });
        
        if (groupsResponse.Data?.PagedData) {
          setRelatedGroups(groupsResponse.Data.PagedData);
        }
      } catch (error) {
        console.warn('Could not fetch related groups:', error);
      }
    } catch (error) {
      console.error('Error fetching group type details:', error);
      toast.error('Failed to fetch group type details');
    } finally {
      setLoading(false);
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

  const getHierarchyName = (level: number) => {
    const names = ['Division', 'SubDivision', 'Category', 'SubCategory', 'Brand'];
    return names[level - 1] || `Level ${level}`;
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

  if (!groupType) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center py-8">
          <h2 className="text-2xl font-bold mb-2">Group Type Not Found</h2>
          <p className="text-muted-foreground mb-4">The requested group type could not be found.</p>
          <Button onClick={() => router.push('/productssales/product-management/group-types')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Group Types
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
          onClick={() => router.push('/productssales/product-management/group-types')}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Group Types
        </Button>
      </div>

      {/* Main Information Card */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-2xl flex items-center gap-3">
              <Layers className="h-6 w-6" />
              {groupType.Name}
            </CardTitle>
            <div className="flex gap-2">
              <Badge className={getLevelBadgeColor(groupType.ItemLevel)}>
                Level {groupType.ItemLevel}
              </Badge>
              <Badge variant="outline">
                {getHierarchyName(groupType.ItemLevel)}
              </Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="space-y-1">
              <p className="text-sm text-muted-foreground flex items-center gap-1">
                <Hash className="h-3 w-3" />
                Code
              </p>
              <p className="font-semibold">{groupType.Code}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm text-muted-foreground flex items-center gap-1">
                <Building className="h-3 w-3" />
                Organization
              </p>
              <p className="font-semibold">{groupType.OrgUID}</p>
            </div>

            <div className="space-y-1">
              <p className="text-sm text-muted-foreground flex items-center gap-1">
                <Filter className="h-3 w-3" />
                Available for Filter
              </p>
              <Badge variant={groupType.AvailableForFilter ? 'default' : 'secondary'}>
                {groupType.AvailableForFilter ? 'Yes' : 'No'}
              </Badge>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Hierarchy Information */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Parent Type */}
        {parentType ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Layers className="h-5 w-5" />
                Parent Type
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-sm text-muted-foreground">Name</p>
                <p className="font-semibold">{parentType.Name}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Code</p>
                <p className="font-mono text-sm">{parentType.Code}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Level</p>
                <Badge className={getLevelBadgeColor(parentType.ItemLevel)}>
                  Level {parentType.ItemLevel} - {getHierarchyName(parentType.ItemLevel)}
                </Badge>
              </div>
              {parentType.UID && (
                <Button 
                  variant="outline" 
                  size="sm"
                  onClick={() => router.push(`/productssales/product-management/group-types/view/${parentType.UID}`)}
                >
                  View Parent Type
                </Button>
              )}
            </CardContent>
          </Card>
        ) : groupType.ParentUID ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Layers className="h-5 w-5" />
                Parent Type
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <p className="text-sm text-muted-foreground">Parent UID</p>
                <p className="font-mono text-sm">{groupType.ParentUID}</p>
                <p className="text-xs text-muted-foreground">Parent type details could not be loaded</p>
              </div>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Layers className="h-5 w-5" />
                Parent Type
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-muted-foreground">This is a root-level type with no parent</p>
            </CardContent>
          </Card>
        )}

        {/* Child Types */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Layers className="h-5 w-5" />
              Child Types
            </CardTitle>
          </CardHeader>
          <CardContent>
            {childTypes.length > 0 ? (
              <div className="space-y-2">
                {childTypes.map((child) => (
                  <div 
                    key={child.UID} 
                    className="flex items-center justify-between p-2 border rounded hover:bg-accent cursor-pointer"
                    onClick={() => router.push(`/productssales/product-management/group-types/view/${child.UID}`)}
                  >
                    <div>
                      <p className="font-medium">{child.Name}</p>
                      <p className="text-sm text-muted-foreground">{child.Code}</p>
                    </div>
                    <Badge className={getLevelBadgeColor(child.ItemLevel)}>
                      Level {child.ItemLevel}
                    </Badge>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-muted-foreground">No child types found</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Related Groups */}
      {relatedGroups.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Groups Using This Type
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
              {relatedGroups.map((group) => (
                <div 
                  key={group.UID} 
                  className="flex items-center justify-between p-3 border rounded hover:bg-accent cursor-pointer"
                  onClick={() => router.push(`/productssales/product-management/product-groups/view/${group.UID}`)}
                >
                  <div>
                    <p className="font-medium">{group.Name}</p>
                    <p className="text-sm text-muted-foreground">{group.Code}</p>
                  </div>
                  <ArrowLeft className="h-4 w-4 rotate-180 text-muted-foreground" />
                </div>
              ))}
            </div>
            {relatedGroups.length === 10 && (
              <p className="text-sm text-muted-foreground mt-2">
                Showing first 10 groups...
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {/* Audit Information */}
      {(groupType.CreatedBy || groupType.ModifiedBy) && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5" />
              Audit Information
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {groupType.CreatedBy && (
                <div className="space-y-3">
                  <div>
                    <p className="text-sm text-muted-foreground flex items-center gap-1">
                      <User className="h-3 w-3" />
                      Created By
                    </p>
                    <p className="font-semibold">{groupType.CreatedBy}</p>
                  </div>
                  {groupType.CreatedTime && (
                    <div>
                      <p className="text-sm text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        Created On
                      </p>
                      <p className="text-sm">{formatDateToDayMonthYear(groupType.CreatedTime)}</p>
                    </div>
                  )}
                </div>
              )}
              
              {groupType.ModifiedBy && (
                <div className="space-y-3">
                  <div>
                    <p className="text-sm text-muted-foreground flex items-center gap-1">
                      <User className="h-3 w-3" />
                      Last Modified By
                    </p>
                    <p className="font-semibold">{groupType.ModifiedBy}</p>
                  </div>
                  {groupType.ModifiedTime && (
                    <div>
                      <p className="text-sm text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        Last Modified On
                      </p>
                      <p className="text-sm">{formatDateToDayMonthYear(groupType.ModifiedTime)}</p>
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