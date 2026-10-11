using System;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "PlayableSettings", menuName = "Duet Cats/Playable Settings")]
    public class PlayableSettings : ScriptableObject
    {
        // One Inspector asset supplies configuration to all playable systems.
        #region Configuration types

        [Serializable]
        public struct LayoutProfile
        {
            [Tooltip("World-unit margin on each side of the camera.")]
            [Min(0f)] public float sideMargin;
            [Tooltip("Upper limit for the centered gameplay area's width in world units.")]
            [Min(0.1f)] public float maxPlayWidth;
            [Tooltip("Visible gap between the two cats at their closest positions.")]
            [Min(0f)] public float centerGap;
            [Tooltip("Approximate half-width of a cat, used to keep it within its half of the play area.")]
            [Min(0f)] public float catHalfWidth;
            [Tooltip("Default cat center as a fraction of playWidth from its outer edge.")]
            [Range(0f, 0.5f)] public float catX;
            [Tooltip("Outer note lane as a fraction of playWidth from its outer edge.")]
            [Range(0f, 0.5f)] public float outerLaneX;
            [Tooltip("Inner note lane as a fraction of playWidth from its outer edge.")]
            [Range(0f, 0.5f)] public float innerLaneX;
            [Tooltip("Cat origin height in the camera viewport: 0 = bottom, 1 = top.")]
            [Range(0f, 1f)] public float catY;
            [Tooltip("Note arrival height in the camera viewport.")]
            [Range(0f, 1f)] public float hitY;
            [Tooltip("Note spawn height in viewport units. Above 1 starts just off screen.")]
            public float spawnY;
        }

        #endregion

        #region Inspector fields

        [Header("Hit VFX")]
        [SerializeField] private HitVfxSettings hitVfx = new HitVfxSettings();

        [Header("Hit text")]
        [SerializeField] private HitTextSettings hitText = new HitTextSettings();

        [Header("Responsive layout")]
        [SerializeField] private LayoutProfile portrait;
        [SerializeField] private LayoutProfile landscape;

        [Header("Notes")]
        [SerializeField] private NoteSettings notes = new NoteSettings();

        [Header("Game flow")]
        [Min(0f)] [SerializeField] private float startTransitionSeconds = 2f;
        [Min(0f)] [SerializeField] private float resultSeconds = 3f;
        [Min(0f)] [SerializeField] private float ctaReloadSeconds = 2f;

        [Header("Background")]
        [SerializeField] private Sprite portraitBackground;
        [SerializeField] private Sprite landscapeBackground;

        [Header("Score")]
        [SerializeField] private ScoreSettings score = new ScoreSettings();

        #endregion

        #region Public settings

        public LayoutProfile Portrait => portrait;
        public LayoutProfile Landscape => landscape;
        public INoteSettings Notes => notes;
        public HitVfxSettings HitVfx => hitVfx;
        public HitTextSettings HitText => hitText;
        public float StartTransitionSeconds => startTransitionSeconds;
        public float ResultSeconds => resultSeconds;
        public float CtaReloadSeconds => ctaReloadSeconds;
        public Sprite PortraitBackground => portraitBackground;
        public Sprite LandscapeBackground => landscapeBackground;
        public IScoreRules ScoreRules => score;

        #endregion
    }
}
