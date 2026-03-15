// /services/profileService.ts
import { useApi } from '~/services/api'
import type {
  PublicProfileLibraryItemResponseDto,
  PublicProfileResponseDto,
} from '~/types/profile'

export async function getPublicProfile(userName: string): Promise<PublicProfileResponseDto> {
  const api = useApi()
  const response = await api.get<PublicProfileResponseDto>(`/api/Profiles/${encodeURIComponent(userName)}`)
  return response.data
}

export async function getPublicLibrary(
  userName: string,
): Promise<PublicProfileLibraryItemResponseDto[]> {
  const api = useApi()
  const response = await api.get<PublicProfileLibraryItemResponseDto[]>(
    `/api/Profiles/${encodeURIComponent(userName)}/library`,
  )
  return response.data
}
