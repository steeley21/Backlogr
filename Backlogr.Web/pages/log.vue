<script setup lang="ts">
import SectionHeader from '~/components/layout/SectionHeader.vue'

const gameTitle = ref('')
const status = ref<'Backlog' | 'Playing' | 'Played'>('Played')
const rating = ref(4.5)
const review = ref('')
</script>

<template>
  <div>
    <SectionHeader icon="mdi-plus" title="Log a Game" right-text="Create a log or review" />

    <v-card class="panel" rounded="xl" flat>
      <v-text-field v-model="gameTitle" label="Game" variant="solo-filled" rounded="xl" hide-details />
      <div class="mt-3 d-flex flex-wrap ga-3">
        <v-select
          v-model="status"
          :items="['Backlog','Playing','Played']"
          label="Status"
          variant="solo-filled"
          rounded="xl"
          hide-details
          style="max-width: 220px"
        />
        <v-text-field
          v-model="rating"
          label="Rating (0–5)"
          type="number"
          step="0.5"
          variant="solo-filled"
          rounded="xl"
          hide-details
          style="max-width: 180px"
        />
      </div>

      <v-textarea v-model="review" class="mt-3" label="Review (optional)" auto-grow rows="5" variant="solo-filled" rounded="xl" hide-details />

      <div class="mt-4 d-flex ga-3">
        <v-btn color="primary" rounded="pill" class="text-none px-6">Save</v-btn>
        <v-btn variant="text" rounded="pill" class="text-none">Cancel</v-btn>
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
</style>