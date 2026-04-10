import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2, MapPin } from 'lucide-react'
import { useApiary, useCreateApiary, useUpdateApiary } from '../../core/services/queries'
import { LoadingSpinner, ErrorMessage, PageHeader } from '../../shared/components'
import type { CreateApiaryPayload } from '../../core/models'

export default function ApiaryFormPage() {
  const { id } = useParams<{ id?: string }>()
  const isEditing = Boolean(id)
  const apiaryId = Number(id)
  const navigate = useNavigate()

  const { data: apiary, isLoading } = useApiary(apiaryId)
  const createMutation = useCreateApiary()
  const updateMutation = useUpdateApiary(apiaryId)

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<CreateApiaryPayload>()

  const lat = watch('latitude')
  const lon = watch('longitude')
  const bothFilled = lat != null && lat !== ('' as never) && lon != null && lon !== ('' as never)

  // Populate form when editing
  useEffect(() => {
    if (apiary && isEditing) {
      reset({
        name: apiary.name,
        description: apiary.description ?? '',
        latitude: apiary.latitude ?? undefined,
        longitude: apiary.longitude ?? undefined,
      })
    }
  }, [apiary, isEditing, reset])

  const onSubmit = async (data: CreateApiaryPayload) => {
    // Coerce empty string → null so the API receives null, not ''
    const payload: CreateApiaryPayload = {
      ...data,
      latitude:  data.latitude  !== ('' as never) ? Number(data.latitude)  : null,
      longitude: data.longitude !== ('' as never) ? Number(data.longitude) : null,
    }
    if (isEditing) {
      await updateMutation.mutateAsync(payload)
      navigate(`/apiaries/${apiaryId}`)
    } else {
      const created = await createMutation.mutateAsync(payload)
      navigate(`/apiaries/${created.id}`)
    }
  }

  if (isEditing && isLoading) return <LoadingSpinner message="Loading apiary…" />

  const mutationError = createMutation.error ?? updateMutation.error

  return (
    <div className="animate-fade-in max-w-lg mx-auto">
      <PageHeader
        title={isEditing ? 'Edit Apiary' : 'New Apiary'}
        backButton={
          <button
            onClick={() => navigate(isEditing ? `/apiaries/${apiaryId}` : '/apiaries')}
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
          {/* Name */}
          <div>
            <label className="form-label" htmlFor="name">
              Apiary Name <span className="text-red-500">*</span>
            </label>
            <input
              id="name"
              type="text"
              placeholder="e.g. Mountain Apiary"
              className="form-input"
              {...register('name', {
                required: 'Apiary name is required',
                maxLength: { value: 200, message: 'Name must not exceed 200 characters' },
              })}
            />
            {errors.name && <p className="form-error">{errors.name.message}</p>}
          </div>

          {/* Description */}
          <div>
            <label className="form-label" htmlFor="description">
              Description
            </label>
            <textarea
              id="description"
              rows={3}
              placeholder="Location details, flora type, notes…"
              className="form-input resize-none"
              {...register('description', {
                maxLength: { value: 1000, message: 'Description must not exceed 1000 characters' },
              })}
            />
            {errors.description && <p className="form-error">{errors.description.message}</p>}
          </div>

          {/* Location */}
          <div>
            <label className="form-label flex items-center gap-1.5">
              <MapPin className="w-3.5 h-3.5 text-honey-600" />
              Location <span className="text-gray-400 font-normal">(for weather forecast)</span>
            </label>
            <p className="text-xs text-gray-500 mb-2">
              Find coordinates on{' '}
              <a
                href="https://maps.google.com"
                target="_blank"
                rel="noreferrer"
                className="text-honey-600 hover:underline"
              >
                Google Maps
              </a>
              {' '}— right-click any spot and copy the lat/lng shown.
            </p>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block" htmlFor="latitude">
                  Latitude
                </label>
                <input
                  id="latitude"
                  type="number"
                  step="any"
                  placeholder="e.g. 43.8563"
                  className="form-input"
                  {...register('latitude', {
                    min: { value: -90,  message: 'Must be ≥ -90'  },
                    max: { value:  90,  message: 'Must be ≤ 90'   },
                  })}
                />
                {errors.latitude && <p className="form-error">{errors.latitude.message}</p>}
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block" htmlFor="longitude">
                  Longitude
                </label>
                <input
                  id="longitude"
                  type="number"
                  step="any"
                  placeholder="e.g. 18.4131"
                  className="form-input"
                  {...register('longitude', {
                    min: { value: -180, message: 'Must be ≥ -180' },
                    max: { value:  180, message: 'Must be ≤ 180'  },
                  })}
                />
                {errors.longitude && <p className="form-error">{errors.longitude.message}</p>}
              </div>
            </div>
            {bothFilled && (
              <a
                href={`https://maps.google.com/?q=${lat},${lon}`}
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-1 mt-2 text-xs text-honey-600 hover:underline"
              >
                <MapPin className="w-3 h-3" /> Preview on Google Maps
              </a>
            )}
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={() => navigate(isEditing ? `/apiaries/${apiaryId}` : '/apiaries')}
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
                'Create Apiary'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
