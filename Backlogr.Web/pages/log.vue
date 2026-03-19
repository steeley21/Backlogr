<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { navigateTo, useRoute } from '#imports'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import { runReviewAssistant } from '~/services/aiService'
import { getGameById } from '~/services/gameService'
import { upsertLibraryLog } from '~/services/libraryService'
import { createReview } from '~/services/reviewService'
import type { ReviewAssistantMode } from '~/types/ai'
import type { GameDetailResponseDto } from '~/types/game'
import type { LibraryStatus, UpsertLibraryLogRequestDto } from '~/types/library'

definePageMeta({
  layout: 'default',
})

const route = useRoute()

const statusOptions: LibraryStatus[] = [
  'Backlog',
  'Playing',
  'Played',
  'Wishlist',
  'Dropped',
]

type AssistantAction = {
  label: string
  mode: ReviewAssistantMode
  icon: string
  requiresExistingText?: boolean
}

const assistantActions: AssistantAction[] = [
  { label: 'Draft', mode: 'draft', icon: 'mdi-file-document-edit-outline' },
  { label: 'Rewrite', mode: 'rewrite', icon: 'mdi-pencil-outline', requiresExistingText: true },
  { label: 'Shorten', mode: 'shorten', icon: 'mdi-arrow-collapse-horizontal', requiresExistingText: true },
  { label: 'Expand', mode: 'expand', icon: 'mdi-arrow-expand-horizontal', requiresExistingText: true },
  { label: 'Spoiler-safe', mode: 'spoiler-safe-summary', icon: 'mdi-shield-half-full', requiresExistingText: true },
]

const assistantExamples = [
  'Focus on the combat and pacing.',
  'Make this more concise and less repetitive.',
  'Keep the same opinion, but make it smoother.',
]

const game = ref<GameDetailResponseDto | null>(null)
const isLoadingGame = ref(false)
const isSubmitting = ref(false)
const isRunningAssistant = ref(false)
const errorMessage = ref('')
const assistantErrorMessage = ref('')

const status = ref<LibraryStatus>('Played')
const rating = ref<string>('4.5')
const platform = ref('')
const hours = ref<string>('')
const startedAt = ref('')
const finishedAt = ref('')
const notes = ref('')
const reviewText = ref('')
const hasSpoilers = ref(false)
const assistantPrompt = ref('')

const gameId = computed(() => {
  const value = route.query.gameId
  return typeof value === 'string' ? value : ''
})

const pageTitle = computed(() => {
  return game.value ? `Log ${game.value.title}` : 'Log a Game'
})

const canSubmit = computed(() => {
  return !isLoadingGame.value
    && !isSubmitting.value
    && gameId.value.length > 0
    && game.value !== null
})

const heroMeta = computed(() => {
  if (!game.value) {
    return ''
  }

  const parts: string[] = []

  if (game.value.genres) {
    parts.push(game.value.genres)
  }

  if (game.value.platforms) {
    parts.push(game.value.platforms)
  }

  if (game.value.releaseDate) {
    parts.push(new Date(game.value.releaseDate).getUTCFullYear().toString())
  }

  return parts.join(' • ')
})

function parseOptionalNumber(value: string): number | null {
  const trimmed = value.trim()

  if (!trimmed) {
    return null
  }

  const parsed = Number(trimmed)

  if (Number.isNaN(parsed)) {
    return null
  }

  return parsed
}

function parseOptionalDate(value: string): string | null {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
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

  return 'Unable to save your log right now. Please try again.'
}

function getAssistantErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const apiMessage = error.response?.data

    if (typeof apiMessage === 'string' && apiMessage.trim().length > 0) {
      return apiMessage
    }

    if (Array.isArray(apiMessage) && apiMessage.length > 0) {
      return apiMessage.join(', ')
    }
  }

  return 'Unable to run the review assistant right now. Please try again.'
}

function applyAssistantExample(example: string): void {
  assistantPrompt.value = example
}

async function loadGame(): Promise<void> {
  if (!gameId.value) {
    game.value = null
    return
  }

  isLoadingGame.value = true
  errorMessage.value = ''

  try {
    game.value = await getGameById(gameId.value)
  }
  catch (error: unknown) {
    game.value = null
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isLoadingGame.value = false
  }
}

async function handleAssistant(action: AssistantAction): Promise<void> {
  if (!game.value || isRunningAssistant.value) {
    return
  }

  assistantErrorMessage.value = ''

  const existingText = reviewText.value.trim()
  const promptText = assistantPrompt.value.trim()

  if (action.requiresExistingText && existingText.length === 0) {
    assistantErrorMessage.value = 'Add some review text first before using that assistant action.'
    return
  }

  if (action.mode === 'draft' && promptText.length === 0) {
    assistantErrorMessage.value = 'Add a prompt for the assistant to draft from.'
    return
  }

  isRunningAssistant.value = true

  try {
    const response = await runReviewAssistant({
      mode: action.mode,
      prompt: promptText || `Write a ${action.label.toLowerCase()} review for ${game.value.title}.`,
      existingText: existingText || null,
    })

    reviewText.value = response.resultText
  }
  catch (error: unknown) {
    assistantErrorMessage.value = getAssistantErrorMessage(error)
  }
  finally {
    isRunningAssistant.value = false
  }
}

async function handleSubmit(): Promise<void> {
  if (!canSubmit.value || !gameId.value) {
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const payload: UpsertLibraryLogRequestDto = {
      gameId: gameId.value,
      status: status.value,
      rating: parseOptionalNumber(rating.value),
      platform: platform.value.trim() || null,
      hours: parseOptionalNumber(hours.value),
      startedAt: parseOptionalDate(startedAt.value),
      finishedAt: parseOptionalDate(finishedAt.value),
      notes: notes.value.trim() || null,
    }

    await upsertLibraryLog(payload)

    if (reviewText.value.trim().length > 0) {
      await createReview({
        gameId: gameId.value,
        text: reviewText.value.trim(),
        hasSpoilers: hasSpoilers.value,
      })
    }

    await navigateTo(`/game/${gameId.value}`)
  }
  catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isSubmitting.value = false
  }
}

watch(
  () => gameId.value,
  async () => {
    await loadGame()
  },
  { immediate: true },
)
</script>

<template>
  <div>
    <SectionHeader
      icon="mdi-plus"
      :title="pageTitle"
      right-text="Create a log or review"
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

    <v-card
      v-if="isLoadingGame"
      class="panel"
      rounded="xl"
      flat
    >
      <v-skeleton-loader type="article, article, article" />
    </v-card>

    <v-card
      v-else-if="!game"
      class="panel empty-state"
      rounded="xl"
      flat
    >
      <div class="text-h6 font-weight-bold mb-2">Choose a game first</div>
      <div class="muted mb-4">
        Logging requires a real game from your catalog. Start from Browse or a game page.
      </div>

      <v-btn
        to="/browse"
        color="primary"
        rounded="pill"
        class="text-none px-6"
      >
        Browse games
      </v-btn>
    </v-card>

    <template v-else>
      <v-card class="panel game-panel mb-4" rounded="xl" flat>
        <div class="game-summary">
          <div class="cover-wrap">
            <v-img
              v-if="game.coverImageUrl"
              :src="game.coverImageUrl"
              :alt="game.title"
              cover
              class="cover"
            />
            <div v-else class="cover cover-fallback">
              {{ game.title.slice(0, 1).toUpperCase() }}
            </div>
          </div>

          <div class="game-copy">
            <div class="text-h6 font-weight-bold">{{ game.title }}</div>
            <div v-if="heroMeta" class="muted mt-1">{{ heroMeta }}</div>
            <div class="muted mt-3">
              {{ game.summary || 'No summary is available for this game yet.' }}
            </div>
          </div>
        </div>
      </v-card>

      <v-card class="panel" rounded="xl" flat>
        <v-form @submit.prevent="handleSubmit">
          <div class="form-grid">
            <v-select
              v-model="status"
              :items="statusOptions"
              label="Status"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
            />

            <v-text-field
              v-model="rating"
              label="Rating (0–5)"
              type="number"
              step="0.5"
              min="0"
              max="5"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
            />

            <v-text-field
              v-model="platform"
              label="Platform"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
            />

            <v-text-field
              v-model="hours"
              label="Hours played"
              type="number"
              step="0.1"
              min="0"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
            />

            <v-text-field
              v-model="startedAt"
              label="Started at"
              type="date"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
            />

            <v-text-field
              v-model="finishedAt"
              label="Finished at"
              type="date"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
            />
          </div>

          <v-textarea
            v-model="notes"
            class="mt-4"
            label="Private notes (optional)"
            auto-grow
            rows="4"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
          />

          <v-divider class="my-5" />

          <div class="assistant-header">
            <div>
              <div class="text-subtitle-1 font-weight-bold mb-1">Public review</div>
              <div class="muted">
                Use the AI assistant to draft, rewrite, shorten, expand, or make your review spoiler-safe before saving.
              </div>
            </div>
          </div>

          <v-alert
            type="info"
            variant="tonal"
            rounded="lg"
            class="mt-4"
          >
            Draft works best from a prompt. The other actions work best when you already have review text written.
          </v-alert>

          <v-alert
            v-if="assistantErrorMessage"
            type="error"
            variant="tonal"
            rounded="lg"
            class="mt-4"
          >
            {{ assistantErrorMessage }}
          </v-alert>

          <v-textarea
            v-model="assistantPrompt"
            class="mt-4"
            label="Assistant prompt"
            auto-grow
            rows="2"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            placeholder="Example: Focus on the combat, pacing, and whether the ending worked."
          />

          <div class="assistant-examples mt-3">
            <span class="assistant-examples__label">Try:</span>

            <v-chip
              v-for="example in assistantExamples"
              :key="example"
              size="small"
              variant="outlined"
              class="mr-2 mb-2"
              @click="applyAssistantExample(example)"
            >
              {{ example }}
            </v-chip>
          </div>

          <div class="assistant-tools mt-4">
            <div class="assistant-tools__header">
              <div class="assistant-tools__title">Assistant tools</div>
              <div class="assistant-tools__hint">
                Pick an action to generate or improve your review.
              </div>
            </div>

            <div class="assistant-actions mt-3">
              <v-btn
                v-for="action in assistantActions"
                :key="action.mode"
                variant="tonal"
                color="primary"
                rounded="pill"
                class="text-none assistant-action-btn"
                :prepend-icon="action.icon"
                :loading="isRunningAssistant"
                :disabled="isRunningAssistant"
                @click="handleAssistant(action)"
              >
                {{ action.label }}
              </v-btn>
            </div>
          </div>

          <v-textarea
            v-model="reviewText"
            class="mt-4"
            label="Review text"
            auto-grow
            rows="6"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            placeholder="Write your review here, or use the assistant to help draft one."
          />

          <v-checkbox
            v-model="hasSpoilers"
            label="This review contains spoilers"
            hide-details
            class="mt-2"
          />

          <div class="mt-5 d-flex ga-3 flex-wrap">
            <v-btn
              color="primary"
              rounded="pill"
              class="text-none px-6"
              type="submit"
              :loading="isSubmitting"
              :disabled="!canSubmit"
            >
              Save log
            </v-btn>

            <v-btn
              :to="`/game/${game.gameId}`"
              variant="text"
              rounded="pill"
              class="text-none"
            >
              Cancel
            </v-btn>
          </div>
        </v-form>
      </v-card>
    </template>
  </div>
</template>

<style scoped>
.panel {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 16px;
}

.empty-state {
  padding: 24px;
}

.game-panel {
  padding: 18px;
}

.game-summary {
  display: grid;
  grid-template-columns: 112px minmax(0, 1fr);
  gap: 18px;
  align-items: start;
}

.cover-wrap {
  width: 112px;
}

.cover {
  aspect-ratio: 2 / 3;
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.cover-fallback {
  display: grid;
  place-items: center;
  background: linear-gradient(135deg, rgba(168, 85, 247, 0.2), rgba(28, 34, 40, 0.92));
  color: var(--foreground);
  font-size: 2rem;
  font-weight: 800;
}

.game-copy {
  min-width: 0;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

.assistant-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.assistant-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.assistant-examples__label {
  display: inline-block;
  margin-right: 10px;
  color: var(--muted-foreground);
  font-size: 0.95rem;
}

.assistant-tools {
  margin-top: 16px;
  padding: 16px;
  border: 1px solid var(--border);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.02);
}

.assistant-tools__header {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.assistant-tools__title {
  font-weight: 700;
  color: var(--foreground);
}

.assistant-tools__hint {
  color: var(--muted-foreground);
  font-size: 0.95rem;
}

.assistant-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.assistant-action-btn {
  min-height: 40px;
}

.muted {
  color: var(--muted-foreground);
}

@media (max-width: 760px) {
  .game-summary {
    grid-template-columns: 1fr;
  }

  .cover-wrap {
    width: 96px;
  }

  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>