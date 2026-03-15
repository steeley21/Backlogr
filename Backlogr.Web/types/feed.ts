// /types/feed.ts
export type GameStatus = 'Playing' | 'Played' | 'Backlog' | 'Wishlist' | 'Dropped'
export type FeedItemType = 'GameLog' | 'Review'

export interface UserSummary {
  userId: string
  displayName: string
  userName: string
  avatarUrl?: string
}

export interface GameSummary {
  gameId: string
  title: string
  coverUrl?: string
}

export interface FeedReviewItem {
  type: 'review'
  id: string
  user: UserSummary
  game: GameSummary
  rating?: number
  reviewedAt: string
  text: string
  hasSpoilers?: boolean
  likeCount: number
  commentCount: number
  liked?: boolean
  isOwner: boolean
}

export interface FeedLogItem {
  type: 'log'
  id: string
  user: UserSummary
  game: GameSummary
  status: GameStatus
  rating?: number
  platform?: string
  hours?: number
  updatedAt: string
}

export type FeedItem = FeedReviewItem | FeedLogItem

export interface FeedItemResponseDto {
  itemType: FeedItemType
  activityAt: string
  userId: string
  userName: string
  displayName: string
  avatarUrl: string | null
  gameId: string
  gameTitle: string
  coverImageUrl: string | null
  gameLogId: string | null
  reviewId: string | null
  status: GameStatus | null
  rating: number | null
  platform: string | null
  hours: number | null
  reviewText: string | null
  hasSpoilers: boolean | null
  likeCount: number
  commentCount: number
  likedByCurrentUser: boolean
  isOwner: boolean
}
