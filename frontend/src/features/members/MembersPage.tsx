import { useNavigate } from 'react-router-dom'
import { Users, Pencil, Loader2, AlertCircle } from 'lucide-react'
import { useOrgMembers } from '../../core/services/orgQueries'
import { useAuth } from '../../core/context/AuthContext'

export default function MembersPage() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { data: members = [], isLoading, error } = useOrgMembers()

  const isOrgAdmin = user?.role === 'OrgAdmin'

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Members</h1>
        <p className="text-sm text-gray-500 mt-1">
          {isOrgAdmin
            ? 'Assign apiaries to Admins and beehives to Users in your organization.'
            : 'Assign beehives from your apiary to Users in your organization.'}
        </p>
      </div>

      {error && (
        <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm">
          <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
          Failed to load members.
        </div>
      )}

      {isLoading ? (
        <div className="flex justify-center py-16">
          <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
        </div>
      ) : members.length === 0 ? (
        <div className="text-center py-16 bg-white rounded-xl border border-dashed border-honey-200">
          <Users className="w-8 h-8 text-honey-300 mx-auto mb-2" />
          <p className="text-sm text-gray-500">No assignable members found in your organization.</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-honey-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-honey-50 border-b border-honey-100">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Name</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600 hidden sm:table-cell">Email</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Role</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600 hidden md:table-cell">Assignments</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-honey-50">
              {members.map((member) => (
                <tr key={member.id} className="hover:bg-honey-50/50 transition-colors">
                  <td className="px-4 py-3 font-medium text-gray-900">
                    {member.firstName} {member.lastName}
                  </td>
                  <td className="px-4 py-3 text-gray-500 hidden sm:table-cell">{member.email}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium
                      ${member.role === 'Admin' ? 'bg-honey-100 text-honey-700' : 'bg-gray-100 text-gray-600'}`}>
                      {member.role}
                    </span>
                  </td>
                  <td className="px-4 py-3 hidden md:table-cell">
                    {member.role === 'Admin' ? (
                      <span className="text-gray-500 text-xs">
                        {member.apiaryName
                          ? <span className="text-honey-700 font-medium">{member.apiaryName}</span>
                          : <span className="text-gray-400 italic">No apiary assigned</span>}
                      </span>
                    ) : (
                      <span className="text-gray-500 text-xs">
                        {member.assignedBeehiveNames.length > 0
                          ? member.assignedBeehiveNames.join(', ')
                          : <span className="text-gray-400 italic">No hives assigned</span>}
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => navigate(`/members/${member.id}/assignments`)}
                      className="p-1.5 rounded-lg text-gray-400 hover:text-honey-600 hover:bg-honey-50 transition-colors"
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
    </div>
  )
}
