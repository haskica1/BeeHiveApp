import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2 } from 'lucide-react'
import {
  useAdminUser,
  useCreateAdminUser,
  useUpdateAdminUser,
} from '../../core/services/adminQueries'
import { useAdminOrganizations } from '../../core/services/adminQueries'

interface UserForm {
  firstName: string
  lastName: string
  email: string
  password: string
  role: string
  organizationId: string
}

export default function UserFormPage() {
  const { id } = useParams<{ id: string }>()
  const isEdit = !!id
  const userId = id ? parseInt(id) : 0
  const navigate = useNavigate()

  const { data: existing, isLoading: loadingExisting } = useAdminUser(userId)
  const { data: organizations = [] } = useAdminOrganizations()
  const createUser = useCreateAdminUser()
  const updateUser = useUpdateAdminUser(userId)

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<UserForm>({ defaultValues: { role: 'Admin' } })

  const selectedRole = watch('role')

  useEffect(() => {
    if (existing) {
      reset({
        firstName: existing.firstName,
        lastName: existing.lastName,
        email: existing.email,
        password: '',
        role: existing.role,
        organizationId: existing.organizationId?.toString() ?? '',
      })
    }
  }, [existing, reset])

  async function onSubmit(data: UserForm) {
    const orgId = data.organizationId ? parseInt(data.organizationId) : null

    try {
      if (isEdit) {
        await updateUser.mutateAsync({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          role: data.role,
          organizationId: orgId,
        })
      } else {
        await createUser.mutateAsync({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          password: data.password,
          role: data.role,
          organizationId: orgId,
        })
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
          {isEdit ? 'Edit User' : 'New User'}
        </h1>

        {errors.root && (
          <div className="mb-5 bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3 text-sm">
            {errors.root.message}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                First Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                placeholder="First name"
                className={`w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all bg-gray-50 focus:bg-white
                  ${errors.firstName ? 'border-red-400 focus:ring-2 focus:ring-red-200' : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'}`}
                {...register('firstName', { required: 'Required' })}
              />
              {errors.firstName && <p className="mt-1 text-xs text-red-600">{errors.firstName.message}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Last Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                placeholder="Last name"
                className={`w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all bg-gray-50 focus:bg-white
                  ${errors.lastName ? 'border-red-400 focus:ring-2 focus:ring-red-200' : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'}`}
                {...register('lastName', { required: 'Required' })}
              />
              {errors.lastName && <p className="mt-1 text-xs text-red-600">{errors.lastName.message}</p>}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              Email <span className="text-red-500">*</span>
            </label>
            <input
              type="email"
              placeholder="user@example.com"
              className={`w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all bg-gray-50 focus:bg-white
                ${errors.email ? 'border-red-400 focus:ring-2 focus:ring-red-200' : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'}`}
              {...register('email', {
                required: 'Email is required',
                pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Invalid email' },
              })}
            />
            {errors.email && <p className="mt-1.5 text-xs text-red-600">{errors.email.message}</p>}
          </div>

          {!isEdit && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Password <span className="text-red-500">*</span>
              </label>
              <input
                type="password"
                placeholder="••••••••"
                className={`w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all bg-gray-50 focus:bg-white
                  ${errors.password ? 'border-red-400 focus:ring-2 focus:ring-red-200' : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'}`}
                {...register('password', {
                  required: isEdit ? false : 'Password is required',
                  minLength: { value: 6, message: 'Minimum 6 characters' },
                })}
              />
              {errors.password && <p className="mt-1.5 text-xs text-red-600">{errors.password.message}</p>}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              Role <span className="text-red-500">*</span>
            </label>
            <select
              className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm outline-none bg-gray-50 focus:bg-white focus:border-honey-400 focus:ring-2 focus:ring-honey-100 transition-all"
              {...register('role', { required: 'Role is required' })}
            >
              <option value="Admin">Admin</option>
              <option value="SystemAdmin">System Admin</option>
            </select>
          </div>

          {selectedRole !== 'SystemAdmin' && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Organization <span className="text-red-500">*</span>
              </label>
              <select
                className={`w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all bg-gray-50 focus:bg-white
                  ${errors.organizationId ? 'border-red-400 focus:ring-2 focus:ring-red-200' : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'}`}
                {...register('organizationId', {
                  validate: (v) =>
                    selectedRole === 'SystemAdmin' || !!v || 'Organization is required for non-SystemAdmin users',
                })}
              >
                <option value="">Select organization…</option>
                {organizations.map((org) => (
                  <option key={org.id} value={org.id}>{org.name}</option>
                ))}
              </select>
              {errors.organizationId && <p className="mt-1.5 text-xs text-red-600">{errors.organizationId.message}</p>}
            </div>
          )}

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
              {isEdit ? 'Save Changes' : 'Create User'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
