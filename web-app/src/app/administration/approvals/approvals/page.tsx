'use client';

import React, { useState, useEffect } from 'react';
import { Card, Tabs } from 'antd';
import {
  SettingOutlined,
  PartitionOutlined,
  HistoryOutlined
} from '@ant-design/icons';
import {
  RuleMasterList,
  ApprovalHierarchyList,
  ApprovalLogsList
} from '@/components/approvals';

export default function ApprovalsManagementPage() {
  const [activeTab, setActiveTab] = useState('rules');

  const tabItems = [
    {
      key: 'rules',
      label: (
        <span>
          <SettingOutlined />
          Approval Rules
        </span>
      ),
      children: (
        <div className="p-4">
          <RuleMasterList />
        </div>
      )
    },
    {
      key: 'hierarchy',
      label: (
        <span>
          <PartitionOutlined />
          Approval Hierarchy
        </span>
      ),
      children: (
        <div className="p-4">
          <ApprovalHierarchyList />
        </div>
      )
    },
    {
      key: 'logs',
      label: (
        <span>
          <HistoryOutlined />
          Approval Logs
        </span>
      ),
      children: (
        <div className="p-4">
          <ApprovalLogsList viewMode="table" />
        </div>
      )
    }
  ];

  return (
    <div className="approvals-management-page p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Approval Configuration</h1>
        <p className="text-gray-600">
          Configure approval rules, hierarchy, and view approval history
        </p>
      </div>

      <Card>
        <Tabs
          activeKey={activeTab}
          onChange={(key) => setActiveTab(key)}
          items={tabItems}
        />
      </Card>
    </div>
  );
}
