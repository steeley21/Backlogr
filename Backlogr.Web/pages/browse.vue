<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from '#imports'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'
import { getGames } from '~/services/gameService'
import type { GameSummaryResponseDto } from '~/types/game'

const route = useRoute()

const games = ref<GameSummaryResponseDto[]>([])
const isLoading = ref(false)
const errorMessage = ref('')

const q = computed(() => {
  const value = route.query.q
  return typeof value === 'string' ? value.trim() : ''
})

const headerRightText = computed(() => {
  if (isLoading.value) {
    return 'Loading games...'
  }

  if (q.value) {
    return `${games.value.length} result${games.value.length === 1 ? '' : 's'} for “${q.value}”`
  }

  return 'Discover games'
})

function buildFallbackCover(title: string): string {
  const safeTitle = encodeURIComponent(title)
  return `https://placehold.co/300x450/1c2228/f5f5f5?text=${safeTitle}`
}

function formatSubtitle(game: GameSummaryResponseDto): string {
  const parts: string[] = []

  if (game.genres) {
    parts.push(game.genres)
  }

  if (game.releaseDate) {
    parts.push(new Date(game.releaseDate).getUTCFullYear().toString())
  }

  return parts.join(' • ')
}

function getErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const apiMessage = error.response?.data

    if (typeof apiMessage === 'string' && apiMessage.trim().length > 0) {
      return apiMessage
    }
  }

  return 'Unable to load games right now. Please try again.'
}

async function loadGames(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    games.value = await getGames({
      query: q.value || undefined,
      take: 30,
    })
  }
  catch (error: unknown) {
    games.value = []
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isLoading.value = false
  }
}

watch(
  () => q.value,
  async () => {
    await loadGames()
  },
  { immediate: true },
)
</script>

<template>
  <div>
    <SectionHeader
      icon="mdi-magnify"
      title="Browse"
      :right-text="headerRightText"
    />

    <v-alert
      v-if="errorMessage"
      type="error"
      variant="tonal"
      rounded="lg"
      class="mb-4"
    >
      {{ errorMessage }}
    </v-alert>

    <v-row v-if="isLoading" dense>
      <v-col
        v-for="n in 12"
        :key="n"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <v-skeleton-loader
          type="image, article"
          class="browse-skeleton"
        />
      </v-col>
    </v-row>

    <v-card
      v-else-if="games.length === 0"
      class="empty-state"
      rounded="xl"
      flat
    >
      <div class="text-h6 font-weight-bold mb-2">No games found</div>
      <div class="muted">
        Try a different search term or browse again with a broader query.
      </div>
    </v-card>

    <v-row v-else dense>
      <v-col
        v-for="game in games"
        :key="game.gameId"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <GamePosterCard
          :title="game.title"
          :cover-url="game.coverImageUrl ?? buildFallbackCover(game.title)"
          :subtitle="formatSubtitle(game)"
          :to="`/game/${game.gameId}`"
        />
      </v-col>
    </v-row>
  </div>
</template>

<style scoped>
.empty-state {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 24px;
}

.muted {
  color: var(--muted-foreground);
}

.browse-skeleton {
  background: transparent;
}
</style>