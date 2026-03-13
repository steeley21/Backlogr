<!-- /components/app/AppTopBar.vue -->
<script setup lang="ts">
import { computed, ref } from 'vue'
import { navigateTo, useRoute } from '#imports'
import BacklogrLogo from '~/components/shared/BacklogrLogo.vue'
import { useAuthStore } from '~/stores/auth'

type NavItem = {
  label: string
  to: string
  icon?: string
}

const navItems: NavItem[] = [
  { label: 'Feed', to: '/' },
  { label: 'Browse', to: '/browse' },
  { label: 'Library', to: '/library' },
  { label: 'AI Picks', to: '/recommend', icon: 'mdi-sparkles' },
]

const route = useRoute()
const authStore = useAuthStore()
const search = ref('')

const isAuthPage = computed(() => {
  return route.path === '/login' || route.path === '/register'
})

const profileDisplayName = computed(() => {
  return authStore.displayName || authStore.userName || 'Account'
})

const profileSubtext = computed(() => {
  if (authStore.userName) {
    return `@${authStore.userName}`
  }

  return authStore.user?.email ?? ''
})

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

function isActive(path: string): boolean {
  return route.path === path
}

async function submitSearch(): Promise<void> {
  const q = search.value.trim()

  if (!q) {
    await navigateTo('/browse')
    return
  }

  await navigateTo({
    path: '/browse',
    query: { q },
  })
}

async function handleLogout(): Promise<void> {
  authStore.logout()
  await navigateTo('/login')
}
</script>

<template>
  <v-app-bar height="76" flat class="appbar">
    <div class="bar-inner" :class="{ 'auth-mode': isAuthPage }">
      <div class="left">
        <BacklogrLogo />
      </div>

      <template v-if="!isAuthPage">
        <div class="center nav">
          <v-btn
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            variant="text"
            class="text-none nav-btn"
            :class="{ active: isActive(item.to) }"
            rounded="pill"
          >
            <v-icon
              v-if="item.icon"
              :icon="item.icon"
              size="18"
              class="mr-2"
            />
            {{ item.label }}
          </v-btn>
        </div>

        <div class="right d-flex align-center justify-end">
          <v-text-field
            v-model="search"
            class="search mr-3"
            density="compact"
            hide-details
            variant="solo-filled"
            rounded="pill"
            placeholder="Search games..."
            prepend-inner-icon="mdi-magnify"
            @keyup.enter="submitSearch"
          />

          <v-btn
            to="/log"
            color="primary"
            rounded="pill"
            class="text-none px-5 mr-3 log-btn"
            prepend-icon="mdi-plus"
          >
            Log
          </v-btn>

          <v-menu location="bottom end" offset="10">
            <template #activator="{ props }">
              <v-btn
                v-bind="props"
                icon
                variant="text"
                aria-label="Profile menu"
              >
                <v-avatar size="36" class="profile-avatar">
                  <v-img
                    v-if="authStore.avatarUrl"
                    :src="authStore.avatarUrl"
                    :alt="profileDisplayName"
                    cover
                  />
                  <span v-else class="avatar-fallback">{{ avatarInitials }}</span>
                </v-avatar>
              </v-btn>
            </template>

            <v-card class="profile-menu" rounded="xl" flat min-width="240">
              <div class="profile-menu__header">
                <div class="profile-menu__name">{{ profileDisplayName }}</div>
                <div class="profile-menu__subtext">{{ profileSubtext }}</div>
              </div>

              <v-divider />

              <v-list bg-color="transparent" density="comfortable" nav>
                <v-list-item
                  to="/profile"
                  prepend-icon="mdi-account-outline"
                  title="Profile"
                />
                <v-list-item
                  to="/library"
                  prepend-icon="mdi-bookshelf"
                  title="Library"
                />
                <v-list-item
                  prepend-icon="mdi-logout"
                  title="Sign out"
                  @click="handleLogout"
                />
              </v-list>
            </v-card>
          </v-menu>
        </div>
      </template>

      <template v-else>
        <div class="right auth-actions d-flex align-center justify-end">
          <v-btn
            to="/login"
            variant="text"
            rounded="pill"
            class="text-none"
            :class="{ active: route.path === '/login' }"
          >
            Sign in
          </v-btn>

          <v-btn
            to="/register"
            color="primary"
            rounded="pill"
            class="text-none px-5"
          >
            Create account
          </v-btn>
        </div>
      </template>
    </div>
  </v-app-bar>
</template>

<style scoped>
.appbar {
  background: rgba(20, 24, 28, 0.78);
  backdrop-filter: blur(14px);
  border-bottom: 1px solid var(--border);
}

.bar-inner {
  width: min(1100px, calc(100% - 48px));
  margin: 0 auto;
  display: grid;
  grid-template-columns: 220px 1fr 380px;
  align-items: center;
  gap: 16px;
}

.bar-inner.auth-mode {
  grid-template-columns: 220px 1fr;
}

.nav {
  display: flex;
  justify-content: center;
  gap: 6px;
}

.nav-btn {
  color: color-mix(in srgb, var(--foreground) 70%, transparent);
  padding-inline: 14px;
  min-height: 38px;
}

.nav-btn:hover {
  background: rgba(255, 255, 255, 0.06);
}

.nav-btn.active {
  background: rgba(255, 255, 255, 0.10);
  color: var(--foreground);
}

.search {
  max-width: 280px;
  min-width: 200px;
}

.search :deep(.v-field__prepend-inner .v-icon) {
  color: var(--muted-foreground);
}

.search :deep(input::placeholder) {
  color: color-mix(in srgb, var(--muted-foreground) 80%, transparent);
}

.log-btn {
  box-shadow: 0 10px 22px rgba(0, 0, 0, 0.25);
}

.profile-avatar {
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.06);
}

.avatar-fallback {
  font-size: 0.8rem;
  font-weight: 800;
  color: var(--foreground);
}

.profile-menu {
  background: var(--card);
  border: 1px solid var(--border);
  overflow: hidden;
}

.profile-menu__header {
  padding: 14px 16px 12px;
}

.profile-menu__name {
  font-weight: 700;
  color: var(--foreground);
}

.profile-menu__subtext {
  margin-top: 4px;
  font-size: 0.9rem;
  color: var(--muted-foreground);
}

.auth-actions {
  gap: 10px;
}

.auth-actions .active {
  background: rgba(255, 255, 255, 0.10);
  color: var(--foreground);
}

@media (max-width: 960px) {
  .bar-inner {
    grid-template-columns: 200px 1fr;
    grid-template-areas:
      "left right"
      "center center";
  }

  .bar-inner.auth-mode {
    grid-template-columns: 1fr auto;
    grid-template-areas: "left right";
  }

  .left {
    grid-area: left;
  }

  .center {
    grid-area: center;
    justify-content: flex-start;
    overflow-x: auto;
    padding-bottom: 2px;
  }

  .right {
    grid-area: right;
    justify-content: flex-end;
  }

  .search {
    max-width: 220px;
    min-width: 160px;
  }
}

@media (max-width: 700px) {
  .bar-inner:not(.auth-mode) {
    grid-template-columns: 1fr;
    grid-template-areas:
      "left"
      "right"
      "center";
  }

  .right {
    width: 100%;
  }

  .search {
    flex: 1 1 auto;
    max-width: none;
    min-width: 0;
  }
}
</style>