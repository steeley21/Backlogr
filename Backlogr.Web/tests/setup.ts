import { afterEach, beforeEach, vi } from 'vitest'

;(globalThis as { definePageMeta?: (meta: unknown) => void }).definePageMeta = () => {}
;(process as { client?: boolean }).client = true

beforeEach(() => {
  localStorage.clear()
})

afterEach(() => {
  vi.clearAllMocks()
})
