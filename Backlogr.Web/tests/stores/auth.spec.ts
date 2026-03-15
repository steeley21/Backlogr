import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useAuthStore } from '~/stores/auth'
import type { AuthResponseDto, MeResponseDto } from '~/types/auth'

const authMocks = vi.hoisted(() => ({
  clearStoredAuth: vi.fn(() => {
    localStorage.removeItem('backlogr.auth')
  }),
  loginRequest: vi.fn<(...args: unknown[]) => Promise<AuthResponseDto>>(),
  registerRequest: vi.fn<(...args: unknown[]) => Promise<AuthResponseDto>>(),
  getMe: vi.fn<() => Promise<MeResponseDto>>(),
}))

vi.mock('~/services/api', () => ({
  AUTH_STORAGE_KEY: 'backlogr.auth',
  clearStoredAuth: authMocks.clearStoredAuth,
}))

vi.mock('~/services/authService', () => ({
  login: authMocks.loginRequest,
  register: authMocks.registerRequest,
  getMe: authMocks.getMe,
}))

const authResponse: AuthResponseDto = {
  token: 'token-123',
  expiresAtUtc: '2099-01-01T00:00:00Z',
  userId: 'user-1',
  userName: 'kate',
  displayName: 'Kate Steele',
  email: 'kate@example.com',
  roles: ['User'],
}

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    authMocks.clearStoredAuth.mockClear()
    authMocks.loginRequest.mockReset()
    authMocks.registerRequest.mockReset()
    authMocks.getMe.mockReset()
  })

  it('logs in and persists auth state', async () => {
    authMocks.loginRequest.mockResolvedValue(authResponse)

    const store = useAuthStore()
    await store.login({ emailOrUserName: 'kate', password: 'Horse1234!' })

    expect(store.isAuthenticated).toBe(true)
    expect(store.userName).toBe('kate')
    expect(store.displayName).toBe('Kate Steele')
    expect(JSON.parse(localStorage.getItem('backlogr.auth') || '{}')).toMatchObject({
      token: 'token-123',
      user: {
        userName: 'kate',
        displayName: 'Kate Steele',
      },
    })
  })

  it('hydrates from storage and fetches the user when needed', async () => {
    localStorage.setItem('backlogr.auth', JSON.stringify({
      token: 'token-123',
      expiresAtUtc: '2099-01-01T00:00:00Z',
      user: null,
      roles: ['User'],
    }))

    authMocks.getMe.mockResolvedValue({
      userId: 'user-1',
      userName: 'kate',
      displayName: 'Kate Steele',
      email: 'kate@example.com',
      avatarUrl: null,
      bio: 'Collector of RPGs',
      roles: ['User'],
    })

    const store = useAuthStore()
    await store.hydrate()

    expect(authMocks.getMe).toHaveBeenCalledTimes(1)
    expect(store.isInitialized).toBe(true)
    expect(store.user?.bio).toBe('Collector of RPGs')
    expect(store.roles).toEqual(['User'])
  })

  it('logs out and clears persisted auth state', async () => {
    authMocks.loginRequest.mockResolvedValue(authResponse)

    const store = useAuthStore()
    await store.login({ emailOrUserName: 'kate', password: 'Horse1234!' })
    store.logout()

    expect(store.isAuthenticated).toBe(false)
    expect(store.user).toBeNull()
    expect(authMocks.clearStoredAuth).toHaveBeenCalledTimes(1)
    expect(localStorage.getItem('backlogr.auth')).toBeNull()
  })
})
