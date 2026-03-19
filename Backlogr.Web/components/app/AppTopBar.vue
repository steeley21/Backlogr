<!-- /components/app/AppTopBar.vue -->
<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { navigateTo, useRoute } from '#imports'
import BacklogrLogo from '~/components/shared/BacklogrLogo.vue'
import { useAuthStore } from '~/stores/auth'
import { isAdminLike } from '~/utils/roles'

type NavItem = {
  label: string
  to: string
  icon?: string
}

const route = useRoute()
const authStore = useAuthStore()
const search = ref('')
const mobileMenuOpen = ref(false)

watch(() => route.path, () => {
  mobileMenuOpen.value = false
})

const isAuthPage = computed(() => {
  return route.path === '/login' || route.path === '/register'
})

const isLandingPage = computed(() => {
  return route.path === '/'
})

const isAdminUser = computed(() => {
  return isAdminLike(authStore.roles)
})

const navItems = computed<NavItem[]>(() => {
  const items: NavItem[] = [
    { label: 'Feed', to: '/feed' },
    { label: 'Browse', to: '/browse' },
    { label: 'Library', to: '/library' },
    { label: 'AI Picks', to: '/recommend', icon: 'mdi-sparkles' },
  ]

  if (isAdminUser.value) {
    items.push({ label: 'Admin', to: '/admin', icon: 'mdi-shield-account' })
  }

  return items
})

const showAuthenticatedChrome = computed(() => {
  return authStore.isAuthenticated && !isAuthPage.value && !isLandingPage.value
})

const currentGameId = computed(() => {
  if (!route.path.startsWith('/game/')) {
    return ''
  }

  const value = route.params.id
  return typeof value === 'string' ? value : ''
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

const publicProfilePath = computed(() => {
  return authStore.userName ? `/u/${authStore.userName}` : '/profile'
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

async function handleLogClick(): Promise<void> {
  if (currentGameId.value) {
    await navigateTo({
      path: '/log',
      query: { gameId: currentGameId.value },
    })
    return
  }

  await navigateTo('/browse')
}

async function handleLogout(): Promise<void> {
  authStore.logout()
  await navigateTo('/login')
}

function mobileTabIcon(path: string): string {
  const map: Record<string, string> = {
    '/feed': 'mdi-home-variant-outline',
    '/browse': 'mdi-magnify',
    '/library': 'mdi-bookshelf',
    '/recommend': 'mdi-sparkles',
    '/admin': 'mdi-shield-account',
  }

  return map[path] ?? 'mdi-circle-outline'
}
</script>

<template>
  <v-app-bar height="60" flat class="appbar">
    <div class="bar-inner" :class="{ 'public-mode': !showAuthenticatedChrome }">
      <div class="left">
        <BacklogrLogo />
      </div>

      <template v-if="showAuthenticatedChrome">
        <div class="center nav desktop-only">
          <v-btn
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            variant="text"
            class="text-none nav-btn"
            :class="{ active: isActive(item.to), 'ai-picks': item.to === '/recommend' }"
            rounded="pill"
            :ripple="false"
          >
            {{ item.label }}
          </v-btn>
        </div>

        <div class="right d-flex align-center justify-end desktop-only">
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
            color="primary"
            rounded="pill"
            class="text-none px-5 mr-3 log-btn"
            prepend-icon="mdi-plus"
            @click="handleLogClick"
          >
            Log
          </v-btn>

          <v-menu location="bottom end" offset="10">
            <template #activator="{ props }">
              <v-btn v-bind="props" icon variant="text" aria-label="Profile menu">
                <v-avatar size="36" class="profile-avatar">
                  <v-img v-if="authStore.avatarUrl" :src="authStore.avatarUrl" :alt="profileDisplayName" cover />
                  <span v-else class="avatar-fallback">{{ avatarInitials }}</span>
                </v-avatar>
              </v-btn>
            </template>

            <v-card class="profile-menu" rounded="xl" flat min-width="260">
              <div class="profile-menu__header">
                <div class="profile-menu__header-bg" />
                <div class="profile-menu__header-content">
                  <v-avatar size="48" class="profile-menu__avatar">
                    <v-img v-if="authStore.avatarUrl" :src="authStore.avatarUrl" :alt="profileDisplayName" cover />
                    <span v-else class="profile-menu__avatar-fallback">{{ avatarInitials }}</span>
                  </v-avatar>
                  <div class="profile-menu__identity">
                    <div class="profile-menu__name">{{ profileDisplayName }}</div>
                    <div class="profile-menu__subtext">{{ profileSubtext }}</div>
                  </div>
                </div>
              </div>

              <div class="profile-menu__items">
                <NuxtLink to="/profile" class="profile-menu__item">
                  <v-icon icon="mdi-tune" size="17" class="profile-menu__item-icon" />
                  <span>Account settings</span>
                </NuxtLink>
                <NuxtLink :to="publicProfilePath" class="profile-menu__item">
                  <v-icon icon="mdi-account-circle-outline" size="17" class="profile-menu__item-icon" />
                  <span>Public profile</span>
                </NuxtLink>
                <NuxtLink to="/library" class="profile-menu__item">
                  <v-icon icon="mdi-bookshelf" size="17" class="profile-menu__item-icon" />
                  <span>Library</span>
                </NuxtLink>
                <NuxtLink v-if="isAdminUser" to="/admin" class="profile-menu__item">
                  <v-icon icon="mdi-shield-account" size="17" class="profile-menu__item-icon" />
                  <span>Admin</span>
                </NuxtLink>
                <div class="profile-menu__divider" />
                <button class="profile-menu__item profile-menu__item--danger" @click="handleLogout">
                  <v-icon icon="mdi-logout" size="17" class="profile-menu__item-icon" />
                  <span>Sign out</span>
                </button>
              </div>
            </v-card>
          </v-menu>
        </div>

        <div class="right mobile-right mobile-only">
          <v-btn
            color="primary"
            rounded="pill"
            size="small"
            class="text-none log-btn-mobile mr-2"
            icon
            @click="handleLogClick"
          >
            <v-icon icon="mdi-plus" size="20" />
          </v-btn>

          <v-btn icon variant="text" aria-label="Profile menu" @click="mobileMenuOpen = true">
            <v-avatar size="34" class="profile-avatar">
              <v-img v-if="authStore.avatarUrl" :src="authStore.avatarUrl" :alt="profileDisplayName" cover />
              <span v-else class="avatar-fallback">{{ avatarInitials }}</span>
            </v-avatar>
          </v-btn>
        </div>
      </template>

      <template v-else>
        <div class="right public-actions d-flex align-center justify-end">
          <v-btn
            v-if="authStore.isAuthenticated && isLandingPage"
            to="/feed"
            variant="text"
            rounded="pill"
            class="text-none"
          >
            Open feed
          </v-btn>
          <v-btn
            v-else
            to="/login"
            variant="text"
            rounded="pill"
            class="text-none"
            :class="{ active: route.path === '/login' }"
          >
            Sign in
          </v-btn>

          <v-btn
            v-if="authStore.isAuthenticated && isLandingPage"
            to="/browse"
            color="primary"
            rounded="pill"
            class="text-none px-5"
          >
            Browse games
          </v-btn>
          <v-btn
            v-else
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

  <Teleport v-if="showAuthenticatedChrome" to="body">
    <nav class="mobile-tab-bar mobile-only">
      <NuxtLink
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        class="mobile-tab"
        :class="{ active: isActive(item.to) }"
      >
        <v-icon
          :icon="mobileTabIcon(item.to)"
          size="22"
          class="mobile-tab__icon"
        />
        <span class="mobile-tab__label">{{ item.label }}</span>
      </NuxtLink>
    </nav>

    <Transition name="drawer">
      <div v-if="mobileMenuOpen" class="mobile-drawer-backdrop" @click="mobileMenuOpen = false">
        <div class="mobile-drawer" @click.stop>
          <div class="mobile-drawer__handle" />

          <div class="mobile-drawer__header">
            <div class="mobile-drawer__header-bg" />
            <div class="mobile-drawer__header-content">
              <v-avatar size="56" class="profile-menu__avatar">
                <v-img v-if="authStore.avatarUrl" :src="authStore.avatarUrl" :alt="profileDisplayName" cover />
                <span v-else class="profile-menu__avatar-fallback">{{ avatarInitials }}</span>
              </v-avatar>
              <div>
                <div class="mobile-drawer__name">{{ profileDisplayName }}</div>
                <div class="mobile-drawer__subtext">{{ profileSubtext }}</div>
              </div>
            </div>
          </div>

          <div class="mobile-drawer__search">
            <v-text-field
              v-model="search"
              density="compact"
              hide-details
              variant="solo-filled"
              rounded="pill"
              placeholder="Search games..."
              prepend-inner-icon="mdi-magnify"
              @keyup.enter="submitSearch"
            />
          </div>

          <div class="profile-menu__items">
            <NuxtLink to="/profile" class="profile-menu__item">
              <v-icon icon="mdi-tune" size="18" class="profile-menu__item-icon" />
              <span>Account settings</span>
            </NuxtLink>
            <NuxtLink :to="publicProfilePath" class="profile-menu__item">
              <v-icon icon="mdi-account-circle-outline" size="18" class="profile-menu__item-icon" />
              <span>Public profile</span>
            </NuxtLink>
            <NuxtLink to="/library" class="profile-menu__item">
              <v-icon icon="mdi-bookshelf" size="18" class="profile-menu__item-icon" />
              <span>Library</span>
            </NuxtLink>
            <NuxtLink v-if="isAdminUser" to="/admin" class="profile-menu__item">
              <v-icon icon="mdi-shield-account" size="18" class="profile-menu__item-icon" />
              <span>Admin</span>
            </NuxtLink>
            <div class="profile-menu__divider" />
            <button class="profile-menu__item profile-menu__item--danger" @click="handleLogout">
              <v-icon icon="mdi-logout" size="18" class="profile-menu__item-icon" />
              <span>Sign out</span>
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
/* ─── Helpers ─────────────────────────────────────────────── */
.desktop-only { display: flex; }
.mobile-only  { display: none; }

@media (max-width: 700px) {
  .desktop-only { display: none !important; }
  .mobile-only  { display: flex !important; }
}

/* ─── App bar ─────────────────────────────────────────────── */
.appbar {
  background: rgba(12, 15, 20, 0.88);
  backdrop-filter: blur(18px) saturate(1.4);
  border-bottom: 1px solid rgba(255,255,255,0.06);
}

.appbar::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(168,85,247,0.3) 40%, rgba(168,85,247,0.3) 60%, transparent);
  pointer-events: none;
}

/* ─── Bar inner grid ──────────────────────────────────────── */
.bar-inner {
  width: min(1100px, calc(100% - 48px));
  margin: 0 auto;
  display: grid;
  grid-template-columns: 200px 1fr 380px;
  align-items: center;
  gap: 16px;
}

.bar-inner.public-mode {
  grid-template-columns: 200px 1fr;
}

@media (max-width: 960px) {
  .bar-inner {
    grid-template-columns: 200px 1fr 280px;
  }

  .search {
    max-width: 180px;
    min-width: 120px;
  }
}

@media (max-width: 700px) {
  .bar-inner,
  .bar-inner.public-mode {
    width: calc(100% - 32px);
    grid-template-columns: 1fr auto;
    gap: 0;
  }
}

/* ─── Desktop nav ─────────────────────────────────────────── */
.nav {
  justify-content: center;
  align-items: center;
  gap: 2px;
}

.nav-btn {
  color: var(--muted-foreground);
  padding-inline: 12px;
  min-height: 36px;
  min-width: unset !important;
  font-size: 0.9rem;
  font-weight: 500;
  letter-spacing: 0.01em;
  position: relative;
  transition: color 160ms ease;
}

.nav-btn :deep(.v-btn__overlay) { display: none !important; }

.nav-btn::after {
  content: '';
  position: absolute;
  bottom: 5px;
  left: 50%;
  transform: translateX(-50%) scaleX(0);
  width: 16px;
  height: 2px;
  border-radius: 2px;
  background: var(--primary);
  transition: transform 220ms cubic-bezier(0.34, 1.56, 0.64, 1), opacity 200ms ease;
  opacity: 0;
}

.nav-btn:hover {
  color: var(--foreground);
  background: rgba(255,255,255,0.05) !important;
}

.nav-btn.active {
  color: var(--foreground);
  font-weight: 600;
  background: transparent !important;
}

.nav-btn.active::after {
  transform: translateX(-50%) scaleX(1);
  opacity: 1;
}

.nav-btn.ai-picks.active { color: #c084fc; }
.nav-btn.ai-picks.active::after { background: #c084fc; }

/* ─── Search + Log ────────────────────────────────────────── */
.right { gap: 0; }

.search {
  max-width: 240px;
  min-width: 180px;
}

.search :deep(.v-field) { font-size: 0.875rem; }
.search :deep(.v-field__prepend-inner .v-icon) { color: var(--muted-foreground); }
.search :deep(input::placeholder) { color: color-mix(in srgb, var(--muted-foreground) 70%, transparent); }

.log-btn {
  box-shadow: 0 4px 16px rgba(168,85,247,0.25);
  font-size: 0.875rem;
}

/* ─── Mobile right ────────────────────────────────────────── */
.mobile-right {
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
}

.log-btn-mobile {
  box-shadow: 0 4px 12px rgba(168,85,247,0.3);
  width: 34px !important;
  height: 34px !important;
}

/* ─── Profile avatar ──────────────────────────────────────── */
.profile-avatar {
  border: 1.5px solid rgba(255,255,255,0.1);
  background: rgba(168,85,247,0.1);
  transition: border-color 160ms ease;
}

.profile-avatar:hover { border-color: rgba(168,85,247,0.35); }

.avatar-fallback {
  font-size: 0.78rem;
  font-weight: 800;
  color: var(--foreground);
}

/* ─── Desktop profile menu ────────────────────────────────── */
.profile-menu {
  background: var(--card);
  border: 1px solid rgba(255,255,255,0.09);
  overflow: hidden;
  box-shadow:
    0 0 0 1px rgba(168,85,247,0.08),
    0 24px 48px rgba(0,0,0,0.55),
    0 8px 16px rgba(0,0,0,0.3);
}

.profile-menu__header {
  position: relative;
  overflow: hidden;
  padding: 20px 18px 18px;
}

.profile-menu__header-bg {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse 180px 120px at 90% 0%, rgba(168,85,247,0.2), transparent 70%),
    radial-gradient(ellipse 120px 80px at 0% 100%, rgba(88,28,135,0.15), transparent 60%);
  pointer-events: none;
}

.profile-menu__header-bg::after {
  content: '';
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(255,255,255,0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,0.03) 1px, transparent 1px);
  background-size: 24px 24px;
  mask-image: linear-gradient(to bottom, rgba(0,0,0,0.5), transparent);
}

.profile-menu__header-content {
  position: relative;
  display: flex;
  align-items: center;
  gap: 13px;
}

.profile-menu__avatar {
  border: 2px solid rgba(168,85,247,0.4);
  background: rgba(168,85,247,0.15);
  box-shadow: 0 0 0 4px rgba(168,85,247,0.08);
  flex-shrink: 0;
}

.profile-menu__avatar-fallback {
  font-size: 0.95rem;
  font-weight: 800;
  color: #c084fc;
  letter-spacing: 0.02em;
}

.profile-menu__identity { min-width: 0; }

.profile-menu__name {
  font-weight: 700;
  font-size: 0.95rem;
  color: var(--foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.profile-menu__subtext {
  margin-top: 2px;
  font-size: 0.8rem;
  color: var(--muted-foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.profile-menu__items {
  padding: 6px 8px 8px;
}

.profile-menu__item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 9px 10px;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 500;
  color: color-mix(in srgb, var(--foreground) 75%, transparent);
  text-decoration: none;
  background: transparent;
  border: none;
  cursor: pointer;
  text-align: left;
  transition: background 150ms ease, color 150ms ease, transform 150ms ease;
}

.profile-menu__item:hover {
  background: rgba(255,255,255,0.06);
  color: var(--foreground);
  transform: translateX(2px);
}

.profile-menu__item-icon {
  opacity: 0.6;
  transition: opacity 150ms ease;
  flex-shrink: 0;
}

.profile-menu__item:hover .profile-menu__item-icon { opacity: 1; }

.profile-menu__divider {
  height: 1px;
  background: rgba(255,255,255,0.06);
  margin: 6px 2px;
}

.profile-menu__item--danger {
  color: color-mix(in srgb, #f87171 70%, transparent);
}

.profile-menu__item--danger:hover {
  background: rgba(248,113,113,0.08);
  color: #fca5a5;
}

.profile-menu__item--danger .profile-menu__item-icon { color: inherit; }

/* ─── Public actions ──────────────────────────────────────── */
.public-actions { gap: 10px; }
.public-actions .active { color: var(--foreground); }

/* ─── Mobile bottom tab bar ───────────────────────────────── */
.mobile-tab-bar {
  display: none;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: rgba(12, 15, 20, 0.94);
  backdrop-filter: blur(20px) saturate(1.5);
  border-top: 1px solid rgba(255,255,255,0.08);
  padding: 6px 0 max(10px, env(safe-area-inset-bottom));
  justify-content: space-around;
  align-items: center;
}

@media (max-width: 700px) {
  .mobile-tab-bar { display: flex; }
}

.mobile-tab {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
  padding: 4px 12px;
  text-decoration: none;
  color: var(--muted-foreground);
  transition: color 180ms ease;
  flex: 1;
  min-width: 0;
}

.mobile-tab.active { color: var(--primary); }

.mobile-tab__icon {
  transition: transform 200ms cubic-bezier(0.34, 1.56, 0.64, 1);
}

.mobile-tab.active .mobile-tab__icon {
  transform: translateY(-2px);
}

.mobile-tab__label {
  font-size: 0.65rem;
  font-weight: 600;
  letter-spacing: 0.02em;
  white-space: nowrap;
}

/* ─── Mobile profile drawer ───────────────────────────────── */
.mobile-drawer-backdrop {
  position: fixed;
  inset: 0;
  z-index: 2000;
  background: rgba(0,0,0,0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: flex-end;
}

.mobile-drawer {
  width: 100%;
  background: var(--card);
  border-top: 1px solid rgba(255,255,255,0.1);
  border-radius: 20px 20px 0 0;
  overflow: hidden;
  padding-bottom: max(16px, env(safe-area-inset-bottom));
}

.mobile-drawer__handle {
  width: 36px;
  height: 4px;
  background: rgba(255,255,255,0.15);
  border-radius: 2px;
  margin: 12px auto 0;
}

.mobile-drawer__header {
  position: relative;
  overflow: hidden;
  padding: 20px 20px 16px;
}

.mobile-drawer__header-bg {
  position: absolute;
  inset: 0;
  background: radial-gradient(ellipse 240px 120px at 100% 0%, rgba(168,85,247,0.18), transparent 65%);
  pointer-events: none;
}

.mobile-drawer__header-content {
  position: relative;
  display: flex;
  align-items: center;
  gap: 14px;
}

.mobile-drawer__name {
  font-weight: 700;
  font-size: 1rem;
  color: var(--foreground);
}

.mobile-drawer__subtext {
  font-size: 0.82rem;
  color: var(--muted-foreground);
  margin-top: 2px;
}

.mobile-drawer__search {
  padding: 0 12px 4px;
}

/* ─── Drawer transition ───────────────────────────────────── */
.drawer-enter-active,
.drawer-leave-active {
  transition: opacity 240ms ease;
}

.drawer-enter-active .mobile-drawer,
.drawer-leave-active .mobile-drawer {
  transition: transform 280ms cubic-bezier(0.4, 0, 0.2, 1);
}

.drawer-enter-from,
.drawer-leave-to {
  opacity: 0;
}

.drawer-enter-from .mobile-drawer,
.drawer-leave-to .mobile-drawer {
  transform: translateY(100%);
}
</style>