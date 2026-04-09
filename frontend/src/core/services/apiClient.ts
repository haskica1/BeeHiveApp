import axios from 'axios'

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

// Response interceptor — normalise errors into a consistent shape
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const message =
      error.response?.data?.title ??
      error.response?.data?.message ??
      error.message ??
      'An unexpected error occurred'

    return Promise.reject(new Error(message))
  },
)

export default apiClient
