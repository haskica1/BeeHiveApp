import { useState } from 'react'
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'
import { Home, Menu, X } from 'lucide-react'
import clsx from 'clsx'

export default function Layout() {
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <div className="min-h-screen flex flex-col bg-honey-50">
      {/* ── Top Nav Bar ─────────────────────────────────────────────────────── */}
      <header className="sticky top-0 z-40 bg-white/90 backdrop-blur border-b border-honey-200 shadow-sm">
        <div className="max-w-5xl mx-auto px-4 h-14 flex items-center justify-between">
          {/* Logo */}
          <Link to="/apiaries" className="flex items-center gap-2.5 group">
            <span className="text-2xl">🐝</span>
            <span className="font-display text-xl font-bold text-honey-800 group-hover:text-honey-600 transition-colors">
              BeeHive
            </span>
          </Link>

          {/* Desktop nav */}
          <nav className="hidden sm:flex items-center gap-1">
            <NavItem to="/apiaries" icon={<Home className="w-4 h-4" />} label="Apiaries" />
          </nav>

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
            <MobileNavItem
              to="/apiaries"
              label="🏡 Apiaries"
              onClick={() => setMobileOpen(false)}
            />
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
