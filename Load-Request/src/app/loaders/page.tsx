"use client"

import { useState } from "react"
import { 
  Loader, 
  PageLoader, 
  ButtonLoader, 
  CardLoader, 
  SkeletonLoader,
  InlineLoader 
} from "@/components/ui/loader"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"

export default function LoadersPage() {
  const [showPageLoader, setShowPageLoader] = useState(false)
  
  return (
    <div className="max-w-7xl mx-auto p-8 space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Professional Loaders
        </h1>
        <p className="text-gray-600 dark:text-gray-400 mt-2">
          A collection of professional loading states for the application
        </p>
      </div>
      
      {/* Page Loader Demo */}
      <Card>
        <CardHeader>
          <CardTitle>Page Loader</CardTitle>
          <CardDescription>Full page loading state with backdrop</CardDescription>
        </CardHeader>
        <CardContent>
          <Button onClick={() => setShowPageLoader(true)}>
            Show Page Loader
          </Button>
          {showPageLoader && (
            <div className="fixed inset-0 z-50">
              <PageLoader text="Loading application..." />
              <Button 
                className="fixed top-4 right-4 z-[60]"
                onClick={() => setShowPageLoader(false)}
              >
                Close
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
      
      {/* Loader Variants */}
      <Card>
        <CardHeader>
          <CardTitle>Loader Variants</CardTitle>
          <CardDescription>Different color variants for various contexts</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            <div className="text-center">
              <Loader variant="default" />
              <p className="text-sm text-gray-600 mt-2">Default</p>
            </div>
            <div className="text-center">
              <Loader variant="primary" />
              <p className="text-sm text-gray-600 mt-2">Primary</p>
            </div>
            <div className="text-center">
              <Loader variant="secondary" />
              <p className="text-sm text-gray-600 mt-2">Secondary</p>
            </div>
            <div className="text-center">
              <Loader variant="destructive" />
              <p className="text-sm text-gray-600 mt-2">Destructive</p>
            </div>
            <div className="text-center">
              <Loader variant="outline" />
              <p className="text-sm text-gray-600 mt-2">Outline</p>
            </div>
            <div className="text-center">
              <Loader variant="ghost" />
              <p className="text-sm text-gray-600 mt-2">Ghost</p>
            </div>
            <div className="text-center dark:bg-gray-800 p-4 rounded">
              <Loader variant="white" />
              <p className="text-sm text-white mt-2">White</p>
            </div>
          </div>
        </CardContent>
      </Card>
      
      {/* Loader Sizes */}
      <Card>
        <CardHeader>
          <CardTitle>Loader Sizes</CardTitle>
          <CardDescription>Different sizes for various use cases</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-around">
            <div className="text-center">
              <Loader size="sm" showText={false} />
              <p className="text-sm text-gray-600 mt-2">Small</p>
            </div>
            <div className="text-center">
              <Loader size="md" showText={false} />
              <p className="text-sm text-gray-600 mt-2">Medium</p>
            </div>
            <div className="text-center">
              <Loader size="default" showText={false} />
              <p className="text-sm text-gray-600 mt-2">Default</p>
            </div>
            <div className="text-center">
              <Loader size="lg" showText={false} />
              <p className="text-sm text-gray-600 mt-2">Large</p>
            </div>
            <div className="text-center">
              <Loader size="xl" showText={false} />
              <p className="text-sm text-gray-600 mt-2">Extra Large</p>
            </div>
          </div>
        </CardContent>
      </Card>
      
      {/* Button Loader */}
      <Card>
        <CardHeader>
          <CardTitle>Button Loader</CardTitle>
          <CardDescription>Loading states for buttons</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-4">
            <Button disabled>
              <ButtonLoader className="mr-2" />
              Saving...
            </Button>
            <Button variant="secondary" disabled>
              <ButtonLoader className="mr-2" variant="secondary" />
              Processing...
            </Button>
            <Button variant="destructive" disabled>
              <ButtonLoader className="mr-2" variant="white" />
              Deleting...
            </Button>
          </div>
        </CardContent>
      </Card>
      
      {/* Card Loader */}
      <Card>
        <CardHeader>
          <CardTitle>Card Loader</CardTitle>
          <CardDescription>Loading state for card content</CardDescription>
        </CardHeader>
        <CardContent>
          <CardLoader text="Fetching data..." />
        </CardContent>
      </Card>
      
      {/* Skeleton Loader */}
      <Card>
        <CardHeader>
          <CardTitle>Skeleton Loader</CardTitle>
          <CardDescription>Content placeholder with shimmer effect</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <SkeletonLoader className="h-4 w-full" />
            <SkeletonLoader className="h-4 w-3/4" />
            <SkeletonLoader className="h-4 w-1/2" />
          </div>
          <div className="flex gap-4">
            <SkeletonLoader className="h-20 w-20 rounded-lg" />
            <div className="flex-1 space-y-2">
              <SkeletonLoader className="h-4 w-full" />
              <SkeletonLoader className="h-4 w-2/3" />
            </div>
          </div>
        </CardContent>
      </Card>
      
      {/* Inline Loader */}
      <Card>
        <CardHeader>
          <CardTitle>Inline Loader</CardTitle>
          <CardDescription>Loading state within text content</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-gray-700 dark:text-gray-300">
            Loading user data <InlineLoader /> please wait a moment.
          </p>
          <p className="text-gray-700 dark:text-gray-300 mt-2">
            Processing <InlineLoader variant="primary" /> 85% complete
          </p>
        </CardContent>
      </Card>
    </div>
  )
}