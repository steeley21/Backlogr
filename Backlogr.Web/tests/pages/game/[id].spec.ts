import { flushPromises } from '@vue/test-utils'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { defineComponent, h } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { FeedItem, FeedReviewItem } from '~/types/feed'
import type { GameDetailResponseDto, GameViewerStateResponseDto } from '~/types/game'

const useRouteMock = vi.hoisted(() => vi.fn())
const navigateToMock = vi.hoisted(() => vi.fn())

const gameServiceMocks = vi.hoisted(() => ({
  getGameById: vi.fn<(gameId: string) => Promise<GameDetailResponseDto>>(),
  getGameViewerState: vi.fn<(gameId: string) => Promise<GameViewerStateResponseDto>>(),
  getGameReviews: vi.fn<(gameId: string, take?: number) => Promise<FeedReviewItem[]>>(),
  getGameActivity: vi.fn<(gameId: string, take?: number) => Promise<FeedItem[]>>(),
}))

vi.mock('~/services/gameService', () => ({
  getGameById: gameServiceMocks.getGameById,
  getGameViewerState: gameServiceMocks.getGameViewerState,
  getGameReviews: gameServiceMocks.getGameReviews,
  getGameActivity: gameServiceMocks.getGameActivity,
}))

mockNuxtImport('useRoute', () => useRouteMock)
mockNuxtImport('navigateTo', () => navigateToMock)

const VBtnToggleStub = defineComponent({
  name: 'VBtnToggle',
  inheritAttrs: false,
  props: {
    modelValue: {
      type: null,
      default: undefined,
    },
  },
  emits: ['update:modelValue'],
  setup(_, { attrs, slots }) {
    return () => h('div', { ...attrs, 'data-stub': 'VBtnToggle' }, slots.default?.())
  },
})

const VSnackbarStub = defineComponent({
  name: 'VSnackbar',
  inheritAttrs: false,
  setup(_, { attrs, slots }) {
    return () => h('div', { ...attrs, 'data-stub': 'VSnackbar' }, slots.default?.())
  },
})

const FeedReviewCardStub = defineComponent({
  name: 'FeedReviewCard',
  props: {
    item: {
      type: Object,
      required: true,
    },
  },
  setup(props) {
    return () => h('div', { 'data-test': 'feed-review-card' }, `Review Card: ${(props.item as { text?: string }).text ?? ''}`)
  },
})

const FeedLogCardStub = defineComponent({
  name: 'FeedLogCard',
  props: {
    item: {
      type: Object,
      required: true,
    },
  },
  setup(props) {
    return () => h('div', { 'data-test': 'feed-log-card' }, `Log Card: ${(props.item as { status?: string }).status ?? ''}`)
  },
})

async function mountPage(gameId = 'game-1') {
  useRouteMock.mockReturnValue({
    params: {
      id: gameId,
    },
  })

  const GameDetailPage = (await import('~/pages/game/[id].vue')).default

  return mountSuspended(GameDetailPage, {
    global: {
      stubs: {
        ...uiStubs,
        SectionHeader: true,
        StarRating: true,
        FeedReviewCard: FeedReviewCardStub,
        FeedLogCard: FeedLogCardStub,
        VBtnToggle: VBtnToggleStub,
        VSnackbar: VSnackbarStub,
      },
    },
  })
}

describe('game detail page', () => {
  beforeEach(() => {
    useRouteMock.mockReset()
    navigateToMock.mockReset()
    navigateToMock.mockResolvedValue(undefined)
    gameServiceMocks.getGameById.mockReset()
    gameServiceMocks.getGameViewerState.mockReset()
    gameServiceMocks.getGameReviews.mockReset()
    gameServiceMocks.getGameActivity.mockReset()

    gameServiceMocks.getGameById.mockResolvedValue({
      gameId: 'game-1',
      igdbId: 1,
      title: 'Mass Effect',
      slug: 'mass-effect',
      summary: 'A sci-fi RPG about saving the galaxy.',
      coverImageUrl: 'cover.jpg',
      releaseDate: '2007-11-20T00:00:00Z',
      developer: 'BioWare',
      publisher: 'EA',
      platforms: 'PC, Xbox 360',
      genres: 'RPG',
      createdAt: '2026-03-01T00:00:00Z',
      updatedAt: '2026-03-10T00:00:00Z',
    })

    gameServiceMocks.getGameViewerState.mockResolvedValue({
      log: {
        gameLogId: 'log-1',
        gameId: 'game-1',
        gameTitle: 'Mass Effect',
        coverImageUrl: 'cover.jpg',
        status: 'Played',
        rating: 4.5,
        platform: 'PC',
        hours: 38,
        startedAt: '2026-03-01T00:00:00Z',
        finishedAt: '2026-03-10T00:00:00Z',
        notes: 'Loved the squad banter.',
        updatedAt: '2026-03-10T12:00:00Z',
      },
      review: {
        reviewId: 'review-1',
        userId: 'user-1',
        userName: 'kate',
        displayName: 'Kate Steele',
        gameId: 'game-1',
        gameTitle: 'Mass Effect',
        text: 'Still one of my favorite RPGs.',
        hasSpoilers: false,
        createdAt: '2026-03-10T12:00:00Z',
        updatedAt: '2026-03-11T12:00:00Z',
      },
    })

    gameServiceMocks.getGameReviews.mockResolvedValue([
      {
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
        rating: 4.5,
        reviewedAt: '2026-03-11T12:00:00Z',
        text: 'Still one of my favorite RPGs.',
        hasSpoilers: false,
        likeCount: 2,
        commentCount: 1,
        liked: false,
        isOwner: true,
      },
    ])

    gameServiceMocks.getGameActivity.mockResolvedValue([
      {
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
        rating: 4.5,
        reviewedAt: '2026-03-11T12:00:00Z',
        text: 'Still one of my favorite RPGs.',
        hasSpoilers: false,
        likeCount: 2,
        commentCount: 1,
        liked: false,
        isOwner: true,
      },
      {
        type: 'log',
        id: 'log-1',
        user: {
          userId: 'user-1',
          userName: 'kate',
          displayName: 'Kate Steele',
        },
        game: {
          gameId: 'game-1',
          title: 'Mass Effect',
        },
        status: 'Played',
        rating: 4.5,
        platform: 'PC',
        hours: 38,
        updatedAt: '2026-03-10T12:00:00Z',
      },
    ])
  })

  it('loads the game detail, viewer state, and community sections on mount', async () => {
    const wrapper = await mountPage('game-1')
    await flushPromises()

    expect(gameServiceMocks.getGameById).toHaveBeenCalledWith('game-1')
    expect(gameServiceMocks.getGameViewerState).toHaveBeenCalledWith('game-1')
    expect(gameServiceMocks.getGameReviews).toHaveBeenCalledWith('game-1', 20)
    expect(gameServiceMocks.getGameActivity).toHaveBeenCalledWith('game-1', 25)

    expect(wrapper.text()).toContain('Mass Effect')
    expect(wrapper.text()).toContain('Your review')
    expect(wrapper.text()).toContain('Still one of my favorite RPGs.')
    expect(wrapper.text()).toContain('1 review • 2 recent activities')
  })

  it('switches between reviews and activity tabs', async () => {
    const wrapper = await mountPage('game-1')
    await flushPromises()

    const tabToggle = wrapper.findComponent({ name: 'VBtnToggle' })
    expect(tabToggle.exists()).toBe(true)

    await tabToggle.vm.$emit('update:modelValue', 'reviews')
    await flushPromises()

    expect(wrapper.text()).toContain('Review Card: Still one of my favorite RPGs.')

    await tabToggle.vm.$emit('update:modelValue', 'activity')
    await flushPromises()

    expect(wrapper.text()).toContain('Review Card: Still one of my favorite RPGs.')
    expect(wrapper.text()).toContain('Log Card: Played')
  })

  it('routes to the log page from the hero action', async () => {
    const wrapper = await mountPage('game-1')
    await flushPromises()

    const logButton = wrapper.findAll('button').find(button => button.text().includes('Log'))
    expect(logButton).toBeTruthy()

    await logButton!.trigger('click')

    expect(navigateToMock).toHaveBeenCalledWith({
      path: '/log',
      query: {
        gameId: 'game-1',
      },
    })
  })
})
