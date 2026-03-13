<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, navigateTo } from '#imports'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import { getGameById } from '~/services/gameService'
import type { GameDetailResponseDto } from '~/types/game'

const route = useRoute()

const game = ref<GameDetailResponseDto | null>(null)
const isLoading = ref(false)
const errorMessage = ref('')
const tab = ref<'about' | 'reviews' | 'activity'>('about')

const gameId = computed(() => {
  const value = route.params.id
  return typeof value === 'string' ? value : ''
})

const releaseYear = computed(() => {
  if (!game.value?.releaseDate) {
    return null
  }

  return new Date(game.value.releaseDate).getUTCFullYear().toString()
})

const heroMeta = computed(() => {
  if (!game.value) {
    return ''
  }

  const parts: string[] = []

  if (game.value.genres) {
    parts.push(game.value.genres)
  }

  if (releaseYear.value) {
    parts.push(releaseYear.value)
  }

  if (game.value.platforms) {
    parts.push(game.value.platforms)
  }

  return parts.join(' • ')
})

const coverStyle = computed(() => {
  const url = game.value?.coverImageUrl?.trim()

  if (!url) {
    return {
      background:
        'linear-gradient(135deg, rgba(168, 85, 247, 0.18), rgba(28, 34, 40, 0.94))',
    }
  }

  return {
    backgroundImage: `url('${url}')`,
    backgroundSize: 'cover',
    backgroundPosition: 'center',
  }
})

function getErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const apiMessage = error.response?.data

    if (typeof apiMessage === 'string' && apiMessage.trim().length > 0) {
      return apiMessage
    }
  }

  return 'Unable to load this game right now. Please try again.'
}

async function loadGame(): Promise<void> {
  if (!gameId.value) {
    game.value = null
    errorMessage.value = 'Invalid game id.'
    return
  }

  isLoading.value = true
  errorMessage.value = ''

  try {
    game.value = await getGameById(gameId.value)
  }
  catch (error: unknown) {
    game.value = null
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isLoading.value = false
  }
}

watch(
  () => gameId.value,
  async () => {
    await loadGame()
  },
  { immediate: true },
)

async function goToLog(): Promise<void> {
  if (!gameId.value) {
    return
  }

  await navigateTo({
    path: '/log',
    query: { gameId: gameId.value },
  })
}
</script>

<template>
  <div>
    <v-skeleton-loader
      v-if="isLoading"
      type="image, article, article"
      class="detail-skeleton"
    />

    <template v-else-if="game">
      <v-card
        class="hero"
        rounded="xl"
        flat
        :style="coverStyle"
      >
        <div class="hero-overlay" />
        <div class="hero-inner">
          <div class="text-h4 font-weight-bold">{{ game.title }}</div>
          <div v-if="heroMeta" class="muted mt-2">{{ heroMeta }}</div>

          <div class="mt-4 d-flex ga-3 flex-wrap">
            <v-btn
              color="primary"
              rounded="pill"
              class="text-none px-6"
              @click="goToLog"
            >
              Log
            </v-btn>

            <v-btn
              variant="text"
              rounded="pill"
              class="text-none"
              @click="goToLog"
            >
              Add to Library
            </v-btn>
          </div>
        </div>
      </v-card>

      <div class="mt-5">
        <v-btn-toggle
          v-model="tab"
          mandatory
          rounded="pill"
          class="filter"
        >
          <v-btn value="about" class="text-none">About</v-btn>
          <v-btn value="reviews" class="text-none">Reviews</v-btn>
          <v-btn value="activity" class="text-none">Activity</v-btn>
        </v-btn-toggle>

        <v-card class="panel mt-4" rounded="xl" flat>
          <div v-if="tab === 'about'">
            <SectionHeader icon="mdi-information" title="About" />

            <div class="about-grid">
              <div class="about-main">
                <div class="muted">
                  {{ game.summary || 'No summary is available for this game yet.' }}
                </div>
              </div>

              <div class="meta-card">
                <div class="meta-row">
                  <div class="meta-label">Release</div>
                  <div class="meta-value">
                    {{ game.releaseDate ? new Date(game.releaseDate).toLocaleDateString() : 'Unknown' }}
                  </div>
                </div>

                <div class="meta-row">
                  <div class="meta-label">Developer</div>
                  <div class="meta-value">{{ game.developer || 'Unknown' }}</div>
                </div>

                <div class="meta-row">
                  <div class="meta-label">Publisher</div>
                  <div class="meta-value">{{ game.publisher || 'Unknown' }}</div>
                </div>

                <div class="meta-row">
                  <div class="meta-label">Platforms</div>
                  <div class="meta-value">{{ game.platforms || 'Unknown' }}</div>
                </div>

                <div class="meta-row">
                  <div class="meta-label">Genres</div>
                  <div class="meta-value">{{ game.genres || 'Unknown' }}</div>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="tab === 'reviews'">
            <SectionHeader icon="mdi-message-text" title="Reviews" />
            <div class="muted">
              Review wiring comes next after the core log and review endpoints are connected.
            </div>
          </div>

          <div v-else>
            <SectionHeader icon="mdi-history" title="Activity" />
            <div class="muted">
              Activity wiring will be added after feed and library integration are in place.
            </div>
          </div>
        </v-card>
      </div>
    </template>

    <template v-else>
      <v-alert
        v-if="errorMessage"
        type="error"
        variant="tonal"
        rounded="lg"
        class="mb-4"
      >
        {{ errorMessage }}
      </v-alert>

      <v-card class="panel" rounded="xl" flat>
        <div class="text-h6 font-weight-bold mb-2">Game unavailable</div>
        <div class="muted">
          We could not load this game.
        </div>
      </v-card>
    </template>
  </div>
</template>

<style scoped>
.hero {
  position: relative;
  border: 1px solid var(--border);
  overflow: hidden;
  min-height: 260px;
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(900px 260px at 20% 10%, rgba(168, 85, 247, 0.18), transparent 55%),
    linear-gradient(90deg, rgba(20, 24, 28, 0.94) 0%, rgba(20, 24, 28, 0.62) 55%, rgba(20, 24, 28, 0.36) 100%);
}

.hero-inner {
  position: relative;
  padding: 34px 34px;
  max-width: 760px;
}

.panel {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 16px;
}

.muted {
  color: var(--muted-foreground);
}

.filter {
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid var(--border);
  padding: 4px;
}

.filter :deep(.v-btn) {
  border-radius: 9999px !important;
  color: var(--muted-foreground);
  background: transparent;
  min-height: 34px;
}

.filter :deep(.v-btn--active) {
  background: rgba(255, 255, 255, 0.10);
  color: var(--foreground);
}

.about-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 280px;
  gap: 20px;
  align-items: start;
}

.meta-card {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: var(--radius);
  padding: 14px;
}

.meta-row + .meta-row {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.meta-label {
  font-size: 0.82rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--muted-foreground);
  margin-bottom: 4px;
}

.meta-value {
  color: var(--foreground);
  line-height: 1.45;
}

.detail-skeleton {
  background: transparent;
}

@media (max-width: 860px) {
  .about-grid {
    grid-template-columns: 1fr;
  }

  .hero-inner {
    padding: 28px 24px;
  }
}
</style>