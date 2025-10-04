"use client"
import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { Button } from './ui/button'
import { Truck, Package, Shield, ArrowRight, Users, BarChart3, Settings } from 'lucide-react'
import { useAuth } from '../contexts/AuthContext'

const HomePage = () => {
  const { user, loading } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!loading && user) {
      // Redirect authenticated users to their respective dashboard
      if (user.system === 'stock') {
        router.push('/user/manager/stock-receiving-dashboard')
      } else if (user.system === 'delivery') {
        router.push('/user/delivery/delivery-dashboard')
      } else if (user.system === 'admin') {
        router.push('/user/admin/dashboard')
      } else if (user.system === 'itinerary') {
        router.push('/user/itinerary/dashboard')
      }
    }
  }, [user, loading, router])

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-gray-600 text-sm">Loading...</p>
        </div>
      </div>
    )
  }

  if (user) {
    return null // Will redirect via useEffect
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
      <div className="max-w-4xl w-full">
        {/* Header */}
        <div className="text-center mb-12">
          <div className="w-20 h-20 bg-blue-600 rounded-xl flex items-center justify-center mx-auto mb-6">
            <span className="text-white text-4xl font-bold">L</span>
          </div>
          <h1 className="text-4xl lg:text-5xl font-bold text-gray-900 mb-4">
            LION Management System
          </h1>
          <p className="text-xl text-gray-600 mb-2">LION Business Central</p>
          <p className="text-sm text-gray-500">Powered by WINIT thinking mobile</p>
        </div>

        {/* Feature Cards */}
        <div className="grid md:grid-cols-3 gap-6 mb-12">
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 text-center">
            <Package className="w-12 h-12 text-blue-600 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Stock Management</h3>
            <p className="text-gray-600 text-sm">Comprehensive inventory tracking and stock receiving management</p>
          </div>
          
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 text-center">
            <Truck className="w-12 h-12 text-green-600 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Delivery Management</h3>
            <p className="text-gray-600 text-sm">Real-time delivery tracking and logistics coordination</p>
          </div>
          
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 text-center">
            <Shield className="w-12 h-12 text-purple-600 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Admin Control</h3>
            <p className="text-gray-600 text-sm">Complete system administration and user management</p>
          </div>
        </div>

        {/* Main CTA Section */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">
            Access Your System
          </h2>
          <p className="text-gray-600 mb-8 max-w-2xl mx-auto">
            Single login portal for all systems with role-based access control. 
            Secure authentication ensures you only see what you need to see.
          </p>
          
          <div className="flex flex-col sm:flex-row gap-4 justify-center items-center mb-6">
            <Link href="/login" className="w-full sm:w-auto">
              <Button className="w-full sm:w-auto bg-blue-600 hover:bg-blue-700 text-white px-8 py-3 text-lg font-semibold flex items-center justify-center gap-2 transition-all duration-200">
                LOGIN TO SYSTEM
                <ArrowRight className="w-5 h-5" />
              </Button>
            </Link>
          </div>

          <div className="flex flex-wrap justify-center gap-4 text-sm text-gray-500">
            <div className="flex items-center gap-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span>Secure Authentication</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
              <span>Role-Based Access</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-2 h-2 bg-purple-500 rounded-full"></div>
              <span>Real-Time Updates</span>
            </div>
          </div>
        </div>

        {/* System Status */}
        <div className="mt-8 text-center">
          <div className="inline-flex items-center gap-2 text-sm text-gray-600 bg-white px-4 py-2 rounded-full border border-gray-200">
            <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
            <span>System Online & Ready</span>
          </div>
        </div>

        {/* Footer */}
        <div className="mt-12 text-center text-xs text-gray-500">
          <p>© 2025 LION Business Management System</p>
          <p>All rights reserved. Powered by WINIT thinking mobile</p>
        </div>
      </div>
    </div>
  )
}

export default HomePage