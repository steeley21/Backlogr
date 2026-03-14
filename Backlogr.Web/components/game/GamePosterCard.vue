<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  title: string
  coverUrl: string
  subtitle?: string
  to?: string
  loading?: boolean
  disabled?: boolean
  clickable?: boolean
}>(), {
  subtitle: undefined,
  to: undefined,
  loading: false,
  disabled: false,
  clickable: false,
})

const emit = defineEmits<{
  click: []
}>()

const isClickable = computed(() => {
  return Boolean(props.to) || props.clickable
})

function handleClick(): void {
  if (props.disabled || props.loading) {
    return
  }

  if (props.to) {
    return
  }

  if (!props.clickable) {
    return
  }

  emit('click')
}
</script>

<template>
  <v-card
    class="card"
    :class="{
      'card--clickable': isClickable,
      'card--disabled': disabled,
    }"
    rounded="xl"
    flat
    :to="to && !disabled && !loading ? to : undefined"
    :link="Boolean(to) && !disabled && !loading"
    :ripple="isClickable && !disabled && !loading"
    :tabindex="!to && clickable ? 0 : undefined"
    :role="!to && clickable ? 'button' : undefined"
    :aria-busy="loading ? 'true' : 'false'"
    @click="handleClick"
    @keydown.enter.prevent="handleClick"
    @keydown.space.prevent="handleClick"
  >
    <div class="cover">
      <v-img :src="coverUrl" cover />
      <div v-if="loading" class="loading-overlay">
        <v-progress-circular indeterminate size="34" width="4" />
      </div>
    </div>

    <div class="meta">
      <div class="title">{{ title }}</div>
      <div v-if="subtitle" class="subtitle">{{ subtitle }}</div>
    </div>
  </v-card>
</template>

<style scoped>
.card {
  background: var(--card);
  border: 1px solid rgba(255,255,255,0.06);
  border-radius: var(--radius) !important;
  overflow: hidden;
  transition: transform 120ms ease, box-shadow 120ms ease, border-color 120ms ease, opacity 120ms ease;
}

.card--clickable {
  cursor: pointer;
}

.card:hover {
  transform: translateY(-1px);
  border-color: rgba(168, 85, 247, 0.18);
  box-shadow: 0 14px 28px rgba(0,0,0,0.22);
}

.card--disabled {
  opacity: 0.72;
}

.cover {
  position: relative;
  aspect-ratio: 2 / 3;
  margin: 10px;
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(255,255,255,0.06);
}

.loading-overlay {
  position: absolute;
  inset: 0;
  display: grid;
  place-items: center;
  background: rgba(10, 12, 16, 0.55);
}

.meta {
  padding: 10px 12px 14px;
}

.title {
  color: var(--foreground);
  font-weight: 700;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.subtitle {
  margin-top: 6px;
  color: var(--muted-foreground);
  font-size: 0.92rem;
}
</style>