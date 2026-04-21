import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Building2, Users, Plus, Pencil, Trash2, AlertCircle, Loader2 } from 'lucide-react'
import {
  useAdminOrganizations,
  useAdminUsers,
  useDeleteOrganization,
  useDeleteAdminUser,
} from '../../core/services/adminQueries'

export default function AdminDashboardPage() {
  const navigate = useNavigate()
  const { data: organizations = [], isLoading: orgsLoading } = useAdminOrganizations()
  const { data: users = [], isLoading: usersLoading } = useAdminUsers()
  const deleteOrg = useDeleteOrganization()
  const deleteUser = useDeleteAdminUser()

  const [deletingOrgId, setDeletingOrgId] = useState<number | null>(null)
  const [deletingUserId, setDeletingUserId] = useState<number | null>(null)
  const [orgError, setOrgError] = useState<string | null>(null)
  const [userError, setUserError] = useState<string | null>(null)

  async function handleDeleteOrg(id: number, name: string) {
    if (!confirm(`Delete organization "${name}"? This cannot be undone.`)) return
    setOrgError(null)
    setDeletingOrgId(id)
    try {
      await deleteOrg.mutateAsync(id)
    } catch (e: any) {
      setOrgError(e?.response?.data?.detail ?? e?.message ?? 'Failed to delete organization.')
    } finally {
      setDeletingOrgId(null)
    }
  }

  async function handleDeleteUser(id: number, name: string) {
    if (!confirm(`Delete user "${name}"? This cannot be undone.`)) return
    setUserError(null)
    setDeletingUserId(id)
    try {
      await deleteUser.mutateAsync(id)
    } catch (e: any) {
      setUserError(e?.response?.data?.detail ?? e?.message ?? 'Failed to delete user.')
    } finally {
      setDeletingUserId(null)
    }
  }

  return (
    <div className="space-y-10">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">System Dashboard</h1>
        <p className="text-sm text-gray-500 mt-1">Manage all organizations and users across the platform.</p>
      </div>

      {/* Stats strip */}
      <div className="grid grid-cols-2 gap-4">
        <div className="bg-white rounded-xl border border-honey-200 p-5 flex items-center gap-4">
          <div className="w-10 h-10 rounded-lg bg-honey-100 flex items-center justify-center">
            <Building2 className="w-5 h-5 text-honey-700" />
          </div>
          <div>
            <p className="text-2xl font-bold text-gray-900">{organizations.length}</p>
            <p className="text-sm text-gray-500">Organizations</p>
          </div>
        </div>
        <div className="bg-white rounded-xl border border-honey-200 p-5 flex items-center gap-4">
          <div className="w-10 h-10 rounded-lg bg-honey-100 flex items-center justify-center">
            <Users className="w-5 h-5 text-honey-700" />
          </div>
          <div>
            <p className="text-2xl font-bold text-gray-900">{users.length}</p>
            <p className="text-sm text-gray-500">Users</p>
          </div>
        </div>
      </div>

      {/* ── Organizations ───────────────────────────────────────────────────── */}
      <section>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
            <Building2 className="w-5 h-5 text-honey-600" />
            Organizations
          </h2>
          <button
            onClick={() => navigate('/admin/organizations/new')}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-honey-500 hover:bg-honey-600 text-white text-sm font-medium rounded-lg transition-colors"
          >
            <Plus className="w-4 h-4" />
            Add Organization
          </button>
        </div>

        {orgError && (
          <div className="mb-3 flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm">
            <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
            {orgError}
          </div>
        )}

        {orgsLoading ? (
          <div className="flex justify-center py-10">
            <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
          </div>
        ) : organizations.length === 0 ? (
          <div className="text-center py-10 bg-white rounded-xl border border-dashed border-honey-200">
            <Building2 className="w-8 h-8 text-honey-300 mx-auto mb-2" />
            <p className="text-sm text-gray-500">No organizations yet.</p>
          </div>
        ) : (
          <div className="bg-white rounded-xl border border-honey-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-honey-50 border-b border-honey-100">
                <tr>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Name</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600 hidden md:table-cell">Description</th>
                  <th className="text-center px-4 py-3 font-medium text-gray-600">Users</th>
                  <th className="text-center px-4 py-3 font-medium text-gray-600">Apiaries</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-honey-50">
                {organizations.map((org) => (
                  <tr key={org.id} className="hover:bg-honey-50/50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-900">{org.name}</td>
                    <td className="px-4 py-3 text-gray-500 hidden md:table-cell max-w-xs truncate">
                      {org.description ?? '—'}
                    </td>
                    <td className="px-4 py-3 text-center text-gray-700">{org.userCount}</td>
                    <td className="px-4 py-3 text-center text-gray-700">{org.apiaryCount}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => navigate(`/admin/organizations/${org.id}/edit`)}
                          className="p-1.5 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
                          title="Edit"
                        >
                          <Pencil className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDeleteOrg(org.id, org.name)}
                          disabled={deletingOrgId === org.id}
                          className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 transition-colors disabled:opacity-50"
                          title="Delete"
                        >
                          {deletingOrgId === org.id
                            ? <Loader2 className="w-4 h-4 animate-spin" />
                            : <Trash2 className="w-4 h-4" />}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {/* ── Users ────────────────────────────────────────────────────────────── */}
      <section>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
            <Users className="w-5 h-5 text-honey-600" />
            Users
          </h2>
          <button
            onClick={() => navigate('/admin/users/new')}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-honey-500 hover:bg-honey-600 text-white text-sm font-medium rounded-lg transition-colors"
          >
            <Plus className="w-4 h-4" />
            Add User
          </button>
        </div>

        {userError && (
          <div className="mb-3 flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm">
            <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
            {userError}
          </div>
        )}

        {usersLoading ? (
          <div className="flex justify-center py-10">
            <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
          </div>
        ) : users.length === 0 ? (
          <div className="text-center py-10 bg-white rounded-xl border border-dashed border-honey-200">
            <Users className="w-8 h-8 text-honey-300 mx-auto mb-2" />
            <p className="text-sm text-gray-500">No users yet.</p>
          </div>
        ) : (
          <div className="bg-white rounded-xl border border-honey-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-honey-50 border-b border-honey-100">
                <tr>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Name</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600 hidden sm:table-cell">Email</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Role</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600 hidden md:table-cell">Organization</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-honey-50">
                {users.map((user) => (
                  <tr key={user.id} className="hover:bg-honey-50/50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-900">
                      {user.firstName} {user.lastName}
                    </td>
                    <td className="px-4 py-3 text-gray-500 hidden sm:table-cell">{user.email}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium
                        ${user.role === 'SystemAdmin'
                          ? 'bg-purple-100 text-purple-700'
                          : 'bg-honey-100 text-honey-700'
                        }`}>
                        {user.role === 'SystemAdmin' ? 'System Admin' : 'Admin'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500 hidden md:table-cell">
                      {user.organizationName ?? '—'}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => navigate(`/admin/users/${user.id}/edit`)}
                          className="p-1.5 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
                          title="Edit"
                        >
                          <Pencil className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDeleteUser(user.id, `${user.firstName} ${user.lastName}`)}
                          disabled={deletingUserId === user.id}
                          className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 transition-colors disabled:opacity-50"
                          title="Delete"
                        >
                          {deletingUserId === user.id
                            ? <Loader2 className="w-4 h-4 animate-spin" />
                            : <Trash2 className="w-4 h-4" />}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
