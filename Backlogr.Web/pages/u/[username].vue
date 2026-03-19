<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from '#imports'
import SectionHeader from '~/components/layout/SectionHeader.vue'
import PublicProfileLibraryCard from '~/components/profile/PublicProfileLibraryCard.vue'
import PublicProfileReviewCard from '~/components/profile/PublicProfileReviewCard.vue'
import { followUser, unfollowUser } from '~/services/followService'
import { getPublicLibrary, getPublicProfile } from '~/services/profileService'
import type {
  PublicProfileLibraryItemResponseDto,
  PublicProfileResponseDto,
} from '~/types/profile'
import { getApiErrorMessage } from '~/utils/apiError'

const route = useRoute()

const profile = ref<PublicProfileResponseDto | null>(null)
const libraryItems = ref<PublicProfileLibraryItemResponseDto[]>([])
const isLoadingProfile = ref(false)
const isLoadingLibrary = ref(false)
const isSubmittingFollow = ref(false)
const errorMessage = ref('')
const activeTab = ref('overview')

const profileUserName = computed(() => {
  const value = route.params.username
  return typeof value === 'string' ? value.trim() : ''
})

const profileHandle = computed(() => {
  return profile.value?.userName ? `@${profile.value.userName}` : ''
})

const avatarInitials = computed(() => {
  const source = profile.value?.displayName || profile.value?.userName || 'B'
  const parts = source
    .trim()
    .split(/\s+/)
    .filter(Boolean)

  if (parts.length === 0) {
    return 'B'
  }

  if (parts.length === 1) {
    return parts[0].slice(0, 2).toUpperCase()
  }

  return `${parts[0][0] ?? ''}${parts[1][0] ?? ''}`.toUpperCase()
})

const overviewReviews = computed(() => {
  return profile.value?.recentReviews.slice(0, 3) ?? []
})

const allRecentReviews = computed(() => {
  return profile.value?.recentReviews ?? []
})

const followButtonText = computed(() => {
  if (!profile.value || profile.value.isSelf) {
    return ''
  }

  return profile.value.isFollowing ? 'Following' : 'Follow'
})

async function loadProfile(): Promise<void> {
  if (!profileUserName.value) {
    profile.value = null
    errorMessage.value = 'That profile could not be loaded.'
    return
  }

  isLoadingProfile.value = true

  try {
    profile.value = await getPublicProfile(profileUserName.value)
  }
  catch (error: unknown) {
    profile.value = null
    errorMessage.value = getApiErrorMessage(error, 'Unable to load this profile right now. Please try again.')
  }
  finally {
    isLoadingProfile.value = false
  }
}

async function loadLibrary(): Promise<void> {
  if (!profileUserName.value) {
    libraryItems.value = []
    return
  }

  isLoadingLibrary.value = true

  try {
    libraryItems.value = await getPublicLibrary(profileUserName.value)
  }
  catch (error: unknown) {
    libraryItems.value = []
    errorMessage.value = getApiErrorMessage(error, 'Unable to load this library right now. Please try again.')
  }
  finally {
    isLoadingLibrary.value = false
  }
}

async function loadPage(): Promise<void> {
  errorMessage.value = ''
  await Promise.all([
    loadProfile(),
    loadLibrary(),
  ])
}

async function toggleFollow(): Promise<void> {
  if (!profile.value || profile.value.isSelf || isSubmittingFollow.value) {
    return
  }

  const wasFollowing = profile.value.isFollowing
  const previousFollowerCount = profile.value.followerCount

  profile.value.isFollowing = !wasFollowing
  profile.value.followerCount = Math.max(0, previousFollowerCount + (wasFollowing ? -1 : 1))
  isSubmittingFollow.value = true
  errorMessage.value = ''

  try {
    if (wasFollowing) {
      await unfollowUser(profile.value.userId)
    }
    else {
      await followUser(profile.value.userId)
    }
  }
  catch (error: unknown) {
    profile.value.isFollowing = wasFollowing
    profile.value.followerCount = previousFollowerCount
    errorMessage.value = getApiErrorMessage(error, 'Unable to update follow status right now. Please try again.')
  }
  finally {
    isSubmittingFollow.value = false
  }
}

watch(
  () => profileUserName.value,
  async () => {
    activeTab.value = 'overview'
    await loadPage()
  },
  { immediate: true },
)
</script>

<template>
  <div>
    <SectionHeader
      icon="mdi-account-circle-outline"
      :title="profile?.displayName || 'Profile'"
      :right-text="profileHandle || 'Backlogr profile'"
    />

    <v-alert
      v-if="errorMessage"
      type="error"
      variant="tonal"
      rounded="lg"
      class="mb-4"
    >
      {{ errorMessage }}
    </v-alert>

    <v-card class="hero-card" rounded="xl" flat>
      <template v-if="isLoadingProfile && !profile">
        <div class="hero-skeleton">
          <v-skeleton-loader type="avatar, heading, paragraph, paragraph" />
        </div>
      </template>

      <template v-else-if="profile">
        <div class="hero-layout">
          <div class="identity-block">
            <v-avatar size="92" class="avatar">
              <v-img
                v-if="profile.avatarUrl"
                :src="profile.avatarUrl"
                :alt="profile.displayName"
                cover
              />
              <span v-else class="avatar-fallback">{{ avatarInitials }}</span>
            </v-avatar>

            <div class="identity-copy">
              <div class="identity-row">
                <div>
                  <div class="text-h4 font-weight-bold">{{ profile.displayName }}</div>
                  <div class="muted mt-1">{{ profileHandle }}</div>
                </div>

                <div class="identity-actions">
                  <v-chip
                    v-if="profile.isSelf"
                    color="primary"
                    variant="tonal"
                    size="small"
                  >
                    This is you
                  </v-chip>

                  <v-btn
                    v-else
                    :color="profile.isFollowing ? undefined : 'primary'"
                    :variant="profile.isFollowing ? 'outlined' : 'flat'"
                    rounded="pill"
                    class="text-none px-6"
                    :loading="isSubmittingFollow"
                    @click="toggleFollow"
                  >
                    {{ followButtonText }}
                  </v-btn>
                </div>
              </div>

              <p class="bio muted mt-4">
                {{ profile.bio || 'No bio added yet.' }}
              </p>
            </div>
          </div>

          <div class="stats-grid">
            <div class="stat-card">
              <div class="stat-number">{{ profile.followerCount }}</div>
              <div class="stat-label">Followers</div>
            </div>
            <div class="stat-card">
              <div class="stat-number">{{ profile.followingCount }}</div>
              <div class="stat-label">Following</div>
            </div>
            <div class="stat-card">
              <div class="stat-number">{{ profile.reviewCount }}</div>
              <div class="stat-label">Reviews</div>
            </div>
            <div class="stat-card">
              <div class="stat-number">{{ profile.libraryCount }}</div>
              <div class="stat-label">Library</div>
            </div>
          </div>
        </div>
      </template>

      <template v-else>
        <div class="empty-state">
          <div class="text-h6 font-weight-bold mb-2">Profile unavailable</div>
          <div class="muted">This user could not be loaded.</div>
        </div>
      </template>
    </v-card>

    <template v-if="profile">
      <div class="profile-tab-bar mt-5">
        <button
          v-for="tab in ['overview', 'library', 'reviews']"
          :key="tab"
          class="profile-tab"
          :class="{ active: activeTab === tab }"
          @click="activeTab = tab"
        >
          {{ tab.charAt(0).toUpperCase() + tab.slice(1) }}
        </button>
      </div>

      <v-window v-model="activeTab" class="mt-4" touch>
      <v-window-item value="overview">
        <v-row>
          <v-col cols="12" md="7">
            <v-card class="panel" rounded="xl" flat>
              <div class="section-title">Recent reviews</div>

              <div v-if="overviewReviews.length === 0" class="muted empty-copy">
                No reviews yet.
              </div>

              <div v-else class="d-flex flex-column ga-4 mt-4">
                <PublicProfileReviewCard
                  v-for="review in overviewReviews"
                  :key="review.reviewId"
                  :review="review"
                />
              </div>
            </v-card>
          </v-col>

          <v-col cols="12" md="5">
            <v-card class="panel" rounded="xl" flat>
              <div class="section-title">At a glance</div>

              <div class="summary-list mt-4">
                <div class="summary-row">
                  <span class="muted">Handle</span>
                  <span>{{ profileHandle || 'Unknown' }}</span>
                </div>
                <div class="summary-row">
                  <span class="muted">Library entries</span>
                  <span>{{ profile?.libraryCount ?? 0 }}</span>
                </div>
                <div class="summary-row">
                  <span class="muted">Reviews written</span>
                  <span>{{ profile?.reviewCount ?? 0 }}</span>
                </div>
                <div class="summary-row">
                  <span class="muted">Following status</span>
                  <span>
                    {{ profile?.isSelf ? 'This is your account' : profile?.isFollowing ? 'Following' : 'Not following' }}
                  </span>
                </div>
              </div>
            </v-card>
          </v-col>
        </v-row>
      </v-window-item>

      <v-window-item value="library">
        <v-card class="panel" rounded="xl" flat>
          <div class="section-title">Public library</div>

          <div v-if="isLoadingLibrary" class="library-grid mt-4">
            <v-skeleton-loader
              v-for="n in 6"
              :key="n"
              type="image, article"
              class="library-skeleton"
            />
          </div>

          <div v-else-if="libraryItems.length === 0" class="muted empty-copy mt-4">
            No public library entries yet.
          </div>

          <v-row v-else dense class="mt-2">
            <v-col
              v-for="item in libraryItems"
              :key="item.gameLogId"
              cols="6"
              sm="4"
              lg="3"
            >
              <PublicProfileLibraryCard :item="item" />
            </v-col>
          </v-row>
        </v-card>
      </v-window-item>

      <v-window-item value="reviews">
        <v-card class="panel" rounded="xl" flat>
          <div class="section-title">Recent reviews</div>
          <div class="section-subtitle muted">
            This is the current recent-review slice returned by the API.
          </div>

          <div v-if="allRecentReviews.length === 0" class="muted empty-copy mt-4">
            No reviews yet.
          </div>

          <div v-else class="d-flex flex-column ga-4 mt-4">
            <PublicProfileReviewCard
              v-for="review in allRecentReviews"
              :key="review.reviewId"
              :review="review"
            />
          </div>
        </v-card>
      </v-window-item>
      </v-window>
    </template>
  </div>
</template>

<style scoped>
.hero-card,
.panel {
  background: var(--card);
  border: 1px solid var(--border);
}

.hero-card {
  padding: 24px;
}

.hero-layout {
  display: grid;
  gap: 22px;
}

.identity-block {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 18px;
  align-items: start;
}

.avatar {
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.06);
}

.avatar-fallback {
  font-size: 1.3rem;
  font-weight: 800;
  color: var(--foreground);
}

.identity-copy {
  min-width: 0;
}

.identity-row {
  display: flex;
  gap: 16px;
  justify-content: space-between;
  align-items: flex-start;
}

.identity-actions {
  display: flex;
  justify-content: flex-end;
  flex: 0 0 auto;
}

.bio {
  line-height: 1.7;
  white-space: pre-wrap;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
}

.stat-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 16px;
  padding: 16px;
}

.stat-number {
  font-size: 1.5rem;
  font-weight: 800;
}

.stat-label,
.muted {
  color: var(--muted-foreground);
}

.stat-label {
  margin-top: 4px;
}

.profile-tab-bar {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid var(--border);
  border-radius: 9999px;
  padding: 4px;
}

.profile-tab {
  border: none;
  background: transparent;
  color: var(--muted-foreground);
  font-size: 0.875rem;
  font-weight: 500;
  padding: 6px 18px;
  border-radius: 9999px;
  cursor: pointer;
  transition: background 160ms ease, color 160ms ease;
  white-space: nowrap;
}

.profile-tab:hover {
  color: var(--foreground);
  background: rgba(255, 255, 255, 0.06);
}

.profile-tab.active {
  background: rgba(255, 255, 255, 0.10);
  color: var(--foreground);
  font-weight: 600;
}

.panel {
  padding: 20px;
}

.section-title {
  font-size: 1.1rem;
  font-weight: 700;
}

.section-subtitle {
  margin-top: 6px;
}

.empty-copy,
.hero-skeleton,
.empty-state {
  padding: 8px 0;
}

.summary-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.summary-row:last-child {
  border-bottom: 0;
  padding-bottom: 0;
}

.library-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

@media (max-width: 960px) {
  .stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 700px) {
  .hero-card {
    padding: 16px;
  }

  .identity-block {
    grid-template-columns: 1fr;
  }

  .avatar {
    width: 64px !important;
    height: 64px !important;
  }

  .identity-row {
    flex-direction: column;
    gap: 10px;
  }

  .identity-actions {
    justify-content: flex-start;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 8px;
  }

  .stat-card {
    padding: 12px;
    border-radius: 12px;
  }

  .stat-number {
    font-size: 1.25rem;
  }

  .panel {
    padding: 14px;
  }

  .profile-tab-bar {
    width: 100%;
    justify-content: stretch;
  }

  .profile-tab {
    flex: 1;
    text-align: center;
    padding: 6px 10px;
    font-size: 0.82rem;
  }

  .summary-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }
}
</style>
