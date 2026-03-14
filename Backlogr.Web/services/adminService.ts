import { useApi } from '~/services/api'
import type {
  AdminCreateUserRequestDto,
  AdminUpdateUserRoleRequestDto,
  AdminUserSummaryDto,
} from '~/types/admin'

export async function getAdminUsers(): Promise<AdminUserSummaryDto[]> {
  const api = useApi()
  const response = await api.get<AdminUserSummaryDto[]>('/api/admin/users')
  return response.data
}

export async function createAdminUser(payload: AdminCreateUserRequestDto): Promise<AdminUserSummaryDto> {
  const api = useApi()
  const response = await api.post<AdminUserSummaryDto>('/api/admin/users', payload)
  return response.data
}

export async function updateAdminUserRole(
  userId: string,
  payload: AdminUpdateUserRoleRequestDto,
): Promise<AdminUserSummaryDto> {
  const api = useApi()
  const response = await api.put<AdminUserSummaryDto>(`/api/admin/users/${userId}/role`, payload)
  return response.data
}
