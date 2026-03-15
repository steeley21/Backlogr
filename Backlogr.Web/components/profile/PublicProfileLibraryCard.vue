<script setup lang="ts">
import { computed } from 'vue'
import StarRating from '~/components/shared/StarRating.vue'
import type { PublicProfileLibraryItemResponseDto } from '~/types/profile'

const props = defineProps<{
  item: PublicProfileLibraryItemResponseDto
}>()

const subtitle = computed(() => {
  const parts: string[] = [props.item.status]

  if (props.item.platform) {
    parts.push(props.item.platform)
  }

  if (typeof props.item.hours === 'number' && props.item.hours > 0) {
    parts.push(`${props.item.hours}h`)
  }

  return parts.join(' • ')
})

const updatedLabel = computed(() => {
  const date = new Date(props.item.updatedAt)

  return date.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
})
</script>

<template>
  <v-card class="library-card" rounded="xl" flat :to="`/game/${item.gameId}`" link>
    <div class="cover-wrap">
      <v-img
        :src="item.coverImageUrl || '/images/fallback-game-cover.svg'"
        :alt="item.gameTitle"
        cover
        class="cover"
      />
    </div>

    <div class="meta">
      <div class="title">{{ item.gameTitle }}</div>
      <div class="subtitle">{{ subtitle }}</div>

      <div class="detail-row">
        <StarRating v-if="typeof item.rating === 'number'" :rating="item.rating" />
        <span class="muted">Updated {{ updatedLabel }}</span>
      </div>
    </div>
  </v-card>
</template>

<style scoped>
.library-card {
  height: 100%;
  background: var(--card);
  border: 1px solid var(--border);
  transition: transform 120ms ease, box-shadow 120ms ease, border-color 120ms ease;
}

.library-card:hover {
  transform: translateY(-1px);
  border-color: rgba(168, 85, 247, 0.18);
  box-shadow: 0 14px 28px rgba(0, 0, 0, 0.22);
}

.cover-wrap {
  padding: 12px 12px 0;
}

.cover {
  aspect-ratio: 2 / 3;
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.meta {
  padding: 12px 14px 16px;
}

.title {
  color: var(--foreground);
  font-weight: 700;
  line-height: 1.35;
}

.subtitle,
.muted {
  color: var(--muted-foreground);
}

.subtitle {
  margin-top: 6px;
  font-size: 0.95rem;
}

.detail-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-top: 12px;
  flex-wrap: wrap;
}

.muted {
  font-size: 0.88rem;
}
</style>
