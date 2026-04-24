import { useAuth } from '../context/AuthContext'

export function usePermissions() {
  const { user } = useAuth()
  const role = user?.role

  return {
    /** SystemAdmin or OrgAdmin — can create/edit/delete apiaries */
    canManageApiaries: role === 'SystemAdmin' || role === 'OrgAdmin',
    /** SystemAdmin, OrgAdmin, or Admin — can edit/delete beehives, inspections, diets, todos */
    canEditDelete: role === 'SystemAdmin' || role === 'OrgAdmin' || role === 'Admin',
  }
}
