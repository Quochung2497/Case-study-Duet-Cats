using Input;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class PlayableInstaller : MonoBehaviour, IInstaller, IDependencyProvider
    {
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

        [Provide] public PlayableSettings ProvideSettings() => settings;
        [Provide] public LayoutSceneRefs ProvideSceneRefs() => SceneRefs;
        [Provide] public IInputReader ProvideInputReader() => inputReader;
        [Provide] public NoteBehaviour ProvideNotePrefab() => notePrefab;
        [Provide] public ResponsiveLayout ProvideLayout() => responsiveLayout;
        [Provide] public NoteManager ProvideNoteManager() => noteManager;
        [Provide] public GameManager ProvideGameManager() => gameObject.GetOrAdd<GameManager>();

        public void AwakeInitialize()
        {
            if (settings == null || inputReader == null || notePrefab == null ||
                gameplayCamera == null ||
                leftCat == null || rightCat == null || responsiveLayout == null ||
                noteManager == null)
            {
                Debug.LogError("PlayableInstaller has missing references.", this);
                return;
            }

            inputReader.Initialize();
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();
        private void OnDestroy() => ShutdownInput();

        private void ShutdownInput()
        {
            if (inputReader != null)
                inputReader.Shutdown();
        }
    }
}