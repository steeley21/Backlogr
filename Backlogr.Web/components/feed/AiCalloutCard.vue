<!-- /components/feed/AiCalloutCard.vue -->
<template>
  <v-card class="ai" rounded="xl" flat>
    <div class="d-flex align-center mb-2">
      <v-icon icon="mdi-sparkles" color="primary" class="mr-2" />
      <div class="text-h6 font-weight-semibold">What should you play next?</div>
    </div>

    <div class="muted mb-4">
      Get AI-powered recommendations based on your gaming taste.
    </div>

    <v-btn to="/recommend" color="primary" rounded="pill" class="text-none px-5">
      Get Recommendations
    </v-btn>
  </v-card>
</template>

<style scoped>
.ai {
  background:
    radial-gradient(600px 220px at 20% 0%, rgba(168, 85, 247, 0.18), transparent 60%),
    var(--card);
  border: 1px solid rgba(168, 85, 247, 0.28);
  border-radius: var(--radius) !important;
  padding: 18px;
  box-shadow: 0 10px 24px rgba(0, 0, 0, 0.25);
}

.muted {
  color: var(--muted-foreground);
}
</style>