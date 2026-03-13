<!-- /pages/index.vue -->
<script setup lang="ts">
import type { FeedItem } from '~/types/feed'
import FeedReviewCard from '~/components/feed/FeedReviewCard.vue'
import FeedLogCard from '~/components/feed/FeedLogCard.vue'
import AiCalloutCard from '~/components/feed/AiCalloutCard.vue'
import StarRating from '~/components/shared/StarRating.vue'

const filter = ref<'all' | 'reviews' | 'logs'>('all')

const feedItems = ref<FeedItem[]>([
  {
    type: 'review',
    id: 'r1',
    user: { userId: 'u1', displayName: 'Maya Chen', avatarUrl: 'https://i.pravatar.cc/100?img=5' },
    game: { gameId: 101, title: "Ronin's Path", coverUrl: 'https://picsum.photos/seed/ronin/200/300' },
    rating: 4.5,
    reviewedAt: '2026-02-28T00:00:00Z',
    text:
      "Ronin's Path is a masterpiece. The swordplay feels so precise and satisfying — every duel had me holding my breath. This is one of those games that stays with you.",
    likeCount: 234,
    commentCount: 45,
    liked: false,
  },
  {
    type: 'log',
    id: 'l1',
    user: { userId: 'u2', displayName: 'Marcus Webb', avatarUrl: 'https://i.pravatar.cc/100?img=12' },
    game: { gameId: 102, title: 'Neon Drift: 2084', coverUrl: 'https://picsum.photos/seed/neon/200/300' },
    status: 'Playing',
    rating: 4.0,
    platform: 'PC',
    hours: 80,
    updatedAt: '2026-02-24T00:00:00Z',
  },
  {
    type: 'review',
    id: 'r2',
    user: { userId: 'u3', displayName: 'Sarah Kim', avatarUrl: 'https://i.pravatar.cc/100?img=20' },
    game: { gameId: 103, title: 'Whisperwood', coverUrl: 'https://picsum.photos/seed/whisper/200/300' },
    rating: 4.0,
    reviewedAt: '2026-02-24T00:00:00Z',
    text:
      "Quiet, contemplative, and deeply unsettling all at once. The environmental puzzles are clever without being frustrating. I wanted more of it.",
    likeCount: 156,
    commentCount: 28,
    liked: true,
  },
])

const visibleItems = computed(() => {
  if (filter.value === 'reviews') return feedItems.value.filter((x) => x.type === 'review')
  if (filter.value === 'logs') return feedItems.value.filter((x) => x.type === 'log')
  return feedItems.value
})

type TrendingGame = { gameId: number; title: string; coverUrl: string; avgRating: number; reviewCount: number }

const trendingGames = ref<TrendingGame[]>([
  { gameId: 201, title: "Ronin's Path", coverUrl: 'https://picsum.photos/seed/tr1/300/450', avgRating: 4.5, reviewCount: 18765 },
  { gameId: 202, title: 'Stellar Odyssey', coverUrl: 'https://picsum.photos/seed/tr2/300/450', avgRating: 4.0, reviewCount: 15023 },
  { gameId: 203, title: 'Automata: Reborn', coverUrl: 'https://picsum.photos/seed/tr3/300/450', avgRating: 4.5, reviewCount: 14321 },
  { gameId: 204, title: 'Ash & Ember', coverUrl: 'https://picsum.photos/seed/tr4/300/450', avgRating: 4.0, reviewCount: 11234 },
])
</script>

<template>
  <div class="page">
    <!-- Featured hero -->
    <v-card class="hero" rounded="xl" flat>
      <div class="hero-overlay" />

      <div class="hero-inner">
        <div class="overline">FEATURED</div>
        <div class="hero-title">Ronin's Path</div>
        <div class="hero-sub muted">
          Walk the path of the ronin in feudal Japan. A stunning action game with precise swordplay and a deeply personal story.
        </div>

        <v-btn color="primary" rounded="pill" class="text-none px-6 hero-btn" to="/game/101">
          View Game
        </v-btn>
      </div>
    </v-card>

    <v-row align="start" class="content" dense>
      <!-- Left: feed -->
      <v-col cols="12" md="8" class="feed-col">
        <div class="section-head">
          <div class="title">
            <v-icon icon="mdi-account-group" color="primary" size="20" />
            <span>Your Feed</span>
          </div>

          <v-btn-toggle v-model="filter" mandatory density="comfortable" rounded="pill" class="filter">
            <v-btn value="all" class="text-none">All</v-btn>
            <v-btn value="reviews" class="text-none">Reviews</v-btn>
            <v-btn value="logs" class="text-none">Logs</v-btn>
          </v-btn-toggle>
        </div>

        <div class="d-flex flex-column ga-4">
          <template v-for="item in visibleItems" :key="item.id">
            <FeedReviewCard v-if="item.type === 'review'" :item="item" />
            <FeedLogCard v-else :item="item" />
          </template>
        </div>
      </v-col>

      <!-- Right rail -->
      <v-col cols="12" md="4" class="rail-col">
        <div class="section-head right-head">
          <div class="title">
            <v-icon icon="mdi-trending-up" color="primary" size="20" />
            <span>Trending Games</span>
          </div>
        </div>

        <v-row dense class="trend-grid">
          <v-col v-for="g in trendingGames" :key="g.gameId" cols="6">
            <v-card rounded="xl" flat class="trend-card">
              <div class="trend-cover">
                <v-img :src="g.coverUrl" cover />
              </div>

              <div class="trend-meta">
                <div class="trend-title">{{ g.title }}</div>
                <div class="trend-rating">
                  <StarRating :rating="g.avgRating" :size="14" />
                  <span class="muted">{{ g.reviewCount.toLocaleString() }}</span>
                </div>
              </div>
            </v-card>
          </v-col>
        </v-row>

        <AiCalloutCard class="rail-block" />

        <v-card class="members rail-block" rounded="xl" flat>
          <div class="section-head compact">
            <div class="title">
              <v-icon icon="mdi-account-multiple" color="primary" size="20" />
              <span>Popular Members</span>
            </div>
          </div>

          <div class="d-flex flex-column ga-3">
            <div class="d-flex align-center" v-for="n in 3" :key="n">
              <v-avatar size="36" class="mr-3">
                <v-img :src="`https://i.pravatar.cc/100?img=${30 + n}`" cover />
              </v-avatar>
              <div class="flex-grow-1">
                <div class="font-weight-semibold">Member {{ n }}</div>
                <div class="muted text-caption">{{ 200 + n * 120 }} games • {{ 60 + n * 20 }} reviews</div>
              </div>
            </div>
          </div>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<style scoped>
.page {
  padding-top: 6px;
}

.content {
  margin-top: 18px;
}

/* HERO — closer to mock proportions + inset border */
.hero {
  position: relative;
  background: url('https://picsum.photos/seed/hero/1400/550');
  background-size: cover;
  background-position: center;
  border: 1px solid var(--border);
  border-radius: calc(var(--radius) + 4px) !important;
  overflow: hidden;
  height: 230px;
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.35);
}

.hero::after {
  content: "";
  position: absolute;
  inset: 0;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: calc(var(--radius) + 4px);
  pointer-events: none;
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(900px 260px at 20% 10%, rgba(168, 85, 247, 0.18), transparent 55%),
    linear-gradient(90deg, rgba(20, 24, 28, 0.94) 0%, rgba(20, 24, 28, 0.56) 55%, rgba(20, 24, 28, 0.32) 100%);
}

.hero-inner {
  position: relative;
  padding: 40px 40px;
  max-width: 620px;
}

.overline {
  color: var(--primary);
  font-weight: 700;
  letter-spacing: 0.18em;
  font-size: 0.75rem;
  margin-bottom: 10px;
}

.hero-title {
  font-size: 2.25rem;
  font-weight: 800;
  line-height: 1.05;
  margin-bottom: 10px;
  color: var(--foreground);
}

.hero-sub {
  max-width: 520px;
  line-height: 1.55;
  margin-bottom: 18px;
}

.hero-btn {
  box-shadow: 0 14px 28px rgba(0, 0, 0, 0.30);
}

/* SECTION HEADERS */
.section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
}

.section-head.compact {
  margin-bottom: 12px;
}

.title {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  font-size: 1.15rem;
  font-weight: 750;
}

/* FILTER PILLS — looks like 1 group */
.filter {
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid var(--border);
  padding: 4px;
  box-shadow: 0 10px 22px rgba(0, 0, 0, 0.22);
}

.filter :deep(.v-btn) {
  border-radius: 9999px !important;
  color: var(--muted-foreground);
  background: transparent;
  min-height: 34px;
}

.filter :deep(.v-btn--active) {
  background: rgba(255, 255, 255, 0.10);
  color: var(--foreground);
}

/* RIGHT RAIL STACK */
.rail-block {
  margin-top: 14px;
}

/* TRENDING */
.trend-grid {
  margin-top: -6px;
}

.trend-card {
  background: var(--card);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: var(--radius) !important;
  overflow: hidden;
  transition: border-color 120ms ease, transform 120ms ease, box-shadow 120ms ease;
}

.trend-card:hover {
  border-color: rgba(168, 85, 247, 0.18);
  box-shadow: 0 14px 28px rgba(0, 0, 0, 0.22);
  transform: translateY(-1px);
}

.trend-cover {
  aspect-ratio: 2 / 3;
  border-radius: 16px;
  overflow: hidden;
  margin: 10px 10px 0;
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.trend-meta {
  padding: 10px 10px 12px;
}

.trend-rating :deep(.v-icon) {
  opacity: 0.95;
}

.trend-title {
  font-weight: 700;
  font-size: 0.98rem;
  color: var(--foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.trend-rating {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 6px;
  font-size: 0.9rem;
}

.muted {
  color: var(--muted-foreground);
}

/* MEMBERS */
.members {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius) !important;
  padding: 16px;
}

/* Responsive hero */
@media (max-width: 600px) {
  .hero {
    height: 250px;
  }

  .hero-inner {
    padding: 34px 24px;
  }

  .hero-title {
    font-size: 2rem;
  }
}

@media (min-width: 960px) {
  /* keep feed + rail on the same row */
  .content {
    flex-wrap: nowrap !important;
    column-gap: 22px;
  }

  /* left: fill remaining space */
  .feed-col {
    flex: 1 1 auto !important;
    max-width: none !important;
    min-width: 0; /* prevents overflow pushing rail off-screen */
  }

  /* right: fixed width like the mock */
  .rail-col {
    flex: 0 0 360px !important;
    width: 360px !important;
    max-width: 360px !important;
  }
}
</style>