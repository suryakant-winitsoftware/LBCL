'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { ArrowLeft, CheckCircle, Package2 } from 'lucide-react';
import LoadRequestGrid from '@/components/LoadRequestGrid';

export default function CreateLSRPage() {
  const router = useRouter();
  const [showSuccess, setShowSuccess] = useState(false);

  const handleBack = () => {
    router.push('/load-management/lsr');
  };

  const handleCreateOrder = () => {
    setShowSuccess(true);
    setTimeout(() => {
      setShowSuccess(false);
      router.push('/load-management/lsr/history');
    }, 3000);
  };

  return (
    <div className="p-8">
      {/* Success Message */}
      <div className={`fixed top-20 right-4 z-50 transition-all duration-500 transform ${
        showSuccess ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'
      }`}>
        <div className="bg-background rounded-lg shadow-lg border p-4 flex items-center space-x-3 min-w-[300px]">
          <div className="bg-muted rounded-full p-2">
            <CheckCircle className="w-5 h-5 text-muted-foreground" />
          </div>
          <div>
            <p className="text-foreground font-medium">Success!</p>
            <p className="text-muted-foreground text-sm">Load request submitted for approval</p>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto">
        {/* Back Button */}
        <div className="mb-6">
          <Button
            variant="outline"
            size="sm"
            onClick={handleBack}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to LSR
          </Button>
        </div>

        {/* Header Section */}
        <div className="mb-8">
          <div className="flex items-center space-x-3 mb-3">
            <div className="bg-muted rounded-lg p-3">
              <Package2 className="w-6 h-6 text-muted-foreground" />
            </div>
            <div>
              <h1 className="text-3xl font-bold text-foreground">
                Load Request Management
              </h1>
              <p className="text-muted-foreground mt-1">Create and manage your load requests efficiently</p>
            </div>
          </div>
        </div>

        {/* Load Request Grid Component */}
        <div className="bg-white rounded-lg border shadow-xs p-6">
          <LoadRequestGrid onCreateOrder={handleCreateOrder} />
        </div>
      </div>
    </div>
  );
}