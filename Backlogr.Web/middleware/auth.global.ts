// /middleware/auth.global.ts
import { navigateTo } from '#imports'
import { useAuthStore } from '~/stores/auth'

const PUBLIC_ROUTES = new Set(['/', '/login', '/register'])
const DEFAULT_AUTHENTICATED_ROUTE = '/feed'

export default defineNuxtRouteMiddleware(async (to) => {
  const authStore = useAuthStore()

  if (!authStore.isInitialized) {
    await authStore.hydrate()
  }

  const isPublicRoute = PUBLIC_ROUTES.has(to.path)
  const isAuthPage = to.path === '/login' || to.path === '/register'

  if (isAuthPage && authStore.isAuthenticated) {
    const redirectTarget = typeof to.query.redirect === 'string' && to.query.redirect.startsWith('/')
      ? to.query.redirect
      : DEFAULT_AUTHENTICATED_ROUTE

    return navigateTo(redirectTarget)
  }

  if (!isPublicRoute && !authStore.isAuthenticated) {
    return navigateTo({
      path: '/login',
      query: {
        redirect: to.fullPath,
      },
    })
  }
})
