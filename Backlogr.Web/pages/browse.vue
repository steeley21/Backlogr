<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from '#imports'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'
import { importGameFromIgdb, searchBrowseGames } from '~/services/gameService'
import type { GameBrowseResultDto } from '~/types/game'

const route = useRoute()
const router = useRouter()

const fallbackCoverUrl = '/images/fallback-game-cover.svg'

const games = ref<GameBrowseResultDto[]>([])
const isLoading = ref(false)
const errorMessage = ref('')
const activeGameKey = ref('')

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

function getGameKey(game: GameBrowseResultDto): string {
  if (game.gameId) {
    return game.gameId
  }

  if (game.igdbId !== null) {
    return `igdb-${game.igdbId}`
  }

  return game.title
}

function formatSubtitle(game: GameBrowseResultDto): string {
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

    if (Array.isArray(apiMessage) && apiMessage.length > 0) {
      return apiMessage.join(', ')
    }
  }

  return 'Unable to load games right now. Please try again.'
}

async function loadGames(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    games.value = await searchBrowseGames({
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

async function handleGameSelected(game: GameBrowseResultDto): Promise<void> {
  const gameKey = getGameKey(game)

  if (activeGameKey.value) {
    return
  }

  activeGameKey.value = gameKey
  errorMessage.value = ''

  try {
    let localGameId = game.gameId

    if (!localGameId) {
      if (game.igdbId === null) {
        throw new Error('This game could not be opened.')
      }

      const importedGame = await importGameFromIgdb(game.igdbId)
      localGameId = importedGame.gameId

      games.value = games.value.map((currentGame) => {
        if (getGameKey(currentGame) !== gameKey) {
          return currentGame
        }

        return {
          ...currentGame,
          gameId: importedGame.gameId,
          igdbId: importedGame.igdbId,
          title: importedGame.title,
          coverImageUrl: importedGame.coverImageUrl ?? currentGame.coverImageUrl,
          releaseDate: importedGame.releaseDate ?? currentGame.releaseDate,
          platforms: importedGame.platforms ?? currentGame.platforms,
          genres: importedGame.genres ?? currentGame.genres,
        }
      })
    }

    await router.push(`/game/${localGameId}`)
  }
  catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    activeGameKey.value = ''
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
        :key="getGameKey(game)"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >

        <GamePosterCard
          :title="game.title"
          :cover-url="game.coverImageUrl ?? fallbackCoverUrl"
          :subtitle="formatSubtitle(game)"
          :loading="activeGameKey === getGameKey(game)"
          :disabled="activeGameKey.length > 0 && activeGameKey !== getGameKey(game)"
          clickable
          @click="handleGameSelected(game)"
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