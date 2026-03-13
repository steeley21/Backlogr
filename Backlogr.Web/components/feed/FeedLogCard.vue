<script setup lang="ts">
import type { FeedLogItem } from '~/types/feed'

const props = defineProps<{ item: FeedLogItem }>()

const timeLabel = computed(() => {
  const d = new Date(props.item.updatedAt)
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
})
</script>

<template>
  <v-card class="card" rounded="xl" flat>
    <div class="top-row">
      <v-avatar size="44" class="avatar">
        <v-img :src="item.user.avatarUrl" :alt="item.user.displayName" cover />
      </v-avatar>

      <div class="content">
        <div class="headline">
          <span class="name">{{ item.user.displayName }}</span>
          <span class="muted">added</span>

          <NuxtLink :to="`/game/${item.game.gameId}`" class="game-link">
            {{ item.game.title }}
          </NuxtLink>

          <span class="muted">to</span>
          <span class="status">{{ item.status }}</span>
        </div>

        <div class="subline">
          <StarRating v-if="typeof item.rating === 'number'" :rating="item.rating" />
          <span v-if="item.platform" class="muted">{{ item.platform }}</span>
          <span v-if="item.hours" class="muted">{{ item.hours }}h</span>
          <span class="muted">{{ timeLabel }}</span>
        </div>
      </div>

      <div v-if="item.game.coverUrl" class="thumb">
        <v-img :src="item.game.coverUrl" cover />
      </div>
    </div>

    <div class="divider" />
  </v-card>
</template>

<style scoped>
.card {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius) !important;
  padding: 16px 18px;
  transition: border-color 120ms ease, transform 120ms ease, box-shadow 120ms ease;
}

.card:hover {
  border-color: rgba(168, 85, 247, 0.22);
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.28);
  transform: translateY(-1px);
}

.top-row {
  display: flex;
  align-items: center;
  gap: 14px;
}

.avatar {
  flex: 0 0 auto;
}

.content {
  flex: 1 1 auto;
  min-width: 0;
}

.headline {
  display: flex;
  align-items: baseline;
  flex-wrap: wrap;
  gap: 10px;
}

.name {
  font-weight: 650;
  color: var(--foreground);
}

.subline {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 6px;
  flex-wrap: wrap;
}

.muted {
  color: var(--muted-foreground);
  font-size: 0.95rem;
}

.status {
  color: var(--primary);
  font-weight: 650;
}

.game-link {
  color: var(--foreground);
  text-decoration: none;
  font-weight: 600;
}

.game-link:hover {
  text-decoration: underline;
}

.thumb {
  width: 44px;
  height: 56px;
  flex: 0 0 auto;
  border-radius: 10px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.08);
  opacity: 0.95;
}

.divider {
  margin-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}
</style>