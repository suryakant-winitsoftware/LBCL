'use client'
import React, { useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import { colors } from '../styles/colors'

interface JourneyComplianceProps {
  salesmen: any[]
  selectedSalesman: string
  date: string
}

// Generate planned vs actual visits
const generateJourneyData = (salesmanId: string) => {
  const customers = []
  const numCustomers = Math.floor(Math.random() * 8 + 12) // 12-20 customers

  for (let i = 0; i < numCustomers; i++) {
    const isVisited = Math.random() > 0.15 // 85% compliance rate
    const plannedTime = 9 + i * 0.5 // Every 30 minutes

    customers.push({
      id: `C${i + 1}`,
      name: `Customer ${Math.floor(Math.random() * 500)}`,
      type: ['Modern Trade', 'Traditional', 'HORECA', 'Wholesale'][Math.floor(Math.random() * 4)],
      plannedTime: `${Math.floor(plannedTime)}:${plannedTime % 1 === 0 ? '00' : '30'}`,
      actualTime: isVisited ?
        `${Math.floor(plannedTime + Math.random() * 0.5 - 0.25)}:${Math.floor(Math.random() * 60).toString().padStart(2, '0')}` :
        null,
      status: isVisited ?
        (Math.random() > 0.3 ? 'completed' : 'partial') :
        (Math.random() > 0.5 ? 'skipped' : 'pending'),
      duration: isVisited ? Math.floor(Math.random() * 30 + 15) : 0,
      productive: isVisited && Math.random() > 0.2,
      reason: !isVisited ?
        ['Customer closed', 'Rescheduled', 'Route changed', 'Time constraint'][Math.floor(Math.random() * 4)] :
        null,
      deviation: isVisited ? Math.floor(Math.random() * 30 - 15) : 0 // minutes early/late
    })
  }

  return customers
}

// Generate weekly compliance data
const generateWeeklyCompliance = () => {
  return [
    { day: 'Mon', planned: 18, actual: 16, compliance: 89 },
    { day: 'Tue', planned: 20, actual: 18, compliance: 90 },
    { day: 'Wed', planned: 19, actual: 17, compliance: 89 },
    { day: 'Thu', planned: 21, actual: 19, compliance: 90 },
    { day: 'Fri', planned: 18, actual: 15, compliance: 83 },
    { day: 'Sat', planned: 15, actual: 14, compliance: 93 },
    { day: 'Sun', planned: 0, actual: 0, compliance: 0 }
  ]
}

export const JourneyCompliance: React.FC<JourneyComplianceProps> = ({ salesmen, selectedSalesman, date }) => {
  const [viewMode, setViewMode] = useState<'calendar' | 'list'>('calendar')
  const journeyData = generateJourneyData(selectedSalesman)
  const weeklyData = generateWeeklyCompliance()

  const completedVisits = journeyData.filter(c => c.status === 'completed').length
  const partialVisits = journeyData.filter(c => c.status === 'partial').length
  const skippedVisits = journeyData.filter(c => c.status === 'skipped').length
  const pendingVisits = journeyData.filter(c => c.status === 'pending').length
  const totalPlanned = journeyData.length
  const complianceRate = ((completedVisits + partialVisits) / totalPlanned * 100).toFixed(1)

  const getStatusColor = (status: string) => {
    switch(status) {
      case 'completed': return colors.success.main
      case 'partial': return colors.warning.main
      case 'skipped': return colors.error.main
      case 'pending': return colors.gray[400]
      default: return colors.gray[300]
    }
  }

  return (
    <div>
      <h3 style={{ fontSize: '18px', fontWeight: '600', marginBottom: '20px', color: colors.gray[900] }}>
        Journey Plan Compliance - {date}
      </h3>

      {/* Compliance Summary */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
        gap: '12px',
        marginBottom: '24px'
      }}>
        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <div style={{ fontSize: '24px', fontWeight: '700', color: colors.gray[900] }}>
            {complianceRate}%
          </div>
          <div style={{ fontSize: '12px', color: colors.gray[500], marginTop: '4px' }}>
            Compliance Rate
          </div>
        </div>

        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.success.light,
          border: `1px solid ${colors.success.main}`
        }}>
          <div style={{ fontSize: '24px', fontWeight: '700', color: colors.success.dark }}>
            {completedVisits}
          </div>
          <div style={{ fontSize: '12px', color: colors.success.main, marginTop: '4px' }}>
            Completed
          </div>
        </div>

        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.warning.light,
          border: `1px solid ${colors.warning.main}`
        }}>
          <div style={{ fontSize: '24px', fontWeight: '700', color: colors.warning.dark }}>
            {partialVisits}
          </div>
          <div style={{ fontSize: '12px', color: colors.warning.main, marginTop: '4px' }}>
            Partial
          </div>
        </div>

        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.error.light,
          border: `1px solid ${colors.error.main}`
        }}>
          <div style={{ fontSize: '24px', fontWeight: '700', color: colors.error.dark }}>
            {skippedVisits}
          </div>
          <div style={{ fontSize: '12px', color: colors.error.main, marginTop: '4px' }}>
            Skipped
          </div>
        </div>

        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <div style={{ fontSize: '24px', fontWeight: '700', color: colors.gray[600] }}>
            {pendingVisits}
          </div>
          <div style={{ fontSize: '12px', color: colors.gray[500], marginTop: '4px' }}>
            Pending
          </div>
        </div>

        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <div style={{ fontSize: '24px', fontWeight: '700', color: colors.gray[900] }}>
            {totalPlanned}
          </div>
          <div style={{ fontSize: '12px', color: colors.gray[500], marginTop: '4px' }}>
            Total Planned
          </div>
        </div>
      </div>

      {/* View Toggle */}
      <div style={{ display: 'flex', gap: '12px', marginBottom: '20px' }}>
        <button
          onClick={() => setViewMode('calendar')}
          style={{
            padding: '8px 16px',
            borderRadius: '6px',
            border: `1px solid ${viewMode === 'calendar' ? colors.primary[500] : colors.gray[300]}`,
            backgroundColor: viewMode === 'calendar' ? colors.primary[50] : colors.background.primary,
            color: viewMode === 'calendar' ? colors.primary[600] : colors.gray[600],
            fontWeight: '500',
            cursor: 'pointer',
            fontSize: '13px'
          }}
        >
          Calendar View
        </button>
        <button
          onClick={() => setViewMode('list')}
          style={{
            padding: '8px 16px',
            borderRadius: '6px',
            border: `1px solid ${viewMode === 'list' ? colors.primary[500] : colors.gray[300]}`,
            backgroundColor: viewMode === 'list' ? colors.primary[50] : colors.background.primary,
            color: viewMode === 'list' ? colors.primary[600] : colors.gray[600],
            fontWeight: '500',
            cursor: 'pointer',
            fontSize: '13px'
          }}
        >
          List View
        </button>
      </div>

      {viewMode === 'calendar' ? (
        <>
          {/* Calendar/Timeline View */}
          <div style={{
            padding: '20px',
            borderRadius: '8px',
            backgroundColor: colors.background.secondary,
            border: `1px solid ${colors.gray[200]}`,
            marginBottom: '24px'
          }}>
            <h4 style={{ fontSize: '14px', fontWeight: '600', marginBottom: '20px', color: colors.gray[800] }}>
              Journey Plan Timeline
            </h4>

            {/* Time headers */}
            <div style={{
              display: 'grid',
              gridTemplateColumns: '120px repeat(14, 1fr)',
              gap: '4px',
              marginBottom: '8px',
              fontSize: '11px',
              color: colors.gray[500]
            }}>
              <div></div>
              {[8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18].map(hour => (
                <div key={hour} style={{ textAlign: 'center' }}>
                  {hour > 12 ? `${hour - 12}PM` : `${hour}AM`}
                </div>
              ))}
            </div>

            {/* Journey Timeline */}
            <div style={{
              display: 'grid',
              gridTemplateColumns: '120px 1fr',
              gap: '8px'
            }}>
              <div style={{ fontSize: '12px', fontWeight: '600', color: colors.gray[700], paddingTop: '8px' }}>
                Planned
              </div>
              <div style={{
                display: 'flex',
                height: '40px',
                backgroundColor: colors.gray[100],
                borderRadius: '4px',
                position: 'relative',
                alignItems: 'center'
              }}>
                {journeyData.map((customer, index) => {
                  const hourPosition = parseFloat(customer.plannedTime.replace(':', '.')) - 8
                  return (
                    <div
                      key={customer.id}
                      style={{
                        position: 'absolute',
                        left: `${(hourPosition / 10) * 100}%`,
                        width: '30px',
                        height: '30px',
                        backgroundColor: colors.primary[100],
                        border: `2px solid ${colors.primary[500]}`,
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '10px',
                        fontWeight: '600',
                        color: colors.primary[700],
                        cursor: 'pointer',
                        zIndex: 10 - index
                      }}
                      title={`${customer.name} - ${customer.plannedTime}`}
                    >
                      {index + 1}
                    </div>
                  )
                })}
              </div>
            </div>

            <div style={{
              display: 'grid',
              gridTemplateColumns: '120px 1fr',
              gap: '8px',
              marginTop: '12px'
            }}>
              <div style={{ fontSize: '12px', fontWeight: '600', color: colors.gray[700], paddingTop: '8px' }}>
                Actual
              </div>
              <div style={{
                display: 'flex',
                height: '40px',
                backgroundColor: colors.gray[100],
                borderRadius: '4px',
                position: 'relative',
                alignItems: 'center'
              }}>
                {journeyData.filter(c => c.actualTime).map((customer, index) => {
                  const hourPosition = parseFloat(customer.actualTime.replace(':', '.')) - 8
                  return (
                    <div
                      key={customer.id}
                      style={{
                        position: 'absolute',
                        left: `${(hourPosition / 10) * 100}%`,
                        width: '30px',
                        height: '30px',
                        backgroundColor: getStatusColor(customer.status) + '20',
                        border: `2px solid ${getStatusColor(customer.status)}`,
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '10px',
                        fontWeight: '600',
                        color: getStatusColor(customer.status),
                        cursor: 'pointer',
                        zIndex: 10 - index
                      }}
                      title={`${customer.name} - ${customer.actualTime} (${customer.status})`}
                    >
                      {journeyData.indexOf(customer) + 1}
                    </div>
                  )
                })}
              </div>
            </div>
          </div>

          {/* Weekly Compliance Chart */}
          <div style={{
            padding: '20px',
            borderRadius: '8px',
            backgroundColor: colors.background.secondary,
            border: `1px solid ${colors.gray[200]}`
          }}>
            <h4 style={{ fontSize: '14px', fontWeight: '600', marginBottom: '20px', color: colors.gray[800] }}>
              Weekly Compliance Trend
            </h4>
            <ResponsiveContainer width="100%" height={200}>
              <BarChart data={weeklyData}>
                <CartesianGrid strokeDasharray="3 3" stroke={colors.gray[200]} />
                <XAxis dataKey="day" stroke={colors.gray[400]} tick={{ fill: colors.gray[500], fontSize: 11 }} />
                <YAxis stroke={colors.gray[400]} tick={{ fill: colors.gray[500], fontSize: 11 }} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: colors.background.primary,
                    border: `1px solid ${colors.gray[200]}`,
                    borderRadius: '6px'
                  }}
                />
                <Bar dataKey="planned" fill={colors.gray[400]} name="Planned" />
                <Bar dataKey="actual" fill={colors.primary[500]} name="Actual" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </>
      ) : (
        /* List View */
        <div style={{
          padding: '20px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: `2px solid ${colors.gray[200]}` }}>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  #
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Customer
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Type
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Planned
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Actual
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Status
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Deviation
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Duration
                </th>
                <th style={{ padding: '12px', textAlign: 'left', fontSize: '12px', color: colors.gray[500], fontWeight: '600' }}>
                  Productive
                </th>
              </tr>
            </thead>
            <tbody>
              {journeyData.map((customer, index) => (
                <tr key={customer.id} style={{ borderBottom: `1px solid ${colors.gray[100]}` }}>
                  <td style={{ padding: '12px', fontSize: '13px', color: colors.gray[600] }}>
                    {index + 1}
                  </td>
                  <td style={{ padding: '12px', fontSize: '13px', color: colors.gray[700], fontWeight: '500' }}>
                    {customer.name}
                  </td>
                  <td style={{ padding: '12px', fontSize: '12px', color: colors.gray[600] }}>
                    {customer.type}
                  </td>
                  <td style={{ padding: '12px', fontSize: '13px', color: colors.gray[700] }}>
                    {customer.plannedTime}
                  </td>
                  <td style={{ padding: '12px', fontSize: '13px', color: colors.gray[700] }}>
                    {customer.actualTime || '-'}
                  </td>
                  <td style={{ padding: '12px' }}>
                    <span style={{
                      padding: '4px 8px',
                      borderRadius: '4px',
                      backgroundColor: getStatusColor(customer.status) + '20',
                      color: getStatusColor(customer.status),
                      fontSize: '11px',
                      fontWeight: '600'
                    }}>
                      {customer.status.toUpperCase()}
                    </span>
                  </td>
                  <td style={{ padding: '12px', fontSize: '12px' }}>
                    {customer.deviation !== 0 && (
                      <span style={{
                        color: Math.abs(customer.deviation) > 15 ? colors.error.main : colors.warning.main
                      }}>
                        {customer.deviation > 0 ? '+' : ''}{customer.deviation} min
                      </span>
                    )}
                    {customer.deviation === 0 && customer.actualTime && (
                      <span style={{ color: colors.success.main }}>On time</span>
                    )}
                    {!customer.actualTime && customer.reason && (
                      <span style={{ color: colors.gray[500], fontSize: '11px' }}>{customer.reason}</span>
                    )}
                  </td>
                  <td style={{ padding: '12px', fontSize: '12px', color: colors.gray[600] }}>
                    {customer.duration > 0 ? `${customer.duration} min` : '-'}
                  </td>
                  <td style={{ padding: '12px' }}>
                    {customer.productive !== null && (
                      <div style={{
                        width: '20px',
                        height: '20px',
                        borderRadius: '50%',
                        backgroundColor: customer.productive ? colors.success.main : colors.gray[300],
                        margin: '0 auto'
                      }} />
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}