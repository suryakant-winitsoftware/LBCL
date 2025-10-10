"use client";

import React from "react";
import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Truck, UserCheck, BarChart3 } from "lucide-react";

export default function LoadManagementPage() {
  const navigationCards = [
    {
      title: "LSR (Van Sales Rep)",
      description: "Initiate daily load requests, review system recommendations, adjust buffer, and submit",
      icon: <Truck className="h-12 w-12" />,
      href: "/load-management/lsr",
      color: "text-blue-600"
    },
    {
      title: "Agent Logistics Officer",
      description: "Review requests depot-wise, adjust & approve load, assign trucks, drivers & helpers, and finalize load sheets",
      icon: <UserCheck className="h-12 w-12" />,
      href: "/load-management/logistics-approval",
      color: "text-green-600"
    }
  ];

  return (
    <div className="flex flex-col gap-6 p-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Load Management</h1>
          <p className="text-gray-500 mt-1">
            Manage load requests and logistics approvals
          </p>
        </div>
      </div>

      {/* Navigation Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 lg:gap-8">
        {navigationCards.map((card, index) => (
          <Link key={index} href={card.href}>
            <Card className="transition-all hover:shadow-lg hover:scale-[1.02] cursor-pointer h-full">
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-2xl font-bold">
                  {card.title}
                </CardTitle>
                <div className={card.color}>{card.icon}</div>
              </CardHeader>
              <CardContent>
                <p className="text-gray-500 text-sm">{card.description}</p>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}