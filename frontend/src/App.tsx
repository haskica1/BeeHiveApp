import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import Layout from './shared/components/Layout'
import ApiaryListPage from './features/apiaries/ApiaryListPage'
import ApiaryDetailPage from './features/apiaries/ApiaryDetailPage'
import ApiaryFormPage from './features/apiaries/ApiaryFormPage'
import BeehiveDetailPage from './features/beehives/BeehiveDetailPage'
import BeehiveFormPage from './features/beehives/BeehiveFormPage'
import InspectionFormPage from './features/inspections/InspectionFormPage'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          {/* Redirect root to apiaries */}
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
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
