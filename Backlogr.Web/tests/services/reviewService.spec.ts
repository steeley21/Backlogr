import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  createReviewComment,
  deleteReview,
  deleteReviewComment,
  getReviewComments,
  likeReview,
  unlikeReview,
  updateReview,
} from '~/services/reviewService'

const api = {
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  delete: vi.fn(),
}

vi.mock('~/services/api', async (importOriginal) => {
  const actual = await importOriginal<typeof import('~/services/api')>()

  return {
    ...actual,
    useApi: () => api,
  }
})

describe('reviewService', () => {
  beforeEach(() => {
    api.get.mockReset()
    api.post.mockReset()
    api.put.mockReset()
    api.delete.mockReset()
  })

  it('updates a review through the expected endpoint', async () => {
    const payload = { text: 'Updated text', hasSpoilers: true }
    api.put.mockResolvedValue({ data: { reviewId: 'review-1' } })

    const result = await updateReview('review-1', payload)

    expect(api.put).toHaveBeenCalledWith('/api/Reviews/review-1', payload)
    expect(result).toEqual({ reviewId: 'review-1' })
  })

  it('supports like and unlike actions', async () => {
    api.post.mockResolvedValue({})
    api.delete.mockResolvedValue({})

    await likeReview('review-1')
    await unlikeReview('review-1')

    expect(api.post).toHaveBeenCalledWith('/api/Reviews/review-1/like')
    expect(api.delete).toHaveBeenCalledWith('/api/Reviews/review-1/like')
  })

  it('loads, creates, and deletes comments from the review endpoints', async () => {
    api.get.mockResolvedValue({ data: [{ reviewCommentId: 'comment-1' }] })
    api.post.mockResolvedValue({ data: { reviewCommentId: 'comment-2' } })
    api.delete.mockResolvedValue({})

    const loaded = await getReviewComments('review-1')
    const created = await createReviewComment('review-1', { text: 'Nice review' })
    await deleteReviewComment('comment-2')
    await deleteReview('review-1')

    expect(api.get).toHaveBeenCalledWith('/api/Reviews/review-1/comments')
    expect(api.post).toHaveBeenCalledWith('/api/Reviews/review-1/comments', { text: 'Nice review' })
    expect(api.delete).toHaveBeenCalledWith('/api/Comments/comment-2')
    expect(api.delete).toHaveBeenCalledWith('/api/Reviews/review-1')
    expect(loaded).toEqual([{ reviewCommentId: 'comment-1' }])
    expect(created).toEqual({ reviewCommentId: 'comment-2' })
  })
})
