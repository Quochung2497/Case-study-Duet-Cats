using System;
using Control;
using Control.Cat;
using Control.Note;
using Input;
using Spine.Unity;
using UnityEngine;
using Utility;

namespace Game
{
    public class CatBehaviour : MonoBehaviour, ICatAnimation
    {
        // This behaviour bridges Unity input and transforms with plain-C# action and FSM logic.
        #region Fields and events

        private static readonly CatClip[] IdleClips =
        {
            CatClip.IdleStart,
            CatClip.IdlePlaying,
            CatClip.IdleHungry,
            CatClip.IdleLiemchan,
            CatClip.IdleYawn
        };

        private IInputReader _input;
        private ResponsiveLayout _layout;
        private ICatAction _action;
        private IStateMachine<CatState> _fsm;
        private CatHitState _hitState;
        private CatResultState _resultState;
        private SkeletonAnimation _spine;
        private int _animVersion;
        private int _lastIdle = -1;
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

        public void SetupAnimation(SkeletonAnimation spine) => _spine = spine;

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

        #endregion

        #region Animation

        public void PlayIdle(Action onComplete)
        {
            // Pick one of five idle clips at random without repeating the previous one.
            var index = UnityEngine.Random.Range(0, IdleClips.Length);
            if (index == _lastIdle) index = (index + 1) % IdleClips.Length;
            _lastIdle = index;
            Play(IdleClips[index], false, onComplete);
        }

        public void PlayPlaying() => Play(CatClip.Listening, true);

        public void PlayResult(bool won) =>
            Play(won ? CatClip.Victory : CatClip.MissObjectLose2, true);

        public void PlayHit(NoteVisualType type, Action onComplete)
        {
            // Normal uses either single-object clip; other visual types currently use EatShot.
            var clip = type == NoteVisualType.Normal
                ? (UnityEngine.Random.value < 0.5f ? CatClip.EatingSingle1 : CatClip.EatingSingle2)
                : CatClip.EatShot;
            Play(clip, false, onComplete);
        }

        private void Play(CatClip clip, bool loop, Action onComplete = null)
        {
            // The version ignores completion callbacks from animations replaced mid-play.
            var version = ++_animVersion;
            var entry = AnimationPlayer.Play(_spine, clip, loop);
            if (onComplete != null)
                entry.Complete += _ =>
                {
                    if (version == _animVersion) onComplete();
                };
        }

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
