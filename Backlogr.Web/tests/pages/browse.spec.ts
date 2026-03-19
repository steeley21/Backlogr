import { flushPromises } from '@vue/test-utils'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { defineComponent, h } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { SemanticSearchResultDto } from '~/types/ai'
import type { GameBrowseResultDto } from '~/types/game'

const gameServiceMocks = vi.hoisted(() => ({
  searchBrowseGames: vi.fn<(options: { query?: string, take: number }) => Promise<GameBrowseResultDto[]>>(),
  importGameFromIgdb: vi.fn(),
}))

const aiServiceMocks = vi.hoisted(() => ({
  semanticSearch: vi.fn<(query: string, take?: number) => Promise<SemanticSearchResultDto[]>>(),
}))

const routerState = vi.hoisted(() => ({
  route: null as { query: Record<string, unknown> } | null,
  replace: vi.fn(async (location: { path?: string, query?: Record<string, unknown> } = {}) => {
    if (!routerState.route) {
      return
    }

    const nextQuery = location.query ?? routerState.route.query ?? {}
    routerState.route.query = Object.fromEntries(
      Object.entries(nextQuery).filter(([, value]) => value !== undefined),
    )
  }),
  push: vi.fn(),
  afterEach: vi.fn(() => {
    return () => {}
  }),
  beforeEach: vi.fn(() => {
    return () => {}
  }),
  beforeResolve: vi.fn(() => {
    return () => {}
  }),
  onError: vi.fn(() => {
    return () => {}
  }),
  options: {
    scrollBehaviorType: 'auto',
  },
}))

vi.mock('~/services/gameService', () => ({
  searchBrowseGames: gameServiceMocks.searchBrowseGames,
  importGameFromIgdb: gameServiceMocks.importGameFromIgdb,
}))

vi.mock('~/services/aiService', () => ({
  semanticSearch: aiServiceMocks.semanticSearch,
}))

vi.mock('~/stores/auth', () => ({
  useAuthStore: () => ({
    isInitialized: true,
    isAuthenticated: true,
    hydrate: vi.fn(),
    displayName: 'Kate Steele',
    userName: 'kate',
  }),
}))

vi.mock('vue-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue-router')>()
  const { reactive } = await import('vue')

  if (!routerState.route) {
    routerState.route = reactive({
      query: {} as Record<string, unknown>,
    })
  }

  return {
    ...actual,
    useRoute: () => routerState.route!,
    useRouter: () => ({
        replace: routerState.replace,
        push: routerState.push,
        afterEach: routerState.afterEach,
        beforeEach: routerState.beforeEach,
        beforeResolve: routerState.beforeResolve,
        onError: routerState.onError,
        options: routerState.options,
        }),
  }
})

mockNuxtImport('useRoute', () => {
  return () => routerState.route!
})

mockNuxtImport('useRouter', () => {
    return () => ({
    replace: routerState.replace,
    push: routerState.push,
    afterEach: routerState.afterEach,
    beforeEach: routerState.beforeEach,
    beforeResolve: routerState.beforeResolve,
    onError: routerState.onError,
    options: routerState.options,
    })
})

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

const VTextFieldStub = defineComponent({
  name: 'VTextField',
  inheritAttrs: false,
  props: {
    modelValue: {
      type: String,
      default: '',
    },
  },
  emits: ['update:modelValue'],
  setup(props, { attrs, emit }) {
    return () => h('input', {
      ...attrs,
      value: props.modelValue,
      onInput: (event: Event) => emit('update:modelValue', (event.target as HTMLInputElement).value),
    })
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

const GamePosterCardStub = defineComponent({
  name: 'GamePosterCard',
  props: {
    title: {
      type: String,
      required: true,
    },
    subtitle: {
      type: String,
      default: '',
    },
  },
  template: `
    <div data-test="game-poster-card">
      <div>{{ title }}</div>
      <div>{{ subtitle }}</div>
    </div>
  `,
})

async function mountPage() {
  const BrowsePage = (await import('~/pages/browse.vue')).default

  return mountSuspended(BrowsePage, {
    global: {
      stubs: {
        ...uiStubs,
        SectionHeader: true,
        GamePosterCard: GamePosterCardStub,
        VBtnToggle: VBtnToggleStub,
        VTextField: VTextFieldStub,
        VChip: VChipStub,
      },
    },
  })
}

describe('browse page', () => {
  beforeEach(() => {
    gameServiceMocks.searchBrowseGames.mockReset()
    gameServiceMocks.importGameFromIgdb.mockReset()
    aiServiceMocks.semanticSearch.mockReset()

    gameServiceMocks.searchBrowseGames.mockResolvedValue([])
    aiServiceMocks.semanticSearch.mockResolvedValue([])

    routerState.replace.mockClear()
    routerState.push.mockClear()
    routerState.afterEach.mockClear()
    routerState.beforeEach.mockClear()
    routerState.beforeResolve.mockClear()
    routerState.onError.mockClear()

    if (routerState.route) {
      routerState.route.query = {}
    }
  })

  it('loads standard browse results by default', async () => {
    await mountPage()
    await flushPromises()

    expect(gameServiceMocks.searchBrowseGames).toHaveBeenCalledWith({
      query: undefined,
      take: 30,
    })
    expect(aiServiceMocks.semanticSearch).not.toHaveBeenCalled()
  })

  it('loads semantic results when semantic mode is active', async () => {
    if (routerState.route) {
      routerState.route.query = {
        q: 'cozy farming game',
        mode: 'semantic',
      }
    }

    aiServiceMocks.semanticSearch.mockResolvedValue([
      {
        gameId: 'game-1',
        title: 'Stardew Valley',
        coverImageUrl: null,
        summary: 'Farm, fish, and build relationships in Pelican Town.',
        genres: 'Simulator, Role-playing (RPG)',
        platforms: 'PC',
        score: 0.92,
        why: 'Matched by hybrid semantic search.',
      },
    ])

    const wrapper = await mountPage()
    await flushPromises()

    expect(aiServiceMocks.semanticSearch).toHaveBeenCalledWith('cozy farming game', 24)
    expect(gameServiceMocks.searchBrowseGames).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('Stardew Valley')
  })

  it('switches to semantic mode and reloads results', async () => {
    if (routerState.route) {
      routerState.route.query = {
        q: 'cozy farming game',
      }
    }

    const wrapper = await mountPage()
    await flushPromises()

    gameServiceMocks.searchBrowseGames.mockClear()
    aiServiceMocks.semanticSearch.mockClear()
    routerState.replace.mockClear()

    const toggle = wrapper.findComponent({ name: 'VBtnToggle' })
    await toggle.vm.$emit('update:modelValue', 'semantic')
    await flushPromises()
    await flushPromises()

    expect(routerState.replace).toHaveBeenCalledWith({
      path: '/browse',
      query: {
        q: 'cozy farming game',
        mode: 'semantic',
      },
    })
    expect(aiServiceMocks.semanticSearch).toHaveBeenCalledWith('cozy farming game', 24)
  })
})