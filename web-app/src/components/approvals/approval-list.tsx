'use client';

import React, { useState, useEffect } from 'react';
import { Table, Tag, Button, Space, Input, Select, DatePicker, message, Spin } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  EyeOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SearchOutlined,
  FilterOutlined
} from '@ant-design/icons';
import { ViewChangeRequestApproval, ApprovalStatus, ApprovalItemType } from '@/types/approval.types';
import { approvalService } from '@/services/approval.service';
import { format } from 'date-fns';

const { RangePicker } = DatePicker;

interface ApprovalListProps {
  onView?: (approval: ViewChangeRequestApproval) => void;
  onApprove?: (approval: ViewChangeRequestApproval) => void;
  onReject?: (approval: ViewChangeRequestApproval) => void;
  filterByStatus?: ApprovalStatus[];
  filterByType?: string[];
  showActions?: boolean;
}

export const ApprovalList: React.FC<ApprovalListProps> = ({
  onView,
  onApprove,
  onReject,
  filterByStatus,
  filterByType,
  showActions = true
}) => {
  const [approvals, setApprovals] = useState<ViewChangeRequestApproval[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string[]>(filterByStatus || []);
  const [selectedType, setSelectedType] = useState<string[]>(filterByType || []);

  useEffect(() => {
    loadApprovals();
  }, []);

  const loadApprovals = async () => {
    setLoading(true);
    try {
      const data = await approvalService.getAllChangeRequests();
      console.log('Loaded approvals:', data, 'Count:', data?.length || 0);
      setApprovals(data || []);
    } catch (error) {
      console.error('Error loading approvals:', error);
      message.error('Failed to load approvals. Please check the console for details.');
      setApprovals([]);
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string): string => {
    const statusMap: { [key: string]: string } = {
      [ApprovalStatus.PENDING]: 'orange',
      [ApprovalStatus.APPROVED]: 'green',
      [ApprovalStatus.REJECTED]: 'red',
      [ApprovalStatus.IN_PROGRESS]: 'blue',
    };
    return statusMap[status] || 'default';
  };

  const getTypeColor = (type: string): string => {
    const typeMap: { [key: string]: string } = {
      [ApprovalItemType.INITIATIVE]: 'purple',
      [ApprovalItemType.RETURN_ORDER]: 'cyan',
      [ApprovalItemType.LEAVE_REQUEST]: 'magenta',
      [ApprovalItemType.CREDIT_OVERRIDE]: 'gold',
    };
    return typeMap[type] || 'default';
  };

  const columns: ColumnsType<ViewChangeRequestApproval> = [
    {
      title: 'Request ID',
      dataIndex: 'requestID',
      key: 'requestID',
      width: 120,
      fixed: 'left',
      render: (text) => <span className="font-mono text-sm">{text}</span>
    },
    {
      title: 'Type',
      dataIndex: 'linkedItemType',
      key: 'linkedItemType',
      width: 150,
      filters: Object.values(ApprovalItemType).map(type => ({ text: type, value: type })),
      onFilter: (value, record) => record.linkedItemType === value,
      render: (type) => <Tag color={getTypeColor(type)}>{type}</Tag>
    },
    {
      title: 'Description',
      dataIndex: 'itemDescription',
      key: 'itemDescription',
      ellipsis: true,
      render: (text, record) => text || record.linkedItemUID
    },
    {
      title: 'Requester',
      dataIndex: 'requesterName',
      key: 'requesterName',
      width: 150,
      ellipsis: true
    },
    {
      title: 'Current Approver',
      dataIndex: 'currentApproverName',
      key: 'currentApproverName',
      width: 150,
      ellipsis: true
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      filters: Object.values(ApprovalStatus).map(status => ({ text: status, value: status })),
      onFilter: (value, record) => record.status === value,
      render: (status) => <Tag color={getStatusColor(status)}>{status}</Tag>
    },
    {
      title: 'Created On',
      dataIndex: 'createdOn',
      key: 'createdOn',
      width: 160,
      sorter: (a, b) => new Date(a.createdOn).getTime() - new Date(b.createdOn).getTime(),
      render: (date) => date ? format(new Date(date), 'MMM dd, yyyy HH:mm') : 'N/A'
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 200,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          {onView && (
            <Button
              type="link"
              size="small"
              icon={<EyeOutlined />}
              onClick={() => onView(record)}
            >
              View
            </Button>
          )}
          {showActions && record.status === ApprovalStatus.PENDING && (
            <>
              {onApprove && (
                <Button
                  type="link"
                  size="small"
                  icon={<CheckCircleOutlined />}
                  className="text-green-600"
                  onClick={() => onApprove(record)}
                >
                  Approve
                </Button>
              )}
              {onReject && (
                <Button
                  type="link"
                  size="small"
                  danger
                  icon={<CloseCircleOutlined />}
                  onClick={() => onReject(record)}
                >
                  Reject
                </Button>
              )}
            </>
          )}
        </Space>
      )
    }
  ];

  // Filter data based on search and filters
  const filteredData = approvals.filter(approval => {
    const matchesSearch = !searchTerm ||
      approval.requestID?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      approval.requesterName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      approval.itemDescription?.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesStatus = selectedStatus.length === 0 || selectedStatus.includes(approval.status);
    const matchesType = selectedType.length === 0 || selectedType.includes(approval.linkedItemType);

    return matchesSearch && matchesStatus && matchesType;
  });

  return (
    <div className="approval-list">
      {/* Filters */}
      <div className="mb-4 flex gap-2 flex-wrap">
        <Input
          placeholder="Search by request ID, requester, or description"
          prefix={<SearchOutlined />}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          style={{ width: 300 }}
        />
        <Select
          mode="multiple"
          placeholder="Filter by status"
          value={selectedStatus}
          onChange={setSelectedStatus}
          style={{ width: 200 }}
          options={Object.values(ApprovalStatus).map(status => ({ label: status, value: status }))}
        />
        <Select
          mode="multiple"
          placeholder="Filter by type"
          value={selectedType}
          onChange={setSelectedType}
          style={{ width: 200 }}
          options={Object.values(ApprovalItemType).map(type => ({ label: type, value: type }))}
        />
        <Button
          icon={<FilterOutlined />}
          onClick={() => {
            setSearchTerm('');
            setSelectedStatus([]);
            setSelectedType([]);
          }}
        >
          Clear Filters
        </Button>
      </div>

      {/* Table */}
      <Table
        columns={columns}
        dataSource={filteredData}
        loading={loading}
        rowKey={(record) => record.uid || record.requestID || Math.random().toString()}
        scroll={{ x: 1200 }}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} items`
        }}
        locale={{
          emptyText: approvals.length === 0
            ? 'No approval requests found. Check console for API response details.'
            : 'No items match your filters.'
        }}
      />
    </div>
  );
};

export default ApprovalList;
