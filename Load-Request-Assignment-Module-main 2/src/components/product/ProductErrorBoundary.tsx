'use client'

import React, { Component, ReactNode } from 'react';
import { AlertCircle, RefreshCw, Home } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: React.ErrorInfo | null;
}

export class ProductErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null
    };
  }

  static getDerivedStateFromError(error: Error): State {
    return {
      hasError: true,
      error,
      errorInfo: null
    };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Product Error Boundary caught an error:', error, errorInfo);
    
    // Log to error reporting service
    this.logErrorToService(error, errorInfo);
    
    this.setState({
      error,
      errorInfo
    });
  }

  logErrorToService = (error: Error, errorInfo: React.ErrorInfo) => {
    // Here you would send error details to your error tracking service
    // e.g., Sentry, LogRocket, etc.
    const errorData = {
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack,
      timestamp: new Date().toISOString(),
      url: window.location.href,
      userAgent: navigator.userAgent
    };
    
    console.log('Error logged:', errorData);
    
    // If you have an error tracking service:
    // errorTrackingService.logError(errorData);
  };

  handleReset = () => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null
    });
  };

  handleReload = () => {
    window.location.reload();
  };

  handleGoHome = () => {
    window.location.href = '/';
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return <>{this.props.fallback}</>;
      }

      const { error, errorInfo } = this.state;
      const isDevelopment = process.env.NODE_ENV === 'development';

      return (
        <div className="min-h-screen flex items-center justify-center p-4">
          <Card className="max-w-2xl w-full">
            <CardHeader>
              <div className="flex items-center gap-2">
                <AlertCircle className="h-6 w-6 text-destructive" />
                <CardTitle>Something went wrong</CardTitle>
              </div>
              <CardDescription>
                An error occurred while processing your product operation
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertTitle>Error Details</AlertTitle>
                <AlertDescription>
                  {error?.message || 'An unexpected error occurred'}
                </AlertDescription>
              </Alert>

              {isDevelopment && error && (
                <div className="space-y-2">
                  <h4 className="text-sm font-semibold">Stack Trace (Development Only):</h4>
                  <pre className="text-xs bg-gray-100 dark:bg-gray-900 p-3 rounded-lg overflow-auto max-h-48">
                    {error.stack}
                  </pre>
                  {errorInfo && (
                    <>
                      <h4 className="text-sm font-semibold mt-4">Component Stack:</h4>
                      <pre className="text-xs bg-gray-100 dark:bg-gray-900 p-3 rounded-lg overflow-auto max-h-48">
                        {errorInfo.componentStack}
                      </pre>
                    </>
                  )}
                </div>
              )}

              <div className="flex gap-3">
                <Button onClick={this.handleReset} variant="default">
                  <RefreshCw className="h-4 w-4 mr-2" />
                  Try Again
                </Button>
                <Button onClick={this.handleReload} variant="outline">
                  Reload Page
                </Button>
                <Button onClick={this.handleGoHome} variant="outline">
                  <Home className="h-4 w-4 mr-2" />
                  Go Home
                </Button>
              </div>

              <div className="text-sm text-muted-foreground">
                <p>If this problem persists, please contact support with the following information:</p>
                <ul className="list-disc list-inside mt-2 space-y-1">
                  <li>Error: {error?.message}</li>
                  <li>Time: {new Date().toLocaleString()}</li>
                  <li>Page: Product Management</li>
                </ul>
              </div>
            </CardContent>
          </Card>
        </div>
      );
    }

    return this.props.children;
  }
}

// Functional component wrapper for easier use
export const withProductErrorBoundary = <P extends object>(
  Component: React.ComponentType<P>,
  fallback?: ReactNode
) => {
  return (props: P) => (
    <ProductErrorBoundary fallback={fallback}>
      <Component {...props} />
    </ProductErrorBoundary>
  );
};