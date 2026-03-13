// /services/libraryService.ts
import { useApi } from '~/services/api'
import type {
  LibraryLogResponseDto,
  UpsertLibraryLogRequestDto,
} from '~/types/library'

export async function getMyLibrary(): Promise<LibraryLogResponseDto[]> {
  const api = useApi()
  const response = await api.get<LibraryLogResponseDto[]>('/api/Library/me')
  return response.data
}

export async function upsertLibraryLog(
  payload: UpsertLibraryLogRequestDto,
): Promise<LibraryLogResponseDto> {
  const api = useApi()
  const response = await api.post<LibraryLogResponseDto>('/api/Library', payload)
  return response.data
}

export async function deleteLibraryLog(gameId: string): Promise<void> {
  const api = useApi()
  await api.delete(`/api/Library/${gameId}`)
}