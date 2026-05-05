import axios from 'axios'
import apiClient from './apiClient'
import type {
  Beehive,
  BeehiveDetail,
  CreateBeehivePayload,
  UpdateBeehivePayload,
  Inspection,
  CreateInspectionPayload,
  UpdateInspectionPayload,
} from '../models'

export interface BeehiveScanInfo {
  id: number
  name: string
  apiaryId: number
}

// Raw axios instance for unauthenticated calls (no auth redirect interceptor)
const publicClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 10_000,
})

// ── Beehive Service ───────────────────────────────────────────────────────────

export const beehiveService = {
  getByApiary: async (apiaryId: number): Promise<Beehive[]> => {
    const res = await apiClient.get<Beehive[]>(`/beehives/by-apiary/${apiaryId}`)
    return res.data
  },

  getById: async (id: number): Promise<BeehiveDetail> => {
    const res = await apiClient.get<BeehiveDetail>(`/beehives/${id}`)
    return res.data
  },

  create: async (payload: CreateBeehivePayload): Promise<Beehive> => {
    const res = await apiClient.post<Beehive>('/beehives', payload)
    return res.data
  },

  update: async (id: number, payload: UpdateBeehivePayload): Promise<Beehive> => {
    const res = await apiClient.put<Beehive>(`/beehives/${id}`, payload)
    return res.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/beehives/${id}`)
  },

  /** Public — no auth required. Resolves a scan uniqueId to {id, name, apiaryId}. Returns null if not found. */
  scanLookup: async (uniqueId: string): Promise<BeehiveScanInfo | null> => {
    try {
      const res = await publicClient.get<BeehiveScanInfo>(`/beehives/scan/${uniqueId}`)
      return res.data
    } catch (err: any) {
      if (err.response?.status === 404) return null
      throw err
    }
  },

  /** Authenticated — asks the backend whether the current user can access this beehive. */
  checkAccess: async (id: number): Promise<boolean> => {
    const res = await apiClient.get<{ hasAccess: boolean }>(`/beehives/${id}/has-access`)
    return res.data.hasAccess
  },
}

// ── Inspection Service ────────────────────────────────────────────────────────

export const inspectionService = {
  getByBeehive: async (beehiveId: number): Promise<Inspection[]> => {
    const res = await apiClient.get<Inspection[]>(`/inspections/by-beehive/${beehiveId}`)
    return res.data
  },

  getById: async (id: number): Promise<Inspection> => {
    const res = await apiClient.get<Inspection>(`/inspections/${id}`)
    return res.data
  },

  create: async (payload: CreateInspectionPayload): Promise<Inspection> => {
    const res = await apiClient.post<Inspection>('/inspections', payload)
    return res.data
  },

  update: async (id: number, payload: UpdateInspectionPayload): Promise<Inspection> => {
    const res = await apiClient.put<Inspection>(`/inspections/${id}`, payload)
    return res.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/inspections/${id}`)
  },
}
