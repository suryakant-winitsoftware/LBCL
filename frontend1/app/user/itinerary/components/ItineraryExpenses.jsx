"use client"
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { 
  MapPin,
  Upload,
  Clock,
  Navigation,
  Coffee
} from 'lucide-react'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'

const ItineraryExpenses = () => {
  const { user } = useAuth()
  const router = useRouter()
  const [selectedExpenseType, setSelectedExpenseType] = useState('Food Bill')
  const [amount, setAmount] = useState('0.00')
  const [remarks, setRemarks] = useState('')
  const [selectedDate, setSelectedDate] = useState(16)


  const generateCalendarDays = () => {
    const days = []
    for (let i = 1; i <= 31; i++) {
      days.push({
        date: i,
        isWeekend: [4, 11, 18, 25].includes(i), // Sundays in red
        isSelected: i === selectedDate
      })
    }
    return days
  }

  const calendarDays = generateCalendarDays()
  const weekDays = ['S', 'M', 'T', 'W', 'T', 'F', 'S']

  const timeData = [
    { icon: Clock, label: 'Start Time', value: '8:46 AM' },
    { icon: Coffee, label: 'First Check-in', value: '09:08 AM' },
    { icon: Clock, label: 'Last Check-out', value: '05:08 PM' },
    { icon: Clock, label: 'EOS Time', value: '05:25 PM' },
    { icon: Clock, label: 'Total working Time', value: '8 Hrs 39 Min' },
    { icon: Navigation, label: 'Time Spent in the outlet', value: '5 Hrs 49 Min' },
    { icon: MapPin, label: 'Time Spent while Traveling', value: '2 Hrs 50 Min' }
  ]

  const handleSubmit = () => {
    alert('Expense submitted for review!')
  }

  return (
    <ProtectedRoute requiredSystem="itinerary">
      <BusinessLayout title="Expenses">
        <div className="space-y-6">
          <div style={{ display: 'flex', gap: '24px' }}>
          {/* Left Panel - Calendar */}
          <div style={{ width: '300px' }}>
            {/* Calendar */}
            <div style={{
              backgroundColor: '#ffffff',
              borderRadius: '8px',
              padding: '20px',
              boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
              marginBottom: '16px'
            }}>
              <h3 style={{
                fontSize: '16px',
                fontWeight: '600',
                color: '#000000',
                marginBottom: '16px',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center'
              }}>
                <span>May</span>
                <span>2025</span>
              </h3>

              {/* Week Days Header */}
              <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(7, 1fr)',
                gap: '4px',
                marginBottom: '8px'
              }}>
                {weekDays.map(day => (
                  <div key={day} style={{
                    textAlign: 'center',
                    fontSize: '12px',
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
                {/* First week padding */}
                {[1, 2].map(i => (
                  <div key={`empty-${i}`} style={{ height: '32px' }}></div>
                ))}
                
                {calendarDays.slice(0, 31).map((day, index) => (
                  <button key={day.date} 
                    onClick={() => setSelectedDate(day.date)}
                    style={{
                      height: '32px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '13px',
                      fontWeight: day.isSelected ? '600' : '500',
                      backgroundColor: day.isSelected ? '#375AE6' : 'transparent',
                      color: day.isSelected ? '#ffffff' : day.isWeekend ? '#000000' : '#000000',
                      border: 'none',
                      borderRadius: '4px',
                      cursor: 'pointer'
                    }}
                  >
                    {day.date}
                  </button>
                ))}
              </div>
            </div>

            {/* Time Tracking */}
            <div style={{
              backgroundColor: '#ffffff',
              borderRadius: '8px',
              padding: '16px',
              boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)'
            }}>
              {timeData.map((item, index) => {
                const Icon = item.icon
                return (
                  <div key={index} style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '12px',
                    padding: '8px 0',
                    borderBottom: index < timeData.length - 1 ? '1px solid #ffffff' : 'none',
                    fontSize: '11px'
                  }}>
                    <Icon size={16} style={{ color: '#000000' }} />
                    <div style={{ flex: 1 }}>
                      <div style={{ color: '#000000', marginBottom: '2px' }}>{item.label}</div>
                      <div style={{ color: '#000000', fontWeight: '600' }}>{item.value}</div>
                    </div>
                  </div>
                )
              })}
            </div>
          </div>

          {/* Right Panel - Map and Form */}
          <div style={{ flex: 1 }}>
            {/* Map Section */}
            <div style={{
              backgroundColor: '#ffffff',
              borderRadius: '8px',
              padding: '16px',
              marginBottom: '16px',
              boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-start'
            }}>
              <div style={{
                width: '60%',
                height: '200px',
                backgroundColor: '#ffffff',
                borderRadius: '8px',
                backgroundImage: 'url("data:image/svg+xml,%3Csvg width="100" height="100" xmlns="http://www.w3.org/2000/svg"%3E%3Cdefs%3E%3Cpattern id="grid" width="20" height="20" patternUnits="userSpaceOnUse"%3E%3Cpath d="M 20 0 L 0 0 0 20" fill="none" stroke="%23e0f2fe" stroke-width="1"/%3E%3C/pattern%3E%3C/defs%3E%3Crect width="100%" height="100%" fill="url(%23grid)" /%3E%3C/svg%3E")',
                position: 'relative',
                overflow: 'hidden'
              }}>
                {/* Mock route line */}
                <svg style={{
                  position: 'absolute',
                  top: 0,
                  left: 0,
                  width: '100%',
                  height: '100%',
                  pointerEvents: 'none'
                }}>
                  <path d="M50,30 Q100,50 150,40 T250,60 T350,80" 
                        stroke="#375AE6" 
                        strokeWidth="3" 
                        fill="none" 
                        strokeDasharray="5,5" />
                </svg>
                
                {/* Mock markers */}
                {[
                  { top: '25%', left: '15%', color: '#375AE6' },
                  { top: '45%', left: '35%', color: '#000000' },
                  { top: '35%', left: '55%', color: '#375AE6' },
                  { top: '55%', left: '75%', color: '#375AE6' }
                ].map((marker, index) => (
                  <div key={index} style={{
                    position: 'absolute',
                    top: marker.top,
                    left: marker.left,
                    width: '12px',
                    height: '12px',
                    backgroundColor: marker.color,
                    borderRadius: '50%',
                    border: '2px solid #ffffff',
                    boxShadow: '0 2px 4px rgba(0,0,0,0.2)'
                  }}></div>
                ))}
              </div>

              {/* Distance and Amount Cards */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                <div style={{
                  backgroundColor: '#ffffff',
                  border: '1px solid #375AE6',
                  borderRadius: '8px',
                  padding: '16px',
                  textAlign: 'center',
                  minWidth: '140px'
                }}>
                  <div style={{ fontSize: '12px', color: '#000000', marginBottom: '4px' }}>
                    Total Distance
                  </div>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#000000' }}>
                    43.7 <span style={{ fontSize: '14px' }}>Kms</span>
                  </div>
                </div>
                
                <div style={{
                  backgroundColor: '#ffffff',
                  border: '1px solid #375AE6',
                  borderRadius: '8px',
                  padding: '16px',
                  textAlign: 'center',
                  minWidth: '140px'
                }}>
                  <div style={{ fontSize: '12px', color: '#000000', marginBottom: '4px' }}>
                    Eligible Amount
                  </div>
                  <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#000000' }}>
                    4,031.70 <span style={{ fontSize: '12px' }}>LKR</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Expense Form */}
            <div style={{
              backgroundColor: '#ffffff',
              borderRadius: '8px',
              padding: '24px',
              boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)'
            }}>
              <h3 style={{
                fontSize: '18px',
                fontWeight: '600',
                color: '#000000',
                marginBottom: '20px'
              }}>
                Add New Expense
              </h3>

              <div style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr 1fr',
                gap: '16px',
                marginBottom: '16px'
              }}>
                <div>
                  <label style={{
                    display: 'block',
                    fontSize: '14px',
                    fontWeight: '500',
                    color: '#000000',
                    marginBottom: '8px'
                  }}>
                    Select Expense Type
                  </label>
                  <select
                    value={selectedExpenseType}
                    onChange={(e) => setSelectedExpenseType(e.target.value)}
                    style={{
                      width: '100%',
                      padding: '12px',
                      border: '1px solid #000000',
                      borderRadius: '4px',
                      fontSize: '14px',
                      backgroundColor: '#ffffff'
                    }}
                  >
                    <option>Food Bill</option>
                    <option>Transportation</option>
                    <option>Accommodation</option>
                    <option>Fuel</option>
                    <option>Other</option>
                  </select>
                </div>

                <div>
                  <label style={{
                    display: 'block',
                    fontSize: '14px',
                    fontWeight: '500',
                    color: '#000000',
                    marginBottom: '8px'
                  }}>
                    Amount
                  </label>
                  <div style={{ position: 'relative' }}>
                    <span style={{
                      position: 'absolute',
                      left: '12px',
                      top: '12px',
                      fontSize: '14px',
                      color: '#000000'
                    }}>
                      LKR
                    </span>
                    <input
                      type="text"
                      value={amount}
                      onChange={(e) => setAmount(e.target.value)}
                      style={{
                        width: '100%',
                        padding: '12px 12px 12px 50px',
                        border: '1px solid #000000',
                        borderRadius: '4px',
                        fontSize: '14px',
                        textAlign: 'right'
                      }}
                    />
                  </div>
                </div>

                <div>
                  <label style={{
                    display: 'block',
                    fontSize: '14px',
                    fontWeight: '500',
                    color: '#000000',
                    marginBottom: '8px'
                  }}>
                    Upload Image
                  </label>
                  <button style={{
                    width: '100%',
                    padding: '12px',
                    border: '1px solid #000000',
                    borderRadius: '4px',
                    backgroundColor: '#ffffff',
                    fontSize: '14px',
                    color: '#000000',
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: '8px'
                  }}>
                    <Upload size={16} />
                    Upload
                  </button>
                </div>
              </div>

              <div style={{ marginBottom: '20px' }}>
                <label style={{
                  display: 'block',
                  fontSize: '14px',
                  fontWeight: '500',
                  color: '#000000',
                  marginBottom: '8px'
                }}>
                  Remarks
                </label>
                <textarea
                  value={remarks}
                  onChange={(e) => setRemarks(e.target.value)}
                  placeholder="Enter Here"
                  rows={3}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: '1px solid #000000',
                    borderRadius: '4px',
                    fontSize: '14px',
                    resize: 'vertical',
                    minHeight: '80px'
                  }}
                />
              </div>

              <div style={{ textAlign: 'right' }}>
                <button
                  onClick={handleSubmit}
                  style={{
                    padding: '12px 32px',
                    backgroundColor: '#375AE6',
                    color: '#ffffff',
                    border: 'none',
                    borderRadius: '4px',
                    fontSize: '14px',
                    fontWeight: '600',
                    cursor: 'pointer'
                  }}
                >
                  Submit for Review
                </button>
              </div>
            </div>
          </div>
          </div>
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default ItineraryExpenses