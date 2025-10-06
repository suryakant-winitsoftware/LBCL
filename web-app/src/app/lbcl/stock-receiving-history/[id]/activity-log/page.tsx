import StockReceivingActivityLogView from "@/app/lbcl/components/stock-receiving-activity-log-view"

export default async function HistoryActivityLogPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  return <StockReceivingActivityLogView deliveryId={id} />
}
