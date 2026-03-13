<!-- /components/shared/StarRating.vue -->
<script setup lang="ts">
const props = defineProps<{
  rating: number // 0..5 in 0.5 steps
  size?: number
}>()

function stars() {
  const r = Math.max(0, Math.min(5, props.rating))
  const full = Math.floor(r)
  const half = r - full >= 0.5 ? 1 : 0
  const empty = 5 - full - half

  return {
    full,
    half,
    empty,
  }
}
</script>

<template>
  <div class="d-flex align-center">
    <v-icon
      v-for="n in stars().full"
      :key="`f-${n}`"
      icon="mdi-star"
      color="primary"
      :size="size ?? 18"
      class="mr-1"
    />
    <v-icon
      v-for="n in stars().half"
      :key="`h-${n}`"
      icon="mdi-star-half-full"
      color="primary"
      :size="size ?? 18"
      class="mr-1"
    />
    <v-icon
      v-for="n in stars().empty"
      :key="`e-${n}`"
      icon="mdi-star-outline"
      :color="'rgba(255,255,255,0.22)'"
      :size="size ?? 18"
      class="mr-1"
    />
  </div>
</template>