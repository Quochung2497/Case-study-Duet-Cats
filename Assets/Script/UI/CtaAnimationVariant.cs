using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    [DisallowMultipleComponent]
    public class CtaAnimationVariant : MonoBehaviour
    {
        [Inject] private CtaAnimationSet _animations;
        [Inject] private IViewport _viewport;

        private Animator _animator;
        private AnimatorOverrideController _overrideController;
        private bool? _appliedPortrait;

        private void Awake()
        {
            _animator = gameObject.GetOrAdd<Animator>();
            var controller = _animator.runtimeAnimatorController;
            if (controller == null || _animations == null ||
                _animations.PortraitClip == null || _animations.LandscapeClip == null)
            {
                Debug.LogError("CTA animation variant needs a controller and both clips.", this);
                enabled = false;
                return;
            }

            var hasPortraitClip = false;
            foreach (var clip in controller.animationClips)
            {
                if (clip != _animations.PortraitClip) continue;
                hasPortraitClip = true;
                break;
            }

            if (!hasPortraitClip)
            {
                Debug.LogError("The CTA Animator controller does not use the configured portrait clip.", this);
                enabled = false;
                return;
            }

            _overrideController = new AnimatorOverrideController(controller);
            _animator.runtimeAnimatorController = _overrideController;
        }

        private void OnEnable()
        {
            if (_viewport == null) return;
            _viewport.Changed += ApplyVariant;
            if (_viewport.HasViewport) ApplyVariant(_viewport.IsPortrait);
        }

        private void OnDisable()
        {
            if (_viewport != null) _viewport.Changed -= ApplyVariant;
        }

        private void ApplyVariant(bool portrait)
        {
            if (_overrideController == null) return;
            if (_appliedPortrait == portrait) return;

            _overrideController[_animations.PortraitClip] = portrait
                ? _animations.PortraitClip
                : _animations.LandscapeClip;
            _appliedPortrait = portrait;
        }

        private void OnDestroy()
        {
            if (_overrideController != null) Destroy(_overrideController);
        }
    }
}
