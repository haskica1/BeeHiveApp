import axios from 'axios'
import { authService } from './authService'

/**
 * Pre-configured Axios instance for BeeHive API calls.
 * The base URL is handled by the Vite dev-proxy during development,
 * and replaced with the real API URL in production via the VITE_API_URL env var.
 */
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10_000,
})

// Attach JWT token from localStorage on every request
apiClient.interceptors.request.use((config) => {
  const token = authService.getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Normalise errors; redirect to /login on 401
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      authService.logout()
      window.location.href = '/login'
    }

    const message =
      error.response?.data?.title ??
      error.response?.data?.message ??
      error.message ??
      'An unexpected error occurred'

    return Promise.reject(new Error(message))
  },
)

export default apiClient
