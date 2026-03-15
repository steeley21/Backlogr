<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AiCalloutCard from '~/components/feed/AiCalloutCard.vue'
import FeedLogCard from '~/components/feed/FeedLogCard.vue'
import FeedReviewCard from '~/components/feed/FeedReviewCard.vue'
import { getFeed } from '~/services/feedService'
import { useAuthStore } from '~/stores/auth'
import type { FeedItem, FeedReviewItem } from '~/types/feed'
import type { ReviewResponseDto } from '~/types/review'
import { getApiErrorMessage } from '~/utils/apiError'

type FeedFilter = 'all' | 'reviews' | 'logs'
type SnackbarColor = 'success' | 'error'

const authStore = useAuthStore()

const filter = ref<FeedFilter>('all')
const feedItems = ref<FeedItem[]>([])
const isLoading = ref(false)
const errorMessage = ref('')
const snackbar = ref({
  isOpen: false,
  color: 'success' as SnackbarColor,
  message: '',
})

const displayName = computed(() => {
  return authStore.displayName || authStore.userName || 'there'
})

const visibleItems = computed(() => {
  if (filter.value === 'reviews') {
    return feedItems.value.filter(item => item.type === 'review')
  }

  if (filter.value === 'logs') {
    return feedItems.value.filter(item => item.type === 'log')
  }

  return feedItems.value
})

const logCount = computed(() => {
  return feedItems.value.filter(item => item.type === 'log').length
})

const reviewCount = computed(() => {
  return feedItems.value.filter(item => item.type === 'review').length
})

function showSnackbar(message: string, color: SnackbarColor): void {
  snackbar.value = {
    isOpen: true,
    color,
    message,
  }
}

function handleFeedFeedback(message: string, color: SnackbarColor): void {
  showSnackbar(message, color)
}

function sortFeedItems(items: FeedItem[]): FeedItem[] {
  return [...items].sort((left, right) => {
    const leftDate = left.type === 'review' ? left.reviewedAt : left.updatedAt
    const rightDate = right.type === 'review' ? right.reviewedAt : right.updatedAt

    return Date.parse(rightDate) - Date.parse(leftDate)
  })
}

async function loadFeed(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    feedItems.value = await getFeed(25)
  }
  catch (error: unknown) {
    feedItems.value = []
    errorMessage.value = getApiErrorMessage(error, 'Unable to load your feed right now. Please try again.')
  }
  finally {
    isLoading.value = false
  }
}

function handleReviewUpdated(updatedReview: ReviewResponseDto): void {
  const nextItems = feedItems.value.map((item) => {
    if (item.type !== 'review' || item.id !== updatedReview.reviewId) {
      return item
    }

    const updatedItem: FeedReviewItem = {
      ...item,
      text: updatedReview.text,
      hasSpoilers: updatedReview.hasSpoilers,
      reviewedAt: updatedReview.updatedAt,
    }

    return updatedItem
  })

  feedItems.value = sortFeedItems(nextItems)
  showSnackbar('Review updated.', 'success')
}

function handleReviewDeleted(reviewId: string): void {
  feedItems.value = feedItems.value.filter(item => !(item.type === 'review' && item.id === reviewId))
  showSnackbar('Review deleted.', 'success')
}

onMounted(async () => {
  await loadFeed()
})
</script>

<template>
  <div class="page">
    <v-card class="hero" rounded="xl" flat>
      <div class="hero-overlay" />

      <div class="hero-inner">
        <div class="overline">YOUR ACTIVITY</div>
        <div class="hero-title">Welcome back, {{ displayName }}</div>
        <div class="hero-sub muted">
          Your feed includes your own latest logs and reviews, plus activity from people you follow.
        </div>

        <div class="hero-actions">
          <v-btn color="primary" rounded="pill" class="text-none px-6" to="/browse">
            Browse Games
          </v-btn>

          <v-btn variant="text" rounded="pill" class="text-none" to="/library">
            View Library
          </v-btn>
        </div>
      </div>
    </v-card>

    <v-row align="start" class="content" dense>
      <v-col cols="12" md="8" class="feed-col">
        <div class="section-head">
          <div class="title">
            <v-icon icon="mdi-account-group" color="primary" size="20" />
            <span>Your Feed</span>
          </div>

          <div class="section-actions">
            <v-btn
              variant="text"
              rounded="pill"
              class="text-none"
              prepend-icon="mdi-refresh"
              :loading="isLoading"
              @click="loadFeed"
            >
              Refresh
            </v-btn>

            <v-btn-toggle v-model="filter" mandatory density="comfortable" rounded="pill" class="filter">
              <v-btn value="all" class="text-none">All</v-btn>
              <v-btn value="reviews" class="text-none">Reviews</v-btn>
              <v-btn value="logs" class="text-none">Logs</v-btn>
            </v-btn-toggle>
          </div>
        </div>

        <v-alert
          v-if="errorMessage"
          type="error"
          variant="tonal"
          rounded="lg"
          class="mb-4"
        >
          {{ errorMessage }}
        </v-alert>

        <div v-if="isLoading" class="d-flex flex-column ga-4">
          <v-skeleton-loader
            v-for="n in 3"
            :key="n"
            type="article"
            class="feed-skeleton"
          />
        </div>

        <v-card
          v-else-if="visibleItems.length === 0"
          class="empty-state"
          rounded="xl"
          flat
        >
          <div class="text-h6 font-weight-bold mb-2">Your feed is quiet right now</div>
          <div class="muted mb-4">
            Log a game or write a review to see your own activity here. When you follow other members, their activity will appear too.
          </div>

          <div class="d-flex ga-3 flex-wrap">
            <v-btn color="primary" rounded="pill" class="text-none px-6" to="/browse">
              Find a game
            </v-btn>

            <v-btn variant="text" rounded="pill" class="text-none" to="/library">
              Open library
            </v-btn>
          </div>
        </v-card>

        <div v-else class="d-flex flex-column ga-4">
          <template v-for="item in visibleItems" :key="item.id">
            <FeedReviewCard
              v-if="item.type === 'review'"
              :item="item"
              @updated="handleReviewUpdated"
              @deleted="handleReviewDeleted"
              @feedback="handleFeedFeedback"
            />
            <FeedLogCard v-else :item="item" />
          </template>
        </div>
      </v-col>

      <v-col cols="12" md="4" class="rail-col">
        <v-card class="stats-card rail-block" rounded="xl" flat>
          <div class="section-head compact">
            <div class="title">
              <v-icon icon="mdi-chart-box-outline" color="primary" size="20" />
              <span>Feed Snapshot</span>
            </div>
          </div>

          <div class="stats-grid">
            <div class="stat">
              <div class="num">{{ feedItems.length }}</div>
              <div class="label">Items</div>
            </div>

            <div class="stat">
              <div class="num">{{ reviewCount }}</div>
              <div class="label">Reviews</div>
            </div>

            <div class="stat">
              <div class="num">{{ logCount }}</div>
              <div class="label">Logs</div>
            </div>
          </div>
        </v-card>

        <AiCalloutCard class="rail-block" />

        <v-card class="info-card rail-block" rounded="xl" flat>
          <div class="section-head compact">
            <div class="title">
              <v-icon icon="mdi-information-outline" color="primary" size="20" />
              <span>How the feed works</span>
            </div>
          </div>

          <div class="muted info-copy">
            You’ll see your own recent activity here first. As you follow other members, their logs and reviews will be mixed into the same timeline.
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-snackbar
      v-model="snackbar.isOpen"
      :color="snackbar.color"
      timeout="2800"
      location="bottom right"
    >
      {{ snackbar.message }}
    </v-snackbar>
  </div>
</template>

<style scoped>
.page {
  padding-top: 6px;
}

.content {
  margin-top: 18px;
}

.hero {
  position: relative;
  border: 1px solid var(--border);
  border-radius: calc(var(--radius) + 4px) !important;
  overflow: hidden;
  min-height: 220px;
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.35);
  background:
    radial-gradient(900px 320px at 20% 10%, rgba(168, 85, 247, 0.18), transparent 55%),
    linear-gradient(135deg, rgba(20, 24, 28, 0.96), rgba(28, 34, 40, 0.94));
}

.hero::after {
  content: "";
  position: absolute;
  inset: 0;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: calc(var(--radius) + 4px);
  pointer-events: none;
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(600px 220px at 80% 0%, rgba(168, 85, 247, 0.12), transparent 60%);
}

.hero-inner {
  position: relative;
  padding: 36px 36px;
  max-width: 640px;
}

.overline {
  color: var(--primary);
  font-weight: 700;
  letter-spacing: 0.18em;
  font-size: 0.75rem;
  margin-bottom: 10px;
}

.hero-title {
  font-size: 2.2rem;
  font-weight: 800;
  line-height: 1.05;
  margin-bottom: 10px;
  color: var(--foreground);
}

.hero-sub {
  max-width: 560px;
  line-height: 1.55;
  margin-bottom: 18px;
}

.hero-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
  gap: 12px;
}

.section-head.compact {
  margin-bottom: 12px;
}

.section-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 700;
}

.filter {
  background: rgba(255, 255, 255, 0.04);
}

.stats-card,
.info-card,
.empty-state {
  background: color-mix(in srgb, var(--card) 88%, black);
  border: 1px solid var(--border);
}

.stats-card,
.info-card {
  padding: 20px;
}

.rail-block + .rail-block {
  margin-top: 18px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.stat {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 18px;
  padding: 14px;
}

.num {
  font-size: 1.6rem;
  font-weight: 800;
  color: var(--foreground);
}

.label,
.muted {
  color: var(--muted-foreground);
}

.empty-state {
  padding: 26px;
}

.info-copy {
  line-height: 1.6;
}

.feed-skeleton {
  border-radius: 24px;
  overflow: hidden;
}

@media (max-width: 960px) {
  .hero-inner {
    padding: 28px 24px;
  }

  .hero-title {
    font-size: 1.9rem;
  }

  .stats-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 600px) {
  .section-head,
  .section-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .hero-title {
    font-size: 1.7rem;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>
