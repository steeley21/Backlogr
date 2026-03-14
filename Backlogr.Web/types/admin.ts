// /types/admin.ts

export type AdminAssignableRole = 'User' | 'Admin'

export interface AdminUserSummaryDto {
  userId: string
  userName: string
  displayName: string
  email: string
  roles: string[]
  createdAtUtc: string | null
}

export interface AdminCreateUserRequestDto {
  userName: string
  displayName: string
  email: string
  password: string
  role: AdminAssignableRole
}
