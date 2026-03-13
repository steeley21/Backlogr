<script setup lang="ts">
import SectionHeader from '~/components/layout/SectionHeader.vue'

const user = {
  displayName: 'Kate Steele',
  handle: '@katesteel',
  avatarUrl: 'https://i.pravatar.cc/120?img=12',
  stats: { games: 124, reviews: 38, followers: 52, following: 44 },
}
</script>

<template>
  <div>
    <SectionHeader icon="mdi-account" title="Profile" />

    <v-card class="panel" rounded="xl" flat>
      <div class="d-flex align-center">
        <v-avatar size="72" class="mr-4">
          <v-img :src="user.avatarUrl" cover />
        </v-avatar>

        <div class="flex-grow-1">
          <div class="text-h5 font-weight-bold">{{ user.displayName }}</div>
          <div class="muted">{{ user.handle }}</div>
        </div>

        <v-btn color="primary" rounded="pill" class="text-none px-6">Edit</v-btn>
      </div>

      <v-divider class="my-4" />

      <div class="stats">
        <div class="stat"><div class="num">{{ user.stats.games }}</div><div class="label">Games</div></div>
        <div class="stat"><div class="num">{{ user.stats.reviews }}</div><div class="label">Reviews</div></div>
        <div class="stat"><div class="num">{{ user.stats.followers }}</div><div class="label">Followers</div></div>
        <div class="stat"><div class="num">{{ user.stats.following }}</div><div class="label">Following</div></div>
      </div>
    </v-card>
  </div>
</template>

<style scoped>
.panel {
  background: var(--card);
  border: 1px solid var(--border);
  padding: 16px;
}

.muted {
  color: var(--muted-foreground);
}

.stats {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
}

.stat {
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.06);
  border-radius: var(--radius);
  padding: 12px;
  text-align: center;
}

.num {
  font-weight: 800;
  font-size: 1.2rem;
}

.label {
  color: var(--muted-foreground);
  font-size: 0.9rem;
}
</style>