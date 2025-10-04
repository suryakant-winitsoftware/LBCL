'use client';

import React, { useState } from 'react';
import { Modal, Form, Input, Select, Button, message, Space } from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  SwapOutlined
} from '@ant-design/icons';
import { ViewChangeRequestApproval } from '@/types/approval.types';

const { TextArea } = Input;

interface ApprovalActionModalProps {
  visible: boolean;
  approval: ViewChangeRequestApproval | null;
  actionType: 'approve' | 'reject' | 'reassign';
  onCancel: () => void;
  onSubmit: (values: { comments?: string; reassignTo?: string }) => Promise<void>;
  users?: Array<{ userId: string; userName: string }>;
}

export const ApprovalActionModal: React.FC<ApprovalActionModalProps> = ({
  visible,
  approval,
  actionType,
  onCancel,
  onSubmit,
  users = []
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const getTitle = () => {
    switch (actionType) {
      case 'approve':
        return 'Approve Request';
      case 'reject':
        return 'Reject Request';
      case 'reassign':
        return 'Reassign Request';
      default:
        return 'Action';
    }
  };

  const getIcon = () => {
    switch (actionType) {
      case 'approve':
        return <CheckCircleOutlined className="text-green-500" />;
      case 'reject':
        return <CloseCircleOutlined className="text-red-500" />;
      case 'reassign':
        return <SwapOutlined className="text-blue-500" />;
      default:
        return null;
    }
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setLoading(true);
      await onSubmit(values);
      form.resetFields();
      message.success(`Request ${actionType}d successfully`);
    } catch (error) {
      console.error('Error submitting action:', error);
      message.error(`Failed to ${actionType} request`);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    form.resetFields();
    onCancel();
  };

  return (
    <Modal
      title={
        <Space>
          {getIcon()}
          <span>{getTitle()}</span>
        </Space>
      }
      open={visible}
      onCancel={handleCancel}
      footer={[
        <Button key="cancel" onClick={handleCancel}>
          Cancel
        </Button>,
        <Button
          key="submit"
          type="primary"
          loading={loading}
          onClick={handleSubmit}
          danger={actionType === 'reject'}
          className={actionType === 'approve' ? 'bg-green-500 hover:bg-green-600' : ''}
        >
          {actionType === 'approve' ? 'Approve' : actionType === 'reject' ? 'Reject' : 'Reassign'}
        </Button>
      ]}
      width={600}
    >
      {approval && (
        <div className="mb-4">
          <div className="bg-gray-50 p-3 rounded mb-4">
            <div className="text-sm mb-1">
              <strong>Request ID:</strong> {approval.requestID}
            </div>
            <div className="text-sm mb-1">
              <strong>Type:</strong> {approval.linkedItemType}
            </div>
            <div className="text-sm mb-1">
              <strong>Requester:</strong> {approval.requesterName || 'Unknown'}
            </div>
            {approval.itemDescription && (
              <div className="text-sm">
                <strong>Description:</strong> {approval.itemDescription}
              </div>
            )}
          </div>

          <Form
            form={form}
            layout="vertical"
            initialValues={{
              comments: '',
              reassignTo: undefined
            }}
          >
            {actionType === 'reassign' && (
              <Form.Item
                name="reassignTo"
                label="Reassign To"
                rules={[{ required: true, message: 'Please select a user to reassign to' }]}
              >
                <Select
                  placeholder="Select user"
                  showSearch
                  optionFilterProp="children"
                  filterOption={(input, option) =>
                    (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                  }
                  options={users.map(user => ({
                    label: user.userName,
                    value: user.userId
                  }))}
                />
              </Form.Item>
            )}

            <Form.Item
              name="comments"
              label="Comments"
              rules={[
                {
                  required: actionType === 'reject',
                  message: 'Please provide a reason for rejection'
                }
              ]}
            >
              <TextArea
                rows={4}
                placeholder={
                  actionType === 'approve'
                    ? 'Add any comments (optional)'
                    : actionType === 'reject'
                    ? 'Please provide a reason for rejection'
                    : 'Add any comments about the reassignment'
                }
              />
            </Form.Item>
          </Form>

          {actionType === 'approve' && (
            <div className="bg-green-50 border border-green-200 rounded p-3 text-sm text-green-800">
              <strong>Note:</strong> Approving this request will move it to the next approval level
              or complete the approval process if this is the final level.
            </div>
          )}

          {actionType === 'reject' && (
            <div className="bg-red-50 border border-red-200 rounded p-3 text-sm text-red-800">
              <strong>Warning:</strong> Rejecting this request will cancel the entire approval
              process. The requester will be notified of the rejection.
            </div>
          )}

          {actionType === 'reassign' && (
            <div className="bg-blue-50 border border-blue-200 rounded p-3 text-sm text-blue-800">
              <strong>Note:</strong> Reassigning this request will transfer approval responsibility
              to the selected user.
            </div>
          )}
        </div>
      )}
    </Modal>
  );
};

export default ApprovalActionModal;
