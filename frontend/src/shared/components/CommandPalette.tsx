import { useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Search, CornerDownLeft, Home, LayoutDashboard, Users,
  ReceiptText, CalendarDays, BarChart2, Settings,
} from 'lucide-react'
import { useApiaries } from '../../core/services/queries'
import { usePermissions } from '../../core/hooks/usePermissions'

interface CommandItem {
  id: string
  label: string
  hint?: string
  icon: React.ReactNode
  group: string
  run: () => void
}

export function CommandPalette({ open, onClose }: { open: boolean; onClose: () => void }) {
  const navigate = useNavigate()
  const { isSystemAdmin, isOrgAdmin, isAdmin } = usePermissions()
  const { data: apiaries = [] } = useApiaries()
  const [query, setQuery] = useState('')
  const [active, setActive] = useState(0)
  const inputRef = useRef<HTMLInputElement>(null)

  const canSeeMembers = isOrgAdmin || isAdmin
  const canSeeExpenses = isSystemAdmin || isOrgAdmin || isAdmin

  const items = useMemo<CommandItem[]>(() => {
    const go = (to: string) => () => { onClose(); navigate(to) }
    const nav: CommandItem[] = []
    if (isSystemAdmin) nav.push({ id: 'admin', label: 'Dashboard', icon: <LayoutDashboard className="w-4 h-4" />, group: 'Navigate', run: go('/admin') })
    else nav.push({ id: 'apiaries', label: 'Apiaries', icon: <Home className="w-4 h-4" />, group: 'Navigate', run: go('/apiaries') })
    if (canSeeMembers) nav.push({ id: 'members', label: 'Members', icon: <Users className="w-4 h-4" />, group: 'Navigate', run: go('/members') })
    if (canSeeExpenses) nav.push({ id: 'expenses', label: 'Expenses', icon: <ReceiptText className="w-4 h-4" />, group: 'Navigate', run: go('/expenses') })
    nav.push({ id: 'calendar', label: 'Calendar', icon: <CalendarDays className="w-4 h-4" />, group: 'Navigate', run: go('/calendar') })
    nav.push({ id: 'stats', label: 'Statistics', icon: <BarChart2 className="w-4 h-4" />, group: 'Navigate', run: go('/stats') })
    nav.push({ id: 'profile', label: 'Edit profile', icon: <Settings className="w-4 h-4" />, group: 'Navigate', run: go('/profile') })

    const ap: CommandItem[] = apiaries.map(a => ({
      id: `ap-${a.id}`,
      label: a.name,
      hint: `${a.beehiveCount} hive${a.beehiveCount === 1 ? '' : 's'}`,
      icon: <span className="text-base leading-none">🏡</span>,
      group: 'Apiaries',
      run: go(`/apiaries/${a.id}`),
    }))
    return [...nav, ...ap]
  }, [apiaries, isSystemAdmin, canSeeMembers, canSeeExpenses, navigate, onClose])

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase()
    if (!q) return items
    return items.filter(i => i.label.toLowerCase().includes(q) || i.group.toLowerCase().includes(q))
  }, [items, query])

  useEffect(() => { setActive(0) }, [query, open])
  useEffect(() => {
    if (open) {
      setQuery('')
      const t = window.setTimeout(() => inputRef.current?.focus(), 20)
      return () => window.clearTimeout(t)
    }
  }, [open])

  useEffect(() => {
    if (!open) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') { e.preventDefault(); onClose() }
      else if (e.key === 'ArrowDown') { e.preventDefault(); setActive(a => Math.min(a + 1, filtered.length - 1)) }
      else if (e.key === 'ArrowUp') { e.preventDefault(); setActive(a => Math.max(a - 1, 0)) }
      else if (e.key === 'Enter') { e.preventDefault(); filtered[active]?.run() }
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [open, filtered, active, onClose])

  if (!open) return null

  let lastGroup = ''
  return (
    <div
      className="fixed inset-0 z-[90] flex items-start justify-center p-4 pt-[12vh] bg-black/40 backdrop-blur-sm"
      onClick={onClose}
    >
      <div
        className="w-full max-w-lg bg-white dark:bg-slate-900 rounded-2xl shadow-2xl border border-honey-100 dark:border-slate-800 overflow-hidden animate-slide-up"
        onClick={e => e.stopPropagation()}
      >
        {/* Search input */}
        <div className="flex items-center gap-2 px-4 border-b border-honey-100 dark:border-slate-800">
          <Search className="w-4 h-4 text-gray-400 dark:text-slate-500 shrink-0" />
          <input
            ref={inputRef}
            value={query}
            onChange={e => setQuery(e.target.value)}
            placeholder="Search pages and apiaries…"
            className="flex-1 py-3.5 bg-transparent outline-none text-sm text-gray-800 dark:text-slate-100 placeholder:text-gray-400 dark:placeholder:text-slate-500"
          />
          <kbd className="text-[10px] font-mono text-gray-400 dark:text-slate-500 border border-gray-200 dark:border-slate-700 rounded px-1.5 py-0.5">ESC</kbd>
        </div>

        {/* Results */}
        <div className="max-h-80 overflow-y-auto py-2">
          {filtered.length === 0 ? (
            <p className="px-4 py-8 text-center text-sm text-gray-400 dark:text-slate-500">No results.</p>
          ) : (
            filtered.map((item, i) => {
              const header = item.group !== lastGroup ? item.group : null
              lastGroup = item.group
              return (
                <div key={item.id}>
                  {header && (
                    <p className="px-4 pt-2 pb-1 text-[11px] font-semibold uppercase tracking-wide text-gray-400 dark:text-slate-500">
                      {header}
                    </p>
                  )}
                  <button
                    onClick={item.run}
                    onMouseEnter={() => setActive(i)}
                    className={`w-full flex items-center gap-3 px-4 py-2.5 text-left transition-colors ${i === active ? 'bg-honey-50 dark:bg-slate-800' : ''}`}
                  >
                    <span className="text-honey-600 dark:text-honey-400 shrink-0">{item.icon}</span>
                    <span className="flex-1 text-sm text-gray-800 dark:text-slate-100 truncate">{item.label}</span>
                    {item.hint && <span className="text-xs text-gray-400 dark:text-slate-500 shrink-0">{item.hint}</span>}
                    {i === active && <CornerDownLeft className="w-3.5 h-3.5 text-gray-300 dark:text-slate-600 shrink-0" />}
                  </button>
                </div>
              )
            })
          )}
        </div>
      </div>
    </div>
  )
}
