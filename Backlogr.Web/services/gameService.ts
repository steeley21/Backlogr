import { useApi } from '~/services/api'
import type {
  FeedItem,
  FeedItemResponseDto,
  FeedLogItem,
  FeedReviewItem,
} from '~/types/feed'
import type {
  GameBrowseResultDto,
  GameDetailResponseDto,
  GameSummaryResponseDto,
  GameViewerStateResponseDto,
  GetGamesParams,
  ImportedGameResponseDto,
} from '~/types/game'

export async function getGames(params: GetGamesParams = {}): Promise<GameSummaryResponseDto[]> {
  const api = useApi()
  const response = await api.get<GameSummaryResponseDto[]>('/api/Games', {
    params: {
      query: params.query?.trim() || undefined,
      take: params.take ?? 25,
    },
  })

  return response.data
}

export async function searchBrowseGames(params: GetGamesParams = {}): Promise<GameBrowseResultDto[]> {
  const api = useApi()
  const response = await api.get<GameBrowseResultDto[]>('/api/Games/search', {
    params: {
      query: params.query?.trim() || undefined,
      take: params.take ?? 25,
    },
  })

  return response.data
}

export async function importGameFromIgdb(igdbId: number): Promise<ImportedGameResponseDto> {
  const api = useApi()
  const response = await api.post<ImportedGameResponseDto>(`/api/Igdb/import/${igdbId}`)
  return response.data
}

export async function getGameById(gameId: string): Promise<GameDetailResponseDto> {
  const api = useApi()
  const response = await api.get<GameDetailResponseDto>(`/api/Games/${gameId}`)
  return response.data
}

export async function getGameViewerState(gameId: string): Promise<GameViewerStateResponseDto> {
  const api = useApi()
  const response = await api.get<GameViewerStateResponseDto>(`/api/Games/${gameId}/me`)
  return response.data
}

export async function getGameReviews(gameId: string, take = 20): Promise<FeedReviewItem[]> {
  const api = useApi()
  const response = await api.get<FeedItemResponseDto[]>(`/api/Games/${gameId}/reviews`, {
    params: { take },
  })

  return response.data
    .map(mapFeedItem)
    .filter((item): item is FeedReviewItem => item.type === 'review')
}

export async function getGameActivity(gameId: string, take = 25): Promise<FeedItem[]> {
  const api = useApi()
  const response = await api.get<FeedItemResponseDto[]>(`/api/Games/${gameId}/activity`, {
    params: { take },
  })

  return response.data.map(mapFeedItem)
}

function mapFeedItem(item: FeedItemResponseDto): FeedItem {
  if (item.itemType === 'Review') {
    const reviewItem: FeedReviewItem = {
      type: 'review',
      id: item.reviewId ?? `review-${item.gameId}-${item.activityAt}`,
      user: {
        userId: item.userId,
        userName: item.userName,
        displayName: item.displayName,
        avatarUrl: item.avatarUrl ?? undefined,
      },
      game: {
        gameId: item.gameId,
        title: item.gameTitle,
        coverUrl: item.coverImageUrl ?? undefined,
      },
      rating: item.rating ?? undefined,
      reviewedAt: item.activityAt,
      text: item.reviewText ?? '',
      hasSpoilers: item.hasSpoilers ?? false,
      likeCount: item.likeCount,
      commentCount: item.commentCount,
      liked: item.likedByCurrentUser,
      isOwner: item.isOwner,
    }

    return reviewItem
  }

  const logItem: FeedLogItem = {
    type: 'log',
    id: item.gameLogId ?? `log-${item.gameId}-${item.activityAt}`,
    user: {
      userId: item.userId,
      userName: item.userName,
      displayName: item.displayName,
      avatarUrl: item.avatarUrl ?? undefined,
    },
    game: {
      gameId: item.gameId,
      title: item.gameTitle,
      coverUrl: item.coverImageUrl ?? undefined,
    },
    status: item.status ?? 'Backlog',
    rating: item.rating ?? undefined,
    platform: item.platform ?? undefined,
    hours: item.hours ?? undefined,
    updatedAt: item.activityAt,
  }

  return logItem
}
