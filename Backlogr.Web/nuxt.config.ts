export default defineNuxtConfig({
  ssr: false,
  modules: [
    'vuetify-nuxt-module',
    '@pinia/nuxt',
    '@nuxt/eslint',
    '@nuxt/test-utils/module'
  ],
  css: [
    '@mdi/font/css/materialdesignicons.css'
  ],
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5000'
    }
  }
})