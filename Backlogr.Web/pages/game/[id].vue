<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { navigateTo, useRoute } from '#imports'
import FeedLogCard from '~/components/feed/FeedLogCard.vue'
import FeedReviewCard from '~/components/feed/FeedReviewCard.vue'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import StarRating from '~/components/shared/StarRating.vue'
import {
  getGameActivity,
  getGameById,
  getGameReviews,
  getGameViewerState,
} from '~/services/gameService'
import type { FeedItem, FeedReviewItem } from '~/types/feed'
import type { GameDetailResponseDto, GameViewerStateResponseDto } from '~/types/game'
import type { ReviewResponseDto } from '~/types/review'
import { getApiErrorMessage } from '~/utils/apiError'

type GameDetailTab = 'about' | 'reviews' | 'activity'
type SnackbarColor = 'success' | 'error'

const route = useRoute()

const game = ref<GameDetailResponseDto | null>(null)
const viewerState = ref<GameViewerStateResponseDto | null>(null)
const reviewItems = ref<FeedReviewItem[]>([])
const activityItems = ref<FeedItem[]>([])

const isLoadingGame = ref(false)
const isLoadingViewerState = ref(false)
const isLoadingReviews = ref(false)
const isLoadingActivity = ref(false)

const gameErrorMessage = ref('')
const viewerErrorMessage = ref('')
const reviewsErrorMessage = ref('')
const activityErrorMessage = ref('')

const tab = ref<GameDetailTab>('about')
const snackbar = ref({
  isOpen: false,
  color: 'success' as SnackbarColor,
  message: '',
})

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

const hasViewerContent = computed(() => {
  return Boolean(viewerState.value?.log || viewerState.value?.review)
})

const myLogStatusColor = computed(() => {
  const status = viewerState.value?.log?.status

  switch (status) {
    case 'Played':
      return 'primary'
    case 'Playing':
      return 'success'
    case 'Wishlist':
      return 'secondary'
    case 'Dropped':
      return 'error'
    default:
      return 'default'
  }
})

const communitySummary = computed(() => {
  const reviewCount = reviewItems.value.length
  const activityCount = activityItems.value.length

  return `${reviewCount} review${reviewCount === 1 ? '' : 's'} • ${activityCount} recent activit${activityCount === 1 ? 'y' : 'ies'}`
})

function showSnackbar(message: string, color: SnackbarColor): void {
  snackbar.value = {
    isOpen: true,
    color,
    message,
  }
}

function sortFeedItems(items: FeedItem[]): FeedItem[] {
  return [...items].sort((left, right) => {
    const leftDate = left.type === 'review' ? left.reviewedAt : left.updatedAt
    const rightDate = right.type === 'review' ? right.reviewedAt : right.updatedAt

    return Date.parse(rightDate) - Date.parse(leftDate)
  })
}

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'Unknown'
  }

  return new Date(value).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

function getSectionFallbackMessage(section: 'game' | 'viewer' | 'reviews' | 'activity'): string {
  switch (section) {
    case 'viewer':
      return 'Unable to load your log state for this game right now.'
    case 'reviews':
      return 'Unable to load reviews for this game right now.'
    case 'activity':
      return 'Unable to load activity for this game right now.'
    default:
      return 'Unable to load this game right now. Please try again.'
  }
}

async function loadGame(): Promise<boolean> {
  if (!gameId.value) {
    game.value = null
    gameErrorMessage.value = 'Invalid game id.'
    return false
  }

  isLoadingGame.value = true
  gameErrorMessage.value = ''

  try {
    game.value = await getGameById(gameId.value)
    return true
  }
  catch (error: unknown) {
    game.value = null
    gameErrorMessage.value = getApiErrorMessage(error, getSectionFallbackMessage('game'))
    return false
  }
  finally {
    isLoadingGame.value = false
  }
}

async function loadViewerState(): Promise<void> {
  if (!gameId.value) {
    viewerState.value = null
    return
  }

  isLoadingViewerState.value = true
  viewerErrorMessage.value = ''

  try {
    viewerState.value = await getGameViewerState(gameId.value)
  }
  catch (error: unknown) {
    viewerState.value = null
    viewerErrorMessage.value = getApiErrorMessage(error, getSectionFallbackMessage('viewer'))
  }
  finally {
    isLoadingViewerState.value = false
  }
}

async function loadReviews(): Promise<void> {
  if (!gameId.value) {
    reviewItems.value = []
    return
  }

  isLoadingReviews.value = true
  reviewsErrorMessage.value = ''

  try {
    reviewItems.value = await getGameReviews(gameId.value, 20)
  }
  catch (error: unknown) {
    reviewItems.value = []
    reviewsErrorMessage.value = getApiErrorMessage(error, getSectionFallbackMessage('reviews'))
  }
  finally {
    isLoadingReviews.value = false
  }
}

async function loadActivity(): Promise<void> {
  if (!gameId.value) {
    activityItems.value = []
    return
  }

  isLoadingActivity.value = true
  activityErrorMessage.value = ''

  try {
    activityItems.value = await getGameActivity(gameId.value, 25)
  }
  catch (error: unknown) {
    activityItems.value = []
    activityErrorMessage.value = getApiErrorMessage(error, getSectionFallbackMessage('activity'))
  }
  finally {
    isLoadingActivity.value = false
  }
}

async function loadCommunitySections(): Promise<void> {
  await Promise.all([
    loadViewerState(),
    loadReviews(),
    loadActivity(),
  ])
}

async function loadPage(): Promise<void> {
  viewerState.value = null
  reviewItems.value = []
  activityItems.value = []
  viewerErrorMessage.value = ''
  reviewsErrorMessage.value = ''
  activityErrorMessage.value = ''

  const loaded = await loadGame()

  if (!loaded) {
    return
  }

  await loadCommunitySections()
}

async function goToLog(): Promise<void> {
  if (!gameId.value) {
    return
  }

  await navigateTo({
    path: '/log',
    query: { gameId: gameId.value },
  })
}

function handleReviewUpdated(updatedReview: ReviewResponseDto): void {
  reviewItems.value = sortFeedItems(
    reviewItems.value.map((item) => {
      if (item.id !== updatedReview.reviewId) {
        return item
      }

      return {
        ...item,
        text: updatedReview.text,
        hasSpoilers: updatedReview.hasSpoilers,
        reviewedAt: updatedReview.updatedAt,
      }
    }),
  ).filter((item): item is FeedReviewItem => item.type === 'review')

  activityItems.value = sortFeedItems(
    activityItems.value.map((item) => {
      if (item.type !== 'review' || item.id !== updatedReview.reviewId) {
        return item
      }

      return {
        ...item,
        text: updatedReview.text,
        hasSpoilers: updatedReview.hasSpoilers,
        reviewedAt: updatedReview.updatedAt,
      }
    }),
  )

  if (viewerState.value?.review?.reviewId === updatedReview.reviewId) {
    viewerState.value = {
      ...viewerState.value,
      review: updatedReview,
    }
  }

  showSnackbar('Review updated.', 'success')
}

function handleReviewDeleted(reviewId: string): void {
  reviewItems.value = reviewItems.value.filter(item => item.id !== reviewId)
  activityItems.value = activityItems.value.filter(item => !(item.type === 'review' && item.id === reviewId))

  if (viewerState.value?.review?.reviewId === reviewId) {
    viewerState.value = {
      ...viewerState.value,
      review: null,
    }
  }

  showSnackbar('Review deleted.', 'success')
}

function handleFeedFeedback(message: string, color: SnackbarColor): void {
  showSnackbar(message, color)
}

watch(
  () => gameId.value,
  async () => {
    await loadPage()
  },
  { immediate: true },
)
</script>

<template>
  <div>
    <v-skeleton-loader
      v-if="isLoadingGame"
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

      <v-card class="panel mt-5" rounded="xl" flat>
        <SectionHeader
          icon="mdi-controller-classic-outline"
          title="My log"
          right-text="Your history with this game"
        />

        <v-skeleton-loader
          v-if="isLoadingViewerState"
          type="article"
          class="detail-skeleton"
        />

        <v-alert
          v-else-if="viewerErrorMessage"
          type="error"
          variant="tonal"
          rounded="lg"
          class="mb-4"
        >
          {{ viewerErrorMessage }}
        </v-alert>

        <div v-else-if="hasViewerContent" class="my-log-grid">
          <div class="my-log-main">
            <div class="my-log-topline">
              <v-chip
                v-if="viewerState?.log?.status"
                :color="myLogStatusColor"
                variant="tonal"
                size="small"
              >
                {{ viewerState.log.status }}
              </v-chip>

              <StarRating
                v-if="typeof viewerState?.log?.rating === 'number'"
                :rating="viewerState.log.rating"
                :size="18"
              />

              <span v-if="viewerState?.log?.platform" class="muted">{{ viewerState.log.platform }}</span>
              <span v-if="typeof viewerState?.log?.hours === 'number'" class="muted">{{ viewerState.log.hours }}h</span>
              <span v-if="viewerState?.review" class="muted">Reviewed {{ formatDate(viewerState.review.updatedAt) }}</span>
            </div>

            <div v-if="viewerState?.review" class="review-shell mt-4">
              <div class="text-subtitle-1 font-weight-bold mb-2">Your review</div>
              <p class="review-text">
                {{ viewerState.review.text }}
              </p>
              <div class="d-flex ga-2 flex-wrap mt-3">
                <v-chip
                  v-if="viewerState.review.hasSpoilers"
                  color="warning"
                  variant="tonal"
                  size="small"
                >
                  Spoilers
                </v-chip>
                <v-chip size="small" variant="outlined">
                  Updated {{ formatDate(viewerState.review.updatedAt) }}
                </v-chip>
              </div>
            </div>
            <div v-else class="muted mt-4">
              You have this game in your library, but you have not written a review yet.
            </div>

            <div class="d-flex ga-3 flex-wrap mt-5">
              <v-btn
                v-if="!viewerState?.review"
                color="primary"
                rounded="pill"
                class="text-none px-5"
                @click="goToLog"
              >
                {{ viewerState?.log ? 'Add a review' : 'Log this game' }}
              </v-btn>

              <v-btn
                variant="text"
                rounded="pill"
                class="text-none"
                to="/library"
              >
                Open Library
              </v-btn>
            </div>
          </div>

          <div class="meta-card">
            <div class="meta-row">
              <div class="meta-label">Status</div>
              <div class="meta-value">{{ viewerState?.log?.status ?? 'Not in your library yet' }}</div>
            </div>

            <div class="meta-row">
              <div class="meta-label">Started</div>
              <div class="meta-value">{{ formatDate(viewerState?.log?.startedAt) }}</div>
            </div>

            <div class="meta-row">
              <div class="meta-label">Finished</div>
              <div class="meta-value">{{ formatDate(viewerState?.log?.finishedAt) }}</div>
            </div>

            <div class="meta-row">
              <div class="meta-label">Notes</div>
              <div class="meta-value">{{ viewerState?.log?.notes || 'No personal notes yet.' }}</div>
            </div>
          </div>
        </div>

        <div v-else class="empty-state mt-1">
          <div class="text-h6 font-weight-bold mb-2">Nothing logged here yet</div>
          <div class="muted">
            Add this game to your library or write your first review to start building your history with it.
          </div>

          <div class="mt-4">
            <v-btn
              color="primary"
              rounded="pill"
              class="text-none px-5"
              @click="goToLog"
            >
              Log this game
            </v-btn>
          </div>
        </div>
      </v-card>

      <div class="mt-5">
        <div class="tabs-row">
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

          <div class="muted tabs-summary">
            {{ communitySummary }}
          </div>
        </div>

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

            <v-skeleton-loader
              v-if="isLoadingReviews"
              type="article, article"
              class="detail-skeleton"
            />

            <v-alert
              v-else-if="reviewsErrorMessage"
              type="error"
              variant="tonal"
              rounded="lg"
              class="mb-4"
            >
              {{ reviewsErrorMessage }}
            </v-alert>

            <div v-else-if="reviewItems.length === 0" class="empty-state compact-empty">
              <div class="text-subtitle-1 font-weight-bold mb-2">No reviews yet</div>
              <div class="muted">
                Be the first person to share what you thought about {{ game.title }}.
              </div>
            </div>

            <div v-else class="stack">
              <FeedReviewCard
                v-for="item in reviewItems"
                :key="item.id"
                :item="item"
                @updated="handleReviewUpdated"
                @deleted="handleReviewDeleted"
                @feedback="handleFeedFeedback"
              />
            </div>
          </div>

          <div v-else>
            <SectionHeader icon="mdi-history" title="Activity" />

            <v-skeleton-loader
              v-if="isLoadingActivity"
              type="article, article"
              class="detail-skeleton"
            />

            <v-alert
              v-else-if="activityErrorMessage"
              type="error"
              variant="tonal"
              rounded="lg"
              class="mb-4"
            >
              {{ activityErrorMessage }}
            </v-alert>

            <div v-else-if="activityItems.length === 0" class="empty-state compact-empty">
              <div class="text-subtitle-1 font-weight-bold mb-2">No recent activity yet</div>
              <div class="muted">
                Once people log or review {{ game.title }}, their activity will show up here.
              </div>
            </div>

            <div v-else class="stack">
              <template v-for="item in activityItems" :key="`${item.type}-${item.id}`">
                <FeedReviewCard
                  v-if="item.type === 'review'"
                  :item="item"
                  @updated="handleReviewUpdated"
                  @deleted="handleReviewDeleted"
                  @feedback="handleFeedFeedback"
                />

                <FeedLogCard
                  v-else
                  :item="item"
                />
              </template>
            </div>
          </div>
        </v-card>
      </div>
    </template>

    <template v-else>
      <v-alert
        v-if="gameErrorMessage"
        type="error"
        variant="tonal"
        rounded="lg"
        class="mb-4"
      >
        {{ gameErrorMessage }}
      </v-alert>

      <v-card class="panel" rounded="xl" flat>
        <div class="text-h6 font-weight-bold mb-2">Game unavailable</div>
        <div class="muted">
          We could not load this game.
        </div>
      </v-card>
    </template>

    <v-snackbar
      v-model="snackbar.isOpen"
      :color="snackbar.color"
      timeout="2400"
      location="bottom right"
    >
      {{ snackbar.message }}
    </v-snackbar>
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

.tabs-row {
  display: flex;
  justify-content: space-between;
  gap: 14px;
  align-items: center;
  flex-wrap: wrap;
}

.tabs-summary {
  font-size: 0.95rem;
}

.about-grid,
.my-log-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 280px;
  gap: 20px;
  align-items: start;
}

.about-main,
.my-log-main {
  min-width: 0;
}

.my-log-topline {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.review-shell {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: var(--radius);
  padding: 16px;
}

.review-text {
  color: var(--foreground);
  line-height: 1.65;
  white-space: pre-wrap;
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

.stack {
  display: grid;
  gap: 14px;
}

.empty-state {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: var(--radius);
  padding: 20px;
}

.compact-empty {
  margin-top: 4px;
}

.detail-skeleton {
  background: transparent;
}

@media (max-width: 860px) {
  .about-grid,
  .my-log-grid {
    grid-template-columns: 1fr;
  }

  .hero-inner {
    padding: 28px 24px;
  }
}

@media (max-width: 600px) {
  .hero {
    min-height: 200px;
  }

  .hero-inner {
    padding: 22px 18px;
  }

  .hero-inner :deep(.text-h4) {
    font-size: 1.4rem !important;
  }

  .panel {
    padding: 14px;
  }

  /* Hide the summary text on mobile, keep just the toggle */
  .tabs-summary {
    display: none;
  }

  .tabs-row {
    justify-content: flex-start;
  }

  .filter :deep(.v-btn) {
    padding-inline: 12px;
    font-size: 0.82rem;
  }

  .meta-card {
    padding: 12px;
  }

  .empty-state {
    padding: 16px;
  }
}
</style>
