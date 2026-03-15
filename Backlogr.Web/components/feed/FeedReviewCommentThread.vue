<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  createReviewComment,
  deleteReviewComment,
  getReviewComments,
} from '~/services/reviewService'
import type { ReviewCommentResponseDto } from '~/types/review'
import { getApiErrorMessage } from '~/utils/apiError'

const props = defineProps<{
  reviewId: string
}>()

const emit = defineEmits<{
  countUpdated: [count: number]
  feedback: [message: string, color: 'success' | 'error']
}>()

const comments = ref<ReviewCommentResponseDto[]>([])
const commentText = ref('')
const isLoading = ref(false)
const isSubmitting = ref(false)
const deletingCommentId = ref<string | null>(null)
const localErrorMessage = ref('')

const canSubmit = computed(() => {
  return commentText.value.trim().length > 0 && !isSubmitting.value
})

function buildAvatarInitials(comment: ReviewCommentResponseDto): string {
  const source = comment.displayName || comment.userName || 'B'
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
}

function formatCreatedAt(value: string): string {
  const date = new Date(value)

  return date.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

async function loadComments(): Promise<void> {
  isLoading.value = true
  localErrorMessage.value = ''

  try {
    comments.value = await getReviewComments(props.reviewId)
    emit('countUpdated', comments.value.length)
  }
  catch (error: unknown) {
    comments.value = []
    localErrorMessage.value = getApiErrorMessage(error, 'Unable to load comments right now.')
    emit('feedback', localErrorMessage.value, 'error')
  }
  finally {
    isLoading.value = false
  }
}

async function submitComment(): Promise<void> {
  const trimmedText = commentText.value.trim()

  if (!trimmedText || isSubmitting.value) {
    return
  }

  isSubmitting.value = true
  localErrorMessage.value = ''

  try {
    const createdComment = await createReviewComment(props.reviewId, { text: trimmedText })
    comments.value = [...comments.value, createdComment]
    commentText.value = ''
    emit('countUpdated', comments.value.length)
  }
  catch (error: unknown) {
    localErrorMessage.value = getApiErrorMessage(error, 'Unable to add this comment right now.')
    emit('feedback', localErrorMessage.value, 'error')
  }
  finally {
    isSubmitting.value = false
  }
}

async function removeComment(reviewCommentId: string): Promise<void> {
  if (deletingCommentId.value) {
    return
  }

  deletingCommentId.value = reviewCommentId
  localErrorMessage.value = ''

  try {
    await deleteReviewComment(reviewCommentId)
    comments.value = comments.value.filter(comment => comment.reviewCommentId !== reviewCommentId)
    emit('countUpdated', comments.value.length)
  }
  catch (error: unknown) {
    localErrorMessage.value = getApiErrorMessage(error, 'Unable to delete this comment right now.')
    emit('feedback', localErrorMessage.value, 'error')
  }
  finally {
    deletingCommentId.value = null
  }
}

onMounted(async () => {
  await loadComments()
})
</script>

<template>
  <div class="thread-shell">
    <v-alert
      v-if="localErrorMessage"
      type="error"
      variant="tonal"
      rounded="lg"
      density="comfortable"
      class="mb-3"
    >
      {{ localErrorMessage }}
    </v-alert>

    <div v-if="isLoading" class="d-flex flex-column ga-3">
      <v-skeleton-loader
        v-for="n in 2"
        :key="n"
        type="list-item-avatar-two-line"
      />
    </div>

    <div v-else>
      <div v-if="comments.length === 0" class="empty-copy muted mb-3">
        No comments yet. Start the conversation.
      </div>

      <div v-else class="d-flex flex-column ga-3 mb-4">
        <v-card
          v-for="comment in comments"
          :key="comment.reviewCommentId"
          class="comment-card"
          rounded="xl"
          flat
        >
          <div class="comment-row">
            <NuxtLink :to="`/u/${comment.userName}`" class="avatar-link" :aria-label="`Open ${comment.displayName}`">
              <v-avatar size="38" class="avatar">
                <v-img v-if="comment.avatarUrl" :src="comment.avatarUrl" :alt="comment.displayName" cover />
                <span v-else class="avatar-fallback">{{ buildAvatarInitials(comment) }}</span>
              </v-avatar>
            </NuxtLink>

            <div class="comment-content">
              <div class="comment-headline">
                <div class="comment-meta">
                  <NuxtLink :to="`/u/${comment.userName}`" class="name-link">
                    {{ comment.displayName }}
                  </NuxtLink>
                  <span class="muted">{{ formatCreatedAt(comment.createdAt) }}</span>
                </div>

                <v-btn
                  v-if="comment.isOwner"
                  icon="mdi-delete-outline"
                  variant="text"
                  density="comfortable"
                  color="error"
                  :loading="deletingCommentId === comment.reviewCommentId"
                  :disabled="deletingCommentId === comment.reviewCommentId"
                  aria-label="Delete comment"
                  @click="removeComment(comment.reviewCommentId)"
                />
              </div>

              <p class="comment-body">{{ comment.text }}</p>
            </div>
          </div>
        </v-card>
      </div>

      <v-card class="composer-card" rounded="xl" flat>
        <v-textarea
          v-model="commentText"
          label="Add a comment"
          variant="outlined"
          rows="3"
          auto-grow
          no-resize
          maxlength="2000"
          counter
          hide-details="auto"
        />

        <div class="composer-actions">
          <span class="muted helper-copy">Keep it thoughtful and respectful.</span>
          <v-btn
            color="primary"
            rounded="pill"
            class="text-none px-5"
            :disabled="!canSubmit"
            :loading="isSubmitting"
            @click="submitComment"
          >
            Post comment
          </v-btn>
        </div>
      </v-card>
    </div>
  </div>
</template>

<style scoped>
.thread-shell {
  margin-top: 10px;
}

.comment-card,
.composer-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.comment-card {
  padding: 12px 14px;
}

.comment-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.avatar-link {
  text-decoration: none;
  flex: 0 0 auto;
}

.avatar-fallback {
  color: var(--foreground);
  font-size: 0.78rem;
  font-weight: 800;
}

.comment-content {
  min-width: 0;
  flex: 1 1 auto;
}

.comment-headline {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.comment-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.name-link {
  color: var(--foreground);
  text-decoration: none;
  font-weight: 700;
}

.name-link:hover {
  text-decoration: underline;
}

.comment-body {
  color: color-mix(in srgb, var(--foreground) 86%, transparent);
  line-height: 1.6;
  margin: 10px 0 0;
  white-space: pre-wrap;
}

.composer-card {
  padding: 14px;
}

.composer-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 12px;
}

.muted {
  color: var(--muted-foreground);
}

.helper-copy,
.empty-copy {
  font-size: 0.94rem;
}

@media (max-width: 700px) {
  .composer-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .helper-copy {
    text-align: center;
  }
}
</style>
