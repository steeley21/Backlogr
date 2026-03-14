<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminCreateUserDialog from '~/components/admin/AdminCreateUserDialog.vue'
import AdminUserTable from '~/components/admin/AdminUserTable.vue'
import { createAdminUser, getAdminUsers } from '~/services/adminService'
import { useAuthStore } from '~/stores/auth'
import type { AdminCreateUserRequestDto, AdminUserSummaryDto } from '~/types/admin'
import { getApiErrorMessage } from '~/utils/apiError'
import { isAdminLike, isSuperAdmin } from '~/utils/roles'

definePageMeta({
  middleware: 'admin',
})

const authStore = useAuthStore()

const users = ref<AdminUserSummaryDto[]>([])
const isLoadingUsers = ref(false)
const loadErrorMessage = ref('')
const createErrorMessage = ref('')
const successMessage = ref('')
const isCreateDialogOpen = ref(false)
const isCreatingUser = ref(false)

const canCreateAdmin = computed(() => {
  return isSuperAdmin(authStore.roles)
})

const canViewAdminPage = computed(() => {
  return isAdminLike(authStore.roles)
})

const adminUserCount = computed(() => {
  return users.value.filter(user => isAdminLike(user.roles)).length
})

async function loadUsers(): Promise<void> {
  isLoadingUsers.value = true
  loadErrorMessage.value = ''

  try {
    users.value = await getAdminUsers()
  }
  catch (error: unknown) {
    users.value = []
    loadErrorMessage.value = getApiErrorMessage(error, 'Unable to load admin user data right now.')
  }
  finally {
    isLoadingUsers.value = false
  }
}

async function handleCreateUser(payload: AdminCreateUserRequestDto): Promise<void> {
  isCreatingUser.value = true
  createErrorMessage.value = ''
  successMessage.value = ''

  try {
    const createdUser = await createAdminUser(payload)
    isCreateDialogOpen.value = false
    successMessage.value = `${createdUser.displayName} was created successfully.`
    await loadUsers()
  }
  catch (error: unknown) {
    createErrorMessage.value = getApiErrorMessage(error, 'Unable to create the user right now.')
  }
  finally {
    isCreatingUser.value = false
  }
}

onMounted(async () => {
  await loadUsers()
})
</script>

<template>
  <div v-if="canViewAdminPage" class="admin-page">
    <v-card class="hero" rounded="xl" flat>
      <div class="hero-copy">
        <div class="overline">ADMIN TOOLS</div>
        <div class="hero-title">Backlogr account management</div>
        <div class="muted hero-subtitle">
          Use this dashboard to review existing accounts and create new users with the correct starting role.
        </div>
      </div>

      <div class="hero-actions">
        <v-btn
          color="primary"
          rounded="pill"
          class="text-none px-6"
          prepend-icon="mdi-account-plus-outline"
          @click="isCreateDialogOpen = true"
        >
          Create user
        </v-btn>

        <v-btn
          variant="text"
          rounded="pill"
          class="text-none"
          prepend-icon="mdi-refresh"
          :loading="isLoadingUsers"
          @click="loadUsers"
        >
          Refresh
        </v-btn>
      </div>
    </v-card>

    <v-alert
      v-if="successMessage"
      type="success"
      variant="tonal"
      rounded="lg"
      class="mt-4"
    >
      {{ successMessage }}
    </v-alert>

    <v-row class="mt-2" dense>
      <v-col cols="12" md="4">
        <v-card class="summary-card" rounded="xl" flat>
          <div class="summary-label">Signed-in role</div>
          <div class="summary-value">{{ canCreateAdmin ? 'SuperAdmin' : 'Admin' }}</div>
          <div class="muted summary-copy">
            {{ canCreateAdmin
              ? 'You can create both User and Admin accounts.'
              : 'You can create standard User accounts.' }}
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="summary-card" rounded="xl" flat>
          <div class="summary-label">Users returned</div>
          <div class="summary-value">{{ users.length }}</div>
          <div class="muted summary-copy">
            This count comes from the admin users endpoint once it is available.
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="summary-card" rounded="xl" flat>
          <div class="summary-label">Admin-level accounts</div>
          <div class="summary-value">{{ adminUserCount }}</div>
          <div class="muted summary-copy">
            Admin and SuperAdmin roles are counted together for a quick permission snapshot.
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-card class="section-card mt-4" rounded="xl" flat>
      <div class="section-header">
        <div>
          <div class="section-title">Users</div>
          <div class="muted section-copy">
            Review current accounts and verify role assignments before handing out elevated access.
          </div>
        </div>
      </div>

      <AdminUserTable
        :users="users"
        :is-loading="isLoadingUsers"
        :error-message="loadErrorMessage"
      />
    </v-card>

    <AdminCreateUserDialog
      v-model="isCreateDialogOpen"
      :can-create-admin="canCreateAdmin"
      :is-submitting="isCreatingUser"
      :error-message="createErrorMessage"
      @submit="handleCreateUser"
    />
  </div>
</template>

<style scoped>
.admin-page {
  padding-top: 6px;
}

.hero,
.summary-card,
.section-card {
  background: color-mix(in srgb, var(--card) 90%, black);
  border: 1px solid var(--border);
}

.hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  padding: 28px;
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.28);
}

.hero-copy {
  max-width: 640px;
}

.overline {
  color: var(--primary);
  font-weight: 700;
  letter-spacing: 0.16em;
  font-size: 0.75rem;
}

.hero-title,
.section-title,
.summary-value {
  color: var(--foreground);
}

.hero-title {
  margin-top: 10px;
  font-size: 2rem;
  font-weight: 800;
}

.hero-subtitle,
.muted,
.summary-label {
  color: var(--muted-foreground);
}

.hero-subtitle {
  margin-top: 10px;
  line-height: 1.6;
}

.hero-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.summary-card {
  height: 100%;
  padding: 20px;
}

.summary-label {
  font-size: 0.85rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.summary-value {
  margin-top: 10px;
  font-size: 1.8rem;
  font-weight: 800;
}

.summary-copy {
  margin-top: 10px;
  line-height: 1.55;
}

.section-card {
  padding: 24px;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.section-title {
  font-size: 1.4rem;
  font-weight: 800;
}

.section-copy {
  margin-top: 6px;
}

@media (max-width: 900px) {
  .hero {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
