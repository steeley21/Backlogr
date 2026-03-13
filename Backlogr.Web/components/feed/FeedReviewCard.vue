<script setup lang="ts">
import type { FeedReviewItem } from '~/types/feed'

const props = defineProps<{ item: FeedReviewItem }>()

const dateLabel = computed(() => {
  const d = new Date(props.item.reviewedAt)
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
          <span class="muted">reviewed</span>
          <NuxtLink :to="`/game/${item.game.gameId}`" class="game-link">
            {{ item.game.title }}
          </NuxtLink>
        </div>

        <div class="subline">
          <StarRating :rating="item.rating" />
          <span class="muted">{{ dateLabel }}</span>
        </div>
      </div>

      <div v-if="item.game.coverUrl" class="thumb">
        <v-img :src="item.game.coverUrl" cover />
      </div>
    </div>

    <div class="divider" />

    <p class="body">
      {{ item.text }}
    </p>

    <div class="d-flex align-center actions">
      <v-btn icon variant="text" density="comfortable" aria-label="Like">
        <v-icon :icon="item.liked ? 'mdi-heart' : 'mdi-heart-outline'" :color="item.liked ? 'primary' : undefined" />
      </v-btn>
      <span class="muted mr-4">{{ item.likeCount }}</span>

      <v-btn icon variant="text" density="comfortable" aria-label="Comments">
        <v-icon icon="mdi-message-outline" />
      </v-btn>
      <span class="muted">{{ item.commentCount }}</span>
    </div>
  </v-card>
</template>

<style scoped>
.card {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius) !important;
  padding: 18px;
  transition: border-color 120ms ease, transform 120ms ease, box-shadow 120ms ease;
}

.card:hover {
  border-color: rgba(168, 85, 247, 0.22);
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.28);
  transform: translateY(-1px);
}

.top-row {
  display: flex;
  align-items: flex-start;
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
}

.muted {
  color: var(--muted-foreground);
  font-size: 0.95rem;
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
  width: 56px;
  height: 76px;
  flex: 0 0 auto;
  border-radius: 10px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.08);
  opacity: 0.95;
}

.divider {
  margin: 12px 0 14px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.body {
  color: color-mix(in srgb, var(--foreground) 86%, transparent);
  line-height: 1.65;
  margin: 0 0 10px;
}

.actions :deep(.v-btn) {
  margin-left: -8px;
}
</style>