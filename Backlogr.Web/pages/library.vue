<script setup lang="ts">
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'

const tab = ref<'backlog' | 'playing' | 'played'>('backlog')

type LibraryGame = { gameId: number; title: string; coverUrl: string }

const data = {
  backlog: [
    { gameId: 201, title: "Ronin's Path", coverUrl: 'https://picsum.photos/seed/l1/300/450' },
    { gameId: 202, title: 'Stellar Odyssey', coverUrl: 'https://picsum.photos/seed/l2/300/450' },
  ],
  playing: [
    { gameId: 203, title: 'Neon Drift: 2084', coverUrl: 'https://picsum.photos/seed/l3/300/450' },
  ],
  played: [
    { gameId: 204, title: 'Ash & Ember', coverUrl: 'https://picsum.photos/seed/l4/300/450' },
  ],
} satisfies Record<string, LibraryGame[]>

const games = computed(() => data[tab.value])
</script>

<template>
  <div>
    <SectionHeader icon="mdi-bookshelf" title="Your Library" />

    <v-btn-toggle v-model="tab" mandatory rounded="pill" class="filter">
      <v-btn value="backlog" class="text-none">Backlog</v-btn>
      <v-btn value="playing" class="text-none">Playing</v-btn>
      <v-btn value="played" class="text-none">Played</v-btn>
    </v-btn-toggle>

    <v-row dense class="mt-4">
      <v-col v-for="g in games" :key="g.gameId" cols="6" sm="4" md="3" lg="2">
        <GamePosterCard :title="g.title" :cover-url="g.coverUrl" :to="`/game/${g.gameId}`" />
      </v-col>
    </v-row>
  </div>
</template>

<style scoped>
.filter {
  background: rgba(255,255,255,0.06);
  border: 1px solid var(--border);
  padding: 4px;
  box-shadow: 0 10px 22px rgba(0,0,0,0.22);
}

.filter :deep(.v-btn) {
  border-radius: 9999px !important;
  color: var(--muted-foreground);
  background: transparent;
  min-height: 34px;
}

.filter :deep(.v-btn--active) {
  background: rgba(255,255,255,0.10);
  color: var(--foreground);
}
</style>