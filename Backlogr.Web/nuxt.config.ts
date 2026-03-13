export default defineNuxtConfig({
  ssr: false,
  modules: [
    'vuetify-nuxt-module',
    '@pinia/nuxt',
    '@nuxt/eslint',
    '@nuxt/test-utils/module'
  ],
  css: [
    '@mdi/font/css/materialdesignicons.css',
     '~/assets/styles/tokens.css',
     '~/assets/styles/base.css',
  ],
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5042'
    }
  },
  compatibilityDate: '2026-03-12',
  plugins: [
    '~/plugins/vuetify.client.ts',
  ],
  vuetify: {
    vuetifyOptions: {
      theme: {
        defaultTheme: 'backlogrDark',
        themes: {
          backlogrDark: {
            dark: true,
            colors: {
              primary: '#a855f7',
              background: '#14181c',
              surface: '#1c2228',
            },
          },
        },
      },
    },
  },
  build: { transpile: ['vuetify'] },
})