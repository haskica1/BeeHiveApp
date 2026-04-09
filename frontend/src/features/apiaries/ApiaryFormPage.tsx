import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2 } from 'lucide-react'
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
    formState: { errors, isSubmitting },
  } = useForm<CreateApiaryPayload>()

  // Populate form when editing
  useEffect(() => {
    if (apiary && isEditing) {
      reset({ name: apiary.name, description: apiary.description ?? '' })
    }
  }, [apiary, isEditing, reset])

  const onSubmit = async (data: CreateApiaryPayload) => {
    if (isEditing) {
      await updateMutation.mutateAsync(data)
      navigate(`/apiaries/${apiaryId}`)
    } else {
      const created = await createMutation.mutateAsync(data)
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
