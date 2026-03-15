// /services/reviewService.ts
import { useApi } from '~/services/api'
import type {
  CreateReviewCommentRequestDto,
  CreateReviewRequestDto,
  ReviewCommentResponseDto,
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

export async function likeReview(reviewId: string): Promise<void> {
  const api = useApi()
  await api.post(`/api/Reviews/${reviewId}/like`)
}

export async function unlikeReview(reviewId: string): Promise<void> {
  const api = useApi()
  await api.delete(`/api/Reviews/${reviewId}/like`)
}

export async function getReviewComments(reviewId: string): Promise<ReviewCommentResponseDto[]> {
  const api = useApi()
  const response = await api.get<ReviewCommentResponseDto[]>(`/api/Reviews/${reviewId}/comments`)
  return response.data
}

export async function createReviewComment(
  reviewId: string,
  payload: CreateReviewCommentRequestDto,
): Promise<ReviewCommentResponseDto> {
  const api = useApi()
  const response = await api.post<ReviewCommentResponseDto>(`/api/Reviews/${reviewId}/comments`, payload)
  return response.data
}

export async function deleteReviewComment(reviewCommentId: string): Promise<void> {
  const api = useApi()
  await api.delete(`/api/Comments/${reviewCommentId}`)
}
