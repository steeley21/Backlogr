<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { AxiosError } from 'axios'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()

const isRefreshing = ref(false)
const errorMessage = ref('')

const user = computed(() => authStore.user)

const avatarInitials = computed(() => {
  const source = authStore.displayName || authStore.userName || 'B'
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
})

const profileHandle = computed(() => {
  return authStore.userName ? `@${authStore.userName}` : ''
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

  return 'Unable to refresh your profile right now. Please try again.'
}

async function refreshProfile(): Promise<void> {
  isRefreshing.value = true
  errorMessage.value = ''

  try {
    await authStore.fetchMe()
  }
  catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
  }
  finally {
    isRefreshing.value = false
  }
}

onMounted(async () => {
  if (!authStore.user) {
    await refreshProfile()
  }
})
</script>

<template>
  <div>
    <SectionHeader
      icon="mdi-account"
      title="Profile"
      right-text="Your Backlogr account"
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

    <v-card class="panel" rounded="xl" flat>
      <div class="profile-head">
        <v-avatar size="80" class="avatar">
          <v-img
            v-if="user?.avatarUrl"
            :src="user.avatarUrl"
            :alt="user.displayName"
            cover
          />
          <span v-else class="avatar-fallback">{{ avatarInitials }}</span>
        </v-avatar>

        <div class="identity">
          <div class="text-h5 font-weight-bold">
            {{ user?.displayName || authStore.displayName || 'Backlogr User' }}
          </div>
          <div v-if="profileHandle" class="muted">{{ profileHandle }}</div>
          <div class="muted mt-3">
            {{ user?.bio || 'No bio added yet.' }}
          </div>
        </div>

        <div class="head-actions">
          <v-btn
            color="primary"
            rounded="pill"
            class="text-none px-6"
            prepend-icon="mdi-refresh"
            :loading="isRefreshing"
            @click="refreshProfile"
          >
            Refresh
          </v-btn>
        </div>
      </div>

      <v-divider class="my-5" />

      <div class="detail-grid">
        <div class="detail-card">
          <div class="detail-label">Email</div>
          <div class="detail-value">{{ user?.email || 'Unknown' }}</div>
        </div>

        <div class="detail-card">
          <div class="detail-label">Username</div>
          <div class="detail-value">{{ user?.userName || 'Unknown' }}</div>
        </div>

        <div class="detail-card">
          <div class="detail-label">Display name</div>
          <div class="detail-value">{{ user?.displayName || 'Unknown' }}</div>
        </div>

        <div class="detail-card">
          <div class="detail-label">Roles</div>
          <div class="detail-value">
            <div class="d-flex flex-wrap ga-2">
              <v-chip
                v-for="role in user?.roles ?? []"
                :key="role"
                size="small"
                color="primary"
                variant="tonal"
              >
                {{ role }}
              </v-chip>

              <span v-if="!(user?.roles?.length)" class="muted">None</span>
            </div>
          </div>
        </div>
      </div>
    </v-card>

    <v-card class="panel mt-4" rounded="xl" flat>
      <div class="text-h6 font-weight-bold mb-2">What’s next</div>
      <div class="muted">
        Public profile pages, follow counts, review totals, and editable profile fields can be added once those backend endpoints are in place.
      </div>
    </v-card>
  </div>
</template>

<style scoped>
.panel {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 20px;
}

.profile-head {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  gap: 18px;
  align-items: start;
}

.avatar {
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.06);
}

.avatar-fallback {
  font-size: 1.2rem;
  font-weight: 800;
  color: var(--foreground);
}

.identity {
  min-width: 0;
}

.head-actions {
  display: flex;
  justify-content: flex-end;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

.detail-card {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: var(--radius);
  padding: 14px;
}

.detail-label {
  font-size: 0.82rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--muted-foreground);
  margin-bottom: 6px;
}

.detail-value {
  color: var(--foreground);
  line-height: 1.45;
  word-break: break-word;
}

.muted {
  color: var(--muted-foreground);
}

@media (max-width: 800px) {
  .profile-head {
    grid-template-columns: 1fr;
  }

  .head-actions {
    justify-content: flex-start;
  }

  .detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>