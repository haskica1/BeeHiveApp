import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Pencil, Plus, Trash2 } from 'lucide-react'
import { format } from 'date-fns'
import { useApiary, useDeleteBeehive } from '../../core/services/queries'
import {
  LoadingSpinner,
  ErrorMessage,
  EmptyState,
  ConfirmDialog,
  PageHeader,
  HoneyLevelBadge,
} from '../../shared/components'
import type { Beehive } from '../../core/models'

export default function ApiaryDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const apiaryId = Number(id)

  const { data: apiary, isLoading, error } = useApiary(apiaryId)
  const deleteMutation = useDeleteBeehive(apiaryId)

  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null)

  const handleDeleteBeehive = async () => {
    if (!deleteTarget) return
    await deleteMutation.mutateAsync(deleteTarget.id)
    setDeleteTarget(null)
  }

  if (isLoading) return <LoadingSpinner message="Loading apiary…" />
  if (error) return <ErrorMessage message={error.message} />
  if (!apiary) return null

  return (
    <div className="animate-fade-in">
      <PageHeader
        title={apiary.name}
        subtitle={apiary.description ?? undefined}
        backButton={
          <button
            onClick={() => navigate('/apiaries')}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-honey-600 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> All Apiaries
          </button>
        }
        actions={
          <>
            <Link to={`/apiaries/${apiaryId}/edit`} className="btn-secondary text-sm">
              <Pencil className="w-4 h-4" /> Edit
            </Link>
            <Link
              to={`/beehives/new?apiaryId=${apiaryId}`}
              className="btn-primary text-sm"
            >
              <Plus className="w-4 h-4" /> Add Beehive
            </Link>
          </>
        }
      />

      {/* Stats strip */}
      <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 mb-6">
        <StatCard icon="🐝" label="Beehives" value={apiary.beehiveCount} />
        <StatCard
          icon="📋"
          label="Total Inspections"
          value={apiary.beehives?.reduce((s, b) => s + b.inspectionCount, 0) ?? 0}
        />
        <StatCard
          icon="📅"
          label="Since"
          value={format(new Date(apiary.createdAt), 'MMM yyyy')}
        />
      </div>

      {/* Beehive list */}
      <h2 className="font-display text-xl font-semibold text-gray-800 mb-4">Beehives</h2>

      {!apiary.beehives?.length ? (
        <EmptyState
          title="No beehives yet"
          description="Add your first beehive to this apiary."
          action={
            <Link to={`/beehives/new?apiaryId=${apiaryId}`} className="btn-primary text-sm">
              <Plus className="w-4 h-4" /> Add Beehive
            </Link>
          }
        />
      ) : (
        <div className="grid gap-3 sm:grid-cols-2">
          {apiary.beehives.map((beehive: Beehive) => (
            <div
              key={beehive.id}
              className="card hover:shadow-honey hover:-translate-y-0.5 transition-all duration-200 group cursor-pointer"
              onClick={() => navigate(`/beehives/${beehive.id}`)}
            >
              <div className="flex items-start gap-3">
                <span className="text-2xl shrink-0 mt-0.5">🏠</span>
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between gap-2">
                    <h3 className="font-semibold text-gray-800 truncate group-hover:text-honey-700 transition-colors">
                      {beehive.name}
                    </h3>
                    <div
                      className="flex gap-1 shrink-0"
                      onClick={e => e.stopPropagation()}
                    >
                      <Link
                        to={`/beehives/${beehive.id}/edit`}
                        className="p-1.5 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
                      >
                        <Pencil className="w-3.5 h-3.5" />
                      </Link>
                      <button
                        onClick={() => setDeleteTarget({ id: beehive.id, name: beehive.name })}
                        className="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 transition-colors"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>

                  <div className="flex flex-wrap gap-1.5 mt-1.5">
                    <span className="badge bg-honey-100 text-honey-700">{beehive.typeName}</span>
                    <span className="badge bg-gray-100 text-gray-600">{beehive.materialName}</span>
                  </div>

                  <p className="mt-2 text-xs text-gray-500">
                    📋 {beehive.inspectionCount} inspection{beehive.inspectionCount !== 1 ? 's' : ''}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        isOpen={!!deleteTarget}
        title="Delete Beehive"
        message={`Delete "${deleteTarget?.name}"? All inspection records will also be removed.`}
        onConfirm={handleDeleteBeehive}
        onCancel={() => setDeleteTarget(null)}
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}

function StatCard({ icon, label, value }: { icon: string; label: string; value: string | number }) {
  return (
    <div className="card text-center py-4">
      <div className="text-2xl mb-1">{icon}</div>
      <div className="font-display text-xl font-bold text-honey-700">{value}</div>
      <div className="text-xs text-gray-500 mt-0.5">{label}</div>
    </div>
  )
}
