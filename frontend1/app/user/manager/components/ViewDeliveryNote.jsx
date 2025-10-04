"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { X } from 'lucide-react'
import { Button } from '../../../components/ui/button'

const ViewDeliveryNote = ({ isOpen, onClose }) => {
  const router = useRouter()
  const [pickSheetData, setPickSheetData] = useState([])
  const [documentInfo, setDocumentInfo] = useState({})

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setPickSheetData(data.deliveryNote.pickSheetData)
        setDocumentInfo(data.deliveryNote.documentInfo)
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    if (isOpen) {
      fetchData()
    }
  }, [isOpen])

  if (!isOpen) return null

  return (
    <div style={{backgroundColor: 'rgba(0,0,0,0.5)'}} className="fixed inset-0 flex items-center justify-center z-50 p-4">
      <div style={{backgroundColor: '#ffffff', border: '1px solid #000000'}} className="w-full max-w-md">
        {/* Modal Header */}
        <div style={{borderBottom: '1px solid #000000'}} className="flex items-center justify-between p-4">
          <h3 style={{color: '#000000'}} className="text-lg font-semibold">View Delivery Note</h3>
          <button onClick={onClose}>
            <X style={{color: '#000000'}} className="w-5 h-5" />
          </button>
        </div>

        {/* Document Preview */}
        <div className="p-4">
          <div style={{backgroundColor: '#ffffff', border: '1px solid #000000'}} className="p-4 h-80 overflow-y-auto">
            <div style={{backgroundColor: '#ffffff'}} className="min-h-full p-4">
              {/* Document Header */}
              <div style={{borderBottom: '1px solid #000000'}} className="flex justify-between items-center mb-4 pb-2">
                <h4 style={{color: '#000000'}} className="font-bold">Pick Sheet</h4>
                <span style={{color: '#000000'}} className="text-sm">Page 1/3</span>
              </div>

              {/* Document Info */}
              <div className="grid grid-cols-2 gap-4 text-xs mb-4">
                <div>
                  <div className="mb-2"><span style={{color: '#000000'}} className="font-medium">Store:</span> {documentInfo.store}</div>
                  <div><span style={{color: '#000000'}} className="font-medium">Run:</span> {documentInfo.run}</div>
                </div>
                <div>
                  <div className="mb-2"><span style={{color: '#000000'}} className="font-medium">Run Date:</span> {documentInfo.runDate}</div>
                  <div><span style={{color: '#000000'}} className="font-medium">Src Depot:</span> {documentInfo.srcDepot}</div>
                </div>
              </div>

              {/* Table Header */}
              <div style={{backgroundColor: '#ffffff', border: '1px solid #000000'}} className="grid grid-cols-4 gap-2 py-1 mb-1 text-xs font-medium">
                <div style={{color: '#000000'}}>Code</div>
                <div style={{color: '#000000'}}>Item</div>
                <div style={{color: '#000000'}} className="text-center">Qty</div>
                <div style={{color: '#000000'}} className="text-center">Location</div>
              </div>

              {/* Table Body */}
              <div className="text-xs pb-4">
                {pickSheetData.map((item, index) => (
                  <div 
                    key={index} 
                    style={{borderTop: index > 0 ? '1px solid #000000' : 'none'}} 
                    className="grid grid-cols-4 gap-2 py-1"
                  >
                    <div style={{color: '#000000'}} className="font-mono">{item.code}</div>
                    <div style={{color: '#000000'}}>{item.item}</div>
                    <div style={{color: '#000000'}} className="text-center">{item.qty}</div>
                    <div style={{color: '#000000'}} className="text-center">{item.location}</div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Modal Footer */}
        <div style={{borderTop: '1px solid #000000'}} className="p-4">
          <Button 
            onClick={onClose}
            style={{backgroundColor: '#375AE6', color: '#ffffff', border: '1px solid #000000'}}
            className="w-full"
          >
            DONE
          </Button>
        </div>
      </div>
    </div>
  )
}

export default ViewDeliveryNote