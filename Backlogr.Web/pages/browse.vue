<script setup lang="ts">
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'

const route = useRoute()
const q = computed(() => String(route.query.q ?? ''))

type BrowseGame = { gameId: number; title: string; coverUrl: string; subtitle: string }

const games = ref<BrowseGame[]>([
  { gameId: 101, title: "Ronin's Path", coverUrl: 'https://picsum.photos/seed/b1/300/450', subtitle: 'Action • 2026' },
  { gameId: 102, title: 'Neon Drift: 2084', coverUrl: 'https://picsum.photos/seed/b2/300/450', subtitle: 'Racing • 2024' },
  { gameId: 103, title: 'Whisperwood', coverUrl: 'https://picsum.photos/seed/b3/300/450', subtitle: 'Puzzle • 2025' },
  { gameId: 104, title: 'Ash & Ember', coverUrl: 'https://picsum.photos/seed/b4/300/450', subtitle: 'Adventure • 2023' },
])
</script>

<template>
  <div>
    <SectionHeader icon="mdi-magnify" title="Browse" :right-text="q ? `Results for “${q}”` : 'Discover games'" />

    <v-row dense>
      <v-col v-for="g in games" :key="g.gameId" cols="6" sm="4" md="3" lg="2">
        <GamePosterCard
          :title="g.title"
          :cover-url="g.coverUrl"
          :subtitle="g.subtitle"
          :to="`/game/${g.gameId}`"
        />
      </v-col>
    </v-row>
  </div>
</template>