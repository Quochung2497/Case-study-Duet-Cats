using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Duet Cats/Input Reader")]
    public sealed class InputReader : ScriptableObject, IInputReader, PlayerControls.IGameplayActions
    {
        private const int MouseId = 0;
        private const double MouseTouchDelay = 0.2;

        private readonly Dictionary<TouchControl, PointerSample> _touches = new();
        private PlayerControls _controls;
        private int _nextPointerId = 1;
        private bool _mouseDown;
        private double _mouseBlockedUntil = double.NegativeInfinity;

        public event Action<PointerSample> PointerDown;
        public event Action<PointerSample> PointerMoved;
        public event Action<PointerSample> PointerUp;

        public void Initialize()
        {
            if (_controls != null)
                return;

            _touches.Clear();
            _nextPointerId = 1;
            _mouseDown = false;
            _mouseBlockedUntil = double.NegativeInfinity;

            _controls = new PlayerControls();
            _controls.Gameplay.SetCallbacks(this);
            _controls.Gameplay.Enable();
        }

        public void Shutdown()
        {
            if (_controls == null)
                return;

            _controls.Gameplay.Disable();
            _controls.Gameplay.RemoveCallbacks(this);
            _controls.Dispose();
            _controls = null;

            _touches.Clear();
            _mouseDown = false;
        }

        // Pass Through actions perform on both press and release; canceled is not the release signal.
        public void OnPointerPress(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            if (ctx.control.parent is TouchControl touch)
            {
                _mouseBlockedUntil = ctx.time + MouseTouchDelay;
                HandleTouchPress(touch, ctx.ReadValueAsButton());
                return;
            }

            if (ctx.control.device is Mouse mouse &&
                _touches.Count == 0 &&
                ctx.time >= _mouseBlockedUntil)
            {
                HandleMousePress(mouse, ctx.ReadValueAsButton());
            }
        }

        public void OnPointerPosition(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            if (ctx.control.parent is TouchControl touch)
            {
                _mouseBlockedUntil = ctx.time + MouseTouchDelay;
                if (!_touches.TryGetValue(touch, out var prev))
                    return;

                var pos = ctx.ReadValue<Vector2>();
                var sam = new PointerSample(prev.PointerId, pos.x, pos.y);
                _touches[touch] = sam;
                PointerMoved?.Invoke(sam);
                return;
            }

            if (_mouseDown && ctx.control.device is Mouse &&
                _touches.Count == 0 &&
                ctx.time >= _mouseBlockedUntil)
            {
                var pos = ctx.ReadValue<Vector2>();
                PointerMoved?.Invoke(new PointerSample(MouseId, pos.x, pos.y));
            }
        }

        private void HandleTouchPress(TouchControl touch, bool down)
        {
            if (down)
            {
                if (_touches.ContainsKey(touch))
                    return;

                // A real touch takes precedence over an emulated mouse pointer.
                if (_mouseDown)
                {
                    _mouseDown = false;
                    var mousePos = Mouse.current != null
                        ? Mouse.current.position.ReadValue()
                        : Vector2.zero;
                    PointerUp?.Invoke(new PointerSample(MouseId, mousePos.x, mousePos.y));
                }

                var pos = touch.position.ReadValue();
                var sam = new PointerSample(_nextPointerId++, pos.x, pos.y);
                _touches.Add(touch, sam);
                PointerDown?.Invoke(sam);
            }
            else if (_touches.TryGetValue(touch, out var sam))
            {
                _touches.Remove(touch);
                PointerUp?.Invoke(sam);
            }
        }

        private void HandleMousePress(Mouse mouse, bool down)
        {
            if (_mouseDown == down)
                return;

            _mouseDown = down;
            var pos = mouse.position.ReadValue();
            var sam = new PointerSample(MouseId, pos.x, pos.y);
            if (down)
                PointerDown?.Invoke(sam);
            else
                PointerUp?.Invoke(sam);
        }
    }
}
