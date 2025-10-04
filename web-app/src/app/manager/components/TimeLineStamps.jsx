"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Menu } from 'lucide-react'
import { Button } from '../../../components/ui/button'
import ProtectedRoute from '../../../components/ProtectedRoute'
import NavigationSidebar from '../../../components/NavigationSidebar'

const TimeLineStamps = () => {
  const router = useRouter()
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  
  // Fetch timeline data from centralized data.json
  const [timelineData, setTimelineData] = useState([])

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setTimelineData(data.timeline)
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    fetchData()
  }, [])

  const handleDone = () => {
    router.back()
  }

  return (
    <ProtectedRoute requiredSystem="stock">
      <div style={{backgroundColor: '#ffffff'}} className="min-h-screen">
        {/* Header */}
        <header style={{backgroundColor: '#ffffff', borderBottom: '1px solid #000000'}} className="px-4 py-3">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <button 
                onClick={() => setIsSidebarOpen(true)}
                style={{backgroundColor: '#ffffff', border: '1px solid #000000'}}
                className="p-1"
              >
                <Menu style={{color: '#000000'}} className="w-6 h-6" />
              </button>
              <h1 style={{color: '#000000'}} className="text-xl font-semibold">Time Line Stamps</h1>
            </div>
            
            <Button 
              onClick={handleDone}
              style={{backgroundColor: '#375AE6', color: '#ffffff', border: '1px solid #000000'}}
            >
              DONE
            </Button>
          </div>
        </header>

        {/* Timeline Table */}
        <div className="p-2">
          <div style={{backgroundColor: '#ffffff', border: '1px solid #000000'}}>
            {/* Table Header */}
            <div style={{backgroundColor: '#ffffff', borderBottom: '1px solid #000000'}} className="overflow-x-auto">
              <table className="w-full min-w-[1200px]">
                <thead>
                  <tr style={{color: '#000000'}} className="text-sm font-medium">
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Delivery Order Number</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Date</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Prime Mover No</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Prime Mover Driver Name</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Departure Time</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Agency Arrival Time</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Time Taken to Arrive Agency</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Time Taken to Unload</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Time Taken to Empty Load</th>
                    <th style={{borderRight: '1px solid #000000'}} className="p-2 text-left">Departure Time</th>
                    <th className="p-2 text-left">LION Arrived Time</th>
                  </tr>
                </thead>
              </table>
            </div>

            {/* Table Body */}
            <div className="overflow-x-auto">
              <table className="w-full min-w-[1200px]">
                <tbody>
                  {timelineData.map((item, index) => (
                    <tr 
                      key={index} 
                      style={{color: '#000000', borderTop: index > 0 ? '1px solid #000000' : 'none'}} 
                      className="text-sm"
                    >
                      <td style={{borderRight: '1px solid #000000'}} className="p-2 font-medium">{item.deliveryOrderNumber}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.date}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.primeMoverNo}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.primeMoverDriverName}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.departureTime}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.agencyArrivalTime}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.timeTakenToArriveAgency}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.timeTakenToUnload}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.timeTakenToEmptyLoad}</td>
                      <td style={{borderRight: '1px solid #000000'}} className="p-2">{item.departureTime2}</td>
                      <td className="p-2">{item.lionArrivedTime}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        {/* Navigation Sidebar */}
        <NavigationSidebar 
          isOpen={isSidebarOpen} 
          onClose={() => setIsSidebarOpen(false)} 
        />
      </div>
    </ProtectedRoute>
  )
}

export default TimeLineStamps