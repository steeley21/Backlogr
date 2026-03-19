import { flushPromises } from '@vue/test-utils'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import { defineComponent } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { RecommendationResponseDto } from '~/types/ai'

const aiServiceMocks = vi.hoisted(() => ({
  getRecommendations: vi.fn<(take?: number) => Promise<RecommendationResponseDto>>(),
}))

vi.mock('~/services/aiService', () => ({
  getRecommendations: aiServiceMocks.getRecommendations,
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

const VSelectStub = defineComponent({
  name: 'VSelect',
  props: {
    modelValue: {
      type: Number,
      default: 6,
    },
  },
  emits: ['update:modelValue'],
  template: `
    <select
      :value="modelValue"
      @change="$emit('update:modelValue', Number($event.target.value))"
    >
      <slot />
    </select>
  `,
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
  const RecommendPage = (await import('~/pages/recommend.vue')).default

  return mountSuspended(RecommendPage, {
    global: {
      stubs: {
        ...uiStubs,
        SectionHeader: true,
        GamePosterCard: GamePosterCardStub,
        VSelect: VSelectStub,
      },
    },
  })
}

describe('recommend page', () => {
  beforeEach(() => {
    aiServiceMocks.getRecommendations.mockReset()
  })

  it('loads recommendations on mount and renders the real AI copy', async () => {
    aiServiceMocks.getRecommendations.mockResolvedValue({
      items: [
        {
          gameId: 'game-1',
          title: 'Outer Wilds',
          coverImageUrl: null,
          why: 'Recommended because it semantically matches the games and review themes in your taste profile.',
        },
      ],
    })

    const wrapper = await mountPage()
    await flushPromises()

    expect(aiServiceMocks.getRecommendations).toHaveBeenCalledWith(6)
    expect(wrapper.text()).toContain('These picks are based on your logged games, ratings, and review themes.')
    expect(wrapper.text()).toContain('Outer Wilds')
  })

  it('shows the improved empty state when no recommendations are returned', async () => {
    aiServiceMocks.getRecommendations.mockResolvedValue({
      items: [],
    })

    const wrapper = await mountPage()
    await flushPromises()

    expect(wrapper.text()).toContain('No recommendations yet')
    expect(wrapper.text()).toContain('Log, rate, or review a few games')
    expect(wrapper.text()).not.toContain('current recommendation stub')
  })
})