// types/feed.ts
export type GameStatus = 'Playing' | 'Played' | 'Backlog' | 'Wishlist' | 'Dropped'

export interface UserSummary {
  userId: string
  displayName: string
  avatarUrl?: string
}

export interface GameSummary {
  gameId: number
  title: string
  coverUrl?: string
}

export interface FeedReviewItem {
  type: 'review'
  id: string
  user: UserSummary
  game: GameSummary
  rating: number // 0.0–5.0 in 0.5 steps
  reviewedAt: string // ISO
  text: string
  likeCount: number
  commentCount: number
  liked?: boolean
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
  updatedAt: string // ISO
}

export type FeedItem = FeedReviewItem | FeedLogItem