import { AlertTriangle, Loader2, PackageOpen, X } from 'lucide-react'
import { useEffect } from 'react'

// ── LoadingSpinner ─────────────────────────────────────────────────────────────

export function LoadingSpinner({ message = 'Loading…' }: { message?: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 gap-3 animate-fade-in">
      <Loader2 className="w-8 h-8 text-honey-500 animate-spin" />
      <p className="text-sm text-gray-500">{message}</p>
    </div>
  )
}

// ── ErrorMessage ──────────────────────────────────────────────────────────────

export function ErrorMessage({ message }: { message: string }) {
  return (
    <div className="flex items-center gap-3 p-4 bg-red-50 border border-red-200 rounded-xl animate-fade-in">
      <AlertTriangle className="w-5 h-5 text-red-500 shrink-0" />
      <p className="text-sm text-red-700">{message}</p>
    </div>
  )
}

// ── EmptyState ────────────────────────────────────────────────────────────────

interface EmptyStateProps {
  title: string
  description?: string
  action?: React.ReactNode
}

export function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 gap-4 text-center animate-fade-in">
      <div className="w-16 h-16 bg-honey-100 rounded-full flex items-center justify-center">
        <PackageOpen className="w-8 h-8 text-honey-400" />
      </div>
      <div>
        <p className="font-display text-lg font-semibold text-gray-700">{title}</p>
        {description && <p className="mt-1 text-sm text-gray-500">{description}</p>}
      </div>
      {action && <div className="mt-2">{action}</div>}
    </div>
  )
}

// ── ConfirmDialog ─────────────────────────────────────────────────────────────

interface ConfirmDialogProps {
  isOpen: boolean
  title: string
  message: string
  confirmLabel?: string
  onConfirm: () => void
  onCancel: () => void
  isLoading?: boolean
}

export function ConfirmDialog({
  isOpen,
  title,
  message,
  confirmLabel = 'Delete',
  onConfirm,
  onCancel,
  isLoading,
}: ConfirmDialogProps) {
  // Close on Escape key
  useEffect(() => {
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') onCancel() }
    document.addEventListener('keydown', handler)
    return () => document.removeEventListener('keydown', handler)
  }, [onCancel])

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/40 backdrop-blur-sm"
        onClick={onCancel}
      />
      {/* Dialog */}
      <div className="relative bg-white rounded-2xl shadow-2xl p-6 max-w-sm w-full animate-slide-up">
        <button
          onClick={onCancel}
          className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 transition-colors"
        >
          <X className="w-5 h-5" />
        </button>

        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center shrink-0">
            <AlertTriangle className="w-5 h-5 text-red-500" />
          </div>
          <h3 className="font-display text-lg font-semibold text-gray-800">{title}</h3>
        </div>

        <p className="text-sm text-gray-600 mb-6">{message}</p>

        <div className="flex gap-3 justify-end">
          <button onClick={onCancel} className="btn-secondary text-sm">
            Cancel
          </button>
          <button
            onClick={onConfirm}
            className="btn-danger text-sm"
            disabled={isLoading}
          >
            {isLoading ? <Loader2 className="w-4 h-4 animate-spin" /> : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}

// ── PageHeader ────────────────────────────────────────────────────────────────

interface PageHeaderProps {
  title: string
  subtitle?: string
  actions?: React.ReactNode
  backButton?: React.ReactNode
}

export function PageHeader({ title, subtitle, actions, backButton }: PageHeaderProps) {
  return (
    <div className="flex flex-col gap-1 mb-6 sm:flex-row sm:items-center sm:justify-between">
      <div>
        {backButton && <div className="mb-2">{backButton}</div>}
        <h1 className="font-display text-2xl sm:text-3xl font-bold text-gray-900">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-gray-500">{subtitle}</p>}
      </div>
      {actions && <div className="flex gap-2 mt-3 sm:mt-0">{actions}</div>}
    </div>
  )
}

// ── HoneyLevelBadge ───────────────────────────────────────────────────────────

import { HoneyLevel } from '../../core/models'

const honeyLevelConfig: Record<HoneyLevel, { label: string; className: string }> = {
  [HoneyLevel.Low]:    { label: 'Low',    className: 'bg-red-100 text-red-700' },
  [HoneyLevel.Medium]: { label: 'Medium', className: 'bg-yellow-100 text-yellow-700' },
  [HoneyLevel.High]:   { label: 'High',   className: 'bg-green-100 text-green-700' },
}

export function HoneyLevelBadge({ level }: { level: HoneyLevel }) {
  const config = honeyLevelConfig[level] ?? { label: 'Unknown', className: 'bg-gray-100 text-gray-600' }
  return (
    <span className={`badge ${config.className}`}>
      🍯 {config.label}
    </span>
  )
}
