// plugins/vuetify.client.ts
import { createVuetify } from 'vuetify'
import 'vuetify/styles'

import { aliases, mdi } from 'vuetify/iconsets/mdi-svg'
import '@mdi/font/css/materialdesignicons.css'

export default defineNuxtPlugin((nuxtApp) => {
  const vuetify = createVuetify({
    icons: {
      defaultSet: 'mdi',
      aliases,
      sets: { mdi },
    },
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
    defaults: {
      VBtn: {
        style: 'text-transform:none;',
      },
      VCard: {
        rounded: 'xl',
      },
    },
  })

  nuxtApp.vueApp.use(vuetify)
})