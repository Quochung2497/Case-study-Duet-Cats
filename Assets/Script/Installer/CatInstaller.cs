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
        [Inject] private CatAnimationSet _animations;

        public void AwakeInitialize()
        {
            // Build the cat FSMs only after both cat objects have the required components.
            if (_animations == null || _sceneRefs == null || _sceneRefs.LeftCat == null ||
                _sceneRefs.RightCat == null ||
                !_sceneRefs.LeftCat.TryGetComponent<CatBehaviour>(out var leftCat) ||
                !_sceneRefs.RightCat.TryGetComponent<CatBehaviour>(out var rightCat))
            {
                Debug.LogError("CatInstaller needs animation settings and both cat references.", this);
                return;
            }

            _layout.RecalculateLayout();
            Build(leftCat, left: true);
            Build(rightCat, left: false);
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();

        private void Build(CatBehaviour cat, bool left)
        {
            // The collider receives note triggers; SkeletonAnimation plays Spine clips.
            if (!cat.TryGetComponent<Collider2D>(out _))
                Debug.LogError($"{cat.name} needs a Collider2D for note hits.", cat);

            if (!cat.TryGetComponent<SkeletonAnimation>(out var spine))
            {
                Debug.LogError($"{cat.name} needs SkeletonAnimation.", cat);
                return;
            }

            var skeletonData = spine.SkeletonDataAsset != null
                ? spine.SkeletonDataAsset.GetSkeletonData(false)
                : null;
            if (!_animations.TryValidate(skeletonData, out var error))
            {
                Debug.LogError($"{cat.name} has invalid animation settings: {error}", cat);
                return;
            }

            // Give each cat its own action, states, and FSM so their drag and hit flows stay separate.
            var action = new CatAction(left);
            var animation = new CatAnimationController(spine, _animations);
            var hitState = new CatHitState(animation);
            var resultState = new CatResultState(animation);
            var fsm = new StateBuilder<CatState>()
                .Add(new CatIdleState(animation))
                .Add(new CatPlayingState(animation))
                .Add(hitState)
                .Add(resultState)
                .Build(CatState.Idle);
            cat.ConfigureStateMachine(fsm, hitState, resultState);
            cat.Initialize(action, left, _input, _layout);
        }
    }
}
