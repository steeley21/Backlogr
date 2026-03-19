<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'
import { getRecommendations } from '~/services/aiService'
import type { RecommendedGameDto } from '~/types/ai'

const fallbackCoverUrl = '/images/fallback-game-cover.svg'

const recs = ref<RecommendedGameDto[]>([])
const isLoading = ref(false)
const errorMessage = ref('')
const take = ref(6)

const headerRightText = computed(() => {
  if (isLoading.value) {
    return 'Generating recommendations...'
  }

  return 'Personalized recommendations'
})

const introText = computed(() => {
  return 'These picks are based on your logged games, ratings, and review themes.'
})

const emptyStateText = computed(() => {
  return 'Log, rate, or review a few games to help Backlogr understand your taste and generate stronger recommendations.'
})

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

  return 'Unable to load recommendations right now. Please try again.'
}

async function loadRecommendations(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const response = await getRecommendations(take.value)
    recs.value = response.items
  }
  catch (error: unknown) {
    recs.value = []
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  await loadRecommendations()
})
</script>

<template>
  <div>
    <SectionHeader
      icon="mdi-sparkles"
      title="AI Picks"
      :right-text="headerRightText"
    />

    <v-card class="panel" rounded="xl" flat>
      <div class="panel-head">
        <div>
          <div class="text-h6 font-weight-semibold mb-1">Recommendations for you</div>
          <div class="muted">
            {{ introText }}
          </div>
        </div>

        <div class="panel-actions">
          <v-select
            v-model="take"
            :items="[3, 6, 9, 12]"
            label="Count"
            density="compact"
            variant="solo-filled"
            rounded="xl"
            hide-details
            class="take-select"
          />

          <v-btn
            color="primary"
            rounded="pill"
            class="text-none px-6"
            prepend-icon="mdi-refresh"
            :loading="isLoading"
            @click="loadRecommendations"
          >
            Refresh
          </v-btn>
        </div>
      </div>

      <v-alert
        type="info"
        variant="tonal"
        rounded="lg"
        class="mt-4"
      >
        Better logs lead to better picks. Add ratings and reviews to improve recommendation quality.
      </v-alert>
    </v-card>

    <v-alert
      v-if="errorMessage"
      type="error"
      variant="tonal"
      rounded="lg"
      class="mt-4"
    >
      {{ errorMessage }}
    </v-alert>

    <v-row v-if="isLoading" dense class="mt-5">
      <v-col
        v-for="n in take"
        :key="n"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <v-skeleton-loader type="image, article" class="rec-skeleton" />
      </v-col>
    </v-row>

    <v-card
      v-else-if="recs.length === 0"
      class="panel empty-state mt-5"
      rounded="xl"
      flat
    >
      <div class="text-h6 font-weight-bold mb-2">No recommendations yet</div>
      <div class="muted mb-4">
        {{ emptyStateText }}
      </div>

      <div class="d-flex ga-3 flex-wrap">
        <v-btn
          to="/browse"
          color="primary"
          rounded="pill"
          class="text-none px-6"
        >
          Browse games
        </v-btn>

        <v-btn
          to="/library"
          variant="text"
          rounded="pill"
          class="text-none"
        >
          Go to library
        </v-btn>
      </div>
    </v-card>

    <div v-else class="mt-5">
      <SectionHeader icon="mdi-star" title="Recommended for you" />

      <v-row dense>
        <v-col
          v-for="game in recs"
          :key="game.gameId"
          cols="6"
          sm="4"
          md="3"
          lg="2"
        >
          <GamePosterCard
            :title="game.title"
            :cover-url="game.coverImageUrl ?? fallbackCoverUrl"
            :subtitle="game.why ?? 'Recommended for your taste profile'"
            :to="`/game/${game.gameId}`"
          />
        </v-col>
      </v-row>
    </div>
  </div>
</template>

<style scoped>
.panel {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 16px;
}

.panel-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.panel-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.take-select {
  width: 110px;
}

.empty-state {
  padding: 24px;
}

.muted {
  color: var(--muted-foreground);
}

.rec-skeleton {
  background: transparent;
}
</style>