<script setup lang="ts">
import { computed } from 'vue'
import StarRating from '~/components/shared/StarRating.vue'
import type { FeedLogItem } from '~/types/feed'

const props = defineProps<{ item: FeedLogItem }>()

const timeLabel = computed(() => {
  const d = new Date(props.item.updatedAt)
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
})

const profilePath = computed(() => {
  return `/u/${props.item.user.userName}`
})

const avatarInitials = computed(() => {
  const source = props.item.user.displayName || props.item.user.userName || 'B'
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
</script>

<template>
  <v-card class="card" rounded="xl" flat>
    <div class="top-row">
      <NuxtLink :to="profilePath" class="avatar-link" :aria-label="`Open ${item.user.displayName}`">
        <v-avatar size="44" class="avatar">
          <v-img v-if="item.user.avatarUrl" :src="item.user.avatarUrl" :alt="item.user.displayName" cover />
          <span v-else class="avatar-fallback">{{ avatarInitials }}</span>
        </v-avatar>
      </NuxtLink>

      <div class="content">
        <div class="headline">
          <NuxtLink :to="profilePath" class="name-link">
            {{ item.user.displayName }}
          </NuxtLink>
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
          <span v-if="typeof item.hours === 'number'" class="muted">{{ item.hours }}h</span>
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
  padding: 16px 18px 0;
  transition: border-color 200ms ease, transform 200ms cubic-bezier(0.25, 0.46, 0.45, 0.94), box-shadow 200ms ease;
  position: relative;
  overflow: hidden;
}

.card::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 2px;
  background: linear-gradient(to bottom, rgba(168, 85, 247, 0), rgba(168, 85, 247, 0));
  transition: background 250ms ease;
  border-radius: 2px 0 0 2px;
}

.card:hover {
  border-color: rgba(168, 85, 247, 0.28);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35), 0 2px 8px rgba(168, 85, 247, 0.08);
  transform: translateY(-2px);
}

.card:hover::before {
  background: linear-gradient(to bottom, rgba(168, 85, 247, 0.8), rgba(168, 85, 247, 0.2));
}

.card:hover .thumb {
  opacity: 1;
  transform: scale(1.03);
  border-color: rgba(168, 85, 247, 0.3);
}

.top-row {
  display: flex;
  align-items: center;
  gap: 14px;
}

.avatar,
.avatar-link {
  flex: 0 0 auto;
}

.avatar-link {
  text-decoration: none;
}

.avatar :deep(.v-avatar) {
  background: rgba(168, 85, 247, 0.15);
  border: 1.5px solid rgba(168, 85, 247, 0.25);
}

.avatar-fallback {
  color: var(--primary);
  font-size: 0.82rem;
  font-weight: 800;
  letter-spacing: 0.02em;
}

.content {
  flex: 1 1 auto;
  min-width: 0;
}

.headline {
  display: flex;
  align-items: baseline;
  flex-wrap: wrap;
  gap: 8px;
  font-size: 0.94rem;
}

.name-link {
  font-weight: 700;
  color: var(--foreground);
  text-decoration: none;
  transition: color 150ms ease;
}

.name-link:hover {
  color: var(--primary);
}

.subline {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 6px;
  flex-wrap: wrap;
}

.muted {
  color: var(--muted-foreground);
  font-size: 0.88rem;
}

.status {
  color: var(--primary);
  font-weight: 700;
  font-size: 0.92rem;
  background: rgba(168, 85, 247, 0.1);
  padding: 1px 8px;
  border-radius: 20px;
  border: 1px solid rgba(168, 85, 247, 0.2);
}

.game-link {
  color: var(--foreground);
  text-decoration: none;
  font-weight: 600;
  transition: color 150ms ease;
}

.game-link:hover {
  color: #d8b4fe;
}

.thumb {
  width: 44px;
  height: 58px;
  flex: 0 0 auto;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.1);
  opacity: 0.85;
  transition: opacity 200ms ease, transform 200ms ease, border-color 200ms ease;
}

.divider {
  margin-top: 14px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
}
</style>
