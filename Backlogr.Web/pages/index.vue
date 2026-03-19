<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()

const primaryCtaTo = computed(() => {
  return authStore.isAuthenticated ? '/feed' : '/register'
})

const primaryCtaLabel = computed(() => {
  return authStore.isAuthenticated ? 'Open your feed' : 'Create account'
})

const secondaryCtaTo = computed(() => {
  return authStore.isAuthenticated ? '/browse' : '/login'
})

const secondaryCtaLabel = computed(() => {
  return authStore.isAuthenticated ? 'Browse games' : 'Sign in'
})

const featureCards = [
  {
    icon: 'mdi-controller-classic-outline',
    title: 'Track your library',
    copy: 'Log what you are playing, what you have finished, and what still belongs in your backlog.',
  },
  {
    icon: 'mdi-star-outline',
    title: 'Write quick reviews',
    copy: 'Capture your thoughts, spoiler-tag them when needed, and keep a history of how you felt about each game.',
  },
  {
    icon: 'mdi-account-group-outline',
    title: 'Follow activity',
    copy: 'See your own recent logs and reviews first, then build out a social feed as you connect with other players.',
  },
  {
    icon: 'mdi-shimmer',
    title: 'Get AI picks',
    copy: 'Use your logged games and ratings to generate recommendation ideas for what to play next.',
  },
] as const

const steps = [
  'Create an account and build your player profile.',
  'Browse the catalog and log games into your library.',
  'Write reviews, rate your experience, and keep your history in one place.',
  'Use recommendations when you need help choosing the next game.',
] as const
</script>

<template>
  <div class="landing-page">
    <v-card class="hero" rounded="xl" flat>
      <div class="hero-glow" />

      <div class="hero-grid">
        <div class="hero-copy">
          <div class="overline">WELCOME TO BACKLOGR</div>
          <h1 class="hero-title">A social backlog for the games you actually play.</h1>
          <p class="hero-subtitle">
            Backlogr gives you one place to log games, write reviews, follow activity, and get AI-powered suggestions for what to pick up next.
          </p>

          <div class="hero-actions">
            <v-btn color="primary" rounded="pill" class="text-none px-6" :to="primaryCtaTo">
              {{ primaryCtaLabel }}
            </v-btn>

            <v-btn variant="text" rounded="pill" class="text-none" :to="secondaryCtaTo">
              {{ secondaryCtaLabel }}
            </v-btn>
          </div>
        </div>

        <div class="hero-panel">
          <div class="panel-kicker">What Backlogr helps you do</div>

          <div class="panel-stats">
            <div class="stat-card">
              <div class="stat-value">Library</div>
              <div class="stat-label">Track statuses, ratings, and notes</div>
            </div>

            <div class="stat-card">
              <div class="stat-value">Reviews</div>
              <div class="stat-label">Capture reactions while they are fresh</div>
            </div>

            <div class="stat-card">
              <div class="stat-value">Feed</div>
              <div class="stat-label">Keep up with your own activity and others</div>
            </div>
          </div>
        </div>
      </div>
    </v-card>

    <v-row class="section-row" dense>
      <v-col cols="12" lg="8">
        <v-card class="section-card" rounded="xl" flat>
          <div class="section-overline">CORE FEATURES</div>
          <h2 class="section-title">Built for tracking, reviewing, and discovering games.</h2>

          <v-row class="mt-2" dense>
            <v-col
              v-for="feature in featureCards"
              :key="feature.title"
              cols="12"
              sm="6"
            >
              <div class="feature-card">
                <div class="feature-icon-wrap">
                  <v-icon :icon="feature.icon" size="22" color="primary" />
                </div>
                <div class="feature-title">{{ feature.title }}</div>
                <div class="muted feature-copy">{{ feature.copy }}</div>
              </div>
            </v-col>
          </v-row>
        </v-card>
      </v-col>

      <v-col cols="12" lg="4">
        <v-card class="section-card" rounded="xl" flat>
          <div class="section-overline">HOW IT FLOWS</div>
          <h2 class="section-title">Start simple and build from there.</h2>

          <div class="steps-list mt-5">
            <div v-for="(step, index) in steps" :key="step" class="step-item">
              <div class="step-badge">{{ index + 1 }}</div>
              <div class="step-copy">{{ step }}</div>
            </div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-card class="closing-card" rounded="xl" flat>
      <div class="section-overline">READY TO START?</div>
      <div class="closing-grid">
        <div>
          <h2 class="section-title mb-2">Stop losing track of what you played.</h2>
          <div class="muted closing-copy">
            Build a cleaner record of your gaming history and keep recommendations close when your backlog gets hard to choose from.
          </div>
        </div>

        <div class="closing-actions">
          <v-btn color="primary" rounded="pill" class="text-none px-6" :to="primaryCtaTo">
            {{ primaryCtaLabel }}
          </v-btn>

          <v-btn variant="text" rounded="pill" class="text-none" :to="secondaryCtaTo">
            {{ secondaryCtaLabel }}
          </v-btn>
        </div>
      </div>
    </v-card>
  </div>
</template>

<style scoped>
.landing-page {
  display: flex;
  flex-direction: column;
  gap: 18px;
  padding-top: 6px;
}

.hero,
.section-card,
.closing-card {
  background: color-mix(in srgb, var(--card) 92%, black);
  border: 1px solid var(--border);
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.28);
}

.hero {
  position: relative;
  overflow: hidden;
  background:
    radial-gradient(860px 320px at 18% 0%, rgba(168, 85, 247, 0.2), transparent 55%),
    linear-gradient(135deg, rgba(20, 24, 28, 0.98), rgba(28, 34, 40, 0.96));
}

.hero::after {
  content: "";
  position: absolute;
  inset: 0;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: calc(var(--radius) + 4px);
  pointer-events: none;
}

.hero-glow {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(520px 220px at 84% 0%, rgba(168, 85, 247, 0.14), transparent 58%);
}

.hero-grid {
  position: relative;
  display: grid;
  grid-template-columns: minmax(0, 1.4fr) minmax(280px, 0.9fr);
  gap: 24px;
  padding: 34px;
}

.hero-copy {
  max-width: 660px;
}

.overline,
.section-overline,
.panel-kicker {
  color: var(--primary);
  font-weight: 700;
  letter-spacing: 0.16em;
  font-size: 0.75rem;
}

.hero-title,
.section-title {
  color: var(--foreground);
  font-weight: 800;
  line-height: 1.08;
}

.hero-title {
  margin-top: 10px;
  font-size: 2.6rem;
  max-width: 620px;
}

.hero-subtitle,
.muted {
  color: var(--muted-foreground);
}

.hero-subtitle {
  margin-top: 14px;
  max-width: 560px;
  line-height: 1.65;
  font-size: 1rem;
}

.hero-actions,
.closing-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.hero-actions {
  margin-top: 22px;
}

.hero-panel {
  align-self: stretch;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 24px;
  padding: 20px;
}

.panel-stats {
  display: grid;
  gap: 12px;
  margin-top: 18px;
}

.stat-card,
.feature-card {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 20px;
}

.stat-card {
  padding: 16px;
}

.stat-value {
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--foreground);
}

.stat-label,
.feature-copy,
.step-copy,
.closing-copy {
  line-height: 1.6;
}

.stat-label {
  margin-top: 6px;
  color: var(--muted-foreground);
}

.section-row {
  margin-top: 0;
}

.section-card {
  height: 100%;
  padding: 24px;
}

.section-title {
  margin-top: 10px;
  font-size: 1.8rem;
}

.feature-card {
  height: 100%;
  padding: 18px;
}

.feature-icon-wrap {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border-radius: 14px;
  background: rgba(168, 85, 247, 0.08);
  margin-bottom: 14px;
}

.feature-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--foreground);
  margin-bottom: 8px;
}

.steps-list {
  display: grid;
  gap: 12px;
}

.step-item {
  display: grid;
  grid-template-columns: 40px 1fr;
  gap: 12px;
  align-items: start;
}

.step-badge {
  display: grid;
  place-items: center;
  width: 40px;
  height: 40px;
  border-radius: 999px;
  background: rgba(168, 85, 247, 0.14);
  color: var(--foreground);
  font-weight: 800;
}

.step-copy {
  color: var(--muted-foreground);
  padding-top: 8px;
}

.closing-card {
  padding: 24px;
}

.closing-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 16px;
  align-items: center;
  margin-top: 10px;
}

@media (max-width: 960px) {
  .hero-grid,
  .closing-grid {
    grid-template-columns: 1fr;
  }

  .hero-title {
    font-size: 2.2rem;
  }
}

@media (max-width: 600px) {
  .hero-grid {
    padding: 24px 20px;
    gap: 20px;
  }

  .section-card,
  .closing-card {
    padding: 20px;
  }

  .hero-title {
    font-size: 1.75rem;
  }

  .hero-subtitle {
    font-size: 0.92rem;
  }

  .section-title {
    font-size: 1.4rem;
  }

  .hero-actions,
  .closing-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .hero-actions :deep(.v-btn),
  .closing-actions :deep(.v-btn) {
    width: 100%;
  }

  /* Collapse hero panel into a horizontal scroll row */
  .hero-panel {
    padding: 16px;
  }

  .panel-stats {
    grid-template-columns: repeat(3, 1fr);
    gap: 8px;
    margin-top: 12px;
  }

  .stat-card {
    padding: 12px 10px;
  }

  .stat-value {
    font-size: 0.9rem;
  }

  .stat-label {
    font-size: 0.72rem;
    margin-top: 4px;
  }

  .feature-card {
    padding: 14px;
  }

  .closing-grid {
    gap: 20px;
  }
}
</style>
