import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Users, Pencil, Loader2, AlertCircle, Search } from 'lucide-react'
import { useOrgMembers } from '../../core/services/orgQueries'
import { useAuth } from '../../core/context/AuthContext'

export default function MembersPage() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { data: members = [], isLoading, error } = useOrgMembers()
  const [query, setQuery] = useState('')

  const isOrgAdmin = user?.role === 'OrgAdmin'

  // ── Derived vitals ──
  const adminCount = members.filter(m => m.role === 'Admin').length
  const userCount  = members.filter(m => m.role === 'User').length
  const assignedCount = members.filter(m =>
    m.role === 'Admin' ? !!m.apiaryName : m.assignedBeehiveNames.length > 0,
  ).length

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase()
    if (!q) return members
    return members.filter(m =>
      `${m.firstName} ${m.lastName}`.toLowerCase().includes(q) ||
      m.email.toLowerCase().includes(q) ||
      m.role.toLowerCase().includes(q),
    )
  }, [members, query])

  return (
    <div className="animate-fade-in space-y-6">

      {/* ── Hero ──────────────────────────────────────────────────────────────── */}
      <div className="relative overflow-hidden rounded-3xl border border-honey-200 dark:border-slate-800
                      bg-gradient-to-br from-honey-100 via-white to-honey-50
                      dark:from-slate-900 dark:via-slate-900 dark:to-slate-950 shadow-card dark:shadow-none">
        <div className="absolute inset-0 bg-honeycomb opacity-60 dark:opacity-100 pointer-events-none" />
        <div className="relative p-5 sm:p-7 flex items-center gap-4">
          <div className="w-14 h-14 shrink-0 rounded-2xl bg-white/70 dark:bg-slate-800 border border-honey-200 dark:border-slate-700 flex items-center justify-center text-3xl shadow-honey dark:shadow-none">
            👥
          </div>
          <div className="min-w-0">
            <h1 className="font-display text-2xl sm:text-3xl font-bold text-gray-900 dark:text-slate-50">Members</h1>
            <p className="mt-0.5 text-sm text-gray-600 dark:text-slate-400">
              {isOrgAdmin
                ? 'Assign apiaries to Admins and beehives to Users in your organization.'
                : 'Assign beehives from your apiary to Users in your organization.'}
            </p>
          </div>
        </div>
      </div>

      {error && (
        <div className="flex items-start gap-2 bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/30 text-red-700 dark:text-red-300 rounded-lg px-4 py-3 text-sm">
          <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
          Failed to load members.
        </div>
      )}

      {isLoading ? (
        <div className="flex justify-center py-16">
          <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
        </div>
      ) : members.length === 0 ? (
        <div className="text-center py-16 bg-white dark:bg-slate-900 rounded-2xl border border-dashed border-honey-200 dark:border-slate-700">
          <Users className="w-8 h-8 text-honey-300 dark:text-honey-500/50 mx-auto mb-2" />
          <p className="text-sm text-gray-500 dark:text-slate-400">No assignable members found in your organization.</p>
        </div>
      ) : (
        <>
          {/* ── Vitals strip ──────────────────────────────────────────────── */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
            <VitalCard icon="👥" label="Members"  value={String(members.length)} sub="in your org"    gradient="from-honey-400 to-honey-600" />
            <VitalCard icon="🛡️" label="Admins"   value={String(adminCount)}     sub="apiary admins"  gradient="from-amber-400 to-orange-500" />
            <VitalCard icon="🧑‍🌾" label="Users"    value={String(userCount)}      sub="field workers"  gradient="from-sky-400 to-blue-600" />
            <VitalCard icon="🔗" label="Assigned" value={String(assignedCount)}  sub="have a posting" gradient="from-violet-400 to-indigo-600" />
          </div>

          {/* ── Members table ─────────────────────────────────────────────── */}
          <section className="rounded-2xl border border-honey-100 dark:border-slate-800 bg-white dark:bg-slate-900 shadow-card dark:shadow-none overflow-hidden">
            <div className="flex flex-col sm:flex-row sm:items-center gap-3 p-4">
              <h2 className="flex items-center gap-2 font-display text-lg font-semibold text-gray-800 dark:text-slate-100">
                <Users className="w-5 h-5 text-honey-600 dark:text-honey-400" />
                Team
                <span className="badge bg-honey-100 text-honey-700 dark:bg-honey-500/15 dark:text-honey-300 text-xs">{members.length}</span>
              </h2>
              <div className="relative sm:ml-auto sm:w-56">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 dark:text-slate-500 pointer-events-none" />
                <input
                  value={query}
                  onChange={e => setQuery(e.target.value)}
                  placeholder="Search members…"
                  className="form-input pl-9 py-2 text-sm w-full"
                />
              </div>
            </div>

            {filtered.length === 0 ? (
              <div className="text-center py-12 border-t border-honey-100 dark:border-slate-800">
                <Search className="w-7 h-7 text-honey-300 dark:text-honey-500/40 mx-auto mb-2" />
                <p className="text-sm text-gray-500 dark:text-slate-400">No members match “{query}”.</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="bg-honey-50 dark:bg-slate-800/60 border-y border-honey-100 dark:border-slate-800">
                    <tr>
                      <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-slate-300">Name</th>
                      <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-slate-300 hidden sm:table-cell">Email</th>
                      <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-slate-300">Role</th>
                      <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-slate-300 hidden md:table-cell">Assignments</th>
                      <th className="px-4 py-3" />
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-honey-50 dark:divide-slate-800">
                    {filtered.map((member) => (
                      <tr key={member.id} className="hover:bg-honey-50/50 dark:hover:bg-slate-800/50 transition-colors">
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-slate-100">
                          {member.firstName} {member.lastName}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-slate-400 hidden sm:table-cell">{member.email}</td>
                        <td className="px-4 py-3">
                          <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium
                            ${member.role === 'Admin' ? 'bg-honey-100 text-honey-700 dark:bg-honey-500/15 dark:text-honey-300' : 'bg-gray-100 text-gray-600 dark:bg-slate-700 dark:text-slate-300'}`}>
                            {member.role}
                          </span>
                        </td>
                        <td className="px-4 py-3 hidden md:table-cell">
                          {member.role === 'Admin' ? (
                            <span className="text-gray-500 dark:text-slate-400 text-xs">
                              {member.apiaryName
                                ? <span className="text-honey-700 dark:text-honey-400 font-medium">{member.apiaryName}</span>
                                : <span className="text-gray-400 dark:text-slate-500 italic">No apiary assigned</span>}
                            </span>
                          ) : (
                            <span className="text-gray-500 dark:text-slate-400 text-xs">
                              {member.assignedBeehiveNames.length > 0
                                ? member.assignedBeehiveNames.join(', ')
                                : <span className="text-gray-400 dark:text-slate-500 italic">No hives assigned</span>}
                            </span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <button
                            onClick={() => navigate(`/members/${member.id}/assignments`)}
                            className="p-1.5 rounded-lg text-gray-400 dark:text-slate-500 hover:text-honey-600 dark:hover:text-honey-400 hover:bg-honey-50 dark:hover:bg-slate-800 transition-colors"
                            title="Edit assignments"
                          >
                            <Pencil className="w-4 h-4" />
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </>
      )}
    </div>
  )
}

// ── Vitals KPI tile ────────────────────────────────────────────────────────────

function VitalCard({ icon, label, value, sub, gradient }: {
  icon: string; label: string; value: string; sub?: string; gradient: string
}) {
  return (
    <div className={`relative overflow-hidden rounded-2xl p-4 sm:p-5 text-white shadow-lg bg-gradient-to-br ${gradient}`}>
      <span className="absolute -right-2 -top-3 text-6xl opacity-20 select-none pointer-events-none leading-none">
        {icon}
      </span>
      <div className="relative">
        <p className="text-2xl sm:text-3xl font-bold font-display leading-none truncate">{value}</p>
        <p className="text-sm font-medium opacity-95 mt-2">{label}</p>
        {sub && <p className="text-xs mt-0.5 opacity-80">{sub}</p>}
      </div>
    </div>
  )
}
