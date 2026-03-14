<!-- /components/admin/AdminUserTable.vue -->
<script setup lang="ts">
import type { AdminUserSummaryDto } from '~/types/admin'
import { ADMIN_ROLE, SUPER_ADMIN_ROLE } from '~/utils/roles'

interface Props {
  users: AdminUserSummaryDto[]
  isLoading: boolean
  errorMessage: string
}

const props = defineProps<Props>()

function formatDate(value: string | null): string {
  if (!value) {
    return '—'
  }

  const parsed = new Date(value)

  if (Number.isNaN(parsed.getTime())) {
    return '—'
  }

  return parsed.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

function getRoleChipColor(role: string): string {
  if (role === SUPER_ADMIN_ROLE) {
    return 'secondary'
  }

  if (role === ADMIN_ROLE) {
    return 'primary'
  }

  return 'default'
}
</script>

<template>
  <div>
    <v-alert
      v-if="errorMessage"
      type="error"
      variant="tonal"
      rounded="lg"
      class="mb-4"
    >
      {{ errorMessage }}
    </v-alert>

    <div v-if="isLoading" class="d-flex flex-column ga-3">
      <v-skeleton-loader
        v-for="n in 3"
        :key="n"
        type="table-row-divider"
      />
    </div>

    <v-card
      v-else-if="props.users.length === 0"
      class="empty-state"
      rounded="xl"
      flat
    >
      <div class="text-h6 font-weight-bold mb-2">No users returned yet</div>
      <div class="muted">
        Once the admin user endpoint is available, user records will appear here for quick account management.
      </div>
    </v-card>

    <v-table v-else class="users-table">
      <thead>
        <tr>
          <th>User</th>
          <th>Email</th>
          <th>Roles</th>
          <th>Created</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="user in props.users" :key="user.userId">
          <td>
            <div class="font-weight-bold">{{ user.displayName }}</div>
            <div class="muted text-body-2">@{{ user.userName }}</div>
          </td>
          <td>{{ user.email }}</td>
          <td>
            <div class="d-flex ga-2 flex-wrap">
              <v-chip
                v-for="role in user.roles"
                :key="`${user.userId}-${role}`"
                :color="getRoleChipColor(role)"
                size="small"
                variant="tonal"
              >
                {{ role }}
              </v-chip>
            </div>
          </td>
          <td>{{ formatDate(user.createdAtUtc) }}</td>
        </tr>
      </tbody>
    </v-table>
  </div>
</template>

<style scoped>
.users-table {
  background: transparent;
}

.users-table :deep(th) {
  color: var(--muted-foreground);
  font-weight: 700;
}

.users-table :deep(td) {
  color: var(--foreground);
  border-color: rgba(255, 255, 255, 0.06);
}

.empty-state {
  background: color-mix(in srgb, var(--card) 88%, black);
  border: 1px dashed var(--border);
  padding: 24px;
}

.muted {
  color: var(--muted-foreground);
}
</style>
