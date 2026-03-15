import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getFeed } from '~/services/feedService'

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

describe('feedService', () => {
  beforeEach(() => {
    api.get.mockReset()
  })

  it('maps review and log feed items from the API response', async () => {
    api.get.mockResolvedValue({
      data: [
        {
          itemType: 'Review',
          activityAt: '2026-03-14T12:00:00Z',
          userId: 'user-1',
          userName: 'kate',
          displayName: 'Kate Steele',
          avatarUrl: null,
          gameId: 'game-1',
          gameTitle: 'Mass Effect',
          coverImageUrl: 'cover-1.jpg',
          gameLogId: null,
          reviewId: 'review-1',
          status: null,
          rating: 4,
          platform: null,
          hours: null,
          reviewText: 'Amazing sci-fi RPG.',
          hasSpoilers: true,
          likeCount: 8,
          commentCount: 3,
          likedByCurrentUser: true,
          isOwner: true,
        },
        {
          itemType: 'GameLog',
          activityAt: '2026-03-13T08:00:00Z',
          userId: 'user-2',
          userName: 'sam',
          displayName: 'Sam Carter',
          avatarUrl: 'avatar.jpg',
          gameId: 'game-2',
          gameTitle: 'Halo Reach',
          coverImageUrl: null,
          gameLogId: 'log-1',
          reviewId: null,
          status: 'Playing',
          rating: 5,
          platform: 'PC',
          hours: 12,
          reviewText: null,
          hasSpoilers: null,
          likeCount: 0,
          commentCount: 0,
          likedByCurrentUser: false,
          isOwner: false,
        },
      ],
    })

    const result = await getFeed(10)

    expect(api.get).toHaveBeenCalledWith('/api/Feed', {
      params: { take: 10 },
    })
    expect(result).toEqual([
      {
        type: 'review',
        id: 'review-1',
        user: {
          userId: 'user-1',
          userName: 'kate',
          displayName: 'Kate Steele',
          avatarUrl: undefined,
        },
        game: {
          gameId: 'game-1',
          title: 'Mass Effect',
          coverUrl: 'cover-1.jpg',
        },
        rating: 4,
        reviewedAt: '2026-03-14T12:00:00Z',
        text: 'Amazing sci-fi RPG.',
        hasSpoilers: true,
        likeCount: 8,
        commentCount: 3,
        liked: true,
        isOwner: true,
      },
      {
        type: 'log',
        id: 'log-1',
        user: {
          userId: 'user-2',
          userName: 'sam',
          displayName: 'Sam Carter',
          avatarUrl: 'avatar.jpg',
        },
        game: {
          gameId: 'game-2',
          title: 'Halo Reach',
          coverUrl: undefined,
        },
        status: 'Playing',
        rating: 5,
        platform: 'PC',
        hours: 12,
        updatedAt: '2026-03-13T08:00:00Z',
      },
    ])
  })
})
