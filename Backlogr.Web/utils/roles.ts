// /utils/roles.ts

export const USER_ROLE = 'User'
export const ADMIN_ROLE = 'Admin'
export const SUPER_ADMIN_ROLE = 'SuperAdmin'

export function isAdminLike(roles: string[]): boolean {
  return roles.includes(ADMIN_ROLE) || roles.includes(SUPER_ADMIN_ROLE)
}

export function isSuperAdmin(roles: string[]): boolean {
  return roles.includes(SUPER_ADMIN_ROLE)
}
