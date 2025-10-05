"use client";

import { usePathname, useRouter } from "next/navigation";
import { PageHeader } from "@/app/lbcl/components/page-header";
import { Button } from "@/components/ui/button";
import { LogOut, ArrowLeft } from "lucide-react";
import { useDeliveryAuth } from "@/providers/delivery-auth-provider";

interface DeliveryLayoutProps {
  children: React.ReactNode;
}

export function DeliveryLayout({ children }: DeliveryLayoutProps) {
  const pathname = usePathname();
  const router = useRouter();
  const { user, logout } = useDeliveryAuth();

  // Don't show header on login page
  if (pathname === "/lbcl/login" || pathname === "/lbcl") {
    return <>{children}</>;
  }

  const handleLogout = () => {
    logout();
    router.push("/lbcl/login");
  };

  const handleBackToLBCL = () => {
    router.push("/login");
  };

  // Get page title based on pathname
  const getPageTitle = () => {
    if (pathname?.includes("/dashboard")) return "Dashboard";
    if (pathname?.includes("/delivery-plans")) return "Delivery Plans";
    if (pathname?.includes("/stock-receiving/history")) return "Stock History";
    if (pathname?.includes("/stock-receiving/timeline"))
      return "Timeline Stamps";
    if (pathname?.includes("/stock-receiving")) return "Stock Receiving";
    if (pathname?.includes("/truck-loading/request")) return "Loading Request";
    if (pathname?.includes("/truck-loading/collection"))
      return "Load Collection";
    if (pathname?.includes("/truck-loading/activity-log"))
      return "Truck Activity Log";
    if (pathname?.includes("/truck-loading")) return "Truck Loading";
    if (pathname?.includes("/empties-receiving/activity-log"))
      return "Empties Activity Log";
    if (pathname?.includes("/empties-receiving")) return "Empties Receiving";
    if (pathname?.includes("/damage-collection")) return "Damage Collection";
    if (pathname?.includes("/stock-reconciliation"))
      return "Stock Reconciliation";
    return "Delivery Management";
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <PageHeader
        title={getPageTitle()}
        rightContent={
          <div className="flex items-center gap-2">
            {user && (
              <span className="text-sm text-gray-600 hidden sm:inline">
                {user.name || user.loginId}
              </span>
            )}
            <Button
              variant="ghost"
              size="sm"
              onClick={handleBackToLBCL}
              className="text-xs"
            >
              <ArrowLeft className="w-4 h-4 mr-1" />
              <span className="hidden sm:inline">LBCL</span>
            </Button>
            <Button variant="ghost" size="sm" onClick={handleLogout}>
              <LogOut className="w-4 h-4" />
            </Button>
          </div>
        }
      />
      <main className="p-4">{children}</main>
    </div>
  );
}
