'use client';

import React, { useState, useEffect } from 'react';
import { Table, Button, Space, Tag, message, Modal, Form, Input, Select } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';

interface RuleMaster {
  ruleId: number;
  ruleName: string;
  ruleType: string;
  isActive: boolean;
  createdBy?: string;
  createdOn?: string;
  modifiedBy?: string;
  modifiedOn?: string;
}

export const RuleMasterList: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [dataSource, setDataSource] = useState<RuleMaster[]>([]);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingRule, setEditingRule] = useState<RuleMaster | null>(null);
  const [form] = Form.useForm();

  useEffect(() => {
    loadRules();
  }, []);

  const loadRules = async () => {
    setLoading(true);
    try {
      // TODO: Replace with actual API call
      // const response = await approvalService.getRuleMasterData();
      // setDataSource(response);

      // Mock data for now
      setDataSource([
        {
          ruleId: 1,
          ruleName: 'Store Creation Approval',
          ruleType: 'Store',
          isActive: true,
          createdBy: 'Admin',
          createdOn: '2025-01-01'
        }
      ]);
    } catch (error) {
      console.error('Error loading rules:', error);
      message.error('Failed to load approval rules');
    } finally {
      setLoading(false);
    }
  };

  const handleAdd = () => {
    setEditingRule(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (record: RuleMaster) => {
    setEditingRule(record);
    form.setFieldsValue(record);
    setModalVisible(true);
  };

  const handleDelete = async (record: RuleMaster) => {
    Modal.confirm({
      title: 'Delete Rule',
      content: `Are you sure you want to delete "${record.ruleName}"?`,
      onOk: async () => {
        try {
          // TODO: Call delete API
          message.success('Rule deleted successfully');
          loadRules();
        } catch (error) {
          message.error('Failed to delete rule');
        }
      }
    });
  };

  const handleSubmit = async (values: any) => {
    try {
      if (editingRule) {
        // TODO: Call update API
        message.success('Rule updated successfully');
      } else {
        // TODO: Call create API
        message.success('Rule created successfully');
      }
      setModalVisible(false);
      loadRules();
    } catch (error) {
      message.error('Failed to save rule');
    }
  };

  const columns: ColumnsType<RuleMaster> = [
    {
      title: 'Rule ID',
      dataIndex: 'ruleId',
      key: 'ruleId',
      width: 100,
    },
    {
      title: 'Rule Name',
      dataIndex: 'ruleName',
      key: 'ruleName',
    },
    {
      title: 'Rule Type',
      dataIndex: 'ruleType',
      key: 'ruleType',
      render: (type) => <Tag color="blue">{type}</Tag>,
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
    {
      title: 'Created By',
      dataIndex: 'createdBy',
      key: 'createdBy',
    },
    {
      title: 'Created On',
      dataIndex: 'createdOn',
      key: 'createdOn',
      render: (date) => date ? new Date(date).toLocaleDateString() : '-',
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            Edit
          </Button>
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDelete(record)}
          >
            Delete
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between' }}>
        <h2>Approval Rules</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
          Add Rule
        </Button>
      </div>

      <Table
        columns={columns}
        dataSource={dataSource}
        loading={loading}
        rowKey="ruleId"
        pagination={{ pageSize: 10 }}
      />

      <Modal
        title={editingRule ? 'Edit Rule' : 'Add New Rule'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        width={600}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            name="ruleName"
            label="Rule Name"
            rules={[{ required: true, message: 'Please enter rule name' }]}
          >
            <Input placeholder="Enter rule name" />
          </Form.Item>

          <Form.Item
            name="ruleType"
            label="Rule Type"
            rules={[{ required: true, message: 'Please select rule type' }]}
          >
            <Select placeholder="Select rule type">
              <Select.Option value="Store">Store</Select.Option>
              <Select.Option value="Contact">Contact</Select.Option>
              <Select.Option value="Address">Address</Select.Option>
              <Select.Option value="Other">Other</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="isActive"
            label="Status"
            initialValue={true}
          >
            <Select>
              <Select.Option value={true}>Active</Select.Option>
              <Select.Option value={false}>Inactive</Select.Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};
