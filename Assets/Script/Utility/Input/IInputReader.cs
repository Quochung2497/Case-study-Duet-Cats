using System;

namespace Input
{
    /// <summary>One pointer position in screen pixels. Mouse uses id 0; touches use positive ids.</summary>
    // The ID lets each cat follow the same finger that began its drag.
    public readonly struct PointerSample
    {
        public int PointerId { get; }
        public float ScreenX { get; }
        public float ScreenY { get; }

        public PointerSample(int pointerId, float screenX, float screenY)
        {
            PointerId = pointerId;
            ScreenX = screenX;
            ScreenY = screenY;
        }
    }

    /// <summary>Input events consumed by scene presenters without depending on Unity's Input System.</summary>
    // Consumers receive pointer events without knowing Input System callbacks.
    public interface IInputReader
    {
        event Action<PointerSample> PointerDown;
        event Action<PointerSample> PointerMoved;
        event Action<PointerSample> PointerUp;

        void Initialize();
        void Shutdown();
    }
}
