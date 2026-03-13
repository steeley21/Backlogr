<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, navigateTo } from '#imports'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import { useAuthStore } from '~/stores/auth'
import type { RegisterRequestDto } from '~/types/auth'

definePageMeta({
  layout: 'default',
})

const authStore = useAuthStore()
const route = useRoute()

const form = ref<RegisterRequestDto>({
  userName: '',
  displayName: '',
  email: '',
  password: '',
})

const confirmPassword = ref('')
const isSubmitting = ref(false)
const errorMessage = ref('')

const redirectTarget = computed(() => {
  const redirect = route.query.redirect

  if (typeof redirect === 'string' && redirect.startsWith('/')) {
    return redirect
  }

  return '/'
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

  return 'Unable to create your account right now. Please try again.'
}

async function handleSubmit(): Promise<void> {
  if (!isFormValid.value || isSubmitting.value) {
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    await authStore.register(form.value)
    await navigateTo(redirectTarget.value)
  }
  catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-shell">
      <SectionHeader
        icon="mdi-account-plus-outline"
        title="Create account"
        right-text="Join Backlogr"
      />

      <v-card class="auth-card" rounded="xl" flat>
        <div class="text-h6 font-weight-bold mb-1">Start tracking what you play</div>
        <div class="muted mb-5">
          Create your account to build your library, post reviews, and get AI-powered recommendations.
        </div>

        <v-alert
          v-if="errorMessage"
          type="error"
          variant="tonal"
          class="mb-4"
          rounded="lg"
        >
          {{ errorMessage }}
        </v-alert>

        <v-form @submit.prevent="handleSubmit">
          <v-text-field
            v-model="form.userName"
            label="Username"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            class="mb-3"
            autocomplete="username"
          />

          <v-text-field
            v-model="form.displayName"
            label="Display name"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            class="mb-3"
            autocomplete="name"
          />

          <v-text-field
            v-model="form.email"
            label="Email"
            type="email"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            class="mb-3"
            autocomplete="email"
          />

          <v-text-field
            v-model="form.password"
            label="Password"
            type="password"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            class="mb-3"
            autocomplete="new-password"
          />

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

          <div class="actions mt-5">
            <v-btn
              color="primary"
              rounded="pill"
              class="text-none px-6"
              :loading="isSubmitting"
              :disabled="!isFormValid"
              type="submit"
            >
              Create account
            </v-btn>

            <v-btn
              to="/login"
              variant="text"
              rounded="pill"
              class="text-none"
            >
              Already have an account?
            </v-btn>
          </div>
        </v-form>
      </v-card>
    </div>
  </div>
</template>

<style scoped>
.auth-page {
  min-height: calc(100vh - 120px);
  display: grid;
  place-items: center;
  padding-block: 24px;
}

.auth-shell {
  width: 100%;
  max-width: 560px;
}

.auth-card {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 24px;
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.28);
}

.muted {
  color: var(--muted-foreground);
}

.actions {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
</style>