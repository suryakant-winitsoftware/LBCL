'use client'

import { useState, useEffect } from 'react'
import { useRouter, useParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Skeleton } from '@/components/ui/skeleton'
import { Separator } from '@/components/ui/separator'
import { useToast } from '@/components/ui/use-toast'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'
import currencyService, { Currency, OrgCurrency } from '@/services/currencyService'
import {
  ArrowLeft,
  DollarSign,
  Hash,
  Calendar,
  User,
  Settings,
  Activity,
  Clock,
  Shield,
  Building2,
  Globe,
  TrendingUp,
  Info,
  Edit,
  Trash2,
  RefreshCw
} from 'lucide-react'

export default function ViewCurrencyPage() {
  const router = useRouter()
  const params = useParams()
  const { toast } = useToast()
  const currencyId = params.id as string

  const [loading, setLoading] = useState(true)
  const [currency, setCurrency] = useState<Currency | null>(null)
  const [orgCurrencies, setOrgCurrencies] = useState<OrgCurrency[]>([])
  const [refreshing, setRefreshing] = useState(false)

  useEffect(() => {
    if (currencyId) {
      fetchCurrencyDetails(currencyId)
    }
  }, [currencyId])

  const fetchCurrencyDetails = async (uid: string) => {
    try {
      setLoading(true)
      const currencyData = await currencyService.getCurrencyById(uid)
      setCurrency(currencyData)

      // Try to fetch organization currencies (may return empty)
      try {
        const orgData = await currencyService.getCurrencyListByOrgUID('WINIT')
        setOrgCurrencies(orgData.filter(org => org.CurrencyUID === uid))
      } catch (error) {
        console.log('No organization currency data available')
      }
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch currency details',
        variant: 'destructive'
      })
      console.error('Error fetching currency:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleRefresh = async () => {
    setRefreshing(true)
    await fetchCurrencyDetails(currencyId)
    setRefreshing(false)
    toast({
      title: 'Success',
      description: 'Currency details refreshed'
    })
  }

  const handleEdit = () => {
    router.push(`/administration/currency/currency-management?edit=${currencyId}`)
  }

  const handleDelete = async () => {
    if (confirm('Are you sure you want to delete this currency?')) {
      try {
        await currencyService.deleteCurrency(currencyId)
        toast({
          title: 'Success',
          description: 'Currency deleted successfully'
        })
        router.push('/administration/currency/currency-management')
      } catch (error) {
        toast({
          title: 'Error',
          description: 'Failed to delete currency',
          variant: 'destructive'
        })
      }
    }
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Skeleton className="h-10 w-10" />
            <div>
              <Skeleton className="h-8 w-64 mb-2" />
              <Skeleton className="h-4 w-32" />
            </div>
          </div>
          <Skeleton className="h-10 w-32" />
        </div>
        <div className="grid gap-6 md:grid-cols-3">
          {[1, 2, 3].map(i => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
        <Skeleton className="h-96" />
      </div>
    )
  }

  if (!currency) {
    return (
      <div className="flex flex-col items-center justify-center py-12">
        <Info className="h-12 w-12 text-muted-foreground mb-4" />
        <h2 className="text-xl font-semibold mb-2">Currency Not Found</h2>
        <p className="text-muted-foreground mb-4">
          The requested currency could not be found.
        </p>
        <Button onClick={() => router.push('/administration/currency/currency-management')}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Currency Management
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.push('/administration/currency/currency-management')}
          >
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight flex items-center gap-3">
              {currency.Name}
              <Badge variant="outline" className="text-lg px-3 py-1">
                {currency.Code}
              </Badge>
            </h1>
            <p className="text-muted-foreground mt-1">
              Currency Details and Configuration
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handleRefresh} disabled={refreshing}>
            <RefreshCw className={`h-4 w-4 mr-2 ${refreshing ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Button variant="outline" size="sm" onClick={handleEdit}>
            <Edit className="h-4 w-4 mr-2" />
            Edit
          </Button>
          <Button variant="destructive" size="sm" onClick={handleDelete}>
            <Trash2 className="h-4 w-4 mr-2" />
            Delete
          </Button>
        </div>
      </div>

      {/* Key Metrics Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Currency Symbol
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <DollarSign className="h-5 w-5 text-primary" />
              <span className="text-3xl font-bold">{currency.Symbol}</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Decimal Places
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Hash className="h-5 w-5 text-blue-600" />
              <span className="text-2xl font-bold">{currency.Digits}</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Fraction Name
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Settings className="h-5 w-5 text-green-600" />
              <span className="text-xl font-semibold">
                {currency.FractionName || 'Not Set'}
              </span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Status
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Activity className="h-5 w-5 text-purple-600" />
              {currency.IsPrimary ? (
                <Badge className="bg-green-100 text-green-800">Primary</Badge>
              ) : (
                <Badge variant="outline">Secondary</Badge>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Detailed Information Tabs */}
      <Tabs defaultValue="details" className="w-full">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="details">Details</TabsTrigger>
          <TabsTrigger value="configuration">Configuration</TabsTrigger>
          <TabsTrigger value="organizations">Organizations</TabsTrigger>
          <TabsTrigger value="audit">Audit Info</TabsTrigger>
        </TabsList>

        {/* Details Tab */}
        <TabsContent value="details" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Currency Information</CardTitle>
              <CardDescription>
                Basic information about this currency
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Currency Code
                  </label>
                  <p className="text-lg font-semibold flex items-center gap-2">
                    <Globe className="h-4 w-4 text-muted-foreground" />
                    {currency.Code}
                  </p>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Currency Name
                  </label>
                  <p className="text-lg font-semibold">{currency.Name}</p>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Symbol
                  </label>
                  <p className="text-2xl font-bold">{currency.Symbol}</p>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Unique Identifier
                  </label>
                  <p className="font-mono text-sm bg-muted px-2 py-1 rounded">
                    {currency.UID}
                  </p>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Decimal Places
                  </label>
                  <p className="text-lg">{currency.Digits} digits</p>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Fraction Name
                  </label>
                  <p className="text-lg">
                    {currency.FractionName || 'Not configured'}
                  </p>
                </div>
              </div>

              <Separator />

              <div className="space-y-1">
                <label className="text-sm font-medium text-muted-foreground">
                  Primary Currency
                </label>
                <div className="flex items-center gap-2">
                  <Shield className="h-4 w-4 text-muted-foreground" />
                  {currency.IsPrimary ? (
                    <Badge className="bg-green-100 text-green-800">
                      This is a primary currency
                    </Badge>
                  ) : (
                    <Badge variant="outline">
                      This is a secondary currency
                    </Badge>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Configuration Tab */}
        <TabsContent value="configuration" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Rounding Configuration</CardTitle>
              <CardDescription>
                Configure how amounts are rounded for this currency
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Round Off Minimum Limit
                  </label>
                  <div className="flex items-center gap-2">
                    <TrendingUp className="h-4 w-4 text-muted-foreground" />
                    <p className="text-lg font-semibold">
                      {currency.RoundOffMinLimit?.toFixed(2) || '0.00'}
                    </p>
                  </div>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Round Off Maximum Limit
                  </label>
                  <div className="flex items-center gap-2">
                    <TrendingUp className="h-4 w-4 text-muted-foreground" />
                    <p className="text-lg font-semibold">
                      {currency.RoundOffMaxLimit?.toFixed(2) || '0.00'}
                    </p>
                  </div>
                </div>
              </div>

              <Separator />

              <div className="bg-muted/50 p-4 rounded-lg">
                <h4 className="text-sm font-medium mb-2">Rounding Rules</h4>
                <ul className="text-sm text-muted-foreground space-y-1">
                  <li>• Amounts below {currency.RoundOffMinLimit?.toFixed(2) || '0.00'} will be rounded down</li>
                  <li>• Amounts above {currency.RoundOffMaxLimit?.toFixed(2) || '0.00'} will be rounded up</li>
                  <li>• Currency supports {currency.Digits} decimal places</li>
                  {currency.FractionName && (
                    <li>• Fractional amounts are displayed in {currency.FractionName}</li>
                  )}
                </ul>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Organizations Tab */}
        <TabsContent value="organizations" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Organization Usage</CardTitle>
              <CardDescription>
                Organizations using this currency
              </CardDescription>
            </CardHeader>
            <CardContent>
              {orgCurrencies.length > 0 ? (
                <div className="space-y-4">
                  {orgCurrencies.map((org, index) => (
                    <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                      <div className="flex items-center gap-3">
                        <Building2 className="h-5 w-5 text-muted-foreground" />
                        <div>
                          <p className="font-medium">Organization {org.OrgUID}</p>
                          <p className="text-sm text-muted-foreground">
                            {org.IsPrimary ? 'Primary Currency' : 'Secondary Currency'}
                          </p>
                        </div>
                      </div>
                      {org.IsPrimary && (
                        <Badge className="bg-green-100 text-green-800">Primary</Badge>
                      )}
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8">
                  <Building2 className="h-12 w-12 text-muted-foreground mx-auto mb-3" />
                  <p className="text-muted-foreground">
                    No organizations are currently using this currency
                  </p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Audit Info Tab */}
        <TabsContent value="audit" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Audit Information</CardTitle>
              <CardDescription>
                Track creation and modification history
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Created Date
                  </label>
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    <p className="text-lg">
                      {currency.CreatedTime
                        ? formatDateToDayMonthYear(currency.CreatedTime)
                        : 'Not available'}
                    </p>
                  </div>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Last Modified
                  </label>
                  <div className="flex items-center gap-2">
                    <Clock className="h-4 w-4 text-muted-foreground" />
                    <p className="text-lg">
                      {currency.ServerModifiedTime
                        ? formatDateToDayMonthYear(currency.ServerModifiedTime)
                        : 'Not available'}
                    </p>
                  </div>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    Server Add Time
                  </label>
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    <p className="text-lg">
                      {currency.ServerAddTime
                        ? formatDateToDayMonthYear(currency.ServerAddTime)
                        : 'Not available'}
                    </p>
                  </div>
                </div>

                <div className="space-y-1">
                  <label className="text-sm font-medium text-muted-foreground">
                    System Status
                  </label>
                  <div className="flex items-center gap-2">
                    <Activity className="h-4 w-4 text-muted-foreground" />
                    <Badge variant="outline">
                      SS: {currency.SS || 0}
                    </Badge>
                  </div>
                </div>
              </div>

              <Separator />

              <div className="bg-muted/50 p-4 rounded-lg">
                <h4 className="text-sm font-medium mb-2">System Information</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-muted-foreground">Record ID:</span>
                    <span className="ml-2 font-mono">{currency.Id || 'N/A'}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">UID:</span>
                    <span className="ml-2 font-mono">{currency.UID}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Is Selected:</span>
                    <span className="ml-2">{currency.IsSelected ? 'Yes' : 'No'}</span>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}