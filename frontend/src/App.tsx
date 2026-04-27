import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './core/context/AuthContext'
import Layout from './shared/components/Layout'
import ProtectedRoute from './shared/components/ProtectedRoute'
import AdminRoute from './shared/components/AdminRoute'
import RoleRoute from './shared/components/RoleRoute'
import LoginPage from './features/auth/LoginPage'
import ApiaryListPage from './features/apiaries/ApiaryListPage'
import ApiaryDetailPage from './features/apiaries/ApiaryDetailPage'
import ApiaryFormPage from './features/apiaries/ApiaryFormPage'
import BeehiveDetailPage from './features/beehives/BeehiveDetailPage'
import BeehiveFormPage from './features/beehives/BeehiveFormPage'
import InspectionFormPage from './features/inspections/InspectionFormPage'
import DietFormPage from './features/diets/DietFormPage'
import DietDetailPage from './features/diets/DietDetailPage'
import AdminDashboardPage from './features/admin/AdminDashboardPage'
import OrganizationFormPage from './features/admin/OrganizationFormPage'
import UserFormPage from './features/admin/UserFormPage'
import SmartRedirect from './shared/components/SmartRedirect'

const APIARY_MANAGERS  = ['OrgAdmin', 'SystemAdmin']
const HIVE_MANAGERS    = ['Admin', 'OrgAdmin', 'SystemAdmin']

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public route */}
          <Route path="/login" element={<LoginPage />} />

          {/* Protected routes — redirect to /login if not authenticated */}
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Layout />}>
              <Route index element={<SmartRedirect />} />

              {/* Apiary list + detail — all authenticated users */}
              <Route path="apiaries"  element={<ApiaryListPage />} />
              <Route path="apiaries/:id" element={<ApiaryDetailPage />} />

              {/* Apiary create/edit — OrgAdmin and SystemAdmin only */}
              <Route element={<RoleRoute allowedRoles={APIARY_MANAGERS} />}>
                <Route path="apiaries/new"      element={<ApiaryFormPage />} />
                <Route path="apiaries/:id/edit" element={<ApiaryFormPage />} />
              </Route>

              {/* Beehive detail — all authenticated users */}
              <Route path="beehives/:id" element={<BeehiveDetailPage />} />

              {/* Beehive create/edit — Admin, OrgAdmin, SystemAdmin */}
              <Route element={<RoleRoute allowedRoles={HIVE_MANAGERS} />}>
                <Route path="beehives/new"      element={<BeehiveFormPage />} />
                <Route path="beehives/:id/edit" element={<BeehiveFormPage />} />
              </Route>

              {/* Inspection create — all authenticated users (all roles can manage inspections) */}
              <Route path="inspections/new" element={<InspectionFormPage />} />

              {/* Inspection edit — Admin, OrgAdmin, SystemAdmin */}
              <Route element={<RoleRoute allowedRoles={HIVE_MANAGERS} />}>
                <Route path="inspections/:id/edit" element={<InspectionFormPage />} />
              </Route>

              {/* Diet detail — all authenticated users (User role can view) */}
              <Route path="diets/:id" element={<DietDetailPage />} />

              {/* Diet create/edit — Admin, OrgAdmin, SystemAdmin */}
              <Route element={<RoleRoute allowedRoles={HIVE_MANAGERS} />}>
                <Route path="diets/new"      element={<DietFormPage />} />
                <Route path="diets/:id/edit" element={<DietFormPage />} />
              </Route>

              {/* Admin routes — SystemAdmin only */}
              <Route element={<AdminRoute />}>
                <Route path="admin"                        element={<AdminDashboardPage />} />
                <Route path="admin/organizations/new"      element={<OrganizationFormPage />} />
                <Route path="admin/organizations/:id/edit" element={<OrganizationFormPage />} />
                <Route path="admin/users/new"              element={<UserFormPage />} />
                <Route path="admin/users/:id/edit"         element={<UserFormPage />} />
              </Route>
            </Route>
          </Route>

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
