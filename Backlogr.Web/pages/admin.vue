<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminCreateUserDialog from '~/components/admin/AdminCreateUserDialog.vue'
import AdminEditUserRoleDialog from '~/components/admin/AdminEditUserRoleDialog.vue'
import AdminUserTable from '~/components/admin/AdminUserTable.vue'
import { createAdminUser, getAdminUsers, updateAdminUserRole } from '~/services/adminService'
import { useAuthStore } from '~/stores/auth'
import type {
  AdminCreateUserRequestDto,
  AdminUpdateUserRoleRequestDto,
  AdminUserSummaryDto,
} from '~/types/admin'
import { getApiErrorMessage } from '~/utils/apiError'
import { ADMIN_ROLE, isAdminLike, isSuperAdmin, SUPER_ADMIN_ROLE, USER_ROLE } from '~/utils/roles'

definePageMeta({
  middleware: 'admin',
})

type RoleFilter = 'All' | typeof USER_ROLE | typeof ADMIN_ROLE | typeof SUPER_ADMIN_ROLE

authenticateAdminPage()

function authenticateAdminPage(): void {
  // Keeps page setup logic explicit for readability in this admin view.
}

const authStore = useAuthStore()

const users = ref<AdminUserSummaryDto[]>([])
const isLoadingUsers = ref(false)
const loadErrorMessage = ref('')
const createErrorMessage = ref('')
const editErrorMessage = ref('')
const isCreateDialogOpen = ref(false)
const isEditDialogOpen = ref(false)
const isCreatingUser = ref(false)
const isUpdatingRole = ref(false)
const selectedUser = ref<AdminUserSummaryDto | null>(null)
const searchQuery = ref('')
const selectedRoleFilter = ref<RoleFilter>('All')
const snackbar = ref({
  isOpen: false,
  color: 'success' as 'success' | 'error',
  message: '',
})

const roleFilterItems: RoleFilter[] = ['All', USER_ROLE, ADMIN_ROLE, SUPER_ADMIN_ROLE]

const currentUserId = computed(() => {
  return authStore.user?.userId ?? null
})

const canCreateAdmin = computed(() => {
  return isSuperAdmin(authStore.roles)
})

const canViewAdminPage = computed(() => {
  return isAdminLike(authStore.roles)
})

const canManageRoles = computed(() => {
  return isSuperAdmin(authStore.roles)
})

const sortedUsers = computed(() => {
  return [...users.value].sort((left, right) => {
    const leftDate = left.createdAtUtc ? Date.parse(left.createdAtUtc) : 0
    const rightDate = right.createdAtUtc ? Date.parse(right.createdAtUtc) : 0

    if (leftDate !== rightDate) {
      return rightDate - leftDate
    }

    return left.displayName.localeCompare(right.displayName)
  })
})

const filteredUsers = computed(() => {
  const normalizedQuery = searchQuery.value.trim().toLowerCase()

  return sortedUsers.value.filter((user) => {
    const matchesRole = selectedRoleFilter.value === 'All'
      || user.roles.includes(selectedRoleFilter.value)

    if (!matchesRole) {
      return false
    }

    if (!normalizedQuery) {
      return true
    }

    return [user.displayName, user.userName, user.email]
      .some(value => value.toLowerCase().includes(normalizedQuery))
  })
})

const adminUserCount = computed(() => {
  return users.value.filter(user => isAdminLike(user.roles)).length
})

const filteredSummaryText = computed(() => {
  if (searchQuery.value.trim().length === 0 && selectedRoleFilter.value === 'All') {
    return `Showing all ${filteredUsers.value.length} users.`
  }

  return `Showing ${filteredUsers.value.length} of ${users.value.length} users.`
})

const emptyStateTitle = computed(() => {
  if (users.value.length === 0) {
    return 'No users returned yet'
  }

  return 'No users match those filters'
})

const emptyStateMessage = computed(() => {
  if (users.value.length === 0) {
    return 'User records will appear here for quick account management once the admin users endpoint returns data.'
  }

  return 'Try clearing the search or switching the role filter to see more accounts.'
})

function showSnackbar(message: string, color: 'success' | 'error'): void {
  snackbar.value = {
    isOpen: true,
    color,
    message,
  }
}

async function loadUsers(): Promise<void> {
  isLoadingUsers.value = true
  loadErrorMessage.value = ''

  try {
    users.value = await getAdminUsers()
  }
  catch (error: unknown) {
    users.value = []
    loadErrorMessage.value = getApiErrorMessage(error, 'Unable to load admin user data right now.')
    showSnackbar(loadErrorMessage.value, 'error')
  }
  finally {
    isLoadingUsers.value = false
  }
}

async function handleCreateUser(payload: AdminCreateUserRequestDto): Promise<void> {
  isCreatingUser.value = true
  createErrorMessage.value = ''

  try {
    const createdUser = await createAdminUser(payload)
    isCreateDialogOpen.value = false
    showSnackbar(`${createdUser.displayName} was created successfully.`, 'success')
    await loadUsers()
  }
  catch (error: unknown) {
    createErrorMessage.value = getApiErrorMessage(error, 'Unable to create the user right now.')
    showSnackbar(createErrorMessage.value, 'error')
  }
  finally {
    isCreatingUser.value = false
  }
}

function openEditRoleDialog(user: AdminUserSummaryDto): void {
  selectedUser.value = user
  editErrorMessage.value = ''
  isEditDialogOpen.value = true
}

async function handleUpdateRole(payload: AdminUpdateUserRoleRequestDto): Promise<void> {
  if (!selectedUser.value) {
    return
  }

  isUpdatingRole.value = true
  editErrorMessage.value = ''

  try {
    const updatedUser = await updateAdminUserRole(selectedUser.value.userId, payload)
    isEditDialogOpen.value = false
    selectedUser.value = null
    showSnackbar(`${updatedUser.displayName} is now assigned to ${payload.role}.`, 'success')
    await loadUsers()
  }
  catch (error: unknown) {
    editErrorMessage.value = getApiErrorMessage(error, 'Unable to update the user role right now.')
    showSnackbar(editErrorMessage.value, 'error')
  }
  finally {
    isUpdatingRole.value = false
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
          Use this dashboard to review existing accounts, create new users, and manage role assignments.
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

    <v-row class="mt-2" dense>
      <v-col cols="12" md="4">
        <v-card class="summary-card" rounded="xl" flat>
          <div class="summary-label">Signed-in role</div>
          <div class="summary-value">{{ canCreateAdmin ? 'SuperAdmin' : 'Admin' }}</div>
          <div class="muted summary-copy">
            {{ canCreateAdmin
              ? 'You can create both User and Admin accounts, and edit existing User/Admin roles.'
              : 'You can create standard User accounts.' }}
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="summary-card" rounded="xl" flat>
          <div class="summary-label">Users returned</div>
          <div class="summary-value">{{ users.length }}</div>
          <div class="muted summary-copy">
            This count comes from the live admin users endpoint.
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
            Review current accounts and manage elevated access with care.
          </div>
        </div>
      </div>

      <div class="filters-grid mt-4">
        <v-text-field
          v-model="searchQuery"
          label="Search users"
          placeholder="Find by display name, username, or email"
          variant="solo-filled"
          rounded="xl"
          hide-details="auto"
          prepend-inner-icon="mdi-magnify"
          clearable
        />

        <v-select
          v-model="selectedRoleFilter"
          :items="roleFilterItems"
          label="Role filter"
          variant="solo-filled"
          rounded="xl"
          hide-details="auto"
        />
      </div>

      <div class="table-meta mt-3 mb-2">
        <div class="muted">{{ filteredSummaryText }}</div>
        <div class="muted permission-note">
          SuperAdmin can edit User/Admin roles. SuperAdmin accounts and your own account stay protected.
        </div>
      </div>

      <AdminUserTable
        :users="filteredUsers"
        :is-loading="isLoadingUsers"
        :error-message="loadErrorMessage"
        :can-manage-roles="canManageRoles"
        :current-user-id="currentUserId"
        :empty-title="emptyStateTitle"
        :empty-message="emptyStateMessage"
        @edit-role="openEditRoleDialog"
      />
    </v-card>

    <AdminCreateUserDialog
      v-model="isCreateDialogOpen"
      :can-create-admin="canCreateAdmin"
      :is-submitting="isCreatingUser"
      :error-message="createErrorMessage"
      @submit="handleCreateUser"
    />

    <AdminEditUserRoleDialog
      v-model="isEditDialogOpen"
      :user="selectedUser"
      :is-submitting="isUpdatingRole"
      :error-message="editErrorMessage"
      @submit="handleUpdateRole"
    />

    <v-snackbar
      v-model="snackbar.isOpen"
      :color="snackbar.color"
      rounded="pill"
      location="bottom right"
      timeout="3200"
    >
      {{ snackbar.message }}
    </v-snackbar>
  </div>
</template>

<style scoped>
.admin-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.hero,
.summary-card,
.section-card {
  background:
    radial-gradient(circle at top left, rgba(142, 189, 255, 0.14), transparent 30%),
    color-mix(in srgb, var(--card) 92%, black);
  border: 1px solid var(--border);
}

.hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  padding: 28px;
}

.hero-copy {
  max-width: 720px;
}

.overline {
  color: var(--primary);
  font-size: 0.78rem;
  font-weight: 800;
  letter-spacing: 0.14em;
  margin-bottom: 10px;
}

.hero-title {
  font-size: clamp(1.8rem, 3vw, 2.6rem);
  font-weight: 800;
  line-height: 1.1;
}

.hero-subtitle {
  margin-top: 12px;
  max-width: 60ch;
}

.hero-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: flex-end;
}

.summary-card {
  padding: 20px;
  height: 100%;
}

.summary-label {
  color: var(--muted-foreground);
  font-size: 0.82rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.summary-value {
  font-size: clamp(1.6rem, 2.8vw, 2.2rem);
  font-weight: 800;
  margin-top: 10px;
}

.summary-copy {
  margin-top: 10px;
}

.section-card {
  padding: 24px;
}

.section-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.section-title {
  font-size: 1.15rem;
  font-weight: 800;
}

.section-copy {
  margin-top: 6px;
}

.filters-grid {
  display: grid;
  gap: 16px;
  grid-template-columns: minmax(0, 2fr) minmax(220px, 320px);
}

.table-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.permission-note {
  text-align: right;
}

.muted {
  color: var(--muted-foreground);
}

@media (max-width: 960px) {
  .hero {
    flex-direction: column;
    align-items: flex-start;
  }

  .hero-actions {
    width: 100%;
    justify-content: flex-start;
  }

  .filters-grid {
    grid-template-columns: 1fr;
  }

  .permission-note {
    text-align: left;
  }
}
</style>
