"use client"

import { EditTerritory } from "@/components/Territory/EditTerritory"

export default function EditTerritoryPage({ params }: { params: { id: string } }) {
  return <EditTerritory territoryId={params.id} />
}
