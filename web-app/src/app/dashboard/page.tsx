'use client';

import React from "react";
import Link from 'next/link';
import { Building2, Store, Warehouse, Package, Users, ArrowRight, Truck } from 'lucide-react';

const modules = [
  {
    title: 'Distributor Management',
    description: 'Manage your distributor network',
    icon: Building2,
    href: '/administration/distributor-management/distributors',
    color: 'bg-blue-500',
  },
  {
    title: 'Store Management',
    description: 'Manage stores and outlets',
    icon: Store,
    href: '/administration/store-management/stores',
    color: 'bg-green-500',
  },
  {
    title: 'Warehouse Management',
    description: 'Manage warehouse operations',
    icon: Warehouse,
    href: '/administration/warehouse-management/warehouses',
    color: 'bg-purple-500',
  },
  {
    title: 'Product Management',
    description: 'Manage products and inventory',
    icon: Package,
    href: '/productssales/products',
    color: 'bg-orange-500',
  },
  {
    title: 'Team Management',
    description: 'Manage employees and teams',
    icon: Users,
    href: '/administration/team-management',
    color: 'bg-indigo-500',
  },
  {
    title: 'Delivery Operations',
    description: 'Access delivery dashboard and manage delivery operations',
    icon: Truck,
    href: '/delivery/delivery-dashboard',
    color: 'bg-cyan-500',
  },
  {
    title: 'Stock Receiving',
    description: 'Access manager dashboard and stock receiving operations',
    icon: Package,
    href: '/manager/stock-receiving-dashboard',
    color: 'bg-teal-500',
  },
];

const page = () => {
  return (
    <div className="container mx-auto p-6">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">Dashboard</h1>
        <p className="text-lg text-muted-foreground">
          Welcome back! Quick access to key modules
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {modules.map((module) => {
          const Icon = module.icon;
          return (
            <Link
              key={module.href}
              href={module.href}
              className="group relative overflow-hidden rounded-lg border bg-card p-6 hover:shadow-lg transition-all duration-200"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className={`inline-flex p-3 rounded-lg ${module.color} bg-opacity-10 mb-4`}>
                    <Icon className={`h-6 w-6 ${module.color.replace('bg-', 'text-')}`} />
                  </div>
                  <h3 className="font-semibold text-lg mb-2 group-hover:text-primary transition-colors">
                    {module.title}
                  </h3>
                  <p className="text-sm text-muted-foreground">
                    {module.description}
                  </p>
                </div>
                <ArrowRight className="h-5 w-5 text-muted-foreground group-hover:text-primary transition-all group-hover:translate-x-1" />
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
};

export default page;
