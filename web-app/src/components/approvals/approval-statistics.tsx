'use client';

import React from 'react';
import { Card, Row, Col, Statistic, Progress } from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  FileDoneOutlined
} from '@ant-design/icons';
import { ApprovalStatistics as ApprovalStats } from '@/types/approval.types';

interface ApprovalStatisticsProps {
  statistics: ApprovalStats;
  loading?: boolean;
}

export const ApprovalStatistics: React.FC<ApprovalStatisticsProps> = ({
  statistics,
  loading = false
}) => {
  const total = statistics.totalPending + statistics.totalApproved + statistics.totalRejected;
  const approvalRate = total > 0 ? (statistics.totalApproved / total) * 100 : 0;
  const rejectionRate = total > 0 ? (statistics.totalRejected / total) * 100 : 0;

  return (
    <div className="approval-statistics">
      <Row gutter={[16, 16]}>
        {/* Total Pending */}
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Pending Approvals"
              value={statistics.totalPending}
              prefix={<ClockCircleOutlined className="text-orange-500" />}
              valueStyle={{ color: '#fa8c16' }}
              loading={loading}
            />
          </Card>
        </Col>

        {/* Total Approved */}
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Approved"
              value={statistics.totalApproved}
              prefix={<CheckCircleOutlined className="text-green-500" />}
              valueStyle={{ color: '#52c41a' }}
              loading={loading}
            />
          </Card>
        </Col>

        {/* Total Rejected */}
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Rejected"
              value={statistics.totalRejected}
              prefix={<CloseCircleOutlined className="text-red-500" />}
              valueStyle={{ color: '#ff4d4f' }}
              loading={loading}
            />
          </Card>
        </Col>

        {/* Total Requests */}
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Requests"
              value={total}
              prefix={<FileDoneOutlined />}
              loading={loading}
            />
          </Card>
        </Col>
      </Row>

      {/* Approval Rate */}
      <Row gutter={[16, 16]} className="mt-4">
        <Col xs={24} lg={12}>
          <Card title="Approval Rate">
            <Progress
              percent={Number(approvalRate.toFixed(1))}
              strokeColor="#52c41a"
              format={(percent) => `${percent}%`}
            />
            <div className="text-sm text-gray-500 mt-2">
              {statistics.totalApproved} out of {total} requests approved
            </div>
          </Card>
        </Col>

        <Col xs={24} lg={12}>
          <Card title="Rejection Rate">
            <Progress
              percent={Number(rejectionRate.toFixed(1))}
              strokeColor="#ff4d4f"
              format={(percent) => `${percent}%`}
            />
            <div className="text-sm text-gray-500 mt-2">
              {statistics.totalRejected} out of {total} requests rejected
            </div>
          </Card>
        </Col>
      </Row>

      {/* By Type Breakdown */}
      {Object.keys(statistics.byType).length > 0 && (
        <Card title="Approvals by Type" className="mt-4">
          <Row gutter={[16, 16]}>
            {Object.entries(statistics.byType).map(([type, counts]) => (
              <Col xs={24} sm={12} lg={8} key={type}>
                <Card size="small" className="bg-gray-50">
                  <div className="text-center">
                    <div className="text-lg font-semibold mb-2">{type}</div>
                    <div className="flex justify-around text-sm">
                      <div>
                        <div className="text-orange-500 font-bold">{counts?.pending || 0}</div>
                        <div className="text-gray-500">Pending</div>
                      </div>
                      <div>
                        <div className="text-green-500 font-bold">{counts?.approved || 0}</div>
                        <div className="text-gray-500">Approved</div>
                      </div>
                      <div>
                        <div className="text-red-500 font-bold">{counts?.rejected || 0}</div>
                        <div className="text-gray-500">Rejected</div>
                      </div>
                    </div>
                  </div>
                </Card>
              </Col>
            ))}
          </Row>
        </Card>
      )}
    </div>
  );
};

export default ApprovalStatistics;
