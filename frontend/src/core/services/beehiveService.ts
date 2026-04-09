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
