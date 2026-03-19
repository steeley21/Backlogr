<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import FeedReviewCommentThread from '~/components/feed/FeedReviewCommentThread.vue'
import FeedReviewDeleteDialog from '~/components/feed/FeedReviewDeleteDialog.vue'
import FeedReviewEditDialog from '~/components/feed/FeedReviewEditDialog.vue'
import StarRating from '~/components/shared/StarRating.vue'
import { likeReview, unlikeReview } from '~/services/reviewService'
import type { FeedReviewItem } from '~/types/feed'
import type { ReviewResponseDto } from '~/types/review'
import { getApiErrorMessage } from '~/utils/apiError'

const props = defineProps<{ item: FeedReviewItem }>()

const emit = defineEmits<{
  updated: [review: ReviewResponseDto]
  deleted: [reviewId: string]
  feedback: [message: string, color: 'success' | 'error']
}>()

const isEditDialogOpen = ref(false)
const isDeleteDialogOpen = ref(false)
const isCommentsOpen = ref(false)
const hasOpenedComments = ref(false)
const isSubmittingLike = ref(false)
const liked = ref(Boolean(props.item.liked))
const likeCount = ref(props.item.likeCount)
const commentCount = ref(props.item.commentCount)

watch(
  () => props.item.liked,
  (nextValue) => {
    liked.value = Boolean(nextValue)
  },
)

watch(
  () => props.item.likeCount,
  (nextValue) => {
    likeCount.value = nextValue
  },
)

watch(
  () => props.item.commentCount,
  (nextValue) => {
    commentCount.value = nextValue
  },
)

const dateLabel = computed(() => {
  const d = new Date(props.item.reviewedAt)
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
})

const profilePath = computed(() => {
  return `/u/${props.item.user.userName}`
})

const avatarInitials = computed(() => {
  const source = props.item.user.displayName || props.item.user.userName || 'B'
  const parts = source
    .trim()
    .split(/\s+/)
    .filter(Boolean)

  if (parts.length === 0) {
    return 'B'
  }

  if (parts.length === 1) {
    return parts[0].slice(0, 2).toUpperCase()
  }

  return `${parts[0][0] ?? ''}${parts[1][0] ?? ''}`.toUpperCase()
})

const commentButtonLabel = computed(() => {
  return isCommentsOpen.value ? 'Hide comments' : 'Show comments'
})

function openEditDialog(): void {
  isEditDialogOpen.value = true
}

function openDeleteDialog(): void {
  isDeleteDialogOpen.value = true
}

function handleReviewSaved(review: ReviewResponseDto): void {
  emit('updated', review)
}

function handleReviewDeleted(reviewId: string): void {
  emit('deleted', reviewId)
}

async function toggleLike(): Promise<void> {
  if (isSubmittingLike.value) {
    return
  }

  const wasLiked = liked.value
  const previousLikeCount = likeCount.value

  liked.value = !wasLiked
  likeCount.value = Math.max(0, previousLikeCount + (wasLiked ? -1 : 1))
  isSubmittingLike.value = true

  try {
    if (wasLiked) {
      await unlikeReview(props.item.id)
    }
    else {
      await likeReview(props.item.id)
    }
  }
  catch (error: unknown) {
    liked.value = wasLiked
    likeCount.value = previousLikeCount
    emit('feedback', getApiErrorMessage(error, 'Unable to update the like right now.'), 'error')
  }
  finally {
    isSubmittingLike.value = false
  }
}

function toggleComments(): void {
  isCommentsOpen.value = !isCommentsOpen.value

  if (isCommentsOpen.value) {
    hasOpenedComments.value = true
  }
}

function handleCommentCountUpdated(nextCount: number): void {
  commentCount.value = nextCount
}

function handleThreadFeedback(message: string, color: 'success' | 'error'): void {
  emit('feedback', message, color)
}
</script>

<template>
  <v-card class="card" rounded="xl" flat>
    <div class="top-row">
      <NuxtLink :to="profilePath" class="avatar-link" :aria-label="`Open ${item.user.displayName}`">
        <v-avatar size="44" class="avatar">
          <v-img v-if="item.user.avatarUrl" :src="item.user.avatarUrl" :alt="item.user.displayName" cover />
          <span v-else class="avatar-fallback">{{ avatarInitials }}</span>
        </v-avatar>
      </NuxtLink>

      <div class="content">
        <div class="headline">
          <NuxtLink :to="profilePath" class="name-link">
            {{ item.user.displayName }}
          </NuxtLink>
          <span class="muted">reviewed</span>
          <NuxtLink :to="`/game/${item.game.gameId}`" class="game-link">
            {{ item.game.title }}
          </NuxtLink>
        </div>

        <div class="subline">
          <StarRating v-if="typeof item.rating === 'number'" :rating="item.rating" />
          <v-chip
            v-if="item.hasSpoilers"
            size="small"
            color="warning"
            variant="tonal"
          >
            Spoilers
          </v-chip>
          <span class="muted">{{ dateLabel }}</span>
        </div>
      </div>

      <div class="top-actions">
        <div v-if="item.game.coverUrl" class="thumb">
          <v-img :src="item.game.coverUrl" cover />
        </div>

        <v-menu v-if="item.isOwner" location="bottom end">
          <template #activator="{ props: menuProps }">
            <v-btn
              v-bind="menuProps"
              icon="mdi-dots-horizontal"
              variant="text"
              density="comfortable"
              aria-label="Review actions"
            />
          </template>

          <v-list class="action-list" density="comfortable">
            <v-list-item
              prepend-icon="mdi-pencil-outline"
              title="Edit review"
              @click="openEditDialog"
            />
            <v-list-item
              prepend-icon="mdi-delete-outline"
              title="Delete review"
              base-color="error"
              @click="openDeleteDialog"
            />
          </v-list>
        </v-menu>
      </div>
    </div>

    <div class="divider" />

    <p class="body">
      {{ item.text }}
    </p>

    <div class="d-flex align-center actions">
      <v-btn
        icon
        variant="text"
        density="comfortable"
        :aria-label="liked ? 'Unlike review' : 'Like review'"
        :loading="isSubmittingLike"
        @click="toggleLike"
      >
        <v-icon :icon="liked ? 'mdi-heart' : 'mdi-heart-outline'" :color="liked ? 'primary' : undefined" />
      </v-btn>
      <span class="muted mr-4">{{ likeCount }}</span>

      <v-btn
        icon
        variant="text"
        density="comfortable"
        :aria-label="commentButtonLabel"
        @click="toggleComments"
      >
        <v-icon :icon="isCommentsOpen ? 'mdi-message' : 'mdi-message-outline'" />
      </v-btn>
      <span class="muted">{{ commentCount }}</span>
    </div>

    <v-expand-transition>
      <div v-show="isCommentsOpen" class="comments-shell">
        <FeedReviewCommentThread
          v-if="hasOpenedComments"
          :review-id="item.id"
          @count-updated="handleCommentCountUpdated"
          @feedback="handleThreadFeedback"
        />
      </div>
    </v-expand-transition>
  </v-card>

  <FeedReviewEditDialog
    v-model="isEditDialogOpen"
    :review-id="item.id"
    :game-title="item.game.title"
    :initial-text="item.text"
    :initial-has-spoilers="item.hasSpoilers ?? false"
    @saved="handleReviewSaved"
  />

  <FeedReviewDeleteDialog
    v-model="isDeleteDialogOpen"
    :review-id="item.id"
    :game-title="item.game.title"
    @deleted="handleReviewDeleted"
  />
</template>

<style scoped>
.card {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius) !important;
  padding: 18px;
  transition: border-color 200ms ease, transform 200ms cubic-bezier(0.25, 0.46, 0.45, 0.94), box-shadow 200ms ease;
  position: relative;
  overflow: hidden;
}

.card::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 2px;
  background: linear-gradient(to bottom, rgba(168, 85, 247, 0), rgba(168, 85, 247, 0));
  transition: background 250ms ease;
  border-radius: 2px 0 0 2px;
}

.card:hover {
  border-color: rgba(168, 85, 247, 0.28);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35), 0 2px 8px rgba(168, 85, 247, 0.08);
  transform: translateY(-2px);
}

.card:hover::before {
  background: linear-gradient(to bottom, rgba(168, 85, 247, 0.8), rgba(168, 85, 247, 0.2));
}

.card:hover .thumb {
  opacity: 1;
  transform: scale(1.03);
  border-color: rgba(168, 85, 247, 0.3);
}

.top-row {
  display: flex;
  align-items: flex-start;
  gap: 14px;
}

.avatar,
.avatar-link {
  flex: 0 0 auto;
}

.avatar-link {
  text-decoration: none;
}

.avatar :deep(.v-avatar) {
  background: rgba(168, 85, 247, 0.15);
  border: 1.5px solid rgba(168, 85, 247, 0.25);
}

.avatar-fallback {
  color: var(--primary);
  font-size: 0.82rem;
  font-weight: 800;
  letter-spacing: 0.02em;
}

.content {
  flex: 1 1 auto;
  min-width: 0;
}

.headline {
  display: flex;
  align-items: baseline;
  flex-wrap: wrap;
  gap: 8px;
  font-size: 0.94rem;
}

.name-link,
.game-link {
  color: var(--foreground);
  text-decoration: none;
  font-weight: 700;
  transition: color 150ms ease;
}

.name-link:hover {
  color: var(--primary);
}

.game-link:hover {
  color: #d8b4fe;
}

.subline {
  margin-top: 8px;
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.muted {
  color: var(--muted-foreground);
  font-size: 0.88rem;
}

.top-actions {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  margin-left: auto;
}

.thumb {
  width: 56px;
  height: 76px;
  flex: 0 0 auto;
  border-radius: 9px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.1);
  opacity: 0.85;
  transition: opacity 200ms ease, transform 200ms ease, border-color 200ms ease;
}

.divider {
  margin: 14px 0 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
}

.body {
  color: color-mix(in srgb, var(--foreground) 82%, transparent);
  line-height: 1.7;
  margin: 0 0 10px;
  white-space: pre-wrap;
  font-size: 0.95rem;
}

.action-list {
  background: var(--card);
  border: 1px solid var(--border);
}

.actions :deep(.v-btn) {
  margin-left: -8px;
}

.comments-shell {
  padding-top: 8px;
}

@media (max-width: 600px) {
  .top-actions {
    flex-direction: column;
    align-items: flex-end;
  }
}
</style>
