import apiClient from './apiClient'
import type { Todo, CreateTodoPayload, UpdateTodoPayload, AssignableUser } from '../models'

export const todoService = {
  getByApiary: async (apiaryId: number): Promise<Todo[]> => {
    const res = await apiClient.get<Todo[]>(`/todos/by-apiary/${apiaryId}`)
    return res.data
  },

  getByBeehive: async (beehiveId: number): Promise<Todo[]> => {
    const res = await apiClient.get<Todo[]>(`/todos/by-beehive/${beehiveId}`)
    return res.data
  },

  create: async (payload: CreateTodoPayload): Promise<Todo> => {
    const res = await apiClient.post<Todo>('/todos', payload)
    return res.data
  },

  update: async (id: number, payload: UpdateTodoPayload): Promise<Todo> => {
    const res = await apiClient.put<Todo>(`/todos/${id}`, payload)
    return res.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/todos/${id}`)
  },

  getAssignableUsers: async (): Promise<AssignableUser[]> => {
    const res = await apiClient.get<AssignableUser[]>('/todos/assignable-users')
    return res.data
  },
}
