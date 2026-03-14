<!-- /components/admin/AdminCreateUserDialog.vue -->
<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { AdminAssignableRole, AdminCreateUserRequestDto } from '~/types/admin'
import { USER_ROLE, ADMIN_ROLE } from '~/utils/roles'

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

const passwordsMatch = computed(() => {
  return form.value.password === confirmPassword.value
})

const isFormValid = computed(() => {
  return form.value.userName.trim().length > 0
    && form.value.displayName.trim().length > 0
    && form.value.email.trim().length > 0
    && form.value.password.trim().length > 0
    && confirmPassword.value.trim().length > 0
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
  emit('update:modelValue', false)
}

function submit(): void {
  if (!isFormValid.value || props.isSubmitting) {
    return
  }

  emit('submit', {
    userName: form.value.userName.trim(),
    displayName: form.value.displayName.trim(),
    email: form.value.email.trim(),
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

        <v-btn icon="mdi-close" variant="text" @click="closeDialog" />
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
              :error="confirmPassword.length > 0 && !passwordsMatch"
              :error-messages="confirmPassword.length > 0 && !passwordsMatch ? ['Passwords do not match.'] : []"
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
            />
          </v-col>
        </v-row>

        <div class="dialog-actions mt-6">
          <v-btn
            variant="text"
            rounded="pill"
            class="text-none"
            @click="closeDialog"
          >
            Cancel
          </v-btn>

          <v-btn
            color="primary"
            rounded="pill"
            class="text-none px-6"
            :loading="isSubmitting"
            :disabled="!isFormValid"
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
