"use client"
import { useState, useEffect } from 'react'

const ItineraryChart = () => {
  const [chartData] = useState({
    days: [
      'May 1', 'May 2', 'May 3', 'May 4', 'May 5', 'May 6', 'May 7', 'May 8', 'May 9', 'May10', 
      'May 11', 'May 12', 'May 13', 'May 14', 'May 15', 'May 16', 'May 17'
    ],
    outlets: {
      'May 1': { total: 21, lion: 19, heineken: 2, special: 0, activity: 0 },
      'May 2': { total: 18, lion: 16, heineken: 2, special: 0, activity: 0 },
      'May 3': { total: 12, lion: 8, heineken: 2, special: 2, activity: 0 },
      'May 4': { total: 0, lion: 0, heineken: 0, special: 0, activity: 0, holiday: true },
      'May 5': { total: 21, lion: 15, heineken: 3, special: 2, activity: 1 },
      'May 6': { total: 18, lion: 13, heineken: 2, special: 2, activity: 1 },
      'May 7': { total: 15, lion: 12, heineken: 2, special: 1, activity: 0 },
      'May 8': { total: 21, lion: 16, heineken: 3, special: 2, activity: 0 },
      'May 9': { total: 18, lion: 14, heineken: 2, special: 2, activity: 0 },
      'May10': { total: 12, lion: 9, heineken: 2, special: 1, activity: 0 },
      'May 11': { total: 0, lion: 0, heineken: 0, special: 0, activity: 0, holiday: true },
      'May 12': { total: 0, lion: 0, heineken: 0, special: 0, activity: 0, holiday: true },
      'May 13': { total: 18, lion: 14, heineken: 2, special: 2, activity: 0 },
      'May 14': { total: 15, lion: 12, heineken: 2, special: 1, activity: 0 },
      'May 15': { total: 21, lion: 16, heineken: 3, special: 2, activity: 0 },
      'May 16': { total: 18, lion: 14, heineken: 2, special: 2, activity: 0 },
      'May 17': { total: 12, lion: 9, heineken: 2, special: 1, activity: 0 }
    }
  })

  const maxValue = 21

  const getBarColor = (type) => {
    switch (type) {
      case 'lion': return '#375AE6'
      case 'heineken': return '#000000'
      case 'special': return '#375AE6'
      case 'activity': return '#000000'
      default: return '#000000'
    }
  }

  const renderBar = (day, data) => {
    if (data.holiday) {
      return (
        <div key={day} style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          width: '50px'
        }}>
          <div style={{
            height: '200px',
            width: '32px',
            backgroundColor: '#ffffff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: '10px',
            color: '#000000',
            fontWeight: '600',
            writingMode: 'vertical-lr',
            textOrientation: 'mixed',
            transform: 'rotate(180deg)'
          }}>
            Holiday
          </div>
          <div style={{
            fontSize: '11px',
            fontWeight: '500',
            color: '#000000',
            marginTop: '8px'
          }}>
            {day}
          </div>
        </div>
      )
    }

    const barHeight = 200
    const lionHeight = (data.lion / maxValue) * barHeight
    const heinekenHeight = (data.heineken / maxValue) * barHeight
    const specialHeight = (data.special / maxValue) * barHeight
    const activityHeight = (data.activity / maxValue) * barHeight

    return (
      <div key={day} style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        width: '50px'
      }}>
        <div style={{
          height: '200px',
          width: '32px',
          display: 'flex',
          flexDirection: 'column-reverse',
          backgroundColor: '#ffffff',
          position: 'relative'
        }}>
          {data.lion > 0 && (
            <div style={{
              height: `${lionHeight}px`,
              backgroundColor: getBarColor('lion'),
              width: '100%'
            }}></div>
          )}
          {data.heineken > 0 && (
            <div style={{
              height: `${heinekenHeight}px`,
              backgroundColor: getBarColor('heineken'),
              width: '100%'
            }}></div>
          )}
          {data.special > 0 && (
            <div style={{
              height: `${specialHeight}px`,
              backgroundColor: getBarColor('special'),
              width: '100%'
            }}></div>
          )}
          {data.activity > 0 && (
            <div style={{
              height: `${activityHeight}px`,
              backgroundColor: getBarColor('activity'),
              width: '100%'
            }}></div>
          )}
          <div style={{
            position: 'absolute',
            top: '-20px',
            left: '50%',
            transform: 'translateX(-50%)',
            fontSize: '10px',
            fontWeight: '600',
            color: '#000000'
          }}>
            {data.total}
          </div>
        </div>
        <div style={{
          fontSize: '11px',
          fontWeight: '500',
          color: '#000000',
          marginTop: '8px'
        }}>
          {day}
        </div>
      </div>
    )
  }

  return (
    <div style={{
      backgroundColor: '#ffffff',
      borderRadius: '8px',
      padding: '24px',
      boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
    }}>
      <h3 style={{
        fontSize: '18px',
        fontWeight: '600',
        color: '#000000',
        marginBottom: '24px',
        textAlign: 'center'
      }}>
        May 2025 - Sales Itinerary Planning (Approved)
      </h3>

      {/* Y-axis labels */}
      <div style={{
        display: 'flex',
        alignItems: 'flex-start',
        gap: '16px'
      }}>
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          height: '200px',
          justifyContent: 'space-between',
          fontSize: '11px',
          color: '#6b7280',
          paddingTop: '10px'
        }}>
          <span>21</span>
          <span>18</span>
          <span>15</span>
          <span>12</span>
          <span>09</span>
          <span>06</span>
          <span>03</span>
          <span>00</span>
        </div>

        {/* Chart bars */}
        <div style={{
          display: 'flex',
          gap: '8px',
          alignItems: 'flex-end',
          flex: 1,
          justifyContent: 'center'
        }}>
          {chartData.days.map((day) => 
            renderBar(day, chartData.outlets[day])
          )}
        </div>
      </div>

      {/* Legend */}
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        gap: '24px',
        marginTop: '32px',
        flexWrap: 'wrap'
      }}>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px'
        }}>
          <div style={{
            width: '16px',
            height: '16px',
            backgroundColor: getBarColor('lion'),
            borderRadius: '2px'
          }}></div>
          <span style={{
            fontSize: '12px',
            color: '#000000'
          }}>
            80% Contribution Outlets (LION)
          </span>
        </div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px'
        }}>
          <div style={{
            width: '16px',
            height: '16px',
            backgroundColor: getBarColor('heineken'),
            borderRadius: '2px'
          }}></div>
          <span style={{
            fontSize: '12px',
            color: '#000000'
          }}>
            80% Contribution Outlets (Heineken)
          </span>
        </div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px'
        }}>
          <div style={{
            width: '16px',
            height: '16px',
            backgroundColor: getBarColor('special'),
            borderRadius: '2px'
          }}></div>
          <span style={{
            fontSize: '12px',
            color: '#000000'
          }}>
            Special Outlets
          </span>
        </div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px'
        }}>
          <div style={{
            width: '16px',
            height: '16px',
            backgroundColor: getBarColor('activity'),
            borderRadius: '2px'
          }}></div>
          <span style={{
            fontSize: '12px',
            color: '#000000'
          }}>
            Activity (National, Brand, Tourist board licenses)
          </span>
        </div>
      </div>

      {/* Bottom line indicator */}
      <div style={{
        marginTop: '16px',
        display: 'flex',
        justifyContent: 'center'
      }}>
        <div style={{
          width: '60%',
          height: '4px',
          backgroundColor: '#375AE6',
          borderRadius: '2px'
        }}></div>
      </div>
    </div>
  )
}

export default ItineraryChart