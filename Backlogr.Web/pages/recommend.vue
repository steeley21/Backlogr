<script setup lang="ts">
import SectionHeader from '~/components/layout/SectionHeader.vue'
import GamePosterCard from '~/components/game/GamePosterCard.vue'

const prompt = ref('Recommend games like Ronin’s Path and Neon Drift…')

type RecGame = { gameId: number; title: string; coverUrl: string; subtitle: string }

const recs = ref<RecGame[]>([
  { gameId: 301, title: 'Bladefall', coverUrl: 'https://picsum.photos/seed/r1/300/450', subtitle: 'Because you like action duels' },
  { gameId: 302, title: 'Skyline Circuit', coverUrl: 'https://picsum.photos/seed/r2/300/450', subtitle: 'Fast neon racing vibes' },
  { gameId: 303, title: 'Quiet Hollow', coverUrl: 'https://picsum.photos/seed/r3/300/450', subtitle: 'Moody puzzle storytelling' },
])
</script>

<template>
  <div>
    <SectionHeader icon="mdi-sparkles" title="AI Picks" right-text="Personalized recommendations" />

    <v-card class="panel" rounded="xl" flat>
      <div class="text-h6 font-weight-semibold mb-2">Tell Backlogr what you’re into</div>
      <v-textarea v-model="prompt" auto-grow rows="3" variant="solo-filled" rounded="xl" hide-details />
      <div class="mt-3">
        <v-btn color="primary" rounded="pill" class="text-none px-6">Get Picks</v-btn>
      </div>
    </v-card>

    <div class="mt-5">
      <SectionHeader icon="mdi-star" title="Recommended for you" />
      <v-row dense>
        <v-col v-for="g in recs" :key="g.gameId" cols="6" sm="4" md="3" lg="2">
          <GamePosterCard :title="g.title" :cover-url="g.coverUrl" :subtitle="g.subtitle" :to="`/game/${g.gameId}`" />
        </v-col>
      </v-row>
    </div>
  </div>
</template>

<style scoped>
.panel {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 16px;
}
</style>