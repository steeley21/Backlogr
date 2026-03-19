<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'
import { getMyLibrary } from '~/services/libraryService'
import type { LibraryLogResponseDto, LibraryStatus } from '~/types/library'

type LibraryFilter = LibraryStatus | 'All'

type LibraryTab = {
  label: string
  value: LibraryFilter
}

const tabs: LibraryTab[] = [
  { label: 'All', value: 'All' },
  { label: 'Backlog', value: 'Backlog' },
  { label: 'Playing', value: 'Playing' },
  { label: 'Played', value: 'Played' },
  { label: 'Wishlist', value: 'Wishlist' },
  { label: 'Dropped', value: 'Dropped' },
]

const fallbackCoverUrl = '/images/fallback-game-cover.svg'

const activeTab = ref<LibraryFilter>('All')
const slideDirection = ref<'left' | 'right'>('left')
const logs = ref<LibraryLogResponseDto[]>([])
const isLoading = ref(false)
const errorMessage = ref('')

const tabIndex = computed(() => tabs.findIndex(t => t.value === activeTab.value))

const visibleLogs = computed(() => {
  if (activeTab.value === 'All') {
    return logs.value
  }
  return logs.value.filter(log => log.status === activeTab.value)
})

const headerRightText = computed(() => {
  if (isLoading.value) {
    return 'Loading your library...'
  }

  const total = logs.value.length
  return `${total} game${total === 1 ? '' : 's'} in your library`
})

const emptyStateTitle = computed(() => {
  if (activeTab.value === 'All') return 'Your library is empty'
  return `No games in ${activeTab.value.toLowerCase()}`
})

const emptyStateBody = computed(() => {
  if (activeTab.value === 'All') {
    return 'Head to Browse to find games and start logging them to your library.'
  }
  return 'Add a game from Browse or log one from a game page to start building your library.'
})

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

function handleTabChange(next: LibraryFilter): void {
  const currentIndex = tabIndex.value
  const nextIndex = tabs.findIndex(t => t.value === next)
  slideDirection.value = nextIndex > currentIndex ? 'left' : 'right'
  activeTab.value = next
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
      :model-value="activeTab"
      mandatory
      rounded="pill"
      class="filter"
      @update:model-value="handleTabChange"
    >
      <v-btn
        v-for="tab in tabs"
        :key="tab.value"
        :value="tab.value"
        class="text-none tab-btn"
      >
        {{ tab.label }}
        <span
          v-if="tab.value !== 'All' && logs.filter(l => l.status === tab.value).length > 0"
          class="tab-count"
        >
          {{ logs.filter(l => l.status === tab.value).length }}
        </span>
        <span
          v-else-if="tab.value === 'All' && logs.length > 0"
          class="tab-count"
        >
          {{ logs.length }}
        </span>
      </v-btn>
    </v-btn-toggle>

    <div class="grid-shell">
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

      <Transition v-else :name="`slide-${slideDirection}`" mode="out-in">
        <div :key="activeTab">
          <v-card
            v-if="visibleLogs.length === 0"
            class="empty-state mt-4"
            rounded="xl"
            flat
          >
            <div class="text-h6 font-weight-bold mb-2">
              {{ emptyStateTitle }}
            </div>
            <div class="muted">
              {{ emptyStateBody }}
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
                :cover-url="log.coverImageUrl ?? fallbackCoverUrl"
                :subtitle="formatSubtitle(log)"
                :to="`/game/${log.gameId}`"
              />
            </v-col>
          </v-row>
        </div>
      </Transition>
    </div>
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

.tab-btn {
  display: flex;
  align-items: center;
  gap: 6px;
}

.tab-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  border-radius: 9px;
  font-size: 0.7rem;
  font-weight: 700;
  background: rgba(255, 255, 255, 0.08);
  color: var(--muted-foreground);
  line-height: 1;
  margin-left: 5px;
  transition: background 150ms ease, color 150ms ease;
}

.filter :deep(.v-btn--active) .tab-count {
  background: rgba(168, 85, 247, 0.2);
  color: #c084fc;
}

.grid-shell {
  position: relative;
  overflow: hidden;
}

/* Slide left (moving to a tab further right) */
.slide-left-enter-active,
.slide-left-leave-active,
.slide-right-enter-active,
.slide-right-leave-active {
  transition: transform 280ms cubic-bezier(0.4, 0, 0.2, 1), opacity 240ms ease;
  will-change: transform, opacity;
}

.slide-left-enter-from {
  transform: translateX(48px);
  opacity: 0;
}
.slide-left-leave-to {
  transform: translateX(-48px);
  opacity: 0;
}

/* Slide right (moving to a tab further left) */
.slide-right-enter-from {
  transform: translateX(-48px);
  opacity: 0;
}
.slide-right-leave-to {
  transform: translateX(48px);
  opacity: 0;
}

.slide-left-leave-active,
.slide-right-leave-active {
  position: absolute;
  width: 100%;
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