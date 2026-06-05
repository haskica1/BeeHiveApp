import { useEffect, useRef, useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { BarChart2, CalendarDays, Home, LayoutDashboard, LogOut, Menu, Moon, QrCode, ReceiptText, Search, Settings, Sun, Users, X } from 'lucide-react'
import clsx from 'clsx'
import { useAuth } from '../../core/context/AuthContext'
import { useTheme } from '../../core/hooks/useTheme'
import QrScannerModal from './QrScannerModal'
import NotificationBell from './NotificationBell'
import { CommandPalette } from './CommandPalette'

export default function Layout() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)
  const [scannerOpen, setScannerOpen] = useState(false)
  const [paletteOpen, setPaletteOpen] = useState(false)
  const profileRef = useRef<HTMLDivElement>(null)
  const { user, logout } = useAuth()
  const { isDark, toggleTheme } = useTheme()
  const navigate = useNavigate()

  const isSystemAdmin  = user?.role === 'SystemAdmin'
  const isOrgAdmin     = user?.role === 'OrgAdmin'
  const isAdmin        = user?.role === 'Admin'
  const canSeeExpenses = isSystemAdmin || isOrgAdmin || isAdmin
  const isMac = typeof navigator !== 'undefined' && /Mac|iPhone|iPad/.test(navigator.platform)

  const avatarClass = isSystemAdmin
    ? 'bg-purple-100 text-purple-700 dark:bg-purple-500/20 dark:text-purple-300'
    : isOrgAdmin
    ? 'bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300'
    : 'bg-honey-100 text-honey-700 dark:bg-honey-500/20 dark:text-honey-300'

  const roleLabel = isSystemAdmin
    ? 'System Admin'
    : isOrgAdmin
    ? `Org Admin · ${user?.organizationName}`
    : isAdmin
    ? `Admin · ${user?.organizationName}`
    : user?.organizationName ?? ''

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  // Close profile dropdown on outside click
  useEffect(() => {
    function onOutsideClick(e: MouseEvent) {
      if (profileRef.current && !profileRef.current.contains(e.target as Node)) {
        setProfileOpen(false)
      }
    }
    if (profileOpen) document.addEventListener('mousedown', onOutsideClick)
    return () => document.removeEventListener('mousedown', onOutsideClick)
  }, [profileOpen])

  // Open the command palette with Ctrl/Cmd+K
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault()
        setPaletteOpen(v => !v)
      }
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [])

  return (
    <div className="min-h-screen flex flex-col">

      {/* ── Header ──────────────────────────────────────────────────────────── */}
      <header className="sticky top-0 z-40 bg-white/90 dark:bg-slate-900/90 backdrop-blur border-b border-honey-200 dark:border-slate-800 shadow-sm dark:shadow-none">
        <div className="max-w-5xl mx-auto px-4 h-14 flex items-center justify-between gap-4">

          {/* Logo */}
          <Link
            to={isSystemAdmin ? '/admin' : '/apiaries'}
            className="flex items-center gap-2 group shrink-0"
          >
            <span className="text-2xl leading-none">🐝</span>
            <span className="font-display text-xl font-bold text-honey-800 dark:text-honey-300 group-hover:text-honey-600 dark:group-hover:text-honey-400 transition-colors">
              BeeHive
            </span>
          </Link>

          {/* ── Desktop right side ─────────────────────────────────────────── */}
          <div className="hidden sm:flex items-center gap-3">

            {/* Nav pill group */}
            <nav className="flex items-center gap-0.5 bg-gray-100 dark:bg-slate-800 rounded-xl p-1">
              {isSystemAdmin ? (
                <NavPill to="/admin" icon={<LayoutDashboard className="w-4 h-4" />} label="Dashboard" />
              ) : (
                <NavPill to="/apiaries" icon={<Home className="w-4 h-4" />} label="Apiaries" />
              )}
              {(isOrgAdmin || isAdmin) && (
                <NavPill to="/members" icon={<Users className="w-4 h-4" />} label="Members" />
              )}
              {canSeeExpenses && (
                <NavPill to="/expenses" icon={<ReceiptText className="w-4 h-4" />} label="Expenses" />
              )}
              <NavPill to="/calendar" icon={<CalendarDays className="w-4 h-4" />} label="Calendar" />
              <NavPill to="/stats" icon={<BarChart2 className="w-4 h-4" />} label="Statistics" />
              <button
                onClick={() => setScannerOpen(true)}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium text-gray-600 dark:text-slate-300 hover:bg-white dark:hover:bg-slate-700 hover:shadow-sm hover:text-honey-700 dark:hover:text-honey-300 transition-all"
              >
                <QrCode className="w-4 h-4" />
                Scan
              </button>
            </nav>

            {/* Command palette trigger */}
            <button
              onClick={() => setPaletteOpen(true)}
              className="hidden md:flex items-center gap-2 pl-3 pr-2 py-1.5 rounded-xl text-sm text-gray-500 dark:text-slate-400 bg-gray-100 dark:bg-slate-800 hover:bg-gray-200 dark:hover:bg-slate-700 transition-colors"
              aria-label="Open search"
            >
              <Search className="w-4 h-4" />
              <span className="hidden lg:inline">Search</span>
              <kbd className="text-[10px] font-mono bg-white dark:bg-slate-900 border border-gray-200 dark:border-slate-700 rounded px-1.5 py-0.5 leading-none">
                {isMac ? '⌘K' : 'Ctrl K'}
              </kbd>
            </button>

            {/* Dark mode toggle */}
            <button
              onClick={toggleTheme}
              className="w-8 h-8 rounded-full flex items-center justify-center text-gray-500 dark:text-slate-300 hover:bg-gray-100 dark:hover:bg-slate-800 transition-colors"
              aria-label={isDark ? 'Switch to light mode' : 'Switch to dark mode'}
              title={isDark ? 'Light mode' : 'Dark mode'}
            >
              {isDark ? <Sun className="w-[18px] h-[18px]" /> : <Moon className="w-[18px] h-[18px]" />}
            </button>

            {/* Notification bell */}
            <NotificationBell />

            {/* Profile avatar + dropdown */}
            <div ref={profileRef} className="relative">
              <button
                onClick={() => setProfileOpen(v => !v)}
                className={clsx(
                  'w-8 h-8 rounded-full flex items-center justify-center font-semibold text-sm select-none',
                  'transition-all hover:ring-2 hover:ring-honey-300 hover:ring-offset-1',
                  avatarClass,
                  profileOpen && 'ring-2 ring-honey-400 ring-offset-1'
                )}
                aria-label="Open profile menu"
              >
                {user?.firstName[0] ?? '?'}
              </button>

              {/* Dropdown */}
              {profileOpen && (
                <div className="absolute right-0 top-11 w-56 bg-white dark:bg-slate-800 rounded-2xl shadow-xl border border-gray-100 dark:border-slate-700 overflow-hidden animate-fade-in">
                  {/* User info */}
                  <div className="px-4 py-3 border-b border-gray-100 dark:border-slate-700">
                    <div className="flex items-center gap-2.5">
                      <div className={clsx('w-9 h-9 rounded-full flex items-center justify-center font-semibold text-sm shrink-0', avatarClass)}>
                        {user?.firstName[0]}
                      </div>
                      <div className="min-w-0">
                        <p className="text-sm font-semibold text-gray-800 dark:text-slate-100 truncate">
                          {user?.firstName} {user?.lastName}
                        </p>
                        <p className="text-xs text-gray-500 dark:text-slate-400 truncate mt-0.5">{roleLabel}</p>
                      </div>
                    </div>
                  </div>
                  {/* Edit profile */}
                  <button
                    onClick={() => { setProfileOpen(false); navigate('/profile') }}
                    className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm font-medium text-gray-700 dark:text-slate-200 hover:bg-gray-50 dark:hover:bg-slate-700 transition-colors"
                  >
                    <Settings className="w-4 h-4" />
                    Edit Profile
                  </button>
                  {/* Sign out */}
                  <button
                    onClick={() => { setProfileOpen(false); handleLogout() }}
                    className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors"
                  >
                    <LogOut className="w-4 h-4" />
                    Sign out
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* ── Mobile: dark toggle + hamburger ─────────────────────────────── */}
          <div className="sm:hidden flex items-center gap-1">
            <button
              onClick={() => setPaletteOpen(true)}
              className="p-2 rounded-lg text-gray-600 dark:text-slate-300 hover:bg-honey-100 dark:hover:bg-slate-800 transition-colors"
              aria-label="Search"
            >
              <Search className="w-5 h-5" />
            </button>
            <button
              onClick={toggleTheme}
              className="p-2 rounded-lg text-gray-600 dark:text-slate-300 hover:bg-honey-100 dark:hover:bg-slate-800 transition-colors"
              aria-label={isDark ? 'Switch to light mode' : 'Switch to dark mode'}
            >
              {isDark ? <Sun className="w-5 h-5" /> : <Moon className="w-5 h-5" />}
            </button>
            <button
              className="p-2 rounded-lg text-gray-600 dark:text-slate-300 hover:bg-honey-100 dark:hover:bg-slate-800 transition-colors"
              onClick={() => setMobileOpen(v => !v)}
              aria-label="Toggle menu"
            >
              {mobileOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
            </button>
          </div>
        </div>

        {/* Mobile panel */}
        {mobileOpen && (
          <div className="sm:hidden border-t border-honey-100 dark:border-slate-800 bg-white dark:bg-slate-900 px-4 py-3 space-y-1 animate-fade-in">
            {/* Nav items */}
            {isSystemAdmin ? (
              <MobileNavItem
                to="/admin"
                icon={<LayoutDashboard className="w-4 h-4" />}
                label="Dashboard"
                onClick={() => setMobileOpen(false)}
              />
            ) : (
              <MobileNavItem
                to="/apiaries"
                icon={<Home className="w-4 h-4" />}
                label="Apiaries"
                onClick={() => setMobileOpen(false)}
              />
            )}
            {(isOrgAdmin || isAdmin) && (
              <MobileNavItem
                to="/members"
                icon={<Users className="w-4 h-4" />}
                label="Members"
                onClick={() => setMobileOpen(false)}
              />
            )}
            {canSeeExpenses && (
              <MobileNavItem
                to="/expenses"
                icon={<ReceiptText className="w-4 h-4" />}
                label="Expenses"
                onClick={() => setMobileOpen(false)}
              />
            )}
            <MobileNavItem
              to="/calendar"
              icon={<CalendarDays className="w-4 h-4" />}
              label="Calendar"
              onClick={() => setMobileOpen(false)}
            />
            <MobileNavItem
              to="/stats"
              icon={<BarChart2 className="w-4 h-4" />}
              label="Statistics"
              onClick={() => setMobileOpen(false)}
            />
            <button
              onClick={() => { setMobileOpen(false); setScannerOpen(true) }}
              className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-gray-700 dark:text-slate-200 hover:bg-honey-50 dark:hover:bg-slate-800 transition-colors"
            >
              <QrCode className="w-4 h-4 text-honey-600 dark:text-honey-400" />
              Scan
            </button>

            {/* User section */}
            {user && (
              <div className="pt-2 mt-1 border-t border-honey-100 dark:border-slate-800 space-y-1">
                <div className="flex items-center gap-3 px-3 py-2.5 rounded-xl bg-gray-50 dark:bg-slate-800">
                  <div className={clsx('w-8 h-8 rounded-full flex items-center justify-center font-semibold text-sm shrink-0', avatarClass)}>
                    {user.firstName[0]}
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-semibold text-gray-800 dark:text-slate-100 truncate">
                      {user.firstName} {user.lastName}
                    </p>
                    <p className="text-xs text-gray-500 dark:text-slate-400 truncate">{roleLabel}</p>
                  </div>
                </div>
                <button
                  onClick={() => { setMobileOpen(false); navigate('/profile') }}
                  className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-gray-700 dark:text-slate-200 hover:bg-honey-50 dark:hover:bg-slate-800 transition-colors"
                >
                  <Settings className="w-4 h-4 text-honey-600 dark:text-honey-400" />
                  Edit Profile
                </button>
                <button
                  onClick={() => { setMobileOpen(false); handleLogout() }}
                  className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors"
                >
                  <LogOut className="w-4 h-4" />
                  Sign out
                </button>
              </div>
            )}
          </div>
        )}
      </header>

      {/* ── Main Content ────────────────────────────────────────────────────── */}
      <main className="flex-1 max-w-5xl mx-auto w-full px-4 py-6">
        <Outlet />
      </main>

      {/* ── Footer ──────────────────────────────────────────────────────────── */}
      <footer className="border-t border-honey-200 dark:border-slate-800 bg-white dark:bg-slate-900 py-4 text-center text-xs text-gray-400 dark:text-slate-500">
        BeeHive App © {new Date().getFullYear()} — Keeping your colonies thriving 🍯
      </footer>

      {/* ── Mobile FAB (scan) ───────────────────────────────────────────────── */}
      <button
        onClick={() => setScannerOpen(true)}
        className="sm:hidden fixed bottom-6 right-6 z-40 flex items-center justify-center w-14 h-14 rounded-full bg-honey-500 hover:bg-honey-600 active:bg-honey-700 text-white shadow-honey shadow-lg transition-colors"
        aria-label="Scan beehive QR code"
      >
        <QrCode className="w-6 h-6" />
      </button>

      {/* ── QR Scanner Modal ────────────────────────────────────────────────── */}
      {scannerOpen && <QrScannerModal onClose={() => setScannerOpen(false)} />}

      {/* ── Command palette (Ctrl/Cmd+K) ────────────────────────────────────── */}
      <CommandPalette open={paletteOpen} onClose={() => setPaletteOpen(false)} />
    </div>
  )
}

// ── Desktop nav pill ──────────────────────────────────────────────────────────

function NavPill({ to, icon, label }: { to: string; icon: React.ReactNode; label: string }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        clsx(
          'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-all',
          isActive
            ? 'bg-white dark:bg-slate-700 text-honey-800 dark:text-honey-300 shadow-sm'
            : 'text-gray-600 dark:text-slate-300 hover:bg-white/70 dark:hover:bg-slate-700/70 hover:text-honey-700 dark:hover:text-honey-300'
        )
      }
    >
      {icon}
      {label}
    </NavLink>
  )
}

// ── Mobile nav item ───────────────────────────────────────────────────────────

function MobileNavItem({ to, icon, label, onClick }: { to: string; icon: React.ReactNode; label: string; onClick: () => void }) {
  return (
    <NavLink
      to={to}
      onClick={onClick}
      className={({ isActive }) =>
        clsx(
          'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-colors',
          isActive
            ? 'bg-honey-100 dark:bg-honey-500/15 text-honey-800 dark:text-honey-300'
            : 'text-gray-700 dark:text-slate-200 hover:bg-honey-50 dark:hover:bg-slate-800'
        )
      }
    >
      <span className="text-honey-600 dark:text-honey-400">{icon}</span>
      {label}
    </NavLink>
  )
}
