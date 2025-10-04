'use client'

import { use } from 'react'
import EditPurchaseOrderTemplate from '../../components/EditPurchaseOrderTemplate'

export default function EditPurchaseOrderTemplatePage({ params }) {
  const resolvedParams = use(params)
  return <EditPurchaseOrderTemplate templateId={resolvedParams.id} />
}