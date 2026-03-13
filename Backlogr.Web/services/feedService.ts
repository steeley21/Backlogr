// /services/feedService.ts
import { useApi } from '~/services/api'
import type {
  FeedItem,
  FeedItemResponseDto,
  FeedLogItem,
  FeedReviewItem,
} from '~/types/feed'

export async function getFeed(take = 25): Promise<FeedItem[]> {
  const api = useApi()
  const response = await api.get<FeedItemResponseDto[]>('/api/Feed', {
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
      likeCount: 0,
      commentCount: 0,
      liked: false,
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