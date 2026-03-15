// /services/followService.ts
import { useApi } from '~/services/api'

export async function followUser(userId: string): Promise<void> {
  const api = useApi()
  await api.post(`/api/Follows/${userId}`)
}

export async function unfollowUser(userId: string): Promise<void> {
  const api = useApi()
  await api.delete(`/api/Follows/${userId}`)
}
