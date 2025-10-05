"use client"

import { useRouter } from "next/navigation"
import { useEffect } from "react"
import { Button } from "@/components/ui/button"
import { ArrowLeft, Search, Mail } from "lucide-react"

export default function NotFound() {
  const router = useRouter()

  // Remove layout by adding a marker to the body
  useEffect(() => {
    document.body.setAttribute("data-not-found", "true")
    return () => {
      document.body.removeAttribute("data-not-found")
    }
  }, [])

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center p-4">
      <div className="max-w-2xl w-full text-center">
        {/* 404 Visual */}
        <div className="mb-8">
          <h1 className="text-9xl font-bold text-gray-200 dark:text-gray-700 select-none">
            404
          </h1>
          <div className="relative -mt-20">
            <div className="text-6xl mb-4">🔍</div>
            <h2 className="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-3">
              Page Not Found
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 mb-8 max-w-md mx-auto">
              The page you&apos;re looking for doesn&apos;t exist or has been moved.
            </p>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-4 justify-center items-center mb-12">
          <Button
            size="lg"
            className="gap-2 min-w-[160px]"
            onClick={() => router.back()}
          >
            <ArrowLeft className="h-4 w-4" />
            Go Back
          </Button>
        </div>

        {/* Help Section */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 border border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
            Need Help?
          </h3>
          <div className="grid sm:grid-cols-2 gap-4 text-sm">
            <div className="flex items-start gap-3 text-left">
              <Search className="h-5 w-5 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0" />
              <div>
                <p className="font-medium text-gray-900 dark:text-gray-100">Search Our Site</p>
                <p className="text-gray-600 dark:text-gray-400">
                  Use the search function to find what you need
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3 text-left">
              <Mail className="h-5 w-5 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0" />
              <div>
                <p className="font-medium text-gray-900 dark:text-gray-100">Contact Support</p>
                <p className="text-gray-600 dark:text-gray-400">
                  Our team is here to assist you
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Footer */}
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-8">
          Error Code: 404 | Page Not Found
        </p>
      </div>
    </div>
  )
}
