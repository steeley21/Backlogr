import { flushPromises } from '@vue/test-utils'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import { defineComponent, h } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { FeedItem } from '~/types/feed'

const feedServiceMocks = vi.hoisted(() => ({
  getFeed: vi.fn<(options?: { take?: number, scope?: 'for-you' | 'following' }) => Promise<FeedItem[]>>(),
}))

const routerState = vi.hoisted(() => ({
  route: null as { query: Record<string, unknown> } | null,
  replace: vi.fn(async ({ query }: { query: Record<string, unknown> }) => {
    if (routerState.route) {
      routerState.route.query = query
    }
  }),
  afterEach: vi.fn(() => {
    return () => {}
  }),
}))

vi.mock('~/services/feedService', () => ({
  getFeed: feedServiceMocks.getFeed,
}))

vi.mock('~/stores/auth', () => ({
  useAuthStore: () => ({
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
      afterEach: routerState.afterEach,
    }),
  }
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

const VSnackbarStub = defineComponent({
  name: 'VSnackbar',
  inheritAttrs: false,
  setup(_, { attrs, slots }) {
    return () => h('div', { ...attrs, 'data-stub': 'VSnackbar' }, slots.default?.())
  },
})

async function mountPage() {
  const FeedPage = (await import('~/pages/feed.vue')).default

  return mountSuspended(FeedPage, {
    global: {
      stubs: {
        ...uiStubs,
        VBtnToggle: VBtnToggleStub,
        VSnackbar: VSnackbarStub,
        AiCalloutCard: true,
        FeedLogCard: true,
        FeedReviewCard: true,
      },
    },
  })
}

describe('feed page', () => {
  beforeEach(() => {
    feedServiceMocks.getFeed.mockReset()
    feedServiceMocks.getFeed.mockResolvedValue([])
    routerState.replace.mockClear()
    routerState.afterEach.mockClear()

    if (routerState.route) {
      routerState.route.query = {}
    }
  })

  it('loads the following feed when the route query requests it', async () => {
    if (routerState.route) {
      routerState.route.query = {
        tab: 'following',
      }
    }

    const wrapper = await mountPage()
    await flushPromises()

    expect(feedServiceMocks.getFeed).toHaveBeenCalledWith({
      take: 25,
      scope: 'following',
    })
    expect(wrapper.text()).toContain('Following Feed')
  })

  it('normalizes a missing tab query to for-you', async () => {
    if (routerState.route) {
      routerState.route.query = {}
    }

    await mountPage()
    await flushPromises()

    expect(routerState.replace).toHaveBeenCalledWith({
      query: {
        tab: 'for-you',
      },
    })
    expect(feedServiceMocks.getFeed).toHaveBeenCalledWith({
      take: 25,
      scope: 'for-you',
    })
  })

  it('updates the route query and reloads when the source tab changes', async () => {
    if (routerState.route) {
      routerState.route.query = {
        tab: 'for-you',
      }
    }

    const wrapper = await mountPage()
    await flushPromises()

    feedServiceMocks.getFeed.mockClear()

    const tabToggles = wrapper.findAllComponents({ name: 'VBtnToggle' })
    expect(tabToggles.length).toBeGreaterThan(0)

    await tabToggles[0]!.vm.$emit('update:modelValue', 'following')
    await flushPromises()

    expect(routerState.replace).toHaveBeenCalledWith({
      query: {
        tab: 'following',
      },
    })
    expect(feedServiceMocks.getFeed).toHaveBeenCalledWith({
      take: 25,
      scope: 'following',
    })
  })
})