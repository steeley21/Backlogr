import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  getGameActivity,
  getGameReviews,
  getGameViewerState,
} from '~/services/gameService'

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

describe('gameService', () => {
  beforeEach(() => {
    api.get.mockReset()
  })

  it('loads the current viewer state for a game', async () => {
    api.get.mockResolvedValue({
      data: {
        log: {
          gameLogId: 'log-1',
          gameId: 'game-1',
          gameTitle: 'Mass Effect',
          coverImageUrl: 'cover.jpg',
          status: 'Played',
          rating: 4.5,
          platform: 'PC',
          hours: 40,
          startedAt: '2026-03-01T00:00:00Z',
          finishedAt: '2026-03-10T00:00:00Z',
          notes: 'Loved the party banter.',
          updatedAt: '2026-03-10T12:00:00Z',
        },
        review: {
          reviewId: 'review-1',
          userId: 'user-1',
          userName: 'kate',
          displayName: 'Kate Steele',
          gameId: 'game-1',
          gameTitle: 'Mass Effect',
          text: 'Still one of my favorite RPGs.',
          hasSpoilers: false,
          createdAt: '2026-03-10T12:00:00Z',
          updatedAt: '2026-03-11T12:00:00Z',
        },
      },
    })

    const result = await getGameViewerState('game-1')

    expect(api.get).toHaveBeenCalledWith('/api/Games/game-1/me')
    expect(result.log?.status).toBe('Played')
    expect(result.review?.reviewId).toBe('review-1')
  })

  it('maps game review items and filters out non-review entries', async () => {
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
          coverImageUrl: 'cover.jpg',
          gameLogId: null,
          reviewId: 'review-1',
          status: null,
          rating: 4.5,
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
          gameId: 'game-1',
          gameTitle: 'Mass Effect',
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

    const result = await getGameReviews('game-1', 10)

    expect(api.get).toHaveBeenCalledWith('/api/Games/game-1/reviews', {
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
          coverUrl: 'cover.jpg',
        },
        rating: 4.5,
        reviewedAt: '2026-03-14T12:00:00Z',
        text: 'Amazing sci-fi RPG.',
        hasSpoilers: true,
        likeCount: 8,
        commentCount: 3,
        liked: true,
        isOwner: true,
      },
    ])
  })

  it('maps mixed game activity items', async () => {
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
          coverImageUrl: 'cover.jpg',
          gameLogId: null,
          reviewId: 'review-1',
          status: null,
          rating: 4.5,
          platform: null,
          hours: null,
          reviewText: 'Amazing sci-fi RPG.',
          hasSpoilers: false,
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
          gameId: 'game-1',
          gameTitle: 'Mass Effect',
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

    const result = await getGameActivity('game-1', 25)

    expect(api.get).toHaveBeenCalledWith('/api/Games/game-1/activity', {
      params: { take: 25 },
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
          coverUrl: 'cover.jpg',
        },
        rating: 4.5,
        reviewedAt: '2026-03-14T12:00:00Z',
        text: 'Amazing sci-fi RPG.',
        hasSpoilers: false,
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
          gameId: 'game-1',
          title: 'Mass Effect',
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
