import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './core/context/AuthContext'
import Layout from './shared/components/Layout'
import ProtectedRoute from './shared/components/ProtectedRoute'
import LoginPage from './features/auth/LoginPage'
import ApiaryListPage from './features/apiaries/ApiaryListPage'
import ApiaryDetailPage from './features/apiaries/ApiaryDetailPage'
import ApiaryFormPage from './features/apiaries/ApiaryFormPage'
import BeehiveDetailPage from './features/beehives/BeehiveDetailPage'
import BeehiveFormPage from './features/beehives/BeehiveFormPage'
import InspectionFormPage from './features/inspections/InspectionFormPage'
import DietFormPage from './features/diets/DietFormPage'
import DietDetailPage from './features/diets/DietDetailPage'

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
              <Route index element={<Navigate to="/apiaries" replace />} />

              {/* Apiary routes */}
              <Route path="apiaries"             element={<ApiaryListPage />} />
              <Route path="apiaries/new"         element={<ApiaryFormPage />} />
              <Route path="apiaries/:id"         element={<ApiaryDetailPage />} />
              <Route path="apiaries/:id/edit"    element={<ApiaryFormPage />} />

              {/* Beehive routes */}
              <Route path="beehives/new"          element={<BeehiveFormPage />} />
              <Route path="beehives/:id"          element={<BeehiveDetailPage />} />
              <Route path="beehives/:id/edit"     element={<BeehiveFormPage />} />

              {/* Inspection routes */}
              <Route path="inspections/new"       element={<InspectionFormPage />} />
              <Route path="inspections/:id/edit"  element={<InspectionFormPage />} />

              {/* Diet routes */}
              <Route path="diets/new"             element={<DietFormPage />} />
              <Route path="diets/:id"             element={<DietDetailPage />} />
              <Route path="diets/:id/edit"        element={<DietFormPage />} />
            </Route>
          </Route>

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
