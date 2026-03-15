<script setup lang="ts">
import { ref, watch } from 'vue'
import { deleteReview } from '~/services/reviewService'
import { getApiErrorMessage } from '~/utils/apiError'

interface Props {
  modelValue: boolean
  reviewId: string
  gameTitle: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  deleted: [reviewId: string]
}>()

const isSubmitting = ref(false)
const errorMessage = ref('')

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    errorMessage.value = ''
  }
})

function closeDialog(): void {
  if (isSubmitting.value) {
    return
  }

  emit('update:modelValue', false)
}

async function confirmDelete(): Promise<void> {
  if (isSubmitting.value) {
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    await deleteReview(props.reviewId)
    emit('deleted', props.reviewId)
    emit('update:modelValue', false)
  }
  catch (error: unknown) {
    errorMessage.value = getApiErrorMessage(error, 'Unable to delete your review right now. Please try again.')
  }
  finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="520"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="dialog-card" rounded="xl" flat>
      <div class="dialog-header">
        <div>
          <div class="text-h6 font-weight-bold">Delete review</div>
          <div class="muted mt-1">
            Remove your review for {{ gameTitle }} from the feed.
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

      <v-alert type="warning" variant="tonal" rounded="lg">
        This permanently removes your written review. Your library log for the game will still remain.
      </v-alert>

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
          color="error"
          rounded="pill"
          class="text-none px-6"
          :loading="isSubmitting"
          @click="confirmDelete"
        >
          Delete review
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
