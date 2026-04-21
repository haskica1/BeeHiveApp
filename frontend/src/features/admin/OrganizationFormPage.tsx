import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2 } from 'lucide-react'
import {
  useAdminOrganization,
  useCreateOrganization,
  useUpdateOrganization,
} from '../../core/services/adminQueries'

interface OrgForm {
  name: string
  description: string
}

export default function OrganizationFormPage() {
  const { id } = useParams<{ id: string }>()
  const isEdit = !!id
  const orgId = id ? parseInt(id) : 0
  const navigate = useNavigate()

  const { data: existing, isLoading: loadingExisting } = useAdminOrganization(orgId)
  const createOrg = useCreateOrganization()
  const updateOrg = useUpdateOrganization(orgId)

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<OrgForm>()

  useEffect(() => {
    if (existing) {
      reset({ name: existing.name, description: existing.description ?? '' })
    }
  }, [existing, reset])

  async function onSubmit(data: OrgForm) {
    const payload = {
      name: data.name,
      description: data.description || undefined,
    }
    try {
      if (isEdit) {
        await updateOrg.mutateAsync(payload)
      } else {
        await createOrg.mutateAsync(payload)
      }
      navigate('/admin')
    } catch (e: any) {
      const detail = e?.response?.data?.detail ?? e?.message ?? 'An error occurred.'
      setError('root', { message: detail })
    }
  }

  if (isEdit && loadingExisting) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
      </div>
    )
  }

  return (
    <div className="max-w-xl mx-auto">
      <button
        onClick={() => navigate('/admin')}
        className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-700 mb-6 transition-colors"
      >
        <ArrowLeft className="w-4 h-4" />
        Back to Dashboard
      </button>

      <div className="bg-white rounded-2xl shadow-sm border border-honey-100 px-8 py-8">
        <h1 className="text-xl font-bold text-gray-900 mb-6">
          {isEdit ? 'Edit Organization' : 'New Organization'}
        </h1>

        {errors.root && (
          <div className="mb-5 bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3 text-sm">
            {errors.root.message}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              Name <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              placeholder="Organization name"
              className={`w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all
                bg-gray-50 focus:bg-white
                ${errors.name
                  ? 'border-red-400 focus:ring-2 focus:ring-red-200'
                  : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'
                }`}
              {...register('name', { required: 'Name is required', maxLength: { value: 200, message: 'Max 200 characters' } })}
            />
            {errors.name && <p className="mt-1.5 text-xs text-red-600">{errors.name.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Description</label>
            <textarea
              rows={3}
              placeholder="Brief description (optional)"
              className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm outline-none
                bg-gray-50 focus:bg-white focus:border-honey-400 focus:ring-2 focus:ring-honey-100
                transition-all resize-none"
              {...register('description', { maxLength: { value: 1000, message: 'Max 1000 characters' } })}
            />
            {errors.description && <p className="mt-1.5 text-xs text-red-600">{errors.description.message}</p>}
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={() => navigate('/admin')}
              className="flex-1 px-4 py-3 rounded-xl border border-gray-200 text-sm font-medium text-gray-700
                hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl
                bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold
                disabled:opacity-60 disabled:cursor-not-allowed transition-colors"
            >
              {isSubmitting ? <Loader2 className="w-4 h-4 animate-spin" /> : null}
              {isEdit ? 'Save Changes' : 'Create Organization'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
