// /types/library.ts

export type LibraryStatus = 'Backlog' | 'Playing' | 'Played' | 'Wishlist' | 'Dropped'

export interface LibraryLogResponseDto {
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
  notes: string | null
  updatedAt: string
}

export interface UpsertLibraryLogRequestDto {
  gameId: string
  status: LibraryStatus
  rating: number | null
  platform: string | null
  hours: number | null
  startedAt: string | null
  finishedAt: string | null
  notes: string | null
}