import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import FeedReviewCommentThread from '~/components/feed/FeedReviewCommentThread.vue'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { ReviewCommentResponseDto } from '~/types/review'

const reviewServiceMocks = vi.hoisted(() => ({
  getReviewComments: vi.fn<(...args: unknown[]) => Promise<ReviewCommentResponseDto[]>>(),
  createReviewComment: vi.fn<(...args: unknown[]) => Promise<ReviewCommentResponseDto>>(),
  deleteReviewComment: vi.fn<(...args: unknown[]) => Promise<void>>(),
}))

vi.mock('~/services/reviewService', () => ({
  getReviewComments: reviewServiceMocks.getReviewComments,
  createReviewComment: reviewServiceMocks.createReviewComment,
  deleteReviewComment: reviewServiceMocks.deleteReviewComment,
}))

const existingComment: ReviewCommentResponseDto = {
  reviewCommentId: 'comment-1',
  reviewId: 'review-1',
  userId: 'user-1',
  userName: 'kate',
  displayName: 'Kate Steele',
  avatarUrl: null,
  text: 'First!',
  createdAt: '2026-03-14T12:00:00Z',
  isOwner: true,
}

function mountThread() {
  return mount(FeedReviewCommentThread, {
    props: { reviewId: 'review-1' },
    global: {
      stubs: uiStubs,
    },
  })
}

describe('FeedReviewCommentThread', () => {
  beforeEach(() => {
    reviewServiceMocks.getReviewComments.mockReset()
    reviewServiceMocks.createReviewComment.mockReset()
    reviewServiceMocks.deleteReviewComment.mockReset()
  })

  it('loads comments on mount and emits the starting count', async () => {
    reviewServiceMocks.getReviewComments.mockResolvedValue([existingComment])

    const wrapper = mountThread()
    await flushPromises()

    expect(reviewServiceMocks.getReviewComments).toHaveBeenCalledWith('review-1')
    expect(wrapper.text()).toContain('First!')
    expect(wrapper.emitted('countUpdated')).toEqual([[1]])
  })

  it('adds and removes comments while keeping the count in sync', async () => {
    reviewServiceMocks.getReviewComments.mockResolvedValue([existingComment])
    reviewServiceMocks.createReviewComment.mockResolvedValue({
      reviewCommentId: 'comment-2',
      reviewId: 'review-1',
      userId: 'user-2',
      userName: 'sam',
      displayName: 'Sam Carter',
      avatarUrl: null,
      text: 'Nice take.',
      createdAt: '2026-03-15T12:00:00Z',
      isOwner: true,
    })
    reviewServiceMocks.deleteReviewComment.mockResolvedValue()

    const wrapper = mountThread()
    await flushPromises()

    await wrapper.get('textarea').setValue('  Nice take.  ')
    const postButton = wrapper.findAll('button').find(button => button.text().includes('Post comment'))
    expect(postButton).toBeTruthy()
    await postButton!.trigger('click')
    await flushPromises()

    expect(reviewServiceMocks.createReviewComment).toHaveBeenCalledWith('review-1', { text: 'Nice take.' })
    expect(wrapper.text()).toContain('Nice take.')

    const deleteButtons = wrapper.findAll('button[aria-label="Delete comment"]')
    expect(deleteButtons).toHaveLength(2)

    await deleteButtons[1]!.trigger('click')
    await flushPromises()

    expect(reviewServiceMocks.deleteReviewComment).toHaveBeenCalledWith('comment-2')
    expect(wrapper.text()).not.toContain('Nice take.')
    expect(wrapper.emitted('countUpdated')).toEqual([[1], [2], [1]])
  })
})
