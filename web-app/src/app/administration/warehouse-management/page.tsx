'use client'

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Package, ClipboardList, ArrowLeftRight, BarChart3, ArrowRight, Warehouse } from 'lucide-react'
import { useRouter } from 'next/navigation'

export default function WarehouseManagementPage() {
  const router = useRouter()

  const modules = [
    {
      title: 'Warehouses',
      description: 'Manage warehouse locations, details, and configurations',
      icon: Warehouse,
      color: 'indigo',
      path: '/administration/warehouse-management/warehouses',
      features: [
        'Create and manage warehouses',
        'Update warehouse details',
        'View warehouse locations',
        'Export warehouse data'
      ]
    },
    {
      title: 'Stock Requests',
      description: 'Manage warehouse stock requests, load/unload operations, and transfers',
      icon: Package,
      color: 'blue',
      path: '/administration/warehouse-management/stock-requests',
      features: [
        'Create and manage stock requests',
        'Track request status and approvals',
        'View request history',
        'Export request data'
      ]
    },
    {
      title: 'Stock Summary',
      description: 'View real-time warehouse stock levels and inventory',
      icon: BarChart3,
      color: 'green',
      path: '/administration/warehouse-management/stock-summary',
      features: [
        'Real-time stock levels',
        'Available vs reserved stock',
        'Stock in transit tracking',
        'Export stock reports'
      ]
    },
    {
      title: 'Stock Audits',
      description: 'Conduct and manage warehouse stock audits',
      icon: ClipboardList,
      color: 'purple',
      path: '/administration/warehouse-management/stock-audits',
      features: [
        'Physical stock audits',
        'Audit findings management',
        'Discrepancy tracking',
        'Audit history'
      ],
      comingSoon: true
    },
    {
      title: 'Stock Conversions',
      description: 'Manage stock UOM conversions and packaging changes',
      icon: ArrowLeftRight,
      color: 'orange',
      path: '/administration/warehouse-management/stock-conversions',
      features: [
        'UOM conversions',
        'Packaging conversions',
        'Conversion history',
        'Conversion reports'
      ],
      comingSoon: true
    },
  ]

  const colorClasses: Record<string, { bg: string; text: string; border: string }> = {
    indigo: { bg: 'bg-indigo-100', text: 'text-indigo-600', border: 'border-indigo-200' },
    blue: { bg: 'bg-blue-100', text: 'text-blue-600', border: 'border-blue-200' },
    green: { bg: 'bg-green-100', text: 'text-green-600', border: 'border-green-200' },
    purple: { bg: 'bg-purple-100', text: 'text-purple-600', border: 'border-purple-200' },
    orange: { bg: 'bg-orange-100', text: 'text-orange-600', border: 'border-orange-200' },
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold mb-2">Warehouse Management</h1>
        <p className="text-gray-600">
          Comprehensive warehouse and inventory management system
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {modules.map((module) => {
          const Icon = module.icon
          const colors = colorClasses[module.color]

          return (
            <Card
              key={module.path}
              className={`hover:shadow-lg transition-shadow cursor-pointer ${module.comingSoon ? 'opacity-75' : ''}`}
              onClick={() => !module.comingSoon && router.push(module.path)}
            >
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className={`inline-flex items-center justify-center w-12 h-12 rounded-lg ${colors.bg} ${colors.text} mb-4`}>
                      <Icon className="h-6 w-6" />
                    </div>
                    <CardTitle className="text-xl mb-2">
                      {module.title}
                      {module.comingSoon && (
                        <span className="ml-2 text-xs font-normal text-gray-500 bg-gray-100 px-2 py-1 rounded">
                          Coming Soon
                        </span>
                      )}
                    </CardTitle>
                    <CardDescription>{module.description}</CardDescription>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <ul className="space-y-2">
                  {module.features.map((feature, index) => (
                    <li key={index} className="flex items-start gap-2 text-sm">
                      <ArrowRight className={`h-4 w-4 mt-0.5 flex-shrink-0 ${colors.text}`} />
                      <span className="text-gray-600">{feature}</span>
                    </li>
                  ))}
                </ul>
                <div className="mt-4 pt-4 border-t">
                  <Button
                    variant={module.comingSoon ? 'outline' : 'default'}
                    className="w-full"
                    disabled={module.comingSoon}
                    onClick={(e) => {
                      e.stopPropagation()
                      if (!module.comingSoon) {
                        router.push(module.path)
                      }
                    }}
                  >
                    {module.comingSoon ? 'Under Development' : 'Open Module'}
                    {!module.comingSoon && <ArrowRight className="ml-2 h-4 w-4" />}
                  </Button>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>

      <Card className="bg-blue-50 border-blue-200">
        <CardContent className="py-6">
          <div className="flex items-start gap-4">
            <Package className="h-8 w-8 text-blue-600 flex-shrink-0" />
            <div>
              <h3 className="font-semibold text-blue-900 mb-2">About Warehouse Management</h3>
              <p className="text-sm text-blue-800">
                This module provides complete warehouse operations management including stock requests,
                real-time inventory tracking, stock audits, and conversion management. Use the modules
                above to access different warehouse management features.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
