'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { ArrowLeft, CheckCircle } from 'lucide-react';
import LogisticsApprovalGrid from '@/components/LogisticsApprovalGrid';

export default function IncomingRequestsPage() {
  const router = useRouter();
  const [showSuccess, setShowSuccess] = useState(false);

  const handleBack = () => {
    router.push('/load-management/logistics-approval');
  };

  const handleApproveOrder = () => {
    setShowSuccess(true);
    setTimeout(() => setShowSuccess(false), 4000);
  };

  return (
    <div className="p-8">
      {/* Success Message */}
      <div className={`fixed top-20 right-4 z-50 transition-all duration-500 transform ${
        showSuccess ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'
      }`}>
        <div className="bg-background rounded-lg shadow-lg border p-4 flex items-center space-x-3 min-w-[350px]">
          <div className="bg-muted rounded-full p-2">
            <CheckCircle className="w-5 h-5 text-muted-foreground" />
          </div>
          <div>
            <p className="text-foreground font-medium">Request Approved!</p>
            <p className="text-muted-foreground text-sm">Load request has been successfully approved</p>
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
            Back to Logistics Approval
          </Button>
        </div>

        {/* Header Section */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground">
            Logistics Approval Center
          </h1>
          <p className="text-muted-foreground mt-1">Review and approve load requests with advanced workflow management</p>
        </div>

        {/* Logistics Approval Grid Component */}
        <div className="space-y-6">
          <LogisticsApprovalGrid onApproveOrder={handleApproveOrder} />
        </div>
      </div>
    </div>
  );
}