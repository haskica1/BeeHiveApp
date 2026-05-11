import { useState } from 'react'
import { ChevronDown } from 'lucide-react'

interface Props {
  title: string
  icon: React.ReactNode
  count?: number
  action?: React.ReactNode
  defaultOpen?: boolean
  children: React.ReactNode
}

export function CollapsibleSection({ title, icon, count, action, defaultOpen = true, children }: Props) {
  const [open, setOpen] = useState(defaultOpen)

  return (
    <div className="mb-8 rounded-2xl border border-honey-100 shadow-card overflow-hidden">
      {/* ── Header ── */}
      <div
        className="flex items-center bg-white hover:bg-honey-50/60 transition-colors cursor-pointer select-none"
        onClick={() => setOpen(v => !v)}
      >
        {/* Left: icon + title + count */}
        <div className="flex items-center gap-2.5 flex-1 min-w-0 px-4 py-3.5">
          <span className="text-lg leading-none shrink-0">{icon}</span>
          <span className="font-display font-semibold text-gray-800 text-lg leading-tight truncate">
            {title}
          </span>
          {count != null && count > 0 && (
            <span className="badge bg-honey-100 text-honey-700 text-xs shrink-0">{count}</span>
          )}
        </div>

        {/* Right: action button + rotating chevron */}
        <div className="flex items-center gap-2 pr-4 shrink-0">
          {action && (
            <div onClick={e => e.stopPropagation()}>
              {action}
            </div>
          )}
          <ChevronDown
            className={`w-4 h-4 text-gray-400 transition-transform duration-300 shrink-0 ${open ? 'rotate-180' : ''}`}
          />
        </div>
      </div>

      {/* ── Body ── */}
      {open && (
        <div className="border-t border-honey-100 bg-white p-4">
          {children}
        </div>
      )}
    </div>
  )
}
