import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Loader2, AlertCircle } from 'lucide-react'
import {
  useOrgMember,
  useAvailableBeehives,
  useAvailableApiaries,
  useUpdateBeehiveAssignments,
  useUpdateApiaryAssignment,
} from '../../core/services/orgQueries'
import { useAuth } from '../../core/context/AuthContext'

export default function MemberAssignmentPage() {
  const { id } = useParams<{ id: string }>()
  const memberId = parseInt(id ?? '0')
  const navigate = useNavigate()
  const { user } = useAuth()

  const isOrgAdmin = user?.role === 'OrgAdmin'

  const { data: member, isLoading: loadingMember } = useOrgMember(memberId)
  const { data: beehives = [], isLoading: loadingBeehives } = useAvailableBeehives()
  const { data: apiaries = [], isLoading: loadingApiaries } = useAvailableApiaries(isOrgAdmin)

  const updateBeehives = useUpdateBeehiveAssignments(memberId)
  const updateApiary = useUpdateApiaryAssignment(memberId)

  const [selectedBeehiveIds, setSelectedBeehiveIds] = useState<number[]>([])
  const [selectedApiaryId, setSelectedApiaryId] = useState<string>('')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (member) {
      setSelectedBeehiveIds(member.assignedBeehiveIds ?? [])
      setSelectedApiaryId(member.apiaryId?.toString() ?? '')
    }
  }, [member])

  function toggleBeehive(beehiveId: number) {
    setSelectedBeehiveIds(prev =>
      prev.includes(beehiveId)
        ? prev.filter(id => id !== beehiveId)
        : [...prev, beehiveId]
    )
  }

  async function handleSaveBeehives() {
    setError(null)
    try {
      await updateBeehives.mutateAsync({ beehiveIds: selectedBeehiveIds })
      navigate('/members')
    } catch (e: any) {
      setError(e?.response?.data?.detail ?? e?.message ?? 'Failed to update assignments.')
    }
  }

  async function handleSaveApiary() {
    setError(null)
    try {
      await updateApiary.mutateAsync({ apiaryId: selectedApiaryId ? parseInt(selectedApiaryId) : null })
      navigate('/members')
    } catch (e: any) {
      setError(e?.response?.data?.detail ?? e?.message ?? 'Failed to update apiary assignment.')
    }
  }

  if (loadingMember) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-6 h-6 animate-spin text-honey-500" />
      </div>
    )
  }

  if (!member) {
    return (
      <div className="text-center py-20 text-gray-500">Member not found.</div>
    )
  }

  const isUserRole = member.role === 'User'
  const isAdminRole = member.role === 'Admin'
  const isSaving = updateBeehives.isPending || updateApiary.isPending

  return (
    <div className="max-w-xl mx-auto">
      <button
        onClick={() => navigate('/members')}
        className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-700 mb-6 transition-colors"
      >
        <ArrowLeft className="w-4 h-4" />
        Back to Members
      </button>

      <div className="bg-white rounded-2xl shadow-sm border border-honey-100 px-8 py-8 space-y-6">
        {/* Member info */}
        <div>
          <h1 className="text-xl font-bold text-gray-900">
            {member.firstName} {member.lastName}
          </h1>
          <p className="text-sm text-gray-500 mt-0.5">{member.email}</p>
          <span className={`mt-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium
            ${isAdminRole ? 'bg-honey-100 text-honey-700' : 'bg-gray-100 text-gray-600'}`}>
            {member.role}
          </span>
        </div>

        {error && (
          <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3 text-sm">
            <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
            {error}
          </div>
        )}

        {/* Beehive assignments — for User role */}
        {isUserRole && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Assigned Hives
            </label>
            {loadingBeehives ? (
              <div className="flex justify-center py-6">
                <Loader2 className="w-5 h-5 animate-spin text-honey-400" />
              </div>
            ) : beehives.length === 0 ? (
              <p className="text-sm text-gray-400 py-2">
                No hives available to assign.
              </p>
            ) : (
              <div className="border border-gray-200 rounded-xl divide-y divide-gray-100 max-h-60 overflow-y-auto">
                {beehives.map((b) => (
                  <label
                    key={b.id}
                    className="flex items-center gap-3 px-4 py-2.5 cursor-pointer hover:bg-gray-50 transition-colors"
                  >
                    <input
                      type="checkbox"
                      checked={selectedBeehiveIds.includes(b.id)}
                      onChange={() => toggleBeehive(b.id)}
                      className="w-4 h-4 accent-honey-500"
                    />
                    <span className="text-sm text-gray-800">{b.name}</span>
                    <span className="text-xs text-gray-400 ml-auto">{b.apiaryName}</span>
                  </label>
                ))}
              </div>
            )}
            <p className="mt-1.5 text-xs text-gray-400">
              {selectedBeehiveIds.length > 0
                ? `${selectedBeehiveIds.length} hive${selectedBeehiveIds.length !== 1 ? 's' : ''} selected`
                : 'No hives assigned'}
            </p>

            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={() => navigate('/members')}
                className="flex-1 px-4 py-3 rounded-xl border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleSaveBeehives}
                disabled={isSaving}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold disabled:opacity-60 transition-colors"
              >
                {isSaving && <Loader2 className="w-4 h-4 animate-spin" />}
                Save Assignments
              </button>
            </div>
          </div>
        )}

        {/* Apiary assignment — for Admin role, OrgAdmin only */}
        {isAdminRole && isOrgAdmin && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Assigned Apiary
            </label>
            {loadingApiaries ? (
              <div className="flex justify-center py-6">
                <Loader2 className="w-5 h-5 animate-spin text-honey-400" />
              </div>
            ) : (
              <select
                value={selectedApiaryId}
                onChange={e => setSelectedApiaryId(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm outline-none bg-gray-50 focus:bg-white focus:border-honey-400 focus:ring-2 focus:ring-honey-100 transition-all"
              >
                <option value="">No apiary assigned</option>
                {apiaries.map((a) => (
                  <option key={a.id} value={a.id}>{a.name}</option>
                ))}
              </select>
            )}

            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={() => navigate('/members')}
                className="flex-1 px-4 py-3 rounded-xl border border-gray-200 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleSaveApiary}
                disabled={isSaving}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl bg-honey-500 hover:bg-honey-600 text-white text-sm font-semibold disabled:opacity-60 transition-colors"
              >
                {isSaving && <Loader2 className="w-4 h-4 animate-spin" />}
                Save Assignment
              </button>
            </div>
          </div>
        )}

        {/* Admin member but caller is also Admin (not OrgAdmin) */}
        {isAdminRole && !isOrgAdmin && (
          <div className="text-sm text-gray-500 bg-gray-50 rounded-xl px-4 py-3">
            Only Organization Admins can change apiary assignments for Admin users.
          </div>
        )}
      </div>
    </div>
  )
}
