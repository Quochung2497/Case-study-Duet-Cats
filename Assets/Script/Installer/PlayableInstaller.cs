using System.Collections.Generic;
using System.Reflection;
using Input;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class PlayableInstaller : MonoBehaviour, IDependencyProvider
    {
        // Keep Inspector references in one place instead of duplicating them in each installer.
        [Header("Scriptable Objects")]
        [SerializeField] private PlayableSettings settings;
        [SerializeField] private InputReader inputReader;

        [Header("Prefabs")]
        [SerializeField] private NoteBehaviour notePrefab;

        [Header("Scene References")]
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform leftCat;
        [SerializeField] private Transform rightCat;
        [SerializeField] private ResponsiveLayout responsiveLayout;
        [SerializeField] private NoteManager noteManager;

        public PlayableSettings Settings => settings;
        public LayoutSceneRefs SceneRefs => new(gameplayCamera, leftCat, rightCat);

        // Injector scans [Provide] methods and registers each object by its return type.
        [Provide] public PlayableSettings ProvideSettings() => settings;
        [Provide] public IScoreRules ProvideScoreRules() => settings.ScoreRules;
        [Provide] public INoteSettings ProvideNoteSettings() => settings.Notes;
        [Provide] public HitVfxSettings ProvideHitVfxSettings() => settings.HitVfx;
        [Provide] public HitTextSettings ProvideHitTextSettings() => settings.HitText;
        [Provide] public LayoutSceneRefs ProvideSceneRefs() => SceneRefs;
        [Provide] public IInputReader ProvideInputReader() => inputReader;
        [Provide] public NoteBehaviour ProvideNotePrefab() => notePrefab;
        [Provide] public ResponsiveLayout ProvideLayout() => responsiveLayout;
        [Provide] public NoteManager ProvideNoteManager() => noteManager;
        [Provide] public GameManager ProvideGameManager() => gameObject.GetOrAdd<GameManager>();

        private void Awake() => ValidateReferences();

#if UNITY_EDITOR
        private void OnValidate() => ValidateReferences();
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
