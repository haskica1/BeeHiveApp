import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { Loader2 } from 'lucide-react'
import {
  useAdminOrganization,
  useCreateOrganization,
  useUpdateOrganization,
} from '../../core/services/adminQueries'
import { FormHeader } from '../../shared/components'

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
      <FormHeader
        icon="🏢"
        title={isEdit ? 'Uredi organizaciju' : 'Nova organizacija'}
        onBack={() => navigate('/admin')}
        backLabel="Nazad na kontrolnu ploču"
      />

      <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-sm dark:shadow-none border border-honey-100 dark:border-slate-800 px-8 py-8">
        {errors.root && (
          <div className="mb-5 bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/30 text-red-700 dark:text-red-300 rounded-xl px-4 py-3 text-sm">
            {errors.root.message}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1.5">
              Naziv <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              placeholder="Naziv organizacije"
              className={`w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all
                bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100
                ${errors.name
                  ? 'border-red-400 focus:ring-2 focus:ring-red-200'
                  : 'border-gray-200 dark:border-slate-700 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'
                }`}
              {...register('name', { required: 'Naziv je obavezan', maxLength: { value: 200, message: 'Maks 200 znakova' } })}
            />
            {errors.name && <p className="mt-1.5 text-xs text-red-600">{errors.name.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1.5">Opis</label>
            <textarea
              rows={3}
              placeholder="Kratki opis (opcionalno)"
              className="w-full px-4 py-3 rounded-xl border border-gray-200 dark:border-slate-700 text-sm outline-none
                bg-gray-50 focus:bg-white dark:bg-slate-800 dark:focus:bg-slate-800 dark:text-slate-100 focus:border-honey-400 focus:ring-2 focus:ring-honey-100
                transition-all resize-none"
              {...register('description', { maxLength: { value: 1000, message: 'Maks 1000 znakova' } })}
            />
            {errors.description && <p className="mt-1.5 text-xs text-red-600">{errors.description.message}</p>}
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={() => navigate('/admin')}
              className="flex-1 px-4 py-3 rounded-xl border border-gray-200 dark:border-slate-700 text-sm font-medium text-gray-700 dark:text-slate-200
                hover:bg-gray-50 dark:hover:bg-slate-800 transition-colors"
            >
              Otkaži
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl
                bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold
                disabled:opacity-60 disabled:cursor-not-allowed transition-colors"
            >
              {isSubmitting ? <Loader2 className="w-4 h-4 animate-spin" /> : null}
              {isEdit ? 'Spremi promjene' : 'Napravi organizaciju'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
