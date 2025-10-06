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
  PackageOpen
} from "lucide-react";
import Image from "next/image";
import { NavigationMenu } from "@/app/lbcl/components/navigation-menu";

export function DashboardMenu() {
  const router = useRouter();

  const menuOptions = [
    {
      title: "Manage Agent Stock Receiving",
      icon: Package,
      path: "/stock-receiving",
      description: "Manage and track agent stock inventory"
    },
    {
      title: "Delivery Plan Activity Log Report",
      icon: ClipboardList,
      path: "/delivery-plans",
      description: "View and manage delivery plans and activity logs"
    },
    {
      title: "RD Truck Loading",
      icon: Truck,
      path: "/truck-loading",
      description: "View truck loading list and details"
    },
    {
      title: "RD Truck Loading Request",
      icon: FileCheck,
      path: "/truck-loading/request",
      description: "Create and manage loading requests with buffer quantities"
    },
    {
      title: "Empties Stock Receiving",
      icon: Package,
      path: "/empties-receiving",
      description: "Manage empties stock receiving and physical count"
    },
    {
      title: "Empties Stock Loading",
      icon: PackageOpen,
      path: "/empties-loading",
      description: "Manage empties stock loading operations"
    },
    {
      title: "Damage Collection & Scrapping",
      icon: AlertTriangle,
      path: "/damage-collection",
      description: "Manage damaged items collection and scrapping process"
    },
    {
      title: "Warehouse Stock Take Reconciliation",
      icon: ClipboardCheck,
      path: "/stock-reconciliation",
      description: "Perform warehouse stock take and reconciliation"
    }
  ];

  return (
    <div className="max-w-7xl mx-auto">
      {/* Main Content */}
      <div className="flex items-center justify-center py-12">
        <div className="text-center">
          <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold text-gray-900 mb-4">
            Welcome
          </h1>
          <p className="text-gray-600 mb-8">
            Select an option from the menu to get started
          </p>
        </div>
      </div>

      {/* Menu Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {menuOptions.map((option) => {
          const IconComponent = option.icon;
          return (
            <button
              key={option.path}
              onClick={() => router.push(`/lbcl${option.path}`)}
              className="bg-white p-6 rounded-lg border border-gray-200 hover:border-[#A08B5C] hover:shadow-md transition-all duration-200 text-left"
            >
              <div className="flex items-start gap-4">
                <div className="w-12 h-12 rounded-lg bg-[#A08B5C]/10 flex items-center justify-center flex-shrink-0">
                  <IconComponent className="w-6 h-6 text-[#A08B5C]" />
                </div>
                <div className="flex-1">
                  <h3 className="text-lg font-semibold text-gray-900 mb-1">
                    {option.title}
                  </h3>
                  <p className="text-sm text-gray-600">{option.description}</p>
                </div>
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
}
