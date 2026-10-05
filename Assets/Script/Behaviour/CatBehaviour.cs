using Control;
using Control.Cat;
using Input;
using UnityEngine;

namespace Game
{
    public class CatBehaviour : MonoBehaviour
    {
        private IInputReader _input;
        private ResponsiveLayout _layout;
        private ICatAction _action;
        private IStateMachine<CatState> _fsm;
        private bool _left;
        private bool _subscribed;

        public bool IsLeft => _left;

        public void Initialize(ICatAction action, IStateMachine<CatState> fsm, bool left,
            IInputReader input, ResponsiveLayout layout)
        {
            _action = action;
            _fsm = fsm;
            _left = left;
            _input = input;
            _layout = layout;
            if (isActiveAndEnabled) Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => StopDragInput();

        private void StopDragInput()
        {
            Unsubscribe();
            CancelDrag();
        }

        private void Update() => _fsm?.Tick(Time.deltaTime);

        private void Subscribe()
        {
            if (_subscribed || _input == null) return;
            _input.PointerDown += OnPointerDown;
            _input.PointerMoved += OnPointerMoved;
            _input.PointerUp += OnPointerUp;
            _layout.Changed += CancelDrag;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            _input.PointerDown -= OnPointerDown;
            _input.PointerMoved -= OnPointerMoved;
            _input.PointerUp -= OnPointerUp;
            _layout.Changed -= CancelDrag;
            _subscribed = false;
        }

        private void CancelDrag() => _action?.Cancel();

        private void OnPointerDown(PointerSample sam)
        {
            var data = _layout.Current;
            var centerX = (data.PlayLeft + data.PlayRight) * 0.5f;
            var pointerX = _layout.ScreenToWorldX(sam.ScreenX, sam.ScreenY);
            if (_action.TryBegin(sam.PointerId, pointerX, centerX) &&
                _fsm.CurrentStateKey == CatState.Idle)
                _fsm.TransitionToState(CatState.Tracking);
        }

        private void OnPointerMoved(PointerSample sam)
        {
            var data = _layout.Current;
            var minX = _left ? data.LeftMinX : data.RightMinX;
            var maxX = _left ? data.LeftMaxX : data.RightMaxX;
            var pointerX = _layout.ScreenToWorldX(sam.ScreenX, sam.ScreenY);
            if (!_action.TryMove(sam.PointerId, pointerX, transform.position.x,
                    minX, maxX, out var nextX)) return;

            var pos = transform.position;
            transform.position = new Vector3(nextX, pos.y, pos.z);
        }

        private void OnPointerUp(PointerSample sam) => _action.TryEnd(sam.PointerId);
    }
}
