import { useEffect, useRef, useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { Home, LayoutDashboard, LogOut, Menu, QrCode, Settings, X } from 'lucide-react'
import clsx from 'clsx'
import { useAuth } from '../../core/context/AuthContext'
import QrScannerModal from './QrScannerModal'

export default function Layout() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)
  const [scannerOpen, setScannerOpen] = useState(false)
  const profileRef = useRef<HTMLDivElement>(null)
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const isSystemAdmin = user?.role === 'SystemAdmin'
  const isOrgAdmin    = user?.role === 'OrgAdmin'
  const isAdmin       = user?.role === 'Admin'

  const avatarClass = isSystemAdmin
    ? 'bg-purple-100 text-purple-700'
    : isOrgAdmin
    ? 'bg-blue-100 text-blue-700'
    : 'bg-honey-100 text-honey-700'

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

  return (
    <div className="min-h-screen flex flex-col bg-honey-50">

      {/* ── Header ──────────────────────────────────────────────────────────── */}
      <header className="sticky top-0 z-40 bg-white/90 backdrop-blur border-b border-honey-200 shadow-sm">
        <div className="max-w-5xl mx-auto px-4 h-14 flex items-center justify-between gap-4">

          {/* Logo */}
          <Link
            to={isSystemAdmin ? '/admin' : '/apiaries'}
            className="flex items-center gap-2 group shrink-0"
          >
            <span className="text-2xl leading-none">🐝</span>
            <span className="font-display text-xl font-bold text-honey-800 group-hover:text-honey-600 transition-colors">
              BeeHive
            </span>
          </Link>

          {/* ── Desktop right side ─────────────────────────────────────────── */}
          <div className="hidden sm:flex items-center gap-3">

            {/* Nav pill group */}
            <nav className="flex items-center gap-0.5 bg-gray-100 rounded-xl p-1">
              {isSystemAdmin ? (
                <NavPill to="/admin" icon={<LayoutDashboard className="w-4 h-4" />} label="Dashboard" />
              ) : (
                <NavPill to="/apiaries" icon={<Home className="w-4 h-4" />} label="Apiaries" />
              )}
              <button
                onClick={() => setScannerOpen(true)}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium text-gray-600 hover:bg-white hover:shadow-sm hover:text-honey-700 transition-all"
              >
                <QrCode className="w-4 h-4" />
                Scan
              </button>
            </nav>

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
                <div className="absolute right-0 top-11 w-56 bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden animate-fade-in">
                  {/* User info */}
                  <div className="px-4 py-3 border-b border-gray-100">
                    <div className="flex items-center gap-2.5">
                      <div className={clsx('w-9 h-9 rounded-full flex items-center justify-center font-semibold text-sm shrink-0', avatarClass)}>
                        {user?.firstName[0]}
                      </div>
                      <div className="min-w-0">
                        <p className="text-sm font-semibold text-gray-800 truncate">
                          {user?.firstName} {user?.lastName}
                        </p>
                        <p className="text-xs text-gray-500 truncate mt-0.5">{roleLabel}</p>
                      </div>
                    </div>
                  </div>
                  {/* Edit profile */}
                  <button
                    onClick={() => { setProfileOpen(false); navigate('/profile') }}
                    className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                  >
                    <Settings className="w-4 h-4" />
                    Edit Profile
                  </button>
                  {/* Sign out */}
                  <button
                    onClick={() => { setProfileOpen(false); handleLogout() }}
                    className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm font-medium text-red-600 hover:bg-red-50 transition-colors"
                  >
                    <LogOut className="w-4 h-4" />
                    Sign out
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* ── Mobile: hamburger ──────────────────────────────────────────── */}
          <button
            className="sm:hidden p-2 rounded-lg text-gray-600 hover:bg-honey-100 transition-colors"
            onClick={() => setMobileOpen(v => !v)}
            aria-label="Toggle menu"
          >
            {mobileOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
          </button>
        </div>

        {/* Mobile panel */}
        {mobileOpen && (
          <div className="sm:hidden border-t border-honey-100 bg-white px-4 py-3 space-y-1 animate-fade-in">
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
            <button
              onClick={() => { setMobileOpen(false); setScannerOpen(true) }}
              className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-gray-700 hover:bg-honey-50 transition-colors"
            >
              <QrCode className="w-4 h-4 text-honey-600" />
              Scan
            </button>

            {/* User section */}
            {user && (
              <div className="pt-2 mt-1 border-t border-honey-100 space-y-1">
                <div className="flex items-center gap-3 px-3 py-2.5 rounded-xl bg-gray-50">
                  <div className={clsx('w-8 h-8 rounded-full flex items-center justify-center font-semibold text-sm shrink-0', avatarClass)}>
                    {user.firstName[0]}
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-semibold text-gray-800 truncate">
                      {user.firstName} {user.lastName}
                    </p>
                    <p className="text-xs text-gray-500 truncate">{roleLabel}</p>
                  </div>
                </div>
                <button
                  onClick={() => { setMobileOpen(false); navigate('/profile') }}
                  className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-gray-700 hover:bg-honey-50 transition-colors"
                >
                  <Settings className="w-4 h-4 text-honey-600" />
                  Edit Profile
                </button>
                <button
                  onClick={() => { setMobileOpen(false); handleLogout() }}
                  className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-red-600 hover:bg-red-50 transition-colors"
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
      <footer className="border-t border-honey-200 bg-white py-4 text-center text-xs text-gray-400">
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
            ? 'bg-white text-honey-800 shadow-sm'
            : 'text-gray-600 hover:bg-white/70 hover:text-honey-700'
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
          isActive ? 'bg-honey-100 text-honey-800' : 'text-gray-700 hover:bg-honey-50'
        )
      }
    >
      <span className="text-honey-600">{icon}</span>
      {label}
    </NavLink>
  )
}
