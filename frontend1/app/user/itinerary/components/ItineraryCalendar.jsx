"use client"
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { 
  Plus,
  ChevronLeft,
  ChevronRight,
  Search,
  Calendar
} from 'lucide-react'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'

const ItineraryCalendar = () => {
  const { user } = useAuth()
  const router = useRouter()
  const [selectedView, setSelectedView] = useState('Day')
  const [currentMonth, setCurrentMonth] = useState('June 2025')

  const outlets = [
    { id: 'RT050105', name: 'Diyasenpura', channel: 'TOFT', chain: 'ROC', category: '80% Contribution Outlets (LION)', priority: 'Low', location: 'North Anuradhapura' },
    { id: 'RT050115', name: 'Gajamuthu Upali', channel: 'TOFT', chain: 'GRD', category: '80% Contribution Outlets (Heineken)', priority: 'High', location: 'North Anuradhapura' },
    { id: 'RT050117', name: 'Ajith Putha Ws - Rambewa', channel: 'TOFT', chain: 'AJP', category: 'Special Outlets', priority: 'High', location: 'North Anuradhapura' },
    { id: 'RT050163', name: 'Medawachtchiya', channel: 'TOFT', chain: 'N/A', category: 'Activity (National, Brand, Tourist Board Licenses)', priority: 'Medium', location: 'North Anuradhapura' },
    { id: 'RT050168', name: 'Midland Ws', channel: 'TOFT', chain: 'MID', category: 'Special Outlets', priority: 'High', location: 'North Anuradhapura' },
    { id: 'RT050215', name: 'Thambuttegama Ws', channel: 'TOFT', chain: 'ROC', category: '80% Contribution Outlets (Heineken)', priority: 'High', location: 'North Anuradhapura' },
    { id: 'RT050125', name: 'Grand WS', channel: 'TOFT', chain: 'GRD', category: 'Activity (National, Brand, Tourist Board Licenses)', priority: 'Priority', location: 'North Anuradhapura' },
    { id: 'RT050153', name: 'Luxman Wine Stores', channel: 'TOFT', chain: 'AJP', category: '80% Contribution Outlets (LION)', priority: 'Low', location: 'North Anuradhapura' },
    { id: 'RT050179', name: 'Nandana Kekirawa', channel: 'TOFT', chain: 'LUC-A', category: 'Activity (National, Brand, Tourist Board Licenses)', priority: 'Medium', location: 'North Anuradhapura' },
    { id: 'RT050194', name: 'New Medirigiriya', channel: 'TOFT', chain: 'N/A', category: '80% Contribution Outlets (Heineken)', priority: 'High', location: 'North Anuradhapura' }
  ]

  const getPriorityColor = (priority) => {
    switch (priority) {
      case 'High': return '#000000'
      case 'Medium': return '#375AE6'
      case 'Low': return '#375AE6'
      case 'Priority': return '#000000'
      default: return '#000000'
    }
  }


  const generateCalendarDays = () => {
    const days = []
    const startDate = new Date(2025, 5, 1) // June 2025
    const endDate = new Date(2025, 5, 30)
    
    for (let d = new Date(startDate); d <= endDate; d.setDate(d.getDate() + 1)) {
      days.push({
        date: d.getDate(),
        isWeekend: d.getDay() === 0 || d.getDay() === 6,
        isToday: d.getDate() === 2
      })
    }
    return days
  }

  const calendarDays = generateCalendarDays()

  return (
    <ProtectedRoute requiredSystem="itinerary">
      <BusinessLayout title="Preview Sales Itinerary Configuration">
        <div className="space-y-6">
          <div style={{ display: 'flex', gap: '24px' }}>
            {/* Left Panel - Calendar */}
            <div style={{ width: '300px' }}>
              <div style={{
                backgroundColor: '#ffffff',
                borderRadius: '8px',
                padding: '20px',
                boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
                marginBottom: '16px'
              }}>
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '8px',
                  marginBottom: '20px'
                }}>
                  <button style={{
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer',
                    padding: '4px'
                  }}>
                    <Plus size={16} style={{ color: '#375AE6' }} />
                  </button>
                </div>

                {/* Calendar Header */}
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  marginBottom: '20px'
                }}>
                  <button style={{
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer'
                  }}>
                    <ChevronLeft size={20} />
                  </button>
                  <h2 style={{
                    fontSize: '18px',
                    fontWeight: '600',
                    color: '#000000',
                    margin: 0
                  }}>
                    {currentMonth}
                  </h2>
                  <button style={{
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer'
                  }}>
                    <ChevronRight size={20} />
                  </button>
                </div>

                {/* Calendar Days Header */}
                <div style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(7, 1fr)',
                  gap: '2px',
                  marginBottom: '8px'
                }}>
                  {['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'].map(day => (
                    <div key={day} style={{
                      textAlign: 'center',
                      fontSize: '11px',
                      fontWeight: '500',
                      color: '#000000',
                      padding: '4px'
                    }}>
                      {day}
                    </div>
                  ))}
                </div>

                {/* Calendar Grid */}
                <div style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(7, 1fr)',
                  gap: '2px'
                }}>
                  {calendarDays.map((day, index) => (
                    <div key={index} style={{
                      aspectRatio: '1',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '13px',
                      fontWeight: day.isToday ? '600' : '500',
                      backgroundColor: day.isToday ? '#375AE6' : day.isWeekend ? '#ffffff' : 'transparent',
                      color: day.isToday ? '#ffffff' : day.isWeekend ? '#000000' : '#000000',
                      borderRadius: '4px',
                      cursor: 'pointer',
                      border: day.date === 2 ? '2px solid #375AE6' : 'none'
                    }}>
                      {day.date}
                    </div>
                  ))}
                  <div style={{
                    gridColumn: 'span 7',
                    display: 'flex',
                    justifyContent: 'space-between',
                    marginTop: '8px'
                  }}>
                    {[29, 30, 1, 2, 3, 4, 5].map(date => (
                      <div key={date} style={{
                        aspectRatio: '1',
                        width: '32px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '13px',
                        color: '#000000'
                      }}>
                        {date}
                      </div>
                    ))}
                  </div>
                </div>

                {/* Calendar Summary */}
                <div style={{
                  marginTop: '24px',
                  padding: '16px',
                  backgroundColor: '#ffffff',
                  borderRadius: '4px'
                }}>
                  <h4 style={{
                    fontSize: '14px',
                    fontWeight: '600',
                    color: '#000000',
                    marginBottom: '12px',
                    margin: 0
                  }}>
                    Scheduled Outlets
                  </h4>
                  <div style={{
                    fontSize: '32px',
                    fontWeight: 'bold',
                    color: '#375AE6',
                    textAlign: 'center',
                    marginBottom: '8px'
                  }}>
                    123/123
                  </div>
                  <div style={{
                    fontSize: '12px',
                    color: '#000000',
                    textAlign: 'center'
                  }}>
                    Total Qualified Outlets in Plan
                  </div>
                </div>

                {/* Configuration Summary */}
                <div style={{
                  marginTop: '16px',
                  fontSize: '12px',
                  color: '#000000'
                }}>
                  <h4 style={{
                    fontSize: '14px',
                    fontWeight: '600',
                    marginBottom: '8px'
                  }}>
                    Configuration Summary
                  </h4>
                  <div style={{ marginBottom: '4px' }}>
                    <span style={{ color: '#375AE6', fontWeight: '600' }}>14 ■</span> 80% Contribution Outlets (LION)
                  </div>
                  <div style={{ marginBottom: '4px' }}>
                    <span style={{ color: '#000000', fontWeight: '600' }}>75 ■</span> 80% Contribution Outlets (Heineken)
                  </div>
                  <div style={{ marginBottom: '4px' }}>
                    <span style={{ color: '#375AE6', fontWeight: '600' }}>30 ■</span> Special Outlets
                  </div>
                  <div>
                    <span style={{ color: '#000000', fontWeight: '600' }}>04 ■</span> Activity (National, Brand, Tourist board licenses)
                  </div>
                </div>

                {/* Submit Button */}
                <button style={{
                  width: '100%',
                  padding: '12px',
                  backgroundColor: '#375AE6',
                  color: '#ffffff',
                  border: 'none',
                  borderRadius: '4px',
                  fontSize: '14px',
                  fontWeight: '600',
                  cursor: 'pointer',
                  marginTop: '20px'
                }}>
                  Submit for Review
                </button>
              </div>
            </div>

            {/* Right Panel - Outlets Table */}
            <div style={{ flex: 1 }}>
              {/* View Controls */}
              <div style={{
                backgroundColor: '#e5e7eb',
                borderRadius: '4px',
                padding: '16px',
                marginBottom: '16px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between'
              }}>
                <div style={{ display: 'flex', gap: '8px' }}>
                  {['Day', 'Week', 'Month'].map(view => (
                    <button
                      key={view}
                      onClick={() => setSelectedView(view)}
                      style={{
                        padding: '8px 16px',
                        backgroundColor: selectedView === view ? '#375AE6' : '#ffffff',
                        color: selectedView === view ? '#ffffff' : '#000000',
                        border: 'none',
                        borderRadius: '4px',
                        fontSize: '14px',
                        fontWeight: '500',
                        cursor: 'pointer'
                      }}
                    >
                      {view}
                    </button>
                  ))}
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                  <div style={{
                    position: 'relative',
                    display: 'flex',
                    alignItems: 'center'
                  }}>
                    <Search size={16} style={{
                      position: 'absolute',
                      left: '12px',
                      color: '#000000'
                    }} />
                    <input
                      type="text"
                      placeholder="Search"
                      style={{
                        padding: '8px 12px 8px 36px',
                        border: '1px solid #d1d5db',
                        borderRadius: '4px',
                        fontSize: '14px',
                        width: '200px'
                      }}
                    />
                  </div>
                  <button style={{
                    padding: '8px 16px',
                    backgroundColor: '#375AE6',
                    color: '#ffffff',
                    border: 'none',
                    borderRadius: '4px',
                    fontSize: '14px',
                    fontWeight: '500',
                    cursor: 'pointer'
                  }}>
                    Add New Outlet
                  </button>
                </div>
              </div>

              {/* Table */}
              <div style={{
                backgroundColor: '#ffffff',
                borderRadius: '8px',
                overflow: 'hidden',
                boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)'
              }}>
                {/* Table Header */}
                <div style={{
                  display: 'grid',
                  gridTemplateColumns: '120px 1fr 80px 60px 150px 80px 120px 120px',
                  gap: '16px',
                  padding: '16px',
                  backgroundColor: '#ffffff',
                  borderBottom: '1px solid #e5e7eb',
                  fontSize: '12px',
                  fontWeight: '600',
                  color: '#000000'
                }}>
                  <div>Outlet Name</div>
                  <div>Channel</div>
                  <div>Chain</div>
                  <div>Category</div>
                  <div>Priority</div>
                  <div>Location</div>
                  <div>Change Date</div>
                </div>

                {/* Table Rows */}
                {outlets.map((outlet, index) => (
                  <div key={outlet.id} style={{
                    display: 'grid',
                    gridTemplateColumns: '120px 1fr 80px 60px 150px 80px 120px 120px',
                    gap: '16px',
                    padding: '16px',
                    borderBottom: index < outlets.length - 1 ? '1px solid #f3f4f6' : 'none',
                    fontSize: '12px',
                    alignItems: 'center'
                  }}>
                    <div>
                      <div style={{ fontWeight: '600', color: '#000000' }}>{outlet.id}</div>
                      <div style={{ color: '#000000', fontSize: '11px' }}>{outlet.name}</div>
                    </div>
                    <div>{outlet.channel}</div>
                    <div>{outlet.chain}</div>
                    <div>{outlet.category}</div>
                    <div style={{
                      color: getPriorityColor(outlet.priority),
                      fontWeight: '600'
                    }}>
                      {outlet.priority}
                    </div>
                    <div style={{ color: '#000000' }}>{outlet.location}</div>
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '8px'
                    }}>
                      <span style={{ color: '#000000' }}>Set Date</span>
                      <Calendar size={14} style={{ color: '#375AE6' }} />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default ItineraryCalendar