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
        [SerializeField] private CatBehaviour leftCat;
        [SerializeField] private CatBehaviour rightCat;

        [Inject] private IInputReader _input;
        [Inject] private ResponsiveLayout _layout;

        public void AwakeInitialize()
        {
            if (leftCat == null || rightCat == null)
            {
                Debug.LogError("Assign both cats on CatInstaller.", this);
                return;
            }

            _layout.Refresh();
            Build(leftCat, left: true);
            Build(rightCat, left: false);
        }

        public void StartInitialize()
        {
            // TODO: GameManager will signal Ready -> Playing when the song starts.
            leftCat?.StartPlaying();
            rightCat?.StartPlaying();
        }

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
            var fsm = new StateBuilder<CatState>()
                .Add(new CatIdleState(cat))
                .Add(new CatPlayingState(cat))
                .Add(hitState)
                .Build(CatState.Idle);
            cat.Initialize(action, fsm, hitState, left, _input, _layout);
        }
    }
}
