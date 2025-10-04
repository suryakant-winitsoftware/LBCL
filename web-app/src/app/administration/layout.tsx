export default function AdministrationLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <div className="administration-module">
      {children}
    </div>
  )
}