// /types/ai.ts

export interface RecommendationRequestDto {
  take: number
}

export interface RecommendedGameDto {
  gameId: string
  title: string
  coverImageUrl: string | null
  why: string | null
}

export interface RecommendationResponseDto {
  items: RecommendedGameDto[]
}

export type ReviewAssistantMode =
  | 'draft'
  | 'rewrite'
  | 'shorten'
  | 'expand'
  | 'spoiler-safe-summary'

export interface ReviewAssistantRequestDto {
  mode: ReviewAssistantMode
  prompt: string
  existingText: string | null
}

export interface ReviewAssistantResponseDto {
  mode: string
  resultText: string
}

export interface SemanticSearchResultDto {
  gameId: string
  title: string
  coverImageUrl: string | null
  summary: string | null
  genres: string | null
  platforms: string | null
  score: number
  why: string | null
}