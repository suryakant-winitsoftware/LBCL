"use client"

import { EditVehicle } from "@/components/Vehicle/EditVehicle"

export default function EditVehiclePage({ params }: { params: { uid: string } }) {
  return <EditVehicle uid={params.uid} />
}
