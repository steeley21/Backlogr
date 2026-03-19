import { flushPromises } from '@vue/test-utils'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { defineComponent, h } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { GameDetailResponseDto, GameViewerStateResponseDto } from '~/types/game'

const useRouteMock = vi.hoisted(() => vi.fn())
const navigateToMock = vi.hoisted(() => vi.fn())

const aiServiceMocks = vi.hoisted(() => ({
  runReviewAssistant: vi.fn(),
}))

const gameServiceMocks = vi.hoisted(() => ({
  getGameById: vi.fn<(gameId: string) => Promise<GameDetailResponseDto>>(),
  getGameViewerState: vi.fn<(gameId: string) => Promise<GameViewerStateResponseDto>>(),
}))

const libraryServiceMocks = vi.hoisted(() => ({
  upsertLibraryLog: vi.fn(),
}))

const reviewServiceMocks = vi.hoisted(() => ({
  createReview: vi.fn(),
  updateReview: vi.fn(),
}))

vi.mock('~/services/aiService', () => ({
  runReviewAssistant: aiServiceMocks.runReviewAssistant,
}))

vi.mock('~/services/gameService', () => ({
  getGameById: gameServiceMocks.getGameById,
  getGameViewerState: gameServiceMocks.getGameViewerState,
}))

vi.mock('~/services/libraryService', () => ({
  upsertLibraryLog: libraryServiceMocks.upsertLibraryLog,
}))

vi.mock('~/services/reviewService', () => ({
  createReview: reviewServiceMocks.createReview,
  updateReview: reviewServiceMocks.updateReview,
}))

mockNuxtImport('useRoute', () => useRouteMock)
mockNuxtImport('navigateTo', () => navigateToMock)

const VFormStub = defineComponent({
  name: 'VForm',
  inheritAttrs: false,
  emits: ['submit'],
  setup(_, { attrs, emit, slots }) {
    return () => h('form', {
      ...attrs,
      onSubmit: (event: Event) => {
        event.preventDefault()
        emit('submit', event)
      },
    }, slots.default?.())
  },
})

const VTextFieldStub = defineComponent({
  name: 'VTextField',
  inheritAttrs: false,
  props: {
    modelValue: {
      type: [String, Number],
      default: '',
    },
    label: {
      type: String,
      default: '',
    },
    type: {
      type: String,
      default: 'text',
    },
  },
  emits: ['update:modelValue'],
  setup(props, { attrs, emit }) {
    return () => h('input', {
      ...attrs,
      type: props.type,
      'aria-label': props.label,
      value: props.modelValue ?? '',
      onInput: (event: Event) => emit('update:modelValue', (event.target as HTMLInputElement).value),
    })
  },
})

const VTextareaStub = defineComponent({
  name: 'VTextarea',
  inheritAttrs: false,
  props: {
    modelValue: {
      type: String,
      default: '',
    },
    label: {
      type: String,
      default: '',
    },
  },
  emits: ['update:modelValue'],
  setup(props, { attrs, emit }) {
    return () => h('textarea', {
      ...attrs,
      'aria-label': props.label,
      value: props.modelValue,
      onInput: (event: Event) => emit('update:modelValue', (event.target as HTMLTextAreaElement).value),
    })
  },
})

const VSelectStub = defineComponent({
  name: 'VSelect',
  inheritAttrs: false,
  props: {
    modelValue: {
      type: String,
      default: '',
    },
    items: {
      type: Array,
      default: () => [],
    },
    label: {
      type: String,
      default: '',
    },
  },
  emits: ['update:modelValue'],
  setup(props, { attrs, emit }) {
    return () => h('select', {
      ...attrs,
      'aria-label': props.label,
      value: props.modelValue,
      onChange: (event: Event) => emit('update:modelValue', (event.target as HTMLSelectElement).value),
    }, props.items.map(item => h('option', { value: item }, item as string)))
  },
})

const VCheckboxStub = defineComponent({
  name: 'VCheckbox',
  inheritAttrs: false,
  props: {
    modelValue: {
      type: Boolean,
      default: false,
    },
    label: {
      type: String,
      default: '',
    },
  },
  emits: ['update:modelValue'],
  setup(props, { attrs, emit }) {
    return () => h('label', [
      h('input', {
        ...attrs,
        type: 'checkbox',
        'aria-label': props.label,
        checked: props.modelValue,
        onChange: (event: Event) => emit('update:modelValue', (event.target as HTMLInputElement).checked),
      }),
      props.label,
    ])
  },
})

const VChipStub = defineComponent({
  name: 'VChip',
  inheritAttrs: false,
  emits: ['click'],
  setup(_, { attrs, emit, slots }) {
    return () => h('button', {
      ...attrs,
      onClick: (event: MouseEvent) => emit('click', event),
    }, slots.default?.())
  },
})

const SectionHeaderStub = defineComponent({
  name: 'SectionHeader',
  props: {
    title: {
      type: String,
      default: '',
    },
    rightText: {
      type: String,
      default: '',
    },
  },
  setup(props) {
    return () => h('div', [
      h('div', props.title),
      h('div', props.rightText),
    ])
  },
})

function findInputByLabel(wrapper: Awaited<ReturnType<typeof mountPage>>, label: string) {
  return wrapper.find(`[aria-label="${label}"]`)
}

async function mountPage(gameId = 'game-1') {
  useRouteMock.mockReturnValue({
    query: {
      gameId,
    },
  })

  const LogPage = (await import('~/pages/log.vue')).default

  return mountSuspended(LogPage, {
    global: {
      stubs: {
        ...uiStubs,
        SectionHeader: SectionHeaderStub,
        VForm: VFormStub,
        VTextField: VTextFieldStub,
        VTextarea: VTextareaStub,
        VSelect: VSelectStub,
        VCheckbox: VCheckboxStub,
        VChip: VChipStub,
        VDivider: true,
      },
    },
  })
}

describe('log page', () => {
  beforeEach(() => {
    useRouteMock.mockReset()
    navigateToMock.mockReset()
    navigateToMock.mockResolvedValue(undefined)
    aiServiceMocks.runReviewAssistant.mockReset()
    gameServiceMocks.getGameById.mockReset()
    gameServiceMocks.getGameViewerState.mockReset()
    libraryServiceMocks.upsertLibraryLog.mockReset()
    reviewServiceMocks.createReview.mockReset()
    reviewServiceMocks.updateReview.mockReset()

    gameServiceMocks.getGameById.mockResolvedValue({
      gameId: 'game-1',
      igdbId: 1,
      title: 'Elden Ring',
      slug: 'elden-ring',
      summary: 'An open-world action RPG.',
      coverImageUrl: 'cover.jpg',
      releaseDate: '2022-02-25T00:00:00Z',
      developer: 'FromSoftware',
      publisher: 'Bandai Namco',
      platforms: 'PC, PS5',
      genres: 'Action RPG',
      createdAt: '2026-03-01T00:00:00Z',
      updatedAt: '2026-03-10T00:00:00Z',
    })
  })

  it('loads an existing log and review into edit mode', async () => {
    gameServiceMocks.getGameViewerState.mockResolvedValue({
      log: {
        gameLogId: 'log-1',
        gameId: 'game-1',
        gameTitle: 'Elden Ring',
        coverImageUrl: 'cover.jpg',
        status: 'Playing',
        rating: 4,
        platform: 'PC',
        hours: 22,
        startedAt: '2026-03-01T00:00:00Z',
        finishedAt: '2026-03-15T00:00:00Z',
        notes: 'Try a different build next time.',
        updatedAt: '2026-03-16T12:00:00Z',
      },
      review: {
        reviewId: 'review-1',
        userId: 'user-1',
        userName: 'kate',
        displayName: 'Kate Steele',
        gameId: 'game-1',
        gameTitle: 'Elden Ring',
        text: 'Great combat and exploration.',
        hasSpoilers: true,
        createdAt: '2026-03-15T12:00:00Z',
        updatedAt: '2026-03-16T12:00:00Z',
      },
    })

    const wrapper = await mountPage('game-1')
    await flushPromises()

    expect(gameServiceMocks.getGameById).toHaveBeenCalledWith('game-1')
    expect(gameServiceMocks.getGameViewerState).toHaveBeenCalledWith('game-1')
    expect(wrapper.text()).toContain('Your existing log and review for this game were loaded into the form.')
    expect(wrapper.text()).toContain('Update log')

    expect((findInputByLabel(wrapper, 'Platform').element as HTMLInputElement).value).toBe('PC')
    expect((findInputByLabel(wrapper, 'Hours played').element as HTMLInputElement).value).toBe('22')
    expect((findInputByLabel(wrapper, 'Review text').element as HTMLTextAreaElement).value).toBe('Great combat and exploration.')
  })

  it('updates an existing review instead of creating a new one', async () => {
    gameServiceMocks.getGameViewerState.mockResolvedValue({
      log: {
        gameLogId: 'log-1',
        gameId: 'game-1',
        gameTitle: 'Elden Ring',
        coverImageUrl: 'cover.jpg',
        status: 'Playing',
        rating: 4,
        platform: 'PC',
        hours: 22,
        startedAt: '2026-03-01T00:00:00Z',
        finishedAt: '2026-03-15T00:00:00Z',
        notes: 'Try a different build next time.',
        updatedAt: '2026-03-16T12:00:00Z',
      },
      review: {
        reviewId: 'review-1',
        userId: 'user-1',
        userName: 'kate',
        displayName: 'Kate Steele',
        gameId: 'game-1',
        gameTitle: 'Elden Ring',
        text: 'Great combat and exploration.',
        hasSpoilers: true,
        createdAt: '2026-03-15T12:00:00Z',
        updatedAt: '2026-03-16T12:00:00Z',
      },
    })

    libraryServiceMocks.upsertLibraryLog.mockResolvedValue(undefined)
    reviewServiceMocks.updateReview.mockResolvedValue({
      reviewId: 'review-1',
    })

    const wrapper = await mountPage('game-1')
    await flushPromises()

    await findInputByLabel(wrapper, 'Platform').setValue('Xbox')
    await findInputByLabel(wrapper, 'Hours played').setValue('24')
    await findInputByLabel(wrapper, 'Review text').setValue('Still incredible on a second run.')
    await findInputByLabel(wrapper, 'This review contains spoilers').setValue(false)
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(libraryServiceMocks.upsertLibraryLog).toHaveBeenCalledWith({
      gameId: 'game-1',
      status: 'Playing',
      rating: 4,
      platform: 'Xbox',
      hours: 24,
      startedAt: '2026-03-01',
      finishedAt: '2026-03-15',
      notes: 'Try a different build next time.',
    })
    expect(reviewServiceMocks.updateReview).toHaveBeenCalledWith('review-1', {
      text: 'Still incredible on a second run.',
      hasSpoilers: false,
    })
    expect(reviewServiceMocks.createReview).not.toHaveBeenCalled()
    expect(navigateToMock).toHaveBeenCalledWith('/game/game-1')
  })

  it('shows validation errors before submitting invalid date ranges', async () => {
    gameServiceMocks.getGameViewerState.mockResolvedValue({
      log: null,
      review: null,
    })

    const wrapper = await mountPage('game-1')
    await flushPromises()

    await findInputByLabel(wrapper, 'Started at').setValue('2026-03-20')
    await findInputByLabel(wrapper, 'Finished at').setValue('2026-03-10')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(wrapper.text()).toContain('Finished date cannot be earlier than the started date.')
    expect(libraryServiceMocks.upsertLibraryLog).not.toHaveBeenCalled()
    expect(reviewServiceMocks.createReview).not.toHaveBeenCalled()
    expect(reviewServiceMocks.updateReview).not.toHaveBeenCalled()
  })
})
