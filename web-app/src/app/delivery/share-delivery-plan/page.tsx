"use client"

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { ArrowLeft, MessageSquare, Mail, MessageCircle, X } from 'lucide-react'

export default function ShareDeliveryPlanPage() {
  const router = useRouter()
  const [showModal, setShowModal] = useState(false)

  return (
    <div className="space-y-6">
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center gap-4">
            <Button variant="outline" size="icon" onClick={() => router.back()}>
              <ArrowLeft className="w-4 h-4" />
            </Button>
            <div>
              <h1 className="text-2xl font-bold">Share Delivery Plan</h1>
              <p className="text-sm text-muted-foreground mt-1">Send delivery plan details</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <p className="text-sm text-muted-foreground">
              The delivery plan will be automatically sent to a group of recipients who are part of the list
            </p>
            <Button onClick={() => setShowModal(true)} className="w-full">
              Open Share Options
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Share Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6 relative">
            <button
              onClick={() => setShowModal(false)}
              className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
            >
              <X className="w-5 h-5" />
            </button>

            <h2 className="text-xl font-bold mb-4">Share Delivery Plan</h2>

            <p className="text-sm text-gray-600 mb-6">
              The delivery plan will be automatically sent to a group of recipients who are part of the list
            </p>

            <div className="mb-6">
              <h3 className="text-sm font-semibold mb-4">Share Files Via</h3>
              <div className="flex justify-center gap-8">
                <button className="flex flex-col items-center gap-2 p-4 hover:bg-gray-50 rounded-lg transition-colors">
                  <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                    <MessageSquare className="w-8 h-8 text-gray-700" />
                  </div>
                  <span className="text-xs font-medium">SMS</span>
                </button>

                <button className="flex flex-col items-center gap-2 p-4 hover:bg-gray-50 rounded-lg transition-colors">
                  <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                    <Mail className="w-8 h-8 text-gray-700" />
                  </div>
                  <span className="text-xs font-medium">Email</span>
                </button>

                <button className="flex flex-col items-center gap-2 p-4 hover:bg-gray-50 rounded-lg transition-colors">
                  <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                    <MessageCircle className="w-8 h-8 text-gray-700" />
                  </div>
                  <span className="text-xs font-medium">WhatsApp</span>
                </button>
              </div>
            </div>

            <Button
              onClick={() => setShowModal(false)}
              className="w-full bg-[#9d8b5c] hover:bg-[#8a7a50]"
            >
              DONE
            </Button>
          </div>
        </div>
      )}

      <Card>
        <CardContent className="pt-6">
          <h3 className="font-semibold mb-4">Delivery Plan Details</h3>
          <div className="space-y-3">
            <div className="flex justify-between py-2 border-b">
              <span className="text-muted-foreground">Plan ID:</span>
              <span className="font-medium">DP-2025-001</span>
            </div>
            <div className="flex justify-between py-2 border-b">
              <span className="text-muted-foreground">Date:</span>
              <span className="font-medium">{new Date().toLocaleDateString()}</span>
            </div>
            <div className="flex justify-between py-2 border-b">
              <span className="text-muted-foreground">Status:</span>
              <span className="font-medium text-green-600">Approved</span>
            </div>
            <div className="flex justify-between py-2">
              <span className="text-muted-foreground">Total Items:</span>
              <span className="font-medium">45</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
