<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AiCalloutCard from '~/components/feed/AiCalloutCard.vue'
import FeedLogCard from '~/components/feed/FeedLogCard.vue'
import FeedReviewCard from '~/components/feed/FeedReviewCard.vue'
import { getFeed } from '~/services/feedService'
import { useAuthStore } from '~/stores/auth'
import type { FeedItem, FeedReviewItem, FeedScope } from '~/types/feed'
import type { ReviewResponseDto } from '~/types/review'
import { getApiErrorMessage } from '~/utils/apiError'

type FeedFilter = 'all' | 'reviews' | 'logs'
type SnackbarColor = 'success' | 'error'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()

const filter = ref<FeedFilter>('all')
const activeTab = ref<FeedScope>('for-you')
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

const tabLabel = computed(() => {
  return activeTab.value === 'for-you' ? 'For You' : 'Following'
})

const tabIcon = computed(() => {
  return activeTab.value === 'for-you' ? 'mdi-earth' : 'mdi-account-group'
})

const heroOverline = computed(() => {
  return activeTab.value === 'for-you' ? 'DISCOVER ACTIVITY' : 'FOLLOWING ACTIVITY'
})

const heroSubtitle = computed(() => {
  if (activeTab.value === 'for-you') {
    return 'See the latest logs and reviews from across Backlogr, including your own activity.'
  }

  return 'Stay caught up with people you follow, while keeping your own recent logs and reviews in the mix.'
})

const emptyTitle = computed(() => {
  if (activeTab.value === 'for-you') {
    return 'The For You feed is quiet right now'
  }

  return 'The Following feed is quiet right now'
})

const emptyMessage = computed(() => {
  if (activeTab.value === 'for-you') {
    return 'Once members start logging games and posting reviews, you’ll see the latest activity here.'
  }

  return 'Log a game or write a review to see your own activity here. As you follow more members, their activity will appear too.'
})

const infoCopy = computed(() => {
  if (activeTab.value === 'for-you') {
    return 'For You shows the latest activity across the platform, so you can discover new reviews, logs, and members to follow.'
  }

  return 'Following narrows the feed to activity from people you follow, while still keeping your own recent posts visible.'
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

function getRouteTabValue(value: unknown): string | undefined {
  if (typeof value === 'string') {
    return value
  }

  if (Array.isArray(value) && typeof value[0] === 'string') {
    return value[0]
  }

  return undefined
}

function isFeedScope(value: unknown): value is FeedScope {
  return value === 'for-you' || value === 'following'
}

function parseFeedTab(value: unknown): FeedScope {
  const tab = getRouteTabValue(value)

  if (tab === 'following') {
    return 'following'
  }

  return 'for-you'
}

async function replaceTabQuery(tab: FeedScope): Promise<void> {
  await router.replace({
    query: {
      ...route.query,
      tab,
    },
  })
}

async function loadFeed(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    feedItems.value = await getFeed({
      take: 25,
      scope: activeTab.value,
    })
  }
  catch (error: unknown) {
    feedItems.value = []

    const fallbackMessage = activeTab.value === 'for-you'
      ? 'Unable to load the For You feed right now. Please try again.'
      : 'Unable to load the Following feed right now. Please try again.'

    errorMessage.value = getApiErrorMessage(error, fallbackMessage)
  }
  finally {
    isLoading.value = false
  }
}

async function handleTabChange(value: unknown): Promise<void> {
  if (!isFeedScope(value) || value === activeTab.value) {
    return
  }

  await replaceTabQuery(value)
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

watch(
  () => route.query.tab,
  async (routeTabValue) => {
    const rawTab = getRouteTabValue(routeTabValue)
    const normalizedTab = parseFeedTab(routeTabValue)

    activeTab.value = normalizedTab

    if (rawTab !== normalizedTab) {
      await replaceTabQuery(normalizedTab)
      return
    }

    await loadFeed()
  },
  { immediate: true },
)
</script>

<template>
  <div class="page">
    <v-card class="hero" rounded="xl" flat>
      <div class="hero-overlay" />

      <div class="hero-inner">
        <div class="overline">{{ heroOverline }}</div>
        <div class="hero-title">Welcome back, {{ displayName }}</div>
        <div class="hero-sub muted">
          {{ heroSubtitle }}
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
            <v-icon :icon="tabIcon" color="primary" size="20" />
            <span>{{ tabLabel }} Feed</span>
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

            <v-btn-toggle
              :model-value="activeTab"
              mandatory
              density="comfortable"
              rounded="pill"
              class="filter"
              @update:model-value="handleTabChange"
            >
              <v-btn value="for-you" class="text-none">For You</v-btn>
              <v-btn value="following" class="text-none">Following</v-btn>
            </v-btn-toggle>

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
          <div class="text-h6 font-weight-bold mb-2">{{ emptyTitle }}</div>
          <div class="muted mb-4">
            {{ emptyMessage }}
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
              <span>How this tab works</span>
            </div>
          </div>

          <div class="muted info-copy">
            {{ infoCopy }}
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
  border: 1px solid rgba(168, 85, 247, 0.15);
  border-radius: calc(var(--radius) + 4px) !important;
  overflow: hidden;
  min-height: 230px;
  box-shadow:
    0 0 0 1px rgba(168, 85, 247, 0.06),
    0 24px 56px rgba(0, 0, 0, 0.5),
    0 4px 16px rgba(168, 85, 247, 0.08);
  background:
    radial-gradient(ellipse 800px 300px at 15% 0%, rgba(168, 85, 247, 0.22), transparent 60%),
    radial-gradient(ellipse 400px 200px at 85% 100%, rgba(88, 28, 135, 0.18), transparent 55%),
    linear-gradient(145deg, #131720 0%, #0f1217 100%);
}

/* Subtle grid overlay for texture */
.hero::before {
  content: '';
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(255,255,255,0.025) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,0.025) 1px, transparent 1px);
  background-size: 48px 48px;
  mask-image: linear-gradient(to bottom right, rgba(0,0,0,0.6), transparent 60%);
  pointer-events: none;
}

.hero::after {
  content: "";
  position: absolute;
  inset: 0;
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: calc(var(--radius) + 4px);
  pointer-events: none;
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(500px 180px at 90% 10%, rgba(168, 85, 247, 0.1), transparent 60%);
}

.hero-inner {
  position: relative;
  padding: 40px 40px;
  max-width: 660px;
}

.overline {
  color: var(--primary);
  font-weight: 800;
  letter-spacing: 0.22em;
  font-size: 0.7rem;
  margin-bottom: 12px;
  text-transform: uppercase;
}

.hero-title {
  font-size: 2.3rem;
  font-weight: 800;
  line-height: 1.05;
  margin-bottom: 12px;
  color: var(--foreground);
  letter-spacing: -0.02em;
}

.hero-sub {
  max-width: 520px;
  line-height: 1.6;
  margin-bottom: 24px;
  font-size: 0.95rem;
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
  font-size: 0.95rem;
}

.filter {
  background: rgba(255, 255, 255, 0.04);
}

.stats-card,
.info-card,
.empty-state {
  background: var(--card);
  border: 1px solid var(--border);
}

.stats-card,
.info-card {
  padding: 20px;
}

.rail-block + .rail-block {
  margin-top: 16px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}

.stat {
  background: rgba(168, 85, 247, 0.06);
  border: 1px solid rgba(168, 85, 247, 0.12);
  border-radius: 14px;
  padding: 14px 12px;
  text-align: center;
  transition: background 180ms ease, border-color 180ms ease;
}

.stat:hover {
  background: rgba(168, 85, 247, 0.1);
  border-color: rgba(168, 85, 247, 0.22);
}

.num {
  font-size: 1.7rem;
  font-weight: 800;
  color: var(--foreground);
  line-height: 1;
  letter-spacing: -0.02em;
}

.label,
.muted {
  color: var(--muted-foreground);
  font-size: 0.82rem;
  margin-top: 3px;
}

.empty-state {
  padding: 26px;
}

.info-copy {
  line-height: 1.65;
  font-size: 0.9rem;
}

.feed-skeleton {
  border-radius: 24px;
  overflow: hidden;
}

@media (max-width: 960px) {
  .hero-inner {
    padding: 30px 26px;
  }

  .hero-title {
    font-size: 1.9rem;
  }

  .stats-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 600px) {
  .hero-inner {
    padding: 24px 20px;
  }

  .hero-title {
    font-size: 1.5rem;
    letter-spacing: -0.01em;
  }

  .hero-sub {
    font-size: 0.88rem;
  }

  .section-head {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }

  .section-actions {
    flex-direction: row;
    flex-wrap: wrap;
    justify-content: flex-start;
    gap: 8px;
  }

  /* Stack filter toggles vertically and let them fill width */
  .section-actions :deep(.v-btn-toggle) {
    flex: 1 1 auto;
  }

  .stats-grid {
    grid-template-columns: repeat(3, 1fr);
  }

  .stat {
    padding: 10px 8px;
    border-radius: 10px;
    text-align: center;
  }

  .num {
    font-size: 1.3rem;
  }

  .label {
    font-size: 0.72rem;
    margin-top: 2px;
  }

  .stats-card,
  .info-card {
    padding: 14px;
  }
}
</style>