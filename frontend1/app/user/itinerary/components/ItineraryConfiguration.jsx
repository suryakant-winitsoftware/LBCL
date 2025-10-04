"use client"
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { 
  Plus,
  Edit,
  Trash
} from 'lucide-react'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'

const ItineraryConfiguration = () => {
  const { user } = useAuth()
  const router = useRouter()
  const [configData, setConfigData] = useState({
    totalWorkingDays: '22',
    averageOutletsPerDay: '6',
    estimatedMonthlyCapacity: '132'
  })
  
  const [holidays, setHolidays] = useState([
    { id: 1, date: 'June 1, 2025', day: 'Sunday', reason: 'Week-Off (Default)' },
    { id: 2, date: 'June 6, 2025', day: 'Saturday', reason: 'Eid Holiday' },
    { id: 3, date: 'June 8, 2025', day: 'Sunday', reason: 'Week-Off (Default)' },
    { id: 4, date: 'June 15, 2025', day: 'Sunday', reason: 'Week-Off (Default)' },
    { id: 5, date: 'June 22, 2025', day: 'Sunday', reason: 'Week-Off (Default)' },
    { id: 6, date: 'June 29, 2025', day: 'Sunday', reason: 'Week-Off (Default)' }
  ])

  const [kpiCategories] = useState([
    { name: 'Vulnerable Outlet Base', percentage: '90%', color: '#000000', description: 'Outlets At Risk Of Churn Or Reduction In Orders' },
    { name: 'Isolated Outlet Base', percentage: '30%', color: '#375AE6', description: 'Outlets In Remote Or Hard To Reach Locations' },
    { name: 'Exceptional Effort', percentage: '60%', color: '#375AE6', description: 'Outlets Requiring Special Attention Or Effort' },
    { name: 'New Licenses', percentage: '10%', color: '#375AE6', description: 'Newly Licensed Outlets Requiring Onboarding' },
    { name: '80% Contribution Outlets (Heineken)', percentage: '90%', color: '#000000', description: 'Key Outlets That Contribute To 80% Of Heineken Sales' }
  ])


  const handleConfigChange = (field, value) => {
    setConfigData(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const handleDeleteHoliday = (id) => {
    setHolidays(prev => prev.filter(h => h.id !== id))
  }

  const handleSaveConfiguration = () => {
    alert('Configuration saved successfully!')
  }

  return (
    <ProtectedRoute requiredSystem="itinerary">
      <BusinessLayout title="Sales Itinerary Configuration">
        <div className="space-y-6">
          {/* Configuration Section */}
          <div style={{
            backgroundColor: '#ffffff',
            borderRadius: '8px',
            padding: '24px',
            marginBottom: '24px',
            boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)'
          }}>
            <h2 style={{
              fontSize: '18px',
              fontWeight: '600',
              color: '#000000',
              marginBottom: '24px'
            }}>
              Sales Itinerary Configuration
            </h2>
            <div style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
              gap: '24px',
              marginBottom: '24px'
            }}>
              <div>
                <label style={{
                  display: 'block',
                  fontSize: '14px',
                  fontWeight: '500',
                  color: '#000000',
                  marginBottom: '8px'
                }}>
                  Total Working Days In Month (June 2025)
                </label>
                <input
                  type="text"
                  value={configData.totalWorkingDays}
                  onChange={(e) => handleConfigChange('totalWorkingDays', e.target.value)}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: '1px solid #000000',
                    borderRadius: '4px',
                    fontSize: '14px'
                  }}
                />
              </div>
              <div>
                <label style={{
                  display: 'block',
                  fontSize: '14px',
                  fontWeight: '500',
                  color: '#000000',
                  marginBottom: '8px'
                }}>
                  Average Outlets Per Day
                </label>
                <input
                  type="text"
                  value={configData.averageOutletsPerDay}
                  onChange={(e) => handleConfigChange('averageOutletsPerDay', e.target.value)}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: '1px solid #000000',
                    borderRadius: '4px',
                    fontSize: '14px'
                  }}
                />
              </div>
              <div>
                <label style={{
                  display: 'block',
                  fontSize: '14px',
                  fontWeight: '500',
                  color: '#000000',
                  marginBottom: '8px'
                }}>
                  Estimated Monthly Capacity
                </label>
                <input
                  type="text"
                  value={configData.estimatedMonthlyCapacity}
                  onChange={(e) => handleConfigChange('estimatedMonthlyCapacity', e.target.value)}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: '1px solid #000000',
                    borderRadius: '4px',
                    fontSize: '14px'
                  }}
                />
              </div>
            </div>
            <div style={{ textAlign: 'right' }}>
              <button
                onClick={handleSaveConfiguration}
                style={{
                  padding: '12px 24px',
                  backgroundColor: '#375AE6',
                  color: '#ffffff',
                  border: 'none',
                  borderRadius: '4px',
                  fontSize: '14px',
                  fontWeight: '600',
                  cursor: 'pointer'
                }}
              >
                Save Configuration
              </button>
            </div>
          </div>

          {/* Two Column Layout */}
          <div style={{
            display: 'grid',
            gridTemplateColumns: '1fr 1fr',
            gap: '24px'
          }}>
            {/* Holidays Section */}
            <div style={{
              backgroundColor: '#ffffff',
              borderRadius: '8px',
              padding: '24px',
              boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)'
            }}>
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '16px'
              }}>
                <h3 style={{
                  fontSize: '16px',
                  fontWeight: '600',
                  color: '#000000',
                  margin: 0
                }}>
                  Holidays In Month
                </h3>
                <button style={{
                  padding: '8px 16px',
                  backgroundColor: 'transparent',
                  color: '#375AE6',
                  border: '1px solid #375AE6',
                  borderRadius: '4px',
                  fontSize: '12px',
                  fontWeight: '500',
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '4px'
                }}>
                  <Plus size={14} />
                  Add New
                </button>
              </div>

              {/* Table Header */}
              <div style={{
                display: 'grid',
                gridTemplateColumns: '40px 1fr 80px 120px 80px',
                gap: '12px',
                padding: '12px 0',
                borderBottom: '1px solid #000000',
                fontSize: '12px',
                fontWeight: '600',
                color: '#000000',
                backgroundColor: '#ffffff'
              }}>
                <div>S.No</div>
                <div>Date</div>
                <div>Day</div>
                <div>Reason</div>
                <div>Action</div>
              </div>

              {/* Table Rows */}
              {holidays.map((holiday, index) => (
                <div key={holiday.id} style={{
                  display: 'grid',
                  gridTemplateColumns: '40px 1fr 80px 120px 80px',
                  gap: '12px',
                  padding: '12px 0',
                  borderBottom: '1px solid #ffffff',
                  fontSize: '12px',
                  alignItems: 'center'
                }}>
                  <div>{index + 1}</div>
                  <div>{holiday.date}</div>
                  <div>{holiday.day}</div>
                  <div>{holiday.reason}</div>
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <button style={{
                      padding: '4px',
                      backgroundColor: 'transparent',
                      border: 'none',
                      cursor: 'pointer',
                      color: '#375AE6'
                    }}>
                      <Edit size={14} />
                    </button>
                    <button 
                      onClick={() => handleDeleteHoliday(holiday.id)}
                      style={{
                        padding: '4px',
                        backgroundColor: 'transparent',
                        border: 'none',
                        cursor: 'pointer',
                        color: '#000000'
                      }}
                    >
                      <Trash size={14} />
                    </button>
                  </div>
                </div>
              ))}
            </div>

            {/* KPI Categories Section */}
            <div style={{
              backgroundColor: '#ffffff',
              borderRadius: '8px',
              padding: '24px',
              boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)'
            }}>
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '16px'
              }}>
                <h3 style={{
                  fontSize: '16px',
                  fontWeight: '600',
                  color: '#000000',
                  margin: 0
                }}>
                  Days I Dont Want To Travel
                </h3>
                <button style={{
                  padding: '8px 16px',
                  backgroundColor: 'transparent',
                  color: '#375AE6',
                  border: '1px solid #375AE6',
                  borderRadius: '4px',
                  fontSize: '12px',
                  fontWeight: '500',
                  cursor: 'pointer'
                }}>
                  Add New
                </button>
              </div>

              <h4 style={{
                fontSize: '14px',
                fontWeight: '600',
                color: '#000000',
                marginBottom: '16px',
                marginTop: '24px'
              }}>
                General KPI Category
              </h4>

              {kpiCategories.map((category, index) => (
                <div key={index} style={{
                  padding: '12px 0',
                  borderBottom: index < kpiCategories.length - 1 ? '1px solid #ffffff' : 'none'
                }}>
                  <div style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    marginBottom: '4px'
                  }}>
                    <span style={{
                      fontSize: '14px',
                      fontWeight: '500',
                      color: '#000000'
                    }}>
                      {category.name}
                    </span>
                    <span style={{
                      fontSize: '14px',
                      fontWeight: '600',
                      color: category.color,
                      backgroundColor: `${category.color}20`,
                      padding: '4px 8px',
                      borderRadius: '4px'
                    }}>
                      {category.percentage}
                    </span>
                  </div>
                  <p style={{
                    fontSize: '11px',
                    color: '#000000',
                    margin: 0
                  }}>
                    {category.description}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default ItineraryConfiguration