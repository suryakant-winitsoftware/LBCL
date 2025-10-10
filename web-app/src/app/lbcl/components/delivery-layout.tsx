"use client";

import { usePathname } from "next/navigation";
import { PageHeader } from "@/app/lbcl/components/page-header";

interface DeliveryLayoutProps {
  children: React.ReactNode;
}

export function DeliveryLayout({ children }: DeliveryLayoutProps) {
  const pathname = usePathname();

  // Don't show header on login page
  if (pathname === "/lbcl/login" || pathname === "/lbcl") {
    return <>{children}</>;
  }

  // Get page title based on pathname
  const getPageTitle = () => {
    if (pathname?.includes("/dashboard")) return "Dashboard";
    if (pathname?.includes("/sales-itinerary")) return "Itinerary Planning";
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
    if (pathname?.includes("/empties-receiving/physical-count"))
      return "Physical Count & Audit Empties";
    if (pathname?.includes("/empties-receiving/damage-results"))
      return "Empties Damage Results";
    if (pathname?.includes("/empties-receiving")) return "Empties Receiving";
    if (pathname?.includes("/empties-loading")) return "Empties Stock Loading";
    if (pathname?.includes("/damage-collection")) return "Damage Collection";
    if (pathname?.includes("/stock-reconciliation"))
      return "Stock Reconciliation";
    return "Delivery Management";
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <PageHeader
        title={getPageTitle()}
      />
      <main className="p-4">{children}</main>
    </div>
  );
}
