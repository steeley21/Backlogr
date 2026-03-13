<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, navigateTo } from '#imports'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import { useAuthStore } from '~/stores/auth'
import type { LoginRequestDto } from '~/types/auth'

definePageMeta({
  layout: 'default',
})

const authStore = useAuthStore()
const route = useRoute()

const form = ref<LoginRequestDto>({
  emailOrUserName: '',
  password: '',
})

const isSubmitting = ref(false)
const errorMessage = ref('')

const redirectTarget = computed(() => {
  const redirect = route.query.redirect

  if (typeof redirect === 'string' && redirect.startsWith('/')) {
    return redirect
  }

  return '/'
})

const isFormValid = computed(() => {
  return form.value.emailOrUserName.trim().length > 0
    && form.value.password.trim().length > 0
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

  return 'Unable to sign in. Please check your credentials and try again.'
}

async function handleSubmit(): Promise<void> {
  if (!isFormValid.value || isSubmitting.value) {
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    await authStore.login(form.value)
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
        icon="mdi-lock-outline"
        title="Sign in"
        right-text="Welcome back to Backlogr"
      />

      <v-card class="auth-card" rounded="xl" flat>
        <div class="text-h6 font-weight-bold mb-1">Continue your backlog</div>
        <div class="muted mb-5">
          Sign in to access your feed, library, logs, reviews, and AI picks.
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
            v-model="form.emailOrUserName"
            label="Email or username"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            class="mb-3"
            autocomplete="username"
          />

          <v-text-field
            v-model="form.password"
            label="Password"
            type="password"
            variant="solo-filled"
            rounded="xl"
            hide-details="auto"
            autocomplete="current-password"
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
              Sign in
            </v-btn>

            <v-btn
              to="/register"
              variant="text"
              rounded="pill"
              class="text-none"
            >
              Create account
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