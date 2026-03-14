<!-- /components/admin/AdminCreateUserDialog.vue -->
<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { AdminAssignableRole, AdminCreateUserRequestDto } from '~/types/admin'
import { ADMIN_ROLE, USER_ROLE } from '~/utils/roles'

interface Props {
  modelValue: boolean
  canCreateAdmin: boolean
  isSubmitting: boolean
  errorMessage: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  submit: [payload: AdminCreateUserRequestDto]
}>()

const form = ref<AdminCreateUserRequestDto>({
  userName: '',
  displayName: '',
  email: '',
  password: '',
  role: USER_ROLE,
})
const confirmPassword = ref('')

const availableRoles = computed(() => {
  const roles: AdminAssignableRole[] = [USER_ROLE]

  if (props.canCreateAdmin) {
    roles.push(ADMIN_ROLE)
  }

  return roles
})

const normalizedEmail = computed(() => form.value.email.trim())
const trimmedUserName = computed(() => form.value.userName.trim())
const trimmedDisplayName = computed(() => form.value.displayName.trim())

const usernameErrorMessages = computed(() => {
  if (trimmedUserName.value.length === 0) {
    return []
  }

  const messages: string[] = []

  if (trimmedUserName.value.length < 3) {
    messages.push('Username must be at least 3 characters.')
  }

  if (!/^[A-Za-z0-9_.-]+$/.test(trimmedUserName.value)) {
    messages.push('Use letters, numbers, dots, dashes, or underscores only.')
  }

  return messages
})

const displayNameErrorMessages = computed(() => {
  if (trimmedDisplayName.value.length === 0) {
    return []
  }

  return trimmedDisplayName.value.length >= 2
    ? []
    : ['Display name must be at least 2 characters.']
})

const emailErrorMessages = computed(() => {
  if (normalizedEmail.value.length === 0) {
    return []
  }

  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalizedEmail.value)
    ? []
    : ['Enter a valid email address.']
})

const passwordErrorMessages = computed(() => {
  if (form.value.password.length === 0) {
    return []
  }

  return form.value.password.length >= 8
    ? []
    : ['Password must be at least 8 characters.']
})

const passwordsMatch = computed(() => {
  return form.value.password === confirmPassword.value
})

const confirmPasswordErrorMessages = computed(() => {
  if (confirmPassword.value.length === 0) {
    return []
  }

  return passwordsMatch.value ? [] : ['Passwords do not match.']
})

const isFormValid = computed(() => {
  return trimmedUserName.value.length > 0
    && trimmedDisplayName.value.length > 0
    && normalizedEmail.value.length > 0
    && form.value.password.trim().length > 0
    && confirmPassword.value.trim().length > 0
    && usernameErrorMessages.value.length === 0
    && displayNameErrorMessages.value.length === 0
    && emailErrorMessages.value.length === 0
    && passwordErrorMessages.value.length === 0
    && confirmPasswordErrorMessages.value.length === 0
    && passwordsMatch.value
})

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    resetForm()
  }
})

function resetForm(): void {
  form.value = {
    userName: '',
    displayName: '',
    email: '',
    password: '',
    role: USER_ROLE,
  }
  confirmPassword.value = ''
}

function closeDialog(): void {
  if (props.isSubmitting) {
    return
  }

  emit('update:modelValue', false)
}

function submit(): void {
  if (!isFormValid.value || props.isSubmitting) {
    return
  }

  emit('submit', {
    userName: trimmedUserName.value,
    displayName: trimmedDisplayName.value,
    email: normalizedEmail.value,
    password: form.value.password,
    role: form.value.role,
  })
}
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="620"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card class="dialog-card" rounded="xl" flat>
      <div class="dialog-header">
        <div>
          <div class="text-h6 font-weight-bold">Create user</div>
          <div class="muted mt-1">
            Add a new Backlogr account and assign its starting role.
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

      <v-alert type="info" variant="tonal" rounded="lg" class="mb-4">
        {{ canCreateAdmin
          ? 'SuperAdmin can create both User and Admin accounts.'
          : 'Admin accounts can only create standard User accounts.' }}
      </v-alert>

      <v-form @submit.prevent="submit">
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="form.userName"
              label="Username"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
              autocomplete="username"
              :readonly="isSubmitting"
              :error-messages="usernameErrorMessages"
            />
          </v-col>

          <v-col cols="12" md="6">
            <v-text-field
              v-model="form.displayName"
              label="Display name"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
              autocomplete="name"
              :readonly="isSubmitting"
              :error-messages="displayNameErrorMessages"
            />
          </v-col>

          <v-col cols="12">
            <v-text-field
              v-model="form.email"
              label="Email"
              type="email"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
              autocomplete="email"
              :readonly="isSubmitting"
              :error-messages="emailErrorMessages"
            />
          </v-col>

          <v-col cols="12" md="6">
            <v-text-field
              v-model="form.password"
              label="Password"
              type="password"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
              autocomplete="new-password"
              :readonly="isSubmitting"
              :error-messages="passwordErrorMessages"
            />
          </v-col>

          <v-col cols="12" md="6">
            <v-text-field
              v-model="confirmPassword"
              label="Confirm password"
              type="password"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
              autocomplete="new-password"
              :readonly="isSubmitting"
              :error-messages="confirmPasswordErrorMessages"
            />
          </v-col>

          <v-col cols="12">
            <v-select
              v-model="form.role"
              :items="availableRoles"
              label="Role"
              variant="solo-filled"
              rounded="xl"
              hide-details="auto"
              :disabled="isSubmitting"
            />
          </v-col>
        </v-row>

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
            :disabled="!isFormValid || isSubmitting"
            type="submit"
          >
            Create user
          </v-btn>
        </div>
      </v-form>
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
