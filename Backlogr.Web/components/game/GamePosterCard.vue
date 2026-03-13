<script setup lang="ts">
const props = defineProps<{
  title: string
  coverUrl: string
  subtitle?: string
  to?: string
}>()
</script>

<template>
  <v-card class="card" rounded="xl" flat :to="to" :link="!!to">
    <div class="cover">
      <v-img :src="coverUrl" cover />
    </div>

    <div class="meta">
      <div class="title">{{ title }}</div>
      <div v-if="subtitle" class="subtitle">{{ subtitle }}</div>
    </div>
  </v-card>
</template>

<style scoped>
.card {
  background: var(--card);
  border: 1px solid rgba(255,255,255,0.06);
  border-radius: var(--radius) !important;
  overflow: hidden;
  transition: transform 120ms ease, box-shadow 120ms ease, border-color 120ms ease;
}

.card:hover {
  transform: translateY(-1px);
  border-color: rgba(168, 85, 247, 0.18);
  box-shadow: 0 14px 28px rgba(0,0,0,0.22);
}

.cover {
  aspect-ratio: 2 / 3;
  margin: 10px;
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(255,255,255,0.06);
}

.meta {
  padding: 10px 12px 14px;
}

.title {
  color: var(--foreground);
  font-weight: 700;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.subtitle {
  margin-top: 6px;
  color: var(--muted-foreground);
  font-size: 0.92rem;
}
</style>