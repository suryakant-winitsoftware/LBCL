"use client";

import React from "react";
import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Truck, Package } from "lucide-react";

const DashboardPage = () => {
  const navigationCards = [
    {
      title: "Delivery",
      description: "Access delivery dashboard and manage delivery operations",
      icon: <Truck className="h-12 w-12" />,
      href: "/delivery/delivery-dashboard",
      color: "text-blue-600",
    },
    {
      title: "Manager",
      description: "Access manager dashboard and stock receiving operations",
      icon: <Package className="h-12 w-12" />,
      href: "/manager/stock-receiving-dashboard",
      color: "text-green-600",
    },
  ];

  return (
    <div className="flex flex-col gap-6 p-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
          <p className="text-gray-500 mt-1">
            Welcome back! Select a module to get started.
          </p>
        </div>
      </div>

      {/* Navigation Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:gap-8">
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
};

export default DashboardPage;
