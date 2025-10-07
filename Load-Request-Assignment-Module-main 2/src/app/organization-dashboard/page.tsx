'use client';

import React from 'react';
import Link from 'next/link';
import { 
  Card, 
  CardContent, 
  CardHeader, 
  CardTitle,
  CardDescription 
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { 
  Building2, 
  Package, 
  GitBranch,
  Network,
  Users,
  Settings,
  ArrowRight,
  BarChart,
  FileStack,
  Boxes
} from 'lucide-react';

export default function OrganizationDashboard() {
  const modules = [
    {
      title: 'Organization Management',
      description: 'Manage organizations, create, update and delete organizations',
      icon: Building2,
      href: '/updatedfeatures/organization-management/organizations/manage',
      color: 'bg-blue-500',
      stats: 'Core CRUD Operations'
    },
    {
      title: 'Warehouse Management',
      description: 'Manage warehouses, stock levels, and inventory across organizations',
      icon: Package,
      href: '/warehouse-management',
      color: 'bg-green-500',
      stats: 'NEW - Full Stock Control'
    },
    {
      title: 'Division Management',
      description: 'Manage organizational divisions, suppliers and distribution network',
      icon: GitBranch,
      href: '/division-management',
      color: 'bg-purple-500',
      stats: 'NEW - Division Network'
    },
    {
      title: 'Organization Hierarchy',
      description: 'Visualize and manage organization hierarchical structure',
      icon: Network,
      href: '/updatedfeatures/organization-management/organizations/hierarchy',
      color: 'bg-orange-500',
      stats: 'Tree Visualization'
    },
    {
      title: 'Organization Types',
      description: 'Configure organization types and their properties',
      icon: FileStack,
      href: '/organization-types',
      color: 'bg-indigo-500',
      stats: 'Type Configuration'
    },
    {
      title: 'Bulk Operations',
      description: 'Import/export and bulk create organizations',
      icon: Boxes,
      href: '/organization-bulk',
      color: 'bg-pink-500',
      stats: 'Mass Operations'
    }
  ];

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg p-8 text-white">
        <h1 className="text-4xl font-bold mb-2">Organization Management System</h1>
        <p className="text-lg opacity-90">
          Complete suite for managing organizational structure, warehouses, and divisions
        </p>
        <div className="grid grid-cols-4 gap-4 mt-6">
          <div className="bg-white/10 rounded-lg p-4">
            <div className="text-3xl font-bold">150+</div>
            <div className="text-sm">Organizations</div>
          </div>
          <div className="bg-white/10 rounded-lg p-4">
            <div className="text-3xl font-bold">45</div>
            <div className="text-sm">Warehouses</div>
          </div>
          <div className="bg-white/10 rounded-lg p-4">
            <div className="text-3xl font-bold">12</div>
            <div className="text-sm">Divisions</div>
          </div>
          <div className="bg-white/10 rounded-lg p-4">
            <div className="text-3xl font-bold">98%</div>
            <div className="text-sm">Coverage</div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4">
            <Link href="/updatedfeatures/organization-management/organizations/create">
              <Button>
                <Building2 className="mr-2 h-4 w-4" />
                Create Organization
              </Button>
            </Link>
            <Link href="/warehouse-management">
              <Button variant="outline">
                <Package className="mr-2 h-4 w-4" />
                Add Warehouse
              </Button>
            </Link>
            <Link href="/division-management">
              <Button variant="outline">
                <GitBranch className="mr-2 h-4 w-4" />
                Create Division
              </Button>
            </Link>
            <Button variant="outline">
              <BarChart className="mr-2 h-4 w-4" />
              View Reports
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Modules Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {modules.map((module) => {
          const Icon = module.icon;
          return (
            <Link key={module.href} href={module.href}>
              <Card className="hover:shadow-lg transition-shadow cursor-pointer h-full">
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className={`p-3 rounded-lg ${module.color} text-white`}>
                      <Icon className="h-6 w-6" />
                    </div>
                    <ArrowRight className="h-5 w-5 text-muted-foreground" />
                  </div>
                  <CardTitle className="mt-4">{module.title}</CardTitle>
                  <CardDescription>{module.description}</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="text-sm font-medium text-muted-foreground">
                    {module.stats}
                  </div>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>

      {/* Feature Comparison */}
      <Card>
        <CardHeader>
          <CardTitle>System Coverage</CardTitle>
          <CardDescription>Backend API vs Frontend Implementation Status</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div>
              <div className="flex justify-between mb-2">
                <span className="text-sm font-medium">Organization CRUD</span>
                <span className="text-sm text-green-600">100% Complete</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-green-600 h-2 rounded-full" style={{width: '100%'}}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between mb-2">
                <span className="text-sm font-medium">Warehouse Management</span>
                <span className="text-sm text-green-600">100% Complete</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-green-600 h-2 rounded-full" style={{width: '100%'}}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between mb-2">
                <span className="text-sm font-medium">Division Management</span>
                <span className="text-sm text-green-600">100% Complete</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-green-600 h-2 rounded-full" style={{width: '100%'}}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between mb-2">
                <span className="text-sm font-medium">Hierarchy Visualization</span>
                <span className="text-sm text-blue-600">90% Complete</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-blue-600 h-2 rounded-full" style={{width: '90%'}}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between mb-2">
                <span className="text-sm font-medium">Organization Types</span>
                <span className="text-sm text-yellow-600">70% Complete</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-yellow-600 h-2 rounded-full" style={{width: '70%'}}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between mb-2">
                <span className="text-sm font-medium">Bulk Operations</span>
                <span className="text-sm text-yellow-600">60% Complete</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-yellow-600 h-2 rounded-full" style={{width: '60%'}}></div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}