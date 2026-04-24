import { useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { Home, LayoutDashboard, LogOut, Menu, X } from 'lucide-react'
import clsx from 'clsx'
import { useAuth } from '../../core/context/AuthContext'

export default function Layout() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const isSystemAdmin = user?.role === 'SystemAdmin'
  const isOrgAdmin = user?.role === 'OrgAdmin'
  const isAdmin = user?.role === 'Admin'
  const isUser = user?.role === 'User'

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="min-h-screen flex flex-col bg-honey-50">
      {/* ── Top Nav Bar ─────────────────────────────────────────────────────── */}
      <header className="sticky top-0 z-40 bg-white/90 backdrop-blur border-b border-honey-200 shadow-sm">
        <div className="max-w-5xl mx-auto px-4 h-14 flex items-center justify-between">
          {/* Logo */}
          <Link
            to={isSystemAdmin ? '/admin' : '/apiaries'}
            className="flex items-center gap-2.5 group"
          >
            <span className="text-2xl">🐝</span>
            <span className="font-display text-xl font-bold text-honey-800 group-hover:text-honey-600 transition-colors">
              BeeHive
            </span>
          </Link>

          {/* Desktop nav */}
          <nav className="hidden sm:flex items-center gap-1">
            {isSystemAdmin ? (
              <NavItem to="/admin" icon={<LayoutDashboard className="w-4 h-4" />} label="Dashboard" />
            ) : (
              <NavItem to="/apiaries" icon={<Home className="w-4 h-4" />} label="Apiaries" />
            )}
          </nav>

          {/* User info + logout (desktop) */}
          <div className="hidden sm:flex items-center gap-3">
            {user && (
              <div className="text-right">
                <p className="text-sm font-medium text-gray-800 leading-tight">
                  {user.firstName} {user.lastName}
                </p>
                <p className="text-xs leading-tight">
                  {isSystemAdmin ? (
                    <span className="text-purple-600 font-medium">System Admin</span>
                  ) : isOrgAdmin ? (
                    <span className="text-blue-600 font-medium">Org Admin · {user.organizationName}</span>
                  ) : isAdmin ? (
                    <span className="text-honey-600 font-medium">Admin · {user.organizationName}</span>
                  ) : isUser ? (
                    <span className="text-gray-500">{user.organizationName}</span>
                  ) : (
                    <span className="text-honey-600">{user.organizationName}</span>
                  )}
                </p>
              </div>
            )}
            <div className={clsx(
              'w-8 h-8 rounded-full flex items-center justify-center font-semibold text-sm select-none',
              isSystemAdmin ? 'bg-purple-100 text-purple-700'
              : isOrgAdmin ? 'bg-blue-100 text-blue-700'
              : 'bg-honey-100 text-honey-700'
            )}>
              {user ? user.firstName[0] : '?'}
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm text-gray-600 hover:bg-red-50 hover:text-red-600 transition-colors"
              title="Sign out"
            >
              <LogOut className="w-4 h-4" />
              <span className="hidden md:inline">Sign out</span>
            </button>
          </div>

          {/* Mobile menu button */}
          <button
            className="sm:hidden p-2 rounded-lg text-gray-600 hover:bg-honey-100 transition-colors"
            onClick={() => setMobileOpen(v => !v)}
            aria-label="Toggle menu"
          >
            {mobileOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
          </button>
        </div>

        {/* Mobile dropdown nav */}
        {mobileOpen && (
          <div className="sm:hidden border-t border-honey-100 bg-white px-4 py-2 animate-fade-in">
            {isSystemAdmin ? (
              <MobileNavItem
                to="/admin"
                label="🗂 Dashboard"
                onClick={() => setMobileOpen(false)}
              />
            ) : (
              <MobileNavItem
                to="/apiaries"
                label="🏡 Apiaries"
                onClick={() => setMobileOpen(false)}
              />
            )}
            {user && (
              <div className="mt-2 pt-2 border-t border-honey-50">
                <p className="px-3 py-1 text-xs text-gray-500">
                  {user.firstName} {user.lastName}
                  {isSystemAdmin ? ' · System Admin'
                  : isOrgAdmin ? ` · Org Admin · ${user.organizationName}`
                  : ` · ${user.organizationName}`}
                </p>
                <button
                  onClick={() => { setMobileOpen(false); handleLogout() }}
                  className="w-full text-left flex items-center gap-2 px-3 py-2.5 rounded-lg text-sm font-medium text-red-600 hover:bg-red-50 transition-colors"
                >
                  <LogOut className="w-4 h-4" />
                  Sign out
                </button>
              </div>
            )}
          </div>
        )}
      </header>

      {/* ── Main Content ─────────────────────────────────────────────────────── */}
      <main className="flex-1 max-w-5xl mx-auto w-full px-4 py-6">
        <Outlet />
      </main>

      {/* ── Footer ───────────────────────────────────────────────────────────── */}
      <footer className="border-t border-honey-200 bg-white py-4 text-center text-xs text-gray-400">
        BeeHive App © {new Date().getFullYear()} — Keeping your colonies thriving 🍯
      </footer>
    </div>
  )
}

function NavItem({ to, icon, label }: { to: string; icon: React.ReactNode; label: string }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        clsx(
          'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors',
          isActive
            ? 'bg-honey-100 text-honey-800'
            : 'text-gray-600 hover:bg-honey-50 hover:text-honey-700',
        )
      }
    >
      {icon}
      {label}
    </NavLink>
  )
}

function MobileNavItem({ to, label, onClick }: { to: string; label: string; onClick: () => void }) {
  return (
    <NavLink
      to={to}
      onClick={onClick}
      className={({ isActive }) =>
        clsx(
          'block px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
          isActive ? 'bg-honey-100 text-honey-800' : 'text-gray-700 hover:bg-honey-50',
        )
      }
    >
      {label}
    </NavLink>
  )
}
