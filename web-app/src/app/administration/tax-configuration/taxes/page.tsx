'use client';

import React from 'react';
import { Card } from '@/components/ui/card';
import TaxListView from './components/TaxListView';

const TaxManagement = () => {
  return (
    <div className="container mx-auto p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold">Tax Management</h1>
        <p className="text-gray-600 mt-2">
          Manage taxes for your organization
        </p>
      </div>

      <Card className="p-6">
        <TaxListView />
      </Card>
    </div>
  );
};

export default TaxManagement;