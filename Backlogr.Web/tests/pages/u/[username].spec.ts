import { flushPromises } from '@vue/test-utils'
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { uiStubs } from '~/tests/helpers/uiStubs'
import type { PublicProfileLibraryItemResponseDto, PublicProfileResponseDto } from '~/types/profile'

const useRouteMock = vi.hoisted(() => vi.fn())

const profileServiceMocks = vi.hoisted(() => ({
  getPublicProfile: vi.fn<(...args: unknown[]) => Promise<PublicProfileResponseDto>>(),
  getPublicLibrary: vi.fn<(...args: unknown[]) => Promise<PublicProfileLibraryItemResponseDto[]>>(),
}))

const followServiceMocks = vi.hoisted(() => ({
  followUser: vi.fn<(...args: unknown[]) => Promise<void>>(),
  unfollowUser: vi.fn<(...args: unknown[]) => Promise<void>>(),
}))

mockNuxtImport('useRoute', () => useRouteMock)

vi.mock('~/services/profileService', () => ({
  getPublicProfile: profileServiceMocks.getPublicProfile,
  getPublicLibrary: profileServiceMocks.getPublicLibrary,
}))

vi.mock('~/services/followService', () => ({
  followUser: followServiceMocks.followUser,
  unfollowUser: followServiceMocks.unfollowUser,
}))

function findButtonByText(wrapper: Awaited<ReturnType<typeof mountPage>>, text: string) {
  return wrapper.findAll('button').find(button => button.text().includes(text))
}

async function mountPage(username = 'kate') {
  useRouteMock.mockReturnValue({
    params: {
      username,
    },
  })

  const PublicProfilePage = (await import('~/pages/u/[username].vue')).default

  return mountSuspended(PublicProfilePage, {
    global: {
      stubs: {
        ...uiStubs,
        SectionHeader: true,
        PublicProfileLibraryCard: true,
        PublicProfileReviewCard: true,
      },
    },
  })
}

describe('public profile page', () => {
  beforeEach(() => {
    profileServiceMocks.getPublicProfile.mockReset()
    profileServiceMocks.getPublicLibrary.mockReset()
    followServiceMocks.followUser.mockReset()
    followServiceMocks.unfollowUser.mockReset()
    useRouteMock.mockReset()
  })

  it('loads the profile and library for the username in the route', async () => {
    profileServiceMocks.getPublicProfile.mockResolvedValue({
      userId: 'user-1',
      userName: 'kate',
      displayName: 'Kate Steele',
      bio: 'Retro RPG fan',
      avatarUrl: null,
      followerCount: 10,
      followingCount: 4,
      reviewCount: 3,
      libraryCount: 7,
      isFollowing: false,
      isSelf: false,
      recentReviews: [],
    })
    profileServiceMocks.getPublicLibrary.mockResolvedValue([])

    const wrapper = await mountPage('kate')
    await flushPromises()

    expect(profileServiceMocks.getPublicProfile).toHaveBeenCalledWith('kate')
    expect(profileServiceMocks.getPublicLibrary).toHaveBeenCalledWith('kate')
    expect(wrapper.text()).toContain('Kate Steele')
    expect(wrapper.text()).toContain('@kate')
    expect(wrapper.text()).toContain('Retro RPG fan')
  })

  it('follows another user and updates the visible button state', async () => {
    profileServiceMocks.getPublicProfile.mockResolvedValue({
      userId: 'user-1',
      userName: 'kate',
      displayName: 'Kate Steele',
      bio: null,
      avatarUrl: null,
      followerCount: 10,
      followingCount: 4,
      reviewCount: 3,
      libraryCount: 7,
      isFollowing: false,
      isSelf: false,
      recentReviews: [],
    })
    profileServiceMocks.getPublicLibrary.mockResolvedValue([])
    followServiceMocks.followUser.mockResolvedValue()

    const wrapper = await mountPage('kate')
    await flushPromises()

    const followButton = findButtonByText(wrapper, 'Follow')
    expect(followButton).toBeTruthy()

    await followButton!.trigger('click')
    await flushPromises()

    expect(followServiceMocks.followUser).toHaveBeenCalledWith('user-1')
    expect(findButtonByText(wrapper, 'Following')).toBeTruthy()
    expect(wrapper.text()).toContain('11')
  })
})
