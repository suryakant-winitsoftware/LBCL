'use client'

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Package, ArrowRight } from 'lucide-react'
import { useRouter } from 'next/navigation'

export default function StockAuditsPage() {
  const router = useRouter()

  return (
    <div className="container mx-auto py-8">
      <div className="max-w-2xl mx-auto">
        <Card>
          <CardHeader className="text-center">
            <div className="mx-auto w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mb-4">
              <Package className="h-8 w-8 text-blue-600" />
            </div>
            <CardTitle className="text-2xl">Stock Audits</CardTitle>
            <CardDescription>
              Warehouse stock audit management system
            </CardDescription>
          </CardHeader>
          <CardContent className="text-center space-y-4">
            <p className="text-gray-600">
              This feature allows you to:
            </p>
            <ul className="text-left space-y-2 max-w-md mx-auto">
              <li className="flex items-start gap-2">
                <ArrowRight className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
                <span>Conduct physical stock audits</span>
              </li>
              <li className="flex items-start gap-2">
                <ArrowRight className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
                <span>Record audit findings and discrepancies</span>
              </li>
              <li className="flex items-start gap-2">
                <ArrowRight className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
                <span>Generate audit reports</span>
              </li>
              <li className="flex items-start gap-2">
                <ArrowRight className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
                <span>Track audit history</span>
              </li>
            </ul>
            <div className="pt-4">
              <Button variant="outline" onClick={() => router.back()}>
                Back to Warehouse Management
              </Button>
            </div>
            <p className="text-sm text-gray-500 pt-4">
              Coming soon - Under development
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
