import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Plus, Pencil, Trash2, ChevronRight, Search, MapPin, X } from 'lucide-react'
import { format } from 'date-fns'
import clsx from 'clsx'
import {
  useApiaries,
  useDeleteApiary,
} from '../../core/services/queries'
import {
  LoadingSpinner,
  ErrorMessage,
  EmptyState,
  ConfirmDialog,
} from '../../shared/components'
import type { Apiary } from '../../core/models'
import { usePermissions } from '../../core/hooks/usePermissions'

type SortKey = 'name' | 'hives' | 'newest'

const SORT_OPTIONS: { key: SortKey; label: string }[] = [
  { key: 'name',   label: 'Name' },
  { key: 'hives',  label: 'Most hives' },
  { key: 'newest', label: 'Newest' },
]

export default function ApiaryListPage() {
  const navigate = useNavigate()
  const { data, isLoading, error } = useApiaries()
  const deleteMutation = useDeleteApiary()
  const { canManageApiaries } = usePermissions()

  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null)
  const [query, setQuery] = useState('')
  const [sort, setSort] = useState<SortKey>('name')

  const apiaries = useMemo(() => data ?? [], [data])

  // ── Overview vitals (derived from the loaded list — no extra requests) ──
  const totalBeehives = apiaries.reduce((s, a) => s + a.beehiveCount, 0)
  const mappedCount   = apiaries.filter(a => a.hasLocation).length
  const avgHives      = apiaries.length ? (totalBeehives / apiaries.length) : 0

  // ── Filter + sort ──
  const visible = useMemo(() => {
    const q = query.trim().toLowerCase()
    let list = apiaries
    if (q) {
      list = list.filter(a =>
        a.name.toLowerCase().includes(q) ||
        (a.description?.toLowerCase().includes(q) ?? false),
      )
    }
    const sorted = [...list]
    sorted.sort((a, b) => {
      if (sort === 'hives')  return b.beehiveCount - a.beehiveCount
      if (sort === 'newest') return +new Date(b.createdAt) - +new Date(a.createdAt)
      return a.name.localeCompare(b.name)
    })
    return sorted
  }, [apiaries, query, sort])

  const handleDelete = async () => {
    if (!deleteTarget) return
    await deleteMutation.mutateAsync(deleteTarget.id)
    setDeleteTarget(null)
  }

  if (isLoading) return <LoadingSpinner message="Loading apiaries…" />
  if (error) return <ErrorMessage message={error.message} />

  return (
    <div className="animate-fade-in space-y-6">

      {/* ── Hero ──────────────────────────────────────────────────────────────── */}
      <div className="relative overflow-hidden rounded-3xl border border-honey-200 dark:border-slate-800
                      bg-gradient-to-br from-honey-100 via-white to-honey-50
                      dark:from-slate-900 dark:via-slate-900 dark:to-slate-950 shadow-card dark:shadow-none">
        <div className="absolute inset-0 bg-honeycomb opacity-60 dark:opacity-100 pointer-events-none" />
        <div className="relative p-5 sm:p-7 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div className="flex items-center gap-4 min-w-0">
            <div className="w-14 h-14 shrink-0 rounded-2xl bg-white/70 dark:bg-slate-800 border border-honey-200 dark:border-slate-700 flex items-center justify-center text-3xl shadow-honey dark:shadow-none">
              🏡
            </div>
            <div className="min-w-0">
              <h1 className="font-display text-2xl sm:text-3xl font-bold text-gray-900 dark:text-slate-50">
                My Apiaries
              </h1>
              <p className="mt-0.5 text-sm text-gray-600 dark:text-slate-400">
                {apiaries.length} apiar{apiaries.length === 1 ? 'y' : 'ies'}
                {apiaries.length > 0 && <> · {totalBeehives} beehive{totalBeehives === 1 ? '' : 's'} total</>}
              </p>
            </div>
          </div>

          {canManageApiaries && (
            <Link to="/apiaries/new" className="btn-primary text-sm shrink-0">
              <Plus className="w-4 h-4" /> New Apiary
            </Link>
          )}
        </div>
      </div>

      {apiaries.length === 0 ? (
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
        <>
          {/* ── Overview vitals ─────────────────────────────────────────────── */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
            <VitalCard icon="🏡" label="Apiaries"   value={String(apiaries.length)} sub="registered" gradient="from-honey-400 to-honey-600" />
            <VitalCard icon="🐝" label="Beehives"   value={String(totalBeehives)}   sub="across all"  gradient="from-amber-400 to-orange-500" />
            <VitalCard icon="📍" label="Mapped"     value={String(mappedCount)}     sub={`of ${apiaries.length} located`} gradient="from-sky-400 to-blue-600" />
            <VitalCard icon="📊" label="Avg hives"  value={avgHives.toFixed(1)}     sub="per apiary"  gradient="from-violet-400 to-indigo-600" />
          </div>

          {/* ── Search + sort toolbar ───────────────────────────────────────── */}
          <div className="flex flex-col sm:flex-row gap-3 sm:items-center">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 dark:text-slate-500 pointer-events-none" />
              <input
                value={query}
                onChange={e => setQuery(e.target.value)}
                placeholder="Search apiaries…"
                className="form-input pl-9 pr-9"
                aria-label="Search apiaries"
              />
              {query && (
                <button
                  onClick={() => setQuery('')}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-slate-500 hover:text-gray-600 dark:hover:text-slate-300 transition-colors"
                  aria-label="Clear search"
                >
                  <X className="w-4 h-4" />
                </button>
              )}
            </div>

            <div className="flex items-center gap-0.5 bg-gray-100 dark:bg-slate-800 rounded-xl p-1 shrink-0 self-start sm:self-auto">
              {SORT_OPTIONS.map(o => (
                <button
                  key={o.key}
                  onClick={() => setSort(o.key)}
                  className={clsx(
                    'px-3 py-1.5 rounded-lg text-sm font-medium transition-all',
                    sort === o.key
                      ? 'bg-white dark:bg-slate-700 text-honey-800 dark:text-honey-300 shadow-sm'
                      : 'text-gray-600 dark:text-slate-300 hover:text-honey-700 dark:hover:text-honey-300',
                  )}
                >
                  {o.label}
                </button>
              ))}
            </div>
          </div>

          {/* ── Grid / no-results ───────────────────────────────────────────── */}
          {visible.length === 0 ? (
            <div className="text-center py-16">
              <div className="w-14 h-14 mx-auto rounded-full bg-honey-100 dark:bg-honey-500/15 flex items-center justify-center mb-3">
                <Search className="w-6 h-6 text-honey-400" />
              </div>
              <p className="text-gray-600 dark:text-slate-300 font-medium">No apiaries match “{query}”.</p>
              <button onClick={() => setQuery('')} className="mt-3 text-sm text-honey-600 dark:text-honey-400 hover:underline font-medium">
                Clear search
              </button>
            </div>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {visible.map(apiary => (
                <ApiaryCard
                  key={apiary.id}
                  apiary={apiary}
                  canManage={canManageApiaries}
                  onOpen={() => navigate(`/apiaries/${apiary.id}`)}
                  onDelete={() => setDeleteTarget({ id: apiary.id, name: apiary.name })}
                />
              ))}
            </div>
          )}
        </>
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

// ── Apiary card ────────────────────────────────────────────────────────────────

function ApiaryCard({ apiary, canManage, onOpen, onDelete }: {
  apiary: Apiary
  canManage: boolean
  onOpen: () => void
  onDelete: () => void
}) {
  return (
    <div
      onClick={onOpen}
      className="group relative card cursor-pointer flex flex-col hover:-translate-y-0.5 hover:shadow-honey dark:hover:shadow-none hover:border-honey-300 dark:hover:border-honey-500/40 transition-all duration-200"
    >
      {/* Header */}
      <div className="flex items-start gap-3">
        <div className="w-12 h-12 shrink-0 rounded-2xl bg-gradient-to-br from-honey-400 to-honey-600 text-white flex items-center justify-center text-2xl shadow-honey dark:shadow-none">
          🏡
        </div>
        <div className="flex-1 min-w-0">
          <h2 className="font-display text-lg font-semibold text-gray-800 dark:text-slate-100 truncate group-hover:text-honey-700 dark:group-hover:text-honey-400 transition-colors">
            {apiary.name}
          </h2>
          <p className="text-xs text-gray-400 dark:text-slate-500 mt-0.5 truncate">
            {format(new Date(apiary.createdAt), 'dd MMM yyyy')}
            {apiary.createdByName ? ` · ${apiary.createdByName}` : ''}
          </p>
        </div>
        <ChevronRight className="w-5 h-5 text-gray-300 dark:text-slate-600 group-hover:text-honey-500 group-hover:translate-x-0.5 shrink-0 mt-1 transition-all" />
      </div>

      {/* Description */}
      {apiary.description && (
        <p className="text-sm text-gray-500 dark:text-slate-400 mt-3 line-clamp-2">{apiary.description}</p>
      )}

      {/* Footer */}
      <div className="flex items-center gap-2 mt-4 pt-3 border-t border-honey-100 dark:border-slate-800">
        <span className="badge bg-honey-100 text-honey-700 dark:bg-honey-500/15 dark:text-honey-300 gap-1">
          🐝 {apiary.beehiveCount} {apiary.beehiveCount === 1 ? 'hive' : 'hives'}
        </span>
        {apiary.hasLocation && (
          <span className="badge bg-sky-100 text-sky-700 dark:bg-sky-500/15 dark:text-sky-300 gap-1">
            <MapPin className="w-3 h-3" /> Located
          </span>
        )}

        {canManage && (
          <div
            className="ml-auto flex gap-1 opacity-100 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity"
            onClick={e => e.stopPropagation()}
          >
            <Link
              to={`/apiaries/${apiary.id}/edit`}
              className="p-1.5 rounded-lg text-gray-400 dark:text-slate-500 hover:text-honey-600 dark:hover:text-honey-400 hover:bg-honey-50 dark:hover:bg-slate-800 transition-colors"
              title="Edit"
            >
              <Pencil className="w-4 h-4" />
            </Link>
            <button
              onClick={onDelete}
              className="p-1.5 rounded-lg text-gray-400 dark:text-slate-500 hover:text-red-500 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors"
              title="Delete"
            >
              <Trash2 className="w-4 h-4" />
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

// ── Vitals KPI tile ────────────────────────────────────────────────────────────

function VitalCard({ icon, label, value, sub, gradient }: {
  icon: string; label: string; value: string; sub?: string; gradient: string
}) {
  return (
    <div className={`relative overflow-hidden rounded-2xl p-4 sm:p-5 text-white shadow-lg bg-gradient-to-br ${gradient}`}>
      <span className="absolute -right-2 -top-3 text-6xl opacity-20 select-none pointer-events-none leading-none">
        {icon}
      </span>
      <div className="relative">
        <p className="text-2xl sm:text-3xl font-bold font-display leading-none truncate">{value}</p>
        <p className="text-sm font-medium opacity-95 mt-2">{label}</p>
        {sub && <p className="text-xs mt-0.5 opacity-80">{sub}</p>}
      </div>
    </div>
  )
}
