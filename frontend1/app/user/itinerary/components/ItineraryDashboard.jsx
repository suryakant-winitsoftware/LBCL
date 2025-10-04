"use client"
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import DashboardMetrics from '../../../components/dashboard/DashboardMetrics'
import ItineraryChart from './ItineraryChart'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'

const ItineraryDashboard = () => {
  const { user } = useAuth()

  return (
    <ProtectedRoute requiredSystem="itinerary">
      <BusinessLayout title="Sales Itinerary Planning">
        <div className="space-y-6">
          {/* Welcome Section */}
          <div className="bg-gradient-to-r from-[#375AE6] to-[#2947d1] rounded-lg p-6 text-white">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold mb-2">
                  Welcome back, {user?.name}!
                </h1>
                <p className="text-white/90">
                  Here's your sales itinerary overview for today
                </p>
              </div>
              <div className="text-right">
                <p className="text-white/90 text-sm">Today's Date</p>
                <p className="text-xl font-semibold">
                  {new Date().toLocaleDateString('en-GB', {
                    day: '2-digit',
                    month: 'short',
                    year: 'numeric'
                  })}
                </p>
              </div>
            </div>
          </div>

          {/* Metrics Cards */}
          <DashboardMetrics userSystem="itinerary" />

          {/* Current and Next Month Tabs */}
          <div className="flex justify-center gap-10 mb-6">
            <div className="text-center px-6 py-3 bg-[#375AE6] text-white rounded font-semibold text-sm">
              CURRENT MONTH ( MAY 2025)
            </div>
            <div className="text-center px-6 py-3 bg-gray-400 text-white rounded font-semibold text-sm">
              NEXT MONTH ( JUNE 2025)
            </div>
          </div>

          {/* Chart Section */}
          <ItineraryChart />
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default ItineraryDashboard