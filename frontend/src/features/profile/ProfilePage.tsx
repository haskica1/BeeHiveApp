import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useMutation } from '@tanstack/react-query'
import { Check, Eye, EyeOff, KeyRound, Mail, User } from 'lucide-react'
import { useAuth } from '../../core/context/AuthContext'
import { profileService } from '../../core/services/profileService'
import type { UpdateProfilePayload } from '../../core/services/profileService'
import { PageHeader } from '../../shared/components'
import clsx from 'clsx'

interface ProfileForm {
  firstName: string
  lastName: string
  email: string
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export default function ProfilePage() {
  const { user, updateUser } = useAuth()
  const [showCurrent, setShowCurrent] = useState(false)
  const [showNew, setShowNew] = useState(false)
  const [showConfirm, setShowConfirm] = useState(false)
  const [saved, setSaved] = useState(false)

  const {
    register,
    handleSubmit,
    watch,
    setError,
    reset,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<ProfileForm>({
    defaultValues: {
      firstName: user?.firstName ?? '',
      lastName: user?.lastName ?? '',
      email: user?.email ?? '',
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  })

  const newPassword = watch('newPassword')

  const mutation = useMutation({
    mutationFn: (payload: UpdateProfilePayload) => profileService.update(payload),
    onSuccess: (data) => {
      updateUser({ firstName: data.firstName, lastName: data.lastName, email: data.email })
      reset({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
      })
      setSaved(true)
      setTimeout(() => setSaved(false), 3000)
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      const msg = err?.response?.data?.message ?? 'Something went wrong.'
      if (msg.toLowerCase().includes('password')) {
        setError('currentPassword', { message: msg })
      } else if (msg.toLowerCase().includes('email')) {
        setError('email', { message: msg })
      } else {
        setError('root', { message: msg })
      }
    },
  })

  async function onSubmit(data: ProfileForm) {
    if (data.newPassword && data.newPassword !== data.confirmPassword) {
      setError('confirmPassword', { message: 'Passwords do not match.' })
      return
    }

    const payload: UpdateProfilePayload = {
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
    }

    if (data.newPassword) {
      payload.currentPassword = data.currentPassword
      payload.newPassword = data.newPassword
    }

    await mutation.mutateAsync(payload)
  }

  const avatarClass = user?.role === 'SystemAdmin'
    ? 'bg-purple-100 text-purple-700'
    : user?.role === 'OrgAdmin'
    ? 'bg-blue-100 text-blue-700'
    : 'bg-honey-100 text-honey-700'

  const roleLabel = user?.role === 'SystemAdmin'
    ? 'System Admin'
    : user?.role === 'OrgAdmin'
    ? `Org Admin · ${user?.organizationName}`
    : user?.role === 'Admin'
    ? `Admin · ${user?.organizationName}`
    : user?.organizationName ?? ''

  return (
    <div className="animate-fade-in max-w-lg mx-auto">
      <PageHeader title="Edit Profile" subtitle="Update your personal information and password" />

      {/* Avatar display */}
      <div className="flex items-center gap-4 mb-8 p-4 card">
        <div className={clsx('w-16 h-16 rounded-full flex items-center justify-center font-bold text-2xl shrink-0', avatarClass)}>
          {user?.firstName[0] ?? '?'}
        </div>
        <div>
          <p className="font-semibold text-gray-800 text-lg">{user?.firstName} {user?.lastName}</p>
          <p className="text-sm text-gray-500">{roleLabel}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">

        {/* Personal info */}
        <div className="card space-y-4">
          <div className="flex items-center gap-2 mb-1">
            <User className="w-4 h-4 text-honey-500" />
            <h3 className="font-semibold text-gray-700">Personal Information</h3>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">First Name</label>
              <input
                {...register('firstName', { required: 'First name is required' })}
                className={clsx('form-input', errors.firstName && 'border-red-400 focus:ring-red-300')}
                placeholder="First name"
              />
              {errors.firstName && <p className="text-xs text-red-500 mt-1">{errors.firstName.message}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
              <input
                {...register('lastName', { required: 'Last name is required' })}
                className={clsx('form-input', errors.lastName && 'border-red-400 focus:ring-red-300')}
                placeholder="Last name"
              />
              {errors.lastName && <p className="text-xs text-red-500 mt-1">{errors.lastName.message}</p>}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                {...register('email', {
                  required: 'Email is required',
                  pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Invalid email address' },
                })}
                type="email"
                className={clsx('form-input pl-9', errors.email && 'border-red-400 focus:ring-red-300')}
                placeholder="you@example.com"
              />
            </div>
            {errors.email && <p className="text-xs text-red-500 mt-1">{errors.email.message}</p>}
          </div>
        </div>

        {/* Password change */}
        <div className="card space-y-4">
          <div className="flex items-center gap-2 mb-1">
            <KeyRound className="w-4 h-4 text-honey-500" />
            <h3 className="font-semibold text-gray-700">Change Password</h3>
            <span className="text-xs text-gray-400 font-normal">(optional)</span>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Current Password</label>
            <div className="relative">
              <input
                {...register('currentPassword')}
                type={showCurrent ? 'text' : 'password'}
                className={clsx('form-input pr-10', errors.currentPassword && 'border-red-400 focus:ring-red-300')}
                placeholder="Enter current password"
                autoComplete="current-password"
              />
              <button type="button" onClick={() => setShowCurrent(v => !v)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                {showCurrent ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
            {errors.currentPassword && <p className="text-xs text-red-500 mt-1">{errors.currentPassword.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">New Password</label>
            <div className="relative">
              <input
                {...register('newPassword', {
                  minLength: newPassword ? { value: 6, message: 'Minimum 6 characters' } : undefined,
                })}
                type={showNew ? 'text' : 'password'}
                className={clsx('form-input pr-10', errors.newPassword && 'border-red-400 focus:ring-red-300')}
                placeholder="Enter new password"
                autoComplete="new-password"
              />
              <button type="button" onClick={() => setShowNew(v => !v)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                {showNew ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
            {errors.newPassword && <p className="text-xs text-red-500 mt-1">{errors.newPassword.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Confirm New Password</label>
            <div className="relative">
              <input
                {...register('confirmPassword')}
                type={showConfirm ? 'text' : 'password'}
                className={clsx('form-input pr-10', errors.confirmPassword && 'border-red-400 focus:ring-red-300')}
                placeholder="Repeat new password"
                autoComplete="new-password"
              />
              <button type="button" onClick={() => setShowConfirm(v => !v)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                {showConfirm ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
            {errors.confirmPassword && <p className="text-xs text-red-500 mt-1">{errors.confirmPassword.message}</p>}
          </div>
        </div>

        {/* Root error */}
        {errors.root && (
          <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-xl px-4 py-2.5">
            {errors.root.message}
          </p>
        )}

        {/* Submit */}
        <div className="flex items-center justify-end gap-3">
          {saved && (
            <span className="flex items-center gap-1.5 text-sm text-green-600 font-medium animate-fade-in">
              <Check className="w-4 h-4" /> Saved successfully
            </span>
          )}
          <button
            type="submit"
            disabled={isSubmitting || !isDirty}
            className="btn-primary"
          >
            {isSubmitting ? 'Saving…' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  )
}
