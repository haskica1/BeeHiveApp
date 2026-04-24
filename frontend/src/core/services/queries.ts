import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiaryService } from '../services/apiaryService'
import { beehiveService, inspectionService } from '../services/beehiveService'
import { todoService } from '../services/todoService'
import { dietService } from '../services/dietService'
import type {
  CreateApiaryPayload,
  UpdateApiaryPayload,
  CreateBeehivePayload,
  UpdateBeehivePayload,
  CreateInspectionPayload,
  UpdateInspectionPayload,
  CreateTodoPayload,
  UpdateTodoPayload,
  CreateDietPayload,
  UpdateDietPayload,
  CompleteEarlyPayload,
} from '../models'

// ── Query Keys ────────────────────────────────────────────────────────────────

export const queryKeys = {
  apiaries:           ['apiaries'] as const,
  apiary:             (id: number) => ['apiaries', id] as const,
  apiaryWeather:      (id: number) => ['apiaries', id, 'weather'] as const,
  beehivesByApiary:   (apiaryId: number) => ['beehives', 'apiary', apiaryId] as const,
  beehive:            (id: number) => ['beehives', id] as const,
  inspectionsByHive:  (beehiveId: number) => ['inspections', 'beehive', beehiveId] as const,
  inspection:         (id: number) => ['inspections', id] as const,
  todosByApiary:      (apiaryId: number) => ['todos', 'apiary', apiaryId] as const,
  todosByBeehive:     (beehiveId: number) => ['todos', 'beehive', beehiveId] as const,
  assignableUsers:    ['todos', 'assignable-users'] as const,
  dietsByBeehive:     (beehiveId: number) => ['diets', 'beehive', beehiveId] as const,
  diet:               (id: number) => ['diets', id] as const,
}

// ── Apiary Hooks ──────────────────────────────────────────────────────────────

export const useApiaries = () =>
  useQuery({ queryKey: queryKeys.apiaries, queryFn: apiaryService.getAll })

export const useApiary = (id: number) =>
  useQuery({ queryKey: queryKeys.apiary(id), queryFn: () => apiaryService.getById(id), enabled: !!id })

export const useApiaryWeather = (id: number, hasLocation: boolean) =>
  useQuery({
    queryKey: queryKeys.apiaryWeather(id),
    queryFn: () => apiaryService.getWeather(id),
    enabled: !!id && hasLocation,
    staleTime: 1000 * 60 * 30, // weather data stays fresh for 30 minutes
  })

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

// ── Todo Hooks ────────────────────────────────────────────────────────────────

export const useAssignableUsers = () =>
  useQuery({
    queryKey: queryKeys.assignableUsers,
    queryFn: todoService.getAssignableUsers,
    staleTime: 1000 * 60 * 5,
  })

export const useTodosByApiary = (apiaryId: number) =>
  useQuery({
    queryKey: queryKeys.todosByApiary(apiaryId),
    queryFn: () => todoService.getByApiary(apiaryId),
    enabled: !!apiaryId,
  })

export const useTodosByBeehive = (beehiveId: number) =>
  useQuery({
    queryKey: queryKeys.todosByBeehive(beehiveId),
    queryFn: () => todoService.getByBeehive(beehiveId),
    enabled: !!beehiveId,
  })

export const useCreateTodo = (invalidateKey: readonly unknown[]) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateTodoPayload) => todoService.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: invalidateKey }),
  })
}

export const useUpdateTodo = (invalidateKey: readonly unknown[]) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: UpdateTodoPayload }) =>
      todoService.update(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: invalidateKey }),
  })
}

export const useDeleteTodo = (invalidateKey: readonly unknown[]) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => todoService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: invalidateKey }),
  })
}

// ── Diet Hooks ────────────────────────────────────────────────────────────────

export const useDietsByBeehive = (beehiveId: number) =>
  useQuery({
    queryKey: queryKeys.dietsByBeehive(beehiveId),
    queryFn:  () => dietService.getByBeehive(beehiveId),
    enabled:  !!beehiveId,
  })

export const useDiet = (id: number) =>
  useQuery({
    queryKey: queryKeys.diet(id),
    queryFn:  () => dietService.getById(id),
    enabled:  !!id,
  })

export const useCreateDiet = (beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateDietPayload) => dietService.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.dietsByBeehive(beehiveId) }),
  })
}

export const useUpdateDiet = (id: number, beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpdateDietPayload) => dietService.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.diet(id) })
      qc.invalidateQueries({ queryKey: queryKeys.dietsByBeehive(beehiveId) })
    },
  })
}

export const useDeleteDiet = (beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => dietService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.dietsByBeehive(beehiveId) }),
  })
}

export const useCompleteEarlyDiet = (id: number, beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CompleteEarlyPayload) => dietService.completeEarly(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.diet(id) })
      qc.invalidateQueries({ queryKey: queryKeys.dietsByBeehive(beehiveId) })
    },
  })
}

export const useCompleteFeedingEntry = (dietId: number, beehiveId: number) => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (entryId: number) => dietService.completeFeedingEntry(dietId, entryId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.diet(dietId) })
      qc.invalidateQueries({ queryKey: queryKeys.dietsByBeehive(beehiveId) })
    },
  })
}
