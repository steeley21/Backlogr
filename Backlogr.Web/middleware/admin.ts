// /middleware/admin.ts
import { navigateTo } from '#imports'
import { useAuthStore } from '~/stores/auth'
import { isAdminLike } from '~/utils/roles'

export default defineNuxtRouteMiddleware(async (to) => {
  const authStore = useAuthStore()

  if (!authStore.isInitialized) {
    await authStore.hydrate()
  }

  if (!authStore.isAuthenticated) {
    return navigateTo({
      path: '/login',
      query: {
        redirect: to.fullPath,
      },
    })
  }

  if (!isAdminLike(authStore.roles)) {
    return navigateTo('/feed')
  }
})
