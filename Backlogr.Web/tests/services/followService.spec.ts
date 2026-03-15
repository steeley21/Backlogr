import { beforeEach, describe, expect, it, vi } from 'vitest'
import { followUser, unfollowUser } from '~/services/followService'

const api = {
  post: vi.fn(),
  delete: vi.fn(),
}

vi.mock('~/services/api', async (importOriginal) => {
  const actual = await importOriginal<typeof import('~/services/api')>()

  return {
    ...actual,
    useApi: () => api,
  }
})

describe('followService', () => {
  beforeEach(() => {
    api.post.mockReset()
    api.delete.mockReset()
  })

  it('follows and unfollows a user by id', async () => {
    api.post.mockResolvedValue({})
    api.delete.mockResolvedValue({})

    await followUser('user-1')
    await unfollowUser('user-1')

    expect(api.post).toHaveBeenCalledWith('/api/Follows/user-1')
    expect(api.delete).toHaveBeenCalledWith('/api/Follows/user-1')
  })
})
