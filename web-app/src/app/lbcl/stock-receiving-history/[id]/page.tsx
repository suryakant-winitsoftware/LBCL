import PhysicalCountPage from "@/app/lbcl/components/physical-count-page"

export default async function HistoryDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  return <PhysicalCountPage deliveryId={id} readOnly={true} />
}
