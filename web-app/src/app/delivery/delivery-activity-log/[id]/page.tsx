import DeliveryActivityLog from '../../components/DeliveryActivityLog'

export default function DeliveryActivityLogDetailPage({ params }: { params: { id: string } }) {
  return <DeliveryActivityLog planId={params.id} />
}