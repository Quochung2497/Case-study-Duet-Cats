using System.Collections.Generic;
using System.Reflection;
using Control.Game;
using Input;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class PlayableInstaller : MonoBehaviour, IDependencyProvider
    {
        [System.Serializable]
        private struct SongChoice
        {
            [Tooltip("Synced from SongCatalog. Edit song IDs in the asset.")]
            public string id;
            [Tooltip("Select exactly one song choice before playing or building.")]
            public bool selected;
        }

        // Keep Inspector references in one place instead of duplicating them in each installer.
        [Header("Scriptable Objects")]
        [SerializeField] private PlayableSettings settings;
        [SerializeField] private CatAnimationSet catAnimations;
        [SerializeField] private CtaAnimationSet ctaAnimations;
        [SerializeField] private InputReader inputReader;

        [Header("Song configuration")]
        [SerializeField] private SongCatalog songCatalog;
        [Header("Song choices - select exactly one")]
        [SerializeField] private List<SongChoice> songChoices = new();

        [Header("Prefabs")]
        [SerializeField] private NoteBehaviour notePrefab;

        [Header("Scene References")]
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform leftCat;
        [SerializeField] private Transform rightCat;
        [SerializeField] private ResponsiveLayout responsiveLayout;
        [SerializeField] private ViewportController viewportController;
        [SerializeField] private NoteManager noteManager;
        [SerializeField] private ScoreManager scoreManager;

        public SceneVisualSettings SceneVisuals => settings != null ? settings.SceneVisuals : null;
        public LayoutSceneRefs SceneRefs => new(gameplayCamera, leftCat, rightCat);
        public IViewport Viewport => viewportController;

        // Injector scans [Provide] methods and registers each object by its return type.
        [Provide] public IScoreRules ProvideScoreRules() => settings.ScoreRules;
        [Provide] public CatAnimationSet ProvideCatAnimationSet() => catAnimations;
        [Provide] public CtaAnimationSet ProvideCtaAnimationSet() => ctaAnimations;
        [Provide] public INoteSettings ProvideNoteSettings() => settings.Notes;
        [Provide]
        public SongDefinition ProvideSelectedSong()
        {
            // Scene choices select one entry without storing its audio/chart in the scene.
            var selected = songChoices.FindAll(choice => choice.selected);
            if (selected.Count != 1)
                throw new System.InvalidOperationException("Tick Selected on exactly one song choice in PlayableInstaller.");

            return songCatalog.Get(selected[0].id);
        }
        [Provide] public HitVfxSettings ProvideHitVfxSettings() => settings.HitVfx;
        [Provide] public HitTextSettings ProvideHitTextSettings() => settings.HitText;
        [Provide] public IGameFlowSettings ProvideGameFlowSettings() => settings.GameFlow;
        [Provide] public SceneVisualSettings ProvideSceneVisualSettings() => settings.SceneVisuals;
        [Provide] public LayoutSceneRefs ProvideSceneRefs() => SceneRefs;
        [Provide] public IInputReader ProvideInputReader() => inputReader;
        [Provide] public NoteBehaviour ProvideNotePrefab() => notePrefab;
        [Provide] public IResponsiveLayout ProvideLayout() => responsiveLayout;
        [Provide] public IViewport ProvideViewport() => viewportController;
        [Provide] public INoteManager ProvideNoteManager() => noteManager;
        [Provide] public Transform ProvideNoteParent() => noteManager.transform;
        [Provide] public IScoreManager ProvideScoreManager() => scoreManager;
        [Provide] public IGameState ProvideGameState() => gameObject.GetOrAdd<GameManager>();
        [Provide] public IGameHost ProvideGameHost() => gameObject.GetOrAdd<GameManager>();

        private void Awake() => ValidateReferences();

#if UNITY_EDITOR
        private void OnValidate()
        {
            SyncSongChoices();
            ValidateReferences();
        }

        [ContextMenu("Refresh Song Choices")]
        private void RefreshSongChoices()
        {
            UnityEditor.Undo.RecordObject(this, "Refresh Song Choices");
            SyncSongChoices();
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }

        private void SyncSongChoices()
        {
            var choices = new List<SongChoice>();
            if (songCatalog != null)
                foreach (var song in songCatalog.Songs)
                {
                    // Match by ID so reordering or adding catalog entries preserves the scene choice.
                    var previous = songChoices.Find(choice => choice.id == song.Id);
                    choices.Add(new SongChoice { id = song.Id, selected = previous.selected });
                }

            songChoices = choices;
        }
#endif

        private void ValidateReferences()
        {
            // Treat every serialized Unity object field as a required Inspector reference.
            var missing = new List<string>();
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!field.IsDefined(typeof(SerializeField), false) ||
                    !typeof(Object).IsAssignableFrom(field.FieldType))
                    continue;

                if ((Object)field.GetValue(this) == null)
                    missing.Add(field.Name);
            }

            if (missing.Count > 0)
                Debug.LogError($"PlayableInstaller is missing: {string.Join(", ", missing)}.", this);
        }
    }
}
