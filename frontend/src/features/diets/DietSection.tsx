import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ChevronRight, Plus, Leaf } from 'lucide-react'
import { CollapsibleSection } from '../../shared/components/CollapsibleSection'
import { format } from 'date-fns'
import { useDietsByBeehive } from '../../core/services/queries'
import { LoadingSpinner } from '../../shared/components'
import { DietStatus } from '../../core/models'
import type { Diet } from '../../core/models'
import { usePermissions } from '../../core/hooks/usePermissions'

// ── Status helpers ────────────────────────────────────────────────────────────

const STATUS_STYLES: Record<DietStatus, string> = {
  [DietStatus.NotStarted]:   'bg-gray-100 text-gray-600',
  [DietStatus.InProgress]:   'bg-blue-100 text-blue-700',
  [DietStatus.Completed]:    'bg-green-100 text-green-700',
  [DietStatus.StoppedEarly]: 'bg-red-100 text-red-600',
}

// ── Diet card ─────────────────────────────────────────────────────────────────

function DietCard({ diet }: { diet: Diet }) {
  const progressPct = diet.totalEntries > 0
    ? Math.round((diet.completedEntries / diet.totalEntries) * 100)
    : 0

  const barColor =
    diet.status === DietStatus.Completed    ? 'bg-green-400' :
    diet.status === DietStatus.StoppedEarly ? 'bg-red-400'   : 'bg-honey-400'

  return (
    <Link
      to={`/diets/${diet.id}`}
      className="card group flex items-center gap-4 hover:border-honey-300 hover:shadow-md transition-all animate-slide-up"
    >
      {/* Icon */}
      <div className="w-10 h-10 rounded-full bg-honey-100 flex items-center justify-center shrink-0 group-hover:bg-honey-200 transition-colors">
        <Leaf className="w-5 h-5 text-honey-600" />
      </div>

      {/* Info */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <p className="font-semibold text-gray-800 truncate">{diet.name}</p>
          <span className={`badge shrink-0 ${STATUS_STYLES[diet.status]}`}>
            {diet.statusName}
          </span>
        </div>

        <p className="text-xs text-gray-500 mb-2">
          {diet.foodTypeName}
          {diet.foodType === 5 && diet.customFoodType ? ` — ${diet.customFoodType}` : ''}
          {' · '}
          Started {format(new Date(diet.startDate), 'dd MMM yyyy')}
        </p>

        {/* Mini progress bar */}
        <div className="flex items-center gap-2">
          <div className="flex-1 h-1.5 bg-gray-100 rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full ${barColor} transition-all`}
              style={{ width: `${progressPct}%` }}
            />
          </div>
          <span className="text-xs text-gray-400 shrink-0">
            {diet.completedEntries}/{diet.totalEntries}
          </span>
        </div>
      </div>

      {/* Arrow */}
      <ChevronRight className="w-4 h-4 text-gray-300 group-hover:text-honey-500 transition-colors shrink-0" />
    </Link>
  )
}

// ── Section ───────────────────────────────────────────────────────────────────

export default function DietSection({ beehiveId }: { beehiveId: number }) {
  const { data: diets = [], isLoading } = useDietsByBeehive(beehiveId)
  const { canManageDiets, isAssignedToHive } = usePermissions()
  const canManageThisDiet = canManageDiets || isAssignedToHive(beehiveId)
  const [showAll, setShowAll] = useState(false)

  const active   = diets.filter(d => d.status === DietStatus.InProgress || d.status === DietStatus.NotStarted)
  const finished = diets.filter(d => d.status === DietStatus.Completed   || d.status === DietStatus.StoppedEarly)

  const visibleFinished = showAll ? finished : finished.slice(0, 2)

  const addAction = canManageThisDiet
    ? <Link to={`/diets/new?beehiveId=${beehiveId}`} className="btn-primary text-sm"><Plus className="w-4 h-4" /> Add Diet</Link>
    : null

  return (
    <CollapsibleSection
      title="Feeding Programmes"
      icon={<Leaf className="w-5 h-5 text-honey-500" />}
      count={diets.length}
      action={addAction}
    >
      {isLoading ? (
        <LoadingSpinner message="Loading diets…" />
      ) : diets.length === 0 ? (
        <div className="card text-center py-8 text-gray-400">
          <Leaf className="w-10 h-10 mx-auto mb-3 text-gray-200" />
          <p className="font-medium text-gray-500">No feeding programmes yet</p>
          <p className="text-sm mt-1">
            {canManageThisDiet
              ? 'Create a diet to track feeding schedules for this hive.'
              : 'No feeding programmes have been scheduled yet.'}
          </p>
          {canManageThisDiet && (
            <Link
              to={`/diets/new?beehiveId=${beehiveId}`}
              className="btn-primary text-sm mt-4 inline-flex"
            >
              <Plus className="w-4 h-4" /> Add First Diet
            </Link>
          )}
        </div>
      ) : (
        <>
          {/* Active diets */}
          {active.length > 0 && (
            <div className="space-y-3 mb-4">
              {active.map(d => <DietCard key={d.id} diet={d} />)}
            </div>
          )}

          {/* Finished diets */}
          {finished.length > 0 && (
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">
                Completed / Stopped
              </p>
              <div className="space-y-3 opacity-75">
                {visibleFinished.map(d => <DietCard key={d.id} diet={d} />)}
              </div>
              {finished.length > 2 && (
                <button
                  onClick={() => setShowAll(v => !v)}
                  className="mt-2 text-sm text-honey-600 hover:text-honey-700 font-medium"
                >
                  {showAll
                    ? 'Show less'
                    : `Show ${finished.length - 2} more…`}
                </button>
              )}
            </div>
          )}
        </>
      )}
    </CollapsibleSection>
  )
}
