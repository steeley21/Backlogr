<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from '#imports'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'
import { semanticSearch } from '~/services/aiService'
import { importGameFromIgdb, searchBrowseGames } from '~/services/gameService'
import type { SemanticSearchResultDto } from '~/types/ai'
import type { GameBrowseResultDto } from '~/types/game'

type BrowseSearchMode = 'standard' | 'semantic'

type BrowseDisplayGame = GameBrowseResultDto & {
  why?: string | null
  score?: number | null
}

const route = useRoute()
const router = useRouter()

const fallbackCoverUrl = '/images/fallback-game-cover.svg'

const games = ref<BrowseDisplayGame[]>([])
const isLoading = ref(false)
const errorMessage = ref('')
const activeGameKey = ref('')
const searchInput = ref('')

const semanticExamples = [
  'cozy farming game',
  'story rich sci fi adventure',
  'challenging action RPG',
]

const q = computed(() => {
  const value = route.query.q
  return typeof value === 'string' ? value.trim() : ''
})

const searchMode = computed<BrowseSearchMode>(() => {
  const value = route.query.mode
  return value === 'semantic' ? 'semantic' : 'standard'
})

const isSemanticMode = computed(() => {
  return searchMode.value === 'semantic'
})

const searchPlaceholder = computed(() => {
  return isSemanticMode.value
    ? 'Describe the kind of game you want...'
    : 'Search by title, genre, platform, or keyword...'
})

const headerRightText = computed(() => {
  if (isLoading.value) {
    return isSemanticMode.value ? 'Searching by vibe...' : 'Loading games...'
  }

  if (q.value) {
    return `${games.value.length} result${games.value.length === 1 ? '' : 's'} for “${q.value}”`
  }

  return isSemanticMode.value
    ? 'Describe the kind of game you want'
    : 'Discover games'
})

const modeDescription = computed(() => {
  return isSemanticMode.value
    ? 'Semantic search uses AI to match your natural-language query against game metadata.'
    : 'Standard search uses the regular game catalog search.'
})

const emptyStateTitle = computed(() => {
  if (isSemanticMode.value && !q.value) {
    return 'Try a natural-language search'
  }

  if (!isSemanticMode.value && !q.value) {
    return 'Search the catalog'
  }

  return 'No games found'
})

const emptyStateBody = computed(() => {
  if (isSemanticMode.value && !q.value) {
    return 'Try something like “cozy farming game”, “story rich sci fi adventure”, or “challenging action RPG”.'
  }

  if (!isSemanticMode.value && !q.value) {
    return 'Search by title, genre, or platform to browse the catalog.'
  }

  if (isSemanticMode.value) {
    return 'Try a different natural-language description or switch back to standard search.'
  }

  return 'Try a different search term or browse again with a broader query.'
})

function getGameKey(game: BrowseDisplayGame): string {
  if (game.gameId) {
    return game.gameId
  }

  if (game.igdbId !== null) {
    return `igdb-${game.igdbId}`
  }

  return game.title
}

function formatSubtitle(game: BrowseDisplayGame): string {
  const parts: string[] = []

  if (game.genres) {
    parts.push(game.genres)
  }

  if (game.releaseDate) {
    parts.push(new Date(game.releaseDate).getUTCFullYear().toString())
  }

  if (parts.length === 0 && isSemanticMode.value && game.why) {
    return game.why
  }

  return parts.join(' • ')
}

function mapSemanticResultToBrowseGame(result: SemanticSearchResultDto): BrowseDisplayGame {
  return {
    gameId: result.gameId,
    igdbId: null,
    title: result.title,
    coverImageUrl: result.coverImageUrl,
    releaseDate: null,
    platforms: result.platforms,
    genres: result.genres,
    why: result.why,
    score: result.score,
  }
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

async function setSearchMode(mode: BrowseSearchMode): Promise<void> {
  const currentQuery = searchInput.value.trim() || undefined

  await router.replace({
    path: '/browse',
    query: {
      ...route.query,
      q: currentQuery,
      mode: mode === 'semantic' ? 'semantic' : undefined,
    },
  })
}

async function submitSearch(): Promise<void> {
  const nextQuery = searchInput.value.trim()

  await router.replace({
    path: '/browse',
    query: {
      ...route.query,
      q: nextQuery || undefined,
      mode: isSemanticMode.value ? 'semantic' : undefined,
    },
  })
}

async function clearSearch(): Promise<void> {
  searchInput.value = ''

  await router.replace({
    path: '/browse',
    query: {
      ...route.query,
      q: undefined,
      mode: isSemanticMode.value ? 'semantic' : undefined,
    },
  })
}

async function applyExampleSearch(example: string): Promise<void> {
  searchInput.value = example
  await submitSearch()
}

async function loadGames(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    if (isSemanticMode.value) {
      if (!q.value) {
        games.value = []
        return
      }

      const semanticResults = await semanticSearch(q.value, 24)
      games.value = semanticResults.map(mapSemanticResultToBrowseGame)
      return
    }

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

async function handleGameSelected(game: BrowseDisplayGame): Promise<void> {
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
  (newQuery) => {
    searchInput.value = newQuery
  },
  { immediate: true },
)

watch(
  () => [q.value, searchMode.value],
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

    <v-card class="browse-toolbar mb-4" rounded="xl" flat>
      <div class="browse-toolbar__row">
        <div>
          <div class="browse-toolbar__title">Search mode</div>
          <div class="browse-toolbar__description">
            {{ modeDescription }}
          </div>
        </div>

        <v-btn-toggle
          :model-value="searchMode"
          mandatory
          divided
          rounded="pill"
          color="primary"
          @update:model-value="setSearchMode"
        >
          <v-btn value="standard" class="text-none">
            Standard
          </v-btn>
          <v-btn value="semantic" class="text-none">
            Semantic
          </v-btn>
        </v-btn-toggle>
      </div>

      <v-alert
        v-if="isSemanticMode"
        type="info"
        variant="tonal"
        rounded="lg"
        class="mt-4"
      >
        Try describing the kind of game you want in plain language instead of using exact titles.
      </v-alert>

      <div class="browse-search-panel mt-4">
        <div class="browse-search-panel__heading">
          {{ isSemanticMode ? 'What kind of game are you looking for?' : '' }}
        </div>
        <div class="browse-search-panel__subheading">
          {{ isSemanticMode
            ? 'Use vibe-based language like cozy, story rich, difficult, relaxing, or sci fi.'
            : '' }}
        </div>

        <div class="browse-search-panel__controls">
          <v-text-field
            v-model="searchInput"
            :placeholder="searchPlaceholder"
            prepend-inner-icon="mdi-magnify"
            variant="solo-filled"
            flat
            hide-details
            rounded="xl"
            class="browse-search-panel__input"
            @keyup.enter="submitSearch"
          />

          <v-btn
            color="primary"
            size="large"
            rounded="pill"
            class="text-none"
            @click="submitSearch"
          >
            Search
          </v-btn>

          <v-btn
            variant="text"
            size="large"
            rounded="pill"
            class="text-none"
            @click="clearSearch"
          >
            Clear
          </v-btn>
        </div>

        <div v-if="isSemanticMode" class="browse-search-panel__examples">
          <span class="browse-search-panel__examples-label">Try:</span>

          <v-chip
            v-for="example in semanticExamples"
            :key="example"
            size="small"
            variant="outlined"
            class="mr-2 mb-2"
            @click="applyExampleSearch(example)"
          >
            {{ example }}
          </v-chip>
        </div>
      </div>
    </v-card>

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
      <div class="text-h6 font-weight-bold mb-2">{{ emptyStateTitle }}</div>
      <div class="muted">
        {{ emptyStateBody }}
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
.browse-toolbar {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 22px 24px;
  position: relative;
  overflow: hidden;
}

.browse-toolbar::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(168, 85, 247, 0.4), transparent);
  pointer-events: none;
}

.browse-toolbar__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.browse-toolbar__title {
  font-weight: 700;
  color: var(--foreground);
  font-size: 0.95rem;
}

.browse-toolbar__description {
  margin-top: 4px;
  color: var(--muted-foreground);
  font-size: 0.88rem;
}

.browse-search-panel {
  padding-top: 4px;
}

.browse-search-panel__heading {
  font-size: 1rem;
  font-weight: 700;
  color: var(--foreground);
  letter-spacing: -0.01em;
}

.browse-search-panel__subheading {
  margin-top: 5px;
  color: var(--muted-foreground);
  font-size: 0.88rem;
}

.browse-search-panel__controls {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  margin-top: 16px;
}

.browse-search-panel__input {
  flex: 1 1 420px;
}

.browse-search-panel__examples {
  margin-top: 14px;
}

.browse-search-panel__examples-label {
  display: inline-block;
  margin-right: 10px;
  color: var(--muted-foreground);
  font-size: 0.88rem;
}

.empty-state {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 32px;
  text-align: center;
}

.muted {
  color: var(--muted-foreground);
}

.browse-skeleton {
  background: transparent;
}

/* Staggered fade-in for the game grid */
:deep(.v-col) {
  animation: fadeUp 320ms ease both;
}

:deep(.v-col:nth-child(1))  { animation-delay: 20ms; }
:deep(.v-col:nth-child(2))  { animation-delay: 45ms; }
:deep(.v-col:nth-child(3))  { animation-delay: 70ms; }
:deep(.v-col:nth-child(4))  { animation-delay: 95ms; }
:deep(.v-col:nth-child(5))  { animation-delay: 115ms; }
:deep(.v-col:nth-child(6))  { animation-delay: 135ms; }
:deep(.v-col:nth-child(7))  { animation-delay: 155ms; }
:deep(.v-col:nth-child(8))  { animation-delay: 170ms; }
:deep(.v-col:nth-child(9))  { animation-delay: 185ms; }
:deep(.v-col:nth-child(10)) { animation-delay: 200ms; }
:deep(.v-col:nth-child(11)) { animation-delay: 210ms; }
:deep(.v-col:nth-child(12)) { animation-delay: 220ms; }

@keyframes fadeUp {
  from { opacity: 0; transform: translateY(12px); }
  to   { opacity: 1; transform: translateY(0); }
}

@media (max-width: 600px) {
  .browse-toolbar {
    padding: 16px;
  }

  .browse-toolbar__row {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }

  /* Mode toggle fills full width */
  .browse-toolbar__row :deep(.v-btn-toggle) {
    width: 100%;
  }

  .browse-toolbar__row :deep(.v-btn-toggle .v-btn) {
    flex: 1;
  }

  .browse-search-panel__controls {
    flex-direction: column;
    gap: 10px;
  }

  .browse-search-panel__input {
    flex: none;
    width: 100%;
  }

  /* Search + buttons stack full width */
  .browse-search-panel__controls :deep(.v-btn) {
    width: 100%;
  }

  .empty-state {
    padding: 20px;
  }
}
</style>