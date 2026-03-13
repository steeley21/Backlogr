// /middleware/auth.global.ts
import { useAuthStore } from '~/stores/auth'

export default defineNuxtRouteMiddleware(async (to) => {
  const authStore = useAuthStore()

  if (!authStore.isInitialized) {
    await authStore.hydrate()
  }

  const isAuthPage = to.path === '/login' || to.path === '/register'

  if (isAuthPage && authStore.isAuthenticated) {
    const redirectTarget = typeof to.query.redirect === 'string' && to.query.redirect.startsWith('/')
      ? to.query.redirect
      : '/'

    return navigateTo(redirectTarget)
  }

  if (!isAuthPage && !authStore.isAuthenticated) {
    return navigateTo({
      path: '/login',
      query: {
        redirect: to.fullPath,
      },
    })
  }
})