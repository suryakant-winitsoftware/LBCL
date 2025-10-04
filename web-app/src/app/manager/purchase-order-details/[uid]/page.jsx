'use client'

import { useParams } from 'next/navigation'
import PurchaseOrderDetails from '../../components/PurchaseOrderDetails'

export default function PurchaseOrderDetailsPage() {
  const params = useParams()
  const uid = params.uid

  return <PurchaseOrderDetails uid={uid} />
}
