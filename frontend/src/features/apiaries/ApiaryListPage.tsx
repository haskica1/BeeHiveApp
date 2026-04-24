import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Plus, Pencil, Trash2, ChevronRight } from 'lucide-react'
import { format } from 'date-fns'
import {
  useApiaries,
  useDeleteApiary,
} from '../../core/services/queries'
import {
  LoadingSpinner,
  ErrorMessage,
  EmptyState,
  ConfirmDialog,
  PageHeader,
} from '../../shared/components'
import { usePermissions } from '../../core/hooks/usePermissions'

export default function ApiaryListPage() {
  const navigate = useNavigate()
  const { data: apiaries, isLoading, error } = useApiaries()
  const deleteMutation = useDeleteApiary()
  const { canManageApiaries } = usePermissions()

  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null)

  const handleDelete = async () => {
    if (!deleteTarget) return
    await deleteMutation.mutateAsync(deleteTarget.id)
    setDeleteTarget(null)
  }

  if (isLoading) return <LoadingSpinner message="Loading apiaries…" />
  if (error) return <ErrorMessage message={error.message} />

  return (
    <div className="animate-fade-in">
      <PageHeader
        title="My Apiaries"
        subtitle={`${apiaries?.length ?? 0} apiar${apiaries?.length === 1 ? 'y' : 'ies'} registered`}
        actions={
          canManageApiaries ? (
            <Link to="/apiaries/new" className="btn-primary text-sm">
              <Plus className="w-4 h-4" /> New Apiary
            </Link>
          ) : undefined
        }
      />

      {!apiaries?.length ? (
        <EmptyState
          title="No apiaries yet"
          description="Create your first apiary to start managing your beehives."
          action={
            canManageApiaries ? (
              <Link to="/apiaries/new" className="btn-primary text-sm">
                <Plus className="w-4 h-4" /> Create Apiary
              </Link>
            ) : undefined
          }
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {apiaries.map(apiary => (
            <div
              key={apiary.id}
              className="card hover:shadow-honey hover:-translate-y-0.5 transition-all duration-200 group cursor-pointer"
              onClick={() => navigate(`/apiaries/${apiary.id}`)}
            >
              {/* Header row */}
              <div className="flex items-start justify-between gap-2 mb-3">
                <div className="flex items-center gap-2.5 min-w-0">
                  <span className="text-2xl shrink-0">🏡</span>
                  <div className="min-w-0">
                    <h2 className="font-display text-lg font-semibold text-gray-800 truncate group-hover:text-honey-700 transition-colors">
                      {apiary.name}
                    </h2>
                    <p className="text-xs text-gray-400">
                      Created {format(new Date(apiary.createdAt), 'dd MMM yyyy')}
                    </p>
                  </div>
                </div>
                <ChevronRight className="w-5 h-5 text-gray-300 group-hover:text-honey-500 shrink-0 mt-0.5 transition-colors" />
              </div>

              {/* Description */}
              {apiary.description && (
                <p className="text-sm text-gray-500 mb-3 line-clamp-2">{apiary.description}</p>
              )}

              {/* Stats row */}
              <div className="flex items-center justify-between pt-3 border-t border-honey-100">
                <span className="flex items-center gap-1.5 text-sm text-gray-600">
                  <span className="text-base">🐝</span>
                  <strong className="text-honey-700">{apiary.beehiveCount}</strong>
                  {apiary.beehiveCount === 1 ? ' beehive' : ' beehives'}
                </span>

                {/* Action buttons — stop propagation so card click doesn't fire */}
                {canManageApiaries && (
                  <div
                    className="flex gap-1"
                    onClick={e => e.stopPropagation()}
                  >
                    <Link
                      to={`/apiaries/${apiary.id}/edit`}
                      className="p-1.5 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
                      title="Edit"
                    >
                      <Pencil className="w-4 h-4" />
                    </Link>
                    <button
                      onClick={() => setDeleteTarget({ id: apiary.id, name: apiary.name })}
                      className="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 transition-colors"
                      title="Delete"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        isOpen={!!deleteTarget}
        title="Delete Apiary"
        message={`Are you sure you want to delete "${deleteTarget?.name}"? This will also delete all its beehives and inspection records. This action cannot be undone.`}
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}
