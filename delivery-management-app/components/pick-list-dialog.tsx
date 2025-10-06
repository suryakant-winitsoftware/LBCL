"use client"

import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"

interface PickListDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function PickListDialog({ open, onOpenChange }: PickListDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh]">
        <DialogHeader>
          <DialogTitle className="text-lg sm:text-xl">View / Generate Pick List</DialogTitle>
        </DialogHeader>

        <div className="py-4">
          <div className="bg-gray-100 rounded-lg p-4 sm:p-6 min-h-[400px] flex items-center justify-center">
            <div className="text-center">
              <div className="text-sm text-gray-600 mb-2">Page 1/2</div>
              <div className="font-medium mb-4">Pick Sheet</div>
              <div className="space-y-2 text-xs text-gray-500">
                <div>Truck: [P801] DEPOT</div>
                <div>Run Date: Wednesday 20 Nov, 2024</div>
                <div>Run: [CY00000000] Depot_seed</div>
                <div>Printed Date/Time: 25/11/2024 02:36:09 AM</div>
              </div>
              <div className="mt-6 text-xs text-gray-400">[PDF Preview Content]</div>
            </div>
          </div>
        </div>

        <Button onClick={() => onOpenChange(false)} className="w-full bg-[#A08B5C] hover:bg-[#8F7A4B] text-white h-12">
          DONE
        </Button>
      </DialogContent>
    </Dialog>
  )
}
