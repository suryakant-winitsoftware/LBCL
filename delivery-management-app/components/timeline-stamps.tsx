"use client"

import { Menu, Clock, Bell } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useRouter } from "next/navigation"

const timelineData = [
  {
    deliveryOrderNo: "85444127121",
    date: "20-May-25",
    primeMoverNo: "LK1673",
    driverName: "U Kumar",
    departureTime: "11: 05 AM",
    agencyArrivalTime: "11: 29 AM",
    timeToArriveAgency: "24 Min",
    timeToUnload: "20 Min",
    timeToEmptyLoad: "20 Min",
    departureTime2: "24 Min",
    lbclArrivedTime: "3: 00 AM",
  },
  {
    deliveryOrderNo: "85444127121",
    date: "20-May-25",
    primeMoverNo: "LK1674",
    driverName: "Vasanth Rao",
    departureTime: "11: 32 AM",
    agencyArrivalTime: "11: 40 AM",
    timeToArriveAgency: "08 Min",
    timeToUnload: "20 Min",
    timeToEmptyLoad: "20 Min",
    departureTime2: "08 Min",
    lbclArrivedTime: "3: 00 AM",
  },
  {
    deliveryOrderNo: "85444127121",
    date: "20-May-25",
    primeMoverNo: "LK1675",
    driverName: "Tarun M",
    departureTime: "11: 45 AM",
    agencyArrivalTime: "12: 20 PM",
    timeToArriveAgency: "35 Min",
    timeToUnload: "35 Min",
    timeToEmptyLoad: "35 Min",
    departureTime2: "35 Min",
    lbclArrivedTime: "3: 00 AM",
  },
  {
    deliveryOrderNo: "85444127121",
    date: "20-May-25",
    primeMoverNo: "LK1676",
    driverName: "Sanjay",
    departureTime: "12: 22 PM",
    agencyArrivalTime: "12: 28 PM",
    timeToArriveAgency: "06 Min",
    timeToUnload: "15 Min",
    timeToEmptyLoad: "15 Min",
    departureTime2: "06 Min",
    lbclArrivedTime: "3: 00 AM",
  },
  {
    deliveryOrderNo: "85444127121",
    date: "20-May-25",
    primeMoverNo: "LK1677",
    driverName: "Praveen",
    departureTime: "12: 30 PM",
    agencyArrivalTime: "12: 35 PM",
    timeToArriveAgency: "05 Min",
    timeToUnload: "10 Min",
    timeToEmptyLoad: "10 Min",
    departureTime2: "05 Min",
    lbclArrivedTime: "3: 00 AM",
  },
]

export default function TimelineStamps() {
  const router = useRouter()

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between sticky top-0 z-10">
        <button className="p-2">
          <Menu className="w-6 h-6" />
        </button>
        <h1 className="text-lg sm:text-xl md:text-2xl font-bold flex-1 text-center">Time Line Stamps</h1>
        <div className="flex items-center gap-2">
          <button className="p-2">
            <Clock className="w-6 h-6" />
          </button>
          <button className="p-2">
            <Bell className="w-6 h-6" />
          </button>
          <Button className="bg-[#A08B5C] hover:bg-[#8A7549] text-white" onClick={() => router.back()}>
            DONE
          </Button>
        </div>
      </header>

      {/* Table - Scrollable */}
      <div className="overflow-x-auto">
        <table className="w-full min-w-[1400px]">
          <thead className="bg-[#F5E6D3] sticky top-[73px]">
            <tr>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Delivery Order Number</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Date</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Prime Mover No</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Prime Mover Driver Name</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Departure Time</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Agency Arrival Time</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">
                Time Taken to Arrive Agency
              </th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Time Taken to Unload</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Time Taken to Empty Load</th>
              <th className="text-left p-3 font-semibold text-sm border-r border-gray-300">Departure Time</th>
              <th className="text-left p-3 font-semibold text-sm">LBCL Arrived Time</th>
            </tr>
          </thead>
          <tbody>
            {timelineData.map((row, index) => (
              <tr key={index} className="border-b border-gray-200 hover:bg-gray-50">
                <td className="p-3 border-r border-gray-200">{row.deliveryOrderNo}</td>
                <td className="p-3 border-r border-gray-200">{row.date}</td>
                <td className="p-3 border-r border-gray-200">{row.primeMoverNo}</td>
                <td className="p-3 border-r border-gray-200">{row.driverName}</td>
                <td className="p-3 border-r border-gray-200">{row.departureTime}</td>
                <td className="p-3 border-r border-gray-200">{row.agencyArrivalTime}</td>
                <td className="p-3 border-r border-gray-200">{row.timeToArriveAgency}</td>
                <td className="p-3 border-r border-gray-200">{row.timeToUnload}</td>
                <td className="p-3 border-r border-gray-200">{row.timeToEmptyLoad}</td>
                <td className="p-3 border-r border-gray-200">{row.departureTime2}</td>
                <td className="p-3">{row.lbclArrivedTime}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile Warning */}
      <div className="p-4 bg-yellow-50 border-t border-yellow-200 lg:hidden">
        <p className="text-sm text-yellow-800 text-center">
          For best viewing experience, please use a larger screen or scroll horizontally
        </p>
      </div>
    </div>
  )
}
