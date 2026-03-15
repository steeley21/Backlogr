<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { updateReview } from '~/services/reviewService'
import type { ReviewResponseDto } from '~/types/review'
import { getApiErrorMessage } from '~/utils/apiError'

interface Props {
  modelValue: boolean
  reviewId: string
  gameTitle: string
  initialText: string
  initialHasSpoilers: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  saved: [review: ReviewResponseDto]
}>()

const reviewText = ref('')
const hasSpoilers = ref(false)
const isSubmitting = ref(false)
const errorMessage = ref('')

const trimmedText = computed(() => reviewText.value.trim())
const remainingCharacters = computed(() => 2000 - reviewText.value.length)
const canSubmit = computed(() => {
  return !isSubmitting.value
    && trimmedText.value.length > 0
    && trimmedText.value.length <= 2000
})

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    reviewText.value = props.initialText
    hasSpoilers.value = props.initialHasSpoilers
    errorMessage.value = ''
  }
})

function closeDialog(): void {
  if (isSubmitting.value) {
    return
  }

  emit('update:modelValue', false)
}

async function submit(): Promise<void> {
  if (!canSubmit.value) {
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const updatedReview = await updateReview(props.reviewId, {
      text: trimmedText.value,
      hasSpoilers: hasSpoilers.value,
    })

    emit('saved', updatedReview)
    emit('update:modelValue', false)
  }
  catch (error: unknown) {
    errorMessage.value = getApiErrorMessage(error, 'Unable to update your review right now. Please try again.')
  }
  finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="760"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="dialog-card" rounded="xl" flat>
      <div class="dialog-header">
        <div>
          <div class="text-h6 font-weight-bold">Edit review</div>
          <div class="muted mt-1">
            Update your review for {{ gameTitle }}.
          </div>
        </div>

        <v-btn
          icon="mdi-close"
          variant="text"
          :disabled="isSubmitting"
          @click="closeDialog"
        />
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

      <v-textarea
        v-model="reviewText"
        label="Review"
        variant="outlined"
        rounded="xl"
        rows="8"
        auto-grow
        counter="2000"
        :readonly="isSubmitting"
      />

      <div class="d-flex align-center justify-space-between flex-wrap ga-3 mt-2">
        <v-checkbox
          v-model="hasSpoilers"
          hide-details
          density="comfortable"
          :disabled="isSubmitting"
          label="This review contains spoilers"
        />

        <div
          class="muted text-body-2"
          :class="{ 'text-error': remainingCharacters < 0 }"
        >
          {{ remainingCharacters }} characters remaining
        </div>
      </div>

      <div class="dialog-actions mt-6">
        <v-btn
          variant="text"
          rounded="pill"
          class="text-none"
          :disabled="isSubmitting"
          @click="closeDialog"
        >
          Cancel
        </v-btn>

        <v-btn
          color="primary"
          rounded="pill"
          class="text-none px-6"
          :loading="isSubmitting"
          :disabled="!canSubmit"
          @click="submit"
        >
          Save changes
        </v-btn>
      </div>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.dialog-card {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 24px;
}

.dialog-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 20px;
}

.dialog-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  flex-wrap: wrap;
}

.muted {
  color: var(--muted-foreground);
}
</style>
