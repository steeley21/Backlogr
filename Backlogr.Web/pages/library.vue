<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'
import { getMyLibrary } from '~/services/libraryService'
import type { LibraryLogResponseDto, LibraryStatus } from '~/types/library'

type LibraryTab = {
  label: string
  value: LibraryStatus
}

const tabs: LibraryTab[] = [
  { label: 'Backlog', value: 'Backlog' },
  { label: 'Playing', value: 'Playing' },
  { label: 'Played', value: 'Played' },
  { label: 'Wishlist', value: 'Wishlist' },
  { label: 'Dropped', value: 'Dropped' },
]

const activeTab = ref<LibraryStatus>('Backlog')
const logs = ref<LibraryLogResponseDto[]>([])
const isLoading = ref(false)
const errorMessage = ref('')

const visibleLogs = computed(() => {
  return logs.value.filter(log => log.status === activeTab.value)
})

const headerRightText = computed(() => {
  if (isLoading.value) {
    return 'Loading your library...'
  }

  const total = logs.value.length
  return `${total} game${total === 1 ? '' : 's'} in your library`
})

function buildFallbackCover(title: string): string {
  const safeTitle = encodeURIComponent(title)
  return `https://placehold.co/300x450/1c2228/f5f5f5?text=${safeTitle}`
}

function formatSubtitle(log: LibraryLogResponseDto): string {
  const parts: string[] = []

  if (typeof log.rating === 'number') {
    parts.push(`${log.rating.toFixed(1)} ★`)
  }

  if (log.platform) {
    parts.push(log.platform)
  }

  if (typeof log.hours === 'number') {
    parts.push(`${log.hours}h`)
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

  return 'Unable to load your library right now. Please try again.'
}

async function loadLibrary(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    logs.value = await getMyLibrary()
  }
  catch (error: unknown) {
    logs.value = []
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  await loadLibrary()
})
</script>

<template>
  <div>
    <SectionHeader
      icon="mdi-bookshelf"
      title="Your Library"
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

    <v-btn-toggle
      v-model="activeTab"
      mandatory
      rounded="pill"
      class="filter"
    >
      <v-btn
        v-for="tab in tabs"
        :key="tab.value"
        :value="tab.value"
        class="text-none"
      >
        {{ tab.label }}
      </v-btn>
    </v-btn-toggle>

    <v-row v-if="isLoading" dense class="mt-4">
      <v-col
        v-for="n in 10"
        :key="n"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <v-skeleton-loader
          type="image, article"
          class="library-skeleton"
        />
      </v-col>
    </v-row>

    <v-card
      v-else-if="visibleLogs.length === 0"
      class="empty-state mt-4"
      rounded="xl"
      flat
    >
      <div class="text-h6 font-weight-bold mb-2">
        No games in {{ activeTab.toLowerCase() }}
      </div>
      <div class="muted">
        Add a game from Browse or log one from a game page to start building your library.
      </div>
    </v-card>

    <v-row v-else dense class="mt-4">
      <v-col
        v-for="log in visibleLogs"
        :key="log.gameLogId"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <GamePosterCard
          :title="log.gameTitle"
          :cover-url="log.coverImageUrl ?? buildFallbackCover(log.gameTitle)"
          :subtitle="formatSubtitle(log)"
          :to="`/game/${log.gameId}`"
        />
      </v-col>
    </v-row>
  </div>
</template>

<style scoped>
.filter {
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid var(--border);
  padding: 4px;
  box-shadow: 0 10px 22px rgba(0, 0, 0, 0.22);
  flex-wrap: wrap;
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

.empty-state {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 24px;
}

.muted {
  color: var(--muted-foreground);
}

.library-skeleton {
  background: transparent;
}
</style>