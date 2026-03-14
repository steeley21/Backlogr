// /utils/apiError.ts
import { AxiosError } from 'axios'

interface ValidationProblemDetails {
  title?: string
  errors?: Record<string, string[]>
}

function flattenValidationErrors(errors: Record<string, string[]>): string[] {
  return Object.values(errors)
    .flatMap(messages => messages)
    .filter(message => typeof message === 'string' && message.trim().length > 0)
}

export function getApiErrorMessage(error: unknown, fallbackMessage: string): string {
  if (!(error instanceof AxiosError)) {
    return fallbackMessage
  }

  const apiMessage = error.response?.data

  if (typeof apiMessage === 'string' && apiMessage.trim().length > 0) {
    return apiMessage
  }

  if (Array.isArray(apiMessage) && apiMessage.length > 0) {
    return apiMessage.join(', ')
  }

  if (apiMessage && typeof apiMessage === 'object') {
    const problemDetails = apiMessage as ValidationProblemDetails

    if (problemDetails.errors) {
      const flattened = flattenValidationErrors(problemDetails.errors)

      if (flattened.length > 0) {
        return flattened.join(', ')
      }
    }

    if (typeof problemDetails.title === 'string' && problemDetails.title.trim().length > 0) {
      return problemDetails.title
    }
  }

  return fallbackMessage
}
