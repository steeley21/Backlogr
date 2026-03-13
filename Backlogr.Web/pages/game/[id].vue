<script setup lang="ts">
import SectionHeader from '~/components/layout/SectionHeader.vue'

const route = useRoute()
const gameId = computed(() => Number(route.params.id))

const tab = ref<'about' | 'reviews' | 'activity'>('about')

const game = computed(() => ({
  title: `Game #${gameId.value}`,
  coverUrl: 'https://picsum.photos/seed/gamehero/1400/700',
  summary: 'This is placeholder game detail content. Later this will come from IGDB + your API.',
  genres: ['Action', 'Adventure'],
}))
</script>

<template>
  <div>
    <v-card class="hero" rounded="xl" flat>
      <div class="hero-overlay" />
      <div class="hero-inner">
        <div class="text-h4 font-weight-bold">{{ game.title }}</div>
        <div class="muted mt-2">{{ game.genres.join(' • ') }}</div>

        <div class="mt-4 d-flex ga-3">
          <v-btn color="primary" rounded="pill" class="text-none px-6" :to="`/log?game=${gameId}`">Log</v-btn>
          <v-btn variant="text" rounded="pill" class="text-none">Add to Library</v-btn>
        </div>
      </div>
    </v-card>

    <div class="mt-5">
      <v-btn-toggle v-model="tab" mandatory rounded="pill" class="filter">
        <v-btn value="about" class="text-none">About</v-btn>
        <v-btn value="reviews" class="text-none">Reviews</v-btn>
        <v-btn value="activity" class="text-none">Activity</v-btn>
      </v-btn-toggle>

      <v-card class="panel mt-4" rounded="xl" flat>
        <div v-if="tab === 'about'">
          <SectionHeader icon="mdi-information" title="About" />
          <div class="muted">{{ game.summary }}</div>
        </div>

        <div v-else-if="tab === 'reviews'">
          <SectionHeader icon="mdi-message-text" title="Reviews" />
          <div class="muted">Reviews will go here.</div>
        </div>

        <div v-else>
          <SectionHeader icon="mdi-history" title="Activity" />
          <div class="muted">Recent activity will go here.</div>
        </div>
      </v-card>
    </div>
  </div>
</template>

<style scoped>
.hero {
  position: relative;
  background: url('https://picsum.photos/seed/gamehero/1400/700');
  background-size: cover;
  background-position: center;
  border: 1px solid var(--border);
  overflow: hidden;
  height: 240px;
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, rgba(20,24,28,0.92), rgba(20,24,28,0.35));
}

.hero-inner {
  position: relative;
  padding: 34px 34px;
  max-width: 720px;
}

.panel {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 16px;
}

.muted {
  color: var(--muted-foreground);
}

.filter {
  background: rgba(255,255,255,0.06);
  border: 1px solid var(--border);
  padding: 4px;
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