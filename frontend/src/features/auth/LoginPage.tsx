import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { Eye, EyeOff, Loader2 } from 'lucide-react'
import { useAuth } from '../../core/context/AuthContext'
import type { LoginResponse } from '../../core/services/authService'

interface LoginForm {
  email: string
  password: string
}

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [showPassword, setShowPassword] = useState(false)
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginForm>()

  async function onSubmit(data: LoginForm) {
    setServerError(null)
    try {
      const response = await login(data.email, data.password) as LoginResponse
      navigate(response.role === 'SystemAdmin' ? '/admin' : '/apiaries', { replace: true })
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Login failed. Please try again.')
    }
  }

  return (
    <div className="min-h-screen flex">
      {/* ── Left panel — decorative ─────────────────────────────────────────── */}
      <div className="hidden lg:flex lg:w-1/2 relative overflow-hidden bg-honey-600 flex-col items-center justify-center">
        {/* Honeycomb SVG background pattern */}
        <svg
          className="absolute inset-0 w-full h-full opacity-10"
          xmlns="http://www.w3.org/2000/svg"
        >
          <defs>
            <pattern id="honeycomb" x="0" y="0" width="56" height="100" patternUnits="userSpaceOnUse">
              <path
                d="M28 66 L0 50 L0 16 L28 0 L56 16 L56 50 Z"
                fill="none"
                stroke="white"
                strokeWidth="1.5"
              />
              <path
                d="M28 166 L0 150 L0 116 L28 100 L56 116 L56 150 Z"
                fill="none"
                stroke="white"
                strokeWidth="1.5"
              />
              <path
                d="M56 116 L84 100 L84 66 L56 50 L28 66 L28 100 Z"
                fill="none"
                stroke="white"
                strokeWidth="1.5"
              />
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#honeycomb)" />
        </svg>

        {/* Radial glow overlay */}
        <div className="absolute inset-0 bg-gradient-to-br from-honey-500/60 via-honey-600 to-amber-800/80" />

        <div className="relative z-10 text-center px-12">
          <div className="text-8xl mb-6 drop-shadow-lg">🐝</div>
          <h1 className="text-5xl font-bold text-white mb-4 tracking-tight">
            BeeHive
          </h1>
          <p className="text-honey-100 text-xl leading-relaxed max-w-sm">
            Manage your apiaries, track your colonies, and keep your bees thriving.
          </p>

          <div className="mt-10 grid grid-cols-3 gap-6 text-center">
            {[
              { icon: '🏡', label: 'Apiaries' },
              { icon: '🍯', label: 'Inspections' },
              { icon: '📋', label: 'Feeding Plans' },
            ].map((item) => (
              <div key={item.label} className="bg-white/15 backdrop-blur rounded-xl p-4">
                <div className="text-3xl mb-1">{item.icon}</div>
                <div className="text-white text-sm font-medium">{item.label}</div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* ── Right panel — form ──────────────────────────────────────────────── */}
      <div className="flex-1 flex items-center justify-center bg-honey-50 px-6 py-12">
        <div className="w-full max-w-md animate-fade-in">
          {/* Mobile logo */}
          <div className="lg:hidden text-center mb-8">
            <span className="text-5xl">🐝</span>
            <h1 className="text-3xl font-bold text-honey-800 mt-2">BeeHive</h1>
          </div>

          <div className="bg-white rounded-2xl shadow-xl border border-honey-100 px-8 py-10">
            <div className="mb-8">
              <h2 className="text-2xl font-bold text-gray-900">Welcome back</h2>
              <p className="text-gray-500 mt-1 text-sm">Sign in to your beekeeping workspace</p>
            </div>

            {/* Server error */}
            {serverError && (
              <div className="mb-6 flex items-start gap-3 bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3 text-sm">
                <span className="mt-0.5">⚠️</span>
                <span>{serverError}</span>
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
              {/* Email */}
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1.5">
                  Email address
                </label>
                <input
                  id="email"
                  type="email"
                  autoComplete="email"
                  placeholder="you@example.com"
                  className={`w-full px-4 py-3 rounded-xl border text-sm transition-all duration-200 outline-none
                    bg-gray-50 focus:bg-white
                    ${errors.email
                      ? 'border-red-400 focus:ring-2 focus:ring-red-200'
                      : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'
                    }`}
                  {...register('email', {
                    required: 'Email is required',
                    pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Enter a valid email' },
                  })}
                />
                {errors.email && (
                  <p className="mt-1.5 text-xs text-red-600">{errors.email.message}</p>
                )}
              </div>

              {/* Password */}
              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1.5">
                  Password
                </label>
                <div className="relative">
                  <input
                    id="password"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="current-password"
                    placeholder="••••••••"
                    className={`w-full px-4 py-3 pr-11 rounded-xl border text-sm transition-all duration-200 outline-none
                      bg-gray-50 focus:bg-white
                      ${errors.password
                        ? 'border-red-400 focus:ring-2 focus:ring-red-200'
                        : 'border-gray-200 focus:border-honey-400 focus:ring-2 focus:ring-honey-100'
                      }`}
                    {...register('password', { required: 'Password is required' })}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((v) => !v)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
                    tabIndex={-1}
                  >
                    {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                </div>
                {errors.password && (
                  <p className="mt-1.5 text-xs text-red-600">{errors.password.message}</p>
                )}
              </div>

              {/* Submit */}
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full flex items-center justify-center gap-2 bg-honey-500 hover:bg-honey-600 active:bg-honey-700
                  disabled:opacity-60 disabled:cursor-not-allowed
                  text-white font-semibold py-3 px-6 rounded-xl
                  transition-all duration-200 shadow-sm hover:shadow-md
                  focus:outline-none focus:ring-2 focus:ring-honey-400 focus:ring-offset-2
                  mt-2"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    Signing in…
                  </>
                ) : (
                  'Sign in'
                )}
              </button>
            </form>

            {/* Test credentials hint */}
            <div className="mt-8 pt-6 border-t border-gray-100">
              <p className="text-xs text-gray-400 text-center mb-3">Test accounts</p>
              <div className="grid grid-cols-1 gap-2">
                <div className="bg-purple-50 rounded-lg p-2.5 text-center border border-purple-100">
                  <p className="text-xs font-medium text-purple-700">System Admin</p>
                  <p className="text-xs text-gray-500 mt-0.5">sysadmin@beehive.com</p>
                  <p className="text-xs text-gray-400 font-mono mt-0.5">SysAdmin123!</p>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  {[
                    { org: 'Golden Hive Co', email: 'admin@goldenhive.com' },
                    { org: 'Mountain Bees', email: 'admin@mountainbees.com' },
                  ].map((acc) => (
                    <div key={acc.email} className="bg-honey-50 rounded-lg p-2.5 text-center">
                      <p className="text-xs font-medium text-honey-700">{acc.org}</p>
                      <p className="text-xs text-gray-500 mt-0.5 truncate">{acc.email}</p>
                    </div>
                  ))}
                </div>
              </div>
              <p className="text-xs text-gray-400 text-center mt-2">
                Org admin password: <span className="font-mono">Admin123!</span>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
