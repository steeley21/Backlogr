import { defineComponent, h } from 'vue'

export const NuxtLinkStub = defineComponent({
  name: 'NuxtLink',
  props: {
    to: {
      type: [String, Object],
      default: '/',
    },
  },
  setup(props, { attrs, slots }) {
    return () => h('a', { ...attrs, href: typeof props.to === 'string' ? props.to : '#' }, slots.default?.())
  },
})

export const VBtnStub = defineComponent({
  name: 'VBtn',
  inheritAttrs: false,
  props: {
    disabled: Boolean,
    loading: Boolean,
  },
  emits: ['click'],
  setup(props, { attrs, emit, slots }) {
    return () => h(
      'button',
      {
        ...attrs,
        disabled: props.disabled || props.loading,
        onClick: (event: MouseEvent) => emit('click', event),
      },
      slots.default?.(),
    )
  },
})

export const VTextareaStub = defineComponent({
  name: 'VTextarea',
  inheritAttrs: false,
  props: {
    modelValue: {
      type: String,
      default: '',
    },
  },
  emits: ['update:modelValue'],
  setup(props, { attrs, emit }) {
    return () => h('textarea', {
      ...attrs,
      value: props.modelValue,
      onInput: (event: Event) => emit('update:modelValue', (event.target as HTMLTextAreaElement).value),
    })
  },
})

export const VMenuStub = defineComponent({
  name: 'VMenu',
  setup(_, { slots }) {
    return () => h('div', [
      slots.activator?.({ props: {} }),
      slots.default?.(),
    ])
  },
})

export const VListItemStub = defineComponent({
  name: 'VListItem',
  inheritAttrs: false,
  emits: ['click'],
  setup(_, { attrs, emit, slots }) {
    return () => h('button', {
      ...attrs,
      onClick: (event: MouseEvent) => emit('click', event),
    }, slots.default?.())
  },
})

export const passthroughStub = (name: string, tag = 'div') => defineComponent({
  name,
  inheritAttrs: false,
  setup(_, { attrs, slots }) {
    return () => h(tag, { ...attrs, 'data-stub': name }, slots.default?.())
  },
})

export const uiStubs = {
  NuxtLink: NuxtLinkStub,
  VAlert: passthroughStub('VAlert'),
  VAvatar: passthroughStub('VAvatar'),
  VCard: passthroughStub('VCard'),
  VChip: passthroughStub('VChip', 'span'),
  VCol: passthroughStub('VCol'),
  VExpandTransition: passthroughStub('VExpandTransition'),
  ExpandTransition: passthroughStub('ExpandTransition'),
  VIcon: passthroughStub('VIcon', 'span'),
  VImg: passthroughStub('VImg', 'img'),
  VList: passthroughStub('VList'),
  VListItem: VListItemStub,
  VMenu: VMenuStub,
  VRow: passthroughStub('VRow'),
  VSkeletonLoader: passthroughStub('VSkeletonLoader'),
  VBtn: VBtnStub,
  VTab: VBtnStub,
  VTabs: passthroughStub('VTabs'),
  VTextarea: VTextareaStub,
  VWindow: passthroughStub('VWindow'),
  VWindowItem: passthroughStub('VWindowItem'),
}
