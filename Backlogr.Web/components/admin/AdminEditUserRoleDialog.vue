<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { AdminAssignableRole, AdminUpdateUserRoleRequestDto, AdminUserSummaryDto } from '~/types/admin'
import { ADMIN_ROLE, USER_ROLE } from '~/utils/roles'

interface Props {
  modelValue: boolean
  user: AdminUserSummaryDto | null
  isSubmitting: boolean
  errorMessage: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  submit: [payload: AdminUpdateUserRoleRequestDto]
}>()

const selectedRole = ref<AdminAssignableRole>(USER_ROLE)
const confirmRoleChange = ref(false)

const availableRoles: AdminAssignableRole[] = [USER_ROLE, ADMIN_ROLE]

const currentAssignableRole = computed<AdminAssignableRole>(() => {
  if (props.user?.roles.includes(ADMIN_ROLE)) {
    return ADMIN_ROLE
  }

  return USER_ROLE
})

const isChanged = computed(() => {
  return selectedRole.value !== currentAssignableRole.value
})

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    selectedRole.value = currentAssignableRole.value
    confirmRoleChange.value = false
  }
})

watch(() => props.user?.userId, () => {
  selectedRole.value = currentAssignableRole.value
  confirmRoleChange.value = false
})

function closeDialog(): void {
  if (props.isSubmitting) {
    return
  }

  emit('update:modelValue', false)
}

function submit(): void {
  if (!props.user || !isChanged.value || props.isSubmitting || !confirmRoleChange.value) {
    return
  }

  emit('submit', {
    role: selectedRole.value,
  })
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
          <div class="text-h6 font-weight-bold">Edit role</div>
          <div class="muted mt-1">
            Update the account role for {{ user?.displayName ?? 'this user' }}.
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

      <div class="mb-4">
        <div class="text-subtitle-2 font-weight-bold">Current access</div>
        <div class="muted mt-1">
          {{ currentAssignableRole }}
        </div>
      </div>

      <v-select
        v-model="selectedRole"
        :items="availableRoles"
        label="New role"
        variant="solo-filled"
        rounded="xl"
        hide-details="auto"
        :disabled="isSubmitting"
      />

      <v-alert
        v-if="user && isChanged"
        type="warning"
        variant="tonal"
        rounded="lg"
        class="mt-4"
      >
        You are changing {{ user.displayName }} from {{ currentAssignableRole }} to {{ selectedRole }}.
        This updates their admin access immediately.
      </v-alert>

      <v-checkbox
        v-model="confirmRoleChange"
        class="mt-4"
        hide-details
        :disabled="!isChanged || isSubmitting"
        label="I understand this changes the user's access level immediately."
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
          color="primary"
          rounded="pill"
          class="text-none px-6"
          :loading="isSubmitting"
          :disabled="!isChanged || !confirmRoleChange || isSubmitting"
          @click="submit"
        >
          Confirm role change
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
