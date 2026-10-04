using Control;
using Control.Cat;
using Input;
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

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();

        private void Build(CatBehaviour cat, bool left)
        {
            ICatAction action = new CatAction(left);
            var fsm = new StateBuilder<CatState>()
                .Add(new CatIdleState())
                .Add(new CatTrackingState(action))
                .Build(CatState.Idle);
            cat.Initialize(action, fsm, left, _input, _layout);
        }
    }
}
