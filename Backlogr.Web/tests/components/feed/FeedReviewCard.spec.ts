import { defineComponent } from 'vue'
import { flushPromises, shallowMount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import FeedReviewCard from '~/components/feed/FeedReviewCard.vue'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { FeedReviewItem } from '~/types/feed'

const reviewServiceMocks = vi.hoisted(() => ({
  likeReview: vi.fn<(...args: unknown[]) => Promise<void>>(),
  unlikeReview: vi.fn<(...args: unknown[]) => Promise<void>>(),
}))

vi.mock('~/services/reviewService', () => ({
  likeReview: reviewServiceMocks.likeReview,
  unlikeReview: reviewServiceMocks.unlikeReview,
}))

const FeedReviewCommentThreadStub = defineComponent({
  name: 'FeedReviewCommentThread',
  props: {
    reviewId: {
      type: String,
      required: true,
    },
  },
  template: '<div data-test="comment-thread">Thread for {{ reviewId }}</div>',
})

const baseItem: FeedReviewItem = {
  type: 'review',
  id: 'review-1',
  user: {
    userId: 'user-1',
    userName: 'kate',
    displayName: 'Kate Steele',
  },
  game: {
    gameId: 'game-1',
    title: 'Mass Effect',
  },
  rating: 4,
  reviewedAt: '2026-03-14T12:00:00Z',
  text: 'Great review text.',
  hasSpoilers: false,
  likeCount: 2,
  commentCount: 1,
  liked: false,
  isOwner: true,
}

function mountCard(item: FeedReviewItem = baseItem) {
  return shallowMount(FeedReviewCard, {
    props: { item },
    global: {
      stubs: {
        ...uiStubs,
        StarRating: true,
        FeedReviewCommentThread: FeedReviewCommentThreadStub,
        FeedReviewEditDialog: true,
        FeedReviewDeleteDialog: true,
      },
    },
  })
}

describe('FeedReviewCard', () => {
  beforeEach(() => {
    reviewServiceMocks.likeReview.mockReset()
    reviewServiceMocks.unlikeReview.mockReset()
  })

  it('likes a review and updates the local UI state', async () => {
    reviewServiceMocks.likeReview.mockResolvedValue()

    const wrapper = mountCard()
    await wrapper.get('button[aria-label="Like review"]').trigger('click')
    await flushPromises()

    expect(reviewServiceMocks.likeReview).toHaveBeenCalledWith('review-1')
    expect(wrapper.find('button[aria-label="Unlike review"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('3')
  })

  it('reverts the optimistic like update and emits feedback on failure', async () => {
    reviewServiceMocks.likeReview.mockRejectedValue(new Error('network failure'))

    const wrapper = mountCard()
    await wrapper.get('button[aria-label="Like review"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('button[aria-label="Like review"]').exists()).toBe(true)
    expect(wrapper.emitted('feedback')).toEqual([
      ['Unable to update the like right now.', 'error'],
    ])
  })

  it('lazily mounts the comment thread the first time comments are opened', async () => {
    const wrapper = mountCard()

    expect(wrapper.find('[data-test="comment-thread"]').exists()).toBe(false)

    await wrapper.get('button[aria-label="Show comments"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-test="comment-thread"]').exists()).toBe(true)
    expect(wrapper.find('button[aria-label="Hide comments"]').exists()).toBe(true)
  })
})
