"use client"
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { 
  X, 
  Home, 
  Truck, 
  Package, 
  FileText, 
  LogOut,
  User,
  Settings,
  Activity,
  ClipboardList,
  Share,
  Edit3
} from 'lucide-react'
import { useAuth } from '../contexts/AuthContext'
import { useWorkflow } from '../contexts/WorkflowContext'

const NavigationSidebar = ({ isOpen, onClose }) => {
  const { user, logout } = useAuth()
  const { isStepCompleted } = useWorkflow()
  const router = useRouter()

  const handleNavigation = (path) => {
    router.push(path)
    onClose()
  }

  const handleLogout = () => {
    logout()
    router.push('/user/admin/admin-login')
    onClose()
  }

  const navigationItems = [
    {
      id: 'home',
      label: 'Home',
      icon: Home,
      path: '/',
      showForAll: true
    },
    {
      id: 'delivery-dashboard',
      label: 'Delivery Dashboard',
      icon: Truck,
      path: '/user/delivery/delivery-dashboard',
      showForDelivery: true
    },
    {
      id: 'signature',
      label: 'Signature Capture',
      icon: Edit3,
      path: '/user/delivery/signature-capture',
      showForDelivery: true
    },
    {
      id: 'stock-dashboard',
      label: 'Stock Dashboard',
      icon: Package,
      path: '/user/manager/stock-receiving-dashboard',
      showForStock: true
    }
  ]

  const shouldShowItem = (item) => {
    if (item.showForAll) return true
    if (item.showForDelivery && user?.system === 'delivery') {
      // Hide items that are marked as hideWhenCompleted and are completed
      if (item.hideWhenCompleted && isStepCompleted(item.id, '85444127121')) {
        return false
      }
      return true
    }
    if (item.showForStock && user?.system === 'stock') return true
    return false
  }

  if (!isOpen) return null

  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black bg-opacity-30 z-40"
        onClick={onClose}
      />
      
      {/* Sidebar */}
      <div className="fixed left-0 top-0 h-full w-72 bg-white shadow-lg z-50 transform transition-transform duration-200 ease-in-out border-r border-black">
        {/* Header */}
        <div className="flex items-center justify-between p-3 border-b border-black bg-white">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-[#375AE6] rounded flex items-center justify-center">
              <span className="text-white font-bold text-sm">L</span>
            </div>
            <div>
              <h2 className="text-lg font-bold text-black">LION</h2>
              <p className="text-xs text-black">Management System</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 hover:bg-white rounded transition-colors"
          >
            <X className="w-4 h-4 text-black" />
          </button>
        </div>

        {/* User Info */}
        {user && (
          <div className="p-3 border-b border-black bg-white">
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 bg-white rounded-full flex items-center justify-center">
                <User className="w-4 h-4 text-black" />
              </div>
              <div>
                <h3 className="font-medium text-black text-sm">{user.name}</h3>
                <p className="text-xs text-black">{user.role.replace('_', ' ')}</p>
                <p className="text-xs text-[#375AE6]">
                  {user.system === 'delivery' ? 'Delivery' : 'Stock'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Navigation Items */}
        <div className="flex-1 py-2">
          <nav className="px-2">
            {navigationItems.map((item) => {
              if (!shouldShowItem(item)) return null
              
              const Icon = item.icon
              return (
                <button
                  key={item.id}
                  onClick={() => handleNavigation(item.path)}
                  className="w-full flex items-center gap-2 px-3 py-2 text-left rounded hover:bg-white transition-colors text-sm mb-1"
                >
                  <Icon className="w-4 h-4 text-black" />
                  <span className="font-medium text-black">
                    {item.label}
                  </span>
                </button>
              )
            })}
          </nav>
        </div>

        {/* System Info & Actions */}
        <div className="border-t border-black p-3">
          {/* System Status */}
          <div className="flex items-center justify-between text-xs mb-2">
            <span className="text-black">Status</span>
            <span className="flex items-center gap-1 text-[#375AE6]">
              <div className="w-2 h-2 bg-[#375AE6] rounded-full"></div>
              Online
            </span>
          </div>

          {/* Quick Actions */}
          <div className="space-y-1">
            <button className="w-full flex items-center gap-2 px-2 py-2 text-xs text-black hover:bg-white rounded transition-colors">
              <Settings className="w-3 h-3" />
              Settings
            </button>
            <button
              onClick={handleLogout}
              className="w-full flex items-center gap-2 px-2 py-2 text-xs text-black hover:bg-white rounded transition-colors"
            >
              <LogOut className="w-3 h-3" />
              Logout
            </button>
          </div>
        </div>

        {/* Footer */}
        <div className="border-t border-black p-2">
          <div className="text-center text-xs text-black">
            <div>Powered by <span className="text-[#375AE6] font-medium">WINIT</span></div>
            <div>thinking mobile</div>
          </div>
        </div>
      </div>
    </>
  )
}

export default NavigationSidebar