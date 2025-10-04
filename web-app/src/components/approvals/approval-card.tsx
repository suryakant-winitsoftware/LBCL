'use client';

import React from 'react';
import { Card, Tag, Button, Space, Tooltip } from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  UserOutlined,
  CalendarOutlined,
  EyeOutlined
} from '@ant-design/icons';
import { ViewChangeRequestApproval, ApprovalStatus, ApprovalItemType } from '@/types/approval.types';
import { format } from 'date-fns';

interface ApprovalCardProps {
  approval: ViewChangeRequestApproval;
  onView?: (approval: ViewChangeRequestApproval) => void;
  onApprove?: (approval: ViewChangeRequestApproval) => void;
  onReject?: (approval: ViewChangeRequestApproval) => void;
  showActions?: boolean;
}

const getStatusColor = (status: ApprovalStatus): string => {
  switch (status) {
    case ApprovalStatus.PENDING:
      return 'orange';
    case ApprovalStatus.APPROVED:
      return 'green';
    case ApprovalStatus.REJECTED:
      return 'red';
    case ApprovalStatus.IN_PROGRESS:
      return 'blue';
    default:
      return 'default';
  }
};

const getStatusIcon = (status: ApprovalStatus) => {
  switch (status) {
    case ApprovalStatus.PENDING:
      return <ClockCircleOutlined />;
    case ApprovalStatus.APPROVED:
      return <CheckCircleOutlined />;
    case ApprovalStatus.REJECTED:
      return <CloseCircleOutlined />;
    case ApprovalStatus.IN_PROGRESS:
      return <ClockCircleOutlined />;
    default:
      return <ClockCircleOutlined />;
  }
};

const getTypeColor = (type: string): string => {
  const typeMap: { [key: string]: string } = {
    [ApprovalItemType.INITIATIVE]: 'purple',
    [ApprovalItemType.RETURN_ORDER]: 'cyan',
    [ApprovalItemType.LEAVE_REQUEST]: 'magenta',
    [ApprovalItemType.CREDIT_OVERRIDE]: 'gold',
    [ApprovalItemType.PROMOTION]: 'geekblue',
    [ApprovalItemType.STORE]: 'lime',
  };
  return typeMap[type] || 'default';
};

export const ApprovalCard: React.FC<ApprovalCardProps> = ({
  approval,
  onView,
  onApprove,
  onReject,
  showActions = true
}) => {
  const createdDate = approval.createdOn
    ? format(new Date(approval.createdOn), 'MMM dd, yyyy HH:mm')
    : 'N/A';

  return (
    <Card
      hoverable
      className="approval-card mb-4"
      extra={
        <Tag color={getStatusColor(approval.status)} icon={getStatusIcon(approval.status)}>
          {approval.status}
        </Tag>
      }
    >
      <div className="flex flex-col gap-3">
        {/* Header */}
        <div className="flex justify-between items-start">
          <div className="flex-1">
            <div className="flex items-center gap-2 mb-2">
              <Tag color={getTypeColor(approval.linkedItemType)}>
                {approval.linkedItemType}
              </Tag>
              <span className="text-sm text-gray-500">#{approval.requestID}</span>
            </div>
            <h3 className="text-lg font-semibold mb-1">
              {approval.itemDescription || approval.linkedItemUID}
            </h3>
          </div>
        </div>

        {/* Details */}
        <div className="flex flex-col gap-2 text-sm">
          <div className="flex items-center gap-2 text-gray-600">
            <UserOutlined />
            <span>Requested by: <strong>{approval.requesterName || 'Unknown'}</strong></span>
          </div>

          {approval.currentApproverName && (
            <div className="flex items-center gap-2 text-gray-600">
              <UserOutlined />
              <span>Current Approver: <strong>{approval.currentApproverName}</strong></span>
            </div>
          )}

          <div className="flex items-center gap-2 text-gray-600">
            <CalendarOutlined />
            <span>{createdDate}</span>
          </div>
        </div>

        {/* Comments */}
        {approval.comments && (
          <div className="text-sm text-gray-600 bg-gray-50 p-2 rounded">
            <strong>Comments:</strong> {approval.comments}
          </div>
        )}

        {/* Actions */}
        {showActions && approval.status === ApprovalStatus.PENDING && (
          <div className="flex justify-end gap-2 pt-2 border-t">
            <Space>
              {onView && (
                <Button
                  icon={<EyeOutlined />}
                  onClick={() => onView(approval)}
                >
                  View Details
                </Button>
              )}
              {onApprove && (
                <Button
                  type="primary"
                  icon={<CheckCircleOutlined />}
                  onClick={() => onApprove(approval)}
                  className="bg-green-500 hover:bg-green-600"
                >
                  Approve
                </Button>
              )}
              {onReject && (
                <Button
                  danger
                  icon={<CloseCircleOutlined />}
                  onClick={() => onReject(approval)}
                >
                  Reject
                </Button>
              )}
            </Space>
          </div>
        )}
      </div>
    </Card>
  );
};

export default ApprovalCard;
