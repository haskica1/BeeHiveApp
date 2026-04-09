import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Pencil, Plus, Thermometer, Trash2 } from 'lucide-react'
import { format } from 'date-fns'
import { useBeehive, useDeleteInspection } from '../../core/services/queries'
import {
  LoadingSpinner,
  ErrorMessage,
  EmptyState,
  ConfirmDialog,
  PageHeader,
  HoneyLevelBadge,
} from '../../shared/components'
import type { Inspection } from '../../core/models'

export default function BeehiveDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const beehiveId = Number(id)

  const { data: beehive, isLoading, error } = useBeehive(beehiveId)
  const deleteMutation = useDeleteInspection(beehiveId)

  const [deleteTarget, setDeleteTarget] = useState<{ id: number } | null>(null)

  const handleDelete = async () => {
    if (!deleteTarget) return
    await deleteMutation.mutateAsync(deleteTarget.id)
    setDeleteTarget(null)
  }

  if (isLoading) return <LoadingSpinner message="Loading beehive…" />
  if (error) return <ErrorMessage message={error.message} />
  if (!beehive) return null

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
            <Link to={`/beehives/${beehiveId}/edit`} className="btn-secondary text-sm">
              <Pencil className="w-4 h-4" /> Edit
            </Link>
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
      </div>

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

      <ConfirmDialog
        isOpen={!!deleteTarget}
        title="Delete Inspection"
        message="Are you sure you want to delete this inspection record? This cannot be undone."
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
        isLoading={deleteMutation.isPending}
      />
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
