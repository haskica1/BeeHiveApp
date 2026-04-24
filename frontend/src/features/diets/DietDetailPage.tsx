import { useState } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import {
  ArrowLeft, CheckCircle2, Circle, Clock, Pencil, Trash2,
  CalendarDays, Utensils, AlertTriangle, ChevronDown, ChevronUp,
} from 'lucide-react'
import { format, isPast, isToday } from 'date-fns'
import {
  useDiet,
  useDeleteDiet,
  useCompleteEarlyDiet,
  useCompleteFeedingEntry,
} from '../../core/services/queries'
import {
  LoadingSpinner, ErrorMessage, ConfirmDialog, PageHeader,
} from '../../shared/components'
import { DietStatus, FeedingEntryStatus } from '../../core/models'
import type { FeedingEntry } from '../../core/models'
import { usePermissions } from '../../core/hooks/usePermissions'

// ── Status badge ──────────────────────────────────────────────────────────────

function StatusBadge({ status, statusName }: { status: DietStatus; statusName: string }) {
  const styles: Record<DietStatus, string> = {
    [DietStatus.NotStarted]:   'bg-gray-100 text-gray-600',
    [DietStatus.InProgress]:   'bg-blue-100 text-blue-700',
    [DietStatus.Completed]:    'bg-green-100 text-green-700',
    [DietStatus.StoppedEarly]: 'bg-red-100 text-red-600',
  }
  return (
    <span className={`badge ${styles[status]}`}>{statusName}</span>
  )
}

// ── Feeding entry row ─────────────────────────────────────────────────────────

function EntryRow({
  entry,
  canComplete,
  onComplete,
  isCompleting,
}: {
  entry: FeedingEntry
  canComplete: boolean
  onComplete: (id: number) => void
  isCompleting: boolean
}) {
  const date = new Date(entry.scheduledDate)
  const done = entry.status === FeedingEntryStatus.Completed
  const overdue = !done && isPast(date) && !isToday(date)
  const today = !done && isToday(date)

  return (
    <div
      className={`flex items-center gap-3 py-3 px-4 rounded-xl border transition-colors ${
        done
          ? 'bg-green-50 border-green-100'
          : overdue
          ? 'bg-red-50 border-red-100'
          : today
          ? 'bg-amber-50 border-amber-100'
          : 'bg-white border-gray-100'
      }`}
    >
      {/* Checkbox / icon */}
      {done ? (
        <CheckCircle2 className="w-5 h-5 text-green-500 shrink-0" />
      ) : (
        <button
          onClick={() => canComplete && onComplete(entry.id)}
          disabled={!canComplete || isCompleting}
          className={`shrink-0 transition-colors ${
            canComplete
              ? 'text-gray-300 hover:text-honey-500 cursor-pointer'
              : 'text-gray-200 cursor-not-allowed'
          }`}
          title={canComplete ? 'Mark as completed' : 'Diet is finished'}
        >
          <Circle className="w-5 h-5" />
        </button>
      )}

      {/* Date info */}
      <div className="flex-1 min-w-0">
        <p className={`text-sm font-medium ${done ? 'text-gray-500 line-through' : 'text-gray-800'}`}>
          {format(date, 'EEEE, dd MMM yyyy')}
          {today && <span className="ml-2 text-xs font-semibold text-amber-600">Today</span>}
          {overdue && <span className="ml-2 text-xs font-semibold text-red-500">Overdue</span>}
        </p>
        {done && entry.completionDate && (
          <p className="text-xs text-gray-400 mt-0.5">
            Completed {format(new Date(entry.completionDate), 'dd MMM yyyy, HH:mm')}
          </p>
        )}
      </div>

      {/* Status chip */}
      <span className={`badge shrink-0 ${
        done ? 'bg-green-100 text-green-700' :
        overdue ? 'bg-red-100 text-red-600' :
        today ? 'bg-amber-100 text-amber-700' :
        'bg-gray-100 text-gray-500'
      }`}>
        {done ? 'Done' : overdue ? 'Overdue' : today ? 'Today' : 'Pending'}
      </span>
    </div>
  )
}

// ── Early-complete modal ──────────────────────────────────────────────────────

function CompleteEarlyModal({
  onConfirm,
  onCancel,
  isLoading,
}: {
  onConfirm: (comment: string) => void
  onCancel: () => void
  isLoading: boolean
}) {
  const [comment, setComment] = useState('')
  const [err, setErr] = useState('')

  function submit() {
    if (!comment.trim()) { setErr('Please enter a reason.'); return }
    onConfirm(comment.trim())
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md animate-fade-in">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 rounded-full bg-red-100 flex items-center justify-center shrink-0">
            <AlertTriangle className="w-5 h-5 text-red-500" />
          </div>
          <div>
            <h2 className="font-semibold text-gray-800">Stop Diet Early</h2>
            <p className="text-sm text-gray-500">Please explain why the diet is being stopped early.</p>
          </div>
        </div>

        <textarea
          className={`form-input resize-none h-24 ${err ? 'border-red-400' : ''}`}
          placeholder="Reason for early completion…"
          value={comment}
          onChange={e => { setComment(e.target.value); setErr('') }}
        />
        {err && <p className="form-error mt-1">{err}</p>}

        <div className="flex gap-3 mt-4">
          <button onClick={onCancel} className="btn-secondary flex-1" disabled={isLoading}>
            Cancel
          </button>
          <button onClick={submit} className="btn-primary flex-1 bg-red-500 hover:bg-red-600" disabled={isLoading}>
            {isLoading ? 'Stopping…' : 'Stop Diet'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Main page ─────────────────────────────────────────────────────────────────

export default function DietDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const dietId = Number(id)

  const { canEditDelete } = usePermissions()
  const { data: diet, isLoading, error } = useDiet(dietId)
  const deleteMutation   = useDeleteDiet(diet?.beehiveId ?? 0)
  const completeMutation = useCompleteEarlyDiet(dietId, diet?.beehiveId ?? 0)
  const entryMutation    = useCompleteFeedingEntry(dietId, diet?.beehiveId ?? 0)

  const [showDeleteConfirm,  setShowDeleteConfirm]  = useState(false)
  const [showCompleteEarly,  setShowCompleteEarly]   = useState(false)
  const [showCompleted,      setShowCompleted]       = useState(true)

  if (isLoading) return <LoadingSpinner message="Loading diet…" />
  if (error)     return <ErrorMessage message={error.message} />
  if (!diet)     return null

  const isFinished = diet.status === DietStatus.Completed || diet.status === DietStatus.StoppedEarly
  const canDelete  = canEditDelete && diet.status === DietStatus.NotStarted
  const canEdit    = canEditDelete && !isFinished

  const pendingEntries   = diet.feedingEntries.filter(e => e.status === FeedingEntryStatus.Pending)
  const completedEntries = diet.feedingEntries.filter(e => e.status === FeedingEntryStatus.Completed)

  async function handleDelete() {
    await deleteMutation.mutateAsync(dietId)
    navigate(`/beehives/${diet.beehiveId}`)
  }

  async function handleCompleteEarly(comment: string) {
    await completeMutation.mutateAsync({ comment })
    setShowCompleteEarly(false)
  }

  const progressPct = diet.totalEntries > 0
    ? Math.round((diet.completedEntries / diet.totalEntries) * 100)
    : 0

  return (
    <div className="animate-fade-in">
      <PageHeader
        title={diet.name}
        subtitle={`${diet.foodTypeName} · ${diet.reasonName}`}
        backButton={
          <button
            onClick={() => navigate(`/beehives/${diet.beehiveId}`)}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-honey-600 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> Back to Beehive
          </button>
        }
        actions={
          <div className="flex gap-2 flex-wrap justify-end">
            {canDelete && (
              <button
                onClick={() => setShowDeleteConfirm(true)}
                className="btn-secondary text-sm text-red-500 hover:text-red-600 hover:bg-red-50 border-red-200"
              >
                <Trash2 className="w-4 h-4" /> Delete
              </button>
            )}
            {canEditDelete && !isFinished && (
              <button
                onClick={() => setShowCompleteEarly(true)}
                className="btn-secondary text-sm"
              >
                Stop Early
              </button>
            )}
            {canEdit && (
              <Link to={`/diets/${dietId}/edit`} className="btn-secondary text-sm">
                <Pencil className="w-4 h-4" /> Edit
              </Link>
            )}
          </div>
        }
      />

      {/* Summary card */}
      <div className="card mb-6 bg-gradient-to-br from-honey-50 to-white">
        <div className="flex items-center justify-between gap-4 mb-4">
          <StatusBadge status={diet.status} statusName={diet.statusName} />
          <span className="text-sm text-gray-500">
            {diet.completedEntries} / {diet.totalEntries} feedings
          </span>
        </div>

        {/* Progress bar */}
        <div className="h-2 bg-gray-100 rounded-full overflow-hidden mb-4">
          <div
            className={`h-full rounded-full transition-all duration-500 ${
              diet.status === DietStatus.Completed ? 'bg-green-400' :
              diet.status === DietStatus.StoppedEarly ? 'bg-red-400' :
              'bg-honey-400'
            }`}
            style={{ width: `${progressPct}%` }}
          />
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-center text-sm">
          <InfoItem icon={<CalendarDays className="w-4 h-4" />} label="Start Date"
            value={format(new Date(diet.startDate), 'dd MMM yyyy')} />
          <InfoItem icon={<Clock className="w-4 h-4" />} label="Duration"
            value={`${diet.durationDays} days`} />
          <InfoItem icon={<Clock className="w-4 h-4" />} label="Frequency"
            value={`Every ${diet.frequencyDays} day${diet.frequencyDays !== 1 ? 's' : ''}`} />
          <InfoItem icon={<Utensils className="w-4 h-4" />} label="Food"
            value={diet.foodType === 5 && diet.customFoodType ? diet.customFoodType : diet.foodTypeName} />
        </div>

        {diet.status === DietStatus.StoppedEarly && diet.earlyCompletionComment && (
          <div className="mt-4 pt-4 border-t border-red-100 flex gap-2 text-sm text-red-700">
            <AlertTriangle className="w-4 h-4 shrink-0 mt-0.5" />
            <span><strong>Stopped early:</strong> {diet.earlyCompletionComment}</span>
          </div>
        )}
        {diet.createdByName && (
          <p className="mt-3 pt-3 border-t border-honey-100 text-xs text-gray-500 flex items-center gap-1.5">
            👤 Created by {diet.createdByName}
          </p>
        )}
      </div>

      {/* Pending entries */}
      <h2 className="font-display text-lg font-semibold text-gray-800 mb-3">
        Upcoming Feedings
        {pendingEntries.length > 0 && (
          <span className="ml-2 text-sm font-normal text-gray-400">({pendingEntries.length})</span>
        )}
      </h2>

      {pendingEntries.length === 0 ? (
        <div className="card text-center text-gray-400 py-8 mb-6">
          <CheckCircle2 className="w-10 h-10 mx-auto mb-2 text-green-300" />
          <p className="text-sm">All feedings accounted for.</p>
        </div>
      ) : (
        <div className="space-y-2 mb-6">
          {pendingEntries.map(entry => (
            <EntryRow
              key={entry.id}
              entry={entry}
              canComplete={!isFinished}
              onComplete={id => entryMutation.mutate(id)}
              isCompleting={entryMutation.isPending}
            />
          ))}
        </div>
      )}

      {/* Completed entries (collapsible) */}
      {completedEntries.length > 0 && (
        <div className="mb-8">
          <button
            onClick={() => setShowCompleted(v => !v)}
            className="flex items-center gap-2 text-sm font-semibold text-gray-600 hover:text-gray-800 mb-3 transition-colors"
          >
            {showCompleted
              ? <ChevronUp className="w-4 h-4" />
              : <ChevronDown className="w-4 h-4" />
            }
            Completed Feedings ({completedEntries.length})
          </button>

          {showCompleted && (
            <div className="space-y-2 opacity-80">
              {completedEntries.map(entry => (
                <EntryRow
                  key={entry.id}
                  entry={entry}
                  canComplete={false}
                  onComplete={() => {}}
                  isCompleting={false}
                />
              ))}
            </div>
          )}
        </div>
      )}

      {/* Delete confirm */}
      <ConfirmDialog
        isOpen={showDeleteConfirm}
        title="Delete Diet"
        message="Are you sure you want to delete this diet? This cannot be undone."
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteConfirm(false)}
        isLoading={deleteMutation.isPending}
      />

      {/* Complete early modal */}
      {showCompleteEarly && (
        <CompleteEarlyModal
          onConfirm={handleCompleteEarly}
          onCancel={() => setShowCompleteEarly(false)}
          isLoading={completeMutation.isPending}
        />
      )}
    </div>
  )
}

function InfoItem({
  icon, label, value,
}: {
  icon: React.ReactNode; label: string; value: string
}) {
  return (
    <div>
      <div className="flex justify-center mb-1 text-honey-500">{icon}</div>
      <div className="text-xs text-gray-500 mb-0.5">{label}</div>
      <div className="text-sm font-semibold text-gray-800">{value}</div>
    </div>
  )
}
