# Professional Loading Components

Enterprise-grade loading components with minimal, professional design.

## Components

### 1. NavigationLoader
Top progress bar that shows during page navigation.

```tsx
import { NavigationLoader } from "@/components/ui/navigation-loader"

// Add to your layout
<NavigationLoader />
```

### 2. LoadingProvider
Context provider for managing loading states across the app.

```tsx
import { LoadingProvider } from "@/components/providers/loading-provider"

// Wrap your app
<LoadingProvider>
  <YourApp />
</LoadingProvider>
```

### 3. LoadingSpinner
Simple, professional spinner component.

```tsx
import { LoadingSpinner } from "@/components/ui/navigation-loader"

<LoadingSpinner size="sm" />
<LoadingSpinner size="default" />
<LoadingSpinner size="lg" />
```

### 4. LoadingOverlay
Overlay for loading states on specific sections.

```tsx
import { LoadingOverlay } from "@/components/ui/loading-overlay"

<div className="relative">
  <YourContent />
  <LoadingOverlay isLoading={loading} message="Saving..." />
</div>
```

### 5. FullPageLoader
Full page loading screen.

```tsx
import { FullPageLoader } from "@/components/ui/loading-overlay"

<FullPageLoader show={isInitialLoading} message="Initializing..." />
```

### 6. InlineLoader
Inline loading indicator.

```tsx
import { InlineLoader } from "@/components/ui/loading-overlay"

<InlineLoader message="Processing..." />
```

## Hook Usage

### useNavigationLoading
Hook for programmatic navigation with loading states.

```tsx
import { useNavigationLoading } from "@/hooks/use-navigation-loading"

function MyComponent() {
  const { navigateWithLoading } = useNavigationLoading()
  
  const handleClick = () => {
    navigateWithLoading("/dashboard")
  }
  
  return <button onClick={handleClick}>Go to Dashboard</button>
}
```

### useLoading
Access loading context directly.

```tsx
import { useLoading } from "@/components/providers/loading-provider"

function MyComponent() {
  const { isLoading, setLoading } = useLoading()
  
  const handleSubmit = async () => {
    setLoading(true)
    try {
      await submitData()
    } finally {
      setLoading(false)
    }
  }
}
```

## Setup Instructions

1. Add LoadingProvider to your root layout:

```tsx
// app/layout.tsx
import { LoadingProvider } from "@/components/providers/loading-provider"

export default function RootLayout({ children }) {
  return (
    <html>
      <body>
        <LoadingProvider>
          {children}
        </LoadingProvider>
      </body>
    </html>
  )
}
```

2. Add NavigationLoader to show progress on route changes:

```tsx
// In your main layout component
import { NavigationLoader } from "@/components/ui/navigation-loader"

<NavigationLoader />
```

## Features

- ✅ Professional, minimal design suitable for enterprise
- ✅ No excessive animations or colors
- ✅ TypeScript support
- ✅ Client-side navigation handling
- ✅ Progress indication
- ✅ Multiple loading states (overlay, inline, full-page)
- ✅ Context-based state management
- ✅ Automatic cleanup and memory management

## Styling

All components use neutral colors:
- Blue progress bar (#2563eb)
- Gray text and backgrounds
- Minimal animations (simple spin, smooth transitions)
- No flashy colors or excessive motion