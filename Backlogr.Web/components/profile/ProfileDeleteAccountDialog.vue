<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { DeleteAccountRequestDto } from '~/types/auth'

interface Props {
  modelValue: boolean
  userName: string
  displayName: string
  isSubmitting: boolean
  errorMessage: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  submit: [payload: DeleteAccountRequestDto]
}>()

const password = ref('')
const confirmationUserName = ref('')
const confirmDelete = ref(false)

const isConfirmationMatch = computed(() => {
  return confirmationUserName.value.trim() === props.userName
})

const canSubmit = computed(() => {
  return !props.isSubmitting
    && password.value.trim().length > 0
    && confirmDelete.value
    && isConfirmationMatch.value
  })

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    password.value = ''
    confirmationUserName.value = ''
    confirmDelete.value = false
  }
})

function closeDialog(): void {
  if (props.isSubmitting) {
    return
  }

  emit('update:modelValue', false)
}

function submit(): void {
  if (!canSubmit.value) {
    return
  }

  emit('submit', {
    password: password.value,
    confirmationUserName: confirmationUserName.value.trim(),
  })
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="560"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="dialog-card" rounded="xl" flat>
      <div class="dialog-header">
        <div>
          <div class="text-h6 font-weight-bold">Delete your account</div>
          <div class="muted mt-1">
            Permanently remove {{ displayName || userName }} from Backlogr.
          </div>
        </div>

        <v-btn icon="mdi-close" variant="text" :disabled="isSubmitting" @click="closeDialog" />
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
        This action permanently deletes your Backlogr account and cannot be undone.
      </v-alert>

      <div class="muted mt-4 mb-2">
        Type <strong>{{ userName }}</strong> and enter your password to confirm.
      </div>

      <v-text-field
        v-model="confirmationUserName"
        label="Confirm your username"
        variant="outlined"
        density="comfortable"
        :disabled="isSubmitting"
        autocomplete="off"
        class="mt-2"
      />

      <v-text-field
        v-model="password"
        label="Current password"
        type="password"
        variant="outlined"
        density="comfortable"
        :disabled="isSubmitting"
        autocomplete="current-password"
      />

      <v-checkbox
        v-model="confirmDelete"
        class="mt-1"
        hide-details
        :disabled="isSubmitting"
        label="I understand this permanently deletes my account and profile data."
      />

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
          :disabled="!canSubmit"
          @click="submit"
        >
          Delete account
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
