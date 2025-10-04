'use client';

import React, { useState, useEffect } from 'react';
import { Table, Tag, Timeline, Card, message } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined, SwapOutlined, ClockCircleOutlined } from '@ant-design/icons';
import { approvalService } from '@/services/approval.service';
import type { ColumnsType } from 'antd/es/table';

interface ApprovalLog {
  id?: number;
  requestId: number;
  approverId: string;
  approverName?: string;
  level: number;
  status: string;
  comments?: string;
  modifiedBy?: string;
  reassignTo?: string;
  createdOn: string;
}

interface ApprovalLogsListProps {
  requestId?: string;
  viewMode?: 'table' | 'timeline';
}

export const ApprovalLogsList: React.FC<ApprovalLogsListProps> = ({
  requestId,
  viewMode = 'table'
}) => {
  const [loading, setLoading] = useState(false);
  const [dataSource, setDataSource] = useState<ApprovalLog[]>([]);

  useEffect(() => {
    if (requestId) {
      loadLogs();
    }
  }, [requestId]);

  const loadLogs = async () => {
    if (!requestId) return;

    setLoading(true);
    try {
      const response = await approvalService.getApprovalLog(requestId);
      setDataSource(response as any);
    } catch (error) {
      console.error('Error loading logs:', error);
      message.error('Failed to load approval logs');
    } finally {
      setLoading(false);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'approved':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'rejected':
        return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
      case 'reassigned':
        return <SwapOutlined style={{ color: '#1890ff' }} />;
      default:
        return <ClockCircleOutlined style={{ color: '#faad14' }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'approved':
        return 'green';
      case 'rejected':
        return 'red';
      case 'reassigned':
        return 'blue';
      case 'pending':
        return 'orange';
      default:
        return 'default';
    }
  };

  const columns: ColumnsType<ApprovalLog> = [
    {
      title: 'Level',
      dataIndex: 'level',
      key: 'level',
      width: 100,
      render: (level) => <Tag color="blue">Level {level}</Tag>,
    },
    {
      title: 'Approver',
      dataIndex: 'approverId',
      key: 'approverId',
      render: (id, record) => record.approverName || id,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => (
        <Tag icon={getStatusIcon(status)} color={getStatusColor(status)}>
          {status}
        </Tag>
      ),
    },
    {
      title: 'Comments',
      dataIndex: 'comments',
      key: 'comments',
      render: (comments) => comments || '-',
    },
    {
      title: 'Reassigned To',
      dataIndex: 'reassignTo',
      key: 'reassignTo',
      render: (reassignTo) => reassignTo || '-',
    },
    {
      title: 'Date',
      dataIndex: 'createdOn',
      key: 'createdOn',
      render: (date) => new Date(date).toLocaleString(),
    },
  ];

  if (viewMode === 'timeline') {
    return (
      <Card title="Approval History" loading={loading}>
        <Timeline>
          {dataSource.map((log, index) => (
            <Timeline.Item
              key={index}
              color={getStatusColor(log.status)}
              dot={getStatusIcon(log.status)}
            >
              <div>
                <strong>Level {log.level}</strong> - {log.approverName || log.approverId}
              </div>
              <Tag color={getStatusColor(log.status)}>{log.status}</Tag>
              {log.comments && <div style={{ marginTop: 8 }}>{log.comments}</div>}
              <div style={{ color: '#999', fontSize: 12, marginTop: 4 }}>
                {new Date(log.createdOn).toLocaleString()}
              </div>
            </Timeline.Item>
          ))}
        </Timeline>
      </Card>
    );
  }

  return (
    <Card title="Approval Logs">
      <Table
        columns={columns}
        dataSource={dataSource}
        loading={loading}
        rowKey={(record, index) => `${record.requestId}-${index}`}
        pagination={{ pageSize: 10 }}
      />
    </Card>
  );
};
