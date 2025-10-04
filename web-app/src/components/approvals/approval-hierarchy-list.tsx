'use client';

import React, { useState, useEffect } from 'react';
import { Table, Tag, message, Card } from 'antd';
import { approvalService } from '@/services/approval.service';
import type { ColumnsType } from 'antd/es/table';

interface ApprovalHierarchy {
  id: number;
  ruleId: number;
  level: number;
  approverId: string;
  approverName?: string;
  minAmount?: number;
  maxAmount?: number;
  isActive: boolean;
}

export const ApprovalHierarchyList: React.FC<{ ruleId?: string }> = ({ ruleId }) => {
  const [loading, setLoading] = useState(false);
  const [dataSource, setDataSource] = useState<ApprovalHierarchy[]>([]);

  useEffect(() => {
    if (ruleId) {
      loadHierarchy();
    }
  }, [ruleId]);

  const loadHierarchy = async () => {
    if (!ruleId) return;

    setLoading(true);
    try {
      const response = await approvalService.getApprovalHierarchy(ruleId);
      setDataSource(response as any);
    } catch (error) {
      console.error('Error loading hierarchy:', error);
      message.error('Failed to load approval hierarchy');
    } finally {
      setLoading(false);
    }
  };

  const columns: ColumnsType<ApprovalHierarchy> = [
    {
      title: 'Level',
      dataIndex: 'level',
      key: 'level',
      width: 100,
      render: (level) => <Tag color="blue">Level {level}</Tag>,
    },
    {
      title: 'Approver ID',
      dataIndex: 'approverId',
      key: 'approverId',
    },
    {
      title: 'Approver Name',
      dataIndex: 'approverName',
      key: 'approverName',
      render: (name) => name || '-',
    },
    {
      title: 'Min Amount',
      dataIndex: 'minAmount',
      key: 'minAmount',
      render: (amount) => amount ? `$${amount.toLocaleString()}` : '-',
    },
    {
      title: 'Max Amount',
      dataIndex: 'maxAmount',
      key: 'maxAmount',
      render: (amount) => amount ? `$${amount.toLocaleString()}` : 'Unlimited',
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
  ];

  return (
    <Card title="Approval Hierarchy">
      <Table
        columns={columns}
        dataSource={dataSource}
        loading={loading}
        rowKey="id"
        pagination={false}
      />
    </Card>
  );
};
