using Input;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "PlayableSettings", menuName = "Duet Cats/Playable Settings")]
    public class PlayableSettings : ScriptableObject
    {
        // One Inspector asset supplies configuration to all playable systems.
        #region Inspector fields

        [Header("Shared assets")]
        [SerializeField] private SongCatalog songCatalog;
        [SerializeField] private CatAnimationSet catAnimations;
        [SerializeField] private CtaAnimationSet ctaAnimations;
        [SerializeField] private InputReader inputReader;

        [Header("Hit VFX")]
        [SerializeField] private HitVfxSettings hitVfx = new HitVfxSettings();

        [Header("Hit text")]
        [SerializeField] private HitTextSettings hitText = new HitTextSettings();

        [Header("Notes")]
        [SerializeField] private NoteSettings notes = new NoteSettings();

        [Header("Game flow")]
        [SerializeField] private GameFlowSettings gameFlow = new GameFlowSettings();

        [Header("Scene visuals")]
        [SerializeField] private SceneVisualSettings sceneVisuals = new SceneVisualSettings();

        [Header("Score")]
        [SerializeField] private ScoreSettings score = new ScoreSettings();

        #endregion

        #region Public settings

        public SongCatalog SongCatalog => songCatalog;
        public CatAnimationSet CatAnimations => catAnimations;
        public CtaAnimationSet CtaAnimations => ctaAnimations;
        public IInputReader InputReader => inputReader;

        public INoteSettings Notes => notes;
        public HitVfxSettings HitVfx => hitVfx;
        public HitTextSettings HitText => hitText;
        public IGameFlowSettings GameFlow => gameFlow;
        public SceneVisualSettings SceneVisuals => sceneVisuals;
        public IScoreRules ScoreRules => score;

        #endregion
    }
}
