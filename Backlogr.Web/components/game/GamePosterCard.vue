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
      <v-img :src="coverUrl" cover :alt="title" />
      <div class="cover-glow" />
      <div class="cover-overlay">
        <div class="cover-overlay-body">
          <span class="cover-overlay-title">{{ title }}</span>
          <span v-if="subtitle" class="cover-overlay-subtitle">{{ subtitle }}</span>
        </div>
      </div>
      <div v-if="loading" class="loading-overlay">
        <v-progress-circular indeterminate size="34" width="4" />
      </div>
    </div>
  </v-card>
</template>

<style scoped>
.card {
  background: transparent;
  border: 1px solid rgba(255,255,255,0.06);
  border-radius: var(--radius) !important;
  overflow: hidden;
  transition: transform 240ms cubic-bezier(0.25, 0.46, 0.45, 0.94),
              box-shadow 240ms cubic-bezier(0.25, 0.46, 0.45, 0.94),
              border-color 240ms ease,
              opacity 200ms ease;
}

.card--clickable {
  cursor: pointer;
}

.card--clickable:hover {
  transform: translateY(-6px) scale(1.015);
  border-color: rgba(168, 85, 247, 0.4);
  box-shadow:
    0 0 0 1px rgba(168, 85, 247, 0.15),
    0 20px 48px rgba(0, 0, 0, 0.55),
    0 6px 16px rgba(168, 85, 247, 0.12);
}

.card--clickable:hover .cover-overlay {
  opacity: 1;
}

.card--clickable:hover .cover-overlay-body {
  transform: translateY(0);
  opacity: 1;
}

.card--clickable:hover .cover-glow {
  opacity: 1;
}

.card--disabled {
  opacity: 0.45;
  pointer-events: none;
}

.cover {
  position: relative;
  aspect-ratio: 2 / 3;
  width: 100%;
  display: block;
  overflow: hidden;
  border-radius: 0;
}

/* Force v-img to fill the cover exactly */
.cover :deep(.v-img) {
  width: 100%;
  height: 100%;
  display: block;
}

.cover :deep(.v-img__img) {
  object-fit: cover;
  width: 100%;
  height: 100%;
}

.cover-glow {
  position: absolute;
  inset: 0;
  background: radial-gradient(ellipse at 50% 0%, rgba(168, 85, 247, 0.22), transparent 65%);
  opacity: 0;
  transition: opacity 300ms ease;
  pointer-events: none;
  z-index: 2;
}

.cover-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    to top,
    rgba(6, 3, 14, 0.92) 0%,
    rgba(6, 3, 14, 0.55) 35%,
    transparent 65%
  );
  opacity: 0;
  transition: opacity 260ms ease;
  z-index: 3;
  display: flex;
  align-items: flex-end;
  padding: 14px 12px;
}

.cover-overlay-body {
  display: flex;
  flex-direction: column;
  gap: 3px;
  transform: translateY(6px);
  opacity: 0;
  transition: transform 280ms cubic-bezier(0.25, 0.46, 0.45, 0.94) 30ms,
              opacity 280ms ease 30ms;
}

.cover-overlay-title {
  color: #fff;
  font-size: 0.85rem;
  font-weight: 700;
  line-height: 1.25;
  letter-spacing: 0.01em;
  text-shadow: 0 1px 8px rgba(0,0,0,0.7);
}

.cover-overlay-subtitle {
  color: rgba(255,255,255,0.65);
  font-size: 0.75rem;
  font-weight: 500;
  line-height: 1.2;
  text-shadow: 0 1px 6px rgba(0,0,0,0.6);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.loading-overlay {
  position: absolute;
  inset: 0;
  display: grid;
  place-items: center;
  background: rgba(10, 12, 16, 0.65);
  z-index: 5;
}

</style>