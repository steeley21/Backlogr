// /services/api.ts
import axios, { type AxiosError, type AxiosInstance } from 'axios'

export const AUTH_STORAGE_KEY = 'backlogr.auth'

interface StoredAuthState {
  token?: string
  expiresAtUtc?: string
  user?: unknown
  roles?: string[]
}

let apiInstance: AxiosInstance | null = null

function normalizeBaseUrl(url: string): string {
  return url.replace(/\/+$/, '')
}

function readStoredAuth(): StoredAuthState | null {
  if (!process.client) {
    return null
  }

  const raw = localStorage.getItem(AUTH_STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as StoredAuthState
  }
  catch {
    localStorage.removeItem(AUTH_STORAGE_KEY)
    return null
  }
}

function getStoredToken(): string | null {
  const auth = readStoredAuth()
  return auth?.token?.trim() || null
}

export function clearStoredAuth(): void {
  if (!process.client) {
    return
  }

  localStorage.removeItem(AUTH_STORAGE_KEY)
}

function redirectToLogin(): void {
  if (!process.client) {
    return
  }

  const currentPath = `${window.location.pathname}${window.location.search}${window.location.hash}`
  const isAuthPage = window.location.pathname === '/login' || window.location.pathname === '/register'

  if (isAuthPage) {
    return
  }

  const redirect = encodeURIComponent(currentPath || '/')
  window.location.assign(`/login?redirect=${redirect}`)
}

export function useApi(): AxiosInstance {
  if (apiInstance) {
    return apiInstance
  }

  const config = useRuntimeConfig()

  apiInstance = axios.create({
    baseURL: normalizeBaseUrl(config.public.apiBase),
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
  })

  apiInstance.interceptors.request.use((requestConfig) => {
    const token = getStoredToken()

    if (token) {
      requestConfig.headers.Authorization = `Bearer ${token}`
    }

    return requestConfig
  })

  apiInstance.interceptors.response.use(
    response => response,
    (error: AxiosError) => {
      if (error.response?.status === 401) {
        clearStoredAuth()
        redirectToLogin()
      }

      return Promise.reject(error)
    },
  )

  return apiInstance
}