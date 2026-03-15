import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getPublicLibrary, getPublicProfile } from '~/services/profileService'

const api = {
  get: vi.fn(),
}

vi.mock('~/services/api', async (importOriginal) => {
  const actual = await importOriginal<typeof import('~/services/api')>()

  return {
    ...actual,
    useApi: () => api,
  }
})

describe('profileService', () => {
  beforeEach(() => {
    api.get.mockReset()
  })

  it('loads a public profile using an encoded username', async () => {
    api.get.mockResolvedValue({ data: { userName: 'kate steele' } })

    const result = await getPublicProfile('kate steele')

    expect(api.get).toHaveBeenCalledWith('/api/Profiles/kate%20steele')
    expect(result).toEqual({ userName: 'kate steele' })
  })

  it('loads a public library using an encoded username', async () => {
    api.get.mockResolvedValue({ data: [{ gameLogId: 'log-1' }] })

    const result = await getPublicLibrary('kate steele')

    expect(api.get).toHaveBeenCalledWith('/api/Profiles/kate%20steele/library')
    expect(result).toEqual([{ gameLogId: 'log-1' }])
  })
})
