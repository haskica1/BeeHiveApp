import apiClient from './apiClient'
import type {
  Diet,
  DietDetail,
  CreateDietPayload,
  UpdateDietPayload,
  CompleteEarlyPayload,
} from '../models'

export const dietService = {
  getByBeehive: async (beehiveId: number): Promise<Diet[]> => {
    const res = await apiClient.get<Diet[]>(`/diets/by-beehive/${beehiveId}`)
    return res.data
  },

  getById: async (id: number): Promise<DietDetail> => {
    const res = await apiClient.get<DietDetail>(`/diets/${id}`)
    return res.data
  },

  create: async (payload: CreateDietPayload): Promise<DietDetail> => {
    const res = await apiClient.post<DietDetail>('/diets', payload)
    return res.data
  },

  update: async (id: number, payload: UpdateDietPayload): Promise<DietDetail> => {
    const res = await apiClient.put<DietDetail>(`/diets/${id}`, payload)
    return res.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/diets/${id}`)
  },

  completeEarly: async (id: number, payload: CompleteEarlyPayload): Promise<DietDetail> => {
    const res = await apiClient.post<DietDetail>(`/diets/${id}/complete-early`, payload)
    return res.data
  },

  completeFeedingEntry: async (dietId: number, entryId: number): Promise<DietDetail> => {
    const res = await apiClient.post<DietDetail>(
      `/diets/${dietId}/feeding-entries/${entryId}/complete`,
    )
    return res.data
  },
}
