import { useEffect } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2 } from 'lucide-react'
import {
  useCreateInspection,
  useUpdateInspection,
} from '../../core/services/queries'
import { inspectionService } from '../../core/services/beehiveService'
import { useQuery } from '@tanstack/react-query'
import { queryKeys } from '../../core/services/queries'
import { LoadingSpinner, ErrorMessage, PageHeader } from '../../shared/components'
import { HoneyLevel, HoneyLevelLabels } from '../../core/models'
import type { CreateInspectionPayload } from '../../core/models'

export default function InspectionFormPage() {
  const { id } = useParams<{ id?: string }>()
  const [searchParams] = useSearchParams()
  const isEditing = Boolean(id)
  const inspectionId = Number(id)
  const beehiveId = Number(searchParams.get('beehiveId') ?? 0)
  const navigate = useNavigate()

  // When editing, load existing inspection
  const { data: inspection, isLoading } = useQuery({
    queryKey: queryKeys.inspection(inspectionId),
    queryFn: () => inspectionService.getById(inspectionId),
    enabled: isEditing && !!inspectionId,
  })

  const resolvedBeehiveId = isEditing ? (inspection?.beehiveId ?? beehiveId) : beehiveId

  const createMutation = useCreateInspection(resolvedBeehiveId)
  const updateMutation = useUpdateInspection(inspectionId, resolvedBeehiveId)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateInspectionPayload>({
    defaultValues: {
      date: new Date().toISOString().split('T')[0],
      honeyLevel: HoneyLevel.Medium,
      beehiveId: beehiveId || undefined,
    },
  })

  useEffect(() => {
    if (inspection && isEditing) {
      reset({
        date: inspection.date.split('T')[0],
        temperature: inspection.temperature ?? undefined,
        honeyLevel: inspection.honeyLevel,
        broodStatus: inspection.broodStatus ?? '',
        notes: inspection.notes ?? '',
        beehiveId: inspection.beehiveId,
      })
    }
  }, [inspection, isEditing, reset])

  const onSubmit = async (data: CreateInspectionPayload) => {
    const payload: CreateInspectionPayload = {
      ...data,
      honeyLevel: Number(data.honeyLevel),
      beehiveId: resolvedBeehiveId,
      temperature: data.temperature ? Number(data.temperature) : undefined,
    }

    if (isEditing) {
      await updateMutation.mutateAsync(payload)
    } else {
      await createMutation.mutateAsync(payload)
    }
    navigate(`/beehives/${resolvedBeehiveId}`)
  }

  if (isEditing && isLoading) return <LoadingSpinner message="Loading inspection…" />

  const mutationError = createMutation.error ?? updateMutation.error
  const backUrl = `/beehives/${resolvedBeehiveId}`

  return (
    <div className="animate-fade-in max-w-lg mx-auto">
      <PageHeader
        title={isEditing ? 'Edit Inspection' : 'Record Inspection'}
        backButton={
          <button
            onClick={() => navigate(backUrl)}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-honey-600 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> Back to Beehive
          </button>
        }
      />

      <div className="card">
        {mutationError && (
          <div className="mb-5">
            <ErrorMessage message={mutationError.message} />
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-5">
          {/* Date */}
          <div>
            <label className="form-label" htmlFor="date">
              Inspection Date <span className="text-red-500">*</span>
            </label>
            <input
              id="date"
              type="date"
              className="form-input"
              max={new Date().toISOString().split('T')[0]}
              {...register('date', { required: 'Inspection date is required' })}
            />
            {errors.date && <p className="form-error">{errors.date.message}</p>}
          </div>

          {/* Temperature + Honey Level row */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="form-label" htmlFor="temperature">
                Temperature (°C)
              </label>
              <input
                id="temperature"
                type="number"
                step="0.1"
                min="-50"
                max="60"
                placeholder="e.g. 22.5"
                className="form-input"
                {...register('temperature', {
                  min: { value: -50, message: 'Min -50°C' },
                  max: { value: 60, message: 'Max 60°C' },
                })}
              />
              {errors.temperature && <p className="form-error">{errors.temperature.message}</p>}
            </div>

            <div>
              <label className="form-label" htmlFor="honeyLevel">
                Honey Level <span className="text-red-500">*</span>
              </label>
              <select
                id="honeyLevel"
                className="form-input"
                {...register('honeyLevel', { required: 'Honey level is required' })}
              >
                {Object.entries(HoneyLevelLabels).map(([val, label]) => (
                  <option key={val} value={val}>{label}</option>
                ))}
              </select>
              {errors.honeyLevel && <p className="form-error">{errors.honeyLevel.message}</p>}
            </div>
          </div>

          {/* Brood Status */}
          <div>
            <label className="form-label" htmlFor="broodStatus">Brood Status</label>
            <input
              id="broodStatus"
              type="text"
              placeholder="e.g. Healthy pattern, queen spotted, eggs visible…"
              className="form-input"
              {...register('broodStatus', {
                maxLength: { value: 500, message: 'Max 500 characters' },
              })}
            />
            {errors.broodStatus && <p className="form-error">{errors.broodStatus.message}</p>}
          </div>

          {/* Notes */}
          <div>
            <label className="form-label" htmlFor="notes">Notes</label>
            <textarea
              id="notes"
              rows={3}
              placeholder="Any other observations, treatments applied, honey harvested…"
              className="form-input resize-none"
              {...register('notes', {
                maxLength: { value: 2000, message: 'Notes must not exceed 2000 characters' },
              })}
            />
            {errors.notes && <p className="form-error">{errors.notes.message}</p>}
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={() => navigate(backUrl)}
              className="btn-secondary flex-1"
            >
              Cancel
            </button>
            <button type="submit" className="btn-primary flex-1" disabled={isSubmitting}>
              {isSubmitting ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : isEditing ? (
                'Save Changes'
              ) : (
                'Record Inspection'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
