import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Loader2 } from 'lucide-react'
import {
  useAdminUser,
  useCreateAdminUser,
  useUpdateAdminUser,
  useAdminOrganizations,
  useApiariesByOrganization,
  useBeehivesByOrganization,
} from '../../core/services/adminQueries'

interface UserForm {
  firstName: string
  lastName: string
  email: string
  password: string
  role: string
  organizationId: string
  apiaryId: string
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

  const [selectedBeehiveIds, setSelectedBeehiveIds] = useState<number[]>([])

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<UserForm>({ defaultValues: { role: 'OrgAdmin' } })

  const selectedRole = watch('role')
  const selectedOrgId = watch('organizationId')
  const orgIdNumber = selectedOrgId ? parseInt(selectedOrgId) : 0

  const { data: apiaries = [] } = useApiariesByOrganization(orgIdNumber)
  const { data: beehives = [] } = useBeehivesByOrganization(orgIdNumber)

  const needsOrg    = selectedRole !== 'SystemAdmin'
  const needsApiary = selectedRole === 'Admin'
  const needsHives  = selectedRole === 'User'

  useEffect(() => {
    if (existing) {
      reset({
        firstName: existing.firstName,
        lastName: existing.lastName,
        email: existing.email,
        password: '',
        role: existing.role,
        organizationId: existing.organizationId?.toString() ?? '',
        apiaryId: existing.apiaryId?.toString() ?? '',
      })
      setSelectedBeehiveIds(existing.assignedBeehiveIds ?? [])
    }
  }, [existing, reset])

  function toggleBeehive(beehiveId: number) {
    setSelectedBeehiveIds(prev =>
      prev.includes(beehiveId)
        ? prev.filter(id => id !== beehiveId)
        : [...prev, beehiveId]
    )
  }

  async function onSubmit(data: UserForm) {
    const orgId    = data.organizationId ? parseInt(data.organizationId) : null
    const apiaryId = data.apiaryId ? parseInt(data.apiaryId) : null

    try {
      if (isEdit) {
        await updateUser.mutateAsync({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          role: data.role,
          organizationId: orgId,
          apiaryId: needsApiary ? apiaryId : null,
          assignedBeehiveIds: needsHives ? selectedBeehiveIds : [],
        })
      } else {
        await createUser.mutateAsync({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          password: data.password,
          role: data.role,
          organizationId: orgId,
          apiaryId: needsApiary ? apiaryId : null,
          assignedBeehiveIds: needsHives ? selectedBeehiveIds : [],
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

  const inputCls = (hasError: boolean) =>
    `w-full px-4 py-3 rounded-xl border text-sm outline-none transition-all bg-gray-50 focus:bg-white ${
      hasError
        ? 'border-red-400 focus:ring-2 focus:ring-red-200'
        : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'
    }`

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
                className={inputCls(!!errors.firstName)}
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
                className={inputCls(!!errors.lastName)}
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
              className={inputCls(!!errors.email)}
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
                className={inputCls(!!errors.password)}
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
              <option value="OrgAdmin">Org Admin</option>
              <option value="Admin">Admin</option>
              <option value="User">User</option>
              <option value="SystemAdmin">System Admin</option>
            </select>
            <p className="mt-1 text-xs text-gray-400">
              {selectedRole === 'OrgAdmin' && 'Can manage all apiaries, hives, diets, inspections, and todos within the org.'}
              {selectedRole === 'Admin' && 'Scoped to one apiary — can manage hives, diets, inspections, and hive todos.'}
              {selectedRole === 'User' && 'Can create inspections, manage todos on assigned hives, and view diets.'}
              {selectedRole === 'SystemAdmin' && 'Full platform access — no org required.'}
            </p>
          </div>

          {needsOrg && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Organization <span className="text-red-500">*</span>
              </label>
              <select
                className={inputCls(!!errors.organizationId)}
                {...register('organizationId', {
                  validate: (v) => !needsOrg || !!v || 'Organization is required for this role',
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

          {needsApiary && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Apiary <span className="text-red-500">*</span>
              </label>
              <select
                className={inputCls(!!errors.apiaryId)}
                {...register('apiaryId', {
                  validate: (v) => !needsApiary || !!v || 'Apiary is required for Admin users',
                })}
                disabled={!orgIdNumber}
              >
                <option value="">{orgIdNumber ? 'Select apiary…' : 'Select an organization first'}</option>
                {apiaries.map((a) => (
                  <option key={a.id} value={a.id}>{a.name}</option>
                ))}
              </select>
              {errors.apiaryId && <p className="mt-1.5 text-xs text-red-600">{errors.apiaryId.message}</p>}
            </div>
          )}

          {needsHives && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Assigned Hives
              </label>
              {!orgIdNumber ? (
                <p className="text-xs text-gray-400 py-2">Select an organization first to see available hives.</p>
              ) : beehives.length === 0 ? (
                <p className="text-xs text-gray-400 py-2">No hives found in this organization.</p>
              ) : (
                <div className="border border-gray-200 rounded-xl divide-y divide-gray-100 max-h-48 overflow-y-auto">
                  {beehives.map((b) => (
                    <label
                      key={b.id}
                      className="flex items-center gap-3 px-4 py-2.5 cursor-pointer hover:bg-gray-50 transition-colors"
                    >
                      <input
                        type="checkbox"
                        checked={selectedBeehiveIds.includes(b.id)}
                        onChange={() => toggleBeehive(b.id)}
                        className="w-4 h-4 accent-honey-500"
                      />
                      <span className="text-sm text-gray-800">{b.name}</span>
                      <span className="text-xs text-gray-400 ml-auto">{b.apiaryName}</span>
                    </label>
                  ))}
                </div>
              )}
              <p className="mt-1 text-xs text-gray-400">
                {selectedBeehiveIds.length > 0
                  ? `${selectedBeehiveIds.length} hive${selectedBeehiveIds.length !== 1 ? 's' : ''} selected`
                  : 'No hives assigned — user can still view but not manage todos.'}
              </p>
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
