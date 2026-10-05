using Control;
using Control.Cat;
using Input;
using Spine.Unity;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class CatInstaller : MonoBehaviour, IInstaller
    {
        [Inject] private IInputReader _input;
        [Inject] private ResponsiveLayout _layout;
        [Inject] private LayoutSceneRefs _sceneRefs;

        public void AwakeInitialize()
        {
            if (_sceneRefs == null || _sceneRefs.LeftCat == null ||
                _sceneRefs.RightCat == null ||
                !_sceneRefs.LeftCat.TryGetComponent(out CatBehaviour leftCat) ||
                !_sceneRefs.RightCat.TryGetComponent(out CatBehaviour rightCat))
            {
                Debug.LogError("PlayableInstaller needs both cat references.", this);
                return;
            }

            _layout.Refresh();
            Build(leftCat, left: true);
            Build(rightCat, left: false);
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();

        private void Build(CatBehaviour cat, bool left)
        {
            if (!cat.TryGetComponent<Collider2D>(out _))
                Debug.LogError($"{cat.name} needs a Collider2D for note hits.", cat);

            if (!cat.TryGetComponent<SkeletonAnimation>(out var spine))
            {
                Debug.LogError($"{cat.name} needs SkeletonAnimation.", cat);
                return;
            }

            cat.SetupAnimation(spine);
            ICatAction action = new CatAction(left);
            var hitState = new CatHitState(cat);
            var resultState = new CatResultState(cat);
            var fsm = new StateBuilder<CatState>()
                .Add(new CatIdleState(cat))
                .Add(new CatPlayingState(cat))
                .Add(hitState)
                .Add(resultState)
                .Build(CatState.Idle);
            cat.Initialize(action, fsm, hitState, resultState, left, _input, _layout);
        }
    }
}
