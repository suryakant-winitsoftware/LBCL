"use client"
import { useState } from 'react'
import { 
  Menu, 
  X, 
  Bell,
  Search,
  LogOut,
  Settings,
  ChevronDown,
  User
} from 'lucide-react'
import { useAuth } from '../../contexts/AuthContext'
import { useRouter } from 'next/navigation'
import DeliveryAgentSidebar from './DeliveryAgentSidebar'

const DeliveryAgentLayout = ({ children, title = "Delivery Management System" }) => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false)
  const { user, logout } = useAuth()
  const router = useRouter()

  const handleLogout = () => {
    logout()
    router.push('/user/admin/admin-login')
  }

  return (
    <div className="min-h-screen bg-white flex">
      {/* Desktop Sidebar */}
      <div className="hidden lg:flex">
        <DeliveryAgentSidebar />
      </div>

      {/* Mobile Sidebar Overlay */}
      {isSidebarOpen && (
        <div 
          className="fixed inset-0 z-40 bg-black bg-opacity-50 lg:hidden"
          onClick={() => setIsSidebarOpen(false)}
        />
      )}

      {/* Mobile Sidebar */}
      <div className={`fixed inset-y-0 left-0 z-50 transform transition-transform duration-300 ease-in-out lg:hidden ${
        isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
      }`}>
        <div className="flex items-center justify-between p-4 bg-white border-b">
          <span className="text-lg font-semibold text-black">Menu</span>
          <button
            onClick={() => setIsSidebarOpen(false)}
            className="p-2 rounded-md hover:bg-white"
          >
            <X className="w-5 h-5 text-black" />
          </button>
        </div>
        <DeliveryAgentSidebar />
      </div>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {/* Top Header */}
        <header className="bg-white border-b border-black shadow-sm h-16 flex items-center justify-between px-6">
          {/* Left section */}
          <div className="flex items-center space-x-4">
            <button
              onClick={() => setIsSidebarOpen(true)}
              className="p-2 rounded-md hover:bg-white lg:hidden"
            >
              <Menu className="w-6 h-6 text-black" />
            </button>
            
            <div className="flex items-center space-x-3">
              <div className="w-8 h-8 bg-[#375AE6] rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-sm">L</span>
              </div>
              <div>
                <h1 className="text-xl font-bold text-black">LION</h1>
                <p className="text-xs text-black">Delivery Management</p>
              </div>
            </div>
          </div>

          {/* Center - Search */}
          <div className="hidden md:flex flex-1 max-w-md mx-8">
            <div className="relative w-full">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-black w-4 h-4" />
              <input
                type="text"
                placeholder="Search delivery plans..."
                className="w-full pl-10 pr-4 py-2 border border-black rounded-lg focus:ring-2 focus:ring-[#375AE6] focus:border-transparent"
              />
            </div>
          </div>

          {/* Right section */}
          <div className="flex items-center space-x-4">
            {/* Notifications */}
            <button className="p-2 rounded-md relative hover:bg-white">
              <Bell className="w-5 h-5 text-black" />
              <span className="absolute top-1 right-1 w-2 h-2 bg-black rounded-full"></span>
            </button>

            {/* User Menu */}
            <div className="relative">
              <button
                onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                className="flex items-center space-x-2 p-2 rounded-md hover:bg-white"
              >
                <div className="w-8 h-8 bg-[#375AE6] rounded-full flex items-center justify-center">
                  <User className="w-4 h-4 text-white" />
                </div>
                <div className="hidden sm:block text-left">
                  <p className="text-sm font-medium text-black">{user?.name}</p>
                  <p className="text-xs text-black">Delivery Agent</p>
                </div>
                <ChevronDown className="w-4 h-4 text-black" />
              </button>

              {isUserMenuOpen && (
                <div className="absolute right-0 mt-2 w-56 bg-white border border-black rounded-lg shadow-lg z-50">
                  <div className="p-3 border-b border-gray-100">
                    <p className="text-sm font-medium text-black">{user?.name}</p>
                    <p className="text-xs text-black">{user?.username}</p>
                    <p className="text-xs text-[#375AE6]">Delivery System</p>
                  </div>
                  <div className="p-1">
                    <button className="flex items-center w-full px-3 py-2 text-sm text-gray-700 hover:bg-white rounded-md">
                      <Settings className="w-4 h-4 mr-3" />
                      Settings
                    </button>
                    <button 
                      onClick={handleLogout}
                      className="flex items-center w-full px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-md"
                    >
                      <LogOut className="w-4 h-4 mr-3" />
                      Sign Out
                    </button>
                  </div>
                </div>
              )}
            </div>
          </div>
        </header>

        {/* Main Content */}
        <main className="flex-1 p-6 overflow-auto">
          {children}
        </main>
      </div>
    </div>
  )
}

export default DeliveryAgentLayout