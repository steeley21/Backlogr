<!-- /components/app/AppTopBar.vue -->
<script setup lang="ts">

import BacklogrLogo from '~/components/shared/BacklogrLogo.vue'
type NavItem = { label: string; to: string; icon?: string }

const navItems: NavItem[] = [
  { label: 'Feed', to: '/' },
  { label: 'Browse', to: '/browse' },
  { label: 'Library', to: '/library' },
  { label: 'AI Picks', to: '/recommend', icon: 'mdi-sparkles' },
]

const route = useRoute()
const search = ref('')

function isActive(path: string): boolean {
  return route.path === path
}

async function submitSearch() {
  const q = search.value.trim()
  if (!q) {
    await navigateTo('/browse')
    return
  }
  await navigateTo({ path: '/browse', query: { q } })
}
</script>

<template>
  <v-app-bar height="76" flat class="appbar">
    <div class="bar-inner">
      <div class="left">
        <BacklogrLogo />
      </div>

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
          <v-icon v-if="item.icon" :icon="item.icon" size="18" class="mr-2" />
          {{ item.label }}
        </v-btn>
      </div>

      <div class="right d-flex align-center">
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

        <v-btn to="/log" color="primary" rounded="pill" class="text-none px-5 mr-3 log-btn" prepend-icon="mdi-plus">
          Log
        </v-btn>

        <v-badge dot color="primary" class="mr-3" offset-x="2" offset-y="2">
          <v-btn icon variant="text" aria-label="Notifications">
            <v-icon icon="mdi-bell-outline" />
          </v-btn>
        </v-badge>

        <v-btn icon variant="text" to="/profile" aria-label="Profile">
          <v-avatar size="34">
            <v-img src="https://i.pravatar.cc/100?img=12" alt="Profile avatar" cover />
          </v-avatar>
        </v-btn>
      </div>
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
  max-width: 320px;
  min-width: 220px;
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

@media (max-width: 960px) {
  .bar-inner {
    grid-template-columns: 200px 1fr;
    grid-template-areas:
      "left right"
      "center center";
  }
  .left { grid-area: left; }
  .center { grid-area: center; justify-content: flex-start; }
  .right { grid-area: right; justify-content: flex-end; }
}
</style>