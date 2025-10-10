"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import {
  Plus,
  Tag,
  BarChart,
  Calendar,
  DollarSign,
  ShoppingBag,
  TrendingUp,
  TrendingDown,
  Clock,
  CheckCircle,
  XCircle,
  ArrowRight,
  Sparkles,
  Gift,
  PieChart,
  Users,
  Zap,
  Flame,
  ArrowUp,
  ChevronRight,
  AlertTriangle,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { promotionService } from "../../services/promotion.service";
import { IPromotionView } from "../../types/promotion.types";
import {
  getPromotionDisplayType,
  getPromotionStatus,
  groupPromotionsByType,
} from "../../utils/promotionDisplayUtils";

interface DashboardStats {
  totalPromotions: number;
  activePromotions: number;
  scheduledPromotions: number;
  expiredPromotions: number;
  totalDiscountValue: number;
  averageDiscount: number;
  promotionsByType: Record<string, number>;
  upcomingPromotions: IPromotionView[];
  expiringPromotions: IPromotionView[];
}

const formatDate = (
  date: string | undefined | null
): string => {
  if (!date) return "N/A";
  try {
    const parsedDate = new Date(date);
    if (isNaN(parsedDate.getTime())) return "Invalid date";
    return parsedDate.toLocaleDateString();
  } catch {
    return "Invalid date";
  }
};

export default function PromotionReportsPage() {
  const router = useRouter();
  const [stats, setStats] = useState<DashboardStats>({
    totalPromotions: 0,
    activePromotions: 0,
    scheduledPromotions: 0,
    expiredPromotions: 0,
    totalDiscountValue: 0,
    averageDiscount: 0,
    promotionsByType: {},
    upcomingPromotions: [],
    expiringPromotions: [],
  });
  const [recentPromotions, setRecentPromotions] = useState<IPromotionView[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await promotionService.getPromotionDetails(1, 20);

      if (response.success) {
        let promotions: IPromotionView[] = [];

        if (response.data) {
          if (
            response.data.PagedData &&
            Array.isArray(response.data.PagedData)
          ) {
            promotions = response.data.PagedData;
          } else if (
            response.data.Data?.PagedData &&
            Array.isArray(response.data.Data.PagedData)
          ) {
            promotions = response.data.Data.PagedData;
          } else if (response.data.Data && Array.isArray(response.data.Data)) {
            promotions = response.data.Data;
          } else if (Array.isArray(response.data)) {
            promotions = response.data;
          }
        }

        // Calculate statistics
        const now = new Date();
        const activePromotions = promotions.filter(p => {
          const status = getPromotionStatus(p);
          return status.status === 'active';
        });
        
        const scheduledPromotions = promotions.filter(p => {
          const status = getPromotionStatus(p);
          return status.status === 'scheduled';
        });
        
        const expiredPromotions = promotions.filter(p => {
          const status = getPromotionStatus(p);
          return status.status === 'expired';
        });

        // Get upcoming promotions (starting in next 7 days)
        const nextWeek = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
        const upcomingPromotions = promotions.filter(p => {
          const validFrom = new Date(p.ValidFrom);
          return validFrom > now && validFrom <= nextWeek;
        }).slice(0, 5);

        // Get expiring promotions (ending in next 7 days)
        const expiringPromotions = promotions.filter(p => {
          const validUpto = new Date(p.ValidUpto);
          return validUpto > now && validUpto <= nextWeek && p.IsActive;
        }).slice(0, 5);

        // Group by type
        const promotionsByType = groupPromotionsByType(promotions);
        const typeStats: Record<string, number> = {};
        Object.keys(promotionsByType).forEach(type => {
          typeStats[type] = promotionsByType[type].length;
        });

        setStats({
          totalPromotions: promotions.length,
          activePromotions: activePromotions.length,
          scheduledPromotions: scheduledPromotions.length,
          expiredPromotions: expiredPromotions.length,
          totalDiscountValue: 0, // Would need to calculate from promotion details
          averageDiscount: 0, // Would need to calculate from promotion details
          promotionsByType: typeStats,
          upcomingPromotions,
          expiringPromotions,
        });

        // Set recent promotions (last 10)
        const sortedByDate = [...promotions].sort((a, b) => {
          const dateA = new Date(a.CreatedTime || a.ModifiedTime || '');
          const dateB = new Date(b.CreatedTime || b.ModifiedTime || '');
          return dateB.getTime() - dateA.getTime();
        });
        setRecentPromotions(sortedByDate.slice(0, 10));

      } else {
        throw new Error(response.error || "Failed to fetch promotions");
      }
    } catch (err: any) {
      setError(err.message || "Failed to fetch dashboard data");
    } finally {
      setLoading(false);
    }
  };

  const handleCreateNew = () => {
    router.push("/updatedfeatures/promotion-management/promotions/manage?create=true");
  };

  const handleViewAllPromotions = () => {
    router.push("/updatedfeatures/promotion-management/promotions/manage");
  };

  const handleViewPromotion = (promotionId: string) => {
    router.push(`/updatedfeatures/promotion-management/promotions/${promotionId}`);
  };

  const getStatusBadge = (promotion: IPromotionView) => {
    const status = getPromotionStatus(promotion);
    const variants: Record<string, "default" | "secondary" | "destructive" | "outline"> = {
      green: "default",
      blue: "secondary", 
      red: "destructive",
      gray: "outline"
    };
    
    return (
      <Badge variant={variants[status.color]} className="text-xs">
        {status.label}
      </Badge>
    );
  };

  const StatCardSkeleton = () => (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-center">
          <div className="w-12 h-12 bg-gray-200 rounded-lg animate-pulse"></div>
          <div className="ml-4 flex-1 space-y-2">
            <div className="h-3 bg-gray-200 rounded animate-pulse"></div>
            <div className="h-6 bg-gray-200 rounded animate-pulse w-20"></div>
          </div>
        </div>
      </CardContent>
    </Card>
  );

  const PromotionItemSkeleton = () => (
    <div className="flex items-center justify-between p-3 border rounded-lg">
      <div className="flex-1 space-y-2">
        <div className="h-4 bg-gray-200 rounded animate-pulse"></div>
        <div className="h-3 bg-gray-200 rounded animate-pulse w-32"></div>
        <div className="h-3 bg-gray-200 rounded animate-pulse w-24"></div>
      </div>
      <div className="flex items-center space-x-2">
        <div className="w-16 h-5 bg-gray-200 rounded animate-pulse"></div>
        <div className="w-4 h-4 bg-gray-200 rounded animate-pulse"></div>
      </div>
    </div>
  );

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        {/* Header Skeleton */}
        <div className="flex justify-between items-center">
          <div className="space-y-2">
            <div className="h-8 bg-gray-200 rounded animate-pulse w-80"></div>
            <div className="h-4 bg-gray-200 rounded animate-pulse w-96"></div>
          </div>
          <div className="flex items-center space-x-3">
            <div className="w-40 h-10 bg-gray-200 rounded animate-pulse"></div>
            <div className="w-44 h-10 bg-gray-200 rounded animate-pulse"></div>
          </div>
        </div>

        {/* Stats Skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <StatCardSkeleton />
          <StatCardSkeleton />
          <StatCardSkeleton />
          <StatCardSkeleton />
        </div>

        {/* Charts Section Skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <div className="h-5 bg-gray-200 rounded animate-pulse w-48"></div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {[1,2,3,4,5].map(i => (
                  <div key={i} className="flex items-center justify-between">
                    <div className="flex-1 space-y-1">
                      <div className="h-3 bg-gray-200 rounded animate-pulse w-24"></div>
                      <div className="h-2 bg-gray-200 rounded animate-pulse"></div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader>
              <div className="h-5 bg-gray-200 rounded animate-pulse w-32"></div>
            </CardHeader>
            <CardContent className="space-y-3">
              {[1,2,3].map(i => (
                <div key={i} className="w-full h-12 bg-gray-200 rounded animate-pulse"></div>
              ))}
            </CardContent>
          </Card>
        </div>

        {/* Promotions Lists Skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <div className="h-5 bg-gray-200 rounded animate-pulse w-40"></div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {[1,2,3].map(i => <PromotionItemSkeleton key={i} />)}
              </div>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader>
              <div className="h-5 bg-gray-200 rounded animate-pulse w-32"></div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {[1,2,3].map(i => <PromotionItemSkeleton key={i} />)}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Recent Activity Skeleton */}
        <Card>
          <CardHeader>
            <div className="h-5 bg-gray-200 rounded animate-pulse w-40"></div>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {[1,2,3,4,5].map(i => (
                <div key={i} className="flex items-center justify-between p-4 border rounded-lg">
                  <div className="flex items-center space-x-4">
                    <div className="w-9 h-9 bg-gray-200 rounded-lg animate-pulse"></div>
                    <div className="space-y-2">
                      <div className="h-4 bg-gray-200 rounded animate-pulse w-32"></div>
                      <div className="h-3 bg-gray-200 rounded animate-pulse w-48"></div>
                    </div>
                  </div>
                  <div className="flex items-center space-x-3">
                    <div className="space-y-1">
                      <div className="h-3 bg-gray-200 rounded animate-pulse w-24"></div>
                      <div className="h-3 bg-gray-200 rounded animate-pulse w-16"></div>
                    </div>
                    <div className="w-16 h-5 bg-gray-200 rounded animate-pulse"></div>
                    <div className="w-4 h-4 bg-gray-200 rounded animate-pulse"></div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="border-b border-gray-200 pb-6">
        <div className="flex justify-between items-center">
          <div>
            <div className="flex items-center mb-2">
              <BarChart className="h-6 w-6 mr-3 text-gray-600" />
              <h1 className="text-2xl font-semibold text-gray-900">Promotion Analytics</h1>
            </div>
            <p className="text-gray-600 max-w-2xl">
              Comprehensive insights and performance metrics for your promotional campaigns
            </p>
          </div>
          <div className="flex items-center space-x-3">
            <Button 
              variant="outline" 
              onClick={handleViewAllPromotions}
            >
              <ShoppingBag className="h-4 w-4 mr-2" />
              Manage Promotions
            </Button>
            <Button onClick={handleCreateNew}>
              <Plus className="h-4 w-4 mr-2" />
              Create New Promotion
            </Button>
          </div>
        </div>
      </div>

      {/* Key Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="p-2 bg-gray-100 rounded-lg">
                <Tag className="h-5 w-5 text-gray-600" />
              </div>
              <div className="ml-4 flex-1">
                <p className="text-sm font-medium text-gray-600">Total Promotions</p>
                <div className="flex items-center mt-1">
                  <p className="text-2xl font-bold text-gray-900">{stats.totalPromotions}</p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="p-2 bg-green-100 rounded-lg">
                <CheckCircle className="h-5 w-5 text-green-600" />
              </div>
              <div className="ml-4 flex-1">
                <p className="text-sm font-medium text-gray-600">Active</p>
                <div className="flex items-center mt-1">
                  <p className="text-2xl font-bold text-green-600">{stats.activePromotions}</p>
                  <span className="ml-2 text-xs text-gray-500">
                    ({stats.totalPromotions > 0 
                      ? Math.round((stats.activePromotions / stats.totalPromotions) * 100)
                      : 0
                    }%)
                  </span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="p-2 bg-blue-100 rounded-lg">
                <Clock className="h-5 w-5 text-blue-600" />
              </div>
              <div className="ml-4 flex-1">
                <p className="text-sm font-medium text-gray-600">Scheduled</p>
                <div className="flex items-center mt-1">
                  <p className="text-2xl font-bold text-blue-600">{stats.scheduledPromotions}</p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="p-2 bg-gray-100 rounded-lg">
                <XCircle className="h-5 w-5 text-gray-600" />
              </div>
              <div className="ml-4 flex-1">
                <p className="text-sm font-medium text-gray-600">Expired</p>
                <div className="flex items-center mt-1">
                  <p className="text-2xl font-bold text-gray-600">{stats.expiredPromotions}</p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Promotion Types Breakdown */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <PieChart className="h-5 w-5 mr-2 text-gray-600" />
              Promotion Types Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            {Object.keys(stats.promotionsByType).length > 0 ? (
              <div className="space-y-4">
                {Object.entries(stats.promotionsByType)
                  .sort(([,a], [,b]) => b - a)
                  .slice(0, 5)
                  .map(([type, count]) => {
                    const percentage = stats.totalPromotions > 0 
                      ? Math.round((count / stats.totalPromotions) * 100)
                      : 0;
                    return (
                      <div key={type}>
                        <div className="flex items-center justify-between mb-1">
                          <span className="text-sm font-medium text-gray-700">{type}</span>
                          <span className="text-sm text-gray-500">{count} ({percentage}%)</span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div 
                            className="bg-gray-600 h-2 rounded-full"
                            style={{ width: `${percentage}%` }}
                          ></div>
                        </div>
                      </div>
                    );
                  })}
              </div>
            ) : (
              <div className="text-center py-8">
                <PieChart className="mx-auto h-12 w-12 text-gray-300" />
                <p className="text-gray-500 text-sm mt-2">No promotion data available</p>
              </div>
            )}
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Upcoming Promotions */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                <div className="flex items-center">
                  <Calendar className="h-5 w-5 mr-2 text-gray-600" />
                  Upcoming Promotions
                </div>
                <Badge variant="secondary">{stats.upcomingPromotions.length}</Badge>
              </CardTitle>
            </CardHeader>
            <CardContent>
              {stats.upcomingPromotions.length > 0 ? (
                <div className="space-y-3">
                  {stats.upcomingPromotions.map((promotion) => (
                    <div 
                      key={promotion.UID} 
                      className="flex items-center justify-between p-3 border rounded-lg hover:bg-gray-50 cursor-pointer"
                      onClick={() => handleViewPromotion(promotion.UID!)}
                    >
                      <div className="flex-1">
                        <h4 className="font-medium text-gray-900">{promotion.Name}</h4>
                        <p className="text-sm text-gray-500">
                          Starts: {formatDate(promotion.ValidFrom)}
                        </p>
                        <p className="text-xs text-gray-400">
                          {getPromotionDisplayType(promotion)}
                        </p>
                      </div>
                      <div className="flex items-center space-x-2">
                        {getStatusBadge(promotion)}
                        <ChevronRight className="h-4 w-4 text-gray-400" />
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8">
                  <Calendar className="mx-auto h-12 w-12 text-gray-300" />
                  <p className="mt-2 text-sm text-gray-500">No upcoming promotions</p>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Expiring Soon */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                <div className="flex items-center">
                  <AlertTriangle className="h-5 w-5 mr-2 text-gray-600" />
                  Expiring Soon
                </div>
                <Badge variant="secondary">{stats.expiringPromotions.length}</Badge>
              </CardTitle>
            </CardHeader>
            <CardContent>
              {stats.expiringPromotions.length > 0 ? (
                <div className="space-y-3">
                  {stats.expiringPromotions.map((promotion) => (
                    <div 
                      key={promotion.UID} 
                      className="flex items-center justify-between p-3 border rounded-lg hover:bg-gray-50 cursor-pointer"
                      onClick={() => handleViewPromotion(promotion.UID!)}
                    >
                      <div className="flex-1">
                        <h4 className="font-medium text-gray-900">{promotion.Name}</h4>
                        <p className="text-sm text-gray-500">
                          Expires: {formatDate(promotion.ValidUpto)}
                        </p>
                        <p className="text-xs text-gray-400">
                          {getPromotionDisplayType(promotion)}
                        </p>
                      </div>
                      <div className="flex items-center space-x-2">
                        {getStatusBadge(promotion)}
                        <ChevronRight className="h-4 w-4 text-gray-400" />
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8">
                  <CheckCircle className="mx-auto h-12 w-12 text-green-300" />
                  <p className="mt-2 text-sm text-gray-500">No promotions expiring soon</p>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>


      {/* Recent Activity */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <div className="flex items-center">
              <BarChart className="h-5 w-5 mr-2 text-green-600" />
              Recent Promotions
            </div>
            <Button variant="ghost" size="sm" onClick={handleViewAllPromotions}>
              View All
              <ArrowRight className="ml-1 h-4 w-4" />
            </Button>
          </CardTitle>
        </CardHeader>
        <CardContent>
          {recentPromotions.length > 0 ? (
            <div className="space-y-3">
              {recentPromotions.slice(0, 5).map((promotion) => (
                <div 
                  key={promotion.UID} 
                  className="flex items-center justify-between p-4 border rounded-lg hover:bg-gray-50 cursor-pointer"
                  onClick={() => handleViewPromotion(promotion.UID!)}
                >
                  <div className="flex items-center space-x-4">
                    <div className="p-2 bg-blue-100 rounded-lg">
                      <Gift className="h-5 w-5 text-blue-600" />
                    </div>
                    <div>
                      <h4 className="font-medium text-gray-900">{promotion.Name}</h4>
                      <div className="flex items-center space-x-2 mt-1">
                        <span className="text-sm text-gray-500">{promotion.Code}</span>
                        <span className="text-gray-300">•</span>
                        <span className="text-sm text-gray-500">{getPromotionDisplayType(promotion)}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center space-x-3">
                    <div className="text-right">
                      <p className="text-sm text-gray-500">
                        {formatDate(promotion.ValidFrom)} - {formatDate(promotion.ValidUpto)}
                      </p>
                      <p className="text-xs text-gray-400">Priority: {promotion.Priority || "N/A"}</p>
                    </div>
                    {getStatusBadge(promotion)}
                    <ChevronRight className="h-4 w-4 text-gray-400" />
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <Tag className="mx-auto h-12 w-12 text-gray-300" />
              <p className="mt-2 text-sm text-gray-500">No promotions found</p>
              <Button className="mt-4" onClick={handleCreateNew}>
                <Plus className="mr-2 h-4 w-4" />
                Create Your First Promotion
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {error && (
        <Card>
          <CardContent className="py-8 text-center">
            <AlertTriangle className="mx-auto h-12 w-12 text-red-500 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Error Loading Dashboard</h3>
            <p className="text-gray-600">{error}</p>
            <Button className="mt-4" onClick={fetchDashboardData}>
              Try Again
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
}