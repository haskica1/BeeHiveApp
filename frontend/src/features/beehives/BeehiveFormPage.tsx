import { useEffect } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2 } from 'lucide-react'
import { useBeehive, useCreateBeehive, useUpdateBeehive } from '../../core/services/queries'
import { LoadingSpinner, ErrorMessage, PageHeader } from '../../shared/components'
import {
  BeehiveType,
  BeehiveTypeLabels,
  BeehiveMaterial,
  BeehiveMaterialLabels,
} from '../../core/models'
import type { CreateBeehivePayload } from '../../core/models'

export default function BeehiveFormPage() {
  const { id } = useParams<{ id?: string }>()
  const [searchParams] = useSearchParams()
  const isEditing = Boolean(id)
  const beehiveId = Number(id)
  const preselectedApiaryId = Number(searchParams.get('apiaryId') ?? 0)
  const navigate = useNavigate()

  const { data: beehive, isLoading } = useBeehive(beehiveId)
  const createMutation = useCreateBeehive(preselectedApiaryId)
  const updateMutation = useUpdateBeehive(beehiveId, beehive?.apiaryId ?? preselectedApiaryId)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateBeehivePayload>({
    defaultValues: {
      type: BeehiveType.Langstroth,
      material: BeehiveMaterial.Wood,
      dateCreated: new Date().toISOString().split('T')[0],
      apiaryId: preselectedApiaryId || undefined,
    },
  })

  useEffect(() => {
    if (beehive && isEditing) {
      reset({
        name: beehive.name,
        type: beehive.type,
        material: beehive.material,
        dateCreated: beehive.dateCreated.split('T')[0],
        notes: beehive.notes ?? '',
        apiaryId: beehive.apiaryId,
      })
    }
  }, [beehive, isEditing, reset])

  const onSubmit = async (data: CreateBeehivePayload) => {
    const payload = {
      ...data,
      type: Number(data.type),
      material: Number(data.material),
      apiaryId: Number(data.apiaryId),
    }

    if (isEditing) {
      await updateMutation.mutateAsync(payload)
      navigate(`/beehives/${beehiveId}`)
    } else {
      const created = await createMutation.mutateAsync(payload)
      navigate(`/beehives/${created.id}`)
    }
  }

  if (isEditing && isLoading) return <LoadingSpinner message="Loading beehive…" />

  const mutationError = createMutation.error ?? updateMutation.error
  const backUrl = isEditing
    ? `/beehives/${beehiveId}`
    : preselectedApiaryId
    ? `/apiaries/${preselectedApiaryId}`
    : '/apiaries'

  return (
    <div className="animate-fade-in max-w-lg mx-auto">
      <PageHeader
        title={isEditing ? 'Edit Beehive' : 'New Beehive'}
        backButton={
          <button
            onClick={() => navigate(backUrl)}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-honey-600 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> Back
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
          {/* Hidden field — keeps apiaryId in form state so handleSubmit always includes it */}
          <input type="hidden" {...register('apiaryId', { valueAsNumber: true })} />

          {/* Name */}
          <div>
            <label className="form-label" htmlFor="name">
              Name / Label <span className="text-red-500">*</span>
            </label>
            <input
              id="name"
              type="text"
              placeholder="e.g. Košnica A1"
              className="form-input"
              {...register('name', {
                required: 'Beehive name is required',
                maxLength: { value: 100, message: 'Name must not exceed 100 characters' },
              })}
            />
            {errors.name && <p className="form-error">{errors.name.message}</p>}
          </div>

          {/* Type + Material row */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="form-label" htmlFor="type">
                Type <span className="text-red-500">*</span>
              </label>
              <select
                id="type"
                className="form-input"
                {...register('type', { required: 'Type is required' })}
              >
                {Object.entries(BeehiveTypeLabels).map(([val, label]) => (
                  <option key={val} value={val}>{label}</option>
                ))}
              </select>
              {errors.type && <p className="form-error">{errors.type.message}</p>}
            </div>

            <div>
              <label className="form-label" htmlFor="material">
                Material <span className="text-red-500">*</span>
              </label>
              <select
                id="material"
                className="form-input"
                {...register('material', { required: 'Material is required' })}
              >
                {Object.entries(BeehiveMaterialLabels).map(([val, label]) => (
                  <option key={val} value={val}>{label}</option>
                ))}
              </select>
              {errors.material && <p className="form-error">{errors.material.message}</p>}
            </div>
          </div>

          {/* Date Created */}
          <div>
            <label className="form-label" htmlFor="dateCreated">
              Date Established <span className="text-red-500">*</span>
            </label>
            <input
              id="dateCreated"
              type="date"
              className="form-input"
              max={new Date().toISOString().split('T')[0]}
              {...register('dateCreated', { required: 'Date established is required' })}
            />
            {errors.dateCreated && <p className="form-error">{errors.dateCreated.message}</p>}
          </div>

          {/* Notes */}
          <div>
            <label className="form-label" htmlFor="notes">Notes</label>
            <textarea
              id="notes"
              rows={3}
              placeholder="Queen info, colony strength, special observations…"
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
                'Create Beehive'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
