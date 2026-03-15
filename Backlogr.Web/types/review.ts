// /types/review.ts

export interface CreateReviewRequestDto {
  gameId: string
  text: string
  hasSpoilers: boolean
}

export interface UpdateReviewRequestDto {
  text: string
  hasSpoilers: boolean
}

export interface CreateReviewCommentRequestDto {
  text: string
}

export interface ReviewResponseDto {
  reviewId: string
  userId: string
  userName: string
  displayName: string
  gameId: string
  gameTitle: string
  text: string
  hasSpoilers: boolean
  createdAt: string
  updatedAt: string
}

export interface ReviewCommentResponseDto {
  reviewCommentId: string
  reviewId: string
  userId: string
  userName: string
  displayName: string
  avatarUrl?: string | null
  text: string
  createdAt: string
  isOwner: boolean
}
