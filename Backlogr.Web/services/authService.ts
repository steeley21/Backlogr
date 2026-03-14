// /services/authService.ts
import { useApi } from '~/services/api'
import type {
  AuthResponseDto,
  DeleteAccountRequestDto,
  LoginRequestDto,
  MeResponseDto,
  RegisterRequestDto,
} from '~/types/auth'

export async function login(payload: LoginRequestDto): Promise<AuthResponseDto> {
  const api = useApi()
  const response = await api.post<AuthResponseDto>('/api/Auth/login', payload)
  return response.data
}

export async function register(payload: RegisterRequestDto): Promise<AuthResponseDto> {
  const api = useApi()
  const response = await api.post<AuthResponseDto>('/api/Auth/register', payload)
  return response.data
}

export async function getMe(): Promise<MeResponseDto> {
  const api = useApi()
  const response = await api.get<MeResponseDto>('/api/Auth/me')
  return response.data
}
export async function deleteAccount(payload: DeleteAccountRequestDto): Promise<void> {
  const api = useApi()
  await api.post('/api/Auth/delete-account', payload)
}
