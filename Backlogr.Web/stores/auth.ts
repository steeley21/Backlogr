// /stores/auth.ts
import { defineStore } from 'pinia'
import { AUTH_STORAGE_KEY, clearStoredAuth } from '~/services/api'
import { getMe, login as loginRequest, register as registerRequest } from '~/services/authService'
import type {
  AuthResponseDto,
  LoginRequestDto,
  MeResponseDto,
  RegisterRequestDto,
} from '~/types/auth'

interface StoredAuthState {
  token: string
  expiresAtUtc: string
  user: MeResponseDto | null
  roles: string[]
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

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const expiresAtUtc = ref<string | null>(null)
  const user = ref<MeResponseDto | null>(null)
  const roles = ref<string[]>([])
  const isInitialized = ref(false)

  const isAuthenticated = computed(() => {
    if (!token.value || !expiresAtUtc.value) {
      return false
    }

    return new Date(expiresAtUtc.value).getTime() > Date.now()
  })

  const displayName = computed(() => user.value?.displayName ?? '')
  const userName = computed(() => user.value?.userName ?? '')
  const avatarUrl = computed(() => user.value?.avatarUrl ?? null)

  function persist(): void {
    if (!process.client) {
      return
    }

    if (!token.value || !expiresAtUtc.value) {
      clearStoredAuth()
      return
    }

    const payload: StoredAuthState = {
      token: token.value,
      expiresAtUtc: expiresAtUtc.value,
      user: user.value,
      roles: roles.value,
    }

    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(payload))
  }

  function applyAuthResponse(response: AuthResponseDto): void {
    token.value = response.token
    expiresAtUtc.value = response.expiresAtUtc
    roles.value = [...response.roles]
    user.value = {
      userId: response.userId,
      userName: response.userName,
      displayName: response.displayName,
      email: response.email,
      avatarUrl: null,
      bio: null,
      roles: [...response.roles],
    }

    persist()
  }

  function clearState(): void {
    token.value = null
    expiresAtUtc.value = null
    user.value = null
    roles.value = []
    clearStoredAuth()
  }

  async function hydrate(): Promise<void> {
    if (isInitialized.value) {
      return
    }

    const stored = readStoredAuth()

    if (!stored?.token || !stored.expiresAtUtc) {
      clearState()
      isInitialized.value = true
      return
    }

    token.value = stored.token
    expiresAtUtc.value = stored.expiresAtUtc
    user.value = stored.user
    roles.value = stored.roles ?? []

    if (!isAuthenticated.value) {
      clearState()
      isInitialized.value = true
      return
    }

    if (!user.value) {
      try {
        await fetchMe()
      }
      catch {
        clearState()
      }
    }

    isInitialized.value = true
  }

  async function login(payload: LoginRequestDto): Promise<void> {
    const response = await loginRequest(payload)
    applyAuthResponse(response)

    try {
      await fetchMe()
    }
    catch {
      // auth response already contains enough to treat the user as signed in
    }

    isInitialized.value = true
  }

  async function register(payload: RegisterRequestDto): Promise<void> {
    const response = await registerRequest(payload)
    applyAuthResponse(response)

    try {
      await fetchMe()
    }
    catch {
      // auth response already contains enough to treat the user as signed in
    }

    isInitialized.value = true
  }

  async function fetchMe(): Promise<void> {
    const profile = await getMe()
    user.value = profile
    roles.value = [...profile.roles]
    persist()
  }

  function logout(): void {
    clearState()
    isInitialized.value = true
  }

  function isInRole(role: string): boolean {
    return roles.value.includes(role)
  }

  return {
    token,
    expiresAtUtc,
    user,
    roles,
    isInitialized,
    isAuthenticated,
    displayName,
    userName,
    avatarUrl,
    hydrate,
    login,
    register,
    fetchMe,
    logout,
    isInRole,
  }
})