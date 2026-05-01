import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Download, Pencil, Plus, QrCode, Thermometer, Trash2 } from 'lucide-react'
import { format } from 'date-fns'
import { jsPDF } from 'jspdf'
import {
  useBeehive, useDeleteInspection,
  useTodosByBeehive, useCreateTodo, useUpdateTodo, useDeleteTodo,
  useAssignableUsers,
  queryKeys,
} from '../../core/services/queries'
import {
  LoadingSpinner,
  ErrorMessage,
  EmptyState,
  ConfirmDialog,
  PageHeader,
  HoneyLevelBadge,
} from '../../shared/components'
import { TodoSection } from '../../shared/components/TodoSection'
import DietSection from '../diets/DietSection'
import type { Inspection } from '../../core/models'
import { usePermissions } from '../../core/hooks/usePermissions'

// ── PDF download helper ───────────────────────────────────────────────────────

function downloadQrPdf(beehiveName: string, uniqueId: string, qrBase64: string) {
  const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' })

  const pageW = doc.internal.pageSize.getWidth()

  // Title
  doc.setFont('helvetica', 'bold')
  doc.setFontSize(22)
  doc.setTextColor(180, 120, 20)        // honey colour
  doc.text('BeeHive', pageW / 2, 22, { align: 'center' })

  // Divider
  doc.setDrawColor(180, 120, 20)
  doc.setLineWidth(0.5)
  doc.line(20, 27, pageW - 20, 27)

  // Beehive name
  doc.setFont('helvetica', 'bold')
  doc.setFontSize(16)
  doc.setTextColor(40, 40, 40)
  doc.text(beehiveName, pageW / 2, 40, { align: 'center' })

  // QR code image — centre it
  const imgSize = 100
  const imgX = (pageW - imgSize) / 2
  doc.addImage(`data:image/png;base64,${qrBase64}`, 'PNG', imgX, 50, imgSize, imgSize)

  // Unique ID label
  doc.setFont('helvetica', 'normal')
  doc.setFontSize(9)
  doc.setTextColor(120, 120, 120)
  doc.text('Unique ID', pageW / 2, 158, { align: 'center' })

  doc.setFont('courier', 'normal')
  doc.setFontSize(10)
  doc.setTextColor(60, 60, 60)
  doc.text(uniqueId, pageW / 2, 165, { align: 'center' })

  // Footer
  doc.setFont('helvetica', 'italic')
  doc.setFontSize(8)
  doc.setTextColor(160, 160, 160)
  doc.text(
    `Generated ${format(new Date(), 'dd MMM yyyy')}`,
    pageW / 2,
    285,
    { align: 'center' },
  )

  doc.save(`beehive-${beehiveName.replace(/\s+/g, '-')}-qr.pdf`)
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function BeehiveDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const beehiveId = Number(id)

  const { canEditDelete, canManageInspections, canManageHiveTodos, isAssignedToHive } = usePermissions()
  const { data: beehive, isLoading, error } = useBeehive(beehiveId)
  const deleteMutation = useDeleteInspection(beehiveId)

  const todoKey = queryKeys.todosByBeehive(beehiveId)
  const { data: todos = [], isLoading: todosLoading } = useTodosByBeehive(beehiveId)
  const { data: assignableUsers = [] } = useAssignableUsers()
  const createTodo = useCreateTodo(todoKey)
  const updateTodo = useUpdateTodo(todoKey)
  const deleteTodo = useDeleteTodo(todoKey)

  const [deleteTarget, setDeleteTarget] = useState<{ id: number } | null>(null)
  const [qrOpen, setQrOpen] = useState(false)

  const handleDelete = async () => {
    if (!deleteTarget) return
    await deleteMutation.mutateAsync(deleteTarget.id)
    setDeleteTarget(null)
  }

  if (isLoading) return <LoadingSpinner message="Loading beehive…" />
  if (error) return <ErrorMessage message={error.message} />
  if (!beehive) return null

  const hasQr = !!beehive.uniqueId && !!beehive.qrCodeBase64

  return (
    <div className="animate-fade-in">
      <PageHeader
        title={beehive.name}
        subtitle={`${beehive.typeName} · ${beehive.materialName}`}
        backButton={
          <button
            onClick={() => navigate(`/apiaries/${beehive.apiaryId}`)}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-honey-600 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> Back to Apiary
          </button>
        }
        actions={
          <>
            {hasQr && (
              <button
                onClick={() => setQrOpen(true)}
                className="btn-secondary text-sm"
              >
                <QrCode className="w-4 h-4" /> QR Code
              </button>
            )}
            {canEditDelete && (
              <Link to={`/beehives/${beehiveId}/edit`} className="btn-secondary text-sm">
                <Pencil className="w-4 h-4" /> Edit
              </Link>
            )}
            <Link
              to={`/inspections/new?beehiveId=${beehiveId}`}
              className="btn-primary text-sm"
            >
              <Plus className="w-4 h-4" /> Add Inspection
            </Link>
          </>
        }
      />

      {/* Beehive info card */}
      <div className="card mb-6 bg-gradient-to-br from-honey-50 to-white">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-center">
          <InfoItem icon="🐝" label="Type" value={beehive.typeName} />
          <InfoItem icon="🪵" label="Material" value={beehive.materialName} />
          <InfoItem
            icon="📅"
            label="Established"
            value={format(new Date(beehive.dateCreated), 'dd MMM yyyy')}
          />
          <InfoItem
            icon="📋"
            label="Inspections"
            value={String(beehive.inspectionCount)}
          />
        </div>
        {beehive.notes && (
          <p className="mt-4 pt-4 border-t border-honey-100 text-sm text-gray-600 italic">
            📝 {beehive.notes}
          </p>
        )}
        {/* Unique ID chip */}
        {beehive.uniqueId && (
          <p className="mt-3 pt-3 border-t border-honey-100 text-xs text-gray-400 font-mono flex items-center gap-1.5">
            <QrCode className="w-3.5 h-3.5 shrink-0 text-honey-400" />
            {beehive.uniqueId}
          </p>
        )}
        {beehive.createdByName && (
          <p className="mt-3 pt-3 border-t border-honey-100 text-xs text-gray-500 flex items-center gap-1.5">
            👤 Created by {beehive.createdByName}
          </p>
        )}
      </div>

      {/* Feeding programmes */}
      <DietSection beehiveId={beehiveId} />

      {/* To-do list */}
      <TodoSection
        todos={todos}
        isLoading={todosLoading}
        beehiveId={beehiveId}
        assignableUsers={assignableUsers}
        canCreate={canManageHiveTodos || isAssignedToHive(beehiveId)}
        canManage={canManageHiveTodos || isAssignedToHive(beehiveId)}
        onCreate={p => createTodo.mutateAsync(p)}
        onUpdate={(id, p) => updateTodo.mutateAsync({ id, payload: p })}
        onDelete={id => deleteTodo.mutateAsync(id)}
        isMutating={createTodo.isPending || updateTodo.isPending || deleteTodo.isPending}
      />

      {/* Inspections */}
      <h2 className="font-display text-xl font-semibold text-gray-800 mb-4">Inspection History</h2>

      {!beehive.inspections?.length ? (
        <EmptyState
          title="No inspections recorded"
          description="Record your first inspection for this beehive."
          action={
            <Link to={`/inspections/new?beehiveId=${beehiveId}`} className="btn-primary text-sm">
              <Plus className="w-4 h-4" /> Record Inspection
            </Link>
          }
        />
      ) : (
        <div className="space-y-3">
          {beehive.inspections.map((inspection: Inspection) => (
            <div key={inspection.id} className="card animate-slide-up">
              {/* Header */}
              <div className="flex items-start justify-between gap-3 mb-3">
                <div>
                  <p className="font-semibold text-gray-800">
                    {format(new Date(inspection.date), 'EEEE, dd MMMM yyyy')}
                  </p>
                  <div className="flex flex-wrap items-center gap-2 mt-1.5">
                    <HoneyLevelBadge level={inspection.honeyLevel} />
                    {inspection.temperature != null && (
                      <span className="badge bg-blue-100 text-blue-700 flex items-center gap-1">
                        <Thermometer className="w-3 h-3" />
                        {inspection.temperature}°C
                      </span>
                    )}
                  </div>
                </div>
                {(canManageInspections || isAssignedToHive(beehiveId)) && (
                  <div className="flex gap-1 shrink-0">
                    <Link
                      to={`/inspections/${inspection.id}/edit?beehiveId=${beehiveId}`}
                      className="p-1.5 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
                    >
                      <Pencil className="w-3.5 h-3.5" />
                    </Link>
                    <button
                      onClick={() => setDeleteTarget({ id: inspection.id })}
                      className="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 transition-colors"
                    >
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                )}
              </div>

              {/* Details */}
              {inspection.broodStatus && (
                <div className="flex gap-2 text-sm text-gray-600 mb-1">
                  <span className="shrink-0 text-base">🐛</span>
                  <span><strong>Brood:</strong> {inspection.broodStatus}</span>
                </div>
              )}
              {inspection.notes && (
                <div className="flex gap-2 text-sm text-gray-500 mt-2 pt-2 border-t border-gray-100">
                  <span className="shrink-0">📝</span>
                  <span className="italic">{inspection.notes}</span>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Delete inspection confirmation */}
      <ConfirmDialog
        isOpen={!!deleteTarget}
        title="Delete Inspection"
        message="Are you sure you want to delete this inspection record? This cannot be undone."
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
        isLoading={deleteMutation.isPending}
      />

      {/* QR code modal */}
      {qrOpen && hasQr && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm"
          onClick={() => setQrOpen(false)}
        >
          <div
            className="bg-white rounded-2xl shadow-2xl p-8 max-w-sm w-full mx-4 animate-fade-in"
            onClick={e => e.stopPropagation()}
          >
            <div className="text-center mb-4">
              <h2 className="font-display text-xl font-bold text-gray-800">{beehive.name}</h2>
              <p className="text-xs text-gray-400 font-mono mt-1">{beehive.uniqueId}</p>
            </div>

            <img
              src={`data:image/png;base64,${beehive.qrCodeBase64}`}
              alt={`QR code for ${beehive.name}`}
              className="w-full max-w-[240px] mx-auto block rounded-lg border border-gray-100 p-2"
            />

            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setQrOpen(false)}
                className="btn-secondary flex-1"
              >
                Close
              </button>
              <button
                onClick={() =>
                  downloadQrPdf(beehive.name, beehive.uniqueId!, beehive.qrCodeBase64!)
                }
                className="btn-primary flex-1"
              >
                <Download className="w-4 h-4" /> Download PDF
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

function InfoItem({ icon, label, value }: { icon: string; label: string; value: string }) {
  return (
    <div>
      <div className="text-xl mb-1">{icon}</div>
      <div className="text-xs text-gray-500 mb-0.5">{label}</div>
      <div className="text-sm font-semibold text-gray-800">{value}</div>
    </div>
  )
}
