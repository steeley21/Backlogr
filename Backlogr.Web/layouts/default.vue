<!-- /layouts/default.vue -->
<template>
  <div class="app-bg">
    <AppTopBar />

    <v-main>
      <v-container class="py-6 app-container">
        <slot />
      </v-container>
    </v-main>
  </div>
</template>

<style scoped>
.app-bg::before {
  content: "";
  position: fixed;
  inset: -200px;
  background:
    radial-gradient(900px 450px at 20% 0%, rgba(168, 85, 247, 0.14), transparent 60%),
    radial-gradient(700px 380px at 85% 12%, rgba(168, 85, 247, 0.08), transparent 55%);
  pointer-events: none;
  z-index: 0;
}

.app-bg > * {
  position: relative;
  z-index: 1;
}

.app-container {
  max-width: 1100px;
}
</style>