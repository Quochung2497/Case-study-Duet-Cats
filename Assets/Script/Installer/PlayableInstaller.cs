using System.Collections.Generic;
using System.Reflection;
using Control.Game;
using Input;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    [DefaultExecutionOrder(-1100)]
    public sealed class PlayableInstaller : MonoBehaviour, IDependencyProvider
    {
        [System.Serializable]
        private struct SongChoice
        {
            [Tooltip("Synced from SongCatalog. Edit song IDs in the asset.")]
            public string id;
            [Tooltip("Selecting this song clears the other choices. Select one before playing or building.")]
            public bool selected;
        }

        // Keep Inspector references in one place instead of duplicating them in each installer.
        [Header("Playable configuration")]
        [SerializeField] private PlayableSettings settings;

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

        private SongDefinition _selectedSong;
#if UNITY_EDITOR
        [System.NonSerialized] private HashSet<string> _previousSongSelection;
#endif

        public SceneVisualSettings SceneVisuals => settings != null ? settings.SceneVisuals : null;
        public LayoutSceneRefs SceneRefs => new(gameplayCamera, leftCat, rightCat);
        public IViewport Viewport => viewportController;

        // Injector scans [Provide] methods and registers each object by its return type.
        [Provide] public IScoreRules ProvideScoreRules() => settings.ScoreRules;
        [Provide] public CatAnimationSet ProvideCatAnimationSet() => settings.CatAnimations;
        [Provide] public CtaAnimationSet ProvideCtaAnimationSet() => settings.CtaAnimations;
        [Provide] public INoteSettings ProvideNoteSettings() => settings.Notes;
        // Awake resolves the song before Injector registers this provider.
        [Provide] public SongDefinition ProvideSelectedSong() => _selectedSong;
        [Provide] public HitVfxSettings ProvideHitVfxSettings() => settings.HitVfx;
        [Provide] public HitTextSettings ProvideHitTextSettings() => settings.HitText;
        [Provide] public IGameFlowSettings ProvideGameFlowSettings() => settings.GameFlow;
        [Provide] public SceneVisualSettings ProvideSceneVisualSettings() => settings.SceneVisuals;
        [Provide] public LayoutSceneRefs ProvideSceneRefs() => SceneRefs;
        [Provide] public IInputReader ProvideInputReader() => settings.InputReader;
        [Provide] public NoteBehaviour ProvideNotePrefab() => notePrefab;
        [Provide] public IResponsiveLayout ProvideLayout() => responsiveLayout;
        [Provide] public IViewport ProvideViewport() => viewportController;
        [Provide] public INoteManager ProvideNoteManager() => noteManager;
        [Provide] public Transform ProvideNoteParent() => noteManager.transform;
        [Provide] public IScoreManager ProvideScoreManager() => scoreManager;
        [Provide] public IGameState ProvideGameState() => gameObject.GetOrAdd<GameManager>();
        [Provide] public IGameHost ProvideGameHost() => gameObject.GetOrAdd<GameManager>();

        private void Awake()
        {
            if (TryValidateConfiguration(out _selectedSong, out var error)) return;

            Debug.LogError(error, this);
            StopInvalidScene();
        }

        private void StopInvalidScene()
        {
            // Stop scene callbacks immediately; the Editor exits Play Mode after this frame.
            foreach (var root in gameObject.scene.GetRootGameObjects())
                root.SetActive(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            RefreshEditorConfiguration();
        }

        [ContextMenu("Refresh Song Choices")]
        private void RefreshSongChoices()
        {
            if (Application.isPlaying) return;
            UnityEditor.Undo.RecordObject(this, "Refresh Song Choices");
            RefreshEditorConfiguration();
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }

        private void RefreshEditorConfiguration()
        {
            SyncSongChoices();
            UpdateSongSelection();
            if (!TryValidateConfiguration(out _, out var error)) Debug.LogError(error, this);
        }

        private void SyncSongChoices()
        {
            var choices = new List<SongChoice>();
            if (settings != null && settings.SongCatalog != null)
                foreach (var song in settings.SongCatalog.Songs)
                {
                    // Match by ID so reordering or adding catalog entries preserves the scene choice.
                    var previous = songChoices.Find(choice => choice.id == song.Id);
                    choices.Add(new SongChoice { id = song.Id, selected = previous.selected });
                }

            songChoices = choices;
        }

        private void UpdateSongSelection()
        {
            var selectedIds = new HashSet<string>();
            var newSelectionIndex = -1;
            var newSelectionCount = 0;
            for (var i = 0; i < songChoices.Count; i++)
            {
                var choice = songChoices[i];
                if (!choice.selected) continue;
                selectedIds.Add(choice.id);
                if (_previousSongSelection == null || _previousSongSelection.Contains(choice.id)) continue;

                newSelectionIndex = i;
                newSelectionCount++;
            }

            // One newly checked ID is the user's choice; leave ambiguous bulk edits for validation.
            if (newSelectionCount == 1)
            {
                for (var i = 0; i < songChoices.Count; i++)
                {
                    var choice = songChoices[i];
                    choice.selected = i == newSelectionIndex;
                    songChoices[i] = choice;
                }
                selectedIds.Clear();
                selectedIds.Add(songChoices[newSelectionIndex].id);
            }

            // Seed from the loaded state on the first call; IDs stay stable when the list is reordered.
            _previousSongSelection = selectedIds;
        }
#endif

        private bool TryValidateConfiguration(out SongDefinition song, out string error)
        {
            song = default;
            var selected = songChoices.FindAll(choice => choice.selected);
            if (selected.Count != 1)
            {
                error = $"PlayableInstaller needs exactly one selected song; found {selected.Count}. Tick Selected on one song choice.";
                return false;
            }

            var missing = GetMissingReferences(this);
            if (settings != null) missing.AddRange(GetMissingReferences(settings));
            if (missing.Count > 0)
            {
                error = $"Missing required references: {string.Join(", ", missing)}.";
                return false;
            }

            try
            {
                song = settings.SongCatalog.Get(selected[0].id);
            }
            catch (System.InvalidOperationException exception)
            {
                error = exception.Message;
                return false;
            }

            if (song.Audio == null || song.Chart == null)
            {
                error = $"Selected song '{song.Id}' needs both audio and a JSON chart in SongCatalog.";
                return false;
            }

            error = null;
            return true;
        }

        private static List<string> GetMissingReferences(Object source)
        {
            // Treat every serialized Unity object field as a required Inspector reference.
            var missing = new List<string>();
            var type = source.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!field.IsDefined(typeof(SerializeField), false) ||
                    !typeof(Object).IsAssignableFrom(field.FieldType))
                    continue;

                if ((Object)field.GetValue(source) == null)
                    missing.Add($"{type.Name}.{field.Name}");
            }

            return missing;
        }
    }
}
