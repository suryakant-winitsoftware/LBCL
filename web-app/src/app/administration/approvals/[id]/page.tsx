'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Card, Row, Col, Button, Spin, Tag, Descriptions, Space, message } from 'antd';
import {
  ArrowLeftOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SwapOutlined
} from '@ant-design/icons';
import {
  ApprovalTimeline,
  ApprovalActionModal
} from '@/components/approvals';
import {
  ApprovalDetail,
  ViewChangeRequestApproval,
  ApprovalStatus
} from '@/types/approval.types';
import { approvalService } from '@/services/approval.service';
import { format } from 'date-fns';

export default function ApprovalDetailPage() {
  const params = useParams();
  const router = useRouter();
  const approvalId = params.id as string;

  const [loading, setLoading] = useState(true);
  const [approvalDetail, setApprovalDetail] = useState<ApprovalDetail | null>(null);
  const [actionModalVisible, setActionModalVisible] = useState(false);
  const [actionType, setActionType] = useState<'approve' | 'reject' | 'reassign'>('approve');

  useEffect(() => {
    // Only load if approvalId is a valid UID/number, not a route path
    if (approvalId && approvalId !== 'approvals' && !approvalId.includes('tab=')) {
      loadApprovalDetail();
    } else {
      // Invalid or missing ID - show error state instead of redirecting
      setLoading(false);
      setApprovalDetail(null);
    }
  }, [approvalId]);

  const loadApprovalDetail = async () => {
    setLoading(true);
    try {
      const detail = await approvalService.getCompleteApprovalDetail(approvalId);
      setApprovalDetail(detail);
    } catch (error) {
      console.error('Error loading approval detail:', error);
      message.error('Failed to load approval details');
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = () => {
    setActionType('approve');
    setActionModalVisible(true);
  };

  const handleReject = () => {
    setActionType('reject');
    setActionModalVisible(true);
  };

  const handleReassign = () => {
    setActionType('reassign');
    setActionModalVisible(true);
  };

  const handleActionSubmit = async (values: { comments?: string; reassignTo?: string }) => {
    if (!approvalDetail) return;

    try {
      const actionRequest = {
        requestId: approvalId,
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
      loadApprovalDetail();
      message.success(`Request ${actionType}d successfully`);
    } catch (error) {
      console.error('Error submitting action:', error);
      throw error;
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <Spin size="large" />
      </div>
    );
  }

  if (!approvalDetail) {
    return (
      <div className="p-6">
        <Card>
          <div className="text-center py-8">
            <p className="text-gray-500">Approval not found</p>
            <Button
              type="primary"
              onClick={() => router.push('/administration/approvals')}
              className="mt-4"
            >
              Back to Approvals
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  const { request, linkedItem, hierarchy, logs } = approvalDetail;
  const isPending = (linkedItem as ViewChangeRequestApproval).status === ApprovalStatus.PENDING;

  return (
    <div className="approval-detail-page p-6">
      {/* Header */}
      <div className="mb-6">
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => router.push('/administration/approvals')}
          className="mb-4"
        >
          Back to Approvals
        </Button>

        <div className="flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold mb-2">Approval Details</h1>
            <p className="text-gray-600">Request ID: {approvalId}</p>
          </div>

          {isPending && (
            <Space>
              <Button
                type="primary"
                icon={<CheckCircleOutlined />}
                onClick={handleApprove}
                className="bg-green-500 hover:bg-green-600"
              >
                Approve
              </Button>
              <Button
                danger
                icon={<CloseCircleOutlined />}
                onClick={handleReject}
              >
                Reject
              </Button>
              <Button
                icon={<SwapOutlined />}
                onClick={handleReassign}
              >
                Reassign
              </Button>
            </Space>
          )}
        </div>
      </div>

      <Row gutter={[16, 16]}>
        {/* Main Details */}
        <Col xs={24} lg={16}>
          <Card title="Request Information" className="mb-4">
            <Descriptions column={1} bordered>
              <Descriptions.Item label="Status">
                <Tag color={
                  (linkedItem as ViewChangeRequestApproval).status === ApprovalStatus.PENDING ? 'orange' :
                  (linkedItem as ViewChangeRequestApproval).status === ApprovalStatus.APPROVED ? 'green' :
                  (linkedItem as ViewChangeRequestApproval).status === ApprovalStatus.REJECTED ? 'red' : 'blue'
                }>
                  {(linkedItem as ViewChangeRequestApproval).status}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Type">
                <Tag color="blue">{linkedItem.linkedItemType}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Item UID">
                {linkedItem.linkedItemUID}
              </Descriptions.Item>
              <Descriptions.Item label="Requester">
                {(linkedItem as ViewChangeRequestApproval).requesterName || 'Unknown'}
              </Descriptions.Item>
              <Descriptions.Item label="Current Approver">
                {(linkedItem as ViewChangeRequestApproval).currentApproverName || 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Created On">
                {linkedItem.createdOn
                  ? format(new Date(linkedItem.createdOn), 'MMM dd, yyyy HH:mm:ss')
                  : 'N/A'}
              </Descriptions.Item>
              {(linkedItem as ViewChangeRequestApproval).comments && (
                <Descriptions.Item label="Comments">
                  {(linkedItem as ViewChangeRequestApproval).comments}
                </Descriptions.Item>
              )}
            </Descriptions>
          </Card>

          {/* Approval Hierarchy */}
          {hierarchy && hierarchy.length > 0 && (
            <Card title="Approval Hierarchy" className="mb-4">
              <div className="space-y-2">
                {hierarchy.map((level, index) => (
                  <div
                    key={level.id}
                    className="flex items-center justify-between p-3 bg-gray-50 rounded"
                  >
                    <div>
                      <div className="font-semibold">
                        Level {level.level}: {level.approverName || level.approverId}
                      </div>
                      {level.roleName && (
                        <div className="text-sm text-gray-500">Role: {level.roleName}</div>
                      )}
                      {level.approverEmail && (
                        <div className="text-sm text-gray-500">{level.approverEmail}</div>
                      )}
                    </div>
                    <div>
                      {level.approverType && (
                        <Tag>{level.approverType}</Tag>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </Card>
          )}
        </Col>

        {/* Timeline */}
        <Col xs={24} lg={8}>
          <ApprovalTimeline logs={logs} loading={loading} />
        </Col>
      </Row>

      {/* Action Modal */}
      <ApprovalActionModal
        visible={actionModalVisible}
        approval={linkedItem as ViewChangeRequestApproval}
        actionType={actionType}
        onCancel={() => setActionModalVisible(false)}
        onSubmit={handleActionSubmit}
        users={[]} // TODO: Load users from API
      />
    </div>
  );
}
