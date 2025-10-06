"use client";

import { useRouter } from "next/navigation";
import {
  Package,
  ClipboardList,
  Truck,
  FileCheck,
  AlertTriangle,
  ClipboardCheck,
  Bell,
  User,
} from "lucide-react";
import Image from "next/image";
import { NavigationMenu } from "@/components/navigation-menu";

export function DashboardMenu() {
  const router = useRouter();

  const menuOptions = [
    {
      title: "Manage Agent Stock Receiving",
      icon: Package,
      path: "/stock-receiving",
      description: "Manage and track agent stock inventory",
    },
    {
      title: "Delivery Plan Activity Log Report",
      icon: ClipboardList,
      path: "/lbcl-plans",
      description: "View and manage delivery plans and activity logs",
    },
    {
      title: "RD Truck Loading",
      icon: Truck,
      path: "/truck-loading",
      description: "View truck loading list and details",
    },
    {
      title: "RD Truck Loading Request",
      icon: FileCheck,
      path: "/truck-loading/request",
      description: "Create and manage loading requests with buffer quantities",
    },
    {
      title: "Empties Stock Receiving",
      icon: Package,
      path: "/empties-receiving",
      description: "Manage empties stock receiving and physical count",
    },
    {
      title: "Damage Collection & Scrapping",
      icon: AlertTriangle,
      path: "/damage-collection",
      description: "Manage damaged items collection and scrapping process",
    },
    {
      title: "Warehouse Stock Take Reconciliation",
      icon: ClipboardCheck,
      path: "/stock-reconciliation",
      description: "Perform warehouse stock take and reconciliation",
    },
  ];

  return (
    <div className="min-h-screen bg-white flex flex-col">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between max-w-7xl mx-auto">
          <NavigationMenu />
          <div className="flex items-center gap-3 sm:gap-4">
            <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors">
              <Bell className="w-5 h-5 sm:w-6 sm:h-6 text-gray-700" />
            </button>
            <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors">
              <User className="w-5 h-5 sm:w-6 sm:h-6 text-gray-700" />
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 px-4 py-8 sm:px-6 lg:px-8 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold text-gray-900">
            Welcome
          </h1>
        </div>
      </main>
    </div>
  );
}
