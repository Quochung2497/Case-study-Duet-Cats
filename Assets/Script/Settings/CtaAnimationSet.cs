using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "CtaAnimationSet", menuName = "Playable/CTA Animation Set")]
    public class CtaAnimationSet : ScriptableObject
    {
        [SerializeField] private AnimationClip portraitClip;
        [SerializeField] private AnimationClip landscapeClip;

        public AnimationClip PortraitClip => portraitClip;
        public AnimationClip LandscapeClip => landscapeClip;
    }
}
