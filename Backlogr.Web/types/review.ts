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