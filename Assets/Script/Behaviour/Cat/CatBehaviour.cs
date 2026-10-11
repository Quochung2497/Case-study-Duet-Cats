using System;
using Control;
using Control.Cat;
using Control.Note;
using Input;
using UnityEngine;

namespace Game
{
    public class CatBehaviour : MonoBehaviour, ICatBehaviour
    {
        // This behaviour bridges Unity input and transforms with plain-C# action and FSM logic.
        #region Fields and events

        private IInputReader _input;
        private ResponsiveLayout _layout;
        private ICatAction _action;
        private IStateMachine<CatState> _fsm;
        private CatHitState _hitState;
        private CatResultState _resultState;
        private bool _left;
        private bool _playing;
        private bool _subscribed;

        public bool IsLeft => _left;
        public event Action<NoteVisualType> NoteHit;

        #endregion

        #region Setup and gameplay

        public void Initialize(ICatAction action, IStateMachine<CatState> fsm, CatHitState hitState,
            CatResultState resultState, bool left, IInputReader input, ResponsiveLayout layout)
        {
            // CatInstaller builds these objects; the cat stores them and subscribes to input.
            _action = action;
            _fsm = fsm;
            _hitState = hitState;
            _resultState = resultState;
            _left = left;
            _input = input;
            _layout = layout;
            if (isActiveAndEnabled) Subscribe();
        }

        public void OnNoteHit(NoteVisualType type)
        {
            if (_fsm == null || _fsm.CurrentStateKey == CatState.Result) return;

            // Set the hit type before entering Hit so the state selects the correct clip.
            _hitState.OnHit(type);
            if (_fsm.CurrentStateKey != CatState.Hit)
                _fsm.TransitionToState(CatState.Hit);

            // VFX and hit text listen to this event; CatBehaviour does not reference them.
            NoteHit?.Invoke(type);
        }

        public void StartPlaying()
        {
            // Stay Idle while dragging; return to Listening/Playing after release.
            _playing = true;
            if (_fsm != null && !_action.IsDragging && _fsm.CurrentStateKey == CatState.Idle)
                _fsm.TransitionToState(CatState.Playing);
        }

        public void StopPlaying()
        {
            _playing = false;
            StopDragInput();
        }

        public void ShowResult(bool won)
        {
            // Stop drag input before entering the win or loss animation.
            StopPlaying();
            if (_fsm == null) return;

            _resultState.SetOutcome(won);
            _fsm.TransitionToState(CatState.Result);
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        #endregion

        #region Lifecycle and state

        private void OnEnable() => Subscribe();

        private void OnDisable() => StopDragInput();

        private void StopDragInput()
        {
            Unsubscribe();
            CancelDrag();
        }

        private void Update() => UpdateCatState();

        private void UpdateCatState()
        {
            if (_fsm == null) return;

            _fsm.Tick(Time.deltaTime);
            // After the hit clip, CatBehaviour selects Listening or Idle from the current drag state.
            if (_fsm.CurrentStateKey == CatState.Hit && _hitState.IsComplete)
                _fsm.TransitionToState(_playing && !_action.IsDragging ? CatState.Playing : CatState.Idle);
        }

        #endregion

        #region Input

        private void Subscribe()
        {
            // _subscribed prevents duplicate event subscriptions from Initialize and OnEnable.
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

        private void CancelDrag()
        {
            // A layout change invalidates the old drag coordinates, so cancel the drag.
            if (_action == null || !_action.IsDragging) return;
            _action.Cancel();
            ResumeListening();
        }

        private void ResumeListening()
        {
            if (_playing && _fsm != null && _fsm.CurrentStateKey == CatState.Idle)
                _fsm.TransitionToState(CatState.Playing);
        }

        private void OnPointerDown(PointerSample sam)
        {
            // Convert screen pixels to world X before CatAction checks the left or right half.
            var data = _layout.Current;
            var centerX = (data.PlayLeft + data.PlayRight) * 0.5f;
            var pointerX = _layout.ScreenToWorldX(sam.ScreenX, sam.ScreenY);
            if (_action.TryBegin(sam.PointerId, pointerX, centerX) &&
                _playing && _fsm != null && _fsm.CurrentStateKey == CatState.Playing)
                _fsm.TransitionToState(CatState.Idle);
        }

        private void OnPointerMoved(PointerSample sam)
        {
            // Use this cat's bounds; the action returns a new X only for its pointer ID.
            var data = _layout.Current;
            var minX = _left ? data.LeftMinX : data.RightMinX;
            var maxX = _left ? data.LeftMaxX : data.RightMaxX;
            var pointerX = _layout.ScreenToWorldX(sam.ScreenX, sam.ScreenY);
            if (!_action.TryMove(sam.PointerId, pointerX, transform.position.x,
                    minX, maxX, out var nextX)) return;

            var pos = transform.position;
            transform.position = new Vector3(nextX, pos.y, pos.z);
        }

        private void OnPointerUp(PointerSample sam)
        {
            if (_action.TryEnd(sam.PointerId)) ResumeListening();
        }

        #endregion
    }
}
