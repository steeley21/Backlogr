<script setup lang="ts">
const props = defineProps<{
  icon?: string
  title: string
  rightText?: string
}>()
</script>

<template>
  <div class="wrap">
    <div class="left">
      <v-icon v-if="icon" :icon="icon" color="primary" size="20" />
      <div class="title">{{ title }}</div>
    </div>

    <div v-if="rightText" class="right muted">
      {{ rightText }}
    </div>

    <slot name="right" />
  </div>
</template>

<style scoped>
.wrap {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
}

.left {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.title {
  font-size: 1.15rem;
  font-weight: 750;
}

.muted {
  color: var(--muted-foreground);
  font-size: 0.95rem;
}
</style>