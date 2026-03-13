// /services/aiService.ts
import { useApi } from '~/services/api'
import type {
  RecommendationResponseDto,
  ReviewAssistantRequestDto,
  ReviewAssistantResponseDto,
  SemanticSearchResultDto,
} from '~/types/ai'

export async function getRecommendations(take = 6): Promise<RecommendationResponseDto> {
  const api = useApi()
  const response = await api.post<RecommendationResponseDto>('/api/Ai/recommendations', {
    take,
  })

  return response.data
}

export async function runReviewAssistant(
  payload: ReviewAssistantRequestDto,
): Promise<ReviewAssistantResponseDto> {
  const api = useApi()
  const response = await api.post<ReviewAssistantResponseDto>('/api/Ai/review-assistant', payload)
  return response.data
}

export async function semanticSearch(
  query: string,
  take = 10,
): Promise<SemanticSearchResultDto[]> {
  const api = useApi()
  const response = await api.get<SemanticSearchResultDto[]>('/api/Ai/semantic-search', {
    params: {
      query: query.trim(),
      take,
    },
  })

  return response.data
}