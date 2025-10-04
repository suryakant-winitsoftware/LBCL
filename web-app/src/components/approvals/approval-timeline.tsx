'use client';

import React from 'react';
import { Timeline, Tag, Card, Avatar } from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  UserOutlined,
  SwapOutlined
} from '@ant-design/icons';
import { ApprovalLog, ApprovalStatus } from '@/types/approval.types';
import { format } from 'date-fns';

interface ApprovalTimelineProps {
  logs: ApprovalLog[];
  loading?: boolean;
}

const getStatusIcon = (status: ApprovalStatus) => {
  switch (status) {
    case ApprovalStatus.APPROVED:
      return <CheckCircleOutlined className="text-green-500" />;
    case ApprovalStatus.REJECTED:
      return <CloseCircleOutlined className="text-red-500" />;
    case ApprovalStatus.PENDING:
      return <ClockCircleOutlined className="text-orange-500" />;
    default:
      return <ClockCircleOutlined className="text-blue-500" />;
  }
};

const getStatusColor = (status: ApprovalStatus): string => {
  switch (status) {
    case ApprovalStatus.APPROVED:
      return 'green';
    case ApprovalStatus.REJECTED:
      return 'red';
    case ApprovalStatus.PENDING:
      return 'orange';
    default:
      return 'blue';
  }
};

export const ApprovalTimeline: React.FC<ApprovalTimelineProps> = ({ logs, loading = false }) => {
  if (loading) {
    return <Card loading={true} />;
  }

  if (!logs || logs.length === 0) {
    return (
      <Card>
        <div className="text-center text-gray-500 py-8">
          <ClockCircleOutlined className="text-4xl mb-2" />
          <p>No approval history available</p>
        </div>
      </Card>
    );
  }

  // Sort logs by date (most recent first)
  const sortedLogs = [...logs].sort((a, b) =>
    new Date(b.createdOn).getTime() - new Date(a.createdOn).getTime()
  );

  const timelineItems = sortedLogs.map((log, index) => ({
    key: log.id || index,
    dot: getStatusIcon(log.status),
    color: getStatusColor(log.status),
    children: (
      <Card className="mb-2" size="small">
        <div className="flex flex-col gap-2">
          {/* Header */}
          <div className="flex justify-between items-start">
            <div className="flex items-center gap-2">
              <Avatar size="small" icon={<UserOutlined />} />
              <div>
                <div className="font-semibold">{log.approverName || log.approverId}</div>
                <div className="text-xs text-gray-500">Level {log.level} Approver</div>
              </div>
            </div>
            <Tag color={getStatusColor(log.status)}>{log.status}</Tag>
          </div>

          {/* Comments */}
          {log.comments && (
            <div className="text-sm bg-gray-50 p-2 rounded">
              <strong>Comments:</strong> {log.comments}
            </div>
          )}

          {/* Reassignment */}
          {log.reassignTo && (
            <div className="text-sm flex items-center gap-2 text-blue-600">
              <SwapOutlined />
              <span>
                Reassigned to: <strong>{log.reassignToName || log.reassignTo}</strong>
              </span>
            </div>
          )}

          {/* Modified By */}
          {log.modifiedBy && (
            <div className="text-xs text-gray-500">
              Modified by: {log.modifiedBy}
            </div>
          )}

          {/* Timestamp */}
          <div className="text-xs text-gray-400">
            {format(new Date(log.createdOn), 'MMM dd, yyyy HH:mm:ss')}
          </div>
        </div>
      </Card>
    )
  }));

  return (
    <Card title="Approval History" className="approval-timeline">
      <Timeline
        mode="left"
        items={timelineItems}
      />
    </Card>
  );
};

export default ApprovalTimeline;
