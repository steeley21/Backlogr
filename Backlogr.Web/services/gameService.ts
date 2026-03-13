// /services/gameService.ts
import { useApi } from '~/services/api'
import type {
  GameDetailResponseDto,
  GameSummaryResponseDto,
  GetGamesParams,
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

export async function getGameById(gameId: string): Promise<GameDetailResponseDto> {
  const api = useApi()
  const response = await api.get<GameDetailResponseDto>(`/api/Games/${gameId}`)
  return response.data
}