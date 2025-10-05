"use client"
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import {
  FileText,
  ShoppingCart,
  Package,
  ClipboardList,
  Warehouse
} from 'lucide-react'
import PurchaseOrderTemplates from './PurchaseOrderTemplates'
import PurchaseOrderStatus from './PurchaseOrderStatus'
import ReceiptStock from './ReceiptStock'
import ManualStockReceipt from './ManualStockReceipt'
import StockReceivingDashboard from './StockReceivingDashboard'

const PurchaseOrderDashboard = () => {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState('templates')

  const tabs = [
    {
      id: 'templates',
      label: 'Purchase Order Templates',
      icon: FileText,
      component: PurchaseOrderTemplates
    },
    {
      id: 'status',
      label: 'Purchase Order Status',
      icon: ShoppingCart,
      component: PurchaseOrderStatus
    },
    {
      id: 'receipt',
      label: 'Receipt Stock',
      icon: Package,
      component: ReceiptStock
    },
    {
      id: 'manual',
      label: 'Manual Stock Receipt',
      icon: ClipboardList,
      component: ManualStockReceipt
    },
    {
      id: 'stock-receiving',
      label: 'Stock Receiving Dashboard',
      icon: Warehouse,
      component: StockReceivingDashboard
    }
  ]

  const ActiveComponent = tabs.find(tab => tab.id === activeTab)?.component

  return (
    <div className="min-h-screen bg-[var(--background)]">
      {/* Tab Navigation */}
      <div className="bg-[var(--card)] border-b border-[var(--border)] shadow-sm sticky top-16 z-30">
        <div className="px-4 sm:px-6">
          <div className="flex overflow-x-auto scrollbar-hide">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-2 px-4 py-3 text-sm font-medium whitespace-nowrap transition-all border-b-2 ${
                  activeTab === tab.id
                    ? 'text-[var(--primary)] border-[var(--primary)]'
                    : 'text-[var(--muted-foreground)] border-transparent hover:text-[var(--foreground)] hover:border-[var(--muted)]'
                }`}
              >
                <tab.icon className="w-4 h-4" />
                <span>{tab.label}</span>
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Active Tab Content */}
      <div className="animate-in fade-in duration-200">
        {ActiveComponent && <ActiveComponent />}
      </div>
    </div>
  )
}

export default PurchaseOrderDashboard