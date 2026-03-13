// /types/game.ts

export interface GameSummaryResponseDto {
  gameId: string
  igdbId: number | null
  title: string
  coverImageUrl: string | null
  releaseDate: string | null
  platforms: string | null
  genres: string | null
}

export interface GameDetailResponseDto {
  gameId: string
  igdbId: number | null
  title: string
  slug: string | null
  summary: string | null
  coverImageUrl: string | null
  releaseDate: string | null
  developer: string | null
  publisher: string | null
  platforms: string | null
  genres: string | null
  createdAt: string
  updatedAt: string
}

export interface GetGamesParams {
  query?: string
  take?: number
}