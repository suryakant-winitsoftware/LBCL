import React from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  ComposedChart
} from 'recharts'
import { SalesTrendData } from '@/types'
import { formatCurrency, formatNumber, formatDateShort } from '@/lib/utils'

interface SalesChartProps {
  data: SalesTrendData[]
  loading?: boolean
  chartType?: 'line' | 'bar' | 'area' | 'composed'
}

const CustomTooltip = ({ active, payload, label }: any) => {
  if (active && payload && payload.length) {
    return (
      <div className="bg-white border rounded-lg shadow-lg p-3">
        <p className="text-sm font-medium">{formatDateShort(label)}</p>
        {payload.map((entry: any, index: number) => (
          <p key={index} className="text-sm" style={{ color: entry.color }}>
            {entry.name}: {
              entry.dataKey === 'sales'
                ? formatCurrency(entry.value)
                : formatNumber(entry.value)
            }
          </p>
        ))}
      </div>
    )
  }
  return null
}

export const SalesChart: React.FC<SalesChartProps> = ({
  data,
  loading = false,
  chartType = 'composed'
}) => {
  const [timeRange, setTimeRange] = React.useState<'7d' | '30d' | '90d'>('30d')

  const filteredData = React.useMemo(() => {
    const days = timeRange === '7d' ? 7 : timeRange === '30d' ? 30 : 90
    return data.slice(-days)
  }, [data, timeRange])

  const chartColors = {
    sales: '#3b82f6',
    orders: '#10b981',
    customers: '#f59e0b'
  }

  if (loading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Sales Trend</CardTitle>
          <CardDescription>Daily sales performance</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="h-[350px] bg-gray-200 animate-pulse rounded" />
        </CardContent>
      </Card>
    )
  }

  const renderChart = () => {
    const commonProps = {
      data: filteredData,
      margin: { top: 5, right: 30, left: 20, bottom: 5 }
    }

    switch (chartType) {
      case 'line':
        return (
          <LineChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-gray-200" />
            <XAxis
              dataKey="date"
              tickFormatter={(value) => formatDateShort(value)}
              className="text-xs"
            />
            <YAxis
              yAxisId="left"
              tickFormatter={(value) => `$${(value / 1000).toFixed(0)}k`}
              className="text-xs"
            />
            <YAxis
              yAxisId="right"
              orientation="right"
              className="text-xs"
            />
            <Tooltip content={<CustomTooltip />} />
            <Legend />
            <Line
              yAxisId="left"
              type="monotone"
              dataKey="sales"
              stroke={chartColors.sales}
              strokeWidth={2}
              dot={false}
              name="Sales"
            />
            <Line
              yAxisId="right"
              type="monotone"
              dataKey="orders"
              stroke={chartColors.orders}
              strokeWidth={2}
              dot={false}
              name="Orders"
            />
          </LineChart>
        )

      case 'bar':
        return (
          <BarChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-gray-200" />
            <XAxis
              dataKey="date"
              tickFormatter={(value) => formatDateShort(value)}
              className="text-xs"
            />
            <YAxis
              tickFormatter={(value) => `$${(value / 1000).toFixed(0)}k`}
              className="text-xs"
            />
            <Tooltip content={<CustomTooltip />} />
            <Legend />
            <Bar
              dataKey="sales"
              fill={chartColors.sales}
              name="Sales"
              radius={[4, 4, 0, 0]}
            />
          </BarChart>
        )

      case 'area':
        return (
          <AreaChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-gray-200" />
            <XAxis
              dataKey="date"
              tickFormatter={(value) => formatDateShort(value)}
              className="text-xs"
            />
            <YAxis
              tickFormatter={(value) => `$${(value / 1000).toFixed(0)}k`}
              className="text-xs"
            />
            <Tooltip content={<CustomTooltip />} />
            <Legend />
            <Area
              type="monotone"
              dataKey="sales"
              stroke={chartColors.sales}
              fill={chartColors.sales}
              fillOpacity={0.3}
              strokeWidth={2}
              name="Sales"
            />
          </AreaChart>
        )

      case 'composed':
      default:
        return (
          <ComposedChart {...commonProps}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-gray-200" />
            <XAxis
              dataKey="date"
              tickFormatter={(value) => formatDateShort(value)}
              className="text-xs"
            />
            <YAxis
              yAxisId="left"
              tickFormatter={(value) => `$${(value / 1000).toFixed(0)}k`}
              className="text-xs"
            />
            <YAxis
              yAxisId="right"
              orientation="right"
              className="text-xs"
            />
            <Tooltip content={<CustomTooltip />} />
            <Legend />
            <Bar
              yAxisId="left"
              dataKey="sales"
              fill={chartColors.sales}
              name="Sales"
              radius={[4, 4, 0, 0]}
              fillOpacity={0.8}
            />
            <Line
              yAxisId="right"
              type="monotone"
              dataKey="orders"
              stroke={chartColors.orders}
              strokeWidth={2}
              dot={false}
              name="Orders"
            />
            <Line
              yAxisId="right"
              type="monotone"
              dataKey="customers"
              stroke={chartColors.customers}
              strokeWidth={2}
              dot={false}
              name="Customers"
            />
          </ComposedChart>
        )
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Sales Trend</CardTitle>
            <CardDescription>Daily sales, orders, and customer metrics</CardDescription>
          </div>
          <div className="flex gap-2">
            <Button
              variant={timeRange === '7d' ? 'default' : 'outline'}
              size="sm"
              onClick={() => setTimeRange('7d')}
            >
              7D
            </Button>
            <Button
              variant={timeRange === '30d' ? 'default' : 'outline'}
              size="sm"
              onClick={() => setTimeRange('30d')}
            >
              30D
            </Button>
            <Button
              variant={timeRange === '90d' ? 'default' : 'outline'}
              size="sm"
              onClick={() => setTimeRange('90d')}
            >
              90D
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={350}>
          {renderChart()}
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}