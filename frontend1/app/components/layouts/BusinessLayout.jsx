"use client"
import { useState, useEffect, useRef } from 'react'
import { useRouter, usePathname } from 'next/navigation'
import {
  User,
  LogOut,
  ChevronDown,
  Bell,
  Settings,
  Home,
  Truck,
  Package,
  ClipboardList,
  BarChart3,
  Search,
  MapPin,
  CalendarDays,
  Receipt,
  Cog,
  Menu,
  X,
  ShoppingCart
} from 'lucide-react'
import { useAuth } from '../../contexts/AuthContext'
import { Button } from '../ui/button'

const BusinessLayout = ({ children, title = "LION Business Management" }) => {
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false)
  const [isNotificationOpen, setIsNotificationOpen] = useState(false)
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [notifications, setNotifications] = useState([
    {
      id: 1,
      type: 'info',
      title: 'New Purchase Order',
      message: 'PO #2024-001 has been approved',
      time: '5 min ago',
      read: false
    },
    {
      id: 2,
      type: 'warning',
      title: 'Low Stock Alert',
      message: 'Product SKU-1234 is running low',
      time: '1 hour ago',
      read: false
    },
    {
      id: 3,
      type: 'success',
      title: 'Delivery Completed',
      message: 'Order #5678 delivered successfully',
      time: '2 hours ago',
      read: true
    },
    {
      id: 4,
      type: 'error',
      title: 'Stock Count Discrepancy',
      message: 'Physical count mismatch in Warehouse A',
      time: '3 hours ago',
      read: false
    },
    {
      id: 5,
      type: 'info',
      title: 'System Update',
      message: 'Scheduled maintenance tonight at 2 AM',
      time: '5 hours ago',
      read: true
    }
  ])
  const { user, logout } = useAuth()
  const router = useRouter()
  const pathname = usePathname()

  const unreadCount = notifications.filter(n => !n.read).length

  const markAsRead = (id) => {
    setNotifications(prev =>
      prev.map(notif =>
        notif.id === id ? { ...notif, read: true } : notif
      )
    )
  }

  const markAllAsRead = () => {
    setNotifications(prev =>
      prev.map(notif => ({ ...notif, read: true }))
    )
  }

  const clearNotifications = () => {
    setNotifications([])
    setIsNotificationOpen(false)
  }

  // Click outside handler for notifications
  const notificationRef = useRef(null)
  const userMenuRef = useRef(null)

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (notificationRef.current && !notificationRef.current.contains(event.target)) {
        setIsNotificationOpen(false)
      }
      if (userMenuRef.current && !userMenuRef.current.contains(event.target)) {
        setIsUserMenuOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [])

  const navigationItems = user?.system === 'delivery' ? [
    {
      title: 'Dashboard',
      icon: Home,
      href: '/user/delivery/delivery-dashboard',
      active: pathname === '/user/delivery/delivery-dashboard'
    },
    {
      title: 'Delivery Management',
      icon: Truck,
      href: '/user/delivery/delivery-plan-dashboard',
      active: pathname === '/user/delivery/delivery-plan-dashboard' || 
              pathname === '/user/delivery/pick-list-view' ||
              pathname === '/user/delivery/share-delivery-plan' ||
              pathname === '/user/delivery/signature-capture'
    },
    {
      title: 'Reports & Analytics',
      icon: BarChart3,
      href: '/user/delivery/delivery-activity-log',
      active: pathname === '/user/delivery/delivery-activity-log'
    }
  ] : user?.system === 'itinerary' ? [
    {
      title: 'Dashboard',
      icon: Home,
      href: '/user/itinerary/dashboard',
      active: pathname === '/user/itinerary/dashboard'
    },
    {
      title: 'Itinerary / Calendar',
      icon: CalendarDays,
      href: '/user/itinerary/calendar',
      active: pathname === '/user/itinerary/calendar'
    },
    {
      title: 'Expenses',
      icon: Receipt,
      href: '/user/itinerary/expenses',
      active: pathname === '/user/itinerary/expenses'
    },
    {
      title: 'Configuration',
      icon: Cog,
      href: '/user/itinerary/configuration',
      active: pathname === '/user/itinerary/configuration'
    }
  ] : [
    {
      title: 'Dashboard',
      icon: Home,
      href: '/user/manager/stock-receiving-dashboard',
      active: pathname === '/user/manager/stock-receiving-dashboard' || pathname === '/user/manager'
    },
    {
      title: 'Stock Management',
      icon: Package,
      href: '/user/manager/physical-count',
      active: pathname === '/user/manager/physical-count' ||
              pathname === '/user/manager/physical-count-edit' ||
              pathname === '/user/manager/view-delivery-note'
    },
    {
      title: 'Purchase Orders',
      icon: ShoppingCart,
      href: '/user/manager/purchase-order-templates',
      active: pathname === '/user/manager/purchase-order-templates' ||
              pathname === '/user/manager/purchase-order-status' ||
              pathname === '/user/manager/receipt-stock' ||
              pathname === '/user/manager/manual-stock-receipt'
    },
    {
      title: 'Reports & Analytics',
      icon: BarChart3,
      href: '/user/manager/stock-receiving-history',
      active: pathname === '/user/manager/stock-receiving-history'
    }
  ]

  const handleLogout = () => {
    logout()
    router.push('/user/unified-login')
  }

  return (
    <div className="min-h-screen bg-[var(--background)]">
      {/* Header */}
      <header className="bg-[var(--card)] border-b border-[var(--border)] shadow-sm sticky top-0 z-40">
        <div className="flex items-center justify-between h-16 px-4">
          {/* Left section */}
          <div className="flex items-center space-x-4">
            {/* Menu Button */}
            <button
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="p-2 rounded-md text-[var(--foreground)] hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <Menu className="w-5 h-5" />
            </button>

            <div className="flex items-center space-x-3">
              <div className="w-8 h-8 bg-[var(--primary)] rounded-lg flex items-center justify-center">
                <span className="text-[var(--primary-foreground)] font-bold text-sm">L</span>
              </div>
              <div>
                <h1 className="text-xl font-bold text-[var(--foreground)]">LION</h1>
                <p className="text-xs text-[var(--muted-foreground)]">Business Management System</p>
              </div>
            </div>
          </div>

          {/* Center - Search */}
          <div className="hidden md:flex flex-1 max-w-md mx-8">
            <div className="relative w-full">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-[var(--muted-foreground)] w-4 h-4" />
              <input
                type="text"
                placeholder="Search delivery plans, stock items..."
                className="w-full pl-10 pr-4 py-2 border border-[var(--input-border)] rounded-lg focus:ring-2 focus:ring-[var(--primary)] focus:border-transparent bg-[var(--input)] text-[var(--foreground)] placeholder:text-[var(--muted-foreground)]"
              />
            </div>
          </div>

          {/* Right section */}
          <div className="flex items-center space-x-4">
            {/* Notifications */}
            <div className="relative" ref={notificationRef}>
              <button
                onClick={() => setIsNotificationOpen(!isNotificationOpen)}
                className="p-2 rounded-md relative hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                <Bell className="w-5 h-5 text-[var(--foreground)]" />
                {unreadCount > 0 && (
                  <span className="absolute top-1 right-1 w-2 h-2 bg-[var(--destructive)] rounded-full"></span>
                )}
              </button>

              {isNotificationOpen && (
                <div className="absolute right-0 mt-2 w-96 bg-[var(--card)] border border-[var(--border)] rounded-lg shadow-lg z-50 max-h-[500px] overflow-hidden flex flex-col">
                  {/* Notification Header */}
                  <div className="p-4 border-b border-[var(--border)] flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <h3 className="text-lg font-semibold text-[var(--foreground)]">Notifications</h3>
                      {unreadCount > 0 && (
                        <span className="bg-[var(--primary)] text-[var(--primary-foreground)] text-xs px-2 py-0.5 rounded-full">
                          {unreadCount} new
                        </span>
                      )}
                    </div>
                    <div className="flex items-center space-x-2">
                      {notifications.length > 0 && (
                        <>
                          <button
                            onClick={markAllAsRead}
                            className="text-xs text-[var(--primary)] hover:text-[var(--primary-hover)]"
                          >
                            Mark all read
                          </button>
                          <button
                            onClick={clearNotifications}
                            className="text-xs text-[var(--muted-foreground)] hover:text-[var(--foreground)]"
                          >
                            Clear all
                          </button>
                        </>
                      )}
                    </div>
                  </div>

                  {/* Notification List */}
                  <div className="flex-1 overflow-y-auto">
                    {notifications.length === 0 ? (
                      <div className="p-8 text-center">
                        <Bell className="w-12 h-12 text-[var(--muted-foreground)] mx-auto mb-3" />
                        <p className="text-[var(--muted-foreground)]">No notifications</p>
                      </div>
                    ) : (
                      <div className="divide-y divide-[var(--border)]">
                        {notifications.map((notification) => (
                          <div
                            key={notification.id}
                            className={`p-4 hover:bg-gray-100 dark:hover:bg-gray-800 cursor-pointer transition-colors ${
                              !notification.read ? 'bg-gray-50 dark:bg-gray-900' : ''
                            }`}
                            onClick={() => markAsRead(notification.id)}
                          >
                            <div className="flex items-start space-x-3">
                              <div className={`mt-1 w-2 h-2 rounded-full flex-shrink-0 ${
                                notification.type === 'info' ? 'bg-blue-500' :
                                notification.type === 'success' ? 'bg-green-500' :
                                notification.type === 'warning' ? 'bg-yellow-500' :
                                notification.type === 'error' ? 'bg-red-500' : 'bg-gray-500'
                              }`} />
                              <div className="flex-1 min-w-0">
                                <div className="flex items-start justify-between">
                                  <div className="flex-1">
                                    <p className="text-sm font-medium text-[var(--foreground)]">
                                      {notification.title}
                                    </p>
                                    <p className="text-sm text-[var(--muted-foreground)] mt-0.5">
                                      {notification.message}
                                    </p>
                                  </div>
                                </div>
                                <p className="text-xs text-[var(--muted-foreground)] mt-1">
                                  {notification.time}
                                </p>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>

                  {/* View All Footer */}
                  {notifications.length > 0 && (
                    <div className="p-3 border-t border-[var(--border)]">
                      <button className="w-full text-center text-sm text-[var(--primary)] hover:text-[var(--primary-hover)]">
                        View all notifications
                      </button>
                    </div>
                  )}
                </div>
              )}
            </div>

            {/* User Menu */}
            <div className="relative" ref={userMenuRef}>
              <button
                onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                className="flex items-center space-x-2 p-2 rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                <div className="w-8 h-8 bg-[var(--primary)] rounded-full flex items-center justify-center">
                  <User className="w-4 h-4 text-[var(--primary-foreground)]" />
                </div>
                <div className="hidden sm:block text-left">
                  <p className="text-sm font-medium text-[var(--foreground)]">{user?.name}</p>
                  <p className="text-xs text-[var(--muted-foreground)] capitalize">{user?.role?.replace('_', ' ')}</p>
                </div>
                <ChevronDown className="w-4 h-4 text-[var(--foreground)]" />
              </button>

              {isUserMenuOpen && (
                <div className="absolute right-0 mt-2 w-56 bg-[var(--card)] border border-[var(--border)] rounded-lg shadow-lg z-50">
                  <div className="p-3 border-b border-[var(--border)]">
                    <p className="text-sm font-medium text-[var(--foreground)]">{user?.name}</p>
                    <p className="text-xs text-[var(--muted-foreground)]">{user?.username}</p>
                    <p className="text-xs text-[var(--primary)] capitalize">{user?.system} System</p>
                  </div>
                  <div className="p-1">
                    <button className="flex items-center w-full px-3 py-2 text-sm text-[var(--foreground)] hover:bg-gray-100 dark:hover:bg-gray-800 rounded-md transition-colors">
                      <Settings className="w-4 h-4 mr-3" />
                      Settings
                    </button>
                    <button 
                      onClick={handleLogout}
                      className="flex items-center w-full px-3 py-2 text-sm text-[var(--destructive)] hover:bg-[var(--destructive-light)] rounded-md transition-colors"
                    >
                      <LogOut className="w-4 h-4 mr-3" />
                      Sign Out
                    </button>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Mobile Overlay */}
        {sidebarOpen && (
          <div
            className="fixed inset-0 bg-black/50 z-40 lg:hidden"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        {/* Sidebar */}
        <aside className={`
          fixed lg:relative z-50 lg:z-auto
          bg-[var(--sidebar)] border-r border-[var(--sidebar-border)]
          transform transition-all duration-300 ease-in-out
          flex flex-col min-h-[calc(100vh-64px)] overflow-x-hidden
          ${sidebarOpen
            ? 'translate-x-0 w-64'
            : '-translate-x-full lg:translate-x-0 lg:w-0'
          }
        `}>
          <div className="flex flex-col h-full">
            {/* Sidebar Header with Close Button */}
            <div className="flex items-center justify-between p-4 border-b border-[var(--sidebar-border)] lg:hidden">
              <div className="flex items-center space-x-2">
                <div className="w-6 h-6 bg-[var(--primary)] rounded flex items-center justify-center">
                  <span className="text-[var(--primary-foreground)] font-bold text-xs">L</span>
                </div>
                <span className="font-semibold text-[var(--sidebar-foreground)] text-sm">Menu</span>
              </div>
              <button
                onClick={() => setSidebarOpen(false)}
                className="p-2 rounded-md text-[var(--sidebar-foreground)] hover:bg-[var(--sidebar-hover)] transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Navigation */}
            <nav className="flex-1 px-4 py-4 space-y-1">
              {navigationItems.map((item) => {
                if (item.visible === false) return null

                const IconComponent = item.icon
                return (
                  <button
                    key={item.title}
                    onClick={() => {
                      router.push(item.href)
                      setSidebarOpen(false) // Close sidebar on mobile when navigating
                    }}
                    className={`flex items-center w-full px-3 py-2.5 text-sm font-medium rounded-lg transition-colors ${
                      item.active
                        ? 'bg-[var(--sidebar-active)] text-[var(--primary)] border-l-4 border-[var(--primary)]'
                        : 'text-[var(--sidebar-foreground)] hover:bg-[var(--sidebar-hover)] hover:text-[var(--primary)]'
                    }`}
                  >
                    <IconComponent className="w-5 h-5 mr-3 flex-shrink-0" />
                    <span className="truncate">{item.title}</span>
                  </button>
                )
              })}
            </nav>

            {/* Footer */}
            <div className="p-4 border-t border-[var(--sidebar-border)]">
              <div className="text-center text-xs text-[var(--sidebar-foreground)]">
                <p className="font-medium">Powered by WINIT</p>
                <p>thinking mobile</p>
              </div>
            </div>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 min-h-screen">
          <div className="p-4 sm:p-6">
            {children}
          </div>
        </main>
      </div>
    </div>
  )
}

export default BusinessLayout