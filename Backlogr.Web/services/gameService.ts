import { useApi } from '~/services/api'
import type {
  GameBrowseResultDto,
  GameDetailResponseDto,
  GameSummaryResponseDto,
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