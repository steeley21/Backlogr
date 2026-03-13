<!-- /components/shared/BackLogrLogo.vue -->
<template>
  <NuxtLink to="/" class="logo d-flex align-center text-decoration-none">
    <v-icon icon="mdi-gamepad-variant-outline" size="26" class="mr-2 logo-icon" />
    <span class="text-h6 font-weight-bold tracking">BACKLOGR</span>
  </NuxtLink>
</template>

<style scoped>
.logo {
  color: var(--primary);
}

.logo-icon {
  color: var(--primary);
}

.tracking {
  letter-spacing: 0.18em;
}
</style>