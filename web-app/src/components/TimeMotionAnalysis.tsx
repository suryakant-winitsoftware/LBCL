'use client'
import React from 'react'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts'
import { colors, CHART_COLORS } from '../styles/colors'

interface TimeMotionAnalysisProps {
  salesmen: any[]
  selectedSalesman: string
  date: string
}

// Generate realistic time blocks for a salesman's day journey
const generateTimeBlocks = () => {
  const blocks = []
  const activityColors = {
    warehouse: colors.info.main,
    productive: colors.success.main,
    travel: colors.primary[500],
    break: colors.warning.main,
    idle: colors.error.main,
    admin: colors.gray[500]
  }

  // Customer names for realistic simulation
  const customers = [
    'Carrefour - City Centre', 'Spinneys - Marina Mall', 'Lulu Hypermarket - Barsha',
    'Union Coop - Jumeirah', 'Choithrams - Silicon Oasis', 'Geant - Ibn Battuta',
    'West Zone Supermarket', 'Al Maya - Discovery Gardens'
  ]

  let currentTime = 8 * 60 // Start at 8:00 AM in minutes
  const endTime = 18 * 60 // End at 6:00 PM

  // Leave warehouse
  blocks.push({
    startTime: '8:00',
    endTime: '8:15',
    activity: 'warehouse',
    color: activityColors.warehouse,
    duration: 15,
    description: 'Morning briefing & load vehicle',
    customer: null
  })
  currentTime = 8 * 60 + 15

  // Morning customers (3-4 visits)
  const morningCustomers = customers.slice(0, 4)
  for (let i = 0; i < morningCustomers.length && currentTime < 12 * 60; i++) {
    // Travel to customer
    const travelTime = Math.floor(Math.random() * 20) + 15 // 15-35 minutes
    const travelEndTime = Math.min(currentTime + travelTime, 12 * 60)
    blocks.push({
      startTime: `${Math.floor(currentTime / 60)}:${String(currentTime % 60).padStart(2, '0')}`,
      endTime: `${Math.floor(travelEndTime / 60)}:${String(travelEndTime % 60).padStart(2, '0')}`,
      activity: 'travel',
      color: activityColors.travel,
      duration: travelEndTime - currentTime,
      description: `Travel to ${morningCustomers[i]}`,
      customer: null
    })
    currentTime = travelEndTime

    if (currentTime >= 12 * 60) break

    // Visit customer
    const visitTime = Math.floor(Math.random() * 30) + 30 // 30-60 minutes
    const visitEndTime = Math.min(currentTime + visitTime, 12 * 60)
    blocks.push({
      startTime: `${Math.floor(currentTime / 60)}:${String(currentTime % 60).padStart(2, '0')}`,
      endTime: `${Math.floor(visitEndTime / 60)}:${String(visitEndTime % 60).padStart(2, '0')}`,
      activity: 'productive',
      color: activityColors.productive,
      duration: visitEndTime - currentTime,
      description: 'Customer visit',
      customer: morningCustomers[i]
    })
    currentTime = visitEndTime
  }

  // Lunch break
  if (currentTime < 13 * 60) {
    blocks.push({
      startTime: `${Math.floor(currentTime / 60)}:${String(currentTime % 60).padStart(2, '0')}`,
      endTime: '13:00',
      activity: 'break',
      color: activityColors.break,
      duration: 13 * 60 - currentTime,
      description: 'Lunch break',
      customer: null
    })
    currentTime = 13 * 60
  }

  // Afternoon customers (3-4 visits)
  const afternoonCustomers = customers.slice(4, 8)
  for (let i = 0; i < afternoonCustomers.length && currentTime < 17 * 60; i++) {
    // Travel to customer
    const travelTime = Math.floor(Math.random() * 20) + 15 // 15-35 minutes
    const travelEndTime = Math.min(currentTime + travelTime, 17 * 60)
    blocks.push({
      startTime: `${Math.floor(currentTime / 60)}:${String(currentTime % 60).padStart(2, '0')}`,
      endTime: `${Math.floor(travelEndTime / 60)}:${String(travelEndTime % 60).padStart(2, '0')}`,
      activity: 'travel',
      color: activityColors.travel,
      duration: travelEndTime - currentTime,
      description: `Travel to ${afternoonCustomers[i]}`,
      customer: null
    })
    currentTime = travelEndTime

    if (currentTime >= 17 * 60) break

    // Visit customer
    const visitTime = Math.floor(Math.random() * 30) + 25 // 25-55 minutes
    const visitEndTime = Math.min(currentTime + visitTime, 17 * 60)
    blocks.push({
      startTime: `${Math.floor(currentTime / 60)}:${String(currentTime % 60).padStart(2, '0')}`,
      endTime: `${Math.floor(visitEndTime / 60)}:${String(visitEndTime % 60).padStart(2, '0')}`,
      activity: 'productive',
      color: activityColors.productive,
      duration: visitEndTime - currentTime,
      description: 'Customer visit',
      customer: afternoonCustomers[i]
    })
    currentTime = visitEndTime
  }

  // Return to warehouse
  if (currentTime < 17 * 60 + 30) {
    const travelTime = Math.floor(Math.random() * 15) + 20 // 20-35 minutes back
    blocks.push({
      startTime: `${Math.floor(currentTime / 60)}:${String(currentTime % 60).padStart(2, '0')}`,
      endTime: `${Math.floor((currentTime + travelTime) / 60)}:${String((currentTime + travelTime) % 60).padStart(2, '0')}`,
      activity: 'travel',
      color: activityColors.travel,
      duration: travelTime,
      description: 'Return to warehouse',
      customer: null
    })
    currentTime += travelTime
  }

  // End of day at warehouse
  if (currentTime < endTime) {
    blocks.push({
      startTime: `${Math.floor(currentTime / 60)}:${String(currentTime % 60).padStart(2, '0')}`,
      endTime: '18:00',
      activity: 'warehouse',
      color: activityColors.warehouse,
      duration: endTime - currentTime,
      description: 'Unload vehicle & daily report',
      customer: null
    })
  }

  return blocks
}

// Generate daily summary data from time blocks
const generateDailySummary = (timeBlocks: any[]) => {
  const summary = {
    productive: 0,
    travel: 0,
    break: 0,
    warehouse: 0,
    idle: 0,
    admin: 0
  }

  timeBlocks.forEach(block => {
    if (summary[block.activity] !== undefined) {
      summary[block.activity] += block.duration
    }
  })

  return [
    { name: 'Productive', value: summary.productive, color: colors.success.main },
    { name: 'Travel', value: summary.travel, color: colors.primary[500] },
    { name: 'Break', value: summary.break, color: colors.warning.main },
    { name: 'Warehouse', value: summary.warehouse, color: colors.info.main },
    { name: 'Idle', value: summary.idle, color: colors.error.main },
    { name: 'Admin', value: summary.admin, color: colors.gray[500] }
  ].filter(item => item.value > 0) // Only show activities that occurred
}

// Generate hourly productivity data
const generateHourlyProductivity = () => {
  return [
    { hour: '8 AM', productive: 45, travel: 15, other: 0 },
    { hour: '9 AM', productive: 50, travel: 10, other: 0 },
    { hour: '10 AM', productive: 55, travel: 5, other: 0 },
    { hour: '11 AM', productive: 45, travel: 10, other: 5 },
    { hour: '12 PM', productive: 30, travel: 10, other: 20 },
    { hour: '1 PM', productive: 20, travel: 5, other: 35 },
    { hour: '2 PM', productive: 40, travel: 15, other: 5 },
    { hour: '3 PM', productive: 50, travel: 10, other: 0 },
    { hour: '4 PM', productive: 45, travel: 10, other: 5 },
    { hour: '5 PM', productive: 35, travel: 20, other: 5 }
  ]
}

export const TimeMotionAnalysis: React.FC<TimeMotionAnalysisProps> = ({ salesmen, selectedSalesman, date }) => {
  const timeBlocks = generateTimeBlocks()
  const dailySummary = generateDailySummary(timeBlocks)
  const hourlyData = generateHourlyProductivity()

  const totalMinutes = dailySummary.reduce((sum, item) => sum + item.value, 0)

  const filteredSalesmen = selectedSalesman === 'all'
    ? salesmen
    : salesmen.filter(sm => sm.id === selectedSalesman)

  return (
    <div>
      <h3 style={{ fontSize: '18px', fontWeight: '600', marginBottom: '20px', color: colors.gray[900] }}>
        Time & Motion Analysis - {date}
      </h3>

      {/* Summary Cards */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
        gap: '12px',
        marginBottom: '24px'
      }}>
        {dailySummary.map(item => (
          <div key={item.name} style={{
            padding: '16px',
            borderRadius: '8px',
            backgroundColor: colors.background.secondary,
            border: `1px solid ${colors.gray[200]}`
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
              <div style={{
                width: '8px',
                height: '8px',
                borderRadius: '50%',
                backgroundColor: item.color
              }} />
              <span style={{ fontSize: '12px', color: colors.gray[600], fontWeight: '500' }}>
                {item.name}
              </span>
            </div>
            <div style={{ fontSize: '20px', fontWeight: '700', color: colors.gray[900] }}>
              {Math.floor(item.value / 60)}h {item.value % 60}m
            </div>
            <div style={{ fontSize: '11px', color: colors.gray[500], marginTop: '4px' }}>
              {((item.value / totalMinutes) * 100).toFixed(1)}% of day
            </div>
          </div>
        ))}
      </div>

      {/* Main Content Grid */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: '24px', marginBottom: '24px' }}>
        {/* Timeline View */}
        <div style={{
          padding: '20px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <h4 style={{ fontSize: '14px', fontWeight: '600', marginBottom: '20px', color: colors.gray[800] }}>
            Daily Timeline
          </h4>

          {/* Time blocks visualization */}
          <div style={{
            display: 'flex',
            height: '60px',
            borderRadius: '8px',
            overflow: 'hidden',
            border: `1px solid ${colors.gray[200]}`,
            marginBottom: '24px'
          }}>
            {timeBlocks.map((block, index) => (
              <div
                key={index}
                style={{
                  flex: block.duration,
                  backgroundColor: block.color,
                  position: 'relative',
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}
                title={`${block.description || block.activity}: ${block.startTime} - ${block.endTime}${block.customer ? ` (${block.customer})` : ''}`}
              >
                {block.duration > 20 && (
                  <div style={{
                    fontSize: '11px',
                    color: 'white',
                    fontWeight: '600',
                    whiteSpace: 'nowrap',
                    textOverflow: 'ellipsis',
                    overflow: 'hidden',
                    padding: '0 4px',
                    maxWidth: '100%'
                  }}>
                    {block.activity === 'productive' && block.customer ?
                      block.customer.split(' - ')[0] :
                      block.activity === 'travel' ? 'TRA' :
                      block.activity === 'break' ? 'BRE' :
                      block.activity === 'warehouse' ? 'WAR' :
                      block.activity.substring(0, 3).toUpperCase()}
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* Activity List */}
          <div style={{ marginTop: '24px' }}>
            <h5 style={{ fontSize: '13px', fontWeight: '600', marginBottom: '12px', color: colors.gray[700] }}>
              Activity Log
            </h5>
            <div style={{ maxHeight: '200px', overflowY: 'auto' }}>
              {timeBlocks.map((block, index) => (
                <div key={index} style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '12px',
                  padding: '8px',
                  borderBottom: `1px solid ${colors.gray[100]}`,
                  fontSize: '12px'
                }}>
                  <div style={{
                    width: '8px',
                    height: '8px',
                    borderRadius: '50%',
                    backgroundColor: block.color,
                    flexShrink: 0
                  }} />
                  <span style={{ color: colors.gray[600], width: '100px' }}>
                    {block.startTime} - {block.endTime}
                  </span>
                  <span style={{ color: colors.gray[700], fontWeight: '500', flex: 1 }}>
                    {block.description || (
                      block.activity === 'productive' ? 'Customer Visit' :
                      block.activity === 'travel' ? 'Travel' :
                      block.activity === 'break' ? 'Break' :
                      block.activity === 'warehouse' ? 'Warehouse' :
                      block.activity === 'idle' ? 'Idle Time' : 'Admin Work'
                    )}
                  </span>
                  {block.customer && (
                    <span style={{ color: colors.gray[600], fontSize: '11px' }}>
                      {block.customer}
                    </span>
                  )}
                  <span style={{ color: colors.gray[500], fontSize: '11px' }}>
                    {Math.floor(block.duration / 60)}h {block.duration % 60}m
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Two Column Grid for Charts */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px', marginBottom: '24px' }}>
        {/* Pie Chart */}
        <div style={{
          padding: '20px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <h4 style={{ fontSize: '14px', fontWeight: '600', marginBottom: '20px', color: colors.gray[800] }}>
            Time Distribution
          </h4>
          <ResponsiveContainer width="100%" height={200}>
            <PieChart>
              <Pie
                data={dailySummary}
                cx="50%"
                cy="50%"
                innerRadius={60}
                outerRadius={80}
                paddingAngle={2}
                dataKey="value"
              >
                {dailySummary.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip formatter={(value: any) => `${Math.floor(value / 60)}h ${value % 60}m`} />
            </PieChart>
          </ResponsiveContainer>

          {/* Legend */}
          <div style={{ marginTop: '16px' }}>
            {dailySummary.map(item => (
              <div key={item.name} style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '8px'
              }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <div style={{
                    width: '12px',
                    height: '12px',
                    borderRadius: '2px',
                    backgroundColor: item.color
                  }} />
                  <span style={{ fontSize: '12px', color: colors.gray[600] }}>
                    {item.name}
                  </span>
                </div>
                <span style={{ fontSize: '12px', color: colors.gray[800], fontWeight: '600' }}>
                  {((item.value / totalMinutes) * 100).toFixed(0)}%
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Hourly Productivity Chart */}
        <div style={{
          padding: '20px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <h4 style={{ fontSize: '14px', fontWeight: '600', marginBottom: '20px', color: colors.gray[800] }}>
            Hourly Productivity Breakdown
          </h4>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={hourlyData}>
              <CartesianGrid strokeDasharray="3 3" stroke={colors.gray[200]} />
              <XAxis dataKey="hour" stroke={colors.gray[400]} tick={{ fill: colors.gray[500], fontSize: 11 }} />
              <YAxis stroke={colors.gray[400]} tick={{ fill: colors.gray[500], fontSize: 11 }} />
              <Tooltip
                contentStyle={{
                  backgroundColor: colors.background.primary,
                  border: `1px solid ${colors.gray[200]}`,
                  borderRadius: '6px'
                }}
              />
              <Bar dataKey="productive" stackId="a" fill={colors.success.main} name="Productive" />
              <Bar dataKey="travel" stackId="a" fill={colors.primary[500]} name="Travel" />
              <Bar dataKey="other" stackId="a" fill={colors.gray[400]} name="Other" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Performance Metrics */}
      <div style={{
        marginTop: '24px',
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
        gap: '16px'
      }}>
        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <div style={{ fontSize: '13px', color: colors.gray[600], marginBottom: '8px' }}>
            Productivity Score
          </div>
          <div style={{ fontSize: '28px', fontWeight: '700', color: colors.success.main }}>
            {(() => {
              const productive = dailySummary.find(item => item.name === 'Productive')?.value || 0
              const total = dailySummary.reduce((sum, item) => sum + item.value, 0)
              const score = total > 0 ? Math.round((productive / total) * 100) : 0
              return `${score}%`
            })()}
          </div>
          <div style={{
            marginTop: '8px',
            height: '4px',
            backgroundColor: colors.gray[200],
            borderRadius: '2px',
            overflow: 'hidden'
          }}>
            <div style={{
              width: (() => {
                const productive = dailySummary.find(item => item.name === 'Productive')?.value || 0
                const total = dailySummary.reduce((sum, item) => sum + item.value, 0)
                return total > 0 ? `${(productive / total) * 100}%` : '0%'
              })(),
              height: '100%',
              backgroundColor: colors.success.main
            }} />
          </div>
        </div>

        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <div style={{ fontSize: '13px', color: colors.gray[600], marginBottom: '8px' }}>
            Customer Visits
          </div>
          <div style={{ fontSize: '28px', fontWeight: '700', color: colors.primary[500] }}>
            {timeBlocks.filter(block => block.activity === 'productive').length}
          </div>
          <div style={{ fontSize: '11px', color: colors.gray[500], marginTop: '4px' }}>
            Total visits today
          </div>
        </div>

        <div style={{
          padding: '16px',
          borderRadius: '8px',
          backgroundColor: colors.background.secondary,
          border: `1px solid ${colors.gray[200]}`
        }}>
          <div style={{ fontSize: '13px', color: colors.gray[600], marginBottom: '8px' }}>
            Travel Time
          </div>
          <div style={{ fontSize: '28px', fontWeight: '700', color: colors.gray[900] }}>
            {(() => {
              const travel = dailySummary.find(item => item.name === 'Travel')?.value || 0
              return `${Math.floor(travel / 60)}h ${travel % 60}m`
            })()}
          </div>
          <div style={{ fontSize: '11px', color: colors.gray[500], marginTop: '4px' }}>
            Total travel duration
          </div>
        </div>
      </div>
    </div>
  )
}