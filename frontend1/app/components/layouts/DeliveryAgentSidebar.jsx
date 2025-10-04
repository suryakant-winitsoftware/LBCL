"use client"
import { useState } from 'react'
import { useRouter, usePathname } from 'next/navigation'
import { 
  Home,
  Truck,
  BarChart3,
  ClipboardList,
  User
} from 'lucide-react'
import { useAuth } from '../../contexts/AuthContext'

const DeliveryAgentSidebar = () => {
  const { user } = useAuth()
  const router = useRouter()
  const pathname = usePathname()

  const navigationItems = [
    {
      title: 'Dashboard',
      icon: Home,
      href: '/user/delivery/delivery-dashboard',
      active: pathname.includes('delivery-dashboard')
    },
    {
      title: 'Delivery Management',
      icon: Truck,
      href: '/user/delivery/delivery-plan-dashboard',
      active: pathname.includes('delivery-plan-dashboard')
    },
    {
      title: 'Reports & Analytics',
      icon: BarChart3,
      href: '/user/delivery/delivery-activity-log',
      active: pathname.includes('delivery-activity-log') || pathname.includes('reports')
    }
  ]

  return (
    <div className="w-64 min-h-screen bg-white border-r border-black flex flex-col">
      {/* User Profile Section */}
      <div className="p-4 border-b border-black">
        <div className="flex items-center space-x-3">
          <div className="w-10 h-10 bg-[#375AE6] rounded-full flex items-center justify-center">
            <User className="w-5 h-5 text-white" />
          </div>
          <div>
            <p className="text-sm font-semibold text-black">{user?.name || 'Delivery Agent'}</p>
            <p className="text-xs text-black">Delivery System</p>
          </div>
        </div>
      </div>

      {/* Navigation Menu */}
      <nav className="flex-1 py-4">
        {navigationItems.map((item) => {
          const IconComponent = item.icon
          return (
            <button
              key={item.title}
              onClick={() => router.push(item.href)}
              className={`
                flex items-center w-full px-6 py-4 text-left transition-all duration-200
                ${item.active 
                  ? 'bg-[#375AE6] text-white border-r-4 border-[#000000] shadow-inner' 
                  : 'text-black hover:bg-[#ffffff] hover:text-[#375AE6]'
                }
              `}
            >
              <IconComponent className={`w-5 h-5 mr-4 ${item.active ? 'text-white' : 'text-black'}`} />
              <span className="font-medium text-sm">{item.title}</span>
            </button>
          )
        })}
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-black">
        <div className="text-center text-xs text-black">
          <p className="font-medium">Powered by WINIT</p>
          <p>thinking mobile</p>
        </div>
      </div>
    </div>
  )
}

export default DeliveryAgentSidebar