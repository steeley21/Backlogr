// /types/profile.ts
import type { LibraryStatus } from '~/types/library'

export interface PublicProfileReviewSummaryDto {
  reviewId: string
  gameId: string
  gameTitle: string
  coverImageUrl: string | null
  text: string
  hasSpoilers: boolean
  createdAt: string
  updatedAt: string
  likeCount: number
  commentCount: number
  likedByCurrentUser: boolean
  isOwner: boolean
}

export interface PublicProfileResponseDto {
  userId: string
  userName: string
  displayName: string
  bio: string | null
  avatarUrl: string | null
  followerCount: number
  followingCount: number
  reviewCount: number
  libraryCount: number
  isFollowing: boolean
  isSelf: boolean
  recentReviews: PublicProfileReviewSummaryDto[]
}

export interface PublicProfileLibraryItemResponseDto {
  gameLogId: string
  gameId: string
  gameTitle: string
  coverImageUrl: string | null
  status: LibraryStatus
  rating: number | null
  platform: string | null
  hours: number | null
  startedAt: string | null
  finishedAt: string | null
  updatedAt: string
}
