// /services/reviewService.ts
import { useApi } from '~/services/api'
import type {
  CreateReviewRequestDto,
  ReviewResponseDto,
  UpdateReviewRequestDto,
} from '~/types/review'

export async function createReview(
  payload: CreateReviewRequestDto,
): Promise<ReviewResponseDto> {
  const api = useApi()
  const response = await api.post<ReviewResponseDto>('/api/Reviews', payload)
  return response.data
}

export async function updateReview(
  reviewId: string,
  payload: UpdateReviewRequestDto,
): Promise<ReviewResponseDto> {
  const api = useApi()
  const response = await api.put<ReviewResponseDto>(`/api/Reviews/${reviewId}`, payload)
  return response.data
}

export async function deleteReview(reviewId: string): Promise<void> {
  const api = useApi()
  await api.delete(`/api/Reviews/${reviewId}`)
}