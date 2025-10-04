'use client';

import React, { useState, useEffect } from 'react';
import { Card, Tabs, message, Spin } from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  BarChartOutlined,
  SettingOutlined,
  PartitionOutlined,
  HistoryOutlined
} from '@ant-design/icons';
import {
  ApprovalList,
  ApprovalActionModal,
  ApprovalStatistics,
  RuleMasterList,
  ApprovalHierarchyList,
  ApprovalLogsList
} from '@/components/approvals';
import {
  ViewChangeRequestApproval,
  ApprovalStatus,
  ApprovalStatistics as ApprovalStatsType
} from '@/types/approval.types';
import { approvalService } from '@/services/approval.service';
import { useRouter, useSearchParams } from 'next/navigation';

export default function ApprovalsPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const tabParam = searchParams.get('tab');
  const [activeTab, setActiveTab] = useState(tabParam || 'dashboard');
  const [loading, setLoading] = useState(false);
  const [statistics, setStatistics] = useState<ApprovalStatsType>({
    totalPending: 0,
    totalApproved: 0,
    totalRejected: 0,
    byType: {}
  });
  const [actionModalVisible, setActionModalVisible] = useState(false);
  const [selectedApproval, setSelectedApproval] = useState<ViewChangeRequestApproval | null>(null);
  const [actionType, setActionType] = useState<'approve' | 'reject' | 'reassign'>('approve');

  useEffect(() => {
    loadStatistics();
  }, []);

  useEffect(() => {
    if (tabParam) {
      setActiveTab(tabParam);
    }
  }, [tabParam]);

  const loadStatistics = async () => {
    setLoading(true);
    try {
      const stats = await approvalService.getApprovalStatistics();
      setStatistics(stats);
    } catch (error) {
      console.error('Error loading statistics:', error);
      message.error('Failed to load approval statistics');
    } finally {
      setLoading(false);
    }
  };

  const handleView = (approval: ViewChangeRequestApproval) => {
    router.push(`/administration/approvals/${approval.uid}`);
  };

  const handleApprove = (approval: ViewChangeRequestApproval) => {
    setSelectedApproval(approval);
    setActionType('approve');
    setActionModalVisible(true);
  };

  const handleReject = (approval: ViewChangeRequestApproval) => {
    setSelectedApproval(approval);
    setActionType('reject');
    setActionModalVisible(true);
  };

  const handleActionSubmit = async (values: { comments?: string; reassignTo?: string }) => {
    if (!selectedApproval) return;

    try {
      const actionRequest = {
        requestId: selectedApproval.requestID,
        approverId: 'CURRENT_USER_ID', // TODO: Get from auth context
        action: actionType,
        comments: values.comments,
        reassignTo: values.reassignTo
      };

      if (actionType === 'approve') {
        await approvalService.approveRequest(actionRequest);
      } else if (actionType === 'reject') {
        await approvalService.rejectRequest(actionRequest);
      } else if (actionType === 'reassign') {
        await approvalService.reassignRequest(actionRequest);
      }

      setActionModalVisible(false);
      setSelectedApproval(null);
      loadStatistics();
      message.success(`Request ${actionType}d successfully`);
    } catch (error) {
      console.error('Error submitting action:', error);
      throw error;
    }
  };

  const tabItems = [
    {
      key: 'dashboard',
      label: (
        <span>
          <BarChartOutlined />
          Dashboard
        </span>
      ),
      children: (
        <div className="p-4">
          <ApprovalStatistics statistics={statistics} loading={loading} />
        </div>
      )
    },
    {
      key: 'pending',
      label: (
        <span>
          <ClockCircleOutlined />
          Pending ({statistics.totalPending})
        </span>
      ),
      children: (
        <ApprovalList
          filterByStatus={[ApprovalStatus.PENDING]}
          onView={handleView}
          onApprove={handleApprove}
          onReject={handleReject}
          showActions={true}
        />
      )
    },
    {
      key: 'approved',
      label: (
        <span>
          <CheckCircleOutlined />
          Approved ({statistics.totalApproved})
        </span>
      ),
      children: (
        <ApprovalList
          filterByStatus={[ApprovalStatus.APPROVED]}
          onView={handleView}
          showActions={false}
        />
      )
    },
    {
      key: 'rejected',
      label: (
        <span>
          <CloseCircleOutlined />
          Rejected ({statistics.totalRejected})
        </span>
      ),
      children: (
        <ApprovalList
          filterByStatus={[ApprovalStatus.REJECTED]}
          onView={handleView}
          showActions={false}
        />
      )
    },
    {
      key: 'all',
      label: 'All Requests',
      children: (
        <ApprovalList
          onView={handleView}
          onApprove={handleApprove}
          onReject={handleReject}
          showActions={true}
        />
      )
    },
    {
      key: 'rules',
      label: (
        <span>
          <SettingOutlined />
          Approval Rules
        </span>
      ),
      children: <RuleMasterList />
    },
    {
      key: 'hierarchy',
      label: (
        <span>
          <PartitionOutlined />
          Approval Hierarchy
        </span>
      ),
      children: <ApprovalHierarchyList />
    },
    {
      key: 'logs',
      label: (
        <span>
          <HistoryOutlined />
          Approval Logs
        </span>
      ),
      children: <ApprovalLogsList viewMode="table" />
    }
  ];

  return (
    <div className="approvals-page p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Approval Management</h1>
        <p className="text-gray-600">
          Review and manage pending approvals for initiatives, return orders, leave requests, and more.
        </p>
      </div>

      <Card>
        <Tabs
          activeKey={activeTab}
          onChange={(key) => setActiveTab(key)}
          items={tabItems}
        />
      </Card>

      {/* Action Modal */}
      <ApprovalActionModal
        visible={actionModalVisible}
        approval={selectedApproval}
        actionType={actionType}
        onCancel={() => {
          setActionModalVisible(false);
          setSelectedApproval(null);
        }}
        onSubmit={handleActionSubmit}
        users={[]} // TODO: Load users from API
      />
    </div>
  );
}
