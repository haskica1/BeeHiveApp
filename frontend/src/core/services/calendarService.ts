import apiClient from './apiClient'
import type { CalendarEventsResponse } from '../models'

export const calendarService = {
  getEvents: (): Promise<CalendarEventsResponse> =>
    apiClient.get<CalendarEventsResponse>('/calendar/events').then(r => r.data),
}
