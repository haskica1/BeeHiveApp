import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiaryService } from '../services/apiaryService'
import { beehiveService, inspectionService } from '../services/beehiveService'
import type {
  CreateApiaryPayload,
  UpdateApiaryPayload,
  CreateBeehivePayload,
  UpdateBeehivePayload,
  CreateInspectionPayload,
  UpdateInspectionPayload,
} from '../models'

// ── Query Keys ────────────────────────────────────────────────────────────────

export const queryKeys = {
  apiaries:          ['apiaries'] as const,
  apiary:            (id: number) => ['apiaries', id] as const,
  beehivesByApiary:  (apiaryId: number) => ['beehives', 'apiary', apiaryId] as const,
  beehive:           (id: number) => ['beehives', id] as const,
  inspectionsByHive: (beehiveId: number) => ['inspections', 'beehive', beehiveId] as const,
  inspection:        (id: number) => ['inspections', id] as const,
}

// ── Apiary Hooks ──────────────────────────────────────────────────────────────

export const useApiaries = () =>
  useQuery({ queryKey: queryKeys.apiaries, queryFn: apiaryService.getAll })

export const useApiary = (id: number) =>
  useQuery({ queryKey: queryKeys.apiary(id), queryFn: () => apiaryService.getById(id), enabled: !!id })

export const useCreateApiary = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateApiaryPayload) => apiaryService.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.apiaries }),
  })
}

export const useUpdateApiary = (id: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpdateApiaryPayload) => apiaryService.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.apiaries })
      qc.invalidateQueries({ queryKey: queryKeys.apiary(id) })
    },
  })
}

export const useDeleteApiary = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => apiaryService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.apiaries }),
  })
}

// ── Beehive Hooks ─────────────────────────────────────────────────────────────

export const useBeehive = (id: number) =>
  useQuery({ queryKey: queryKeys.beehive(id), queryFn: () => beehiveService.getById(id), enabled: !!id })

export const useCreateBeehive = (apiaryId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateBeehivePayload) => beehiveService.create(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.apiary(apiaryId) })
      qc.invalidateQueries({ queryKey: queryKeys.beehivesByApiary(apiaryId) })
    },
  })
}

export const useUpdateBeehive = (id: number, apiaryId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpdateBeehivePayload) => beehiveService.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.beehive(id) })
      qc.invalidateQueries({ queryKey: queryKeys.apiary(apiaryId) })
    },
  })
}

export const useDeleteBeehive = (apiaryId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => beehiveService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.apiary(apiaryId) })
    },
  })
}

// ── Inspection Hooks ──────────────────────────────────────────────────────────

export const useInspectionsByBeehive = (beehiveId: number) =>
  useQuery({
    queryKey: queryKeys.inspectionsByHive(beehiveId),
    queryFn: () => inspectionService.getByBeehive(beehiveId),
    enabled: !!beehiveId,
  })

export const useCreateInspection = (beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateInspectionPayload) => inspectionService.create(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.beehive(beehiveId) })
      qc.invalidateQueries({ queryKey: queryKeys.inspectionsByHive(beehiveId) })
    },
  })
}

export const useUpdateInspection = (id: number, beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpdateInspectionPayload) => inspectionService.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.inspectionsByHive(beehiveId) })
      qc.invalidateQueries({ queryKey: queryKeys.beehive(beehiveId) })
    },
  })
}

export const useDeleteInspection = (beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => inspectionService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.inspectionsByHive(beehiveId) })
      qc.invalidateQueries({ queryKey: queryKeys.beehive(beehiveId) })
    },
  })
}
