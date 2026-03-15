<script setup lang="ts">
import { computed } from 'vue'
import type { PublicProfileReviewSummaryDto } from '~/types/profile'

const props = defineProps<{
  review: PublicProfileReviewSummaryDto
}>()

const dateLabel = computed(() => {
  const dateSource = props.review.updatedAt || props.review.createdAt
  const date = new Date(dateSource)

  return date.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
})

const previewText = computed(() => {
  const value = props.review.text.trim()

  if (value.length <= 220) {
    return value
  }

  return `${value.slice(0, 220).trimEnd()}…`
})
</script>

<template>
  <v-card class="review-card" rounded="xl" flat>
    <div class="review-card__inner">
      <NuxtLink :to="`/game/${review.gameId}`" class="cover-link" :aria-label="`Open ${review.gameTitle}`">
        <v-img
          :src="review.coverImageUrl || '/images/fallback-game-cover.svg'"
          :alt="review.gameTitle"
          cover
          class="cover"
        />
      </NuxtLink>

      <div class="content">
        <div class="headline-row">
          <div>
            <NuxtLink :to="`/game/${review.gameId}`" class="game-link">
              {{ review.gameTitle }}
            </NuxtLink>

            <div class="meta-row">
              <v-chip
                v-if="review.hasSpoilers"
                size="small"
                color="warning"
                variant="tonal"
              >
                Spoilers
              </v-chip>
              <span class="muted">{{ dateLabel }}</span>
            </div>
          </div>

          <div class="stats muted">
            <span>{{ review.likeCount }} likes</span>
            <span>{{ review.commentCount }} comments</span>
          </div>
        </div>

        <p class="body">{{ previewText }}</p>
      </div>
    </div>
  </v-card>
</template>

<style scoped>
.review-card {
  background: var(--card);
  border: 1px solid var(--border);
}

.review-card__inner {
  display: grid;
  grid-template-columns: 72px minmax(0, 1fr);
  gap: 16px;
  padding: 16px;
}

.cover-link {
  display: block;
}

.cover {
  aspect-ratio: 2 / 3;
  border-radius: 14px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.content {
  min-width: 0;
}

.headline-row {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  justify-content: space-between;
}

.game-link {
  color: var(--foreground);
  font-weight: 700;
  text-decoration: none;
}

.game-link:hover {
  text-decoration: underline;
}

.meta-row {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 8px;
}

.stats {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
  white-space: nowrap;
}

.muted {
  color: var(--muted-foreground);
  font-size: 0.92rem;
}

.body {
  color: color-mix(in srgb, var(--foreground) 86%, transparent);
  line-height: 1.65;
  margin: 14px 0 0;
  white-space: pre-wrap;
}

@media (max-width: 700px) {
  .review-card__inner {
    grid-template-columns: 1fr;
  }

  .cover-link {
    max-width: 110px;
  }

  .headline-row {
    flex-direction: column;
  }

  .stats {
    align-items: flex-start;
  }
}
</style>
