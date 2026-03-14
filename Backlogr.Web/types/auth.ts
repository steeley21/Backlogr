// /types/auth.ts

export interface LoginRequestDto {
  emailOrUserName: string
  password: string
}

export interface RegisterRequestDto {
  userName: string
  displayName: string
  email: string
  password: string
}

export interface AuthResponseDto {
  token: string
  expiresAtUtc: string
  userId: string
  userName: string
  displayName: string
  email: string
  roles: string[]
}

export interface MeResponseDto {
  userId: string
  userName: string
  displayName: string
  email: string
  avatarUrl: string | null
  bio: string | null
  roles: string[]
}
export interface DeleteAccountRequestDto {
  password: string
  confirmationUserName: string
}
